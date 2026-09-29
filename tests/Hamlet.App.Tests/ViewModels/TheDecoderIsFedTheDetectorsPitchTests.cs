using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// What the view model feeds the decoder's second rung: the detector's pitch whenever it says
/// keying, and nothing otherwise (work instruction 488, HM-DEC-193).
/// </summary>
/// <remarks>Synthetic audio written here, nothing read from disk (R96).</remarks>
public sealed class TheDecoderIsFedTheDetectorsPitchTests
{
    /// <remarks>A detector reporting keying at 625 Hz feeds 625, a number and not NaN.</remarks>
    [Fact]
    public void KeyingAt625FeedsANumber()
    {
        var reading = CwEnvelopeReading.None with { Keying = true, PitchHz = 625 };

        Assert.Equal(625, MainWindowViewModel.PitchForTheDecoder(reading));
    }

    /// <remarks>Nobody keying feeds nothing, whatever pitch the reading still names.</remarks>
    [Fact]
    public void NotKeyingFeedsNothing()
    {
        var reading = CwEnvelopeReading.None with { Keying = false, PitchHz = 625 };

        Assert.True(double.IsNaN(MainWindowViewModel.PitchForTheDecoder(reading)));
    }

    /// <remarks>
    /// Driven with a keyed tone at 625 Hz and 23 words a minute: on every reading that says keying,
    /// gaps included, the rung is fed a number. Red before work instruction 488, when four keying
    /// readings in five carried no pitch.
    /// </remarks>
    [Fact]
    public void EveryKeyingReadingFeedsANumber()
    {
        const int rate = 8000;
        var audio = CwSignal.Generate(new CwSignalRequest(
            "CQ CQ DE N0CALL N0CALL K", WordsPerMinute: 23, ToneHz: 625, SampleRate: rate, Amplitude: 0.5,
            NoiseAmplitude: 0.04, LeadInSeconds: 3, TailSeconds: 3, Seed: 488));
        var detector = new CwEnvelopeDetector(rate);
        int keying = 0, fedNothing = 0;

        for (var at = 0; at + 80 <= audio.Samples.Length; at += 80)
        {
            detector.Process(audio.Samples.AsSpan(at, 80));

            if (!detector.Reading.Keying)
            {
                continue;
            }

            keying++;

            if (!double.IsFinite(MainWindowViewModel.PitchForTheDecoder(detector.Reading)))
            {
                fedNothing++;
            }
        }

        Assert.True(keying > 0, "the detector never said keying");
        Assert.Equal(0, fedNothing);
    }
}
