using System.Globalization;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Tests.Rig;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Transmit;

/// <summary>
/// The sequence keys, plays and unkeys - and the unkey happens whatever goes
/// wrong in between.
/// </summary>
/// <remarks>
/// <para>**THIS IS THE UNIT'S REASON FOR EXISTING** (work instruction 255, task
/// 2). Step 3's second exit criterion is *key, transmit, unkey, with the unkey
/// happening even if the audio path throws*, and every test here is a way for
/// the transmission to end badly with the radio still coming back to receive.
/// </para>
/// <para>**NOTHING HERE OPENS A DEVICE.** The transport is unit 253's
/// <see cref="FakeSerialPort"/>, extended with scripted write failures; the
/// audio end is <see cref="FakeTransmitAudioSink"/>, which makes no sound. No
/// radio has ever been attached to this machine (SHACK_FACTS.md FACT-004), so a
/// device opened here would prove something about this machine and nothing about
/// the radio.</para>
/// <para>**THE WIRE IS QUOTED, NOT DESCRIBED.** Every assertion about the radio
/// coming out of transmit is an assertion about the bytes the transport actually
/// took.</para>
/// </remarks>
public sealed class TheUnkeyHappensWhateverGoesWrongTests
{
    /// <summary>`FE FE 94 E0 1C 00 01 FD` - into transmit.</summary>
    private static readonly byte[] KeyFrame =
        [0xFE, 0xFE, 0x94, 0xE0, 0x1C, 0x00, 0x01, 0xFD];

    /// <summary>`FE FE 94 E0 1C 00 00 FD` - back to receive.</summary>
    private static readonly byte[] UnkeyFrame =
        [0xFE, 0xFE, 0x94, 0xE0, 0x1C, 0x00, 0x00, 0xFD];

    /// <summary>`FE FE 94 E0 17 FF FD` - the abort's first frame, stop the keyer.</summary>
    private static readonly byte[] AbortCwStopFrame =
        [0xFE, 0xFE, 0x94, 0xE0, 0x17, 0xFF, 0xFD];

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fixture.</summary>
    /// <param name="output">xUnit's output sink.</param>
    public TheUnkeyHappensWhateverGoesWrongTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **The ordinary run: gate, key, play, unkey - and it is not an abort.**
    /// </summary>
    /// <remarks>
    /// The wire carries exactly two frames and neither of them is the abort's
    /// `17 FF`. A successful transmission that reported itself as an abort would
    /// make every real abort unfindable in the record.
    /// </remarks>
    [Fact]
    public async Task AGoodRunKeysPlaysAndUnkeysAndSaysItWasNotAnAbort()
    {
        var port = new FakeSerialPort();
        var sink = new FakeTransmitAudioSink();
        var sequence = new Ft8TransmitSequence(port, sink);

        var run = await sequence.RunAsync(Send());

        _output.WriteLine($"outcome         : {run.Outcome}");
        _output.WriteLine($"came out of tx  : {run.CameOutOfTransmit}");
        _output.WriteLine($"wire            : {Hex(port.Written)}");
        _output.WriteLine($"samples offered : {run.SamplesOffered} at {sink.RateAskedFor} Hz");
        _output.WriteLine($"seconds offered : {run.SecondsOffered:0.###}");

        Assert.Equal(Ft8TransmitOutcome.Sent, run.Outcome);
        Assert.True(run.Keyed);
        Assert.True(run.UnkeyedNormally);
        Assert.Null(run.Abort);
        Assert.Equal(UnkeyRoute.OrdinaryUnkey, run.CameOutOfTransmit);
        Assert.True(run.RadioIsInReceive);

        // The whole wire, in order: key then unkey, and nothing else at all.
        Assert.Equal(Hex([.. KeyFrame, .. UnkeyFrame]), Hex(port.Written));
        Assert.DoesNotContain(Hex(AbortCwStopFrame), Hex(port.Written), StringComparison.Ordinal);

        // The sink got the whole transmission at the rate it was composed at.
        Assert.Equal(1, sink.TimesCalled);
        Assert.Equal(run.SamplesOffered, sink.SamplesHandedOver);
    }

    /// <summary>**The sink throws, and the radio still comes out of transmit.**</summary>
    [Fact]
    public async Task TheRadioComesOutOfTransmitWhenTheSinkThrows()
    {
        var port = new FakeSerialPort();
        var sink = new FakeTransmitAudioSink
        {
            Throws = new InvalidOperationException("the render client died"),
        };

        var run = await new Ft8TransmitSequence(port, sink).RunAsync(Send());

        Report("the sink throws", run, port);

        Assert.Equal(Ft8TransmitOutcome.AudioFailed, run.Outcome);
        Assert.True(run.Keyed);
        Assert.False(run.UnkeyedNormally);
        Assert.NotNull(run.Abort);
        Assert.Equal(UnkeyRoute.TheAbort, run.CameOutOfTransmit);
        Assert.True(run.RadioIsInReceive);

        // Keyed once, then both halves of the abort, and no ordinary unkey.
        Assert.Equal(
            Hex([.. KeyFrame, .. AbortCwStopFrame, .. UnkeyFrame]),
            Hex(port.Written));

        // The abort's record is carried out rather than swallowed.
        Assert.True(run.Abort!.CwStop.Written);
        Assert.True(run.Abort.PttOff.Written);
        Assert.Contains("render client died", run.Reason, StringComparison.Ordinal);
    }

    /// <summary>
    /// **The sink returns early, which is a failure and not a small success.**
    /// </summary>
    /// <remarks>
    /// The quiet one. A sink that plays four seconds of a twelve second
    /// transmission and returns without complaint has put an undecodable signal
    /// in somebody else's slot, and the radio must come out of transmit for it.
    /// </remarks>
    [Fact]
    public async Task TheRadioComesOutOfTransmitWhenTheSinkReturnsEarly()
    {
        var port = new FakeSerialPort();
        var sink = new FakeTransmitAudioSink { PlaysOnly = 48_000 };

        var run = await new Ft8TransmitSequence(port, sink).RunAsync(Send());

        Report("the sink returns early", run, port);

        Assert.Equal(Ft8TransmitOutcome.AudioFailed, run.Outcome);
        Assert.Equal(UnkeyRoute.TheAbort, run.CameOutOfTransmit);
        Assert.True(run.RadioIsInReceive);
        Assert.Equal(48_000, run.Played!.Value.SamplesPlayed);
        Assert.Contains("48000 of 151680 samples", run.Reason, StringComparison.Ordinal);
        Assert.Equal(
            Hex([.. KeyFrame, .. AbortCwStopFrame, .. UnkeyFrame]),
            Hex(port.Written));
    }

    /// <summary>**The transmission is cancelled, and the radio comes back.**</summary>
    [Fact]
    public async Task TheRadioComesOutOfTransmitWhenTheSinkIsCancelled()
    {
        var port = new FakeSerialPort();
        using var stopping = new CancellationTokenSource();
        var sink = new FakeTransmitAudioSink
        {
            Throws = new OperationCanceledException(stopping.Token),
        };

        await stopping.CancelAsync();

        var run = await new Ft8TransmitSequence(port, sink).RunAsync(Send(), stopping.Token);

        Report("cancelled mid-transmission", run, port);

        Assert.Equal(Ft8TransmitOutcome.Cancelled, run.Outcome);
        Assert.Equal(UnkeyRoute.TheAbort, run.CameOutOfTransmit);
        Assert.True(run.RadioIsInReceive);

        // THE UNKEY IS NOT CANCELLED WITH THE TRANSMISSION. Whoever cancelled
        // wanted the radio to stop, which is the opposite of wanting the frame
        // that stops it to be skipped.
        Assert.Equal(
            Hex([.. KeyFrame, .. AbortCwStopFrame, .. UnkeyFrame]),
            Hex(port.Written));
    }

    /// <summary>
    /// **The port dies on the way out, and the abort's frames still land.**
    /// </summary>
    /// <remarks>
    /// Write two is the ordinary unkey. Scripting exactly that one to throw is
    /// the case a `finally` alone does not survive: the ordinary route out of
    /// transmit is gone and the radio is still keyed.
    /// </remarks>
    [Fact]
    public async Task TheRadioComesOutOfTransmitWhenThePortThrowsOnTheWayOut()
    {
        var port = new FakeSerialPort();
        port.WritesThatThrow.Add(2);
        var sink = new FakeTransmitAudioSink();

        var run = await new Ft8TransmitSequence(port, sink).RunAsync(Send());

        Report("the port throws on the way out", run, port);

        Assert.Equal(Ft8TransmitOutcome.PortFailed, run.Outcome);
        Assert.True(run.Keyed);
        Assert.False(run.UnkeyedNormally);
        Assert.Equal(UnkeyRoute.TheAbort, run.CameOutOfTransmit);
        Assert.True(run.RadioIsInReceive);

        // Write 2 threw and its bytes never reached the radio; writes 3 and 4,
        // the abort's two halves, did.
        Assert.Equal(4, port.WritesAttempted);
        Assert.Equal(
            Hex([.. KeyFrame, .. AbortCwStopFrame, .. UnkeyFrame]),
            Hex(port.Written));
    }

    /// <summary>
    /// **The keying write itself throws, and the abort fires anyway.**
    /// </summary>
    /// <remarks>
    /// A write that threw is not a write that is known not to have arrived. The
    /// driver may have put the bytes on the wire and failed afterwards, so the
    /// only honest response is to unkey a radio that may never have keyed - two
    /// frames at a port that is probably already dead, against a transmitter
    /// possibly left on.
    /// </remarks>
    [Fact]
    public async Task TheAbortFiresEvenWhenTheKeyingWriteItselfThrew()
    {
        var port = new FakeSerialPort();
        port.WritesThatThrow.Add(1);
        var sink = new FakeTransmitAudioSink();

        var run = await new Ft8TransmitSequence(port, sink).RunAsync(Send());

        Report("the keying write throws", run, port);

        Assert.Equal(Ft8TransmitOutcome.PortFailed, run.Outcome);
        Assert.False(run.Keyed);
        Assert.Equal(UnkeyRoute.TheAbort, run.CameOutOfTransmit);
        Assert.True(run.RadioIsInReceive);

        // The sink was never reached: there is no transmission to play into a
        // radio that would not take the frame that starts one.
        Assert.True(sink.WasNeverTouched);
        Assert.Equal(Hex([.. AbortCwStopFrame, .. UnkeyFrame]), Hex(port.Written));
    }

    /// <summary>
    /// **A port that takes nothing at all is reported as that, and not as an
    /// unkey.**
    /// </summary>
    /// <remarks>
    /// §0.0 pointed at a result rather than a display. Where every write throws,
    /// nothing in software knows whether the radio is transmitting, and a result
    /// that claimed the radio was in receive would be a guess presented as a
    /// measurement. It says <see cref="UnkeyRoute.NothingReachedTheRadio"/>
    /// instead, and <c>RadioIsInReceive</c> is false.
    /// </remarks>
    [Fact]
    public async Task ADeadPortIsSaidToBeDeadRatherThanCalledAnUnkey()
    {
        var port = new FakeSerialPort();
        port.WritesThatThrow.UnionWith([1, 2, 3, 4]);
        var sink = new FakeTransmitAudioSink();

        var run = await new Ft8TransmitSequence(port, sink).RunAsync(Send());

        Report("every write throws", run, port);

        Assert.Equal(Ft8TransmitOutcome.PortFailed, run.Outcome);
        Assert.NotNull(run.Abort);
        Assert.False(run.Abort!.AnythingReachedTheRadio);
        Assert.Equal(UnkeyRoute.NothingReachedTheRadio, run.CameOutOfTransmit);
        Assert.False(run.RadioIsInReceive);
        Assert.Empty(port.Written);

        // The abort still tried both halves and recorded why each failed.
        Assert.False(run.Abort.CwStop.Written);
        Assert.False(run.Abort.PttOff.Written);
        Assert.NotNull(run.Abort.CwStop.Failure);
        Assert.NotNull(run.Abort.PttOff.Failure);
    }

    /// <summary>
    /// **Every failure mode, counted, with what each one left on the wire.**
    /// </summary>
    /// <remarks>
    /// The report's `NUMBER:` comes from this test rather than from adding up the
    /// ones above by hand.
    /// </remarks>
    [Fact]
    public async Task EveryFailureModeLeavesTheRadioUnkeyed()
    {
        var modes = new (string Name, Func<(FakeSerialPort Port, FakeTransmitAudioSink Sink)> Set)[]
        {
            ("the sink throws", () =>
                (new FakeSerialPort(),
                 new FakeTransmitAudioSink { Throws = new IOException("device lost") })),
            ("the sink returns early", () =>
                (new FakeSerialPort(), new FakeTransmitAudioSink { PlaysOnly = 1 })),
            ("the sink is cancelled", () =>
                (new FakeSerialPort(),
                 new FakeTransmitAudioSink { Throws = new OperationCanceledException() })),
            ("the port throws on the way out", () =>
            {
                var port = new FakeSerialPort();
                port.WritesThatThrow.Add(2);
                return (port, new FakeTransmitAudioSink());
            }),
            ("the keying write throws", () =>
            {
                var port = new FakeSerialPort();
                port.WritesThatThrow.Add(1);
                return (port, new FakeTransmitAudioSink());
            }),
            ("the licence gate refuses", () =>
                (new FakeSerialPort(), new FakeTransmitAudioSink())),
        };

        var unkeyed = 0;

        foreach (var (name, set) in modes)
        {
            var (port, sink) = set();
            var refusing = name.Contains("licence", StringComparison.Ordinal);

            var run = await new Ft8TransmitSequence(port, sink).RunAsync(
                refusing
                    ? Send(licenseClass: LicenseClass.Technician)
                    : Send());

            if (run.RadioIsInReceive)
            {
                unkeyed++;
            }

            _output.WriteLine(
                $"{name,-32} {run.Outcome,-20} {run.CameOutOfTransmit,-22} {Hex(port.Written)}");
        }

        _output.WriteLine($"left the radio unkeyed: {unkeyed} of {modes.Length}");

        Assert.Equal(modes.Length, unkeyed);
    }

    /// <summary>
    /// **Nothing in the sequence can start a transmission on its own.**
    /// </summary>
    /// <remarks>
    /// <para>Step 3's sixth exit criterion, checked against the file rather than
    /// asserted in prose - the shape
    /// <c>TheSeamNamesNoRadioNoDeviceAndNoEncoderOfItsOwn</c> already uses for the
    /// composer. A clock read, a wait, a repetition or anything with a schedule in
    /// it is a way for this code to decide the moment has come, and the moment is
    /// the operator's alone (§0.2).</para>
    /// <para>Doc comments are stripped before the search, so the words may be
    /// discussed and may not be used.</para>
    /// </remarks>
    [Fact]
    public void NothingInTheSequenceCanStartATransmissionOnItsOwn()
    {
        var path = Path.Combine(
            RepositoryRoot(), "src", "Hamlet.RadioEngine", "Transmit", "Ft8TransmitSequence.cs");
        var source = File.ReadAllText(path);
        var body = CodeOnly(source);

        string[] forbidden =
        [
            "Timer", "Delay", "Interval", "Elapsed", "Schedul", "Periodic", "Recurring",
            "Sleep", "Stopwatch", "TickCount", "DateTime.UtcNow", "DateTime.Now",
            "Environment.Tick", "PeriodicTimer", "OnTick", "AutoCall", "Repeat",
        ];

        var found = forbidden
            .Where(name => body.Contains(name, StringComparison.Ordinal))
            .ToList();

        _output.WriteLine($"sequence       : {path}");
        _output.WriteLine($"lines          : {source.Split('\n').Length}");
        _output.WriteLine($"patterns tried : {forbidden.Length}");
        _output.WriteLine($"found in code  : {(found.Count == 0 ? "none" : string.Join(", ", found))}");

        Assert.Empty(found);
    }

    /// <summary>
    /// **Nothing in the shipped tree calls the sequence.**
    /// </summary>
    /// <remarks>
    /// <para>It is built, proved and unreachable. The first caller is step 5's
    /// right-click; until then the only way into a transmission is a test.</para>
    /// <para>Doc comments are stripped first: <c>CivConstants.PttOn</c> and
    /// <see cref="ITransmitAudioSink"/> both name the sequence in their remarks,
    /// saying where the keying write lives and what does not implement it. **A
    /// sentence about a type is not a call to it**, and a test that could not tell
    /// those apart would push this unit's own documentation out of the tree.
    /// </para>
    /// </remarks>
    [Fact]
    public void NothingInTheShippedTreeCallsTheSequence()
    {
        var source = Path.Combine(RepositoryRoot(), "src");

        var callers = Directory
            .EnumerateFiles(source, "*.cs", SearchOption.AllDirectories)
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(file => Path.GetFileName(file) != "Ft8TransmitSequence.cs")
            .Where(file => CodeOnly(File.ReadAllText(file))
                .Contains("Ft8TransmitSequence", StringComparison.Ordinal))
            .ToList();

        foreach (var caller in callers)
        {
            _output.WriteLine($"caller: {caller}");
        }

        _output.WriteLine($"callers under src/: {callers.Count}");

        Assert.Empty(callers);
    }

    /// <summary>One operator's send, with everything the sequence needs.</summary>
    /// <param name="licenseClass">The operator's class.</param>
    /// <param name="frequencyHz">Where it would go out.</param>
    /// <param name="guardEnabled">The Settings toggle.</param>
    /// <param name="startSecondsIntoSlot">How far after the boundary it starts.</param>
    /// <param name="text">What to say.</param>
    /// <remarks>
    /// <c>ComposeSignal</c>, not <c>Compose</c>: the send path plays the 12.64 s
    /// of tones, and the padded 15 s slot is what a decoder reads.
    /// </remarks>
    internal static OperatorSend Send(
        LicenseClass licenseClass = LicenseClass.General,
        long frequencyHz = 14_074_000,
        bool guardEnabled = true,
        double startSecondsIntoSlot = 0.5,
        string text = "CQ KC3QIS FN00")
    {
        var composed = Ft8Composer.ComposeSignal(text);
        Assert.True(composed.Composed, composed.Explanation);

        return new OperatorSend(
            composed.Transmission!,
            frequencyHz,
            licenseClass,
            guardEnabled,
            new DateTime(2026, 9, 6, 23, 45, 0, DateTimeKind.Utc),
            startSecondsIntoSlot);
    }

    /// <summary>A source file with its doc comments taken out.</summary>
    /// <param name="source">The whole file.</param>
    /// <returns>Every line that is not a <c>///</c> line.</returns>
    internal static string CodeOnly(string source)
        => string.Join(
            '\n',
            source.Split('\n')
                .Where(line => !line.TrimStart().StartsWith("///", StringComparison.Ordinal)));

    /// <summary>Bytes as the record shows them: two hex digits, space separated.</summary>
    internal static string Hex(IEnumerable<byte> bytes)
        => string.Join(' ', bytes.Select(b => b.ToString("X2", CultureInfo.InvariantCulture)));

    /// <summary>The repository root, found from the test assembly's location.</summary>
    internal static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Hamlet.sln")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return directory!.FullName;
    }

    /// <summary>What one failure mode did, for the record.</summary>
    private void Report(string mode, TransmitRun run, FakeSerialPort port)
    {
        _output.WriteLine($"mode           : {mode}");
        _output.WriteLine($"outcome        : {run.Outcome}");
        _output.WriteLine($"came out of tx : {run.CameOutOfTransmit}");
        _output.WriteLine($"keyed          : {run.Keyed}, unkeyed normally: {run.UnkeyedNormally}");
        _output.WriteLine($"writes tried   : {port.WritesAttempted}");
        _output.WriteLine($"wire           : {Hex(port.Written)}");
        _output.WriteLine($"reason         : {run.Reason}");

        if (run.Abort is { } abort)
        {
            _output.WriteLine(
                $"abort cw stop  : {Hex(abort.CwStop.Frame.ToWireBytes())} "
                + $"written {abort.CwStop.Written} {abort.CwStop.Failure}");
            _output.WriteLine(
                $"abort ptt off  : {Hex(abort.PttOff.Frame.ToWireBytes())} "
                + $"written {abort.PttOff.Written} {abort.PttOff.Failure}");
        }
    }
}
