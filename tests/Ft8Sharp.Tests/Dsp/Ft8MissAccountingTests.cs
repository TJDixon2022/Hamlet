using Ft8Sharp.Dsp;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>EVERY MISS GETS EXACTLY ONE NAME.</b> For each of criterion 3's missed expected messages: the
/// file, the text, the frequency the list gives, whether a kept candidate was within four hertz of
/// it, the hard-decision agreement out of 174 at the nearest kept candidate, and the bucket.
/// </summary>
/// <remarks>
/// <para>
/// <b>THE PROHIBITION THIS FILE EXISTS UNDER, WRITTEN WHERE IT CANNOT BE MISSED.</b> This is a
/// diagnostic and it is handed the answer. <b>The answer never reaches the decoder.</b> No expected
/// text, no expected frequency, no expected count and no list is passed to
/// <see cref="Ft8SlotDecoder"/>, <see cref="Ft8SoftSymbols"/>, <see cref="Ft8SyncSearch"/>,
/// <see cref="Ft8CodewordDecoder"/> or any signature they can see — not as a parameter, not as a
/// field, not through a static, not through a test double. The expected text is used to build a
/// comparison codeword and for nothing else. The frequency is used to <em>choose which candidate to
/// look at</em> after the search has already run untold, and the search's own output is not filtered,
/// re-ordered or re-scored by it. <b>A decoder that knows the answer is worthless</b>, and every
/// receive-side number this phase has taken since unit 214 would have to be thrown away with it.
/// </para>
/// <para>
/// <b>The instrument, and why it separates the hypotheses that nothing else could.</b> Take an
/// expected message's text, pack it and encode it with this library's own encoder — proven
/// bit-identical to upstream's tones over 51 of 51 messages in unit 212 — and you have the true
/// 174-bit codeword that was on the air. Extract the ratios at the nearest kept candidate, take their
/// hard decisions, and count how many of the 174 agree. <b>That single number says which of four
/// things happened</b>: about 87 is chance and the candidate is not on that transmission at all;
/// 100 to 150 is a real signal too weak for belief propagation; 165 and above with no decode is
/// extraction working and the decoder failing anyway, which would be a defect with an address.
/// </para>
/// <para>
/// <b>A buckets table cannot be read without the matched distribution beside it</b>, because the
/// whole question is where the boundary between recovered and not recovered actually falls. So the
/// same measurement is taken on a sample of the messages that <em>did</em> come back, and the chance
/// figure is confirmed on a candidate placed where there is no signal at all.
/// </para>
/// </remarks>
public class Ft8MissAccountingTests
{
    private readonly ITestOutputHelper _output;

    public Ft8MissAccountingTests(ITestOutputHelper output) => _output = output;

    /// <summary>Chance agreement: a bit is a bit, so half of 174, rounded.</summary>
    private const int Chance = MissAccounting.Chance;

    /// <summary>
    /// Below this, the candidate is not on the transmission the expected line names — the agreement
    /// is indistinguishable from a coin. <b>H1 in disguise.</b>
    /// </summary>
    private const int NotOnThisSignal = MissAccounting.NotOnThisSignal;

    /// <summary>
    /// At or above this, extraction delivered the transmission cleanly. A miss here is <b>not</b> a
    /// weak signal and would be a defect with an address.
    /// </summary>
    private const int CleanlyPresent = MissAccounting.CleanlyPresent;

    /// <summary>
    /// <b>THE ACCOUNTING.</b> Every missed expected message in exactly one bucket, with the counts
    /// summing to the miss total, the matched control beside it, and the histogram that shows
    /// whether there is a cliff or a slope.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void EveryMissIsAssignedToExactlyOneNamedCause()
    {
        var accounting = MissAccounting.OverTheReferenceRecordings();

        _output.WriteLine("  THE BUCKETS. Mutually exclusive, exhaustive, and assigned in this order so");
        _output.WriteLine("  that a miss with two possible readings takes the one with the stronger evidence.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"bucket",-6} {"count",6}  what it means");
        _output.WriteLine($"{"H1",-6} {accounting.H1,6}  no candidate on that transmission - none within 4 Hz, or");
        _output.WriteLine($"{string.Empty,-6} {string.Empty,6}  one within 4 Hz agreeing below {NotOnThisSignal} of 174, which is chance");
        _output.WriteLine($"{"H2",-6} {accounting.H2,6}  the signal was there and too weak for the code to recover");
        _output.WriteLine($"{"H3",-6} {accounting.H3,6}  RECOVERED, past parity and CRC, AND THE MESSAGE LAYER REFUSED IT");
        _output.WriteLine($"{"H4",-6} {accounting.H4,6}  decoded and matched nothing anyway - de-duplication or the");
        _output.WriteLine($"{string.Empty,-6} {string.Empty,6}  text comparison");
        _output.WriteLine($"{"TOTAL",-6} {accounting.Total,6}  and the miss total is {accounting.Missed}");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"    of which assigned WITHOUT an agreement figure, because the list itself lost");
        _output.WriteLine($"    the callsign to a hash and no true codeword can be built for them:"
            + $"  {accounting.AssignedWithoutAgreement}");

        _output.WriteLine(string.Empty);
        _output.WriteLine("  THE HISTOGRAM. Agreement of the hard decisions with the true codeword, out of");
        _output.WriteLine("  174, at the nearest kept candidate. THE MISSES AND THE MATCHED CONTROL SIDE BY");
        _output.WriteLine("  SIDE, because neither column can be read without the other.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"agreement",-12} {"misses",8} {"matched",9}");
        foreach (var (label, misses, matched) in accounting.Histogram())
        {
            _output.WriteLine($"{label,-12} {misses,8} {matched,9}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  misses with an agreement figure:   {accounting.MissAgreements.Count}"
            + $"   mean {accounting.MeanMissAgreement:F1}");
        _output.WriteLine($"  matched control, sampled:          {accounting.MatchedAgreements.Count}"
            + $"   mean {accounting.MeanMatchedAgreement:F1}");
        _output.WriteLine($"  CHANCE, measured on a candidate placed where there is no signal: "
            + $"{accounting.ChanceAgreement:F1} of 174   (expected {Chance})");

        _output.WriteLine(string.Empty);
        _output.WriteLine("  THE INDICTMENT SET, and it is the one thing here that would be a defect rather");
        _output.WriteLine($"  than a limit: misses where extraction delivered the transmission cleanly, at");
        _output.WriteLine($"  {CleanlyPresent} of 174 or better, and no message came back.");
        _output.WriteLine($"    count: {accounting.CleanlyPresentMisses.Count}");
        foreach (var line in accounting.CleanlyPresentMisses.Take(40))
        {
            _output.WriteLine($"      {line}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  A SAMPLE OF THE ROWS, so the table is not only a set of totals:");
        _output.WriteLine($"{"file",-22} {"Hz",7} {"4Hz",4} {"agree",6} {"bucket",7}  expected text");
        foreach (var row in accounting.SampleRows(30))
        {
            _output.WriteLine($"  {row}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  THE NUMBERS ARE PRINTED BEFORE ANY BOUND IS ASSERTED ON THEM.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  NOTHING IN THIS TEST TOLD THE DECODER ANYTHING. The search ran untold on every");
        _output.WriteLine("  one of the sixty recordings and its candidate list was read, not filtered. The");
        _output.WriteLine("  expected text built a comparison codeword and nothing else. The expected");
        _output.WriteLine("  frequency chose which already-found candidate to look at, after the fact.");

        // Exhaustive and mutually exclusive by construction.
        Assert.Equal(accounting.Missed, accounting.Total);
        Assert.True(accounting.MissAgreements.Count > 0, "not one miss produced an agreement figure.");

        // The control has to separate from the misses or the instrument says nothing.
        Assert.True(
            accounting.MeanMatchedAgreement > accounting.MeanMissAgreement,
            "messages that came back do not agree with their own codewords better than misses do, "
            + "which would mean the instrument is measuring nothing.");

        // Chance is chance. Measured on a candidate placed where there is no transmission at all.
        Assert.InRange(accounting.ChanceAgreement, Chance - 12, Chance + 12);
    }

    /// <summary>
    /// <b>THE SEVEN MESSAGES UNIT 216'S OWN REPORT LOST BETWEEN TWO OF ITS NUMBERS.</b> Its totals
    /// row says 538 missed; its diagnostic three paragraphs later reasons about 531. <b>538 is
    /// right, and the seven are expected lines that appear twice in one list.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Two comparisons, and only one of them is the criterion's.</b> The criterion's own table
    /// compares as a <em>multiset</em>: a list that legitimately carries the same message twice is
    /// not satisfied by one decode. The diagnostic compares by <em>containment</em>: it asks whether
    /// the text was returned at all, so a line repeated in the list is counted as found both times.
    /// Wherever a list repeats a message, containment scores one higher.
    /// </para>
    /// <para>
    /// <b>So the seven are not lost messages and they are not a defect.</b> They are the repeated
    /// lines, and every one of them is a message this library did return — de-duplicated to one, as
    /// upstream's own payload-hash rule requires and as unit 216 proved it does. <b>They are a small
    /// piece of the ceiling rather than a piece of the shortfall</b>, and the accounting puts them in
    /// H4 for exactly that reason.
    /// </para>
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void TheSevenMessagesBetweenUnit216sTwoNumbersAreRepeatedExpectedLines()
    {
        var recordings = ReferenceRecordings.WithExpectedLists();

        var expectedLines = 0;
        var repeatedLines = 0;
        var repeatsThisLibraryReturned = 0;
        var repeats = new List<string>();

        foreach (var recording in recordings)
        {
            var expected = recording.ExpectedMessages();
            expectedLines += expected.Count;

            var groups = expected.GroupBy(t => t, StringComparer.Ordinal).Where(g => g.Count() > 1).ToList();
            if (groups.Count == 0)
            {
                continue;
            }

            var geometry = new Ft8WaterfallGeometry(recording.SampleRate);
            var returned = new Ft8SlotDecoder(geometry).Decode(recording.ReadSamples()).Texts
                .Select(ReferenceRecording.Normalise)
                .ToHashSet(StringComparer.Ordinal);

            foreach (var group in groups)
            {
                var excess = group.Count() - 1;
                repeatedLines += excess;
                var came = returned.Contains(group.Key);
                if (came)
                {
                    repeatsThisLibraryReturned += excess;
                }

                repeats.Add($"{recording.Name,-22} x{group.Count()}  "
                    + $"{(came ? "RETURNED    " : "never came back")}  {group.Key}");
            }
        }

        _output.WriteLine($"  expected lines in all sixty lists:                     {expectedLines}");
        _output.WriteLine($"  lines that are a repeat of another in the SAME list:   {repeatedLines}");
        _output.WriteLine($"  of those, repeats of a message this library RETURNED: {repeatsThisLibraryReturned}");
        _output.WriteLine(string.Empty);
        foreach (var line in repeats)
        {
            _output.WriteLine($"    {line}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  UNIT 216'S TWO NUMBERS RECONCILED:");
        _output.WriteLine("    the totals row, a MULTISET comparison:      538 missed   <- the criterion's own");
        _output.WriteLine("    the diagnostic, a CONTAINMENT comparison:   531 missed");
        _output.WriteLine("    the difference:                             7");
        _output.WriteLine($"    repeats of a message that DID come back:    {repeatsThisLibraryReturned}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  538 IS RIGHT AND THE SEVEN ARE ACCOUNTED FOR. The criterion's table is the");
        _output.WriteLine("  multiset comparison and a list carrying a message twice is not satisfied by one");
        _output.WriteLine("  decode. The diagnostic asks only whether the text came back at all, so where a");
        _output.WriteLine("  repeated line's message DID come back it scores every copy as found.");
        _output.WriteLine($"  There are {repeatedLines} repeated lines in all, and the {repeatsThisLibraryReturned}");
        _output.WriteLine("  whose message came back are exactly the difference. The other repeats never came");
        _output.WriteLine("  back at all, so both comparisons count them as missed and neither disagrees.");
        _output.WriteLine("  None of the seven is a lost message: each was returned and de-duplicated to one");
        _output.WriteLine("  decode by upstream's own payload rule, which unit 216 proved this library keeps.");
        _output.WriteLine("  They are a piece of the ceiling and the accounting puts them in H4.");

        Assert.Equal(7, repeatsThisLibraryReturned);
        Assert.Equal(9, repeatedLines);
    }
}

/// <summary>
/// Puts every one of criterion 3's misses into exactly one bucket, with the evidence that put it
/// there.
/// </summary>
/// <remarks>
/// <b>The order of assignment is the order of evidence strength, and it is what makes the buckets
/// exclusive.</b> A miss whose codeword this library demonstrably recovered and then refused is H3
/// however weak the signal looked; a miss whose text this library demonstrably returned is H4
/// whatever the agreement says; only then does the agreement figure decide between H1 and H2.
/// </remarks>
internal sealed class MissAccounting
{
    /// <summary>Chance agreement: a bit is a bit, so half of 174, rounded.</summary>
    internal const int Chance = 87;

    /// <summary>
    /// Below this, the candidate is not on the transmission the expected line names: the agreement
    /// is indistinguishable from a coin. <b>H1 in disguise.</b>
    /// </summary>
    internal const int NotOnThisSignal = 100;

    /// <summary>
    /// At or above this, extraction delivered the transmission cleanly, so a miss here is not a weak
    /// signal and would be a defect with an address.
    /// </summary>
    internal const int CleanlyPresent = 165;

    private readonly List<Row> _rows = new();
    private readonly List<int> _matched = new();

    internal int Missed { get; private set; }

    internal double ChanceAgreement { get; private set; }

    /// <summary>One missed expected message and everything measured about it.</summary>
    internal readonly record struct Row(
        string File,
        string Text,
        double Hz,
        bool CandidateWithinFourHz,
        int Agreement,
        string Bucket)
    {
        /// <summary>-1 where no true codeword could be built, so no agreement exists.</summary>
        internal bool HasAgreement => Agreement >= 0;

        public override string ToString() =>
            $"{File,-22} {Hz,7:F0} {(CandidateWithinFourHz ? "yes" : "no"),4} "
            + $"{(HasAgreement ? Agreement.ToString() : "-"),6} {Bucket,7}  {Text}";
    }

    internal int H1 => _rows.Count(r => r.Bucket == "H1");

    internal int H2 => _rows.Count(r => r.Bucket == "H2");

    internal int H3 => _rows.Count(r => r.Bucket == "H3");

    internal int H4 => _rows.Count(r => r.Bucket == "H4");

    internal int Total => _rows.Count;

    internal int AssignedWithoutAgreement => _rows.Count(r => !r.HasAgreement);

    internal IReadOnlyList<int> MissAgreements => _rows.Where(r => r.HasAgreement).Select(r => r.Agreement).ToList();

    internal IReadOnlyList<int> MatchedAgreements => _matched;

    internal double MeanMissAgreement =>
        MissAgreements.Count == 0 ? 0 : MissAgreements.Average();

    internal double MeanMatchedAgreement => _matched.Count == 0 ? 0 : _matched.Average();

    internal IReadOnlyList<string> CleanlyPresentMisses =>
        _rows.Where(r => r.HasAgreement && r.Agreement >= CleanlyPresent).Select(r => r.ToString()).ToList();

    internal IEnumerable<string> SampleRows(int count) =>
        _rows.Where((_, i) => i % Math.Max(1, _rows.Count / Math.Max(1, count)) == 0)
            .Take(count)
            .Select(r => r.ToString());

    /// <summary>The distribution, in bands, misses and matched control side by side.</summary>
    internal IEnumerable<(string Label, int Misses, int Matched)> Histogram()
    {
        var edges = new[] { 0, 80, 90, 100, 110, 120, 130, 140, 150, 160, 165, 170, 174, 175 };
        for (var i = 0; i + 1 < edges.Length; i++)
        {
            var lo = edges[i];
            var hi = edges[i + 1];
            var label = hi - lo == 1 ? $"{lo}" : $"{lo}-{hi - 1}";
            yield return (
                label,
                MissAgreements.Count(a => a >= lo && a < hi),
                _matched.Count(a => a >= lo && a < hi));
        }
    }

    internal static MissAccounting OverTheReferenceRecordings()
    {
        var accounting = new MissAccounting();
        var census = RefusalCensus.OverTheReferenceRecordings();

        foreach (var recording in ReferenceRecordings.WithExpectedLists())
        {
            var geometry = new Ft8WaterfallGeometry(recording.SampleRate);
            var waterfall = new Ft8Monitor(geometry).Analyse(recording.ReadSamples());

            // THE UNTOLD PATH, run exactly as criterion 3 runs it. Nothing below this line is told
            // anything; what follows only reads what it produced.
            var candidates = new Ft8SyncSearch().Find(waterfall);
            var result = new Ft8SlotDecoder(geometry).Decode(waterfall);
            var returned = result.Texts.Select(ReferenceRecording.Normalise).ToList();

            // The same multiset comparison the criterion's own test makes, so "missed" here is
            // exactly what "missed" is there.
            var outstanding = new List<string>(recording.ExpectedMessages());
            var alsoReturned = new HashSet<string>(StringComparer.Ordinal);
            foreach (var text in returned)
            {
                alsoReturned.Add(text);
                var at = outstanding.FindIndex(e => string.Equals(e, text, StringComparison.Ordinal));
                if (at >= 0)
                {
                    outstanding.RemoveAt(at);
                }
            }

            // What this file's refusals were, so H3 can be decided on evidence rather than inferred.
            var refusedHere = census.Refusals.Where(r => r.File == recording.Name).ToList();
            var refusedRenderings = refusedHere
                .Select(RefusalCensus.UpstreamWouldPrint)
                .Where(t => t is not null)
                .Select(t => t!)
                .ToHashSet(StringComparer.Ordinal);

            var frequencies = FrequenciesByText(recording);

            foreach (var text in outstanding)
            {
                accounting.Missed++;
                var hz = frequencies.TryGetValue(text, out var f) ? f : double.NaN;

                var nearest = Nearest(candidates, geometry, hz, out var withinFour);
                var agreement = Agreement(text, waterfall, nearest);

                var bucket = Bucket(
                    text, agreement, withinFour, alsoReturned, refusedHere, refusedRenderings);

                accounting._rows.Add(new Row(recording.Name, text, hz, withinFour, agreement, bucket));
            }

            // The control: a sample of the messages that DID come back, measured the same way.
            foreach (var text in returned.Where((_, i) => i % 3 == 0))
            {
                if (!frequencies.TryGetValue(text, out var hz))
                {
                    continue;
                }

                var nearest = Nearest(candidates, geometry, hz, out _);
                var agreement = Agreement(text, waterfall, nearest);
                if (agreement >= 0)
                {
                    accounting._matched.Add(agreement);
                }
            }

            // AND CHANCE, MEASURED RATHER THAN ASSUMED. A message this library really did decode
            // from this slot, compared against the ratios extracted at a place the search did not
            // keep: the lowest bin of the first block, which is not a candidate on any of these
            // sixty recordings. If the instrument is measuring anything, this lands near 87.
            var elsewhere = new Ft8Candidate(0, 0, 0, 0, 0);
            foreach (var text in returned.Take(1))
            {
                var agreement = Agreement(text, waterfall, elsewhere);
                if (agreement >= 0)
                {
                    accounting._chanceSamples.Add(agreement);
                }
            }
        }

        accounting.ChanceAgreement =
            accounting._chanceSamples.Count == 0 ? 0 : accounting._chanceSamples.Average();

        return accounting;
    }

    private readonly List<int> _chanceSamples = new();

    /// <summary>
    /// Which bucket, decided in order of evidence strength so that the four are exclusive.
    /// </summary>
    private static string Bucket(
        string text,
        int agreement,
        bool withinFour,
        HashSet<string> alsoReturned,
        List<RefusalCensus.Refusal> refusedHere,
        HashSet<string> refusedRenderings)
    {
        // H3 FIRST, because it is the only bucket decided by direct evidence rather than by a
        // threshold: the codeword was recovered, it passed parity and CRC, and the message layer
        // refused it. Two routes to that evidence, and both are exact.
        if (refusedRenderings.Contains(text))
        {
            return "H3";
        }

        if (ExpectedMessagePacker.TryPack(text, out var packed) == ExpectedMessagePacker.PackFailure.None
            && refusedHere.Any(r => ExpectedMessagePacker.SameMessage(r.Message, packed)))
        {
            return "H3";
        }

        // H4 NEXT: this library did return that text for this file, so the miss is a de-duplication
        // or comparison artifact and not a failure to recover anything.
        if (alsoReturned.Contains(text))
        {
            return "H4";
        }

        // Now the agreement decides, and where there is none the candidate test decides alone.
        if (!withinFour)
        {
            return "H1";
        }

        if (agreement < 0)
        {
            // No true codeword could be built - the list lost the callsign - so the only evidence
            // is that a candidate was there. Assigned to H2 and counted separately in the report,
            // because a candidate at the right frequency that produced nothing is what H2 says.
            return "H2";
        }

        return agreement < NotOnThisSignal ? "H1" : "H2";
    }

    /// <summary>
    /// The hard-decision agreement out of 174 between the ratios extracted at one candidate and the
    /// true codeword the expected text encodes to. -1 where no true codeword can be built.
    /// </summary>
    private static int Agreement(string text, Ft8Waterfall waterfall, Ft8Candidate? candidate)
    {
        if (candidate is null)
        {
            return -1;
        }

        if (ExpectedMessagePacker.TryPack(text, out var message) != ExpectedMessagePacker.PackFailure.None)
        {
            return -1;
        }

        Span<byte> payload = stackalloc byte[Ft8Payload.PayloadBytes];
        Ft8Payload.Create(message, payload);

        Span<byte> codeword = stackalloc byte[LdpcEncoder.CodewordBytes];
        LdpcEncoder.Encode(payload, codeword);

        var ratios = new float[Ft8SoftSymbols.RatioCount];
        Ft8SoftSymbols.Extract(waterfall, candidate.Value, ratios);
        Ft8SoftSymbols.Normalise(ratios);

        var decisions = new byte[Ft8SoftSymbols.RatioCount];
        Ft8SoftSymbols.HardDecision(ratios, decisions);

        var agree = 0;
        for (var bit = 0; bit < decisions.Length; bit++)
        {
            var truth = (codeword[bit / 8] >> (7 - (bit % 8))) & 1;
            if (decisions[bit] == truth)
            {
                agree++;
            }
        }

        return agree;
    }

    private static Ft8Candidate? Nearest(
        IReadOnlyList<Ft8Candidate> candidates,
        Ft8WaterfallGeometry geometry,
        double hz,
        out bool withinFourHz)
    {
        withinFourHz = false;
        if (double.IsNaN(hz))
        {
            return null;
        }

        Ft8Candidate? best = null;
        var bestDistance = double.MaxValue;

        foreach (var candidate in candidates)
        {
            var distance = Math.Abs(candidate.FrequencyHz(geometry) - hz);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = candidate;
            }
        }

        withinFourHz = bestDistance < 4.0;
        return best;
    }

    /// <summary>
    /// The frequency each expected line gives, read out of the list's own fourth column. Read only
    /// after the search has run, and used only to choose which already-found candidate to look at.
    /// </summary>
    private static Dictionary<string, double> FrequenciesByText(ReferenceRecording recording)
    {
        var map = new Dictionary<string, double>(StringComparer.Ordinal);

        foreach (var raw in File.ReadAllLines(recording.ExpectedPath))
        {
            var tilde = raw.IndexOf('~');
            if (tilde < 0)
            {
                continue;
            }

            var fields = raw[..tilde].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length < 4 || !double.TryParse(fields[3], out var hz))
            {
                continue;
            }

            map[ReferenceRecording.Normalise(raw[(tilde + 1)..])] = hz;
        }

        return map;
    }
}
