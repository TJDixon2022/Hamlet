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
    /// <para>**WORK INSTRUCTION 340 TASK 1: ON PSK31 AS WELL**, where the power offer is drawn in
    /// the rig panel (ruling 6), with the same 10% tolerance. FT8 is put back before the window
    /// closes.</para>
    /// </remarks>
    [AvaloniaFact]
    public void AtNineteenTwentyTheTopRowIsAbout190AndTheWorkingCardTakesTheRest()
    {
        foreach (var (width, mode) in new[] { (1400.0, "FT8"), (1920.0, "FT8"), (1400.0, "PSK31"), (1920.0, "PSK31") })
        {
            var window = Realized(width);
            var model = (MainWindowViewModel)window.DataContext!;

            try
            {
                model.ChosenDigitalMode = mode;
                Settle(window);

                var m = Measure(window);

                _output.WriteLine("MODE " + mode);
                Print(width, m);

                if (width < 1900)
                {
                    continue;
                }

                Assert.True(
                    Math.Abs(m.TopRowHeight - TopRowTarget) <= TopRowTarget * 0.10,
                    "at 1920 on " + mode + " the top row is " + Px(m.TopRowHeight) + " px against "
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
                model.ChosenDigitalMode = "FT8";
                window.Close();
            }
        }
    }

    /// <summary>
    /// **Assertion 2: the green block is inside the neighborhood card, under the strip and the
    /// legend, with the band as its largest text and the count at its right** - at 1920 and at
    /// 1400.
    /// </summary>
    /// <remarks>
    /// **WORK INSTRUCTION 339 TASK 1: BOTH WIDTHS, AND THE SPARKLINE WHERE RULING 3 KEEPS IT.**
    /// The width rule is `MainWindow.FitTheHeardCount`, and it hides the sparkline only where the
    /// text column beside it would wrap. So the test asserts what the rule drew, not a width: with
    /// the sparkline drawn, the license line and the rule of thumb each sit on one line beside it;
    /// with it hidden, *heard just now* stands over the count.
    /// </remarks>
    [AvaloniaFact]
    public void TheGreenBlockIsInsideTheCardUnderTheStripWithTheBandLargest()
    {
        foreach (var width in new[] { 1920.0, 1400.0 })
        {
            var window = Realized(width);

            try
            {
                var card = Card(window);
                var block = Block(window);
                var strip = window.GetVisualDescendants().OfType<NeighborhoodMapControl>().First();
                var legend = window.GetVisualDescendants().OfType<MapLegendControl>().First();
                var band = Named<TextBlock>(window, "GreenZoneBand");
                var left = Named<Control>(window, "GreenZoneLeft");
                var heard = Named<TextBlock>(window, "GreenZoneHeard");
                var label = Named<TextBlock>(window, "GreenZoneHeardLabel");
                var sparkline = Named<SparklineControl>(window, "GreenZoneSparkline");
                var wrapped = new[] { "GreenZoneLicenseLine", "GreenZoneRuleOfThumb" }
                    .Select(name => Named<TextBlock>(window, name))
                    .Where(t => t.IsEffectivelyVisible && t.TextLayout.TextLines.Count > 1)
                    .Select(t => t.Name + " (" + t.TextLayout.TextLines.Count + " lines)")
                    .ToList();

                _output.WriteLine("WINDOW " + Px(width) + " x " + Px(WindowHeight) + ", licensed");
                _output.WriteLine("  card     : " + Box(RectIn(card, window)));
                _output.WriteLine("  strip    : " + Box(RectIn(strip, window)));
                _output.WriteLine("  legend   : " + Box(RectIn(legend, window)));
                _output.WriteLine("  block    : " + Box(RectIn(block, window)));
                _output.WriteLine("  left     : " + Box(RectIn(left, window)));
                _output.WriteLine("  label    : " + Box(RectIn(label, window)) + " visible " + label.IsEffectivelyVisible);
                _output.WriteLine("  heard    : " + Box(RectIn(heard, window)));
                _output.WriteLine(
                    "  sparkline: " + (sparkline.IsEffectivelyVisible ? Box(RectIn(sparkline, window)) : "hidden by the width rule")
                    + "; lines that wrap [" + string.Join(", ", wrapped) + "]");

                foreach (var text in VisibleText(block))
                {
                    _output.WriteLine("    " + Px(text.FontSize).PadLeft(5) + "  " + text.Text);
                }

                Assert.True(block.GetVisualAncestors().Contains(card), "at " + Px(width) + " the green block is not inside the card");
                Assert.True(
                    RectIn(block, window).Top >= RectIn(legend, window).Bottom - 0.5
                    && RectIn(block, window).Top >= RectIn(strip, window).Bottom - 0.5,
                    "at " + Px(width) + " the green block is not under the strip and the legend");

                // **THE BAND IS THE LARGEST TEXT IN THE BLOCK AND IN THE CARD.**
                Assert.True(band.IsEffectivelyVisible, "at " + Px(width) + " the band is not drawn");
                Assert.All(
                    VisibleText(card).Where(t => !ReferenceEquals(t, band)),
                    t => Assert.True(
                        t.FontSize < band.FontSize,
                        "at " + Px(width) + " [" + t.Text + "] at " + Px(t.FontSize) + " is as large as the band at "
                        + Px(band.FontSize)));

                // **THE COUNT, IN THE GREEN BLOCK AT ITS RIGHT, AT EVERY WIDTH.**
                Assert.True(heard.IsEffectivelyVisible, "at " + Px(width) + " the heard count is not drawn");
                Assert.True(
                    block.GetVisualDescendants().Contains(heard),
                    "at " + Px(width) + " the count is not in the green block");
                Assert.True(
                    RectIn(heard, window).Left >= RectIn(left, window).Right - 0.5,
                    "at " + Px(width) + " the count is not right of the band's block");

                if (sparkline.IsEffectivelyVisible)
                {
                    // **RULING 3 KEEPS IT**: the sparkline beside the count, and the text column
                    // beside them holds its lines without wrapping.
                    Assert.True(
                        block.GetVisualDescendants().Contains(sparkline)
                        && RectIn(sparkline, window).Left >= RectIn(left, window).Right - 0.5,
                        "at " + Px(width) + " the sparkline is not in the green block right of the band's block");
                    Assert.True(
                        wrapped.Count == 0,
                        "at " + Px(width) + " the sparkline is drawn and the text beside it wraps: " + string.Join(", ", wrapped));
                }
                else
                {
                    // **RULING 3 HID IT**: *heard just now* stands over the count in its place.
                    var labelAt = RectIn(label, window);
                    var heardAt = RectIn(heard, window);

                    Assert.True(label.IsEffectivelyVisible, "at " + Px(width) + " the sparkline is hidden and *heard just now* is not drawn");
                    Assert.True(
                        block.GetVisualDescendants().Contains(label) && labelAt.Left >= RectIn(left, window).Right - 0.5,
                        "at " + Px(width) + " *heard just now* is not in the green block right of the band's block");
                    Assert.True(
                        labelAt.Bottom <= heardAt.Top + 0.5 && labelAt.Left < heardAt.Right && heardAt.Left < labelAt.Right,
                        "at " + Px(width) + " *heard just now* is " + Box(labelAt) + " and the count " + Box(heardAt)
                        + ", so it does not stand over the count");
                }
            }
            finally
            {
                window.Close();
            }
        }
    }

    /// <summary>
    /// **Assertion 3: the world clock is in the card at its right end, at the mockup's size,
    /// with one marker** - at 1920 and at 1400.
    /// </summary>
    /// <remarks>
    /// **WORK INSTRUCTION 339 TASK 1: BOTH WIDTHS**, with the assertions it made at 1920. R26 says
    /// *at 1400 the same shape*, and the card is 520 px narrower there.
    /// </remarks>
    [AvaloniaFact]
    public void TheWorldClockIsAtTheCardsRightEndWithOneMarker()
    {
        foreach (var width in new[] { 1920.0, 1400.0 })
        {
            var window = Realized(width);

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

                _output.WriteLine("WINDOW " + Px(width) + " x " + Px(WindowHeight) + ", licensed");
                _output.WriteLine("  card  : " + Box(cardAt));
                _output.WriteLine("  block : " + Box(blockAt));
                _output.WriteLine(
                    "  clock : " + Box(clockAt) + ", markers " + drawn.Markers + ", "
                    + Px(cardAt.Right - clockAt.Right) + " px from the card's right edge");

                Assert.True(clock.GetVisualAncestors().Contains(card), "at " + Px(width) + " the world clock is not in the card");
                Assert.False(
                    clock.GetVisualAncestors().Contains(block),
                    "at " + Px(width) + " the world clock is inside the green block rather than at the card's right end");

                Assert.True(
                    clockAt.Left >= blockAt.Right - 0.5,
                    "at " + Px(width) + " the clock starts at x=" + Px(clockAt.Left) + " and the green block ends at x="
                    + Px(blockAt.Right) + ", so it is not at the card's right");
                Assert.True(
                    cardAt.Right - clockAt.Right <= 40,
                    "at " + Px(width) + " the clock ends " + Px(cardAt.Right - clockAt.Right) + " px short of the card's right edge");

                Assert.InRange(clockAt.Height, ClockHeight * 0.9, ClockHeight * 1.1);

                Assert.Equal(HisGrid, clock.OperatorGrid);
                Assert.Equal(1, drawn.Markers);
            }
            finally
            {
                window.Close();
            }
        }
    }

    /// <summary>
    /// **Assertion 4: drive and the power offer are in the rig panel under the S-meter, and not
    /// in the send area; the send area keeps CQ and Stop; the rig panel is the card's height** -
    /// at 1920 and at 1400.
    /// </summary>
    /// <remarks>
    /// <para>**WORK INSTRUCTION 339 TASK 1: BOTH WIDTHS.** The rig panel's height is the card's at
    /// each width, whatever that height is: 190 px at 1920 and 219 at 1400 on this fixture in unit
    /// 338.</para>
    /// <para>**CQ AND STOP WHERE THE ARBITER'S RULING 5 LEAVES THEM**: in the send area, which
    /// rides the tab row beside `ModeTabs`, above the working card and inside nothing that
    /// collapses (§0.2). This test reads where they are and presses nothing.</para>
    /// <para>**WORK INSTRUCTION 340 TASK 1: THE OFFER DRAWN, ON PSK31** (the arbiter's ruling 6).
    /// On FT8 the offer is not drawn, so containment was all that could be asserted. On PSK31 its
    /// border is on the screen, and the test asserts it is visible, in the rig panel, out of the
    /// send area and under the rig display, with the rig panel still the card's height. Every FT8
    /// assertion is kept. FT8 is put back on the model before each window closes, and nothing is
    /// pressed (§0.2, HM-DEC-084).</para>
    /// </remarks>
    [AvaloniaFact]
    public void DriveAndThePowerOfferAreUnderTheRigAndTheSendAreaKeepsCqAndStop()
    {
        foreach (var (width, mode) in new[] { (1920.0, "FT8"), (1920.0, "PSK31"), (1400.0, "FT8"), (1400.0, "PSK31") })
        {
            var window = Realized(width);
            var model = (MainWindowViewModel)window.DataContext!;

            try
            {
                model.ChosenDigitalMode = mode;
                Settle(window);

                var rig = window.GetVisualDescendants().OfType<RigDisplayControl>().First();
                var panel = RigPanel(window);
                var card = Card(window);
                var reserved = Named<Border>(window, "DigitalSendReserved");
                var tabs = Named<Control>(window, "ModeTabs");
                var workspace = Named<Control>(window, "WorkspaceBoundary");
                var drive = Named<NumericUpDown>(window, "DigitalTransmitDriveBox");
                var offer = Named<TextBlock>(window, "DigitalPsk31PowerOffer");

                _output.WriteLine("WINDOW " + Px(width) + " x " + Px(WindowHeight) + ", licensed, " + mode);
                _output.WriteLine("  rig display : " + Box(RectIn(rig, window)));
                _output.WriteLine("  rig panel   : " + Box(RectIn(panel, window)));
                _output.WriteLine("  card        : " + Box(RectIn(card, window)));
                _output.WriteLine("  drive box   : " + Box(RectIn(drive, window)));
                _output.WriteLine("  power offer : " + Box(RectIn(offer, window)) + " visible " + offer.IsEffectivelyVisible);
                _output.WriteLine("  tabs        : " + Box(RectIn(tabs, window)));
                _output.WriteLine("  send area   : " + Box(RectIn(reserved, window)));
                _output.WriteLine("  working card: " + Box(RectIn(workspace, window)));

                foreach (var (name, control) in new (string, Control)[] { ("drive", drive), ("power offer", offer) })
                {
                    Assert.True(
                        control.GetVisualAncestors().Contains(panel),
                        "at " + Px(width) + " the " + name + " is not in the rig panel");
                    Assert.False(
                        control.GetVisualAncestors().Contains(reserved),
                        "at " + Px(width) + " the " + name + " is still in the send area");
                }

                Assert.True(
                    RectIn(drive, window).Top >= RectIn(rig, window).Bottom - 0.5,
                    "at " + Px(width) + " the drive is not under the rig display's S-meter");

                foreach (var name in new[] { "DigitalSendCqButton", "DigitalStopButton" })
                {
                    var button = Named<Button>(window, name);

                    Assert.True(
                        button.GetVisualAncestors().Contains(reserved),
                        "at " + Px(width) + " " + name + " has left the send area");
                    Assert.True(button.IsEffectivelyVisible, "at " + Px(width) + " " + name + " is not visible");
                    Assert.Empty(button.GetVisualAncestors().OfType<CollapsiblePanel>());
                }

                // **RULING 5: THE SEND AREA RIDES THE TAB ROW, RIGHT OF THE TABS, ABOVE THE WORKING CARD.**
                Assert.True(
                    ReferenceEquals(reserved.GetVisualParent(), tabs.GetVisualParent()),
                    "at " + Px(width) + " the send area is not in the tab row beside ModeTabs");
                Assert.True(
                    RectIn(reserved, window).Left >= RectIn(tabs, window).Right - 0.5,
                    "at " + Px(width) + " the send area is not right of the tabs");
                Assert.True(
                    RectIn(reserved, window).Bottom <= RectIn(workspace, window).Top + 0.5,
                    "at " + Px(width) + " the send area is not above the working card");

                Assert.True(
                    Math.Abs(RectIn(panel, window).Height - RectIn(card, window).Height) <= 1,
                    "at " + Px(width) + " on " + mode + " the rig panel is " + Px(RectIn(panel, window).Height) + " px and the card is "
                    + Px(RectIn(card, window).Height) + " px, so they are not one height");

                if (mode != "PSK31")
                {
                    continue;
                }

                // **THE OFFER ITSELF, DRAWN** (ruling 6): its border, not only the sentence inside it.
                var border = OfferBorder(window);
                var borderAt = RectIn(border, window);
                var at = "at " + Px(width) + " on PSK31";

                _output.WriteLine("  offer border: " + Box(borderAt) + " visible " + border.IsEffectivelyVisible);

                Assert.True(
                    border.IsEffectivelyVisible && borderAt.Width > 0 && borderAt.Height > 0,
                    at + " the power offer is not drawn: " + Box(borderAt) + " visible " + border.IsEffectivelyVisible);
                Assert.True(border.GetVisualAncestors().Contains(panel), at + " the power offer is not in the rig panel");
                Assert.False(border.GetVisualAncestors().Contains(reserved), at + " the power offer is in the send area");
                Assert.True(
                    borderAt.Top >= RectIn(rig, window).Bottom - 0.5,
                    at + " the power offer starts at y=" + Px(borderAt.Top) + " and the rig display ends at y="
                    + Px(RectIn(rig, window).Bottom) + ", so it is not under the S-meter");
            }
            finally
            {
                model.ChosenDigitalMode = "FT8";
                window.Close();
            }
        }
    }

    /// <summary>
    /// **Work instruction 338 task 2: at 1400 the licensed operator's top row is back to the
    /// mockup's proportion, the rig panel is the card's height at both widths, the rule of thumb
    /// is the mockup's sentence, and the count is on the card at both widths.**
    /// </summary>
    /// <remarks>
    /// <para>**THE MOCKUP'S TOP ROW IS 186 OF THE 710 PX BELOW ITS PILLS, 0.262.** Unit 337
    /// measured 273 of 910 here, 0.300, on this fixture: a General license and a heard count, so
    /// the green block carries every line it can.</para>
    /// <para>**AND THE SAME SHAPE AT 1400** (R26, the arbiter's ruling 1): the three panels
    /// themselves take at least half the height below the pills, with the readiness strip hidden.
    /// This is the licensed half of <see cref="TheWorkingPanelsTests"/>' assertion; it lives here
    /// because the top row is what makes it reachable.</para>
    /// </remarks>
    [AvaloniaFact]
    public void AtFourteenHundredTheLicensedTopRowIsTheMockupsShare()
    {
        // **WORK INSTRUCTION 340 TASK 1: ON PSK31 AS WELL**, where the power offer is drawn in the
        // rig panel (ruling 6), with the same 0.262 and one half. FT8 is put back before each
        // window closes.
        foreach (var (width, mode) in new[] { (1920.0, "FT8"), (1400.0, "FT8"), (1920.0, "PSK31"), (1400.0, "PSK31") })
        {
            var window = Realized(width);
            var model = (MainWindowViewModel)window.DataContext!;

            try
            {
                model.ChosenDigitalMode = mode;
                Settle(window);

                _output.WriteLine("MODE " + mode);

                var m = Measure(window);
                var pills = window.GetVisualDescendants().OfType<ItemsControl>()
                    .First(i => i.GetVisualDescendants().OfType<Button>().Any(b => b.Classes.Contains("hm-band")));
                var below = window.Bounds.Height - RectIn(pills, window).Bottom;
                var rule = Named<TextBlock>(window, "GreenZoneRuleOfThumb");
                var heard = Named<TextBlock>(window, "GreenZoneHeard");
                var sparkline = Named<SparklineControl>(window, "GreenZoneSparkline");

                // **THE SHARE WITH THE READINESS STRIP SHOWING, PRINTED** (work instruction 339 task
                // 0): ruling 1 measures with the strip hidden and reports it showing as well.
                var shownPanels = TheWorkingPanelsTests.Panels(window)[0].Rect.Height;
                var stripShown = Named<Border>(window, "DigitalReadinessStrip").IsEffectivelyVisible;

                Named<Border>(window, "DigitalReadinessStrip").IsVisible = false;

                for (var i = 0; i < 4; i++)
                {
                    Avalonia.Threading.Dispatcher.UIThread.RunJobs();
                    window.UpdateLayout();
                }

                var panels = TheWorkingPanelsTests.Panels(window);

                _output.WriteLine("WINDOW " + Px(width) + " x " + Px(WindowHeight) + ", licensed");
                _output.WriteLine(
                    "  top row " + Px(m.TopRowHeight) + " px of " + Px(below) + " = "
                    + (m.TopRowHeight / below).ToString("0.000", CultureInfo.InvariantCulture) + " (mockup 0.262)");
                _output.WriteLine("  card " + Box(m.Card) + "; rig panel " + Box(m.Rig));
                _output.WriteLine("  green block " + Box(RectIn(Block(window), window)));
                _output.WriteLine(
                    "  panels " + Px(panels[0].Rect.Height) + " px = "
                    + (panels[0].Rect.Height / below).ToString("0.000", CultureInfo.InvariantCulture)
                    + " with the readiness strip hidden");
                _output.WriteLine(
                    "  panels " + Px(shownPanels) + " px = "
                    + (shownPanels / below).ToString("0.000", CultureInfo.InvariantCulture)
                    + " with the readiness strip " + (stripShown ? "showing" : "not showing (nothing to say)"));
                _output.WriteLine("  rule [" + rule.Text + "] " + Box(RectIn(rule, window)));
                _output.WriteLine(
                    "  count [" + heard.Text + "] visible " + heard.IsEffectivelyVisible + "; sparkline visible "
                    + sparkline.IsEffectivelyVisible);

                Assert.True(
                    Math.Abs(m.Rig.Height - m.Card.Height) <= 2,
                    "at " + Px(width) + " the rig panel is " + Px(m.Rig.Height) + " px and the card " + Px(m.Card.Height));

                Assert.Equal("20 m and up want daylight along the path; 40 m and down want dark.", rule.Text);

                Assert.True(
                    heard.IsEffectivelyVisible && (heard.Text ?? "").Length > 0,
                    "at " + Px(width) + " the heard count is not on the card");

                if (width > 1900)
                {
                    continue;
                }

                Assert.True(
                    m.TopRowHeight <= 0.262 * below,
                    "at 1400 on " + mode + " the licensed top row is " + Px(m.TopRowHeight) + " px of " + Px(below) + " = "
                    + (m.TopRowHeight / below).ToString("0.000", CultureInfo.InvariantCulture) + ", above the mockup's 0.262");

                Assert.True(
                    panels[0].Rect.Height >= below / 2,
                    "at 1400 on " + mode + " the licensed operator's three panels are " + Px(panels[0].Rect.Height) + " px of "
                    + Px(below) + ", less than half");
            }
            finally
            {
                model.ChosenDigitalMode = "FT8";
                window.Close();
            }
        }
    }

    /// <summary>
    /// **Work instruction 338 task 3, step 0's nice-to-pass: on the realized window at 1920 the
    /// band pill wearing *best bet now* and the green block name the same band, and the check is
    /// drawn where that band is the one he is on.**
    /// </summary>
    /// <remarks>
    /// <para>**THE JOIN IS BY CONSTRUCTION** - `GreenZone.BestBet` reads the band whose button
    /// carries the badge - and until now it was asserted on the view model only. Task 0 found no
    /// band ranked on the test host, so nothing on the window had ever shown it. The badge is set
    /// here the way the test for the view model sets it, and the green zone is re-announced the way
    /// the ranking does.</para>
    /// <para>**BOTH SIDES OF THE CHECK** (§0.6: the check is the non-color carrier): the best bet on
    /// the band he is on wears it, and a best bet elsewhere does not.</para>
    /// </remarks>
    [AvaloniaFact]
    public void TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow()
    {
        var window = Realized(1920);

        try
        {
            var model = (MainWindowViewModel)window.DataContext!;
            var here = Named<TextBlock>(window, "GreenZoneBand").Text;

            foreach (var bestName in new[] { "20 m", "40 m" })
            {
                foreach (var band in model.Bands)
                {
                    band.IsBestBet = band.Band.Name == bestName;
                }

                model.NotifyGreenZoneForTests();

                for (var i = 0; i < 4; i++)
                {
                    Avalonia.Threading.Dispatcher.UIThread.RunJobs();
                    window.UpdateLayout();
                }

                var badged = window.GetVisualDescendants().OfType<TextBlock>()
                    .Where(t => t.IsEffectivelyVisible && t.Text == "best bet now")
                    .Select(t => (t.DataContext as BandButtonViewModel)?.Band.Name)
                    .ToList();
                var bet = Named<Button>(window, "GreenZoneBestBet");
                var said = bet.Content as string ?? "";
                var checkDrawn = said.Contains(GreenZone.OnIt.Trim(), StringComparison.Ordinal);

                _output.WriteLine(
                    "best bet " + bestName + ": pills wearing the badge [" + string.Join(", ", badged)
                    + "]; green block band [" + here + "], best bet [" + said + "] visible "
                    + bet.IsEffectivelyVisible + ", check drawn " + checkDrawn);

                Assert.Single(badged);
                Assert.True(bet.IsEffectivelyVisible, "the green block's best bet is not drawn");
                Assert.StartsWith(badged[0] + "", said, StringComparison.Ordinal);
                Assert.Equal("20 m", here);
                Assert.Equal(badged[0] == here, checkDrawn);
            }
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// **Work instruction 340 task 0: the power offer drawn on PSK31, the mode that offers it,
    /// beside FT8, at 1920 and 1400.** Printed, not asserted - the trace task 1 is built from.
    /// </summary>
    /// <remarks>
    /// <para>**THE ARBITER'S RULING 6**: the offer is measured where it is offered.
    /// `HasPsk31PowerOffer` is `IsPsk31Chosen` and not yet answered, so choosing PSK31 draws it
    /// with no radio and no transmission.</para>
    /// <para>**NOTHING IS PRESSED** (§0.2, HM-DEC-084). The accept and decline buttons are read
    /// where they are drawn. FT8 is put back on the model before each window closes.</para>
    /// </remarks>
    [AvaloniaFact]
    public void Unit340TraceThePowerOfferOnPsk31()
    {
        foreach (var width in new[] { 1920.0, 1400.0 })
        {
            foreach (var mode in new[] { "PSK31", "FT8" })
            {
                var window = Realized(width);
                var model = (MainWindowViewModel)window.DataContext!;

                try
                {
                    model.ChosenDigitalMode = mode;
                    Settle(window);

                    var m = Measure(window);
                    var rig = RectIn(window.GetVisualDescendants().OfType<RigDisplayControl>().First(), window);
                    var drive = Named<NumericUpDown>(window, "DigitalTransmitDriveBox");
                    var sentence = Named<TextBlock>(window, "DigitalPsk31PowerOffer");
                    var accept = Named<Button>(window, "DigitalPsk31PowerAccept");
                    var decline = Named<Button>(window, "DigitalPsk31PowerDecline");
                    var alc = Named<TextBlock>(window, "DigitalPsk31AlcReference");
                    var offer = OfferBorder(window);
                    var offerAt = RectIn(offer, window);
                    var pills = window.GetVisualDescendants().OfType<ItemsControl>()
                        .First(i => i.GetVisualDescendants().OfType<Button>().Any(b => b.Classes.Contains("hm-band")));
                    var below = window.Bounds.Height - RectIn(pills, window).Bottom;
                    var strip = Named<Border>(window, "DigitalReadinessStrip");
                    var stripShown = strip.IsEffectivelyVisible;
                    var shownPanels = TheWorkingPanelsTests.Panels(window);

                    strip.IsVisible = false;
                    Settle(window);

                    var hiddenPanels = TheWorkingPanelsTests.Panels(window);

                    _output.WriteLine(
                        "=== " + mode + " " + Px(width) + " x " + Px(WindowHeight) + ", licensed; HasPsk31PowerOffer "
                        + model.HasPsk31PowerOffer + ", chosen " + model.ChosenDigitalMode);
                    _output.WriteLine("  rig display : " + Box(rig));
                    _output.WriteLine("  drive box   : " + Box(RectIn(drive, window)));
                    _output.WriteLine(
                        "  offer border: " + Box(offerAt) + " visible " + offer.IsEffectivelyVisible
                        + "; top - rig display bottom = " + Px(offerAt.Top - rig.Bottom));
                    _output.WriteLine(
                        "  sentence    : " + Box(RectIn(sentence, window)) + ", " + sentence.TextLayout.TextLines.Count
                        + " lines, " + (sentence.Text ?? "").Length + " chars");
                    _output.WriteLine("  accept      : " + Box(RectIn(accept, window)) + " [" + accept.Content + "]");
                    _output.WriteLine("  decline     : " + Box(RectIn(decline, window)) + " [" + decline.Content + "]");
                    _output.WriteLine(
                        "  ALC line    : " + Box(RectIn(alc, window)) + ", " + alc.TextLayout.TextLines.Count + " lines");
                    _output.WriteLine("  rig panel   : " + Box(m.Rig) + "; card " + Box(m.Card) + "; rig - card = " + Px(m.Rig.Height - m.Card.Height));
                    _output.WriteLine(
                        "  top row     : " + Px(m.TopRowHeight) + " px of " + Px(below) + " below the pills = "
                        + (m.TopRowHeight / below).ToString("0.000", CultureInfo.InvariantCulture));
                    _output.WriteLine(
                        "  panels      : " + Px(hiddenPanels[0].Rect.Height) + " px = "
                        + (hiddenPanels[0].Rect.Height / below).ToString("0.000", CultureInfo.InvariantCulture)
                        + " strip hidden; " + Px(shownPanels[0].Rect.Height) + " px = "
                        + (shownPanels[0].Rect.Height / below).ToString("0.000", CultureInfo.InvariantCulture)
                        + " strip " + (stripShown ? "showing" : "not showing (nothing to say)"));

                    foreach (var (name, rect) in hiddenPanels)
                    {
                        _output.WriteLine("    " + name.PadRight(10) + Box(rect));
                    }

                    if (mode == "PSK31")
                    {
                        // **THE OPTIONS BEYOND ARRANGEMENT, MEASURED FOR THE REPORT** (work instruction
                        // 340 task 1, ruling 7). Each is set on this test window only, never in the
                        // markup, and nothing is pressed. Choosing among them is Tim's, not the unit's.
                        foreach (var (label, apply) in new (string, Action)[]
                        {
                            ("sentence and ALC line off, the buttons kept", () =>
                            {
                                sentence.IsVisible = false;
                                alc.IsVisible = false;
                            }),
                            ("the mockup's words [RF power 50 % offered], ALC line off, the buttons kept", () =>
                            {
                                sentence.IsVisible = true;
                                sentence.Text = "RF power 50 % offered";
                            }),
                            ("the offer out of the top row", () => offer.IsVisible = false),
                        })
                        {
                            apply();
                            Settle(window);

                            var option = Measure(window);
                            var optionPanels = TheWorkingPanelsTests.Panels(window)[0].Rect.Height;

                            _output.WriteLine(
                                "  option, " + label + ": offer " + Box(RectIn(offer, window)) + "; top row "
                                + Px(option.TopRowHeight) + " = "
                                + (option.TopRowHeight / below).ToString("0.000", CultureInfo.InvariantCulture)
                                + "; panels " + Px(optionPanels) + " = "
                                + (optionPanels / below).ToString("0.000", CultureInfo.InvariantCulture));
                        }
                    }

                    _output.WriteLine("");
                }
                finally
                {
                    model.ChosenDigitalMode = "FT8";
                    window.Close();
                }
            }
        }
    }

    // ------------------------------------------------------------------------------------

    /// <summary>The power offer's border: the box `HasPsk31PowerOffer` shows, around the sentence.</summary>
    public static Border OfferBorder(Window window)
        => Named<TextBlock>(window, "DigitalPsk31PowerOffer").GetVisualAncestors().OfType<Border>().First();

    private static void Settle(Window window)
    {
        for (var i = 0; i < 4; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

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
