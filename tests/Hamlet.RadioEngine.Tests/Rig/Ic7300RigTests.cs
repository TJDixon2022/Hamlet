using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Rig;

public sealed class Ic7300RigTests
{
    private static byte[] Frame(byte to, byte from, byte cmd, params byte[] data)
        => new CivFrame(to, from, cmd, data).ToWireBytes();

    /// <remarks>Proves: connect probes with read-frequency and succeeds when
    /// the radio answers — radio, address and baud agreement in one check.</remarks>
    [Fact]
    public async Task Connect_ProbeAnswered_Succeeds()
    {
        var port = new FakeSerialPort();
        using var rig = new Ic7300Rig(port);

        var connect = rig.ConnectAsync();
        // Radio answers the probe: 7.030.000 back to the controller.
        port.EnqueueIncoming(Frame(0xE0, 0x94, 0x03, 0x00, 0x00, 0x03, 0x07, 0x00));

        Assert.True(await connect);
        Assert.True(rig.IsConnected);
    }

    /// <remarks>Proves: a port that cannot open yields false, not a throw —
    /// unreachable is a condition, per the IRig contract.</remarks>
    [Fact]
    public async Task Connect_PortMissing_ReturnsFalse()
    {
        var port = new FakeSerialPort { FailOnOpen = true };
        using var rig = new Ic7300Rig(port);

        Assert.False(await rig.ConnectAsync());
        Assert.False(rig.IsConnected);
    }

    /// <remarks>Proves: a silent radio (wrong baud, wrong address, cable out)
    /// fails the connect probe within the timeout instead of hanging.</remarks>
    [Fact]
    public async Task Connect_NoAnswer_ReturnsFalse()
    {
        var port = new FakeSerialPort();
        using var rig = new Ic7300Rig(port);

        Assert.False(await rig.ConnectAsync());
        Assert.False(rig.IsConnected);
    }

    /// <remarks>Proves: set-frequency puts exactly the documented bytes on
    /// the wire — FE FE 94 E0 05 [BCD] FD for 14.074.000 — and completes on
    /// the radio's OK. The wire bytes are the contract HM-OPEN-002's manual
    /// check will verify against.</remarks>
    [Fact]
    public async Task SetFrequency_WiresDocumentedBytes_CompletesOnOk()
    {
        var port = new FakeSerialPort();
        using var rig = new Ic7300Rig(port);

        var connect = rig.ConnectAsync();
        port.EnqueueIncoming(Frame(0xE0, 0x94, 0x03, 0x00, 0x00, 0x03, 0x07, 0x00));
        await connect;

        var set = rig.SetFrequencyHzAsync(14_074_000);
        port.EnqueueIncoming(Frame(0xE0, 0x94, 0xFB));
        await set;

        var expected = new byte[]
        {
            0xFE, 0xFE, 0x94, 0xE0, 0x05, 0x00, 0x40, 0x07, 0x14, 0x00, 0xFD,
        };
        Assert.EndsWith(
            Convert.ToHexString(expected),
            Convert.ToHexString(port.Written),
            StringComparison.Ordinal);
    }

    /// <remarks>Proves: the radio's unsolicited transceive report (the
    /// operator's knob) raises FrequencyChanged with the decoded value —
    /// the event the shell's display rides on.</remarks>
    [Fact]
    public async Task KnobTurn_TransceiveFrame_RaisesFrequencyChanged()
    {
        var port = new FakeSerialPort();
        using var rig = new Ic7300Rig(port);

        var connect = rig.ConnectAsync();
        port.EnqueueIncoming(Frame(0xE0, 0x94, 0x03, 0x00, 0x00, 0x03, 0x07, 0x00));
        await connect;

        var tcs = new TaskCompletionSource<long>(TaskCreationOptions.RunContinuationsAsynchronously);
        rig.FrequencyChanged += (_, e) => tcs.TrySetResult(e.FrequencyHz);

        // Transceive broadcast: to=0x00, from=radio, cmd=0x00, BCD 7.040.000.
        port.EnqueueIncoming(Frame(0x00, 0x94, 0x00, 0x00, 0x00, 0x04, 0x07, 0x00));

        var reported = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.Equal(7_040_000, reported);
    }

    /// <remarks>Proves: our own echoed transmission (from == controller) is
    /// ignored rather than mistaken for a response.</remarks>
    [Fact]
    public async Task EchoedOwnFrame_IsIgnored()
    {
        var port = new FakeSerialPort();
        using var rig = new Ic7300Rig(port);

        var connect = rig.ConnectAsync();
        // Echo of our own probe arrives first, then the real answer.
        port.EnqueueIncoming(Frame(0x94, 0xE0, 0x03));
        port.EnqueueIncoming(Frame(0xE0, 0x94, 0x03, 0x00, 0x00, 0x03, 0x07, 0x00));

        Assert.True(await connect);
        Assert.Equal(7_030_000, await ReadAnswer(rig, port));
    }

    private static async Task<long> ReadAnswer(Ic7300Rig rig, FakeSerialPort port)
    {
        var get = rig.GetFrequencyHzAsync();
        port.EnqueueIncoming(Frame(0xE0, 0x94, 0x03, 0x00, 0x00, 0x03, 0x07, 0x00));
        return await get;
    }
}
