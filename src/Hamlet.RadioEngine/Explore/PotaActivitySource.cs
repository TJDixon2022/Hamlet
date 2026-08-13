using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hamlet.RadioEngine.Explore;

/// <summary>
/// Parks on the Air: operators who have carried a radio into a park and want
/// very much to be found.
/// </summary>
/// <remarks>
/// <para>Endpoint <c>https://api.pota.app/spot/activator</c>, verified live
/// against the running service on 2026-08-13. It takes no authentication and
/// returns the current activator spots as a JSON array; field names below are
/// copied from a real response, not from memory (HM-DEC-024).</para>
/// <para>POTA publishes no rate card — it is an unofficial API run for the
/// community — so politeness is self-imposed: <see cref="MinimumInterval"/>
/// floors the poll rate underneath whatever the operator sets, and a cached
/// answer is returned inside that window instead of a second request.</para>
/// <para>Why this source is weighted up for a newcomer: a park activator is
/// the friendliest contact on the band. They are there on purpose, they need
/// contacts to make the activation count, and they will slow down for
/// somebody who is clearly new.</para>
/// <para>Spots are returned for every band, not just the one on screen. The
/// whole feed is barely a hundred records, and holding all of it is what lets
/// the band-conditions line say "nothing on 20 m — try 40 m" with a count
/// behind it instead of a shrug (HM-DEC-025). RBN is filtered at the source
/// instead, because six spots a second is a different problem.</para>
/// </remarks>
public sealed class PotaActivitySource : IContextualActivitySource, IDisposable
{
    /// <summary>The settings key and display name for this source.</summary>
    public const string SourceName = "POTA";

    /// <summary>The activator-spot endpoint.</summary>
    public const string Endpoint = "https://api.pota.app/spot/activator";

    /// <summary>
    /// The floor under the poll rate, whatever the operator's refresh
    /// interval says. POTA regenerates spots about once a minute, so asking
    /// faster buys nothing and costs somebody else's server.
    /// </summary>
    public static readonly TimeSpan MinimumInterval = TimeSpan.FromSeconds(60);

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _client;
    private readonly bool _ownsClient;
    private readonly Func<DateTime> _utcNow;
    private readonly object _gate = new();

    private ActivityContext _context = new();
    private IReadOnlyList<ActivitySpot> _cached = Array.Empty<ActivitySpot>();
    private DateTime? _lastFetchUtc;

    /// <summary>Creates the source.</summary>
    /// <param name="client">Client to use; the caller keeps ownership.</param>
    /// <param name="utcNow">Clock, injected for testability.</param>
    public PotaActivitySource(HttpClient client, Func<DateTime>? utcNow = null)
    {
        _client = client;
        _ownsClient = false;
        _utcNow = utcNow ?? (() => DateTime.UtcNow);
    }

    /// <summary>Creates the source with its own polite client.</summary>
    /// <param name="version">App version for the User-Agent.</param>
    /// <param name="callsign">Operator callsign for the User-Agent.</param>
    /// <param name="handler">Transport, injected by tests.</param>
    /// <param name="utcNow">Clock, injected for testability.</param>
    public PotaActivitySource(
        string version,
        string? callsign,
        HttpMessageHandler? handler = null,
        Func<DateTime>? utcNow = null)
    {
        _client = HamletIdentity.CreateClient(version, callsign, handler);
        _ownsClient = true;
        _utcNow = utcNow ?? (() => DateTime.UtcNow);
    }

    /// <inheritdoc/>
    public string Name => SourceName;

    /// <inheritdoc/>
    public void SetContext(ActivityContext context) => _context = context;

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ActivitySpot>> GetSpotsAsync(
        CancellationToken cancellationToken = default)
    {
        var now = _utcNow();

        lock (_gate)
        {
            if (_lastFetchUtc is not null && now - _lastFetchUtc.Value < MinimumInterval)
            {
                return _cached;
            }
        }

        var json = await _client.GetStringAsync(Endpoint, cancellationToken)
            .ConfigureAwait(false);

        var raw = JsonSerializer.Deserialize<PotaSpot[]>(json, Json)
                  ?? Array.Empty<PotaSpot>();

        var spots = new List<ActivitySpot>(raw.Length);
        foreach (var r in raw)
        {
            var spot = Convert(r);
            if (spot is not null)
            {
                spots.Add(spot);
            }
        }

        lock (_gate)
        {
            _cached = spots;
            _lastFetchUtc = now;
        }

        return spots;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_ownsClient)
        {
            _client.Dispose();
        }
    }

    /// <summary>
    /// Turn one POTA record into a spot, or null when it should not be shown.
    /// </summary>
    /// <param name="r">The deserialized record.</param>
    /// <returns>The spot, or null.</returns>
    /// <remarks>
    /// Exposed for tests, which feed it captured records rather than reaching
    /// the network.
    /// </remarks>
    internal ActivitySpot? Convert(PotaSpot r)
    {
        if (r.Invalid is not null)
        {
            return null;
        }

        var hz = ParseKilohertz(r.Frequency);
        if (hz is null)
        {
            return null;
        }

        // "QRT" in the comments means the operator has packed up. Showing a
        // newcomer an empty frequency is worse than showing them nothing.
        if (r.Comments is not null
            && r.Comments.Contains("QRT", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var activator = (r.Activator ?? "").Trim();
        var mode = NormalizeMode(r.Mode);
        var place = (r.LocationDesc ?? "").Trim();
        var parkName = FirstNonBlank(r.Name, r.ParkName);

        return new ActivitySpot(
            BuildStory(activator, parkName, place, mode),
            hz.Value,
            mode,
            SourceName,
            ParseUtc(r.SpotTime) ?? _utcNow(),
            ParseWpm(r.Comments))
        {
            // A park activation is, by definition, somebody asking to be
            // called. That is what the activation IS — not an inference about
            // what they might be transmitting.
            CallType = SpotCallType.Cq,
            IsActivation = true,
            DxCall = activator.Length == 0 ? null : activator,
            SpotterCall = string.IsNullOrWhiteSpace(r.Spotter) ? null : r.Spotter.Trim(),
            Reference = string.IsNullOrWhiteSpace(r.Reference) ? null : r.Reference.Trim(),
            PlaceLabel = place.Length == 0 ? null : place,
            Proximity = ProximityOf(r),

            // The park's own coordinates, as POTA states them. The activator
            // is standing in the park, so this is where the STATION is —
            // which is the only thing a distance may be drawn from
            // (HM-DEC-038).
            StationLocation = ParkLocation(r),
        };
    }

    /// <summary>
    /// The park's position as POTA states it, or null.
    /// </summary>
    /// <remarks>
    /// The coordinates are used rather than <c>grid6</c>, because they are
    /// what POTA derived the grid from and because distance wants degrees.
    /// Both must be present and in range; a half-answer is discarded, since a
    /// longitude of zero would place a park in the Gulf of Guinea and look
    /// entirely plausible on a card.
    /// </remarks>
    internal static LatLon? ParkLocation(PotaSpot r)
    {
        if (r.Latitude is not { } lat || r.Longitude is not { } lon)
        {
            return null;
        }

        if (lat is < -90 or > 90 || lon is < -180 or > 180)
        {
            return null;
        }

        return Math.Abs(lat) < 0.0001 && Math.Abs(lon) < 0.0001
            ? null
            : new LatLon(lat, lon);
    }

    /// <summary>
    /// How close the park is — computed from the park's own coordinates when
    /// POTA supplies them, never from the activator's callsign.
    /// </summary>
    private SpotProximity ProximityOf(PotaSpot r)
    {
        var district = _context.HomeDistrict;
        var place = (r.LocationDesc ?? "").Trim();

        // "US-PA" and friends: the state is stated outright, so the district
        // it belongs to is a lookup rather than a guess.
        if (place.StartsWith("US-", StringComparison.OrdinalIgnoreCase) && place.Length >= 5)
        {
            var parkDistrict = OperatorLocation.HomeDistrict(place[3..]);
            if (parkDistrict is not null && district is not null)
            {
                return CallsignRegions.IsNeighboring(parkDistrict.Value, district.Value)
                    ? SpotProximity.Local
                    : SpotProximity.Continent;
            }

            return SpotProximity.Continent;
        }

        if (place.StartsWith("CA-", StringComparison.OrdinalIgnoreCase))
        {
            return SpotProximity.Continent;
        }

        return place.Length == 0 ? SpotProximity.Unknown : SpotProximity.Distant;
    }

    private static string BuildStory(
        string activator, string? parkName, string place, string mode)
    {
        var who = activator.Length == 0 ? "Someone" : activator;
        var where = string.IsNullOrWhiteSpace(parkName)
            ? (place.Length == 0 ? "a park" : $"a park in {place}")
            : parkName.Trim();

        var modePhrase = mode switch
        {
            "CW" => "on CW — Morse, and they want callers",
            "SSB" => "on voice",
            "" => "",
            _ => $"on {mode}",
        };

        var tail = modePhrase.Length == 0 ? "" : $", {modePhrase}";
        return $"{who} is activating {where}{tail}";
    }

    private static string? FirstNonBlank(string? a, string? b)
        => !string.IsNullOrWhiteSpace(a) ? a : (!string.IsNullOrWhiteSpace(b) ? b : null);

    /// <summary>POTA reports frequency in kilohertz, as a string.</summary>
    internal static long? ParseKilohertz(string? khz)
    {
        if (string.IsNullOrWhiteSpace(khz)
            || !double.TryParse(khz, NumberStyles.Float, CultureInfo.InvariantCulture, out var k)
            || k <= 0)
        {
            return null;
        }

        return (long)Math.Round(k * 1000.0);
    }

    /// <summary>POTA timestamps are UTC, without a zone marker.</summary>
    internal static DateTime? ParseUtc(string? stamp)
        => DateTime.TryParse(
            stamp, CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var t)
            ? t
            : null;

    /// <summary>
    /// Pull a CW speed out of free-text comments, e.g. "calling at 15wpm".
    /// Absent or unparseable means null — never a default speed.
    /// </summary>
    internal static int? ParseWpm(string? comments)
    {
        if (string.IsNullOrWhiteSpace(comments))
        {
            return null;
        }

        var text = comments.AsSpan();
        var idx = comments.IndexOf("wpm", StringComparison.OrdinalIgnoreCase);
        if (idx <= 0)
        {
            return null;
        }

        var end = idx;
        while (end > 0 && !char.IsAsciiDigit(text[end - 1]))
        {
            end--;
            if (idx - end > 2)
            {
                return null;
            }
        }

        var start = end;
        while (start > 0 && char.IsAsciiDigit(text[start - 1]))
        {
            start--;
        }

        if (start == end)
        {
            return null;
        }

        return int.TryParse(text[start..end], out var wpm) && wpm is > 0 and < 100
            ? wpm
            : null;
    }

    private static string NormalizeMode(string? mode)
    {
        var m = (mode ?? "").Trim().ToUpperInvariant();
        return m switch
        {
            "" => "CW",
            "PHONE" => "SSB",
            "USB" or "LSB" => "SSB",
            _ => m,
        };
    }

    /// <summary>One record from the POTA activator-spot endpoint.</summary>
    /// <remarks>Field names are those the live service returns.</remarks>
    internal sealed class PotaSpot
    {
        /// <summary>POTA's spot id.</summary>
        [JsonPropertyName("spotId")]
        public long SpotId { get; set; }

        /// <summary>The activating station's callsign.</summary>
        [JsonPropertyName("activator")]
        public string? Activator { get; set; }

        /// <summary>Frequency in kilohertz, as a string.</summary>
        [JsonPropertyName("frequency")]
        public string? Frequency { get; set; }

        /// <summary>Mode, e.g. "CW", "SSB", "FT8".</summary>
        [JsonPropertyName("mode")]
        public string? Mode { get; set; }

        /// <summary>Park reference, e.g. "US-1234".</summary>
        [JsonPropertyName("reference")]
        public string? Reference { get; set; }

        /// <summary>Park name, when POTA sends it in this field.</summary>
        [JsonPropertyName("parkName")]
        public string? ParkName { get; set; }

        /// <summary>Park name, the field the live service actually fills.</summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>Spot time, UTC, without a zone marker.</summary>
        [JsonPropertyName("spotTime")]
        public string? SpotTime { get; set; }

        /// <summary>Who posted the spot.</summary>
        [JsonPropertyName("spotter")]
        public string? Spotter { get; set; }

        /// <summary>Free-text comments; sometimes carries a CW speed.</summary>
        [JsonPropertyName("comments")]
        public string? Comments { get; set; }

        /// <summary>Location code, e.g. "US-PA" or "CA-ON".</summary>
        [JsonPropertyName("locationDesc")]
        public string? LocationDesc { get; set; }

        /// <summary>Four-character grid square of the park.</summary>
        [JsonPropertyName("grid4")]
        public string? Grid4 { get; set; }

        /// <summary>The park's latitude in degrees north.</summary>
        /// <remarks>
        /// Present on every record the live service returns. Used for the
        /// distance on a card, which is a distance to the station because the
        /// activator is in the park (HM-DEC-038).
        /// </remarks>
        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; }

        /// <summary>The park's longitude in degrees east.</summary>
        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }

        /// <summary>Non-null when POTA has flagged the spot as invalid.</summary>
        [JsonPropertyName("invalid")]
        public object? Invalid { get; set; }
    }
}
