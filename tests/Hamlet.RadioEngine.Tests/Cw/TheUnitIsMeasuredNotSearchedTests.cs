using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The sender's dit is measured from the audio rather than searched for.
/// </summary>
/// <remarks>
/// <para>**THE SEARCH COULD NOT TELL THE SPEEDS APART.** Measured on
/// `cw-2026-08-18-004507`, the likelihood across the whole grid from eleven words
/// a minute to thirty-two spans 0.05 out of 33, so the winner was decided in the
/// fourth significant figure and changed with the window. The same information is
/// sitting in two medians.</para>
/// <para>**THE MECHANISM IS THAT THE BIAS CANCELS.** Any level the envelope is cut
/// at catches the skirts of every mark, so a mark reads long and the gap beside it
/// reads short by the same amount. Their average is the dit.</para>
/// </remarks>
public sealed class TheUnitIsMeasuredNotSearchedTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public TheUnitIsMeasuredNotSearchedTests(ITestOutputHelper output)
        => _output = output;

    private static double[] EnvelopeOf(MonoAudio audio, double toneHz)
        => CwProbabilisticDecoder.Envelope(audio.Samples, audio.SampleRate, toneHz);

    /// <remarks>
    /// <para>Proves it against truth: audio generated at a known speed, read back
    /// to within a few per cent, at three speeds and two noise levels.</para>
    /// <para>**AND IT PROVES THE CANCELLATION RATHER THAN THE ANSWER.** The mark
    /// alone and the gap alone are both wrong, in opposite directions, by roughly
    /// the same amount, which is the whole reason the average works.</para>
    /// </remarks>
    /// <param name="wordsPerMinute">What the audio was generated at.</param>
    [Theory]
    [InlineData(12)]
    [InlineData(18)]
    [InlineData(25)]
    public void ItRecoversASpeedItWasNeverTold(int wordsPerMinute)
    {
        foreach (var noise in new[] { 0.02, 0.08 })
        {
            var audio = CwSignal.Generate(new CwSignalRequest(
                "CQ CQ DE W1AW W1AW K",
                WordsPerMinute: wordsPerMinute,
                ToneHz: 600,
                Amplitude: 0.5,
                NoiseAmplitude: noise,
                Seed: 11));

            var reading = CwUnitEstimator.Measure(
                EnvelopeOf(audio, 600), CwProbabilisticDecoder.HopMilliseconds);

            _output.WriteLine(
                $"true {wordsPerMinute} wpm, noise {noise:0.00}: "
                + $"measured {reading.WordsPerMinute:0.0} wpm, "
                + $"mark alone {1200.0 / reading.DitMarkMilliseconds:0.0}, "
                + $"gap alone {1200.0 / reading.ElementGapMilliseconds:0.0}");

            Assert.True(reading.IsReady, "nothing was measured at all");

            Assert.InRange(
                reading.WordsPerMinute, wordsPerMinute * 0.9, wordsPerMinute * 1.1);

            // The two halves are wrong in opposite directions. That is the
            // mechanism, and if it ever stops being true the average is a
            // coincidence rather than a measurement.
            Assert.True(
                reading.DitMarkMilliseconds > 1200.0 / wordsPerMinute,
                "the mark did not read long");

            Assert.True(
                reading.ElementGapMilliseconds < 1200.0 / wordsPerMinute,
                "the gap did not read short");
        }
    }

    /// <remarks>
    /// <para>Proves the hysteresis is a mechanism rather than a tuned number:
    /// **the mark count is flat from five decibels to eight** on a real capture,
    /// and rises steeply below four as the envelope crosses one level back and
    /// forth on every edge.</para>
    /// <para>Measured on `cw-2026-08-18-004507`: 213 marks at one decibel, 162 at
    /// three, and 126, 121 and 116 at five, six and eight, against an independent
    /// count near 125 in thirty seconds.</para>
    /// </remarks>
    [Fact]
    public void TheFiveToEightDecibelPlateauHolds()
    {
        var audio = WavAudio.Read(Path.Combine(
            CapturedSignalTests.Folder, "cw-2026-08-18-004507.wav"));

        var envelope = EnvelopeOf(audio, 500);
        var counts = new Dictionary<double, int>();

        foreach (var depth in new[] { 1.0, 3, 5, 6, 8 })
        {
            var (marks, _) = CwUnitEstimator.Elements(
                envelope, CwProbabilisticDecoder.HopMilliseconds, depth);

            counts[depth] = marks.Count;

            _output.WriteLine($"{depth:0} dB: {marks.Count} marks");
        }

        // Flat where it is used: nothing across five to eight moves by more than
        // a tenth of the count at six.
        var middle = counts[6];

        foreach (var depth in new[] { 5.0, 6, 8 })
        {
            Assert.True(
                Math.Abs(counts[depth] - middle) <= middle * 0.15,
                $"{depth:0} dB gave {counts[depth]} against {middle} at six, "
                + "so the plateau this depth was chosen from is gone");
        }

        // And steep below it, which is what makes a single level unusable.
        Assert.True(
            counts[1] > middle * 1.5,
            $"one decibel gave {counts[1]} against {middle}, so the chatter a "
            + "two-level trigger exists to remove is no longer there");
    }

    /// <remarks>
    /// Proves the estimator says nothing rather than guessing when the window
    /// holds too little keying to cluster (§0.0, HM-DEC-120).
    /// </remarks>
    [Fact]
    public void ItSaysNothingWhenThereIsNothingToMeasure()
    {
        var audio = WavAudio.Read(Path.Combine(
            CapturedSignalTests.Folder, "unadjudicated", "cw-2026-08-20-014854.wav"));

        var envelope = EnvelopeOf(audio, 600);
        var short_ = CwUnitEstimator.Measure(
            envelope.Take(8).ToArray(), CwProbabilisticDecoder.HopMilliseconds);

        _output.WriteLine($"eight hops: ready {short_.IsReady}");

        Assert.False(short_.IsReady);
    }
}
