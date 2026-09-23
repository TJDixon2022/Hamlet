namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// The frequency of one keyed element, measured over that element's own samples.
/// </summary>
/// <remarks>
/// <para>**EVERY OTHER PITCH IN THIS ENGINE IS A PITCH FOR A WHOLE RECORDING.**
/// <see cref="CwSpectralPeak"/> averages a spectrum over seconds, the tone survey
/// ranks bins over its whole window, and the decoder mixes down at one number for
/// as long as it is tracking. All three answer *where is the station*, and none of
/// them can answer *were these two dahs sent by the same person*, because both
/// questions have been folded into one average before either is asked.</para>
/// <para>**THE RESOLUTION IS THE ELEMENT'S OWN LENGTH AND NOTHING BUYS PAST IT.**
/// A transform over T seconds separates tones 1/T apart, so a 190 ms dah resolves
/// to about 5 Hz and a 55 ms dit to about 18 Hz. The parabola below refines the
/// peak *within* that limit, which is worth having; it does not lift the limit,
/// and a caller comparing two elements has to carry the length of the shorter one
/// as the error on the comparison (<see cref="ResolutionHz"/>).</para>
/// <para>**IT MEASURES AND DECIDES NOTHING.** No admission, no tracking, no
/// emission depends on a number from here. That is deliberate for the reason unit
/// 055 recorded after a catastrophic regression: swing said whether a station was
/// there and was fed to the mixdown as well, and the corpus fell from 0.894 to
/// 0.470. A measurement added to the record is not the same thing as a
/// measurement added to the decision path, and this one is the first kind.</para>
/// </remarks>
public static class CwElementPitch
{
    /// <summary>
    /// How far either side of the mixdown pitch the peak is looked for, in hertz.
    /// </summary>
    /// <remarks>
    /// **SIXTY, WHICH IS WIDER THAN THE FILTER THE ELEMENT CAME THROUGH.** The
    /// decoder's integrator is 45 Hz wide
    /// (<see cref="CwProbabilisticDecoder.IntegratorBandwidthHz"/>), so anything
    /// this could find beyond about 22 Hz from the mixdown pitch has already been
    /// attenuated on its way in. The search is deliberately wider than that: a
    /// peak found at the very edge of the band is evidence the mixdown is
    /// mispointed, and a search that stopped at the filter's own edge could never
    /// produce that evidence.
    /// </remarks>
    public const double SearchHz = 60.0;

    /// <summary>The shortest element worth measuring, in milliseconds.</summary>
    /// <remarks>
    /// **TWENTY, BECAUSE BELOW IT THE ANSWER IS WIDER THAN THE SEARCH.** A 20 ms
    /// element resolves to 50 Hz, which is most of the band being searched, and a
    /// figure whose error bar covers its own search range is not a measurement. A
    /// shorter element gets <see cref="double.NaN"/>, which is *nobody measured*
    /// rather than *it was at the mixdown pitch* (§0.0).
    /// </remarks>
    public const double ShortestMilliseconds = 20.0;

    /// <summary>What an element of a given length can resolve, in hertz.</summary>
    /// <param name="milliseconds">How long the element ran.</param>
    /// <returns>The Rayleigh limit, or infinity for a length of nought.</returns>
    /// <remarks>
    /// The error a caller must carry on any comparison between two elements is
    /// this figure for the shorter of the two. **A split between two senders that
    /// does not clear it is a split invented by the measurement** (task 3).
    /// </remarks>
    public static double ResolutionHz(double milliseconds)
        => milliseconds <= 0 ? double.PositiveInfinity : 1000.0 / milliseconds;

    /// <summary>Measure one element's own frequency.</summary>
    /// <param name="samples">The whole recording.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="aroundHz">The pitch the decoder was mixing down at.</param>
    /// <param name="firstSample">The element's first sample.</param>
    /// <param name="sampleCount">How many samples it spans.</param>
    /// <returns>
    /// The measured pitch in hertz, or <see cref="double.NaN"/> where the element
    /// is too short to measure, runs off the end of the audio, or holds no peak.
    /// </returns>
    /// <remarks>
    /// <para>**A DIRECT TRANSFORM AT THE ELEMENT'S OWN BIN SPACING**, rather than
    /// a padded FFT. The element is whatever length it is, and zero-padding it to
    /// a power of two would put the bins closer together without putting any more
    /// information between them — the interpolation would then be fitting a
    /// parabola through three samples of one main lobe and reporting a precision
    /// the audio never had. Summing the transform at k/T for a handful of k costs
    /// less than the pad and cannot mislead about its own resolution.</para>
    /// <para>**HANN, BECAUSE A RECTANGLE'S MAIN LOBE IS TWO BINS WIDE.** A
    /// parabola needs three points on one lobe to mean anything, and a rectangular
    /// window rarely gives three. The Hann lobe spans four, at the cost of a
    /// resolution figure 1.5 times the Rayleigh limit — which is why
    /// <see cref="ResolutionHz"/> is the honest number to compare against and not
    /// the interpolated spread.</para>
    /// </remarks>
    public static double Measure(
        IReadOnlyList<float> samples,
        int sampleRate,
        double aroundHz,
        int firstSample,
        int sampleCount)
    {
        ArgumentNullException.ThrowIfNull(samples);

        if (sampleRate <= 0 || firstSample < 0 || sampleCount <= 0
            || firstSample + sampleCount > samples.Count)
        {
            return double.NaN;
        }

        var milliseconds = 1000.0 * sampleCount / sampleRate;

        if (milliseconds < ShortestMilliseconds)
        {
            return double.NaN;
        }

        // The element's own bin spacing. Everything below is in units of it.
        var binHz = (double)sampleRate / sampleCount;

        var lowest = (int)Math.Floor((aroundHz - SearchHz) / binHz);
        var highest = (int)Math.Ceiling((aroundHz + SearchHz) / binHz);

        lowest = Math.Max(1, lowest);
        highest = Math.Min(sampleCount / 2 - 1, highest);

        // One bin either side of the peak is needed for the parabola, so a search
        // with no room for three bins has no answer to give.
        if (highest - lowest < 2)
        {
            return double.NaN;
        }

        var magnitudes = new double[highest - lowest + 1];

        for (var k = lowest; k <= highest; k++)
        {
            var omega = -2 * Math.PI * k / sampleCount;
            double real = 0;
            double imaginary = 0;

            for (var n = 0; n < sampleCount; n++)
            {
                // Hann, computed inline rather than cached: the window length is
                // this element's and changes from one element to the next, so a
                // cached table would be rebuilt every call anyway.
                var w = 0.5 * (1 - Math.Cos(2 * Math.PI * n / (sampleCount - 1)));
                var value = samples[firstSample + n] * w;
                var angle = omega * n;

                real += value * Math.Cos(angle);
                imaginary += value * Math.Sin(angle);
            }

            magnitudes[k - lowest] = Math.Sqrt((real * real) + (imaginary * imaginary));
        }

        var peak = 0;

        for (var i = 1; i < magnitudes.Length; i++)
        {
            if (magnitudes[i] > magnitudes[peak])
            {
                peak = i;
            }
        }

        // **A PEAK ON THE EDGE OF THE SEARCH IS NOT A PEAK.** It is the largest
        // thing that was looked at, which is a different claim, and it has no
        // neighbour on one side to fit a parabola through.
        if (peak == 0 || peak == magnitudes.Length - 1)
        {
            return double.NaN;
        }

        return Interpolate(magnitudes, peak, lowest, binHz);
    }

    /// <summary>Measure every mark in an element stream over its own samples.</summary>
    /// <param name="elements">The stream the decoder walked.</param>
    /// <param name="samples">The audio those hops index into.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="aroundHz">The pitch the decoder was mixing down at.</param>
    /// <param name="hopMilliseconds">How long one hop is.</param>
    /// <param name="firstHopAtSample">
    /// Which sample the stream's hop nought sits at, for a window that does not
    /// begin at the start of the recording.
    /// </param>
    /// <returns>The same elements, each mark carrying its own measured pitch.</returns>
    /// <remarks>
    /// <para>**GAPS ARE NOT MEASURED AND ARE NOT GIVEN A NOUGHT.** The frequency
    /// of a stretch where nobody is keying is the frequency of whatever noise was
    /// loudest, which is a real number and a meaningless one. They keep
    /// <see cref="double.NaN"/>, which is the only honest entry (§0.0).</para>
    /// <para>**THE HOP IS A CENTRE AND THE ELEMENT IS A SPAN.** The envelope's
    /// hop <c>h</c> is taken at sample <c>h * hop</c>, so a mark running from hop
    /// <c>a</c> to hop <c>b</c> spans the samples between those two centres. The
    /// integrator that produced those hops is 33 ms wide and centred, so the
    /// first and last few milliseconds of the span carry the mark's own edges
    /// rounded off — which costs a little of the element and cannot move its
    /// frequency, since a slower rise does not change what the tone is.</para>
    /// </remarks>
    public static IReadOnlyList<CwElement> MeasureAll(
        IReadOnlyList<CwElement> elements,
        IReadOnlyList<float> samples,
        int sampleRate,
        double aroundHz,
        double hopMilliseconds,
        int firstHopAtSample = 0)
    {
        ArgumentNullException.ThrowIfNull(elements);
        ArgumentNullException.ThrowIfNull(samples);

        var perHop = sampleRate * hopMilliseconds / 1000.0;

        if (perHop <= 0)
        {
            return elements;
        }

        var measured = new List<CwElement>(elements.Count);

        foreach (var element in elements)
        {
            if (!element.IsMark)
            {
                measured.Add(element);

                continue;
            }

            var first = firstHopAtSample + (int)Math.Round(element.StartHop * perHop);
            var last = firstHopAtSample + (int)Math.Round(element.EndHop * perHop);

            measured.Add(element with
            {
                PitchHz = Measure(
                    samples, sampleRate, aroundHz, first, last - first),
            });
        }

        return measured;
    }

    /// <summary>Where the peak sits between the bins either side of it.</summary>
    /// <remarks>
    /// **ON THE LOGARITHM, WHICH IS WHERE A HANN LOBE IS NEARLY A PARABOLA.**
    /// Fitting the linear magnitude biases the answer toward the centre bin;
    /// fitting the decibels does not, and the whole point of interpolating is to
    /// beat the bin. A fit landing more than a bin away is a fit to something that
    /// is not a peak, and the bin itself is then the honest answer, which is the
    /// same guard <see cref="CwSpectralPeak"/> carries.
    /// </remarks>
    private static double Interpolate(
        double[] magnitudes, int peak, int firstBin, double binHz)
    {
        var a = Math.Log(magnitudes[peak - 1] + 1e-30);
        var b = Math.Log(magnitudes[peak] + 1e-30);
        var c = Math.Log(magnitudes[peak + 1] + 1e-30);

        var denominator = a - (2 * b) + c;
        var bin = firstBin + peak;

        if (denominator == 0)
        {
            return bin * binHz;
        }

        var offset = 0.5 * (a - c) / denominator;

        return Math.Abs(offset) > 1 ? bin * binHz : (bin + offset) * binHz;
    }
}
