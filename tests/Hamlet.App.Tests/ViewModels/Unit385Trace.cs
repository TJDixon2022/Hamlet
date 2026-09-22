using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 385 task 1: **what the card, the row and the log already do, before anything
/// is built.**
/// </summary>
/// <remarks>
/// <para>**THIS IS A MEASUREMENT, NOT AN ASSERTION.** It builds nothing, repairs nothing and
/// asserts nothing about the product, so it cannot become a wall. Three of step 9's five criteria
/// are claims about things the tree may already do, and R14 forbids rebuilding what is there;
/// what this prints decides what tasks 2, 3 and 4 are allowed to touch.</para>
/// <para>**EVERY LINE IS COMPOSED ON THE DEVELOPMENT MACHINE - COMPUTED, NOT SEEN** (FACT-004).
/// No port is opened, no device enumerated and nothing keyed. **The callsign is a test
/// callsign**: no callsign from the owner's record or from R44 appears here.</para>
/// </remarks>
public sealed class Unit385Trace : IDisposable
{
    private const string Mine = "KC3QIS";
    private const string Him = "W1ABC";

    /// <summary>His first over: addressed to the operator, handing back, and clean.</summary>
    private const string FirstOver = Mine + " de " + Him + " GM TNX CALL UR 599 599 BTU " + Mine + " de " + Him + " K\n";

    /// <summary>His second over, handing back again and not reading cleanly.</summary>
    private const string SecondOver = Mine + " de " + Him + " R R NAME BOB 5#9 QTH ERIE BTU " + Mine + " de " + Him + " K\n";

    private readonly ITestOutputHelper _output;
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "hamlet-385-" + Guid.NewGuid().ToString("N"));

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where the six tables are printed.</param>
    public Unit385Trace(ITestOutputHelper output)
    {
        _output = output;
        Directory.CreateDirectory(_folder);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, true);
        }
        catch (IOException)
        {
        }
    }

    /// <summary>Print the six before-tables, in the order the instruction asks for them.</summary>
    [Fact]
    public void WhatTheCardTheRowAndTheLogAlreadyDo()
    {
        One();
        Two();
        Three();
        Four();
        Five();
        Six();
    }

    /// <summary>1. The contact itself, out of the record if this machine has it.</summary>
    private void One()
    {
        _output.WriteLine("== 1. the owner's record of 2026-09-21");

        var file = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Hamlet", "telemetry", "2026-09-21.jsonl");

        if (File.Exists(file))
        {
            var lines = File.ReadAllLines(file).Where(l => l.Contains("T17:4", StringComparison.Ordinal)).ToList();

            _output.WriteLine($"   the file is on this machine: {lines.Count} lines between 17:40 and 17:49 UTC");

            foreach (var line in lines.Take(40))
            {
                _output.WriteLine("   " + (line.Length > 200 ? line[..200] : line));
            }

            return;
        }

        _output.WriteLine("   NOT ON THIS MACHINE: " + file);
        _output.WriteLine("   The radio is on the other computer (FACT-004), so the fixture below is CONSTRUCTED to");
        _output.WriteLine("   R44's stated timings - a station whose carrier is up while the operator sends, a first");
        _output.WriteLine("   hand-back, and a second hand-back that does not read cleanly - with a test callsign.");
        _output.WriteLine("   It is never described as the owner's record.");
    }

    /// <summary>2. The two candidate facts for *his carrier is on the air*, side by side.</summary>
    private void Two()
    {
        _output.WriteLine("");
        _output.WriteLine("== 2. row.Ended against Psk31TurnState.HeIsSending, over one station's whole visit");
        _output.WriteLine("   moment                     | row.Ended | StoppedUtc | TurnWord            | state       | certain");

        var model = Panel(out _, out _);

        foreach (var (what, text, carrier) in Visit())
        {
            if (carrier)
            {
                model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, text) });
            }
            else
            {
                model.ShowPsk31ChannelsForTests(Array.Empty<Psk31Channel>());
            }

            var row = model.DigitalDecodes.FirstOrDefault(r => r.IsTextOnly);
            var card = model.DigitalCards.FirstOrDefault(c => c.Callsign == Him);
            var state = card?.Turn?.State.ToString() ?? "(no card)";
            var certain = card?.Turn is { } turn ? turn.IsCertain.ToString() : "-";

            _output.WriteLine(
                $"   {what,-26} | {(row?.Ended.ToString() ?? "(no row)"),-9} | "
                + $"{(string.IsNullOrEmpty(row?.StoppedUtc) ? "-" : row!.StoppedUtc),-10} | "
                + $"{(card?.TurnWord ?? "(no card)"),-19} | {state,-11} | {certain}");
        }

        _output.WriteLine("   The disagreement to read: during the pause between his two overs the carrier is UP");
        _output.WriteLine("   (row.Ended false) while the turn word is NOT 'He is still sending', because the engine");
        _output.WriteLine("   documents HeIsSending as characters arriving after the last complete message.");
    }

    /// <summary>3. The four controls, while his carrier is up.</summary>
    private void Three()
    {
        _output.WriteLine("");
        _output.WriteLine("== 3. the four controls while his carrier is up");

        var model = Panel(out var radio, out _);

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, FirstOver) });

        var card = model.DigitalCards.FirstOrDefault(c => c.Callsign == Him);
        var row = model.DigitalDecodes.FirstOrDefault(r => r.IsTextOnly);

        if (card is null || row is null)
        {
            _output.WriteLine("   NO CARD OR NO ROW - nothing to measure");
            return;
        }

        _output.WriteLine($"   row.Ended            : {row.Ended} (his carrier is up)");
        _output.WriteLine($"   card.Offered         : {card.Offered}   ActionKind {card.ActionKind}");
        _output.WriteLine($"   card.HasAction       : {card.HasAction}");
        _output.WriteLine($"   card.ActionLabel     : [{card.ActionLabel}]");
        _output.WriteLine($"   card.ActionMessage   : [{card.ActionMessage}]");
        _output.WriteLine($"   card.OfferNote       : [{card.OfferNote}]");
        _output.WriteLine($"   card.TurnWord        : [{card.TurnWord}]   sentence: {card.Sentence}");
        _output.WriteLine($"   card.CanType         : {card.CanType}   TypedNote [{card.TypedNote}]");
        _output.WriteLine($"   canned lines offered : {model.CannedLines.Lines.Count} in the set");
        _output.WriteLine($"   CanLogRow(his row)   : {model.CanLogRow(row)}");
        _output.WriteLine($"   keyings before press : {radio.Sink.TimesCalled}");

        // **THEN PRESS ONE**, which is the exact moment the owner hit: his carrier is up and
        // Hamlet composes anyway.
        model.CardActionCommand.Execute(card);
        Settle(model);

        _output.WriteLine($"   PRESSED the offer    : keyings {radio.Sink.TimesCalled}, line [{model.DigitalSendLine}]");
        _output.WriteLine($"   row.Ended at press   : {row.Ended}");
    }

    /// <summary>4. Log, before: on an unfinished card and on a finished one.</summary>
    private void Four()
    {
        _output.WriteLine("");
        _output.WriteLine("== 4. Log, before");

        var model = Panel(out _, out _);

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, FirstOver) });

        var card = model.DigitalCards.FirstOrDefault(c => c.Callsign == Him);
        var row = model.DigitalDecodes.FirstOrDefault(r => r.IsTextOnly);

        if (card is null)
        {
            _output.WriteLine("   NO CARD");
            return;
        }

        _output.WriteLine($"   unfinished card: ShowsLogLink {card.ShowsLogLink}, ActionKind {card.ActionKind}");
        _output.WriteLine($"   LogLabel       : [{card.LogLabel}]");
        _output.WriteLine($"   LogTip         : {card.LogTip}");
        _output.WriteLine($"   CanLogRow(row) : {(row is null ? "(no row)" : model.CanLogRow(row).ToString())}");

        Fields(model, "unfinished");

        // **AND THE SAME ON A FINISHED EXCHANGE**: his 73 and the operator's own sign-off.
        var finished = Panel(out _, out _);

        finished.ShowPsk31ChannelsForTests(new[]
        {
            new Psk31Channel(5, 1500, 10.0, FirstOver + Mine + " de " + Him + " TNX QSO 73 SK\n"),
        });

        var done = finished.DigitalCards.FirstOrDefault(c => c.Callsign == Him);

        _output.WriteLine($"   finished card  : ShowsLogLink {done?.ShowsLogLink.ToString() ?? "(no card)"}, "
            + $"turn [{done?.TurnWord}]");

        Fields(finished, "finished");
    }

    /// <summary>Every field of the log entry the application would build for him right now.</summary>
    private void Fields(MainWindowViewModel model, string what)
    {
        var entry = model.ContactLogEntryForStation(Him, 14_070_000);

        if (entry is null)
        {
            _output.WriteLine($"   {what}: ContactLogEntryForStation returns NULL - there is nothing to log yet");
            return;
        }

        foreach (var field in entry.GetType().GetProperties().OrderBy(p => p.Name))
        {
            var value = field.GetValue(entry);
            var shown = value switch
            {
                null => "(null)",
                string s when s.Length == 0 => "(empty)",
                _ => value.ToString() ?? "(null)",
            };

            _output.WriteLine($"   {what,-10} {field.Name,-16} = {shown}");
        }
    }

    /// <summary>5. The turn, and whether a second hand-back moves the card.</summary>
    private void Five()
    {
        _output.WriteLine("");
        _output.WriteLine("== 5. two hand-backs, and which of them moves the card");

        var model = Panel(out _, out _);
        var splitter = new Psk31MessageSplitter(Mine);
        var parsed = new List<Psk31Exchange>();

        foreach (var character in FirstOver + SecondOver)
        {
            if (splitter.Add(character) is { } message)
            {
                parsed.Add(message.Exchange);
            }
        }

        foreach (var (at, exchange) in parsed.Select((e, i) => (i + 1, e)))
        {
            _output.WriteLine($"   parse {at}: kind {exchange.Kind}, certain {exchange.IsCertain}, "
                + $"handsOver {exchange.HandsOver}, toOperator {exchange.IsForOperator}, speaker {exchange.Speaker}");
        }

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, FirstOver) });

        var afterFirst = model.DigitalCards.FirstOrDefault(c => c.Callsign == Him);

        _output.WriteLine($"   after his first hand-back : [{afterFirst?.TurnWord}] guess "
            + $"{afterFirst?.TurnIsGuess}, offered {afterFirst?.Offered}");

        // **HAMLET ANSWERS**, which is what puts the turn on his side before the second over.
        // Without this the second hand-back is measured from a turn that never left the operator,
        // which is not the sequence R44 names.
        if (afterFirst is { HasAction: true })
        {
            model.CardActionCommand.Execute(afterFirst);
            Settle(model);
        }
        else
        {
            _output.WriteLine($"   NO ACTION ON THE CARD to press: offered {afterFirst?.Offered}, "
                + $"message [{afterFirst?.ActionMessage}]");
        }

        var afterOurs = model.DigitalCards.FirstOrDefault(c => c.Callsign == Him);

        _output.WriteLine($"   after Hamlet answered     : [{afterOurs?.TurnWord}] guess {afterOurs?.TurnIsGuess}, "
            + $"offered {afterOurs?.Offered}");

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, FirstOver + SecondOver) });

        var afterSecond = model.DigitalCards.FirstOrDefault(c => c.Callsign == Him);

        _output.WriteLine($"   after his second hand-back: [{afterSecond?.TurnWord}] guess "
            + $"{afterSecond?.TurnIsGuess}, offered {afterSecond?.Offered}");
        _output.WriteLine("   The question this answers: is the second line not parsed, parsed and not applied,");
        _output.WriteLine("   or applied and not shown? The parse lines above say which.");

        // **AND THE CASE R44 ACTUALLY DESCRIBES**: his reply came back garbled. A hand-back whose
        // callsigns are damaged is the one that may name no speaker at all.
        _output.WriteLine("");
        _output.WriteLine("   the same again, with his second over garbled:");

        var garbled = "KC3Q#S de W1A~C R R NAME BOB QTH ERIE BTU K\n";
        var reading = new Psk31MessageSplitter(Mine);

        foreach (var character in garbled)
        {
            if (reading.Add(character) is { } message)
            {
                var e = message.Exchange;

                _output.WriteLine($"   parse garbled: kind {e.Kind}, certain {e.IsCertain}, handsOver {e.HandsOver}, "
                    + $"toOperator {e.IsForOperator}, speaker {e.Speaker ?? "(none)"}, addressee {e.Addressee ?? "(none)"}");
            }
        }

        var second = Panel(out _, out _);

        second.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, FirstOver) });

        var opened = second.DigitalCards.FirstOrDefault(c => c.Callsign == Him);

        if (opened is { HasAction: true })
        {
            second.CardActionCommand.Execute(opened);
            Settle(second);
        }

        _output.WriteLine($"   after Hamlet answered     : [{second.DigitalCards.FirstOrDefault(c => c.Callsign == Him)?.TurnWord}]");

        second.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, FirstOver + garbled) });

        var afterGarbled = second.DigitalCards.FirstOrDefault(c => c.Callsign == Him);

        _output.WriteLine($"   after his garbled hand-back: [{afterGarbled?.TurnWord}] guess {afterGarbled?.TurnIsGuess}, "
            + $"offered {afterGarbled?.Offered}, cards {second.DigitalCards.Count}");

        // **WHY**, measured rather than reasoned: a message completes on a turnover word that
        // follows a sign, and the sign wants CLEAN callsigns
        // (`Psk31MessageSplitter.FollowsASign`, which asks `Psk31ExchangeParser.IsCleanCallsign`).
        // A garbled over therefore never becomes a parsed line at all, so its text stays pending
        // and the card reads *He is still sending* for as long as it sits there.
        var pending = new Psk31MessageSplitter(Mine);
        var completed = 0;

        foreach (var character in garbled)
        {
            if (pending.Add(character) is not null)
            {
                completed++;
            }
        }

        _output.WriteLine($"   splitter on the garbled over: {completed} messages completed, "
            + $"pending [{pending.Pending.Trim()}]");

        // The same sentence with clean callsigns, as the control.
        var clean = new Psk31MessageSplitter(Mine);
        var cleanCompleted = 0;

        foreach (var character in Mine + " de " + Him + " R R NAME BOB QTH ERIE BTU " + Mine + " de " + Him + " K\n")
        {
            if (clean.Add(character) is not null)
            {
                cleanCompleted++;
            }
        }

        _output.WriteLine($"   splitter on the same words, clean calls: {cleanCompleted} messages completed");
        _output.WriteLine("   So the second hand-back Tim watched is NOT PARSED, not parsed-and-unapplied: the");
        _output.WriteLine("   deciding site is Psk31MessageSplitter.FollowsASign, which is under src/Hamlet.RadioEngine/Psk31/.");
    }

    /// <summary>6. The X, before: what is on the screen and what can remove it.</summary>
    private void Six()
    {
        _output.WriteLine("");
        _output.WriteLine("== 6. the X, before");

        var model = Panel(out _, out var telemetry);

        model.SendCallToAnyoneCommand.Execute(null);
        Settle(model);

        _output.WriteLine($"   cards after a CQ        : {string.Join(", ", model.DigitalCards.Select(c => c.Callsign + (c.IsCallToAnyone ? " (receipt)" : "")))}");

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, FirstOver) });

        _output.WriteLine($"   cards after his answer  : {string.Join(", ", model.DigitalCards.Select(c => c.Callsign + (c.IsCallToAnyone ? " (receipt)" : "")))}");

        foreach (var card in model.DigitalCards)
        {
            _output.WriteLine($"   {card.Callsign,-10}: ActionKind {card.ActionKind}, ShowsLogLink {card.ShowsLogLink}, "
                + $"IsCallToAnyone {card.IsCallToAnyone}");
        }

        _output.WriteLine("   Controls that remove a card today: measured by the properties above - there is no");
        _output.WriteLine("   dismiss command on the view model and no card_dismissed anywhere under src.");

        // **WHAT THE RECONCILE ALREADY WRITES WHEN A CARD LEAVES**, verbatim from the file.
        model.FlushOnScreenForTests();
        telemetry.Dispose();

        foreach (var line in Directory.GetFiles(_folder, "*.jsonl")
                     .SelectMany(File.ReadAllLines)
                     .Where(l => l.Contains("on_screen", StringComparison.Ordinal))
                     .Take(6))
        {
            _output.WriteLine("   " + (line.Length > 220 ? line[..220] : line));
        }
    }

    /// <summary>His whole visit: carrier up, two overs with a pause, then the carrier goes.</summary>
    private static IEnumerable<(string What, string Text, bool Carrier)> Visit()
    {
        yield return ("carrier up, nothing read", "", true);
        yield return ("mid-over, partial text", FirstOver[..30], true);
        yield return ("his first over complete", FirstOver, true);
        yield return ("pause, carrier still up", FirstOver, true);
        yield return ("his second over", FirstOver + SecondOver, true);
        yield return ("carrier gone", "", false);
    }

    private static void Settle(MainWindowViewModel model)
    {
        for (var tries = 0; tries < 200 && model.HasSomethingToStop; tries++)
        {
            System.Threading.Thread.Sleep(10);
        }
    }

    private MainWindowViewModel Panel(out Radio radio, out JsonlTelemetry telemetry)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN00";
        settings.Operator.OperatorName = "Tester";
        settings.Operator.Location = "Erie PA";
        settings.Operator.LicenseClass = LicenseClass.General;

        telemetry = new JsonlTelemetry(_folder, "385", _ => true);

        var model = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.ChooseDigitalModeCommand.Execute("PSK31");
        model.SelectedBand = model.Bands.First(b => b.Band.LowHz <= 14_070_000 && b.Band.HighHz >= 14_070_000);
        model.FrequencyHz = 14_070_000;

        var port = new FakePort();
        var sink = new FakeSink();

        model.UseRigPortForTests(port);
        model.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(port, sink, guard: null, telemetry)));

        radio = new Radio(port, sink);

        return model;
    }

    private sealed record Radio(FakePort Port, FakeSink Sink);
}
