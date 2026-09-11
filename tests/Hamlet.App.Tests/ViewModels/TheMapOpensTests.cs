using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Explore;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 310 task 5: **click the map and it opens.**
/// </summary>
/// <remarks>
/// <para>**R8** (Tim, 2026-09-11): *"When I click on a map it gets enlarged"*, *"it is
/// a popup with a dismiss X"*, *"zoomed into path"*.</para>
/// <para>**THE PATH DEFINES THE FRAME, NOT THE TWO ENDPOINTS.** The arc bulges well
/// north of both stations, so a box drawn from the endpoints alone crops the top off
/// the thing being looked at.</para>
/// <para>**COMPUTED, NOT SEEN.** What is asserted is the box the opened map frames and
/// whether the popup is open. Nothing here looks at a pixel, and nothing here can say
/// the result is legible.</para>
/// </remarks>
public sealed class TheMapOpensTests
{
    private const string HisCall = "KC3QIS";

    /// <summary>The operator's own grid, which every case here starts from.</summary>
    private const string HisGrid = "FN00DJ";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the frames are printed.</param>
    public TheMapOpensTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Clicking opens the map and the dismiss X closes it.**</summary>
    [Fact]
    public void ClickingOpensTheMapAndTheXClosesIt()
    {
        var card = ConversationWith("JA1ABC", "PM95");

        Assert.True(card.MapOpens, "the card offers no map to open");
        Assert.False(card.MapIsOpen);

        card.OpenTheMapCommand.Execute(null);

        Assert.True(card.MapIsOpen, "clicking the map did not open it");

        card.CloseTheMapCommand.Execute(null);

        Assert.False(card.MapIsOpen, "the dismiss X did not close it");
    }

    /// <summary>**The frame holds every sampled point and both markers, with margin.**</summary>
    [Fact]
    public void TheFrameHoldsEverySampledPointAndBothMarkers()
    {
        foreach (var (call, grid) in new[]
        {
            ("JA1ABC", "PM95"),
            ("UA3ABC", "KO85"),
            ("G0ABC", "IO91"),
            ("PY2ABC", "GG66"),
        })
        {
            var plot = ConversationWith(call, grid).Globe;
            var (left, top, width, height) = plot.OpenFrame;

            _output.WriteLine(
                call.PadRight(7) + grid + "  " + plot.OpenFrameLine);

            var points = plot.Path.SelectMany(run => run).ToList();

            Assert.NotEmpty(points);

            if (plot.HasOperator)
            {
                points.Add((plot.OperatorX, plot.OperatorY));
            }

            if (plot.HasStation)
            {
                points.Add((plot.StationX, plot.StationY));
            }

            foreach (var (x, y) in points)
            {
                Assert.True(
                    x >= left && x <= left + width && y >= top && y <= top + height,
                    call + ": (" + x.ToString("0.0", CultureInfo.InvariantCulture)
                    + ", " + y.ToString("0.0", CultureInfo.InvariantCulture)
                    + ") is outside " + plot.OpenFrameLine);
            }

            // **AND NOTHING SITS ON AN EDGE.** A frame that merely contains the path
            // puts the northernmost point on the top line of the picture, which reads
            // as a path leaving the map.
            var spare = Math.Min(
                Math.Min(points.Min(p => p.Item1) - left,
                    left + width - points.Max(p => p.Item1)),
                Math.Min(points.Min(p => p.Item2) - top,
                    top + height - points.Max(p => p.Item2)));

            _output.WriteLine(
                "          closest approach to an edge: "
                + spare.ToString("0.0", CultureInfo.InvariantCulture) + " px");

            // **UNLESS THE FRAME IS THE WHOLE FILE**, where the edge is the edge of
            // what Hamlet knows and there is nowhere further to go.
            var whole = width >= Ft8GlobePlot.Map.WidthPixels
                && height >= Ft8GlobePlot.Map.HeightPixels;

            Assert.True(whole || spare > 1.0, call + ": the path touches an edge");
        }
    }

    /// <summary>**The path chooses the frame, and the two endpoints do not.**</summary>
    /// <remarks>
    /// <para>**THE INSTRUCTION NAMES TOKYO FOR THIS AND TOKYO CANNOT SHOW IT**
    /// (measured, reported as a mismatch). FN00DJ to Tokyo crosses the antimeridian, so
    /// it is two runs against opposite edges and it shows the whole world - which does
    /// contain the bulge, but by the date-line rule rather than by the frame following
    /// the arc. <see cref="ATokyoPathShowsTheWholeWorld"/> records that.</para>
    /// <para>**MOSCOW IS THE CASE THAT PROVES IT.** FN00DJ to KO85 goes north over
    /// Greenland, stays on one side of the date line, and peaks a long way above both
    /// stations. A box drawn from the two endpoints would crop the top off the arc,
    /// which is the half of the picture that teaches where a signal actually goes.
    /// </para>
    /// </remarks>
    [Fact]
    public void ThePathAndNotTheEndpointsChoosesTheFrame()
    {
        var plot = ConversationWith("UA3ABC", "KO85").Globe;

        Assert.Single(plot.Path);

        var north = plot.Path.SelectMany(run => run).Min(p => p.Y);
        var endpoints = Math.Min(plot.OperatorY, plot.StationY);

        _output.WriteLine("frame        : " + plot.OpenFrameLine);
        _output.WriteLine(
            "the arc peaks at y "
            + north.ToString("0.0", CultureInfo.InvariantCulture));
        _output.WriteLine(
            "the stations sit at y "
            + plot.OperatorY.ToString("0.0", CultureInfo.InvariantCulture)
            + " and " + plot.StationY.ToString("0.0", CultureInfo.InvariantCulture));

        foreach (var latitude in new double[] { 55, 60, 66.8, 70 })
        {
            if (Ft8GlobePlot.Map.Place(latitude, 0) is { } at)
            {
                _output.WriteLine(
                    "   " + latitude.ToString("0.0", CultureInfo.InvariantCulture)
                    + "N is at y " + at.Y.ToString("0.0", CultureInfo.InvariantCulture));
            }
        }

        // **THE ARC GOES NORTH OF BOTH STATIONS.**
        Assert.True(
            north < endpoints,
            "the arc peaks at y " + north.ToString("0.0", CultureInfo.InvariantCulture)
            + " and the higher station is at y "
            + endpoints.ToString("0.0", CultureInfo.InvariantCulture));

        // **AND THE FRAME FOLLOWED THE ARC AND NOT THE STATIONS.**
        Assert.True(
            plot.OpenFrame.Top <= north,
            "the frame starts at y "
            + plot.OpenFrame.Top.ToString("0.0", CultureInfo.InvariantCulture)
            + " and the arc reaches y "
            + north.ToString("0.0", CultureInfo.InvariantCulture));

        Assert.True(
            plot.OpenFrame.Top < endpoints,
            "an endpoints-only box would have started at y "
            + endpoints.ToString("0.0", CultureInfo.InvariantCulture));

        // **AND THE ARC REALLY IS THAT FAR NORTH**, stated as a latitude rather than
        // as a row of the file, because a row means nothing to a reader.
        if (Ft8GlobePlot.Map.Place(60, 0) is { } sixty)
        {
            Assert.True(
                north <= sixty.Y,
                "the arc does not reach 60N");
        }
    }

    /// <summary>**A path to Tokyo shows the whole world, and why.**</summary>
    /// <remarks>
    /// **MEASURED, AND IT IS NOT WHAT THE INSTRUCTION EXPECTED.** It names Tokyo as the
    /// case proving the frame follows the arc. FN00DJ to Tokyo goes over northern
    /// Alaska and **crosses the antimeridian**, so the path is two runs against
    /// opposite edges of the file and R8's own date-line rule takes it: the box that
    /// holds both runs is the whole picture. The bulge is on screen either way.
    /// </remarks>
    [Fact]
    public void ATokyoPathShowsTheWholeWorld()
    {
        var plot = ConversationWith("JA1ABC", "PM95").Globe;

        var north = plot.Path.SelectMany(run => run).Min(p => p.Y);

        _output.WriteLine("runs         : " + plot.Path.Count);
        _output.WriteLine("frame        : " + plot.OpenFrameLine);
        _output.WriteLine(
            "the arc peaks at y "
            + north.ToString("0.0", CultureInfo.InvariantCulture)
            + ", the stations sit at y "
            + plot.OperatorY.ToString("0.0", CultureInfo.InvariantCulture)
            + " and " + plot.StationY.ToString("0.0", CultureInfo.InvariantCulture));

        Assert.True(plot.Path.Count > 1, "this path does not cross the date line");

        Assert.Equal(Ft8GlobePlot.Map.WidthPixels, plot.OpenFrame.Width);
        Assert.Equal(Ft8GlobePlot.Map.HeightPixels, plot.OpenFrame.Height);

        // **AND THE BULGE IS INSIDE IT**, which is what R8 asked for.
        Assert.True(plot.OpenFrame.Top <= north);
        Assert.True(north < Math.Min(plot.OperatorY, plot.StationY));
    }

    /// <summary>**A path across the date line shows the whole world.**</summary>
    /// <remarks>
    /// **TWO RUNS AGAINST OPPOSITE EDGES** (R8). The box that holds both is the file,
    /// and anything tighter needs a re-centred map, which this unit is not building.
    /// </remarks>
    [Fact]
    public void ADateLinePathShowsTheWholeWorld()
    {
        var plot = DateLinePlot();

        _output.WriteLine("runs         : " + plot.Path.Count);
        _output.WriteLine("frame        : " + plot.OpenFrameLine);

        Assert.True(plot.Path.Count > 1, "this path does not cross the date line");

        Assert.Equal(0, plot.OpenFrame.Left);
        Assert.Equal(0, plot.OpenFrame.Top);
        Assert.Equal(Ft8GlobePlot.Map.WidthPixels, plot.OpenFrame.Width);
        Assert.Equal(Ft8GlobePlot.Map.HeightPixels, plot.OpenFrame.Height);
    }

    /// <summary>**The zoom floor holds for a short contact.**</summary>
    /// <remarks>
    /// **A QUARTER OF THE FILE** (<see cref="Ft8GlobePlot.ZoomFloorShare"/>). Two
    /// stations a couple of hundred miles apart would otherwise fill the popup from a
    /// crop sixty pixels across, and there is no more detail in the photograph than it
    /// holds.
    /// </remarks>
    [Fact]
    public void TheZoomFloorHoldsForAShortContact()
    {
        // **FN20 IS ABOUT A HUNDRED AND TWENTY MILES FROM FN00DJ.**
        var plot = ConversationWith("W2ABC", "FN20").Globe;

        _output.WriteLine("frame        : " + plot.OpenFrameLine);
        _output.WriteLine(
            "miles        : "
            + (plot.Miles ?? 0).ToString("0", CultureInfo.InvariantCulture));

        var floorWidth = Ft8GlobePlot.Map.WidthPixels * Ft8GlobePlot.ZoomFloorShare;
        var floorHeight = Ft8GlobePlot.Map.HeightPixels * Ft8GlobePlot.ZoomFloorShare;

        _output.WriteLine(
            "floor        : " + floorWidth.ToString("0.0", CultureInfo.InvariantCulture)
            + " by " + floorHeight.ToString("0.0", CultureInfo.InvariantCulture));

        Assert.True(
            plot.OpenFrame.Width >= floorWidth - 0.001,
            "the frame is " + plot.OpenFrame.Width.ToString(
                "0.0", CultureInfo.InvariantCulture) + " wide");

        Assert.True(
            plot.OpenFrame.Height >= floorHeight - 0.001,
            "the frame is " + plot.OpenFrame.Height.ToString(
                "0.0", CultureInfo.InvariantCulture) + " tall");

        // **AND IT STILL LIES INSIDE THE PICTURE.**
        Assert.True(plot.OpenFrame.Left >= 0);
        Assert.True(plot.OpenFrame.Top >= 0);

        Assert.True(
            plot.OpenFrame.Left + plot.OpenFrame.Width
            <= Ft8GlobePlot.Map.WidthPixels + 0.001);

        Assert.True(
            plot.OpenFrame.Top + plot.OpenFrame.Height
            <= Ft8GlobePlot.Map.HeightPixels + 0.001);
    }

    /// <summary>**The opened map really is zoomed, and both markers are on it.**</summary>
    /// <remarks>
    /// <para>**THE SAME CONTROL DRAWS BOTH MAPS**, so there is one set of arithmetic
    /// placing the markers and one set of constants describing the picture. What
    /// changes is the window of the file it frames, and this reads that back at the
    /// size the popup gives it.</para>
    /// <para>**AND THE MARKER HOVERS COME WITH IT** - the instruction's drop candidate.
    /// They were not built again: the hover asks the same one place for the frame that
    /// the render does, so it follows the zoom on its own. There is no test of the
    /// hover here, because the words it shows are behind a pointer this project cannot
    /// move.</para>
    /// </remarks>
    [Fact]
    public void TheOpenedMapIsZoomedAndBothMarkersAreOnIt()
    {
        var plot = ConversationWith("G0ABC", "IO91").Globe;

        const double width = 720;
        const double height = 400;

        var small = Ft8GlobeControl.WhatWouldBeDrawn(plot, width, height);
        var open = Ft8GlobeControl.WhatWouldBeDrawn(plot, width, height, opened: true);

        double Apart(Ft8GlobeControl.Drawn drawn)
            => Math.Sqrt(
                Math.Pow(drawn.OperatorAt.X - drawn.StationAt.X, 2)
                + Math.Pow(drawn.OperatorAt.Y - drawn.StationAt.Y, 2));

        _output.WriteLine("frame        : " + plot.OpenFrameLine);
        _output.WriteLine(
            "the whole file puts them "
            + Apart(small).ToString("0", CultureInfo.InvariantCulture)
            + " px apart at this size, the opened map "
            + Apart(open).ToString("0", CultureInfo.InvariantCulture));

        Assert.True(open.HasOperatorMarker);
        Assert.True(open.HasStationMarker);
        Assert.True(open.PathSegments > 0);

        // **IT IS BIGGER, WHICH IS THE WHOLE POINT OF OPENING IT.**
        Assert.True(
            Apart(open) > Apart(small) * 1.5,
            "the opened map is not meaningfully larger");
    }

    /// <summary>**A receipt has no map, so there is nothing to click.**</summary>
    [Fact]
    public void AReceiptHasNoMapAndNothingToClick()
    {
        var model = Panel();

        model.SendCallToAnyoneCommand.Execute(null);

        var receipt = Assert.Single(model.DigitalCards);

        Assert.True(receipt.IsCallToAnyone);
        Assert.False(receipt.ShowsGlobe);
        Assert.False(receipt.MapOpens);

        // **AND PRESSING IT ANYWAY OPENS NOTHING.** The markup gives him nothing to
        // press, and the command refuses on its own account as well, because a button
        // that is not drawn is not the same as a command that would work if it were.
        receipt.OpenTheMapCommand.Execute(null);

        Assert.False(receipt.MapIsOpen);
    }

    /// <summary>**A station the picture cannot place has nothing to open.**</summary>
    /// <remarks>
    /// **THE INSTRUCTION SAYS SUCH A CARD HAS NO MAP ROW.** It has one: the operator is
    /// placed even when the station is not, so the row draws his own marker and the
    /// caption says where the picture stops. **Reported as a mismatch.** What is gated
    /// is the opening, which is what R8 is about.
    /// </remarks>
    [Fact]
    public void AStationThePictureCannotPlaceHasNothingToOpen()
    {
        // **RB77 IS DEEP IN ANTARCTICA**, below the 63.79S the file reaches.
        var card = ConversationWith("VP8ABC", "RB77");

        _output.WriteLine("row shown    : " + card.ShowsGlobe);
        _output.WriteLine("opens        : " + card.MapOpens);
        _output.WriteLine("path runs    : " + card.Globe.Path.Count);

        Assert.False(card.Globe.HasStation, "the file can place this station after all");
        Assert.False(card.MapOpens);

        card.OpenTheMapCommand.Execute(null);

        Assert.False(card.MapIsOpen);
    }

    /// <summary>A plot whose great circle crosses the antimeridian.</summary>
    /// <returns>The plot, whichever of the candidates splits into two runs.</returns>
    private Ft8GlobePlot DateLinePlot()
    {
        foreach (var grid in new[] { "RE78", "QF56", "PM95", "AH01" })
        {
            var plot = new Ft8GlobePlot(HisGrid, grid, "ZL1ABC", "");

            _output.WriteLine(
                "candidate " + grid + ": " + plot.Path.Count + " run(s)");

            if (plot.Path.Count > 1)
            {
                return plot;
            }
        }

        return new Ft8GlobePlot(HisGrid, "RE78", "ZL1ABC", "");
    }

    private static Ft8ContactCard ConversationWith(string call, string grid)
    {
        var model = Panel();

        model.AddDecodeRowForTests(
            "021115", "-09", "0.2", "1240",
            HisCall + " " + call + " " + grid,
            Slot("02:11:15"), heardOnHz: 14_074_000);

        model.CardsNowForTests = Slot("02:11:30");
        model.RebuildCardsForTests();

        return model.DigitalCards.Single(c => c.Callsign == call);
    }

    private static MainWindowViewModel Panel()
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = HisCall;
        settings.Operator.GridSquare = HisGrid;

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        model.ClockOffset = new Hamlet.RadioEngine.Audio.ClockOffset(
            0.033, DateTime.UtcNow);

        return model;
    }

    private static DateTime Slot(string at)
        => DateTime.ParseExact(
            "2026-09-10 " + at, "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
}
