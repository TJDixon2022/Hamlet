using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Headless.XUnit;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Telemetry;

/// <summary>
/// Work instruction 305 task 5: **when a decode reaches a panel, the record says
/// that it did.**
/// </summary>
/// <remarks>
/// <para>**A SLOT THAT DECODES AND SHOWS NO ROW IS INVISIBLE.** The file could say a
/// slot was cut and what the decoder found, and stopped there - so a message that
/// was filtered out, one swallowed as a duplicate, and one the operator actually
/// read all left the same line.</para>
/// <para>**THE CASE THIS TEST CONSTRUCTS IS THE BAD ONE.** The same slot is offered
/// twice: the first draws rows, the second draws none because every message in it is
/// already on the table, and the second is the line that used to be invisible.</para>
/// </remarks>
public sealed class Unit305DrawnTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the record is printed.</param>
    public Unit305DrawnTests(ITestOutputHelper output) => _output = output;

    /// <summary>**A decode that reaches the table says so, and one that does not.**</summary>
    [AvaloniaFact]
    public void ADecodeThatReachesTheTableSaysSo()
    {
        var slot = new DateTime(2026, 9, 2, 14, 22, 15, DateTimeKind.Utc);

        var heard = new Ft8Reception(
            new[]
            {
                new Ft8Decode(slot, 0.2, 1240, 24, "CQ K1ABC FN42"),
                new Ft8Decode(slot, 0.3, 1310, 21, "CQ W9XYZ EM48"),
            },
            1,
            2,
            "");

        var lines = Session(panel =>
        {
            panel.NoteSlot(heard);

            // **THE SAME SLOT AGAIN**, every message of which is already on the
            // table. It decodes and draws nothing.
            panel.NoteSlot(heard);
        });

        var drawn = lines
            .Where(l => l.Contains("decodes_drawn", StringComparison.Ordinal))
            .ToList();

        foreach (var line in drawn)
        {
            _output.WriteLine("  " + line);
        }

        Assert.Equal(2, drawn.Count);

        // **THE FIRST DREW TWO ROWS.**
        Assert.Contains("\"rowsAdded\":2", drawn[0], StringComparison.Ordinal);
        Assert.Contains("\"reason\":\"drawn\"", drawn[0], StringComparison.Ordinal);

        // **THE SECOND DECODED AND DREW NOTHING, AND SAYS SO IN ITS OWN WORDS.**
        Assert.Contains("\"rowsAdded\":0", drawn[1], StringComparison.Ordinal);

        Assert.Contains(
            "\"reason\":\"decoded_but_no_row_added\"", drawn[1], StringComparison.Ordinal);

        // **AND IT IS A WARNING**, because it is the one worth finding by scanning.
        Assert.Contains("\"level\":\"warn\"", drawn[1], StringComparison.Ordinal);
    }

    /// <summary>**The counts are the screen's, not the decoder's.**</summary>
    /// <remarks>
    /// **THE FILTER IS WHAT SEPARATES THEM** (unit 252's ruling: a filtered row
    /// never reaches the visible table at all). With CQ-only on, a message that is
    /// not a CQ is decoded, is added to the whole table, and is not on the one the
    /// operator is reading - which is three different numbers on one line.
    /// </remarks>
    [AvaloniaFact]
    public void TheCountsAreTheScreensNotTheDecoders()
    {
        var slot = new DateTime(2026, 9, 2, 14, 22, 15, DateTimeKind.Utc);

        var heard = new Ft8Reception(
            new[]
            {
                new Ft8Decode(slot, 0.2, 1240, 24, "CQ K1ABC FN42"),
                new Ft8Decode(slot, 0.3, 1310, 21, "W9XYZ VE7AA -12"),
            },
            1,
            2,
            "");

        var lines = Session(panel =>
        {
            panel.ShowsCqOnly = true;
            panel.NoteSlot(heard);
        });

        var drawn = Assert.Single(
            lines, l => l.Contains("decodes_drawn", StringComparison.Ordinal));

        _output.WriteLine("  " + drawn);

        Assert.Contains("\"decodes\":2", drawn, StringComparison.Ordinal);
        Assert.Contains("\"rowsAdded\":2", drawn, StringComparison.Ordinal);

        // **AND ONE OF THE TWO IS ON THE TABLE HE IS ACTUALLY READING.**
        Assert.Contains("\"rowsOnTheTable\":1", drawn, StringComparison.Ordinal);
    }

    /// <summary>**No message text reaches the record** (§2.1).</summary>
    [AvaloniaFact]
    public void NoMessageTextReachesTheRecord()
    {
        var slot = new DateTime(2026, 9, 2, 14, 22, 15, DateTimeKind.Utc);

        var lines = Session(panel => panel.NoteSlot(new Ft8Reception(
            new[] { new Ft8Decode(slot, 0.2, 1240, 24, "CQ K1ABC FN42") },
            1, 1, "")));

        Assert.DoesNotContain(
            lines, l => l.Contains("K1ABC", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>One session, one act, its telemetry read back.</summary>
    private static List<string> Session(Action<MainWindowViewModel> act)
    {
        var folder = Path.Combine(
            Path.GetTempPath(), "hamlet-unit305r-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(folder);

        try
        {
            using var telemetry = new JsonlTelemetry(folder, "1.12.269", _ => true);

            var settings = new AppSettings { ReconnectOnStartup = false };

            settings.Operator.Callsign = "KC3QIS";

            var panel = new MainWindowViewModel(settings, telemetry)
            {
                OperatingMode = "Digital",
            };

            act(panel);

            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            telemetry.Dispose();

            return Directory.GetFiles(folder, "*.jsonl")
                .SelectMany(File.ReadAllLines)
                .ToList();
        }
        finally
        {
            try
            {
                Directory.Delete(folder, recursive: true);
            }
            catch (IOException)
            {
                // Not this test's business.
            }
        }
    }
}
