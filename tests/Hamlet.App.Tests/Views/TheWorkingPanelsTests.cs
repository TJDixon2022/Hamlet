using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
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
/// Work instruction 337 task 2: **the working panels are the window** - waterfall, decoded text
/// and For You, one top and one bottom, the decoded list as wide as its longest line.
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-12**, on unit 334's window: *the waterfall, the decoded list and For You
/// were squeezed into the bottom third.* R26: *below the tabs, waterfall, decoded text, For You,
/// all the same height, full to the status bar. The decoded list is as wide as its longest line
/// needs and no wider; For You takes the rest, wide enough that the card's facts sit beside its
/// map.*</para>
/// <para>**COMPUTED, NOT SEEN** (§0.0). Every number is read off a realized headless window at
/// <see cref="TheTopRowTests.WindowHeight"/> px tall, which is the unit's own choice. The headless
/// host draws the list's monospace at 10.0 px a character, wider than Consolas on the glass, so
/// a column that holds a line here has headroom there.</para>
/// </remarks>
public sealed class TheWorkingPanelsTests
{
    /// <summary>His call.</summary>
    private const string HisCall = "KC3QIS";

    private const string Station = "K9XP";

    /// <summary>Vienna - a long path, so the card's widest fact row renders.</summary>
    private const string FarGrid = "JN88";

    /// <summary>
    /// **The longest line the decoded list has to hold** - the fixture `ThePanelsMakeRoomTests`
    /// sized the column to: two callsigns, one with a suffix, and a signed report with a roger.
    /// </summary>
    private const string LongestLine = "VP2MAA/P KC3QIS R-09";

    /// <summary>
    /// **The row's fixed columns and the panel's chrome, from the markup's own arithmetic**: 24
    /// px of quill gutter, 76 for `utc`, 48 for `snr`, and 35 for the panel's margin, border,
    /// padding and scroller. The message column is measured, not written here.
    /// </summary>
    private const double FixedColumnsAndChrome = 24 + 76 + 48 + 35;

    /// <summary>The decoded panel's own left margin, which is inside its column.</summary>
    private const double DecodedMargin = 5;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where every rectangle is printed.</param>
    public TheWorkingPanelsTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **Assertion 1: the three panels share one top and one bottom, and the bottom is the
    /// working card's floor.**
    /// </summary>
    [AvaloniaFact]
    public void TheThreePanelsShareOneTopAndOneBottom()
    {
        foreach (var width in new[] { 1400.0, 1920.0 })
        {
            var window = Realized(width);

            try
            {
                var workspace = TheTopRowTests.Named<Border>(window, "WorkspaceBoundary");
                var panels = Panels(window);
                var floor = TheTopRowTests.RectIn(workspace, window).Bottom
                    - workspace.Padding.Bottom - workspace.BorderThickness.Bottom;

                _output.WriteLine("WINDOW " + Px(width));

                foreach (var (name, rect) in panels)
                {
                    _output.WriteLine("  " + name.PadRight(10) + Box(rect));
                }

                _output.WriteLine("  working card floor at y " + Px(floor));
                _output.WriteLine("");

                if (width < 1900)
                {
                    continue;
                }

                var top = panels[0].Rect.Top;
                var bottom = panels[0].Rect.Bottom;

                foreach (var (name, rect) in panels)
                {
                    Assert.True(
                        Math.Abs(rect.Top - top) <= 0.5,
                        name + " starts at y=" + Px(rect.Top) + " and the waterfall at y=" + Px(top));
                    Assert.True(
                        Math.Abs(rect.Bottom - bottom) <= 0.5,
                        name + " ends at y=" + Px(rect.Bottom) + " and the waterfall at y=" + Px(bottom));
                }

                Assert.True(
                    bottom >= floor - 0.5,
                    "the panels end at y=" + Px(bottom) + " and the working card's floor is at y="
                    + Px(floor) + ", so they are not full to the status bar");
            }
            finally
            {
                window.Close();
            }
        }
    }

    /// <summary>
    /// **Assertion 2: the decoded list is as wide as its longest line needs, within 10 px, and
    /// the split is printed as pixels and as a fraction.**
    /// </summary>
    [AvaloniaFact]
    public void TheDecodedListIsAsWideAsItsLongestLineNeeds()
    {
        var need = FixedColumnsAndChrome + Measure(LongestLine);

        foreach (var width in new[] { 1400.0, 1920.0 })
        {
            var window = Realized(width);

            try
            {
                var tab = TheTopRowTests.Named<Control>(window, "DigitalPanes");
                var panels = Panels(window);
                var decoded = panels.First(p => p.Name == "decoded").Rect;
                var column = decoded.Width + DecodedMargin;

                _output.WriteLine("WINDOW " + Px(width) + ": the tab's panes are " + Px(tab.Bounds.Width) + " px");

                foreach (var (name, rect) in panels)
                {
                    _output.WriteLine(
                        "  " + name.PadRight(10) + Px(rect.Width).PadLeft(7) + " px = "
                        + (rect.Width / tab.Bounds.Width).ToString("0.000", CultureInfo.InvariantCulture)
                        + " of the panes");
                }

                _output.WriteLine(
                    "  the longest FT8 line [" + LongestLine + "] measures " + Px(Measure(LongestLine))
                    + " px; with the fixed columns and chrome the list needs " + Px(need)
                    + " px and its column is " + Px(column));
                _output.WriteLine("");

                Assert.True(
                    Math.Abs(column - need) <= 10,
                    "at " + Px(width) + " the decoded column is " + Px(column) + " px and its stated need is "
                    + Px(need) + " px");
            }
            finally
            {
                window.Close();
            }
        }
    }

    /// <summary>**Assertion 3: at 1920 the conversation card's facts sit beside its map.**</summary>
    [AvaloniaFact]
    public void AtNineteenTwentyTheCardsFactsSitBesideTheMap()
    {
        var window = Realized(1920);

        try
        {
            var placed = Placement(window);

            _output.WriteLine(placed.Said);

            Assert.True(placed.Beside, "at 1920 the card's facts are not beside its map: " + placed.Said);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>**Assertion 4: no callsign in the decoded list is clipped.**</summary>
    /// <remarks>
    /// **THIS SIDE CLIPS RATHER THAN WRAPPING** (unit 273), so a message cell narrower than its
    /// text is a callsign cut off partway - a station misidentified (§0.0). The need is measured
    /// from the text itself, unconstrained, because a constrained `DesiredSize` reports a fit.
    /// </remarks>
    [AvaloniaFact]
    public void NoCallsignIsClipped()
    {
        var window = Realized(1920);

        try
        {
            var clipped = Clipped(window, _output);

            Assert.True(clipped.Count == 0, "clipped at 1920: " + string.Join("; ", clipped));
        }
        finally
        {
            window.Close();
        }
    }

    // ------------------------------------------------------------------------------------

    /// <summary>Where the card's facts were put against its map, and a sentence saying so.</summary>
    public sealed record Placed(bool Found, bool Beside, bool Under, string Said);

    /// <summary>The three panels, in order, in the window's frame.</summary>
    public static List<(string Name, Rect Rect)> Panels(Window window)
        => new()
        {
            ("waterfall", TheTopRowTests.RectIn(TheTopRowTests.Named<Control>(window, "DigitalWaterfallPanel"), window)),
            ("decoded", TheTopRowTests.RectIn(TheTopRowTests.Named<Control>(window, "DigitalDecodedPanel"), window)),
            ("For You", TheTopRowTests.RectIn(TheTopRowTests.Named<Control>(window, "DigitalMinePanel"), window)),
        };

    /// <summary>Whether the card's table is beside its map or under it.</summary>
    public static Placed Placement(Window window)
    {
        var cards = TheTopRowTests.Named<ItemsControl>(window, "DigitalContactCards");
        var beside = cards.GetVisualDescendants().OfType<Control>().FirstOrDefault(c => c.Name == "CardBeside");
        var globe = cards.GetVisualDescendants().OfType<Hamlet.App.Controls.Ft8GlobeControl>()
            .FirstOrDefault(g => g.IsEffectivelyVisible && g.Bounds.Width > 0);
        var table = cards.GetVisualDescendants().OfType<Control>().FirstOrDefault(c => c.Name == "CardRightColumn");

        if (beside is null || globe is null || table is null)
        {
            return new Placed(false, false, false, "no card, map or table was realized");
        }

        var map = new Rect(globe.TranslatePoint(new Point(0, 0), beside)!.Value, globe.Bounds.Size);
        var facts = new Rect(table.TranslatePoint(new Point(0, 0), beside)!.Value, table.Bounds.Size);

        var isBeside = facts.X >= map.Right - 0.5 && facts.Y < map.Bottom;
        var isUnder = facts.Y >= map.Bottom - 0.5;

        return new Placed(
            true, isBeside, isUnder,
            "card inside " + Px(beside.Bounds.Width) + " px; map " + Box(map) + "; facts " + Box(facts)
            + " -> " + (isBeside ? "BESIDE" : isUnder ? "UNDER" : "NEITHER"));
    }

    /// <summary>Every visible message cell narrower than its own text, as a sentence each.</summary>
    public static List<string> Clipped(Window window, ITestOutputHelper output)
    {
        var clipped = new List<string>();

        foreach (var cell in TheTopRowTests.Named<ItemsControl>(window, "DigitalDecodedRows")
            .GetVisualDescendants().OfType<StackPanel>()
            .Where(p => p.Name == "DecodedMessageCell" && p.IsEffectivelyVisible))
        {
            var text = string.Concat(cell.GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text));
            var need = Measure(text);

            output.WriteLine("  message cell " + Px(cell.Bounds.Width) + " px holds [" + text + "], which needs " + Px(need));

            if (cell.Bounds.Width + 0.51 < need)
            {
                clipped.Add("[" + text + "] needs " + Px(need) + " px in " + Px(cell.Bounds.Width));
            }
        }

        Assert.True(clipped.Count > 0 || window.GetVisualDescendants().OfType<StackPanel>()
            .Any(p => p.Name == "DecodedMessageCell" && p.IsEffectivelyVisible), "no message cell was realized");

        return clipped;
    }

    /// <summary>How wide a line is in the decoded row's own typeface: monospace at 12.</summary>
    public static double Measure(string text)
        => new Avalonia.Media.FormattedText(
            text,
            CultureInfo.InvariantCulture,
            Avalonia.Media.FlowDirection.LeftToRight,
            new Avalonia.Media.Typeface(new Avalonia.Media.FontFamily("Consolas,Menlo,monospace")),
            12,
            Avalonia.Media.Brushes.Black).Width;

    /// <summary>The digital tab, realized, with the longest line on the left and a card on the right.</summary>
    /// <param name="width">How wide the window is.</param>
    public static Window Realized(double width)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = HisCall;
        settings.Operator.GridSquare = "FN00";

        var model = new MainWindowViewModel(settings, null)
        {
            DigitalNewestFirst = false,
            OperatingMode = "Digital",
            DigitalWaterfallExpanded = true,
            DigitalDecodedExpanded = true,
            DigitalMineExpanded = true,
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        model.AddDecodeRowForTests(
            "021100", "-14", "0.1", "1240", LongestLine, Slot("02:11:00"), 14_074_000);

        model.AddDecodeRowForTests(
            "021115", "-09", "0.2", "1310", "CQ DX " + Station + " " + FarGrid,
            Slot("02:11:15"), 14_074_000);

        model.AddSentRowForTests(Station + " " + HisCall + " FN00", Slot("02:11:00"));
        model.RecordSentForTests(Station + " " + HisCall + " FN00", Slot("02:11:00"));

        model.AddDecodeRowForTests(
            "021130", "-09", "0.2", "1240", HisCall + " " + Station + " " + FarGrid,
            Slot("02:11:30"), 14_074_000);

        model.CardsNowForTests = Slot("02:12:00");
        model.RebuildCardsForTests();

        var window = new MainWindow { DataContext = model, Width = width, Height = TheTopRowTests.WindowHeight };

        window.Show();

        for (var i = 0; i < 6; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }

        return window;
    }

    private static DateTime Slot(string at)
        => DateTime.ParseExact(
            "2026-09-08 " + at,
            "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);

    private static string Px(double value)
        => value.ToString("0.0", CultureInfo.InvariantCulture);

    private static string Box(Rect r)
        => Px(r.Width) + " x " + Px(r.Height) + " at " + Px(r.X) + "," + Px(r.Y);
}
