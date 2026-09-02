using System;

namespace Ft8Sharp.Dsp;

/// <summary>
/// Every extent of the waterfall, derived from a sample rate, a passband and two oversampling
/// factors — <b>by upstream's own arithmetic, in upstream's own precision.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>The single precision here is not an accident and must not be "improved".</b> Upstream's
/// <c>monitor_init</c> holds the symbol period in a <c>float</c> and computes the block size, the
/// first bin and the last bin by multiplying it and truncating. <c>0.160f</c> is not 0.160 — it is
/// 0.1599999964237213 — and the whole geometry turns on which way each of those products falls when
/// the fraction is discarded:
/// </para>
/// <code>
///                       in float (upstream, and this)   in double ("more accurate")
///   block size          12000 * 0.160f -> 1920.0f -> 1920      1919.99995708 -> 1919
///   first kept bin        200 * 0.160f ->   32.0f ->   32        31.99999928 ->   31
///   last kept bin        3000 * 0.160f ->  480.0f ->  481       479.99998927 ->  480
/// </code>
/// <para>
/// A block one sample short would misalign every symbol after the first; a first bin one lower would
/// shift every frequency this library reports by 6.25 Hz, which is one whole FT8 tone. <b>This is
/// unit 212's lesson arriving again on the receive side</b>: that unit measured its waveform
/// agreeing with upstream to one count <em>because</em> it kept the phase step in single precision as
/// upstream does, while its own more accurate double-precision version drifted to 117 counts. A port
/// that is better than upstream is a port that disagrees with it.
/// <c>Ft8WaterfallGeometryTests</c> computes both columns above and prints them rather than leaving
/// this paragraph to be believed.
/// </para>
/// <para>
/// <b>What is refused, and it is a divergence from upstream.</b> Upstream truncates whatever the
/// products give it and carries on. Two of those truncations are silent corruption rather than
/// approximation, so they are refused here with the reason — see
/// <see cref="Ft8WaterfallGeometry(int, float, float, int, int)"/>. Recorded in
/// <c>porting-notes.md</c>.
/// </para>
/// </remarks>
public sealed class Ft8WaterfallGeometry
{
    /// <summary>
    /// The FT8 symbol period in seconds, <b>as a single-precision value</b> because that is what
    /// every product below depends on. The protocol's published figure, the same one
    /// <c>Ft8Waveform.SymbolPeriodSeconds</c> carries; held separately so that nothing on the
    /// receive side depends on the transmit side.
    /// </summary>
    public const float SymbolPeriodSeconds = 0.160f;

    /// <summary>The FT8 slot in seconds, single precision for the same reason.</summary>
    public const float SlotSeconds = 15.0f;

    /// <summary>The rate the FT8 world decodes at, and the one every figure here was read at.</summary>
    public const int DefaultSampleRate = 12000;

    /// <summary>The passband upstream's own decoder analyses. Its choice, not the library's.</summary>
    public const float DefaultMinFrequencyHz = 200.0f;

    /// <summary>The upper end of that passband.</summary>
    public const float DefaultMaxFrequencyHz = 3000.0f;

    /// <summary>Upstream's time oversampling factor: two analyses per symbol.</summary>
    public const int DefaultTimeOversampling = 2;

    /// <summary>Upstream's frequency oversampling factor: two bins per tone spacing.</summary>
    public const int DefaultFrequencyOversampling = 2;

    /// <summary>Builds the geometry, or refuses it.</summary>
    /// <param name="sampleRate">Samples per second of the audio to be analysed.</param>
    /// <param name="minFrequencyHz">The low end of the passband kept in the waterfall.</param>
    /// <param name="maxFrequencyHz">The high end of the passband kept in the waterfall.</param>
    /// <param name="timeOversampling">Analyses per symbol.</param>
    /// <param name="frequencyOversampling">Bins per tone spacing.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A factor is below one, or the passband is empty or inverted.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <b>The sample rate does not divide the geometry.</b> Two shapes, both refused rather than
    /// truncated, and both of them divergences from upstream:
    /// <list type="number">
    /// <item>
    /// The rate times the symbol period is not a whole number of samples, so a block would not cover
    /// exactly one symbol and every symbol after the first would sit at the wrong offset by a
    /// growing amount.
    /// </item>
    /// <item>
    /// The block does not divide by the time oversampling factor. This one is the dangerous one:
    /// the analysis consumes <c>timeOversampling * subblock</c> samples from a block while the
    /// caller advances by the whole block, so a remainder is <em>audio that is silently never
    /// looked at</em>. Upstream inherits this because at 12 kHz there is no remainder.
    /// </item>
    /// </list>
    /// This is the same reasoning as divergence 16 in <c>porting-notes.md</c>: an inconsistency that
    /// cannot arise at the rate upstream uses, and that would be reported as a defect in the
    /// analysis if it ever did.
    /// </exception>
    public Ft8WaterfallGeometry(
        int sampleRate = DefaultSampleRate,
        float minFrequencyHz = DefaultMinFrequencyHz,
        float maxFrequencyHz = DefaultMaxFrequencyHz,
        int timeOversampling = DefaultTimeOversampling,
        int frequencyOversampling = DefaultFrequencyOversampling)
    {
        if (sampleRate < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sampleRate), sampleRate, "A sample rate must be positive.");
        }

        if (timeOversampling < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(timeOversampling),
                timeOversampling,
                "The time oversampling factor is analyses per symbol and must be at least one.");
        }

        if (frequencyOversampling < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(frequencyOversampling),
                frequencyOversampling,
                "The frequency oversampling factor is bins per tone spacing and must be at least one.");
        }

        if (!(maxFrequencyHz > minFrequencyHz) || minFrequencyHz < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxFrequencyHz),
                maxFrequencyHz,
                $"The passband must be non-negative and increasing; it was given as {minFrequencyHz} "
                + $"to {maxFrequencyHz} Hz. An empty or inverted passband has no bins to keep.");
        }

        // Single precision, deliberately. See the remarks on this type.
        var exactBlock = (float)(sampleRate * SymbolPeriodSeconds);
        BlockSize = (int)exactBlock;

        if (BlockSize < 1 || Math.Abs(exactBlock - BlockSize) > 0)
        {
            throw new ArgumentException(
                $"At {sampleRate} Hz a symbol is {exactBlock} samples, which is not a whole number. "
                + "A block that does not cover exactly one symbol puts every symbol after the first "
                + "at a growing offset, so the rate is refused rather than truncated to "
                + $"{BlockSize}. FT8 is decoded at {DefaultSampleRate} Hz, where a symbol is exactly "
                + $"{(int)(DefaultSampleRate * SymbolPeriodSeconds)} samples.",
                nameof(sampleRate));
        }

        if (BlockSize % timeOversampling != 0)
        {
            throw new ArgumentException(
                $"A block of {BlockSize} samples does not divide into {timeOversampling} analyses. "
                + $"The analysis would consume {BlockSize / timeOversampling * timeOversampling} "
                + $"samples of every block and the remaining {BlockSize % timeOversampling} would "
                + "never be looked at — audio dropped silently rather than analysed. Refused.",
                nameof(timeOversampling));
        }

        SampleRate = sampleRate;
        MinFrequencyHz = minFrequencyHz;
        MaxFrequencyHz = maxFrequencyHz;
        TimeOversampling = timeOversampling;
        FrequencyOversampling = frequencyOversampling;

        SubblockSize = BlockSize / timeOversampling;
        TransformLength = BlockSize * frequencyOversampling;
        MaxBlocks = (int)(float)(SlotSeconds / SymbolPeriodSeconds);
        MinBin = (int)(float)(minFrequencyHz * SymbolPeriodSeconds);
        MaxBin = (int)(float)(maxFrequencyHz * SymbolPeriodSeconds) + 1;
        BinCount = MaxBin - MinBin;
        BlockStride = timeOversampling * frequencyOversampling * BinCount;

        if (BinCount < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minFrequencyHz),
                minFrequencyHz,
                $"The passband {minFrequencyHz}..{maxFrequencyHz} Hz keeps {BinCount} bins at "
                + $"{sampleRate} Hz. A waterfall with no bins holds nothing.");
        }

        // The transform's own bin spacing. The identity below is what makes a waterfall bin index
        // mean a frequency, and it is checked here rather than assumed: sampleRate / transformLength
        // equals the tone spacing divided by the frequency oversampling factor, because the
        // transform is exactly frequencyOversampling symbols long.
        TransformBinSpacingHz = (double)sampleRate / TransformLength;
    }

    /// <summary>Samples per second of the audio analysed.</summary>
    public int SampleRate { get; }

    /// <summary>The low end of the kept passband, as configured.</summary>
    public float MinFrequencyHz { get; }

    /// <summary>The high end of the kept passband, as configured.</summary>
    public float MaxFrequencyHz { get; }

    /// <summary>Analyses per symbol.</summary>
    public int TimeOversampling { get; }

    /// <summary>Bins per tone spacing.</summary>
    public int FrequencyOversampling { get; }

    /// <summary>Samples in one symbol, which is one waterfall block. 1920 at 12 kHz.</summary>
    public int BlockSize { get; }

    /// <summary>Samples the analysis frame advances between transforms. 960 at 12 kHz.</summary>
    public int SubblockSize { get; }

    /// <summary>Points in one transform. 3840 at 12 kHz, and not a power of two.</summary>
    public int TransformLength { get; }

    /// <summary>Blocks a whole slot holds. 93 for FT8.</summary>
    public int MaxBlocks { get; }

    /// <summary>The first transform bin kept, counted in tone spacings. 32 at 12 kHz.</summary>
    public int MinBin { get; }

    /// <summary>One past the last transform bin kept, counted in tone spacings. 481 at 12 kHz.</summary>
    public int MaxBin { get; }

    /// <summary>Bins kept per frequency sub-offset. 449 at 12 kHz.</summary>
    public int BinCount { get; }

    /// <summary>Magnitudes from one block to the next.</summary>
    public int BlockStride { get; }

    /// <summary>Hertz between adjacent bins of the underlying transform. 3.125 Hz at 12 kHz.</summary>
    public double TransformBinSpacingHz { get; }

    /// <summary>Hertz between adjacent waterfall bins at the same sub-offset — the tone spacing.</summary>
    public double ToneSpacingHz => 1.0 / SymbolPeriodSeconds;

    /// <summary>Magnitudes a whole slot's waterfall holds.</summary>
    public int MagnitudeCount => MaxBlocks * BlockStride;

    /// <summary>
    /// The centre frequency of a waterfall bin at a frequency sub-offset, in hertz.
    /// </summary>
    /// <remarks>
    /// Upstream's own expression, from <c>decode_ft8.c</c>: the bin index counted from zero, plus
    /// the first kept bin, plus the sub-offset as a fraction of a bin, all divided by the symbol
    /// period. Dividing by the symbol period is multiplying by the tone spacing.
    /// </remarks>
    public double FrequencyHz(int bin, int frequencySubOffset) =>
        (MinBin + bin + ((double)frequencySubOffset / FrequencyOversampling)) / SymbolPeriodSeconds;

    /// <summary>The index into the underlying transform of a waterfall bin at a sub-offset.</summary>
    public int TransformBin(int bin, int frequencySubOffset) =>
        ((MinBin + bin) * FrequencyOversampling) + frequencySubOffset;

    /// <summary>
    /// The time a block at a time sub-offset begins, in seconds from the start of the analysis.
    /// </summary>
    /// <remarks>
    /// Upstream's own expression, from <c>decode_ft8.c</c>. <b>It is the block's nominal position and
    /// not the centre of the window that produced it</b> — the analysis frame is prefilled with
    /// zeros and slides, so the samples behind a block reach back before it. Task 2 could not settle
    /// the exact alignment by reading and it is not asserted as one here.
    /// </remarks>
    public double TimeSeconds(int block, int timeSubOffset) =>
        (block + ((double)timeSubOffset / TimeOversampling)) * SymbolPeriodSeconds;

    /// <summary>The nearest waterfall bin to a frequency, and its sub-offset.</summary>
    /// <returns>
    /// False when the frequency is outside the kept passband, in which case the outputs are the
    /// nearest edge. <b>It reports rather than throws</b>, because "outside the passband" is an
    /// ordinary answer for a caller scanning frequencies and not a programming error.
    /// </returns>
    public bool TryBinFor(double frequencyHz, out int bin, out int frequencySubOffset)
    {
        var subBins = (int)Math.Round(frequencyHz * SymbolPeriodSeconds * FrequencyOversampling);
        var relative = subBins - (MinBin * FrequencyOversampling);

        bin = Math.Clamp(
            (int)Math.Floor((double)relative / FrequencyOversampling), 0, BinCount - 1);
        frequencySubOffset = Math.Clamp(
            relative - (bin * FrequencyOversampling), 0, FrequencyOversampling - 1);

        return relative >= 0 && relative < BinCount * FrequencyOversampling;
    }
}
