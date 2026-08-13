using System.Threading.Channels;
using Hamlet.RadioEngine.Transport;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// In-memory <see cref="ISerialPort"/>: tests script the radio's side by
/// enqueuing incoming bytes and inspecting what the rig wrote. The engine's
/// hand-rolled-seams rule (§6) instead of a mocking framework.
/// </summary>
internal sealed class FakeSerialPort : ISerialPort
{
    private readonly Channel<byte[]> _incoming = Channel.CreateUnbounded<byte[]>();
    private readonly List<byte> _written = new();
    private readonly object _writeLock = new();

    public bool IsOpen { get; private set; }

    /// <summary>When true, <see cref="Open"/> throws — the "port doesn't
    /// exist" condition ConnectAsync must translate to false.</summary>
    public bool FailOnOpen { get; set; }

    /// <summary>Everything the rig has written, as one contiguous byte run.</summary>
    public byte[] Written
    {
        get { lock (_writeLock) { return _written.ToArray(); } }
    }

    /// <summary>Script bytes "from the radio"; the rig's read loop will see
    /// them on its next read.</summary>
    public void EnqueueIncoming(params byte[] bytes)
        => _incoming.Writer.TryWrite(bytes);

    public void Open()
    {
        if (FailOnOpen)
        {
            throw new IOException("No such port (scripted).");
        }

        IsOpen = true;
    }

    public void Close() => IsOpen = false;

    public async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        var chunk = await _incoming.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
        chunk.CopyTo(buffer);
        return chunk.Length;
    }

    public ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
    {
        lock (_writeLock)
        {
            _written.AddRange(buffer.ToArray());
        }

        return ValueTask.CompletedTask;
    }

    public void Dispose() => Close();
}
