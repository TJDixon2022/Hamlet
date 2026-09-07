using System.Diagnostics;
using Ft8Sharp.Dsp;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Tests.Rig;
using Hamlet.RadioEngine.Transmit;
using NAudio.CoreAudioApi;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// **THE LOOPBACK. Compose, play out of the machine, capture it coming back,
/// resample, decode, and get the same message.**
/// </summary>
/// <remarks>
/// <para>**This is step 3's third exit criterion in the plan's own words** -
/// *a loopback proves the whole chain: generate, play, capture on the tap,
/// decode, and get the message back* - and the plan says in the same sentence
/// that it needs no antenna and no radio state.</para>
/// <para>**IT MAKES REAL SOUND ON A REAL COMPUTER.** One transmission is 12.64
/// seconds of FT8 tones leaving a sound card at the speed sound leaves a sound
/// card, and the run says which endpoint, at what peak, and for how long.</para>
/// <para>**NOTHING HERE OPENS A SERIAL PORT OR KEYS ANYTHING.** The device in
/// this test is a sound card. A real port and a real sink together would be a
/// real transmission and this machine has no radio to make one on
/// (`SHACK_FACTS.md` FACT-004).</para>
/// <para>**AND THE COMPARISON IS THE WHOLE MESSAGE OR NOTHING** - ordinal, on
/// `ReadsBackAs`, exactly as the existing round-trip test compares. Not a
/// substring, not a prefix, not a callsign.</para>
/// </remarks>
public sealed class TheLoopbackProvesTheWholeChainTests
{
    /// <summary>How long to let the capture settle before playing.</summary>
    private static readonly TimeSpan PreRoll = TimeSpan.FromSeconds(1);

    /// <summary>How long to keep capturing after the play returns.</summary>
    private static readonly TimeSpan PostRoll = TimeSpan.FromSeconds(1);

    /// <summary>One FT8 slot, which is what the decoder's geometry describes.</summary>
    private const double SlotSeconds = 15.0;

    private readonly ITestOutputHelper _output;

    public TheLoopbackProvesTheWholeChainTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **THE ONE. One message, out of the sound card and back into Hamlet's own
    /// decoder as the same text - and the rate lie, in the same run, returning
    /// nothing.**
    /// </summary>
    /// <remarks>
    /// <para>**THE BREAKAGE IS CARRIED HERE RATHER THAN IN A SECOND
    /// TRANSMISSION.** The captured audio is at the endpoint's rate; hand it to
    /// <c>Ft8SlotDecoder</c> without <c>Ft8Resample.ToFt8Rate</c> and the decoder
    /// is being told that 48000 Hz samples are 12000 Hz samples - four times the
    /// tone spacing, four times the symbol rate. **It returns nothing**, and
    /// nothing about the chain looks broken: the audio played, the capture
    /// arrived, the level was fine. That is the real defect this test exists to
    /// catch.</para>
    /// <para>Decoding the same capture twice costs no extra wall clock, and it
    /// keeps this whole class inside the cap of three transmissions.</para>
    /// </remarks>
    [Fact]
    public async Task AMessageHamletComposedLeavesThisMachineAndComesBackFromItsOwnDecoder()
    {
        var run = await Loopback("CQ KC3QIS FN00");

        Report(run);

        Assert.True(run.Attempted, run.Note);

        // **§0.0. A decode that did not happen is reported as one that did not
        // happen**, with what did come back, rather than being talked around.
        Assert.True(
            run.CameBack,
            $"the message \"{run.Sent}\" went out of {run.Endpoint} and the decoder returned "
            + $"{Quoted(run.Texts)} from the captured audio. The capture peaked at "
            + $"{run.CapturePeakDb:0.#} dB; the tap's live meter last read peak {run.PeakDb:0.#} dB, "
            + $"floor {run.FloorDb:0.#} dB.");

        // **AND THE RATE LIE RETURNS NOTHING**, watched red before this chain was
        // right and kept as an assertion so it stays caught.
        Assert.DoesNotContain(run.Expected, run.TextsUnresampled, StringComparer.Ordinal);

        // The whole send path ran: the gate permitted, the radio was keyed and it
        // came back out of transmit the ordinary way.
        Assert.Equal(Ft8TransmitOutcome.Sent.ToString(), run.Outcome);
        Assert.Equal(UnkeyRoute.OrdinaryUnkey.ToString(), run.CameOutOfTransmit);

        // **AND THE RECORD CARRIES NO CALLSIGN AND NO MESSAGE** (HM-DEC-018).
        // The message that just went out is not findable anywhere in the record's
        // keys or its values.
        var written = string.Join(
            "|",
            run.Record.Select(pair => $"{pair.Key}={pair.Value}"));

        Assert.NotEmpty(run.Record);
        Assert.DoesNotContain("KC3QIS", written, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(run.Sent, written, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// **Two more messages, of other shapes, out and back.**
    /// </summary>
    /// <remarks>
    /// <para>**THE NAMED DROP CANDIDATE** (work instruction 256, task 4). The
    /// first message proves the chain; these two only widen it, and each costs
    /// 12.64 seconds of wall clock. Three transmissions in this class in total,
    /// which is the cap.</para>
    /// <para>A report-with-grid and a signal report, rather than three CQs: the
    /// point of widening is to send a different message type through the same
    /// path, not the same one three times.</para>
    /// </remarks>
    [Fact]
    public async Task TwoMoreMessagesOfOtherShapesGoOutAndComeBack()
    {
        string[] messages = ["KC3QIS W9XYZ FN00", "W9XYZ KC3QIS -12"];

        var back = 0;
        var failures = new List<string>();
        var clock = Stopwatch.StartNew();

        foreach (var message in messages)
        {
            var run = await Loopback(message);

            Report(run);
            _output.WriteLine(string.Empty);

            if (!run.Attempted)
            {
                failures.Add($"\"{message}\" - not attempted: {run.Note}");
                continue;
            }

            if (run.CameBack)
            {
                back++;
            }
            else
            {
                failures.Add(
                    $"\"{message}\" went out and the decoder returned {Quoted(run.Texts)}; "
                    + $"the capture peaked at {run.CapturePeakDb:0.#} dB");
            }
        }

        clock.Stop();

        _output.WriteLine($"came back  : {back} of {messages.Length}");
        _output.WriteLine($"wall clock : {clock.Elapsed.TotalSeconds:0.###} s");

        Assert.True(
            failures.Count == 0,
            string.Join("\n", failures));
    }

    /// <summary>Everything one loopback run measured.</summary>
    private sealed record Run(
        bool Attempted,
        string Note,
        string Endpoint,
        string Sent,
        string Expected,
        int RateAsked,
        int RateGot,
        int CaptureRate,
        int CaptureChannels,
        int SamplesOffered,
        int SamplesPlayed,
        double PlaySeconds,
        double WallSeconds,
        double PeakWritten,
        double RmsWritten,
        long Clipped,
        long CapturedSamples,
        double CapturePeakDb,
        double PeakDb,
        double FloorDb,
        bool NearlySilent,
        string Outcome,
        string CameOutOfTransmit,
        int WireBytes,
        IReadOnlyDictionary<string, object?> Record,
        IReadOnlyList<string> Texts,
        IReadOnlyList<string> TextsUnresampled)
    {
        public bool CameBack => Texts.Contains(Expected, StringComparer.Ordinal);
    }

    /// <summary>
    /// Composes, plays, captures the same endpoint, resamples and decodes.
    /// </summary>
    /// <param name="text">The message to send.</param>
    /// <returns>What happened, measured.</returns>
    private async Task<Run> Loopback(string text)
    {
        var endpoint = RenderChoice.Preferred(out var why);

        if (endpoint is null)
        {
            return NotAttempted($"LOOPBACK ROUTE: file would be forced - {why} (FACT-004)");
        }

        _output.WriteLine($"chosen because  : {why}");

        using var sink = new WasapiTransmitSink(endpoint.Id);
        var rate = sink.EndpointSampleRate;

        // **ComposeSignal, not Compose.** The signal alone is what goes on the
        // air; the padded slot is the route into a decoder and would key a radio
        // for 1.18 s of silence first.
        var composed = Ft8Composer.ComposeSignal(text, rate);
        Assert.True(composed.Composed, composed.Explanation);
        var transmission = composed.Transmission!;

        var tap = new AudioTap();
        var mono = Array.Empty<float>();
        var captureRate = 0;
        var captureChannels = 0;

        using var enumerator = new MMDeviceEnumerator();
        using var device = enumerator.GetDevice(endpoint.Id);
        using var capture = new NAudio.Wave.WasapiLoopbackCapture(device);
        using var stopped = new ManualResetEventSlim(false);

        captureRate = capture.WaveFormat.SampleRate;
        captureChannels = capture.WaveFormat.Channels;

        capture.DataAvailable += (_, e) =>
        {
            if (e.BytesRecorded <= 0)
            {
                // **AN IDLE ENDPOINT DELIVERS EMPTY PACKETS**, measured in task 1.
                // They are not silence to be recorded; they are nothing arriving.
                return;
            }

            // **THE ENGINE'S OWN DOWNMIX, NOT A SECOND ONE.** This is the same
            // conversion the receive path uses on every buffer the radio delivers.
            var frames = WasapiAudioSource.Downmix(
                e.Buffer, e.BytesRecorded, capture.WaveFormat, ref mono);

            if (frames > 0)
            {
                tap.Take(mono.AsSpan(0, frames), capture.WaveFormat.SampleRate);
            }
        };

        capture.RecordingStopped += (_, _) => stopped.Set();

        // **THE WHOLE SEND PATH, NOT JUST THE SINK.** The gate, the keying frame,
        // the play and the guaranteed unkey, with the record written at the end -
        // so the transmission that goes through this loopback is recorded exactly
        // the way unit 255 built it. The transport is unit 253's fake: a real port
        // beside a real sink would be a real transmission.
        var port = new FakeSerialPort();
        var telemetry = new RecordingTelemetry();

        var send = new OperatorSend(
            transmission,
            14_074_000,
            LicenseClass.General,
            true,
            new DateTime(2026, 9, 6, 23, 45, 0, DateTimeKind.Utc),
            0.5);

        var clock = Stopwatch.StartNew();

        capture.StartRecording();
        await Task.Delay(PreRoll).ConfigureAwait(false);

        var run = await new Ft8TransmitSequence(port, sink, guard: null, telemetry: telemetry)
            .RunAsync(send)
            .ConfigureAwait(false);

        var played = run.Played ?? new PlayedAudio(0, TimeSpan.Zero);

        await Task.Delay(PostRoll).ConfigureAwait(false);
        capture.StopRecording();
        stopped.Wait(TimeSpan.FromSeconds(5));

        var captured = tap.Snapshot();
        clock.Stop();

        if (captured is null || captured.Samples.Length == 0)
        {
            return NotAttempted(
                "the loopback capture started and delivered nothing while the endpoint was "
                + $"rendering - {tap.SamplesSeen} samples reached the tap");
        }

        // **THE CAPTURE, PLACED AT THE START OF ONE SLOT.** The decoder's geometry
        // describes 15 s and the capture is 12.64 s of signal with nothing before
        // it, so the rest of the slot is silence - the same shape the composer's
        // own SynthesizeSlot produces, and well inside the search's -10 to +19
        // block sweep. Nothing here resamples, buffers or decodes anything: the
        // three that do are Ft8Resample, AudioTap and Ft8SlotDecoder.
        var slot = new float[(int)Math.Round(SlotSeconds * captured.SampleRate)];
        var take = Math.Min(captured.Samples.Length, slot.Length);
        Array.Copy(captured.Samples, slot, take);

        var decoder = new Ft8SlotDecoder();

        // The rate lie, decoded first: the endpoint's own samples handed over as
        // though they were the decoder's rate.
        var lied = decoder.Decode(slot).Texts;

        var resampled = Ft8Resample.ToFt8Rate(new MonoAudio(captured.SampleRate, slot));
        var texts = decoder.Decode(resampled.Samples).Texts;

        return new Run(
            true,
            string.Empty,
            sink.DeviceName,
            transmission.Text,
            transmission.ReadsBackAs,
            sink.RateAsked,
            sink.RateGot,
            captureRate,
            captureChannels,
            transmission.Samples.Length,
            played.SamplesPlayed,
            played.Took.TotalSeconds,
            clock.Elapsed.TotalSeconds,
            sink.PeakWritten,
            sink.RmsWritten,
            sink.ClippedSamples,
            captured.Samples.Length,

            // **THE PEAK OF THE WHOLE CAPTURE, NOT THE METER'S LAST READING.**
            // AudioTap.Level is the last fifth of a second, which by the time a
            // run finishes is the silence after the transmission - the tap's own
            // remarks on PeakOf say exactly this and it is why PeakOf exists.
            AudioTap.PeakOf(captured),
            tap.Level.PeakDb,
            tap.Level.FloorDb,
            tap.Level.NearlySilent,
            run.Outcome.ToString(),
            run.CameOutOfTransmit.ToString(),
            port.Written.Length,
            telemetry.Events.Count == 1
                ? telemetry.Events[0].Data
                : new Dictionary<string, object?>(StringComparer.Ordinal),
            texts,
            lied);
    }

    /// <summary>A run that never got as far as playing anything.</summary>
    private static Run NotAttempted(string note) =>
        new(false, note, "none", "", "", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -90, 0, 0, true,
            "none", "none", 0, new Dictionary<string, object?>(StringComparer.Ordinal), [], []);

    /// <summary>Everything the run measured, printed.</summary>
    private void Report(Run run)
    {
        if (!run.Attempted)
        {
            _output.WriteLine($"NOT ATTEMPTED: {run.Note}");

            return;
        }

        _output.WriteLine("LOOPBACK ROUTE  : device");
        _output.WriteLine($"endpoint        : {run.Endpoint}  [development machine]");
        _output.WriteLine($"rate asked      : {run.RateAsked} Hz");
        _output.WriteLine($"rate got        : {run.RateGot} Hz  [development machine]");
        _output.WriteLine(
            $"capture format  : {run.CaptureRate} Hz, {run.CaptureChannels} ch  [development machine]");
        _output.WriteLine($"offered         : {run.SamplesOffered} samples, "
            + $"{run.SamplesOffered / (double)run.RateGot:0.###} s");
        _output.WriteLine($"played          : {run.SamplesPlayed} samples in {run.PlaySeconds:0.###} s");
        _output.WriteLine($"peak written    : {run.PeakWritten:0.######}  [development machine]");
        _output.WriteLine($"rms written     : {run.RmsWritten:0.######}  [development machine]");
        _output.WriteLine($"clipped         : {run.Clipped}");
        _output.WriteLine($"captured        : {run.CapturedSamples} samples on the tap, "
            + $"{run.CapturedSamples / (double)run.CaptureRate:0.###} s");
        _output.WriteLine(
            $"capture peak    : {run.CapturePeakDb:0.#} dBFS over the whole capture  [development machine]");
        _output.WriteLine($"tap live meter  : peak {run.PeakDb:0.#} dB, floor {run.FloorDb:0.#} dB, "
            + $"nearly silent: {run.NearlySilent} - the last 0.2 s, which is after the "
            + "transmission  [development machine]");
        _output.WriteLine($"message in      : \"{run.Sent}\"");
        _output.WriteLine($"reads back as   : \"{run.Expected}\"");
        _output.WriteLine($"decoded         : {Quoted(run.Texts)}");
        _output.WriteLine($"decoded (lied)  : {Quoted(run.TextsUnresampled)}");
        _output.WriteLine($"came back       : {run.CameBack}");
        _output.WriteLine($"outcome         : {run.Outcome}");
        _output.WriteLine($"came out of tx  : {run.CameOutOfTransmit}");
        _output.WriteLine($"wire            : {run.WireBytes} bytes to the fake transport");
        _output.WriteLine("record          :");

        foreach (var pair in run.Record)
        {
            _output.WriteLine($"    {pair.Key,-22}: {pair.Value}");
        }

        _output.WriteLine($"wall clock      : {run.WallSeconds:0.###} s");
    }

    /// <summary>A list of decodes, quoted, or the word nothing.</summary>
    private static string Quoted(IReadOnlyList<string> texts) =>
        texts.Count == 0 ? "nothing" : "\"" + string.Join("\", \"", texts) + "\"";

    /// <summary>Keeps what the sequence wrote, so the record can be quoted.</summary>
    private sealed class RecordingTelemetry : ITelemetry
    {
        private readonly List<TelemetryEvent> _events = [];

        /// <summary>Everything written, in order.</summary>
        public IReadOnlyList<TelemetryEvent> Events => _events;

        /// <inheritdoc/>
        public long DroppedEventCount => 0;

        /// <inheritdoc/>
        public void Write(
            TelemetryCategory category,
            string eventName,
            IReadOnlyDictionary<string, object?>? data = null,
            TelemetryLevel level = TelemetryLevel.Info)
            => _events.Add(new TelemetryEvent(
                new DateTime(2026, 9, 6, 23, 45, 1, DateTimeKind.Utc),
                "test",
                level,
                "test",
                category,
                eventName,
                data ?? new Dictionary<string, object?>(StringComparer.Ordinal)));
    }
}
