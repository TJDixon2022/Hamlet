using System.Diagnostics;
using Ft8Sharp.Deep;
using Ft8Sharp.Dsp;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>What each ordered statistics order buys and what it costs, measured on the scoreboard at
/// -21 dB.</b> Step 2's fourth must-pass exit, and it says <em>measured</em>, not tuned to a target.
/// </summary>
/// <remarks>
/// <para>
/// <b>Every row sees the same seed and the same noise draw.</b> <c>Ft8LadderHarness.Run</c>
/// synthesises the audio once per trial and hands the same array to every decoder in its list, so
/// this is a paired comparison: where two rows differ, the difference is the order and nothing else.
/// </para>
/// <para>
/// <b>Nothing here is tuned to hit a number.</b> The rows are run, the table is printed, and the
/// default is then read off the table. A figure reached by trying settings until one passed is not a
/// measurement, and this phase would carry it forward as though it were.
/// </para>
/// <para>
/// <b>The one assertion that bites is zero wrong decodes.</b> A wrong decode is worse than a missed
/// one - <c>CLAUDE.md</c> §12.1 and §0.0 - and an order that produces one is an order this phase
/// rejects whatever it did to the rate.
/// </para>
/// </remarks>
public class Ft8Unit246OrderTests(ITestOutputHelper output)
{
    private const double Rung = -21.0;

    /// <summary>One row of the table: a decoder, and what its OSD stage did behind it.</summary>
    private sealed class Row(string name, Ft8DeepSlotDecoder? decoder)
    {
        internal string Name { get; } = name;

        internal Ft8DeepSlotDecoder? Decoder { get; } = decoder;

        internal int Offered { get; private set; }

        internal int Produced { get; private set; }

        internal int Accepted { get; private set; }

        internal long Reencodings { get; private set; }

        internal double WorstSlotMilliseconds { get; private set; }

        internal Ft8SlotResult Run(float[] samples)
        {
            var clock = Stopwatch.StartNew();
            var result = Decoder is null
                ? Port.Decode(samples)
                : Decoder.Decode(samples);
            clock.Stop();

            if (clock.Elapsed.TotalMilliseconds > WorstSlotMilliseconds)
            {
                WorstSlotMilliseconds = clock.Elapsed.TotalMilliseconds;
            }

            if (Decoder is not null)
            {
                var counts = Decoder.LastOsd;
                Offered += counts.Offered;
                Produced += counts.Produced;
                Accepted += counts.Accepted;
                Reencodings += counts.Reencodings;
            }

            return result;
        }

        internal Ft8SlotDecoder Port { get; } = new();
    }

    /// <summary>
    /// <b>Order 0, 1, 2 and 3 at -21 dB over one whole 51-trial block, with the port and OSD off
    /// beside them.</b>
    /// </summary>
    /// <remarks>
    /// Order 3 is past what step 2's exit asks for and is run anyway, because unit 246's task 1
    /// measured the ceiling admitting 10 of 51 trials at order 3 against 7 at order 2, and an order
    /// that buys something at a cost nobody can pay is still worth knowing about by its cost.
    /// </remarks>
    [Fact]
    public void WhatEachOrderBuysAndWhatItCostsAtMinus21Db()
    {
        var rows = new List<Row>
        {
            new("Ft8Sharp", null),
            new("Deep OSD off", new Ft8DeepSlotDecoder()),
            new("Deep order 0", new Ft8DeepSlotDecoder(osd: new Ft8DeepOsdSettings(0))),
            new("Deep order 1", new Ft8DeepSlotDecoder(osd: new Ft8DeepOsdSettings(1))),
            new("Deep order 2", new Ft8DeepSlotDecoder(osd: new Ft8DeepOsdSettings(2))),
            new("Deep order 3", new Ft8DeepSlotDecoder(osd: new Ft8DeepOsdSettings(3))),
        };

        var decoders = rows
            .Select(row => new Ft8LadderHarness.Decoder(row.Name, row.Run))
            .ToArray();

        var trials = Ft8Step6Ladder.Population().Count;
        var results = Ft8LadderHarness.Run(Rung, trials, decoders: decoders);

        foreach (var line in Ft8LadderHarness.Report(results))
        {
            output.WriteLine(line);
        }

        output.WriteLine(string.Empty);
        output.WriteLine("WHAT EACH ORDER BOUGHT AND WHAT IT COST");
        output.WriteLine(
            "row           DECODED  MISSED  WRONG    ms/tr   worst ms   offered  produced  accepted"
            + "   re-encodings");

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var result = results[i];
            output.WriteLine(
                $"{row.Name,-12} {result.Decoded,8} {result.Missed,7} {result.Wrong,6} "
                + $"{result.MillisecondsPerTrial,8:F1} {row.WorstSlotMilliseconds,10:F1} "
                + $"{row.Offered,9} {row.Produced,9} {row.Accepted,9} {row.Reencodings,14}");
        }

        output.WriteLine(string.Empty);
        var baseline = results[0].Decoded;
        for (var i = 2; i < rows.Count; i++)
        {
            var bought = results[i].Decoded - baseline;
            var cost = results[i].MillisecondsPerTrial - results[0].MillisecondsPerTrial;
            output.WriteLine(
                $"{rows[i].Name}: bought {bought:+0;-0;0} decode(s) of {trials} over the port, cost "
                + $"{cost:F1} ms a trial, {rows[i].Accepted} codeword(s) accepted by the port's own "
                + "gates.");
        }

        output.WriteLine(string.Empty);
        output.WriteLine(
            "A row that bought nothing is reported as buying nothing. The order the default is set to");
        output.WriteLine(
            "is read off this table, not chosen from a paper and not chosen to hit a target - see the");
        output.WriteLine(
            $"remarks on Ft8DeepOsdSettings.Default, which is order {Ft8DeepOsdSettings.Default.Order}.");

        // OSD off must equal the port, or every other row's difference is unattributable.
        Assert.Equal(results[0].Decoded, results[1].Decoded);
        Assert.Equal(results[0].Missed, results[1].Missed);
        Assert.Equal(results[0].Wrong, results[1].Wrong);

        foreach (var result in results)
        {
            Assert.True(
                result.Wrong == 0,
                $"{result.Decoder} returned {result.Wrong} message(s) that were not sent at "
                    + $"{Rung:F1} dB. A wrong decode is worse than a missed one, so this approach is "
                    + "rejected rather than reported as a rate.");
        }
    }
}
