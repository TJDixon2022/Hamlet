using System.Globalization;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE SCOREBOARD IS THE CW GUARD** (work instruction 552, task 4, HM-DEC-256). The owner, 2026-10-07: *yes*. It stands on the
/// carry-forward line in place of the three CW read guards retired then, which read the corpus R88 bars and were pinned to the
/// decoder that came out.
/// </summary>
/// <remarks>
/// <para>**A GUARD THAT IS RED SAYS NOTHING.** The scoreboard's own test is red on two hard limits known since before this unit,
/// the random carrier printing at one seed and one letter in a silence, so this asserts no regression against the last row
/// recorded in <c>docs\cw-scoreboard.md</c>: the score not below it, spaces right not below it and spaces added not above it,
/// letters in a silence no more; the random carrier at no more seeds than the recorded figure under that file's guard heading;
/// the first recording whole; loud noise silent.</para>
/// <para>A unit that raises the board appends its row, and the guard holds it there from then on.</para>
/// </remarks>
public sealed partial class TheScoreboardGuardsCwTests(ITestOutputHelper output)
{
    /// <summary>The last row recorded, as the guard reads it.</summary>
    internal sealed record Recorded(string Unit, int Score, int SilencePrints, int SpacesRight, int SpacesAdded, int CarrierSeeds);

    /// <summary>The scoreboard's file.</summary>
    internal static string Board()
    {
        var dir = AppContext.BaseDirectory;

        while (dir is not null && !File.Exists(Path.Combine(dir, "docs", "cw-scoreboard.md")))
        {
            dir = Path.GetDirectoryName(dir);
        }

        return Path.Combine(dir ?? throw new FileNotFoundException("docs\\cw-scoreboard.md"), "docs", "cw-scoreboard.md");
    }

    /// <summary>The last unit row in the file, and the guard's recorded carrier figure.</summary>
    internal static Recorded LastRecorded(string text)
    {
        var row = text.Split('\n').Select(l => l.Trim()).Last(l => UnitRow().IsMatch(l));
        var cells = row.Split('|').Select(c => c.Trim()).ToArray();
        int First(string cell) => int.Parse(Number().Match(cell).Value, CultureInfo.InvariantCulture);
        var spaces = Number().Matches(cells[8]);

        return new Recorded(
            cells[1],
            First(cells[6]),
            First(cells[7]),
            int.Parse(spaces[0].Value, CultureInfo.InvariantCulture),
            int.Parse(spaces[2].Value, CultureInfo.InvariantCulture),
            int.Parse(CarrierLine().Match(text).Groups[1].Value, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void TheScoreboardFallsNowhere()
    {
        var recorded = LastRecorded(File.ReadAllText(Board()));
        var board = TheRecordingsScoreboardTests.Score();
        var carriers = board.Noise.Count(n => n.What.StartsWith("a carrier", StringComparison.Ordinal) && n.Reads.Length > 0);
        var loud = board.Noise.Where(n => n.What.Contains("loud noise", StringComparison.Ordinal)).ToList();

        output.WriteLine($"recorded, {recorded.Unit}: score {recorded.Score}, spaces {recorded.SpacesRight} right and {recorded.SpacesAdded} added, {recorded.SilencePrints} in a silence, the carrier at {recorded.CarrierSeeds} seeds");
        output.WriteLine($"now: score {board.Score}, spaces {board.SpacesRight} right and {board.SpacesAdded} added, {board.SilencePrints.Count} in a silence, the carrier at {carriers} seeds; the first recording reads `{board.FirstReads}`");

        Assert.True(board.Score >= recorded.Score, $"the score fell from {recorded.Score} to {board.Score}");
        Assert.True(board.SpacesRight >= recorded.SpacesRight, $"spaces right fell from {recorded.SpacesRight} to {board.SpacesRight}");
        Assert.True(board.SpacesAdded <= recorded.SpacesAdded, $"spaces added rose from {recorded.SpacesAdded} to {board.SpacesAdded}");
        Assert.True(board.SilencePrints.Count <= recorded.SilencePrints, $"letters in a silence rose from {recorded.SilencePrints} to {board.SilencePrints.Count}");
        Assert.True(carriers <= recorded.CarrierSeeds, $"the random carrier printed at {carriers} seeds, recorded {recorded.CarrierSeeds}");
        Assert.Equal(TheRecordingsScoreboardTests.FirstRecording, board.FirstReads);
        Assert.NotEmpty(loud);
        Assert.All(loud, n => Assert.Equal(string.Empty, n.Reads));
    }

    /// <remarks>
    /// The guard reads the last unit row and not a scan row after it, takes the score from the bold cell and the spaces from
    /// "right of reference, added", and the carrier from its guard line.
    /// </remarks>
    [Fact]
    public void TheLastUnitRowIsWhatTheGuardReads()
    {
        const string text = """
            guard: the random carrier prints at 2 of 20 seeds (5195, 5206), recorded at unit 900

            | unit | date | right | wrong | invented | score | printed in silence | spaces right | what changed |
            |---|---|---|---|---|---|---|---|---|
            | 900 task 1 | 2026-10-07 | 248 of 288 | 19 | 2 | **227** | 1, as at HEAD | 65 of 87, 2 added | a row |
            | 900 task 2 | 2026-10-07 | 250 of 288 | 18 | 2 | **230** | 0 | 66 of 87, 1 added | the last |

            | unit | catch | tone | reference | right | wrong | printed |
            |---|---|---|---|---|---|---|
            | 544 task 1 | `catch-153810-7033367` | 860 | `X` | 1 | 0 | `X` |
            """;

        Assert.Equal(new Recorded("900 task 2", 230, 0, 66, 1, 2), LastRecorded(text));
    }

    /// <summary>The last row of the `w1aw` table, its unit and its score (work instruction 554, task 3).</summary>
    internal static (string Unit, int Score) LastW1aw(string text)
    {
        var match = text.Split('\n').Select(l => W1awRow().Match(l.Trim())).Last(m => m.Success);

        return (match.Groups[1].Value.Trim(), int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture));
    }

    /// <remarks>
    /// **THE W1AW TABLE FALLS NOWHERE** (work instruction 554, task 3, HM-DEC-258): the four pieces of W1AW's session of
    /// 2026-10-07, scored in their own table, hold to its last row.
    /// </remarks>
    [Fact]
    public void TheW1awTableFallsNowhere()
    {
        var recorded = LastW1aw(File.ReadAllText(Board()));
        var board = TheW1awTableTests.Score();

        output.WriteLine($"recorded, {recorded.Unit}: w1aw score {recorded.Score}; now {board.Score}, {board.Right} of {board.OutOf} right, {board.Wrong} wrong, {board.Invented} invented");

        Assert.True(board.Score >= recorded.Score, $"the w1aw score fell from {recorded.Score} to {board.Score}");
    }

    /// <remarks>The w1aw guard reads the w1aw table's last row; the units guard never reads a w1aw row.</remarks>
    [Fact]
    public void TheW1awRowIsWhatItsGuardReads()
    {
        const string text = """
            guard: the random carrier prints at 1 of 20 seeds (5195), recorded at unit 900

            | unit | date | right | wrong | invented | score | printed in silence | spaces right | what changed |
            |---|---|---|---|---|---|---|---|---|
            | 900 task 1 | 2026-10-07 | 250 of 288 | 18 | 2 | **230** | 0 | 66 of 87, 1 added | the last |

            | unit | date | score | right | wrong | invented | spaces right | what changed |
            |---|---|---|---|---|---|---|---|
            | 900 baseline | 2026-10-07 | **2000** | 2050 of 2151 | 50 | 0 | 437 of 440, 20 added | before |
            | 900 task 1 | 2026-10-07 | **2097** | 2116 of 2151 | 19 | 0 | 438 of 440, 11 added | after |
            """;

        Assert.Equal(("900 task 1", 2097), LastW1aw(text));
        Assert.Equal(230, LastRecorded(text).Score);
    }

    // A row of the w1aw table: its unit, its date, then its score in bold.
    [GeneratedRegex(@"^\|\s*(\d{3} (?:task \d+|baseline))\s*\|[^|]*\|\s*\*\*(\d+)\*\*")]
    private static partial Regex W1awRow();

    // A unit row of the units table: its unit, then its date, then right as "N of N"; the scan table's rows carry a catch there.
    [GeneratedRegex(@"^\|\s*\d{3} task \d+\s*\|[^|]*\|\s*\d+ of \d+")]
    private static partial Regex UnitRow();

    [GeneratedRegex(@"\d+")]
    private static partial Regex Number();

    [GeneratedRegex(@"guard: the random carrier prints at (\d+) of 20 seeds")]
    private static partial Regex CarrierLine();
}
