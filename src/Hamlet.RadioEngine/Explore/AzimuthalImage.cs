using System.Security.Cryptography;

namespace Hamlet.RadioEngine.Explore;

/// <summary>
/// **One raster map, and every number that describes it.**
/// </summary>
/// <remarks>
/// <para>**THESE BELONG TO ONE PARTICULAR FILE AND TO NO OTHER** (§0.0,
/// HM-DEC-092). A re-saved, resized or re-cropped image has different constants,
/// and using these against it places every station somewhere plausible and untrue.
/// **A wrong dot is the one fault nobody ever checks**, which is why the hash rides
/// here beside the geometry rather than in a comment.</para>
/// <para>**THE PROJECTION IS NOT A SECOND ONE.** <see cref="AzimuthalMap.Place"/>
/// is the tree's azimuthal equidistant arithmetic, proved against `GridPath` by
/// `AzimuthalMapTests` on trigonometry that shares no line of code with it. This
/// record centres it on the pole, turns it onto the picture's own orientation and
/// moves it to the picture's own centre pixel - three affine steps over one piece
/// of trigonometry, so a change to the projection cannot leave this behind.</para>
/// <para>**COVERAGE IS PART OF THE DESCRIPTION.** This picture's rim is the equator
/// and its disc is cropped by the frame, so a station can be off it in two separate
/// ways. Both are asked, and a station that is off it gets **no position at all** -
/// never an edge marker, never an approximation (§0.0).</para>
/// </remarks>
/// <param name="Resource">Where the image is loaded from.</param>
/// <param name="Sha256">The file's SHA-256, lower case hex.</param>
/// <param name="WidthPixels">The bitmap's width.</param>
/// <param name="HeightPixels">The bitmap's height.</param>
/// <param name="CentreX">The pixel the projection is centred on.</param>
/// <param name="CentreY">The pixel the projection is centred on.</param>
/// <param name="CentreLatitude">What that pixel is, in degrees north.</param>
/// <param name="PixelsPerDegree">Pixels per degree of arc from the centre.</param>
/// <param name="RotationDegrees">
/// How far the picture is turned, as the screen angle at zero longitude.
/// </param>
/// <param name="RimDegrees">How many degrees of arc the outer rim is.</param>
public sealed record AzimuthalImage(
    string Resource,
    string Sha256,
    int WidthPixels,
    int HeightPixels,
    double CentreX,
    double CentreY,
    double CentreLatitude,
    double PixelsPerDegree,
    double RotationDegrees,
    double RimDegrees)
{
    /// <summary>Whether these numbers describe this file.</summary>
    /// <param name="bytes">The image file's own bytes.</param>
    /// <returns>True only where the hash matches exactly.</returns>
    /// <remarks>
    /// **THE REFUSAL IS THE POINT.** A caller that cannot answer yes here places
    /// nothing, and says so, rather than drawing a dot it cannot stand behind.
    /// </remarks>
    public bool Describes(byte[]? bytes)
        => bytes is not null
           && string.Equals(
               System.Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(),
               Sha256,
               System.StringComparison.Ordinal);

    /// <summary>How many pixels of this image one half-turn of arc is.</summary>
    /// <remarks>
    /// **THE GENERIC PROJECTION MEASURES ITS RIM AT A HALF TURN** and this picture's
    /// rim is a quarter of one, so the two are related by this and not by being the
    /// same number. Keeping the conversion here is what stops a caller pairing this
    /// image's rim radius with the projection's meaning of the word.
    /// </remarks>
    public double HalfTurnPixels => 180.0 * PixelsPerDegree;

    /// <summary>Where a station lands on this image, or null where it cannot.</summary>
    /// <param name="latitude">Degrees north.</param>
    /// <param name="longitude">Degrees east.</param>
    /// <returns>The pixel, or null where this picture does not cover the place.</returns>
    /// <remarks>
    /// <para>**TWO SEPARATE WAYS TO BE OFF THIS PICTURE, AND BOTH ARE ASKED.** Past
    /// the rim is past the projection's coverage; outside the bitmap is the frame
    /// having cropped the disc. A place can be inside the disc and still not on the
    /// file.</para>
    /// <para>**NULL, NEVER A CLAMPED EDGE.** A dot on the rim would say *he is at
    /// the equator*, which is a claim nobody made.</para>
    /// </remarks>
    public (double X, double Y)? Place(double latitude, double longitude)
    {
        var settings = new AzimuthalMap.Settings(
            CentreLatitude, CentreLongitude: 0, RimRadiusPixels: HalfTurnPixels);

        var (u, v) = AzimuthalMap.Place(settings, latitude, longitude);

        // **PAST THE RIM IS OFF THE MAP.** The arc from the centre is what the
        // radius measures, and this picture stops at `RimDegrees` of it.
        if (System.Math.Sqrt((u * u) + (v * v)) > RimDegrees * PixelsPerDegree)
        {
            return null;
        }

        var turn = RotationDegrees * System.Math.PI / 180.0;
        var s = System.Math.Sin(turn);
        var c = System.Math.Cos(turn);

        var x = CentreX + (s * u) + (c * v);
        var y = CentreY - (c * u) + (s * v);

        // **AND THE DISC IS CROPPED**, so inside the projection is not the same
        // question as inside the file.
        return x < 0 || y < 0 || x >= WidthPixels || y >= HeightPixels
            ? null
            : (x, y);
    }
}
