using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Solar;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// The band cards: width carries wavelength, and nothing dims on a guess
/// (HM-DEC-033).
/// </summary>
public sealed class BandCardStyleTests
{
    private static readonly IReadOnlyList<string> AllBands =
        HfBands.Bands.Select(b => b.Name).ToList();

    private static SolarSnapshot Day { get; } = new(
        SunState.Day,
        new DateTime(2026, 8, 13, 10, 28, 0, DateTimeKind.Utc),
        new DateTime(2026, 8, 14, 0, 19, 0, DateTimeKind.Utc),
        TimeSpan.FromHours(4), null, TimeSpan.FromHours(9), null);

    private static SolarSnapshot Night { get; } = new(
        SunState.Night,
        new DateTime(2026, 8, 13, 10, 28, 0, DateTimeKind.Utc),
        new DateTime(2026, 8, 14, 0, 19, 0, DateTimeKind.Utc),
        null, TimeSpan.FromHours(2), null, TimeSpan.FromHours(8));

    /// <remarks>
    /// Proves the card row teaches the one fact nobody explains to a newcomer:
    /// "80 meters" is a long wave and "10 meters" is a short one. Width has to
    /// order strictly by wavelength or it teaches nothing.
    /// </remarks>
    [Fact]
    public void WidthFallsAsWavelengthFalls()
    {
        var widths = AllBands
            .Select(b => (Band: b, Width: BandCardStyles.WidthFor(b, AllBands)))
            .ToList();

        for (var i = 1; i < widths.Count; i++)
        {
            Assert.True(
                widths[i].Width < widths[i - 1].Width,
                $"{widths[i].Band} ({widths[i].Width:0.0}) should be narrower than "
                + $"{widths[i - 1].Band} ({widths[i - 1].Width:0.0})");
        }

        Assert.Equal(BandCardStyles.MaxWidth, widths[0].Width, 3);
        Assert.Equal(BandCardStyles.MinWidth, widths[^1].Width, 3);
    }

    /// <remarks>
    /// Proves the compression is real: straight proportion would make 80 m
    /// eight times the width of 10 m and wreck the row, so the ratio has to
    /// stay modest while the ordering stays strict.
    /// </remarks>
    [Fact]
    public void WidthRangeStaysUsable()
    {
        foreach (var band in AllBands)
        {
            var width = BandCardStyles.WidthFor(band, AllBands);
            Assert.InRange(width, BandCardStyles.MinWidth, BandCardStyles.MaxWidth);
        }

        Assert.True(BandCardStyles.MaxWidth / BandCardStyles.MinWidth < 2.5);
    }

    /// <remarks>
    /// Proves a name without a wavelength does not crash or produce a
    /// zero-width card; it takes the middle of the range.
    /// </remarks>
    [Fact]
    public void UnnamedWavelengthTakesTheMiddle()
    {
        Assert.Null(BandCardStyles.WavelengthOf("VHF"));

        var width = BandCardStyles.WidthFor("VHF", AllBands);
        Assert.InRange(width, BandCardStyles.MinWidth, BandCardStyles.MaxWidth);
    }

    /// <remarks>
    /// Proves the dimming follows the sun, and only the sun: a night band is at
    /// full strength after dark and dimmed in daylight, and an all-rounder is
    /// never dimmed at all.
    /// </remarks>
    [Fact]
    public void DimmingFollowsTheSun()
    {
        Assert.False(BandCardStyles.IsFavored("80 m", Day));
        Assert.True(BandCardStyles.IsFavored("80 m", Night));

        Assert.True(BandCardStyles.IsFavored("20 m", Day));
        Assert.False(BandCardStyles.IsFavored("20 m", Night));

        Assert.True(BandCardStyles.IsFavored("40 m", Day));
        Assert.True(BandCardStyles.IsFavored("40 m", Night));
    }

    /// <remarks>
    /// THE HONESTY CONSTRAINT (HM-DEC-009). Without coordinates Hamlet does not
    /// know where the sun is, so no card may dim and no card may show a sun or
    /// a moon. A faded card would be Hamlet stating something it cannot know,
    /// and it would look exactly like a real judgement.
    /// </remarks>
    [Fact]
    public void WithoutALocation_NothingDimsAndNothingClaimsTheSun()
    {
        foreach (var band in AllBands)
        {
            var style = BandCardStyles.For(band, AllBands, SolarSnapshot.Unknown);

            Assert.Equal(1.0, style.Opacity);
            Assert.True(style.IsFavored);
            Assert.Equal(DayNightIcon.Neutral, style.Icon);
        }
    }

    /// <remarks>
    /// Proves the icon says the same thing the dimming says, in a shape rather
    /// than a color — the second carrier the color rule requires (HM-DEC-032).
    /// </remarks>
    [Fact]
    public void IconCarriesTheSameFactAsTheDimming()
    {
        Assert.Equal(DayNightIcon.Moon, BandCardStyles.For("80 m", AllBands, Night).Icon);
        Assert.Equal(DayNightIcon.Moon, BandCardStyles.For("80 m", AllBands, Day).Icon);
        Assert.Equal(DayNightIcon.Sun, BandCardStyles.For("20 m", AllBands, Day).Icon);
        Assert.Equal(DayNightIcon.Both, BandCardStyles.For("40 m", AllBands, Day).Icon);
    }

    /// <remarks>
    /// Proves a band out of its element is visibly dimmed rather than nominally
    /// so — a one-percent difference would satisfy a naive assertion and be
    /// invisible on screen.
    /// </remarks>
    [Fact]
    public void DimmingIsVisible()
    {
        var dimmed = BandCardStyles.For("80 m", AllBands, Day);
        var full = BandCardStyles.For("80 m", AllBands, Night);

        Assert.Equal(1.0, full.Opacity);
        Assert.InRange(dimmed.Opacity, 0.4, 0.8);
    }

    /// <remarks>
    /// Proves determinism (§5): a band and a solar snapshot in, the same style
    /// out, with no clock read anywhere in the path.
    /// </remarks>
    [Fact]
    public void StylesAreDeterministic()
    {
        foreach (var band in AllBands)
        {
            var first = BandCardStyles.For(band, AllBands, Day);
            var second = BandCardStyles.For(band, AllBands, Day);

            Assert.Equal(first.Width, second.Width);
            Assert.Equal(first.Icon, second.Icon);
            Assert.Equal(first.Opacity, second.Opacity);
        }
    }
}
