using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Tuning from the app moves the display and leaves it moved.
/// </summary>
/// <remarks>
/// <para>**WHAT THE OPERATOR WATCHED, ON HIS OWN RADIO, EVERY TIME.** He clicked
/// a dot, dragged the map or changed band; the radio moved correctly and stayed
/// moved, and the app's picture snapped back to where he had just been and held
/// it for about thirty seconds before catching up. Turning the radio's own knob
/// was perfect throughout, and the dividing line was the write.</para>
/// <para>**THE MECHANISM IS A READING OLDER THAN THE TUNE.** The guard that stops
/// a reading dragging the dial back covered the queue and was released the
/// instant the send began, so the whole round trip was unprotected: the model
/// still held the pre-tune frequency and the display took it. The thirty seconds
/// was the session sweep that was then the frequency's only refresh.</para>
/// </remarks>
public sealed class DialGuardTests
{
    private const long Was = 7_030_000;
    private const long Clicked = 7_061_000;

    private static readonly DateTime Now =
        new(2026, 8, 19, 20, 0, 0, DateTimeKind.Utc);

    private static RigValue Reading(long hz, DateTime takenUtc)
        => RigValue.Known(
            RigField.Frequency, hz, (hz / 1_000_000.0).ToString("0.000"),
            takenUtc, "CI-V 03");

    /// <remarks>
    /// **THE REPRODUCTION.** A reading taken before the tune arrives after it, and
    /// it may not move the display. This is the operator's report in one line.
    /// </remarks>
    [Fact]
    public void AReadingOlderThanTheTuneMayNotMoveTheDisplay()
        => Assert.False(
            DialGuard.MayFollow(Reading(Was, Now.AddMilliseconds(-100)), Now));

    /// <remarks>
    /// Proves the fix is bounded by the write rather than by a timer: a reading
    /// taken after the tune is believed at once, including when it disagrees,
    /// because the radio is always right about its own frequency (§0.0).
    /// </remarks>
    [Fact]
    public void AReadingTakenAfterTheTuneIsBelievedImmediately()
        => Assert.True(
            DialGuard.MayFollow(Reading(7_055_000, Now.AddMilliseconds(300)), Now));

    /// <remarks>
    /// Proves nothing was slowed down for the case that always worked. With no
    /// tune of Hamlet's own behind it, every reading may move the display, which
    /// is the radio's own knob and it tracked in real time throughout.
    /// </remarks>
    [Fact]
    public void WithNoTuneOfOurOwnEveryReadingMayMoveTheDisplay()
    {
        Assert.True(DialGuard.MayFollow(Reading(Was, Now.AddSeconds(-30)), null));
        Assert.True(DialGuard.MayFollow(Reading(Was, Now.AddSeconds(30)), null));
    }

    /// <remarks>
    /// **THE ASSERTION THE ORDER ASKS FOR IN SO MANY WORDS**: after a tune, no
    /// reading older than the tune is ever taken. Swept over a run of them
    /// arriving out of order, which is what a busy link produces.
    /// </remarks>
    [Fact]
    public void AfterATuneNoOlderReadingIsEverTaken()
    {
        foreach (var late in new[] { -30_000, -400, -250, -100, -10, -1 })
        {
            Assert.False(
                DialGuard.MayFollow(Reading(Was, Now.AddMilliseconds(late)), Now),
                $"a reading {-late} ms older than the tune moved the display");
        }

        Assert.True(DialGuard.MayFollow(Reading(Clicked, Now.AddMilliseconds(1)), Now));
    }

    /// <remarks>
    /// Proves the boundary is on the safe side of itself: a reading stamped at the
    /// very instant of the write crossed it on the wire, so it is about the past
    /// whatever its clock says (§0.2.1).
    /// </remarks>
    [Fact]
    public void AReadingStampedAtTheInstantOfTheTuneIsNotBelieved()
        => Assert.False(DialGuard.MayFollow(Reading(Was, Now), Now));

    /// <remarks>
    /// Proves an unread value cannot argue with a tune. Unknown is a state and
    /// never a licence (HM-DEC-050).
    /// </remarks>
    [Fact]
    public void AnUnreadValueMayNotMoveTheDisplayAfterATune()
        => Assert.False(
            DialGuard.MayFollow(RigState.Empty[RigField.Frequency], Now));

    /// <remarks>
    /// Proves the signature the record now carries: the reading that takes the
    /// display back to exactly where the tune started, when the tune asked for
    /// somewhere else. The radio does not tune itself backwards.
    /// </remarks>
    [Fact]
    public void GoingBackToWhereTheTuneStartedIsTheSignature()
    {
        Assert.True(DialGuard.WouldGoBackwards(Was, Was, Clicked));

        // Arriving at what was asked for is the ordinary case and says nothing.
        Assert.False(DialGuard.WouldGoBackwards(Clicked, Was, Clicked));

        // Somewhere else entirely is the operator's own hand, not a snap-back.
        Assert.False(DialGuard.WouldGoBackwards(7_055_000, Was, Clicked));

        // And with no tune behind it there is nothing to go back from.
        Assert.False(DialGuard.WouldGoBackwards(Was, Was, null));
    }
}
