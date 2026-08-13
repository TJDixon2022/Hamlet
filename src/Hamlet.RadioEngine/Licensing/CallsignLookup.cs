using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hamlet.RadioEngine.Explore;

namespace Hamlet.RadioEngine.Licensing;

/// <summary>What a callsign lookup found.</summary>
/// <param name="Callsign">The callsign as the service reports it.</param>
/// <param name="Class">The operator class, or Unknown.</param>
/// <param name="SourceName">Which service answered, for provenance.</param>
/// <param name="RetrievedUtc">When it answered.</param>
public sealed record CallsignLookupResult(
    string Callsign, LicenseClass Class, string SourceName, DateTime RetrievedUtc);

/// <summary>Looks up a US callsign's operator class.</summary>
public interface ICallsignLookup
{
    /// <summary>The service's name, shown to the operator as provenance.</summary>
    string SourceName { get; }

    /// <summary>
    /// Look up a callsign.
    /// </summary>
    /// <param name="callsign">The callsign to look up.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>
    /// The result, or null when the service answered but does not know the
    /// callsign. Throws for transport failures, which the caller treats as
    /// "try again later" rather than "no such license".
    /// </returns>
    Task<CallsignLookupResult?> LookupAsync(
        string callsign, CancellationToken cancellationToken = default);
}

/// <summary>
/// Resolves a callsign's operator class from callook.info, which republishes
/// FCC ULS data.
/// </summary>
/// <remarks>
/// <para>TERMS. callook.info's API reference states, under a "Usage Terms"
/// heading: "The callook.info API is publicly available and is free to use
/// however you wish." It states no rate limit, no attribution requirement, no
/// restriction on automated access and no restriction on how the software was
/// written. Read on 2026-08-13. Unlike SOTA (HM-DEC-024), nothing in those
/// terms stands in the way, so this ships switched on.</para>
/// <para>Politeness is still self-imposed: the User-Agent names the app, its
/// version and the operator (HM-DEC-024), and a result is cached so a
/// callsign is asked about once rather than on every profile touch.</para>
/// <para>WHAT IS READ, AND WHAT IS DELIBERATELY NOT. The response carries the
/// licensee's full name and street address. Hamlet reads the operator class
/// and nothing else. It is the operator's own record, but a program that
/// quietly harvested a home address because it happened to be in the payload
/// would be doing something nobody asked it to do — and the parser is the
/// natural place for that restraint to be visible.</para>
/// <para>The class is public information, as public as the callsign itself:
/// it is in the FCC's own searchable database. Sending the callsign to look
/// it up is ordinary. It still never enters telemetry (HM-DEC-019).</para>
/// </remarks>
public sealed class CallookCallsignLookup : ICallsignLookup, IDisposable
{
    /// <summary>The service's display name.</summary>
    public const string ServiceName = "callook.info";

    /// <summary>Endpoint format; the callsign is the path segment.</summary>
    public const string EndpointFormat = "https://callook.info/{0}/json";

    /// <summary>Status the service reports for a callsign it knows.</summary>
    private const string ValidStatus = "VALID";

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _client;
    private readonly bool _ownsClient;
    private readonly Func<DateTime> _utcNow;

    /// <summary>Creates the lookup with its own polite client.</summary>
    /// <param name="version">App version for the User-Agent.</param>
    /// <param name="callsign">Operator callsign for the User-Agent.</param>
    /// <param name="handler">Transport, injected by tests.</param>
    /// <param name="utcNow">Clock, injected for testability.</param>
    public CallookCallsignLookup(
        string version,
        string? callsign,
        HttpMessageHandler? handler = null,
        Func<DateTime>? utcNow = null)
    {
        _client = HamletIdentity.CreateClient(version, callsign, handler);
        _ownsClient = true;
        _utcNow = utcNow ?? (() => DateTime.UtcNow);
    }

    /// <summary>Creates the lookup over a caller-owned client.</summary>
    /// <param name="client">Client to use.</param>
    /// <param name="utcNow">Clock, injected for testability.</param>
    public CallookCallsignLookup(HttpClient client, Func<DateTime>? utcNow = null)
    {
        _client = client;
        _ownsClient = false;
        _utcNow = utcNow ?? (() => DateTime.UtcNow);
    }

    /// <inheritdoc/>
    public string SourceName => ServiceName;

    /// <inheritdoc/>
    public async Task<CallsignLookupResult?> LookupAsync(
        string callsign, CancellationToken cancellationToken = default)
    {
        var call = (callsign ?? "").Trim().ToUpperInvariant();
        if (call.Length == 0)
        {
            return null;
        }

        var url = string.Format(
            System.Globalization.CultureInfo.InvariantCulture, EndpointFormat, call);

        var json = await _client.GetStringAsync(url, cancellationToken).ConfigureAwait(false);
        var dto = JsonSerializer.Deserialize<CallookResponse>(json, Json);

        if (dto is null
            || !string.Equals(dto.Status, ValidStatus, StringComparison.OrdinalIgnoreCase))
        {
            // The service answered and does not know this callsign. That is an
            // answer, not a failure — the caller must not retry it forever.
            return null;
        }

        var cls = ParseClass(dto.Current?.OperClass);
        if (cls == LicenseClass.Unknown)
        {
            return null;
        }

        return new CallsignLookupResult(
            string.IsNullOrWhiteSpace(dto.Current?.Callsign) ? call : dto.Current!.Callsign!,
            cls,
            ServiceName,
            _utcNow());
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
    /// Map the service's class string to a license class.
    /// </summary>
    /// <param name="operClass">The service's value, e.g. "GENERAL".</param>
    /// <returns>The class, or Unknown when unrecognized.</returns>
    /// <remarks>
    /// An unrecognized value returns Unknown rather than a nearest guess. The
    /// whole feature exists to state privileges correctly, and a class Hamlet
    /// invented would be the one lie with legal consequences (HM-DEC-009).
    /// </remarks>
    internal static LicenseClass ParseClass(string? operClass)
        => (operClass ?? "").Trim().ToUpperInvariant() switch
        {
            "NOVICE" => LicenseClass.Novice,
            "TECHNICIAN" or "TECHNICIAN PLUS" or "TECH" => LicenseClass.Technician,
            "GENERAL" => LicenseClass.General,
            "ADVANCED" => LicenseClass.Advanced,
            "EXTRA" or "AMATEUR EXTRA" => LicenseClass.Extra,
            _ => LicenseClass.Unknown,
        };

    /// <summary>
    /// The two fields Hamlet reads. The response carries more — a name, a
    /// street address, coordinates — and this type does not name them, so
    /// there is nothing to accidentally start using.
    /// </summary>
    private sealed class CallookResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("current")]
        public CurrentBlock? Current { get; set; }

        public sealed class CurrentBlock
        {
            [JsonPropertyName("callsign")]
            public string? Callsign { get; set; }

            [JsonPropertyName("operClass")]
            public string? OperClass { get; set; }
        }
    }
}
