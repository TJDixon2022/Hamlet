using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Reading the radio's state over CI-V, against scripted byte sequences
/// (HM-DEC-007, HM-DEC-050). No test needs a radio.
/// </summary>
public sealed class RigReadTests
{
    private const byte Radio = CivConstants.DefaultRadioAddress;
    private const byte Controller = CivConstants.DefaultControllerAddress;

    /// <summary>A frame from the radio to the controller.</summary>
    private static byte[] FromRadio(byte command, params byte[] data)
        => new CivFrame(Controller, Radio, command, data).ToWireBytes();

    /// <summary>A connected rig with its handshake already answered.</summary>
    /// <remarks>
    /// The request is started before the answer is scripted, throughout this
    /// file. Enqueuing first races the read loop against the request
    /// registering, and the loop wins often enough to make a test flaky rather
    /// than wrong.
    /// </remarks>
    private static async Task<(Ic7300Rig Rig, FakeSerialPort Port)> ConnectAsync()
    {
        var port = new FakeSerialPort();
        var rig = new Ic7300Rig(port);

        // ConnectAsync probes with a frequency read.
        var connect = rig.ConnectAsync();
        port.EnqueueIncoming(FromRadio(
            CivConstants.CmdReadFrequency, 0x00, 0x30, 0x07, 0x07, 0x00));

        Assert.True(await connect);
        return (rig, port);
    }

    /// <summary>Issue a read and script its answer.</summary>
    private static async Task<IReadOnlyList<RigValue>> ReadAsync(
        Ic7300Rig rig, FakeSerialPort port, RigField field, RigState context,
        params byte[][] answers)
    {
        var read = rig.ReadAsync(field, context);

        foreach (var answer in answers)
        {
            port.EnqueueIncoming(answer);
        }

        return await read;
    }

    /// <remarks>
    /// THE BADGE THAT LIED. The mode was hardcoded to "CW" since the LCD was
    /// built, so the screen said CW whatever the radio was set to. One command
    /// answers both the mode and the filter designator (p. 19-9), which is why
    /// asking for one returns both rather than spending a second transaction on
    /// a slow bus.
    /// </remarks>
    [Theory]
    [InlineData(0x00, 0x01, "LSB", "FIL1")]
    [InlineData(0x01, 0x02, "USB", "FIL2")]
    [InlineData(0x03, 0x03, "CW", "FIL3")]
    [InlineData(0x05, 0x01, "FM", "FIL1")]
    public async Task ReadingTheModeAlsoAnswersTheFilter(
        byte modeByte, byte filterByte, string mode, string filter)
    {
        var (rig, port) = await ConnectAsync();
        using var _ = rig;

        var values = await ReadAsync(
            rig, port, RigField.Mode, RigState.Empty,
            FromRadio(0x04, modeByte, filterByte));

        Assert.Equal(2, values.Count);
        Assert.Equal(mode, values.Single(v => v.Field == RigField.Mode).Text);
        Assert.Equal(filter, values.Single(v => v.Field == RigField.FilterSelection).Text);
        Assert.All(values, v => Assert.True(v.IsKnown));
    }

    /// <remarks>
    /// THE READ THAT WOULD HAVE SAVED THE EVENING. The filter was wide open and
    /// nobody could see it. The index means nothing without the mode, because
    /// the scale it sits on depends on it, so the read takes the state it
    /// already has as context.
    /// </remarks>
    [Fact]
    public async Task TheFilterWidthComesBackInHertz()
    {
        var (rig, port) = await ConnectAsync();
        using var _ = rig;

        var mode = await ReadAsync(
            rig, port, RigField.Mode, RigState.Empty, FromRadio(0x04, 0x03, 0x01));

        var state = RigState.Empty.With(mode);

        // Index 0x28 is 28, which on the non-AM scale is 600 + 18 * 100.
        var width = await ReadAsync(
            rig, port, RigField.FilterBandwidth, state, FromRadio(0x1A, 0x03, 0x28));

        var value = Assert.Single(width);
        Assert.True(value.IsKnown);
        Assert.Equal(2400, value.Number);
        Assert.Equal("2.4 kHz", value.Text);
    }

    /// <remarks>
    /// Proves the width is refused rather than guessed when the mode is not
    /// known. Reading an AM index on the sideband scale would report 2.4 kHz as
    /// 600 Hz, and this is the number an operator would act on (§0.0).
    /// </remarks>
    [Fact]
    public async Task TheFilterWidthIsRefusedWhenTheModeIsUnknown()
    {
        var (rig, port) = await ConnectAsync();
        using var _ = rig;

        var width = await ReadAsync(
            rig, port, RigField.FilterBandwidth, RigState.Empty,
            FromRadio(0x1A, 0x03, 0x28));

        var value = Assert.Single(width);
        Assert.Equal(RigValueState.Unknown, value.State);
        Assert.Null(value.Number);
        Assert.Contains("mode", value.Source, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves the S-meter parses against the manual's own anchors (p. 19-3), and
    /// that the value carries the command it came from so a wrong reading
    /// arrives with its provenance (§0.0.1).
    /// </remarks>
    [Theory]
    [InlineData(0x00, 0x00, "S0")]
    [InlineData(0x00, 0x67, "S5")]
    [InlineData(0x01, 0x20, "S9")]
    [InlineData(0x02, 0x41, "S9+60")]
    public async Task TheSMeterParsesAgainstTheManualsAnchors(
        byte high, byte low, string expected)
    {
        var (rig, port) = await ConnectAsync();
        using var _ = rig;

        var value = Assert.Single(await ReadAsync(
            rig, port, RigField.SMeter, RigState.Empty,
            FromRadio(0x15, 0x02, high, low)));

        Assert.True(value.IsKnown);
        Assert.Equal(expected, value.Text);
        Assert.Equal("CI-V 15 02", value.Source);
        Assert.NotNull(value.AtUtc);
    }

    /// <remarks>
    /// MATCHING ON THE COMMAND BYTE ALONE IS NOT ENOUGH. The AGC, the preamp and
    /// the noise blanker are all command 16 and differ only in their
    /// sub-command. Without checking it, a reply about the preamp would satisfy
    /// a request about the AGC and the model would fill one field with another
    /// field's value.
    /// </remarks>
    [Fact]
    public async Task AReplyToADifferentSubCommandDoesNotAnswerThisRequest()
    {
        var (rig, port) = await ConnectAsync();
        using var _ = rig;

        // The preamp answering while the AGC was asked for. It must be ignored,
        // and then the real answer must still be matched.
        var value = Assert.Single(await ReadAsync(
            rig, port, RigField.Agc, RigState.Empty,
            FromRadio(0x16, 0x02, 0x01),
            FromRadio(0x16, 0x12, 0x03)));

        Assert.True(value.IsKnown);
        Assert.Equal("SLOW", value.Text);
    }

    /// <remarks>
    /// Proves each on/off and choice read lands on the words the manual uses.
    /// </remarks>
    [Theory]
    [InlineData(RigField.Preamp, (byte)0x16, (byte)0x02, (byte)0x02, "preamp 2")]
    [InlineData(RigField.NoiseBlanker, (byte)0x16, (byte)0x22, (byte)0x01, "on")]
    [InlineData(RigField.NoiseReduction, (byte)0x16, (byte)0x40, (byte)0x00, "off")]
    [InlineData(RigField.AutoNotch, (byte)0x16, (byte)0x41, (byte)0x01, "on")]
    [InlineData(RigField.ManualNotch, (byte)0x16, (byte)0x48, (byte)0x00, "off")]
    [InlineData(RigField.BreakIn, (byte)0x16, (byte)0x47, (byte)0x02, "full")]
    [InlineData(RigField.Split, (byte)0x0F, (byte)0xFF, (byte)0x01, "on")]
    [InlineData(RigField.Attenuator, (byte)0x11, (byte)0xFF, (byte)0x20, "20 dB")]
    [InlineData(RigField.SquelchStatus, (byte)0x15, (byte)0x05, (byte)0x01, "open")]
    [InlineData(RigField.TransmitStatus, (byte)0x1C, (byte)0x00, (byte)0x00, "receiving")]
    public async Task EachSettingParsesToTheManualsOwnWords(
        RigField field, byte command, byte subCommand, byte value, string expected)
    {
        var (rig, port) = await ConnectAsync();
        using var _ = rig;

        var data = subCommand == 0xFF
            ? new[] { value }
            : new[] { subCommand, value };

        var read = Assert.Single(await ReadAsync(
            rig, port, field, RigState.Empty, FromRadio(command, data)));

        Assert.True(read.IsKnown);
        Assert.Equal(expected, read.Text);
    }

    /// <remarks>
    /// Proves the level scales land on the figures the manual states: the keyer
    /// runs 6 to 48 words a minute and the CW pitch 300 to 900 Hz, both across
    /// the same 0 to 255 (p. 19-3). The pitch is the one the decoder is told to
    /// start listening at, so a few hertz of arithmetic error would matter.
    /// </remarks>
    [Theory]
    [InlineData(RigField.KeyerSpeed, (byte)0x0C, (byte)0x00, (byte)0x00, 6)]
    [InlineData(RigField.KeyerSpeed, (byte)0x0C, (byte)0x02, (byte)0x55, 48)]
    [InlineData(RigField.CwPitch, (byte)0x09, (byte)0x00, (byte)0x00, 300)]
    [InlineData(RigField.CwPitch, (byte)0x09, (byte)0x01, (byte)0x28, 600)]
    [InlineData(RigField.CwPitch, (byte)0x09, (byte)0x02, (byte)0x55, 900)]
    public async Task TheLevelScalesLandOnTheManualsFigures(
        RigField field, byte subCommand, byte high, byte low, int expected)
    {
        var (rig, port) = await ConnectAsync();
        using var _ = rig;

        var value = Assert.Single(await ReadAsync(
            rig, port, field, RigState.Empty,
            FromRadio(0x14, subCommand, high, low)));

        Assert.True(value.IsKnown);
        Assert.Equal(expected, value.Number);
    }

    /// <remarks>
    /// A TIMED-OUT READ MARKS THE VALUE UNKNOWN AND STOPS. It does not throw,
    /// because a radio that stopped answering is a condition rather than an
    /// error, and it does not retry, because a slow bus that is already
    /// struggling is the last thing to send more commands to (HM-DEC-050).
    /// </remarks>
    [Fact]
    public async Task AReadThatTimesOutMarksTheValueUnknownWithoutThrowing()
    {
        var (rig, port) = await ConnectAsync();
        using var _ = rig;

        // Nothing enqueued: the radio simply does not answer.
        var before = port.Written.Length;
        var value = Assert.Single(await rig.ReadAsync(RigField.Agc, RigState.Empty));

        Assert.Equal(RigValueState.Unknown, value.State);
        Assert.Null(value.Number);
        Assert.Null(value.AtUtc);
        Assert.Contains("timeout", value.Source, StringComparison.OrdinalIgnoreCase);

        // One command went out, not a stream of them.
        var sent = port.Written.Length - before;
        Assert.Equal(new CivFrame(Radio, Controller, 0x16, new byte[] { 0x12 })
            .ToWireBytes().Length, sent);
    }

    /// <remarks>
    /// Proves a garbled reply produces unknown rather than a nearest legal
    /// value. A mode badge confidently showing the wrong mode is the prime
    /// directive broken on the app's most-read surface (§0.0).
    /// </remarks>
    [Fact]
    public async Task AnUndocumentedReplyProducesUnknownRatherThanAGuess()
    {
        var (rig, port) = await ConnectAsync();
        using var _ = rig;

        // 06 is absent from the manual's mode table.
        var values = await ReadAsync(
            rig, port, RigField.Mode, RigState.Empty, FromRadio(0x04, 0x06, 0x01));

        Assert.All(values, v => Assert.Equal(RigValueState.Unknown, v.State));
    }

    /// <remarks>
    /// Proves a field the manual documents no read for says so, rather than
    /// having a command byte invented for it (§4). Nothing goes on the wire.
    /// </remarks>
    [Fact]
    public async Task AnUndocumentedFieldSendsNothingAndSaysSo()
    {
        var (rig, port) = await ConnectAsync();
        using var _ = rig;

        var before = port.Written.Length;
        var value = Assert.Single(await rig.ReadAsync(RigField.Vfo, RigState.Empty));

        Assert.Equal(RigValueState.Undocumented, value.State);
        Assert.Equal(before, port.Written.Length);
    }

    /// <remarks>
    /// PREFER WHAT THE RADIO VOLUNTEERS OVER ASKING FOR IT. The IC-7300
    /// broadcasts a mode change as the operator makes it, which is instant,
    /// costs no bus traffic and cannot be stale. Nothing is sent to receive it.
    /// </remarks>
    [Fact]
    public async Task AModeChangeOnTheRadioArrivesWithoutBeingAskedFor()
    {
        var (rig, port) = await ConnectAsync();
        using var _ = rig;

        var reported = new TaskCompletionSource<IReadOnlyList<RigValue>>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        rig.ValuesReported += (_, e) =>
        {
            if (e.Values.Any(v => v.Field == RigField.Mode))
            {
                reported.TrySetResult(e.Values);
            }
        };

        var before = port.Written.Length;

        // The operator turning the mode knob: broadcast, from the radio, to the
        // controller address.
        port.EnqueueIncoming(new CivFrame(
            Controller, Radio, CivConstants.CmdTransceiveMode,
            new byte[] { 0x01, 0x02 }).ToWireBytes());

        var values = await reported.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal("USB", values.Single(v => v.Field == RigField.Mode).Text);
        Assert.Equal("FIL2", values.Single(v => v.Field == RigField.FilterSelection).Text);
        Assert.Equal("transceive 01", values.First(v => v.Field == RigField.Mode).Source);
        Assert.Equal(before, port.Written.Length);
    }

    /// <remarks>
    /// DEGRADES HONESTLY (HM-DEC-030). The training radio has no receiver, so
    /// its AGC is unsupported rather than unknown: nothing is coming, and the UI
    /// can stop waiting rather than showing a value that will never arrive. The
    /// two fields it does answer are the two it genuinely models.
    /// </remarks>
    [Fact]
    public async Task TheTrainingRadioReportsUnsupportedRatherThanInventingValues()
    {
        var rig = new TrainingRig(7_030_000);

        var agc = Assert.Single(await rig.ReadAsync(RigField.Agc, RigState.Empty));
        var meter = Assert.Single(await rig.ReadAsync(RigField.SMeter, RigState.Empty));
        var mode = Assert.Single(await rig.ReadAsync(RigField.Mode, RigState.Empty));
        var hz = Assert.Single(await rig.ReadAsync(RigField.Frequency, RigState.Empty));

        Assert.Equal(RigValueState.Unsupported, agc.State);
        Assert.Equal(RigValueState.Unsupported, meter.State);
        Assert.Null(meter.Number);

        Assert.True(mode.IsKnown);
        Assert.Equal("CW", mode.Text);
        Assert.True(hz.IsKnown);
        Assert.Equal(7_030_000, hz.Number);
    }
}
