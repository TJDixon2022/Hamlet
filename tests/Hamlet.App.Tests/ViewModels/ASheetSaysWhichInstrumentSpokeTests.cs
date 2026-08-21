using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// A roster row carries the fit behind its speed, and the columns say what each
/// one measures (HM-DEC-091).
/// </summary>
/// <remarks>
/// <para>**A SPEED IS ONE NUMBER OUT OF A FIT, AND A FIT THAT IS NOT A FIST
/// PRODUCES ONE JUST AS READILY AS A FIT THAT IS.** On the evening this column
/// was added, four captures of one clean fourteen words a minute fist produced no
/// speed at all and a fifth produced one from a fit whose dah measured nearly
/// four dits, and nothing on any sheet distinguished them.</para>
/// <para>**NEITHER FIGURE IS A VERDICT.** `N4L` sends a dah of 4.24 dits and is a
/// real station this project read by hand (HM-DEC-144), so a ratio far from three
/// is a thing to look at rather than a fault. Nothing in the decoder reads either
/// number.</para>
/// </remarks>
public sealed class ASheetSaysWhichInstrumentSpokeTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the rows are printed.</param>
    public ASheetSaysWhichInstrumentSpokeTests(ITestOutputHelper output)
        => _output = output;

    private static string Cell(CwCase one, string column)
        => CwCaseRoster.Row(one).Split('\t')[
            Array.IndexOf(CwCaseRoster.Header.Split('\t'), column)];

    private static CwCase Case(string fit) => new(
        new DateTime(2026, 8, 20, 1, 3, 36, DateTimeKind.Utc),
        7_059_000, "40 m", "cw-2026-08-20-010336.wav", "",
        ToneHz: 825, SnrDb: 41.4, Wpm: 12, Emitted: 11, Unsure: 2,
        Text: "UR RST", Covers: CwCountsCover.Recording, Meter: "keying",
        SeedWpm: null, Fit: fit);

    /// <remarks>
    /// Proves the column exists, is named, and carries what it was given.
    /// </remarks>
    [Fact]
    public void TheRowCarriesTheFitBehindItsSpeed()
    {
        var fit = "dah 2.94 dits, clusters 18.3 apart in their own scatter, "
                  + "19 of 83 marks under half a dit, 0 set aside as too quiet";

        var row = CwCaseRoster.Row(Case(fit));

        _output.WriteLine(CwCaseRoster.Header);
        _output.WriteLine(row);

        Assert.Contains("fit", CwCaseRoster.Header.Split('\t'));
        Assert.Equal(fit, Cell(Case(fit), "fit"));

        // One row is still one line, or every column after this one lands under
        // the wrong heading.
        Assert.DoesNotContain('\n', row);
        Assert.Equal(
            CwCaseRoster.Header.Count(c => c == '\t'),
            row.Count(c => c == '\t'));
    }

    /// <remarks>
    /// Proves a row with no fit behind it says so rather than leaving a cell that
    /// looks like a column somebody forgot to fill in (HM-DEC-091).
    /// </remarks>
    [Fact]
    public void ARowWithNoFitSaysSo()
    {
        Assert.Equal("not fitted", Cell(Case(""), "fit"));
        Assert.Equal(
            CwCaseRoster.Header.Split('\t').Length,
            CwCaseRoster.Row(Case("")).Split('\t').Length);
    }

    /// <remarks>
    /// Proves the operator's own column is still last and still empty, whatever
    /// columns get added in front of it.
    /// </remarks>
    [Fact]
    public void TheReadColumnIsStillLastAndStillEmpty()
    {
        var columns = CwCaseRoster.Header.Split('\t');

        Assert.Equal("read", columns[^1]);
        Assert.Equal(string.Empty, Cell(Case("dah 3.01 dits"), "read"));
    }
}
