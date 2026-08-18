using Hamlet.RadioEngine.Bands;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Bands;

public sealed class BandPlanTests
{
    /// <remarks>Proves: every band is internally consistent — CW segment
    /// inside the band, jump spot inside the CW segment. A band plan that
    /// violates this would jump the user outside their own segment, which
    /// the mode line would then correctly flag: better caught here.</remarks>
    [Fact]
    public void EveryBand_CwSegmentAndJumpAreCoherent()
    {
        foreach (var b in HfBands.Bands)
        {
            Assert.True(b.LowHz < b.HighHz, b.Name);
            Assert.InRange(b.CwLowHz, b.LowHz, b.HighHz);
            Assert.InRange(b.CwHighHz, b.CwLowHz, b.HighHz);
            Assert.True(b.IsInCwSegment(b.JumpHz), $"{b.Name} jump outside CW segment");
        }
    }

    /// <remarks>Proves: bands are ordered lowest-first and non-overlapping —
    /// the invariant BandFor relies on.</remarks>
    [Fact]
    public void Bands_OrderedAndDisjoint()
    {
        for (var i = 1; i < HfBands.Bands.Count; i++)
        {
            Assert.True(HfBands.Bands[i - 1].HighHz < HfBands.Bands[i].LowHz);
        }
    }

    /// <remarks>Proves: BandFor resolves an in-band frequency and returns
    /// null between bands rather than guessing (§0.0).</remarks>
    [Fact]
    public void BandFor_ResolvesAndRefuses()
    {
        Assert.Equal("40 m", HfBands.BandFor(7_030_000)?.Name);
        Assert.Null(HfBands.BandFor(5_000_000));
    }

    /// <remarks>Proves: the advisor is deterministic per hour (§5) and always
    /// names bands that exist in the plan.</remarks>
    [Theory]
    [InlineData(2)]
    [InlineData(7)]
    [InlineData(13)]
    [InlineData(19)]
    [InlineData(22)]
    public void BestBets_DeterministicAndValid(int hour)
    {
        var first = HfBands.BestBets(hour);
        var second = HfBands.BestBets(hour);

        Assert.Equal(first, second);
        Assert.All(first, name => Assert.Contains(HfBands.Bands, b => b.Name == name));
    }

    /// <remarks>Proves: an out-of-range hour fails loud.</remarks>
    [Fact]
    public void BestBets_RejectsBadHour()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => HfBands.BestBets(24));
    }
}
