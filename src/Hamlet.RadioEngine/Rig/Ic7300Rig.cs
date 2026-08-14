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
/// <para>Command bytes are verified against the Full Manual's section 19
/// command table with the page on every row (HM-DEC-049); see
/// <see cref="CivReads"/>. Every frame in and out is surfaced via
/// <see cref="FrameTrace"/> so a session can log wire traffic verbatim
/// (§0.0.1).</para>
/// <para>READS ONLY, apart from frequency. Nothing added by HM-DEC-050 writes
/// to the radio: the state reads issue a command with its sub-command and no
/// payload, which is the read form of commands the manual documents as
/// "send/read". Changing somebody's rig gets its own ruling.</para>
/// </remarks>
public sealed class Ic7300Rig : IRig, IDisposable
{
    private static readonly TimeSpan ResponseTimeout = TimeSpan.FromMilliseconds(750);

    /// <summary>
    /// How long teardown waits for the read loop before abandoning it.
    /// </summary>
    /// <remarks>
    /// LONG ENOUGH FOR THE ORDINARY CASE AND SHORT ENOUGH TO NEVER MATTER.
    /// Closing the handle normally makes the parked read fault within
    /// milliseconds, so this budget is not usually spent at all. When it is,
    /// the loop is genuinely stuck and waiting longer would only make the
    /// button stay dead for longer.
    /// </remarks>
    private static readonly TimeSpan ReadLoopStopTimeout = TimeSpan.FromMilliseconds(500);

    private readonly ISerialPort _port;
    private readonly byte _radioAddress;
    private readonly byte _controllerAddress;
    private readonly SemaphoreSlim _commandGate = new(1, 1);
    private readonly CivFrameReader _reader = new();

    private CancellationTokenSource? _readLoopCts;
    private Task? _readLoop;
    private TaskCompletionSource<CivFrame>? _pending;
    private byte[]? _pendingSubCommand;
    private CivMode? _lastMode;
    private string? _lastFilterName;

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
    /// <remarks>Always false: there is a radio on the other end of this.</remarks>
    public bool IsSimulated => false;

    /// <inheritdoc/>
    /// <remarks>
    /// The 7300's own feature set: a spectrum scope over CI-V 0x27
    /// (HM-DEC-005), an internal keyer driven by 0x17, and a USB audio codec
    /// on the same cable (§4). CanTransmit is true of the radio; whether
    /// Hamlet will key it is a separate question answered by
    /// <see cref="Licensing.TransmitGuard"/> and gated on HM-DEC-008.
    /// </remarks>
    public RigCapabilities Capabilities { get; } = new(
        "IC-7300",
        HasSpectrumScope: true,
        HasBuiltInCwKeyer: true,
        HasUsbAudio: true,
        CanTransmit: true,
        Bands.BandPlan.Bands.Select(b => b.Name).ToList());

    /// <inheritdoc/>
    public bool IsConnected { get; private set; }

    /// <inheritdoc/>
    public event EventHandler<FrequencyChangedEventArgs>? FrequencyChanged;

    /// <inheritdoc/>
    public event EventHandler<RigValuesReportedEventArgs>? ValuesReported;

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
                CivConstants.CmdReadFrequency, Array.Empty<byte>(),
                cancellationToken).ConfigureAwait(false);
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
    /// <remarks>
    /// Returns promptly whatever the port does, and never throws (§8).
    /// Disconnecting is the operator asking to be let go, and an app that
    /// cannot honor that has taken their radio hostage.
    /// </remarks>
    public async Task DisconnectAsync() => await TearDownAsync().ConfigureAwait(false);

    /// <inheritdoc/>
    /// <exception cref="TimeoutException">The rig stopped answering.</exception>
    public async Task<long> GetFrequencyHzAsync(CancellationToken cancellationToken = default)
    {
        var response = await RequestAsync(CivConstants.CmdReadFrequency, Array.Empty<byte>(),
            CivConstants.CmdReadFrequency, Array.Empty<byte>(),
            cancellationToken).ConfigureAwait(false);
        return Bcd.DecodeFrequencyHz(response.Data);
    }

    /// <inheritdoc/>
    /// <exception cref="TimeoutException">The rig stopped answering.</exception>
    /// <exception cref="InvalidOperationException">The rig refused (NG).</exception>
    public async Task SetFrequencyHzAsync(long frequencyHz, CancellationToken cancellationToken = default)
    {
        var response = await RequestAsync(CivConstants.CmdSetFrequency,
            Bcd.EncodeFrequencyHz(frequencyHz), null, null,
            cancellationToken).ConfigureAwait(false);

        if (response.Command == CivConstants.ResultNg)
        {
            throw new InvalidOperationException(
                $"Rig refused set-frequency {frequencyHz} Hz (NG).");
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Never throws. A radio that stopped answering is a condition, not an
    /// error, so a timed-out read comes back unknown with the reason attached
    /// and the caller moves on. Retrying here would turn one unresponsive value
    /// into a stream of commands on a bus that is already struggling
    /// (HM-DEC-050).
    /// </remarks>
    public async Task<IReadOnlyList<RigValue>> ReadAsync(
        RigField field, RigState context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (CivReads.Undocumented.TryGetValue(field, out var why))
        {
            return new[] { RigValue.Undocumented(field, why) };
        }

        if (CivReads.For(field) is not { } read)
        {
            return new[]
            {
                RigValue.Unsupported(field, Capabilities.Model + ": Hamlet reads nothing for this"),
            };
        }

        try
        {
            var response = await RequestAsync(
                read.Command, read.SubCommand, read.Command, read.SubCommand,
                cancellationToken).ConfigureAwait(false);

            var values = Decode(read, response, ModeFrom(context), FilterFrom(context));
            RememberModeAndFilter(values);
            return values;
        }
        catch (TimeoutException)
        {
            return new[]
            {
                RigValue.Unknown(field, read.Label + " did not answer within the timeout"),
            };
        }
        catch (OperationCanceledException)
        {
            return new[] { RigValue.Unknown(field, read.Label + " was cancelled") };
        }
    }

    /// <summary>
    /// Decode a response, outside the async method.
    /// </summary>
    /// <remarks>
    /// Separate because a span cannot live across an await in C# 12, and
    /// copying the payload to a fresh array on every read would allocate on a
    /// path that runs several times a second for as long as the app is open.
    /// </remarks>
    private static IReadOnlyList<RigValue> Decode(
        CivRead read, CivFrame response, CivMode? mode, string? filterName)
    {
        // The radio echoes the sub-command in front of the payload, so the
        // payload starts after it.
        var payload = response.Data.Length >= read.SubCommand.Length
            ? response.Data.AsSpan(read.SubCommand.Length)
            : ReadOnlySpan<byte>.Empty;

        return CivDecode.Values(read, payload, DateTime.UtcNow, mode, filterName);
    }

    /// <summary>The mode to decode against: what the caller knows, or what was
    /// last seen here.</summary>
    private CivMode? ModeFrom(RigState context) => context.Mode ?? _lastMode;

    /// <summary>The filter designator to decode against.</summary>
    private string? FilterFrom(RigState context)
        => context[RigField.FilterSelection] is { IsKnown: true } filter
            ? filter.Text
            : _lastFilterName;

    /// <summary>
    /// Keep the mode and filter to hand, because the filter width cannot be
    /// decoded without them.
    /// </summary>
    private void RememberModeAndFilter(IReadOnlyList<RigValue> values)
    {
        foreach (var value in values)
        {
            if (value is { Field: RigField.Mode, IsKnown: true, Number: { } mode })
            {
                _lastMode = (CivMode)(int)mode;
            }
            else if (value is { Field: RigField.FilterSelection, IsKnown: true } filter)
            {
                _lastFilterName = filter.Text;
            }
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Bounded, like <see cref="DisconnectAsync"/>, so a stuck read loop cannot
    /// wedge shutdown. In the ordinary path teardown has already run and this
    /// returns at once.
    /// </remarks>
    public void Dispose()
    {
        try
        {
            TearDownAsync().GetAwaiter().GetResult();
        }
        catch (Exception)
        {
            // Shutdown never throws (§8).
        }

        _commandGate.Dispose();
        _port.Dispose();
    }

    /// <summary>Send one command and await the matching response frame:
    /// either the echo-back of <paramref name="expectedResponseCommand"/> or,
    /// when null, the radio's OK/NG result.</summary>
    private async Task<CivFrame> RequestAsync(
        byte command, byte[] data, byte? expectedResponseCommand,
        byte[]? expectedSubCommand,
        CancellationToken cancellationToken)
    {
        await _commandGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var tcs = new TaskCompletionSource<CivFrame>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            _pendingExpected = expectedResponseCommand;
            _pendingSubCommand = expectedSubCommand;
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
            _pendingSubCommand = null;
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
            var hz = Bcd.DecodeFrequencyHz(frame.Data);
            FrequencyChanged?.Invoke(this, new FrequencyChangedEventArgs(hz));

            ValuesReported?.Invoke(this, new RigValuesReportedEventArgs(new[]
            {
                RigValue.Known(
                    RigField.Frequency, hz, FrequencyText(hz),
                    DateTime.UtcNow, "transceive 00"),
            }));

            return;
        }

        // The operator changing mode on the radio's own front panel. Better
        // than polling for it in every way: instant, free of bus traffic, and
        // it cannot be stale (HM-DEC-050).
        if (frame.Command == CivConstants.CmdTransceiveMode && frame.Data.Length >= 1)
        {
            var values = CivDecode.Values(
                CivReads.ModeAndFilter, frame.Data, DateTime.UtcNow,
                _lastMode, _lastFilterName, "transceive 01");

            RememberModeAndFilter(values);
            ValuesReported?.Invoke(this, new RigValuesReportedEventArgs(values));
            return;
        }

        // Response to the command in flight.
        var pending = _pending;
        if (pending is null)
        {
            return;
        }

        var expected = _pendingExpected;
        var isExpectedEcho = expected.HasValue
                             && frame.Command == expected.Value
                             && SubCommandMatches(frame);
        var isResult = frame.Command is CivConstants.ResultOk or CivConstants.ResultNg;

        if (isExpectedEcho || (!expected.HasValue && isResult))
        {
            pending.TrySetResult(frame);
        }
    }

    /// <summary>
    /// Whether a frame echoes back the sub-command that was asked for.
    /// </summary>
    /// <remarks>
    /// MATCHING ON THE COMMAND BYTE ALONE IS NOT ENOUGH once there is more than
    /// one read per command. The AGC, the preamp and the noise blanker are all
    /// command 16 and differ only in their sub-command, so a reply to one would
    /// otherwise satisfy a request for another and the model would fill a field
    /// with a different field's value. The radio echoes the sub-command in
    /// front of the payload, which is exactly what makes this checkable.
    /// </remarks>
    private bool SubCommandMatches(CivFrame frame)
    {
        var expected = _pendingSubCommand;

        if (expected is null || expected.Length == 0)
        {
            return true;
        }

        if (frame.Data.Length < expected.Length)
        {
            return false;
        }

        for (var i = 0; i < expected.Length; i++)
        {
            if (frame.Data[i] != expected[i])
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>A frequency in the form the diagnostics screen shows it.</summary>
    internal static string FrequencyText(long hz)
        => (hz / 1_000_000.0).ToString("0.000000", System.Globalization.CultureInfo.InvariantCulture)
           + " MHz";

    /// <summary>
    /// Stop the read loop and close the port, promptly and whatever happens.
    /// </summary>
    /// <remarks>
    /// <para>THIS USED TO HANG FOREVER ON A REAL RADIO, and it is worth writing
    /// down why so nobody reintroduces it. The old order was: cancel the token,
    /// then await the read loop, then close the port. On Windows
    /// <c>SerialPort.BaseStream.ReadAsync</c> does not observe its cancellation
    /// token, so the loop stayed parked inside a read that was never going to
    /// return, the await never completed, and everything downstream waited with
    /// it. The Disconnect button and the port list stayed disabled and the app
    /// believed it was still connected, because the line that said otherwise sat
    /// after the await.</para>
    /// <para>So the order is inverted. Connection state drops first, because no
    /// port behavior may leave the UI believing something untrue. The handle is
    /// closed next, which is what actually makes a parked read return. Only then
    /// is the loop waited for, and only for a bounded moment: if it has not
    /// finished by then it is abandoned rather than waited on, since a loop that
    /// survives its own port being closed is not going to finish because
    /// somebody waited longer.</para>
    /// <para>Never throws, and never blocks past the budget. Disconnecting is
    /// the one thing the operator must always be able to do (§8).</para>
    /// </remarks>
    private async Task TearDownAsync()
    {
        // First, and unconditionally. Everything below is best effort; this is
        // not.
        IsConnected = false;

        var cts = _readLoopCts;
        var loop = _readLoop;
        _readLoopCts = null;
        _readLoop = null;

        // Closing before cancelling is the whole fix: it is the handle going
        // away that makes a parked read fault, not the token.
        try
        {
            _port.Close();
        }
        catch (Exception)
        {
            // A port that refuses to close is still a port we are done with.
        }

        cts?.Cancel();

        var finished = loop is null;

        if (loop is not null)
        {
            try
            {
                await loop.WaitAsync(ReadLoopStopTimeout).ConfigureAwait(false);
                finished = true;
            }
            catch (TimeoutException)
            {
                // Genuinely stuck. Abandoned on purpose.
            }
            catch (Exception)
            {
                // Faulted on the closed port, which is the expected way out.
                finished = true;
            }
        }

        // Only disposed when the loop is definitely done with it. An abandoned
        // loop still holding a disposed token would throw on its next check,
        // which it would swallow, but there is no reason to hand it that.
        if (finished)
        {
            cts?.Dispose();
        }
    }
}
