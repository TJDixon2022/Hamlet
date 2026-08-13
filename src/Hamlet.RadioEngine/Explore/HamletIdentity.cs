using System.Net.Http;
using System.Net.Http.Headers;

namespace Hamlet.RadioEngine.Explore;

/// <summary>
/// How Hamlet introduces itself to somebody else's server.
/// </summary>
/// <remarks>
/// <para>Every request names the app, its version, where its source lives and
/// who is running it, e.g.
/// <c>Hamlet/0.1 (+https://github.com/TJDixon2022/Hamlet; KC3QIS)</c>. These
/// are volunteer-run services with no rate card and no support contract; an
/// operator whose client misbehaves should be reachable, and an anonymous
/// client cannot be warned before it is blocked.</para>
/// <para>The callsign genuinely goes over the wire here — RBN will not accept
/// a login without one — and that changes nothing about HM-DEC-018. Telling
/// POTA who is asking is the courtesy the service is owed; writing the same
/// string into Hamlet's own telemetry file would be surveillance of the
/// operator by their own software. The two are not the same act, and only one
/// of them is permitted (HM-DEC-024).</para>
/// </remarks>
public static class HamletIdentity
{
    /// <summary>Where the source lives, for a server admin who needs to look
    /// up what is calling them.</summary>
    public const string ProjectUrl = "https://github.com/TJDixon2022/Hamlet";

    /// <summary>The product token used in every User-Agent.</summary>
    public const string Product = "Hamlet";

    /// <summary>
    /// Build the User-Agent string.
    /// </summary>
    /// <param name="version">App version, e.g. "0.1".</param>
    /// <param name="callsign">The operator's callsign, or null/blank when
    /// they have not set one — then the header carries the app and its URL
    /// and simply omits the operator, rather than inventing a placeholder.</param>
    /// <returns>The full User-Agent value.</returns>
    public static string UserAgent(string version, string? callsign)
    {
        var v = string.IsNullOrWhiteSpace(version) ? "0.0" : version.Trim();
        var call = callsign?.Trim();

        return string.IsNullOrEmpty(call)
            ? $"{Product}/{v} (+{ProjectUrl})"
            : $"{Product}/{v} (+{ProjectUrl}; {call.ToUpperInvariant()})";
    }

    /// <summary>
    /// Create an <see cref="HttpClient"/> that identifies itself properly.
    /// </summary>
    /// <param name="version">App version.</param>
    /// <param name="callsign">Operator callsign, or null.</param>
    /// <param name="handler">Transport, injected by tests so no test ever
    /// reaches the live internet (§5). Null uses the default handler.</param>
    /// <param name="timeout">Request timeout; defaults to 20 seconds.</param>
    /// <returns>A configured client the caller owns and disposes.</returns>
    public static HttpClient CreateClient(
        string version,
        string? callsign,
        HttpMessageHandler? handler = null,
        TimeSpan? timeout = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, false);
        client.Timeout = timeout ?? TimeSpan.FromSeconds(20);
        client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent(version, callsign));
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }
}
