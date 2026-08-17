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
    /// <inheritdoc />
    public string PortName { get; init; } = "COM-TEST";

    /// <inheritdoc />
    public int BaudRate { get; init; } = 115_200;

    private readonly Channel<byte[]> _incoming = Channel.CreateUnbounded<byte[]>();
    private readonly List<byte> _written = new();
    private readonly object _writeLock = new();

    public bool IsOpen { get; private set; }

    /// <summary>When true, <see cref="Open"/> throws — the "port doesn't
    /// exist" condition ConnectAsync must translate to false.</summary>
    public bool FailOnOpen { get; set; }

    /// <summary>
    /// When true, <see cref="ReadAsync"/> never returns and ignores its
    /// cancellation token, closed port or not.
    /// </summary>
    /// <remarks>
    /// THIS IS WINDOWS, NOT A HYPOTHETICAL.
    /// <c>SerialPort.BaseStream.ReadAsync</c> has a long history of ignoring
    /// the token it is handed, which is what left Disconnect dead against a
    /// real IC-7300 (HM-DEC-051). Modelling the worst version of it here, where
    /// even closing the handle does not free the read, is what proves teardown
    /// gives up rather than waits.
    /// </remarks>
    public bool ReadNeverReturns { get; set; }

    /// <summary>True once a stuck read has actually been entered.</summary>
    public bool IsReadParked { get; private set; }

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
        if (ReadNeverReturns)
        {
            IsReadParked = true;

            // No token, no timeout, no way out. Exactly the behavior that hung
            // the app, so that the fix is proved against the real failure rather
            // than against a polite imitation of it.
            await new TaskCompletionSource<int>().Task.ConfigureAwait(false);
        }

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

    /// <summary>The abort path's synchronous write (§0.2).</summary>
    public void Write(ReadOnlySpan<byte> buffer)
    {
        lock (_writeLock)
        {
            _written.AddRange(buffer.ToArray());
        }
    }

    public void Dispose() => Close();
}
