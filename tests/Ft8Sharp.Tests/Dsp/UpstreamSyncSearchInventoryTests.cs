using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// The sanctioned read of the pinned clone for unit 214: how upstream finds the Costas sync pattern
/// in a waterfall, how it scores a hypothesis, how far it sweeps, and where the two numbers that
/// bound the answer actually come from.
/// </summary>
/// <remarks>
/// <para>
/// <b>Read it before porting it, and leave behind something that fails loudly if a re-pin changes
/// it.</b> Every shape asserted here is a shape the search in <c>src/Ft8Sharp/Dsp/</c> was written
/// against. If upstream's search is ever re-pinned to a different one, this goes red beside the port
/// rather than the port drifting quietly. <b>This file is the record of the read alone</b> — the
/// assertions that bind these shapes to the port's own constants live beside the port, in
/// <c>Ft8SyncSearchProvenanceTests</c>, so that this one compiles and answers on its own.
/// </para>
/// <para>
/// <b>Shapes and counts, never values that are upstream's to own.</b> Identifiers, presences, guard
/// conditions and structural facts are asserted and printed; nothing from the clone is committed. The
/// protocol's published facts — three sync groups of seven symbols thirty-six apart, the seven-tone
/// Costas array — are in the QEX paper the NOTICE cites and are free.
/// </para>
/// <para>
/// <b>Strong and weak anchoring is reported, not blurred</b>, and on this reading the split is the
/// most useful thing in the file. The candidate record, the search entry point and the sync geometry
/// are <em>strong</em>: macros and typedefs in headers, which cannot be misread. The scoring
/// arithmetic and the sweep ranges are <em>weak</em>: expressions inside a static function body.
/// And <c>kMin_score</c> and <c>kMax_candidates</c> are <em>weakest of all</em> — they are not in
/// the library at all, they are two file-scope constants in the demo application, so a caller of
/// the library has to supply them and this port exposes them as parameters rather than burying
/// them. See <see cref="TheMinimumScoreAndCandidateLimitBelongToTheApplicationAndNotTheLibrary"/>.
/// </para>
/// <para>
/// <b>The finding criterion 3 turns on is asserted here</b>, in
/// <see cref="TheSortIsAHeapsortOnScoreAloneAndIsThereforeNotATotalOrder"/>: upstream compares
/// candidates on <c>score</c> and on nothing else, and heapsort is not stable, so two candidates
/// that tie can come back in either order. That is why this library adds a tie-break — divergence
/// 19 in <c>porting-notes.md</c>.
/// </para>
/// <para>
/// <b>Values that are upstream's to own are printed, not asserted as literals.</b> The demo's
/// minimum score and candidate limit are read out and shown; what they have to equal is asserted
/// against the port's own defaults next door, which is the only place the two can drift apart.
/// </para>
/// <para><b>Absent is a skip.</b> A fresh clone stays green.</para>
/// </remarks>
public class UpstreamSyncSearchInventoryTests
{
    private readonly ITestOutputHelper _output;

    public UpstreamSyncSearchInventoryTests(ITestOutputHelper output) => _output = output;

    /// <summary>The files unit 214 is licensed to read for shapes, and no others.</summary>
    private static readonly string[] SearchSources =
    {
        @"ft8\decode.h", @"ft8\decode.c", @"ft8\constants.h", @"demo\decode_ft8.c",
    };

    /// <summary>
    /// Discovery, and it runs because assuming which file holds the search is exactly the mistake
    /// this project has paid for before.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheSearchesFilesAreFoundRatherThanAssumed()
    {
        var location = RequireReachableClone();
        _output.WriteLine($"clone: {location}");

        foreach (var relative in SearchSources)
        {
            var path = Path.Combine(location, relative);
            Assert.True(File.Exists(path), $"the pin no longer holds {relative}.");
            _output.WriteLine($"  {relative}: {File.ReadAllLines(path).Length} lines");
        }
    }

    /// <summary>
    /// STRONG. The candidate is a typedef in a header with five named fields, and the score is an
    /// integer — not a float, which is the fact that lets a port compare two candidates exactly.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheCandidateCarriesFivePositionFieldsAndAnIntegerScore()
    {
        var header = ReadSource(@"ft8\decode.h");
        var body = ExtractTypedefBody(header, "ftx_candidate_t");

        foreach (var (field, type) in new[]
                 {
                     ("score", "int16_t"),
                     ("time_offset", "int16_t"),
                     ("freq_offset", "int16_t"),
                     ("time_sub", "uint8_t"),
                     ("freq_sub", "uint8_t"),
                 })
        {
            Assert.Matches($@"\b{type}\s+{field}\s*;", body);
            _output.WriteLine($"  {field,-12} : {type}");
        }

        Assert.DoesNotMatch(@"\bfloat\s+score\s*;", body);
        _output.WriteLine("  the score is an integer type, so two candidates compare exactly.");
        _output.WriteLine("  Ft8Candidate carries the same five fields, widened to int.");
    }

    /// <summary>
    /// STRONG. The entry point, its four parameters, and the two of them the caller must choose.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheSearchEntryPointTakesACandidateLimitAndAMinimumScoreFromItsCaller()
    {
        var header = ReadSource(@"ft8\decode.h");
        var declaration = Regex.Match(
            header, @"int\s+ftx_find_candidates\s*\(([^)]*)\)\s*;", RegexOptions.Singleline);

        Assert.True(declaration.Success, "ftx_find_candidates is no longer declared in decode.h.");

        var parameters = declaration.Groups[1].Value
            .Split(',')
            .Select(p => p.Trim())
            .ToArray();

        Assert.Equal(4, parameters.Length);
        foreach (var parameter in parameters)
        {
            _output.WriteLine($"  {parameter}");
        }

        Assert.Contains(parameters, p => p.Contains("num_candidates", StringComparison.Ordinal));
        Assert.Contains(parameters, p => p.Contains("min_score", StringComparison.Ordinal));
        _output.WriteLine(
            "  Both bounds are the CALLER'S. The port takes them as parameters with the demo's "
            + "values as defaults rather than burying them in a loop.");
    }

    /// <summary>
    /// STRONG. What the score arithmetic actually reads out of the waterfall: the stored byte as an
    /// integer count, not the decibels it stands for. Half a decibel per count, and the scoring never
    /// converts.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheScoringReadsTheStoredByteAsAnIntegerCountAndNotAsDecibels()
    {
        var header = ReadSource(@"ft8\decode.h");

        Assert.Matches(@"#define\s+WF_ELEM_MAG_INT\(x\)\s*\(int\)\(x\)", header);
        Assert.Matches(@"#define\s+WF_ELEM_T\s+uint8_t", header);
        _output.WriteLine("  WF_ELEM_MAG_INT(x) is (int)(x) in the uint8_t branch that is compiled.");
        _output.WriteLine("  So a score is a sum of differences of STORED BYTES: whole counts of half");
        _output.WriteLine("  a decibel each, never floating point. Ft8SyncSearch reads the same bytes.");

        var score = ExtractFunctionBody(ReadSource(@"ft8\decode.c"), "ft8_sync_score");
        Assert.Contains("WF_ELEM_MAG_INT", score, StringComparison.Ordinal);
        Assert.DoesNotContain("WF_ELEM_MAG(", score, StringComparison.Ordinal);
    }

    /// <summary>
    /// STRONG. The sync geometry: three groups of seven, thirty-six symbols apart. Published in the
    /// QEX paper, and asserted here because the correlator's outer two loops are exactly these.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheSyncPatternIsThreeGroupsOfSevenThirtySixSymbolsApart()
    {
        var constants = ReadSource(@"ft8\constants.h");

        // The published protocol facts, from the QEX paper the NOTICE cites: three groups of seven
        // symbols, thirty-six apart. Asserted as numbers because they are public, unlike the two
        // application constants below.
        foreach (var (macro, expected) in new[]
                 {
                     ("FT8_LENGTH_SYNC", 7),
                     ("FT8_NUM_SYNC", 3),
                     ("FT8_SYNC_OFFSET", 36),
                 })
        {
            var match = Regex.Match(constants, $@"#define\s+{macro}\s*\((\d+)\)");
            Assert.True(match.Success, $"{macro} is no longer a macro in constants.h.");
            Assert.Equal(expected, int.Parse(match.Groups[1].Value));
            _output.WriteLine($"  {macro,-16} = {expected}");
        }

        Assert.Matches(@"extern\s+const\s+uint8_t\s+kFT8_Costas_pattern\[7\]", constants);
        Assert.Equal(7, Ft8Tables.Ft8CostasPattern.Length);
        _output.WriteLine(
            "  kFT8_Costas_pattern[7] is the array this port uses THROUGH Ft8Tables.Ft8CostasPattern, "
            + "machine-generated in step 1 and never re-transcribed.");
    }

    /// <summary>
    /// WEAK — every one of these is an expression inside a static function body, and the port is
    /// only as good as this reading. The score is a sum of up to four differences per sync symbol:
    /// one bin lower, one bin higher, one symbol back, one symbol forward, each guarded, and the
    /// total divided by however many were actually taken.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheScoreIsUpToFourGuardedNeighbourDifferencesPerSyncSymbolThenAveraged()
    {
        var score = ExtractFunctionBody(ReadSource(@"ft8\decode.c"), "ft8_sync_score");

        // The four neighbour terms, in the order upstream adds them. The order is asserted because
        // integer division of the total is the last step and a different order gives the same total,
        // but a MISSING term does not — and a term dropped silently is exactly what would make this
        // search deaf without making any test red.
        var terms = new[]
        {
            (@"score\s*\+=\s*WF_ELEM_MAG_INT\(p8\[sm\]\)\s*-\s*WF_ELEM_MAG_INT\(p8\[sm\s*-\s*1\]\)",
                "one frequency bin lower"),
            (@"score\s*\+=\s*WF_ELEM_MAG_INT\(p8\[sm\]\)\s*-\s*WF_ELEM_MAG_INT\(p8\[sm\s*\+\s*1\]\)",
                "one frequency bin higher"),
            (@"score\s*\+=\s*WF_ELEM_MAG_INT\(p8\[sm\]\)\s*-\s*WF_ELEM_MAG_INT\(p8\[sm\s*-\s*wf->block_stride\]\)",
                "one symbol back in time"),
            (@"score\s*\+=\s*WF_ELEM_MAG_INT\(p8\[sm\]\)\s*-\s*WF_ELEM_MAG_INT\(p8\[sm\s*\+\s*wf->block_stride\]\)",
                "one symbol forward in time"),
        };

        var at = -1;
        foreach (var (pattern, what) in terms)
        {
            var match = Regex.Match(score, pattern);
            Assert.True(match.Success, $"the '{what}' term is no longer in ft8_sync_score.");
            Assert.True(match.Index > at, $"the '{what}' term has moved in the order.");
            at = match.Index;
            _output.WriteLine($"  term: {what}");
        }

        // The guards. Each term is taken only where its neighbour exists, and the port carries the
        // same four conditions.
        Assert.Matches(@"if\s*\(sm\s*>\s*0\)", score);
        Assert.Matches(@"if\s*\(sm\s*<\s*7\)", score);
        Assert.Matches(@"if\s*\(\(k\s*>\s*0\)\s*&&\s*\(block_abs\s*>\s*0\)\)", score);
        Assert.Matches(
            @"if\s*\(\(\(k\s*\+\s*1\)\s*<\s*FT8_LENGTH_SYNC\)\s*&&\s*\(\(block_abs\s*\+\s*1\)\s*<\s*wf->num_blocks\)\)",
            score);
        _output.WriteLine("  guards: sm>0, sm<7, (k>0 && block_abs>0), (k+1<7 && block_abs+1<blocks)");

        // The block boundary handling, and the asymmetry in it is deliberate upstream: a block before
        // the start of the slot is SKIPPED and the sweep carries on, but a block past the end BREAKS
        // out of the group. Both are reproduced.
        Assert.Matches(@"if\s*\(block_abs\s*<\s*0\)\s*continue\s*;", CollapseWhitespace(score));
        Assert.Matches(
            @"if\s*\(block_abs\s*>=\s*wf->num_blocks\)\s*break\s*;", CollapseWhitespace(score));
        _output.WriteLine("  before the slot: continue.  past the end of the slot: break.");

        // And the average, which is why a candidate at the edge of the slot is comparable with one in
        // the middle: an integer division by the number of terms actually taken.
        Assert.Matches(@"num_average\s*>\s*0", score);
        Assert.Matches(@"score\s*/=\s*num_average", score);
        _output.WriteLine("  the total is divided by the number of terms taken - integer division,");
        _output.WriteLine("  truncating toward zero, which C and C# do identically for a negative total.");
    }

    /// <summary>
    /// WEAK. The sweep: every time sub-offset, every frequency sub-offset, time offsets that begin
    /// ten blocks BEFORE the slot, and every frequency offset that leaves room for eight tones.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheSweepRunsFromBeforeTheSlotAndStopsWhereTheTopToneWouldLeaveThePassband()
    {
        var find = ExtractFunctionBody(ReadSource(@"ft8\decode.c"), "ftx_find_candidates");

        Assert.Matches(@"candidate\.time_sub\s*=\s*0;\s*candidate\.time_sub\s*<\s*wf->time_osr", find);
        Assert.Matches(@"candidate\.freq_sub\s*=\s*0;\s*candidate\.freq_sub\s*<\s*wf->freq_osr", find);

        var timeOffsets = Regex.Match(
            find, @"candidate\.time_offset\s*=\s*(-?\d+);\s*candidate\.time_offset\s*<\s*(-?\d+)");
        Assert.True(timeOffsets.Success, "the time offset sweep is no longer a literal range.");
        var firstOffset = int.Parse(timeOffsets.Groups[1].Value);
        var pastLastOffset = int.Parse(timeOffsets.Groups[2].Value);
        Assert.True(firstOffset < 0, "the sweep no longer begins before the start of the slot.");
        Assert.True(pastLastOffset > 0, "the sweep no longer reaches past the start of the slot.");
        _output.WriteLine(
            $"  time offsets {timeOffsets.Groups[1].Value} .. {timeOffsets.Groups[2].Value} exclusive "
            + "- the sweep STARTS BEFORE THE SLOT, which is how a transmission that began early is found.");

        Assert.Matches(
            @"\(candidate\.freq_offset\s*\+\s*num_tones\s*-\s*1\)\s*<\s*wf->num_bins", find);
        _output.WriteLine(
            "  frequency offsets run while the EIGHTH tone still fits inside the kept bins, so the top");
        _output.WriteLine("  seven bins of the passband are never a candidate's base frequency.");

        Assert.Matches(@"num_tones\s*=\s*\(wf->protocol\s*==\s*FTX_PROTOCOL_FT4\)\s*\?\s*4\s*:\s*8", find);
        _output.WriteLine("  num_tones is 8 for FT8. FT4 is not this library's and was not read for structure.");
    }

    /// <summary>
    /// WEAK, and this is the one criterion 3 turns on. The candidates are kept in a min-heap ordered
    /// on <c>score</c> and on nothing else, and then heapsorted into descending order.
    /// <b>Heapsort is not stable and the comparison has no tie-break</b>, so where two candidates
    /// share a score, upstream's returned order is whatever the heap's swaps happened to leave —
    /// reproducible for one build over one input, and not a defined function of the input.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheSortIsAHeapsortOnScoreAloneAndIsThereforeNotATotalOrder()
    {
        var source = ReadSource(@"ft8\decode.c");
        var down = ExtractFunctionBody(source, "heapify_down");
        var up = ExtractFunctionBody(source, "heapify_up");
        var find = ExtractFunctionBody(source, "ftx_find_candidates");

        // Every comparison in both heap helpers is on .score, and there are no others.
        var comparisons = Regex.Matches(down + up, @"heap\[\w+\]\.(\w+)\s*<\s*heap\[\w+\]\.(\w+)");
        Assert.NotEmpty(comparisons);
        foreach (Match comparison in comparisons)
        {
            Assert.Equal("score", comparison.Groups[1].Value);
            Assert.Equal("score", comparison.Groups[2].Value);
        }

        _output.WriteLine($"  {comparisons.Count} comparisons in the heap helpers, ALL of them on .score");
        Assert.DoesNotMatch(@"\.time_offset\s*[<>]", down + up);
        Assert.DoesNotMatch(@"\.freq_offset\s*[<>]", down + up);

        // The eviction and the descending sort.
        Assert.Matches(@"heap_size\s*==\s*num_candidates.*candidate\.score\s*>\s*heap\[0\]\.score",
            CollapseWhitespace(find));
        Assert.Matches(@"heapify_down\(heap,\s*len_unsorted\)", find);
        _output.WriteLine("  full heap evicts heap[0], the SMALLEST score, only for a STRICTLY greater one.");
        _output.WriteLine("  the sort swaps root to the end repeatedly: descending by score, heapsort.");

        _output.WriteLine(string.Empty);
        _output.WriteLine("  FINDING, and it is the reason for divergence 19:");
        _output.WriteLine("  upstream's order is NOT A TOTAL ORDER. Scores are small integers and tie");
        _output.WriteLine("  constantly; heapsort is not stable; nothing but .score is ever compared.");
        _output.WriteLine("  Two candidates with equal scores can come back in either order, decided by");
        _output.WriteLine("  the accident of the heap's swaps. Ft8SyncSearch breaks ties explicitly.");
    }

    /// <summary>
    /// WEAKEST. The two numbers that bound the answer are not in the library at all. They are
    /// file-scope constants in the demo program, which means they are one application's judgement
    /// and not a property of FT8.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheMinimumScoreAndCandidateLimitBelongToTheApplicationAndNotTheLibrary()
    {
        var demo = ReadSource(@"demo\decode_ft8.c");

        var minScore = Regex.Match(demo, @"const\s+int\s+kMin_score\s*=\s*(\d+)\s*;");
        var maxCandidates = Regex.Match(demo, @"const\s+int\s+kMax_candidates\s*=\s*(\d+)\s*;");
        Assert.True(minScore.Success, "kMin_score is no longer a constant in the demo.");
        Assert.True(maxCandidates.Success, "kMax_candidates is no longer a constant in the demo.");

        // Printed, not asserted as a literal here: what these have to equal is asserted against the
        // port's own defaults in Ft8SyncSearchProvenanceTests, which is the one place the read and
        // the port could drift apart without anything going red.
        _output.WriteLine($"  kMin_score      = {minScore.Groups[1].Value}");
        _output.WriteLine($"  kMax_candidates = {maxCandidates.Groups[1].Value}");

        // And they are NOT in the library, which is the whole point of the split.
        foreach (var relative in new[] { @"ft8\decode.c", @"ft8\decode.h", @"ft8\constants.h" })
        {
            var library = ReadSource(relative);
            Assert.DoesNotContain("kMin_score", library, StringComparison.Ordinal);
            Assert.DoesNotContain("kMax_candidates", library, StringComparison.Ordinal);
        }

        _output.WriteLine("  kMin_score and kMax_candidates live in demo/decode_ft8.c ONLY.");
        _output.WriteLine("  Neither appears anywhere in ft8/. They are the application's choices.");
        _output.WriteLine(
            "  Ft8SyncSearch therefore exposes MinimumScore and CandidateLimit as constructor");
        _output.WriteLine(
            "  parameters, defaulting to the demo's, so a caller that wants a different sensitivity");
        _output.WriteLine("  does not have to fork the search.");
    }

    /// <summary>
    /// The split itself, printed as one table, because every unit of this phase from 209 onward has
    /// been required to say which of its shapes could have been misread.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheAnchoringOfEachSearchShapeIsReported()
    {
        RequireReachableClone();

        var rows = new (string Shape, string Anchor, string Where)[]
        {
            ("candidate record, five fields, integer score", "STRONG", @"typedef in ft8\decode.h"),
            ("search entry point and its four parameters", "STRONG", @"declaration in ft8\decode.h"),
            ("stored magnitude read as an integer count", "STRONG", @"macro in ft8\decode.h"),
            ("waterfall axis order and block stride", "STRONG", @"struct + comment in ft8\decode.h"),
            ("three sync groups of seven, thirty-six apart", "STRONG", @"macros in ft8\constants.h"),
            ("the seven-tone Costas array", "STRONG", @"extern declaration in ft8\constants.h"),
            ("four neighbour difference terms and their guards", "weak", @"function body, ft8\decode.c"),
            ("skip before the slot, break past the end", "weak", @"function body, ft8\decode.c"),
            ("integer division by the terms actually taken", "weak", @"function body, ft8\decode.c"),
            ("time offsets -10 .. 19", "weak", @"loop literals, ft8\decode.c"),
            ("frequency offsets bounded by the eighth tone", "weak", @"loop condition, ft8\decode.c"),
            ("min-heap on score, then heapsort descending", "weak", @"function bodies, ft8\decode.c"),
            ("minimum score of 10", "WEAKEST", @"application constant, demo\decode_ft8.c"),
            ("candidate limit of 140", "WEAKEST", @"application constant, demo\decode_ft8.c"),
        };

        _output.WriteLine($"{"shape",-48} {"anchor",-8} where");
        foreach (var (shape, anchor, where) in rows)
        {
            _output.WriteLine($"{shape,-48} {anchor,-8} {where}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  strong: {rows.Count(r => r.Anchor == "STRONG")}");
        _output.WriteLine($"  weak:   {rows.Count(r => r.Anchor == "weak")}");
        _output.WriteLine($"  weakest (the application's): {rows.Count(r => r.Anchor == "WEAKEST")}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  UNREAD, and named as unread rather than guessed:");
        _output.WriteLine("  1. What the reference decoder actually returns for a given slot. The binary");
        _output.WriteLine("     is not built on this machine (HM-OPEN-065) and a unit may not build one,");
        _output.WriteLine("     so no candidate list of upstream's was compared against this port's.");
        _output.WriteLine("  2. The exact alignment between a block index and a sample offset. Unit 213");
        _output.WriteLine("     carried this forward unsettled and reading the search does not settle it:");
        _output.WriteLine("     upstream's own time_offset is a block index and its sample meaning is");
        _output.WriteLine("     never written down. Task 4 MEASURES it as a mean signed error instead.");
        _output.WriteLine("  3. Whether upstream's heap order for tied scores is reproducible across");
        _output.WriteLine("     compilers. Not readable from the source, and not needed: this port does");
        _output.WriteLine("     not reproduce it, it replaces it with a defined one.");
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
    /// <c>ft8_sync_score</c> cannot be satisfied by a line in <c>ft4_sync_score</c>. Unit 209 was
    /// caught by exactly that and the habit is kept — and here it matters more than usual, because
    /// the FT4 scorer sitting next door is nearly the same text.
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

        Assert.Fail($"{name} has no closing brace.");
        return string.Empty;
    }

    /// <summary>Pulls one typedef struct's body out the same way, for the same reason.</summary>
    private static string ExtractTypedefBody(string source, string name)
    {
        var end = source.IndexOf($"}} {name};", StringComparison.Ordinal);
        Assert.True(end >= 0, $"{name} is no longer a typedef in the source read.");

        var start = source.LastIndexOf('{', end);
        Assert.True(start >= 0, $"{name}'s typedef has no opening brace.");
        return source[start..end];
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
