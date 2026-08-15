using Hamlet.App.Settings;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// The notice about the back of the radio retires on evidence (HM-DEC-081).
/// </summary>
/// <remarks>
/// It earns its place exactly once, before somebody's first transmission. After
/// that it is a standing block of orange text above the controls that the
/// operator has stopped reading, and a warning nobody reads is worse than none
/// because it teaches everything near it to be ignored.
/// </remarks>
public sealed class DummyLoadNoticeTests
{
    private static readonly DateTime Now = new(2026, 8, 15, 20, 0, 0, DateTimeKind.Utc);

    private static RigState State() => RigState.Empty.With(new[]
    {
        RigValue.Known(RigField.Mode, (int)CivMode.Cw, "CW", Now, "CI-V 04"),
    });

    /// <remarks>
    /// Proves HM-DEC-081: it is there before anything has been measured, which
    /// is the one moment it is worth reading.
    /// </remarks>
    [Fact]
    public void ItIsThereBeforeAnythingHasBeenMeasured()
        => Assert.Contains(
            TransmitNotes.WhatIsConnected,
            TransmitNotes.For(State(), hasMeasured: false));

    /// <remarks>
    /// Proves HM-DEC-081: once a send has produced a real reading it is gone,
    /// because by then Hamlet has measured something about the socket and the
    /// operator has seen the number. Evidence rather than a counter.
    /// </remarks>
    [Fact]
    public void ItIsGoneOnceASendHasMeasuredSomething()
    {
        var notes = TransmitNotes.For(State(), hasMeasured: true);

        Assert.DoesNotContain(TransmitNotes.WhatIsConnected, notes);
        Assert.DoesNotContain(
            notes, n => n.Contains("back of the radio", StringComparison.Ordinal));
    }

    /// <remarks>
    /// Proves HM-DEC-081: the power line is unaffected either way. Retiring one
    /// note must not take the others with it.
    /// </remarks>
    [Fact]
    public void RetiringItDoesNotTakeTheOtherNotesWithIt()
    {
        var quiet = State().With(
            RigValue.Known(RigField.RfPower, 5, "5%", Now, "CI-V 14 0A"));

        var before = TransmitNotes.For(quiet, hasMeasured: false);
        var after = TransmitNotes.For(quiet, hasMeasured: true);

        Assert.Equal(2, before.Count);
        Assert.Single(after);
        Assert.Contains("percent of its range", after[0], StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves HM-DEC-081: the fact survives a restart, so the note does not come
    /// back the next evening and have to be dismissed by being ignored again.
    /// </remarks>
    [Fact]
    public void TheFactSurvivesARestart()
    {
        var folder = Path.Combine(
            Path.GetTempPath(), "hamlet-tests", Guid.NewGuid().ToString("N"));

        try
        {
            var path = Path.Combine(folder, "settings.json");

            Assert.False(new AppSettings().HasMeasuredSwr);

            SettingsStore.SaveTo(new AppSettings { HasMeasuredSwr = true }, path);

            Assert.True(SettingsStore.LoadFrom(path).HasMeasuredSwr);
        }
        finally
        {
            try
            {
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, recursive: true);
                }
            }
            catch (IOException)
            {
                // A leftover temp folder is not a test failure.
            }
        }
    }
}
