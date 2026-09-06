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
/// side, and the room under the waterfall is kept for Send.
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THIS WOULD HAVE CAUGHT.** Tim ruled this layout during
/// the phase before unit 251 and it never reached an instruction, so for a whole
/// phase the two panels stayed stacked in one vertical scroller: reading a slot
/// meant scrolling the picture of it off the screen. A ruling with no test behind
/// it is a ruling the next layout edit undoes without anybody noticing, which is
/// exactly how it was lost the first time.</para>
/// <para>**IT ASSERTS ARRANGED GEOMETRY AND NOT MARKUP.** A test over the axaml
/// as text would pass on `ColumnDefinitions="*,*"` while a stray `Margin`, an
/// `HorizontalAlignment` or a nested panel made the two panels different widths
/// on screen. What the operator has is the arranged rectangle, so that is what is
/// measured.</para>
/// <para>**THE RESERVED REGION IS ASSERTED BY NAME.** `DigitalSendReserved` is
/// the whole point of it: transmit drops into a region that already exists, and
/// the waterfall above it never moves a second time. A gap that happens to be
/// there has no name and cannot be asserted.</para>
/// </remarks>
public sealed class TheDigitalTabIsTwoColumnsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the test.</summary>
    /// <param name="output">Where the arranged rectangles are printed.</param>
    public TheDigitalTabIsTwoColumnsTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// Waterfall left, decoded text right, the same width, and the reserved Send
    /// region beneath the waterfall.
    /// </summary>
    [AvaloniaFact]
    public void TheTwoPanelsAreEqualColumnsWithSendReservedBeneathTheWaterfall()
    {
        var model = new MainWindowViewModel(new AppSettings(), null)
        {
            // **THE DIGITAL WORKSPACE IS COLLAPSED UNLESS THIS TAB IS THE ONE
            // SHOWING**, so without it nothing under it is arranged and the test
            // would measure a tree of zero-sized rectangles and pass by finding
            // nothing to disagree.
            OperatingMode = "Digital",

            // Expand state is loaded from settings in the constructor, so both
            // are set here rather than relied on. A collapsed panel is
            // deliberately allowed to be header-height (see the theme's
            // `:collapsed` style), which would make the height comparison
            // meaningless.
            DigitalWaterfallExpanded = true,
            DigitalDecodedExpanded = true,
        };

        // **BIG ENOUGH THAT THE TAB REGION IS NOT THE CONSTRAINT.** The headless
        // window defaults to a size in which the workspace under the band strip,
        // the mode strip and the tabs is about 250 pixels tall - less than the
        // waterfall's own 180 plus the region under it. At that size the left
        // column scrolls, which is the correct behaviour and the wrong question:
        // this test is about where the two columns sit when there is room, which
        // is every window the operator actually uses.
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

        var waterfall = Named<CollapsiblePanel>(window, "DigitalWaterfallPanel");
        var decoded = Named<CollapsiblePanel>(window, "DigitalDecodedPanel");

        // **BY NAME, BECAUSE THE NAME IS THE CONTRACT.** The transmit phase is
        // told to drop into `DigitalSendReserved`, so a region that exists under
        // some other name is not the thing that was reserved.
        var reserved = Named<Border>(window, "DigitalSendReserved");

        var waterfallRect = RectIn(window, waterfall);
        var decodedRect = RectIn(window, decoded);
        var reservedRect = RectIn(window, reserved);

        _output.WriteLine("waterfall : " + Describe(waterfallRect));
        _output.WriteLine("decoded   : " + Describe(decodedRect));
        _output.WriteLine("reserved  : " + Describe(reservedRect));

        Assert.True(
            waterfallRect.Width > 0 && decodedRect.Width > 0,
            "one of the panels arranged to no width at all, so nothing was "
            + "compared: " + Describe(waterfallRect) + " and "
            + Describe(decodedRect));

        // **EQUAL WIDTH, WHICH IS THE RULING.** Half the available width each.
        // Half a pixel of tolerance, the same as the decoded-columns test uses,
        // because an odd number of available pixels has to go somewhere.
        Assert.True(
            Math.Abs(waterfallRect.Width - decodedRect.Width) < 0.5,
            "the waterfall is " + waterfallRect.Width.ToString("0.##")
            + " wide and the decoded panel is "
            + decodedRect.Width.ToString("0.##")
            + " - they are not the two halves of the tab");

        // **SIDE BY SIDE, AND THE WATERFALL IS THE LEFT ONE.** Ruled: waterfall
        // left, decoded text right. Stacked panels would satisfy an equal-width
        // check on their own, which is what this rules out.
        Assert.True(
            decodedRect.X >= waterfallRect.Right,
            "the decoded panel starts at x=" + decodedRect.X.ToString("0.##")
            + " and the waterfall ends at x="
            + waterfallRect.Right.ToString("0.##")
            + " - they overlap or the decoded panel is the left-hand one");

        // **THE SAME VERTICAL RUN.** Both columns start level; the decoded panel
        // fills the run the waterfall and the reserved region share.
        Assert.True(
            Math.Abs(decodedRect.Y - waterfallRect.Y) < 0.5,
            "the two columns start at y=" + waterfallRect.Y.ToString("0.##")
            + " and y=" + decodedRect.Y.ToString("0.##")
            + " - they are not the same vertical run");

        Assert.True(
            decodedRect.Bottom >= reservedRect.Bottom - 0.5,
            "the decoded panel ends at y=" + decodedRect.Bottom.ToString("0.##")
            + " and the left column ends at y="
            + reservedRect.Bottom.ToString("0.##")
            + " - the decoded panel does not fill the same vertical run");

        // **BENEATH THE WATERFALL, AND IN THE WATERFALL'S OWN COLUMN.** Below it
        // vertically and lined up with it horizontally: a region below the
        // waterfall but under the decoded panel would not be the space Tim
        // reserved.
        Assert.True(
            reservedRect.Y >= waterfallRect.Bottom - 0.5,
            "the reserved region starts at y=" + reservedRect.Y.ToString("0.##")
            + " and the waterfall ends at y="
            + waterfallRect.Bottom.ToString("0.##")
            + " - it is not beneath the waterfall");

        Assert.True(
            Math.Abs(reservedRect.X - waterfallRect.X) < 0.5
            && Math.Abs(reservedRect.Width - waterfallRect.Width) < 0.5,
            "the reserved region is " + Describe(reservedRect)
            + " and the waterfall is " + Describe(waterfallRect)
            + " - it is not in the waterfall's own column");

        Assert.True(
            reservedRect.Height > 0,
            "the reserved region arranged to no height, so nothing is actually "
            + "being kept for Send");

        // **AND IT SAYS TRANSMIT IS NOT BUILT.** An empty bordered box where a
        // Send panel will go reads as a Send panel that is broken (§0.0), so the
        // region carries a line saying what it is. Asserted on the control's own
        // text rather than on a phrase, so wording can be improved without this
        // test having an opinion about prose.
        var line = Named<TextBlock>(window, "DigitalSendReservedLine");

        Assert.False(
            string.IsNullOrWhiteSpace(line.Text),
            "the reserved region says nothing, so it reads as a broken panel "
            + "rather than as space being kept");

        // **NO CONTROL IN IT, LIVE OR GREYED** (§0.5.1, HM-DEC-087). A greyed
        // button claims a feature exists and is unavailable; transmit does not
        // exist at all, and this region must not imply otherwise.
        Assert.Empty(reserved.GetVisualDescendants().OfType<Button>());

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
    /// **TRANSLATED, BECAUSE THE THREE CONTROLS HAVE DIFFERENT PARENTS.**
    /// `Bounds` is relative to the parent, and the waterfall panel, the reserved
    /// region and the decoded panel do not share one - comparing raw `Bounds`
    /// across them would compare numbers measured from different origins and
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
