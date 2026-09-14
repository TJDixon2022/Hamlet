using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Telemetry;
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

                    // **WORK INSTRUCTION 342 RULING 13: THE GAP IS ON THE LINE AS WELL**, as the
                    // picture's `· 14 to Silver` is, beside the words over the bar - and drawn.
                    Assert.True(category.GapLine.Length > 0, kind + " has a next level and no gap to it");
                    Assert.Contains(score.NextLevelName, category.GapLine, StringComparison.Ordinal);
                    Assert.EndsWith(" · " + category.GapLine, category.BandLine, StringComparison.Ordinal);
                    Assert.Contains(category.BandLine, said);
                }
                else
                {
                    // **NO NEXT LEVEL, NO BAR ASSERTING A FRACTION OF NOTHING, AND WORDS.**
                    Assert.False(category.HasLevelBar, kind + " draws a bar toward no level");
                    Assert.Empty(bars);
                    Assert.True(category.NoNextLevelLine.Length > 0, kind + " says nothing about its level");
                    Assert.Contains(category.NoNextLevelLine, said);

                    // **AND NO GAP CLAUSE**: the line ends at the level (ruling 13).
                    Assert.Equal("", category.GapLine);
                    Assert.EndsWith(category.LevelName, category.BandLine, StringComparison.Ordinal);
                }

                screen.BackCommand.Execute(null);
                Settle(window);
            }

            // **PINNED AGAINST THE SHIPPED FILE**: eight countries toward the Bronze line at ten,
            // and five modes, which is Gold and the top of that kind's levels.
            screen.OpenCategoryCommand.Execute(AchievementKinds.Countries);

            Assert.Equal("8 of 10 to Bronze", screen.Category!.LevelBarLine);
            Assert.Equal(0.8, screen.Category.LevelFraction, 3);

            // **RULING 13, PINNED**: the picture's shape, `· 2 to Bronze` on the line.
            Assert.Equal("one per entity · 8 worked · 40 pts · unranked · 2 to Bronze", screen.Category.BandLine);

            // **AND WHERE THE LINE WITH THE GAP WOULD PASS SIXTY CHARACTERS** - the band line's
            // 608 px slot at the window's own 1040 on the test host - the meaning goes and the
            // name above it still says what the kind is (§6: shortened, and said which).
            screen.BackCommand.Execute(null);
            screen.OpenCategoryCommand.Execute(AchievementKinds.Grids);

            Assert.Equal("10 worked · 20 pts · Bronze · 15 to Silver", screen.Category!.BandLine);

            screen.BackCommand.Execute(null);
            screen.OpenCategoryCommand.Execute(AchievementKinds.TotalMiles);

            Assert.Equal("42,041 mi so far · 0 pts · unranked · 7,959 to Bronze", screen.Category!.BandLine);

            screen.BackCommand.Execute(null);
            screen.OpenCategoryCommand.Execute(AchievementKinds.Modes);

            Assert.False(screen.Category!.HasLevelBar);
            Assert.Equal("Gold, the top level", screen.Category.NoNextLevelLine);
            Assert.Equal("five modes to work · 5 of 5 · 85 pts · Gold", screen.Category.BandLine);
        }
        finally
        {
            window.Close();
        }

        // **WORK INSTRUCTION 345 TASK 1, RULING 17: THE BAND AS DRAWN AT 1400 AND 1920**, on all eight
        // kinds and all seven continent pages. The 1040 run and its pinned strings above stay; this is
        // what criterion 1 is counted on.
        var watched = false;

        foreach (var width in new[] { 1400.0, 1920.0 })
        {
            var wide = Realized(TheAchievementsPageTests.TwelveContacts(), width, Calling(), new BandBet("17 m", "best bet now"));
            var opened = (AchievementsViewModel)wide.DataContext!;

            try
            {
                foreach (var kind in AchievementKinds.All.Concat(ContinentKinds()))
                {
                    OpenOnWindow(wide, opened, kind);

                    var miss = BandMiss(wide, opened.Category!);

                    _output.WriteLine(
                        F(width) + " " + kind.PadRight(14)
                        + (miss ?? "drawn: " + string.Join(" | ", VisibleText(Named<Border>(wide, "AchievementsCategoryBand")))));

                    Assert.True(miss is null, F(width) + " " + kind + ": " + miss);

                    if (!watched)
                    {
                        // **RULING 19, WATCHED RED ON THE TEST WINDOW ONLY**: this drawn band held
                        // against the band of a kind it is not. Nothing in the view changes.
                        var other = Screen(TheAchievementsPageTests.TwelveContacts(), CqSnapshot.None, BandBet.None);

                        other.OpenCategoryCommand.Execute(kind == AchievementKinds.Modes ? AchievementKinds.Countries : AchievementKinds.Modes);

                        var wrong = BandMiss(wide, other.Category!);

                        _output.WriteLine(F(width) + " " + kind + " held against " + other.Category!.Kind + "'s band, watched red: " + wrong);

                        Assert.NotNull(wrong);
                        watched = true;
                    }

                    ToThePage(wide, opened);
                }
            }
            finally
            {
                wide.Close();
            }
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

        // **WORK INSTRUCTION 342 TASK 1, RULING 11: AT 1400 AND 1920 THE MAP RUNS THE CARD'S INNER
        // WIDTH AT THE STATED HEIGHT, AND THE NO-MAP LIST IS THE SAME HEIGHT.** Ruling 12: both
        // stations lie inside the frame the control drew, and that frame is no larger than the
        // crop bound - which a whole-globe frame fails, asserted on every card and shown once on
        // the test window.
        var wholeGlobeShown = false;

        foreach (var width in new[] { 1400.0, 1920.0 })
        {
            foreach (var fixture in new[] { TheAchievementsPageTests.TwelveContacts(), FiveContacts() })
            {
                var wide = Realized(fixture, width);
                var opened = (AchievementsViewModel)wide.DataContext!;

                try
                {
                    opened.OpenCategoryCommand.Execute(AchievementKinds.Countries);
                    Settle(wide);

                    var drawn = Named<ItemsControl>(wide, "AchievementsCategoryCards").GetVisualDescendants().OfType<Border>()
                        .Where(b => b.IsEffectivelyVisible && b.Classes.Contains("trading-card")
                            && b.DataContext is AchievementCategoryCard { Earned: true })
                        .ToList();

                    Assert.NotEmpty(drawn);

                    foreach (var card in drawn)
                    {
                        var data = (AchievementCategoryCard)card.DataContext!;
                        var inner = card.GetVisualDescendants().OfType<StackPanel>().First(s => Grid.GetColumn(s) == 1).Bounds.Width;
                        var state = F(width) + " " + data.Title;

                        if (!data.HasMap)
                        {
                            var list = card.GetVisualDescendants().OfType<Border>()
                                .Single(b => b.IsEffectivelyVisible && b.Classes.Contains("card-list"));

                            _output.WriteLine(state + ": no map, its list " + F(list.Bounds.Width) + " x " + F(list.Bounds.Height));

                            Assert.True(
                                Math.Abs(list.Bounds.Height - CardMapHeight) <= 0.5,
                                state + ": the no-map list is " + F(list.Bounds.Height) + " tall, not the map's " + F(CardMapHeight));

                            continue;
                        }

                        var globe = card.GetVisualDescendants().OfType<Ft8GlobeControl>().Single(g => g.IsEffectivelyVisible);
                        var plot = globe.Plot!;
                        var frame = globe.DrawnFrame;
                        var scale = Math.Min(globe.Bounds.Width / frame.Width, globe.Bounds.Height / frame.Height);
                        var drawnWidth = frame.Width * scale;
                        var drawnHeight = frame.Height * scale;

                        _output.WriteLine(
                            state + ": inner " + F(inner) + ", map drawn " + F(drawnWidth) + " x " + F(drawnHeight)
                            + " in a control " + F(globe.Bounds.Width) + " x " + F(globe.Bounds.Height)
                            + ", frame left " + F(frame.Left) + " top " + F(frame.Top) + " " + F(frame.Width) + " by " + F(frame.Height));

                        // **RULING 11: ACROSS THE CARD, AT THE STATED HEIGHT.**
                        Assert.True(
                            Math.Abs(drawnWidth - inner) <= 1,
                            state + ": the map is drawn " + F(drawnWidth) + " x " + F(drawnHeight) + " in a card " + F(inner) + " wide inside");
                        Assert.True(
                            Math.Abs(globe.Bounds.Height - CardMapHeight) <= 0.5 && Math.Abs(drawnHeight - CardMapHeight) <= 1,
                            state + ": the map is " + F(drawnHeight) + " tall in a control " + F(globe.Bounds.Height) + ", not " + F(CardMapHeight));

                        // **RULING 12: CROPPED TO THE TWO STATIONS, AS A NUMBER.**
                        var outside = OutsideTheCrop(plot, frame, inner, CardMapHeight);

                        Assert.True(outside is null, state + ": " + outside);

                        // **AND THE BOUND IS NOT LOOSE**: the whole file fails it on this card.
                        Assert.NotNull(OutsideTheCrop(
                            plot, (0, 0, Ft8GlobePlot.Map.WidthPixels, Ft8GlobePlot.Map.HeightPixels), inner, CardMapHeight));

                        if (!wholeGlobeShown)
                        {
                            // **SHOWN ONCE ON THE TEST WINDOW**: this card's globe opened to the
                            // whole file, and the same check says what is wrong. Put back after.
                            globe.Opened = false;
                            Settle(wide);

                            var wholeGlobe = OutsideTheCrop(plot, globe.DrawnFrame, inner, CardMapHeight);

                            _output.WriteLine(state + ", a whole-globe frame set on the test window: " + wholeGlobe);

                            Assert.NotNull(wholeGlobe);

                            globe.Opened = true;
                            Settle(wide);
                            wholeGlobeShown = true;
                        }
                    }
                }
                finally
                {
                    wide.Close();
                }
            }
        }

        // **WORK INSTRUCTION 345 TASK 2, RULING 21: AT 1400 AND 1920 EVERY EARNED CARD ON COUNTRIES,
        // STATES AND GRIDS DRAWS THE CONTACT THAT EARNED IT** - entity, callsign, grid, distance, band,
        // mode, date and points, each from the log entry rather than from the card - and where the log
        // lacks a fact, what is there, no dash and no map without a grid (§6). The map and crop above
        // stay. `StatesCountWhatTheLogsStateFieldSays` measures States at both widths for fit and white
        // cards only, so States' facts are asserted here and not duplicated.
        var shipped = AchievementPoints.Parse(AchievementPoints.Shipped());
        var watchedFacts = false;

        foreach (var width in new[] { 1400.0, 1920.0 })
        {
            foreach (var (fixture, label, kind) in new[]
            {
                (TheAchievementsPageTests.TwelveContacts(), "twelve contacts", AchievementKinds.Countries),
                (FiveContacts(), "five contacts", AchievementKinds.Countries),
                (TheAchievementsPageTests.TwelveContacts(), "twelve contacts", AchievementKinds.Grids),
                (FiveContacts(), "five contacts", AchievementKinds.Grids),
                (StateContacts(), "state contacts", AchievementKinds.States),
            })
            {
                var contacts = new AchievementLog(fixture, MyGrid).Contacts;
                var wide = Realized(fixture, width);
                var opened = (AchievementsViewModel)wide.DataContext!;
                var where = F(width) + " " + label + " " + kind;

                try
                {
                    OpenOnWindow(wide, opened, kind);

                    var drawn = TradingCards(wide).Where(b => b.DataContext is AchievementCategoryCard { Earned: true }).ToList();
                    var keys = contacts.Select(c => EarnedBy(kind, c)).OfType<string>().Distinct(StringComparer.Ordinal).ToList();

                    Assert.True(drawn.Count == keys.Count, where + ": " + drawn.Count + " earned cards drawn for " + keys.Count + " earned in the log");

                    foreach (var card in drawn)
                    {
                        var title = card.GetVisualDescendants().OfType<TextBlock>()
                            .Single(t => t.IsEffectivelyVisible && t.Classes.Contains("card-title")).Text ?? "";
                        var entry = contacts.Where(c => EarnedBy(kind, c) == title).OrderBy(c => c.StartedUtc ?? DateTime.MaxValue).FirstOrDefault();

                        Assert.True(entry is not null, where + ": the drawn title [" + title + "] is nothing the log earned");

                        var miss = EarnedCardMiss(card, kind, entry!, shipped);

                        _output.WriteLine(where + " [" + title + "] " + entry!.Callsign + ": " + (miss ?? "all eight drawn, or what the entry has"));

                        Assert.True(miss is null, where + " [" + title + "]: " + miss);

                        if (!watchedFacts
                            && contacts.FirstOrDefault(c => EarnedBy(kind, c) == title && c.Callsign != entry.Callsign) is { } later)
                        {
                            // **RULING 19, WATCHED RED ON THE TEST WINDOW ONLY**: this drawn card held
                            // against the other contact the log has for the same place.
                            var wrong = EarnedCardMiss(card, kind, later, shipped);

                            _output.WriteLine(where + " [" + title + "] held against " + later.Callsign + "'s entry, watched red: " + wrong);

                            Assert.NotNull(wrong);
                            watchedFacts = true;
                        }
                    }
                }
                finally
                {
                    wide.Close();
                }
            }
        }
    }

    /// <summary>
    /// **Work instruction 342 task 3, the nice-to-pass: a card's map opens in its popup on a click,
    /// never on a hover, and a click outside closes it** (ruling 16), at 1400 and 1920.
    /// </summary>
    /// <remarks>
    /// <para>**ITS OWN METHOD** (R14): the popup is the nice-to-pass, and no must-pass test is its
    /// home.</para>
    /// <para>**NOTHING IS WRITTEN FOR A MAP OPENED OR CLOSED**, because the conversation card's popup
    /// writes nothing: opening a map is a view and not a stage (R13).</para>
    /// <para>**REAL CLICKS ON THE WINDOW**, at the map's own place, so a map that could not be
    /// reached by a mouse fails here rather than passing on a command call.</para>
    /// </remarks>
    [AvaloniaTheory]
    [InlineData(1400.0)]
    [InlineData(1920.0)]
    public void ACardsMapOpensInItsPopupOnAClickAndAClickOutsideClosesIt(double width)
    {
        // **ONE WINDOW PER CASE, AND THE POINTER MOVES BEFORE IT CLICKS OUTSIDE.** With a press sent
        // straight to the outside point, whichever window came second kept its popup open - 1920
        // when it ran second and 1400 when it did - in one loop and as two cases alike, and
        // activating the window changed nothing. Moving the pointer there first, as a mouse does,
        // closed it in both. Each width stays its own case, as the band tests here already are.
        {
            var sink = new Written();
            var window = new AchievementsWindow
            {
                DataContext = new AchievementsViewModel(
                    TheAchievementsPageTests.TwelveContacts(), MyGrid, AchievementPoints.Parse(AchievementPoints.Shipped()))
                {
                    Telemetry = sink,
                },
                Width = width,
            };

            window.Show();
            Settle(window);

            try
            {
                var screen = (AchievementsViewModel)window.DataContext!;

                screen.OpenCategoryCommand.Execute(AchievementKinds.Countries);
                Settle(window);

                var card = Named<ItemsControl>(window, "AchievementsCategoryCards").GetVisualDescendants().OfType<Border>()
                    .First(b => b.IsEffectivelyVisible && b.Classes.Contains("trading-card")
                        && b.DataContext is AchievementCategoryCard { HasMap: true });
                var data = (AchievementCategoryCard)card.DataContext!;
                var map = card.GetVisualDescendants().OfType<Ft8GlobeControl>().Single(g => g.IsEffectivelyVisible);
                var state = F(width) + " " + data.Title;

                List<Popup> Open() => window.GetVisualDescendants().OfType<Popup>().Where(p => p.IsOpen).ToList();

                // **CLOSED AT START.**
                Assert.Empty(Open());

                var centre = map.TranslatePoint(new Point(map.Bounds.Width / 2, map.Bounds.Height / 2), window);

                Assert.True(centre.HasValue, state + ": the map has no place on the window");

                // **A HOVER OPENS NOTHING.**
                window.MouseMove(centre!.Value);
                Settle(window);

                Assert.True(Open().Count == 0, state + ": a hover over the map opened a popup");

                // **A CLICK OPENS IT, HOLDING THIS CARD'S PATH.**
                window.MouseDown(centre.Value, MouseButton.Left);
                window.MouseUp(centre.Value, MouseButton.Left);
                Settle(window);

                var open = Open();

                Assert.True(open.Count == 1, state + ": a click on the map opened " + open.Count + " popups");

                var popup = open[0];
                var opened = popup.Child!.GetVisualDescendants().OfType<Ft8GlobeControl>().Single();

                _output.WriteLine(
                    state + ": opened " + opened.Plot?.Callsign + ", map " + F(opened.Bounds.Width) + " x " + F(opened.Bounds.Height)
                    + ", frame " + F(opened.DrawnFrame.Width) + " by " + F(opened.DrawnFrame.Height)
                    + " (the card's " + F(map.DrawnFrame.Width) + " by " + F(map.DrawnFrame.Height) + ")");

                Assert.Same(data.Globe, opened.Plot);
                Assert.True(opened.Opened && !opened.FillsBox, state + ": the popup's map is not the conversation card's opened map");

                // **IT NEVER COVERS THE BACK CONTROL.**
                var back = Named<Button>(window, "AchievementsBack");
                var backBottom = back.PointToScreen(new Point(0, back.Bounds.Height)).Y;
                var popupTop = popup.Child!.PointToScreen(new Point(0, 0)).Y;

                _output.WriteLine(state + ": back control's bottom at screen y " + backBottom + ", popup's top at " + popupTop);

                Assert.True(popupTop > backBottom, state + ": the popup's top at " + popupTop + " covers the back control down to " + backBottom);

                // **A CLICK OUTSIDE CLOSES IT**, in the window's empty margin above the band, halfway
                // across. The first run clicked the top right corner: it closed the popup at 1400
                // and not at 1920, so what that corner hits is printed.
                var corner = new Point(window.Bounds.Width - 4, 4);
                var outside = new Point(window.Bounds.Width / 2, 6);

                _output.WriteLine(
                    state + ": client " + F(window.ClientSize.Width) + " x " + F(window.ClientSize.Height)
                    + "; the top right corner hits " + (window.InputHitTest(corner)?.GetType().Name ?? "nothing")
                    + "; the click outside at " + F(outside.X) + ", " + F(outside.Y) + " hits "
                    + (window.InputHitTest(outside)?.GetType().Name ?? "nothing")
                    + "; window active " + window.IsActive + "; popup host " + (popup.Host?.GetType().Name ?? "none"));

                // **THE POINTER MOVES THERE FIRST**, as a mouse does before it clicks.
                window.MouseMove(outside);
                window.MouseDown(outside, MouseButton.Left);
                window.MouseUp(outside, MouseButton.Left);
                Settle(window);

                Assert.True(Open().Count == 0, state + ": a click outside left the popup open");

                // **AND NOTHING WAS WRITTEN FOR THE MAP**: the category's own opening, and no more.
                _output.WriteLine(state + ": written " + string.Join(", ", sink.Names));

                Assert.Equal(new[] { "achievement_category_opened" }, sink.Names);
            }
            finally
            {
                window.Close();
            }
        }
    }

    /// <summary>A sink that keeps the names of the events written.</summary>
    private sealed class Written : ITelemetry
    {
        public List<string> Names { get; } = new();

        public long DroppedEventCount => 0;

        public void Write(
            TelemetryCategory category,
            string eventName,
            IReadOnlyDictionary<string, object?>? data = null,
            TelemetryLevel level = TelemetryLevel.Info)
            => Names.Add(eventName);
    }

    /// <summary>
    /// **How tall the earned card's map is**, and the no-map list with it (work instruction 342
    /// ruling 11, the unit's own number): 231 px, the height at which every fixture card's frame
    /// takes the card's shape inside the file at 1920 as well as at 1400.
    /// </summary>
    private const double CardMapHeight = 231;

    /// <summary>
    /// **Ruling 12's crop, as a number**: null where both stations lie inside the frame and the frame
    /// is no larger than the crop bound; otherwise what is wrong.
    /// </summary>
    /// <remarks>
    /// <para>**THE BOUND.** The box holding every sampled point of the path and both markers,
    /// widened each way by the popup rule's margin - `Ft8GlobePlot.MarginShare`, 6 per cent of that
    /// box's longer side - then each side grown to the popup rule's zoom floor
    /// (`ZoomFloorShare`, a quarter of the file) and to the card box over `ZoomCap`, neither past
    /// the file; then the other side grown to the card's own shape, neither past the file.</para>
    /// <para>**A WHOLE-GLOBE FRAME FAILS IT** on every card the fixtures draw, and the test says
    /// so card by card.</para>
    /// </remarks>
    private static string? OutsideTheCrop(
        Ft8GlobePlot plot, (double Left, double Top, double Width, double Height) frame, double boxWidth, double boxHeight)
    {
        bool Inside(double x, double y)
            => x >= frame.Left - 0.5 && x <= frame.Left + frame.Width + 0.5
            && y >= frame.Top - 0.5 && y <= frame.Top + frame.Height + 0.5;

        if (!Inside(plot.OperatorX, plot.OperatorY) || !Inside(plot.StationX, plot.StationY))
        {
            return "a station lies outside the frame left " + F(frame.Left) + " top " + F(frame.Top) + " " + F(frame.Width)
                + " by " + F(frame.Height) + ": you at " + F(plot.OperatorX) + ", " + F(plot.OperatorY)
                + ", him at " + F(plot.StationX) + ", " + F(plot.StationY);
        }

        double fileWidth = Ft8GlobePlot.Map.WidthPixels;
        double fileHeight = Ft8GlobePlot.Map.HeightPixels;
        var xs = plot.Path.SelectMany(r => r.Select(p => p.X)).Append(plot.OperatorX).Append(plot.StationX).ToList();
        var ys = plot.Path.SelectMany(r => r.Select(p => p.Y)).Append(plot.OperatorY).Append(plot.StationY).ToList();
        var pathWidth = xs.Max() - xs.Min();
        var pathHeight = ys.Max() - ys.Min();
        var margin = Ft8GlobePlot.MarginShare * Math.Max(pathWidth, pathHeight);
        var wide = Math.Min(fileWidth, Math.Max(Math.Max(pathWidth + (2 * margin), fileWidth * Ft8GlobePlot.ZoomFloorShare), boxWidth / Ft8GlobePlot.ZoomCap));
        var tall = Math.Min(fileHeight, Math.Max(Math.Max(pathHeight + (2 * margin), fileHeight * Ft8GlobePlot.ZoomFloorShare), boxHeight / Ft8GlobePlot.ZoomCap));
        var aspect = boxWidth / boxHeight;
        var mostWide = Math.Min(fileWidth, Math.Max(wide, tall * aspect));
        var mostTall = Math.Min(fileHeight, Math.Max(tall, wide / aspect));

        return frame.Width <= mostWide + 0.5 && frame.Height <= mostTall + 0.5
            ? null
            : "the frame " + F(frame.Width) + " by " + F(frame.Height) + " is larger than the crop bound " + F(mostWide) + " by "
                + F(mostTall) + " (the path " + F(pathWidth) + " by " + F(pathHeight) + ", margin " + F(margin) + ")";
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

        // **WORK INSTRUCTION 342 RULING 14: THE PICTURE'S SENTENCE ONLY WHERE THE LIST DRAWS IT.** A
        // decoded row is marked by its sender's country (`NudgeSet.WouldOpen`): the still green
        // quill for an unworked country on a continent the log has reached, the ringed amber one
        // where the continent is new too, and nothing for a worked country.
        _output.WriteLine("  quiet countries quill [" + none.QuillLine + "]");

        // **NOBODY LISTED, AND TWO CONTINENTS STILL UNWORKED**, so a caller could carry either.
        Assert.Equal(5, log.Continents.Count);
        Assert.Equal("On the CQ list they carry a quill.", none.QuillLine);

        void Quill(string kind, string? inside, string expected)
        {
            screen.OpenCategoryCommand.Execute(kind);

            if (inside is not null)
            {
                screen.OpenCategoryCommand.Execute(inside);
            }

            var next = screen.Category!.Cards[^1];

            _output.WriteLine("  " + (inside ?? kind) + " quill [" + next.QuillLine + "]");

            Assert.False(next.Earned);
            Assert.Equal(expected, next.QuillLine);

            while (screen.Category is not null)
            {
                screen.BackCommand.Execute(null);
            }
        }

        // **COUNTRIES**: Austria and Grenada, both on continents already reached - green.
        Assert.All(country.Callers, c => Assert.False(c.OpensContinent, c.Place + " opens a continent"));
        Quill(AchievementKinds.Countries, null, "On the CQ list they carry the green quill.");

        // **EUROPE, REACHED**: every unworked country there is green.
        Quill(AchievementKinds.Continents, "continent-EU", "On the CQ list they carry the green quill.");

        // **GRIDS AND STATES: THE LIST MARKS A COUNTRY, NOT A SQUARE OR A STATE**, so no sentence.
        Quill(AchievementKinds.Grids, null, "");
        Quill(AchievementKinds.States, null, "");

        // **A LIST WITH A CALLER WHO WOULD OPEN A CONTINENT**: his row is ringed and the others are
        // green, so the sentence says a quill and each caller's own mark says which.
        var mixed = new AchievementsViewModel(records, MyGrid, points) { Calling = Calling() };

        mixed.OpenCategoryCommand.Execute(AchievementKinds.Countries);

        var either = mixed.Category!.Cards[^1];

        Print(either);

        Assert.Contains(either.Callers, c => c.OpensContinent);
        Assert.Contains(either.Callers, c => !c.OpensContinent);
        Assert.Equal("On the CQ list they carry a quill.", either.QuillLine);

        mixed.BackCommand.Execute(null);

        // **OCEANIA, NEVER REACHED: EVERY CALLER ON IT OPENS IT**, so the ringed quill - on its card
        // among the seven and on its own page of countries.
        mixed.OpenCategoryCommand.Execute(AchievementKinds.Continents);

        var oceania = mixed.Category!.SubBadges.Single(b => b.Kind == "continent-OC").Card!;

        Assert.False(oceania.Earned);
        Assert.Equal("On the CQ list they carry the ringed quill.", oceania.QuillLine);

        mixed.OpenCategoryCommand.Execute("continent-OC");

        Assert.Equal("On the CQ list they carry the ringed quill.", mixed.Category!.Cards[^1].QuillLine);

        // **AND DRAWN ON THE CARD AT 1400, UNDER THE WANTS LINE.**
        var quillWindow = Realized(records, 1400, calling);

        try
        {
            ((AchievementsViewModel)quillWindow.DataContext!).OpenCategoryCommand.Execute(AchievementKinds.Countries);
            Settle(quillWindow);

            var drawn = Named<ItemsControl>(quillWindow, "AchievementsCategoryCards").GetVisualDescendants().OfType<TextBlock>()
                .Where(t => t.IsEffectivelyVisible)
                .ToList();
            var wants = drawn.Single(t => t.Text == "Any country you have not worked");
            var quill = drawn.Single(t => t.Text == "On the CQ list they carry the green quill.");

            Assert.True(
                Top(quill, quillWindow) > Top(wants, quillWindow),
                "the quill line is at y " + F(Top(quill, quillWindow)) + ", not under the wants line at " + F(Top(wants, quillWindow)));
        }
        finally
        {
            quillWindow.Close();
        }

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

        // **WORK INSTRUCTION 345 TASK 1, RULING 17: EVERY NEXT CARD AS DRAWN AT 1400 AND 1920**, on
        // every kind, the Continents page and every continent page, over the four callers and the best
        // bet the page-wide test uses. **Modes on the five-contact log as well**, because the twelve
        // contacts have worked every mode and draw no next card there.
        var bet = new BandBet("17 m", "best bet now");
        var watchedNext = false;

        foreach (var width in new[] { 1400.0, 1920.0 })
        {
            foreach (var (fixture, kinds) in new[]
            {
                (records, AchievementKinds.All.Concat(ContinentKinds()).ToList()),
                (FiveContacts(), new List<string> { AchievementKinds.Modes }),
            })
            {
                var wide = Realized(fixture, width, Calling(), bet);
                var opened = (AchievementsViewModel)wide.DataContext!;

                try
                {
                    foreach (var kind in kinds)
                    {
                        OpenOnWindow(wide, opened, kind);

                        var category = opened.Category!;
                        var held = category.Cards.Concat(category.SubBadges.Select(b => b.Card!)).Where(c => !c.Earned).ToList();
                        var drawn = wide.GetVisualDescendants().OfType<Border>()
                            .Where(b => b.IsEffectivelyVisible && b.Classes.Contains("trading-card")
                                && b.DataContext is AchievementCategoryCard { Earned: false })
                            .ToList();
                        var where = F(width) + " " + kind;
                        var fromTheCqList = kind == AchievementKinds.Countries || kind == AchievementKinds.Grids
                            || kind == AchievementKinds.Continents || kind.StartsWith(AchievementCategory.ContinentPrefix, StringComparison.Ordinal);

                        Assert.True(
                            drawn.Count == held.Count,
                            where + ": the view model holds " + held.Count + " next card(s) and the page draws " + drawn.Count);

                        foreach (var next in held)
                        {
                            var card = drawn.Single(b => ReferenceEquals(b.DataContext, next));
                            var said = VisibleText(card).ToList();
                            var rows = CallerRows(card);
                            var miss = NextCardMiss(card, next);

                            _output.WriteLine(where + " [" + next.Title + "] " + (miss ?? "drawn: " + string.Join(" | ", said)));

                            Assert.True(miss is null, where + ": " + miss);

                            if (!watchedNext && next.Callers.Count > 0)
                            {
                                // **RULING 19, WATCHED RED ON THE TEST WINDOW ONLY**: the drawn card held
                                // against a caller line the view model does not hold.
                                var wrong = NextCardMiss(
                                    card, next with { Callers = next.Callers.Append(new NextCaller("Nowhere", "XX0XX · 1 mi")).ToList() });

                                _output.WriteLine(where + " [" + next.Title + "] with a caller the view model does not hold, watched red: " + wrong);

                                Assert.NotNull(wrong);
                                watchedNext = true;
                            }

                            // **FROM THE CQ LIST: THE READ TIME, AND A DISTANCE ON EVERY CALLER, OR NO ONE.**
                            if (fromTheCqList)
                            {
                                Assert.Contains("calling CQ at 21:41 UTC, unworked", said);
                                Assert.All(rows, r => Assert.Matches(@" · [\d,]+ mi$", r.CallLine));

                                if (rows.Count == 0)
                                {
                                    Assert.Contains("no one is calling from there now", said);
                                }
                            }

                            // **GRIDS AND STATES: THE LIST MARKS NO SQUARE OR STATE, SO NO QUILL IS DRAWN**;
                            // States says in words that a caller carries no state.
                            if (kind == AchievementKinds.Grids || kind == AchievementKinds.States)
                            {
                                Assert.DoesNotContain(
                                    card.GetVisualDescendants().OfType<TextBlock>(),
                                    t => t.IsEffectivelyVisible && t.Classes.Contains("card-quill"));
                            }

                            if (kind == AchievementKinds.States)
                            {
                                Assert.Contains(AchievementCategory.NoStateFromTheAir, said);
                            }

                            // **BANDS: THE BEST BET IS THE FIRST ROW DRAWN.**
                            if (kind == AchievementKinds.Bands)
                            {
                                Assert.Equal(new NextCaller("17 m", "best bet now"), rows[0]);
                            }

                            // **MODES: WHERE EACH UNWORKED MODE LIVES, DRAWN.**
                            if (kind == AchievementKinds.Modes)
                            {
                                Assert.Equal(new[] { "CW", "FT4", "PSK31" }, rows.Select(r => r.Place).OrderBy(p => p, StringComparer.Ordinal));
                            }

                            // **HALL OF FAME: THE NEXT FIRST, WITH ITS BAR DRAWN.**
                            if (kind == AchievementKinds.HallOfFame)
                            {
                                Assert.Contains("Over 10,000 miles", said);
                                Assert.Contains(
                                    card.GetVisualDescendants().OfType<BadgeProgressControl>(),
                                    b => b.IsEffectivelyVisible && b.Bounds.Width > 0);
                            }
                        }

                        ToThePage(wide, opened);
                    }
                }
                finally
                {
                    wide.Close();
                }
            }
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

        // **WORK INSTRUCTION 345 TASK 2, RULING 17: THE SAME FIVE KINDS AS DRAWN AT 1400 AND 1920**, on
        // the same fixtures. The view-model half above stays; this is what criterion 4 is counted on.
        var watched = false;

        foreach (var width in new[] { 1400.0, 1920.0 })
        {
            var where = F(width) + " ";

            void Holds(string what, string? miss)
            {
                _output.WriteLine(where + what + ": " + (miss ?? "drawn"));

                Assert.True(miss is null, where + what + ": " + miss);
            }

            var window = Realized(records, width, Calling(), bet);
            var shown = (AchievementsViewModel)window.DataContext!;

            try
            {
                // **CONTINENTS: SEVEN DRAWN; AN OPENED ONE ITS FIRST CONTACT AND ITS COUNTRIES, AN
                // UNOPENED ONE ITS NAME AND WHO IS CALLING FROM IT.**
                OpenOnWindow(window, shown, AchievementKinds.Continents);

                var badges = Named<ItemsControl>(window, "AchievementsSubBadges").GetVisualDescendants().OfType<Button>()
                    .Where(b => b.IsEffectivelyVisible && b.Classes.Contains("hm-badge"))
                    .ToList();

                Assert.True(badges.Count == 7, where + "Continents draws " + badges.Count + " badges");

                foreach (var badge in badges)
                {
                    var code = ((string)badge.CommandParameter!)[AchievementCategory.ContinentPrefix.Length..];
                    var card = badge.GetVisualDescendants().OfType<Border>().Single(b => b.IsEffectivelyVisible && b.Classes.Contains("trading-card"));

                    if (log.Continents.Contains(code, StringComparer.OrdinalIgnoreCase))
                    {
                        var worked = log.EntitiesOn(code);

                        Holds(
                            "continents " + code,
                            CardMiss(card, DxccContinents.NameOf(code), EarliestOf(log.OnContinent(code)).Callsign, worked + (worked == 1 ? " country" : " countries") + " worked there"));
                    }
                    else
                    {
                        Holds(
                            "continents " + code,
                            CardMiss(card, DxccContinents.NameOf(code), "calling CQ at 21:41 UTC, unworked"));
                    }
                }

                var oceania = badges
                    .Select(b => b.GetVisualDescendants().OfType<Border>().Single(c => c.IsEffectivelyVisible && c.Classes.Contains("trading-card")))
                    .Single(c => VisibleText(c).Contains(zealand));

                Assert.Contains(CallerRows(oceania), r => r.CallLine.StartsWith("ZL1ABC", StringComparison.Ordinal));

                ToThePage(window, shown);

                // **TOTAL MILES: THE TIER BAR, DRAWN WITH WIDTH, AND ITS LINE.**
                OpenOnWindow(window, shown, AchievementKinds.TotalMiles);

                var tierCard = TradingCards(window).Single();

                Holds(
                    "total_miles tier",
                    CardMiss(tierCard, reached.ToString("#,0", CultureInfo.InvariantCulture) + " of 50,000 mi") ?? (HasBar(tierCard) ? null : "no tier bar with width"));

                ToThePage(window, shown);

                // **BANDS: 20 M'S FIRST CONTACT WITH A MAP, AND THE BEST BET FIRST ON THE NEXT CARD.**
                OpenOnWindow(window, shown, AchievementKinds.Bands);

                var bandCards = TradingCards(window);
                var twentyCard = bandCards.Single(b => b.DataContext is AchievementCategoryCard { Earned: true } c && c.Title == AdifLog.BandDisplayNameFor("20m"));

                Holds(
                    "bands 20 m",
                    CardMiss(twentyCard, AdifLog.BandDisplayNameFor("20m"), EarliestOf(log.OnBand("20m")).Callsign) ?? (HasMap(twentyCard) ? null : "no map with width"));

                Assert.Equal(
                    new NextCaller("17 m", "best bet now"),
                    CallerRows(bandCards.Single(b => b.DataContext is AchievementCategoryCard { Earned: false }))[0]);

                ToThePage(window, shown);

                // **HALL OF FAME: EACH EARNED FIRST WITH ITS CALLSIGN AND A MAP; OVER 10,000 MILES
                // WITH ITS BAR.**
                OpenOnWindow(window, shown, AchievementKinds.HallOfFame);

                var firstCallsigns = new Dictionary<string, string>
                {
                    ["Your first contact"] = EarliestOf(log.Contacts).Callsign,
                    ["A DX contact"] = EarliestOf(log.Contacts.Where(c => c.Entity is not null && c.Entity != mine)).Callsign,
                    ["A PSK31 contact"] = "DL1ABC",
                    ["A Morse contact"] = "VA3VRR",
                    ["Over 5,000 miles"] = EarliestOf(log.Contacts.Where(c => c.Miles >= 5000)).Callsign,
                };
                var firstCards = TradingCards(window);
                var earnedFirsts = firstCards.Where(b => b.DataContext is AchievementCategoryCard { Earned: true }).ToList();

                Assert.Equal(
                    firstCallsigns.Keys.OrderBy(k => k, StringComparer.Ordinal),
                    earnedFirsts.Select(b => ((AchievementCategoryCard)b.DataContext!).Title).OrderBy(k => k, StringComparer.Ordinal));

                foreach (var card in earnedFirsts)
                {
                    var title = ((AchievementCategoryCard)card.DataContext!).Title;

                    Holds("hall_of_fame " + title, CardMiss(card, title, firstCallsigns[title]) ?? (HasMap(card) ? null : "no map with width"));

                    if (!watched)
                    {
                        // **RULING 19, WATCHED RED ON THE TEST WINDOW ONLY**: this first held against
                        // the callsign of a first it is not.
                        var other = firstCallsigns.First(f => f.Key != title && f.Value != firstCallsigns[title]);
                        var wrong = CardMiss(card, title, other.Value);

                        _output.WriteLine(where + "hall_of_fame " + title + " held against " + other.Key + "'s " + other.Value + ", watched red: " + wrong);

                        Assert.NotNull(wrong);
                        watched = true;
                    }
                }

                var tenThousand = firstCards.Single(b => b.DataContext is AchievementCategoryCard { Earned: false });

                Holds("hall_of_fame next", CardMiss(tenThousand, "Over 10,000 miles") ?? (HasBar(tenThousand) ? null : "no bar with width"));
            }
            finally
            {
                window.Close();
            }

            // **TOTAL MILES, REACHED: THE CROSSING CARD WITH ITS DATE AND A MAP.**
            var farWindow = Realized(nine, width);

            try
            {
                OpenOnWindow(farWindow, (AchievementsViewModel)farWindow.DataContext!, AchievementKinds.TotalMiles);

                var crossingCard = TradingCards(farWindow).First(b => b.DataContext is AchievementCategoryCard { Earned: true });

                Holds(
                    "total_miles crossed",
                    CardMiss(crossingCard, "50,000 miles", crossing.StartedUtc!.Value.ToString("MMM d, yyyy", CultureInfo.InvariantCulture))
                        ?? (HasMap(crossingCard) ? null : "no map with width"));
            }
            finally
            {
                farWindow.Close();
            }

            // **MODES: FT8 WITH LA1ZZZ, AND WHERE CW, FT4 AND PSK31 LIVE.**
            var fewWindow = Realized(FiveContacts(), width, Calling(), bet);

            try
            {
                OpenOnWindow(fewWindow, (AchievementsViewModel)fewWindow.DataContext!, AchievementKinds.Modes);

                var modeCards = TradingCards(fewWindow);
                var ft8 = modeCards.Single(b => b.DataContext is AchievementCategoryCard { Earned: true } c && c.Title == "FT8");
                var rows = CallerRows(modeCards.Single(b => b.DataContext is AchievementCategoryCard { Earned: false }));

                Holds("modes FT8", CardMiss(ft8, "FT8", "LA1ZZZ"));

                Assert.Equal(new[] { "CW", "FT4", "PSK31" }, rows.Select(r => r.Place).OrderBy(p => p, StringComparer.Ordinal));
                Assert.StartsWith(
                    (block.JumpHz / 1_000_000.0).ToString("0.000", CultureInfo.InvariantCulture) + " on " + on,
                    rows.Single(r => r.Place == "PSK31").CallLine,
                    StringComparison.Ordinal);
            }
            finally
            {
                fewWindow.Close();
            }
        }
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

                    var page = window.GetVisualDescendants().OfType<ItemsControl>()
                        .First(i => i.IsEffectivelyVisible && (i.Name == "AchievementsCategoryCards" || i.Name == "AchievementsSubBadges"));

                    _output.WriteLine(
                        "  " + kind.PadRight(14) + runs + " runs fit, " + cards.Count + " cards, " + white + " white, page "
                        + F(page.Bounds.Height) + " px tall");

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
    /// **Work instruction 342 task 0: the Countries page against
    /// `assets/category-page-countries.png`, before a card is changed.** It asserts nothing and
    /// presses nothing.
    /// </summary>
    /// <remarks>
    /// **EVERY OPTION IS SET ON THE TEST WINDOW ONLY**, never in markup, and every number is
    /// computed on the headless host, not seen.
    /// </remarks>
    [AvaloniaFact]
    public void Unit342TraceTheCountriesPageAgainstItsMockup()
    {
        var bet = new BandBet("17 m", "best bet now");
        var twelve = TheAchievementsPageTests.TwelveContacts();
        var log = new AchievementLog(twelve, MyGrid);
        var nudges = NudgeSet.From(log);
        var calling = Calling();

        _output.WriteLine("THE CROP RULE, READ FROM THE SOURCE");
        _output.WriteLine("  the card's globe is Opened, so Ft8GlobeControl.FrameOf(plot, true, offer) -> Ft8GlobePlot.OpenFrameFor(offer w, offer h)");
        _output.WriteLine("  Ft8GlobePlot.Opened(): every path sample and both markers, widened each side by MarginShare "
            + F(Ft8GlobePlot.MarginShare) + " of the longer side; Fitted: each side grown to ZoomFloorShare "
            + F(Ft8GlobePlot.ZoomFloorShare) + " of the file about its center, clamped into the file; the whole file where Path.Count != 1");
        _output.WriteLine("  OpenFrameFor: each side grown to at least offer / ZoomCap " + F(Ft8GlobePlot.ZoomCap)
            + " about its center, then Fitted again");
        _output.WriteLine("  file " + Ft8GlobePlot.Map.WidthPixels + " x " + Ft8GlobePlot.Map.HeightPixels
            + "; MeasureOverride asks for the frame's own shape inside the offer, so the control is only as wide as the frame's aspect allows");
        _output.WriteLine("THE LOG: continents " + string.Join(", ", log.Continents) + "; entities " + string.Join(", ", log.Entities));

        _output.WriteLine("THE MARK A CALLER CARRIES ON THE DECODED LIST (MainWindowViewModel.MarkIfItOpensSomething: NudgeSet.Explain on the row's sender; DigitalDecodeRow.NudgeForm)");

        foreach (var call in calling.Calls.Concat(new[] { new CqCall("LA8ENA", "JO59", "214100"), new CqCall("W1AW", "FN31", "214100") }))
        {
            var (kind, _) = nudges.WouldOpen(call.Callsign);

            _output.WriteLine(
                "  " + call.Callsign.PadRight(8) + call.Grid.PadRight(6) + (DxccPrefixes.EntityOf(call.Callsign) ?? "no entity").PadRight(28)
                + kind + " -> " + (kind == NudgeKind.Visible ? "the still green quill (Counter)"
                    : kind == NudgeKind.Door ? "the ringed amber quill (Door)" : "no mark"));
        }

        var model = Screen(twelve, calling, bet);
        var kinds = AchievementKinds.All
            .Concat(DxccContinents.Codes.Keys.OrderBy(k => k, StringComparer.Ordinal).Select(k => AchievementCategory.ContinentPrefix + k))
            .ToList();

        _output.WriteLine("THE BAND LINE AND THE NEXT CARD, EVERY KIND");

        foreach (var kind in kinds)
        {
            if (kind.StartsWith(AchievementCategory.ContinentPrefix, StringComparison.Ordinal))
            {
                model.OpenCategoryCommand.Execute(AchievementKinds.Continents);
            }

            model.OpenCategoryCommand.Execute(kind);

            var category = model.Category!;

            _output.WriteLine(
                "  " + kind.PadRight(14) + "band [" + category.BandLine + "] gap [" + category.GapLine + "] bar " + category.HasLevelBar
                + ", gap clause on the line " + (category.GapLine.Length > 0 && category.BandLine.Contains(category.GapLine, StringComparison.Ordinal))
                + ", back [" + category.BackLabel + "]");

            var nexts = category.Cards.Where(c => !c.Earned)
                .Concat(category.SubBadges.Select(b => b.Card).Where(c => c is not null && !c.Earned).Select(c => c!));

            foreach (var next in nexts)
            {
                _output.WriteLine(
                    "      next [" + next.Title + "] wants [" + next.WantsLine + "] heading [" + next.CallersHeading + "] callers "
                    + string.Join(" / ", next.Callers.Select(c => c.Place + " " + c.CallLine + (c.OpensContinent ? " (door)" : " (counter)")))
                    + (next.NoCallerLine.Length > 0 ? " [" + next.NoCallerLine + "]" : ""));
            }

            while (model.Category is not null)
            {
                model.BackCommand.Execute(null);
            }
        }

        foreach (var width in new[] { 1040.0, 1400.0, 1920.0 })
        {
            Unit342TraceWindow(twelve, "twelve contacts", width, calling, bet, kinds);
            Unit342TraceWindow(FiveContacts(), "five contacts", width, calling, bet, new[] { AchievementKinds.Countries });
        }

        _output.WriteLine("THE CONVERSATION CARD'S MAP POPUP, READ FROM THE SOURCE");
        _output.WriteLine("  Ft8ContactCard.MapIsOpen, an observable property; OpenTheMapCommand sets it where MapOpens (ShowsGlobe and Globe.Opens); CloseTheMapCommand clears it");
        _output.WriteLine("  MainWindow.axaml: a transparent Button around the card's Ft8GlobeControl (CardGlobe, Height 120) runs OpenTheMapCommand - a click, never a hover");
        _output.WriteLine("  the Popup: IsOpen two-way on MapIsOpen, Placement Center on the window, IsLightDismissEnabled, a dismiss X, the same control Opened with MaxWidth 720 and MaxHeight 400");
        _output.WriteLine("  telemetry: OpenTheMap and CloseTheMap write nothing, so opening a map is a view and not a stage");
        _output.WriteLine("  reuse: the mechanism (a Button over the globe, a bool on the card, a light-dismiss Popup) can be copied; the property and commands live on Ft8ContactCard, not on AchievementCategoryCard");
    }

    /// <summary>Work instruction 342 task 0's window half: one fixture at one width.</summary>
    private void Unit342TraceWindow(
        IReadOnlyList<AdifLogRecord> records, string label, double width, CqSnapshot calling, BandBet bet, IReadOnlyList<string> kinds)
    {
        const double Picture = 596.0 / 195.0;
        var window = Realized(records, width, calling, bet);
        var screen = (AchievementsViewModel)window.DataContext!;

        try
        {
            _output.WriteLine("WINDOW " + F(window.Bounds.Width) + " x " + F(window.Bounds.Height) + ", " + label);

            foreach (var kind in kinds)
            {
                if (kind.StartsWith(AchievementCategory.ContinentPrefix, StringComparison.Ordinal))
                {
                    screen.OpenCategoryCommand.Execute(AchievementKinds.Continents);
                }

                screen.OpenCategoryCommand.Execute(kind);
                Settle(window);

                var category = screen.Category!;
                var back = Named<Button>(window, "AchievementsBack");
                var bandLine = Named<TextBlock>(window, "AchievementsCategoryBandLine");
                var withGap = category.BandLine + " · " + category.GapLine;

                _output.WriteLine(
                    "  " + kind + ": back [" + back.Content + "] " + F(back.Bounds.Width) + " x " + F(back.Bounds.Height)
                    + "; band line slot " + F(bandLine.Bounds.Width) + ", needs " + F(Unit342Needs(bandLine, category.BandLine))
                    + (category.HasLevelBar && category.GapLine.Length > 0 ? ", with the gap clause needs " + F(Unit342Needs(bandLine, withGap)) : ""));

                foreach (var wants in window.GetVisualDescendants().OfType<TextBlock>()
                    .Where(t => t.IsEffectivelyVisible && t.Classes.Contains("card-wants")))
                {
                    var said = wants.Text ?? "";

                    _output.WriteLine(
                        "    wants [" + said + "] slot " + F(wants.Bounds.Width) + ", needs " + F(Unit342Needs(wants, said))
                        + "; + green quill " + F(Unit342Needs(wants, said + ". On the CQ list they carry the green quill."))
                        + "; + ringed quill " + F(Unit342Needs(wants, said + ". On the CQ list they carry the ringed quill."))
                        + "; + a quill " + F(Unit342Needs(wants, said + ". On the CQ list they carry a quill.")));
                }

                var list = window.GetVisualDescendants().OfType<ItemsControl>()
                    .First(i => i.IsEffectivelyVisible && (i.Name == "AchievementsCategoryCards" || i.Name == "AchievementsSubBadges"));
                var cards = list.GetVisualDescendants().OfType<Border>()
                    .Where(b => b.IsEffectivelyVisible && b.Classes.Contains("trading-card"))
                    .ToList();
                var inner = cards.Select(c => Unit342Column(c).Bounds.Width).DefaultIfEmpty(0).Max();

                if (kind == AchievementKinds.Countries)
                {
                    foreach (var card in cards)
                    {
                        Unit342TraceCard(card);
                    }
                }

                foreach (var height in new[] { 170.0, Math.Round((170 + (inner / Picture)) / 2), Math.Round(inner / Picture) })
                {
                    foreach (var globe in list.GetVisualDescendants().OfType<Ft8GlobeControl>())
                    {
                        globe.Height = height;

                        if (globe.GetVisualParent() is Border frame)
                        {
                            frame.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
                        }
                    }

                    foreach (var noMap in list.GetVisualDescendants().OfType<Border>()
                        .Where(b => b.Classes.Contains("card-list") && !double.IsNaN(b.Height)))
                    {
                        noMap.Height = height;
                    }

                    Settle(window);

                    string fits;

                    try
                    {
                        fits = "fits, " + Fits(window, F(width) + " " + kind) + " runs";
                    }
                    catch (Xunit.Sdk.XunitException e)
                    {
                        fits = "CLIPS: " + e.Message.Split('\n')[0];
                    }

                    var drawn = list.GetVisualDescendants().OfType<Ft8GlobeControl>()
                        .Where(g => g.IsEffectivelyVisible && g.Plot is not null)
                        .Select(g => Unit342Drawn(g))
                        .ToList();

                    _output.WriteLine(
                        "    height " + F(height) + " (inner " + F(inner) + ", " + F(inner / height) + ":1): cards "
                        + string.Join(", ", cards.Select(c => F(c.Bounds.Height)).Distinct())
                        + "; page " + F(list.Bounds.Height) + " in a viewport " + F(list.GetVisualParent() is Control v ? v.Bounds.Height : 0)
                        + "; maps drawn " + string.Join(", ", drawn.Select(d => F(d.Width) + " x " + F(d.Height)).Distinct())
                        + "; " + fits);

                    if (kind == AchievementKinds.Countries)
                    {
                        foreach (var globe in list.GetVisualDescendants().OfType<Ft8GlobeControl>().Where(g => g.IsEffectivelyVisible && g.Plot is not null))
                        {
                            var plot = globe.Plot!;
                            var (boundWidth, boundHeight) = Unit342Bound(plot, inner, height);

                            _output.WriteLine(
                                "      " + plot.Callsign.PadRight(8) + "a frame filled to " + F(inner) + " x " + F(height)
                                + " is bounded by " + F(boundWidth) + " x " + F(boundHeight)
                                + (plot.Path.Count != 1 ? " (the date line: the whole file)" : "")
                                + ", fills the card " + (Math.Abs((boundWidth / boundHeight) - (inner / height)) < 0.01)
                                + ", a whole-globe frame fails it "
                                + (Ft8GlobePlot.Map.WidthPixels > boundWidth + 0.5 || Ft8GlobePlot.Map.HeightPixels > boundHeight + 0.5));
                        }
                    }
                }

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

    private void Unit342TraceCard(Border card)
    {
        var data = (AchievementCategoryCard)card.DataContext!;
        var column = Unit342Column(card);
        var said = "    [" + data.Title + "] card " + F(card.Bounds.Width) + " x " + F(card.Bounds.Height) + ", inner " + F(column.Bounds.Width);
        var distance = card.GetVisualDescendants().OfType<TextBlock>()
            .FirstOrDefault(t => t.IsEffectivelyVisible && t.Classes.Contains("card-distance"));

        if (card.GetVisualDescendants().OfType<Ft8GlobeControl>().FirstOrDefault(g => g.IsEffectivelyVisible) is { Plot: { } plot } globe)
        {
            var offer = Unit342Offer(globe);
            var frame = globe.DrawnFrame;
            var scale = Math.Min(globe.Bounds.Width / frame.Width, globe.Bounds.Height / frame.Height);
            var at = globe.TranslatePoint(new Point(0, 0), card) ?? default;
            var box = Unit342PathBox(plot);
            var drawn = Unit342Drawn(globe);

            string On(double x, double y)
                => "(" + F((x - frame.Left) * scale) + ", " + F((y - frame.Top) * scale) + ")";

            said += "; globe " + F(globe.Bounds.Width) + " x " + F(globe.Bounds.Height) + " at x " + F(at.X)
                + ", offered " + F(offer.Width) + " x " + F(offer.Height)
                + ", drawn " + F(drawn.Width) + " x " + F(drawn.Height)
                + "; you at " + On(plot.OperatorX, plot.OperatorY) + ", him at " + On(plot.StationX, plot.StationY)
                + "; frame " + Unit342Box(frame) + "; path box " + Unit342Box(box) + " (" + plot.Path.Count + " runs)";
        }
        else
        {
            said += "; no map [" + data.NoMapWord + "]";
        }

        _output.WriteLine(said + "; distance " + (distance is null ? "none" : F(distance.Bounds.Width) + " x " + F(distance.Bounds.Height) + " at " + F(distance.FontSize)));
    }

    private static StackPanel Unit342Column(Border card)
        => card.GetVisualDescendants().OfType<StackPanel>().First(s => Grid.GetColumn(s) == 1);

    private static Size Unit342Offer(Ft8GlobeControl globe)
        => (Size)typeof(Ft8GlobeControl)
            .GetField("_openedAgainst", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(globe)!;

    private static (double Width, double Height) Unit342Drawn(Ft8GlobeControl globe)
    {
        var frame = globe.DrawnFrame;
        var scale = Math.Min(globe.Bounds.Width / frame.Width, globe.Bounds.Height / frame.Height);

        return (frame.Width * scale, frame.Height * scale);
    }

    private static (double Left, double Top, double Width, double Height) Unit342PathBox(Ft8GlobePlot plot)
    {
        var xs = plot.Path.SelectMany(r => r.Select(p => p.X)).Append(plot.OperatorX).Append(plot.StationX).ToList();
        var ys = plot.Path.SelectMany(r => r.Select(p => p.Y)).Append(plot.OperatorY).Append(plot.StationY).ToList();

        return (xs.Min(), ys.Min(), xs.Max() - xs.Min(), ys.Max() - ys.Min());
    }

    /// <summary>
    /// The largest frame the rule could open for a card box, grown to the box's shape: the path
    /// box with its margin, the floor and the cap, then the other side to the box's aspect,
    /// neither past the file.
    /// </summary>
    private static (double Width, double Height) Unit342Bound(Ft8GlobePlot plot, double boxWidth, double boxHeight)
    {
        double fileWidth = Ft8GlobePlot.Map.WidthPixels;
        double fileHeight = Ft8GlobePlot.Map.HeightPixels;

        if (plot.Path.Count != 1)
        {
            return (fileWidth, fileHeight);
        }

        var box = Unit342PathBox(plot);
        var margin = Ft8GlobePlot.MarginShare * Math.Max(box.Width, box.Height);
        var wide = Math.Max(Math.Max(box.Width + (2 * margin), fileWidth * Ft8GlobePlot.ZoomFloorShare), boxWidth / Ft8GlobePlot.ZoomCap);
        var tall = Math.Max(Math.Max(box.Height + (2 * margin), fileHeight * Ft8GlobePlot.ZoomFloorShare), boxHeight / Ft8GlobePlot.ZoomCap);
        var aspect = boxWidth / boxHeight;

        return (Math.Min(fileWidth, Math.Max(wide, tall * aspect)), Math.Min(fileHeight, Math.Max(tall, wide / aspect)));
    }

    private static string Unit342Box((double Left, double Top, double Width, double Height) box)
        => "left " + F(box.Left) + ", top " + F(box.Top) + ", " + F(box.Width) + " by " + F(box.Height);

    private static double Unit342Needs(TextBlock text, string what)
        => new Avalonia.Media.TextFormatting.TextLayout(
            what,
            new Avalonia.Media.Typeface(text.FontFamily, text.FontStyle, text.FontWeight),
            text.FontSize,
            null).Width;

    /// <summary>
    /// **Work instruction 345 task 0: what step 1 asserts, and what the page draws, before a test is
    /// changed** - at 1400 and 1920, on every kind, the Continents page's seven and each continent's
    /// own page. It asserts nothing and presses nothing that transmits.
    /// </summary>
    /// <remarks>
    /// <para>**EVERY NUMBER IS COMPUTED ON THE HEADLESS HOST, NOT SEEN.** A fact the view model holds
    /// that no visible text on its band or card carries is printed `NOT DRAWN`.</para>
    /// <para>**THE FIXTURE IS `NoStringClipsAndNoCardIsWhiteAtFourteenHundredAndNineteenTwenty`'s**:
    /// twelve contacts, four callers and the best bet. Countries on the five-contact log and States on
    /// the state log are traced as well, because the twelve contacts carry no grid-less card, no
    /// date-less card and no `STATE`.</para>
    /// </remarks>
    [AvaloniaFact]
    public void Unit345TraceStepOneOnTheWindow()
    {
        var bet = new BandBet("17 m", "best bet now");
        var twelve = TheAchievementsPageTests.TwelveContacts();
        var log = new AchievementLog(twelve, MyGrid);
        var continents = DxccContinents.Codes.Keys.OrderBy(k => k, StringComparer.Ordinal)
            .Select(k => AchievementCategory.ContinentPrefix + k)
            .ToList();

        foreach (var width in new[] { 1400.0, 1920.0 })
        {
            var window = Realized(twelve, width, Calling(), bet);
            var screen = (AchievementsViewModel)window.DataContext!;
            var tallest = (Height: 0.0, Kind: "");

            try
            {
                _output.WriteLine("WINDOW " + F(window.Bounds.Width) + " x " + F(window.Bounds.Height) + ", twelve contacts");

                foreach (var kind in AchievementKinds.All.Concat(continents))
                {
                    var height = Unit345TracePage(window, screen, kind, width);

                    if (height > tallest.Height)
                    {
                        tallest = (height, kind);
                    }
                }

                // **THE SEVEN, DRAWN, AND WHAT EACH ONE OPENS.**
                screen.OpenCategoryCommand.Execute(AchievementKinds.Continents);
                Settle(window);

                var seven = Named<ItemsControl>(window, "AchievementsSubBadges").GetVisualDescendants().OfType<Button>()
                    .Where(b => b.IsEffectivelyVisible && b.Classes.Contains("hm-badge"))
                    .Select(b => (Kind: (string)b.CommandParameter!, Said: string.Join(" | ", VisibleText(b))))
                    .ToList();

                _output.WriteLine("  THE SEVEN AT " + F(width) + ": " + seven.Count + " sub-badges drawn");

                foreach (var (code, said) in seven)
                {
                    screen.OpenCategoryCommand.Execute(code);
                    Settle(window);

                    var drawn = Named<ItemsControl>(window, "AchievementsCategoryCards").GetVisualDescendants().OfType<Border>()
                        .Where(b => b.IsEffectivelyVisible && b.Classes.Contains("trading-card"))
                        .Select(b => (AchievementCategoryCard)b.DataContext!)
                        .ToList();

                    _output.WriteLine(
                        "    " + code + " badge [" + said + "] opens: name [" + Named<TextBlock>(window, "AchievementsCategoryName").Text
                        + "] back [" + Named<Button>(window, "AchievementsBack").Content + "] earned cards drawn "
                        + drawn.Count(c => c.Earned) + " of " + log.EntitiesOn(code[AchievementCategory.ContinentPrefix.Length..])
                        + " entities worked there; next cards drawn " + drawn.Count(c => !c.Earned)
                        + " [" + string.Join(" / ", drawn.Where(c => !c.Earned).Select(c => c.Title)) + "]");

                    screen.BackCommand.Execute(null);
                    Settle(window);
                }

                while (screen.Category is not null)
                {
                    screen.BackCommand.Execute(null);
                }

                Settle(window);
            }
            finally
            {
                window.Close();
            }

            _output.WriteLine("  TALLEST PAGE AT " + F(width) + ": " + tallest.Kind + ", " + F(tallest.Height) + " px");
        }

        // **THE CARDS THE TWELVE CONTACTS DO NOT HAVE**: no grid, no date, and a `STATE`.
        foreach (var width in new[] { 1400.0, 1920.0 })
        {
            foreach (var (records, label, kind) in new[]
            {
                (FiveContacts(), "five contacts", AchievementKinds.Countries),
                (StateContacts(), "state contacts", AchievementKinds.States),
            })
            {
                var window = Realized(records, width, Calling(), bet);

                try
                {
                    _output.WriteLine("WINDOW " + F(window.Bounds.Width) + " x " + F(window.Bounds.Height) + ", " + label);
                    Unit345TracePage(window, (AchievementsViewModel)window.DataContext!, kind, width);
                }
                finally
                {
                    window.Close();
                }
            }
        }
    }

    /// <summary>
    /// Work instruction 345 task 0's page half: one kind at one width - the band, then every drawn card
    /// top to bottom - returning the page's height. Leaves the window on the page of eight.
    /// </summary>
    private double Unit345TracePage(Window window, AchievementsViewModel screen, string kind, double width)
    {
        if (kind.StartsWith(AchievementCategory.ContinentPrefix, StringComparison.Ordinal))
        {
            screen.OpenCategoryCommand.Execute(AchievementKinds.Continents);
        }

        screen.OpenCategoryCommand.Execute(kind);
        Settle(window);

        var category = screen.Category!;
        var band = Named<Border>(window, "AchievementsCategoryBand");
        var bandSaid = VisibleText(band).ToList();
        var bars = band.GetVisualDescendants().OfType<BadgeProgressControl>().Where(b => b.IsEffectivelyVisible).ToList();
        var bandLine = Named<TextBlock>(window, "AchievementsCategoryBandLine");

        _output.WriteLine("  " + kind + " at " + F(width));
        _output.WriteLine(
            "    BAND drawn [" + string.Join(" | ", bandSaid) + "]; "
            + (bars.Count == 0 ? "no bar" : bars.Count + " bar, width " + string.Join(", ", bars.Select(b => F(b.Bounds.Width)))));
        _output.WriteLine(
            "    BAND model: Standing [" + category.Standing + "] " + Unit345Drawn(bandSaid, category.Standing)
            + "; ScoreLine [" + category.ScoreLine + "] " + Unit345Drawn(bandSaid, category.ScoreLine)
            + "; LevelName [" + category.LevelName + "] " + Unit345Drawn(bandSaid, category.LevelName)
            + "; BandLine [" + category.BandLine + "] " + Unit345Drawn(bandSaid, category.BandLine)
            + "; LevelBarLine [" + category.LevelBarLine + "] " + Unit345Drawn(bandSaid, category.LevelBarLine)
            + "; GapLine [" + category.GapLine + "] on the drawn band line "
            + (category.GapLine.Length > 0 && (bandLine.Text ?? "").EndsWith(" · " + category.GapLine, StringComparison.Ordinal))
            + "; NoNextLevelLine [" + category.NoNextLevelLine + "] " + Unit345Drawn(bandSaid, category.NoNextLevelLine));

        var page = window.GetVisualDescendants().OfType<ItemsControl>()
            .First(i => i.IsEffectivelyVisible && (i.Name == "AchievementsCategoryCards" || i.Name == "AchievementsSubBadges"));
        var cards = page.GetVisualDescendants().OfType<Border>()
            .Where(b => b.IsEffectivelyVisible && b.Classes.Contains("trading-card"))
            .OrderBy(b => Math.Round(Top(b, window)))
            .ThenBy(b => b.TranslatePoint(new Point(0, 0), window)?.X ?? 0)
            .ToList();
        var eightFacts = kind == AchievementKinds.Countries || kind == AchievementKinds.States || kind == AchievementKinds.Grids
            || kind.StartsWith(AchievementCategory.ContinentPrefix, StringComparison.Ordinal);

        _output.WriteLine("    PAGE " + page.Name + " " + F(page.Bounds.Width) + " x " + F(page.Bounds.Height) + ", " + cards.Count + " cards");

        foreach (var card in cards)
        {
            var data = (AchievementCategoryCard)card.DataContext!;
            var said = VisibleText(card).ToList();
            var globe = card.GetVisualDescendants().OfType<Ft8GlobeControl>()
                .FirstOrDefault(g => g.IsEffectivelyVisible && g.Plot is not null && g.Bounds.Width > 0);
            var bar = card.GetVisualDescendants().OfType<BadgeProgressControl>()
                .FirstOrDefault(b => b.IsEffectivelyVisible && b.Bounds.Width > 0);
            var list = card.GetVisualDescendants().OfType<Border>()
                .Any(b => b.IsEffectivelyVisible && b.Classes.Contains("card-list") && VisibleText(b).Any());
            var drawnMap = globe is null ? "no map" : "map drawn " + F(Unit342Drawn(globe).Width) + " x " + F(Unit342Drawn(globe).Height);

            _output.WriteLine(
                "    CARD y " + F(Top(card, window)) + " " + (data.Earned ? "earned" : "NEXT") + " [" + string.Join(" | ", said) + "]; "
                + drawnMap + "; " + (bar is null ? "no bar" : "bar " + F(bar.Bounds.Width)) + "; " + (list ? "a list" : "no list"));

            if (globe is not null && kind == AchievementKinds.Countries)
            {
                var plot = globe.Plot!;
                var inner = Unit342Column(card).Bounds.Width;
                var box = Unit342PathBox(plot);
                var (boundWidth, boundHeight) = Unit342Bound(plot, inner, CardMapHeight);

                _output.WriteLine(
                    "      crop: frame " + F(globe.DrawnFrame.Width) + " by " + F(globe.DrawnFrame.Height) + " within the bound " + F(boundWidth)
                    + " by " + F(boundHeight) + ", margin " + F(Ft8GlobePlot.MarginShare * Math.Max(box.Width, box.Height))
                    + " (MarginShare " + F(Ft8GlobePlot.MarginShare) + " of the path's longer side " + F(Math.Max(box.Width, box.Height))
                    + "); inside " + (OutsideTheCrop(plot, globe.DrawnFrame, inner, CardMapHeight) ?? "yes"));
            }

            if (data.Earned && eightFacts)
            {
                var bandMode = data.BandModeLine.Split(" · ");
                var entity = kind == AchievementKinds.Grids
                    ? data.CallGridLine[(data.CallGridLine.IndexOf(" · ", StringComparison.Ordinal) is var at && at >= 0 ? at + 3 : data.CallGridLine.Length)..]
                    : data.Title;

                _output.WriteLine(
                    "      EIGHT: entity [" + entity + "] " + Unit345Drawn(said, entity)
                    + "; callsign [" + data.Callsign + "] " + Unit345Drawn(said, data.Callsign)
                    + "; grid [" + data.Grid + "] " + Unit345Drawn(said, data.Grid)
                    + "; distance [" + data.DistanceLine + "] " + Unit345Drawn(said, data.DistanceLine)
                    + "; band [" + (bandMode.Length > 1 ? bandMode[0] : "") + "] " + Unit345Drawn(said, bandMode.Length > 1 ? bandMode[0] : "")
                    + "; mode [" + bandMode[^1] + "] " + Unit345Drawn(said, bandMode[^1])
                    + "; date [" + data.DateLine + "] " + Unit345Drawn(said, data.DateLine)
                    + "; points [" + data.PointsLine + "] " + Unit345Drawn(said, data.PointsLine)
                    + (data.HasMap ? "" : "; no map [" + data.NoMapWord + "] " + Unit345Drawn(said, data.NoMapWord)));
            }

            var held = new List<(string Name, string Value)>
            {
                ("title", data.Title),
                ("callgrid", data.CallGridLine),
                ("count", data.CountLine),
                ("tier", data.TierLine),
                ("wants", data.WantsLine),
                ("quill", data.QuillLine),
                ("heading", data.CallersHeading),
                ("more", data.MoreCallersLine),
                ("nocaller", data.NoCallerLine),
                ("distance", data.DistanceLine),
                ("bandmode", data.BandModeLine),
                ("date", data.DateLine),
                ("points", data.PointsLine),
            };

            held.AddRange(data.Callers.SelectMany(c => new[] { ("caller place", c.Place), ("caller call", c.CallLine) }));

            _output.WriteLine(
                "      MODEL: " + string.Join("; ", held.Where(h => h.Value.Length > 0).Select(h => h.Name + " [" + h.Value + "] " + Unit345Drawn(said, h.Value)))
                + (data.Figure.Length > 0 ? "; figure [" + data.Figure + "] " + (data.ShowsFigure ? Unit345Drawn(said, data.Figure) : "hidden by ShowsFigure") : ""));
        }

        var height = page.Bounds.Height;

        while (screen.Category is not null)
        {
            screen.BackCommand.Execute(null);
        }

        Settle(window);

        return height;
    }

    /// <summary>
    /// **Work instruction 346 task 0: the Modes next card, the CW place, and every page's fit, before
    /// anything changes** - at 1400 and 1920. It asserts nothing and presses nothing that transmits or
    /// tunes.
    /// </summary>
    /// <remarks>
    /// **EVERY NUMBER IS COMPUTED ON THE HEADLESS HOST, NOT SEEN.** A Modes row with no line is printed
    /// `EMPTY`, and a run that would clip is printed rather than asserted.
    /// </remarks>
    [AvaloniaFact]
    public void Unit346TraceTheModesNextCardAndEveryPage()
    {
        var bet = new BandBet("17 m", "best bet now");
        var lists = new[] { (Label: "Calling()", List: Calling()), (Label: "Calling() and a PSK31 caller", List: CallingWithPsk31()) };

        static string Mhz(long hz) => (hz / 1_000_000.0).ToString("0.000", CultureInfo.InvariantCulture);

        // **WHAT MODE EACH ROW ON THE LIST CARRIES, AND WHETHER ANY CAN SAY FT4.**
        foreach (var (label, list) in lists)
        {
            _output.WriteLine(
                "LIST " + label + ": " + string.Join(", ", list.Calls.Select(c => c.Callsign + " grid [" + c.Grid + "] mode [" + c.Mode + "]"))
                + "; calls saying FT4: " + list.Calls.Count(c => c.Mode == "FT4"));
        }

        // **THE CW PLACE CANDIDATES, PER HF BAND**, through the public route.
        _output.WriteLine("CW ROUTE: HfBands.Bands (public) -> CwBand.JumpHz, which Build fills from HfBands.Landing (private); CwBand.CwLowHz and CwHighHz");

        foreach (var band in HfBands.Bands)
        {
            _output.WriteLine(
                "  " + band.Name.PadRight(6) + "lands " + Mhz(band.JumpHz) + ", CW segment " + Mhz(band.CwLowHz) + " to " + Mhz(band.CwHighHz)
                + "; FT4 row " + (DigitalCallingFrequencies.Find(band.Name, "FT4") is { } ft4 ? Mhz(ft4.JumpHz) : "none")
                + ", PSK31 row " + (DigitalCallingFrequencies.Find(band.Name, "PSK31") is { } psk ? Mhz(psk.JumpHz) : "none")
                + (band.Name == bet.Band ? "  <- the fixture's best bet" : ""));
        }

        // **THE MODES NEXT CARD ON THE FIVE-CONTACT LOG, DRAWN BESIDE THE VIEW MODEL, ROW BY ROW.**
        foreach (var width in new[] { 1400.0, 1920.0 })
        {
            foreach (var (label, list) in lists)
            {
                var window = Realized(FiveContacts(), width, list, bet);
                var screen = (AchievementsViewModel)window.DataContext!;

                try
                {
                    OpenOnWindow(window, screen, AchievementKinds.Modes);

                    var next = screen.Category!.Cards[^1];
                    var card = TradingCards(window).Single(b => ReferenceEquals(b.DataContext, next));
                    var rows = CallerRows(card);

                    _output.WriteLine(
                        "MODES NEXT at " + F(width) + ", five contacts, " + label + ": earned " + next.Earned + ", heading [" + next.CallersHeading
                        + "], no-caller [" + next.NoCallerLine + "], " + rows.Count + " rows drawn; card drawn [" + string.Join(" | ", VisibleText(card)) + "]");

                    foreach (var held in next.Callers)
                    {
                        var drawn = rows.FirstOrDefault(r => r.Place == held.Place);

                        _output.WriteLine(
                            "  row " + held.Place.PadRight(6) + " model [" + (held.CallLine.Length == 0 ? "EMPTY" : held.CallLine) + "] drawn ["
                            + (drawn is null ? "NOT DRAWN" : drawn.CallLine.Length == 0 ? "EMPTY" : drawn.CallLine) + "]");
                    }
                }
                finally
                {
                    window.Close();
                }
            }
        }

        // **EVERY PAGE RULING 27 NAMES, AND EVERY EARNED CARD WITH A MAP ON IT.**
        foreach (var width in new[] { 1400.0, 1920.0 })
        {
            foreach (var (records, label, list, kinds) in new[]
            {
                (TheAchievementsPageTests.TwelveContacts(), "twelve contacts", Calling(), AchievementKinds.All.Concat(ContinentKinds()).ToList()),
                (FiveContacts(), "five contacts and a PSK31 caller", CallingWithPsk31(), new List<string> { AchievementKinds.Modes }),
            })
            {
                var window = Realized(records, width, list, bet);
                var screen = (AchievementsViewModel)window.DataContext!;
                var tallest = (Height: 0.0, Kind: "");

                try
                {
                    _output.WriteLine("PAGES at " + F(width) + ", " + label + ", window " + F(window.Bounds.Width) + " x " + F(window.Bounds.Height));

                    foreach (var kind in kinds)
                    {
                        OpenOnWindow(window, screen, kind);

                        var (fit, clips) = Unit346Fit(window);
                        var cards = window.GetVisualDescendants().OfType<Border>()
                            .Where(b => b.IsEffectivelyVisible && b.Classes.Contains("trading-card"))
                            .ToList();
                        var page = window.GetVisualDescendants().OfType<ItemsControl>()
                            .First(i => i.IsEffectivelyVisible && (i.Name == "AchievementsCategoryCards" || i.Name == "AchievementsSubBadges"));

                        if (page.Bounds.Height > tallest.Height)
                        {
                            tallest = (page.Bounds.Height, kind);
                        }

                        _output.WriteLine(
                            "  " + kind.PadRight(14) + fit + " runs fit, " + clips.Count + " clip or wrap, " + cards.Count + " cards, "
                            + cards.Count(c => !Unit346Filled(c)) + " white, page " + F(page.Bounds.Height) + " px tall");

                        foreach (var clip in clips)
                        {
                            _output.WriteLine("    CLIPS " + clip);
                        }

                        foreach (var card in cards.Where(c => c.DataContext is AchievementCategoryCard { Earned: true }))
                        {
                            var data = (AchievementCategoryCard)card.DataContext!;
                            var globe = card.GetVisualDescendants().OfType<Ft8GlobeControl>()
                                .FirstOrDefault(g => g.IsEffectivelyVisible && g.Plot is not null && g.Bounds.Width > 0);

                            if (globe is null)
                            {
                                continue;
                            }

                            var column = card.GetVisualDescendants().OfType<StackPanel>().FirstOrDefault(s => Grid.GetColumn(s) == 1);
                            var drawn = Unit342Drawn(globe);

                            _output.WriteLine(
                                "    MAP [" + data.Title + "] " + data.Callsign + " drawn " + F(drawn.Width) + " x " + F(drawn.Height)
                                + " in a control " + F(globe.Bounds.Width) + " x " + F(globe.Bounds.Height) + "; card inner width "
                                + (column is null ? "no column" : F(column.Bounds.Width) + ", short by " + F(column.Bounds.Width - drawn.Width)));
                        }

                        ToThePage(window, screen);
                    }
                }
                finally
                {
                    window.Close();
                }

                _output.WriteLine("  TALLEST at " + F(width) + ", " + label + ": " + tallest.Kind + ", " + F(tallest.Height) + " px");
            }
        }
    }

    /// <summary>
    /// Work instruction 346 task 0: `Fits`' measurement without its asserts - how many visible runs fit,
    /// and each one that would clip or wrap, in `Fits`' own words.
    /// </summary>
    private static (int Fit, List<string> Clips) Unit346Fit(Window window)
    {
        var fit = 0;
        var clips = new List<string>();

        foreach (var text in window.GetVisualDescendants().OfType<TextBlock>()
            .Where(t => t.IsEffectivelyVisible && (t.Text ?? "").Trim().Length > 0))
        {
            var needs = Unit342Needs(text, text.Text ?? "");
            var clip = text.TextWrapping != Avalonia.Media.TextWrapping.NoWrap ? "[" + text.Text + "] is allowed to wrap"
                : needs > text.Bounds.Width + 0.5 ? "[" + text.Text + "] needs " + F(needs) + " px and its slot is " + F(text.Bounds.Width)
                : null;

            foreach (var box in clip is null ? text.GetVisualAncestors().OfType<Control>() : Enumerable.Empty<Control>())
            {
                var left = text.TranslatePoint(new Point(0, 0), box)?.X ?? 0;

                if (left < -0.5 || left + needs > box.Bounds.Width + 0.5)
                {
                    clip = "[" + text.Text + "] runs from " + F(left) + " to " + F(left + needs) + " inside a " + box.GetType().Name + " " + F(box.Bounds.Width) + " wide";
                    break;
                }
            }

            if (clip is null)
            {
                fit++;
            }
            else
            {
                clips.Add(clip);
            }
        }

        return (fit, clips);
    }

    /// <summary>True where a card draws a map, a bar or a list - the page-wide test's measure of a card that is not white.</summary>
    private static bool Unit346Filled(Border card)
    {
        var inside = card.GetVisualDescendants().Where(v => v is Control { IsEffectivelyVisible: true }).ToList();

        return inside.OfType<Ft8GlobeControl>().Any(m => m.Plot is not null && m.Bounds.Width > 0)
            || inside.OfType<BadgeProgressControl>().Any(b => b.Bounds.Width > 0)
            || inside.OfType<Border>().Any(b => b.Classes.Contains("card-list") && VisibleText(b).Any());
    }

    private static string Unit345Drawn(IReadOnlyList<string> said, string value)
        => value.Length == 0 ? "(none held)"
            : said.Any(s => s.Contains(value, StringComparison.Ordinal)) ? "drawn" : "NOT DRAWN";

    /// <summary>The seven continent pages' kinds, `continent-AF` to `continent-SA`.</summary>
    private static IEnumerable<string> ContinentKinds()
        => DxccContinents.Codes.Keys.OrderBy(k => k, StringComparer.Ordinal).Select(k => AchievementCategory.ContinentPrefix + k);

    /// <summary>Opens a kind on the realized window, through Continents where it is a continent.</summary>
    private static void OpenOnWindow(Window window, AchievementsViewModel screen, string kind)
    {
        if (kind.StartsWith(AchievementCategory.ContinentPrefix, StringComparison.Ordinal))
        {
            screen.OpenCategoryCommand.Execute(AchievementKinds.Continents);
        }

        screen.OpenCategoryCommand.Execute(kind);
        Settle(window);
    }

    /// <summary>Back to the page of eight.</summary>
    private static void ToThePage(Window window, AchievementsViewModel screen)
    {
        while (screen.Category is not null)
        {
            screen.BackCommand.Execute(null);
        }

        Settle(window);
    }

    /// <summary>
    /// **Work instruction 345 task 1, criterion 1 on the drawn band**: null where the band shows the
    /// kind's name, its count, score and level as whole parts of the drawn band line, its color, and
    /// either one bar with width, the words over it and the gap at the line's end, or no bar and the
    /// words saying there is no next level; otherwise what is missing.
    /// </summary>
    private static string? BandMiss(Window window, AchievementCategory category)
    {
        var band = Named<Border>(window, "AchievementsCategoryBand");
        var said = VisibleText(band).ToList();
        var line = Named<TextBlock>(window, "AchievementsCategoryBandLine");
        var drawnLine = line.IsEffectivelyVisible ? line.Text ?? "" : "";
        var parts = drawnLine.Split(" · ");
        var bars = band.GetVisualDescendants().OfType<BadgeProgressControl>().Where(b => b.IsEffectivelyVisible).ToList();
        var shown = " (drawn: " + string.Join(" | ", said) + "; " + bars.Count + " bar)";

        foreach (var (what, value) in new[] { ("the count", category.Standing), ("the score", category.ScoreLine), ("the level", category.LevelName) })
        {
            if (value.Length > 0 && !parts.Contains(value))
            {
                return what + " [" + value + "] is not a part of the drawn band line" + shown;
            }
        }

        if (!said.Contains(category.Name))
        {
            return "the name [" + category.Name + "] is not drawn" + shown;
        }

        if (drawnLine != category.BandLine)
        {
            return "the drawn band line is not [" + category.BandLine + "]" + shown;
        }

        if (band.Background is not Avalonia.Media.ISolidColorBrush fill || fill.Color != Avalonia.Media.Color.Parse(category.Band))
        {
            return "the band is not filled " + category.Band + shown;
        }

        if (category.HasLevelBar)
        {
            if (bars.Count != 1 || bars[0].Bounds.Width <= 0)
            {
                return "a next level and " + bars.Count + " bar(s), or a bar with no width" + shown;
            }

            if (!said.Contains(category.LevelBarLine))
            {
                return "the words over the bar [" + category.LevelBarLine + "] are not drawn" + shown;
            }

            if (category.GapLine.Length == 0 || parts[^1] != category.GapLine)
            {
                return "the drawn band line does not end in the gap [" + category.GapLine + "]" + shown;
            }
        }
        else
        {
            if (bars.Count != 0)
            {
                return "no next level and " + bars.Count + " bar(s) drawn" + shown;
            }

            if (category.NoNextLevelLine.Length == 0 || !said.Contains(category.NoNextLevelLine))
            {
                return "no next level and the words [" + category.NoNextLevelLine + "] are not drawn" + shown;
            }
        }

        return null;
    }

    /// <summary>The caller rows a card draws, top to bottom, as place and call line.</summary>
    private static List<NextCaller> CallerRows(Border card)
    {
        List<TextBlock> Slot(string slot)
            => card.GetVisualDescendants().OfType<TextBlock>()
                .Where(t => t.IsEffectivelyVisible && t.Classes.Contains(slot))
                .OrderBy(t => t.TranslatePoint(new Point(0, 0), card)?.Y ?? 0)
                .ToList();

        return Slot("card-caller-place").Zip(Slot("card-caller-call"), (p, c) => new NextCaller(p.Text ?? "", c.Text ?? "")).ToList();
    }

    /// <summary>
    /// **Work instruction 345 task 1, criterion 3 on the drawn next card**: null where the card draws
    /// every line the view model holds for it - the title, the wants line, the quill line and no quill
    /// where it holds none, the callers heading, each caller's place and call line in order, the
    /// no-caller and more-callers lines, and the tier line with a bar - otherwise what is missing.
    /// </summary>
    private static string? NextCardMiss(Border card, AchievementCategoryCard next)
    {
        var said = VisibleText(card).ToList();
        var shown = " (drawn: " + string.Join(" | ", said) + ")";

        string? Missing(string what, string value)
            => value.Length > 0 && !said.Contains(value) ? what + " [" + value + "] is not drawn" + shown : null;

        var miss = Missing("the title", next.Title)
            ?? Missing("the wants line", next.WantsLine)
            ?? Missing("the quill line", next.QuillLine)
            ?? Missing("the callers heading", next.CallersHeading)
            ?? Missing("the no-caller line", next.NoCallerLine)
            ?? Missing("the more-callers line", next.MoreCallersLine)
            ?? Missing("the tier line", next.TierLine);

        if (miss is not null)
        {
            return miss;
        }

        if (next.QuillLine.Length == 0
            && card.GetVisualDescendants().OfType<TextBlock>().Any(t => t.IsEffectivelyVisible && t.Classes.Contains("card-quill")))
        {
            return "a quill line is drawn where the view model holds none" + shown;
        }

        var rows = CallerRows(card);
        var held = next.Callers.Select(c => new NextCaller(c.Place, c.CallLine)).ToList();

        if (!rows.SequenceEqual(held))
        {
            return "the callers drawn are [" + string.Join(" / ", rows.Select(r => r.Place + " " + r.CallLine)) + "], not ["
                + string.Join(" / ", held.Select(r => r.Place + " " + r.CallLine)) + "]" + shown;
        }

        if (next.HasCallersPanel && held.Count == 0 && next.NoCallerLine.Length == 0)
        {
            return "a callers panel with no caller and no words saying so" + shown;
        }

        if (next.HasTierBar && !card.GetVisualDescendants().OfType<BadgeProgressControl>().Any(b => b.IsEffectivelyVisible && b.Bounds.Width > 0))
        {
            return "a tier line and no bar with width" + shown;
        }

        return null;
    }

    /// <summary>The trading cards the open category draws in its card list, top to bottom.</summary>
    private static List<Border> TradingCards(Window window)
        => Named<ItemsControl>(window, "AchievementsCategoryCards").GetVisualDescendants().OfType<Border>()
            .Where(b => b.IsEffectivelyVisible && b.Classes.Contains("trading-card"))
            .OrderBy(b => Math.Round(Top(b, window)))
            .ThenBy(b => b.TranslatePoint(new Point(0, 0), window)?.X ?? 0)
            .ToList();

    /// <summary>True where the card draws a map with a plot and width.</summary>
    private static bool HasMap(Border card)
        => card.GetVisualDescendants().OfType<Ft8GlobeControl>().Any(g => g.IsEffectivelyVisible && g.Plot is not null && g.Bounds.Width > 0);

    /// <summary>True where the card draws a bar with width.</summary>
    private static bool HasBar(Border card)
        => card.GetVisualDescendants().OfType<BadgeProgressControl>().Any(b => b.IsEffectivelyVisible && b.Bounds.Width > 0);

    /// <summary>
    /// **Work instruction 345 task 2**: null where every wanted string is drawn on the card, as a whole
    /// line or as one whole part of a line split at ` · `; otherwise which are not.
    /// </summary>
    private static string? CardMiss(Border card, params string[] wanted)
    {
        var said = VisibleText(card).ToList();
        var absent = wanted.Where(w => !said.Any(s => s == w || s.Split(" · ").Contains(w))).ToList();

        return absent.Count == 0
            ? null
            : "[" + string.Join("] [", absent) + "] not drawn on the card (drawn: " + string.Join(" | ", said) + ")";
    }

    /// <summary>
    /// What a contact earns on a kind whose cards are the contact that earned them - the entity's
    /// spoken name on Countries, the square on Grids, the scored state on States - or null.
    /// </summary>
    private static string? EarnedBy(string kind, AchievementContact contact)
        => kind == AchievementKinds.Countries ? (contact.Entity is null ? null : EntitySpoken.Of(contact.Entity))
            : kind == AchievementKinds.Grids ? contact.Grid
            : kind == AchievementKinds.States ? AchievementLog.StateOf(contact)
            : null;

    /// <summary>
    /// **Work instruction 345 task 2, ruling 21**: null where the drawn earned card is `entry` - the
    /// entity, callsign, grid, distance, band, mode, date and points each drawn as the log entry has
    /// them, the points from the shipped file - and where the entry lacks a fact, nothing in its place,
    /// no dash, and no map without a grid; otherwise what is wrong.
    /// </summary>
    private static string? EarnedCardMiss(Border card, string kind, AchievementContact entry, AchievementPoints points)
    {
        var said = VisibleText(card).ToList();
        var parts = said.SelectMany(s => s.Split(" · ")).ToList();
        var shown = " (drawn: " + string.Join(" | ", said) + ")";
        var key = EarnedBy(kind, entry)!;
        var wanted = new List<(string What, string Value, bool Whole)>
        {
            (kind == AchievementKinds.Grids ? "the grid" : "the entity", key, true),
            ("the callsign", entry.Callsign, false),
            ("the points", AchievementCategory.Pts(points.Special(kind, key) ?? points.Per(kind)), true),
        };

        if (kind == AchievementKinds.Grids && entry.Entity is not null)
        {
            wanted.Add(("the entity", EntitySpoken.Short(entry.Entity), false));
        }
        else if (kind != AchievementKinds.Grids && entry.Grid is not null)
        {
            wanted.Add(("the grid", entry.Grid, false));
        }

        if (entry.Miles is { } miles)
        {
            wanted.Add(("the distance", GridPath.DescribeMiles(miles).Replace(" miles", " mi", StringComparison.Ordinal), true));
        }

        if (entry.Band is not null)
        {
            wanted.Add(("the band", AdifLog.BandDisplayNameFor(entry.Band), false));
        }

        if (entry.Mode is not null)
        {
            wanted.Add(("the mode", entry.Mode.Name, false));
        }

        if (entry.StartedUtc is { } at)
        {
            wanted.Add(("the date", at.ToString("MMM d, yyyy", CultureInfo.InvariantCulture), true));
        }

        foreach (var (what, value, whole) in wanted)
        {
            if (!(whole ? said.Contains(value) : parts.Contains(value)))
            {
                return what + " [" + value + "] from " + entry.Callsign + "'s log entry is not drawn" + shown;
            }
        }

        if (said.Any(s => s.Trim() == "-" || s.Contains('—', StringComparison.Ordinal)))
        {
            return "a dash is drawn" + shown;
        }

        var slots = card.GetVisualDescendants().OfType<TextBlock>().Where(t => t.IsEffectivelyVisible).ToList();

        if (entry.Grid is null || entry.Miles is null)
        {
            if (HasMap(card))
            {
                return "a map is drawn for an entry with no grid" + shown;
            }

            if (!said.Contains("no grid, so no map"))
            {
                return "no map and no word saying why" + shown;
            }

            if (slots.Any(t => t.Classes.Contains("card-distance")))
            {
                return "a distance slot is drawn for an entry with no distance" + shown;
            }
        }
        else if (!HasMap(card))
        {
            return "an entry with a grid and no map with width" + shown;
        }

        var facts = slots.Count(t => t.Classes.Contains("card-facts"));
        var held = (entry.Band is not null || entry.Mode is not null ? 1 : 0) + (entry.StartedUtc is not null ? 1 : 0);

        return facts == held ? null : facts + " band, mode and date lines drawn where the entry has " + held + shown;
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

    /// <summary>
    /// `Calling()`'s four, and a PSK31 station in Spain calling CQ - a text-only row read by the PSK31
    /// parser, as the list builds one (work instruction 346).
    /// </summary>
    private static CqSnapshot CallingWithPsk31()
    {
        const string text = "CQ CQ CQ de EA3XYZ EA3XYZ K";

        return CqSnapshot.From(
            new[]
            {
                Heard("CQ OE8DDX JN76"),
                Heard("CQ DX J38DX FK92"),
                Heard("CQ K1ABC FN42"),
                Heard("CQ ZL1ABC RF72"),
                new DigitalDecodeRow(
                    "214100", "+10", DigitalDecodeRow.NotMeasured, "1000", text,
                    IsTextOnly: true, Reading: Hamlet.RadioEngine.Psk31.Psk31ExchangeParser.Read(text, "KC3QIS")),
            },
            new DateTime(2026, 9, 12, 21, 41, 0, DateTimeKind.Utc));
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

    private static double Top(Visual visual, Visual root)
        => visual.TranslatePoint(new Point(0, 0), root)?.Y ?? double.NaN;

    private static IEnumerable<string> VisibleText(Control root)
        => root.GetVisualDescendants().OfType<TextBlock>()
            .Where(t => t.IsEffectivelyVisible && (t.Text ?? "").Trim().Length > 0)
            .Select(t => t.Text!);

    private static string F(double value) => value.ToString("0.00", CultureInfo.InvariantCulture);
}
