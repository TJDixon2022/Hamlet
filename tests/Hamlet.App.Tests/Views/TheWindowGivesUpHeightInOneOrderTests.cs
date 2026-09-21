using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 372 task 2, criterion 1.2: **the order in which height is given up.**
/// Rewritten by work instruction 374 task 2 under §R12, to §6's first ruling.
/// </summary>
/// <remarks>
/// <para>**THE RULE IS THE TREE'S AND UNIT 356'S, NOT THIS UNIT'S.** It is written above
/// <c>TopRow</c> in <c>src/Hamlet.App/Views/MainWindow.axaml</c>: *the working panels give up
/// height first, then the top row, and the send area is never the thing that leaves.* Unit 356
/// capped the row at <c>MaxHeight="300"</c> and asserted the nine sizes; **what it did not do was
/// measure the order**, and a comment is not a test. R34 (Tim, 2026-09-14) was ruled about exactly
/// this state of affairs: at 1100 x 780, the size Hamlet opens at, CQ was drawn at y 800 and the
/// mode tabs at y 847, below a window 780 tall.</para>
/// <para>**WHY THIS TYPE WAS REWRITTEN, AND IT IS §R12 WORK RATHER THAN A LOOSENING.** As unit 372
/// wrote it, the sweep was 780 down to 620 and <see cref="TheWorkingPanelsLoseHeightBeforeTheTopRow
/// LosesAny"/> read the order of surrender off transitions inside that band. **Unit 373's floor
/// then made the band flat**: <c>WorkspaceCanvas</c> keeps <c>MinHeight="185"</c> and a scroller of
/// its own, so at width 1100 the panel row is 73 px at 780 and 73 px at every height down to 620,
/// and nothing shrinks anywhere in the sweep. The name went red **on its own precondition** -
/// <c>shrinks.Count &gt;= 2</c> - and not on the rule, which never broke. Unit 373 measured that no
/// floor satisfies both criterion 1.3 and that precondition: floors below 145 px give the guard its
/// two shrinks and leave the decoded viewport at 0, floors of 172 px and up give the panels a
/// working scroller and give the guard one shrink or none, and there is nothing in the 27 px
/// between. **The window the rule was being read through had gone flat, so the window is what
/// moved.**</para>
/// <para>**THE SWEEP NOW STARTS AT 920, AND THAT NUMBER IS MEASURED** (<c>Unit374TraceTests.
/// TheHeightLadderAtTwoWidths</c>, task 1). At width 1100, over a ladder from 1040 to 620 in steps
/// of 40, the panel row runs 331, 291, 251, 211, 171, 131, 91 px from 1040 down to 800, reaches
/// 71 px at 780 and **stays at 71 px at 740, 700, 660 and 620** - so the floor begins to bind at
/// 780, with seven shrink transitions above it and none below. A sweep topped at 840 gives the
/// guard exactly two shrinks; **920 is taken, which gives four**, so the precondition is met with
/// margin rather than on the edge. At width 900 the same ladder binds at 740 instead, one step
/// lower, which is why the flat band below is asserted from the height the measurement names and
/// not from an assumption.</para>
/// <para>**NOTHING WAS REMOVED AND NOTHING WAS WEAKENED.** Every height from 780 to 620 is still in
/// the sweep; <c>shrinks.Count &gt;= 2</c> stands at its threshold; the 0.5 px slack is unchanged;
/// and everything <c>shrinks.Take(2)</c> asserted, it still asserts. **The type asserts the same
/// four rules over nine heights where it asserted them over five, plus one rule it never asserted
/// at all**: below the floor the panel row is constant, <c>TopRow</c> still gives up none, and the
/// canvas scrolls instead. That last is what ties 1.2 to 1.3 rather than pitting them against each
/// other - it states the rule unit 373's repair actually established.</para>
/// <para>**WHY THE SWEEP STOPS AT 620.** That is <c>MinHeight</c>, and the window will not go below
/// it: asked for 580, 540 and 500, a headless window drew 620 each time. What happens at and below
/// the minimum is criterion 1.3's question and <c>TheWindowHoldsBelowItsMinimumTests</c>'.</para>
/// <para>**BOTH WINDOWS, BECAUSE ONE OF THEM HAS NOTHING IN ITS PANELS.**
/// <c>TheTopRowTests.Realized</c> declares the pinned facts and no decoded rows;
/// <c>TheWorkingPanelsTests.Realized</c> carries the longest line the decoded list has to hold and
/// a card. A claim about Hamlet rather than about one fixture has to hold on both. The two differ
/// by 2 px in the panel row at every height, which is the card's own chrome and not a disagreement
/// about the rule.</para>
/// <para>**COMPUTED, NOT SEEN** (<c>SHACK_FACTS.md</c> FACT-004). Every number is a <c>Bounds</c>
/// rectangle, or a <c>ScrollViewer</c>'s own extent and viewport, off a realized headless window.
/// Nothing is pressed (CLAUDE.md §0.2), nothing is opened, and nothing here is evidence about the
/// radio.</para>
/// </remarks>
public sealed class TheWindowGivesUpHeightInOneOrderTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where every height's numbers are printed.</param>
    public TheWindowGivesUpHeightInOneOrderTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// The descending sweep at width 1100, from above the size Hamlet opens at down to
    /// <c>MinHeight</c>. **The top is task 1's measurement and not a guess**; the bottom five
    /// heights are unit 372's own sweep, kept entire.
    /// </summary>
    private static readonly double[] Sweep = { 920, 880, 840, 800, 780, 740, 700, 660, 620 };

    /// <summary>
    /// The highest window height at which the floor is allowed to begin binding at width 1100 -
    /// unit 374's measured 780, kept as a ceiling rather than as the answer.
    /// </summary>
    /// <remarks>
    /// <para>**MEASURED, NOT CHOSEN** (<c>Unit374TraceTests.TheHeightLadderAtTwoWidths</c>): the
    /// panel row was 91 px at 800 and 71 px at 780, and then 71 px at 740, 700, 660 and 620, so
    /// unit 373's 185 px canvas floor began to bind at 780.</para>
    /// <para>**AND SINCE WORK INSTRUCTION 376 IT IS A CEILING, NOT A CONSTANT** (§R12; the name
    /// went red on this number and on nothing else). Unit 376 took 29 px out of the top row, so
    /// the panels go on shrinking one step further down: the panel row is 92 px at 780 - where it
    /// was 71 - and 71 px at 740 and below, and **the floor now begins to bind at 740**. The rule
    /// this name exists for never moved. So the height the flat band starts at is **read off the
    /// sweep** instead of written here, and this number stays as the assertion that it can only
    /// ever move DOWN: a change that puts height back into the top row would make the floor bind
    /// higher again, and that is caught here rather than passing quietly.</para>
    /// </remarks>
    private const double TheFloorBindsNoHigherThan = 780;

    /// <summary>Unit 356's cap on <c>TopRow</c>, in <c>MainWindow.axaml</c> at the row itself.</summary>
    private const double TopRowCap = 300;

    /// <summary>The five controls R34 says are never the thing that leaves.</summary>
    private static readonly string[] TheSendArea =
    {
        "DigitalSendCqButton", "ModeTabs", "DigitalSendReserved",
        "DigitalTransmitDriveNote", "DigitalStopButton",
    };

    /// <summary>
    /// **The send area's drawn height is the same at every height in the sweep**, which is what
    /// *never the thing that leaves* says as a number: a thing giving up height would shrink.
    /// </summary>
    [AvaloniaFact]
    public void TheSendAreaIsTheSameHeightAtEveryHeightInTheSweep()
    {
        foreach (var plain in new[] { false, true })
        {
            var read = Sweep.Select(h => Measure(1100, h, plain)).ToList();
            var first = read[0];

            foreach (var row in read)
            {
                _output.WriteLine(
                    Which(plain) + " at " + Px(row.Asked) + ": the send area is "
                    + Px(row.SendArea) + " px tall");
            }

            foreach (var row in read)
            {
                Assert.True(
                    Math.Abs(row.SendArea - first.SendArea) <= 1,
                    Which(plain) + ": the send area is " + Px(row.SendArea) + " px tall at "
                    + Px(row.Asked) + " and " + Px(first.SendArea) + " px at " + Px(first.Asked)
                    + ". It gave up height, and it is the one thing that never does.");
            }
        }
    }

    /// <summary>
    /// **The working panels lose height before <c>TopRow</c> loses any.** At the first two heights
    /// in the sweep where anything shrinks at all, the panels are strictly smaller than at the
    /// height above and <c>TopRow</c> is unchanged - **and below the floor they stop surrendering
    /// height altogether, the canvas scrolling in their place while <c>TopRow</c> still gives up
    /// none.**
    /// </summary>
    /// <remarks>
    /// **THE SECOND HALF IS WORK INSTRUCTION 374 §6's ADDED ASSERTION**, and it is the one that
    /// ties this criterion to 1.3 instead of setting them against each other. Unit 373's floor is
    /// why nothing shrinks from the floor down - <see cref="TheFloorBindsNoHigherThan"/> is how far
    /// up the sweep that is allowed to start; *that* is the rule the floor
    /// established, so it is asserted here rather than left to read as the absence of a shrink.
    /// </remarks>
    [AvaloniaFact]
    public void TheWorkingPanelsLoseHeightBeforeTheTopRowLosesAny()
    {
        foreach (var plain in new[] { false, true })
        {
            var read = Sweep.Select(h => Measure(1100, h, plain)).ToList();
            var shrinks = new List<(Measured Above, Measured Here)>();

            for (var i = 1; i < read.Count; i++)
            {
                if (read[i].Panels < read[i - 1].Panels - 0.5
                    || read[i].TopRow < read[i - 1].TopRow - 0.5)
                {
                    shrinks.Add((read[i - 1], read[i]));
                }
            }

            foreach (var row in read)
            {
                _output.WriteLine(
                    Which(plain) + " at " + Px(row.Asked) + ": TopRow " + Px(row.TopRow)
                    + ", the panel row " + Px(row.PanelRow) + ", the three together "
                    + Px(row.Panels));
            }

            // **SOMETHING HAS TO GIVE SOMEWHERE IN THE SWEEP**, or this name proves nothing.
            Assert.True(
                shrinks.Count >= 2,
                Which(plain) + ": nothing shrank at two heights in the sweep, so there is no "
                + "order to read. The sweep " + string.Join(", ", Sweep.Select(Px))
                + " has to contain the point where the window runs short.");

            foreach (var (above, here) in shrinks.Take(2))
            {
                _output.WriteLine(
                    Which(plain) + " " + Px(above.Asked) + " -> " + Px(here.Asked)
                    + ": the panels went " + Px(above.Panels) + " -> " + Px(here.Panels)
                    + " and TopRow " + Px(above.TopRow) + " -> " + Px(here.TopRow));

                Assert.True(
                    here.Panels < above.Panels - 0.5,
                    Which(plain) + ": from " + Px(above.Asked) + " to " + Px(here.Asked)
                    + " the working panels held " + Px(above.Panels) + " px and then "
                    + Px(here.Panels) + " px. They are what gives first, and they gave nothing.");

                Assert.True(
                    Math.Abs(here.TopRow - above.TopRow) <= 0.5,
                    Which(plain) + ": from " + Px(above.Asked) + " to " + Px(here.Asked)
                    + " TopRow went " + Px(above.TopRow) + " -> " + Px(here.TopRow)
                    + ". It gave up height while the working panels still had some.");
            }

            // **AND BELOW THE FLOOR THE PANELS STOP SURRENDERING HEIGHT AT ALL** (work instruction
            // 374 §6, clause 4). The canvas scrolls in their place, and `TopRow` still gives up
            // nothing - so the band unit 373 made flat is asserted as flat rather than read as a
            // failure to shrink.
            // **THE FLAT BAND IS FOUND IN THE SWEEP, NOT DECLARED** (work instruction 376 task 3,
            // §R12). The lowest height in the sweep is on the floor by construction - the window
            // will not go below `MinHeight` - so the flat band is every height whose panel row
            // matches it, and the highest of those is where the floor begins to bind.
            var lowest = read[^1];
            var flat = read.Where(r => Math.Abs(r.PanelRow - lowest.PanelRow) <= 0.5).ToList();
            var atTheFloor = flat[0];

            // **AND IT MAY ONLY EVER BIND LOWER.** Height taken out of the top row is height the
            // panels keep, so the point where they stop shrinking moves DOWN the sweep; if it
            // moves up, the top row has taken height back.
            Assert.True(
                atTheFloor.Asked <= TheFloorBindsNoHigherThan + 0.5,
                Which(plain) + ": the panel row stops shrinking at " + Px(atTheFloor.Asked)
                + " px of window, where unit 374 measured it stopping at "
                + Px(TheFloorBindsNoHigherThan) + ". The floor binds higher than it did, which is"
                + " the top row taking height back from the working panels.");

            foreach (var row in flat)
            {
                _output.WriteLine(
                    Which(plain) + " at " + Px(row.Asked) + ", below the floor: the panel row "
                    + Px(row.PanelRow) + ", TopRow " + Px(row.TopRow) + ", the canvas viewport "
                    + Px(row.Viewport) + " in an extent of " + Px(row.Extent));

                Assert.True(
                    Math.Abs(row.PanelRow - atTheFloor.PanelRow) <= 0.5,
                    Which(plain) + ": the panel row is " + Px(row.PanelRow) + " px at "
                    + Px(row.Asked) + " where it is " + Px(atTheFloor.PanelRow) + " px at "
                    + Px(atTheFloor.Asked) + ". Below the floor it does not move, and it moved.");

                Assert.True(
                    Math.Abs(row.TopRow - atTheFloor.TopRow) <= 0.5,
                    Which(plain) + ": TopRow is " + Px(row.TopRow) + " px at " + Px(row.Asked)
                    + " where it is " + Px(atTheFloor.TopRow) + " px at " + Px(atTheFloor.Asked)
                    + ". It gives up none of its height, at any height in the sweep.");

                if (row.Asked >= atTheFloor.Asked - 0.5)
                {
                    // **AT THE FLOOR ITSELF NOTHING HAS TO SCROLL YET** - the canvas fills its
                    // viewport exactly there, and what it does below is the assertion.
                    continue;
                }

                Assert.True(
                    row.Extent > row.Viewport + 0.5,
                    Which(plain) + ": at " + Px(row.Asked) + " the canvas reports an extent of "
                    + Px(row.Extent) + " in a viewport of " + Px(row.Viewport)
                    + ". The panels stopped shrinking here, so the canvas has to be scrolling "
                    + "instead, and nothing is being reached by scrolling.");
            }
        }
    }

    /// <summary>
    /// **<c>TopRow</c> gives up its share only after the panels have**, and never exceeds unit 356's
    /// 300 px cap at any height in the sweep.
    /// </summary>
    [AvaloniaFact]
    public void TheTopRowGivesUpItsShareOnlyAfterThePanelsHaveAndNeverExceedsItsCap()
    {
        foreach (var plain in new[] { false, true })
        {
            var read = Sweep.Select(h => Measure(1100, h, plain)).ToList();

            foreach (var row in read)
            {
                _output.WriteLine(
                    Which(plain) + " at " + Px(row.Asked) + ": TopRow " + Px(row.TopRow)
                    + " against the " + Px(TopRowCap) + " px cap; the panels " + Px(row.Panels));

                Assert.True(
                    row.TopRow <= TopRowCap + 0.5,
                    Which(plain) + ": TopRow is " + Px(row.TopRow) + " px tall at "
                    + Px(row.Asked) + ", past unit 356's " + Px(TopRowCap) + " px cap.");
            }

            for (var i = 1; i < read.Count; i++)
            {
                if (read[i].TopRow >= read[i - 1].TopRow - 0.5)
                {
                    continue;
                }

                // **IT GAVE SOMETHING UP HERE**, so the panels must already have nothing left.
                Assert.True(
                    read[i - 1].Panels <= 0.5,
                    Which(plain) + ": TopRow went " + Px(read[i - 1].TopRow) + " -> "
                    + Px(read[i].TopRow) + " between " + Px(read[i - 1].Asked) + " and "
                    + Px(read[i].Asked) + ", while the working panels still held "
                    + Px(read[i - 1].Panels) + " px. The panels give first.");
            }
        }
    }

    /// <summary>
    /// **At every height in the sweep, CQ, the mode tabs, the reserved send area, the drive note
    /// and Stop are whole on the window** - R34's own sentence, as a measurement.
    /// </summary>
    [AvaloniaFact]
    public void AtEveryHeightInTheSweepTheSendAreaAndStopAreWholeOnTheWindow()
    {
        var misses = new List<string>();

        foreach (var plain in new[] { false, true })
        {
            foreach (var height in Sweep)
            {
                var window = Realize(1100, height, plain);

                try
                {
                    Pump(window);

                    var bounds = window.Bounds;
                    var label = Which(plain) + " at 1100 x " + Px(height);

                    foreach (var name in TheSendArea)
                    {
                        var control = TheTopRowTests.Named<Control>(window, name);
                        var at = TheTopRowTests.RectIn(control, window);
                        var whole = at.Width > 0 && at.Height > 0
                            && at.Left >= -0.5 && at.Top >= -0.5
                            && at.Right <= bounds.Width + 0.5
                            && at.Bottom <= bounds.Height + 0.5;

                        _output.WriteLine(
                            label + "  " + name.PadRight(26) + Box(at)
                            + "  visible " + control.IsEffectivelyVisible + "  whole " + whole);

                        if (!control.IsEffectivelyVisible)
                        {
                            misses.Add(label + ": " + name + " is not visible");
                        }

                        if (!whole)
                        {
                            misses.Add(
                                label + ": " + name + " " + Box(at) + " is not whole on the "
                                + Box(bounds) + " window");
                        }
                    }
                }
                finally
                {
                    window.Close();
                }
            }
        }

        Assert.True(misses.Count == 0, string.Join(Environment.NewLine, misses));
    }

    /// <summary>What one window at one height gave each of the three things that can take it.</summary>
    /// <param name="Asked">The height the window was asked for.</param>
    /// <param name="Drawn">The height it drew, which is not the same below <c>MinHeight</c>.</param>
    /// <param name="TopRow">The top row's drawn height.</param>
    /// <param name="PanelRow">The working panels' shared row height.</param>
    /// <param name="Panels">The three working panels' heights added up.</param>
    /// <param name="SendArea">The reserved send area's drawn height.</param>
    /// <param name="Viewport">How much of the panel canvas <c>WorkspaceCanvasScroller</c> shows.</param>
    /// <param name="Extent">How much of it there is - unit 373's floor, seen from the scroller.</param>
    private sealed record Measured(
        double Asked, double Drawn, double TopRow, double PanelRow, double Panels, double SendArea,
        double Viewport, double Extent);

    /// <summary>Realizes one window, reads the four numbers off it, and closes it.</summary>
    private static Measured Measure(double width, double height, bool plain)
    {
        var window = Realize(width, height, plain);

        try
        {
            Pump(window);

            var panels = TheWorkingPanelsTests.Panels(window);
            var canvas = TheTopRowTests.Named<ScrollViewer>(window, "WorkspaceCanvasScroller");

            return new Measured(
                height,
                window.Bounds.Height,
                TheTopRowTests.RectIn(TheTopRowTests.Named<Control>(window, "TopRow"), window).Height,
                panels[0].Rect.Height,
                panels.Sum(p => p.Rect.Height),
                TheTopRowTests.RectIn(
                    TheTopRowTests.Named<Control>(window, "DigitalSendReserved"), window).Height,
                canvas.Viewport.Height,
                canvas.Extent.Height);
        }
        finally
        {
            window.Close();
        }
    }

    private static Window Realize(double width, double height, bool plain)
        => plain
            ? TheWorkingPanelsTests.Realized(width, height, null)
            : TheTopRowTests.Realized(width, height, null, null);

    private static string Which(bool plain)
        => plain ? "the window with content" : "the pinned-facts window";

    /// <summary>Runs the dispatcher and lays the window out.</summary>
    private static void Pump(Window window)
    {
        for (var i = 0; i < 5; i++)
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
