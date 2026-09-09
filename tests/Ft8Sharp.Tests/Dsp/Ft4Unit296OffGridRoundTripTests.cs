using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>Unit 296 task 5 — step 1's must-pass criteria 1 and 3, re-taken where a real station lands.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>WHAT THIS CATCHES, AND IT IS A TRUE FIGURE ABOUT THE WRONG SIGNAL.</b> Unit 289 met step 1's
/// first criterion — a message becomes FT4 symbols becomes audio and decodes back to the same
/// message, over at least a hundred messages — at <c>const float frequency = 1000.0f</c> with
/// <see cref="Ft4Waveform.SynthesizeSlot"/>'s fixed padding. 1000 Hz is exactly 96 bins at FT4's
/// 10.4167 Hz step and the padding is a whole number of time steps, so <b>106 of 106 is true, and
/// true at a placement Hamlet chose for itself</b>. Nothing on 14.080 arranges itself to Hamlet's
/// analysis grid. The breakage is a must-pass criterion that stays green for ever while the
/// operator tunes up and sees an empty table, and no on-grid test in this suite can see it.
/// </para>
/// <para>
/// <b>THE PLACEMENT IS DRAWN, NOT CHOSEN.</b> Each message gets a frequency offset uniform in
/// [0, one tone spacing) and a lead offset uniform in [0, one symbol period) — the two spans are
/// protocol facts and belong to no analysis grid — from <see cref="Seed"/>, stated so the run is a
/// number rather than a range. That is deliberately not the lattice: the lattice says where the
/// decoder is weak, and a draw says what a station picked out of the air is likely to get.
/// </para>
/// <para>
/// <b>BESIDE THE ON-GRID RUN AND NOT INSTEAD OF IT.</b> <c>Ft4RoundTripTests</c>'s on-grid assertion
/// is untouched. The two figures side by side are the evidence; one replacing the other is a claim.
/// </para>
/// <para>
/// <b>AND AT BOTH GRIDS, WHICH IS WHAT MAKES IT A MEASUREMENT OF TASK 4.</b> Every figure is taken
/// at upstream's own 2x2 and at the grid task 4 adopted, on the same draws and the same noise, so
/// the difference between two rows is the grid and nothing else.
/// </para>
/// </remarks>
public class Ft4Unit296OffGridRoundTripTests(ITestOutputHelper output)
{
    private const int Rate = Ft4Unit296PlacementLattice.Rate;

    /// <summary>The seed the placements are drawn from. Fixed, and stated.</summary>
    private const int Seed = 296;

    /// <summary>Where a message was put and what it was.</summary>
    private sealed record Drawn(
        Ft4RoundTripCorpus.Entry Entry, double FrequencyHz, int LeadSamples);

    /// <summary>
    /// <b>Criterion 1 and criterion 3, off grid, in clear air.</b> The same thing unit 289 measured
    /// on grid: no noise, one transmission a slot, the composed text as ground truth.
    /// </summary>
    [Fact]
    public void AHundredFt4MessagesComeBackFromWhereARealStationLands()
        => RoundTrip(rung: null);

    /// <summary>
    /// <b>The same run at -13 dB, which is where the criterion actually bites.</b>
    /// </summary>
    /// <remarks>
    /// In clear air a placement costs almost nothing — task 1b read 900 of 900 at -10 dB across the
    /// whole lattice even at HEAD's grid — so a clean round trip is a gate that a decoder with a
    /// two-decibel placement loss walks through. <b>-13 dB is the lowest rung at which the on-grid
    /// rate is exactly one</b>, so anything short of one here is the placement and not the code.
    /// This is reported beside criterion 1 rather than as it: the criterion is the clean round trip
    /// and this is what it does not see.
    /// </remarks>
    [Fact]
    public void TheSameHundredComeBackAtMinusThirteenDecibelsOffGrid()
        => RoundTrip(rung: -13.0);

    /// <summary>Runs the whole corpus at drawn placements, at both grids.</summary>
    private void RoundTrip(double? rung)
    {
        var corpus = Ft4RoundTripCorpus.Build();

        Assert.True(
            corpus.Count >= 100,
            $"step 1 asks for at least a hundred messages and the corpus holds {corpus.Count}");

        // DRAWN ONCE AND USED AT BOTH GRIDS. The same station in the same place, decoded two ways.
        var random = new Random(Seed);
        var drawn = corpus
            .Select(entry => new Drawn(
                entry,
                Ft4Unit296PlacementLattice.BaseFrequencyHz
                    + (random.NextDouble() * Ft4Unit296PlacementLattice.ToneSpacingHz),
                Ft4Unit296PlacementLattice.BaseLeadSamples
                    + random.Next(Ft4Unit296PlacementLattice.SymbolPeriodSamples)))
            .ToList();

        output.WriteLine(
            rung is null
                ? "STEP 1 CRITERION 1 AND 3, OFF GRID, IN CLEAR AIR"
                : $"THE SAME RUN AT {rung:F0} dB");
        output.WriteLine(
            $"  corpus            {corpus.Count} messages - compound callsigns, grids, reports and "
            + "RR73");
        output.WriteLine(
            $"  placement         drawn uniform in [0, {Ft4Unit296PlacementLattice.ToneSpacingHz:F4} Hz) "
            + $"and [0, {Ft4Unit296PlacementLattice.SymbolPeriodSamples} samples), seed {Seed}");
        output.WriteLine(
            $"  spans             one whole FT4 tone spacing and one whole FT4 symbol period, both "
            + "protocol facts");
        output.WriteLine(
            $"  frequencies drawn {drawn.Min(d => d.FrequencyHz):F3} to "
            + $"{drawn.Max(d => d.FrequencyHz):F3} Hz; leads "
            + $"{drawn.Min(d => d.LeadSamples)} to {drawn.Max(d => d.LeadSamples)} samples");
        output.WriteLine(string.Empty);
        output.WriteLine($"{"grid",-10}{"trials",-9}{"decoded",-10}{"WRONG",-8}missed");

        var results = new List<(string Grid, int Decoded, int Wrong, int Missed, List<string> Detail)>();

        foreach (var (time, frequency, label) in new[]
                 {
                     (2, 2, "2x2 upstream"),
                     (Ft4WaterfallGeometry.DefaultTimeOversampling,
                         Ft4WaterfallGeometry.DefaultFrequencyOversampling,
                         "4x2 shipping"),
                 })
        {
            var decoder = new Ft4SlotDecoder(new Ft4WaterfallGeometry(
                timeOversampling: time, frequencyOversampling: frequency));
            var noise = rung is null ? null : new GaussianNoise(Seed);
            var decoded = 0;
            var wrong = 0;
            var detail = new List<string>();

            foreach (var place in drawn)
            {
                var symbols = Ft4SymbolEncoder.Encode(place.Entry.Message);
                var signal = Ft4Waveform.Synthesize(symbols, Rate, (float)place.FrequencyHz);
                var slot = Ft4Unit296PlacementLattice.Slot(signal, place.LeadSamples);

                if (noise is not null)
                {
                    var sigma = SignalToNoise.NoiseAmplitudeFor(
                        SignalToNoise.MeanSquare(signal), rung!.Value, Rate);
                    slot = noise.AddedTo(slot, sigma);
                }

                var result = decoder.Decode(slot);
                var itsOwn = false;

                foreach (var message in result.Messages)
                {
                    if (string.Equals(message.Text, place.Entry.Text, StringComparison.Ordinal))
                    {
                        itsOwn = true;
                        continue;
                    }

                    // ONE transmission went into this slot, so anything else out of it is a message
                    // nobody sent. Counted apart from a miss, whatever else the slot did.
                    wrong++;
                    detail.Add(
                        $"WRONG: sent \"{place.Entry.Text}\" at {place.FrequencyHz:F3} Hz lead "
                        + $"{place.LeadSamples} and read \"{message.Text}\"");
                }

                if (itsOwn)
                {
                    decoded++;
                }
                else
                {
                    detail.Add(
                        $"missed: \"{place.Entry.Text}\" ({place.Entry.Kind}) at "
                        + $"{place.FrequencyHz:F3} Hz lead {place.LeadSamples}; the slot gave "
                        + $"{result.Messages.Count} message(s) from {result.CandidateCount} "
                        + "candidate(s)");
                }
            }

            results.Add((label, decoded, wrong, corpus.Count - decoded, detail));

            output.WriteLine(
                $"{label,-10}{corpus.Count,-9}{decoded,-10}{wrong,-8}{corpus.Count - decoded}");
        }

        var shipping = results[^1];
        var upstream = results[0];

        output.WriteLine(string.Empty);
        output.WriteLine("CRITERION 3 - ZERO WRONG DECODES, STATED SEPARATELY FROM THE MISSED COUNT");
        output.WriteLine(
            $"  wrong at 2x2 {upstream.Wrong}, wrong at 4x2 {shipping.Wrong}; missed at 2x2 "
            + $"{upstream.Missed}, missed at 4x2 {shipping.Missed}");

        foreach (var line in shipping.Detail.Take(20))
        {
            output.WriteLine($"  {line}");
        }

        if (shipping.Detail.Count > 20)
        {
            output.WriteLine($"  ... and {shipping.Detail.Count - 20} more");
        }

        // CRITERION 3 IS THE ONE THAT IS ASSERTED, AND IT IS ASSERTED AT BOTH GRIDS. A wrong decode
        // is the finding of this unit and goes above any rate improvement, so it fails the run
        // rather than being reported under one.
        Assert.True(
            shipping.Wrong == 0 && upstream.Wrong == 0,
            $"criterion 3 asks for zero wrong decodes and the run read {upstream.Wrong} at 2x2 and "
            + $"{shipping.Wrong} at 4x2: "
            + string.Join("; ", results.SelectMany(r => r.Detail).Where(d => d.StartsWith("WRONG"))));

        if (rung is null)
        {
            // CRITERION 1, OFF GRID. Unit 289 met it on grid at 106 of 106; this is the same gate
            // at a placement Hamlet did not choose.
            Assert.True(
                shipping.Decoded >= 100,
                $"step 1's first criterion asks for at least a hundred messages round tripping and "
                + $"{shipping.Decoded} of {corpus.Count} came back at drawn placements. Upstream's "
                + $"own grid read {upstream.Decoded}.");
        }
    }
}
