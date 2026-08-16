using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// How long a message takes, which is what gives the latch a real end
/// (HM-DEC-085).
/// </summary>
/// <remarks>
/// Handing the message to the keyer is not the transmission. Command `17`
/// returns in about thirteen milliseconds and the radio then keys for eighteen
/// seconds, so the end has to be predicted before the start.
/// </remarks>
public sealed class CwDurationTests
{
    /// <remarks>
    /// <para>Proves HM-DEC-085 against the definition of the unit rather than
    /// against itself. A word a minute means the word PARIS once a minute, and
    /// PARIS with the space after it is fifty dit lengths, so twenty words a
    /// minute is exactly twenty PARISes in sixty seconds.</para>
    /// <para>If this passes, the whole clock is right, because everything else
    /// in it is element counting.</para>
    /// </remarks>
    [Theory]
    [InlineData(5)]
    [InlineData(13)]
    [InlineData(20)]
    [InlineData(40)]
    public void TwentyWordsAMinuteMeansTwentyWordsInAMinute(int wpm)
    {
        var text = string.Join(' ', Enumerable.Repeat("PARIS", wpm));

        // The trailing word space is inside the definition and outside the
        // message, so it is added back to compare against the minute.
        var elapsed = CwDuration.Of(text, wpm) + CwDuration.Dit(wpm) * 7;

        // To the millisecond rather than exactly: a dit at thirteen words a
        // minute is not a whole number of ticks, and the truncation adds up to
        // sixty microseconds across a minute. That is the clock's own floor and
        // not an error in the counting.
        Assert.Equal(60.0, elapsed.TotalSeconds, precision: 3);
    }

    /// <remarks>
    /// Proves HM-DEC-085: the real call the app sends lands in the region the
    /// operator timed with a stopwatch, which was about eighteen seconds. The
    /// range is wide on purpose, because the exact text and the exact keyer
    /// speed of those two sends were not recorded, so this catches an answer off
    /// by a factor and deliberately does not pretend to more than that.
    /// </remarks>
    [Fact]
    public void TheRealCqTakesTheBetterPartOfHalfAMinute()
    {
        var seconds = CwDuration.Of(MorseCode.CqCall("KC3QIS"), 20).TotalSeconds;

        Assert.InRange(seconds, 12.0, 24.0);
    }

    /// <remarks>
    /// Proves HM-DEC-085: it scales the way Morse does, so halving the speed
    /// doubles the time.
    /// </remarks>
    [Fact]
    public void HalvingTheSpeedDoublesTheTime()
    {
        var fast = CwDuration.Of("CQ DE KC3QIS K", 24).TotalSeconds;
        var slow = CwDuration.Of("CQ DE KC3QIS K", 12).TotalSeconds;

        Assert.Equal(fast * 2, slow, precision: 6);
    }

    /// <remarks>
    /// Proves HM-DEC-085: the hold-off outlasts the longest silence a message
    /// can legitimately contain. A word space is seven dit lengths, so anything
    /// past that means finished. **Edge detection would end the state between the
    /// first two dits**, which is the bug this exists to prevent.
    /// </remarks>
    [Theory]
    [InlineData(5)]
    [InlineData(13)]
    [InlineData(20)]
    [InlineData(40)]
    public void TheHoldOffOutlastsAWordSpace(int wpm)
        => Assert.True(CwDuration.Silence(wpm) > CwDuration.Dit(wpm) * 7);

    /// <remarks>
    /// Proves HM-DEC-085: the hold-off never drops below the floor, which is
    /// three samples of the transmit line at the rate the rig is polled. At forty
    /// words a minute two word gaps is a quarter of a second, which is one
    /// sample, and one sample is not evidence of anything.
    /// </remarks>
    [Theory]
    [InlineData(20, 840)]
    [InlineData(30, 750)]
    [InlineData(40, 750)]
    public void TheHoldOffNeverFallsBelowTheFloor(int wpm, double milliseconds)
        => Assert.Equal(
            milliseconds, CwDuration.Silence(wpm).TotalMilliseconds, precision: 3);

    /// <remarks>
    /// Proves HM-DEC-085: nothing to send takes no time, and a character the
    /// table does not carry contributes nothing rather than a guessed average,
    /// because the radio will not send it either.
    /// </remarks>
    [Fact]
    public void NothingSendableTakesNoTime()
    {
        Assert.Equal(TimeSpan.Zero, CwDuration.Of("", 20));
        Assert.Equal(TimeSpan.Zero, CwDuration.Of(null, 20));
        Assert.Equal(TimeSpan.Zero, CwDuration.Of("   ", 20));

        // A letter and an unsendable character take as long as the letter alone.
        Assert.Equal(CwDuration.Of("E", 20), CwDuration.Of("E©", 20));
    }

    /// <remarks>
    /// Proves HM-DEC-085: an unread keyer speed falls back to what this radio
    /// was observed sending at, which sizes a progress bar and never claims a
    /// speed. The reading wins whenever there is one (§0.0).
    /// </remarks>
    [Fact]
    public void AnUnreadSpeedFallsBackRatherThanFailing()
        => Assert.Equal(
            CwDuration.Of("CQ", CwDuration.DefaultWpm), CwDuration.Of("CQ", 0));
}
