using Hamlet.App.Settings;
using Hamlet.RadioEngine.Licensing;

namespace Hamlet.App.Licensing;

/// <summary>What a resolution attempt did.</summary>
public enum LicenceResolutionOutcome
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
public sealed record LicenceResolution(
    LicenceResolutionOutcome Outcome,
    LicenceClass Found,
    LicenceClass Existing,
    string SourceName,
    string Narration)
{
    /// <summary>True when the operator has a decision to make.</summary>
    public bool NeedsOperatorDecision => Outcome == LicenceResolutionOutcome.Mismatch;
}

/// <summary>
/// Fills in the operator's licence class quietly, whenever it is missing.
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
/// date and the operator decides. It is their licence; software that silently
/// corrected them would be wrong even on the occasions it was right.</para>
/// <para>Nobody is ever blocked. A service that is down leaves the class
/// unknown, the band map draws no overlay and says why, and Settings still
/// takes a hand-picked answer.</para>
/// </remarks>
public sealed class LicenceResolver
{
    private readonly ICallsignLookup _lookup;
    private readonly Func<DateTime> _utcNow;

    /// <summary>Creates the resolver.</summary>
    /// <param name="lookup">The lookup service.</param>
    /// <param name="utcNow">Clock, injected for testability.</param>
    public LicenceResolver(ICallsignLookup lookup, Func<DateTime>? utcNow = null)
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
           && profile.LicenceClass == LicenceClass.Unknown;

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
    public async Task<LicenceResolution> ResolveAsync(
        OperatorProfile profile, CancellationToken cancellationToken = default)
    {
        var callsign = (profile.Callsign ?? "").Trim();

        if (callsign.Length == 0)
        {
            return new LicenceResolution(
                LicenceResolutionOutcome.NotNeeded, LicenceClass.Unknown,
                profile.LicenceClass, "", "");
        }

        // A class that is already known and was looked up needs nothing. A
        // hand-set class is still checked, because a disagreement is worth
        // showing — but it is shown, never applied.
        if (profile.LicenceClass != LicenceClass.Unknown && !profile.LicenceClassWasSetByHand)
        {
            return new LicenceResolution(
                LicenceResolutionOutcome.NotNeeded, LicenceClass.Unknown,
                profile.LicenceClass, "", "");
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
            return new LicenceResolution(
                LicenceResolutionOutcome.Unavailable, LicenceClass.Unknown,
                profile.LicenceClass, _lookup.SourceName,
                $"Couldn't reach {_lookup.SourceName} — set your licence class in "
                + "Settings, or try again later.");
        }

        if (result is null)
        {
            return new LicenceResolution(
                LicenceResolutionOutcome.NotFound, LicenceClass.Unknown,
                profile.LicenceClass, _lookup.SourceName,
                $"{_lookup.SourceName} doesn't have {callsign.ToUpperInvariant()} — "
                + "set your licence class in Settings.");
        }

        if (profile.LicenceClassWasSetByHand && result.Class != profile.LicenceClass)
        {
            // The one case that must not be automatic.
            return new LicenceResolution(
                LicenceResolutionOutcome.Mismatch, result.Class, profile.LicenceClass,
                result.SourceName,
                $"FCC data shows {PrivilegePlan.Describe(result.Class)}; you have "
                + $"{PrivilegePlan.Describe(profile.LicenceClass)} set.");
        }

        if (profile.LicenceClassWasSetByHand)
        {
            // They agree. Leave the operator's own answer standing — it is
            // still their statement, now corroborated.
            return new LicenceResolution(
                LicenceResolutionOutcome.NotNeeded, result.Class, profile.LicenceClass,
                result.SourceName, "");
        }

        profile.SetLicenceClass(
            result.Class, LicenceClassSource.LookedUp, result.SourceName, _utcNow());

        return new LicenceResolution(
            LicenceResolutionOutcome.Resolved, result.Class, LicenceClass.Unknown,
            result.SourceName,
            $"{PrivilegePlan.Describe(result.Class)} — from FCC data, today.");
    }

    /// <summary>
    /// The provenance line shown beside the class in Settings and the status
    /// bar.
    /// </summary>
    /// <param name="profile">The operator profile.</param>
    /// <returns>Plain language, or "" when nothing is known.</returns>
    public static string DescribeProvenance(OperatorProfile profile)
    {
        if (profile.LicenceClass == LicenceClass.Unknown)
        {
            return "Licence class not set — the band map will not guess at your privileges.";
        }

        var name = PrivilegePlan.Describe(profile.LicenceClass);
        var on = string.IsNullOrWhiteSpace(profile.LicenceClassSetOn)
            ? ""
            : $", {profile.LicenceClassSetOn}";

        return profile.LicenceClassSource switch
        {
            LicenceClassSource.LookedUp =>
                $"{name} — from {profile.LicenceClassSourceName}{on}.",
            LicenceClassSource.EnteredByOperator =>
                $"{name} — you set this{on}.",
            _ => name,
        };
    }
}
