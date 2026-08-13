using System.Net.Sockets;
using System.Text;

namespace Hamlet.RadioEngine.Explore;

/// <summary>
/// A line-oriented network conversation — the seam the RBN telnet client is
/// built on, so tests drive it from a script instead of the live cluster (§5).
/// </summary>
public interface ITextConnection : IDisposable
{
    /// <summary>True once connected and not yet torn down.</summary>
    bool IsConnected { get; }

    /// <summary>Open the connection.</summary>
    /// <param name="cancellationToken">Cancels the attempt.</param>
    /// <returns>A task that completes when the socket is open.</returns>
    Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Read the next line, or null at end of stream.
    /// </summary>
    /// <param name="cancellationToken">Cancels the read.</param>
    /// <returns>The line without its terminator, or null.</returns>
    /// <remarks>
    /// A login prompt arrives without a newline, so an implementation must be
    /// able to hand back a partial line — see
    /// <see cref="TcpTextConnection"/>.
    /// </remarks>
    Task<string?> ReadLineAsync(CancellationToken cancellationToken = default);

    /// <summary>Send a line, terminated as the far end expects.</summary>
    /// <param name="line">Text to send, without a terminator.</param>
    /// <param name="cancellationToken">Cancels the write.</param>
    /// <returns>A task that completes when the bytes are away.</returns>
    Task WriteLineAsync(string line, CancellationToken cancellationToken = default);
}

/// <summary>A TCP connection that yields text lines.</summary>
/// <remarks>
/// Cluster servers prompt for a callsign without a trailing newline, so a
/// plain <c>StreamReader.ReadLineAsync</c> blocks forever waiting for one.
/// This reader therefore returns whatever has arrived when the stream goes
/// quiet, which is what makes the login handshake work at all.
/// </remarks>
public sealed class TcpTextConnection : ITextConnection
{
    private readonly string _host;
    private readonly int _port;
    private readonly TimeSpan _quietTime;
    private readonly StringBuilder _pending = new();

    private TcpClient? _client;
    private NetworkStream? _stream;
    private readonly byte[] _buffer = new byte[8192];

    /// <summary>Creates the connection.</summary>
    /// <param name="host">Host name.</param>
    /// <param name="port">TCP port.</param>
    /// <param name="quietTime">How long a partial line may sit before it is
    /// handed back as-is; this is what releases a prompt with no newline.</param>
    public TcpTextConnection(string host, int port, TimeSpan? quietTime = null)
    {
        _host = host;
        _port = port;
        _quietTime = quietTime ?? TimeSpan.FromMilliseconds(400);
    }

    /// <inheritdoc/>
    public bool IsConnected => _client?.Connected == true;

    /// <inheritdoc/>
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        _client = new TcpClient();
        await _client.ConnectAsync(_host, _port, cancellationToken).ConfigureAwait(false);
        _stream = _client.GetStream();
    }

    /// <inheritdoc/>
    public async Task<string?> ReadLineAsync(CancellationToken cancellationToken = default)
    {
        if (_stream is null)
        {
            return null;
        }

        while (true)
        {
            var line = TakeLine();
            if (line is not null)
            {
                return line;
            }

            using var quiet = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            if (_pending.Length > 0)
            {
                quiet.CancelAfter(_quietTime);
            }

            int read;
            try
            {
                read = await _stream.ReadAsync(_buffer, quiet.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // The stream went quiet mid-line: that is the prompt.
                var partial = _pending.ToString();
                _pending.Clear();
                return partial;
            }

            if (read == 0)
            {
                return null;
            }

            _pending.Append(Encoding.ASCII.GetString(_buffer, 0, read));
        }
    }

    /// <inheritdoc/>
    public async Task WriteLineAsync(string line, CancellationToken cancellationToken = default)
    {
        if (_stream is null)
        {
            return;
        }

        var bytes = Encoding.ASCII.GetBytes(line + "\r\n");
        await _stream.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
        await _stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _stream?.Dispose();
        _client?.Dispose();
        _stream = null;
        _client = null;
    }

    private string? TakeLine()
    {
        for (var i = 0; i < _pending.Length; i++)
        {
            if (_pending[i] != '\n')
            {
                continue;
            }

            var line = _pending.ToString(0, i).TrimEnd('\r');
            _pending.Remove(0, i + 1);
            return line;
        }

        return null;
    }
}
