using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Hamlet.RadioEngine.Explore;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// Work instruction 306 task 2: **the delivered image's geometry, checked against
/// the image itself.**
/// </summary>
/// <remarks>
/// <para>**THE FIXTURE IS NOT BUILT FROM THE CODE'S OWN ASSUMPTIONS** (§12.5). The
/// fourteen pixel positions below were found by locating the white city dots in the
/// picture and matching them to published coordinates - **measured off the
/// photograph, not computed by the arithmetic under test.** That is what makes this
/// an acceptance test rather than a restatement.</para>
/// <para>**THE HASH IS LOAD-BEARING.** These constants describe one particular file.
/// A re-saved, resized or re-cropped image has different ones and would place every
/// station somewhere plausible and untrue, which is §0.0 broken by a picture and the
/// kind of fault nobody would ever check. So the geometry refuses to place anything
/// against a file whose bytes it does not recognise.</para>
/// <para>**A DEV-MACHINE MEASUREMENT IS NOT A FINDING ABOUT THE RADIO**
/// (`SHACK_FACTS.md` FACT-004). Nothing here touches a radio; what it asserts is
/// arithmetic against a file in the repository.</para>
/// </remarks>
public sealed class TheAzimuthalAssetTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the placements are printed.</param>
    public TheAzimuthalAssetTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Two pixels, and anything worse is a finding about the constants.**</summary>
    private const double Tolerance = 2.0;

    /// <summary>**The geometry knows which file it describes.**</summary>
    [Fact]
    public void TheGeometryMatchesTheDeliveredImageHash()
    {
        var bytes = File.ReadAllBytes(AssetPath());

        var hash = Convert
            .ToHexString(System.Security.Cryptography.SHA256.HashData(bytes))
            .ToLowerInvariant();

        _output.WriteLine("the file on disk : " + hash);
        _output.WriteLine("the record says  : " + AzimuthalMap.NorthPolar.Sha256);
        _output.WriteLine("bytes            : " + bytes.Length);

        Assert.Equal(AzimuthalMap.NorthPolar.Sha256, hash);

        // **AND THE RECORD ANSWERS FOR ITSELF**, so nothing has to remember to ask.
        Assert.True(
            AzimuthalMap.NorthPolar.Describes(bytes),
            "the shipped geometry does not recognise the shipped image");
    }

    /// <summary>**Fourteen city dots, each within two pixels of where it was found.**</summary>
    /// <param name="city">The city, for the failure message.</param>
    /// <param name="latitude">Its published latitude.</param>
    /// <param name="longitude">Its published longitude.</param>
    /// <param name="x">Where its dot was found in the image.</param>
    /// <param name="y">Where its dot was found in the image.</param>
    [Theory]
    [InlineData("London", 51.51, -0.13, 514.5, 105.5)]
    [InlineData("Istanbul", 41.01, 28.98, 409.0, 46.5)]
    [InlineData("Tehran", 35.69, 51.39, 306.0, 57.0)]
    [InlineData("Karachi", 24.86, 67.00, 209.5, 69.0)]
    [InlineData("Kolkata", 22.57, 88.36, 135.0, 167.5)]
    [InlineData("Beijing", 39.90, 116.41, 202.5, 318.5)]
    [InlineData("Shanghai", 31.23, 121.47, 163.5, 349.5)]
    [InlineData("Taipei", 25.03, 121.56, 133.5, 357.5)]
    [InlineData("Seoul", 37.57, 126.98, 201.5, 367.5)]
    [InlineData("Osaka", 34.69, 135.50, 203.5, 409.5)]
    [InlineData("Tokyo", 35.68, 139.69, 217.5, 425.5)]
    [InlineData("Chicago", 41.88, -87.63, 681.5, 356.5)]
    [InlineData("Miami", 25.76, -80.19, 768.5, 339.0)]
    [InlineData("Bogota", 4.71, -74.07, 877.5, 310.5)]
    public void TheFourteenCityDotsLandWithinTwoPixels(
        string city, double latitude, double longitude, double x, double y)
    {
        var placed = AzimuthalMap.NorthPolar.Place(latitude, longitude);

        Assert.NotNull(placed);

        var (px, py) = placed!.Value;
        var off = Math.Sqrt(((px - x) * (px - x)) + ((py - y) * (py - y)));

        _output.WriteLine(
            city + ": placed at "
            + px.ToString("0.00", CultureInfo.InvariantCulture) + ", "
            + py.ToString("0.00", CultureInfo.InvariantCulture)
            + " against a dot found at "
            + x.ToString("0.0", CultureInfo.InvariantCulture) + ", "
            + y.ToString("0.0", CultureInfo.InvariantCulture)
            + " - out by " + off.ToString("0.00", CultureInfo.InvariantCulture)
            + " px");

        Assert.True(
            off <= Tolerance,
            city + " lands " + off.ToString("0.00", CultureInfo.InvariantCulture)
            + " px from its dot, past the " + Tolerance + " px tolerance");
    }

    /// <summary>**Placing refuses against a file the geometry does not describe.**</summary>
    /// <remarks>
    /// **A SWAPPED IMAGE PLACES STATIONS SILENTLY WRONG**, which is the one failure
    /// mode a picture has that a sentence does not: nobody checks a dot.
    /// </remarks>
    [Fact]
    public void PlacingRefusesWhenTheAssetHashDoesNotMatch()
    {
        var real = File.ReadAllBytes(AssetPath());
        var tampered = real.ToArray();

        // One byte, deep inside the pixel data, is the whole of the difference.
        tampered[^1] ^= 0xFF;

        _output.WriteLine("the real file is described  : "
            + AzimuthalMap.NorthPolar.Describes(real));

        _output.WriteLine("a one-byte change is not    : "
            + AzimuthalMap.NorthPolar.Describes(tampered));

        Assert.True(AzimuthalMap.NorthPolar.Describes(real));
        Assert.False(AzimuthalMap.NorthPolar.Describes(tampered));
    }

    /// <summary>**The same projection the rest of the tree is proved against.**</summary>
    /// <remarks>
    /// **THERE IS NOT A SECOND PROJECTION** (the instruction forbids one, and it
    /// would be a second thing to drift). The image record is the generic
    /// <see cref="AzimuthalMap.Place(AzimuthalMap.Settings, double, double)"/>
    /// centred on the pole, rotated onto the picture and translated to its centre
    /// pixel; this asserts the two agree.
    /// </remarks>
    [Fact]
    public void TheImagePlacementIsTheGenericProjectionRotated()
    {
        var settings = new AzimuthalMap.Settings(
            CentreLatitude: 90,
            CentreLongitude: 0,
            RimRadiusPixels: 180.0 * AzimuthalMap.NorthPolar.PixelsPerDegree);

        foreach (var (lat, lon) in new[]
        {
            (51.51, -0.13), (35.68, 139.69), (4.71, -74.07), (41.88, -87.63),
        })
        {
            var (u, v) = AzimuthalMap.Place(settings, lat, lon);

            var turn = AzimuthalMap.NorthPolar.RotationDegrees * Math.PI / 180.0;
            var s = Math.Sin(turn);
            var c = Math.Cos(turn);

            var expectedX = AzimuthalMap.NorthPolar.CentreX + (s * u) + (c * v);
            var expectedY = AzimuthalMap.NorthPolar.CentreY - (c * u) + (s * v);

            var (x, y) = AzimuthalMap.NorthPolar.Place(lat, lon)!.Value;

            _output.WriteLine(
                lat.ToString("0.00", CultureInfo.InvariantCulture) + ", "
                + lon.ToString("0.00", CultureInfo.InvariantCulture)
                + " -> image " + x.ToString("0.0000", CultureInfo.InvariantCulture)
                + ", " + y.ToString("0.0000", CultureInfo.InvariantCulture)
                + " | generic rotated "
                + expectedX.ToString("0.0000", CultureInfo.InvariantCulture)
                + ", " + expectedY.ToString("0.0000", CultureInfo.InvariantCulture));

            Assert.Equal(expectedX, x, 6);
            Assert.Equal(expectedY, y, 6);
        }
    }

    /// <summary>The delivered image, from the repository root.</summary>
    private static string AssetPath()
    {
        var here = new DirectoryInfo(AppContext.BaseDirectory);

        while (here is not null
               && !File.Exists(Path.Combine(here.FullName, "Hamlet.sln")))
        {
            here = here.Parent;
        }

        Assert.NotNull(here);

        return Path.Combine(
            here!.FullName, "assets", "azimuthal-north-polar.png");
    }
}
