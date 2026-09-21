using System;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// **Work instruction 376 task 1: what the top band is made of, before one pixel moves.**
/// </summary>
/// <remarks>
/// <para>**THE BAND IS NOT WHAT `TheTopRowTests.Measured` MEASURES.** That record takes two
/// rectangles - the neighborhood card and the rig face - and spans them. Criterion 6.1 names
/// *pills, neighborhood strip, green zone, rig display*, and the pills are a row of their own
/// above both. A criterion met against the narrower thing would not be the criterion, so work
/// instruction 376 section 6's first ruling defines the band as **the top of the band-pills row
/// down to the bottom of whichever of the neighborhood card, the rig face and
/// <c>RigDriveAndPower</c> ends lowest**, measured in the window's own frame by
/// <see cref="Band(Window)"/> - **one helper, used everywhere in this unit**.</para>
/// <para>**BOTH READINGS, ALWAYS** (the same ruling): the band with the pills, which is what the
/// 180 is met on, and the band without them, which is what the existing `TopRowTarget` names have
/// always measured, printed beside it so the two can be told apart.</para>
/// <para>**THE ELEMENT THE INSTRUCTION COULD NOT NAME.** `Grid.Row="0"` of the strip grid at
/// `MainWindow.axaml` line 2896 is an **unnamed `ItemsControl` at line 3274** -
/// `ItemsSource="{Binding Bands}"`, `VerticalAlignment="Top"`, `ClipToBounds="False"`,
/// `Margin="0,14,0,10"`, its item template the `Button Classes="hm-band"` cards with the best-bet
/// badge drawn over them. It is found here the way <c>WithTheBestBetPinned</c> already finds it -
/// by the `hm-band` class of its buttons - and not by a name this unit added.</para>
/// <para>**COMPUTED, NOT SEEN** (§0.0, FACT-004). Every number is read off a realized headless
/// window. Nothing here is evidence about the radio: no port is opened and nothing is keyed.</para>
/// <para>**THIS TRACE ASSERTS NOTHING ABOUT THE LAYOUT**, in the shape of unit 353's and unit
/// 374's traces. It is the before that task 3's arithmetic is computed against.</para>
/// </remarks>
public sealed class Unit376TheTopBandTests
{
    /// <summary>Criterion 6.1's number: the band is at or under 180 px at 1920 and at 1400.</summary>
    public const double BandTarget = 180;

    /// <summary>The three sizes this unit measures: 6.1's two, and the size Hamlet opens at.</summary>
    /// <remarks>**1100 x 780 IS REPORTED AND NOT ASSERTED** (section 6's first ruling, point 3).</remarks>
    public static readonly (double Width, double Height)[] Sizes =
    {
        (1920, 1040), (1400, 1040), (1100, 780),
    };

    /// <summary>Every mode the strip can be in. The band that must be at or under 180 is the tallest.</summary>
    public static readonly string[] Modes = { "FT8", "PSK31", "Olivia" };

    /// <summary>Unit 354's nine sizes, as <c>TheStopIsAlwaysOnScreenTests</c> lists them.</summary>
    public static readonly (double Width, double Height)[] Unit354Sizes =
    {
        (1920, 1040), (900, 620), (1100, 780), (1280, 720), (1366, 728),
        (1536, 824), (1400, 1040), (1920, 1017), (2560, 1400),
    };

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where every rectangle is printed.</param>
    public Unit376TheTopBandTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **The band, band by band, at three widths and on three modes - and the panel row at unit
    /// 354's nine sizes.** Asserts nothing.
    /// </summary>
    [AvaloniaFact]
    public void Unit376TraceWhatTheBandIsMadeOfBeforeOnePixelMoves()
    {
        _output.WriteLine("THE BAND ROW IS THE UNNAMED ItemsControl AT MainWindow.axaml 3274:");
        _output.WriteLine("  Grid.Row=0 of the strip grid at 2896, ItemsSource={Binding Bands}, VerticalAlignment=Top,");
        _output.WriteLine("  ClipToBounds=False, Margin=0,14,0,10, items are Button Classes=hm-band with the badge over them.");
        _output.WriteLine("");

        foreach (var (width, height) in Sizes)
        {
            foreach (var mode in Modes)
            {
                var window = TheTopRowTests.Realized(width, height, null, null);
                var model = (MainWindowViewModel)window.DataContext!;

                try
                {
                    model.ChosenDigitalMode = mode;
                    Pump(window);

                    Print(width, height, mode, window);
                }
                finally
                {
                    model.ChosenDigitalMode = "FT8";
                    window.Close();
                }
            }
        }

        _output.WriteLine("THE PANEL ROW AND THE BAND AT UNIT 354'S NINE SIZES, on the pinned-facts window, FT8");
        _output.WriteLine("  (unit 374 recorded the panel row as 452, 73, 73, 90, 90, 230, 426, 429, 829)");

        foreach (var (width, height) in Unit354Sizes)
        {
            var window = TheTopRowTests.Realized(width, height, null, null);

            try
            {
                Pump(window);

                var band = Band(window);

                _output.WriteLine(
                    "  " + Size(width, height).PadRight(12)
                    + " panel row " + Px(PanelRow(window)).PadLeft(7)
                    + " | band with pills " + Px(band.WithPills).PadLeft(7)
                    + " | without " + Px(band.WithoutPills).PadLeft(7)
                    + " | TopRow drawn " + Px(TopRow(window).Height).PadLeft(7)
                    + " | " + band.Governs);
            }
            finally
            {
                window.Close();
            }
        }

        _output.WriteLine("");
    }

    // ------------------------------------------------------------------------------------

    /// <summary>
    /// **The band, per work instruction 376 section 6's first ruling**, in the window's own frame.
    /// </summary>
    /// <param name="Pills">The band-pills row - the unnamed ItemsControl at MainWindow.axaml 3274.</param>
    /// <param name="Card">The neighborhood card, as <c>TheTopRowTests.Card</c> finds it.</param>
    /// <param name="Rig">The rig face's bordered box, as <c>TheTopRowTests.RigPanel</c> finds it.</param>
    /// <param name="Drive">`RigDriveAndPower` - the drive and the power offer.</param>
    /// <param name="CardWants">What the card's content asks for: the scroller's extent.</param>
    /// <param name="RigWants">What the rig column asks for: the rig border's desired height.</param>
    public sealed record TopBand(
        Rect Pills, Rect Card, Rect Rig, Rect Drive, double CardWants, double RigWants)
    {
        /// <summary>The top of the band the criterion names: the pills are in it.</summary>
        public double TopWithPills => Math.Min(Pills.Top, TopWithoutPills);

        /// <summary>The top of the band the existing tests measure: the two halves only.</summary>
        public double TopWithoutPills => Math.Min(Card.Top, Rig.Top);

        /// <summary>The bottom of whichever of the three ends lowest.</summary>
        public double Bottom => Math.Max(Card.Bottom, Math.Max(Rig.Bottom, Drive.Bottom));

        /// <summary>The band with the pills - what 6.1 is met on.</summary>
        public double WithPills => Bottom - TopWithPills;

        /// <summary>The band without the pills - what `TopRowTarget` has always measured.</summary>
        public double WithoutPills => Bottom - TopWithoutPills;

        /// <summary>Which of the two columns governs the band's height, and by how much.</summary>
        /// <remarks>
        /// **THE QUESTION TASK 1 EXISTS FOR.** The row is `Auto` and both halves stretch to it, so
        /// shrinking the shorter column buys nothing at all.
        /// </remarks>
        public string Governs
            => (CardWants >= RigWants ? "CARD governs" : "RIG governs")
                + " (card wants " + Px(CardWants) + ", rig wants " + Px(RigWants)
                + ", margin " + Px(Math.Abs(CardWants - RigWants)) + ")";
    }

    /// <summary>**The one helper.** Measures the band on a realized window, both readings.</summary>
    /// <param name="window">A realized, settled window.</param>
    /// <returns>The band and the two columns' appetites.</returns>
    public static TopBand Band(Window window)
    {
        var scroller = TheTopRowTests.Named<ScrollViewer>(window, "TopRowCardScroller");
        var rig = TheTopRowTests.RigPanel(window);

        return new TopBand(
            TheTopRowTests.RectIn(Pills(window), window),
            TheTopRowTests.RectIn(TheTopRowTests.Card(window), window),
            TheTopRowTests.RectIn(rig, window),
            TheTopRowTests.RectIn(TheTopRowTests.Named<Control>(window, "RigDriveAndPower"), window),
            scroller.Extent.Height,
            rig.DesiredSize.Height);
    }

    /// <summary>
    /// The band-pills row: the `ItemsControl` whose buttons wear the `hm-band` class.
    /// </summary>
    /// <remarks>
    /// **FOUND BY WHAT IT IS, NOT BY A NAME THIS UNIT ADDED** - the pattern
    /// <c>TheTopRowTests.WithTheBestBetPinned</c> has used since work instruction 351.
    /// </remarks>
    /// <param name="window">A realized window.</param>
    /// <returns>The row.</returns>
    public static ItemsControl Pills(Window window)
        => window.GetVisualDescendants().OfType<ItemsControl>()
            .First(i => i.GetVisualDescendants().OfType<Button>().Any(b => b.Classes.Contains("hm-band")));

    /// <summary>`TopRow` itself - the capped grid holding the two columns.</summary>
    /// <param name="window">A realized window.</param>
    /// <returns>Its rectangle in the window's frame.</returns>
    public static Rect TopRow(Window window)
        => TheTopRowTests.RectIn(TheTopRowTests.Named<Control>(window, "TopRow"), window);

    /// <summary>The working panels' shared row height - the waterfall's, as unit 374 read it.</summary>
    /// <param name="window">A realized window.</param>
    /// <returns>The row height.</returns>
    public static double PanelRow(Window window)
        => TheWorkingPanelsTests.Panels(window)[0].Rect.Height;

    /// <summary>Runs the dispatcher and lays the window out.</summary>
    /// <param name="window">The window.</param>
    public static void Pump(Window window)
    {
        for (var i = 0; i < 4; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    // ------------------------------------------------------------------------------------

    private void Print(double width, double height, string mode, Window window)
    {
        var band = Band(window);
        var scroller = TheTopRowTests.Named<ScrollViewer>(window, "TopRowCardScroller");

        _output.WriteLine("WINDOW " + Size(width, height) + "  MODE " + mode);
        _output.WriteLine(
            "  BAND WITH PILLS   : y " + Px(band.TopWithPills) + " to " + Px(band.Bottom)
            + " = " + Px(band.WithPills) + " px, against " + Px(BandTarget) + " - "
            + Over(band.WithPills));
        _output.WriteLine(
            "  BAND WITHOUT PILLS: y " + Px(band.TopWithoutPills) + " to " + Px(band.Bottom)
            + " = " + Px(band.WithoutPills) + " px, against " + Px(BandTarget) + " - "
            + Over(band.WithoutPills));
        _output.WriteLine("  GOVERNING COLUMN  : " + band.Governs);
        _output.WriteLine(
            "  TopRow (capped 300): " + Box(TopRow(window))
            + " | card scroller viewport " + Px(scroller.Viewport.Height)
            + ", extent " + Px(scroller.Extent.Height)
            + (scroller.Extent.Height > scroller.Viewport.Height + 0.5 ? " - THE CARD IS SCROLLING INSIDE THE CAP" : ""));
        _output.WriteLine("  band row (pills)  : " + Box(band.Pills));
        _output.WriteLine("  neighborhood card : " + Box(band.Card));

        Line(window, "    strip", Typed<NeighborhoodMapControl>(window));
        Line(window, "    legend", Typed<MapLegendControl>(window));
        Line(window, "    GreenZoneBlock", Named(window, "GreenZoneBlock"));
        Line(window, "      GreenZoneBand", Named(window, "GreenZoneBand"));
        Line(window, "      GreenZoneFrequency", Named(window, "GreenZoneFrequency"));
        Line(window, "      GreenZoneModeLine", Named(window, "GreenZoneModeLine"));
        Line(window, "      GreenZoneLicenseLine", Named(window, "GreenZoneLicenseLine"));
        Line(window, "      GreenZoneRuleOfThumb", Named(window, "GreenZoneRuleOfThumb"));
        Line(window, "      GreenZoneBestBetRow", Named(window, "GreenZoneBestBetRow"));
        Line(window, "      GreenZoneBestBet", Named(window, "GreenZoneBestBet"));
        Line(window, "      GreenZoneHeardGrid", Named(window, "GreenZoneHeardGrid"));
        Line(window, "      GreenZoneSparkline", Named(window, "GreenZoneSparkline"));
        Line(window, "      GreenZoneHeard", Named(window, "GreenZoneHeard"));
        Line(window, "      GreenZoneHeardWindow", Named(window, "GreenZoneHeardWindow"));
        Line(window, "      GreenZoneStrayedLine", Named(window, "GreenZoneStrayedLine"));
        Line(window, "    GreenZoneMap", Named(window, "GreenZoneMap"));
        Line(window, "      GreenZoneGrayLine", Named(window, "GreenZoneGrayLine"));
        Line(window, "      GreenZoneClockCaption", Named(window, "GreenZoneClockCaption"));

        _output.WriteLine("  rig face Border   : " + Box(band.Rig) + ", wants " + Px(band.RigWants));

        Line(window, "    RigDisplayControl", Typed<RigDisplayControl>(window));

        _output.WriteLine("  RigDriveAndPower  : " + Box(band.Drive));

        Line(window, "    DigitalTransmitDriveBox", Named(window, "DigitalTransmitDriveBox"));
        Line(window, "    DigitalTransmitDriveNote", Named(window, "DigitalTransmitDriveNote"));
        Line(window, "    DigitalTransmitDriveTip", Named(window, "DigitalTransmitDriveTip"));
        Line(window, "    DigitalPsk31PowerLine", Named(window, "DigitalPsk31PowerLine"));

        var panels = TheWorkingPanelsTests.Panels(window);

        _output.WriteLine(
            "  PANEL ROW         : " + Px(panels[0].Rect.Height) + " px ("
            + string.Join(", ", panels.Select(p => p.Name + " " + Px(p.Rect.Height))) + ")");
        _output.WriteLine("");
    }

    private void Line(Window window, string label, Control? control)
    {
        if (control is null)
        {
            _output.WriteLine(label.PadRight(30) + ": absent from the visual tree");

            return;
        }

        _output.WriteLine(
            label.PadRight(30) + ": " + Box(TheTopRowTests.RectIn(control, window))
            + (control.IsEffectivelyVisible ? "" : " - NOT DRAWN"));
    }

    private static Control? Named(Window window, string name)
        => window.GetVisualDescendants().OfType<Control>().FirstOrDefault(c => c.Name == name);

    private static Control? Typed<T>(Window window)
        where T : Control
        => window.GetVisualDescendants().OfType<T>().FirstOrDefault();

    private static string Over(double band)
        => band <= BandTarget
            ? Px(BandTarget - band) + " px under"
            : Px(band - BandTarget) + " px over";

    private static string Size(double width, double height)
        => Px(width) + " x " + Px(height);

    private static string Px(double value)
        => value.ToString("0.#", CultureInfo.InvariantCulture);

    private static string Box(Rect r)
        => Px(r.X) + "," + Px(r.Y) + " " + Px(r.Width) + " x " + Px(r.Height);
}
