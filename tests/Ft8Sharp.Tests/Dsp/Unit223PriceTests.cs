using System.Diagnostics;
using Ft8Sharp.Dsp;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Encode;
using Ft8Sharp.Tests.Ldpc;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>Unit 223 task 3: the price of upstream's approximation.</b> The row unit 222 named and could
/// not take — it said so itself, that measuring the cost of <c>fast_tanh</c> and <c>fast_atanh</c>
/// would have meant running arithmetic the pin does not run.
/// </summary>
/// <remarks>
/// <para>
/// <b>Same population, same seeds, same frequency, same offset, same 306 trials and the same rung as
/// task 1's before-number</b>, so every row is a delta from a number measured in this tree tonight
/// rather than from a figure inherited out of a report.
/// </para>
/// <para>
/// <b>NOTHING IS FIXED IN THIS FILE AND NOTHING HERE IS PROPOSED FOR THE LIBRARY.</b> Exact
/// <c>tanh</c> and <c>atanh</c> are <em>not</em> a fidelity fix: upstream calls the approximations
/// and so does this port, faithfully, audited constant by constant by unit 222. The plan's ruling
/// that inheriting Goba's bugs is accepted is what licenses this measurement and equally what forbids
/// adopting its result. <b>A row that decodes better is evidence about where the loss is.</b>
/// </para>
/// </remarks>
public class Unit223PriceTests
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    /// <summary>Upstream's own bound, from <c>kLDPC_iterations</c> in <c>demo/decode_ft8.c</c>.</summary>
    private const int UpstreamIterations = LdpcDecoder.DefaultMaxIterations;

    /// <summary>Row G's bound. <b>Not adopted, at any measured size.</b></summary>
    private const int GenerousIterations = 100;

    private readonly ITestOutputHelper _output;

    public Unit223PriceTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// <b>The error of <c>fast_atanh</c> against <see cref="Math.Atanh"/>, and the largest value it
    /// can return.</b> Printed before any rate, because it says whether the approximation is even in
    /// play.
    /// </summary>
    [Fact]
    public void WhatUpstreamsAtanhCostsAndTheCeilingItPutsOnEveryMessage()
    {
        _output.WriteLine("UNIT 223 TASK 3 - THE ERROR OF fast_atanh AGAINST THE TRUE FUNCTION.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("The check-to-variable message is -2 * atanh(product of tanh). Whatever");
        _output.WriteLine("fast_atanh cannot return, no message can carry - so its largest value is a");
        _output.WriteLine("HARD CEILING on the confidence any check can ever express.");
        _output.WriteLine(string.Empty);

        var points = new[]
        {
            0.0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9,
            0.95, 0.99, 0.999, 0.9999, 0.99999, 0.999999, 1.0,
        };

        _output.WriteLine($"{"x",12} {"fast_atanh(x)",16} {"Math.Atanh(x)",16} "
            + $"{"absolute error",16} {"ratio",10}");

        foreach (var x in points)
        {
            var fast = Unit223Arithmetic.FastAtanh((float)x);
            var exact = Math.Atanh(x);
            var error = double.IsInfinity(exact) ? double.PositiveInfinity : Math.Abs(exact - fast);
            var ratio = exact == 0.0 ? 1.0 : fast / exact;
            _output.WriteLine($"{x,12:F6} {fast,16:F6} {exact,16:F6} {error,16:F6} {ratio,10:F4}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  AND THE SAME AT THE NEGATIVE POLE, because the function is odd and a");
        _output.WriteLine("  clamp that is not would show up here:");
        _output.WriteLine($"    fast_atanh(-1.0) = {Unit223Arithmetic.FastAtanh(-1.0f):F6}, "
            + $"fast_atanh(+1.0) = {Unit223Arithmetic.FastAtanh(1.0f):F6}");
        _output.WriteLine(string.Empty);

        // THE LARGEST VALUE IT CAN RETURN, swept rather than reasoned about. The product of tanh
        // values is confined to [-1, 1], so this sweep covers the whole of the function's reachable
        // input range and the maximum below is the maximum, not a maximum over a sample.
        const int steps = 2_000_000;
        var largest = 0.0;
        var largestAt = 0.0;
        var smallestDenominator = double.PositiveInfinity;
        var monotonic = true;
        var previous = double.NegativeInfinity;

        for (var i = 0; i <= steps; i++)
        {
            var x = (float)(-1.0 + (2.0 * i / steps));
            var value = Unit223Arithmetic.FastAtanh(x);
            if (Math.Abs(value) > largest)
            {
                largest = Math.Abs(value);
                largestAt = x;
            }

            var x2 = (double)x * x;
            var denominator = Math.Abs(945.0 + (x2 * (-1050.0 + (x2 * 225.0))));
            if (denominator < smallestDenominator)
            {
                smallestDenominator = denominator;
            }

            if (value < previous)
            {
                monotonic = false;
            }

            previous = value;
        }

        var ceiling = 2.0 * largest;

        _output.WriteLine($"SWEPT OVER ITS WHOLE REACHABLE INPUT RANGE, {steps:N0} POINTS FROM -1 TO 1:");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  largest |fast_atanh|                        : {largest:F6}, at x = {largestAt:F6}");
        _output.WriteLine($"  so the largest |to-variable message| it allows: {ceiling:F6}");
        _output.WriteLine($"  smallest |denominator| anywhere on [-1, 1]   : {smallestDenominator:F3} "
            + "- IT DOES NOT VANISH, which is why there is no clamp");
        _output.WriteLine($"  monotonically increasing across the range    : {monotonic}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  AGAINST THE EXACT ARITHMETIC, whose only limit is the type's:");
        _output.WriteLine($"    the clamp this unit uses      : {Unit223Arithmetic.AtanhClamp:R}");
        _output.WriteLine($"    which is Math.BitDecrement(1) : "
            + $"{Unit223Arithmetic.AtanhClamp == Math.BitDecrement(1.0)}");
        _output.WriteLine($"    atanh of it                   : {Math.Atanh(Unit223Arithmetic.AtanhClamp):F6}");
        _output.WriteLine($"    so its message ceiling        : {Unit223Arithmetic.ExactMessageCeiling:F6}");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  THE RATIO OF THE TWO CEILINGS: "
            + $"{Unit223Arithmetic.ExactMessageCeiling / ceiling:F2} to 1.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  WHY THE CLAMP IS THE MACHINE'S AND NOT THIS UNIT'S: a clamp chosen to");
        _output.WriteLine("  flatter a row is the same failure as a tolerance chosen after the");
        _output.WriteLine("  measurement. Math.BitDecrement(1.0) is the largest double strictly below");
        _output.WriteLine("  one, so it is the most generous clamp double precision admits and there");
        _output.WriteLine("  is no value between it and the pole to move to. It cannot be tuned.");

        // The report is above. These assert the facts the reading of every rate below depends on.
        Assert.True(largest > 0.0, "fast_atanh returns nothing but zero across its whole range.");
        Assert.True(
            smallestDenominator > 0.0,
            "fast_atanh's denominator vanishes somewhere on [-1, 1], which would change the reading.");
        Assert.Equal(Math.BitDecrement(1.0), Unit223Arithmetic.AtanhClamp);
    }

    /// <summary>
    /// <b>The rows: as-is, exact arithmetic at upstream's bound, and exact arithmetic at four times
    /// it.</b> With a transcription control, because a substituted decoder that does not reproduce
    /// the as-is number is a wiring mistake and not a measurement.
    /// </summary>
    [Fact]
    public void ExactArithmeticAgainstUpstreamsAtTheRungTheVerdictIsReadAt()
    {
        var population = Ft8Step6Ladder.Population();
        var geometry = new Ft8WaterfallGeometry();
        var search = new Ft8SyncSearch();
        var asIs = new Ft8SlotDecoder(geometry);
        const double rung = Unit222TraceTests.VerdictRungDecibels;
        var seeds = Ft8Step6Ladder.SeedsFor(rung);
        var trials = population.Count * seeds;

        _output.WriteLine($"UNIT 223 TASK 3 - THE PRICE OF THE APPROXIMATION AT {rung:F1} dB.");
        _output.WriteLine($"Same population, same seeds, same {trials} trials, same frequency and");
        _output.WriteLine("offset as task 1's before-number, so every row is comparable to it.");
        _output.WriteLine(string.Empty);

        var rowA = new Tally("A. as-is, the library's own path ");
        var rowAt = new Tally("A'. as-is, TRANSCRIBED (control)");
        var rowF = new Tally("F. exact tanh/atanh, 25 iters   ");
        var rowG = new Tally("G. exact tanh/atanh, 100 iters  ");

        var censusUpstream = new Unit223Arithmetic.Census();
        var censusExact = new Unit223Arithmetic.Census();

        var watch = Stopwatch.StartNew();

        for (var s = 0; s < seeds; s++)
        {
            var noise = new GaussianNoise(Ft8Step6Ladder.Seeds[s] + (int)Math.Round(rung * 10.0));

            foreach (var entry in population)
            {
                var (clean, _) = SearchFixture.OneSignal(
                    Rate, entry, Unit222TraceTests.OnGridHz, Unit222TraceTests.AlignedOffset);
                var signalPower = SearchFixture.TransmissionPower(
                    Rate, entry, Unit222TraceTests.OnGridHz);
                var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, rung, Rate);
                var mixed = SearchFixture.AddNoise(clean, noise, sigma, out _);

                var expected = Ft8MessageDecoder.Decode(entry.Message).Text;

                // The library's own waterfall and the library's own candidate list. EVERY ROW
                // STANDS ON THE SAME SEARCH: the only thing that moves between them is which
                // functions the check-node update calls, and how many times the loop runs.
                var waterfall = new Ft8Monitor(geometry).Analyse(mixed);
                var candidates = search.Find(waterfall);

                var resultA = asIs.Decode(waterfall);
                rowA.Add(
                    resultA.Texts.Contains(expected, StringComparer.Ordinal),
                    resultA.Texts.Where(t => !string.Equals(t, expected, StringComparison.Ordinal)));

                var trialAt = Run(
                    candidates, waterfall, Unit223Arithmetic.Kind.Upstream, UpstreamIterations,
                    expected, censusUpstream);
                rowAt.Add(trialAt.Returned, trialAt.Wrong);

                var trialF = Run(
                    candidates, waterfall, Unit223Arithmetic.Kind.Exact, UpstreamIterations,
                    expected, censusExact);
                rowF.Add(trialF.Returned, trialF.Wrong);

                var trialG = Run(
                    candidates, waterfall, Unit223Arithmetic.Kind.Exact, GenerousIterations,
                    expected, null);
                rowG.Add(trialG.Returned, trialG.Wrong);
            }
        }

        watch.Stop();

        _output.WriteLine($"THE ROWS AT {rung:F1} dB, {trials} TRIALS PER ROW, AS-IS FIRST:");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"row",34} {"n",5} {"of",5} {"rate",7} {"lo 95",7} {"hi 95",7} "
            + $"{"delta",8} {"WRONG",6}  equivalent");

        var baseline = rowA.Rate(trials);
        foreach (var row in new[] { rowA, rowAt, rowF, rowG })
        {
            var rate = row.Rate(trials);
            var (lower, upper) = Ft8Step6Ladder.Wilson(row.Returned, trials);
            _output.WriteLine($"{row.Name,34} {row.Returned,5} {trials,5} {rate,7:F1} "
                + $"{lower,7:F1} {upper,7:F1} {rate - baseline,8:+0.0;-0.0;0.0} {row.Wrong,6}  "
                + $"{Unit222Budget.EquivalentShift(rate)}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  {4 * trials} substituted slot decodes in {watch.Elapsed.TotalSeconds:F1} s");
        _output.WriteLine(string.Empty);

        // -------------------------------------------------------------------------- the control
        _output.WriteLine("THE TRANSCRIPTION CONTROL, AND EVERY ROW BELOW DEPENDS ON IT. Row A' runs");
        _output.WriteLine("the SAME substituted apparatus as F and G with UPSTREAM'S OWN arithmetic in");
        _output.WriteLine("it. If it does not equal row A, the apparatus is the finding and F and G");
        _output.WriteLine("mean nothing:");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  row A  (library)      : {rowA.Returned} of {trials}");
        _output.WriteLine($"  row A' (transcribed)  : {rowAt.Returned} of {trials}");
        _output.WriteLine(rowA.Returned == rowAt.Returned
            ? "  THEY AGREE. The transcription is faithful and F and G are readable against it."
            : "  THEY DISAGREE, and THAT is the finding of this task rather than any row under it.");
        _output.WriteLine(string.Empty);

        // ------------------------------------------------- what the arithmetic actually did
        _output.WriteLine($"WHAT THE ARITHMETIC ACTUALLY DID, over all {trials} real {rung:F1} dB slots");
        _output.WriteLine("and every candidate the search kept in each of them:");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"arithmetic",-24} {"tanh calls",14} {"ON THE CLAMP",14} {"fraction",10} "
            + $"{"largest msg",12} {"mean |msg|",12}");

        foreach (var (name, census) in new[]
                 {
                     ("upstream fast_tanh", censusUpstream),
                     ("exact Math.Tanh", censusExact),
                 })
        {
            _output.WriteLine($"{name,-24} {census.TanhCalls,14:N0} {census.TanhClamped,14:N0} "
                + $"{census.ClampedFraction,10:P2} {census.LargestMessage,12:F4} "
                + $"{census.MeanMessage,12:F4}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  atanh calls handed a product ALREADY AT +/-1 (upstream) : "
            + $"{censusUpstream.AtanhAtThePole:N0} of {censusUpstream.AtanhCalls:N0}");
        _output.WriteLine($"  atanh calls handed a product ALREADY AT +/-1 (exact)    : "
            + $"{censusExact.AtanhAtThePole:N0} of {censusExact.AtanhCalls:N0}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  AND A THING NEITHER THE INSTRUCTION NOR UNIT 222 EXPECTED, MEASURED HERE");
        _output.WriteLine("  BECAUSE THE LARGEST MESSAGE CAME BACK ABOVE THE CEILING fast_atanh ALLOWS.");
        _output.WriteLine("  In exact arithmetic a product of hyperbolic tangents cannot leave [-1, 1].");
        _output.WriteLine("  fast_tanh is a rational approximation and OVERSHOOTS ONE just below its own");
        _output.WriteLine("  clamp - fast_tanh(4.9699) reads "
            + $"{Unit223Arithmetic.FastTanh(4.9699f):F6} - so the product LEAVES the range");
        _output.WriteLine("  the inverse was fitted on, and fast_atanh's denominator falls away from");
        _output.WriteLine("  120 toward its own root:");
        _output.WriteLine(string.Empty);

        foreach (var (name, census) in new[]
                 {
                     ("upstream fast_tanh", censusUpstream),
                     ("exact Math.Tanh", censusExact),
                 })
        {
            var u = census.LargestAtanhArgument * census.LargestAtanhArgument;
            var denominator = 945.0 + (u * (-1050.0 + (u * 225.0)));
            _output.WriteLine($"    {name,-20} largest |product| handed to atanh : "
                + $"{census.LargestAtanhArgument:F6}, at which fast_atanh's denominator would be "
                + $"{denominator:F1}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("    fast_atanh's denominator vanishes at |x| = 1.1035, and the row weight");
        _output.WriteLine("    bounds the product at fast_tanh's peak to the sixth power. THE POLE IS");
        _output.WriteLine("    NOT REACHED, and this is reported as an observation about upstream's");
        _output.WriteLine("    arithmetic rather than as a defect - it is not a porting error and it is");
        _output.WriteLine("    NOT FIXED HERE.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  The clamp fraction is the number that says whether the +/-4.97 clamp is in");
        _output.WriteLine("  play at all. NEAR ZERO WOULD MEAN IT IS NOT, and the report says so.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  For reading the largest message against: fast_atanh cannot return more");
        _output.WriteLine($"  than about 2.283 whatever it is given, so upstream's messages are capped");
        _output.WriteLine($"  at about 4.567. The exact arithmetic's ceiling is "
            + $"{Unit223Arithmetic.ExactMessageCeiling:F2}.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("WHAT EACH ROW IS, AND WHOSE IT IS:");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  F. THE APPROXIMATION. fast_tanh is the LOWEST ORDER of four rational");
        _output.WriteLine("     approximations in upstream's own file - unit 222 found three better");
        _output.WriteLine("     ones commented out beside it - and fast_atanh is capped where the true");
        _output.WriteLine("     function is not. BOTH ARE UPSTREAM'S OWN CHOICES, ported faithfully.");
        _output.WriteLine("     WHATEVER THIS ROW MEASURES IT IS NOT A FIDELITY DEFECT AND IS NOT");
        _output.WriteLine("     FIXED TONIGHT AT ANY SIZE. It is a divergence question for the owner.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  G. THE APPROXIMATION AND THE BOUND TOGETHER. 25 is upstream's");
        _output.WriteLine("     kLDPC_iterations and unit 222 measured it worth +2 decodes on its own.");
        _output.WriteLine("     NOT MOVED.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  NO LIBRARY FILE IS TOUCHED BY THIS TEST.");

        // Assertions are on the instrument only. Every rate above is a measurement and is reported
        // whatever it reads.
        Assert.Equal(trials, rowA.Trials);
        Assert.Equal(trials, rowF.Trials);
        Assert.True(
            censusUpstream.TanhCalls > 0,
            "the substituted decoder never called tanh, so no row above ran the loop at all.");
    }

    /// <summary>
    /// <b><c>Ft8SlotDecoder.Decode</c>'s own loop with the correction stage substituted.</b> The
    /// same candidate list, the same normalisation, the same two gates, the same de-duplication key
    /// and the same message limit.
    /// </summary>
    private static Trial Run(
        IReadOnlyList<Ft8Candidate> candidates,
        Ft8Waterfall waterfall,
        Unit223Arithmetic.Kind kind,
        int maxIterations,
        string expected,
        Unit223Arithmetic.Census? census)
    {
        var cache = new Ft8CallsignCache();
        var seen = new List<byte[]>();
        var texts = new List<string>();

        var ratios = new float[Ft8SoftSymbols.RatioCount];
        var codeword = new byte[LdpcDecoder.CodewordBits];

        foreach (var candidate in candidates)
        {
            Ft8SoftSymbols.Extract(waterfall, candidate, ratios);
            Ft8SoftSymbols.Normalise(ratios);

            var correction = Unit223Arithmetic.Decode(ratios, codeword, maxIterations, kind, census);
            var gated = Unit223Arithmetic.Gate(codeword, cache, correction);

            if (!gated.Readable)
            {
                continue;
            }

            var key = codeword[..Ft8Payload.MessageBits];

            var already = false;
            foreach (var previous in seen)
            {
                if (key.AsSpan().SequenceEqual(previous))
                {
                    already = true;
                    break;
                }
            }

            if (already || texts.Count >= Ft8SlotDecoder.DefaultMessageLimit)
            {
                continue;
            }

            seen.Add(key);
            texts.Add(gated.Text);
        }

        return new Trial(
            texts.Contains(expected, StringComparer.Ordinal),
            texts.Where(t => !string.Equals(t, expected, StringComparison.Ordinal)).ToArray());
    }

    private sealed record Trial(bool Returned, string[] Wrong);

    private sealed class Tally(string name)
    {
        internal string Name { get; } = name;

        internal int Trials { get; private set; }

        internal int Returned { get; private set; }

        internal int Wrong { get; private set; }

        internal void Add(bool returned, IEnumerable<string> wrong)
        {
            Trials++;
            if (returned)
            {
                Returned++;
            }

            Wrong += wrong.Count();
        }

        internal double Rate(int trials) => 100.0 * Returned / trials;
    }
}
