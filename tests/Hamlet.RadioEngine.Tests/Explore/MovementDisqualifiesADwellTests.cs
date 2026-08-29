using Hamlet.RadioEngine.Explore;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// The dial has to come to rest before entering a block means anything.
/// </summary>
/// <remarks>
/// <para>**MOVEMENT DISQUALIFIES, NOT POSITION** (work instruction 050, task 4).
/// A slow tune sits inside a three-kilohertz block for longer than a second while
/// still moving, and the operator crosses data territory every time he scans from
/// Morse up to voice — so a rule written on time-in-block would fire on both.</para>
/// <para>Everything here advances its own clock by hand, because the type takes
/// the time as an argument and reads no clock of its own (§5.4).</para>
/// </remarks>
public sealed class MovementDisqualifiesADwellTests
{
    private static readonly DateTime Start = new(2026, 8, 29, 12, 0, 0, DateTimeKind.Utc);

    private readonly ITestOutputHelper _output;

    public MovementDisqualifiesADwellTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>A dial that stops matures after a second and fires once.</summary>
    [Fact]
    public void AStillDialMaturesOnceAndOnlyOnce()
    {
        var dwell = ModeDwell.Nowhere;
        var fired = 0;

        for (var tick = 0; tick <= 12; tick++)
        {
            var at = Start.AddMilliseconds(tick * 250);

            (dwell, var matured) = dwell.Observe("FT8", 14_074_000, at, scanning: false);

            if (matured)
            {
                fired++;

                _output.WriteLine($"matured at {tick * 250} ms");
            }
        }

        Assert.Equal(1, fired);
    }

    /// <summary>
    /// A dial creeping through the block never matures, however long it is inside.
    /// </summary>
    /// <remarks>
    /// **THIS IS THE CASE THE RULE EXISTS FOR.** Three kilohertz at a slow tune is
    /// several seconds inside the block, and none of it is an arrival.
    /// </remarks>
    [Fact]
    public void ASlowTuneThroughTheBlockNeverMatures()
    {
        var dwell = ModeDwell.Nowhere;
        var hz = 14_074_000L;

        for (var tick = 0; tick < 40; tick++)
        {
            var at = Start.AddMilliseconds(tick * 250);

            // Fifty hertz every quarter second: ten seconds to cross three
            // kilohertz, and never still.
            hz += 50;

            (dwell, var matured) = dwell.Observe("FT8", hz, at, scanning: false);

            Assert.False(
                matured,
                $"a dial still moving matured at {tick * 250} ms, {hz} Hz");
        }
    }

    /// <summary>Nothing matures while the scanner is running.</summary>
    /// <remarks>
    /// A scan moves the dial on its own, so every block it crosses would look
    /// like an arrival.
    /// </remarks>
    [Fact]
    public void AScanMaturesNothing()
    {
        var dwell = ModeDwell.Nowhere;

        for (var tick = 0; tick <= 20; tick++)
        {
            var at = Start.AddMilliseconds(tick * 250);

            (dwell, var matured) = dwell.Observe("FT8", 14_074_000, at, scanning: true);

            Assert.False(matured, "a dwell matured while the scanner was running");
        }
    }

    /// <summary>Leaving and coming back starts the second again.</summary>
    [Fact]
    public void LeavingAndReturningReArmsFromZero()
    {
        var dwell = ModeDwell.Nowhere;

        // Most of a second in the block.
        (dwell, _) = dwell.Observe("FT8", 14_074_000, Start, false);
        (dwell, var early) = dwell.Observe(
            "FT8", 14_074_000, Start.AddMilliseconds(900), false);

        Assert.False(early);

        // Out, then straight back in.
        (dwell, _) = dwell.Observe("CW", 14_030_000, Start.AddMilliseconds(950), false);
        (dwell, var onReturn) = dwell.Observe(
            "FT8", 14_074_000, Start.AddMilliseconds(1_000), false);

        Assert.False(onReturn, "coming back matured instantly");

        // And it takes a fresh second from the moment of return.
        (dwell, var later) = dwell.Observe(
            "FT8", 14_074_000, Start.AddMilliseconds(2_100), false);

        Assert.True(later, "a full second after returning did not mature");
    }

    /// <summary>Leaving before maturity says nothing at all.</summary>
    /// <remarks>
    /// A write that did not happen is not narrated. The dwell simply resets, and
    /// there is no state left behind to report.
    /// </remarks>
    [Fact]
    public void LeavingEarlyDiscardsSilently()
    {
        var dwell = ModeDwell.Nowhere;

        (dwell, _) = dwell.Observe("FT8", 14_074_000, Start, false);
        (dwell, var matured) = dwell.Observe(
            "CW", 14_030_000, Start.AddMilliseconds(500), false);

        Assert.False(matured);
        Assert.Equal("CW", dwell.Block);
    }

    /// <summary>Off the map matures nothing.</summary>
    [Fact]
    public void OffTheMapMaturesNothing()
    {
        var dwell = ModeDwell.Nowhere;

        (dwell, _) = dwell.Observe("", 14_074_000, Start, false);
        (dwell, var matured) = dwell.Observe(
            "", 14_074_000, Start.AddSeconds(5), false);

        Assert.False(matured);
    }

    /// <summary>A second exactly is enough; a whisker under is not.</summary>
    [Fact]
    public void TheBoundaryIsWhereItSays()
    {
        var dwell = ModeDwell.Nowhere;

        (dwell, _) = dwell.Observe("FT8", 14_074_000, Start, false);

        var (_, justUnder) = dwell.Observe(
            "FT8", 14_074_000, Start.AddMilliseconds(999), false);

        var (_, exactly) = dwell.Observe(
            "FT8", 14_074_000, Start.Add(ModeDwell.Matures), false);

        Assert.False(justUnder);
        Assert.True(exactly);
    }
}
