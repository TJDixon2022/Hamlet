using Hamlet.RadioEngine.Cw;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The roster's `chars` column says what its numbers are counts of (HM-DEC-091).
/// </summary>
/// <remarks>
/// <para>**THE COLUMN IS SCORED, AND IT HELD THE WHOLE EVENING'S TOTALS.** The
/// decoder's counters run from the moment listening starts, so a press seven
/// hours in put a character count earned hours earlier on another band into a row
/// about a station heard just now. The number was never wrong; the column was,
/// because a figure sitting beside a recording is read as being about the
/// recording and the percentage gets computed from it either way.</para>
/// <para>**THE COLUMNS AND THEIR ORDER ARE UNCHANGED AND `read` IS UNTOUCHED.**
/// Only what goes into `chars` changes.</para>
/// </remarks>
public sealed class CwCaseCountsSayWhatTheyCountTests
{
    private static readonly DateTime When = new(2026, 8, 20, 1, 48, 54, DateTimeKind.Utc);

    private static CwCase Case(string wav, string refusal, CwCountsCover covers)
        => new(When, 14_028_000, "20 m", wav, refusal, 800, 14.1, 18, 69, 23, "", covers);

    /// <summary>Where a named column sits in the row.</summary>
    /// <remarks>
    /// **BY NAME AND NOT BY POSITION.** These were written against literal
    /// indexes, so the row's shape was pinned by four tests that all broke the
    /// first time a column was added between two others, saying nothing about
    /// the column they were actually about. The header is where the order lives
    /// and it is the header that is asked (§0).
    /// </remarks>
    private static int Column(string name)
        => Array.IndexOf(CwCaseRoster.Header.Split('	'), name);

    /// <remarks>
    /// Proves HM-DEC-091: where the figures cover the audio on the row, the cell
    /// is the plain pair of numbers, because that is what the column has always
    /// claimed to be.
    /// </remarks>
    [Fact]
    public void CountsAboutTheRecordingAreStatedPlainly()
    {
        var cell = CwCaseRoster.Row(
            Case("cw-2026-08-20-014854.wav", "", CwCountsCover.Recording)).Split('\t')[Column("chars")];

        Assert.Equal("69 emitted, 23 unsure", cell);
    }

    /// <remarks>
    /// Proves HM-DEC-091: **this is the row that misled everybody.** The counts
    /// are the session's, a recording sits beside them, and without the clause
    /// nothing on the sheet distinguishes this from the row above.
    /// </remarks>
    [Fact]
    public void CountsAboutTheEveningSayThatOnTheRow()
    {
        var cell = CwCaseRoster.Row(
            Case("cw-2026-08-20-014854.wav", "", CwCountsCover.Session)).Split('\t')[Column("chars")];

        Assert.StartsWith("69 emitted, 23 unsure", cell, StringComparison.Ordinal);
        Assert.Contains("not this case", cell, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves HM-DEC-091 and HM-DEC-090: a refused press is still a case and still
    /// carries what Hamlet knew, and a count with no recording behind it says
    /// there is none rather than being deleted to avoid the problem.
    /// </remarks>
    [Fact]
    public void CountsWithNoRecordingBehindThemSaySo()
    {
        var cell = CwCaseRoster.Row(
            Case("", "no new audio since the last one", CwCountsCover.NoRecording))
            .Split('\t')[Column("chars")];

        Assert.StartsWith("69 emitted, 23 unsure", cell, StringComparison.Ordinal);
        Assert.Contains("no recording was kept", cell, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves HM-DEC-091: **the weaker claim is the default.** A row built without
    /// saying what its counts cover must not come out looking like one that was
    /// measured over its own audio.
    /// </remarks>
    [Fact]
    public void ARowThatDoesNotSayIsTakenToBeTheWholeEvening()
    {
        var quiet = new CwCase(
            When, 14_028_000, "20 m", "cw-2026-08-20-014854.wav", "",
            800, 14.1, 18, 69, 23);

        Assert.Contains(
            "not this case",
            CwCaseRoster.Row(quiet).Split('\t')[Column("chars")],
            StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves HM-DEC-091: the columns and their order do not move, and the
    /// operator's own verdict column stays empty whatever the counts say.
    /// </remarks>
    [Theory]
    [InlineData(CwCountsCover.Recording)]
    [InlineData(CwCountsCover.Session)]
    [InlineData(CwCountsCover.NoRecording)]
    public void TheShapeOfTheRowIsUnchanged(CwCountsCover covers)
    {
        var columns = CwCaseRoster.Row(
            Case("cw-2026-08-20-014854.wav", "", covers)).Split('\t');

        Assert.Equal(CwCaseRoster.Header.Split('\t').Length, columns.Length);
        Assert.Equal(string.Empty, columns[Column("read")]);
    }
}
