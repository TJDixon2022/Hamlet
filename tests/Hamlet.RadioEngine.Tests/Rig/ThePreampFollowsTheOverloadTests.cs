using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
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
/// <para>**R98 LEFT THE FOLLOW WITH NO REAL ROW TO FOLLOW** (HM-DEC-191, work instruction
/// 486). The CW row's preamp is now a plain constant, off, with no `bands` and no
/// `whenOverloading`, so on every real block <see cref="ReceiverSetup.FollowOverloadAsync"/>
/// finds nothing it owns and writes nothing. The mechanism is unchanged and these facts are
/// about the mechanism, so they drive it through a preamp condition built here, shaped the way
/// the CW row was under HM-DEC-177 (<see cref="OverloadRule"/>), put in place of the block's own
/// preamp row. <see cref="TheRealMorseBlockFollowsNothing"/> pins that the real CW row follows
/// nothing.</para>
/// <para>Against <see cref="ScriptedRadio"/>, so every result is an indication (FACT-006).</para>
/// </remarks>
public sealed class ThePreampFollowsTheOverloadTests
{
    /// <summary>Several live passes, about three seconds at the measured cadence.</summary>
    private const int Held = 12;

    /// <summary>
    /// **AT 7.030**, where the old rule turned the preamp off by band: with the rule built here,
    /// the quiet tune-in writes preamp 1, then an overload that rises and holds is followed by
    /// exactly one write of off.
    /// </summary>
    [Fact]
    public async Task AtSevenThirtyAnOverloadThatHoldsTurnsThePreampOffOnce()
    {
        var run = await TuneInQuietWithTheRuleAsync(7_030_000);
        using var rig = run.Rig;

        Assert.Equal(new[] { 1 }, LivePollBench.PreampWrites(run.Radio));

        ModeEntryBench.ClearWrites(run.Radio);
        run.Radio.Overloading = true;
        var state = await LivePollBench.PollAsync(run, Held);

        Assert.Equal(new[] { 0 }, LivePollBench.PreampWrites(run.Radio));
        Assert.Equal(0, run.Radio.Switches[ModeEntryBench.Preamp]);
        Assert.True(state[RigField.Overflow].Number > 0);
    }

    /// <summary>
    /// **AT 14.050**, the same script with the rule built here, then he turns the preamp on by hand while it is still
    /// overloading: nothing more is written, through the overload, its clearing and its return,
    /// until the next tune-in.
    /// </summary>
    [Fact]
    public async Task AtFourteenFiftyHisHandWhileItOverloadsStopsTheFollowing()
    {
        var run = await TuneInQuietWithTheRuleAsync(14_050_000);
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
        var run = await TuneInQuietWithTheRuleAsync(7_030_000);
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
    /// **THE HOLD** (task 3): a burst shorter than <see cref="ReceiverSetup.OverloadHoldReadings"/>
    /// readings writes nothing, and one that reaches it writes once.
    /// </summary>
    [Fact]
    public async Task AShortBurstWritesNothingAndOneThatHoldsWritesOnce()
    {
        var run = await TuneInQuietWithTheRuleAsync(7_030_000);
        using var rig = run.Rig;
        var hold = ReceiverSetup.OverloadHoldReadings;

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
        var run = await TuneInQuietWithTheRuleAsync(7_030_000);
        using var rig = run.Rig;
        var clear = ReceiverSetup.ClearHoldReadings;

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

        Assert.DoesNotContain(run.Conditions, c => c.Field == RigField.Preamp);

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
        var run = await TuneInQuietWithTheRuleAsync(7_030_000);
        using var rig = run.Rig;

        ModeEntryBench.ClearWrites(run.Radio);
        run.Radio.Transmitting = null;
        run.Radio.Overloading = true;
        await LivePollBench.PollAsync(run, Held);

        Assert.Empty(ModeEntryBench.Writes(run.Radio));
    }

    /// <summary>
    /// **THE REAL MORSE BLOCK FOLLOWS NOTHING** (R98, work instruction 486): at 7.030 the
    /// block's own preamp row is off with no overload rule, so the quiet tune-in writes
    /// nothing to a radio already off, and an overload that holds writes nothing either.
    /// </summary>
    [Fact]
    public async Task TheRealMorseBlockFollowsNothing()
    {
        var run = await LivePollBench.TuneInQuietAsync(7_030_000);
        using var rig = run.Rig;

        var preamp = Assert.Single(run.Conditions, c => c.Field == RigField.Preamp);
        Assert.Null(preamp.WhenOverloading);
        Assert.Empty(LivePollBench.PreampWrites(run.Radio));

        ModeEntryBench.ClearWrites(run.Radio);
        run.Radio.Overloading = true;
        await LivePollBench.PollAsync(run, Held);

        Assert.Empty(LivePollBench.PreampWrites(run.Radio));
        Assert.Equal(0, run.Radio.Switches[ModeEntryBench.Preamp]);
    }

    /// <summary>
    /// A preamp condition with an overload rule, shaped the way the CW row was under
    /// HM-DEC-177: preamp 1 from 1.8 to 29.999 MHz, preamp 2 from 50 to 54 MHz, off while
    /// the front end reads overloading. No real row carries one since R98 (work instruction
    /// 486); it is built here so the follow mechanism keeps its facts.
    /// </summary>
    private static readonly ReceiverCondition OverloadRule = new(
        "preamp",
        RigField.Preamp,
        1,
        "preamp 1 on HF, preamp 2 at 50 MHz, off while overloading",
        "a test needs a row the overload follow can follow",
        "Built in the test, shaped like the CW row under HM-DEC-177.",
        Confirmed: true,
        Condition: "band")
    {
        Bands = new[]
        {
            new ConditionBand(1_800_000, 29_999_999, 1, "test"),
            new ConditionBand(50_000_000, 54_000_000, 2, "test"),
        },
        WhenOverloading = 0,
    };

    /// <summary>
    /// <see cref="LivePollBench.TuneInQuietAsync"/> with the block's own preamp row replaced
    /// by <see cref="OverloadRule"/>.
    /// </summary>
    /// <param name="hz">The dial.</param>
    /// <returns>The run, with the tune-in's writes still on the radio's record.</returns>
    private static async Task<LivePollBench.Run> TuneInQuietWithTheRuleAsync(long hz)
    {
        var block = ModeEntryBench.BlockAt(hz);
        var fromBlock = ReceiverConditions.ForBlock(block);
        var conditions = (fromBlock.Count > 0 ? fromBlock : ReceiverConditions.ForMode("CW"))
            .Select(c => c.Field == RigField.Preamp ? OverloadRule : c)
            .ToList();

        var radio = ModeEntryBench.AsLeft(hz, data: false);
        radio.Transmitting = false;
        var rig = await ModeEntryBench.ConnectAsync(radio);
        var run = new LivePollBench.Run { Rig = rig, Radio = radio, Conditions = conditions };

        var (results, memory) = await ReceiverSetup.ApplyAsync(rig, conditions, ReceiverSetupMemory.Empty);
        run.Results = results;
        run.Memory = memory;
        return run;
    }
}
