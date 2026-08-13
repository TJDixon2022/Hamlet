using Hamlet.App.Settings;
using Hamlet.RadioEngine.Licensing;

namespace Hamlet.App.Licensing;

/// <summary>What a resolution attempt did.</summary>
public enum LicenseResolutionOutcome
{
    /// <summary>Nothing to do: no callsign, or the class is already known.</summary>
    NotNeeded,

    /// <summary>The class was unknown and is now set from a lookup.</summary>
    Resolved,

    /// <summary>The lookup disagrees with a hand-set class; the operator decides.</summary>
    Mismatch,

    /// <summary>The service answered and does not know this callsign.</summary>
    NotFound,

    /// <summary>The service could not be reached. Try again later.</summary>
    Unavailable,
}

/// <summary>The result of one resolution attempt.</summary>
/// <param name="Outcome">What happened.</param>
/// <param name="Found">The class the lookup reported, or Unknown.</param>
/// <param name="Existing">The class already on the profile, or Unknown.</param>
/// <param name="SourceName">Which service answered, or "".</param>
/// <param name="Narration">One line for the status bar.</param>
public sealed record LicenseResolution(
    LicenseResolutionOutcome Outcome,
    LicenseClass Found,
    LicenseClass Existing,
    string SourceName,
    string Narration)
{
    /// <summary>True when the operator has a decision to make.</summary>
    public bool NeedsOperatorDecision => Outcome == LicenseResolutionOutcome.Mismatch;
}

/// <summary>
/// Fills in the operator's license class quietly, whenever it is missing.
/// </summary>
/// <remarks>
/// <para>LAZY, NOT A WIZARD STEP (HM-DEC-028). People skip wizards, and the
/// callsign can arrive from Settings, a hand-edited settings file or an older
/// version that never asked. So resolution is attached to the fact rather
/// than to a screen: whenever there is a callsign and no class, this runs. It
/// never blocks and never opens a dialog; the status bar narrates and the
/// operator carries on.</para>
/// <para>A LOOKUP NEVER OVERWRITES A HAND-SET CLASS. If the operator chose
/// General and the FCC data says Extra, both are shown with the source and
/// date and the operator decides. It is their license; software that silently
/// corrected them would be wrong even on the occasions it was right.</para>
/// <para>Nobody is ever blocked. A service that is down leaves the class
/// unknown, the band map draws no overlay and says why, and Settings still
/// takes a hand-picked answer.</para>
/// </remarks>
public sealed class LicenseResolver
{
    private readonly ICallsignLookup _lookup;
    private readonly Func<DateTime> _utcNow;

    /// <summary>Creates the resolver.</summary>
    /// <param name="lookup">The lookup service.</param>
    /// <param name="utcNow">Clock, injected for testability.</param>
    public LicenseResolver(ICallsignLookup lookup, Func<DateTime>? utcNow = null)
    {
        _lookup = lookup;
        _utcNow = utcNow ?? (() => DateTime.UtcNow);
    }

    /// <summary>
    /// True when a profile is worth looking up: it has a callsign, and either
    /// no class or a class nothing has vouched for.
    /// </summary>
    /// <param name="profile">The operator profile.</param>
    /// <returns>True when a lookup should run.</returns>
    public static bool NeedsResolution(OperatorProfile profile)
        => !string.IsNullOrWhiteSpace(profile.Callsign)
           && profile.LicenseClass == LicenseClass.Unknown;

    /// <summary>
    /// The line the status bar shows while a lookup is in flight.
    /// </summary>
    /// <param name="callsign">The callsign being looked up.</param>
    /// <returns>Narration text.</returns>
    public static string LookingUpNarration(string callsign)
        => $"Looking up {callsign.Trim().ToUpperInvariant()}…";

    /// <summary>
    /// Resolve the class if it is missing, or detect a disagreement.
    /// </summary>
    /// <param name="profile">
    /// The profile. Written to only when the class was unknown — a hand-set
    /// class is left exactly as it was, whatever the lookup says.
    /// </param>
    /// <param name="cancellationToken">Cancels the lookup.</param>
    /// <returns>What happened, and what to narrate.</returns>
    public async Task<LicenseResolution> ResolveAsync(
        OperatorProfile profile, CancellationToken cancellationToken = default)
    {
        var callsign = (profile.Callsign ?? "").Trim();

        if (callsign.Length == 0)
        {
            return new LicenseResolution(
                LicenseResolutionOutcome.NotNeeded, LicenseClass.Unknown,
                profile.LicenseClass, "", "");
        }

        // A class that is already known and was looked up needs nothing. A
        // hand-set class is still checked, because a disagreement is worth
        // showing — but it is shown, never applied.
        if (profile.LicenseClass != LicenseClass.Unknown && !profile.LicenseClassWasSetByHand)
        {
            return new LicenseResolution(
                LicenseResolutionOutcome.NotNeeded, LicenseClass.Unknown,
                profile.LicenseClass, "", "");
        }

        CallsignLookupResult? result;
        try
        {
            result = await _lookup.LookupAsync(callsign, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            // Unreachable is a condition, not an error. The operator is not
            // blocked: Settings still takes a hand-picked class.
            return new LicenseResolution(
                LicenseResolutionOutcome.Unavailable, LicenseClass.Unknown,
                profile.LicenseClass, _lookup.SourceName,
                $"Couldn't reach {_lookup.SourceName}. Set your license class in "
                + "Settings, or try again later.");
        }

        return Apply(profile, result, _lookup.SourceName, callsign, _utcNow());
    }

    /// <summary>
    /// Apply an already-fetched lookup to the class on a profile.
    /// </summary>
    /// <param name="profile">The profile. Written to only when the class was
    /// unknown.</param>
    /// <param name="result">What the service said, or null when it does not
    /// know this callsign.</param>
    /// <param name="sourceName">Which service answered.</param>
    /// <param name="callsign">The callsign that was asked about.</param>
    /// <param name="utcNow">The moment to stamp on the provenance.</param>
    /// <returns>What happened, and what to narrate.</returns>
    /// <remarks>
    /// Split out from <see cref="ResolveAsync"/> so one HTTP call can serve
    /// both the class and the grid square (HM-DEC-037). Pure: no clock read,
    /// no network, so the mismatch rule that matters most is testable without
    /// either.
    /// </remarks>
    public static LicenseResolution Apply(
        OperatorProfile profile,
        CallsignLookupResult? result,
        string sourceName,
        string callsign,
        DateTime utcNow)
    {
        if (result is null)
        {
            return new LicenseResolution(
                LicenseResolutionOutcome.NotFound, LicenseClass.Unknown,
                profile.LicenseClass, sourceName,
                $"{sourceName} doesn't have {callsign.Trim().ToUpperInvariant()}. "
                + "Set your license class in Settings.");
        }

        if (profile.LicenseClass != LicenseClass.Unknown && !profile.LicenseClassWasSetByHand)
        {
            // Already resolved by a lookup. Nothing to say.
            return new LicenseResolution(
                LicenseResolutionOutcome.NotNeeded, result.Class,
                profile.LicenseClass, sourceName, "");
        }

        if (profile.LicenseClassWasSetByHand && result.Class != profile.LicenseClass)
        {
            // The one case that must not be automatic.
            return new LicenseResolution(
                LicenseResolutionOutcome.Mismatch, result.Class, profile.LicenseClass,
                result.SourceName,
                $"FCC data shows {PrivilegePlan.Describe(result.Class)}; you have "
                + $"{PrivilegePlan.Describe(profile.LicenseClass)} set.");
        }

        if (profile.LicenseClassWasSetByHand)
        {
            // They agree. Leave the operator's own answer standing — it is
            // still their statement, now corroborated.
            return new LicenseResolution(
                LicenseResolutionOutcome.NotNeeded, result.Class, profile.LicenseClass,
                result.SourceName, "");
        }

        profile.SetLicenseClass(
            result.Class, LicenseClassSource.LookedUp, result.SourceName, utcNow);

        return new LicenseResolution(
            LicenseResolutionOutcome.Resolved, result.Class, LicenseClass.Unknown,
            result.SourceName,
            $"{PrivilegePlan.Describe(result.Class)}, from FCC data, today.");
    }

    /// <summary>
    /// The provenance line shown beside the class in Settings and the status
    /// bar.
    /// </summary>
    /// <param name="profile">The operator profile.</param>
    /// <returns>Plain language, or "" when nothing is known.</returns>
    public static string DescribeProvenance(OperatorProfile profile)
    {
        if (profile.LicenseClass == LicenseClass.Unknown)
        {
            return "License class not set, so the band map will not guess at your privileges.";
        }

        var name = PrivilegePlan.Describe(profile.LicenseClass);
        var on = string.IsNullOrWhiteSpace(profile.LicenseClassSetOn)
            ? ""
            : $", {profile.LicenseClassSetOn}";

        return profile.LicenseClassSource switch
        {
            LicenseClassSource.LookedUp =>
                $"{name}, from {profile.LicenseClassSourceName}{on}.",
            LicenseClassSource.EnteredByOperator =>
                $"{name}, and you set this{on}.",
            _ => name,
        };
    }
}
