using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Training;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Every stage of the scope path is countable (HM-DEC-093).
/// </summary>
/// <remarks>
/// <para>**THE WATERFALL WAS REPORTED WORKING THREE TIMES AND HAD NEVER DRAWN A
/// PIXEL FROM A RADIO.** None of those claims was checkable, because between the
/// wire and the drawing there were four stages with no numbers on them. A parser
/// that quietly returns on a part it cannot read is a parser that can be wrong
/// for months.</para>
/// <para>These prove the measurement, which is what this session set out to
/// build. They cannot prove the waterfall works: only a radio can do that.</para>
/// </remarks>
public sealed class ScopeStageCountTests
{
    private const byte Radio = CivConstants.DefaultRadioAddress;
    private const byte Controller = CivConstants.DefaultControllerAddress;

    /// <summary>A first part: wave information, and no waveform data.</summary>
    private static byte[] Information(int total = 11)
    {
        var bytes = new List<byte> { 0x01, (byte)total, 0x00 };

        bytes.AddRange(Bcd.EncodeFrequencyHz(7_100_000));
        bytes.AddRange(Bcd.EncodeFrequencyHz(200_000));
        bytes.Add(0x00);

        return bytes.ToArray();
    }

    /// <summary>A continuation part: minimal header, then amplitudes.</summary>
    private static byte[] Waveform(int sequence, int total = 11)
    {
        var bytes = new List<byte> { (byte)sequence, (byte)total };

        for (var i = 0; i < 48; i++)
        {
            bytes.Add((byte)(i % CivScope.MaximumAmplitude));
        }

        return bytes.ToArray();
    }

    private static async Task<(Ic7300Rig Rig, FakeSerialPort Port)> ConnectAsync()
    {
        var port = new FakeSerialPort();
        var rig = new Ic7300Rig(port);

        var connect = rig.ConnectAsync();

        port.EnqueueIncoming(new CivFrame(
            Controller, Radio, CivConstants.CmdReadFrequency,
            Bcd.EncodeFrequencyHz(7_030_000)).ToWireBytes());

        await connect.ConfigureAwait(false);

        return (rig, port);
    }

    private static void Deliver(FakeSerialPort port, byte[] payload)
    {
        var data = new byte[payload.Length + 1];
        data[0] = CivConstants.ScopeWaveformSub;
        payload.CopyTo(data, 1);

        port.EnqueueIncoming(
            new CivFrame(Controller, Radio, CivConstants.CmdScope, data).ToWireBytes());
    }

    /// <summary>Wait for the read loop to catch up, without a fixed sleep.</summary>
    private static async Task WaitFor(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);

        while (!condition() && DateTime.UtcNow < deadline)
        {
            await Task.Delay(5).ConfigureAwait(false);
        }
    }

    /// <remarks>
    /// <para>Proves HM-DEC-093: **the four stages move independently**, so the
    /// gap between any two of them is a diagnosis. A whole sweep arriving,
    /// parsing and being delivered leaves all four nonzero and rejected at
    /// nought.</para>
    /// </remarks>
    [Fact]
    public async Task AWholeSweepMovesEveryCounter()
    {
        var (rig, port) = await ConnectAsync();

        using (rig)
        {
            using var source = new RigSpectrumSource(rig);
            var delivered = 0;
            source.FrameReady += (in SpectrumFrame _) => delivered++;
            source.Start();

            Deliver(port, Information());

            for (var part = 2; part <= 11; part++)
            {
                Deliver(port, Waveform(part));
            }

            await WaitFor(() => source.SweepsDelivered > 0); 

            Assert.Equal(11, source.PartsReceived);
            Assert.Equal(11, source.PartsParsed);
            Assert.Equal(0, source.PartsRejected);
            Assert.Equal("", source.FirstRejection);
            Assert.Equal(1, source.SweepsDelivered);
            Assert.Equal(1, delivered);
            Assert.NotNull(source.LastPartUtc);
        }
    }

    /// <remarks>
    /// <para>Proves HM-DEC-093, and it is the whole point. **A part the parser
    /// cannot read is counted and its reason kept**, where it used to be a bare
    /// `return`. If the real eleven-part shape on the wire differs from the
    /// constructed frames the parser was built against, every sweep vanishes at
    /// that line, and until now nothing anywhere would have said so.</para>
    /// <para>The first reason is kept rather than the last, because the first is
    /// the one that happened before anything else went wrong.</para>
    /// </remarks>
    [Fact]
    public async Task APartThatCannotBeReadIsCountedAndExplained()
    {
        var (rig, port) = await ConnectAsync();

        using (rig)
        {
            using var source = new RigSpectrumSource(rig);
            source.Start();

            // Too short to carry even the order and maximum.
            Deliver(port, new byte[] { 0x01 });
            Deliver(port, new byte[] { 0x02 });

            await WaitFor(() => source.PartsRejected >= 2); 

            Assert.Equal(2, source.PartsReceived);
            Assert.Equal(0, source.PartsParsed);
            Assert.Equal(2, source.PartsRejected);
            Assert.Equal(0, source.SweepsDelivered);

            Assert.NotEqual("", source.FirstRejection);
            Assert.Contains("unreadable", source.FirstRejection, StringComparison.Ordinal);

            // The first one, not the most recent: "1 bytes" is the first part.
            Assert.Contains("1 bytes", source.FirstRejection, StringComparison.Ordinal);
        }
    }

    /// <remarks>
    /// <para>Proves HM-DEC-093: **a sweep that never completes is visible as
    /// parts arriving with nothing delivered**, which is a completely different
    /// fault from nothing arriving at all and used to look identical.</para>
    /// <para>This is the documented shape the parser has never met: a first part
    /// carrying wave information and no waveform, and continuation parts that
    /// simply stop coming.</para>
    /// </remarks>
    [Fact]
    public async Task ASweepThatStopsHalfwayShowsAsPartsWithoutDelivery()
    {
        var (rig, port) = await ConnectAsync();

        using (rig)
        {
            using var source = new RigSpectrumSource(rig);
            source.Start();

            Deliver(port, Information());
            Deliver(port, Waveform(2));
            Deliver(port, Waveform(3));

            await WaitFor(() => source.PartsParsed >= 3); 

            Assert.Equal(3, source.PartsReceived);
            Assert.Equal(3, source.PartsParsed);
            Assert.Equal(0, source.PartsRejected);
            Assert.Equal(0, source.SweepsDelivered);
        }
    }

    /// <remarks>
    /// Proves HM-DEC-093: nothing at all arriving leaves every counter at nought
    /// and the last-part time unset, which is the state the display must render
    /// as words rather than as innocent black (HM-DEC-092).
    /// </remarks>
    [Fact]
    public async Task NothingArrivingLeavesEveryCounterAtNought()
    {
        var (rig, _) = await ConnectAsync();

        using (rig)
        {
            using var source = new RigSpectrumSource(rig);
            source.Start();

            Assert.Equal(0, source.PartsReceived);
            Assert.Equal(0, source.PartsParsed);
            Assert.Equal(0, source.PartsRejected);
            Assert.Equal(0, source.SweepsDelivered);
            Assert.Null(source.LastPartUtc);
            Assert.Equal("", source.FirstRejection);
        }
    }

    /// <remarks>
    /// Proves HM-DEC-093 and closes HM-OPEN-013: the CI-V USB port is read with
    /// the citation Tim supplied, and it is read-only, so Hamlet can check the
    /// scope's precondition and can never change it (FACT-002).
    /// </remarks>
    [Fact]
    public void TheCivUsbPortIsReadAndNeverWritten()
    {
        var read = CivReads.CivUsbPort;

        Assert.Equal(0x1A, read.Command);
        Assert.Equal(new byte[] { 0x05, 0x00, 0x74 }, read.SubCommand);
        Assert.Equal("19-5", read.Page);
        Assert.Contains(CivReads.All, r => r.Field == RigField.CivUsbPort);

        // Read only, per the manual's own row.
        Assert.DoesNotContain(CivWrites.All, w => w.Field == RigField.CivUsbPort);
    }
}
