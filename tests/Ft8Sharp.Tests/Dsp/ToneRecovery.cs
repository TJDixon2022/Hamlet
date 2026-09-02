using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// Asks the waterfall, for one symbol whose tone and position are already known, which of the eight
/// tones has the most energy where that symbol is.
/// </summary>
/// <remarks>
/// <para>
/// <b>THIS IS NOT A SEARCH. You are told where to look, in frequency and in time, by
/// construction.</b> The base frequency was chosen by the caller and handed to the synthesizer; the
/// sample offset was chosen by the caller and used to place the signal; the symbol index is a loop
/// variable. Nothing here scans a passband, correlates against the Costas pattern, scores a
/// candidate or ranks anything. It measures whether the energy is where it was put. Finding a signal
/// nobody pointed at is the next unit's work and no result from this file is evidence about it.
/// </para>
/// <para>
/// <b>The alignment is computed, not searched for.</b> The analysis for block <c>b</c> at time
/// sub-offset <c>t</c> is a window of <c>transformLength</c> samples ending at input sample
/// <c>b*blockSize + (t+1)*subblockSize</c>, so its centre sits at
/// <c>b*blockSize + (t+1)*subblockSize - transformLength/2</c>. Setting that equal to the centre of
/// symbol <c>n</c> — <c>offset + n*blockSize + blockSize/2</c> — gives the block and sub-offset
/// directly. Where the two cannot be made equal, because the offset is not a whole number of
/// sub-blocks, the nearest is taken and <see cref="Alignment.ResidualSamples"/> reports by how much
/// it misses.
/// </para>
/// </remarks>
internal static class ToneRecovery
{
    /// <summary>Where symbol <paramref name="symbol"/> is looked at, and how well it lines up.</summary>
    internal readonly record struct Alignment(int Block, int TimeSubOffset, double ResidualSamples);

    /// <summary>What was found at one symbol.</summary>
    /// <param name="Expected">The tone that was transmitted there.</param>
    /// <param name="Strongest">The tone with the most energy in the neighbourhood.</param>
    /// <param name="MarginDecibels">
    /// How far the transmitted tone's cell sits above the strongest of the other seven. Negative
    /// when the wrong tone won.
    /// </param>
    /// <param name="RunnerUp">The strongest tone other than the transmitted one.</param>
    internal readonly record struct SymbolResult(
        int Symbol,
        int Expected,
        int Strongest,
        int RunnerUp,
        double MarginDecibels,
        Alignment Where)
    {
        public bool Recovered => Expected == Strongest;
    }

    /// <summary>What was found over a whole message.</summary>
    internal sealed record MessageResult(
        string Label,
        double BaseFrequencyHz,
        int OffsetSamples,
        int Recovered,
        int Total,
        double WorstMarginDecibels,
        IReadOnlyList<SymbolResult> Failures,
        IReadOnlyList<SymbolResult> All);

    /// <summary>
    /// The block and time sub-offset whose analysis window is centred nearest the centre of symbol
    /// <paramref name="symbol"/>, for a signal placed at <paramref name="offsetSamples"/>.
    /// </summary>
    internal static Alignment AlignmentFor(Ft8WaterfallGeometry geometry, int offsetSamples, int symbol)
    {
        var symbolCentre = offsetSamples + (symbol * geometry.BlockSize) + (geometry.BlockSize / 2.0);

        // b*blockSize + (t+1)*subblockSize - transformLength/2 == symbolCentre, and blockSize is
        // timeOversampling sub-blocks, so in units of sub-blocks: (b*osr + t + 1) = wanted.
        var wanted = (symbolCentre + (geometry.TransformLength / 2.0)) / geometry.SubblockSize;
        var units = (int)Math.Round(wanted) - 1;

        var block = Math.DivRem(units, geometry.TimeOversampling, out var timeSub);
        if (timeSub < 0)
        {
            timeSub += geometry.TimeOversampling;
            block--;
        }

        var actualCentre = (block * geometry.BlockSize)
            + ((timeSub + 1) * geometry.SubblockSize)
            - (geometry.TransformLength / 2.0);

        return new Alignment(block, timeSub, actualCentre - symbolCentre);
    }

    /// <summary>
    /// Measures one message: synthesize it at a chosen frequency, place it at a chosen offset, run
    /// it through the waterfall, and ask at each symbol which of the eight tones is strongest.
    /// </summary>
    /// <param name="noise">
    /// Optional. When given, noise is added to the whole slot at
    /// <paramref name="noiseRootMeanSquare"/> before the analysis. When
    /// <paramref name="signalPresent"/> is false the signal is left out altogether and only the
    /// noise is analysed, which is the measurement that says whether the recovery means anything.
    /// </param>
    internal static MessageResult Measure(
        Ft8Monitor monitor,
        string label,
        ReadOnlySpan<byte> packedMessage,
        float baseFrequencyHz,
        int offsetSamples,
        GaussianNoise? noise = null,
        double noiseRootMeanSquare = 0,
        bool signalPresent = true)
    {
        var geometry = monitor.Geometry;
        var symbols = Ft8SymbolEncoder.Encode(packedMessage);

        var slot = new float[Ft8Waveform.SlotSampleCount(geometry.SampleRate)];
        if (signalPresent)
        {
            var signal = Ft8Waveform.Synthesize(symbols, geometry.SampleRate, baseFrequencyHz);
            signal.AsSpan().CopyTo(slot.AsSpan(offsetSamples));
        }

        var audio = noise is null ? slot : noise.AddedTo(slot, noiseRootMeanSquare);
        var waterfall = monitor.Analyse(audio);

        var results = new List<SymbolResult>(symbols.Length);
        var failures = new List<SymbolResult>();
        var worstMargin = double.MaxValue;

        for (var n = 0; n < symbols.Length; n++)
        {
            var where = AlignmentFor(geometry, offsetSamples, n);
            if (where.Block < 0 || where.Block >= waterfall.BlockCount)
            {
                throw new InvalidOperationException(
                    $"symbol {n} of '{label}' at offset {offsetSamples} aligns to block "
                    + $"{where.Block}, which is outside the {waterfall.BlockCount} blocks analysed. "
                    + "The placement puts part of the signal outside the slot.");
            }

            var best = -1;
            var bestDecibels = double.NegativeInfinity;
            var runnerUp = -1;
            var runnerUpDecibels = double.NegativeInfinity;
            var expectedDecibels = double.NegativeInfinity;

            // The eight candidate cells: one per tone of the signal we transmitted. THE NUMBER OF
            // CANDIDATES IS EIGHT, so chance is one in eight.
            for (var tone = 0; tone < Ft8Waveform.ToneCount; tone++)
            {
                var hertz = baseFrequencyHz + (tone * Ft8Waveform.ToneSpacingHz);
                if (!geometry.TryBinFor(hertz, out var bin, out var freqSub))
                {
                    throw new InvalidOperationException(
                        $"tone {tone} of '{label}' at {hertz} Hz is outside the "
                        + $"{geometry.MinFrequencyHz}..{geometry.MaxFrequencyHz} Hz passband.");
                }

                var decibels = waterfall.DecibelsAt(where.Block, where.TimeSubOffset, freqSub, bin);

                if (tone == symbols[n])
                {
                    expectedDecibels = decibels;
                }

                if (decibels > bestDecibels)
                {
                    best = tone;
                    bestDecibels = decibels;
                }
            }

            foreach (var tone in Enumerable.Range(0, Ft8Waveform.ToneCount).Where(t => t != symbols[n]))
            {
                var hertz = baseFrequencyHz + (tone * Ft8Waveform.ToneSpacingHz);
                geometry.TryBinFor(hertz, out var bin, out var freqSub);
                var decibels = waterfall.DecibelsAt(where.Block, where.TimeSubOffset, freqSub, bin);
                if (decibels > runnerUpDecibels)
                {
                    runnerUp = tone;
                    runnerUpDecibels = decibels;
                }
            }

            var margin = expectedDecibels - runnerUpDecibels;
            var result = new SymbolResult(n, symbols[n], best, runnerUp, margin, where);
            results.Add(result);

            if (!result.Recovered)
            {
                failures.Add(result);
            }

            if (margin < worstMargin)
            {
                worstMargin = margin;
            }
        }

        return new MessageResult(
            label,
            baseFrequencyHz,
            offsetSamples,
            results.Count(r => r.Recovered),
            results.Count,
            worstMargin,
            failures,
            results);
    }
}
