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
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Tests.Psk31;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 319 task 3: **the PSK31 conversation card, with whose turn it is where the slot clock was.**
/// </summary>
/// <remarks>
/// <para>**THROUGH THE VIEW MODEL, THE WAY THE PANEL IS DRIVEN**: each transcript is one channel,
/// its text growing as a listener would list it, handed over on the path the tick takes.</para>
/// <para>**COMPUTED, NOT SEEN.** The window checks build the real window headless and walk the
/// visual tree, which says a control exists and is visible and says nothing about what it looks
/// like. The corpus was typed (FACT-004).</para>
/// </remarks>
public sealed class ThePsk31ConversationCardTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the cards are printed.</param>
    public ThePsk31ConversationCardTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Assertion 1: a message certainly addressed to the operator, from a callsign, opens exactly one card for that station.**</summary>
    [Fact]
    public void ACertainCallToTheOperatorOpensExactlyOneCardForThatStation()
    {
        var corpus = Psk31Corpus.Load();
        var textbook = Transcript(corpus, "01-textbook");
        var model = Panel();

        for (var lines = 1; lines <= textbook.Lines.Count; lines++)
        {
            model.ShowPsk31ChannelsForTests(new[] { Channel(1, 1000, textbook, lines) });

            _output.WriteLine("01-textbook through line " + lines + ": " + Describe(model));

            if (lines < 3)
            {
                // **A CQ AND HIS OWN ANSWER OPEN NOTHING.** Neither is addressed to him.
                Assert.Empty(model.DigitalCards);
                continue;
            }

            var card = Assert.Single(model.DigitalCards);

            Assert.True(card.IsPsk31);
            Assert.Equal("W1AW", card.Callsign);
        }
    }

    /// <summary>**Assertion 2: two answers make two cards, neither inherits, and a further message updates in place.**</summary>
    /// <remarks>
    /// **COMPOSED FROM TWO TRANSCRIPTS.** No transcript in the corpus has two stations calling the
    /// operator, so `01-textbook` and `02-chatty` are carried on two channels at once.
    /// </remarks>
    [Fact]
    public void TwoStationsCertainlyCallingMakeTwoCardsAndAFurtherMessageUpdatesInPlace()
    {
        var corpus = Psk31Corpus.Load();
        var textbook = Transcript(corpus, "01-textbook");
        var chatty = Transcript(corpus, "02-chatty");
        var model = Panel();

        model.ShowPsk31ChannelsForTests(new[] { Channel(1, 800, textbook, 3), Channel(2, 1300, chatty, 3) });

        _output.WriteLine("composed from 01-textbook and 02-chatty, each through line 3: " + Describe(model));

        Assert.Equal(2, model.DigitalCards.Count);

        var w1aw = model.DigitalCards.Single(c => c.Callsign == "W1AW");
        var g4xyz = model.DigitalCards.Single(c => c.Callsign == "G4XYZ");
        var at = model.DigitalCards.IndexOf(w1aw);

        Assert.True(w1aw.IsPsk31 && g4xyz.IsPsk31);
        Assert.Equal("Your turn", w1aw.StateWord);
        Assert.Equal("Your turn", g4xyz.StateWord);

        // **THE OPERATOR ANSWERS W1AW ONLY.**
        model.ShowPsk31ChannelsForTests(new[] { Channel(1, 800, textbook, 4), Channel(2, 1300, chatty, 3) });

        _output.WriteLine("01-textbook through line 4: " + Describe(model));

        Assert.Equal(2, model.DigitalCards.Count);
        Assert.Equal("W1AW", model.DigitalCards[at].Callsign);
        Assert.Equal("His turn", model.DigitalCards[at].StateWord);

        // **NEITHER INHERITS**: G4XYZ's card is still his own, with his own turn reading.
        // **REWRITTEN UNDER R12 BY WORK INSTRUCTION 385 TASK 2 (criterion 9.2).** It asserted the
        // same instance, and whether a station is still sending is now part of what a card says -
        // so a card whose station stopped mid-over and then handed back is rebuilt in place rather
        // than left saying he is sending. **What it guards is what it always meant**: the card
        // belongs to G4XYZ, it is one card, and it carries his reading and not W1AW's.
        var stillHis = model.DigitalCards.Single(c => c.Callsign == "G4XYZ");

        Assert.Equal("G4XYZ", stillHis.Callsign);
        Assert.Equal("Your turn", stillHis.StateWord);
        Assert.Equal(g4xyz.StateWord, stillHis.StateWord);

        model.ShowPsk31ChannelsForTests(new[] { Channel(1, 800, textbook, 5), Channel(2, 1300, chatty, 3) });

        _output.WriteLine("01-textbook through line 5: " + Describe(model));

        Assert.Equal(2, model.DigitalCards.Count);
        Assert.Equal("W1AW", model.DigitalCards[at].Callsign);
        Assert.Equal("Your turn", model.DigitalCards[at].StateWord);
    }

    /// <summary>
    /// **Assertion 3, rewritten under R12 by work instruction 371 task 1: a guessed answer to the
    /// operator opens his card as a guess, and a guessed line addressed to nobody opens none.**
    /// </summary>
    /// <remarks>
    /// **THE DOOR THIS GUARDED IS OPEN.** It walked `05-garbled` line by line and required an
    /// empty card panel throughout, including at line 3 - a report addressed to the operator,
    /// handing the turn back, uncertain because its digits are damaged. That is the case Tim hit
    /// on 2026-09-20. **The row's own behaviour is unchanged** and is still asserted: it is on his
    /// side and marked a guess.
    /// </remarks>
    [Fact]
    public void AGuessedAnswerOpensHisCardAndAGuessedAddresseeOpensNone()
    {
        var corpus = Psk31Corpus.Load();
        var garbled = Transcript(corpus, "05-garbled");
        var model = Panel();

        for (var lines = 1; lines <= garbled.Lines.Count; lines++)
        {
            model.ShowPsk31ChannelsForTests(new[] { Channel(1, 1000, garbled, lines) });

            var row = model.DigitalDecodes.Single(r => r.IsTextOnly);
            var onHisSide = model.DigitalMineDecodes.Contains(row);

            _output.WriteLine("05-garbled through line " + lines + ": " + Describe(model) + (onHisSide ? ", row on his side" : ""));

            if (lines < 3)
            {
                // **NOTHING HAS BEEN ADDRESSED TO HIM YET**: line 1 is a CQ to anybody and line 2
                // is his own answer.
                Assert.Empty(model.DigitalCards);
            }

            if (lines == 3)
            {
                Assert.True(onHisSide, "05-garbled line 3 left his side");
                Assert.True(row.IsGuess);

                // **AND HIS CARD IS THERE, AS A GUESS**, with the doubt beside what it offers.
                var card = Assert.Single(model.DigitalCards);

                Assert.Equal("K3ABC", card.Callsign);
                Assert.True(card.TurnIsGuess);
                Assert.Equal("not sure it is your turn", card.OfferNote);
            }
        }

        foreach (var name in new[] { "04-not-for-me", "06-cq-dx" })
        {
            var other = Panel();
            var transcript = Transcript(corpus, name);

            other.ShowPsk31ChannelsForTests(new[] { Channel(1, 1000, transcript, transcript.Lines.Count) });

            _output.WriteLine(name + ", whole: " + Describe(other));

            Assert.Empty(other.DigitalCards);
        }
    }

    /// <summary>**Assertion 4: the turn indicator is on the card, says unknown, and says a guess in words.**</summary>
    [Fact]
    public void TheTurnIndicatorIsOnTheCardAndSaysUnknownAndAGuessInWords()
    {
        var corpus = Psk31Corpus.Load();
        var textbook = Transcript(corpus, "01-textbook");
        var stream = string.Concat(textbook.Lines.Select(l => l.Text + "\n"));
        var model = Panel();
        var splitter = new Psk31MessageSplitter(corpus.Operator);
        var words = new List<string>();
        var sawSending = false;

        // **CHARACTER BY CHARACTER**, and the card's word printed after each message.
        for (var i = 1; i <= stream.Length; i++)
        {
            model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(1, 1000, 10.0, stream.Substring(0, i)) });

            var card = model.DigitalCards.SingleOrDefault(c => c.IsPsk31);
            var message = splitter.Add(stream[i - 1]);

            if (card is not null && card.StateWord == "He is still sending")
            {
                sawSending = true;
            }

            if (message is null)
            {
                continue;
            }

            var word = card?.StateWord ?? "(no card)";

            words.Add(word);

            _output.WriteLine("after message " + words.Count + " (" + message.Exchange.Speaker + " > " + message.Exchange.Addressee
                + " " + message.Exchange.Kind + "): card word [" + word + "]");
        }

        Assert.Equal(new[] { "(no card)", "(no card)", "Your turn", "His turn", "Your turn" }, words);
        Assert.True(sawSending, "the card never said he is still sending while characters arrived");

        // **UNKNOWN**: a message from him that names nobody he is sending to.
        var unknown = Channel(1, 1000, textbook, 3).Text + "de W1AW K\n";

        model = Panel();
        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(1, 1000, 10.0, unknown) });

        var unknownCard = Assert.Single(model.DigitalCards);

        _output.WriteLine("then [de W1AW K]: card word [" + unknownCard.StateWord + "], sentence [" + unknownCard.Sentence + "]");

        Assert.Equal("Unknown", unknownCard.StateWord);
        Assert.True(unknownCard.TurnIsGuess);

        // **A GUESS, IN WORDS**: a damaged report handed over to him.
        var guessed = unknown + "KC3QIS de W1AW 5#9 K\n";

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(1, 1000, 10.0, guessed) });

        var guessCard = Assert.Single(model.DigitalCards);

        _output.WriteLine("then [KC3QIS de W1AW 5#9 K]: card word [" + guessCard.StateWord + "], sentence [" + guessCard.Sentence + "]");

        // The card's word is 9.4's own, *your turn?* (work instruction 389, rewritten under R12
        // from a *guess* in the word); the sentence under it still says *guess* in words.
        Assert.Equal("Your turn?", guessCard.StateWord);
        Assert.Contains("guess", guessCard.Sentence, StringComparison.Ordinal);
        Assert.True(guessCard.TurnIsGuess);
    }

    /// <summary>**Assertion 5: no slot clock under PSK31; FT8 and FT4 still show it.**</summary>
    /// <remarks>
    /// **THE SLOT CLOCK IS NOT ON ANY CARD.** Unit 305 moved it above the panels (`SlotClock` in
    /// `MainWindow.axaml`), so what PSK31 must not show is that control, and a PSK31 card carries no
    /// ring of its own.
    /// </remarks>
    [AvaloniaFact]
    public void NoSlotClockUnderPsk31AndFt8AndFt4StillShowIt()
    {
        var corpus = Psk31Corpus.Load();
        var (window, model) = Window();

        foreach (var mode in new[] { "FT8", "FT4", "PSK31" })
        {
            model.ChooseDigitalModeCommand.Execute(mode);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var clock = window.GetVisualDescendants().OfType<Control>().FirstOrDefault(c => c.Name == "SlotClock");

            _output.WriteLine(mode + ": slot clock visible " + (clock?.IsVisible ?? false));

            Assert.NotNull(clock);
            Assert.Equal(mode != "PSK31", clock!.IsVisible);
        }

        model.ShowPsk31ChannelsForTests(new[] { Channel(1, 1000, Transcript(corpus, "01-textbook"), 3) });
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var cards = window.FindControl<ItemsControl>("DigitalContactCards");
        var card = Assert.Single(model.DigitalCards);

        Assert.NotNull(cards);
        Assert.False(card.ShowsRing);
        Assert.DoesNotContain(cards!.GetVisualDescendants().OfType<Control>(), c => c.Name == "SlotClock");
    }

    /// <summary>**Assertion 6: no state from name or QTH, and no station fact the parse did not supply.**</summary>
    [Fact]
    public void TheCardShowsNoStationFactTheParseDidNotSupply()
    {
        var corpus = Psk31Corpus.Load();
        var model = Panel();

        model.ShowPsk31ChannelsForTests(new[]
        {
            Channel(1, 800, Transcript(corpus, "01-textbook"), 3),
            Channel(2, 1300, Transcript(corpus, "02-chatty"), 3),
        });

        Assert.Equal(2, model.DigitalCards.Count);

        foreach (var card in model.DigitalCards)
        {
            var said = new[] { card.Callsign, card.Place, card.StateWord, card.Sentence, card.TimeLine, card.Detail, card.DetailHover, card.ActionLabel, card.ActionTip };

            _output.WriteLine(card.Callsign + ": place [" + card.Place + "], word [" + card.StateWord + "], sentence [" + card.Sentence
                + "], time [" + card.TimeLine + "], detail rows " + card.DetailRows.Count);

            foreach (var name in new[] { "BOB", "NEWINGTON", "Dave", "Reading", "Berkshire", "TIM", "Tim", "TRAFFORD" })
            {
                Assert.All(said, s => Assert.DoesNotContain(name, s, StringComparison.Ordinal));
            }

            Assert.Empty(card.DetailRows);
            Assert.Equal("", card.TimeLine);
            Assert.False(card.HasMessages);

            // **REWRITTEN UNDER R12 BY WORK INSTRUCTION 385 TASK 3 (criterion 9.3).** It asserted
            // that a card with no station facts offers no Log, which was the door this unit opens:
            // Log is on a conversation card from the moment the card exists, because a half
            // exchange is still a contact he made. **What it guards now is the rule**: the link is
            // there, and it is still never on a receipt.
            Assert.True(card.ShowsLogLink);
            Assert.False(card.IsCallToAnyone);
        }
    }

    /// <summary>**Assertion 8, rewritten by work instruction 323 task 1b under `PHASE_PLAN.md` §R12: nothing on the card or the panel transmits without the operator's click.**</summary>
    /// <remarks>
    /// <para>**WHAT THIS GUARDED BEFORE, AND WHY IT COULD NOT STAY.** Unit 319 wrote it while
    /// the PSK31 send door was shut, and it guarded *the shut door*: it scanned `src/` for the
    /// string `NowAsync`, and it pinned the four source lines of `CanTransmitIn` character for
    /// character. Both pins fail the moment the door is opened **even though the rule they
    /// exist for is untouched**, and §R12 says a test a session wrote while a door was shut is
    /// that session's successor's to rewrite, in its own commit, so that it guards the rule
    /// and not the door.</para>
    /// <para>**THE RULE IS THE ONE THAT IS THE OWNER'S** (§0.2): *Hamlet never transmits
    /// without the operator's click.* Nothing else about the shape of the code is this test's
    /// business, and it no longer reads a line of source.</para>
    /// <para>**THREE THINGS, ALL MEASURED THROUGH THE RUNNING APPLICATION AND ITS OWN RECORD.**
    /// (a) A whole conversation arrives, a card goes up, it becomes the operator's turn - and
    /// with nobody clicking anything, not one line of the transmit path is written. (b) The
    /// door PSK31 goes through is the same one FT8 goes through and it is still a door: a
    /// label with no modulator is refused by it. (c) A press composes, once, and nothing on
    /// the card keys anything - the count of composed sends equals the count of clicks.</para>
    /// <para>**COMPUTED, NOT SEEN**, and no radio was involved (FACT-006).</para>
    /// </remarks>
    [Fact]
    public void NothingOnTheCardTransmits()
    {
        var corpus = Psk31Corpus.Load();
        var folder = Path.Combine(
            Path.GetTempPath(), "hamlet-psk31-card-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(folder);

        try
        {
            // (a) **A WHOLE CONVERSATION, AND NOT ONE CLICK.**
            using (var telemetry = new JsonlTelemetry(folder, "no-click", _ => true))
            {
                var model = Panel(telemetry);
                var textbook = Transcript(corpus, "01-textbook");

                for (var through = 1; through <= textbook.Lines.Count; through++)
                {
                    model.ShowPsk31ChannelsForTests(new[] { Channel(1, 1000, textbook, through) });
                }

                // **NOT VACUOUS.** The card is up and it is his turn, which is the one
                // moment a card with a mind of its own would answer for him.
                var card = Assert.Single(model.DigitalCards);

                _output.WriteLine("no click: card " + card.Callsign + " [" + card.StateWord + "]");

                Assert.True(card.IsPsk31);
                Assert.Equal("W1AW", card.Callsign);
                Assert.Equal("Your turn", card.StateWord);
            }

            var quiet = Read(folder);

            foreach (var line in quiet.Where(Transmitting))
            {
                _output.WriteLine("WROTE WITHOUT A CLICK: " + line);
            }

            _output.WriteLine("lines written with no click: " + quiet.Count
                + ", of them on the transmit path: " + quiet.Count(Transmitting));

            // **THE WHOLE PATH, NOT ONE STAGE OF IT.** A request, a stage, a composed
            // macro, a keying, an unkeying and a transmission record: none of the six.
            Assert.DoesNotContain(quiet, Transmitting);

            // **AND THE FILE IS NOT EMPTY**, so the assertion above is about silence on
            // one path rather than about a sink that was never written to.
            Assert.NotEmpty(quiet);
        }
        finally
        {
            Remove(folder);
        }

        // (b) **THE ONE DOOR, AND IT IS STILL A DOOR.**
        foreach (var (mode, refusedForItsMode) in
            new[] { ("FT8", false), ("PSK31", false), ("WSPR", true) })
        {
            var model = Panel();

            model.ChooseDigitalModeCommand.Execute(mode);
            model.SendCallToAnyoneCommand.Execute(null);

            _output.WriteLine(mode.PadRight(6) + ": " + model.DigitalSendLine);

            // **REFUSED FOR BEING THIS MODE, WHICH IS NOT THE SAME AS REFUSED.** With no
            // radio on this machine every press stops somewhere; what is measured is
            // whether the mode gate is what stopped it.
            Assert.Equal(
                refusedForItsMode,
                model.DigitalSendLine.Contains("cannot send " + mode, StringComparison.Ordinal));
        }

        // (c) **ONE CLICK, ONE COMPOSED SEND, AND NOTHING ON THE CARD KEYS.**
        var pressed = Path.Combine(
            Path.GetTempPath(), "hamlet-psk31-press-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(pressed);

        try
        {
            using (var telemetry = new JsonlTelemetry(pressed, "one-click", _ => true))
            {
                var model = Panel(telemetry);

                model.ShowPsk31ChannelsForTests(
                    new[] { Channel(1, 1000, Transcript(corpus, "01-textbook"), 5) });

                model.SendCallToAnyoneCommand.Execute(null);
            }

            var after = Read(pressed);
            var composed = after.Where(l => Names(l, "psk31_send_composed")).ToList();
            var keyed = after.Where(l => Names(l, "psk31_send_keyed")).ToList();

            foreach (var line in after.Where(Transmitting))
            {
                _output.WriteLine("one click: " + line);
            }

            // **THE COUNT OF SENDS IS THE COUNT OF CLICKS.**
            Assert.Single(composed);

            // **AND IT IS THE CQ MACRO OF §R2 THAT WAS COMPOSED**, not FT8 tones on a
            // PSK31 frequency, which is the fault the shut door was standing in for.
            Assert.All(composed, l => Assert.Contains("\"macro\":\"cq\"", l, StringComparison.Ordinal));

            // **NOTHING KEYED.** No radio is connected on this machine, so the press
            // reaches the composer and stops; a keying here would be one nobody asked for.
            Assert.Empty(keyed);
        }
        finally
        {
            Remove(pressed);
        }
    }

    /// <summary>True where this line is any stage of the transmit path.</summary>
    /// <param name="line">One JSONL line.</param>
    /// <returns>True where a send was requested, staged, composed, keyed, unkeyed or recorded.</returns>
    private static bool Transmitting(string line)
        => Names(line, SendStage.EventName)
            || Names(line, TransmitRecord.EventName)
            || Names(line, "psk31_send_composed")
            || Names(line, "psk31_send_keyed")
            || Names(line, "psk31_send_unkeyed")
            || (Names(line, "operator_action")
                && line.Contains("\"send_requested\"", StringComparison.Ordinal));

    /// <summary>True where the line's event is this one.</summary>
    private static bool Names(string line, string eventName)
        => line.Contains("\"event\":\"" + eventName + "\"", StringComparison.Ordinal);

    /// <summary>Every line the sink wrote.</summary>
    private static List<string> Read(string folder)
        => Directory.GetFiles(folder, "*.jsonl").SelectMany(File.ReadAllLines).ToList();

    /// <summary>Takes the telemetry folder away; a left-over one is not a failure.</summary>
    private static void Remove(string folder)
    {
        try
        {
            Directory.Delete(folder, true);
        }
        catch (IOException)
        {
            // A left-over temp folder is not a test failure.
        }
    }

    /// <summary>**Assertion 8, drawn: a PSK31 card offers nothing until the parser is certain.**</summary>
    /// <remarks>
    /// **REWRITTEN UNDER §R12** (work instruction 323 task 3). It asserted no send control was
    /// drawn on a PSK31 card at all, which is the shut door; from unit 323 a card offers the
    /// next macro for one click when - and only when - the parser is certain it is the
    /// operator's turn (§R1). **What it guards now is that side of it**: on a conversation
    /// where it is not his turn there is nothing to press, and no Log, because the Log arrives
    /// only when both sides have closed the exchange.
    /// </remarks>
    [AvaloniaFact]
    public void APsk31CardOffersNothingWhileItIsNotHisTurn()
    {
        var corpus = Psk31Corpus.Load();
        var (window, model) = Window();

        model.ChooseDigitalModeCommand.Execute("PSK31");

        // **THROUGH HIS REPORT AND THE OPERATOR'S OWN ANSWER TO IT**, which is a conversation
        // standing at *his turn* - nobody is waiting on a click.
        model.ShowPsk31ChannelsForTests(new[] { Channel(1, 1000, Transcript(corpus, "01-textbook"), 4) });
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var card = Assert.Single(model.DigitalCards);
        var cards = window.FindControl<ItemsControl>("DigitalContactCards");

        Assert.NotNull(cards);

        var drawn = cards!.GetVisualDescendants().OfType<TextBlock>().Where(t => t.IsEffectivelyVisible).Select(t => t.Text ?? "").ToList();
        var buttons = cards.GetVisualDescendants().OfType<Button>().Where(b => b.IsEffectivelyVisible).ToList();

        _output.WriteLine("the card is [" + card.StateWord + "] and offers [" + card.OfferedMacro + "]");
        _output.WriteLine("drawn on the card: " + string.Join(" | ", drawn.Where(t => t.Length > 0)));
        _output.WriteLine("visible buttons: " + string.Join(" | ", buttons.Select(b => (b.Content?.ToString() ?? "") + " -> " + (b.Command?.GetType().Name ?? "no command"))));

        // **NOT VACUOUS**: the card is in the tree, and it is not his turn.
        Assert.Contains("W1AW", drawn);
        Assert.Equal("His turn", card.StateWord);

        // **NOTHING THAT TRANSMITS IS DRAWN**, which is what this test is for (§0.2).
        Assert.DoesNotContain(buttons, b => ReferenceEquals(b.Command, model.CardActionCommand));
        Assert.DoesNotContain(buttons, b => ReferenceEquals(b.Command, model.ShowMessagesCommand));

        // **REWRITTEN UNDER R12 BY WORK INSTRUCTION 385 TASK 3 (criterion 9.3).** The Log button
        // was in the list of what must not be drawn, which was the door this unit opens - Log is
        // on a conversation card from the moment the card exists. **It is drawn and it transmits
        // nothing**: that is what is asserted now, and it is more than the line it replaced.
        Assert.Contains(buttons, b => ReferenceEquals(b.Command, model.LogStationCommand));
    }

    private static Psk31CorpusTranscript Transcript(Psk31Corpus corpus, string name)
        => corpus.Transcripts.Single(t => t.Name == name);

    private static Psk31Channel Channel(int id, double hz, Psk31CorpusTranscript transcript, int lines)
        => new(id, hz, 10.0, string.Concat(transcript.Lines.Take(lines).Select(l => l.Text + "\n")));

    private static string Describe(MainWindowViewModel model)
        => model.DigitalCards.Count + " card(s)"
            + string.Concat(model.DigitalCards.Select(c => " " + c.Callsign + " [" + c.StateWord + "]"));

    private static (MainWindow Window, MainWindowViewModel Model) Window()
    {
        var model = Panel();

        model.DigitalDecodedExpanded = true;

        var window = new MainWindow { DataContext = model };

        window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        return (window, model);
    }

    private static MainWindowViewModel Panel(JsonlTelemetry? telemetry = null)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = Psk31Corpus.Load().Operator;
        settings.Operator.GridSquare = "FN00DJ";

        var model = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        var entities = Array.Empty<string>()
            .Select(DxccPrefixes.EntityOf)
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .ToList();

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.UseNudgeSetForTests(new NudgeSet(entities, entities.Select(DxccContinents.Of)));
        model.ChooseDigitalModeCommand.Execute("PSK31");

        return model;
    }
}
