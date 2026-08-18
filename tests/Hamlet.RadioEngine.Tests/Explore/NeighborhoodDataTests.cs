using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// The cited neighborhood conventions (HM-DEC-054).
/// </summary>
/// <remarks>
/// THE EVENING THAT PUT THESE HERE. The operator tuned to 14.075, heard what he
/// described as whale song, and had no way to find out he was sitting in the
/// FT8 watering hole. The map called the whole of 14.000 to 14.150 Morse and
/// the card told him his license covered Morse there and invited him to call
/// away. Both statements were defensible about the regulation and wrong about
/// the world, and acting on either one would have put a Morse call into a wall
/// of digital signals that cannot hear it.
/// </remarks>
public sealed class NeighborhoodDataTests
{
    private static readonly NeighborhoodData Data = NeighborhoodData.Current;

    /// <remarks>
    /// Proves the file ships, parses, and covers every band the app draws. A
    /// map that is honest on 20 m and stale on 40 m is worse than one that is
    /// uniformly rough, because nobody can tell which band they are looking at.
    /// </remarks>
    [Fact]
    public void EveryBandTheAppDrawsHasCitedConventions()
    {
        Assert.NotEmpty(Data.Sources);
        Assert.False(string.IsNullOrWhiteSpace(Data.RetrievedUtc));

        foreach (var band in HfBands.Bands)
        {
            Assert.NotEmpty(Data.ForBand(band.Name));
        }
    }

    /// <remarks>
    /// THE ROW THAT WOULD HAVE PREVENTED THE EVENING. 14.074 is the FT8 dial
    /// frequency WSJT-X itself ships with, so it is where the whole world is
    /// tuned, and 14.075 sits inside the block it occupies.
    /// </remarks>
    [Theory]
    [InlineData(14_074_000)]
    [InlineData(14_075_000)]
    [InlineData(14_076_500)]
    public void TheFt8WateringHoleIsOnTheMapAsDigital(long hz)
    {
        var twenty = HfBands.Bands.Single(b => b.Name == "20 m");
        var here = NeighborhoodPlan.ForBand(twenty).Single(n => n.Contains(hz));

        Assert.Equal(ModeFamily.Digital, here.Family);
        Assert.Contains("FT8", here.Name, StringComparison.Ordinal);
        Assert.Equal("wsjtx-frequencies", here.Cite);
    }

    /// <remarks>
    /// Proves the other digital neighborhoods that were missing are there too,
    /// each on the band the brief named them on. One right answer and four
    /// still-blank blocks would leave the same trap two kilohertz away.
    /// </remarks>
    [Theory]
    [InlineData(14_070_000, "PSK31")]
    [InlineData(14_078_000, "JS8")]
    [InlineData(14_080_000, "FT4")]
    [InlineData(14_090_000, "RTTY")]
    public void TheDigitalNeighborhoodsAreAllOnTheMap(long hz, string label)
    {
        var twenty = HfBands.Bands.Single(b => b.Name == "20 m");
        var here = NeighborhoodPlan.ForBand(twenty).Single(n => n.Contains(hz));

        Assert.Equal(label, here.ShortName);
        Assert.Equal(ModeFamily.Digital, here.Family);
    }

    /// <remarks>
    /// EVERY ROW CARRIES ITS SOURCE, which is the rule that keeps a frequency
    /// from being written down because somebody remembered it. A neighborhood
    /// invented from memory is the prime directive broken in the data layer,
    /// where it is hardest to see and where it outlives everybody who could
    /// correct it (§0.0, §4).
    /// </remarks>
    [Fact]
    public void EveryCitedRowNamesASourceTheFileDeclares()
    {
        var known = Data.Sources.Select(s => s.Id).ToHashSet(StringComparer.Ordinal);

        Assert.NotEmpty(known);

        foreach (var band in HfBands.Bands)
        {
            foreach (var hood in Data.ForBand(band.Name))
            {
                Assert.False(
                    string.IsNullOrWhiteSpace(hood.Cite),
                    $"{band.Name} {hood.Name} carries no source");

                Assert.Contains(hood.Cite, known);
            }
        }
    }

    /// <remarks>
    /// Proves every row is describable as well as placed: a family, a name, and
    /// a story. A colored block with nothing to say is decoration that looks
    /// like information (§0.6).
    /// </remarks>
    [Fact]
    public void EveryRowHasAFamilyAndSomethingToSay()
    {
        foreach (var band in HfBands.Bands)
        {
            foreach (var hood in NeighborhoodPlan.ForBand(band))
            {
                Assert.False(string.IsNullOrWhiteSpace(hood.Name));
                Assert.False(string.IsNullOrWhiteSpace(hood.Vibe));
                Assert.True(hood.Blurb.Length > 40, $"{hood.Name} says almost nothing");
                Assert.Contains(hood.Family, Enum.GetValues<ModeFamily>());
            }
        }
    }

    /// <remarks>
    /// Proves what the file will not do. Several conventions could not be
    /// sourced this session, the slow-speed CW gathering places most of all,
    /// and those are recorded as declared unknowns with a reason rather than
    /// filled in with a plausible number (§4).
    /// </remarks>
    [Fact]
    public void WhatCouldNotBeSourcedIsDeclaredRatherThanGuessed()
    {
        Assert.NotEmpty(Data.Unknowns);

        Assert.All(Data.Unknowns, u =>
        {
            Assert.False(string.IsNullOrWhiteSpace(u.Topic));
            Assert.True(u.Reason.Length > 40, $"{u.Topic} gives no reason");
        });

        // The one that matters most to the operator this app is for. An earlier
        // map said 7.055 was the slow-speed club, and that number came from
        // nobody's source.
        Assert.Contains(
            Data.Unknowns,
            u => u.Topic.Contains("Slow-speed", StringComparison.OrdinalIgnoreCase));
    }

    /// <remarks>
    /// Proves the map still tiles every band it draws, gap-free and in order,
    /// after the cited rows are laid over it. The rows themselves have holes in
    /// them on purpose, because nobody publishes a convention for every
    /// kilohertz, and those holes are filled from the band's own structure
    /// rather than left as a map with pieces missing.
    /// </remarks>
    [Fact]
    public void TheMapStillTilesEveryBandWithoutGaps()
    {
        foreach (var band in HfBands.Bands)
        {
            var hoods = NeighborhoodPlan.ForBand(band);

            Assert.Equal(band.LowHz, hoods[0].LowHz);
            Assert.Equal(band.HighHz, hoods[^1].HighHz);

            for (var i = 1; i < hoods.Count; i++)
            {
                Assert.Equal(hoods[i - 1].HighHz + 1, hoods[i].LowHz);
            }
        }
    }

    /// <remarks>
    /// Proves a block whose crowd cannot hear Morse says so. The caution is a
    /// consequence and never an instruction, so it is swept for the imperative
    /// voice the same way the rest of the app's copy is (§0.7).
    /// </remarks>
    [Fact]
    public void ADigitalBlockSaysWhatAMorseCallThereWouldDo()
    {
        var twenty = HfBands.Bands.Single(b => b.Name == "20 m");
        var ft8 = NeighborhoodPlan.ForBand(twenty).Single(n => n.Contains(14_074_500));

        Assert.NotNull(ft8.Caution);
        Assert.Contains("cannot hear Morse", ft8.Caution, StringComparison.Ordinal);

        // Imperatives only. "will never know it arrived" is a consequence and
        // belongs; "you must not" would be operating somebody's radio for them.
        foreach (var scold in new[] { "you must", "you should", "do not ", "don't " })
        {
            Assert.DoesNotContain(scold, ft8.Caution, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <remarks>
    /// Proves the filled-in stretches are marked open rather than claimed. Below
    /// the phone segment the regulation allows Morse and the data modes alike,
    /// so painting an unclaimed stretch amber would say Morse owns ground nobody
    /// published a claim to (§0.6).
    /// </remarks>
    [Fact]
    public void FilledStretchesAreOpenGroundAndCarryNoCitation()
    {
        var filled = HfBands.Bands
            .SelectMany(NeighborhoodPlan.ForBand)
            .Where(h => h.Cite.Length == 0)
            .ToList();

        Assert.NotEmpty(filled);

        Assert.All(filled, h => Assert.True(
            h.Family is ModeFamily.Open or ModeFamily.Phone,
            $"{h.Name} claims {h.Family} for a stretch nobody published"));

        // Open ground has nothing to warn about; the voice end does, and it
        // says it for the same reason the cited phone rows do.
        Assert.All(
            filled.Where(h => h.Family == ModeFamily.Open),
            h => Assert.Null(h.Caution));

        Assert.All(
            filled.Where(h => h.Family == ModeFamily.Phone),
            h => Assert.NotNull(h.Caution));
    }
}
