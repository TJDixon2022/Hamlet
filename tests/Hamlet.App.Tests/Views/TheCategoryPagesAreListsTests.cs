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
            var window = Realized(records, 1040);
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
    /// with the distance the largest thing on it after the title; and how many rows show at 1040 x 720.
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
            var window = Realized(records, 1040);
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

                _output.WriteLine(label + ": " + whole + " earned rows show whole at 1040 x " + F(window.Bounds.Height) + " without scrolling, of " + rows.Count);
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
        var window = Realized(FiveContacts(), 1040);
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
    /// **Task 1: no string on any row or panel clips or wraps at 1040, 1400 and 1920 wide**, on the eight
    /// kinds and the seven continent pages.
    /// </summary>
    [AvaloniaFact]
    public void NoStringOnARowClipsAtThreeWidths()
    {
        foreach (var width in new[] { 1040.0, 1400.0, 1920.0 })
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

    /// <summary>The rows and panels the open category draws in its card list, top to bottom.</summary>
    private static List<Border> TradingCards(Window window)
        => Named<ItemsControl>(window, "AchievementsCategoryCards").GetVisualDescendants().OfType<Border>()
            .Where(b => b.IsEffectivelyVisible && b.Classes.Contains("category-card"))
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
