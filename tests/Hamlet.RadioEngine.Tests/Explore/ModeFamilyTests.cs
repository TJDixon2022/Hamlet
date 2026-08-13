using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// The mode family is the single source the color comes from (HM-DEC-032).
/// </summary>
public sealed class ModeFamilyTests
{
    /// <remarks>
    /// Proves every neighborhood on every band resolves to exactly one family.
    /// A record is a family or it is not; there is no per-neighborhood color
    /// literal left to disagree with it, which is the whole point of the
    /// change.
    /// </remarks>
    [Fact]
    public void EveryNeighborhoodHasExactlyOneFamily()
    {
        var known = Enum.GetValues<ModeFamily>();

        foreach (var band in BandPlan.Bands)
        {
            var hoods = NeighborhoodPlan.ForBand(band);

            Assert.NotEmpty(hoods);

            foreach (var hood in hoods)
            {
                Assert.Contains(hood.Family, known);
            }
        }
    }

    /// <remarks>
    /// Proves the families are assigned by what actually lives there, not
    /// alphabetically or by position: the CW streets are CW, the FT8 and RTTY
    /// streets are digital, and the SSB streets are voice.
    /// </remarks>
    [Fact]
    public void FamiliesMatchWhatLivesInTheNeighborhood()
    {
        var fortyMeters = NeighborhoodPlan.ForBand(
            BandPlan.Bands.Single(b => b.Name == "40 m"));

        ModeFamily FamilyOf(string name)
            => fortyMeters.Single(h => h.Name == name).Family;

        Assert.Equal(ModeFamily.Cw, FamilyOf("CW fast lane"));
        Assert.Equal(ModeFamily.Cw, FamilyOf("CW main street"));
        Assert.Equal(ModeFamily.Digital, FamilyOf("Digital corner"));
        Assert.Equal(ModeFamily.Digital, FamilyOf("FT8 city"));
        Assert.Equal(ModeFamily.Open, FamilyOf("Quiet blocks"));
        Assert.Equal(ModeFamily.Phone, FamilyOf("Phone downtown"));
        Assert.Equal(ModeFamily.Phone, FamilyOf("Ragchew boulevard"));
    }

    /// <remarks>
    /// Proves all four families appear somewhere, so the legend never names a
    /// color the map cannot show.
    /// </remarks>
    [Fact]
    public void AllFourFamiliesAppearOnTheMap()
    {
        var seen = BandPlan.Bands
            .SelectMany(NeighborhoodPlan.ForBand)
            .Select(h => h.Family)
            .Distinct()
            .ToList();

        foreach (var family in Enum.GetValues<ModeFamily>())
        {
            Assert.Contains(family, seen);
        }
    }

    /// <remarks>
    /// Proves the field guide agrees with the map: a mode described in the
    /// guide carries the same family the map would color its neighborhood
    /// with, so a newcomer who learns "lavender means digital" from the legend
    /// finds the same lavender in the guide.
    /// </remarks>
    [Fact]
    public void FieldGuideAndMapAgree()
    {
        ModeFamily GuideFamily(string mode)
            => ModeGuide.Modes.Single(m => m.Name == mode).Family;

        Assert.Equal(ModeFamily.Cw, GuideFamily("CW"));
        Assert.Equal(ModeFamily.Digital, GuideFamily("FT8"));

        foreach (var mode in ModeGuide.Modes)
        {
            Assert.NotEqual(ModeFamily.Open, mode.Family);
        }
    }

    /// <remarks>
    /// Proves no neighborhood claims a family for space that is deliberately
    /// unclaimed. "Quiet blocks" is open ground, and coloring it as though a
    /// mode owned it would be an invention.
    /// </remarks>
    [Fact]
    public void UnclaimedSpaceIsMarkedOpen()
    {
        var quiet = NeighborhoodPlan
            .ForBand(BandPlan.Bands.Single(b => b.Name == "40 m"))
            .Single(h => h.Name == "Quiet blocks");

        Assert.Equal(ModeFamily.Open, quiet.Family);
        Assert.Equal("", quiet.ShortName);
    }
}
