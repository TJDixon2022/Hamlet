using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Mode follows the map, and the first write this app makes (HM-DEC-056).
/// </summary>
/// <remarks>
/// A pure decision, so every case is testable without a radio, the way
/// <c>ReconnectPlan.Decide</c> already is. The cases that matter are the ones
/// nobody exercises by hand: the operator's own mode change, a drag across three
/// neighborhoods in one gesture, a write the radio never confirmed.
/// </remarks>
public sealed class ModeFollowTests
{
    private const byte Radio = CivConstants.DefaultRadioAddress;
    private const byte Controller = CivConstants.DefaultControllerAddress;

    private static CwBand Twenty => HfBands.Bands.Single(b => b.Name == "20 m");

    private static Neighborhood At(long hz)
        => NeighborhoodPlan.WithEdges(Twenty).Single(n => n.Contains(hz));

    private static byte[] FromRadio(byte command, params byte[] data)
        => new CivFrame(Controller, Radio, command, data).ToWireBytes();

    private static async Task<(Ic7300Rig Rig, FakeSerialPort Port)> ConnectAsync()
    {
        var port = new FakeSerialPort();
        var rig = new Ic7300Rig(port);

        var connect = rig.ConnectAsync();
        port.EnqueueIncoming(FromRadio(
            CivConstants.CmdReadFrequency, 0x00, 0x30, 0x07, 0x07, 0x00));

        Assert.True(await connect);
        return (rig, port);
    }

    /// <remarks>
    /// USB AND USB-D ARE DIFFERENT FACTS TO THIS RADIO, and getting that wrong
    /// is the difference between hearing FT8 and hearing nothing useful. It is
    /// also why the write is command 26 rather than 06: 06 sets a mode and a
    /// filter and has no way at all to say whether the data variant is wanted
    /// (p. 19-8 against p. 19-11).
    /// </remarks>
    [Fact]
    public void TheDigitalBlocksAskForTheDataVariantAndTheMorseOnesDoNot()
    {
        var ft8 = ModeFollowPlan.TargetFor(At(14_075_000));
        var morse = ModeFollowPlan.TargetFor(At(14_030_000));

        Assert.NotNull(ft8);
        Assert.Equal(CivMode.Usb, ft8!.Mode);
        Assert.True(ft8.DataMode);
        Assert.Equal("USB-D", ft8.Name);

        Assert.NotNull(morse);
        Assert.Equal(CivMode.Cw, morse!.Mode);
        Assert.False(morse.DataMode);
    }

    /// <remarks>
    /// The sideband convention is cited rather than remembered. IARU Region 2
    /// Band Plan: "For SSB phone operations below 10 MHz use lower sideband
    /// (LSB); above 10 MHz use upper sideband (USB)."
    /// </remarks>
    [Fact]
    public void VoiceTakesTheSidebandTheConventionGivesIt()
    {
        var forty = HfBands.Bands.Single(b => b.Name == "40 m");

        var low = ModeFollowPlan.TargetFor(
            NeighborhoodPlan.WithEdges(forty).Single(n => n.Contains(7_200_000)));
        var high = ModeFollowPlan.TargetFor(At(14_250_000));

        Assert.Equal(CivMode.Lsb, low?.Mode);
        Assert.Equal(CivMode.Usb, high?.Mode);
        Assert.False(low?.DataMode);
        Assert.False(high?.DataMode);
    }

    /// <remarks>
    /// Proves a block with nothing to say says nothing. Open ground and the
    /// beacon block do not tell the operator what they would be doing there, so
    /// the automation leaves the radio alone rather than picking something.
    /// </remarks>
    [Fact]
    public void AStretchWithNoConventionAsksForNoMode()
    {
        var open = NeighborhoodPlan.WithEdges(Twenty)
            .First(n => n.Family == ModeFamily.Open);

        Assert.Null(ModeFollowPlan.TargetFor(open));
        Assert.Null(ModeFollowPlan.TargetFor(null));
    }

    /// <remarks>
    /// WITH THE SETTING OFF NO WRITE IS EVER ISSUED. It is the first thing this
    /// app does to somebody's radio without being asked, so anybody who would
    /// rather drive themselves has to be obeyed.
    /// </remarks>
    [Fact]
    public void WithTheSettingOffNothingIsWritten()
    {
        var decision = ModeFollowPlan.Decide(
            ModeFollowState.Armed(enabled: false),
            CivMode.Cw, false, ModeFollowPlan.TargetFor(At(14_075_000)));

        Assert.False(decision.Write);
        Assert.Empty(decision.Narration);
    }

    /// <remarks>
    /// THE OPERATOR'S OWN HAND ALWAYS WINS, and a band change re-arms it.
    /// Somebody who sets a mode on purpose has said something, and an app that
    /// changed it back two seconds later would be arguing with them about their
    /// own radio. A band change is a fresh start rather than a continuation.
    /// </remarks>
    [Fact]
    public void AManualChangeSuspendsAndABandChangeRearms()
    {
        var target = ModeFollowPlan.TargetFor(At(14_075_000));
        var armed = ModeFollowState.Armed(enabled: true);

        Assert.True(ModeFollowPlan.Decide(armed, CivMode.Cw, false, target).Write);

        var suspended = armed.SuspendedByOperator();
        Assert.False(ModeFollowPlan.Decide(suspended, CivMode.Cw, false, target).Write);

        var rearmed = suspended.Rearmed();
        Assert.True(ModeFollowPlan.Decide(rearmed, CivMode.Cw, false, target).Write);
    }

    /// <remarks>
    /// Proves the radio is left alone when it is already right. That is what
    /// keeps a drag across three neighborhoods from becoming three commands on a
    /// slow bus, together with the settle delay the ViewModel applies before
    /// asking at all.
    /// </remarks>
    [Fact]
    public void ARadioAlreadyInTheRightModeIsNotWrittenTo()
    {
        var target = ModeFollowPlan.TargetFor(At(14_075_000));
        var armed = ModeFollowState.Armed(enabled: true);

        Assert.False(ModeFollowPlan.Decide(armed, CivMode.Usb, true, target).Write);

        // The data flag alone is enough to make it wrong. USB is not USB-D.
        Assert.True(ModeFollowPlan.Decide(armed, CivMode.Usb, false, target).Write);
    }

    /// <remarks>
    /// Proves the status line says what changed and why, in the app's voice
    /// rather than as a command echo. A radio that changes itself silently is
    /// the "is it broken" confusion relocated rather than removed.
    /// </remarks>
    [Fact]
    public void ItSaysWhatChangedAndWhy()
    {
        var decision = ModeFollowPlan.Decide(
            ModeFollowState.Armed(enabled: true),
            CivMode.Cw, false, ModeFollowPlan.TargetFor(At(14_075_000)));

        Assert.Contains("USB-D", decision.Narration, StringComparison.Ordinal);
        Assert.Contains("digital modes gather", decision.Narration, StringComparison.Ordinal);

        // A sentence rather than a command echo.
        Assert.DoesNotContain("26", decision.Narration, StringComparison.Ordinal);
        Assert.DoesNotContain("CI-V", decision.Narration, StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>THE WRITE ON THE WIRE. Command 26, the selected VFO, the mode, the
    /// data flag, and no filter byte where the caller asked for none, so the
    /// radio picks the filter it would have picked for that mode itself
    /// (p. 19-11).</para>
    /// <para>**AND THEN HAMLET ASKS WHAT IT ACTUALLY DID** (work instruction 042,
    /// task 1). This used to assert a single frame, and a single frame was the
    /// defect: the acknowledgement was folded into the model as though it were a
    /// reading, and it says nothing at all about the filter that same frame had
    /// just changed. Two reads follow every confirmed write, and the assertion
    /// is now on the order rather than on the count.</para>
    /// </remarks>
    [Fact]
    public async Task TheWriteIsCommand26AndTheRadioIsAskedWhatItDid()
    {
        var (rig, port) = await ConnectAsync();
        using var _ = rig;

        var sent = new List<CivFrame>();
        rig.FrameTrace += (outgoing, frame) =>
        {
            if (outgoing)
            {
                sent.Add(frame);
            }
        };

        var write = rig.SetModeAsync(CivMode.Usb, dataMode: true);
        port.EnqueueIncoming(FromRadio(CivConstants.ResultOk));

        Assert.True((await write).Worked);

        Assert.Equal(0x26, sent[0].Command);
        Assert.Equal(new byte[] { 0x00, 0x01, 0x01 }, sent[0].Data);

        // The mode, the data flag and the filter slot together, then the width.
        Assert.Equal(0x26, sent[1].Command);
        Assert.Equal(new byte[] { 0x00 }, sent[1].Data);

        Assert.Equal(0x1A, sent[2].Command);
        Assert.Equal(new byte[] { 0x03 }, sent[2].Data);
    }

    /// <remarks>
    /// A CONFIRMED WRITE UPDATES THE MODEL, sourced to the write that made it
    /// true rather than waiting for something to poll it back. The badge on the
    /// rig display reads the same model, so it moves with the radio.
    /// </remarks>
    [Fact]
    public async Task AConfirmedWriteBecomesWhatTheAppBelieves()
    {
        var (rig, port) = await ConnectAsync();
        using var _ = rig;

        var reported = new List<RigValue>();
        rig.ValuesReported += (_, e) => reported.AddRange(e.Values);

        var write = rig.SetModeAsync(CivMode.Cw, dataMode: false);
        port.EnqueueIncoming(FromRadio(CivConstants.ResultOk));

        Assert.Equal(RigWriteOutcome.Confirmed, (await write).Outcome);

        var mode = Assert.Single(reported, v => v.Field == RigField.Mode);
        Assert.Equal(RigValueState.Known, mode.State);
        Assert.Equal("CW", mode.Text);
        Assert.Equal("CI-V 26", mode.Source);
    }

    /// <remarks>
    /// A FAILED WRITE LEAVES THE MODE UNKNOWN RATHER THAN ASSUMED. A mode Hamlet
    /// believes it set and did not is a guess presented as a decode, and it
    /// would put the badge and the radio's own face out of step with nothing on
    /// screen saying so (§0.0).
    /// </remarks>
    [Fact]
    public async Task ARefusedWriteLeavesTheModeUnknown()
    {
        var (rig, port) = await ConnectAsync();
        using var _ = rig;

        var reported = new List<RigValue>();
        rig.ValuesReported += (_, e) => reported.AddRange(e.Values);

        var write = rig.SetModeAsync(CivMode.Usb, dataMode: true);
        port.EnqueueIncoming(FromRadio(CivConstants.ResultNg));

        var result = await write;

        Assert.Equal(RigWriteOutcome.Refused, result.Outcome);
        Assert.False(result.Worked);
        Assert.NotEmpty(result.Detail);

        var mode = Assert.Single(reported, v => v.Field == RigField.Mode);
        Assert.Equal(RigValueState.Unknown, mode.State);
        Assert.Equal("unknown", mode.Text);
    }

    /// <remarks>
    /// Proves the same holds when the radio simply does not answer, which is
    /// what an unplugged cable looks like from here. Never a throw: this runs
    /// off a timer and §8 says logging and automation that can crash the app are
    /// worse than none.
    /// </remarks>
    [Fact]
    public async Task AWriteNobodyAnsweredLeavesTheModeUnknownAndDoesNotThrow()
    {
        var (rig, _) = await ConnectAsync();
        using var owned = rig;

        var reported = new List<RigValue>();
        rig.ValuesReported += (_, e) => reported.AddRange(e.Values);

        // Nothing enqueued: the radio says nothing at all.
        var result = await rig.SetModeAsync(CivMode.Usb, dataMode: true);

        Assert.Equal(RigWriteOutcome.NoAnswer, result.Outcome);
        Assert.Contains("did not answer", result.Detail, StringComparison.Ordinal);
        Assert.Contains(reported, v => v.Field == RigField.Mode && !v.IsKnown);
    }

    /// <remarks>
    /// Proves the training radio degrades honestly rather than pretending
    /// (HM-DEC-030). It synthesizes Morse and nothing else, so a mode it cannot
    /// be in is not a mode it can be set to, and answering "confirmed" would put
    /// a mode on the badge that nothing behind it is producing.
    /// </remarks>
    [Fact]
    public async Task TheTrainingRadioRefusesAModeItCannotProduce()
    {
        var rig = new TrainingRig();

        Assert.Equal(
            RigWriteOutcome.NotSupported,
            (await rig.SetModeAsync(CivMode.Usb, dataMode: true)).Outcome);

        Assert.True((await rig.SetModeAsync(CivMode.Cw, dataMode: false)).Worked);
    }

    /// <remarks>
    /// NOTHING HERE KEYS A TRANSMITTER (§0.2). The write table holds one entry
    /// and it is a mode selection; the CW message command stays unused until
    /// transmit gets its own conversation.
    /// </remarks>
    [Fact]
    public void NoWriteInTheTableGoesNearTheTransmitter()
    {
        Assert.All(CivWrites.All, w => Assert.NotEqual(
            CivConstants.CmdSendCwMessage, w.Command));

        Assert.All(CivWrites.All, w => Assert.False(string.IsNullOrWhiteSpace(w.Page)));
    }

    /// <remarks>
    /// **THE HALF THAT MADE EVERY TRIGGER WRITE** (HM-OPEN-041). Command `26`
    /// carries the mode and the data byte in one frame, so a radio that
    /// acknowledged it acknowledged both. Folding only the mode left `DataMode`
    /// reading as it had before, so a target of USB with data on could never read
    /// back as satisfied and the plan wrote again on every trigger.
    /// </remarks>
    [Fact]
    public async Task AConfirmedModeWriteFoldsTheDataVariantTooAsync()
    {
        var port = new FakeSerialPort();
        var rig = new Ic7300Rig(port);
        using var _ = rig;

        var connect = rig.ConnectAsync();
        port.EnqueueIncoming(new CivFrame(
            CivConstants.DefaultControllerAddress,
            CivConstants.DefaultRadioAddress,
            CivConstants.CmdReadFrequency,
            new byte[] { 0x00, 0x30, 0x07, 0x07, 0x00 }).ToWireBytes());

        Assert.True(await connect);

        var reported = new List<RigValue>();
        rig.ValuesReported += (_, e) => reported.AddRange(e.Values);

        var write = rig.SetModeAsync(CivMode.Usb, dataMode: true);

        port.EnqueueIncoming(new CivFrame(
            CivConstants.DefaultControllerAddress,
            CivConstants.DefaultRadioAddress,
            CivConstants.ResultOk, Array.Empty<byte>()).ToWireBytes());

        Assert.True((await write).Worked);

        var mode = Assert.Single(reported, v => v.Field == RigField.Mode);
        var data = Assert.Single(reported, v => v.Field == RigField.DataMode);

        Assert.Equal((int)CivMode.Usb, mode.Number);
        Assert.True(data.IsKnown);
        Assert.Equal(1, data.Number);
        Assert.Equal(mode.Source, data.Source);
    }
}
