using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Telemetry;
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
    /// <para>**WORK INSTRUCTION 341 TASK 2: ON PSK31 AS WELL**, where the green block carries the
    /// strayed-frequency line and the offer line stands under the S-meter, with the assertions it
    /// already made. FT8 is put back before each window closes.</para>
    /// </remarks>
    [AvaloniaFact]
    public void TheGreenBlockIsInsideTheCardUnderTheStripWithTheBandLargest()
    {
        foreach (var (width, mode) in new[] { (1920.0, "FT8"), (1400.0, "FT8"), (1920.0, "PSK31"), (1400.0, "PSK31") })
        {
            var window = Realized(width);
            var model = (MainWindowViewModel)window.DataContext!;

            try
            {
                model.ChosenDigitalMode = mode;
                Settle(window);
                _output.WriteLine("MODE " + mode);

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
                model.ChosenDigitalMode = "FT8";
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
    /// <para>**WORK INSTRUCTION 341 TASK 2: ON PSK31 AS WELL**, with the assertions it already made.
    /// FT8 is put back before each window closes.</para>
    /// </remarks>
    [AvaloniaFact]
    public void TheWorldClockIsAtTheCardsRightEndWithOneMarker()
    {
        foreach (var (width, mode) in new[] { (1920.0, "FT8"), (1400.0, "FT8"), (1920.0, "PSK31"), (1400.0, "PSK31") })
        {
            var window = Realized(width);
            var model = (MainWindowViewModel)window.DataContext!;

            try
            {
                model.ChosenDigitalMode = mode;
                Settle(window);
                _output.WriteLine("MODE " + mode);

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
                model.ChosenDigitalMode = "FT8";
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
    /// <para>**WORK INSTRUCTION 341 TASK 1: THE OFFER AS THE MOCKUP'S ONE LINE, AND THE FULL OFFER IN
    /// ITS POPUP** (the arbiter's ruling 8, rewritten under R12). Under the S-meter on PSK31 is the
    /// line *RF power 50 % offered*; opening it by its own command writes no `psk31_power_*` event
    /// and leaves the offer unanswered; the popup then holds the sentence, both buttons and the
    /// ALC line with the view model's words, and no accept or decline is on the window outside it.
    /// **The offer's containment in the rig panel is now the line's and the popup's**, in both
    /// modes, because a closed popup's sentence has no visual parent to be contained by. Accept
    /// and decline are never executed.</para>
    /// </remarks>
    [AvaloniaFact]
    public void DriveAndThePowerOfferAreUnderTheRigAndTheSendAreaKeepsCqAndStop()
    {
        foreach (var (width, mode) in new[] { (1920.0, "FT8"), (1920.0, "PSK31"), (1400.0, "FT8"), (1400.0, "PSK31") })
        {
            var folder = Path.Combine(Path.GetTempPath(), "hamlet-unit341-offer-" + Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(folder);

            var telemetry = new JsonlTelemetry(folder, "341", _ => true);
            var window = Realized(width, telemetry);
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
                var line = Named<Button>(window, "DigitalPsk31PowerLine");
                var popup = Named<Popup>(window, "DigitalPsk31PowerPopup");

                _output.WriteLine("WINDOW " + Px(width) + " x " + Px(WindowHeight) + ", licensed, " + mode);
                _output.WriteLine("  rig display : " + Box(RectIn(rig, window)));
                _output.WriteLine("  rig panel   : " + Box(RectIn(panel, window)));
                _output.WriteLine("  card        : " + Box(RectIn(card, window)));
                _output.WriteLine("  drive box   : " + Box(RectIn(drive, window)));
                _output.WriteLine("  offer line  : " + Box(RectIn(line, window)) + " visible " + line.IsEffectivelyVisible);
                _output.WriteLine("  tabs        : " + Box(RectIn(tabs, window)));
                _output.WriteLine("  send area   : " + Box(RectIn(reserved, window)));
                _output.WriteLine("  working card: " + Box(RectIn(workspace, window)));

                foreach (var (name, control) in new (string, Control)[] { ("drive", drive), ("power offer line", line), ("power offer popup", popup) })
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

                // **THE LINE, DRAWN UNDER THE S-METER** (ruling 8): the mockup's words, with the
                // number read from the constant rather than typed, and every character on the glass.
                var lineAt = RectIn(line, window);
                var at = "at " + Px(width) + " on PSK31";
                var words = "RF power " + MainWindowViewModel.Psk31PowerPercent + " % offered";

                _output.WriteLine(
                    "  offer line wants " + Px(line.DesiredSize.Width) + " x " + Px(line.DesiredSize.Height) + ", says ["
                    + line.Content + "], top - rig display bottom = " + Px(lineAt.Top - RectIn(rig, window).Bottom));

                Assert.True(
                    line.IsEffectivelyVisible && lineAt.Width > 0 && lineAt.Height > 0,
                    at + " the offer line is not drawn: " + Box(lineAt) + " visible " + line.IsEffectivelyVisible);
                Assert.False(line.GetVisualAncestors().Contains(reserved), at + " the offer line is in the send area");
                Assert.True(
                    lineAt.Top >= RectIn(rig, window).Bottom - 0.5,
                    at + " the offer line starts at y=" + Px(lineAt.Top) + " and the rig display ends at y="
                    + Px(RectIn(rig, window).Bottom) + ", so it is not under the S-meter");
                // `DesiredSize` carries the margin and `Bounds` does not, so the margin comes off the
                // wanted width; and the words themselves are one line inside their own box.
                var wants = line.DesiredSize.Width - line.Margin.Left - line.Margin.Right;
                var face = line.GetVisualDescendants().OfType<TextBlock>().First(t => t.Text == line.Content as string);

                Assert.True(
                    wants <= lineAt.Width + 0.5 && lineAt.Right <= RectIn(panel, window).Right + 0.5,
                    at + " the offer line is clipped: it wants " + Px(wants) + " px and has " + Box(lineAt)
                    + " in a rig panel ending at x=" + Px(RectIn(panel, window).Right));
                Assert.True(
                    face.TextLayout.TextLines.Count == 1 && face.TextLayout.WidthIncludingTrailingWhitespace <= face.Bounds.Width + 0.5,
                    at + " the offer line's words take " + face.TextLayout.TextLines.Count + " lines and "
                    + Px(face.TextLayout.WidthIncludingTrailingWhitespace) + " px in a " + Px(face.Bounds.Width) + " px box");
                Assert.Equal(words, line.Content as string);
                Assert.Contains(MainWindowViewModel.Psk31PowerPercent.ToString(CultureInfo.InvariantCulture), line.Content as string ?? "", StringComparison.Ordinal);

                // **OPENING IT WRITES NOTHING** (§0.2, HM-DEC-084): closed at the start, opened by the
                // line's own command, and afterwards no event, the offer still unanswered and the
                // drive where it was.
                var driveBefore = model.TransmitDrivePercent;

                Assert.False(popup.IsOpen, at + " the full offer is open before anything was clicked");

                line.Command!.Execute(line.CommandParameter);
                Settle(window);

                Assert.True(popup.IsOpen, at + " the offer line's command did not open the full offer");
                Assert.True(model.HasPsk31PowerOffer, at + " opening the full offer answered it");
                Assert.Equal(driveBefore, model.TransmitDrivePercent);

                // **THE FULL OFFER, UNCHANGED, INSIDE THE POPUP** (ruling 8).
                var host = popup.Host as Visual;

                Assert.True(host is not null, at + " the open popup has no host to read");

                var inside = host!.GetVisualDescendants().ToHashSet();

                foreach (var (name, expected) in new (string, string)[]
                {
                    ("DigitalPsk31PowerOffer", model.Psk31PowerOffer),
                    ("DigitalPsk31PowerAccept", model.Psk31PowerAccept),
                    ("DigitalPsk31PowerDecline", "I will set it myself"),
                    ("DigitalPsk31AlcReference", model.Psk31AlcReferenceLine),
                })
                {
                    var control = inside.OfType<Control>().FirstOrDefault(c => c.Name == name);

                    Assert.True(control is not null, at + " " + name + " is not inside the open popup");

                    var said = control switch
                    {
                        TextBlock t => t.Text,
                        Button b => b.Content as string,
                        _ => null,
                    };

                    _output.WriteLine(
                        "  in popup " + name.PadRight(26) + Px(control!.Bounds.Width) + " x " + Px(control.Bounds.Height)
                        + " visible " + control.IsEffectivelyVisible + " [" + said + "]");

                    Assert.True(
                        control.IsEffectivelyVisible && control.Bounds.Width > 0 && control.Bounds.Height > 0,
                        at + " " + name + " is not drawn inside the popup: " + Px(control.Bounds.Width) + " x "
                        + Px(control.Bounds.Height) + " visible " + control.IsEffectivelyVisible);
                    Assert.Equal(expected, said);
                }

                // **ACCEPT AND DECLINE EXIST ONLY INSIDE THE POPUP**, so the whole sentence is on the
                // glass at every press that writes.
                var outside = window.GetVisualDescendants().OfType<Button>()
                    .Where(b => ReferenceEquals(b.Command, model.AcceptPsk31PowerCommand)
                        || ReferenceEquals(b.Command, model.DeclinePsk31PowerCommand))
                    .Where(b => !inside.Contains(b))
                    .Select(b => b.Name ?? "-")
                    .ToList();

                Assert.True(outside.Count == 0, at + " accept or decline is on the window outside the popup: " + string.Join(", ", outside));

                // **CLOSING WITHOUT AN ANSWER LEAVES IT OFFERED.**
                popup.IsOpen = false;
                Settle(window);

                Assert.True(model.HasPsk31PowerOffer, at + " closing the full offer answered it");
            }
            finally
            {
                if (window.GetVisualDescendants().OfType<Popup>().FirstOrDefault(p => p.Name == "DigitalPsk31PowerPopup") is { } open)
                {
                    open.IsOpen = false;
                }

                model.ChosenDigitalMode = "FT8";
                window.Close();
                telemetry.Dispose();
            }

            var recorded = Directory.GetFiles(folder, "*.jsonl")
                .SelectMany(File.ReadAllLines)
                .Where(l => l.Contains("\"psk31_power_", StringComparison.Ordinal))
                .ToList();

            try
            {
                Directory.Delete(folder, true);
            }
            catch (IOException)
            {
                // A left-over temp folder is not a test failure.
            }

            Assert.True(
                recorded.Count == 0,
                "at " + Px(width) + " on " + mode + " a power offer answer was recorded: " + string.Join(" | ", recorded));
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
    /// **Work instruction 350 task 1: the top row, the panel share and the rig panel's height
    /// hold with the best bet pinned drawn and absent, not at the hour of the run** - at 1920 and
    /// 1400, on FT8 and PSK31.
    /// </summary>
    /// <remarks>
    /// <para>**THE ARBITER'S RULING 49.** <see cref="AtFourteenHundredTheLicensedTopRowIsTheMockupsShare"/>
    /// never sets the best bet, and `RankBands` reads `DateTime.Now.Hour`, so which case it measured
    /// was the clock's. Here the badge is set on the test window only, on the band the fixture is on
    /// and on no band, and every case is measured. The limits are the tree's, unchanged: at 1920 the
    /// top row within 10% of 190; at 1400 the top row at most 0.262 of the height below the pills;
    /// at both widths the three panels at least half of it with the readiness strip hidden (ruling
    /// 1), and the rig panel within 2 px of the card.</para>
    /// <para>**THE PIN IS READ BACK AFTER THE LAST SETTLE.** A spot reload that lands during the
    /// settle re-ranks the bands; where it moved the badge, the case fails with that message and
    /// is not worked around (ruling 49). Every case is measured before anything is asserted, so a
    /// red names every miss at once. Nothing is pressed (§0.2).</para>
    /// </remarks>
    [AvaloniaFact]
    public void TheTopRowAndThePanelShareHoldWithTheBestBetPinnedBothWays()
    {
        var misses = new List<string>();

        foreach (var width in new[] { 1920.0, 1400.0 })
        {
            foreach (var mode in new[] { "FT8", "PSK31" })
            {
                foreach (var pinned in new[] { "20 m", null })
                {
                    var c = WithTheBestBetPinned(width, mode, pinned);
                    var at = "at " + Px(width) + " on " + mode + " with the best bet " + (pinned is null ? "absent" : "drawn on " + pinned);

                    _output.WriteLine(
                        at + ": top row " + Px(c.TopRow) + " of " + Px(c.Below) + " = " + Share(c.TopRow, c.Below)
                        + "; panels " + Px(c.PanelsHidden) + " = " + Share(c.PanelsHidden, c.Below) + " strip hidden; card "
                        + Px(c.Card.Height) + ", rig " + Px(c.Rig.Height) + "; green block " + Px(c.Block.Height)
                        + "; pin held " + c.PinHeld + ", best bet visible " + c.BestBetVisible + " [" + c.BestBetSaid + "]");

                    if (!c.PinHeld)
                    {
                        misses.Add(at + " the pin did not hold after settling: " + c.PinWhy);
                    }

                    if (Math.Abs(c.Rig.Height - c.Card.Height) > 2)
                    {
                        misses.Add(at + " the rig panel is " + Px(c.Rig.Height) + " px and the card " + Px(c.Card.Height));
                    }

                    if (c.PanelsHidden < c.Below / 2)
                    {
                        misses.Add(at + " the three panels are " + Px(c.PanelsHidden) + " px of " + Px(c.Below) + ", less than half");
                    }

                    if (width > 1900 && Math.Abs(c.TopRow - TopRowTarget) > TopRowTarget * 0.10)
                    {
                        misses.Add(at + " the top row is " + Px(c.TopRow) + " px against " + Px(TopRowTarget) + " within 10%");
                    }

                    if (width < 1900 && c.TopRow > 0.262 * c.Below)
                    {
                        misses.Add(
                            at + " the licensed top row is " + Px(c.TopRow) + " px of " + Px(c.Below) + " = "
                            + Share(c.TopRow, c.Below) + ", above the mockup's 0.262 (" + Px(0.262 * c.Below) + " px)");
                    }
                }
            }
        }

        Assert.True(misses.Count == 0, string.Join(Environment.NewLine, misses));
    }

    /// <summary>
    /// **Work instruction 351 ruling 56: the same limits with the best bet drawn on another band** -
    /// the case unit 350 printed and did not assert.
    /// </summary>
    /// <remarks>
    /// <para>**BESIDE THE PINNED FACT, NOT INSIDE IT**, so that fact goes green with its body unedited.
    /// The best bet is pinned on 40 m, so the green block says *40 m* without the check, at 1920 and
    /// 1400 on FT8 and PSK31, with the limits unchanged: the 1920 row within 10% of 190, the 1400 row
    /// at most 0.262 of below, the three panels at least half with the readiness strip hidden (ruling
    /// 1), the rig panel within 2 px of the card, and the pin read back after the last settle. A pin
    /// overwritten by a spot reload fails with that message and is not worked around. Nothing is
    /// pressed (§0.2).</para>
    /// </remarks>
    [AvaloniaFact]
    public void TheTopRowAndThePanelShareHoldWithTheBestBetOnAnotherBand()
    {
        const double limit = 0.262;
        var misses = new List<string>();

        foreach (var width in new[] { 1920.0, 1400.0 })
        {
            foreach (var mode in new[] { "FT8", "PSK31" })
            {
                var c = WithTheBestBetPinned(width, mode, "40 m");
                var at = "at " + Px(width) + " on " + mode + " with the best bet drawn on 40 m";

                _output.WriteLine(
                    at + ": top row " + Px(c.TopRow) + " of " + Px(c.Below) + " = " + Share(c.TopRow, c.Below)
                    + "; panels " + Px(c.PanelsHidden) + " = " + Share(c.PanelsHidden, c.Below) + " strip hidden; card "
                    + Px(c.Card.Height) + ", rig " + Px(c.Rig.Height) + "; green block " + Px(c.Block.Height)
                    + "; pin held " + c.PinHeld + ", best bet visible " + c.BestBetVisible + " [" + c.BestBetSaid + "]");

                if (!c.PinHeld)
                {
                    misses.Add(at + " the pin did not hold after settling: " + c.PinWhy);
                }

                if (Math.Abs(c.Rig.Height - c.Card.Height) > 2)
                {
                    misses.Add(at + " the rig panel is " + Px(c.Rig.Height) + " px and the card " + Px(c.Card.Height));
                }

                if (c.PanelsHidden < c.Below / 2)
                {
                    misses.Add(at + " the three panels are " + Px(c.PanelsHidden) + " px of " + Px(c.Below) + ", less than half");
                }

                if (width > 1900 && Math.Abs(c.TopRow - TopRowTarget) > TopRowTarget * 0.10)
                {
                    misses.Add(at + " the top row is " + Px(c.TopRow) + " px against " + Px(TopRowTarget) + " within 10%");
                }

                if (width < 1900 && c.TopRow > limit * c.Below)
                {
                    misses.Add(
                        at + " the licensed top row is " + Px(c.TopRow) + " px of " + Px(c.Below) + " = "
                        + Share(c.TopRow, c.Below) + ", above the mockup's " + limit.ToString("0.000", CultureInfo.InvariantCulture)
                        + " (" + Px(limit * c.Below) + " px)");
                }
            }
        }

        Assert.True(misses.Count == 0, string.Join(Environment.NewLine, misses));
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
    /// <para>**WORK INSTRUCTION 350 TASK 3: AT 1400 AS WELL** (the arbiter's ruling 51), in the same
    /// loop, with the assertions unchanged. Nothing is pressed (§0.2).</para>
    /// </remarks>
    [AvaloniaFact]
    public void TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow()
    {
        foreach (var width in new[] { 1920.0, 1400.0 })
        {
            var window = Realized(width);

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
                        "at " + Px(width) + " best bet " + bestName + ": pills wearing the badge [" + string.Join(", ", badged)
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
                    var sentence = InOffer<TextBlock>(window, "DigitalPsk31PowerOffer");
                    var accept = InOffer<Button>(window, "DigitalPsk31PowerAccept");
                    var decline = InOffer<Button>(window, "DigitalPsk31PowerDecline");
                    var alc = InOffer<TextBlock>(window, "DigitalPsk31AlcReference");
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

    /// <summary>
    /// **Work instruction 341 task 0: where the one-line offer fits under the S-meter, and which
    /// green block line makes PSK31 taller than FT8** - at 1920 and 1400, on PSK31 and FT8.
    /// Printed, not asserted - the trace task 1 is built from.
    /// </summary>
    /// <remarks>
    /// <para>**THE ARBITER'S RULING 8**: under the S-meter the offer becomes the mockup's line,
    /// *RF power 50 % offered*. Where it goes is measured by setting the line on this test window
    /// only - on the drive row, on the drive note's row, and on its own line - with the offer's
    /// border hidden, as the popup takes it off the row. Nothing goes into the markup.</para>
    /// <para>**RULING 9**: the green block is printed line by line in both modes, so the PSK31
    /// height has a name.</para>
    /// <para>**NOTHING IS PRESSED** (§0.2, HM-DEC-084). FT8 is put back before each window
    /// closes.</para>
    /// </remarks>
    [AvaloniaFact]
    public void Unit341TraceTheOneLineOfferAndThePsk31GreenBlock()
    {
        var words = "RF power " + MainWindowViewModel.Psk31PowerPercent + " % offered";

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
                    var column = Named<StackPanel>(window, "RigDriveAndPower");
                    var driveRow = (Grid)column.Children[0];
                    var noteRow = (StackPanel)column.Children[1];
                    var panel = RigPanel(window);
                    var offer = OfferBorder(window);
                    var block = Block(window);
                    var pills = window.GetVisualDescendants().OfType<ItemsControl>()
                        .First(i => i.GetVisualDescendants().OfType<Button>().Any(b => b.Classes.Contains("hm-band")));
                    var below = window.Bounds.Height - RectIn(pills, window).Bottom;
                    var strip = Named<Border>(window, "DigitalReadinessStrip");
                    var stripShown = strip.IsEffectivelyVisible;
                    var shownPanels = TheWorkingPanelsTests.Panels(window)[0].Rect.Height;

                    double InnerBottom() => RectIn(panel, window).Bottom - panel.BorderThickness.Bottom - panel.Padding.Bottom;

                    void Row(string label, Panel row)
                    {
                        var columnAt = RectIn(column, window);
                        var parts = row.Children.Where(c => c.IsVisible).ToList();
                        var right = parts.Count == 0 ? columnAt.Left : parts.Max(c => RectIn(c, window).Right);

                        _output.WriteLine(
                            "  " + label.PadRight(12) + ": " + Box(RectIn(row, window)) + "; right edge " + Px(right)
                            + ", column right " + Px(columnAt.Right) + ", left over " + Px(columnAt.Right - right));

                        foreach (var part in parts)
                        {
                            var said = part switch
                            {
                                TextBlock t => "[" + t.Text + "] " + t.TextLayout.TextLines.Count + " lines",
                                _ => part.GetType().Name,
                            };

                            _output.WriteLine("      " + (part.Name ?? "-").PadRight(26) + Box(RectIn(part, window)) + " " + said);
                        }
                    }

                    _output.WriteLine(
                        "=== " + mode + " " + Px(width) + " x " + Px(WindowHeight) + ", licensed; HasPsk31PowerOffer "
                        + model.HasPsk31PowerOffer);
                    _output.WriteLine("  rig display : " + Box(rig));
                    _output.WriteLine("  rig column  : " + Box(RectIn(column, window)));
                    Row("drive row", driveRow);
                    Row("note row", noteRow);
                    _output.WriteLine("  offer border: " + Box(RectIn(offer, window)) + " visible " + offer.IsEffectivelyVisible);
                    _output.WriteLine(
                        "  rig panel " + Box(m.Rig) + ", inner bottom " + Px(InnerBottom()) + "; column bottom "
                        + Px(RectIn(column, window).Bottom) + ", spare under it " + Px(InnerBottom() - RectIn(column, window).Bottom)
                        + "; card " + Box(m.Card));

                    // **THE GREEN BLOCK, LINE BY LINE** (ruling 9).
                    _output.WriteLine("  green block : " + Box(RectIn(block, window)));

                    foreach (var part in ((Panel)block.Child!).Children.Where(c => c.IsVisible))
                    {
                        _output.WriteLine("    row " + (part.Name ?? part.GetType().Name).PadRight(22) + Box(RectIn(part, window)));
                    }

                    foreach (var text in block.GetVisualDescendants().OfType<TextBlock>().Where(t => t.IsEffectivelyVisible))
                    {
                        var owner = text.Name
                            ?? text.GetVisualAncestors().OfType<Control>().FirstOrDefault(c => !string.IsNullOrEmpty(c.Name))?.Name
                            ?? "-";

                        _output.WriteLine(
                            "      " + owner.PadRight(24) + Box(RectIn(text, window)) + ", " + text.TextLayout.TextLines.Count
                            + " lines [" + text.Text + "]");
                    }

                    _output.WriteLine(
                        "      sparkline visible " + Named<SparklineControl>(window, "GreenZoneSparkline").IsEffectivelyVisible
                        + "; best bet visible " + Named<Button>(window, "GreenZoneBestBet").IsEffectivelyVisible);

                    // **WHAT EACH CANDIDATE COSTS THE BLOCK, SET ON THIS WINDOW ONLY** (ruling 9). A
                    // row that is visible with nothing in it still takes the stack's spacing.
                    var rows = ((Panel)block.Child!).Children;
                    var strayed = Named<TextBlock>(window, "GreenZoneStrayedLine");
                    var emptyRows = rows.Where(c => c.IsVisible && c.Bounds.Height == 0).ToList();

                    foreach (var (label, hide, show) in new (string, Action, Action)[]
                    {
                        ("strayed line hidden", () => strayed.IsVisible = false, () => strayed.ClearValue(Visual.IsVisibleProperty)),
                        ("empty rows hidden (" + emptyRows.Count + ")", () => emptyRows.ForEach(c => c.IsVisible = false), () => emptyRows.ForEach(c => c.IsVisible = true)),
                    })
                    {
                        var before = RectIn(block, window).Height;

                        hide();
                        Settle(window);
                        _output.WriteLine(
                            "      option " + label + ": block " + Px(before) + " -> " + Px(RectIn(block, window).Height)
                            + ", card " + Px(RectIn(Card(window), window).Height));
                        show();
                        Settle(window);
                    }

                    strip.IsVisible = false;
                    Settle(window);

                    var hiddenPanels = TheWorkingPanelsTests.Panels(window)[0].Rect.Height;

                    _output.WriteLine(
                        "  top row     : " + Px(m.TopRowHeight) + " px of " + Px(below) + " = "
                        + (m.TopRowHeight / below).ToString("0.000", CultureInfo.InvariantCulture));
                    _output.WriteLine(
                        "  panels      : " + Px(hiddenPanels) + " px = "
                        + (hiddenPanels / below).ToString("0.000", CultureInfo.InvariantCulture) + " strip hidden; "
                        + Px(shownPanels) + " px = " + (shownPanels / below).ToString("0.000", CultureInfo.InvariantCulture)
                        + " strip " + (stripShown ? "showing" : "not showing (nothing to say)"));

                    // **THE BUILT LINE'S INK AGAINST THE RIG PANEL'S FILL** (§0.6), once it exists.
                    if (window.GetVisualDescendants().OfType<Button>().FirstOrDefault(b => b.Name == "DigitalPsk31PowerLine") is { } built
                        && built.Foreground is Avalonia.Media.ISolidColorBrush ink
                        && panel.Background is Avalonia.Media.ISolidColorBrush fill)
                    {
                        static double Lum(Avalonia.Media.Color c)
                        {
                            static double Channel(byte v)
                            {
                                var s = v / 255.0;

                                return s <= 0.03928 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
                            }

                            return (0.2126 * Channel(c.R)) + (0.7152 * Channel(c.G)) + (0.0722 * Channel(c.B));
                        }

                        var (hi, lo) = (Math.Max(Lum(ink.Color), Lum(fill.Color)), Math.Min(Lum(ink.Color), Lum(fill.Color)));

                        _output.WriteLine(
                            "  built line: " + Box(RectIn(built, window)) + " visible " + built.IsEffectivelyVisible + ", ink "
                            + ink.Color + " on fill " + fill.Color + " = "
                            + ((hi + 0.05) / (lo + 0.05)).ToString("0.00", CultureInfo.InvariantCulture) + ":1");
                    }

                    // **THE LINE, SET ON THIS WINDOW ONLY** (ruling 8). The detached width is the words
                    // alone at the drive's 11 px; the placements use a card link, which is how a
                    // pressable line is drawn elsewhere on this window.
                    var plain = new TextBlock { Text = words, FontSize = 11 };

                    plain.Measure(Size.Infinity);
                    _output.WriteLine("  the line [" + words + "] needs " + Px(plain.DesiredSize.Width) + " px as bare 11 px text");

                    offer.IsVisible = false;
                    Settle(window);

                    var bare = Measure(window);

                    _output.WriteLine(
                        "  offer border hidden: top row " + Px(bare.TopRowHeight) + " = "
                        + (bare.TopRowHeight / below).ToString("0.000", CultureInfo.InvariantCulture) + "; panels "
                        + Px(TheWorkingPanelsTests.Panels(window)[0].Rect.Height) + "; spare under the column "
                        + Px(InnerBottom() - RectIn(column, window).Bottom));

                    foreach (var (label, place, unplace) in new (string, Action<Button>, Action<Button>)[]
                    {
                        ("on the drive row", line =>
                        {
                            driveRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                            Grid.SetColumn(line, 3);
                            driveRow.Children.Add(line);
                        }, line =>
                        {
                            driveRow.Children.Remove(line);
                            driveRow.ColumnDefinitions.RemoveAt(3);
                        }),
                        ("on the note row", line => noteRow.Children.Add(line), line => noteRow.Children.Remove(line)),
                        ("on its own line", line => column.Children.Insert(2, line), line => column.Children.Remove(line)),
                    })
                    {
                        var line = new Button
                        {
                            Content = words,
                            Classes = { "hm-cardlink" },
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        };

                        place(line);
                        Settle(window);

                        var at = RectIn(line, window);
                        var option = Measure(window);
                        var optionPanels = TheWorkingPanelsTests.Panels(window)[0].Rect.Height;

                        _output.WriteLine(
                            "  line " + label.PadRight(17) + ": " + Box(at) + ", wants " + Px(line.DesiredSize.Width)
                            + " x " + Px(line.DesiredSize.Height) + ", column right " + Px(RectIn(column, window).Right)
                            + ", past it " + Px(at.Right - RectIn(column, window).Right) + "; top below the rig display "
                            + Px(at.Top - rig.Bottom) + "; top row " + Px(option.TopRowHeight) + " = "
                            + (option.TopRowHeight / below).ToString("0.000", CultureInfo.InvariantCulture) + "; panels "
                            + Px(optionPanels) + " = " + (optionPanels / below).ToString("0.000", CultureInfo.InvariantCulture)
                            + "; spare under the column " + Px(InnerBottom() - RectIn(column, window).Bottom));

                        unplace(line);
                        Settle(window);
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

    /// <summary>
    /// **Work instruction 350 task 0: step 0 with the best bet pinned both ways, and what stands
    /// between the three panels and the status bar** - at 1920 and 1400, on FT8 and PSK31.
    /// Printed, not asserted - the trace task 1 is built from.
    /// </summary>
    /// <remarks>
    /// <para>**THE ARBITER'S RULING 49**: `RankBands` reads `DateTime.Now.Hour`, so the green
    /// block's best bet, and with it the 1400 top row, followed the hour the test ran. The badge is
    /// set here on the test window only, the way
    /// <see cref="TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow"/> sets it - drawn on
    /// the band the fixture is on, and absent - and the pin is read back after settling. A best bet
    /// on another band is printed as well, for the report only.</para>
    /// <para>**RULING 50**: from the panels' bottom edge to the status bar's top edge, every
    /// visual with height that overlaps the gap, with its margin, border and padding.</para>
    /// <para>**NOTHING IS PRESSED** (§0.2). `GreenZoneBestBet` moves the operator's band; it is
    /// read and never clicked. FT8 is put back before each window closes.</para>
    /// </remarks>
    [AvaloniaFact]
    public void Unit350TraceStepZeroBothWays()
    {
        _output.WriteLine("local hour the run read: " + DateTime.Now.Hour.ToString("00", CultureInfo.InvariantCulture));

        foreach (var width in new[] { 1920.0, 1400.0 })
        {
            foreach (var mode in new[] { "FT8", "PSK31" })
            {
                foreach (var (label, pinned) in new (string, string?)[] { ("drawn", "20 m"), ("absent", null), ("elsewhere, report only", "40 m") })
                {
                    var c = WithTheBestBetPinned(width, mode, pinned);

                    _output.WriteLine(
                        "=== " + mode + " " + Px(width) + " x " + Px(WindowHeight) + ", licensed; best bet " + label
                        + " (" + (pinned ?? "no band") + "); before pinning the hour's best bet was visible " + c.VisibleBeforePin);
                    _output.WriteLine(
                        "  top row  : " + Px(c.TopRow) + " px of " + Px(c.Below) + " = " + Share(c.TopRow, c.Below)
                        + "; limit 0.262 x below = " + Px(0.262 * c.Below) + ", margin " + Px((0.262 * c.Below) - c.TopRow));
                    _output.WriteLine("  card " + Box(c.Card) + "; rig panel " + Box(c.Rig) + "; rig - card = " + Px(c.Rig.Height - c.Card.Height));
                    _output.WriteLine("  green block " + Box(c.Block) + " = " + Px(c.Block.Height) + " px");
                    _output.WriteLine(
                        "  panels   : " + Px(c.PanelsHidden) + " px = " + Share(c.PanelsHidden, c.Below) + " strip hidden; "
                        + Px(c.PanelsShown) + " px = " + Share(c.PanelsShown, c.Below) + " strip "
                        + (c.StripShown ? "showing" : "not showing (nothing to say)") + "; half = " + Px(c.Below / 2));
                    _output.WriteLine(
                        "  pin held " + c.PinHeld + ": badged [" + string.Join(", ", c.Badged) + "], best bet visible "
                        + c.BestBetVisible + " [" + c.BestBetSaid + "]" + (c.PinHeld ? "" : "; " + c.PinWhy));
                }
            }
        }

        // **THE FLOOR** (ruling 50): the panels' bottom, the status bar's top, and every visual with
        // height that overlaps what is between them.
        foreach (var width in new[] { 1920.0, 1400.0 })
        {
            var window = Realized(width);
            var model = (MainWindowViewModel)window.DataContext!;

            try
            {
                Settle(window);

                var panels = TheWorkingPanelsTests.Panels(window);
                var waterfall = Named<Control>(window, "DigitalWaterfallPanel");
                var status = Named<Control>(window, "StatusBar");
                var floor = panels.Max(p => p.Rect.Bottom);
                var top = RectIn(status, window).Top;
                var holding = waterfall.GetVisualAncestors().ToHashSet();
                var statusHolding = status.GetVisualAncestors().ToHashSet();

                _output.WriteLine(
                    "=== FLOOR FT8 " + Px(width) + " x " + Px(WindowHeight) + ": panels' bottoms ["
                    + string.Join(", ", panels.Select(p => p.Name + " " + Px(p.Rect.Bottom))) + "]; status bar " + Box(RectIn(status, window))
                    + "; gap y " + Px(floor) + " to " + Px(top) + " = " + Px(top - floor) + " px");

                foreach (var v in window.GetVisualDescendants().OfType<Control>().Prepend(window))
                {
                    var r = RectIn(v, window);

                    if (!v.IsEffectivelyVisible || r.Height <= 0 || r.Bottom <= floor + 0.5 || r.Top >= top - 0.5)
                    {
                        continue;
                    }

                    var role = holding.Contains(v) && statusHolding.Contains(v) ? "holds the panels and the status bar"
                        : holding.Contains(v) ? "holds the panels; " + Px(r.Bottom - floor) + " px of it below their floor"
                        : r.Top >= floor - 0.5 && r.Bottom <= top + 0.5 ? "STANDS IN THE GAP"
                        : "crosses the gap";

                    _output.WriteLine(
                        "  " + (v.GetType().Name + " " + (v.Name ?? "-")).PadRight(44) + "top " + Px(r.Top) + ", height " + Px(r.Height)
                        + ", bottom " + Px(r.Bottom) + "; " + Spacing(v) + "; " + role);
                }

                // **WHAT EACH PIXEL IS**: the chain from the waterfall panel up to the working card's
                // border, bottom edges and what each adds under its child.
                _output.WriteLine("  chain from the waterfall panel to the window:");

                foreach (var v in waterfall.GetVisualAncestors().OfType<Control>().Prepend(waterfall))
                {
                    var r = RectIn(v, window);

                    _output.WriteLine("    " + (v.GetType().Name + " " + (v.Name ?? "-")).PadRight(44) + "bottom " + Px(r.Bottom) + "; " + Spacing(v));

                    if (ReferenceEquals(v, Named<Control>(window, "WorkspaceBoundary")))
                    {
                        break;
                    }
                }
            }
            finally
            {
                model.ChosenDigitalMode = "FT8";
                window.Close();
            }
        }
    }

    /// <summary>
    /// **Work instruction 351 task 0: what the check costs the green block, line by line, and what
    /// each arrangement gives** - at 1920 and 1400, on FT8 and PSK31, with the best bet on his band,
    /// on 40 m and absent. Printed, not asserted - the trace task 1 is built from.
    /// </summary>
    /// <remarks>
    /// <para>**THE ARBITER'S RULING 54**: the right column's width, the left column's width, each
    /// left line's height and line count, the best bet row's height and the word measured with and
    /// without *✓*, before anything is built. The best bet is pinned the way
    /// <see cref="Unit350TraceStepZeroBothWays"/> pins it, and read back after the last settle.</para>
    /// <para>**THE CANDIDATES ARE SET ON THE TEST WINDOW ONLY**, on 1400 PSK31 with the best bet on
    /// his band and on 1920 with the same pin, and every one keeps ruling 55's list: no word
    /// changes, nothing hides, and `GreenZoneBestBet` keeps its name, command, tooltip and
    /// visibility binding. Nothing goes into the markup.</para>
    /// <para>**NOTHING IS PRESSED** (§0.2). The best bet is read and never clicked. FT8 is put back
    /// before each window closes.</para>
    /// </remarks>
    [AvaloniaFact]
    public void Unit351TraceTheCheckInTheGreenBlock()
    {
        _output.WriteLine("local hour the run read: " + DateTime.Now.Hour.ToString("00", CultureInfo.InvariantCulture));

        foreach (var width in new[] { 1920.0, 1400.0 })
        {
            foreach (var mode in new[] { "FT8", "PSK31" })
            {
                foreach (var pinned in new[] { "20 m", "40 m", null })
                {
                    TraceTheGreenBlock(width, mode, pinned, "as built", null);
                }
            }
        }

        static Panel BestBetRow(Window w) => (Panel)Named<Button>(w, "GreenZoneBestBet").GetVisualParent()!;

        static void Detach(Control c) => ((Panel)c.GetVisualParent()!).Children.Remove(c);

        var candidates = new (string Name, Action<Window> Arrange)[]
        {
            ("the best bet row moved to the left column, under the rule of thumb", w =>
            {
                var row = BestBetRow(w);

                Detach(row);
                row.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                Named<StackPanel>(w, "GreenZoneLeft").Children.Add(row);
            }),
            ("the best bet row moved into the band line, after the verdict", w =>
            {
                var row = BestBetRow(w);

                Detach(row);
                row.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                ((WrapPanel)Named<StackPanel>(w, "GreenZoneLeft").Children[0]).Children.Add(row);
            }),
            ("the best bet row moved under the regions, right-aligned across the block", w =>
            {
                var row = BestBetRow(w);
                var stack = (StackPanel)Named<Border>(w, "GreenZoneBlock").Child!;

                Detach(row);
                stack.Children.Insert(stack.Children.IndexOf(Named<Grid>(w, "GreenZoneRegions")) + 1, row);
            }),
            ("the right column held to the heard grid's width, the best bet row wrapping", w =>
            {
                var row = BestBetRow(w);
                var right = Named<StackPanel>(w, "GreenZoneRight");
                var wrap = new WrapPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                };
                var parts = row.Children.ToList();

                row.Children.Clear();
                parts.ForEach(p => wrap.Children.Add(p));
                right.Children.Remove(row);
                right.Children.Insert(0, wrap);
                right.Width = Named<Grid>(w, "GreenZoneHeardGrid").Bounds.Width;
            }),
            ("the check's line held to the prefix's line height", w =>
            {
                var prefix = Named<TextBlock>(w, "GreenZoneBestBetPrefix");

                foreach (var inner in Named<Button>(w, "GreenZoneBestBet").GetVisualDescendants().OfType<TextBlock>())
                {
                    inner.LineHeight = prefix.TextLayout.TextLines[0].Height;
                }
            }),
        };

        foreach (var (name, arrange) in candidates)
        {
            foreach (var width in new[] { 1400.0, 1920.0 })
            {
                TraceTheGreenBlock(width, "PSK31", "20 m", name, arrange);
            }
        }

        // **TASK 3: U2'S OVERRULE NUMBER** - the upgrade row shown again on this window only, the
        // row whose one button toggles the upgrade ladder, as it stood before work instruction 341.
        // Its button keeps its own binding, and nothing is pressed.
        foreach (var pinned in new[] { "20 m", "40 m", null })
        {
            TraceTheGreenBlock(1400, "PSK31", pinned, "the upgrade row put back (U2's overrule)", w =>
            {
                var model = (MainWindowViewModel)w.DataContext!;
                var upgrade = Named<Border>(w, "GreenZoneBlock").GetVisualDescendants().OfType<Button>()
                    .First(b => ReferenceEquals(b.Command, model.ToggleUpgradeLadderCommand));

                ((Control)upgrade.GetVisualParent()!).IsVisible = true;
            });
        }
    }

    /// <summary>
    /// One pinned window for <see cref="Unit351TraceTheCheckInTheGreenBlock"/>: the top row, the
    /// panels and the green block line by line, with <paramref name="arrange"/> set on this window
    /// only where it is given.
    /// </summary>
    private void TraceTheGreenBlock(double width, string mode, string? pinned, string arrangement, Action<Window>? arrange)
    {
        var window = Realized(width);
        var model = (MainWindowViewModel)window.DataContext!;

        static string Lines(TextBlock t)
        {
            var lines = t.TextLayout.TextLines;
            var one = lines.Count == 0 ? 0 : lines[0].Height;

            return lines.Count + " lines of " + Px(one) + " px, height over line " + (one > 0 ? Px(t.Bounds.Height / one) : "-");
        }

        static (double Width, double Height) Words(string text, TemplatedControl face)
        {
            var f = new Avalonia.Media.FormattedText(
                text,
                CultureInfo.CurrentUICulture,
                Avalonia.Media.FlowDirection.LeftToRight,
                new Avalonia.Media.Typeface(face.FontFamily, face.FontStyle, face.FontWeight),
                face.FontSize,
                Avalonia.Media.Brushes.Black);

            return (f.Width, f.Height);
        }

        try
        {
            model.ChosenDigitalMode = mode;
            Settle(window);

            foreach (var band in model.Bands)
            {
                band.IsBestBet = band.Band.Name == pinned;
            }

            model.NotifyGreenZoneForTests();
            Settle(window);

            if (arrange is not null)
            {
                arrange(window);
                Settle(window);
            }

            var m = Measure(window);
            var pills = window.GetVisualDescendants().OfType<ItemsControl>()
                .First(i => i.GetVisualDescendants().OfType<Button>().Any(b => b.Classes.Contains("hm-band")));
            var below = window.Bounds.Height - RectIn(pills, window).Bottom;
            var block = Named<Border>(window, "GreenZoneBlock");
            var regions = Named<Grid>(window, "GreenZoneRegions");
            var left = Named<StackPanel>(window, "GreenZoneLeft");
            var right = Named<StackPanel>(window, "GreenZoneRight");
            var bet = Named<Button>(window, "GreenZoneBestBet");
            var prefix = Named<TextBlock>(window, "GreenZoneBestBetPrefix");
            var said = bet.Content as string ?? "";

            _output.WriteLine(
                "=== " + mode + " " + Px(width) + ", licensed; best bet " + (pinned ?? "absent") + "; " + arrangement);
            _output.WriteLine(
                "  top row " + Px(m.TopRowHeight) + " px of " + Px(below) + " = " + Share(m.TopRowHeight, below)
                + " (limit " + Px(0.262 * below) + "); card " + Px(m.Card.Height) + ", rig " + Px(m.Rig.Height));
            _output.WriteLine(
                "  green block " + Box(RectIn(block, window)) + "; regions " + Box(RectIn(regions, window))
                + "; left " + Box(RectIn(left, window)) + " margin right " + Px(left.Margin.Right)
                + "; right " + Box(RectIn(right, window)));

            foreach (var row in ((Panel)block.Child!).Children.Where(c => c.IsVisible))
            {
                _output.WriteLine("    block row " + (row.Name ?? row.GetType().Name).PadRight(24) + Box(RectIn(row, window)));
            }

            var bandLine = (Panel)left.Children[0];
            var shown = bandLine.Children.Where(c => c.IsVisible).ToList();
            var wraps = shown.Skip(1).Where((c, i) => c.Bounds.X <= shown[i].Bounds.X).Count();

            _output.WriteLine("    left band line " + Box(RectIn(bandLine, window)) + ", " + (shown.Count == 0 ? 0 : wraps + 1) + " rows");

            foreach (var name in new[] { "GreenZoneBand", "GreenZoneFrequency", "GreenZoneModeLine", "GreenZoneLicenseLine", "GreenZoneRuleOfThumb" })
            {
                var t = Named<TextBlock>(window, name);

                _output.WriteLine(
                    "      " + name.PadRight(24) + Box(RectIn(t, window)) + " visible " + t.IsEffectivelyVisible + "; "
                    + Lines(t) + " [" + t.Text + "]");
            }

            foreach (var part in left.Children.Skip(3).Where(c => c.IsVisible))
            {
                _output.WriteLine("      left, added: " + part.GetType().Name + " " + Box(RectIn(part, window)));
            }

            var betRow = (Control)bet.GetVisualParent()!;
            var inner = bet.GetVisualDescendants().OfType<TextBlock>().FirstOrDefault();
            var withCheck = Words(said, bet);
            var without = Words(said.Replace(GreenZone.OnIt, "", StringComparison.Ordinal), bet);
            var check = Words(GreenZone.OnIt.Trim(), bet);

            _output.WriteLine(
                "    best bet row " + (betRow.GetType().Name) + " " + Box(RectIn(betRow, window)) + " visible " + betRow.IsEffectivelyVisible
                + "; prefix " + Box(RectIn(prefix, window)) + ", " + Lines(prefix));
            _output.WriteLine(
                "      button [" + said + "] " + Box(RectIn(bet, window)) + ", desired " + Px(bet.DesiredSize.Width) + " x "
                + Px(bet.DesiredSize.Height) + ", visible " + bet.IsEffectivelyVisible
                + (inner is null ? "" : "; inner text " + Box(RectIn(inner, window)) + ", " + Lines(inner)));
            _output.WriteLine(
                "      the word measured: with the check " + Px(withCheck.Width) + " x " + Px(withCheck.Height)
                + ", without " + Px(without.Width) + " x " + Px(without.Height) + ", the check alone "
                + Px(check.Width) + " x " + Px(check.Height));

            if (inner is not null)
            {
                foreach (var run in inner.TextLayout.TextLines.SelectMany(l => l.TextRuns))
                {
                    _output.WriteLine(
                        "      run " + run.GetType().Name + " length " + run.Length
                        + (run is Avalonia.Media.TextFormatting.DrawableTextRun d ? ", size " + Px(d.Size.Width) + " x " + Px(d.Size.Height) : "")
                        + (run is Avalonia.Media.TextFormatting.ShapedTextRun s ? ", face " + s.GlyphRun.GlyphTypeface.FamilyName : ""));
                }
            }

            _output.WriteLine(
                "    heard grid " + Box(RectIn(Named<Grid>(window, "GreenZoneHeardGrid"), window)) + "; sparkline visible "
                + Named<SparklineControl>(window, "GreenZoneSparkline").IsEffectivelyVisible);

            var strip = Named<Border>(window, "DigitalReadinessStrip");

            strip.IsVisible = false;
            Settle(window);

            var hidden = TheWorkingPanelsTests.Panels(window)[0].Rect.Height;
            var badged = model.Bands.Where(b => b.IsBestBet).Select(b => b.Band.Name).ToList();
            var held = badged.SequenceEqual(pinned is null ? Array.Empty<string>() : new[] { pinned })
                && bet.IsEffectivelyVisible == (pinned is not null);

            _output.WriteLine(
                "  panels " + Px(hidden) + " px = " + Share(hidden, below) + " strip hidden (half " + Px(below / 2)
                + "); after hiding the strip the top row is " + Px(Measure(window).TopRowHeight) + "; pin held " + held
                + " [" + string.Join(", ", badged) + "] best bet [" + (bet.Content as string) + "]");
        }
        finally
        {
            model.ChosenDigitalMode = "FT8";
            window.Close();
        }
    }

    /// <summary>
    /// **Work instruction 352 task 0: what lands on the pinned best bet** - the 1920 PSK31 cases
    /// unit 351's pin reds fell on, pinned on 20 m and absent, five times each.
    /// </summary>
    /// <remarks>
    /// <para>**THE ARBITER'S RULING 61. A TRACE, AND IT ASSERTS NOTHING.** Each window is built with
    /// `Realized(width, telemetry)` and taken through the pinned fact's own steps: PSK31, settle, the
    /// pin, settle, the readiness strip hidden, settle, the pin read back. It prints, in time order,
    /// every spot reload the telemetry file recorded with its trigger and spot count, every source the
    /// model reported unhealthy, every move of `IsBestBet` and of the sources summary, each marked
    /// before or after the pin; then the spots on the list by source, where the best bet ended and
    /// whether the pin held.</para>
    /// <para>It reads public members and the telemetry file only. **Nothing is pressed** (§0.2).</para>
    /// </remarks>
    [AvaloniaFact]
    public void Unit352TraceTheSpotReloadOnTheTestWindow()
    {
        // **TASK 1: THE SAME TEN WINDOWS WITH THE CHANGE IN PLACE** - the network sources switched off
        // in `FixtureSettings`. The ten as built are task 0's, at `4ad20a25`: the pin held 7 of 10, each
        // miss the `band_changed` reload waiting on POTA. They are not printed again, because building
        // them again would reach POTA and RBN under the fixture's callsign on every run of this class.
        const string arrangement = "the network sources switched off (work instruction 352)";
        var settings = FixtureSettings();

        _output.WriteLine("=== the sources BuildSources makes for this window (MainWindowViewModel.cs:7602-7638); " + arrangement);

        foreach (var name in NetworkSources.Append(Hamlet.RadioEngine.Explore.FakeActivitySource.SourceName))
        {
            _output.WriteLine(
                "  " + name.PadRight(7) + (name == Hamlet.RadioEngine.Explore.RbnActivitySource.SourceName ? "built, because the callsign is set" : "built")
                + "; enabled " + settings.IsSourceEnabled(name));
        }

        var held = 0;

        foreach (var pinned in new[] { "20 m", null })
        {
            for (var run = 1; run <= 5; run++)
            {
                held += TraceTheSpotReload(pinned, run, arrangement) ? 1 : 0;
            }
        }

        _output.WriteLine("=== " + arrangement + ": the pin held in " + held + " of 10");
    }

    /// <summary>
    /// One pinned 1920 PSK31 window for <see cref="Unit352TraceTheSpotReloadOnTheTestWindow"/>, with its
    /// telemetry read back after the window closes.
    /// </summary>
    /// <returns>Whether the pin held.</returns>
    private bool TraceTheSpotReload(string? pinned, int run, string arrangement)
    {
        var folder = Path.Combine(Path.GetTempPath(), "hamlet-unit352-reload-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(folder);

        var telemetry = new JsonlTelemetry(folder, "352", _ => true);
        var moves = new List<(DateTime At, string What)>();
        var start = DateTime.UtcNow;
        var pinnedAt = DateTime.MaxValue;
        var readAt = DateTime.MaxValue;
        var held = false;
        var window = Realized(1920, telemetry);
        var model = (MainWindowViewModel)window.DataContext!;

        string Ms(DateTime t) => "+" + (t - start).TotalMilliseconds.ToString("0", CultureInfo.InvariantCulture) + " ms";

        string When(DateTime t) => t < pinnedAt ? "before the pin" : t <= readAt ? "AFTER THE PIN" : "after the read-back";

        string Badged() => string.Join(", ", model.Bands.Where(b => b.IsBestBet).Select(b => b.Band.Name));

        void OnBand(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BandButtonViewModel.IsBestBet) && sender is BandButtonViewModel b)
            {
                moves.Add((DateTime.UtcNow, "IsBestBet " + b.IsBestBet + " on " + b.Band.Name));
            }
        }

        void OnModel(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainWindowViewModel.SourcesSummary))
            {
                moves.Add((DateTime.UtcNow, "sources summary [" + model.SourcesSummary + "]"));
            }
        }

        _output.WriteLine("=== 1920 PSK31, best bet pinned " + (pinned ?? "absent") + ", run " + run + "; " + arrangement);
        _output.WriteLine(
            "  realized " + Ms(DateTime.UtcNow) + ": best bet on [" + Badged() + "], sources summary [" + model.SourcesSummary
            + "], spots on the list " + model.Spots.Count);

        foreach (var band in model.Bands)
        {
            band.PropertyChanged += OnBand;
        }

        model.PropertyChanged += OnModel;

        try
        {
            model.ChosenDigitalMode = "PSK31";
            Settle(window);

            pinnedAt = DateTime.UtcNow;

            foreach (var band in model.Bands)
            {
                band.IsBestBet = band.Band.Name == pinned;
            }

            model.NotifyGreenZoneForTests();
            moves.Add((DateTime.UtcNow, "the pin set"));
            Settle(window);

            Named<Border>(window, "DigitalReadinessStrip").IsVisible = false;
            Settle(window);

            readAt = DateTime.UtcNow;

            var badged = model.Bands.Where(b => b.IsBestBet).Select(b => b.Band.Name).ToList();
            var bet = Named<Button>(window, "GreenZoneBestBet");
            var bySource = model.Spots
                .Select(s => s.Provenance.Split(" · ") is { Length: >= 2 } parts ? parts[1] : "?")
                .GroupBy(s => s)
                .Select(g => g.Count() + " " + g.Key);

            held = badged.SequenceEqual(pinned is null ? Array.Empty<string>() : new[] { pinned })
                && bet.IsEffectivelyVisible == (pinned is not null);

            _output.WriteLine(
                "  read back " + Ms(readAt) + ": best bet on [" + string.Join(", ", badged) + "], green block says ["
                + (bet.IsEffectivelyVisible ? bet.Content as string : "") + "]; pin held " + held);
            _output.WriteLine(
                "  sources summary [" + model.SourcesSummary + "]; spots on the list " + model.Spots.Count + " ("
                + string.Join(", ", bySource) + "); heard in the last minute " + model.HeardInTheLastMinute);
        }
        finally
        {
            foreach (var band in model.Bands)
            {
                band.PropertyChanged -= OnBand;
            }

            model.PropertyChanged -= OnModel;
            model.ChosenDigitalMode = "FT8";
            window.Close();
            telemetry.Dispose();
        }

        var events = new List<(DateTime At, string What)>();

        foreach (var file in Directory.GetFiles(folder, "*.jsonl"))
        {
            foreach (var line in File.ReadAllLines(file))
            {
                using var doc = System.Text.Json.JsonDocument.Parse(line);
                var root = doc.RootElement;
                var name = root.GetProperty("event").GetString() ?? "";

                if (!(name.Contains("spot", StringComparison.Ordinal) || name.Contains("source", StringComparison.Ordinal)
                    || name.Contains("rbn", StringComparison.Ordinal) || name.Contains("pota", StringComparison.Ordinal)
                    || name.Contains("lead", StringComparison.Ordinal) || name == "band_changed"))
                {
                    continue;
                }

                var at = DateTime.Parse(
                    root.GetProperty("ts").GetString()!, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

                events.Add((at, "telemetry " + name + " " + root.GetProperty("data").GetRawText()));
            }
        }

        foreach (var (at, what) in events.Concat(moves).OrderBy(e => e.At))
        {
            _output.WriteLine("    " + Ms(at).PadLeft(10) + " " + When(at).PadRight(20) + what);
        }

        var reloads = events.Where(e => e.What.StartsWith("telemetry spots_refreshed", StringComparison.Ordinal)).ToList();

        _output.WriteLine(
            "  spot reloads " + reloads.Count + ", after the pin and before the read-back "
            + reloads.Count(e => e.At >= pinnedAt && e.At <= readAt));

        return held;
    }

    /// <summary>What one window measured with the best bet set by hand, and whether it stayed set.</summary>
    public sealed record BestBetCase(
        double TopRow,
        double Below,
        Rect Card,
        Rect Rig,
        Rect Block,
        double PanelsShown,
        double PanelsHidden,
        bool StripShown,
        bool VisibleBeforePin,
        bool PinHeld,
        string PinWhy,
        IReadOnlyList<string> Badged,
        bool BestBetVisible,
        string BestBetSaid);

    /// <summary>
    /// Realizes the licensed window on <paramref name="mode"/>, sets the best bet on
    /// <paramref name="pinned"/> or on no band, and measures the top row and the panels.
    /// </summary>
    /// <remarks>
    /// **THE PIN IS SET ON THE TEST WINDOW ONLY** (the arbiter's ruling 49), by `IsBestBet` and
    /// `NotifyGreenZoneForTests`, after the mode has settled, and read back after every settle
    /// that follows. Nothing is pressed.
    /// </remarks>
    private static BestBetCase WithTheBestBetPinned(double width, string mode, string? pinned)
    {
        var window = Realized(width);
        var model = (MainWindowViewModel)window.DataContext!;

        try
        {
            model.ChosenDigitalMode = mode;
            Settle(window);

            var bet = Named<Button>(window, "GreenZoneBestBet");
            var visibleBeforePin = bet.IsEffectivelyVisible;

            foreach (var band in model.Bands)
            {
                band.IsBestBet = band.Band.Name == pinned;
            }

            model.NotifyGreenZoneForTests();
            Settle(window);

            var m = Measure(window);
            var pills = window.GetVisualDescendants().OfType<ItemsControl>()
                .First(i => i.GetVisualDescendants().OfType<Button>().Any(b => b.Classes.Contains("hm-band")));
            var below = window.Bounds.Height - RectIn(pills, window).Bottom;
            var block = RectIn(Block(window), window);
            var strip = Named<Border>(window, "DigitalReadinessStrip");
            var stripShown = strip.IsEffectivelyVisible;
            var shown = TheWorkingPanelsTests.Panels(window)[0].Rect.Height;

            strip.IsVisible = false;
            Settle(window);

            var hidden = TheWorkingPanelsTests.Panels(window)[0].Rect.Height;

            // **READ BACK AFTER THE LAST SETTLE**: a re-rank would have moved the badge.
            var badged = model.Bands.Where(b => b.IsBestBet).Select(b => b.Band.Name).ToList();
            var said = bet.Content as string ?? "";
            var why = new List<string>();

            if (!badged.SequenceEqual(pinned is null ? Array.Empty<string>() : new[] { pinned }))
            {
                why.Add("IsBestBet is on [" + string.Join(", ", badged) + "], pinned [" + pinned + "]");
            }

            if (bet.IsEffectivelyVisible != (pinned is not null))
            {
                why.Add("the green block's best bet is " + (bet.IsEffectivelyVisible ? "drawn" : "not drawn"));
            }

            if (pinned is not null && !said.StartsWith(pinned, StringComparison.Ordinal))
            {
                why.Add("the green block's best bet says [" + said + "]");
            }

            return new BestBetCase(
                m.TopRowHeight, below, m.Card, m.Rig, block, shown, hidden, stripShown, visibleBeforePin,
                why.Count == 0, string.Join("; ", why), badged, bet.IsEffectivelyVisible, said);
        }
        finally
        {
            model.ChosenDigitalMode = "FT8";
            window.Close();
        }
    }

    private static string Share(double part, double whole)
        => (part / whole).ToString("0.000", CultureInfo.InvariantCulture);

    /// <summary>A visual's margin, and its border and padding where it has them.</summary>
    private static string Spacing(Control v)
    {
        static string T(Thickness t) => Px(t.Left) + "," + Px(t.Top) + "," + Px(t.Right) + "," + Px(t.Bottom);

        var (border, padding) = v switch
        {
            Border b => (b.BorderThickness, b.Padding),
            TemplatedControl t => (t.BorderThickness, t.Padding),
            Decorator d => (default(Thickness), d.Padding),
            Avalonia.Controls.Presenters.ContentPresenter p => (p.BorderThickness, p.Padding),
            _ => (default(Thickness), default(Thickness)),
        };

        return "margin " + T(v.Margin) + ", border " + T(border) + ", padding " + T(padding);
    }

    // ------------------------------------------------------------------------------------

    /// <summary>The power offer's border: the box `HasPsk31PowerOffer` shows, around the sentence.</summary>
    /// <remarks>
    /// **BY THE LOGICAL TREE SINCE WORK INSTRUCTION 341** (ruling 8): inside a closed popup the
    /// sentence has no visual parent, and its border is still its logical ancestor.
    /// </remarks>
    public static Border OfferBorder(Window window)
        => InOffer<TextBlock>(window, "DigitalPsk31PowerOffer").GetLogicalAncestors().OfType<Border>().First();

    /// <summary>
    /// A control of the full power offer, found in a popup's content whether the popup is open or
    /// not, or on the window where no popup holds it.
    /// </summary>
    /// <remarks>
    /// **RULING 8 MOVED THE OFFER INTO A POPUP** (work instruction 341), and a closed popup's
    /// content is in the logical tree only, so a visual walk of the window cannot find it.
    /// </remarks>
    public static T InOffer<T>(Window window, string name)
        where T : Control
    {
        var found = window.GetVisualDescendants().OfType<Popup>()
            .Select(p => p.Child)
            .OfType<Control>()
            .SelectMany(c => new ILogical[] { c }.Concat(c.GetLogicalDescendants()))
            .OfType<T>()
            .FirstOrDefault(c => c.Name == name)
            ?? window.GetVisualDescendants().OfType<T>().FirstOrDefault(c => c.Name == name);

        Assert.True(found is not null, "there is no " + typeof(T).Name + " named " + name + " in a popup or on the window");

        return found!;
    }

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
    public static Window Realized(double width) => Realized(width, null);

    /// <summary>The same window, with the model recording to <paramref name="telemetry"/>.</summary>
    /// <param name="width">How wide the window is.</param>
    /// <param name="telemetry">Where the model records, or null for nowhere.</param>
    public static Window Realized(double width, JsonlTelemetry? telemetry)
    {
        var settings = FixtureSettings();

        var model = new MainWindowViewModel(settings, telemetry)
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

    /// <summary>The spot sources that reach over the network: POTA, SOTA and RBN.</summary>
    public static readonly string[] NetworkSources =
    {
        Hamlet.RadioEngine.Explore.PotaActivitySource.SourceName,
        Hamlet.RadioEngine.Explore.SotaActivitySource.SourceName,
        Hamlet.RadioEngine.Explore.RbnActivitySource.SourceName,
    };

    /// <summary>The licensed operator the test window is built for, with the network spot sources off.</summary>
    /// <remarks>
    /// <para>**THE NETWORK SOURCES ARE SWITCHED OFF SINCE WORK INSTRUCTION 352** (the arbiter's ruling
    /// 61, the unit's own and overrulable). The callsign made the window a real operator: POTA and RBN
    /// were on by default (`AppSettings.DefaultSourceEnabled`), and the reload `SelectBand` starts
    /// waited on POTA's reply. `Unit352TraceTheSpotReloadOnTheTestWindow` as built (`4ad20a25`), at 1920
    /// on PSK31: the pin held in 7 of 10 windows, and each of the 3 misses was that `band_changed`
    /// reload with 0 spots, landing 86 to 116 ms in, after the pin, with the sources summary turning to
    /// *POTA, RBN* as it landed and the badge going to 40 m on the hour's table. Each source is switched
    /// off by its own `SourceName`, through `SetSourceEnabled`, so nothing is fetched and nothing is
    /// recorded into the run's spot history. With the change the same ten windows took both reloads,
    /// `band_changed` and `startup`, 16 to 148 ms in (1246 on the first window of one run), inside
    /// `Realized`'s own settle and before the pin, and the pin held in 10 of 10 in each of four trace
    /// runs. **The callsign, grid, license and dial are unchanged**, and the
    /// sample feed stays at its default, off.</para>
    /// <para>**WHAT THAT MOVES, MEASURED.** Because both reloads now land inside `Realized`, a test that
    /// does not pin the best bet measures the one the ranking gives - the hour's table, *80 m* or *40 m*
    /// at the hour of the run - and the count the reload gives, 0 to 2 stations where the fixture set 6.
    /// At 1920 its green block is 64 px on FT8 and 76 on PSK31 (was 55 and 67), the pinned fact's drawn
    /// numbers. The pinned facts' numbers did not move.</para>
    /// </remarks>
    public static AppSettings FixtureSettings()
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.LicenseClass = LicenseClass.General;
        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = HisGrid;

        foreach (var name in NetworkSources)
        {
            settings.SetSourceEnabled(name, false);
        }

        return settings;
    }

    private static string Px(double value)
        => value.ToString("0.0", CultureInfo.InvariantCulture);

    private static string Box(Rect r)
        => Px(r.Width) + " x " + Px(r.Height) + " at " + Px(r.X) + "," + Px(r.Y);
}
