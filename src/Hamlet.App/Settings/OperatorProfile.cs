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
