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
/// The tabs begin where the operating area begins, and the header outlives a
/// mode change.
/// </summary>
/// <remarks>
/// <para>**THE DIVIDER IS THE WHOLE POINT AND ONLY A TEST KEEPS IT TRUE** (Tim's
/// ruling of 2026-08-27). The band plan, the neighborhood and the radio are the
/// same in every mode; if a later layout edit slides them inside the tab region
/// they will start being torn down and rebuilt on every switch, and nothing on
/// screen will say so until the operator notices a flicker.</para>
/// <para>Nothing here hit-tests, per unit 1.11.13's rule.</para>
/// </remarks>
public sealed class TheTabsAndTheTrayTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the geometry is printed.</param>
    public TheTabsAndTheTrayTests(ITestOutputHelper output)
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
        var layouts = Hamlet.App.Layout.LayoutStore.Path;

        Hamlet.App.Layout.LayoutStore.Path =
            Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json");

        try
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
        finally
        {
            Hamlet.App.Layout.LayoutStore.Path = layouts;
        }
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

    private static ScrollViewer? OperatingArea(MainWindow window)
        => window.GetVisualDescendants()
            .OfType<ScrollViewer>()
            .FirstOrDefault(c => c.Name == "CanvasView");

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
                + $"operating area x={areaAt.X:0} y={areaAt.Y:0}");

            Assert.True(
                Math.Abs(tabsAt.X - areaAt.X) < 0.5,
                $"the tabs start at x={tabsAt.X:0} and the area they belong to "
                + $"at x={areaAt.X:0}");

            Assert.True(
                tabsAt.Bottom <= areaAt.Y + 0.5,
                $"the tabs end at y={tabsAt.Bottom:0} and the area starts at "
                + $"y={areaAt.Y:0}, so the strip is not above what it switches");
        });
    }

    /// <remarks>
    /// Proves the tray is outside the tab region and to the left of it. The
    /// widgets are shared across modes, so the tray must not move or reload when
    /// the mode changes.
    /// </remarks>
    [AvaloniaFact]
    public void TheTrayIsOutsideTheTabRegionAndToItsLeft()
    {
        With((window, _) =>
        {
            var tabs = Tabs(window);

            Assert.NotNull(tabs);

            var tray = window.GetVisualDescendants()
                .OfType<TextBlock>()
                .FirstOrDefault(t => t.Text == "Add to the canvas");

            Assert.NotNull(tray);

            var trayAt = OnScreen(tray!, window);
            var tabsAt = OnScreen(tabs!, window);

            _output.WriteLine(
                $"tray x={trayAt.X:0}, tabs x={tabsAt.X:0}");

            Assert.True(
                trayAt.X < tabsAt.X,
                $"the tray is at x={trayAt.X:0} and the tabs at x={tabsAt.X:0}");

            // And it is not inside the strip's own subtree, which is the part an
            // x comparison alone would not catch.
            Assert.DoesNotContain(
                tray!, tabs!.GetVisualDescendants().OfType<TextBlock>());
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
