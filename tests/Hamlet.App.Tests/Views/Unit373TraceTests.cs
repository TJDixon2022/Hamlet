using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 373 task 1: **measure both repairs before either is made.** Printed, not
/// asserted - nothing here asserts anything about new behavior, and no source file changes at this
/// task.
/// </summary>
/// <remarks>
/// <para>**WHY THIS EXISTS AT ALL.** Unit 372 swept width 1100 only, and its stated reason for
/// calling criterion 1.3's repair a redesign rather than a number was that *the rest of the canvas
/// varies with width as well as height*. That claim decides whether the floor is one number or
/// three, and it was never measured. <see cref="ThePanelCanvasAtThreeWidths"/> measures it.</para>
/// <para>**AND WHY THE SECOND HALF EXISTS.** `TheTopRowTests.TheBestBetPillAndTheGreenBlockNameThe
/// SameBandOnTheWindow` fails alone at *40 m* with the collection of badged pills empty. Two causes
/// would produce that print and they want opposite repairs: a badge drawn wearing the other of
/// HM-DEC-046's two ruled labels, which is the test asserting a label instead of the rule, or no
/// badge drawn at all, which is the application. <see cref="TheBestBetBadgeAtBothWidths"/> prints
/// every band's flag, label and badge visibility so the report can say which in one sentence.</para>
/// <para>**COMPUTED, NOT SEEN** (`SHACK_FACTS.md` FACT-004). Every number is a `Bounds` rectangle or
/// a property read off a realized headless window. **Nothing is pressed** (CLAUDE.md §0.2) - in
/// particular `GreenZoneBestBet` is read and never clicked, because clicking it moves the operator's
/// band.</para>
/// </remarks>
public sealed class Unit373TraceTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where every box is printed.</param>
    public Unit373TraceTests(ITestOutputHelper output) => _output = output;

    /// <summary>The three widths the chrome claim is tested across.</summary>
    private static readonly double[] Widths = { 900, 1100, 1920 };

    /// <summary>The five heights, from the size Hamlet opens at down to the smallest it will take.</summary>
    private static readonly double[] Heights = { 780, 740, 700, 660, 620 };

    /// <summary>
    /// **Task 1 item 1: the panel canvas at three widths.** The window box, `WorkspaceBoundary`'s
    /// box, the box of the `Panel` it wraps, the three working panels, and the decoded and For You
    /// scrollers' extent and viewport - then, per width, how many pixels stand between the
    /// boundary's outer edge and the first panel pixel.
    /// </summary>
    [AvaloniaFact]
    public void ThePanelCanvasAtThreeWidths()
    {
        var chromeByWidth = new Dictionary<double, List<double>>();

        foreach (var width in Widths)
        {
            chromeByWidth[width] = new List<double>();

            foreach (var height in Heights)
            {
                var window = TheWorkingPanelsTests.Realized(width, height, null);

                try
                {
                    Pump(window);

                    var bounds = window.Bounds;
                    var boundary = TheTopRowTests.Named<Border>(window, "WorkspaceBoundary");
                    var boundaryAt = TheTopRowTests.RectIn(boundary, window);
                    // **THE CANVAS IS NAMED SINCE TASK 2.** Before the floor it was the border's
                    // only child and was found that way; it is now inside `WorkspaceCanvasScroller`,
                    // so it is found by its name and the search still answers on either tree.
                    var canvas = (Panel?)window.GetVisualDescendants().OfType<Control>()
                            .FirstOrDefault(c => c.Name == "WorkspaceCanvas")
                        ?? boundary.GetVisualChildren().OfType<Panel>().FirstOrDefault();
                    var canvasAt = canvas is null ? default : TheTopRowTests.RectIn(canvas, window);
                    var panels = TheWorkingPanelsTests.Panels(window);
                    var chrome = panels[0].Rect.Top - boundaryAt.Top;

                    chromeByWidth[width].Add(chrome);

                    _output.WriteLine(
                        "=== asked " + Px(width) + " x " + Px(height) + ", drawn "
                        + Px(bounds.Width) + " x " + Px(bounds.Height));
                    _output.WriteLine("    WorkspaceBoundary   " + Box(boundaryAt)
                        + "  border " + boundary.BorderThickness + " padding " + boundary.Padding);
                    _output.WriteLine("    the Panel it wraps  "
                        + (canvas is null ? "NOT FOUND" : Box(canvasAt)));

                    // **THE ROWS THAT STAND BETWEEN THE BOUNDARY AND THE PANELS**, each named, so
                    // the chrome total can be put down to a row rather than guessed at.
                    foreach (var name in new[]
                    {
                        "DigitalWorkspace", "DigitalModeStrip", "DigitalReadinessStrip",
                        "DigitalTuneStrip", "DigitalPanes",
                    })
                    {
                        var row = window.GetVisualDescendants().OfType<Control>()
                            .FirstOrDefault(c => c.Name == name);

                        _output.WriteLine("      " + name.PadRight(22)
                            + (row is null ? "absent"
                                : row.IsEffectivelyVisible
                                    ? Box(TheTopRowTests.RectIn(row, window)) + " margin " + row.Margin
                                    : "not shown (margin " + row.Margin + ")"));
                    }

                    foreach (var (name, rect) in panels)
                    {
                        var scroll = Scroller(window, name);

                        _output.WriteLine("    panel " + name.PadRight(14) + Box(rect)
                            + (scroll is null
                                ? "  no scroller"
                                : "  viewport " + Px(scroll.Viewport.Height)
                                  + " extent " + Px(scroll.Extent.Height)));
                    }

                    _output.WriteLine(
                        "    CHROME: the boundary's outer edge is at y " + Px(boundaryAt.Top)
                        + " and the first panel pixel at y " + Px(panels[0].Rect.Top)
                        + " -> " + Px(chrome) + " px consumed");
                    _output.WriteLine("");
                }
                finally
                {
                    window.Close();
                }
            }
        }

        // **THE ONE ANSWER THIS TASK EXISTS FOR.** Unit 372 measured 138 px at width 1100 and built
        // its reasoning on it. If that number is the same at 900 and 1920 the floor is one number;
        // if it varies the floor has to be written per width or bound.
        _output.WriteLine("=== CHROME PER WIDTH, which decides whether the floor is one number or three");

        foreach (var width in Widths)
        {
            var seen = chromeByWidth[width];

            _output.WriteLine(
                "    width " + Px(width).PadLeft(6) + ": " + string.Join(", ", seen.Select(Px))
                + "  (min " + Px(seen.Min()) + ", max " + Px(seen.Max())
                + ", spread " + Px(seen.Max() - seen.Min()) + ")");
        }

        var all = chromeByWidth.Values.SelectMany(v => v).ToList();

        _output.WriteLine(
            "    ACROSS ALL THREE WIDTHS: min " + Px(all.Min()) + ", max " + Px(all.Max())
            + ", spread " + Px(all.Max() - all.Min()));
    }

    /// <summary>
    /// **Task 1 item 2: the best-bet red, printed rather than guessed.** At 1920 and 1400, for both
    /// `bestName` values the failing test uses, every band's name, `IsBestBet`, `BestBetLabel`, and
    /// whether its badge `Border` is `IsEffectivelyVisible` - and what that border's own `TextBlock`
    /// is actually showing.
    /// </summary>
    /// <remarks>
    /// **NOTHING IS PRESSED** (§0.2). The flags are set the way the failing test sets them, by hand
    /// on the view model, and `GreenZoneBestBet` is read for its content only.
    /// </remarks>
    [AvaloniaFact]
    public void TheBestBetBadgeAtBothWidths()
    {
        _output.WriteLine(
            "The wall clock this ran on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
            + " local, hour " + DateTime.Now.Hour + " - which is what MainWindowViewModel.RankBands "
            + "passes to BandOpportunities.Rank, so it is part of the fixture whether anybody meant it to be.");
        _output.WriteLine("");

        foreach (var width in new[] { 1920.0, 1400.0 })
        {
            var window = TheTopRowTests.Realized(width);

            try
            {
                var model = (MainWindowViewModel)window.DataContext!;
                var here = TheTopRowTests.Named<TextBlock>(window, "GreenZoneBand").Text;

                // **WHAT THE RANKING LEFT BEHIND, BEFORE THE TEST TOUCHES A FLAG.** The fixture
                // clears every IsBestBet after the reloads, but ApplyBestBet only writes a label on
                // the band it badges, so whichever band the hour ranked first is still carrying the
                // label the ranking gave it.
                _output.WriteLine("=== " + Px(width) + " as the fixture returns it, green block band [" + here + "]");

                foreach (var band in model.Bands)
                {
                    _output.WriteLine(
                        "    " + band.Band.Name.PadRight(8) + " IsBestBet " + band.IsBestBet.ToString().PadRight(6)
                        + " BestBetLabel [" + band.BestBetLabel + "]");
                }

                _output.WriteLine("");

                foreach (var bestName in new[] { "20 m", "40 m" })
                {
                    foreach (var band in model.Bands)
                    {
                        band.IsBestBet = band.Band.Name == bestName;
                    }

                    model.NotifyGreenZoneForTests();
                    Pump(window, 4);

                    _output.WriteLine("=== " + Px(width) + ", best bet set by hand on " + bestName);

                    foreach (var (name, flag, label, badge, shown) in Badges(window))
                    {
                        _output.WriteLine(
                            "    " + name.PadRight(8) + " IsBestBet " + flag.ToString().PadRight(6)
                            + " BestBetLabel [" + label + "]"
                            + "  badge Border " + (badge is null ? "NOT FOUND" : badge.IsEffectivelyVisible ? "VISIBLE" : "hidden")
                            + "  wearing [" + shown + "]");
                    }

                    // **THE FILTER THE FAILING TEST USES**, reproduced exactly, so the print says
                    // what it would have collected.
                    var byText = window.GetVisualDescendants().OfType<TextBlock>()
                        .Where(t => t.IsEffectivelyVisible && t.Text == "best bet now")
                        .Select(t => (t.DataContext as BandButtonViewModel)?.Band.Name)
                        .ToList();
                    var byBorder = Badges(window).Where(b => b.Badge?.IsEffectivelyVisible == true)
                        .Select(b => b.Name).ToList();
                    var bet = TheTopRowTests.Named<Button>(window, "GreenZoneBestBet");

                    _output.WriteLine(
                        "    the failing test's text filter collects [" + string.Join(", ", byText)
                        + "]; the badge borders actually drawn are [" + string.Join(", ", byBorder) + "]");
                    _output.WriteLine(
                        "    green block best bet [" + (bet.Content as string ?? "") + "] visible "
                        + bet.IsEffectivelyVisible);
                    _output.WriteLine("");
                }
            }
            finally
            {
                window.Close();
            }
        }

        _output.WriteLine(
            "HM-DEC-046's two ruled labels are [best bet now] for an observation and "
            + "[likely, going on the hour] for a clock guess, and BandRanking.BadgeLabel is the only "
            + "thing that chooses between them.");
    }

    /// <summary>
    /// **Task 2's measurement: what each candidate floor buys and what it costs.** For a ladder of
    /// canvas floors, the panel row at every height in the 1.2 sweep, the three panels' viewports at
    /// the bottom of it, and the number of heights at which anything shrank - which is the
    /// precondition <c>TheWindowGivesUpHeightInOneOrderTests</c> reads.
    /// </summary>
    /// <remarks>
    /// <para>**WHY THIS IS MEASURED RATHER THAN ARGUED.** Work instruction 373 §6 asks for *the
    /// smallest floor at which all three panels report a working scroller at 900 x 620*, and task 2
    /// asks that 1.2's guard still be 4 of 4. This name is what says whether one floor can do both.
    /// The floor is moved on the realized window by setting `WorkspaceCanvas.MinHeight`, which is the
    /// same property the markup sets and nothing else.</para>
    /// <para>**PRINTED, NOT ASSERTED.** It measures a property of the layout, not a rule, and §R14
    /// says a unit writes the tests its criteria need and no others.</para>
    /// </remarks>
    [AvaloniaFact]
    public void WhatEachCandidateFloorBuysAndWhatItCosts()
    {
        foreach (var width in new[] { 1100.0, 900.0 })
        {
            _output.WriteLine("=== width " + Px(width));

            foreach (var floor in new[] { 0.0, 120, 144, 160, 172, 185, 192, 240, 480 })
            {
                var rows = new List<double>();
                var said = "";

                foreach (var height in Heights)
                {
                    var window = TheWorkingPanelsTests.Realized(width, height, null);

                    try
                    {
                        var canvas = window.GetVisualDescendants().OfType<Control>()
                            .First(c => c.Name == "WorkspaceCanvas");

                        canvas.MinHeight = floor;
                        Pump(window);

                        var panels = TheWorkingPanelsTests.Panels(window);

                        rows.Add(panels[0].Rect.Height);

                        if (Math.Abs(height - 620) < 0.5)
                        {
                            var decoded = Scroller(window, "decoded");
                            var mine = Scroller(window, "For You");

                            said = "at 620 the panel row is " + Px(panels[0].Rect.Height)
                                + ", decoded viewport " + Px(decoded?.Viewport.Height ?? -1)
                                + " extent " + Px(decoded?.Extent.Height ?? -1)
                                + ", For You viewport " + Px(mine?.Viewport.Height ?? -1)
                                + " extent " + Px(mine?.Extent.Height ?? -1);
                        }
                    }
                    finally
                    {
                        window.Close();
                    }
                }

                var shrinks = 0;

                for (var i = 1; i < rows.Count; i++)
                {
                    if (rows[i] < rows[i - 1] - 0.5)
                    {
                        shrinks++;
                    }
                }

                _output.WriteLine(
                    "    floor " + Px(floor).PadLeft(5) + ": panel row " + string.Join(", ", rows.Select(r => Px(r).PadLeft(4)))
                    + "  shrinks " + shrinks + " (1.2's guard needs 2 or more)");
                _output.WriteLine("        " + said);
            }

            _output.WriteLine("");
        }
    }

    // ------------------------------------------------------------------------------------

    /// <summary>Every band pill's flag, label, badge border and the words that border is showing.</summary>
    /// <remarks>
    /// **THE BADGE IS FOUND BY THE BORDER, NOT BY ITS WORDS.** The markup's badge is the
    /// `IsHitTestVisible="False"` border at `MainWindow.axaml` line 3313, bound to `IsBestBet`; it
    /// is the only such border inside a band pill. Finding it by its text is exactly the reading the
    /// failing test makes, and it cannot see a badge wearing the other ruled label.
    /// </remarks>
    private static List<(string Name, bool Flag, string Label, Border? Badge, string Shown)> Badges(Window window)
    {
        var found = new List<(string, bool, string, Border?, string)>();

        foreach (var pill in window.GetVisualDescendants().OfType<Control>()
            .Where(c => c.DataContext is BandButtonViewModel))
        {
            if (pill.DataContext is not BandButtonViewModel band || found.Any(f => f.Item1 == band.Band.Name))
            {
                continue;
            }

            var badge = pill.GetVisualDescendants().OfType<Border>()
                .FirstOrDefault(b => !b.IsHitTestVisible && b.Child is TextBlock);
            var shown = badge?.Child is TextBlock text ? text.Text ?? "" : "";

            found.Add((band.Band.Name, band.IsBestBet, band.BestBetLabel, badge, shown));
        }

        return found;
    }

    /// <summary>The scroller a list panel scrolls its own content in, as the red test finds it.</summary>
    private static ScrollViewer? Scroller(Window window, string name)
    {
        if (name == "waterfall")
        {
            return null;
        }

        var anchor = name == "decoded" ? "DigitalDecodedRows" : "DigitalContactCards";

        return window.GetVisualDescendants().OfType<Control>()
            .FirstOrDefault(c => c.Name == anchor)
            ?.GetVisualAncestors().OfType<ScrollViewer>().FirstOrDefault();
    }

    private static void Pump(Window window, int passes = 6)
    {
        for (var i = 0; i < passes; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    private static string Px(double value)
        => value.ToString("0.#", CultureInfo.InvariantCulture);

    private static string Box(Rect r)
        => Px(r.X) + "," + Px(r.Y) + " " + Px(r.Width) + " x " + Px(r.Height);
}
