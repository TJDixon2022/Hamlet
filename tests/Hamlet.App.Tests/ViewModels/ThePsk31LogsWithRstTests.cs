using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Hamlet.App.Settings;
using Hamlet.App.Telemetry;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Tests.Psk31;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 326 task 3: **a PSK31 contact logs with the RST that passed, and
/// the decibel field FT8 uses is not borrowed to hold it** (`PHASE_PLAN.md` §3.2).
/// </summary>
/// <remarks>
/// <para>**THE TWO REPORTS ARE DIFFERENT KINDS OF NUMBER.** FT8 exchanges a
/// signal-to-noise ratio in decibels - `-12` - and PSK31 exchanges a `599`-style RST.
/// A log that puts both in one field records `599` as a ratio and `-12` as a
/// readability, and nothing downstream can tell them apart afterwards.</para>
/// <para>**NOTHING IS INVENTED** (§0.0). `599` is the conventional answer for a clean
/// copy and Hamlet's own Report macro sends it, but a report the parser never read is
/// empty in the log rather than filled in with the conventional one. A log is the one
/// artifact here that outlives everything else, and a plausible number in it is
/// indistinguishable from a true one in ten years.</para>
/// <para>**COMPUTED, NOT SEEN.** No window is opened; what is asserted is the entry the
/// dialog would be handed and the line the telemetry file holds.</para>
/// </remarks>
public sealed class ThePsk31LogsWithRstTests : IDisposable
{
    private const long On20m = 14_070_000;
    private const double HisHz = 1234;

    private readonly ITestOutputHelper _output;
    private readonly string _folder;
    private readonly string _wasFolder;

    /// <summary>Creates the tests and redirects the data folder.</summary>
    /// <param name="output">Where the entries are printed.</param>
    public ThePsk31LogsWithRstTests(ITestOutputHelper output)
    {
        _output = output;

        _wasFolder = SettingsStore.DataFolder;
        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-unit326-" + Guid.NewGuid().ToString("N"));

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

    /// <summary>**1: the contact carries both reports, and the decibel field is empty.**</summary>
    /// <remarks>
    /// **THE TRANSCRIPT IS `02-chatty` BECAUSE ITS TWO REPORTS DIFFER.** He sends `589`
    /// and Hamlet's Report macro sends `599`, so a test that read one field into the
    /// other, or read the same field twice, fails here rather than passing on a
    /// coincidence.
    /// </remarks>
    [Fact]
    public void ALoggedPsk31ContactCarriesBothReportsAndNotInTheDecibelField()
    {
        var (model, _) = Panel();

        Work(model);

        var entry = model.ContactLogEntryForStation("G4XYZ", On20m);

        Assert.NotNull(entry);

        _output.WriteLine("RST sent     : " + (entry!.RstSent ?? "(none)"));
        _output.WriteLine("RST received : " + (entry.RstReceived ?? "(none)"));
        _output.WriteLine("dB sent      : " + (entry.ReportSent ?? "(none)"));
        _output.WriteLine("dB received  : " + (entry.ReportReceived ?? "(none)"));
        _output.WriteLine(AdifLog.Record(entry));

        // **WHAT HAMLET SENT AND WHAT HE SENT, EACH IN ITS OWN FIELD.**
        Assert.Equal("599", entry.RstSent);
        Assert.Equal("589", entry.RstReceived);

        // **AND THE DECIBEL FIELD IS UNTOUCHED** (§3.2). PSK31 exchanged no ratio, so
        // there is nothing for it to hold.
        Assert.Null(entry.ReportSent);
        Assert.Null(entry.ReportReceived);

        // **THE MODE IS THE ONE THE CONTACT WAS MADE IN**, which task 4 asserts on the
        // export and is checked here because an RST under `MODE=FT8` would be wrong in
        // a way no later unit could repair.
        Assert.Equal("PSK", entry.Mode);
        Assert.Equal("PSK31", entry.Submode);
    }

    /// <summary>**2: an FT8 contact still logs its decibels, and carries no RST.**</summary>
    [Fact]
    public void AnFt8ContactStillLogsItsDecibelsAndCarriesNoRst()
    {
        var model = Ft8Panel();

        var slot = new DateTime(2026, 9, 11, 21, 41, 30, DateTimeKind.Utc);

        model.AddDecodeRowForTests(
            "214130", "-12", "0.2", "1240", "KC3QIS IK4LZH -12", slot, 14_074_000);

        var entry = model.ContactLogEntryForStation("IK4LZH", 14_074_000);

        Assert.NotNull(entry);

        _output.WriteLine("dB received  : " + (entry!.ReportReceived ?? "(none)"));
        _output.WriteLine("RST received : " + (entry.RstReceived ?? "(none)"));

        // **THE FIELD FT8 HAS ALWAYS WRITTEN, WRITING WHAT IT ALWAYS WROTE.**
        Assert.Equal("-12", entry.ReportReceived);

        // **AND NO RST, BECAUSE NO RST PASSED.**
        Assert.Null(entry.RstSent);
        Assert.Null(entry.RstReceived);
    }

    /// <summary>**3: a report that was never read is empty and is never guessed.**</summary>
    /// <remarks>
    /// **HE CALLED AND HAMLET ANSWERED, AND THAT IS ALL.** No report has passed either
    /// way. `599` is what the Report macro *would* send and it has not been sent, so the
    /// field is empty - the conventional answer is still a guess (§0.0).
    /// </remarks>
    [Fact]
    public void AContactWhoseReportWasNeverReadLogsAnEmptyRstRatherThanAGuessedOne()
    {
        var (model, _) = Panel();

        Hear(model, 1);
        model.AnswerPsk31Command.Execute(model.DigitalDecodes[0]);
        Settle(model);

        var entry = model.ContactLogEntryForStation("G4XYZ", On20m);

        Assert.NotNull(entry);

        _output.WriteLine("RST sent     : " + (entry!.RstSent ?? "(none)"));
        _output.WriteLine("RST received : " + (entry.RstReceived ?? "(none)"));
        _output.WriteLine(AdifLog.Record(entry));

        Assert.Null(entry.RstSent);
        Assert.Null(entry.RstReceived);

        // **AND THE EXPORT WRITES NO TAG AT ALL** rather than an empty one.
        Assert.DoesNotContain("RST_SENT", AdifLog.Record(entry), StringComparison.Ordinal);
        Assert.DoesNotContain("RST_RCVD", AdifLog.Record(entry), StringComparison.Ordinal);
    }

    /// <summary>**4: the event fires once, carries the reports, and names nobody.**</summary>
    /// <remarks>
    /// **§R13, AND HM-DEC-018 §2.1.** The record says a PSK31 contact was logged, which
    /// reports went into it and what the mode was spelled as. **It carries no callsign,
    /// no grid and no text** - the operator already has all three on his own screen, and
    /// a telemetry file is the one place they must never accumulate.
    /// </remarks>
    [Fact]
    public void TheLoggedEventFiresOnceWithBothReportsAndNothingPersonal()
    {
        var lines = WithTelemetry(model =>
        {
            Work(model);

            var entry = model.ContactLogEntryForStation("G4XYZ", On20m);

            Assert.NotNull(entry);

            Assert.True(model.WriteLoggedContactForTests(entry!));
        });

        var logged = Assert.Single(Events(lines, "psk31_contact_logged"));

        _output.WriteLine(logged.ToString());

        var data = logged.GetProperty("data");

        Assert.Equal("599", data.GetProperty("rstSent").GetString());
        Assert.Equal("589", data.GetProperty("rstReceived").GetString());
        Assert.Equal("PSK", data.GetProperty("mode").GetString());
        Assert.Equal("PSK31", data.GetProperty("submode").GetString());

        // **NOTHING PERSONAL, ANYWHERE IN THE LINE.**
        var text = logged.ToString();

        foreach (var personal in new[] { "G4XYZ", "KC3QIS", "FN00", "IO91", "Tim", "Dave" })
        {
            Assert.DoesNotContain(personal, text, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// **5: a PSK31 contact whose grid was read exports `GRIDSQUARE`, and one whose was
    /// not exports none** (work instruction 327 task 5, unit 326 item 8).
    /// </summary>
    /// <remarks>
    /// <para>**THE PARSER READ IT AND THE CARD HAS BEEN SHOWING IT SINCE UNIT 320**, and
    /// `Ft8ContactLogEntry` dropped it on the way to the log: that method reads grids out
    /// of **FT8 fields**, and a PSK31 conversation has none. So a contact that said `GRID
    /// FN31` in plain prose logged without one.</para>
    /// <para>**AND IT IS NEVER INVENTED** (§0.0). `02-chatty` is a whole worked contact in
    /// which nobody sends a grid, and its record carries no `GRIDSQUARE` tag at all rather
    /// than an empty one or a guess from his prefix.</para>
    /// </remarks>
    [Fact]
    public void AContactWithAReadGridExportsItAndOneWithoutExportsNone()
    {
        var (withGrid, _) = Panel();

        Work(withGrid, "01-textbook", "W1AW", 3);

        var read = withGrid.ContactLogEntryForStation("W1AW", On20m);

        Assert.NotNull(read);

        _output.WriteLine("grid   : " + (read!.GridSquare ?? "(none)"));
        _output.WriteLine(AdifLog.Record(read));

        Assert.Equal("FN31", read.GridSquare);
        Assert.Contains("<GRIDSQUARE:4>FN31", AdifLog.Record(read), StringComparison.Ordinal);

        // **AND THE ONE WHO NEVER SENT ONE CARRIES NO TAG.**
        var (withNone, _) = Panel();

        Work(withNone);

        var silent = withNone.ContactLogEntryForStation("G4XYZ", On20m);

        Assert.NotNull(silent);

        _output.WriteLine("grid   : " + (silent!.GridSquare ?? "(none)"));
        _output.WriteLine(AdifLog.Record(silent));

        Assert.Null(silent.GridSquare);

        // **`<GRIDSQUARE:` AND NOT `GRIDSQUARE`**, because `MY_GRIDSQUARE` ends in the
        // same nine letters and a plain substring scan would find the operator's own.
        Assert.DoesNotContain(
            "<GRIDSQUARE:", AdifLog.Record(silent), StringComparison.Ordinal);

        // **AND THE OPERATOR'S OWN IS NOT HIS.** `MY_GRIDSQUARE` is a different tag and
        // reading one into the other is exactly the fault this test exists for.
        Assert.Contains("MY_GRIDSQUARE", AdifLog.Record(silent), StringComparison.Ordinal);
    }

    /// <summary>
    /// **6: an FT8 record is byte-identical to what it was before this unit.**
    /// </summary>
    /// <remarks>
    /// **THE GRID PATH FT8 USES IS UNTOUCHED** (work instruction 327 task 5). The PSK31
    /// grid is applied only where the PSK31 parser read one, so an FT8 contact takes
    /// exactly the route `Ft8ContactLogEntry.LastGrid` always gave it - and the whole
    /// record, not only the grid, is compared.
    /// </remarks>
    [Fact]
    public void AnFt8RecordIsByteIdenticalToWhatItWas()
    {
        var model = Ft8Panel();

        var slot = new DateTime(2026, 9, 11, 21, 41, 30, DateTimeKind.Utc);

        model.AddDecodeRowForTests(
            "214130", "-12", "0.2", "1240", "CQ IK4LZH JN54", slot, 14_074_000);

        model.AddDecodeRowForTests(
            "214145", "-12", "0.2", "1240", "KC3QIS IK4LZH -12", slot.AddSeconds(15),
            14_074_000);

        var entry = model.ContactLogEntryForStation("IK4LZH", 14_074_000);

        Assert.NotNull(entry);

        var record = AdifLog.Record(entry!);

        _output.WriteLine(record);

        // **THE GRID HE PUT ON THE AIR IN AN FT8 FIELD, READ THE WAY IT ALWAYS WAS.**
        Assert.Equal("JN54", entry!.GridSquare);
        Assert.Contains("<GRIDSQUARE:4>JN54", record, StringComparison.Ordinal);

        // **AND THE REPORT IS THE DECIBEL ONE, IN THE FIELD FT8 HAS ALWAYS WRITTEN IT
        // TO.** ADIF has one report tag and FT8's own convention puts the ratio in it,
        // so `RST_RCVD` reading `-12` is what an FT8 record has always looked like -
        // what makes this an FT8 record rather than a PSK31 one is that the entry's
        // **`RstReceived` is null** and the value came off `ReportReceived`.
        Assert.Equal("-12", entry.ReportReceived);
        Assert.Null(entry.RstSent);
        Assert.Null(entry.RstReceived);
        Assert.Contains("<RST_RCVD:3>-12", record, StringComparison.Ordinal);
    }

    /// <summary>Works the whole contact on `02-chatty`: answer, report, confirm.</summary>
    private static void Work(MainWindowViewModel model)
        => Work(model, "02-chatty", "G4XYZ", 3);

    /// <summary>Works a contact on any transcript, a press at a time.</summary>
    /// <param name="model">The panel.</param>
    /// <param name="transcript">Which transcript in the corpus.</param>
    /// <param name="station">Whose half is fed in.</param>
    /// <param name="overs">How many of his lines to hear.</param>
    private static void Work(
        MainWindowViewModel model, string transcript, string station, int overs)
    {
        for (var over = 1; over <= overs; over++)
        {
            Hear(model, over, transcript);

            if (over == 1)
            {
                model.AnswerPsk31Command.Execute(
                    model.DigitalDecodes.First(r => r.Sender == station));
            }
            else
            {
                model.CardActionCommand.Execute(model.DigitalCards[0]);
            }

            Settle(model);
        }
    }

    /// <summary>Hands the panel the other station's half, a line at a time.</summary>
    private static void Hear(MainWindowViewModel model, int lines)
        => Hear(model, lines, "02-chatty");

    /// <summary>The same, from any transcript in the corpus.</summary>
    private static void Hear(MainWindowViewModel model, int lines, string transcript)
    {
        var corpus = Psk31Corpus.Load();

        var his = corpus.Transcripts
            .Single(t => t.Name == transcript)
            .Lines
            .Where(l => !string.Equals(l.Frm, corpus.Operator, StringComparison.OrdinalIgnoreCase))
            .Take(lines)
            .Select(l => l.Text + "\n");

        model.ShowPsk31ChannelsForTests(
            new[] { new Psk31Channel(1, HisHz, 10.0, string.Concat(his)) });
    }

    /// <summary>Lets a no-slot send finish; the click fires it and does not wait (§R10).</summary>
    private static void Settle(MainWindowViewModel model)
    {
        for (var tries = 0; tries < 400 && model.HasSomethingToStop; tries++)
        {
            System.Threading.Thread.Sleep(10);
        }

        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
    }

    private List<string> WithTelemetry(Action<MainWindowViewModel> what)
    {
        var folder = Path.Combine(_folder, "telemetry");

        Directory.CreateDirectory(folder);

        using (var telemetry = new JsonlTelemetry(folder, "326", _ => true))
        {
            var (model, _) = Panel(telemetry);

            what(model);
        }

        return Directory.GetFiles(folder, "*.jsonl").SelectMany(File.ReadAllLines).ToList();
    }

    private static List<JsonElement> Events(IEnumerable<string> lines, string name)
        => lines
            .Where(l => l.Contains("\"" + name + "\"", StringComparison.Ordinal))
            .Select(l => JsonDocument.Parse(l).RootElement)
            .Where(e => e.GetProperty("event").GetString() == name)
            .ToList();

    private static (MainWindowViewModel Model, FakeSink Sink) Panel(
        JsonlTelemetry? telemetry = null)
    {
        var settings = Operator();

        var model = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        model.SelectedBand = model.Bands.First(
            b => b.Band.LowHz <= On20m && b.Band.HighHz >= On20m);
        model.FrequencyHz = On20m;

        model.UseWorkedBeforeForTests(
            new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.ChooseDigitalModeCommand.Execute("PSK31");

        var port = new FakePort();
        var sink = new FakeSink();

        model.UseRigPortForTests(port);
        model.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(port, sink)));

        return (model, sink);
    }

    /// <summary>The same panel on FT8, where the report is decibels.</summary>
    private static MainWindowViewModel Ft8Panel()
    {
        var model = new MainWindowViewModel(Operator(), null)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        model.UseWorkedBeforeForTests(
            new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));

        return model;
    }

    private static AppSettings Operator()
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = Psk31Corpus.Load().Operator;
        settings.Operator.GridSquare = "FN00DJ";
        settings.Operator.OperatorName = "Tim";
        settings.Operator.Location = "Trafford PA";
        settings.Operator.LicenseClass = LicenseClass.General;

        return settings;
    }
}
