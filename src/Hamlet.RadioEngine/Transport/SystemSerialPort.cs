using System.IO.Ports;

namespace Hamlet.RadioEngine.Transport;

/// <summary>
/// <see cref="ISerialPort"/> over <see cref="SerialPort"/>. Reads and writes
/// go through the base stream so they are genuinely asynchronous.
/// </summary>
public sealed class SystemSerialPort : ISerialPort
{
    private readonly SerialPort _port;

    /// <summary>Create an unopened port. 115200 8N1 is the working default
    /// for the IC-7300's USB CI-V; the radio menu must agree (HM-OPEN-003).</summary>
    public SystemSerialPort(string portName, int baudRate = 115_200)
    {
        _port = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
        {
            ReadTimeout = SerialPort.InfiniteTimeout,
            WriteTimeout = 2000,
        };
    }

    /// <inheritdoc/>
    public bool IsOpen => _port.IsOpen;

    /// <inheritdoc/>
    public void Open() => _port.Open();

    /// <inheritdoc/>
    public void Close()
    {
        if (_port.IsOpen)
        {
            _port.Close();
        }
    }

    /// <inheritdoc/>
    public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken)
        => _port.BaseStream.ReadAsync(buffer, cancellationToken);

    /// <inheritdoc/>
    public async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
        => await _port.BaseStream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public void Dispose()
    {
        Close();
        _port.Dispose();
    }

    /// <summary>The serial port names present on this machine, sorted.</summary>
    public static string[] GetPortNames()
    {
        var names = SerialPort.GetPortNames();
        Array.Sort(names, StringComparer.OrdinalIgnoreCase);
        return names;
    }
}
