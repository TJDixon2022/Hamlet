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
/// <para>**THIS TYPE IS RED ON ONE NAME AND THAT IS DELIBERATE.**
/// <see cref="TheWorkingPanelsScrollInsideThemselvesRatherThanCollapsing"/> fails against the tree,
/// and unit 372 did not loosen it to make a criterion pass (`PHASE_PLAN.md` §6). **Criterion 1.3
/// is `partial`**, measured, with the cause named. The other two names pass: the send area stays
/// put and keeps its 22 px, and the header and the status bar are pinned.</para>
/// <para>**WHY NOTHING WAS REPAIRED, IN NUMBERS.** At 1100 x 620 - the worst size that can be
/// reached - the workspace region is given **51 px**, and it needs **138 px** before the panels get
/// their first pixel: 26 px of the boundary's border and padding, and 112 px of the mode strip and
/// the row beneath it. The deficit is **87 px**. At width 1100 the panel row's height is exactly
/// *window height minus 707*, which reads 33 px at 740, 0 at 700 and 0 at 620. **The only pool of
/// height above the panels is <c>TopRow</c>'s 300 px** - the radio's own face inside it measures
/// 110 px, so some of it could be taken without clipping the picture. But taking the 87 px of
/// deficit plus any usable viewport means taking about 147 px, and the grid would have to start
/// taking it at 767 px of window height and below - **which includes 1280 x 720 and 1366 x 728, two
/// of unit 354's nine sizes where the layout is sound today** and where unit 356 measured its cap as
/// inert. There is no way to express *keep the panels at a floor and let the top row pay for it* in
/// this grid, because the rest of the canvas varies with width as well as height, so the repair is a
/// redesign of how root row 1 allocates height and not a layout number. **It was not made, and it is
/// raised in unit 372's report section 4 as a finding that wants a ruling.**</para>
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
    /// The sizes 1.3 is about: three below <c>MinHeight</c> at the opening width, and the smallest
    /// size Hamlet will open at.
    /// </summary>
    private static readonly (double Width, double Height)[] Small =
    {
        (1100, 580), (1100, 540), (1100, 500), (900, 620),
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
    /// **The working panels scroll inside themselves**: the decoded panel and the For You panel each
    /// have a scroller whose extent exceeds its viewport, and scrolling it to the end shows its last
    /// content. **Nothing is hidden, only scrolled** (§0.5) - a panel collapsed to nothing with its
    /// content unreachable fails this, and so does a panel whose bottom is off the window.
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

                foreach (var (name, rect) in TheWorkingPanelsTests.Panels(window))
                {
                    // **THE PART THAT APPLIES TO ALL THREE**: it is drawn, and it is on the window.
                    if (rect.Height <= 0.5)
                    {
                        misses.Add(
                            label + ": the " + name + " panel is " + Px(rect.Height)
                            + " px tall. It has collapsed to nothing and its content cannot be "
                            + "reached, which is hiding information rather than detail (CLAUDE.md 0.5).");
                        continue;
                    }

                    if (!Whole(rect, bounds))
                    {
                        misses.Add(
                            label + ": the " + name + " panel " + Box(rect)
                            + " is not whole on the " + Box(bounds) + " window, so the bottom of "
                            + "what it is showing cannot be seen.");
                    }

                    if (name == "waterfall")
                    {
                        continue;
                    }

                    // **AND THE PART THAT APPLIES TO A LIST**: it scrolls, and the end is reachable.
                    var scroll = Scroller(window, name);

                    if (scroll is null)
                    {
                        misses.Add(label + ": the " + name + " panel has no scroller of its own");
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
