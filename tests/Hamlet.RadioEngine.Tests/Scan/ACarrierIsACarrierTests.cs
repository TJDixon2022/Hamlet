using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Scan;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Scan;

/// <summary>
/// **A CARRIER IS A CARRIER** (work instruction 544, task 3, HM-DEC-248): a steady tone, one that never keys, is told from
/// a station by the share of its time key-up, through the scan's own ear.
/// </summary>
/// <remarks>
/// R88 is lifted for the scan catches work instruction 544 names; this reads the carrier in the tree,
/// `catch-154614-7047190`, which the scan called negative at shape 0.45. The one called positive, `catch-143718-7007212`,
/// is not in the tree; a synthetic unkeyed carrier held 78 s stands in for it.
/// </remarks>
public sealed class ACarrierIsACarrierTests
{
    private const int Rate = 8000;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the measurements are printed.</param>
    public ACarrierIsACarrierTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// The scan's ear on some audio, as the scan feeds it: from <see cref="CwCatchScan.CarrierListen"/> on, it judges the tone
    /// once, as the stay does, and where it is a carrier the catch ends there. The tone found in the passband, its share key-up,
    /// the text printed by the time the catch ended, whether a shape was seen, and when it ended.
    /// </summary>
    private static (double? ToneHz, double? KeyUp, string Text, bool ShapeSeen, double EndedAt) Ear(float[] samples, int rate)
    {
        var source = new TheCatchScanTests.FakeAudio();
        using var ear = new CwCatchEar(source, 600, 500);

        ear.Begin();

        var chunk = rate / 100;
        double? tone = null;
        double? keyUp = null;
        var judged = false;
        var endedAt = samples.Length / (double)rate;

        for (var at = 0; at + chunk <= samples.Length; at += chunk)
        {
            source.Push(new AudioChunk(at, rate, samples.AsSpan(at, chunk).ToArray()));
            ear.CatchUpForTests();

            if (!judged && at >= CwCatchScan.CarrierListen.TotalSeconds * rate)
            {
                judged = true;
                tone = ear.Tone(CwCatchScan.CarrierListen.TotalSeconds, 350, 850)?.Hz;
                keyUp = tone is { } hz ? ear.KeyUpShare(hz) : null;

                if (keyUp < CwCatchScan.CarrierKeyUpShare)
                {
                    endedAt = at / (double)rate;
                    break;
                }
            }
        }

        var shape = ear.Sense().ShapeSeen;
        var heard = ear.End(null);

        return (tone, keyUp, heard.Text, shape, endedAt);
    }

    /// <remarks>
    /// **Must not:** the carrier in the tree is a carrier. Its tone is heard, its share key-up is under
    /// <see cref="CwCatchScan.CarrierKeyUpShare"/>, so the scan calls it `carrier` rather than positive or negative, and the
    /// ear prints nothing.
    /// </remarks>
    [Fact]
    public void TheCarrierInTheTreeIsACarrier()
    {
        var audio = WavAudio.Read(TheStrongStationsOverTests.Wav(TheStrongStationsOverTests.Carrier));
        var (tone, keyUp, text, shape, endedAt) = Ear(audio.Samples, audio.SampleRate);

        _output.WriteLine($"{TheStrongStationsOverTests.Carrier}: tone {tone:0.0} Hz, key-up {keyUp:0.000}, shape seen {shape}, printed `{text}`, the catch ended at {endedAt:0.0} s");

        Assert.NotNull(tone);
        Assert.InRange(keyUp!.Value, 0, CwCatchScan.CarrierKeyUpShare);
        Assert.Equal(string.Empty, text);
    }

    /// <remarks>
    /// **Must not:** a synthetic unkeyed carrier held 78 s over band noise, standing in for the one the scan called positive,
    /// steady and fading 6 dB over ten seconds, is a carrier and prints nothing; a station keyed at 20 WPM at the same level is
    /// not a carrier.
    /// </remarks>
    [Theory]
    [InlineData("steady", 0.0)]
    [InlineData("fading 6 dB", 6.0)]
    public void ASyntheticCarrierHeld78SecondsIsACarrier(string what, double fadeDb)
    {
        var noise = CwSignal.Generate(new CwSignalRequest(
            " ", SampleRate: Rate, Amplitude: 0, NoiseAmplitude: 0.04, LeadInSeconds: 39, TailSeconds: 39, Seed: 5441)).Samples;
        var samples = new float[noise.Length];

        for (var i = 0; i < samples.Length; i++)
        {
            var t = i / (double)Rate;
            var gain = Math.Pow(10, -fadeDb * (0.5 + (0.5 * Math.Sin(2 * Math.PI * t / 10))) / 20);

            samples[i] = noise[i] + (float)(0.1 * gain * Math.Sin(2 * Math.PI * 640 * t));
        }

        var (tone, keyUp, text, shape, endedAt) = Ear(samples, Rate);

        _output.WriteLine($"carrier {what}, 78 s: tone {tone:0.0} Hz, key-up {keyUp:0.000}, shape seen {shape}, printed `{text}`, the catch ended at {endedAt:0.0} s");

        Assert.NotNull(tone);
        Assert.InRange(keyUp!.Value, 0, CwCatchScan.CarrierKeyUpShare);
        Assert.Equal(string.Empty, text);
    }

    /// <remarks>A station keyed at 20 WPM over the same noise, at the carrier's level, is key-up far more than a carrier.</remarks>
    [Fact]
    public void AKeyedStationIsNotACarrier()
    {
        var keyed = CwSignal.Generate(new CwSignalRequest(
            "CQ CQ CQ DE W1AW W1AW W1AW K CQ CQ CQ DE W1AW W1AW W1AW K", WordsPerMinute: 20, ToneHz: 640, SampleRate: Rate, Amplitude: 0.1,
            NoiseAmplitude: 0.04, LeadInSeconds: 0.5, TailSeconds: 0.5, Seed: 5442)).Samples;
        var (tone, keyUp, text, _, endedAt) = Ear(keyed, Rate);

        _output.WriteLine($"keyed station: tone {tone:0.0} Hz, key-up {keyUp:0.000}, printed `{text}`, the catch ended at {endedAt:0.0} s");

        Assert.True(keyUp > CwCatchScan.CarrierKeyUpShare, $"key-up {keyUp}");
    }
}
