using System.Diagnostics;
using Ft8Sharp.Dsp;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Ldpc;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>Unit 223 task 5: the normalisation target, swept as a measurement.</b>
/// <see cref="Ft8SoftSymbols.NormalisedVariance"/> is 24.0f and upstream's own comment beside it
/// calls it an <em>experimentally found coefficient</em> — one number chosen by measurement, in a
/// function body, and anchored WEAK in this tree since unit 216.
/// </summary>
/// <remarks>
/// <para>
/// <b>THIS IS EVIDENCE ABOUT UPSTREAM'S CONSTANT AND IT IS NEVER AN ADOPTION.</b> The value in
/// <c>src/Ft8Sharp/</c> does not move tonight whatever this table says — that is a divergence
/// question and it belongs to the owner, exactly as the byte-quantised waterfall and the
/// 25-iteration bound did in unit 222 and as exact arithmetic does in task 3.
/// </para>
/// <para>
/// <b>Why the target is not an arbitrary scale, which is what makes the sweep worth taking at
/// all.</b> Belief propagation here is <em>not</em> scale-free: <c>fast_tanh</c> saturates at ±4.97
/// and its clamp is not homogeneous, so the target variance decides how hard the decoder is driven
/// into saturation. An array of variance 24 has a standard deviation of 4.9, which puts a typical
/// ratio <b>exactly at the clamp</b> — and task 3 measured 5.58 per cent of <c>fast_tanh</c> calls
/// landing on it. This sweep is therefore a sweep of that saturation, and 24 sits in the middle of it
/// by construction rather than by accident.
/// </para>
/// <para>
/// <b>The range is stated before the run and 24 is marked in the table.</b> Same population, same
/// seeds, same frequency, same offset and the same 306 trials as task 1's before-number, so the row
/// at 24 must reproduce it.
/// </para>
/// </remarks>
public class Unit223NormalisationSweepTests
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    /// <summary>
    /// <b>The targets, fixed here before the sweep runs.</b> Three below the clamp, three around it,
    /// and three well past it, with upstream's 24 in the middle.
    /// </summary>
    private static readonly float[] Targets =
    {
        3.0f, 6.0f, 12.0f, 18.0f, 21.0f, 24.0f, 27.0f, 30.0f, 36.0f, 48.0f, 96.0f, 192.0f,
    };

    private readonly ITestOutputHelper _output;

    public Unit223NormalisationSweepTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// <b>The decode rate at -21 dB at each target.</b> A table, and nothing more.
    /// </summary>
    [Fact]
    public void TheNormalisationTargetSweptAtTheRungTheVerdictIsReadAt()
    {
        var population = Ft8Step6Ladder.Population();
        var geometry = new Ft8WaterfallGeometry();
        var search = new Ft8SyncSearch();
        const double rung = Unit222TraceTests.VerdictRungDecibels;
        var seeds = Ft8Step6Ladder.SeedsFor(rung);
        var trials = population.Count * seeds;

        _output.WriteLine($"UNIT 223 TASK 5 - THE NORMALISATION TARGET SWEPT AT {rung:F1} dB.");
        _output.WriteLine($"Same population, same seeds, same {trials} trials as task 1's");
        _output.WriteLine("before-number, so the row at 24 must reproduce 13 of 306.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  the library's value       : {Ft8SoftSymbols.NormalisedVariance}f");
        _output.WriteLine($"  targets swept             : {string.Join(", ", Targets)}");
        _output.WriteLine($"  upstream's fast_tanh clamp: +/-4.97, so a target of "
            + $"{4.97 * 4.97:F1} is where a typical ratio sits exactly on it");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  THE VALUE IN src/Ft8Sharp/ DOES NOT MOVE TONIGHT WHATEVER THIS SAYS.");
        _output.WriteLine(string.Empty);

        var returned = new int[Targets.Length];
        var wrong = new int[Targets.Length];
        var clamped = new long[Targets.Length];
        var tanhCalls = new long[Targets.Length];

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

                // ONE waterfall and ONE candidate list, shared by every target, so the only thing
                // moving across the row is the scale the ratios are put on.
                var waterfall = new Ft8Monitor(geometry).Analyse(mixed);
                var candidates = search.Find(waterfall);

                for (var t = 0; t < Targets.Length; t++)
                {
                    var census = new Unit223Arithmetic.Census();
                    var trial = Run(candidates, waterfall, Targets[t], expected, census);

                    if (trial.Returned)
                    {
                        returned[t]++;
                    }

                    wrong[t] += trial.Wrong.Length;
                    clamped[t] += census.TanhClamped;
                    tanhCalls[t] += census.TanhCalls;
                }
            }
        }

        watch.Stop();

        _output.WriteLine($"THE TABLE, {trials} TRIALS AT EVERY TARGET:");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"target",10} {"n",6} {"of",6} {"rate",8} {"lo 95",8} {"hi 95",8} "
            + $"{"WRONG",6} {"on the clamp",14}");

        for (var t = 0; t < Targets.Length; t++)
        {
            var rate = 100.0 * returned[t] / trials;
            var (lower, upper) = Ft8Step6Ladder.Wilson(returned[t], trials);
            var fraction = tanhCalls[t] == 0 ? 0.0 : (double)clamped[t] / tanhCalls[t];
            var mark = Targets[t] == Ft8SoftSymbols.NormalisedVariance ? "  <-- UPSTREAM'S" : string.Empty;
            _output.WriteLine($"{Targets[t],10:F1} {returned[t],6} {trials,6} {rate,8:F1} "
                + $"{lower,8:F1} {upper,8:F1} {wrong[t],6} {fraction,14:P2}{mark}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  {Targets.Length * trials} slot decodes in {watch.Elapsed.TotalSeconds:F1} s");
        _output.WriteLine(string.Empty);

        var atUpstream = Array.IndexOf(Targets, Ft8SoftSymbols.NormalisedVariance);
        var best = 0;
        for (var t = 1; t < Targets.Length; t++)
        {
            if (returned[t] > returned[best])
            {
                best = t;
            }
        }

        _output.WriteLine("THE READING, AND IT IS A READING AND NOT A PROPOSAL:");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  at upstream's {Targets[atUpstream]:F1} : {returned[atUpstream]} of {trials}");
        _output.WriteLine($"  the best row in the sweep : {returned[best]} of {trials} at "
            + $"{Targets[best]:F1}");
        _output.WriteLine($"  the difference            : {returned[best] - returned[atUpstream]} decodes");

        var (bestLower, bestUpper) = Ft8Step6Ladder.Wilson(returned[best], trials);
        var (upstreamLower, upstreamUpper) = Ft8Step6Ladder.Wilson(returned[atUpstream], trials);
        var outside = 100.0 * returned[best] / trials > upstreamUpper
            || 100.0 * returned[best] / trials < upstreamLower;

        _output.WriteLine($"  is the best row OUTSIDE the 24 row's own 95 per cent interval of "
            + $"{upstreamLower:F1} to {upstreamUpper:F1}? {(outside ? "YES" : "NO")}");
        _output.WriteLine($"  and the best row's own interval : {bestLower:F1} to {bestUpper:F1}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  A SWEEP'S BEST ROW IS THE MAXIMUM OF TWELVE DRAWS AND IS BIASED UPWARD BY");
        _output.WriteLine("  CONSTRUCTION. Reading it as a gain would be selecting on the noise, which");
        _output.WriteLine("  is why the interval is printed beside it and why NOTHING HERE MOVES.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  WHAT THE CLAMP COLUMN SHOWS: the target decides how hard the decoder is");
        _output.WriteLine("  driven into fast_tanh's saturation, which is the reason the constant is not");
        _output.WriteLine("  a free scale. Upstream chose a number that puts a typical ratio on the");
        _output.WriteLine("  clamp, and this is the first time this tree has measured what that costs.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  NO LIBRARY FILE IS TOUCHED BY THIS TEST. 24.0f is upstream's number and it");
        _output.WriteLine("  stays where upstream put it.");

        // The row at upstream's own value must reproduce task 1's before-number, or the sweep is
        // measuring something other than the target.
        Assert.Equal(13, returned[atUpstream]);
    }

    /// <summary>
    /// The slot loop with the ratios normalised to a stated target rather than to the library's.
    /// </summary>
    private static Trial Run(
        IReadOnlyList<Ft8Candidate> candidates,
        Ft8Waterfall waterfall,
        float target,
        string expected,
        Unit223Arithmetic.Census census)
    {
        var cache = new Ft8CallsignCache();
        var seen = new List<byte[]>();
        var texts = new List<string>();

        var ratios = new float[Ft8SoftSymbols.RatioCount];
        var codeword = new byte[LdpcDecoder.CodewordBits];

        foreach (var candidate in candidates)
        {
            Ft8SoftSymbols.Extract(waterfall, candidate, ratios);
            NormaliseTo(ratios, target);

            var correction = Unit223Arithmetic.Decode(
                ratios, codeword, LdpcDecoder.DefaultMaxIterations,
                Unit223Arithmetic.Kind.Upstream, census);
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

    /// <summary>
    /// <see cref="Ft8SoftSymbols.Normalise"/> with the target as a parameter, and identical in every
    /// other term — the same single-precision variance, the same square root, the same multiply, the
    /// same guard on a degenerate variance.
    /// </summary>
    private static void NormaliseTo(Span<float> ratios, float target)
    {
        var variance = Ft8SoftSymbols.Variance(ratios);
        if (!(variance > 0.0f))
        {
            return;
        }

        var factor = MathF.Sqrt(target / variance);
        for (var i = 0; i < ratios.Length; i++)
        {
            ratios[i] *= factor;
        }
    }

    private sealed record Trial(bool Returned, string[] Wrong);
}
