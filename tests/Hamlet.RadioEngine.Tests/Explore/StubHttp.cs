using System.Net;
using System.Net.Http;
using System.Text;
using Hamlet.RadioEngine.Explore;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// An <see cref="HttpMessageHandler"/> that answers from a canned string.
/// </summary>
/// <remarks>
/// No test in this repository reaches the live internet. A test that depended
/// on POTA being up would fail for reasons that have nothing to do with the
/// code, and would quietly stop proving anything the day the shape of the
/// response changed (§5).
/// </remarks>
public sealed class StubHttp : HttpMessageHandler
{
    private readonly string _body;
    private readonly HttpStatusCode _status;

    /// <summary>Answers every request with this body.</summary>
    /// <param name="body">Response body.</param>
    /// <param name="status">Status code to return.</param>
    public StubHttp(string body, HttpStatusCode status = HttpStatusCode.OK)
    {
        _body = body;
        _status = status;
    }

    /// <summary>Requests this handler was asked for.</summary>
    public List<HttpRequestMessage> Requests { get; } = new();

    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);

        return Task.FromResult(new HttpResponseMessage(_status)
        {
            Content = new StringContent(_body, Encoding.UTF8, "application/json"),
        });
    }
}

/// <summary>A source that returns what it is told, or throws on command.</summary>
public sealed class StubSource : IActivitySource
{
    private readonly Func<IReadOnlyList<ActivitySpot>> _get;

    /// <summary>Creates the stub.</summary>
    /// <param name="name">Source name.</param>
    /// <param name="get">Called on every fetch; may throw.</param>
    public StubSource(string name, Func<IReadOnlyList<ActivitySpot>> get)
    {
        Name = name;
        _get = get;
    }

    /// <inheritdoc/>
    public string Name { get; }

    /// <summary>How many times it has been asked.</summary>
    public int Calls { get; private set; }

    /// <inheritdoc/>
    public Task<IReadOnlyList<ActivitySpot>> GetSpotsAsync(
        CancellationToken cancellationToken = default)
    {
        Calls++;
        return Task.FromResult(_get());
    }
}
