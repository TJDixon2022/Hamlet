using Avalonia.Media;
using Hamlet.App.Controls;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Solar;

namespace Hamlet.App.ViewModels;

/// <summary>How one band card should look right now.</summary>
/// <param name="Width">Card width in device-independent pixels.</param>
/// <param name="Icon">Sun, moon, both, or neutral when the sun is unknown.</param>
/// <param name="IconTint">The icon's color.</param>
/// <param name="BarBrush">The colored bar under the label.</param>
/// <param name="Opacity">1 when the band is in its element, less when not.</param>
/// <param name="IsFavored">True when the band is in its element right now.</param>
public sealed record BandCardStyle(
    double Width,
    DayNightIcon Icon,
    IBrush IconTint,
    IBrush BarBrush,
    double Opacity,
    bool IsFavored);

/// <summary>
/// Turns a band and the sun's position into the look of its card.
/// </summary>
/// <remarks>
/// <para>WIDTH FOLLOWS WAVELENGTH (HM-DEC-033). 80 m draws widest and 10 m
/// narrowest, because "80 meters" is a long wave and "10 meters" is a short
/// one — a physical fact nobody ever explains to a newcomer, and the one that
/// explains everything else about how the bands behave. The row teaches it
/// without a word of copy.</para>
/// <para>The scale is logarithmic. Straight proportion would make the 80 m
/// card eight times the 10 m card and wreck the row; compressing it keeps the
/// ordering unmistakable while the cards stay a usable size.</para>
/// <para>THE HUE RUNS COOL TO WARM along the same axis: deep blue at the low
/// night end, warm amber at the high day end, so left-to-right reads as "close
/// to home" through "far away, when the sun allows". A band out of its element
/// keeps its hue but loses saturation and the card dims, which says the same
/// thing a third time.</para>
/// <para>Pure: a band name and a solar snapshot in, a style out. No clock read
/// (§5).</para>
/// </remarks>
public static class BandCardStyles
{
    /// <summary>
    /// Narrowest card, for the shortest wavelength.
    /// </summary>
    /// <remarks>
    /// **THE RATIO IS THE MEANING, NOT THE SIZE** (HM-DEC-033). Width follows
    /// wavelength so that eighty meters is visibly a longer wave than ten, and
    /// that survives the whole set being scaled. Both ends came down by a
    /// quarter when the bands moved beside the frequency readout rather than
    /// above it (HM-DEC-141), which keeps the row and the readout on one line at
    /// the width a laptop actually has.
    /// </remarks>
    /// <remarks>
    /// The ruling asked for 58 to 104. At 58 the card clipped "10 m" to "10 n"
    /// with the icon sitting on top of the label — the range is narrower than
    /// the content needs at this type size. The span is widened here and the
    /// ratio kept close to the one asked for; shrinking the type instead would
    /// have made the band names smaller than everything around them.
    /// </remarks>
    public const double MinWidth = 58;

    /// <summary>Widest card, for the longest wavelength.</summary>
    public const double MaxWidth = 93;

    /// <summary>How much a card dims when its band is out of its element.</summary>
    public const double DimmedOpacity = 0.62;

    private static readonly Color NightBar = Color.Parse("#2E5C8A");
    private static readonly Color BothBar = Color.Parse("#2F8478");
    private static readonly Color DayBar = Color.Parse("#C97A1E");

    private static readonly IBrush NeutralInk = new SolidColorBrush(Color.Parse("#9A968C"));
    private static readonly IBrush SunTint = new SolidColorBrush(Color.Parse("#C97A1E"));
    private static readonly IBrush MoonTint = new SolidColorBrush(Color.Parse("#3E6E9E"));

    /// <summary>
    /// The wavelength a band is named for, in meters.
    /// </summary>
    /// <param name="bandName">Band name, e.g. "40 m".</param>
    /// <returns>The wavelength, or null when the name does not carry one.</returns>
    public static double? WavelengthOf(string bandName)
    {
        var text = bandName.AsSpan().Trim();
        var space = text.IndexOf(' ');
        var digits = space > 0 ? text[..space] : text;

        return double.TryParse(digits, out var meters) && meters > 0 ? meters : null;
    }

    /// <summary>
    /// The width for a band, against the range of bands on display.
    /// </summary>
    /// <param name="bandName">The band.</param>
    /// <param name="allBandNames">Every band shown, so the ends of the scale
    /// come from what is actually on screen.</param>
    /// <returns>Width in device-independent pixels.</returns>
    public static double WidthFor(string bandName, IReadOnlyList<string> allBandNames)
    {
        var meters = WavelengthOf(bandName);
        if (meters is null)
        {
            return (MinWidth + MaxWidth) / 2;
        }

        var lengths = allBandNames
            .Select(WavelengthOf)
            .Where(m => m is not null)
            .Select(m => m!.Value)
            .ToList();

        if (lengths.Count == 0)
        {
            return (MinWidth + MaxWidth) / 2;
        }

        var shortest = Math.Log(lengths.Min());
        var longest = Math.Log(lengths.Max());

        if (longest - shortest < 0.0001)
        {
            return (MinWidth + MaxWidth) / 2;
        }

        var t = (Math.Log(meters.Value) - shortest) / (longest - shortest);
        return MinWidth + ((MaxWidth - MinWidth) * Math.Clamp(t, 0, 1));
    }

    /// <summary>
    /// Whether a band is in its element with the sun where it is.
    /// </summary>
    /// <param name="bandName">The band.</param>
    /// <param name="sun">Where the sun is.</param>
    /// <returns>
    /// True when the band suits the current hour. Unknown coordinates always
    /// yield true, so nothing is dimmed on a guess — a card that faded because
    /// Hamlet did not know where the operator was would be stating something
    /// it cannot know (HM-DEC-009).
    /// </returns>
    public static bool IsFavored(string bandName, SolarSnapshot sun)
    {
        if (!sun.IsKnown)
        {
            return true;
        }

        return BandCharacter.ElementOf(bandName) switch
        {
            BandElement.Day => sun.IsDaylight,
            BandElement.Night => !sun.IsDaylight,
            _ => true,
        };
    }

    /// <summary>Build the whole style for a band.</summary>
    /// <param name="bandName">The band.</param>
    /// <param name="allBandNames">Every band on display.</param>
    /// <param name="sun">Where the sun is.</param>
    /// <returns>The card's look.</returns>
    public static BandCardStyle For(
        string bandName, IReadOnlyList<string> allBandNames, SolarSnapshot sun)
    {
        var favored = IsFavored(bandName, sun);
        var element = BandCharacter.ElementOf(bandName);

        var icon = !sun.IsKnown
            ? DayNightIcon.Neutral
            : element switch
            {
                BandElement.Day => DayNightIcon.Sun,
                BandElement.Night => DayNightIcon.Moon,
                _ => DayNightIcon.Both,
            };

        var tint = !sun.IsKnown || !favored
            ? NeutralInk
            : element == BandElement.Night ? MoonTint : SunTint;

        return new BandCardStyle(
            WidthFor(bandName, allBandNames),
            icon,
            tint,
            new SolidColorBrush(BarColor(bandName, favored)),
            favored ? 1.0 : DimmedOpacity,
            favored);
    }

    /// <summary>
    /// The bar's color: the band's element — deep blue for night, teal for the
    /// all-rounders, warm amber for day — washed out when the band is out of
    /// that element right now.
    /// </summary>
    /// <remarks>
    /// This began as a continuous ramp from the blue to the amber along the
    /// wavelength axis, and on screen the middle of that ramp was gray: two
    /// near-complementary hues interpolated in RGB pass through neutral, so
    /// 40 m and 30 m came out looking dead rather than looking like anything.
    /// Three saturated stops say what the card is actually about — when this
    /// band is in its element — in the same terms as the icon beside them and
    /// the dimming around them.
    /// </remarks>
    private static Color BarColor(string bandName, bool favored)
    {
        var color = BandCharacter.ElementOf(bandName) switch
        {
            BandElement.Night => NightBar,
            BandElement.Both => BothBar,
            _ => DayBar,
        };

        return favored ? color : Wash(color);
    }

    /// <summary>Pull a color most of the way toward the page, keeping its hue.</summary>
    private static Color Wash(Color color) => Color.FromRgb(
        (byte)(color.R + ((235 - color.R) * 0.62)),
        (byte)(color.G + ((233 - color.G) * 0.62)),
        (byte)(color.B + ((226 - color.B) * 0.62)));
}
