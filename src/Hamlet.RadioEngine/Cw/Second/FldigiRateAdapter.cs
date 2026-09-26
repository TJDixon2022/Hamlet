namespace Hamlet.RadioEngine.Cw.Second;

/// <summary>
/// Brings audio to the 8000 Hz <see cref="FldigiCwDecoder"/> runs at.
/// </summary>
/// <remarks>
/// <para>**HAMLET'S, NOT FLDIGI'S, AND NOT A PORTED FILE** (work instruction 456,
/// task 2). fldigi's sound layer converts the card's rate to the modem's with
/// libsamplerate, which is outside the receive path and is not ported. This
/// adapter is the stand-in: for a rate that is a whole multiple of 8000 it
/// low-passes with a Blackman-windowed sinc and keeps every Nth sample; at 8000
/// it passes the samples through untouched. Any other rate is refused rather
/// than guessed at.</para>
/// <para>The filter: 481 taps, cut-off 3600 Hz, Blackman window, unity gain at
/// DC. The captures run at 48000 Hz, so N is 6.</para>
/// </remarks>
public static class FldigiRateAdapter
{
    private const int Taps = 481;
    private const double CutoffHz = 3600;

    /// <summary>Converts audio to 8000 Hz.</summary>
    /// <param name="samples">The audio.</param>
    /// <param name="sampleRate">Its rate, 8000 or a whole multiple of it.</param>
    /// <returns>The audio at 8000 Hz, as the doubles fldigi's receiver takes.</returns>
    public static double[] ToFldigiRate(IReadOnlyList<float> samples, int sampleRate)
    {
        ArgumentNullException.ThrowIfNull(samples);

        if (sampleRate == FldigiCwDecoder.CW_SAMPLERATE)
        {
            var same = new double[samples.Count];
            for (var i = 0; i < same.Length; i++) same[i] = samples[i];
            return same;
        }

        if (sampleRate <= 0 || sampleRate % FldigiCwDecoder.CW_SAMPLERATE != 0)
        {
            throw new ArgumentException(
                $"{sampleRate} Hz is not a whole multiple of fldigi's {FldigiCwDecoder.CW_SAMPLERATE} Hz.", nameof(sampleRate));
        }

        var factor = sampleRate / FldigiCwDecoder.CW_SAMPLERATE;
        var taps = Kernel(CutoffHz / sampleRate);
        var half = Taps / 2;
        var output = new double[samples.Count / factor];

        for (var o = 0; o < output.Length; o++)
        {
            var centre = o * factor;
            var sum = 0.0;

            for (var t = 0; t < Taps; t++)
            {
                var at = centre + t - half;
                if (at >= 0 && at < samples.Count) sum += taps[t] * samples[at];
            }

            output[o] = sum;
        }

        return output;
    }

    private static double[] Kernel(double cutoff)
    {
        var taps = new double[Taps];
        var half = Taps / 2;
        var total = 0.0;

        for (var i = 0; i < Taps; i++)
        {
            var x = i - half;
            var sinc = x == 0 ? 2 * cutoff : Math.Sin(2 * Math.PI * cutoff * x) / (Math.PI * x);
            var window = 0.42 - (0.5 * Math.Cos(2 * Math.PI * i / (Taps - 1))) + (0.08 * Math.Cos(4 * Math.PI * i / (Taps - 1)));
            taps[i] = sinc * window;
            total += taps[i];
        }

        for (var i = 0; i < Taps; i++) taps[i] /= total;

        return taps;
    }
}
