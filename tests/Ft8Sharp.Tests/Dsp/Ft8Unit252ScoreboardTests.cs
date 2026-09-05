using System.Diagnostics;
using Ft8Sharp.Deep;
using Ft8Sharp.Dsp;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>The scoreboard at 306 trials with the window on it: five columns, one rung per test method,
/// before and after, isolated from every other stage.</b> Step 3's first must-pass exit.
/// </summary>
/// <remarks>
/// <para>
/// <b>THE BREAKAGE THIS WOULD HAVE CAUGHT.</b> Unit 246's scoreboard has three columns and no way
/// to add a fourth without changing it, and every figure this phase quotes at -21 dB - 13 of 306,
/// 33 of 306, the -19.81 dB crossing - came out of it. **A new decoder setting measured against
/// numbers copied out of a document rather than re-run beside it is a comparison across two runs of
/// two different binaries**, and unit 248's own §4.1 is the demonstration that this matters: it
/// re-ran both control columns and only then could say the instrument had not moved. Unit 252
/// changes the enumeration inside `Ft8DeepOrderedStatistics.Search`, which every existing figure
/// went through. Without the port and the OSD-off and the order-2 columns re-run on the same audio
/// on the same night, a window that quietly cost the default path a decode would be invisible and
/// would be reported as the window's gain.
/// </para>
/// <para>
/// <b>ONE RUNG PER TEST METHOD, and that is the licensed answer to a measurement too big for one
/// call.</b> HM-DEC-155 forbids backgrounding and polling, and the watchdog fires at twelve minutes
/// of silence. Three rungs in one method would approach it; three methods run one at a time by exact
/// name do not. <b>Splitting is not shrinking</b> - every rung is 306 trials, six whole blocks of
/// the 51-message population, which is the count every recorded figure in this phase was taken at.
/// </para>
/// <para>
/// <b>FINE SYNC IS OFF ON EVERY COLUMN OF THE ISOLATION.</b> Step 3's first exit says <em>separately
/// from every other stage</em>. The shipping configuration - fine sync on, which is what
/// <c>Ft8Reception</c> builds - is measured in its own method at -21 dB and is labelled as not part
/// of the isolation.
/// </para>
/// <para>
/// <b>Nothing here asserts a bound on the rate.</b> No target, no 40 per cent, no floor. A rung that
/// returns nothing is a measurement. The two assertions that bite are <b>zero wrong decodes on every
/// row</b> and <b>the OSD-off column equalling the port column</b>, and the second is what makes
/// every other column attributable.
/// </para>
/// </remarks>
public class Ft8Unit252ScoreboardTests(ITestOutputHelper output)
{
    /// <summary>306 trials: six whole blocks of the 51-message population.</summary>
    private const int Trials = 306;

    private const int FullBasis = Ft8DeepOsdSettings.FullBasis;

    /// <summary><b>The cell task 3 chose</b>: order 3 over the least reliable 60 of the basis.</summary>
    private const int AfterOrder = 3;

    private const int AfterWindow = 60;

    /// <summary>One column, with the worst single slot it saw and what its OSD stage did.</summary>
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

    private static Column Deep(string name, Ft8DeepOsdSettings? osd, Ft8DeepFineSyncSettings? fine)
    {
        var decoder = new Ft8DeepSlotDecoder(osd: osd, fineSync: fine);
        return new Column(name, samples => decoder.Decode(samples)) { Deep = decoder };
    }

    /// <summary>
    /// The five isolation columns, in the order the report prints them: the port, the OSD-off
    /// reproduction, the <b>before</b>, the <b>after</b>, and the full-basis order 3 unit 246 left
    /// unresolved. <b>Fine sync off on all five.</b>
    /// </summary>
    private static Column[] Isolation()
    {
        var port = new Ft8SlotDecoder();
        return
        [
            new Column("Ft8Sharp", samples => port.Decode(samples)),
            Deep("Deep OSD off", null, null),
            Deep("o2 full", new Ft8DeepOsdSettings(2, FullBasis), null),
            Deep($"o{AfterOrder} W{AfterWindow}", new Ft8DeepOsdSettings(AfterOrder, AfterWindow), null),
            Deep("o3 full", new Ft8DeepOsdSettings(3, FullBasis), null),
        ];
    }

    /// <summary>Walks one rung through a set of columns and reports every count on every row.</summary>
    private void Walk(double rung, Column[] columns, bool isolation)
    {
        var decoders = columns
            .Select(column => new Ft8LadderHarness.Decoder(column.Name, column.Run))
            .ToArray();

        output.WriteLine(
            $"UNIT 252 SCOREBOARD, {rung:F1} dB, {Trials} trials, {columns.Length} columns.");
        output.WriteLine(
            isolation
                ? "Fine sync OFF on every column. OSD is the only variable, which is what step 3's "
                    + "first exit means by separately from every other stage."
                : "NOT PART OF THE ISOLATION. Fine sync ON, which is what Ft8Reception ships, so "
                    + "this says what the operator actually gets rather than what the isolation says.");
        output.WriteLine(string.Empty);

        var clock = Stopwatch.StartNew();
        var results = Ft8LadderHarness.Run(rung, Trials, decoders: decoders);
        clock.Stop();

        output.WriteLine(Ft8LadderHarness.Header);
        foreach (var result in results)
        {
            output.WriteLine(result.AsRow());
        }

        output.WriteLine(string.Empty);
        output.WriteLine("THE TIME BUDGET AND WHAT THE OSD STAGE DID, on this rung:");
        output.WriteLine(
            "column         worst slot ms   its candidates   margin vs 15 s   offered  accepted"
            + "     re-encodings");

        foreach (var column in columns)
        {
            output.WriteLine(
                $"{column.Name,-14} {column.WorstSlotMilliseconds,13:F1} "
                + $"{column.WorstSlotCandidates,16} {15000.0 / column.WorstSlotMilliseconds,15:F0}x "
                + $"{column.Offered,9} {column.Accepted,9} {column.Reencodings,16:N0}");
        }

        output.WriteLine(string.Empty);
        output.WriteLine($"rung wall clock {clock.Elapsed.TotalSeconds:F1} s.");

        if (isolation)
        {
            // The reproduction has to hold at every rung, or the columns beyond it are measuring
            // the reproduction as well as the OSD and are not evidence for anything.
            Assert.True(
                results[0].Decoded == results[1].Decoded
                    && results[0].Missed == results[1].Missed
                    && results[0].Wrong == results[1].Wrong,
                $"at {rung:F1} dB the port returned {results[0].Decoded}/{results[0].Missed}/"
                    + $"{results[0].Wrong} and the OSD-off reproduction returned "
                    + $"{results[1].Decoded}/{results[1].Missed}/{results[1].Wrong}. A difference "
                    + "between the later columns would no longer be attributable to OSD.");
        }

        foreach (var result in results)
        {
            foreach (var wrong in result.WrongReturns)
            {
                output.WriteLine($"WRONG on {result.Decoder}: {wrong}");
            }

            Assert.True(
                result.Wrong == 0,
                $"{result.Decoder} at {rung:F1} dB returned {result.Wrong} message(s) that were not "
                    + "sent. A wrong decode is worse than a missed one, so this approach is "
                    + "rejected rather than reported as a rate.");
        }

        foreach (var column in columns)
        {
            Assert.True(
                column.WorstSlotMilliseconds < 15000.0,
                $"{column.Name}'s worst slot took {column.WorstSlotMilliseconds:F0} ms, which is not "
                    + "inside FT8's 15 seconds. The decoder would not keep up with the air.");
        }

        // Nothing about the rate is asserted. A rung that returns nothing is a measurement.
    }

    /// <summary>The -19 dB rung. One of the two the 50 per cent crossing is interpolated between.</summary>
    [Fact]
    public void TheScoreboardAtMinus19Db() => Walk(-19.0, Isolation(), isolation: true);

    /// <summary>The -20 dB rung. The other one the crossing is interpolated between.</summary>
    [Fact]
    public void TheScoreboardAtMinus20Db() => Walk(-20.0, Isolation(), isolation: true);

    /// <summary>
    /// <b>The -21 dB rung: the one number this phase reads, before and after.</b> The recorded before
    /// is 33 of 306, 10.8 per cent, 95 per cent Wilson 7.8 to 14.8, zero wrong.
    /// </summary>
    [Fact]
    public void TheScoreboardAtMinus21Db() => Walk(-21.0, Isolation(), isolation: true);

    /// <summary>
    /// <b>NICE TO PASS, AND NOT PART OF THE ISOLATION: the shipping configuration at -21 dB, fine
    /// sync on, before and after.</b>
    /// </summary>
    /// <remarks>
    /// <c>Ft8Reception</c> builds <c>new Ft8DeepSlotDecoder(osd: Ft8DeepOsdSettings.Default,
    /// fineSync: Ft8DeepFineSyncSettings.Default)</c>, so this is the pair of columns that says what
    /// changes on the operator's screen rather than what the isolation says. <b>Two stages are
    /// stacked here and no figure from this method is reported as step 3's</b> - step 3's exits are
    /// judged on the isolation above.
    /// </remarks>
    [Fact]
    public void TheShippingConfigurationAtMinus21Db()
    {
        var fine = Ft8DeepFineSyncSettings.Default;
        Column[] columns =
        [
            Deep("ship today", new Ft8DeepOsdSettings(2, FullBasis), fine),
            Deep($"ship o{AfterOrder} W{AfterWindow}", new Ft8DeepOsdSettings(AfterOrder, AfterWindow), fine),
        ];

        Walk(-21.0, columns, isolation: false);
    }
}
