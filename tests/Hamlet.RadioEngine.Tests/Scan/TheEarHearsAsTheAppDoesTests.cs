using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Scan;
using Hamlet.RadioEngine.Tests.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Scan;

/// <summary>
/// **THE SCAN'S EAR HEARS AS THE APP DOES** (work instruction 542, task 1, HM-DEC-246): a clean 20 WPM call at 12 dB through
/// the radio's filter with a 1 dB AGC overshoot, read by the app's chain (<see cref="CwChain"/>) and by the scan's ear.
/// </summary>
/// <remarks>
/// Synthetic audio from the bench's own keyer and filter; no recording and no scan catch is read.
/// </remarks>
public sealed class TheEarHearsAsTheAppDoesTests
{
    private const int Rate = 8000;
    private const double Pitch = 600;
    private const double Width = 500;
    private const string Call = "CQ CQ CQ DE W1AW W1AW W1AW K";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public TheEarHearsAsTheAppDoesTests(ITestOutputHelper output) => _output = output;

    /// <summary>What a chain made of the call.</summary>
    internal sealed record Hearing(int Marks, double Shape, string Text, bool Green);

    /// <summary>The bench's call: 20 WPM, 12 dB, a 1 dB AGC overshoot, through the 500 Hz filter at the pitch.</summary>
    internal static float[] TheCall(double pitch = Pitch)
        => NarrownessReadsTheFiltersBandTests.ThroughTheFilter(
            AHandIsReadAgainstItselfTests.Keyed(Call, _ => new AHandIsReadAgainstItselfTests.Sending(20, 0), 5421, db: 12, overshootDb: 1, pitch: pitch),
            Pitch, Width);

    /// <summary>The app's chain: <see cref="CwChain"/>, its passband the radio's pitch and filter, fed in 10 ms chunks.</summary>
    internal static Hearing ByTheApp(float[] samples)
    {
        using var chain = new CwChain(Rate, Pitch);
        var text = new System.Text.StringBuilder();
        var green = false;

        chain.Detector.SetPassband(Pitch, Width);
        chain.Decoder.Runs.CharacterRead += c => text.Append(c.Text);

        for (var at = 0; at + (Rate / 100) <= samples.Length; at += Rate / 100)
        {
            chain.Process(new AudioChunk(at, Rate, samples.AsSpan(at, Rate / 100)));
            green |= chain.Detector.Reading.ShapeLight is CwShapeLight.Found or CwShapeLight.Reading;
        }

        chain.Decoder.Flush();

        var reading = chain.Decoder.ShapeSide;

        return new Hearing(reading.Senders.Sum(s => s.Marks), reading.Senders.Select(s => s.ShapeScore).DefaultIfEmpty(0).Max(), text.ToString().Trim(), green);
    }

    /// <summary>The scan's ear, as the scan builds it, fed the same chunks.</summary>
    internal static Hearing ByTheEar(float[] samples)
    {
        var source = new Pushed();
        using var ear = new CwCatchEar(source, Pitch, Width);

        ear.Begin();

        for (var at = 0; at + (Rate / 100) <= samples.Length; at += Rate / 100)
        {
            source.Push(new AudioChunk(at, Rate, samples.AsSpan(at, Rate / 100)));
        }

        var green = ear.Sense().ShapeSeen;
        var heard = ear.End(null);

        return new Hearing(heard.Stations.Sum(s => s.Marks), heard.Stations.Select(s => s.ShapeScore).DefaultIfEmpty(0).Max(), heard.Text, green);
    }

    /// <remarks>
    /// Task 1: the app's chain and the scan's ear find the same marks, the same shape score and the same text, and the call
    /// reaches green.
    /// </remarks>
    [Fact]
    public void TheEarAndTheAppHearTheCallAlike()
    {
        var samples = TheCall();
        var app = ByTheApp(samples);
        var ear = ByTheEar(samples);

        _output.WriteLine($"app: {app.Marks} marks, shape {app.Shape:0.000}, green {app.Green}, `{app.Text}`");
        _output.WriteLine($"ear: {ear.Marks} marks, shape {ear.Shape:0.000}, green {ear.Green}, `{ear.Text}`");

        Assert.Equal(app.Marks, ear.Marks);
        Assert.Equal(app.Shape, ear.Shape, 6);
        Assert.Equal(app.Text, ear.Text);
        Assert.True(app.Green);
        Assert.True(ear.Green);
    }

    /// <summary>Band noise through the radio's filter, with a steady carrier at a pitch where one is asked for.</summary>
    private static float[] Band(double seconds, int seed, double? carrierHz)
    {
        var samples = RadioEngine.Training.CwSignal.Generate(new RadioEngine.Training.CwSignalRequest(
            " ", SampleRate: Rate, Amplitude: 0, NoiseAmplitude: 0.04, LeadInSeconds: seconds / 2, TailSeconds: seconds / 2, Seed: seed)).Samples;

        if (carrierHz is { } hz)
        {
            for (var i = 0; i < samples.Length; i++)
            {
                samples[i] += (float)(0.1 * Math.Sin(2 * Math.PI * hz * i / Rate));
            }
        }

        return NarrownessReadsTheFiltersBandTests.ThroughTheFilter(samples, Pitch, Width);
    }

    /// <summary>The scan's ear on some audio: how long its light stood at each state, and whether it ever went green.</summary>
    private static (double Amber, double Green, double Shape, string Text) Lit(float[] samples)
    {
        var source = new Pushed();
        using var ear = new CwCatchEar(source, Pitch, Width);

        ear.Begin();

        for (var at = 0; at + (Rate / 100) <= samples.Length; at += Rate / 100)
        {
            source.Push(new AudioChunk(at, Rate, samples.AsSpan(at, Rate / 100)));
        }

        var heard = ear.End(null);
        var end = samples.Length / (double)Rate;
        double amber = 0, green = 0;

        for (var i = 0; i < heard.Lights.Count; i++)
        {
            var until = i + 1 < heard.Lights.Count ? heard.Lights[i + 1].Seconds : end;
            var span = until - heard.Lights[i].Seconds;

            if (heard.Lights[i].Light == "shape forming")
            {
                amber += span;
            }
            else if (heard.Lights[i].Light is "shape found" or "reading")
            {
                green += span;
            }
        }

        return (amber, green, heard.Stations.Select(s => s.ShapeScore).DefaultIfEmpty(0).Max(), heard.Text);
    }

    /// <remarks>
    /// Task 4: a steady carrier at 750 Hz, 26 s, through the filter at five seeds of band noise, never reaches green and
    /// prints nothing. The first real scan's carrier scored 0.41.
    /// </remarks>
    [Theory]
    [InlineData(5421)]
    [InlineData(5422)]
    [InlineData(5423)]
    [InlineData(5424)]
    [InlineData(5425)]
    public void ASteadyCarrierNeverGoesGreen(int seed)
    {
        var (amber, green, shape, text) = Lit(Band(26, seed, 750));

        _output.WriteLine($"carrier at 750 Hz, seed {seed}: amber {amber:0.0} s, green {green:0.0} s, shape {shape:0.000}, `{text}`");

        Assert.Equal(0, green);
        Assert.Equal(string.Empty, text);
    }

    /// <remarks>
    /// Task 4: 26 s of band noise through the filter, at five seeds: how long the light stands amber is printed, and it never
    /// goes green. The first real scan held amber 9.8 s on noise.
    /// </remarks>
    [Theory]
    [InlineData(5421)]
    [InlineData(5422)]
    [InlineData(5423)]
    [InlineData(5424)]
    [InlineData(5425)]
    public void BandNoiseNeverGoesGreen(int seed)
    {
        var (amber, green, shape, text) = Lit(Band(26, seed, null));

        _output.WriteLine($"band noise, seed {seed}: amber {amber:0.0} s, green {green:0.0} s, shape {shape:0.000}, `{text}`");

        Assert.Equal(0, green);
        Assert.Equal(string.Empty, text);
    }

    private sealed class Pushed : IAudioSource
    {
        public string DeviceName => "bench";

        public int SampleRate => Rate;

        public bool IsSimulated => true;

        public bool IsRunning => true;

        public event AudioChunkHandler? SamplesReady;

        public void Push(in AudioChunk chunk) => SamplesReady?.Invoke(in chunk);

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Dispose()
        {
        }
    }
}
