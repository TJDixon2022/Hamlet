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
/// <b>WHAT A LOUD NEIGHBOUR ACTUALLY COSTS A QUIET STATION, MEASURED BEFORE A SUBTRACTOR EXISTS.</b>
/// Two transmissions summed into one slot, across five frequency separations and four level
/// differences, with the ceiling — the same audio with the loud station absent and the identical
/// noise draw — beside every cell.
/// </summary>
/// <remarks>
/// <para>
/// <b>THIS IS MEASURED FIRST BECAUSE THE HONEST OUTCOME MIGHT BE ZERO.</b> The discipline is
/// <c>Ft8SecondPassMeasurementTests</c>'s opening remark, borrowed and nothing else taken from it —
/// <b>that file's "second pass" is a message-layer re-offer of payloads refused for an unresolved
/// callsign hash and touches no DSP; the collision is in the name only.</b> If no cell shows a gap
/// between the single pass and the ceiling, subtraction has nothing to recover on this instrument at
/// these separations, and that removes a hypothesis permanently and is worth as much as a fix.
/// </para>
/// <para>
/// <b>EVERY FIGURE THIS PROJECT HAS EVER TAKEN WAS TAKEN ON A SLOT CONTAINING EXACTLY ONE
/// STATION.</b> <c>Ft8LadderHarness.Run</c> places one transmission and adds noise, so sensitivity
/// is the only thing that has been measured. On 14.074 the thing that costs the operator a message
/// is more often occupancy than noise: one capture from the shack returned 80 candidates and seven
/// distinct messages from a single slot. This is the first measurement in the repository of what
/// that costs.
/// </para>
/// <para>
/// <b>THE BREAKAGE THIS WOULD HAVE CAUGHT.</b> A subtraction stage built and reported as a gain
/// against a single-pass column, with no ceiling beside it, cannot tell "subtraction recovered
/// four-fifths of what was there" from "the neighbour was never costing anything and the four
/// decodes are noise". Unit 252 could not say whether 41 of 306 beat 33 of 306 for a related reason
/// and had to leave the default where it was. <b>A gain quoted without its ceiling is a number with
/// no scale</b>, and this file is what puts the scale under §7 of
/// <c>docs/unit253-subtraction.md</c>.
/// </para>
/// <para>
/// <b>NOTHING HERE ASSERTS A RATE.</b> No bound, no target. The one thing asserted on every row is
/// <b>zero wrong</b> — a message returned that neither station sent — with the two sent messages
/// printed beside it. A cell that returns nothing is a measurement.
/// </para>
/// <para>
/// <b>THE CEILING IS COMPUTED ONCE PER TRIAL AND NOT ONCE PER CELL, AND THAT IS DELIBERATE.</b> The
/// ceiling column is the quiet station alone in the identical noise draw; the loud station's
/// separation and level do not enter it, so the same <c>float[]</c> serves all twenty cells. The
/// prediction in <c>docs/unit253-subtraction.md</c> §4.2 item 5 — that the ceiling is flat across
/// the grid — is therefore a property of the construction here rather than something this test
/// measures, and it is reported as such.
/// </para>
/// </remarks>
public class Ft8Unit253MaskingSurveyTests
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    /// <summary>Where the quiet station's lowest tone sits. <c>Ft8LadderHarness.DefaultFrequencyHz</c>.</summary>
    private const double QuietFrequencyHz = Ft8LadderHarness.DefaultFrequencyHz;

    /// <summary>
    /// <b>The rung the survey is walked at.</b> Above the port's measured 50 per cent crossing of
    /// -19.54 dB (<c>docs/unit246-osd.md</c> §4), so the ceiling is high enough for a neighbour to
    /// have something to take away. At -21 dB the unmasked column is 33 of 306 and a flat table
    /// would say nothing about masking.
    /// </summary>
    private const double RungDecibels = -18.0;

    /// <summary>
    /// <b>The stride from the quiet message to the loud one in the population.</b> Co-prime with 51,
    /// so the pairing is a fixed permutation, deterministic in a fresh process, and the two stations
    /// never carry the same text.
    /// </summary>
    private const int LoudMessageStride = 25;

    /// <summary>
    /// The separations walked, in hertz between the two stations' lowest tones. <b>50 Hz is the
    /// negative control</b>: a transmission is eight tones at 6.25 Hz, so at 50 Hz no tone bin is
    /// shared and the prediction is that nothing happens there.
    /// </summary>
    private static readonly double[] Separations = [0.0, 6.25, 12.5, 25.0, 50.0];

    /// <summary>
    /// The level differences walked, loud minus quiet, in decibels. <b>+13 dB is
    /// <c>PHASE_PLAN.md</c> step 4's own example</b> — a station at -5 dB sitting on one at -18.
    /// </summary>
    private static readonly double[] LevelDifferences = [0.0, 6.0, 13.0, 20.0];

    private readonly ITestOutputHelper _output;

    public Ft8Unit253MaskingSurveyTests(ITestOutputHelper output) => _output = output;

    private static int Offset => Ft8LadderHarness.DefaultOffsetSamples;

    /// <summary>
    /// <b>The amplitude parameter unit 253 added to <c>SearchFixture.Place</c> does not move the
    /// audio every recorded figure in this phase was taken on.</b>
    /// </summary>
    /// <remarks>
    /// <b>THE BREAKAGE THIS WOULD HAVE CAUGHT.</b> A fixture helper that several recorded
    /// measurements run through, given a new parameter, and the defaulted path quietly changed by a
    /// float-versus-double round trip. Nothing downstream would fail: the ladder would still run,
    /// still report, and still look sane — it would simply no longer be measuring the same audio as
    /// unit 246's 33 of 306 or <c>HM-OPEN-067</c>'s 13 of 306, and the drift would be invisible until
    /// somebody re-ran an old figure and got a different one. <b>Bit-identical is asserted, not
    /// nearly-identical</b>, because a tolerance here would swallow exactly the difference that
    /// matters.
    /// </remarks>
    [Fact]
    public void UnitAmplitudePlacesBitIdenticalSamplesToTheSynthesizersOwn()
    {
        var population = Ft8Step6Ladder.Population();
        Assert.NotEmpty(population);

        var checkedEntries = 0;

        foreach (var entry in population.Take(5))
        {
            var symbols = Ft8SymbolEncoder.Encode(entry.Message);
            var signal = Ft8Waveform.Synthesize(symbols, Rate, (float)QuietFrequencyHz);

            var byDefault = SearchFixture.EmptySlot(Rate);
            SearchFixture.Place(byDefault, Rate, entry, QuietFrequencyHz, Offset);

            var explicitOne = SearchFixture.EmptySlot(Rate);
            SearchFixture.Place(explicitOne, Rate, entry, QuietFrequencyHz, Offset, 1.0);

            var byHand = SearchFixture.EmptySlot(Rate);
            for (var i = 0; i < signal.Length; i++)
            {
                byHand[Offset + i] += signal[i];
            }

            // Sample for sample and not within a tolerance. Assert.Equal on float[] is ordinal.
            Assert.Equal(byHand, byDefault);
            Assert.Equal(byHand, explicitOne);
            checkedEntries++;
        }

        _output.WriteLine(
            $"{checkedEntries} messages placed through the defaulted amplitude and through an "
            + "explicit 1.0, both bit-identical to Ft8Waveform.Synthesize summed by hand over all "
            + $"{Ft8Waveform.SlotSampleCount(Rate)} samples of the slot.");

        // And the parameter does something when it is asked to: 20 dB is a factor of ten.
        var loud = SearchFixture.EmptySlot(Rate);
        SearchFixture.Place(loud, Rate, population[0], QuietFrequencyHz, Offset, 10.0);
        var quiet = SearchFixture.EmptySlot(Rate);
        SearchFixture.Place(quiet, Rate, population[0], QuietFrequencyHz, Offset);

        var ratio = SignalToNoise.MeanSquare(loud) / SignalToNoise.MeanSquare(quiet);
        _output.WriteLine($"amplitude 10.0 delivers {10.0 * Math.Log10(ratio):F3} dB of extra power.");
        Assert.Equal(20.0, 10.0 * Math.Log10(ratio), 3);
    }

    /// <summary>
    /// <b>THE SURVEY. Twenty cells, one whole block of the 51-message population in each, with the
    /// ceiling beside every one.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>The audio, per trial.</b> The quiet station is placed exactly as
    /// <c>Ft8LadderHarness.Run</c> places its one station — same population order, same seed
    /// arithmetic <c>seed + block + round(rung * 10)</c>, same frequency, same offset, same
    /// <c>SearchFixture.AddNoise</c> call — and the array that call returns <b>is</b> the ceiling
    /// column. The masked column is a clone of that array with one more transmission summed into it.
    /// <b>The noise is not merely drawn from the same distribution; it is the same array</b>, so a
    /// cell and its ceiling differ in exactly one thing.
    /// </para>
    /// <para>
    /// <b>What is scored.</b> The quiet station's text and nothing else. The loud station's text is
    /// known and is excluded from the wrong count — it was sent, so returning it is correct, and
    /// counting it as wrong would make every cell fail for the decoder doing its job. Anything
    /// returned that <b>neither</b> station sent is wrong and is asserted to be zero.
    /// </para>
    /// <para>
    /// <b>Ordered statistics and fine sync are off.</b> <c>new Ft8DeepSlotDecoder()</c> with no
    /// arguments is the port exactly — <c>Ft8DeepIdentityTests</c> asserts the whole result — so the
    /// single-pass column here is the same decode the recorded rows were taken with, and the only
    /// difference between this survey and the ladder is the second transmission.
    /// </para>
    /// </remarks>
    [Fact]
    public void AQuietStationBehindALoudOneIsSurveyedAcrossSeparationAndLevel()
    {
        var population = Ft8Step6Ladder.Population();
        var trials = population.Count;
        var decoder = new Ft8DeepSlotDecoder();
        var rungOffset = (int)Math.Round(RungDecibels * 10.0);
        var blockSeed = Ft8LadderHarness.DefaultSeed + 0 + rungOffset;

        _output.WriteLine(
            $"THE MASKING SURVEY. quiet station at {RungDecibels:F1} dB requested, "
            + $"{QuietFrequencyHz:F2} Hz, offset {Offset} samples; one whole block of "
            + $"{trials} messages a cell; seed {blockSeed}.");
        _output.WriteLine(
            $"loud station: population[(i + {LoudMessageStride}) mod {trials}], same offset, "
            + "frequency and amplitude per cell.");
        _output.WriteLine(string.Empty);

        // The ceiling: the quiet station alone in the identical noise draw. Built and decoded ONCE
        // because it does not depend on the loud station's separation or level - see the remarks.
        var ceilingDecoded = 0;
        var ceilingWrong = new List<string>();
        var quietSlots = new float[trials][];
        var quietTexts = new string[trials];
        var loudTexts = new string[trials];
        var deliveredTotal = 0.0;

        var noise = new GaussianNoise(blockSeed);
        var ceilingClock = new Stopwatch();

        for (var trial = 0; trial < trials; trial++)
        {
            var quiet = population[trial];
            var (clean, _) = SearchFixture.OneSignal(Rate, quiet, QuietFrequencyHz, Offset);
            var signalPower = SearchFixture.TransmissionPower(Rate, quiet, QuietFrequencyHz);
            var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, RungDecibels, Rate);
            quietSlots[trial] = SearchFixture.AddNoise(clean, noise, sigma, out var noisePower);
            deliveredTotal += SignalToNoise.DecibelsFor(signalPower, noisePower, Rate);

            quietTexts[trial] = Ft8MessageDecoder.Decode(quiet.Message).Text;
            loudTexts[trial] = Ft8MessageDecoder.Decode(
                population[(trial + LoudMessageStride) % trials].Message).Text;

            ceilingClock.Start();
            var result = decoder.Decode(quietSlots[trial]);
            ceilingClock.Stop();

            if (result.Texts.Contains(quietTexts[trial], StringComparer.Ordinal))
            {
                ceilingDecoded++;
            }

            foreach (var text in result.Texts)
            {
                if (!string.Equals(text, quietTexts[trial], StringComparison.Ordinal))
                {
                    ceilingWrong.Add($"    trial {trial,3}  SENT \"{quietTexts[trial]}\"  RETURNED \"{text}\"");
                }
            }
        }

        var delivered = deliveredTotal / trials;

        _output.WriteLine(
            $"CEILING (loud station absent, identical noise): {ceilingDecoded} of {trials}, "
            + $"delivered {delivered:F3} dB, {ceilingClock.Elapsed.TotalMilliseconds / trials:F1} ms a trial.");
        _output.WriteLine(string.Empty);
        _output.WriteLine(
            "  sep Hz   level dB   single pass   ceiling   gap   LOUD   WRONG   ms/tr   worst slot ms");

        var rows = new List<(double Separation, double Level, int Single, int Gap, int Loud)>();
        var allWrong = new List<string>();
        var worstOverall = 0.0;

        foreach (var separation in Separations)
        {
            foreach (var level in LevelDifferences)
            {
                var amplitude = Math.Pow(10.0, level / 20.0);
                var decoded = 0;
                var loudDecoded = 0;
                var wrong = new List<string>();
                var clock = new Stopwatch();
                var worstSlot = 0.0;

                for (var trial = 0; trial < trials; trial++)
                {
                    var masked = (float[])quietSlots[trial].Clone();
                    SearchFixture.Place(
                        masked,
                        Rate,
                        population[(trial + LoudMessageStride) % trials],
                        QuietFrequencyHz + separation,
                        Offset,
                        amplitude);

                    var slotClock = Stopwatch.StartNew();
                    clock.Start();
                    var result = decoder.Decode(masked);
                    clock.Stop();
                    slotClock.Stop();
                    worstSlot = Math.Max(worstSlot, slotClock.Elapsed.TotalMilliseconds);

                    if (result.Texts.Contains(quietTexts[trial], StringComparer.Ordinal))
                    {
                        decoded++;
                    }

                    // THE COLUMN THAT DECIDES WHICH CELL THE LADDER WALKS. A subtractor can only
                    // subtract a message the first pass decoded. A cell where the loud station is
                    // itself lost has nothing to subtract, so its gap - however large - is a gap
                    // multi-pass cannot touch, and picking it would be picking a cell where the
                    // answer is known to be zero before the ladder runs.
                    if (result.Texts.Contains(loudTexts[trial], StringComparer.Ordinal))
                    {
                        loudDecoded++;
                    }

                    // WRONG is anything NEITHER station sent. The loud station's own text is a
                    // correct return - it was transmitted - and counting it would fail every cell
                    // for the decoder working.
                    foreach (var text in result.Texts)
                    {
                        if (!string.Equals(text, quietTexts[trial], StringComparison.Ordinal)
                            && !string.Equals(text, loudTexts[trial], StringComparison.Ordinal))
                        {
                            wrong.Add(
                                $"    sep {separation:F2} Hz level {level:F0} dB trial {trial,3}  "
                                + $"QUIET SENT \"{quietTexts[trial]}\"  LOUD SENT \"{loudTexts[trial]}\"  "
                                + $"RETURNED \"{text}\"");
                        }
                    }
                }

                worstOverall = Math.Max(worstOverall, worstSlot);
                allWrong.AddRange(wrong);
                rows.Add((separation, level, decoded, ceilingDecoded - decoded, loudDecoded));

                _output.WriteLine(
                    $"{separation,8:F2} {level,10:F0} {decoded,13} {ceilingDecoded,9} "
                    + $"{ceilingDecoded - decoded,5} {loudDecoded,6} {wrong.Count,7} "
                    + $"{clock.Elapsed.TotalMilliseconds / trials,7:F1} {worstSlot,15:F1}");
            }
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine(
            $"  worst observed slot over the whole survey: {worstOverall:F1} ms, a margin of "
            + $"{15000.0 / Math.Max(worstOverall, 0.001):F0}x against FT8's 15 000 ms.");
        _output.WriteLine(string.Empty);

        if (allWrong.Count == 0)
        {
            _output.WriteLine("NO WRONG DECODES. 0 messages returned that neither station sent.");
        }
        else
        {
            _output.WriteLine($"{allWrong.Count} WRONG, each on its own line:");
            foreach (var line in allWrong)
            {
                _output.WriteLine(line);
            }
        }

        foreach (var line in ceilingWrong)
        {
            _output.WriteLine($"  ceiling column: {line}");
        }

        // THE DECISION THIS SURVEY EXISTS TO PRODUCE: which cell the ladder walks.
        //
        // THREE CONDITIONS AND ALL THREE ARE NECESSARY. The single pass must lose the quiet
        // message; the ceiling must say it was recoverable; and THE LOUD STATION MUST DECODE ON
        // THE FIRST PASS, because a subtractor subtracts a decoded message and a cell where the
        // loud station is itself lost is a cell where multi-pass is arithmetically incapable of
        // doing anything at all. Picking the largest gap without the third condition picks a cell
        // whose answer is known to be zero before the ladder is walked.
        var half = trials / 2;
        var eligible = rows.Where(r => r.Loud >= half).ToArray();

        _output.WriteLine(string.Empty);
        _output.WriteLine("THE DECISION:");
        _output.WriteLine(
            $"  cells with a gap at all:                            {rows.Count(r => r.Gap > 0)} of {rows.Count}");
        _output.WriteLine(
            $"  cells where the loud station decoded in >= {half,2} trials: {eligible.Length} of {rows.Count}");
        _output.WriteLine(
            $"  cells that are both - a gap AND something to subtract: "
            + $"{eligible.Count(r => r.Gap > 0)} of {rows.Count}");

        var best = eligible
            .Where(r => r.Gap > 0)
            .OrderByDescending(r => r.Gap)
            .ThenBy(r => r.Separation)
            .ThenBy(r => r.Level)
            .FirstOrDefault();

        if (ceilingDecoded == 0 || best == default)
        {
            _output.WriteLine(
                "  NO CELL QUALIFIES. Either no cell shows a gap, or every cell that shows one is "
                + "a cell where the loud station is lost too and there is nothing for a subtractor "
                + "to subtract. Subtraction has nothing to recover on this instrument at these "
                + "separations and the step closes on that figure.");
        }
        else
        {
            _output.WriteLine(
                $"  THE LADDER WALKS separation {best.Separation:F2} Hz, level difference "
                + $"{best.Level:F0} dB: the single pass returns {best.Single} of {trials} where the "
                + $"ceiling returns {ceilingDecoded} of {trials}, a gap of {best.Gap}, and the loud "
                + $"station itself decodes in {best.Loud} of {trials} - so there is a message to "
                + "subtract on the first pass and a message to find underneath it.");
        }

        // THE ONE ASSERTION. No bound on any rate; a cell that returns nothing is a measurement.
        Assert.Empty(allWrong);
        Assert.Empty(ceilingWrong);
    }
}
