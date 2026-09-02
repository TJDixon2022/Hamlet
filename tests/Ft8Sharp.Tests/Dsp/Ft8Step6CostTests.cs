using System.Diagnostics;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>What one slot decode costs, in milliseconds, and therefore how many of them a night can
/// afford.</b> Unit 221's task 1 measurement, taken before the step 6 curve's trial counts were
/// chosen rather than discovered when the run was half done.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this is worth a test of its own.</b> Nineteen units built ladders and every one of them
/// chose its trial count by feel. Unit 218's ladder stands on 52 trials a rung, which cannot support
/// the word <em>comparable</em> at any rate near the collapse: 2 of 52 has a 95 per cent Wilson
/// interval running from about 1 per cent to about 13. The fix is more trials, and the number of
/// trials a night can carry is a measurement rather than a guess.
/// </para>
/// <para>
/// <b>The rung is mid-ladder on purpose.</b> A decode that succeeds and a decode that fails cost
/// different amounts — belief propagation stops early when the parity closes — so a cost taken at
/// -10 dB, where everything returns, would under-state a ladder whose bottom half returns nothing.
/// -20 dB is where unit 218 measured the rate at 25 per cent, so roughly one trial in four succeeds
/// and the mean is honest for the run it is sizing.
/// </para>
/// <para>
/// <b>Nothing here asserts a bound on the cost.</b> The figure is printed and the arithmetic with it.
/// The only assertion is that the measurement happened at all.
/// </para>
/// </remarks>
public class Ft8Step6CostTests
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;
    private const double OnGridHz = 1000.0;

    /// <summary>The rung the cost is taken at. Mid-collapse, so successes and failures both feature.</summary>
    private const double CostRungDecibels = -20.0;

    private readonly ITestOutputHelper _output;

    public Ft8Step6CostTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void OneSlotDecodeIsTimedSoTheTrialCountsAreChosenRatherThanDiscovered()
    {
        var messages = SensitivityLadder.Messages();
        var samplesPerSymbol = Ft8Waveform.SamplesPerSymbol(Rate);
        var alignedOffset = samplesPerSymbol * 3;
        var decoder = new Ft8SlotDecoder();
        var geometry = decoder.Geometry;

        // Two warm-up trials, discarded. The first decode in a process pays for the JIT and for the
        // transform's first allocation, and charging the night for that would over-state the cost.
        const int warmUp = 2;
        const int timed = 24;

        var elapsed = new List<double>();
        var returned = 0;

        for (var trial = 0; trial < warmUp + timed; trial++)
        {
            var entry = messages[trial % messages.Count];
            var noise = new GaussianNoise(221_900 + trial);

            var (clean, _) = SearchFixture.OneSignal(Rate, entry, OnGridHz, alignedOffset);
            var signalPower = SearchFixture.TransmissionPower(Rate, entry, OnGridHz);
            var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, CostRungDecibels, Rate);
            var mixed = SearchFixture.AddNoise(clean, noise, sigma, out _);

            var watch = Stopwatch.StartNew();
            var waterfall = new Ft8Monitor(geometry).Analyse(mixed);
            var result = decoder.Decode(waterfall);
            watch.Stop();

            if (trial < warmUp)
            {
                continue;
            }

            elapsed.Add(watch.Elapsed.TotalMilliseconds);
            if (result.Texts.Count > 0)
            {
                returned++;
            }
        }

        var mean = elapsed.Average();
        var worst = elapsed.Max();
        var best = elapsed.Min();

        _output.WriteLine("WHAT ONE SLOT DECODE COSTS, AND THEREFORE WHAT THE NIGHT CAN AFFORD.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  rung timed at            : {CostRungDecibels:F1} dB, mid-collapse");
        _output.WriteLine($"  warm-up trials discarded : {warmUp}");
        _output.WriteLine($"  timed trials             : {timed}");
        _output.WriteLine($"  of those, returning text : {returned}");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  MEAN COST PER SLOT DECODE: {mean:F1} ms");
        _output.WriteLine($"  fastest                  : {best:F1} ms");
        _output.WriteLine($"  slowest                  : {worst:F1} ms");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  THE ARITHMETIC, so the trial counts below are read rather than trusted:");

        foreach (var minutes in new[] { 5, 10, 20, 30 })
        {
            _output.WriteLine($"    {minutes,3} minutes = {minutes * 60_000.0 / mean,9:F0} slot decodes");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  WHAT THE FLOORS COST. Task 2d's floors are 200 trials on each of the nine");
        _output.WriteLine("  rungs from -16 to -24 and 100 on each of the five anchors, which is");
        _output.WriteLine("  9 x 200 + 5 x 100 = 2300 slot decodes for ONE pass of the curve, and the");
        _output.WriteLine("  curve is drawn TWICE in two separate processes for criterion 1.");
        _output.WriteLine($"    2300 slot decodes  : {2300 * mean / 60_000.0:F1} minutes");
        _output.WriteLine($"    4600, drawn twice  : {4600 * mean / 60_000.0:F1} minutes");

        Assert.Equal(timed, elapsed.Count);
        Assert.True(mean > 0.0, "a slot decode cannot cost nothing");
    }
}
