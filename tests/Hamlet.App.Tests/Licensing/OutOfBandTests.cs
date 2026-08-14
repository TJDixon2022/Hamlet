using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Licensing;
using Xunit;

namespace Hamlet.App.Tests.Licensing;

/// <summary>
/// Out of band, on every surface that speaks (HM-DEC-055).
/// </summary>
/// <remarks>
/// WHAT HAPPENED. The operator tuned to 14.350, the very top edge of 20 m, and
/// the card said "yours to use, call away". A little further up there is no
/// amateur spectrum at all, and the privilege overlay was reading "past the end
/// of my data" as "no restriction found". That inverts the meaning of the
/// silence, in the one place in this app where a confident error has legal
/// consequences (§0.0, HM-DEC-029).
/// </remarks>
public sealed class OutOfBandTests
{
    private static CwBand Twenty => BandPlan.Bands.First(b => b.Name == "20 m");

    /// <remarks>
    /// Proves the fact itself: above the top of 20 m is not amateur spectrum,
    /// and the band edge itself still is. The edge is inclusive because the
    /// regulation's is.
    /// </remarks>
    [Theory]
    [InlineData(14_350_000, true)]
    [InlineData(14_350_001, false)]
    [InlineData(14_360_000, false)]
    [InlineData(13_999_999, false)]
    [InlineData(14_000_000, true)]
    public void TheEdgeIsWhereTheRegulationPutsIt(long hz, bool amateur)
        => Assert.Equal(amateur, AmateurSpectrum.IsAmateur(hz));

    /// <remarks>
    /// Proves the standing says which edge was crossed. "You have gone past the
    /// top of 20 m" is something somebody can act on; "out of band" is not.
    /// </remarks>
    [Fact]
    public void ItSaysWhichEdgeWasCrossed()
    {
        var above = AmateurSpectrum.Describe(14_360_000);

        Assert.False(above.IsAmateur);
        Assert.Contains("past the top of 20 m", above.Detail, StringComparison.Ordinal);
        Assert.Equal("20 m", above.NearestBand?.Name);
        Assert.Equal("97.301", above.Citation);
    }

    /// <remarks>
    /// THE CARD NEVER INVITES A TRANSMISSION OFF A HAM BAND. Amber rather than
    /// red, because HM-DEC-029 explains and does not scold, and the sentence
    /// about listening is there because it is the one that removes the fear.
    /// </remarks>
    [Fact]
    public void TheCardExplainsInAmberAndNeverInvites()
    {
        var line = PrivilegeStatusLine.Build(
            new PrivilegePlan(), LicenseClass.General, 14_360_000, TransmitMode.Cw);

        Assert.Equal(PrivilegeTone.ListenOnly, line.Tone);
        Assert.Contains("not an amateur band", line.Headline, StringComparison.Ordinal);
        Assert.Contains("no amateur license", line.Detail, StringComparison.Ordinal);
        Assert.NotEmpty(line.Citation);

        // Listening is never restricted, anywhere, on any license.
        Assert.Contains("Listening here is fine", line.Reassurance, StringComparison.Ordinal);

        foreach (var invitation in new[] { "yours to use", "Call away", "license covers" })
        {
            Assert.DoesNotContain(invitation, line.Headline, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(invitation, line.Detail, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <remarks>
    /// Proves it does not depend on who is asking. An Extra holds every US
    /// privilege there is and still may not transmit on somebody else's
    /// allocation, so the answer is the same for all four classes and for an
    /// operator whose class Hamlet does not know.
    /// </remarks>
    [Theory]
    [InlineData(LicenseClass.Unknown)]
    [InlineData(LicenseClass.Technician)]
    [InlineData(LicenseClass.General)]
    [InlineData(LicenseClass.Extra)]
    public void NoLicenseClassMakesItAllRight(LicenseClass cls)
    {
        var line = PrivilegeStatusLine.Build(
            new PrivilegePlan(), cls, 14_360_000, TransmitMode.Cw);

        Assert.Equal(PrivilegeTone.ListenOnly, line.Tone);
        Assert.Contains("not an amateur band", line.Headline, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the map draws past the edge, so the wall is visible rather than
    /// being the end of the picture. An operator who cannot see the edge cannot
    /// learn where it is.
    /// </remarks>
    [Fact]
    public void TheMapDrawsBeyondBothEdges()
    {
        var map = NeighborhoodPlan.WithEdges(Twenty);

        Assert.True(map[0].LowHz < Twenty.LowHz);
        Assert.True(map[^1].HighHz > Twenty.HighHz);

        Assert.Equal(ModeFamily.OutsideTheBand, map[0].Family);
        Assert.Equal(ModeFamily.OutsideTheBand, map[^1].Family);

        // And nothing in between claims to be outside the band.
        Assert.All(
            map.Skip(1).Take(map.Count - 2),
            h => Assert.NotEqual(ModeFamily.OutsideTheBand, h.Family));
    }

    /// <remarks>
    /// THREE THINGS IT IS NOT, and each one is a different fact. Not the
    /// listen-only hatching, because "you may listen and not transmit" is true
    /// inside the band too. Not the open neutral, because open means unclaimed
    /// amateur space and this is not amateur space at all. And not any of the
    /// three mode families, because no mode lives here (§0.6).
    /// </remarks>
    [Fact]
    public void TheBeyondEdgeRegionIsItsOwnThingAndNotTheOpenNeutral()
    {
        var beyond = ModePalette.For(ModeFamily.OutsideTheBand);

        Assert.NotEqual(ModePalette.Open.Fill, beyond.Fill);
        Assert.NotEqual(ModePalette.Open.Ink, beyond.Ink);
        Assert.Contains(beyond, ModePalette.Legend);
        Assert.DoesNotContain(beyond, ModePalette.All);
    }

    /// <remarks>
    /// THE GRAYSCALE TEST (§0.6). Print the screen without color and the label
    /// still says what the region is, so the fill is never the only carrier.
    /// The label is on the block and the legend names it in words.
    /// </remarks>
    [Fact]
    public void TheRegionSaysWhatItIsInWordsAsWellAsColor()
    {
        var map = NeighborhoodPlan.WithEdges(Twenty);

        Assert.NotEmpty(map[^1].ShortName);
        Assert.Contains("not a ham band", map[^1].Name, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(
            "Not a ham band",
            ModePalette.For(ModeFamily.OutsideTheBand).Label);
    }

    /// <remarks>
    /// Proves the three surfaces agree, because they read one derivation
    /// (HM-DEC-046's pattern applied to the band edge). The dial tape used to
    /// say "OUTSIDE the CW segment" above the top of 20 m, which is true and
    /// wildly understates matters.
    /// </remarks>
    [Fact]
    public void EverySurfaceReadsTheSameFact()
    {
        const long above = 14_360_000;

        var standing = AmateurSpectrum.Describe(above);
        var card = PrivilegeStatusLine.Build(
            new PrivilegePlan(), LicenseClass.General, above, TransmitMode.Cw);
        var onMap = NeighborhoodPlan.WithEdges(Twenty).Single(h => h.Contains(above));

        Assert.False(standing.IsAmateur);
        Assert.Equal(PrivilegeTone.ListenOnly, card.Tone);
        Assert.Equal(ModeFamily.OutsideTheBand, onMap.Family);

        // And the same frequency inside the band agrees the other way.
        var inside = AmateurSpectrum.Describe(14_030_000);
        var insideCard = PrivilegeStatusLine.Build(
            new PrivilegePlan(), LicenseClass.General, 14_030_000, TransmitMode.Cw);

        Assert.True(inside.IsAmateur);
        Assert.Equal(PrivilegeTone.Yours, insideCard.Tone);
    }

    /// <remarks>
    /// NOTHING STOPS THE DIAL (HM-DEC-029). Tuning is never restricted and
    /// neither is receiving; the protection is the screen telling the truth. So
    /// every one of these is a statement and none of them is a gate, which is
    /// visible here as there being no method to call that would refuse.
    /// </remarks>
    [Fact]
    public void TheFactIsAStatementAndNeverAGate()
    {
        // Describing a frequency nowhere near a ham band still answers rather
        // than throwing or refusing.
        var broadcast = AmateurSpectrum.Describe(9_500_000);

        Assert.False(broadcast.IsAmateur);
        Assert.NotEmpty(broadcast.Detail);
        Assert.Contains("Listening", broadcast.Detail, StringComparison.Ordinal);
    }
}
