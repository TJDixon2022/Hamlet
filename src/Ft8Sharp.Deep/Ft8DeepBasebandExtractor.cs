using System;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;

namespace Ft8Sharp.Deep;

/// <summary>
/// <b>174 log-likelihood ratios read out of a baseband at a position it is told - a continuous time
/// and a continuous frequency, neither of them a grid index.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>The one named change is where the magnitudes are measured from, and that is the whole of
/// step 4.</b> The Gray map, the bit partition and the ratio arithmetic are the port's:
/// <c>Ft8SoftSymbols.ExtractSymbol</c> is public, is pinned against upstream's written partition by
/// <c>UpstreamExtractionInventoryTests</c>, and is called here for every non-sync symbol. <b>None of
/// it is re-implemented in this library</b>, because a second copy is a second thing to be wrong.
/// </para>
/// <para>
/// <b>What the port does at the same seam, for comparison.</b> <c>Ft8SoftSymbols.Extract</c> reads
/// <c>waterfall.DecibelsAt(block, timeSub, freqSub, binOffset + gray[value])</c> - a byte quantised
/// to half a decibel, from a 3840-point transform of a Hann-squared-sine-windowed frame, at one of
/// two time sub-offsets in a 1920-sample block and one of two frequency sub-offsets in a 3.125 Hz
/// bin. Everything below reads the same eight numbers from the samples instead, at a position no
/// grid names.
/// </para>
/// <para>
/// <b>The Costas correlation and the FT8 frame it is applied to.</b> The 79-symbol frame, the three
/// seven-symbol Costas arrays at symbols 0, 36 and 72, and the eight-tone alphabet spaced at the
/// reciprocal of the symbol period are all from the published protocol description - Franke K9AN,
/// Somerville G4WJS and Taylor K1JT, <i>The FT4 and FT8 Communication Protocols</i>, QEX,
/// July/August 2020. The arrays themselves are the port's <c>Ft8Tables.Ft8CostasPattern</c> and the
/// layout is <c>Ft8SymbolEncoder</c>'s. <b>Correlating a known sequence against a received one to
/// find its position is textbook</b> and is cited as such.
/// </para>
/// </remarks>
public static class Ft8DeepBasebandExtractor
{
    /// <summary>How many doubles a scratch tone grid needs.</summary>
    public const int GridLength = Ft8SymbolEncoder.SymbolCount * Ft8SymbolEncoder.ToneCount;

    /// <summary>
    /// Reads 174 ratios at a continuous position, allocating its own scratch.
    /// </summary>
    /// <param name="baseband">The slot, mixed about this candidate's eight tones.</param>
    /// <param name="startSeconds">When the frame's first symbol begins, in seconds into the slot.</param>
    /// <param name="frequencyOffsetHz">How far the tones sit from where the baseband was mixed.</param>
    /// <param name="ratios">
    /// Exactly <c>Ft8SoftSymbols.RatioCount</c> ratios, in codeword bit order, positive meaning the
    /// bit is more likely one. <b>Unnormalised</b>, exactly as the port's <c>Extract</c> leaves them.
    /// </param>
    /// <exception cref="ArgumentNullException">The baseband is null.</exception>
    /// <exception cref="ArgumentException">The ratio span is the wrong length.</exception>
    public static void Extract(
        Ft8DeepBaseband baseband,
        double startSeconds,
        double frequencyOffsetHz,
        Span<float> ratios) =>
        Extract(baseband, startSeconds, frequencyOffsetHz, ratios, new double[GridLength]);

    /// <summary>
    /// Reads 174 ratios at a continuous position, into scratch the caller owns.
    /// </summary>
    /// <param name="baseband">The slot, mixed about this candidate's eight tones.</param>
    /// <param name="startSeconds">When the frame's first symbol begins, in seconds into the slot.</param>
    /// <param name="frequencyOffsetHz">How far the tones sit from where the baseband was mixed.</param>
    /// <param name="ratios">Exactly <c>Ft8SoftSymbols.RatioCount</c> ratios, in codeword bit order.</param>
    /// <param name="grid"><see cref="GridLength"/> doubles of scratch, overwritten.</param>
    /// <exception cref="ArgumentNullException">The baseband is null.</exception>
    /// <exception cref="ArgumentException">Either span is the wrong length.</exception>
    /// <remarks>
    /// <b>A symbol whose window falls outside the slot contributes three zero ratios</b>, which is
    /// the port's own rule at <c>Ft8SoftSymbols.cs</c> and means <i>no opinion</i> rather than
    /// <i>refuse</i>. A transmission that began early or late is exactly what a search sweeping time
    /// offsets exists to catch, and refusing those candidates would throw away the ones it was
    /// written for.
    /// </remarks>
    public static void Extract(
        Ft8DeepBaseband baseband,
        double startSeconds,
        double frequencyOffsetHz,
        Span<float> ratios,
        Span<double> grid)
    {
        ArgumentNullException.ThrowIfNull(baseband);

        if (ratios.Length != Ft8SoftSymbols.RatioCount)
        {
            throw new ArgumentException(
                $"A transmission carries {Ft8SoftSymbols.RatioCount} log-likelihood ratios and a "
                + $"span of {ratios.Length} was given.",
                nameof(ratios));
        }

        baseband.TonePowerGrid(startSeconds, frequencyOffsetHz, grid);

        var gray = Ft8Tables.Ft8GrayMap;
        Span<double> byValue = stackalloc double[Ft8SymbolEncoder.ToneCount];

        var bit = 0;
        for (var symbol = 0; symbol < Ft8SymbolEncoder.SymbolCount; symbol++)
        {
            // The sync blocks are stepped OVER, and the layout is Ft8SymbolEncoder's rather than
            // laid out a second time here.
            if (Ft8SymbolEncoder.IsSyncSymbol(symbol))
            {
                continue;
            }

            var row = symbol * Ft8SymbolEncoder.ToneCount;

            if (double.IsNaN(grid[row]))
            {
                ratios[bit] = 0.0f;
                ratios[bit + 1] = 0.0f;
                ratios[bit + 2] = 0.0f;
                bit += Ft8SymbolEncoder.BitsPerSymbol;
                continue;
            }

            // VALUE ORDER THROUGH THE FORWARD GRAY MAP, which is the convention ExtractSymbol takes
            // and is the same indirection Ft8SoftSymbols.Extract performs against the waterfall's
            // bins: byValue[v] is the strength of the tone that would have carried symbol value v.
            for (var value = 0; value < Ft8SymbolEncoder.ToneCount; value++)
            {
                byValue[value] = grid[row + gray[value]];
            }

            Ft8SoftSymbols.ExtractSymbol(byValue, ratios.Slice(bit, Ft8SymbolEncoder.BitsPerSymbol));
            bit += Ft8SymbolEncoder.BitsPerSymbol;
        }
    }

}
