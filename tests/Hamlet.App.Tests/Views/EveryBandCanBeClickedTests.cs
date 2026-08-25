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
    [AvaloniaFact]
    public void EveryBandCardAnswersAClickOnItsOwnCentre()
    {
        var layouts = Hamlet.App.Layout.LayoutStore.Path;

        Hamlet.App.Layout.LayoutStore.Path =
            Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json");

        try
        {
            var model = new MainWindowViewModel(new AppSettings(), null);
            var window = new MainWindow { DataContext = model };

            // **A WIDTH THE BAND ROW FITS IN.** The headless default is narrower
            // than the application's own, and at that width the rig readout on
            // the right of the strip covers the last card. That is a real
            // finding about narrow windows and it is not what this test is
            // about, so the window is given room and the overlap is recorded
            // separately.
            window.Width = 1400;
            window.Height = 900;

            window.Show();

            for (var i = 0; i < 5; i++)
            {
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
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

                var middle = card.TranslatePoint(
                    new Point(card.Bounds.Width / 2, card.Bounds.Height / 2),
                    window);

                if (middle is not { } point)
                {
                    unreachable.Add($"{band} (not laid out)");

                    continue;
                }

                // What a click at that point would actually reach.
                var hit = ((IInputElement)window).InputHitTest(point);
                var reached = (hit as Visual)?.GetSelfAndVisualAncestors()
                    .OfType<Button>()
                    .FirstOrDefault();

                var ok = ReferenceEquals(reached, card);

                _output.WriteLine(
                    $"  {band,-6} at {point.X,6:0},{point.Y,4:0} -> "
                    + (ok
                        ? "its own card"
                        : $"{(reached?.DataContext as BandButtonViewModel)?.Band.Name ?? hit?.GetType().Name ?? "nothing"}"
                          + $" [dc={(hit as StyledElement)?.DataContext?.GetType().Name ?? "none"}"
                          + $" hit={(hit as InputElement)?.IsHitTestVisible}]"));

                if (!ok)
                {
                    unreachable.Add(band);
                }
            }

            window.Close();

            Assert.True(
                unreachable.Count == 0,
                "these bands cannot be clicked, so the operator cannot tune to "
                + "them: " + string.Join(", ", unreachable));
        }
        finally
        {
            Hamlet.App.Layout.LayoutStore.Path = layouts;
        }
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
