// Copyright (c) Hamlet contributors. Licensed under the MIT licence.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ft8Sharp.Deep;
using Ft8Sharp.Dsp;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>Unit 248 task 7: does re-syncing underneath combining recover what placement jitter took from
/// step 6?</b> <c>HM-OPEN-075</c> says it should and names this step as the work that would.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this is not run through <c>Ft8LadderHarness.RunRepeats</c> and why nothing was modified to
/// make it possible.</b> <c>RunRepeats</c> builds its combined column as
/// <c>new Ft8DeepRepeatDecoder(combining: rule)</c> with the default inner slot decoder, and there
/// is no argument that reaches that inner decoder. Configuring fine sync underneath combining would
/// need either a change to <c>RunRepeats</c> - which every row this phase has recorded came through
/// and which unit 248 was told not to touch - or a change to <c>Ft8DeepRepeatDecoder</c>, which
/// step 6 owns and which unit 248 was told not to touch either. <b>Both are left alone.</b>
/// </para>
/// <para>
/// <b>So the trial construction is reproduced here exactly rather than the harness being changed.</b>
/// Slot <c>r</c> of block <c>s</c> at rung <c>d</c> draws from
/// <c>seed + s + round(d * 10) + r * RepeatSeedStride</c>, one <c>GaussianNoise</c> per repeat per
/// block, drawn in the population's fixed order - which is <c>RunRepeats</c>'s own arithmetic, read
/// off the harness and not invented. <b><see cref="TheUnmodifiedHarnessRowIsTheControl"/> runs
/// <c>RunRepeats</c> itself, unmodified, on the same rung and the same jitter</b>, and its combined
/// column is what the fine-sync column below is read against. If the two constructions had drifted,
/// that test's single-slot column would not equal the port's own.
/// </para>
/// <para>
/// <b>Scored the way <c>RunRepeats</c> scores</b>: the union over the trial's slots, because that is
/// what a decoder fed slots in order actually puts in front of an operator.
/// </para>
/// </remarks>
public class Ft8Unit248RepeatsTests(ITestOutputHelper output)
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    private const int Trials = 306;

    private const double Rung = -21.0;

    /// <summary>The jitter unit 247 used and <c>HM-OPEN-075</c> quotes: a third of a tone.</summary>
    private const double FrequencyJitterHz = 2.0;

    /// <summary>A quarter of a symbol period.</summary>
    private const int OffsetJitterSamples = 480;

    /// <summary>
    /// <b>The control: <c>Ft8LadderHarness.RunRepeats</c> unmodified, at the rung and the jitter
    /// unit 247 measured.</b>
    /// </summary>
    /// <remarks>
    /// Unit 247 left this at <b>68 of 306, 22.2 per cent, zero wrong</b> in the combined column with
    /// <b>55 of 306</b> trials that no single slot decoded alone. <b>Asserted rather than assumed</b>,
    /// because unit 248 changed <c>Ft8DeepSlotDecoder</c>'s samples-carrying entry point and a claim
    /// that it changed no decision is worth what it is measured at.
    /// </remarks>
    [Fact]
    public void TheUnmodifiedHarnessRowIsTheControl()
    {
        var run = Ft8LadderHarness.RunRepeats(
            Rung,
            Trials,
            repeats: 2,
            frequencyJitterHz: FrequencyJitterHz,
            offsetJitterSamples: OffsetJitterSamples);

        foreach (var line in Ft8LadderHarness.RepeatsReport(run))
        {
            output.WriteLine(line);
        }

        foreach (var row in run.Rows)
        {
            Assert.Equal(0, row.Wrong);
        }

        Assert.Equal(0, run.LostByCombining);
        Assert.Equal(13, run.Rows[0].Decoded);
        Assert.Equal(68, run.Rows[2].Decoded);
        Assert.Equal(55, run.OnlyCombined);

        output.WriteLine(string.Empty);
        output.WriteLine("Exactly what unit 247 left: 13 of 306 for a single slot, 68 of 306 combined,");
        output.WriteLine("55 of 306 that no single slot reached, zero wrong. Nothing tonight moved it.");
    }

    /// <summary>
    /// <b>The same trials with fine sync on underneath combining.</b>
    /// </summary>
    /// <remarks>
    /// <b>Nothing is told to any decode path.</b> The frequencies and offsets go to the synthesiser;
    /// each decoder is handed samples and the truth is used once, after the code has answered, to
    /// compare the text.
    /// </remarks>
    [Fact]
    public void FineSyncUnderneathCombiningAtMinus21DecibelsWithTheJitterOn()
    {
        var population = Ft8Step6Ladder.Population();
        var rungOffset = (int)Math.Round(Rung * 10.0);

        var combinedOnly = new Ft8DeepRepeatDecoder(
            new Ft8DeepSlotDecoder(rememberHearings: true),
            Ft8DeepCombineSettings.Default);

        var combinedWithFineSync = new Ft8DeepRepeatDecoder(
            new Ft8DeepSlotDecoder(
                rememberHearings: true, fineSync: Ft8DeepFineSyncSettings.Default),
            Ft8DeepCombineSettings.Default);

        var columns = new (string Name, Ft8DeepRepeatDecoder Decoder)[]
        {
            ("combined x2", combinedOnly),
            ("combined x2 + fine sync", combinedWithFineSync),
        };

        var decoded = new int[columns.Length];
        var wrong = new int[columns.Length];
        var onlyCombined = new int[columns.Length];
        var clocks = columns.Select(_ => new Stopwatch()).ToArray();
        var wrongLines = new List<string>();
        var worstSlot = new double[columns.Length];

        var singleSlotDecoded = 0;
        var port = new Ft8SlotDecoder();

        var trial = 0;
        for (var block = 0; trial < Trials; block++)
        {
            var noise = new[]
            {
                new GaussianNoise(Ft8LadderHarness.DefaultSeed + block + rungOffset),
                new GaussianNoise(
                    Ft8LadderHarness.DefaultSeed + block + rungOffset
                        + Ft8LadderHarness.RepeatSeedStride),
            };

            foreach (var entry in population)
            {
                if (trial >= Trials)
                {
                    break;
                }

                var slots = new float[2][];
                for (var r = 0; r < 2; r++)
                {
                    var slotFrequency =
                        Ft8LadderHarness.DefaultFrequencyHz + (r * FrequencyJitterHz);
                    var slotOffset =
                        Ft8LadderHarness.DefaultOffsetSamples + (r * OffsetJitterSamples);

                    var (clean, _) = SearchFixture.OneSignal(Rate, entry, slotFrequency, slotOffset);
                    var signalPower = SearchFixture.TransmissionPower(Rate, entry, slotFrequency);
                    var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, Rung, Rate);
                    slots[r] = SearchFixture.AddNoise(clean, noise[r], sigma, out _);
                }

                var sent = Ft8MessageDecoder.Decode(entry.Message).Text;

                var alone = slots.Any(s => port.Decode(s).Texts.Contains(sent, StringComparer.Ordinal));
                if (alone)
                {
                    singleSlotDecoded++;
                }

                for (var c = 0; c < columns.Length; c++)
                {
                    columns[c].Decoder.Reset();
                    var texts = new List<string>();

                    for (var r = 0; r < 2; r++)
                    {
                        var slotClock = Stopwatch.StartNew();
                        clocks[c].Start();
                        var result = columns[c].Decoder.Decode(slots[r]);
                        clocks[c].Stop();
                        slotClock.Stop();

                        worstSlot[c] = Math.Max(worstSlot[c], slotClock.Elapsed.TotalMilliseconds);
                        texts.AddRange(result.Texts);
                    }

                    if (texts.Contains(sent, StringComparer.Ordinal))
                    {
                        decoded[c]++;
                        if (!alone)
                        {
                            onlyCombined[c]++;
                        }
                    }

                    foreach (var text in texts.Where(t => !string.Equals(t, sent, StringComparison.Ordinal)).Distinct())
                    {
                        wrong[c]++;
                        wrongLines.Add(
                            $"    trial {trial,5}  {columns[c].Name}  SENT \"{sent}\"  RETURNED \"{text}\"");
                    }
                }

                trial++;
            }
        }

        output.WriteLine(
            "=============================================================================");
        output.WriteLine($"FINE SYNC UNDERNEATH COMBINING at {Rung:F1} dB, {Trials} trials, "
            + $"two slots a trial");
        output.WriteLine($"  the later slot sits {FrequencyJitterHz:F2} Hz and {OffsetJitterSamples} "
            + $"samples ({OffsetJitterSamples / (double)Rate:F3} s) from the earlier one");
        output.WriteLine(
            "=============================================================================");
        output.WriteLine($"  a single slot alone decoded {singleSlotDecoded} of {Trials}");
        output.WriteLine(string.Empty);
        output.WriteLine("  column                    DECODED   rate    lo 95   hi 95   WRONG   "
            + "only-combined   worst slot ms");
        output.WriteLine(new string('-', 110));

        for (var c = 0; c < columns.Length; c++)
        {
            var (lower, upper) = Ft8Step6Ladder.Wilson(decoded[c], Trials);
            output.WriteLine(
                $"  {columns[c].Name,-24} {decoded[c],7}  {100.0 * decoded[c] / Trials,6:F1} "
                + $"{lower,7:F1} {upper,7:F1}  {wrong[c],6}   {onlyCombined[c],13}   "
                + $"{worstSlot[c],13:F1}");
        }

        foreach (var line in wrongLines)
        {
            output.WriteLine(line);
        }

        output.WriteLine(string.Empty);
        output.WriteLine("  Unit 247 left this at 68 of 306 combined with 55 of 306 only-combined,");
        output.WriteLine("  and HM-OPEN-075 named step 4 as the work that would recover the rest.");

        foreach (var count in wrong)
        {
            Assert.Equal(0, count);
        }
    }
}
