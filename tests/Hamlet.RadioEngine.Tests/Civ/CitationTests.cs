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

    private static void AssertPageExists(string citation, string what)
    {
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
