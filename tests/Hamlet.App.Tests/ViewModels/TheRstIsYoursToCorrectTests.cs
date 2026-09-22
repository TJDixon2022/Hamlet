using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **Criterion 9.3's last clause: the RST fields are editable, and a heard report and one the
/// operator typed are told apart** - work instruction 390 task 3, under Tim's ruling A of
/// 2026-09-22.
/// </summary>
/// <remarks>
/// <para>**THE RULING, AS IT WAS GIVEN.** RST editable; heard values marked *heard*, typed values
/// marked *yours*; both kept in the record. It supersedes the 2026-09-07 read-only line for the
/// RST fields only - every other field in the dialog is still what Hamlet observed and cannot be
/// typed over.</para>
/// <para>**§0.0 ON THE LOG**: a value Hamlet did not hear is never shown as heard. That is the
/// assertion this type exists for, in every direction a typed value can arrive.</para>
/// <para>**NOTHING IS KEYED AND NO PORT IS OPENED** (FACT-004). The data folder is a temporary
/// one.</para>
/// </remarks>
public sealed class TheRstIsYoursToCorrectTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly string _folder;
    private readonly string _wasFolder;

    /// <summary>Creates the tests and redirects the data folder.</summary>
    /// <param name="output">Where each field is printed.</param>
    public TheRstIsYoursToCorrectTests(ITestOutputHelper output)
    {
        _output = output;
        _wasFolder = SettingsStore.DataFolder;
        _folder = Path.Combine(Path.GetTempPath(), "hamlet-unit390-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_folder);
        SettingsStore.DataFolder = _folder;
    }

    /// <summary>Puts the real folder back.</summary>
    public void Dispose()
    {
        SettingsStore.DataFolder = _wasFolder;

        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (IOException)
        {
        }
    }

    /// <summary>
    /// **A heard 599 is shown and marked; typing 579 replaces it, marks it *yours*, and logs it** -
    /// in the entry, in the ADIF and in the record, which says it was typed.
    /// </summary>
    [Fact]
    public void AHeard599IsShownAndMarkedAndTyping579ReplacesItAndLogsIt()
    {
        var model = new LogContactViewModel(Psk31Contact(sent: "599", received: "599"));
        var received = Report(model, "RST_RCVD");

        _output.WriteLine("before: [" + received.Text + "] " + received.Mark + " (" + received.Source + ")");

        Assert.Equal("599", received.Text);
        Assert.Equal(LogReport.HeardMark, received.Mark);
        Assert.Equal("heard", received.Source);

        received.Text = "579";

        _output.WriteLine("after : [" + received.Text + "] " + received.Mark + " (" + received.Source + ")");

        Assert.Equal(LogReport.YoursMark, received.Mark);
        Assert.Equal("yours", received.Source);
        Assert.NotEqual(LogReport.HeardMark, received.Mark);

        var entry = model.Entry;

        Assert.Equal("579", entry.RstReceived);
        Assert.Equal("599", entry.RstSent);
        Assert.Contains("<RST_RCVD:3>579", AdifLog.Record(entry), StringComparison.Ordinal);

        // **THE RECORD CARRIES WHICH IT WAS.**
        var line = Logged(model);

        _output.WriteLine(line);

        Assert.Contains("\"rstReceived\":\"579\"", line, StringComparison.Ordinal);
        Assert.Contains("\"rstReceivedSource\":\"yours\"", line, StringComparison.Ordinal);
        Assert.Contains("\"rstSentSource\":\"heard\"", line, StringComparison.Ordinal);
    }

    /// <summary>
    /// **A contact with no heard report starts blank and is marked *yours* once typed** - never
    /// *heard*.
    /// </summary>
    [Fact]
    public void WithNoHeardReportItStartsBlankAndIsYoursWhenTyped()
    {
        var model = new LogContactViewModel(Psk31Contact(sent: "599", received: null));
        var received = Report(model, "RST_RCVD");

        _output.WriteLine("before: [" + received.Text + "] " + received.Mark);

        Assert.Equal("", received.Text);
        Assert.Null(received.Source);
        Assert.NotEqual(LogReport.HeardMark, received.Mark);

        // Left blank it stays out of the file, as an unheard field always has.
        Assert.DoesNotContain("RST_RCVD", AdifLog.Record(model.Entry), StringComparison.Ordinal);

        received.Text = "559";

        _output.WriteLine("after : [" + received.Text + "] " + received.Mark);

        Assert.Equal(LogReport.YoursMark, received.Mark);
        Assert.Equal("yours", received.Source);
        Assert.Equal("559", model.Entry.RstReceived);
        Assert.Contains("\"rstReceivedSource\":\"yours\"", Logged(model), StringComparison.Ordinal);
    }

    /// <summary>
    /// **Nothing else in the dialog became editable**, and an FT8 contact's typed report goes to
    /// its decibel field rather than inventing an RST.
    /// </summary>
    [Fact]
    public void OnlyTheTwoReportsAreEditableAndFt8KeepsItsOwnField()
    {
        var ft8 = new AdifContact
        {
            Call = "IK4LZH", Mode = "FT8", ReportSent = "-09", ReportReceived = "-12", GridSquare = "JN54",
        };

        var model = new LogContactViewModel(ft8);

        Assert.Equal(2, model.Fields.Count(f => f.Report is not null));
        Assert.All(
            model.Fields.Where(f => f.Report is not null),
            f => Assert.Contains(f.AdifField, new[] { "RST_SENT", "RST_RCVD" }));

        Report(model, "RST_SENT").Text = "-07";

        Assert.Equal("-07", model.Entry.ReportSent);
        Assert.Null(model.Entry.RstSent);
        Assert.Equal("-12", model.Entry.ReportReceived);
    }

    /// <summary>**On the window the two reports are boxes he can type in, each with its mark.**</summary>
    [AvaloniaFact]
    public void OnTheWindowTheTwoReportsAreBoxesWithTheirMarks()
    {
        var model = new LogContactViewModel(Psk31Contact(sent: "599", received: null));
        var window = new LogContactWindow { DataContext = model };

        window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        try
        {
            // Every row's template carries the box; only the two report rows draw it.
            var boxes = window.GetVisualDescendants().OfType<TextBox>()
                .Where(b => b.Name == "LogReportBox" && b.IsEffectivelyVisible).ToList();
            var marks = window.GetVisualDescendants().OfType<TextBlock>()
                .Where(t => t.Name == "LogReportMark" && t.IsEffectivelyVisible).Select(t => t.Text).ToList();

            foreach (var box in boxes)
            {
                _output.WriteLine("box [" + box.Text + "] read-only " + box.IsReadOnly + ", enabled " + box.IsEffectivelyEnabled);
            }

            _output.WriteLine("marks: " + string.Join(" | ", marks));

            Assert.Equal(2, boxes.Count);
            Assert.All(boxes, b => Assert.False(b.IsReadOnly));
            Assert.All(boxes, b => Assert.True(b.IsEffectivelyEnabled));
            Assert.Equal(new[] { "599", "" }, boxes.Select(b => b.Text ?? ""));
            Assert.Equal(LogReport.HeardMark, marks[0]);
            Assert.NotEqual(LogReport.HeardMark, marks[1]);
        }
        finally
        {
            window.Close();
        }
    }

    // -------------------------------------------------------------------------

    private static AdifContact Psk31Contact(string? sent, string? received) => new()
    {
        Call = "W1AW",
        StationCallsign = "KC3QIS",
        StartedUtc = new DateTime(2026, 9, 21, 17, 44, 0, DateTimeKind.Utc),
        Band = "20m",
        Mode = "PSK",
        Submode = "PSK31",
        RstSent = sent,
        RstReceived = received,
    };

    private static LogReport Report(LogContactViewModel model, string adif)
        => model.Fields.Single(f => f.AdifField == adif).Report!;

    /// <summary>Writes the entry as Save does and returns the record's one logged line.</summary>
    private string Logged(LogContactViewModel model)
    {
        using var telemetry = new JsonlTelemetry(_folder, "390", _ => true);

        var panel = new MainWindowViewModel(new AppSettings { ReconnectOnStartup = false }, telemetry);

        Assert.True(panel.WriteLoggedContactForTests(model.Entry, model.SentSource, model.ReceivedSource));

        telemetry.Dispose();

        return Directory.GetFiles(_folder, "*.jsonl", SearchOption.AllDirectories)
            .SelectMany(File.ReadAllLines)
            .Single(l => l.Contains("psk31_contact_logged", StringComparison.Ordinal));
    }
}
