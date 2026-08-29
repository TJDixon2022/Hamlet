using Hamlet.App.ViewModels;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Fine-tuning a station does not throw away what the decoder has learned
/// about it.
/// </summary>
/// <remarks>
/// <para>**A SMALL NUDGE IS NOT A MOVE** (Tim's ruling of 2026-08-29).
/// `OnFrequencyHzChanged` called `Retuned()` on every change to the dial,
/// including a ten-hertz one, so nudging a station a few hertz to centre it
/// threw away the pitch the survey had just measured on it, the held peak, and
/// the twelve seconds of audio being read. **The station a nudge is aimed at is
/// the station already being read**, so that is the opposite of what a reset is
/// for.</para>
/// <para>**THE FIGURE IS PROVISIONAL AND THE NUMBER IS TIM'S** (§12.4). Five
/// hundred hertz is the CW filter's own width, so inside it the receiver is
/// still passing the same signal. The operator's own moves on 2026-08-29 were
/// 8.8 kHz and 13.0 kHz, twenty times this. Three candidates are costed in unit
/// 043's report.</para>
/// <para>This pins the number so that changing it is a decision somebody makes
/// rather than a line somebody edits.</para>
/// </remarks>
public sealed class ANudgeIsNotAMoveTests
{
    /// <summary>The threshold is the CW filter's width, and it is stated once.</summary>
    [Fact]
    public void TheNudgeThresholdIsTheFiltersOwnWidth()
        => Assert.Equal(500, MainWindowViewModel.NudgeHz);

    /// <summary>
    /// The moves the operator actually made are moves, and a nudge is not.
    /// </summary>
    /// <remarks>
    /// Measured against 2026-08-29's own dial: 7.0284 to 7.0372 is 8.8 kHz and
    /// 7.0372 to 7.0502 is 13.0 kHz. Both are far outside the threshold, and
    /// centring a station by a couple of hundred hertz is well inside it.
    /// </remarks>
    [Theory]
    [InlineData(8_800, true)]
    [InlineData(13_000, true)]
    [InlineData(500, true)]
    [InlineData(499, false)]
    [InlineData(200, false)]
    [InlineData(10, false)]
    [InlineData(0, false)]
    public void OnlyAMoveCountsAsAMove(long movedByHz, bool isMove)
        => Assert.Equal(isMove, movedByHz >= MainWindowViewModel.NudgeHz);
}
