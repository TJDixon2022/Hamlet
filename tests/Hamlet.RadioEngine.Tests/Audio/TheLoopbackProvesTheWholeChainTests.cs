using System.Diagnostics;
using Ft8Sharp.Dsp;
using Hamlet.RadioEngine.Audio;
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
    /// decoder as the same text.**
    /// </summary>
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
            + $"{Quoted(run.Texts)} from the captured audio. Tap level: peak {run.PeakDb:0.#} dB, "
            + $"floor {run.FloorDb:0.#} dB, nearly silent: {run.NearlySilent}.");
    }

    /// <summary>
    /// **THE BREAKAGE THIS TEST EXISTS TO CATCH: one number wrong on the way
    /// back, and a perfectly good transmission decodes to nothing.**
    /// </summary>
    /// <remarks>
    /// <para>The captured audio is at the endpoint's rate. Hand it to
    /// <c>Ft8SlotDecoder</c> without <c>Ft8Resample.ToFt8Rate</c> and the decoder
    /// is being told that 48000 Hz samples are 12000 Hz samples - four times the
    /// tone spacing and four times the symbol rate. **It returns nothing**, and
    /// nothing about the chain looks broken: the audio played, the capture
    /// arrived, the level was fine.</para>
    /// <para>**This assertion is the red watched and kept.** The same capture is
    /// decoded twice in one run, so it costs no extra wall clock, and the defect
    /// stays caught.</para>
    /// </remarks>
    [Fact]
    public async Task TheSameCaptureDecodesToNothingWhenItsRateIsLiedAbout()
    {
        var run = await Loopback("CQ KC3QIS FN00");

        Report(run);

        Assert.True(run.Attempted, run.Note);

        _output.WriteLine("--- the rate lie ---");
        _output.WriteLine($"told the decoder {run.CaptureRate} Hz audio was "
            + $"{Ft8Resample.TargetSampleRate} Hz");
        _output.WriteLine($"decoder returned : {Quoted(run.TextsUnresampled)}");

        Assert.DoesNotContain(run.Expected, run.TextsUnresampled, StringComparer.Ordinal);

        // And the same samples, resampled, do come back. Without this the
        // assertion above would also pass on a chain that captured silence.
        Assert.Contains(run.Expected, run.Texts, StringComparer.Ordinal);
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
        double PeakDb,
        double FloorDb,
        bool NearlySilent,
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

        var clock = Stopwatch.StartNew();

        capture.StartRecording();
        await Task.Delay(PreRoll).ConfigureAwait(false);

        var played = await sink
            .PlayAsync(transmission.Samples, rate, CancellationToken.None)
            .ConfigureAwait(false);

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
            tap.Level.PeakDb,
            tap.Level.FloorDb,
            tap.Level.NearlySilent,
            texts,
            lied);
    }

    /// <summary>A run that never got as far as playing anything.</summary>
    private static Run NotAttempted(string note) =>
        new(false, note, "none", "", "", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, true, [], []);

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
        _output.WriteLine($"tap level       : peak {run.PeakDb:0.#} dB, floor {run.FloorDb:0.#} dB, "
            + $"nearly silent: {run.NearlySilent}  [development machine]");
        _output.WriteLine($"message in      : \"{run.Sent}\"");
        _output.WriteLine($"reads back as   : \"{run.Expected}\"");
        _output.WriteLine($"decoded         : {Quoted(run.Texts)}");
        _output.WriteLine($"decoded (lied)  : {Quoted(run.TextsUnresampled)}");
        _output.WriteLine($"came back       : {run.CameBack}");
        _output.WriteLine($"wall clock      : {run.WallSeconds:0.###} s");
    }

    /// <summary>A list of decodes, quoted, or the word nothing.</summary>
    private static string Quoted(IReadOnlyList<string> texts) =>
        texts.Count == 0 ? "nothing" : "\"" + string.Join("\", \"", texts) + "\"";
}
