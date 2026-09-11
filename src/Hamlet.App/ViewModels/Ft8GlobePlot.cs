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
        "This map reaches almost everywhere, and he is in one of the two places it "
        + "does not - so there is nowhere on it to draw him.";

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
        _mineGrid = (operatorGrid ?? "").Trim();

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

        // **THE PATH IS SAMPLED, NOT DRAWN STRAIGHT** (work instruction 308 task 3).
        // A flat map only lies about direction if a straight line is drawn on it;
        // one hundred and eighty segments of the great circle, each projected, is
        // the true path on any projection. From FN00DJ to Tokyo that goes over
        // northern Alaska and the straight line goes across Spain.
        // **BOTH ENDS PLACED, NOT MERELY RESOLVED** (work instruction 309 task 2,
        // found by its own test). A station in Antarctica resolves perfectly well and
        // has nowhere on this picture, and the path was being built from his
        // coordinates anyway - so a line ran from the operator toward the bottom
        // edge and stopped where the samples fell off the file, pointing at a marker
        // that is deliberately not drawn. **A line to a place the picture says it
        // cannot show is the same claim as a dot there** (§0.0).
        Path = HasOperator && HasStation
            && here is { } pathFrom && there is { } pathTo
            ? GreatCirclePath.On(
                Map,
                pathFrom.Latitude, pathFrom.Longitude,
                pathTo.Latitude, pathTo.Longitude)
            : Array.Empty<IReadOnlyList<(double X, double Y)>>();

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

    /// <summary>The drawn path, in one run per side of the date line.</summary>
    /// <remarks>
    /// **EMPTY WHERE EITHER END IS UNKNOWN**, and split wherever the arc crosses the
    /// antimeridian or leaves the picture. Nothing here is labelled as a route or a
    /// bearing; it is where the signal goes.
    /// </remarks>
    public IReadOnlyList<IReadOnlyList<(double X, double Y)>> Path { get; }
        = Array.Empty<IReadOnlyList<(double X, double Y)>>();

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
    /// <remarks>
    /// <para>**THE FLAT RELIEF MAP SINCE UNIT 308** (Tim, 2026-09-10: *"use this one,
    /// make it work"*). The polar picture reached 40.4% of the globe and had no place
    /// at all for Nairobi, Buenos Aires, Sydney, Cape Town, Sao Paulo, Lima, Auckland
    /// or Honolulu. **This one reaches 93.7%**, and for an application whose whole
    /// purpose is to give a new operator somewhere new to chase, a map that cannot
    /// show South America was the wrong map.</para>
    /// <para>**THE POLAR RECORD STAYS IN THE TREE, GREEN AND UNTOUCHED.**
    /// <see cref="AzimuthalMap.NorthPolar"/> and its seventeen tests are not deleted;
    /// nothing here reads them.</para>
    /// </remarks>
    public static FlatWorldImage Map => FlatWorldMap.Relief;

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

    /// <summary>The operator's own grid, as it was handed in.</summary>
    private readonly string _mineGrid;

    /// <summary>What the operator's own marker says on a deliberate look.</summary>
    /// <remarks>
    /// **NOTHING ON THE FACE** (Tim: *"I want clean visual screens with text only
    /// where I, the user, intentionally hover"*). The marker is a ring on a
    /// photograph and says nothing until it is asked.
    /// </remarks>
    public string OperatorTip
        => HasOperator ? "You, in grid " + _mineGrid.ToUpperInvariant() + "." : "";

    /// <summary>What the station's marker says on a deliberate look.</summary>
    /// <remarks>
    /// <para>**THE DISTANCE AND A COMPASS WORD, NEVER A NUMBER OF DEGREES**
    /// (HM-DEC-038). *480 miles northeast* is a direction a person can picture;
    /// *480 miles at 47 degrees* is a reading off an instrument.</para>
    /// <para>**EACH PART IS ABSENT WHERE ITS FACT IS** (§0.0). No operator grid
    /// means no distance and no direction, because both are measured between two
    /// points and there is only one.</para>
    /// </remarks>
    public string StationTip
    {
        get
        {
            if (!HasStation)
            {
                return "";
            }

            var said = Callsign + ", in grid " + StationGrid + ".";

            if (Miles is { } miles && Bearing is { } bearing)
            {
                said += " " + GridPath.DescribeMiles(miles) + " "
                    + OperatorLocation.DescribeCompass(bearing) + " of you.";
            }

            return said;
        }
    }

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
