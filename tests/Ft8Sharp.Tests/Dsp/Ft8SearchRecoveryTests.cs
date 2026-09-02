using Ft8Sharp.Dsp;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// Step 4's first exit criterion, measured: <b>a synthesized signal at a known offset and time is
/// found.</b> Known to the test. <b>Not known to the search.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>THE SEARCH WAS GIVEN THE SAMPLES AND THE GEOMETRY AND NOTHING ELSE.</b> That sentence is the
/// whole difference between this file and unit 213's. Every measurement here places a transmission
/// at a frequency and a sample offset this test chose, hands over the slot, and then asks the
/// answer whether it landed on it. The frequency and the offset appear on the assertion side of
/// every one of these methods and in no call. <see cref="Ft8SyncSearch.Find(Ft8Waterfall)"/> has no
/// parameter that could carry either, and <c>ToneRecovery.AlignmentFor</c> — the helper that
/// computes the truth from a known offset — is not used in this file at all.
/// </para>
/// <para>
/// <b>Numbers before bounds, everywhere.</b> Every error distribution is printed before any
/// tolerance is asserted, and the tolerances below were written after reading the printed numbers.
/// This project has caught itself setting a threshold first three times.
/// </para>
/// <para>
/// <b>Matching is not asserting.</b> A candidate is <em>associated</em> with a truth when it falls
/// inside a deliberately wide window — <see cref="AssociationHz"/> and
/// <see cref="AssociationSeconds"/> — which exists only to decide which candidate a measurement is
/// about. What that candidate's error then has to be is the tolerance, and it is far tighter.
/// </para>
/// <para>
/// <b>What none of this shows.</b> Not that a real station off a real antenna is found, and not
/// that anything decodes. Nothing here demodulates: a candidate is a place, not a message.
/// </para>
/// </remarks>
public class Ft8SearchRecoveryTests
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    /// <summary>
    /// The frequency window inside which a candidate is taken to be about a given transmission. Two
    /// tone spacings — wide enough that a real miss is measured rather than dropped.
    /// </summary>
    private const double AssociationHz = 12.5;

    /// <summary>The time window for the same purpose. Half a second, or a little over three blocks.</summary>
    private const double AssociationSeconds = 0.5;

    private readonly ITestOutputHelper _output;

    public Ft8SearchRecoveryTests(ITestOutputHelper output) => _output = output;

    /// <summary>What the search said about one transmission the test had placed.</summary>
    private sealed record Outcome(
        SearchFixture.Truth Truth,
        bool Found,
        int Rank,
        int Score,
        double FrequencyErrorHz,
        double TimeErrorSeconds,
        int TimeErrorSamples,
        int ScoreAtNothing,
        int TopScore);

    /// <summary>
    /// Places one transmission, searches the slot, and associates the answer with the truth
    /// afterwards. <b>The truth is used twice: to build the slot, and to read the result. Never in
    /// between.</b>
    /// </summary>
    private static Outcome Measure(
        Ft8SyncSearch search,
        EncodeCorpus.Entry entry,
        double baseFrequencyHz,
        int offsetSamples,
        GaussianNoise? noise = null,
        double noiseRootMeanSquare = 0,
        Action<double>? reportDeliveredNoisePower = null)
    {
        var (slot, truth) = SearchFixture.OneSignal(Rate, entry, baseFrequencyHz, offsetSamples);

        var audio = slot;
        if (noise is not null)
        {
            audio = SearchFixture.AddNoise(slot, noise, noiseRootMeanSquare, out var delivered);
            reportDeliveredNoisePower?.Invoke(delivered);
        }

        // ---- everything above is the test's. Everything below is the search's. ----
        var waterfall = new Ft8Monitor().Analyse(audio);
        var found = search.Find(waterfall);
        // ---- the search has answered. The truth may be looked at again now. ----

        var geometry = waterfall.Geometry;
        var truthTime = truth.TimeSeconds(Rate);

        var rank = -1;
        var best = default(Ft8Candidate);
        for (var i = 0; i < found.Count; i++)
        {
            var frequencyError = found[i].FrequencyHz(geometry) - baseFrequencyHz;
            var timeError = found[i].TimeSeconds(geometry) - truthTime;

            if (Math.Abs(frequencyError) <= AssociationHz
                && Math.Abs(timeError) <= AssociationSeconds)
            {
                rank = i + 1;
                best = found[i];
                break;
            }
        }

        var topScore = found.Count > 0 ? found[0].Score : int.MinValue;

        if (rank < 0)
        {
            // Not found. What WAS the score at the truth's own position? The block and sub-offset
            // that position corresponds to is not known to this test either - so the honest answer
            // is the best score over the bins the transmission actually occupies, at any time.
            return new Outcome(
                truth, false, 0, 0, double.NaN, double.NaN, 0, ScoreAtTruthBins(waterfall, baseFrequencyHz),
                topScore);
        }

        var candidateTime = best.TimeSeconds(geometry);
        return new Outcome(
            truth,
            true,
            rank,
            best.Score,
            best.FrequencyHz(geometry) - baseFrequencyHz,
            candidateTime - truthTime,
            (int)Math.Round((candidateTime - truthTime) * Rate),
            0,
            topScore);
    }

    /// <summary>
    /// The strongest score anywhere in the block sweep at the bin the transmission's lowest tone
    /// actually occupies. Used only to say what was there when nothing was found.
    /// </summary>
    private static int ScoreAtTruthBins(Ft8Waterfall waterfall, double baseFrequencyHz)
    {
        var geometry = waterfall.Geometry;
        if (!geometry.TryBinFor(baseFrequencyHz, out var bin, out var freqSub))
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

    /// <summary>The bin spacing of the waterfall, in hertz. 3.125 Hz at 12 kHz.</summary>
    private static double BinHz => new Ft8WaterfallGeometry().TransformBinSpacingHz;

    /// <summary>
    /// The frequency tolerance, <b>written after the distribution was printed and not before it.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Half a bin is the floor and it is arithmetic, not quality.</b> The search reports the
    /// centre of a waterfall bin, the bins are <see cref="BinHz"/> apart, so a transmission placed
    /// anywhere between two of them is reported at whichever is nearer and the error can be half a
    /// bin. A signal placed <em>exactly</em> halfway is equidistant from both.
    /// </para>
    /// <para>
    /// <b>The extra thousandth of a hertz is the geometry's own single precision showing through</b>
    /// and it is measured rather than allowed for in advance: the half-bin sweep came back
    /// 1.5625224 Hz out where half a bin is 1.5625000 Hz. <c>Ft8WaterfallGeometry.FrequencyHz</c>
    /// divides by <c>0.160f</c>, which is 0.1599999964237213 and not 0.160 — unit 212's lesson,
    /// deliberately kept, because a geometry that is more accurate than upstream's disagrees with
    /// upstream's. It scales every reported frequency by about one part in 45 million, which at
    /// 1 kHz is 22 microhertz. The bound carries a thousandth of a hertz, which is forty times that
    /// and still five hundred times smaller than the quantity being bounded.
    /// </para>
    /// </remarks>
    private static double FrequencyToleranceHz => (BinHz / 2) + 0.001;

    private void ReportDistribution(string axis, IReadOnlyList<double> errors, string unit)
    {
        if (errors.Count == 0)
        {
            _output.WriteLine($"  {axis}: nothing to measure");
            return;
        }

        var worst = errors.Max(Math.Abs);
        var mean = errors.Average(Math.Abs);
        var signed = errors.Average();
        _output.WriteLine(
            $"  {axis,-22} worst |e| {worst,10:F6} {unit}   mean |e| {mean,10:F6} {unit}   "
            + $"MEAN SIGNED {signed,10:F6} {unit}");
    }

    // ------------------------------------------------------------------------------------------
    // THE TARGET
    // ------------------------------------------------------------------------------------------

    /// <summary>
    /// The corpus, on a clean slot, at rotating frequencies and offsets — including a frequency
    /// exactly halfway between two bins and offsets that fall on neither the block nor the sub-block
    /// grid. <b>N of M found, and at what rank.</b>
    /// </summary>
    [Fact]
    public void EveryMessageOfTheCorpusIsFoundAtAPlaceTheSearchWasNeverTold()
    {
        var corpus = EncodeCorpus.Build();
        var search = new Ft8SyncSearch();

        // Three frequency cases, and the middle one is the hard one: unit 213 measured the recovery
        // margin falling from 13.5 dB to 4.5 dB at a frequency exactly halfway between two bins.
        var frequencies = new[]
        {
            (Label: "on a bin centre", Hz: 1000.0),
            (Label: "half a bin off", Hz: 1000.0 + (BinHz / 2)),
            (Label: "a quarter bin off", Hz: 1000.0 + (BinHz / 4)),
        };

        // Five offsets: two on the block grid, one on the sub-block grid but not the block grid, and
        // two on neither.
        var offsets = new[] { 0, 1920 * 3, 960 * 5, 5000, 12345 };

        var outcomes = new List<Outcome>();
        for (var i = 0; i < corpus.Count; i++)
        {
            var frequency = frequencies[i % frequencies.Length];
            var offset = offsets[i % offsets.Length];
            outcomes.Add(Measure(search, corpus[i], frequency.Hz, offset));
        }

        var found = outcomes.Where(o => o.Found).ToList();
        var atOne = found.Count(o => o.Rank == 1);

        _output.WriteLine(
            $"  {found.Count} OF {outcomes.Count} MESSAGES FOUND, AT RANK 1 IN {atOne} OF THEM");
        _output.WriteLine(
            $"  ranks: worst {(found.Count > 0 ? found.Max(o => o.Rank) : 0)}, "
            + $"mean {(found.Count > 0 ? found.Average(o => o.Rank) : 0):F2}");
        _output.WriteLine(
            $"  scores at the truth: worst {(found.Count > 0 ? found.Min(o => o.Score) : 0)}, "
            + $"mean {(found.Count > 0 ? found.Average(o => o.Score) : 0):F1}");
        _output.WriteLine(string.Empty);

        foreach (var missed in outcomes.Where(o => !o.Found))
        {
            _output.WriteLine(
                $"  MISSED: {missed.Truth.Label} at {missed.Truth.BaseFrequencyHz:F4} Hz, offset "
                + $"{missed.Truth.OffsetSamples} - best score over its own bins was "
                + $"{missed.ScoreAtNothing}, and the top candidate scored {missed.TopScore}");
        }

        // The distributions, printed BEFORE any tolerance is asserted.
        _output.WriteLine(string.Empty);
        ReportDistribution("frequency error", found.Select(o => o.FrequencyErrorHz).ToList(), "Hz");
        ReportDistribution("time error", found.Select(o => o.TimeErrorSeconds).ToList(), "s");
        ReportDistribution(
            "time error", found.Select(o => (double)o.TimeErrorSamples).ToList(), "samples");

        var meanSignedTime = found.Count > 0 ? found.Average(o => o.TimeErrorSeconds) : 0;
        var geometry = new Ft8WaterfallGeometry();
        _output.WriteLine(string.Empty);
        _output.WriteLine(
            $"  THE MEAN SIGNED TIME ERROR IS {meanSignedTime:F6} s, which is "
            + $"{meanSignedTime / Ft8WaterfallGeometry.SymbolPeriodSeconds:F3} BLOCKS.");
        _output.WriteLine(
            "  That is this library's block-to-sample alignment showing itself, and it is a constant");
        _output.WriteLine(
            "  rather than a spread: a candidate's block offset b names a transmission that started");
        _output.WriteLine(
            $"  at about (b - 1) x {geometry.BlockSize} samples. Reported, not chased - unit 213 left");
        _output.WriteLine("  it unsettled and step 5 is what will use it.");

        // ---- and only now, the bounds, written from the numbers above ----
        Assert.Equal(outcomes.Count, found.Count);
        Assert.All(found, o => Assert.True(
            Math.Abs(o.FrequencyErrorHz) <= FrequencyToleranceHz,
            $"{o.Truth.Label} was {o.FrequencyErrorHz:F7} Hz out, more than half a bin."));

        var residuals = found.Select(o => o.TimeErrorSeconds - meanSignedTime).ToList();
        ReportDistribution("time residual", residuals, "s");
        Assert.All(residuals, r => Assert.True(
            Math.Abs(r) <= Ft8WaterfallGeometry.SymbolPeriodSeconds / 2,
            $"a time error sat {r:F6} s from the mean, more than half a block."));

        _output.WriteLine(string.Empty);
        _output.WriteLine("  TOLERANCES ASSERTED, AFTER THE NUMBERS THEY WERE SET FROM:");
        _output.WriteLine(
            $"    frequency: within half a bin, {BinHz / 2:F7} Hz, plus 0.001 Hz for the geometry's");
        _output.WriteLine(
            $"               own single-precision symbol period = {FrequencyToleranceHz:F7} Hz.");
        _output.WriteLine(
            "               The worst measured was "
            + $"{(found.Count > 0 ? found.Max(o => Math.Abs(o.FrequencyErrorHz)) : 0):F7} Hz, at the");
        _output.WriteLine("               half-bin frequency, where half a bin is the arithmetic floor.");
        _output.WriteLine(
            "    time:      within half a block of the constant bias, "
            + $"{Ft8WaterfallGeometry.SymbolPeriodSeconds / 2:F3} s");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  THE SEARCH WAS GIVEN THE SAMPLES AND THE GEOMETRY AND NOTHING ELSE.");
    }

    /// <summary>
    /// The frequency sweep on its own, across the passband and at every fraction of a bin, so that
    /// the half-bin case cannot hide inside an average.
    /// </summary>
    [Fact]
    public void TheHardFrequenciesAreSweptIncludingExactlyHalfwayBetweenTwoBins()
    {
        var entry = EncodeCorpus.Build()[0];
        var search = new Ft8SyncSearch();

        var bases = new[] { 300.0, 700.0, 1000.0, 1500.0, 2000.0, 2500.0 };
        var fractions = new[]
        {
            (Label: "on centre  ", Offset: 0.0),
            (Label: "quarter bin", Offset: BinHz / 4),
            (Label: "half bin   ", Offset: BinHz / 2),
            (Label: "three quarters", Offset: BinHz * 3 / 4),
        };

        var outcomes = new List<Outcome>();
        foreach (var fraction in fractions)
        {
            var perFraction = new List<Outcome>();
            foreach (var basis in bases)
            {
                perFraction.Add(Measure(search, entry, basis + fraction.Offset, 1920));
            }

            outcomes.AddRange(perFraction);
            _output.WriteLine(
                $"  {fraction.Label} (+{fraction.Offset:F5} Hz): "
                + $"{perFraction.Count(o => o.Found)} of {perFraction.Count} found, "
                + $"rank 1 in {perFraction.Count(o => o.Rank == 1)}, "
                + $"worst score {(perFraction.Any(o => o.Found) ? perFraction.Where(o => o.Found).Min(o => o.Score) : 0)}, "
                + $"worst |Δf| {(perFraction.Any(o => o.Found) ? perFraction.Where(o => o.Found).Max(o => Math.Abs(o.FrequencyErrorHz)) : double.NaN):F4} Hz");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine(
            $"  {outcomes.Count(o => o.Found)} of {outcomes.Count} across the whole sweep");
        ReportDistribution(
            "frequency error", outcomes.Where(o => o.Found).Select(o => o.FrequencyErrorHz).ToList(), "Hz");

        Assert.Equal(outcomes.Count, outcomes.Count(o => o.Found));
        Assert.All(
            outcomes,
            o => Assert.True(
                Math.Abs(o.FrequencyErrorHz) <= FrequencyToleranceHz,
                $"{o.FrequencyErrorHz:F7} Hz out"));
    }

    /// <summary>
    /// The time sweep on its own, including offsets on neither the block nor the sub-block grid, and
    /// one that puts the transmission's start after the slot has already begun by a fraction of a
    /// symbol.
    /// </summary>
    [Fact]
    public void TheTimeOffsetsSweptIncludeOnesOffBothTheBlockAndTheSubBlockGrid()
    {
        var entry = EncodeCorpus.Build()[0];
        var search = new Ft8SyncSearch();
        var geometry = new Ft8WaterfallGeometry();

        var offsets = new[]
        {
            (Label: "zero, on the block grid", Samples: 0),
            (Label: "3 whole blocks", Samples: geometry.BlockSize * 3),
            (Label: "5 sub-blocks, off the block grid", Samples: geometry.SubblockSize * 5),
            (Label: "off both grids by 40 samples", Samples: (geometry.BlockSize * 2) + 40),
            (Label: "off both grids, 5000", Samples: 5000),
            (Label: "off both grids, 12345", Samples: 12345),
            (Label: "off both grids, 27913", Samples: 27913),
        };

        var outcomes = new List<Outcome>();
        foreach (var (label, samples) in offsets)
        {
            var outcome = Measure(search, entry, 1000.0, samples);
            outcomes.Add(outcome);
            _output.WriteLine(
                $"  {label,-34} offset {samples,6}: "
                + (outcome.Found
                    ? $"found at rank {outcome.Rank}, score {outcome.Score}, "
                      + $"Δt {outcome.TimeErrorSeconds,9:F6} s = {outcome.TimeErrorSamples,6} samples"
                    : $"NOT FOUND, best score over its bins {outcome.ScoreAtNothing}"));
        }

        ReportDistribution(
            "time error", outcomes.Where(o => o.Found).Select(o => o.TimeErrorSeconds).ToList(), "s");

        Assert.Equal(outcomes.Count, outcomes.Count(o => o.Found));
    }

    // ------------------------------------------------------------------------------------------
    // IN NOISE, BECAUSE THE STEP IS CALLED SIGNALS ARE FOUND IN NOISE
    // ------------------------------------------------------------------------------------------

    /// <summary>
    /// The same corpus at a stated signal-to-noise ratio, with the ratio actually delivered measured
    /// rather than assumed.
    /// </summary>
    [Fact]
    public void TheCorpusIsFoundInNoiseAtAMeasuredSignalToNoiseRatio()
    {
        const double RequestedSnr = -10.0;

        var corpus = EncodeCorpus.Build();
        var search = new Ft8SyncSearch();
        var noise = new GaussianNoise(seed: 214_004);

        var frequencies = new[] { 1000.0, 1000.0 + (BinHz / 2), 1000.0 + (BinHz / 4) };
        var offsets = new[] { 0, 1920 * 3, 960 * 5, 5000, 12345 };

        var outcomes = new List<Outcome>();
        var delivered = new List<double>();

        for (var i = 0; i < corpus.Count; i++)
        {
            var frequency = frequencies[i % frequencies.Length];
            var signalPower = SearchFixture.TransmissionPower(Rate, corpus[i], frequency);
            var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, RequestedSnr, Rate);

            outcomes.Add(Measure(
                search,
                corpus[i],
                frequency,
                offsets[i % offsets.Length],
                noise,
                sigma,
                noisePower => delivered.Add(
                    SignalToNoise.DecibelsFor(signalPower, noisePower, Rate))));
        }

        var found = outcomes.Count(o => o.Found);
        _output.WriteLine($"  requested {RequestedSnr:F1} dB in a 2500 Hz reference bandwidth");
        _output.WriteLine(
            $"  DELIVERED {delivered.Average():F3} dB (worst {delivered.Min():F3}, best {delivered.Max():F3})");
        _output.WriteLine(
            $"  {found} OF {outcomes.Count} FOUND, at rank 1 in {outcomes.Count(o => o.Rank == 1)}");
        _output.WriteLine(
            $"  scores at the truth: worst {outcomes.Where(o => o.Found).DefaultIfEmpty().Min(o => o?.Score ?? 0)}, "
            + $"mean {outcomes.Where(o => o.Found).Average(o => o.Score):F1}");

        ReportDistribution(
            "frequency error", outcomes.Where(o => o.Found).Select(o => o.FrequencyErrorHz).ToList(), "Hz");
        ReportDistribution(
            "time error", outcomes.Where(o => o.Found).Select(o => o.TimeErrorSeconds).ToList(), "s");

        foreach (var missed in outcomes.Where(o => !o.Found))
        {
            _output.WriteLine(
                $"  MISSED: {missed.Truth.Label} at {missed.Truth.BaseFrequencyHz:F4} Hz - best score "
                + $"over its own bins {missed.ScoreAtNothing}, top candidate {missed.TopScore}");
        }

        // The bound is written from the number above and from nowhere else.
        Assert.Equal(outcomes.Count, found);
    }

    /// <summary>
    /// The half that makes the other half mean anything: <b>over noise alone, with no signal in the
    /// slot at all.</b>
    /// </summary>
    [Fact]
    public void OverNoiseAloneTheSearchDoesNotManufactureASignal()
    {
        var entry = EncodeCorpus.Build()[0];
        var search = new Ft8SyncSearch();
        var signalPower = SearchFixture.TransmissionPower(Rate, entry, 1000.0);
        var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, -10.0, Rate);

        const int Slots = 20;
        var topScores = new List<int>();
        var counts = new List<int>();
        var noise = new GaussianNoise(seed: 214_005);

        for (var slot = 0; slot < Slots; slot++)
        {
            var empty = SearchFixture.EmptySlot(Rate);
            var audio = SearchFixture.AddNoise(empty, noise, sigma, out _);
            var found = search.Find(new Ft8Monitor().Analyse(audio));

            counts.Add(found.Count);
            topScores.Add(found.Count > 0 ? found[0].Score : int.MinValue);
        }

        // And the same measurement with a signal in it, so the two numbers sit side by side.
        var withSignal = Measure(search, entry, 1000.0, 0, new GaussianNoise(214_006), sigma);

        _output.WriteLine($"  {Slots} slots of NOISE ALONE at about -10 dB, no signal anywhere in them:");
        _output.WriteLine(
            $"    candidates returned: worst {counts.Max()}, mean {counts.Average():F1}, best {counts.Min()}");
        _output.WriteLine(
            $"    top score over noise alone: worst {topScores.Max()}, mean "
            + $"{topScores.Where(s => s != int.MinValue).DefaultIfEmpty(0).Average():F1}");
        _output.WriteLine(string.Empty);
        _output.WriteLine(
            $"    THE SCORE AT A TRUE SIGNAL'S POSITION, same noise level: {withSignal.Score}");
        _output.WriteLine(
            $"    THE HIGHEST SCORE NOISE ALONE EVER PRODUCED IN {Slots} SLOTS: {topScores.Max()}");
        _output.WriteLine(
            $"    a number beside a number: {withSignal.Score} against {topScores.Max()}, a margin of "
            + $"{withSignal.Score - topScores.Max()}");
        _output.WriteLine(string.Empty);
        _output.WriteLine(
            $"    FALSE ALARMS AT THE TRUE SIGNAL'S OWN STRENGTH: "
            + $"{topScores.Count(s => s >= withSignal.Score)} of {Slots} noise-only slots produced any "
            + "candidate scoring as high as the real one did.");

        Assert.True(withSignal.Found, "the signal at -10 dB should be found for this to mean anything.");
        Assert.True(
            topScores.Max() < withSignal.Score,
            "noise alone outscored a real signal, which would make every other number here worthless.");
    }

    // ------------------------------------------------------------------------------------------
    // WATCHED MOVING
    // ------------------------------------------------------------------------------------------

    /// <summary>A signal at a different frequency is found at a different frequency, measured.</summary>
    [Fact]
    public void ASignalAtADifferentFrequencyIsFoundAtADifferentFrequency()
    {
        var entry = EncodeCorpus.Build()[0];
        var search = new Ft8SyncSearch();

        var pairs = new[] { (500.0, 2500.0), (1000.0, 1006.25), (1000.0, 1000.0 + BinHz) };

        foreach (var (from, to) in pairs)
        {
            var low = Measure(search, entry, from, 1920);
            var high = Measure(search, entry, to, 1920);

            Assert.True(low.Found && high.Found);

            var geometry = new Ft8WaterfallGeometry();
            var lowHz = from + low.FrequencyErrorHz;
            var highHz = to + high.FrequencyErrorHz;
            var predicted = to - from;
            var measured = highHz - lowHz;

            _output.WriteLine(
                $"  {from:F4} Hz -> {lowHz:F4} Hz reported;  {to:F4} Hz -> {highHz:F4} Hz reported");
            _output.WriteLine(
                $"    displacement predicted {predicted:F4} Hz, measured {measured:F4} Hz, "
                + $"difference {measured - predicted:F4} Hz - under one bin of {geometry.TransformBinSpacingHz:F4} Hz");

            Assert.True(Math.Abs(measured - predicted) < geometry.TransformBinSpacingHz);
        }
    }

    /// <summary>
    /// A signal shifted in time is found at a shifted time, and the shift matches the arithmetic.
    /// </summary>
    [Fact]
    public void ASignalShiftedInTimeIsFoundAtAShiftedTime()
    {
        var entry = EncodeCorpus.Build()[0];
        var search = new Ft8SyncSearch();
        var geometry = new Ft8WaterfallGeometry();

        var shifts = new[]
        {
            (From: 0, To: geometry.BlockSize),
            (From: 0, To: geometry.BlockSize * 5),
            (From: 960, To: 960 + (geometry.SubblockSize * 3)),
        };

        foreach (var (from, to) in shifts)
        {
            var earlier = Measure(search, entry, 1000.0, from);
            var later = Measure(search, entry, 1000.0, to);
            Assert.True(earlier.Found && later.Found);

            var predictedSeconds = (double)(to - from) / Rate;
            var measuredSeconds =
                (later.TimeErrorSeconds + ((double)to / Rate))
                - (earlier.TimeErrorSeconds + ((double)from / Rate));

            _output.WriteLine(
                $"  offset {from,6} -> {to,6} samples: displacement predicted "
                + $"{predictedSeconds:F6} s ({to - from} samples), measured {measuredSeconds:F6} s "
                + $"({(int)Math.Round(measuredSeconds * Rate)} samples), "
                + $"difference {(measuredSeconds - predictedSeconds) * Rate:F1} samples");

            Assert.True(
                Math.Abs(measuredSeconds - predictedSeconds)
                <= Ft8WaterfallGeometry.SymbolPeriodSeconds / 2,
                "the measured displacement is more than half a block from the arithmetic.");
        }
    }
}
