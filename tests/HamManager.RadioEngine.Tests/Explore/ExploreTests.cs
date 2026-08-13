using HamManager.RadioEngine.Bands;
using HamManager.RadioEngine.Explore;
using Xunit;

namespace HamManager.RadioEngine.Tests.Explore;

public sealed class ExploreTests
{
    /// <remarks>Proves: 40 m neighborhoods tile the band — contiguous, in
    /// order, first edge to last edge — and every jump spot lands inside its
    /// own neighborhood. A map with gaps or teleporting doors is worse than
    /// no map.</remarks>
    [Fact]
    public void FortyMeterNeighborhoods_TileTheBand()
    {
        var band = BandPlan.Bands.First(b => b.Name == "40 m");
        var hoods = NeighborhoodPlan.ForBand(band);

        Assert.Equal(band.LowHz, hoods[0].LowHz);
        Assert.Equal(band.HighHz, hoods[^1].HighHz);
        for (var i = 1; i < hoods.Count; i++)
        {
            Assert.Equal(hoods[i - 1].HighHz, hoods[i].LowHz);
        }

        Assert.All(hoods, h => Assert.True(h.Contains(h.JumpHz), h.Name));
    }

    /// <remarks>Proves: every band gets a truthful map — never empty, always
    /// within the band — even before its editorial map is written.</remarks>
    [Fact]
    public void EveryBand_GetsAnHonestMap()
    {
        foreach (var band in BandPlan.Bands)
        {
            var hoods = NeighborhoodPlan.ForBand(band);
            Assert.NotEmpty(hoods);
            Assert.All(hoods, h =>
            {
                Assert.InRange(h.LowHz, band.LowHz, band.HighHz);
                Assert.InRange(h.HighHz, band.LowHz, band.HighHz);
                Assert.False(string.IsNullOrWhiteSpace(h.Blurb));
            });
        }
    }

    /// <remarks>Proves: every field-guide entry that claims a 40 m home
    /// actually points inside 40 m, so "take me where it lives" never
    /// teleports out of band.</remarks>
    [Fact]
    public void ModeGuide_HomesAreReal()
    {
        var band = BandPlan.Bands.First(b => b.Name == "40 m");
        foreach (var mode in ModeGuide.Modes.Where(m => m.LivesAt40mHz is not null))
        {
            Assert.InRange(mode.LivesAt40mHz!.Value, band.LowHz, band.HighHz);
        }
    }

    /// <remarks>Proves: fixture spots carry the honesty fields — a source
    /// label and a timestamp — and tune targets that land inside a known
    /// band. The prime directive applies to sample data too.</remarks>
    [Fact]
    public async Task FakeSpots_AreHonestAndTunable()
    {
        var spots = await new FakeActivitySource().GetSpotsAsync();

        Assert.NotEmpty(spots);
        Assert.All(spots, s =>
        {
            Assert.Equal("sample", s.Source);
            Assert.False(string.IsNullOrWhiteSpace(s.Story));
            Assert.NotNull(BandPlan.BandFor(s.FrequencyHz));
            Assert.True(s.HeardAtUtc <= DateTime.UtcNow);
        });
    }
}
