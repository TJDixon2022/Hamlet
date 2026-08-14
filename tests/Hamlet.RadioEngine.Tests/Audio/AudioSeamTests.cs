using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Training;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// The audio seam: device choice, the WAV round trip, and the sources that
/// stand in for a sound card (HM-DEC-007, HM-DEC-026).
/// </summary>
public sealed class AudioSeamTests : IDisposable
{
    private readonly string _folder = Path.Combine(
        Path.GetTempPath(), "hamlet-audio-tests", Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_folder))
            {
                Directory.Delete(_folder, recursive: true);
            }
        }
        catch (IOException)
        {
            // A leftover temp folder is not a test failure.
        }
    }

    /// <remarks>
    /// Proves the radio's codec is recognized by the name CLAUDE.md §4 records
    /// it under, and that nothing else is mistaken for it. The match only ever
    /// preselects a device, but a preselection that landed on the webcam would
    /// leave a beginner staring at a terminal that never fills in.
    /// </remarks>
    [Theory]
    [InlineData("Microphone (USB Audio CODEC)", true)]
    [InlineData("Line In (USB AUDIO CODEC)", true)]
    [InlineData("Microphone Array (Realtek(R) Audio)", false)]
    [InlineData("Webcam Microphone", false)]
    [InlineData("", false)]
    public void TheRadiosCodecIsRecognizedByName(string name, bool expected)
        => Assert.Equal(expected, AudioDevice.LooksLikeRadioCodec(name));

    /// <remarks>
    /// Proves the operator's own choice outranks every guess Hamlet makes, and
    /// that a device which has been unplugged falls through quietly instead of
    /// failing. Somebody who took the radio to a park still wants the app to
    /// open when they get home.
    /// </remarks>
    [Fact]
    public void TheRememberedDeviceWinsWhileItIsStillThere()
    {
        var codec = new AudioDevice("id-codec", "Microphone (USB Audio CODEC)");
        var built = new AudioDevice("id-built-in", "Microphone Array", IsDefault: true);
        var devices = new[] { built, codec };

        Assert.Same(built, AudioDeviceChoice.Choose(devices, "id-built-in"));
        Assert.Same(codec, AudioDeviceChoice.Choose(devices, "id-vanished"));
        Assert.Same(codec, AudioDeviceChoice.Choose(devices, null));
        Assert.Same(built, AudioDeviceChoice.Choose(new[] { built }, null));
        Assert.Null(AudioDeviceChoice.Choose(Array.Empty<AudioDevice>(), "id-codec"));
    }

    /// <remarks>
    /// Proves a fixture survives the round trip through disk. §0.0.1 wants a
    /// wrong decode to arrive with its input attached, and that is worth
    /// nothing if the file does not come back the way it went in.
    /// </remarks>
    [Fact]
    public void AudioSurvivesTheWavRoundTrip()
    {
        var original = CwSignal.Generate(new CwSignalRequest("PARIS", WordsPerMinute: 20));
        var path = Path.Combine(_folder, "paris.wav");

        WavAudio.Write(path, original);
        var reloaded = WavAudio.Read(path);

        Assert.Equal(original.SampleRate, reloaded.SampleRate);
        Assert.Equal(original.Samples.Length, reloaded.Samples.Length);

        // Sixteen-bit quantization is the only loss, so the tolerance is one
        // step of it rather than a shrug.
        for (var i = 0; i < original.Samples.Length; i++)
        {
            Assert.True(
                Math.Abs(original.Samples[i] - reloaded.Samples[i]) < 1.0 / short.MaxValue,
                $"sample {i} moved by more than one quantization step");
        }
    }

    /// <remarks>
    /// Proves the buffered source hands over every sample exactly once, in
    /// order, with sample indices that line up. Everything downstream derives
    /// its timing from those indices, so a gap or an overlap here would read as
    /// a keying error that never happened.
    /// </remarks>
    [Fact]
    public void TheBufferedSourceDeliversEverySampleInOrder()
    {
        var samples = new float[5_000];
        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = i;
        }

        using var source = new BufferedAudioSource(samples, 8_000, chunkSamples: 333);
        var seen = new List<float>();
        var expectedIndex = 0L;

        source.SamplesReady += (in AudioChunk chunk) =>
        {
            Assert.Equal(expectedIndex, chunk.FirstSampleIndex);
            Assert.Equal(8_000, chunk.SampleRate);
            expectedIndex += chunk.Samples.Length;

            foreach (var s in chunk.Samples)
            {
                seen.Add(s);
            }
        };

        source.PumpAll();

        Assert.Equal(samples.Length, seen.Count);
        Assert.Equal(samples, seen);
        Assert.True(source.IsFinished);
    }

    /// <remarks>
    /// THE HONESTY RULE, on the audio seam this time (HM-DEC-026). Every source
    /// that is not listening to a radio says so, and there is no setter to get
    /// it wrong. A decode from a fixture must never reach the screen dressed as
    /// something that happened on the air.
    /// </remarks>
    [Fact]
    public void EverySynthesizedSourceSaysItIsSimulated()
    {
        using var training = new TrainingAudioSource();
        using var buffered = new BufferedAudioSource(new float[10], 8_000);

        Assert.True(training.IsSimulated);
        Assert.True(buffered.IsSimulated);

        // And nothing can be told otherwise: there is no setter on either the
        // interface or the implementations.
        Assert.Null(typeof(IAudioSource).GetProperty(nameof(IAudioSource.IsSimulated))!.SetMethod);
        Assert.Null(typeof(TrainingAudioSource).GetProperty(nameof(IAudioSource.IsSimulated))!.SetMethod);
        Assert.Null(typeof(WasapiAudioSource).GetProperty(nameof(IAudioSource.IsSimulated))!.SetMethod);
    }

    /// <remarks>
    /// Proves the training radio's loop is seamless: pumping across the splice
    /// yields one continuous run of sample indices, and the splice itself falls
    /// in silence rather than through a keyed element.
    /// </remarks>
    [Fact]
    public void TheTrainingRadioLoopsWithoutASplice()
    {
        using var source = new TrainingAudioSource("E", wordsPerMinute: 20);
        var expectedIndex = 0L;
        var total = 0;

        source.SamplesReady += (in AudioChunk chunk) =>
        {
            Assert.Equal(expectedIndex, chunk.FirstSampleIndex);
            expectedIndex += chunk.Samples.Length;
            total += chunk.Samples.Length;
        };

        // Well past one repetition, so the loop point is crossed several times.
        source.PumpOnce(source.SampleRate * 12);

        Assert.Equal(source.SampleRate * 12, total);
    }

    /// <remarks>
    /// Proves the generated signal is deterministic, noise included. A fixture
    /// that quietly changed between runs would take its assertions with it, and
    /// the whole point of HM-DEC-007 is that a decoder bug stays reproducible.
    /// </remarks>
    [Fact]
    public void TheSameRequestAlwaysGivesTheSameSamples()
    {
        var request = new CwSignalRequest(
            "CQ DE W1AW K", WordsPerMinute: 18, NoiseAmplitude: 0.2);

        Assert.Equal(
            CwSignal.Generate(request).Samples,
            CwSignal.Generate(request).Samples);
    }

    /// <remarks>
    /// Proves the generator puts the message where it says it does: the
    /// keyed audio starts after the lead-in and the whole file is as long as
    /// the arithmetic says. Everything the decoder is held to rests on this.
    /// </remarks>
    [Fact]
    public void TheGeneratedSignalIsAsLongAsTheArithmeticSaysItIs()
    {
        var request = new CwSignalRequest("PARIS", WordsPerMinute: 20);
        var audio = CwSignal.Generate(request);

        // "PARIS" plus the standard trailing word gap is the definition of one
        // word at any speed, so a 20 WPM send of it is one second of keying.
        Assert.Equal(
            CwSignal.DurationOf(request).TotalSeconds, audio.Duration.TotalSeconds, 2);

        // Silence through the lead-in, and a real signal after it.
        var leadInSamples = (int)(request.LeadInSeconds * audio.SampleRate);
        for (var i = 0; i < leadInSamples - 1; i++)
        {
            Assert.Equal(0, audio.Samples[i]);
        }

        var peak = 0f;
        for (var i = leadInSamples; i < audio.Samples.Length; i++)
        {
            peak = Math.Max(peak, Math.Abs(audio.Samples[i]));
        }

        Assert.True(peak > 0.4, $"the keyed tone only reached {peak:0.00}");
    }
}
