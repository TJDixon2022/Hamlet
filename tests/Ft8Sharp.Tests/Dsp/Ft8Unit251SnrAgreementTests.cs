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
/// <b>The signal-to-noise ratio <c>Ft8DeepSignalToNoise</c> reports for a decoded message, measured
/// against the ratio the ladder actually delivered to it.</b> <c>PHASE_PLAN.md</c> step 2's second
/// must-pass exit.
/// </summary>
/// <remarks>
/// <para>
/// <b>THE LADDER IS LICENSED FOR THIS.</b> <c>docs/gate-set.md</c> rules in its own words that
/// <em>the ladder is a measurement, not a test</em>. Nothing here is walked through
/// <see cref="Ft8LadderHarness.Run"/>, because that entry point records three counts and throws the
/// audio away, and this needs the samples, the message and the delivered ratio of every trial. The
/// synthesis, the noise draw, the seed arithmetic and the delivered-ratio arithmetic below are
/// <b>the ladder's own, called rather than copied</b> — <see cref="Ft8Step6Ladder.Population"/>,
/// <see cref="SearchFixture.OneSignal"/>, <see cref="SearchFixture.TransmissionPower"/>,
/// <see cref="SignalToNoise.NoiseAmplitudeFor"/>, <see cref="SearchFixture.AddNoise"/> and
/// <see cref="SignalToNoise.DecibelsFor"/>, in that order, exactly as <c>Run</c> calls them. The
/// seed of block <c>s</c> at rung <c>d</c> is <c>DefaultSeed + s + round(d * 10)</c>, which is
/// <c>Run</c>'s line, so a trial here draws the noise a trial there would have drawn.
/// </para>
/// <para>
/// <b>THE ESTIMATE IS TAKEN AT THE PLACE THE DECODER REPORTS, NOT AT THE TRUTH.</b> That is the
/// whole point of the measurement: it is the number Hamlet will put on the screen, and Hamlet has
/// only what the decoder returned. <see cref="Ft8SlotMessage.Candidate"/> is the <b>coarse</b>
/// candidate even where fine sync moved it — <c>Ft8DeepSlotDecoder</c> adds
/// <c>new Ft8SlotMessage(candidate, result)</c> with the candidate it started from — so the place is
/// quantised to the waterfall's 0.080 s by 3.125 Hz cell, and the estimator has to find the signal
/// inside it. The frequency and the offset go to the synthesiser and to nothing else; the truth is
/// used after the decoder has answered, to compare the text, which is <c>Walk</c>'s rule and stays.
/// </para>
/// <para>
/// <b>AND THE SYMBOL SEQUENCE IS RECOVERED THE WAY HAMLET WILL HAVE TO RECOVER IT.</b> Not from the
/// corpus entry's own bits, which no receiver has, but by packing the decoded message again through
/// <see cref="Ft8DeepMessageSymbols"/>. Where that succeeds, <b>this test asserts the recovered
/// symbols are byte for byte the transmitted ones</b>. That assertion is the guard against the
/// failure mode the whole route has: a hashed callsign that packs to different bits than were sent
/// would give a ratio measured against a transmission nobody made.
/// </para>
/// <para>
/// <b>THE SELECTION EFFECT, STATED RATHER THAN HIDDEN.</b> An agreement figure is taken only over
/// messages that <em>decoded</em>. At a rung near the code's threshold only the lucky noise draws
/// decode, so the sample is biased toward trials whose noise happened to be kind, and an estimator
/// measured there sees a signal-to-noise ratio better than the one commanded. <b>The rungs here are
/// chosen high enough that the decode rate is near one</b> and the bias is small, and the decoded
/// count is printed at every rung so a reader can see where it is not. The rungs are
/// <see cref="Rungs"/>: -18, -15, -12, -9 and -6 dB. <b>Nothing is claimed at -21 dB</b>, where the
/// rate is about 11 per cent and the sample would be selected beyond use.
/// </para>
/// <para>
/// <b>TWO PLACEMENTS, BECAUSE G1 IS IN THE BREAKAGE RECORD.</b> Every figure this phase quoted
/// before unit 248 was taken exactly on a bin centre and exactly on a sub-block boundary, where the
/// coarse grid has nothing to lose. Both placements are walked here: <b>on grid</b>, and <b>at the
/// cell centre</b> — <c>+1.5625 Hz, +480 samples</c>, unit 248's own definition — which is where a
/// real station lands, because nothing on 14.074 arranges itself on Hamlet's analysis grid.
/// </para>
/// <para>
/// <b>NOTHING IS FITTED.</b> No constant in <c>Ft8DeepSignalToNoise</c> is adjusted by anything
/// measured here; <c>ReferenceOffsetDecibels</c> is <c>10 log10(2500 / 6.25)</c> and is derived in
/// <c>docs/unit251-snr-trace.md</c> §4. The refined and unrefined figures are both printed, so the
/// alignment search's contribution is visible rather than absorbed.
/// </para>
/// </remarks>
public class Ft8Unit251SnrAgreementTests(ITestOutputHelper output)
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    /// <summary>
    /// <b>The rungs, in decibels in the 2500 Hz reference bandwidth.</b> Chosen for a decode rate
    /// near one, so the agreement figure is not taken on a sample the decoder selected. Twelve
    /// decibels of span, so a systematic error in the reference constant would show as a bias and a
    /// scale error would show as a tilt.
    /// </summary>
    private static readonly double[] Rungs = [-18.0, -15.0, -12.0, -9.0, -6.0];

    /// <summary>
    /// One whole block of the population at each rung. <b>The population is 51 messages</b>, so this
    /// is a whole block and not a partial one, and the five rungs put <b>255 trials</b> through each
    /// placement.
    /// </summary>
    private const int TrialsPerRung = 51;

    /// <summary>
    /// <b>How many trials of each rung are decoded a second time to prove the estimate changed
    /// nothing.</b> Ten, spread across every rung and both placements, because a whole second walk
    /// would double the wall clock of a measurement whose cost is already the decoder's.
    /// </summary>
    private const int RedecodedPerRung = 10;

    /// <summary>The lowest tone the synthesiser is told to put the transmission on.</summary>
    private const double OnGridFrequencyHz = Ft8LadderHarness.DefaultFrequencyHz;

    /// <summary>
    /// <b>Half a waterfall frequency step.</b> The geometry oversamples frequency twice, so its
    /// step is half the 6.25 Hz tone spacing and the furthest a signal can sit from a bin centre is
    /// half of that again.
    /// </summary>
    private const double CellCentreFrequencyOffsetHz = 1.5625;

    /// <summary>
    /// <b>Half a waterfall time step: 480 samples at 12 kHz, which is 0.040 s.</b> The geometry
    /// oversamples time twice, so its step is half the 0.160 s symbol and the furthest a signal can
    /// sit from a block boundary is half of that again.
    /// </summary>
    private const int CellCentreOffsetSamples = 480;

    /// <summary>Where the synthesiser is told to put a transmission, and what to call it.</summary>
    private sealed record Placement(string Name, double FrequencyHz, int OffsetSamples);

    /// <summary>One decoded message, its delivered ratio and what the estimator made of it.</summary>
    private sealed record Reading(
        double Delivered, double Refined, double Unrefined, int Symbols, double TimeShift, double FrequencyShift);

    /// <summary>
    /// <b>THE MEASUREMENT.</b> Five rungs, two placements, 255 trials each, the estimate taken at
    /// the place the decoder reported and against the symbol sequence packed back out of the text.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>The decoder is the one Hamlet runs</b>: <c>Ft8DeepSlotDecoder</c> with ordered statistics
    /// and fine sync both on, which is what <c>Ft8Reader.Read</c> builds when nobody passes one.
    /// Measuring through a different configuration would be measuring a number the operator will
    /// never see.
    /// </para>
    /// <para>
    /// <b>What is asserted, and what is only reported.</b> Asserted: at least two hundred messages
    /// were measured; every recovered symbol sequence is the transmitted one; the whole
    /// <see cref="Ft8SlotResult"/> is identical when the same slot is decoded again after the
    /// estimate has been taken; the samples are unchanged; and the mean absolute error is inside
    /// <see cref="Bound"/>. Reported and not asserted: the 95th percentile, the bias, the per-rung
    /// counts and the unrefined figures. <b>A rung that reads badly is a measurement, not a
    /// failure</b>, and the step closes on the figure it reached.
    /// </para>
    /// </remarks>
    [Fact]
    public void TheEstimateAgreesWithTheCommandedRatioOverTwoHundredSynthesizedMessages()
    {
        var started = Stopwatch.StartNew();
        var geometry = new Ft8WaterfallGeometry();
        var population = Ft8Step6Ladder.Population();

        Placement[] placements =
        [
            new("on grid", OnGridFrequencyHz, Ft8LadderHarness.DefaultOffsetSamples),
            new(
                "cell centre",
                OnGridFrequencyHz + CellCentreFrequencyOffsetHz,
                Ft8LadderHarness.DefaultOffsetSamples + CellCentreOffsetSamples),
        ];

        var readings = new Dictionary<string, List<Reading>>(StringComparer.Ordinal);
        var repackRefused = 0;
        var notMeasured = 0;
        var decodedTotal = 0;
        var trialsTotal = 0;

        output.WriteLine(
            "placement     rung   trials  decoded  measured   MAE ref   p95 ref   bias ref"
            + "   MAE raw   p95 raw");

        foreach (var placement in placements)
        {
            var all = new List<Reading>();
            readings[placement.Name] = all;

            foreach (var rung in Rungs)
            {
                var atRung = new List<Reading>();

                // THE LADDER'S OWN SEED LINE. Block zero of this rung, so the noise is bit for bit
                // what Ft8LadderHarness.Run draws for the first 51 trials of the same rung.
                var noise = new GaussianNoise(
                    Ft8LadderHarness.DefaultSeed + (int)Math.Round(rung * 10.0));

                // ONE DECODER PER RUNG, not one per trial. It carries no state across slots that
                // matters here - the callsign cache is built inside Decode - and building it once
                // keeps the ordered statistics scratch out of the inner loop.
                var decoder = new Ft8DeepSlotDecoder(
                    osd: Ft8DeepOsdSettings.Default,
                    fineSync: Ft8DeepFineSyncSettings.Default);

                var decodedHere = 0;

                for (var trial = 0; trial < TrialsPerRung; trial++)
                {
                    var entry = population[trial % population.Count];
                    trialsTotal++;

                    var (clean, _) = SearchFixture.OneSignal(
                        Rate, entry, placement.FrequencyHz, placement.OffsetSamples);
                    var signalPower = SearchFixture.TransmissionPower(
                        Rate, entry, placement.FrequencyHz);
                    var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, rung, Rate);
                    var mixed = SearchFixture.AddNoise(clean, noise, sigma, out var noisePower);
                    var delivered = SignalToNoise.DecibelsFor(signalPower, noisePower, Rate);
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

                    // ROUTE A, AND THE GUARD THAT MAKES IT SAFE. The bits are packed back out of
                    // the words; where the round trip does not hold there is no measurement.
                    var symbols = Ft8DeepMessageSymbols.TryEncode(message.Result.Message);

                    if (symbols is null)
                    {
                        repackRefused++;
                        continue;
                    }

                    // AND THE ASSERTION THAT MAKES THE WHOLE MEASUREMENT MEAN ANYTHING. A recovered
                    // sequence that is not the transmitted one would give a ratio measured against
                    // a signal nobody sent, which is exactly the fault CLAUDE.md 0.0 names.
                    var truth = Ft8SymbolEncoder.Encode(entry.Message);
                    Assert.True(
                        truth.AsSpan().SequenceEqual(symbols),
                        $"{placement.Name} {rung:F0} dB trial {trial}: the symbols packed back out "
                        + $"of '{sent}' are not the symbols that were transmitted.");

                    var before = Checksum(mixed);

                    // THE PLACE THE DECODER REPORTED, BIASED BY THE ONE SYMBOL UNIT 248 MEASURED.
                    // Ft8SlotMessage.TimeSeconds is a candidate's nominal time and the signal
                    // starts one symbol period before it.
                    var start = message.TimeSeconds(geometry)
                        + Ft8DeepSlotDecoder.CandidateTimeBiasSeconds;
                    var baseband = Ft8DeepBaseband.Build(
                        mixed, Rate, message.FrequencyHz(geometry));

                    var refined = Ft8DeepSignalToNoise.Estimate(
                        baseband, start, 0.0, symbols, refine: true);
                    var unrefined = Ft8DeepSignalToNoise.Estimate(
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

                    // EXIT CRITERION FOUR, AND AN ASSERTION IS THE ONLY THING THAT MAKES IT A FACT.
                    // The same slot, through the same decoder, after the estimate has been taken.
                    if (trial < RedecodedPerRung)
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
                        refined.Symbols,
                        refined.TimeAdjustmentSeconds,
                        refined.FrequencyAdjustmentHz);

                    atRung.Add(reading);
                    all.Add(reading);
                }

                Report(output, placement.Name, rung, TrialsPerRung, decodedHere, atRung);
            }
        }

        output.WriteLine("");

        var everything = readings.Values.SelectMany(r => r).ToList();
        Report(output, "BOTH", double.NaN, trialsTotal, decodedTotal, everything);

        output.WriteLine("");
        output.WriteLine(
            $"trials {trialsTotal}, decoded {decodedTotal}, measured {everything.Count}, "
            + $"re-pack refused {repackRefused}, no measurement {notMeasured}");
        output.WriteLine(
            $"decoder: Ft8DeepSlotDecoder with ordered statistics and fine sync both on, "
            + $"which is what Ft8Reader.Read builds by default");
        output.WriteLine(
            $"reference offset: {Ft8DeepSignalToNoise.ReferenceOffsetDecibels:F4} dB, derived "
            + $"from a {Ft8DeepSignalToNoise.BinBandwidthHz:F4} Hz bin against "
            + $"{Ft8DeepSignalToNoise.ReferenceBandwidthHz:F0} Hz. Nothing was fitted.");
        output.WriteLine($"wall clock {started.Elapsed.TotalSeconds:F1} s");

        Assert.True(
            everything.Count >= 200,
            $"the step asks for at least two hundred synthesized messages and {everything.Count} "
            + "were measured. Cutting rungs is licensed; cutting the count is not.");

        var mae = MeanAbsoluteError(everything, r => r.Refined);

        Assert.True(
            mae <= Bound,
            $"the mean absolute error against the delivered ratio is {mae:F2} dB over "
            + $"{everything.Count} messages, against a bound of {Bound:F2} dB.");
    }

    /// <summary>
    /// <b>What the mean absolute error has to be inside for this to be a gate rather than a
    /// printout.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>This is not the plan's 2 dB and it is not trying to be.</b> The plan's threshold decides
    /// whether the <c>snr</c> column shows a number or keeps its dash, and that verdict was taken
    /// once, by unit 251, against the figure this test printed. <b>This bound is a regression
    /// guard</b>: <b>one decibel, against a measured mean absolute error of 0.26 dB over 510
    /// messages</b> — four times the headroom of the figure it guards, and tight enough to catch
    /// every way this number is known to break. A wrong reference constant is 26 dB. A Costas sync
    /// score substituted for a ratio is tens of decibels. Averaging the grid's decibels instead of
    /// inverting its floor first is 2.51 dB. Dropping
    /// <c>Ft8DeepSlotDecoder.CandidateTimeBiasSeconds</c> is a whole symbol and 2.5 dB. Measuring
    /// at the place the decoder reported without refining it is <b>3.50 dB</b>, and that is not a
    /// hypothetical: it is what this test printed on the run it was watched failing on.
    /// </para>
    /// <para>
    /// <b>The plan's own 2 dB would have been the looser choice</b> and would let a 1.5 dB
    /// regression through in silence, which is why it is not used here.
    /// </para>
    /// <para>
    /// <b>It is quoted here rather than buried in the assertion</b> so that a later unit changing it
    /// has to change a documented constant.
    /// </para>
    /// </remarks>
    private const double Bound = 1.0;

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
            + $"{Percentile95(readings, r => r.Unrefined),9:F2}");
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
    /// <b>A cheap witness that the samples handed to the estimator came back untouched.</b> Not a
    /// cryptographic hash and not trying to be: it is a running mix over every sample of a slot that
    /// this test owns and nothing else can see.
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
    /// <b>The comparison, whole.</b> Five counts, then every message's text, candidate, frequency
    /// and dt, in order — <c>Ft8DeepIdentityTests</c>' own shape, for the same reason: a comparison
    /// on text alone passes while the counts differ, and the counts are what this phase is read on.
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
