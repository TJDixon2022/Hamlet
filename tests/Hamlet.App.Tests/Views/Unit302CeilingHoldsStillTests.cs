using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 302 task 2: **the character ceiling measures something that does
/// not move.**
/// </summary>
/// <remarks>
/// <para>**A RED TEST THAT STAYS RED GETS IGNORED.** Two ceiling rows were red for
/// three units running because the figure they cap moves by itself: 1281 and 1293 an
/// hour apart in unit 300, 1259 in unit 301. This project has already paid for a
/// standing red once, when `TheMenuIsUnderTheMouseTests` sat at 5 of 6 failing for
/// three units.</para>
/// <para>**WHAT MOVES, MEASURED RATHER THAN SEARCHED FOR** (unit 284's rule, and this
/// instruction's). Standing the Digital tab up twice 37 seconds apart, the two runs
/// differed by exactly one line: the slot countdown, `10` and then `3`. A second,
/// slower one is the card's relative age, which grows a digit as an evening
/// wears on.</para>
/// <para>**BOTH STAY ON SCREEN.** The countdown is Tim's own ask and the card's
/// staleness is why its time line exists; both are live measurements rather than
/// prose, and the ceiling exists to stop prose growing. **So the sweep counts each at
/// its widest reading**, which is the instruction's first branch: the moving part
/// belongs on screen, so the ceiling measures it with its width bounded.</para>
/// </remarks>
public sealed class Unit302CeilingHoldsStillTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the two measurements are printed.</param>
    public Unit302CeilingHoldsStillTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>**Two measurements of the same tree return the same figure.**</summary>
    /// <remarks>
    /// **THE SECOND SWEEP IS TAKEN AFTER THE COUNTDOWN HAS MOVED**, not immediately:
    /// two reads in the same instant would agree however unstable the surface is,
    /// which is the test passing for the wrong reason. It waits for the live readout
    /// to actually change and only then re-measures.
    /// </remarks>
    [AvaloniaFact]
    public void TwoMeasurementsOfTheSameTreeAgree()
    {
        foreach (var surface in new[]
        {
            "MainWindow — Digital tab",
            "MainWindow — Digital tab, working",
            "MainWindow — CW tab",
        })
        {
            var first = Measure(surface, out var firstLive);

            // **WAIT UNTIL A LIVE READOUT HAS REALLY MOVED.** The countdown ticks
            // once a second; a second and a half is comfortably past a tick and is
            // nowhere near the watchdog.
            var until = DateTime.UtcNow.AddSeconds(1.5);

            while (DateTime.UtcNow < until)
            {
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            }

            // **AND BUILD THE OTHER SURFACES IN BETWEEN**, because that is the
            // condition the ceiling sweep actually runs in: it realizes thirteen
            // windows in one process, one after another. Measured, this surface was
            // rock steady on its own across thirty-four sweeps and still read 1305
            // once inside the full class, so *alone* is not the state to prove it in.
            foreach (var other in new[] { "MainWindow — CW tab", "AboutWindow" })
            {
                var noise = HowMuchTheApplicationSaysTests.SurfaceForSweep(other);

                noise.Close();
            }

            var second = Measure(surface, out var secondLive);

            _output.WriteLine(
                surface.PadRight(34)
                + first.ToString(CultureInfo.InvariantCulture).PadLeft(5)
                + "  then "
                + second.ToString(CultureInfo.InvariantCulture).PadLeft(5)
                + (first == second ? "   same" : "   MOVED"));

            _output.WriteLine("      live readouts, first  : " + firstLive);
            _output.WriteLine("      live readouts, second : " + secondLive);

            Assert.True(
                first == second,
                surface + " measured " + first + " and then " + second
                + ", so the ceiling is being set against a moving number");
        }
    }

    /// <summary>
    /// **The countdown is still on screen, and it is still the clock's.**
    /// </summary>
    /// <remarks>
    /// **BOUNDING WHAT THE SWEEP COUNTS MUST NOT QUIETLY REMOVE THE READOUT.** Tim
    /// asked for the countdown outright, so a repair that made the ceiling stable by
    /// taking it off the screen would be the wrong repair passing its own test.
    /// </remarks>
    [AvaloniaFact]
    public void TheLiveReadoutsAreStillOnScreen()
    {
        var window = HowMuchTheApplicationSaysTests.SurfaceForSweep(
            "MainWindow — Digital tab, working");

        try
        {
            var countdown = Named(window, "TurnRingCountText");
            var timeLine = Named(window, "CardTimeLineText");

            _output.WriteLine("the slot countdown : " + Quoted(countdown));
            _output.WriteLine("the card time line : " + Quoted(timeLine));

            Assert.True(
                countdown is not null,
                "the slot countdown is no longer on the Digital tab");

            Assert.True(
                timeLine is not null,
                "the card's time line is no longer on the Digital tab");

            Assert.True(
                countdown!.IsEffectivelyVisible,
                "the slot countdown is on the tab but not visible");

            Assert.True(
                timeLine!.IsEffectivelyVisible,
                "the card's time line is on the tab but not visible");
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>One sweep of a surface, with what its live readouts said.</summary>
    private static int Measure(string surface, out string live)
    {
        var window = HowMuchTheApplicationSaysTests.SurfaceForSweep(surface);

        try
        {
            var readouts = new List<string>();

            foreach (var name in new[] { "TurnRingCountText", "CardTimeLineText" })
            {
                if (Named(window, name) is { } block)
                {
                    readouts.Add(name + "=" + Quoted(block));
                }
            }

            live = readouts.Count == 0 ? "(none on this surface)"
                : string.Join("  ", readouts);

            return HowMuchTheApplicationSaysTests.CharsForSweep(window);
        }
        finally
        {
            window.Close();
        }
    }

    private static TextBlock? Named(Window window, string name)
        => window.GetVisualDescendants()
            .OfType<TextBlock>()
            .FirstOrDefault(b => b.Name == name);

    private static string Quoted(TextBlock? block)
        => block is null ? "(not on this surface)" : "\"" + block.Text + "\"";
}
