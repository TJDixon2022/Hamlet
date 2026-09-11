using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Explore;
using System.IO;

namespace Hamlet.App.Controls;

/// <summary>
/// **A small map: the coastline, the operator, the station, and a line between.**
/// </summary>
/// <remarks>
/// <para>**§0.0 BINDS A PICTURE AS HARD AS A SENTENCE** (HM-DEC-092), and this
/// unit's exposure is a dot in the wrong place, which nobody would ever check. So
/// the dots are placed by <see cref="Ft8GlobePlot"/>, which uses **the coastline's
/// own projection, written in its own `&lt;desc&gt;`** - there is not a second one
/// anywhere in this control.</para>
/// <para>**IT DRAWS WHAT IS KNOWN AND NOTHING ELSE.** No station grid means no
/// station dot and no line; no operator grid means no operator dot. The caption
/// beside it says which, so an absence is never left as a picture of one dot the
/// reader has to interpret.</para>
/// <para>**THE COASTLINE IS NOT SURVEY DATA** and `assets/world-coastline.md` says
/// so. At this size fidelity buys nothing, and drawing it removed a download, a
/// licence question and a network call from the unit that built it.</para>
/// <para>**IT IS A `Control` AND NOT A COMPOSITION.** A Border wrapping a Canvas
/// wrapping shapes would hand the pointer several hit targets for one picture, and
/// the whole thing lives inside a tooltip that must behave as one object.</para>
/// </remarks>
public sealed class Ft8GlobeControl : Control
{
    /// <summary>Where the coastline lives.</summary>
    /// <remarks>
    /// **NOTHING HERE DRAWS IT SINCE UNIT 306**, which put a real photograph under
    /// the dots. The file is not deleted and this constant is not removed, because
    /// the asset note beside it records what it is and other surfaces may reach for
    /// it.
    /// </remarks>
    public const string CoastlineUri =
        "avares://Hamlet.App/Assets/world-coastline.svg";

    /// <summary>What is being plotted.</summary>
    public static readonly StyledProperty<Ft8GlobePlot?> PlotProperty =
        AvaloniaProperty.Register<Ft8GlobeControl, Ft8GlobePlot?>(nameof(Plot));

    /// <summary>True where this is the enlarged map rather than the card's own.</summary>
    /// <remarks>
    /// **R8** (Tim, 2026-09-11), *"zoomed into path"*. The same control draws both, so
    /// there is one set of arithmetic placing the markers and one set of constants
    /// describing the picture. What changes is which window of the file it frames.
    /// </remarks>
    public static readonly StyledProperty<bool> OpenedProperty =
        AvaloniaProperty.Register<Ft8GlobeControl, bool>(nameof(Opened));

    /// <summary>
    /// **The map itself, loaded once and only where its bytes are recognised.**
    /// </summary>
    /// <remarks>
    /// <para>**THE HASH IS THE GATE AND A FAILURE SPEAKS UNASKED** (§0.0,
    /// HM-DEC-092). The geometry that places the dots describes one particular file;
    /// against a re-saved or re-cropped one every station would land somewhere
    /// plausible and untrue, and nobody ever checks a dot. So the bitmap is only
    /// accepted where <see cref="AzimuthalImage.Describes"/> says yes, and where it
    /// does not there is no picture and therefore no dots.</para>
    /// <para>**READ THROUGH THE ASSET LOADER**, which is how every other resource in
    /// this application is reached, so nothing here depends on a file path at run
    /// time.</para>
    /// </remarks>
    private static readonly Lazy<Bitmap?> Map = new(LoadTheMap);

    private static readonly IBrush Paper = new SolidColorBrush(Color.Parse("#DDE6EC"));
    private static readonly IBrush Ink = new SolidColorBrush(Color.Parse("#5F5C53"));
    private static readonly IBrush Mine = new SolidColorBrush(Color.Parse("#C8842A"));
    private static readonly IBrush Theirs = new SolidColorBrush(Color.Parse("#2F7D4F"));

    /// <summary>What every stroke on the picture is cased with.</summary>
    /// <remarks>
    /// <para>**THE FAULT WAS CONTRAST, NOT SIZE** (work instruction 309 task 1,
    /// measured). The path and both markers were drawn the whole time; what was
    /// wrong is that a muted grey dash vanishes over dark blue ocean and an amber
    /// ring vanishes over bright orange land. **One colour reads over one ground or
    /// the other and not both**, which is what a shipped photograph puts underneath.
    /// </para>
    /// <para>**SO EVERY STROKE IS DRAWN TWICE**: this underneath, wider, and the
    /// ink over it. The pair reads over land and over sea, and **neither of the two
    /// markers depends on hue to be told apart** - one is a ring and one is filled
    /// (§0.6).</para>
    /// <para>**IT IS PAPER, WHICH THE APPLICATION ALREADY USES.** `#DDE6EC` is this
    /// control own ground colour; no new colour is introduced (§0.5).</para>
    /// </remarks>
    private static readonly IBrush Casing = new SolidColorBrush(Color.Parse("#DDE6EC"));

    /// <summary>How wide the path is drawn, in the control own units.</summary>
    /// <remarks>
    /// **IN CONTROL UNITS AND NEVER IN BITMAP ONES.** Placement scales with the
    /// picture; size does not. The card draws this map at about a third of the
    /// bitmap, and anything sized in bitmap coordinates would arrive at a third of
    /// its stated width. **Nothing here reads the card own size**, so the same card
    /// at half the width still shows the line.
    /// </remarks>
    public const double PathStrokeWidth = 2.0;

    /// <summary>How wide the casing under the path is.</summary>
    public const double PathCasingWidth = 4.0;

    /// <summary>How big a station marker is drawn, in the control own units.</summary>
    public const double MarkerRadius = 4.5;

    /// <summary>How wide a marker own outline is.</summary>
    public const double MarkerStrokeWidth = 2.0;

    /// <summary>The dashes the path is drawn with, in stroke widths.</summary>
    /// <remarks>
    /// **THE PATTERN IS IN CONTROL SPACE TOO**, for the same reason the width is: a
    /// dash measured in bitmap pixels collapses into a solid line at this scale and
    /// stops reading as a path rather than a border.
    /// </remarks>
    private static readonly double[] Dashes = { 3, 2 };

    /// <summary>What a render would put on the surface, without a surface.</summary>
    /// <param name="Plot">What is being drawn.</param>
    /// <param name="PathRuns">How many runs of the path reach the surface.</param>
    /// <param name="PathSegments">How many line segments those runs hold.</param>
    /// <param name="PathStrokes">How many strokes are drawn, casing included.</param>
    /// <param name="HasOperatorMarker">Whether the operator marker is drawn.</param>
    /// <param name="HasStationMarker">Whether the station marker is drawn.</param>
    /// <param name="OperatorAt">Where the operator marker lands on the control.</param>
    /// <param name="StationAt">Where the station marker lands on the control.</param>
    /// <param name="StrokeWidth">The path width that reaches the screen.</param>
    /// <param name="CasingWidth">The casing width that reaches the screen.</param>
    /// <param name="MarkerRadius">The marker radius that reaches the screen.</param>
    /// <remarks>
    /// **NOTHING IN THIS REPOSITORY CAN LOOK AT A PICTURE**, and two faults reached
    /// the operator screen through exactly that gap. This is the nearest thing
    /// available: **what the render decides, separated from the drawing of it**, so a
    /// test can assert the decisions rather than assert that a control exists and
    /// hope. **It still says nothing about whether the result is legible.**
    /// </remarks>
    public sealed record Drawn(
        Ft8GlobePlot? Plot,
        int PathRuns,
        int PathSegments,
        int PathStrokes,
        bool HasOperatorMarker,
        bool HasStationMarker,
        Point OperatorAt,
        Point StationAt,
        double StrokeWidth,
        double CasingWidth,
        double MarkerRadius);

    /// <summary>What a render at this size would draw.</summary>
    /// <param name="plot">What is being drawn.</param>
    /// <param name="width">The control own width.</param>
    /// <param name="height">The control own height.</param>
    /// <param name="opened">True for the enlarged map, which frames the path.</param>
    /// <returns>The decisions a render would make.</returns>
    public static Drawn WhatWouldBeDrawn(
        Ft8GlobePlot? plot, double width, double height, bool opened = false)
    {
        if (plot is null || width <= 0 || height <= 0)
        {
            return new Drawn(
                plot, 0, 0, 0, false, false, default, default,
                PathStrokeWidth, PathCasingWidth, MarkerRadius);
        }

        var (left, top, mapWidth, mapHeight) = FrameOf(plot, opened);
        var scale = Math.Min(width / mapWidth, height / mapHeight);

        Point At(double x, double y)
            => new((x - left) * scale, (y - top) * scale);

        var runs = 0;
        var segments = 0;

        foreach (var run in plot.Path)
        {
            if (run.Count < 2)
            {
                continue;
            }

            runs++;
            segments += run.Count - 1;
        }

        return new Drawn(
            plot,
            runs,
            segments,

            // **CASED MEANS DRAWN TWICE**, which is what makes it read over both
            // grounds rather than one.
            segments * 2,
            plot.HasOperator,
            plot.HasStation,
            plot.HasOperator ? At(plot.OperatorX, plot.OperatorY) : default,
            plot.HasStation ? At(plot.StationX, plot.StationY) : default,
            PathStrokeWidth,
            PathCasingWidth,
            MarkerRadius);
    }

    static Ft8GlobeControl()
    {
        AffectsRender<Ft8GlobeControl>(PlotProperty, OpenedProperty);

        // **BOTH CHANGE THE SHAPE THE CONTROL ASKS FOR**, because the frame decides
        // it: a zoomed window is not the file's proportions and the row has to follow.
        AffectsMeasure<Ft8GlobeControl>(PlotProperty, OpenedProperty);
    }

    /// <summary>How near the pointer has to be to a marker, in map pixels.</summary>
    /// <remarks>
    /// **TWELVE, AND THE MARKER IS DRAWN AT FOUR.** A hit target the size of the ink
    /// is a target nobody can hit; three times the radius is about a fingertip at the
    /// size this picture is drawn and is the same reasoning unit 301 used when the
    /// hint mark got a transparent rectangle rather than an outline.
    /// </remarks>
    public const double MarkerReachPixels = 12.0;

    /// <summary>Which marker is at a point on the map, in words, or null.</summary>
    /// <param name="x">A position in the picture own pixels.</param>
    /// <param name="y">A position in the picture own pixels.</param>
    /// <param name="width">How wide the picture is, for the reach.</param>
    /// <returns>The words for that marker, or null where there is no marker.</returns>
    /// <remarks>
    /// <para>**THE STATION IS ASKED FIRST.** Where the two markers overlap - an
    /// operator working somebody a few hundred miles away - the one he is reading
    /// about is the other station, and his own position is the one he already
    /// knows.</para>
    /// <para>**INTERNAL SO A TEST CAN ASK IT WITHOUT A POINTER.** Driving a real
    /// pointer through a headless window to assert a tooltip is a test about
    /// Avalonia; what this unit owns is which marker a position belongs to.</para>
    /// </remarks>
    public string? WordsAt(double x, double y, double width)
    {
        if (Plot is not { } plot || width <= 0)
        {
            return null;
        }

        bool Near(double markerX, double markerY)
            => Math.Sqrt(
                ((x - markerX) * (x - markerX))
                + ((y - markerY) * (y - markerY))) <= MarkerReachPixels;

        if (plot.HasStation && Near(plot.StationX, plot.StationY))
        {
            return plot.StationTip;
        }

        return plot.HasOperator && Near(plot.OperatorX, plot.OperatorY)
            ? plot.OperatorTip
            : null;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// **THE MAP OWNS ITS OWN HIT TARGETS NOW** (work instruction 308 task 4). It sits
    /// on the card face rather than inside a tooltip, so each marker can answer for
    /// itself - which is what unit 306 built the words for and could not attach.
    /// </remarks>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (Plot is not { } plot || Bounds.Width <= 0)
        {
            return;
        }

        var (left, top, width, height) = FrameOf(plot, Opened);

        if (width <= 0 || height <= 0)
        {
            return;
        }

        var scale = Math.Min(Bounds.Width / width, Bounds.Height / height);

        if (scale <= 0)
        {
            return;
        }

        var at = e.GetPosition(this);
        var words = WordsAt((at.X / scale) + left, (at.Y / scale) + top, width);

        // **NULL AND NOT THE EMPTY STRING**, because Avalonia draws an empty tooltip
        // for "" and a blank box following the pointer across an ocean is worse than
        // no tooltip at all - the same finding unit 274 recorded about a row.
        ToolTip.SetTip(this, string.IsNullOrEmpty(words) ? null : words);
    }

    /// <summary>What is being plotted.</summary>
    public Ft8GlobePlot? Plot
    {
        get => GetValue(PlotProperty);
        set => SetValue(PlotProperty, value);
    }

    /// <summary>True where this is the enlarged map.</summary>
    public bool Opened
    {
        get => GetValue(OpenedProperty);
        set => SetValue(OpenedProperty, value);
    }

    /// <summary>Which window of the file a control in this state frames.</summary>
    /// <param name="plot">What is being plotted.</param>
    /// <param name="opened">True for the enlarged map.</param>
    /// <returns>Left, top, width and height in the file's own pixels.</returns>
    /// <remarks>
    /// **ONE PLACE DECIDES IT**, so the size the control asks for, the picture it
    /// draws and what a test reads back cannot come to disagree (0).
    /// </remarks>
    public static (double Left, double Top, double Width, double Height) FrameOf(
        Ft8GlobePlot? plot, bool opened)
        => plot is null
            ? (0, 0, FlatWorldMap.Relief.WidthPixels, FlatWorldMap.Relief.HeightPixels)
            : opened ? plot.OpenFrame : plot.Frame;

    /// <summary>How tall the map is at a given width.</summary>
    /// <param name="width">A width in the control own units.</param>
    /// <returns>The height that keeps the picture own proportions exactly.</returns>
    /// <remarks>
    /// **THE ASPECT IS READ FROM THE PICTURE RECORD**, so a different map changes
    /// this with it and there is no second copy of the number to drift (0).
    /// </remarks>
    public static double HeightFor(double width)
        => width <= 0
            ? 0
            : width * FlatWorldMap.Relief.HeightPixels
              / FlatWorldMap.Relief.WidthPixels;

    /// <summary>How wide the map is at a given height.</summary>
    /// <param name="height">A height in the control own units.</param>
    /// <returns>The width that keeps the picture own proportions exactly.</returns>
    public static double WidthFor(double height)
        => height <= 0
            ? 0
            : height * FlatWorldMap.Relief.WidthPixels
              / FlatWorldMap.Relief.HeightPixels;

    /// <inheritdoc/>
    /// <remarks>
    /// <para>**THE ROW SHRINKS TO THE MAP** (R7, Tim 2026-09-11, choosing that over
    /// cropping the picture and over stretching it). This asks for **the largest box
    /// at the map own proportions that fits in what it is offered**, which is exactly
    /// the box <see cref="Render"/> already draws into. So the control and the
    /// picture become the same rectangle and there is nothing left over to be
    /// grey.</para>
    /// <para>**STRETCHING WAS NEVER ON THE TABLE.** The projection six measured
    /// constants describe this bitmap at its own proportions, and filling a row of
    /// some other shape would move every marker off the place it belongs - the fault
    /// those constants exist to prevent (0.0, HM-DEC-092).</para>
    /// <para>**NOTHING HERE IS A HARD-CODED SIZE.** Both figures come from the
    /// picture record; what the card offers decides the rest.</para>
    /// </remarks>
    protected override Size MeasureOverride(Size availableSize)
    {
        // **THE FRAME DECIDES THE SHAPE, NOT THE FILE.** The card's own map frames
        // the whole picture, so this is the bitmap's proportions there; the enlarged
        // one frames a window on to it and is whatever shape that window is.
        var (_, _, frameWidth, frameHeight) = FrameOf(Plot, Opened);

        var width = availableSize.Width;
        var height = availableSize.Height;

        // **OFFERED NOTHING, IT ASKS FOR THE FRAME OWN SIZE.** A control with no
        // constraint at all is still the right shape.
        if (double.IsInfinity(width) && double.IsInfinity(height))
        {
            return new Size(frameWidth, frameHeight);
        }

        if (double.IsInfinity(width))
        {
            width = height * frameWidth / frameHeight;
        }

        if (double.IsInfinity(height))
        {
            height = width * frameHeight / frameWidth;
        }

        if (width <= 0 || height <= 0 || frameWidth <= 0 || frameHeight <= 0)
        {
            return default;
        }

        // **THE SAME FIT THE RENDER MAKES**, written once in each place because the
        // render needs it against `Bounds` and this needs it against an offer.
        var scale = Math.Min(width / frameWidth, height / frameHeight);

        return new Size(frameWidth * scale, frameHeight * scale);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <para>**THE PICTURE IS THE MAP AND THE CODE PUTS DOTS ON IT.** Nothing here
    /// draws a coastline, a graticule or a degree label; the ring of degrees printed
    /// round the asset is part of the photograph and is not Hamlet asserting
    /// anything (Tim, 2026-09-10).</para>
    /// <para>**NO MAP MEANS NO DOTS.** Where the bitmap is missing or its bytes are
    /// not the ones the geometry describes, this draws the plain ground and stops -
    /// a dot without the picture it was placed against is a claim with nothing
    /// behind it.</para>
    /// </remarks>
    public override void Render(DrawingContext context)
    {
        if (Plot is not { } plot || Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return;
        }

        var (left, top, width, height) = FrameOf(plot, Opened);

        if (width <= 0 || height <= 0)
        {
            return;
        }

        context.FillRectangle(Paper, new Rect(Bounds.Size));

        if (Map.Value is not { } map)
        {
            return;
        }

        var scale = Math.Min(Bounds.Width / width, Bounds.Height / height);

        context.DrawImage(
            map,
            new Rect(left, top, width, height),
            new Rect(0, 0, width * scale, height * scale));

        // **THE DOTS ARE DRAWN AT SCREEN SCALE**, so a small hover does not turn one
        // into a smear or a hairline.
        Point At(double x, double y)
            => new((x - left) * scale, (y - top) * scale);

        // **THE PATH IS A POLYLINE ALONG THE GREAT CIRCLE, NOT A STRAIGHT LINE**
        // (work instruction 308 task 3). Each run is one side of the date line; two
        // runs are never joined, because joining them would draw a horizontal stripe
        // across the picture describing a route nobody took.
        // **AND IT IS CASED** (work instruction 309 task 2). The casing goes down
        // first, whole, so the ink over it is unbroken by the joins - drawing the
        // two together segment by segment leaves a pale notch at every vertex, and
        // there are up to a hundred and eighty of them.
        var casing = new Pen(Casing, PathCasingWidth, lineCap: PenLineCap.Round);

        var ink = new Pen(
            Ink, PathStrokeWidth, new DashStyle(Dashes, 0), PenLineCap.Round);

        foreach (var pen in new[] { casing, ink })
        {
            foreach (var run in plot.Path)
            {
                for (var i = 1; i < run.Count; i++)
                {
                    context.DrawLine(
                        pen,
                        At(run[i - 1].X, run[i - 1].Y),
                        At(run[i].X, run[i].Y));
                }
            }
        }

        if (plot.HasOperator)
        {
            Dot(context, At(plot.OperatorX, plot.OperatorY), Mine);
        }

        if (plot.HasStation)
        {
            Dot(context, At(plot.StationX, plot.StationY), Theirs);
        }
    }

    /// <summary>The map, or null where this build cannot stand behind it.</summary>
    private static Bitmap? LoadTheMap()
    {
        try
        {
            var uri = new Uri(FlatWorldMap.Relief.Resource);

            using var stream = AssetLoader.Open(uri);
            using var memory = new MemoryStream();

            stream.CopyTo(memory);

            var bytes = memory.ToArray();

            if (!FlatWorldMap.Relief.Describes(bytes))
            {
                return null;
            }

            memory.Position = 0;

            return new Bitmap(memory);
        }
        catch (Exception)
        {
            // **A MISSING OR UNREADABLE MAP IS NO MAP** (§8's never-throw
            // discipline). Drawing nothing is the honest answer and the caption
            // beside it still says where the station is in words.
            return null;
        }
    }

    /// <summary>One station, drawn so it survives grayscale.</summary>
    /// <remarks>
    /// **THE TWO DOTS DIFFER BY MORE THAN A HUE** (§0.6): the operator's is a ring
    /// and the station's is filled, so a reader who cannot tell the colors apart can
    /// still tell which is which, and the caption names them in words as well.
    /// </remarks>
    private static void Dot(DrawingContext context, Point at, IBrush ink)
    {
        var filled = ReferenceEquals(ink, Theirs);

        // **CASED, FOR THE REASON THE PATH IS** (work instruction 309 task 2). The
        // operator marker is an amber ring and eastern North America is bright
        // orange on this photograph, so it was drawn and invisible. A pale ring just
        // outside it separates the marker from whatever it lands on.
        context.DrawEllipse(
            null,
            new Pen(Casing, MarkerStrokeWidth + 2.0),
            at,
            MarkerRadius,
            MarkerRadius);

        // **ONE IS A RING AND ONE IS FILLED, WHICH IS THE NON-COLOUR CARRIER**
        // (§0.6). A reader who cannot tell amber from green still knows which is
        // which, and so does a greyscale print.
        context.DrawEllipse(
            filled ? ink : null,
            new Pen(ink, MarkerStrokeWidth),
            at,
            MarkerRadius,
            MarkerRadius);
    }
}
