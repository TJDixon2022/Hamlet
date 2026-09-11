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

    /// <summary>**Assertion 4: nothing clickable is drawn for the offer while the door is shut.**</summary>
    [AvaloniaFact]
    public void NothingClickableIsDrawnForTheOfferWhileTheDoorIsShut()
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

        foreach (var macro in new[] { "Answer", "Report", "Confirm" })
        {
            Assert.DoesNotContain(drawn, t => t.Contains(macro, StringComparison.Ordinal));
            Assert.DoesNotContain(buttons, b => (b.Content?.ToString() ?? "").Contains(macro, StringComparison.Ordinal));
        }

        Assert.DoesNotContain(buttons, b => ReferenceEquals(b.Command, model.CardActionCommand));
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
