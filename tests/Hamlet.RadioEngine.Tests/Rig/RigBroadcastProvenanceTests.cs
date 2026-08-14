using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Broadcast is a provenance, not an absence (HM-DEC-053).
/// </summary>
/// <remarks>
/// THE FAILURE THESE EXIST TO STOP. The diagnostics screen said the frequency
/// was "not on this radio" while the IC-7300's own face was showing it, because
/// the sweep walked every field, found no poll command for the frequency, and
/// mapped that absence onto the state reserved for what the radio genuinely
/// lacks. A screen built to prove what the app knows (§0.0.1) was asserting the
/// opposite of what the app knew, which is HM-DEC-009 broken on the one surface
/// that is supposed to be immune to it.
/// </remarks>
public sealed class RigBroadcastProvenanceTests
{
    private const byte Radio = CivConstants.DefaultRadioAddress;
    private const byte Controller = CivConstants.DefaultControllerAddress;

    private static byte[] FromRadio(byte command, params byte[] data)
        => new CivFrame(Controller, Radio, command, data).ToWireBytes();

    /// <summary>A frame the radio sends nobody in particular.</summary>
    private static byte[] Broadcast(byte command, params byte[] data)
        => new CivFrame(CivConstants.BroadcastAddress, Radio, command, data).ToWireBytes();

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
    /// THE BUG, END TO END. A broadcast arrives, the model holds the frequency,
    /// and then the sweep that runs whenever somebody opens the diagnostics
    /// screen walks past it. The value has to survive that walk with its
    /// broadcast provenance intact, because the screen's whole worth is that a
    /// wrong reading arrives with the mechanism that produced it (§0.0.1).
    /// </remarks>
    [Fact]
    public async Task ABroadcastFrequencySurvivesTheSweepWithItsProvenance()
    {
        var (rig, port) = await ConnectAsync();
        using var _ = rig;
        using var monitor = new RigStateMonitor(rig, (_, _) => Task.CompletedTask);

        // The operator turns the dial: 14.074 MHz, the FT8 watering hole.
        port.EnqueueIncoming(Broadcast(
            CivConstants.CmdTransceiveFrequency, 0x00, 0x40, 0x07, 0x14, 0x00));

        await WaitForKnownFrequencyAsync(monitor);

        var before = monitor.State[RigField.Frequency];
        Assert.Equal(RigValueState.Known, before.State);
        Assert.Equal(14_074_000, before.Number);
        Assert.Equal("transceive 00", before.Source);

        // Now the sweep the diagnostics screen runs. It used to overwrite the
        // reading above with "not on this radio".
        var sweep = rig.ReadAsync(RigField.Frequency, monitor.State);
        port.EnqueueIncoming(FromRadio(
            CivConstants.CmdReadFrequency, 0x00, 0x40, 0x07, 0x14, 0x00));

        var values = await sweep;
        var after = Assert.Single(values);

        Assert.Equal(RigValueState.Known, after.State);
        Assert.Equal(14_074_000, after.Number);
        Assert.Contains("14.074", after.Text, StringComparison.Ordinal);

        // Named as the mechanism that actually produced it, never as a command
        // Hamlet does not issue.
        Assert.Equal("CI-V 03", after.Source);
    }

    /// <remarks>
    /// UNKNOWN BEFORE THE FIRST BROADCAST, NEVER UNSUPPORTED AND NEVER ZERO.
    /// Transceive can be switched off at the radio, in which case nothing is
    /// ever pushed. That is a value nobody has heard yet, which is exactly what
    /// unknown means, and 0 Hz would be a plausible number on the one field
    /// every other surface in the app trusts.
    /// </remarks>
    [Fact]
    public void BeforeAnythingArrivesTheFrequencyIsUnknownRatherThanUnsupported()
    {
        var value = RigState.Empty[RigField.Frequency];

        Assert.Equal(RigValueState.Unknown, value.State);
        Assert.Null(value.Number);
        Assert.Null(value.AtUtc);
    }

    /// <remarks>
    /// The filter designator is the second field this bug hit, and it hit it the
    /// same way: one command answers the mode and the filter together (p. 19-9),
    /// so the filter has no read of its own, so the sweep concluded the radio
    /// had no filter moments after reporting which one was selected.
    /// </remarks>
    [Fact]
    public async Task AskingForACompanionFieldLeavesWhatAnsweredItAlone()
    {
        var (rig, port) = await ConnectAsync();
        using var _ = rig;

        var mode = rig.ReadAsync(RigField.Mode, RigState.Empty);
        port.EnqueueIncoming(FromRadio(0x04, 0x03, 0x02));

        var state = RigState.Empty.With(await mode);
        Assert.Equal("FIL2", state[RigField.FilterSelection].Text);

        // No command is issued and nothing is reported, so the reading above
        // stands rather than being replaced by an absence.
        var values = await rig.ReadAsync(RigField.FilterSelection, state);

        Assert.Empty(values);
        Assert.Equal("FIL2", state.With(values)[RigField.FilterSelection].Text);
    }

    /// <remarks>
    /// THE CLASSIFICATION RULE, STATED ONCE. No field the rig display is capable
    /// of showing may come back "not on this radio" from a radio that has it.
    /// This sweeps the whole enumeration rather than naming the two that were
    /// broken, so a field added next month cannot quietly reintroduce it.
    /// </remarks>
    [Fact]
    public async Task NoFieldTheRadioActuallyHasIsEverCalledNotOnThisRadio()
    {
        var (rig, port) = await ConnectAsync();
        using var _ = rig;

        foreach (var field in Enum.GetValues<RigField>())
        {
            var read = rig.ReadAsync(field, RigState.Empty);

            // Anything with a command of its own gets a reply that does not
            // decode, which is an unknown reading and not an absent field.
            if (CivReads.For(field) is { } command)
            {
                port.EnqueueIncoming(FromRadio(command.Command, command.SubCommand));
            }

            foreach (var value in await read)
            {
                Assert.NotEqual(RigValueState.Unsupported, value.State);
            }
        }
    }

    /// <remarks>
    /// Proves the two mechanisms are named apart. "CI-V 03" and "transceive 00"
    /// both produce a frequency and mean different things about how current it
    /// is, and a screen that called them the same thing would be hiding the
    /// difference the operator needs.
    /// </remarks>
    [Fact]
    public void EveryBroadcastNamesItsMechanismAndItsPage()
    {
        Assert.NotEmpty(CivReads.Broadcasts);

        foreach (var broadcast in CivReads.Broadcasts)
        {
            Assert.StartsWith("transceive", broadcast.Label, StringComparison.Ordinal);
            Assert.False(string.IsNullOrWhiteSpace(broadcast.Page));
            Assert.False(string.IsNullOrWhiteSpace(broadcast.Note));
        }

        Assert.NotNull(CivReads.BroadcastFor(RigField.Frequency));
        Assert.Equal("CI-V 03", CivReads.For(RigField.Frequency)!.Label);
    }

    /// <summary>Wait for the monitor to hear the broadcast, without a sleep.</summary>
    private static async Task WaitForKnownFrequencyAsync(RigStateMonitor monitor)
    {
        for (var i = 0; i < 200; i++)
        {
            if (monitor.State[RigField.Frequency].IsKnown)
            {
                return;
            }

            await Task.Delay(10);
        }

        Assert.Fail("the broadcast never reached the model");
    }
}
