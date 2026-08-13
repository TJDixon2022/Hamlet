using System.Text.Json.Serialization;
using Hamlet.RadioEngine.Licensing;

namespace Hamlet.App.Settings;

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

    /// <summary>Maidenhead grid square, e.g. "FN00". Optional.</summary>
    public string GridSquare { get; set; } = "";

    /// <summary>
    /// The operator's licence class, which decides what the band map shows as
    /// theirs to use (HM-DEC-028).
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="LicenceClass.Unknown"/>, and unknown means the
    /// app says so rather than assuming the commonest class. Guessing here
    /// would be the one guess with legal consequences (HM-DEC-009).
    /// </remarks>
    public LicenceClass LicenceClass { get; set; } = LicenceClass.Unknown;

    /// <summary>How the class came to be known.</summary>
    public LicenceClassSource LicenceClassSource { get; set; } = LicenceClassSource.Unset;

    /// <summary>Which service answered, when it was looked up.</summary>
    public string LicenceClassSourceName { get; set; } = "";

    /// <summary>
    /// When the class was set, as an ISO date. Empty when never set.
    /// </summary>
    /// <remarks>
    /// Provenance travels with the value: "General, from FCC data, today" and
    /// "General, because you said so in 2019" are different claims, and the
    /// operator is entitled to see which one they are looking at.
    /// </remarks>
    public string LicenceClassSetOn { get; set; } = "";

    /// <summary>True when the operator chose the class themselves.</summary>
    /// <remarks>
    /// The one flag that decides whether a lookup may write: a hand-set class
    /// is never silently overwritten (HM-DEC-028).
    /// </remarks>
    [JsonIgnore]
    public bool LicenceClassWasSetByHand
        => LicenceClassSource == LicenceClassSource.EnteredByOperator
           && LicenceClass != LicenceClass.Unknown;

    /// <summary>
    /// Record a licence class along with where it came from.
    /// </summary>
    /// <param name="licenceClass">The class.</param>
    /// <param name="source">How it was determined.</param>
    /// <param name="sourceName">Service name for a lookup, else "".</param>
    /// <param name="onUtc">When, for the provenance line.</param>
    public void SetLicenceClass(
        LicenceClass licenceClass,
        LicenceClassSource source,
        string sourceName,
        DateTime onUtc)
    {
        LicenceClass = licenceClass;
        LicenceClassSource = source;
        LicenceClassSourceName = sourceName;
        LicenceClassSetOn = onUtc.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
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
