using System;
using System.Collections.Generic;
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
/// Work instruction 319 task 4, the panel half: **the card knows which macro it would offer, and draws no button for it.**
/// </summary>
/// <remarks>
/// **COMPUTED, NOT SEEN.** The window is built headless and its visual tree walked; that says what
/// exists and is visible, and nothing about how it looks. The corpus was typed (FACT-004).
/// </remarks>
public sealed class ThePsk31OfferTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the offers are printed.</param>
    public ThePsk31OfferTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Assertion 1 through the card: a certain your turn names the macro; nothing else does.**</summary>
    [Fact]
    public void TheCardNamesTheMacroOnlyOnACertainYourTurn()
    {
        var corpus = Psk31Corpus.Load();
        var textbook = corpus.Transcripts.Single(t => t.Name == "01-textbook");
        var model = Panel();
        var named = new List<string>();

        for (var lines = 3; lines <= textbook.Lines.Count; lines++)
        {
            model.ShowPsk31ChannelsForTests(new[] { Channel(textbook, lines) });

            var card = Assert.Single(model.DigitalCards);

            _output.WriteLine("01-textbook through line " + lines + ": [" + card.StateWord + "] offers [" + card.OfferedMacro + "]");

            named.Add(card.OfferedMacro);
        }

        Assert.Equal(new[] { "Report", "", "Confirm" }, named);

        var unknown = Channel(textbook, 3).Text + "de W1AW K\n";
        var guessed = unknown + "KC3QIS de W1AW 5#9 K\n";

        foreach (var text in new[] { unknown, guessed })
        {
            model = Panel();
            model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(1, 1000, 10.0, text) });

            var card = Assert.Single(model.DigitalCards);

            _output.WriteLine("[" + card.StateWord + "] offers [" + card.OfferedMacro + "]");

            Assert.Equal("", card.OfferedMacro);
        }
    }

    /// <summary>**Assertion 4: the offer is one button, and it is the one the engine named.**</summary>
    /// <remarks>
    /// **REWRITTEN UNDER §R12** (work instruction 323 task 3). It read *nothing clickable is
    /// drawn for the offer while the door is shut* and asserted there was no button at all,
    /// which is the shut door rather than the rule; the door is open from unit 323 on and the
    /// offer is what the operator clicks. **What is guarded now is that there is exactly one
    /// of them, that it is the macro `Psk31Offer` named, and that it goes through the one
    /// send command** - so a card can still never do anything the engine did not offer, and
    /// it can never do it twice.
    /// </remarks>
    [AvaloniaFact]
    public void TheOfferIsOneButtonAndItIsTheOneTheEngineNamed()
    {
        var corpus = Psk31Corpus.Load();
        var model = Panel();

        model.DigitalDecodedExpanded = true;

        var window = new MainWindow { DataContext = model };

        window.Show();
        model.ShowPsk31ChannelsForTests(new[] { Channel(corpus.Transcripts.Single(t => t.Name == "01-textbook"), 5) });
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var card = Assert.Single(model.DigitalCards);
        var cards = window.FindControl<ItemsControl>("DigitalContactCards");

        Assert.NotNull(cards);

        var drawn = cards!.GetVisualDescendants().OfType<TextBlock>().Where(t => t.IsEffectivelyVisible).Select(t => t.Text ?? "").ToList();
        var buttons = cards.GetVisualDescendants().OfType<Button>().Where(b => b.IsEffectivelyVisible).ToList();

        _output.WriteLine("the card offers [" + card.OfferedMacro + "]");
        _output.WriteLine("drawn: " + string.Join(" | ", drawn.Where(t => t.Length > 0)));
        _output.WriteLine("visible buttons: " + string.Join(" | ", buttons.Select(b => b.Content?.ToString() ?? "")));

        Assert.Equal("Confirm", card.OfferedMacro);
        Assert.Contains("W1AW", drawn);

        // **ONE BUTTON, AND IT IS THE ONE SEND COMMAND.**
        var offer = Assert.Single(
            buttons, b => ReferenceEquals(b.Command, model.CardActionCommand));

        Assert.Same(card, offer.CommandParameter);

        // **IT SAYS WHAT IT DOES, NOT WHAT THE FIELD SHAPE IS CALLED** (§0.5.1, and the
        // FT8 side's own rule): the text that would go on the air is on the hover.
        Assert.Equal(card.ActionLabel, offer.Content?.ToString());

        // **AND THE BARE MACRO NAME IS NOT ON THE CARD ANYWHERE** (§0.5.1). `Confirm` is
        // this application's word for a step in an exchange and means nothing to somebody
        // reading a card for the first time; the button says what pressing it does.
        Assert.DoesNotContain(drawn, t => t.Trim() == card.OfferedMacro);
    }

    private static Psk31Channel Channel(Psk31CorpusTranscript transcript, int lines)
        => new(1, 1000, 10.0, string.Concat(transcript.Lines.Take(lines).Select(l => l.Text + "\n")));

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
