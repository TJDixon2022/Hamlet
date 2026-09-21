using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 372 task 2, criterion 1.2: **the order in which height is given up.**
/// </summary>
/// <remarks>
/// <para>**THE RULE IS THE TREE'S AND UNIT 356'S, NOT THIS UNIT'S.** It is written above
/// <c>TopRow</c> in <c>src/Hamlet.App/Views/MainWindow.axaml</c>: *the working panels give up
/// height first, then the top row, and the send area is never the thing that leaves.* Unit 356
/// capped the row at <c>MaxHeight="300"</c> and asserted the nine sizes; **what it did not do was
/// measure the order**, and a comment is not a test. R34 (Tim, 2026-09-14) was ruled about exactly
/// this state of affairs: at 1100 x 780, the size Hamlet opens at, CQ was drawn at y 800 and the
/// mode tabs at y 847, below a window 780 tall.</para>
/// <para>**WHAT THE TREE ALREADY DID, MEASURED BEFORE THIS TYPE WAS WRITTEN**
/// (<c>Unit372TraceTests</c>, at <c>d14badb1</c>): the send area is 22 px at every height from 780
/// down to 620; <c>TopRow</c> is 300 px - its cap - at every one of them and never gives up a
/// pixel; the working panels give up all of it, 73 px at 780, 33 at 740 and 0 from 700 down. So all
/// four assertions below hold against the tree as it stands and **no source file was changed for
/// this type.** That is the state R34 was ruled about - a rule that holds with nothing asserting it
/// - and 1.2 asks for the rule *stated and held*, so the type is written and committed anyway.</para>
/// <para>**WATCHED AGAINST THE TREE AT TASK 0, AND HOW.** The instruction asked for a worktree at
/// task 0's commit; this session was not permitted to create one, so the same thing was established
/// the way a worktree would have: <c>git diff d14badb1 -- src/</c> is **empty**, so the application
/// these four names ran against is task 0's application byte for byte, and everything this unit had
/// added was a test. **All four were green on that tree**, 4 of 4 in 17 s. Nothing was watched
/// failing because there was nothing failing to watch, and that is the finding 1.2 was written to
/// get rather than a step that was skipped.</para>
/// <para>**WHY THE SWEEP STOPS AT 620.** That is <c>MinHeight</c>, and the window will not go below
/// it: asked for 580, 540 and 500, a headless window drew 620 each time. What happens at and below
/// the minimum is criterion 1.3's question and <c>TheWindowHoldsBelowItsMinimumTests</c>'.</para>
/// <para>**BOTH WINDOWS, BECAUSE ONE OF THEM HAS NOTHING IN ITS PANELS.**
/// <c>TheTopRowTests.Realized</c> declares the pinned facts and no decoded rows;
/// <c>TheWorkingPanelsTests.Realized</c> carries the longest line the decoded list has to hold and
/// a card. A claim about Hamlet rather than about one fixture has to hold on both.</para>
/// <para>**COMPUTED, NOT SEEN** (<c>SHACK_FACTS.md</c> FACT-004). Every number is a <c>Bounds</c>
/// rectangle off a realized headless window. Nothing is pressed (CLAUDE.md §0.2), nothing is
/// opened, and nothing here is evidence about the radio.</para>
/// </remarks>
public sealed class TheWindowGivesUpHeightInOneOrderTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where every height's numbers are printed.</param>
    public TheWindowGivesUpHeightInOneOrderTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// The descending sweep at width 1100, from the size Hamlet opens at down to <c>MinHeight</c>.
    /// </summary>
    private static readonly double[] Sweep = { 780, 740, 700, 660, 620 };

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
    /// height above and <c>TopRow</c> is unchanged.
    /// </summary>
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
    private sealed record Measured(
        double Asked, double Drawn, double TopRow, double PanelRow, double Panels, double SendArea);

    /// <summary>Realizes one window, reads the four numbers off it, and closes it.</summary>
    private static Measured Measure(double width, double height, bool plain)
    {
        var window = Realize(width, height, plain);

        try
        {
            Pump(window);

            var panels = TheWorkingPanelsTests.Panels(window);

            return new Measured(
                height,
                window.Bounds.Height,
                TheTopRowTests.RectIn(TheTopRowTests.Named<Control>(window, "TopRow"), window).Height,
                panels[0].Rect.Height,
                panels.Sum(p => p.Rect.Height),
                TheTopRowTests.RectIn(
                    TheTopRowTests.Named<Control>(window, "DigitalSendReserved"), window).Height);
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
