using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.VisualTree;
using Hamlet.App.Telemetry;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 332 task 1: **the achievements page is eight badges, and a badge clicks
/// in.**
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-12, ON UNIT 331'S WINDOW**: *"This is wrong. It should open like
/// this, but not scroll. When you click on a category, that category replaces it. It's a
/// click-into system. Also the text is not fitting."*</para>
/// <para>**THE FIT IS MEASURED, NOT EYEBALLED.** Every visible run of text is laid out on
/// its own with its own face and size, and that width is compared with the slot the window
/// gave it and with every box above it. **The test host advances a flat ten pixels a
/// character**, wider than any face on the glass, so a string that fits here fits
/// there.</para>
/// </remarks>
public sealed class TheAchievementsPageClicksInTests
{
    private const string MyGrid = "FN00";

    /// <summary>The longest entity name the cited table holds (work instruction 299).</summary>
    private const string LongestEntity = "Sovereign Military Order of Malta";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the measurements are printed.</param>
    public TheAchievementsPageClicksInTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **The page has no scroller, and nothing sits below the legend.**
    /// </summary>
    [AvaloniaFact]
    public void ThePageHasNoScrollerAndNothingBelowTheLegend()
    {
        var window = Realized(TheAchievementsPageTests.TwelveContacts());

        try
        {
            _output.WriteLine(
                "window " + F(window.Bounds.Width) + " x " + F(window.Bounds.Height));

            var page = Named<Control>(window, "AchievementsPage");

            Assert.True(page.IsEffectivelyVisible, "the page is not showing");

            var scrollers = window.GetVisualDescendants().OfType<ScrollViewer>()
                .Where(s => s.IsEffectivelyVisible)
                .ToList();

            Assert.True(scrollers.Count == 0, "the page has " + scrollers.Count + " scroller(s)");

            var legend = Named<TextBlock>(window, "AchievementsLegend");
            var legendBottom = Top(legend, window) + legend.Bounds.Height;

            _output.WriteLine("legend  : " + legend.Text);
            _output.WriteLine("bottom  : " + F(legendBottom));

            foreach (var text in VisibleText(window))
            {
                var top = Top(text, window);

                Assert.True(
                    top < legendBottom + 0.5,
                    "[" + text.Text + "] starts at " + F(top) + ", below the legend at "
                    + F(legendBottom));
            }

            // **THE OLD SCREEN IS GONE FROM THIS PAGE**: the card view, the belt, Your
            // best and the file path.
            var drawn = VisibleText(window).Select(t => t.Text ?? "").ToList();

            Assert.DoesNotContain(drawn, t => t == "Your best");
            Assert.DoesNotContain(drawn, t => t == "Go and try");
            Assert.DoesNotContain(drawn, t => t.Contains("contacts.adi", StringComparison.Ordinal));
            Assert.DoesNotContain(drawn, t => t.Contains("cannot work", StringComparison.Ordinal));
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// **Clicking a badge replaces the page with its category; the back control returns.**
    /// </summary>
    [AvaloniaFact]
    public void ClickingABadgeReplacesThePageAndTheBackControlReturns()
    {
        var complaints = new List<string>();
        var was = Logger.Sink;

        Logger.Sink = new Collector(complaints);

        try
        {
            var window = Realized(TheAchievementsPageTests.TwelveContacts());

            try
            {
                var size = window.Bounds.Size;
                var badges = Named<ItemsControl>(window, "AchievementsBadges");
                var buttons = badges.GetVisualDescendants().OfType<Button>().ToList();

                _output.WriteLine(
                    "pressable badges: "
                    + string.Join(", ", buttons.Select(b => b.CommandParameter)));

                Assert.Equal(8, buttons.Count);

                Press(window, buttons.Single(
                    b => (b.CommandParameter as string) == AchievementKinds.Countries));

                Assert.False(
                    Named<Control>(window, "AchievementsPage").IsEffectivelyVisible,
                    "the page is still showing under the category");
                Assert.True(
                    Named<Control>(window, "AchievementsCategory").IsEffectivelyVisible,
                    "the category did not replace the page");
                Assert.Equal("Countries", Named<TextBlock>(window, "AchievementsCategoryName").Text);

                // **THE SAME WINDOW AND THE SAME FRAME.**
                Assert.Equal(size, window.Bounds.Size);

                var back = Named<Button>(window, "AchievementsBack");

                _output.WriteLine("back control: " + back.Content);

                Assert.Equal("‹ All achievements", back.Content as string);

                Press(window, back);

                Assert.True(
                    Named<Control>(window, "AchievementsPage").IsEffectivelyVisible,
                    "the back control did not return to the page");
                Assert.False(Named<Control>(window, "AchievementsCategory").IsEffectivelyVisible);

                // **AND CONTINENTS, THEN EUROPE, THEN BACK TWICE**, realized, so every
                // binding inside the category templates is reached.
                Press(window, Named<ItemsControl>(window, "AchievementsBadges")
                    .GetVisualDescendants().OfType<Button>()
                    .Single(b => (b.CommandParameter as string) == AchievementKinds.Continents));

                var seven = Named<ItemsControl>(window, "AchievementsSubBadges")
                    .GetVisualDescendants().OfType<Button>().ToList();

                Assert.Equal(7, seven.Count);

                Press(window, seven.Single(b => (b.CommandParameter as string) == "continent-EU"));

                Assert.Equal("Europe", Named<TextBlock>(window, "AchievementsCategoryName").Text);
                Assert.Equal("‹ Continents", Named<Button>(window, "AchievementsBack").Content as string);

                Press(window, Named<Button>(window, "AchievementsBack"));
                Assert.Equal("Continents", Named<TextBlock>(window, "AchievementsCategoryName").Text);

                Press(window, Named<Button>(window, "AchievementsBack"));
                Assert.True(Named<Control>(window, "AchievementsPage").IsEffectivelyVisible);
            }
            finally
            {
                window.Close();
            }
        }
        finally
        {
            Logger.Sink = was;
        }

        var bindings = complaints
            .Where(l => l.Contains("[Binding]", StringComparison.Ordinal))
            .Distinct()
            .ToList();

        Assert.True(
            bindings.Count == 0,
            "the achievements window has bindings that do not resolve:"
            + Environment.NewLine + string.Join(Environment.NewLine, bindings));
    }

    /// <summary>
    /// **Inside a category: the earned cards, then one unearned, and every card shows its
    /// points.**
    /// </summary>
    [AvaloniaFact]
    public void InsideACategoryTheEarnedCardsComeFirstThenOneUnearned()
    {
        var screen = Screen(TheAchievementsPageTests.TwelveContacts());

        foreach (var kind in AchievementKinds.All.Where(k => k != AchievementKinds.Continents))
        {
            screen.OpenCategoryCommand.Execute(kind);

            var category = screen.Category;

            Assert.True(category is not null, kind + " did not open");

            _output.WriteLine(category!.Name + " - " + category.PointsLine + " - " + category.GapLine);

            foreach (var card in category.Cards)
            {
                _output.WriteLine(
                    "   " + (card.Earned ? "earned " : "next   ") + card.Title.PadRight(24)
                    + card.Figure.PadRight(12) + card.PointsLine);
            }

            var unearned = category.Cards.Where(c => !c.Earned).ToList();

            Assert.True(unearned.Count <= 1, kind + " shows " + unearned.Count + " unearned cards");

            if (unearned.Count == 1)
            {
                Assert.Same(unearned[0], category.Cards[^1]);
            }

            Assert.All(category.Cards, c => Assert.True(c.HasPoints, c.Title + " shows no points"));

            screen.BackCommand.Execute(null);

            Assert.Null(screen.Category);
        }

        // **THE FIXTURE, KIND BY KIND**: eight countries worked and one more to go.
        screen.OpenCategoryCommand.Execute(AchievementKinds.Countries);

        Assert.Equal(8, screen.Category!.Cards.Count(c => c.Earned));
        Assert.Equal("One more country", screen.Category.Cards[^1].Title);
        Assert.Equal("5 pts", screen.Category.Cards[^1].PointsLine);

        screen.BackCommand.Execute(null);

        // **HALL OF FAME: five firsts held, and the 10,000-mile one next at 100.**
        screen.OpenCategoryCommand.Execute(AchievementKinds.HallOfFame);

        Assert.Equal(5, screen.Category!.Cards.Count(c => c.Earned));
        Assert.Equal("Over 10,000 miles", screen.Category.Cards[^1].Title);
        Assert.Equal("100 pts", screen.Category.Cards[^1].PointsLine);

        screen.BackCommand.Execute(null);

        // **TOTAL MILES: no tier reached, the first tier next, and a bar toward it.**
        screen.OpenCategoryCommand.Execute(AchievementKinds.TotalMiles);

        var miles = screen.Category!;

        _output.WriteLine("bar: " + miles.BarLine + " = " + F(miles.BarFraction));

        Assert.True(miles.HasBar);
        Assert.EndsWith("of 50,000 miles", miles.BarLine, StringComparison.Ordinal);
        Assert.InRange(miles.BarFraction, 0.01, 0.99);
        Assert.Equal("50,000 miles", miles.Cards.Single().Title);
    }

    /// <summary>**Continents opens to seven, and each to its countries.**</summary>
    [AvaloniaFact]
    public void ContinentsOpensToSevenAndEachToItsCountries()
    {
        var screen = Screen(TheAchievementsPageTests.TwelveContacts());

        screen.OpenCategoryCommand.Execute(AchievementKinds.Continents);

        var continents = screen.Category;

        Assert.True(continents is not null, "Continents did not open");

        foreach (var badge in continents!.SubBadges)
        {
            _output.WriteLine(
                badge.Name.PadRight(15) + badge.Emblem.PadRight(14) + badge.NextCard.PadRight(20)
                + badge.Standing.PadRight(10) + badge.PointsLine + " | " + badge.GapLine);
        }

        Assert.Equal(7, continents.SubBadges.Count);
        Assert.Equal(7, continents.SubBadges.Select(b => b.Emblem).Distinct().Count());
        Assert.Empty(continents.Cards);

        foreach (var badge in continents.SubBadges)
        {
            screen.OpenCategoryCommand.Execute(badge.Kind);

            var inside = screen.Category!;

            Assert.Equal(badge.Name, inside.Name);
            Assert.Equal("‹ Continents", inside.BackLabel);
            Assert.True(inside.Cards.Count(c => !c.Earned) <= 1);

            screen.BackCommand.Execute(null);

            Assert.Equal(AchievementKinds.Continents, screen.Category!.Kind);
        }

        screen.OpenCategoryCommand.Execute("continent-EU");

        var europe = screen.Category!;

        _output.WriteLine("Europe: " + string.Join(", ", europe.Cards.Select(c => c.Title)));

        Assert.Equal(
            screen.Page!.Log.OnContinent("EU").Select(c => c.Entity).Distinct().Count(),
            europe.Cards.Count(c => c.Earned));
        Assert.Equal("One more country", europe.Cards[^1].Title);

        screen.BackCommand.Execute(null);
        screen.BackCommand.Execute(null);

        Assert.Null(screen.Category);
    }

    /// <summary>
    /// **No string in any slot is clipped or wrapped at the window's own size, measured.**
    /// </summary>
    [AvaloniaFact]
    public void NoStringInAnySlotIsClippedAtTheWindowsSize()
    {
        var window = Realized(TheAchievementsPageTests.TwelveContacts());
        var screen = (AchievementsViewModel)window.DataContext!;

        try
        {
            Fits(window, "the page");

            // **THE LONGEST STRING EACH BADGE SLOT CAN CARRY**, against the slot, whether or
            // not the fixture reaches it.
            var badges = Named<ItemsControl>(window, "AchievementsBadges");

            SlotHolds(badges, "badge-next", AchievementBadgePage.EveryNextCard);
            SlotHolds(badges, "badge-name", new[] { "Hall of Fame", "North America", "South America" });
            SlotHolds(badges, "badge-corner", new[] { "0 pts · unranked", "9,999,999 to Platinum", "1,000 pts · Platinum", "9,999,999 mi so far" });

            foreach (var kind in AchievementKinds.All)
            {
                screen.OpenCategoryCommand.Execute(kind);
                Settle(window);
                Fits(window, kind);
                screen.BackCommand.Execute(null);
                Settle(window);
            }

            screen.OpenCategoryCommand.Execute(AchievementKinds.Continents);
            Settle(window);
            screen.OpenCategoryCommand.Execute("continent-EU");
            Settle(window);
            Fits(window, "continent-EU");

            SlotHolds(
                Named<ItemsControl>(window, "AchievementsCategoryCards"),
                "card-title",
                new[] { LongestEntity });
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// **Opening and closing a category writes the kind and nothing else.**
    /// </summary>
    [AvaloniaFact]
    public void OpeningAndClosingACategoryWritesTheKindAndNothingElse()
    {
        var sink = new Recording();
        var screen = new AchievementsViewModel(
            TheAchievementsPageTests.TwelveContacts(), MyGrid,
            AchievementPoints.Parse(AchievementPoints.Shipped()))
        {
            Telemetry = sink,
        };

        AppEvents.AchievementsOpened(sink, screen.Page!.Badges.Count);
        screen.OpenCategoryCommand.Execute(AchievementKinds.Countries);
        screen.BackCommand.Execute(null);

        foreach (var line in sink.Written)
        {
            _output.WriteLine(line);
        }

        Assert.Equal(
            new[] { "achievements_opened", "achievement_category_opened", "achievement_category_closed" },
            sink.Events.Select(e => e.Name));

        Assert.Equal(new[] { "kind" }, sink.Events[1].Data.Keys);
        Assert.Equal(AchievementKinds.Countries, sink.Events[1].Data["kind"]);
        Assert.Equal(new[] { "kind" }, sink.Events[2].Data.Keys);

        var everything = string.Join(" ", sink.Written);

        foreach (var personal in new[] { "LA8ENA", "Norway", "JO59", "KC3QIS" })
        {
            Assert.DoesNotContain(personal, everything, StringComparison.OrdinalIgnoreCase);
        }
    }

    // ------------------------------------------------------------------------------------

    private static AchievementsViewModel Screen(IReadOnlyList<AdifLogRecord> records)
        => new(records, MyGrid, AchievementPoints.Parse(AchievementPoints.Shipped()));

    /// <summary>The window at its own declared size, over the shipped points file.</summary>
    private static Window Realized(IReadOnlyList<AdifLogRecord> records)
    {
        var window = new AchievementsWindow { DataContext = Screen(records) };

        window.Show();
        Settle(window);

        return window;
    }

    private static void Settle(Window window)
    {
        for (var i = 0; i < 6; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    private static void Press(Window window, Button button)
    {
        Assert.True(
            button.Command?.CanExecute(button.CommandParameter) ?? false,
            "[" + button.Content + "] cannot be pressed");

        button.Command!.Execute(button.CommandParameter);
        Settle(window);
    }

    private static T Named<T>(Window window, string name)
        where T : Control
    {
        var found = window.GetVisualDescendants().OfType<T>().FirstOrDefault(c => c.Name == name);

        Assert.True(found is not null, "there is no " + typeof(T).Name + " named " + name);

        return found!;
    }

    private static IEnumerable<TextBlock> VisibleText(Visual root)
        => root.GetVisualDescendants().OfType<TextBlock>()
            .Where(t => t.IsEffectivelyVisible && (t.Text ?? "").Trim().Length > 0);

    private static double Top(Visual visual, Visual root)
        => visual.TranslatePoint(new Point(0, 0), root)?.Y ?? double.NaN;

    /// <summary>What a run of text needs, laid out on its own with its own face and size.</summary>
    private static double Natural(TextBlock text, string? what = null)
        => new TextLayout(
            what ?? text.Text ?? "",
            new Typeface(text.FontFamily, text.FontStyle, text.FontWeight),
            text.FontSize,
            null).Width;

    /// <summary>
    /// **Every visible run fits its own slot and every box above it, and none wraps.**
    /// </summary>
    private void Fits(Window window, string state)
    {
        var checkedCount = 0;

        foreach (var text in VisibleText(window))
        {
            var needs = Natural(text);
            var has = text.Bounds.Width;

            Assert.True(
                text.TextWrapping == TextWrapping.NoWrap,
                state + ": [" + text.Text + "] is allowed to wrap");

            Assert.True(
                needs <= has + 0.5,
                state + ": [" + text.Text + "] needs " + F(needs) + " px and its slot is " + F(has));

            // **AND EVERY BOX ABOVE IT HOLDS IT**, so a slot wider than the badge it sits in
            // is caught where the badge clips it.
            foreach (var box in text.GetVisualAncestors().OfType<Control>())
            {
                var left = text.TranslatePoint(new Point(0, 0), box)?.X ?? 0;

                Assert.True(
                    left >= -0.5 && left + needs <= box.Bounds.Width + 0.5,
                    state + ": [" + text.Text + "] runs from " + F(left) + " to " + F(left + needs)
                    + " inside a " + box.GetType().Name + " " + F(box.Bounds.Width) + " wide");
            }

            checkedCount++;
        }

        _output.WriteLine(state + ": " + checkedCount + " runs fit");

        Assert.True(checkedCount > 0, state + ": nothing was measured");
    }

    /// <summary>**The longest string a slot can carry, against the narrowest such slot.**</summary>
    private void SlotHolds(Visual root, string slotClass, IEnumerable<string> strings)
    {
        var slots = root.GetVisualDescendants().OfType<TextBlock>()
            .Where(t => t.IsEffectivelyVisible && t.Classes.Contains(slotClass))
            .ToList();

        Assert.True(slots.Count > 0, "no visible slot with class " + slotClass);

        var narrowest = slots.OrderBy(t => t.Bounds.Width).First();
        var longest = strings.OrderByDescending(s => Natural(narrowest, s)).First();
        var needs = Natural(narrowest, longest);

        _output.WriteLine(
            slotClass.PadRight(13) + " slot " + F(narrowest.Bounds.Width) + " px; longest ["
            + longest + "] needs " + F(needs));

        Assert.True(
            needs <= narrowest.Bounds.Width + 0.5,
            slotClass + ": [" + longest + "] needs " + F(needs) + " px and the slot is "
            + F(narrowest.Bounds.Width));
    }

    private static string F(double value) => value.ToString("0.0", CultureInfo.InvariantCulture);

    /// <summary>A sink that remembers, so a test can read what was written.</summary>
    private sealed class Recording : ITelemetry
    {
        public List<(TelemetryCategory Category, string Name,
            IReadOnlyDictionary<string, object?> Data, TelemetryLevel Level)> Events
        { get; } = new();

        public List<string> Written { get; } = new();

        public long DroppedEventCount => 0;

        public void Write(
            TelemetryCategory category,
            string eventName,
            IReadOnlyDictionary<string, object?>? data = null,
            TelemetryLevel level = TelemetryLevel.Info)
        {
            var fields = data ?? new Dictionary<string, object?>();

            Events.Add((category, eventName, fields, level));
            Written.Add(
                category + "/" + eventName + "  "
                + string.Join("  ", fields.Select(kv => kv.Key + "=" + kv.Value)));
        }
    }

    /// <summary>Keeps every line Avalonia logs while the window is up.</summary>
    private sealed class Collector : ILogSink
    {
        private readonly List<string> _lines;

        public Collector(List<string> lines) => _lines = lines;

        public bool IsEnabled(LogEventLevel level, string area) => true;

        public void Log(LogEventLevel level, string area, object? source, string messageTemplate)
            => _lines.Add("[" + area + "] " + messageTemplate);

        public void Log(
            LogEventLevel level, string area, object? source, string messageTemplate,
            params object?[] propertyValues)
            => _lines.Add("[" + area + "] " + messageTemplate + " "
                          + string.Join(", ", propertyValues));
    }
}
