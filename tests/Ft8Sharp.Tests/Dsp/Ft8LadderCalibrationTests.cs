using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>The instrument, proved before it is trusted.</b> A ladder is worth exactly as much as the
/// calibration under it, and a mis-scaled noise generator would produce a beautiful and entirely
/// fictional number.
/// </summary>
/// <remarks>
/// <para>
/// <b>Unit 214 proved this instrument once, at eight ratios, against a tolerance of 0.01 dB</b> —
/// <see cref="Ft8NoiseTests.TheRequestedSignalToNoiseRatioIsTheDeliveredOne"/>. What it did not do is
/// prove it <em>at the rungs unit 218 actually uses</em>, over a <em>single slot</em> rather than
/// twenty slots' worth of samples, which is the draw every trial of tonight's ladder gets. A finite
/// draw is not its own standard deviation, and the difference between requested and delivered over
/// 180 000 samples is the honest width of tonight's dB axis.
/// </para>
/// <para>
/// <b>The fourth test here is the single most important assertion in the unit.</b> A path that
/// manufactures messages out of noise would make every rung above it meaningless, and would violate
/// HM-DEC-009 outright. It must return nothing, and it is watched returning nothing at the noise
/// amplitudes tonight's ladder actually uses.
/// </para>
/// </remarks>
public class Ft8LadderCalibrationTests
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    private readonly ITestOutputHelper _output;

    public Ft8LadderCalibrationTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// <b>The convention, read out of the instrument rather than remembered, and then the whole
    /// ladder's requested-against-delivered printed before any bound is asserted on it.</b>
    /// </summary>
    [Fact]
    public void RequestedEqualsDeliveredAtEveryRungOfTonightsLadder()
    {
        var corpus = SensitivityLadder.Messages();
        var reference = corpus[0];
        var signalPower = SearchFixture.TransmissionPower(Rate, reference, 1000.0);
        var slotSamples = Ft8Waveform.SlotSampleCount(Rate);

        _output.WriteLine("THE CONVENTION, read out of SignalToNoise and stated in words:");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  A ratio needs a bandwidth to be a number. This project uses the amateur");
        _output.WriteLine("  weak-signal convention, the one the published FT8 figure near -21 dB is");
        _output.WriteLine("  quoted against:");
        _output.WriteLine(string.Empty);
        _output.WriteLine("    SNR(dB) = 10 log10( signal power / noise power in a "
            + $"{SignalToNoise.ReferenceBandwidthHz:F0} Hz reference bandwidth )");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  reference bandwidth : {SignalToNoise.ReferenceBandwidthHz:F0} Hz "
            + "(SignalToNoise.ReferenceBandwidthHz)");
        _output.WriteLine($"  sampled bandwidth   : {Rate / 2.0:F0} Hz, one-sided, real samples at {Rate} Hz");
        _output.WriteLine("  White noise of standard deviation sigma spreads sigma^2 evenly over");
        _output.WriteLine($"  0..{Rate / 2.0:F0} Hz, so the power it puts inside the reference bandwidth is");
        _output.WriteLine($"  sigma^2 * {SignalToNoise.ReferenceBandwidthHz:F0} / {Rate / 2.0:F0}"
            + $" = sigma^2 * {SignalToNoise.ReferenceBandwidthHz / (Rate / 2.0):F5}. Solving for sigma:");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"    sigma = sqrt( signalPower * ({Rate}/2) / "
            + $"({SignalToNoise.ReferenceBandwidthHz:F0} * 10^(snr/10)) )");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  The signal power is MEASURED from the samples that will be transmitted,");
        _output.WriteLine("  not assumed to be 0.5 for a sine: the FT8 waveform is constant-envelope");
        _output.WriteLine("  with raised-cosine ramps at its two ends.");
        _output.WriteLine($"    reference message   : {reference.Label}");
        _output.WriteLine($"    its mean square     : {signalPower:F6}");
        _output.WriteLine($"    slot length         : {slotSamples} samples, {slotSamples / (double)Rate:F2} s");
        _output.WriteLine(string.Empty);
        _output.WriteLine("REQUESTED AGAINST DELIVERED, over ONE SLOT'S DRAW, at every rung of");
        _output.WriteLine("tonight's ladder and at both seeds. THE DIFFERENCES ARE PRINTED FIRST.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"requested",10} {"seed",8} {"sigma",12} {"noise power",14} {"delivered",11} {"error",9}");

        var errors = new List<double>();

        foreach (var requested in SensitivityLadder.Rungs)
        {
            foreach (var seed in SensitivityLadder.Seeds)
            {
                var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, requested, Rate);
                var noise = new GaussianNoise(seed + (int)Math.Round(requested * 10));
                var drawn = noise.Block(slotSamples, sigma);
                var power = SignalToNoise.MeanSquare(drawn);
                var delivered = SignalToNoise.DecibelsFor(signalPower, power, Rate);

                errors.Add(Math.Abs(delivered - requested));
                _output.WriteLine($"{requested,10:F1} {seed,8} {sigma,12:F6} {power,14:E5} "
                    + $"{delivered,11:F4} {delivered - requested,9:F4}");
            }
        }

        var worst = errors.Max();
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  points measured                 : {errors.Count}");
        _output.WriteLine($"  WORST |requested - delivered|   : {worst:F4} dB over one slot's draw");
        _output.WriteLine($"  mean  |requested - delivered|   : {errors.Average():F4} dB");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  MEASURED FIRST, BOUNDED SECOND. The bound below is set from the number");
        _output.WriteLine("  above at 0.05 dB. What remains is the sampling error of estimating a");
        _output.WriteLine("  variance from a finite draw; it shrinks as the square root of the sample");
        _output.WriteLine("  count and it is not a bias. IT IS ALSO WHY EVERY LADDER IN THIS UNIT IS");
        _output.WriteLine("  BINNED BY THE DELIVERED RATIO AND NEVER BY THE REQUESTED ONE.");

        Assert.True(worst < 0.05, $"a rung missed its requested ratio by {worst:F4} dB.");
    }

    /// <summary>
    /// <b>The noise is noise: its mean, its variance against the sigma that was asked for, and the
    /// seed deciding the draw.</b> Determinism is what makes tonight's numbers re-runnable.
    /// </summary>
    [Fact]
    public void TheNoiseIsNoiseAndTheSeedDecidesTheDraw()
    {
        var slotSamples = Ft8Waveform.SlotSampleCount(Rate);

        _output.WriteLine("MEAN AND VARIANCE, one slot's draw at each of three amplitudes:");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"sigma asked",13} {"mean",12} {"variance",14} {"sigma^2 asked",15} {"ratio",8}");

        foreach (var sigma in new[] { 1.0, 0.1, 0.0125 })
        {
            var drawn = new GaussianNoise(218_101).Block(slotSamples, sigma);
            var mean = drawn.Select(s => (double)s).Average();
            var variance = SignalToNoise.MeanSquare(drawn) - (mean * mean);

            _output.WriteLine($"{sigma,13:F6} {mean,12:E3} {variance,14:E6} {sigma * sigma,15:E6} "
                + $"{variance / (sigma * sigma),8:F5}");

            Assert.True(Math.Abs(mean) < sigma * 0.02, $"the noise mean is {mean:E3} at sigma {sigma}.");
            Assert.InRange(variance / (sigma * sigma), 0.98, 1.02);
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("THE SEED DECIDES IT. Same seed, sample for sample; different seed, not.");

        var a = new GaussianNoise(218_102).Block(slotSamples, 1.0);
        var again = new GaussianNoise(218_102).Block(slotSamples, 1.0);
        var other = new GaussianNoise(218_103).Block(slotSamples, 1.0);

        var identical = 0;
        var differing = 0;
        for (var i = 0; i < slotSamples; i++)
        {
            if (a[i] == again[i])
            {
                identical++;
            }

            if (a[i] != other[i])
            {
                differing++;
            }
        }

        _output.WriteLine($"  seed 218102 twice        : {identical} of {slotSamples} samples IDENTICAL");
        _output.WriteLine($"  seed 218102 against 218103: {differing} of {slotSamples} samples DIFFERENT");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  Asserted on the SAMPLES, never on a count of them: two runs agreeing on");
        _output.WriteLine("  how many while disagreeing on which is exactly what a count hides.");

        Assert.Equal(slotSamples, identical);
        Assert.Equal(slotSamples, differing);
    }

    /// <summary>
    /// <b>THE SINGLE MOST IMPORTANT ASSERTION IN THIS UNIT. Pure noise, no signal in it at all,
    /// returns no messages.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>A path that manufactures messages out of noise would make every rung of tonight's ladder
    /// meaningless</b>, because a decode rate measured against a floor that is not zero is not a
    /// decode rate. It would also violate HM-DEC-009 outright.
    /// </para>
    /// <para>
    /// <b>The candidate, parity and text counts are reported too, and that is deliberate.</b> A path
    /// that finds candidates in noise and turns none of them into text is behaving <em>correctly</em>
    /// — the search is meant to be permissive and the gates are meant to be the ones that refuse — and
    /// it is worth showing that it does rather than only showing the zero at the end.
    /// </para>
    /// <para>
    /// <b>Unit 216 took this measurement once, over twenty slots at one amplitude.</b> This one takes
    /// it at the noise amplitudes tonight's ladder actually delivers, so the floor row of tonight's
    /// table is a run at tonight's levels rather than a borrowed number.
    /// </para>
    /// </remarks>
    [Fact]
    public void PureNoiseWithNoSignalInItReturnsNoMessages()
    {
        var corpus = SensitivityLadder.Messages();
        var signalPower = SearchFixture.TransmissionPower(Rate, corpus[0], 1000.0);
        var slotSamples = Ft8Waveform.SlotSampleCount(Rate);
        var decoder = new Ft8SlotDecoder();

        var seeds = new[] { 218_201, 218_202, 218_203, 218_204, 218_205, 218_206 };

        _output.WriteLine("PURE NOISE. NO SIGNAL. The slot is a Gaussian draw and nothing else.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"as if rung",11} {"seed",8} {"sigma",11} {"cand",6} {"par",5} {"crc",5} "
            + $"{"txt",5} {"MESSAGES",9}");

        long candidates = 0;
        long parity = 0;
        long checksum = 0;
        long text = 0;
        var messages = 0;
        var slots = 0;

        // The amplitudes are the ones the top, middle and bottom of tonight's ladder deliver, so
        // the floor is measured where the ladder actually stands rather than at one arbitrary level.
        foreach (var rung in new[] { -10.0, -18.0, -26.0 })
        {
            var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, rung, Rate);

            foreach (var seed in seeds)
            {
                var slot = new GaussianNoise(seed).Block(slotSamples, sigma);
                var result = decoder.Decode(slot);

                slots++;
                candidates += result.CandidateCount;
                parity += result.ParitySatisfiedCount;
                checksum += result.ChecksumPassedCount;
                text += result.BecameTextCount;
                messages += result.Messages.Count;

                _output.WriteLine($"{rung,11:F1} {seed,8} {sigma,11:F6} {result.CandidateCount,6} "
                    + $"{result.ParitySatisfiedCount,5} {result.ChecksumPassedCount,5} "
                    + $"{result.BecameTextCount,5} {result.Messages.Count,9}");

                foreach (var message in result.Messages)
                {
                    _output.WriteLine($"      A MESSAGE OUT OF NOISE: {message.Text}");
                }
            }
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  slots of pure noise decoded : {slots}");
        _output.WriteLine($"  candidates FOUND            : {candidates}");
        _output.WriteLine($"  reached parity              : {parity}");
        _output.WriteLine($"  passed the checksum         : {checksum}");
        _output.WriteLine($"  became text                 : {text}");
        _output.WriteLine($"  MESSAGES RETURNED           : {messages}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  Candidates found and no text is the path behaving CORRECTLY: the search");
        _output.WriteLine("  is permissive by design and the parity and CRC gates are what refuse.");
        _output.WriteLine("  HM-DEC-009 is the rule and this is the strongest test of it this phase");
        _output.WriteLine("  has taken on the whole path at tonight's noise levels.");

        Assert.Equal(0, messages);
        Assert.Equal(0, text);
    }
}
