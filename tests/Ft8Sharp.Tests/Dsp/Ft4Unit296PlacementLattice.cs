using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Tests.Encode;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>The placement instrument unit 296 task 1b asks for: a lattice spanning one whole FT4 tone
/// spacing and one whole FT4 symbol period, which belongs to no analysis grid.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>WHY THE LATTICE IS FIVE BY FIVE AND NOT FOUR BY FOUR, AND THIS IS THE WHOLE POINT OF THE
/// INSTRUMENT.</b> Unit 294 measured one off-grid placement — the <em>cell centre</em>, half an
/// analysis step each way. That offset is defined against the analysis grid, so densifying the grid
/// turns the same physical placement into an on-grid point and a re-measurement there would read
/// beautifully while nothing had improved for a real station. This lattice is defined against the
/// <em>protocol</em> instead: its steps are one fifth of FT4's tone spacing (20.8333 Hz) and one
/// fifth of FT4's symbol period (0.048 s, 576 samples at 12 kHz). Five is chosen because every
/// analysis grid this project can build oversamples by a power of two, and no power of two divides
/// five — so exactly one of the twenty-five cells is on the analysis grid, and it is the same one
/// cell whatever the oversampling is. The lattice cannot be flattered by densifying the grid.
/// </para>
/// <para>
/// <b>The one on-grid cell is cell (0, 0)</b> and it is unit 294's own on-grid placement: 1000.0 Hz,
/// which is a whole number of bins at every oversampling this project builds (96 bins at 2, 192 at
/// 4, 384 at 8), and a lead of 51 × 288 = 14688 samples, which is a whole number of sub-blocks at
/// every one of them (51 at time oversampling 2, 102 at 4, 204 at 8). Continuity with units 289 and
/// 294 is kept by that cell and nothing else needs to be held fixed.
/// </para>
/// <para>
/// <b>A wrong decode is counted separately from a missed one, at every cell and every rung</b>, per
/// the phase's own ruling. One transmission goes into each slot, so any other message coming out of
/// it is a message nobody sent.
/// </para>
/// <para>
/// <b>The noise draw is the same at every cell of a rung.</b> A fresh <see cref="GaussianNoise"/> is
/// seeded at the start of each cell, from the rung, so the sequence of draws a cell sees is byte for
/// byte the sequence every other cell saw. Placement is then the only thing that differs between two
/// cells of a row, which is what makes the comparison between them a measurement rather than a race
/// between two noise realisations.
/// </para>
/// <para>
/// <b>The slot is built here rather than taken from <see cref="Ft4Waveform.SynthesizeSlot"/></b>,
/// which pads by a fixed amount and therefore fixes the lead. This is unit 294's
/// <c>Slot(signal, leadSamples)</c> harness, moved here so that units 296 and after share one rather
/// than writing a third.
/// </para>
/// </remarks>
internal static class Ft4Unit296PlacementLattice
{
    /// <summary>The sample rate every FT4 measurement in this project is taken at.</summary>
    internal const int Rate = Ft4Waveform.DefaultSampleRate;

    /// <summary>
    /// <b>One whole FT4 tone spacing, in hertz.</b> A protocol fact — the reciprocal of the symbol
    /// period — and it belongs to no analysis grid.
    /// </summary>
    /// <remarks>
    /// <b>The reciprocal is taken in double and not in float.</b> <c>Ft4Timing.ToneSpacingHz</c> is
    /// <c>1.0f / 0.048f</c>, which is 20.833334 — three parts in a million above the protocol's
    /// 20.83333…, and enough that a placement built from it misses a bin centre by 3e-6 of a bin.
    /// This is a physical placement in hertz rather than a waterfall extent, so nothing here turns
    /// on matching upstream's single precision, and a lattice that cannot say which of its own cells
    /// is on the grid is not an instrument.
    /// </remarks>
    internal const double ToneSpacingHz = 1.0 / (double)Ft4Timing.SymbolPeriodSeconds;

    /// <summary>
    /// <b>One whole FT4 symbol period, in samples at 12 kHz: 576.</b> Also a protocol fact.
    /// </summary>
    /// <remarks>
    /// Derived in single precision, which is <c>Ft4WaterfallGeometry</c>'s own arithmetic and
    /// upstream's: <c>12000 * 0.048f</c> is <c>576.0f</c> exactly.
    /// </remarks>
    internal const int SymbolPeriodSamples = (int)(Rate * Ft4Timing.SymbolPeriodSeconds);

    /// <summary>How many divisions each axis of the lattice is cut into.</summary>
    /// <remarks>
    /// <b>Five, because no analysis oversampling this project builds divides it.</b> See the type
    /// remarks. Twenty-five cells, one of them on grid.
    /// </remarks>
    internal const int Divisions = 5;

    /// <summary>The on-grid frequency: 1000 Hz, a whole number of bins at 2, 4 and 8 bins per tone.</summary>
    internal const double BaseFrequencyHz = 1000.0;

    /// <summary>
    /// The on-grid lead: 51 sub-blocks of 288 samples, which is a whole number of sub-blocks at
    /// time oversampling 2, 4 and 8.
    /// </summary>
    internal const int BaseLeadSamples = 51 * 288;

    /// <summary>Where a transmission was put, and what to call it.</summary>
    internal sealed record Placement(
        int FrequencyIndex,
        int TimeIndex,
        double FrequencyHz,
        int LeadSamples)
    {
        /// <summary>True for the one cell that sits on the analysis grid at every oversampling.</summary>
        internal bool OnGrid => FrequencyIndex == 0 && TimeIndex == 0;

        /// <summary>The cell's name in the tables.</summary>
        internal string Name => $"({FrequencyIndex},{TimeIndex})";
    }

    /// <summary>One cell of one rung: what went in and what came out, wrong counted apart.</summary>
    internal sealed record Cell(
        Placement Placement,
        double Rung,
        int Trials,
        int Decoded,
        int Wrong,
        int Missed,
        double DeliveredMean)
    {
        /// <summary>The decode rate in this cell, in the range zero to one.</summary>
        internal double Rate => Trials == 0 ? double.NaN : (double)Decoded / Trials;
    }

    /// <summary>The whole lattice, in reading order, with the on-grid cell first.</summary>
    internal static IReadOnlyList<Placement> Build()
    {
        var cells = new List<Placement>();

        for (var k = 0; k < Divisions; k++)
        {
            for (var j = 0; j < Divisions; j++)
            {
                cells.Add(new Placement(
                    k,
                    j,
                    BaseFrequencyHz + (ToneSpacingHz * k / Divisions),
                    BaseLeadSamples + (SymbolPeriodSamples * j / Divisions)));
            }
        }

        return cells;
    }

    /// <summary>A whole slot: silence, the signal at a chosen lead, silence.</summary>
    internal static float[] Slot(ReadOnlySpan<float> signal, int leadSamples)
    {
        var slot = new float[Ft4Waveform.SlotSampleCount(Rate)];
        signal.CopyTo(slot.AsSpan(leadSamples));
        return slot;
    }

    /// <summary>
    /// <b>Runs one cell at one rung: every corpus entry once, decoded and missed and wrong counted
    /// apart.</b>
    /// </summary>
    internal static Cell Measure(
        Ft4SlotDecoder decoder,
        IReadOnlyList<Ft4RoundTripCorpus.Entry> corpus,
        Placement placement,
        double rung,
        int seed)
    {
        // THE SAME NOISE AT EVERY CELL OF THIS RUNG. Seeded from the rung alone, so placement is
        // the only difference between two cells of a row.
        var noise = new GaussianNoise(seed + (int)Math.Round(rung * 10.0));
        var decoded = 0;
        var wrong = 0;
        var deliveredSum = 0.0;

        foreach (var entry in corpus)
        {
            var symbols = Ft4SymbolEncoder.Encode(entry.Message);
            var signal = Ft4Waveform.Synthesize(symbols, Rate, (float)placement.FrequencyHz);
            var clean = Slot(signal, placement.LeadSamples);

            // Over the transmission and not over the slot: two thirds of an FT4 slot is silence.
            var signalPower = SignalToNoise.MeanSquare(signal);
            var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, rung, Rate);

            // The delivered ratio is measured from the noise that was drawn, not from the sigma
            // that was commanded. A finite draw is not its own parameter.
            var drawn = noise.Block(clean.Length, sigma);
            var noisePower = SignalToNoise.MeanSquare(drawn);
            deliveredSum += SignalToNoise.DecibelsFor(signalPower, noisePower, Rate);

            var mixed = new float[clean.Length];
            for (var i = 0; i < clean.Length; i++)
            {
                mixed[i] = clean[i] + drawn[i];
            }

            var result = decoder.Decode(mixed);
            var itsOwn = false;

            foreach (var message in result.Messages)
            {
                if (string.Equals(message.Text, entry.Text, StringComparison.Ordinal))
                {
                    itsOwn = true;
                    continue;
                }

                wrong++;
            }

            if (itsOwn)
            {
                decoded++;
            }
        }

        return new Cell(
            placement,
            rung,
            corpus.Count,
            decoded,
            wrong,
            corpus.Count - decoded,
            deliveredSum / corpus.Count);
    }

    /// <summary>
    /// A stated subset of the corpus: every <paramref name="stride"/>th entry from the front, so
    /// the subset spans the corpus's kinds rather than taking a block of one of them.
    /// </summary>
    internal static IReadOnlyList<Ft4RoundTripCorpus.Entry> Subset(
        IReadOnlyList<Ft4RoundTripCorpus.Entry> corpus, int stride)
    {
        var taken = new List<Ft4RoundTripCorpus.Entry>();

        for (var i = 0; i < corpus.Count; i += stride)
        {
            taken.Add(corpus[i]);
        }

        return taken;
    }
}
