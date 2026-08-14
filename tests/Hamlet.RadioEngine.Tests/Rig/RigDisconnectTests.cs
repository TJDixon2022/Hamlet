using System.Diagnostics;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Disconnecting, which must always work (HM-DEC-051).
/// </summary>
/// <remarks>
/// Found against a real IC-7300 on COM3: connecting worked, the app followed
/// the radio, and Disconnect left the button and the port list dead with the
/// app still believing it was connected. The read loop was parked inside
/// <c>SerialPort.BaseStream.ReadAsync</c>, which on Windows does not observe
/// its cancellation token, so the await on the loop never returned and the line
/// that cleared the connected state sat after it.
/// </remarks>
public sealed class RigDisconnectTests
{
    private const byte Radio = CivConstants.DefaultRadioAddress;
    private const byte Controller = CivConstants.DefaultControllerAddress;

    /// <summary>How long "promptly" is allowed to mean, generously.</summary>
    private static readonly TimeSpan Promptly = TimeSpan.FromSeconds(3);

    private static byte[] FrequencyReply()
        => new CivFrame(
            Controller, Radio, CivConstants.CmdReadFrequency,
            new byte[] { 0x00, 0x30, 0x07, 0x07, 0x00 }).ToWireBytes();

    private static async Task<(Ic7300Rig Rig, FakeSerialPort Port)> ConnectAsync(
        FakeSerialPort port)
    {
        var rig = new Ic7300Rig(port);
        var connect = rig.ConnectAsync();
        port.EnqueueIncoming(FrequencyReply());

        Assert.True(await connect);
        Assert.True(rig.IsConnected);
        return (rig, port);
    }

    /// <remarks>
    /// THE BUG, REPRODUCED AND FIXED. A read that never returns and ignores
    /// cancellation, closed handle or not, is the worst case Windows offers.
    /// Teardown must abandon it rather than wait on it, because a loop that
    /// survives its own port closing will not finish because somebody waited
    /// longer.
    /// </remarks>
    [Fact]
    public async Task ARigWhoseReadLoopIsStuckStillDisconnects()
    {
        var port = new FakeSerialPort();
        var (rig, _) = await ConnectAsync(port);
        using var _guard = rig;

        // Park the read loop the way a real serial port does, then let it get
        // there.
        port.ReadNeverReturns = true;
        port.EnqueueIncoming(FrequencyReply());

        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (!port.IsReadParked && DateTime.UtcNow < deadline)
        {
            await Task.Delay(5);
        }

        Assert.True(port.IsReadParked, "the read loop never reached the stuck read");

        var clock = Stopwatch.StartNew();
        await rig.DisconnectAsync();
        clock.Stop();

        Assert.False(rig.IsConnected);
        Assert.True(
            clock.Elapsed < Promptly,
            $"disconnect took {clock.Elapsed.TotalSeconds:0.0} s, which is not promptly");
    }

    /// <remarks>
    /// Proves the ordinary path is still quick and clean: a port that lets its
    /// read fault when the handle closes ends the loop at once, which is what
    /// happens on most systems and what closing first is for.
    /// </remarks>
    [Fact]
    public async Task AnOrdinaryDisconnectIsImmediate()
    {
        var port = new FakeSerialPort();
        var (rig, _) = await ConnectAsync(port);
        using var _guard = rig;

        var clock = Stopwatch.StartNew();
        await rig.DisconnectAsync();
        clock.Stop();

        Assert.False(rig.IsConnected);
        Assert.False(port.IsOpen);
        Assert.True(
            clock.Elapsed < TimeSpan.FromSeconds(1),
            $"a clean disconnect took {clock.Elapsed.TotalMilliseconds:0} ms");
    }

    /// <remarks>
    /// THE ORDER THAT MATTERS. The port is closed before the loop is waited on,
    /// because it is the handle going away that frees a parked read and not the
    /// token. Getting this backwards is what caused the bug.
    /// </remarks>
    [Fact]
    public async Task ThePortIsClosedBeforeTheLoopIsWaitedFor()
    {
        var port = new FakeSerialPort { ReadNeverReturns = true };
        var rig = new Ic7300Rig(port);
        var connect = rig.ConnectAsync();
        port.EnqueueIncoming(FrequencyReply());
        await connect;

        using var _guard = rig;

        await rig.DisconnectAsync();

        Assert.False(port.IsOpen);
    }

    /// <remarks>
    /// Proves disconnecting twice, or disconnecting something never connected,
    /// is quiet rather than an error. The operator clicking a button twice is
    /// not a fault condition.
    /// </remarks>
    [Fact]
    public async Task DisconnectingTwiceIsHarmless()
    {
        var port = new FakeSerialPort();
        var (rig, _) = await ConnectAsync(port);
        using var _guard = rig;

        await rig.DisconnectAsync();
        await rig.DisconnectAsync();

        Assert.False(rig.IsConnected);

        var never = new Ic7300Rig(new FakeSerialPort());
        await never.DisconnectAsync();
        Assert.False(never.IsConnected);
    }

    /// <remarks>
    /// Proves disposing a rig with a stuck read loop does not wedge shutdown
    /// either. Closing the window has to work as reliably as clicking
    /// Disconnect.
    /// </remarks>
    [Fact]
    public async Task DisposingARigWithAStuckLoopReturnsPromptly()
    {
        var port = new FakeSerialPort { ReadNeverReturns = true };
        var rig = new Ic7300Rig(port);
        var connect = rig.ConnectAsync();
        port.EnqueueIncoming(FrequencyReply());
        await connect;

        var clock = Stopwatch.StartNew();
        rig.Dispose();
        clock.Stop();

        Assert.True(
            clock.Elapsed < Promptly,
            $"dispose took {clock.Elapsed.TotalSeconds:0.0} s");
    }

    /// <remarks>
    /// Proves the state monitor cannot hold a disconnect up either. It polls
    /// through the same rig, so a stuck port could otherwise park its loop and
    /// block the shutdown that was meant to free everything.
    /// </remarks>
    [Fact]
    public async Task TheStateMonitorDoesNotHoldUpADisconnect()
    {
        var port = new FakeSerialPort();
        var (rig, _) = await ConnectAsync(port);
        using var _guard = rig;

        using var monitor = new RigStateMonitor(rig);
        monitor.Start();

        // Let it get as far as its first read, which nothing will answer.
        await Task.Delay(100);

        port.ReadNeverReturns = true;

        var clock = Stopwatch.StartNew();
        monitor.Dispose();
        await rig.DisconnectAsync();
        clock.Stop();

        Assert.False(rig.IsConnected);
        Assert.True(
            clock.Elapsed < Promptly,
            $"stopping the monitor and disconnecting took {clock.Elapsed.TotalSeconds:0.0} s");
    }
}
