namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// Where the strongest steady tone in the passband is, from a time-averaged
/// magnitude spectrum.
/// </summary>
/// <remarks>
/// <para>**TWELVE LINES OF ARITHMETIC THAT GOT TWO PITCHES RIGHT WHERE THE
/// TRACKER DID NOT** (work instruction 050, task 3). A standalone bench
/// implementing nothing but a magnitude peak with parabolic interpolation
/// measured 400.4 Hz where the tracker committed to 850, and 801.3 Hz where the
/// tracker declined to call anything a station at all. That is the whole of the
/// method: average the magnitude spectrum over the recording, take the largest
/// bin in the range Morse is worked in, and fit a parabola through it and its
/// two neighbours to find the peak between bins.</para>
/// <para>**IT MEASURES A PITCH AND IT DECIDES NOTHING.** Admission — whether
/// anybody is keying at all — is not this type's business and is not changed by
/// it (HM-DEC-095, HM-DEC-120). A peak exists in any recording, including one
/// holding nothing but noise, so a number from here is a candidate pitch and
/// never evidence that a station is there. **The bench that inspired it has no
/// refusal and emits text from noise; that is exactly what Hamlet must not
/// do.**</para>
/// <para>**AVERAGING IS THE POINT, NOT AN OPTIMISATION.** A keyed signal is
/// absent for most of a recording, so a peak taken from one window lands wherever
/// that window happened to fall. Averaging magnitude over the whole file lets a
/// signal present a third of the time out-vote noise that is present all of it,
/// which is the same argument HM-DEC-090 made for held peaks.</para>
/// </remarks>
public static class CwSpectralPeak
{
    /// <summary>The transform length, in samples.</summary>
    /// <remarks>
    /// At eight kilohertz this is about two seconds of audio and just under half
    /// a hertz a bin, which is far finer than the twenty-five hertz spacing the
    /// tone tracker's bank uses. The interpolation below refines it further.
    /// </remarks>
    public const int Window = 16384;

    /// <summary>The lowest pitch considered, in hertz.</summary>
    /// <remarks>The radio's own CW pitch range is 300 to 900 Hz (§4, p. 4-14).</remarks>
    public const double LowHz = 300;

    /// <summary>The highest pitch considered, in hertz.</summary>
    /// <remarks>
    /// Above the radio's range on purpose: the operator's own captures have been
    /// found with the tracker sitting at 850, and a search that could not reach a
    /// wrong answer could not disagree with one either.
    /// </remarks>
    public const double HighHz = 1200;

    /// <summary>Find the strongest steady tone, or null.</summary>
    /// <param name="samples">The audio.</param>
    /// <param name="sampleRate">Its sample rate.</param>
    /// <returns>
    /// The peak in hertz, or **null where there is not enough audio to transform
    /// even once**. Null is "nobody measured", not "nothing is there" (§0.0).
    /// </returns>
    public static double? Find(float[] samples, int sampleRate)
    {
        ArgumentNullException.ThrowIfNull(samples);

        if (sampleRate <= 0 || samples.Length < Window)
        {
            return null;
        }

        var spectrum = Average(samples, sampleRate);
        var binHz = (double)sampleRate / Window;

        var lowest = Math.Max(1, (int)Math.Ceiling(LowHz / binHz));
        var highest = Math.Min(spectrum.Length - 2, (int)(HighHz / binHz));

        if (highest <= lowest)
        {
            return null;
        }

        var peak = lowest;

        for (var i = lowest; i <= highest; i++)
        {
            if (spectrum[i] > spectrum[peak])
            {
                peak = i;
            }
        }

        return Interpolate(spectrum, peak, binHz);
    }

    /// <summary>
    /// Find the strongest steady tone over the loudest stretch of the audio
    /// rather than over all of it.
    /// </summary>
    /// <param name="samples">The audio.</param>
    /// <param name="sampleRate">Its sample rate.</param>
    /// <param name="stretchSeconds">How long a stretch to measure over.</param>
    /// <returns>The peak in hertz, or null where there is not enough audio.</returns>
    /// <remarks>
    /// <para>**THE ERROR IS A DUTY-CYCLE EFFECT AND THIS IS THE CONSEQUENCE**
    /// (work instruction 052, task 4). Unit 051 measured `CwSpectralPeak` against
    /// synthetic carriers: on a busy message it is accurate to three hundredths
    /// of a hertz at every carrier and speed tried, and on a low-duty one it errs
    /// by ±1.25 Hz — the same magnitude as the real error that retired `N4L`.
    /// **Averaging a spectrum over silence adds noise to the average and nothing
    /// else**, so the fix is to average over the part where somebody is
    /// sending.</para>
    /// <para>**THE STRETCH IS CHOSEN BY ENERGY ALONE.** Not by where characters
    /// came out, not by where a pitch was admitted: either would make the
    /// measurement circular, and the order forbids both. Total energy over a
    /// sliding stretch, largest wins.</para>
    /// <para>**IT IS A PITCH AND STILL NOT A VERDICT.** Choosing the loudest
    /// stretch of a recording that holds nothing gives the loudest stretch of
    /// noise, and admission is asked elsewhere (HM-DEC-095, HM-DEC-120).</para>
    /// </remarks>
    public static double? FindOverLoudestStretch(
        float[] samples, int sampleRate, double stretchSeconds)
    {
        ArgumentNullException.ThrowIfNull(samples);

        if (sampleRate <= 0 || samples.Length < Window)
        {
            return null;
        }

        var stretch = (int)(stretchSeconds * sampleRate);

        if (stretch < Window || stretch >= samples.Length)
        {
            // Nothing to choose between: the stretch is the whole recording, or
            // shorter than one transform.
            return Find(samples, sampleRate);
        }

        // Energy in blocks of one transform, so the search is over the same
        // granularity the transform sees.
        var blocks = samples.Length / Window;
        var energy = new double[blocks];

        for (var b = 0; b < blocks; b++)
        {
            var sum = 0.0;

            for (var i = b * Window; i < (b + 1) * Window; i++)
            {
                sum += (double)samples[i] * samples[i];
            }

            energy[b] = sum;
        }

        var span = Math.Max(1, stretch / Window);
        var running = 0.0;

        for (var b = 0; b < Math.Min(span, blocks); b++)
        {
            running += energy[b];
        }

        var best = running;
        var bestStart = 0;

        for (var b = span; b < blocks; b++)
        {
            running += energy[b] - energy[b - span];

            if (running > best)
            {
                best = running;
                bestStart = b - span + 1;
            }
        }

        var from = bestStart * Window;
        var count = Math.Min(span * Window, samples.Length - from);

        return Find(samples.AsSpan(from, count).ToArray(), sampleRate);
    }

    /// <summary>The magnitude spectrum, averaged over the whole recording.</summary>
    /// <remarks>
    /// Hann-windowed, half-overlapped. The window is what stops a tone that does
    /// not fall exactly on a bin from smearing across the whole spectrum and
    /// burying a weaker neighbour.
    /// </remarks>
    private static double[] Average(float[] samples, int sampleRate)
    {
        var bins = (Window / 2) + 1;
        var total = new double[bins];
        var hann = Hann();
        var windows = 0;

        var real = new double[Window];
        var imaginary = new double[Window];

        for (var start = 0; start + Window <= samples.Length; start += Window / 2)
        {
            for (var i = 0; i < Window; i++)
            {
                real[i] = samples[start + i] * hann[i];
                imaginary[i] = 0;
            }

            Transform(real, imaginary);

            for (var i = 0; i < bins; i++)
            {
                total[i] += Math.Sqrt(
                    (real[i] * real[i]) + (imaginary[i] * imaginary[i]));
            }

            windows++;
        }

        if (windows > 1)
        {
            for (var i = 0; i < bins; i++)
            {
                total[i] /= windows;
            }
        }

        return total;
    }

    /// <summary>The Hann window.</summary>
    private static double[] Hann()
    {
        var w = new double[Window];

        for (var i = 0; i < Window; i++)
        {
            w[i] = 0.5 * (1 - Math.Cos(2 * Math.PI * i / (Window - 1)));
        }

        return w;
    }

    /// <summary>Where the peak really is, between the bins either side of it.</summary>
    /// <remarks>
    /// **A PARABOLA THROUGH THREE MAGNITUDES.** Without it the answer is quantised
    /// to the bin spacing, and a pitch reported to the nearest half hertz is worth
    /// having when the decoder mixes down to it.
    /// </remarks>
    private static double Interpolate(double[] spectrum, int peak, double binHz)
    {
        var a = spectrum[peak - 1];
        var b = spectrum[peak];
        var c = spectrum[peak + 1];

        var denominator = a - (2 * b) + c;

        if (denominator == 0)
        {
            return peak * binHz;
        }

        var offset = 0.5 * (a - c) / denominator;

        // A fit that lands more than a bin away is a fit to something that is not
        // a peak, so the bin itself is the honest answer.
        return Math.Abs(offset) > 1
            ? peak * binHz
            : (peak + offset) * binHz;
    }

    /// <summary>An in-place radix-2 transform.</summary>
    /// <param name="real">The real part, overwritten with the result.</param>
    /// <param name="imaginary">The imaginary part, overwritten with the result.</param>
    /// <remarks>
    /// <para>**THIS IS THE FIRST TRANSFORM IN THE ENGINE AND IT NARROWS §6'S
    /// ANSWER RATHER THAN OVERTURNING IT.** That row says there is no FFT, and
    /// gives the reason: the decoder wants a couple of dozen known frequencies,
    /// which is a Goertzel bank. That is still true of the decoder. **Finding a
    /// pitch nobody has named is the opposite problem** — every frequency at once
    /// rather than a handful — and running six hundred Goertzels to answer it
    /// would be the same transform written out longhand.</para>
    /// <para>No dependency was added for it. `Window` is a power of two, so this
    /// is forty lines, and a dependency for forty lines is a dependency to
    /// maintain, license and vendor for the life of a GPL-3.0 release.</para>
    /// </remarks>
    private static void Transform(double[] real, double[] imaginary)
    {
        var n = real.Length;

        for (int i = 1, j = 0; i < n; i++)
        {
            var bit = n >> 1;

            for (; (j & bit) != 0; bit >>= 1)
            {
                j ^= bit;
            }

            j ^= bit;

            if (i < j)
            {
                (real[i], real[j]) = (real[j], real[i]);
                (imaginary[i], imaginary[j]) = (imaginary[j], imaginary[i]);
            }
        }

        for (var length = 2; length <= n; length <<= 1)
        {
            var angle = -2 * Math.PI / length;

            for (var i = 0; i < n; i += length)
            {
                for (var k = 0; k < length / 2; k++)
                {
                    var turn = angle * k;
                    var cos = Math.Cos(turn);
                    var sin = Math.Sin(turn);

                    var evenReal = real[i + k];
                    var evenImaginary = imaginary[i + k];

                    var oddReal = (real[i + k + (length / 2)] * cos)
                                  - (imaginary[i + k + (length / 2)] * sin);
                    var oddImaginary = (real[i + k + (length / 2)] * sin)
                                       + (imaginary[i + k + (length / 2)] * cos);

                    real[i + k] = evenReal + oddReal;
                    imaginary[i + k] = evenImaginary + oddImaginary;
                    real[i + k + (length / 2)] = evenReal - oddReal;
                    imaginary[i + k + (length / 2)] = evenImaginary - oddImaginary;
                }
            }
        }
    }
}
