using System.Globalization;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;
using static Hamlet.RadioEngine.Tests.Cw.TheRecordingsScoreboardTests;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE SPLIT, TRACED** (work instruction 575, task 1): for every word gap the twelve-plus-three miss under the line and every
/// letter gap they space over it, the gaps the split had in hand when the letter after it printed, how the walk divided them,
/// the path it took, the boundary it drew, and which side the gap landed on; then the same for twenty word gaps W1AW reads.
/// Asserts nothing.
/// </summary>
public sealed class TheSplitTraceTests(ITestOutputHelper output)
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <remarks>The seventeen, then twenty of W1AW's word gaps read.</remarks>
    [Fact]
    public void TheSplitOnTheSeventeenAndOnW1aw()
    {
        var board = Score(limits: false);
        var seventeen = board.Stretches.Where(s => s.Stretch.Confidence >= Confidence.Medium).SelectMany(TheSpacesByCauseTests.PlacesOf)
            .Where(p => p.Cause.Contains("under the line", StringComparison.Ordinal) || p.Cause.Contains("over the line", StringComparison.Ordinal))
            .ToList();

        output.WriteLine("## the seventeen");
        Header();

        foreach (var p in seventeen)
        {
            Row(p, p.Missing ? "word gap missed" : "letter gap spaced");
        }

        var w1aw = TheW1awTableTests.Score();
        var read = w1aw.Stretches.Where(s => s.Stretch.Confidence >= Confidence.Medium)
            .SelectMany(s => TheSpacesByCauseTests.PlacesOf(s, rights: true))
            .Where(p => p.Cause == "a word gap read" && p.Lines?.Given is { Count: > 0 })
            .ToList();
        var step = Math.Max(1, read.Count / 20);

        output.WriteLine("## twenty of W1AW's word gaps read");
        Header();

        foreach (var p in read.Where((_, i) => i % step == 0).Take(20))
        {
            Row(p, "word gap read");
        }

        output.WriteLine(string.Create(Inv, $"board: spaces {board.SpacesRight} of {board.SpacesOutOf}, {board.SpacesAdded} added; w1aw spaces {w1aw.SpacesRight} of {w1aw.SpacesOutOf}, {w1aw.SpacesAdded} added; w1aw word gaps read {read.Count}"));
    }

    private void Header()
    {
        output.WriteLine("| recording | around | kind | gap | gaps in hand, sorted (ms) | walk clusters | largest jump | path | letter / word | boundary | line | side | gap dit | letter line | gap in gap dits | the midpoint split's line | then |");
        output.WriteLine("|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|");
    }

    private void Row(TheSpacesByCauseTests.Place p, string kind)
    {
        var lines = p.Lines;
        var given = lines?.Given ?? [];
        var (clusters, at, path, jump, boundary) = CwPatternGate.TraceSplit(given);
        var hand = string.Join(" ", given.Select(g => (g * 1000).ToString("0", Inv)));
        var counts = $"{lines?.LetterGaps?.Count.ToString(Inv) ?? "-"} / {lines?.WordGaps?.Count.ToString(Inv) ?? "-"}";
        var side = double.IsNaN(p.GapMs) ? "-" : p.GapMs > p.LineMs ? "word" : "letter";
        var midpoint = (lines?.GapDitSeconds ?? double.NaN) * CwPatternGate.WordMidpointGapDits * 1000;
        var straddles = lines?.LetterGaps is { Count: > 0 } lg && lg.Min() * 1000 < midpoint && lg.Max() * 1000 >= midpoint;
        var newLine = straddles ? midpoint : p.LineMs;
        var then = double.IsNaN(p.GapMs) ? "-" : p.GapMs > newLine ? "word" : "letter";

        output.WriteLine(string.Create(Inv, $"| `{p.Recording}` | `{p.Around}` | {kind} | {p.GapMs:0} | {hand} | {string.Join("+", clusters)} (letter #{at}) | {jump:0.00} | {path} | {counts} | {boundary * 1000:0} | {p.LineMs:0} | {side} | {(lines?.GapDitSeconds ?? double.NaN) * 1000:0} | {(lines?.CharacterSeconds ?? double.NaN) * 1000:0} | {p.GapMs / ((lines?.GapDitSeconds ?? double.NaN) * 1000):0.0} | {newLine:0}{(straddles ? "" : " (unmoved)")} | {then} |"));
    }
}
