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
/// States are each several thousand miles wide. <see cref="StationGridResolved"/> is
/// false and the caption says Hamlet does not know where he is.</para>
/// <para>**A LINE THROUGH THE CENTRE OF THIS PROJECTION IS THE GREAT-CIRCLE PATH**
/// (work instruction 306). The old caveat said the opposite, correctly, about the
/// equirectangular coastline this replaced; carrying it onto an azimuthal
/// equidistant picture would be telling the operator something untrue about a
/// picture that is now right (§0.0). **What is said instead is where the picture
/// stops**: see <see cref="OffTheMap"/>.</para>
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
    /// <summary>
    /// **What the picture cannot draw, said in words rather than at the rim.**
    /// </summary>
    /// <remarks>
    /// **THIS REPLACED A CAVEAT ABOUT A FLAT MAP** (work instruction 306). The old
    /// sentence said the line was straight on a flat picture and a real signal curves
    /// away from it, which was true of the drawn coastline and is **not true of this
    /// one**: on an azimuthal equidistant projection a line through the centre is the
    /// great-circle path, and repeating the caveat would be telling the operator
    /// something untrue about a picture that is now right (§0.0).
    /// </remarks>
    public const string OffTheMap =
        "This map reaches from the North Pole to the equator, so there is nowhere on "
        + "it to draw him.";

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

        // **PLACED ON THE PICTURE, NOT ON A FLAT RECTANGLE** (work instruction 306).
        // `AzimuthalMap.NorthPolar.Place` answers null in two separate cases - past
        // the rim, which on this asset is the equator, and outside the cropped
        // bitmap - and **null means no marker at all** rather than one pushed to the
        // edge. A dot is a claim (§0.0).
        var mine = here is { } a ? Map.Place(a.Latitude, a.Longitude) : null;
        var his = there is { } b ? Map.Place(b.Latitude, b.Longitude) : null;

        HasOperator = mine is not null;
        HasStation = his is not null;

        // **A GRID THAT RESOLVED AND A PLACE ON THIS MAP ARE DIFFERENT FACTS**, and
        // the caption needs both: a station south of the equator did put a grid on
        // the air, and saying Hamlet does not know where he is would be untrue.
        StationGridResolved = there is not null;
        OperatorGridResolved = here is not null;

        if (mine is { } m)
        {
            (OperatorX, OperatorY) = m;
        }

        if (his is { } h)
        {
            (StationX, StationY) = h;
        }

        // **THE DISTANCE IS A FACT ABOUT TWO GRIDS AND NOT ABOUT THIS PICTURE.**
        // A station off the southern edge is still a measured number of miles away,
        // and the number is honest even where the marker cannot be drawn.
        Miles = here is { } from && there is { } to
            ? GridPath.MilesBetween(from, to)
            : null;

        Bearing = here is { } start && there is { } end
            ? GridPath.BearingDegrees(start, end)
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
            // **NOT KNOWING WHERE HE IS AND HAVING NOWHERE TO DRAW HIM ARE
            // DIFFERENT FACTS** (§0.0, and this was caught by a test rather than
            // reasoned about). A station south of the equator did put a grid on the
            // air; saying Hamlet does not know where he is would be untrue about a
            // station who told it.
            if (!StationGridResolved)
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

            // **THE COVERAGE LIMIT IS NAMED WHERE IT BITES**, so an absent marker
            // reads as this picture running out rather than as Hamlet being unsure.
            return HasStation ? said : said + " " + OffTheMap;
        }
    }

    /// <summary>The picture everything here is placed on.</summary>
    public static AzimuthalImage Map => AzimuthalMap.NorthPolar;

    /// <summary>Whether the station put a grid on the air at all.</summary>
    /// <remarks>
    /// **DIFFERENT FROM <see cref="HasStation"/>**, which is whether he can be drawn
    /// on this particular picture. A station in the southern hemisphere is known and
    /// unplaceable, and conflating the two would have the caption say Hamlet does not
    /// know where he is about a station who told it.
    /// </remarks>
    public bool StationGridResolved { get; }

    /// <summary>Whether the operator's own grid resolved at all.</summary>
    public bool OperatorGridResolved { get; }

    /// <summary>The initial bearing to the station, or null.</summary>
    public double? Bearing { get; }

    /// <summary>The window the map draws, fitted to whatever is known.</summary>
    /// <remarks>
    /// **IT IS THE WHOLE PICTURE SINCE UNIT 306.** It used to fit both points with a
    /// margin, which is right for a drawn coastline that can be zoomed and wrong for
    /// a photograph. <see cref="MapWidth"/>, <see cref="MapHeight"/>,
    /// <see cref="Margin"/> and <see cref="SmallestFrame"/> describe that older
    /// arrangement and are read by its tests; nothing in `src/` uses them now.
    /// </remarks>
    private (double, double, double, double) Framed()
    {
        // **THE WHOLE PICTURE, ALWAYS** (work instruction 306). The old frame zoomed
        // to fit two points on a drawn coastline, which a photograph cannot do
        // without either scaling badly or cropping the land the operator is reading.
        // **The map is the map**: it is small, both stations are on it, and a fixed
        // frame is one fewer thing that can put a dot somewhere it does not belong.
        return (0, 0, Map.WidthPixels, Map.HeightPixels);
    }

    /// <summary>The frame, as a reader sees it, for a test and for the record.</summary>
    public string FrameLine
        => string.Format(
            CultureInfo.InvariantCulture,
            "left {0:0.#}, top {1:0.#}, {2:0.#} by {3:0.#}",
            Frame.Left, Frame.Top, Frame.Width, Frame.Height);
}
