using System;
using System.Collections.Generic;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Tests.Psk31;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 323 task 3: **answering a station, and the rest of the exchange.**
/// </summary>
/// <remarks>
/// <para>**ONE SCRIPTED CONTACT, DRIVEN THE WAY THE OPERATOR DRIVES IT.** The other
/// station's half arrives as text on a channel, a line at a time, exactly as the listener
/// would list it; Hamlet's half happens only where this test clicks something. **No line of
/// the corpus that the operator sent is ever fed in as if it had been heard** - the whole
/// point is that Hamlet's half comes from clicks.</para>
/// <para>**AND THE RULE THAT IS TIM'S IS THE LAST ASSERTION** (§0.2): over the whole
/// exchange, the number of transmissions equals the number of clicks.</para>
/// <para>**COMPUTED, NOT SEEN.** No window is opened here; what is asserted is the card's
/// own values and what reached a sink that counts.</para>
/// </remarks>
public sealed class ThePsk31ExchangeTests
{
    private const long On20m = 14_070_000;
    private const double HisHz = 1234;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the cards are printed.</param>
    public ThePsk31ExchangeTests(ITestOutputHelper output) => _output = output;

    /// <summary>**1: clicking a CQ row sends one Answer at his offset and opens his card at his turn.**</summary>
    [Fact]
    public void ClickingACqRowAnswersHimOnHisOwnFrequency()
    {
        var (model, sink) = Panel();

        Hear(model, 1);

        var row = Assert.Single(model.DigitalDecodes);

        _output.WriteLine("row: " + row.Hz + " Hz  " + row.Message.Trim());

        // **THE ROW OFFERS ONE THING AND IT NAMES HIM.**
        var label = model.Psk31AnswerLabelFor(row);

        Assert.Equal("Answer W1AW", label);

        model.AnswerPsk31Command.Execute(row);
        Settle(model);

        _output.WriteLine("send line: " + model.DigitalSendLine);
        _output.WriteLine("cards    : " + Describe(model));

        // **ONE TRANSMISSION FOR ONE CLICK, AND IT WENT OUT WHERE HE IS.**
        Assert.Equal(1, sink.TimesCalled);
        Assert.Contains(
            Psk31Macros.Answer("W1AW", "KC3QIS"),
            model.DigitalSendLine,
            StringComparison.Ordinal);

        var card = Assert.Single(model.DigitalCards);

        Assert.Equal("W1AW", card.Callsign);
        Assert.True(card.IsPsk31);

        // **AND THE BALL IS WITH HIM**, because Hamlet has just handed over.
        Assert.Equal("His turn", card.StateWord);
        Assert.False(card.HasAction);
    }

    /// <summary>**2 and 3: the offer follows the exchange, and an uncertain line offers nothing.**</summary>
    [Fact]
    public void TheOfferFollowsTheExchangeAndNeverFollowsAGuess()
    {
        var (model, sink) = Panel();

        Hear(model, 1);
        model.AnswerPsk31Command.Execute(model.DigitalDecodes[0]);
        Settle(model);

        // **HIS CERTAIN REPORT OFFERS THE REPORT.**
        Hear(model, 2);

        var afterHisReport = Assert.Single(model.DigitalCards);

        _output.WriteLine("after his report : [" + afterHisReport.StateWord + "] "
            + afterHisReport.OfferedMacro + " -> " + afterHisReport.ActionLabel);

        Assert.Equal("Your turn", afterHisReport.StateWord);
        Assert.Equal(Psk31Macro.Report, afterHisReport.Offered);
        Assert.True(afterHisReport.HasAction);

        // **ONE CLICK, ONE REPORT.**
        model.CardActionCommand.Execute(afterHisReport);
        Settle(model);

        Assert.Equal(2, sink.TimesCalled);

        var afterOurReport = Assert.Single(model.DigitalCards);

        _output.WriteLine("after our report : [" + afterOurReport.StateWord + "] "
            + afterOurReport.Sentence);

        Assert.Equal("His turn", afterOurReport.StateWord);
        Assert.False(afterOurReport.HasAction);

        // **HIS CERTAIN SIGN-OFF OFFERS THE CONFIRMATION, AND NOT BEFORE.**
        Hear(model, 3);

        var beforeConfirm = Assert.Single(model.DigitalCards);

        _output.WriteLine("after his 73     : [" + beforeConfirm.StateWord + "] "
            + beforeConfirm.OfferedMacro);

        Assert.Equal(Psk31Macro.Confirm, beforeConfirm.Offered);

        // **AN UNCERTAIN LINE OFFERS NOTHING AND THE CARD SAYS WHY**, on its own channel so
        // the exchange above is untouched.
        var (guessed, _) = Panel();

        Hear(guessed, 1, "05-garbled");
        guessed.AnswerPsk31Command.Execute(guessed.DigitalDecodes[0]);
        Settle(guessed);

        Hear(guessed, 2, "05-garbled");

        var uncertain = Assert.Single(guessed.DigitalCards);

        _output.WriteLine("on a guess       : [" + uncertain.StateWord + "] "
            + uncertain.Sentence);

        Assert.Equal(Psk31Macro.None, uncertain.Offered);
        Assert.False(uncertain.HasAction);
        Assert.Contains(
            "Waiting to be sure it is your turn",
            uncertain.Sentence,
            StringComparison.Ordinal);
    }

    /// <summary>**4 and 5: his certain 73 finishes it, the Log appears, and keyings equal clicks.**</summary>
    [Fact]
    public void HisSignOffFinishesItAndNothingWentOutWithoutAClick()
    {
        var (model, sink) = Panel();
        var clicks = 0;

        Hear(model, 1);

        model.AnswerPsk31Command.Execute(model.DigitalDecodes[0]);
        clicks++;
        Settle(model);

        Hear(model, 2);

        model.CardActionCommand.Execute(model.DigitalCards[0]);
        clicks++;
        Settle(model);

        Hear(model, 3);

        model.CardActionCommand.Execute(model.DigitalCards[0]);
        clicks++;
        Settle(model);

        var card = Assert.Single(model.DigitalCards);

        _output.WriteLine("at the end: [" + card.StateWord + "] " + card.Sentence);
        _output.WriteLine("clicks " + clicks + ", transmissions " + sink.TimesCalled);

        // **FINISHED, BECAUSE BOTH SIDES CLOSED IT.**
        Assert.Equal("Finished", card.StateWord);

        // **AND THE LOG IS HERE, ON HIS CARD** (§R8: never on the receipt).
        Assert.True(card.ShowsLogLink);

        // **THE RULE THAT IS TIM'S, OVER THE WHOLE EXCHANGE** (§0.2).
        Assert.Equal(clicks, sink.TimesCalled);
    }

    /// <summary>**6: his carrier has gone and his CQ row stays; clicking it sends one Answer at his offset and opens his card.**</summary>
    /// <remarks>
    /// **HE MAY BE LISTENING** (work instruction 355 task 3). A station who called CQ and dropped his
    /// carrier to listen is the station most likely to hear an answer, so an ended row is still a
    /// station: it keeps what it read of him and the fade, and the click is the same one click.
    /// </remarks>
    [Fact]
    public void ClickingAnEndedCqRowStillAnswersHimAndOpensHisCard()
    {
        var folder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "hamlet-u355-ended-cq-" + Guid.NewGuid().ToString("N"));

        System.IO.Directory.CreateDirectory(folder);

        var telemetry = new Hamlet.RadioEngine.Telemetry.JsonlTelemetry(folder, "355", _ => true);
        var (model, sink) = Panel(telemetry);

        Hear(model, 1);

        var live = Assert.Single(model.DigitalDecodes);
        var sender = live.Sender;
        var country = live.SenderHelp;

        // **HIS CARRIER GOES.**
        model.ShowPsk31ChannelsForTests(Array.Empty<Psk31Channel>());

        var row = Assert.Single(model.DigitalDecodes);

        _output.WriteLine("ended row: " + row.Hz + " Hz [" + row.EndedWord + "] " + row.Sender + " (" + row.SenderHelp + ") opacity "
            + row.RowOpacity + "  " + row.Message.Trim());

        Assert.True(row.Ended);
        Assert.Equal(sender, row.Sender);
        Assert.Equal(country, row.SenderHelp);
        Assert.Equal(0.55, row.RowOpacity);
        Assert.Equal("Answer W1AW", model.Psk31AnswerLabelFor(row));

        model.AnswerPsk31Command.Execute(row);
        Settle(model);

        _output.WriteLine("send line: " + model.DigitalSendLine);
        _output.WriteLine("cards    : " + Describe(model));

        Assert.Equal(1, sink.TimesCalled);
        Assert.Contains(Psk31Macros.Answer("W1AW", "KC3QIS"), model.DigitalSendLine, StringComparison.Ordinal);
        // **AT HIS OFFSET**, as the send path recorded it when it composed the Answer. The send line
        // names the offset only while the send is going out, and it has finished by now.
        telemetry.Dispose();

        var composed = System.IO.Directory.GetFiles(folder, "*.jsonl")
            .SelectMany(System.IO.File.ReadAllLines)
            .Select(l => System.Text.Json.JsonDocument.Parse(l).RootElement)
            .Where(e => e.GetProperty("event").GetString() == "psk31_send_composed")
            .ToList();

        var offset = Assert.Single(composed).GetProperty("data").GetProperty("offsetHz").GetDouble();

        _output.WriteLine("composed at: " + offset + " Hz");

        Assert.Equal(HisHz, offset);

        var card = Assert.Single(model.DigitalCards);

        Assert.Equal("W1AW", card.Callsign);
        Assert.True(card.IsPsk31);
        Assert.Equal("His turn", card.StateWord);
    }

    /// <summary>Hands the panel the other station's half, a line at a time.</summary>
    /// <param name="model">The panel.</param>
    /// <param name="lines">How many of his lines have arrived.</param>
    /// <param name="name">Which transcript.</param>
    /// <remarks>
    /// **HIS LINES ONLY.** A transcript holds both halves; the operator's own are what this
    /// unit's clicks produce, and feeding them in as though they had been heard would prove
    /// nothing about whether a click was needed to send them.
    /// </remarks>
    private static void Hear(
        MainWindowViewModel model, int lines, string name = "01-textbook")
    {
        var corpus = Psk31Corpus.Load();

        var his = corpus.Transcripts
            .Single(t => t.Name == name)
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

    private static string Describe(MainWindowViewModel model)
        => model.DigitalCards.Count + " card(s)"
            + string.Concat(model.DigitalCards.Select(
                c => " " + c.Callsign + " [" + c.StateWord + "]"));

    /// <summary>**R29: a right-click on a row that is not a CQ makes his card, and sends nothing.**</summary>
    /// <remarks>
    /// **WHAT TIM COULD NOT DO BEFORE.** On 2026-09-14 he read a whole QSO between KE0JBT and
    /// N7WE off 14.070 and had no way to open either station: a card appeared only where one had
    /// certainly addressed him, or where Hamlet had already sent to one. Neither was true of two
    /// people talking to each other.
    /// </remarks>
    [Fact]
    public void ARightClickOnARowThatIsNotACqMakesHisCard()
    {
        var (model, sink) = Panel();

        // **`04-not-for-me` IS TIM'S OWN CASE**: a full exchange between two other stations,
        // with nothing addressed to the operator. Its first two lines are F4DIA's call and
        // EI4GNB's answer to him, so the latest complete message is EI4GNB's and it is a
        // conversation the operator is only reading. **A first draft of this test used
        // `01-textbook` and found a card already there**, correctly: that station's second
        // line is addressed to KC3QIS, which is the rule that already made one.
        Hear(model, 2, "04-not-for-me");

        var row = Assert.Single(model.DigitalDecodes);

        _output.WriteLine("row    : " + row.Hz + " Hz  " + Flat(row.Message));
        _output.WriteLine("reading: " + row.Reading?.Kind + ", speaker " + row.Sender);

        // **NOT A CQ AND NOT FOR HIM, SO THE OLD MENU OFFERED NOTHING AND NO CARD EXISTED.**
        Assert.Null(model.Psk31AnswerLabelFor(row));
        Assert.Empty(model.DigitalCards);
        Assert.Equal("EI4GNB", model.Psk31StationOn(row));

        model.OpenPsk31CardCommand.Execute(row);

        _output.WriteLine("cards  : " + Describe(model));

        var card = Assert.Single(model.DigitalCards);

        Assert.Equal("EI4GNB", card.Callsign);
        Assert.True(card.IsPsk31);

        // **AND NOTHING WENT ON THE AIR** (0.2). Opening a card is not a transmission.
        Assert.Equal(0, sink.TimesCalled);
        Assert.False(model.HasSomethingToStop);
    }

    /// <summary>**R29: on a CQ row the same right-click makes the card, and Answer is still offered.**</summary>
    [Fact]
    public void OnACqRowTheRightClickMakesTheCardAndAnswerIsStillOffered()
    {
        var (model, sink) = Panel();

        Hear(model, 1);

        var row = Assert.Single(model.DigitalDecodes);

        model.OpenPsk31CardCommand.Execute(row);

        var card = Assert.Single(model.DigitalCards);

        _output.WriteLine("cards : " + Describe(model));
        _output.WriteLine("answer: " + model.Psk31AnswerLabelFor(row));

        Assert.Equal("W1AW", card.Callsign);

        // **THE CQ MENU IS UNTOUCHED** (unit 323 task 3): the card is what the right-click
        // makes, and Answer is what the menu still offers on top of it.
        Assert.Equal("Answer W1AW", model.Psk31AnswerLabelFor(row));

        Assert.Equal(0, sink.TimesCalled);
    }

    /// <summary>**R29: a second right-click brings the card forward and never makes a second one.**</summary>
    /// <remarks>
    /// **FORWARD IS THIS UNIT'S OWN READING OF *focuses*.** The panel has no selection of its
    /// own, so the card moves to the front of the list; what is not the unit's choice is that it
    /// must not duplicate, and `_psk31Cards` is keyed by callsign so it cannot.
    /// </remarks>
    [Fact]
    public void ASecondRightClickBringsTheCardForwardAndNeverDuplicates()
    {
        var (model, sink) = Panel();

        // **TWO STATIONS ON THE PANEL AT ONCE**, so *forward* is a question with an answer.
        var corpus = Psk31Corpus.Load();

        model.ShowPsk31ChannelsForTests(new[]
        {
            new Psk31Channel(1, HisHz, 10.0, HisLines(corpus, "01-textbook", 2)),
            new Psk31Channel(2, HisHz + 200, 8.0, HisLines(corpus, "02-chatty", 2)),
        });

        var rows = model.DigitalDecodes.ToList();

        Assert.Equal(2, rows.Count);

        var first = rows.Single(r => r.Sender == "W1AW");
        var second = rows.Single(r => r.Sender == "G4XYZ");

        model.OpenPsk31CardCommand.Execute(first);
        model.OpenPsk31CardCommand.Execute(second);

        _output.WriteLine("after two : " + Describe(model));

        Assert.Equal(2, model.DigitalCards.Count);
        Assert.Equal("G4XYZ", model.DigitalCards[0].Callsign);

        // **THE SECOND CLICK ON THE FIRST STATION.**
        model.OpenPsk31CardCommand.Execute(first);

        _output.WriteLine("after re  : " + Describe(model));

        Assert.Equal(2, model.DigitalCards.Count);
        Assert.Equal("W1AW", model.DigitalCards[0].Callsign);

        // **AND A THIRD AND FOURTH CLICK STILL MAKE NO NEW CARD.**
        model.OpenPsk31CardCommand.Execute(first);
        model.OpenPsk31CardCommand.Execute(second);

        Assert.Equal(2, model.DigitalCards.Count);
        Assert.Equal(0, sink.TimesCalled);
    }

    /// <summary>One row's text on one line, so a printed trace stays readable.</summary>
    private static string Flat(string text)
        => string.Join(" / ", text.Split(
                new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim()));

    /// <summary>His lines from one transcript, as one channel's text.</summary>
    private static string HisLines(Psk31Corpus corpus, string name, int lines)
        => string.Concat(corpus.Transcripts
            .Single(t => t.Name == name)
            .Lines
            .Where(l => !string.Equals(l.Frm, corpus.Operator, StringComparison.OrdinalIgnoreCase))
            .Take(lines)
            .Select(l => l.Text + "\n"));

    private static (MainWindowViewModel Model, FakeSink Sink) Panel(
        Hamlet.RadioEngine.Telemetry.JsonlTelemetry? telemetry = null)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = Psk31Corpus.Load().Operator;
        settings.Operator.GridSquare = "FN00DJ";
        settings.Operator.OperatorName = "Tim";
        settings.Operator.Location = "Trafford PA";
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

        var port = new FakePort();
        var sink = new FakeSink();

        model.UseRigPortForTests(port);
        model.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(port, sink)));

        return (model, sink);
    }
}
