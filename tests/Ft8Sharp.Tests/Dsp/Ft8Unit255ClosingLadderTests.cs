using System.Diagnostics;
using Ft8Sharp.Deep;
using Ft8Sharp.Dsp;
using Ft8Sharp.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>THE CLOSING MEASUREMENT: six columns, three rungs, both placements, 306 trials a cell.</b>
/// The port, Deep with every stage off, fine sync alone, ordered statistics alone, <b>the
/// configuration <c>Ft8Reception.cs:460</c> actually builds</b>, and subtraction alone.
/// </summary>
/// <remarks>
/// <para>
/// <b>THIS IS A MEASUREMENT AND NOT A TEST, AND IT IS NOT IN THE GATE SET.</b>
/// <c>docs/gate-set.md</c> rules that <c>Ft8LadderHarness.Run</c> is called when a step needs a
/// number and is never a gate-set entry, and rule 5 forbids adding a test without naming the
/// breakage it would have caught. <b>A closing measurement has no defect to watch fail</b>, so
/// none of these methods was watched failing first and none earns a breakage-record entry. No red
/// is manufactured to satisfy a rule that does not bind.
/// </para>
/// <para>
/// <b>ONE RUNG-PLACEMENT PER TEST METHOD.</b> HM-DEC-155 forbids backgrounding and polling and the
/// watchdog fires at twelve minutes of silence. <c>docs/unit255-closing-measurement.md</c> §1.6
/// priced one rung-placement over all six columns at <b>217 s</b> from the recorded per-trial
/// costs — below the 300 s line at which the columns would have had to be split across two methods
/// — against a stated 480 s ceiling. <b>Splitting is not shrinking:</b> every cell is 306 trials,
/// six whole blocks of the 51-message population.
/// </para>
/// <para>
/// <b>EVERY TABLE IS PRINTED BEFORE ANY ASSERTION IS EVALUATED.</b> A closing measurement that
/// dies on an assertion and takes its own numbers with it has cost the night for nothing.
/// </para>
/// <para>
/// <b>THE ONLY TWO ASSERTIONS THAT BITE ARE ZERO WRONG ON EVERY ROW AND THE ATTRIBUTION EQUALITY.</b>
/// <c>Deep all off</c> must equal <c>Ft8Sharp</c> in decoded, missed and wrong, or nothing to its
/// right is attributable to the stage that names the column. <b>No bound is asserted on any decode
/// rate, at any rung, in any column</b> — targets are waypoints and a rung that returns nothing is
/// a measurement.
/// </para>
/// <para>
/// <b>THE PAIRING IS WHAT MAKES THE DISCORDANT COUNTS WORTH MORE THAN THE INTERVALS.</b>
/// <c>Ft8LadderHarness.Run</c> synthesises the audio once per trial and hands every column the same
/// array, so <c>Discordance</c> answers the question two overlapping Wilson intervals cannot —
/// which is <c>HM-OPEN-078</c>, and what stopped unit 252 moving a default.
/// </para>
/// </remarks>
public class Ft8Unit255ClosingLadderTests(ITestOutputHelper output)
{
    /// <summary>306 trials: six whole blocks of the 51-message population.</summary>
    private const int Trials = 306;

    /// <summary>FT8's whole slot, which every worst-slot margin below is quoted against.</summary>
    private const double SlotBudgetMilliseconds = 15_000.0;

    /// <summary>
    /// <b>Unit 248's cell centre</b>, from <c>Ft8Unit248ScoreboardTests</c> <c>:44</c> and <c>:46</c>.
    /// These two constants and no others, so tonight's rows are comparable with unit 248's own.
    /// </summary>
    private const double CellCentreFrequencyOffsetHz = 1.56;

    private const int CellCentreOffsetSamples = 480;

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
    /// <b>The six columns, each one call to the constructor at
    /// <c>src/Ft8Sharp.Deep/Ft8DeepSlotDecoder.cs:76</c>.</b> No new type is needed for any of them,
    /// because every stage parameter on that constructor is nullable and defaults to off.
    /// </summary>
    /// <remarks>
    /// <b>Column 5 is transcribed from <c>src/Hamlet.RadioEngine/Audio/Ft8Reception.cs:460</c> and
    /// not assumed.</b> That line builds
    /// <c>new Ft8DeepSlotDecoder(osd: Ft8DeepOsdSettings.Default, fineSync:
    /// Ft8DeepFineSyncSettings.Default)</c> — ordered statistics and fine sync, no subtraction, and
    /// <c>rememberHearings</c> left false. It is the only column here that says what is on the
    /// operator's screen.
    /// </remarks>
    private static Column[] Columns()
    {
        var port = new Ft8SlotDecoder();
        var allOff = new Ft8DeepSlotDecoder();
        var fineSync = new Ft8DeepSlotDecoder(fineSync: Ft8DeepFineSyncSettings.Default);
        var osd = new Ft8DeepSlotDecoder(osd: Ft8DeepOsdSettings.Default);
        var shipping = new Ft8DeepSlotDecoder(
            osd: Ft8DeepOsdSettings.Default,
            fineSync: Ft8DeepFineSyncSettings.Default);
        var subtraction = new Ft8DeepSlotDecoder(subtraction: Ft8DeepSubtractionSettings.Default);

        return
        [
            new Column("Ft8Sharp", samples => port.Decode(samples)),
            new Column("Deep all off", samples => allOff.Decode(samples)),
            new Column("fine sync only", samples => fineSync.Decode(samples)),
            new Column("OSD only", samples => osd.Decode(samples)),
            new Column("SHIPPING", samples => shipping.Decode(samples)),
            new Column("subtraction only", samples => subtraction.Decode(samples)),
        ];
    }

    /// <summary>
    /// <b>Where a walk's report is written, as well as to the test output.</b>
    /// </summary>
    /// <remarks>
    /// <b>This is not decoration.</b> VSTest does not surface <c>ITestOutputHelper</c> for a test
    /// that PASSES, and the first run of this class passed in 3 m 55 s and printed nothing. A
    /// closing measurement whose numbers exist only in a console buffer has cost the night for
    /// nothing, so the report is written to a file that is committed and transcribed from.
    /// </remarks>
    private static string RunLogPath(string stem)
    {
        var folder = Path.Combine(
            Ft8CaptureFixtures.RepositoryRoot(), "docs", "unit255-runs");
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, stem + ".txt");
    }

    /// <summary>Walks one rung at one placement through all six columns and prints every count.</summary>
    /// <param name="rung">The ratio asked for, in decibels in the 2500 Hz reference bandwidth.</param>
    /// <param name="onGrid">
    /// <see langword="true"/> for <c>DefaultFrequencyHz</c> and <c>DefaultOffsetSamples</c>;
    /// <see langword="false"/> for unit 248's cell centre, <c>+1.56 Hz</c> and <c>+480 samples</c>.
    /// </param>
    private void Walk(double rung, bool onGrid)
    {
        var lines = new List<string>();

        void Say(string line)
        {
            lines.Add(line);
            output.WriteLine(line);
        }

        var columns = Columns();
        var decoders = columns
            .Select(column => new Ft8LadderHarness.Decoder(column.Name, column.Run))
            .ToArray();

        var placement = onGrid ? "ON GRID" : "CELL CENTRE";
        var where = onGrid
            ? "1000.0 Hz, three whole symbol periods in - Ft8LadderHarness.DefaultFrequencyHz and "
                + "DefaultOffsetSamples"
            : $"+{CellCentreFrequencyOffsetHz:F2} Hz, +{CellCentreOffsetSamples} samples - unit 248's "
                + "WorstFrequencyOffsetHz and WorstOffsetSamples, and no others";

        Say(
            $"UNIT 255 CLOSING LADDER, {rung:F1} dB, {placement}, {Trials} trials, "
            + $"{columns.Length} columns.");
        Say($"PLACEMENT: {where}.");
        Say(
            "LADDER: Ft8LadderHarness.Run - one signal, no neighbour. The audio is synthesised once "
            + "per trial and every column is handed the same array, so this is a PAIRED design.");
        Say(
            "SHIPPING is new Ft8DeepSlotDecoder(osd: Ft8DeepOsdSettings.Default, fineSync: "
            + "Ft8DeepFineSyncSettings.Default) - transcribed from Ft8Reception.cs:460.");
        Say(string.Empty);

        var clock = Stopwatch.StartNew();
        var results = onGrid
            ? Ft8LadderHarness.Run(rung, Trials, decoders: decoders)
            : Ft8LadderHarness.Run(
                rung,
                Trials,
                decoders: decoders,
                frequencyHz: Ft8LadderHarness.DefaultFrequencyHz + CellCentreFrequencyOffsetHz,
                offsetSamples: Ft8LadderHarness.DefaultOffsetSamples + CellCentreOffsetSamples);
        clock.Stop();

        // THE TABLE IS PRINTED BEFORE ANYTHING IS ASSERTED. Ruling 4.
        Say(Ft8LadderHarness.Header);
        foreach (var result in results)
        {
            Say(result.AsRow());
        }

        Say(string.Empty);
        Say("THE TIME BUDGET, on this rung and placement:");
        Say(
            "column              worst slot ms   its candidates   margin vs 15 000 ms      ms/trial");

        for (var i = 0; i < columns.Length; i++)
        {
            var column = columns[i];
            Say(
                $"{column.Name,-19} {column.WorstSlotMilliseconds,13:F1} "
                + $"{column.WorstSlotCandidates,16} "
                + $"{SlotBudgetMilliseconds / column.WorstSlotMilliseconds,20:F0}x "
                + $"{results[i].MillisecondsPerTrial,13:F1}");
        }

        // THE DISCORDANT COUNTS. On a paired design these two integers say more than two
        // overlapping Wilson intervals, which is what stopped unit 252 moving a default.
        var shipping = results[4];
        var againstPort = Ft8LadderHarness.Discordance(results[0], shipping);
        var againstAllOff = Ft8LadderHarness.Discordance(results[1], shipping);

        Say(string.Empty);
        Say("THE DISCORDANT COUNTS FOR SHIPPING, on identical audio:");
        Say("comparison                       only the first   only SHIPPING");
        Say(
            $"SHIPPING vs Ft8Sharp             {againstPort.OnlyFirst,14}  {againstPort.OnlySecond,14}");
        Say(
            $"SHIPPING vs Deep all off         {againstAllOff.OnlyFirst,14}  "
            + $"{againstAllOff.OnlySecond,14}");

        Say(string.Empty);
        Say($"rung-placement wall clock {clock.Elapsed.TotalSeconds:F1} s.");

        // Every wrong return is printed, sent beside returned, before it is asserted away.
        foreach (var result in results)
        {
            foreach (var wrong in result.WrongReturns)
            {
                Say($"WRONG on {result.Decoder}: {wrong}");
            }
        }

        var stem = $"minus{Math.Abs(rung):F0}-{(onGrid ? "on-grid" : "cell-centre")}";
        File.WriteAllLines(RunLogPath(stem), lines);

        // ASSERTION ONE OF TWO: the attribution column. If this goes red, every column to its
        // right is measuring the reproduction as well as the stage that names it, and the table
        // above is still evidence.
        Assert.True(
            results[0].Decoded == results[1].Decoded
                && results[0].Missed == results[1].Missed
                && results[0].Wrong == results[1].Wrong,
            $"at {rung:F1} dB {placement} the port returned {results[0].Decoded}/"
                + $"{results[0].Missed}/{results[0].Wrong} and Deep with every stage off returned "
                + $"{results[1].Decoded}/{results[1].Missed}/{results[1].Wrong}. Deep with every "
                + "stage null is meant to be the port byte for byte, so a difference here means no "
                + "column to its right is attributable to the stage that names it.");

        // ASSERTION TWO OF TWO: zero wrong on every row. A message shown that nobody sent is
        // worse than a message missed, and no unit may be the one that stops checking.
        foreach (var result in results)
        {
            Assert.True(
                result.Wrong == 0,
                $"{result.Decoder} at {rung:F1} dB {placement} returned {result.Wrong} message(s) "
                    + "that were not sent. A wrong decode is counted separately from a missed one "
                    + "everywhere in this project and every column measured reads zero.");
        }

        // Nothing about the rate is asserted, at any rung, in any column.
    }

    /// <summary>-19 dB on the grid. The rung the port reads 248 of 306 at, in the record.</summary>
    [Fact]
    public void TheClosingLadderAtMinus19OnGrid() => Walk(-19.0, onGrid: true);

    /// <summary>-20 dB on the grid.</summary>
    [Fact]
    public void TheClosingLadderAtMinus20OnGrid() => Walk(-20.0, onGrid: true);

    /// <summary>
    /// -21 dB on the grid — <b>the only rung and placement the shipping stack has ever been
    /// measured at</b>, at 35 of 306 in <c>docs/unit252-osd-window.md</c>.
    /// </summary>
    [Fact]
    public void TheClosingLadderAtMinus21OnGrid() => Walk(-21.0, onGrid: true);

    /// <summary>
    /// <b>-19 dB at the cell centre, and this is where the interesting number is.</b> Unit 248
    /// measured the port at <b>6 of 306</b> here against <b>248</b> on the grid — a collapse from
    /// 81 per cent to 2 for a sender moving one and a half hertz and an eightieth of a second, a
    /// distance no operator could control or would notice. <b>What the shipping stack does about
    /// that has never been measured.</b>
    /// </summary>
    [Fact]
    public void TheClosingLadderAtMinus19AtCellCentre() => Walk(-19.0, onGrid: false);

    /// <summary>-20 dB at the cell centre.</summary>
    [Fact]
    public void TheClosingLadderAtMinus20AtCellCentre() => Walk(-20.0, onGrid: false);

    /// <summary>-21 dB at the cell centre.</summary>
    [Fact]
    public void TheClosingLadderAtMinus21AtCellCentre() => Walk(-21.0, onGrid: false);
}
