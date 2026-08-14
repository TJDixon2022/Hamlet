using Hamlet.App.Licensing;
using Hamlet.App.Settings;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Licensing;
using Xunit;

namespace Hamlet.App.Tests.Settings;

/// <summary>
/// The verified badges: a direct rendering of stored provenance, and nothing
/// else (HM-DEC-044).
/// </summary>
public sealed class ProfileFactBadgeTests
{
    private static readonly DateTime Now = new(2026, 8, 13, 12, 0, 0, DateTimeKind.Utc);

    private static readonly LatLon Home = new(40.3782746, -79.7081649);

    /// <summary>Tim's profile as it stands after one successful lookup.</summary>
    private static OperatorProfile LookedUp()
    {
        var profile = new OperatorProfile
        {
            Callsign = "KC3QIS",
            OperatorName = "Tim",
            Location = "Trafford, PA",
        };

        profile.RecordLookup("KC3QIS", LicenseClass.General, "callook.info", Now);
        profile.SetLicenseClass(
            LicenseClass.General, LicenseClassSource.LookedUp, "callook.info", Now);
        profile.SetPositionFromLookup(Home, "callook.info", Now);

        return profile;
    }

    /// <remarks>
    /// THE HAND-VERIFICATION CASE. Proves the three facts callook confirmed on
    /// Tim's machine all carry the badge, and the two he typed himself do not.
    /// </remarks>
    [Fact]
    public void ALookedUpFactIsVerifiedAndATypedOneIsNot()
    {
        var profile = LookedUp();

        Assert.True(ProfileFacts.Callsign(profile).IsVerified);
        Assert.True(ProfileFacts.GridSquare(profile).IsVerified);
        Assert.True(ProfileFacts.LicenseClass(profile).IsVerified);

        // Name and Location are typed. They have no lookup behind them and no
        // badge, and there is deliberately no ProfileFacts entry for them.
        Assert.Equal("Tim", profile.OperatorName);
        Assert.Equal("Trafford, PA", profile.Location);
    }

    /// <remarks>
    /// NO SOURCE MEANS NO BADGE. Proves a fresh profile shows nothing at all.
    /// A check mark that does not correspond to a real lookup is the confident
    /// decoration HM-DEC-009 forbids.
    /// </remarks>
    [Fact]
    public void AFactWithNoRecordedSourceShowsNothing()
    {
        var profile = new OperatorProfile { Callsign = "KC3QIS" };

        Assert.False(ProfileFacts.Callsign(profile).IsVisible);
        Assert.False(ProfileFacts.GridSquare(profile).IsVisible);
        Assert.False(ProfileFacts.LicenseClass(profile).IsVisible);
    }

    /// <remarks>
    /// Proves a value the operator typed, with no lookup ever run, carries no
    /// badge. Setting something by hand is not verification.
    /// </remarks>
    [Fact]
    public void AHandSetValueAloneIsNotVerified()
    {
        var profile = new OperatorProfile { Callsign = "KC3QIS" };
        profile.SetGridByHand("FN00DJ", Now);
        profile.SetLicenseClass(
            LicenseClass.Extra, LicenseClassSource.EnteredByOperator, "", Now);

        Assert.False(ProfileFacts.GridSquare(profile).IsVisible);
        Assert.False(ProfileFacts.LicenseClass(profile).IsVisible);
    }

    /// <remarks>
    /// THE BADGE CLEARS AS YOU TYPE. Proves that changing the text drops the
    /// badge with no save and no flag reset, one character at a time, because
    /// it is computed from the value rather than from a stored boolean.
    /// </remarks>
    [Theory]
    [InlineData("KC3QI")]
    [InlineData("KC3QISX")]
    [InlineData("W1ABC")]
    [InlineData("")]
    public void EditingAVerifiedFieldClearsItsBadge(string typed)
    {
        var profile = LookedUp();
        Assert.True(ProfileFacts.Callsign(profile).IsVerified);

        profile.Callsign = typed;

        Assert.False(ProfileFacts.Callsign(profile).IsVerified);
    }

    /// <remarks>
    /// Proves the grid badge clears the same way, through the setter the
    /// Settings box actually calls on every keystroke.
    /// </remarks>
    [Fact]
    public void TypingInTheGridFieldClearsItsBadge()
    {
        var profile = LookedUp();
        Assert.True(ProfileFacts.GridSquare(profile).IsVerified);

        profile.SetGridByHand("FN00D", Now);

        Assert.False(ProfileFacts.GridSquare(profile).IsVerified);
    }

    /// <remarks>
    /// Proves typing the confirmed value back restores the badge. It says
    /// "this is what the FCC record holds", which is true again the moment the
    /// text matches, whatever route it took to get there.
    /// </remarks>
    [Fact]
    public void TypingTheVerifiedValueBackRestoresTheBadge()
    {
        var profile = LookedUp();

        profile.Callsign = "W1ABC";
        Assert.False(ProfileFacts.Callsign(profile).IsVerified);

        profile.Callsign = "kc3qis";
        Assert.True(ProfileFacts.Callsign(profile).IsVerified);
    }

    /// <remarks>
    /// THE DISAGREEMENT STATE. Proves a hand-set class that differs from what
    /// the lookup reported shows the amber pill rather than the green one, and
    /// carries both values so hovering can name them.
    /// </remarks>
    [Fact]
    public void AHandSetValueThatDiffersFromTheLookupSaysSo()
    {
        var profile = LookedUp();

        // The operator insists on Extra; callook said General.
        profile.SetLicenseClass(
            LicenseClass.Extra, LicenseClassSource.EnteredByOperator, "", Now);

        var badge = ProfileFacts.LicenseClass(profile);

        Assert.True(badge.Differs);
        Assert.False(badge.IsVerified);
        Assert.Contains("General", badge.Tooltip, StringComparison.Ordinal);
        Assert.Contains("Extra", badge.Tooltip, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the same for a text field: a grid the operator typed over a
    /// looked-up one is a disagreement rather than a blank.
    /// </remarks>
    [Fact]
    public void AHandTypedGridThatDiffersSaysSo()
    {
        var profile = LookedUp();
        profile.SetGridByHand("EM79", Now);

        var badge = ProfileFacts.GridSquare(profile);

        Assert.True(badge.Differs);
        Assert.Contains("FN00DJ", badge.Tooltip, StringComparison.Ordinal);
        Assert.Contains("EM79", badge.Tooltip, StringComparison.Ordinal);
    }

    /// <remarks>
    /// NOTHING IS KNOWABLE BY COLOR ALONE (§0.6). Proves every badge that
    /// shows carries a word saying what it means, so a green tick is never the
    /// only carrier.
    /// </remarks>
    [Fact]
    public void NoBadgeIsMeaningfulByColorAlone()
    {
        var verified = ProfileFacts.Callsign(LookedUp());

        var differing = LookedUp();
        differing.SetGridByHand("EM79", Now);

        foreach (var badge in new[] { verified, ProfileFacts.GridSquare(differing) })
        {
            Assert.True(badge.IsVisible);
            Assert.False(string.IsNullOrWhiteSpace(badge.Label));
            Assert.True(
                badge.Label.Any(char.IsLetter),
                $"the label carries no words: '{badge.Label}'");
            Assert.False(string.IsNullOrWhiteSpace(badge.Tooltip));
        }

        Assert.Equal("verified", verified.Label);
        Assert.Contains("differs", ProfileFacts.GridSquare(differing).Label, StringComparison.Ordinal);
    }

    /// <remarks>
    /// WHAT "VERIFIED" ACTUALLY CLAIMS. Proves the tooltip names the source and
    /// says plainly that this is a match against a public record rather than a
    /// check on the person. Letting somebody believe otherwise would be the
    /// confident overreach the prime directive forbids.
    /// </remarks>
    [Fact]
    public void TheTooltipSaysWhatVerifiedDoesAndDoesNotMean()
    {
        var badge = ProfileFacts.Callsign(LookedUp());

        Assert.Contains("callook.info", badge.Tooltip, StringComparison.Ordinal);
        Assert.Contains("2026-08-13", badge.Tooltip, StringComparison.Ordinal);
        Assert.Contains("FCC record", badge.Tooltip, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("who you say you are", badge.Tooltip, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves a lookup records what it SAW even when the profile refuses to
    /// adopt it. That is what lets the disagreement pill exist at all without
    /// the profile pretending to agree with the FCC (HM-DEC-028).
    /// </remarks>
    [Fact]
    public void ALookupRecordsWhatItSawWithoutAdoptingIt()
    {
        var profile = new OperatorProfile { Callsign = "KC3QIS" };
        profile.SetLicenseClass(
            LicenseClass.Extra, LicenseClassSource.EnteredByOperator, "", Now);

        profile.RecordLookup("KC3QIS", LicenseClass.General, "callook.info", Now);

        // Untouched.
        Assert.Equal(LicenseClass.Extra, profile.LicenseClass);
        Assert.True(profile.LicenseClassWasSetByHand);

        // But seen.
        Assert.Equal(LicenseClass.General, profile.LicenseClassVerifiedAs);
        Assert.True(ProfileFacts.LicenseClass(profile).Differs);
    }

    /// <remarks>
    /// Proves a profile that claims a lookup but never recorded what it
    /// confirmed asks again, rather than having a badge inferred for it. This
    /// is every profile written before this ruling, Tim's included.
    /// </remarks>
    [Fact]
    public void AProfileWithNoReceiptAsksAgain()
    {
        var old = new OperatorProfile { Callsign = "KC3QIS" };
        old.SetLicenseClass(
            LicenseClass.General, LicenseClassSource.LookedUp, "callook.info", Now);
        old.SetPositionFromLookup(Home, "callook.info", Now);
        old.CallsignVerifiedAs = "";

        Assert.True(ProfileResolver.NeedsLookup(old));

        // And once it has been asked, it stops asking.
        old.RecordLookup("KC3QIS", LicenseClass.General, "callook.info", Now);
        Assert.False(ProfileResolver.NeedsLookup(old));
    }

    /// <remarks>
    /// Proves an empty field never carries a badge, so a cleared box does not
    /// keep claiming it was checked.
    /// </remarks>
    [Fact]
    public void AnEmptyFieldCarriesNoBadge()
    {
        var profile = LookedUp();
        profile.SetGridByHand("", Now);

        Assert.False(ProfileFacts.GridSquare(profile).IsVisible);
    }

    /// <remarks>
    /// Proves the badge is pure (§5): the same profile always gives the same
    /// answer, with no clock read and nothing cached.
    /// </remarks>
    [Fact]
    public void BadgesAreDeterministic()
    {
        var profile = LookedUp();

        Assert.Equal(ProfileFacts.Callsign(profile), ProfileFacts.Callsign(profile));
        Assert.Equal(ProfileFacts.LicenseClass(profile), ProfileFacts.LicenseClass(profile));
    }
}
