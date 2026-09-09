using System;
using Ft8Sharp.Encode;
using Ft8Sharp.Ldpc;

namespace Ft8Sharp.Dsp;

/// <summary>
/// The FT4 join: a waterfall and a candidate in, <b>174 log-likelihood ratios out.</b> Ported from
/// <c>ft4_extract_likelihood</c> and <c>ft4_extract_symbol</c> in <c>ft8/decode.c</c>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Two bits per symbol over four magnitude bins, not three over eight.</b> That is the whole of
/// the arithmetic difference from <see cref="Ft8SoftSymbols"/>, and it changes the shape of the
/// extraction rather than a constant in it: 87 data symbols carry 174 bits exactly, where FT8's 58
/// carry the same 174 in threes.
/// </para>
/// <para>
/// <b>The sync skip is 5, then 9, then 13, and it is not 7-then-14.</b> Upstream writes
/// <c>sym_idx = k + ((k &lt; 29) ? 5 : ((k &lt; 58) ? 9 : 13))</c>. The three steps are the ramp and
/// one sync group, then two groups, then three — because FT4's layout is
/// <c>R S4 D29 S4 D29 S4 D29 S4 R</c> and a data symbol has passed one more group each time. FT8's
/// two steps are 7 and 14 for the same reason with one fewer group. <b>This library does not lay
/// that out a second time</b>: the skip is taken from <see cref="Ft4SymbolEncoder"/>'s own layout,
/// which task 3 proved against upstream's generator symbol for symbol, and the derived sequence is
/// held against upstream's three literals by <c>Ft4SoftSymbolsTests</c>.
/// </para>
/// <para>
/// <b>The alignment cannot drift from the search.</b> Both this and <see cref="Ft4SyncSearch"/> reach
/// the store through the same index arithmetic on the candidate's own four fields, so the blocks
/// extraction reads are by construction the blocks the search scored. If the two ever disagreed by
/// one block the sync tones would still correlate and nothing would ever decode — a failure that
/// reads as a broken decoder rather than as an off-by-one.
/// </para>
/// <para>
/// <b>The magnitudes are read as decibels and gathered in VALUE order through the FORWARD Gray
/// map</b>, exactly as FT8's are: <c>s2[value]</c> is the strength of the tone that would have
/// carried symbol value <c>value</c>. No inverse map exists in upstream's decoder and none is built
/// here.
/// </para>
/// <para>
/// <b>Positive means the bit is one</b>, the convention <see cref="LdpcDecoder"/> settled and
/// <c>porting-notes.md</c> records. Upstream writes the two lines as
/// <c>max2(s2[2], s2[3]) - max2(s2[0], s2[1])</c> and
/// <c>max2(s2[1], s2[3]) - max2(s2[0], s2[2])</c>; the same partition is <em>bit i of the value</em>,
/// so this tests the bit and upstream's two lines fall out of it.
/// </para>
/// <para>
/// <b>The normalisation is <see cref="Ft8SoftSymbols.Normalise"/>'s and is not written again.</b>
/// <c>ftx_normalize_logl</c> is protocol-neutral in upstream too — one function, called from
/// <c>ftx_decode_candidate</c> after either extractor — and it is not optional: the belief
/// propagation is not scale-free.
/// </para>
/// </remarks>
public static class Ft4SoftSymbols
{
    /// <summary>How many log-likelihood ratios one transmission carries.</summary>
    public const int RatioCount = LdpcDecoder.RatioCount;

    /// <summary>
    /// Reads 174 log-likelihood ratios out of a waterfall at an FT4 candidate's position.
    /// </summary>
    /// <param name="waterfall">The slot's spectrogram.</param>
    /// <param name="candidate">Where in it to read. Its block offset may be negative.</param>
    /// <param name="ratios">
    /// Exactly <see cref="RatioCount"/> ratios, written in codeword bit order. <b>Positive means the
    /// bit is more likely one.</b> Unnormalised — see <see cref="Ft8SoftSymbols.Normalise"/>.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="waterfall"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="ratios"/> is the wrong length, or the candidate's four tones do not all fall
    /// inside the waterfall's bins.
    /// </exception>
    /// <remarks>
    /// <b>A symbol whose block falls outside the waterfall contributes two zero ratios</b>, which is
    /// upstream's rule and is not the same as refusing the candidate. A zero ratio is <em>no
    /// opinion</em>: the decoder is told nothing about that bit and the code's redundancy is left to
    /// supply it.
    /// </remarks>
    public static void Extract(Ft8Waterfall waterfall, Ft8Candidate candidate, Span<float> ratios)
    {
        ArgumentNullException.ThrowIfNull(waterfall);

        if (ratios.Length != RatioCount)
        {
            throw new ArgumentException(
                $"An FT4 transmission carries {RatioCount} log-likelihood ratios and a span of "
                + $"{ratios.Length} was given. Refused rather than filled as far as it goes: a short "
                + "span would hand the decoder a codeword whose tail is whatever was in the buffer.",
                nameof(ratios));
        }

        var geometry = waterfall.Geometry;
        var topTone = candidate.BinOffset + Ft4SymbolEncoder.ToneCount - 1;
        if (candidate.BinOffset < 0 || topTone >= geometry.BinCount)
        {
            throw new ArgumentException(
                $"A candidate at bin {candidate.BinOffset} needs bins "
                + $"{candidate.BinOffset}..{topTone} and this waterfall keeps {geometry.BinCount}. "
                + "Its fourth tone falls outside the passband, so there is nothing there to read. "
                + "Ft4SyncSearch never proposes such a candidate; upstream does not check, and "
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

        Span<double> magnitudes = stackalloc double[Ft4SymbolEncoder.ToneCount];
        var gray = Ft8Tables.Ft4GrayMap;

        var bit = 0;
        for (var symbol = 0; symbol < Ft4SymbolEncoder.SymbolCount; symbol++)
        {
            // The ramps and the four sync groups are stepped OVER. The layout is
            // Ft4SymbolEncoder's and is not laid out a second time here.
            if (!Ft4SymbolEncoder.IsDataSymbol(symbol))
            {
                continue;
            }

            var block = candidate.BlockOffset + symbol;
            if (block < 0 || block >= waterfall.BlockCount)
            {
                // Upstream's rule: two zeros, meaning no opinion about these two bits.
                ratios[bit] = 0.0f;
                ratios[bit + 1] = 0.0f;
                bit += Ft4SymbolEncoder.BitsPerSymbol;
                continue;
            }

            for (var value = 0; value < Ft4SymbolEncoder.ToneCount; value++)
            {
                magnitudes[value] = waterfall.DecibelsAt(
                    block,
                    candidate.TimeSubOffset,
                    candidate.FrequencySubOffset,
                    candidate.BinOffset + gray[value]);
            }

            ExtractSymbol(magnitudes, ratios.Slice(bit, Ft4SymbolEncoder.BitsPerSymbol));
            bit += Ft4SymbolEncoder.BitsPerSymbol;
        }

        if (bit != RatioCount)
        {
            throw new InvalidOperationException(
                $"The FT4 layout produced {bit} ratios and a codeword is {RatioCount}. The symbol "
                + "geometry constants disagree with each other.");
        }
    }

    /// <summary>One symbol's two ratios, from its four tone magnitudes in symbol-value order.</summary>
    /// <param name="magnitudes">Four magnitudes in decibels, indexed by symbol value.</param>
    /// <param name="ratios">Two ratios, most significant bit of the value first.</param>
    /// <exception cref="ArgumentException">Either span is the wrong length.</exception>
    public static void ExtractSymbol(ReadOnlySpan<double> magnitudes, Span<float> ratios)
    {
        if (magnitudes.Length != Ft4SymbolEncoder.ToneCount)
        {
            throw new ArgumentException(
                $"An FT4 symbol has {Ft4SymbolEncoder.ToneCount} tone magnitudes and "
                + $"{magnitudes.Length} were given.",
                nameof(magnitudes));
        }

        if (ratios.Length != Ft4SymbolEncoder.BitsPerSymbol)
        {
            throw new ArgumentException(
                $"An FT4 symbol carries {Ft4SymbolEncoder.BitsPerSymbol} bits and a span of "
                + $"{ratios.Length} was given.",
                nameof(ratios));
        }

        for (var bit = 0; bit < Ft4SymbolEncoder.BitsPerSymbol; bit++)
        {
            var shift = Ft4SymbolEncoder.BitsPerSymbol - 1 - bit;
            var bestOne = double.NegativeInfinity;
            var bestZero = double.NegativeInfinity;

            for (var value = 0; value < Ft4SymbolEncoder.ToneCount; value++)
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

            ratios[bit] = (float)(bestOne - bestZero);
        }
    }

    /// <summary>
    /// The channel symbol index data symbol <paramref name="dataIndex"/> sits at — upstream's
    /// 5-then-9-then-13 skip, derived from the layout rather than transcribed.
    /// </summary>
    /// <remarks>
    /// Exposed so that the derivation can be held against upstream's three literals rather than
    /// believed, and so that a reader can see what the skip <em>is</em> without reading a loop.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">There is no such data symbol.</exception>
    public static int ChannelSymbolForDataSymbol(int dataIndex)
    {
        if (dataIndex < 0 || dataIndex >= Ft4SymbolEncoder.DataSymbolCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dataIndex),
                dataIndex,
                $"An FT4 transmission carries {Ft4SymbolEncoder.DataSymbolCount} data symbols.");
        }

        var seen = 0;
        for (var symbol = 0; symbol < Ft4SymbolEncoder.SymbolCount; symbol++)
        {
            if (!Ft4SymbolEncoder.IsDataSymbol(symbol))
            {
                continue;
            }

            if (seen == dataIndex)
            {
                return symbol;
            }

            seen++;
        }

        throw new InvalidOperationException(
            "The FT4 layout produced fewer data symbols than the geometry says it has.");
    }
}
