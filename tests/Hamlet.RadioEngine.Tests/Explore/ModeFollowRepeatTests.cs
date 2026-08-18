using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// Mode follow writes once for a place and then stops, whatever the radio says
/// about itself (HM-OPEN-041).
/// </summary>
/// <remarks>
/// <para>**THE EVENING THIS IS TAKEN FROM.** Session `9f9d23eb`, 2026-08-18:
/// eighteen `mode_followed` events, ten of them with no tuning request anywhere
/// near them, including an unbroken run at 20:30:39, :50, :51, :53, :56, :57,
/// :59 and 20:31:02 with the dial standing still. The decision only ever asked
/// the radio what mode it was in, so a field that came back unknown or a variant
/// the radio does not report separately made every tick look like a fresh
/// arrival at a neighborhood.</para>
/// <para>Nothing here writes where the old test would have refused. The memory
/// can only take writes away.</para>
/// </remarks>
public sealed class ModeFollowRepeatTests
{
    private const long Here = 7_030_000;

    private static readonly ModeTarget Morse =
        new(CivMode.Cw, false, "this is the Morse end of the band");

    /// <remarks>
    /// Proves HM-OPEN-041: with nothing changing — the dial still, the radio
    /// saying nothing useful about its own mode — the decision writes exactly
    /// once and refuses every time after.
    /// </remarks>
    [Fact]
    public void NothingChangingProducesExactlyOneFollow()
    {
        var state = ModeFollowState.Armed(true);
        var writes = 0;

        // Twenty ticks, which is more than the evening's own run, and the radio
        // never reports back: the mode reads null throughout, which is exactly
        // the state that produced eighteen writes.
        for (var i = 0; i < 20; i++)
        {
            var decision = ModeFollowPlan.Decide(state, null, false, Morse, Here);

            if (!decision.Write)
            {
                continue;
            }

            writes++;
            state = state.Done(Here, decision.Mode, decision.DataMode);
        }

        Assert.Equal(1, writes);
    }

    /// <remarks>
    /// Proves HM-OPEN-041: the memory is of a write to a place, so moving
    /// somewhere else in the same neighborhood asks again. A single memory would
    /// otherwise silence the automation for the rest of the session.
    /// </remarks>
    [Fact]
    public void MovingSomewhereElseAsksAgain()
    {
        var state = ModeFollowState.Armed(true).Done(Here, CivMode.Cw, false);

        Assert.False(ModeFollowPlan.Decide(state, null, false, Morse, Here).Write);
        Assert.True(ModeFollowPlan.Decide(state, null, false, Morse, 7_047_000).Write);
    }

    /// <remarks>
    /// Proves HM-OPEN-041: a write is remembered only where the radio confirmed
    /// it, so an unconfirmed one is tried again. The caller does the confirming;
    /// this states what the state means, which is what the caller relies on.
    /// </remarks>
    [Fact]
    public void AWriteThatWasNotConfirmedIsNotRemembered()
    {
        var state = ModeFollowState.Armed(true);

        Assert.True(ModeFollowPlan.Decide(state, null, false, Morse, Here).Write);
        Assert.True(ModeFollowPlan.Decide(state, null, false, Morse, Here).Write);
    }

    /// <remarks>
    /// Proves HM-OPEN-041 against HM-DEC-056: the operator taking the mode knob
    /// still wins, and coming back to the automation is a fresh start rather
    /// than a memory of what Hamlet did before he intervened.
    /// </remarks>
    [Fact]
    public void TheOperatorsOwnHandClearsTheMemory()
    {
        var state = ModeFollowState.Armed(true).Done(Here, CivMode.Cw, false);

        var suspended = state.SuspendedByOperator();
        Assert.False(ModeFollowPlan.Decide(suspended, null, false, Morse, Here).Write);

        var rearmed = suspended.Rearmed();
        Assert.True(ModeFollowPlan.Decide(rearmed, null, false, Morse, Here).Write);
    }

    /// <remarks>
    /// Proves HM-OPEN-041: a different target at the same place is a different
    /// write, because the memory is of what was set and not only of where.
    /// </remarks>
    [Fact]
    public void ADifferentTargetAtTheSamePlaceStillWrites()
    {
        var state = ModeFollowState.Armed(true).Done(Here, CivMode.Cw, false);

        var digital = new ModeTarget(CivMode.Usb, true, "the digital block");

        Assert.True(ModeFollowPlan.Decide(state, null, false, digital, Here).Write);
    }

    /// <remarks>
    /// Proves HM-DEC-077: every refusal names its own branch. Four states used
    /// to fall through to `already_transmitting`, so a refusal on the operator's
    /// license wrote a record saying the radio was busy while the state field one
    /// column over said `OutsidePrivileges`.
    /// </remarks>
    [Theory]
    [InlineData(CwReadyState.OutsidePrivileges, "outside_privileges")]
    [InlineData(CwReadyState.ListenOnly, "listen_only")]
    [InlineData(CwReadyState.LicenseClassUnknown, "license_class_unknown")]
    [InlineData(CwReadyState.FrequencyUnknown, "frequency_unknown")]
    [InlineData(CwReadyState.AlreadyTransmitting, "already_transmitting")]
    [InlineData(CwReadyState.BreakInOff, "break_in_off")]
    public void EveryRefusalNamesItsOwnBranch(CwReadyState state, string reason)
    {
        var verdict = new CwReadiness(state, false, "", "");

        Assert.Equal(reason, verdict.Reason);
    }

    /// <remarks>
    /// Proves HM-DEC-077 the other way round: no two refusals share a token, so
    /// counting refusals by cause across sessions means something.
    /// </remarks>
    [Fact]
    public void NoTwoRefusalsShareAToken()
    {
        var reasons = Enum.GetValues<CwReadyState>()
            .Where(s => s != CwReadyState.Ready)
            .Select(s => new CwReadiness(s, false, "", "").Reason)
            .ToList();

        Assert.Equal(reasons.Count, reasons.Distinct().Count());
    }
}
