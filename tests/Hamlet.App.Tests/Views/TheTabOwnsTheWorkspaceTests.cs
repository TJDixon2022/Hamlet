using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// The tab strip and the space below it are one bounded region.
/// </summary>
/// <remarks>
/// <para>**IT HAS TO BE OBVIOUS THAT THE TAB OWNS THE SPACE** (Tim's ruling of
/// 2026-08-27: *"Everything below the CW Digital and Voice is the workspace
/// canvas. That space is bounded by the controlling tab. It needs to be obvious
/// to the user."*). Send and Receive used to float below the strip with no
/// boundary of any kind, so nothing on the screen said which tab they belonged
/// to.</para>
/// <para>Nothing here hit-tests, and nothing asks a control for its own
/// `IsVisible` where effective visibility is what the operator sees.</para>
/// </remarks>
public sealed class TheTabOwnsTheWorkspaceTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the geometry is printed.</param>
    public TheTabOwnsTheWorkspaceTests(ITestOutputHelper output)
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

    private static void With(int width, Action<MainWindow, MainWindowViewModel> check)
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        var window = new MainWindow
        {
            DataContext = model,
            Width = width,
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

    private static Border? Boundary(MainWindow window)
        => window.GetVisualDescendants()
            .OfType<Border>()
            .FirstOrDefault(b => b.Name == "WorkspaceBoundary");

    private static ItemsControl? Strip(MainWindow window)
        => window.GetVisualDescendants()
            .OfType<ItemsControl>()
            .FirstOrDefault(c => c.Name == "ModeTabs");

    private static List<RadioButton> Tabs(MainWindow window)
        => window.GetVisualDescendants()
            .OfType<RadioButton>()
            .Where(b => b.Classes.Contains("hm-tab"))
            .ToList();

    private static void Press(MainWindow window, string name)
    {
        Tabs(window).First(t => (string?)t.Content == name).IsChecked = true;

        Settle(window);
    }

    /// <remarks>
    /// Proves the boundary meets the strip and encloses the workspace: its top
    /// edge at the strip's bottom, and its sides reaching past the panels inside
    /// it.
    /// </remarks>
    /// <param name="width">How wide the window is.</param>
    [AvaloniaTheory]
    [InlineData(1200)]
    [InlineData(1400)]
    public void TheBoundaryMeetsTheStripAndEnclosesTheWorkspace(int width)
    {
        With(width, (window, _) =>
        {
            var edge = Boundary(window);
            var strip = Strip(window);

            Assert.NotNull(edge);
            Assert.NotNull(strip);

            var edgeAt = OnScreen(edge!, window);
            var stripAt = OnScreen(strip!, window);

            var send = window.GetVisualDescendants()
                .OfType<Border>().First(b => b.Name == "SendPanel");

            var receive = window.GetVisualDescendants()
                .OfType<Border>().First(b => b.Name == "ReceivePanel");

            var sendAt = OnScreen(send, window);
            var receiveAt = OnScreen(receive, window);

            _output.WriteLine(
                $"{width} px: strip bottom={stripAt.Bottom:0}, "
                + $"boundary top={edgeAt.Y:0} left={edgeAt.X:0} "
                + $"right={edgeAt.Right:0}");

            // The strip sits on the boundary's top edge, within a pixel.
            Assert.True(
                Math.Abs(stripAt.Bottom - edgeAt.Y) <= 1.5,
                $"the strip ends at {stripAt.Bottom:0} and the boundary starts "
                + $"at {edgeAt.Y:0}");

            Assert.True(
                edgeAt.X <= sendAt.X && edgeAt.Right >= receiveAt.Right,
                $"the boundary runs {edgeAt.X:0} to {edgeAt.Right:0} and the "
                + $"panels run {sendAt.X:0} to {receiveAt.Right:0}");

            Assert.True(
                edgeAt.Bottom >= receiveAt.Bottom,
                $"the boundary ends at {edgeAt.Bottom:0} and Receive at "
                + $"{receiveAt.Bottom:0}");
        });
    }

    /// <remarks>
    /// <para>Proves the selected tab merges into the boundary rather than
    /// sitting above it: same fill, same edge colour, and no border along the
    /// bottom where the two meet.</para>
    /// <para>**AND THE UNSELECTED ONES DO NOT**, which is the half that makes the
    /// selected one mean something.</para>
    /// </remarks>
    [AvaloniaFact]
    public void TheSelectedTabMergesIntoTheBoundary()
    {
        With(1400, (window, _) =>
        {
            var edge = Boundary(window);

            Assert.NotNull(edge);

            foreach (var tab in Tabs(window))
            {
                var chosen = tab.IsChecked == true;

                _output.WriteLine(
                    $"{tab.Content,-8} checked={chosen} "
                    + $"fill={(tab.Background as ISolidColorBrush)?.Color} "
                    + $"border={tab.BorderThickness}");

                if (chosen)
                {
                    Assert.Equal(
                        (edge!.Background as ISolidColorBrush)?.Color,
                        (tab.Background as ISolidColorBrush)?.Color);

                    Assert.Equal(
                        (edge.BorderBrush as ISolidColorBrush)?.Color,
                        (tab.BorderBrush as ISolidColorBrush)?.Color);

                    Assert.Equal(0, tab.BorderThickness.Bottom);
                }
                else
                {
                    Assert.True(
                        tab.BorderThickness.Bottom > 0,
                        $"the unselected {tab.Content} has no bottom edge, so it "
                        + "reads as merged into a space it does not control");
                }
            }
        });
    }

    /// <remarks>
    /// Proves the boundary is the same region on all three tabs — the thing that
    /// makes it read as a space rather than as three separate panels. Asserted by
    /// reference identity and by its rectangle.
    /// </remarks>
    [AvaloniaFact]
    public void TheBoundaryIsTheSameRegionOnEveryTab()
    {
        With(1400, (window, _) =>
        {
            var first = Boundary(window);
            var where = OnScreen(first!, window);

            foreach (var name in new[] { "Digital", "Voice", "CW" })
            {
                Press(window, name);

                var now = Boundary(window);
                var here = OnScreen(now!, window);

                _output.WriteLine($"on {name,-8} boundary {here}");

                Assert.Same(first, now);

                Assert.True(
                    now!.IsEffectivelyVisible,
                    $"the boundary is not on the screen on {name}");

                Assert.True(
                    Math.Abs(here.X - where.X) < 0.5
                    && Math.Abs(here.Width - where.Width) < 0.5,
                    $"the boundary moved on {name}: was {where}, now {here}");
            }
        });
    }
}
