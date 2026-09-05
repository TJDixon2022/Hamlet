using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>The ladder with a handle on it, exercised: the three rungs this phase is measured on, the
/// reproduction of the figure <c>HM-OPEN-067</c> carries, and the determinism the whole thing rests
/// on.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>NO BOUND ON A DECODE RATE IS ASSERTED ANYWHERE IN THIS FILE.</b> The rates are printed and
/// read. Unit 212's rule on this project, and the rule that has caught every instrument defect in
/// this phase: a curve read first and judged afterwards is a rationalisation. The only assertions
/// here are about the <em>instrument</em> — that the same call twice gives the same answer, and that
/// the delivered ratio is the one that was asked for.
/// </para>
/// <para>
/// <b>What a later unit runs, and it is one line:</b>
/// </para>
/// <code>
///   dotnet test tests/Ft8Sharp.Tests --filter "FullyQualifiedName~Ft8LadderHarnessTests" \
///     --nologo -l "console;verbosity=detailed"
/// </code>
/// <para>
/// <b>What it costs is printed by every row</b>, in wall-clock seconds for the rung and milliseconds
/// for one trial, because every later unit of this phase pays it and a unit that does not know the
/// price plans badly.
/// </para>
/// </remarks>
public class Ft8LadderHarnessTests
{
    /// <summary>
    /// <b>306 is not a round number and is not a taste.</b> The population is 51 scoreable messages
    /// and the collapse rungs get six noise draws, so 306 is six whole blocks — and it is the trial
    /// count unit 221 measured 13 decodes at, which <c>HM-OPEN-067</c> carries and this phase's
    /// targets are quoted against.
    /// </summary>
    private const int BaselineTrials = 306;

    /// <summary>
    /// <b>The three rungs, and three rather than one on purpose.</b> A single point cannot say
    /// whether a curve moved or an axis did. -19 and -20 are unit 221's 81.0 and 23.9 per cent, and
    /// they are how a reproduction at -21 is told apart from a coincidence at -21.
    /// </summary>
    private static readonly double[] TheRungsThePhaseIsMeasuredOn = { -19.0, -20.0, -21.0 };

    private readonly ITestOutputHelper _output;

    public Ft8LadderHarnessTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// <b>The baseline: three rungs, 306 trials each, three counts each, and the wall clock.</b>
    /// </summary>
    [Fact]
    public void TheThreeRungsThePhaseIsMeasuredOnAreWalkedAndTheThreeCountsAreReported()
    {
        var decoders = Ft8LadderHarness.Available();

        _output.WriteLine("THE LADDER, WALKED THROUGH THE HARNESS ANY UNIT OF THIS PHASE CAN CALL.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  entry point   : Ft8LadderHarness.Run(rung, trials, seed)");
        _output.WriteLine($"  trials a rung : {BaselineTrials}, which is six whole blocks of the "
            + $"{Ft8Step6Ladder.Population().Count}-message population");
        _output.WriteLine($"  seed          : {Ft8LadderHarness.DefaultSeed}, which reproduces "
            + "Ft8Step6Ladder.Walk exactly");
        _output.WriteLine($"  frequency     : {Ft8LadderHarness.DefaultFrequencyHz:F2} Hz, on a bin centre");
        _output.WriteLine($"  offset        : {Ft8LadderHarness.DefaultOffsetSamples} samples, on the block grid");
        _output.WriteLine($"  decoders      : {string.Join(", ", decoders.Select(d => d.Name))}");

        if (decoders.Count == 1)
        {
            _output.WriteLine("                  Ft8Sharp.Deep DOES NOT EXIST YET - step 1 of PHASE_PLAN.md");
            _output.WriteLine("                  creates it. Ft8LadderHarness.Available() is where it joins,");
            _output.WriteLine("                  and every trial then runs BOTH over the SAME samples.");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  THE FIGURES THIS IS READ AGAINST, from HM-OPEN-067 and units 221-223,");
        _output.WriteLine("  written here before the run rather than fitted to it:");
        _output.WriteLine("      -19 dB : 248 of 306, 81.0 per cent, 0 wrong");
        _output.WriteLine("      -20 dB :  73 of 306, 23.9 per cent, 0 wrong");
        _output.WriteLine("      -21 dB :  13 of 306,  4.2 per cent, 0 wrong, interval 2.5 to 7.1");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  NOTHING BELOW IS ADJUSTED TO MEET THEM. A disagreement is the finding.");
        _output.WriteLine(string.Empty);

        var all = new List<Ft8LadderHarness.Result>();

        foreach (var rung in TheRungsThePhaseIsMeasuredOn)
        {
            all.AddRange(Ft8LadderHarness.Run(rung, BaselineTrials, decoders: decoders));
        }

        foreach (var line in Ft8LadderHarness.Report(all))
        {
            _output.WriteLine(line);
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("REQUESTED AGAINST DELIVERED, PRINTED BEFORE ANY BOUND IS ASSERTED:");

        foreach (var result in all)
        {
            _output.WriteLine($"  {result.Decoder} at {result.Requested,6:F1} dB : delivered "
                + $"{result.DeliveredMean:F4}, worst trial error {result.WorstDeliveryError:F4} dB");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("WHAT A 306-TRIAL RUNG COSTS, because every later unit pays it:");

        foreach (var result in all)
        {
            _output.WriteLine($"  {result.Decoder} at {result.Requested,6:F1} dB : "
                + $"{result.Elapsed.TotalSeconds:F1} s, {result.MillisecondsPerTrial:F1} ms a trial");
        }

        // The only bound in this file, and it is on the INSTRUMENT rather than on the receiver: the
        // ratio put on the samples has to be the ratio that was asked for, or every rate above is
        // labelled with the wrong number. Unit 221 measured this at hundredths of a decibel.
        foreach (var result in all)
        {
            Assert.True(
                Math.Abs(result.DeliveredMean - result.Requested) < 0.2,
                $"the harness delivered {result.DeliveredMean:F4} dB at a requested "
                + $"{result.Requested:F1} dB, so the axis and not the receiver is what moved");
        }
    }

    /// <summary>
    /// <b>The same call twice gives the same answer</b> — the property every measurement in this
    /// phase is quoted as a number rather than a range on.
    /// </summary>
    /// <remarks>
    /// One block of the population at a rung well inside the collapse, so the answer is neither all
    /// decodes nor none and a difference would actually show.
    /// </remarks>
    [Fact]
    public void TheSameRungWalkedTwiceGivesTheSameThreeCounts()
    {
        const double rung = -19.0;
        var trials = Ft8Step6Ladder.Population().Count;

        var first = Ft8LadderHarness.Run(rung, trials).Single();
        var second = Ft8LadderHarness.Run(rung, trials).Single();

        _output.WriteLine($"one block of {trials} trials at {rung:F1} dB, walked twice:");
        _output.WriteLine(Ft8LadderHarness.Header);
        _output.WriteLine(first.AsRow());
        _output.WriteLine(second.AsRow());

        Assert.Equal(first.Decoded, second.Decoded);
        Assert.Equal(first.Missed, second.Missed);
        Assert.Equal(first.Wrong, second.Wrong);
        Assert.Equal(first.DeliveredMean, second.DeliveredMean, 12);
        Assert.Equal(first.Trials, first.Decoded + first.Missed);
    }

    /// <summary>
    /// <b>A different seed is a different draw and the harness does not quietly ignore it.</b>
    /// </summary>
    /// <remarks>
    /// The check is on the noise, not on the rate: two seeds can land on the same count by chance,
    /// but they cannot land on the same delivered mean to twelve places.
    /// </remarks>
    [Fact]
    public void ADifferentSeedIsADifferentDraw()
    {
        const double rung = -19.0;
        var trials = Ft8Step6Ladder.Population().Count;

        var asPublished = Ft8LadderHarness.Run(rung, trials).Single();
        var elsewhere = Ft8LadderHarness.Run(rung, trials, seed: 243_001).Single();

        _output.WriteLine(Ft8LadderHarness.Header);
        _output.WriteLine(asPublished.AsRow());
        _output.WriteLine(elsewhere.AsRow());

        Assert.NotEqual(asPublished.DeliveredMean, elsewhere.DeliveredMean, 12);
    }
}
