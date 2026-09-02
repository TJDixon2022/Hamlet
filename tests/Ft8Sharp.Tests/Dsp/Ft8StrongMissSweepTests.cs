using Ft8Sharp.Dsp;
using Ft8Sharp.Tests.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>THE CENTRE OF UNIT 219.</b> Every missed expected line whose own list says it was strong,
/// taken one at a time and asked a single question with three possible answers: <b>is the
/// transmission present in the recording and being lost by this library, is it present and beyond
/// recovery, or is it not present at all as far as this receiver can see?</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>Why per signal, when three units have taken aggregates.</b> Criterion 3 has stood at 760 of
/// 1298 for three units, and every measurement of it has been a count over 1298 lines. A count
/// cannot distinguish <em>this receiver missed a strong signal</em> from <em>the list claims a
/// strong signal that is not there</em> — and unit 218 produced both readings at once and could not
/// choose between them, which is what <c>HM-OPEN-066</c> records. <b>A single signal can choose.</b>
/// </para>
/// <para>
/// <b>THE THREE OUTCOMES ARE FIXED BEFORE THE RUN and the numbers that decide them are fixed with
/// them.</b> A — some alignment point decodes to the expected text. B — no point decodes and
/// agreement peaks at <see cref="AlignmentSweep.PresentButUnrecoverable"/> of 174 or better. C —
/// agreement stays near the null everywhere. The null for the best-of-neighbourhood statistic is
/// not unit 218's one-point figure of 84.8; it is measured by
/// <c>Ft8AlignmentSweepControlTests.TheSweepReturnsChanceAndNoMessageWhereTheListSaysNothingIs</c>
/// over the same 600 points on empty air, and this file's bound sits above the highest that control
/// ever reached.
/// </para>
/// <para>
/// <b>NOTHING HERE IS ADDED TO ANY TOTAL.</b> The sweep is told the file, the frequency and the
/// text. A point at which it recovers the expected text is evidence that the transmission is
/// present and it is nothing else — not a decode, not a match, and no part of criterion 3's 760,
/// which is re-taken only through
/// <c>TheReferenceRecordingsDecodeAgainstUpstreamsOwnExpectedLists</c> unchanged.
/// </para>
/// <para>
/// <b>A line that lands in outcome C is a measurement and not a failure.</b> This file asserts only
/// what must always be true: no point is reported as a decode unless the recovered text equals the
/// expected text exactly, and the outcomes are exhaustive. <b>It prints the whole table before it
/// asserts anything about it</b>, and no expected split is written into it anywhere.
/// </para>
/// </remarks>
public class Ft8StrongMissSweepTests
{
    private readonly ITestOutputHelper _output;

    public Ft8StrongMissSweepTests(ITestOutputHelper output) => _output = output;

    /// <summary><b>The 78.</b> Every missed matchable expected line at 0 dB or better.</summary>
    [RequiresReferenceCloneFact]
    public void TheStrongMissesAreEachPlacedInExactlyOneNamedOutcome()
    {
        var swept = StrongMissSweep.Run(0.0, double.PositiveInfinity);
        Report("THE 78 - MISSED MATCHABLE EXPECTED LINES AT 0 dB OR BETTER", swept);

        _output.WriteLine(string.Empty);
        _output.WriteLine("TASK 4 READING 1 - THE SCORE AT THE TRUE POINT, AGAINST THE SEARCH'S OWN");
        _output.WriteLine("THRESHOLD. If recoverable transmissions are scoring below what the search");
        _output.WriteLine($"keeps, that is the address. DefaultMinimumScore is {Ft8SyncSearch.DefaultMinimumScore} and");
        _output.WriteLine($"DefaultCandidateLimit is {Ft8SyncSearch.DefaultCandidateLimit}. NEITHER IS CHANGED HERE - the park on both");
        _output.WriteLine("is narrowed for READING only, and this is a reading.");
        _output.WriteLine(string.Empty);

        var present = swept.Rows.Where(r => r.Outcome.Verdict is 'A' or 'B').ToList();
        _output.WriteLine($"  outcome A and B lines                     : {present.Count}");
        if (present.Count > 0)
        {
            _output.WriteLine($"  mean best sync score in neighbourhood     : {present.Average(r => r.Outcome.BestScore.Score):F1}");
            _output.WriteLine($"  lowest                                    : {present.Min(r => r.Outcome.BestScore.Score)}");
            _output.WriteLine($"  BELOW DefaultMinimumScore of {Ft8SyncSearch.DefaultMinimumScore}             : "
                + $"{present.Count(r => r.Outcome.BestScore.Score < Ft8SyncSearch.DefaultMinimumScore)}");
            _output.WriteLine($"  the search kept NO point in the sweep     : {present.Count(r => r.Outcome.BestRankInNeighbourhood < 0)}");
            _output.WriteLine(string.Empty);
            _output.WriteLine($"{"file",-22} {"list Hz",8} {"out",4} {"best score",11} {"slot kept min",14} "
                + $"{"slot kept max",14} {"kept here",10} {"best rank",10}");
            foreach (var row in present)
            {
                var slot = swept.Slots[row.File];
                _output.WriteLine($"{row.File,-22} {row.Hz,8:F0} {row.Outcome.Verdict,4} "
                    + $"{row.Outcome.BestScore.Score,11} {slot.LowestKeptScore,14} {slot.HighestKeptScore,14} "
                    + $"{row.Outcome.KeptPointsInNeighbourhood,10} "
                    + $"{(row.Outcome.BestRankInNeighbourhood > 0 ? row.Outcome.BestRankInNeighbourhood.ToString() : "-"),10}");
            }
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("TASK 4 READING 3 - WHERE THE STRONG MISSES SIT IN THE PASSBAND, against the");
        _output.WriteLine("distribution of the lines that matched. If they cluster at one end, that is a");
        _output.WriteLine("property of the passband; if they do not, that is equally useful.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"band, Hz",14} {"the misses",11} {"the matched",12} {"miss share %",13}");
        foreach (var (label, misses, matched) in swept.FrequencyBands())
        {
            var share = misses + matched == 0 ? 0.0 : 100.0 * misses / (misses + matched);
            _output.WriteLine($"{label,14} {misses,11} {matched,12} {share,13:F1}");
        }

        _output.WriteLine($"{"TOTAL",14} {swept.Rows.Count,11} {swept.MatchedFrequencies.Count,12}");

        _output.WriteLine(string.Empty);
        _output.WriteLine("  THE WHOLE TABLE IS PRINTED BEFORE ANYTHING IS ASSERTED ABOUT IT, and no");
        _output.WriteLine("  expected split was written into this test.");

        // The population must be the one unit 218 produced, or tonight is measuring something else.
        Assert.Equal(78, swept.Rows.Count);

        // EXHAUSTIVE AND EXCLUSIVE BY CONSTRUCTION.
        Assert.Equal(
            swept.Rows.Count,
            swept.Rows.Count(r => r.Outcome.Verdict == 'A')
            + swept.Rows.Count(r => r.Outcome.Verdict == 'B')
            + swept.Rows.Count(r => r.Outcome.Verdict == 'C'));

        // THE ONE ASSERTION THAT MUST ALWAYS HOLD: a point is reported as a decode only where the
        // recovered text equalled the expected text exactly. NOTHING is asserted about how many.
        Assert.All(
            swept.Rows.Where(r => r.Outcome.Decoded),
            r => Assert.NotNull(r.Outcome.DecodedAt));
    }

    /// <summary>
    /// <b>Task 5 — the wider population, kept separate.</b> Unit 218 measured 169 matchable missed
    /// lines at −5 dB or better against the 78 at 0 dB or better. The additional lines are swept the
    /// same way and reported on their own, <b>so the two populations never merge into one number.</b>
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheAdditionalLinesBetweenMinusFiveAndZeroAreSweptSeparately()
    {
        var swept = StrongMissSweep.Run(-5.0, 0.0);
        Report("THE ADDITIONAL LINES FROM -5.0 dB UP TO BUT NOT INCLUDING 0.0 dB", swept);

        _output.WriteLine(string.Empty);
        _output.WriteLine("  THESE ARE REPORTED SEPARATELY AND ARE NEVER ADDED TO THE 78's SPLIT.");
        _output.WriteLine("  Unit 218 measured 169 matchable missed lines at -5 dB or better and 78 at");
        _output.WriteLine("  0 dB or better; this population is the difference between the two, so the");
        _output.WriteLine($"  two counts here should sum to 169 - {swept.Rows.Count} plus 78.");

        Assert.NotEmpty(swept.Rows);
        Assert.Equal(
            swept.Rows.Count,
            swept.Rows.Count(r => r.Outcome.Verdict == 'A')
            + swept.Rows.Count(r => r.Outcome.Verdict == 'B')
            + swept.Rows.Count(r => r.Outcome.Verdict == 'C'));
    }

    /// <summary>
    /// <b>Task 4's other two readings</b>, which come off the untold run rather than off the sweep:
    /// the messages each recording returned against <see cref="Ft8SlotDecoder.DefaultMessageLimit"/>,
    /// and the one recording that produces nothing at all.
    /// </summary>
    /// <remarks>
    /// <b>Both are readings and neither changes anything.</b> The message limit is not altered, not
    /// swept and not proposed as a fix.
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void TheMessageLimitAndTheRecordingThatProducesNothingAreRead()
    {
        var search = new Ft8SyncSearch();
        var rows = new List<(string Name, int Expected, int Returned, int Candidates, int BestScore, int Parity)>();

        foreach (var recording in ReferenceRecordings.WithExpectedLists())
        {
            var geometry = new Ft8WaterfallGeometry(recording.SampleRate);
            var waterfall = new Ft8Monitor(geometry).Analyse(recording.ReadSamples());
            var candidates = search.Find(waterfall);
            var result = new Ft8SlotDecoder(geometry).Decode(waterfall);

            rows.Add((
                recording.Name,
                recording.ExpectedCount,
                result.Texts.Count,
                candidates.Count,
                candidates.Count == 0 ? int.MinValue : candidates.Max(c => c.Score),
                result.ParitySatisfiedCount));
        }

        _output.WriteLine("TASK 4 READING 2 - THE MESSAGE LIMIT. Ft8SlotDecoder.DefaultMessageLimit is");
        _output.WriteLine($"{Ft8SlotDecoder.DefaultMessageLimit}, and NOBODY IN THIS PHASE HAS REPORTED HOW MANY MESSAGES ANY SINGLE");
        _output.WriteLine("RECORDING RETURNED AGAINST IT. The strong misses cluster in the 20m_busy files.");
        _output.WriteLine("THIS IS A READING. The limit is not changed, not swept and not proposed as a fix.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  recordings                                : {rows.Count}");
        _output.WriteLine($"  RECORDINGS THAT RETURNED EXACTLY {Ft8SlotDecoder.DefaultMessageLimit}       : "
            + $"{rows.Count(r => r.Returned == Ft8SlotDecoder.DefaultMessageLimit)}");
        _output.WriteLine($"  most messages any recording returned      : {rows.Max(r => r.Returned)}");
        _output.WriteLine($"  most expected lines any list carries      : {rows.Max(r => r.Expected)}");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"file",-22} {"expected",9} {"returned",9} {"candidates",11} {"best score",11} {"parity",7}");
        foreach (var row in rows)
        {
            _output.WriteLine($"{row.Name,-22} {row.Expected,9} {row.Returned,9} {row.Candidates,11} "
                + $"{(row.BestScore == int.MinValue ? "-" : row.BestScore.ToString()),11} {row.Parity,7}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("TASK 4 READING 4 - THE ONE RECORDING THAT PRODUCED NOTHING. Units 216 and 218");
        _output.WriteLine("both report candidates found in 191111_110115.wav and not one reaching parity,");
        _output.WriteLine("across the whole file. A WHOLE FILE RETURNING NOTHING HAS A CAUSE AND NOBODY HAS");
        _output.WriteLine("LOOKED AT IT.");
        _output.WriteLine(string.Empty);

        var silent = rows.FirstOrDefault(r => r.Name == "191111_110115.wav");
        if (silent.Name is not null)
        {
            _output.WriteLine($"  expected lines it carries    : {silent.Expected}");
            _output.WriteLine($"  candidates the search found  : {silent.Candidates}");
            _output.WriteLine($"  BEST SYNC SCORE ANYWHERE     : {silent.BestScore}");
            _output.WriteLine($"  candidates reaching parity   : {silent.Parity}");
            _output.WriteLine($"  messages returned            : {silent.Returned}");
            _output.WriteLine($"  best score in the other 59   : "
                + $"{rows.Where(r => r.Name != silent.Name).Average(r => r.BestScore):F1} mean, "
                + $"{rows.Where(r => r.Name != silent.Name).Min(r => r.BestScore)} lowest");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  THE NUMBERS ARE PRINTED BEFORE ANY BOUND IS ASSERTED ON THEM.");

        // The reading must have found the sixty recordings and the file it names.
        Assert.Equal(60, rows.Count);
        Assert.Contains(rows, r => r.Name == "191111_110115.wav");

        // The limit is a ceiling and nothing may exceed it. NO bound is put on how close anything got.
        Assert.All(rows, r => Assert.True(r.Returned <= Ft8SlotDecoder.DefaultMessageLimit));
    }

    /// <summary>Prints one population's whole table: the counts first, then every row.</summary>
    private void Report(string title, StrongMissSweep.Result swept)
    {
        var a = swept.Rows.Count(r => r.Outcome.Verdict == 'A');
        var b = swept.Rows.Count(r => r.Outcome.Verdict == 'B');
        var c = swept.Rows.Count(r => r.Outcome.Verdict == 'C');

        _output.WriteLine(title);
        _output.WriteLine(string.Empty);
        _output.WriteLine("THE COUNT IN EACH OUTCOME, FIRST, BEFORE ANY INTERPRETATION.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  lines swept                                    : {swept.Rows.Count}");
        _output.WriteLine($"  A  PRESENT AND RECOVERABLE                     : {a}");
        _output.WriteLine($"  B  present and not recoverable                 : {b}");
        _output.WriteLine($"  C  not present as far as this receiver can see : {c}");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  points swept per line                          : {AlignmentSweep.BinSpan * 0 + swept.PointsPerLine}");
        _output.WriteLine($"  belief propagations run in all                 : {swept.Rows.Sum(r => r.Outcome.DecodesRun)}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  AND THE ONE READING THAT CHANGES WHAT AN A MEANS: some expected lists carry");
        _output.WriteLine("  the same message twice, and this library de-duplicates by upstream's own");
        _output.WriteLine("  payload rule. Where the untold path DID return that text for that file, the");
        _output.WriteLine("  line is a repeated expected line and not a lost transmission.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  lines whose text the untold path DID return    : {swept.Rows.Count(r => r.ReturnedByUntoldPath)}");
        _output.WriteLine($"  of those, in outcome A                         : "
            + $"{swept.Rows.Count(r => r.ReturnedByUntoldPath && r.Outcome.Verdict == 'A')}");
        _output.WriteLine($"  OUTCOME A LINES THAT ARE NOT A REPEATED LINE   : "
            + $"{swept.Rows.Count(r => !r.ReturnedByUntoldPath && r.Outcome.Verdict == 'A')}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  Of the outcome A lines, WHETHER THE SEARCH KEPT THE DECODING POINT - which");
        _output.WriteLine("  is what names the stage that lost the transmission:");
        _output.WriteLine($"    the search KEPT it, so the loss is after the search : "
            + $"{swept.Rows.Count(r => r.Outcome.Verdict == 'A' && r.Outcome.RankOfDecodedPoint > 0)}");
        _output.WriteLine($"    the search kept NO candidate there                  : "
            + $"{swept.Rows.Count(r => r.Outcome.Verdict == 'A' && r.Outcome.RankOfDecodedPoint < 0)}");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  best agreement over the population, mean       : "
            + $"{(swept.Rows.Count == 0 ? 0 : swept.Rows.Average(r => r.Outcome.BestAgreement.Agreement)):F1} of 174");
        _output.WriteLine($"  highest best agreement                         : "
            + $"{(swept.Rows.Count == 0 ? 0 : swept.Rows.Max(r => r.Outcome.BestAgreement.Agreement))}");
        _output.WriteLine($"  lowest best agreement                          : "
            + $"{(swept.Rows.Count == 0 ? 0 : swept.Rows.Min(r => r.Outcome.BestAgreement.Agreement))}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  THE DISTRIBUTION OF THAT BEST-OF-NEIGHBOURHOOD AGREEMENT, so the bound can be");
        _output.WriteLine("  SEEN rather than believed. The quiet-frequency control reached 106 to 115 on");
        _output.WriteLine("  empty air over the same 600 points; the bound is at "
            + $"{AlignmentSweep.PresentButUnrecoverable} and was fixed before the run.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"agreement",-12} {"lines",6}");
        for (var low = 100; low < 180; low += 10)
        {
            var count = swept.Rows.Count(r => r.Outcome.BestAgreement.Agreement >= low
                && r.Outcome.BestAgreement.Agreement < low + 10);
            _output.WriteLine($"{$"{low}-{low + 9}",-12} {count,6}{(low == 130 ? "   <- the bound is at the foot of this band" : string.Empty)}");
        }

        var highestC = swept.Rows.Where(r => r.Outcome.Verdict == 'C').ToList();
        var lowestB = swept.Rows.Where(r => r.Outcome.Verdict == 'B').ToList();
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  HIGHEST agreement among the C lines : {(highestC.Count == 0 ? 0 : highestC.Max(r => r.Outcome.BestAgreement.Agreement))}");
        _output.WriteLine($"  LOWEST agreement among the B lines  : {(lowestB.Count == 0 ? 0 : lowestB.Min(r => r.Outcome.BestAgreement.Agreement))}");
        _output.WriteLine("  If those two are adjacent the bound is cutting through a cluster and should be");
        _output.WriteLine("  read with suspicion; if there is a gap between them, it is not.");
        _output.WriteLine(string.Empty);

        _output.WriteLine("EVERY ROW. file, list Hz, list SNR, best agreement and where, best score and");
        _output.WriteLine("where, the rank the search gave any point in the neighbourhood, whether");
        _output.WriteLine("anything decoded, the outcome letter, and the text.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"file",-22} {"listHz",7} {"snr",5} {"agree",5} {"at",-24} {"score",5} {"at",-24} "
            + $"{"rank",5} {"dec",8} {"rpt",4} {"O",1}  text");

        foreach (var row in swept.Rows.OrderByDescending(r => r.Snr).ThenBy(r => r.File, StringComparer.Ordinal))
        {
            var o = row.Outcome;
            _output.WriteLine($"{row.File,-22} {row.Hz,7:F0} {row.Snr,5:F0} "
                + $"{o.BestAgreement.Agreement,5} {o.BestAgreement,-24} "
                + $"{o.BestScore.Score,5} {o.BestScore,-24} "
                + $"{(o.BestRankInNeighbourhood > 0 ? o.BestRankInNeighbourhood.ToString() : "-"),5} "
                + $"{(o.Decoded ? "DECODED" : o.CodewordRecovered ? "codeword" : "no"),8} "
                + $"{(row.ReturnedByUntoldPath ? "yes" : ""),4} {o.Verdict,1}  {row.Text}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("THE RECURRING TEXTS AS A GROUP. A station missed in file after file at the same");
        _output.WriteLine("frequency either recurs in ONE outcome, which is a property with a cause, or it");
        _output.WriteLine("scatters across outcomes, which is a different finding entirely.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"times",6} {"snr range",12} {"outcomes",12}  text");

        var repeats = swept.Rows
            .GroupBy(r => r.Text, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .OrderByDescending(g => g.Count())
            .ToList();

        foreach (var group in repeats)
        {
            var letters = string.Concat(group.Select(r => r.Outcome.Verdict).OrderBy(v => v));
            _output.WriteLine($"{group.Count(),6} {group.Min(r => r.Snr),4:F0} to {group.Max(r => r.Snr),3:F0} "
                + $"{letters,12}  {group.Key}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  texts appearing more than once                 : {repeats.Count}");
        _output.WriteLine($"  lines belonging to one                         : {repeats.Sum(g => g.Count())}");
        _output.WriteLine($"  of those texts, ALL their lines in one outcome : "
            + $"{repeats.Count(g => g.Select(r => r.Outcome.Verdict).Distinct().Count() == 1)}");
        _output.WriteLine($"  scattered across outcomes                      : "
            + $"{repeats.Count(g => g.Select(r => r.Outcome.Verdict).Distinct().Count() > 1)}");

        _output.WriteLine(string.Empty);
        _output.WriteLine("DIVERGENCE 22 - a hand-built candidate whose eighth tone leaves the passband is");
        _output.WriteLine("REFUSED by Ft8SoftSymbols.Extract, where upstream reads past its own array. Any");
        _output.WriteLine("line whose neighbourhood met that refusal, WITH THE TWO NUMBERS FROM ITS MESSAGE:");
        _output.WriteLine(string.Empty);

        var refused = swept.Rows.Where(r => r.Outcome.PassbandRefusal is not null).ToList();
        _output.WriteLine($"  lines that met it : {refused.Count}");
        foreach (var row in refused)
        {
            _output.WriteLine($"    {row.File,-22} {row.Hz,7:F0}  {row.Outcome.PassbandRefusal}  ({row.Text})");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  THAT IS A REPORT AND NOT A FIX. Divergence 22 is a decided divergence and the");
        _output.WriteLine("  lines it touched swept the bins that remained.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  NOTHING FROM THIS SWEEP WAS ADDED TO ANY TOTAL. No expected text, frequency,");
        _output.WriteLine("  count or list reached Ft8SlotDecoder, Ft8SoftSymbols, Ft8SyncSearch or");
        _output.WriteLine("  Ft8CodewordDecoder on any path whose output is counted. The untold run that");
        _output.WriteLine("  produced 'missed' here is the criterion's own, unchanged, and its candidate");
        _output.WriteLine("  list was read rather than filtered.");
    }
}

/// <summary>
/// Runs <see cref="AlignmentSweep"/> over one SNR band of criterion 3's missed expected lines, with
/// each recording's waterfall built once and shared across every line in it.
/// </summary>
/// <remarks>
/// <b>The population is built exactly the way unit 218's strong-miss pass built it</b>, so the 78 are
/// the same 78: the untold path decodes the recording, the expected list is read afterwards, the
/// returned texts are removed from it one for one, and what is left over at or above the floor and
/// not lost to a hash by the list's own writer is the population.
/// </remarks>
internal static class StrongMissSweep
{
    /// <summary>Everything the search kept in one recording, so a score can be read against it.</summary>
    internal sealed record Slot(int Candidates, int LowestKeptScore, int HighestKeptScore);

    /// <summary>One swept missed line.</summary>
    internal sealed record Row(
        string File,
        double Snr,
        string Text,
        double Hz,
        bool ReturnedByUntoldPath,
        AlignmentSweep.Outcome Outcome);

    internal sealed class Result
    {
        internal List<Row> Rows { get; } = new();

        internal Dictionary<string, Slot> Slots { get; } = new(StringComparer.Ordinal);

        /// <summary>The frequencies of the expected lines the untold path DID match.</summary>
        internal List<double> MatchedFrequencies { get; } = new();

        internal int PointsPerLine { get; set; }

        /// <summary>The misses and the matched, counted into 500 Hz bands of the passband.</summary>
        internal IEnumerable<(string Label, int Misses, int Matched)> FrequencyBands()
        {
            for (var low = 0.0; low < 3000.0; low += 500.0)
            {
                var high = low + 500.0;
                yield return (
                    $"{low,4:F0}-{high,4:F0}",
                    Rows.Count(r => r.Hz >= low && r.Hz < high),
                    MatchedFrequencies.Count(f => f >= low && f < high));
            }
        }
    }

    /// <summary>
    /// Sweeps every missed matchable expected line whose list SNR is at or above
    /// <paramref name="floor"/> and below <paramref name="ceiling"/>.
    /// </summary>
    internal static Result Run(double floor, double ceiling)
    {
        var result = new Result();
        var search = new Ft8SyncSearch();

        foreach (var recording in ReferenceRecordings.WithExpectedLists())
        {
            var lines = AlignmentSweep.ExpectedLines(recording);

            // Unit 218's own filter: at or above the floor, and not a line whose callsign the LIST
            // ITSELF lost to a hash, because no receiver could ever match one of those.
            var wanted = lines
                .Where(l => l.Snr >= floor && l.Snr < ceiling)
                .Where(l => ExpectedMessagePacker.TryPack(l.Text, out _)
                    != ExpectedMessagePacker.PackFailure.HashedCallsignLostInTheList)
                .ToList();

            if (wanted.Count == 0)
            {
                continue;
            }

            var geometry = new Ft8WaterfallGeometry(recording.SampleRate);
            var waterfall = new Ft8Monitor(geometry).Analyse(recording.ReadSamples());

            // THE UNTOLD PATH, run exactly as criterion 3 runs it. Everything below this line only
            // reads what it produced.
            var candidates = search.Find(waterfall);
            var returned = new Ft8SlotDecoder(geometry).Decode(waterfall).Texts
                .Select(ReferenceRecording.Normalise)
                .ToList();

            result.Slots[recording.Name] = new Slot(
                candidates.Count,
                candidates.Count == 0 ? 0 : candidates.Min(c => c.Score),
                candidates.Count == 0 ? 0 : candidates.Max(c => c.Score));

            var returnedSet = returned.ToHashSet(StringComparer.Ordinal);

            // THE SAME ONE-FOR-ONE REMOVAL THE CRITERION'S MULTISET COMPARISON MAKES. A list that
            // legitimately carries the same message twice is not satisfied by one decode, so the
            // row that was satisfied is marked rather than only counted.
            var rows = lines.Select(l => (l.Text, l.Hz, l.Snr, Matched: false)).ToArray();
            foreach (var text in returned)
            {
                for (var i = 0; i < rows.Length; i++)
                {
                    if (!rows[i].Matched && string.Equals(rows[i].Text, text, StringComparison.Ordinal))
                    {
                        rows[i].Matched = true;
                        break;
                    }
                }
            }

            // The matched side of task 4's frequency distribution, read off the same run.
            foreach (var row in rows.Where(r => r.Matched))
            {
                result.MatchedFrequencies.Add(row.Hz);
            }

            foreach (var line in wanted)
            {
                var at = Array.FindIndex(
                    rows,
                    r => !r.Matched
                        && string.Equals(r.Text, line.Text, StringComparison.Ordinal)
                        && r.Hz == line.Hz
                        && r.Snr == line.Snr);
                if (at < 0)
                {
                    continue;
                }

                rows[at].Matched = true;

                var truth = AlignmentSweep.TrueCodeword(line.Text);
                if (truth is null)
                {
                    continue;
                }

                var outcome = AlignmentSweep.Sweep(
                    waterfall, candidates, search, recording.Name, line.Text, line.Snr, line.Hz, truth);

                result.PointsPerLine = Math.Max(result.PointsPerLine, outcome.Points);
                result.Rows.Add(new Row(
                    recording.Name, line.Snr, line.Text, line.Hz, returnedSet.Contains(line.Text), outcome));
            }
        }

        return result;
    }
}
