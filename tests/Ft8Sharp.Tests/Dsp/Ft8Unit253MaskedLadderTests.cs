using System.Diagnostics;
using Ft8Sharp.Deep;
using Ft8Sharp.Dsp;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>THE MASKED LADDER: what subtracting a loud station buys the quiet one underneath it, with the
/// ceiling beside it saying what was there to be recovered.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>THE CELL WAS CHOSEN BY <c>Ft8Unit253MaskingSurveyTests</c> AND NOT BY THIS FILE.</b> That
/// survey walked twenty cells before a subtractor existed and applied a rule written before it ran:
/// the single pass must lose the quiet message, the ceiling must say it was recoverable, and
/// <b>the loud station must itself decode on the first pass</b> - because a subtractor subtracts a
/// decoded message, and a cell where the loud station is lost too is a cell where multi-pass is
/// arithmetically incapable of doing anything. Fourteen of twenty cells qualified and the rule's
/// tie-break landed on the hardest of them.
/// </para>
/// <para>
/// <b>NOTHING HERE ASSERTS A RATE.</b> No bound, no target - <c>PHASE_PLAN.md</c>'s ruling is that
/// targets are waypoints and a step closes on the figure it reached. A column that returns nothing
/// is a measurement. <b>What is asserted on every row is zero wrong</b>, with every wrong return
/// printed with both sent messages beside it, because a message returned that neither station sent
/// is this step's own hazard: a subtraction leaves residue, and residue that decodes is the one
/// thing this stage can invent that no earlier stage could.
/// </para>
/// <para>
/// <b>THE ISOLATION IS TOTAL.</b> Ordered statistics off and fine synchronisation off on every
/// column, so the difference between the single-pass column and the two-pass column is subtraction
/// and nothing else. The shipping configuration is a different measurement and is labelled as one
/// wherever it appears.
/// </para>
/// </remarks>
public class Ft8Unit253MaskedLadderTests(ITestOutputHelper output)
{
    /// <summary>The separation the survey chose, in hertz between the two lowest tones.</summary>
    private const double SeparationHz = 0.0;

    /// <summary>The level difference the survey chose, loud minus quiet, in decibels.</summary>
    private const double LevelDecibels = 6.0;

    /// <summary>The rung the survey was walked at and the ladder is walked at.</summary>
    private const double RungDecibels = -18.0;

    /// <summary>The rung the single-signal control is walked at.</summary>
    private const double ControlRungDecibels = -20.0;

    /// <summary>One block of the scoreable population.</summary>
    private const int OneBlock = 51;

    /// <summary>Six whole blocks - the count every recorded figure in this phase was taken at.</summary>
    private const int SixBlocks = 306;

    /// <summary>
    /// One column, with its decoder and the counts the decoder publishes after every slot.
    /// </summary>
    /// <remarks>
    /// <b>A column is a delegate over samples</b>, so a multi-pass column needs no change to the
    /// harness at all - unit 247's <c>Decoder</c> record already had the shape. Everything a report
    /// wants beyond the three counts is read off <c>Ft8DeepSlotDecoder</c> after the call and
    /// accumulated here.
    /// </remarks>
    private sealed class Column(string name, Ft8DeepSlotDecoder decoder)
    {
        internal string Name { get; } = name;

        internal double WorstSlotMilliseconds { get; private set; }

        internal int PassesRun { get; private set; }

        internal int Subtracted { get; private set; }

        internal int RefusedForWantOfSymbols { get; private set; }

        internal int RefusedForWantOfFrame { get; private set; }

        internal int DuplicatesAcrossPasses { get; private set; }

        internal int FromLaterPasses { get; private set; }

        internal int Slots { get; private set; }

        internal double PassesPerSlot => Slots == 0 ? double.NaN : (double)PassesRun / Slots;

        internal Ft8LadderHarness.MaskedDecoder Masked(bool unmasked = false) =>
            new(Name, Decode, unmasked);

        internal Ft8LadderHarness.Decoder Plain() => new(Name, Decode);

        private Ft8SlotResult Decode(float[] samples)
        {
            var clock = Stopwatch.StartNew();
            var result = decoder.Decode(samples);
            clock.Stop();

            WorstSlotMilliseconds = Math.Max(WorstSlotMilliseconds, clock.Elapsed.TotalMilliseconds);

            var counts = decoder.LastSubtraction;
            PassesRun += counts.PassesRun;
            Subtracted += counts.MessagesSubtracted;
            RefusedForWantOfSymbols += counts.RefusedForWantOfSymbols;
            RefusedForWantOfFrame += counts.RefusedForWantOfFrame;
            DuplicatesAcrossPasses += counts.DuplicatesAcrossPasses;
            FromLaterPasses += counts.MessagesFromLaterPasses;
            Slots++;

            return result;
        }
    }

    private static Column Passes(int passes) =>
        new(
            passes == 1 ? "1 pass" : $"{passes} passes",
            passes == 1
                ? new Ft8DeepSlotDecoder()
                : new Ft8DeepSlotDecoder(subtraction: new Ft8DeepSubtractionSettings(maxPasses: passes)));

    private void WriteRows(IReadOnlyList<Ft8LadderHarness.Result> rows)
    {
        foreach (var line in Ft8LadderHarness.Report(rows))
        {
            output.WriteLine(line);
        }
    }

    private void WriteStage(IReadOnlyList<Column> columns)
    {
        output.WriteLine(string.Empty);
        output.WriteLine(
            "column        slots  passes  passes/slot  subtracted  refused-sym  refused-frame  "
            + "dup-cross  later-pass  worst slot ms");

        foreach (var column in columns)
        {
            output.WriteLine(
                $"{column.Name,-12} {column.Slots,6} {column.PassesRun,7} {column.PassesPerSlot,12:F2} "
                + $"{column.Subtracted,11} {column.RefusedForWantOfSymbols,12} "
                + $"{column.RefusedForWantOfFrame,14} {column.DuplicatesAcrossPasses,10} "
                + $"{column.FromLaterPasses,11} {column.WorstSlotMilliseconds,15:F1}");
        }
    }

    private void WriteDiscordance(
        Ft8LadderHarness.Result baseline, IReadOnlyList<Ft8LadderHarness.Result> others)
    {
        output.WriteLine(string.Empty);
        output.WriteLine(
            "  DISCORDANT COUNTS against \"" + baseline.Decoder + "\" - the paired statistic. The");
        output.WriteLine(
            "  concordant trials carry no information about which column is better; the evidence is");
        output.WriteLine(
            "  entirely in the trials where the two disagree. HM-OPEN-078.");

        foreach (var other in others)
        {
            var (onlyBaseline, onlyOther) = Ft8LadderHarness.Discordance(baseline, other);
            output.WriteLine(
                $"    vs {other.Decoder,-14} only \"{baseline.Decoder}\" decoded it: {onlyBaseline,4}   "
                + $"only \"{other.Decoder}\" decoded it: {onlyOther,4}");
        }
    }

    private void AssertNoWrongDecodes(IReadOnlyList<Ft8LadderHarness.Result> rows)
    {
        foreach (var row in rows)
        {
            Assert.True(
                row.Wrong == 0,
                $"\"{row.Decoder}\" returned {row.Wrong} messages that neither station sent. Every "
                + "one is printed above with both sent messages beside it. A wrong decode is worse "
                + "than a missed one and this step's own hazard is residue that decodes.");
        }
    }

    /// <summary>
    /// <b>4a - THE PASS-COUNT SWEEP. One block of 51 trials on the cell the survey chose, at one,
    /// two, three and four passes, and the stopping rule is read off this table.</b>
    /// </summary>
    /// <remarks>
    /// <b>THE BREAKAGE THIS WOULD HAVE CAUGHT.</b> A pass count shipped as a setting somebody
    /// chose - two because two sounded reasonable, or four because more sounded better - with
    /// nothing in the record saying what the third and fourth passes bought or what they cost. Every
    /// pass is a whole extra decode and puts another slot's worth of codewords to the port's CRC-14
    /// at about one in 16 384 each, so an unmeasured pass count is an unmeasured false-accept budget
    /// spent on nothing. This table is cheap enough that the answer exists even if the night ends
    /// early.
    /// </remarks>
    [Fact]
    public void ThePassCountSweepPricesWhatEachFurtherPassBuys()
    {
        var columns = new[] { Passes(1), Passes(2), Passes(3), Passes(4) };

        output.WriteLine(
            $"4a - THE PASS-COUNT SWEEP. quiet station at {RungDecibels:F1} dB, loud station "
            + $"{LevelDecibels:F0} dB up at {SeparationHz:F2} Hz separation, same offset, "
            + $"{OneBlock} trials - one whole block of the population.");
        output.WriteLine("ordered statistics OFF and fine sync OFF on every column.");
        output.WriteLine(string.Empty);

        var rows = Ft8LadderHarness.RunMasked(
            RungDecibels,
            OneBlock,
            SeparationHz,
            LevelDecibels,
            columns.Select(c => c.Masked()).ToArray());

        WriteRows(rows);
        WriteStage(columns);

        output.WriteLine(string.Empty);
        output.WriteLine("  WHAT EACH FURTHER PASS BOUGHT, AND WHAT IT COST:");

        for (var i = 1; i < rows.Count; i++)
        {
            var gained = rows[i].Decoded - rows[i - 1].Decoded;
            var cost = rows[i].MillisecondsPerTrial - rows[i - 1].MillisecondsPerTrial;
            var (onlyBefore, onlyAfter) = Ft8LadderHarness.Discordance(rows[i - 1], rows[i]);

            output.WriteLine(
                $"    pass {i + 1} over pass {i}: {gained,+4} decodes of {OneBlock}, "
                + $"{cost,+8:F1} ms a trial   (only pass {i}: {onlyBefore}, only pass {i + 1}: {onlyAfter})");
        }

        WriteDiscordance(rows[0], rows.Skip(1).ToArray());

        var worst = columns.Max(c => c.WorstSlotMilliseconds);
        output.WriteLine(string.Empty);
        output.WriteLine(
            $"  WORST OBSERVED SLOT over the whole sweep: {worst:F1} ms, a margin of "
            + $"{15000.0 / Math.Max(worst, 0.001):F0}x against FT8's 15 000 ms.");

        AssertNoWrongDecodes(rows);
    }

    /// <summary>
    /// <b>4b - THE SCOREBOARD. 306 trials, six whole blocks, single pass against two passes with
    /// the ceiling beside them.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>THIS IS THE ROW STEP 4'S SECOND EXIT CRITERION IS READ FROM</b> - the ladder showing more
    /// decodes from the same audio than a single pass, at a stated signal-to-noise ratio with its
    /// trial count. The ceiling is beside it because <b>a gain quoted without the ceiling is a
    /// number with no scale</b>: it is the difference between "subtraction recovered four fifths of
    /// what was there" and "the neighbour was never costing anything".
    /// </para>
    /// <para>
    /// <b>THE BREAKAGE THIS WOULD HAVE CAUGHT.</b> A subtraction stage measured only against a
    /// single-pass column, reporting a rise, with no way to tell whether it recovered what was
    /// masked or found something else - and with two overlapping Wilson intervals underneath it,
    /// which on a paired design is the wrong question asked and answered. Unit 252 was stopped by
    /// exactly that and had to leave a default where it was.
    /// </para>
    /// </remarks>
    [Fact]
    public void TheMaskedLadderShowsWhatSubtractionBoughtAgainstTheCeiling()
    {
        var single = Passes(1);
        var two = Passes(2);

        // THE NAMED DROP CANDIDATE, KEPT. The work instruction names the three-pass column at 306
        // trials as the most expensive optional thing in the night and licenses dropping it if the
        // night runs long. 4a priced it at 51 trials - zero decodes over two passes for 65 ms a
        // trial - and the night had the budget, so it is run at 306 rather than quoted from 51.
        var three = Passes(3);
        var ceiling = new Column("ceiling", new Ft8DeepSlotDecoder());

        output.WriteLine(
            $"4b - THE SCOREBOARD. quiet station at {RungDecibels:F1} dB, loud station "
            + $"{LevelDecibels:F0} dB up at {SeparationHz:F2} Hz separation, same offset, "
            + $"{SixBlocks} trials - six whole blocks of the 51-message population.");
        output.WriteLine(
            "ordered statistics OFF and fine sync OFF on every column, so the difference between");
        output.WriteLine("the first two columns is subtraction and nothing else.");
        output.WriteLine(
            "\"ceiling\" is the SAME AUDIO with the loud station absent and the identical noise");
        output.WriteLine("draw - bit-for-bit what Ft8LadderHarness.Run draws at this rung and seed.");
        output.WriteLine(string.Empty);

        var rows = Ft8LadderHarness.RunMasked(
            RungDecibels,
            SixBlocks,
            SeparationHz,
            LevelDecibels,
            new[] { single.Masked(), two.Masked(), three.Masked(), ceiling.Masked(unmasked: true) });

        WriteRows(rows);
        WriteStage(new[] { single, two, three, ceiling });
        WriteDiscordance(rows[0], new[] { rows[1], rows[2], rows[3] });

        var recovered = rows[1].Decoded - rows[0].Decoded;
        var available = rows[3].Decoded - rows[0].Decoded;

        output.WriteLine(string.Empty);
        output.WriteLine("  WHAT SUBTRACTION BOUGHT, AGAINST WHAT WAS THERE TO BE RECOVERED:");
        output.WriteLine(
            $"    single pass  {rows[0].Decoded} of {SixBlocks}   two passes  {rows[1].Decoded} of "
            + $"{SixBlocks}   three passes  {rows[2].Decoded} of {SixBlocks}   ceiling  "
            + $"{rows[3].Decoded} of {SixBlocks}");
        output.WriteLine(
            $"    recovered {recovered} of the {available} the ceiling says were there"
            + (available > 0 ? $" - {100.0 * recovered / available:F1} per cent." : "."));

        var worst = Math.Max(single.WorstSlotMilliseconds, two.WorstSlotMilliseconds);
        output.WriteLine(string.Empty);
        output.WriteLine(
            $"  WORST OBSERVED SLOT: single pass {single.WorstSlotMilliseconds:F1} ms, two passes "
            + $"{two.WorstSlotMilliseconds:F1} ms - a margin of "
            + $"{15000.0 / Math.Max(worst, 0.001):F0}x against FT8's 15 000 ms.");

        AssertNoWrongDecodes(rows);
    }

    /// <summary>
    /// <b>THE NICE-TO-PASS ROW, AND IT IS NOT PART OF THE ISOLATION.</b> The shipping
    /// configuration - ordered statistics on at the settled default and fine synchronisation on -
    /// with one pass against two, so the report can say what the operator would actually get rather
    /// than what the isolation says.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>LABELLED AS NOT PART OF THE ISOLATION, EVERY TIME IT IS QUOTED.</b> Three stages are on
    /// here, so a difference between these two columns is subtraction <em>in the presence of</em>
    /// ordered statistics and fine sync, and it is not attributable to subtraction alone the way
    /// 4b's columns are. It is here because the shipping verdict in task 5 has to say what turning
    /// subtraction on would buy the operator and cost him a slot, and the isolation cannot answer
    /// that.
    /// </para>
    /// <para>
    /// <b>One block of 51 trials rather than six, and that is a stated limit.</b> The shipping
    /// configuration costs about five times the isolation a slot, and this row is optional; six
    /// blocks of it would have crowded out something that is not. The Wilson interval on 51 trials
    /// is wide and is printed with the row so nobody reads the rate as if it were 4b's.
    /// </para>
    /// </remarks>
    [Fact]
    public void TheShippingConfigurationIsMeasuredOnceAndIsLabelledNotPartOfTheIsolation()
    {
        var one = new Column(
            "ship 1 pass",
            new Ft8DeepSlotDecoder(
                osd: Ft8DeepOsdSettings.Default, fineSync: Ft8DeepFineSyncSettings.Default));

        var two = new Column(
            "ship 2 pass",
            new Ft8DeepSlotDecoder(
                osd: Ft8DeepOsdSettings.Default,
                fineSync: Ft8DeepFineSyncSettings.Default,
                subtraction: Ft8DeepSubtractionSettings.Default));

        var ceiling = new Column(
            "ship ceil",
            new Ft8DeepSlotDecoder(
                osd: Ft8DeepOsdSettings.Default, fineSync: Ft8DeepFineSyncSettings.Default));

        output.WriteLine(
            "*** NOT PART OF THE ISOLATION. Ordered statistics ON at the settled default and fine");
        output.WriteLine(
            "*** synchronisation ON, so a difference between these columns is subtraction IN THE");
        output.WriteLine(
            "*** PRESENCE OF two other stages and is not attributable to subtraction alone. 4b is");
        output.WriteLine("*** the isolation and this is not comparable with it.");
        output.WriteLine(string.Empty);
        output.WriteLine(
            $"ONE BLOCK of {OneBlock} trials, not six - the interval is wide and is printed.");
        output.WriteLine(
            $"quiet station at {RungDecibels:F1} dB, loud station {LevelDecibels:F0} dB up at "
            + $"{SeparationHz:F2} Hz separation.");
        output.WriteLine(string.Empty);

        var rows = Ft8LadderHarness.RunMasked(
            RungDecibels,
            OneBlock,
            SeparationHz,
            LevelDecibels,
            new[] { one.Masked(), two.Masked(), ceiling.Masked(unmasked: true) });

        WriteRows(rows);
        WriteStage(new[] { one, two, ceiling });
        WriteDiscordance(rows[0], new[] { rows[1], rows[2] });

        output.WriteLine(string.Empty);
        output.WriteLine("  WHAT THE OPERATOR WOULD ACTUALLY GET, ON ONE BLOCK:");
        output.WriteLine(
            $"    shipping one pass {rows[0].Decoded} of {OneBlock}, two passes {rows[1].Decoded} "
            + $"of {OneBlock}, ceiling {rows[2].Decoded} of {OneBlock}");
        output.WriteLine(
            $"    cost: {rows[0].MillisecondsPerTrial:F1} ms a slot becomes "
            + $"{rows[1].MillisecondsPerTrial:F1} ms - "
            + $"{rows[1].MillisecondsPerTrial - rows[0].MillisecondsPerTrial:F1} ms more.");
        output.WriteLine(
            $"    worst observed slot: one pass {one.WorstSlotMilliseconds:F1} ms, two passes "
            + $"{two.WorstSlotMilliseconds:F1} ms - margin "
            + $"{15000.0 / Math.Max(two.WorstSlotMilliseconds, 0.001):F0}x against 15 000 ms.");

        AssertNoWrongDecodes(rows);
    }

    /// <summary>
    /// <b>4c - THE CONTROL, on the single-signal ladder, and it is not droppable. Subtraction must
    /// take nothing away and add no wrong decode.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Through <c>Ft8LadderHarness.Run</c> itself</b>, which is the audio every recorded row of
    /// this phase was taken on - one station, no neighbour, the ordinary sensitivity case. This is
    /// the row that says the stage is safe to have in the path at all.
    /// </para>
    /// <para>
    /// <b>THE BREAKAGE THIS WOULD HAVE CAUGHT.</b> A subtraction stage that recovers masked
    /// messages and quietly costs unmasked ones - by subtracting a message and then failing to
    /// return it, by mis-fitting a lone station and leaving a shaped remnant that produces a wrong
    /// decode, or by returning a message twice under a duplicate rule that does not hold across
    /// passes. On 14.074 most slots are not the masked case, so a stage that trades an ordinary
    /// decode for a masked one is a net loss the masked ladder alone cannot see.
    /// </para>
    /// </remarks>
    [Fact]
    public void SubtractionTakesNothingAwayFromTheSingleSignalLadder()
    {
        var off = new Column("sub off", new Ft8DeepSlotDecoder());
        var on = new Column(
            "sub on", new Ft8DeepSlotDecoder(subtraction: Ft8DeepSubtractionSettings.Default));

        output.WriteLine(
            $"4c - THE CONTROL. One station, no neighbour, {ControlRungDecibels:F1} dB, "
            + $"{SixBlocks} trials, through Ft8LadderHarness.Run itself - the same audio every");
        output.WriteLine("recorded row of this phase was taken on.");
        output.WriteLine(string.Empty);

        var rows = Ft8LadderHarness.Run(
            ControlRungDecibels,
            SixBlocks,
            decoders: new[] { off.Plain(), on.Plain() });

        WriteRows(rows);
        WriteStage(new[] { off, on });

        var (lost, gained) = Ft8LadderHarness.Discordance(rows[0], rows[1]);

        output.WriteLine(string.Empty);
        output.WriteLine("  THE CONTROL'S FIGURE:");
        output.WriteLine(
            $"    subtraction off {rows[0].Decoded} of {SixBlocks}, on {rows[1].Decoded} of "
            + $"{SixBlocks}.");
        output.WriteLine(
            $"    trials only OFF decoded (what subtraction TOOK AWAY): {lost}");
        output.WriteLine(
            $"    trials only ON decoded (what subtraction added here):  {gained}");
        output.WriteLine(
            $"    worst observed slot: off {off.WorstSlotMilliseconds:F1} ms, on "
            + $"{on.WorstSlotMilliseconds:F1} ms - margin "
            + $"{15000.0 / Math.Max(on.WorstSlotMilliseconds, 0.001):F0}x against 15 000 ms.");

        AssertNoWrongDecodes(rows);

        // THE ONE THING THIS ROW ASSERTS BEYOND ZERO WRONG. Subtraction only ever adds a pass over
        // a residual; it never removes a message the first pass returned, because the first pass's
        // messages are added to the result before anything is subtracted. A non-zero count here is
        // a defect and is reported as one rather than averaged into a rate.
        Assert.True(
            lost == 0,
            $"subtraction cost the single-signal ladder {lost} trials that the same audio decoded "
            + "without it. The first pass's messages are added to the result before anything is "
            + "subtracted, so this cannot happen by design and a non-zero count is a defect.");
    }
}
