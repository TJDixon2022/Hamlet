using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Explore;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// How much of the map the CW-segment guard silences.
/// </summary>
/// <remarks>
/// <para>**THE BLAST RADIUS** (work instruction 051, task 1). Mode-follow asks
/// `IsInsideCwSegment` whether the operator is working Morse, and a CW segment in
/// this tree is derived from the emission ranges carrying `TransmitMode.Data` in
/// 47 CFR 97.305(c) — **it is the CW *and data* segment**, which is the same
/// stretch of band the digital watering holes live in. That is what they are.</para>
/// <para>So for a digital block the target is USB-D, the target is not CW,
/// `workingCw` is true on the segment test alone whether or not anything is
/// decoding, and the decision is `Nothing`. Silently.</para>
/// <para>**THIS COUNTS IT RATHER THAN ARGUING IT**, and it walks the real map
/// against the real band plan, because a count taken from the order would be the
/// order marking its own homework (§12.5).</para>
/// </remarks>
public sealed class TheGuardSilencesEveryDigitalBlockTests
{
    private readonly ITestOutputHelper _output;

    public TheGuardSilencesEveryDigitalBlockTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>Every row on the map, with the band it belongs to.</summary>
    private static IEnumerable<(CwBand Band, Neighborhood Hood)> WholeMap()
        => HfBands.Bands.SelectMany(
            b => NeighborhoodPlan.ForBand(b).Select(n => (Band: b, Hood: n)));

    /// <summary>
    /// Count the digital rows that sit inside their band's CW segment.
    /// </summary>
    /// <remarks>
    /// **THE NUMBER THIS UNIT IS COMMISSIONED ON.** A row is counted as silenced
    /// when its own jump frequency lies inside the segment, because that is where
    /// the app tunes when the operator presses the block.
    /// </remarks>
    [Fact]
    public void EveryDigitalRowSitsInsideItsBandsCwSegment()
    {
        var digital = 0;
        var silenced = 0;
        var outside = new List<string>();

        foreach (var (band, hood) in WholeMap())
        {
            var target = ModeFollowPlan.TargetFor(hood);

            if (target is null || !target.DataMode)
            {
                continue;
            }

            digital++;

            if (band.IsInCwSegment(hood.JumpHz))
            {
                silenced++;
            }
            else
            {
                outside.Add($"{band.Name} {hood.Name} at {hood.JumpHz} Hz");
            }
        }

        _output.WriteLine($"{silenced} of {digital} digital rows are inside a CW segment");

        foreach (var row in outside)
        {
            _output.WriteLine($"  OUTSIDE: {row}");
        }

        Assert.Equal(28, digital);
        Assert.Equal(digital, silenced);
    }

    /// <summary>
    /// And no digital block so much as straddles a segment edge, so no frequency
    /// inside one escapes the guard.
    /// </summary>
    /// <remarks>
    /// <para>**THE JUMP FREQUENCY IS NOT THE WHOLE BLOCK.** A row whose span
    /// crossed the top of the CW segment would give the operator a stretch of
    /// dial where the write does go out, which would make the fault intermittent
    /// rather than total — a materially different bug, and one that would show up
    /// as "it worked that one time".</para>
    /// <para>It does not happen: **every digital block lies wholly inside its
    /// band's CW segment, both edges.** So "the dial is in a digital block" and
    /// "the guard fires" are the same statement, with no frequency between them.
    /// </para>
    /// </remarks>
    [Fact]
    public void NoDigitalBlockStraddlesASegmentEdge()
    {
        var straddling = new List<string>();
        var whollyInside = 0;

        foreach (var (band, hood) in WholeMap())
        {
            var target = ModeFollowPlan.TargetFor(hood);

            if (target is null || !target.DataMode)
            {
                continue;
            }

            var low = band.IsInCwSegment(hood.LowHz);
            var high = band.IsInCwSegment(hood.HighHz);

            if (low && high)
            {
                whollyInside++;
            }
            else
            {
                straddling.Add(
                    $"{band.Name} {hood.Name} {hood.LowHz}-{hood.HighHz} Hz: "
                    + $"low inside {low}, high inside {high}");
            }
        }

        _output.WriteLine($"{whollyInside} digital blocks lie wholly inside a CW segment");

        foreach (var row in straddling)
        {
            _output.WriteLine($"  STRADDLES: {row}");
        }

        Assert.Empty(straddling);
    }

    /// <summary>The Morse rows, for the other half of the picture.</summary>
    /// <remarks>
    /// **THE CONTROL.** If the segment test caught nothing, or caught everything
    /// including rows the map calls Morse, the count above would mean something
    /// else. It catches all of both, which is exactly the complaint: **the
    /// regulatory segment cannot tell orange from purple, because under 97.305(c)
    /// they are the same segment.**
    /// </remarks>
    [Fact]
    public void TheMorseRowsAreInsideItToo()
    {
        var morse = 0;
        var inside = 0;

        foreach (var (band, hood) in WholeMap())
        {
            var target = ModeFollowPlan.TargetFor(hood);

            if (target is null || target.Mode != CivMode.Cw)
            {
                continue;
            }

            morse++;

            if (band.IsInCwSegment(hood.JumpHz))
            {
                inside++;
            }
        }

        _output.WriteLine($"{inside} of {morse} Morse rows are inside a CW segment");

        Assert.Equal(20, morse);
        Assert.Equal(morse, inside);
    }

    /// <summary>
    /// With the segment as evidence, no digital block can ever be written.
    /// </summary>
    /// <remarks>
    /// **THIS IS THE DECISION AND NOT THE PREDICATE.** The test above measures
    /// where the blocks are; this one runs the real `Decide` with the real
    /// `workingCw` the view model computes at that frequency, and asserts what
    /// the operator actually got: nothing, everywhere, every time.
    /// </remarks>
    [Fact]
    public void NotOneDigitalBlockProducesAWriteWhileTheSegmentIsTheEvidence()
    {
        var wrote = 0;
        var refused = 0;

        foreach (var (band, hood) in WholeMap())
        {
            var target = ModeFollowPlan.TargetFor(hood);

            if (target is null || !target.DataMode)
            {
                continue;
            }

            // Exactly line 5803, with the terminal idle: the most favourable
            // case there is for a write to go out.
            var workingCw = band.IsInCwSegment(hood.JumpHz) || false;

            var decision = ModeFollowPlan.Decide(
                ModeFollowState.Armed(true),
                currentMode: CivMode.Cw,
                currentDataMode: false,
                target,
                hood.JumpHz,
                workingCw);

            if (decision.Write)
            {
                wrote++;

                _output.WriteLine($"  WROTE: {band.Name} {hood.Name}");
            }
            else
            {
                refused++;
            }
        }

        _output.WriteLine($"{refused} refused, {wrote} written");

        Assert.Equal(0, wrote);
    }
}
