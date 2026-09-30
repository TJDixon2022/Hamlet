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
/// Work instruction 505: **a category is a list, and the map opens on a click** (HM-DEC-209).
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-30**: *"we list continents, countries, whatever, and you can click on it and
/// it'll pop up the map ... But we're just overdoing it on maps."* It supersedes the trading card of
/// work instructions 335 and 342, whose tests were carried here in substance or retired, each named in
/// that unit's report.</para>
/// <para>**EVERYTHING HERE IS COMPUTED ON THE HEADLESS HOST, NOT SEEN.** The window is stood up and
/// its layout read back; nothing looks at a pixel.</para>
/// </remarks>
public sealed class TheCategoryPagesAreListsTests
{
    private const string MyGrid = "FN00";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the measurements are printed.</param>
    public TheCategoryPagesAreListsTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **Task 1: with the popup closed, no map is realized anywhere in a category**, on each of the
    /// eight kinds and on a continent's own page, on both fixtures.
    /// </summary>
    [AvaloniaFact]
    public void NoMapIsDrawnInACategoryUntilARowIsPressed()
    {
        var drawn = new List<string>();

        foreach (var (records, label) in new[]
        {
            (TheAchievementsPageTests.TwelveContacts(), "twelve contacts"),
            (FiveContacts(), "five contacts"),
        })
        {
            var window = Realized(records, 1280);
            var screen = (AchievementsViewModel)window.DataContext!;

            try
            {
                foreach (var kind in AchievementKinds.All.Append(AchievementCategory.ContinentPrefix + "EU"))
                {
                    OpenOnWindow(window, screen, kind);

                    var maps = window.GetVisualDescendants().OfType<Ft8GlobeControl>().Count();

                    _output.WriteLine(label + " " + kind.PadRight(14) + " maps realized with the popup closed: " + maps);

                    Assert.False(screen.MapIsOpen);

                    if (maps > 0)
                    {
                        drawn.Add(label + " " + kind + ": " + maps);
                    }

                    ToThePage(window, screen);
                }
            }
            finally
            {
                window.Close();
            }
        }

        Assert.True(drawn.Count == 0, "maps realized with the popup closed: " + string.Join("; ", drawn));
    }

    /// <summary>
    /// **Task 1: every earned row shows its title, call line, distance, band and mode, date and points**,
    /// with the distance the largest thing on it after the title; and how many rows show at 1280 x 860.
    /// </summary>
    [AvaloniaFact]
    public void EveryEarnedRowShowsItsFacts()
    {
        foreach (var (records, label) in new[]
        {
            (TheAchievementsPageTests.TwelveContacts(), "twelve contacts"),
            (FiveContacts(), "five contacts"),
        })
        {
            var window = Realized(records, 1280);
            var screen = (AchievementsViewModel)window.DataContext!;

            try
            {
                OpenOnWindow(window, screen, AchievementKinds.Countries);

                var rows = CategoryCards(window).Where(b => b.DataContext is AchievementCategoryCard { Earned: true }).ToList();
                var earned = screen.Category!.Cards.Where(c => c.Earned).ToList();

                Assert.True(rows.Count == earned.Count, label + ": " + rows.Count + " earned rows drawn for " + earned.Count + " earned");

                foreach (var row in rows)
                {
                    var card = (AchievementCategoryCard)row.DataContext!;
                    var said = VisibleText(row).ToList();

                    _output.WriteLine(label + " row " + F(row.Bounds.Height) + " px: " + string.Join(" | ", said));

                    foreach (var (what, value) in new[]
                    {
                        ("the title", card.Title), ("the call line", card.CallGridLine), ("the distance", card.DistanceLine),
                        ("the band and mode", card.BandModeLine), ("the date", card.DateLine), ("the points", card.PointsLine),
                    })
                    {
                        Assert.True(value.Length == 0 || said.Contains(value), label + " [" + card.Title + "]: " + what + " [" + value + "] is not drawn");
                    }

                    // **THE DISTANCE IS THE LARGEST THING ON THE ROW AFTER THE TITLE.**
                    if (card.HasDistance)
                    {
                        var texts = row.GetVisualDescendants().OfType<TextBlock>().Where(t => t.IsEffectivelyVisible).ToList();
                        var distance = texts.Single(t => t.Classes.Contains("card-distance"));

                        Assert.All(
                            texts.Where(t => !t.Classes.Contains("card-title") && !t.Classes.Contains("card-distance")),
                            t => Assert.True(t.FontSize < distance.FontSize, "[" + t.Text + "] is as large as the distance"));
                    }
                }

                var viewport = CardsScroller(window);
                var whole = rows.Count(r => Inside(r, viewport));

                _output.WriteLine(label + ": " + whole + " earned rows show whole at 1280 x " + F(window.Bounds.Height) + " without scrolling, of " + rows.Count);
            }
            finally
            {
                window.Close();
            }
        }
    }

    /// <summary>
    /// **Task 1: a row with a map is a button that says `map`, and a row without one is not a button and
    /// says why in a word**, with its contacts in its tooltip.
    /// </summary>
    [AvaloniaFact]
    public void ARowWithAMapIsAButtonAndOneWithoutSaysWhy()
    {
        var window = Realized(FiveContacts(), 1280);
        var screen = (AchievementsViewModel)window.DataContext!;

        try
        {
            OpenOnWindow(window, screen, AchievementKinds.Countries);

            var rows = CategoryCards(window).Where(b => b.DataContext is AchievementCategoryCard { Earned: true }).ToList();

            Assert.Contains(rows, r => ((AchievementCategoryCard)r.DataContext!).HasMap);
            Assert.Contains(rows, r => !((AchievementCategoryCard)r.DataContext!).HasMap);

            foreach (var row in rows)
            {
                var card = (AchievementCategoryCard)row.DataContext!;
                var button = RowButton(row);
                var said = VisibleText(row).ToList();

                _output.WriteLine(
                    "[" + card.Title + "] " + (button is null ? "not a button" : "a button, cursor " + button.Cursor + ", focusable " + button.Focusable)
                    + "; says " + string.Join(" | ", said));

                if (card.HasMap)
                {
                    Assert.True(button is not null, card.Title + " has a map and its row is not a button");
                    Assert.Same(screen.OpenTheMapCommand, button!.Command);
                    Assert.Same(card, button.CommandParameter);
                    Assert.True(button.Focusable && button.IsEffectivelyEnabled, card.Title + "'s row cannot be reached by the keyboard");
                    Assert.Contains(AchievementCategoryCard.MapWord, said);
                }
                else
                {
                    Assert.True(button is null, card.Title + " has no map and its row is a button");
                    Assert.Contains(card.NoMapWord, said);
                    Assert.DoesNotContain(AchievementCategoryCard.MapWord, said);

                    var tip = row.GetSelfAndVisualAncestors().OfType<Control>().Select(ToolTip.GetTip).OfType<string>().FirstOrDefault();

                    Assert.True(tip is not null && card.ContactLines.All(l => tip.Contains(l, StringComparison.Ordinal)), card.Title + "'s contacts are not in its tooltip: " + tip);
                }
            }
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// **Task 1: no string on any row or panel clips or wraps at 1280, 1400 and 1920 wide**, on the eight
    /// kinds and the seven continent pages.
    /// </summary>
    [AvaloniaFact]
    public void NoStringOnARowClipsAtThreeWidths()
    {
        foreach (var width in new[] { 1280.0, 1400.0, 1920.0 })
        {
            var window = Realized(TheAchievementsPageTests.TwelveContacts(), width, Calling(), new BandBet("17 m", "best bet now"));
            var screen = (AchievementsViewModel)window.DataContext!;

            try
            {
                foreach (var kind in AchievementKinds.All.Concat(ContinentKinds()))
                {
                    OpenOnWindow(window, screen, kind);

                    var cards = window.GetVisualDescendants().OfType<Border>()
                        .Where(b => b.IsEffectivelyVisible && b.Classes.Contains("category-card"))
                        .ToList();
                    var clips = Unit346Fit(window).Clips
                        .Where(c => cards.Any(card => VisibleText(card).Any(t => c.StartsWith("[" + t + "]", StringComparison.Ordinal))))
                        .ToList();

                    _output.WriteLine(F(width) + " " + kind.PadRight(14) + cards.Count + " rows and panels, " + (clips.Count == 0 ? "nothing clips" : string.Join("; ", clips)));

                    Assert.True(cards.Count > 0, F(width) + " " + kind + ": no row is drawn");
                    Assert.True(clips.Count == 0, F(width) + " " + kind + ": " + string.Join("; ", clips));

                    ToThePage(window, screen);
                }
            }
            finally
            {
                window.Close();
            }
        }
    }

    /// <summary>
    /// **Task 2: pressing a row opens that row's path in the popup, headed by the place and the
    /// station; the X and a click outside close it; another row shows its own; a row with no map
    /// opens nothing.** Real clicks on the window, at the row's own place.
    /// </summary>
    [AvaloniaFact]
    public void ARowPressOpensItsPathAndThePopupSaysWhere()
    {
        var window = Realized(FiveContacts(), 1280);
        var screen = (AchievementsViewModel)window.DataContext!;

        List<Popup> Open() => window.GetVisualDescendants().OfType<Popup>().Where(p => p.IsOpen).ToList();

        void Click(Control control)
        {
            var at = control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), window)!.Value;

            window.MouseMove(at);
            window.MouseDown(at, MouseButton.Left);
            window.MouseUp(at, MouseButton.Left);
            Settle(window);
        }

        try
        {
            OpenOnWindow(window, screen, AchievementKinds.Countries);

            Border Row(string callsign)
                => CategoryCards(window).Single(b => b.DataContext is AchievementCategoryCard c && c.Callsign == callsign);

            foreach (var callsign in new[] { "LA1ZZZ", "G0MNO" })
            {
                var row = Row(callsign);
                var card = (AchievementCategoryCard)row.DataContext!;

                Assert.Empty(Open());

                Click(row);

                var open = Open();

                Assert.True(open.Count == 1, card.Title + ": a press on the row opened " + open.Count + " popups");

                var globe = open[0].Child!.GetVisualDescendants().OfType<Ft8GlobeControl>().Single();
                var heading = VisibleText((Control)open[0].Child!).ToList();

                _output.WriteLine("[" + card.Title + "] opened " + globe.Plot?.Callsign + ", heading " + string.Join(" | ", heading));

                Assert.Same(card.Globe, globe.Plot);
                Assert.Contains(heading, h => h.Contains(card.Title, StringComparison.Ordinal) && h.Contains(card.Callsign, StringComparison.Ordinal));

                // **THE X CLOSES THE FIRST, AND A CLICK OUTSIDE THE SECOND.**
                if (callsign == "LA1ZZZ")
                {
                    screen.CloseTheMapCommand.Execute(null);
                    Settle(window);
                }
                else
                {
                    var outside = new Point(window.Bounds.Width / 2, 6);

                    window.MouseMove(outside);
                    window.MouseDown(outside, MouseButton.Left);
                    window.MouseUp(outside, MouseButton.Left);
                    Settle(window);
                }

                Assert.Empty(Open());
            }

            // **A ROW WITH NO MAP OPENS NOTHING.**
            Click(Row("VE3PQR"));

            Assert.Empty(Open());
            Assert.False(screen.MapIsOpen);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// **The next stamp stands in the column beside the list, not in it** (work instruction 506 task 4,
    /// superseding unit 505's drawn-first-above-the-rows): on every kind that holds both, it is right of the
    /// list, carries its wants line and callers, is not a button, and the earned rows run newest first.
    /// </summary>
    /// <remarks>
    /// **CARRIED FROM `TheOneToEarnNextIsDrawnAboveTheEarnedRows`**, whose substance changed: the picture
    /// puts the next stamp in a column at the right, where it is always seen, so *first in the list* became
    /// *beside the list*.
    /// </remarks>
    [AvaloniaFact]
    public void TheNextStampStandsBesideTheList()
    {
        var window = Realized(TheAchievementsPageTests.TwelveContacts(), 1280, Calling(), new BandBet("17 m", "best bet now"));
        var screen = (AchievementsViewModel)window.DataContext!;
        var checkedKinds = 0;

        try
        {
            foreach (var kind in AchievementKinds.All.Concat(ContinentKinds()))
            {
                OpenOnWindow(window, screen, kind);

                var category = screen.Category!;

                if (!category.HasCards)
                {
                    ToThePage(window, screen);
                    continue;
                }

                var list = Named<ItemsControl>(window, "AchievementsCategoryCards");
                var rows = CategoryCards(window);

                _output.WriteLine(kind.PadRight(14) + "next [" + category.NextStamp?.Title + "]; rows " + string.Join(" / ", rows.Select(b => ((AchievementCategoryCard)b.DataContext!).Title)));

                // **NO UNEARNED ONE IN THE LIST, AND THE ROWS NEWEST FIRST.**
                Assert.DoesNotContain(rows, b => b.DataContext is AchievementCategoryCard { Earned: false });
                Assert.Equal(category.DrawnCards, rows.Select(b => (AchievementCategoryCard)b.DataContext!));

                if (category.NextStamp is not { } next)
                {
                    ToThePage(window, screen);
                    continue;
                }

                var panel = Named<Control>(window, "AchievementsNextCard");
                var card = panel.GetVisualDescendants().OfType<Border>().Single(b => b.IsEffectivelyVisible && b.Classes.Contains("category-card"));

                Assert.Same(next, card.DataContext);
                Assert.DoesNotContain(panel, list.GetVisualDescendants());
                Assert.Null(NextCardMiss(card, next));
                Assert.Null(RowButton(card));

                if (rows.Count > 0)
                {
                    checkedKinds++;

                    var listRight = list.TranslatePoint(new Point(list.Bounds.Width, 0), window)!.Value.X;
                    var panelLeft = panel.TranslatePoint(new Point(0, 0), window)!.Value.X;

                    Assert.True(panelLeft >= listRight - 0.5, kind + ": the next stamp starts at x " + F(panelLeft) + ", inside the list ending at " + F(listRight));
                }

                ToThePage(window, screen);
            }
        }
        finally
        {
            window.Close();
        }

        Assert.True(checkedKinds >= 4, "only " + checkedKinds + " kinds hold both a next one and an earned one");
    }

    /// <summary>
    /// **Every earned row carries its seal, with a code that is not empty** (work instruction 506 task 4), on
    /// the eight kinds, the Continents page and every continent's own page.
    /// </summary>
    [AvaloniaFact]
    public void EveryEarnedRowCarriesASeal()
    {
        var window = Realized(TheAchievementsPageTests.TwelveContacts(), 1280);
        var screen = (AchievementsViewModel)window.DataContext!;
        var sealed_ = 0;

        try
        {
            foreach (var kind in AchievementKinds.All.Concat(ContinentKinds()))
            {
                OpenOnWindow(window, screen, kind);

                var rows = Named<Control>(window, "AchievementsCategory").GetVisualDescendants().OfType<Border>()
                    .Where(b => b.IsEffectivelyVisible && b.Classes.Contains("category-card") && b.DataContext is AchievementCategoryCard { Earned: true })
                    .ToList();

                foreach (var row in rows)
                {
                    var card = (AchievementCategoryCard)row.DataContext!;
                    var seal = row.GetVisualDescendants().OfType<AchievementSealControl>().SingleOrDefault(s => s.IsEffectivelyVisible);

                    Assert.True(seal is not null, kind + " [" + card.Title + "] has no seal");
                    Assert.False(string.IsNullOrEmpty(seal!.Code), kind + " [" + card.Title + "]'s seal is empty");
                    Assert.Equal(card.SealCode, seal.Code);

                    sealed_++;
                }

                _output.WriteLine(kind.PadRight(14) + string.Join(", ", rows.Select(r => ((AchievementCategoryCard)r.DataContext!).Title + " [" + ((AchievementCategoryCard)r.DataContext!).SealCode + "]")));

                ToThePage(window, screen);
            }
        }
        finally
        {
            window.Close();
        }

        Assert.True(sealed_ > 20, "only " + sealed_ + " sealed rows across the kinds");
    }

    /// <summary>**Your reach: the farthest is the greatest distance among the cards, and it is drawn** (work instruction 506 task 4).</summary>
    [AvaloniaFact]
    public void YourReachSaysTheFarthest()
    {
        var window = Realized(TheAchievementsPageTests.TwelveContacts(), 1280);
        var screen = (AchievementsViewModel)window.DataContext!;

        try
        {
            OpenOnWindow(window, screen, AchievementKinds.Countries);

            var category = screen.Category!;
            var farthest = category.Cards.Where(c => c.Earned && c.Miles is not null).MaxBy(c => c.Miles)!;
            var said = VisibleText(Named<Control>(window, "AchievementsReach")).ToList();

            _output.WriteLine("reach: " + string.Join(" | ", said));

            Assert.Equal(farthest.Title + ", " + farthest.DistanceLine, category.ReachFarthest);
            Assert.Contains(category.ReachFarthest, said);
            Assert.Contains(category.ReachNewest, said);
            Assert.Equal(category.DrawnCards.First(c => c.EarnedUtc is not null).Title, category.ReachNewest);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// **Work instruction 335 task 3: the next card names what it wants and who on the CQ list would
    /// earn it, with distance - and says no one is calling from there when nobody listed would.**
    /// </summary>
    /// <remarks>
    /// **CARRIED WHOLE FROM `TheCategoryPagesAreTradingCardsTests` BY WORK INSTRUCTION 505 TASK 3.** Its
    /// assertions are unchanged; the drawn card is found by the class every row and panel now carries,
    /// `category-card`, where it was `trading-card`.
    /// </remarks>
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

            var drawn = Named<Control>(quillWindow, "AchievementsNextCard").GetVisualDescendants().OfType<TextBlock>()
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
        var window = Realized(records, 1280, calling);
        var shown = (AchievementsViewModel)window.DataContext!;

        try
        {
            shown.OpenCategoryCommand.Execute(AchievementKinds.Countries);
            Settle(window);

            var said = VisibleText(Named<Control>(window, "AchievementsNextCard")).ToList();

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
            // **WORK INSTRUCTION 346 TASK 1: MODES WITH A PSK31 CALLER AS WELL**, so a Modes row's who is
            // there is drawn where the list's rows say the mode and someone is calling in it.
            // **WORK INSTRUCTION 347 TASK 1, RULINGS 29 AND 30: THE PSK31 ROW AS DRAWN, WORKED OUT HERE FROM
            // THE TEXT AND `GridPath`** and not from the snapshot - a certain CQ with a grid carries his
            // distance, a CQ with no grid or an uncertain reading is the callsign alone, and of two callers
            // the nearest by miles is named. Europe is drawn on the certain caller's list too, where he is.
            foreach (var (fixture, kinds, list, psk31Row) in new[]
            {
                (records, AchievementKinds.All.Concat(ContinentKinds()).ToList(), Calling(), (string?)null),
                (FiveContacts(), new List<string> { AchievementKinds.Modes }, Calling(), "3.580 on 80 m · no one calling at 21:41 UTC"),
                (FiveContacts(), new List<string> { AchievementKinds.Modes }, CallingWithPsk31(), "3.580 on 80 m · EA3XYZ"),
                (FiveContacts(), new List<string> { AchievementKinds.Modes, AchievementCategory.ContinentPrefix + "EU" },
                    CallingWith(CertainPsk31WithGrid), "3.580 on 80 m · EA3ABC · " + Mi("JN11")),
                (FiveContacts(), new List<string> { AchievementKinds.Modes }, CallingWith(UncertainPsk31WithGrid), "3.580 on 80 m · EA3ABC"),
                (FiveContacts(), new List<string> { AchievementKinds.Modes },
                    CallingWith(Psk31WithNoGrid, CertainPsk31WithGrid), "3.580 on 80 m · EA3ABC · " + Mi("JN11") + " and 1 more"),
            })
            {
                var wide = Realized(fixture, width, list, bet);
                var opened = (AchievementsViewModel)wide.DataContext!;

                try
                {
                    foreach (var kind in kinds)
                    {
                        OpenOnWindow(wide, opened, kind);

                        var category = opened.Category!;
                        var held = category.Cards.Concat(category.SubBadges.Select(b => b.Card!)).Where(c => !c.Earned).ToList();
                        var drawn = wide.GetVisualDescendants().OfType<Border>()
                            .Where(b => b.IsEffectivelyVisible && b.Classes.Contains("category-card")
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
                                // **§R12, WORK INSTRUCTION 368.** This named the three unworked modes
                                // of one fixture - CW, FT4 and PSK31 - which is a fact about that log
                                // and about a table with five workable modes in it, not about the rule.
                                // Decision BY put Olivia among the modes there are to work, and on the
                                // twelve-contact fixture, where all five of the old table were worked
                                // and no Modes next card was drawn at all, this assertion had never
                                // run. **The rule it was written for is asserted instead**: every row
                                // drawn is a mode this log has not worked, and the rows are the
                                // unworked modes the card has room for.
                                var worked = opened.Page!.Log.Modes;
                                var stillToWork = ContactModes.Logged
                                    .Where(m => m.IsContactMode
                                        && !worked.Contains(m.Name, StringComparer.OrdinalIgnoreCase))
                                    .Select(m => m.Name)
                                    .Take(rows.Count)
                                    .OrderBy(p => p, StringComparer.Ordinal);

                                Assert.Equal(stillToWork, rows.Select(r => r.Place).OrderBy(p => p, StringComparer.Ordinal));

                                Assert.All(
                                    rows,
                                    r => Assert.DoesNotContain(r.Place, worked, StringComparer.OrdinalIgnoreCase));

                                // **WORK INSTRUCTION 346 TASK 1, RULINGS 25 AND 26: EVERY ROW SAYS WHERE ITS MODE
                                // LIVES AND WHO IS THERE, AND NONE IS EMPTY.**
                                var modesMiss = ModesRowMiss(rows, list, bet);

                                _output.WriteLine(
                                    where + " with " + list.Calls.Count + " on the CQ list, modes rows: "
                                    + (modesMiss ?? string.Join(" / ", rows.Select(r => r.Place + " [" + r.CallLine + "]"))));

                                if (psk31Row is not null)
                                {
                                    var drawnPsk31 = rows.Single(r => r.Place == "PSK31").CallLine;

                                    _output.WriteLine(where + " PSK31 row drawn [" + drawnPsk31 + "], wanted [" + psk31Row + "]");

                                    Assert.True(drawnPsk31 == psk31Row, where + ": the PSK31 row draws [" + drawnPsk31 + "], not [" + psk31Row + "]");
                                }

                                // **WORK INSTRUCTION 347 RULING 31: THE LIST WAS READ ONCE, SO NO ROW SAYS *NOW*.**
                                Assert.DoesNotContain(
                                    rows, r => System.Text.RegularExpressions.Regex.IsMatch(r.CallLine, @"\bnow\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase));

                                Assert.True(modesMiss is null, where + " with " + list.Calls.Count + " on the CQ list: " + modesMiss);

                                // **RULING 19, WATCHED RED ON THE TEST WINDOW ONLY**: the same rows held against a
                                // best bet on another band.
                                var wrongBet = ModesRowMiss(rows, list, bet with { Band = "40 m" });

                                _output.WriteLine(where + " modes rows held against a 40 m best bet, watched red: " + wrongBet);

                                Assert.NotNull(wrongBet);
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

    /// <summary>
    /// **Task 4: an earned continent's row carries two pressable things side by side** - the row,
    /// which opens its countries, and a small `map` beside it, not inside it, which opens the path of
    /// the contact that opened the continent. An unearned continent has no map button, and no button
    /// in the window sits inside another.
    /// </summary>
    [AvaloniaFact]
    public void AnEarnedContinentOpensItsCountriesAndItsMapSeparately()
    {
        var window = Realized(TheAchievementsPageTests.TwelveContacts(), 1280, Calling());
        var screen = (AchievementsViewModel)window.DataContext!;

        try
        {
            OpenOnWindow(window, screen, AchievementKinds.Continents);

            var seven = screen.Category!.SubBadges;
            var list = Named<ItemsControl>(window, "AchievementsSubBadges");
            var earnedSeen = 0;

            foreach (var badge in seven)
            {
                var card = badge.Card!;
                var opens = list.GetVisualDescendants().OfType<Button>()
                    .Single(b => b.IsEffectivelyVisible && b.Classes.Contains("hm-badge") && (b.CommandParameter as string) == badge.Kind);
                var map = list.GetVisualDescendants().OfType<Button>()
                    .Where(b => b.IsEffectivelyVisible && b.Classes.Contains("category-map") && ReferenceEquals(b.CommandParameter, card))
                    .ToList();

                _output.WriteLine(badge.Name.PadRight(15) + (card.Earned ? "earned" : "unearned") + ", map buttons " + map.Count);

                Assert.Same(screen.OpenCategoryCommand, opens.Command);

                // **THE ROW THAT OPENS THE COUNTRIES DOES NOT SAY `map`**; the button beside it does.
                Assert.DoesNotContain(AchievementCategoryCard.MapWord, VisibleText(opens));

                if (!card.OpensAMap)
                {
                    Assert.Empty(map);
                    continue;
                }

                earnedSeen++;

                Assert.Single(map);
                Assert.Same(screen.OpenTheMapCommand, map[0].Command);
                Assert.Contains(AchievementCategoryCard.MapWord, map[0].Content as string ?? "", StringComparison.Ordinal);
                Assert.DoesNotContain(map[0], opens.GetVisualDescendants().OfType<Button>());
                Assert.True(
                    map[0].TranslatePoint(new Point(0, 0), window)!.Value.X >= opens.TranslatePoint(new Point(opens.Bounds.Width, 0), window)!.Value.X - 0.5,
                    badge.Name + ": the map button is not beside the row");

                // **THE MAP BUTTON OPENS THIS CONTINENT'S FIRST PATH, AND THE CATEGORY STAYS.**
                map[0].Command!.Execute(map[0].CommandParameter);
                Settle(window);

                Assert.True(screen.MapIsOpen);
                Assert.Same(card.Globe, screen.OpenedMap);
                Assert.Equal(AchievementKinds.Continents, screen.Category!.Kind);
                Assert.Contains(card.Title, screen.OpenedMapHeading, StringComparison.Ordinal);

                screen.CloseTheMapCommand.Execute(null);
                Settle(window);
            }

            Assert.True(earnedSeen > 0, "no earned continent on the fixture");

            // **A BUTTON INSIDE A BUTTON IS NOT BUILT**, on this page or any other.
            foreach (var kind in AchievementKinds.All.Concat(ContinentKinds()))
            {
                ToThePage(window, screen);
                OpenOnWindow(window, screen, kind);

                var nested = window.GetVisualDescendants().OfType<Button>()
                    .Where(b => b.GetVisualAncestors().OfType<Button>().Any())
                    .ToList();

                Assert.True(nested.Count == 0, kind + ": " + nested.Count + " buttons inside a button");
            }
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// **Task 1: every kind's band carries its count, score and level, and a bar to the next
    /// level - or says in words that there is none.**
    /// </summary>
    [AvaloniaFact]
    public void EveryKindsBandCarriesCountScoreLevelAndABar()
    {
        var window = Realized(TheAchievementsPageTests.TwelveContacts(), 1280);
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

            // **§R12, WORK INSTRUCTION 368.** This was pinned to
            // "five modes to work · 5 of 5 · 85 pts · Gold", which was the badge's words, the
            // fixture's count and the *all modes* bonus of 50 on top of 35, all typed as one
            // string. Decision BY made Olivia one of the modes there are to work, so the words
            // follow the count and the bonus is no longer paid at five of six - and the pin was
            // guarding the door this unit was told to open. **The number is still not typed**:
            // the count comes from AchievementScores.WorkableModes, which is a count of the
            // table, and the words come from the badge that reads it.
            Assert.Equal(6, AchievementScores.WorkableModes);
            Assert.Equal(
                AchievementBadgePage.ModesToWork + " · 5 of 6 · 35 pts · Gold",
                screen.Category.BandLine);
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

        // **ON THE WINDOW: EVERY ROW THAT HAS A MAP IS THE BUTTON THAT OPENS IT, AND THE ONE THAT DOES
        // NOT SAYS WHY IN A WORD** (work instruction 505, carried from the card's map on every card).
        var window = Realized(records, 1280);
        var shown = (AchievementsViewModel)window.DataContext!;

        try
        {
            shown.OpenCategoryCommand.Execute(AchievementKinds.Countries);
            Settle(window);

            var list = Named<ItemsControl>(window, "AchievementsCategoryCards");
            var maps = TradingCards(window).Count(HasMap);
            var words = VisibleText(list).Count(t => t == "no grid, so no map");

            _output.WriteLine("window: " + maps + " rows open a map, " + words + " no-map words");

            Assert.Equal(shown.Category!.Cards.Count(c => c.HasMap), maps);
            Assert.Equal(1, words);
        }
        finally
        {
            window.Close();
        }

        // **WORK INSTRUCTION 342 RULINGS 11 AND 12 ARE RETIRED HERE** (work instruction 505, HM-DEC-209):
        // they measured the map across the card at 231 px and its crop to the two stations, and no map is
        // drawn on a row. The one map in the window is the popup, which is ruling 16's and the
        // conversation card's opened control, measured by `TheMapOpensTests`.

        // **WORK INSTRUCTION 345 TASK 2, RULING 21: AT 1400 AND 1920 EVERY EARNED CARD ON COUNTRIES,
        // STATES AND GRIDS DRAWS THE CONTACT THAT EARNED IT** - entity, callsign, grid, distance, band,
        // mode, date and points, each from the log entry rather than from the card - and where the log
        // lacks a fact, what is there, no dash and no map without a grid (§6). The
        // window check above stays. `StatesCountWhatTheLogsStateFieldSays` measures States at both widths for fit and white
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
    /// **Work instruction 342 task 3, the nice-to-pass: a row's map opens in its popup on a click,
    /// never on a hover, and a click outside closes it** (ruling 16), at 1400 and 1920.
    /// </summary>
    /// <remarks>
    /// <para>**CARRIED BY WORK INSTRUCTION 505** from `ACardsMapOpensInItsPopupOnAClickAndAClickOutsideClosesIt`:
    /// the click lands on the row, which is the thing he presses now, where it landed on the card's
    /// map. Every other assertion is unchanged.</para>
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
    public void ARowsMapOpensInItsPopupOnAClickAndAClickOutsideClosesIt(double width)
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
                    .First(b => b.IsEffectivelyVisible && b.Classes.Contains("category-card")
                        && b.DataContext is AchievementCategoryCard { OpensAMap: true });
                var data = (AchievementCategoryCard)card.DataContext!;
                var map = card;
                var state = F(width) + " " + data.Title;

                List<Popup> Open() => window.GetVisualDescendants().OfType<Popup>().Where(p => p.IsOpen).ToList();

                // **CLOSED AT START.**
                Assert.Empty(Open());

                var centre = map.TranslatePoint(new Point(map.Bounds.Width / 2, map.Bounds.Height / 2), window);

                Assert.True(centre.HasValue, state + ": the row has no place on the window");

                // **A HOVER OPENS NOTHING.**
                window.MouseMove(centre!.Value);
                Settle(window);

                Assert.True(Open().Count == 0, state + ": a hover over the row opened a popup");

                // **A CLICK OPENS IT, HOLDING THIS CARD'S PATH.**
                window.MouseDown(centre.Value, MouseButton.Left);
                window.MouseUp(centre.Value, MouseButton.Left);
                Settle(window);

                var open = Open();

                Assert.True(open.Count == 1, state + ": a click on the row opened " + open.Count + " popups");

                var popup = open[0];
                var opened = popup.Child!.GetVisualDescendants().OfType<Ft8GlobeControl>().Single();

                _output.WriteLine(
                    state + ": opened " + opened.Plot?.Callsign + ", map " + F(opened.Bounds.Width) + " x " + F(opened.Bounds.Height)
                    + ", frame " + F(opened.DrawnFrame.Width) + " by " + F(opened.DrawnFrame.Height));

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

        // **WORK INSTRUCTION 346 TASK 1, RULINGS 25 AND 26: CW LIVES WHERE A BAND BUTTON LANDS**
        // (HM-DEC-110), on the best-bet band, in `LivesAt`'s form, and the CQ list carries no Morse.
        var landing = HfBands.Bands.FirstOrDefault(b => b.Name == bet.Band) ?? HfBands.Bands[0];
        var cwLine = (landing.JumpHz / 1_000_000.0).ToString("0.000", CultureInfo.InvariantCulture) + " on " + landing.Name
            + " · " + AchievementCategory.NoMorseOnTheList;

        _output.WriteLine("  modes CW row [" + modes[^1].Callers.Single(c => c.Place == "CW").CallLine + "], wanted [" + cwLine + "]");

        Assert.Equal(cwLine, modes[^1].Callers.Single(c => c.Place == "CW").CallLine);
        Assert.StartsWith("3.575 on 80 m", modes[^1].Callers.Single(c => c.Place == "FT4").CallLine, StringComparison.Ordinal);
        Assert.StartsWith("3.580 on 80 m", modes[^1].Callers.Single(c => c.Place == "PSK31").CallLine, StringComparison.Ordinal);

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
                    var card = badge.GetVisualDescendants().OfType<Border>().Single(b => b.IsEffectivelyVisible && b.Classes.Contains("category-card"));

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
                    .Select(b => b.GetVisualDescendants().OfType<Border>().Single(c => c.IsEffectivelyVisible && c.Classes.Contains("category-card")))
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
                Assert.Equal(cwLine, rows.Single(r => r.Place == "CW").CallLine);
                Assert.StartsWith("3.575 on 80 m", rows.Single(r => r.Place == "FT4").CallLine, StringComparison.Ordinal);
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
        var watched = false;
        var watchedRow = false;

        static bool Filled(Border card) => RowFilled(card);

        foreach (var width in new[] { 1400.0, 1920.0 })
        {
            // **WORK INSTRUCTION 346 TASK 2, RULING 27: EVERY PAGE STEP 1'S DRAWN TESTS REALIZE** - the eight
            // kinds and all seven continent pages on the twelve contacts, and the Modes page on the five
            // contacts with a PSK31 caller on the list, where its next card and its longest row are drawn.
            // States on its own log is measured in `StatesCountWhatTheLogsStateFieldSays`.
            foreach (var (records, label, calling, kinds) in new[]
            {
                (TheAchievementsPageTests.TwelveContacts(), "twelve contacts", Calling(), AchievementKinds.All.Concat(ContinentKinds()).ToList()),
                (FiveContacts(), "five contacts and a PSK31 caller", CallingWithPsk31(), new List<string> { AchievementKinds.Modes }),

                // **WORK INSTRUCTION 347 TASK 2**: the longest PSK31 line, with two PSK31 callers, on Modes and on
                // Europe where the caller is listed; and the no-caller words with none.
                (FiveContacts(), "five contacts and two PSK31 callers", CallingWith(Psk31WithNoGrid, CertainPsk31WithGrid),
                    new List<string> { AchievementKinds.Modes, AchievementCategory.ContinentPrefix + "EU" }),
                (FiveContacts(), "five contacts and no PSK31 caller", Calling(), new List<string> { AchievementKinds.Modes }),

                // **WORK INSTRUCTION 347 TASK 3, RULING 32: EVERY ROW THE MODES NEXT CARD CAN DRAW.** FT4 alone draws
                // CW, FT8 and PSK31 before its more line; CW and FT4 draw FT8, PSK31 and Voice. Each with two PSK31
                // callers on the list, and with no list, where PSK31 says the list was not read.
                (Ft4Only(), "FT4 only and two PSK31 callers", CallingWith(Psk31WithNoGrid, CertainPsk31WithGrid), new List<string> { AchievementKinds.Modes }),
                (Ft4Only(), "FT4 only and no list", CqSnapshot.None, new List<string> { AchievementKinds.Modes }),
                (CwAndFt4(), "CW and FT4 and two PSK31 callers", CallingWith(Psk31WithNoGrid, CertainPsk31WithGrid), new List<string> { AchievementKinds.Modes }),
                (CwAndFt4(), "CW and FT4 and no list", CqSnapshot.None, new List<string> { AchievementKinds.Modes }),
            })
            {
                var window = Realized(records, width, calling, bet);
                var screen = (AchievementsViewModel)window.DataContext!;
                var tallest = (Height: 0.0, Kind: "");

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

                        var state = F(width) + " " + label + " " + kind;
                        var runs = Fits(window, state);
                        var cards = window.GetVisualDescendants().OfType<Border>()
                            .Where(b => b.IsEffectivelyVisible && b.Classes.Contains("category-card"))
                            .ToList();

                        Assert.True(cards.Count > 0, state + ": no trading card is drawn");

                        var white = 0;

                        foreach (var card in cards.Where(c => !Filled(c)))
                        {
                            white++;
                            _output.WriteLine("  white card: " + string.Join(" | ", VisibleText(card)));
                        }

                        var page = window.GetVisualDescendants().OfType<ItemsControl>()
                            .First(i => i.IsEffectivelyVisible && (i.Name == "AchievementsCategoryCards" || i.Name == "AchievementsSubBadges"));

                        if (page.Bounds.Height > tallest.Height)
                        {
                            tallest = (page.Bounds.Height, kind);
                        }

                        _output.WriteLine(
                            "  " + kind.PadRight(14) + runs + " runs fit, " + cards.Count + " cards, " + white + " white, page "
                            + F(page.Bounds.Height) + " px tall");

                        Assert.True(white == 0, state + ": " + white + " row(s) or panel(s) with nothing but a title");

                        // **WORK INSTRUCTION 347 TASK 3: EACH MODES ROW DRAWN, AND HOW MANY BEFORE *AND N MORE*.** The
                        // rows are measured by `Fits` above with every other run on the page.
                        if (kind == AchievementKinds.Modes
                            && cards.FirstOrDefault(c => c.DataContext is AchievementCategoryCard { Earned: false }) is { } modesNext)
                        {
                            var rows = CallerRows(modesNext);
                            var more = ((AchievementCategoryCard)modesNext.DataContext!).MoreCallersLine;

                            _output.WriteLine(
                                "    modes rows: " + rows.Count + " drawn before [" + more + "]: "
                                + string.Join(" / ", rows.Select(r => r.Place + " [" + r.CallLine + "]")));

                            if (!watchedRow)
                            {
                                // **RULING 19, WATCHED RED ON THE TEST WINDOW ONLY**: the longest row's call line held
                                // in a slot 10 px narrower than it needs, then let go.
                                var line = modesNext.GetVisualDescendants().OfType<TextBlock>()
                                    .Where(t => t.IsEffectivelyVisible && t.Classes.Contains("card-caller-call"))
                                    .OrderByDescending(t => (t.Text ?? "").Length)
                                    .First();
                                var needs = Unit342Needs(line, line.Text ?? "");

                                // **UNDER ITS LONGEST WORD SINCE WORK INSTRUCTION 506**: the line may wrap between words.
                                line.MaxWidth = LongestWord(line) - 10;
                                Settle(window);

                                var narrowed = Unit346Fit(window).Clips;

                                _output.WriteLine(
                                    "    " + state + " [" + line.Text + "] held to " + F(needs - 10) + " px on the test window, watched red: "
                                    + (narrowed.Count == 0 ? "fits" : string.Join("; ", narrowed)));

                                Assert.Contains(narrowed, c => c.StartsWith("[" + line.Text + "]", StringComparison.Ordinal));

                                line.MaxWidth = double.PositiveInfinity;
                                Settle(window);

                                Assert.Empty(Unit346Fit(window).Clips);
                                watchedRow = true;
                            }
                        }

                        // **THE MODES PAGE ON THE FIVE CONTACTS DRAWS ITS NEXT CARD**, or it measures nothing new.
                        if (calling.Calls.Any(c => c.Mode == "PSK31"))
                        {
                            Assert.Contains(cards, c => c.DataContext is AchievementCategoryCard { Earned: false });
                        }

                        if (!watched)
                        {
                            // **RULING 19, WATCHED RED ON THE TEST WINDOW ONLY**: one card with its map, bar and
                            // list hidden is counted white, and then shown again.
                            var card = cards[0];
                            var hidden = card.GetVisualDescendants().OfType<Control>()
                                .Where(v => v.IsVisible && IsFilling(v))
                                .ToList();

                            hidden.ForEach(v => v.IsVisible = false);
                            Settle(window);

                            var counted = Filled(card) ? "not white" : "white";

                            _output.WriteLine(
                                "  " + state + " [" + ((AchievementCategoryCard)card.DataContext!).Title + "] with its facts, bar and list hidden on the test window ("
                                + hidden.Count + " hidden), watched red: counted " + counted);

                            Assert.Equal("white", counted);

                            hidden.ForEach(v => v.IsVisible = true);
                            Settle(window);

                            Assert.True(Filled(card), state + ": the card is not filled again once shown");
                            watched = true;
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

                _output.WriteLine("  TALLEST at " + F(width) + ", " + label + ": " + tallest.Kind + ", " + F(tallest.Height) + " px");
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
    /// <para>**EXTENDED BY WORK INSTRUCTION 348 TASK 1** (rulings 34 and 35): the badge and the band say
    /// `2 worked, from STATE`, drawn at both widths; the next card counts the US records carrying no
    /// `STATE`; and no run or hover on the page, the eight kinds or the seven continent pages, and no
    /// string outside a comment in an achievements view model, contains *confirm*. Watched red on the
    /// tree as it was: `Expected: "2 worked, from STATE"` against `2 worked`.</para>
    /// <para>**EXTENDED BY WORK INSTRUCTION 349 TASK 3** (ruling 45): on a state log whose US records
    /// with no `STATE` number 1,234, the size a real log draws, the next card draws `1,234 US contacts
    /// carry no STATE` whole, with its thousands separator, at 1400 and 1920, and nothing on the page
    /// clips or wraps. Watched red built in, on the test window only: the same drawn line held to half
    /// the width it needs is caught by the page's own clip measure, and a line without the separator is
    /// reported not drawn.</para>
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

        // **WORK INSTRUCTION 348 RULING 35: THE COUNT SAYS WHAT IT COUNTS** - states worked, read from the
        // log's `STATE` field - in the fewest words that fit.
        const string counted = "2 worked, from STATE";
        const string noStateLine = "1 US contact carries no STATE";

        Assert.Equal(counted, badge.Standing);

        // **THE PAGE DRAWS TWO EARNED CARDS, EACH THE CONTACT THAT EARNED IT.**
        screen.OpenCategoryCommand.Execute(AchievementKinds.States);

        // **THE BAND SAYS THE SAME WORDS, SO THE SAME NUMBER** (ruling 35).
        Assert.Equal(badge.Standing, screen.Category!.Standing);
        Assert.Contains(counted, screen.Category.BandLine.Split(" · "));

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

        // **RULING 35: HOW MANY US RECORDS CARRY NO `STATE`** - W1AW here - which says nothing about
        // where that station is. N3DC's `DC` is a STATE that scores nothing, so it is not counted.
        Assert.Equal(noStateLine, next.CountLine);

        // **MEASURED AT 1400 AND 1920 NOW THAT STATES HAS CARDS**: nothing clips or wraps, and no
        // card is white.
        foreach (var width in new[] { 1400.0, 1920.0 })
        {
            var window = Realized(records, width);
            var shown = (AchievementsViewModel)window.DataContext!;

            try
            {
                // **AS DRAWN** (ruling 35): the badge's corner, the band's line and the next card.
                var drawnBadge = Named<ItemsControl>(window, "AchievementsBadges").GetVisualDescendants().OfType<Button>()
                    .First(b => b.DataContext is AchievementBadge { Kind: AchievementKinds.States });

                // **CARRIED BY WORK INSTRUCTION 506**: the tile draws the count large and what it counts
                // beside it, so ruling 35's words are split across the two - `2` and `states, from STATE` -
                // and still say the count is read from the log's STATE field.
                Assert.Contains("2", VisibleText(drawnBadge));
                Assert.Contains("states, from STATE", VisibleText(drawnBadge));

                shown.OpenCategoryCommand.Execute(AchievementKinds.States);
                Settle(window);

                Assert.Contains(counted, Named<TextBlock>(window, "AchievementsCategoryBandLine").Text!.Split(" · "));

                var drawnNext = TradingCards(window).Single(c => c.DataContext is AchievementCategoryCard { Earned: false });

                Assert.Null(CardMiss(drawnNext, "Any state you have not worked", AchievementCategory.NoStateFromTheAir, noStateLine));

                var state = F(width) + " states";
                var runs = Fits(window, state);
                var cards = window.GetVisualDescendants().OfType<Border>()
                    .Where(b => b.IsEffectivelyVisible && b.Classes.Contains("category-card"))
                    .ToList();
                var white = cards.Count(card => !RowFilled(card));

                _output.WriteLine(state + ": " + runs + " runs fit, " + cards.Count + " cards, " + white + " white");

                Assert.Equal(3, cards.Count);
                Assert.Equal(0, white);
            }
            finally
            {
                window.Close();
            }
        }

        // **WORK INSTRUCTION 349 RULING 45: THE NO-STATE LINE AT FOUR DIGITS**, the size Tim's own log will
        // draw, measured the way step 1's clip test measures rather than left as arithmetic.
        const string thousandsLine = "1,234 US contacts carry no STATE";

        foreach (var width in new[] { 1400.0, 1920.0 })
        {
            var window = Realized(StateContactsWithThousandsNoState(), width);
            var shown = (AchievementsViewModel)window.DataContext!;

            try
            {
                shown.OpenCategoryCommand.Execute(AchievementKinds.States);
                Settle(window);

                var drawnNext = TradingCards(window).Single(c => c.DataContext is AchievementCategoryCard { Earned: false });

                Assert.Null(CardMiss(drawnNext, thousandsLine));

                var state = F(width) + " states, 1,234 US records with no STATE";
                var runs = Fits(window, state);
                var line = drawnNext.GetVisualDescendants().OfType<TextBlock>()
                    .Single(t => t.IsEffectivelyVisible && t.Text == thousandsLine);

                _output.WriteLine(
                    state + ": [" + thousandsLine + "] drawn " + F(line.Bounds.Width) + " px, needs "
                    + F(Unit342Needs(line, thousandsLine)) + "; " + runs + " runs fit");

                // **WATCHED RED, BUILT IN (ruling 19), ON THE TEST WINDOW ONLY**: the same line held to half the
                // width it needs is caught by the page's clip measure, and then let go again.
                // **SINCE WORK INSTRUCTION 506 THE LINE MAY WRAP BETWEEN WORDS** in the next stamp, so it is held
                // under its longest word, which is the one thing it may never do.
                line.MaxWidth = LongestWord(line) - 10;
                Settle(window);

                var caught = Unit346Fit(window).Clips.Where(c => c.Contains(thousandsLine, StringComparison.Ordinal)).ToList();

                _output.WriteLine("WATCHED RED " + F(width) + ": " + (caught.Count == 0 ? "NOT CAUGHT" : string.Join("; ", caught)));
                Assert.NotEmpty(caught);

                line.MaxWidth = double.PositiveInfinity;
                Settle(window);
                Assert.Empty(Unit346Fit(window).Clips);

                // **AND THE SEPARATOR IS WHAT IS ASSERTED**: the line without it is not drawn.
                var withoutSeparator = CardMiss(drawnNext, "1234 US contacts carry no STATE");

                _output.WriteLine("WATCHED RED " + F(width) + ": " + (withoutSeparator ?? "NOT CAUGHT"));
                Assert.NotNull(withoutSeparator);
            }
            finally
            {
                window.Close();
            }
        }

        // **RULING 34: NO `confirm` IN ANY RUN OR HOVER THE ACHIEVEMENTS WINDOW DRAWS** - the page, the
        // eight kinds and the seven continent pages, at 1400 and 1920, on this log and the twelve contacts.
        foreach (var (fixture, label) in new (IReadOnlyList<AdifLogRecord> Records, string Label)[]
        {
            (records, "state contacts"),
            (TheAchievementsPageTests.TwelveContacts(), "twelve contacts"),
        })
        {
            foreach (var width in new[] { 1400.0, 1920.0 })
            {
                var window = Realized(fixture, width);
                var shown = (AchievementsViewModel)window.DataContext!;
                var found = new List<string>();

                try
                {
                    found.AddRange(Unit348Said(window).Where(s => s.Contains("confirm", StringComparison.OrdinalIgnoreCase)).Select(s => "page [" + s + "]"));

                    foreach (var kind in AchievementKinds.All.Concat(ContinentKinds()))
                    {
                        OpenOnWindow(window, shown, kind);
                        found.AddRange(Unit348Said(window).Where(s => s.Contains("confirm", StringComparison.OrdinalIgnoreCase)).Select(s => kind + " [" + s + "]"));
                        ToThePage(window, shown);
                    }
                }
                finally
                {
                    window.Close();
                }

                _output.WriteLine("confirm drawn at " + F(width) + ", " + label + ": " + found.Count);

                Assert.True(found.Count == 0, F(width) + " " + label + " draws " + string.Join("; ", found));
            }
        }

        // **AND NONE IN A STRING AN ACHIEVEMENTS VIEW MODEL HOLDS** (ruling 34): every line of
        // `src/Hamlet.App/ViewModels/Achievement*.cs` that is not a comment.
        var held = System.IO.Directory.GetFiles(
                System.IO.Path.Combine(Unit348Root(), "src", "Hamlet.App", "ViewModels"), "Achievement*.cs")
            .SelectMany(file => System.IO.File.ReadAllLines(file).Select((line, i) => (File: System.IO.Path.GetFileName(file), Line: i + 1, Text: line)))
            .Where(l => !l.Text.TrimStart().StartsWith("//", StringComparison.Ordinal)
                && l.Text.Contains("confirm", StringComparison.OrdinalIgnoreCase))
            .Select(l => l.File + ":" + l.Line + " " + l.Text.Trim())
            .ToList();

        Assert.True(held.Count == 0, "confirm held in a string: " + string.Join(" | ", held));
    }

    /// <summary>
    /// **A row or panel that is not white** (work instruction 505, carried from the card's measure of a
    /// map, a bar or a list): a bar with width, a list with text, or on an earned row a fact beside its
    /// title - the call line, the distance, the band and mode or date, a count, or the word saying
    /// whether it opens a map.
    /// </summary>
    private static bool RowFilled(Border card)
        => card.GetVisualDescendants().OfType<Control>().Any(v => v.IsEffectivelyVisible && IsFilling(v)
            && (v is not BadgeProgressControl || v.Bounds.Width > 0)
            && (v is not Border list || VisibleText(list).Any())
            && (v is not TextBlock t || (t.Text ?? "").Trim().Length > 0));

    /// <summary>True where the control is one of the things that keep a row or panel from being white.</summary>
    private static bool IsFilling(Control control)
        => control is BadgeProgressControl
            || (control is Border b && b.Classes.Contains("card-list"))
            || (control is TextBlock t && new[] { "card-callgrid", "card-distance", "card-facts", "card-count", "card-nomap", "card-open" }.Any(t.Classes.Contains));

    /// <summary>
    /// **Task 5: Countries, Continents and Total Miles at 1280, 1400 and 1920 wide, described as
    /// computed** - how many rows show whole, how many rows of that height the list's viewport holds
    /// below the panel, and what clips. It asserts only that nothing clips.
    /// </summary>
    [AvaloniaFact]
    public void ThreePagesAtThreeWidthsDescribed()
    {
        foreach (var width in new[] { 1280.0, 1400.0, 1920.0 })
        {
            var window = Realized(TheAchievementsPageTests.TwelveContacts(), width, Calling(), new BandBet("17 m", "best bet now"));
            var screen = (AchievementsViewModel)window.DataContext!;

            try
            {
                foreach (var kind in new[] { AchievementKinds.Countries, AchievementKinds.Continents, AchievementKinds.TotalMiles })
                {
                    OpenOnWindow(window, screen, kind);

                    var list = window.GetVisualDescendants().OfType<ItemsControl>()
                        .First(i => i.IsEffectivelyVisible && (i.Name == "AchievementsCategoryCards" || i.Name == "AchievementsSubBadges"));
                    var viewport = list.GetVisualAncestors().OfType<ScrollViewer>().First();
                    var drawn = list.GetVisualDescendants().OfType<Border>()
                        .Where(b => b.IsEffectivelyVisible && b.Classes.Contains("category-card"))
                        .ToList();
                    var rows = drawn.Where(b => b.DataContext is AchievementCategoryCard { Earned: true }).ToList();
                    var panel = drawn.Where(b => b.DataContext is AchievementCategoryCard { Earned: false }).Sum(b => b.Bounds.Height + 8);
                    var pitch = rows.Count > 0 ? rows.Average(r => r.Bounds.Height) + 8 : double.NaN;
                    var holds = double.IsNaN(pitch) ? 0 : (int)Math.Floor((viewport.Bounds.Height - (kind == AchievementKinds.Continents ? 0 : panel)) / pitch);
                    var clips = Unit346Fit(window).Clips;

                    _output.WriteLine(
                        F(width) + " x " + F(window.Bounds.Height) + " " + kind.PadRight(12) + drawn.Count + " drawn, " + rows.Count(r => Inside(r, viewport))
                        + " earned rows whole of " + rows.Count + "; viewport " + F(viewport.Bounds.Height) + " px, next panel " + F(panel)
                        + " px, rows " + F(pitch - 8) + " px, room for " + holds + " earned rows without scrolling; "
                        + (clips.Count == 0 ? "nothing clips" : string.Join("; ", clips)));

                    Assert.Empty(clips);

                    ToThePage(window, screen);
                }
            }
            finally
            {
                window.Close();
            }
        }
    }

    /// <summary>The rows and panels the open category draws in its list, top to bottom.</summary>
    private static List<Border> CategoryCards(Window window)
        => Named<ItemsControl>(window, "AchievementsCategoryCards").GetVisualDescendants().OfType<Border>()
            .Where(b => b.IsEffectivelyVisible && b.Classes.Contains("category-card"))
            .OrderBy(b => Math.Round(Top(b, window)))
            .ToList();

    /// <summary>The visible button a row is drawn inside, or null where it is not a button.</summary>
    private static Button? RowButton(Border row)
        => row.GetVisualAncestors().OfType<Button>().FirstOrDefault(b => b.IsEffectivelyVisible && b.Classes.Contains("category-row"));

    /// <summary>The scroller the list sits in.</summary>
    private static ScrollViewer CardsScroller(Window window)
        => Named<ItemsControl>(window, "AchievementsCategoryCards").GetVisualAncestors().OfType<ScrollViewer>().First();

    /// <summary>True where the control lies whole inside the scroller's viewport.</summary>
    private static bool Inside(Control control, ScrollViewer viewport)
    {
        var top = control.TranslatePoint(new Point(0, 0), viewport)?.Y ?? double.NaN;

        return top >= -0.5 && top + control.Bounds.Height <= viewport.Bounds.Height + 0.5;
    }

    /// <summary>The width the widest single word of a run needs (work instruction 506).</summary>
    private static double LongestWord(TextBlock text)
        => (text.Text ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries).Max(w => Unit342Needs(text, w));

    private static double Unit342Needs(TextBlock text, string what)
        => new Avalonia.Media.TextFormatting.TextLayout(
            what,
            new Avalonia.Media.Typeface(text.FontFamily, text.FontStyle, text.FontWeight),
            text.FontSize,
            null).Width;

    private static List<string> Unit348Said(Window window)
        => window.GetVisualDescendants().OfType<TextBlock>()
            .Where(t => t.IsEffectivelyVisible && (t.Text ?? "").Trim().Length > 0)
            .Select(t => t.Text!)
            .Concat(window.GetVisualDescendants().OfType<Control>()
                .Where(c => c.IsEffectivelyVisible)
                .Select(c => ToolTip.GetTip(c) as string)
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(s => s!))
            .ToList();

    /// <summary>The repository root: the folder holding `Hamlet.sln`.</summary>
    private static string Unit348Root()
    {
        var at = new System.IO.DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !System.IO.File.Exists(System.IO.Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName ?? throw new InvalidOperationException("no Hamlet.sln above " + AppContext.BaseDirectory);
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
            if (TheAchievementTilesTests.WrapsBetweenWordsInAStamp(text))
            {
                fit++;
                continue;
            }

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

    /// <summary>
    /// **Work instruction 346 task 1, rulings 25 and 26**: null where every Modes row drawn has a place and
    /// a line, and the line is where that mode lives then who is there - CW at the place a band button
    /// lands on the best-bet band or the lowest band, a digital mode at its calling row on the same rule;
    /// then the no-Morse words for CW, the words that the list cannot tell FT4 from FT8 for FT4, and for a
    /// mode the list's rows do say the nearest caller in it with his distance where the list holds his
    /// grid and how many more, or that no one was calling CQ in it when the list was read (work instruction
    /// 347 ruling 31). Otherwise what is wrong.
    /// </summary>
    private static string? ModesRowMiss(IReadOnlyList<NextCaller> rows, CqSnapshot calling, BandBet bet)
    {
        static string Mhz(long hz) => (hz / 1_000_000.0).ToString("0.000", CultureInfo.InvariantCulture);

        static double? Miles(string grid)
            => OperatorLocation.FromGrid(MyGrid) is { } a && OperatorLocation.FromGrid(grid) is { } b ? GridPath.MilesBetween(a, b) : null;

        var shown = " (rows drawn: " + string.Join(" / ", rows.Select(r => r.Place + " [" + r.CallLine + "]")) + ")";

        foreach (var row in rows)
        {
            if (row.Place.Trim().Length == 0 || row.CallLine.Trim().Length == 0)
            {
                return "the " + (row.Place.Trim().Length == 0 ? "row with no place" : row.Place + " row") + " draws an EMPTY line" + shown;
            }

            string lives;

            if (row.Place == "CW")
            {
                var band = HfBands.Bands.FirstOrDefault(b => b.Name == bet.Band) ?? HfBands.Bands[0];

                lives = Mhz(band.JumpHz) + " on " + band.Name;
            }
            else if (string.Equals(row.Place, ContactModes.OliviaName, StringComparison.OrdinalIgnoreCase))
            {
                // **§R12, WORK INSTRUCTION 368.** Olivia has no block in the band plan - its
                // calling spots are its own cited table, `data/bands/olivia-calling.json`, the
                // same one the tab tunes from - so this helper asked `DigitalCallingFrequencies`
                // for a list that is empty and took its first element. **The rule rulings 25 and
                // 26 wrote is unchanged**: every row still says where its mode lives; what
                // changed is that Olivia is now one of the modes there are to work.
                var table = Hamlet.RadioEngine.Olivia.OliviaData.Current.Calling!;
                var where = table.CallingRowFor(bet.Band) ?? table.Rows[0];

                lives = Mhz(table.DialHzFor(where)!.Value) + " on " + where.Band;
            }
            else
            {
                var home = DigitalCallingFrequencies.BandsWith(row.Place);
                var on = home.Contains(bet.Band, StringComparer.Ordinal) ? bet.Band : home[0];

                lives = Mhz(DigitalCallingFrequencies.Find(on, row.Place)!.JumpHz) + " on " + on;
            }

            var callers = calling.Calls.Where(c => c.Mode == row.Place).OrderBy(c => Miles(c.Grid) ?? double.MaxValue).ToList();
            var who = row.Place switch
            {
                "CW" => AchievementCategory.NoMorseOnTheList,
                "FT4" => "the CQ list cannot tell FT4 from FT8",

                // **§R12, WORK INSTRUCTION 368**: the list reads a text-only row as PSK31 and an
                // Olivia row is a text-only row, so it cannot tell the two keyboard modes apart -
                // the same kind of fact the FT4 row states about FT8.
                "Olivia" => AchievementCategory.ListCannotTellOlivia,
                _ when callers.Count == 0 => "no one calling at " + calling.ReadAt,
                _ => callers[0].Callsign
                    + (Miles(callers[0].Grid) is { } mi ? " · " + GridPath.DescribeMiles(mi).Replace(" miles", " mi", StringComparison.Ordinal) : "")
                    + (callers.Count > 1 ? " and " + (callers.Count - 1).ToString(CultureInfo.InvariantCulture) + " more" : ""),
            };
            var wanted = lives + " · " + who;

            if (row.CallLine != wanted)
            {
                return "the " + row.Place + " row draws [" + row.CallLine + "], not [" + wanted + "]" + shown;
            }
        }

        return null;
    }

    /// <summary>The rows and the next stamp the open category draws, top to bottom and left to right (work instruction 506: the next stamp stands in its own column).</summary>
    private static List<Border> TradingCards(Window window)
        => Named<Control>(window, "AchievementsCategory").GetVisualDescendants().OfType<Border>()
            .Where(b => b.IsEffectivelyVisible && b.Classes.Contains("category-card"))
            .OrderBy(b => Math.Round(Top(b, window)))
            .ThenBy(b => b.TranslatePoint(new Point(0, 0), window)?.X ?? 0)
            .ToList();

    /// <summary>
    /// True where the row opens a map when pressed: it is drawn inside the visible `category-row` button (work
    /// instruction 505; on the card this was a map drawn with a plot and width).
    /// </summary>
    private static bool HasMap(Border card)
        => RowButton(card) is not null;

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
    /// **Work instruction 349 task 3: the state log with 1,234 US records carrying no `STATE`** -
    /// `StateContacts()`, whose W1AW is one, and 1,233 more US calls, `W1AAA` onward, with none.
    /// </summary>
    internal static IReadOnlyList<AdifLogRecord> StateContactsWithThousandsNoState()
    {
        var text = new System.Text.StringBuilder(AdifLog.Header("test"));

        for (var i = 0; i < 1233; i++)
        {
            var started = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc).AddMinutes(3 * i);

            text.Append(AdifLog.Record(new AdifContact
            {
                Call = "W1" + (char)('A' + (i / 676 % 26)) + (char)('A' + (i / 26 % 26)) + (char)('A' + (i % 26)),
                StationCallsign = "KC3QIS",
                Mode = "FT8",
                Band = "20m",
                GridSquare = "FN31",
                MyGridSquare = MyGrid,
                StartedUtc = started,
                EndedUtc = started.AddMinutes(2),
            }));
        }

        return StateContacts().Concat(AdifLog.ReadRecords(text.ToString())).ToList();
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
            // **A STAMP'S SENTENCE MAY WRAP BETWEEN WORDS** (work instruction 506 task 4), and nowhere else.
            if (TheAchievementTilesTests.WrapsBetweenWordsInAStamp(text))
            {
                count++;
                continue;
            }

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

    /// <summary>A certain PSK31 CQ that carries a grid: Barcelona's square (work instruction 347).</summary>
    private const string CertainPsk31WithGrid = "CQ CQ CQ de EA3ABC EA3ABC JN11 K";

    /// <summary>The same CQ with its turnover gone, so the parser reads the grid and is not certain.</summary>
    private const string UncertainPsk31WithGrid = "CQ CQ CQ de EA3ABC EA3ABC JN11";

    /// <summary>`CallingWithPsk31()`'s own text, with no grid.</summary>
    private const string Psk31WithNoGrid = "CQ CQ CQ de EA3XYZ EA3XYZ K";

    /// <summary>`Calling()`'s four, and a PSK31 row for each text, read by the PSK31 parser as the list builds one.</summary>
    private static CqSnapshot CallingWith(params string[] psk31Texts)
        => CqSnapshot.From(
            new[]
            {
                Heard("CQ OE8DDX JN76"),
                Heard("CQ DX J38DX FK92"),
                Heard("CQ K1ABC FN42"),
                Heard("CQ ZL1ABC RF72"),
            }.Concat(psk31Texts.Select(text => new DigitalDecodeRow(
                "214100", "+10", DigitalDecodeRow.NotMeasured, "1000", text,
                IsTextOnly: true, Reading: Hamlet.RadioEngine.Psk31.Psk31ExchangeParser.Read(text, "KC3QIS")))),
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

    /// <summary>One FT4 contact, so CW, FT8, PSK31 and Voice are unworked (work instruction 347 task 3).</summary>
    private static IReadOnlyList<AdifLogRecord> Ft4Only()
        => new[]
        {
            Record("LA8ENA", "MFSK", "FT4", "20m", "JO59", new DateTime(2026, 8, 17, 21, 41, 30, DateTimeKind.Utc)),
        };

    /// <summary>A CW and an FT4 contact, so FT8, PSK31 and Voice are unworked (work instruction 347 task 3).</summary>
    private static IReadOnlyList<AdifLogRecord> CwAndFt4()
        => new[]
        {
            Record("W3YNI", "CW", null, "40m", "FN20", new DateTime(2026, 8, 14, 21, 41, 30, DateTimeKind.Utc)),
            Record("LA8ENA", "MFSK", "FT4", "20m", "JO59", new DateTime(2026, 8, 17, 21, 41, 30, DateTimeKind.Utc)),
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
