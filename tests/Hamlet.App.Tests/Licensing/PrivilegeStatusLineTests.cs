using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Bands;
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
            new PrivilegePlan(), LicenceClass.General, 7_030_000, TransmitMode.Cw);

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
            new PrivilegePlan(), LicenceClass.Technician, 7_200_000, TransmitMode.Cw);

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
            new PrivilegePlan(), LicenceClass.Unknown, 7_200_000, TransmitMode.Cw);

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
            new PrivilegePlan(), LicenceClass.Technician, Forty);

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
            new PrivilegePlan(), LicenceClass.Extra, Forty);

        Assert.Single(lines);
        Assert.Contains("every US privilege", lines[0], StringComparison.OrdinalIgnoreCase);
    }
}
