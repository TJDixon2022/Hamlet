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

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 282, task 3: the log says where the other station was.
/// </summary>
/// <remarks>
/// <para>**BOTH ARE WRITTEN AND ONE WAS SHOWN.** `AdifLog` has written `GRIDSQUARE`
/// and `MY_GRIDSQUARE` as separate fields since unit 274, and the window carried a
/// column for the second and none at all for the first — so a log of contacts said
/// where he was standing every time and never where the station he worked was.</para>
/// <para>**AND THEY ARE LABELLED APART.** A locator with nobody's name on it is a
/// number a reader attaches to whichever station he had in mind.</para>
/// </remarks>
public sealed class TheLogShowsBothGridsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the row is printed.</param>
    public TheLogShowsBothGridsTests(ITestOutputHelper output) => _output = output;

    /// <summary>**A row carries the station's grid and his own, kept apart.**</summary>
    /// <remarks>
    /// Watched failing first: `ContactLogRow` had no `TheirGridSquare` at all, so
    /// this did not compile.
    /// </remarks>
    [Fact]
    public void ARowCarriesBothGrids()
    {
        var row = Row(new AdifContact
        {
            Call = "IK4LZH",
            StationCallsign = "KC3QIS",
            StartedUtc = new DateTime(2026, 9, 7, 21, 41, 30, DateTimeKind.Utc),
            Band = "20m",
            Mode = "FT8",
            GridSquare = "JN54",
            MyGridSquare = "FN00DJ",
        });

        _output.WriteLine("call       : " + row.Call);
        _output.WriteLine("their grid : " + row.TheirGridSquare);
        _output.WriteLine("my grid    : " + row.MyGridSquare);

        Assert.Equal("JN54", row.TheirGridSquare);
        Assert.Equal("FN00DJ", row.MyGridSquare);
    }

    /// <summary>**A grid the record does not carry says so, and is not blank.**</summary>
    /// <remarks>
    /// §0.0. A hole in a table is a hole a reader fills in from habit, and the habit
    /// here would be to assume it was the same square as the column beside it.
    /// </remarks>
    [Fact]
    public void AGridTheRecordDoesNotCarrySaysSo()
    {
        var row = Row(new AdifContact
        {
            Call = "W3YNI",
            StationCallsign = "KC3QIS",
            StartedUtc = new DateTime(2026, 9, 7, 21, 41, 30, DateTimeKind.Utc),
            MyGridSquare = "FN00DJ",
        });

        _output.WriteLine("their grid : " + row.TheirGridSquare);

        Assert.Equal(ContactLogRow.Absent, row.TheirGridSquare);
        Assert.NotEqual("", row.TheirGridSquare);

        // And it is not quietly filled in from the column beside it.
        Assert.NotEqual(row.MyGridSquare, row.TheirGridSquare);
    }

    /// <summary>**The window draws both, under labels that name whose is whose.**</summary>
    [AvaloniaFact]
    public void TheWindowDrawsBothUnderLabelsThatNameWhoseIsWhose()
    {
        File.WriteAllText(
            ContactLogStore.LogPath,
            AdifLog.Header("1.12.183")
            + AdifLog.Record(new AdifContact
            {
                Call = "IK4LZH",
                StationCallsign = "KC3QIS",
                StartedUtc = new DateTime(2026, 9, 7, 21, 41, 30, DateTimeKind.Utc),
                Band = "20m",
                Mode = "FT8",
                GridSquare = "JN54",
                MyGridSquare = "FN00DJ",
            }));

        var window = new ContactLogWindow
        {
            DataContext = new ContactLogViewModel(
                ContactLogStore.ReadRecords(), ContactLogStore.LogPath),
        };

        window.Show();
        Views.HowMuchTheApplicationSaysTests.Pump(window);

        var shown = Views.HowMuchTheApplicationSaysTests.Shown(window).ToList();

        foreach (var t in shown)
        {
            _output.WriteLine("  " + t);
        }

        Assert.Contains("their grid", shown);
        Assert.Contains("my grid", shown);
        Assert.Contains("JN54", shown);
        Assert.Contains("FN00DJ", shown);

        // **NOT ONE COLUMN CALLED `grid`.** The two labels are different strings.
        Assert.Equal(
            2, shown.Count(t => t.EndsWith("grid", StringComparison.Ordinal)));
    }

    /// <summary>One row, through the writer and the reader (§12.5).</summary>
    /// <param name="contact">What was worked.</param>
    /// <returns>The row the window would draw.</returns>
    /// <remarks>
    /// **THROUGH `AdifLog` BOTH WAYS.** A hand-built `AdifLogRecord` would prove the
    /// row reads a property, and the question is whether the field survives the file
    /// — which is where it was being lost.
    /// </remarks>
    private static ContactLogRow Row(AdifContact contact)
    {
        var text = AdifLog.Header("1.12.183") + AdifLog.Record(contact);

        return new ContactLogRow(AdifLog.ReadRecords(text).Single());
    }
}
