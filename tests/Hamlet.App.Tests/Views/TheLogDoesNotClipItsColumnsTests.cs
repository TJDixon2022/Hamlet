using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 282, task 5: the log window stops cutting words in half.
/// </summary>
/// <remarks>
/// <para>**OBSERVED 2026-09-08**: a `sent` column reading `not recorc`. The columns
/// were fixed pixel widths and the widest thing they carry is the twelve-character
/// word this table uses for absent, which does not fit in sixty-four pixels of
/// twelve-point Consolas.</para>
/// <para>**A CLIPPED WORD IS WORSE THAN A NARROW COLUMN** (§0.0). `not recorc` is
/// not a shorter way of saying `not recorded`; it is a string that means nothing,
/// and a reader who has not seen the full word cannot tell it from a value.</para>
/// <para>**THE WIDTHS ARE MEASURED, NOT GUESSED.** This asks each realized cell how
/// wide it wants to be and compares that with the width it was given, which is the
/// only way to know whether a font on this machine fits a column somebody chose on
/// another one.</para>
/// </remarks>
public sealed class TheLogDoesNotClipItsColumnsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the wanted and given widths are printed.</param>
    public TheLogDoesNotClipItsColumnsTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>**Every cell gets at least the width it asked for.**</summary>
    /// <remarks>
    /// Watched failing first: with the columns as unit 274 left them, `sent`,
    /// `rcvd` and `my grid` all wanted more than they were given.
    /// </remarks>
    [AvaloniaFact]
    public void NoCellIsNarrowerThanTheWordItHasToShow()
    {
        var window = Log(Barest());

        var cells = window.GetVisualDescendants().OfType<Grid>()
            .Where(g => g.Name is "ContactLogHeader" or "ContactLogRowCells")
            .SelectMany(g => g.Children.OfType<TextBlock>().Select(t => (Grid: g, Cell: t)))
            .ToList();

        Assert.True(cells.Count > 0, "no log cells were realized");

        var clipped = 0;

        foreach (var (grid, cell) in cells)
        {
            var column = Grid.GetColumn(cell);

            // The notes column is a star and takes what is left; it wraps.
            if (column >= grid.ColumnDefinitions.Count - 1)
            {
                continue;
            }

            // **THE MARGIN IS PART OF WHAT THE CELL ASKED FOR AND NOT PART OF WHAT
            // IT WAS DRAWN IN.** `DesiredSize` includes it and `Bounds` does not, so
            // comparing the two raw is an apples-to-oranges red of the test's own
            // making - which this test produced on its first run.
            var given = cell.Bounds.Width + cell.Margin.Left + cell.Margin.Right;
            var wanted = cell.DesiredSize.Width;

            _output.WriteLine(
                grid.Name!.PadRight(20) + "col " + column
                + "  wants " + wanted.ToString("0.0").PadLeft(6)
                + "  has " + given.ToString("0.0").PadLeft(6)
                + "   " + cell.Text);

            if (wanted > given + 0.5)
            {
                clipped++;
            }
        }

        Assert.True(
            clipped == 0,
            clipped + " cells are narrower than the text they carry, so that text "
            + "is cut off mid-word");
    }

    /// <summary>**The measurement behind the defect, so the number is not a guess.**</summary>
    /// <remarks>
    /// <para>The column that produced `not recorc` was 64 pixels wide. This measures
    /// what the word actually needs, unclamped by any grid, and the answer on this
    /// machine is nearly twice that — which is the evidence that the old widths were
    /// wrong rather than merely tight.</para>
    /// <para>**AND IT IS WHY THE FIX IS NOT A BIGGER NUMBER.** The same word measures
    /// differently under a different font, so a width chosen here would be wrong on
    /// his screen in one direction or the other. The Grid is asked instead of
    /// told.</para>
    /// </remarks>
    [AvaloniaFact]
    public void TheAbsentWordNeverFittedTheColumnItWasGiven()
    {
        var block = new TextBlock
        {
            Text = ContactLogRow.Absent,
            FontFamily = new Avalonia.Media.FontFamily("Consolas,Menlo,monospace"),
            FontSize = 12,
        };

        block.Measure(new Avalonia.Size(double.PositiveInfinity, double.PositiveInfinity));

        _output.WriteLine(
            "\"" + ContactLogRow.Absent + "\" wants "
            + block.DesiredSize.Width.ToString("0.0") + " px; it was given 64");

        Assert.True(
            block.DesiredSize.Width > 64,
            "the absent word fits in 64 px on this machine, so the reported "
            + "truncation has some other cause and this fix is aimed wrongly");
    }

    /// <summary>**The absent word survives whole in every column that shows it.**</summary>
    /// <remarks>
    /// The measurement above is about pixels; this is about the string. `not
    /// recorded` is what this table says when a record carries nothing, and a column
    /// that can only fit part of it says something else.
    /// </remarks>
    [AvaloniaFact]
    public void TheAbsentWordIsNeverCutInHalf()
    {
        var window = Log(Barest());

        var shown = HowMuchTheApplicationSaysTests.Shown(window).ToList();

        _output.WriteLine(string.Join(" | ", shown));

        Assert.Contains(ContactLogRow.Absent, shown);

        // Nothing on the window is a prefix of the word without being the word,
        // which is what a clipped cell would leave in the tree if it clipped by
        // trimming rather than by drawing.
        Assert.DoesNotContain(
            shown,
            t => t.Length < ContactLogRow.Absent.Length
                 && ContactLogRow.Absent.StartsWith(t, StringComparison.Ordinal)
                 && t.Length > 3);
    }

    /// <summary>**The header and the rows still share one column string.**</summary>
    /// <remarks>
    /// Unit 241 wrote a class about this: two copies of a column list drift, and a
    /// header sitting over the wrong cells is worse than either width alone.
    /// </remarks>
    [AvaloniaFact]
    public void TheHeaderAndTheRowsAgreeOnEveryColumn()
    {
        var window = Log(Barest());

        var header = window.GetVisualDescendants().OfType<Grid>()
            .First(g => g.Name == "ContactLogHeader");

        var row = window.GetVisualDescendants().OfType<Grid>()
            .First(g => g.Name == "ContactLogRowCells");

        var headerWidths = header.ColumnDefinitions.Select(c => c.ActualWidth).ToList();
        var rowWidths = row.ColumnDefinitions.Select(c => c.ActualWidth).ToList();

        _output.WriteLine("header : " + string.Join(", ", headerWidths.Select(w => w.ToString("0"))));
        _output.WriteLine("rows   : " + string.Join(", ", rowWidths.Select(w => w.ToString("0"))));

        Assert.Equal(headerWidths.Count, rowWidths.Count);

        for (var i = 0; i < headerWidths.Count; i++)
        {
            Assert.True(
                Math.Abs(headerWidths[i] - rowWidths[i]) < 0.5,
                "column " + i + " is " + headerWidths[i] + " in the header and "
                + rowWidths[i] + " in the rows");
        }
    }

    /// <summary>A contact with nothing in it but a callsign and a time.</summary>
    /// <returns>The record text.</returns>
    /// <remarks>
    /// **THE WORST CASE IS THE EMPTY ONE**, because every unfilled column then
    /// carries the twelve-character absent word rather than a three-character band.
    /// </remarks>
    private static string Barest()
        => AdifLog.Header("1.12.185")
           + AdifLog.Record(new AdifContact
           {
               Call = "IK4LZH",
               StationCallsign = "KC3QIS",
               StartedUtc = new DateTime(2026, 9, 7, 21, 41, 30, DateTimeKind.Utc),
           });

    /// <summary>The log window, realized, over a given file.</summary>
    /// <param name="adif">The file's contents.</param>
    /// <returns>The shown window.</returns>
    private static Window Log(string adif)
    {
        File.WriteAllText(ContactLogStore.LogPath, adif);

        var window = new ContactLogWindow
        {
            DataContext = new ContactLogViewModel(
                ContactLogStore.ReadRecords(), ContactLogStore.LogPath),
        };

        window.Show();
        HowMuchTheApplicationSaysTests.Pump(window);

        return window;
    }
}
