using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using Avalonia.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Transmit;
using Hamlet.RadioEngine.Transport;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 383 task 1: **what the sheet says and what the record writes, read out of the
/// tree before anything is repaired.**
/// </summary>
/// <remarks>
/// <para>**IT MEASURES AND IT REPAIRS NOTHING.** No file under <c>src\</c> is touched by this
/// type, no sheet is written and nothing is asserted about the radio. Where a measurement here
/// disagrees with work instruction 383 section 5, **the measurement wins** and it goes in the
/// report's section 4 - which is how unit 380 found the squelch, unit 381 the second named
/// carrier and unit 382 that the <c>mode</c> token cannot fire.</para>
/// <para>**COMPUTED, NOT SEEN** (FACT-004). No port is opened, no device is enumerated, nothing
/// is keyed and every sample lives in an array. The one wire here is
/// <see cref="MuteWire"/>, which is a fake that takes nothing on purpose.</para>
/// <para>**EVERY ITEM IS A <c>[Fact]</c>**: none of the six needs a window. The menu items below
/// are built by <see cref="MainWindow.SendFlyoutFor"/>, which
/// <c>TheCannedListIsOfferedTests</c> has driven from a plain fact since unit 378.</para>
/// </remarks>
public sealed class Unit383Trace : IDisposable
{
    private const string His = "W1AW";

    private const string Reporting = "K3ABC";

    private const string Closing = "K4XYZ";

    private const string Mine = "KC3QIS";

    private const long DialOn20m = 14_071_500;

    /// <summary>
    /// **Tier 1, the instructions** (work instruction 383 section 6 ruling 1 item 2).
    /// </summary>
    /// <remarks>
    /// Each one is a phrase whose shape is *go and operate it*: it names the box or a control on
    /// it AND tells the reader to do something to it. The union of the eleven words at
    /// <c>TheRadioSheetQuotesTheScreenTests.cs:225-229</c> with the two R11 lists already in the
    /// tree - <c>TheOliviaSendTests.cs:390</c> and <c>TheAlcSentenceTests.cs:44-49</c> - split by
    /// what the entry does in English.
    /// </remarks>
    private static readonly string[] Tier1 =
    {
        "move the dial", "turn the dial", "set the dial", "tune the radio", "turn the radio",
        "turn the knob", "at the radio", "on the radio", "at the rig's front", "ALC bar",
        "marked zone",
    };

    /// <summary>
    /// **Tier 2, the hardware nouns** - counted and listed, never a red (ruling 1 item 3).
    /// </summary>
    /// <remarks>
    /// These name the box and its controls and are not by themselves an instruction: *a chip is
    /// lit when the dial is inside that mode's block* describes where the receiver is tuned, and
    /// *pick the radio's sound card* names a Windows device.
    /// </remarks>
    private static readonly string[] Tier2 =
    {
        "knob", "dial", "VFO", "PTT", "mic gain", "RF gain", "meter", "ALC", "rig",
        "transceiver", "radio",
    };

    /// <summary>The eleven the sheet's own test used before tonight.</summary>
    private static readonly string[] TheEleven =
    {
        "knob", "VFO", "PTT", "mic gain", "RF gain", "transceiver", "rig",
        "tune the radio", "turn the radio", "on the radio", "at the rig's front",
    };

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where every number and every line is printed.</param>
    public Unit383Trace(ITestOutputHelper output)
    {
        _output = output;
        _folder = Path.Combine(Path.GetTempPath(), "hamlet-unit383-" + Guid.NewGuid().ToString("N"));

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
    // Item 1 - the scan as it stands and the scan ruling 1 defines.
    // ------------------------------------------------------------------

    /// <summary>
    /// **Four numbers side by side: how big tonight's job is.**
    /// </summary>
    [Fact]
    public void TheScanAsItStandsAndTheScanRulingOneDefines()
    {
        var sheet = Sheet();
        var prose = sheet.Where(l => !IsQuote(l)).ToList();

        var elevenOverProse = Hits(prose.Select((line, at) => (At: 0, Line: line)).ToList(), TheEleven);
        var numbered = sheet.Select((line, at) => (At: at + 1, Line: line)).ToList();
        var elevenOverAll = Hits(numbered, TheEleven);
        var tier1 = Hits(numbered, Tier1);
        var tier2 = Hits(numbered, Tier2);

        _output.WriteLine("the sheet is " + sheet.Count + " lines, of which "
            + sheet.Count(IsQuote) + " are quote lines and " + prose.Count + " are prose.");
        _output.WriteLine("");
        _output.WriteLine("the eleven words, PROSE ONLY  (what the test reports today) : " + elevenOverProse.Count);
        _output.WriteLine("the eleven words, EVERY LINE                                : " + elevenOverAll.Count);
        _output.WriteLine("TIER 1 (" + Tier1.Length + " phrases), EVERY LINE                          : " + tier1.Count);
        _output.WriteLine("TIER 2 (" + Tier2.Length + " words), EVERY LINE                            : " + tier2.Count);
        _output.WriteLine("");

        _output.WriteLine("TIER 1, hit by hit:");

        foreach (var hit in tier1)
        {
            _output.WriteLine(
                "  line " + hit.At.ToString(CultureInfo.InvariantCulture).PadLeft(3) + "  "
                + (IsQuote(hit.Line) ? "QUOTE" : "prose") + "  \"" + hit.Word + "\"");
            _output.WriteLine("        " + hit.Line.Trim());
        }

        if (tier1.Count == 0)
        {
            _output.WriteLine("  none.");
        }

        _output.WriteLine("");
        _output.WriteLine("TIER 2, hit by hit:");

        foreach (var hit in tier2)
        {
            _output.WriteLine(
                "  line " + hit.At.ToString(CultureInfo.InvariantCulture).PadLeft(3) + "  "
                + (IsQuote(hit.Line) ? "QUOTE" : "prose") + "  \"" + hit.Word + "\"   "
                + Short(hit.Line.Trim()));
        }

        _output.WriteLine("");
        _output.WriteLine(
            "tier 2 in the sheet's own prose: " + tier2.Count(h => !IsQuote(h.Line))
            + "; inside quotes: " + tier2.Count(h => IsQuote(h.Line)));
        _output.WriteLine(
            "tier 1 in the sheet's own prose: " + tier1.Count(h => !IsQuote(h.Line))
            + "; inside quotes: " + tier1.Count(h => IsQuote(h.Line)));
    }

    // ------------------------------------------------------------------
    // Item 2 - for every tier-1 hit inside a quote, where Hamlet says it.
    // ------------------------------------------------------------------

    /// <summary>
    /// **Every tier-1 hit on a `&gt; ` line, and whether Hamlet itself produces that sentence.**
    /// </summary>
    /// <remarks>
    /// **A HIT THAT IS NEITHER KIND (a) NOR KIND (b) IS THE IMPORTANT FINDING OF THE NIGHT**: it
    /// would mean the sheet carries a sentence Hamlet does not say, which is 4.1's own rule. It is
    /// reported, never silently deleted.
    /// </remarks>
    [Fact]
    public void ForEveryTierOneHitInsideAQuoteWhereHamletSaysIt()
    {
        var numbered = Sheet().Select((line, at) => (At: at + 1, Line: line)).ToList();
        var inQuotes = Hits(numbered, Tier1).Where(h => IsQuote(h.Line)).ToList();

        var produced = WhatHamletSays();
        var sources = OperatorFacingSources();

        _output.WriteLine(inQuotes.Count + " tier-1 hits are inside quotes.");
        _output.WriteLine("");

        foreach (var hit in inQuotes)
        {
            var quote = Quoted(hit.Line);

            _output.WriteLine("line " + hit.At + ", \"" + hit.Word + "\":");
            _output.WriteLine("  " + quote);

            var same = produced.FirstOrDefault(one => one.Sentence == quote);

            if (same.Sentence is not null)
            {
                _output.WriteLine("  KIND (a) - produced whole, character for character: " + same.Label);

                continue;
            }

            var parts = FixedParts(quote);
            var near = produced
                .Where(one => Holds(one.Sentence, parts))
                .OrderBy(one => Math.Abs(one.Sentence.Length - quote.Length))
                .FirstOrDefault();

            if (near.Sentence is not null)
            {
                _output.WriteLine("  KIND (a) - produced, fixed parts in order: " + near.Label);
                _output.WriteLine("        Hamlet said: " + near.Sentence);

                continue;
            }

            var found = sources.FirstOrDefault(one => Holds(one.Text, parts));

            _output.WriteLine(found.Name is null
                ? "  NEITHER - no fixture produced it and no literal under src/Hamlet.App holds it."
                : "  KIND (b) - found as a literal in " + found.Name);
        }

        _output.WriteLine("");
        _output.WriteLine("what this run could make Hamlet say, for the record:");

        foreach (var one in produced)
        {
            _output.WriteLine("  " + one.Label.PadRight(34) + Short(one.Sentence));
        }
    }

    // ------------------------------------------------------------------
    // Item 3 - the three stop-did-not-reach-the-radio sites.
    // ------------------------------------------------------------------

    /// <summary>
    /// **The three sites that say *stop it at the radio*, and which of them a fixture can reach.**
    /// </summary>
    /// <remarks>
    /// **THIS IS THE ONE SENTENCE SAFETY TURNS ON** (ruling 1 item 5): it is what Hamlet says when
    /// its own unkey did not get out. Nothing here changes it, and nothing here keys: the wire is
    /// <see cref="MuteWire"/> and it takes nothing.
    /// </remarks>
    [Fact]
    public void TheThreeStopDidNotReachTheRadioSites()
    {
        var file = Path.Combine(Root(), "src", "Hamlet.App", "ViewModels", "MainWindowViewModel.cs");
        var lines = File.ReadAllLines(file);

        _output.WriteLine("the sites, read out of the file:");

        for (var at = 0; at < lines.Length; at++)
        {
            if (lines[at].Contains("stop it at the radio", StringComparison.Ordinal))
            {
                _output.WriteLine("  MainWindowViewModel.cs:" + (at + 1) + "  " + lines[at].Trim());
            }
        }

        _output.WriteLine("");
        _output.WriteLine("what state produces each one:");
        _output.WriteLine(
            "  :18913  Psk31WentLine, a no-slot run whose Outcome is Cancelled and whose "
            + "CameOutOfTransmit is NothingReachedTheRadio - the PSK31 and Olivia path.");
        _output.WriteLine(
            "  :19154  WentLine, the same two facts on a SLOTTED run - FT8 and FT4, not this "
            + "unit's two modes.");
        _output.WriteLine(
            "  :19587  StopLine, the stop press itself: an abort was fired and NEITHER frame got "
            + "out. It is the sentence the operator is left looking at when he presses Stop.");
        _output.WriteLine("");

        // **THE STOP PRESS, OVER A WIRE THAT TAKES NOTHING.** Nothing is armed, nothing is
        // composed and nothing is keyed; the abort is fired at a port that refuses both frames,
        // which is exactly the state :19587 exists for.
        using var telemetry = new JsonlTelemetry(_folder, "383", _ => true);

        var model = Panel("PSK31", telemetry);
        var wire = new MuteWire();
        var sink = new FakeSink();

        model.UseRigPortForTests(wire);
        model.UseArmedSendForTests(
            new Ft8ArmedSend(new Ft8TransmitSequence(wire, sink, guard: null, telemetry)));

        model.StopSendingCommand.Execute(null);
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var said = model.DigitalSendLine;

        _output.WriteLine("the stop press over a wire that takes nothing produced:");
        _output.WriteLine("  " + said);
        _output.WriteLine("");
        _output.WriteLine(
            said.Contains("stop it at the radio", StringComparison.Ordinal)
                ? "SO :19587 IS REACHABLE FROM A FIXTURE IN THIS REPOSITORY, kind (a), whole."
                : "SO :19587 WAS NOT REACHED BY THIS FIXTURE and the sheet quotes nothing.");

        // **NOTHING WENT OUT** (§0.2, FACT-004): the wire refused every frame it was handed.
        Assert.Equal(0, sink.TimesCalled);
        Assert.Empty(wire.Written);

        _output.WriteLine(
            "the wire was handed " + wire.Refused + " frames and took none; the sound card was "
            + "called " + sink.TimesCalled + " times.");
    }

    // ------------------------------------------------------------------
    // Item 4 - what the record writes today for all seven canned rows.
    // ------------------------------------------------------------------

    /// <summary>
    /// **All seven canned rows pressed, and the `psk31_send_composed` line each one writes.**
    /// </summary>
    /// <remarks>
    /// **THIS IS 7.2's BEFORE.** Three rows are offered on three different conversations, because
    /// a card offers one macro at a time and which one is the conversation's own answer: a CQ
    /// gives Answer, his report before one of yours gives Report, and his closing gives Confirm.
    /// </remarks>
    [Fact]
    public void WhatTheRecordWritesTodayForAllSevenCannedRows()
    {
        var seen = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var (station, what) in new[]
                 {
                     (His, "calling CQ"),
                     (Reporting, "his report, before one of mine"),
                     (Closing, "his closing"),
                 })
        {
            _output.WriteLine("==== the row where " + station + " is " + what);

            var set = Psk31CannedLines.ReadFrom(Psk31CannedLines.ShippedPath);

            for (var at = 0; at < set.Lines.Count; at++)
            {
                var line = set.Lines[at];
                var written = PressOne(station, at, out var header, out var kind);

                _output.WriteLine(
                    "  " + line.Label.PadRight(26)
                    + (line.IsText ? "text " : "macro")
                    + "  command: " + kind
                    + (written is null ? "  -  nothing composed" : ""));

                if (header is not null && header != line.Label)
                {
                    _output.WriteLine("        the menu said: " + header);
                }

                if (written is null)
                {
                    continue;
                }

                _output.WriteLine("        " + written);

                if (!seen.ContainsKey(line.Label))
                {
                    seen[line.Label] = written;
                }
            }
        }

        _output.WriteLine("");
        _output.WriteLine("THE SEVEN, AND THE TOKEN EACH ONE WRITES TODAY:");

        var shipped = Psk31CannedLines.ReadFrom(Psk31CannedLines.ShippedPath);

        foreach (var line in shipped.Lines)
        {
            var written = seen.TryGetValue(line.Label, out var one) ? one : null;
            var token = written is null
                ? "(not composed on any of the three rows)"
                : JsonDocument.Parse(written).RootElement.GetProperty("macro").GetString() ?? "";

            _output.WriteLine(
                "  " + line.Label.PadRight(26) + (line.IsText ? "text " : "macro") + "  macro: " + token);
        }
    }

    // ------------------------------------------------------------------
    // Item 5 - every mention of the canned flag, and what each one does.
    // ------------------------------------------------------------------

    /// <summary>
    /// **Every mention of `_psk31Canned`, read out of the file, and whether the token expression
    /// is its only consumer** (ruling 2 item 4).
    /// </summary>
    /// <remarks>
    /// **IF IT IS READ ANYWHERE ELSE - by the cap, the frame, the burst, the arm or the key - then
    /// ruling 2's mechanism is wrong and task 3 does not start.** This is the instruction's claim
    /// to be checked, not inherited.
    /// </remarks>
    [Fact]
    public void EveryMentionOfThePsk31CannedFlagAndWhatItDoes()
    {
        var file = Path.Combine(Root(), "src", "Hamlet.App", "ViewModels", "MainWindowViewModel.cs");
        var lines = File.ReadAllLines(file);
        var mentions = new List<(int At, string Line)>();

        for (var at = 0; at < lines.Length; at++)
        {
            if (lines[at].Contains("_psk31Canned", StringComparison.Ordinal))
            {
                mentions.Add((at + 1, lines[at].Trim()));
            }
        }

        var reads = new List<(int At, string Line)>();

        foreach (var mention in mentions)
        {
            var declaration = mention.Line.StartsWith("private ", StringComparison.Ordinal);
            var write = Regex.IsMatch(mention.Line, @"_psk31Canned\s*=[^=]");
            var what = declaration ? "DECLARATION"
                : write ? "WRITE       "
                : "READ        ";

            if (!declaration && !write)
            {
                reads.Add(mention);
            }

            _output.WriteLine(
                what + "  MainWindowViewModel.cs:" + mention.At + "  " + mention.Line);
        }

        _output.WriteLine("");
        _output.WriteLine(mentions.Count + " mentions in all, " + reads.Count + " of them reads.");

        foreach (var read in reads)
        {
            var inTheToken = lines
                .Skip(read.At)
                .Take(6)
                .Any(l => l.Contains("canned ? \"canned\"", StringComparison.Ordinal));

            _output.WriteLine(
                "  read at :" + read.At + " - "
                + (inTheToken
                    ? "it is the token expression in SendUnslotted, and nothing else reads it."
                    : "IT IS NOT THE TOKEN EXPRESSION - ruling 2's mechanism is wrong here."));
        }

        _output.WriteLine("");
        _output.WriteLine(
            reads.Count == 1
                ? "THE TOKEN EXPRESSION IS THE FLAG'S ONLY CONSUMER: nothing about the cap, the "
                  + "burst, the frame, the arm or the key reads it."
                : "THE FLAG HAS MORE THAN ONE CONSUMER AND TASK 3 STOPS BEFORE IT STARTS.");
    }

    // ------------------------------------------------------------------
    // Item 6 - the path a macro row takes to the composer.
    // ------------------------------------------------------------------

    /// <summary>
    /// **Where *Answer him* goes, and whether a macro row and a text row reach the same composer,
    /// the same cap and the same burst.**
    /// </summary>
    [Fact]
    public void ThePathAMacroRowTakesToTheComposer()
    {
        var file = Path.Combine(Root(), "src", "Hamlet.App", "ViewModels", "MainWindowViewModel.cs");
        var lines = File.ReadAllLines(file);

        _output.WriteLine("every call to Psk31Events.SendComposed in the application:");

        for (var at = 0; at < lines.Length; at++)
        {
            if (lines[at].Contains("Psk31Events.SendComposed", StringComparison.Ordinal))
            {
                _output.WriteLine("  MainWindowViewModel.cs:" + (at + 1) + "  " + lines[at].Trim());
            }
        }

        _output.WriteLine("");
        _output.WriteLine(
            "Answer him  ->  AnswerPsk31Command (MainWindowViewModel.cs:17531)  ->  SendMessage  "
            + "->  SendUnslotted  ->  the token expression  ->  Psk31Events.SendComposed.");
        _output.WriteLine(
            "a text row  ->  SendCannedPsk31Command (:18023)  ->  SendMessage  ->  SendUnslotted  "
            + "->  the same token expression  ->  the same write.");
        _output.WriteLine(
            "Send my report / Confirm and 73  ->  CardActionCommand (:6666), the PSK31 branch  ->  "
            + "SendMessage  ->  SendUnslotted  ->  the same write.");
        _output.WriteLine("");

        // **AND IT IS MEASURED RATHER THAN READ.** One answer press and one text-row press, each
        // through its own command, compared on the record they leave.
        var answer = PressOne(His, 0, out _, out var answerCommand);
        var text = PressOne(His, 3, out _, out var textCommand);

        _output.WriteLine("the answer press went through " + answerCommand + " and wrote:");
        _output.WriteLine("  " + answer);
        _output.WriteLine("a text row went through " + textCommand + " and wrote:");
        _output.WriteLine("  " + text);

        Assert.NotNull(answer);
        Assert.NotNull(text);

        var one = JsonDocument.Parse(answer!).RootElement;
        var other = JsonDocument.Parse(text!).RootElement;

        _output.WriteLine("");
        _output.WriteLine(
            "same cap    : " + (one.GetProperty("capSeconds").GetDouble()
                == other.GetProperty("capSeconds").GetDouble()));
        _output.WriteLine(
            "same burst  : " + (one.GetProperty("rsidCode").GetInt32()
                == other.GetProperty("rsidCode").GetInt32())
            + "  (code " + one.GetProperty("rsidCode").GetInt32() + ")");
        _output.WriteLine(
            "announced   : " + one.GetProperty("announced").GetBoolean() + " and "
            + other.GetProperty("announced").GetBoolean());
        _output.WriteLine(
            "the tokens  : " + one.GetProperty("macro").GetString() + " and "
            + other.GetProperty("macro").GetString());
        _output.WriteLine("");
        _output.WriteLine(
            "SO A MACRO ROW AND A TEXT ROW REACH THE SAME COMPOSER, THE SAME CAP AND THE SAME "
            + "BURST, and the only thing that differs is the token - which is what 7.2 is about.");
    }

    // ------------------------------------------------------------------
    // One press, driven through the menu a right-click actually builds.
    // ------------------------------------------------------------------

    /// <summary>Press one entry of the canned menu and return the `psk31_send_composed` line.</summary>
    /// <param name="station">Whose row.</param>
    /// <param name="at">Which of the seven.</param>
    /// <param name="header">What the menu item said.</param>
    /// <param name="command">Which command it carried.</param>
    /// <returns>The event's payload, or null where nothing was composed.</returns>
    private string? PressOne(string station, int at, out string? header, out string command)
    {
        header = null;
        command = "none - the row is a note";

        using (var telemetry = new JsonlTelemetry(_folder, "383", _ => true))
        {
            var model = Panel("PSK31", telemetry);

            Show(model);

            var port = new FakePort();
            var sink = new FakeSink();

            model.UseRigPortForTests(port);
            model.UseArmedSendForTests(
                new Ft8ArmedSend(new Ft8TransmitSequence(port, sink, guard: null, telemetry)));

            var row = model.DigitalDecodes.FirstOrDefault(r => model.Psk31StationOn(r) == station);

            if (row is null)
            {
                command = "none - no row names " + station;

                return null;
            }

            model.CardsNowForTests = DateTime.UtcNow;
            model.OpenPsk31CardCommand.Execute(row);

            var flyout = MainWindow.SendFlyoutFor(model, row);
            var items = flyout?.Items.OfType<MenuItem>().ToList() ?? [];

            if (at >= items.Count)
            {
                command = "none - the menu has " + items.Count + " entries";

                return null;
            }

            header = items[at].Header as string ?? "";

            if (items[at].Command is not { } pressed)
            {
                return null;
            }

            command = ReferenceEquals(pressed, model.SendCannedPsk31Command) ? "SendCannedPsk31Command"
                : ReferenceEquals(pressed, model.AnswerPsk31Command) ? "AnswerPsk31Command"
                : ReferenceEquals(pressed, model.CardActionCommand) ? "CardActionCommand"
                : pressed.GetType().Name;

            pressed.Execute(items[at].CommandParameter);

            for (var tries = 0; tries < 2000 && model.HasSomethingToStop; tries++)
            {
                Thread.Sleep(10);
            }

            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        }

        var written = Directory.GetFiles(_folder, "*.jsonl")
            .SelectMany(File.ReadAllLines)
            .Select(l => JsonDocument.Parse(l).RootElement)
            .Where(e => e.TryGetProperty("event", out var what)
                && what.GetString() == "psk31_send_composed")
            .Select(e => e.TryGetProperty("data", out var data) ? data.GetRawText() : e.GetRawText())
            .ToList();

        foreach (var file in Directory.GetFiles(_folder, "*.jsonl"))
        {
            File.Delete(file);
        }

        return written.Count == 0 ? null : written[^1];
    }

    // ------------------------------------------------------------------
    // What Hamlet says, for item 2's kind (a).
    // ------------------------------------------------------------------

    /// <summary>One operator-facing string, and where it came from.</summary>
    private readonly record struct Said(string Label, string Sentence);

    /// <summary>The sentences this trace can make Hamlet produce whole.</summary>
    private List<Said> WhatHamletSays()
    {
        var said = new List<Said>();

        // The crowded band, which is the refusal on line 109 of the sheet.
        using (var telemetry = new JsonlTelemetry(_folder, "383", _ => true))
        {
            var model = Panel("PSK31", telemetry);
            var port = new FakePort();
            var sink = new FakeSink();

            model.UseRigPortForTests(port);
            model.UseArmedSendForTests(
                new Ft8ArmedSend(new Ft8TransmitSequence(port, sink, guard: null, telemetry)));

            model.UsePsk31CandidatesForTests(CrowdedBand());
            model.SendCallToAnyoneCommand.Execute(null);

            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            said.Add(new("no clear spot", model.DigitalSendLine));
        }

        // The stop whose two frames got nowhere.
        using (var telemetry = new JsonlTelemetry(_folder, "383", _ => true))
        {
            var model = Panel("PSK31", telemetry);
            var wire = new MuteWire();
            var sink = new FakeSink();

            model.UseRigPortForTests(wire);
            model.UseArmedSendForTests(
                new Ft8ArmedSend(new Ft8TransmitSequence(wire, sink, guard: null, telemetry)));

            model.StopSendingCommand.Execute(null);

            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            said.Add(new("the stop, neither frame out", model.DigitalSendLine));
        }

        return said;
    }

    // ------------------------------------------------------------------
    // The sheet and the scan.
    // ------------------------------------------------------------------

    private static List<string> Sheet()
        => File.ReadAllLines(Path.Combine(Root(), "docs", "RADIO_SHEET.md")).ToList();

    private static bool IsQuote(string line)
        => line.StartsWith("> ", StringComparison.Ordinal);

    /// <summary>What a quote line says, between its first and its last double quote.</summary>
    private static string Quoted(string line)
    {
        var opens = line.IndexOf('"');
        var shuts = line.LastIndexOf('"');

        return opens >= 0 && shuts > opens ? line[(opens + 1)..shuts] : line[2..];
    }

    /// <summary>Every whole-word hit of any of the words, in the lines handed in.</summary>
    /// <remarks>
    /// **WHOLE WORDS, BECAUSE A SUBSTRING SCAN ANSWERS A DIFFERENT QUESTION** - unit 382 measured
    /// that a substring scan matches `rig` inside `Right-click` and inside `right now`.
    /// </remarks>
    private static List<(int At, string Line, string Word)> Hits(
        List<(int At, string Line)> lines, string[] words)
    {
        var hits = new List<(int At, string Line, string Word)>();

        foreach (var (at, line) in lines)
        {
            foreach (var word in words)
            {
                if (Regex.IsMatch(line, "\\b" + Regex.Escape(word) + "\\b", RegexOptions.IgnoreCase))
                {
                    hits.Add((at, line, word));
                }
            }
        }

        return hits;
    }

    private static readonly Regex Spliced = new("<[^<>]*>", RegexOptions.Compiled);

    private static readonly Regex Concatenation = new("\"\\s*\\+\\s*\"", RegexOptions.Compiled);

    private static List<string> FixedParts(string quote)
        => Spliced.Split(quote)
            .Select(part => part.Trim())
            .Where(part => part.Length > 0)
            .ToList();

    private static bool Holds(string sentence, IReadOnlyList<string> parts)
    {
        var at = 0;

        foreach (var part in parts)
        {
            var found = sentence.IndexOf(part, at, StringComparison.Ordinal);

            if (found < 0)
            {
                return false;
            }

            at = found + part.Length;
        }

        return parts.Count > 0;
    }

    private static string Short(string sentence)
        => sentence.Length <= 96 ? sentence : sentence[..93] + "...";

    /// <summary>One file that can hold an operator-facing literal.</summary>
    private readonly record struct Source(string Name, string Text);

    /// <summary>Every file under `src/Hamlet.App` that could hold a sentence.</summary>
    private static List<Source> OperatorFacingSources()
    {
        var root = Path.Combine(Root(), "src", "Hamlet.App");
        var sources = new List<Source>();

        foreach (var file in Directory
                     .EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
                     .Where(f => f.EndsWith(".cs", StringComparison.Ordinal)
                                 || f.EndsWith(".axaml", StringComparison.Ordinal))
                     .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                                 && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)))
        {
            var text = File.ReadAllText(file);

            sources.Add(new(
                Path.GetRelativePath(Root(), file),
                Concatenation.Replace(text, "").Replace("\\\"", "\"", StringComparison.Ordinal)));
        }

        return sources;
    }

    // ------------------------------------------------------------------
    // The fixtures.
    // ------------------------------------------------------------------

    /// <summary>Three stations: one calling CQ, one reporting, one closing.</summary>
    private static void Show(MainWindowViewModel model)
    {
        const string CallingCq = "CQ CQ CQ de " + His + " " + His + " pse K\n";
        const string HisReport = Mine + " de " + Reporting + "  RST 599 599  BTU " + Mine
            + " de " + Reporting + " K\n";
        const string HisClosing = Mine + " de " + Closing + "  TNX FER QSO 73 GL  BTU " + Mine
            + " de " + Closing + " K\n";

        model.ShowPsk31ChannelsForTests(new[]
        {
            new Psk31Channel(1, 1000, 10, CallingCq),
            new Psk31Channel(2, 1500, 12, HisReport),
            new Psk31Channel(3, 2000, 8, HisClosing),
        });
    }

    private static MainWindowViewModel Panel(string mode, JsonlTelemetry? telemetry = null)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN00DJ";
        settings.Operator.OperatorName = "Tim";
        settings.Operator.Location = "Pennsylvania";
        settings.Operator.LicenseClass = LicenseClass.General;

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

    private static string Root()
    {
        var here = new DirectoryInfo(AppContext.BaseDirectory);

        while (here is not null && !File.Exists(Path.Combine(here.FullName, "CLAUDE.md")))
        {
            here = here.Parent;
        }

        Assert.NotNull(here);

        return here!.FullName;
    }

    /// <summary>
    /// **A wire that opens nothing and takes nothing**, which is the state
    /// `MainWindowViewModel.cs:19587` exists to describe.
    /// </summary>
    /// <remarks>
    /// **NO PORT IS EVER OPENED IN THIS PROJECT** (FACT-004). `FakePort` keeps every frame it is
    /// handed; this one refuses every frame it is handed, and counts them. That is the only
    /// difference, and it is the difference between *the radio was told to stop* and *neither
    /// frame got out*.
    /// </remarks>
    private sealed class MuteWire : ISerialPort
    {
        /// <summary>
        /// What this wire says when it will not take a frame.
        /// </summary>
        /// <remarks>
        /// **DELIBERATELY NOT `the port took nothing`**, which is the sentence's own fallback
        /// where neither attempt recorded a reason. A fake whose words are the fallback's words
        /// would leave a reader unable to say which of the two he was looking at.
        /// </remarks>
        public const string Refusal = "the wire would not take the frame";

        /// <summary>How many frames it was handed and would not take.</summary>
        public int Refused { get; private set; }

        /// <summary>Nothing is ever written, and this says so.</summary>
        public IReadOnlyList<byte[]> Written => [];

        /// <inheritdoc/>
        public bool IsOpen { get; private set; } = true;

        /// <inheritdoc/>
        public string PortName => "MUTE1";

        /// <inheritdoc/>
        public int BaudRate => 115_200;

        /// <inheritdoc/>
        public void Open() => IsOpen = true;

        /// <inheritdoc/>
        public void Close() => IsOpen = false;

        /// <inheritdoc/>
        public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken)
            => ValueTask.FromResult(0);

        /// <inheritdoc/>
        public ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
        {
            Refused++;

            throw new IOException(Refusal);
        }

        /// <inheritdoc/>
        public void Write(ReadOnlySpan<byte> buffer)
        {
            Refused++;

            throw new IOException(Refusal);
        }

        /// <inheritdoc/>
        public void Dispose() => IsOpen = false;
    }
}
