using Ft8Sharp.Dsp;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// Step 4's second exit criterion, measured: <b>twenty simultaneous synthesized signals across the
/// passband are found.</b> One slot, twenty transmissions, twenty frequencies, several start times,
/// noise over the lot, and <b>one search</b>.
/// </summary>
/// <remarks>
/// <para>
/// <b>THE SEARCH WAS GIVEN THE SAMPLES AND THE GEOMETRY AND NOTHING ELSE</b>, here as in
/// <see cref="Ft8SearchRecoveryTests"/>. It is not told that there are twenty, nor where any of them
/// is, nor that any of them exists.
/// </para>
/// <para>
/// <b>What <i>found</i> means, and it is defined rather than assumed.</b> For each of the twenty
/// truths there is a candidate <em>somewhere in the returned list</em> within the tolerance task 4
/// established on both axes. <b>Not: the top twenty candidates are the twenty signals.</b>
/// Near-duplicates of one strong signal are expected in a candidate list and are not a defect — a
/// transmission is a couple of bins wide and half a block long in this geometry, so several
/// neighbouring hypotheses score well on the same energy. What matters, and what is reported, is
/// <b>how deep the list has to be read to cover all twenty</b>, because that depth is what step 5
/// will pay for in decode attempts.
/// </para>
/// </remarks>
public class Ft8SearchPassbandTests
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;
    private const int Signals = 20;

    private readonly ITestOutputHelper _output;

    public Ft8SearchPassbandTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// The frequency tolerance task 4 established: half a bin, plus a thousandth of a hertz for the
    /// geometry's own single-precision symbol period.
    /// </summary>
    private static double FrequencyToleranceHz =>
        (new Ft8WaterfallGeometry().TransformBinSpacingHz / 2) + 0.001;

    /// <summary>
    /// The time tolerance task 4 established: half a block either side of the constant one-block
    /// bias that measurement found and named.
    /// </summary>
    private const double TimeBiasSeconds = Ft8WaterfallGeometry.SymbolPeriodSeconds;

    private const double TimeToleranceSeconds = Ft8WaterfallGeometry.SymbolPeriodSeconds / 2;

    /// <summary>Builds the twenty-signal slot. Shared with the stability measurement.</summary>
    internal static (float[] Slot, IReadOnlyList<SearchFixture.Truth> Truths) BuildPassbandSlot()
    {
        var geometry = new Ft8WaterfallGeometry();
        return SearchFixture.ManySignals(
            Rate,
            EncodeCorpus.Build(),
            Signals,
            lowestHz: 300.0,
            spacingHz: 130.0,
            binHz: geometry.TransformBinSpacingHz);
    }

    /// <summary>
    /// The same slot with noise over it, at a per-transmission ratio measured rather than assumed.
    /// </summary>
    internal static (float[] Audio, IReadOnlyList<SearchFixture.Truth> Truths, double DeliveredSnr)
        BuildNoisyPassbandSlot(double requestedSnr, int seed)
    {
        var (slot, truths) = BuildPassbandSlot();
        var corpus = EncodeCorpus.Build();

        // The ratio is quoted PER TRANSMISSION, against the power of one of them alone. Quoting it
        // against the whole slot would flatter it by ten decibels for no reason but the arithmetic.
        var onePower = SearchFixture.TransmissionPower(Rate, corpus[0], truths[0].BaseFrequencyHz);
        var sigma = SignalToNoise.NoiseAmplitudeFor(onePower, requestedSnr, Rate);

        var audio = SearchFixture.AddNoise(slot, new GaussianNoise(seed), sigma, out var noisePower);
        return (audio, truths, SignalToNoise.DecibelsFor(onePower, noisePower, Rate));
    }

    /// <summary>How the returned list covered one truth.</summary>
    private sealed record Cover(
        SearchFixture.Truth Truth,
        bool Found,
        int Rank,
        int Score,
        double FrequencyErrorHz,
        double TimeErrorSeconds,
        int BestScoreOverItsOwnBins);

    private static IReadOnlyList<Cover> CoverageOf(
        IReadOnlyList<Ft8Candidate> found,
        Ft8Waterfall waterfall,
        IReadOnlyList<SearchFixture.Truth> truths,
        out int depthNeeded,
        out int duplicates,
        out bool[] isDuplicate)
    {
        var geometry = waterfall.Geometry;
        var covers = new List<Cover>();
        var covered = new bool[truths.Count];
        isDuplicate = new bool[found.Count];

        for (var t = 0; t < truths.Count; t++)
        {
            var truth = truths[t];
            var truthTime = truth.TimeSeconds(Rate);

            var rank = -1;
            var candidate = default(Ft8Candidate);
            for (var i = 0; i < found.Count; i++)
            {
                var frequencyError = found[i].FrequencyHz(geometry) - truth.BaseFrequencyHz;
                var timeError = found[i].TimeSeconds(geometry) - truthTime - TimeBiasSeconds;

                if (Math.Abs(frequencyError) <= FrequencyToleranceHz
                    && Math.Abs(timeError) <= TimeToleranceSeconds)
                {
                    rank = i + 1;
                    candidate = found[i];
                    break;
                }
            }

            if (rank < 0)
            {
                covers.Add(new Cover(
                    truth, false, 0, 0, double.NaN, double.NaN,
                    BestScoreOverBins(waterfall, truth.BaseFrequencyHz)));
                continue;
            }

            covered[t] = true;
            covers.Add(new Cover(
                truth,
                true,
                rank,
                candidate.Score,
                candidate.FrequencyHz(geometry) - truth.BaseFrequencyHz,
                candidate.TimeSeconds(geometry) - truthTime,
                0));
        }

        depthNeeded = covers.Where(c => c.Found).Select(c => c.Rank).DefaultIfEmpty(0).Max();

        // A duplicate is a candidate that lands on a truth some better-ranked candidate already
        // covered. Expected, and counted rather than complained about.
        var claimed = new bool[truths.Count];
        duplicates = 0;
        for (var i = 0; i < found.Count; i++)
        {
            for (var t = 0; t < truths.Count; t++)
            {
                var truthTime = truths[t].TimeSeconds(Rate);
                var frequencyError = found[i].FrequencyHz(geometry) - truths[t].BaseFrequencyHz;
                var timeError = found[i].TimeSeconds(geometry) - truthTime - TimeBiasSeconds;

                if (Math.Abs(frequencyError) > FrequencyToleranceHz
                    || Math.Abs(timeError) > TimeToleranceSeconds)
                {
                    continue;
                }

                if (claimed[t])
                {
                    duplicates++;
                    isDuplicate[i] = true;
                }

                claimed[t] = true;
                break;
            }
        }

        return covers;
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

    private void Report(
        string what,
        IReadOnlyList<Cover> covers,
        IReadOnlyList<Ft8Candidate> found,
        int depth,
        int duplicates)
    {
        var hits = covers.Where(c => c.Found).ToList();

        _output.WriteLine($"  {hits.Count} OF {Signals} FOUND — {what}");
        _output.WriteLine(
            $"  the list holds {found.Count} candidates; {depth} DEEP TO COVER ALL THAT WERE FOUND; "
            + $"{duplicates} of them are duplicates of a signal already covered");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  {"Hz",10} {"offset",7} {"rank",5} {"score",6} {"dF Hz",10} {"dt s",10}");

        foreach (var cover in covers)
        {
            if (cover.Found)
            {
                _output.WriteLine(
                    $"  {cover.Truth.BaseFrequencyHz,10:F4} {cover.Truth.OffsetSamples,7} "
                    + $"{cover.Rank,5} {cover.Score,6} {cover.FrequencyErrorHz,10:F5} "
                    + $"{cover.TimeErrorSeconds,10:F5}");
            }
            else
            {
                _output.WriteLine(
                    $"  {cover.Truth.BaseFrequencyHz,10:F4} {cover.Truth.OffsetSamples,7}   MISSED — "
                    + $"the best score anywhere over its own bins was {cover.BestScoreOverItsOwnBins}, "
                    + $"and the top of the list scored {(found.Count > 0 ? found[0].Score : 0)}");
            }
        }

        if (hits.Count > 0)
        {
            _output.WriteLine(string.Empty);
            _output.WriteLine(
                $"  ranks: worst {hits.Max(c => c.Rank)}, mean {hits.Average(c => c.Rank):F1}; "
                + $"scores: worst {hits.Min(c => c.Score)}, mean {hits.Average(c => c.Score):F1}");
            _output.WriteLine(
                $"  worst |dF| {hits.Max(c => Math.Abs(c.FrequencyErrorHz)):F5} Hz, "
                + $"worst |dt - bias| "
                + $"{hits.Max(c => Math.Abs(c.TimeErrorSeconds - TimeBiasSeconds)):F5} s");
        }
    }

    /// <summary>
    /// How close together the twenty are, printed before the result, because two signals near enough
    /// in frequency to be confused would be the most useful thing this measurement could find.
    /// </summary>
    private void ReportSeparations(IReadOnlyList<SearchFixture.Truth> truths)
    {
        var sorted = truths.Select(t => t.BaseFrequencyHz).OrderBy(hz => hz).ToList();
        var gaps = new List<double>();
        for (var i = 1; i < sorted.Count; i++)
        {
            gaps.Add(sorted[i] - sorted[i - 1]);
        }

        var geometry = new Ft8WaterfallGeometry();
        _output.WriteLine(
            $"  twenty transmissions from {sorted[0]:F4} Hz to {sorted[^1]:F4} Hz across a "
            + $"{geometry.MinFrequencyHz:F0}..{geometry.MaxFrequencyHz:F0} Hz passband");
        _output.WriteLine(
            $"  closest pair {gaps.Min():F4} Hz apart, which is {gaps.Min() / geometry.ToneSpacingHz:F1} "
            + $"tone spacings and {gaps.Min() / geometry.TransformBinSpacingHz:F1} bins - "
            + (gaps.Min() > geometry.ToneSpacingHz * 8
                ? "no two of them overlap in frequency at all."
                : "SOME OF THEM OVERLAP, and that is stated rather than hidden."));
        _output.WriteLine(string.Empty);
    }

    // ------------------------------------------------------------------------------------------

    /// <summary>Twenty at once on a clean slot: the criterion without the noise.</summary>
    [Fact]
    public void TwentySimultaneousSignalsAcrossThePassbandAreFoundOnACleanSlot()
    {
        var (slot, truths) = BuildPassbandSlot();
        ReportSeparations(truths);

        var waterfall = new Ft8Monitor().Analyse(slot);
        var found = new Ft8SyncSearch().Find(waterfall);

        var covers = CoverageOf(found, waterfall, truths, out var depth, out var duplicates, out _);
        Report("clean slot, no noise", covers, found, depth, duplicates);

        Assert.Equal(Signals, covers.Count(c => c.Found));
    }

    /// <summary>
    /// Twenty at once with noise over the lot, which is the criterion as the step means it.
    /// </summary>
    [Fact]
    public void TwentySimultaneousSignalsAreFoundWithNoiseOverTheWholePassband()
    {
        const double Requested = -10.0;
        var (audio, truths, delivered) = BuildNoisyPassbandSlot(Requested, seed: 214_020);

        ReportSeparations(truths);
        _output.WriteLine(
            $"  noise requested at {Requested:F1} dB per transmission in a 2500 Hz reference "
            + $"bandwidth; DELIVERED {delivered:F3} dB");
        _output.WriteLine(string.Empty);

        var waterfall = new Ft8Monitor().Analyse(audio);
        var found = new Ft8SyncSearch().Find(waterfall);

        var covers = CoverageOf(found, waterfall, truths, out var depth, out var duplicates, out _);
        Report($"twenty at once, {delivered:F3} dB delivered", covers, found, depth, duplicates);

        Assert.Equal(Signals, covers.Count(c => c.Found));
    }

    /// <summary>
    /// What the default candidate limit costs. The limit is upstream's application's number, and a
    /// list too short to hold twenty signals plus their near-duplicates would lose signals that were
    /// found — so the depth needed is measured against the limit rather than assumed to fit inside
    /// it.
    /// </summary>
    [Fact]
    public void TheDepthNeededToCoverTwentyIsMeasuredAgainstTheDefaultCandidateLimit()
    {
        var (audio, truths, delivered) = BuildNoisyPassbandSlot(-10.0, seed: 214_021);
        var waterfall = new Ft8Monitor().Analyse(audio);

        _output.WriteLine($"  delivered {delivered:F3} dB per transmission");
        _output.WriteLine($"  {"limit",8} {"returned",9} {"covered",8} {"depth",7} {"duplicates",11}");

        foreach (var limit in new[] { 20, 40, 80, Ft8SyncSearch.DefaultCandidateLimit, 400 })
        {
            var found = new Ft8SyncSearch(candidateLimit: limit).Find(waterfall);
            var covers = CoverageOf(found, waterfall, truths, out var depth, out var duplicates, out _);
            _output.WriteLine(
                $"  {limit,8} {found.Count,9} {covers.Count(c => c.Found),8} {depth,7} {duplicates,11}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine(
            "  The depth is what step 5 pays for: every candidate above it is a decode attempt that");
        _output.WriteLine(
            "  has to be made before the last of the twenty is reached. Reported, not tuned.");
    }
}
