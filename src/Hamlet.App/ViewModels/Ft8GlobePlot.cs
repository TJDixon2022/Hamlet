using System;
using System.Globalization;
using Hamlet.RadioEngine.Explore;

namespace Hamlet.App.ViewModels;

/// <summary>
/// **Where two stations are on the coastline's own grid, and what the map should
/// frame.**
/// </summary>
/// <remarks>
/// <para>**THE PROJECTION IS THE COASTLINE'S AND THERE IS NOT A SECOND ONE**
/// (work instruction 299 task 3). `assets/world-coastline.svg` writes it into its
/// own `&lt;desc&gt;`:</para>
/// <para><c>x = (longitude + 180) * 2, y = (90 - latitude) * 2</c></para>
/// <para>**A DOT PLACED BY DIFFERENT ARITHMETIC FROM THE COASTLINE IS A DOT IN THE
/// WRONG PLACE**, and §0.0 binds a picture as hard as a sentence (HM-DEC-092). A map
/// is the one surface on which nobody would ever check.</para>
/// <para>**A STATION WITH NO GRID GETS NO DOT.** Not a guess from the callsign: a
/// DXCC entity is a country and a country is not a point, and Russia and the United
/// States are each several thousand miles wide. <see cref="HasStation"/> is false and
/// the caption says Hamlet does not know where he is.</para>
/// <para>**THE LINE IS STRAIGHT ON THIS PROJECTION AND THAT IS A SIMPLIFICATION.**
/// A real signal follows a great circle, which on an equirectangular map is a curve,
/// and a path near a pole looks nothing like a straight line. <see cref="Caveat"/>
/// says so and the caption carries it, so the picture never presents itself as the
/// path the signal took.</para>
/// </remarks>
public sealed class Ft8GlobePlot
{
    /// <summary>The coastline's own width, in its viewBox units.</summary>
    public const double MapWidth = 720;

    /// <summary>The coastline's own height, in its viewBox units.</summary>
    public const double MapHeight = 360;

    /// <summary>How much room to leave around the two points, in map units.</summary>
    /// <remarks>
    /// **A FRAME WITH NOTHING ROUND THE DOTS IS A PICTURE OF TWO DOTS.** Some
    /// coastline either side is what makes it a map, and the instruction asks the
    /// frame to fit both points **with a margin**.
    /// </remarks>
    public const double Margin = 40;

    /// <summary>The smallest frame the map is allowed to be, in map units.</summary>
    /// <remarks>
    /// **TWO STATIONS A HUNDRED MILES APART WOULD OTHERWISE ZOOM TO A FIELD.** At
    /// that scale this coastline has nothing in it to recognise, because it is a
    /// simplified outline rather than survey data, so the frame stops well before the
    /// picture stops meaning anything.
    /// </remarks>
    public const double SmallestFrame = 120;

    /// <summary>What the sentence says about the straight line, once.</summary>
    public const string Caveat =
        "The line is drawn straight on this flat map. A real signal follows a great "
        + "circle, which curves on a picture like this one, so the line says who is "
        + "where rather than the path the signal took.";

    /// <summary>Plot the two stations.</summary>
    /// <param name="operatorGrid">The operator's own locator, or null.</param>
    /// <param name="stationGrid">The station's locator, or null.</param>
    /// <param name="callsign">The station, for the caption.</param>
    /// <param name="place">Where he is in words, or empty.</param>
    public Ft8GlobePlot(
        string? operatorGrid, string? stationGrid, string callsign, string place)
    {
        Callsign = callsign;
        StationGrid = (stationGrid ?? "").Trim().ToUpperInvariant();

        var here = OperatorLocation.FromGrid(operatorGrid);
        var there = OperatorLocation.FromGrid(stationGrid);

        HasOperator = here is not null;
        HasStation = there is not null;

        if (here is { } from)
        {
            OperatorX = X(from.Longitude);
            OperatorY = Y(from.Latitude);
        }

        if (there is { } to)
        {
            StationX = X(to.Longitude);
            StationY = Y(to.Latitude);
        }

        Miles = here is { } a && there is { } b
            ? GridPath.MilesBetween(a, b)
            : null;

        Place = place;

        Frame = Framed();
    }

    /// <summary>The station.</summary>
    public string Callsign { get; }

    /// <summary>Its four-character locator, upper case, or "".</summary>
    public string StationGrid { get; }

    /// <summary>Where the station is in words, or "".</summary>
    public string Place { get; }

    /// <summary>True where the operator's own position is known.</summary>
    public bool HasOperator { get; }

    /// <summary>True where the station's position is known.</summary>
    public bool HasStation { get; }

    /// <summary>True where the map has anything to draw at all.</summary>
    public bool HasMap => HasOperator || HasStation;

    /// <summary>True where a line between the two can be drawn.</summary>
    public bool HasPath => HasOperator && HasStation;

    /// <summary>The operator's own place, in the coastline's units.</summary>
    public double OperatorX { get; }

    /// <summary>The operator's own place, in the coastline's units.</summary>
    public double OperatorY { get; }

    /// <summary>The station's place, in the coastline's units.</summary>
    public double StationX { get; }

    /// <summary>The station's place, in the coastline's units.</summary>
    public double StationY { get; }

    /// <summary>Great-circle distance, or null where either end is unknown.</summary>
    public double? Miles { get; }

    /// <summary>
    /// What the map is framed to, in the coastline's own units: left, top, width,
    /// height.
    /// </summary>
    public (double Left, double Top, double Width, double Height) Frame { get; }

    /// <summary>What the map says under it.</summary>
    /// <remarks>
    /// <para>**IT NAMES THE PLACE, THE GRID AND THE DISTANCE** (the instruction), and
    /// **each part is absent where its fact is** (§0.0).</para>
    /// <para>**AND IT CARRIES THE CAVEAT WHEREVER A LINE IS DRAWN**, so the picture
    /// is never presented as the path the signal took.</para>
    /// </remarks>
    public string Caption
    {
        get
        {
            if (!HasStation)
            {
                return $"Hamlet does not know where {Callsign} is. He has not put a "
                    + "grid square on the air, and a callsign only names a country, "
                    + "which is not a point on a map.";
            }

            var said = Place.Length > 0
                ? $"{Callsign} is in {Place}, grid {StationGrid}."
                : $"{Callsign} is in grid {StationGrid}.";

            if (Miles is { } miles)
            {
                said += " That is " + GridPath.DescribeMiles(miles) + " from you.";
            }
            else if (!HasOperator)
            {
                said += " Hamlet needs your own grid square in Settings before it "
                    + "can put you on the map beside him.";
            }

            return HasPath ? said + " " + Caveat : said;
        }
    }

    /// <summary>Longitude to the coastline's own x.</summary>
    /// <remarks>**THE `&lt;desc&gt;`'S OWN ARITHMETIC**, and nothing else's.</remarks>
    public static double X(double longitude) => (longitude + 180.0) * 2.0;

    /// <summary>Latitude to the coastline's own y.</summary>
    public static double Y(double latitude) => (90.0 - latitude) * 2.0;

    /// <summary>The window the map draws, fitted to whatever is known.</summary>
    /// <remarks>
    /// **IT FRAMES TO FIT BOTH POINTS WITH A MARGIN** (the instruction), stops
    /// shrinking at <see cref="SmallestFrame"/>, and is clamped inside the map so a
    /// station near the date line does not frame empty space off the edge.
    /// </remarks>
    private (double, double, double, double) Framed()
    {
        if (!HasMap)
        {
            return (0, 0, MapWidth, MapHeight);
        }

        var left = HasOperator ? OperatorX : StationX;
        var right = left;
        var top = HasOperator ? OperatorY : StationY;
        var bottom = top;

        if (HasStation)
        {
            left = Math.Min(left, StationX);
            right = Math.Max(right, StationX);
            top = Math.Min(top, StationY);
            bottom = Math.Max(bottom, StationY);
        }

        left -= Margin;
        right += Margin;
        top -= Margin;
        bottom += Margin;

        var width = Math.Max(right - left, SmallestFrame);
        var height = Math.Max(bottom - top, SmallestFrame * MapHeight / MapWidth);

        // **THE SAME SHAPE AS THE MAP**, so nothing is squashed: whichever axis needs
        // more room decides, and the other grows to match.
        var ratio = MapHeight / MapWidth;

        if (height < width * ratio)
        {
            var grew = width * ratio;

            top -= (grew - height) / 2;
            height = grew;
        }
        else
        {
            var grew = height / ratio;

            left -= (grew - width) / 2;
            width = grew;
        }

        width = Math.Min(width, MapWidth);
        height = Math.Min(height, MapHeight);

        left = Math.Clamp(left, 0, MapWidth - width);
        top = Math.Clamp(top, 0, MapHeight - height);

        return (left, top, width, height);
    }

    /// <summary>The frame, as a reader sees it, for a test and for the record.</summary>
    public string FrameLine
        => string.Format(
            CultureInfo.InvariantCulture,
            "left {0:0.#}, top {1:0.#}, {2:0.#} by {3:0.#}",
            Frame.Left, Frame.Top, Frame.Width, Frame.Height);
}
