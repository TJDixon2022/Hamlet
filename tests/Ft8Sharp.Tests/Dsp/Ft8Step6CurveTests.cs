using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>STEP 6'S CURVE, AND IT CLAIMS STEP 6'S CRITERIA.</b> The decode rate against signal-to-noise
/// ratio, on signals this library synthesized itself, at one decibel of resolution through the
/// collapse — with the published figure, the trial counts and the verdict band fixed in
/// <see cref="Ft8Step6Ladder"/> and committed before this file was executed once.
/// </summary>
/// <remarks>
/// <para>
/// <b>How this differs from <see cref="Ft8SensitivityLadderTests"/>, which says in its own header
/// that it is not step 6.</b> That is unit 218's diagnostic: ten rungs at two decibels, 26 messages,
/// two seeds, 52 trials a rung, and no verdict of any kind. This is the measurement: fourteen rungs
/// with nine of them one decibel apart, the whole 56-message corpus, six noise draws through the
/// collapse and three on the anchors, <b>336 trials on every rung the verdict is read from</b>, a
/// Wilson interval on every rate, and a verdict band written down before the first trial ran.
/// </para>
/// <para>
/// <b>Expect the number to fall short.</b> Unit 218's diagnostic reads 2 of 52 at -21 dB. On that
/// table this receiver is somewhere between two and three decibels short of the published figure.
/// <c>PHASE_PLAN.md</c> has already ruled on what that means: <i>if the number falls short, the step
/// has done its job. Failing here is the step working.</i> <b>Nothing in the library was changed to
/// move it.</b> No threshold moved, no iteration bound rose, no candidate limit widened.
/// </para>
/// <para>
/// <b>Binned by the delivered ratio and never by the requested one</b>, which is unit 218's rule and
/// stays. The worst requested-versus-delivered error over the whole run is printed before any bound
/// is asserted.
/// </para>
/// </remarks>
public class Ft8Step6CurveTests
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    /// <summary>Exactly on a bin centre: 1000 / 6.25 is 160, a whole number of tone spacings.</summary>
    private const double OnGridHz = 1000.0;

    private readonly ITestOutputHelper _output;

    public Ft8Step6CurveTests(ITestOutputHelper output) => _output = output;

    /// <summary>A whole number of symbol periods in, which is also a whole number of half-symbol blocks.</summary>
    private static int AlignedOffset => Ft8Waveform.SamplesPerSymbol(Rate) * 3;

    /// <summary>
    /// <b>Task 2e — the population is proved before the curve stands on it.</b> Every message in the
    /// population offered once at a ratio far above the collapse, so that a rung short of 100 per cent
    /// at the top of the ladder can be read as the receiver rather than as a message the path was
    /// never going to return.
    /// </summary>
    /// <remarks>
    /// <b>Why this test exists at all.</b> Unit 218's ladder dropped every corpus entry carrying a
    /// hashed callsign, and left no record of whether that was necessary or merely cautious. Task 2e
    /// requires at least one hashed-callsign message and one free-text message in the population
    /// unless the ladder's construction forbids it. <b>This measures whether it does</b>, rather than
    /// inheriting the exclusion — and the answer decides the population before the curve runs.
    /// </remarks>
    [Fact]
    public void EveryMessageInThePopulationIsProvedToComeBackBeforeTheCurveStandsOnIt()
    {
        const double strong = -10.0;
        var offered = EncodeCorpus.Build();
        var population = Ft8Step6Ladder.Population();
        var unscoreable = Ft8Step6Ladder.Unscoreable();
        var decoder = new Ft8SlotDecoder();
        var geometry = decoder.Geometry;
        var noise = new GaussianNoise(221_500);

        _output.WriteLine("THE POPULATION, PROVED AT A STRONG RATIO BEFORE THE CURVE STANDS ON IT.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  ratio offered   : {strong:F1} dB, six decibels above where unit 218's");
        _output.WriteLine("                    diagnostic first fell below 100 per cent");
        _output.WriteLine($"  corpus          : {offered.Count} messages, every one of them offered here");
        _output.WriteLine($"  population      : {population.Count} of those, the ones with a text to score");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"#",4} {"kind",32} {"hashed",7} {"in pop",7} {"back",6}  text");

        var missing = new List<string>();
        var byKind = new Dictionary<string, (int Offered, int Back)>(StringComparer.Ordinal);

        for (var i = 0; i < offered.Count; i++)
        {
            var entry = offered[i];
            var (clean, _) = SearchFixture.OneSignal(Rate, entry, OnGridHz, AlignedOffset);
            var signalPower = SearchFixture.TransmissionPower(Rate, entry, OnGridHz);
            var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, strong, Rate);
            var mixed = SearchFixture.AddNoise(clean, noise, sigma, out _);

            var result = decoder.Decode(new Ft8Monitor(geometry).Analyse(mixed));
            var expected = Ft8MessageDecoder.Decode(entry.Message).Text;
            var back = result.Texts.Contains(expected, StringComparer.Ordinal);

            var inPopulation = Ft8Step6Ladder.CanBeScored(entry);
            if (inPopulation)
            {
                var seen = byKind.TryGetValue(entry.Kind, out var tally) ? tally : (0, 0);
                byKind[entry.Kind] = (seen.Item1 + 1, seen.Item2 + (back ? 1 : 0));

                if (!back)
                {
                    missing.Add($"{entry.Label} ({entry.Kind}): expected '{expected}', "
                        + $"got [{string.Join(" | ", result.Texts)}]");
                }
            }

            _output.WriteLine($"{i + 1,4} {entry.Kind,32} {entry.CarriesHashedCallsign,7} "
                + $"{inPopulation,7} {(back ? "back" : "MISSED"),6}  {expected}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("BY KIND, over the population only:");
        foreach (var kind in byKind.Keys.OrderBy(k => k, StringComparer.Ordinal))
        {
            var (count, back) = byKind[kind];
            _output.WriteLine($"  {kind,32} : {back} of {count}");
        }

        var freeText = population.Count(e => string.Equals(e.Kind, "free text", StringComparison.Ordinal));

        _output.WriteLine(string.Empty);
        _output.WriteLine("TASK 2e's TWO NAMED REQUIREMENTS, AND ONE OF THEM THE LADDER'S OWN");
        _output.WriteLine("CONSTRUCTION FORBIDS - WHICH IS MEASURED HERE RATHER THAN INHERITED:");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  FREE TEXT messages in the population : {freeText}  - REQUIREMENT MET");
        _output.WriteLine($"  HASHED-CALLSIGN messages excluded    : {unscoreable.Count}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  A HASHED CALLSIGN CANNOT BE SCORED IN A SINGLE SYNTHESIZED SLOT, and the");
        _output.WriteLine("  reason is the protocol rather than this port. A 22-bit hashed callsign");
        _output.WriteLine("  resolves only against a cache the receiver warms from EARLIER decodes, and");
        _output.WriteLine("  one slot has no history behind it. The truth side of the comparison is");
        _output.WriteLine("  Ft8MessageDecoder.Decode(entry.Message).Text, and for these entries that");
        _output.WriteLine("  is ITSELF THE EMPTY STRING - so there is no text on either side to compare");
        _output.WriteLine("  and such an entry can never count as returned. Keeping them in would cap");
        _output.WriteLine("  every rate on the curve at 51 of 56 and make the TOP of the ladder read 91");
        _output.WriteLine("  per cent for a reason with nothing to do with sensitivity.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  THE EXCLUDED ENTRIES, NAMED:");
        foreach (var entry in unscoreable)
        {
            _output.WriteLine($"    {entry.Label} ({entry.Kind}), truth text "
                + $"'{Ft8MessageDecoder.Decode(entry.Message).Text}'");
        }

        _output.WriteLine(string.Empty);

        if (missing.Count == 0)
        {
            _output.WriteLine($"  EVERY ONE OF THE {population.Count} MESSAGES IN THE POPULATION CAME BACK");
            _output.WriteLine($"  AT {strong:F1} dB, so a rung short of 100 per cent lower down the ladder");
            _output.WriteLine("  is the receiver and not a message the path was never going to return.");
        }
        else
        {
            _output.WriteLine($"  {missing.Count} MESSAGE(S) IN THE POPULATION DID NOT COME BACK AT "
                + $"{strong:F1} dB:");
            foreach (var line in missing)
            {
                _output.WriteLine($"    {line}");
            }

            _output.WriteLine(string.Empty);
            _output.WriteLine("  These stay in the population and are NOT removed to flatter the curve.");
            _output.WriteLine("  The top rung's rate is read against this count rather than against 100.");
        }

        // Only what must always be true: the population is scoreable and the excluded ones are not.
        Assert.True(freeText > 0, "task 2e wants at least one free-text message in the population");
        Assert.All(population, e => Assert.NotEqual(string.Empty, Ft8MessageDecoder.Decode(e.Message).Text));
        Assert.All(unscoreable, e => Assert.Equal(string.Empty, Ft8MessageDecoder.Decode(e.Message).Text));
        Assert.Equal(population.Count, byKind.Values.Sum(v => v.Offered));
    }

    /// <summary>
    /// <b>CRITERION 1 AND CRITERION 2. The curve.</b> Fourteen rungs from -10 dB to -30 dB, one
    /// decibel apart through the collapse, with a Wilson 95 per cent interval on every rate and the
    /// verdict at -21 dB read against the band fixed before the run.
    /// </summary>
    [Fact]
    public void TheDecodeRateIsMeasuredAtEveryRungAndTheVerdictIsReadAtMinusTwentyOne()
    {
        var population = Ft8Step6Ladder.Population();

        _output.WriteLine("STEP 6'S CURVE. Samples in, text out, on signals this library made.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("THE THINGS FIXED IN WRITING BEFORE THIS RAN, all of them in");
        _output.WriteLine("Ft8Step6Ladder.cs and committed before this test executed once:");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  the published figure   : {Ft8Step6Ladder.PublishedThresholdDecibels:F1} dB");
        _output.WriteLine("  its convention         : signal power over noise power in a 2500 Hz");
        _output.WriteLine("                           reference bandwidth, and a threshold in that mode");
        _output.WriteLine("                           conventionally means about 50 per cent decodes");
        _output.WriteLine("  ITS PROVENANCE         : PHASE_PLAN.md's step 6 names it. THE QEX PAPER");
        _output.WriteLine("                           IS NOT ON THIS MACHINE and neither is any copy of");
        _output.WriteLine("                           the WSJT-X documentation, so THE FIGURE AND THE");
        _output.WriteLine("                           50 PER CENT ARE STATED AS AN ASSUMPTION rather");
        _output.WriteLine("                           than quoted from a source that was opened. The");
        _output.WriteLine("                           licensing boundary forbids ft4_ft8_public/ and");
        _output.WriteLine("                           WSJT-X source, and no route around it was taken.");
        _output.WriteLine($"  the verdict band       : >= {Ft8Step6Ladder.MetAtOrAbovePercent:F0} % MET, "
            + $"{Ft8Step6Ladder.NotMetBelowPercent:F0} to {Ft8Step6Ladder.MetAtOrAbovePercent:F0} % "
            + $"PARTIAL, < {Ft8Step6Ladder.NotMetBelowPercent:F0} % NOT MET");
        _output.WriteLine($"  the rungs              : {Ft8Step6Ladder.Rungs.Length}, "
            + $"{string.Join(", ", Ft8Step6Ladder.Rungs.Select(r => r.ToString("F0")))}");
        _output.WriteLine($"  the population         : {population.Count} distinct messages of the "
            + $"corpus's {population.Count + Ft8Step6Ladder.Unscoreable().Count}, "
            + $"{population.Count(e => e.Kind == "free text")} of them free text; the "
            + $"{Ft8Step6Ladder.Unscoreable().Count} excluded are the hashed-callsign entries, which "
            + "have no text on either side to compare and are named by the population probe");
        _output.WriteLine($"  the seeds              : {Ft8Step6Ladder.SeedsFor(-20.0)} through the "
            + $"collapse, {Ft8Step6Ladder.SeedsFor(-10.0)} on the anchors, from "
            + $"{string.Join(", ", Ft8Step6Ladder.Seeds)}");
        _output.WriteLine($"  trials per rung        : {Ft8Step6Ladder.TrialsFor(-21.0)} from -16 to -24, "
            + $"{Ft8Step6Ladder.TrialsFor(-10.0)} elsewhere");
        _output.WriteLine("  the floors             : 200 and 100. NOTHING WAS THINNED.");

        var total = Ft8Step6Ladder.Rungs.Sum(Ft8Step6Ladder.TrialsFor);
        _output.WriteLine($"  SLOT DECODES, ONE PASS : {total}");
        _output.WriteLine($"  base frequency         : {OnGridHz:F2} Hz, EXACTLY ON A BIN CENTRE");
        _output.WriteLine($"  sample offset          : {AlignedOffset}, ON THE BLOCK GRID");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  NO BOUND ON THE RATE IS ASSERTED ANYWHERE IN THIS TEST. The table is");
        _output.WriteLine("  printed and the verdict is read off it against the band above.");
        _output.WriteLine(string.Empty);

        var rows = Ft8Step6Ladder.Walk(population, OnGridHz, AlignedOffset);

        _output.WriteLine("THE CURVE. Binned by the DELIVERED ratio, never by the requested one.");
        _output.WriteLine(string.Empty);
        _output.WriteLine(Ft8Step6Ladder.Header);
        foreach (var row in rows)
        {
            _output.WriteLine(row.AsRow());
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  lo 95 and hi 95 are the WILSON score interval at 95 per cent, which is the");
        _output.WriteLine("  interval a rate near zero needs - the normal approximation puts 2 of 52");
        _output.WriteLine("  below zero and is worthless exactly where this curve is interesting.");
        _output.WriteLine("  cand / par / crc / txt are MEANS PER SLOT: candidates found, of those how");
        _output.WriteLine("  many reached a valid codeword, of those how many carried their own");
        _output.WriteLine("  checksum, of those how many became words.");
        _output.WriteLine(string.Empty);

        var worstDelivery = rows.Max(r => r.WorstDeliveryError);
        _output.WriteLine("REQUESTED AGAINST DELIVERED, PRINTED BEFORE ANY BOUND IS ASSERTED:");
        _output.WriteLine($"  WORST ERROR OVER THE WHOLE RUN : {worstDelivery:F4} dB");
        _output.WriteLine($"  mean absolute error            : "
            + $"{rows.Average(r => Math.Abs(r.DeliveredMean - r.Requested)):F4} dB");
        _output.WriteLine(string.Empty);

        var atThreshold = rows.Single(r => Math.Abs(r.Requested - Ft8Step6Ladder.PublishedThresholdDecibels) < 1e-9);
        var (lower, upper) = atThreshold.Interval;

        _output.WriteLine("CRITERION 2 - THE RATE AT THE RATIO THE CRITERION NAMES:");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  DELIVERED {atThreshold.DeliveredMean:F3} dB: "
            + $"{atThreshold.Returned} of {atThreshold.Trials}, {atThreshold.Rate:F1} per cent, "
            + $"95 per cent interval {lower:F1} to {upper:F1}");
        _output.WriteLine(string.Empty);

        var verdict = atThreshold.Rate >= Ft8Step6Ladder.MetAtOrAbovePercent
            ? "MET"
            : atThreshold.Rate < Ft8Step6Ladder.NotMetBelowPercent
                ? "NOT MET"
                : "PARTIAL";

        _output.WriteLine($"  VERDICT AGAINST THE BAND FIXED BEFORE THE RUN: {verdict}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("THE NEIGHBOURING RUNGS, because a threshold is a curve's shape and not a");
        _output.WriteLine("single point:");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"delivered",10} {"n of N",14} {"rate %",8} {"95 % interval",18}");

        foreach (var rung in new[] { -19.0, -20.0, -21.0, -22.0, -23.0 })
        {
            var row = rows.Single(r => Math.Abs(r.Requested - rung) < 1e-9);
            var (lo, hi) = row.Interval;
            _output.WriteLine($"{row.DeliveredMean,10:F3} {$"{row.Returned} of {row.Trials}",14} "
                + $"{row.Rate,8:F1} {$"{lo:F1} to {hi:F1}",18}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("DOES THE SHAPE RESEMBLE A THRESHOLD AT ALL? A real decoder's rate falls off");
        _output.WriteLine("over several decibels. The span this one falls over, measured:");
        _output.WriteLine(string.Empty);

        var lastHigh = rows.LastOrDefault(r => r.Rate >= 90.0);
        var firstZero = rows.FirstOrDefault(r => r.Rate <= 0.0);
        var crossing = rows.LastOrDefault(r => r.Rate >= 50.0);

        _output.WriteLine($"  lowest rung still at 90 per cent or better : "
            + $"{(lastHigh is null ? "none" : $"{lastHigh.DeliveredMean:F1} dB at {lastHigh.Rate:F1} %")}");
        _output.WriteLine($"  lowest rung still at 50 per cent or better : "
            + $"{(crossing is null ? "none" : $"{crossing.DeliveredMean:F1} dB at {crossing.Rate:F1} %")}");
        _output.WriteLine($"  highest rung returning nothing at all      : "
            + $"{(firstZero is null ? "none" : $"{firstZero.DeliveredMean:F1} dB")}");

        if (lastHigh is not null && firstZero is not null)
        {
            _output.WriteLine($"  SO THE WHOLE COLLAPSE HAPPENS INSIDE            : "
                + $"{Math.Abs(firstZero.DeliveredMean - lastHigh.DeliveredMean):F1} dB");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("CRITERION 3 - THE WRONG-MESSAGE COUNT AT EVERY RUNG OF THE WHOLE LADDER,");
        _output.WriteLine("not only the bottom ones. It is the single number in this phase that must");
        _output.WriteLine("be zero.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"delivered",10} {"trials",8} {"WRONG",7}");

        foreach (var row in rows)
        {
            _output.WriteLine($"{row.DeliveredMean,10:F3} {row.Trials,8} {row.Wrong,7}");
            foreach (var wrongText in row.WrongTexts)
            {
                _output.WriteLine($"    RETURNED BUT NOT TRANSMITTED: {wrongText}");
            }
        }

        var totalWrong = rows.Sum(r => r.Wrong);
        var totalTrials = rows.Sum(r => r.Trials);

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  WRONG MESSAGES, WHOLE LADDER: {totalWrong} out of {totalTrials} trials");
        _output.WriteLine(string.Empty);
        _output.WriteLine("REPRODUCIBILITY IS PROVEN BY RUNNING THIS TEST TWICE IN TWO SEPARATE");
        _output.WriteLine("PROCESSES AND COMPARING THE TABLE ROW FOR ROW. An in-process re-run would");
        _output.WriteLine("hide exactly the state a fresh process exposes, and criterion 1 says");
        _output.WriteLine("reproducible rather than ran once.");

        // ONLY WHAT MUST ALWAYS BE TRUE. Nothing here asserts where the collapse is, and nothing
        // here asserts a rate at -21 dB - the verdict is read off the table above.
        Assert.Equal(0, totalWrong);
        Assert.Equal(Ft8Step6Ladder.Rungs.Length, rows.Count);
        Assert.True(worstDelivery < 0.05, $"the delivered ratio drifted from the requested one by "
            + $"{worstDelivery:F4} dB, which would make the row's bin a fiction");
    }

    /// <summary>
    /// <b>CRITERION 3 — below the threshold, degrade rather than lie.</b> Pure noise slots with no
    /// signal in them at all, at the amplitudes the bottom three rungs deliver.
    /// </summary>
    /// <remarks>
    /// <b>Candidates found in noise are the search behaving correctly and are not a failure.</b> The
    /// search is permissive by design and the parity and CRC gates are what refuse. Unit 218 measured
    /// 183 candidates on 18 empty slots with zero reaching parity. <b>The number that must be zero is
    /// messages returned</b>, because a message returned from a slot with nothing in it is a message
    /// nobody sent.
    /// </remarks>
    [Fact]
    public void SlotsWithNoSignalInThemAtAllReturnNothing()
    {
        var population = Ft8Step6Ladder.Population();
        var reference = population[0];
        var decoder = new Ft8SlotDecoder();
        var geometry = decoder.Geometry;
        var signalPower = SearchFixture.TransmissionPower(Rate, reference, OnGridHz);

        // The amplitudes the bottom three rungs deliver, so the floor is measured at the noise levels
        // the curve's own bottom actually carries rather than at an arbitrary one.
        var amplitudeRungs = new[] { -26.0, -28.0, -30.0 };
        var seeds = new[] { 221_701, 221_702, 221_703, 221_704, 221_705, 221_706 };

        _output.WriteLine("PURE NOISE. No signal in the slot at all, at the amplitudes the bottom");
        _output.WriteLine("three rungs of the curve deliver.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  amplitudes from rungs : {string.Join(", ", amplitudeRungs.Select(r => $"{r:F0} dB"))}");
        _output.WriteLine($"  seeds per amplitude   : {seeds.Length}");
        _output.WriteLine($"  EMPTY SLOTS IN TOTAL  : {amplitudeRungs.Length * seeds.Length}");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"rung",8} {"seed",8} {"cand",6} {"par",5} {"crc",5} {"txt",5} {"MESSAGES",9}");

        long candidates = 0;
        long parity = 0;
        long checksum = 0;
        long text = 0;
        long messages = 0;

        foreach (var rung in amplitudeRungs)
        {
            var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, rung, Rate);

            foreach (var seed in seeds)
            {
                var empty = SearchFixture.EmptySlot(Rate);
                var noise = new GaussianNoise(seed);
                var slot = SearchFixture.AddNoise(empty, noise, sigma, out _);

                var result = decoder.Decode(new Ft8Monitor(geometry).Analyse(slot));

                candidates += result.CandidateCount;
                parity += result.ParitySatisfiedCount;
                checksum += result.ChecksumPassedCount;
                text += result.BecameTextCount;
                messages += result.Texts.Count;

                _output.WriteLine($"{rung,8:F1} {seed,8} {result.CandidateCount,6} "
                    + $"{result.ParitySatisfiedCount,5} {result.ChecksumPassedCount,5} "
                    + $"{result.BecameTextCount,5} {result.Texts.Count,9}");

                foreach (var got in result.Texts)
                {
                    _output.WriteLine($"    RETURNED FROM AN EMPTY SLOT: {got}");
                }
            }
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  candidates found, total          : {candidates}");
        _output.WriteLine($"  of those, reaching parity        : {parity}");
        _output.WriteLine($"  of those, passing their checksum : {checksum}");
        _output.WriteLine($"  of those, becoming words         : {text}");
        _output.WriteLine($"  MESSAGES RETURNED                : {messages}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  Candidates and no text is the path behaving correctly. The search is");
        _output.WriteLine("  permissive by design; the parity and CRC gates are what refuse.");

        Assert.Equal(0, messages);
        Assert.Equal(0, checksum);
    }
}
