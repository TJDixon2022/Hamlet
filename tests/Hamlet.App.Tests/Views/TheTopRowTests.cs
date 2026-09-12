using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Licensing;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 337 task 1: **the top row is one short band, and the working card gets the
/// rest.**
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-12**, on unit 334's window: the green zone grew to two thirds of the
/// window, the waterfall, the decoded list and For You were squeezed into the bottom third,
/// and a third of the window under the rig display stood empty. *"Mock up what Hamlet main
/// screen will look like if you do this right."* `assets/main-screen-mockup.png` is the
/// ruling and `PHASE_PLAN.md` R26 is its outcomes.</para>
/// <para>**FOUND BY WHAT THEY ARE, NOT BY A NAME THIS UNIT ADDED.** The neighborhood card is
/// the collapsible panel titled *Neighborhood map*, the rig panel is the bordered box around
/// the rig display, and the green block is the bordered box around the license line - so the
/// test measures the window before the change and after it with the same code, and its first
/// red is a red about geometry rather than about a missing name.</para>
/// <para>**COMPUTED, NOT SEEN** (§0.0). Every number is read off a realized headless window.
/// **The window is 1040 px tall at both widths, the unit's own choice**: a maximized window on
/// a 1080 px screen less a taskbar. The mockup is drawn at 810.</para>
/// </remarks>
public sealed class TheTopRowTests
{
    /// <summary>R26: *about 190 px tall at 1920*; the instruction's tolerance is 10%.</summary>
    public const double TopRowTarget = 190;

    /// <summary>The window height both widths are measured at - the unit's choice.</summary>
    public const double WindowHeight = 1040;

    /// <summary>The mockup's world clock: 246 x 134.</summary>
    private const double ClockHeight = 134;

    private const long Ft8On20 = 14_074_000;

    private const string HisGrid = "FN00";

    /// <summary>2 pm EDT, the instant the green zone's own tests draw the night side for.</summary>
    private static readonly DateTime TwoPmEdt = new(2026, 9, 12, 18, 0, 0, DateTimeKind.Utc);

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where every rectangle is printed.</param>
    public TheTopRowTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **Assertion 1: at 1920 the top row is about 190 px, and the working card runs from
    /// under the tabs to the status bar.**
    /// </summary>
    /// <remarks>
    /// The top row is measured as the span of its two halves - the neighborhood card and the
    /// rig panel - because that is the band the operator sees, whatever grid holds it. Both
    /// widths are printed; 1920 is asserted here and 1400 is task 3's.
    /// </remarks>
    [AvaloniaFact]
    public void AtNineteenTwentyTheTopRowIsAbout190AndTheWorkingCardTakesTheRest()
    {
        foreach (var width in new[] { 1400.0, 1920.0 })
        {
            var window = Realized(width);

            try
            {
                var m = Measure(window);

                Print(width, m);

                if (width < 1900)
                {
                    continue;
                }

                Assert.True(
                    Math.Abs(m.TopRowHeight - TopRowTarget) <= TopRowTarget * 0.10,
                    "at 1920 the top row is " + Px(m.TopRowHeight) + " px against "
                    + Px(TopRowTarget) + " within 10%");

                // **THE REST, TO THE STATUS BAR.** The working card's bottom is the status
                // bar's top less the bar's own 12 px margin, and between the top row and the
                // card there is only the divider and the tabs.
                Assert.True(
                    m.Workspace.Bottom >= m.StatusBar.Top - 12.5,
                    "the working card ends at y=" + Px(m.Workspace.Bottom)
                    + " and the status bar starts at y=" + Px(m.StatusBar.Top));

                Assert.True(
                    m.Workspace.Top - m.TopRowBottom <= 80,
                    "there are " + Px(m.Workspace.Top - m.TopRowBottom)
                    + " px between the top row and the working card, which is more than the"
                    + " divider and the tabs");
            }
            finally
            {
                window.Close();
            }
        }
    }

    /// <summary>
    /// **Assertion 2: the green block is inside the neighborhood card, under the strip and the
    /// legend, with the band as its largest text and the count at its right.**
    /// </summary>
    [AvaloniaFact]
    public void TheGreenBlockIsInsideTheCardUnderTheStripWithTheBandLargest()
    {
        var window = Realized(1920);

        try
        {
            var card = Card(window);
            var block = Block(window);
            var strip = window.GetVisualDescendants().OfType<NeighborhoodMapControl>().First();
            var legend = window.GetVisualDescendants().OfType<MapLegendControl>().First();
            var band = Named<TextBlock>(window, "GreenZoneBand");
            var left = Named<Control>(window, "GreenZoneLeft");
            var heard = Named<TextBlock>(window, "GreenZoneHeard");
            var sparkline = Named<SparklineControl>(window, "GreenZoneSparkline");

            _output.WriteLine("card     : " + Box(RectIn(card, window)));
            _output.WriteLine("strip    : " + Box(RectIn(strip, window)));
            _output.WriteLine("legend   : " + Box(RectIn(legend, window)));
            _output.WriteLine("block    : " + Box(RectIn(block, window)));
            _output.WriteLine("left     : " + Box(RectIn(left, window)));
            _output.WriteLine("heard    : " + Box(RectIn(heard, window)));
            _output.WriteLine("sparkline: " + Box(RectIn(sparkline, window)));

            foreach (var text in VisibleText(block))
            {
                _output.WriteLine("  " + Px(text.FontSize).PadLeft(5) + "  " + text.Text);
            }

            Assert.True(block.GetVisualAncestors().Contains(card), "the green block is not inside the card");
            Assert.True(
                RectIn(block, window).Top >= RectIn(legend, window).Bottom - 0.5
                && RectIn(block, window).Top >= RectIn(strip, window).Bottom - 0.5,
                "the green block is not under the strip and the legend");

            // **THE BAND IS THE LARGEST TEXT IN THE BLOCK AND IN THE CARD.**
            Assert.True(band.IsEffectivelyVisible, "the band is not drawn");
            Assert.All(
                VisibleText(card).Where(t => !ReferenceEquals(t, band)),
                t => Assert.True(
                    t.FontSize < band.FontSize,
                    "[" + t.Text + "] at " + Px(t.FontSize) + " is as large as the band at "
                    + Px(band.FontSize)));

            // **THE COUNT AND ITS SPARKLINE AT THE BLOCK'S RIGHT.**
            Assert.True(heard.IsEffectivelyVisible, "the heard count is not drawn");
            Assert.True(
                RectIn(heard, window).Left >= RectIn(left, window).Right - 0.5
                && RectIn(sparkline, window).Left >= RectIn(left, window).Right - 0.5,
                "the count and the sparkline are not right of the band's block");
            Assert.True(
                block.GetVisualDescendants().Contains(heard)
                && block.GetVisualDescendants().Contains(sparkline),
                "the count and the sparkline are not in the green block");
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// **Assertion 3: the world clock is in the card at its right end, at the mockup's size,
    /// with one marker.**
    /// </summary>
    [AvaloniaFact]
    public void TheWorldClockIsAtTheCardsRightEndWithOneMarker()
    {
        var window = Realized(1920);

        try
        {
            var card = Card(window);
            var block = Block(window);
            var clock = Named<GrayLineMapControl>(window, "GreenZoneGrayLine");

            var cardAt = RectIn(card, window);
            var blockAt = RectIn(block, window);
            var clockAt = RectIn(clock, window);

            var drawn = GrayLineMapControl.WhatWouldBeDrawn(
                clock.Utc, clock.OperatorGrid, clock.Bounds.Width, clock.Bounds.Height);

            _output.WriteLine("card  : " + Box(cardAt));
            _output.WriteLine("block : " + Box(blockAt));
            _output.WriteLine("clock : " + Box(clockAt) + ", markers " + drawn.Markers);

            Assert.True(clock.GetVisualAncestors().Contains(card), "the world clock is not in the card");
            Assert.False(
                clock.GetVisualAncestors().Contains(block),
                "the world clock is inside the green block rather than at the card's right end");

            Assert.True(
                clockAt.Left >= blockAt.Right - 0.5,
                "the clock starts at x=" + Px(clockAt.Left) + " and the green block ends at x="
                + Px(blockAt.Right) + ", so it is not at the card's right");
            Assert.True(
                cardAt.Right - clockAt.Right <= 40,
                "the clock ends " + Px(cardAt.Right - clockAt.Right) + " px short of the card's right edge");

            Assert.InRange(clockAt.Height, ClockHeight * 0.9, ClockHeight * 1.1);

            Assert.Equal(HisGrid, clock.OperatorGrid);
            Assert.Equal(1, drawn.Markers);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// **Assertion 4: drive and the power offer are in the rig panel under the S-meter, and not
    /// in the send area; the send area keeps CQ and Stop; the rig panel is the card's height.**
    /// </summary>
    [AvaloniaFact]
    public void DriveAndThePowerOfferAreUnderTheRigAndTheSendAreaKeepsCqAndStop()
    {
        var window = Realized(1920);

        try
        {
            var rig = window.GetVisualDescendants().OfType<RigDisplayControl>().First();
            var panel = RigPanel(window);
            var card = Card(window);
            var reserved = Named<Border>(window, "DigitalSendReserved");
            var drive = Named<NumericUpDown>(window, "DigitalTransmitDriveBox");
            var offer = Named<TextBlock>(window, "DigitalPsk31PowerOffer");

            _output.WriteLine("rig display: " + Box(RectIn(rig, window)));
            _output.WriteLine("rig panel  : " + Box(RectIn(panel, window)));
            _output.WriteLine("card       : " + Box(RectIn(card, window)));
            _output.WriteLine("drive box  : " + Box(RectIn(drive, window)));

            foreach (var (name, control) in new (string, Control)[] { ("drive", drive), ("power offer", offer) })
            {
                Assert.True(
                    control.GetVisualAncestors().Contains(panel),
                    "the " + name + " is not in the rig panel");
                Assert.False(
                    control.GetVisualAncestors().Contains(reserved),
                    "the " + name + " is still in the send area");
            }

            Assert.True(
                RectIn(drive, window).Top >= RectIn(rig, window).Bottom - 0.5,
                "the drive is not under the rig display's S-meter");

            foreach (var name in new[] { "DigitalSendCqButton", "DigitalStopButton" })
            {
                Assert.True(
                    Named<Button>(window, name).GetVisualAncestors().Contains(reserved),
                    name + " has left the send area");
            }

            Assert.True(
                Math.Abs(RectIn(panel, window).Height - RectIn(card, window).Height) <= 1,
                "the rig panel is " + Px(RectIn(panel, window).Height) + " px and the card is "
                + Px(RectIn(card, window).Height) + " px, so they are not one height");
        }
        finally
        {
            window.Close();
        }
    }

    // ------------------------------------------------------------------------------------

    /// <summary>What the layout measured on one window, in the window's own frame.</summary>
    public sealed record Measured(
        Rect Card,
        Rect Rig,
        Rect Workspace,
        Rect StatusBar,
        Rect Waterfall,
        Rect Decoded,
        Rect ForYou)
    {
        /// <summary>The top of the band the two halves make.</summary>
        public double TopRowTop => Math.Min(Card.Top, Rig.Top);

        /// <summary>The bottom of the band the two halves make.</summary>
        public double TopRowBottom => Math.Max(Card.Bottom, Rig.Bottom);

        /// <summary>How tall the top row is.</summary>
        public double TopRowHeight => TopRowBottom - TopRowTop;
    }

    /// <summary>Measures the window the same way before and after the change.</summary>
    /// <param name="window">A realized window.</param>
    /// <returns>The rectangles.</returns>
    public static Measured Measure(Window window)
        => new(
            RectIn(Card(window), window),
            RectIn(RigPanel(window), window),
            RectIn(Named<Control>(window, "WorkspaceBoundary"), window),
            RectIn(Named<Control>(window, "StatusBar"), window),
            RectIn(Named<Control>(window, "DigitalWaterfallPanel"), window),
            RectIn(Named<Control>(window, "DigitalDecodedPanel"), window),
            RectIn(Named<Control>(window, "DigitalMinePanel"), window));

    private void Print(double width, Measured m)
    {
        _output.WriteLine("WINDOW " + Px(width) + " x " + Px(WindowHeight));
        _output.WriteLine("  neighborhood card: " + Box(m.Card));
        _output.WriteLine("  rig panel        : " + Box(m.Rig));
        _output.WriteLine("  top row          : y " + Px(m.TopRowTop) + " to " + Px(m.TopRowBottom) + " = " + Px(m.TopRowHeight) + " px");
        _output.WriteLine("  working card     : " + Box(m.Workspace) + " = " + Px(m.Workspace.Height) + " px tall");
        _output.WriteLine("  status bar       : " + Box(m.StatusBar));
        _output.WriteLine("  waterfall        : " + Box(m.Waterfall));
        _output.WriteLine("  decoded text     : " + Box(m.Decoded));
        _output.WriteLine("  For You          : " + Box(m.ForYou));
        _output.WriteLine("");
    }

    /// <summary>The neighborhood card: the collapsible panel titled Neighborhood map.</summary>
    public static CollapsiblePanel Card(Window window)
        => window.GetVisualDescendants().OfType<CollapsiblePanel>()
            .First(p => p.Title == "Neighborhood map");

    /// <summary>The rig panel: the bordered box around the rig display.</summary>
    public static Border RigPanel(Window window)
        => window.GetVisualDescendants().OfType<RigDisplayControl>().First()
            .GetVisualAncestors().OfType<Border>()
            .First(b => b.BorderThickness.Left > 0);

    /// <summary>The green block: the bordered box around the license line.</summary>
    public static Border Block(Window window)
        => Named<TextBlock>(window, "GreenZoneLicenseLine").GetVisualAncestors()
            .OfType<Border>()
            .First(b => b.BorderThickness.Left > 0);

    /// <summary>A control's rectangle in the window's frame.</summary>
    public static Rect RectIn(Control control, Window window)
    {
        var at = control.TranslatePoint(new Point(0, 0), window) ?? default;

        return new Rect(at, control.Bounds.Size);
    }

    /// <summary>Finds a control by name with a visual walk, across template name scopes.</summary>
    public static T Named<T>(Window window, string name)
        where T : Control
    {
        var found = window.GetVisualDescendants().OfType<T>().FirstOrDefault(c => c.Name == name);

        Assert.True(found is not null, "there is no " + typeof(T).Name + " named " + name);

        return found!;
    }

    private static IEnumerable<TextBlock> VisibleText(Visual root)
        => root.GetVisualDescendants().OfType<TextBlock>()
            .Where(t => t.IsEffectivelyVisible && (t.Text ?? "").Trim().Length > 0);

    /// <summary>
    /// The main window on 20 m FT8 with a General license, a grid, a heard count, and every
    /// panel of the digital tab open.
    /// </summary>
    /// <param name="width">How wide the window is.</param>
    public static Window Realized(double width)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.LicenseClass = LicenseClass.General;
        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = HisGrid;

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
        };

        model.SelectBandCommand.Execute(model.Bands.First(b => b.Band.Name == "20 m"));
        model.FrequencyHz = Ft8On20;
        model.ChosenDigitalMode = "FT8";
        model.MapExpanded = true;
        model.GrayLineUtc = TwoPmEdt;
        model.HeardInTheLastMinute = 6;
        model.HeardSparkline = GreenZone.Sparkline(
            new[] { 3, 9, 14, 30, 44, 58 }.Select(s => TwoPmEdt.AddSeconds(-s)), TwoPmEdt);
        model.DigitalWaterfallExpanded = true;
        model.DigitalDecodedExpanded = true;
        model.DigitalMineExpanded = true;

        var window = new MainWindow { DataContext = model, Width = width, Height = WindowHeight };

        window.Show();

        for (var i = 0; i < 6; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }

        return window;
    }

    private static string Px(double value)
        => value.ToString("0.0", CultureInfo.InvariantCulture);

    private static string Box(Rect r)
        => Px(r.Width) + " x " + Px(r.Height) + " at " + Px(r.X) + "," + Px(r.Y);
}
