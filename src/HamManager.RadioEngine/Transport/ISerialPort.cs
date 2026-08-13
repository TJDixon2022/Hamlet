namespace HamManager.RadioEngine.Transport;

/// <summary>
/// The engine's seam over a serial port (§6: hand-rolled interfaces, no
/// mocking framework). The system implementation wraps System.IO.Ports;
/// tests substitute an in-memory fake.
/// </summary>
public interface ISerialPort : IDisposable
{
    /// <summary>True while the port is open.</summary>
    bool IsOpen { get; }

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
}
