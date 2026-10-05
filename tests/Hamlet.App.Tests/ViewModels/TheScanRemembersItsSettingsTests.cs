using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **THE SCAN REMEMBERS ITS SETTINGS** (work instruction 541, task 3, HM-DEC-245): its length and two stays are kept in
/// Hamlet's settings and read back at start, rather than resetting to 30 minutes, 90 s and 30 s on every restart.
/// </summary>
public sealed class TheScanRemembersItsSettingsTests : IDisposable
{
    private readonly string _folder;
    private readonly string _wasFolder;

    /// <summary>Creates the tests and redirects the data folder.</summary>
    public TheScanRemembersItsSettingsTests()
    {
        _wasFolder = SettingsStore.DataFolder;
        _folder = Path.Combine(Path.GetTempPath(), "hamlet-unit541-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_folder);
        SettingsStore.DataFolder = _folder;
    }

    /// <summary>Puts the real folder back.</summary>
    public void Dispose()
    {
        SettingsStore.DataFolder = _wasFolder;

        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (IOException)
        {
        }
    }

    /// <remarks>Set in the popover, the four come back in a new window read from the saved file.</remarks>
    [Fact]
    public void WhatWasSetComesBackAfterARestart()
    {
        var first = new MainWindowViewModel(new AppSettings(), null)
        {
            CatchScanMinutes = 45,
            CatchPositiveStaySeconds = 60,
            CatchNegativeStaySeconds = 20,
            CatchSurveySeconds = 5,
        };

        Assert.Equal(45, first.CatchScanMinutes);

        var again = new MainWindowViewModel(SettingsStore.Load(), null);

        Assert.Equal(45, again.CatchScanMinutes);
        Assert.Equal(60, again.CatchPositiveStaySeconds);
        Assert.Equal(20, again.CatchNegativeStaySeconds);
        Assert.Equal(5, again.CatchSurveySeconds);
    }

    /// <remarks>A fresh install starts at the owner's defaults: 30 minutes, 90 s and 30 s, and a survey of 3 s (work instruction 543).</remarks>
    [Fact]
    public void AFreshInstallStartsAtTheDefaults()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        Assert.Equal(30, model.CatchScanMinutes);
        Assert.Equal(90, model.CatchPositiveStaySeconds);
        Assert.Equal(30, model.CatchNegativeStaySeconds);
        Assert.Equal(3, model.CatchSurveySeconds);
    }
}
