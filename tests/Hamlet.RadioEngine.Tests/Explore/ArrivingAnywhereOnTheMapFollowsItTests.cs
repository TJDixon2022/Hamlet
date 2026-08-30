using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Explore;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// Every row on the map, driven the way the application drives it.
/// </summary>
/// <remarks>
/// <para>**THE SEAM THE OLD TESTS WERE ON THE WRONG SIDE OF** (work instruction
/// 051, task 3). Every mode-follow test in this repository handed `workingCw` to
/// `Decide` by hand. `ArrivingInADigitalBlockDoingNothingElseStillFollows` passed
/// `false` at 14.074 MHz and asserted a USB-D write; the running application
/// computed `true` at that frequency and wrote nothing. **The test asserted a
/// state the app could not reach, passed, and the radio stayed in CW for weeks.**
/// </para>
/// <para>So nothing here supplies that value. It is derived from the map, by
/// `ModeFollowPlan.WorkingCw`, which is the expression the view model calls —
/// one expression, not a second copy of it (§0).</para>
/// <para>**AND IT WALKS ALL 79 ROWS RATHER THAN A CHOSEN FEW.** The fault was
/// uniform across 28 blocks and no test noticed, because the tests that existed
/// each named one frequency.</para>
/// </remarks>
public sealed class ArrivingAnywhereOnTheMapFollowsItTests
{
    private readonly ITestOutputHelper _output;

    public ArrivingAnywhereOnTheMapFollowsItTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>Every row on the map, with the band it belongs to.</summary>
    private static IEnumerable<(CwBand Band, Neighborhood Hood)> WholeMap()
        => HfBands.Bands.SelectMany(
            b => NeighborhoodPlan.ForBand(b).Select(n => (Band: b, Hood: n)));

    /// <summary>What the app would decide on arriving at this row's dial.</summary>
    /// <remarks>
    /// The whole of `FollowTheMapAsync`'s decision path, with the radio in CW and
    /// the data flag off — the state the operator was actually in on 2026-08-29.
    /// </remarks>
    private static (ModeFollowDecision Decision, ModeTarget? Target) Arrive(
        Neighborhood hood, bool copying = false)
    {
        var target = ModeFollowPlan.TargetFor(hood);

        return (
            ModeFollowPlan.Decide(
                ModeFollowState.Armed(true),
                currentMode: CivMode.Cw,
                currentDataMode: false,
                target,
                hood.JumpHz,
                ModeFollowPlan.WorkingCw(target, copying)),
            target);
    }

    /// <summary>Arriving in any digital block sets the data mode.</summary>
    /// <remarks>
    /// **THE NUMBER THIS UNIT IS COMMISSIONED ON.** It was 0 of 28 before.
    /// </remarks>
    [Fact]
    public void EveryDigitalBlockGetsItsDataWrite()
    {
        var digital = 0;
        var wrote = 0;
        var refused = new List<string>();

        foreach (var (band, hood) in WholeMap())
        {
            var (decision, target) = Arrive(hood);

            if (target is null || !target.DataMode)
            {
                continue;
            }

            digital++;

            if (decision.Write
                && decision.Mode == CivMode.Usb
                && decision.DataMode)
            {
                wrote++;
            }
            else
            {
                refused.Add($"{band.Name} {hood.Name} at {hood.JumpHz} Hz");
            }
        }

        _output.WriteLine($"{wrote} of {digital} digital blocks get a USB-D write");

        foreach (var row in refused)
        {
            _output.WriteLine($"  REFUSED: {row}");
        }

        Assert.Equal(28, digital);
        Assert.Equal(digital, wrote);
    }

    /// <summary>Arriving in any Morse block asks for CW and no data variant.</summary>
    /// <remarks>
    /// **THE OTHER HALF, AND IT MUST NOT HAVE MOVED.** A change that made the
    /// digital blocks work by making every block digital would satisfy the test
    /// above on its own.
    /// </remarks>
    [Fact]
    public void EveryMorseBlockAsksForCwAndNeverTheDataVariant()
    {
        var morse = 0;
        var wrong = new List<string>();

        foreach (var (band, hood) in WholeMap())
        {
            var (decision, target) = Arrive(hood);

            if (target is null || target.Mode != CivMode.Cw)
            {
                continue;
            }

            morse++;

            // The radio is already in CW here, so the right answer is to leave it
            // alone. What must never happen is a data write.
            if (decision.Write && (decision.Mode != CivMode.Cw || decision.DataMode))
            {
                wrong.Add(
                    $"{band.Name} {hood.Name}: {decision.Mode} data={decision.DataMode}");
            }
        }

        _output.WriteLine($"{morse} Morse blocks, {wrong.Count} asking for the wrong thing");

        foreach (var row in wrong)
        {
            _output.WriteLine($"  WRONG: {row}");
        }

        Assert.Equal(20, morse);
        Assert.Empty(wrong);
    }

    /// <summary>
    /// **THE CONTROL: the terminal actually copying still stops the write.**
    /// </summary>
    /// <remarks>
    /// <para>This is the 2026-08-18 protection, and the whole point of the guard.
    /// Mode-follow wrote USB-D over and over while the operator sat with a signal
    /// decoding, and the send controls refused `not_in_morse` for sixty-six
    /// seconds — he could not answer a station because the app had moved his radio
    /// out from under him.</para>
    /// <para>**Without this test the repair would be indistinguishable from
    /// deleting the guard.** With characters arriving, every one of the 28 digital
    /// blocks must still refuse.</para>
    /// </remarks>
    [Fact]
    public void CopyingMorseInsideADigitalBlockStillRefuses()
    {
        var digital = 0;
        var stillWrote = new List<string>();

        foreach (var (band, hood) in WholeMap())
        {
            var (decision, target) = Arrive(hood, copying: true);

            if (target is null || !target.DataMode)
            {
                continue;
            }

            digital++;

            if (decision.Write)
            {
                stillWrote.Add($"{band.Name} {hood.Name}");
            }
        }

        _output.WriteLine(
            $"{digital} digital blocks with characters arriving, "
            + $"{stillWrote.Count} written");

        foreach (var row in stillWrote)
        {
            _output.WriteLine($"  WROTE ANYWAY: {row}");
        }

        Assert.Equal(28, digital);
        Assert.Empty(stillWrote);
    }

    /// <summary>
    /// The frequency the operator was actually on, named rather than swept.
    /// </summary>
    /// <remarks>
    /// 20 m FT8 on 2026-08-29, where the radio stayed in CW and nothing was said.
    /// The sweeps above would catch it, and a named case is what somebody reads
    /// when they come back to ask what went wrong.
    /// </remarks>
    [Fact]
    public void TwentyMetreFt8OnTheTwentyNinthWritesUsbD()
    {
        var hood = NeighborhoodPlan
            .ForBand(HfBands.Bands.First(b => b.Name == "20 m"))
            .First(n => n.Contains(14_074_000));

        var (decision, _) = Arrive(hood);

        _output.WriteLine(
            $"{hood.Name}: write {decision.Write}, {decision.Mode}, "
            + $"data {decision.DataMode} — {decision.Narration}");

        Assert.True(decision.Write, "20 m FT8 still writes nothing");
        Assert.Equal(CivMode.Usb, decision.Mode);
        Assert.True(decision.DataMode);
    }
}
