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

    /// <remarks>
    /// <para>Proves HM-DEC-091: **14.028 MHz is 20 metres and a capture header
    /// labelled it 40 m.** The lookup was never the fault, which is why this is
    /// pinned here and the repair is in the caller: the sidecar derived its
    /// frequency from one source and its band from another, so the two could
    /// disagree without either being wrong on its own terms.</para>
    /// </remarks>
    [Fact]
    public void TheFrequencyThatWasLabelledFortyMetresIsTwentyMetres()
    {
        Assert.Equal("20 m", HfBands.BandFor(14_028_000)?.Name);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-091 and §0: **every edge, taken from the band data
    /// itself rather than typed in again.** A test that retypes the boundaries
    /// is a second copy of them, and a second copy drifts. Both edges belong to
    /// their band and both neighbours of those edges do not, which is the pair of
    /// claims an off-by-one in the lookup would break.</para>
    /// </remarks>
    [Fact]
    public void EveryBandOwnsBothItsEdgesAndNeitherNeighbour()
    {
        Assert.NotEmpty(HfBands.Bands);

        foreach (var band in HfBands.Bands)
        {
            Assert.Equal(band.Name, HfBands.BandFor(band.LowHz)?.Name);
            Assert.Equal(band.Name, HfBands.BandFor(band.HighHz)?.Name);
            Assert.NotEqual(band.Name, HfBands.BandFor(band.LowHz - 1)?.Name);
            Assert.NotEqual(band.Name, HfBands.BandFor(band.HighHz + 1)?.Name);
        }
    }

    /// <remarks>
    /// Proves HM-DEC-091: the middle of every band Hamlet offers names that band,
    /// so the sweep above cannot pass by finding nothing anywhere.
    /// </remarks>
    [Fact]
    public void TheMiddleOfEveryBandNamesItsOwnBand()
    {
        foreach (var band in HfBands.Bands)
        {
            var middle = band.LowHz + ((band.HighHz - band.LowHz) / 2);

            Assert.Equal(band.Name, HfBands.BandFor(middle)?.Name);
        }
    }
}
