using System.Security.Cryptography;

namespace Hamlet.RadioEngine.Explore;

/// <summary>
/// **One flat world map, and every number that describes it.**
/// </summary>
/// <remarks>
/// <para>**IT SITS BESIDE <see cref="AzimuthalImage"/> RATHER THAN REPLACING IT.**
/// The polar record and the seventeen tests that prove it stay in the tree
/// untouched; this is a second picture with a second projection, and the two do not
/// share a number.</para>
/// <para>**THE TWO SCALES ARE NOT INTERCHANGEABLE.** The aspect is 1.2736, so this
/// is a vertically stretched equirectangular and using one number for both axes is
/// the fault that looks almost right everywhere and is wrong by a continent at the
/// edges. `TheFlatWorldAssetTests` swaps them deliberately and requires Tokyo to
/// move more than a hundred pixels.</para>
/// <para>**THE HASH IS LOAD-BEARING** (§0.0, HM-DEC-092). These numbers describe one
/// file; against a re-saved or re-cropped one every station lands somewhere plausible
/// and untrue, and nobody ever checks a dot.</para>
/// <para>**THE PALE CURVES ON THE OCEAN ARE ARTWORK, NOT A GRATICULE.** They do not
/// agree with this projection. Nothing reads them, aligns to them or labels them, and
/// Hamlet draws no tick, degree label or bearing readout on this picture - the same
/// rule the polar asset's printed degree ring gets, for the opposite reason.</para>
/// </remarks>
/// <param name="Resource">Where the image is loaded from.</param>
/// <param name="Sha256">The file's SHA-256, lower case hex.</param>
/// <param name="WidthPixels">The bitmap's width.</param>
/// <param name="HeightPixels">The bitmap's height.</param>
/// <param name="PixelsPerDegreeLongitude">Horizontal scale.</param>
/// <param name="PixelsPerDegreeLatitude">Vertical scale, which is not the same.</param>
/// <param name="OriginX">The pixel longitude zero falls on.</param>
/// <param name="OriginY">The pixel latitude zero falls on.</param>
public sealed record FlatWorldImage(
    string Resource,
    string Sha256,
    int WidthPixels,
    int HeightPixels,
    double PixelsPerDegreeLongitude,
    double PixelsPerDegreeLatitude,
    double OriginX,
    double OriginY)
{
    /// <summary>Whether these numbers describe this file.</summary>
    /// <param name="bytes">The image file's own bytes.</param>
    /// <returns>True only where the hash matches exactly.</returns>
    public bool Describes(byte[]? bytes)
        => bytes is not null
           && string.Equals(
               System.Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(),
               Sha256,
               System.StringComparison.Ordinal);

    /// <summary>Where a station lands, or null where this picture has no room.</summary>
    /// <param name="latitude">Degrees north.</param>
    /// <param name="longitude">Degrees east.</param>
    /// <returns>The pixel, or null.</returns>
    /// <remarks>
    /// <para>**A REFUSAL, NEVER A CLAMPED EDGE.** A pixel pinned to the border would
    /// say *he is there*, which is a claim nobody made. Two places on Earth fall
    /// outside this file: Antarctica below 63.79 degrees south, and a four-and-a-half
    /// degree strip of longitude west of 175.52 degrees west.</para>
    /// <para>**THE BOUNDS ARE THE BITMAP'S OWN**, so a change to the picture's size
    /// changes what is placeable without anything else having to be told.</para>
    /// </remarks>
    public (double X, double Y)? Place(double latitude, double longitude)
    {
        var x = OriginX + (PixelsPerDegreeLongitude * longitude);
        var y = OriginY - (PixelsPerDegreeLatitude * latitude);

        return x < 0 || y < 0 || x > WidthPixels - 1 || y > HeightPixels - 1
            ? null
            : (x, y);
    }

    /// <summary>What place a pixel is, which is <see cref="Place"/> run backwards.</summary>
    /// <param name="x">Pixels from the left.</param>
    /// <param name="y">Pixels from the top.</param>
    /// <returns>The latitude and longitude that pixel stands for.</returns>
    /// <remarks>
    /// **IT ANSWERS FOR ANY PIXEL, INCLUDING ONE OFF THE FILE**, because it is
    /// arithmetic rather than a claim about the picture. What refuses is
    /// <see cref="Place"/>, which is where a marker gets drawn or does not.
    /// </remarks>
    public (double Latitude, double Longitude) Coordinate(double x, double y)
        => ((OriginY - y) / PixelsPerDegreeLatitude,
            (x - OriginX) / PixelsPerDegreeLongitude);
}

/// <summary>
/// **The flat relief map Hamlet ships, and the numbers measured off it.**
/// </summary>
/// <remarks>
/// <para>**TIM RULED ON THE PICTURE ON 2026-09-10**: *"use this one, make it work."*
/// It supersedes the search for a full-disc polar image.</para>
/// <para>**THE PROJECTION WAS MEASURED, NOT ASSUMED.** The author fitted sixteen
/// candidate projections against `naturalearth_lowres` coastlines; equirectangular
/// with an independent latitude scale scored an intersection-over-union of **0.9391**
/// and the next best, patterson, scored 0.8173, stable to four decimal places across
/// three different polar clips. **That is an indication and not a finding**
/// (`SHACK_FACTS.md` FACT-004): it was measured on a machine with no radio.</para>
/// <para>**WHAT IT COVERS.** Longitude -175.52 to +180 and latitude -63.79 to +90 -
/// **93.7% of the globe**, against the polar asset's 40.4%. Two places have nowhere:
/// Antarctica, and a narrow strip of ocean west of the Hawaiian chain.</para>
/// </remarks>
public static class FlatWorldMap
{
    /// <summary>The shipped picture.</summary>
    public static FlatWorldImage Relief { get; } = new(
        Resource: "avares://Hamlet.App/Assets/world-flat-relief.png",
        Sha256: "b3164c2789570f55e7c20ce665901aa47f650f0d935936b4cf6d0ccb18d45796",
        WidthPixels: 698,
        HeightPixels: 381,
        PixelsPerDegreeLongitude: 1.8609739340135842,
        PixelsPerDegreeLatitude: 2.3700838951974585,
        OriginX: 326.6361941977651,
        OriginY: 228.8094706337587);
}
