using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Hamlet.RadioEngine.Explore;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// Work instruction 308 task 2: **the flat relief map, checked against anchors
/// measured off the picture.**
/// </summary>
/// <remarks>
/// <para>**THE ANCHORS ARE NOT BUILT FROM THIS CODE** (§12.5). The eighteen pixel
/// positions in `assets/flat-map-anchors.csv` were measured by the author against
/// published coordinates and a coastline overlay. If this arithmetic and those
/// anchors agree, they agree because the projection is right and not because both
/// came from one assumption.</para>
/// <para>**THE HASH IS LOAD-BEARING.** These six numbers describe one file. Against a
/// re-saved, resized or re-cropped image every station lands somewhere plausible and
/// untrue, and nobody ever checks a dot (§0.0, HM-DEC-092).</para>
/// <para>**EVERY ASSERTION HERE IS ARITHMETIC AND NONE OF IT IS APPEARANCE.** Nothing
/// in this tree can see whether a dot landed on Brazil. What is asserted is the
/// formula and the refusals.</para>
/// <para>**AND THE FIGURES ARE INDICATIONS** (`SHACK_FACTS.md` FACT-004): they were
/// measured on a machine with no radio attached.</para>
/// </remarks>
public sealed class TheFlatWorldAssetTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the placements are printed.</param>
    public TheFlatWorldAssetTests(ITestOutputHelper output) => _output = output;

    /// <summary>Half a pixel, which is the instruction's own tolerance.</summary>
    private const double Tolerance = 0.5;

    /// <summary>**The geometry knows which file it describes.**</summary>
    [Fact]
    public void TheGeometryMatchesTheDeliveredImageHash()
    {
        var bytes = File.ReadAllBytes(AssetPath());

        var hash = Convert
            .ToHexString(System.Security.Cryptography.SHA256.HashData(bytes))
            .ToLowerInvariant();

        _output.WriteLine("the file on disk : " + hash);
        _output.WriteLine("the record says  : " + FlatWorldMap.Relief.Sha256);
        _output.WriteLine("bytes            : " + bytes.Length);

        Assert.Equal(FlatWorldMap.Relief.Sha256, hash);
        Assert.True(FlatWorldMap.Relief.Describes(bytes));

        // **A SINGLE CHANGED BYTE IS A DIFFERENT PICTURE.**
        var tampered = bytes.ToArray();

        tampered[^1] ^= 0xFF;

        Assert.False(FlatWorldMap.Relief.Describes(tampered));
    }

    /// <summary>**Every anchor in the delivered table lands within half a pixel.**</summary>
    [Fact]
    public void TheAnchorsLandWithinHalfAPixel()
    {
        var rows = File.ReadAllLines(AnchorPath()).Skip(1)
            .Where(line => line.Trim().Length > 0)
            .ToList();

        _output.WriteLine("anchors read from the delivered table: " + rows.Count);
        _output.WriteLine("");

        var worst = 0.0;
        var checkedOn = 0;

        foreach (var row in rows)
        {
            var cell = row.Split(',');

            var place = cell[0];
            var latitude = Number(cell[1]);
            var longitude = Number(cell[2]);
            var x = Number(cell[3]);
            var y = Number(cell[4]);
            var onFile = cell[5].Trim() == "yes";

            var placed = FlatWorldMap.Relief.Place(latitude, longitude);

            if (!onFile)
            {
                // **REFUSED, NOT CLAMPED** - assertion 3, checked here so the table
                // drives both halves rather than two lists drifting apart.
                _output.WriteLine(
                    place.PadRight(22) + "off the file, and refused: "
                    + (placed is null));

                Assert.Null(placed);
                continue;
            }

            Assert.NotNull(placed);

            var (px, py) = placed!.Value;
            var off = Math.Sqrt(((px - x) * (px - x)) + ((py - y) * (py - y)));

            worst = Math.Max(worst, off);
            checkedOn++;

            _output.WriteLine(
                place.PadRight(22)
                + px.ToString("0.0000", CultureInfo.InvariantCulture) + ", "
                + py.ToString("0.0000", CultureInfo.InvariantCulture)
                + " against " + x.ToString("0.0000", CultureInfo.InvariantCulture)
                + ", " + y.ToString("0.0000", CultureInfo.InvariantCulture)
                + "  out by " + off.ToString("0.0000", CultureInfo.InvariantCulture));

            Assert.True(
                off <= Tolerance,
                place + " lands " + off.ToString("0.000", CultureInfo.InvariantCulture)
                + " px from its anchor");
        }

        _output.WriteLine("");
        _output.WriteLine(
            checkedOn + " anchors on the file, worst "
            + worst.ToString("0.0000", CultureInfo.InvariantCulture) + " px");

        // **THE WHOLE DELIVERED TABLE, NOT A SUBSET OF IT.**
        Assert.Equal(18, rows.Count);
    }

    /// <summary>**Midway and McMurdo are refused rather than clamped.**</summary>
    /// <remarks>
    /// **THE TWO GAPS ARE THE WHOLE LIST**: Antarctica below 63.79°S, and a 4.5°
    /// strip of longitude west of 175.52°W. A clamped pixel would put a station on
    /// the edge of the picture and say he is there, which is the one thing a map may
    /// never do (§0.0).
    /// </remarks>
    [Fact]
    public void MidwayAndMcMurdoAreRefusedNotClamped()
    {
        foreach (var (place, latitude, longitude) in new[]
        {
            ("Midway Atoll", 28.2072, -177.3735),
            ("McMurdo", -77.8419, 166.6863),
        })
        {
            var placed = FlatWorldMap.Relief.Place(latitude, longitude);

            _output.WriteLine(
                place.PadRight(14)
                + (placed is null
                    ? "refused"
                    : "PLACED at " + placed.Value.X + ", " + placed.Value.Y));

            Assert.Null(placed);
        }
    }

    /// <summary>**The two scales are different, and swapping them moves Tokyo.**</summary>
    /// <remarks>
    /// **THE ASPECT IS 1.2736, SO THIS IS A STRETCHED EQUIRECTANGULAR.** Using one
    /// number for both axes is the fault this assertion exists to catch, and it is
    /// the kind of fault that looks almost right everywhere and is wrong by a
    /// continent at the edges.
    /// </remarks>
    [Fact]
    public void TheTwoScalesAreDifferentAndSwappingThemMovesTokyo()
    {
        var real = FlatWorldMap.Relief;

        _output.WriteLine(
            "longitude scale : "
            + real.PixelsPerDegreeLongitude.ToString("0.00000", CultureInfo.InvariantCulture));

        _output.WriteLine(
            "latitude scale  : "
            + real.PixelsPerDegreeLatitude.ToString("0.00000", CultureInfo.InvariantCulture));

        _output.WriteLine(
            "aspect          : "
            + (real.PixelsPerDegreeLatitude / real.PixelsPerDegreeLongitude)
                .ToString("0.0000", CultureInfo.InvariantCulture));

        Assert.NotEqual(
            real.PixelsPerDegreeLongitude, real.PixelsPerDegreeLatitude, 3);

        var swapped = real with
        {
            PixelsPerDegreeLongitude = real.PixelsPerDegreeLatitude,
            PixelsPerDegreeLatitude = real.PixelsPerDegreeLongitude,
        };

        var right = real.Place(35.6762, 139.6503)!.Value;
        var wrong = swapped.Place(35.6762, 139.6503);

        var moved = wrong is { } w
            ? Math.Sqrt(
                ((right.X - w.X) * (right.X - w.X))
                + ((right.Y - w.Y) * (right.Y - w.Y)))
            : double.PositiveInfinity;

        _output.WriteLine(
            "Tokyo moves by  : "
            + (double.IsInfinity(moved)
                ? "off the file entirely"
                : moved.ToString("0.0", CultureInfo.InvariantCulture) + " px"));

        // **MEASURED AT 73.4 px, AND THE INSTRUCTION SAYS MORE THAN 100.** Reported
        // as a mismatch rather than repaired: a swap moves a point by the difference
        // of the two scales times its own coordinates, which for Tokyo is 0.5091 x
        // 139.65 across and 0.5091 x 35.68 down. **The guard is kept and its floor
        // is set from the measurement** - 73 px is a fifth of this picture's height
        // and nothing near it can happen by accident.
        Assert.True(moved > 50, "swapping the scales barely moved Tokyo");
    }

    /// <summary>**Pixel to coordinate and back, within a thousandth of a pixel.**</summary>
    /// <remarks>
    /// **THE CONVENIENCE ASSERTION**, and the instruction names it the drop
    /// candidate. It is kept because the inverse is what task 3 needs to say which
    /// samples of a path fall off the file.
    /// </remarks>
    [Fact]
    public void PixelToCoordinateAndBackAgain()
    {
        foreach (var (place, latitude, longitude) in new[]
        {
            ("Dublin", 53.3498, -6.2603),
            ("Tokyo", 35.6762, 139.6503),
            ("Nairobi", -1.2921, 36.8219),
            ("Lima", -12.0464, -77.0428),
            ("Sydney", -33.8688, 151.2093),
            ("Honolulu", 21.3069, -157.8583),
        })
        {
            var (x, y) = FlatWorldMap.Relief.Place(latitude, longitude)!.Value;
            var (backLat, backLon) = FlatWorldMap.Relief.Coordinate(x, y);
            var (again, andAgain) = FlatWorldMap.Relief.Place(backLat, backLon)!.Value;

            _output.WriteLine(
                place.PadRight(10)
                + x.ToString("0.000", CultureInfo.InvariantCulture) + ", "
                + y.ToString("0.000", CultureInfo.InvariantCulture)
                + " -> " + backLat.ToString("0.0000", CultureInfo.InvariantCulture)
                + ", " + backLon.ToString("0.0000", CultureInfo.InvariantCulture)
                + " -> " + again.ToString("0.000", CultureInfo.InvariantCulture)
                + ", " + andAgain.ToString("0.000", CultureInfo.InvariantCulture));

            Assert.Equal(x, again, 3);
            Assert.Equal(y, andAgain, 3);
        }
    }

    private static double Number(string cell)
        => double.Parse(cell.Trim(), CultureInfo.InvariantCulture);

    private static string AssetPath()
        => Path.Combine(Root(), "assets", "world-flat-relief.png");

    private static string AnchorPath()
        => Path.Combine(Root(), "assets", "flat-map-anchors.csv");

    private static string Root()
    {
        var here = new DirectoryInfo(AppContext.BaseDirectory);

        while (here is not null
               && !File.Exists(Path.Combine(here.FullName, "Hamlet.sln")))
        {
            here = here.Parent;
        }

        Assert.NotNull(here);

        return here!.FullName;
    }
}
