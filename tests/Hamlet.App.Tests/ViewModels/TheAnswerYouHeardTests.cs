using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
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
/// Work instruction 371 task 1: **the man who answered your CQ gets a card, even when Hamlet is
/// not quite sure.**
/// </summary>
/// <remarks>
/// <para>**TIM'S OWN RECORD OF 2026-09-20**, replayed: he called CQ at 1726 Hz, and twenty-three
/// seconds later a station appeared at 1733, sent thirty-five characters addressed to him with a
/// hand-back, stopped, and sent the same thing again. The parser read it - `toOperator: true`,
/// `turnover: true` - and called it `Chat`, `certain: false`, and **no card appeared**.</para>
/// <para>**A CARD THAT APPEARS SENDS NOTHING** (§0.2). Every assertion about sending here counts
/// what the fake sound card was handed; the button goes out on a click and on nothing else.</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004).</para>
/// </remarks>
public sealed class TheAnswerYouHeardTests : IDisposable
{
    private const string Mine = "KC3QIS";
    private const string Him = "W1AW";
    private const string Her = "K9XP";

    /// <summary>
    /// His answer as the record has it: addressed to the operator, handing the turn back, and
    /// uncertain because a symbol is welded into the report.
    /// </summary>
    private const string HisGuessedAnswer = Mine + " de " + Him + " GM TIM UR 5#9 BTU " + Mine + " de " + Him + " K\n";

    /// <summary>The same shape from a second station, so two answers make two cards.</summary>
    private const string HerGuessedAnswer = Mine + " de " + Her + " GM TIM UR 5#9 BTU " + Mine + " de " + Her + " K\n";

    /// <summary>A certain answer, which must behave exactly as it did before this unit.</summary>
    private const string HisCertainAnswer = Mine + " de " + Him + " " + Mine + " de " + Him + " K\n";

    private readonly ITestOutputHelper _output;
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "hamlet-answer-" + Guid.NewGuid().ToString("N"));

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the cards and the record are printed.</param>
    public TheAnswerYouHeardTests(ITestOutputHelper output)
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

    /// <summary>**The fixture is the case: addressed to him, handing over, and not certain.**</summary>
    [Fact]
    public void TheAnswerInTheRecordIsUncertainAndAddressedToTheOperator()
    {
        var parsed = Read(HisGuessedAnswer);

        _output.WriteLine("guessed: " + parsed);

        Assert.False(parsed.IsCertain);
        Assert.True(parsed.IsForOperator);
        Assert.True(parsed.HandsOver);
        Assert.Equal(Him, parsed.Speaker);

        var certain = Read(HisCertainAnswer);

        _output.WriteLine("certain: " + certain);

        Assert.True(certain.IsCertain);
        Assert.True(certain.IsForOperator);
    }

    /// <summary>
    /// **His card opens on the first uncertain line, marked as a guess, and the receipt retires.**
    /// </summary>
    [Fact]
    public void AnUncertainAnswerOpensHisCardAsAGuessAndRetiresTheReceipt()
    {
        var model = Panel(out var radio, out var telemetry);

        model.SendCallToAnyoneCommand.Execute(null);
        Settle(model);

        Assert.Contains(model.DigitalCards, c => c.Callsign == Ft8ContactLedger.CallToAnyone);

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1733, 10.0, HisGuessedAnswer) });

        var card = Assert.Single(model.DigitalCards);

        _output.WriteLine($"card {card.Callsign}: turn [{card.TurnWord}] guess {card.TurnIsGuess}, "
            + $"offered {card.Offered}, action [{card.ActionLabel}], note [{card.OfferNote}]");

        // **HIS CARD IS THERE**, and the call to anybody is not.
        Assert.Equal(Him, card.Callsign);
        Assert.DoesNotContain(model.DigitalCards, c => c.Callsign == Ft8ContactLedger.CallToAnyone);

        // **AND IT SAYS IT IS A GUESS** (§0.0), in 9.4's own word since work instruction 389
        // (rewritten under R12: the word was *Your turn, a guess* and the plan's is *your turn?*).
        Assert.True(card.TurnIsGuess);
        Assert.Equal("Your turn?", card.TurnWord);

        // **REPORT IS OFFERED, WITH THE DOUBT BESIDE IT.**
        Assert.Equal(Psk31Macro.Report, card.Offered);
        Assert.Equal(Ft8CardActionKind.Send, card.ActionKind);
        Assert.Equal("not sure it is your turn", card.OfferNote);

        // **AND NOTHING HAS GONE OUT** (§0.2): the CQ was the only transmission.
        Assert.Equal(1, radio.Sink.TimesCalled);

        telemetry.Dispose();

        var taken = Assert.Single(Events("psk31_answer_taken"));

        _output.WriteLine("record: " + taken);

        Assert.False(taken.GetProperty("certain").GetBoolean());
        Assert.Equal(1733, taken.GetProperty("offsetHz").GetDouble());
        NothingPersonal();
    }

    /// <summary>**The offered Report sends once, on his click, and not before.**</summary>
    [Fact]
    public void TheOfferedReportSendsOnceOnHisClick()
    {
        var model = Panel(out var radio, out var telemetry);

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1733, 10.0, HisGuessedAnswer) });

        var card = Assert.Single(model.DigitalCards);

        Assert.Equal(Psk31Macro.Report, card.Offered);

        // **NOT BEFORE**: the card has been on the screen and nothing has been handed to the card.
        Assert.Equal(0, radio.Sink.TimesCalled);

        model.CardActionCommand.Execute(card);
        Settle(model);

        _output.WriteLine($"after the click: keyings {radio.Sink.TimesCalled}, line [{model.DigitalSendLine}]");

        Assert.Equal(1, radio.Sink.TimesCalled);

        telemetry.Dispose();
        NothingPersonal();
    }

    /// <summary>**A second station answering gets his own card.**</summary>
    [Fact]
    public void ASecondStationAnsweringGetsHisOwnCard()
    {
        var model = Panel(out _, out var telemetry);

        model.SendCallToAnyoneCommand.Execute(null);
        Settle(model);

        model.ShowPsk31ChannelsForTests(new[]
        {
            new Psk31Channel(5, 1733, 10.0, HisGuessedAnswer),
            new Psk31Channel(7, 1500, 10.0, HerGuessedAnswer),
        });

        _output.WriteLine("cards: " + string.Join(", ", model.DigitalCards.Select(c => c.Callsign + " " + c.TurnWord)));

        Assert.Equal(2, model.DigitalCards.Count);
        Assert.Contains(model.DigitalCards, c => c.Callsign == Him);
        Assert.Contains(model.DigitalCards, c => c.Callsign == Her);
        Assert.DoesNotContain(model.DigitalCards, c => c.Callsign == Ft8ContactLedger.CallToAnyone);

        telemetry.Dispose();

        Assert.Equal(2, Events("psk31_answer_taken").Count);
    }

    /// <summary>**A certain answer is what it always was: a card that is not a guess.**</summary>
    [Fact]
    public void ACertainAnswerIsUnchanged()
    {
        var model = Panel(out _, out var telemetry);

        model.SendCallToAnyoneCommand.Execute(null);
        Settle(model);

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1733, 10.0, HisCertainAnswer) });

        var card = Assert.Single(model.DigitalCards);

        _output.WriteLine($"card {card.Callsign}: turn [{card.TurnWord}] guess {card.TurnIsGuess}, "
            + $"offered {card.Offered}, note [{card.OfferNote}]");

        Assert.Equal(Him, card.Callsign);
        Assert.False(card.TurnIsGuess);
        Assert.Equal("", card.OfferNote);
        Assert.Equal(Psk31Macro.Report, card.Offered);

        telemetry.Dispose();

        Assert.True(Assert.Single(Events("psk31_answer_taken")).GetProperty("certain").GetBoolean());
    }

    /// <summary>
    /// **An Olivia station who answers gets the same card, by construction** (the instruction's
    /// §9): its rows go through the one card path, so this asserts the inheritance rather than a
    /// second rule.
    /// </summary>
    [Fact]
    public void AnOliviaStationAnsweringGetsTheSameGuessedCard()
    {
        var model = Panel(out _, out var telemetry);

        model.ChooseDigitalModeCommand.Execute("Olivia");
        model.ShowOliviaChannelsForTests(new[]
        {
            new OliviaChannel(3, "8/250", 1000, OliviaListener.FoundByRsid, 0, HisGuessedAnswer, 9, 0, false, 1000),
        });

        var card = Assert.Single(model.DigitalCards);

        _output.WriteLine($"olivia card {card.Callsign}: turn [{card.TurnWord}], offered {card.Offered}, note [{card.OfferNote}]");

        Assert.Equal(Him, card.Callsign);
        Assert.True(card.TurnIsGuess);
        Assert.Equal("not sure it is your turn", card.OfferNote);

        telemetry.Dispose();
    }

    private static Psk31Exchange Read(string line)
    {
        var splitter = new Psk31MessageSplitter(Mine);

        foreach (var character in line)
        {
            if (splitter.Add(character) is { } message)
            {
                return message.Exchange;
            }
        }

        throw new InvalidOperationException("the fixture line did not complete a message");
    }

    private List<JsonElement> Events(string name)
        => Directory.GetFiles(_folder, "*.jsonl")
            .SelectMany(File.ReadAllLines)
            .Select(line => JsonDocument.Parse(line).RootElement)
            .Where(e => e.GetProperty("event").GetString() == name)
            .Select(e => e.GetProperty("data").Clone())
            .ToList();

    /// <summary>**No callsign and no text in any line this unit writes** (HM-DEC-018).</summary>
    /// <remarks>
    /// **THE CALLSIGNS ARE SWEPT OVER EVERY LINE AND THE OPERATOR'S NAME OVER THIS UNIT'S OWN.**
    /// A name as short as *Tim* is a substring of ordinary event names - `spot_timer_changed` is
    /// one - so a sweep for it across the whole file fails on words that are not names.
    /// </remarks>
    private void NothingPersonal()
    {
        var lines = Directory.GetFiles(_folder, "*.jsonl").SelectMany(File.ReadAllLines).ToList();

        Assert.NotEmpty(lines);

        foreach (var line in lines)
        {
            Assert.DoesNotContain(Him, line, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Her, line, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Mine, line, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var line in lines.Where(l => l.Contains("psk31_answer_taken", StringComparison.Ordinal)))
        {
            Assert.DoesNotContain("TIM", line, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("BTU", line, StringComparison.OrdinalIgnoreCase);
        }
    }

    private MainWindowViewModel Panel(out Radio radio, out JsonlTelemetry telemetry)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN00";
        settings.Operator.OperatorName = "Tim";
        settings.Operator.Location = "Trafford PA";
        settings.Operator.LicenseClass = LicenseClass.General;

        telemetry = new JsonlTelemetry(_folder, "371", _ => true);

        var model = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.ChooseDigitalModeCommand.Execute("PSK31");

        var port = new FakePort();
        var sink = new FakeSink();

        model.UseRigPortForTests(port);
        model.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(port, sink, guard: null, telemetry)));

        radio = new Radio(port, sink);

        return model;
    }

    private static void Settle(MainWindowViewModel model)
    {
        for (var tries = 0; tries < 200 && model.HasSomethingToStop; tries++)
        {
            System.Threading.Thread.Sleep(10);
        }
    }

    private sealed record Radio(FakePort Port, FakeSink Sink);
}
