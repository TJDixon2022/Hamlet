using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 313 task 3: **the For You panel scrolls.**
/// </summary>
/// <remarks>
/// <para>**R10, Tim 2026-09-11**: *"fix for you scrolling"*, and the unbuilt half of
/// unit 310's R6, which ruled conversation cards unlimited with a vertical scroll.
/// **This is the more important of the unit's two faults: a card that cannot be reached
/// is a contact that cannot be logged.**</para>
/// <para>**MEASURED BEFORE** (`Unit313PanelTraceTests`): with six conversations the cards
/// control laid out **1,748 px inside a panel 264 px tall**, five of the six started
/// below the bottom edge, and the nearest `ScrollViewer` above the cards was **none** -
/// while the decoded list to its left and the conversation rows beneath it both had
/// one.</para>
/// <para>**AND THE HORIZONTAL TRUNCATION WAS NOT THE APP.** The card measured 301 px
/// wide inside a 331 px panel. What is cut off on the right of the screenshot is the
/// screenshot's own edge at 1,207 px, which also cuts the rig display at the top, and
/// that is not in this panel at all.</para>
/// <para>**COMPUTED, NOT SEEN.** A headless window lays out and does not paint. What is
/// asserted is the container, the extent, the offset and where the layout put each card.
/// Nothing here can say whether a scrollbar is visible to a person.</para>
/// </remarks>
public sealed class ThePanelScrollsTests
{
    private const string HisCall = "KC3QIS";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the layout is printed.</param>
    public ThePanelScrollsTests(ITestOutputHelper output) => _output = output;

    /// <summary>**With more cards than fit, every card is reachable.**</summary>
    [AvaloniaFact]
    public void WithMoreCardsThanFitEveryCardIsReachable()
    {
        var model = Model();

        Answers(model, "K9XP", "W1ABC", "VE3XN", "G0ABC", "JA1ABC", "VK2ABC");

        using var app = Stand(model);

        var scroll = app.Scroll();

        Assert.NotNull(scroll);

        _output.WriteLine("viewport " + Say(scroll!.Viewport.Height)
            + ", extent " + Say(scroll.Extent.Height)
            + ", scrollable " + Say(scroll.Extent.Height - scroll.Viewport.Height));

        // **THERE IS MORE THAN FITS**, which is what makes the rest of this a test.
        Assert.True(
            scroll.Extent.Height > scroll.Viewport.Height,
            "six cards fitted in the viewport, so this proves nothing");

        // **AND THE LAST ONE CAN BE GOT TO.** Scrolling to the bottom brings the
        // furthest card inside the viewport.
        scroll.Offset = new Vector(scroll.Offset.X, scroll.Extent.Height);

        app.Settle();

        var bottom = scroll.Offset.Y + scroll.Viewport.Height;

        _output.WriteLine("scrolled to " + Say(scroll.Offset.Y)
            + ", showing up to " + Say(bottom) + " of " + Say(scroll.Extent.Height));

        Assert.True(
            bottom >= scroll.Extent.Height - 1,
            "the bottom of the content cannot be reached");

        foreach (var (card, top, height) in app.Cards())
        {
            _output.WriteLine("  " + card.PadRight(8) + " top " + Say(top)
                + ", height " + Say(height));
        }
    }

    /// <summary>**No card is clipped horizontally.**</summary>
    /// <remarks>
    /// **THE VERTICAL SCROLL MUST NOT BUY A HORIZONTAL ONE.** A `ScrollViewer` that
    /// scrolls both ways gives its content unlimited width, and a card wider than the
    /// panel is a card with its Log button off the side.
    /// </remarks>
    [AvaloniaFact]
    public void NoCardIsClippedHorizontally()
    {
        var model = Model();

        Answers(model, "K9XP", "W1ABC", "VE3XN", "G0ABC", "JA1ABC", "VK2ABC");

        using var app = Stand(model);

        var scroll = app.Scroll();

        Assert.NotNull(scroll);

        _output.WriteLine("viewport is " + Say(scroll!.Viewport.Width) + " wide, extent "
            + Say(scroll.Extent.Width));

        Assert.True(
            scroll.Extent.Width <= scroll.Viewport.Width + 1,
            "the content is wider than the viewport, so a card is cut off");

        foreach (var (card, _, _) in app.Cards())
        {
            _output.WriteLine("  " + card);
        }

        foreach (var width in app.CardWidths())
        {
            Assert.True(
                width <= scroll.Viewport.Width + 1,
                "a card is " + Say(width) + " wide in a viewport of "
                + Say(scroll.Viewport.Width));
        }
    }

    /// <summary>**A card arriving does not move the scroll off the card it was on.**</summary>
    /// <remarks>
    /// **A SCROLL THAT JUMPS IS WORSE THAN CLIPPING** (the instruction). He is reaching
    /// for Log on one card and a slot lands; if the list jumps to the top or the bottom
    /// he presses whatever arrived instead.
    /// </remarks>
    [AvaloniaFact]
    public void ACardArrivingDoesNotMoveTheScrollOffTheCardItWasOn()
    {
        var model = Model();

        Answers(model, "K9XP", "W1ABC", "VE3XN", "G0ABC", "JA1ABC", "VK2ABC");

        using var app = Stand(model);

        var scroll = app.Scroll();

        Assert.NotNull(scroll);

        // **PARKED HALF WAY DOWN**, which is where he would be reading.
        var parked = (scroll!.Extent.Height - scroll.Viewport.Height) / 2;

        scroll.Offset = new Vector(0, parked);

        app.Settle();

        var before = app.Cards()
            .Select(c => (c.Callsign, Screen: c.Top - scroll.Offset.Y))
            .ToList();

        _output.WriteLine("parked at " + Say(scroll.Offset.Y));

        // **A SEVENTH STATION ANSWERS.**
        Heard(model, "02:17:15", HisCall + " DL1ABC EN52");
        model.CardsNowForTests = Slot("02:17:30");
        model.RebuildCardsForTests();

        app.Settle();

        _output.WriteLine("after the arrival, at " + Say(scroll.Offset.Y));

        var after = app.Cards()
            .Select(c => (c.Callsign, Screen: c.Top - scroll.Offset.Y))
            .ToList();

        foreach (var (call, screen) in before)
        {
            var now = after.FirstOrDefault(a => a.Callsign == call);

            if (now.Callsign is null)
            {
                continue;
            }

            _output.WriteLine("  " + call.PadRight(8)
                + Say(screen) + " -> " + Say(now.Screen));
        }

        // **THE SCROLL IS NOT THROWN TO EITHER END**, which is the thing the
        // instruction calls worse than clipping. It stays exactly where he left it.
        Assert.Equal(parked, scroll.Offset.Y, 1);

        Assert.True(scroll.Offset.Y > 1, "the scroll jumped to the top");

        Assert.True(
            scroll.Offset.Y < scroll.Extent.Height - scroll.Viewport.Height - 1,
            "the scroll jumped to the bottom");

        // **AND THE CARDS DO MOVE UNDER IT, BY EXACTLY ONE CARD.** A new station is
        // put at the top of this panel, so everything below it slides down by that
        // card's height while the offset stays put. **Holding the card he was reading
        // still is not achievable from here**: the panel clears and rebuilds every
        // card on every slot, so after a rebuild there is no *same card* for a scroll
        // anchor to hold on to - a new object with the same callsign is not the object
        // the viewport was pointing at. Fixing that means changing how the cards are
        // rebuilt, which is ask 10's territory and is raised rather than guessed at.
        var moved = before
            .Select(b => (b.Callsign, Was: b.Screen,
                Now: after.FirstOrDefault(a => a.Callsign == b.Callsign).Screen))
            .Where(m => after.Any(a => a.Callsign == m.Callsign))
            .ToList();

        var shifts = moved.Select(m => m.Now - m.Was).Distinct().ToList();

        _output.WriteLine("every card shifted by: "
            + string.Join(", ", shifts.Select(Say)));

        // **ONE SHIFT, THE SAME FOR ALL OF THEM**, which is what tells a reader this
        // is an insertion above rather than a reflow.
        Assert.Single(shifts);
    }

    /// <summary>**The panel shows no scroll it does not need.**</summary>
    /// <remarks>
    /// <para>**AND ONE CARD DOES NOT FIT, WHICH WAS WORTH FINDING OUT.** A conversation
    /// card with its map on it measures **293 px** and the panel at a 1400 by 900 window
    /// is **220 px** of card room, so a scroll bar appears with a single conversation.
    /// That is the honest picture rather than a fault - the card is what it is and the
    /// panel is what the window gives it - but it does mean the earlier draft of this
    /// test, which asserted that one card needs no scroll, was asserting something
    /// untrue about this screen.</para>
    /// <para>**SO WHAT IS ASSERTED IS THE EMPTY CASE AND THE POLICY**: nothing to show
    /// means nothing to scroll, and the bar is asked for only when it is needed rather
    /// than standing there always.</para>
    /// </remarks>
    [AvaloniaFact]
    public void ThePanelShowsNoScrollItDoesNotNeed()
    {
        var model = Model();

        using var app = Stand(model);

        // **WITH NOTHING TO SHOW THE REGION IS NOT THERE AT ALL**, which is the
        // strongest form of not showing a scroll it does not need. The whole thing is
        // bound to `HasDigitalCards`, so an empty panel has no viewport, no bar and no
        // reserved gap where one would be.
        _output.WriteLine("with no cards, the cards region is "
            + (app.Maybe() is null ? "absent" : "present"));

        Assert.Null(app.Maybe());

        // **ONE CARD BRINGS IT BACK.**
        Answers(model, "K9XP");

        app.Settle();

        var scroll = app.Scroll();

        Assert.NotNull(scroll);

        _output.WriteLine("with one card: viewport " + Say(scroll!.Viewport.Height)
            + ", extent " + Say(scroll.Extent.Height));

        // **AND THE BAR IS ASKED FOR ONLY WHEN IT IS NEEDED**, which is the policy
        // rather than a measurement of this window.
        Assert.Equal(ScrollBarVisibility.Auto, scroll.VerticalScrollBarVisibility);
        Assert.Equal(ScrollBarVisibility.Disabled, scroll.HorizontalScrollBarVisibility);

        _output.WriteLine(scroll.Extent.Height > scroll.Viewport.Height
            ? "  -> a bar is needed for a single conversation, because one card with "
              + "its map is taller than the room this window gives the panel"
            : "  -> one card fits and no bar is needed");
    }

    /// <summary>**The receipt and the conversation cards scroll together.**</summary>
    /// <remarks>
    /// **ONE REGION, SO DISMISSING THE RECEIPT LEAVES NO GAP AT THE TOP** (the
    /// instruction). They are one collection and one items control, which is what makes
    /// this true rather than a second thing to keep in step.
    /// </remarks>
    [AvaloniaFact]
    public void TheReceiptAndTheConversationCardsScrollTogether()
    {
        var model = Model();

        Answers(model, "K9XP", "W1ABC");

        model.SendCallToAnyoneCommand.Execute(null);

        using var app = Stand(model);

        var scroll = app.Scroll();

        Assert.NotNull(scroll);

        var inside = app.Cards().Select(c => c.Callsign).ToList();

        _output.WriteLine("in the one scrolling region: " + string.Join(", ", inside));

        Assert.Contains(Ft8ContactLedger.CallToAnyone, inside);
        Assert.Contains("K9XP", inside);
        Assert.Contains("W1ABC", inside);
    }

    private static MainWindowViewModel Model()
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = HisCall;
        settings.Operator.GridSquare = "FN00DJ";

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        model.ClockOffset = new Hamlet.RadioEngine.Audio.ClockOffset(
            0.033, DateTime.UtcNow);

        return model;
    }

    private static void Answers(MainWindowViewModel model, params string[] callers)
    {
        for (var i = 0; i < callers.Length; i++)
        {
            Heard(model, "02:1" + i + ":15", HisCall + " " + callers[i] + " EN52");
        }

        model.CardsNowForTests = Slot("02:16:00");
        model.RebuildCardsForTests();
    }

    private static void Heard(MainWindowViewModel model, string at, string message)
        => model.AddDecodeRowForTests(
            at.Replace(":", "", StringComparison.Ordinal),
            "-09", "0.2", "1240", message, Slot(at), heardOnHz: 14_074_000);

    private static DateTime Slot(string at)
        => DateTime.ParseExact(
            "2026-09-10 " + at, "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

    private static string Say(double value)
        => value.ToString("0.0", CultureInfo.InvariantCulture);

    private static Standing Stand(MainWindowViewModel model)
        => new(model);

    /// <summary>The real window, stood up headless and laid out.</summary>
    private sealed class Standing : IDisposable
    {
        private readonly MainWindow _window;

        public Standing(MainWindowViewModel model)
        {
            _window = new MainWindow
            {
                DataContext = model,
                Width = 1400,
                Height = 900,
            };

            _window.Show();

            Settle();
        }

        public void Settle()
        {
            for (var i = 0; i < 10; i++)
            {
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            }

            _window.UpdateLayout();
        }

        public ItemsControl? Maybe()
            => _window.GetVisualDescendants()
                .OfType<ItemsControl>()
                .FirstOrDefault(c => c.Name == "DigitalContactCards");

        public ItemsControl Control()
            => Maybe() ?? throw new InvalidOperationException("no cards control");

        public ScrollViewer? Scroll()
            => Maybe()?.GetVisualAncestors().OfType<ScrollViewer>().FirstOrDefault();

        public IReadOnlyList<(string Callsign, double Top, double Height)> Cards()
        {
            var control = Control();

            return control.GetVisualDescendants()
                .OfType<ContentPresenter>()
                .Where(p => p.DataContext is Ft8ContactCard)
                .Where(p => p.Bounds.Width > 0)
                .Select(p => (
                    ((Ft8ContactCard)p.DataContext!).Callsign,
                    p.TranslatePoint(new Point(0, 0), control)?.Y ?? 0,
                    p.Bounds.Height))
                .GroupBy(c => c.Callsign)
                .Select(g => g.OrderByDescending(c => c.Height).First())
                .OrderBy(c => c.Item2)
                .ToList();
        }

        public IReadOnlyList<double> CardWidths()
            => Control().GetVisualDescendants()
                .OfType<ContentPresenter>()
                .Where(p => p.DataContext is Ft8ContactCard)
                .Select(p => p.Bounds.Width)
                .ToList();

        public void Dispose() => _window.Close();
    }
}
