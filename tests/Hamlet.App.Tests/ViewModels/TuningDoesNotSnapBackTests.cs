using Avalonia.Headless.XUnit;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// The rule is wired into the seam where rig state enters the user interface.
/// </summary>
/// <remarks>
/// <para>**THE RULE ITSELF IS `DialGuardTests` IN THE ENGINE**, where it is pure
/// and deterministic (§5). What is left here is the one thing only this layer can
/// answer: that `ApplyRigState` actually asks it before moving the display.</para>
/// <para>It runs under the headless application because that seam marshals to the
/// user interface thread, and a test without a dispatcher passes or fails
/// depending on what else is running beside it. That is how these five started
/// life, and a flaky test about a display bug is worse than none (HM-DEC-087).</para>
/// </remarks>
public sealed class TuningDoesNotSnapBackTests
{
    // **BOTH INSIDE THE SAME BAND ON PURPOSE.** The dial is clamped to the
    // picture on screen (HM-DEC-055), so a test that jumps bands without moving
    // the band buttons first is testing the clamp rather than the guard.
    private const long Was = 7_030_000;
    private const long Clicked = 7_061_000;

    private static readonly DateTime Now =
        new(2026, 8, 19, 20, 0, 0, DateTimeKind.Utc);

    private static RigState At(long hz, DateTime takenUtc)
        => RigState.Empty.With(new[]
        {
            RigValue.Known(
                RigField.Frequency, hz, (hz / 1_000_000.0).ToString("0.000"),
                takenUtc, "CI-V 03"),
        });

    /// <remarks>
    /// **THE OPERATOR'S REPORT, END TO END.** A reading taken before his tune
    /// arrives after it, and the display stays where he put it.
    /// </remarks>
    [AvaloniaFact]
    public void TheDisplayDoesNotSnapBackToWhereHeWas()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        model.NoteTuneWritten(Clicked, Was, Now);
        model.FrequencyHz = Clicked;

        model.ApplyRigState(At(Was, Now.AddMilliseconds(-100)));

        Assert.Equal(Clicked, model.FrequencyHz);
    }

    /// <remarks>
    /// And the other half at the same seam: a reading of the world after the tune
    /// moves the display at once, so nothing is frozen waiting out a window.
    /// </remarks>
    [AvaloniaFact]
    public void AReadingFromAfterTheTuneStillMovesIt()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        model.NoteTuneWritten(Clicked, Was, Now);
        model.FrequencyHz = Clicked;

        model.ApplyRigState(At(7_055_000, Now.AddMilliseconds(300)));

        Assert.Equal(7_055_000, model.FrequencyHz);
    }
}
