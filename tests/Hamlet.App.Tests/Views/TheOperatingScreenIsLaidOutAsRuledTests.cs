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
/// The operating screen is stacked the way Tim ruled it on 2026-08-27.
/// </summary>
/// <remarks>
/// <para>**NOTHING HERE HIT-TESTS**, per unit 1.11.13's rule. An unexplained
/// headless-versus-real geometry offset of about thirteen pixels once let three
/// faults the operator could plainly see sit behind a green hit test. What is
/// asserted is the geometry that causes a fault — visual-tree order, render
/// bounds, clipping ancestors — never that a point reaches a control.</para>
/// <para>The order, top to bottom: the band plan full width; the neighborhood
/// and the radio on one row beneath it; a divider; and everything belonging to
/// the mode below that.</para>
/// </remarks>
public sealed class TheOperatingScreenIsLaidOutAsRuledTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the geometry is printed.</param>
    public TheOperatingScreenIsLaidOutAsRuledTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>Where a control is drawn, by summing its own laid-out chain.</summary>
    /// <param name="visual">The control.</param>
    /// <param name="root">What to measure it against.</param>
    /// <returns>Its rectangle in the root's coordinates.</returns>
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

    private static void With(int width, Action<MainWindow, List<Button>> check)
    {
        var layouts = Hamlet.App.Layout.LayoutStore.Path;

        Hamlet.App.Layout.LayoutStore.Path =
            Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json");

        try
        {
            var window = new MainWindow
            {
                DataContext = new MainWindowViewModel(new AppSettings(), null),
                Width = width,
                Height = 900,
            };

            window.Show();

            for (var i = 0; i < 6; i++)
            {
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
            }

            var cards = window.GetVisualDescendants()
                .OfType<Button>()
                .Where(b => b.Classes.Contains("hm-band"))
                .ToList();

            Assert.True(
                cards.Count >= 7,
                $"only {cards.Count} band cards were found, so this test is not "
                + "looking at the band row at all");

            check(window, cards);

            window.Close();
        }
        finally
        {
            Hamlet.App.Layout.LayoutStore.Path = layouts;
        }
    }

    private static Visual? Neighborhood(MainWindow window)
        => window.GetVisualDescendants()
            .OfType<NeighborhoodMapControl>()
            .FirstOrDefault();

    private static Visual? Radio(MainWindow window)
        => window.GetVisualDescendants()
            .OfType<RigDisplayControl>()
            .FirstOrDefault();

    /// <remarks>
    /// Proves the ruling's own order: the band plan, then the row holding the
    /// neighborhood and the radio. The bands are the essential driver for a
    /// session and go first.
    /// </remarks>
    /// <param name="width">How wide the window is.</param>
    [AvaloniaTheory]
    [InlineData(1200)]
    [InlineData(1400)]
    public void BandPlanThenTheNeighborhoodAndTheRadio(int width)
    {
        With(width, (window, cards) =>
        {
            var bands = OnScreen(cards[0], window);
            var map = Neighborhood(window);
            var rig = Radio(window);

            Assert.NotNull(map);
            Assert.NotNull(rig);

            var mapAt = OnScreen(map!, window);
            var rigAt = OnScreen(rig!, window);

            _output.WriteLine(
                $"{width} px: bands y={bands.Y:0}, neighborhood y={mapAt.Y:0} "
                + $"x={mapAt.X:0} w={mapAt.Width:0}, radio y={rigAt.Y:0} "
                + $"x={rigAt.X:0} w={rigAt.Width:0}");

            Assert.True(
                bands.Y < mapAt.Y,
                $"the bands are at {bands.Y:0} and the neighborhood at {mapAt.Y:0}");

            Assert.True(
                bands.Y < rigAt.Y,
                $"the bands are at {bands.Y:0} and the radio at {rigAt.Y:0}");

            // **NEIGHBORHOOD LEFT, RADIO RIGHT**, which is the half of the
            // ruling an ordering assertion alone would miss.
            Assert.True(
                mapAt.X < rigAt.X,
                $"the neighborhood is at x={mapAt.X:0} and the radio at "
                + $"x={rigAt.X:0}, so they are the wrong way round");
        });
    }

    /// <remarks>
    /// Proves every band label renders inside its own card, `10 m` included.
    /// The operator read `10 n` off his own screen on 2026-08-26 because the
    /// label was given less width than it asked for and cut mid-glyph.
    /// </remarks>
    /// <param name="width">How wide the window is.</param>
    [AvaloniaTheory]
    [InlineData(1200)]
    [InlineData(1400)]
    public void EveryBandLabelRendersInsideItsOwnCard(int width)
    {
        With(width, (window, cards) =>
        {
            var cut = new List<string>();

            foreach (var card in cards)
            {
                var band = (card.DataContext as BandButtonViewModel)?.Band.Name
                    ?? "?";

                var label = card.GetVisualDescendants().OfType<TextBlock>()
                    .FirstOrDefault(t => t.Text == band);

                if (label is null)
                {
                    continue;
                }

                label.Measure(Size.Infinity);

                var wanted = label.DesiredSize.Width;
                var got = label.Bounds.Width;

                _output.WriteLine($"  {band,-6} wants {wanted:0}, has {got:0}");

                if (got + 0.5 < wanted)
                {
                    cut.Add($"{band} (wants {wanted:0}, has {got:0})");
                }
            }

            Assert.True(
                cut.Count == 0,
                $"at {width} px these band names are cut short: "
                + string.Join(", ", cut));
        });
    }

    /// <remarks>
    /// <para>Proves the best-bet badge is not clipped. It is drawn over its card
    /// with a negative top margin so it costs the row no line of its own, and a
    /// host whose bounds are exactly one card tall cuts everything above that
    /// edge away, which is what the operator saw as a sliver.</para>
    /// <para>**THE CLIPPING ANCESTOR IS NAMED IN THE OUTPUT**, because "nothing
    /// clips it" is only checkable if the thing that would have is
    /// identified.</para>
    /// </remarks>
    /// <param name="width">How wide the window is.</param>
    [AvaloniaTheory]
    [InlineData(1200)]
    [InlineData(1400)]
    public void NothingBetweenTheCardsAndTheWindowClipsTheBadge(int width)
    {
        With(width, (window, cards) =>
        {
            var clipper = cards[0].GetVisualAncestors()
                .FirstOrDefault(a => a.ClipToBounds);

            var card = OnScreen(cards[0], window);

            _output.WriteLine(
                $"{width} px: first clipping ancestor above a band card is "
                + $"{clipper?.GetType().Name ?? "none"}; card at {card}");

            Assert.True(
                clipper is null || ReferenceEquals(clipper, window),
                $"{clipper?.GetType().Name} clips the row the badge hangs above");
        });
    }

    /// <remarks>
    /// Proves the whole band row is inside the window, which is the other half
    /// of what "cut off" could have meant.
    /// </remarks>
    /// <param name="width">How wide the window is.</param>
    [AvaloniaTheory]
    [InlineData(1200)]
    [InlineData(1400)]
    public void TheWholeBandRowIsInsideTheWindow(int width)
    {
        With(width, (window, cards) =>
        {
            var last = OnScreen(cards[^1], window);

            _output.WriteLine(
                $"{width} px: the row ends at {last.Right:0} of "
                + $"{window.Bounds.Width:0}");

            Assert.True(
                last.Right <= window.Bounds.Width,
                $"the row ends at {last.Right:0} and the window is "
                + $"{window.Bounds.Width:0} wide");
        });
    }

    /// <remarks>
    /// <para>Proves the fault the operator photographed: Receive rendering
    /// `wh at the rad io is he ari ng` one or two letters to a line, because it
    /// was squeezed beside the whole previous canvas.</para>
    /// <para>**THE ASSERTION IS A WIDTH IN CHARACTERS**, not a pixel count. A
    /// column is wide enough or it is not, and the thing that went wrong is that
    /// text had nowhere to go — so what is measured is how many characters of the
    /// terminal's own font fit across the panel that holds it.</para>
    /// </remarks>
    /// <param name="width">How wide the window is.</param>
    [AvaloniaTheory]
    [InlineData(1200)]
    [InlineData(1400)]
    public void ReceiveIsWideEnoughToReadALineOfMorse(int width)
    {
        With(width, (window, _) =>
        {
            var receive = window.GetVisualDescendants()
                .OfType<Border>()
                .FirstOrDefault(b => b.Name == "ReceivePanel");

            Assert.NotNull(receive);

            var terminal = receive!.GetVisualDescendants()
                .OfType<CwTerminalControl>()
                .FirstOrDefault();

            Assert.NotNull(terminal);

            // One character of the terminal's own monospace face, measured
            // rather than assumed.
            var probe = new TextBlock
            {
                Text = new string('M', 10),
                FontFamily = terminal!.FontFamily,
                FontSize = terminal.FontSize,
            };

            probe.Measure(Size.Infinity);

            var perCharacter = probe.DesiredSize.Width / 10;
            var fits = terminal.Bounds.Width / Math.Max(perCharacter, 0.01);

            _output.WriteLine(
                $"{width} px: receive w={receive.Bounds.Width:0}, "
                + $"terminal w={terminal.Bounds.Width:0}, "
                + $"{perCharacter:0.0} px a character, **{fits:0} characters to a line**");

            Assert.True(
                fits >= 40,
                $"at {width} px only {fits:0} characters fit across the decoded "
                + "text, and the photographed failure was one or two");
        });
    }

    /// <remarks>
    /// Proves the operating area holds two panels and no widgets. Everything
    /// else is in the tray on the far left (Tim, 2026-08-27).
    /// </remarks>
    [AvaloniaFact]
    public void TheOperatingAreaHoldsTwoPanelsAndNoWidgets()
    {
        With(1400, (window, _) =>
        {
            var model = (MainWindowViewModel)window.DataContext!;

            _output.WriteLine(
                $"{model.Canvas.Placed.Count} widgets out, "
                + $"{model.Canvas.Tray.Count} in the tray");

            Assert.Empty(model.Canvas.Placed);

            var receive = window.GetVisualDescendants()
                .OfType<Border>().FirstOrDefault(b => b.Name == "ReceivePanel");

            var send = window.GetVisualDescendants()
                .OfType<Border>().FirstOrDefault(b => b.Name == "SendPanel");

            Assert.NotNull(receive);
            Assert.NotNull(send);

            // And every widget is still offered, so nothing was deleted.
            Assert.NotEmpty(model.Canvas.Tray);
        });
    }

    /// <remarks>
    /// Proves the neighborhood and the radio do not overlap. They share a row
    /// and the neighborhood is the wider of the two, so the failure this guards
    /// against is one being drawn over the other.
    /// </remarks>
    /// <param name="width">How wide the window is.</param>
    [AvaloniaTheory]
    [InlineData(1200)]
    [InlineData(1400)]
    public void TheNeighborhoodAndTheRadioDoNotOverlap(int width)
    {
        With(width, (window, _) =>
        {
            var map = Neighborhood(window);
            var rig = Radio(window);

            Assert.NotNull(map);
            Assert.NotNull(rig);

            var mapAt = OnScreen(map!, window);
            var rigAt = OnScreen(rig!, window);

            _output.WriteLine(
                $"{width} px: neighborhood right edge {mapAt.Right:0}, "
                + $"radio left edge {rigAt.X:0}");

            Assert.True(
                mapAt.Right <= rigAt.X + 0.5,
                $"the neighborhood reaches {mapAt.Right:0} and the radio starts "
                + $"at {rigAt.X:0}, so one is drawn over the other");
        });
    }
}
