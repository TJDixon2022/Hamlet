using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>Unit 222 task 2: is the ruler right?</b> The whole of step 6's verdict rests on the claim that
/// the slot handed to the decoder carried -21.001 dB, and <b>no unit has ever checked that claim with
/// an instrument other than the one that made it.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>Why unit 221's 0.05 dB agreement is not this check.</b> That measurement compared the ratio
/// requested against the ratio delivered, and <em>both sides of it come from
/// <see cref="SignalToNoise"/></em> — one call sets the noise amplitude and the other reads the ratio
/// back, through the same two lines of arithmetic. A systematic convention error is invisible to it,
/// and a convention error is exactly the size of the discrepancy being chased: a one-sided against a
/// two-sided noise density is <b>3.01 dB</b>, the sampled bandwidth taken for the reference bandwidth
/// is <b>3.80 dB</b> at 12 kHz, and the signal averaged over the slot rather than over the
/// transmission is <b>0.74 dB</b> — against a shortfall of 1.5.
/// </para>
/// <para>
/// <b>The second reading shares no line of code with the first.</b> It never calls
/// <see cref="SignalToNoise"/> and never calls <see cref="SearchFixture"/>'s power helpers. It takes
/// the samples, transforms them, and sums power per hertz. The only thing the two readings have in
/// common is the audio itself, which is the point.
/// </para>
/// <para>
/// <b>Both readings and their difference are printed before any bound is asserted</b> — unit 212's
/// rule, and the rule that has caught every instrument defect in this phase.
/// </para>
/// <para>
/// <b>Nothing under <c>src/Ft8Sharp/</c> is changed by this file.</b> The axis is measurement
/// apparatus and lives entirely under <c>tests/</c>.
/// </para>
/// </remarks>
public class Unit222AxisTests
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    /// <summary>The reference bandwidth the published figure is quoted in.</summary>
    private const double ReferenceBandwidthHz = 2500.0;

    /// <summary>
    /// The analysis length for the second reading. <b>Chosen for the transform and not for the
    /// answer</b>: a power of two, well under the shortest span measured, giving 2.93 Hz bins.
    /// </summary>
    private const int SegmentLength = 4096;

    /// <summary>
    /// The bound the axis is judged against, fixed by the instruction before this ran: <b>agree
    /// within 0.2 dB and the axis is sound.</b>
    /// </summary>
    private const double AxisSoundWithinDecibels = 0.2;

    private readonly ITestOutputHelper _output;

    public Unit222AxisTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// <b>Task 2a and 2b: the convention in force, read off the tree, and a second reading built
    /// from the samples.</b>
    /// </summary>
    [Fact]
    public void TheDecibelAxisIsCheckedAgainstASecondReadingThatSharesNoCodeWithIt()
    {
        var population = Ft8Step6Ladder.Population();

        _output.WriteLine("UNIT 222 TASK 2 - THE DECIBEL AXIS, CHECKED AGAINST A SECOND INSTRUMENT.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("2a. THE CONVENTION IN FORCE, READ OFF THE TREE AND NOT OFF MEMORY.");
        _output.WriteLine("    SignalToNoise.cs and SearchFixture.cs, one line each:");
        _output.WriteLine(string.Empty);

        var slotSamples = Ft8Waveform.SlotSampleCount(Rate);
        var probeSymbols = Ft8SymbolEncoder.Encode(population[0].Message);
        var transmissionSamples = Ft8Waveform.Synthesize(probeSymbols, Rate, 1000.0f).Length;

        _output.WriteLine($"  BANDWIDTH THE RATIO IS QUOTED IN : {ReferenceBandwidthHz:F0} Hz "
            + "(SignalToNoise.ReferenceBandwidthHz)");
        _output.WriteLine($"  SIGNAL POWER AVERAGED OVER       : the transmission's OWN samples, "
            + $"{transmissionSamples} of them");
        _output.WriteLine($"                                     ({transmissionSamples / (double)Rate:F2} s), "
            + "by SearchFixture.TransmissionPower, which synthesizes");
        _output.WriteLine("                                     the waveform alone and takes its mean square");
        _output.WriteLine($"  NOISE POWER MEASURED OVER        : the WHOLE SLOT, {slotSamples} samples "
            + $"({slotSamples / (double)Rate:F2} s),");
        _output.WriteLine("                                     by SearchFixture.AddNoise, which draws the "
            + "block and");
        _output.WriteLine("                                     measures the mean square it actually delivered");
        _output.WriteLine($"  NOISE DENSITY IS                 : ONE-SIDED. sigma^2 is spread over 0 to "
            + $"fs/2 = {Rate / 2} Hz,");
        _output.WriteLine("                                     so the power inside the reference band is "
            + "sigma^2 * 2500 / 6000");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  SO THE LADDER'S ARITHMETIC IS, IN FULL:");
        _output.WriteLine("     SNR(dB) = 10 log10( meanSquare(transmission) / "
            + "(meanSquare(noise over slot) * 2500 / 6000) )");
        _output.WriteLine(string.Empty);

        _output.WriteLine("2c. THE CANDIDATES FOR A DIFFERENCE, NAMED WITH THEIR SIZES BEFORE LOOKING:");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  signal over the whole slot rather than the transmission : 0.74 dB");
        _output.WriteLine("  a one-sided against a two-sided noise density           : 3.01 dB");
        _output.WriteLine("  noise over the transmission rather than the slot        : a fraction of a dB");
        _output.WriteLine("  the reference bandwidth taken as the sampled bandwidth  : 3.80 dB at 12 kHz");
        _output.WriteLine(string.Empty);

        _output.WriteLine("2b. THE SECOND READING. Built from the samples themselves: the noise power");
        _output.WriteLine($"    inside {ReferenceBandwidthHz:F0} Hz by periodogram of a NOISE-ONLY slot, and the");
        _output.WriteLine("    signal power of the CLEAN transmission. It never calls SignalToNoise and");
        _output.WriteLine("    never calls SearchFixture's power helpers.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  analysis length : {SegmentLength} samples, "
            + $"{(double)Rate / SegmentLength:F3} Hz per bin, rectangular");
        _output.WriteLine($"  segments        : {slotSamples / SegmentLength} whole ones in a slot, "
            + $"{transmissionSamples / SegmentLength} in a transmission, averaged");
        _output.WriteLine(string.Empty);

        // THE SECOND INSTRUMENT IS PROVED BEFORE IT IS TRUSTED. If the periodogram's bins do not sum
        // to the mean square of the samples they came from, every band power below is wrong by an
        // unknown factor and nothing after this line means anything.
        var proofNoise = new GaussianNoise(222_100).Block(SegmentLength * 8, 0.37);
        var proofBins = PowerPerHertz(proofNoise, 0, SegmentLength * 8);
        var proofTotal = proofBins.Sum() * ((double)Rate / SegmentLength);
        var proofDirect = DirectMeanSquare(proofNoise);

        _output.WriteLine("  THE SECOND INSTRUMENT, PROVED BEFORE IT IS TRUSTED - Parseval, on a block");
        _output.WriteLine("  of noise the axis never sees:");
        _output.WriteLine($"    sum of the periodogram over every bin : {proofTotal:E6}");
        _output.WriteLine($"    the same block's mean square directly : {proofDirect:E6}");
        _output.WriteLine($"    relative difference                   : "
            + $"{Math.Abs(proofTotal - proofDirect) / proofDirect:E3}");
        _output.WriteLine(string.Empty);

        var rows = new List<(double Rung, double First, double Second, double SlotSignal, double Sigma,
            double BandPower, double FullBandPower)>();

        foreach (var (rung, count) in new[] { (-21.0, 20), (-10.0, 10) })
        {
            var noise = new GaussianNoise(222_000 + (int)Math.Round(-rung));

            for (var i = 0; i < count; i++)
            {
                var entry = population[i % population.Count];

                // ---- the audio, built exactly as the ladder builds it ----
                var symbols = Ft8SymbolEncoder.Encode(entry.Message);
                var transmission = Ft8Waveform.Synthesize(symbols, Rate, (float)Unit222TraceTests.OnGridHz);

                var signalPower = SearchFixture.TransmissionPower(
                    Rate, entry, Unit222TraceTests.OnGridHz);
                var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, rung, Rate);
                var drawn = noise.Block(slotSamples, sigma);

                // ---- READING ONE: the ladder's own, through SignalToNoise ----
                var deliveredNoisePower = SignalToNoise.MeanSquare(drawn);
                var first = SignalToNoise.DecibelsFor(signalPower, deliveredNoisePower, Rate);

                // ---- READING TWO: from the samples, no SignalToNoise anywhere below this line ----
                var noiseDensity = MeanPowerPerHertzInBand(drawn, 0, slotSamples, 0.0, ReferenceBandwidthHz);
                var noiseInReference = noiseDensity * ReferenceBandwidthHz;

                var signalBins = PowerPerHertz(
                    transmission, 0, transmission.Length / SegmentLength * SegmentLength);
                var signalTotal = signalBins.Sum() * ((double)Rate / SegmentLength);

                var second = 10.0 * Math.Log10(signalTotal / noiseInReference);

                // The named 0.74 dB candidate, measured rather than argued: the same signal power
                // spread over the whole slot instead of over the transmission.
                var slotSignal = 10.0 * Math.Log10(
                    signalTotal * transmission.Length / slotSamples / noiseInReference);

                var fullBand = MeanPowerPerHertzInBand(drawn, 0, slotSamples, 0.0, Rate / 2.0)
                    * (Rate / 2.0);

                rows.Add((rung, first, second, slotSignal, sigma, noiseInReference, fullBand));
            }
        }

        _output.WriteLine("  BOTH READINGS AND THEIR DIFFERENCE, PRINTED BEFORE ANY BOUND IS ASSERTED:");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"rung",7} {"#",4} {"reading 1",11} {"reading 2",11} {"diff dB",9} "
            + $"{"slot-avg 2",11} {"slot diff",10}");

        var n = 0;
        foreach (var row in rows)
        {
            n++;
            _output.WriteLine($"{row.Rung,7:F1} {n,4} {row.First,11:F4} {row.Second,11:F4} "
                + $"{row.Second - row.First,9:F4} {row.SlotSignal,11:F4} "
                + $"{row.SlotSignal - row.First,10:F4}");
        }

        _output.WriteLine(string.Empty);

        var differences = rows.Select(r => r.Second - r.First).ToArray();
        var worst = differences.Max(Math.Abs);

        _output.WriteLine("  ACROSS ALL " + rows.Count + " TRIALS:");
        _output.WriteLine($"    mean difference    : {differences.Average():F4} dB");
        _output.WriteLine($"    largest difference : {worst:F4} dB");
        _output.WriteLine($"    lowest / highest   : {differences.Min():F4} / {differences.Max():F4} dB");
        _output.WriteLine(string.Empty);

        _output.WriteLine("  THE THREE OTHER CANDIDATES, MEASURED FROM THE SAME SAMPLES RATHER THAN");
        _output.WriteLine("  ARGUED, so that a convention error would show as a NUMBER and not as a");
        _output.WriteLine("  suspicion:");
        _output.WriteLine(string.Empty);

        var probe = rows[0];
        var sigmaSquared = probe.Sigma * probe.Sigma;
        _output.WriteLine($"    sigma^2 as the ladder set it                 : {sigmaSquared:E6}");
        _output.WriteLine($"    noise power in 2500 Hz, MEASURED            : {probe.BandPower:E6}");
        _output.WriteLine($"    what a ONE-SIDED density predicts           : "
            + $"{sigmaSquared * ReferenceBandwidthHz / (Rate / 2.0):E6}   "
            + $"({10 * Math.Log10(probe.BandPower / (sigmaSquared * ReferenceBandwidthHz / (Rate / 2.0))):+0.000;-0.000;0.000} dB from measured)");
        _output.WriteLine($"    what a TWO-SIDED density would predict      : "
            + $"{sigmaSquared * ReferenceBandwidthHz / Rate:E6}   "
            + $"({10 * Math.Log10(probe.BandPower / (sigmaSquared * ReferenceBandwidthHz / Rate)):+0.000;-0.000;0.000} dB from measured)");
        _output.WriteLine($"    noise power over the WHOLE sampled band     : {probe.FullBandPower:E6}   "
            + $"({10 * Math.Log10(probe.FullBandPower / probe.BandPower):+0.000;-0.000;0.000} dB above the 2500 Hz figure)");
        _output.WriteLine(string.Empty);

        var slotDifferences = rows.Select(r => r.SlotSignal - r.First).ToArray();
        _output.WriteLine($"    signal over the SLOT rather than the burst  : "
            + $"{slotDifferences.Average():F4} dB, against the 0.74 named above");
        _output.WriteLine(string.Empty);

        _output.WriteLine("2d. THE VERDICT ON THE AXIS.");
        _output.WriteLine(string.Empty);

        if (worst <= AxisSoundWithinDecibels)
        {
            _output.WriteLine($"    THE TWO READINGS AGREE WITHIN {AxisSoundWithinDecibels:F1} dB - largest "
                + $"disagreement {worst:F4} dB.");
            _output.WriteLine("    THE AXIS IS SOUND, THE 1.5 dB BELONGS TO THE RECEIVER, AND THE BUDGET");
            _output.WriteLine("    PROCEEDS. Every number in task 3 is quoted on an axis two independent");
            _output.WriteLine("    instruments now agree about.");
        }
        else
        {
            _output.WriteLine($"    THE TWO READINGS DISAGREE BY {worst:F4} dB, MORE THAN THE "
                + $"{AxisSoundWithinDecibels:F1} dB BOUND.");
            _output.WriteLine("    THAT IS THE NIGHT'S FINDING and the budget is quoted against a");
            _output.WriteLine("    corrected axis. See the candidate table above for which convention");
            _output.WriteLine("    is responsible.");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("    WHAT THIS CHECK DOES NOT SETTLE, said plainly: it confirms the noise side");
        _output.WriteLine("    of the convention from the samples - the one-sided density and the 2500 Hz");
        _output.WriteLine("    reference band are both measured, not assumed - and it confirms the signal");
        _output.WriteLine("    power over the burst. It CANNOT settle whether the published figure itself");
        _output.WriteLine("    is quoted against the burst or against the slot, because that is a fact");
        _output.WriteLine("    about a paper that is not on this machine. THE 0.74 dB SEPARATING THE TWO");
        _output.WriteLine("    IS PRINTED ABOVE so the owner can read the verdict either way.");

        // The instrument proof is an assertion. Everything else is a measurement and is reported.
        Assert.True(
            Math.Abs(proofTotal - proofDirect) / proofDirect < 1e-9,
            $"the periodogram does not satisfy Parseval: {proofTotal:E9} against {proofDirect:E9}");
    }

    /// <summary>
    /// <b>One-sided power per hertz, per bin, averaged over whole segments.</b> Rectangular window,
    /// because the quantity wanted is power and a rectangular window is the one that conserves it.
    /// </summary>
    private static double[] PowerPerHertz(ReadOnlySpan<float> samples, int from, int count)
    {
        var fft = new Ft8RealFft(SegmentLength);
        var bins = new double[fft.BinCount];
        var real = new double[fft.BinCount];
        var imaginary = new double[fft.BinCount];
        var window = new double[SegmentLength];

        var segments = count / SegmentLength;
        var binWidth = (double)Rate / SegmentLength;

        for (var s = 0; s < segments; s++)
        {
            for (var i = 0; i < SegmentLength; i++)
            {
                window[i] = samples[from + (s * SegmentLength) + i];
            }

            fft.Transform(window, real, imaginary);

            for (var k = 0; k < fft.BinCount; k++)
            {
                // |X[k]|^2 / N^2 is bin k's share of the mean square. Doubled everywhere except at
                // DC and Nyquist, which have no conjugate partner to fold in.
                var share = ((real[k] * real[k]) + (imaginary[k] * imaginary[k]))
                    / ((double)SegmentLength * SegmentLength);
                if (k > 0 && k < fft.BinCount - 1)
                {
                    share *= 2.0;
                }

                bins[k] += share / binWidth / segments;
            }
        }

        return bins;
    }

    /// <summary>
    /// The mean power per hertz across a band, which for white noise is its density and is free of
    /// any argument about where a bin edge falls.
    /// </summary>
    private static double MeanPowerPerHertzInBand(
        ReadOnlySpan<float> samples, int from, int count, double lowHz, double highHz)
    {
        var bins = PowerPerHertz(samples, from, count);
        var binWidth = (double)Rate / SegmentLength;

        var total = 0.0;
        var used = 0;
        for (var k = 0; k < bins.Length; k++)
        {
            var frequency = k * binWidth;
            if (frequency < lowHz || frequency > highHz)
            {
                continue;
            }

            total += bins[k];
            used++;
        }

        return total / used;
    }

    /// <summary>The mean square of a block, computed here so the check owes nothing to the axis.</summary>
    private static double DirectMeanSquare(ReadOnlySpan<float> samples)
    {
        var total = 0.0;
        foreach (var sample in samples)
        {
            total += (double)sample * sample;
        }

        return total / samples.Length;
    }
}
