using Avalonia.Headless.XUnit;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Nothing changing recomputes nothing (HM-OPEN-041).
/// </summary>
/// <remarks>
/// <para>**EIGHTEEN `mode_followed` EVENTS IN ONE EVENING**, ten of them with no
/// tuning request within three seconds, including an unbroken run at 20:30:39,
/// :50, :51, :53, :56, :57, :59 and 20:31:02 with the dial standing still. The
/// last session gave the plan a memory so it will not repeat a write the radio
/// confirmed, and recorded that **what recomputed the decision was still
/// unseen.**</para>
/// <para>**IT IS THE FREQUENCY CHANGING, AND IT WAS THE SNAP-BACK CHANGING IT.**
/// `ScheduleModeFollow` has two callers: a band change, and `FrequencyHz`
/// changing by any route including a reading from the radio. In the build that
/// evening, a reading older than the operator's own tune dragged the display back
/// and the next poll moved it forward again, so the number changed twice per
/// tune with nobody touching anything, and each change restarted the six hundred
/// millisecond settle. The gaps in that run — one to eleven seconds — are what a
/// settle timer does when it is restarted by a value that will not sit still.
/// `recent_dwell_short` fires from the same handler, which is why HM-OPEN-041
/// named it as the instrument: four of them in a session with two tunes is four
/// frequency changes the operator did not make.</para>
/// <para>So the repair is the one already shipped — `DialGuard` refuses a reading
/// older than the tune — and what was missing was anything asserting the quiet
/// case. These are that.</para>
/// </remarks>
public sealed class ModeFollowRecomputeTests
{
    private const long Here = 7_030_000;

    private static readonly DateTime Now =
        new(2026, 8, 19, 20, 30, 0, DateTimeKind.Utc);

    private static RigState At(long hz, DateTime takenUtc)
        => RigState.Empty.With(new[]
        {
            RigValue.Known(
                RigField.Frequency, hz, (hz / 1_000_000.0).ToString("0.000"),
                takenUtc, "CI-V 03"),
        });

    /// <remarks>
    /// **THE FIXTURE THE ORDER ASKS FOR.** Forty polls at the live rate, every one
    /// reporting the frequency the display already holds, and nothing recomputes.
    /// Forty is ten seconds of a real poll loop, which is longer than the run that
    /// was observed.
    /// </remarks>
    [AvaloniaFact]
    public void NothingChangingRecomputesNothing()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);
        model.FrequencyHz = Here;

        var before = model.ModeFollowReschedules;

        for (var i = 0; i < 40; i++)
        {
            model.ApplyRigState(At(Here, Now.AddMilliseconds(250 * i)));
        }

        Assert.Equal(Here, model.FrequencyHz);
        Assert.Equal(before, model.ModeFollowReschedules);
    }

    /// <remarks>
    /// **THE CASE THAT WAS PRODUCING THE RUN**, now that the guard is in: a stale
    /// reading arriving after a tune neither moves the display nor recomputes
    /// anything. Without `DialGuard` this alternates and each alternation is a
    /// reschedule.
    /// </remarks>
    [AvaloniaFact]
    public void AStaleReadingAfterATuneRecomputesNothing()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);
        model.FrequencyHz = Here;

        model.NoteTuneWritten(7_061_000, Here, Now);
        model.FrequencyHz = 7_061_000;

        var before = model.ModeFollowReschedules;

        // The poll that was already on the wire when the write went out, arriving
        // eight times as a busy link would deliver it.
        for (var i = 0; i < 8; i++)
        {
            model.ApplyRigState(At(Here, Now.AddMilliseconds(-100 + i)));
        }

        Assert.Equal(7_061_000, model.FrequencyHz);
        Assert.Equal(before, model.ModeFollowReschedules);
    }

    /// <remarks>
    /// And the ordinary case still works: the radio really moved, so the display
    /// follows and the mode decision is asked again. A guard that silenced this
    /// would be worse than the loop.
    /// </remarks>
    [AvaloniaFact]
    public void AGenuineMoveStillRecomputesOnce()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);
        model.FrequencyHz = Here;

        var before = model.ModeFollowReschedules;

        model.ApplyRigState(At(7_047_000, Now.AddSeconds(1)));

        Assert.Equal(7_047_000, model.FrequencyHz);
        Assert.Equal(before + 1, model.ModeFollowReschedules);
    }
}
