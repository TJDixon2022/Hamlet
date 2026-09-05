using System.Diagnostics;
using Ft8Sharp.Deep;
using Ft8Sharp.Dsp;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>The scoreboard, whole: three rungs, 306 trials each, three columns, three counts on every
/// row.</b> Nothing this phase claims is claimed without it.
/// </summary>
/// <remarks>
/// <para>
/// <b>The middle column is what makes the third one mean anything.</b> <c>Ft8Sharp.Deep</c> with OSD
/// off runs the port's per-candidate loop itself, and it must return the same three counts as the
/// port on every rung. Where it does, a difference between the second and third columns is
/// attributable to ordered statistics decoding and to nothing else - not to the seam, not to the
/// harness wiring, not to a reproduction that quietly diverged.
/// </para>
/// <para>
/// <b>Zero wrong decodes across all three rungs and all three columns, or the approach is
/// rejected.</b> <c>PHASE_PLAN.md</c> step 2's third must-pass exit and <c>CLAUDE.md</c> §12.1: a
/// decode this phase produces that nobody sent is worse than a decode it misses. Every wrong return
/// is printed with the message sent beside the message returned.
/// </para>
/// <para>
/// <b>This is a long measurement and it is a measurement rather than a smoke test.</b> 918 trials
/// through three decoders is several minutes of wall clock, which is the price of the one number the
/// phase reads.
/// </para>
/// </remarks>
public class Ft8Unit246ScoreboardTests(ITestOutputHelper output)
{
    /// <summary>306 trials: six whole blocks of the 51-message population, which is the count
    /// <c>HM-OPEN-067</c>'s 13 of 306 was taken at.</summary>
    private const int Trials = 306;

    /// <summary>One column, with the worst single slot it saw and what that slot carried.</summary>
    private sealed class Column(string name, Func<float[], Ft8SlotResult> decode)
    {
        internal string Name { get; } = name;

        internal double WorstSlotMilliseconds { get; private set; }

        internal int WorstSlotCandidates { get; private set; }

        internal int Offered { get; private set; }

        internal int Accepted { get; private set; }

        internal long Reencodings { get; private set; }

        internal Ft8DeepSlotDecoder? Deep { get; init; }

        internal Ft8SlotResult Run(float[] samples)
        {
            var clock = Stopwatch.StartNew();
            var result = decode(samples);
            clock.Stop();

            if (clock.Elapsed.TotalMilliseconds > WorstSlotMilliseconds)
            {
                WorstSlotMilliseconds = clock.Elapsed.TotalMilliseconds;
                WorstSlotCandidates = result.CandidateCount;
            }

            if (Deep is not null)
            {
                Offered += Deep.LastOsd.Offered;
                Accepted += Deep.LastOsd.Accepted;
                Reencodings += Deep.LastOsd.Reencodings;
            }

            return result;
        }
    }

    /// <summary>
    /// <b>THE ONE NUMBER THE PHASE READS: the decode rate at -21 dB over 306 trials, with its Wilson
    /// interval and its wrong count, against 4.2 per cent (13 of 306) as-is.</b>
    /// </summary>
    [Fact]
    public void TheWholeLadderThroughThreeColumns()
    {
        var port = new Ft8SlotDecoder();
        var off = new Ft8DeepSlotDecoder();
        var on = new Ft8DeepSlotDecoder(osd: Ft8DeepOsdSettings.Default);

        var columns = new[]
        {
            new Column("Ft8Sharp", samples => port.Decode(samples)),
            new Column("Deep OSD off", samples => off.Decode(samples)) { Deep = off },
            new Column("Deep OSD on", samples => on.Decode(samples)) { Deep = on },
        };

        var decoders = columns
            .Select(column => new Ft8LadderHarness.Decoder(column.Name, column.Run))
            .ToArray();

        Assert.Null(off.Osd);
        Assert.NotNull(on.Osd);

        output.WriteLine(
            $"THE SCOREBOARD, WHOLE. Three rungs, {Trials} trials each, three columns.");
        output.WriteLine(
            $"The OSD-on column runs at order {on.Osd!.Order}, which is "
            + "Ft8DeepOsdSettings.Default and was read off unit 246 task 6's table.");
        output.WriteLine(string.Empty);

        var clock = Stopwatch.StartNew();
        var atMinus21 = new List<Ft8LadderHarness.Result>();

        foreach (var rung in new[] { -19.0, -20.0, -21.0 })
        {
            var results = Ft8LadderHarness.Run(rung, Trials, decoders: decoders);

            foreach (var line in Ft8LadderHarness.Report(results))
            {
                output.WriteLine(line);
            }

            output.WriteLine(string.Empty);

            // The reproduction has to hold at every rung. If it does not, the third column is
            // measuring the reproduction as well as the OSD and is not evidence for anything.
            Assert.True(
                results[0].Decoded == results[1].Decoded
                    && results[0].Missed == results[1].Missed
                    && results[0].Wrong == results[1].Wrong,
                $"at {rung:F1} dB the port returned {results[0].Decoded}/{results[0].Missed}/"
                    + $"{results[0].Wrong} and the OSD-off reproduction returned "
                    + $"{results[1].Decoded}/{results[1].Missed}/{results[1].Wrong}. A difference "
                    + "between the second and third columns would no longer be attributable to OSD.");

            foreach (var result in results)
            {
                Assert.True(
                    result.Wrong == 0,
                    $"{result.Decoder} at {rung:F1} dB returned {result.Wrong} message(s) that were "
                        + "not sent. A wrong decode is worse than a missed one, so this approach is "
                        + "rejected rather than reported as a rate.");
            }

            if (rung == -21.0)
            {
                atMinus21.AddRange(results);
            }
        }

        clock.Stop();

        output.WriteLine("THE TIME BUDGET, worst single slot observed rather than the mean:");
        output.WriteLine("column          worst slot ms   its candidates   margin against 15 s");
        foreach (var column in columns)
        {
            output.WriteLine(
                $"{column.Name,-14} {column.WorstSlotMilliseconds,13:F1} {column.WorstSlotCandidates,16} "
                + $"{15000.0 / column.WorstSlotMilliseconds,21:F0}x");
        }

        output.WriteLine(string.Empty);
        output.WriteLine("WHAT THE OSD STAGE ITSELF DID, across all three rungs:");
        foreach (var column in columns)
        {
            if (column.Deep?.Osd is null)
            {
                continue;
            }

            output.WriteLine(
                $"{column.Name}: {column.Offered} candidates offered, {column.Accepted} codewords "
                + $"the PORT then accepted, {column.Reencodings} re-encodings spent.");
        }

        output.WriteLine(string.Empty);
        output.WriteLine("THE ONE NUMBER, at -21 dB over 306 trials:");
        foreach (var result in atMinus21)
        {
            var (lower, upper) = result.Interval;
            output.WriteLine(
                $"  {result.Decoder,-14} {result.Rate,5:F1} per cent ({result.Decoded} of "
                + $"{result.Trials}), 95 per cent Wilson {lower:F1} to {upper:F1}, "
                + $"{result.Wrong} WRONG");
        }

        output.WriteLine(string.Empty);
        output.WriteLine("  as-is going in: 4.2 per cent (13 of 306), 0 wrong.");
        output.WriteLine($"  whole run wall clock {clock.Elapsed.TotalMinutes:F1} minutes.");

        Assert.Equal(3, atMinus21.Count);

        foreach (var column in columns)
        {
            Assert.True(
                column.WorstSlotMilliseconds < 15000.0,
                $"{column.Name}'s worst slot took {column.WorstSlotMilliseconds:F0} ms, which is not "
                    + "inside FT8's 15 seconds. The decoder would not keep up with the air.");
        }
    }
}
