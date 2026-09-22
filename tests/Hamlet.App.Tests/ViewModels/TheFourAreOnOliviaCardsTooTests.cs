using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 385 task 5, criterion 9.5: **the four are on PSK31 and Olivia cards alike.**
/// </summary>
/// <remarks>
/// <para>**BY IDENTITY WHERE THEY ARE THE SAME OBJECT** (unit 378's 7.4 is the precedent, and R14).
/// An Olivia row is a `DigitalDecodeRow` with the same `Ended`, an Olivia card is an
/// `Ft8ContactCard` built by the same `ForPsk31`, and both modes leave by the same
/// `SendMessage` - so the hold, the Log link and the X are one implementation and not two.
/// **What is driven down an Olivia channel here is the hold and the Log**; the turn is driven in
/// `TheTurnMovesOnEveryHandBackTests`, and the X is the same one control on the same type.</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004).</para>
/// </remarks>
public sealed class TheFourAreOnOliviaCardsTooTests : IDisposable
{
    private const string Mine = "KC3QIS";
    private const string Him = "W1ABC";

    private const string HisOver = Mine + " de " + Him + " GM TNX CALL UR 599 599 BTU " + Mine + " de " + Him + " K\n";
    private const string MidOver = HisOver + Mine + " de " + Him + " R R NAME BOB QTH ERIE AND THE RIG HERE IS";

    private readonly ITestOutputHelper _output;
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "hamlet-olivia9-" + Guid.NewGuid().ToString("N"));

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public TheFourAreOnOliviaCardsTooTests(ITestOutputHelper output)
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

    /// <summary>**9.2 on Olivia: a station mid-over holds the send, and the hold lets go.**</summary>
    [Fact]
    public void AnOliviaStationMidOverHoldsTheSendAndLetsGo()
    {
        var model = Panel(out var radio, out var telemetry);

        model.ChooseDigitalModeCommand.Execute("Olivia");
        model.ShowOliviaChannelsForTests(new[] { Channel(MidOver, ended: false) });

        var card = Assert.Single(model.DigitalCards);
        var row = Assert.Single(model.DigitalDecodes, r => r.IsTextOnly);

        _output.WriteLine($"olivia mid-over: row ended {row.Ended}, sending [{row.SendingWord}], "
            + $"card live {card.CarrierIsLive}, typed send {card.CanSendTyped}, note [{card.OfferNote}]");

        Assert.True(model.HisCarrierIsLive(Him));
        Assert.Equal("sending", row.SendingWord);
        Assert.True(card.CarrierIsLive);
        Assert.False(card.CanSendTyped);

        // **AND THE REST OF 9.2's HOLD, DRIVEN ON OLIVIA RATHER THAN CLAIMED BY IDENTITY** (work
        // instruction 389): the card's word, the offered button, and the canned lines as one note.
        Assert.Equal(Ft8ContactCard.HeIsStillSending, card.SendingWord);
        Assert.False(card.CanPressAction);
        Assert.Equal(Ft8ContactCard.HeIsStillSending, card.OfferNote);
        Assert.Equal(Ft8ContactCard.HeIsStillSending, card.TypedHoldNote);

        var canned = model.Psk31CannedMenuFor(row);

        Assert.NotNull(canned);
        Assert.All(canned!, entry => Assert.True(entry.IsNote));
        Assert.Contains(Ft8ContactCard.HeIsStillSending, canned![0].Label, StringComparison.Ordinal);

        card.TypedText = "HELLO OM";
        model.SendTypedPsk31Command.Execute(card);
        Settle(model);

        Assert.Equal(0, radio.Sink.TimesCalled);

        // **AND IT LETS GO** when he hands back, with his carrier still up.
        model.ShowOliviaChannelsForTests(new[] { Channel(MidOver + " BTU " + Mine + " de " + Him + " K\n", ended: false) });

        var after = Assert.Single(model.DigitalCards);

        _output.WriteLine($"after his hand-back: live {after.CarrierIsLive}, typed send {after.CanSendTyped}");

        Assert.False(model.HisCarrierIsLive(Him));
        Assert.True(after.CanSendTyped);

        after.TypedText = "HELLO OM";
        model.SendTypedPsk31Command.Execute(after);
        Settle(model);

        Assert.Equal(1, radio.Sink.TimesCalled);

        telemetry.Dispose();

        var refused = Assert.Single(Events("psk31_send_refused"));

        Assert.Equal(MainWindowViewModel.HisCarrierLiveReason, refused.GetProperty("reason").GetString());
    }

    /// <summary>**9.3 and 9.1 on Olivia: Log from the start, and the X takes the card away.**</summary>
    [Fact]
    public void AnOliviaCardOffersLogFromTheStartAndTheXTakesItAway()
    {
        var model = Panel(out _, out var telemetry);

        model.ChooseDigitalModeCommand.Execute("Olivia");
        model.ShowOliviaChannelsForTests(new[] { Channel(HisOver, ended: false) });

        var card = Assert.Single(model.DigitalCards);

        _output.WriteLine($"olivia card {card.Callsign}: ShowsLogLink {card.ShowsLogLink}, "
            + $"variant [{card.OliviaVariant}]");

        Assert.True(card.ShowsLogLink);
        Assert.False(card.IsCallToAnyone);

        model.ClearCardCommand.Execute(card.Callsign);

        Assert.Empty(model.DigitalCards);
        Assert.Contains(model.DigitalDecodes, r => r.IsTextOnly);

        telemetry.Dispose();

        // **AND THE PRESS IS WRITTEN IN 9.1's TOKEN ON OLIVIA TOO** (work instruction 389).
        Assert.Single(
            Events("operator_action"),
            e => e.GetProperty("action").GetString() == "card_dismissed");
    }

    /// <summary>
    /// **And the four are one implementation, not two**: the same card type, the same row type,
    /// the same gate.
    /// </summary>
    [Fact]
    public void TheCardTheRowAndTheGateAreTheSameOnesPsk31Uses()
    {
        var model = Panel(out _, out var telemetry);

        model.ChooseDigitalModeCommand.Execute("PSK31");
        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, HisOver) });

        var psk31Card = Assert.Single(model.DigitalCards);
        var psk31Row = Assert.Single(model.DigitalDecodes, r => r.IsTextOnly);

        var olivia = Panel(out _, out var second);

        olivia.ChooseDigitalModeCommand.Execute("Olivia");
        olivia.ShowOliviaChannelsForTests(new[] { Channel(HisOver, ended: false) });

        var oliviaCard = Assert.Single(olivia.DigitalCards);
        var oliviaRow = Assert.Single(olivia.DigitalDecodes, r => r.IsTextOnly);

        _output.WriteLine($"card types: {psk31Card.GetType().Name} and {oliviaCard.GetType().Name}");
        _output.WriteLine($"row types : {psk31Row.GetType().Name} and {oliviaRow.GetType().Name}");

        // **ONE TYPE EACH**, which is why the X, the Log link and the hold need no second copy.
        Assert.Same(psk31Card.GetType(), oliviaCard.GetType());
        Assert.Same(psk31Row.GetType(), oliviaRow.GetType());

        // **AND ONE READING OF WHO IS SENDING**, asked of the same method on both.
        Assert.False(model.HisCarrierIsLive(Him));
        Assert.False(olivia.HisCarrierIsLive(Him));

        // **AND THE SAME THREE PROPERTIES ANSWER ON BOTH.**
        Assert.Equal(psk31Card.ShowsLogLink, oliviaCard.ShowsLogLink);
        Assert.Equal(psk31Card.CanSendTyped, oliviaCard.CanSendTyped);
        Assert.Equal(psk31Row.SendingWord, oliviaRow.SendingWord);

        telemetry.Dispose();
        second.Dispose();
    }

    private static OliviaChannel Channel(string text, bool ended)
        => new(3, "8/250", 1000, OliviaListener.FoundByRsid, 0, text, text.Length > 0 ? 9 : 0, 0, ended, 1000);

    private List<JsonElement> Events(string name)
        => Directory.GetFiles(_folder, "*.jsonl")
            .SelectMany(File.ReadAllLines)
            .Select(line => JsonDocument.Parse(line).RootElement)
            .Where(e => e.GetProperty("event").GetString() == name)
            .Select(e => e.GetProperty("data").Clone())
            .ToList();

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
