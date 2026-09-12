using Avalonia;
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
/// Work instruction 251, task 2: the waterfall and the decoded text sit side by
/// side, and the send area has a named place of its own.
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THIS WOULD HAVE CAUGHT.** Tim ruled this layout during
/// the phase before unit 251 and it never reached an instruction, so for a whole
/// phase the two panels stayed stacked in one vertical scroller: reading a slot
/// meant scrolling the picture of it off the screen. A ruling with no test behind
/// it is a ruling the next layout edit undoes without anybody noticing, which is
/// exactly how it was lost the first time.</para>
/// <para>**REWRITTEN UNDER R12 IN WORK INSTRUCTION 337.** Tim, 2026-09-12, on
/// `assets/main-screen-mockup.png` (R26): waterfall, decoded text and For You are
/// three panels of one height, full to the status bar. The two equal halves this
/// test asserted, and the send area under the waterfall, are what that ruling
/// replaced; what it did not touch - the waterfall is the left panel, the lists
/// are beside it, and the send area is a named region in the waterfall's own
/// column - is still asserted. The send area is above the waterfall now, so the
/// waterfall fills the height the lists fill.</para>
/// <para>**IT ASSERTS ARRANGED GEOMETRY AND NOT MARKUP.** A test over the axaml
/// as text would pass on the column definitions while a stray `Margin`, an
/// `HorizontalAlignment` or a nested panel moved a panel on screen. What the
/// operator has is the arranged rectangle, so that is what is measured.</para>
/// <para>**THE SEND AREA IS ASSERTED BY NAME.** `DigitalSendReserved` is where
/// the transmit phase dropped its controls, and a region that exists under some
/// other name is not the thing that was reserved.</para>
/// </remarks>
public sealed class TheDigitalTabIsTwoColumnsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the test.</summary>
    /// <param name="output">Where the arranged rectangles are printed.</param>
    public TheDigitalTabIsTwoColumnsTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// Waterfall left, then decoded text, then For You, one top and one bottom,
    /// and the send area above the waterfall in its own column.
    /// </summary>
    [AvaloniaFact]
    public void TheThreePanelsAreColumnsWithTheSendAreaAboveTheWaterfall()
    {
        var model = new MainWindowViewModel(new AppSettings(), null)
        {
            // **THE DIGITAL WORKSPACE IS COLLAPSED UNLESS THIS TAB IS THE ONE
            // SHOWING**, so without it nothing under it is arranged and the test
            // would measure a tree of zero-sized rectangles and pass by finding
            // nothing to disagree.
            OperatingMode = "Digital",

            // Expand state is loaded from settings in the constructor, so every
            // panel is set here rather than relied on. A collapsed panel is
            // deliberately allowed to be header-height (see the theme's
            // `:collapsed` style), which would make the height comparison
            // meaningless.
            DigitalWaterfallExpanded = true,
            DigitalDecodedExpanded = true,
            DigitalMineExpanded = true,
        };

        var window = new MainWindow
        {
            DataContext = model,
            Width = 1400,
            Height = 1200,
        };

        window.Show();

        // A layout pass as well as the jobs: running the dispatcher realizes
        // controls, it does not necessarily measure and arrange them, and an
        // arranged rectangle is the only thing this test is about.
        for (var i = 0; i < 5; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }

        var waterfall = RectIn(window, Named<CollapsiblePanel>(window, "DigitalWaterfallPanel"));
        var decoded = RectIn(window, Named<CollapsiblePanel>(window, "DigitalDecodedPanel"));
        var mine = RectIn(window, Named<CollapsiblePanel>(window, "DigitalMinePanel"));
        var reserved = RectIn(window, Named<Border>(window, "DigitalSendReserved"));

        _output.WriteLine("waterfall : " + Describe(waterfall));
        _output.WriteLine("decoded   : " + Describe(decoded));
        _output.WriteLine("For you   : " + Describe(mine));
        _output.WriteLine("reserved  : " + Describe(reserved));

        Assert.True(
            waterfall.Width > 0 && decoded.Width > 0 && mine.Width > 0,
            "one of the panels arranged to no width at all, so nothing was compared: "
            + Describe(waterfall) + ", " + Describe(decoded) + " and " + Describe(mine));

        // **SIDE BY SIDE, IN THE RULED ORDER.** Stacked panels would satisfy the
        // height checks below on their own, which is what this rules out.
        Assert.True(
            decoded.X >= waterfall.Right,
            "the decoded panel starts at x=" + decoded.X.ToString("0.##")
            + " and the waterfall ends at x=" + waterfall.Right.ToString("0.##"));

        Assert.True(
            mine.X >= decoded.Right,
            "For you starts at x=" + mine.X.ToString("0.##")
            + " and the decoded panel ends at x=" + decoded.Right.ToString("0.##"));

        // **ONE TOP AND ONE BOTTOM** (R26), which is what *the same vertical run*
        // always meant and now holds for all three panels rather than two columns.
        foreach (var (name, rect) in new[] { ("decoded", decoded), ("For you", mine) })
        {
            Assert.True(
                Math.Abs(rect.Y - waterfall.Y) < 0.5 && Math.Abs(rect.Bottom - waterfall.Bottom) < 0.5,
                "the " + name + " panel is " + Describe(rect) + " and the waterfall is "
                + Describe(waterfall) + " - they are not one vertical run");
        }

        // **THE DECODED PANEL IS THE NARROW ONE.** Its column is what its longest
        // line needs; the numbers are `TheWorkingPanelsTests`'.
        Assert.True(
            decoded.Width < waterfall.Width && decoded.Width < mine.Width,
            "the decoded panel is " + decoded.Width.ToString("0.##")
            + " wide against a waterfall of " + waterfall.Width.ToString("0.##")
            + " and For you of " + mine.Width.ToString("0.##"));

        // **ABOVE THE WATERFALL, AND IN THE WATERFALL'S OWN COLUMN.** A region
        // above the lists would not be the space kept for Send.
        Assert.True(
            reserved.Bottom <= waterfall.Y + 0.5,
            "the send area ends at y=" + reserved.Bottom.ToString("0.##")
            + " and the waterfall starts at y=" + waterfall.Y.ToString("0.##")
            + " - it is not above the waterfall");

        Assert.True(
            Math.Abs(reserved.X - waterfall.X) < 0.5
            && Math.Abs(reserved.Width - waterfall.Width) < 0.5,
            "the send area is " + Describe(reserved)
            + " and the waterfall is " + Describe(waterfall)
            + " - it is not in the waterfall's own column");

        Assert.True(
            reserved.Height > 0,
            "the send area arranged to no height, so nothing is actually being kept for Send");

        // **AND IT SAYS WHAT IT IS.** An empty bordered box where Send goes reads as
        // a Send panel that is broken (§0.0), so the region carries a line.
        // Asserted on the control's own text rather than on a phrase, so wording
        // can be improved without this test having an opinion about prose.
        var line = Named<TextBlock>(window, "DigitalSendReservedLine");

        Assert.False(
            string.IsNullOrWhiteSpace(line.Text),
            "the send area says nothing, so it reads as a broken panel");

        // **THE REGION HOLDS THE SEND CONTROLS** (rewritten under §R12 in work
        // instruction 331 task 1a): the send control is the named one, so the
        // region is still the place the transmit phase was told to drop into.
        Assert.Contains(
            Named<Border>(window, "DigitalSendReserved").GetVisualDescendants().OfType<Button>(),
            b => b.Name == "DigitalSendCqButton");

        window.Close();
    }

    /// <summary>
    /// Both panels keep their header, their title and their summary, and a
    /// collapsed one still carries its summary.
    /// </summary>
    /// <remarks>
    /// **HM-DEC-012 AND §0.5, CHECKED AT THE SAME TIME AS THE MOVE.** Moving a
    /// panel into a new parent is exactly when a header or a summary binding gets
    /// dropped, and a shut panel that goes silent is the prime directive broken
    /// by omission.
    /// </remarks>
    [AvaloniaFact]
    public void ACollapsedPanelInTheNewLayoutStillCarriesItsSummary()
    {
        var model = new MainWindowViewModel(new AppSettings(), null)
        {
            OperatingMode = "Digital",
            DigitalWaterfallExpanded = false,
            DigitalDecodedExpanded = false,
        };

        var window = new MainWindow { DataContext = model };

        window.Show();

        for (var i = 0; i < 5; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }

        var waterfall = Named<CollapsiblePanel>(window, "DigitalWaterfallPanel");
        var decoded = Named<CollapsiblePanel>(window, "DigitalDecodedPanel");

        _output.WriteLine("waterfall summary : " + waterfall.Summary);
        _output.WriteLine("decoded summary   : " + decoded.Summary);

        Assert.Equal("Waterfall", waterfall.Title);
        Assert.Equal("Decoded text", decoded.Title);

        Assert.False(waterfall.IsExpanded);
        Assert.False(decoded.IsExpanded);

        Assert.False(
            string.IsNullOrWhiteSpace(waterfall.Summary),
            "the collapsed waterfall panel went silent");

        Assert.False(
            string.IsNullOrWhiteSpace(decoded.Summary),
            "the collapsed decoded panel went silent");

        window.Close();
    }

    /// <summary>
    /// The decoded panel wears the decode family, and its header bar is not
    /// filled with it.
    /// </summary>
    /// <remarks>
    /// <para>**UNIT 249 FOUND THE TWO DISAGREEING.** The markup said `Lavender`
    /// while the sender field inside the panel is green, and §0.5's families are
    /// amber for tuning, blue for spectrum and **green for decode**. This panel is
    /// the decode, so the markup was the wrong one of the two.</para>
    /// <para>**AND THE BAR IS NEVER FILLED** (HM-DEC-012). The family is text
    /// colour and edge colour only; a column of filled header bars reads as
    /// stripes rather than as structure, which is the thing that ruling exists to
    /// stop and the thing a family change is most likely to undo by accident.</para>
    /// </remarks>
    [AvaloniaFact]
    public void TheDecodedPanelWearsTheDecodeFamilyAndItsBarIsNotFilled()
    {
        var model = new MainWindowViewModel(new AppSettings(), null)
        {
            OperatingMode = "Digital",
            DigitalDecodedExpanded = true,
        };

        var window = new MainWindow
        {
            DataContext = model,
            Width = 1400,
            Height = 1200,
        };

        window.Show();

        for (var i = 0; i < 5; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }

        var decoded = Named<CollapsiblePanel>(window, "DigitalDecodedPanel");

        _output.WriteLine("decoded panel family : " + decoded.Family);

        Assert.Equal(PanelFamily.Green, decoded.Family);

        // **THE HEADER BUTTON IS TRANSPARENT AT REST.** It is the control the
        // whole bar is, and a family fill would land on it.
        var header = decoded.GetVisualDescendants()
            .OfType<Button>()
            .FirstOrDefault(b => b.Name == "PART_Header");

        Assert.True(header is not null, "the panel has no header button");

        var fill = PanelPalette.Green.FillBrush;

        _output.WriteLine("header background    : " + header!.Background);
        _output.WriteLine("the family's fill    : " + fill);

        Assert.NotEqual(fill, header.Background);

        // And the title carries the family instead, which is where it belongs.
        Assert.Equal(PanelPalette.Green.TitleBrush, decoded.FamilyBrush);
    }

    /// <summary>The named control, or a failure that says which name was missing.</summary>
    private static T Named<T>(Visual root, string name)
        where T : Control
    {
        var found = root.GetVisualDescendants()
            .OfType<T>()
            .FirstOrDefault(c => c.Name == name);

        Assert.True(
            found is not null,
            "no " + typeof(T).Name + " called " + name + " is in the window");

        return found!;
    }

    /// <summary>
    /// A control's arranged rectangle in the window's own coordinates.
    /// </summary>
    /// <remarks>
    /// **TRANSLATED, BECAUSE THE CONTROLS HAVE DIFFERENT PARENTS.** `Bounds` is
    /// relative to the parent, and comparing raw `Bounds` across controls that
    /// do not share one would compare numbers measured from different origins and
    /// would pass or fail for reasons that have nothing to do with the layout.
    /// </remarks>
    private static Rect RectIn(Visual window, Control control)
    {
        var origin = control.TranslatePoint(new Point(0, 0), window);

        Assert.True(
            origin.HasValue,
            "the control " + control.Name + " is not connected to the window, "
            + "so it has no position in it");

        return new Rect(origin!.Value, control.Bounds.Size);
    }

    private static string Describe(Rect rect)
        => "x=" + rect.X.ToString("0.##")
           + " y=" + rect.Y.ToString("0.##")
           + " w=" + rect.Width.ToString("0.##")
           + " h=" + rect.Height.ToString("0.##");
}
