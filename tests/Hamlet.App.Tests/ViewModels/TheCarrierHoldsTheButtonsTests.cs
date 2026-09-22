using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 385 task 2, criterion 9.2: **Hamlet does not key on top of a man who is still
/// sending, and the hold lets go the moment he stops.**
/// </summary>
/// <remarks>
/// <para>**R44, Tim, 2026-09-21, from his own contact.** He answered a station and Hamlet composed
/// and sent while that station's carrier was still on the air. **The fact is the carrier and not
/// the parser** (ruling 1 item 1): `row.Ended` is written in one place and answers at every
/// moment, where `Psk31TurnState.HeIsSending` means characters are still arriving after the last
/// complete message and was never once true while his carrier was up (`Unit385Trace`).</para>
/// <para>**THE HOLD IS A READING AND NEVER A LATCH** (ruling 1 item 4). A gate that stuck would
/// leave Hamlet unable to transmit at all, which is worse than the fault it fixes, so the release
/// and the empty band are each proved here in a test of their own.</para>
/// <para>**IT REFUSES; IT NEVER CAUSES** (§0.2). Nothing here starts, delays or alters a send, and
/// every count is what the fake sound card was handed. Nothing is evidence about the radio
/// (FACT-004).</para>
/// </remarks>
public sealed class TheCarrierHoldsTheButtonsTests : IDisposable
{
    private const string Mine = "KC3QIS";
    private const string Him = "W1ABC";

    /// <summary>His first over, addressed to the operator and handing the turn back.</summary>
    private const string HisOver = Mine + " de " + Him + " GM TNX CALL UR 599 599 BTU " + Mine + " de " + Him + " K\n";

    /// <summary>
    /// **And then he starts sending again**: his carrier is up and this over is not finished, so
    /// he has handed nothing back. This is the moment R44 names.
    /// </summary>
    private const string MidOver = HisOver + Mine + " de " + Him + " R R NAME BOB QTH ERIE AND THE RIG HERE IS";

    private readonly ITestOutputHelper _output;
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "hamlet-hold-" + Guid.NewGuid().ToString("N"));

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the refusal and the counts are printed.</param>
    public TheCarrierHoldsTheButtonsTests(ITestOutputHelper output)
    {
        _output = output;
        Directory.CreateDirectory(_folder);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, true);
        }
        catch (IOException)
        {
        }
    }

    /// <summary>**A press while his carrier is up is refused, and nothing is composed.**</summary>
    [Fact]
    public void APressWhileHisCarrierIsUpIsRefusedAndNothingIsComposed()
    {
        var model = Panel(out var radio, out var telemetry);

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, MidOver) });

        var card = Assert.Single(model.DigitalCards);
        var row = Assert.Single(model.DigitalDecodes, r => r.IsTextOnly);

        // **HE IS MID-OVER** - his carrier is up and he has handed nothing back.
        Assert.False(row.Ended);
        Assert.True(model.HisCarrierIsLive(Him));

        // **THE CONTROL THAT WAS LIVE HERE IS THE TYPED LINE.** The offered macro is already
        // withheld mid-over by §R1's own certainty gate, which is why the typed line and the
        // canned lines are what this criterion had to reach.
        Assert.Equal(Psk31Macro.None, card.Offered);
        Assert.True(card.CanType);

        card.TypedText = "HELLO OM";
        model.SendTypedPsk31Command.Execute(card);
        Settle(model);

        _output.WriteLine("screen: " + model.DigitalSendLine);

        // **NOTHING REACHED THE SOUND CARD** (§0.2).
        Assert.Equal(0, radio.Sink.TimesCalled);
        Assert.True(radio.Port.WasNeverWrittenTo);

        // **AND THE SCREEN SAYS THE ONE SENTENCE.**
        Assert.Contains(MainWindowViewModel.HeIsStillSending, model.DigitalSendLine, StringComparison.Ordinal);

        telemetry.Dispose();

        // **AND THE RECORD SAYS WHY, THROUGH THE WRITERS THAT ALREADY EXIST** (R13).
        var refused = Assert.Single(Events("psk31_send_refused"));

        Assert.Equal(MainWindowViewModel.HisCarrierLiveReason, refused.GetProperty("reason").GetString());
        Assert.Equal("gate", refused.GetProperty("stage").GetString());

        var action = Assert.Single(
            Events("operator_action"),
            e => e.GetProperty("action").GetString() == "send_refused");

        Assert.Equal(MainWindowViewModel.HisCarrierLiveReason, action.GetProperty("detail").GetString());
        NothingPersonal();
    }

    /// <summary>
    /// **The hold lets go: his carrier drops and the same press goes out** (ruling 1 item 4).
    /// </summary>
    [Fact]
    public void WhenHisCarrierDropsTheSamePressGoesOut()
    {
        var model = Panel(out var radio, out var telemetry);

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, MidOver) });

        var card = Assert.Single(model.DigitalCards);

        card.TypedText = "HELLO OM";
        model.SendTypedPsk31Command.Execute(card);
        Settle(model);

        Assert.Equal(0, radio.Sink.TimesCalled);

        // **HIS CARRIER GOES.**
        model.ShowPsk31ChannelsForTests(Array.Empty<Psk31Channel>());

        var row = Assert.Single(model.DigitalDecodes, r => r.IsTextOnly);

        Assert.True(row.Ended);
        Assert.False(model.HisCarrierIsLive(Him));

        var after = Assert.Single(model.DigitalCards);

        after.TypedText = "HELLO OM";
        model.SendTypedPsk31Command.Execute(after);
        Settle(model);

        _output.WriteLine($"after his carrier dropped: keyings {radio.Sink.TimesCalled}, line [{model.DigitalSendLine}]");

        // **THE HOLD WAS A READING, NOT A LATCH.**
        Assert.Equal(1, radio.Sink.TimesCalled);

        telemetry.Dispose();

        // **EXACTLY ONE REFUSAL WAS WRITTEN**, for the press that was held and not for the one
        // that went.
        Assert.Single(Events("psk31_send_refused"));
    }

    /// <summary>
    /// **A press with nobody on the frequency is not refused**, so the guard cannot silence him on
    /// an empty band (ruling 1 item 4).
    /// </summary>
    [Fact]
    public void APressOnAnEmptyBandIsNotRefused()
    {
        var model = Panel(out var radio, out var telemetry);

        Assert.False(model.HisCarrierIsLive(Him));
        Assert.False(model.HisCarrierIsLive(""));
        Assert.False(model.HisCarrierIsLive(null));

        model.SendCallToAnyoneCommand.Execute(null);
        Settle(model);

        _output.WriteLine($"a CQ on an empty band: keyings {radio.Sink.TimesCalled}, line [{model.DigitalSendLine}]");

        Assert.Equal(1, radio.Sink.TimesCalled);

        telemetry.Dispose();

        Assert.Empty(Events("psk31_send_refused"));
    }

    /// <summary>
    /// **A call to anybody while somebody else's carrier is up is not held**: it is addressed to
    /// nobody, so there is no man to key on top of.
    /// </summary>
    [Fact]
    public void ACallToAnybodyIsNotHeldByAnotherStationsCarrier()
    {
        var model = Panel(out var radio, out var telemetry);

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, MidOver) });

        Assert.True(model.HisCarrierIsLive(Him));

        model.SendCallToAnyoneCommand.Execute(null);
        Settle(model);

        _output.WriteLine($"a CQ while he is sending: keyings {radio.Sink.TimesCalled}, line [{model.DigitalSendLine}]");

        Assert.Equal(1, radio.Sink.TimesCalled);

        telemetry.Dispose();

        Assert.Empty(Events("psk31_send_refused"));
    }

    /// <summary>
    /// **A hand-back releases the hold while his carrier is still up** - the measurement that
    /// corrected ruling 1 item 1.
    /// </summary>
    /// <remarks>
    /// **A ROW OUTLIVES THE OVER BY A LONG WAY.** `Psk31Listener.RetiredWithinSeconds` is 9 s, and
    /// an Olivia channel is retired 56 characters after its last block - 22.94 s at 32/1000 up to
    /// 38.23 s at 8/250. Holding on the carrier alone would have refused every answer for that
    /// long after the other man said `K`, which is the gate that sticks ruling 1 item 4 forbids.
    /// **So the hold ends at the hand-back**, which is the moment the operator is meant to answer.
    /// </remarks>
    [Fact]
    public void AHandBackReleasesTheHoldWhileHisCarrierIsStillUp()
    {
        var model = Panel(out var radio, out var telemetry);

        // Mid-over: held.
        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, MidOver) });

        Assert.True(model.HisCarrierIsLive(Him));

        // He finishes the over and hands back - **and his carrier is still up**, because a row
        // lives on for seconds after the man has stopped.
        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, MidOver + " BTU " + Mine + " de " + Him + " K\n") });

        var row = Assert.Single(model.DigitalDecodes, r => r.IsTextOnly);
        var card = Assert.Single(model.DigitalCards);

        _output.WriteLine($"after his hand-back: row ended {row.Ended}, held {model.HisCarrierIsLive(Him)}, "
            + $"press {card.CanPressAction}");

        Assert.False(row.Ended, "the fixture no longer has his carrier up, so this proves nothing");
        Assert.False(model.HisCarrierIsLive(Him));
        Assert.True(card.CanPressAction);

        model.CardActionCommand.Execute(card);
        Settle(model);

        Assert.Equal(1, radio.Sink.TimesCalled);

        telemetry.Dispose();

        Assert.Empty(Events("psk31_send_refused"));
    }

    /// <summary>
    /// **The replay: the press Tim made at 17:45:40 would have been held** - a station whose
    /// carrier is up, an answer offered, and the send refused at the gate above the composer.
    /// </summary>
    /// <remarks>
    /// **THE TIMINGS ARE R44's AND THE CALLSIGN IS A TEST CALLSIGN.** The owner's record of
    /// 2026-09-21 is not on this machine (FACT-004, `Unit385Trace` item 1), so this is a
    /// constructed fixture in the shape R44 states and is never described as his record.
    /// </remarks>
    [Fact]
    public void TheReportHeSentOnTopOfTheCarrierWouldNowBeHeld()
    {
        var model = Panel(out var radio, out var telemetry);

        // 17:44 - his carrier appears, he calls the operator, and he is sending again.
        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1726, 10.0, MidOver) });

        var card = Assert.Single(model.DigitalCards);

        // 17:45:40 - the operator sends while that over is still running.
        card.TypedText = "R R TNX FER RPRT";
        model.SendTypedPsk31Command.Execute(card);
        Settle(model);

        _output.WriteLine("what he would see now: " + model.DigitalSendLine);

        Assert.Equal(0, radio.Sink.TimesCalled);

        telemetry.Dispose();

        var refused = Assert.Single(Events("psk31_send_refused"));

        Assert.Equal(MainWindowViewModel.HisCarrierLiveReason, refused.GetProperty("reason").GetString());
        NothingPersonal();
    }

    /// <summary>
    /// **His row and his card say he is sending, and the four controls are held with the one
    /// sentence** - ruling 1 item 2 (a) and (b), and item 3.
    /// </summary>
    [Fact]
    public void HisRowAndHisCardSayItAndTheFourControlsAreHeld()
    {
        var model = Panel(out _, out var telemetry);

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, MidOver) });

        var card = Assert.Single(model.DigitalCards);
        var row = Assert.Single(model.DigitalDecodes, r => r.IsTextOnly);

        _output.WriteLine($"row  : sending [{row.SendingWord}] ended [{row.EndedWord}]");
        _output.WriteLine($"card : live {card.CarrierIsLive}, word [{card.SendingWord}], note [{card.OfferNote}], "
            + $"press {card.CanPressAction}, typed send {card.CanSendTyped}, typed note [{card.TypedHoldNote}]");

        // (a) **HIS ROW AND HIS CARD SAY IT.**
        Assert.Equal("sending", row.SendingWord);
        Assert.True(row.HasSendingWord);
        Assert.Equal("", row.EndedWord);
        Assert.True(card.CarrierIsLive);
        Assert.Equal(Ft8ContactCard.HeIsStillSending, card.SendingWord);

        // (b) **THE FOUR CONTROLS ARE HELD.** Report and Confirm are the one offered button, and
        // mid-over §R1's certainty gate has already withheld it - so what this unit had to hold is
        // the typed line and the canned lines, and the button is held too wherever one is offered.
        Assert.Equal(Psk31Macro.None, card.Offered);
        Assert.False(card.CanPressAction);
        Assert.Equal(Ft8ContactCard.HeIsStillSending, card.OfferNote);
        Assert.True(card.CanType, "the typed block is hidden rather than held");
        Assert.False(card.CanSendTyped);
        Assert.Equal(Ft8ContactCard.HeIsStillSending, card.TypedHoldNote);

        var canned = model.Psk31CannedMenuFor(row);

        Assert.NotNull(canned);
        _output.WriteLine($"canned: {canned!.Count} entries, first [{canned[0].Label}]");
        Assert.All(canned, entry => Assert.True(entry.IsNote));
        Assert.Contains(Ft8ContactCard.HeIsStillSending, canned[0].Label, StringComparison.Ordinal);

        // **AND ALL OF IT LETS GO WHEN HIS CARRIER DROPS** (ruling 1 item 4).
        model.ShowPsk31ChannelsForTests(Array.Empty<Psk31Channel>());

        var after = Assert.Single(model.DigitalCards);
        var ended = Assert.Single(model.DigitalDecodes, r => r.IsTextOnly);

        _output.WriteLine($"after : row sending [{ended.SendingWord}] ended [{ended.EndedWord}], "
            + $"card live {after.CarrierIsLive}, press {after.CanPressAction}, typed send {after.CanSendTyped}");

        Assert.Equal("", ended.SendingWord);
        Assert.Equal("ended", ended.EndedWord);
        Assert.False(after.CarrierIsLive);
        Assert.True(after.CanSendTyped);
        Assert.Equal("", after.TypedHoldNote);
        // **THE LINES COME BACK.** Two of the seven stay notes for reasons of their own - an
        // Answer wants a certain CQ and a Confirm wants his turn - so what the release means here
        // is that the menu is the seven again, that lines are pressable, and that not one note
        // still blames his carrier.
        var back = model.Psk31CannedMenuFor(ended)!;

        _output.WriteLine($"canned after: {back.Count} entries, {back.Count(e => !e.IsNote)} pressable");

        Assert.Equal(7, back.Count);
        Assert.Contains(back, entry => !entry.IsNote);
        Assert.DoesNotContain(back, entry => entry.Label.Contains(Ft8ContactCard.HeIsStillSending, StringComparison.Ordinal));

        telemetry.Dispose();
    }

    /// <summary>
    /// **His card carries a color and the word, drawn on the window** - 9.2's first clause for the
    /// card, measured on the drawn control and not only on the view model (work instruction 389
    /// task 2).
    /// </summary>
    /// <remarks>
    /// **THE CARD'S WORD WAS BOUND NOWHERE BEFORE WORK INSTRUCTION 389**: `SendingWord` existed on
    /// the card and the card said the sentence only as the grey notes beside the held controls.
    /// The color is the row's own green, looked up by the resource key both are drawn with.
    /// </remarks>
    [AvaloniaFact]
    public void HisCardIsDrawnInTheSendingGreenWithTheWord()
    {
        var model = Panel(out _, out var telemetry);

        // Arranged before the window exists, as TheTopRowTests.Realized arranges them: both
        // panels open, so the card is realized to be read.
        model.DigitalDecodedExpanded = true;
        model.DigitalMineExpanded = true;
        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, MidOver) });

        var window = new MainWindow { DataContext = model, Width = 1920, Height = 1040 };

        try
        {
            window.Show();

            for (var i = 0; i < 5; i++)
            {
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
            }

            Assert.True(window.TryFindResource("HmGreenBrush", out var green), "HmGreenBrush is not a resource");

            var drawn = window.GetVisualDescendants().OfType<TextBlock>().ToList();
            var cardWord = Assert.Single(drawn, t => t.Name == "CardSendingWord");

            _output.WriteLine($"card: [{cardWord.Text}] visible {cardWord.IsEffectivelyVisible} {cardWord.Bounds.Width:0.#} px, "
                + $"brush {cardWord.Foreground}");

            Assert.True(cardWord.IsEffectivelyVisible);
            Assert.True(cardWord.Bounds.Width > 0);
            Assert.Equal(Ft8ContactCard.HeIsStillSending, cardWord.Text);
            Assert.Same(green, cardWord.Foreground);

            // **THE ROW'S WORD IS UNIT 385's** and is asserted on the view model at
            // HisRowAndHisCardSayItAndTheFourControlsAreHeld; its green is the same resource
            // (MainWindow.axaml, DecodedRowSendingWord). This window does not realize the decoded
            // rows, so it is not read here.

            // **AND IT GOES WHEN HIS CARRIER DOES.**
            model.ShowPsk31ChannelsForTests(Array.Empty<Psk31Channel>());

            for (var i = 0; i < 5; i++)
            {
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
            }

            Assert.DoesNotContain(
                window.GetVisualDescendants().OfType<TextBlock>(),
                t => t.Name == "CardSendingWord" && t.IsEffectivelyVisible);
        }
        finally
        {
            window.Close();
            telemetry.Dispose();
        }
    }

    private List<JsonElement> Events(string name)
        => Directory.GetFiles(_folder, "*.jsonl")
            .SelectMany(File.ReadAllLines)
            .Select(line => JsonDocument.Parse(line).RootElement)
            .Where(e => e.GetProperty("event").GetString() == name)
            .Select(e => e.GetProperty("data").Clone())
            .ToList();

    /// <summary>**No callsign and no word of his text in any line** (HM-DEC-018 §2.1).</summary>
    private void NothingPersonal()
    {
        foreach (var line in Directory.GetFiles(_folder, "*.jsonl").SelectMany(File.ReadAllLines))
        {
            Assert.DoesNotContain(Him, line, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Mine, line, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static void Settle(MainWindowViewModel model)
    {
        for (var tries = 0; tries < 200 && model.HasSomethingToStop; tries++)
        {
            System.Threading.Thread.Sleep(10);
        }
    }

    private MainWindowViewModel Panel(out Radio radio, out JsonlTelemetry telemetry)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN00";
        settings.Operator.OperatorName = "Tester";
        settings.Operator.Location = "Erie PA";
        settings.Operator.LicenseClass = LicenseClass.General;

        telemetry = new JsonlTelemetry(_folder, "385", _ => true);

        var model = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.ChooseDigitalModeCommand.Execute("PSK31");
        model.SelectedBand = model.Bands.First(b => b.Band.LowHz <= 14_070_000 && b.Band.HighHz >= 14_070_000);
        model.FrequencyHz = 14_070_000;

        var port = new FakePort();
        var sink = new FakeSink();

        model.UseRigPortForTests(port);
        model.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(port, sink, guard: null, telemetry)));

        radio = new Radio(port, sink);

        return model;
    }

    private sealed record Radio(FakePort Port, FakeSink Sink);
}
