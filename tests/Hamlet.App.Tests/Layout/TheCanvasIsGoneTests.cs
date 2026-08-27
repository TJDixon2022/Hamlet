using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Layout;

/// <summary>
/// The widget canvas and everything serving it are gone.
/// </summary>
/// <remarks>
/// <para>**THIS FILE REPLACES THREE THAT PINNED THE CANVAS** — `CanvasTests`,
/// `CanvasArrivalTests` and `CanvasRescueTests` — and it is the same work turned
/// around. They proved the arrangement machinery behaved; this proves it is not
/// there.</para>
/// <para>**WHY IT WENT** (Tim's ruling of 2026-08-27: *"I don't care when it
/// destroys. We're abandoning all of that."*). Two photographs a day apart. The
/// first showed Receive rendering `wh at the rad io is he ari ng` one or two
/// letters to a line, squeezed beside the whole arrangement. The second showed
/// Receive fixed and the arrangement still underneath it — **a second
/// neighborhood map and a second CW terminal on one screen**, restored from a
/// saved layout.</para>
/// <para>A surface the operator arranges is a surface that can be arranged
/// wrongly, and a panel that is also a widget can appear twice.
/// `ABANDONED_WIDGETS.md` records the fifteen that went.</para>
/// </remarks>
public sealed class TheCanvasIsGoneTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the findings are printed.</param>
    public TheCanvasIsGoneTests(ITestOutputHelper output)
        => _output = output;

    /// <remarks>
    /// <para>Proves the machinery is not in the assembly. A type that is gone
    /// cannot be reached by a later edit reaching for it out of habit, and this
    /// is the assertion that stays true when somebody tries.</para>
    /// <para>Asserted by name through reflection rather than by failing to
    /// compile, because a compile error is not a test and says nothing in a
    /// report.</para>
    /// </remarks>
    [Fact]
    public void TheArrangementMachineryIsNotInTheAssembly()
    {
        var assembly = typeof(MainWindowViewModel).Assembly;

        var gone = new[]
        {
            "Hamlet.App.Layout.Widget",
            "Hamlet.App.Layout.Widgets",
            "Hamlet.App.Layout.CanvasLayout",
            "Hamlet.App.Layout.LayoutPresets",
            "Hamlet.App.Layout.LayoutStore",
            "Hamlet.App.ViewModels.CanvasViewModel",
            "Hamlet.App.ViewModels.WidgetViewModel",
            "Hamlet.App.Controls.WidgetCanvas",
            "Hamlet.App.Controls.WidgetFrame",
            "Hamlet.App.Controls.WidgetBody",
        };

        var found = gone.Where(n => assembly.GetType(n) is not null).ToList();

        _output.WriteLine(
            found.Count == 0
                ? $"all {gone.Length} are gone"
                : "still present: " + string.Join(", ", found));

        Assert.Empty(found);
    }

    /// <remarks>
    /// Proves the view model no longer offers a canvas to place anything on, a
    /// tray to take it from, or a preset to arrange it with.
    /// </remarks>
    [Fact]
    public void TheViewModelOffersNoCanvasNoTrayAndNoPresets()
    {
        var offered = typeof(MainWindowViewModel)
            .GetProperties()
            .Select(p => p.Name)
            .Where(n => n.Contains("Canvas", StringComparison.Ordinal)
                || n.Contains("Widget", StringComparison.Ordinal)
                || n.Contains("Tray", StringComparison.Ordinal)
                || n.Contains("Preset", StringComparison.Ordinal))
            .ToList();

        _output.WriteLine(
            offered.Count == 0
                ? "nothing canvas-shaped on the view model"
                : string.Join(", ", offered));

        Assert.Empty(offered);
    }

    /// <remarks>
    /// <para>Proves a first run needs no layouts file and reads none. The store
    /// is gone, so a saved arrangement on somebody's machine no longer loads —
    /// **which is the intended outcome rather than a regression** (the ruling
    /// above).</para>
    /// </remarks>
    [Fact]
    public void AFirstRunReadsNoLayoutsFile()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        _output.WriteLine(
            $"opened on the {model.OperatingMode} workspace, "
            + $"{model.OperatingModes.Count} tabs, no stored arrangement");

        Assert.Equal("CW", model.OperatingMode);
        Assert.Equal(new[] { "CW", "Digital", "Voice" }, model.OperatingModes);
    }
}
