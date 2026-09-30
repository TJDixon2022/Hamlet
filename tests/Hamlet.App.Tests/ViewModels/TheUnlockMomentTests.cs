using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **The moment something unlocks** (work instruction 506 task 5, HM-DEC-211): a contact written to the log
/// that earns something he did not have shows a panel over the main window, and one that earns nothing
/// shows nothing.
/// </summary>
/// <remarks>
/// **A SCRATCH DATA FOLDER, SO THE LOG AND THE POINTS FILE ARE THE TEST'S OWN**, with the shipped points file
/// placed where the application reads it. The write goes through <c>WriteLoggedContactForTests</c>, the Save
/// button's own door.
/// </remarks>
public sealed class TheUnlockMomentTests : IDisposable
{
    private const string MyGrid = "FN00";

    private readonly ITestOutputHelper _output;
    private readonly string _folder;
    private readonly string _wasFolder;

    /// <summary>Points the settings at a scratch folder.</summary>
    /// <param name="output">Where each case prints the panel.</param>
    public TheUnlockMomentTests(ITestOutputHelper output)
    {
        _output = output;
        _wasFolder = SettingsStore.DataFolder;
        _folder = Path.Combine(Path.GetTempPath(), "hamlet-unit506-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_folder);
        SettingsStore.DataFolder = _folder;
    }

    /// <summary>Puts the settings back and takes the folder away.</summary>
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

    private static void PointsFile(string json)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsStore.AchievementPointsPath)!);
        File.WriteAllText(SettingsStore.AchievementPointsPath, json);
    }

    private static AdifContact Contact(string call, string grid, string band, DateTime at)
        => new()
        {
            Call = call,
            StationCallsign = "KC3QIS",
            Mode = "FT8",
            Band = band,
            GridSquare = grid,
            MyGridSquare = MyGrid,
            StartedUtc = at,
            EndedUtc = at.AddMinutes(2),
        };

    private static MainWindowViewModel Panel()
    {
        var settings = new AppSettings();

        settings.Operator.GridSquare = MyGrid;

        return new MainWindowViewModel(settings, null);
    }

    private void Print(string what, MainWindowViewModel model)
        => _output.WriteLine(what + ": " + (model.UnlockIsOpen && model.Unlocked is { } m
            ? m.Heading + " | " + m.Name + " | " + m.Sentence + " | " + m.PointsAdded + " | " + m.RankCrossed + " | " + m.AlsoLine + " | " + m.BarLeft + " .. " + m.BarRight + " | " + m.BarUnder
            : "no panel"));

    /// <remarks>
    /// A logged contact that earns a new country shows the panel naming what it earned, with the points it
    /// added; one that earns nothing shows nothing.
    /// </remarks>
    [AvaloniaFact]
    public void ANewCountryShowsThePanelAndNothingNewShowsNothing()
    {
        PointsFile(AchievementPoints.Shipped());

        var model = Panel();
        var first = new DateTime(2026, 9, 28, 14, 0, 0, DateTimeKind.Utc);

        Assert.True(model.WriteLoggedContactForTests(Contact("LA1ZZZ", "JO59", "20m", first)));
        Print("first contact, Norway", model);

        Assert.True(model.UnlockIsOpen);
        Assert.NotNull(model.Unlocked);
        Assert.StartsWith("+", model.Unlocked!.PointsAdded, StringComparison.Ordinal);

        model.CloseUnlockCommand.Execute(null);
        Assert.False(model.UnlockIsOpen);

        // **A NEW COUNTRY**: Norway is in the log, so a first contact with Italy earns the country.
        Assert.True(model.WriteLoggedContactForTests(Contact("I2ZZZ", "JN45", "20m", first.AddDays(1))));
        Print("Italy", model);

        var italy = model.Unlocked!;

        Assert.True(model.UnlockIsOpen);
        Assert.Contains(new[] { italy.Name }.Concat(italy.AlsoLine.Split(", ")), s => s.Contains("Italy", StringComparison.Ordinal));
        Assert.Contains("I2ZZZ", italy.Sentence, StringComparison.Ordinal);
        Assert.Contains("Italy", italy.Sentence, StringComparison.Ordinal);
        Assert.Matches(@"^\+\d[\d,]* points?$", italy.PointsAdded);

        model.CloseUnlockCommand.Execute(null);

        // **NOTHING NEW**: the same station, grid, band and mode again earns nothing he does not have.
        Assert.True(model.WriteLoggedContactForTests(Contact("I2ZZZ", "JN45", "20m", first.AddDays(2))));
        Print("Italy again", model);

        Assert.False(model.UnlockIsOpen);
    }

    /// <remarks>A contact that crosses a rank says the new rank in the panel.</remarks>
    [AvaloniaFact]
    public void CrossingARankSaysTheRank()
    {
        var shipped = AchievementPoints.Parse(AchievementPoints.Shipped());
        var one = new[] { new AdifLogRecord(Contact("LA1ZZZ", "JO59", "20m", new DateTime(2026, 9, 28, 14, 0, 0, DateTimeKind.Utc)), Array.Empty<string>(), true) };
        var total = new AchievementScores(new AchievementLog(one, MyGrid), shipped).Total!.Value;

        // **THE FIRST RANK OPENS AT EXACTLY ONE CONTACT'S WORTH**, so the first contact crosses it.
        PointsFile(Views.TheAchievementsStandingTests.WithRanksJson(new long[] { total, total + 1000 }));

        var model = Panel();

        Assert.True(model.WriteLoggedContactForTests(one[0].Contact));
        Print("crossing a rank", model);

        Assert.True(model.UnlockIsOpen);
        Assert.Equal("You reached Rank 2.", model.Unlocked!.RankCrossed);
        Assert.Equal(0, model.Unlocked.BeforeFraction);
    }

    /// <remarks>An unreadable points file: no panel.</remarks>
    [AvaloniaFact]
    public void AnUnreadablePointsFileShowsNoPanel()
    {
        PointsFile("{ this is not json");

        var model = Panel();

        Assert.True(model.WriteLoggedContactForTests(Contact("LA1ZZZ", "JO59", "20m", new DateTime(2026, 9, 28, 14, 0, 0, DateTimeKind.Utc))));
        Print("unreadable file", model);

        Assert.False(model.UnlockIsOpen);
    }

    /// <remarks>
    /// On the realized main window the panel opens without taking the keyboard, and closes the moment the
    /// radio reports it is transmitting.
    /// </remarks>
    [AvaloniaFact]
    public void ATransmissionClosesItAndItHoldsNoFocus()
    {
        PointsFile(AchievementPoints.Shipped());

        var model = Panel();
        var window = new MainWindow { DataContext = model };

        window.Show();

        for (var i = 0; i < 5; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        }

        try
        {
            var focusedBefore = window.FocusManager?.GetFocusedElement();

            Assert.True(model.WriteLoggedContactForTests(Contact("LA1ZZZ", "JO59", "20m", new DateTime(2026, 9, 28, 14, 0, 0, DateTimeKind.Utc))));

            for (var i = 0; i < 5; i++)
            {
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            }

            var popup = window.GetVisualDescendants().OfType<Popup>().Single(p => p.Name == "UnlockPanel");
            var focused = window.FocusManager?.GetFocusedElement();

            _output.WriteLine("open " + popup.IsOpen + "; focused before " + focusedBefore?.GetType().Name + ", after " + focused?.GetType().Name);

            Assert.True(popup.IsOpen, "the unlock panel did not open");
            Assert.Same(focusedBefore, focused);
            Assert.All(
                popup.Child!.GetVisualDescendants().OfType<InputElement>().Where(e => e is Button),
                b => Assert.False(b.Focusable, "a button on the unlock panel can take the keyboard"));

            model.ApplyRigState(RigState.Empty.With(RigValue.Known(RigField.TransmitStatus, 1, "transmitting", DateTime.UtcNow, "poll")));

            for (var i = 0; i < 5; i++)
            {
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            }

            Assert.False(model.UnlockIsOpen);
            Assert.False(popup.IsOpen, "the unlock panel outlived the start of a transmission");
        }
        finally
        {
            window.Close();
        }
    }
}
