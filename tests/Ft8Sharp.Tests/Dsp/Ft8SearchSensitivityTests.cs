using Ft8Sharp.Dsp;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// How far down the search still hears: the find rate against the signal-to-noise ratio, with the
/// score at a true signal's position set beside the best score noise alone produced at the same
/// ratio.
/// </summary>
/// <remarks>
/// <para>
/// <b>This is provisioning for steps 5 and 6, not an exit criterion.</b> Step 6 will measure a
/// decode rate, and it cannot start without knowing what the <em>search</em> costs in sensitivity,
/// because a signal never found is a message never decoded.
/// </para>
/// <para>
/// <b>It is a measurement and nothing is tuned to improve it.</b> No threshold is moved, no
/// tolerance widened, no minimum lowered. The numbers are what they are.
/// </para>
/// <para>
/// <b>It is not compared with the published sensitivity figure and must not be.</b> That number is
/// about decodes, and error correction stands between a found signal and a decoded one. Naming it
/// here would invite a comparison this unit cannot support: <b>nothing in this library demodulates
/// anything.</b>
/// </para>
/// </remarks>
public class Ft8SearchSensitivityTests
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    /// <summary>Messages measured at each ratio. Enough for a shape rather than an anecdote.</summary>
    private const int MessagesPerPoint = 20;

    /// <summary>Slots of noise alone measured at each ratio, for the false-alarm side.</summary>
    private const int NoiseSlotsPerPoint = 5;

    private static double FrequencyToleranceHz =>
        (new Ft8WaterfallGeometry().TransformBinSpacingHz / 2) + 0.001;

    private const double TimeBiasSeconds = Ft8WaterfallGeometry.SymbolPeriodSeconds;
    private const double TimeToleranceSeconds = Ft8WaterfallGeometry.SymbolPeriodSeconds / 2;

    private readonly ITestOutputHelper _output;

    public Ft8SearchSensitivityTests(ITestOutputHelper output) => _output = output;

    private sealed record Point(
        double Requested,
        double Delivered,
        int Found,
        int Total,
        int WorstTrueScore,
        double MeanTrueScore,
        int BestFalseScore,
        double MeanFalseScore,
        int MeanCandidates);

    [Fact]
    public void TheFindRateAndTheScoresAreSweptAcrossSignalToNoiseRatio()
    {
        var corpus = EncodeCorpus.Build();
        var search = new Ft8SyncSearch();
        var geometry = new Ft8WaterfallGeometry();
        var binHz = geometry.TransformBinSpacingHz;

        var frequencies = new[] { 1000.0, 1000.0 + (binHz / 2), 1000.0 + (binHz / 4) };
        var offsets = new[] { 0, 1920 * 3, 960 * 5, 5000, 12345 };

        var ratios = new[] { -4.0, -8.0, -11.0, -13.0, -15.0, -17.0, -19.0, -21.0, -24.0 };
        var points = new List<Point>();

        foreach (var requested in ratios)
        {
            var noise = new GaussianNoise(seed: 214_070 + (int)(-requested * 10));
            var delivered = new List<double>();
            var trueScores = new List<int>();
            var found = 0;

            for (var i = 0; i < MessagesPerPoint; i++)
            {
                var entry = corpus[i % corpus.Count];
                var frequency = frequencies[i % frequencies.Length];
                var offset = offsets[i % offsets.Length];

                var signalPower = SearchFixture.TransmissionPower(Rate, entry, frequency);
                var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, requested, Rate);

                var (slot, truth) = SearchFixture.OneSignal(Rate, entry, frequency, offset);
                var audio = SearchFixture.AddNoise(slot, noise, sigma, out var noisePower);
                delivered.Add(SignalToNoise.DecibelsFor(signalPower, noisePower, Rate));

                // ---- the test's knowledge stops here ----
                var waterfall = new Ft8Monitor().Analyse(audio);
                var candidates = search.Find(waterfall);
                // ---- and starts again here ----

                var truthTime = truth.TimeSeconds(Rate);
                var hit = false;
                foreach (var candidate in candidates)
                {
                    var frequencyError = candidate.FrequencyHz(waterfall.Geometry) - frequency;
                    var timeError =
                        candidate.TimeSeconds(waterfall.Geometry) - truthTime - TimeBiasSeconds;

                    if (Math.Abs(frequencyError) <= FrequencyToleranceHz
                        && Math.Abs(timeError) <= TimeToleranceSeconds)
                    {
                        trueScores.Add(candidate.Score);
                        hit = true;
                        break;
                    }
                }

                if (hit)
                {
                    found++;
                }
                else
                {
                    // Not found, so the score that mattered is whatever was there at its own bins.
                    trueScores.Add(BestScoreOverBins(waterfall, frequency));
                }
            }

            // The other half of the picture at the same ratio: what noise alone produced.
            var falseScores = new List<int>();
            var counts = new List<int>();
            for (var slot = 0; slot < NoiseSlotsPerPoint; slot++)
            {
                var reference = SearchFixture.TransmissionPower(Rate, corpus[0], 1000.0);
                var sigma = SignalToNoise.NoiseAmplitudeFor(reference, requested, Rate);
                var empty = SearchFixture.EmptySlot(Rate);
                var audio = SearchFixture.AddNoise(empty, noise, sigma, out _);

                var candidates = search.Find(new Ft8Monitor().Analyse(audio));
                counts.Add(candidates.Count);
                falseScores.Add(candidates.Count > 0 ? candidates[0].Score : int.MinValue);
            }

            points.Add(new Point(
                requested,
                delivered.Average(),
                found,
                MessagesPerPoint,
                trueScores.Min(),
                trueScores.Average(),
                falseScores.Max(),
                falseScores.Where(s => s != int.MinValue).DefaultIfEmpty(0).Average(),
                (int)Math.Round(counts.Average())));
        }

        _output.WriteLine(
            $"  {MessagesPerPoint} messages and {NoiseSlotsPerPoint} noise-only slots at each ratio.");
        _output.WriteLine(
            "  'true score' is the score at the found candidate, or the best score over the signal's");
        _output.WriteLine("  own bins where it was not found. 'false score' is the top of the list over");
        _output.WriteLine("  noise alone at the same ratio.");
        _output.WriteLine(string.Empty);
        _output.WriteLine(
            $"  {"asked",7} {"delivered",10} {"found",9} {"rate",7} {"worst true",11} {"mean true",10} "
            + $"{"best false",11} {"mean false",11} {"cands",6}");

        foreach (var point in points)
        {
            _output.WriteLine(
                $"  {point.Requested,7:F1} {point.Delivered,10:F3} {point.Found,4}/{point.Total,-4} "
                + $"{100.0 * point.Found / point.Total,6:F1}% {point.WorstTrueScore,11} "
                + $"{point.MeanTrueScore,10:F1} {point.BestFalseScore,11} {point.MeanFalseScore,11:F1} "
                + $"{point.MeanCandidates,6}");
        }

        _output.WriteLine(string.Empty);

        var lastClean = points.LastOrDefault(p => p.Found == p.Total);
        var firstOverlap = points.FirstOrDefault(p => p.WorstTrueScore <= p.BestFalseScore);
        var firstMiss = points.FirstOrDefault(p => p.Found < p.Total);

        _output.WriteLine(
            lastClean is null
                ? "  no ratio in this sweep found every message."
                : $"  EVERY MESSAGE FOUND DOWN TO {lastClean.Delivered:F3} dB DELIVERED.");
        _output.WriteLine(
            firstMiss is null
                ? "  nothing was missed anywhere in this sweep."
                : $"  THE FIRST MISS IS AT {firstMiss.Delivered:F3} dB: "
                  + $"{firstMiss.Found} of {firstMiss.Total}.");
        _output.WriteLine(
            firstOverlap is null
                ? "  THE TWO DISTRIBUTIONS NEVER OVERLAP ANYWHERE IN THIS SWEEP: at every ratio "
                  + "measured, the weakest true score is still above the strongest noise-alone score."
                : $"  THE DISTRIBUTIONS BEGIN TO OVERLAP AT {firstOverlap.Delivered:F3} dB, where the "
                  + $"weakest true score {firstOverlap.WorstTrueScore} has fallen to or below the "
                  + $"best noise-alone score {firstOverlap.BestFalseScore}. That is the ratio at "
                  + "which a threshold can no longer separate them.");

        _output.WriteLine(string.Empty);
        _output.WriteLine(
            "  Reported as a measurement. Nothing was tuned to improve it, and it is NOT compared");
        _output.WriteLine(
            "  with any published sensitivity figure: those are about decodes, error correction");
        _output.WriteLine("  stands between a found signal and a decoded one, and nothing here decodes.");

        // The only assertion is that the sweep is a sweep: it has to get harder as the ratio falls,
        // or the measurement is not measuring what it says.
        Assert.True(points.Count == ratios.Length);
        Assert.True(
            points[0].MeanTrueScore > points[^1].MeanTrueScore,
            "the score at a true signal should fall as the noise rises; if it does not, this sweep "
            + "is not delivering the noise it says it is.");
    }

    private static int BestScoreOverBins(Ft8Waterfall waterfall, double baseFrequencyHz)
    {
        var geometry = waterfall.Geometry;
        if (!geometry.TryBinFor(baseFrequencyHz, out var bin, out var freqSub)
            || bin + Ft8SyncSearch.ToneCount > geometry.BinCount)
        {
            return int.MinValue;
        }

        var best = int.MinValue;
        for (var block = Ft8SyncSearch.DefaultFirstBlockOffset;
             block <= Ft8SyncSearch.DefaultLastBlockOffset;
             block++)
        {
            for (var timeSub = 0; timeSub < geometry.TimeOversampling; timeSub++)
            {
                best = Math.Max(best, Ft8SyncSearch.ScoreAt(waterfall, block, timeSub, bin, freqSub));
            }
        }

        return best;
    }
}
