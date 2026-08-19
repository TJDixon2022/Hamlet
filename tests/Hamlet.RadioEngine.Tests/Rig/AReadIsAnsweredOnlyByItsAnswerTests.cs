using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// A read completes on the frame that answers it and on nothing else.
/// </summary>
/// <remarks>
/// <para>**THE SUSPECT THIS ORDER NAMED, TESTED AT THE SEAM.** HM-OPEN-042 found
/// that a request issued with no expected response command completes on `FB` or
/// `FA`, and a tune write's acknowledgement is an `FB`. If a frequency read could
/// be completed that way it would resolve against the wrong frame and the
/// pre-write value would go back into the model, which is the snap-back the
/// operator sees.</para>
/// <para>**It cannot, and these prove it.** `ReadAsync` passes its own command
/// and sub-command as the expected reply, so an acknowledgement does not satisfy
/// it. The two are also serialized by the command gate, so they cannot overlap on
/// the wire in the first place. The defect was real and it was one layer up, in
/// what the app did with a reading taken before the write.</para>
/// <para>These stay because the rule is load-bearing and was never asserted: the
/// day somebody passes null for an expected command on a read, this fails rather
/// than the operator watching his display flick backwards.</para>
/// </remarks>
public sealed class AReadIsAnsweredOnlyByItsAnswerTests
{
    private const byte Radio = CivConstants.DefaultRadioAddress;
    private const byte Controller = CivConstants.DefaultControllerAddress;

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
    /// **THE SEAM.** A frequency read is in flight and a tune write's `FB` arrives
    /// first. The read must not take it, and the value that eventually lands must
    /// be the one the radio actually sent.
    /// </remarks>
    [Fact]
    public async Task AnAcknowledgementDoesNotCompleteAFrequencyRead()
    {
        var (rig, port) = await ConnectAsync();
        using var _ = rig;

        var read = rig.ReadAsync(RigField.Frequency, RigState.Empty);

        // The acknowledgement a tune write would produce, arriving while the read
        // is outstanding.
        port.EnqueueIncoming(FromRadio(CivConstants.ResultOk));

        await Task.Delay(50);

        Assert.False(
            read.IsCompleted,
            "an acknowledgement is not an answer to a read, and taking it as one "
            + "would put the pre-write frequency back into the model");

        // 14.040, which is where the operator had just tuned.
        port.EnqueueIncoming(FromRadio(
            CivConstants.CmdReadFrequency, 0x00, 0x00, 0x04, 0x14, 0x00));

        var values = await read;
        var value = Assert.Single(values);

        Assert.True(value.IsKnown);
        Assert.Equal(14_040_000, value.Number);
    }

    /// <remarks>
    /// Proves a refusal cannot complete a read either, which is the same fault
    /// with the other acknowledgement byte.
    /// </remarks>
    [Fact]
    public async Task ARefusalDoesNotCompleteAFrequencyRead()
    {
        var (rig, port) = await ConnectAsync();
        using var _ = rig;

        var read = rig.ReadAsync(RigField.Frequency, RigState.Empty);

        port.EnqueueIncoming(FromRadio(CivConstants.ResultNg));
        await Task.Delay(50);

        Assert.False(read.IsCompleted);

        port.EnqueueIncoming(FromRadio(
            CivConstants.CmdReadFrequency, 0x00, 0x00, 0x04, 0x14, 0x00));

        Assert.Equal(14_040_000, Assert.Single(await read).Number);
    }

    /// <remarks>
    /// Proves the general rule rather than the frequency's case: a reply to a
    /// different command does not satisfy the request in flight. Command 16 alone
    /// covers the AGC, the preamp and the noise blanker, so this is the fault that
    /// would fill one field with another field's value.
    /// </remarks>
    [Fact]
    public async Task AReplyToAnotherCommandDoesNotCompleteThisOne()
    {
        var (rig, port) = await ConnectAsync();
        using var _ = rig;

        var read = rig.ReadAsync(RigField.SMeter, RigState.Empty);

        // The mode, which nobody asked for here.
        port.EnqueueIncoming(FromRadio(0x04, 0x03, 0x02));
        await Task.Delay(50);

        Assert.False(read.IsCompleted);

        port.EnqueueIncoming(FromRadio(0x15, 0x02, 0x01, 0x20));

        Assert.Contains(await read, v => v.Field == RigField.SMeter && v.IsKnown);
    }

    /// <remarks>
    /// Proves the writes are the ones that take an acknowledgement, which is
    /// correct and is why the rule above has to be stated: the same transport
    /// carries both, and only one of them is answered by `FB`.
    /// </remarks>
    [Fact]
    public async Task AWriteIsTheThingThatTakesAnAcknowledgement()
    {
        var (rig, port) = await ConnectAsync();
        using var _ = rig;

        var write = rig.SetFrequencyHzAsync(14_040_000);

        port.EnqueueIncoming(FromRadio(CivConstants.ResultOk));

        await write;

        var sent = new CivFrame(
            Radio, Controller, CivConstants.CmdSetFrequency,
            Bcd.EncodeFrequencyHz(14_040_000)).ToWireBytes();

        Assert.Contains(
            string.Join(" ", sent.Select(b => b.ToString("X2"))),
            string.Join(" ", port.Written.Select(b => b.ToString("X2"))),
            StringComparison.Ordinal);
    }
}
