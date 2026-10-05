using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Scan;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;
using static Hamlet.RadioEngine.Tests.Scan.TheCatchScanTests;

namespace Hamlet.RadioEngine.Tests.Scan;

/// <summary>
/// **WHILE A SCAN RUNS, NOTHING IN HAMLET CAN TRANSMIT** (work instruction 540, task 3, HM-DEC-244): the engine's half of
/// the proof, a whole scan on the fake radio of <see cref="TheCatchScanTests"/>. The application's half presses every
/// transmit control in the window (<c>AScanListensOnlyTests</c>).
/// </summary>
public sealed class AScanNeverTransmitsTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "hamlet-scan-tests", Guid.NewGuid().ToString("N"));

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the refusals are printed.</param>
    public AScanNeverTransmitsTests(ITestOutputHelper output) => _output = output;

    /// <inheritdoc/>
    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (IOException)
        {
        }
    }

    /// <remarks>
    /// Test 8, task 3: **listen only, proven.** A whole scan runs with every keying path built on the scan's lock - the
    /// radio's keyer and the push-to-talk sequence every audio mode keys through - and both are asked to send at two
    /// points in it. Nothing is keyed: no keyer command reaches the rig, no frame reaches the port, and no audio reaches the
    /// sound card. After the scan, the same keyer keys, so it was the lock and nothing else that held it.
    /// </remarks>
    [Fact]
    public async Task NothingTransmitsWhileItRuns()
    {
        using var world = await ScanWorld.Ready(keepsSending: true);
        var listenOnly = world.ListenOnly;
        var keyer = new CwTransmitter(new KeyerCwSender(world.Rig, listenOnly));
        var port = new RecordingPort();
        var sink = new CountingSink();
        var ptt = new Ft8TransmitSequence(port, sink, null, null, CivConstants.DefaultRadioAddress, CivConstants.DefaultControllerAddress, listenOnly);
        var refusals = new List<string>();

        void Try()
        {
            var cw = keyer.SendAsync("CQ DE W1AW", Context(world)).GetAwaiter().GetResult();
            var audio = ptt.RunAsync(OperatorSend.Now(
                new UnslottedTransmission(UnslottedMode.Psk31, new float[8000], 8000, 3), 7_070_000, LicenseClass.Extra, true))
                .GetAwaiter().GetResult();

            refusals.Add($"keyer: {cw.Detail}; push-to-talk: {audio.Outcome}");

            Assert.False(cw.Sent);
            Assert.Equal(Ft8TransmitOutcome.RefusedWhileScanning, audio.Outcome);
        }

        world.At(10, Try);
        world.At(100, Try);

        var (_, summary) = await world.Run(_folder, Short with { Length = TimeSpan.FromMinutes(2) });

        refusals.ForEach(_output.WriteLine);

        Assert.Equal(2, refusals.Count);
        Assert.Equal(0, world.Rig.KeyingAttempts);
        Assert.Empty(port.Written);
        Assert.Equal(0, sink.Calls);
        Assert.False(listenOnly.IsHeld);
        Assert.Equal(CwScanEnd.LengthReached, summary.Ended);

        // After the scan the lock is let go, and the same keyer keys: the lock was what held it.
        var after = await keyer.SendAsync("CQ", Context(world));

        Assert.True(world.Rig.KeyingAttempts > 0, after.Detail);
    }

    /// <remarks>Task 3: the one door's own check refuses while the lock is held, with the scan's reason and its own token.</remarks>
    [Fact]
    public void TheCwDoorRefusesWhileTheLockIsHeld()
    {
        var listenOnly = new ListenOnlyLock();
        var door = new CwTransmitter(new KeyerCwSender(new ScanFakeRig(Home, () => DateTime.UtcNow), listenOnly), listenOnly: listenOnly);
        var context = new TransmitContext(LicenseClass.Extra, 7_030_000, true, true, null, new Hamlet.RadioEngine.Rig.RigState(new Dictionary<Hamlet.RadioEngine.Rig.RigField, Hamlet.RadioEngine.Rig.RigValue>(), "test"));

        using (listenOnly.Hold())
        {
            var held = door.Check(context);

            Assert.False(held.Sent);
            Assert.Equal(CwReadyState.ScanRunning, held.Readiness?.State);
            Assert.Equal("scan_running", held.Readiness?.Reason);
        }

        Assert.NotEqual(CwReadyState.ScanRunning, door.Check(context).Readiness?.State);
    }

    private static TransmitContext Context(ScanWorld world) => new(
        LicenseClass.Extra, 7_030_000, true, true, world.Rig.Capabilities, world.Monitor.State);
}
