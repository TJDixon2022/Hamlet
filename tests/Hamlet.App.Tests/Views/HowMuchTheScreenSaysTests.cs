using System;
using System.Linq;
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
/// Work instruction 280: how much permanently-visible text the Digital tab carries.
/// </summary>
/// <remarks>
/// <para>**TIM'S RULING, 2026-09-08**: *"This is so full of unneeded text and the
/// contacts are understated. More visual, less text. Words == not as good."* The
/// unit's own `NUMBER` is characters of permanently-visible text on this tab, before
/// and after, and this is what measures it.</para>
/// <para>**MEASURED THROUGH THE REAL WINDOW RATHER THAN BY COUNTING SOURCE.** Almost
/// none of the tab's wording is a literal in the markup: it is bound to view-model
/// properties that compose sentences, so a source count would have measured the
/// wrong thing and flattered whatever was changed. This walks the realized visual
/// tree of the Digital workspace and sums what is actually on the screen.</para>
/// <para>**PERMANENTLY VISIBLE MEANS WITHOUT HOVERING OR OPENING ANYTHING.** A
/// tooltip is not counted, which is the whole point of the ruling: advice is allowed
/// to exist, it is not allowed to occupy the screen while he operates.</para>
/// <para>**IT ASSERTS NOTHING ABOUT THE TOTAL.** A threshold here would be this
/// session deciding how terse is terse enough, which is not its call. It prints, and
/// it fails only if it measured nothing at all — which is task 4 of unit 279's rule
/// applied to a measurement rather than a finder.</para>
/// </remarks>
public sealed class HowMuchTheScreenSaysTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the count and the longest strings are printed.</param>
    public HowMuchTheScreenSaysTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>**Every character the Digital tab shows without being asked.**</summary>
    [AvaloniaFact]
    public void TheDigitalTabIsMeasuredForWords()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = "FN00";

        var panel = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalDecodedExpanded = true,
        };

        var window = new MainWindow { DataContext = panel };

        window.Show();
        Pump(window);

        // A slot of traffic, so the panel is showing what it shows while he works
        // rather than an empty tab.
        var slot = new DateTime(2026, 9, 8, 21, 41, 30, DateTimeKind.Utc);

        panel.AddDecodeRowForTests(
            "214130", "-11", "0.2", "1240", "CQ W3YNI FN00", slot, 14_074_000);
        panel.AddDecodeRowForTests(
            "214130", "-13", "0.3", "1310", "KC3QIS W3YNI +02", slot, 14_074_000);

        Pump(window);

        var workspace = window.GetVisualDescendants()
            .OfType<Grid>()
            .FirstOrDefault(g => g.Name == "DigitalWorkspace");

        Assert.True(
            workspace is not null,
            "DigitalWorkspace is not on the realized window. Named grids: ["
            + string.Join(", ", window.GetVisualDescendants().OfType<Grid>()
                .Where(g => g.Name is not null).Select(g => g.Name)) + "]");

        var shown = workspace!.GetVisualDescendants()
            .OfType<TextBlock>()
            .Where(t => t.IsEffectivelyVisible && !string.IsNullOrWhiteSpace(t.Text))
            .Select(t => t.Text!)
            .ToList();

        var chars = shown.Sum(t => t.Length);

        _output.WriteLine("Digital tab, permanently visible:");
        _output.WriteLine("  text blocks : " + shown.Count);
        _output.WriteLine("  characters  : " + chars);
        _output.WriteLine("");
        _output.WriteLine("the longest of them:");

        foreach (var text in shown.OrderByDescending(t => t.Length).Take(12))
        {
            _output.WriteLine("  " + text.Length.ToString().PadLeft(4) + "  " + text);
        }

        // **A MEASUREMENT THAT MEASURED NOTHING HAS NOT PASSED**, which is unit
        // 279's rule applied here. Everything else is printed and judged by a
        // person.
        Assert.True(chars > 0, "nothing was measured on the Digital tab");
    }

    /// <summary>Let the layout settle so the workspace is realized.</summary>
    private static void Pump(Window window)
    {
        window.UpdateLayout();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
    }
}
