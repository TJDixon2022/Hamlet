using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>What the collapse is made of.</b> Two decibels either side of where the aligned ladder stops
/// answering, read message by message rather than as a rate — and then the reading that gives unit
/// 217's on-air histogram a decibel value for the first time.
/// </summary>
/// <remarks>
/// <para>
/// <b>The question this file answers is not "where does it stop" — task 3 answered that — it is
/// "what stops".</b> A rung where the signal is still a candidate but nothing passes parity says
/// extraction or the code's correcting power; a rung where it stops being a candidate at all says
/// the search. Those are different faults with different addresses and the next unit needs to know
/// which.
/// </para>
/// <para>
/// <b>And the calibration.</b> Unit 217 put the on-air misses at a mean hard-decision agreement of
/// 122.8 of 174 against 167.7 for the matched and 84.8 for chance, and <b>nobody knew what SNR any of
/// those numbers corresponds to</b>, because agreement had never been measured on a signal of known
/// strength. Here it is, on signals whose true codeword is exact because this fixture generated it.
/// </para>
/// <para>
/// <b>Nothing is told to the decode path.</b> The frequency and offset are handed to the synthesizer;
/// the truth is used after the code has answered, to compare the text and to choose which candidate
/// the agreement is read at.
/// </para>
/// </remarks>
public class Ft8CollapseAnatomyTests
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;
    private const double OnGridHz = 1000.0;

    private readonly ITestOutputHelper _output;

    public Ft8CollapseAnatomyTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// <b>The last rung that returns most messages against the first that does not, message by
    /// message.</b>
    /// </summary>
    [Fact]
    public void WhatChangedBetweenTheLastGoodRungAndTheFirstBadOne()
    {
        // Task 3 printed the whole ladder before anything was asserted; these two rungs are the
        // pair it bracketed the collapse with, plus -21 because that is the ratio step 6 will
        // eventually be judged at. NOTHING IS ASSERTED ABOUT WHERE THE COLLAPSE IS.
        var rungs = new[] { -18.0, -20.0, -21.0 };

        var messages = EncodeCorpus.Build().Where(e => !e.CarriesHashedCallsign).ToArray();
        var offset = Ft8Waveform.SamplesPerSymbol(Rate) * 3;
        var decoder = new Ft8SlotDecoder();
        var search = new Ft8SyncSearch();
        var geometry = decoder.Geometry;

        _output.WriteLine("THE TWO RUNGS EITHER SIDE OF THE COLLAPSE, AND -21 BESIDE THEM.");
        _output.WriteLine($"  messages : {messages.Length}, the whole corpus filtered to !CarriesHashedCallsign");
        _output.WriteLine($"  seeds    : {SensitivityLadder.Seeds.Length}");
        _output.WriteLine($"  DECODES  : {rungs.Length} x {messages.Length} x {SensitivityLadder.Seeds.Length}"
            + $" = {rungs.Length * messages.Length * SensitivityLadder.Seeds.Length}");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"delivered",10} {"offered",8} {"back",6} {"rate %",8} {"WRONG",6} "
            + $"{"no cand",8} {"cand",7} {"par",7} {"agree back",11} {"agree missed",13}");

        var summaries = new List<(double Rung, double Delivered, int Offered, int Back, int Wrong,
            int NoCandidate, double Candidates, double Parity, List<int> BackAgree, List<int> MissAgree)>();

        foreach (var requested in rungs)
        {
            var delivered = new List<double>();
            var backAgree = new List<int>();
            var missAgree = new List<int>();
            var offered = 0;
            var back = 0;
            var wrong = 0;
            var noCandidate = 0;
            long candidates = 0;
            long parity = 0;

            foreach (var seed in SensitivityLadder.Seeds)
            {
                var noise = new GaussianNoise(seed + (int)Math.Round(requested * 10));

                foreach (var entry in messages)
                {
                    var (clean, _) = SearchFixture.OneSignal(Rate, entry, OnGridHz, offset);
                    var signalPower = SearchFixture.TransmissionPower(Rate, entry, OnGridHz);
                    var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, requested, Rate);
                    var mixed = SearchFixture.AddNoise(clean, noise, sigma, out var noisePower);
                    delivered.Add(SignalToNoise.DecibelsFor(signalPower, noisePower, Rate));

                    var waterfall = new Ft8Monitor(geometry).Analyse(mixed);
                    var result = decoder.Decode(waterfall);

                    var expected = Ft8MessageDecoder.Decode(entry.Message).Text;
                    var returned = result.Texts.Contains(expected, StringComparer.Ordinal);

                    offered++;
                    candidates += result.CandidateCount;
                    parity += result.ParitySatisfiedCount;
                    wrong += result.Texts.Count(t => !string.Equals(t, expected, StringComparison.Ordinal));
                    if (returned)
                    {
                        back++;
                    }

                    var nearest = SensitivityLadder.NearestTo(search.Find(waterfall), geometry, OnGridHz);
                    var agreement = SensitivityLadder.AgreementAt(
                        waterfall, nearest, SensitivityLadder.TrueCodeword(entry));

                    if (agreement < 0)
                    {
                        noCandidate++;
                    }
                    else if (returned)
                    {
                        backAgree.Add(agreement);
                    }
                    else
                    {
                        missAgree.Add(agreement);
                    }
                }
            }

            summaries.Add((requested, delivered.Average(), offered, back, wrong, noCandidate,
                candidates / (double)offered, parity / (double)offered, backAgree, missAgree));

            _output.WriteLine($"{delivered.Average(),10:F3} {offered,8} {back,6} "
                + $"{100.0 * back / offered,8:F1} {wrong,6} {noCandidate,8} "
                + $"{candidates / (double)offered,7:F1} {parity / (double)offered,7:F1} "
                + $"{(backAgree.Count == 0 ? "-" : backAgree.Average().ToString("F1")),11} "
                + $"{(missAgree.Count == 0 ? "-" : missAgree.Average().ToString("F1")),13}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  'no cand' is trials where the search kept NOTHING within four hertz of");
        _output.WriteLine("  where the fixture actually put the transmission - unit 217's own rule.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("WHAT CHANGED, stated from the columns above and not from preference:");

        var good = summaries[0];
        var bad = summaries[1];

        _output.WriteLine($"  the rate falls from {100.0 * good.Back / good.Offered:F1} per cent to "
            + $"{100.0 * bad.Back / bad.Offered:F1} per cent");
        _output.WriteLine($"  candidates per slot go {good.Candidates:F1} -> {bad.Candidates:F1}   "
            + $"(a change of {bad.Candidates - good.Candidates:F1})");
        _output.WriteLine($"  trials with NO candidate go {good.NoCandidate} -> {bad.NoCandidate} "
            + $"of {good.Offered}");
        _output.WriteLine($"  reaching parity per slot goes {good.Parity:F2} -> {bad.Parity:F2}");
        _output.WriteLine($"  mean agreement goes "
            + $"{good.BackAgree.Concat(good.MissAgree).Average():F1} -> "
            + $"{bad.BackAgree.Concat(bad.MissAgree).Average():F1} of 174");

        _output.WriteLine(string.Empty);
        _output.WriteLine("THE HISTOGRAM AT EACH RUNG. Agreement out of 174, misses and returns side");
        _output.WriteLine("by side, so a slope can be told from a cliff.");

        foreach (var summary in summaries)
        {
            _output.WriteLine(string.Empty);
            _output.WriteLine($"  delivered {summary.Delivered:F3} dB:");
            _output.WriteLine($"{"agreement",-12} {"back",7} {"missed",8}");

            var edges = new[] { 0, 90, 100, 110, 120, 130, 140, 150, 160, 165, 170, 174, 175 };
            for (var i = 0; i + 1 < edges.Length; i++)
            {
                var lo = edges[i];
                var hi = edges[i + 1];
                var backCount = summary.BackAgree.Count(a => a >= lo && a < hi);
                var missCount = summary.MissAgree.Count(a => a >= lo && a < hi);
                if (backCount == 0 && missCount == 0)
                {
                    continue;
                }

                _output.WriteLine($"{lo,4} to {hi - 1,-5} {backCount,7} {missCount,8}");
            }
        }

        var totalWrong = summaries.Sum(s => s.Wrong);
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  WRONG MESSAGES RETURNED ACROSS THESE THREE RUNGS: {totalWrong} out of "
            + $"{summaries.Sum(s => s.Offered)}");

        // The one thing that must always be true, whatever the rates say.
        Assert.Equal(0, totalWrong);
    }

    /// <summary>
    /// <b>The agreement curve against decibels, and unit 217's three on-air numbers read off it.</b>
    /// </summary>
    /// <remarks>
    /// <b>No decoding here, deliberately.</b> This measures what the ratios look like before error
    /// correction is asked to do anything with them, which is why it can be taken at every rung
    /// cheaply and why it is the right instrument for placing a number that unit 217 also measured
    /// before correction.
    /// </remarks>
    [Fact]
    public void TheAgreementCurveGivesUnit217sOnAirHistogramADecibelValue()
    {
        var messages = SensitivityLadder.Messages();
        var offset = Ft8Waveform.SamplesPerSymbol(Rate) * 3;
        var search = new Ft8SyncSearch();
        var geometry = new Ft8WaterfallGeometry(Rate);

        _output.WriteLine("HARD-DECISION AGREEMENT AGAINST SIGNAL-TO-NOISE RATIO, on signals whose");
        _output.WriteLine("true codeword is EXACT because this fixture generated it.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"requested",10} {"delivered",11} {"trials",8} {"no cand",8} {"mean agree",11} "
            + $"{"worst",7} {"best",6}");

        var curve = new List<(double Delivered, double Agreement)>();

        foreach (var requested in SensitivityLadder.Rungs)
        {
            var delivered = new List<double>();
            var agreements = new List<int>();
            var noCandidate = 0;

            foreach (var seed in SensitivityLadder.Seeds)
            {
                var noise = new GaussianNoise(seed + (int)Math.Round(requested * 10));

                foreach (var entry in messages)
                {
                    var (clean, _) = SearchFixture.OneSignal(Rate, entry, OnGridHz, offset);
                    var signalPower = SearchFixture.TransmissionPower(Rate, entry, OnGridHz);
                    var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, requested, Rate);
                    var mixed = SearchFixture.AddNoise(clean, noise, sigma, out var noisePower);
                    delivered.Add(SignalToNoise.DecibelsFor(signalPower, noisePower, Rate));

                    var waterfall = new Ft8Monitor(geometry).Analyse(mixed);
                    var nearest = SensitivityLadder.NearestTo(search.Find(waterfall), geometry, OnGridHz);
                    var agreement = SensitivityLadder.AgreementAt(
                        waterfall, nearest, SensitivityLadder.TrueCodeword(entry));

                    if (agreement < 0)
                    {
                        noCandidate++;
                    }
                    else
                    {
                        agreements.Add(agreement);
                    }
                }
            }

            var mean = agreements.Count == 0 ? double.NaN : agreements.Average();
            curve.Add((delivered.Average(), mean));

            _output.WriteLine($"{requested,10:F1} {delivered.Average(),11:F3} {agreements.Count,8} "
                + $"{noCandidate,8} {mean,11:F1} "
                + $"{(agreements.Count == 0 ? 0 : agreements.Min()),7} "
                + $"{(agreements.Count == 0 ? 0 : agreements.Max()),6}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("UNIT 217'S THREE ON-AIR NUMBERS READ OFF THAT CURVE, by linear interpolation");
        _output.WriteLine("between the two rungs each falls between:");
        _output.WriteLine(string.Empty);

        foreach (var (name, value) in new[]
                 {
                     ("matched, on air", 167.7),
                     ("MISSED, ON AIR ", 122.8),
                     ("chance, measured", 84.8),
                 })
        {
            _output.WriteLine($"  {name} {value,6:F1} of 174  ->  {Where(curve, value)}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  READ THIS AS ORDINAL AND NOT AS A CALIBRATION OF THE LIST'S OWN dB COLUMN.");
        _output.WriteLine("  What it says is: on THIS ladder's axis, a signal that produces the");
        _output.WriteLine("  agreement unit 217 measured on the on-air misses is a signal of about the");
        _output.WriteLine("  strength named beside it. It says nothing about what number the list's");
        _output.WriteLine("  writer would have printed beside such a signal.");

        // The curve must be a curve: agreement falls as the ratio falls. Asserted only after the
        // whole table is printed, and only on the two ends, which is all monotonicity a finite
        // sample supports.
        Assert.True(curve[0].Agreement > curve[^1].Agreement,
            "agreement did not fall as the signal-to-noise ratio fell.");
    }

    /// <summary>Where on the measured curve a given agreement figure sits, in decibels.</summary>
    private static string Where(IReadOnlyList<(double Delivered, double Agreement)> curve, double value)
    {
        if (value > curve[0].Agreement)
        {
            return $"ABOVE {curve[0].Delivered:F1} dB, off the top of this ladder";
        }

        for (var i = 0; i + 1 < curve.Count; i++)
        {
            var high = curve[i];
            var low = curve[i + 1];
            if (value <= high.Agreement && value >= low.Agreement)
            {
                var span = high.Agreement - low.Agreement;
                var fraction = span == 0 ? 0.5 : (value - low.Agreement) / span;
                var db = low.Delivered + (fraction * (high.Delivered - low.Delivered));
                return $"about {db,6:F1} dB   (between {low.Delivered:F1} and {high.Delivered:F1})";
            }
        }

        return $"BELOW {curve[^1].Delivered:F1} dB, off the bottom of this ladder";
    }
}
