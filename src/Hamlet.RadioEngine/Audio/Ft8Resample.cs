namespace Hamlet.RadioEngine.Audio;

/// <summary>
/// Puts a recording onto the twelve kilohertz grid FT8 is decoded on.
/// </summary>
/// <remarks>
/// <para>**THE SOUND CARD DECIDES THE RATE AND THE DECODER DOES NOT GET A VOTE.**
/// <see cref="WasapiAudioSource"/> passes whatever the device hands over straight
/// through — its own remarks say so, and say why: the CW decoder counts samples
/// and is indifferent to the rate. **FT8 is not indifferent.** A symbol is
/// exactly 0.160 s, and the library refuses any rate where that is not a whole
/// number of samples; everything measured about this decoder over five steps was
/// measured at 12 000 Hz, and a rate it merely tolerates is a rate nobody has
/// ever run it at.</para>
/// <para>**BAND-LIMITED, NOT NEAREST-SAMPLE.** Dropping three samples in four to
/// get from 48 kHz to 12 kHz folds everything above 6 kHz back down into the
/// passband, and the fold lands on top of the 200–3000 Hz sliver FT8 lives in.
/// That is a decoder that goes deaf in the presence of hiss, which is the one
/// failure this phase's step 6 exists to catch, arriving through the plumbing
/// instead.</para>
/// <para>**PURE, AND NO CLOCK IS READ.** Samples in, samples out, so the same
/// recording resamples identically on any machine at any hour (§5).</para>
/// </remarks>
public static class Ft8Resample
{
    /// <summary>The rate FT8 is decoded at.</summary>
    /// <remarks>
    /// Twelve thousand, which is `Ft8WaterfallGeometry.DefaultSampleRate` and is
    /// upstream's. Named here rather than referenced so this file stays readable
    /// beside the rest of the audio plumbing; the two are asserted equal by test.
    /// </remarks>
    public const int TargetSampleRate = 12_000;

    /// <summary>How many zero crossings of the sinc each side are kept.</summary>
    /// <remarks>
    /// **THE ONE KNOB, AND IT IS A LENGTH RATHER THAN A QUALITY SETTING.** Sixteen
    /// each side is a long enough kernel that the stop band is far below anything
    /// a receiver's own noise floor would let matter, and short enough that a
    /// fifteen-second slot resamples in a few milliseconds.
    /// </remarks>
    public const int ZeroCrossings = 16;

    /// <summary>
    /// Where the anti-alias filter is put, as a fraction of the lower Nyquist.
    /// </summary>
    /// <remarks>
    /// Nine tenths, so the cutoff at 48 kHz to 12 kHz is 5.4 kHz — well clear of
    /// the 3 kHz top of the FT8 passband and well clear of the 6 kHz fold.
    /// </remarks>
    public const double CutoffFraction = 0.45;

    /// <summary>Puts a recording on the FT8 grid.</summary>
    /// <param name="audio">The recording, at whatever rate it arrived.</param>
    /// <returns>
    /// The same audio at <see cref="TargetSampleRate"/>. The original is returned
    /// unchanged when it is already there.
    /// </returns>
    /// <exception cref="ArgumentNullException">The audio is null.</exception>
    public static MonoAudio ToFt8Rate(MonoAudio audio)
    {
        ArgumentNullException.ThrowIfNull(audio);

        return audio.SampleRate == TargetSampleRate
            ? audio
            : new MonoAudio(
                TargetSampleRate,
                Resample(audio.Samples, audio.SampleRate, TargetSampleRate));
    }

    /// <summary>Resamples a run of samples between two rates.</summary>
    /// <param name="samples">The input.</param>
    /// <param name="fromRate">What the input is at.</param>
    /// <param name="toRate">What the output should be at.</param>
    /// <returns>The resampled run, empty when the input is.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Either rate is not positive.</exception>
    /// <remarks>
    /// <para>**EVERY OUTPUT SAMPLE IS NORMALISED BY ITS OWN KERNEL SUM.** Without
    /// it the gain wanders by a fraction of a decibel with the fractional phase,
    /// and the first and last few samples of every slot fade out — an edge droop
    /// that a decoder looking for a transmission starting near the boundary would
    /// read as a weaker signal.</para>
    /// </remarks>
    public static float[] Resample(
        ReadOnlySpan<float> samples, int fromRate, int toRate)
    {
        if (fromRate <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fromRate), fromRate, "A sample rate must be positive.");
        }

        if (toRate <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(toRate), toRate, "A sample rate must be positive.");
        }

        if (samples.Length == 0)
        {
            return Array.Empty<float>();
        }

        if (fromRate == toRate)
        {
            return samples.ToArray();
        }

        // Cycles per input sample. The filter goes at the lower of the two
        // Nyquists, so the same arithmetic covers decimation and interpolation.
        var cutoff = CutoffFraction * Math.Min(fromRate, toRate) / fromRate;

        var kernel = new SincKernel(cutoff, ZeroCrossings);

        var step = fromRate / (double)toRate;
        var count = (int)Math.Round(samples.Length / step);
        var output = new float[Math.Max(0, count)];

        for (var n = 0; n < output.Length; n++)
        {
            var centre = n * step;

            var first = (int)Math.Ceiling(centre - kernel.HalfWidth);
            var last = (int)Math.Floor(centre + kernel.HalfWidth);

            if (first < 0)
            {
                first = 0;
            }

            if (last > samples.Length - 1)
            {
                last = samples.Length - 1;
            }

            double sum = 0;
            double weight = 0;

            for (var i = first; i <= last; i++)
            {
                var tap = kernel.At(centre - i);
                sum += samples[i] * tap;
                weight += tap;
            }

            output[n] = weight > 0 ? (float)(sum / weight) : 0f;
        }

        return output;
    }

    /// <summary>
    /// A windowed sinc, tabulated once and read with linear interpolation.
    /// </summary>
    /// <remarks>
    /// A slot at 48 kHz needs about twenty-five million taps. Evaluating a sine
    /// for each is a second of arithmetic on the press; reading a table of nine
    /// thousand entries is a few milliseconds, and the interpolation error is
    /// orders below the quantisation of the audio it is filtering.
    /// </remarks>
    private sealed class SincKernel
    {
        private const int PerSample = 128;

        private readonly double[] _taps;

        internal SincKernel(double cutoff, int zeroCrossings)
        {
            HalfWidth = zeroCrossings / (2 * cutoff);

            _taps = new double[(int)Math.Ceiling(HalfWidth * PerSample) + 2];

            for (var k = 0; k < _taps.Length; k++)
            {
                var x = k / (double)PerSample;
                _taps[k] = x > HalfWidth
                    ? 0
                    : Sinc(2 * cutoff * x) * Blackman(x / HalfWidth);
            }
        }

        /// <summary>How far the kernel reaches, in input samples.</summary>
        internal double HalfWidth { get; }

        /// <summary>The tap at a distance, in input samples, from the centre.</summary>
        internal double At(double x)
        {
            var place = Math.Abs(x) * PerSample;
            var k = (int)place;

            if (k + 1 >= _taps.Length)
            {
                return 0;
            }

            var fraction = place - k;

            return (_taps[k] * (1 - fraction)) + (_taps[k + 1] * fraction);
        }

        private static double Sinc(double u)
            => u == 0 ? 1 : Math.Sin(Math.PI * u) / (Math.PI * u);

        /// <summary>The Blackman window, over a half-width normalised to one.</summary>
        private static double Blackman(double w)
        {
            var t = (Math.Clamp(w, -1, 1) + 1) / 2;

            return 0.42
                - (0.5 * Math.Cos(2 * Math.PI * t))
                + (0.08 * Math.Cos(4 * Math.PI * t));
        }
    }
}
