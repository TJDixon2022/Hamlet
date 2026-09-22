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
/// **Work instruction 389 task 1 item 5: the width a sun map at the band's left edge would leave
/// the pills, measured on today's tree before one pixel moves.**
/// </summary>
/// <remarks>
/// <para>**THIS TRACE ASSERTS NOTHING**, in the shape of unit 388's. It is the before that task 3's
/// arithmetic is computed against, and it is not on the carry-forward list: a name that cannot
/// fail teaches nothing by being run.</para>
/// <para>**COMPUTED, NOT SEEN** (§0.0, FACT-004). Every number is read off a realized headless
/// window. No port is opened and nothing is keyed.</para>
/// </remarks>
public sealed class Unit389TraceTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where every number is printed.</param>
    public Unit389TraceTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **The left-edge budget at 1920, 1400 and 1100 x 780, on FT8, PSK31 and Olivia.** Asserts
    /// nothing.
    /// </summary>
    [AvaloniaFact]
    public void Unit389TraceTheLeftEdgeBudgetBeforeOnePixelMoves()
    {
        foreach (var (width, height) in Unit376TheTopBandTests.Sizes)
        {
            foreach (var mode in Unit376TheTopBandTests.Modes)
            {
                var window = TheTopRowTests.Realized(width, height, null, null);
                var model = (MainWindowViewModel)window.DataContext!;

                try
                {
                    model.ChosenDigitalMode = mode;
                    Unit376TheTopBandTests.Pump(window);
                    Print(width, height, mode, window);
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
    /// **After task 3: where the map stands, how tall it is against the band, the pills' rows, the
    /// card and the caption - and the window width at which the map switches place.** Asserts
    /// nothing.
    /// </summary>
    [AvaloniaFact]
    public void Unit389TraceTheLeftEdgeAfterAndWhereItSwitches()
    {
        foreach (var (width, height) in Unit376TheTopBandTests.Sizes)
        {
            foreach (var mode in Unit376TheTopBandTests.Modes)
            {
                var window = TheTopRowTests.Realized(width, height, null, null);
                var model = (MainWindowViewModel)window.DataContext!;

                try
                {
                    model.ChosenDigitalMode = mode;
                    Unit376TheTopBandTests.Pump(window);

                    var band = Unit376TheTopBandTests.Band(window);
                    var row = TheTopRowTests.Named<BandGovernsTheMapPanel>(window, "BandRow");
                    var map = TheTopRowTests.RectIn(TheTopRowTests.Named<Control>(window, "GreenZoneGrayLine"), window);
                    var pills = TheTopRowTests.RectIn(Unit376TheTopBandTests.Pills(window), window);
                    var card = TheTopRowTests.RectIn(TheTopRowTests.Card(window), window);
                    var caption = TheTopRowTests.RectIn(TheTopRowTests.Named<Control>(window, "GreenZoneClockCaption"), window);
                    var buttons = Unit376TheTopBandTests.Pills(window).GetVisualDescendants().OfType<Button>()
                        .Where(b => b.Classes.Contains("hm-band")).ToList();
                    var rows = buttons.Select(b => Math.Round(TheTopRowTests.RectIn(b, window).Top)).Distinct().Count();
                    var rightmost = buttons.Max(b => TheTopRowTests.RectIn(b, window).Right);

                    _output.WriteLine("AFTER " + Px(width) + " x " + Px(height) + " on " + mode + ": left edge "
                        + row.MapIsAtTheLeftEdge + ", reach " + Px(row.PillsReach));
                    _output.WriteLine("  map " + Box(map) + " (" + (map.Width / map.Height).ToString("0.0000", CultureInfo.InvariantCulture)
                        + "); band with pills " + Px(band.WithPills) + " y " + Px(band.TopWithPills) + " to " + Px(band.Bottom)
                        + ", without " + Px(band.WithoutPills) + "; map top - band top " + Px(map.Top - band.TopWithPills)
                        + ", band bottom - map bottom " + Px(band.Bottom - map.Bottom));
                    _output.WriteLine("  pills " + Box(pills) + ", " + buttons.Count + " in " + rows + " row(s), rightmost ends x "
                        + Px(rightmost) + ", order " + string.Join(" ", buttons.Select(b => Px(TheTopRowTests.RectIn(b, window).Left))));
                    _output.WriteLine("  card " + Box(card) + ", rig " + Box(band.Rig) + ", caption " + Box(caption)
                        + " [" + ((TextBlock)TheTopRowTests.Named<Control>(window, "GreenZoneClockCaption")).Text + "]");

                    if (width == 1400)
                    {
                        // The card's lines that take more than one text line's height.
                        var body = TheTopRowTests.Named<Control>(window, "NeighborhoodCardBody");

                        foreach (var part in body.GetVisualDescendants().OfType<TextBlock>()
                            .Where(t => t.IsEffectivelyVisible && t.DesiredSize.Height > 16))
                        {
                            _output.WriteLine("    tall text " + Px(part.DesiredSize.Height) + " [" + part.Name + "] [" + part.Text + "]");
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

        foreach (var mode in Unit376TheTopBandTests.Modes)
        {
            var low = 1100.0;
            var high = 1920.0;

            while (high - low > 1)
            {
                var mid = Math.Floor((low + high) / 2);

                if (AtTheLeftEdge(mid, mode))
                {
                    high = mid;
                }
                else
                {
                    low = mid;
                }
            }

            _output.WriteLine("SWITCH on " + mode + ": stage A at " + Px(low) + " px wide, the left edge from " + Px(high)
                + " (window 1040 tall)");
        }

        // PSK31 with the dial where PSK31 lives, so the strayed line has nothing to say.
        var onIts = TheTopRowTests.Realized(1400, TheTopRowTests.WindowHeight, null, null);
        var psk31 = (MainWindowViewModel)onIts.DataContext!;

        try
        {
            psk31.ChosenDigitalMode = "PSK31";
            psk31.FrequencyHz = 14_070_000;
            Unit376TheTopBandTests.Pump(onIts);

            var map = TheTopRowTests.RectIn(TheTopRowTests.Named<Control>(onIts, "GreenZoneGrayLine"), onIts);

            _output.WriteLine("PSK31 AT 14.070 AT 1400: left edge "
                + TheTopRowTests.Named<BandGovernsTheMapPanel>(onIts, "BandRow").MapIsAtTheLeftEdge
                + ", map " + Box(map) + ", band " + Px(Unit376TheTopBandTests.Band(onIts).WithPills)
                + ", strayed line drawn " + TheTopRowTests.Named<Control>(onIts, "GreenZoneStrayedLine").IsEffectivelyVisible);
        }
        finally
        {
            psk31.ChosenDigitalMode = "FT8";
            onIts.Close();
        }
    }

    private static bool AtTheLeftEdge(double width, string mode)
    {
        var window = TheTopRowTests.Realized(width, TheTopRowTests.WindowHeight, null, null);
        var model = (MainWindowViewModel)window.DataContext!;

        try
        {
            model.ChosenDigitalMode = mode;
            Unit376TheTopBandTests.Pump(window);

            return TheTopRowTests.Named<BandGovernsTheMapPanel>(window, "BandRow").MapIsAtTheLeftEdge;
        }
        finally
        {
            model.ChosenDigitalMode = "FT8";
            window.Close();
        }
    }

    private void Print(double width, double height, string mode, Window window)
    {
        var band = Unit376TheTopBandTests.Band(window);
        var pills = Unit376TheTopBandTests.Pills(window);
        var pillsRect = TheTopRowTests.RectIn(pills, window);
        var bandRow = TheTopRowTests.Named<BandGovernsTheMapPanel>(window, "BandRow");
        var bandRowRect = TheTopRowTests.RectIn(bandRow, window);
        var map = TheTopRowTests.Named<Control>(window, "GreenZoneGrayLine");
        var mapBorder = TheTopRowTests.Named<Control>(window, "GreenZoneMap");
        var mapRect = TheTopRowTests.RectIn(map, window);
        var card = TheTopRowTests.Card(window);
        var cardRect = TheTopRowTests.RectIn(card, window);
        var scroller = TheTopRowTests.Named<Control>(window, "TopRowCardScroller");

        var buttons = pills.GetVisualDescendants().OfType<Button>().Where(b => b.Classes.Contains("hm-band")).ToList();
        var needed = buttons.Sum(b => b.Bounds.Width) + 8 * Math.Max(0, buttons.Count - 1);
        var rows = buttons.Select(b => Math.Round(TheTopRowTests.RectIn(b, window).Top)).Distinct().Count();

        var aspect = mapRect.Width / mapRect.Height;
        var mapAtBand = band.WithPills * aspect;
        var gap = mapBorder.Margin.Right;
        var left = pillsRect.Width - mapAtBand - gap;
        var breakEvenRow = mapAtBand + gap + needed;
        var breakEvenWindow = breakEvenRow + (width - pillsRect.Width);
        var cardSlot = bandRowRect.Width - (mapAtBand + gap) - band.Rig.Width - scroller.Margin.Right;

        _output.WriteLine("WINDOW " + Px(width) + " x " + Px(height) + " on " + mode);
        _output.WriteLine("  band with pills " + Px(band.WithPills) + ", without " + Px(band.WithoutPills)
            + "; pills row " + Box(pillsRect) + ", margin " + pills.Margin + ", " + buttons.Count
            + " pills in " + rows + " row(s) needing " + Px(needed));
        _output.WriteLine("  BandRow " + Box(bandRowRect) + ", map " + Box(mapRect) + " aspect "
            + aspect.ToString("0.0000", CultureInfo.InvariantCulture) + ", map gap " + Px(gap)
            + ", card " + Box(cardRect) + ", rig " + Box(band.Rig));
        _output.WriteLine("  LEFT EDGE: a map " + Px(band.WithPills) + " tall is " + Px(mapAtBand)
            + " wide; the pills would have " + Px(left) + " of the " + Px(needed) + " they need ("
            + (left >= needed ? "one row" : "SHORT by " + Px(needed - left)) + "); the card slot beside it "
            + Px(cardSlot) + " against CardFloor " + Px(BandGovernsTheMapPanel.CardFloor));
        _output.WriteLine("  BREAK-EVEN: a pills row of " + Px(breakEvenRow) + ", a window of "
            + Px(breakEvenWindow) + " (window minus pills row here " + Px(width - pillsRect.Width) + ")");
        _output.WriteLine("  clips: TopRow " + TheTopRowTests.Named<Control>(window, "TopRow").ClipToBounds
            + ", BandRow " + bandRow.ClipToBounds + ", ancestors clipping "
            + string.Join(",", bandRow.GetVisualAncestors().OfType<Control>().Where(c => c.ClipToBounds)
                .Select(c => c.GetType().Name + (string.IsNullOrEmpty(c.Name) ? "" : "#" + c.Name))));
        _output.WriteLine("");
    }

    private static string Px(double value)
        => value.ToString("0.#", CultureInfo.InvariantCulture);

    private static string Box(Rect r)
        => Px(r.X) + "," + Px(r.Y) + " " + Px(r.Width) + " x " + Px(r.Height);
}
