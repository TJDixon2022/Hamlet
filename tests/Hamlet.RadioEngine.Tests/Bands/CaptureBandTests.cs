using Hamlet.RadioEngine.Bands;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Bands;

/// <summary>
/// The band a capture was made on is a fact about its frequency (HM-DEC-096,
/// phase 6).
/// </summary>
/// <remarks>
/// <para>**THREE COMMITTED CAPTURES CARRY A HEADER THAT DISAGREES WITH THEIR OWN
/// RIG BLOCK.** The header says 7.030 MHz and 40 m; the block four lines below
/// says 14.055 MHz. Two fields describing one fact from two sources is the
/// defect, and the wrong value is only the symptom: the header took the band
/// button the operator last pressed and the block took what the radio
/// reported.</para>
/// <para>These pin the lookup the sidecar now derives its band from, using the
/// exact frequencies involved, so the two can never disagree again without this
/// failing.</para>
/// </remarks>
public sealed class CaptureBandTests
{
    /// <remarks>
    /// Proves HM-DEC-096 phase 6: the frequency in those captures' rig blocks is
    /// 20 m, and nothing about it is 40 m.
    /// </remarks>
    [Fact]
    public void TheFrequencyInThoseCapturesIsTwentyMeters()
    {
        var band = HfBands.BandFor(14_055_000);

        Assert.NotNull(band);
        Assert.Equal("20 m", band!.Name);
        Assert.NotEqual("40 m", band.Name);
    }

    /// <remarks>
    /// Proves HM-DEC-096 phase 6: the frequency the headers claimed really is
    /// 40 m, which is what made the disagreement look plausible rather than
    /// obviously broken.
    /// </remarks>
    [Theory]
    [InlineData(7_030_000, "40 m")]
    [InlineData(7_011_900, "40 m")]
    [InlineData(14_055_000, "20 m")]
    [InlineData(14_074_000, "20 m")]
    [InlineData(3_550_000, "80 m")]
    public void EveryCapturedFrequencyNamesItsOwnBand(long hz, string expected)
    {
        Assert.Equal(expected, HfBands.BandFor(hz)?.Name);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-096 phase 6 and §0.0: **a frequency outside every band
    /// Hamlet knows is not quietly given the nearest one.** The sidecar says so
    /// instead, because a band name in a capture header is a claim about where
    /// the radio was.</para>
    /// </remarks>
    [Theory]
    [InlineData(5_000_000)]
    [InlineData(100_000)]
    [InlineData(30_000_000)]
    public void AFrequencyOnNoBandIsNotGivenOne(long hz)
    {
        Assert.Null(HfBands.BandFor(hz));
    }
}
