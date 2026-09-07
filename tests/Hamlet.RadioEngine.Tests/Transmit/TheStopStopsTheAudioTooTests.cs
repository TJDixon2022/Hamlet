using System.Globalization;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Transmit;
using Hamlet.RadioEngine.Transport;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Transmit;

/// <summary>
/// **The operator's stop stops the audio as well as the carrier.**
/// </summary>
/// <remarks>
/// <para>**WHAT THIS EXISTS FOR** (work instruction 263). Unit 261 gave the
/// operator a stop and recorded, in its own entry, that it takes the carrier off
/// and leaves Hamlet feeding the rest of a 12.64 second transmission into the
/// radio. Which is harmless if the radio heard the unkey - and is the whole
/// transmission going out over other people's band if it did not, **which is the
/// exact case the abort exists for.**</para>
/// <para>**THE RED THIS WAS WATCHED AT**, before <c>Ft8ArmedSend</c> had a
/// cancellation source. The wire at the moment of the stop:</para>
/// <code>
/// wire at the stop   : FE FE 94 E0 1C 00 01 FD | FE FE 94 E0 17 FF FD | FE FE 94 E0 1C 00 00 FD
/// played at the stop : 51539
/// played in the end  : 151680 of 151680
/// the run said       : Sent
/// AUDIO AFTER THE STOP: 8345 ms
/// </code>
/// <para>**The carrier came off and the transmission carried on.** 12,640 ms of
/// audio in the slot, of which **8,345 ms left the machine after the operator
/// pressed stop**, and the run reported itself `Sent`. The application handed the
/// boundary no token at all
/// (<c>MainWindowViewModel.cs:8258</c>), so the whole chain ran on
/// <c>CancellationToken.None</c> and nothing could ever have stopped it -
/// <c>docs/unit263-stop-audio-trace.md</c> Q1.</para>
/// <para>**WHY THE EXISTING SUITE WAS GREEN THROUGH ALL OF IT.** Twelve of the
/// twelve tests in <see cref="TheOperatorsStopFiresFromEveryStateTests"/> would
/// still pass if the audio never stopped, because not one assertion in that file
/// reads <see cref="PlayedAudio.SamplesPlayed"/> (trace Q7). The fake it runs
/// against could not have stopped either.</para>
/// <para>**NO DEVICE, NO PORT, NO SOUND** (`SHACK_FACTS.md` FACT-004). The audio
/// is <see cref="FakeTransmitAudioSink"/> with <c>PlaysOver</c> set, which counts
/// samples against a wall clock and makes no sound; the wire is
/// <see cref="RecordingPort"/>, which opens nothing.</para>
/// </remarks>
public sealed class TheStopStopsTheAudioTooTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fixture.</summary>
    /// <param name="output">Where the wire and the milliseconds are quoted.</param>
    public TheStopStopsTheAudioTooTests(ITestOutputHelper output) => _output = output;

    /// <summary>A slot boundary in the middle of a minute.</summary>
    private static readonly DateTime Boundary =
        new(2026, 9, 7, 1, 30, 0, DateTimeKind.Utc);

    /// <summary>What the radio sees when it is keyed.</summary>
    private const string KeyOn = "FE FE 94 E0 1C 00 01 FD";

    /// <summary>The abort's first frame: stop whatever the keyer is sending.</summary>
    private const string CwStop = "FE FE 94 E0 17 FF FD";

    /// <summary>The abort's second frame, and the ordinary unkey: back to receive.</summary>
    private const string PttOff = "FE FE 94 E0 1C 00 00 FD";

    /// <summary>
    /// How long the fake takes to play the whole slot. **The audio milliseconds
    /// this file reports are computed from samples and the rate, not from this**,
    /// so compressing the slot keeps the test quick without touching the number
    /// that matters.
    /// </summary>
    private static readonly TimeSpan SlotCompressedTo = TimeSpan.FromSeconds(2);

    /// <summary>
    /// **Mid-transmission, the stop takes the carrier off AND stops the sound.**
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS THE UNIT'S ONE QUESTION.** The radio is keyed, the tones
    /// are going out, and the operator presses stop from another thread - which is
    /// what a click on a window does. Both halves are then read: the wire, for the
    /// two abort frames, and the sink, for how many samples it actually got
    /// through.</para>
    /// <para>**THE NUMBER IS AUDIO MILLISECONDS, NOT WALL MILLISECONDS.** Samples
    /// played after the stop, over the composed rate: that is how much of the
    /// transmission left the machine after the operator asked for it to stop, and
    /// it does not care how fast the fake ran.</para>
    /// <para>**THE BOUNDARY IS RUN WITH NO TOKEN AT ALL**, exactly as
    /// <c>MainWindowViewModel.cs:8250</c> runs it. A test that handed one in would
    /// prove a path the application does not take.</para>
    /// </remarks>
    [Fact]
    public async Task MidTransmissionTheStopTakesTheCarrierOffAndStopsTheSound()
    {
        var port = new RecordingPort();
        var sink = new FakeTransmitAudioSink { PlaysOver = SlotCompressedTo };
        var armed = new Ft8ArmedSend(new Ft8TransmitSequence(port, sink));
        var send = SendAt(Boundary);
        var total = send.Transmission.Samples.Length;
        var rate = send.Transmission.SampleRate;

        armed.Arm(send);

        // NO TOKEN. This is the application's own call shape.
        var running = armed.AtBoundaryAsync(Boundary);

        Assert.True(sink.Entered.Wait(TimeSpan.FromSeconds(10)), "the sink was never reached");

        // Let a third of the transmission go out, so there is something left to
        // stop and the number below is not a rounding artefact.
        var playedAtStop = await PlayedPast(sink, total / 3);

        Assert.Equal(new[] { KeyOn }, port.Wire);

        var stop = armed.StopNow(port);
        var wireWhileRunning = port.Wire;

        var boundary = await running;
        var playedInTheEnd = boundary.Run!.Played!.Value.SamplesPlayed;

        var afterTheStop = Milliseconds(playedInTheEnd - playedAtStop, rate);

        _output.WriteLine("the slot was       : " + total + " samples at " + rate
            + " Hz = " + Milliseconds(total, rate).ToString("F0", CultureInfo.InvariantCulture)
            + " ms of audio");
        _output.WriteLine("played at the stop : " + playedAtStop);
        _output.WriteLine("played in the end  : " + playedInTheEnd + " of " + total);
        _output.WriteLine("wire at the stop   : " + string.Join(" | ", wireWhileRunning));
        _output.WriteLine("wire at the end    : " + string.Join(" | ", port.Wire));
        _output.WriteLine("the run said       : " + boundary.Run.Outcome);
        _output.WriteLine("stop outcome       : " + stop.Outcome);
        _output.WriteLine("AUDIO AFTER THE STOP: "
            + afterTheStop.ToString("F0", CultureInfo.InvariantCulture) + " ms");

        // ---- the carrier, which already worked before this unit --------------
        Assert.True(stop.AnythingReachedTheRadio);
        Assert.True(stop.Abort!.CwStop.Written);
        Assert.True(stop.Abort.PttOff.Written);
        Assert.Equal(new[] { KeyOn, CwStop, PttOff }, wireWhileRunning);

        // ---- the sound, which is what this unit is for -----------------------
        Assert.True(
            sink.StoppedByTheToken,
            $"the sink played all {total} samples: the token never reached it");

        Assert.True(
            playedInTheEnd < total,
            $"the whole slot went out anyway: {playedInTheEnd} of {total} samples");

        // A THIRD OF A SLOT IS A LOT OF AUDIO AND IT IS NOT THE POINT. What is
        // asserted is what went out AFTER the operator asked for it to stop.
        Assert.True(
            afterTheStop < 500,
            $"{afterTheStop:F0} ms of audio left the machine after the stop");
    }

    /// <summary>
    /// **A transmission the operator stopped reads as cancelled, not as a fault.**
    /// </summary>
    /// <remarks>
    /// **THE BREAKAGE THIS WOULD HAVE CAUGHT.** Before this unit nothing could
    /// cancel a play, so a short read from the sink meant one thing - the device
    /// failed - and <see cref="Ft8TransmitSequence"/> said
    /// <see cref="Ft8TransmitOutcome.AudioFailed"/> with *"the audio path played X
    /// of Y samples and returned"*. Now a short read has two causes and they are
    /// not the same sentence to put in front of an operator: **one is the sound
    /// card letting him down and the other is him pressing the button.**
    /// </remarks>
    [Fact]
    public async Task AStoppedTransmissionSaysItWasStoppedRatherThanThatTheDeviceFailed()
    {
        var port = new RecordingPort();
        var sink = new FakeTransmitAudioSink { PlaysOver = SlotCompressedTo };
        var armed = new Ft8ArmedSend(new Ft8TransmitSequence(port, sink));
        var send = SendAt(Boundary);

        armed.Arm(send);

        var running = armed.AtBoundaryAsync(Boundary);

        Assert.True(sink.Entered.Wait(TimeSpan.FromSeconds(10)), "the sink was never reached");
        await PlayedPast(sink, send.Transmission.Samples.Length / 3);

        armed.StopNow(port);

        var boundary = await running;

        _output.WriteLine("the run said : " + boundary.Run!.Outcome);
        _output.WriteLine("because      : " + boundary.Run.Reason);
        _output.WriteLine("sent         : " + boundary.Run.Sent);

        Assert.Equal(Ft8TransmitOutcome.Cancelled, boundary.Run.Outcome);
        Assert.False(boundary.Run.Sent);
        Assert.Contains("stopped", boundary.Run.Reason, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// **The abort's frames reach the wire even when the cancel itself throws.**
    /// </summary>
    /// <remarks>
    /// <para>**THE ONE PROPERTY THIS UNIT MAY NOT DAMAGE**, and it is asserted
    /// here rather than argued. `PHASE_PLAN.md` step 1: the abort *cannot be
    /// disabled, deferred, or made conditional* - not on a source being non-null,
    /// not on a token's state, and not on a cancel succeeding.</para>
    /// <para>**AND THE CANCEL IS MADE TO REALLY FAIL, WITH NO SEAM IN PRODUCT
    /// CODE.** <see cref="CancellationTokenSource.Cancel()"/> runs every callback
    /// registered on it synchronously, on the calling thread, and wraps anything
    /// they throw in an <see cref="AggregateException"/>. The sink is handed the
    /// token, so a sink that registers a throwing callback makes the operator's
    /// own stop throw - which is exactly the hazard unit 261 named
    /// (<c>docs/unit263-stop-audio-trace.md</c> Q5), reproduced deliberately
    /// instead of assumed absent.</para>
    /// </remarks>
    [Fact]
    public async Task ACancelThatThrowsDoesNotCostTheOperatorHisAbort()
    {
        var port = new RecordingPort();
        var sink = new ThrowsWhenCancelledSink();
        var armed = new Ft8ArmedSend(new Ft8TransmitSequence(port, sink));

        armed.Arm(SendAt(Boundary));

        var running = armed.AtBoundaryAsync(Boundary);

        Assert.True(sink.Entered.Wait(TimeSpan.FromSeconds(10)), "the sink was never reached");

        var stop = armed.StopNow(port);
        var wireWhileRunning = port.Wire;

        sink.Release();
        await running;

        _output.WriteLine("the cancel        : threw, from a callback on the token");
        _output.WriteLine("callbacks run     : " + sink.CallbacksRun);
        _output.WriteLine("wire at the stop  : " + string.Join(" | ", wireWhileRunning));
        _output.WriteLine("reached the radio : " + stop.AnythingReachedTheRadio);

        Assert.Equal(1, sink.CallbacksRun);

        // THE FRAMES LANDED ANYWAY. That is the whole assertion.
        Assert.True(stop.AnythingReachedTheRadio);
        Assert.Equal(new[] { KeyOn, CwStop, PttOff }, wireWhileRunning);
    }

    /// <summary>
    /// **The stop returns while a full transmission is in flight, inside a stated
    /// bound.**
    /// </summary>
    /// <remarks>
    /// <para>**A STOP THAT WAITS ON THE TRANSMISSION IT IS STOPPING IS THE ONE
    /// PROPERTY IT MAY NOT HAVE.** The bound is stated in the shape unit 253 used:
    /// <see cref="Bound"/>, generous by three orders of magnitude against what the
    /// path actually does, because what is being caught is *an await appearing on
    /// the stop path*, not a slow machine. A tight bound on a build agent is a
    /// flaky test; a loose one still fails instantly on a stop that waits 12.64
    /// seconds.</para>
    /// <para>**AND THE TRANSMISSION IS REALLY IN FLIGHT**, at its full 12.64
    /// seconds of wall time, not a compressed one - so a stop that awaited the
    /// play would take twelve seconds to return and this would say so.</para>
    /// </remarks>
    [Fact]
    public async Task TheStopReturnsInsideItsBoundWhileTwelveSecondsOfAudioAreInFlight()
    {
        var port = new RecordingPort();
        var sink = new FakeTransmitAudioSink { PlaysOver = TimeSpan.FromSeconds(12.64) };
        var armed = new Ft8ArmedSend(new Ft8TransmitSequence(port, sink));

        armed.Arm(SendAt(Boundary));

        var running = armed.AtBoundaryAsync(Boundary);

        Assert.True(sink.Entered.Wait(TimeSpan.FromSeconds(10)), "the sink was never reached");

        var clock = System.Diagnostics.Stopwatch.StartNew();
        var stop = armed.StopNow(port);
        clock.Stop();

        var boundary = await running;

        _output.WriteLine("the slot was      : 12.64 s of wall time, really in flight");
        _output.WriteLine("StopNow took      : "
            + clock.Elapsed.TotalMilliseconds.ToString("F1", CultureInfo.InvariantCulture)
            + " ms");
        _output.WriteLine("the bound is      : " + Bound + " ms");
        _output.WriteLine("the run then said : " + boundary.Run!.Outcome);

        Assert.True(
            clock.Elapsed.TotalMilliseconds < Bound,
            $"StopNow took {clock.Elapsed.TotalMilliseconds:F1} ms, which is past the "
            + $"{Bound} ms bound - it waited for something");

        Assert.True(stop.AnythingReachedTheRadio);
    }

    /// <summary>
    /// The bound <see cref="Ft8ArmedSend.StopNow"/> returns inside, in
    /// milliseconds.
    /// </summary>
    /// <remarks>
    /// **250 ms, and the number is a ceiling rather than a measurement.** What the
    /// path does is a lock, a field write, two synchronous port writes and a flag
    /// set; on this machine it is well under a millisecond. The bound is set two
    /// to three orders of magnitude above that so that a loaded build agent cannot
    /// make it red, while a stop that waited on the 12,640 ms transmission fails
    /// it fifty times over.
    /// </remarks>
    private const int Bound = 250;

    /// <summary>Milliseconds of audio, from a sample count and a rate.</summary>
    private static double Milliseconds(int samples, int sampleRate)
        => samples * 1000.0 / sampleRate;

    /// <summary>
    /// Waits until the sink has played past a mark, and says where it got to.
    /// </summary>
    /// <param name="sink">The sink counting samples against a wall clock.</param>
    /// <param name="mark">The sample count to get past.</param>
    /// <returns>What <c>PlayedSoFar</c> read at the moment it went past.</returns>
    private static async Task<int> PlayedPast(FakeTransmitAudioSink sink, int mark)
    {
        var deadline = System.Diagnostics.Stopwatch.StartNew();

        while (sink.PlayedSoFar < mark && deadline.Elapsed < TimeSpan.FromSeconds(30))
        {
            await Task.Delay(2, CancellationToken.None).ConfigureAwait(false);
        }

        Assert.True(sink.PlayedSoFar >= mark, "the transmission never got past the mark");

        return sink.PlayedSoFar;
    }

    /// <summary>One send, armed for a stated boundary.</summary>
    private static OperatorSend SendAt(DateTime slotStartUtc)
    {
        var composed = Ft8Composer.ComposeSignal("W1ABC KC3QIS -10");

        Assert.True(composed.Composed, composed.Explanation);

        return new OperatorSend(
            composed.Transmission!, 14_074_000, LicenseClass.General, true, slotStartUtc, 0.5);
    }

    // ---- the fakes -------------------------------------------------------

    /// <summary>A CI-V transport that keeps whole frames and opens nothing.</summary>
    /// <remarks>**NOTHING HERE OPENS A PORT** (`SHACK_FACTS.md` FACT-004).</remarks>
    private sealed class RecordingPort : ISerialPort
    {
        private readonly List<byte[]> _frames = [];
        private readonly object _gate = new();

        /// <summary>Every frame taken, as hex, whole and in order.</summary>
        public string[] Wire
        {
            get
            {
                lock (_gate)
                {
                    return _frames
                        .Select(frame => string.Join(
                            ' ',
                            frame.Select(b => b.ToString("X2", CultureInfo.InvariantCulture))))
                        .ToArray();
                }
            }
        }

        /// <inheritdoc/>
        public bool IsOpen { get; private set; } = true;

        /// <inheritdoc/>
        public string PortName => "COM-AUDIO-STOP";

        /// <inheritdoc/>
        public int BaudRate => 115_200;

        /// <inheritdoc/>
        public void Open() => IsOpen = true;

        /// <inheritdoc/>
        public void Close() => IsOpen = false;

        /// <inheritdoc/>
        public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken)
            => ValueTask.FromResult(0);

        /// <inheritdoc/>
        public ValueTask WriteAsync(
            ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
        {
            Take(buffer.ToArray());

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public void Write(ReadOnlySpan<byte> buffer) => Take(buffer.ToArray());

        /// <inheritdoc/>
        public void Dispose() => IsOpen = false;

        private void Take(byte[] frame)
        {
            lock (_gate)
            {
                _frames.Add(frame);
            }
        }
    }

    /// <summary>
    /// A sink that registers a throwing callback on the token it is handed, and
    /// then parks.
    /// </summary>
    /// <remarks>
    /// **THIS IS HOW THE CANCEL IS MADE TO FAIL FOR REAL.** It is also the one
    /// registration on the whole transmit path, put there deliberately by a test
    /// to reproduce the hazard unit 261 named: with a callback present,
    /// <c>Cancel()</c> runs it on the operator's own thread and rethrows what it
    /// throws, wrapped. **Product code registers nothing** - trace Q2's grep
    /// returns zero - and this fake exists to prove the abort survives even if
    /// that ever changed.
    /// </remarks>
    private sealed class ThrowsWhenCancelledSink : ITransmitAudioSink
    {
        private readonly ManualResetEventSlim _release = new(false);

        /// <summary>Set once something is actually inside the play.</summary>
        public ManualResetEventSlim Entered { get; } = new(false);

        /// <summary>How many times the throwing callback actually ran.</summary>
        public int CallbacksRun { get; private set; }

        /// <summary>Let the parked transmission finish.</summary>
        public void Release() => _release.Set();

        /// <inheritdoc/>
        public int EndpointSampleRate => Ft8Composer.DefaultSampleRate;

        /// <inheritdoc/>
        public async Task<PlayedAudio> PlayAsync(
            ReadOnlyMemory<float> samples, int sampleRate, CancellationToken cancellationToken)
        {
            var count = samples.Length;

            using var registration = cancellationToken.Register(() =>
            {
                CallbacksRun++;

                throw new InvalidOperationException(
                    "a callback on the transmit token threw (scripted).");
            });

            return await Task.Run(
                () =>
                {
                    Entered.Set();

                    Assert.True(
                        _release.Wait(TimeSpan.FromSeconds(30)),
                        "the parked transmission was never released");

                    return new PlayedAudio(count, TimeSpan.FromSeconds(12.64));
                },
                CancellationToken.None).ConfigureAwait(false);
        }
    }
}
