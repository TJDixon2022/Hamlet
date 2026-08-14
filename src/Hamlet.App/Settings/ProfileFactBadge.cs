using Hamlet.RadioEngine.Licensing;

namespace Hamlet.App.Settings;

/// <summary>What a profile field's badge says, if anything.</summary>
public enum FactBadgeState
{
    /// <summary>Nothing recorded, or the operator typed it. No badge.</summary>
    None,

    /// <summary>A lookup confirmed this exact value.</summary>
    Verified,

    /// <summary>A lookup reported something else. The operator decides.</summary>
    Differs,
}

/// <summary>The badge beside one profile field.</summary>
/// <param name="State">What it says, or that it says nothing.</param>
/// <param name="SourceName">Which service answered, or "".</param>
/// <param name="SetOn">The ISO date it answered, or "".</param>
/// <param name="TheirValue">What the service reported, for a disagreement.</param>
/// <param name="YourValue">What the operator has set, for a disagreement.</param>
public sealed record ProfileFactBadge(
    FactBadgeState State,
    string SourceName,
    string SetOn,
    string TheirValue,
    string YourValue)
{
    /// <summary>No badge at all.</summary>
    public static ProfileFactBadge None { get; } =
        new(FactBadgeState.None, "", "", "", "");

    /// <summary>True when there is a badge to draw.</summary>
    public bool IsVisible => State != FactBadgeState.None;

    /// <summary>True when the value matches what the service holds.</summary>
    public bool IsVerified => State == FactBadgeState.Verified;

    /// <summary>True when it does not.</summary>
    public bool Differs => State == FactBadgeState.Differs;

    /// <summary>
    /// The word on the pill.
    /// </summary>
    /// <remarks>
    /// A word, never only a check mark. Nothing in Hamlet may be knowable by
    /// color alone (§0.6), and a bare green tick beside a field would be
    /// exactly that.
    /// </remarks>
    public string Label => State switch
    {
        FactBadgeState.Verified => "verified",
        FactBadgeState.Differs => "differs from FCC data",
        _ => "",
    };

    /// <summary>
    /// What hovering the pill says.
    /// </summary>
    /// <remarks>
    /// "Verified" means this matches a public FCC record, and the tooltip says
    /// so in as many words. It is emphatically not a claim that the operator
    /// is who they say they are, and a badge that let somebody believe
    /// otherwise would be the confident decoration HM-DEC-009 forbids.
    /// </remarks>
    public string Tooltip => State switch
    {
        FactBadgeState.Verified =>
            $"Found on {SourceName}{Dated}. That means it matches the public FCC "
            + "record for this callsign. It is not a check that you are who you "
            + "say you are, and Hamlet has no way to do that.",

        FactBadgeState.Differs =>
            $"{SourceName} says {TheirValue}{Dated}, and you have {YourValue} set. "
            + "Nothing has been changed. Yours is the one Hamlet uses until you "
            + "say otherwise.",

        _ => "",
    };

    private string Dated => SetOn.Length == 0 ? "" : $", {SetOn}";
}

/// <summary>
/// Turns the provenance the profile already stores into the badge beside each
/// field.
/// </summary>
/// <remarks>
/// <para>DRIVEN ENTIRELY BY WHAT IS STORED (HM-DEC-044). Nothing here infers,
/// assumes or defaults. A field with no recorded source gets no badge, because
/// a check mark that does not correspond to a real lookup is precisely the
/// confident decoration the prime directive forbids (HM-DEC-009).</para>
/// <para>The badge is computed from the CURRENT value against the value a
/// lookup confirmed, rather than from a flag. That is what makes it clear
/// itself the moment somebody types, with nothing to remember to reset, and
/// what makes it still correct after a restart.</para>
/// <para>Pure: a profile in, a badge out. No clock, no UI, no I/O.</para>
/// </remarks>
public static class ProfileFacts
{
    /// <summary>The badge for the callsign field.</summary>
    /// <param name="profile">The operator profile.</param>
    /// <returns>The badge, or <see cref="ProfileFactBadge.None"/>.</returns>
    public static ProfileFactBadge Callsign(OperatorProfile profile)
        => ForText(
            profile.CallsignSource,
            profile.CallsignVerifiedAs,
            profile.Callsign,
            profile.CallsignSourceName,
            profile.CallsignSetOn);

    /// <summary>The badge for the grid square field.</summary>
    /// <param name="profile">The operator profile.</param>
    /// <returns>The badge, or <see cref="ProfileFactBadge.None"/>.</returns>
    public static ProfileFactBadge GridSquare(OperatorProfile profile)
        => ForText(
            profile.GridSquareSource,
            profile.GridSquareVerifiedAs,
            profile.GridSquare,
            profile.GridSquareSourceName,
            profile.GridSquareSetOn);

    /// <summary>The badge for the license class field.</summary>
    /// <param name="profile">The operator profile.</param>
    /// <returns>The badge, or <see cref="ProfileFactBadge.None"/>.</returns>
    /// <remarks>
    /// The class carries its own source enum from HM-DEC-028 rather than the
    /// shared one, so it is mapped here rather than sharing the text path.
    /// </remarks>
    public static ProfileFactBadge LicenseClass(OperatorProfile profile)
    {
        var verified = profile.LicenseClassVerifiedAs;

        if (verified == RadioEngine.Licensing.LicenseClass.Unknown)
        {
            // Nothing has ever been looked up, so there is nothing to say.
            return ProfileFactBadge.None;
        }

        var mine = profile.LicenseClass;

        if (mine == RadioEngine.Licensing.LicenseClass.Unknown)
        {
            return ProfileFactBadge.None;
        }

        return mine == verified
            ? new ProfileFactBadge(
                FactBadgeState.Verified,
                profile.LicenseClassSourceName,
                profile.LicenseClassSetOn,
                PrivilegePlan.Describe(verified),
                PrivilegePlan.Describe(mine))
            : new ProfileFactBadge(
                FactBadgeState.Differs,
                profile.LicenseClassSourceName,
                profile.LicenseClassSetOn,
                PrivilegePlan.Describe(verified),
                PrivilegePlan.Describe(mine));
    }

    /// <summary>
    /// The badge for a text field, from what was verified against what it
    /// says now.
    /// </summary>
    private static ProfileFactBadge ForText(
        ProfileFactSource source,
        string verifiedAs,
        string current,
        string sourceName,
        string setOn)
    {
        var verified = (verifiedAs ?? "").Trim();

        // No lookup has ever confirmed this field. Whether the operator typed
        // it or left it empty, there is nothing to show.
        if (verified.Length == 0)
        {
            return ProfileFactBadge.None;
        }

        var mine = (current ?? "").Trim();

        if (mine.Length == 0)
        {
            return ProfileFactBadge.None;
        }

        if (string.Equals(mine, verified, StringComparison.OrdinalIgnoreCase))
        {
            return new ProfileFactBadge(
                FactBadgeState.Verified, sourceName, setOn, verified, mine);
        }

        // The value differs from the confirmed one. That is a disagreement
        // only when the operator set it deliberately; a field a lookup wrote
        // and something else then changed is not a decision anybody made, so
        // it simply loses the badge.
        return source == ProfileFactSource.EnteredByOperator
            ? new ProfileFactBadge(
                FactBadgeState.Differs, sourceName, setOn, verified, mine)
            : ProfileFactBadge.None;
    }
}
