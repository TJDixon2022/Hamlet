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

    /// <summary>
    /// Which writes throw, counted from one over both write paths in the order
    /// they are attempted.
    /// </summary>
    /// <remarks>
    /// **A DYING PORT IS THE INTERESTING CASE AND THERE WAS NO WAY TO SCRIPT IT**
    /// (work instruction 255, task 1). This fake could refuse to open and could
    /// hang on a read; it always took a write. A transmit sequence has to come out
    /// of transmit when the port dies *between* the keying write and the unkey,
    /// which needs the second write to fail while the third and fourth still land.
    /// Hence a set of write numbers rather than a flag: the number is the position
    /// on the wire, so a test says "the unkey is write two" and reads exactly like
    /// what it is proving.
    /// </remarks>
    public HashSet<int> WritesThatThrow { get; } = new();

    /// <summary>How many writes have been attempted, thrown or not.</summary>
    public int WritesAttempted { get; private set; }

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
            RefuseIfScripted();
            _written.AddRange(buffer.ToArray());
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>The abort path's synchronous write (§0.2).</summary>
    public void Write(ReadOnlySpan<byte> buffer)
    {
        lock (_writeLock)
        {
            RefuseIfScripted();
            _written.AddRange(buffer.ToArray());
        }
    }

    /// <summary>
    /// Count this write and throw where the test scripted this one to fail.
    /// </summary>
    /// <remarks>
    /// The bytes of a refused write never reach <see cref="Written"/>, because a
    /// port that threw did not take them. A test asserting what the radio saw
    /// would otherwise be asserting what the driver was handed.
    /// </remarks>
    private void RefuseIfScripted()
    {
        WritesAttempted++;

        if (WritesThatThrow.Contains(WritesAttempted))
        {
            throw new IOException($"write {WritesAttempted} refused (scripted).");
        }
    }

    public void Dispose() => Close();
}
