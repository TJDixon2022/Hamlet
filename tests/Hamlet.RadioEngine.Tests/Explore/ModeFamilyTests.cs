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

        // First rather than Single: a band can have two stretches of the same
        // character with something else between them, and 40 m does.
        ModeFamily FamilyOf(string name)
            => fortyMeters.First(h => h.Name == name).Family;

        Assert.Equal(ModeFamily.Cw, FamilyOf("CW fast lane"));
        Assert.Equal(ModeFamily.Cw, FamilyOf("CW main street"));
        Assert.Equal(ModeFamily.Cw, FamilyOf("QRP watering hole"));
        Assert.Equal(ModeFamily.Digital, FamilyOf("PSK31 ribbons"));
        Assert.Equal(ModeFamily.Digital, FamilyOf("FT8 city"));
        Assert.Equal(ModeFamily.Digital, FamilyOf("RTTY row"));
        Assert.Equal(ModeFamily.Open, FamilyOf("Open ground"));
        Assert.Equal(ModeFamily.Phone, FamilyOf("Phone downtown"));
        Assert.Equal(ModeFamily.Phone, FamilyOf("Ragchew boulevard"));
    }

    /// <remarks>
    /// Proves every region the legend names appears somewhere on the map, so
    /// the legend never explains a color nobody will ever see. The fifth entry
    /// is not a mode family at all: it is the spectrum either side of the band,
    /// which the map now draws so the edge can be learned (HM-DEC-055).
    /// </remarks>
    [Fact]
    public void EveryRegionTheLegendNamesAppearsOnTheMap()
    {
        var seen = BandPlan.Bands
            .SelectMany(NeighborhoodPlan.WithEdges)
            .Select(h => h.Family)
            .Distinct()
            .ToList();

        foreach (var family in Enum.GetValues<ModeFamily>())
        {
            Assert.Contains(family, seen);
        }

        // And the in-band map never claims a frequency is outside the band.
        Assert.DoesNotContain(
            ModeFamily.OutsideTheBand,
            BandPlan.Bands.SelectMany(NeighborhoodPlan.ForBand).Select(h => h.Family));
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
    /// unclaimed. Below the phone segment the regulation allows Morse and the
    /// data modes alike, so a stretch nobody published a convention for is open
    /// ground, and coloring it amber would say Morse owns space it does not
    /// (HM-DEC-054).
    /// </remarks>
    [Fact]
    public void UnclaimedSpaceIsMarkedOpen()
    {
        var open = NeighborhoodPlan
            .ForBand(BandPlan.Bands.Single(b => b.Name == "40 m"))
            .Where(h => h.Name == "Open ground")
            .ToList();

        Assert.NotEmpty(open);
        Assert.All(open, h => Assert.Equal(ModeFamily.Open, h.Family));
        Assert.All(open, h => Assert.Equal("", h.ShortName));

        // And it is filled rather than cited, which is what makes it open.
        Assert.All(open, h => Assert.Equal("", h.Cite));
    }
}
