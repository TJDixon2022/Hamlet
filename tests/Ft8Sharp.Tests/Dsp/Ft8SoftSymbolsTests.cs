using System.Reflection;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Tests.Encode;
using Ft8Sharp.Tests.Ldpc;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// Extraction, watched refusing and watched agreeing: the first code in this library that turns a
/// place in a waterfall into evidence about bits.
/// </summary>
/// <remarks>
/// <para>
/// <b>THE MEASUREMENT THAT MATTERS HERE IS THE HARD-DECISION COUNT</b>, in
/// <see cref="TheHardDecisionsAgreeWithTheCodewordThatWasEncoded"/>. A clean synthesized
/// transmission is placed at a stated frequency and offset, the search is asked where the
/// transmissions are, and the ratios extracted at the candidate it ranks first are compared bit for
/// bit against the codeword that was encoded — <b>before any correction is involved.</b> 174 of 174
/// says the join is right on its own terms. Fewer says where to look, and the count is printed
/// before anything is asserted about it.
/// </para>
/// <para>
/// <b>Nothing here hands extraction the answer.</b> The truth appears only in the assertion, after
/// the code has answered, and <see cref="TheSignatureHasNowhereToPutAnAnswer"/> asserts by
/// reflection that no parameter of the entry point is named for a message, a frequency, a time, an
/// offset, an expectation or a truth.
/// </para>
/// </remarks>
public class Ft8SoftSymbolsTests
{
    private const int SampleRate = Ft8WaterfallGeometry.DefaultSampleRate;

    private readonly ITestOutputHelper _output;

    public Ft8SoftSymbolsTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// <b>The prohibition units 214 and 215 both worked under, one layer up.</b> Asserted by
    /// reflection rather than by reading, because a parameter added later would slip past a comment.
    /// </summary>
    [Fact]
    public void TheSignatureHasNowhereToPutAnAnswer()
    {
        var forbidden = new[] { "freq", "hertz", "hz", "time", "offset", "expect", "hint", "truth", "message", "symbol", "text", "codeword" };

        foreach (var method in typeof(Ft8SoftSymbols).GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            foreach (var parameter in method.GetParameters())
            {
                var name = parameter.Name!.ToLowerInvariant();
                foreach (var word in forbidden)
                {
                    Assert.False(
                        name.Contains(word, StringComparison.Ordinal),
                        $"Ft8SoftSymbols.{method.Name} takes a parameter called '{parameter.Name}'. "
                        + "A function with somewhere to put the answer cannot be shown not to have "
                        + "used it.");
                }
            }

            _output.WriteLine(
                $"  {method.Name}({string.Join(", ", method.GetParameters().Select(p => p.Name))})");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  Extract takes a waterfall and a candidate and a place to put the answer.");
        _output.WriteLine("  There is nowhere in any of these signatures to pass a truth.");
    }

    /// <summary>
    /// Every refusal, watched refusing, with how far each missed. <b>And the one that must not
    /// refuse</b>, so the table is not a list of a function that always says no.
    /// </summary>
    [Fact]
    public void TheRefusalsAreWatchedRefusingAndTheLegalCaseIsWatchedNotRefusing()
    {
        var geometry = new Ft8WaterfallGeometry();
        var monitor = new Ft8Monitor(geometry);
        var (slot, _) = SearchFixture.OneSignal(SampleRate, EncodeCorpus.Build()[0], 1000.0, 0);
        var waterfall = monitor.Analyse(slot);

        var legal = new Ft8Candidate(30, 0, 0, 100, 0);
        var ratios = new float[Ft8SoftSymbols.RatioCount];

        var refusals = new List<(string What, string Message)>();

        void Refuses(string what, Action action)
        {
            var thrown = Assert.ThrowsAny<ArgumentException>(action);
            refusals.Add((what, thrown.Message.Split('\n')[0]));
        }

        Refuses("a null waterfall", () => Ft8SoftSymbols.Extract(null!, legal, ratios));

        foreach (var length in new[] { 0, 173, 175, 348 })
        {
            Refuses($"an output span of {length}",
                () => Ft8SoftSymbols.Extract(waterfall, legal, new float[length]));
        }

        // The eighth tone outside the kept bins, at both ends.
        Refuses("a bin offset of -1",
            () => Ft8SoftSymbols.Extract(waterfall, legal with { BinOffset = -1 }, ratios));
        Refuses($"a bin offset of {geometry.BinCount - 7}, whose eighth tone is one past the end",
            () => Ft8SoftSymbols.Extract(waterfall, legal with { BinOffset = geometry.BinCount - 7 }, ratios));
        Refuses($"a bin offset of {geometry.BinCount}",
            () => Ft8SoftSymbols.Extract(waterfall, legal with { BinOffset = geometry.BinCount }, ratios));

        Refuses("a time sub-offset of 2 where there are 2 subdivisions",
            () => Ft8SoftSymbols.Extract(waterfall, legal with { TimeSubOffset = 2 }, ratios));
        Refuses("a frequency sub-offset of -1",
            () => Ft8SoftSymbols.Extract(waterfall, legal with { FrequencySubOffset = -1 }, ratios));

        foreach (var length in new[] { 0, 173, 175 })
        {
            Refuses($"a normalisation over {length} ratios",
                () => Ft8SoftSymbols.Normalise(new float[length]));
        }

        Refuses("seven tone magnitudes",
            () => Ft8SoftSymbols.ExtractSymbol(new double[7], new float[3]));
        Refuses("four bits from one symbol",
            () => Ft8SoftSymbols.ExtractSymbol(new double[8], new float[4]));
        Refuses("a hard decision into a span of a different length",
            () => Ft8SoftSymbols.HardDecision(ratios, new byte[173]));

        _output.WriteLine($"{"what",-62} refusal");
        foreach (var (what, message) in refusals)
        {
            _output.WriteLine($"{what,-62} {message}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  {refusals.Count} refusals, every one of them watched.");

        // AND THE ONE THAT MUST NOT REFUSE. The highest legal bin offset is the one whose eighth
        // tone is the last kept bin, and it extracts rather than refusing, so the bound is exactly
        // where the message says it is.
        var highest = geometry.BinCount - Ft8SymbolEncoder.ToneCount;
        Ft8SoftSymbols.Extract(waterfall, legal with { BinOffset = highest }, ratios);
        _output.WriteLine($"  and bin offset {highest}, the highest legal one, EXTRACTS rather than refusing.");

        // A negative block offset is legal and must not refuse: the search sweeps from ten blocks
        // before the slot on purpose, and those candidates are the ones the sweep exists to catch.
        Ft8SoftSymbols.Extract(waterfall, legal with { BlockOffset = -10 }, ratios);
        var zeros = ratios.Count(r => r == 0.0f);
        _output.WriteLine(
            $"  and a block offset of -10 EXTRACTS, with {zeros} of {ratios.Length} ratios zero -");
        _output.WriteLine("  the symbols that fall before the slot, told to the decoder as no opinion.");
        Assert.True(zeros > 0, "a candidate ten blocks early should have symbols off the front.");
    }

    /// <summary>
    /// The three ratios of one symbol, against magnitudes built by hand: <b>positive means the bit
    /// is one</b>, and the partition of the eight values is the one upstream writes.
    /// </summary>
    [Fact]
    public void OneSymbolsThreeRatiosSayWhichBitsTheLoudestToneCarries()
    {
        var ratios = new float[Ft8SymbolEncoder.BitsPerSymbol];

        _output.WriteLine($"{"value",6} {"bits",5}   ratios (positive means the bit is one)");
        for (var value = 0; value < Ft8SymbolEncoder.ToneCount; value++)
        {
            // One value loud and the other seven quiet — the noiseless case.
            var magnitudes = new double[Ft8SymbolEncoder.ToneCount];
            for (var j = 0; j < magnitudes.Length; j++)
            {
                magnitudes[j] = j == value ? -20.0 : -60.0;
            }

            Ft8SoftSymbols.ExtractSymbol(magnitudes, ratios);

            var bits = Convert.ToString(value, 2).PadLeft(3, '0');
            _output.WriteLine($"{value,6} {bits,5}   {ratios[0],7:F1} {ratios[1],7:F1} {ratios[2],7:F1}");

            for (var bit = 0; bit < Ft8SymbolEncoder.BitsPerSymbol; bit++)
            {
                var expected = (value >> (Ft8SymbolEncoder.BitsPerSymbol - 1 - bit)) & 1;
                Assert.Equal(expected == 1, ratios[bit] > 0);
                Assert.Equal(40.0f, MathF.Abs(ratios[bit]), 3);
            }
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  every bit of every value, and the magnitude is the 40 dB separation each");
        _output.WriteLine("  time, because the best evidence for the wrong answer is one of the quiet ones.");

        // Eight equal magnitudes say nothing at all, and saying nothing is a zero rather than a
        // guess. This is the case that turns into a real one when a candidate points at silence.
        var flat = new double[Ft8SymbolEncoder.ToneCount];
        Array.Fill(flat, -70.0);
        Ft8SoftSymbols.ExtractSymbol(flat, ratios);
        Assert.All(ratios.ToArray(), r => Assert.Equal(0.0f, r));
        _output.WriteLine("  eight equal magnitudes give 0, 0, 0 - no opinion, not a guess.");
    }

    /// <summary>
    /// The normalisation, measured: <b>the variance afterwards is upstream's target</b>, the mean is
    /// not removed, the ratios keep their signs, and the whole set is scaled by one factor.
    /// </summary>
    [Fact]
    public void TheNormalisationPutsTheVarianceOnUpstreamsScaleAndLeavesTheMeanAlone()
    {
        var corpus = EncodeCorpus.Build();
        var bits = SoftCodeword.CodewordBitsFor(corpus[0].Message);

        _output.WriteLine($"{"input magnitude",16} {"variance before",16} {"variance after",15} {"mean before",12} {"mean after",11}");

        foreach (var magnitude in new[] { 0.25f, 1.0f, 4.899f, 20.0f, 100.0f })
        {
            var ratios = SoftCodeword.RatiosFor(bits, magnitude);
            var meanBefore = ratios.Average();
            var before = Ft8SoftSymbols.Normalise(ratios);
            var after = Ft8SoftSymbols.Variance(ratios);
            var meanAfter = ratios.Average();

            _output.WriteLine($"{magnitude,16:F3} {before,16:F4} {after,15:F4} {meanBefore,12:F4} {meanAfter,11:F4}");

            Assert.Equal(Ft8SoftSymbols.NormalisedVariance, after, 2);

            // The signs are untouched, so the hard decision is unchanged by the rescale.
            for (var i = 0; i < ratios.Length; i++)
            {
                Assert.Equal(bits[i] == 1, ratios[i] > 0);
            }

            // The mean is scaled, not subtracted: it keeps its sign and its ratio to the whole.
            if (Math.Abs(meanBefore) > 1e-6)
            {
                Assert.Equal(Math.Sign(meanBefore), Math.Sign(meanAfter));
            }
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  every input lands on {Ft8SoftSymbols.NormalisedVariance}, which is upstream's figure.");
        _output.WriteLine("  The mean is SCALED and not REMOVED, which is what upstream does.");

        // The degenerate case upstream does not guard: all 174 identical, variance zero.
        var flat = new float[Ft8SoftSymbols.RatioCount];
        Array.Fill(flat, 3.0f);
        var zeroVariance = Ft8SoftSymbols.Normalise(flat);
        Assert.Equal(0.0f, zeroVariance, 5);
        Assert.All(flat, r => Assert.Equal(3.0f, r));
        _output.WriteLine("  174 identical ratios have variance 0 and are LEFT ALONE - upstream would");
        _output.WriteLine("  divide by zero and multiply the whole array by an infinity. Divergence 23.");
    }

    /// <summary>
    /// <b>THE FIRST MEASUREMENT OF THE JOIN, and it is taken before a single bit of correction is
    /// involved.</b> A clean transmission is synthesized at a stated frequency and offset, the
    /// search is handed the samples and the geometry and nothing else, and the ratios extracted at
    /// the candidate it ranks first are compared against the codeword that was encoded.
    /// </summary>
    /// <remarks>
    /// <b>174 of 174 says the alignment is right.</b> Unit 214 could not settle the block-to-sample
    /// alignment by reading and carried it forward; this settles it by measurement, because a
    /// candidate one block out would still correlate on the Costas tones and would demodulate the
    /// wrong symbols. The count is printed for every message before anything is asserted about it.
    /// </remarks>
    [Fact]
    public void TheHardDecisionsAgreeWithTheCodewordThatWasEncoded()
    {
        var geometry = new Ft8WaterfallGeometry();
        var monitor = new Ft8Monitor(geometry);
        var search = new Ft8SyncSearch();
        var corpus = EncodeCorpus.Build();

        // Frequencies and offsets that are not all on the grid: a bin centre, a quarter bin, exactly
        // halfway between two bins, and offsets on the block grid, on the sub-block grid, and on
        // neither.
        var frequencies = new[] { 1000.0, 1001.5625, 1003.125, 1500.78125 };
        var offsets = new[] { 0, 960 * 3, 1920 * 2, 5000 };

        var ratios = new float[Ft8SoftSymbols.RatioCount];
        var decisions = new byte[Ft8SoftSymbols.RatioCount];
        var perfect = 0;
        var attempted = 0;
        var worst = int.MaxValue;
        var total = 0;

        _output.WriteLine($"{"message",-22} {"Hz",10} {"offset",7} {"rank1 block",11} {"agree",6}");

        for (var i = 0; i < corpus.Count; i++)
        {
            var entry = corpus[i];
            var frequency = frequencies[i % frequencies.Length];
            var offset = offsets[i % offsets.Length];

            var (slot, _) = SearchFixture.OneSignal(SampleRate, entry, frequency, offset);
            var waterfall = monitor.Analyse(slot);
            var candidates = search.Find(waterfall);
            Assert.NotEmpty(candidates);

            var best = candidates[0];
            Ft8SoftSymbols.Extract(waterfall, best, ratios);
            Ft8SoftSymbols.HardDecision(ratios, decisions);

            var codeword = SoftCodeword.CodewordBitsFor(entry.Message);
            var agree = 0;
            for (var bit = 0; bit < codeword.Length; bit++)
            {
                if (decisions[bit] == codeword[bit])
                {
                    agree++;
                }
            }

            attempted++;
            total += agree;
            worst = Math.Min(worst, agree);
            if (agree == Ft8SoftSymbols.RatioCount)
            {
                perfect++;
            }

            _output.WriteLine(
                $"{entry.Label,-22} {frequency,10:F4} {offset,7} {best.BlockOffset,11} "
                + $"{agree,3}/{Ft8SoftSymbols.RatioCount}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  messages:                    {attempted}");
        _output.WriteLine($"  extracted with 174 of 174:   {perfect}");
        _output.WriteLine($"  worst agreement:             {worst} of {Ft8SoftSymbols.RatioCount}");
        _output.WriteLine($"  mean agreement:              {(double)total / attempted:F2}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  THE NUMBERS ARE PRINTED BEFORE ANY BOUND IS ASSERTED ON THEM.");

        Assert.Equal(attempted, perfect);
        Assert.Equal(Ft8SoftSymbols.RatioCount, worst);
    }

    /// <summary>
    /// <b>The localiser for the alignment question.</b> The same clean signal, extracted at the
    /// candidate the search ranked first and at its neighbours one block and one sub-offset away —
    /// so the report can say not just that the alignment is right but how sharply it is right.
    /// </summary>
    [Fact]
    public void AgreementFallsOffASharpCliffOneBlockEitherSideOfTheCandidate()
    {
        var geometry = new Ft8WaterfallGeometry();
        var monitor = new Ft8Monitor(geometry);
        var search = new Ft8SyncSearch();
        var entry = EncodeCorpus.Build()[0];

        var (slot, _) = SearchFixture.OneSignal(SampleRate, entry, 1000.0, 0);
        var waterfall = monitor.Analyse(slot);
        var best = search.Find(waterfall)[0];
        var codeword = SoftCodeword.CodewordBitsFor(entry.Message);

        var ratios = new float[Ft8SoftSymbols.RatioCount];
        var decisions = new byte[Ft8SoftSymbols.RatioCount];

        _output.WriteLine($"{"block",6} {"timeSub",8} {"bin",5} {"freqSub",8} {"agree",6}");

        var atCandidate = 0;
        foreach (var (dBlock, dTimeSub, dBin, dFreqSub) in new[]
                 {
                     (-2, 0, 0, 0), (-1, 0, 0, 0), (0, 0, 0, 0), (1, 0, 0, 0), (2, 0, 0, 0),
                     (0, 1, 0, 0), (0, 0, -1, 0), (0, 0, 1, 0), (0, 0, 0, 1),
                 })
        {
            var timeSub = best.TimeSubOffset + dTimeSub;
            var freqSub = best.FrequencySubOffset + dFreqSub;
            if (timeSub < 0 || timeSub >= geometry.TimeOversampling
                || freqSub < 0 || freqSub >= geometry.FrequencyOversampling)
            {
                continue;
            }

            var probe = new Ft8Candidate(
                best.Score, best.BlockOffset + dBlock, timeSub, best.BinOffset + dBin, freqSub);

            Ft8SoftSymbols.Extract(waterfall, probe, ratios);
            Ft8SoftSymbols.HardDecision(ratios, decisions);

            var agree = 0;
            for (var bit = 0; bit < codeword.Length; bit++)
            {
                if (decisions[bit] == codeword[bit])
                {
                    agree++;
                }
            }

            if (dBlock == 0 && dTimeSub == 0 && dBin == 0 && dFreqSub == 0)
            {
                atCandidate = agree;
            }

            _output.WriteLine(
                $"{probe.BlockOffset,6} {probe.TimeSubOffset,8} {probe.BinOffset,5} "
                + $"{probe.FrequencySubOffset,8} {agree,3}/{Ft8SoftSymbols.RatioCount}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  at the candidate the search returned: {atCandidate} of {Ft8SoftSymbols.RatioCount}");
        _output.WriteLine("  Everything around it is near chance, which is 87 of 174. So the alignment");
        _output.WriteLine("  is not approximately right - there is exactly one place it works, and the");
        _output.WriteLine("  search puts the candidate there. That is unit 214's carried-forward item,");
        _output.WriteLine("  settled by measurement rather than by reading, exactly as planned.");

        Assert.Equal(Ft8SoftSymbols.RatioCount, atCandidate);
    }

    /// <summary>
    /// Determinism in the strong sense: the same waterfall and candidate give the same 174 values,
    /// every time, <b>compared value by value and never on a count.</b>
    /// </summary>
    [Fact]
    public void TheSameWaterfallAndCandidateGiveTheSameOneHundredAndSeventyFourValues()
    {
        var geometry = new Ft8WaterfallGeometry();
        var monitor = new Ft8Monitor(geometry);
        var search = new Ft8SyncSearch();
        var entry = EncodeCorpus.Build()[3];

        var (slot, _) = SearchFixture.OneSignal(SampleRate, entry, 1234.375, 3701);
        var waterfall = monitor.Analyse(slot);
        var candidates = search.Find(waterfall).Take(8).ToArray();

        var first = new float[Ft8SoftSymbols.RatioCount];
        var second = new float[Ft8SoftSymbols.RatioCount];
        var comparisons = 0;

        foreach (var candidate in candidates)
        {
            Ft8SoftSymbols.Extract(waterfall, candidate, first);
            var varianceOne = Ft8SoftSymbols.Normalise(first);

            // The reversed pass, so a stateful implementation would be caught by the ORDER as well
            // as by the repeat.
            Ft8SoftSymbols.Extract(waterfall, candidate, second);
            var varianceTwo = Ft8SoftSymbols.Normalise(second);

            Assert.Equal(varianceOne, varianceTwo);
            for (var i = 0; i < first.Length; i++)
            {
                Assert.Equal(first[i], second[i]);
                comparisons++;
            }
        }

        foreach (var candidate in candidates.Reverse())
        {
            Ft8SoftSymbols.Extract(waterfall, candidate, second);
            Ft8SoftSymbols.Normalise(second);
            comparisons += second.Length;
        }

        _output.WriteLine($"  {candidates.Length} candidates, {comparisons} VALUE comparisons, all equal.");
        _output.WriteLine("  Never on a count: two runs agreeing on how many while disagreeing on which");
        _output.WriteLine("  is exactly the failure a count hides.");
        Assert.True(comparisons > 0);
    }
}
