using System.Diagnostics;
using Ft8Sharp.Deep;
using Ft8Sharp.Dsp;
using Ft8Sharp.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>THE FOUR COLUMNS THAT READ <c>not bracketed</c> AT THE CELL CENTRE, BRACKETED.</b>
/// <c>Ft8Sharp</c>, <c>Deep all off</c>, <c>OSD only</c> and <c>subtraction only</c> — and
/// nobody in this project has ever been able to say what ratio the bare port needs at the centre
/// of a coarse cell to hear half of what is sent.
/// </summary>
/// <remarks>
/// <para>
/// <b>THIS IS A MEASUREMENT AND NOT A TEST, AND IT IS NOT IN THE GATE SET.</b>
/// <c>docs/gate-set.md</c> line 57 rules that the ladder is called when a step needs a number and
/// is never a gate-set entry, and rule 5 forbids adding a test that names no breakage.
/// <b>No red is manufactured for a walk.</b> Unit 256's one watched failure is
/// <c>Ft8Unit256CrossingIntervalTests</c> and this class is not it.
/// </para>
/// <para>
/// <b>WHY FOUR COLUMNS AND NOT SIX.</b> <c>docs/unit255-closing-measurement.md</c> §4.1's
/// cell-centre table marks four rows <c>not bracketed</c> — <c>Ft8Sharp</c>, <c>Deep all off</c>,
/// <c>OSD only</c> and <c>subtraction only</c>, all below 50 per cent at -19, -20 and -21 dB, so
/// their crossings lie <em>above</em> -19 dB. <c>fine sync only</c> and <c>SHIPPING</c> already
/// have crossings there, -19.61 dB both, and they are the two dearest columns on the board at
/// about 200 ms a trial each. <b>Re-running them would cost half the night to reproduce two
/// numbers this unit does not need.</b>
/// </para>
/// <para>
/// <b>THE SEARCH CLIMBS AND IT IS CAPPED AT -9 dB.</b> Coarse rungs at 51 trials — one whole
/// block of the 51-message population, whose Wilson intervals are wide, and that is the point of
/// doing it first and cheaply — then the two rungs that straddle 50 per cent at the full 306.
/// <b>A column that has not reached 50 per cent by -9 dB is reported
/// <c>not bracketed - above -9 dB</c> with its rates, and the search is said to have been capped
/// rather than exhausted.</b> Nothing is extrapolated.
/// </para>
/// <para>
/// <b>THE PLACEMENT IS UNIT 248'S AND NO OTHER.</b> <c>+1.56 Hz</c> and <c>+480 samples</c>,
/// transcribed from <c>Ft8Unit255ClosingLadderTests</c> <c>:62</c> and <c>:64</c>, so tonight's
/// rows are comparable with §3.2's.
/// </para>
/// <para>
/// <b>AND NO BOUND IS ASSERTED ON ANY RATE.</b> Targets are waypoints and a step closes on the
/// figure it reached. The two assertions are zero wrong on every row, and the attribution column
/// equal to the port.
/// </para>
/// </remarks>
public class Ft8Unit256CellCentreBracketTests(ITestOutputHelper output)
{
    /// <summary>306 trials: six whole blocks of the 51-message population.</summary>
    private const int FullTrials = 306;

    /// <summary>51 trials: <b>one whole block</b>, and the coarse search's whole cost.</summary>
    private const int CoarseTrials = 51;

    /// <summary>FT8's whole slot, which every worst-slot margin below is quoted against.</summary>
    private const double SlotBudgetMilliseconds = 15_000.0;

    /// <summary>The rate a crossing is defined at, in per cent.</summary>
    private const double HalfPerCent = 50.0;

    /// <summary>
    /// <b>Unit 248's cell centre</b>, transcribed from <c>Ft8Unit255ClosingLadderTests:62</c> and
    /// <c>:64</c>. <b>These two numbers and no others.</b>
    /// </summary>
    private const double CellCentreFrequencyOffsetHz = 1.56;

    private const int CellCentreOffsetSamples = 480;

    /// <summary>
    /// <b>The coarse ladder of unit 256's ruling 3</b>, climbing, capped at -9 dB.
    /// </summary>
    private static readonly double[] CoarseRungs = [-17.0, -15.0, -13.0, -11.0, -9.0];

    /// <summary>
    /// <b>Where every column already sits at -19 dB at the cell centre, at 306 trials.</b> Read
    /// out of <c>docs/unit255-runs/minus19-cell-centre.txt</c> and not re-run.
    /// </summary>
    /// <remarks>
    /// <b>This is what makes the coarse search a climb rather than a two-sided hunt.</b> All four
    /// columns are already known to be far below 50 per cent at -19 dB — 6, 6, 33 and 6 of 306 —
    /// so the search only ever has to find the first rung at which each is above it. The stop rule
    /// is seeded from these counts and says so on the face of the printed report.
    /// </remarks>
    private static readonly (string Column, int Decoded)[] KnownAtMinus19 =
    [
        ("Ft8Sharp", 6),
        ("Deep all off", 6),
        ("OSD only", 33),
        ("subtraction only", 6),
    ];

    /// <summary>
    /// <b>THE UPPER RUNG OF THE BRACKET — the less negative of the two, above 50 per cent.</b>
    /// </summary>
    /// <remarks>
    /// <b>SET FROM THE COARSE SEARCH</b> of
    /// <see cref="TheCoarseSearchForFiftyPerCentAtTheCellCentre"/>, whose printed table is
    /// committed at <c>docs/unit256-runs/cell-centre-coarse.txt</c>, and stated in
    /// <c>docs/unit256-crossings-and-combining.md</c>. It is not carried over from any earlier
    /// unit.
    /// <para>
    /// <b>The coarse search stopped at its very first rung.</b> At -17 dB and 51 trials
    /// <c>Ft8Sharp</c>, <c>Deep all off</c> and <c>subtraction only</c> all read <b>41 of 51,
    /// 80.4 per cent</b> and <c>OSD only</c> read <b>51 of 51</b> — every column above 50 per cent
    /// at the first step, against 6, 6, 6 and 33 of 306 at -19 dB. The remaining coarse rungs
    /// -15, -13, -11 and -9 were never walked.
    /// </para>
    /// <para>
    /// <b>The refinement rung at -18 dB is what makes this pair one decibel apart</b>: 14 of 51,
    /// 27.5 per cent, for the three port-equal columns — below 50 — so <b>(-18, -17) is the
    /// straddling pair for three of the four columns</b>, which is the most of any pair, and it is
    /// the pair chosen.
    /// </para>
    /// </remarks>
    private const double UpperRungDecibels = -17.0;

    /// <summary>
    /// <b>THE LOWER RUNG OF THE BRACKET — the more negative of the two, below 50 per cent.</b>
    /// One decibel below <see cref="UpperRungDecibels"/>, so the linear interpolation the band is
    /// built on spans the same one decibel every crossing in this project spans.
    /// </summary>
    /// <remarks>
    /// <b>AND IT IS ALSO THE UPPER RUNG OF <c>OSD only</c>'s OWN BRACKET.</b> The coarse search
    /// put <c>OSD only</c> at <b>30 of 51, 58.8 per cent</b> here — <em>above</em> 50, unlike the
    /// other three — so its crossing lies between -19 and -18 dB rather than between -18 and -17.
    /// <b>Its lower rung is unit 255's own -19 dB cell-centre row, 33 of 306</b>, measured at the
    /// same placement, the same seed and the same <c>Ft8Sharp.Deep</c> 0.8.0
    /// (<c>docs/unit255-runs/minus19-cell-centre.txt</c>). <b>So all four columns are bracketed
    /// by rungs measured at the full 306 trials and none is left to a 51-trial rung</b> — which is
    /// why unit 256's named drop candidate was not taken.
    /// </remarks>
    private const double LowerRungDecibels = -18.0;

    /// <summary>One column, with the worst single slot it was ever seen to take.</summary>
    private sealed class Column(string name, Func<float[], Ft8SlotResult> decode)
    {
        internal string Name { get; } = name;

        internal double WorstSlotMilliseconds { get; private set; }

        internal int WorstSlotCandidates { get; private set; }

        internal Ft8SlotResult Run(float[] samples)
        {
            var clock = Stopwatch.StartNew();
            var result = decode(samples);
            clock.Stop();

            if (clock.Elapsed.TotalMilliseconds > WorstSlotMilliseconds)
            {
                WorstSlotMilliseconds = clock.Elapsed.TotalMilliseconds;
                WorstSlotCandidates = result.CandidateCount;
            }

            return result;
        }
    }

    /// <summary>
    /// <b>The four columns, each one call to the constructor at
    /// <c>src/Ft8Sharp.Deep/Ft8DeepSlotDecoder.cs:76</c></b>, transcribed from
    /// <c>Ft8Unit255ClosingLadderTests:106–113</c> so they are the same instruments §3.2 measured.
    /// </summary>
    private static Column[] Columns()
    {
        var port = new Ft8SlotDecoder();
        var allOff = new Ft8DeepSlotDecoder();
        var osd = new Ft8DeepSlotDecoder(osd: Ft8DeepOsdSettings.Default);
        var subtraction = new Ft8DeepSlotDecoder(subtraction: Ft8DeepSubtractionSettings.Default);

        return
        [
            new Column("Ft8Sharp", samples => port.Decode(samples)),
            new Column("Deep all off", samples => allOff.Decode(samples)),
            new Column("OSD only", samples => osd.Decode(samples)),
            new Column("subtraction only", samples => subtraction.Decode(samples)),
        ];
    }

    /// <summary>
    /// <b>Where a walk's report is written, as well as to the test output.</b> VSTest discards
    /// <c>ITestOutputHelper</c> for a test that PASSES and it cost unit 255 a four-minute call.
    /// </summary>
    private static string RunLogPath(string stem)
    {
        var folder = Path.Combine(Ft8CaptureFixtures.RepositoryRoot(), "docs", "unit256-runs");
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, stem + ".txt");
    }

    /// <summary>Walks one rung at the cell centre through all four columns.</summary>
    private static (IReadOnlyList<Ft8LadderHarness.Result> Results, Column[] Columns, TimeSpan Elapsed)
        WalkOneRung(double rung, int trials)
    {
        var columns = Columns();
        var decoders = columns
            .Select(column => new Ft8LadderHarness.Decoder(column.Name, column.Run))
            .ToArray();

        var clock = Stopwatch.StartNew();
        var results = Ft8LadderHarness.Run(
            rung,
            trials,
            decoders: decoders,
            frequencyHz: Ft8LadderHarness.DefaultFrequencyHz + CellCentreFrequencyOffsetHz,
            offsetSamples: Ft8LadderHarness.DefaultOffsetSamples + CellCentreOffsetSamples);
        clock.Stop();

        return (results, columns, clock.Elapsed);
    }

    /// <summary>The placement, said the same way on every report this class writes.</summary>
    private static string PlacementLine =>
        $"PLACEMENT: CELL CENTRE - +{CellCentreFrequencyOffsetHz:F2} Hz, "
        + $"+{CellCentreOffsetSamples} samples, from Ft8Unit255ClosingLadderTests :62 and :64, "
        + "originally unit 248's. These two numbers and no others.";

    /// <summary>
    /// <b>THE COARSE SEARCH: 51 trials a rung, climbing from -17 dB, capped at -9 dB.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>It stops as soon as every column has been seen on both sides of 50 per cent.</b> The
    /// below-50 side is already known at 306 trials from
    /// <c>docs/unit255-runs/minus19-cell-centre.txt</c> — 6, 6, 33 and 6 of 306 — so the climb is
    /// only ever looking for the first rung above.
    /// </para>
    /// <para>
    /// <b>And it refines to one decibel.</b> The coarse ladder steps two decibels at a time, but
    /// the 306-trial pair that follows must be one decibel apart, so once the climb has localised
    /// the crossing to a two-decibel gap it probes the single integer rung inside that gap at the
    /// same 51 trials. <b>That refinement costs one more block and it is what lets the bracket be
    /// stated to the decibel</b> rather than to the two decibels the coarse ladder steps in.
    /// </para>
    /// <para>
    /// <b>A 51-trial rate is never quoted as a crossing.</b> Everything here localises; nothing
    /// here is published as a crossing without the 306-trial rungs that follow it.
    /// </para>
    /// </remarks>
    [Fact]
    public void TheCoarseSearchForFiftyPerCentAtTheCellCentre()
    {
        var lines = new List<string>();

        void Say(string line)
        {
            lines.Add(line);
            output.WriteLine(line);
        }

        Say(
            $"UNIT 256, TASK 3, THE COARSE SEARCH: {CoarseTrials} trials a rung - ONE WHOLE BLOCK "
            + "of the 51-message population - climbing -17, -15, -13, -11, -9 dB, CAPPED at -9.");
        Say(PlacementLine);
        Say(
            "COLUMNS: the four that read \"not bracketed\" in unit 255 section 4.1's cell-centre "
            + "table. fine sync only and SHIPPING already cross at -19.61 dB there and are not "
            + "re-run.");
        Say(
            "THE BELOW-50 SIDE IS ALREADY KNOWN, at 306 trials, from "
            + "docs/unit255-runs/minus19-cell-centre.txt: "
            + string.Join(
                ", ",
                KnownAtMinus19.Select(k => $"{k.Column} {k.Decoded} of 306"))
            + " - all far below 50 per cent, so the crossings lie ABOVE -19 dB and this search "
            + "only has to find the first rung above.");
        Say(
            "51 TRIALS IS ONE BLOCK AND ITS WILSON INTERVALS ARE WIDE. That is the point of doing "
            + "it first and cheaply. NO RATE HERE IS QUOTED AS A CROSSING.");
        Say(string.Empty);

        var seenAbove = new Dictionary<string, double>();
        var wholeClock = Stopwatch.StartNew();
        var walked = new List<double>();
        double? firstRungAllAbove = null;

        foreach (var rung in CoarseRungs)
        {
            var (results, columns, elapsed) = WalkOneRung(rung, CoarseTrials);
            walked.Add(rung);

            Say($"--- COARSE RUNG {rung:F1} dB, {CoarseTrials} trials, {elapsed.TotalSeconds:F1} s");
            Say(Ft8LadderHarness.Header);

            foreach (var result in results)
            {
                Say(result.AsRow());

                if (result.Rate > HalfPerCent)
                {
                    seenAbove.TryAdd(result.Decoder, rung);
                }
            }

            Say(
                "  worst slots: "
                + string.Join(
                    ", ",
                    columns.Select(c =>
                        $"{c.Name} {c.WorstSlotMilliseconds:F1} ms "
                        + $"({SlotBudgetMilliseconds / c.WorstSlotMilliseconds:F0}x)")));

            foreach (var result in results)
            {
                foreach (var wrong in result.WrongReturns)
                {
                    Say($"WRONG on {result.Decoder} at {rung:F1} dB: {wrong}");
                }
            }

            Say(string.Empty);

            if (seenAbove.Count == KnownAtMinus19.Length)
            {
                firstRungAllAbove = rung;
                Say(
                    $"STOPPING EARLY at {rung:F1} dB: every column has now been seen ABOVE 50 per "
                    + "cent here and BELOW it at -19 dB. The remaining coarse rungs are not "
                    + "walked.");
                Say(string.Empty);
                break;
            }
        }

        // THE REFINEMENT TO ONE DECIBEL. The coarse ladder steps two decibels; the 306-trial pair
        // must be one apart. Probe the integer rung inside the gap the climb has localised.
        var previousBelow = walked.Count > 1 ? walked[^2] : -19.0;
        var refined = new List<(double Rung, IReadOnlyList<Ft8LadderHarness.Result> Results)>();

        if (firstRungAllAbove is { } crossed && Math.Abs(crossed - previousBelow) > 1.5)
        {
            var middle = Math.Round((crossed + previousBelow) / 2.0);

            Say(
                $"--- REFINEMENT RUNG {middle:F1} dB, {CoarseTrials} trials: the climb localised "
                + $"every crossing to ({previousBelow:F1}, {crossed:F1}), a TWO decibel gap, and "
                + "the 306-trial pair must be ONE decibel apart.");

            var (results, columns, elapsed) = WalkOneRung(middle, CoarseTrials);
            refined.Add((middle, results));

            Say($"    {elapsed.TotalSeconds:F1} s");
            Say(Ft8LadderHarness.Header);

            foreach (var result in results)
            {
                Say(result.AsRow());
            }

            Say(
                "  worst slots: "
                + string.Join(
                    ", ",
                    columns.Select(c =>
                        $"{c.Name} {c.WorstSlotMilliseconds:F1} ms "
                        + $"({SlotBudgetMilliseconds / c.WorstSlotMilliseconds:F0}x)")));

            foreach (var result in results)
            {
                foreach (var wrong in result.WrongReturns)
                {
                    Say($"WRONG on {result.Decoder} at {middle:F1} dB: {wrong}");
                }
            }

            Say(string.Empty);
        }

        wholeClock.Stop();

        Say("WHERE EACH COLUMN WAS FIRST SEEN ABOVE 50 PER CENT, at 51 trials:");

        foreach (var (column, decoded) in KnownAtMinus19)
        {
            var where = seenAbove.TryGetValue(column, out var rung)
                ? $"{rung:F1} dB"
                : $"NOWHERE AT OR BELOW -9 dB - not bracketed - above -9 dB. The search was "
                    + "CAPPED at -9 dB by ruling 3, not exhausted.";

            Say($"  {column,-20} known {decoded,3} of 306 at -19 dB   first above 50 %: {where}");
        }

        if (refined.Count > 0)
        {
            Say(string.Empty);
            Say("AND WHERE THE REFINEMENT RUNG PUT EACH ONE:");

            foreach (var (rung, results) in refined)
            {
                foreach (var result in results)
                {
                    Say(
                        $"  {result.Decoder,-20} at {rung:F1} dB: {result.Decoded,3} of "
                        + $"{result.Trials} = {result.Rate,5:F1} % - "
                        + (result.Rate > HalfPerCent ? "ABOVE 50" : "below 50"));
                }
            }
        }

        Say(string.Empty);
        Say($"COARSE SEARCH WALL CLOCK {wholeClock.Elapsed.TotalSeconds:F1} s.");
        Say(
            "THE 306-TRIAL PAIR IS SET FROM THIS TABLE, in "
            + "docs/unit256-crossings-and-combining.md, and the two constants at the top of "
            + "Ft8Unit256CellCentreBracketTests are set from it.");

        File.WriteAllLines(RunLogPath("cell-centre-coarse"), lines);

        // ASSERTION ONE OF TWO: zero wrong on every row. Every wrong return was printed above,
        // sent beside returned, before this ran.
        // (The coarse search's rows are collected per rung; the assertion is carried on the
        // 306-trial rungs, where the rows that get published are measured. Here the wrong returns
        // are printed and counted so that nothing is lost, and the search is a localisation.)

        // Nothing about any rate is asserted here, at any rung, in any column.
        Assert.True(
            lines.Count > 0,
            "the coarse search wrote no lines, which means it walked nothing.");
    }

    /// <summary>
    /// Walks one 306-trial bracket rung, prints everything, then asserts the two things and no
    /// more.
    /// </summary>
    private void BracketRung(double rung, string side)
    {
        var lines = new List<string>();

        void Say(string line)
        {
            lines.Add(line);
            output.WriteLine(line);
        }

        Say(
            $"UNIT 256, TASK 3, THE {side} BRACKET RUNG: {rung:F1} dB, {FullTrials} trials, "
            + "4 columns.");
        Say(PlacementLine);
        Say(
            "LADDER: Ft8LadderHarness.Run - one signal, no neighbour. The audio is synthesised "
            + "once per trial and every column is handed the same array, so this is a PAIRED "
            + "design.");
        Say(
            $"THE RUNG WAS SET FROM THE COARSE SEARCH at {CoarseTrials} trials, whose table is "
            + "docs/unit256-runs/cell-centre-coarse.txt. It is not carried over from any earlier "
            + "unit.");
        Say(
            "COLUMNS: Ft8Sharp, Deep all off, OSD only, subtraction only - the four that read "
            + "\"not bracketed\" in unit 255 section 4.1's cell-centre table.");
        Say(string.Empty);

        var (results, columns, elapsed) = WalkOneRung(rung, FullTrials);

        // THE TABLE IS PRINTED BEFORE ANYTHING IS ASSERTED.
        Say(Ft8LadderHarness.Header);

        foreach (var result in results)
        {
            Say(result.AsRow());
        }

        Say(string.Empty);
        Say($"THE TIME BUDGET, at {rung:F1} dB at the cell centre:");
        Say("column              worst slot ms   its candidates   margin vs 15 000 ms      ms/trial");

        for (var i = 0; i < columns.Length; i++)
        {
            var column = columns[i];
            Say(
                $"{column.Name,-19} {column.WorstSlotMilliseconds,13:F1} "
                + $"{column.WorstSlotCandidates,16} "
                + $"{SlotBudgetMilliseconds / column.WorstSlotMilliseconds,20:F0}x "
                + $"{results[i].MillisecondsPerTrial,13:F1}");
        }

        var againstPort = Ft8LadderHarness.Discordance(results[0], results[2]);

        Say(string.Empty);
        Say("THE DISCORDANT COUNTS, on identical audio:");
        Say(
            $"OSD only vs Ft8Sharp             only Ft8Sharp {againstPort.OnlyFirst,6}   "
            + $"only OSD only {againstPort.OnlySecond,6}");

        Say(string.Empty);
        Say($"rung wall clock {elapsed.TotalSeconds:F1} s.");

        // EVERY WRONG RETURN IS PRINTED, SENT BESIDE RETURNED, BEFORE IT IS ASSERTED AWAY.
        var wrongTotal = 0;

        foreach (var result in results)
        {
            wrongTotal += result.Wrong;

            foreach (var wrong in result.WrongReturns)
            {
                Say($"WRONG on {result.Decoder}: {wrong}");
            }
        }

        Say($"TOTAL WRONG ACROSS ALL FOUR COLUMNS AT THIS RUNG: {wrongTotal}.");
        Say(
            $"TOTAL SCORED SLOT DECODES AT THIS RUNG: {results.Count * FullTrials}.");

        File.WriteAllLines(RunLogPath($"cell-centre-minus{Math.Abs(rung):F0}"), lines);

        // ASSERTION ONE OF TWO: the attribution column. If this goes red, every column to its
        // right is measuring the reproduction as well as the stage that names it, and the table
        // above is still evidence.
        Assert.True(
            results[0].Decoded == results[1].Decoded
                && results[0].Missed == results[1].Missed
                && results[0].Wrong == results[1].Wrong,
            $"at {rung:F1} dB at the cell centre the port returned {results[0].Decoded}/"
                + $"{results[0].Missed}/{results[0].Wrong} and Deep with every stage off returned "
                + $"{results[1].Decoded}/{results[1].Missed}/{results[1].Wrong}. Deep with every "
                + "stage null is meant to be the port byte for byte, so a difference here means "
                + "no column to its right is attributable to the stage that names it.");

        // ASSERTION TWO OF TWO: zero wrong on every row. A message shown that nobody sent is
        // worse than a message missed, and no unit may be the one that stops checking.
        foreach (var result in results)
        {
            Assert.True(
                result.Wrong == 0,
                $"{result.Decoder} at {rung:F1} dB at the cell centre returned {result.Wrong} "
                    + "message(s) that were not sent. A wrong decode is counted separately from a "
                    + "missed one everywhere in this project and every column measured reads "
                    + "zero.");
        }

        // Nothing about the rate is asserted, at any rung, in any column.
    }

    /// <summary>
    /// <b>The upper rung of the bracket at 306 trials</b> — the less negative of the two, and the
    /// one that must be above 50 per cent.
    /// </summary>
    [Fact]
    public void TheCellCentreBracketAtTheUpperRung() => BracketRung(UpperRungDecibels, "UPPER");

    /// <summary>
    /// <b>The lower rung of the bracket at 306 trials</b> — the more negative of the two, and the
    /// one that must be below 50 per cent.
    /// </summary>
    [Fact]
    public void TheCellCentreBracketAtTheLowerRung() => BracketRung(LowerRungDecibels, "LOWER");
}
