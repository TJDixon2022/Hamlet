using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Explore;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// A dial passing through data territory sets no modes on the way.
/// </summary>
/// <remarks>
/// <para>**THIS IS THE COMPOSED RULE, NOT EITHER HALF OF IT** (work instruction
/// 050, tasks 4 and 5). <see cref="ModeDwell"/> says whether the dial has come to
/// rest and <see cref="ModeFollowPlan.WaitsForDwell"/> says which blocks care;
/// each is provable alone and neither alone proves the thing the operator
/// notices, which is his radio being left in USB-D somewhere he was only passing
/// through.</para>
/// <para>**THE MAP IS THE REAL ONE.** Forty metres carries three digital blocks
/// nose to tail — PSK31 at 7.070, FT8 at 7.074, JS8 at 7.078 — so a tune from
/// Morse main street up toward the phone portion crosses all three, which is the
/// journey the order names.</para>
/// </remarks>
public sealed class AScanCrossingDataWritesNothingTests
{
    private static readonly DateTime Start = new(2026, 8, 29, 12, 0, 0, DateTimeKind.Utc);

    private readonly ITestOutputHelper _output;

    public AScanCrossingDataWritesNothingTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>Three data blocks crossed at a tuning speed, and no write.</summary>
    /// <remarks>
    /// **A HUNDRED HERTZ EVERY QUARTER SECOND IS A SLOW, DELIBERATE TUNE**, and
    /// it spends about ten seconds inside each three-kilohertz block. A rule
    /// written on time-in-block would fire three times on this pass.
    /// </remarks>
    [Fact]
    public void ASlowTuneAcrossThreeDataBlocksWritesNothing()
    {
        var crossed = Sweep(7_069_000, 7_082_000, stepHz: 100, scanning: false);

        Assert.True(
            crossed.Blocks.Count >= 3,
            $"the sweep only crossed {crossed.Blocks.Count} blocks: "
            + string.Join(", ", crossed.Blocks));

        _output.WriteLine(
            $"crossed {string.Join(", ", crossed.Blocks)} — {crossed.Writes} writes");

        Assert.Equal(0, crossed.Writes);
    }

    /// <summary>The scanner crosses them faster still, and writes nothing.</summary>
    /// <remarks>
    /// **SUPPRESSED ENTIRELY WHILE THE SCANNER RUNS** (task 4). §0.2.1 already
    /// has a scan putting the dial back where it found it; a scan that
    /// reconfigured the radio at every block on the way past would leave the
    /// frequency restored and the mode not.
    /// </remarks>
    [Fact]
    public void AScanAcrossThreeDataBlocksWritesNothing()
    {
        var crossed = Sweep(7_069_000, 7_082_000, stepHz: 500, scanning: true);

        _output.WriteLine(
            $"scan crossed {string.Join(", ", crossed.Blocks)} — "
            + $"{crossed.Writes} writes");

        Assert.Equal(0, crossed.Writes);
    }

    /// <summary>
    /// A scan that stops dead inside a data block still writes nothing while it
    /// is running.
    /// </summary>
    /// <remarks>
    /// The suppression is on the scanner and not on the movement, because a scan
    /// pausing on a signal is the scanner's business and not an arrival.
    /// </remarks>
    [Fact]
    public void AScanParkedInsideDataWritesNothing()
    {
        var dwell = ModeDwell.Nowhere;
        var writes = 0;

        for (var tick = 0; tick <= 40; tick++)
        {
            (dwell, var matured) = dwell.Observe(
                "FT8 city", 7_075_000, Start.AddMilliseconds(tick * 250), scanning: true);

            if (matured)
            {
                writes++;
            }
        }

        Assert.Equal(0, writes);
    }

    /// <summary>And a dial that stops in the same block writes exactly once.</summary>
    /// <remarks>
    /// **THE CONTROL FOR ALL THREE.** Without it the tests above would pass on a
    /// rule that never writes at all, which is §12.5's own failure: a green suite
    /// certifying a capability nobody has.
    /// </remarks>
    [Fact]
    public void ADialThatStopsInDataWritesExactlyOnce()
    {
        var hood = Data(7_075_000);

        Assert.True(ModeFollowPlan.WaitsForDwell(ModeFollowPlan.TargetFor(hood)));

        var dwell = ModeDwell.Nowhere;
        var writes = 0;

        for (var tick = 0; tick <= 40; tick++)
        {
            (dwell, var matured) = dwell.Observe(
                hood!.Name, 7_075_000, Start.AddMilliseconds(tick * 250),
                scanning: false);

            if (matured)
            {
                writes++;
            }
        }

        _output.WriteLine($"stopped in {hood!.Name}: {writes} write(s)");

        Assert.Equal(1, writes);
    }

    /// <summary>Tune from one frequency to another and count what would be written.</summary>
    private (int Writes, List<string> Blocks) Sweep(
        long fromHz, long toHz, long stepHz, bool scanning)
    {
        var dwell = ModeDwell.Nowhere;
        var writes = 0;
        var blocks = new List<string>();
        var tick = 0;

        for (var hz = fromHz; hz <= toHz; hz += stepHz)
        {
            var hood = Data(hz);
            var name = hood?.Name ?? "";

            if (name.Length > 0 && (blocks.Count == 0 || blocks[^1] != name))
            {
                blocks.Add(name);
            }

            (dwell, var matured) = dwell.Observe(
                name, hz, Start.AddMilliseconds(tick * 250), scanning);

            tick++;

            if (matured && ModeFollowPlan.WaitsForDwell(ModeFollowPlan.TargetFor(hood)))
            {
                writes++;
            }
        }

        return (writes, blocks);
    }

    /// <summary>The neighborhood at this frequency on 40 m, or null.</summary>
    private static Neighborhood? Data(long hz)
    {
        foreach (var hood in NeighborhoodPlan.ForBand(HfBands.Bands.Single(
            b => b.Name == "40 m")))
        {
            if (hood.Contains(hz))
            {
                return hood;
            }
        }

        return null;
    }
}
