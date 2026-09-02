namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// The signal-to-noise ratio FT8's published sensitivity figures are quoted in, written down
/// explicitly with its arithmetic shown rather than asserted.
/// </summary>
/// <remarks>
/// <para>
/// <b>The definition, in full, because "SNR" alone names nothing.</b> A ratio needs a bandwidth to
/// be a number, and the amateur weak-signal convention — the one WSJT-X reports and the one the
/// published FT8 threshold of about -21 dB is quoted against — is:
/// </para>
/// <code>
///   SNR(dB) = 10 * log10( signal power / noise power in a 2500 Hz reference bandwidth )
/// </code>
/// <para>
/// <b>Where the 2500 Hz comes from</b> is the nominal SSB channel a receiver hands to the decoder;
/// it is a convention and not a property of the signal, which is why it has to be stated. FT8
/// occupies about 50 Hz, so the same signal against the same noise reads roughly 17 dB better in its
/// own bandwidth than in the reference one, and a figure quoted without saying which is not
/// comparable to anything.
/// </para>
/// <para>
/// <b>Turning that into a noise amplitude, step by step.</b> White Gaussian noise of standard
/// deviation sigma, in real samples at a rate <c>fs</c>, has total power sigma^2 spread evenly over
/// the one-sided band 0 to <c>fs/2</c>. So its power spectral density is
/// <c>sigma^2 / (fs/2)</c> per hertz, and the power it puts inside the reference bandwidth is:
/// </para>
/// <code>
///   noise power in 2500 Hz = sigma^2 * 2500 / (fs / 2)
/// </code>
/// <para>
/// At 12 kHz that is <c>sigma^2 * 2500 / 6000</c>, which is <c>sigma^2 * 0.41667</c> — so the noise
/// inside the reference bandwidth is <em>less</em> than the total noise power, because the sampled
/// band is wider than the reference one. Setting the ratio to the requested SNR and solving:
/// </para>
/// <code>
///   sigma = sqrt( signalPower * (fs/2) / (2500 * 10^(snr/10)) )
/// </code>
/// <para>
/// <b>The signal power is measured, not assumed.</b> The FT8 waveform is a constant-envelope sine
/// with raised-cosine ramps at the two ends, so its mean square is close to but not exactly 0.5.
/// <see cref="MeanSquare"/> takes it from the samples that will actually be transmitted.
/// </para>
/// </remarks>
internal static class SignalToNoise
{
    /// <summary>The reference bandwidth the published FT8 figures are quoted in, in hertz.</summary>
    public const double ReferenceBandwidthHz = 2500.0;

    /// <summary>The mean square of a block of samples: its power.</summary>
    public static double MeanSquare(ReadOnlySpan<float> samples)
    {
        double sum = 0;
        foreach (var sample in samples)
        {
            sum += (double)sample * sample;
        }

        return sum / samples.Length;
    }

    /// <summary>
    /// The noise standard deviation that puts a signal of <paramref name="signalPower"/> at
    /// <paramref name="snrDecibels"/> in the reference bandwidth.
    /// </summary>
    public static double NoiseAmplitudeFor(double signalPower, double snrDecibels, int sampleRate)
    {
        var sampledBandwidth = sampleRate / 2.0;
        var noisePowerInReference = signalPower / Math.Pow(10.0, snrDecibels / 10.0);
        var totalNoisePower = noisePowerInReference * sampledBandwidth / ReferenceBandwidthHz;
        return Math.Sqrt(totalNoisePower);
    }

    /// <summary>
    /// The signal-to-noise ratio, in decibels in the reference bandwidth, of a signal of
    /// <paramref name="signalPower"/> against noise of total power
    /// <paramref name="totalNoisePower"/>.
    /// </summary>
    public static double DecibelsFor(double signalPower, double totalNoisePower, int sampleRate)
    {
        var sampledBandwidth = sampleRate / 2.0;
        var noisePowerInReference = totalNoisePower * ReferenceBandwidthHz / sampledBandwidth;
        return 10.0 * Math.Log10(signalPower / noisePowerInReference);
    }
}
