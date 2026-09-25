using Xunit;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Work instruction 426 task 2, criterion 7.8 and HM-DEC-179: the preamp is turned off when
/// the receiver reports overloading after the tune-in, not only at it.
/// </summary>
/// <remarks>
/// <para>**WATCHED FAILING FIRST.** At HEAD nothing acts on the live poll
/// (<see cref="LivePollBench.OnPollAsync"/>), so the band that starts overloading after he
/// has tuned in leaves the preamp where the tune-in put it (P22).</para>
/// <para>**HM-DEC-179'S LIMITS ARE WHAT IS TESTED**: the preamp only, on the `Overflow` flag
/// only, never while the radio is transmitting, and never after his hand has moved it since
/// Hamlet's last write, until the next tune-in.</para>
/// <para>Against <see cref="ScriptedRadio"/>, so every result is an indication (FACT-006).</para>
/// </remarks>
public sealed class ThePreampFollowsTheOverloadTests
{
    /// <summary>Several live passes, about three seconds at the measured cadence.</summary>
    private const int Held = 12;

    /// <summary>
    /// **AT 7.030**, where the old rule turned the preamp off by band: the quiet tune-in writes
    /// preamp 1, then an overload that rises and holds is followed by exactly one write of off.
    /// </summary>
    [Fact]
    public async Task AtSevenThirtyAnOverloadThatHoldsTurnsThePreampOffOnce()
    {
        var run = await LivePollBench.TuneInQuietAsync(7_030_000);
        using var rig = run.Rig;

        Assert.Equal(new[] { 1 }, LivePollBench.PreampWrites(run.Radio));

        ModeEntryBench.ClearWrites(run.Radio);
        run.Radio.Overloading = true;
        var state = await LivePollBench.PollAsync(run, Held);

        Assert.Equal(new[] { 0 }, LivePollBench.PreampWrites(run.Radio));
        Assert.Equal(0, run.Radio.Switches[ModeEntryBench.Preamp]);
        Assert.True(state[Hamlet.RadioEngine.Rig.RigField.Overflow].Number > 0);
    }

    /// <summary>
    /// **AT 14.050**, the same script, then he turns the preamp on by hand while it is still
    /// overloading: nothing more is written, through the overload, its clearing and its return,
    /// until the next tune-in.
    /// </summary>
    [Fact]
    public async Task AtFourteenFiftyHisHandWhileItOverloadsStopsTheFollowing()
    {
        var run = await LivePollBench.TuneInQuietAsync(14_050_000);
        using var rig = run.Rig;

        ModeEntryBench.ClearWrites(run.Radio);
        run.Radio.Overloading = true;
        await LivePollBench.PollAsync(run, Held);

        Assert.Equal(new[] { 0 }, LivePollBench.PreampWrites(run.Radio));

        ModeEntryBench.ClearWrites(run.Radio);
        run.Radio.OperatorTurnsASwitch(ModeEntryBench.Preamp, 1);

        await LivePollBench.PollAsync(run, Held);
        run.Radio.Overloading = false;
        await LivePollBench.PollAsync(run, 40);
        run.Radio.Overloading = true;
        await LivePollBench.PollAsync(run, Held);

        Assert.Empty(ModeEntryBench.Writes(run.Radio));
        Assert.Equal(1, run.Radio.Switches[ModeEntryBench.Preamp]);
    }

    /// <summary>
    /// **NO WRITE WHILE TRANSMITTING.** The same rise with the radio keying produces no write;
    /// once it is receiving again and the overload still holds, the one write of off goes out.
    /// </summary>
    [Fact]
    public async Task NothingIsWrittenWhileTheRadioTransmits()
    {
        var run = await LivePollBench.TuneInQuietAsync(7_030_000);
        using var rig = run.Rig;

        ModeEntryBench.ClearWrites(run.Radio);
        run.Radio.Transmitting = true;
        run.Radio.Overloading = true;
        await LivePollBench.PollAsync(run, Held);

        Assert.Empty(ModeEntryBench.Writes(run.Radio));
        Assert.Equal(1, run.Radio.Switches[ModeEntryBench.Preamp]);

        run.Radio.Transmitting = false;
        await LivePollBench.PollAsync(run, Held);

        Assert.Equal(new[] { 0 }, LivePollBench.PreampWrites(run.Radio));
    }

    /// <summary>
    /// **THE HOLD** (task 3): a burst shorter than <see cref="Hamlet.RadioEngine.Rig.ReceiverSetup.OverloadHoldReadings"/>
    /// readings writes nothing, and one that reaches it writes once.
    /// </summary>
    [Fact]
    public async Task AShortBurstWritesNothingAndOneThatHoldsWritesOnce()
    {
        var run = await LivePollBench.TuneInQuietAsync(7_030_000);
        using var rig = run.Rig;
        var hold = Hamlet.RadioEngine.Rig.ReceiverSetup.OverloadHoldReadings;

        ModeEntryBench.ClearWrites(run.Radio);
        run.Radio.Overloading = true;
        await LivePollBench.PollAsync(run, hold - 1);
        run.Radio.Overloading = false;
        await LivePollBench.PollAsync(run, 2);

        Assert.Empty(ModeEntryBench.Writes(run.Radio));

        run.Radio.Overloading = true;
        await LivePollBench.PollAsync(run, hold);

        Assert.Equal(new[] { 0 }, LivePollBench.PreampWrites(run.Radio));
    }

    /// <summary>
    /// **BACK ON ONCE, AND OFF FOR GOOD IF IT BRINGS THE OVERLOAD BACK** (task 3): the band's
    /// value is written back after the overload has cleared and stayed clear; an overload that
    /// returns within the clear hold puts it off, and nothing more is written until the next
    /// tune-in.
    /// </summary>
    [Fact]
    public async Task AnOverloadThatComesBackOnceThePreampIsOnAgainLeavesItOff()
    {
        var run = await LivePollBench.TuneInQuietAsync(7_030_000);
        using var rig = run.Rig;
        var clear = Hamlet.RadioEngine.Rig.ReceiverSetup.ClearHoldReadings;

        ModeEntryBench.ClearWrites(run.Radio);
        run.Radio.Overloading = true;
        await LivePollBench.PollAsync(run, Held);
        run.Radio.Overloading = false;
        await LivePollBench.PollAsync(run, clear);

        Assert.Equal(new[] { 0, 1 }, LivePollBench.PreampWrites(run.Radio));

        run.Radio.Overloading = true;
        await LivePollBench.PollAsync(run, Held);
        run.Radio.Overloading = false;
        await LivePollBench.PollAsync(run, clear * 2);
        run.Radio.Overloading = true;
        await LivePollBench.PollAsync(run, Held);

        Assert.Equal(new[] { 0, 1, 0 }, LivePollBench.PreampWrites(run.Radio));
        Assert.Equal(0, run.Radio.Switches[ModeEntryBench.Preamp]);
    }

    /// <summary>
    /// **ONLY WHILE THE BLOCK OWNS IT** (HM-DEC-179): tuned into FT8's block, which does not
    /// state the preamp, an overload writes nothing.
    /// </summary>
    [Fact]
    public async Task ABlockThatDoesNotStateThePreampIsNotFollowed()
    {
        var run = await LivePollBench.TuneInQuietAsync(14_074_000);
        using var rig = run.Rig;

        Assert.DoesNotContain(run.Conditions, c => c.Field == Hamlet.RadioEngine.Rig.RigField.Preamp);

        ModeEntryBench.ClearWrites(run.Radio);
        run.Radio.Overloading = true;
        await LivePollBench.PollAsync(run, Held);

        Assert.Empty(ModeEntryBench.Writes(run.Radio));
    }

    /// <summary>
    /// **A TRANSMIT FLAG NOBODY HAS READ IS NOT A LICENCE**: with the radio silent on `1C 00`,
    /// an overload writes nothing.
    /// </summary>
    [Fact]
    public async Task AnUnreadTransmitFlagWritesNothing()
    {
        var run = await LivePollBench.TuneInQuietAsync(7_030_000);
        using var rig = run.Rig;

        ModeEntryBench.ClearWrites(run.Radio);
        run.Radio.Transmitting = null;
        run.Radio.Overloading = true;
        await LivePollBench.PollAsync(run, Held);

        Assert.Empty(ModeEntryBench.Writes(run.Radio));
    }
}
