using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
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

    static Ft8GlobeControl() => AffectsRender<Ft8GlobeControl>(PlotProperty);

    /// <summary>What is being plotted.</summary>
    public Ft8GlobePlot? Plot
    {
        get => GetValue(PlotProperty);
        set => SetValue(PlotProperty, value);
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

        var (left, top, width, height) = plot.Frame;

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
        var pen = new Pen(Ink, 1.4, new DashStyle(new double[] { 3, 2 }, 0));

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

        context.DrawEllipse(
            filled ? ink : null, new Pen(ink, 2), at, 4, 4);
    }
}
