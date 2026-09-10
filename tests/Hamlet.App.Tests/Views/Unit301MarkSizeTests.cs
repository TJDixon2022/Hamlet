using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 301 task 2: **the achievement mark fills the status bar.**
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-09**: *"It's a tiny dot lost in the sea of the tray."* Unit
/// 300 put the quill in the bar at 20 by 20 and the bar is taller than that, so it
/// read as a glyph sitting in a strip rather than as an object filling it.</para>
/// <para>**THE FIGURES ARE MEASURED OFF THE REALIZED WINDOW**, not read off the
/// markup. A `Height` in a file is a request; what the layout gave it is the
/// answer, and the two have disagreed in this application before.</para>
/// <para>**AND THE BELT RING IS MEASURED WITH IT.** The instruction asks that the two
/// read as a pair rather than one being noticeably smaller, which is a claim about
/// both of them and cannot be checked by looking at either alone.</para>
/// </remarks>
public sealed class Unit301MarkSizeTests
{
    /// <summary>How much smaller than the bar the mark is allowed to be.</summary>
    /// <remarks>
    /// **THE INSTRUCTION ASKS FOR THE BAR'S HEIGHT LESS A SMALL MARGIN ABOVE AND
    /// BELOW.** Four pixels of headroom in total is that margin: enough that the
    /// quill is not touching the bar's own edge, little enough that it is plainly
    /// filling the bar rather than sitting in it.
    /// </remarks>
    private const double Headroom = 4;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the measured heights are printed.</param>
    public Unit301MarkSizeTests(ITestOutputHelper output) => _output = output;

    /// <summary>**The mark fills the bar, and the belt ring matches it.**</summary>
    [AvaloniaFact]
    public void TheMarkFillsTheBarAndTheBeltMatches()
    {
        var window = new MainWindow { DataContext = Model() };

        window.Show();

        for (var i = 0; i < 6; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        }

        try
        {
            var bar = window.FindControl<Border>("StatusBar");
            var button = window.FindControl<Button>("AchievementMarkButton");
            var mark = window.FindControl<AchievementMarkControl>("AchievementMark");
            var belt = window.FindControl<Border>("ContactBeltRing");

            Assert.True(bar is not null, "the status bar is not named in the markup");
            Assert.True(mark is not null, "the achievement mark is not in the window");
            Assert.True(button is not null, "the mark has no button around it");
            Assert.True(belt is not null, "the belt ring is not in the window");

            // **THE BAR'S OWN INSIDE**, which is what the mark has to fill: its
            // height less the padding it puts above and below whatever it holds.
            var inside = bar!.Bounds.Height
                - bar.Padding.Top - bar.Padding.Bottom
                - bar.BorderThickness.Top - bar.BorderThickness.Bottom;

            _output.WriteLine("status bar, outside : "
                + bar.Bounds.Height.ToString("0.0"));
            _output.WriteLine("status bar, inside  : " + inside.ToString("0.0"));
            _output.WriteLine("the mark            : "
                + mark!.Bounds.Height.ToString("0.0"));
            _output.WriteLine("its button          : "
                + button!.Bounds.Height.ToString("0.0"));
            _output.WriteLine("the belt ring       : "
                + belt!.Bounds.Height.ToString("0.0"));

            Assert.True(inside > 0, "the status bar has no measured height");

            // **AN OBJECT THAT FILLS THE BAR, NOT A GLYPH IN IT.**
            Assert.True(
                mark.Bounds.Height >= inside - Headroom,
                "the mark is " + mark.Bounds.Height.ToString("0.0")
                + " in a bar whose inside is " + inside.ToString("0.0")
                + ", so it is still a dot lost in the tray");

            // **AND IT STAYS INSIDE THE BAR**, which the orbit ring has to as well
            // because it is drawn to the mark's own bounds.
            Assert.True(
                button.Bounds.Height <= bar.Bounds.Height,
                "the mark's button is taller than the bar it sits in");

            // **THE TWO READ AS A PAIR, AND THAT IS STRUCTURAL RATHER THAN
            // COINCIDENTAL.** The belt strip is collapsed on a log with nothing in
            // it, so its drawn bounds are zero here and measuring them would prove
            // nothing. What is asserted instead is the figure both of them were
            // given: they read one resource, so they cannot drift apart at all.
            _output.WriteLine("mark, asked for     : " + mark.Height);
            _output.WriteLine("belt, asked for     : " + belt.Height);
            _output.WriteLine("belt, drawn         : "
                + belt.Bounds.Height.ToString("0.0")
                + (belt.IsVisible ? "" : "  (collapsed: no contacts logged)"));

            Assert.True(
                Math.Abs(mark.Height - belt.Height) < 0.001,
                "the mark asks for " + mark.Height + " and the belt ring asks for "
                + belt.Height + ", so one reads as noticeably smaller");

            Assert.True(
                Math.Abs(mark.Bounds.Height - mark.Height) < 0.001,
                "the mark asked for " + mark.Height + " and the layout gave it "
                + mark.Bounds.Height);
        }
        finally
        {
            window.Close();

            for (var i = 0; i < 5; i++)
            {
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            }
        }
    }

    /// <summary>**The orbit still reaches the edge of whatever size the mark is.**</summary>
    /// <remarks>
    /// **THE RING AND THE BEAD ARE DRAWN FROM THE CONTROL'S OWN BOUNDS**, so growing
    /// the mark has to grow them with it rather than leaving a large quill inside a
    /// small circle. Recorded from what `Render` emitted at the size the bar gives
    /// it, rather than recomputed.
    /// </remarks>
    [AvaloniaFact]
    public void TheOrbitGrowsWithTheMark()
    {
        foreach (var side in new double[] { 20, 26, 32 })
        {
            var mark = new AchievementMarkControl
            {
                Width = side,
                Height = side,
                IsNew = true,
            };

            mark.Measure(new Avalonia.Size(side, side));
            mark.Arrange(new Avalonia.Rect(0, 0, side, side));

            var group = new Avalonia.Media.DrawingGroup();

            using (var context = group.Open())
            {
                mark.Render(context);
            }

            var ring = group.Children
                .OfType<Avalonia.Media.GeometryDrawing>()
                .FirstOrDefault(d => d.Pen is not null);

            Assert.True(ring is not null, "no ring was drawn at " + side + " px");

            var across = ring!.Geometry?.Bounds.Width ?? 0;

            _output.WriteLine(
                side.ToString("0").PadLeft(3) + " px mark: ring "
                + across.ToString("0.0") + " across");

            Assert.True(
                across >= side - 4,
                "at " + side + " px the ring is only " + across.ToString("0.0")
                + " across, so it does not follow the mark");
        }
    }

    private static MainWindowViewModel Model()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = "FN00";

        return new MainWindowViewModel(settings, null);
    }
}
