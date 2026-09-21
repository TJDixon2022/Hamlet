using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Xunit;
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

    /// <summary>
    /// **Task 1's before table, measured on this unit's own trace before one pixel moved.**
    /// </summary>
    /// <remarks>
    /// <para>By width: the band with the pills, the band without them, and the working panels' row,
    /// at 1040 px of window on all three modes - FT8, PSK31 and Olivia measured identically at both
    /// widths. **Criterion 6.1's *the working panels are taller by the difference* is arithmetic
    /// against these numbers** (work instruction 376 §6's first ruling, point 5) and never against
    /// a constant chosen in a test.</para>
    /// <para>**THESE ARE THIS UNIT'S OWN MEASUREMENTS**, printed by
    /// <see cref="Unit376TraceWhatTheBandIsMadeOfBeforeOnePixelMoves"/> at task 1 and quoted here:
    /// 1920 - 243, 190, 450. 1400 - 269, 216, 424.</para>
    /// </remarks>
    public static readonly (double Width, double WithPills, double WithoutPills, double PanelRow)[] Before =
    {
        (1920, 243, 190, 450),
        (1400, 269, 216, 424),
    };

    /// <summary>
    /// The panel row at unit 354's nine sizes before this unit changed anything, in the order
    /// <see cref="Unit354Sizes"/> lists them.
    /// </summary>
    /// <remarks>
    /// **MEASURED AT TASK 1 ON THE PINNED-FACTS WINDOW.** Unit 374 recorded 452, 73, 73, 90, 90,
    /// 230, 426, 429, 829 on the window with content, which is 2 px taller at every size - the
    /// card's own chrome, as <c>TheWindowGivesUpHeightInOneOrderTests</c> records. **After this
    /// unit every one of the nine must be the same or larger** (criterion 6.4).
    /// </remarks>
    public static readonly double[] BeforeAtUnit354Sizes =
    {
        450, 71, 71, 88, 88, 228, 424, 427, 827,
    };

    /// <summary>
    /// **What the band measured after R39's four moves, and what it may never again exceed.**
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS A RATCHET AND NOT THE CRITERION.** 6.1 asks for 180 px with the pills and
    /// the band reached 214: the sun map is 134 px and may not shrink (6.3), the neighborhood
    /// card's own header is 27 px, the pills at half height are 30 and the gap to the card is 6,
    /// so **197 px is the arithmetic floor of the band with the pills** before one word would have
    /// to leave the window. The band WITHOUT the pills is 178 and does meet 180. The criterion is
    /// reported as partial with these numbers, and this constant holds the ground that was taken
    /// so that the next unit cannot give it back without a red.</para>
    /// </remarks>
    public const double BandReachedWithThePills = 214;

    /// <summary>The band without the pills, after the moves - and it is inside 6.1's 180.</summary>
    public const double BandReachedWithoutThePills = 178;

    /// <summary>The working panels' row after the moves, at 1920 and at 1400 alike.</summary>
    public const double PanelRowReached = 483;

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
    /// **Criterion 6.1: the top row is one measured band, and every pixel it gave up went to the
    /// working panels** - at 1920 and at 1400, on the tallest of FT8, PSK31 and Olivia.
    /// </summary>
    /// <remarks>
    /// <para>**THE BAND IS MEASURED BOTH WAYS AND ASSERTED BOTH WAYS.** With the pills it is
    /// asserted against <see cref="BandReachedWithThePills"/>, the ground this unit took, with
    /// 6.1's 180 named in the failure message: the criterion is **not** met on that reading and is
    /// reported partial, because the sun map's 134 px may not shrink (6.3) and the card's header,
    /// the pills and the gap put the arithmetic floor at about 197. Without the pills - the reading
    /// <c>TheTopRowTests.Measured</c> has always taken - it is asserted against 6.1's 180 itself,
    /// which it meets at 178.</para>
    /// <para>**AND THE PANELS ARE TALLER BY THE DIFFERENCE**, computed against
    /// <see cref="Before"/> - task 1's own table, measured before one pixel moved - and quoted in
    /// the failure message. Not against a constant chosen here (work instruction 376 §6's first
    /// ruling, point 5).</para>
    /// </remarks>
    [AvaloniaFact]
    public void TheTopBandIsOneShortRowAndThePanelsAreTallerByTheDifference()
    {
        var misses = new List<string>();

        foreach (var (width, wasWithPills, wasWithoutPills, wasPanelRow) in Before)
        {
            foreach (var mode in Modes)
            {
                var window = TheTopRowTests.Realized(width, TheTopRowTests.WindowHeight, null, null);
                var model = (MainWindowViewModel)window.DataContext!;

                try
                {
                    model.ChosenDigitalMode = mode;
                    Pump(window);

                    var band = Band(window);
                    var panelRow = PanelRow(window);
                    var where = Px(width) + " on " + mode;

                    _output.WriteLine(
                        where + ": the band is " + Px(band.WithPills) + " px with the pills from "
                        + Px(wasWithPills) + ", " + Px(band.WithoutPills) + " without from "
                        + Px(wasWithoutPills) + ", and the panel row " + Px(panelRow) + " from "
                        + Px(wasPanelRow) + ". " + band.Governs);

                    if (band.WithPills > BandReachedWithThePills + 0.5)
                    {
                        misses.Add(
                            where + ": the band with the pills is " + Px(band.WithPills)
                            + " px where this unit brought it to " + Px(BandReachedWithThePills)
                            + " from " + Px(wasWithPills) + ", against criterion 6.1's "
                            + Px(BandTarget) + ". Height has gone back into the top row.");
                    }

                    if (band.WithoutPills > BandTarget + 0.5)
                    {
                        misses.Add(
                            where + ": the band without the pills is " + Px(band.WithoutPills)
                            + " px, against criterion 6.1's " + Px(BandTarget) + ", from "
                            + Px(wasWithoutPills) + " before.");
                    }

                    // **TALLER BY THE DIFFERENCE**, and the difference is arithmetic on the before
                    // table: whatever the band gave up, the three working panels have it.
                    var gave = wasWithPills - band.WithPills;

                    if (panelRow < wasPanelRow + gave - 0.5)
                    {
                        misses.Add(
                            where + ": the band gave up " + Px(gave) + " px - " + Px(wasWithPills)
                            + " to " + Px(band.WithPills) + " - and the panel row went "
                            + Px(wasPanelRow) + " to " + Px(panelRow) + ", which is "
                            + Px(wasPanelRow + gave - panelRow) + " px short of it.");
                    }
                }
                finally
                {
                    model.ChosenDigitalMode = "FT8";
                    window.Close();
                }
            }
        }

        Assert.True(misses.Count == 0, string.Join(Environment.NewLine, misses));
    }

    /// <summary>
    /// **Criterion 6.2: nothing left the window.** Every pill, the strip, the band, the frequency,
    /// the verdict, the license phrase, the best bet, the heard count, the drive and the power
    /// offer are drawn by name at both widths and on every mode - and the two lines that moved to
    /// a hover still say what they said, in an operator-facing string.
    /// </summary>
    /// <remarks>
    /// **THE HOVER IS THE POINT OF THIS NAME** (work instruction 376 §6's second ruling). A thing
    /// that comes off the card goes to a hover **carrying the same words**, so *hiding detail*
    /// never becomes hiding information (CLAUDE.md §0.5). The legend's sentence is built from
    /// <c>ModePalette.Legend</c> - the list the map draws its washes from - and the rule of thumb's
    /// is the static the card's own line was bound to, so neither can drift from what it replaced.
    /// </remarks>
    [AvaloniaFact]
    public void NothingLeftTheWindowAndTheTwoLinesThatMovedStillSayWhatTheySaid()
    {
        var misses = new List<string>();

        foreach (var (width, _, _, _) in Before)
        {
            foreach (var mode in Modes)
            {
                var window = TheTopRowTests.Realized(width, TheTopRowTests.WindowHeight, null, null);
                var model = (MainWindowViewModel)window.DataContext!;

                try
                {
                    model.ChosenDigitalMode = mode;
                    Pump(window);

                    var where = Px(width) + " on " + mode;
                    var card = TheTopRowTests.Card(window);

                    // **EVERY PILL.** One button per band in the model, each drawn and each with
                    // its name in it - the row is half as tall and carries what it carried.
                    var pills = Pills(window).GetVisualDescendants().OfType<Button>()
                        .Where(b => b.Classes.Contains("hm-band")).ToList();

                    if (pills.Count != model.Bands.Count || pills.Any(b => !b.IsEffectivelyVisible))
                    {
                        misses.Add(
                            where + ": the band row draws " + pills.Count + " pills of "
                            + model.Bands.Count + ", " + pills.Count(b => b.IsEffectivelyVisible)
                            + " of them drawn.");
                    }

                    foreach (var name in new[]
                    {
                        "GreenZoneBand", "GreenZoneFrequency", "GreenZoneModeLine",
                        "GreenZoneLicenseLine", "GreenZoneHeard", "GreenZoneHeardWindow",
                        "DigitalTransmitDriveBox", "DigitalTransmitDriveNote",
                        "DigitalTransmitDriveTip",
                    })
                    {
                        var control = Named(window, name);

                        if (control is null || !control.IsEffectivelyVisible)
                        {
                            misses.Add(
                                where + ": " + name
                                + (control is null ? " is not on the window at all." : " is not drawn."));
                        }
                    }

                    // **THE STRIP ITSELF**, thinner and still drawn with height to draw in.
                    var strip = window.GetVisualDescendants().OfType<NeighborhoodMapControl>()
                        .FirstOrDefault();

                    if (strip is null || !strip.IsEffectivelyVisible || strip.Bounds.Height < 24)
                    {
                        misses.Add(
                            where + ": the neighborhood strip is "
                            + (strip is null ? "absent" : Px(strip.Bounds.Height) + " px tall") + ".");
                    }

                    // **THE BEST BET AND THE POWER OFFER ARE DRAWN WHEN THERE IS ONE**, which is
                    // the rule; the fixture pins no best bet, so a flat *it is drawn* would be a
                    // test of the fixture rather than of the window.
                    foreach (var (name, expected, why) in new[]
                    {
                        ("GreenZoneBestBet", model.GreenZone.HasBestBet, "the green zone has a best bet"),
                        ("DigitalPsk31PowerLine", model.HasPsk31PowerOffer, "there is a power offer"),
                    })
                    {
                        var control = Named(window, name);

                        if (control is null || control.IsEffectivelyVisible != expected)
                        {
                            misses.Add(
                                where + ": " + name + " is "
                                + (control is null ? "absent" : control.IsEffectivelyVisible ? "drawn" : "not drawn")
                                + " where " + why + " is " + expected + ".");
                        }
                    }

                    // **NOTHING WAS BOUGHT BY COLLAPSING THE CARD.**
                    if (!card.IsExpanded)
                    {
                        misses.Add(where + ": the neighborhood card is collapsed.");
                    }

                    // **AND THE TWO THAT MOVED STILL SAY WHAT THEY SAID.**
                    foreach (var (name, words, what) in new[]
                    {
                        ("MapLegendMark", MapLegendControl.InWords, "the map's color key"),
                        ("GreenZoneRuleOfThumbMark", GreenZone.RuleOfThumb, "the rule of thumb"),
                    })
                    {
                        var mark = Named(window, name) as HintMarkControl;

                        if (mark is null || !mark.IsEffectivelyVisible || mark.Text != words
                            || words.Trim().Length == 0
                            || !mark.GetVisualAncestors().Contains(card))
                        {
                            misses.Add(
                                where + ": " + what + " is not on a drawn mark on the card in its own"
                                + " words - the mark says [" + (mark?.Text ?? "nothing") + "] where the"
                                + " words are [" + words + "].");
                        }
                    }

                    foreach (var word in ModePalette.Legend.Select(c => c.Label)
                        .Concat(new[] { "listen only", "heard just now" }))
                    {
                        if (!MapLegendControl.InWords.Contains(word, StringComparison.Ordinal))
                        {
                            misses.Add(
                                where + ": the legend's hover does not name [" + word
                                + "], which the legend on the card named.");
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

        Assert.True(misses.Count == 0, string.Join(Environment.NewLine, misses));
    }

    /// <summary>
    /// **Criterion 6.3: the sun map keeps its size and its dot.** Its rectangle is what task 1
    /// measured before the band came down, and the one marker is still the operator's own grid.
    /// </summary>
    [AvaloniaFact]
    public void TheSunMapIsTheSizeItWasAndStillCarriesHisGrid()
    {
        var misses = new List<string>();

        foreach (var (width, _, _, _) in Before)
        {
            foreach (var mode in Modes)
            {
                var window = TheTopRowTests.Realized(width, TheTopRowTests.WindowHeight, null, null);
                var model = (MainWindowViewModel)window.DataContext!;

                try
                {
                    model.ChosenDigitalMode = mode;
                    Pump(window);

                    var map = TheTopRowTests.Named<GrayLineMapControl>(window, "GreenZoneGrayLine");
                    var caption = Named(window, "GreenZoneClockCaption");
                    var where = Px(width) + " on " + mode;

                    _output.WriteLine(
                        where + ": the sun map is " + Px(map.Bounds.Width) + " x "
                        + Px(map.Bounds.Height) + " at grid " + map.OperatorGrid);

                    if (Math.Abs(map.Bounds.Width - SunMapWidth) > 0.5
                        || Math.Abs(map.Bounds.Height - SunMapHeight) > 0.5)
                    {
                        misses.Add(
                            where + ": the sun map is " + Px(map.Bounds.Width) + " x "
                            + Px(map.Bounds.Height) + " where task 1 measured " + Px(SunMapWidth)
                            + " x " + Px(SunMapHeight) + " before the band came down. It is not a"
                            + " source of pixels (6.3).");
                    }

                    if (!map.IsEffectivelyVisible
                        || string.IsNullOrWhiteSpace(map.OperatorGrid)
                        || map.OperatorGrid != model.GreenZone.OperatorGrid)
                    {
                        misses.Add(
                            where + ": the sun map's one marker is his grid, and the map is "
                            + (map.IsEffectivelyVisible ? "drawn" : "not drawn") + " at ["
                            + map.OperatorGrid + "] where the green zone holds ["
                            + model.GreenZone.OperatorGrid + "].");
                    }

                    if (caption is null || !caption.IsEffectivelyVisible)
                    {
                        misses.Add(where + ": the caption under the sun map is not drawn.");
                    }
                }
                finally
                {
                    model.ChosenDigitalMode = "FT8";
                    window.Close();
                }
            }
        }

        Assert.True(misses.Count == 0, string.Join(Environment.NewLine, misses));
    }

    /// <summary>
    /// **Criterion 6.4: unit 354's nine sizes hold with the new top** - at every one of them the
    /// working panels' row is the same or taller than task 1 measured it, and the drive note,
    /// which criterion 1.1 names, is still on the window.
    /// </summary>
    /// <remarks>
    /// **STEP 1 IS CLOSED AND THIS IS WHERE IT STAYS CLOSED.** <c>DigitalTransmitDriveNote</c> is
    /// one of the five controls criterion 1.1 asserts at all nine sizes; making it false would be
    /// a regression under HM-DEC-165 rather than a trade.
    /// </remarks>
    [AvaloniaFact]
    public void TheNineSizesHoldAndThePanelsAreNeverShorterThanTheyWere()
    {
        var misses = new List<string>();

        for (var i = 0; i < Unit354Sizes.Length; i++)
        {
            var (width, height) = Unit354Sizes[i];
            var was = BeforeAtUnit354Sizes[i];
            var window = TheTopRowTests.Realized(width, height, null, null);

            try
            {
                Pump(window);

                var panelRow = PanelRow(window);
                var note = Named(window, "DigitalTransmitDriveNote");

                _output.WriteLine(
                    Size(width, height) + ": the panel row is " + Px(panelRow) + " where it was "
                    + Px(was) + ", the band " + Px(Band(window).WithPills) + " with the pills");

                if (panelRow < was - 0.5)
                {
                    misses.Add(
                        Size(width, height) + ": the panel row is " + Px(panelRow)
                        + " px where task 1 measured " + Px(was) + ". Every one of the nine is the"
                        + " same or larger after this unit, and this one is smaller.");
                }

                if (note is null || !note.IsEffectivelyVisible)
                {
                    misses.Add(
                        Size(width, height) + ": DigitalTransmitDriveNote is "
                        + (note is null ? "not on the window" : "not drawn")
                        + ", and criterion 1.1 names it at all nine sizes.");
                }
            }
            finally
            {
                window.Close();
            }
        }

        Assert.True(misses.Count == 0, string.Join(Environment.NewLine, misses));
    }

    /// <summary>The sun map's width, as task 1 measured it before anything moved.</summary>
    public const double SunMapWidth = 246;

    /// <summary>The sun map's height, as task 1 measured it before anything moved.</summary>
    public const double SunMapHeight = 134;

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
