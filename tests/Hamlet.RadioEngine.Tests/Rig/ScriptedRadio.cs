using System.Threading.Channels;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Transport;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// A radio on the other end of the wire that holds its own state and answers
/// for itself.
/// </summary>
/// <remarks>
/// <para>**<see cref="FakeSerialPort"/> ANSWERS WHAT A TEST ENQUEUES, WHICH IS
/// THE WRONG INSTRUMENT FOR THIS QUESTION** (work instruction 042, task 1). The
/// acceptance is that ten round trips between two modes leave the radio in the
/// right state, and a port replaying a script cannot fail that: the script is
/// the answer. What is needed is a radio that **remembers what it was last told
/// and reports that**, so a write Hamlet never sent shows up as the mode not
/// changing.</para>
/// <para>It speaks the commands this path uses, from §4's table: `03` for the
/// frequency, `04` for the mode and filter, `26` in both its read and write
/// forms, and `1A 03` for the passband. Anything else is ignored rather than
/// answered, so a command that arrives unexpectedly times out and is visible.
/// </para>
/// <para>**AND THE MODE KNOB TURNS.** <see cref="OperatorTurnsTheModeKnob"/> is
/// the operator's own hand, which is the state this whole task is about: this
/// radio does not broadcast the change, because the operator's own does not
/// (HM-DEC-138 measured inbound transceive at zero across sixty-one seconds).
/// </para>
/// </remarks>
internal sealed class ScriptedRadio : ISerialPort
{
    private readonly Channel<byte[]> _incoming = Channel.CreateUnbounded<byte[]>();
    private readonly List<byte> _pending = new();
    private readonly object _gate = new();

    /// <inheritdoc/>
    public string PortName => "COM-RADIO";

    /// <inheritdoc/>
    public int BaudRate => 115_200;

    /// <inheritdoc/>
    public bool IsOpen { get; private set; }

    /// <summary>The dial, in hertz.</summary>
    public long FrequencyHz { get; set; } = 14_074_000;

    /// <summary>The mode the radio is in.</summary>
    public CivMode Mode { get; private set; } = CivMode.Cw;

    /// <summary>Whether the data variant is on.</summary>
    public bool DataMode { get; private set; }

    /// <summary>The filter slot, 1 to 3.</summary>
    public byte FilterSlot { get; private set; } = 2;

    /// <summary>How many mode writes the radio has accepted.</summary>
    public int ModeWrites { get; private set; }

    /// <summary>
    /// The receive-side switches, by their `16` sub-command.
    /// </summary>
    /// <remarks>
    /// **KEYED BY THE BYTE RATHER THAN BY A NAME**, so a test that sets the
    /// noise blanker and a Hamlet that writes `16 22` have to agree about which
    /// control that is. Sub-commands from §4: `12` AGC, `22` noise blanker,
    /// `40` noise reduction, `41` auto notch.
    /// </remarks>
    public Dictionary<byte, byte> Switches { get; } = new()
    {
        [0x12] = 3,
        [0x22] = 0,
        [0x40] = 0,
        [0x41] = 0,
    };

    /// <summary>Every `16` write the radio has taken, in order.</summary>
    public List<(byte Sub, byte Value)> SwitchWrites { get; } = new();

    /// <summary>How many `16` reads the radio has answered.</summary>
    public int SwitchReads { get; private set; }

    /// <summary>Sub-commands the radio will not answer, for the unread case.</summary>
    public HashSet<byte> Deaf { get; } = new();

    /// <summary>
    /// Whether the bus echoes the controller's own frames back, as "Link to
    /// [REMOTE]" does by default (§4, p. 12-8).
    /// </summary>
    /// <remarks>
    /// **THIS IS ONE OF THE TWO MECHANISMS A STALE READ WAS BLAMED ON.** A reader
    /// that answers with the next frame off the wire would take its own outgoing
    /// command as the reply.
    /// </remarks>
    public bool EchoOwnFramesBack { get; set; }

    /// <summary>
    /// Whether an unsolicited transceive frame arrives just before each reply.
    /// </summary>
    /// <remarks>
    /// **THE OTHER MECHANISM.** Transceive broadcasts whenever the dial moves,
    /// which is exactly when a scan is running.
    /// </remarks>
    public bool VolunteerTransceiveBeforeReplying { get; set; }

    /// <summary>Whether the radio says nothing at all.</summary>
    public bool AnswerNothing { get; set; }

    /// <summary>Set the mode and its data variant together.</summary>
    /// <param name="mode">The mode byte.</param>
    /// <param name="dataMode">One for the data variant, nought for plain.</param>
    /// <remarks>
    /// Command `04` cannot tell USB from USB-D; `26` is the read that carries the
    /// flag (HM-DEC-056), and this is what puts the two in a known state.
    /// </remarks>
    public void SetModeAndData(byte mode, int dataMode)
    {
        lock (_gate)
        {
            Mode = (CivMode)mode;
            DataMode = dataMode != 0;
        }
    }

    /// <summary>The operator reaches over and works a receive-side control.</summary>
    /// <param name="sub">Which one, by its sub-command byte.</param>
    /// <param name="value">Where he leaves it.</param>
    public void OperatorTurnsASwitch(byte sub, byte value)
    {
        lock (_gate)
        {
            Switches[sub] = value;
        }
    }

    /// <summary>
    /// The operator reaches over and turns the mode knob.
    /// </summary>
    /// <param name="mode">What he turns it to.</param>
    /// <param name="dataMode">Whether that leaves the data variant on.</param>
    /// <param name="filterSlot">The filter that mode last used.</param>
    /// <remarks>
    /// **NOTHING IS BROADCAST**, deliberately. Hamlet finds out by asking, which
    /// is what the readback exists for.
    /// </remarks>
    public void OperatorTurnsTheModeKnob(CivMode mode, bool dataMode, byte filterSlot)
    {
        lock (_gate)
        {
            Mode = mode;
            DataMode = dataMode;
            FilterSlot = filterSlot;
        }
    }

    /// <summary>The passband in hertz, as this radio has its filters set.</summary>
    /// <remarks>
    /// FIL1 is the wide one and FIL2 the narrow one the operator had on
    /// 2026-08-28. The widths go out on the wire through
    /// <see cref="CivFilterWidth"/> own scale rather than as figures typed here.
    /// </remarks>
    public int PassbandHz
    {
        get
        {
            lock (_gate)
            {
                return FilterSlot == 1 ? WideHz : NarrowHz;
            }
        }
    }

    /// <summary>What FIL1 is set to on this radio.</summary>
    public const int WideHz = 3000;

    /// <summary>What FIL2 is set to on this radio.</summary>
    public const int NarrowHz = 500;

    /// <inheritdoc/>
    public void Open() => IsOpen = true;

    /// <inheritdoc/>
    public void Close() => IsOpen = false;

    /// <inheritdoc/>
    public async ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken)
    {
        var chunk = await _incoming.Reader.ReadAsync(cancellationToken)
            .ConfigureAwait(false);
        chunk.CopyTo(buffer);
        return chunk.Length;
    }

    /// <inheritdoc/>
    public ValueTask WriteAsync(
        ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
    {
        Write(buffer.Span);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public void Write(ReadOnlySpan<byte> buffer)
    {
        lock (_gate)
        {
            _pending.AddRange(buffer.ToArray());
            DrainWhileHolding();
        }
    }

    /// <inheritdoc/>
    public void Dispose() => Close();

    private void DrainWhileHolding()
    {
        while (true)
        {
            var end = _pending.IndexOf(0xFD);
            if (end < 0)
            {
                return;
            }

            var frame = _pending.GetRange(0, end + 1).ToArray();
            _pending.RemoveRange(0, end + 1);

            // FE FE to from command data... FD
            if (frame.Length < 6 || frame[0] != 0xFE || frame[1] != 0xFE)
            {
                continue;
            }

            Answer(frame[4], frame[5..^1]);
        }
    }

    private void Answer(byte command, byte[] data)
    {
        switch (command)
        {
            case 0x03:
                Reply(0x03, Frequency());
                break;

            case 0x04:
                Reply(0x04, new[] { (byte)(int)Mode, FilterSlot });
                break;

            // Read: 16 <sub>. Write: 16 <sub> <value>.
            case 0x16 when data.Length == 1 && Switches.ContainsKey(data[0]):
                if (Deaf.Contains(data[0]))
                {
                    break;
                }

                SwitchReads++;
                Reply(0x16, new[] { data[0], Switches[data[0]] });
                break;

            case 0x16 when data.Length >= 2 && Switches.ContainsKey(data[0]):
                Switches[data[0]] = data[1];
                SwitchWrites.Add((data[0], data[1]));
                Reply(CivConstants.ResultOk, Array.Empty<byte>());
                break;

            case 0x1A when data.Length >= 1 && data[0] == 0x03:
                Reply(0x1A, new[] { (byte)0x03, WidthIndex() });
                break;

            // The read form: the VFO selector on its own.
            case 0x26 when data.Length == 1:
                Reply(0x26, new[]
                {
                    (byte)0x00, (byte)(int)Mode, (byte)(DataMode ? 1 : 0), FilterSlot,
                });
                break;

            // The write form: VFO, mode, data flag, and optionally a filter.
            case 0x26 when data.Length >= 3:
                if (CivValues.Mode(data[1]) is { } wanted)
                {
                    Mode = wanted;
                    DataMode = data[2] == 1;

                    // **SKIPPING THE FILTER IS NOT LEAVING IT ALONE** (§4,
                    // p. 19-11): the radio selects that mode default, and on
                    // this one that is the narrow slot.
                    FilterSlot = data.Length >= 4 ? data[3] : (byte)2;
                    ModeWrites++;
                    Reply(CivConstants.ResultOk, Array.Empty<byte>());
                }
                else
                {
                    Reply(CivConstants.ResultNg, Array.Empty<byte>());
                }

                break;
        }
    }

    private byte WidthIndex()
    {
        var hertz = FilterSlot == 1 ? WideHz : NarrowHz;

        for (byte index = 0; index <= 0x49; index++)
        {
            if (CivValues.Level(0x00, index) is { } level
                && CivFilterWidth.Hertz(level, Mode) == hertz)
            {
                return index;
            }
        }

        throw new InvalidOperationException(
            $"no filter index on the {Mode} scale gives {hertz} Hz");
    }

    private byte[] Frequency()
    {
        var bytes = new byte[5];
        var left = FrequencyHz;

        for (var i = 0; i < 5; i++)
        {
            var low = (int)(left % 10);
            left /= 10;
            var high = (int)(left % 10);
            left /= 10;
            bytes[i] = (byte)((high << 4) | low);
        }

        return bytes;
    }

    private void Reply(byte command, byte[] data)
    {
        if (AnswerNothing)
        {
            return;
        }

        // **THE INTERFERENCE GOES IN FRONT OF THE ANSWER**, which is where it
        // arrives on the real bus: the echo is the controller's own frame coming
        // straight back, and a transceive report lands whenever the dial moves.
        if (EchoOwnFramesBack)
        {
            _incoming.Writer.TryWrite(
                new CivFrame(0x94, 0xE0, command, data).ToWireBytes());
        }

        if (VolunteerTransceiveBeforeReplying)
        {
            _incoming.Writer.TryWrite(
                new CivFrame(
                    CivConstants.BroadcastAddress, 0x94,
                    CivConstants.CmdTransceiveMode,
                    new byte[] { 0x03, 0x02 }).ToWireBytes());
        }

        _incoming.Writer.TryWrite(
            new CivFrame(0xE0, 0x94, command, data).ToWireBytes());
    }
}
