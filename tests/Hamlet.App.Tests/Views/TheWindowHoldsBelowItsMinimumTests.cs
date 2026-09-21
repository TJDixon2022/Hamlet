using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 372 task 3, criterion 1.3: **below the sum of the minimums, the panels scroll
/// inside themselves and the send area stays put.**
/// </summary>
/// <remarks>
/// <para>**THE SIZES, AND WHAT THE TREE DOES WITH THEM.** The instruction names 1100 x 580, 540 and
/// 500, and 900 x 620, the smallest size Hamlet will open at. **A headless window honors
/// <c>MinHeight</c>**: asked for 580, 540 and 500, <c>Unit372TraceTests</c> measured a window that
/// drew 620 every time. So the three sizes below the minimum are one size, 1100 x 620, reached three
/// ways, and this type says so rather than pretending to four readings. That is a finding and it is
/// reported; it is not a reason to skip them, because what the application does when it is asked for
/// a size it will not take is worth knowing.</para>
/// <para>**THE WATERFALL IS HELD TO A DIFFERENT LINE THAN THE OTHER TWO, AND THIS IS THE UNIT'S OWN
/// READING** (author's, overrulable, a layout reading and never a stop under R31). Criterion 1.3 asks
/// that each working panel have a scroller whose extent exceeds its viewport. **The waterfall has no
/// scroller at all and never had one** - it is a drawn surface, not a list, and
/// <c>Unit372TraceTests</c> measured zero <c>ScrollViewer</c>s inside it at every one of the nine
/// sizes. Requiring one there would be writing a test for a door this unit is not building (R14).
/// What is asserted of the waterfall is the part of 1.3 that does apply to it: it keeps a usable
/// height and is whole on the window, so it is not the collapsed-to-nothing case §0.5 forbids. The
/// scroller half is asserted of the decoded panel and the For You panel, which are the two
/// <c>ThePanelScrollsTests</c> and <c>TheDecodedPanelScrollsItselfTests</c> are about.</para>
/// <para>**UNIT 372 LEFT THIS TYPE RED ON ONE NAME, DELIBERATELY, AND UNIT 373 TURNED IT GREEN BY
/// CHANGING THE LAYOUT.** The repair is at the site work instruction 373 §6 names: a measured floor
/// and a scroller of its own on the panel canvas inside `WorkspaceBoundary`
/// (`WorkspaceCanvasScroller` and `WorkspaceCanvas` in `MainWindow.axaml`), never on the tab row
/// that carries the send area and never funded from <c>TopRow</c>'s 300 px cap. **At 900 x 620 and
/// 1100 x 620 the panel row goes from 0 px to 73 px**: the decoded panel reads viewport 14 in an
/// extent of 36 and scrolls to show all 36, For You reads viewport 25 in an extent of 371 and
/// scrolls to show all 371, and the waterfall is 73 px of drawn surface instead of none.</para>
/// <para>**ONE CLAUSE OF THIS TYPE WAS REWRITTEN UNDER §R12, AND THIS IS THE ACCOUNT OF IT.** Unit
/// 372 wrote *each panel is whole on the window* while the canvas did not scroll, and with a canvas
/// that does not scroll that clause is the right way to say *nothing is hidden*: a panel below the
/// window's bottom edge was simply lost. **It is a test written while a door was shut, and it blocks
/// the unit told to open the door** - a panel taller than its viewport is never whole on the window,
/// so no scrolling canvas can ever satisfy it, and the clause would have had to be read as *the
/// canvas may not scroll*, which is the opposite of criterion 1.3. §R12 makes that this session's to
/// rewrite, in its own commit, so it guards the rule and not the shut door. **What replaces it
/// asserts more, not less**: each panel's first row is on the window with the canvas at rest, and
/// **each panel's last row is on the window once the canvas is scrolled to its end** - which is
/// §0.5's actual rule, *nothing hidden, only scrolled*, and which *whole on the window* never
/// checked, because a panel can be whole and still have unreachable content. Where the canvas does
/// not scroll at all the two readings are identical and this is the old clause exactly.</para>
/// <para>**AND ONE ASSERTION WAS ADDED THAT UNIT 372 DID NOT MAKE**: each list panel's scroller must
/// report <c>viewport &gt; 0</c> in its own right. The inherited fault was a viewport of 0 with
/// content behind it, and <c>extent &gt; viewport</c> alone does not forbid it - 36 in a viewport of
/// 0 satisfies that comparison and is exactly the state §0.5 calls hiding information.</para>
/// <para>**THE NUMBERS THE FLOOR WAS BUILT ON** (`Unit373TraceTests`, task 1). 125 px stand between
/// `WorkspaceBoundary`'s outer edge and the first panel pixel at widths 900 and 1100, and 108 px at
/// 1920 - **constant down the whole sweep at each width**, so the floor is one number and not three.
/// Unit 372's 138 px at width 1100 is 13 px high; the measured figure is 125, being 13 px of the
/// border's own top edge and padding plus 112 px of the mode strip and the readiness strip. Inside
/// the canvas that leaves the panel row at canvas height minus 112, and all three panels report a
/// working scroller only while the panel row is between 60 and 95 px - the decoded list's content
/// measures 36 px and the panel's chrome takes 59 of the row. **The canvas floor's working band is
/// 172 to 207 px and 185 is the number taken**, being the canvas Hamlet already draws at 1100 x 780,
/// the size it opens at, so nothing changes at the opening size.</para>
/// <para>**PINNED MEANS NO SCROLLING ANCESTOR** (HM-DEC-051). The header and the status bar are
/// asserted whole on the window, at the top and at the bottom of it, and with no <c>ScrollViewer</c>
/// anywhere above them - which is the crisp form of *pinned*, and the one a layout change could
/// break without moving either box a pixel.</para>
/// <para>**COMPUTED, NOT SEEN** (<c>SHACK_FACTS.md</c> FACT-004). Every number is a <c>Bounds</c>
/// rectangle off a realized headless window. Nothing is pressed (CLAUDE.md §0.2), nothing is opened,
/// and nothing here is evidence about the radio.</para>
/// </remarks>
public sealed class TheWindowHoldsBelowItsMinimumTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where every box is printed.</param>
    public TheWindowHoldsBelowItsMinimumTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// The sizes 1.3 is about: three below <c>MinHeight</c> at the opening width, the size all three
    /// of those land on, and the smallest size Hamlet will open at.
    /// </summary>
    /// <remarks>
    /// **1100 x 620 IS NAMED IN ITS OWN RIGHT SINCE UNIT 373** (work instruction 373 task 2, which
    /// asks for 900 x 620 and 1100 x 620). It is where 580, 540 and 500 all land, and a size the
    /// criterion is about should be in the list under the name it is measured at rather than only
    /// reached by asking for one the window will not take.
    /// </remarks>
    private static readonly (double Width, double Height)[] Small =
    {
        (1100, 580), (1100, 540), (1100, 500), (1100, 620), (900, 620),
    };

    /// <summary>The height the send area draws at the size Hamlet opens at, read rather than declared.</summary>
    private const double AtTheOpeningSize = 780;

    /// <summary>The five controls R34 says are never the thing that leaves.</summary>
    private static readonly string[] TheSendArea =
    {
        "DigitalSendCqButton", "ModeTabs", "DigitalSendReserved",
        "DigitalTransmitDriveNote", "DigitalStopButton",
    };

    /// <summary>
    /// **The send area is whole on the window and the same height it is at 780.** It does not
    /// shrink and it does not go off the bottom.
    /// </summary>
    [AvaloniaFact]
    public void TheSendAreaStaysPutAndKeepsItsHeight()
    {
        var misses = new List<string>();

        foreach (var plain in new[] { false, true })
        {
            var tall = SendAreaHeight(1100, AtTheOpeningSize, plain);

            _output.WriteLine(
                Which(plain) + ": the send area is " + Px(tall) + " px tall at 1100 x 780");

            foreach (var (width, height) in Small)
            {
                var window = Realize(width, height, plain);

                try
                {
                    Pump(window);

                    var bounds = window.Bounds;
                    var label = Which(plain) + " at " + Px(width) + " x " + Px(height)
                        + " (drawn " + Px(bounds.Width) + " x " + Px(bounds.Height) + ")";

                    foreach (var name in TheSendArea)
                    {
                        var control = TheTopRowTests.Named<Control>(window, name);
                        var at = TheTopRowTests.RectIn(control, window);

                        _output.WriteLine(
                            label + "  " + name.PadRight(26) + Box(at)
                            + "  whole " + Whole(at, bounds));

                        if (!Whole(at, bounds))
                        {
                            misses.Add(
                                label + ": " + name + " " + Box(at) + " is not whole on the "
                                + Box(bounds) + " window");
                        }
                    }

                    var reserved = TheTopRowTests.RectIn(
                        TheTopRowTests.Named<Control>(window, "DigitalSendReserved"), window).Height;

                    if (Math.Abs(reserved - tall) > 1)
                    {
                        misses.Add(
                            label + ": the send area is " + Px(reserved) + " px tall here and "
                            + Px(tall) + " px at 1100 x 780. It gave up height to make room.");
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

    /// <summary>
    /// **The working panels scroll inside themselves**: none of the three has collapsed, the decoded
    /// panel and the For You panel each have a scroller reporting a viewport above zero and an
    /// extent above it, and scrolling each to the end shows its last content. **Nothing is hidden,
    /// only scrolled** (§0.5) - a panel collapsed to nothing with its content unreachable fails
    /// this, and so does a panel whose last row cannot be brought onto the window.
    /// </summary>
    [AvaloniaFact]
    public void TheWorkingPanelsScrollInsideThemselvesRatherThanCollapsing()
    {
        var misses = new List<string>();

        // **THE WINDOW WITH CONTENT ONLY.** A panel with nothing in it has nothing to scroll and
        // nothing to hide, so the pinned-facts window - which declares no decoded rows and no card -
        // cannot answer this one either way.
        foreach (var (width, height) in Small)
        {
            var window = Realize(width, height, plain: true);

            try
            {
                Pump(window);

                var bounds = window.Bounds;
                var label = Px(width) + " x " + Px(height)
                    + " (drawn " + Px(bounds.Width) + " x " + Px(bounds.Height) + ")";

                Say(window, label);

                // **THE CANVAS ITSELF, WHICH IS WHAT CARRIES THE FLOOR.** Its extent may exceed its
                // viewport - that is the repair - but it may never clip: whatever the floor holds
                // open has to be reachable by scrolling it.
                var canvas = TheTopRowTests.Named<ScrollViewer>(window, "WorkspaceCanvasScroller");
                var end = Math.Max(0, canvas.Extent.Height - canvas.Viewport.Height);

                _output.WriteLine(
                    "    the canvas viewport " + Px(canvas.Viewport.Height) + ", extent "
                    + Px(canvas.Extent.Height) + ", so it scrolls " + Px(end) + " px");

                if (canvas.Viewport.Height <= 0.5)
                {
                    misses.Add(
                        label + ": the panel canvas has a viewport of " + Px(canvas.Viewport.Height)
                        + ", so nothing it holds can be seen at all.");
                }

                // **EVERY PANEL'S LAST ROW, READ WITH THE CANVAS SCROLLED TO ITS END.** This is the
                // clause that replaced unit 372's *whole on the window* under §R12, and the reading
                // is taken once for all three rather than per panel.
                var atRest = TheWorkingPanelsTests.Panels(window);

                canvas.Offset = new Vector(canvas.Offset.X, end);
                Pump(window);

                var atEnd = TheWorkingPanelsTests.Panels(window);

                canvas.Offset = new Vector(canvas.Offset.X, 0);
                Pump(window);

                for (var i = 0; i < atRest.Count; i++)
                {
                    var (name, rect) = atRest[i];
                    var scrolled = atEnd[i].Rect;

                    _output.WriteLine(
                        "    " + name.PadRight(10) + "at rest " + Box(rect)
                        + "; canvas at its end " + Box(scrolled));

                    // **THE PART THAT APPLIES TO ALL THREE**: it is drawn at all.
                    if (rect.Height <= 0.5)
                    {
                        misses.Add(
                            label + ": the " + name + " panel is " + Px(rect.Height)
                            + " px tall. It has collapsed to nothing and its content cannot be "
                            + "reached, which is hiding information rather than detail (CLAUDE.md 0.5).");
                        continue;
                    }

                    // **AND THAT EVERY ROW OF IT CAN BE BROUGHT ONTO THE WINDOW.** Its first row is
                    // on the window with the canvas at rest, and its last row is on the window with
                    // the canvas scrolled to its end. Where the canvas does not scroll these are one
                    // reading and this is *whole on the window* exactly.
                    if (rect.Top < -0.5 || rect.Left < -0.5 || rect.Right > bounds.Width + 0.5)
                    {
                        misses.Add(
                            label + ": the " + name + " panel " + Box(rect)
                            + " does not start on the " + Box(bounds) + " window with the canvas at "
                            + "rest, so the top of what it is showing cannot be seen.");
                    }

                    if (scrolled.Bottom > bounds.Height + 0.5)
                    {
                        misses.Add(
                            label + ": the " + name + " panel ends at y " + Px(scrolled.Bottom)
                            + " with the canvas scrolled to its end on a window " + Px(bounds.Height)
                            + " px tall, so the bottom of what it is showing cannot be reached.");
                    }

                    if (name == "waterfall")
                    {
                        continue;
                    }

                    // **AND THE PART THAT APPLIES TO A LIST**: it has a viewport of its own, it
                    // scrolls, and the end is reachable.
                    var scroll = Scroller(window, name);

                    if (scroll is null)
                    {
                        misses.Add(label + ": the " + name + " panel has no scroller of its own");
                        continue;
                    }

                    // **THE INHERITED FAULT WAS A VIEWPORT OF ZERO WITH CONTENT BEHIND IT**, and
                    // `extent > viewport` alone does not forbid it: 36 in a viewport of 0 satisfies
                    // that comparison and is the exact state §0.5 calls hiding information.
                    if (scroll.Viewport.Height <= 0.5)
                    {
                        misses.Add(
                            label + ": the " + name + " panel's scroller has a viewport of "
                            + Px(scroll.Viewport.Height) + " holding an extent of "
                            + Px(scroll.Extent.Height)
                            + ", so its content is there and cannot be seen (CLAUDE.md 0.5).");
                        continue;
                    }

                    if (scroll.Extent.Height <= scroll.Viewport.Height + 0.5)
                    {
                        misses.Add(
                            label + ": the " + name + " panel's scroller has extent "
                            + Px(scroll.Extent.Height) + " in a viewport of "
                            + Px(scroll.Viewport.Height)
                            + ", so this size proves nothing about scrolling");
                        continue;
                    }

                    scroll.Offset = new Vector(scroll.Offset.X, scroll.Extent.Height);
                    Pump(window);

                    var shown = scroll.Offset.Y + scroll.Viewport.Height;

                    _output.WriteLine(
                        "    " + name + " scrolled to " + Px(scroll.Offset.Y) + ", showing up to "
                        + Px(shown) + " of " + Px(scroll.Extent.Height));

                    if (shown < scroll.Extent.Height - 1)
                    {
                        misses.Add(
                            label + ": the " + name + " panel scrolled to its end shows up to "
                            + Px(shown) + " of " + Px(scroll.Extent.Height)
                            + ", so its last content cannot be reached");
                    }
                }
            }
            finally
            {
                window.Close();
            }
        }

        Assert.True(misses.Count == 0, string.Join(Environment.NewLine, misses));
    }

    /// <summary>
    /// **The status bar and the header stay pinned** (HM-DEC-051): both whole on the window, the
    /// header at the top and the bar at the bottom, and neither inside anything that scrolls.
    /// </summary>
    [AvaloniaFact]
    public void TheHeaderAndTheStatusBarStayPinned()
    {
        var misses = new List<string>();

        foreach (var plain in new[] { false, true })
        {
            foreach (var (width, height) in Small)
            {
                var window = Realize(width, height, plain);

                try
                {
                    Pump(window);

                    var bounds = window.Bounds;
                    var label = Which(plain) + " at " + Px(width) + " x " + Px(height)
                        + " (drawn " + Px(bounds.Width) + " x " + Px(bounds.Height) + ")";

                    // **THE MENU STANDS FOR THE HEADER**, which carries no name of its own. It is
                    // in the root grid's pinned first row and nothing else in the window has one.
                    var header = (Control)window.GetVisualDescendants().OfType<Menu>().First();
                    var bar = TheTopRowTests.Named<Border>(window, "StatusBar");
                    var headerAt = TheTopRowTests.RectIn(header, window);
                    var barAt = TheTopRowTests.RectIn(bar, window);
                    var headerScrolls = header.GetVisualAncestors().OfType<ScrollViewer>().Count();
                    var barScrolls = bar.GetVisualAncestors().OfType<ScrollViewer>().Count();

                    _output.WriteLine(
                        label + ": the header menu " + Box(headerAt) + " whole " + Whole(headerAt, bounds)
                        + ", scrolling ancestors " + headerScrolls
                        + "; the status bar " + Box(barAt) + " whole " + Whole(barAt, bounds)
                        + ", scrolling ancestors " + barScrolls);

                    void Miss(bool holds, string what)
                    {
                        if (!holds)
                        {
                            misses.Add(label + ": " + what);
                        }
                    }

                    Miss(Whole(headerAt, bounds), "the header " + Box(headerAt) + " is not whole on the window");
                    Miss(Whole(barAt, bounds), "the status bar " + Box(barAt) + " is not whole on the window");
                    Miss(headerScrolls == 0, "the header is inside " + headerScrolls + " scrollers, so it is not pinned");
                    Miss(barScrolls == 0, "the status bar is inside " + barScrolls + " scrollers, so it is not pinned");
                    Miss(headerAt.Top < barAt.Top, "the header is not above the status bar");
                    Miss(
                        barAt.Bottom >= bounds.Height - 40,
                        "the status bar ends at " + Px(barAt.Bottom) + " on a window "
                        + Px(bounds.Height) + " tall, so it is not at the bottom");
                }
                finally
                {
                    window.Close();
                }
            }
        }

        Assert.True(misses.Count == 0, string.Join(Environment.NewLine, misses));
    }

    /// <summary>Prints the boxes a reader needs to see why a size did or did not hold.</summary>
    private void Say(Window window, string label)
    {
        var bounds = window.Bounds;

        _output.WriteLine(label + ":");

        foreach (var name in new[]
        {
            "TopRow", "DigitalModeStrip", "DigitalTuneStrip", "WorkspaceBoundary",
            "DigitalPanes", "DigitalSendReserved", "StatusBar",
        })
        {
            var at = TheTopRowTests.RectIn(TheTopRowTests.Named<Control>(window, name), window);

            _output.WriteLine("    " + name.PadRight(22) + Box(at) + "  whole " + Whole(at, bounds));
        }

        // **THE RADIO'S OWN FACE, WHICH CARRIES NO NAME AND BOUNDS ANY REPAIR.** It is inside
        // `TopRow` and HM-DEC-086 says the strip it sits in cannot be closed or moved. It has no
        // scroller of its own, so height taken off `TopRow` past this box clips the picture instead
        // of scrolling it, and a picture that draws less than it claims is §0.0's fault in a
        // display (HM-DEC-092). What TopRow can spare is TopRow's height minus this.
        var rig = window.GetVisualDescendants().OfType<Hamlet.App.Controls.RigDisplayControl>().FirstOrDefault();

        if (rig is not null)
        {
            var rigAt = TheTopRowTests.RectIn(rig, window);
            var topRow = TheTopRowTests.RectIn(TheTopRowTests.Named<Control>(window, "TopRow"), window);

            _output.WriteLine(
                "    " + "the rig display".PadRight(22) + Box(rigAt)
                + "  so TopRow can spare " + Px(topRow.Height - rigAt.Height)
                + " px without clipping it");
        }

        foreach (var (name, rect) in TheWorkingPanelsTests.Panels(window))
        {
            var scroll = Scroller(window, name);

            _output.WriteLine(
                "    panel " + name.PadRight(16) + Box(rect) + "  whole " + Whole(rect, bounds)
                + (scroll is null
                    ? "  no scroller"
                    : "  viewport " + Px(scroll.Viewport.Height) + " extent " + Px(scroll.Extent.Height)));
        }
    }

    /// <summary>
    /// The scroller a list panel scrolls its own content in, found from the items control it fills
    /// the way that panel's own tests find it.
    /// </summary>
    private static ScrollViewer? Scroller(Window window, string name)
    {
        var anchor = name == "decoded" ? "DigitalDecodedRows" : "DigitalContactCards";

        return window.GetVisualDescendants().OfType<Control>()
            .FirstOrDefault(c => c.Name == anchor)
            ?.GetVisualAncestors().OfType<ScrollViewer>().FirstOrDefault();
    }

    /// <summary>The reserved send area's drawn height on one window.</summary>
    private static double SendAreaHeight(double width, double height, bool plain)
    {
        var window = Realize(width, height, plain);

        try
        {
            Pump(window);

            return TheTopRowTests.RectIn(
                TheTopRowTests.Named<Control>(window, "DigitalSendReserved"), window).Height;
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

    private static bool Whole(Rect at, Rect bounds)
        => at.Width > 0 && at.Height > 0
            && at.Left >= -0.5 && at.Top >= -0.5
            && at.Right <= bounds.Width + 0.5
            && at.Bottom <= bounds.Height + 0.5;

    private static string Which(bool plain)
        => plain ? "the window with content" : "the pinned-facts window";

    /// <summary>Runs the dispatcher and lays the window out.</summary>
    private static void Pump(Window window)
    {
        for (var i = 0; i < 6; i++)
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
