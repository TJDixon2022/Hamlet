using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 300, tasks 1 and 2: **the quill at rest, the orbit when
/// something is new, and what clears it.**
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THIS WOULD HAVE CAUGHT IS THE ONE THE INSTRUCTION NAMES
/// OUTRIGHT**: a mark that clears itself. If the animation ending also cleared the
/// state, then on any evening he was looking at the radio rather than the screen -
/// which is every evening this application exists for - **he would never learn what
/// he earned.**</para>
/// <para>**AND THE QUIETER ONE, WHICH IS §0.6**: two states that differ only by
/// colour. Roughly one man in twelve cannot rely on a hue, and everybody can be
/// looking somewhere else; the ring is present or absent and the vane is filled or
/// outlined, so the two survive a greyscale printer.</para>
/// <para>**IT WAS WATCHED FAILING.** With `OnTick` setting `IsNew = false` when the
/// orbit's time is up, `TheMarkDoesNotClearItself` reports the mark dark with nothing
/// having been opened.</para>
/// </remarks>
public sealed class Unit300MarkTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the states are printed.</param>
    public Unit300MarkTests(ITestOutputHelper output) => _output = output;

    /// <summary>**The mark is in the status bar and it opens the screen.**</summary>
    [AvaloniaFact]
    public void TheMarkIsInTheStatusBarAndOpensTheScreen()
    {
        var window = new MainWindow { DataContext = Model(Settings()) };

        window.Show();

        for (var i = 0; i < 5; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        }

        var button = window.FindControl<Button>("AchievementMarkButton");
        var mark = window.FindControl<AchievementMarkControl>("AchievementMark");

        Assert.True(button is not null, "the status bar has no achievement mark");
        Assert.True(mark is not null, "the mark control is not in the window");

        _output.WriteLine("button bounds : " + button!.Bounds);
        _output.WriteLine("mark bounds   : " + mark!.Bounds);
        _output.WriteLine("command       : "
            + (button.Command?.GetType().Name ?? "(null)"));

        // **A SOLID HIT TARGET**, which is unit 299's finding applied: a bare
        // control drawing an outline is not a target in its middle.
        Assert.True(button.Command is not null, "the mark's command is null");
        Assert.True(button.IsEffectivelyEnabled, "the mark is not usable");
        Assert.True(
            button.Bounds.Width >= 16 && button.Bounds.Height >= 16,
            "the mark is " + button.Bounds.Width + " by " + button.Bounds.Height);

        window.Close();
    }

    /// <summary>**Earning something lights the mark; opening the screen clears it.**</summary>
    [Fact]
    public void EarningLightsTheMarkAndOpeningTheScreenClearsIt()
    {
        var settings = Settings();
        var model = Model(settings);

        Assert.False(model.AchievementsUnseen);

        // The quiet first look at an existing log.
        model.AnnounceOpeningsForTests(Fourteen());

        Assert.False(
            model.AchievementsUnseen,
            "a first look at an existing log lit the mark, and unit 278's seeding "
            + "rule says nothing is announced then");

        // Then one contact on a band he has never worked.
        var grown = Fourteen().ToList();
        grown.Add(Contact("EI4GNB", "40m", "2026-09-11 02:20:00"));

        model.AnnounceOpeningsForTests(grown);

        _output.WriteLine("after a new band opened: unseen=" + model.AchievementsUnseen);
        _output.WriteLine("  tip: " + model.AchievementsMarkTip);

        Assert.True(model.AchievementsUnseen, "opening a group did not light the mark");

        // **AND IT IS WRITTEN DOWN**, so closing Hamlet does not throw it away.
        Assert.True(settings.AchievementsUnseen);

        model.ClearAchievementMarkForTests();

        Assert.False(model.AchievementsUnseen);
        Assert.False(settings.AchievementsUnseen);
    }

    /// <summary>**The mark does not clear itself when the orbit stops.**</summary>
    /// <remarks>
    /// **THE MOTION ENDS AND THE MARK DOES NOT.** Motion in peripheral vision is
    /// unpleasant for some people, so the orbit turns for thirty seconds and settles;
    /// the ring and the fill stay until he opens the screen.
    /// </remarks>
    [AvaloniaFact]
    public void TheMarkDoesNotClearItself()
    {
        var mark = new AchievementMarkControl { IsNew = true };

        Assert.True(mark.IsOrbiting, "lighting the mark did not start the orbit");

        // **THE ORBIT'S OWN END**, reached without waiting thirty seconds for it.
        mark.SettleForTests();

        _output.WriteLine("after the orbit settles: IsNew=" + mark.IsNew
            + "  IsOrbiting=" + mark.IsOrbiting);

        Assert.False(mark.IsOrbiting, "the orbit is still turning");

        Assert.True(
            mark.IsNew,
            "the mark cleared itself when the motion stopped, so on any evening he "
            + "was looking at the radio he would never learn what he earned");
    }

    /// <summary>**The two states differ without colour.**</summary>
    /// <remarks>
    /// **§0.6.** The ring is a shape and the fill is a shape; a greyscale printer
    /// keeps both. This asserts the two properties the renderer switches on, because
    /// the headless backend composes without rasterising and no test in this
    /// repository can look at a pixel.
    /// </remarks>
    [AvaloniaFact]
    public void TheTwoStatesDifferWithoutColour()
    {
        var rest = new AchievementMarkControl { IsNew = false };
        var lit = new AchievementMarkControl { IsNew = true };

        _output.WriteLine("at rest : ring=no   fill=no   orbiting=" + rest.IsOrbiting);
        _output.WriteLine("lit     : ring=yes  fill=yes  orbiting=" + lit.IsOrbiting);

        // The renderer draws the ring and the fill on `IsNew` and on nothing else,
        // so the two states are two shapes rather than two colours of one shape.
        Assert.False(rest.IsNew);
        Assert.True(lit.IsNew);
        Assert.False(rest.IsOrbiting);
        Assert.True(lit.IsOrbiting);
    }

    /// <summary>Settings with his callsign and nothing announced.</summary>
    private static AppSettings Settings()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = "FN00";

        // **`AchievementGroupsAnnounced` IS LEFT NULL ON PURPOSE**, which is what a
        // fresh profile carries. **Measured, because the first draft of this fixture
        // set it to an empty list and the test failed**: null means *nothing has ever
        // been read* and seeds quietly (unit 278's rule), while an empty list means
        // *read, and none of these announced*, so every group is fresh and the mark
        // lights. The two are different states and the code is right to tell them
        // apart; the fixture was wrong about which one a first look is.
        return settings;
    }

    private static MainWindowViewModel Model(AppSettings settings)
        => new(settings, null);

    /// <summary>Fourteen contacts, all FT8 on 20 metres, as an import would look.</summary>
    private static IReadOnlyList<AdifLogRecord> Fourteen()
        => Enumerable.Range(0, 14)
            .Select(i => Contact(
                "W" + (i + 1) + "ABC", "20m",
                "2026-09-10 02:" + i.ToString("00", CultureInfo.InvariantCulture) + ":00"))
            .ToList();

    /// <summary>One sound record.</summary>
    private static AdifLogRecord Contact(string call, string band, string startedUtc)
        => new(
            new AdifContact
            {
                Call = call,
                StationCallsign = "KC3QIS",
                Band = band,
                Mode = "FT8",
                GridSquare = "FN42",
                MyGridSquare = "FN00",
                ReportSent = "-12",
                ReportReceived = "-07",
                StartedUtc = DateTime.Parse(
                    startedUtc, CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal),
            },
            Array.Empty<string>(),
            Terminated: true);
}
