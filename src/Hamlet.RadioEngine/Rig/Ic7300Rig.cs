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
        Bands.HfBands.Bands.Select(b => b.Name).ToList());

    /// <inheritdoc/>
    public bool IsConnected { get; private set; }

    /// <inheritdoc/>
    public event EventHandler<FrequencyChangedEventArgs>? FrequencyChanged;

    /// <inheritdoc/>
    public event EventHandler<RigValuesReportedEventArgs>? ValuesReported;

    /// <summary>Every frame sent (out=true) or received (out=false), verbatim,
    /// for the CI-V log. Raised on the read loop thread.</summary>
    public event Action<bool, CivFrame>? FrameTrace;

    /// <summary>
    /// One part of a spectrum sweep, as the radio pushed it (HM-DEC-062).
    /// </summary>
    /// <remarks>
    /// Unsolicited, like the transceive reports beside it. The radio sends these
    /// once its own scope output is on, so Hamlet asks for nothing and the
    /// stream costs the poll loop nothing (HM-DEC-050). The payload is the data
    /// after the echoed sub-command; <see cref="Civ.CivScope"/> reads it.
    /// </remarks>
    public event Action<byte[]>? ScopeData;

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
            // NOTHING TO ASK IS NOT NOTHING TO KNOW. A field with no poll command
            // of its own is answered by another command or pushed by the radio,
            // and reporting either as "not on this radio" is the diagnostics
            // screen contradicting the rig display beside it. Returning nothing
            // leaves whatever is already in the model alone: the broadcast that
            // filled it, or unknown when none has arrived (§0.0).
            if (CivReads.AnsweredBy(field) is not null ||
                CivReads.BroadcastFor(field) is not null)
            {
                return Array.Empty<RigValue>();
            }

            // Unsupported is reserved for what the capabilities record says the
            // radio genuinely lacks (HM-DEC-030). A gap in Hamlet's table is a
            // gap in Hamlet, so it says unknown and names itself as the reason.
            return new[]
            {
                RigValue.Unknown(field, Capabilities.Model + ": Hamlet has no read for this yet"),
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

    /// <inheritdoc/>
    /// <remarks>
    /// <para>THE FIRST WRITE (HM-DEC-056). It goes out through the same gate and
    /// the same trace as every read, so a session log carries it verbatim with
    /// its timestamp like everything else (§0.0.1).</para>
    /// <para>NOTHING IS ASSUMED FROM HAVING SENT IT. The radio acknowledges with
    /// FB or refuses with FA (p. 19-2), and anything else leaves the mode
    /// unknown rather than set to what was asked for. A mode Hamlet believes it
    /// set and did not is a guess presented as a decode, and it would put the
    /// badge and the radio's own face out of step with nothing on screen saying
    /// so (§0.0).</para>
    /// </remarks>
    public async Task<RigWriteResult> SetSettingAsync(
        CivWrite write, int value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(write);

        // NOTHING WRITES A BYTE THAT IS NOT IN THE TABLE (§4, HM-DEC-084). The
        // table is the citation, so a write built from anything else has no page
        // behind it and does not happen.
        if (!CivWrites.All.Contains(write))
        {
            return RigWriteResult.Refused($"{write.Label} is not a documented write");
        }

        try
        {
            var data = BuildSettingData(write, value);

            var response = await RequestAsync(
                write.Command, data, null, null, cancellationToken)
                .ConfigureAwait(false);

            if (response.Command != CivConstants.ResultOk)
            {
                return RigWriteResult.Refused(write.Label);
            }

            // READ IT BACK. An acknowledgement says the radio understood the
            // frame, not that the setting moved, and those come apart on exactly
            // the settings somebody would most want to trust (HM-DEC-084).
            var read = CivReads.All.FirstOrDefault(r => r.Field == write.Field);

            if (read is null)
            {
                return RigWriteResult.Confirmed(write.Label);
            }

            // **THE READBACK HAS TO SAY WHAT IT IS WAITING FOR, AND IT DID NOT.**
            // With no expected command the dispatcher satisfies the request only
            // on `FB` or `FA`, and a readback is answered with the value frame
            // instead — so every readback timed out, every write that the radio
            // took was reported as unanswered, and the setting had moved anyway.
            // HM-DEC-092 saw the symptom from the other end: five settings
            // written one evening, all five reported unanswered, at least two
            // actually in effect. That was read as a link dropping commands. It
            // was this. `27 11` is the same fault: six connects, six failures,
            // and no way to tell them from silence (FACT-003).
            var back = await RequestAsync(
                read.Command, read.SubCommand, read.Command, read.SubCommand,
                cancellationToken)
                .ConfigureAwait(false);

            var values = Decode(read, back, null, null);

            ValuesReported?.Invoke(this, new RigValuesReportedEventArgs(values));

            var confirmed = values.Any(
                v => v.Field == write.Field && v.IsKnown && (int?)v.Number == value);

            return confirmed
                ? RigWriteResult.Confirmed(write.Label)
                : RigWriteResult.ReadBackDisagreed(write.Label);
        }
        catch (TimeoutException)
        {
            return RigWriteResult.NoAnswer(write.Label);
        }
        catch (OperationCanceledException)
        {
            return RigWriteResult.NoAnswer(write.Label);
        }
        catch (Exception)
        {
            return RigWriteResult.NoAnswer(write.Label);
        }
    }

    /// <summary>The data area for one setting write.</summary>
    /// <remarks>
    /// Sub-command bytes first, then the value in whatever shape that command
    /// takes. The 0-to-255 levels go out as BCD decimal digits, which is the
    /// same encoding the reads already decode (p. 19-3).
    /// </remarks>
    private static byte[] BuildSettingData(CivWrite write, int value)
    {
        var sub = write.Sub;

        // A LEVEL IS TWO BCD BYTES; EVERYTHING ELSE IS ONE PLAIN BYTE. Which is
        // which comes from the manual's own range column, carried on the write.
        var isLevel = write.Note.Contains("0000", StringComparison.Ordinal);

        var payload = isLevel
            ? CivWrites.LevelBytes(value)
            : new[] { (byte)value };

        var data = new byte[sub.Length + payload.Length];
        sub.CopyTo(data, 0);
        payload.CopyTo(data, sub.Length);

        return data;
    }

    /// <inheritdoc/>
    public async Task<RigWriteResult> SetModeAsync(
        CivMode mode, bool dataMode, byte? filterSlot = null,
        CancellationToken cancellationToken = default)
    {
        var write = CivWrites.Mode;

        try
        {
            // **ONE FRAME EITHER WAY.** Command 26 already carries the filter
            // byte; skipping it does not leave the filter alone, it selects the
            // mode's default (p. 19-11). So choosing costs nothing on the wire
            // and not choosing was never neutral.
            var data = filterSlot is { } slot
                ? CivWrites.ModeData(mode, dataMode, slot)
                : CivWrites.ModeData(mode, dataMode);

            var response = await RequestAsync(
                write.Command, data, null, null,
                cancellationToken).ConfigureAwait(false);

            if (response.Command == CivConstants.ResultOk)
            {
                var values = await ReadBackTheModeAsync(
                    mode, dataMode, write.Label, cancellationToken)
                    .ConfigureAwait(false);

                ValuesReported?.Invoke(this, new RigValuesReportedEventArgs(values));

                return RigWriteResult.Confirmed(write.Label);
            }

            ReportModeUnknown($"{write.Label} was refused by the radio");
            return RigWriteResult.Refused(write.Label);
        }
        catch (TimeoutException)
        {
            ReportModeUnknown($"{write.Label} was not answered within the timeout");
            return RigWriteResult.NoAnswer(write.Label);
        }
        catch (OperationCanceledException)
        {
            ReportModeUnknown($"{write.Label} was cancelled");
            return RigWriteResult.NoAnswer(write.Label);
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// One keyer message, acknowledged or not. Everything that makes this safe
    /// happened before the call: the transmit guard, the break-in precondition
    /// and the split into pieces the radio will take (HM-DEC-059).
    /// </remarks>
    public async Task<bool> SendCwAsync(
        string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(message))
        {
            return false;
        }

        try
        {
            var response = await RequestAsync(
                CivConstants.CmdSendCwMessage,
                System.Text.Encoding.ASCII.GetBytes(message),
                null, null, cancellationToken).ConfigureAwait(false);

            return response.Command == CivConstants.ResultOk;
        }
        catch (TimeoutException)
        {
            return false;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <para>STRAIGHT AT THE PORT, ON THIS THREAD (§0.2). Every other frame in
    /// this class goes out behind <c>_commandGate</c>, which is right for
    /// politeness on a slow bus and wrong for this: a stop queued behind the
    /// send it is stopping would arrive after the message finished, which is not
    /// a stop at all.</para>
    /// <para>It never throws. An abort that could fail is not an abort, so a
    /// closed port, a disposed rig and a port that refuses the write all end the
    /// same way: nothing happens and nothing propagates (§8).</para>
    /// </remarks>
    public void AbortCw()
    {
        try
        {
            var frame = new CivFrame(
                _radioAddress, _controllerAddress, CivConstants.CmdSendCwMessage,
                new[] { CivConstants.CwStopByte });

            FrameTrace?.Invoke(true, frame);
            _port.Write(frame.ToWireBytes());
        }
        catch (Exception)
        {
            // Nothing here is worth taking the app down for, least of all on
            // the path somebody reaches for when something has gone wrong.
        }
    }

    /// <summary>Mark the mode unknown after a write nobody confirmed.</summary>
    private void ReportModeUnknown(string why)
    {
        _lastMode = null;

        ValuesReported?.Invoke(this, new RigValuesReportedEventArgs(new[]
        {
            RigValue.Unknown(RigField.Mode, why),
        }));
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

    /// <summary>
    /// Ask the radio what the mode write actually left behind.
    /// </summary>
    /// <param name="mode">What was asked for.</param>
    /// <param name="dataMode">Whether the data variant was asked for.</param>
    /// <param name="label">The write, for provenance on the fallback.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>The mode, the data flag, the filter slot and its width.</returns>
    /// <remarks>
    /// <para>**AN ACKNOWLEDGEMENT IS NOT A READING** (work instruction 042, task
    /// 1). This used to fold the request into the model — mode and data flag,
    /// stamped with the moment the acknowledgement arrived and sourced to the
    /// write. That is a defensible claim about two of the four fields and it was
    /// silent about the other two: command `26` carries a filter byte, so a
    /// widening write changed the passband and **nothing told the model.** The
    /// ledger went on reporting the width from before the write until the next
    /// session sweep, up to thirty seconds later. A capture taken in that window
    /// said `500 Hz` beside a three-kilohertz block that Hamlet had already
    /// widened, which is a stale value shown as current — §0.0 broken where it is
    /// hardest to see, because every field on the row looked measured.</para>
    /// <para>`26 00` answers the mode, the data flag and the filter slot in one
    /// transaction, and `1A 03` answers the width. Two reads on a slow bus, once
    /// per tune-in, and the values arrive stamped with the time the radio
    /// answered rather than the time Hamlet asked.</para>
    /// <para>**THE FOLD SURVIVES AS THE FALLBACK AND SAYS SO IN ITS
    /// PROVENANCE.** Where the readback does not answer, the acknowledgement is
    /// still evidence about the mode and the variant — it is the same frame the
    /// radio accepted (HM-OPEN-041, whose write loop this must not reopen) — and
    /// the source names the write rather than a read, so anything displaying it
    /// can tell the two apart. The width is not folded: nothing acknowledged a
    /// number of hertz, and inventing one is the guess this exists to prevent.</para>
    /// </remarks>
    private async Task<IReadOnlyList<RigValue>> ReadBackTheModeAsync(
        CivMode mode, bool dataMode, string label,
        CancellationToken cancellationToken)
    {
        var readBack = await ReadAsync(
            RigField.DataMode, RigState.Empty, cancellationToken)
            .ConfigureAwait(false);

        var values = new List<RigValue>();

        if (readBack.Any(v => v is { Field: RigField.Mode, IsKnown: true }))
        {
            values.AddRange(readBack);
        }
        else
        {
            // The radio took the frame and did not answer the question. Report
            // what the acknowledgement establishes, sourced to it.
            values.Add(RigValue.Known(
                RigField.Mode, (int)mode, CivValues.Name(mode),
                DateTime.UtcNow, label));
            values.Add(RigValue.Known(
                RigField.DataMode, dataMode ? 1 : 0,
                dataMode ? "on" : "off", DateTime.UtcNow, label));
        }

        // The width sits on a scale that depends on the mode, so it is read
        // second and against what the readback just established.
        var context = RigState.Empty.With(values.ToArray());

        values.AddRange(await ReadAsync(
            RigField.FilterBandwidth, context, cancellationToken)
            .ConfigureAwait(false));

        RememberModeAndFilter(values);
        return values;
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

            Interlocked.Increment(ref _sent);

            await _port.WriteAsync(frame.ToWireBytes(), cancellationToken).ConfigureAwait(false);

            var winner = await Task.WhenAny(
                tcs.Task, Task.Delay(ResponseTimeout, cancellationToken)).ConfigureAwait(false);
            if (winner != tcs.Task)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // **COUNTED WHERE IT HAPPENS** (HM-DEC-092). Five settings were
                // written one evening, all five reported as unanswered, and at
                // least two had actually taken effect. Nothing on any screen
                // said the link was dropping commands, so the operator was told
                // things about his radio that were not true and had no way to
                // see why.
                Interlocked.Increment(ref _unanswered);
                _lastUnansweredCommand = command;
                _lastUnansweredUtc = DateTime.UtcNow;

                throw new TimeoutException(
                    $"No CI-V response to 0x{command:X2} within {ResponseTimeout.TotalMilliseconds} ms.");
            }

            Interlocked.Increment(ref _answered);

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

    private long _sent;
    private long _answered;
    private long _unanswered;
    private byte? _lastUnansweredCommand;
    private DateTime? _lastUnansweredUtc;

    // **COUNTED WHERE THE FRAME ARRIVES, NOT WHERE IT IS USED.** Every test in
    // HandleFrame below can discard a frame, and a count taken after any of them
    // cannot tell "the radio said nothing" from "Hamlet threw it away". Four
    // counters and two clocks, all interlocked, allocating nothing: this runs on
    // the read loop and §8's never-throw discipline binds hardest here.
    private long _inbound;
    private long _inboundFromRadio;
    private long _inboundBroadcast;
    private long _inboundTransceive;
    private long _inboundScope;
    private long _inboundBytes;
    private long _lastInboundTicks;
    private long _lastBroadcastTicks;
    private long _lastTransceiveTicks;

    /// <summary>
    /// How the conversation with the radio is going (HM-DEC-092).
    /// </summary>
    /// <remarks>
    /// The diagnostics screen read forty values and said nothing about the link
    /// carrying them. On this station radio frequency energy from the operator's
    /// own transmissions knocks USB devices off the bus, and the CI-V link shares
    /// it, so a link that stops answering mid-send is expected rather than
    /// mysterious. Saying so is worth more than any amount of guessing.
    /// </remarks>
    public CivLinkHealth Link => new(
        _port.PortName,
        _port.BaudRate,
        Interlocked.Read(ref _sent),
        Interlocked.Read(ref _answered),
        Interlocked.Read(ref _unanswered),
        _lastUnansweredCommand,
        _lastUnansweredUtc,
        Interlocked.Read(ref _inbound),
        Interlocked.Read(ref _inboundFromRadio),
        Interlocked.Read(ref _inboundBroadcast),
        Interlocked.Read(ref _inboundTransceive),
        Interlocked.Read(ref _inboundScope),
        Interlocked.Read(ref _inboundBytes),
        Moment(Interlocked.Read(ref _lastInboundTicks)),
        Moment(Interlocked.Read(ref _lastBroadcastTicks)),
        Moment(Interlocked.Read(ref _lastTransceiveTicks)));

    private static DateTime? Moment(long ticks)
        => ticks == 0 ? null : new DateTime(ticks, DateTimeKind.Utc);

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

        // **BEFORE EVERY TEST BELOW.** The first of them drops our own echo, the
        // next three claim frames for the transceive and scope paths, and the
        // last one drops anything that does not answer the command in flight. A
        // count taken after any of those answers a different question from the
        // one that has been argued about for two sessions: did the radio say
        // anything, and was any of it the operator's own dial.
        Interlocked.Increment(ref _inbound);
        Interlocked.Exchange(ref _lastInboundTicks, DateTime.UtcNow.Ticks);

        if (frame.From == _radioAddress)
        {
            Interlocked.Increment(ref _inboundFromRadio);
        }

        if (frame.To == CivConstants.BroadcastAddress)
        {
            Interlocked.Increment(ref _inboundBroadcast);
            Interlocked.Exchange(ref _lastBroadcastTicks, DateTime.UtcNow.Ticks);
        }

        if (frame.Command is CivConstants.CmdTransceiveFrequency
            or CivConstants.CmdTransceiveMode)
        {
            Interlocked.Increment(ref _inboundTransceive);

            // **WHEN, AND NOT ONLY HOW MANY** (HM-DEC-091). A running count says
            // whether the radio has ever volunteered anything; it cannot say
            // whether it did so during the half minute in some particular
            // recording, which is the question a capture sidecar has to answer.
            Interlocked.Exchange(ref _lastTransceiveTicks, DateTime.UtcNow.Ticks);
        }

        if (frame.Command == CivConstants.CmdScope)
        {
            Interlocked.Increment(ref _inboundScope);
        }

        // Six bytes of framing and the data area, which is what actually
        // occupies the cable.
        Interlocked.Add(ref _inboundBytes, 6 + frame.Data.Length);

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
                    RigField.Frequency, hz, CivValues.FrequencyText(hz),
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

        // The scope stream, which the radio pushes rather than being asked for.
        // Handled before the pending-request path because these arrive between
        // ordinary command replies and must not be mistaken for one.
        if (frame.Command == CivConstants.CmdScope
            && frame.Data.Length > 1
            && frame.Data[0] == CivConstants.ScopeWaveformSub)
        {
            ScopeData?.Invoke(frame.Data[1..]);
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
