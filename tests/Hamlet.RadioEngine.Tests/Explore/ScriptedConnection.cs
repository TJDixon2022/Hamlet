using Hamlet.RadioEngine.Explore;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>A connection that replays a script of lines, then blocks.</summary>
public sealed class ScriptedConnection : ITextConnection
{
    private readonly Queue<string> _lines;
    private readonly TaskCompletionSource _exhausted = new();

    /// <summary>Creates the connection.</summary>
    /// <param name="lines">Lines to hand back, in order.</param>
    public ScriptedConnection(params string[] lines)
        => _lines = new Queue<string>(lines);

    /// <summary>Lines written by the client — the login handshake.</summary>
    public List<string> Written { get; } = new();

    /// <inheritdoc/>
    public bool IsConnected { get; private set; }

    /// <summary>Completes once the script has been fully read.</summary>
    public Task Exhausted => _exhausted.Task;

    /// <inheritdoc/>
    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        IsConnected = true;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<string?> ReadLineAsync(CancellationToken cancellationToken = default)
    {
        if (_lines.Count > 0)
        {
            return Task.FromResult<string?>(_lines.Dequeue());
        }

        _exhausted.TrySetResult();
        return Task.FromResult<string?>(null);
    }

    /// <inheritdoc/>
    public Task WriteLineAsync(string line, CancellationToken cancellationToken = default)
    {
        Written.Add(line);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void Dispose() => IsConnected = false;
}
