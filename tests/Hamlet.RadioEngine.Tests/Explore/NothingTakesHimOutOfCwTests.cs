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
    /// Proves the feature survives: arriving in a digital block with the terminal
    /// off and outside any CW segment still follows the map, which is what
    /// HM-DEC-056 was built for.
    /// </remarks>
    [Fact]
    public void ArrivingInADigitalBlockDoingNothingElseStillFollows()
    {
        var state = ModeFollowState.Armed(true);

        var decision = ModeFollowPlan.Decide(
            state, CivMode.Cw, false, Digital, 14_074_000, workingCw: false);

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
