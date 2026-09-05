using System.Diagnostics;
using Ft8Sharp.Deep;
using Ft8Sharp.Dsp;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>The order-and-window grid, priced at 51 trials before anything is spent at 306.</b> Step 3's
/// third must-pass exit asks for <em>order and search weight stated with the cost each buys,
/// measured</em>, and this is the measurement the cell taken to the scoreboard is chosen off.
/// </summary>
/// <remarks>
/// <para>
/// <b>THE BREAKAGE THIS WOULD HAVE CAUGHT, because no test is added to this tree without naming
/// one.</b> Unit 246 chose order 2 as the default off a table of orders, and wrote on
/// <c>Ft8DeepOsdSettings.Default</c> that <em>order 3 is not ruled out</em> at 246 ms a trial more.
/// Unit 252 makes a higher order affordable by searching only the unreliable end of the basis - and
/// <b>a search restricted to a window that the errors do not in fact fall in would cost less and
/// decode less, and would look exactly like a cheaper order</b>. Nothing in the tree measured a
/// windowed cell against a full-basis one on the same audio; the cost tests pin the price and say
/// nothing at all about what it buys. Without this, unit 252 would have shipped a window chosen on
/// arithmetic and reported a price nobody checked against a decode count.
/// </para>
/// <para>
/// <b>Every row sees the same seed and the same noise draw.</b> <c>Ft8LadderHarness.Run</c>
/// synthesises the audio once per trial and hands the same array to every decoder in its list, so
/// where two rows differ the difference is the <c>(order, window)</c> cell and nothing else.
/// </para>
/// <para>
/// <b>51 trials cannot separate one decode from another and this test does not pretend
/// otherwise.</b> Unit 246 recorded exactly that and chose order 2 on cost. The output here is a
/// price list with a decode count beside it, and the sentence that reads a cell off it says which
/// of the two it was chosen on.
/// </para>
/// <para>
/// <b>The one assertion that bites is zero wrong decodes</b> - <c>CLAUDE.md</c> §12.1 and §0.0 - and
/// a cell that returns one is struck from the scoreboard whatever it did to the rate.
/// </para>
/// </remarks>
public class Ft8Unit252GridTests(ITestOutputHelper output)
{
    private const double Rung = -21.0;

    private const int FullBasis = Ft8DeepOsdSettings.FullBasis;

    /// <summary>One row of the grid, and what its OSD stage did behind it.</summary>
    private sealed class Row(string name, Ft8DeepSlotDecoder? decoder, Ft8SlotDecoder? port)
    {
        internal string Name { get; } = name;

        internal Ft8DeepSlotDecoder? Decoder { get; } = decoder;

        internal int Order { get; init; }

        internal int Window { get; init; }

        internal long PerCandidate { get; init; }

        internal int Offered { get; private set; }

        internal int Accepted { get; private set; }

        internal long Reencodings { get; private set; }

        internal double WorstSlotMilliseconds { get; private set; }

        internal Ft8SlotResult Run(float[] samples)
        {
            var clock = Stopwatch.StartNew();
            var result = Decoder is null ? port!.Decode(samples) : Decoder.Decode(samples);
            clock.Stop();

            if (clock.Elapsed.TotalMilliseconds > WorstSlotMilliseconds)
            {
                WorstSlotMilliseconds = clock.Elapsed.TotalMilliseconds;
            }

            if (Decoder is not null)
            {
                var counts = Decoder.LastOsd;
                Offered += counts.Offered;
                Accepted += counts.Accepted;
                Reencodings += counts.Reencodings;
            }

            return result;
        }
    }

    /// <summary><c>1 + sum over i = 1..order of C(window, i)</c>, what one candidate costs.</summary>
    private static long SubsetCount(int order, int window)
    {
        var total = 1L;
        var term = 1L;

        for (var i = 1; i <= order; i++)
        {
            term = term * (window - i + 1) / i;
            total += term;
        }

        return total;
    }

    private static Row Cell(int order, int window)
    {
        var name = window == FullBasis ? $"o{order} full" : $"o{order} W{window}";
        return new Row(
            name,
            new Ft8DeepSlotDecoder(osd: new Ft8DeepOsdSettings(order, window)),
            null)
        {
            Order = order,
            Window = window,
            PerCandidate = SubsetCount(order, window),
        };
    }

    /// <summary>
    /// <b>Eleven cells at -21 dB over one whole 51-trial block, every row on the same noise
    /// draw.</b>
    /// </summary>
    [Fact]
    public void TheOrderAndWindowGridAtMinus21DbOverOneBlock()
    {
        var port = new Ft8SlotDecoder();

        var rows = new List<Row>
        {
            new("Ft8Sharp", null, port),
            new("Deep OSD off", new Ft8DeepSlotDecoder(), null),
            Cell(2, FullBasis),
            Cell(2, 40),
            Cell(3, 20),
            Cell(3, 30),
            Cell(3, 40),
            Cell(3, 60),
            Cell(3, FullBasis),
            Cell(4, 20),
            Cell(4, 30),
        };

        var decoders = rows
            .Select(row => new Ft8LadderHarness.Decoder(row.Name, row.Run))
            .ToArray();

        var trials = Ft8Step6Ladder.Population().Count;

        output.WriteLine(
            $"THE ORDER-AND-WINDOW GRID at {Rung:F1} dB over {trials} trials, one whole block.");
        output.WriteLine(
            "The window is how many of the LEAST RELIABLE basis positions the flips may fall in.");
        output.WriteLine(
            "'full' is all 91, which is what shipped before unit 252 and what row 3 measures.");
        output.WriteLine(string.Empty);

        var clock = Stopwatch.StartNew();
        var results = Ft8LadderHarness.Run(Rung, trials, decoders: decoders);
        clock.Stop();

        output.WriteLine(Ft8LadderHarness.Header);
        foreach (var result in results)
        {
            output.WriteLine(result.AsRow());
        }

        output.WriteLine(string.Empty);
        output.WriteLine("WHAT EACH CELL COST AND WHAT IT BOUGHT");
        output.WriteLine(
            "row           order  window   per cand  DECODED  MISSED  WRONG    ms/tr   worst ms"
            + "   offered  accepted    re-encodings");

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var result = results[i];
            var order = row.Decoder?.Osd is null ? "-" : row.Order.ToString();
            var window = row.Decoder?.Osd is null
                ? "-"
                : row.Window == FullBasis ? "full" : row.Window.ToString();
            var perCandidate = row.Decoder?.Osd is null ? "-" : $"{row.PerCandidate:N0}";

            output.WriteLine(
                $"{row.Name,-12} {order,6} {window,7} {perCandidate,10} {result.Decoded,8} "
                + $"{result.Missed,7} {result.Wrong,6} {result.MillisecondsPerTrial,8:F1} "
                + $"{row.WorstSlotMilliseconds,10:F1} {row.Offered,9} {row.Accepted,9} "
                + $"{row.Reencodings,15:N0}");
        }

        output.WriteLine(string.Empty);
        output.WriteLine("AGAINST THE PORT, AND AGAINST WHAT SHIPS TODAY:");
        var portDecoded = results[0].Decoded;
        var portMilliseconds = results[0].MillisecondsPerTrial;
        var shippingDecoded = results[2].Decoded;
        var shippingMilliseconds = results[2].MillisecondsPerTrial;

        for (var i = 2; i < rows.Count; i++)
        {
            output.WriteLine(
                $"{rows[i].Name,-12}: {results[i].Decoded - portDecoded:+0;-0;0} of {trials} over the "
                + $"port at {results[i].MillisecondsPerTrial - portMilliseconds:+0.0;-0.0;0.0} ms a "
                + $"trial; {results[i].Decoded - shippingDecoded:+0;-0;0} over what ships at "
                + $"{results[i].MillisecondsPerTrial - shippingMilliseconds:+0.0;-0.0;0.0} ms a "
                + $"trial; {rows[i].Accepted} codeword(s) the port's own gates then accepted.");
        }

        output.WriteLine(string.Empty);
        output.WriteLine(
            $"whole grid wall clock {clock.Elapsed.TotalSeconds:F1} s for {rows.Count} rows over "
            + $"{trials} trials.");
        output.WriteLine(
            "The synthesis is outside every decoder's stopwatch, so the sum of the ms/tr column is");
        output.WriteLine("less than the wall clock and the difference is the harness's own cost.");

        output.WriteLine(string.Empty);
        output.WriteLine(
            "51 trials cannot separate one decode from another - unit 246 recorded exactly that and");
        output.WriteLine(
            "chose order 2 on cost. If these cells cannot be separated here, the cell taken to 306");
        output.WriteLine("trials is chosen on price, and unit 252's report says which it was.");

        // OSD off must equal the port, or every other row's difference is unattributable.
        Assert.Equal(results[0].Decoded, results[1].Decoded);
        Assert.Equal(results[0].Missed, results[1].Missed);
        Assert.Equal(results[0].Wrong, results[1].Wrong);

        // The re-encoding count is the cell's own arithmetic times the candidates it was offered,
        // on real audio rather than on synthesised ratios. A window reported but not honoured shows
        // here as a count that did not move.
        for (var i = 2; i < rows.Count; i++)
        {
            Assert.Equal(rows[i].Offered * rows[i].PerCandidate, rows[i].Reencodings);
        }

        foreach (var result in results)
        {
            foreach (var wrong in result.WrongReturns)
            {
                output.WriteLine($"WRONG: {wrong}");
            }

            Assert.True(
                result.Wrong == 0,
                $"{result.Decoder} returned {result.Wrong} message(s) that were not sent at "
                    + $"{Rung:F1} dB. A wrong decode is worse than a missed one, so this cell is "
                    + "struck from the scoreboard rather than reported as a rate.");
        }
    }
}
