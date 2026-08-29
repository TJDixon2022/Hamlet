using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The spectral peak measures a pitch, on the operator's own captures, and it
/// claims nothing else.
/// </summary>
/// <remarks>
/// <para>**THE TRACKER SITS MORE THAN A HUNDRED HERTZ FROM THE STATION ON A
/// THIRD OF THE SCORED CORPUS** (work instruction 050, task 3). Measured: 300 and
/// 325 hertz on recordings whose station is at 500, and 650 twice on two more.
/// The peak lands within a hertz and a half of the strongest keyed bin on every
/// capture in the corpus, and fed to the decoder it takes corpus precision from
/// 0.766 to 0.858.</para>
/// <para>**AND IT IS NOT AN ADMISSION TEST.** A magnitude peak exists in any
/// recording, noise included, so nothing here says a station is present and
/// nothing in the decoder reads it that way (HM-DEC-095, HM-DEC-120). The bench
/// this came from has no refusal and emits text from noise; that is the one thing
/// Hamlet does not copy.</para>
/// </remarks>
public sealed class ThePeakFindsThePitchTheTrackerMissedTests
{
    private readonly ITestOutputHelper _output;

    public ThePeakFindsThePitchTheTrackerMissedTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>A generated tone is found to within a hertz.</summary>
    /// <remarks>
    /// **A TONE OFF THE BIN CENTRE ON PURPOSE.** Without the parabolic
    /// interpolation the answer would quantise to the bin spacing, and a test
    /// that only ever asked for a bin centre could not tell whether the
    /// interpolation was there.
    /// </remarks>
    [Theory]
    [InlineData(400.4)]
    [InlineData(501.2)]
    [InlineData(613.7)]
    [InlineData(801.3)]
    public void AGeneratedToneIsFoundToWithinAHertz(double toneHz)
    {
        var audio = CwSignal.Generate(new CwSignalRequest(
            "CQ CQ DE W1AW K", WordsPerMinute: 18,
            ToneHz: toneHz, NoiseAmplitude: 0.05));

        var found = CwSpectralPeak.Find(audio.Samples, audio.SampleRate);

        Assert.NotNull(found);

        _output.WriteLine($"sent {toneHz:0.0} Hz, found {found:0.0} Hz");

        Assert.True(
            Math.Abs(found!.Value - toneHz) < 1.0,
            $"sent {toneHz:0.0} Hz and found {found:0.0}");
    }

    /// <summary>
    /// On every capture in the scored corpus the peak agrees with the strongest
    /// keyed bin.
    /// </summary>
    /// <remarks>
    /// <para>**`KeyingEnvelope` IS THE REFEREE AND SHARES NO CODE WITH EITHER**
    /// (HM-DEC-091, §12.5). It asks a different question — where is somebody
    /// keying — and a measurement taken with the instrument under test cannot
    /// referee it.</para>
    /// <para>Twelve and a half hertz is half the tracker's own bin spacing, so
    /// agreement inside it means the two would choose the same bin.</para>
    /// </remarks>
    [Theory]
    [InlineData("captured/cw-2026-08-17-134712.wav")]
    [InlineData("captured/cw-2026-08-18-004507.wav")]
    [InlineData("captured/unadjudicated/cw-2026-08-22-031905.wav")]
    [InlineData("captured/unadjudicated/cw-2026-08-22-032050.wav")]
    [InlineData("captured/unadjudicated/cw-2026-08-22-032113.wav")]
    [InlineData("captured/unadjudicated/cw-2026-08-22-032129.wav")]
    public void ThePeakAgreesWithTheKeyedBin(string relative)
    {
        var audio = WavAudio.Read(
            Path.Combine(CwFixtures.Folder, relative.Replace('/', Path.DirectorySeparatorChar)));

        var peak = CwSpectralPeak.Find(audio.Samples, audio.SampleRate);
        var keyed = KeyingEnvelope.Best(audio);

        Assert.NotNull(peak);
        Assert.NotNull(keyed);

        _output.WriteLine(
            $"peak {peak:0.0} Hz, keyed bin {keyed!.Value.ToneHz:0.0} Hz");

        Assert.True(
            Math.Abs(peak!.Value - keyed.Value.ToneHz) < 12.5,
            $"peak {peak:0.0} Hz against a keyed bin at {keyed.Value.ToneHz:0.0}");
    }

    /// <summary>Not enough audio to transform once says so rather than guessing.</summary>
    /// <remarks>
    /// Null is "nobody measured", which is a different thing from "nothing is
    /// there" (§0.0). A number returned from half a window would be a reading
    /// with nothing behind it.
    /// </remarks>
    [Fact]
    public void TooLittleAudioReturnsNothing()
    {
        var samples = new float[CwSpectralPeak.Window - 1];

        Assert.Null(CwSpectralPeak.Find(samples, 8_000));
    }

    /// <summary>Noise still yields a peak, and that is why it decides nothing.</summary>
    /// <remarks>
    /// **THE POINT OF THIS TEST IS THAT IT PASSES.** A peak in noise is not a
    /// fault to be fixed here — every spectrum has a largest bin. It is the
    /// reason this type may never be read as evidence that somebody is keying,
    /// and the reason the bench that inspired it emits text from an empty band.
    /// </remarks>
    [Fact]
    public void NoiseHasAPeakToo()
    {
        var random = new Random(20260829);
        var samples = new float[8_000 * 10];

        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = (float)((random.NextDouble() * 2) - 1) * 0.1f;
        }

        var found = CwSpectralPeak.Find(samples, 8_000);

        _output.WriteLine($"noise peaked at {found:0.0} Hz, which means nothing");

        Assert.NotNull(found);
    }
}
