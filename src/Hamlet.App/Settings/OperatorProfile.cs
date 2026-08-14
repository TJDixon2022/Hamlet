using System.Text.Json.Serialization;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Licensing;

namespace Hamlet.App.Settings;

/// <summary>Where a fact on the operator profile came from.</summary>
/// <remarks>
/// The same three states <see cref="LicenseClassSource"/> carries, for the
/// facts that are not the license class. It is a separate type rather than a
/// reuse because a field named <c>GridSquareSource</c> of type
/// <c>LicenseClassSource</c> reads as a mistake, and because
/// <see cref="LicenseClassSource"/> is a persisted key whose name is now
/// expensive to change (HM-DEC-035).
/// </remarks>
public enum ProfileFactSource
{
    /// <summary>Nothing has established this yet.</summary>
    Unset,

    /// <summary>The operator typed it. A lookup never overwrites this.</summary>
    EnteredByOperator,

    /// <summary>A lookup service supplied it.</summary>
    LookedUp,
}

/// <summary>
/// Who is operating (HM-DEC-019). One shaped object rather than three loose
/// strings on <see cref="AppSettings"/>, because these fields already have
/// futures: location and grid feed propagation and distance-to-spot work
/// (FG-001), and the callsign feeds logging (FG-004).
/// </summary>
/// <remarks>
/// PRIVACY — this is the one part of settings.json that identifies a person.
/// It is displayed in the app and written to settings.json, and it is NEVER
/// written to telemetry (HM-DEC-018, HM-DEC-019). Telemetry payloads are
/// built in one place, <see cref="Telemetry.AppEvents"/>, so that rule has a
/// single site to hold and a single test to prove it.
/// </remarks>
public sealed class OperatorProfile
{
    /// <summary>The operator's callsign. Displayed; never in telemetry.</summary>
    public string Callsign { get; set; } = "KC3QIS";

    /// <summary>How the callsign came to be known.</summary>
    /// <remarks>
    /// A lookup that answers at all has confirmed the callsign exists in the
    /// FCC record, because that is what it was asked about (HM-DEC-044). The
    /// operator always typed it first, so this never decides whether a lookup
    /// may write. It exists only so the Settings window can say whether the
    /// value was ever corroborated.
    /// </remarks>
    public ProfileFactSource CallsignSource { get; set; } = ProfileFactSource.Unset;

    /// <summary>Which service confirmed the callsign, when one did.</summary>
    public string CallsignSourceName { get; set; } = "";

    /// <summary>When it was confirmed, as an ISO date. Empty when never.</summary>
    public string CallsignSetOn { get; set; } = "";

    /// <summary>
    /// The exact callsign a lookup confirmed, so a later edit can be seen for
    /// what it is.
    /// </summary>
    /// <remarks>
    /// THE BADGE CLEARS THE MOMENT THE TEXT DIFFERS (HM-DEC-044), and this is
    /// how. "Verified" means "this is what the FCC record says", so it stops
    /// being true as soon as the field says something else. Comparing against
    /// the value that was actually confirmed makes that live while somebody
    /// types, and correct again after a restart, without a flag anybody has to
    /// remember to clear.
    /// </remarks>
    public string CallsignVerifiedAs { get; set; } = "";

    /// <summary>The operator's name, as they want to be greeted.</summary>
    public string OperatorName { get; set; } = "";

    /// <summary>Free text: city and state, or wherever the antenna is.</summary>
    public string Location { get; set; } = "";

    /// <summary>
    /// Maidenhead grid square, e.g. "FN00DJ". Optional, and derived rather
    /// than demanded (HM-DEC-037).
    /// </summary>
    /// <remarks>
    /// The operator is never asked to look this up. When the callsign lookup
    /// returns coordinates, the locator is computed from them; the field stays
    /// hand-editable for anybody the lookup cannot place, and a hand-entered
    /// value is never overwritten.
    /// </remarks>
    public string GridSquare { get; set; } = "";

    /// <summary>Latitude in degrees north, or null when unknown.</summary>
    /// <remarks>
    /// The coordinates are the stored fact and the grid square is a rendering
    /// of them: distance, bearing and the solar clock all want degrees, and a
    /// locator only ever gives them back to within a few miles. Null means
    /// unknown and is never a zero (HM-DEC-009).
    /// </remarks>
    public double? Latitude { get; set; }

    /// <summary>Longitude in degrees east, or null when unknown.</summary>
    public double? Longitude { get; set; }

    /// <summary>How the grid square came to be known.</summary>
    public ProfileFactSource GridSquareSource { get; set; } = ProfileFactSource.Unset;

    /// <summary>Which service supplied the coordinates, when one did.</summary>
    public string GridSquareSourceName { get; set; } = "";

    /// <summary>When the grid was set, as an ISO date. Empty when never set.</summary>
    public string GridSquareSetOn { get; set; } = "";

    /// <summary>The exact locator a lookup derived, so an edit is visible.</summary>
    /// <remarks>See <see cref="CallsignVerifiedAs"/> for why this is stored.</remarks>
    public string GridSquareVerifiedAs { get; set; } = "";

    /// <summary>True when the operator typed the grid square themselves.</summary>
    /// <remarks>
    /// Same rule as the license class (HM-DEC-028): the one flag that decides
    /// whether a lookup may write. Somebody operating portable, or from a
    /// second location, knows where they are better than the FCC's record of
    /// their mailing address does.
    /// </remarks>
    [JsonIgnore]
    public bool GridSquareWasSetByHand
        => GridSquareSource == ProfileFactSource.EnteredByOperator
           && !string.IsNullOrWhiteSpace(GridSquare);

    /// <summary>The operator's position, or null when it is not known.</summary>
    /// <remarks>
    /// Coordinates first, because they are what was stored; a hand-entered
    /// locator with no coordinates falls back to the center of its square,
    /// which is the best anybody can do with four or six characters.
    /// </remarks>
    [JsonIgnore]
    public LatLon? Position
        => Latitude is { } lat && Longitude is { } lon
            ? new LatLon(lat, lon)
            : OperatorLocation.FromGrid(GridSquare);

    /// <summary>
    /// The operator's license class, which decides what the band map shows as
    /// theirs to use (HM-DEC-028).
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="LicenseClass.Unknown"/>, and unknown means the
    /// app says so rather than assuming the commonest class. Guessing here
    /// would be the one guess with legal consequences (HM-DEC-009).
    /// </remarks>
    public LicenseClass LicenseClass { get; set; } = LicenseClass.Unknown;

    /// <summary>How the class came to be known.</summary>
    public LicenseClassSource LicenseClassSource { get; set; } = LicenseClassSource.Unset;

    /// <summary>Which service answered, when it was looked up.</summary>
    public string LicenseClassSourceName { get; set; } = "";

    /// <summary>
    /// When the class was set, as an ISO date. Empty when never set.
    /// </summary>
    /// <remarks>
    /// Provenance travels with the value: "General, from FCC data, today" and
    /// "General, because you said so in 2019" are different claims, and the
    /// operator is entitled to see which one they are looking at.
    /// </remarks>
    public string LicenseClassSetOn { get; set; } = "";

    /// <summary>The exact class a lookup reported, so a later change shows.</summary>
    /// <remarks>See <see cref="CallsignVerifiedAs"/> for why this is stored.</remarks>
    public LicenseClass LicenseClassVerifiedAs { get; set; } = LicenseClass.Unknown;

    /// <summary>True when the operator chose the class themselves.</summary>
    /// <remarks>
    /// The one flag that decides whether a lookup may write: a hand-set class
    /// is never silently overwritten (HM-DEC-028).
    /// </remarks>
    [JsonIgnore]
    public bool LicenseClassWasSetByHand
        => LicenseClassSource == LicenseClassSource.EnteredByOperator
           && LicenseClass != LicenseClass.Unknown;

    /// <summary>
    /// Record a license class along with where it came from.
    /// </summary>
    /// <param name="licenseClass">The class.</param>
    /// <param name="source">How it was determined.</param>
    /// <param name="sourceName">Service name for a lookup, else "".</param>
    /// <param name="onUtc">When, for the provenance line.</param>
    public void SetLicenseClass(
        LicenseClass licenseClass,
        LicenseClassSource source,
        string sourceName,
        DateTime onUtc)
    {
        LicenseClass = licenseClass;
        LicenseClassSource = source;
        LicenseClassSourceName = sourceName;
        LicenseClassSetOn = onUtc.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Record that a lookup confirmed the callsign, and which class it
    /// reported for it.
    /// </summary>
    /// <param name="callsign">The callsign the service answered about.</param>
    /// <param name="reportedClass">The class it reported.</param>
    /// <param name="sourceName">Which service answered.</param>
    /// <param name="onUtc">When it answered.</param>
    /// <remarks>
    /// Separate from <see cref="SetLicenseClass"/> because it records what was
    /// SEEN rather than what was adopted. A hand-set class is never
    /// overwritten (HM-DEC-028), and this is how the Settings window can still
    /// say what the FCC record holds without the profile pretending to agree
    /// with it (HM-DEC-044).
    /// </remarks>
    public void RecordLookup(
        string callsign, LicenseClass reportedClass, string sourceName, DateTime onUtc)
    {
        var on = onUtc.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

        CallsignVerifiedAs = (callsign ?? "").Trim().ToUpperInvariant();
        CallsignSource = ProfileFactSource.LookedUp;
        CallsignSourceName = sourceName;
        CallsignSetOn = on;

        LicenseClassVerifiedAs = reportedClass;
    }

    /// <summary>
    /// Record a position from a lookup, deriving the grid square from it.
    /// </summary>
    /// <param name="position">The coordinates the service supplied.</param>
    /// <param name="sourceName">Which service supplied them.</param>
    /// <param name="onUtc">When, for the provenance line.</param>
    public void SetPositionFromLookup(LatLon position, string sourceName, DateTime onUtc)
    {
        Latitude = position.Latitude;
        Longitude = position.Longitude;
        GridSquare = OperatorLocation.ToGrid(position);
        GridSquareVerifiedAs = GridSquare;
        GridSquareSource = ProfileFactSource.LookedUp;
        GridSquareSourceName = sourceName;
        GridSquareSetOn = onUtc.ToString(
            "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Record a grid square the operator typed, and the coordinates it implies.
    /// </summary>
    /// <param name="grid">The locator, four or six characters.</param>
    /// <param name="onUtc">When, for the provenance line.</param>
    /// <remarks>
    /// A malformed locator is stored as typed with no coordinates behind it, so
    /// the operator sees their own text rather than having it silently
    /// discarded, and nothing downstream draws a distance from a square that
    /// does not exist.
    /// </remarks>
    public void SetGridByHand(string? grid, DateTime onUtc)
    {
        var text = OperatorLocation.Normalize(grid);
        var point = OperatorLocation.FromGrid(text);

        GridSquare = text;
        Latitude = point?.Latitude;
        Longitude = point?.Longitude;
        GridSquareSource = text.Length == 0
            ? ProfileFactSource.Unset
            : ProfileFactSource.EnteredByOperator;
        GridSquareSourceName = "";
        GridSquareSetOn = text.Length == 0
            ? ""
            : onUtc.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>True when nothing has been filled in at all.</summary>
    /// <remarks>Derived, never stored: a settings file that carries both the
    /// fields and conclusions drawn from them can disagree with itself.</remarks>
    [JsonIgnore]
    public bool IsEmpty
        => string.IsNullOrWhiteSpace(Callsign)
           && string.IsNullOrWhiteSpace(OperatorName)
           && string.IsNullOrWhiteSpace(Location)
           && string.IsNullOrWhiteSpace(GridSquare);

    /// <summary>
    /// The About box byline — "by Tim, KC3QIS", or the parts of it that exist.
    /// Empty when neither name nor callsign is known, so the caller shows just
    /// the app name rather than a byline with a hole in it.
    /// </summary>
    [JsonIgnore]
    public string Byline
    {
        get
        {
            var name = OperatorName.Trim();
            var call = Callsign.Trim();

            if (name.Length > 0 && call.Length > 0)
            {
                return $"by {name}, {call}";
            }

            if (name.Length > 0)
            {
                return $"by {name}";
            }

            return call.Length > 0 ? $"by {call}" : "";
        }
    }
}
