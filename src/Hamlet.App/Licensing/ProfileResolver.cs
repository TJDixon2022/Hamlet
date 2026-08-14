using Hamlet.App.Settings;
using Hamlet.RadioEngine.Licensing;

namespace Hamlet.App.Licensing;

/// <summary>Everything one profile lookup settled.</summary>
/// <param name="License">What happened to the license class.</param>
/// <param name="Grid">What happened to the grid square.</param>
/// <param name="Unavailable">True when the service could not be reached.</param>
/// <param name="UnavailableNarration">What to say when it could not.</param>
public sealed record ProfileResolution(
    LicenseResolution License,
    GridResolution Grid,
    bool Unavailable,
    string UnavailableNarration)
{
    /// <summary>The line for the status bar, or "" when there is nothing to say.</summary>
    /// <remarks>
    /// One line, not two. Both facts are settled by the same request, and a
    /// status bar that fired twice in a second would read as a machine
    /// reporting on itself rather than as somebody telling you something.
    /// The class leads because it is the one with consequences.
    /// </remarks>
    public string Narration
    {
        get
        {
            if (Unavailable)
            {
                return UnavailableNarration;
            }

            if (License.Narration.Length > 0 && Grid.Narration.Length > 0)
            {
                return License.Narration + " " + Grid.Narration;
            }

            return License.Narration.Length > 0 ? License.Narration : Grid.Narration;
        }
    }
}

/// <summary>
/// Settles everything one callsign lookup can settle, in one request.
/// </summary>
/// <remarks>
/// <para>The license class and the grid square come out of the same callook
/// response, so asking twice would be two requests against somebody else's
/// free service for one answer (HM-DEC-024, HM-DEC-037). This makes the call
/// once and hands the result to each fact's own rules, which stay separate
/// because they are genuinely different rules — a class disagreement is about
/// what the FCC granted, and a grid disagreement is usually about where
/// somebody is standing today.</para>
/// <para>Neither fact is ever overwritten once the operator has set it by
/// hand. That is the whole of HM-DEC-028 applied a second time, and it applies
/// harder here: the FCC holds a mailing address, not an antenna.</para>
/// </remarks>
public sealed class ProfileResolver
{
    private readonly ICallsignLookup _lookup;
    private readonly Func<DateTime> _utcNow;

    /// <summary>Creates the resolver.</summary>
    /// <param name="lookup">The lookup service.</param>
    /// <param name="utcNow">Clock, injected for testability.</param>
    public ProfileResolver(ICallsignLookup lookup, Func<DateTime>? utcNow = null)
    {
        _lookup = lookup;
        _utcNow = utcNow ?? (() => DateTime.UtcNow);
    }

    /// <summary>
    /// True when anything on this profile is worth a lookup.
    /// </summary>
    /// <param name="profile">The operator profile.</param>
    /// <returns>True when a request should be made.</returns>
    /// <remarks>
    /// A hand-set class is still checked, because a disagreement is worth
    /// showing even though it is never applied. A hand-set grid is not: the
    /// operator's own answer about where they are is not something the FCC's
    /// address record gets to second-guess on every startup.
    /// </remarks>
    public static bool NeedsLookup(OperatorProfile profile)
        => LicenseResolver.NeedsResolution(profile)
           || GridResolver.NeedsResolution(profile)
           || profile.LicenseClassWasSetByHand
           || MissingItsReceipt(profile);

    /// <summary>
    /// True when the profile says a lookup happened but never recorded what it
    /// confirmed.
    /// </summary>
    /// <remarks>
    /// <para>Profiles written before HM-DEC-044 are in exactly this state:
    /// they know the class came from callook.info and cannot say which
    /// callsign or class was seen. The Settings badges are driven only by what
    /// is stored and never by inference, so rather than assuming the stored
    /// value is still the confirmed one, Hamlet asks again. One request, once,
    /// and the profile is whole.</para>
    /// <para>Backfilling from the current value would have been cheaper and
    /// would have been a guess wearing a check mark (HM-DEC-009).</para>
    /// </remarks>
    private static bool MissingItsReceipt(OperatorProfile profile)
        => !string.IsNullOrWhiteSpace(profile.Callsign)
           && string.IsNullOrWhiteSpace(profile.CallsignVerifiedAs)
           && (profile.LicenseClassSource == LicenseClassSource.LookedUp
               || profile.GridSquareSource == ProfileFactSource.LookedUp);

    /// <summary>
    /// The line the status bar shows while the lookup is in flight.
    /// </summary>
    /// <param name="callsign">The callsign being looked up.</param>
    /// <returns>Narration text.</returns>
    public static string LookingUpNarration(string callsign)
        => LicenseResolver.LookingUpNarration(callsign);

    /// <summary>
    /// Look the callsign up once and apply whatever it settles.
    /// </summary>
    /// <param name="profile">The profile, written to in place.</param>
    /// <param name="cancellationToken">Cancels the lookup.</param>
    /// <returns>What happened to each fact, and what to narrate.</returns>
    public async Task<ProfileResolution> ResolveAsync(
        OperatorProfile profile, CancellationToken cancellationToken = default)
    {
        var callsign = (profile.Callsign ?? "").Trim();

        if (callsign.Length == 0 || !NeedsLookup(profile))
        {
            return new ProfileResolution(
                new LicenseResolution(
                    LicenseResolutionOutcome.NotNeeded, LicenseClass.Unknown,
                    profile.LicenseClass, "", ""),
                GridResolution.None,
                false,
                "");
        }

        CallsignLookupResult? result;
        try
        {
            result = await _lookup.LookupAsync(callsign, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            // Unreachable is a condition, not an error. Nobody is blocked:
            // Settings still takes a hand-picked class and a typed grid.
            return new ProfileResolution(
                new LicenseResolution(
                    LicenseResolutionOutcome.Unavailable, LicenseClass.Unknown,
                    profile.LicenseClass, _lookup.SourceName, ""),
                GridResolution.None,
                true,
                $"Couldn't reach {_lookup.SourceName}. Set your license class in "
                + "Settings, or try again later.");
        }

        var now = _utcNow();

        // What the service actually said, recorded before anything decides
        // whether to adopt it. A hand-set class is never overwritten
        // (HM-DEC-028), and the Settings window still has to be able to say
        // what the FCC record holds (HM-DEC-044).
        if (result is not null)
        {
            profile.RecordLookup(result.Callsign, result.Class, result.SourceName, now);
        }

        // The class first: it is the one with legal consequences, and the grid
        // rules do not depend on its outcome.
        var license = LicenseResolver.Apply(
            profile, result, _lookup.SourceName, callsign, now);

        var grid = GridResolver.Apply(profile, result, now);

        return new ProfileResolution(license, grid, false, "");
    }
}
