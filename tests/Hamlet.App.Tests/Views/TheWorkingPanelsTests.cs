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
    /// <remarks>
    /// **WORK INSTRUCTION 341 TASK 2: WITH PSK31 CHOSEN AS WELL**, where the offer line shows, at
    /// the widths it already realized and with the assertions it already made. FT8 is put back
    /// before each window closes.
    /// </remarks>
    [AvaloniaFact]
    public void TheThreePanelsShareOneTopAndOneBottom()
    {
        foreach (var (width, mode) in new[] { (1400.0, "FT8"), (1920.0, "FT8"), (1400.0, "PSK31"), (1920.0, "PSK31") })
        {
            var window = Realized(width);
            var model = (MainWindowViewModel)window.DataContext!;

            try
            {
                model.ChosenDigitalMode = mode;
                Settle(window);

                var workspace = TheTopRowTests.Named<Border>(window, "WorkspaceBoundary");
                var panels = Panels(window);
                var floor = TheTopRowTests.RectIn(workspace, window).Bottom
                    - workspace.Padding.Bottom - workspace.BorderThickness.Bottom;

                _output.WriteLine("WINDOW " + Px(width) + ", " + mode);

                foreach (var (name, rect) in panels)
                {
                    _output.WriteLine("  " + name.PadRight(10) + Box(rect));
                }

                _output.WriteLine("  working card floor at y " + Px(floor));
                _output.WriteLine("");

                // **WORK INSTRUCTION 340 TASK 1: ASSERTED AT 1400 AS WELL.** Until now the 1400
                // window was printed and skipped, so R26's *full to the status bar* at 1400 rested
                // on a print (unit 339 section 4, item 4).
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
                model.ChosenDigitalMode = "FT8";
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
    /// <remarks>
    /// **WORK INSTRUCTION 341 TASK 2: WITH PSK31 CHOSEN AS WELL**, with the assertion it already
    /// made. FT8 is put back before each window closes.
    /// </remarks>
    [AvaloniaFact]
    public void AtNineteenTwentyTheCardsFactsSitBesideTheMap()
    {
        foreach (var mode in new[] { "FT8", "PSK31" })
        {
            var window = Realized(1920);
            var model = (MainWindowViewModel)window.DataContext!;

            try
            {
                model.ChosenDigitalMode = mode;
                Settle(window);

                var placed = Placement(window);

                _output.WriteLine(mode + ": " + placed.Said);

                Assert.True(placed.Beside, "at 1920 on " + mode + " the card's facts are not beside its map: " + placed.Said);
            }
            finally
            {
                model.ChosenDigitalMode = "FT8";
                window.Close();
            }
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

    /// <summary>
    /// **Work instruction 337 task 3: at 1400 the same shape holds** - the working card at least
    /// half the height below the band pills, no callsign clipped, and the card's facts beside or
    /// under its map by the unit's stated rule.
    /// </summary>
    /// <remarks>
    /// <para>**THE RULE, THE UNIT'S OWN AND OVERRULABLE.** The facts sit beside the map when the
    /// card is wide enough inside for the map, the 10 px between them, and the table's widest row;
    /// otherwise they take a line of their own under it. The decoded column never narrows below
    /// its longest line at any width, so the waterfall and For You give up width first.</para>
    /// <para>**"THE WORKING PANELS" IS READ AS THE WORKING CARD**, the tab's own region below
    /// the tabs that holds the three panels, because R26 says *the working panels below the tabs
    /// take the rest of the window*. That reading is the unit's and is marked as such; the three
    /// panels' own height is printed beside it so the other reading can be judged from the same
    /// run. **The height below the pills runs to the window's bottom edge**, the larger of the two
    /// denominators.</para>
    /// <para>**THE TOP ROW AT 1400 IS PRINTED AGAINST THE MOCKUP'S PROPORTION AND NOT ASSERTED**
    /// (R14: the task names no height for it). The mockup's top row is 186 px of the 710 below
    /// its pills.</para>
    /// </remarks>
    [AvaloniaFact]
    public void AtFourteenHundredTheSameShapeHolds()
    {
        var window = Realized(1400);

        try
        {
            var pills = window.GetVisualDescendants().OfType<ItemsControl>()
                .First(i => i.GetVisualDescendants().OfType<Button>().Any(b => b.Classes.Contains("hm-band")));
            var pillsBottom = TheTopRowTests.RectIn(pills, window).Bottom;
            var below = window.Bounds.Height - pillsBottom;
            var measured = TheTopRowTests.Measure(window);
            var panels = Panels(window);

            _output.WriteLine("WINDOW 1400 x " + Px(TheTopRowTests.WindowHeight));
            _output.WriteLine("  band pills end at y " + Px(pillsBottom) + "; " + Px(below) + " px below them");
            _output.WriteLine(
                "  top row      " + Px(measured.TopRowHeight) + " px = " + Share(measured.TopRowHeight, below)
                + " (the mockup: 186 of 710 = " + Share(186, 710) + ")");
            _output.WriteLine(
                "  working card " + Px(measured.Workspace.Height) + " px = " + Share(measured.Workspace.Height, below));
            _output.WriteLine(
                "  the panels   " + Px(panels[0].Rect.Height) + " px = " + Share(panels[0].Rect.Height, below));

            // **WHAT STANDS BETWEEN THE CARD'S TOP AND THE PANELS**, printed so a change in the
            // panels' height can be put down to a row rather than guessed at. The readiness and
            // tune strips appear only when they have something to say.
            foreach (var name in new[]
            {
                "DigitalModeStrip", "DigitalReadinessStrip", "DigitalTuneStrip",
                "DigitalSendReserved", "DigitalHeaderStrip",
            })
            {
                var row = TheTopRowTests.Named<Control>(window, name);

                _output.WriteLine(
                    "    " + name.PadRight(24) + (row.IsEffectivelyVisible
                        ? Px(row.Bounds.Height) + " px tall"
                        : "not shown"));
            }

            Assert.True(
                measured.Workspace.Height >= below / 2,
                "at 1400 the working card is " + Px(measured.Workspace.Height) + " px of the " + Px(below)
                + " below the band pills, which is less than half");

            var clipped = Clipped(window, _output);

            Assert.True(clipped.Count == 0, "clipped at 1400: " + string.Join("; ", clipped));

            var placed = Placement(window);
            var rule = placed.Inside >= placed.MapWidth + 10 + placed.FactsWidth;

            _output.WriteLine("  " + placed.Said);
            _output.WriteLine(
                "  the rule: " + Px(placed.Inside) + " inside against " + Px(placed.MapWidth) + " + 10 + "
                + Px(placed.FactsWidth) + " = " + Px(placed.MapWidth + 10 + placed.FactsWidth)
                + " -> " + (rule ? "BESIDE" : "UNDER"));

            Assert.True(placed.Found, placed.Said);
            Assert.True(
                rule ? placed.Beside : placed.Under,
                "the rule says " + (rule ? "beside" : "under") + " and the card has: " + placed.Said);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// **Work instruction 338 task 1: the three panels themselves take at least half the height
    /// below the band pills**, one top and one bottom, at 1920 and 1400.
    /// </summary>
    /// <remarks>
    /// <para>**THE ARBITER'S RULING 1**: *the working panels* in R26 are the waterfall, decoded
    /// text and For You panels, measured from their shared top to their shared bottom - not the
    /// working card they sit in. The mockup gives them 382 of the 710 px below its pills.</para>
    /// <para>**MEASURED WITH THE READINESS STRIP HIDDEN**, which is the connected state the mockup
    /// draws; the test host has no sound card, so the strip is hidden by setting its visibility
    /// here. The share with the strip showing is printed beside it.</para>
    /// <para>**THE PLAIN FIXTURE AT BOTH WIDTHS AND THE LICENSED ONE AT 1920.** The licensed
    /// operator at 1400 carries the tallest top row, and task 2 is what brings that row back; its
    /// panels are asserted in <see cref="TheTopRowTests"/> with the row.</para>
    /// </remarks>
    [AvaloniaFact]
    public void TheThreePanelsTakeAtLeastHalfTheHeightBelowTheBandPills()
    {
        foreach (var (label, make, width) in new (string, Func<double, Window>, double)[]
        {
            ("plain", Realized, 1920),
            ("plain", Realized, 1400),
            ("licensed", TheTopRowTests.Realized, 1920),
        })
        {
            var window = make(width);

            try
            {
                var shown = PanelsShare(window);

                TheTopRowTests.Named<Border>(window, "DigitalReadinessStrip").IsVisible = false;
                Settle(window);

                var hidden = PanelsShare(window);
                var panels = Panels(window);

                _output.WriteLine(
                    label + " " + Px(width) + ": pills end at y " + Px(hidden.PillsBottom) + ", " + Px(hidden.Below)
                    + " below; the panels " + Px(hidden.Height) + " px = " + Share(hidden.Height, hidden.Below)
                    + " with the readiness strip hidden, " + Px(shown.Height) + " px = " + Share(shown.Height, shown.Below)
                    + " with it " + (shown.StripShown ? "showing" : "not showing (nothing to say)"));

                foreach (var (name, rect) in panels)
                {
                    Assert.True(
                        Math.Abs(rect.Top - panels[0].Rect.Top) <= 0.5 && Math.Abs(rect.Bottom - panels[0].Rect.Bottom) <= 0.5,
                        label + " " + Px(width) + ": " + name + " is " + Box(rect) + " and the waterfall is "
                        + Box(panels[0].Rect) + ", so they are not one top and one bottom");
                }

                Assert.True(
                    hidden.Height >= hidden.Below / 2,
                    label + " " + Px(width) + ": the three panels are " + Px(hidden.Height) + " px of the "
                    + Px(hidden.Below) + " below the band pills (" + Share(hidden.Height, hidden.Below)
                    + "), which is less than half");
            }
            finally
            {
                window.Close();
            }
        }
    }

    /// <summary>
    /// **Work instruction 338 task 1: Stop is never inside anything that collapses, and the CQ /
    /// everything filter is on screen with the list empty and with *Decoded text* collapsed.**
    /// </summary>
    /// <remarks>
    /// §0.2: what stays absolute is the abort. §R17: *the CQ / Everything filter is visible on an
    /// empty list, because it is a choice about what to see.* Moving either out of the rows above
    /// the panels is only allowed if both still hold, at both widths.
    /// </remarks>
    [AvaloniaFact]
    public void StopNeverCollapsesAndTheFilterStaysOnAnEmptyOrCollapsedList()
    {
        foreach (var width in new[] { 1920.0, 1400.0 })
        {
            foreach (var (label, empty, collapsed) in new[]
            {
                ("empty list, Decoded text open", true, false),
                ("rows, Decoded text collapsed", false, true),
            })
            {
                var window = empty ? EmptyTab(width) : Realized(width);

                try
                {
                    var model = (MainWindowViewModel)window.DataContext!;

                    if (collapsed)
                    {
                        model.DigitalDecodedExpanded = false;
                        Settle(window);
                    }

                    var stop = TheTopRowTests.Named<Button>(window, "DigitalStopButton");
                    var folding = stop.GetVisualAncestors().OfType<Hamlet.App.Controls.CollapsiblePanel>().Select(p => p.Title).ToList();

                    _output.WriteLine(
                        Px(width) + ", " + label + ": rows " + model.DigitalDecodes.Count + ", decoded open "
                        + model.DigitalDecodedExpanded + "; Stop visible " + stop.IsEffectivelyVisible
                        + " at " + Box(TheTopRowTests.RectIn(stop, window)) + ", collapsible ancestors ["
                        + string.Join(", ", folding) + "]");

                    Assert.True(stop.IsEffectivelyVisible, Px(width) + ", " + label + ": Stop is not visible");
                    Assert.True(
                        folding.Count == 0,
                        Px(width) + ", " + label + ": Stop is inside the collapsible panel(s) " + string.Join(", ", folding));

                    foreach (var name in new[] { "DigitalFilterEverything", "DigitalFilterCq" })
                    {
                        var chip = TheTopRowTests.Named<Button>(window, name);
                        var at = TheTopRowTests.RectIn(chip, window);

                        _output.WriteLine("  " + name + " visible " + chip.IsEffectivelyVisible + " at " + Box(at));

                        Assert.True(chip.IsEffectivelyVisible, Px(width) + ", " + label + ": " + name + " is not visible");
                        Assert.True(
                            at.Width > 0 && at.Bottom <= window.Bounds.Height,
                            Px(width) + ", " + label + ": " + name + " is not on the window: " + Box(at));
                    }

                    Assert.Equal(empty, model.DigitalDecodes.Count == 0);
                    Assert.Equal(!collapsed, model.DigitalDecodedExpanded);
                }
                finally
                {
                    window.Close();
                }
            }
        }
    }

    private sealed record Share3(double PillsBottom, double Below, double Height, bool StripShown);

    private static Share3 PanelsShare(Window window)
    {
        var pills = window.GetVisualDescendants().OfType<ItemsControl>()
            .First(i => i.GetVisualDescendants().OfType<Button>().Any(b => b.Classes.Contains("hm-band")));
        var pillsBottom = TheTopRowTests.RectIn(pills, window).Bottom;
        var panels = Panels(window);

        return new Share3(
            pillsBottom,
            window.Bounds.Height - pillsBottom,
            panels[0].Rect.Height,
            TheTopRowTests.Named<Border>(window, "DigitalReadinessStrip").IsEffectivelyVisible);
    }

    private static void Settle(Window window)
    {
        for (var i = 0; i < 4; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    /// <summary>The digital tab with nothing decoded, every panel open.</summary>
    /// <remarks>
    /// **THE NETWORK SOURCES ARE SWITCHED OFF SINCE WORK INSTRUCTION 353** (the arbiter's ruling 70, the
    /// unit's own and overrulable), by the same list as <see cref="TheTopRowTests.FixtureSettings"/>. With
    /// POTA at its default this window's numbers moved between runs of the same tree: at 1920 with the
    /// list empty, Stop at y 373 in two of three runs and 367 in the third, the two filter chips 11 px
    /// higher with it (`u353-t1-run1` to `-run3`). In the third run the plain window drew *1 station*
    /// where the other two drew 0, which is read, not proven, as a spot this window's POTA reply wrote
    /// into the run's shared spot history. Nothing else about the window changes.
    /// </remarks>
    private static Window EmptyTab(double width)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        foreach (var name in TheTopRowTests.NetworkSources)
        {
            settings.SetSourceEnabled(name, false);
        }

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalWaterfallExpanded = true,
            DigitalDecodedExpanded = true,
            DigitalMineExpanded = true,
        };

        var window = new MainWindow { DataContext = model, Width = width, Height = TheTopRowTests.WindowHeight };

        window.Show();
        Settle(window);
        Settle(window);

        return window;
    }

    /// <summary>
    /// **Work instruction 338 task 0: the rows between the tabs and the panels, the panels'
    /// share, the run-order spread, the rig against the card, and the best bet.** Printed, not
    /// asserted - the trace task 1 is built from.
    /// </summary>
    [AvaloniaFact]
    public void Unit338TraceTheRowsAboveThePanels()
    {
        // **THE PLAIN FIXTURE AT 1400, FIRST, BEFORE ANY OTHER WINDOW IN THIS METHOD**, so the
        // same reading can be taken again after the licensed windows and compared row by row.
        var first = Realized(1400);

        TraceWindow("plain 1400, first window of the method", first);
        first.Close();

        foreach (var width in new[] { 1920.0, 1400.0 })
        {
            foreach (var (label, make) in new (string, Func<double, Window>)[]
            {
                ("licensed", TheTopRowTests.Realized),
                ("plain", Realized),
            })
            {
                var window = make(width);

                try
                {
                    TraceWindow(label + " " + Px(width) + ", readiness as bound", window);

                    var strip = TheTopRowTests.Named<Border>(window, "DigitalReadinessStrip");

                    strip.IsVisible = !strip.IsVisible;

                    for (var i = 0; i < 4; i++)
                    {
                        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
                        window.UpdateLayout();
                    }

                    TraceWindow(label + " " + Px(width) + ", readiness toggled to " + (strip.IsVisible ? "SHOWN" : "HIDDEN"), window);
                }
                finally
                {
                    window.Close();
                }
            }
        }

        var again = Realized(1400);

        TraceWindow("plain 1400, again after the licensed windows", again);
        again.Close();
    }

    private void TraceWindow(string label, Window window)
    {
        var pills = window.GetVisualDescendants().OfType<ItemsControl>()
            .First(i => i.GetVisualDescendants().OfType<Button>().Any(b => b.Classes.Contains("hm-band")));
        var pillsBottom = TheTopRowTests.RectIn(pills, window).Bottom;
        var below = window.Bounds.Height - pillsBottom;
        var m = TheTopRowTests.Measure(window);
        var panels = Panels(window);
        var model = (MainWindowViewModel)window.DataContext!;

        _output.WriteLine("=== " + label + " (" + Px(window.Bounds.Width) + " x " + Px(window.Bounds.Height) + ")");
        _output.WriteLine("  band pills end at y " + Px(pillsBottom) + "; " + Px(below) + " below");
        _output.WriteLine("  top row " + Px(m.TopRowHeight) + " = " + Share(m.TopRowHeight, below)
            + "; card " + Box(m.Card) + "; rig panel " + Box(m.Rig));
        _output.WriteLine("  working card " + Box(m.Workspace));
        _output.WriteLine("  mode tabs " + Box(TheTopRowTests.RectIn(TheTopRowTests.Named<Control>(window, "ModeTabs"), window)));

        foreach (var (name, rect) in panels)
        {
            _output.WriteLine("  panel " + name.PadRight(10) + Box(rect) + " = " + Share(rect.Height, below));
        }

        foreach (var name in new[]
        {
            "DigitalModeStrip", "DigitalReadinessStrip", "DigitalTuneStrip",
            "DigitalSendReserved", "DigitalHeaderStrip",
        })
        {
            var row = TheTopRowTests.Named<Control>(window, name);

            _output.WriteLine(
                "    " + name.PadRight(24) + (row.IsEffectivelyVisible
                    ? Box(TheTopRowTests.RectIn(row, window)) + " margin " + row.Margin
                    : "not shown (margin " + row.Margin + ")"));
        }

        foreach (var text in TheTopRowTests.Named<Border>(window, "DigitalSendReserved")
            .GetVisualDescendants().OfType<TextBlock>().Where(t => t.IsEffectivelyVisible && (t.Text ?? "").Length > 0))
        {
            _output.WriteLine("      send area " + (text.Name ?? "-").PadRight(26) + Box(TheTopRowTests.RectIn(text, window)) + " [" + text.Text + "]");
        }

        _output.WriteLine("    readiness line [" + model.DigitalReadinessLine + "]; tune line [" + model.DigitalTuneLine + "]");
        _output.WriteLine(
            "    license class " + model.LicenseClass + "; privilege headline [" + model.PrivilegeStatus.Headline
            + "]; send license line [" + model.DigitalSendLicenceLine + "]");

        foreach (var name in new[] { "GreenZoneBlock", "GreenZoneLeft", "GreenZoneRight", "GreenZoneModeLine", "GreenZoneLicenseLine", "GreenZoneRuleOfThumb", "GreenZoneSparkline", "GreenZoneHeard", "GreenZoneStrayedLine" })
        {
            var control = window.GetVisualDescendants().OfType<Control>().FirstOrDefault(c => c.Name == name);

            _output.WriteLine(
                "    " + name.PadRight(24) + (control is null ? "absent"
                    : control.IsEffectivelyVisible ? Box(TheTopRowTests.RectIn(control, window)) : "not shown"));
        }

        foreach (var text in TheTopRowTests.Block(window).GetVisualDescendants().OfType<TextBlock>()
            .Where(t => t.IsEffectivelyVisible && (t.Text ?? "").Length > 0))
        {
            _output.WriteLine("      block text " + Box(TheTopRowTests.RectIn(text, window)) + " [" + text.Text + "]");
        }

        var best = model.Bands.Where(b => b.IsBestBet).Select(b => b.Band.Name + " (" + b.BestBetLabel + ")");
        var bet = window.GetVisualDescendants().OfType<Button>().FirstOrDefault(b => b.Name == "GreenZoneBestBet");

        _output.WriteLine(
            "    best bet: pills [" + string.Join(", ", best) + "]; green zone [" + model.GreenZone.BestBet
            + "] here=" + model.GreenZone.BestBetIsHere + "; button "
            + (bet is null ? "absent" : bet.IsEffectivelyVisible ? "shown [" + bet.Content + "]" : "not shown"));
        _output.WriteLine("");
    }

    // ------------------------------------------------------------------------------------

    /// <summary>Where the card's facts were put against its map, the widths the rule reads, and a sentence.</summary>
    public sealed record Placed(
        bool Found, bool Beside, bool Under, double Inside, double MapWidth, double FactsWidth, string Said);

    private static string Share(double part, double whole)
        => (part / whole).ToString("0.000", CultureInfo.InvariantCulture);

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
            return new Placed(false, false, false, 0, 0, 0, "no card, map or table was realized");
        }

        var map = new Rect(globe.TranslatePoint(new Point(0, 0), beside)!.Value, globe.Bounds.Size);
        var facts = new Rect(table.TranslatePoint(new Point(0, 0), beside)!.Value, table.Bounds.Size);

        var isBeside = facts.X >= map.Right - 0.5 && facts.Y < map.Bottom;
        var isUnder = facts.Y >= map.Bottom - 0.5;

        return new Placed(
            true, isBeside, isUnder, beside.Bounds.Width, map.Width, facts.Width,
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
    /// <remarks>
    /// **THE NETWORK SOURCES ARE SWITCHED OFF SINCE WORK INSTRUCTION 352**, by the same list and for the
    /// same reason as <see cref="TheTopRowTests.FixtureSettings"/>: the callsign made this window a
    /// real operator too, with POTA and RBN on by default and a spot reload waiting on POTA's reply
    /// while the test settles. The callsign, grid and rows are unchanged.
    /// </remarks>
    public static Window Realized(double width) => Realized(width, null);

    /// <summary>The same window, with <paramref name="afterEachPass"/> called as shown (0) and after each settle pass.</summary>
    /// <param name="width">How wide the window is.</param>
    /// <param name="afterEachPass">Read-only hook for the unit 353 trace, or null.</param>
    internal static Window Realized(double width, Action<int, Window>? afterEachPass)
        => Realized(width, TheTopRowTests.WindowHeight, afterEachPass);

    /// <summary>The same window at <paramref name="height"/> px tall.</summary>
    /// <remarks>
    /// **THE HEIGHT OVERLOAD, SINCE WORK INSTRUCTION 354** (the arbiter's ruling 74, overrulable), for the
    /// reason <see cref="TheTopRowTests"/>' overload gives: the other signatures delegate here with
    /// <see cref="TheTopRowTests.WindowHeight"/>, and the sources and the restore are unchanged.
    /// </remarks>
    /// <param name="width">How wide the window is.</param>
    /// <param name="height">How tall the window is.</param>
    /// <param name="afterEachPass">Read-only hook for the unit 353 trace, or null.</param>
    internal static Window Realized(double width, double height, Action<int, Window>? afterEachPass)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = HisCall;
        settings.Operator.GridSquare = "FN00";

        foreach (var name in TheTopRowTests.NetworkSources)
        {
            settings.SetSourceEnabled(name, false);
        }

        // **GENERAL FOR KC3QIS, HANDED IN** (work instruction 355 task 4). Unit 354 found this window asking
        // callook.info at construction, so the class landed or not with the network.
        var model = new MainWindowViewModel(settings, null, FixedLicenseLookup.GeneralForKc3qis())
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

        var window = new MainWindow { DataContext = model, Width = width, Height = height };

        window.Show();
        afterEachPass?.Invoke(0, window);

        for (var i = 0; i < 6; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            afterEachPass?.Invoke(i + 1, window);
        }

        // **NO BEST BET, SET AGAIN AFTER THE RELOAD, SINCE WORK INSTRUCTION 353** (the arbiter's ruling
        // 67, the unit's own and overrulable), for the reason `TheTopRowTests.Realized` gives: the
        // `startup` reload lands in pass 1 of the six above (`Unit353TraceTheDeclaredWindowAfterTheReloads`,
        // `b01e033e`) and badges the hour's best bet, *80 m* at 2 am. **This window declares no heard
        // count**, so the count is left as the reload gives it: 0 at `b01e033e`, where it was null as
        // shown.
        foreach (var band in model.Bands)
        {
            band.IsBestBet = false;
        }

        model.NotifyGreenZoneForTests();

        for (var i = 0; i < 6; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            afterEachPass?.Invoke(i + 7, window);
        }

        Assert.True(
            !model.Bands.Any(b => b.IsBestBet),
            "the plain test window's IsBestBet held true on [" + string.Join(", ", model.Bands.Where(b => b.IsBestBet).Select(b => b.Band.Name))
            + "] after the restore, where the fixture declares no best bet");

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
