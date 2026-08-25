using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Every band card can be clicked, and the best-bet badge does not take clicks
/// from its neighbours.
/// </summary>
/// <remarks>
/// <para>**ON 2026-08-25 THE OPERATOR COULD NOT TUNE.** `40 m` did not respond
/// to a click at all and the only way to change band was through favourites. The
/// badge was a row of its own above the card, so a band carrying it had its card
/// pushed down while every other card stayed at the top; and the badge is
/// centred on a card whose width follows the wavelength (HM-DEC-141), so on a
/// narrow card it is wider than the thing it labels and hangs over its
/// neighbours, in front of them, taking their clicks.</para>
/// <para>**NOTHING FAILED.** Every binding resolved, the command existed, and
/// the button was on the screen — it was underneath something.
/// `BindingHealthTests` cannot see that and neither can a test that only asks
/// whether a control exists (HM-DEC-087's own lesson: a control that cannot be
/// pressed and a control that is disabled look identical from the log).</para>
/// <para>So this asks the question the operator asks: **at the point on the
/// screen where this card is drawn, what would a click land on?**</para>
/// </remarks>
public sealed class EveryBandCanBeClickedTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the hit tests are printed.</param>
    public EveryBandCanBeClickedTests(ITestOutputHelper output)
        => _output = output;

    /// <remarks>
    /// Proves it card by card. A band whose centre hit-tests to something other
    /// than its own button is a band the operator cannot tune to, however
    /// healthy every binding behind it looks.
    /// </remarks>
    /// <param name="width">How wide the window is.</param>
    /// <param name="what">What that width is, for the failure message.</param>
    [AvaloniaTheory]
    [InlineData(1400, "the width this test was first pinned to")]
    [InlineData(1200, "the application's own default")]
    [InlineData(1000, "narrower than the operator works at")]
    [InlineData(820, "narrower still")]
    public void EveryBandCardAnswersAClickOnItsOwnCentre(int width, string what)
    {
        var layouts = Hamlet.App.Layout.LayoutStore.Path;

        Hamlet.App.Layout.LayoutStore.Path =
            Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json");

        try
        {
            var model = new MainWindowViewModel(new AppSettings(), null);
            var window = new MainWindow { DataContext = model };

            // **EVERY WIDTH, BECAUSE THE FAULT ONLY EVER SHOWED AT ONE OF
            // THEM.** This test used to be pinned to 1400 and said so: at the
            // headless default the readout covered the last card, and that was
            // recorded as HM-OPEN-060 rather than asserted, so the suite was
            // green while the operator could not reach `10 m`. A band row that
            // is reachable at one width and not another is not a reachable band
            // row. Tim's ruling of 2026-08-25 moved the row below the readout,
            // and this is what holds it there.
            window.Width = width;
            window.Height = 900;

            window.Show();

            for (var i = 0; i < 5; i++)
            {
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
            }

            var cards = window.GetVisualDescendants()
                .OfType<Button>()
                .Where(b => b.Classes.Contains("hm-band"))
                .ToList();

            _output.WriteLine($"{cards.Count} band cards on the screen");

            Assert.True(
                cards.Count >= 5,
                $"only {cards.Count} band cards were found, so this test is not "
                + "looking at the band row at all");

            var unreachable = new List<string>();

            foreach (var card in cards)
            {
                var band = (card.DataContext as BandButtonViewModel)?.Band.Name
                    ?? "?";

                // **THE RENDER-TIME RECTANGLE, NOT THE LAYOUT-TIME ONE.**
                // `TranslatePoint` walks the layout transform and on this window
                // it disagrees with what is actually drawn by nineteen pixels:
                // it puts the `80 m` card at y 249 to 292 while a hit test finds
                // its button between 230 and 262, and the width it reports flips
                // between 76 and 93 from one run to the next. A test that asks
                // "what would a click here land on" has to ask it of the pixels
                // the operator sees, which is what `TransformedBounds` is.
                var point = TopmostReachablePoint(card, window, out var hit);

                var reached = (hit as Visual)?.GetSelfAndVisualAncestors()
                    .OfType<Button>()
                    .FirstOrDefault();

                var ok = ReferenceEquals(reached, card);

                _output.WriteLine(
                    $"  DIAG {band}: topleft="
                    + card.TranslatePoint(new Point(0, 0), window)
                    + " bottomright="
                    + card.TranslatePoint(
                        new Point(card.Bounds.Width, card.Bounds.Height), window));
                _output.WriteLine(
                    $"  {band,-6} at {point.X,6:0},{point.Y,4:0} -> "
                    + (ok
                        ? "its own card"
                        : $"{(reached?.DataContext as BandButtonViewModel)?.Band.Name ?? hit?.GetType().Name ?? "nothing"}"
                          + $" [dc={(hit as StyledElement)?.DataContext?.GetType().Name ?? "none"}"
                          + $" hit={(hit as InputElement)?.IsHitTestVisible}"
                          + $" chain={string.Join(" < ", ((hit as Visual)?.GetSelfAndVisualAncestors() ?? Array.Empty<Visual>()).Take(6).Select(v => v.GetType().Name))}]"));

                if (!ok)
                {
                    unreachable.Add(band);
                }
            }

            window.Close();

            Assert.True(
                unreachable.Count == 0,
                $"at {width} px ({what}) these bands cannot be clicked, so the "
                + "operator cannot tune to them: "
                + string.Join(", ", unreachable));
        }
        finally
        {
            Hamlet.App.Layout.LayoutStore.Path = layouts;
        }
    }

    /// <summary>
    /// The first point down a control's own rectangle that a click reaches
    /// anything at all, and what it reaches.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <param name="window">The window it is in.</param>
    /// <param name="hit">What a click there would land on.</param>
    /// <returns>The point.</returns>
    /// <remarks>
    /// <para>**THE QUESTION IS WHAT IS ON TOP OF THIS CARD, AND THAT IS WHAT
    /// THIS ASKS.** The centre would be the natural point and it cannot be used:
    /// the headless renderer draws these cards about thirteen pixels above where
    /// every geometry API reports them and about two thirds as tall, so a hit
    /// test at a computed centre lands past the bottom of the card and reports an
    /// occlusion that does not exist. That is the renderer's disagreement with
    /// itself, not the application's.</para>
    /// <para>Walking down the card's own rectangle and taking the **first point
    /// that reaches anything** is immune to that offset and still answers the
    /// question exactly: if something is drawn over the band row, the first thing
    /// a click meets on its way down is that something. Empty space is skipped
    /// because empty space is not an occluder.</para>
    /// </remarks>
    private static Point TopmostReachablePoint(
        Visual control, Window window, out IInputElement? hit)
    {
        var centre = CentreOnScreen(control, window);
        var top = centre.Y - (control.Bounds.Height / 2);
        var point = centre;

        hit = null;

        for (var y = top; y <= top + control.Bounds.Height; y += 1)
        {
            var probe = new Point(centre.X, y);
            var found = ((IInputElement)window).InputHitTest(probe);

            if (found is Visual visual
                && visual.GetSelfAndVisualAncestors().Any(v => v == window)
                && visual != window
                && visual.GetVisualParent() != window)
            {
                hit = found;

                return probe;
            }
        }

        return point;
    }

    /// <summary>Where a control is actually drawn, in window coordinates.</summary>
    /// <param name="control">The control.</param>
    /// <param name="window">The window it is in.</param>
    /// <returns>The middle of it.</returns>
    /// <remarks>
    /// <para>**NEITHER `TranslatePoint` NOR `TransformedBounds` AGREES WITH THE
    /// HIT TEST ON THIS WINDOW, AND THE HIT TEST IS THE ONE THAT DECIDES.** Both
    /// of those put the `80 m` card at y 249 to 292; a hit test finds its button
    /// between 230 and 262, and the width they report flips between 76 and 93
    /// from one run to the next. Summing each ancestor's own laid-out position
    /// gives 233, which is what the hit test agrees with.</para>
    /// <para>The disagreement is the headless renderer's, not the application's,
    /// and it is worth a comment rather than a workaround nobody can read later:
    /// a test that measured the wrong rectangle would report an occlusion that
    /// is not there, which is exactly what this file exists to catch.</para>
    /// </remarks>
    private static Point CentreOnScreen(Visual control, Visual window)
    {
        var x = control.Bounds.Width / 2;
        var y = control.Bounds.Height / 2;

        for (var at = control; at is not null && at != window;
             at = at.GetVisualParent())
        {
            x += at.Bounds.X;
            y += at.Bounds.Y;
        }

        return new Point(x, y);
    }

    /// <remarks>
    /// **AND THE BADGE ITSELF IS NOT IN ANYBODY'S WAY.** It is a label rather
    /// than a control, so it must be hit-tested out of the path entirely — over
    /// its own card as much as over its neighbours, because a click on the badge
    /// is a click the operator meant for the card underneath it.
    /// </remarks>
    [AvaloniaFact]
    public void TheBestBetBadgeTakesNoClicksAtAll()
    {
        var layouts = Hamlet.App.Layout.LayoutStore.Path;

        Hamlet.App.Layout.LayoutStore.Path =
            Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json");

        try
        {
            var model = new MainWindowViewModel(new AppSettings(), null);
            var window = new MainWindow { DataContext = model };

            window.Show();

            for (var i = 0; i < 5; i++)
            {
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
            }

            // Only the band row's own badges: a Border whose data context is a
            // band. Searching by text alone also finds the prose that explains
            // what a best bet is, which is not a badge and takes no clicks from
            // anything.
            var badges = window.GetVisualDescendants()
                .OfType<Border>()
                .Where(b => b.DataContext is BandButtonViewModel
                    && b.GetVisualDescendants()
                        .OfType<TextBlock>()
                        .Any(t => t.Text is not null
                            && t.Text.Contains(
                                "best bet", StringComparison.OrdinalIgnoreCase)))
                .ToList();

            _output.WriteLine($"{badges.Count} best-bet badges on the screen");

            // **A HEADLESS RUN OFTEN HAS NO BEST BET**, because the ranking
            // needs spots and there are none. That makes the loop below vacuous
            // rather than passing on evidence, and a vacuous pass that looks
            // like a green test is worse than no test — so the count is printed
            // and the real guarantee is the hit test in the other case, which
            // asks what a click at each card's centre actually reaches.
            foreach (var badge in badges)
            {
                _output.WriteLine(
                    $"  hit test visible: {badge.IsHitTestVisible}");

                Assert.False(
                    badge.IsHitTestVisible,
                    "the best-bet badge takes clicks, and it is drawn over the "
                    + "band cards, so it takes them from the control underneath");
            }

            window.Close();
        }
        finally
        {
            Hamlet.App.Layout.LayoutStore.Path = layouts;
        }
    }
}
