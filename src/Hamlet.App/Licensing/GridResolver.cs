using Hamlet.App.Settings;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Licensing;

namespace Hamlet.App.Licensing;

/// <summary>What a grid-square resolution attempt did.</summary>
public enum GridResolutionOutcome
{
    /// <summary>Nothing to do: no callsign, or the grid is already known.</summary>
    NotNeeded,

    /// <summary>The grid was unknown and is now derived from a lookup.</summary>
    Resolved,

    /// <summary>The lookup disagrees with a hand-entered grid; the operator decides.</summary>
    Mismatch,

    /// <summary>The service answered but holds no coordinates for this callsign.</summary>
    NoCoordinates,
}

/// <summary>The result of one grid resolution attempt.</summary>
/// <param name="Outcome">What happened.</param>
/// <param name="Found">The locator derived from the lookup, or "".</param>
/// <param name="Existing">The locator already on the profile, or "".</param>
/// <param name="SourceName">Which service answered, or "".</param>
/// <param name="Narration">One line for the status bar, or "".</param>
public sealed record GridResolution(
    GridResolutionOutcome Outcome,
    string Found,
    string Existing,
    string SourceName,
    string Narration)
{
    /// <summary>Nothing happened and nothing needs saying.</summary>
    public static GridResolution None { get; } =
        new(GridResolutionOutcome.NotNeeded, "", "", "", "");

    /// <summary>True when the operator has a decision to make.</summary>
    public bool NeedsOperatorDecision => Outcome == GridResolutionOutcome.Mismatch;
}

/// <summary>
/// Fills in the operator's grid square from the coordinates the callsign
/// lookup already returns.
/// </summary>
/// <remarks>
/// <para>NOBODY SHOULD HAVE TO LOOK THIS UP (HM-DEC-037). "Maidenhead grid
/// locator" is exactly the kind of jargon Hamlet exists to dissolve, and it is
/// a barrier with nothing behind it: the FCC's record of the licence already
/// carries coordinates, callook republishes them, and the locator is
/// arithmetic on top. So the operator is never asked — the field fills itself
/// the same way the licence class does.</para>
/// <para>LAZY AND AUTOMATIC, the same shape as the class (HM-DEC-028): on
/// startup and after any profile change, if there is a callsign and no grid,
/// resolve it and narrate in the status bar. It never blocks, never opens a
/// dialog, and a service that is down simply leaves the field empty and
/// hand-editable.</para>
/// <para>A HAND-ENTERED GRID IS NEVER OVERWRITTEN. Somebody operating
/// portable, or from a club station, or from the other side of a big city,
/// knows where their antenna is far better than the FCC's record of their
/// mailing address does. A disagreement shows both and the operator chooses.
/// The comparison is at four characters, because the FCC address and the
/// antenna being in different subsquares of the same square is not a
/// disagreement worth interrupting anybody over.</para>
/// <para>NEVER GUESSED FROM THE LOCATION STRING. "Trafford, PA" is enough to
/// name a call district, which is a lookup in a published table, and it is not
/// enough to place a station within seventy miles. A grid derived from a town
/// name would be a guess wearing the clothes of a measurement (§0.0).</para>
/// </remarks>
public static class GridResolver
{
    /// <summary>
    /// How many locator characters have to match before a lookup and a
    /// hand-entered grid count as disagreeing.
    /// </summary>
    /// <remarks>
    /// Four: the square, about seventy by one hundred miles. Two stations in
    /// the same square are near enough for every use Hamlet puts this to, and
    /// interrupting somebody because their antenna sits in FN00DJ rather than
    /// FN00DK would be pedantry with a dialog attached.
    /// </remarks>
    public const int ComparisonLength = 4;

    /// <summary>
    /// True when a profile is worth a lookup for its grid: it has a callsign
    /// and no locator.
    /// </summary>
    /// <param name="profile">The operator profile.</param>
    /// <returns>True when a lookup should run.</returns>
    public static bool NeedsResolution(OperatorProfile profile)
        => !string.IsNullOrWhiteSpace(profile.Callsign)
           && string.IsNullOrWhiteSpace(profile.GridSquare);

    /// <summary>
    /// Apply an already-fetched lookup to the grid square on a profile.
    /// </summary>
    /// <param name="profile">The profile. Written to only when the grid was
    /// unknown.</param>
    /// <param name="result">What the service said, or null.</param>
    /// <param name="utcNow">The moment to stamp on the provenance.</param>
    /// <returns>What happened, and what to narrate.</returns>
    public static GridResolution Apply(
        OperatorProfile profile, CallsignLookupResult? result, DateTime utcNow)
    {
        if (result is null)
        {
            return GridResolution.None;
        }

        var existing = OperatorLocation.Normalize(profile.GridSquare);

        if (result.Location is not { } position)
        {
            // The service answered and holds no coordinates. That is an answer:
            // the field stays empty and hand-editable, and nothing is invented
            // from the address line that is also in the payload.
            return existing.Length > 0
                ? GridResolution.None
                : new GridResolution(
                    GridResolutionOutcome.NoCoordinates, "", "", result.SourceName,
                    $"{result.SourceName} has no location for "
                    + $"{result.Callsign.ToUpperInvariant()}. You can type your grid "
                    + "square in Settings, or leave it and Hamlet will do without.");
        }

        var found = OperatorLocation.ToGrid(position);

        if (profile.GridSquareWasSetByHand)
        {
            return SameSquare(found, existing)
                ? GridResolution.None
                : new GridResolution(
                    GridResolutionOutcome.Mismatch, found, existing, result.SourceName,
                    $"Your license address puts you in {found[..ComparisonLength]}, and "
                    + $"you have {existing} set. If you're operating from somewhere "
                    + "else, yours is the one to keep.");
        }

        if (existing.Length > 0)
        {
            // Already resolved by a lookup. Refresh it silently if the service
            // has moved, but say nothing — nobody wants a status line on every
            // startup about a field they never asked for in the first place.
            profile.SetPositionFromLookup(position, result.SourceName, utcNow);
            return GridResolution.None;
        }

        profile.SetPositionFromLookup(position, result.SourceName, utcNow);

        return new GridResolution(
            GridResolutionOutcome.Resolved, found, "", result.SourceName,
            $"You're in grid {found}. Hamlet can work out sunrise, sunset and how "
            + "far away a station is from here.");
    }

    /// <summary>
    /// Whether two locators name the same square, ignoring the subsquare.
    /// </summary>
    /// <param name="a">One locator.</param>
    /// <param name="b">The other.</param>
    /// <returns>True when the first four characters agree.</returns>
    public static bool SameSquare(string? a, string? b)
    {
        var x = OperatorLocation.Normalize(a);
        var y = OperatorLocation.Normalize(b);

        if (x.Length < ComparisonLength || y.Length < ComparisonLength)
        {
            return false;
        }

        return string.Equals(
            x[..ComparisonLength], y[..ComparisonLength], StringComparison.Ordinal);
    }

    /// <summary>
    /// The provenance line shown beside the grid square in Settings.
    /// </summary>
    /// <param name="profile">The operator profile.</param>
    /// <returns>Plain language, never empty.</returns>
    public static string DescribeProvenance(OperatorProfile profile)
    {
        var grid = OperatorLocation.Normalize(profile.GridSquare);

        if (grid.Length == 0)
        {
            return "Not set yet. Hamlet fills this in from your callsign, and until "
                + "it does it won't guess how far away anything is.";
        }

        var on = string.IsNullOrWhiteSpace(profile.GridSquareSetOn)
            ? ""
            : $", {profile.GridSquareSetOn}";

        return profile.GridSquareSource switch
        {
            ProfileFactSource.LookedUp =>
                $"{grid}, worked out from your license address on "
                + $"{profile.GridSquareSourceName}{on}.",
            ProfileFactSource.EnteredByOperator =>
                $"{grid}, and you set this{on}.",
            _ => grid,
        };
    }

    /// <summary>
    /// The one line that explains what a grid square is, in the app.
    /// </summary>
    /// <remarks>
    /// The jargon gets dissolved where it appears rather than in a help page
    /// nobody opens (§0.7, HM-DEC-034). Somebody who has been told once that
    /// this is a postal code for the planet never has to be told again.
    /// </remarks>
    public const string Explanation =
        "That's a short code for where you are, a bit like a postal code for the "
        + "planet. Hamlet works it out from your callsign and uses it to tell you how "
        + "far away a station is, and when the sun rises and sets where you're sitting.";
}
