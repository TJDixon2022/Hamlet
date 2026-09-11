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

    private static (MainWindowViewModel Model, FakeSink Sink) Panel()
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = Psk31Corpus.Load().Operator;
        settings.Operator.GridSquare = "FN00DJ";
        settings.Operator.OperatorName = "Tim";
        settings.Operator.Location = "Trafford PA";
        settings.Operator.LicenseClass = LicenseClass.General;

        var model = new MainWindowViewModel(settings, null)
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
