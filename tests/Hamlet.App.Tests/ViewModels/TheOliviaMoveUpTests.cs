using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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

    /// <summary>
    /// **4.5, second half: the line goes out at the old place and the old variant** (decision BN) -
    /// the calling center at 8/250, announced with code 69, read back by the detector.
    /// </summary>
    [Fact]
    public void TheLineGoesOutAtTheOldPlaceAndTheOldVariant()
    {
        FakeSink sink;
        FakePort port;
        string text;
        double calling;

        using (var telemetry = new JsonlTelemetry(_folder, "367", _ => true))
        {
            var model = Listening(telemetry);
            var parts = Arm(model, telemetry);

            sink = parts.Sink;
            port = parts.Port;
            calling = CallingOffsetOn(model);
            text = model.OliviaMoveTextFor(His);

            model.ShowOliviaChannelsForTests([Channel(51, OliviaCallingTable.CallingVariant, calling, HisAnswer)]);
            model.OliviaMoveUpCommand.Execute(TheCard(model));

            Settle(model);
        }

        var lines = Lines();
        var composed = Assert.Single(Events(lines, "psk31_send_composed"));
        var record = Assert.Single(Events(lines, TransmitRecord.EventName));
        var code = OliviaData.Current.Rsid!.CodeOf(
            OliviaModulator.AnnouncedAs(OliviaCallingTable.CallingVariant));
        var (variant, centerHz, readCode) = ReadBack(sink);

        _output.WriteLine("the line: \"" + text + "\"");
        _output.WriteLine($"{text.Length} characters");
        _output.WriteLine("composed: " + composed.GetRawText());
        _output.WriteLine("record  : " + record.GetRawText());
        _output.WriteLine(
            $"sent {OliviaCallingTable.CallingVariant} at {calling:0.0} Hz; read back {variant} code {readCode} "
            + $"at {centerHz:0.00} Hz, {Math.Abs(centerHz - calling):0.00} Hz of error");

        // **BOTH CALLSIGNS, THE AMOUNT AND THE VARIANT** (decision BN).
        foreach (var part in new[] { His, Mine, "500", "16/500", "OLIVIA" })
        {
            Assert.Contains(part, text, StringComparison.Ordinal);
        }

        // **AT THE OLD PLACE AND THE OLD VARIANT**, because that is where he is listening.
        Assert.Equal(Math.Round(calling, 1), composed.GetProperty("offsetHz").GetDouble());
        Assert.Equal("qsy", composed.GetProperty("macro").GetString());
        Assert.Equal(OliviaCallingTable.CallingVariant, variant);
        Assert.Equal(69, code);
        Assert.Equal(code, readCode);
        Assert.InRange(centerHz, calling - 5, calling + 5);

        // **ANNOUNCED, AND THE RECORD SAYS SO** (R32 (b)).
        Assert.True(record.GetProperty("announced").GetBoolean());
        Assert.Equal(code, record.GetProperty("rsidCode").GetInt32());
        Assert.Equal(UnslottedMode.Olivia.ToString(), record.GetProperty("mode").GetString());
        Assert.NotEmpty(port.Written);
        Assert.Equal(1, sink.TimesCalled);
        Assert.Empty(Events(lines, "psk31_send_refused"));
    }

    /// <summary>
    /// **4.5: after the ordinary unkey Hamlet has moved - proved by sending one** (decision BO).
    /// </summary>
    /// <remarks>
    /// **NOT BY READING A FIELD.** The next send to that station is composed, armed, keyed and
    /// handed to the sound card, and the audio the card was handed is fed to `RsidDetector` with no
    /// variant, no center and no start time: it comes back 16/500 at the calling center plus 500 Hz
    /// or this fails.
    /// </remarks>
    [Fact]
    public void AfterTheUnkeyTheNextSendGoesOutFiveHundredHertzUpAtSixteenFiveHundred()
    {
        FakeSink sink;
        double calling;
        (double? AtHz, string? Variant) before;
        (double? AtHz, string? Variant) after;

        using (var telemetry = new JsonlTelemetry(_folder, "367", _ => true))
        {
            var model = Listening(telemetry);
            var parts = Arm(model, telemetry);

            sink = parts.Sink;
            calling = CallingOffsetOn(model);

            model.ShowOliviaChannelsForTests([Channel(52, OliviaCallingTable.CallingVariant, calling, HisAnswer)]);

            before = model.OliviaNextSendForTests(His);

            model.OliviaMoveUpCommand.Execute(TheCard(model));
            Settle(model);

            after = model.OliviaNextSendForTests(His);

            // **HE FOLLOWED AND ANSWERED AT THE NEW PLACE**, which is what gives Hamlet something to
            // say next. His old channel is gone from the list, as the listener drops it.
            model.ShowOliviaChannelsForTests(
            [
                Channel(53, OliviaMoveToVariantForTests, calling + 500, HisAnswer),
            ]);

            var card = TheCard(model);

            _output.WriteLine(
                $"after the move the card says turn \"{card.TurnWord}\", offers {card.OfferedMacro} "
                + $"and reads variant \"{card.OliviaVariant}\"");

            Assert.Equal(Ft8CardActionKind.Send, card.ActionKind);
            Assert.Equal(OliviaMoveToVariantForTests, card.OliviaVariant);

            model.CardActionCommand.Execute(card);
            Settle(model);
        }

        var lines = Lines();
        var sends = Events(lines, "psk31_send_composed");
        var wanted = calling + 500;
        var code = OliviaData.Current.Rsid!.CodeOf(OliviaModulator.AnnouncedAs(OliviaMoveToVariantForTests));
        var (variant, centerHz, readCode) = ReadBack(sink);

        _output.WriteLine($"before the press the next send to him was {Say(before)}");
        _output.WriteLine($"after the unkey it is {Say(after)}");

        foreach (var send in sends)
        {
            _output.WriteLine("composed: " + send.GetRawText());
        }

        _output.WriteLine(
            $"the send after the move read back {variant} code {readCode} at {centerHz:0.00} Hz against "
            + $"{wanted:0.0} Hz wanted, {Math.Abs(centerHz - wanted):0.00} Hz of error");

        Assert.Equal((calling, OliviaCallingTable.CallingVariant), (before.AtHz, before.Variant));
        Assert.Equal((wanted, OliviaMoveToVariantForTests), (after.AtHz, after.Variant));

        Assert.Equal(2, sends.Count);
        Assert.Equal(Math.Round(wanted, 1), sends[1].GetProperty("offsetHz").GetDouble());
        Assert.Equal(70, code);
        Assert.Equal(code, readCode);
        Assert.Equal(OliviaMoveToVariantForTests, variant);
        Assert.InRange(centerHz, wanted - 5, wanted + 5);
        Assert.Equal(2, sink.TimesCalled);
    }

    /// <summary>**A Stop mid-play moves nothing, and the card says so** (decision BO).</summary>
    [Fact]
    public async Task AStopMidPlayMovesNothingAndTheCardSaysSo()
    {
        FakeSink sink;
        string moveLine;
        (double? AtHz, string? Variant) after;
        double calling;

        using (var telemetry = new JsonlTelemetry(_folder, "367", _ => true))
        {
            var model = Listening(telemetry);
            var parts = Arm(model, telemetry);

            sink = parts.Sink;
            calling = CallingOffsetOn(model);

            // **LONG ENOUGH TO STILL BE PLAYING** when the Stop lands, without racing a clock.
            sink.PlaysOver = TimeSpan.FromSeconds(1);

            model.ShowOliviaChannelsForTests([Channel(54, OliviaCallingTable.CallingVariant, calling, HisAnswer)]);
            model.OliviaMoveUpCommand.Execute(TheCard(model));

            Assert.True(sink.Entered.Wait(TimeSpan.FromSeconds(10)), "the play never started");

            model.StopSendingCommand.Execute(null);

            await Task.Run(() => WaitForTheRecord()).ConfigureAwait(true);

            Settle(model);

            after = model.OliviaNextSendForTests(His);
            moveLine = TheCard(model).OliviaMoveLine;
        }

        var lines = Lines();
        var unkeyed = Assert.Single(Events(lines, "psk31_send_unkeyed"));
        var refused = Assert.Single(Events(lines, "olivia_move_refused"));

        _output.WriteLine($"stopped by the token: {sink.StoppedByTheToken}, played {sink.PlayedSoFar} of {sink.SamplesHandedOver}");
        _output.WriteLine("olivia_move_refused: " + refused.GetRawText());
        _output.WriteLine($"the next send to him is still {Say(after)}");
        _output.WriteLine("the card says: \"" + moveLine + "\"");

        Assert.True(sink.StoppedByTheToken, "the play ran to the end rather than being stopped");
        Assert.True(unkeyed.GetProperty("aborted").GetBoolean());
        Assert.Empty(Events(lines, "olivia_move_sent"));
        Assert.False(refused.GetProperty("moved").GetBoolean());

        // **NOTHING MOVED**: the next send to him is still at the calling center at 8/250.
        Assert.Equal((calling, OliviaCallingTable.CallingVariant), (after.AtHz, after.Variant));
        Assert.Equal(OliviaMoveWords.NothingMoved, moveLine);
    }

    /// <summary>
    /// **A send the licence gate refuses moves nothing, and the card says so** (decision BO) - with
    /// the arithmetic saying why the cap itself cannot refuse this line at the calling variant.
    /// </summary>
    /// <remarks>
    /// <para>**THE CAP CANNOT REFUSE THE MOVE LINE AT 8/250, AND THAT IS ARITHMETIC RATHER THAN A
    /// CHOICE.** `timing.json` allows a macro 121 characters of the variant and the framed line is
    /// 90, which is 61.94 s against a cap of 82.60 s, so no cap refusal exists to measure and none
    /// is manufactured. What this measures instead is the other refusal on the same path with
    /// nothing played: the licence gate inside the sequence, which is §0.2's own gate and is not
    /// bypassable from any send path. **Nothing about the cap is loosened**; its seconds and its cap
    /// are printed against each other.</para>
    /// </remarks>
    [Fact]
    public void ALicenceRefusalMovesNothingAndTheCardSaysSo()
    {
        var timing = OliviaData.Current.Timing!;
        var settings = Settings();

        settings.Operator.LicenseClass = Hamlet.RadioEngine.Licensing.LicenseClass.Technician;
        settings.RestrictTransmitToPrivileges = true;

        FakeSink sink;
        FakePort port;
        string moveLine;
        string sendLine;
        (double? AtHz, string? Variant) after;
        double calling;
        string text;
        int fits;

        using (var telemetry = new JsonlTelemetry(_folder, "367", _ => true))
        {
            var model = Listening(telemetry, settings);
            var parts = Arm(model, telemetry);

            sink = parts.Sink;
            port = parts.Port;
            calling = CallingOffsetOn(model);
            text = model.OliviaMoveTextFor(His);

            var composed = OliviaModulator.Compose(
                text, OliviaCallingTable.CallingVariant, calling, 12000, 0.5f, OliviaSendKind.Macro);

            fits = timing.CapMacroCharacters;

            _output.WriteLine(
                $"the move line is {text.Length} characters against a macro count of {fits}: "
                + $"{composed.TextSeconds:0.00} s against a cap of {composed.Cap:0.00} s, {composed.Fit}");

            model.ShowOliviaChannelsForTests([Channel(55, OliviaCallingTable.CallingVariant, calling, HisAnswer)]);
            model.OliviaMoveUpCommand.Execute(TheCard(model));

            sendLine = model.DigitalSendLine;

            Settle(model);

            after = model.OliviaNextSendForTests(His);
            moveLine = TheCard(model).OliviaMoveLine;
        }

        var lines = Lines();

        _output.WriteLine("send line: " + sendLine);
        _output.WriteLine($"the next send to him is still {Say(after)}");
        _output.WriteLine("the card says: \"" + moveLine + "\"");
        _output.WriteLine("every event written: " + string.Join(", ", Names(lines)));

        var refusedMove = Assert.Single(Events(lines, "olivia_move_refused"));
        var record = Assert.Single(Events(lines, TransmitRecord.EventName));

        _output.WriteLine("record             : " + record.GetRawText());
        _output.WriteLine("olivia_move_refused: " + refusedMove.GetRawText());

        // The line fits the cap with room, which is why this refusal is the licence gate's.
        Assert.True(text.Length < fits, "the move line is longer than the macro count");

        // **NOTHING WAS KEYED AND NOTHING WAS PLAYED**, because the gate is inside the path (§0.2).
        Assert.False(record.GetProperty("keyed").GetBoolean());
        Assert.Equal(0, sink.TimesCalled);
        Assert.Empty(port.Written);

        // **AND NOTHING MOVED**: the next send to him is still at the calling center at 8/250.
        Assert.Empty(Events(lines, "olivia_move_sent"));
        Assert.False(refusedMove.GetProperty("moved").GetBoolean());
        Assert.Equal((calling, OliviaCallingTable.CallingVariant), (after.AtHz, after.Variant));
        Assert.Equal(OliviaMoveWords.NothingMoved, moveLine);
    }

    /// <summary>**A `no_announcement` refusal moves nothing, and the card says so** (decision BO).</summary>
    [Fact]
    public void ANoAnnouncementRefusalMovesNothingAndTheCardSaysSo()
    {
        FakeSink sink;
        string moveLine;
        string sendLine;
        (double? AtHz, string? Variant) after;
        double calling;

        using (var telemetry = new JsonlTelemetry(_folder, "367", _ => true))
        {
            var model = Listening(telemetry);
            var parts = Arm(model, telemetry);

            sink = parts.Sink;
            calling = CallingOffsetOn(model);

            model.ShowOliviaChannelsForTests([Channel(56, OliviaCallingTable.CallingVariant, calling, HisAnswer)]);

            // **NO BURST, NO SEND** (R27, unit 365 decision AS). The codes file read back empty, so
            // nothing can name the variant it is sending, so nothing goes out - and nothing moves.
            // **THE CALLING TABLE AND THE FORMAT ARE THE REAL ONES**, because the refusal being
            // measured is the announcement's and a missing table would refuse for another reason.
            model.UseOliviaDataForTests(
                OliviaData.Read(
                    File.ReadAllText(Path.Combine(Root(), "data", "bands", "olivia-calling.json")),
                    "{ }",
                    File.ReadAllText(Path.Combine(Root(), "data", "olivia", "format.json"))));

            model.OliviaMoveUpCommand.Execute(TheCard(model));
            sendLine = model.DigitalSendLine;

            Settle(model);

            after = model.OliviaNextSendForTests(His);
            moveLine = TheCard(model).OliviaMoveLine;
        }

        var lines = Lines();

        _output.WriteLine("send line: " + sendLine);
        _output.WriteLine($"the next send to him is still {Say(after)}");
        _output.WriteLine("the card says: \"" + moveLine + "\"");
        _output.WriteLine("every event written: " + string.Join(", ", Names(lines)));

        var refusedSend = Assert.Single(Events(lines, "psk31_send_refused"));
        var refusedMove = Assert.Single(Events(lines, "olivia_move_refused"));

        _output.WriteLine("psk31_send_refused : " + refusedSend.GetRawText());
        _output.WriteLine("olivia_move_refused: " + refusedMove.GetRawText());

        Assert.Equal("no_announcement", refusedSend.GetProperty("reason").GetString());
        Assert.Equal(0, sink.TimesCalled);
        Assert.Empty(Events(lines, "olivia_move_sent"));
        Assert.False(refusedMove.GetProperty("moved").GetBoolean());

        Assert.Equal((calling, OliviaCallingTable.CallingVariant), (after.AtHz, after.Variant));
        Assert.Equal(OliviaMoveWords.NothingMoved, moveLine);
    }

    /// <summary>
    /// **Decision BK's bound: where the moved-to signal would fall outside the passband Hamlet
    /// listens across, the move is not offered and the card says why.**
    /// </summary>
    [Fact]
    public void WhereTheMoveWouldLandOutsideThePassbandItIsNotOfferedAndTheCardSaysWhy()
    {
        var v = OliviaData.Current.Format!.Variant(OliviaMoveToVariantForTests)!;
        var half = (v.Tones - 1) * v.ToneSpacingHz / 2.0;
        var highest = Psk31CarrierSearch.PassbandHighHz - half - 500;

        _output.WriteLine(
            $"Hamlet listens across {Psk31CarrierSearch.PassbandLowHz} to {Psk31CarrierSearch.PassbandHighHz} Hz; "
            + $"{OliviaMoveToVariantForTests} occupies +/-{half:0.000} Hz, so the highest calling offset the "
            + $"move may be offered at is {highest:0.000} Hz");

        // The calling center on 20 m is 14 073 000 Hz. A dial the operator has moved down puts that
        // center high in the passband; at 2400 Hz the move would land at 2900 and reach 3134 Hz.
        foreach (var dial in new[] { 14_071_500L, 14_070_600L })
        {
            var model = Listening(null, Settings(), Mode, dial);
            var calling = CallingOffsetOn(model);

            model.ShowOliviaChannelsForTests([Channel(57, OliviaCallingTable.CallingVariant, calling, HisAnswer)]);

            var card = TheCard(model);

            _output.WriteLine(
                $"dial {dial}: calling offset {calling:0} Hz, the move would put {OliviaMoveToVariantForTests} "
                + $"from {calling + 500 - half:0.0} to {calling + 500 + half:0.0} Hz - offered {card.HasOliviaMove}, "
                + $"card says \"{card.OliviaMoveLine}\"");

            if (calling <= highest)
            {
                Assert.True(card.HasOliviaMove);
                Assert.Equal("", card.OliviaMoveLine);
                continue;
            }

            Assert.False(card.HasOliviaMove);
            Assert.Equal(OliviaMoveWords.OutsideThePassband, card.OliviaMoveLine);
        }
    }

    /// <summary>
    /// **R13: the move writes its own events - offered, sent, the from and to centers and variants -
    /// with no callsign and no text in any of them.**
    /// </summary>
    [Fact]
    public void TheMoveWritesItsOwnEventsAndNoneOfThemCarriesACallsignOrAWord()
    {
        double calling;

        using (var telemetry = new JsonlTelemetry(_folder, "367", _ => true))
        {
            var model = Listening(telemetry);

            Arm(model, telemetry);

            calling = CallingOffsetOn(model);

            model.ShowOliviaChannelsForTests([Channel(58, OliviaCallingTable.CallingVariant, calling, HisAnswer)]);
            model.OliviaMoveUpCommand.Execute(TheCard(model));

            Settle(model);
        }

        var lines = Lines();
        var offered = Assert.Single(Events(lines, "olivia_move_offered"));
        var sent = Assert.Single(Events(lines, "olivia_move_sent"));

        _output.WriteLine("olivia_move_offered: " + offered.GetRawText());
        _output.WriteLine("olivia_move_sent   : " + sent.GetRawText());

        foreach (var e in new[] { offered, sent })
        {
            Assert.Equal(Math.Round(calling, 1), e.GetProperty("fromHz").GetDouble());
            Assert.Equal(Math.Round(calling + 500, 1), e.GetProperty("toHz").GetDouble());
            Assert.Equal(500.0, e.GetProperty("upHz").GetDouble());
            Assert.Equal(OliviaCallingTable.CallingVariant, e.GetProperty("fromVariant").GetString());
            Assert.Equal(OliviaMoveToVariantForTests, e.GetProperty("toVariant").GetString());
        }

        // **THE WINDOW IS THE PRODUCT AND NOT A NUMBER OF SECONDS** (§3.2, decision BP).
        var timing = OliviaData.Current.Timing!;
        var window = timing.PatienceSeconds(OliviaMoveToVariantForTests) * 2;

        _output.WriteLine(
            $"the window is PatienceSeconds({OliviaMoveToVariantForTests}) x 2 = "
            + $"{timing.PatienceSeconds(OliviaMoveToVariantForTests):0.000} x 2 = {window:0.000} s");

        Assert.Equal(Math.Round(window, 3), sent.GetProperty("windowSeconds").GetDouble());

        // **NO CALLSIGN AND NO TEXT IN ANY LINE OF THE FILE'S MOVE EVENTS** (HM-DEC-018, §2.1).
        foreach (var name in new[] { "olivia_move_offered", "olivia_move_sent", "olivia_move_refused", "olivia_move_answered" })
        {
            foreach (var e in Events(lines, name))
            {
                var raw = e.GetRawText();

                Assert.DoesNotContain(Mine, raw, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(His, raw, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("QSY", raw, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    /// <summary>**One `PttOn` site and two `Arm` calls, counted after the move was built.**</summary>
    /// <remarks>
    /// **THE COUNT IS THE GUARD** (decision AZ). What §6 licenses is an audio generator and an RSID
    /// prefix behind the one sequence, and the way a wider change would show is a second keying site
    /// or a second arming. **The move is a fifth thing to send, not a fifth way to send.**
    /// </remarks>
    [Fact]
    public void ThereIsStillOnePttSiteAndTwoArmCalls()
    {
        var src = Path.Combine(Root(), "src");
        var ptt = Sites(src, "CivConstants.PttOn)");
        var arm = Sites(src, "_armedSend.Arm(");

        foreach (var site in ptt.Concat(arm))
        {
            _output.WriteLine(site);
        }

        Assert.Single(ptt);
        Assert.Equal(2, arm.Count);
    }

    // ---------------------------------------------------------------------------------------
    // The harness. Its shape is `TheOliviaSendTests`'s, which is every send test in this
    // project since unit 260: a fake wire, a fake sound card, and the real tick.
    // ---------------------------------------------------------------------------------------

    /// <summary>Where the next send to a station would go, in one line for the output.</summary>
    private static string Say((double? AtHz, string? Variant) next)
        => (next.AtHz?.ToString("0.0", CultureInfo.InvariantCulture) ?? "nowhere")
            + " Hz at " + (next.Variant ?? "no variant");

    /// <summary>Every code line of `src/` holding this text, a comment not being one.</summary>
    private static List<string> Sites(string src, string needle)
    {
        var found = new List<string>();

        foreach (var path in Directory.EnumerateFiles(src, "*.cs", SearchOption.AllDirectories)
                     .Where(p => !p.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                                 && !p.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                     .OrderBy(p => p, StringComparer.Ordinal))
        {
            var lines = File.ReadAllLines(path);

            for (var i = 0; i < lines.Length; i++)
            {
                var trimmed = lines[i].TrimStart();

                if (!trimmed.StartsWith("//", StringComparison.Ordinal)
                    && !trimmed.StartsWith("*", StringComparison.Ordinal)
                    && lines[i].Contains(needle, StringComparison.Ordinal))
                {
                    found.Add($"{Path.GetRelativePath(src, path)}:{i + 1}: {trimmed}");
                }
            }
        }

        return found;
    }

    /// <summary>Wait, bounded, until the sequence has written the transmission's own record.</summary>
    private void WaitForTheRecord()
    {
        for (var tries = 0; tries < 1000; tries++)
        {
            if (Events(Lines(), TransmitRecord.EventName).Count > 0)
            {
                return;
            }

            Thread.Sleep(10);
        }
    }

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

    /// <summary>The operator's settings, with a licence that permits the send being measured.</summary>
    /// <remarks>
    /// **THE LICENCE GATE IS INSIDE THE PATH AND IT IS NOT BYPASSED HERE** (§0.2). 14.0715 MHz is
    /// inside a General class data segment, so the gate permits it and the send is measured rather
    /// than refused; nothing about the gate is loosened for Olivia.
    /// </remarks>
    private static AppSettings Settings()
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN42";
        settings.Operator.OperatorName = "Pat";
        settings.Operator.Location = "Boston MA";
        settings.Operator.LicenseClass = Hamlet.RadioEngine.Licensing.LicenseClass.General;

        return settings;
    }

    /// <summary>A panel on 20 m with a mode pressed and the tap running.</summary>
    private static MainWindowViewModel Listening(JsonlTelemetry? telemetry, string mode = Mode)
        => Listening(telemetry, Settings(), mode);

    /// <summary>A panel with named settings, a named mode and a named dial.</summary>
    private static MainWindowViewModel Listening(
        JsonlTelemetry? telemetry, AppSettings settings, string mode = Mode, long dialHz = DialOn20m)
    {
        var model = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
            TapForTests = new AudioTap(),
        };

        model.SelectedBand = model.Bands.First(b => b.Band.LowHz <= dialHz && b.Band.HighHz >= dialHz);
        model.FrequencyHz = dialHz;
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

    /// <summary>Every event name in the file, in order, for a reading that has gone wrong.</summary>
    private static List<string> Names(IEnumerable<string> lines)
        => lines
            .Select(l => JsonDocument.Parse(l).RootElement.GetProperty("event").GetString() ?? "")
            .Distinct(StringComparer.Ordinal)
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
