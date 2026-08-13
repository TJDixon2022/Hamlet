using System.Text.Json.Serialization;

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
