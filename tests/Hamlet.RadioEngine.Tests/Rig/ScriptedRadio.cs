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
        => _incoming.Writer.TryWrite(
            new CivFrame(0xE0, 0x94, command, data).ToWireBytes());
}
