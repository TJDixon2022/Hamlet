using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// The `everything` / `CQ` chips are on the screen before anything decodes
/// (`PHASE_PLAN.md` §R17, work instruction 325 task 2).
/// </summary>
/// <remarks>
/// <para>**THE FAULT THIS ANSWERS.** The whole header strip was hidden behind
/// `HasDigitalDecodes`, so the filter appeared only once a slot had landed. Tim
/// could not choose what he wanted to see until after the thing he wanted to
/// filter had already gone past. A choice about what to see is not a report
/// about what is there, and it does not wait for one.</para>
/// <para>**`clear` AND THE ORDER TOGGLE STILL WAIT**, and that is deliberate:
/// they are reports about rows, there is nothing to clear or order on an empty
/// table, and a live control that does nothing is HM-DEC-087's fault.</para>
/// <para>**EVERY APPEARANCE CLAIM HERE IS COMPUTED, NOT SEEN.** The window is
/// built headless and `IsEffectivelyVisible` is read off the realized tree.</para>
/// </remarks>
public sealed class TheFilterIsAlwaysThereTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the chips' visibility is printed.</param>
    public TheFilterIsAlwaysThereTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>Both chips are on the window with nothing decoded.</summary>
    [AvaloniaFact]
    public void TheChipsArePresentWithZeroRows()
    {
        var model = OnTheDigitalTab();
        var window = Shown(model);

        Assert.Empty(model.DigitalDecodes);
        Assert.False(model.HasDigitalDecodes);

        var everything = Named<Button>(window, "DigitalFilterEverything");
        var cq = Named<Button>(window, "DigitalFilterCq");

        _output.WriteLine(
            "rows=" + model.DigitalDecodes.Count
            + "  everything visible=" + everything.IsEffectivelyVisible
            + "  CQ visible=" + cq.IsEffectivelyVisible);

        Assert.True(
            everything.IsEffectivelyVisible,
            "the everything chip is not effectively visible on an empty list");
        Assert.True(
            cq.IsEffectivelyVisible,
            "the CQ chip is not effectively visible on an empty list");

        // And both can actually be pressed, rather than being drawn and dead
        // (§0.5.1, HM-DEC-087).
        Assert.True(everything.IsEnabled);
        Assert.True(cq.IsEnabled);
        Assert.NotNull(everything.Command);
        Assert.NotNull(cq.Command);
    }

    /// <summary>`clear` and the order toggle stay away until there are rows.</summary>
    [AvaloniaFact]
    public void TheRowControlsStillWaitForRows()
    {
        var model = OnTheDigitalTab();
        var window = Shown(model);

        var clear = Named<Button>(window, "DigitalClearPress");
        var order = Named<Button>(window, "DigitalOrderToggle");

        _output.WriteLine(
            "empty: clear visible=" + clear.IsEffectivelyVisible
            + "  order visible=" + order.IsEffectivelyVisible);

        Assert.False(clear.IsEffectivelyVisible);
        Assert.False(order.IsEffectivelyVisible);
    }

    /// <summary>Choosing CQ before any decode filters the first slot.</summary>
    /// <remarks>
    /// **THIS IS THE WHOLE POINT OF THE TASK.** The chip existing is worth
    /// nothing if the choice made on it does not survive to meet the rows it
    /// was made about.
    /// </remarks>
    [Fact]
    public void ChoosingCqBeforeAnyDecodeFiltersTheFirstSlot()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = "KD9ABC";

        var model = new MainWindowViewModel(settings, null);

        // The choice, made on an empty list.
        Assert.Empty(model.DigitalDecodes);
        model.ToggleDecodedCqCommand.Execute(null);
        Assert.True(model.ShowsCqOnly);

        // The first slot lands afterwards.
        model.AddDecodeRowForTests("214135", "-11", "0.2", "1240", "CQ TA3MPK KM39");
        model.AddDecodeRowForTests("214135", "-04", "0.2", "1400", "KE9COB N5CH R+14");
        model.AddDecodeRowForTests("214135", "-06", "0.2", "1500", "TNX FER QSO OM");

        _output.WriteLine(
            "heard " + model.DigitalDecodes.Count
            + ", shown " + model.DigitalShownCount
            + ", hidden " + model.DigitalHiddenCount);

        Assert.Equal(3, model.DigitalDecodes.Count);
        Assert.Equal(1, model.DigitalShownCount);
        Assert.Equal(2, model.DigitalHiddenCount);
        Assert.Contains(model.DigitalVisibleDecodes, r => r.Message == "CQ TA3MPK KM39");
    }

    /// <summary>Switching PSK31 to FT8 keeps the choice.</summary>
    /// <remarks>
    /// **THE FILTER IS A PROPERTY OF THE OPERATOR, NOT OF THE MODE.** He chose
    /// to be shown calls he could answer; changing which digital mode is running
    /// does not change that, and silently resetting it would put rows back on a
    /// list he had asked to be kept clear.
    /// </remarks>
    [Fact]
    public void SwitchingModesKeepsTheChoice()
    {
        var model = new MainWindowViewModel(new AppSettings(), null)
        {
            OperatingMode = "Digital",
        };

        model.ChosenDigitalMode = "PSK31";
        model.ShowsCqOnly = true;

        model.ChosenDigitalMode = "FT8";

        _output.WriteLine(
            "after PSK31 -> FT8 : CQ=" + model.ShowsCqOnly
            + " everything=" + model.ShowsEverything);

        Assert.True(model.ShowsCqOnly);
        Assert.False(model.ShowsEverything);

        model.ChosenDigitalMode = "FT4";

        Assert.True(model.ShowsCqOnly);
    }

    private static MainWindowViewModel OnTheDigitalTab()
        => new(new AppSettings(), null)
        {
            // The digital workspace is collapsed unless this tab is showing, so
            // without it the chips are never realized and every visibility
            // assertion here would pass by finding nothing.
            OperatingMode = "Digital",
            DigitalDecodedExpanded = true,
        };

    private static MainWindow Shown(MainWindowViewModel model)
    {
        var window = new MainWindow { DataContext = model };

        window.Show();

        for (var i = 0; i < 5; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }

        return window;
    }

    private static T Named<T>(MainWindow window, string name)
        where T : Control
    {
        var found = window.GetVisualDescendants()
            .OfType<T>()
            .FirstOrDefault(c => c.Name == name);

        Assert.True(
            found is not null,
            name + " is not on the realized window. Named " + typeof(T).Name
            + "s: [" + string.Join(", ", window.GetVisualDescendants()
                .OfType<T>().Where(c => c.Name is not null)
                .Select(c => c.Name)) + "]");

        return found!;
    }
}
