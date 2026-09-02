using System;
using Ft8Sharp.Encode;
using Ft8Sharp.Ldpc;

namespace Ft8Sharp.Dsp;

/// <summary>
/// The join: a waterfall and a candidate in, <b>174 log-likelihood ratios out.</b> Ported from
/// <c>ft8_extract_likelihood</c>, <c>ft8_extract_symbol</c> and <c>ftx_normalize_logl</c> in
/// <c>ft8/decode.c</c>.
/// </summary>
/// <remarks>
/// <para>
/// <b>This is the piece that was missing.</b> Since unit 214 this library could say <em>where</em> a
/// transmission is; since unit 215 it could repair a damaged codeword and refuse one it cannot. The
/// two never met, because nothing turned magnitudes into ratios. This does, and everything above it
/// in <c>Ft8SlotDecoder</c> is a wiring-up of parts that were already proved.
/// </para>
/// <para>
/// <b>THE ALIGNMENT, AND WHY IT CANNOT DRIFT FROM THE SEARCH.</b> Upstream's scorer and its
/// extraction both open with the same helper, <c>get_cand_mag</c>, which folds a candidate's four
/// position fields into one offset in the store's own axis order. So the blocks extraction reads are
/// by construction the blocks the search scored. This port keeps that property by the same means:
/// both go through <see cref="Ft8Waterfall.IndexOf"/> with the candidate's own fields and nothing
/// else. If the two ever disagreed by one block or one sub-offset, the sync tones would still
/// correlate and nothing would ever decode — a failure that reads as a broken decoder rather than as
/// an off-by-one, which is why it is closed structurally here rather than checked once.
/// </para>
/// <para>
/// <b>The sync blocks are stepped over, not through.</b> Data symbol <c>k</c> of 58 is channel
/// symbol <c>k + 7</c> for the first twenty-nine and <c>k + 14</c> for the rest — the 7/29/7/29/7
/// layout. That layout is <b>not laid out a second time here</b>: it is taken from
/// <see cref="Ft8SymbolEncoder.IsSyncSymbol"/>, which step 3 proved against upstream's own encoder,
/// and <c>UpstreamExtractionInventoryTests</c> asserts the two agree on all 58.
/// </para>
/// <para>
/// <b>The magnitudes are read as decibels and gathered in VALUE order.</b> Upstream's scorer reads
/// the raw stored byte (<c>WF_ELEM_MAG_INT</c>); extraction reads it as decibels
/// (<c>WF_ELEM_MAG</c>) — two different reads of the same store, half a decibel per count apart, and
/// both are kept. The eight magnitudes are gathered as <c>s2[value] = decibels of tone
/// GrayMap[value]</c>, which is the <em>forward</em> map from
/// <see cref="Ft8Tables.Ft8GrayMap"/>. <b>No inverse map exists in upstream's decoder and none is
/// built here</b>; unit 216's instruction expected one and that is recorded as a mismatch.
/// </para>
/// <para>
/// <b>Positive means the bit is one.</b> Unit 215 settled that from three independent readings of
/// upstream's source and recorded it in <see cref="LdpcDecoder"/>'s remarks and in
/// <c>porting-notes.md</c>. It is not re-argued here; extraction conforms to it, and each ratio is
/// the largest magnitude among the four values whose bit is one minus the largest among the four
/// whose bit is zero.
/// </para>
/// <para>
/// <b>The normalisation is a separate, callable step and it is not optional.</b> Unit 215 measured
/// that this decoder is <em>not</em> scale-free: <c>fast_tanh</c> and its clamp are not homogeneous,
/// so multiplying every ratio by a constant changes the answer. Upstream rescales all 174 to a fixed
/// variance between extraction and the decoder, and skipping it because something decoded without it
/// is the exact failure that would not show up until the sensitivity measurement.
/// </para>
/// <para>
/// <b>Nothing here depends on ambient state.</b> No clock, no random source, no environment, no
/// parallelism, no dictionary. Every buffer is the caller's or a local.
/// </para>
/// <para>
/// <b>And it takes a waterfall and a candidate and nothing else.</b> No message, no expected
/// symbols, no frequency or time hint, no truth of any kind. A function with a truth parameter
/// cannot be shown not to have used it, so the parameter does not exist — the same prohibition units
/// 214 and 215 worked under, one layer up.
/// </para>
/// </remarks>
public static class Ft8SoftSymbols
{
    /// <summary>How many log-likelihood ratios one transmission carries.</summary>
    public const int RatioCount = LdpcDecoder.RatioCount;

    /// <summary>
    /// The variance upstream rescales every set of ratios to before the decoder sees them.
    /// </summary>
    /// <remarks>
    /// <b>A weak anchor, and it is labelled as one.</b> Upstream's own comment beside it calls it an
    /// "experimentally found coefficient": it is one number chosen by measurement rather than
    /// derived from anything, it lives in a function body in <c>ft8/decode.c</c>, and it is asserted
    /// against the pin by <c>Ft8SoftSymbolsProvenanceTests</c> rather than trusted.
    /// </remarks>
    public const float NormalisedVariance = 24.0f;

    /// <summary>
    /// Reads 174 log-likelihood ratios out of a waterfall at a candidate's position.
    /// </summary>
    /// <param name="waterfall">The slot's spectrogram.</param>
    /// <param name="candidate">Where in it to read. Its block offset may be negative.</param>
    /// <param name="ratios">
    /// Exactly <see cref="RatioCount"/> ratios, written in codeword bit order. <b>Positive means the
    /// bit is more likely one.</b> Unnormalised — see <see cref="Normalise"/>.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="waterfall"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="ratios"/> is not <see cref="RatioCount"/> long, or the candidate's eight
    /// tones do not all fall inside the waterfall's bins.
    /// </exception>
    /// <remarks>
    /// <para>
    /// <b>A symbol whose block falls outside the waterfall contributes three zero ratios</b>, which
    /// is upstream's rule and is not the same as refusing the candidate. A zero ratio is <em>no
    /// opinion</em>: the decoder is told nothing about that bit and the code's redundancy is left to
    /// supply it. The search deliberately sweeps time offsets from ten blocks before the slot, so a
    /// transmission that began early is found with some of its symbols off the front, and refusing
    /// those candidates would throw away exactly the ones the sweep exists to catch.
    /// </para>
    /// <para>
    /// <b>The frequency bound is a refusal, and that IS a divergence.</b> Upstream does not check
    /// the bin at all here — it relies on its own search never proposing a candidate whose eighth
    /// tone leaves the passband, and a hand-built candidate would read past the end of its array.
    /// There is no faithful port of reading past the end of an array, so this refuses, with both
    /// numbers in the message. Recorded as divergence 22 in <c>porting-notes.md</c>.
    /// </para>
    /// </remarks>
    public static void Extract(
        Ft8Waterfall waterfall,
        Ft8Candidate candidate,
        Span<float> ratios)
    {
        ArgumentNullException.ThrowIfNull(waterfall);

        if (ratios.Length != RatioCount)
        {
            throw new ArgumentException(
                $"A transmission carries {RatioCount} log-likelihood ratios and a span of "
                + $"{ratios.Length} was given. Refused rather than filled as far as it goes: a short "
                + "span would hand the decoder a codeword whose tail is whatever was in the buffer.",
                nameof(ratios));
        }

        var geometry = waterfall.Geometry;
        var topTone = candidate.BinOffset + Ft8SymbolEncoder.ToneCount - 1;
        if (candidate.BinOffset < 0 || topTone >= geometry.BinCount)
        {
            throw new ArgumentException(
                $"A candidate at bin {candidate.BinOffset} needs bins "
                + $"{candidate.BinOffset}..{topTone} and this waterfall keeps {geometry.BinCount}. "
                + "Its eighth tone falls outside the passband, so there is nothing there to read. "
                + "Ft8SyncSearch never proposes such a candidate; upstream does not check, and "
                + "reading past the end of the store is not something to port faithfully.",
                nameof(candidate));
        }

        if (candidate.TimeSubOffset < 0 || candidate.TimeSubOffset >= geometry.TimeOversampling
            || candidate.FrequencySubOffset < 0
            || candidate.FrequencySubOffset >= geometry.FrequencyOversampling)
        {
            throw new ArgumentException(
                $"A candidate at time sub-offset {candidate.TimeSubOffset} and frequency sub-offset "
                + $"{candidate.FrequencySubOffset} does not fit a waterfall with "
                + $"{geometry.TimeOversampling} time subdivisions and "
                + $"{geometry.FrequencyOversampling} frequency subdivisions.",
                nameof(candidate));
        }

        Span<double> magnitudes = stackalloc double[Ft8SymbolEncoder.ToneCount];
        var gray = Ft8Tables.Ft8GrayMap;

        var bit = 0;
        for (var symbol = 0; symbol < Ft8SymbolEncoder.SymbolCount; symbol++)
        {
            // The sync blocks are stepped OVER. The layout is Ft8SymbolEncoder's and is not laid
            // out a second time here.
            if (Ft8SymbolEncoder.IsSyncSymbol(symbol))
            {
                continue;
            }

            var block = candidate.BlockOffset + symbol;
            if (block < 0 || block >= waterfall.BlockCount)
            {
                // Upstream's rule: three zeros, meaning no opinion about these three bits.
                ratios[bit] = 0.0f;
                ratios[bit + 1] = 0.0f;
                ratios[bit + 2] = 0.0f;
                bit += Ft8SymbolEncoder.BitsPerSymbol;
                continue;
            }

            // Gathered in VALUE order through the FORWARD Gray map: magnitudes[v] is the strength of
            // the tone that would have carried symbol value v.
            for (var value = 0; value < Ft8SymbolEncoder.ToneCount; value++)
            {
                magnitudes[value] = waterfall.DecibelsAt(
                    block,
                    candidate.TimeSubOffset,
                    candidate.FrequencySubOffset,
                    candidate.BinOffset + gray[value]);
            }

            ExtractSymbol(magnitudes, ratios.Slice(bit, Ft8SymbolEncoder.BitsPerSymbol));
            bit += Ft8SymbolEncoder.BitsPerSymbol;
        }
    }

    /// <summary>
    /// One symbol's three ratios, from its eight tone magnitudes in symbol-value order.
    /// </summary>
    /// <param name="magnitudes">
    /// Eight magnitudes in decibels, indexed by symbol value rather than by tone.
    /// </param>
    /// <param name="ratios">Three ratios, most significant bit of the value first.</param>
    /// <exception cref="ArgumentException">Either span is the wrong length.</exception>
    /// <remarks>
    /// <b>The partition is derived, not transcribed.</b> Upstream writes the three lines out with
    /// their eight indices spelled in full; the same partition is <c>bit i of the value</c>, so this
    /// tests the bit and upstream's three lines fall out of it. <c>UpstreamExtractionInventoryTests</c>
    /// asserts that the derived partition equals upstream's written one, term for term, so the
    /// derivation is checked against the pin rather than believed.
    /// </remarks>
    public static void ExtractSymbol(ReadOnlySpan<double> magnitudes, Span<float> ratios)
    {
        if (magnitudes.Length != Ft8SymbolEncoder.ToneCount)
        {
            throw new ArgumentException(
                $"A symbol has {Ft8SymbolEncoder.ToneCount} tone magnitudes and "
                + $"{magnitudes.Length} were given.",
                nameof(magnitudes));
        }

        if (ratios.Length != Ft8SymbolEncoder.BitsPerSymbol)
        {
            throw new ArgumentException(
                $"A symbol carries {Ft8SymbolEncoder.BitsPerSymbol} bits and a span of "
                + $"{ratios.Length} was given.",
                nameof(ratios));
        }

        for (var bit = 0; bit < Ft8SymbolEncoder.BitsPerSymbol; bit++)
        {
            var shift = Ft8SymbolEncoder.BitsPerSymbol - 1 - bit;
            var bestOne = double.NegativeInfinity;
            var bestZero = double.NegativeInfinity;

            for (var value = 0; value < Ft8SymbolEncoder.ToneCount; value++)
            {
                var magnitude = magnitudes[value];
                if (((value >> shift) & 1) == 1)
                {
                    if (magnitude > bestOne)
                    {
                        bestOne = magnitude;
                    }
                }
                else if (magnitude > bestZero)
                {
                    bestZero = magnitude;
                }
            }

            // Positive means the bit is one: the best evidence for one, less the best for zero.
            ratios[bit] = (float)(bestOne - bestZero);
        }
    }

    /// <summary>
    /// Upstream's <c>ftx_normalize_logl</c>: rescales all 174 ratios to
    /// <see cref="NormalisedVariance"/>.
    /// </summary>
    /// <param name="ratios">The ratios, rewritten in place.</param>
    /// <returns>The variance the ratios had before the rescale.</returns>
    /// <exception cref="ArgumentException"><paramref name="ratios"/> is the wrong length.</exception>
    /// <remarks>
    /// <para>
    /// <b>The mean is removed from the variance and NOT from the ratios.</b> Upstream computes the
    /// population variance — <c>(sum2 - sum*sum/N)/N</c> — and then multiplies every ratio by
    /// <c>sqrt(target / variance)</c>. It never subtracts the mean from the array, and a port that
    /// centred the ratios would be shifting every bit's evidence toward zero or one.
    /// </para>
    /// <para>
    /// <b>Why it is required rather than a refinement.</b> The belief propagation in
    /// <see cref="LdpcDecoder"/> is not scale-free: it applies <c>fast_tanh</c>, a rational
    /// approximation with a hard clamp, so scaling every input by a constant is not the same
    /// experiment. Unit 215 measured its soft sweep's arrays leaving upstream's scale as the noise
    /// grew and printed the variance beside every row for exactly this reason.
    /// </para>
    /// <para>
    /// <b>A degenerate case upstream does not guard and this does.</b> If every ratio is identical —
    /// the variance is zero — upstream divides by zero, gets an infinity or a NaN, and multiplies
    /// the whole array by it. That cannot arise from a real waterfall and it can arise from a
    /// synthetic one, so this leaves such an array untouched and returns the zero variance. Recorded
    /// as divergence 23 in <c>porting-notes.md</c>.
    /// </para>
    /// </remarks>
    public static float Normalise(Span<float> ratios)
    {
        if (ratios.Length != RatioCount)
        {
            throw new ArgumentException(
                $"The normalisation rescales all {RatioCount} ratios together and a span of "
                + $"{ratios.Length} was given. Rescaling a subset would give the decoder a codeword "
                + "whose parts are on different scales.",
                nameof(ratios));
        }

        var variance = Variance(ratios);
        if (!(variance > 0.0f))
        {
            return variance;
        }

        var factor = MathF.Sqrt(NormalisedVariance / variance);
        for (var i = 0; i < ratios.Length; i++)
        {
            ratios[i] *= factor;
        }

        return variance;
    }

    /// <summary>
    /// The population variance of a set of ratios, by upstream's own arithmetic and in upstream's
    /// own precision.
    /// </summary>
    /// <remarks>
    /// <b>Single precision throughout, deliberately.</b> Upstream accumulates both sums in
    /// <c>float</c>. Accumulating in <c>double</c> would be more accurate and would be a different
    /// number from the one the decoder's clamp was tuned against — unit 212's lesson, and the same
    /// reasoning that keeps <see cref="Ft8WaterfallGeometry"/> in single precision.
    /// </remarks>
    public static float Variance(ReadOnlySpan<float> ratios)
    {
        var sum = 0.0f;
        var sumOfSquares = 0.0f;
        for (var i = 0; i < ratios.Length; i++)
        {
            sum += ratios[i];
            sumOfSquares += ratios[i] * ratios[i];
        }

        var inverseCount = 1.0f / ratios.Length;
        return (sumOfSquares - (sum * sum * inverseCount)) * inverseCount;
    }

    /// <summary>
    /// The hard decision a set of ratios makes, one byte per bit — what the decoder starts from
    /// before a single message is passed.
    /// </summary>
    /// <param name="ratios">The ratios.</param>
    /// <param name="bits">One byte per bit, zero or one.</param>
    /// <exception cref="ArgumentException">The spans are not the same length.</exception>
    /// <remarks>
    /// Upstream's own hard decision, <c>(l &gt; 0) ? 1 : 0</c>, which appears in both of its
    /// decoders. <b>Exactly at zero is a zero</b>, so a bit nothing was said about reads as zero
    /// rather than as one. This exists so the join can be measured before any correction is
    /// involved: how many of 174 hard decisions match the codeword that was sent says whether
    /// extraction is right, separately from whether the code can repair it.
    /// </remarks>
    public static void HardDecision(ReadOnlySpan<float> ratios, Span<byte> bits)
    {
        if (ratios.Length != bits.Length)
        {
            throw new ArgumentException(
                $"{ratios.Length} ratios make {ratios.Length} decisions and a span of {bits.Length} "
                + "was given.",
                nameof(bits));
        }

        for (var i = 0; i < ratios.Length; i++)
        {
            bits[i] = ratios[i] > 0.0f ? (byte)1 : (byte)0;
        }
    }
}
