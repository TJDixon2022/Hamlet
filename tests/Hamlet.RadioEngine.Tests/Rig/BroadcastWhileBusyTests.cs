using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// The operator's own dial reaches the model while Hamlet is busy talking to the
/// radio.
/// </summary>
/// <remarks>
/// <para>**THE STATE THE APPLICATION WAS ACTUALLY IN.** Six sessions of
/// 2026-08-18, app 1.9.0: forty-six observations of the frequency, thirty-two
/// read, twelve unknown, two stale, and **not one carrying broadcast
/// provenance** — with ages up to sixty seconds. The frequency on screen was
/// being maintained by the session poll alone, which HM-DEC-109 added as a
/// backstop for a broadcast missed at startup. A backstop carrying the whole
/// load looks exactly like this.</para>
/// <para>The existing broadcast test enqueues its frame into an idle rig, which
/// is the easy half. **The radio does not wait for Hamlet to be idle.** The dial
/// turns while a poll is in flight, and that is the case nothing covered.</para>
/// </remarks>
public sealed class BroadcastWhileBusyTests
{
    private const byte Radio = CivConstants.DefaultRadioAddress;
    private const byte Controller = CivConstants.DefaultControllerAddress;

    private static byte[] FromRadio(byte command, params byte[] data)
        => new CivFrame(Controller, Radio, command, data).ToWireBytes();

    /// <summary>
    /// A frame the radio sends nobody in particular, to destination `00`.
    /// </summary>
    private static byte[] Broadcast(byte command, params byte[] data)
        => new CivFrame(CivConstants.BroadcastAddress, Radio, command, data)
            .ToWireBytes();

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
    /// Proves the regression: the dial turns while a command is in flight, and
    /// the reading has to arrive with broadcast provenance and an age near zero.
    /// An unsolicited frame is not a response to anything, so nothing about a
    /// command being outstanding may consume it.
    /// </remarks>
    [Fact]
    public async Task ABroadcastArrivingDuringACommandStillReachesTheModel()
    {
        var (rig, port) = await ConnectAsync();
        using var _ = rig;
        using var monitor = new RigStateMonitor(rig, (_, _) => Task.CompletedTask);

        // Hamlet asks the radio something. Nothing answers it yet, so the
        // command is in flight for the whole of what follows.
        var inFlight = rig.ReadAsync(RigField.SMeter, monitor.State);

        // The operator turns the dial to 14.074 while that is outstanding.
        port.EnqueueIncoming(Broadcast(
            CivConstants.CmdTransceiveFrequency, 0x00, 0x40, 0x07, 0x14, 0x00));

        await WaitForKnownFrequencyAsync(monitor);

        var value = monitor.State[RigField.Frequency];

        Assert.Equal(RigValueState.Known, value.State);
        Assert.Equal(14_074_000, value.Number);
        Assert.Equal("transceive 00", value.Source);

        Assert.NotNull(value.AtUtc);
        Assert.True(
            DateTime.UtcNow - value.AtUtc!.Value < TimeSpan.FromSeconds(5),
            "the reading has to be as fresh as the dial, not as fresh as the poll");

        // And the command it arrived during still gets its own answer, so
        // nothing was stolen in either direction.
        port.EnqueueIncoming(FromRadio(0x15, 0x02, 0x01, 0x20));
        var meter = await inFlight;

        Assert.Contains(meter, v => v.Field == RigField.SMeter && v.IsKnown);
    }

    /// <remarks>
    /// The same in the other order, which is the commoner one: the dial turns
    /// first and the answer Hamlet was waiting for arrives behind it. A
    /// broadcast must not satisfy the pending request either, or the poll would
    /// take a frequency push as its own reply and the two would be confused.
    /// </remarks>
    [Fact]
    public async Task ABroadcastDoesNotAnswerTheCommandInFlight()
    {
        var (rig, port) = await ConnectAsync();
        using var _ = rig;
        using var monitor = new RigStateMonitor(rig, (_, _) => Task.CompletedTask);

        var inFlight = rig.ReadAsync(RigField.Frequency, monitor.State);

        // A frequency broadcast, which carries the same payload shape as the
        // answer to the read that is outstanding.
        port.EnqueueIncoming(Broadcast(
            CivConstants.CmdTransceiveFrequency, 0x00, 0x40, 0x07, 0x14, 0x00));

        await WaitForKnownFrequencyAsync(monitor);

        Assert.Equal("transceive 00", monitor.State[RigField.Frequency].Source);
        Assert.False(inFlight.IsCompleted, "a broadcast is not a reply");

        port.EnqueueIncoming(FromRadio(
            CivConstants.CmdReadFrequency, 0x00, 0x40, 0x07, 0x14, 0x00));

        var values = await inFlight;
        Assert.Contains(values, v => v.Field == RigField.Frequency && v.IsKnown);
    }

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
    }
}
