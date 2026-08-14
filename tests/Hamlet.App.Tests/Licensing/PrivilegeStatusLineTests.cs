using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Licensing;
using Xunit;

namespace Hamlet.App.Tests.Licensing;

/// <summary>
/// The status line under the band map, and the upgrade ladder behind it
/// (HM-DEC-029).
/// </summary>
/// <remarks>
/// The tone is the feature. An operator who has never transmitted needs the
/// restriction stated as a fact and paired immediately with the thing that
/// removes the fear — that listening is never restricted — rather than
/// scolded at.
/// </remarks>
public sealed class PrivilegeStatusLineTests
{
    private static CwBand Forty => BandPlan.Bands.First(b => b.Name == "40 m");

    /// <remarks>
    /// Proves the status line says "yours to use" inside privileges, in the
    /// green family, with no upgrade prompt to nag about.
    /// </remarks>
    [Fact]
    public void StatusLine_InsidePrivileges_IsEncouraging()
    {
        var line = PrivilegeStatusLine.Build(
            new PrivilegePlan(), LicenseClass.General, 7_030_000, TransmitMode.Cw);

        Assert.Equal(PrivilegeTone.Yours, line.Tone);
        Assert.Contains("7.030 MHz", line.Headline, StringComparison.Ordinal);
        Assert.Contains("yours to use", line.Headline, StringComparison.Ordinal);
        Assert.Contains("Call away", line.Detail, StringComparison.Ordinal);
        Assert.Empty(line.UpgradePrompt);
    }

    /// <remarks>
    /// Proves the outside case explains rather than scolds: amber not red,
    /// the reason, the reassurance that listening is never restricted, and an
    /// invitation to see what an upgrade would open.
    /// </remarks>
    [Fact]
    public void StatusLine_OutsidePrivileges_ExplainsAndReassures()
    {
        var line = PrivilegeStatusLine.Build(
            new PrivilegePlan(), LicenseClass.Technician, 7_200_000, TransmitMode.Cw);

        Assert.Equal(PrivilegeTone.ListenOnly, line.Tone);
        Assert.Contains("listen all you like", line.Headline, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("don't transmit", line.Headline, StringComparison.OrdinalIgnoreCase);
        Assert.NotEmpty(line.Detail);
        Assert.Equal(PrivilegeStatusLine.ListeningIsNeverRestricted, line.Reassurance);
        Assert.Contains("General", line.UpgradePrompt, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves an unknown class produces the explanatory line and no claim —
    /// the UI half of "never draw a privilege overlay on a guessed class".
    /// </remarks>
    [Fact]
    public void StatusLine_UnknownClass_SaysSoAndClaimsNothing()
    {
        var line = PrivilegeStatusLine.Build(
            new PrivilegePlan(), LicenseClass.Unknown, 7_200_000, TransmitMode.Cw);

        Assert.Equal(PrivilegeTone.Unknown, line.Tone);
        Assert.Contains("unknown", line.Detail, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Settings", line.Detail, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("yours to use", line.Headline, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("don't transmit", line.Headline, StringComparison.OrdinalIgnoreCase);

        // And the reassurance still stands: listening is never restricted.
        Assert.Equal(PrivilegeStatusLine.ListeningIsNeverRestricted, line.Reassurance);
    }

    /// <remarks>
    /// Proves the upgrade ladder is concrete: what it opens on this band, in
    /// megahertz, rather than a percentage nobody can picture. Restriction
    /// becomes motivation.
    /// </remarks>
    [Fact]
    public void UpgradeLadder_NamesWhatItWouldOpen()
    {
        var lines = PrivilegeStatusLine.UpgradeLadder(
            new PrivilegePlan(), LicenseClass.Technician, Forty);

        Assert.NotEmpty(lines);
        Assert.Contains(lines, l => l.Contains("40 m", StringComparison.Ordinal));
        Assert.Contains(lines, l => l.Contains("MHz", StringComparison.Ordinal));
        Assert.Contains(lines, l => l.Contains("exam", StringComparison.OrdinalIgnoreCase));
    }

    /// <remarks>
    /// Proves an Extra is told there is nothing above them rather than being
    /// shown an empty panel.
    /// </remarks>
    [Fact]
    public void UpgradeLadder_TellsAnExtraTheyAreDone()
    {
        var lines = PrivilegeStatusLine.UpgradeLadder(
            new PrivilegePlan(), LicenseClass.Extra, Forty);

        Assert.Single(lines);
        Assert.Contains("every US privilege", lines[0], StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// THE CARD THAT SENT SOMEBODY INTO THE FT8 WATERING HOLE (HM-DEC-054). At
    /// 14.074 the license really does cover Morse, and 14.074 is where the
    /// entire world's FT8 traffic sits. The old card said "Call away", which is
    /// a true statement about the regulation and an invitation to key Morse
    /// into a wall of digital signals that cannot hear it. The legal fact stays,
    /// because it is what the operator asked; the invitation goes, and the map
    /// supplies what the regulation cannot.
    /// </remarks>
    [Fact]
    public void AtTheFt8WateringHoleTheCardStopsInvitingAMorseCall()
    {
        var twenty = BandPlan.Bands.First(b => b.Name == "20 m");
        var here = NeighborhoodPlan.ForBand(twenty).Single(n => n.Contains(14_075_000));

        var line = PrivilegeStatusLine.Build(
            new PrivilegePlan(), LicenseClass.General, 14_075_000, TransmitMode.Cw, here);

        // Still legal, and still said so.
        Assert.Equal(PrivilegeTone.Yours, line.Tone);
        Assert.Contains("license covers", line.Detail, StringComparison.Ordinal);
        Assert.NotEmpty(line.Citation);

        // And no longer an invitation.
        Assert.DoesNotContain("Call away", line.Detail, StringComparison.Ordinal);

        // The half the regulation could never supply.
        Assert.Contains("cannot hear Morse", line.Culture, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the invitation survives where it belongs. 7.030 is the QRP
    /// watering hole in the Morse segment, which is exactly where somebody
    /// should be told to go ahead, and a card that hedged everywhere would
    /// teach nothing at all.
    /// </remarks>
    [Fact]
    public void OnTheMorseSideTheCardStillSaysCallAway()
    {
        var forty = Forty;
        var here = NeighborhoodPlan.ForBand(forty).Single(n => n.Contains(7_030_000));

        var line = PrivilegeStatusLine.Build(
            new PrivilegePlan(), LicenseClass.General, 7_030_000, TransmitMode.Cw, here);

        Assert.Contains("Call away", line.Detail, StringComparison.Ordinal);
        Assert.Empty(line.Culture);
    }

    /// <remarks>
    /// Proves the cultural line is about the mode the operator would actually
    /// be sending. "The software here cannot hear Morse" is beside the point to
    /// somebody about to send FT8 in the FT8 block, and a warning that fires
    /// where it does not apply is a warning nobody reads.
    /// </remarks>
    [Fact]
    public void TheCulturalLineOnlySpeaksAboutMorse()
    {
        var twenty = BandPlan.Bands.First(b => b.Name == "20 m");
        var here = NeighborhoodPlan.ForBand(twenty).Single(n => n.Contains(14_075_000));

        var line = PrivilegeStatusLine.Build(
            new PrivilegePlan(), LicenseClass.General, 14_075_000, TransmitMode.Data, here);

        Assert.Empty(line.Culture);
    }

    /// <remarks>
    /// Proves nothing is claimed where the map has nothing to say. A card built
    /// without a neighborhood behaves exactly as it did before, which is what
    /// keeps the two halves independent.
    /// </remarks>
    [Fact]
    public void WithNoNeighborhoodTheCardSaysNothingCultural()
    {
        var line = PrivilegeStatusLine.Build(
            new PrivilegePlan(), LicenseClass.General, 7_030_000, TransmitMode.Cw);

        Assert.Empty(line.Culture);
        Assert.Contains("Call away", line.Detail, StringComparison.Ordinal);
    }
}
