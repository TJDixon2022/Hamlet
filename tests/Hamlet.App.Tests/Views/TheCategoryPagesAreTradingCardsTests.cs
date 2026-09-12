using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Explore;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 335: **every achievements category page is trading cards** (R22).
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-12**: *"So boring."* *"Communicate visually and be appealing. Not white
/// bread boring."* The shape is `assets/category-page-countries.png`.</para>
/// <para>**EVERYTHING HERE IS COMPUTED ON THE HEADLESS HOST, NOT SEEN.** The window is stood up
/// and its layout read back; nothing looks at a pixel.</para>
/// </remarks>
public sealed class TheCategoryPagesAreTradingCardsTests
{
    private const string MyGrid = "FN00";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the measurements are printed.</param>
    public TheCategoryPagesAreTradingCardsTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **Task 1: every kind's band carries its count, score and level, and a bar to the next
    /// level - or says in words that there is none.**
    /// </summary>
    [AvaloniaFact]
    public void EveryKindsBandCarriesCountScoreLevelAndABar()
    {
        var window = Realized(TheAchievementsPageTests.TwelveContacts(), 1040);
        var screen = (AchievementsViewModel)window.DataContext!;

        try
        {
            foreach (var kind in AchievementKinds.All)
            {
                screen.OpenCategoryCommand.Execute(kind);
                Settle(window);

                var category = screen.Category!;
                var score = screen.Page!.Scores.For(kind);
                var band = Named<Border>(window, "AchievementsCategoryBand");
                var said = VisibleText(band).ToList();
                var bars = band.GetVisualDescendants().OfType<BadgeProgressControl>()
                    .Where(b => b.IsEffectivelyVisible)
                    .ToList();

                _output.WriteLine(
                    kind.PadRight(14) + string.Join(" | ", said)
                    + (bars.Count == 1 ? " | bar " + F(bars[0].Fraction) : " | no bar"));

                // **EVERY INK ON THE BAND CLEARS 4.5:1 AGAINST ITS FILL** (§0.6).
                Assert.True(
                    AchievementCategory.Contrast(category.BandInk, category.Band) >= 4.5,
                    kind + "'s band ink " + category.BandInk + " on " + category.Band + " is "
                    + F(AchievementCategory.Contrast(category.BandInk, category.Band)) + ":1");

                // **THE COUNT, THE SCORE AND THE LEVEL, ON THE BAND, FROM THE OWNER'S FILE.**
                Assert.Equal(AchievementCategory.Pts(score.Points), category.ScoreLine);
                Assert.Equal(score.LevelName, category.LevelName);
                Assert.Contains(said, s => s.Contains(category.Standing, StringComparison.Ordinal));
                Assert.Contains(said, s => s.Contains(category.ScoreLine, StringComparison.Ordinal));
                Assert.Contains(said, s => s.Contains(category.LevelName, StringComparison.Ordinal));

                if (score.NextLevelAt is { } next)
                {
                    // **A BAR TOWARD THE NEXT LEVEL, WITH WHAT IT IS OF IN WORDS BESIDE IT.**
                    Assert.True(category.HasLevelBar, kind + " has a next level and no bar");
                    Assert.True(bars.Count == 1, kind + " draws " + bars.Count + " bars on its band");
                    Assert.True(bars[0].Bounds.Width > 0, kind + "'s bar has no width");
                    Assert.Equal(Math.Min(1.0, (double)score.Worked / next), bars[0].Fraction, 3);
                    Assert.Contains(category.LevelBarLine, said);
                    Assert.Contains(score.NextLevelName, category.LevelBarLine, StringComparison.Ordinal);
                }
                else
                {
                    // **NO NEXT LEVEL, NO BAR ASSERTING A FRACTION OF NOTHING, AND WORDS.**
                    Assert.False(category.HasLevelBar, kind + " draws a bar toward no level");
                    Assert.Empty(bars);
                    Assert.True(category.NoNextLevelLine.Length > 0, kind + " says nothing about its level");
                    Assert.Contains(category.NoNextLevelLine, said);
                }

                screen.BackCommand.Execute(null);
                Settle(window);
            }

            // **PINNED AGAINST THE SHIPPED FILE**: eight countries toward the Bronze line at ten,
            // and five modes, which is Gold and the top of that kind's levels.
            screen.OpenCategoryCommand.Execute(AchievementKinds.Countries);

            Assert.Equal("8 of 10 to Bronze", screen.Category!.LevelBarLine);
            Assert.Equal(0.8, screen.Category.LevelFraction, 3);

            screen.BackCommand.Execute(null);
            screen.OpenCategoryCommand.Execute(AchievementKinds.Modes);

            Assert.False(screen.Category!.HasLevelBar);
            Assert.Equal("Gold, the top level", screen.Category.NoNextLevelLine);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// **Task 2: every earned card is the contact that earned it** - the entity large, the
    /// callsign and grid, the path map, the distance, band, mode, date and points - **from the
    /// log entry, with a record that has no grid and one that has no date.**
    /// </summary>
    [AvaloniaFact]
    public void EveryEarnedCardIsTheContactThatEarnedIt()
    {
        var records = FiveContacts();
        var log = new AchievementLog(records, MyGrid);
        var screen = new AchievementsViewModel(
            records, MyGrid, AchievementPoints.Parse(AchievementPoints.Shipped()));

        string Place(string call) => EntitySpoken.Of(DxccPrefixes.EntityOf(call));
        AchievementContact Entry(string call) => log.Contacts.Single(c => c.Callsign == call);

        screen.OpenCategoryCommand.Execute(AchievementKinds.Countries);

        var cards = screen.Category!.Cards.Where(c => c.Earned).ToList();

        foreach (var card in cards)
        {
            _output.WriteLine(
                card.Title.PadRight(26) + card.CallGridLine.PadRight(18) + card.DistanceLine.PadRight(10)
                + card.BandModeLine.PadRight(14) + card.DateLine.PadRight(14) + card.PointsLine
                + (card.HasMap ? "  map " + card.Globe!.OpenFrameLine : "  [" + card.NoMapWord + "] "
                    + string.Join(" / ", card.ContactLines)));
        }

        // **NORWAY IS WORKED TWICE, AND THE EARLIER CONTACT IS LATER IN THE FILE.** The card
        // is the one that earned it: LA1ZZZ on 12 August, not LA8ENA on the 17th.
        var norway = cards.Single(c => c.Title == Place("LA8ENA"));
        var earned = Entry("LA1ZZZ");

        Assert.Equal("LA1ZZZ", norway.Callsign);
        Assert.Equal("JO28", norway.Grid);
        Assert.Equal("LA1ZZZ · JO28", norway.CallGridLine);
        Assert.Equal("40 m · FT8", norway.BandModeLine);
        Assert.Equal("Aug 12, 2026", norway.DateLine);
        Assert.Equal("5 pts", norway.PointsLine);

        // **THE DISTANCE IS THE LOG'S, AND THE MAP IS THE SAME MEASUREMENT.**
        Assert.Equal(
            GridPath.DescribeMiles(earned.Miles!.Value).Replace(" miles", " mi", StringComparison.Ordinal),
            norway.DistanceLine);
        Assert.True(norway.HasMap, "Norway has two grids and no map");
        Assert.True(norway.Globe!.HasPath, "Norway's map has no path");
        Assert.Equal(earned.Miles!.Value, norway.Globe.Miles!.Value, 1);
        Assert.Equal("LA1ZZZ", norway.Globe.Callsign);

        // **NO GRID: NO MAP, SAID IN A WORD, AND A LIST WHERE THE MAP WOULD BE.**
        var noGrid = cards.Single(c => c.Title == Place("VE3PQR"));

        Assert.False(noGrid.HasMap);
        Assert.Null(noGrid.Globe);
        Assert.Equal("no grid, so no map", noGrid.NoMapWord);
        Assert.Equal("VE3PQR", noGrid.CallGridLine);
        Assert.Equal("", noGrid.DistanceLine);
        Assert.NotEmpty(noGrid.ContactLines);

        // **NO DATE: THE CARD SHOWS WHAT IT HAS.**
        var noDate = cards.Single(c => c.Title == Place("G0MNO"));

        Assert.Equal("", noDate.DateLine);
        Assert.Equal(
            AdifLog.BandDisplayNameFor("20m") + " · " + Entry("G0MNO").Mode!.Name,
            noDate.BandModeLine);
        Assert.True(noDate.HasMap);

        // **AND NO DASH ANYWHERE A FACT IS MISSING** (§6).
        foreach (var card in cards)
        {
            foreach (var said in new[] { card.CallGridLine, card.DistanceLine, card.BandModeLine, card.DateLine }
                .Concat(card.ContactLines))
            {
                Assert.DoesNotContain("—", said, StringComparison.Ordinal);
                Assert.False(said.Trim() == "-", card.Title + " draws a dash");
            }
        }

        screen.BackCommand.Execute(null);

        // **GRIDS: THE SQUARE LARGE, AND THE SAME CONTACT UNDER IT.**
        screen.OpenCategoryCommand.Execute(AchievementKinds.Grids);

        var square = screen.Category!.Cards.Single(c => c.Earned && c.Title == "JO28");

        Assert.Equal("LA1ZZZ · " + Place("LA1ZZZ"), square.CallGridLine);
        Assert.True(square.HasMap);
        Assert.Equal(norway.DistanceLine, square.DistanceLine);

        screen.BackCommand.Execute(null);

        // **CONTINENTS' OPENED COUNTRIES ARE THE SAME CARDS.**
        screen.OpenCategoryCommand.Execute(AchievementKinds.Continents);
        screen.OpenCategoryCommand.Execute("continent-EU");

        var inEurope = screen.Category!.Cards.Single(c => c.Earned && c.Title == norway.Title);

        Assert.Equal("LA1ZZZ · JO28", inEurope.CallGridLine);
        Assert.True(inEurope.HasMap);

        // **ON THE WINDOW: A MAP ON EVERY CARD THAT HAS ONE, AND THE WORD AND LIST ON THE ONE
        // THAT DOES NOT.**
        var window = Realized(records, 1040);
        var shown = (AchievementsViewModel)window.DataContext!;

        try
        {
            shown.OpenCategoryCommand.Execute(AchievementKinds.Countries);
            Settle(window);

            var list = Named<ItemsControl>(window, "AchievementsCategoryCards");
            var maps = list.GetVisualDescendants().OfType<Ft8GlobeControl>()
                .Count(m => m.IsEffectivelyVisible && m.Plot is not null && m.Bounds.Width > 0);
            var words = VisibleText(list).Count(t => t == "no grid, so no map");

            _output.WriteLine("window: " + maps + " maps, " + words + " no-map words");

            Assert.Equal(shown.Category!.Cards.Count(c => c.HasMap), maps);
            Assert.Equal(1, words);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// **Task 3: the next card names what it wants and who on the CQ list would earn it, with
    /// distance - and says no one is calling from there when nobody listed would.**
    /// </summary>
    [AvaloniaFact]
    public void TheNextCardKnowsWhoIsCalling()
    {
        var records = TheAchievementsPageTests.TwelveContacts();
        var log = new AchievementLog(records, MyGrid);
        var points = AchievementPoints.Parse(AchievementPoints.Shipped());
        var read = new DateTime(2026, 9, 12, 21, 41, 0, DateTimeKind.Utc);
        var calling = CqSnapshot.From(
            new[]
            {
                Heard("CQ LA8ENA JO59"),
                Heard("CQ OE8DDX JN76"),
                Heard("CQ DX J38DX FK92"),
                Heard("K2ABC W3YNI FN20"),
                Heard("CQ W1AW FN31"),
                Heard("CQ K1ABC FN42"),
            },
            read);

        // **THE SHORT NAME**, which is the tree's own name for a place on a line with a callsign
        // and a distance beside it (`EntitySpoken.Short`).
        string Place(string call) => EntitySpoken.Short(DxccPrefixes.EntityOf(call));

        string Mi(string grid)
            => OperatorLocation.FromGrid(MyGrid) is { } a && OperatorLocation.FromGrid(grid) is { } b
                ? GridPath.DescribeMiles(GridPath.MilesBetween(a, b)).Replace(" miles", " mi", StringComparison.Ordinal)
                : "";

        _output.WriteLine("read at " + calling.ReadAt + ": "
            + string.Join(", ", calling.Calls.Select(c => c.Callsign + " " + c.Grid)));

        // **FIVE CQS; THE REPLY TO W3YNI IS NOT ONE.**
        Assert.Equal(5, calling.Calls.Count);
        Assert.DoesNotContain(calling.Calls, c => c.Callsign == "W3YNI" || c.Callsign == "K2ABC");

        var screen = new AchievementsViewModel(records, MyGrid, points) { Calling = calling };

        void Print(AchievementCategoryCard card)
            => _output.WriteLine(
                "  next [" + card.Title + "] wants [" + card.WantsLine + "] " + card.CallersHeading + ": "
                + string.Join(" / ", card.Callers.Select(c => c.Place + " " + c.CallLine))
                + (card.NoCallerLine.Length > 0 ? " [" + card.NoCallerLine + "]" : ""));

        // **COUNTRIES: THE UNWORKED COUNTRIES CALLING, WITH DISTANCE.** Norway and the United
        // States are in the log, so LA8ENA, W1AW and K1ABC are not listed.
        screen.OpenCategoryCommand.Execute(AchievementKinds.Countries);

        var country = screen.Category!.Cards[^1];

        Print(country);

        var unworked = calling.Calls
            .Where(c => DxccPrefixes.EntityOf(c.Callsign) is { } e
                && !log.Entities.Contains(e, StringComparer.OrdinalIgnoreCase))
            .Select(c => Place(c.Callsign))
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToList();

        Assert.False(country.Earned);
        Assert.Equal("Any country you have not worked", country.WantsLine);
        Assert.Equal("calling CQ at 21:41 UTC, unworked", country.CallersHeading);
        Assert.Contains(Place("OE8DDX"), unworked);
        Assert.Equal(unworked, country.Callers.Select(c => c.Place).OrderBy(p => p, StringComparer.Ordinal));
        Assert.Contains(country.Callers, c => c.Place == Place("OE8DDX") && c.CallLine == "OE8DDX · " + Mi("JN76"));
        Assert.DoesNotContain(country.Callers, c => c.CallLine.StartsWith("LA8ENA", StringComparison.Ordinal));
        Assert.Equal("", country.NoCallerLine);

        screen.BackCommand.Execute(null);

        // **GRIDS: A NEW SQUARE FROM A COUNTRY ALREADY WORKED STILL COUNTS.**
        screen.OpenCategoryCommand.Execute(AchievementKinds.Grids);

        var square = screen.Category!.Cards[^1];

        Print(square);

        Assert.Equal("Any grid you have not worked", square.WantsLine);
        Assert.Contains(square.Callers, c => c.Place == "FN42" && c.CallLine == "K1ABC · " + Mi("FN42"));
        Assert.DoesNotContain(square.Callers, c => c.Place == "FN31" || c.Place == "JO59");

        screen.BackCommand.Execute(null);

        // **INSIDE A CONTINENT: ONLY THE CALLERS ON IT.**
        screen.OpenCategoryCommand.Execute(AchievementKinds.Continents);
        screen.OpenCategoryCommand.Execute("continent-EU");

        var europe = screen.Category!.Cards[^1];

        Print(europe);

        Assert.Equal(new[] { Place("OE8DDX") }, europe.Callers.Select(c => c.Place));

        screen.BackCommand.Execute(null);
        screen.BackCommand.Execute(null);

        // **STATES: A CQ CARRIES NO STATE, AND THE CARD SAYS SO RATHER THAN THAT NO ONE IS
        // CALLING** (the arbiter's proposal, marked for Tim).
        screen.OpenCategoryCommand.Execute(AchievementKinds.States);

        var state = screen.Category!.Cards[^1];

        Print(state);

        Assert.Empty(state.Callers);
        Assert.Equal("Hamlet cannot tell a caller's state", state.NoCallerLine);

        // **A LIST WITH NOBODY WHO WOULD EARN IT.**
        var quiet = new AchievementsViewModel(records, MyGrid, points)
        {
            Calling = CqSnapshot.From(new[] { Heard("CQ LA8ENA JO59"), Heard("CQ W1AW FN31") }, read),
        };

        quiet.OpenCategoryCommand.Execute(AchievementKinds.Countries);

        var none = quiet.Category!.Cards[^1];

        Print(none);

        Assert.Empty(none.Callers);
        Assert.Equal("no one is calling from there now", none.NoCallerLine);
        Assert.Equal("calling CQ at 21:41 UTC, unworked", none.CallersHeading);

        // **ON THE WINDOW: THE HEADING AND EACH CALLER ARE DRAWN ON THE NEXT CARD.**
        var window = Realized(records, 1040, calling);
        var shown = (AchievementsViewModel)window.DataContext!;

        try
        {
            shown.OpenCategoryCommand.Execute(AchievementKinds.Countries);
            Settle(window);

            var said = VisibleText(Named<ItemsControl>(window, "AchievementsCategoryCards")).ToList();

            Assert.Contains("calling CQ at 21:41 UTC, unworked", said);

            foreach (var caller in shown.Category!.Cards[^1].Callers)
            {
                Assert.Contains(caller.Place, said);
                Assert.Contains(caller.CallLine, said);
            }
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>A decoded row, as the list holds one.</summary>
    private static DigitalDecodeRow Heard(string message)
        => new("214100", "-10", "0.2", "1200", message);

    // ------------------------------------------------------------------------------------

    /// <summary>
    /// **Five contacts**: Norway twice with the earlier one later in the file, Canada with no
    /// grid, England with no date, and the operator's own country.
    /// </summary>
    internal static IReadOnlyList<AdifLogRecord> FiveContacts()
        => new[]
        {
            Record("W3YNI", "FT8", null, "20m", "FN20", new DateTime(2026, 8, 14, 21, 41, 30, DateTimeKind.Utc)),
            Record("LA8ENA", "FT8", null, "20m", "JO59", new DateTime(2026, 8, 17, 21, 41, 30, DateTimeKind.Utc)),
            Record("VE3PQR", "FT8", null, "80m", null, new DateTime(2026, 8, 25, 1, 5, 0, DateTimeKind.Utc)),
            Record("LA1ZZZ", "FT8", null, "40m", "JO28", new DateTime(2026, 8, 12, 2, 15, 0, DateTimeKind.Utc)),
            Record("G0MNO", "SSB", null, "20m", "IO91", null),
        };

    private static AdifLogRecord Record(
        string call, string mode, string? submode, string band, string? grid, DateTime? startedUtc)
        => new(
            new AdifContact
            {
                Call = call,
                StationCallsign = "KC3QIS",
                Mode = mode,
                Submode = submode,
                Band = band,
                GridSquare = grid,
                MyGridSquare = MyGrid,
                StartedUtc = startedUtc,
                EndedUtc = startedUtc?.AddMinutes(2),
            },
            Array.Empty<string>(),
            true);

    /// <summary>The window at a stated width, over the shipped points file.</summary>
    private static Window Realized(
        IReadOnlyList<AdifLogRecord> records, double width, CqSnapshot? calling = null)
    {
        var window = new AchievementsWindow
        {
            DataContext = new AchievementsViewModel(
                records, MyGrid, AchievementPoints.Parse(AchievementPoints.Shipped()))
            {
                Calling = calling ?? CqSnapshot.None,
            },
            Width = width,
        };

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

    private static T Named<T>(Window window, string name)
        where T : Control
    {
        var found = window.GetVisualDescendants().OfType<T>().FirstOrDefault(c => c.Name == name);

        Assert.True(found is not null, "there is no " + typeof(T).Name + " named " + name);

        return found!;
    }

    private static IEnumerable<string> VisibleText(Control root)
        => root.GetVisualDescendants().OfType<TextBlock>()
            .Where(t => t.IsEffectivelyVisible && (t.Text ?? "").Trim().Length > 0)
            .Select(t => t.Text!);

    private static string F(double value) => value.ToString("0.00", CultureInfo.InvariantCulture);
}
