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
/// <para>**THE FILTER ITSELF LIVES IN <see cref="SincKernel"/>**, beside this, because
/// unit 324 needed the same filter for a live stream rather than a slot
/// (<see cref="Psk31Resampler"/>) and a hand-copied second set of taps is a second
/// place for them to drift (§0). Nothing about this class's arithmetic changed when it
/// moved.</para>
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
}
