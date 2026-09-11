using System;
using System.Collections.Generic;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Tests.Psk31;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 320 task 3 assertion 5, item 41: **a PSK31 card is handed the grid from a certain message, and never from a guess.**
/// </summary>
/// <remarks>
/// <para>**THE SENTENCE THIS CORRECTS** (item 41). W1AW's card said *"He has not put a grid square on the
/// air"* after his certain report carried FN31. A picture binds as hard as a sentence (HM-DEC-092), so the
/// map row was a false sentence on the panel.</para>
/// <para>**ONLY THIS ASSERTION OF TASK 3 IS HERE.** The others are about the receipt a PSK31 press makes,
/// and unit 320 did not build the press (its report, section 1). The class is named for what it asserts
/// rather than for the task, so it does not promise receipt tests it does not hold.</para>
/// <para>**COMPUTED, NOT SEEN.** The corpus was typed (FACT-004).</para>
/// </remarks>
public sealed class ThePsk31CardIsHandedHisGridTests
{
    /// <summary>What the map row says when the card has no grid.</summary>
    private const string NoGrid = "has not put a grid square on the air";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the cards are printed.</param>
    public ThePsk31CardIsHandedHisGridTests(ITestOutputHelper output) => _output = output;

    /// <summary>**After his certain report carrying FN31, W1AW's card knows his grid, and keeps his and not the operator's.**</summary>
    [Fact]
    public void AfterHisCertainReportCarryingFn31TheCardKnowsWhereHeIs()
    {
        var corpus = Psk31Corpus.Load();
        var textbook = Transcript(corpus, "01-textbook");
        var model = Panel();

        // **FROM LINE 3, WHERE THE CARD OPENS**, to the end: line 4 is the operator's own report and carries
        // FN00, which is his grid and never W1AW's.
        for (var lines = 3; lines <= textbook.Lines.Count; lines++)
        {
            model.ShowPsk31ChannelsForTests(new[] { Channel(1, 1000, textbook, lines) });

            var card = Assert.Single(model.DigitalCards);

            _output.WriteLine("after line " + lines + ": " + card.Callsign + " grid [" + card.Facts.Grid + "] place ["
                + card.Place + "] map row [" + card.Globe.Caption + "]");

            Assert.Equal("W1AW", card.Callsign);
            Assert.Equal("FN31", card.Facts.Grid);
            Assert.DoesNotContain(NoGrid, card.Globe.Caption, StringComparison.Ordinal);
            Assert.Contains("FN31", card.Globe.Caption, StringComparison.Ordinal);
        }
    }

    /// <summary>**A grid from a guessed message is not handed to the card.**</summary>
    [Fact]
    public void AGridFromAGuessedMessageIsNotHanded()
    {
        var corpus = Psk31Corpus.Load();

        const string certain = "KC3QIS de W1AW GA TNX FER CALL UR RST 599 599 HW? KC3QIS de W1AW K\n";
        const string guessed = "KC3QIS de W1AW R R 5#9 GRID FN31 FN31 KC3QIS de W1AW K\n";

        // **NOT VACUOUS**: the parse of the second message is a guess, and it does carry FN31.
        var splitter = new Psk31MessageSplitter(corpus.Operator);
        var messages = new List<Psk31Message>();

        foreach (var character in certain + guessed)
        {
            if (splitter.Add(character) is { } message)
            {
                messages.Add(message);
            }
        }

        foreach (var message in messages)
        {
            _output.WriteLine("parse: " + message.Exchange);
        }

        Assert.Equal(2, messages.Count);
        Assert.True(messages[0].Exchange.IsCertain);
        Assert.Null(messages[0].Exchange.Grid);
        Assert.False(messages[1].Exchange.IsCertain);
        Assert.Equal("FN31", messages[1].Exchange.Grid);

        var model = Panel();

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(1, 1000, 10.0, certain) });

        var before = Assert.Single(model.DigitalCards);

        _output.WriteLine("after the certain message: grid [" + before.Facts.Grid + "] word [" + before.TurnWord
            + "] map row [" + before.Globe.Caption + "]");

        Assert.Null(before.Facts.Grid);
        Assert.Contains(NoGrid, before.Globe.Caption, StringComparison.Ordinal);

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(1, 1000, 10.0, certain + guessed) });

        var after = Assert.Single(model.DigitalCards);

        _output.WriteLine("after the guessed message: grid [" + after.Facts.Grid + "] word [" + after.TurnWord
            + "] map row [" + after.Globe.Caption + "]");

        Assert.Equal("W1AW", after.Callsign);
        Assert.Null(after.Facts.Grid);
        Assert.Contains(NoGrid, after.Globe.Caption, StringComparison.Ordinal);
    }

    private static Psk31CorpusTranscript Transcript(Psk31Corpus corpus, string name)
        => corpus.Transcripts.Single(t => t.Name == name);

    private static Psk31Channel Channel(int id, double hz, Psk31CorpusTranscript transcript, int lines)
        => new(id, hz, 10.0, string.Concat(transcript.Lines.Take(lines).Select(l => l.Text + "\n")));

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
