using Ft8Sharp.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>THE 50 PER CENT CROSSING FOR EACH OF TONIGHT'S SIX COLUMNS, AT BOTH PLACEMENTS.</b>
/// Step 6's second exit asks for the crossing of each configuration, interpolated, with its
/// interval. Unit 256 met it for the columns then measured; this adds the six columns unit 257's
/// zero-jitter panel measured, in the same form and with the same arithmetic.
/// </summary>
/// <remarks>
/// <para>
/// <b>NO NEW ARITHMETIC, AND THAT IS WHY THIS CLASS ADDS NO GATE-SET ENTRY AND NO WATCHED
/// FAILURE.</b> Every band below is computed by <c>Ft8Unit256CrossingBand.Crossing</c>, which is
/// already in the tree, already in the gate set at entry 13, and already watched failing once as
/// <c>B18</c> — <em>a crossing's band built from the wrong pair of bounds, inverted on every
/// row</em>. <b>This class is a caller of that arithmetic and not a second copy of it.</b> The
/// instruction's rule 5 — no test added without naming the breakage it would have caught —
/// bites on new arithmetic, and a ladder walk is a measurement rather than a test
/// (<c>docs/gate-set.md</c> line 57).
/// </para>
/// <para>
/// <b>EVERY COUNT BELOW IS TRANSCRIBED FROM A COMMITTED ARTEFACT UNDER
/// <c>docs/unit257-runs/</c>, NEVER FROM A CONSOLE BUFFER</b>, and the artefact each count came
/// from is named on its own line beside it. The walks are in
/// <c>Ft8Unit257PlacementPanelTests</c>; this class walks nothing and takes seconds.
/// </para>
/// <para>
/// <b>THE BAND IS NOT A CONFIDENCE INTERVAL ON THE CROSSING.</b> It is the two rungs' own 95 per
/// cent Wilson bounds pushed through the same linear interpolation the point crossing uses,
/// under the stated assumption that the decode rate moves linearly in decibels between two rungs
/// one decibel apart. Nothing here may call it anything else.
/// </para>
/// <para>
/// <b>NOTHING IS ASSERTED ABOUT WHERE ANY CROSSING LIES.</b> Targets are waypoints and a step
/// closes on the figure it reached. The one assertion is <c>Band.ContainsPoint</c> — that each
/// published band actually brackets its own point crossing — which is entry 13's invariant and
/// the property <c>B18</c> violated, and it is a statement about the construction rather than a
/// bound on any result.
/// </para>
/// <para>
/// <b>A COLUMN THE THREE RUNGS DO NOT STRADDLE IS REPORTED AS NOT BRACKETED, WITH ITS DIRECTION
/// AND THE CEILING RUNG NAMED, AND IS NEVER EXTRAPOLATED.</b> Unit 255's ruling 3.
/// </para>
/// </remarks>
public class Ft8Unit257CrossingTests(ITestOutputHelper output)
{
    private const int Trials = 306;

    /// <summary>
    /// <b>One column's counts as they were walked</b>, each transcribed from the named artefact.
    /// </summary>
    /// <param name="Decibels">The rung, in decibels in the 2500 Hz reference bandwidth.</param>
    /// <param name="Decoded">Trials that returned the message that was sent.</param>
    /// <param name="Artefact">The committed file this count was read out of.</param>
    private readonly record struct Walked(double Decibels, int Decoded, string Artefact);

    // ------------------------------------------------------------------------------------------
    // ON THE GRID - 1000.0 Hz, three whole symbol periods in. Zero jitter.
    // ------------------------------------------------------------------------------------------
    private static readonly Walked GridMinus19Port = new(-19.0, 248, "placement-panel-on-grid-minus19");
    private static readonly Walked GridMinus20Port = new(-20.0, 73, "placement-panel-on-grid-minus20");

    private static readonly Walked GridMinus19Osd = new(-19.0, 276, "placement-panel-on-grid-minus19");
    private static readonly Walked GridMinus20Osd = new(-20.0, 125, "placement-panel-on-grid-minus20");

    // The three rungs the closing table quotes all read 306 of 306 for summed x4 on the grid, so
    // the crossing lies below -21 dB and the downward extension was spent to bracket it. That
    // decision is docs/unit257-combining-placement.md section 3.1, taken at the start of task 4.
    private static readonly Walked GridMinus22Summed = new(-22.0, 239, "extension-on-grid-minus22");
    private static readonly Walked GridMinus23Summed = new(-23.0, 29, "extension-on-grid-minus23");

    // ------------------------------------------------------------------------------------------
    // AT THE CELL CENTRE - +1.56 Hz, +480 samples. Zero jitter.
    // ------------------------------------------------------------------------------------------
    private static readonly Walked CentreMinus19Port = new(-19.0, 6, "placement-panel-cell-centre-minus19");
    private static readonly Walked CentreMinus19Osd = new(-19.0, 33, "placement-panel-cell-centre-minus19");

    private static readonly Walked CentreMinus20Summed = new(-20.0, 270, "placement-panel-cell-centre-minus20");
    private static readonly Walked CentreMinus21Summed = new(-21.0, 75, "placement-panel-cell-centre-minus21");

    private static Ft8Unit256CrossingBand.Rung Rung(Walked walked) =>
        new(walked.Decibels, walked.Decoded, Trials);

    /// <summary>
    /// <b>The whole crossing table for unit 257's panel, computed and printed.</b>
    /// </summary>
    [Fact]
    public void TheCrossingsForTonightsSixColumns()
    {
        var lines = new List<string>();

        void Say(string line)
        {
            lines.Add(line);
            output.WriteLine(line);
        }

        Say("UNIT 257, TASK 4, THE 50 PER CENT CROSSINGS FOR THE ZERO-JITTER PLACEMENT PANEL.");
        Say(
            "LADDER: Ft8LadderHarness.RunRepeats, four slots a trial, JITTER ZERO on both axes, "
            + "306 trials a rung, Ft8Sharp.Deep 0.8.0. NO ROW HERE IS COMPARABLE WITH A ROW IN "
            + "section 3 of the closing document, which gives each trial ONE slot, NOR WITH A ROW "
            + "IN section 5.4, which is unit 256's JITTERED panel.");
        Say(
            "ARITHMETIC: Ft8Unit256CrossingBand.Crossing - already in the tree, already gate-set "
            + "entry 13, already watched failing once as B18. NO SECOND CROSSING HELPER IS "
            + "WRITTEN and no new watched failure is manufactured.");
        Say(
            "THE BAND IS NOT A CONFIDENCE INTERVAL ON THE CROSSING. It is the two rungs' own 95 "
            + "per cent Wilson bounds pushed through the same linear interpolation the point "
            + "crossing uses, under a stated assumption that the rate moves linearly in decibels "
            + "between two rungs one decibel apart.");
        Say(
            "EVERY COUNT IS TRANSCRIBED FROM A COMMITTED ARTEFACT UNDER docs/unit257-runs/, "
            + "named beside it below. NOTHING IS ASSERTED ABOUT WHERE ANY CROSSING LIES.");
        Say(string.Empty);

        var published = new List<(string Column, string Placement, Ft8Unit256CrossingBand.Band Band)>();

        void Bracketed(string column, string placement, Walked upper, Walked lower)
        {
            var band = Ft8Unit256CrossingBand.Crossing(Rung(upper), Rung(lower));
            published.Add((column, placement, band));

            Say($"SOURCE  {column,-14} {placement,-12} upper {upper.Decibels,6:F1} dB "
                + $"{upper.Decoded,4} of {Trials} from docs/unit257-runs/{upper.Artefact}.txt");
            Say($"SOURCE  {column,-14} {placement,-12} lower {lower.Decibels,6:F1} dB "
                + $"{lower.Decoded,4} of {Trials} from docs/unit257-runs/{lower.Artefact}.txt");
        }

        Bracketed("single slot", "on grid", GridMinus19Port, GridMinus20Port);
        Bracketed("single + OSD", "on grid", GridMinus19Osd, GridMinus20Osd);
        Bracketed("summed x4", "on grid", GridMinus22Summed, GridMinus23Summed);
        Bracketed("summed x4", "cell centre", CentreMinus20Summed, CentreMinus21Summed);

        Say(string.Empty);
        Say(Ft8Unit256CrossingBand.Header);

        foreach (var (column, placement, band) in published)
        {
            Say(Ft8Unit256CrossingBand.AsRow(column, placement, band));
        }

        Say(string.Empty);
        Say("THE TWO COLUMNS THE TABLE'S THREE RUNGS DO NOT STRADDLE, WITH THEIR DIRECTION AND");
        Say("THE CEILING RUNG NAMED. NOTHING IS EXTRAPOLATED - unit 255's ruling 3.");
        Say(
            $"  single slot    cell centre   NOT BRACKETED - ABOVE -19.0 dB. It reads "
            + $"{CentreMinus19Port.Decoded} of {Trials} at -19.0 dB, the SHALLOWEST rung this "
            + "panel walked, and is therefore already below 50 per cent at the top of the "
            + "bracket. The downward extension rung cannot reach it and walking UP the ladder is "
            + "not licensed by this instruction, so -19.0 dB is the stated ceiling.");
        Say(
            $"  single + OSD   cell centre   NOT BRACKETED - ABOVE -19.0 dB. It reads "
            + $"{CentreMinus19Osd.Decoded} of {Trials} at -19.0 dB, same reasoning, same stated "
            + "ceiling.");
        Say(string.Empty);
        Say(
            "AND THE ONE COLUMN THE TABLE'S OWN RUNGS COULD NOT BRACKET DOWNWARD, WHICH THE");
        Say("EXTENSION WAS SPENT ON:");
        Say(
            "  summed x4      on grid       reads 306 of 306 at -19, -20 AND -21 dB - saturated "
            + "at every rung the closing table quotes. Its crossing lies BELOW -21 dB, and the "
            + "downward extension at -22.0 dB (239 of 306) and -23.0 dB (29 of 306) brackets it. "
            + "The search was capped at -23 dB and did not need the cap.");

        Say(string.Empty);
        Say("EVERY PUBLISHED BAND CONTAINS ITS OWN POINT CROSSING (gate-set entry 13, the");
        Say("property B18 violated):");

        foreach (var (column, placement, band) in published)
        {
            Say($"  {column,-14} {placement,-12} point {band.Point,8:F2} dB   band "
                + $"{band.BandText,-34} contains point: {band.ContainsPoint}");
        }

        var folder = Path.Combine(Ft8CaptureFixtures.RepositoryRoot(), "docs", "unit257-runs");
        Directory.CreateDirectory(folder);
        File.WriteAllLines(Path.Combine(folder, "crossing-bands.txt"), lines);

        // THE ONE ASSERTION, AND IT IS NOT A BOUND ON ANY CROSSING. Entry 13's invariant: a band
        // this project publishes must bracket the point crossing it is built around. B18 is the
        // pairing that makes it false.
        foreach (var (column, placement, band) in published)
        {
            Assert.True(
                band.Bracketed,
                $"{column} at {placement} was fed to Crossing as a bracketed pair and came back "
                    + "not bracketed, which means the two rungs transcribed above do not straddle "
                    + "50 per cent. Check the counts against the artefacts named on the SOURCE "
                    + "lines.");

            Assert.True(
                band.ContainsPoint,
                $"{column} at {placement} produced a band that does not contain its own point "
                    + $"crossing of {band.Point:F2} dB - the band reads {band.BandText}. That is "
                    + "B18 in docs/breakage-record.md and gate-set entry 13, and a band that does "
                    + "not bracket its own crossing may not be published.");
        }
    }
}
