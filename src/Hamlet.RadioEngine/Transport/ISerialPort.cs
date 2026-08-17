namespace Hamlet.RadioEngine.Transport;

/// <summary>
/// The engine's seam over a serial port (§6: hand-rolled interfaces, no
/// mocking framework). The system implementation wraps System.IO.Ports;
/// tests substitute an in-memory fake.
/// </summary>
public interface ISerialPort : IDisposable
{
    /// <summary>True while the port is open.</summary>
    bool IsOpen { get; }

    /// <summary>What the port is called, for the record.</summary>
    string PortName { get; }

    /// <summary>
    /// The rate it was opened at (HM-DEC-092).
    /// </summary>
    /// <remarks>
    /// One of the two preconditions on the scope's data output is a baud rate,
    /// and it is one Hamlet does not have to ask the radio about: it opened the
    /// port itself. Reading it back off the port is the difference between
    /// knowing and assuming.
    /// </remarks>
    int BaudRate { get; }

    /// <summary>Open the port. Throws on failure — the caller (the rig)
    /// translates that into its "unreachable is a condition" contract.</summary>
    void Open();

    /// <summary>Close the port. Safe when already closed.</summary>
    void Close();

    /// <summary>Read available bytes into <paramref name="buffer"/>; returns
    /// the count read. Blocks (asynchronously) until at least one byte or
    /// cancellation.</summary>
    ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken);

    /// <summary>Write the whole buffer to the port.</summary>
    ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken);

    /// <summary>
    /// Write the whole buffer, on this thread, awaiting nothing.
    /// </summary>
    /// <param name="buffer">The bytes.</param>
    /// <remarks>
    /// THE ABORT PATH AND NOTHING ELSE (§0.2, HM-DEC-059). Every ordinary write
    /// goes through <see cref="WriteAsync"/> behind the rig's command gate. This
    /// exists because the moment somebody wants a transmitter to stop is the
    /// moment they cannot wait for a task to be scheduled behind whatever is
    /// already queued, and a stop that waits its turn behind the send it is
    /// stopping is not a stop.
    /// </remarks>
    void Write(ReadOnlySpan<byte> buffer);
}
