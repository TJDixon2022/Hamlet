using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Avalonia.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 378 task 2: **R39's seven lines, read from a file, offered on every row that
/// names a station, and sent through the one unslotted door** (step 7, criteria 7.1, 7.2, 7.4).
/// </summary>
/// <remarks>
/// <para>**THE SEVEN COME FROM THE FILE AND NOT FROM HERE** (7.1). Every label and every line
/// this asserts is read out of `data/psk31/canned.json` at the moment it is asserted, so a file
/// Tim edits is a file this test reads; what is pinned is R39's own order and his own labels,
/// which is what the criterion names.</para>
/// <para>**NOTHING IS KEYED AND NO PORT IS OPENED** (FACT-004). The one send driven here goes
/// over `FakePort` and `FakeSink`, as every send test in this project has since unit 260.</para>
/// </remarks>
public sealed class TheCannedListIsOfferedTests : IDisposable
{
    private const string His = "W1AW";

    private const string Mine = "KC3QIS";

    private const long DialOn20m = 14_071_500;

    /// <summary>R39's seven, in his order and his words (PHASE_PLAN.md R39, ruled C).</summary>
    private static readonly string[] Ruled =
    {
        "Answer him",
        "Send my report",
        "Confirm and 73",
        "Say again?",
        "Please repeat your report",
        "QRZ?",
        "73 and out",
    };

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the menu is printed.</param>
    public TheCannedListIsOfferedTests(ITestOutputHelper output)
    {
        _output = output;
        _folder = Path.Combine(Path.GetTempPath(), "hamlet-canned-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_folder);
    }

    /// <summary>Removes the temporary folder.</summary>
    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, true);
        }
        catch (IOException)
        {
            // A left-over temp folder is not a test failure.
        }
    }

    /// <summary>**7.1: the shipped file carries R39's seven, in his order, documented in itself.**</summary>
    [Fact]
    public void TheShippedFileCarriesTheSevenInTheOrderTimRuledAndSaysWhatARowIs()
    {
        var set = Psk31CannedLines.ReadFrom(Psk31CannedLines.ShippedPath);

        _output.WriteLine("read " + set.Path);

        Assert.Null(set.Problem);
        Assert.True(set.IsUsable);
        Assert.Equal(Ruled, set.Lines.Select(l => l.Label).ToArray());

        // **THE FIRST THREE NAME HAMLET'S OWN MACROS AND THE LAST FOUR CARRY TEXT** (section 6
        // ruling 1 item 5). Writing an answer beside the one Hamlet already sends is two
        // spellings of one act, which was unit 377's whole subject.
        Assert.Equal(Psk31CannedMacro.Answer, set.Lines[0].Macro);
        Assert.Equal(Psk31CannedMacro.Report, set.Lines[1].Macro);
        Assert.Equal(Psk31CannedMacro.Confirm, set.Lines[2].Macro);

        foreach (var line in set.Lines.Take(3))
        {
            Assert.Equal("", line.Text);
            Assert.False(line.IsText);
        }

        Assert.Equal(4, set.Lines.Count(l => l.IsText));

        // **THE FORMAT IS DOCUMENTED AT THE TOP OF THE FILE ITSELF** (7.1, ruling 1 item 3), the
        // way `assets/data/rsid-codes.json` carries its own `_note`: JSON has no comments and a
        // README nobody opens is not documentation.
        using var document = JsonDocument.Parse(File.ReadAllText(Psk31CannedLines.ShippedPath));

        Assert.True(document.RootElement.TryGetProperty("_note", out var note));

        var said = string.Join(
            "\n", note.EnumerateArray().Select(l => l.GetString() ?? ""));

        _output.WriteLine(said);

        // It says what a row is, where his own copy goes, and that a line goes out framed.
        Assert.Contains("\"label\"", said, StringComparison.Ordinal);
        Assert.Contains("\"macro\"", said, StringComparison.Ordinal);
        Assert.Contains("\"text\"", said, StringComparison.Ordinal);
        Assert.Contains(Psk31CannedLines.OperatorFileName, said, StringComparison.Ordinal);
        Assert.Contains("%AppData%", said, StringComparison.Ordinal);
        Assert.Contains("hand-back", said, StringComparison.Ordinal);
        Assert.Contains("Hamlet never writes", said, StringComparison.Ordinal);
    }

    /// <summary>**7.1: every shipped line is one PSK31 can send, whole, inside the cap.**</summary>
    /// <remarks>
    /// **A CANNED LINE THAT LOST CHARACTERS TO THE VARICODE WOULD BE HAMLET SHIPPING A LINE IT
    /// CANNOT SEND** (the instruction, task 2). Each is run through `Psk31Macros.Sendable` and
    /// then framed and measured against the cap that will actually apply to it.
    /// </remarks>
    [Fact]
    public void EveryShippedLineSurvivesTheVaricodeAndFitsThePsk31Cap()
    {
        var set = Psk31CannedLines.ReadFrom(Psk31CannedLines.ShippedPath);

        foreach (var line in set.Lines)
        {
            var framed = line.IsText
                ? Psk31Macros.Typed(His, Mine, line.Text)
                : Composed(line.Macro);

            var (clean, dropped) = Psk31Macros.Sendable(line.Text.Length > 0 ? line.Text : framed);
            var seconds = Psk31Modulator.SecondsFor(framed);

            _output.WriteLine(
                line.Label.PadRight(26) + framed.Length + " characters, " + dropped
                + " dropped, " + seconds.ToString("0.00") + " s against "
                + OperatorSend.LongestUnslottedSeconds + " s");

            Assert.Equal(0, dropped);
            Assert.NotEqual(0, clean.Length);
            Assert.True(
                seconds <= OperatorSend.LongestUnslottedSeconds,
                line.Label + " is " + seconds.ToString("0.00") + " s and the cap is "
                + OperatorSend.LongestUnslottedSeconds);
        }
    }

    /// <summary>**7.1 and 7.4: every row that names a station offers the seven, on PSK31 and on Olivia.**</summary>
    /// <remarks>
    /// **THE HANDLER'S OWN ORDER IS DRIVEN** - the card is opened first and the menu built
    /// afterwards, which is what `OnDecodedRowContextRequested` does (R29) - so what is measured
    /// is the menu a right-click actually produces.
    /// </remarks>
    [Fact]
    public void EveryRowThatNamesAStationOffersTheSevenOnBothModes()
    {
        foreach (var mode in new[] { "PSK31", "Olivia" })
        {
            var model = Panel(mode);

            Show(model, mode);

            var rows = model.DigitalDecodes.Where(r => r.IsTextOnly).ToList();
            var withAMenu = 0;
            var named = 0;

            Assert.NotEmpty(rows);

            foreach (var row in rows)
            {
                var station = model.Psk31StationOn(row);

                model.CardsNowForTests = DateTime.UtcNow;
                model.OpenPsk31CardCommand.Execute(row);

                var flyout = MainWindow.SendFlyoutFor(model, row);

                _output.WriteLine(
                    mode + "  " + (station ?? "(no station)").PadRight(10)
                    + (flyout is null ? "no menu" : flyout.Items.OfType<MenuItem>().Count() + " entries"));

                if (station is null)
                {
                    // **A ROW THAT NAMES NOBODY STILL OFFERS NOTHING**, which is what it did
                    // before and is the honest state: there is no station to send anything to.
                    Assert.Null(flyout);

                    continue;
                }

                named++;

                Assert.NotNull(flyout);

                var entries = flyout!.Items.OfType<MenuItem>().ToList();

                withAMenu++;

                // **SEVEN ENTRIES, IN THE FILE'S ORDER, WITH R39's LABELS.** An entry Hamlet
                // cannot do right now is a note saying why, never a missing row and never a
                // row drawn grey.
                Assert.Equal(7, entries.Count);

                for (var at = 0; at < Ruled.Length; at++)
                {
                    var header = entries[at].Header as string ?? "";

                    Assert.StartsWith(Ruled[at], header, StringComparison.Ordinal);

                    if (entries[at].Command is null)
                    {
                        Assert.Contains(" - not offered: ", header, StringComparison.Ordinal);
                        Assert.False(entries[at].IsHitTestVisible);
                    }
                    else
                    {
                        // **NOTHING IS GREYED, HIDDEN OR DISABLED** (ruled 2026-09-06).
                        Assert.True(entries[at].IsHitTestVisible);
                        Assert.True(entries[at].IsEnabled);
                        Assert.Equal(Ruled[at], header);
                    }
                }

                // **THE THREE MACRO ROWS CARRY THE COMMANDS THAT SEND THEM TODAY** and the four
                // text rows carry the canned send, each with its own line beside it.
                foreach (var (entry, line) in entries.Zip(model.CannedLines.Lines))
                {
                    if (entry.Command is null)
                    {
                        continue;
                    }

                    if (line.IsText)
                    {
                        Assert.Same(model.SendCannedPsk31Command, entry.Command);

                        var press = Assert.IsType<Psk31CannedPress>(entry.CommandParameter);

                        Assert.Equal(station, press.Station);
                        Assert.Equal(line.Label, press.Label);
                        Assert.Equal(line.Text, press.Text);
                    }
                    else if (line.Macro == Psk31CannedMacro.Answer)
                    {
                        Assert.Same(model.AnswerPsk31Command, entry.Command);
                        Assert.Same(row, entry.CommandParameter);
                    }
                    else
                    {
                        Assert.Same(model.CardActionCommand, entry.Command);
                        Assert.IsType<Ft8ContactCard>(entry.CommandParameter);
                    }
                }

                // **NO FT8 OPTION EVER APPEARS ON ONE OF THESE ROWS.** The FT8 options are
                // message shapes packed into 77 bits and none of them is a thing to send here.
                Assert.Null(model.SendMenuFor(row));
                Assert.DoesNotContain(entries, i => ReferenceEquals(i.Command, model.SendMessageCommand));
            }

            _output.WriteLine(mode + ": " + named + " rows name a station, " + withAMenu + " have a menu");

            Assert.True(named > 0, mode + " gave no row that names a station");
            Assert.Equal(named, withAMenu);
        }
    }

    /// <summary>**7.1: a malformed file is reported and never guessed - one note and no canned item.**</summary>
    [Theory]
    [InlineData("{ \"lines\": [ { \"label\": \"Answer him\", \"macro\": \"answer\", \"text\": \"HI\" } ] }", "one or the other")]
    [InlineData("{ \"lines\": [ { \"label\": \"Answer him\" } ] }", "carries neither")]
    [InlineData("{ \"lines\": [ { \"label\": \"Answer him\", \"macro\": \"grovel\" } ] }", "Hamlet has three")]
    [InlineData("{ \"lines\": [ { \"macro\": \"answer\" } ] }", "has no label")]
    [InlineData("{ \"lines\": [] }", "carries no lines at all")]
    [InlineData("{ \"note\": \"nothing here\" }", "no \"lines\" array")]
    [InlineData("{ this is not json", "could not read")]
    public void AMalformedFileIsSaidAndNothingIsOffered(string body, string expected)
    {
        var path = Path.Combine(_folder, Guid.NewGuid().ToString("N") + ".json");

        File.WriteAllText(path, body);

        var set = Psk31CannedLines.ReadFrom(path);

        _output.WriteLine("[" + body + "]  ->  " + set.Problem);

        // **NO PARTIAL READ, NO SKIPPED ROW AND NO SILENT FALLBACK TO THE SHIPPED SEVEN.**
        Assert.NotNull(set.Problem);
        Assert.False(set.IsUsable);
        Assert.Empty(set.Lines);
        Assert.Contains(expected, set.Problem!, StringComparison.OrdinalIgnoreCase);

        // **THE SENTENCE NAMES THE FILE**, because *could not be read* on its own is a sentence
        // nobody can act on.
        Assert.Contains(path, set.Problem!, StringComparison.Ordinal);

        var model = Panel("PSK31");

        Show(model, "PSK31");
        model.UseCannedLinesForTests(set);

        var row = model.DigitalDecodes.First(r => model.Psk31StationOn(r) is not null);
        var flyout = MainWindow.SendFlyoutFor(model, row);
        var only = Assert.Single(flyout!.Items.OfType<MenuItem>());

        _output.WriteLine("the menu says: " + (only.Header as string ?? ""));

        // **ONE NOTE, AND NO CANNED ITEM AT ALL.** A note carries no command and cannot be hit.
        Assert.Null(only.Command);
        Assert.False(only.IsHitTestVisible);
        Assert.Contains("could not read its canned lines", only.Header as string ?? "", StringComparison.Ordinal);
        Assert.Contains(set.Problem!, only.Header as string ?? "", StringComparison.Ordinal);
    }

    /// <summary>**7.1 and 7.2: his own copy wins, and Hamlet writes neither file.**</summary>
    [Fact]
    public void HisOwnCopyWinsAndHamletWritesNeitherFile()
    {
        // **THE OPERATOR'S PATH IS THE ONE FOLDER HAMLET WRITES TO**, beside settings.json.
        Assert.Equal(
            Path.Combine(SettingsStore.DataFolder, "canned.json"),
            Psk31CannedLines.OperatorPath);

        Assert.EndsWith(
            Path.Combine("data", "psk31", "canned.json"),
            Psk31CannedLines.ShippedPath,
            StringComparison.OrdinalIgnoreCase);

        // **AND READING DOES NOT CREATE IT.** Hamlet does not copy the shipped file there and
        // does not repair it: he copies one file and edits it.
        var before = File.Exists(Psk31CannedLines.OperatorPath);

        Psk31CannedLines.Read();

        Assert.Equal(before, File.Exists(Psk31CannedLines.OperatorPath));

        // **A COPY OF HIS OWN IS WHAT IS READ**, proved on a named file rather than by writing
        // into the folder the operator's real copy would live in.
        var his = Path.Combine(_folder, "canned.json");

        File.WriteAllText(
            his, "{ \"lines\": [ { \"label\": \"Tim's own\", \"text\": \"GM OM\" } ] }");

        var mine = Psk31CannedLines.ReadFrom(his);

        Assert.Null(mine.Problem);
        Assert.Equal("Tim's own", Assert.Single(mine.Lines).Label);
        Assert.Equal("GM OM", mine.Lines[0].Text);
    }

    /// <summary>
    /// **7.2: one click on a canned line goes out framed, announced, capped, and recorded with
    /// no text - through the one unslotted door and no second one.**
    /// </summary>
    [Fact]
    public void OneClickSendsTheLineFramedAnnouncedCappedAndRecordedWithNoText()
    {
        FakeSink sink;
        FakePort port;
        string sent;
        string line;

        using (var telemetry = new JsonlTelemetry(_folder, "378", _ => true))
        {
            var model = Panel("PSK31", telemetry);

            Show(model, "PSK31");

            port = new FakePort();
            sink = new FakeSink();

            model.UseRigPortForTests(port);
            model.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(port, sink, guard: null, telemetry)));

            var row = model.DigitalDecodes.First(r => model.Psk31StationOn(r) == His);

            model.CardsNowForTests = DateTime.UtcNow;
            model.OpenPsk31CardCommand.Execute(row);

            var flyout = MainWindow.SendFlyoutFor(model, row);
            var item = flyout!.Items.OfType<MenuItem>()
                .First(i => i.CommandParameter is Psk31CannedPress);

            var press = (Psk31CannedPress)item.CommandParameter!;

            sent = Psk31Macros.Typed(press.Station, Mine, press.Text);

            _output.WriteLine("clicked   : " + (item.Header as string ?? ""));
            _output.WriteLine("which sends: \"" + sent + "\"");

            item.Command!.Execute(item.CommandParameter);

            for (var tries = 0; tries < 2000 && model.HasSomethingToStop; tries++)
            {
                Thread.Sleep(10);
            }

            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            line = model.DigitalSendLine;
        }

        var lines = Directory.GetFiles(_folder, "*.jsonl").SelectMany(File.ReadAllLines).ToList();
        var composed = Assert.Single(Events(lines, "psk31_send_composed"));
        foreach (var action in Events(lines, "operator_action"))
        {
            _output.WriteLine("action   : " + action.GetRawText());
        }

        var pressed = Assert.Single(
            Events(lines, "operator_action"),
            e => e.TryGetProperty("action", out var what)
                && what.GetString() == "psk31_canned_pressed");

        _output.WriteLine("composed : " + composed.GetRawText());
        _output.WriteLine("pressed  : " + pressed.GetRawText());
        _output.WriteLine("the panel says: " + line);

        // **THE FRAME IS R39's OWN, ALREADY WRITTEN**: both callsigns in front, the hand-back
        // behind. It is `Psk31Macros.Typed` and there is no second frame.
        Assert.StartsWith(His + " de " + Mine, sent, StringComparison.Ordinal);
        Assert.EndsWith("BTU " + His + " de " + Mine + " K", sent, StringComparison.Ordinal);

        // **THE TOKEN IS `canned`, AND THE CAP IS THE MACRO'S THIRTY AND NOT THE TYPED SIXTY.**
        Assert.Equal("canned", composed.GetProperty("macro").GetString());
        Assert.Equal(OperatorSend.LongestUnslottedSeconds, composed.GetProperty("capSeconds").GetDouble());
        Assert.True(composed.GetProperty("withinCap").GetBoolean());

        // **IT BEGAN WITH ITS RSID BURST** (R27), inherited from the one door.
        Assert.True(composed.GetProperty("announced").GetBoolean());
        Assert.Equal(
            OliviaData.Current.Rsid!.CodeOf(Psk31Modulator.AnnouncedAs),
            composed.GetProperty("rsidCode").GetInt32());

        // **THE CHARACTERS AND THE SECONDS, AND NOTHING ELSE** (HM-DEC-018 §2.1).
        Assert.Equal(sent.Length, composed.GetProperty("characters").GetInt32());

        foreach (var written in lines)
        {
            Assert.DoesNotContain(His, written, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Mine, written, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("BTU", written, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("QRZ", written, StringComparison.OrdinalIgnoreCase);
        }

        // **THE PRESS IS AN OPERATOR ACTION IN THE EXISTING WRITER**, carrying the sub-mode.
        Assert.Equal("PSK31", pressed.GetProperty("detail").GetString());

        // **ONE TRANSMISSION, THROUGH THE ONE DOOR** (§0.2): one call to the sound card, and the
        // wire was written to once by the keying pair.
        Assert.Equal(1, sink.TimesCalled);
        Assert.NotEmpty(port.Written);
        Assert.Contains("Sent \"" + sent + "\"", line, StringComparison.Ordinal);
    }

    // -------------------------------------------------------------------------
    // The fixtures.
    // -------------------------------------------------------------------------

    /// <summary>One event's own payload - the `data` object the writer nests it in.</summary>
    private static IEnumerable<JsonElement> Events(IEnumerable<string> lines, string name)
        => lines
            .Select(l => JsonDocument.Parse(l).RootElement)
            .Where(e => e.TryGetProperty("event", out var what) && what.GetString() == name)
            .Select(e => e.TryGetProperty("data", out var data) ? data : e)
            .ToList();

    private static string Composed(Psk31CannedMacro macro)
        => macro switch
        {
            Psk31CannedMacro.Answer => Psk31Macros.Answer(His, Mine),
            Psk31CannedMacro.Report => Psk31Macros.Report(His, Mine, "Tim", "Pennsylvania", "FN00DJ"),
            _ => Psk31Macros.Confirm(His, Mine),
        };

    private static void Show(MainWindowViewModel model, string mode)
    {
        const string CallingCq = "CQ CQ CQ de " + His + " " + His + " pse K\n";
        const string HisTurn = Mine + " de K3ABC  RST 599 599  BTU " + Mine + " de K3ABC K\n";

        if (mode == "Olivia")
        {
            model.ShowOliviaChannelsForTests(new[]
            {
                new OliviaChannel(1, "8/250", 1000, OliviaListener.FoundByRsid, 0, CallingCq, 9, 0, false),
                new OliviaChannel(2, "16/500", 1500, OliviaListener.FoundByRsid, 0, HisTurn, 9, 0, false),
                new OliviaChannel(3, "4/500", 2000, OliviaListener.FoundByRsid, 0, "CQ CQ de W4G~F W4G~F K\n", 9, 0, false),
            });

            return;
        }

        model.ShowPsk31ChannelsForTests(new[]
        {
            new Psk31Channel(1, 1000, 10, CallingCq),
            new Psk31Channel(2, 1500, 12, HisTurn),
            new Psk31Channel(3, 2000, 8, "CQ CQ de W4G~F W4G~F K\n"),
        });
    }

    private static MainWindowViewModel Panel(string mode, JsonlTelemetry? telemetry = null)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN00DJ";
        settings.Operator.OperatorName = "Tim";
        settings.Operator.Location = "Pennsylvania";
        settings.Operator.LicenseClass = Hamlet.RadioEngine.Licensing.LicenseClass.General;

        var model = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        model.SelectedBand = model.Bands.First(b => b.Band.LowHz <= DialOn20m && b.Band.HighHz >= DialOn20m);
        model.FrequencyHz = DialOn20m;
        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.ChooseDigitalModeCommand.Execute(mode);

        return model;
    }
}
