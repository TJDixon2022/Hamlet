using System;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;

namespace Ft8Sharp.Deep;

/// <summary>
/// <b>One slot's audio, mixed to complex baseband about a candidate's eight tones, low-pass filtered
/// and decimated.</b> This is the thing the waterfall is not: it has phase in it, it is not quantised,
/// and it can be read at a time and a frequency no grid names.
/// </summary>
/// <remarks>
/// <para>
/// <b>Built once per base frequency and read many times.</b> The fine search moves a candidate in
/// frequency by at most a quarter of a tone, which is far inside the low-pass, so a frequency offset
/// is applied in the tone correlation rather than by mixing again. That is what makes a search over
/// a grid of positions affordable: the mixing and filtering, which is the expensive part, happen once.
/// </para>
/// <para>
/// <b>The mixing frequency is the centre of the eight tones and not the base tone.</b> Tone <c>k</c>
/// then sits at <c>(k - 3.5) x 6.25 Hz</c>, so the occupied band is symmetric about zero and the
/// low-pass gives every tone the same gain. Mixing at the base tone would put the eight tones from
/// 0 to +43.75 Hz, and a real low-pass would then have to pass 43.75 Hz on one side and reject the
/// image on the other, which is a harder filter for no reason.
/// </para>
/// <para>
/// <b>Standard multirate DSP, cited as such</b>; see <see cref="Ft8DeepBasebandSettings"/> for the
/// full arithmetic and for the citations. Nothing here is derived from any decoder's source.
/// </para>
/// </remarks>
public sealed class Ft8DeepBaseband
{
    /// <summary>
    /// The rotation table is split into a coarse and a fine part so that the mixing phase is
    /// computed exactly at every sample without a trigonometric call per sample and without a
    /// recurrence that drifts over 180000 of them.
    /// </summary>
    private const int PhaseBlock = 1024;

    private readonly float[] _real;
    private readonly float[] _imaginary;

    /// <summary>
    /// The eight tone exponentials over one symbol window, laid out <c>[tone * L + n]</c>, built
    /// once per baseband because they do not depend on where in the slot the window sits nor on the
    /// fine frequency offset. See <see cref="TonePowerGrid"/> for why that is true.
    /// </summary>
    private readonly double[] _toneCos;
    private readonly double[] _toneSin;

    private Ft8DeepBaseband(
        float[] real,
        float[] imaginary,
        int sampleRate,
        double centreFrequencyHz,
        Ft8DeepBasebandSettings settings)
    {
        _real = real;
        _imaginary = imaginary;
        SampleRate = sampleRate;
        CentreFrequencyHz = centreFrequencyHz;
        Settings = settings;

        var length = settings.SamplesPerSymbol(sampleRate);
        _toneCos = new double[Ft8SymbolEncoder.ToneCount * length];
        _toneSin = new double[Ft8SymbolEncoder.ToneCount * length];

        for (var tone = 0; tone < Ft8SymbolEncoder.ToneCount; tone++)
        {
            for (var n = 0; n < length; n++)
            {
                var (sin, cos) = Math.SinCos(-2.0 * Math.PI * tone * n / length);
                _toneCos[(tone * length) + n] = cos;
                _toneSin[(tone * length) + n] = sin;
            }
        }
    }

    /// <summary>The rate of the audio this was built from.</summary>
    public int SampleRate { get; }

    /// <summary>
    /// The frequency the audio was mixed down about: the centre of the eight tones, which is the
    /// base tone plus three and a half tone spacings.
    /// </summary>
    public double CentreFrequencyHz { get; }

    /// <summary>The frequency of the lowest of the eight tones this was built about.</summary>
    public double BaseFrequencyHz => CentreFrequencyHz - (3.5 * ToneSpacingHz);

    /// <summary>How this was mixed, filtered and decimated.</summary>
    public Ft8DeepBasebandSettings Settings { get; }

    /// <summary>Baseband samples per second.</summary>
    public double RateHz => Settings.DecimatedRateHz(SampleRate);

    /// <summary>Baseband samples in one FT8 symbol. Always whole - the settings refuse otherwise.</summary>
    public int SamplesPerSymbol => Settings.SamplesPerSymbol(SampleRate);

    /// <summary>How many baseband samples there are.</summary>
    public int Length => _real.Length;

    /// <summary>The in-phase part.</summary>
    public ReadOnlySpan<float> Real => _real;

    /// <summary>The quadrature part.</summary>
    public ReadOnlySpan<float> Imaginary => _imaginary;

    /// <summary>The FT8 tone spacing, in hertz. The reciprocal of the symbol period.</summary>
    public static double ToneSpacingHz => 1.0 / Ft8WaterfallGeometry.SymbolPeriodSeconds;

    /// <summary>
    /// Mixes, filters and decimates one slot about the eight tones starting at
    /// <paramref name="baseFrequencyHz"/>.
    /// </summary>
    /// <param name="samples">The slot's audio. May be any length, including empty.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="baseFrequencyHz">The frequency of the lowest of the eight tones, in hertz.</param>
    /// <param name="settings">How to mix, filter and decimate, or null for the default.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The sample rate is not positive, or the settings do not give a whole number of baseband
    /// samples in a symbol at that rate.
    /// </exception>
    /// <remarks>
    /// <b>An empty or very short slot is an ordinary answer and not an error.</b> The result simply
    /// has few or no baseband samples in it, and every window the extractor asks for then falls
    /// outside, which <c>Ft8SoftSymbols</c>'s own rule turns into three zero ratios - no opinion.
    /// A caller scanning a live receiver hands over whatever arrived.
    /// </remarks>
    public static Ft8DeepBaseband Build(
        ReadOnlySpan<float> samples,
        int sampleRate,
        double baseFrequencyHz,
        Ft8DeepBasebandSettings? settings = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleRate);

        var used = settings ?? Ft8DeepBasebandSettings.Default;

        if (used.SamplesPerSymbol(sampleRate) == 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(settings),
                used.Decimation,
                $"A decimation of {used.Decimation} at {sampleRate} Hz does not leave a whole number "
                + "of baseband samples in a 0.160 s symbol. A fractional symbol window is a "
                + "resampling problem and the eight tone correlations stop being orthogonal, so it "
                + "is refused rather than rounded to.");
        }

        var centre = baseFrequencyHz + (3.5 * ToneSpacingHz);
        var taps = BuildLowPass(used.FilterLength, used.CutoffHz, sampleRate);
        var delay = (used.FilterLength - 1) / 2;

        // Mix first, whole, because the filter below reads each input sample up to FilterLength
        // times and re-deriving the rotation at every tap would be the dominant cost.
        var (mixedReal, mixedImaginary) = Mix(samples, sampleRate, centre);

        var outputs = samples.Length == 0 ? 0 : ((samples.Length - 1) / used.Decimation) + 1;
        var real = new float[outputs];
        var imaginary = new float[outputs];

        for (var m = 0; m < outputs; m++)
        {
            // out[m] is the filtered signal at input sample m x Decimation. The group delay of a
            // linear-phase FIR of odd length is exactly (N-1)/2 samples and is removed here by
            // reading the convolution that many samples later, so the baseband's time axis is the
            // audio's time axis and no constant hides in it.
            var centreIndex = (m * used.Decimation) + delay;
            var accumulatorReal = 0.0;
            var accumulatorImaginary = 0.0;

            var first = Math.Max(0, centreIndex - (mixedReal.Length - 1));
            var last = Math.Min(used.FilterLength - 1, centreIndex);

            for (var j = first; j <= last; j++)
            {
                var tap = taps[j];
                var source = centreIndex - j;
                accumulatorReal += tap * mixedReal[source];
                accumulatorImaginary += tap * mixedImaginary[source];
            }

            real[m] = (float)accumulatorReal;
            imaginary[m] = (float)accumulatorImaginary;
        }

        return new Ft8DeepBaseband(real, imaginary, sampleRate, centre, used);
    }

    /// <summary>
    /// Wraps baseband samples that were produced somewhere else, without mixing or filtering them
    /// again.
    /// </summary>
    /// <param name="real">The in-phase part. Copied.</param>
    /// <param name="imaginary">The quadrature part. Copied, and the same length as the in-phase part.</param>
    /// <param name="sampleRate">The rate of the audio the baseband came from, before decimation.</param>
    /// <param name="centreFrequencyHz">The frequency it was mixed down about.</param>
    /// <param name="settings">The decimation and filter it was produced with.</param>
    /// <exception cref="ArgumentException">The two parts are different lengths.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The sample rate is not positive, or the settings do not give a whole number of baseband
    /// samples in a symbol at that rate.
    /// </exception>
    /// <remarks>
    /// <b>Unit 248 used this to measure a windowed variant of the symbol correlation once</b>, which
    /// is the only reason it exists: the taper is a measurement rather than a setting, so it is
    /// applied by a caller to a copy rather than by an option nobody should reach for. It is also
    /// what a caller with a receiver that already produces I and Q would want.
    /// </remarks>
    public static Ft8DeepBaseband FromBasebandSamples(
        ReadOnlySpan<float> real,
        ReadOnlySpan<float> imaginary,
        int sampleRate,
        double centreFrequencyHz,
        Ft8DeepBasebandSettings? settings = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleRate);

        if (real.Length != imaginary.Length)
        {
            throw new ArgumentException(
                $"A complex signal has as many quadrature samples as in-phase ones and "
                + $"{real.Length} were given against {imaginary.Length}.",
                nameof(imaginary));
        }

        var used = settings ?? Ft8DeepBasebandSettings.Default;

        if (used.SamplesPerSymbol(sampleRate) == 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(settings),
                used.Decimation,
                $"A decimation of {used.Decimation} at {sampleRate} Hz does not leave a whole number "
                + "of baseband samples in a 0.160 s symbol.");
        }

        return new Ft8DeepBaseband(
            real.ToArray(), imaginary.ToArray(), sampleRate, centreFrequencyHz, used);
    }

    /// <summary>
    /// <b>The eight tone powers of every symbol of one frame, in decibels, read at a start time this
    /// baseband was told rather than at any grid position.</b>
    /// </summary>
    /// <param name="startSeconds">
    /// When the frame's first symbol begins, in seconds from the start of the slot. <b>Continuous</b>
    /// - it is not a block index and it is not rounded to one.
    /// </param>
    /// <param name="frequencyOffsetHz">
    /// How far the eight tones sit from where this baseband was mixed, in hertz. <b>Continuous</b>,
    /// and small: the fine search never moves a candidate by more than a quarter of a tone, which is
    /// far inside the low-pass, so no second mixing pass is needed.
    /// </param>
    /// <param name="decibels">
    /// <c>SymbolCount x ToneCount</c> magnitudes, laid out <c>[symbol * 8 + tone]</c>, <b>indexed by
    /// tone</b>. A symbol whose window falls outside the baseband is left as
    /// <see cref="double.NaN"/>.
    /// </param>
    /// <param name="syncSymbolsOnly">
    /// When true only the 21 Costas symbols are filled, which is all a sync correlation needs and is
    /// a quarter of the work.
    /// </param>
    /// <returns>How many symbols were filled.</returns>
    /// <exception cref="ArgumentException"><paramref name="decibels"/> is the wrong length.</exception>
    /// <remarks>
    /// <para>
    /// <b>The window is rectangular and exactly one symbol long, and that is a choice with a reason.</b>
    /// The tone spacing is the reciprocal of the symbol period, so over exactly one symbol the eight
    /// tone exponentials are orthogonal and the correlation below is the matched filter for the
    /// alphabet. Any taper - the <c>Ft8Monitor.HannSquaredSine</c> the waterfall applies, for
    /// instance - widens each tone's response until it overlaps its neighbours, which trades
    /// resolution between the tones for resolution against everything outside them. The waterfall
    /// needs the second because it analyses the whole passband at once; a candidate already mixed
    /// down to its own eight tones does not. <b>Unit 248 measured the tapered window once rather
    /// than sweeping windows</b>, and the number is in <c>docs/unit248-baseband-resync.md</c>.
    /// </para>
    /// <para>
    /// Decibels are <c>10 log10(1e-12 + power)</c>, which is <c>Ft8Monitor</c>'s own conversion
    /// including its floor, so the two extractors are on one scale and can be compared without an
    /// offset being fitted between them.
    /// </para>
    /// </remarks>
    public int TonePowerGrid(
        double startSeconds,
        double frequencyOffsetHz,
        Span<double> decibels,
        bool syncSymbolsOnly = false)
    {
        var length = SamplesPerSymbol;
        var tones = Ft8SymbolEncoder.ToneCount;

        if (decibels.Length != Ft8SymbolEncoder.SymbolCount * tones)
        {
            throw new ArgumentException(
                $"The grid is {Ft8SymbolEncoder.SymbolCount} symbols of {tones} tones and a span of "
                + $"{decibels.Length} was given.",
                nameof(decibels));
        }

        decibels.Fill(double.NaN);

        // THE PRE-ROTATION, built once for the whole frame. Two rotations are folded into it: the
        // fine frequency offset, and the half-tone shift that turns "tones at (k - 3.5) spacings"
        // into "DFT bins 0 to 7". After it, every tone is a plain bin of an L-point transform and
        // the eight exponentials are the fixed table built in the constructor. Without this the
        // search would spend a trigonometric call per tone per sample per position.
        var rotationCos = new double[length];
        var rotationSin = new double[length];
        var step = (-2.0 * Math.PI * frequencyOffsetHz / RateHz) + (2.0 * Math.PI * 3.5 / length);
        for (var n = 0; n < length; n++)
        {
            (rotationSin[n], rotationCos[n]) = Math.SinCos(step * n);
        }

        var filled = 0;

        for (var symbol = 0; symbol < Ft8SymbolEncoder.SymbolCount; symbol++)
        {
            if (syncSymbolsOnly && !Ft8SymbolEncoder.IsSyncSymbol(symbol))
            {
                continue;
            }

            var start = SampleAt(startSeconds + (symbol * Ft8WaterfallGeometry.SymbolPeriodSeconds));
            if (start < 0 || start + length > Length)
            {
                // A symbol whose window falls outside the slot keeps its NaN. The caller turns that
                // into the port's own rule - three zero ratios, meaning no opinion.
                continue;
            }

            for (var tone = 0; tone < tones; tone++)
            {
                var toneOffset = tone * length;
                var sumReal = 0.0;
                var sumImaginary = 0.0;

                for (var n = 0; n < length; n++)
                {
                    var re = _real[start + n];
                    var im = _imaginary[start + n];

                    // The pre-rotation and then the tone's own bin, in one pass.
                    var pr = (re * rotationCos[n]) - (im * rotationSin[n]);
                    var pi = (re * rotationSin[n]) + (im * rotationCos[n]);

                    var tc = _toneCos[toneOffset + n];
                    var ts = _toneSin[toneOffset + n];

                    sumReal += (pr * tc) - (pi * ts);
                    sumImaginary += (pr * ts) + (pi * tc);
                }

                var power = (sumReal * sumReal) + (sumImaginary * sumImaginary);
                decibels[(symbol * tones) + tone] = 10.0 * Math.Log10(1e-12 + power);
            }

            filled++;
        }

        return filled;
    }

    /// <summary>
    /// <b>The Costas sync correlation at a continuous position, in decibels.</b> How much of the
    /// energy in each sync symbol's window sits in the one tone the pattern names, averaged over the
    /// 21 sync symbols that fall inside the slot.
    /// </summary>
    /// <param name="startSeconds">When the frame's first symbol begins, in seconds into the slot.</param>
    /// <param name="frequencyOffsetHz">How far the tones sit from where this baseband was mixed.</param>
    /// <returns>
    /// The mean, or <see cref="double.NegativeInfinity"/> when no sync symbol falls inside the slot
    /// at all - a position with nothing to say rather than a good one.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <b>The statistic is the non-coherent matched-filter gain and it is scale-free.</b> For one
    /// window it is <c>|sum x[n] e^(-j2 pi f n)|^2 / (L sum |x[n]|^2)</c>, which is <b>one</b> when
    /// the window holds nothing but that tone and about <b>1/L</b> when it holds nothing but white
    /// noise, whatever the amplitude. That is what lets one threshold-free comparison rank positions
    /// across candidates of wildly different strength. Textbook detection theory, cited as such; the
    /// three seven-symbol Costas arrays and the frame they sit in are from the QEX paper named in
    /// <c>porting-notes.md</c>, and the arrays themselves are the port's
    /// <c>Ft8Tables.Ft8CostasPattern</c>.
    /// </para>
    /// <para>
    /// <b>One tone per symbol rather than eight, and that is what makes the search affordable.</b>
    /// A sync symbol is known, so seven of the eight correlations would be measuring noise at a cost
    /// of seven eighths of the work. Reading them would give a slightly different statistic and a
    /// search grid a sixth the size, which is the worse trade.
    /// </para>
    /// <para>
    /// <b>Averaged rather than summed</b>, so a position near either end of the slot, where fewer
    /// sync symbols are inside it, is not scored down for having less evidence - which would bias
    /// every search toward the middle of the slot.
    /// </para>
    /// </remarks>
    public double SyncScore(double startSeconds, double frequencyOffsetHz)
    {
        var length = SamplesPerSymbol;
        var costas = Ft8Tables.Ft8CostasPattern;

        Span<double> rotationCos = stackalloc double[length];
        Span<double> rotationSin = stackalloc double[length];
        var step = (-2.0 * Math.PI * frequencyOffsetHz / RateHz) + (2.0 * Math.PI * 3.5 / length);
        for (var n = 0; n < length; n++)
        {
            (rotationSin[n], rotationCos[n]) = Math.SinCos(step * n);
        }

        var total = 0.0;
        var counted = 0;

        for (var block = 0; block < Ft8SymbolEncoder.SyncBlockCount; block++)
        {
            var blockStart = Ft8SymbolEncoder.SyncBlockStart(block);

            for (var k = 0; k < Ft8SymbolEncoder.SyncBlockLength; k++)
            {
                var symbol = blockStart + k;
                var start = SampleAt(startSeconds + (symbol * Ft8WaterfallGeometry.SymbolPeriodSeconds));
                if (start < 0 || start + length > Length)
                {
                    continue;
                }

                // costas[k] is a TONE index, which is what the port's ScoreAt adds to its bin
                // offset. No Gray map is involved: a sync symbol is a tone, not a value.
                var toneOffset = costas[k] * length;
                var sumReal = 0.0;
                var sumImaginary = 0.0;
                var energy = 0.0;

                for (var n = 0; n < length; n++)
                {
                    var re = _real[start + n];
                    var im = _imaginary[start + n];
                    energy += (re * re) + (im * im);

                    var pr = (re * rotationCos[n]) - (im * rotationSin[n]);
                    var pi = (re * rotationSin[n]) + (im * rotationCos[n]);

                    var tc = _toneCos[toneOffset + n];
                    var ts = _toneSin[toneOffset + n];

                    sumReal += (pr * tc) - (pi * ts);
                    sumImaginary += (pr * ts) + (pi * tc);
                }

                if (energy <= 0.0)
                {
                    continue;
                }

                var gain = ((sumReal * sumReal) + (sumImaginary * sumImaginary)) / (length * energy);
                total += 10.0 * Math.Log10(gain + 1e-15);
                counted++;
            }
        }

        return counted == 0 ? double.NegativeInfinity : total / counted;
    }

    /// <summary>
    /// The baseband sample a time in seconds from the start of the slot lands on, rounded to the
    /// nearest.
    /// </summary>
    /// <remarks>
    /// <b>The residual is at most half a baseband sample</b> - 1 ms at the default settings, one
    /// hundred and sixtieth of a symbol - and it is the only quantisation this library imposes on a
    /// position it was told. It is stated rather than hidden because a fine search that claims to
    /// resolve below it is claiming more than the representation allows.
    /// </remarks>
    public int SampleAt(double seconds) => (int)Math.Round(seconds * RateHz);

    /// <summary>
    /// <b>A Blackman-windowed sinc low-pass.</b> Textbook: the ideal low-pass impulse response
    /// truncated to <paramref name="length"/> taps and tapered by a Blackman window, then normalised
    /// to unit gain at direct current.
    /// </summary>
    /// <remarks>
    /// Blackman rather than Hann because the stopband has to hold down whatever else is in a 3 kHz
    /// slice of a crowded band before it folds on top of the candidate: about 58 dB against Hann's
    /// 44, at the price of a transition band about 5.5 rather than 3.1 sample-rates over the length.
    /// </remarks>
    public static double[] BuildLowPass(int length, double cutoffHz, int sampleRate)
    {
        var taps = new double[length];
        var centre = (length - 1) / 2.0;
        var normalised = cutoffHz / sampleRate;
        var sum = 0.0;

        for (var i = 0; i < length; i++)
        {
            var offset = i - centre;
            var sinc = Math.Abs(offset) < 1e-12
                ? 2.0 * normalised
                : Math.Sin(2.0 * Math.PI * normalised * offset) / (Math.PI * offset);

            var phase = 2.0 * Math.PI * i / (length - 1);
            var window = 0.42 - (0.5 * Math.Cos(phase)) + (0.08 * Math.Cos(2.0 * phase));

            taps[i] = sinc * window;
            sum += taps[i];
        }

        for (var i = 0; i < length; i++)
        {
            taps[i] /= sum;
        }

        return taps;
    }

    /// <summary>
    /// Multiplies the audio by <c>exp(-j 2 pi f t)</c>, exactly, without a trigonometric call per
    /// sample.
    /// </summary>
    /// <remarks>
    /// The sample index is split as <c>i = q x 1024 + r</c> and the two rotations are looked up and
    /// multiplied. <b>A running rotation would be cheaper and would drift</b>: 180000 successive
    /// complex multiplications accumulate both amplitude and phase error, and a phase that walks
    /// across a slot is exactly the fault this library exists to remove.
    /// </remarks>
    private static (double[] Real, double[] Imaginary) Mix(
        ReadOnlySpan<float> samples, int sampleRate, double centreFrequencyHz)
    {
        var real = new double[samples.Length];
        var imaginary = new double[samples.Length];

        if (samples.Length == 0)
        {
            return (real, imaginary);
        }

        var step = -2.0 * Math.PI * centreFrequencyHz / sampleRate;

        var fineCos = new double[PhaseBlock];
        var fineSin = new double[PhaseBlock];
        for (var r = 0; r < PhaseBlock; r++)
        {
            (fineSin[r], fineCos[r]) = Math.SinCos(step * r);
        }

        var blocks = ((samples.Length - 1) / PhaseBlock) + 1;
        var coarseCos = new double[blocks];
        var coarseSin = new double[blocks];
        for (var q = 0; q < blocks; q++)
        {
            (coarseSin[q], coarseCos[q]) = Math.SinCos(step * (double)q * PhaseBlock);
        }

        for (var i = 0; i < samples.Length; i++)
        {
            var q = i / PhaseBlock;
            var r = i - (q * PhaseBlock);
            var cos = (coarseCos[q] * fineCos[r]) - (coarseSin[q] * fineSin[r]);
            var sin = (coarseSin[q] * fineCos[r]) + (coarseCos[q] * fineSin[r]);

            real[i] = samples[i] * cos;
            imaginary[i] = samples[i] * sin;
        }

        return (real, imaginary);
    }
}
