using System.Text.RegularExpressions;
using Hamlet.RadioEngine.Civ;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Civ;

/// <summary>
/// Every page Hamlet cites exists in the edition Hamlet cites (HM-DEC-071).
/// </summary>
/// <remarks>
/// <para>ONE EDITION OF THE TRUTH. The citation table used to span three
/// printings, and page numbers drift between them. Two rows pointed at a page
/// 19-14 that does not exist in this manual at all, and nothing noticed for
/// weeks because a page number looks like a page number.</para>
/// <para>This is the cheap half of the check and it is the half that runs on
/// every build. It cannot confirm that page 19-3 says what Hamlet claims, which
/// takes a person with the manual open. It can confirm that the page exists, and
/// that is what would have caught 19-14 the day it was written.</para>
/// </remarks>
public sealed class CitationTests
{
    /// <summary>
    /// The last page of each chapter Hamlet cites, in publication A7292-4EX-6.
    /// </summary>
    /// <remarks>
    /// Read from the manual's own page footers, which run consecutively with no
    /// gaps in any chapter. Chapter 19 ending at 19-13 is the row that matters:
    /// it is the one an earlier printing numbered differently.
    /// </remarks>
    private static readonly Dictionary<int, int> LastPage = new()
    {
        [2] = 5, [4] = 31, [12] = 12, [18] = 4, [19] = 13,
    };

    /// <summary>The edition every citation in this repository refers to.</summary>
    public const string Edition = "A7292-4EX-6";

    /// <remarks>
    /// Proves HM-DEC-071: no read cites a page that does not exist. A citation
    /// naming a page the manual does not have is a citation nobody can check,
    /// which is the same as none at all (§4).
    /// </remarks>
    [Fact]
    public void EveryCitedPageExistsInThisEdition()
    {
        Assert.NotEmpty(CivReads.All);

        foreach (var read in CivReads.All)
        {
            AssertPageExists(read.Page, read.Field.ToString());
        }

        foreach (var write in CivWrites.All)
        {
            AssertPageExists(write.Page, write.Field.ToString());
        }
    }

    /// <remarks>
    /// Proves §12.4: a row that says its page is not yet verified names an open
    /// issue, and that issue is in the record and open. Without this the marker
    /// would be a way around the sweep above rather than a question with an
    /// owner, which is exactly the difference the ruling is about.
    /// </remarks>
    [Fact]
    public void EveryUncitedRowNamesAnOpenIssueThatExists()
    {
        var here = new DirectoryInfo(AppContext.BaseDirectory);

        while (here is not null
               && !File.Exists(Path.Combine(here.FullName, "OPEN_ISSUES.md")))
        {
            here = here.Parent;
        }

        Assert.NotNull(here);

        var record = File.ReadAllText(
            Path.Combine(here!.FullName, "OPEN_ISSUES.md"));

        var uncited = CivReads.All
            .Select(r => (Row: r.Field.ToString(), r.Page))
            .Concat(CivWrites.All.Select(w => (Row: w.Field.ToString(), w.Page)))
            .Where(x => MarkedUncited.IsMatch(x.Page))
            .ToList();

        foreach (var (row, page) in uncited)
        {
            var id = page[9..^1];

            Assert.True(
                record.Contains($"id: {id}", StringComparison.Ordinal),
                $"{row} is marked uncited against {id}, and {id} is not in "
                + "OPEN_ISSUES.md");
        }
    }

    /// <remarks>
    /// Proves HM-DEC-071: the pages behind the scope preconditions exist too.
    /// These are prose rather than table rows, so they are swept separately and
    /// the same rule applies.
    /// </remarks>
    [Fact]
    public void TheProseCitationsNameRealPagesToo()
    {
        // Bounded so the edition code itself is not read as a page: the "92-4"
        // inside A7292-4EX-6 matches a bare page pattern perfectly.
        foreach (Match match in Regex.Matches(
                     Hamlet.RadioEngine.Rig.ScopeReadiness.Citation,
                     @"(?<![\w-])\d{1,2}-\d{1,2}(?![\w-])"))
        {
            AssertPageExists(match.Value, "the scope preconditions");
        }

        Assert.Contains(Edition, Hamlet.RadioEngine.Rig.ScopeReadiness.Citation,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// What a row looks like when the figure is real and the page is not yet
    /// verified (§12.4).
    /// </summary>
    /// <remarks>
    /// **AN UNCITED VALUE IS A QUESTION WITH AN OWNER, AND AN UNMARKED ONE IS
    /// INDISTINGUISHABLE FROM A VERIFIED ONE.** Every row in this file names a
    /// page in `A7292-4EX-6` read column-aware, and one does not: the transceive
    /// setting Hamlet reads to say whether the radio announces its own changes,
    /// whose sub-command came from a work order rather than from the manual. The
    /// honest options were to leave the read unbuilt, to write a page number
    /// nobody had read, or to say plainly which it is. The third is what §12.4
    /// exists for, and the test below proves the marker names a live open item
    /// rather than becoming a way around this sweep.
    /// </remarks>
    private static readonly System.Text.RegularExpressions.Regex MarkedUncited =
        new(@"^uncited \(HM-OPEN-\d{3}\)$");

    private static void AssertPageExists(string citation, string what)
    {
        if (MarkedUncited.IsMatch(citation))
        {
            return;
        }

        // A row may cite two pages, e.g. the command table and the data content.
        if (citation.Contains(',', StringComparison.Ordinal))
        {
            foreach (var one in citation.Split(','))
            {
                AssertPageExists(one.Trim(), what);
            }

            return;
        }

        var match = Regex.Match(citation, @"^(\d{1,2})-(\d{1,2})$");

        Assert.True(match.Success, $"{what} cites '{citation}', which is not a page");

        var chapter = int.Parse(match.Groups[1].Value);
        var page = int.Parse(match.Groups[2].Value);

        Assert.True(
            LastPage.TryGetValue(chapter, out var last),
            $"{what} cites chapter {chapter}, which this test does not know");

        Assert.True(
            page >= 1 && page <= last,
            $"{what} cites p. {citation}, and chapter {chapter} of {Edition} "
            + $"ends at {chapter}-{last}");
    }
}
