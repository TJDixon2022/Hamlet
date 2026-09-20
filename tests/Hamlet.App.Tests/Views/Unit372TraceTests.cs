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
/// Work instruction 372 task 1: **the trace - what the tree already does as the window shrinks.**
/// </summary>
/// <remarks>
/// <para>**THIS ASSERTS NOTHING ABOUT NEW BEHAVIOR AND IS NOT ON THE CARRY-FORWARD LIST.** It prints
/// what the layout does at unit 354's nine sizes and then down a sweep at width 1100 from the size
/// Hamlet opens at to 280 px below its own <c>MinHeight</c>. The rule it is measuring against is
/// unit 356's, written into <c>src/Hamlet.App/Views/MainWindow.axaml</c> as a comment above
/// <c>TopRow</c>: *the working panels give up height first, then the top row, and the send area is
/// never the thing that leaves.* A comment is not a test, and nothing in the tree measured the order
/// height is surrendered in or the window below 900 x 620 at all.</para>
/// <para>**THE THREE NAMES BELOW 620 ARE BELOW <c>MinHeight</c> ON PURPOSE.** This is a headless
/// window; <c>MinHeight</c> is honored by a window manager, not by the layout pass, so a headless
/// window can be made shorter than the application would let a person drag it. Whether that holds is
/// itself part of what this trace reports - the sweep prints the height it asked for beside the
/// height the window actually took.</para>
/// <para>**COMPUTED, NOT SEEN** (<c>SHACK_FACTS.md</c> FACT-004). Every number here is a
/// <c>Bounds</c> rectangle read off a realized headless window translated into the window's own
/// frame. Nothing looks at a pixel and nothing here is evidence about the radio. Nothing is pressed
/// (CLAUDE.md §0.2) and nothing is opened.</para>
/// </remarks>
public sealed class Unit372TraceTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where every box is printed.</param>
    public Unit372TraceTests(ITestOutputHelper output) => _output = output;

    /// <summary>Unit 354's nine sizes, as <c>TheStopIsAlwaysOnScreenTests</c> lists them.</summary>
    private static readonly (double Width, double Height)[] NineSizes =
    {
        (1920, 1040), (900, 620), (1100, 780), (1280, 720), (1366, 728),
        (1536, 824), (1400, 1040), (1920, 1017), (2560, 1400),
    };

    /// <summary>
    /// The descending sweep at width 1100: the size Hamlet opens at, then down in 40 px steps
    /// through <c>MinHeight</c> and 120 px past it.
    /// </summary>
    private static readonly double[] Sweep = { 780, 740, 700, 660, 620, 580, 540, 500 };

    /// <summary>The controls the send area is made of, plus the row that gives its height up second.</summary>
    private static readonly string[] Watched =
    {
        "TopRow", "DigitalSendReserved", "ModeTabs", "DigitalSendCqButton",
        "DigitalTransmitDriveNote", "DigitalStopButton",
    };

    /// <summary>
    /// **What the window does at unit 354's nine sizes and then all the way down a sweep at
    /// width 1100** - every watched box, every working panel, and whether each is whole on the window.
    /// </summary>
    [AvaloniaFact]
    public void Unit372TraceTheWindowAsItShrinks()
    {
        _output.WriteLine("PART 1 - UNIT 354'S NINE SIZES, ON THE PINNED-FACTS WINDOW");
        _output.WriteLine("");

        foreach (var (width, height) in NineSizes)
        {
            Read(width, height, plain: false);
        }

        _output.WriteLine("");
        _output.WriteLine("PART 2 - THE DESCENDING SWEEP AT WIDTH 1100, ON THE PINNED-FACTS WINDOW");
        _output.WriteLine("  780 is the size Hamlet opens at; 620 is MinHeight; 580, 540 and 500 are below it.");
        _output.WriteLine("");

        var pinned = new List<string>();

        foreach (var height in Sweep)
        {
            pinned.Add(Read(1100, height, plain: false));
        }

        // **AND THE SAME SWEEP ON THE WINDOW THAT HAS SOMETHING IN ITS PANELS.**
        // `TheTopRowTests.Realized` declares no decoded rows and no card, so its panels have
        // nothing to scroll and every scroller in them measures 0 by 0 however tall the panel is.
        // 1.3 is a question about content that has to stay reachable, so it needs the window that
        // has content: `TheWorkingPanelsTests.Realized` puts the longest line the decoded list has
        // to hold on the left and a card on the right.
        _output.WriteLine("");
        _output.WriteLine("PART 3 - THE SAME SWEEP ON THE WINDOW WITH CONTENT IN ITS PANELS");
        _output.WriteLine("");

        var plain = new List<string>();

        foreach (var height in Sweep)
        {
            plain.Add(Read(1100, height, plain: true));
        }

        _output.WriteLine("");
        _output.WriteLine("THE TABLE - one row per height in the sweep");
        _output.WriteLine("");

        Table("the pinned-facts window", pinned);
        _output.WriteLine("");
        Table("the window with content in its panels", plain);
    }

    /// <summary>Prints one sweep's rows under one heading.</summary>
    private void Table(string which, List<string> rows)
    {
        _output.WriteLine(which + ":");
        _output.WriteLine(
            "asked".PadLeft(6) + "  " + "drawn".PadLeft(6) + "  " + "TopRow".PadLeft(7) + "  "
            + "panelRow".PadLeft(8) + "  " + "x3".PadLeft(7) + "  " + "sendArea".PadLeft(8)
            + "  five controls whole on the window");

        foreach (var row in rows)
        {
            _output.WriteLine(row);
        }
    }

    /// <summary>
    /// Realizes one window, prints every box on it, and returns its one-line row for the table.
    /// </summary>
    /// <param name="width">How wide the window is.</param>
    /// <param name="height">How tall the window is asked to be.</param>
    /// <param name="plain">
    /// True for <c>TheWorkingPanelsTests.Realized</c>, which carries decoded rows and a card;
    /// false for <c>TheTopRowTests.Realized</c>, which carries the pinned facts and neither.
    /// </param>
    private string Read(double width, double height, bool plain)
    {
        var window = plain
            ? TheWorkingPanelsTests.Realized(width, height, null)
            : TheTopRowTests.Realized(width, height, null, null);

        try
        {
            Pump(window);

            var bounds = window.Bounds;
            var label = Px(width) + " x " + Px(height);

            _output.WriteLine(
                label + " (" + (plain ? "content" : "pinned facts") + "): the window asked for "
                + Px(width) + " x " + Px(height) + " and drew " + Box(bounds));

            var boxes = new Dictionary<string, Rect>(StringComparer.Ordinal);
            var whole = new Dictionary<string, bool>(StringComparer.Ordinal);

            foreach (var name in Watched)
            {
                var control = TheTopRowTests.Named<Control>(window, name);
                var at = TheTopRowTests.RectIn(control, window);

                boxes[name] = at;
                whole[name] = IsWhole(at, bounds);

                _output.WriteLine(
                    "    " + name.PadRight(26) + Box(at)
                    + "  visible " + control.IsEffectivelyVisible
                    + "  whole " + whole[name]);
            }

            // **THE THREE PANELS SHARE ONE TOP AND ONE BOTTOM** - they sit side by side, so the
            // row's height is any one of them and the total is that times three. Both are printed:
            // the row height is what the layout gave them, and the total is what the instruction
            // asks the table to carry.
            var panels = TheWorkingPanelsTests.Panels(window);
            var panelRow = panels[0].Rect.Height;
            var panelTotal = panels.Sum(p => p.Rect.Height);

            foreach (var (name, rect) in panels)
            {
                var scrollers = Scrollers(window, name);

                _output.WriteLine(
                    "    panel " + name.PadRight(20) + Box(rect)
                    + "  whole " + IsWhole(rect, bounds)
                    + "  scrollers inside it: " + (scrollers.Count == 0 ? "none" : scrollers.Count.ToString(CultureInfo.InvariantCulture)));

                foreach (var scroll in scrollers)
                {
                    _output.WriteLine(
                        "        viewport " + Px(scroll.Viewport.Height)
                        + " extent " + Px(scroll.Extent.Height)
                        + " scrollable " + Px(Math.Max(0, scroll.Extent.Height - scroll.Viewport.Height))
                        + " at " + Box(TheTopRowTests.RectIn(scroll, window)));
                }
            }

            _output.WriteLine(
                "    the panel row is " + Px(panelRow) + " px tall; the three together total "
                + Px(panelTotal) + " px");
            _output.WriteLine("");

            // **THE FIVE CONTROLS THE RULE IS ABOUT** - the send area and Stop, which R34 says never
            // leave the window, whatever the panels and the top row do above them.
            var five = new[]
            {
                "DigitalSendCqButton", "ModeTabs", "DigitalSendReserved",
                "DigitalTransmitDriveNote", "DigitalStopButton",
            };

            var missing = five.Where(n => !whole[n]).ToList();

            return Px(height).PadLeft(6) + "  " + Px(bounds.Height).PadLeft(6) + "  "
                + Px(boxes["TopRow"].Height).PadLeft(7) + "  "
                + Px(panelRow).PadLeft(8) + "  " + Px(panelTotal).PadLeft(7) + "  "
                + Px(boxes["DigitalSendReserved"].Height).PadLeft(8) + "  "
                + (missing.Count == 0 ? "all five whole" : "OFF THE WINDOW: " + string.Join(", ", missing));
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// **Every <c>ScrollViewer</c> realized inside one working panel**, each with its own viewport
    /// and extent, rather than one picked out by a rule this trace would then be asserting.
    /// </summary>
    /// <remarks>
    /// A <c>CollapsiblePanel</c> can hold more than one, and which of them is *the* panel scroller
    /// is 1.3's question rather than the trace's. Naming one here by guessing at the first
    /// descendant reported a 0 px viewport inside an 827 px panel, which was a fact about the
    /// finder and not about the layout. All of them are printed and the reader can count.
    /// </remarks>
    private static List<ScrollViewer> Scrollers(Window window, string label)
        => TheTopRowTests.Named<Control>(window, PanelName(label))
            .GetVisualDescendants().OfType<ScrollViewer>().ToList();

    /// <summary>The <c>x:Name</c> behind each of <c>TheWorkingPanelsTests.Panels</c>' three labels.</summary>
    private static string PanelName(string label)
        => label switch
        {
            "waterfall" => "DigitalWaterfallPanel",
            "decoded" => "DigitalDecodedPanel",
            _ => "DigitalMinePanel",
        };

    /// <summary>Whether a box is drawn, and drawn inside the window's own bounds.</summary>
    private static bool IsWhole(Rect at, Rect bounds)
        => at.Width > 0 && at.Height > 0
            && at.Left >= -0.5 && at.Top >= -0.5
            && at.Right <= bounds.Width + 0.5
            && at.Bottom <= bounds.Height + 0.5;

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
