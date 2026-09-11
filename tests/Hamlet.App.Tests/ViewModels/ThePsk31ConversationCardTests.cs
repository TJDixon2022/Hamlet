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

        // **NEITHER INHERITS**: G4XYZ's card is the same card, still his own turn reading.
        Assert.Same(g4xyz, model.DigitalCards.Single(c => c.Callsign == "G4XYZ"));
        Assert.Equal("Your turn", g4xyz.StateWord);

        model.ShowPsk31ChannelsForTests(new[] { Channel(1, 800, textbook, 5), Channel(2, 1300, chatty, 3) });

        _output.WriteLine("01-textbook through line 5: " + Describe(model));

        Assert.Equal(2, model.DigitalCards.Count);
        Assert.Equal("W1AW", model.DigitalCards[at].Callsign);
        Assert.Equal("Your turn", model.DigitalCards[at].StateWord);
    }

    /// <summary>**Assertion 3: a guessed addressee opens no card, and the row stays on his side.**</summary>
    [Fact]
    public void AGuessedAddresseeOpensNoCard()
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

            Assert.Empty(model.DigitalCards);

            if (lines == 3)
            {
                Assert.True(onHisSide, "05-garbled line 3 left his side");
                Assert.True(row.IsGuess);
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

        Assert.Contains("guess", guessCard.StateWord, StringComparison.Ordinal);
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
            Assert.False(card.ShowsLogLink);
        }
    }

    /// <summary>**Assertion 8: nothing on the card transmits.**</summary>
    [Fact]
    public void NothingOnTheCardTransmits()
    {
        var corpus = Psk31Corpus.Load();
        var model = Panel();

        model.ShowPsk31ChannelsForTests(new[] { Channel(1, 1000, Transcript(corpus, "01-textbook"), 5) });

        var card = Assert.Single(model.DigitalCards);

        Assert.Equal(Ft8CardActionKind.None, card.ActionKind);
        Assert.False(card.HasAction);
        Assert.False(card.ShowsLogLink);
        Assert.False(card.ShowsRing);
        Assert.Equal("", card.ActionMessage);

        var src = Path.Combine(Psk31Corpus.Root(), "src");
        var arms = new List<string>();
        var nows = new List<string>();

        foreach (var file in Directory.EnumerateFiles(src, "*.cs", SearchOption.AllDirectories))
        {
            var lines = CodeOnly(File.ReadAllText(file)).Split('\n');
            var relative = Path.GetRelativePath(src, file).Replace('\\', '/');

            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(".Arm(", StringComparison.Ordinal))
                {
                    arms.Add(relative + ": " + lines[i].Trim());
                }

                if (relative.StartsWith("Hamlet.App/", StringComparison.Ordinal)
                    && lines[i].Contains("NowAsync", StringComparison.Ordinal))
                {
                    nows.Add(relative + ": " + lines[i].Trim());
                }
            }
        }

        _output.WriteLine(".Arm( in src: " + arms.Count + " - " + string.Join(" | ", arms));
        _output.WriteLine("NowAsync in src/Hamlet.App: " + nows.Count);

        Assert.Single(arms);
        Assert.Empty(nows);

        var shell = File.ReadAllText(Path.Combine(src, "Hamlet.App", "ViewModels", "MainWindowViewModel.cs")).Replace("\r", "");

        const string door =
            "    private static bool CanTransmitIn(string? chosen)\n"
            + "        => chosen is null\n"
            + "            || string.Equals(chosen, \"FT8\", StringComparison.Ordinal)\n"
            + "            || string.Equals(chosen, \"FT4\", StringComparison.Ordinal);\n";

        Assert.Contains(door, shell, StringComparison.Ordinal);
    }

    /// <summary>**Assertion 8, drawn: no control on a PSK31 card that would transmit or log is visible.**</summary>
    [AvaloniaFact]
    public void NothingClickableThatWouldTransmitIsDrawnOnAPsk31Card()
    {
        var corpus = Psk31Corpus.Load();
        var (window, model) = Window();

        model.ChooseDigitalModeCommand.Execute("PSK31");
        model.ShowPsk31ChannelsForTests(new[] { Channel(1, 1000, Transcript(corpus, "01-textbook"), 5) });
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var cards = window.FindControl<ItemsControl>("DigitalContactCards");

        Assert.NotNull(cards);

        var drawn = cards!.GetVisualDescendants().OfType<TextBlock>().Where(t => t.IsEffectivelyVisible).Select(t => t.Text ?? "").ToList();
        var buttons = cards.GetVisualDescendants().OfType<Button>().Where(b => b.IsEffectivelyVisible).ToList();

        _output.WriteLine("drawn on the card: " + string.Join(" | ", drawn.Where(t => t.Length > 0)));
        _output.WriteLine("visible buttons: " + string.Join(" | ", buttons.Select(b => (b.Content?.ToString() ?? "") + " -> " + (b.Command?.GetType().Name ?? "no command"))));

        // **NOT VACUOUS**: the card is in the tree.
        Assert.Contains("W1AW", drawn);

        Assert.DoesNotContain(buttons, b => ReferenceEquals(b.Command, model.CardActionCommand));
        Assert.DoesNotContain(buttons, b => ReferenceEquals(b.Command, model.LogStationCommand));
        Assert.DoesNotContain(buttons, b => ReferenceEquals(b.Command, model.ShowMessagesCommand));
    }

    private static Psk31CorpusTranscript Transcript(Psk31Corpus corpus, string name)
        => corpus.Transcripts.Single(t => t.Name == name);

    private static Psk31Channel Channel(int id, double hz, Psk31CorpusTranscript transcript, int lines)
        => new(id, hz, 10.0, string.Concat(transcript.Lines.Take(lines).Select(l => l.Text + "\n")));

    private static string Describe(MainWindowViewModel model)
        => model.DigitalCards.Count + " card(s)"
            + string.Concat(model.DigitalCards.Select(c => " " + c.Callsign + " [" + c.StateWord + "]"));

    private static string CodeOnly(string source)
        => string.Join('\n', source.Split('\n').Where(line => !line.TrimStart().StartsWith("//", StringComparison.Ordinal)));

    private static (MainWindow Window, MainWindowViewModel Model) Window()
    {
        var model = Panel();

        model.DigitalDecodedExpanded = true;

        var window = new MainWindow { DataContext = model };

        window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        return (window, model);
    }

    private static MainWindowViewModel Panel()
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = Psk31Corpus.Load().Operator;
        settings.Operator.GridSquare = "FN00DJ";

        var model = new MainWindowViewModel(settings, null)
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
