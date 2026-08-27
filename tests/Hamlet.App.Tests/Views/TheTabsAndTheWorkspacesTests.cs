using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// The tabs begin where the workspace begins, each one changes it, and the
/// header outlives a mode change.
/// </summary>
/// <remarks>
/// <para>**THE DIVIDER IS THE WHOLE POINT AND ONLY A TEST KEEPS IT TRUE** (Tim's
/// ruling of 2026-08-27). The band plan, the neighborhood and the radio are the
/// same in every mode; if a later layout edit slides them inside the tab region
/// they will start being torn down and rebuilt on every switch, and nothing on
/// screen will say so until the operator notices a flicker.</para>
/// <para>Nothing here hit-tests, per unit 1.11.13's rule.</para>
/// </remarks>
public sealed class TheTabsAndTheWorkspacesTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the geometry is printed.</param>
    public TheTabsAndTheWorkspacesTests(ITestOutputHelper output)
        => _output = output;

    private static Rect OnScreen(Visual visual, Visual root)
    {
        var x = 0.0;
        var y = 0.0;

        for (var at = visual; at is not null && at != root; at = at.GetVisualParent())
        {
            x += at.Bounds.X;
            y += at.Bounds.Y;
        }

        return new Rect(x, y, visual.Bounds.Width, visual.Bounds.Height);
    }

    private static void With(Action<MainWindow, MainWindowViewModel> check)
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        var window = new MainWindow
        {
            DataContext = model,
            Width = 1400,
            Height = 900,
        };

        window.Show();

        Settle(window);

        check(window, model);

        window.Close();
    }

    private static void Settle(MainWindow window)
    {
        for (var i = 0; i < 6; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    private static ItemsControl? Tabs(MainWindow window)
        => window.GetVisualDescendants()
            .OfType<ItemsControl>()
            .FirstOrDefault(c => c.Name == "ModeTabs");

    /// <summary>
    /// The bounded region the tabs sit on, which is what they align to.
    /// </summary>
    /// <remarks>
    /// **THE TABS ALIGN TO THE BOUNDARY AND NOT TO THE PANELS INSIDE IT** (Tim,
    /// 2026-08-27). This used to look for the CW workspace itself, and once the
    /// boundary gained padding the panels moved thirteen pixels in while the
    /// tabs stayed on the edge they own. The tabs were right and the assertion
    /// was measuring the wrong thing.
    /// </remarks>
    private static Border? OperatingArea(MainWindow window)
        => window.GetVisualDescendants()
            .OfType<Border>()
            .FirstOrDefault(c => c.Name == "WorkspaceBoundary");

    /// <remarks>
    /// Proves the alignment from render bounds rather than by eye: the tab strip
    /// starts at the same x as the area beneath it, because it is inside that
    /// column rather than spanning the row.
    /// </remarks>
    [AvaloniaFact]
    public void TheTabStripBeginsWhereTheOperatingAreaBegins()
    {
        With((window, _) =>
        {
            var tabs = Tabs(window);
            var area = OperatingArea(window);

            Assert.NotNull(tabs);
            Assert.NotNull(area);

            var tabsAt = OnScreen(tabs!, window);
            var areaAt = OnScreen(area!, window);

            _output.WriteLine(
                $"tabs x={tabsAt.X:0} y={tabsAt.Y:0}, "
                + $"boundary x={areaAt.X:0} y={areaAt.Y:0}");

            Assert.True(
                Math.Abs(tabsAt.X - areaAt.X) < 0.5,
                $"the tabs start at x={tabsAt.X:0} and the region they control "
                + $"at x={areaAt.X:0}");

            Assert.True(
                tabsAt.Bottom <= areaAt.Y + 1.5,
                $"the tabs end at y={tabsAt.Bottom:0} and the region starts at "
                + $"y={areaAt.Y:0}, so the strip is not on its edge");
        });
    }

    /// <remarks>
    /// <para>Proves there is no tray, no preset bar and no layout namer anywhere
    /// in the three workspaces (Tim's ruling of 2026-08-27: *"I don't care when
    /// it destroys. We're abandoning all of that."*).</para>
    /// <para>Asserted on the text each of them put on the screen, because that
    /// is what the operator would see if one came back.</para>
    /// </remarks>
    [AvaloniaFact]
    public void ThereIsNoTrayNoPresetBarAndNoLayoutNamer()
    {
        With((window, _) =>
        {
            var said = window.GetVisualDescendants()
                .OfType<TextBlock>()
                .Select(t => t.Text)
                .Where(t => t is not null && (
                    t.Contains("Add to the canvas", StringComparison.Ordinal)
                    || t.Contains("Start from", StringComparison.Ordinal)
                    || t.Contains("Save this layout", StringComparison.Ordinal)))
                .ToList();

            _output.WriteLine(
                said.Count == 0
                    ? "no tray, no preset bar, no layout namer"
                    : string.Join(", ", said));

            Assert.Empty(said);
        });
    }

    /// <remarks>
    /// <para>Proves the header is not re-created when the mode changes. The
    /// assertion is reference identity of the controls themselves — the same
    /// band card, the same neighborhood map, the same rig display — which is the
    /// only thing that distinguishes "still there" from "torn down and rebuilt
    /// identically".</para>
    /// </remarks>
    [AvaloniaFact]
    public void ChangingTheModeDoesNotRecreateTheHeader()
    {
        With((window, model) =>
        {
            Visual? First<T>() where T : Visual
                => window.GetVisualDescendants().OfType<T>().FirstOrDefault();

            var bandBefore = window.GetVisualDescendants().OfType<Button>()
                .FirstOrDefault(b => b.Classes.Contains("hm-band"));

            var mapBefore = First<NeighborhoodMapControl>();
            var rigBefore = First<RigDisplayControl>();

            Assert.NotNull(bandBefore);
            Assert.NotNull(mapBefore);
            Assert.NotNull(rigBefore);

            Assert.Equal("CW", model.OperatingMode);

            model.OperatingMode = "Digital";

            Settle(window);

            var bandAfter = window.GetVisualDescendants().OfType<Button>()
                .FirstOrDefault(b => b.Classes.Contains("hm-band"));

            var mapAfter = First<NeighborhoodMapControl>();
            var rigAfter = First<RigDisplayControl>();

            _output.WriteLine(
                $"band card same: {ReferenceEquals(bandBefore, bandAfter)}, "
                + $"neighborhood same: {ReferenceEquals(mapBefore, mapAfter)}, "
                + $"radio same: {ReferenceEquals(rigBefore, rigAfter)}");

            Assert.Same(bandBefore, bandAfter);
            Assert.Same(mapBefore, mapAfter);
            Assert.Same(rigBefore, rigAfter);

            model.OperatingMode = "CW";

            Settle(window);

            Assert.Same(bandBefore,
                window.GetVisualDescendants().OfType<Button>()
                    .FirstOrDefault(b => b.Classes.Contains("hm-band")));
        });
    }

    /// <remarks>
    /// <para>Proves each of the three tabs changes the workspace below it, which
    /// is what unit 1.11.24 did not deliver: it gave Digital and Voice a line of
    /// text apiece and left the same surface underneath. **A tab that does not
    /// change the screen is not a tab.**</para>
    /// <para>Asserted by reference identity on returning to CW, so "still there"
    /// is told apart from "rebuilt identically".</para>
    /// </remarks>
    [AvaloniaFact]
    public void EachTabChangesTheWorkspace()
    {
        With((window, model) =>
        {
            Grid? Workspace(string name)
                => window.GetVisualDescendants()
                    .OfType<Grid>().FirstOrDefault(g => g.Name == name);

            var cw = Workspace("CwWorkspace");

            Assert.NotNull(cw);
            Assert.True(cw!.IsEffectivelyVisible, "the CW workspace is not showing on CW");
            Assert.False(Workspace("DigitalWorkspace")!.IsEffectivelyVisible);
            Assert.False(Workspace("VoiceWorkspace")!.IsEffectivelyVisible);

            foreach (var mode in new[] { "Digital", "Voice" })
            {
                model.OperatingMode = mode;

                Settle(window);

                _output.WriteLine(
                    $"on {mode}: CW showing {Workspace("CwWorkspace")!.IsEffectivelyVisible}, "
                    + $"{mode} showing {Workspace(mode + "Workspace")!.IsEffectivelyVisible}");

                Assert.False(
                    Workspace("CwWorkspace")!.IsEffectivelyVisible,
                    $"the CW workspace is still showing on {mode}");

                Assert.True(
                    Workspace(mode + "Workspace")!.IsEffectivelyVisible,
                    $"the {mode} workspace is not showing on {mode}");
            }

            model.OperatingMode = "CW";

            Settle(window);

            Assert.Same(cw, Workspace("CwWorkspace"));
            Assert.True(Workspace("CwWorkspace")!.IsEffectivelyVisible);
        });
    }

    /// <remarks>
    /// Proves the three modes the ruling names are the three offered, in order.
    /// </remarks>
    [AvaloniaFact]
    public void TheThreeModesAreCwDigitalAndVoice()
    {
        With((window, model) =>
        {
            _output.WriteLine(string.Join(", ", model.OperatingModes));

            Assert.Equal(new[] { "CW", "Digital", "Voice" }, model.OperatingModes);

            var labels = Tabs(window)!
                .GetVisualDescendants()
                .OfType<RadioButton>()
                .Select(b => b.Content as string)
                .Where(t => t is not null)
                .ToList();

            Assert.Equal(new[] { "CW", "Digital", "Voice" }, labels);
        });
    }
}
