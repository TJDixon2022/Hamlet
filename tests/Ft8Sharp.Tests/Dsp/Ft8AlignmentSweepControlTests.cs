using Ft8Sharp.Dsp;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>THE INSTRUMENT PROVED BEFORE IT IS TRUSTED.</b> A probe that has never failed is not a probe,
/// and a probe that is told the answer can be written to find it whether or not it is there.
/// </summary>
/// <remarks>
/// <para>
/// <b>Three checks, and the sweep in <see cref="AlignmentSweep"/> is worth nothing without all
/// three.</b> It must find what <em>is</em> there — a decoding alignment for lines the untold path
/// already matched. It must refuse what is <em>not</em> there — chance agreement and no decode at
/// frequencies where the list says nothing is, with the same true codewords. And it must agree with
/// the instrument this phase already has, <c>Ft8MissAccountingTests</c>, wherever the two overlap.
/// </para>
/// <para>
/// <b>The second check is also criterion 2's</b> — <em>a candidate failing CRC is never returned as
/// a decode</em>. Every quiet-frequency sweep runs the full gate at its most promising points, and
/// the number of messages that come back must be zero.
/// </para>
/// <para>
/// <b>Nothing here is added to any total.</b> These sweeps are told the file, the frequency and the
/// text; that is what makes them able to answer the question and it is exactly why their answers may
/// never be counted. See the remarks on <see cref="AlignmentSweep"/>.
/// </para>
/// </remarks>
public class Ft8AlignmentSweepControlTests
{
    /// <summary>How many recordings the control group is drawn from.</summary>
    private const int ControlRecordings = 6;

    /// <summary>How many matched lines are taken from each of them.</summary>
    private const int ControlLinesPerRecording = 3;

    /// <summary>How far a quiet frequency must sit from every frequency the list names.</summary>
    private const double QuietClearanceHz = 30.0;

    /// <summary>How many quiet frequencies are swept in each control recording.</summary>
    private const int QuietPerRecording = 2;

    private readonly ITestOutputHelper _output;

    public Ft8AlignmentSweepControlTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// <b>Check one: it finds what is there.</b> Lines the untold path already matched, swept the
    /// same way the 78 will be, with the alignment the decode was found at.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheSweepFindsADecodingAlignmentForLinesTheUntoldPathAlreadyMatched()
    {
        var search = new Ft8SyncSearch();
        var found = new List<AlignmentSweep.Outcome>();

        _output.WriteLine("THE NEIGHBOURHOOD, FIXED HERE AND NOT WIDENED AFTERWARDS.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  block offsets      : {search.FirstBlockOffset} to {search.LastBlockOffset} "
            + $"inclusive, which is the search's OWN range - {search.LastBlockOffset - search.FirstBlockOffset + 1} values");
        _output.WriteLine($"  time sub-offsets   : both, 0 and 1");
        _output.WriteLine($"  bins either side   : {AlignmentSweep.BinSpan}, which is "
            + $"{AlignmentSweep.BinSpan} whole FT8 tone spacings - {(2 * AlignmentSweep.BinSpan) + 1} bins");
        _output.WriteLine($"  frequency sub-offs : both, 0 and 1, so the frequency step is half a tone");
        _output.WriteLine($"  POINTS PER LINE    : "
            + $"{(search.LastBlockOffset - search.FirstBlockOffset + 1) * 2 * ((2 * AlignmentSweep.BinSpan) + 1) * 2}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  WHY THIS SPAN. The block range is the search's own, so the sweep can reach");
        _output.WriteLine("  every alignment the search could have proposed and no more. Two bins either");
        _output.WriteLine("  side with both sub-offsets reaches about 15.6 Hz either way, against the four");
        _output.WriteLine("  hertz every previous unit tested at - so a signal the list has placed two");
        _output.WriteLine("  whole tones away is still inside the sweep.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  THE DECODE RULE, STATED BEFORE THE RUN. Scoring and agreement are taken at");
        _output.WriteLine($"  every point; belief propagation is run at the {AlignmentSweep.DecodeBudget} best-agreeing points");
        _output.WriteLine("  and at every point in the neighbourhood the search itself kept, so the sweep");
        _output.WriteLine("  can never miss a decode the untold path could have had.");
        _output.WriteLine(string.Empty);

        foreach (var recording in ReferenceRecordings.WithExpectedLists().Take(ControlRecordings))
        {
            var geometry = new Ft8WaterfallGeometry(recording.SampleRate);
            var waterfall = new Ft8Monitor(geometry).Analyse(recording.ReadSamples());
            var candidates = search.Find(waterfall);

            // THE UNTOLD PATH. Its output is read; it is not filtered, re-ordered or re-scored.
            var returned = new Ft8SlotDecoder(geometry).Decode(waterfall).Texts
                .Select(ReferenceRecording.Normalise)
                .ToHashSet(StringComparer.Ordinal);

            var taken = 0;
            foreach (var line in AlignmentSweep.ExpectedLines(recording))
            {
                if (taken >= ControlLinesPerRecording || !returned.Contains(line.Text))
                {
                    continue;
                }

                var truth = AlignmentSweep.TrueCodeword(line.Text);
                if (truth is null)
                {
                    continue;
                }

                found.Add(AlignmentSweep.Sweep(
                    waterfall, candidates, search, recording.Name, line.Text, line.Snr, line.Hz, truth));
                taken++;
            }
        }

        _output.WriteLine("THE CONTROL GROUP. Expected lines the untold path ALREADY MATCHED, swept the");
        _output.WriteLine("same way the 78 will be. Every number in task 3 is read against this block.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  lines swept                       : {found.Count}");
        _output.WriteLine($"  THE SWEEP FOUND A DECODING POINT  : {found.Count(o => o.Decoded)}");
        _output.WriteLine($"  found none                        : {found.Count(o => !o.Decoded)}");
        _output.WriteLine($"  mean best agreement of 174        : {(found.Count == 0 ? 0 : found.Average(o => o.BestAgreement.Agreement)):F1}");
        _output.WriteLine($"  lowest best agreement             : {(found.Count == 0 ? 0 : found.Min(o => o.BestAgreement.Agreement))}");
        _output.WriteLine($"  mean best sync score              : {(found.Count == 0 ? 0 : found.Average(o => o.BestScore.Score)):F1}");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"file",-22} {"list Hz",8} {"snr",5} {"best agree",11} {"best score",11} "
            + $"{"rank",5} {"decoded at",26}  text");

        foreach (var outcome in found)
        {
            _output.WriteLine($"{outcome.File,-22} {outcome.ListHz,8:F0} {outcome.ListSnr,5:F0} "
                + $"{outcome.BestAgreement.Agreement,4} @{outcome.BestAgreement,-6} "
                + $"{outcome.BestScore.Score,4} @{outcome.BestScore,-6} "
                + $"{(outcome.BestRankInNeighbourhood > 0 ? outcome.BestRankInNeighbourhood.ToString() : "-"),5} "
                + $"{(outcome.DecodedAt?.ToString() ?? "NONE"),26}  {outcome.Text}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  WHERE IN THE NEIGHBOURHOOD THE DECODING POINT SAT, relative to the centre the");
        _output.WriteLine("  list's own frequency put it at:");
        _output.WriteLine($"{"bin offset from centre",26} {"count",6}");
        foreach (var group in found
            .Where(o => o.DecodedAt is not null)
            .GroupBy(o => o.DecodedAt!.Value.Bin - o.CentreBin)
            .OrderBy(g => g.Key))
        {
            _output.WriteLine($"{group.Key,26} {group.Count(),6}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  THE NUMBERS ARE PRINTED BEFORE ANY BOUND IS ASSERTED ON THEM.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  NOTHING FROM THIS SWEEP IS ADDED TO ANY TOTAL. These lines are already");
        _output.WriteLine("  matched by the untold path and are counted there and nowhere else; this");
        _output.WriteLine("  block exists so that a reader can see the instrument answer correctly on a");
        _output.WriteLine("  known answer before he is asked to believe it on an unknown one.");

        Assert.True(found.Count >= 10, $"the control group is {found.Count} lines and it needs at least ten.");

        // THE CONTROL'S OWN ASSERTION, and it is the one that makes task 3 readable: an instrument
        // that cannot find a transmission it KNOWS is there cannot be believed when it says one is
        // not. No bound is put on the agreement figures themselves.
        Assert.True(
            found.Count(o => o.Decoded) == found.Count,
            "the sweep failed to find a decoding alignment for a line the untold path itself decoded: "
            + string.Join("; ", found.Where(o => !o.Decoded).Select(o => o.Row())));
    }

    /// <summary>
    /// <b>Check two: it refuses what is not there.</b> The same true codewords, swept at frequencies
    /// the expected list places nothing within thirty hertz of.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheSweepReturnsChanceAndNoMessageWhereTheListSaysNothingIs()
    {
        var search = new Ft8SyncSearch();
        var quiet = new List<AlignmentSweep.Outcome>();
        var everyPointAgreement = new List<int>();

        foreach (var recording in ReferenceRecordings.WithExpectedLists().Take(ControlRecordings))
        {
            var geometry = new Ft8WaterfallGeometry(recording.SampleRate);
            var waterfall = new Ft8Monitor(geometry).Analyse(recording.ReadSamples());
            var candidates = search.Find(waterfall);

            var lines = AlignmentSweep.ExpectedLines(recording);
            var returned = new Ft8SlotDecoder(geometry).Decode(waterfall).Texts
                .Select(ReferenceRecording.Normalise)
                .ToHashSet(StringComparer.Ordinal);

            // The codewords are real ones - messages this recording genuinely carried and this
            // library genuinely decoded - so the question asked at the quiet frequency is exactly
            // the question asked at a miss: IS THIS MESSAGE HERE.
            var codewords = lines
                .Where(l => returned.Contains(l.Text))
                .Select(l => (l.Text, Truth: AlignmentSweep.TrueCodeword(l.Text)))
                .Where(p => p.Truth is not null)
                .ToList();

            if (codewords.Count == 0)
            {
                continue;
            }

            var taken = 0;
            for (var hz = 400.0; hz <= 2600.0 && taken < QuietPerRecording; hz += 137.0)
            {
                if (lines.Any(l => Math.Abs(l.Hz - hz) < QuietClearanceHz))
                {
                    continue;
                }

                var (text, truth) = codewords[taken % codewords.Count];
                var outcome = AlignmentSweep.Sweep(
                    waterfall, candidates, search, recording.Name, text, double.NaN, hz, truth!);

                quiet.Add(outcome);
                everyPointAgreement.Add(outcome.BestAgreement.Agreement);
                taken++;
            }
        }

        _output.WriteLine("IT REFUSES WHAT IS NOT THERE. The same true codewords, swept the same way, at");
        _output.WriteLine($"frequencies the expected list places nothing within {QuietClearanceHz:F0} Hz of.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  quiet neighbourhoods swept        : {quiet.Count}");
        _output.WriteLine($"  MESSAGES RETURNED                 : {quiet.Count(o => o.Decoded)}"
            + "   <- criterion 2, and it must be zero");
        _output.WriteLine($"  true codewords recovered          : {quiet.Count(o => o.CodewordRecovered)}");
        _output.WriteLine($"  points swept in each              : {(quiet.Count == 0 ? 0 : quiet[0].Points)}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  THE BEST-OF-NEIGHBOURHOOD AGREEMENT, WHICH IS THE STATISTIC TASK 3 READS.");
        _output.WriteLine("  Unit 218 measured chance at ONE point as 84.8 of 174. The best of several");
        _output.WriteLine("  hundred correlated points is a different and HIGHER statistic, and this is");
        _output.WriteLine("  the only honest null distribution for the column task 3 prints.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  mean best agreement               : {(quiet.Count == 0 ? 0 : quiet.Average(o => o.BestAgreement.Agreement)):F1} of 174");
        _output.WriteLine($"  HIGHEST best agreement anywhere   : {(quiet.Count == 0 ? 0 : quiet.Max(o => o.BestAgreement.Agreement))} of 174");
        _output.WriteLine($"  lowest best agreement             : {(quiet.Count == 0 ? 0 : quiet.Min(o => o.BestAgreement.Agreement))} of 174");
        _output.WriteLine($"  mean best sync score              : {(quiet.Count == 0 ? 0 : quiet.Average(o => o.BestScore.Score)):F1}");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"file",-22} {"quiet Hz",9} {"best agree",11} {"best score",11} {"decoded",8}  codeword asked for");
        foreach (var outcome in quiet)
        {
            _output.WriteLine($"{outcome.File,-22} {outcome.ListHz,9:F0} "
                + $"{outcome.BestAgreement.Agreement,4} @{outcome.BestAgreement,-6} "
                + $"{outcome.BestScore.Score,4} @{outcome.BestScore,-6} "
                + $"{(outcome.Decoded ? "YES" : "no"),8}  {outcome.Text}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  THE BOUND TASK 3 USES, FIXED BEFORE THAT RUN: a line with no decoding point");
        _output.WriteLine($"  is called PRESENT AND NOT RECOVERABLE only at a best agreement of");
        _output.WriteLine($"  {AlignmentSweep.PresentButUnrecoverable} of 174 or better. The highest this control reached is printed above.");

        Assert.True(quiet.Count >= 10, $"only {quiet.Count} quiet neighbourhoods were swept and ten are needed.");

        // CRITERION 2, TAKEN AGAIN AND HARDER. Not one of these may return a message.
        Assert.Equal(0, quiet.Count(o => o.Decoded));
        Assert.Equal(0, quiet.Count(o => o.CodewordRecovered));
    }

    /// <summary>
    /// <b>Check three: it agrees with the instrument this phase already has.</b> At the nearest kept
    /// candidate — the one place both instruments read — the two agreement figures must be equal.
    /// </summary>
    /// <remarks>
    /// <b>If they disagree, one of them is wrong and everything downstream depends on which.</b> The
    /// two are separately written: <c>MissAccounting</c> has its own private extraction and its own
    /// private nearest-candidate rule, and <see cref="AlignmentSweep.AgreementAt"/> is this unit's.
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void TheSweepsAgreementMatchesTheExistingInstrumentAtTheNearestKeptCandidate()
    {
        var accounting = MissAccounting.OverTheReferenceRecordings();
        var byFile = accounting.Rows
            .Where(r => r.HasAgreement && r.CandidateWithinFourHz)
            .GroupBy(r => r.File, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

        var search = new Ft8SyncSearch();
        var compared = 0;
        var equal = 0;
        var rows = new List<string>();

        foreach (var recording in ReferenceRecordings.WithExpectedLists())
        {
            if (!byFile.TryGetValue(recording.Name, out var theirs) || compared >= 12)
            {
                continue;
            }

            var geometry = new Ft8WaterfallGeometry(recording.SampleRate);
            var waterfall = new Ft8Monitor(geometry).Analyse(recording.ReadSamples());
            var candidates = search.Find(waterfall);

            foreach (var row in theirs.Take(3))
            {
                var truth = AlignmentSweep.TrueCodeword(row.Text);
                var nearest = SensitivityLadder.NearestTo(candidates, geometry, row.Hz);
                if (truth is null || nearest is null)
                {
                    continue;
                }

                var mine = AlignmentSweep.AgreementAt(waterfall, nearest.Value, truth);
                compared++;
                if (mine == row.Agreement)
                {
                    equal++;
                }

                rows.Add($"{recording.Name,-22} {row.Hz,7:F0} "
                    + $"{row.Agreement,6} {mine,6} {(mine == row.Agreement ? "equal" : "DIFFER"),7}  {row.Text}");
            }
        }

        _output.WriteLine("THE TWO INSTRUMENTS HELD AGAINST EACH OTHER, at the one place both of them");
        _output.WriteLine("read: the nearest kept candidate within four hertz of the list's frequency.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  lines compared : {compared}");
        _output.WriteLine($"  EQUAL          : {equal}");
        _output.WriteLine($"  differing      : {compared - equal}");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"file",-22} {"list Hz",7} {"theirs",6} {"mine",6} {"verdict",7}  text");
        foreach (var row in rows)
        {
            _output.WriteLine(row);
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  Ft8MissAccountingTests has its own private extraction and its own private");
        _output.WriteLine("  nearest-candidate rule; AlignmentSweep.AgreementAt is this unit's, written");
        _output.WriteLine("  separately. Equality is therefore two implementations agreeing rather than");
        _output.WriteLine("  one implementation agreeing with itself.");

        Assert.True(compared >= 5, $"only {compared} lines could be compared and at least five are needed.");
        Assert.Equal(compared, equal);
    }
}
