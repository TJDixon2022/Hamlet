using Ft8Sharp.Dsp;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>THE REFUSAL CENSUS. What happens to every codeword that passes parity and CRC and does not
/// become text.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>The number this exists for is in unit 216's own table and no unit had looked at it.</b> Over
/// upstream's sixty off-air recordings, 7803 candidates produced 2733 that satisfied all 83 parity
/// checks, 2733 that carried their own CRC-14 — the two agree on every row — and only 2263 that
/// became text. <b>470 validated FT8 codewords were thrown away after being proven genuine</b>, and
/// the stage that threw them away is not the DSP. It is <see cref="Ft8MessageDecoder"/>, which has
/// exactly three ways to refuse.
/// </para>
/// <para>
/// <b>Nothing is instrumented inside the library.</b> The path is reproduced here candidate by
/// candidate out of the same public parts <see cref="Ft8SlotDecoder"/> composes — the same monitor,
/// the same search, <see cref="Ft8SoftSymbols"/>, and <see cref="Ft8CodewordDecoder"/> with one
/// callsign cache per slot — because the gate already hands back the refusal on its result and
/// nothing had ever read it. <b>Ft8SlotDecoder is not modified for this and the totals prove the
/// reproduction is faithful:</b> the candidate, parity, checksum and text counts are asserted equal
/// to the ones the untold path returns for the same recording.
/// </para>
/// <para>
/// <b>Distinct payloads first, occurrences second, every time.</b> A message refused at five
/// candidates is one message and four duplicates. The occurrence count is larger and it flatters the
/// night, so it is never reported alone.
/// </para>
/// </remarks>
public class Ft8RefusalCensusTests
{
    private readonly ITestOutputHelper _output;

    public Ft8RefusalCensusTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// <b>THE CENSUS.</b> Every unpack-stage refusal over all sixty recordings, by status, as
    /// occurrences and as distinct payloads, with the arithmetic that ties the total to
    /// validated-minus-text shown rather than asserted.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void EveryValidatedCodewordThatNeverBecameTextIsAccountedForByStatus()
    {
        var census = RefusalCensus.OverTheReferenceRecordings();

        _output.WriteLine("  THE ARITHMETIC FIRST, so the table below has something to tie to.");
        _output.WriteLine($"    candidates over the sixty recordings:        {census.Candidates}");
        _output.WriteLine($"    of those, satisfied all 83 parity checks:    {census.ParitySatisfied}");
        _output.WriteLine($"    of those, carried their own CRC-14:          {census.ChecksumPassed}");
        _output.WriteLine($"    of those, became text:                       {census.BecameText}");
        _output.WriteLine($"    VALIDATED AND NEVER BECAME TEXT:             "
            + $"{census.ChecksumPassed} - {census.BecameText} = {census.ChecksumPassed - census.BecameText}");
        _output.WriteLine(string.Empty);

        _output.WriteLine($"{"refusal",-22} {"occurrences",12} {"distinct",9} {"on a list",10}");
        var totalOccurrences = 0;
        var totalDistinct = 0;
        var totalOnAList = 0;

        foreach (var status in new[]
        {
            Ft8DecodeStatus.UnsupportedType,
            Ft8DecodeStatus.UnresolvedCallsign,
            Ft8DecodeStatus.MalformedField,
        })
        {
            var occurrences = census.Occurrences(status);
            var distinct = census.Distinct(status);
            var onAList = census.DistinctOnAnExpectedList(status);

            totalOccurrences += occurrences;
            totalDistinct += distinct;
            totalOnAList += onAList;

            _output.WriteLine($"{status,-22} {occurrences,12} {distinct,9} {onAList,10}");
        }

        _output.WriteLine($"{"TOTAL",-22} {totalOccurrences,12} {totalDistinct,9} {totalOnAList,10}");

        _output.WriteLine(string.Empty);
        _output.WriteLine("  THE THREE STATUSES SUM TO THE VALIDATED-MINUS-TEXT COUNT:");
        _output.WriteLine($"    {census.Occurrences(Ft8DecodeStatus.UnsupportedType)}"
            + $" + {census.Occurrences(Ft8DecodeStatus.UnresolvedCallsign)}"
            + $" + {census.Occurrences(Ft8DecodeStatus.MalformedField)}"
            + $" = {totalOccurrences}, and {census.ChecksumPassed} - {census.BecameText}"
            + $" = {census.ChecksumPassed - census.BecameText}");

        // The census is exhaustive by construction: there is no fourth refusal and no way past the
        // gate that does not produce text. If these ever disagree, something returned a message
        // without a status or a status without a message.
        Assert.Equal(census.ChecksumPassed - census.BecameText, totalOccurrences);

        _output.WriteLine(string.Empty);
        _output.WriteLine("  BY TYPE CODE, for the refusals that are about a type this library has not built:");
        _output.WriteLine($"{"type",-24} {"occurrences",12} {"distinct",9}");
        var unsupportedRows = 0;
        foreach (var (type, occurrences, distinct) in census.UnsupportedTypeBreakdown())
        {
            unsupportedRows++;
            _output.WriteLine($"{type,-24} {occurrences,12} {distinct,9}");
        }

        if (unsupportedRows == 0)
        {
            _output.WriteLine("    (no rows: NOT ONE of the 470 was refused for an unsupported type.)");
            _output.WriteLine("    THIS IS TASK 7'S DROP CONDITION AND IT DECIDES IT. The census names no type,");
            _output.WriteLine("    so the bar of 20 distinct expected messages is not cleared - it is not");
            _output.WriteLine("    approached. Task 2 found independently that upstream's own message layer");
            _output.WriteLine("    decodes exactly the same four types this library builds, so there was never");
            _output.WriteLine("    a type of message on these recordings that upstream reads and this does not.");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  BY TYPE CODE, for the refusals about a callsign held as a hash:");
        _output.WriteLine($"{"type",-24} {"occurrences",12} {"distinct",9}");
        foreach (var (type, occurrences, distinct) in census.TypeBreakdown(Ft8DecodeStatus.UnresolvedCallsign))
        {
            _output.WriteLine($"{type,-24} {occurrences,12} {distinct,9}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  BY TYPE CODE, for the refusals about a field the protocol does not define:");
        _output.WriteLine($"{"type",-24} {"occurrences",12} {"distinct",9}");
        foreach (var (type, occurrences, distinct) in census.TypeBreakdown(Ft8DecodeStatus.MalformedField))
        {
            _output.WriteLine($"{type,-24} {occurrences,12} {distinct,9}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  MalformedField deserves its own paragraph whatever its count. A codeword that");
        _output.WriteLine("  satisfied 83 parity checks AND a 14-bit checksum is a genuine transmission with");
        _output.WriteLine("  overwhelming probability, so if this library calls one of its fields malformed");
        _output.WriteLine("  that is a port defect and not a refusal.");
        _output.WriteLine($"    MalformedField, distinct payloads:  {census.Distinct(Ft8DecodeStatus.MalformedField)}");
        _output.WriteLine($"    MalformedField, occurrences:        {census.Occurrences(Ft8DecodeStatus.MalformedField)}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("    WHICH FIELD, re-derived from the same 77 bits. Upstream refuses at exactly");
        _output.WriteLine("    these three places too, with ERROR_CALLSIGN1, ERROR_CALLSIGN2 and ERROR_GRID,");
        _output.WriteLine("    so a refusal landing on one of them is agreement rather than a defect.");
        foreach (var (field, distinct) in census.MalformedFieldByField())
        {
            _output.WriteLine($"      {field,-50} {distinct,5} distinct");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("    every distinct one, in full:");
        foreach (var line in census.MalformedFieldDetail())
        {
            _output.WriteLine($"      {line}");
        }

        // If a malformed-field refusal lands anywhere other than upstream's own three, that is a
        // port defect with an address and it must not pass quietly.
        Assert.DoesNotContain(
            census.MalformedFieldByField(),
            entry => entry.Field.Contains("INVESTIGATE", StringComparison.Ordinal));

        _output.WriteLine(string.Empty);
        _output.WriteLine("  THE JOIN BETWEEN THE TWO LEDGERS. A distinct refused payload is 'on a list' when");
        _output.WriteLine("  its 77 message bits equal the bits an expected line for that same recording packs");
        _output.WriteLine("  to through this library's own packers. Lines the list itself prints <...> for");
        _output.WriteLine("  cannot be packed by anybody and are excluded from the join rather than counted");
        _output.WriteLine("  as misses of it.");
        _output.WriteLine($"    distinct refused payloads:                     {totalDistinct}");
        _output.WriteLine($"    of those, matching an expected line exactly:   {totalOnAList}");
        _output.WriteLine($"    expected lines this packer could represent:    {census.ExpectedLinesRepresentable}"
            + $" of {census.ExpectedLines}");

        _output.WriteLine(string.Empty);
        _output.WriteLine("  THE PRICE OF THE HASHED-CALLSIGN REFUSAL, STATED AS TWO NUMBERS.");
        _output.WriteLine("  This library refuses a message naming a station by a hash nothing in the slot");
        _output.WriteLine("  has heard spelled out. Upstream writes <...> and prints the line. What that");
        _output.WriteLine("  refusal costs criterion 3 is:");
        var (realised, realisedLines) = census.RealisedPriceOfTheHashedCallsignRefusal();
        _output.WriteLine($"    UPPER BOUND - expected lines printed <...>:    "
            + $"{census.ExpectedLinesCarryingAnUnresolvedHash} of {census.ExpectedLines}"
            + $"  ({100.0 * census.ExpectedLinesCarryingAnUnresolvedHash / Math.Max(1, census.ExpectedLines):F1} per cent)");
        _output.WriteLine($"      No improvement to this receiver can ever match one of these while the");
        _output.WriteLine($"      refusal stands, because matching would mean printing the placeholder.");
        _output.WriteLine($"    REALISED - of those, ones whose codeword this library RECOVERED and then");
        _output.WriteLine($"      refused, so a placeholder would have turned them into matches:  {realised}");
        foreach (var line in realisedLines)
        {
            _output.WriteLine($"        {line}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  THE RENDERING USED FOR THAT SECOND NUMBER IS A RULER AND NOT A CHANGE. It is");
        _output.WriteLine("  computed in this test project, it is never returned to a caller and it never");
        _output.WriteLine("  reaches Ft8SlotDecoder or anything below it. The library still refuses the whole");
        _output.WriteLine("  message. HM-DEC-009 stands and CLAUDE.md 12.1 puts the question with the owner.");

        _output.WriteLine(string.Empty);
        _output.WriteLine("  THE NUMBERS ARE PRINTED BEFORE ANY BOUND IS ASSERTED ON THEM.");

        Assert.Equal(census.ChecksumPassed, census.ParitySatisfied);
        Assert.True(totalDistinct > 0, "no validated codeword was refused at the unpack stage at all.");
    }

    /// <summary>
    /// <b>THE CONTROL ON THE JOIN, and without it the join's answer cannot be read.</b> The packer
    /// that turns an expected line back into 77 bits is checked against messages <em>this library
    /// itself decoded</em>: pack the text it returned and see whether the bits come back.
    /// </summary>
    /// <remarks>
    /// <b>A join that finds nothing has two explanations and they are opposite.</b> Either the
    /// refused payloads are genuinely not on the expected lists, or the instrument doing the joining
    /// cannot reproduce wire bits at all. This settles which, on messages whose bits are known
    /// exactly because the decoder returned them, before any conclusion is drawn from a zero.
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void ThePackerReproducesTheBitsOfMessagesThisLibraryItselfDecoded()
    {
        var census = RefusalCensus.OverTheReferenceRecordings();
        Assert.NotEmpty(census.Decoded);

        var reproduced = 0;
        var refused = 0;
        var wrongBits = 0;
        var examples = new List<string>();

        foreach (var (file, text, message) in census.Decoded)
        {
            var normalised = ReferenceRecording.Normalise(text);
            var outcome = ExpectedMessagePacker.TryPack(normalised, out var packed);

            if (outcome != ExpectedMessagePacker.PackFailure.None)
            {
                refused++;
                if (examples.Count < 12)
                {
                    examples.Add($"REFUSED  {outcome,-34} {file,-22} {normalised}");
                }

                continue;
            }

            if (ExpectedMessagePacker.SameMessage(packed, message))
            {
                reproduced++;
            }
            else
            {
                wrongBits++;
                if (examples.Count < 12)
                {
                    examples.Add($"BITS     differ                             {file,-22} {normalised}");
                }
            }
        }

        var total = census.Decoded.Count;
        _output.WriteLine($"  messages this library decoded off the sixty recordings: {total}");
        _output.WriteLine($"    packed back to THE SAME 77 BITS:                     {reproduced}"
            + $"  ({100.0 * reproduced / Math.Max(1, total):F1} per cent)");
        _output.WriteLine($"    the packer refused the text:                         {refused}");
        _output.WriteLine($"    packed, and to DIFFERENT bits:                       {wrongBits}");

        if (examples.Count > 0)
        {
            _output.WriteLine(string.Empty);
            _output.WriteLine("  the first few that did not come back:");
            foreach (var example in examples)
            {
                _output.WriteLine($"    {example}");
            }
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  READ THE JOIN AGAINST THIS FIGURE AND NOT ON ITS OWN. Whatever the join finds,");
        _output.WriteLine("  it can only be trusted as far as this instrument reproduces bits it is given the");
        _output.WriteLine("  text for. The number above is that limit, measured rather than assumed.");

        Assert.True(reproduced > 0, "the packer could not reproduce a single message this library decoded.");
    }

    /// <summary>
    /// <b>The reproduction is faithful, and it is proved rather than asserted.</b> The census walks
    /// the same parts <see cref="Ft8SlotDecoder"/> walks, so its stage counts must equal the ones
    /// the untold path returns for the same recording, file by file.
    /// </summary>
    /// <remarks>
    /// <b>Without this the census is worthless.</b> A count taken from a path that is not the path
    /// under measurement describes a different experiment. This compares all four stage counts on
    /// every one of the sixty recordings — <b>never a total, always per file</b>, because two files
    /// that are wrong in opposite directions sum to right.
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void TheCensusWalksTheSamePathTheSlotDecoderWalks()
    {
        var recordings = ReferenceRecordings.WithExpectedLists();
        Assert.NotEmpty(recordings);

        var comparisons = 0;
        foreach (var recording in recordings)
        {
            var geometry = new Ft8WaterfallGeometry(recording.SampleRate);
            var waterfall = new Ft8Monitor(geometry).Analyse(recording.ReadSamples());

            var told = new Ft8SlotDecoder(geometry).Decode(waterfall);
            var walked = RefusalCensus.OverOneSlot(waterfall);

            Assert.Equal(told.CandidateCount, walked.Candidates);
            Assert.Equal(told.ParitySatisfiedCount, walked.ParitySatisfied);
            Assert.Equal(told.ChecksumPassedCount, walked.ChecksumPassed);
            Assert.Equal(told.BecameTextCount, walked.BecameText);
            comparisons += 4;
        }

        _output.WriteLine($"  {recordings.Count} recordings, {comparisons} stage-count comparisons, all equal.");
        _output.WriteLine("  The census is the same walk the slot decoder makes, so its refusals are that");
        _output.WriteLine("  path's refusals and not another path's.");
        Assert.True(comparisons > 0);
    }
}

/// <summary>
/// Walks the decode path candidate by candidate and keeps what <see cref="Ft8SlotDecoder"/> throws
/// away: the refusal on every validated codeword that did not become text.
/// </summary>
/// <remarks>
/// <b>It composes the library's public parts and re-implements none of them.</b> Monitor, search,
/// <see cref="Ft8SoftSymbols"/> and <see cref="Ft8CodewordDecoder"/>, in the same order and with one
/// <see cref="Ft8CallsignCache"/> per slot, which is what makes its counts the slot decoder's counts.
/// The one thing it does that the slot decoder does not is run the belief propagation a second time
/// on a refused candidate to recover the 77 bits for identification — the same deterministic call the
/// slot decoder already makes on a <em>successful</em> one for de-duplication, so it is not a second
/// gate decision and no new parity or CRC judgement is made anywhere in here.
/// </remarks>
internal sealed class RefusalCensus
{
    private readonly List<Refusal> _refusals = new();
    private readonly List<(string File, string Text, byte[] Message)> _decoded = new();
    private readonly Dictionary<string, List<byte[]>> _expectedByFile = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<string>> _expectedTextByFile = new(StringComparer.Ordinal);

    /// <summary>Every successful decode, with the 77 bits it came from. The packer's control.</summary>
    internal IReadOnlyList<(string File, string Text, byte[] Message)> Decoded => _decoded;

    internal int Candidates { get; private set; }

    internal int ParitySatisfied { get; private set; }

    internal int ChecksumPassed { get; private set; }

    internal int BecameText { get; private set; }

    internal int ExpectedLines { get; private set; }

    internal int ExpectedLinesRepresentable { get; private set; }

    /// <summary>
    /// How many expected lines name a station by a hash the list's own writer could not resolve, and
    /// print it <c>&lt;...&gt;</c>. <b>The upper bound on the price of the refusal this library makes
    /// instead.</b>
    /// </summary>
    internal int ExpectedLinesCarryingAnUnresolvedHash { get; private set; }

    /// <summary>
    /// <b>The realised price of the hashed-callsign refusal.</b> How many expected lines printed
    /// <c>&lt;...&gt;</c> this library actually <em>recovered the codeword for</em> and then refused —
    /// which is the number a placeholder would have converted into a match, as opposed to the upper
    /// bound, which counts lines whose signal may never have been found at all.
    /// </summary>
    internal (int Recovered, IReadOnlyList<string> Lines) RealisedPriceOfTheHashedCallsignRefusal()
    {
        var recovered = new List<string>();

        foreach (var refusal in DistinctRefusals(Ft8DecodeStatus.UnresolvedCallsign))
        {
            var wouldPrint = UpstreamWouldPrint(refusal);
            if (wouldPrint is null
                || !_expectedTextByFile.TryGetValue(refusal.File, out var texts))
            {
                continue;
            }

            if (texts.Any(t => string.Equals(t, wouldPrint, StringComparison.Ordinal)))
            {
                recovered.Add($"{refusal.File,-22} {wouldPrint}");
            }
        }

        return (recovered.Count, recovered);
    }

    /// <summary>One refused payload at one candidate.</summary>
    internal readonly record struct Refusal(
        string File,
        Ft8DecodeStatus Status,
        Ft8MessageType Type,
        byte[] Message,
        int CandidateIndex);

    internal IReadOnlyList<Refusal> Refusals => _refusals;

    /// <summary>The census over every reference recording that carries an expected decode list.</summary>
    internal static RefusalCensus OverTheReferenceRecordings()
    {
        var census = new RefusalCensus();

        foreach (var recording in ReferenceRecordings.WithExpectedLists())
        {
            var geometry = new Ft8WaterfallGeometry(recording.SampleRate);
            var waterfall = new Ft8Monitor(geometry).Analyse(recording.ReadSamples());
            census.Walk(recording.Name, waterfall);

            // The expected side of the join, packed through this library's own packers. Lines the
            // list itself lost to a hash cannot be packed by anyone and are not counted against it.
            var keys = new List<byte[]>();
            var texts = new List<string>();
            foreach (var text in recording.ExpectedMessages())
            {
                census.ExpectedLines++;
                texts.Add(text);
                if (text.Contains(ExpectedMessagePacker.UnresolvedMarker, StringComparison.Ordinal))
                {
                    census.ExpectedLinesCarryingAnUnresolvedHash++;
                }

                if (ExpectedMessagePacker.TryPack(text, out var message) == ExpectedMessagePacker.PackFailure.None)
                {
                    census.ExpectedLinesRepresentable++;
                    keys.Add(message);
                }
            }

            census._expectedByFile[recording.Name] = keys;
            census._expectedTextByFile[recording.Name] = texts;
        }

        return census;
    }

    /// <summary>The stage counts for one already-built waterfall, for the faithfulness proof.</summary>
    internal static RefusalCensus OverOneSlot(Ft8Waterfall waterfall)
    {
        var census = new RefusalCensus();
        census.Walk("(one slot)", waterfall);
        return census;
    }

    internal int Occurrences(Ft8DecodeStatus status) => _refusals.Count(r => r.Status == status);

    /// <summary>
    /// Distinct refused payloads of one status. <b>Distinct means one message once per recording</b>
    /// — a payload refused at five candidates in the same slot is one message and four duplicates,
    /// and the same text genuinely sent in two different recordings is two messages.
    /// </summary>
    internal int Distinct(Ft8DecodeStatus status) => DistinctRefusals(status).Count();

    /// <summary>
    /// How many distinct refused payloads match an expected line for the recording they came from.
    /// </summary>
    internal int DistinctOnAnExpectedList(Ft8DecodeStatus status)
    {
        var seen = new List<(string File, byte[] Message)>();
        var count = 0;

        foreach (var refusal in _refusals.Where(r => r.Status == status))
        {
            if (seen.Any(s => s.File == refusal.File
                && ExpectedMessagePacker.SameMessage(s.Message, refusal.Message)))
            {
                continue;
            }

            seen.Add((refusal.File, refusal.Message));

            if (_expectedByFile.TryGetValue(refusal.File, out var keys)
                && keys.Any(k => ExpectedMessagePacker.SameMessage(k, refusal.Message)))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>The unsupported-type refusals broken down by the type code they declared.</summary>
    internal IEnumerable<(Ft8MessageType Type, int Occurrences, int Distinct)> UnsupportedTypeBreakdown()
    {
        var unsupported = _refusals.Where(r => r.Status == Ft8DecodeStatus.UnsupportedType).ToList();

        foreach (var type in unsupported.Select(r => r.Type).Distinct().OrderBy(t => t.ToString(), StringComparer.Ordinal))
        {
            var ofType = unsupported.Where(r => r.Type == type).ToList();
            var distinct = new List<(string File, byte[] Message)>();
            foreach (var refusal in ofType)
            {
                if (!distinct.Any(d => d.File == refusal.File
                    && ExpectedMessagePacker.SameMessage(d.Message, refusal.Message)))
                {
                    distinct.Add((refusal.File, refusal.Message));
                }
            }

            yield return (type, ofType.Count, distinct.Count);
        }
    }

    /// <summary>
    /// <b>Which field a malformed-field refusal was actually about</b>, re-derived from the same 77
    /// bits by calling the field readers directly. Purely a read: no state is touched and nothing is
    /// decided.
    /// </summary>
    /// <remarks>
    /// <b>This is what separates a port defect from upstream's own refusal.</b> Upstream returns
    /// <c>ERROR_CALLSIGN1</c>, <c>ERROR_CALLSIGN2</c> or <c>ERROR_GRID</c> at exactly these three
    /// places and its demo prints <c>Error [n] while unpacking!</c> in place of the message, so a
    /// refusal here that lands on one of those three is agreement with upstream rather than a
    /// defect. A refusal that lands somewhere else would be the finding.
    /// </remarks>
    internal IEnumerable<(string Field, int Distinct)> MalformedFieldByField()
    {
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var refusal in DistinctRefusals(Ft8DecodeStatus.MalformedField))
        {
            counts.TryGetValue(WhichField(refusal), out var current);
            counts[WhichField(refusal)] = current + 1;
        }

        return counts.OrderByDescending(p => p.Value).Select(p => (p.Key, p.Value));
    }

    /// <summary>Every distinct malformed-field refusal, named by file, declared type and field.</summary>
    internal IEnumerable<string> MalformedFieldDetail()
    {
        foreach (var refusal in DistinctRefusals(Ft8DecodeStatus.MalformedField))
        {
            yield return $"{refusal.File,-22} type {refusal.Type,-20} {WhichField(refusal),-28} "
                + $"first at candidate {refusal.CandidateIndex}";
        }
    }

    /// <summary>
    /// <b>The text upstream's message layer would have printed for these same 77 bits</b>, with its
    /// hash table empty: the resolvable fields as they read, and the literal <c>&lt;...&gt;</c> where
    /// this library refuses. <see langword="null"/> where no rendering exists at all.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>THIS IS A RULER AND IT IS NOT A CHANGE TO ANYTHING.</b> It lives in the test project, it is
    /// called only by a report, and its output is never returned to a caller, never written to a
    /// message, and never reaches <see cref="Ft8SlotDecoder"/> or anything below it. The library goes
    /// on refusing the whole message, which is HM-DEC-009 and a numbered divergence, and whether a
    /// decoder may ever say <em>a station I cannot name</em> is the owner's under <c>CLAUDE.md</c>
    /// §12.1.
    /// </para>
    /// <para>
    /// <b>It exists because the price of that refusal has to be a number.</b> An expected line the
    /// list itself prints <c>&lt;...&gt;</c> for cannot be packed and so cannot join on bits; the only
    /// way to ask whether this library <em>found</em> that transmission is to compare what it would
    /// have said against what the list says. That is a measurement of a cost, not a step towards
    /// paying it.
    /// </para>
    /// </remarks>
    internal static string? UpstreamWouldPrint(Refusal refusal)
    {
        if (refusal.Type != Ft8MessageType.Standard)
        {
            return null;
        }

        var message = refusal.Message;
        var n29a = ((uint)message[0] << 21) | ((uint)message[1] << 13)
            | ((uint)message[2] << 5) | ((uint)message[3] >> 3);
        var n29b = (((uint)message[3] & 0x07u) << 26) | ((uint)message[4] << 18)
            | ((uint)message[5] << 10) | ((uint)message[6] << 2) | ((uint)message[7] >> 6);
        var reportFlag = (message[7] & 0x20) != 0;
        var grid = ((message[7] & 0x1F) << 10) | (message[8] << 2) | (message[9] >> 6);
        var i3 = Ft8MessageTypes.Primary(message);

        var to = Field(n29a, i3);
        var de = Field(n29b, i3);
        if (to is null || de is null)
        {
            return null;
        }

        if (Ft8GridField.TryUnpack(grid, reportFlag, out var extra, out _) != Ft8FieldResult.Ok)
        {
            return null;
        }

        return string.Join(' ', new[] { to, de, extra }.Where(part => part.Length > 0));

        static string? Field(uint n29, int i3)
        {
            var result = Ft8CallsignField.TryUnpack(n29 >> 1, (n29 & 1u) != 0, i3, out var text, out _);
            return result switch
            {
                Ft8FieldResult.Ok => text,
                Ft8FieldResult.UnresolvedCallsign => ExpectedMessagePacker.UnresolvedMarker,
                _ => null,
            };
        }
    }

    /// <summary>The refusals of one status broken down by the type code they declared.</summary>
    internal IEnumerable<(Ft8MessageType Type, int Occurrences, int Distinct)> TypeBreakdown(Ft8DecodeStatus status)
    {
        var ofStatus = _refusals.Where(r => r.Status == status).ToList();
        var distinctOfStatus = DistinctRefusals(status).ToList();

        foreach (var type in ofStatus.Select(r => r.Type).Distinct().OrderBy(t => t.ToString(), StringComparer.Ordinal))
        {
            yield return (
                type,
                ofStatus.Count(r => r.Type == type),
                distinctOfStatus.Count(r => r.Type == type));
        }
    }

    private static string WhichField(Refusal refusal)
    {
        if (refusal.Type != Ft8MessageType.Standard)
        {
            return $"(not a standard message: {refusal.Type})";
        }

        var message = refusal.Message;
        var n29a = ((uint)message[0] << 21) | ((uint)message[1] << 13)
            | ((uint)message[2] << 5) | ((uint)message[3] >> 3);
        var n29b = (((uint)message[3] & 0x07u) << 26) | ((uint)message[4] << 18)
            | ((uint)message[5] << 10) | ((uint)message[6] << 2) | ((uint)message[7] >> 6);
        var reportFlag = (message[7] & 0x20) != 0;
        var grid = ((message[7] & 0x1F) << 10) | (message[8] << 2) | (message[9] >> 6);
        var i3 = Ft8MessageTypes.Primary(message);

        if (Ft8CallsignField.TryUnpack(n29a >> 1, (n29a & 1u) != 0, i3, out _, out _) == Ft8FieldResult.Malformed)
        {
            return "addressed callsign (upstream: ERROR_CALLSIGN1)";
        }

        if (Ft8CallsignField.TryUnpack(n29b >> 1, (n29b & 1u) != 0, i3, out _, out _) == Ft8FieldResult.Malformed)
        {
            return "transmitting callsign (upstream: ERROR_CALLSIGN2)";
        }

        if (Ft8GridField.TryUnpack(grid, reportFlag, out _, out _) == Ft8FieldResult.Malformed)
        {
            return "grid or report (upstream: ERROR_GRID)";
        }

        return "NONE OF THE THREE - INVESTIGATE";
    }

    /// <summary>Every distinct refusal of one status, with the file it came from.</summary>
    internal IEnumerable<Refusal> DistinctRefusals(Ft8DecodeStatus status)
    {
        var seen = new List<(string File, byte[] Message)>();
        foreach (var refusal in _refusals.Where(r => r.Status == status))
        {
            if (seen.Any(s => s.File == refusal.File
                && ExpectedMessagePacker.SameMessage(s.Message, refusal.Message)))
            {
                continue;
            }

            seen.Add((refusal.File, refusal.Message));
            yield return refusal;
        }
    }

    private void Walk(string file, Ft8Waterfall waterfall)
    {
        var candidates = new Ft8SyncSearch().Find(waterfall);
        var cache = new Ft8CallsignCache();
        var ratios = new float[Ft8SoftSymbols.RatioCount];
        var codeword = new byte[LdpcDecoder.CodewordBits];

        Candidates += candidates.Count;

        for (var index = 0; index < candidates.Count; index++)
        {
            Ft8SoftSymbols.Extract(waterfall, candidates[index], ratios);
            Ft8SoftSymbols.Normalise(ratios);

            var result = Ft8CodewordDecoder.Decode(ratios, cache);

            if (result.Status != Ft8CodewordStatus.ParityNeverSatisfied)
            {
                ParitySatisfied++;
            }

            if (result.Status is Ft8CodewordStatus.Decoded or Ft8CodewordStatus.MessageNotReadable)
            {
                ChecksumPassed++;
            }

            if (result.Status == Ft8CodewordStatus.Decoded)
            {
                BecameText++;

                // Recovered the same way the slot decoder recovers them for de-duplication, and
                // kept so the packer can be checked against messages this library itself read.
                LdpcDecoder.Decode(ratios, codeword, LdpcDecoder.DefaultMaxIterations);
                _decoded.Add((
                    file,
                    result.Message.Text,
                    ExpectedMessagePacker.FromBits(codeword.AsSpan(0, Ft8Payload.MessageBits))));
                continue;
            }

            if (result.Status != Ft8CodewordStatus.MessageNotReadable)
            {
                continue;
            }

            // Past both gates and refused by the message layer. Recover the 77 bits the gate did
            // not hand back, by the same deterministic call the slot decoder makes on a successful
            // decode. NO NEW PARITY OR CRC DECISION IS MADE: this payload has already passed both.
            LdpcDecoder.Decode(ratios, codeword, LdpcDecoder.DefaultMaxIterations);
            var message = ExpectedMessagePacker.FromBits(codeword.AsSpan(0, Ft8Payload.MessageBits));

            _refusals.Add(new Refusal(file, result.Message.Status, result.Message.Type, message, index));
        }
    }
}
