using Avalonia;
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
/// The header says each thing once, and nothing sits between it and the tabs.
/// </summary>
/// <remarks>
/// <para>**TWO STRAY BLOCKS IN ONE PHOTOGRAPH** (Tim, 2026-08-27). The
/// frequency block rendered twice — once inside the neighborhood map where it
/// belongs, and again as a loose card beneath it saying the same thing. And the
/// `recent · places you have been · forget this place` row sat between the
/// header and the tabs, in the one strip that is meant to be the same in every
/// mode.</para>
/// <para>Nothing here hit-tests, and nothing asks a control for its own
/// `IsVisible` where effective visibility is what the operator sees.</para>
/// </remarks>
public sealed class TheHeaderSaysEachThingOnceTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the findings are printed.</param>
    public TheHeaderSaysEachThingOnceTests(ITestOutputHelper output)
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

        for (var i = 0; i < 6; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }

        check(window, model);

        window.Close();
    }

    /// <remarks>
    /// <para>Proves the privilege sentence renders once in the whole window. It
    /// is the one the operator reads to know whether he may transmit, and two
    /// copies of it read as two facts rather than one.</para>
    /// <para>**COUNTED ON SCREEN RATHER THAN IN THE TREE**, because a template
    /// that exists and is not shown is not a duplicate.</para>
    /// </remarks>
    [AvaloniaFact]
    public void ThePrivilegeSentenceRendersOnce()
    {
        With((window, model) =>
        {
            var wanted = model.PrivilegeStatus.Headline;

            Assert.False(
                string.IsNullOrWhiteSpace(wanted),
                "there is no privilege sentence to count");

            var showing = window.GetVisualDescendants()
                .OfType<TextBlock>()
                .Where(t => t.IsEffectivelyVisible && t.Text == wanted)
                .ToList();

            _output.WriteLine(
                $"\"{wanted}\" renders {showing.Count} time(s)");

            Assert.Single(showing);
        });
    }

    /// <remarks>
    /// Proves the recent-places row is not between the header and the tabs. It
    /// is asserted on the words the operator would see, because that is what
    /// would tell him it had come back.
    /// </remarks>
    [AvaloniaFact]
    public void TheRecentPlacesRowIsNotAboveTheTabs()
    {
        With((window, _) =>
        {
            var said = window.GetVisualDescendants()
                .OfType<TextBlock>()
                .Where(t => t.IsEffectivelyVisible)
                .Select(t => t.Text)
                .Where(t => t is not null && (
                    t.Equals("recent", StringComparison.Ordinal)
                    || t.Contains("places you have been", StringComparison.Ordinal)
                    || t.Contains("forget this place", StringComparison.Ordinal)))
                .ToList();

            var buttons = window.GetVisualDescendants()
                .OfType<Button>()
                .Where(b => b.IsEffectivelyVisible
                    && (b.Content as string) == "forget this place")
                .ToList();

            _output.WriteLine(
                said.Count + buttons.Count == 0
                    ? "no recent-places row on the screen"
                    : string.Join(", ", said) + string.Join(", ",
                        buttons.Select(b => b.Content)));

            Assert.Empty(said);
            Assert.Empty(buttons);
        });
    }

    /// <remarks>
    /// <para>Proves nothing renders between the divider and the tab strip. That
    /// gap is where the recent-places row sat, and the point of the divider is
    /// that everything above it is the same in every mode.</para>
    /// <para>Measured as a band: any visible control whose whole rectangle falls
    /// between the two.</para>
    /// </remarks>
    [AvaloniaFact]
    public void NothingRendersBetweenTheDividerAndTheTabs()
    {
        With((window, _) =>
        {
            var strip = window.GetVisualDescendants()
                .OfType<ItemsControl>()
                .First(c => c.Name == "ModeTabs");

            var stripAt = OnScreen(strip, window);

            // The divider is the thin border directly above the strip.
            var divider = window.GetVisualDescendants()
                .OfType<Border>()
                .Where(b => b.IsEffectivelyVisible && b.Bounds.Height <= 2)
                .Select(b => OnScreen(b, window))
                .Where(r => r.Bottom <= stripAt.Y)
                .OrderByDescending(r => r.Bottom)
                .FirstOrDefault();

            _output.WriteLine(
                $"divider bottom={divider.Bottom:0}, strip top={stripAt.Y:0}, "
                + $"gap={stripAt.Y - divider.Bottom:0} px");

            var between = window.GetVisualDescendants()
                .OfType<Control>()
                .Where(c => c.IsEffectivelyVisible
                    && c.Bounds.Width > 0 && c.Bounds.Height > 0)
                .Select(c => (c, r: OnScreen(c, window)))
                .Where(x => x.r.Y >= divider.Bottom && x.r.Bottom <= stripAt.Y)
                .Select(x => x.c.GetType().Name)
                .Distinct()
                .ToList();

            _output.WriteLine(
                between.Count == 0
                    ? "nothing between the divider and the tabs"
                    : string.Join(", ", between));

            Assert.Empty(between);
        });
    }
}
