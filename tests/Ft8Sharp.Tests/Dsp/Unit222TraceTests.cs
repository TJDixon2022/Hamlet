using System.Diagnostics;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>Unit 222 task 1: the before-number, and what the night can afford.</b> The -21 dB rung alone,
/// on unit 221's population, seeds and trial count, with nothing widened — so that every row of the
/// loss budget has one number to be a delta from.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this exists rather than re-reading unit 221's report.</b> The budget in
/// <see cref="Unit222BudgetTests"/> is a table of deltas, and a delta from a figure inherited from a
/// report rather than measured tonight is a delta from a rumour. Unit 221 proved its curve
/// byte-identical across two processes; if this rung reads anything other than <b>13 of 306</b>,
/// something moved between then and now and that is worth more than any budget taken on top of it.
/// </para>
/// <para>
/// <b>Nothing here is new apparatus.</b> The mix is <see cref="Ft8Step6Ladder"/>'s own inner loop
/// with the rung list collapsed to one entry, and the population, the seeds, the frequency and the
/// offset are all read from the same constants unit 221 committed before its curve ran.
/// </para>
/// </remarks>
public class Unit222TraceTests
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    /// <summary>Unit 221's frequency: 1000 / 6.25 is 160, exactly on a bin centre.</summary>
    internal const double OnGridHz = 1000.0;

    /// <summary>The rung the whole verdict is read at.</summary>
    internal const double VerdictRungDecibels = -21.0;

    private readonly ITestOutputHelper _output;

    public Unit222TraceTests(ITestOutputHelper output) => _output = output;

    /// <summary>Unit 221's offset: three whole symbol periods in.</summary>
    internal static int AlignedOffset => Ft8Waveform.SamplesPerSymbol(Rate) * 3;

    /// <summary>
    /// <b>The before-number.</b> The -21 dB rung on its own, reported as <c>n of N</c> with its
    /// Wilson interval and the mean ratio actually delivered to the samples.
    /// </summary>
    [Fact]
    public void TheRungTheVerdictIsReadAtReproducesUnitTwoTwentyOnesNumber()
    {
        var population = Ft8Step6Ladder.Population();
        var decoder = new Ft8SlotDecoder();
        var geometry = decoder.Geometry;
        var row = new Ft8Step6Ladder.Row(VerdictRungDecibels);
        var seeds = Ft8Step6Ladder.SeedsFor(VerdictRungDecibels);

        _output.WriteLine("UNIT 222 TASK 1 - THE BEFORE-NUMBER. The -21 dB rung alone, on unit 221's");
        _output.WriteLine("population, seeds and trial count, with nothing widened.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  rung          : {VerdictRungDecibels:F1} dB requested");
        _output.WriteLine($"  population    : {population.Count} messages");
        _output.WriteLine($"  noise draws   : {seeds}");
        _output.WriteLine($"  trials        : {population.Count * seeds}");
        _output.WriteLine($"  frequency     : {OnGridHz:F1} Hz, on a bin centre");
        _output.WriteLine($"  offset        : {AlignedOffset} samples");
        _output.WriteLine(string.Empty);

        var watch = Stopwatch.StartNew();

        for (var s = 0; s < seeds; s++)
        {
            var noise = new GaussianNoise(
                Ft8Step6Ladder.Seeds[s] + (int)Math.Round(VerdictRungDecibels * 10.0));

            foreach (var entry in population)
            {
                var (clean, _) = SearchFixture.OneSignal(Rate, entry, OnGridHz, AlignedOffset);
                var signalPower = SearchFixture.TransmissionPower(Rate, entry, OnGridHz);
                var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, VerdictRungDecibels, Rate);
                var mixed = SearchFixture.AddNoise(clean, noise, sigma, out var noisePower);
                var delivered = SignalToNoise.DecibelsFor(signalPower, noisePower, Rate);

                var waterfall = new Ft8Monitor(geometry).Analyse(mixed);
                var result = decoder.Decode(waterfall);

                var expected = Ft8MessageDecoder.Decode(entry.Message).Text;
                var returned = result.Texts.Contains(expected, StringComparer.Ordinal);
                var wrong = result.Texts
                    .Where(t => !string.Equals(t, expected, StringComparison.Ordinal))
                    .ToArray();

                row.Add(result, delivered, returned, wrong);
            }
        }

        watch.Stop();

        var (lower, upper) = row.Interval;

        _output.WriteLine(Ft8Step6Ladder.Header);
        _output.WriteLine(row.AsRow());
        _output.WriteLine(string.Empty);
        _output.WriteLine("THE BEFORE-NUMBER, STATED THE WAY THE BUDGET WILL QUOTE IT:");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  AT A DELIVERED {row.DeliveredMean:F3} dB : "
            + $"{row.Returned} OF {row.Trials}, {row.Rate:F1} PER CENT, "
            + $"95 PER CENT WILSON {lower:F1} TO {upper:F1}");
        _output.WriteLine($"  wrong messages                : {row.Wrong}");
        _output.WriteLine($"  worst delivery error          : {row.WorstDeliveryError:F4} dB");
        _output.WriteLine(string.Empty);
        _output.WriteLine("UNIT 221 MEASURED 13 OF 306 AT THIS RUNG. This run reads "
            + $"{row.Returned} of {row.Trials}.");
        _output.WriteLine(row.Returned == 13 && row.Trials == 306
            ? "  IT REPRODUCES. The budget stands on the same ground unit 221 measured."
            : "  IT DOES NOT REPRODUCE. Something moved between unit 221 and tonight, and that "
                + "is the finding rather than any budget taken on top of it.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("WHAT THE NIGHT CAN AFFORD, ARITHMETIC SHOWN:");
        _output.WriteLine(string.Empty);

        var perDecodeMs = watch.Elapsed.TotalMilliseconds / row.Trials;
        _output.WriteLine($"  this rung took                : {watch.Elapsed.TotalSeconds:F1} s "
            + $"for {row.Trials} slot decodes");
        _output.WriteLine($"  ONE SLOT DECODE               : {perDecodeMs:F1} ms");
        _output.WriteLine($"  ten minutes buys              : {600_000.0 / perDecodeMs:F0} slot decodes");
        _output.WriteLine($"  one budget row at this rung   : {row.Trials} decodes, "
            + $"{row.Trials * perDecodeMs / 1000.0:F1} s");
        _output.WriteLine($"  five rows at this rung        : {5 * row.Trials} decodes, "
            + $"{5 * row.Trials * perDecodeMs / 1000.0:F1} s");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  Rows B, C and D cost LESS than a full slot decode each, because they run");
        _output.WriteLine("  belief propagation over one candidate or over the kept list rather than");
        _output.WriteLine("  the whole path from samples, and rows C and D share one extra unquantised");
        _output.WriteLine("  analysis pass. Row E costs MORE, at four times the iteration bound.");

        // A measurement, not a gate. The rung is reported whatever it reads; the assertion below is
        // only that the instrument produced a rung at all.
        Assert.Equal(306, row.Trials);
        Assert.Equal(0, row.Wrong);
    }
}
