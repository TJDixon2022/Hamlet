using System.Text.RegularExpressions;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Ldpc;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// The sanctioned read of the pinned clone for unit 216: how upstream turns a candidate's place in
/// the waterfall into 174 log-likelihood ratios, what it does to those ratios before the decoder
/// sees them, how many attempts it makes per candidate, what it de-duplicates on — and, the part
/// criterion 3 stands or falls on, <b>what reference recordings and expected decode lists the clone
/// actually holds.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>Read it before porting it, and leave behind something that fails loudly if a re-pin changes
/// it.</b> Every shape asserted here is a shape <c>src/Ft8Sharp/Dsp/Ft8SoftSymbols.cs</c> was
/// written against. This file is the record of the read alone; the assertions that bind these
/// shapes to the port's own constants live beside the port, so that this one compiles and answers
/// on its own.
/// </para>
/// <para>
/// <b>Shapes and counts, never values that are upstream's to own.</b> Identifiers, presences, guard
/// conditions and structural facts are asserted and printed; nothing from the clone is committed,
/// and in particular no expected decode line and no audio ever enters this repository.
/// </para>
/// <para>
/// <b>THE INSTRUCTION EXPECTED AN INVERSE GRAY MAP AND UPSTREAM DOES NOT USE ONE.</b> Unit 216's
/// instruction said extraction needs a tone-to-value map derived from <c>Ft8GrayMap</c>. It does
/// not: <c>ft8_extract_symbol</c> indexes its eight magnitudes <em>by symbol value</em> through the
/// <em>forward</em> map, <c>s2[j] = mag[kFT8_Gray_map[j]]</c>, so the array it builds is already in
/// value order and the three bit tests read straight off it. Asserted in
/// <see cref="TheEightMagnitudesAreGatheredInValueOrderThroughTheForwardGrayMap"/>, reported as a
/// mismatch and not repaired.
/// </para>
/// <para><b>Absent is a skip.</b> A fresh clone stays green.</para>
/// </remarks>
public class UpstreamExtractionInventoryTests
{
    private readonly ITestOutputHelper _output;

    public UpstreamExtractionInventoryTests(ITestOutputHelper output) => _output = output;

    /// <summary>The files unit 216 is licensed to read for shapes, and no others.</summary>
    private static readonly string[] ExtractionSources =
    {
        @"ft8\decode.h", @"ft8\decode.c", @"ft8\constants.h", @"ft8\constants.c",
        @"demo\decode_ft8.c",
    };

    /// <summary>Where the reference recordings live in the pin.</summary>
    private const string WavDirectory = @"test\wav";

    /// <summary>
    /// Discovery, and it runs because assuming which file holds extraction is exactly the mistake
    /// this project has paid for before.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheExtractionFilesAreFoundRatherThanAssumed()
    {
        var location = RequireReachableClone();
        _output.WriteLine($"clone: {location}");

        foreach (var relative in ExtractionSources)
        {
            var path = Path.Combine(location, relative);
            Assert.True(File.Exists(path), $"the pin no longer holds {relative}.");
            _output.WriteLine($"  {relative}: {File.ReadAllLines(path).Length} lines");
        }
    }

    // ------------------------------------------------------------------------------------------
    // PART A — the extraction path.
    // ------------------------------------------------------------------------------------------

    /// <summary>
    /// WEAK — a function body. <b>The alignment, and it is the whole of unit 214's carried-forward
    /// item.</b> A candidate's four position fields index the waterfall in exactly the axis order
    /// the store is written in, and the block index is the outermost axis. So extraction reads the
    /// same blocks the search scored, by the same convention, and neither is free to disagree.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheCandidateIndexesTheWaterfallInTheStoresOwnAxisOrder()
    {
        var body = ExtractFunctionBody(ReadSource(@"ft8\decode.c"), "get_cand_mag");
        var collapsed = CollapseWhitespace(body);

        // offset = time_offset; then fold in time_sub, then freq_sub, then freq_offset — which is
        // exactly [block][time_osr][freq_osr][num_bins] with the bin varying fastest.
        Assert.Matches(@"offset\s*=\s*candidate->time_offset\s*;", collapsed);
        Assert.Matches(@"offset\s*=\s*\(offset\s*\*\s*wf->time_osr\)\s*\+\s*candidate->time_sub\s*;", collapsed);
        Assert.Matches(@"offset\s*=\s*\(offset\s*\*\s*wf->freq_osr\)\s*\+\s*candidate->freq_sub\s*;", collapsed);
        Assert.Matches(@"offset\s*=\s*\(offset\s*\*\s*wf->num_bins\)\s*\+\s*candidate->freq_offset\s*;", collapsed);

        _output.WriteLine("  offset = ((((block * time_osr) + time_sub) * freq_osr) + freq_sub) * num_bins + bin");
        _output.WriteLine("  which is Ft8Waterfall.IndexOf, field for field and axis for axis.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  THE SAME HELPER IS USED BY THE SCORER AND BY EXTRACTION - ft8_sync_score");
        _output.WriteLine("  and ft8_extract_likelihood both open with get_cand_mag(wf, cand), so the");
        _output.WriteLine("  two CANNOT disagree by a block or a sub-offset in upstream, and the port");
        _output.WriteLine("  keeps that by reading through the same Ft8Waterfall indexer.");

        var score = ExtractFunctionBody(ReadSource(@"ft8\decode.c"), "ft8_sync_score");
        var extract = ExtractFunctionBody(ReadSource(@"ft8\decode.c"), "ft8_extract_likelihood");
        Assert.Contains("get_cand_mag(wf, candidate)", score, StringComparison.Ordinal);
        Assert.Contains("get_cand_mag(wf, cand)", extract, StringComparison.Ordinal);
    }

    /// <summary>
    /// WEAK. <b>The sync blocks are stepped OVER, not through.</b> The data symbol at index k sits
    /// at channel symbol k + 7 for the first twenty-nine and k + 14 for the rest, which is the
    /// 7/29/7/29/7 layout <c>Ft8SymbolEncoder</c> already carries. Three ratios per symbol, and a
    /// symbol whose block falls outside the waterfall contributes three zeros rather than being
    /// skipped or refused.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheDataSymbolsStepOverTheThreeSyncBlocksAndOutOfRangeBlocksGiveZeroRatios()
    {
        var body = CollapseWhitespace(
            ExtractFunctionBody(ReadSource(@"ft8\decode.c"), "ft8_extract_likelihood"));

        Assert.Matches(@"for \(int k = 0; k < FT8_ND; \+\+k\)", body);
        Assert.Matches(@"int sym_idx = k \+ \(\(k < 29\) \? 7 : 14\);", body);
        Assert.Matches(@"int bit_idx = 3 \* k;", body);
        Assert.Matches(@"int block = cand->time_offset \+ sym_idx;", body);
        Assert.Matches(@"if \(\(block < 0\) \|\| \(block >= wf->num_blocks\)\)", body);
        Assert.Matches(@"log174\[bit_idx \+ 0\] = 0; log174\[bit_idx \+ 1\] = 0; log174\[bit_idx \+ 2\] = 0;", body);
        Assert.Matches(@"ft8_extract_symbol\(mag \+ \(sym_idx \* wf->block_stride\), log174 \+ bit_idx\);", body);

        // The same layout, derived from this library's own encoder rather than from the literals
        // above — which is the point of asserting both.
        var stepped = new List<int>();
        for (var symbol = 0; symbol < Ft8SymbolEncoder.SymbolCount; symbol++)
        {
            if (!Ft8SymbolEncoder.IsSyncSymbol(symbol))
            {
                stepped.Add(symbol);
            }
        }

        Assert.Equal(Ft8SymbolEncoder.DataSymbolCount, stepped.Count);
        for (var k = 0; k < stepped.Count; k++)
        {
            Assert.Equal(k + (k < 29 ? 7 : 14), stepped[k]);
        }

        _output.WriteLine($"  {stepped.Count} data symbols at channel indices "
            + $"{stepped[0]}..{stepped[28]} and {stepped[29]}..{stepped[^1]}");
        _output.WriteLine("  upstream's k + (k<29 ? 7 : 14) and Ft8SymbolEncoder.IsSyncSymbol agree on");
        _output.WriteLine("  all 58, so the port lays the symbols out ONCE and not a second time.");
        _output.WriteLine("  A block outside the waterfall contributes THREE ZERO RATIOS - it is not");
        _output.WriteLine("  skipped and the candidate is not refused. A zero ratio is 'no opinion'.");
    }

    /// <summary>
    /// WEAK, and it is the shape unit 216's instruction got wrong. The eight magnitudes are read as
    /// <b>decibels</b> — <c>WF_ELEM_MAG</c>, not the integer <c>WF_ELEM_MAG_INT</c> the scorer uses —
    /// and they are gathered <b>in value order through the forward Gray map</b>. No inverse map
    /// exists anywhere in upstream's decoder.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheEightMagnitudesAreGatheredInValueOrderThroughTheForwardGrayMap()
    {
        var body = CollapseWhitespace(
            ExtractFunctionBody(ReadSource(@"ft8\decode.c"), "ft8_extract_symbol"));

        // The live branch only. The function carries a #if 1 / #else alternative that is not
        // compiled and a commented-out printf that does use the integer macro, and asserting over
        // the whole body would be asserting about dead text.
        var live = body[..body.IndexOf("#else", StringComparison.Ordinal)];

        Assert.Matches(@"float s2\[8\];", live);
        Assert.Matches(@"for \(int j = 0; j < 8; \+\+j\) \{ s2\[j\] = WF_ELEM_MAG\(wf\[kFT8_Gray_map\[j\]\]\); \}", live);
        Assert.DoesNotContain("WF_ELEM_MAG_INT", live, StringComparison.Ordinal);

        _output.WriteLine("  s2[j] = WF_ELEM_MAG(wf[kFT8_Gray_map[j]])  --  j is the SYMBOL VALUE and");
        _output.WriteLine("  kFT8_Gray_map[j] is the TONE that carries it. The FORWARD map. There is no");
        _output.WriteLine("  inverse map in upstream's decoder and the port does not build one.");
        _output.WriteLine("  MISMATCH AGAINST THE INSTRUCTION, reported and not repaired: it asserted");
        _output.WriteLine("  'extraction needs the inverse map, tone to value'. It does not.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  And the magnitudes are DECIBELS here (WF_ELEM_MAG), where the sync scorer");
        _output.WriteLine("  reads the raw stored byte (WF_ELEM_MAG_INT). Two different reads of the");
        _output.WriteLine("  same store, half a decibel per count apart, and the port keeps both.");

        // The forward map is 8 entries and a permutation, taken from the generated table and never
        // transcribed. Nothing here copies a value out of it.
        var map = Ft8Tables.Ft8GrayMap;
        Assert.Equal(Ft8SymbolEncoder.ToneCount, map.Length);
        var seen = new bool[Ft8SymbolEncoder.ToneCount];
        foreach (var tone in map)
        {
            Assert.InRange(tone, 0, Ft8SymbolEncoder.ToneCount - 1);
            Assert.False(seen[tone], "the Gray map is not a permutation.");
            seen[tone] = true;
        }

        _output.WriteLine($"  Ft8Tables.Ft8GrayMap is {map.Length} entries and a permutation of the tones.");
    }

    /// <summary>
    /// WEAK. The three bit tests: the most significant bit splits the top four values from the
    /// bottom four, and each further bit halves again. <b>Positive means the bit is one</b>, which is
    /// the convention unit 215 settled from three independent readings — the maximum over the values
    /// whose bit is one, minus the maximum over the values whose bit is zero.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheThreeRatiosAreAMaximumOverTheOnesMinusAMaximumOverTheZeros()
    {
        var body = CollapseWhitespace(
            ExtractFunctionBody(ReadSource(@"ft8\decode.c"), "ft8_extract_symbol"));

        // Bit i of the three-bit value, most significant first. For each, the four values whose bit
        // is set and the four whose bit is clear — asserted as the exact expressions upstream writes.
        var expected = new (int Bit, string Ones, string Zeros)[]
        {
            (0, @"s2\[4\], s2\[5\], s2\[6\], s2\[7\]", @"s2\[0\], s2\[1\], s2\[2\], s2\[3\]"),
            (1, @"s2\[2\], s2\[3\], s2\[6\], s2\[7\]", @"s2\[0\], s2\[1\], s2\[4\], s2\[5\]"),
            (2, @"s2\[1\], s2\[3\], s2\[5\], s2\[7\]", @"s2\[0\], s2\[2\], s2\[4\], s2\[6\]"),
        };

        foreach (var (bit, ones, zeros) in expected)
        {
            Assert.Matches($@"logl\[{bit}\] = max4\({ones}\) - max4\({zeros}\);", body);

            // The same partition, derived rather than copied: value j has bit `bit` set exactly when
            // (j >> (2 - bit)) is odd.
            var derivedOnes = Enumerable.Range(0, 8).Where(j => ((j >> (2 - bit)) & 1) == 1).ToArray();
            var derivedZeros = Enumerable.Range(0, 8).Where(j => ((j >> (2 - bit)) & 1) == 0).ToArray();
            Assert.Equal(ones, string.Join(", ", derivedOnes.Select(j => $@"s2\[{j}\]")));
            Assert.Equal(zeros, string.Join(", ", derivedZeros.Select(j => $@"s2\[{j}\]")));

            _output.WriteLine($"  logl[{bit}] = max over values {{{string.Join(",", derivedOnes)}}} "
                + $"minus max over {{{string.Join(",", derivedZeros)}}}");
        }

        Assert.Matches(@"return \(a >= b\) \? a : b;", CollapseWhitespace(
            ExtractFunctionBody(ReadSource(@"ft8\decode.c"), "max2")));
        _output.WriteLine("  max2 breaks its tie toward the FIRST argument, and max4 is max2(max2,max2).");
        _output.WriteLine("  POSITIVE MEANS THE BIT IS ONE. Settled by unit 215 and not re-argued here.");
    }

    /// <summary>
    /// WEAK. <b>The normalisation, which unit 215 read and deliberately did not port.</b> The
    /// population variance of all 174 ratios is computed with the mean removed, and every ratio is
    /// then multiplied by the square root of a fixed target over that variance. The mean itself is
    /// <b>not</b> subtracted from the ratios — it is used only to compute the variance.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheNormalisationScalesAllOneHundredAndSeventyFourToAFixedVarianceAndDoesNotRemoveTheMean()
    {
        var body = CollapseWhitespace(
            ExtractFunctionBody(ReadSource(@"ft8\decode.c"), "ftx_normalize_logl"));

        Assert.Matches(@"for \(int i = 0; i < FTX_LDPC_N; \+\+i\) \{ sum \+= log174\[i\]; sum2 \+= log174\[i\] \* log174\[i\]; \}", body);
        Assert.Matches(@"float inv_n = 1\.0f / FTX_LDPC_N;", body);
        Assert.Matches(@"float variance = \(sum2 - \(sum \* sum \* inv_n\)\) \* inv_n;", body);

        var target = Regex.Match(body, @"float norm_factor = sqrtf\((\d+(?:\.\d+)?)f / variance\);");
        Assert.True(target.Success, "the normalisation is no longer a square root of a fixed target over the variance.");
        Assert.Matches(@"for \(int i = 0; i < FTX_LDPC_N; \+\+i\) \{ log174\[i\] \*= norm_factor; \}", body);

        // The mean is never written back into the array.
        Assert.DoesNotMatch(@"log174\[i\] -=", body);
        Assert.DoesNotMatch(@"log174\[i\] = log174\[i\] -", body);

        _output.WriteLine($"  target variance (upstream's, printed and not asserted as a literal here): {target.Groups[1].Value}");
        _output.WriteLine("  variance = (sum2 - sum*sum/N)/N over ALL 174 - the population variance,");
        _output.WriteLine("  mean removed FROM THE VARIANCE and NOT from the ratios themselves.");
        _output.WriteLine("  factor = sqrt(target / variance), applied to all 174.");
        _output.WriteLine("  Unit 215 recorded upstream's figure as 24; this read confirms it.");
        _output.WriteLine("  The comment beside it calls the target 'experimentally found', so it is a");
        _output.WriteLine("  WEAK anchor: one number chosen by measurement, not derived from anything.");
    }

    /// <summary>
    /// WEAK. The order of the three steps, and that <b>there is exactly one attempt per candidate.</b>
    /// Extract, normalise, decode — and no loop over neighbouring time or frequency offsets anywhere.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void ThereIsOneAttemptPerCandidateAndTheOrderIsExtractNormaliseDecode()
    {
        var body = ExtractFunctionBody(ReadSource(@"ft8\decode.c"), "ftx_decode_candidate");
        var collapsed = CollapseWhitespace(body);

        var extractAt = collapsed.IndexOf("ft8_extract_likelihood(wf, cand, log174);", StringComparison.Ordinal);
        var normaliseAt = collapsed.IndexOf("ftx_normalize_logl(log174);", StringComparison.Ordinal);
        var decodeAt = collapsed.IndexOf("bp_decode(log174, max_iterations, plain174", StringComparison.Ordinal);

        Assert.True(extractAt >= 0, "extraction is no longer called from ftx_decode_candidate.");
        Assert.True(normaliseAt > extractAt, "the normalisation no longer sits between extraction and the decoder.");
        Assert.True(decodeAt > normaliseAt, "the decoder no longer runs after the normalisation.");

        // One call each, and no retry loop: nothing repeats between entering the function and the
        // decoder returning. The only loops in the body are the two that copy the payload out, and
        // both sit after the CRC gate in the success branch.
        Assert.Single(Regex.Matches(collapsed, @"ftx_normalize_logl\("));
        Assert.Single(Regex.Matches(collapsed, @"bp_decode\("));
        Assert.DoesNotMatch(@"\bfor\s*\(", collapsed[..(decodeAt + "bp_decode(".Length)]);
        Assert.DoesNotMatch(@"\bwhile\s*\(", collapsed);

        _output.WriteLine("  ftx_decode_candidate: extract -> normalise -> bp_decode -> parity -> CRC.");
        _output.WriteLine("  ONE attempt per candidate. No sweep over neighbouring offsets, no second");
        _output.WriteLine("  hypothesis, no retry. The only loops in the function body are the two that");
        _output.WriteLine("  copy the payload out, and they are inside the success branch.");

        // And the demo calls it once per candidate, in the order the search returned them.
        var demo = CollapseWhitespace(ExtractFunctionBody(ReadSource(@"demo\decode_ft8.c"), "decode"));
        Assert.Matches(@"for \(int idx = 0; idx < num_candidates; \+\+idx\)", demo);
        Assert.Single(Regex.Matches(demo, @"ftx_decode_candidate\("));
        _output.WriteLine("  and the application loops the candidate list ONCE, in rank order.");
    }

    /// <summary>
    /// <b>Dead code, named as dead rather than ported.</b> <c>ft8_decode_multi_symbols</c> is
    /// declared and defined and never called — it is the multi-symbol hypothesis upstream left in the
    /// file. Nothing in this library ports it, and this test is what makes that a decision rather
    /// than an oversight.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheMultiSymbolExtractionIsPresentAndNeverCalled()
    {
        var source = ReadSource(@"ft8\decode.c");
        var mentions = Regex.Matches(source, @"ft8_decode_multi_symbols");

        // A forward declaration and a definition, and nothing else.
        Assert.Equal(2, mentions.Count);
        _output.WriteLine($"  ft8_decode_multi_symbols appears {mentions.Count} times in ft8/decode.c:");
        _output.WriteLine("  once as a forward declaration and once as its definition. NEVER CALLED.");
        _output.WriteLine("  Read and deliberately not ported, like ldpc_decode was by unit 215.");
    }

    /// <summary>
    /// WEAK, and it is the rule the path's de-duplicator is built to. Two decodes are the same
    /// message when <b>the whole packed payload matches</b>; the CRC is used only to pick the hash
    /// bucket. Nothing is keyed on the text, on the frequency or on the candidate.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void DuplicatesAreDecidedOnTheWholePackedPayloadAndNotOnTheText()
    {
        var demo = CollapseWhitespace(ExtractFunctionBody(ReadSource(@"demo\decode_ft8.c"), "decode"));

        Assert.Matches(@"int idx_hash = message\.hash % kMax_decoded_messages;", demo);
        Assert.Matches(
            @"\(decoded_hashtable\[idx_hash\]->hash == message\.hash\) && \(0 == memcmp\(decoded_hashtable\[idx_hash\]->payload, message\.payload, sizeof\(message\.payload\)\)\)",
            demo);
        Assert.Matches(@"idx_hash = \(idx_hash \+ 1\) % kMax_decoded_messages;", demo);

        // And the message is unpacked to text only AFTER it has been found to be new, so the text
        // cannot be what the comparison is on.
        var emptySlotAt = demo.IndexOf("if (found_empty_slot)", StringComparison.Ordinal);
        var unpackAt = demo.IndexOf("ftx_message_decode(", StringComparison.Ordinal);
        Assert.True(emptySlotAt >= 0 && unpackAt > emptySlotAt,
            "the message is no longer unpacked after the duplicate check.");

        _output.WriteLine("  key: the full packed payload. The CRC is the BUCKET, not the identity -");
        _output.WriteLine("  a bucket clash probes forward and only an equal payload counts as a repeat.");
        _output.WriteLine("  The text is produced AFTER the check, so it cannot be what is compared.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  THE PORT'S EQUIVALENT, and why it is equivalent: the payload is the 77");
        _output.WriteLine("  message bits followed by their own CRC-14, and the CRC is a function of");
        _output.WriteLine("  those 77 bits. So comparing the 77 bits partitions the decodes exactly as");
        _output.WriteLine("  comparing upstream's ten payload bytes does. The port compares the 77 bits.");
    }

    /// <summary>
    /// WEAKEST. <b>The four numbers the application chose</b>, all of them file-scope constants in
    /// the demo and none of them in <c>ft8/</c>. Three of them already have a matching default in
    /// this library; the fourth had no counterpart until tonight.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheFourDecodeConstantsBelongToTheApplicationAndThreeAlreadyMatchThisLibrary()
    {
        var demo = ReadSource(@"demo\decode_ft8.c");

        var read = new (string Name, int Value)[]
        {
            ("kMin_score", ReadIntConstant(demo, "kMin_score")),
            ("kMax_candidates", ReadIntConstant(demo, "kMax_candidates")),
            ("kLDPC_iterations", ReadIntConstant(demo, "kLDPC_iterations")),
            ("kMax_decoded_messages", ReadIntConstant(demo, "kMax_decoded_messages")),
        };

        foreach (var (name, value) in read)
        {
            _output.WriteLine($"  {name,-24} = {value,4}   demo\\decode_ft8.c");
        }

        // None of the four is in the library half of the pin.
        foreach (var relative in new[] { @"ft8\decode.c", @"ft8\decode.h", @"ft8\constants.h" })
        {
            var library = ReadSource(relative);
            foreach (var (name, _) in read)
            {
                Assert.DoesNotContain(name, library, StringComparison.Ordinal);
            }
        }

        _output.WriteLine("  NONE of the four appears anywhere in ft8/. All four are the application's.");
        _output.WriteLine(string.Empty);

        // What this library carries, asserted against what was read. NOTHING IS TUNED: these are the
        // port's existing defaults and this test is the reading that settles that they are upstream's.
        Assert.Equal(Ft8SyncSearch.DefaultMinimumScore, read[0].Value);
        Assert.Equal(Ft8SyncSearch.DefaultCandidateLimit, read[1].Value);
        Assert.Equal(LdpcDecoder.DefaultMaxIterations, read[2].Value);

        _output.WriteLine($"  Ft8SyncSearch.DefaultMinimumScore    = {Ft8SyncSearch.DefaultMinimumScore}  MATCHES");
        _output.WriteLine($"  Ft8SyncSearch.DefaultCandidateLimit  = {Ft8SyncSearch.DefaultCandidateLimit}  MATCHES");
        _output.WriteLine($"  LdpcDecoder.DefaultMaxIterations     = {LdpcDecoder.DefaultMaxIterations}  MATCHES");
        _output.WriteLine("  Three of the four already match upstream and NOTHING WAS TUNED - these are");
        _output.WriteLine("  the defaults units 214 and 215 shipped, and this is the reading that says");
        _output.WriteLine("  they are upstream's rather than somebody's preference.");
        _output.WriteLine("  The fourth, the decoded-message limit, had no counterpart in this library");
        _output.WriteLine("  because nothing here returned a list of messages until tonight. The whole");
        _output.WriteLine("  path adds one and Ft8SlotDecoderProvenanceTests binds it to this number.");
    }

    // ------------------------------------------------------------------------------------------
    // PART B — the recordings, and criterion 3 stands on this.
    // ------------------------------------------------------------------------------------------

    /// <summary>
    /// <b>The inventory criterion 3 stands on.</b> What reference WAVs the clone holds, at what rate,
    /// and — the question the arbiter could not answer because its sandbox was refused the clone —
    /// whether an expected decode list exists beside them.
    /// </summary>
    /// <remarks>
    /// <b>Rung 1 of the instruction's ladder.</b> There is a checked-in expected-decode text file
    /// beside most of the recordings, named for the recording, holding one line per message in
    /// upstream's own <c>decode_ft8</c> print format. Nothing was invented and no other rung was
    /// needed.
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void TheCloneCarriesReferenceRecordingsWithExpectedDecodeListsBesideThem()
    {
        var root = Path.Combine(RequireReachableClone(), WavDirectory);
        Assert.True(Directory.Exists(root), $"the pin no longer holds {WavDirectory}.");

        var recordings = ReferenceRecordings.All().ToArray();
        Assert.NotEmpty(recordings);

        _output.WriteLine($"{"recording",-30} {"rate",6} {"ch",3} {"bits",5} {"samples",8} {"seconds",8}  expected");
        foreach (var recording in recordings)
        {
            _output.WriteLine(
                $"{recording.Name,-30} {recording.SampleRate,6} {recording.Channels,3} "
                + $"{recording.BitsPerSample,5} {recording.SampleCount,8} {recording.Seconds,8:F3}  "
                + (recording.HasExpectedList ? $"{recording.ExpectedCount} lines" : "NONE"));
        }

        var withList = recordings.Where(r => r.HasExpectedList).ToArray();
        var withoutList = recordings.Where(r => !r.HasExpectedList).ToArray();

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  recordings:                {recordings.Length}");
        _output.WriteLine($"  with an expected list:     {withList.Length}");
        _output.WriteLine($"  without an expected list:  {withoutList.Length}");
        _output.WriteLine($"  expected messages in all:  {withList.Sum(r => r.ExpectedCount)}");
        _output.WriteLine($"  rates present:             "
            + string.Join(", ", recordings.Select(r => r.SampleRate).Distinct().OrderBy(r => r)));
        _output.WriteLine(string.Empty);
        _output.WriteLine("  RUNG 1 of the instruction's ladder: a checked-in expected-decode file");
        _output.WriteLine("  beside the recordings, named for the recording. Nothing was invented and");
        _output.WriteLine("  no lower rung was needed.");

        Assert.NotEmpty(withList);
        Assert.All(withList, r => Assert.True(r.ExpectedCount > 0));

        // Every recording carrying a list is at a rate the geometry accepts, so the criterion is not
        // silently narrowed by a resampling question.
        foreach (var recording in withList)
        {
            Assert.Equal(1, recording.Channels);
            Assert.Equal(16, recording.BitsPerSample);
            _ = new Ft8WaterfallGeometry(recording.SampleRate);
        }

        _output.WriteLine($"  all {withList.Length} recordings with a list are mono 16-bit at a rate the");
        _output.WriteLine("  geometry accepts, so none of them is skipped for a rate.");
    }

    /// <summary>
    /// The expected list's format, read rather than assumed: it is upstream's own
    /// <c>decode_ft8</c> output line, and the message text is what follows the tilde.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheExpectedListIsUpstreamsOwnPrintFormatWithTheTextAfterTheTilde()
    {
        // The format string the decoder prints each accepted message with. Read UNCOLLAPSED,
        // because the two spaces after the tilde are part of the format and collapsing would eat
        // exactly the thing being asserted.
        var demo = ExtractFunctionBody(ReadSource(@"demo\decode_ft8.c"), "decode");
        Assert.Matches(@"printf\(""%02d%02d%02d %\+05\.1f %\+4\.2f %4\.0f ~  %s\\n""", demo);

        _output.WriteLine(@"  upstream prints: ""%02d%02d%02d %+05.1f %+4.2f %4.0f ~  %s\n""");
        _output.WriteLine("  slot time, SNR, seconds, hertz, a tilde, two spaces, then the message.");
        _output.WriteLine("  So the message text of an expected line is everything after the tilde.");
        _output.WriteLine(string.Empty);

        var recordings = ReferenceRecordings.All().Where(r => r.HasExpectedList).ToArray();
        var lines = 0;
        var hashed = 0;
        var annotated = 0;
        foreach (var recording in recordings)
        {
            foreach (var raw in File.ReadAllLines(recording.ExpectedPath))
            {
                if (raw.Trim().Length == 0)
                {
                    continue;
                }

                lines++;
                var tilde = raw.IndexOf('~');
                Assert.True(tilde > 0, $"an expected line in {recording.Name} has no tilde: it is not upstream's format.");

                var field = raw[(tilde + 1)..].Trim();
                Assert.NotEqual(0, field.Length);

                if (field.Contains("<...>", StringComparison.Ordinal))
                {
                    hashed++;
                }

                if (Regex.IsMatch(field, @"\S  +\S"))
                {
                    annotated++;
                }
            }
        }

        _output.WriteLine($"  {lines} expected lines across {recordings.Length} recordings, every one in that format.");
        _output.WriteLine($"  {hashed} of them name a station by an UNRESOLVED HASH, printed as <...> -");
        _output.WriteLine("  upstream could not read those either, from the same recording, so they are");
        _output.WriteLine("  compared like any other line rather than excused.");
        _output.WriteLine($"  {annotated} of them carry a trailing annotation after a RUN OF TWO OR MORE");
        _output.WriteLine("  SPACES - a country or continent name that upstream's own printf does not");
        _output.WriteLine("  emit, so those lists were post-processed by something other than the demo.");
        _output.WriteLine("  An FT8 message is single-space separated, so the run of two is an unambiguous");
        _output.WriteLine("  boundary and the comparison takes the text to its left. Stated, not hidden.");

        Assert.True(lines > 0);
    }

    /// <summary>
    /// The split itself, printed as one table, because every unit of this phase from 209 onward has
    /// been required to say which of its shapes could have been misread.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheAnchoringOfEachExtractionShapeIsReported()
    {
        RequireReachableClone();

        var rows = new (string Shape, string Anchor, string Where)[]
        {
            ("the extraction entry point and its signature", "STRONG", @"declaration in ft8\decode.c"),
            ("the decode entry point, its five parameters", "STRONG", @"declaration in ft8\decode.h"),
            ("the waterfall axis order and block stride", "STRONG", @"struct + comment in ft8\decode.h"),
            ("the two magnitude macros and which is which", "STRONG", @"macros in ft8\decode.h"),
            ("58 data symbols, 79 channel symbols", "STRONG", @"macros in ft8\constants.h"),
            ("174 codeword bits", "STRONG", @"macro in ft8\constants.h"),
            ("the eight-entry forward Gray map", "STRONG", @"extern in ft8\constants.h"),
            ("candidate indexes the store in axis order", "weak", @"function body, ft8\decode.c"),
            ("sync blocks stepped over: k + (k<29 ? 7 : 14)", "weak", @"function body, ft8\decode.c"),
            ("an out-of-range block gives three zero ratios", "weak", @"function body, ft8\decode.c"),
            ("magnitudes gathered in VALUE order, forward map", "weak", @"function body, ft8\decode.c"),
            ("max over ones minus max over zeros, per bit", "weak", @"function body, ft8\decode.c"),
            ("normalise to a fixed variance, mean not removed", "weak", @"function body, ft8\decode.c"),
            ("extract, normalise, decode; one attempt each", "weak", @"function body, ft8\decode.c"),
            ("duplicates decided on the whole packed payload", "weak", @"function body, demo\decode_ft8.c"),
            ("the normalisation's target variance", "WEAKEST", @"'experimentally found', ft8\decode.c"),
            ("minimum score, candidate limit", "WEAKEST", @"application constants, demo\decode_ft8.c"),
            ("LDPC iterations, decoded-message limit", "WEAKEST", @"application constants, demo\decode_ft8.c"),
        };

        _output.WriteLine($"{"shape",-50} {"anchor",-8} where");
        foreach (var (shape, anchor, where) in rows)
        {
            _output.WriteLine($"{shape,-50} {anchor,-8} {where}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  strong: {rows.Count(r => r.Anchor == "STRONG")}");
        _output.WriteLine($"  weak:   {rows.Count(r => r.Anchor == "weak")}");
        _output.WriteLine($"  weakest: {rows.Count(r => r.Anchor == "WEAKEST")}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  UNREAD, and named as unread rather than guessed:");
        _output.WriteLine("  1. WHAT PRODUCED THE EXPECTED LISTS. They are in upstream's print format,");
        _output.WriteLine("     but some carry a trailing country annotation the demo does not emit, and");
        _output.WriteLine("     nothing in the clone says which build, which version or which machine");
        _output.WriteLine("     wrote them. They are treated as upstream's claim about its own");
        _output.WriteLine("     recordings, which is what criterion 3 asks for, and not as ground truth");
        _output.WriteLine("     about what was transmitted.");
        _output.WriteLine("  2. WHETHER THE LISTS ARE COMPLETE. Nothing states that upstream's decoder");
        _output.WriteLine("     found every message present in each recording, so a message this port");
        _output.WriteLine("     returns that is not on the list is NOT PROVEN to be a false decode - and");
        _output.WriteLine("     it is still counted and reported as one, because that is the safe way");
        _output.WriteLine("     round and this project refuses a decode nobody transmitted.");
        _output.WriteLine("  3. THE ABSOLUTE BLOCK-TO-SAMPLE ALIGNMENT, still. Unit 214 carried it");
        _output.WriteLine("     forward and reading extraction does not settle it - it settles only that");
        _output.WriteLine("     the scorer and extraction share get_cand_mag and therefore cannot");
        _output.WriteLine("     disagree with each other. Task 4 measures it end to end instead.");
    }

    private static int ReadIntConstant(string source, string name)
    {
        var match = Regex.Match(source, $@"const\s+int\s+{Regex.Escape(name)}\s*=\s*(\d+)\s*;");
        Assert.True(match.Success, $"{name} is no longer an int constant in the demo.");
        return int.Parse(match.Groups[1].Value);
    }

    private static string CollapseWhitespace(string source) => Regex.Replace(source, @"\s+", " ");

    private string ReadSource(string relative)
    {
        var path = Path.Combine(RequireReachableClone(), relative);
        Assert.True(File.Exists(path), $"the pin no longer holds {relative}.");
        return File.ReadAllText(path);
    }

    /// <summary>
    /// Pulls one function's body out by brace matching, so an assertion aimed at
    /// <c>ft8_extract_symbol</c> cannot be satisfied by a line in <c>ft4_extract_symbol</c> — the FT4
    /// routine sitting next door is nearly the same text, and unit 209 was caught by exactly that.
    /// </summary>
    private static string ExtractFunctionBody(string source, string name)
    {
        var head = Regex.Match(
            source,
            $@"^[A-Za-z_][A-Za-z0-9_ \t\*]*\b{Regex.Escape(name)}\s*\([^;{{]*\)\s*\{{",
            RegexOptions.Multiline);
        Assert.True(head.Success, $"{name} is no longer defined in the source read.");

        var depth = 0;
        var start = head.Index + head.Length - 1;
        for (var i = start; i < source.Length; i++)
        {
            if (source[i] == '{')
            {
                depth++;
            }
            else if (source[i] == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return source[start..(i + 1)];
                }
            }
        }

        Assert.Fail($"{name}'s body does not close.");
        return string.Empty;
    }

    private string RequireReachableClone()
    {
        if (ReferenceClone.Probe(out var detail) == ReferenceClone.Reach.PresentButUnreadable)
        {
            Assert.Fail(
                $"{ReferenceClone.Location} exists but the test process could not read it: {detail}. "
                + "There is no other route to the pinned source, so nothing can be read tonight.");
        }

        return ReferenceClone.Location;
    }
}
