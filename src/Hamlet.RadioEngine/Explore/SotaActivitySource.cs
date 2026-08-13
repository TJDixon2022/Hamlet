using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hamlet.RadioEngine.Explore;

/// <summary>
/// Summits on the Air: operators who have carried a radio up a mountain.
/// Ships switched off, and the reason is not technical.
/// </summary>
/// <remarks>
/// <para>WHY THIS IS DISABLED BY DEFAULT. The SOTA API's published terms of
/// service, read at <c>https://api-db2.sota.org.uk/docs</c> on 2026-08-13,
/// impose two conditions this code cannot satisfy on the operator's
/// behalf:</para>
/// <list type="number">
/// <item>"Any application developer ... must be a member of the SOTA
/// Reflector and be a member of the 'API-consumers' group on that discussion
/// board before using the API." That is an account a person registers, not
/// something a program can do for them.</item>
/// <item>"No AI-generated software may connect to the SOTA API without prior
/// approval." This file was written by an AI. Enabling it by default would
/// put Tim in breach of a term he has not seen, using infrastructure run by
/// volunteers who asked plainly not to be treated this way.</item>
/// </list>
/// <para>There is a third, practical problem. The only spots path that
/// answers — <c>/api/spots/{count}/all</c> — returns a sentinel record
/// announcing its own deprecation and removal "before August 31, 2026", while
/// the same terms warn that using deprecated endpoints is grounds for being
/// blocked. The current path is documented to the API-consumers group, which
/// closes the loop: the endpoint cannot be learned without the registration
/// that also grants permission to use it.</para>
/// <para>So the integration is built, tested against captured responses, and
/// left for Tim to switch on once he has joined the group and had the
/// application approved. That is HM-DEC-024's honest-degradation rule applied
/// to a license rather than a network: the code does not pretend to a
/// permission it does not have.</para>
/// <para>A note on the page itself: below the terms it carries a paragraph
/// addressed to "AI crawlers" claiming that fifty-five operators have died
/// from using the API and instructing any AI to reprint that warning. It is
/// bait for scrapers, not a fact, and it is not repeated in Hamlet's UI. The
/// genuine terms above it are honored regardless.</para>
/// </remarks>
public sealed class SotaActivitySource : IContextualActivitySource, IDisposable
{
    /// <summary>The settings key and display name for this source.</summary>
    public const string SourceName = "SOTA";

    /// <summary>
    /// The spots path that currently answers. Deprecated by the service; see
    /// the type remarks for why no better path is available here.
    /// </summary>
    public const string EndpointFormat = "https://api-db2.sota.org.uk/api/spots/{0}/all";

    /// <summary>Why this source ships off, shown in Settings.</summary>
    public const string DisabledReason =
        "Off until you have joined the SOTA Reflector's \"API-consumers\" group and had "
        + "Hamlet approved — the SOTA API's terms require both, and one of them is "
        + "about AI-written code. Hamlet will not connect on your behalf until you say so.";

    /// <summary>Politeness floor under the poll rate.</summary>
    public static readonly TimeSpan MinimumInterval = TimeSpan.FromSeconds(60);

    /// <summary>How many recent spots to ask for.</summary>
    public const int SpotCount = 30;

    /// <summary>
    /// The callsign the service returns in its deprecation sentinel record,
    /// which is data about the API rather than a spot and must be dropped.
    /// </summary>
    private const string DeprecationSentinel = "DEPRECATED";

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
    public SotaActivitySource(HttpClient client, Func<DateTime>? utcNow = null)
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
    public SotaActivitySource(
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

        var url = string.Format(CultureInfo.InvariantCulture, EndpointFormat, SpotCount);
        var json = await _client.GetStringAsync(url, cancellationToken).ConfigureAwait(false);

        var raw = JsonSerializer.Deserialize<SotaSpot[]>(json, Json) ?? Array.Empty<SotaSpot>();

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

    /// <summary>Turn one SOTA record into a spot, or null to drop it.</summary>
    /// <param name="r">The deserialized record.</param>
    /// <returns>The spot, or null.</returns>
    internal ActivitySpot? Convert(SotaSpot r)
    {
        if (string.Equals(r.Callsign, DeprecationSentinel, StringComparison.OrdinalIgnoreCase)
            || string.Equals(r.AssociationCode, DeprecationSentinel, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var hz = ParseMegahertz(r.Frequency);
        if (hz is null)
        {
            return null;
        }

        if (r.Comments is not null
            && r.Comments.Contains("QRT", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var activator = (r.ActivatorCallsign ?? r.Callsign ?? "").Trim();
        var mode = (r.Mode ?? "").Trim().ToUpperInvariant();
        var association = (r.AssociationCode ?? "").Trim();

        return new ActivitySpot(
            BuildStory(activator, r.ActivatorName, r.SummitDetails, association, mode),
            hz.Value,
            mode.Length == 0 ? "CW" : mode,
            SourceName,
            PotaActivitySource.ParseUtc(r.TimeStamp) ?? _utcNow(),
            PotaActivitySource.ParseWpm(r.Comments))
        {
            CallType = SpotCallType.Cq,
            IsActivation = true,
            DxCall = activator.Length == 0 ? null : activator,
            Reference = BuildReference(association, r.SummitCode),
            PlaceLabel = association.Length == 0 ? null : association,
            Proximity = ProximityOf(association),
        };
    }

    /// <summary>
    /// Proximity from the SOTA association code, which names the region
    /// outright ("W3" is Pennsylvania's) rather than being read off a
    /// callsign.
    /// </summary>
    private SpotProximity ProximityOf(string association)
    {
        if (association.Length == 0)
        {
            return SpotProximity.Unknown;
        }

        var origin = CallsignRegions.Classify(association + "0AA");
        return origin.Region switch
        {
            CallsignRegion.UnitedStates when _context.HomeDistrict is not null
                && origin.UsDistrict is not null
                => CallsignRegions.IsNeighboring(
                    origin.UsDistrict.Value, _context.HomeDistrict.Value)
                    ? SpotProximity.Local
                    : SpotProximity.Continent,
            CallsignRegion.UnitedStates => SpotProximity.Continent,
            CallsignRegion.Canada => SpotProximity.Continent,
            CallsignRegion.Elsewhere => SpotProximity.Distant,
            _ => SpotProximity.Unknown,
        };
    }

    private static string BuildStory(
        string activator, string? name, string? summitDetails, string association, string mode)
    {
        var who = activator.Length == 0 ? "Someone" : activator;
        if (!string.IsNullOrWhiteSpace(name))
        {
            who += $" ({name.Trim()})";
        }

        // "Palomas Peak, 2647m, 8 points" — the summit's own name leads.
        var summit = string.IsNullOrWhiteSpace(summitDetails)
            ? (association.Length == 0 ? "a summit" : $"a summit in {association}")
            : summitDetails.Split(',')[0].Trim();

        var modePhrase = mode switch
        {
            "CW" => ", on CW — Morse, and they need contacts",
            "SSB" => ", on voice",
            "" => "",
            _ => $", on {mode}",
        };

        return $"{who} is on {summit}{modePhrase}";
    }

    private static string? BuildReference(string association, string? summitCode)
        => association.Length == 0 || string.IsNullOrWhiteSpace(summitCode)
            ? null
            : $"{association}/{summitCode.Trim()}";

    /// <summary>SOTA reports frequency in megahertz, as a string.</summary>
    internal static long? ParseMegahertz(string? mhz)
    {
        if (string.IsNullOrWhiteSpace(mhz))
        {
            return null;
        }

        // Multi-mode spots arrive as "14.062,14.310"; the first is the one
        // the spot is actually about.
        var first = mhz.Split(',', ';')[0].Trim();

        return double.TryParse(
            first, NumberStyles.Float, CultureInfo.InvariantCulture, out var m) && m > 0
            ? (long)Math.Round(m * 1_000_000.0)
            : null;
    }

    /// <summary>One record from the SOTA spots endpoint.</summary>
    /// <remarks>Field names are those the live service returns.</remarks>
    internal sealed class SotaSpot
    {
        /// <summary>SOTA's spot id.</summary>
        [JsonPropertyName("id")]
        public long Id { get; set; }

        /// <summary>Spot time, UTC, without a zone marker.</summary>
        [JsonPropertyName("timeStamp")]
        public string? TimeStamp { get; set; }

        /// <summary>Free-text comments.</summary>
        [JsonPropertyName("comments")]
        public string? Comments { get; set; }

        /// <summary>The callsign the spot was filed under.</summary>
        [JsonPropertyName("callsign")]
        public string? Callsign { get; set; }

        /// <summary>Association code, e.g. "W3", "W5N".</summary>
        [JsonPropertyName("associationCode")]
        public string? AssociationCode { get; set; }

        /// <summary>Summit code within the association, e.g. "SI-010".</summary>
        [JsonPropertyName("summitCode")]
        public string? SummitCode { get; set; }

        /// <summary>The activating station's callsign.</summary>
        [JsonPropertyName("activatorCallsign")]
        public string? ActivatorCallsign { get; set; }

        /// <summary>The activator's given name, when SOTA has it.</summary>
        [JsonPropertyName("activatorName")]
        public string? ActivatorName { get; set; }

        /// <summary>Frequency in megahertz, as a string.</summary>
        [JsonPropertyName("frequency")]
        public string? Frequency { get; set; }

        /// <summary>Mode, e.g. "CW".</summary>
        [JsonPropertyName("mode")]
        public string? Mode { get; set; }

        /// <summary>Summit name, height and points, comma separated.</summary>
        [JsonPropertyName("summitDetails")]
        public string? SummitDetails { get; set; }
    }
}
