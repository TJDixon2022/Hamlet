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
/// The band row sits where Tim ruled it sits, every card shows its whole label,
/// and nothing clips the badge that hangs above them.
/// </summary>
/// <remarks>
/// <para>**THIS AREA WAS VERIFIED GREEN BY HIT-TESTING AND THE REAL WINDOW SHOWED
/// THE FAULTS ANYWAY.** Unit 1.11.9 asserted every card answered a click while
/// recording an unexplained disagreement between the headless geometry and what
/// is drawn. On 2026-08-26 the operator read `10 n` off his screen where `10 m`
/// was written, saw the best-bet badge reduced to a sliver, and found the row
/// itself two hundred pixels down the window.</para>
/// <para>**SO NOTHING HERE HIT-TESTS.** Each of the three faults is asserted as
/// the geometry that causes it: where the row sits relative to the two things
/// ruled above it, whether a label is given the width it asked for, and whether
/// anything between the cards and the window clips what deliberately overhangs
/// them. A hit test says a point reaches a control, and none of these faults is
/// about that.</para>
/// </remarks>
public sealed class TheBandRowIsWhereItWasRuledTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the geometry is printed.</param>
    public TheBandRowIsWhereItWasRuledTests(ITestOutputHelper output)
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
            var model = new MainWindowViewModel(new AppSettings(), null);
            var window = new MainWindow { DataContext = model };

            window.Width = width;
            window.Height = 900;
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

    /// <remarks>
    /// Proves Tim's ruling of 2026-08-26: from the top it is the Hamlet title,
    /// the line of Shakespeare, then the bands. The row had drifted below the rig
    /// readout, about two hundred pixels down, so the first control anybody
    /// reaches for was the last thing they found.
    /// </remarks>
    /// <param name="width">How wide the window is.</param>
    [AvaloniaTheory]
    [InlineData(1200)]
    [InlineData(1400)]
    public void TitleThenTheLineThenTheBands(int width)
    {
        With(width, (window, cards) =>
        {
            var title = window.GetVisualDescendants().OfType<TextBlock>()
                .First(t => t.Text == "Ham");

            var line = window.GetVisualDescendants().OfType<TextBlock>()
                .First(t => t.FontStyle == FontStyle.Italic
                    && t.FontSize is > 12 and < 13);

            var titleAt = OnScreen(title, window);
            var lineAt = OnScreen(line, window);
            var rowAt = OnScreen(cards[0], window);

            _output.WriteLine(
                $"{width} px: title y={titleAt.Y:0}, line y={lineAt.Y:0}, "
                + $"bands y={rowAt.Y:0}");

            Assert.True(
                titleAt.Y < lineAt.Y,
                $"the title is at {titleAt.Y:0} and the line at {lineAt.Y:0}");

            Assert.True(
                lineAt.Y < rowAt.Y,
                $"the line is at {lineAt.Y:0} and the band row at {rowAt.Y:0}");

            // **AND DIRECTLY UNDER IT, NOT MERELY SOMEWHERE BELOW.** The fault
            // being fixed is that the row sat two hundred pixels down with the
            // rig readout in between; an ordering assertion alone would have
            // passed throughout that.
            Assert.True(
                rowAt.Y - lineAt.Y < 80,
                $"the band row is {rowAt.Y - lineAt.Y:0} pixels below the line, "
                + "so something has got in between them again");
        });
    }

    /// <remarks>
    /// <para>Proves the fault the operator read off his own screen: `10 n` where
    /// `10 m` was written. The narrowest card was fifty-eight pixels wide and its
    /// label wants forty, while the padding and the day-night icon beside it take
    /// thirty-one, so the label was given twenty-seven and cut mid-glyph.
    /// `15 m`, `17 m` and `20 m` were short by six, four and one.</para>
    /// <para>**IT ASKS THE LABEL WHAT IT WANTED**, which is the only form of this
    /// question that does not depend on a font measurement written down here.</para>
    /// </remarks>
    /// <param name="width">How wide the window is.</param>
    [AvaloniaTheory]
    [InlineData(1200)]
    [InlineData(1400)]
    public void EveryBandShowsItsWholeName(int width)
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

                _output.WriteLine(
                    $"  {band,-6} label wants {wanted:0} and has {got:0}");

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
    /// Proves every card is drawn inside the window rather than off its right
    /// edge, which is the other half of what "cut on its right" could have meant.
    /// </remarks>
    /// <param name="width">How wide the window is.</param>
    [AvaloniaTheory]
    [InlineData(1200)]
    [InlineData(1400)]
    public void TheWholeRowIsInsideTheWindow(int width)
    {
        With(width, (window, cards) =>
        {
            var last = OnScreen(cards[^1], window);

            _output.WriteLine(
                $"{width} px: the row ends at {last.Right:0} of "
                + $"{window.Bounds.Width:0}");

            Assert.True(
                last.Right <= window.Bounds.Width,
                $"the row ends at {last.Right:0} and the window is only "
                + $"{window.Bounds.Width:0} wide");
        });
    }

    /// <remarks>
    /// <para>Proves the badge fault at its cause. The best-bet badge is drawn over
    /// its card with a negative top margin, so it deliberately reaches above the
    /// card's top edge; the control hosting the cards had bounds exactly one card
    /// tall and clipped to them, so everything above that edge was cut away and
    /// the operator saw the badge's bottom sliver.</para>
    /// <para>**MEASURED, AND THE ANCESTOR IS NAMED**: before the fix the first
    /// clipping ancestor above a card was the band row's own `ItemsControl`,
    /// bounds forty-three pixels tall, with the badge nine pixels above its top.
    /// After it, the first clipping ancestor is the window itself.</para>
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
            var clip = clipper is null ? default : OnScreen(clipper, window);

            _output.WriteLine(
                $"{width} px: first clipping ancestor is "
                + $"{clipper?.GetType().Name ?? "none"} at {clip}, card at {card}");

            Assert.True(
                clipper is null || ReferenceEquals(clipper, window),
                $"the badge hangs above the cards and {clipper?.GetType().Name} "
                + $"clips at {clip.Y:0} while the card starts at {card.Y:0}");
        });
    }
}
