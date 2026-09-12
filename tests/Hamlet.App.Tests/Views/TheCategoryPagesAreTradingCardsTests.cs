using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Bands;
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

        Assert.Equal("LA1ZZZ · " + EntitySpoken.Short(DxccPrefixes.EntityOf("LA1ZZZ")), square.CallGridLine);
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

    /// <summary>**Task 4: the other five kinds, each its own way** (R22).</summary>
    [AvaloniaFact]
    public void TheOtherFiveKindsEachDrawTheirOwnCards()
    {
        var records = TheAchievementsPageTests.TwelveContacts();
        var log = new AchievementLog(records, MyGrid);
        var bet = new BandBet("17 m", "best bet now");
        var screen = Screen(records, Calling(), bet);

        static AchievementContact EarliestOf(IEnumerable<AchievementContact> among)
            => among.OrderBy(c => c.StartedUtc ?? DateTime.MaxValue).First();

        void Print(string kind, AchievementCategoryCard card)
            => _output.WriteLine(
                "  " + kind.PadRight(13) + (card.Earned ? "earned " : "next   ") + card.Title.PadRight(20)
                + card.CallGridLine.PadRight(22) + card.DateLine.PadRight(14) + card.CountLine
                + (card.HasTierBar ? " bar[" + card.TierLine + " = " + F(card.TierFraction) + "]" : "")
                + (card.HasMap ? " map" : "")
                + (card.Callers.Count > 0 ? " callers[" + string.Join(" / ", card.Callers.Select(c => c.Place + " " + c.CallLine)) + "]" : "")
                + (card.NoCallerLine.Length > 0 ? " [" + card.NoCallerLine + "]" : ""));

        // **CONTINENTS: SEVEN CARDS, EACH THE FIRST CONTACT THAT OPENED IT WITH THE COUNTRIES
        // WORKED THERE, OR THE CONTINENT AND WHO IS CALLING FROM IT.**
        screen.OpenCategoryCommand.Execute(AchievementKinds.Continents);

        var seven = screen.Category!.SubBadges;

        Assert.Equal(7, seven.Count);

        foreach (var badge in seven)
        {
            var code = badge.Kind[AchievementCategory.ContinentPrefix.Length..];
            var card = badge.Card;

            Assert.True(card is not null, badge.Name + " has no card");
            Print("continents", card!);
            Assert.Equal(badge.Name, card.Title);

            if (log.Continents.Contains(code, StringComparer.OrdinalIgnoreCase))
            {
                var worked = log.EntitiesOn(code);

                Assert.True(card.Earned, badge.Name + " is opened and its card is not earned");
                Assert.Equal(EarliestOf(log.OnContinent(code)).Callsign, card.Callsign);
                Assert.Equal(
                    worked + (worked == 1 ? " country" : " countries") + " worked there",
                    card.CountLine);
            }
            else
            {
                Assert.False(card.Earned);
                Assert.True(card.HasCallersPanel, badge.Name + " says nothing about who is calling");
            }
        }

        var zealand = DxccContinents.NameOf(DxccContinents.Of(DxccPrefixes.EntityOf("ZL1ABC")));

        Assert.Contains(
            seven.Single(b => b.Name == zealand).Card!.Callers,
            c => c.CallLine.StartsWith("ZL1ABC", StringComparison.Ordinal));

        screen.BackCommand.Execute(null);

        // **TOTAL MILES: A TIER IS A BAR TOWARD ITS LINE.**
        screen.OpenCategoryCommand.Execute(AchievementKinds.TotalMiles);

        var tier = screen.Category!.Cards.Single();
        var reached = (long)Math.Round(AchievementScores.MilesIn(log));

        Print("total_miles", tier);

        Assert.False(tier.Earned);
        Assert.True(tier.HasTierBar);
        Assert.Equal(reached / 50000.0, tier.TierFraction, 3);
        Assert.Equal(reached.ToString("#,0", CultureInfo.InvariantCulture) + " of 50,000 mi", tier.TierLine);

        screen.BackCommand.Execute(null);

        // **AND A REACHED TIER IS THE CONTACT THAT CROSSED ITS LINE**: nine contacts to Tokyo,
        // one a day.
        var nine = Enumerable.Range(1, 9)
            .Select(d => Record("JA1XYZ", "FT8", null, "20m", "PM95", new DateTime(2026, 8, d, 12, 0, 0, DateTimeKind.Utc)))
            .ToList();
        var far = Screen(nine, CqSnapshot.None, BandBet.None);

        far.OpenCategoryCommand.Execute(AchievementKinds.TotalMiles);

        var sum = 0.0;
        var crossing = new AchievementLog(nine, MyGrid).Contacts
            .OrderBy(c => c.StartedUtc)
            .First(c => (sum += c.Miles!.Value) >= 50000);
        var crossed = far.Category!.Cards[0];

        Print("total_miles", crossed);

        Assert.True(crossed.Earned);
        Assert.Equal("50,000 miles", crossed.Title);
        Assert.Equal(crossing.StartedUtc!.Value.ToString("MMM d, yyyy", CultureInfo.InvariantCulture), crossed.DateLine);
        Assert.True(crossed.HasMap);
        Assert.True(crossed.HasTierBar);
        Assert.Equal(1.0, crossed.TierFraction, 3);

        // **BANDS: THE FIRST CONTACT ON EACH, AND THE GREEN ZONE'S BEST BET FIRST AMONG THE REST.**
        screen.OpenCategoryCommand.Execute(AchievementKinds.Bands);

        var bands = screen.Category!.Cards;

        foreach (var card in bands)
        {
            Print("bands", card);
        }

        var twenty = bands.Single(c => c.Earned && c.Title == AdifLog.BandDisplayNameFor("20m"));

        Assert.Equal(EarliestOf(log.OnBand("20m")).Callsign, twenty.Callsign);
        Assert.True(twenty.HasMap);
        Assert.False(bands[^1].Earned);
        Assert.Equal(new NextCaller("17 m", "best bet now"), bands[^1].Callers[0]);
        Assert.DoesNotContain(bands[^1].Callers, c => c.Place == twenty.Title);

        screen.BackCommand.Execute(null);

        // **MODES: THE FIRST CONTACT IN EACH, AND WHERE AN UNWORKED MODE LIVES.** The five-contact
        // log has worked FT8 and Voice only.
        var few = Screen(FiveContacts(), Calling(), bet);

        few.OpenCategoryCommand.Execute(AchievementKinds.Modes);

        var modes = few.Category!.Cards;

        foreach (var card in modes)
        {
            Print("modes", card);
        }

        Assert.Equal("LA1ZZZ", modes.Single(c => c.Earned && c.Title == "FT8").Callsign);
        Assert.False(modes[^1].Earned);
        Assert.Equal(
            new[] { "CW", "FT4", "PSK31" },
            modes[^1].Callers.Select(c => c.Place).OrderBy(p => p, StringComparer.Ordinal));

        var home = DigitalCallingFrequencies.BandsWith("PSK31");

        Assert.NotEmpty(home);

        var on = DigitalCallingFrequencies.Find("17 m", "PSK31") is not null ? "17 m" : home[0];
        var block = DigitalCallingFrequencies.Find(on, "PSK31")!;

        Assert.StartsWith(
            (block.JumpHz / 1_000_000.0).ToString("0.000", CultureInfo.InvariantCulture) + " on " + on,
            modes[^1].Callers.Single(c => c.Place == "PSK31").CallLine,
            StringComparison.Ordinal);

        // **`ModeFirstRow.Why` IS NEVER ON A CARD**: it says false things (parked).
        foreach (var card in modes)
        {
            foreach (var said in new[] { card.Title, card.WantsLine, card.NoCallerLine }
                .Concat(card.Callers.Select(c => c.CallLine)))
            {
                Assert.DoesNotContain("cannot work", said, StringComparison.Ordinal);
            }
        }

        // **HALL OF FAME: THE CONTACT THAT EARNED EACH FIRST; THE NEXT ONE AS UNIT 333 LEFT IT.**
        screen.OpenCategoryCommand.Execute(AchievementKinds.HallOfFame);

        var firsts = screen.Category!.Cards;

        foreach (var card in firsts)
        {
            Print("hall_of_fame", card);
        }

        var mine = log.Contacts.Select(c => c.Entity).First(e => e is not null);

        Assert.Equal(EarliestOf(log.Contacts).Callsign, firsts.Single(c => c.Title == "Your first contact").Callsign);
        Assert.Equal(
            EarliestOf(log.Contacts.Where(c => c.Entity is not null && c.Entity != mine)).Callsign,
            firsts.Single(c => c.Title == "A DX contact").Callsign);
        Assert.Equal("DL1ABC", firsts.Single(c => c.Title == "A PSK31 contact").Callsign);
        Assert.Equal("VA3VRR", firsts.Single(c => c.Title == "A Morse contact").Callsign);
        Assert.Equal(
            EarliestOf(log.Contacts.Where(c => c.Miles >= 5000)).Callsign,
            firsts.Single(c => c.Title == "Over 5,000 miles").Callsign);
        Assert.All(firsts.Where(c => c.Earned), c => Assert.True(c.HasMap, c.Title + " has no map"));

        var nextFirst = firsts[^1];
        var furthest = log.Contacts.Where(c => c.Miles is not null).Max(c => c.Miles!.Value);

        Assert.Equal("Over 10,000 miles", nextFirst.Title);
        Assert.True(nextFirst.HasTierBar);
        Assert.Equal(Math.Min(1.0, furthest / 10000), nextFirst.TierFraction, 3);
    }

    /// <summary>
    /// **Task 4, page-wide: at a window 1400 and 1920 wide, no string in any slot clips or wraps a
    /// word, and no card is a white rectangle - every card has a map, a bar or a list.**
    /// </summary>
    [AvaloniaFact]
    public void NoStringClipsAndNoCardIsWhiteAtFourteenHundredAndNineteenTwenty()
    {
        var bet = new BandBet("17 m", "best bet now");

        foreach (var width in new[] { 1400.0, 1920.0 })
        {
            var window = Realized(TheAchievementsPageTests.TwelveContacts(), width, Calling(), bet);
            var screen = (AchievementsViewModel)window.DataContext!;

            try
            {
                _output.WriteLine("WINDOW " + F(window.Bounds.Width) + " x " + F(window.Bounds.Height));

                foreach (var kind in AchievementKinds.All.Concat(new[] { "continent-EU", "continent-OC" }))
                {
                    if (kind.StartsWith(AchievementCategory.ContinentPrefix, StringComparison.Ordinal))
                    {
                        screen.OpenCategoryCommand.Execute(AchievementKinds.Continents);
                    }

                    screen.OpenCategoryCommand.Execute(kind);
                    Settle(window);

                    var state = F(width) + " " + kind;
                    var runs = Fits(window, state);
                    var cards = window.GetVisualDescendants().OfType<Border>()
                        .Where(b => b.IsEffectivelyVisible && b.Classes.Contains("trading-card"))
                        .ToList();

                    Assert.True(cards.Count > 0, state + ": no trading card is drawn");

                    var white = 0;

                    foreach (var card in cards)
                    {
                        var inside = card.GetVisualDescendants().Where(v => v is Control { IsEffectivelyVisible: true }).ToList();
                        var map = inside.OfType<Ft8GlobeControl>().Any(m => m.Plot is not null && m.Bounds.Width > 0);
                        var bar = inside.OfType<BadgeProgressControl>().Any(b => b.Bounds.Width > 0);
                        var list = inside.OfType<Border>().Any(b => b.Classes.Contains("card-list")
                            && VisibleText(b).Any());

                        if (!(map || bar || list))
                        {
                            white++;
                            _output.WriteLine("  white card: " + string.Join(" | ", VisibleText(card)));
                        }
                    }

                    _output.WriteLine("  " + kind.PadRight(14) + runs + " runs fit, " + cards.Count + " cards, " + white + " white");

                    Assert.True(white == 0, state + ": " + white + " card(s) with no map, bar or list");

                    while (screen.Category is not null)
                    {
                        screen.BackCommand.Execute(null);
                    }

                    Settle(window);
                }
            }
            finally
            {
                window.Close();
            }
        }
    }

    /// <summary>
    /// **Work instruction 336 task 1: States counts what the log's `STATE` field says** (R23), and
    /// only on a United States, Alaska or Hawaii record carrying one of the fifty codes.
    /// </summary>
    /// <remarks>
    /// <para>**THE FIXTURE IS ADI TEXT, SO THE READER IS WHAT IS TESTED.** Five records: a US
    /// contact with `STATE=PA`, a US contact with no `STATE`, an Alaska contact with `STATE=AK`, a
    /// Canadian contact with `STATE=ON` and a US contact with `STATE=DC`. Two score.</para>
    /// <para>**THE NEXT CARD IS UNCHANGED** - its words and its sentence are unit 335's, marked for
    /// Tim - and the page is measured at 1400 and 1920 now that States draws earned cards.</para>
    /// </remarks>
    [AvaloniaFact]
    public void StatesCountWhatTheLogsStateFieldSays()
    {
        var records = StateContacts();
        var points = AchievementPoints.Parse(AchievementPoints.Shipped());
        var log = new AchievementLog(records, MyGrid);
        var score = new AchievementScores(log, points).For(AchievementKinds.States);
        var screen = new AchievementsViewModel(records, MyGrid, points);
        var badge = screen.Page!.Badges.Single(b => b.Kind == AchievementKinds.States);

        foreach (var contact in log.Contacts)
        {
            _output.WriteLine(
                contact.Callsign.PadRight(8) + (contact.Entity ?? "no entity").PadRight(26)
                + "STATE " + (contact.State ?? "none").PadRight(5)
                + (AchievementLog.StateOf(contact) is { } scored ? "scores " + scored : "scores nothing"));
        }

        _output.WriteLine(
            "worked " + score.Worked + ", points " + score.Points + ", badge [" + badge.Standing + "]");

        // **THE FIXTURE MEANS WHAT IT SAYS**: the entities are the cited table's, not assumed.
        Assert.Equal("United States of America", log.Contacts.Single(c => c.Callsign == "K3PA").Entity);
        Assert.Equal("Alaska", log.Contacts.Single(c => c.Callsign == "KL7XYZ").Entity);
        Assert.Equal("Canada", log.Contacts.Single(c => c.Callsign == "VE3PQR").Entity);

        // **TWO: PA ON A US RECORD AND AK ON AN ALASKA ONE.** No STATE, ON and DC score nothing.
        Assert.Equal(2, AchievementScores.WorkedIn(AchievementKinds.States, log));
        Assert.Equal(2, score.Worked);

        // **THE SCORE IS THE SHIPPED FILE'S**: `per` for PA and AK's `special`, which replaces it.
        var fromTheFile = points.Per(AchievementKinds.States)!.Value
            + points.Special(AchievementKinds.States, "AK")!.Value;

        _output.WriteLine("from the file: per " + points.Per(AchievementKinds.States)
            + " + AK " + points.Special(AchievementKinds.States, "AK") + " = " + fromTheFile);

        Assert.Equal(12, fromTheFile);
        Assert.Equal(fromTheFile, score.Points);

        // **THE BADGE COUNT MATCHES THE LOG.**
        Assert.Equal(2, badge.Score.Worked);
        Assert.StartsWith("2 ", badge.Standing, StringComparison.Ordinal);

        // **THE PAGE DRAWS TWO EARNED CARDS, EACH THE CONTACT THAT EARNED IT.**
        screen.OpenCategoryCommand.Execute(AchievementKinds.States);

        var earned = screen.Category!.Cards.Where(c => c.Earned).ToList();

        foreach (var card in screen.Category.Cards)
        {
            _output.WriteLine(
                card.Title.PadRight(18) + card.CallGridLine.PadRight(18) + card.PointsLine.PadRight(8)
                + (card.Earned ? (card.HasMap ? "map" : card.NoMapWord) : card.WantsLine + " / " + card.NoCallerLine));
        }

        Assert.Equal(new[] { "AK", "PA" }, earned.Select(c => c.Title));
        Assert.Equal("KL7XYZ", earned[0].Callsign);
        Assert.Equal("10 pts", earned[0].PointsLine);
        Assert.Equal("K3PA", earned[1].Callsign);
        Assert.Equal("2 pts", earned[1].PointsLine);
        Assert.All(earned, c => Assert.True(c.HasMap, c.Title + " has no map"));

        // **AND THE NEXT CARD AND ITS SENTENCE ARE AS UNIT 335 LEFT THEM.**
        var next = screen.Category.Cards.Single(c => !c.Earned);

        Assert.Equal("Any state you have not worked", next.WantsLine);
        Assert.Equal(AchievementCategory.NoStateFromTheAir, next.NoCallerLine);

        // **MEASURED AT 1400 AND 1920 NOW THAT STATES HAS CARDS**: nothing clips or wraps, and no
        // card is white.
        foreach (var width in new[] { 1400.0, 1920.0 })
        {
            var window = Realized(records, width);
            var shown = (AchievementsViewModel)window.DataContext!;

            try
            {
                shown.OpenCategoryCommand.Execute(AchievementKinds.States);
                Settle(window);

                var state = F(width) + " states";
                var runs = Fits(window, state);
                var cards = window.GetVisualDescendants().OfType<Border>()
                    .Where(b => b.IsEffectivelyVisible && b.Classes.Contains("trading-card"))
                    .ToList();
                var white = cards.Count(card =>
                {
                    var inside = card.GetVisualDescendants().Where(v => v is Control { IsEffectivelyVisible: true }).ToList();

                    return !(inside.OfType<Ft8GlobeControl>().Any(m => m.Plot is not null && m.Bounds.Width > 0)
                        || inside.OfType<BadgeProgressControl>().Any(b => b.Bounds.Width > 0)
                        || inside.OfType<Border>().Any(b => b.Classes.Contains("card-list") && VisibleText(b).Any()));
                });

                _output.WriteLine(state + ": " + runs + " runs fit, " + cards.Count + " cards, " + white + " white");

                Assert.Equal(3, cards.Count);
                Assert.Equal(0, white);
            }
            finally
            {
                window.Close();
            }
        }
    }

    /// <summary>
    /// **Five records as ADI text, with `STATE` where the record carries one** - PA on a US call,
    /// none on a US call, AK on an Alaska call, ON on a Canadian call, DC on a US call.
    /// </summary>
    internal static IReadOnlyList<AdifLogRecord> StateContacts()
    {
        static string One(string call, string grid, string? state, int day)
        {
            var started = new DateTime(2026, 8, day, 1, 15, 0, DateTimeKind.Utc);
            var record = AdifLog.Record(new AdifContact
            {
                Call = call,
                StationCallsign = "KC3QIS",
                Mode = "FT8",
                Band = "20m",
                GridSquare = grid,
                MyGridSquare = MyGrid,
                StartedUtc = started,
                EndedUtc = started.AddMinutes(2),
            });

            return state is null
                ? record
                : record.Replace(
                    "<EOR>",
                    "<STATE:" + state.Length.ToString(CultureInfo.InvariantCulture) + ">" + state + "\n<EOR>",
                    StringComparison.Ordinal);
        }

        return AdifLog.ReadRecords(
            AdifLog.Header("test")
            + One("K3PA", "FN10", "PA", 3)
            + One("W1AW", "FN31", null, 4)
            + One("KL7XYZ", "BP51", "AK", 5)
            + One("VE3PQR", "FN03", "ON", 6)
            + One("N3DC", "FM18", "DC", 7));
    }

    /// <summary>
    /// **Every visible run fits its own slot and every box above it, and none may wrap** - the
    /// same measurement `TheAchievementsPageClicksInTests` makes at the window's own size.
    /// </summary>
    private int Fits(Window window, string state)
    {
        var count = 0;

        foreach (var text in window.GetVisualDescendants().OfType<TextBlock>()
            .Where(t => t.IsEffectivelyVisible && (t.Text ?? "").Trim().Length > 0))
        {
            var needs = new Avalonia.Media.TextFormatting.TextLayout(
                text.Text ?? "",
                new Avalonia.Media.Typeface(text.FontFamily, text.FontStyle, text.FontWeight),
                text.FontSize,
                null).Width;

            Assert.True(
                text.TextWrapping == Avalonia.Media.TextWrapping.NoWrap,
                state + ": [" + text.Text + "] is allowed to wrap");
            Assert.True(
                needs <= text.Bounds.Width + 0.5,
                state + ": [" + text.Text + "] needs " + F(needs) + " px and its slot is " + F(text.Bounds.Width));

            foreach (var box in text.GetVisualAncestors().OfType<Control>())
            {
                var left = text.TranslatePoint(new Avalonia.Point(0, 0), box)?.X ?? 0;

                Assert.True(
                    left >= -0.5 && left + needs <= box.Bounds.Width + 0.5,
                    state + ": [" + text.Text + "] runs from " + F(left) + " to " + F(left + needs)
                    + " inside a " + box.GetType().Name + " " + F(box.Bounds.Width) + " wide");
            }

            count++;
        }

        return count;
    }

    private static AchievementsViewModel Screen(
        IReadOnlyList<AdifLogRecord> records, CqSnapshot calling, BandBet bet)
        => new(records, MyGrid, AchievementPoints.Parse(AchievementPoints.Shipped()))
        {
            Calling = calling,
            BestBet = bet,
        };

    /// <summary>Four callers: Austria, Grenada, a new square in the US, and New Zealand.</summary>
    private static CqSnapshot Calling()
        => CqSnapshot.From(
            new[]
            {
                Heard("CQ OE8DDX JN76"),
                Heard("CQ DX J38DX FK92"),
                Heard("CQ K1ABC FN42"),
                Heard("CQ ZL1ABC RF72"),
            },
            new DateTime(2026, 9, 12, 21, 41, 0, DateTimeKind.Utc));

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
        IReadOnlyList<AdifLogRecord> records, double width, CqSnapshot? calling = null, BandBet? bet = null)
    {
        var window = new AchievementsWindow
        {
            DataContext = Screen(records, calling ?? CqSnapshot.None, bet ?? BandBet.None),
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
