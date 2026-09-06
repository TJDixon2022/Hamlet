using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 251, task 3: the app opens in the mode it was last in, and
/// starting it moves nothing.
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THIS WOULD HAVE CAUGHT.** `OperatingMode` was a field
/// initialised to `"CW"` and `ModeTabs[0].Follow(true)` was called
/// unconditionally, so an operator who spent an evening on Digital opened the
/// next one on CW every time. Nothing wrote the tab to `settings.json` at all.</para>
/// <para>**AND THE ONE THAT MATTERS MORE.** Restoring a mode must not reach the
/// radio. `OnOperatingModeChanged` calls `ScheduleModeFollow`, which is how
/// arriving on a tab puts the rig into USB-D; run during construction against a
/// restored value it would make starting the app a thing that reconfigures the
/// radio. Starting Hamlet has never moved the operator's dial, and §0.2.1's whole
/// argument is that changing the radio out from under the person sitting at it is
/// a different category from reading it.</para>
/// </remarks>
public sealed class TheAppOpensWhereItWasLeftTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the test.</summary>
    /// <param name="output">Where the restored state is printed.</param>
    public TheAppOpensWhereItWasLeftTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The remembered tab and sub-mode are the ones it opens on.</summary>
    [Fact]
    public void ARememberedTabAndSubModeAreWhatItOpensOn()
    {
        var settings = new AppSettings
        {
            LastOperatingMode = "Digital",
            LastDigitalSubMode = "FT4",
        };

        var model = new MainWindowViewModel(settings, null);

        _output.WriteLine("opened on : " + model.OperatingMode);
        _output.WriteLine("sub-mode  : " + model.ChosenDigitalMode);

        Assert.Equal("Digital", model.OperatingMode);
        Assert.True(model.IsDigitalMode);
        Assert.False(model.IsCwMode);
        Assert.Equal("FT4", model.ChosenDigitalMode);

        // **THE TAB STRIP AGREES WITH THE WORKSPACE.** Selection is state on the
        // tab rather than a comparison done at render time, so restoring the mode
        // has to put the tabs in step itself - and a strip showing CW over a
        // Digital workspace is the fault `ModeTabViewModel` exists to prevent.
        Assert.Equal(
            new[] { false, true, false },
            model.ModeTabs.Select(t => t.IsSelected).ToArray());

        // **THE CHIP KNOWS IT WAS CHOSEN AND DOES NOT CLAIM TO BE LIT.** No radio
        // is attached and nothing has tuned, so FT4 is chosen and the dial is
        // somewhere else - which is its own appearance, not the lit one (§0.0).
        var ft4 = model.DigitalModeChips.Single(c => c.Label == "FT4");

        Assert.True(ft4.IsChosen);
        Assert.False(ft4.IsLit);
        Assert.True(ft4.IsChosenElsewhere);
    }

    /// <summary>
    /// Restoring the mode does not ask to write anything to the radio.
    /// </summary>
    /// <remarks>
    /// **`ModeFollowReschedules` IS COUNTED BEFORE ITS OWN GUARD**, which is what
    /// makes it the right thing to assert here: it counts how often a mode write
    /// was *asked for*, whether or not a radio was attached. A restore that leaves
    /// it at nought did not merely fail to reach a radio that was not there - it
    /// never asked.
    /// </remarks>
    [Fact]
    public void RestoringTheModeAsksForNoRadioWrite()
    {
        var restored = new MainWindowViewModel(
            new AppSettings { LastOperatingMode = "Digital" }, null);

        _output.WriteLine("reschedules after a restore : "
            + restored.ModeFollowReschedules);

        Assert.Equal(0, restored.ModeFollowReschedules);

        // **AND THE DIAL IS WHERE A FRESH START PUTS IT.** Same band, same
        // frequency, whichever tab was remembered - the restore chose a tab and
        // not a place on the band.
        var fresh = new MainWindowViewModel(new AppSettings(), null);

        Assert.Equal(fresh.FrequencyHz, restored.FrequencyHz);
        Assert.Equal(fresh.SelectedBand.Band.Name, restored.SelectedBand.Band.Name);

        // A press, by contrast, does ask - which is the behaviour a restore must
        // not share. Without this the assertion above would pass on a view model
        // that had simply stopped following the map at all.
        restored.OperatingMode = "CW";

        _output.WriteLine("reschedules after a press   : "
            + restored.ModeFollowReschedules);

        Assert.True(
            restored.ModeFollowReschedules > 0,
            "arriving on a tab stopped asking for a mode write altogether, so "
            + "the restore assertion above proves nothing");
    }

    /// <summary>Both facts reach `settings.json` and come back out of it.</summary>
    [Fact]
    public void BothFactsSurviveTheSettingsFile()
    {
        var settings = new AppSettings();
        var model = new MainWindowViewModel(settings, null);

        Assert.Equal("CW", model.OperatingMode);
        Assert.Null(model.ChosenDigitalMode);

        model.OperatingMode = "Voice";
        model.ChooseDigitalModeCommand.Execute("PSK31");

        Assert.Equal("Voice", settings.LastOperatingMode);
        Assert.Equal("PSK31", settings.LastDigitalSubMode);

        var path = Path.Combine(
            Path.GetTempPath(),
            "hamlet-unit251-" + Guid.NewGuid().ToString("N") + ".json");

        try
        {
            SettingsStore.SaveTo(settings, path);

            var reloaded = SettingsStore.LoadFrom(path);

            _output.WriteLine("reloaded mode     : " + reloaded.LastOperatingMode);
            _output.WriteLine("reloaded sub-mode : " + reloaded.LastDigitalSubMode);

            Assert.Equal("Voice", reloaded.LastOperatingMode);
            Assert.Equal("PSK31", reloaded.LastDigitalSubMode);

            var reopened = new MainWindowViewModel(reloaded, null);

            Assert.Equal("Voice", reopened.OperatingMode);
            Assert.Equal("PSK31", reopened.ChosenDigitalMode);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// A fresh settings file opens on the same defaults it always did.
    /// </summary>
    [Fact]
    public void AFreshSettingsFileOpensOnTheOldDefaults()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        Assert.Equal("CW", model.OperatingMode);
        Assert.True(model.ModeTabs[0].IsSelected);
        Assert.Null(model.ChosenDigitalMode);

        // Nothing is chosen, so no chip claims to have been picked.
        Assert.All(model.DigitalModeChips, c => Assert.False(c.IsChosen));
        Assert.All(model.DigitalModeChips, c => Assert.True(c.IsPlain || c.IsLit));
    }

    /// <summary>
    /// A settings file naming a tab or a sub-mode that does not exist opens on
    /// the defaults rather than on nothing.
    /// </summary>
    /// <remarks>
    /// **THE BLANK SCREEN OF 2026-08-27.** An unmatched mode string left no
    /// workspace visible at all and the operator photographed it. A settings file
    /// is an input like any other and a value it cannot honour must land
    /// somewhere the app can still be used.
    /// </remarks>
    [Fact]
    public void AModeThatDoesNotExistOpensOnCwRatherThanOnNothing()
    {
        var model = new MainWindowViewModel(
            new AppSettings
            {
                LastOperatingMode = "Packet",
                LastDigitalSubMode = "JS8",
            },
            null);

        _output.WriteLine("opened on : " + model.OperatingMode);
        _output.WriteLine("sub-mode  : " + (model.ChosenDigitalMode ?? "<none>"));

        Assert.Equal("CW", model.OperatingMode);
        Assert.True(model.IsCwMode);
        Assert.True(model.ModeTabs[0].IsSelected);

        // JS8 is a real mode the map knows and the strip has no chip for. A
        // preference nothing can draw is dropped rather than shown.
        Assert.Null(model.ChosenDigitalMode);
    }

    /// <summary>
    /// The chips on the real window are pressable, with a command that resolves.
    /// </summary>
    /// <remarks>
    /// **A BINDING THAT DOES NOT RESOLVE IS A DEFECT, NOT A DIAGNOSTIC**
    /// (§0.5.1, HM-DEC-087). Avalonia yields null on a failed cast rather than
    /// throwing, and a button whose command is null renders and behaves exactly
    /// like a disabled one - so a chip strip nobody can press would look
    /// identical to the label strip it replaced.
    /// </remarks>
    [AvaloniaFact]
    public void EveryChipOnTheStripHasACommandThatResolved()
    {
        var model = new MainWindowViewModel(new AppSettings(), null)
        {
            OperatingMode = "Digital",
        };

        var window = new MainWindow
        {
            DataContext = model,
            Width = 1400,
            Height = 1200,
        };

        window.Show();

        for (var i = 0; i < 5; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }

        var strip = window.GetVisualDescendants()
            .OfType<ItemsControl>()
            .FirstOrDefault(c => c.Name == "DigitalModeChipStrip");

        Assert.True(strip is not null, "the mode strip is not in the window");

        var chips = strip!.GetVisualDescendants()
            .OfType<Button>()
            .ToList();

        _output.WriteLine("chips realized : " + chips.Count);

        Assert.Equal(DigitalModeChip.Labels.Count, chips.Count);

        Assert.All(
            chips,
            b => Assert.True(
                b.Command is not null,
                "a chip's command did not resolve, so it renders as a label "
                + "that looks pressable and is not"));

        Assert.All(chips, b => Assert.True(b.IsEffectivelyEnabled));

        window.Close();
    }
}
