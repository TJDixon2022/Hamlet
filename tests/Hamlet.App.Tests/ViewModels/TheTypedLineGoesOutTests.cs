using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Tests.Psk31;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 357 task 2: **one click sends the line he typed, framed.**
/// </summary>
/// <remarks>
/// <para>**R29, TIM 2026-09-14**, after reading a whole QSO between two strangers off 14.070
/// with nothing to send back but another CQ: *"I want to add the ability to send text I type.
/// That is the thing PSK31 offers over FT8/4."* It overrules PSK31 plan R2's no-keyboard line
/// and nothing else of it.</para>
/// <para>**WHAT HAMLET ADDS IS THE FRAME AND ONLY THE FRAME**: his callsign, the other
/// station's, and the hand-back. Those are the two things a beginner forgets on a keyboard
/// mode, and they are not his to remember.</para>
/// <para>**ONE CLICK, ONE TRANSMISSION, THROUGH THE ONE SEQUENCE** (§0.2, §R10). This asserts
/// the sink was called once and that what was played is the framed text; there is no second
/// keying path to assert about, and `TheUnslottedSendTests` and `TheStopIsAlwaysOnScreenTests`
/// are unedited.</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio: a fake port and
/// a fake sink on a machine with none (FACT-004, FACT-006).</para>
/// </remarks>
public sealed class TheTypedLineGoesOutTests : IDisposable
{
    private const long On20m = 14_070_000;
    private const double HisHz = 1234;

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where every composed line is printed whole.</param>
    public TheTypedLineGoesOutTests(ITestOutputHelper output)
    {
        _output = output;

        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-typed-" + Guid.NewGuid().ToString("N"));

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

    /// <summary>**A typed line composes the framed text exactly, on one click, and the block clears.**</summary>
    [Fact]
    public void ATypedLineComposesTheFramedTextExactly()
    {
        var (model, sink) = Panel();

        var card = CardFor(model);

        Assert.True(card.CanType);

        card.TypedText = "  Nice signal here in Trafford, running 40 watts  ";

        model.SendTypedPsk31Command.Execute(card);
        Settle(model);

        var want =
            "EI4GNB de KC3QIS  Nice signal here in Trafford, running 40 watts  "
            + "BTU EI4GNB de KC3QIS K";

        _output.WriteLine("send line: " + model.DigitalSendLine);
        _output.WriteLine("framed   : " + want);

        // **ONE CLICK, ONE TRANSMISSION** (§0.2).
        Assert.Equal(1, sink.TimesCalled);

        // **HIS WORDS, TRIMMED, WITH THE TWO FRAMES AND NOTHING ELSE.**
        Assert.Contains(want, model.DigitalSendLine, StringComparison.Ordinal);

        // **AND THE BLOCK CLEARS**, because it went out.
        Assert.Equal("", card.TypedText);
    }

    /// <summary>**A line that is only whitespace sends nothing at all.**</summary>
    [Fact]
    public void ALineThatIsOnlyWhitespaceSendsNothing()
    {
        var (model, sink) = Panel();

        var card = CardFor(model);

        card.TypedText = "     ";

        model.SendTypedPsk31Command.Execute(card);
        Settle(model);

        _output.WriteLine("note: " + card.TypedNote);

        Assert.Equal(0, sink.TimesCalled);
        Assert.False(model.HasSomethingToStop);
        Assert.Equal("Type something first.", card.TypedNote);

        // **AND WHAT HE TYPED IS STILL THERE**, because nothing happened to it.
        Assert.Equal("     ", card.TypedText);
    }

    /// <summary>**A character PSK31 cannot send as itself is dropped, and the card says how many.**</summary>
    /// <remarks>
    /// **DROPPED, NOT REFUSED.** The varicode carries the 256 Latin-1 bytes and nothing else,
    /// so a curly quote out of a paste would go on the air as something other than itself
    /// (§0.0). Refusing the whole line over one pasted character would leave him guessing
    /// which one.
    /// </remarks>
    [Fact]
    public void ACharacterOutsideTheTableIsDroppedAndTheCardSaysSo()
    {
        var (model, sink) = Panel();

        var card = CardFor(model);

        // A curly apostrophe and an em dash, neither of them in Latin-1's low half, and a
        // tab: three characters PSK31 cannot send as themselves.
        card.TypedText = "That’s a fine signal—very readable\there";

        model.SendTypedPsk31Command.Execute(card);
        Settle(model);

        _output.WriteLine("send line: " + model.DigitalSendLine);
        _output.WriteLine("note     : " + card.TypedNote);

        Assert.Equal(1, sink.TimesCalled);

        // **THE THREE ARE GONE AND EVERYTHING ELSE WENT.**
        Assert.Contains(
            "EI4GNB de KC3QIS  Thats a fine signalvery readablehere  BTU EI4GNB de KC3QIS K",
            model.DigitalSendLine,
            StringComparison.Ordinal);

        Assert.Contains("3 characters were dropped", card.TypedNote, StringComparison.Ordinal);
        Assert.Equal("", card.TypedText);
    }

    /// <summary>**The record says `typed`, carries a count, and holds none of his words.**</summary>
    [Fact]
    public void TheRecordSaysTypedAndHoldsNoneOfHisWords()
    {
        const string Secret = "meet me behind the barn at midnight";

        string[] lines;

        using (var telemetry = new JsonlTelemetry(_folder, "357", _ => true))
        {
            var (model, _) = Panel(telemetry);

            var card = CardFor(model);

            card.TypedText = Secret;

            model.SendTypedPsk31Command.Execute(card);
            Settle(model);
        }

        lines = Directory.GetFiles(_folder, "*.jsonl").SelectMany(File.ReadAllLines).ToArray();

        var composed = Assert.Single(Events(lines, "psk31_send_composed"));

        _output.WriteLine("composed: " + composed.GetRawText());

        Assert.Equal("typed", composed.GetProperty("macro").GetString());
        Assert.True(composed.GetProperty("characters").GetInt32() > Secret.Length);
        Assert.True(composed.GetProperty("seconds").GetDouble() > 0);

        // **NOT ONE WORD OF IT IS IN THE FILE** (HM-DEC-018, §2.1), and neither is the frame.
        var whole = string.Join("\n", lines);

        Assert.DoesNotContain("midnight", whole, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("barn", whole, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("EI4GNB", whole, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("KC3QIS", whole, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>**The send is one unslotted transmission, the same shape the macros make.**</summary>
    /// <remarks>
    /// **NO SECOND PATH** (§10). What is asserted is that the audio handed to the sink is an
    /// `UnslottedTransmission` in PSK31 mode, which is what `Psk31Modulator.Compose` makes
    /// for every macro; the keying, the abort and the cap are the sequence's and are proved
    /// by `TheUnslottedSendTests`, which this unit did not edit.
    /// </remarks>
    [Fact]
    public void TheSendIsOneUnslottedPsk31Transmission()
    {
        var (model, sink) = Panel();

        var card = CardFor(model);

        card.TypedText = "hello";

        model.SendTypedPsk31Command.Execute(card);
        Settle(model);

        _output.WriteLine(
            "the sink was called " + sink.TimesCalled + " time(s), "
            + sink.PlayedSoFar.ToString(CultureInfo.InvariantCulture) + " samples");

        Assert.Equal(1, sink.TimesCalled);
        Assert.True(sink.PlayedSoFar > 0, "nothing was played");
    }

    /// <summary>**It is offered where the turn is unknown, and not where it is certainly his.**</summary>
    [Fact]
    public void ItIsOfferedWhereTheTurnIsUnknownAndNotWhereItIsCertainlyHis()
    {
        var unknown = new Ft8ContactCardProbe(Psk31TurnState.Unknown, certain: false);
        var his = new Ft8ContactCardProbe(Psk31TurnState.HisTurn, certain: true);
        var guessHis = new Ft8ContactCardProbe(Psk31TurnState.HisTurn, certain: false);
        var mine = new Ft8ContactCardProbe(Psk31TurnState.YourTurn, certain: true);

        foreach (var probe in new[] { unknown, his, guessHis, mine })
        {
            _output.WriteLine(
                probe.Card.TurnWord.PadRight(22) + " can type " + probe.Card.CanType);
        }

        Assert.True(unknown.Card.CanType);
        Assert.True(mine.Card.CanType);

        // **A GUESS THAT IT IS HIS TURN IS NOT CERTAINTY**, so the block is still there.
        Assert.True(guessHis.Card.CanType);

        // **AND HAMLET WILL NOT INVITE HIM TO TALK OVER A STATION IT IS SURE IS SENDING.**
        Assert.False(his.Card.CanType);
    }

    /// <summary>One card built straight from a turn, for the offer question alone.</summary>
    private sealed class Ft8ContactCardProbe
    {
        public Ft8ContactCardProbe(Psk31TurnState state, bool certain)
            => Card = Ft8ContactCard.ForPsk31(
                "EI4GNB", new Psk31TurnReading(state, certain), "FN00DJ");

        public Ft8ContactCard Card { get; }
    }

    /// <summary>The station's card, opened the way a right-click opens it.</summary>
    private static Ft8ContactCard CardFor(MainWindowViewModel model)
    {
        var corpus = Psk31Corpus.Load();

        var his = string.Concat(corpus.Transcripts
            .Single(t => t.Name == "04-not-for-me")
            .Lines
            .Where(l => !string.Equals(l.Frm, corpus.Operator, StringComparison.OrdinalIgnoreCase))
            .Take(2)
            .Select(l => l.Text + "\n"));

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(1, HisHz, 10.0, his) });

        var row = model.DigitalDecodes.Single();

        model.OpenPsk31CardCommand.Execute(row);

        return model.DigitalCards.Single(c => c.Callsign == "EI4GNB");
    }

    private static IReadOnlyList<JsonElement> Events(string[] lines, string name)
        => lines
            .Select(line => JsonDocument.Parse(line).RootElement)
            .Where(e => e.GetProperty("event").GetString() == name)
            .Select(e => e.GetProperty("data"))
            .ToList();

    /// <summary>Lets a no-slot send finish; the click fires it and does not wait (§R10).</summary>
    private static void Settle(MainWindowViewModel model)
    {
        for (var tries = 0; tries < 400 && model.HasSomethingToStop; tries++)
        {
            System.Threading.Thread.Sleep(10);
        }

        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
    }

    private (MainWindowViewModel Model, FakeSink Sink) Panel(JsonlTelemetry? telemetry = null)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = "KC3QIS";
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
