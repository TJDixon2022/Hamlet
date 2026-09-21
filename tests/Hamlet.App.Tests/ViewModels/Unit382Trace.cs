using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Hamlet.App.Settings;
using Hamlet.App.Telemetry;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **Unit 382 task 1: what Hamlet actually says to the operator on PSK31 and on Olivia.**
/// </summary>
/// <remarks>
/// <para>**THIS TYPE BUILDS NOTHING, CHANGES NO SOURCE FILE, REPAIRS NOTHING AND ASSERTS NOTHING
/// ABOUT THE RADIO** (work instruction 382 task 1). It is the harvest the sheet is written from:
/// every sentence here is either PRODUCED by driving the shipped view model into the state and
/// reading what it put on the panel, or FOUND as a literal in a file under <c>src/</c>, and each
/// line of output says which. A sheet written from memory tells the operator to look for words
/// the screen does not say.</para>
/// <para>**THE DECISIVE MEASUREMENT IS THE LAST COLUMN** - whether a fixture can produce the
/// sentence. It decides which sentences get work instruction 382 ruling 1 item 4's kind (a) and
/// which fall back to kind (b), and the report names every kind (b) and why.</para>
/// <para>**EVERY ITEM HERE NEEDS NO WINDOW**, so every one is a <c>[Fact]</c> and none is an
/// <c>[AvaloniaFact]</c>: nothing is drawn, nothing is shown and no scroller is settled. The
/// labels that live in the markup are read out of <c>MainWindow.axaml</c> as text, with the line
/// they were read from, which is a stronger reading than a headless window gives and needs no
/// dispatcher loop.</para>
/// <para>**COMPUTED, NOT SEEN** (FACT-004). No port is opened, no device is enumerated, nothing is
/// keyed, and every sample lives in an array.</para>
/// </remarks>
public sealed class Unit382Trace : IDisposable
{
    /// <summary>The operator's callsign in every fixture here. Not a real station's.</summary>
    private const string Mine = "K1ABC";

    /// <summary>20 m, inside a General class data segment, where both modes have a block.</summary>
    private const long On20m = 14_071_500;

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where every reading is printed.</param>
    public Unit382Trace(ITestOutputHelper output)
    {
        _output = output;

        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-unit382-trace-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_folder);
    }

    /// <summary>Removes the telemetry folder.</summary>
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

    // ------------------------------------------------------------------
    // Item 1 - the press sequence, read off the tree and not remembered.
    // ------------------------------------------------------------------

    /// <summary>
    /// **Item 1: which control chooses the mode, which CQ button belongs to the digital tab, what
    /// the send area and Stop read, and what a right-click offers - each with its file and line.**
    /// </summary>
    /// <remarks>
    /// **THREE BUTTONS IN THE MARKUP READ `CQ` AND ONLY ONE OF THEM SENDS** (work instruction 382
    /// section 5, which leaves this to the reading and does not assert it). They are told apart
    /// here by the command each one carries, which is the only thing that decides what a press
    /// does.
    /// </remarks>
    [Fact]
    public void ItemOneThePressSequence()
    {
        var axaml = Lines(Path.Combine("src", "Hamlet.App", "Views", "MainWindow.axaml"));

        _output.WriteLine("=== ITEM 1 - THE PRESS SEQUENCE, READ OFF THE TREE ===");
        _output.WriteLine("");

        _output.WriteLine("-- the mode strip, which is what chooses PSK31 or Olivia --");

        foreach (var label in DigitalModeChip.Labels)
        {
            _output.WriteLine("  chip label: \"" + label + "\"");
        }

        _output.WriteLine(
            "  the strip is DigitalModeChipStrip at MainWindow.axaml:"
            + At(axaml, "x:Name=\"DigitalModeChipStrip\"")
            + ", its caption reads \"on this frequency\" at :"
            + At(axaml, "Text=\"on this frequency\"")
            + ", and every chip presses ChooseDigitalModeCommand with its own Label at :"
            + At(axaml, "ChooseDigitalModeCommand"));

        _output.WriteLine("");
        _output.WriteLine("-- the three buttons in the markup that read CQ --");

        foreach (var at in AllAt(axaml, "Content=\"CQ\""))
        {
            _output.WriteLine(
                "  MainWindow.axaml:" + at + "  name=" + NearbyName(axaml, at)
                + "  command=" + NearbyCommand(axaml, at));
        }

        _output.WriteLine("");
        _output.WriteLine("-- the send area and the stop --");

        var idle = new MainWindowViewModel(new AppSettings { ReconnectOnStartup = false }, null);

        _output.WriteLine("  send line at rest        : \"" + idle.DigitalSendLine + "\"");
        _output.WriteLine("  its hover at rest        : \"" + MainWindowViewModel.SendIdleTip + "\"");
        _output.WriteLine("  stop label, nothing armed: \"" + idle.StopLabel + "\"");
        _output.WriteLine("  stop hover, nothing armed: \"" + idle.StopTip + "\"");
        _output.WriteLine(
            "  the stop is DigitalStopButton in the status bar at MainWindow.axaml:"
            + At(axaml, "x:Name=\"DigitalStopButton\"")
            + " and presses StopSendingCommand");

        _output.WriteLine("");
        _output.WriteLine("-- what a right-click on a row offers, from data/psk31/canned.json --");

        using var canned = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(Root(), "data", "psk31", "canned.json")));

        var rows = canned.RootElement.GetProperty("lines").EnumerateArray().ToList();

        foreach (var row in rows)
        {
            _output.WriteLine(
                "  \"" + row.GetProperty("label").GetString() + "\""
                + (row.TryGetProperty("macro", out var macro)
                    ? "  (Hamlet's own " + macro.GetString() + " macro)"
                    : "  text: \"" + row.GetProperty("text").GetString() + "\""));
        }

        _output.WriteLine("  seven labels: " + rows.Count);

        _output.WriteLine("");
        _output.WriteLine("-- the file's own note, which is the closest thing in the tree to the sheet's voice --");

        foreach (var note in canned.RootElement.GetProperty("_note").EnumerateArray())
        {
            _output.WriteLine("  | " + note.GetString());
        }

        Assert.Equal(7, rows.Count);
    }

    // ------------------------------------------------------------------
    // Item 2 - every sentence the send path can put in front of the operator.
    // ------------------------------------------------------------------

    /// <summary>
    /// **Item 2: the 24 <c>DigitalSendLine</c> assignments, the nine refusals, and which of them a
    /// fixture can actually produce.**
    /// </summary>
    /// <remarks>
    /// <para>**THE SENTENCES ARE PRODUCED AND NOT TRANSCRIBED WHEREVER A FIXTURE REACHES THEM.**
    /// Each block below drives the shipped view model into one state and prints what Hamlet itself
    /// put in <c>DigitalSendLine</c> or in <c>TransmitRefusalSentence</c>, beside the token the
    /// record carries for it.</para>
    /// <para>**NOTHING IS KEYED** (§0.2, FACT-004): the wire is `FakePort`, the sound card is
    /// `FakeSink`, and every refusal below returns before either is touched.</para>
    /// </remarks>
    [Fact]
    public void ItemTwoEverySentenceTheSendPathCanSay()
    {
        var source = Lines(Path.Combine("src", "Hamlet.App", "ViewModels", "MainWindowViewModel.cs"));
        var sites = AllAt(source, "DigitalSendLine =").ToList();

        _output.WriteLine("=== ITEM 2 - WHAT THE SEND PATH SAYS ===");
        _output.WriteLine("");
        _output.WriteLine(
            "MainWindowViewModel.cs assigns DigitalSendLine at " + sites.Count
            + " sites: " + string.Join(", ", sites));
        _output.WriteLine("");

        _output.WriteLine("-- PRODUCED: what Hamlet itself put on the panel, character for character --");
        _output.WriteLine("");

        Produced("nothing to send", "(no token)", model => model.SendMessageCommand.Execute(""));

        Produced(
            "no_transmit_path, with no device named",
            "psk31_send_refused / no_transmit_path / arm",
            model => model.SendCallToAnyoneCommand.Execute(null),
            arm: false);

        Produced(
            "no_clear_spot",
            "psk31_send_refused / no_clear_spot / spot",
            model =>
            {
                model.UsePsk31CandidatesForTests(CrowdedBand());
                model.SendCallToAnyoneCommand.Execute(null);
            });

        Produced(
            "cap",
            "psk31_send_refused / cap / arm",
            model => model.SendMessageCommand.Execute(TooLongForTheCap()));

        Produced(
            "no_announcement (Olivia, codes unreadable)",
            "psk31_send_refused / no_announcement / announce",
            model =>
            {
                model.UseOliviaDataForTests(OliviaData.Read(null, "{ }", null));
                model.SendCallToAnyoneCommand.Execute(null);
            },
            olivia: true);

        Produced(
            "variant_not_proved (Olivia, nothing proved by loopback)",
            "psk31_send_refused / variant_not_proved / compose",
            model =>
            {
                var format = File.ReadAllText(Path.Combine(Root(), "data", "olivia", "format.json"));

                model.UseOliviaDataForTests(
                    OliviaData.Read(
                        File.ReadAllText(Path.Combine(Root(), "data", "bands", "olivia-calling.json")),
                        File.ReadAllText(Path.Combine(Root(), "assets", "data", "rsid-codes.json")),
                        format.Replace(", \"proved_by_loopback\": true", "", StringComparison.Ordinal)));

                model.SendCallToAnyoneCommand.Execute(null);
            },
            olivia: true);

        Produced(
            "cannot_compose (Olivia, a character it cannot send as itself)",
            "psk31_send_refused / cannot_compose / compose",
            model => model.SendMessageCommand.Execute("CQ — DE " + Mine),
            olivia: true);

        _output.WriteLine("-- PRODUCED: the two step 0 built, off the transmit path itself --");
        _output.WriteLine("");

        ProducedDeviceRefusal("no device chosen", named: false, throws: null);
        ProducedDeviceRefusal(
            "a named device that will not open",
            named: true,
            throws: new InvalidOperationException("the device is in use by another application"));

        _output.WriteLine("-- MEASURED, AND IT DISAGREES WITH THE INSTRUCTION: the mode gate --");
        _output.WriteLine("");

        var wspr = Panel(null, "WSPR");

        wspr.SendCallToAnyoneCommand.Execute(null);

        _output.WriteLine("  pressing CQ under WSPR says: \"" + wspr.DigitalSendLine + "\"");
        _output.WriteLine(
            "  CanTransmitIn answers true for FT8, FT4, PSK31 and Olivia (MainWindowViewModel.cs:"
            + At(source, "private static bool CanTransmitIn") + "), so the gate at :"
            + At(source, "Psk31Events.SendRefused(_telemetry, \"mode\"")
            + " can only fire under a mode the gate refuses - and the only one of the five the");
        _output.WriteLine(
            "  gate refuses is WSPR, where IsPsk31Chosen is false and the psk31 token is not");
        _output.WriteLine(
            "  written at all. THE mode TOKEN IS NOT REACHABLE UNDER PSK31 OR OLIVIA, and the");
        _output.WriteLine(
            "  sentence it guards is a WSPR sentence. It is a finding for section 4, not a repair.");
        _output.WriteLine("");

        _output.WriteLine("-- FOUND, where the sentence belongs to a running send and not to a refusal --");
        _output.WriteLine("");
        _output.WriteLine("  going out : \"Sending \"<your line>\" now, at <n> Hz in the passband.\"");
        _output.WriteLine("              MainWindowViewModel.cs:" + At(source, "\"Sending \\\"\" + wanted + \"\\\" now, at \""));
        _output.WriteLine("  it went   : \"Sent \"<your line>\" - <n> s of <PSK31|Olivia>.\"");
        _output.WriteLine("              MainWindowViewModel.cs:" + At(source, "return \"Sent \\\"\" + wanted + \"\\\" - \""));
        _output.WriteLine("  stopped   : \"Stopped: \"<your line>\" went out for <n> s and the rest of it did not. ...\"");
        _output.WriteLine("              MainWindowViewModel.cs:" + At(source, "return \"Stopped: \\\"\" + wanted + \"\\\" went out for \""));
    }

    // ------------------------------------------------------------------
    // Item 3 - what a capture press writes, and where.
    // ------------------------------------------------------------------

    /// <summary>
    /// **Item 3: the two capture folders, the file name shape, and every sentence a capture press
    /// can put on the screen.**
    /// </summary>
    /// <remarks>
    /// **THERE ARE TWO CAPTURE BUTTONS AND THEY ARE NOT THE SAME BUTTON.** The four
    /// <see cref="DigitalCaptureRefusal"/> values belong to the thirty-second FT8 diagnostic press,
    /// which writes into <c>captures\digital</c>; the PSK31 and Olivia press is a different
    /// control with its own folder, its own file name and its own sentences, and it writes no
    /// <c>DigitalCaptureRefusal</c> at all. Work instruction 382 section 5 names the four as the
    /// sheet's section 4 material; the measurement says which button each one belongs to.
    /// </remarks>
    [Fact]
    public void ItemThreeWhatACapturePressWrites()
    {
        var source = Lines(Path.Combine("src", "Hamlet.App", "ViewModels", "MainWindowViewModel.cs"));
        var axaml = Lines(Path.Combine("src", "Hamlet.App", "Views", "MainWindow.axaml"));

        _output.WriteLine("=== ITEM 3 - THE CAPTURE ===");
        _output.WriteLine("");
        _output.WriteLine("-- two folders, and they are two --");
        _output.WriteLine("  the PSK31 and Olivia capture goes to  : " + MainWindowViewModel.Psk31CaptureFolder);
        _output.WriteLine("  the thirty-second FT8 capture goes to : " + MainWindowViewModel.DigitalCaptureFolder);
        _output.WriteLine(
            "  they are DIFFERENT FOLDERS: the second is the first with \\digital on the end.");
        _output.WriteLine("");

        _output.WriteLine("-- the two buttons --");
        _output.WriteLine(
            "  DigitalPsk31CapturePress at MainWindow.axaml:"
            + At(axaml, "x:Name=\"DigitalPsk31CapturePress\"")
            + " presses CapturePsk31Command, and its own label is the line it carries");
        _output.WriteLine(
            "  DigitalCapturePress at MainWindow.axaml:"
            + At(axaml, "x:Name=\"DigitalCapturePress\"")
            + " reads \"keep the last 30 seconds\" and presses CaptureDigitalCommand");
        _output.WriteLine("");

        _output.WriteLine("-- PRODUCED: what the PSK31 and Olivia press says on the button --");

        var model = Panel(null, "PSK31");

        _output.WriteLine("  at rest   : \"" + model.Psk31CaptureLine + "\"");

        model.CapturePsk31Command.Execute(null);

        _output.WriteLine("  pressed   : \"" + model.Psk31CaptureLine + "\"");

        model.CapturePsk31Command.Execute(null);

        _output.WriteLine("  pressed a second time with nothing heard:");
        _output.WriteLine("            button \"" + model.Psk31CaptureLine + "\"");
        _output.WriteLine("            line   \"" + model.Psk31CaptureWhere + "\"");
        _output.WriteLine("");

        _output.WriteLine("-- FOUND: the file name a PSK31 or an Olivia capture is written under --");
        _output.WriteLine(
            "  <mode>-yyyy-MM-dd-HHmmss.wav, with <mode> \"psk31\" or \"olivia\" taken at the press"
            + " (MainWindowViewModel.cs:" + At(source, "_captureMode = IsOliviaChosen ? \"olivia\" : \"psk31\"")
            + "), written at :" + At(source, "_captureMode + \"-\" + DateTime.UtcNow.ToString("));
        _output.WriteLine(
            "  THE NAME IS A TIMESTAMP AND NOTHING ELSE (HM-DEC-018 2.1): no callsign, no band.");
        _output.WriteLine(
            "  The thirty-second FT8 press writes ft8-yyyy-MM-dd-HHmmss.wav instead"
            + " (MainWindowViewModel.cs:" + At(source, "$\"ft8-{stamp}.wav\"") + ").");
        _output.WriteLine("");

        _output.WriteLine("-- FOUND: the four DigitalCaptureRefusal values and the sentence each one puts up --");

        foreach (var (value, sentence) in new[]
                 {
                     (DigitalCaptureRefusal.NothingIsListening,
                         "Nothing is listening, so there is no audio to keep. Connect a radio or "
                         + "pick the training radio and press it again."),
                     (DigitalCaptureRefusal.NoAudioYet,
                         "No audio has arrived yet, so there is nothing to keep."),
                     (DigitalCaptureRefusal.IOException, "Could not write the capture: <the error>"),
                     (DigitalCaptureRefusal.UnauthorizedAccessException,
                         "Could not write the capture: <the error>"),
                 })
        {
            _output.WriteLine("  " + value + " -> \"" + sentence + "\"");
        }

        _output.WriteLine(
            "  ALL FOUR ARE THE THIRTY-SECOND FT8 PRESS'S, on StatusText, and none of them is");
        _output.WriteLine(
            "  written by the PSK31 or Olivia press. That press has its own two: the one above,");
        _output.WriteLine(
            "  and \"Could not write the capture: <the error>\" on Psk31CaptureWhere.");
    }

    // ------------------------------------------------------------------
    // Item 4 - where the record goes and what is in it.
    // ------------------------------------------------------------------

    /// <summary>
    /// **Item 4: the record's folder, the shape of a day's file name, and three events by name.**
    /// </summary>
    [Fact]
    public void ItemFourWhereTheRecordGoes()
    {
        _output.WriteLine("=== ITEM 4 - THE RECORD ===");
        _output.WriteLine("");
        var settings = Lines(Path.Combine("src", "Hamlet.App", "Settings", "AppSettings.cs"));

        // **THE SHIPPED DEFAULT, COMPOSED FROM THE SAME TWO PIECES THE APPLICATION COMPOSES IT
        // FROM** - not read off `SettingsStore`, which the test assembly redirects to a temporary
        // folder for the whole run so that no test can reach the operator's own files (unit 235).
        var data = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Hamlet");

        _output.WriteLine("  the data folder    : %AppData%\\Hamlet          (here: " + data + ")");
        _output.WriteLine(
            "  the record folder  : %AppData%\\Hamlet\\telemetry    (AppSettings.cs:"
            + At(settings, "public static string TelemetryFolder") + ")");
        _output.WriteLine(
            "  the settings file  : %AppData%\\Hamlet\\settings.json (AppSettings.cs:"
            + At(settings, "public static string SettingsPath") + ")");
        _output.WriteLine("  a day's file name  : yyyy-MM-dd.jsonl, in UTC");
        _output.WriteLine(
            "                       (JsonlTelemetry.cs:"
            + At(Lines(Path.Combine("src", "Hamlet.RadioEngine", "Telemetry", "JsonlTelemetry.cs")),
                "DateTime.UtcNow.ToString(\"yyyy-MM-dd\"")
            + ")");
        _output.WriteLine("");

        _output.WriteLine("-- PRODUCED: three events, written by pressing CQ with nothing armed --");

        List<string> lines;

        using (var telemetry = new JsonlTelemetry(_folder, "382", _ => true))
        {
            var model = Panel(telemetry, "PSK31");

            model.SendCallToAnyoneCommand.Execute(null);

            Settle(model);
        }

        // **READ AFTER THE WRITER IS CLOSED.** `JsonlTelemetry` appends from its own thread, so a
        // read taken inside the `using` asks the file a question the writer has not answered yet.
        lines = Read();

        foreach (var name in new[] { "send_requested", "psk31_send_refused" })
        {
            var found = lines.FirstOrDefault(l => l.Contains(name, StringComparison.Ordinal));

            _output.WriteLine("  " + name + ": " + (found ?? "not written by this press"));
        }

        _output.WriteLine("");
        _output.WriteLine("-- the three events that answer Tim's questions, by name --");
        _output.WriteLine(
            "  on_screen            - what was drawn, filtered, scrolled away, folded or trimmed,");
        _output.WriteLine(
            "                         with each row's own place in whole Hz. It is what answers");
        _output.WriteLine(
            "                         \"why did my list look empty\" without a screenshot. Units 380");
        _output.WriteLine("                         and 381 built it.");
        _output.WriteLine(
            "  psk31_send_refused   - which gate refused a press, by token, and at which stage.");
        _output.WriteLine(
            "                         It is what answers \"I pressed it and nothing went out\".");
        _output.WriteLine(
            "  psk31_capture_finished / olivia_capture_finished - the seconds, the bytes, the");
        _output.WriteLine(
            "                         sha256 and what was being heard when it ran. It is what");
        _output.WriteLine(
            "                         matches a WAV he sends back to the evening it came from.");
        _output.WriteLine("");
        _output.WriteLine(
            "  NOTHING PERSONAL IS IN ANY OF THEM (HM-DEC-018 2.1): no callsign, no decoded text,");
        _output.WriteLine(
            "  no grid, and not even the capture's own path - the path is on the screen only,");
        _output.WriteLine("  because a folder under %AppData% carries the account name.");
    }

    // ------------------------------------------------------------------
    // Item 5 - what the screen says while a send is running.
    // ------------------------------------------------------------------

    /// <summary>
    /// **Item 5: the send-status line, the cap sentence, and the mode chip under Olivia.**
    /// </summary>
    /// <remarks>
    /// **THE CHIP IS UNIT 378'S MEASUREMENT AND IT IS CITED RATHER THAN RE-MEASURED** (work
    /// instruction 382 task 1's drop candidate). What cannot be cited from anywhere is the quoted
    /// status line, so that is produced here.
    /// </remarks>
    [Fact]
    public void ItemFiveWhatTheScreenSaysWhileASendRuns()
    {
        _output.WriteLine("=== ITEM 5 - WHILE A SEND IS RUNNING ===");
        _output.WriteLine("");

        string going;
        string went;

        using (var telemetry = new JsonlTelemetry(_folder, "382", _ => true))
        {
            var model = Panel(telemetry, "PSK31");

            GiveItARadio(model, telemetry);

            model.SendCallToAnyoneCommand.Execute(null);

            going = model.DigitalSendLine;

            Settle(model);

            went = model.DigitalSendLine;
        }

        _output.WriteLine("  PRODUCED, at the press : \"" + going + "\"");
        _output.WriteLine("  PRODUCED, afterwards   : \"" + went + "\"");
        _output.WriteLine("");
        _output.WriteLine(
            "  THE CHIP UNDER OLIVIA, CITED FROM UNIT 378 AND NOT RE-MEASURED (criterion 7.5):");
        _output.WriteLine(
            "  under FT8, FT4, PSK31 and Olivia the chosen chip is the mode chosen and the only");
        _output.WriteLine(
            "  one, because DigitalModeChip.IsChosen has compared case-insensitively against all");
        _output.WriteLine(
            "  five labels since unit 358. Unit 378 also repaired the send line, which carried the");
        _output.WriteLine(
            "  literal PSK31 under Olivia: a 29-second Olivia CQ now reads \"29 s of Olivia\".");
    }

    // ------------------------------------------------------------------
    // Item 6 - the count that sizes the sheet.
    // ------------------------------------------------------------------

    /// <summary>
    /// **Item 6: how many distinct operator-facing sentences the two modes can produce, how many a
    /// fixture reaches, and what the sheet will therefore quote by kind (a) and by kind (b).**
    /// </summary>
    [Fact]
    public void ItemSixTheCountThatSizesTheSheet()
    {
        var source = Lines(Path.Combine("src", "Hamlet.App", "ViewModels", "MainWindowViewModel.cs"));

        _output.WriteLine("=== ITEM 6 - THE COUNT ===");
        _output.WriteLine("");
        _output.WriteLine(
            "  DigitalSendLine assignment sites in MainWindowViewModel.cs : "
            + AllAt(source, "DigitalSendLine =").Count());
        _output.WriteLine(
            "  Psk31Events.SendRefused call sites                          : "
            + AllAt(source, "Psk31Events.SendRefused(").Count());
        _output.WriteLine("");
        _output.WriteLine("  THE NINE REFUSALS, AND WHAT EACH ONE CAN BE PROVED BY:");
        _output.WriteLine("    mode                          - NOT REACHABLE under PSK31 or Olivia  -> kind (b)");
        _output.WriteLine("    no_announcement               - produced                             -> kind (a)");
        _output.WriteLine("    variant_not_proved            - produced                             -> kind (a)");
        _output.WriteLine("    no_clear_spot                 - produced                             -> kind (a)");
        _output.WriteLine("    cannot_compose                - produced                             -> kind (a)");
        _output.WriteLine("    no_transmit_path              - produced                             -> kind (a)");
        _output.WriteLine("    cap                           - produced                             -> kind (a)");
        _output.WriteLine("    no_transmit_device            - produced                             -> kind (a)");
        _output.WriteLine("    transmit_device_would_not_open- produced                             -> kind (a)");
        _output.WriteLine("");
        _output.WriteLine("  EIGHT OF THE NINE ARE REACHABLE FROM A FIXTURE. The one that is not is the");
        _output.WriteLine("  mode gate, and it is not reachable because nothing refuses PSK31 or Olivia at");
        _output.WriteLine("  it: that is a finding, not a fault, and the sheet does not carry its sentence");
        _output.WriteLine("  as one of his two modes' refusals.");
    }

    // ------------------------------------------------------------------
    // The fixtures.
    // ------------------------------------------------------------------

    /// <summary>Drive one refusal and print the sentence Hamlet itself put on the panel.</summary>
    /// <param name="what">What is being produced.</param>
    /// <param name="token">The token the record carries for it.</param>
    /// <param name="press">What the operator did.</param>
    /// <param name="olivia">True to press Olivia rather than PSK31.</param>
    /// <param name="arm">False to leave the panel with no transmit path at all.</param>
    private void Produced(
        string what,
        string token,
        Action<MainWindowViewModel> press,
        bool olivia = false,
        bool arm = true)
    {
        string line;

        using (var telemetry = new JsonlTelemetry(_folder, "382", _ => true))
        {
            var model = Panel(telemetry, olivia ? "Olivia" : "PSK31");

            if (arm)
            {
                GiveItARadio(model, telemetry);
            }

            press(model);

            Settle(model);

            line = model.DigitalSendLine;
        }

        _output.WriteLine("  " + what);
        _output.WriteLine("    token   : " + token);
        _output.WriteLine("    sentence: \"" + line + "\"");
        _output.WriteLine("");
    }

    /// <summary>Drive one of the two transmit-path refusals step 0 built, and print both sentences.</summary>
    /// <param name="what">What is being produced.</param>
    /// <param name="named">Whether a transmit device is named in Settings.</param>
    /// <param name="throws">What the sink factory throws, or null.</param>
    private void ProducedDeviceRefusal(string what, bool named, Exception? throws)
    {
        string panelLine;
        string sentence;
        bool offersSettings;

        using (var telemetry = new JsonlTelemetry(_folder, "382", _ => true))
        {
            var model = Panel(telemetry, "PSK31", named ? "USB Audio CODEC" : null);

            model.TransmitSinkFactory = _ => throw (throws
                ?? new InvalidOperationException("no sink was asked for"));

            var port = new FakePort();

            model.UseRigPortForTests(port);
            model.BuildTheArmedSend(port);

            model.SendCallToAnyoneCommand.Execute(null);

            Settle(model);

            panelLine = model.DigitalSendLine;
            sentence = model.TransmitRefusalSentence;
            offersSettings = model.TransmitRefusalOffersSettings;

            Assert.Empty(port.Written);
        }

        _output.WriteLine("  " + what);
        _output.WriteLine("    on the send line : \"" + panelLine + "\"");
        _output.WriteLine("    the refusal      : \"" + sentence + "\"");
        _output.WriteLine("    offers Settings  : " + offersSettings);
        _output.WriteLine("");
    }

    /// <summary>A panel on 20 m with one of the five sub-modes pressed.</summary>
    /// <param name="telemetry">Where the record goes, or null.</param>
    /// <param name="mode">The chip to press.</param>
    /// <param name="device">The transmit device named in Settings, or null where none is.</param>
    private static MainWindowViewModel Panel(JsonlTelemetry? telemetry, string mode, string? device = null)
    {
        var settings = new AppSettings { ReconnectOnStartup = false, AudioOutputDeviceId = device };

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN42";
        settings.Operator.OperatorName = "Pat";
        settings.Operator.Location = "Boston MA";
        settings.Operator.LicenseClass = LicenseClass.General;

        var model = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
            TapForTests = new AudioTap(),
        };

        model.SelectedBand = model.Bands.First(b => b.Band.LowHz <= On20m && b.Band.HighHz >= On20m);
        model.FrequencyHz = On20m;
        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.ChooseDigitalModeCommand.Execute(mode);

        return model;
    }

    /// <summary>Give the panel the one send path, over fakes. Nothing opens and nothing keys.</summary>
    private static void GiveItARadio(MainWindowViewModel model, JsonlTelemetry telemetry)
    {
        var port = new FakePort();
        var sink = new FakeSink();

        model.UseRigPortForTests(port);
        model.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(port, sink, guard: null, telemetry)));
    }

    /// <summary>A band with no gap over 150 Hz anywhere a call could go.</summary>
    private static List<Psk31Candidate> CrowdedBand()
    {
        var crowded = new List<Psk31Candidate>();

        for (var hz = Psk31ClearSpot.LowestCallHz - 100; hz <= Psk31ClearSpot.HighestCallHz + 100; hz += 100)
        {
            crowded.Add(new Psk31Candidate(hz, 12, Psk31ClearSpot.Occupied + 0.2, true));
        }

        return crowded;
    }

    /// <summary>More PSK31 audio than a macro's cap allows, in Latin-1 the varicode carries.</summary>
    private static string TooLongForTheCap()
        => string.Concat(Enumerable.Repeat("CQ CQ CQ DE " + Mine + " " + Mine + " PSE K ", 40));

    /// <summary>Let a send that the click fired finish, and run what it posted to the UI thread.</summary>
    private static void Settle(MainWindowViewModel model)
    {
        for (var tries = 0; tries < 400 && model.HasSomethingToStop; tries++)
        {
            Thread.Sleep(10);
        }

        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
    }

    /// <summary>Every line the record holds, read while the writer may still have the file open.</summary>
    /// <remarks>
    /// **SHARED, BECAUSE THE WRITER IS A THREAD AND NOT A CALL** (unit 380's item, met again
    /// here). `JsonlTelemetry` appends from its own loop, so a plain
    /// <see cref="File.ReadAllLines(string)"/> races it and throws the file-lock
    /// <see cref="IOException"/> unit 380 reported at `TheOliviaRowsTests.cs:675`. Opening with
    /// <see cref="FileShare.ReadWrite"/> asks the question the reader actually wants asked.
    /// </remarks>
    private List<string> Read()
    {
        var all = new List<string>();

        foreach (var file in Directory.GetFiles(_folder, "*.jsonl"))
        {
            using var stream = new FileStream(
                file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);

            while (reader.ReadLine() is { } line)
            {
                all.Add(line);
            }
        }

        return all;
    }

    // ------------------------------------------------------------------
    // Reading the tree.
    // ------------------------------------------------------------------

    /// <summary>Every line of a repository file, read from the tree above the test binary.</summary>
    private static string[] Lines(string relative)
        => File.ReadAllLines(Path.Combine(Root(), relative));

    /// <summary>The one-based line the needle is first on, or 0.</summary>
    private static int At(string[] lines, string needle)
    {
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains(needle, StringComparison.Ordinal))
            {
                return i + 1;
            }
        }

        return 0;
    }

    /// <summary>Every one-based line the needle is on.</summary>
    private static IEnumerable<int> AllAt(string[] lines, string needle)
    {
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains(needle, StringComparison.Ordinal))
            {
                yield return i + 1;
            }
        }
    }

    /// <summary>The x:Name of the element a line belongs to, read upward from it.</summary>
    private static string NearbyName(string[] lines, int oneBased)
        => Nearby(lines, oneBased, "x:Name=\"");

    /// <summary>The Command a line's element carries, read outward from it.</summary>
    private static string NearbyCommand(string[] lines, int oneBased)
        => Nearby(lines, oneBased, "Command=\"{Binding ");

    private static string Nearby(string[] lines, int oneBased, string opener)
    {
        for (var away = 0; away <= 6; away++)
        {
            foreach (var at in new[] { oneBased - 1 - away, oneBased - 1 + away })
            {
                if (at < 0 || at >= lines.Length)
                {
                    continue;
                }

                var opens = lines[at].IndexOf(opener, StringComparison.Ordinal);

                if (opens < 0)
                {
                    continue;
                }

                var from = opens + opener.Length;
                var shuts = lines[at].IndexOfAny(new[] { '"', '}' }, from);

                return shuts < 0 ? lines[at][from..] : lines[at][from..shuts];
            }
        }

        return "(none within six lines)";
    }

    /// <summary>The repository root, found by walking up to the solution.</summary>
    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }
}
