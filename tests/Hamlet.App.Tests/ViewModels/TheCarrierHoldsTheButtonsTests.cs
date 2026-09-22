using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
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

    /// <summary>His over, addressed to the operator and handing the turn back.</summary>
    private const string HisOver = Mine + " de " + Him + " GM TNX CALL UR 599 599 BTU " + Mine + " de " + Him + " K\n";

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

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, HisOver) });

        var card = Assert.Single(model.DigitalCards);
        var row = Assert.Single(model.DigitalDecodes, r => r.IsTextOnly);

        // **HIS CARRIER IS UP** - the fact the hold reads.
        Assert.False(row.Ended);
        Assert.True(model.HisCarrierIsLive(Him));
        Assert.True(card.HasAction, "the card has nothing to press, so this proves nothing");

        model.CardActionCommand.Execute(card);
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

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, HisOver) });

        var card = Assert.Single(model.DigitalCards);

        model.CardActionCommand.Execute(card);
        Settle(model);

        Assert.Equal(0, radio.Sink.TimesCalled);

        // **HIS CARRIER GOES.**
        model.ShowPsk31ChannelsForTests(Array.Empty<Psk31Channel>());

        var row = Assert.Single(model.DigitalDecodes, r => r.IsTextOnly);

        Assert.True(row.Ended);
        Assert.False(model.HisCarrierIsLive(Him));

        var after = Assert.Single(model.DigitalCards);

        model.CardActionCommand.Execute(after);
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

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, HisOver) });

        Assert.True(model.HisCarrierIsLive(Him));

        model.SendCallToAnyoneCommand.Execute(null);
        Settle(model);

        _output.WriteLine($"a CQ while he is sending: keyings {radio.Sink.TimesCalled}, line [{model.DigitalSendLine}]");

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

        // 17:44 - his carrier appears and he calls the operator.
        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1726, 10.0, HisOver) });

        var card = Assert.Single(model.DigitalCards);

        // 17:45:40 - the operator presses the offered Report while that carrier is still up.
        Assert.Equal(Psk31Macro.Report, card.Offered);

        model.CardActionCommand.Execute(card);
        Settle(model);

        _output.WriteLine("what he would see now: " + model.DigitalSendLine);

        Assert.Equal(0, radio.Sink.TimesCalled);

        telemetry.Dispose();

        var refused = Assert.Single(Events("psk31_send_refused"));

        Assert.Equal(MainWindowViewModel.HisCarrierLiveReason, refused.GetProperty("reason").GetString());
        NothingPersonal();
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
