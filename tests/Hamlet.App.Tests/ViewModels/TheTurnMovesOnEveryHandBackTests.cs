using System;
using System.Collections.Generic;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 385 task 4, criterion 9.4: **every hand-back moves the turn, not only the
/// first.**
/// </summary>
/// <remarks>
/// <para>**ASSERTED, NOT REBUILT** (R14). `Unit385Trace` measured the before: both of a station's
/// parsed hand-backs already move the card - to *Your turn* and then, after Hamlet has answered
/// and he comes back again, to *Your turn, a guess*. There is no second turn tracker here and
/// none was written; what this does is pin the behaviour so it cannot quietly stop being true.</para>
/// <para>**THE GUESSED WORD IS THE PLAN'S SINCE WORK INSTRUCTION 389**: *Your turn?*, where 9.4
/// says *the card reads* your turn?. The three assertions on it were rewritten under R12 from
/// *Your turn, a guess*, and the sentence under the word still says *guess*.</para>
/// <para>**MOVING THE CARD IS NOT OFFERING A SEND**, and that is the distinction the whole
/// criterion turns on. §R1's certainty gate is untouched: a guessed turn offers the Report that
/// unit 371 licensed and never the Confirm, which claims a contact. **Nothing here transmits.**</para>
/// <para>**AND THE CASE THE OWNER ACTUALLY HIT IS NOT THIS ONE.** A garbled over never becomes a
/// parsed line at all - `Psk31MessageSplitter` completes a message on a turnover word that follows
/// clean callsigns - so the card stays at *He is still sending*. That site is in the engine and is
/// reported rather than repaired (section 4).</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004).</para>
/// </remarks>
public sealed class TheTurnMovesOnEveryHandBackTests
{
    private const string Mine = "KC3QIS";
    private const string Him = "W1ABC";

    /// <summary>His first over, clean, handing the turn back.</summary>
    private const string FirstOver = Mine + " de " + Him + " GM TNX CALL UR 599 599 BTU " + Mine + " de " + Him + " K\n";

    /// <summary>His second over, handing back again and not reading cleanly.</summary>
    private const string SecondOver = Mine + " de " + Him + " R R NAME BOB 5#9 QTH ERIE BTU " + Mine + " de " + Him + " K\n";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each hand-back's reading is printed.</param>
    public TheTurnMovesOnEveryHandBackTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Both hand-backs move the card, and the second says it is a guess.**</summary>
    [Fact]
    public void EveryHandBackMovesTheTurnAndTheGuessedOneSaysSo()
    {
        var model = Panel();

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, FirstOver) });

        var first = Assert.Single(model.DigitalCards);

        _output.WriteLine($"his first hand-back : [{first.TurnWord}] guess {first.TurnIsGuess}, offered {first.Offered}");

        Assert.Equal("Your turn", first.TurnWord);
        Assert.False(first.TurnIsGuess);

        // **HAMLET ANSWERS**, which puts the turn on his side - without this the second hand-back
        // would be measured from a turn that never left the operator, which is not the sequence
        // R44 names.
        Assert.True(first.HasAction, "there is nothing to answer with, so this proves nothing");

        model.CardActionCommand.Execute(first);
        Settle(model);

        var handed = Assert.Single(model.DigitalCards);

        _output.WriteLine($"after Hamlet answered: [{handed.TurnWord}] guess {handed.TurnIsGuess}");

        Assert.Equal("His turn", handed.TurnWord);

        // **AND HIS SECOND HAND-BACK MOVES IT BACK**, marked as a guess because it did not read
        // cleanly - which is the criterion, and the word is the one the tree already had.
        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, FirstOver + SecondOver) });

        var second = Assert.Single(model.DigitalCards);

        _output.WriteLine($"his second hand-back: [{second.TurnWord}] guess {second.TurnIsGuess}, offered {second.Offered}");

        Assert.Equal("Your turn?",second.TurnWord);
        Assert.True(second.TurnIsGuess);
    }

    /// <summary>
    /// **Moving the card is not offering a send**: §R1's gate decides what may be sent, and it is
    /// untouched.
    /// </summary>
    /// <remarks>
    /// **THE TREE'S RULE, QUOTED RATHER THAN ASSUMED.** Work instruction 385 section 5 says a
    /// guessed turn offers nothing; since unit 371 a guessed hand-back to the operator offers the
    /// **Report** - which asserts nothing about the contact and goes out on his click with the
    /// doubt beside it - and never the **Confirm**, which claims one. This asserts what is there.
    /// </remarks>
    [Fact]
    public void AGuessedTurnOffersTheReportAndNeverTheConfirm()
    {
        var model = Panel();

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, FirstOver) });

        var card = Assert.Single(model.DigitalCards);

        model.CardActionCommand.Execute(card);
        Settle(model);

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, FirstOver + SecondOver) });

        var guessed = Assert.Single(model.DigitalCards);

        _output.WriteLine($"on the guess: turn [{guessed.TurnWord}], offered {guessed.Offered}, "
            + $"note [{guessed.OfferNote}]");

        // **A CONFIRM IS NEVER OFFERED ON A GUESS**: Hamlet has already sent its Report, so a
        // certain reading here would offer the Confirm, and the guess withholds exactly that.
        Assert.NotEqual(Psk31Macro.Confirm, guessed.Offered);
        Assert.True(guessed.TurnIsGuess);
    }

    /// <summary>
    /// **The replay: at his second hand-back the card reads what the tree's own wording says.**
    /// </summary>
    /// <remarks>
    /// **THE TIMINGS ARE R44's AND THE CALLSIGN IS THIS TEST'S.** The owner's record of 2026-09-21
    /// is not on this machine (FACT-004), so this is a constructed fixture in the shape R44 states.
    /// The card's word is quoted character for character rather than paraphrased. **AND IT IS NOT
    /// WHAT TIM GOT AT 17:48:33**: this over parses (`5#9` is a damaged report inside a clean
    /// frame), where unit 385's own section 4 item 2 measured that his was garbled and completed no
    /// message at all - work instruction 389 reports that, and the engine site stays unopened.
    /// Renamed by work instruction 389 from <c>...ReadsYourTurnAGuess</c>, because it no longer
    /// does.
    /// </remarks>
    [Fact]
    public void AtTheSecondHandBackTheCardReadsYourTurnQuestion()
    {
        var model = Panel();

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1726, 10.0, FirstOver) });

        var card = Assert.Single(model.DigitalCards);

        model.CardActionCommand.Execute(card);
        Settle(model);

        // 17:48:33 - he hands back a second time.
        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1726, 10.0, FirstOver + SecondOver) });

        var after = Assert.Single(model.DigitalCards);

        _output.WriteLine("the card reads: " + after.TurnWord);
        _output.WriteLine("the sentence  : " + after.Sentence);

        Assert.Equal("Your turn?",after.TurnWord);
        Assert.Contains("guess", after.Sentence, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>**And an Olivia station's hand-backs move his card the same way** (9.5).</summary>
    [Fact]
    public void AnOliviaStationsHandBacksMoveTheTurnToo()
    {
        var model = Panel();

        model.ChooseDigitalModeCommand.Execute("Olivia");
        model.ShowOliviaChannelsForTests(new[]
        {
            new OliviaChannel(3, "8/250", 1000, OliviaListener.FoundByRsid, 0, FirstOver, 9, 0, false, 1000),
        });

        var first = Assert.Single(model.DigitalCards);

        Assert.Equal("Your turn", first.TurnWord);

        model.CardActionCommand.Execute(first);
        Settle(model);

        model.ShowOliviaChannelsForTests(new[]
        {
            new OliviaChannel(3, "8/250", 1000, OliviaListener.FoundByRsid, 0, FirstOver + SecondOver, 18, 0, false, 1000),
        });

        var second = Assert.Single(model.DigitalCards);

        _output.WriteLine($"olivia: first [{first.TurnWord}] then [{second.TurnWord}] guess {second.TurnIsGuess}");

        Assert.Equal("Your turn?",second.TurnWord);
        Assert.True(second.TurnIsGuess);
    }

    private static void Settle(MainWindowViewModel model)
    {
        for (var tries = 0; tries < 200 && model.HasSomethingToStop; tries++)
        {
            System.Threading.Thread.Sleep(10);
        }
    }

    private static MainWindowViewModel Panel()
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN00";
        settings.Operator.OperatorName = "Tester";
        settings.Operator.Location = "Erie PA";
        settings.Operator.LicenseClass = LicenseClass.General;

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.ChooseDigitalModeCommand.Execute("PSK31");
        model.SelectedBand = model.Bands.First(b => b.Band.LowHz <= 14_070_000 && b.Band.HighHz >= 14_070_000);
        model.FrequencyHz = 14_070_000;

        var port = new FakePort();

        model.UseRigPortForTests(port);
        model.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(port, new FakeSink(), guard: null, null)));

        return model;
    }
}
