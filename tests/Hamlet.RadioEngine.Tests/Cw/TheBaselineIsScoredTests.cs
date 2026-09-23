using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Every keyed recording scored at HEAD: the correctness phase's baseline
/// (work instruction 410, task 2; PHASE_PLAN.md 0.2).
/// </summary>
/// <remarks>
/// <para>**EVERY KEY HERE IS INFERRED** (R61, CLAUDE.md 0.0). The 17:37 key is
/// reasoned from the fixed form of a CQ call and says so in its own file. The
/// adjudicated texts are read from <see cref="TheAdjudicatedReadingsKeepReadingTests"/>,
/// whose provenance is a ruling or an adjudication and never says exact or
/// transcribed, so they are marked inferred as the instruction orders.</para>
/// <para>**THE SCORED REGION IS THE KEY FILE'S WHERE THERE IS ONE.** 17:37's runs
/// from the first `C` of the first `CQ` to the last character emitted, scored
/// whole. The adjudicated texts name no region: each covers a fragment of its
/// recording, so it is aligned to the stretch of the decode that fits it best
/// and nothing either side is scored (<see cref="CwScorer.Within"/>).</para>
/// <para>A printer. It asserts only that the new scorer and the Levenshtein
/// already in <see cref="TheSeventeenThirtySevenCaptureTests"/> agree on 17:37.</para>
/// </remarks>
public sealed class TheBaselineIsScoredTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the table is printed.</param>
    public TheBaselineIsScoredTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The three adjudicated recordings the baseline names.</summary>
    public static IReadOnlyList<string> Anchors { get; } = new[]
    {
        "cw-2026-08-17-013347",
        "cw-2026-08-17-134712",
        "unadjudicated/cw-2026-08-18-003758",
    };

    /// <summary>The 17:37 capture scored over the region its key file names.</summary>
    /// <returns>The score.</returns>
    internal static CwScore SeventeenThirtySeven()
    {
        var text = string.Concat(
            TheSeventeenThirtySevenCaptureTests.Settle(TheSeventeenThirtySevenCaptureTests.Name)
                .Select(c => c.Text));

        return CwScorer.Whole(
            CwScorer.FromFirst(text, "CQ"),
            TheSeventeenThirtySevenCaptureTests.InferredKey,
            CwKeyKind.Inferred);
    }

    /// <summary>One adjudicated recording scored against its adjudicated text.</summary>
    /// <param name="reading">The reading.</param>
    /// <returns>The score.</returns>
    internal static CwScore Adjudicated(TheAdjudicatedReadingsKeepReadingTests.Reading reading)
        => CwScorer.Within(
            TheAdjudicatedReadingsKeepReadingTests.Settled(reading.Name),
            reading.Adjudicated,
            CwKeyKind.Inferred);

    private void Row(string name, CwScore score, string note)
    {
        _output.WriteLine(
            $"row | {name} | {score.Edits} | {score.ScoredLength} | "
            + $"{(score.Kind == CwKeyKind.Exact ? "exact" : "inferred")} | "
            + $"{score.Region.Length} | `{score.Region}` | `{score.Key}` | {note}");
    }

    /// <remarks>
    /// Proves 0.2: 17:37 and the three adjudicated recordings scored at HEAD, each
    /// with its three parts, and the baseline total. The other adjudicated
    /// recordings in the tree are scored after it and kept out of the total.
    /// </remarks>
    [Fact]
    public void TheBaselineIsPrinted()
    {
        _output.WriteLine(
            "row | recording | edits | scored length | key | region length | region | key text | note");

        var total = new List<CwScore>();
        var seventeen = SeventeenThirtySeven();

        Row(TheSeventeenThirtySevenCaptureTests.Name, seventeen, "key file's region, scored whole");
        total.Add(seventeen);

        foreach (var name in Anchors)
        {
            var reading = TheAdjudicatedReadingsKeepReadingTests.All.Single(r => r.Name == name);
            var score = Adjudicated(reading);

            Row(name, score, $"{reading.Ruling}{(reading.Retired.Length > 0 ? ", retired as an anchor" : "")}");
            total.Add(score);
        }

        _output.WriteLine(
            $"total | {total.Sum(s => s.Edits)} edits over {total.Sum(s => s.ScoredLength)} "
            + "characters against inferred keys, 4 recordings");

        var others = new List<CwScore>();

        foreach (var reading in TheAdjudicatedReadingsKeepReadingTests.All
                     .Where(r => !Anchors.Contains(r.Name)))
        {
            var score = Adjudicated(reading);

            Row(reading.Name, score, $"outside the baseline, {reading.Ruling}"
                + (reading.Retired.Length > 0 ? ", retired as an anchor" : ""));
            others.Add(score);
        }

        _output.WriteLine(
            $"outside | {others.Sum(s => s.Edits)} edits over {others.Sum(s => s.ScoredLength)} "
            + $"characters against inferred keys, {others.Count} recordings");

        Assert.Equal(
            TheSeventeenThirtySevenCaptureTests.Distance(
                seventeen.Region, TheSeventeenThirtySevenCaptureTests.InferredKey),
            seventeen.Edits);
    }
}
