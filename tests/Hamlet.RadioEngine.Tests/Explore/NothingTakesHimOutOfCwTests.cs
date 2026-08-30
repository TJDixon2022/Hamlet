using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// The app does not move the operator out of Morse while he is working Morse.
/// </summary>
/// <remarks>
/// <para>**SIXTY-SIX SECONDS UNABLE TO ANSWER, CAUSED BY THE APP.** Session
/// `9f9d23eb`, 2026-08-18: mode follow wrote USB with the data variant on, over
/// and over, while the operator sat on CW main street with the terminal decoding
/// a signal at 500 hertz. The send controls refused `not_in_morse` from 20:30:07
/// to 20:31:13. A station could have been calling him throughout.</para>
/// <para>**TWO FAULTS MADE IT, AND BOTH ARE HERE.** The map said a digital block
/// and nothing weighed that against what he was visibly doing; and a mode write
/// carrying the data byte folded only the mode into the model, so the target
/// could never read back as satisfied and every trigger wrote again.</para>
/// </remarks>
public sealed class NothingTakesHimOutOfCwTests
{
    private const long OnCwMainStreet = 7_030_000;

    private static readonly ModeTarget Digital =
        new(CivMode.Usb, true, "this block is where the digital modes gather");

    private static readonly ModeTarget Morse =
        new(CivMode.Cw, false, "this is the Morse end of the band");

    /// <remarks>
    /// **THE EVENING, IN ONE ASSERTION.** He is working Morse, the map says
    /// digital, and nothing is written. The map is the weaker evidence and the
    /// operator's own hand wins (HM-DEC-056).
    /// </remarks>
    [Fact]
    public void TheMapDoesNotOverrideWhatHeIsVisiblyDoing()
    {
        var state = ModeFollowState.Armed(true);

        var decision = ModeFollowPlan.Decide(
            state, CivMode.Cw, false, Digital, OnCwMainStreet, workingCw: true);

        Assert.False(decision.Write);
    }

    /// <remarks>
    /// Proves it is not a blanket freeze: a Morse target is still followed while
    /// he is working Morse, which is the case where the automation agrees with him
    /// and is worth having.
    /// </remarks>
    [Fact]
    public void AMorseTargetIsStillFollowedWhileWorkingMorse()
    {
        var state = ModeFollowState.Armed(true);

        var decision = ModeFollowPlan.Decide(
            state, CivMode.Usb, false, Morse, OnCwMainStreet, workingCw: true);

        Assert.True(decision.Write);
        Assert.Equal(CivMode.Cw, decision.Mode);
    }

    /// <remarks>
    /// <para>Proves the feature survives: arriving in a digital block with the
    /// terminal off still follows the map, which is what HM-DEC-056 was built
    /// for.</para>
    /// <para>**THIS TEST WAS A LIE FOR WEEKS AND IT IS WORTH SAYING HOW** (work
    /// instruction 051, task 3). Its remark used to claim 14.074 MHz was "outside
    /// any CW segment" and it handed `workingCw: false` to match. **14.074 is
    /// inside 20 m's CW segment** — every digital block is, because a CW segment
    /// here is derived from the data-carrying emission ranges of 47 CFR 97.305(c)
    /// and that is the same stretch. So the running app computed `true` where this
    /// wrote `false`, the test asserted a state the application could not reach,
    /// it passed, and the radio stayed in CW.</para>
    /// <para>**So the value is no longer written down.** It comes from
    /// `ModeFollowPlan.WorkingCw`, the expression the view model calls, and the
    /// whole map is walked through it in
    /// `ArrivingAnywhereOnTheMapFollowsItTests`. Kept rather than deleted because
    /// the named frequency is the one the operator was on, and a sweep is not what
    /// somebody reads when they come back asking what went wrong.</para>
    /// </remarks>
    [Fact]
    public void ArrivingInADigitalBlockDoingNothingElseStillFollows()
    {
        var state = ModeFollowState.Armed(true);

        var decision = ModeFollowPlan.Decide(
            state, CivMode.Cw, false, Digital, 14_074_000,
            ModeFollowPlan.WorkingCw(Digital, isCopyingMorse: false));

        Assert.True(decision.Write);
        Assert.Equal(CivMode.Usb, decision.Mode);
        Assert.True(decision.DataMode);
    }

    /// <remarks>
    /// **THE LOOP, STATED AS THE PLAN SEES IT.** With the data variant folded in
    /// by the write that set it, the target reads back as satisfied and the second
    /// trigger writes nothing. Before that fold, `currentDataMode` stayed false
    /// after a successful write and this returned true for ever.
    /// </remarks>
    [Fact]
    public void ADataModeTargetIsSatisfiedOnceTheRadioIsInIt()
    {
        var state = ModeFollowState.Armed(true);

        var first = ModeFollowPlan.Decide(
            state, CivMode.Cw, false, Digital, 14_074_000);

        Assert.True(first.Write);

        // What the model holds after the radio confirms that write.
        var second = ModeFollowPlan.Decide(
            state, CivMode.Usb, true, Digital, 14_074_000);

        Assert.False(second.Write);
    }
}
