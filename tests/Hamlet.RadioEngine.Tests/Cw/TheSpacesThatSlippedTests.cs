using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE SPACES THAT SLIPPED** (work instruction 546, task 3): when the neighbour judgement of gaps came back, spaces right
/// fell from 58 to 55 while spaces added fell from 4 to 2. Which spaces, where, the gaps either side and the line.
/// </summary>
/// <remarks>R88 is lifted for the owner's twelve recordings; this reads them through the scoreboard, as it reads them.</remarks>
public sealed class TheSpacesThatSlippedTests
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the trace is printed.</param>
    public TheSpacesThatSlippedTests(ITestOutputHelper output) => _output = output;

    /// <remarks>
    /// Task 3: the scoreboard with a rule off and on: the neighbour judgement of gaps, and three lone letters dropped. For each stretch whose spaces right or added
    /// moved, both printed texts, and every letter where the two disagree about a space before it: when, the gap before it,
    /// the sender's letter and word lines then, and which reading the reference agrees with. Asserts nothing but that the
    /// board was read.
    /// <param name="rule">The rule switched off.</param>
    /// </remarks>
    [Theory]
    [InlineData(CwRules.NeighbourGaps)]
    [InlineData(CwRules.ThreeLone)]
    public void WhichSpacesARuleMoved(string rule)
    {
        TheRecordingsScoreboardTests.Board off;

        using (CwRules.Off(rule))
        {
            off = TheRecordingsScoreboardTests.Score(limits: false);
        }

        var on = TheRecordingsScoreboardTests.Score(limits: false);

        _output.WriteLine($"{rule} off: {off.SpacesRight} spaces right, {off.Stretches.Where(s => s.Stretch.Confidence >= TheRecordingsScoreboardTests.Confidence.Medium).Sum(s => s.Spaces.Added)} added");
        _output.WriteLine($"{rule} on:  {on.SpacesRight} spaces right, {on.Stretches.Where(s => s.Stretch.Confidence >= TheRecordingsScoreboardTests.Confidence.Medium).Sum(s => s.Spaces.Added)} added");

        foreach (var (a, b) in off.Stretches.Zip(on.Stretches))
        {
            if (a.Spaces == b.Spaces || a.Stretch.Confidence < TheRecordingsScoreboardTests.Confidence.Medium)
            {
                continue;
            }

            _output.WriteLine("");
            _output.WriteLine(string.Create(Invariant, $"`{a.Stretch.Recording}` at {a.Stretch.PitchHz:0} Hz, {a.Stretch.From:0.0}-{a.Stretch.To:0.0} s: spaces off {a.Spaces.Right} right {a.Spaces.Added} added, on {b.Spaces.Right} right {b.Spaces.Added} added"));
            _output.WriteLine($"  reference `{a.Stretch.Reference}`");
            _output.WriteLine($"  off       `{a.PrintedText}`");
            _output.WriteLine($"  on        `{b.PrintedText}`");

            var la = a.Letters ?? [];
            var lb = b.Letters ?? [];

            foreach (var x in lb)
            {
                var y = la.FirstOrDefault(l => Math.Abs(l.From - x.From) < 0.02 && l.Text == x.Text);

                if (y.Text is null || y.SpaceBefore == x.SpaceBefore)
                {
                    continue;
                }

                var i = lb.ToList().IndexOf(x);
                var gap = i > 0 ? (x.From - lb[i - 1].Seconds) * 1000 : double.NaN;
                var lines = x.Lines;
                var sb = new StringBuilder();

                sb.Append(string.Create(Invariant, $"  {x.From:0.00} s `{x.Text}`: space before off {y.SpaceBefore}, on {x.SpaceBefore}; gap before {gap:0} ms"));

                if (lines is not null)
                {
                    sb.Append(string.Create(Invariant, $"; letter line {lines.CharacterSeconds * 1000:0} ms, word line {lines.WordSeconds * 1000:0} ms, letter cluster {(lines.LetterGaps is { } lg ? $"{lg.Count} at {lg.Average() * 1000:0}" : "none")}, word cluster {(lines.WordGaps is { } wg ? $"{wg.Count} at {wg.Average() * 1000:0}" : "none")}"));
                }

                _output.WriteLine(sb.ToString());
            }
        }

        Assert.NotEmpty(on.Stretches);
    }
}

/// <summary>Task 3: the spaces with each of the three rules that came back together switched off, alone and all three.</summary>
public sealed class TheSpacesEachRuleCostsTests(ITestOutputHelper output)
{
    /// <remarks>Prints score and spaces for each switch set; asserts only that the board was read.</remarks>
    [Theory]
    [InlineData("none")]
    [InlineData(CwRules.Narrowness)]
    [InlineData(CwRules.ThreeLone)]
    [InlineData(CwRules.NeighbourGaps)]
    [InlineData(CwRules.KeptSplit)]
    [InlineData("all three")]
    public void TheSpacesWithARuleOff(string rule)
    {
        var rules = rule == "none" ? Array.Empty<string>() : rule == "all three" ? [CwRules.Narrowness, CwRules.ThreeLone, CwRules.NeighbourGaps] : [rule];

        using var _ = CwRules.Off(rules);

        var board = TheRecordingsScoreboardTests.Score(limits: false);
        var counted = board.Stretches.Where(s => s.Stretch.Confidence >= TheRecordingsScoreboardTests.Confidence.Medium).ToList();

        output.WriteLine($"off: {rule} | right {board.Total} | wrong {board.Wrong} | spaces right {board.SpacesRight} | added {counted.Sum(s => s.Spaces.Added)}");

        foreach (var s in counted)
        {
            output.WriteLine($"  {s.Stretch.Recording} {s.Stretch.PitchHz:0}: spaces {s.Spaces.Right} of {s.Spaces.OfReference}, added {s.Spaces.Added}");
        }

        Assert.NotEmpty(board.Stretches);
    }
}
