using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 281, task 1: how much permanently-visible text the whole
/// application carries, screen by screen.
/// </summary>
/// <remarks>
/// <para>**TIM'S RULING, 2026-09-08**: *"I want clean visual screens with text only
/// where I, the user, intentionally hover."* Unit 280 measured one tab, because the
/// order it was given named one tab, and the paragraph he was complaining about was
/// on a different screen. **The ruling was always app-wide**, so the measurement is
/// too.</para>
/// <para>**MEASURED THROUGH THE REAL WINDOW RATHER THAN BY COUNTING SOURCE**, for
/// unit 280's reason: almost none of this wording is a literal in the markup, so a
/// source count measures the wrong thing and then flatters whatever moved.</para>
/// <para>**IT COUNTS TWO KINDS OF TEXT AND UNIT 280 COUNTED ONE.** A
/// <see cref="TextBlock"/> and a <see cref="GlossaryTextControl"/> both put words on
/// the screen; the second is a bare <c>Control</c> that draws its own runs, so
/// walking for <c>TextBlock</c> alone silently misses it. That is why the figure
/// here for the Digital tab does not match unit 280's, and the difference is
/// reported rather than reconciled away.</para>
/// <para>**PERMANENTLY VISIBLE MEANS WITHOUT HOVERING OR OPENING ANYTHING.** A
/// tooltip is not counted, which is the whole ruling: advice may exist, it may not
/// occupy the screen while he operates.</para>
/// <para>**IT ASSERTS NO TOTAL.** A threshold would be this session deciding how
/// terse is terse enough, which is not its call. It prints, and it fails only if it
/// measured nothing — unit 279's finder rule applied to a measurement.</para>
/// </remarks>
public sealed class HowMuchTheApplicationSaysTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the per-screen counts are printed.</param>
    public HowMuchTheApplicationSaysTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>**Every character the application shows without being asked.**</summary>
    [AvaloniaFact]
    public void TheWholeApplicationIsMeasuredForWords()
    {
        var screens = new List<(string Name, int Blocks, int Chars)>();

        foreach (var mode in new[] { "CW", "Digital", "Voice" })
        {
            var window = OperatingWindow(mode, out var panel);

            screens.Add(Measure("MainWindow — " + mode + " tab", window));

            // The workspace on its own, so the shared chrome is not counted three
            // times over when the totals are read.
            // The markup names it CwWorkspace, not CWWorkspace, so the mode
            // string is not the name and a case-sensitive compare misses it.
            var workspace = window.GetVisualDescendants().OfType<Grid>()
                .FirstOrDefault(g =>
                    string.Equals(
                        g.Name, mode + "Workspace",
                        StringComparison.OrdinalIgnoreCase));

            if (workspace is not null)
            {
                screens.Add(Measure("    of which " + mode + "Workspace", workspace));
            }

            GC.KeepAlive(panel);
        }

        screens.Add(Measure(
            "SettingsWindow",
            new SettingsWindow { DataContext = new SettingsViewModel() }));

        screens.Add(Measure(
            "AboutWindow",
            new AboutWindow { DataContext = new AboutViewModel() }));

        screens.Add(Measure(
            "ContactLogWindow",
            new ContactLogWindow
            {
                DataContext = new ContactLogViewModel(
                    Array.Empty<AdifLogRecord>(), "contacts.adi"),
            }));

        screens.Add(Measure(
            "RigDiagnosticsWindow",
            new RigDiagnosticsWindow
            {
                DataContext = new RigDiagnosticsViewModel(null, RigState.Empty),
            }));

        screens.Add(Measure(
            "FavoritesWindow",
            new FavoritesWindow { DataContext = new FavoritesViewModel() }));

        screens.Add(Measure(
            "DecisionLogWindow",
            new DecisionLogWindow { DataContext = new DecisionLogViewModel() }));

        screens.Add(Measure(
            "LogContactWindow",
            new LogContactWindow
            {
                DataContext = new LogContactViewModel(
                    new AdifContact { Call = "W3YNI" }),
            }));

        _output.WriteLine("PERMANENTLY-VISIBLE TEXT, SCREEN BY SCREEN");
        _output.WriteLine("");

        foreach (var (name, blocks, chars) in screens)
        {
            _output.WriteLine(
                chars.ToString().PadLeft(6) + " chars  "
                + blocks.ToString().PadLeft(4) + " blocks   " + name);
        }

        var total = screens
            .Where(s => !s.Name.StartsWith("    ", StringComparison.Ordinal))
            .Sum(s => s.Chars);

        _output.WriteLine("");
        _output.WriteLine("TOTAL (indented rows excluded, they are subsets): " + total);

        Assert.True(total > 0, "nothing was measured anywhere in the application");
    }

    /// <summary>The main window in one operating mode, realized.</summary>
    /// <param name="mode">CW, Digital or Voice.</param>
    /// <param name="panel">The view model behind it, kept alive by the caller.</param>
    /// <returns>The shown window.</returns>
    private static Window OperatingWindow(string mode, out MainWindowViewModel panel)
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = "FN00DJ";

        panel = new MainWindowViewModel(settings, null)
        {
            OperatingMode = mode,
            DigitalDecodedExpanded = true,
        };

        var window = new MainWindow { DataContext = panel };

        window.Show();
        Pump(window);

        if (mode == "Digital")
        {
            var slot = new DateTime(2026, 9, 8, 21, 41, 30, DateTimeKind.Utc);

            panel.AddDecodeRowForTests(
                "214130", "-11", "0.2", "1240", "CQ W3YNI FN00", slot, 14_074_000);
            panel.AddDecodeRowForTests(
                "214130", "-13", "0.3", "1310", "KC3QIS W3YNI +02", slot, 14_074_000);

            Pump(window);
        }

        return window;
    }

    /// <summary>Sum what one realized subtree shows, and print its longest.</summary>
    /// <param name="name">What to call it in the printout.</param>
    /// <param name="root">The window or subtree to walk.</param>
    /// <returns>The name, the block count and the character count.</returns>
    private (string Name, int Blocks, int Chars) Measure(string name, Visual root)
    {
        if (root is Window window && !window.IsVisible)
        {
            window.Show();
            Pump(window);
        }

        var shown = Shown(root).ToList();
        var chars = shown.Sum(t => t.Length);

        _output.WriteLine("=== " + name + " — " + chars + " chars in "
            + shown.Count + " blocks");

        foreach (var text in shown.OrderByDescending(t => t.Length))
        {
            _output.WriteLine("  " + text.Length.ToString().PadLeft(4) + "  " + text);
        }

        _output.WriteLine("");

        return (name, shown.Count, chars);
    }

    /// <summary>
    /// Everything on the screen without hovering or opening anything.
    /// </summary>
    /// <param name="root">Where to start.</param>
    /// <returns>The visible strings.</returns>
    /// <remarks>
    /// **BOTH TEXT-BEARING CONTROLS.** <see cref="GlossaryTextControl"/> is a bare
    /// <c>Control</c> that draws its own text, so it is invisible to a walk that
    /// looks only for <see cref="TextBlock"/> — which is what unit 280's harness
    /// did, and why its Digital-tab figure is lower than this one's.
    /// </remarks>
    public static IEnumerable<string> Shown(Visual root)
    {
        foreach (var visual in root.GetVisualDescendants())
        {
            var text = visual switch
            {
                TextBlock block when block.IsEffectivelyVisible => block.Text,
                GlossaryTextControl gloss when gloss.IsEffectivelyVisible => gloss.Text,
                _ => null,
            };

            if (!string.IsNullOrWhiteSpace(text))
            {
                yield return text!;
            }
        }
    }

    /// <summary>Let the layout settle so the tree is realized.</summary>
    /// <param name="window">The window to pump.</param>
    public static void Pump(Window window)
    {
        window.UpdateLayout();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
    }
}
