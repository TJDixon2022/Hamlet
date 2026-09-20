using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.Telemetry;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Transmit;
using Hamlet.RadioEngine.Tests.Psk31;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 323 task 2: **pressing CQ under PSK31 puts one call on the air, on a
/// clear spot, and leaves a receipt.**
/// </summary>
/// <remarks>
/// <para>**THROUGH THE RUNNING APPLICATION, NOT AT THE BENCH.** Every assertion here drives
/// the real view model: the real mode gate, the real composer, the real
/// <see cref="Ft8ArmedSend"/> over the real <see cref="Ft8TransmitSequence"/>. What stands
/// in for the radio is a port that keeps its frames and a sink that counts - no device is
/// opened and no RF exists (`SHACK_FACTS.md` FACT-004, FACT-006).</para>
/// <para>**AND THE RULE THAT IS TIM'S IS WHAT EVERY CASE MEASURES** (§0.2): the count of
/// keyings equals the count of clicks, and a press that cannot be made cleanly is refused
/// in words rather than made anyway.</para>
/// </remarks>
public sealed class ThePsk31CqGoesOutTests : IDisposable
{
    private const long On20m = 14_070_000;

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the record is printed.</param>
    public ThePsk31CqGoesOutTests(ITestOutputHelper output)
    {
        _output = output;

        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-psk31-cq-" + Guid.NewGuid().ToString("N"));

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

    /// <summary>**1: one press, one CQ on a clear spot, one keying, and the record says so.**</summary>
    [Fact]
    public void OnePressPutsOneCqOnAClearSpotAndWritesWhatHappened()
    {
        List<string> lines;
        Radio radio;

        using (var telemetry = new JsonlTelemetry(_folder, "323", _ => true))
        {
            var model = Panel(telemetry);

            radio = GiveItARadio(model, telemetry);

            model.SendCallToAnyoneCommand.Execute(null);

            Settle(model);

            _output.WriteLine("send line: " + model.DigitalSendLine);
        }

        lines = Read();

        foreach (var line in lines.Where(OnThePath))
        {
            _output.WriteLine(line);
        }

        // **ONE COMPOSITION, AND IT IS §R2's CALL TO ANYONE AT A CHOSEN OFFSET.**
        var composed = Assert.Single(Named(lines, "psk31_send_composed"));

        Assert.Equal("cq", Field(composed, "macro"));

        var offset = Number(composed, "offsetHz");

        _output.WriteLine("offset chosen: "
            + offset.ToString("0.0", CultureInfo.InvariantCulture) + " Hz");

        // **INSIDE THE SPAN A CALL MAY BE PLACED IN**, which is what makes it a spot the
        // radio will actually transmit rather than the widest gap in the whole spectrum.
        Assert.InRange(offset, Psk31ClearSpot.LowestCallHz, Psk31ClearSpot.HighestCallHz);

        // **ONE KEYING, AND IT IS THE ONE `PttOn` SITE** (§0.2, §R10).
        Assert.Equal(1, radio.Port.Written.Count(f => f.Contains((byte)0x01)));

        // **IT REACHED THE SINK ONCE.**
        Assert.Equal(1, radio.Sink.TimesCalled);

        // **THE SEND HAD NO SLOT**, said by the stage and by the transmission record.
        Assert.Contains(Named(lines, SendStage.EventName), NoSlot);
        Assert.Contains(
            Named(lines, TransmitRecord.EventName),
            l => l.Contains("\"mode\":\"Psk31\"", StringComparison.Ordinal));

        // **THE SIX LINES OF THE PRESS ARE ALL THERE.**
        foreach (var name in new[]
        {
            "psk31_send_composed",
            SendStage.EventName,
            "psk31_send_keyed",
            "psk31_send_unkeyed",
            TransmitRecord.EventName,
            "psk31_radio_after_send",
        })
        {
            Assert.NotEmpty(Named(lines, name));
        }

        // **AND IN AN ORDER A PERSON CAN READ FORWARDS**: what was composed, then the
        // stages, then what became of the carrier, then what the radio said afterwards.
        Assert.True(
            First(lines, "psk31_send_composed") < First(lines, SendStage.EventName),
            "the composition is written before the stages it went through");

        Assert.True(
            First(lines, "psk31_send_keyed") < First(lines, "psk31_send_unkeyed"),
            "the keying is written before the unkeying");

        Assert.True(
            First(lines, "psk31_send_unkeyed") < First(lines, "psk31_radio_after_send"),
            "what the radio said comes after the carrier has gone");

        // **WITH NO RADIO ANSWERING, THE RECORD SAYS SO** rather than staying silent.
        var after = Assert.Single(Named(lines, "psk31_radio_after_send"));

        Assert.Contains("\"answered\":false", after, StringComparison.Ordinal);
    }

    /// <summary>**2: a second press refreshes the one receipt; it never stacks.**</summary>
    [Fact]
    public void ASecondPressRefreshesTheReceiptAndSendsAgain()
    {
        var model = Panel();
        var radio = GiveItARadio(model);

        model.CardsNowForTests = new DateTime(2026, 9, 11, 22, 0, 0, DateTimeKind.Utc);
        model.SendCallToAnyoneCommand.Execute(null);
        Settle(model);

        var first = Assert.Single(model.DigitalCards);

        _output.WriteLine("after one press : " + Describe(model)
            + ", keyings " + radio.Sink.TimesCalled);

        Assert.Equal(Ft8ContactLedger.CallToAnyone, first.Callsign);
        Assert.Equal(Ft8ContactCard.ReceiptWord, first.StateWord);

        model.CardsNowForTests = model.CardsNowForTests!.Value.AddMinutes(3);
        model.SendCallToAnyoneCommand.Execute(null);
        Settle(model);

        var second = Assert.Single(model.DigitalCards);

        _output.WriteLine("after two presses: " + Describe(model)
            + ", keyings " + radio.Sink.TimesCalled);

        // **ONE RECEIPT, REFRESHED** (Tim, 2026-09-11, R2 and R4 on cards).
        Assert.Equal(Ft8ContactLedger.CallToAnyone, second.Callsign);

        // **AND THE RECORD BEHIND IT HOLDS THE LAST CALL ONLY, NOT A TALLY.**
        Assert.Single(model.LedgerForTests!.For(Ft8ContactLedger.CallToAnyone)!.Sent);

        // **TWO CLICKS, TWO TRANSMISSIONS** (§0.2) - it refreshes and it still sends.
        Assert.Equal(2, radio.Sink.TimesCalled);
    }

    /// <summary>
    /// **3, rewritten under R12 by work instruction 371 task 1: an answer retires the receipt and
    /// opens his card, and a guessed answer is still an answer.**
    /// </summary>
    /// <remarks>
    /// **THE DOOR THIS GUARDED IS OPEN.** It asserted that a guess retires nothing, which is what
    /// happened to Tim on 2026-09-20: a station came back to his CQ twice, addressed to him and
    /// handing the turn over, the parse was not certain, and the screen showed him nothing.
    /// **§R1's strict side governs what Hamlet sends by itself; a card that appears sends
    /// nothing.** What still retires nothing is a line that hands nothing back and names nobody,
    /// which is asserted below.
    /// </remarks>
    [Fact]
    public void AnAnswerRetiresTheReceiptAndAGuessedAnswerIsStillAnAnswer()
    {
        var corpus = Psk31Corpus.Load();

        // **A GUESSED ANSWER OPENS HIS CARD AND RETIRES THE RECEIPT.** 05-garbled's third line is
        // addressed to the operator and hands the turn back; the parser is not certain of it
        // because its report digits are damaged.
        var guessed = Panel();

        GiveItARadio(guessed);
        guessed.SendCallToAnyoneCommand.Execute(null);
        Settle(guessed);

        guessed.ShowPsk31ChannelsForTests(
            new[] { Channel(1, 1000, Transcript(corpus, "05-garbled"), 3) });

        _output.WriteLine("after a guessed answer : " + Describe(guessed));

        var guessedCard = Assert.Single(guessed.DigitalCards);

        Assert.Equal("K3ABC", guessedCard.Callsign);
        Assert.True(guessedCard.TurnIsGuess);
        Assert.Equal("not sure it is your turn", guessedCard.OfferNote);
        Assert.DoesNotContain(
            guessed.DigitalCards,
            c => c.Callsign == Ft8ContactLedger.CallToAnyone);

        // **AND A LINE THAT HANDS NOTHING BACK AND NAMES NOBODY RETIRES NOTHING**: 05-garbled's
        // fourth line has no callsign and no turnover word, so there is nobody to open a card for
        // and no answer to claim.
        var loose = Panel();

        GiveItARadio(loose);
        loose.SendCallToAnyoneCommand.Execute(null);
        Settle(loose);

        loose.ShowPsk31ChannelsForTests(
            new[] { new Psk31Channel(1, 1000, 10.0, Transcript(corpus, "05-garbled").Lines[3].Text + "\n") });

        _output.WriteLine("after a loose line     : " + Describe(loose));

        Assert.Contains(
            loose.DigitalCards,
            c => c.Callsign == Ft8ContactLedger.CallToAnyone);

        // **ONE CERTAIN ANSWER: THE RECEIPT GOES AND HIS CARD ARRIVES.**
        var one = Panel();

        GiveItARadio(one);
        one.SendCallToAnyoneCommand.Execute(null);
        Settle(one);

        Assert.Contains(one.DigitalCards, c => c.Callsign == Ft8ContactLedger.CallToAnyone);

        one.ShowPsk31ChannelsForTests(
            new[] { Channel(1, 1000, Transcript(corpus, "01-textbook"), 3) });

        _output.WriteLine("after one certain answer: " + Describe(one));

        var card = Assert.Single(one.DigitalCards);

        Assert.Equal("W1AW", card.Callsign);

        // **TWO CERTAIN ANSWERS: TWO CARDS, AND NOBODY INHERITS THE CALL** (R5).
        var two = Panel();

        GiveItARadio(two);
        two.SendCallToAnyoneCommand.Execute(null);
        Settle(two);

        two.ShowPsk31ChannelsForTests(new[]
        {
            Channel(1, 1000, Transcript(corpus, "01-textbook"), 3),
            Channel(2, 1400, Transcript(corpus, "02-chatty"), 3),
        });

        _output.WriteLine("after two certain answers: " + Describe(two));

        Assert.Equal(2, two.DigitalCards.Count);
        Assert.DoesNotContain(
            two.DigitalCards, c => c.Callsign == Ft8ContactLedger.CallToAnyone);
    }

    /// <summary>**4: with nowhere clear, the press refuses in words and nothing is armed.**</summary>
    [Fact]
    public void WithNoClearSpotThePressRefusesAndNothingReachesTheSequence()
    {
        List<string> lines;
        Radio radio;

        using (var telemetry = new JsonlTelemetry(_folder, "323", _ => true))
        {
            var model = Panel(telemetry);

            radio = GiveItARadio(model, telemetry);

            // **A BAND WITH NO GAP OVER 150 HZ ANYWHERE A CALL COULD GO.** Every hundred
            // hertz across the whole span a call may be placed in, above the quality the
            // search itself would keep a carrier on.
            var crowded = new List<Psk31Candidate>();

            for (var hz = Psk31ClearSpot.LowestCallHz - 100;
                 hz <= Psk31ClearSpot.HighestCallHz + 100;
                 hz += 100)
            {
                crowded.Add(new Psk31Candidate(hz, 12, Psk31ClearSpot.Occupied + 0.2, true));
            }

            model.UsePsk31CandidatesForTests(crowded);

            model.SendCallToAnyoneCommand.Execute(null);

            Settle(model);

            _output.WriteLine("send line: " + model.DigitalSendLine);

            // **IN PLAIN WORDS, AND IT SAYS WHAT WOULD CHANGE IT.**
            Assert.Contains(
                "too crowded", model.DigitalSendLine, StringComparison.OrdinalIgnoreCase);
        }

        lines = Read();

        var refused = Assert.Single(Named(lines, "psk31_send_refused"));

        _output.WriteLine("refused: " + refused);

        Assert.Equal("no_clear_spot", Field(refused, "reason"));

        // **AND NOTHING GOT PAST IT** - nothing composed, nothing staged, nothing keyed.
        Assert.Empty(Named(lines, "psk31_send_composed"));
        Assert.Empty(Named(lines, SendStage.EventName));
        Assert.Empty(Named(lines, TransmitRecord.EventName));
        Assert.Equal(0, radio.Sink.TimesCalled);
        Assert.True(radio.Port.WasNeverWrittenTo);
    }

    /// <summary>The port and sink standing in for a radio that does not exist.</summary>
    private sealed record Radio(FakePort Port, FakeSink Sink);

    /// <summary>Gives the panel the one send path, over fakes.</summary>
    /// <param name="model">The panel.</param>
    /// <returns>What the send path was given.</returns>
    /// <remarks>
    /// **THE REAL SEQUENCE AND THE REAL ARMED SEND** (§0.2). Only the two ends are fakes:
    /// the port keeps its frames instead of opening a COM port, and the sink counts instead
    /// of opening a render device. Everything between them - the licence gate, the one
    /// keying write, the `finally` that unkeys - is the shipped code.
    /// </remarks>
    private static Radio GiveItARadio(
        MainWindowViewModel model, ITelemetry? telemetry = null)
    {
        var port = new FakePort();
        var sink = new FakeSink();

        model.UseRigPortForTests(port);
        model.UseArmedSendForTests(
            new Ft8ArmedSend(new Ft8TransmitSequence(port, sink, guard: null, telemetry)));

        return new Radio(port, sink);
    }

    /// <summary>Lets the no-slot send finish, since the click fires it (§R10).</summary>
    /// <param name="model">The panel.</param>
    /// <remarks>
    /// **THE CLICK STARTS IT AND DOES NOT WAIT FOR IT.** `SendPsk31` hands the run off so
    /// the panel is not frozen for the length of a macro; a test that looked immediately
    /// would be asking what happened before it had. **It waits on the sink's own count**,
    /// which is a fact the sink measured, rather than sleeping a guessed interval.
    /// </remarks>
    private static void Settle(MainWindowViewModel model)
    {
        for (var tries = 0; tries < 200 && model.HasSomethingToStop; tries++)
        {
            System.Threading.Thread.Sleep(10);
        }

        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
    }

    private static bool NoSlot(string line)
        => line.Contains("\"slotted\":false", StringComparison.Ordinal);

    private static bool OnThePath(string line)
        => Names(line, "psk31_send_composed")
            || Names(line, "psk31_send_refused")
            || Names(line, "psk31_send_keyed")
            || Names(line, "psk31_send_unkeyed")
            || Names(line, "psk31_send_alc")
            || Names(line, "psk31_radio_after_send")
            || Names(line, SendStage.EventName)
            || Names(line, TransmitRecord.EventName);

    private static bool Names(string line, string eventName)
        => line.Contains("\"event\":\"" + eventName + "\"", StringComparison.Ordinal);

    private static List<string> Named(List<string> lines, string eventName)
        => lines.Where(l => Names(l, eventName)).ToList();

    private static int First(List<string> lines, string eventName)
        => lines.FindIndex(l => Names(l, eventName));

    private static string Field(string line, string key)
    {
        using var document = System.Text.Json.JsonDocument.Parse(line);

        return document.RootElement.GetProperty("data").GetProperty(key).GetString() ?? "";
    }

    private static double Number(string line, string key)
    {
        using var document = System.Text.Json.JsonDocument.Parse(line);

        return document.RootElement.GetProperty("data").GetProperty(key).GetDouble();
    }

    private List<string> Read()
        => Directory.GetFiles(_folder, "*.jsonl").SelectMany(File.ReadAllLines).ToList();

    private static string Describe(MainWindowViewModel model)
        => model.DigitalCards.Count + " card(s)"
            + string.Concat(model.DigitalCards.Select(
                c => " " + c.Callsign + " [" + c.StateWord + "]"));

    private static Psk31CorpusTranscript Transcript(Psk31Corpus corpus, string name)
        => corpus.Transcripts.Single(t => t.Name == name);

    private static Psk31Channel Channel(
        int id, double hz, Psk31CorpusTranscript transcript, int lines)
        => new(id, hz, 10.0,
            string.Concat(transcript.Lines.Take(lines).Select(l => l.Text + "\n")));

    private static MainWindowViewModel Panel(JsonlTelemetry? telemetry = null)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = Psk31Corpus.Load().Operator;
        settings.Operator.GridSquare = "FN00DJ";
        settings.Operator.LicenseClass = LicenseClass.General;

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

        return model;
    }
}
