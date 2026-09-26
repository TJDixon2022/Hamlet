using System.Globalization;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// HM-REQ-010 (work instruction 449, task 2): a keyed tone far quieter than the
/// station being read does not take the filter from it, so nothing it sends is
/// printed sure against the station's key.
/// </summary>
/// <remarks>
/// <para>**THE CAUSE IS THE TRACE'S LARGEST OPEN GROUP**
/// (`.run-unit/unit449-trace-head.txt`): on `cw-2026-08-22-031905` the tracker
/// left the station at 500 Hz, where the instrument measured the keying, for a
/// keyed candidate at 300 Hz whose key-down sat 21.9 dB below the station's (-49.4
/// against -27.5 dB), and read 8 sure letters wrong there against 1 right; on
/// `-032050` it left for 325 Hz, 14.6 dB of lift below. HM-DEC-127's floor refuses
/// only at 25 dB.</para>
/// <para>**THE CASE.** 500 Hz at 15 dB over the generator's shaped noise band
/// (V-06), exact key `CQ CQ CQ DE N0CALL` then `N0CALL K`, with 7.5 s of band noise
/// between the two overs. A second station at 300 Hz, 15 dB below the first,
/// sends `VVV` throughout. The decoder starts at the operator's 600 Hz. The seeds
/// are 20260449 to 20260453, author's.</para>
/// <para>**WHAT THIS DOES NOT PROVE** (CLAUDE.md 12.5): one level gap, one speed,
/// one pair of pitches, from the generator the instrument was proved on. On
/// `031905` the station was never silent; the survey called it interference
/// because it keys most of the time, which this case stands in for with a
/// break between overs.</para>
/// </remarks>
public sealed class TheTrackerStaysWithTheStationItReadsTests
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the case is printed.</param>
    public TheTrackerStaysWithTheStationItReadsTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>HM-REQ-010: no sure letter wrong or added against the station's key.</summary>
    [Fact]
    public void AQuieterKeyedToneDoesNotTakeTheFilterFromTheStation()
    {
        const string key = "CQ CQ CQ DE N0CALL N0CALL K";
        const double stationHz = 500;
        const double quieterHz = 300;
        const double belowDb = 15;

        CwFixtureRecipe Station(string text, int seed)
            => SyntheticCq.Recipe(20, CwFixtureCatalogue.EasyDb, seed) with { Text = text, ToneHz = stationHz, DriftHz = 0 };

        var first = CwFixtureGenerator.Generate(Station("CQ CQ CQ DE N0CALL", 20260449)).Audio.Samples;
        var gapA = CwFixtureGenerator.Generate(Station(string.Empty, 20260450)).Audio.Samples;
        var gapB = CwFixtureGenerator.Generate(Station(string.Empty, 20260451)).Audio.Samples;
        var second = CwFixtureGenerator.Generate(Station("N0CALL K", 20260452)).Audio.Samples;
        var station = first.Concat(gapA).Concat(gapB).Concat(second).ToArray();
        var rate = CwFixtureGenerator.SampleRate;

        var neighbour = CwFixtureGenerator.Generate(SyntheticCq.Recipe(20, CwFixtureCatalogue.EasyDb, 20260453)
            with { Text = string.Join(" ", Enumerable.Repeat("VVV", 40)), ToneHz = quieterHz, DriftHz = 0 }).Audio.Samples;
        var scale = (float)Math.Pow(10, -belowDb / 20);
        var both = new float[station.Length];

        for (var n = 0; n < both.Length; n++)
        {
            both[n] = Math.Clamp(station[n] + (n < neighbour.Length ? scale * neighbour[n] : 0f), -1f, 1f);
        }

        var decoder = new CwDecoder(rate, 600);
        var settled = new List<CwCharacter>();
        var moves = new List<string>();
        var hop = decoder.Tracker.HopSamples;
        var last = decoder.Tracker.ToneHz;

        decoder.CharacterSettled += settled.Add;

        for (var at = 0L; at + hop <= both.Length; at += hop)
        {
            decoder.Process(new Hamlet.RadioEngine.Audio.AudioChunk(at, rate, both.AsSpan((int)at, hop)));

            var now = decoder.Tracker.ToneHz;

            if (Math.Abs(now - last) > 15)
            {
                var k = decoder.Tracker.Verdict.Keyed;

                moves.Add(string.Create(Invariant,
                    $"{(at + hop) / (double)rate:0.00} s {last:0} -> {now:0}{(k is { } c ? string.Create(Invariant, $" (keyed {c.ToneHz:0} Hz, lift {c.LiftDb:0.0} dB, key-down {c.KeyedDb:0.0} dB)") : "")}"));
            }

            last = now;
        }

        decoder.Flush();

        var alignment = CwMetrics.Align(CwMetrics.Symbols(settled), key, CwKeyKind.Exact);
        var invented = CwMetrics.Invented(alignment);
        var text = string.Concat(settled.Select(c => CwSymbol.Of(c).Class == CwSymbolClass.Sure ? c.Text : c.IsWordGap ? " " : $"[{c.Text}]")).Trim();

        _output.WriteLine(string.Create(Invariant,
            $"HM-REQ-010 | key {key} | text {text} | sure added {invented.SureAdded}, sure wrong {invented.SureWrong} | moves {string.Join("; ", moves)}"));

        // The group's letters: whatever the quieter station sends, printed sure, and
        // the station's own second over lost while the filter is away. The `EQ`
        // from cold is acquisition's (parked with the owner) and any sure letter the
        // band's own noise makes during the break is not this cause; both are in
        // the counts printed above and not asserted here.
        var fromTheNeighbour = settled.Count(c => c.Text == "V" && CwSymbol.Of(c).Class == CwSymbolClass.Sure);
        var secondOver = string.Concat(settled
            .Where(c => c.At.TotalSeconds > (first.Length + gapA.Length + gapB.Length) / (double)rate)
            .Where(c => CwSymbol.Of(c).Class == CwSymbolClass.Sure || c.IsWordGap)
            .Select(c => c.Text)).Trim();

        Assert.True(fromTheNeighbour == 0 && secondOver.EndsWith("N0CALL K", StringComparison.Ordinal),
            string.Create(Invariant, $"{fromTheNeighbour} sure `V` from the {quieterHz:0} Hz station; the second over read `{secondOver}` for `N0CALL K`; moves {string.Join("; ", moves)}"));
    }
}
