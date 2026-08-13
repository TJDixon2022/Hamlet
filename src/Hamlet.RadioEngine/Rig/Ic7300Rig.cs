using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Transport;

namespace Hamlet.RadioEngine.Rig;

/// <summary>
/// <see cref="IRig"/> over CI-V to an IC-7300. One command in flight at a
/// time; a background loop reads frames, answers pending requests, and
/// raises <see cref="IRig.FrequencyChanged"/> for the radio's unsolicited
/// transceive reports (the operator's VFO knob).
/// </summary>
/// <remarks>
/// Command bytes are unverified against the manual (HM-OPEN-002). This class
/// is proven against <c>FakeSerialPort</c> canned traffic; live-rig trust
/// waits for the vendored citation. Every frame in and out is surfaced via
/// <see cref="FrameTrace"/> so a session can log wire traffic verbatim
/// (§0.0.1).
/// </remarks>
public sealed class Ic7300Rig : IRig, IDisposable
{
    private static readonly TimeSpan ResponseTimeout = TimeSpan.FromMilliseconds(750);

    private readonly ISerialPort _port;
    private readonly byte _radioAddress;
    private readonly byte _controllerAddress;
    private readonly SemaphoreSlim _commandGate = new(1, 1);
    private readonly CivFrameReader _reader = new();

    private CancellationTokenSource? _readLoopCts;
    private Task? _readLoop;
    private TaskCompletionSource<CivFrame>? _pending;

    /// <summary>Create a rig over an open-able port. Addresses default to the
    /// CI-V conventions; both are radio-menu settings (HM-OPEN-003).</summary>
    public Ic7300Rig(
        ISerialPort port,
        byte radioAddress = CivConstants.DefaultRadioAddress,
        byte controllerAddress = CivConstants.DefaultControllerAddress)
    {
        _port = port;
        _radioAddress = radioAddress;
        _controllerAddress = controllerAddress;
    }

    /// <inheritdoc/>
    public bool IsConnected { get; private set; }

    /// <inheritdoc/>
    public event EventHandler<FrequencyChangedEventArgs>? FrequencyChanged;

    /// <summary>Every frame sent (out=true) or received (out=false), verbatim,
    /// for the CI-V log. Raised on the read loop thread.</summary>
    public event Action<bool, CivFrame>? FrameTrace;

    /// <inheritdoc/>
    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (IsConnected)
        {
            return true;
        }

        try
        {
            _port.Open();
        }
        catch (Exception)
        {
            // Unreachable rig is a condition, not an exception (IRig contract).
            return false;
        }

        _readLoopCts = new CancellationTokenSource();
        _readLoop = Task.Run(() => ReadLoopAsync(_readLoopCts.Token), CancellationToken.None);

        // Probe: a frequency read proves radio, address and baud all agree.
        try
        {
            await RequestAsync(CivConstants.CmdReadFrequency, Array.Empty<byte>(),
                CivConstants.CmdReadFrequency, cancellationToken).ConfigureAwait(false);
            IsConnected = true;
            return true;
        }
        catch (TimeoutException)
        {
            await TearDownAsync().ConfigureAwait(false);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task DisconnectAsync()
    {
        await TearDownAsync().ConfigureAwait(false);
        IsConnected = false;
    }

    /// <inheritdoc/>
    /// <exception cref="TimeoutException">The rig stopped answering.</exception>
    public async Task<long> GetFrequencyHzAsync(CancellationToken cancellationToken = default)
    {
        var response = await RequestAsync(CivConstants.CmdReadFrequency, Array.Empty<byte>(),
            CivConstants.CmdReadFrequency, cancellationToken).ConfigureAwait(false);
        return Bcd.DecodeFrequencyHz(response.Data);
    }

    /// <inheritdoc/>
    /// <exception cref="TimeoutException">The rig stopped answering.</exception>
    /// <exception cref="InvalidOperationException">The rig refused (NG).</exception>
    public async Task SetFrequencyHzAsync(long frequencyHz, CancellationToken cancellationToken = default)
    {
        var response = await RequestAsync(CivConstants.CmdSetFrequency,
            Bcd.EncodeFrequencyHz(frequencyHz), null, cancellationToken).ConfigureAwait(false);

        if (response.Command == CivConstants.ResultNg)
        {
            throw new InvalidOperationException(
                $"Rig refused set-frequency {frequencyHz} Hz (NG).");
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        TearDownAsync().GetAwaiter().GetResult();
        _commandGate.Dispose();
        _port.Dispose();
    }

    /// <summary>Send one command and await the matching response frame:
    /// either the echo-back of <paramref name="expectedResponseCommand"/> or,
    /// when null, the radio's OK/NG result.</summary>
    private async Task<CivFrame> RequestAsync(
        byte command, byte[] data, byte? expectedResponseCommand,
        CancellationToken cancellationToken)
    {
        await _commandGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var tcs = new TaskCompletionSource<CivFrame>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            _pendingExpected = expectedResponseCommand;
            _pending = tcs;

            var frame = new CivFrame(_radioAddress, _controllerAddress, command, data);
            FrameTrace?.Invoke(true, frame);
            await _port.WriteAsync(frame.ToWireBytes(), cancellationToken).ConfigureAwait(false);

            var winner = await Task.WhenAny(
                tcs.Task, Task.Delay(ResponseTimeout, cancellationToken)).ConfigureAwait(false);
            if (winner != tcs.Task)
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw new TimeoutException(
                    $"No CI-V response to 0x{command:X2} within {ResponseTimeout.TotalMilliseconds} ms.");
            }

            return await tcs.Task.ConfigureAwait(false);
        }
        finally
        {
            _pending = null;
            _pendingExpected = null;
            _commandGate.Release();
        }
    }

    private byte? _pendingExpected;

    private async Task ReadLoopAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[256];
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var read = await _port.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                if (read <= 0)
                {
                    continue;
                }

                foreach (var frame in _reader.Feed(buffer.AsSpan(0, read)))
                {
                    HandleFrame(frame);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }
        catch (Exception)
        {
            // Port died underneath us. Never-throw discipline (§8): the loop
            // ends, IsConnected goes false, the next command times out loudly.
            IsConnected = false;
        }
    }

    private void HandleFrame(CivFrame frame)
    {
        FrameTrace?.Invoke(false, frame);

        // Our own transmission echoed back by the CI-V bus: ignore.
        if (frame.From == _controllerAddress)
        {
            return;
        }

        // Unsolicited transceive report — the operator's knob.
        if (frame.Command == CivConstants.CmdTransceiveFrequency
            && frame.Data.Length == Bcd.FrequencyByteCount)
        {
            FrequencyChanged?.Invoke(this,
                new FrequencyChangedEventArgs(Bcd.DecodeFrequencyHz(frame.Data)));
            return;
        }

        // Response to the command in flight.
        var pending = _pending;
        if (pending is null)
        {
            return;
        }

        var expected = _pendingExpected;
        var isExpectedEcho = expected.HasValue && frame.Command == expected.Value;
        var isResult = frame.Command is CivConstants.ResultOk or CivConstants.ResultNg;

        if (isExpectedEcho || (!expected.HasValue && isResult))
        {
            pending.TrySetResult(frame);
        }
    }

    private async Task TearDownAsync()
    {
        if (_readLoopCts is not null)
        {
            _readLoopCts.Cancel();
            if (_readLoop is not null)
            {
                try
                {
                    await _readLoop.ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // Shutdown path never throws (§8).
                }
            }

            _readLoopCts.Dispose();
            _readLoopCts = null;
            _readLoop = null;
        }

        _port.Close();
    }
}
