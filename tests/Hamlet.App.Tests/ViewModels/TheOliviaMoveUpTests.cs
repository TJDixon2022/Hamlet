using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Rsid;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 367: **R29's one click - move up 500 Hz and switch to 16/500** (step 4
/// criterion 4.5, decisions BK to BR).
/// </summary>
/// <remarks>
/// <para>**THE OFFER IS R1's GATE AND NO LOOSER ONE** (decision BL). Every *it is not offered* case
/// below is its own assertion, because an offer that appeared on a guess would put a transmission
/// behind a reading Hamlet is not sure of.</para>
/// <para>**NOTHING KEYS A REAL PORT** (FACT-004). The wire is `FakePort` and the card is `FakeSink`.
/// **No Olivia signal from Hamlet has been on the air**, no QSO has been moved off a real calling
/// frequency, and none will be until Tim presses it at step 6.</para>
/// <para>**THE DETECTOR IS NEVER TOLD ANYTHING** (decision I). What a send is read back by is the
/// audio the sound card was actually handed, fed whole to <see cref="RsidDetector"/> with no
/// variant, no center and no start time.</para>
/// </remarks>
public sealed class TheOliviaMoveUpTests : IDisposable
{
    /// <summary>The operator: neither station of any Olivia fixture.</summary>
    private const string Mine = "K1ABC";

    /// <summary>The station being worked. A plain callsign, because a slash opens no card (unit 366 item 1).</summary>
    private const string His = "N1XYZ";

    private const string Mode = "Olivia";

    /// <summary>20 m, where the cited table gives an Olivia calling center and a dial.</summary>
    private const long DialOn20m = 14_071_500;

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each reading is printed.</param>
    public TheOliviaMoveUpTests(ITestOutputHelper output)
    {
        _output = output;
        _folder = Path.Combine(Path.GetTempPath(), "hamlet-olivia-move-" + Guid.NewGuid().ToString("N"));

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

    /// <summary>His certain answer on the calling frequency: the one reading that offers anything.</summary>
    private static string HisAnswer => Mine + " de " + His + " " + His + " K\n";

    /// <summary>
    /// **4.5, first half: the move is offered on a certain answer, under Olivia, on the cited
    /// calling center, at 8/250 - all four of decision BL's conditions together.**
    /// </summary>
    [Fact]
    public void TheMoveIsOfferedOnHisCertainAnswerAtTheCallingCenterAt8250()
    {
        var model = Listening(null);
        var calling = CallingOffsetOn(model);

        model.ShowOliviaChannelsForTests([Channel(31, OliviaCallingTable.CallingVariant, calling, HisAnswer)]);

        var card = TheCard(model);

        _output.WriteLine($"the calling center on 20 m is {calling:0} Hz in the passband at dial {DialOn20m}");
        _output.WriteLine($"the card says turn \"{card.TurnWord}\", offered {card.OfferedMacro}, variant \"{card.OliviaVariant}\"");
        _output.WriteLine($"the move button reads \"{card.OliviaMoveLabel}\"");
        _output.WriteLine($"its hover says \"{card.OliviaMoveTip}\"");

        // 1. Olivia chosen. 2. A certain your turn over his certain handover - the same reading
        // Psk31Offer.For demands, which is why a macro is offered at all. 3. The calling center.
        // 4. The calling variant.
        Assert.Equal(Mode, model.ChosenDigitalMode);
        Assert.Equal(Psk31TurnState.YourTurn, card.Turn!.State);
        Assert.True(card.Turn.IsCertain);
        Assert.NotEqual(Psk31Macro.None, card.Offered);
        Assert.Equal(OliviaCallingTable.CallingVariant, card.OliviaVariant);

        // **AND THE MOVE IS THERE, BESIDE THE MACRO OFFER AND NOT INSTEAD OF IT** (decision BL).
        Assert.True(card.HasOliviaMove);
        Assert.Equal(Ft8CardActionKind.Send, card.ActionKind);
        Assert.NotEqual(card.ActionLabel, card.OliviaMoveLabel);

        // Nothing is said about a follow yet, because nothing has been sent.
        Assert.Equal("", card.OliviaMoveLine);
    }

    /// <summary>**The label is one click and says what it does** - quoted verbatim in the report.</summary>
    [Fact]
    public void TheLabelIsOneClickAndSaysWhatItDoes()
    {
        var model = Listening(null);

        model.ShowOliviaChannelsForTests(
            [Channel(32, OliviaCallingTable.CallingVariant, CallingOffsetOn(model), HisAnswer)]);

        var card = TheCard(model);

        _output.WriteLine("label: \"" + card.OliviaMoveLabel + "\"");
        _output.WriteLine("tip  : \"" + card.OliviaMoveTip + "\"");

        // **R29's OWN WORDS.** The plan says the card offers *move up 500 Hz and switch to 16/500*.
        Assert.Equal("Move up 500 Hz and switch to 16/500", card.OliviaMoveLabel);
        Assert.Equal(OliviaMoveWords.Label, card.OliviaMoveLabel);

        foreach (var word in new[] { "500", "16/500", "up" })
        {
            Assert.Contains(word, card.OliviaMoveLabel, StringComparison.OrdinalIgnoreCase);
        }

        // **AND NOTHING IS ASKED OF HIM AT THE RADIO** (§R11): neither sentence points at a knob.
        foreach (var word in new[] { "dial", "VFO", "on the radio", "meter" })
        {
            Assert.DoesNotContain(word, card.OliviaMoveLabel, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(word, card.OliviaMoveTip, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>**It is not offered on a guessed *your turn*** (§R1). Its own assertion.</summary>
    [Fact]
    public void ItIsNotOfferedOnAGuessedYourTurn()
    {
        var model = Listening(null);
        var calling = CallingOffsetOn(model);

        model.ShowOliviaChannelsForTests([Channel(33, OliviaCallingTable.CallingVariant, calling, HisAnswer)]);

        Assert.True(TheCard(model).HasOliviaMove);

        // A word carrying a character that is neither letter, digit nor joiner is damage, and one
        // damaged word makes the whole message uncertain (Psk31ExchangeParser).
        model.ShowOliviaChannelsForTests(
        [
            Channel(33, OliviaCallingTable.CallingVariant, calling, HisAnswer + Mine + " de " + His + " W1#W K\n"),
        ]);

        var card = TheCard(model);

        _output.WriteLine($"after a damaged message the card says turn \"{card.TurnWord}\", offered {card.OfferedMacro}");

        Assert.Equal(Psk31TurnState.YourTurn, card.Turn!.State);
        Assert.False(card.Turn.IsCertain);
        Assert.Equal(Psk31Macro.None, card.Offered);
        Assert.False(card.HasOliviaMove);
        Assert.Equal("", card.OliviaMoveLine);
    }

    /// <summary>**It is not offered on *his turn*** - Hamlet has handed over. Its own assertion.</summary>
    [Fact]
    public void ItIsNotOfferedOnHisTurn()
    {
        var model = Listening(null);
        var calling = CallingOffsetOn(model);

        model.ShowOliviaChannelsForTests(
        [
            Channel(
                34, OliviaCallingTable.CallingVariant, calling,
                HisAnswer + His + " de " + Mine + " RST 599 599 BTU " + His + " de " + Mine + " K\n"),
        ]);

        var card = TheCard(model);

        _output.WriteLine($"the card says turn \"{card.TurnWord}\", offered {card.OfferedMacro}");

        Assert.Equal(Psk31TurnState.HisTurn, card.Turn!.State);
        Assert.False(card.HasOliviaMove);
    }

    /// <summary>**It is not offered while *he is still sending***. Its own assertion.</summary>
    [Fact]
    public void ItIsNotOfferedWhileHeIsStillSending()
    {
        var model = Listening(null);
        var calling = CallingOffsetOn(model);

        model.ShowOliviaChannelsForTests(
        [
            Channel(35, OliviaCallingTable.CallingVariant, calling, HisAnswer + "and one more thing"),
        ]);

        var card = TheCard(model);

        _output.WriteLine($"the card says turn \"{card.TurnWord}\", offered {card.OfferedMacro}");

        Assert.Equal(Psk31TurnState.HeIsSending, card.Turn!.State);
        Assert.False(card.HasOliviaMove);
    }

    /// <summary>**It is not offered under PSK31** (decision BL's first condition). Its own assertion.</summary>
    /// <remarks>
    /// **THE SAME CHANNEL, THE SAME CENTER, THE SAME VARIANT, THE SAME CERTAIN ANSWER** - and the
    /// only thing different is which tab is pressed. So this fails if the mode test is dropped and
    /// passes for no other reason.
    /// </remarks>
    [Fact]
    public void ItIsNotOfferedUnderPsk31()
    {
        var model = Listening(null, "PSK31");
        var calling = CallingOffsetOn(model);

        model.ShowOliviaChannelsForTests([Channel(36, OliviaCallingTable.CallingVariant, calling, HisAnswer)]);

        var card = TheCard(model);

        _output.WriteLine($"under {model.ChosenDigitalMode} the card says turn \"{card.TurnWord}\", offered {card.OfferedMacro}");

        Assert.NotEqual(Mode, model.ChosenDigitalMode);
        Assert.Equal(Psk31TurnState.YourTurn, card.Turn!.State);
        Assert.NotEqual(Psk31Macro.None, card.Offered);
        Assert.False(card.HasOliviaMove);
        Assert.Equal("", card.OliviaMoveLine);
    }

    /// <summary>**It is not offered at 16/500 or at 32/1000** - the conversation is already widened.</summary>
    [Fact]
    public void ItIsNotOfferedAtTheWiderVariants()
    {
        foreach (var variant in new[] { "16/500", "32/1000" })
        {
            var model = Listening(null);

            model.ShowOliviaChannelsForTests([Channel(37, variant, CallingOffsetOn(model), HisAnswer)]);

            var card = TheCard(model);

            _output.WriteLine($"at {variant} on the calling center: offered {card.OfferedMacro}, move {card.HasOliviaMove}");

            Assert.Equal(variant, card.OliviaVariant);
            Assert.NotEqual(Psk31Macro.None, card.Offered);
            Assert.False(card.HasOliviaMove);
        }
    }

    /// <summary>**It is not offered where the conversation is not on the cited calling center.**</summary>
    /// <remarks>
    /// **AND THE EDGE IS THE CALLING VARIANT'S OWN WIDTH**, from the format file: a channel whose
    /// measured center is inside half of 8/250's occupied bandwidth is on the calling center, and one
    /// outside it is a conversation somewhere else that R29 says nothing about.
    /// </remarks>
    [Fact]
    public void ItIsNotOfferedAwayFromTheCallingCenter()
    {
        var v = OliviaData.Current.Format!.Variant(OliviaCallingTable.CallingVariant)!;
        var half = (v.Tones - 1) * v.ToneSpacingHz / 2.0;

        _output.WriteLine($"8/250 occupies {(v.Tones - 1) * v.ToneSpacingHz:0.0} Hz of tones, so half is {half:0.000} Hz");

        foreach (var away in new[] { 0.0, half - 1, half + 1, 300.0, -400.0 })
        {
            var model = Listening(null);
            var calling = CallingOffsetOn(model);

            model.ShowOliviaChannelsForTests(
                [Channel(38, OliviaCallingTable.CallingVariant, calling + away, HisAnswer)]);

            var card = TheCard(model);

            _output.WriteLine(
                $"his carrier {away:+0.0;-0.0;0} Hz from the calling center ({calling + away:0.0} Hz): "
                + $"move {card.HasOliviaMove}");

            Assert.Equal(Math.Abs(away) <= half, card.HasOliviaMove);
        }
    }

    /// <summary>
    /// **It is not offered a second time after it has been used** (decision BL, once per conversation).
    /// </summary>
    [Fact]
    public void ItIsNotOfferedASecondTimeAfterItHasBeenUsed()
    {
        using var telemetry = new JsonlTelemetry(_folder, "367", _ => true);

        var model = Listening(telemetry);
        var parts = Arm(model, telemetry);
        var calling = CallingOffsetOn(model);

        model.ShowOliviaChannelsForTests([Channel(39, OliviaCallingTable.CallingVariant, calling, HisAnswer)]);

        var card = TheCard(model);

        Assert.True(card.HasOliviaMove);

        model.OliviaMoveUpCommand.Execute(card);
        Settle(model);

        var after = TheCard(model);

        _output.WriteLine($"after the press the card says \"{after.OliviaMoveLine}\", move offered {after.HasOliviaMove}");

        Assert.Equal(1, parts.Sink.TimesCalled);
        Assert.False(after.HasOliviaMove);
        Assert.NotEqual("", after.OliviaMoveLine);

        // **AND A FURTHER CERTAIN ANSWER DOES NOT BRING IT BACK.** The conversation has moved; there
        // is nothing left to offer to move.
        model.ShowOliviaChannelsForTests(
        [
            Channel(40, OliviaMoveToVariantForTests, calling + 500, Mine + " de " + His + " R R here K\n"),
        ]);

        var later = TheCard(model);

        _output.WriteLine($"and after his 16/500 answer at the new place: move offered {later.HasOliviaMove}");

        Assert.False(later.HasOliviaMove);
    }

    /// <summary>
    /// **PSK31's card is unchanged: the two shared types have exactly the members they had, and a
    /// PSK31 card carries nothing about a move** (decision BM).
    /// </summary>
    [Fact]
    public void Psk31sCardIsUnchangedAndNoSharedTypeGrewAMember()
    {
        var kinds = Enum.GetNames<Ft8CardActionKind>();
        var macros = Enum.GetNames<Psk31Macro>();

        _output.WriteLine("Ft8CardActionKind: " + string.Join(", ", kinds));
        _output.WriteLine("Psk31Macro       : " + string.Join(", ", macros));

        // **NO FOURTH ACTION KIND AND NO FIFTH MACRO** (decision BM). The move is its own control.
        Assert.Equal(["None", "Send", "Log"], kinds);
        Assert.Equal(["None", "Cq", "Answer", "Report", "Confirm"], macros);

        // A card built the way PSK31's is built carries no move at all.
        var psk31 = Ft8ContactCard.ForPsk31(
            His, new Psk31TurnReading(Psk31TurnState.YourTurn, true), "FN42", Psk31Macro.Report,
            null, "text", false, Mine);

        _output.WriteLine($"a PSK31 card: move offered {psk31.HasOliviaMove}, line \"{psk31.OliviaMoveLine}\", olivia {psk31.IsOlivia}");

        Assert.False(psk31.HasOliviaMove);
        Assert.False(psk31.HasOliviaMoveLine);
        Assert.False(psk31.IsOlivia);
        Assert.Equal(Ft8CardActionKind.Send, psk31.ActionKind);
        Assert.Equal("Tell him how he is coming through", psk31.ActionLabel);
    }

    /// <summary>
    /// **Nothing is composed and nothing keys by the offer merely appearing** (§0.2): the transmit
    /// category is empty until the press.
    /// </summary>
    [Fact]
    public void TheOfferAppearingComposesNothingAndKeysNothing()
    {
        FakeSink sink;
        FakePort port;
        bool offered;

        using (var telemetry = new JsonlTelemetry(_folder, "367", _ => true))
        {
            var model = Listening(telemetry);
            var parts = Arm(model, telemetry);

            sink = parts.Sink;
            port = parts.Port;

            model.ShowOliviaChannelsForTests(
                [Channel(41, OliviaCallingTable.CallingVariant, CallingOffsetOn(model), HisAnswer)]);

            offered = TheCard(model).HasOliviaMove;

            Settle(model);
        }

        var lines = Lines();

        _output.WriteLine($"the move was offered: {offered}");
        _output.WriteLine("stages: " + string.Join(" | ", Stages(lines)));
        _output.WriteLine("olivia_move_offered lines: " + Events(lines, "olivia_move_offered").Count);

        Assert.True(offered);
        Assert.Equal(0, sink.TimesCalled);
        Assert.Empty(port.Written);
        Assert.Empty(Stages(lines));
        Assert.Empty(Events(lines, "psk31_send_composed"));
        Assert.Empty(Events(lines, TransmitRecord.EventName));

        // **AND THE OFFER ITSELF IS NOT AN EVENT.** Only the press writes one, because only the
        // press is something the operator did.
        Assert.Empty(Events(lines, "olivia_move_offered"));
    }

    // ---------------------------------------------------------------------------------------
    // The harness. Its shape is `TheOliviaSendTests`'s, which is every send test in this
    // project since unit 260: a fake wire, a fake sound card, and the real tick.
    // ---------------------------------------------------------------------------------------

    /// <summary>What R29 widens to, read from the one place it is written.</summary>
    private static string OliviaMoveToVariantForTests => "16/500";

    /// <summary>The one card on the panel for the station being worked.</summary>
    private static Ft8ContactCard TheCard(MainWindowViewModel model)
        => Assert.Single(model.DigitalCards, c => string.Equals(c.Callsign, His, StringComparison.Ordinal));

    /// <summary>An Olivia channel as the listener would list it.</summary>
    private static OliviaChannel Channel(int id, string variant, double centerHz, string text)
        => new(id, variant, centerHz, OliviaListener.FoundByRsid, 0, text, text.Length > 0 ? 9 : 0, 0, false, centerHz);

    /// <summary>Where the band's Olivia calling center sits in the passband, by the table's own arithmetic.</summary>
    private static double CallingOffsetOn(MainWindowViewModel model)
    {
        var row = OliviaData.Current.Calling!.CallingRowFor(model.SelectedBand.Band.Name)!;

        return row.CenterHz - (double)model.FrequencyHz;
    }

    /// <summary>A panel on 20 m with a mode pressed and the tap running.</summary>
    private static MainWindowViewModel Listening(JsonlTelemetry? telemetry, string mode = Mode)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN42";
        settings.Operator.OperatorName = "Pat";
        settings.Operator.Location = "Boston MA";

        // **THE LICENCE GATE IS INSIDE THE PATH AND IT IS NOT BYPASSED HERE** (§0.2).
        settings.Operator.LicenseClass = Hamlet.RadioEngine.Licensing.LicenseClass.General;

        var model = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
            TapForTests = new AudioTap(),
        };

        model.SelectedBand = model.Bands.First(b => b.Band.LowHz <= DialOn20m && b.Band.HighHz >= DialOn20m);
        model.FrequencyHz = DialOn20m;
        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.ChooseDigitalModeCommand.Execute(mode);

        return model;
    }

    /// <summary>Give the panel the fake wire and the fake sound card, and the one armed send.</summary>
    private static (FakeSink Sink, FakePort Port) Arm(MainWindowViewModel model, JsonlTelemetry telemetry)
    {
        var port = new FakePort();
        var sink = new FakeSink();

        model.UseRigPortForTests(port);
        model.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(port, sink, guard: null, telemetry)));

        return (sink, port);
    }

    /// <summary>Wait for the send that the press started, and run what it posted to the UI thread.</summary>
    private static void Settle(MainWindowViewModel model)
    {
        for (var tries = 0; tries < 2000 && model.HasSomethingToStop; tries++)
        {
            Thread.Sleep(10);
        }

        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
    }

    /// <summary>What the detector makes of the audio the sound card was handed, told nothing.</summary>
    private (string Variant, double CenterHz, int Code) ReadBack(FakeSink sink)
    {
        Assert.NotEmpty(sink.LastSamples);

        var codes = OliviaData.Current.Rsid!;
        var audio = new MonoAudio(sink.RateAskedFor, sink.LastSamples);
        var heard = RsidDetector.Detect(codes, audio, 200, audio.SampleRate / 2.0 - 200);

        foreach (var one in heard)
        {
            _output.WriteLine($"detected: code {one.Code} \"{one.Name}\" at {one.CenterHz:0.00} Hz");
        }

        var detection = Assert.Single(heard);
        var variant = RsidCodes.VariantOf(detection.Name);

        Assert.NotNull(variant);

        return (variant!, detection.CenterHz, detection.Code);
    }

    private List<string> Lines()
        => Directory.GetFiles(_folder, "*.jsonl").SelectMany(File.ReadAllLines).ToList();

    private static List<JsonElement> Events(IEnumerable<string> lines, string name)
        => lines
            .Select(l => JsonDocument.Parse(l).RootElement)
            .Where(e => e.GetProperty("event").GetString() == name)
            .Select(e => e.GetProperty("data").Clone())
            .ToList();

    private static List<string> Stages(IEnumerable<string> lines)
        => Events(lines, SendStage.EventName)
            .Select(e => e.GetProperty("stage").GetString() ?? "")
            .ToList();

    /// <summary>A fixture from the mode author's set, after its hash has been checked against the manifest.</summary>
    private static MonoAudio Fixture(string file)
    {
        var folder = Path.Combine(Root(), "assets", "fixtures", "olivia");

        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(folder, "manifest.json")));

        var entry = manifest.RootElement.EnumerateArray().Single(e => e.GetProperty("file").GetString() == file);
        var path = Path.Combine(folder, file);

        Assert.Equal(
            entry.GetProperty("sha256").GetString(),
            Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant());

        return WavAudio.Read(path);
    }

    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }
}
