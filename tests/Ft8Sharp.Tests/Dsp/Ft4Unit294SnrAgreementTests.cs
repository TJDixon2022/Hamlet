using System.Diagnostics;
using Ft8Sharp.Deep;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>The signal-to-noise ratio <c>Ft4DeepSignalToNoise</c> reports for a decoded FT4 message,
/// measured against the ratio the ladder actually delivered to it.</b> Work instruction 294 task 4,
/// which gates task 5.
/// </summary>
/// <remarks>
/// <para>
/// <b>THE THRESHOLD, STATED BEFORE THE NUMBERS AND NOT AFTER THEM.</b> The gate is
/// <see cref="GateDecibels"/> — <b>2 dB mean absolute error</b>, which is <c>PHASE_PLAN.md</c>'s own
/// figure for whether a ratio may go on a surface at all, and it is the figure unit 251's verdict on
/// FT8 was taken against. It is written down here, in a constant, <b>before this test was ever
/// run</b>, precisely so that it cannot be chosen after seeing the answer. If the estimator does not
/// clear it, task 5 does not put a report on the right-click menu and that is a successful outcome of
/// the unit rather than a failure: a report is not a screen, it goes to another operator and into his
/// log.
/// </para>
/// <para>
/// <b>THE RUNGS ARE FT4'S OWN AND NOT FT8'S.</b> <c>Ft4SensitivityLadderTests</c> was run for this
/// unit and is the source: over its 106-message corpus FT4 decodes <b>106 of 106 at 0, -5, -10 and
/// -13 dB, 49 of 106 at -15, and 0 of 106 at -17, -19 and -21</b>. An agreement figure is taken only
/// over messages that decoded, so at a rung near the code's threshold only the lucky noise draws
/// decode and the sample is biased toward trials whose noise happened to be kind. <b>The rungs here
/// are the five at which the decode rate is one</b> — <see cref="Rungs"/>, -13 to -1 dB — so there is
/// no selection at all rather than a small one, and twelve decibels of span, so a systematic error in
/// the reference constant would show as a bias and a scale error as a tilt. <b>Nothing is claimed at
/// -15 dB or below</b>, where FT4 either half-decodes or does not decode.
/// </para>
/// <para>
/// <b>WITH ONE EXCEPTION, AND IT IS NAMED RATHER THAN LEFT IN THE CLAIM.</b> The decode rate is one at
/// every rung <em>on grid</em>. <b>At the cell centre the -13 dB rung decodes 17 of 106</b>, so FT4's
/// decoder loses something like two to three decibels of sensitivity when the signal does not land on
/// the analysis grid. That is a finding about the <em>decoder</em> and not about the estimator, it is
/// not this unit's to chase — unit 289's widened candidate sweep is with the owner — and it means the
/// agreement figure in that one cell is taken on a sample the decoder selected. It is <b>17 of 970
/// readings</b>; the other nine cells are unselected and the overall figure is theirs.
/// </para>
/// <para>
/// <b>AND ONE THING THE MEASUREMENT FOUND THAT THE ESTIMATOR'S REMARKS DID NOT PREDICT.</b> The bias
/// is not flat: it runs from -0.07 dB at -13 dB to <b>-1.18 dB at -1 dB</b>, growing with signal
/// strength. The cause is in the arithmetic and is not a defect in it: the noise is estimated from the
/// three tones that were <em>not</em> sent at that instant, and GFSK smoothing puts a little of the
/// transmitted tone into its neighbours, so the noise estimate carries a term proportional to the
/// <em>signal</em>. At -13 dB that term is lost in the real noise; at -1 dB it is most of what the
/// three wrong bins hold, and the ratio is pulled down. <b>The estimator therefore reads low on strong
/// stations, by about a decibel at -1 dB</b>, and it is reported that way rather than corrected,
/// because a correction fitted to this table would be a fit.
/// </para>
/// <para>
/// <b>THE ESTIMATE IS TAKEN AT THE PLACE THE DECODER REPORTS, NOT AT THE TRUTH.</b> That is the whole
/// point of the measurement: it is the number Hamlet will put on the screen and on the air, and
/// Hamlet has only what the decoder returned. The frequency and the offset go to the synthesiser and
/// to nothing else; the truth is used after the decoder has answered, to compare the text and to
/// measure the candidate time bias.
/// </para>
/// <para>
/// <b>AND THE SYMBOL SEQUENCE IS RECOVERED THE WAY HAMLET WILL HAVE TO RECOVER IT.</b> Not from the
/// corpus entry's own bits, which no receiver has, but by packing the decoded message again through
/// <see cref="Ft4DeepMessageSymbols"/>. Where that succeeds, <b>this test asserts the recovered
/// symbols are byte for byte the transmitted ones</b>. That assertion is the guard against the
/// failure mode the whole route has: a hashed callsign that packs to different bits than were sent
/// would give a ratio measured against a transmission nobody made — and here that ratio would be
/// transmitted as a signal report.
/// </para>
/// <para>
/// <b>TWO PLACEMENTS, BECAUSE G1 IS IN THE BREAKAGE RECORD.</b> <b>On grid</b> — a whole number of
/// waterfall time steps and a bin centre, where the coarse grid has nothing to lose — and <b>at the
/// cell centre</b>, half a step each way, which is where a real station lands, because nothing on
/// 14.080 arranges itself on Hamlet's analysis grid.
/// </para>
/// <para>
/// <b>NOTHING IS FITTED.</b> No constant in <c>Ft4DeepSignalToNoise</c> is adjusted by anything
/// measured here; <c>ReferenceOffsetDecibels</c> is <c>10 log10(2500 / 20.8333)</c> and is derived in
/// <c>docs/unit251-snr-trace.md</c> §4 at FT4's own symbol period. The refined and unrefined figures
/// are both printed, so the alignment search's contribution is visible rather than absorbed.
/// </para>
/// </remarks>
public class Ft4Unit294SnrAgreementTests(ITestOutputHelper output)
{
    private const int Rate = Ft4Waveform.DefaultSampleRate;

    /// <summary>
    /// <b>What the mean absolute error has to be inside for the figure to go on the air.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Two decibels, which is <c>PHASE_PLAN.md</c>'s own threshold for a ratio reaching a
    /// surface</b>, and the same one unit 251's verdict on FT8 was taken against. <b>Written before
    /// the test was run.</b> A threshold invented after seeing the numbers is not a threshold.
    /// </para>
    /// <para>
    /// <b>Why the FT8 regression guard's 1 dB is not used.</b> That bound is a regression guard set at
    /// four times the headroom of a figure already measured at 0.26 dB; there was no FT4 figure to set
    /// headroom against when this constant was written, and inventing one would have been a
    /// prediction dressed as a gate. This is the plan's threshold, which is about whether an operator
    /// may send the number.
    /// </para>
    /// <para>
    /// <b>An operator's reading of it.</b> A report is quoted in whole decibels and is conventionally
    /// read to about that. Two decibels of mean error is a report that is right to within roughly what
    /// the convention carries; a larger one would put a figure in somebody else's log that is wrong by
    /// more than the format can express.
    /// </para>
    /// </remarks>
    private const double GateDecibels = 2.0;

    /// <summary>
    /// <b>The rungs, in decibels in the 2500 Hz reference bandwidth.</b> The five at which
    /// <c>Ft4SensitivityLadderTests</c> measured a decode rate of one over its 106-message corpus, so
    /// the agreement figure is taken on no selected sample at all.
    /// </summary>
    private static readonly double[] Rungs = [-13.0, -10.0, -7.0, -4.0, -1.0];

    /// <summary>The lowest tone the synthesiser is told to put the transmission on.</summary>
    /// <remarks>
    /// <b>A bin centre.</b> FT4's transform bin spacing at 12 kHz is 10.4167 Hz and 1000 is exactly
    /// 96 of them, so the on-grid placement really is on the grid.
    /// </remarks>
    private const double OnGridFrequencyHz = 1000.0;

    /// <summary>
    /// <b>Where the on-grid placement starts, in samples: a whole number of waterfall time steps.</b>
    /// </summary>
    /// <remarks>
    /// FT4's waterfall sub-block is 288 samples at 12 kHz — 0.024 s, half a symbol, because the
    /// geometry oversamples time twice. <b>51 of them is 14688 samples, 1.224 s.</b>
    /// <see cref="Ft4Waveform.PaddingSampleCount"/> is 14760, which is 51.25 steps, so upstream's own
    /// slot layout sits a <em>quarter</em> of a time step off the analysis grid and neither placement
    /// below is it. That is a finding rather than a problem — it is where a real signal lands too —
    /// and it is why the on-grid lead is computed here rather than taken from the waveform.
    /// </remarks>
    private const int OnGridLeadSamples = 51 * 288;

    /// <summary>
    /// <b>Half a waterfall frequency step: 5.2083 Hz.</b> The geometry oversamples frequency twice,
    /// so its step is half FT4's 20.833 Hz tone spacing and the furthest a signal can sit from a bin
    /// centre is half of that again.
    /// </summary>
    private const double CellCentreFrequencyOffsetHz = 20.8333333333 / 2.0 / 2.0;

    /// <summary>
    /// <b>Half a waterfall time step: 144 samples at 12 kHz, which is 0.012 s.</b>
    /// </summary>
    private const int CellCentreOffsetSamples = 144;

    /// <summary>The seed, fixed, so the ladder is a number rather than a range.</summary>
    private const int Seed = 294;

    /// <summary>Where the synthesiser is told to put a transmission, and what to call it.</summary>
    private sealed record Placement(string Name, double FrequencyHz, int LeadSamples);

    /// <summary>One cell of the table: how many trials went in and how many decoded.</summary>
    private sealed record Cell(string Placement, double Rung, int Trials, int Decoded);

    /// <summary>One decoded message, its delivered ratio and what the estimator made of it.</summary>
    private sealed record Reading(
        double Delivered,
        double Refined,
        double Unrefined,
        double WithoutRamps,
        int Symbols,
        double TimeShift,
        double FrequencyShift);

    /// <summary>
    /// <b>THE MEASUREMENT, AND THE GATE.</b> Five rungs, two placements, 106 trials each, the estimate
    /// taken at the place the decoder reported and against the symbol sequence packed back out of the
    /// text.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>What is asserted, and what is only reported.</b> Asserted: at least two hundred messages were
    /// measured; every recovered symbol sequence is the transmitted one; the whole
    /// <see cref="Ft8SlotResult"/> is identical when the same slot is decoded again after the estimate
    /// has been taken; the samples are unchanged; the candidate time bias is
    /// <c>Ft4DeepSignalToNoise.Ft4CandidateTimeBiasSeconds</c> on the on-grid placement; and the mean
    /// absolute error is inside <see cref="GateDecibels"/>. Reported and not asserted: the 95th
    /// percentile, the bias, the per-rung counts, the unrefined figures and the ramp-excluded figure.
    /// <b>A rung that reads badly is a measurement, not a failure.</b>
    /// </para>
    /// <para>
    /// <b>The decoder is the one Hamlet runs</b>: <c>Ft4SlotDecoder</c> with its own defaults, which is
    /// what <c>Ft8Reception.ReadFt4</c> builds. Measuring through a different configuration would be
    /// measuring a number the operator will never see.
    /// </para>
    /// </remarks>
    [Fact]
    public void TheFt4EstimateAgreesWithTheDeliveredRatioAtEveryRungFt4Decodes()
    {
        var started = Stopwatch.StartNew();
        var corpus = Ft4RoundTripCorpus.Build();
        var decoder = new Ft4SlotDecoder();
        var geometry = decoder.Geometry;

        Placement[] placements =
        [
            new("on grid", OnGridFrequencyHz, OnGridLeadSamples),
            new(
                "cell centre",
                OnGridFrequencyHz + CellCentreFrequencyOffsetHz,
                OnGridLeadSamples + CellCentreOffsetSamples),
        ];

        var readings = new Dictionary<string, List<Reading>>(StringComparer.Ordinal);
        var byRung = new Dictionary<double, List<Reading>>();
        var rates = new List<Cell>();
        var repackRefused = 0;
        var notMeasured = 0;
        var decodedTotal = 0;
        var trialsTotal = 0;
        var biases = new List<double>();

        output.WriteLine("THE GATE, STATED BEFORE THE NUMBERS");
        output.WriteLine($"  mean absolute error must be at or inside {GateDecibels:F1} dB, which is");
        output.WriteLine("  PHASE_PLAN.md's own threshold for a ratio reaching a surface and the one");
        output.WriteLine("  unit 251's verdict on FT8 was taken against. If it is not met, the");
        output.WriteLine("  right-click menu stays at three shapes and the report is not offered.");
        output.WriteLine(string.Empty);
        output.WriteLine(
            $"reference offset {Ft4DeepSignalToNoise.ReferenceOffsetDecibels:F4} dB from a "
            + $"{Ft4DeepSignalToNoise.BinBandwidthHz:F4} Hz bin against "
            + $"{Ft4DeepSignalToNoise.ReferenceBandwidthHz:F0} Hz "
            + $"(FT8's is {Ft8DeepSignalToNoise.ReferenceOffsetDecibels:F4} from "
            + $"{Ft8DeepSignalToNoise.BinBandwidthHz:F4} Hz). Nothing was fitted.");
        output.WriteLine(
            $"minimum symbols {Ft4DeepSignalToNoise.MinimumSymbols} of "
            + $"{Ft4SymbolEncoder.SymbolCount}; time search "
            + $"+/-{Ft4DeepSignalToNoise.TimeSearchSeconds * 1000.0:F0} ms, frequency search "
            + $"+/-{Ft4DeepSignalToNoise.FrequencySearchHz:F2} Hz");
        output.WriteLine(string.Empty);
        output.WriteLine(
            "placement     rung   trials  decoded  measured   MAE ref   p95 ref   bias ref"
            + "   MAE raw   p95 raw   no ramps");

        foreach (var placement in placements)
        {
            var all = new List<Reading>();
            readings[placement.Name] = all;

            foreach (var rung in Rungs)
            {
                var atRung = new List<Reading>();
                var noise = new GaussianNoise(Seed + (int)Math.Round(rung * 10.0));
                var decodedHere = 0;

                for (var trial = 0; trial < corpus.Count; trial++)
                {
                    var entry = corpus[trial];
                    trialsTotal++;

                    var truth = Ft4SymbolEncoder.Encode(entry.Message);
                    var signal = Ft4Waveform.Synthesize(truth, Rate, (float)placement.FrequencyHz);
                    var clean = Slot(signal, placement.LeadSamples);

                    // OVER THE TRANSMISSION AND NOT OVER THE SLOT. Two thirds of an FT4 slot is
                    // silence, and a mean square over the whole buffer would read every rung about
                    // 1.7 dB better than it is.
                    var signalPower = SignalToNoise.MeanSquare(signal);
                    var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, rung, Rate);

                    // THE DELIVERED RATIO IS MEASURED FROM THE NOISE THAT WAS ACTUALLY DRAWN, not
                    // from the sigma that was commanded. A finite draw is not its own parameter.
                    var drawn = noise.Block(clean.Length, sigma);
                    var noisePower = SignalToNoise.MeanSquare(drawn);
                    var delivered = SignalToNoise.DecibelsFor(signalPower, noisePower, Rate);

                    var mixed = new float[clean.Length];
                    for (var i = 0; i < clean.Length; i++)
                    {
                        mixed[i] = clean[i] + drawn[i];
                    }

                    var sent = Ft8MessageDecoder.Decode(entry.Message).Text;
                    var result = decoder.Decode(mixed);

                    var found = -1;
                    for (var i = 0; i < result.Messages.Count; i++)
                    {
                        if (string.Equals(result.Messages[i].Text, sent, StringComparison.Ordinal))
                        {
                            found = i;
                            break;
                        }
                    }

                    if (found < 0)
                    {
                        continue;
                    }

                    decodedHere++;
                    decodedTotal++;

                    var message = result.Messages[found];

                    // THE BITS PACKED BACK OUT OF THE WORDS, which is all a receiver has.
                    var symbols = Ft4DeepMessageSymbols.TryEncode(message.Result.Message);

                    if (symbols is null)
                    {
                        repackRefused++;
                        continue;
                    }

                    // AND THE ASSERTION THAT MAKES THE WHOLE MEASUREMENT MEAN ANYTHING. A recovered
                    // sequence that is not the transmitted one would give a ratio measured against a
                    // signal nobody sent - and this one goes on the air as a report.
                    Assert.True(
                        truth.AsSpan().SequenceEqual(symbols),
                        $"{placement.Name} {rung:F0} dB trial {trial}: the symbols packed back out "
                        + $"of '{sent}' are not the symbols that were transmitted.");

                    // THE CANDIDATE TIME BIAS, MEASURED DIRECTLY AND WITH NO ESTIMATOR IN THE LOOP.
                    // The true start of the signal is known exactly - the lead this test wrote - so
                    // the bias is a subtraction. On the cell-centre placement the decoder's answer is
                    // quantised half a step away from the truth by construction, so only the on-grid
                    // placement contributes to the assertion below.
                    var trueStart = placement.LeadSamples / (double)Rate;
                    var reported = message.TimeSeconds(geometry);

                    if (placement.LeadSamples == OnGridLeadSamples)
                    {
                        biases.Add(trueStart - reported);
                    }

                    var before = Checksum(mixed);

                    var start = reported + Ft4DeepSignalToNoise.Ft4CandidateTimeBiasSeconds;
                    var baseband = Ft4DeepBaseband.Build(
                        mixed, Rate, message.FrequencyHz(geometry));

                    var refined = Ft4DeepSignalToNoise.Estimate(
                        baseband, start, 0.0, symbols, refine: true);
                    var unrefined = Ft4DeepSignalToNoise.Estimate(
                        baseband, start, 0.0, symbols, refine: false);

                    Assert.True(
                        before == Checksum(mixed),
                        $"{placement.Name} {rung:F0} dB trial {trial}: the estimator changed the "
                        + "samples it was given.");

                    if (!refined.IsMeasured || !unrefined.IsMeasured)
                    {
                        notMeasured++;
                        continue;
                    }

                    // THE RAMP DECISION, MEASURED BOTH WAYS. The estimator reads all 105 symbols;
                    // this is what it would have said reading only the 103 that are not ramps. The
                    // arithmetic is redone here rather than the library being given a switch nobody
                    // should reach for.
                    var withoutRamps = RatioExcludingRamps(
                        baseband, symbols, start + refined.TimeAdjustmentSeconds,
                        refined.FrequencyAdjustmentHz);

                    // EXIT CRITERION: THE ESTIMATOR DECIDES NOTHING. The same slot, through the same
                    // decoder, after the estimate has been taken. Ten a rung, spread across both
                    // placements, because a whole second walk would double the wall clock.
                    if (trial < 10)
                    {
                        AssertIdentical(
                            result,
                            decoder.Decode(mixed),
                            geometry,
                            $"{placement.Name} {rung:F0} dB trial {trial}");
                    }

                    var reading = new Reading(
                        delivered,
                        refined.Decibels,
                        unrefined.Decibels,
                        withoutRamps,
                        refined.Symbols,
                        refined.TimeAdjustmentSeconds,
                        refined.FrequencyAdjustmentHz);

                    atRung.Add(reading);
                    all.Add(reading);

                    if (!byRung.TryGetValue(rung, out var rungList))
                    {
                        rungList = [];
                        byRung[rung] = rungList;
                    }

                    rungList.Add(reading);
                }

                rates.Add(new Cell(placement.Name, rung, corpus.Count, decodedHere));
                Report(output, placement.Name, rung, corpus.Count, decodedHere, atRung);
            }
        }

        output.WriteLine(string.Empty);

        var everything = readings.Values.SelectMany(r => r).ToList();
        Report(output, "BOTH", double.NaN, trialsTotal, decodedTotal, everything);

        output.WriteLine(string.Empty);
        output.WriteLine(
            $"trials {trialsTotal}, decoded {decodedTotal}, measured {everything.Count}, "
            + $"re-pack refused {repackRefused}, no measurement {notMeasured}");

        output.WriteLine(string.Empty);
        output.WriteLine("THE CANDIDATE TIME BIAS, MEASURED");
        output.WriteLine(
            $"  {biases.Count} on-grid trials; distinct values "
            + string.Join(", ", biases.Distinct().OrderBy(b => b).Select(b => $"{b:F6} s")));
        output.WriteLine(
            $"  the constant in use is "
            + $"{Ft4DeepSignalToNoise.Ft4CandidateTimeBiasSeconds:F6} s, which is minus one FT4 "
            + $"symbol period; FT8's is "
            + $"{Ft8DeepSlotDecoder.CandidateTimeBiasSeconds:F6} s and does not transfer");

        output.WriteLine(string.Empty);
        output.WriteLine("WHERE THE DECODE RATE WAS NOT ONE, AND IT IS THE DECODER AND NOT THIS");
        foreach (var cell in rates.Where(r => r.Decoded < r.Trials))
        {
            output.WriteLine(
                $"  {cell.Placement,-12} {cell.Rung,5:F0} dB   {cell.Decoded} of {cell.Trials} "
                + "decoded - the agreement figure in this cell is on a selected sample");
        }

        output.WriteLine(
            "  FT4 loses sensitivity when a signal does not land on the analysis grid. Not this");
        output.WriteLine(
            "  unit's to chase: unit 289's widened candidate sweep is with the owner. Reported.");

        output.WriteLine(string.Empty);
        output.WriteLine("THE BIAS IS NOT FLAT, AND THE REASON IS IN THE ARITHMETIC");
        output.WriteLine(
            "  the noise is the three tones that were NOT sent at that instant, and GFSK smoothing");
        output.WriteLine(
            "  puts a little of the transmitted tone into its neighbours - so the noise estimate");
        output.WriteLine(
            "  carries a term proportional to the SIGNAL. Lost in the real noise at -13 dB; most of");
        output.WriteLine(
            "  what the three wrong bins hold at -1 dB. The estimator reads LOW on strong stations.");

        foreach (var rung in Rungs)
        {
            var atRung = byRung.TryGetValue(rung, out var list) ? list : [];
            output.WriteLine(
                atRung.Count == 0
                    ? $"  {rung,5:F0} dB   nothing measured"
                    : $"  {rung,5:F0} dB   mean signed error "
                        + $"{atRung.Average(r => r.Refined - r.Delivered),6:F2} dB over "
                        + $"{atRung.Count} messages");
        }

        output.WriteLine(string.Empty);
        output.WriteLine("THE RAMP DECISION");
        output.WriteLine("  all 105 symbols are measured, the two ramps included: they carry no");
        output.WriteLine("  codeword bits but they do carry tone 0, which is known, and a known tone");
        output.WriteLine("  is measurable. What they differ in is the envelope.");
        var rampGap = everything.Average(r => r.WithoutRamps - r.Refined);
        output.WriteLine(
            $"  measuring the 103 non-ramp symbols instead moves the figure by "
            + $"{rampGap:+0.000;-0.000;0.000} dB on average over {everything.Count} messages");

        output.WriteLine(string.Empty);
        output.WriteLine($"wall clock {started.Elapsed.TotalSeconds:F1} s");

        Assert.True(
            everything.Count >= 200,
            $"the measurement asks for at least two hundred synthesized messages and "
            + $"{everything.Count} were measured. Cutting rungs is licensed; cutting the count is not.");

        // THE BIAS IS ONE VALUE ON EVERY ON-GRID TRIAL, which is what makes it a constant rather
        // than an average.
        Assert.All(
            biases,
            b => Assert.Equal(Ft4DeepSignalToNoise.Ft4CandidateTimeBiasSeconds, b, 6));

        var mae = MeanAbsoluteError(everything, r => r.Refined);

        output.WriteLine(
            mae <= GateDecibels
                ? $"THE GATE IS OPEN: {mae:F2} dB against {GateDecibels:F1} dB."
                : $"THE GATE IS SHUT: {mae:F2} dB against {GateDecibels:F1} dB. The menu stays short.");

        Assert.True(
            mae <= GateDecibels,
            $"the mean absolute error against the delivered ratio is {mae:F2} dB over "
            + $"{everything.Count} messages, against the gate of {GateDecibels:F1} dB. The figure "
            + "does not go on the air.");
    }

    /// <summary>
    /// <b>The ratio the estimator would report reading only the 103 symbols that are not ramps.</b>
    /// </summary>
    /// <remarks>
    /// <b>The arithmetic is redone here rather than the library being given an option.</b> A switch on
    /// <c>Ft4DeepSignalToNoise</c> would be a decision surface, and the decision has been taken: all
    /// 105 are measured. This exists so the figure the other way is a measurement in the report rather
    /// than a paragraph asking to be believed.
    /// </remarks>
    private static double RatioExcludingRamps(
        Ft4DeepBaseband baseband,
        ReadOnlySpan<byte> symbols,
        double startSeconds,
        double frequencyOffsetHz)
    {
        var tones = Ft4SymbolEncoder.ToneCount;
        var grid = new double[Ft4SymbolEncoder.SymbolCount * tones];
        baseband.TonePowerGrid(startSeconds, frequencyOffsetHz, grid);

        var correct = 0.0;
        var wrong = 0.0;

        for (var symbol = 0; symbol < Ft4SymbolEncoder.SymbolCount; symbol++)
        {
            if (Ft4SymbolEncoder.IsRampSymbol(symbol) || double.IsNaN(grid[symbol * tones]))
            {
                continue;
            }

            var total = 0.0;
            for (var tone = 0; tone < tones; tone++)
            {
                // The floor the grid folds in, undone exactly, before anything is summed - the same
                // inverse Ft4DeepSignalToNoise.Power applies and for the same reason.
                total += Math.Max(
                    0.0,
                    Math.Pow(10.0, grid[(symbol * tones) + tone] / 10.0)
                        - Ft4DeepSignalToNoise.DecibelFloor);
            }

            var here = Math.Max(
                0.0,
                Math.Pow(10.0, grid[(symbol * tones) + symbols[symbol]] / 10.0)
                    - Ft4DeepSignalToNoise.DecibelFloor);

            correct += here;
            wrong += (total - here) / (tones - 1);
        }

        return wrong <= 0.0 || correct - wrong <= 0.0
            ? double.NaN
            : (10.0 * Math.Log10((correct - wrong) / wrong))
                - Ft4DeepSignalToNoise.ReferenceOffsetDecibels;
    }

    /// <summary>A whole slot: silence, the signal at a chosen lead, silence.</summary>
    /// <remarks>
    /// <b>Written here rather than taken from <see cref="Ft4Waveform.SynthesizeSlot"/></b>, which
    /// centres the signal and therefore fixes the lead at 14760 samples. Placing a station where the
    /// analysis grid has nothing to lose, and where it has everything to lose, needs the lead to be a
    /// parameter.
    /// </remarks>
    private static float[] Slot(ReadOnlySpan<float> signal, int leadSamples)
    {
        var slot = new float[Ft4Waveform.SlotSampleCount(Rate)];
        signal.CopyTo(slot.AsSpan(leadSamples));
        return slot;
    }

    /// <summary>Prints one row of the table.</summary>
    private static void Report(
        ITestOutputHelper output,
        string placement,
        double rung,
        int trials,
        int decoded,
        IReadOnlyList<Reading> readings)
    {
        var rungText = double.IsNaN(rung) ? "  all" : $"{rung,5:F0}";

        if (readings.Count == 0)
        {
            output.WriteLine($"{placement,-12} {rungText} {trials,8} {decoded,8} {0,9}");
            return;
        }

        output.WriteLine(
            $"{placement,-12} {rungText} {trials,8} {decoded,8} {readings.Count,9} "
            + $"{MeanAbsoluteError(readings, r => r.Refined),9:F2} "
            + $"{Percentile95(readings, r => r.Refined),9:F2} "
            + $"{readings.Average(r => r.Refined - r.Delivered),10:F2} "
            + $"{MeanAbsoluteError(readings, r => r.Unrefined),9:F2} "
            + $"{Percentile95(readings, r => r.Unrefined),9:F2} "
            + $"{MeanAbsoluteError(readings, r => r.WithoutRamps),10:F2}");
    }

    /// <summary>The mean of the absolute differences from the delivered ratio, in decibels.</summary>
    private static double MeanAbsoluteError(
        IReadOnlyList<Reading> readings, Func<Reading, double> estimate) =>
        readings.Count == 0 ? double.NaN : readings.Average(r => Math.Abs(estimate(r) - r.Delivered));

    /// <summary>
    /// The 95th percentile of the absolute error, by nearest rank.
    /// </summary>
    /// <remarks>
    /// <b>Nearest rank, and it is stated because there are several conventions and they differ on
    /// small samples.</b> The errors are sorted ascending and the value at index
    /// <c>ceil(0.95 * n) - 1</c> is taken; no interpolation, so the figure quoted is always one that
    /// was actually measured.
    /// </remarks>
    private static double Percentile95(
        IReadOnlyList<Reading> readings, Func<Reading, double> estimate)
    {
        if (readings.Count == 0)
        {
            return double.NaN;
        }

        var errors = readings.Select(r => Math.Abs(estimate(r) - r.Delivered)).OrderBy(e => e).ToArray();
        var rank = (int)Math.Ceiling(0.95 * errors.Length) - 1;
        return errors[Math.Clamp(rank, 0, errors.Length - 1)];
    }

    /// <summary>
    /// <b>A cheap witness that the samples handed to the estimator came back untouched.</b>
    /// </summary>
    private static long Checksum(ReadOnlySpan<float> samples)
    {
        long hash = 17;
        foreach (var sample in samples)
        {
            hash = (hash * 31) + BitConverter.SingleToInt32Bits(sample);
        }

        return hash;
    }

    /// <summary>
    /// <b>The comparison, whole.</b> Five counts, then every message's text, candidate, frequency and
    /// dt, in order — because a comparison on text alone passes while the counts differ.
    /// </summary>
    private static void AssertIdentical(
        Ft8SlotResult first,
        Ft8SlotResult again,
        Ft8WaterfallGeometry geometry,
        string what)
    {
        Assert.True(first.CandidateCount == again.CandidateCount, $"{what}: candidate count");
        Assert.True(first.ParitySatisfiedCount == again.ParitySatisfiedCount, $"{what}: parity satisfied");
        Assert.True(first.ChecksumPassedCount == again.ChecksumPassedCount, $"{what}: checksum passed");
        Assert.True(first.BecameTextCount == again.BecameTextCount, $"{what}: became text");
        Assert.True(first.DuplicateCount == again.DuplicateCount, $"{what}: duplicate count");
        Assert.True(first.Messages.Count == again.Messages.Count, $"{what}: message count");

        for (var i = 0; i < first.Messages.Count; i++)
        {
            Assert.True(
                string.Equals(first.Messages[i].Text, again.Messages[i].Text, StringComparison.Ordinal),
                $"{what}: message {i} text");
            Assert.True(
                first.Messages[i].Candidate.Score == again.Messages[i].Candidate.Score,
                $"{what}: message {i} score");
            Assert.True(
                first.Messages[i].FrequencyHz(geometry) == again.Messages[i].FrequencyHz(geometry),
                $"{what}: message {i} frequency");
            Assert.True(
                first.Messages[i].TimeSeconds(geometry) == again.Messages[i].TimeSeconds(geometry),
                $"{what}: message {i} dt");
        }
    }
}
