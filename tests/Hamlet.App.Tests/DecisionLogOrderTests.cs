using System.Text.RegularExpressions;
using Xunit;

namespace Hamlet.App.Tests;

/// <summary>
/// `CLAUDE.md` §1 is newest-first, and stays that way.
/// </summary>
/// <remarks>
/// <para>**IT SAYS "EVERY RULING, MOST RECENT FIRST" AND IT WAS NOT.** Five rows
/// were out of order and one of them sat at the head, so the newest ruling was not
/// the one a reader's eye landed on and `RULES_AT` could not be taken from row one
/// (HM-OPEN-036). It went unnoticed for days because nothing checked and the table
/// is a hundred and thirty-nine rows long.</para>
/// <para>**TWO CAUSES, BOTH VISIBLE IN THE HISTORY.** `5d00bd4` and `303c4f4`
/// inserted a row immediately *below* the top row rather than at the separator,
/// which is the fixed anchor that open item names. `d263f95` pasted a block of
/// four rulings in the order they were made, which is oldest-first inside a
/// newest-first table. Neither is a script in this repository; both are how a
/// delivery gets composed, so the rule is stated in §1 where the next one will be
/// read, and this test is what makes it real rather than advisory.</para>
/// <para>Dates only. Within one date the id is the only recency there is, so that
/// is checked too, and the one place where a row's date and its id disagree about
/// which ruling is newer is named below rather than sorted away: **changing a
/// ruling's date to make a test pass is falsifying the record.**</para>
/// </remarks>
public sealed class DecisionLogOrderTests
{
    private static readonly Regex Row = new(
        @"^\| (\d{4}-\d{2}-\d{2}) \|.*\| HM-DEC-(\d{3}) \|$",
        RegexOptions.Compiled);

    /// <summary>
    /// The one pair whose date and id disagree, left exactly as it is.
    /// </summary>
    /// <remarks>
    /// HM-DEC-051's row is dated 2026-08-14 and HM-DEC-050's is dated 2026-08-15,
    /// so by date 050 is the newer and by id 051 is. One of the two dates is
    /// wrong and **which one is not a session's to guess** (§12.4): a row's date
    /// may be the day the ruling was made rather than the day it was written
    /// down, and correcting the wrong one falsifies the record rather than fixing
    /// it. Reported to Tim on 2026-08-19 and carried here so the sweep stays
    /// green without the conflict being hidden.
    /// </remarks>
    private static readonly (int Above, int Below) KnownDateConflict = (51, 50);

    private static IReadOnlyList<(string Date, int Id, int Line)> Rows()
    {
        var here = new DirectoryInfo(AppContext.BaseDirectory);

        while (here is not null && !File.Exists(Path.Combine(here.FullName, "CLAUDE.md")))
        {
            here = here.Parent;
        }

        Assert.NotNull(here);

        var rows = new List<(string, int, int)>();
        var lines = File.ReadAllLines(Path.Combine(here!.FullName, "CLAUDE.md"));

        for (var i = 0; i < lines.Length; i++)
        {
            if (Row.Match(lines[i]) is { Success: true } m)
            {
                rows.Add((m.Groups[1].Value, int.Parse(m.Groups[2].Value), i + 1));
            }
        }

        return rows;
    }

    /// <remarks>
    /// Proves §1's own instruction: every ruling, most recent first. The one pair
    /// whose dates and ids disagree is named as an exception rather than sorted
    /// away, because the disagreement is in the data.
    /// </remarks>
    [Fact]
    public void TheDecisionLogIsNewestFirst()
    {
        var rows = Rows();

        Assert.True(rows.Count > 100, "the decision log did not parse");

        var wrong = new List<string>();

        for (var i = 0; i < rows.Count - 1; i++)
        {
            var (aboveDate, aboveId, line) = rows[i];
            var (belowDate, belowId, _) = rows[i + 1];

            if ((aboveId, belowId) == KnownDateConflict)
            {
                continue;
            }

            if (string.CompareOrdinal(aboveDate, belowDate) < 0)
            {
                wrong.Add(
                    $"line {line}: HM-DEC-{aboveId:000} ({aboveDate}) sits above "
                    + $"HM-DEC-{belowId:000} ({belowDate})");
            }
            else if (aboveId == RenumberedRuling.Id || belowId == RenumberedRuling.Id)
            {
                // Its id no longer says when it was ruled, so the id ordering
                // rule cannot speak about it. Its date still can, and the date
                // check above still applies to it.
                continue;
            }
            else if (aboveDate == belowDate && aboveId < belowId)
            {
                wrong.Add(
                    $"line {line}: HM-DEC-{aboveId:000} sits above "
                    + $"HM-DEC-{belowId:000}, both {aboveDate}");
            }
        }

        Assert.True(
            wrong.Count == 0,
            "§1 says every ruling, most recent first. A new row goes immediately "
            + "below the |---| separator and above every existing row, and a batch "
            + "goes in newest first:"
            + Environment.NewLine
            + string.Join(Environment.NewLine, wrong));
    }

    /// <summary>
    /// A ruling whose id is later than its date, by clerical correction.
    /// </summary>
    /// <remarks>
    /// **THE REUSE IS GONE AND THIS IS WHAT IT LEFT BEHIND.** HM-DEC-088 carried
    /// two different 2026-08-16 rulings, which §2.1 forbids outright. Tim ruled on
    /// 2026-08-19 that the later one takes the next free id, and the tiebreak came
    /// from the history rather than from judgment: both index rows arrived in
    /// `49b844c`, the decoder's noise-measurement row is first in that commit, and
    /// `DECISIONS.md`'s only 088 entry is the decoder's. So the top-strip ruling
    /// became HM-DEC-141 and every citation aimed at it was re-pointed.
    /// </remarks>
    private static readonly (int Id, string Date) RenumberedRuling = (141, "2026-08-16");

    /// <remarks>
    /// Proves the index is complete in the direction that matters: no id appears
    /// twice except the one that already does, and the ids present run without a
    /// gap this project has not deliberately made. HM-DEC-136 was drafted and
    /// withdrawn, so its absence is a ruling rather than a defect; HM-DEC-105 is
    /// absent and should not be, which is HM-OPEN-045.
    /// </remarks>
    [Fact]
    public void EveryRulingAppearsOnceAndTheGapsAreTheKnownOnes()
    {
        var ids = Rows().Select(r => r.Id).ToList();

        var repeated = ids.GroupBy(i => i)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .OrderBy(i => i)
            .ToList();

        // **NO ALLOWANCE ANY MORE.** There was one, for HM-DEC-088 carrying two
        // rulings; the renumber removed the thing it was allowing for, so the
        // allowance goes with it rather than staying as a door somebody can walk
        // back through.
        Assert.Empty(repeated);

        // And the renumbered ruling is where it should be: one row, dated the day
        // it was ruled rather than the day it was renumbered.
        Assert.Contains(
            Rows(),
            r => r.Id == RenumberedRuling.Id && r.Date == RenumberedRuling.Date);

        var missing = Enumerable.Range(1, ids.Max())
            .Where(i => !ids.Contains(i))
            .ToList();

        Assert.Equal(new[] { 105, 136 }, missing);
    }
}
