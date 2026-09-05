using Ft8Sharp.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>THE ONLY TEST UNIT 256 CONSTRUCTS THAT IS A TEST AT ALL, AND THE ONLY ONE WATCHED FAILING
/// FIRST.</b> Tasks 3 and 4 are ladder walks, and <c>docs/gate-set.md</c> line 57 rules that a
/// ladder is a measurement rather than a test; rule 5 forbids adding a test that names no
/// breakage. <b>This one names its breakage.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>THE BREAKAGE IT WOULD HAVE CAUGHT.</b> A closing document that publishes <c>-19.90 dB</c> as
/// a bare number, or — worse — publishes a band <em>narrower than the two rungs support</em>, and
/// is then set beside another decoder's <c>-19.7</c> and read as a 0.2 dB win that 306 trials
/// cannot support. <c>CLAUDE.md</c> §0.0 forbids exactly that comparison, and the band is what
/// stops the sentence being written. The concrete defect watched failing first is the one that
/// produces the second, dangerous form: <b>pairing an upper Wilson bound at one rung against a
/// lower bound at the other.</b>
/// </para>
/// <para>
/// <b>WHY THAT PAIRING IS NOT MERELY UNTIDY.</b> The crossing's position in the bracket,
/// <c>t(a, b) = (a - 50)/(a - b)</c>, increases in <em>both</em> rung rates. Mixing the bounds
/// therefore does not bracket anything: on <c>SHIPPING</c> on the grid it puts the so-called
/// optimistic end at <b>-19.81 dB</b>, which is <em>worse</em> than the point crossing
/// <b>-19.90 dB</b> it is supposed to contain, and on <c>Ft8Sharp</c> on the grid it yields a band
/// <b>0.02 dB wide against the 0.16 dB the two rungs support</b> — a claim of precision eight
/// times finer than the measurement.
/// </para>
/// <para>
/// <b>THIS TEST RUNS NO LADDER.</b> Every rate below is a committed count out of
/// <c>docs/unit255-runs/</c>, transcribed with its artefact named. Nothing here decodes anything;
/// it costs milliseconds, not minutes.
/// </para>
/// <para>
/// <b>AND IT ASSERTS NO BOUND ON ANY CROSSING.</b> Targets are waypoints. The three assertions are
/// about the arithmetic's internal consistency and about one hand-computed case, and not one of
/// them says a crossing ought to be at any particular ratio.
/// </para>
/// </remarks>
public class Ft8Unit256CrossingIntervalTests(ITestOutputHelper output)
{
    /// <summary>306 trials: six whole blocks of the 51-message population.</summary>
    private const int Trials = 306;

    /// <summary>
    /// <b>Every crossing unit 255 published, as the counts it was computed from.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Six on the grid and two at the cell centre — eight, and that is all of them.</b> The
    /// other four cell-centre columns read <c>not bracketed</c> in
    /// <c>docs/unit255-closing-measurement.md</c> §4.1 and have no crossing to put a band on;
    /// unit 256's task 3 goes and brackets them.
    /// </para>
    /// <para>
    /// <b>Source, row by row:</b> the on-grid counts are
    /// <c>docs/unit255-runs/minus19-on-grid.txt</c> and <c>minus20-on-grid.txt</c>; the
    /// cell-centre counts are <c>minus19-cell-centre.txt</c> and <c>minus20-cell-centre.txt</c>.
    /// <b>All eight are 306 trials, one signal, no neighbour, <c>Ft8Sharp.Deep</c> 0.8.0.</b>
    /// </para>
    /// </remarks>
    private static IReadOnlyList<(string Column, string Placement, int AtMinus19, int AtMinus20)>
        Published =>
    [
        ("Ft8Sharp", "on grid", 248, 73),
        ("Deep all off", "on grid", 248, 73),
        ("fine sync only", "on grid", 268, 95),
        ("OSD only", "on grid", 276, 125),
        ("SHIPPING", "on grid", 283, 138),
        ("subtraction only", "on grid", 248, 73),
        ("fine sync only", "cell centre", 277, 73),
        ("SHIPPING", "cell centre", 278, 73),
    ];

    /// <summary>
    /// <b>The four cell-centre columns unit 255 could only mark <c>not bracketed</c>, bracketed by
    /// rungs unit 256 measured tonight.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Every rung below is 306 trials at the cell centre — <c>+1.56 Hz</c>, <c>+480
    /// samples</c> — and none of them is a 51-trial coarse rung.</b> The coarse search at 51
    /// trials localised the crossings and is quoted nowhere as a crossing;
    /// <c>docs/unit256-runs/cell-centre-coarse.txt</c> is the record of it.
    /// </para>
    /// <para>
    /// <b>Three columns straddle at (-18, -17) and one at (-19, -18), and that is a measurement
    /// rather than an inconsistency.</b> <c>OSD only</c> reads <b>172 of 306, 56.2 per cent</b> at
    /// -18 dB — already above 50 — so its crossing lies a decibel deeper than the other three.
    /// <b>Its lower rung is unit 255's own -19 dB cell-centre row</b>, 33 of 306, measured at the
    /// same placement, the same seed and the same <c>Ft8Sharp.Deep</c> 0.8.0
    /// (<c>docs/unit255-runs/minus19-cell-centre.txt</c>) — <b>so all four are bracketed by
    /// 306-trial rungs and unit 256's named drop candidate was not taken.</b>
    /// </para>
    /// </remarks>
    private static IReadOnlyList<(
        string Column,
        string Placement,
        double UpperDecibels,
        int UpperDecoded,
        double LowerDecibels,
        int LowerDecoded,
        string Source)> BracketedTonight =>
    [
        ("Ft8Sharp", "cell centre", -17.0, 235, -18.0, 77,
            "unit 256 cell-centre-minus17.txt / cell-centre-minus18.txt"),
        ("Deep all off", "cell centre", -17.0, 235, -18.0, 77,
            "unit 256 cell-centre-minus17.txt / cell-centre-minus18.txt"),
        ("OSD only", "cell centre", -18.0, 172, -19.0, 33,
            "unit 256 cell-centre-minus18.txt / unit 255 minus19-cell-centre.txt"),
        ("subtraction only", "cell centre", -17.0, 235, -18.0, 77,
            "unit 256 cell-centre-minus17.txt / cell-centre-minus18.txt"),
    ];

    /// <summary>
    /// <b>THE COMBINING PANEL'S OWN ROWS, ON A LADDER THAT IS NOT THE ONE ABOVE.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>NO ROW HERE IS COMPARABLE WITH ANY ROW ABOVE AND THE PLACEMENT COLUMN SAYS SO ON ITS
    /// FACE.</b> These are <c>Ft8LadderHarness.RunRepeats</c> rows — <b>four slots a trial</b>
    /// carrying the same message, jittered 2.00 Hz and 480 samples between hearings — and
    /// <c>docs/unit255-closing-measurement.md</c> §5.0 rules that such a row cannot share a table
    /// with a single-slot row. They are computed here only because this is where the crossing
    /// arithmetic lives, and they are printed in their own block under their own heading.
    /// </para>
    /// <para>
    /// <b>The -21 dB rung is unit 255 §5.3's, cited and not re-run</b>
    /// (<c>docs/unit255-runs/accumulated-stacked-minus21.txt</c>, <c>Ft8Sharp.Deep</c> 0.8.0,
    /// which is the version in the tree tonight). The -22 dB rung is unit 256's
    /// (<c>docs/unit256-runs/combining-panel-minus22.txt</c>).
    /// </para>
    /// <para>
    /// <b>And the caveat travels with all three: a four-repeat column gets four single-slot
    /// attempts as well as deeper sums.</b>
    /// </para>
    /// </remarks>
    private static IReadOnlyList<(
        string Row,
        double UpperDecibels,
        int UpperDecoded,
        double LowerDecibels,
        int LowerDecoded)> ThePanel =>
    [
        ("single slot", -21.0, 13, -22.0, 0),
        ("single + OSD", -21.0, 33, -22.0, 1),
        ("summed x4", -21.0, 254, -22.0, 43),
    ];

    /// <summary>
    /// <b>The band brackets the point, is open where a bound curve never reaches 50 per cent, and
    /// reproduces the one crossing this project has already published by hand.</b>
    /// </summary>
    [Fact]
    public void TheCrossingBandBracketsThePointAndIsBuiltFromTheRungsOwnIntervals()
    {
        var lines = new List<string>();

        void Say(string line)
        {
            lines.Add(line);
            output.WriteLine(line);
        }

        Say(
            "UNIT 256, TASK 2: every crossing unit 255 published, with the band the two rungs it "
            + "was interpolated between actually support.");
        Say(
            "THE BAND IS NOT A CONFIDENCE INTERVAL ON THE CROSSING. It is obtained by pushing each "
            + "rung's 95 per cent Wilson bounds through the SAME linear interpolation the point "
            + "crossing uses, under the assumption unit 255 section 4.1 already makes - that the "
            + "decode rate moves linearly in decibels between two rungs one decibel apart.");
        Say(
            "A SIDE THAT DOES NOT REACH 50 PER CENT INSIDE THE BRACKET IS OPEN and is never "
            + "extrapolated. Ruling 2 of unit 256, and unit 255's ruling 3 before it.");
        Say(
            $"SOURCE: committed counts from docs/unit255-runs/, {Trials} trials a rung, one "
            + "signal, no neighbour, Ft8Sharp.Deep 0.8.0. NO LADDER IS WALKED BY THIS TEST.");
        Say(string.Empty);

        var bands = new List<(string Column, string Placement, Ft8Unit256CrossingBand.Band Band)>();

        Say(Ft8Unit256CrossingBand.Header);

        foreach (var (column, placement, atMinus19, atMinus20) in Published)
        {
            var band = Ft8Unit256CrossingBand.Crossing(
                new Ft8Unit256CrossingBand.Rung(-19.0, atMinus19, Trials),
                new Ft8Unit256CrossingBand.Rung(-20.0, atMinus20, Trials));

            bands.Add((column, placement, band));
            Say(Ft8Unit256CrossingBand.AsRow(column, placement, band));
        }

        Say(string.Empty);
        Say(
            "AND THE FOUR CELL-CENTRE COLUMNS UNIT 255 COULD ONLY MARK \"not bracketed\", "
            + $"BRACKETED TONIGHT. Every rung {Trials} trials at the cell centre, +1.56 Hz "
            + "and +480 samples. NO 51-TRIAL COARSE RUNG IS QUOTED HERE.");
        Say(Ft8Unit256CrossingBand.Header);

        foreach (var row in BracketedTonight)
        {
            var band = Ft8Unit256CrossingBand.Crossing(
                new Ft8Unit256CrossingBand.Rung(row.UpperDecibels, row.UpperDecoded, Trials),
                new Ft8Unit256CrossingBand.Rung(row.LowerDecibels, row.LowerDecoded, Trials));

            bands.Add((row.Column, row.Placement, band));
            Say(Ft8Unit256CrossingBand.AsRow(row.Column, row.Placement, band));
        }

        Say(string.Empty);
        Say("WHERE EACH OF THOSE FOUR RUNGS CAME FROM:");

        foreach (var row in BracketedTonight)
        {
            Say(
                $"  {row.Column,-20} {row.UpperDecibels,6:F1} dB and {row.LowerDecibels,6:F1} dB   "
                + row.Source);
        }

        Say(string.Empty);
        Say(
            "AND COMBINING'S OWN PANEL - A DIFFERENT LADDER, AND NO ROW HERE IS COMPARABLE WITH "
            + "ANY ROW ABOVE. Ft8LadderHarness.RunRepeats, FOUR slots a trial carrying the same "
            + "message, jittered 2.00 Hz and 480 samples between hearings. Unit 255 section 5.0.");
        Say(
            "  -21 dB is unit 255 section 5.3's row, CITED and not re-run; -22 dB is unit 256's "
            + "(docs/unit256-runs/combining-panel-minus22.txt). Both 306 trials, Ft8Sharp.Deep "
            + "0.8.0.");
        Say(
            "  THE CAVEAT TRAVELS WITH ALL THREE: a four-repeat column gets four single-slot "
            + "attempts as well as deeper sums.");
        Say(Ft8Unit256CrossingBand.Header);

        foreach (var row in ThePanel)
        {
            var band = Ft8Unit256CrossingBand.Crossing(
                new Ft8Unit256CrossingBand.Rung(row.UpperDecibels, row.UpperDecoded, Trials),
                new Ft8Unit256CrossingBand.Rung(row.LowerDecibels, row.LowerDecoded, Trials));

            bands.Add((row.Row, "repeats x4", band));
            Say(Ft8Unit256CrossingBand.AsRow(row.Row, "repeats x4", band));

            if (!band.Bracketed)
            {
                Say(
                    $"    {row.Row}: NOT BRACKETED - {row.UpperDecoded} of {Trials} at "
                    + $"{row.UpperDecibels:F1} dB is already BELOW 50 per cent, so its crossing "
                    + $"lies ABOVE {row.UpperDecibels:F1} dB, which is this panel's own ceiling. "
                    + "The panel was walked DOWNWARD from -21 dB because that is where combining's "
                    + "crossing is, and it was not walked above it. NOTHING IS EXTRAPOLATED.");
            }
        }

        Say(string.Empty);
        Say("WHAT UNIT 255 PUBLISHED, BESIDE WHAT THE ARITHMETIC RETURNS:");
        Say("column               placement     published    computed    agrees");

        var publishedPoints = new[]
        {
            -19.54, -19.54, -19.66, -19.81, -19.90, -19.54, -19.61, -19.61,
        };

        // ONLY THE EIGHT UNIT 255 PUBLISHED. The four bracketed tonight had no published value to
        // agree with - that is the whole reason they were measured.
        for (var i = 0; i < publishedPoints.Length; i++)
        {
            var (column, placement, band) = bands[i];
            var agrees = Math.Abs(Math.Round(band.Point, 2) - publishedPoints[i]) < 0.005;
            Say(
                $"{column,-20} {placement,-12} {publishedPoints[i],9:F2}  {band.Point,10:F4}  "
                + $"{(agrees ? "yes" : "*** NO ***"),10}");
        }

        Say(string.Empty);
        Say("THE OPEN SIDES, AND WHY EACH ONE IS OPEN:");

        foreach (var (column, placement, band) in bands)
        {
            if (!band.Bracketed)
            {
                continue;
            }

            var (_, hiLower) = band.Lower.Interval;
            var (loUpper, _) = band.Upper.Interval;

            if (band.OptimisticOpen)
            {
                Say(
                    $"  {column} {placement}: the {band.Lower.Decibels:F1} dB rung's Wilson UPPER "
                    + $"bound is {hiLower:F3} %, still above 50, so the optimistic curve never "
                    + $"crosses inside [{band.Lower.Decibels:F1}, {band.Upper.Decibels:F1}]. "
                    + "OPEN, and not extrapolated.");
            }

            if (band.PessimisticOpen)
            {
                Say(
                    $"  {column} {placement}: the {band.Upper.Decibels:F1} dB rung's Wilson LOWER "
                    + $"bound is {loUpper:F3} %, already below 50, so the pessimistic curve never "
                    + $"crosses inside the bracket. OPEN, and not extrapolated.");
            }

            if (!band.OptimisticOpen && !band.PessimisticOpen)
            {
                Say(
                    $"  {column} {placement}: CLOSED on both sides, {band.WidthDecibels:F3} dB "
                    + "wide.");
            }
        }

        Say(string.Empty);
        Say("THE HAND-COMPUTED CASE, from docs/unit256-crossings-and-combining.md section 2.2:");

        var shipping = bands.Single(b => b is { Column: "SHIPPING", Placement: "on grid" }).Band;
        var (loHi19, hiHi19) = shipping.Upper.Interval;
        var (loHi20, hiHi20) = shipping.Lower.Interval;

        Say($"  283 of 306 = {shipping.Upper.Rate:F5} %, Wilson {loHi19:F3} - {hiHi19:F3}");
        Say($"  138 of 306 = {shipping.Lower.Rate:F5} %, Wilson {loHi20:F3} - {hiHi20:F3}");
        Say(
            $"  point       = -19 - ({shipping.Upper.Rate:F5} - 50) / ({shipping.Upper.Rate:F5} - "
            + $"{shipping.Lower.Rate:F5}) = {shipping.Point:F6} dB");
        Say(
            $"  pessimistic = -19 - ({loHi19:F3} - 50) / ({loHi19:F3} - {loHi20:F3}) = "
            + $"{shipping.Pessimistic:F6} dB");
        Say(
            $"  optimistic  = the {hiHi20:F3} % upper bound at -20 dB never falls below 50, so "
            + "this side is OPEN beyond -20 dB");
        Say("  BY HAND, task 1: point -19.90 dB, band open beyond -20 dB to -19.79 dB.");
        Say(string.Empty);
        Say(
            "AND WHY THE PAIRING IS THE THING WORTH GETTING RIGHT - the same cell with the bounds "
            + "paired the WRONG way round, upper at one rung against lower at the other:");

        var wrongOptimistic = -19.0 - ((hiHi19 - 50.0) / (hiHi19 - loHi20));
        var wrongPessimistic = -19.0 - ((loHi19 - 50.0) / (loHi19 - hiHi20));

        Say(
            $"  hi(-19)={hiHi19:F3} against lo(-20)={loHi20:F3} gives {wrongOptimistic:F3} dB, "
            + $"which is WORSE than the point crossing {shipping.Point:F3} dB it is meant to "
            + "bracket");
        Say(
            $"  lo(-19)={loHi19:F3} against hi(-20)={hiHi20:F3} gives {wrongPessimistic:F3} dB, "
            + "which is outside the bracket entirely");
        Say(
            "  so the wrongly-paired band is INVERTED and contains nothing at all. That is unit "
            + "256's watched failure and it is why this file exists.");

        var folder = Path.Combine(Ft8CaptureFixtures.RepositoryRoot(), "docs", "unit256-runs");
        Directory.CreateDirectory(folder);
        File.WriteAllLines(Path.Combine(folder, "crossing-bands.txt"), lines);

        // EVERYTHING IS PRINTED AND WRITTEN BEFORE ANYTHING IS ASSERTED. A measurement that dies
        // on an assertion and takes its own numbers with it has cost the night for nothing.

        // ASSERTION ONE OF THREE: the point crossing lies inside its own band, on every row that
        // HAS one. A row whose two rungs do not straddle 50 per cent has no crossing and no band,
        // and "not bracketed" is a result rather than a failure - unit 255's ruling 3.
        foreach (var (column, placement, band) in bands.Where(b => b.Band.Bracketed))
        {
            Assert.True(
                band.ContainsPoint,
                $"{column} {placement}: the point crossing {band.Point:F4} dB is NOT inside the "
                    + $"band {band.BandText}. A band built from the two rungs' own Wilson bounds "
                    + "must contain the crossing built from their point rates, because the "
                    + "crossing's position in the bracket increases in both rung rates. A band "
                    + "that fails this is not bracketing anything and must not be published.");
        }

        // ASSERTION TWO OF THREE: a side is open exactly when its bound curve does not reach 50
        // per cent inside the bracket - checked on both branches, one row of each.
        var shippingRow = bands.Single(b => b is { Column: "SHIPPING", Placement: "on grid" }).Band;
        var portRow = bands.Single(b => b is { Column: "Ft8Sharp", Placement: "on grid" }).Band;
        var (_, shippingHiLower) = shippingRow.Lower.Interval;
        var (_, portHiLower) = portRow.Lower.Interval;

        Assert.True(
            shippingRow.OptimisticOpen
                && !shippingRow.PessimisticOpen
                && !portRow.OptimisticOpen
                && !portRow.PessimisticOpen,
            $"SHIPPING on grid has a -20 dB Wilson upper bound of {shippingHiLower:F3} %, which is "
                + "above 50, so its optimistic side MUST be open and its pessimistic side must "
                + $"not be; Ft8Sharp on grid has {portHiLower:F3} %, which is below 50, so neither "
                + "of its sides may be open. Got SHIPPING "
                + $"({shippingRow.OptimisticOpen}, {shippingRow.PessimisticOpen}) and Ft8Sharp "
                + $"({portRow.OptimisticOpen}, {portRow.PessimisticOpen}).");

        // ASSERTION THREE OF THREE: the hand-computed case, to two decimals. Task 1 item 2 did
        // this arithmetic in prose before any code ran; this is the code agreeing with it.
        Assert.True(
            Math.Abs(Math.Round(shippingRow.Point, 2) - -19.90) < 0.005
                && Math.Abs(Math.Round(shippingRow.Pessimistic, 2) - -19.79) < 0.005,
            "SHIPPING on grid was computed by hand in docs/unit256-crossings-and-combining.md "
                + "section 2.2 as a point crossing of -19.90 dB and a pessimistic end of "
                + $"-19.79 dB, from 283 and 138 of 306. This returned {shippingRow.Point:F4} and "
                + $"{shippingRow.Pessimistic:F4}.");

        // Nothing asserts that any crossing OUGHT to be at any ratio. Targets are waypoints.
    }
}
