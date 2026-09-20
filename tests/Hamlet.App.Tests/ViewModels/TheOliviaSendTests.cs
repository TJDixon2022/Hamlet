using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
/// Work instruction 366 task 2: **an Olivia press goes out through the one unslotted sequence**
/// (step 4 criteria 4.2 and 4.6, decisions AZ, BA, BF, BH and BI).
/// </summary>
/// <remarks>
/// <para>**THE DETECTOR IS NEVER TOLD ANYTHING** (decision I). What each send is read back by is
/// the audio the sound card was actually handed, fed whole to <see cref="RsidDetector"/> with no
/// variant, no center and no start time. It finds what was sent or the test fails.</para>
/// <para>**NOTHING KEYS A REAL PORT** (FACT-004, criterion 4.6). The wire is `FakePort` and the
/// card is `FakeSink`, as every send test in this project has been since unit 260. **No Olivia
/// signal from Hamlet has been on the air**, and none will be until Tim presses it at step 6.</para>
/// <para>**AND THE ROWS ARE THE MODE AUTHOR'S OWN AUDIO**, fed to the real tick a quarter-second
/// at a time as `TheOliviaRowsTests` feeds it, so the variant and center a send goes at are the
/// ones a signal announced and never the ones a test chose.</para>
/// </remarks>
public sealed class TheOliviaSendTests : IDisposable
{
    /// <summary>The operator's callsign: neither station's in any Olivia fixture.</summary>
    private const string Mine = "K1ABC";

    private const string Mode = "Olivia";

    /// <summary>20 m, where the cited table gives an Olivia calling center and a dial.</summary>
    private const long DialOn20m = 14_071_500;

    /// <summary>The compound callsign the longest Report in the tree is addressed to.</summary>
    private const string Compound = "VP2V/W1AW";

    /// <summary>The slowest variant's blocks of noise fed after each file, as unit 364 feeds it.</summary>
    private const int TailBlocks = 4;

    private const double TailRms = 0.001;

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each send is printed.</param>
    public TheOliviaSendTests(ITestOutputHelper output)
    {
        _output = output;
        _folder = Path.Combine(Path.GetTempPath(), "hamlet-olivia-send-" + Guid.NewGuid().ToString("N"));

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

    private static OliviaFormat Format => OliviaData.Current.Format!;

    /// <summary>
    /// **Olivia's send guard (decision BH): a CQ passes composed, armed, keyed and handed to the
    /// sound card, unkeys ordinarily, and writes one record announced with code 69.**
    /// </summary>
    /// <remarks>
    /// **THIS IS THE NAME ON THE CARRY-FORWARD LINE.** Every working mode has one send guard, and
    /// until this unit Olivia had none because nothing could send it (HM-DEC-165). It is the whole
    /// path: the press, the composition at the cited calling center, the arming at the one
    /// unslotted `Arm` site, the keying frame taken by the wire, the samples handed to the card,
    /// and the record afterwards.
    /// </remarks>
    [Fact]
    public void AnOliviaCqReachesTheAir()
    {
        var (sink, port, lines) = Pressed(model => model.SendCallToAnyoneCommand.Execute(null));

        var stages = Stages(lines);
        var composed = Assert.Single(Events(lines, "psk31_send_composed"));
        var record = Assert.Single(Events(lines, TransmitRecord.EventName));

        _output.WriteLine("stages   : " + string.Join(" | ", stages));
        _output.WriteLine("composed : " + composed.GetRawText());
        _output.WriteLine("record   : " + record.GetRawText());

        foreach (var stage in new[]
                 {
                     SendStage.Composed, SendStage.Armed, SendStage.Keyed, SendStage.HandedToTheSoundCard,
                 })
        {
            Assert.Contains(stage, stages);
        }

        Assert.Equal(1, sink.TimesCalled);
        Assert.NotEmpty(port.Written);

        // **ANNOUNCED, WITH THE CALLING VARIANT'S OWN CODE.** 69 is `OLIVIA_8_250` in
        // `rsid-codes.json`, read rather than typed, so a renumbered file moves this with it.
        var code = OliviaData.Current.Rsid!.CodeOf(OliviaModulator.AnnouncedAs(OliviaCallingTable.CallingVariant));

        _output.WriteLine($"the calling variant {OliviaCallingTable.CallingVariant} is announced as code {code}");

        Assert.Equal(69, code);
        Assert.True(record.GetProperty("announced").GetBoolean());
        Assert.Equal(code, record.GetProperty("rsidCode").GetInt32());
        Assert.Equal(UnslottedMode.Olivia.ToString(), record.GetProperty("mode").GetString());
        Assert.True(composed.GetProperty("announced").GetBoolean());

        // **IT UNKEYED ORDINARILY**: the play ran out rather than being stopped.
        Assert.Contains(SendStage.Unkeyed, stages);
        Assert.False(Assert.Single(Events(lines, "psk31_send_unkeyed")).GetProperty("aborted").GetBoolean());
        Assert.Empty(Events(lines, "psk31_send_refused"));
    }

    /// <summary>
    /// **4.2 whole, and decision BA: a CQ goes at the calling variant on the cited calling center,
    /// and the detector reads back the variant and the center that were sent, within 5 Hz.**
    /// </summary>
    [Fact]
    public void TheCqIsAnnouncedAtTheCallingVariantOnTheCitedCenter()
    {
        var table = OliviaData.Current.Calling!;
        var row = table.CallingRowFor("20 m")!;
        var wanted = row.CenterHz - (double)DialOn20m;

        var (sink, _, lines) = Pressed(model => model.SendCallToAnyoneCommand.Execute(null));

        var (variant, centerHz) = ReadBack(sink);
        var composed = Assert.Single(Events(lines, "psk31_send_composed"));

        _output.WriteLine(
            $"the table: 20 m calls on {row.CenterHz} Hz at {row.Variant}; the dial is {DialOn20m} Hz, "
            + $"so the passband offset is {wanted:0.0} Hz");
        _output.WriteLine($"read back: {variant} at {centerHz:0.00} Hz, error {Math.Abs(centerHz - wanted):0.00} Hz");

        Assert.Equal(OliviaCallingTable.CallingVariant, variant);
        Assert.InRange(centerHz, wanted - 5, wanted + 5);
        Assert.Equal(Math.Round(wanted, 1), composed.GetProperty("offsetHz").GetDouble());

        // **NOT A LITERAL ANYWHERE.** The offset is the table's center less the dial, so moving
        // the dial moves it.
        Assert.Equal(1500, wanted);
    }

    /// <summary>
    /// **4.2 and decision BA: an Answer to the 8/250 row and a Report to the 16/500 row each go at
    /// that row's own variant and center, and the detector reads each back as what was sent.**
    /// </summary>
    /// <param name="variant">The variant the row's RSID announced.</param>
    /// <param name="centerHz">Where that row sits in the passband.</param>
    [Theory]
    [InlineData("8/250", 1000.0)]
    [InlineData("16/500", 2000.0)]
    public void AnAnswerGoesAtTheRowsOwnVariantAndCenter(string variant, double centerHz)
    {
        FakeSink sink;
        List<string> lines;
        string sent;

        using (var telemetry = new JsonlTelemetry(_folder, "366", _ => true))
        {
            var model = Listening(telemetry);
            var sinkAndPort = Arm(model, telemetry);

            sink = sinkAndPort.Sink;

            Feed(model, Fixture("olivia-two-signals-rsid.wav"));

            // **THE VARIANT AND THE CENTER COME OFF THE SIGNAL'S OWN ROW** (decision I). The two
            // stations in the file are found by their RSID and drawn as rows, and what this reads
            // back off them is the variant each announced and the center each was measured at.
            var heard = Assert.Single(model.DigitalDecodes, r => r.Variant == variant);

            foreach (var r in model.DigitalDecodes)
            {
                _output.WriteLine($"heard: {r.Variant} at {r.Hz} Hz, sender [{r.Sender}]");
            }

            Assert.InRange(Hz(heard), centerHz - 5, centerHz + 5);

            // **THE FILE'S TWO CQs NEVER FINISH A LINE**, so neither row is answerable: both end
            // `pse K` with nothing after, and a message the parser has not closed has no speaker
            // and no kind. **So a third channel finishes one**, carrying the variant and the center
            // read off the real row above rather than any number this test chose - the same seam
            // `TheOliviaRowsTests` uses for exactly this, for exactly this reason.
            var finished = new OliviaChannel(
                97, heard.Variant, Hz(heard), OliviaListener.FoundByRsid, 0,
                "CQ CQ CQ de N1XYZ N1XYZ N1XYZ pse K\n", 9, 0, false);

            model.ShowOliviaChannelsForTests(
                model.OliviaChannelsForTests.Where(c => !c.Ended).Append(finished).ToList());

            var row = Assert.Single(model.DigitalDecodes, r => r.Variant == variant && r.Sender.Length > 0);

            // **THE ROW IS A CERTAIN CQ FROM SOMEBODY ELSE, SO THERE IS AN ANSWER TO PRESS** (§R1).
            Assert.NotNull(model.Psk31AnswerLabelFor(row));

            model.AnswerPsk31Command.Execute(row);
            sent = model.DigitalSendLine;

            Settle(model);
        }

        lines = Lines();

        var (readVariant, readCenter) = ReadBack(sink);
        var composed = Assert.Single(Events(lines, "psk31_send_composed"));

        _output.WriteLine("send line: " + sent);
        _output.WriteLine($"read back: {readVariant} at {readCenter:0.00} Hz against {centerHz:0.0}");

        Assert.Equal(variant, readVariant);
        Assert.InRange(readCenter, centerHz - 5, centerHz + 5);
        Assert.Equal(variant, composed.GetProperty("variant").GetString());
        Assert.Equal("olivia", composed.GetProperty("mode").GetString());
        Assert.Equal(1, sink.TimesCalled);
    }

    /// <summary>**Decision BA: no control anywhere offers a variant.**</summary>
    /// <remarks>
    /// **R27 SAYS THE OPERATOR NEVER PICKS ONE**, so the guard is on what the panel binds rather
    /// than on the send path: a list, a chooser or a command whose name carries a variant is how
    /// that ruling would be broken, and there is none.
    /// </remarks>
    [Fact]
    public void NoControlOffersAVariant()
    {
        var offered = typeof(MainWindowViewModel)
            .GetProperties()
            .Where(p => p.CanWrite || p.PropertyType.Name.Contains("Collection", StringComparison.Ordinal))
            .Select(p => p.Name)
            .Where(n => n.Contains("Variant", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var commands = typeof(MainWindowViewModel)
            .GetMembers()
            .Select(m => m.Name)
            .Where(n => n.Contains("Variant", StringComparison.OrdinalIgnoreCase)
                        && n.Contains("Command", StringComparison.Ordinal))
            .ToList();

        _output.WriteLine("settable or list members naming a variant: " + (offered.Count == 0 ? "none" : string.Join(", ", offered)));
        _output.WriteLine("commands naming a variant                : " + (commands.Count == 0 ? "none" : string.Join(", ", commands)));

        Assert.Empty(offered);
        Assert.Empty(commands);
    }

    /// <summary>
    /// **Decision AS: with the RSID codes unreadable, an Olivia press is refused in words and keys
    /// nothing - never sent unannounced.**
    /// </summary>
    [Fact]
    public void WithTheCodesUnreadableThePressIsRefusedAndNothingKeys()
    {
        FakeSink sink;
        FakePort port;
        string line;

        using (var telemetry = new JsonlTelemetry(_folder, "366", _ => true))
        {
            var model = Listening(telemetry);
            var parts = Arm(model, telemetry);

            sink = parts.Sink;
            port = parts.Port;

            // **THE FILE IS NOT EDITED** (§10). A reading of the data with the codes missing is
            // handed to the panel, which is the state a malformed `rsid-codes.json` leaves it in.
            model.UseOliviaDataForTests(OliviaData.Read(null, "{ }", null));

            model.SendCallToAnyoneCommand.Execute(null);
            line = model.DigitalSendLine;

            Settle(model);
        }

        var lines = Lines();

        _output.WriteLine("send line: " + line);

        Assert.Equal(0, sink.TimesCalled);
        Assert.Empty(port.Written);
        Assert.Empty(Events(lines, TransmitRecord.EventName));
        Assert.DoesNotContain(SendStage.Composed, Stages(lines));
        Assert.NotEmpty(line);
    }

    /// <summary>
    /// **4.6: Stop pressed part way through an 8/250 send aborts it by PSK31's own path, with an
    /// ordinary unkey and the abort's record.**
    /// </summary>
    [Fact]
    public async Task StopMidPlayAbortsAnOliviaSend()
    {
        FakeSink sink;
        string stopped;

        using (var telemetry = new JsonlTelemetry(_folder, "366", _ => true))
        {
            var model = Listening(telemetry);
            var parts = Arm(model, telemetry);

            sink = parts.Sink;

            // **LONG ENOUGH TO STILL BE PLAYING.** An 8/250 CQ is about thirty seconds of audio;
            // the fake plays it over a wall second so the press lands in the middle of it without
            // the test racing a clock.
            sink.PlaysOver = TimeSpan.FromSeconds(1);

            model.SendCallToAnyoneCommand.Execute(null);

            Assert.True(sink.Entered.Wait(TimeSpan.FromSeconds(10)), "the play never started");

            model.StopSendingCommand.Execute(null);
            stopped = model.DigitalSendLine;

            // **THE STOP UNARMS AT ONCE AND THE RUN FINISHES AFTERWARDS** (§0.2: the abort is
            // same-thread and no-await). So waiting on `HasSomethingToStop` would let go while the
            // sequence was still writing what it did, and the lines this test reads would be
            // written after the file was closed. It waits for the record instead.
            await Task.Run(() => WaitForTheRecord()).ConfigureAwait(true);

            Settle(model);
        }

        var lines = Lines();
        var unkeyed = Assert.Single(Events(lines, "psk31_send_unkeyed"));
        var record = Assert.Single(Events(lines, TransmitRecord.EventName));

        _output.WriteLine("stop line: " + stopped);
        _output.WriteLine("record   : " + record.GetRawText());
        _output.WriteLine(
            $"played {sink.PlayedSoFar} of {sink.SamplesHandedOver} samples; stopped by the token {sink.StoppedByTheToken}");

        Assert.True(sink.StoppedByTheToken, "the play ran to the end rather than being stopped");
        Assert.True(unkeyed.GetProperty("aborted").GetBoolean());
        Assert.Equal(UnslottedMode.Olivia.ToString(), record.GetProperty("mode").GetString());
        Assert.True(sink.PlayedSoFar < sink.SamplesHandedOver, "every sample went out after the stop");
    }

    /// <summary>
    /// **4.6: one `PttOn` site and one unslotted `Arm` call, counted in `src/` after the gate
    /// opened.**
    /// </summary>
    /// <remarks>
    /// **THE COUNT IS THE GUARD** (decision AZ). What §6 licenses is an audio generator and an RSID
    /// prefix behind the one sequence, and the way a wider change would show is a second keying
    /// site or a second arming.
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

    /// <summary>
    /// **Decision BH: the send's own lines carry Olivia and the variant, and none of them carries a
    /// word anybody typed or a callsign.**
    /// </summary>
    [Fact]
    public void TheSendsLinesCarryTheModeAndTheVariantAndNothingPersonal()
    {
        var (_, _, lines) = Pressed(model => model.SendCallToAnyoneCommand.Execute(null));

        var theSends = new[] { "psk31_send_composed", "psk31_send_keyed", "psk31_send_unkeyed" };

        foreach (var name in theSends)
        {
            var e = Events(lines, name).First();

            _output.WriteLine(name + ": " + e.GetRawText());

            Assert.Equal("olivia", e.GetProperty("mode").GetString());
            Assert.Equal(OliviaCallingTable.CallingVariant, e.GetProperty("variant").GetString());
        }

        // **NOT HIS CALLSIGN ANYWHERE IN THE FILE** (HM-DEC-018, §2.1). The CQ is the operator's
        // own call three times over, and not one line of the record may hold it.
        Assert.All(lines, l => Assert.DoesNotContain(Mine, l, StringComparison.OrdinalIgnoreCase));

        // **AND NOT A WORD OF WHAT WENT OUT, ON THE SEND'S OWN LINES.** `cq` on the composed line
        // is `Psk31MacroToken`'s stable name for which of §R2's four was composed - a token a
        // reader diagnoses from, which is why it is there (§R13) - and `cq_pressed` names a
        // control. **What may never appear is the text**, and the call to anyone is the hardest
        // case because it is the operator's own callsign three times over.
        foreach (var name in theSends.Append(TransmitRecord.EventName))
        {
            foreach (var e in Events(lines, name))
            {
                _output.WriteLine("swept " + name + ": " + e.GetRawText());

                // **THE WORDS AS THE AIR CARRIES THEM, CASE AND ALL.** Read without case a
                // three-letter word finds itself inside a field name - `capSeconds` holds `pSe` -
                // and a sweep that cries wolf is a sweep nobody reads.
                foreach (var word in new[] { "CQ", "pse", " de ", "K1ABC" })
                {
                    Assert.DoesNotContain(word, e.GetRawText(), StringComparison.Ordinal);
                }
            }
        }

        Assert.Equal("cq", Events(lines, "psk31_send_composed").First().GetProperty("macro").GetString());
    }

    /// <summary>
    /// **4.3: the CQ's center is the band's own Olivia calling center read from the cited table,
    /// on two bands, and it is a literal nowhere.**
    /// </summary>
    /// <param name="band">The band as Hamlet spells it.</param>
    /// <param name="dialHz">Where the dial is put, which is not the same distance below the
    /// calling center on the two bands, so a constant could not satisfy both.</param>
    /// <param name="wantedHz">The passband offset that puts the table's center on the air.</param>
    [Theory]
    [InlineData("20 m", 14_071_500L, 1500.0)]
    [InlineData("40 m", 7_072_000L, 1000.0)]
    public void TheCqGoesOutOnTheBandsOwnCallingCenter(string band, long dialHz, double wantedHz)
    {
        var table = OliviaData.Current.Calling!;
        var row = table.CallingRowFor(band)!;

        FakeSink sink;
        List<string> lines;

        using (var telemetry = new JsonlTelemetry(_folder, "366", _ => true))
        {
            var model = Listening(telemetry, dialHz);

            sink = Arm(model, telemetry).Sink;

            model.SendCallToAnyoneCommand.Execute(null);

            Settle(model);
        }

        lines = Lines();

        var (variant, centerHz) = ReadBack(sink);
        var composed = Assert.Single(Events(lines, "psk31_send_composed"));

        _output.WriteLine(
            $"{band}: the table calls on {row.CenterHz} Hz at {row.Variant}; the dial is {dialHz} Hz, "
            + $"so the offset is {row.CenterHz - (double)dialHz:0.0} Hz");
        _output.WriteLine($"{band}: composed at {composed.GetProperty("offsetHz").GetDouble():0.0} Hz, read back {variant} at {centerHz:0.00} Hz");

        Assert.Equal(row.CenterHz - (double)dialHz, wantedHz);
        Assert.Equal(Math.Round(wantedHz, 1), composed.GetProperty("offsetHz").GetDouble());
        Assert.Equal(OliviaCallingTable.CallingVariant, variant);
        Assert.InRange(centerHz, wantedHz - 5, wantedHz + 5);
    }

    /// <summary>
    /// **4.3: with somebody sitting on the calling center, the press finds a spot by
    /// `Psk31ClearSpot`'s rule or refuses in words, and keys nothing when it refuses.**
    /// </summary>
    /// <remarks>
    /// **THE RULE IS PSK31's AND ITS NUMBERS DO NOT MOVE** (decision BB). What is different under
    /// Olivia is only which carriers it is handed: the Olivia listener's, because the PSK31
    /// listener is not running on this tab.
    /// </remarks>
    [Fact]
    public void ACarrierOnTheCallingCenterMovesTheCallOrRefusesItInWords()
    {
        var table = OliviaData.Current.Calling!;
        var onTheSpot = table.CallingRowFor("20 m")!.CenterHz - (double)DialOn20m;

        FakeSink sink;
        string line;
        List<string> lines;

        using (var telemetry = new JsonlTelemetry(_folder, "366", _ => true))
        {
            var model = Listening(telemetry);

            sink = Arm(model, telemetry).Sink;

            // **A STATION ON THE CALLING SPOT, DRAWN THROUGH THE ROW PATH** the listener uses.
            model.ShowOliviaChannelsForTests(
            [
                new OliviaChannel(
                    91, OliviaCallingTable.CallingVariant, onTheSpot, OliviaListener.FoundByRsid, 0,
                    "CQ CQ CQ de N1XYZ N1XYZ N1XYZ pse K\n", 9, 0, false),
            ]);

            model.SendCallToAnyoneCommand.Execute(null);
            line = model.DigitalSendLine;

            Settle(model);
        }

        lines = Lines();

        var composed = Events(lines, "psk31_send_composed");
        var refused = Events(lines, "psk31_send_refused");

        _output.WriteLine($"somebody is on the calling spot at {onTheSpot:0.0} Hz");
        _output.WriteLine("send line: " + line);
        _output.WriteLine("rule     : " + Psk31ClearSpot.Rule);

        if (composed.Count > 0)
        {
            var wentTo = composed[0].GetProperty("offsetHz").GetDouble();
            var (variant, centerHz) = ReadBack(sink);

            _output.WriteLine($"it moved: composed at {wentTo:0.0} Hz, read back {variant} at {centerHz:0.00} Hz");

            // **IT MOVED, AND IT MOVED CLEAR OF HIM BY THE RULE'S OWN MARGIN.**
            Assert.True(
                Math.Abs(wentTo - onTheSpot) >= Psk31ClearSpot.ClearHz,
                $"the call went to {wentTo} Hz, inside {Psk31ClearSpot.ClearHz} Hz of a station at {onTheSpot} Hz");

            Assert.InRange(wentTo, Psk31ClearSpot.LowestCallHz, Psk31ClearSpot.HighestCallHz);
            Assert.Equal(OliviaCallingTable.CallingVariant, variant);
        }
        else
        {
            // **OR IT REFUSED IN WORDS AND KEYED NOTHING.**
            _output.WriteLine("it refused: " + Assert.Single(refused).GetRawText());

            Assert.Equal("no_clear_spot", refused[0].GetProperty("reason").GetString());
            Assert.Equal(0, sink.TimesCalled);
            Assert.Contains("too crowded", line, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// **4.3 and decision BC: the Olivia CQ's receipt is PSK31's - no station facts, no Log - and a
    /// certain answer retires it.**
    /// </summary>
    /// <remarks>
    /// **ASSERTED AS `TheCqReceiptTests` ASSERTS IT FOR PSK31**, on the same properties, because
    /// R28 says the receipt is the same receipt and the way to prove that is to make the same
    /// claims about it.
    /// </remarks>
    [Fact]
    public void TheCqReceiptCarriesNoStationFactsAndNoLogAndACertainAnswerRetiresIt()
    {
        using var telemetry = new JsonlTelemetry(_folder, "366", _ => true);

        var model = Listening(telemetry);

        Arm(model, telemetry);

        model.SendCallToAnyoneCommand.Execute(null);

        var receipt = Assert.Single(model.DigitalCards);

        _output.WriteLine("is a receipt : " + receipt.IsCallToAnyone);
        _output.WriteLine("callsign     : [" + receipt.Callsign + "]");
        _output.WriteLine("place        : [" + receipt.Place + "]");
        _output.WriteLine("state word   : [" + receipt.StateWord + "]");
        _output.WriteLine("sentence     : [" + receipt.Sentence + "]");
        _output.WriteLine("action label : [" + receipt.ActionLabel + "]");
        _output.WriteLine("shows globe  : " + receipt.ShowsGlobe);

        Assert.True(receipt.IsCallToAnyone);

        // **NO PLACE, NO COUNTRY, NO ENTITY, NO MAP ROW.**
        Assert.Empty(receipt.Place);
        Assert.False(receipt.HasPlace);
        Assert.False(receipt.ShowsGlobe);

        // **AND NO LOG.** Nothing has been worked, so there is nothing to log.
        Assert.Equal(Ft8CardActionKind.None, receipt.ActionKind);
        Assert.False(receipt.HasAction);
        Assert.Equal("", receipt.ActionLabel);

        var everything = string.Join(
            " | ",
            receipt.Callsign, receipt.Place, receipt.StateWord, receipt.Sentence,
            receipt.ActionLabel, receipt.ActionTip, receipt.Detail);

        foreach (var forbidden in new[] { "grid", "miles", "north", "south", "Portugal" })
        {
            Assert.DoesNotContain(forbidden, everything, StringComparison.OrdinalIgnoreCase);
        }

        Settle(model);

        // **A CERTAIN ANSWER, ADDRESSED TO HIM, ON AN OLIVIA ROW** - through the same certainty
        // gate PSK31's answer goes through, because it is the same code.
        model.ShowOliviaChannelsForTests(
        [
            new OliviaChannel(
                92, OliviaCallingTable.CallingVariant, 1500, OliviaListener.FoundByRsid, 0,
                Mine + " de N1XYZ N1XYZ K\n", 9, 0, false),
        ]);

        foreach (var card in model.DigitalCards)
        {
            _output.WriteLine($"after the answer: [{card.Callsign}] receipt {card.IsCallToAnyone}");
        }

        Assert.DoesNotContain(model.DigitalCards, c => c.IsCallToAnyone);
        Assert.Contains(model.DigitalCards, c => string.Equals(c.Callsign, "N1XYZ", StringComparison.Ordinal));
    }

    /// <summary>
    /// **4.4: the cap a send is held to is `timing.json`'s count times that variant's seconds per
    /// character, for a macro and for a typed line, and it is a literal nowhere.**
    /// </summary>
    /// <param name="variant">The variant.</param>
    [Theory]
    [InlineData("8/250")]
    [InlineData("16/500")]
    [InlineData("32/1000")]
    public void TheCapIsTheCountTimesTheVariantsSecondsPerCharacter(string variant)
    {
        var timing = OliviaData.Current.Timing!;
        var perCharacter = timing.SecondsPerCharacter[variant];

        foreach (var kind in new[] { OliviaSendKind.Macro, OliviaSendKind.TypedLine })
        {
            var count = kind == OliviaSendKind.TypedLine ? timing.CapTypedCharacters : timing.CapMacroCharacters;
            var composed = OliviaModulator.Compose("CQ de " + Mine, variant, 1500, 12000, 0.5f, kind);

            _output.WriteLine(
                $"{variant} {kind}: cap {composed.Cap:0.0000} s = {count} characters x {perCharacter:0.00000} s");

            // **THE PRODUCT, NOT A NUMBER** (§3.2). The count is the file's and the rate is the
            // variant's, and the cap the send is actually held to is what they make.
            Assert.Equal(count * perCharacter, composed.Cap, 9);
            Assert.NotEqual(OperatorSend.LongestUnslottedSeconds, composed.Cap);
        }
    }

    /// <summary>
    /// **4.4: PSK31's thirty seconds, the typed line's sixty, and the FT8 and FT4 caps have not
    /// moved.**
    /// </summary>
    /// <remarks>
    /// **THE CAP EXISTS SO A COMPOSING FAULT CANNOT LEAVE A CARRIER ON THE AIR**, and a unit that
    /// raised somebody else's to make its own fit would have bought Olivia's length with the other
    /// modes' exposure (decision BD).
    /// </remarks>
    [Fact]
    public void NoOtherModesCapMoved()
    {
        var psk31 = Psk31Modulator.Compose("CQ de " + Mine, 12000, 1500, 0.5f);

        _output.WriteLine($"OperatorSend.LongestUnslottedSeconds = {OperatorSend.LongestUnslottedSeconds}");
        _output.WriteLine($"MainWindowViewModel.LongestTypedSeconds = {MainWindowViewModel.LongestTypedSeconds}");
        _output.WriteLine($"a PSK31 macro is held to {psk31.Cap} s");
        _output.WriteLine($"FT8 slot {Ft8Slots.SlotSeconds} s, FT4 slot {Ft8Sharp.Ft4Timing.SlotSeconds} s");

        Assert.Equal(30, OperatorSend.LongestUnslottedSeconds);
        Assert.Equal(60, MainWindowViewModel.LongestTypedSeconds);
        Assert.Equal(30, psk31.Cap);
        Assert.Equal(15, Ft8Slots.SlotSeconds);
        Assert.Equal(7.5, Ft8Sharp.Ft4Timing.SlotSeconds);
    }

    /// <summary>
    /// **4.4: the longest Report the app composes, to a compound callsign, is armed and plays at
    /// 8/250 - the slow variant is allowed its length.**
    /// </summary>
    [Fact]
    public void TheLongestReportAt8250IsArmedAndPlays()
    {
        var report = Psk31Macros.Report(Compound, Mine, "Pat", "Boston MA", "FN42AA");

        FakeSink sink;
        List<string> lines;

        using (var telemetry = new JsonlTelemetry(_folder, "366", _ => true))
        {
            var model = Listening(telemetry);

            sink = Arm(model, telemetry).Sink;

            // **AT A ROW'S OWN VARIANT AND CENTER**, which is where a Report goes (decision BA).
            model.ShowOliviaChannelsForTests(
            [
                new OliviaChannel(
                    93, "8/250", 1500, OliviaListener.FoundByRsid, 0,
                    Mine + " de " + Compound + " " + Compound + " K\n", 9, 0, false),
            ]);

            foreach (var r in model.DigitalDecodes)
            {
                _output.WriteLine($"row [{r.Variant}] at {r.Hz} Hz, sender [{r.Sender}]: {r.Message}");
            }

            _output.WriteLine($"the longest Report is {report.Length} characters: \"{report}\"");

            // **THROUGH THE ONE DOOR** (§0.2, §R10). `SendMessage` is the application's single send
            // entry point and the card's Report button reaches it by calling it; this hands it the
            // same text, so the mode gate, the variant, the composer, the cap, the licence gate and
            // the one keying site all apply exactly as they do to a press.
            //
            // **WHY NOT THE CARD'S OWN BUTTON.** No card opens for this station: the PSK31 exchange
            // parser reads no speaker from a line whose callsign carries a slash, so the row above
            // shows an empty sender and no conversation is opened. That is a finding about the
            // shared parser and it is reported rather than repaired here (§R14).
            model.SendMessageCommand.Execute(report);

            Settle(model);
        }

        lines = Lines();

        var composed = Assert.Single(Events(lines, "psk31_send_composed"));
        var record = Assert.Single(Events(lines, TransmitRecord.EventName));
        var seconds = composed.GetProperty("seconds").GetDouble();
        var cap = composed.GetProperty("capSeconds").GetDouble();

        _output.WriteLine($"composed {composed.GetProperty("characters").GetInt32()} characters, {seconds:0.00} s against a cap of {cap:0.00} s");
        _output.WriteLine("record   : " + record.GetRawText());

        Assert.Equal(report.Length, composed.GetProperty("characters").GetInt32());
        Assert.Equal("8/250", composed.GetProperty("variant").GetString());
        Assert.True(composed.GetProperty("withinCap").GetBoolean(), "the Report did not fit the 8/250 cap");
        Assert.True(seconds > OperatorSend.LongestUnslottedSeconds, "the Report is shorter than PSK31's own cap, so this proves nothing");
        Assert.Equal("Played", record.GetProperty("outcome").GetString());
        Assert.True(record.GetProperty("keyed").GetBoolean());
        Assert.Equal(1, sink.TimesCalled);
    }

    /// <summary>
    /// **4.4: a text one character past what fits at 8/250 is refused as `LongerThanTheCap`, in
    /// PSK31's own words, and keys nothing.**
    /// </summary>
    [Fact]
    public void OneCharacterPastWhatFitsAt8250IsRefusedAndKeysNothing()
    {
        // **MEASURED, NOT ASSUMED** (decision BD). The count in `timing.json` is a bound on the
        // text and not a promise that a text of that length fits, because the air sends whole
        // blocks; what fits is found by asking the composer.
        var fits = 0;

        for (var characters = 1; characters <= 512; characters++)
        {
            if (OliviaModulator.Compose(new string('A', characters), "8/250", 1500, 12000, 0.5f).Fit == UnslottedFit.Fits)
            {
                fits = characters;
            }
        }

        var overTheCap = OliviaModulator.Compose(new string('A', fits + 1), "8/250", 1500, 12000, 0.5f);
        var atTheEdge = OliviaModulator.Compose(new string('A', fits), "8/250", 1500, 12000, 0.5f);

        _output.WriteLine($"the longest macro that fits at 8/250 is {fits} characters, {atTheEdge.TextSeconds:0.00} s against a cap of {atTheEdge.Cap:0.00} s");
        _output.WriteLine($"one more is {overTheCap.TextSeconds:0.00} s, {overTheCap.Fit}");

        Assert.Equal(UnslottedFit.Fits, atTheEdge.Fit);
        Assert.Equal(UnslottedFit.LongerThanTheCap, overTheCap.Fit);

        // **AND THE REFUSAL AT THE PRESS IS PSK31's OWN** - the same `Arm`, the same reason token,
        // the same sentence shape - with nothing keyed.
        FakeSink sink;
        FakePort port;
        string line;

        using (var telemetry = new JsonlTelemetry(_folder, "366", _ => true))
        {
            var model = Listening(telemetry);
            var parts = Arm(model, telemetry);

            sink = parts.Sink;
            port = parts.Port;

            model.ShowOliviaChannelsForTests(
            [
                new OliviaChannel(
                    94, "8/250", 1500, OliviaListener.FoundByRsid, 0,
                    Mine + " de N1XYZ N1XYZ K\n", 9, 0, false),
            ]);

            var card = Assert.Single(model.DigitalCards, c => string.Equals(c.Callsign, "N1XYZ", StringComparison.Ordinal));

            // A typed line long enough that its framed text is past what fits at 8/250.
            card.TypedText = new string('A', 400);

            _output.WriteLine("the card says: " + card.TypedSecondsWord);

            model.SendTypedPsk31Command.Execute(card);
            line = model.DigitalSendLine;

            Settle(model);
        }

        var lines = Lines();
        var refused = Assert.Single(Events(lines, "psk31_send_refused"));

        _output.WriteLine("send line: " + line);
        _output.WriteLine("refused  : " + refused.GetRawText());

        Assert.Equal("cap", refused.GetProperty("reason").GetString());
        Assert.Equal("arm", refused.GetProperty("stage").GetString());
        Assert.Equal("olivia", refused.GetProperty("mode").GetString());
        // **NOTHING KEYED AND NOTHING PLAYED.** The sequence still writes down what it refused,
        // which is §8.1's own rule - a send that produced silence is the case somebody has to
        // diagnose - and every such record says the transmitter was never keyed.
        foreach (var e in Events(lines, TransmitRecord.EventName))
        {
            _output.WriteLine("record   : " + e.GetRawText());

            Assert.False(e.GetProperty("keyed").GetBoolean());
        }

        Assert.Equal(0, sink.TimesCalled);
        Assert.Empty(port.Written);
    }

    /// <summary>
    /// **4.4: the typed line's card states the seconds for the Olivia variant it would go at, not
    /// for PSK31.**
    /// </summary>
    [Fact]
    public void TheTypedLinesCardStatesTheSecondsForItsOwnVariant()
    {
        var model = Listening(null);

        model.ShowOliviaChannelsForTests(
        [
            new OliviaChannel(95, "8/250", 1500, OliviaListener.FoundByRsid, 0, Mine + " de N1XYZ N1XYZ K\n", 9, 0, false),
        ]);

        var card = Assert.Single(model.DigitalCards, c => string.Equals(c.Callsign, "N1XYZ", StringComparison.Ordinal));

        Assert.Equal("8/250", card.OliviaVariant);
        Assert.True(card.IsOlivia);

        const string typed = "tnx fer the call and the report";

        card.TypedText = typed;

        var framed = Psk31Macros.Typed(card.Callsign, Mine, typed);
        var atPsk31 = Psk31Macros.TypedSeconds(card.Callsign, Mine, typed);
        var atOlivia = OliviaModulator.TextSeconds("8/250", framed.Length);

        _output.WriteLine($"typed: \"{typed}\", framed to {framed.Length} characters");
        _output.WriteLine($"the same line would be {atPsk31:0.0} s at PSK31's rate and {atOlivia:0.0} s at 8/250");
        _output.WriteLine($"the card says: \"{card.TypedSecondsWord}\"");

        Assert.Equal(
            atOlivia.ToString("0.#", CultureInfo.InvariantCulture) + " s of text",
            card.TypedSecondsWord);

        Assert.DoesNotContain(
            atPsk31.ToString("0.#", CultureInfo.InvariantCulture) + " s", card.TypedSecondsWord, StringComparison.Ordinal);
    }

    /// <summary>
    /// **4.4, the patience half (decision BE): where a turn is decided, and what a time-based
    /// patience would be if anything consumed one.**
    /// </summary>
    /// <remarks>
    /// **NOTHING IS INVENTED TO HAVE SOMETHING TO SCALE** (§R14). `Psk31Turn.Read` decides every
    /// turn in this application from the channel's finished messages and its carrier-present fact,
    /// and it reads no clock at all; `Psk31Macros.AnswerSeconds`, the only patience the tree
    /// states, has no caller in `src/`. So what this asserts is that the table's patience scales
    /// with the variant and is a figure in seconds nowhere, and where the turn is actually decided
    /// is named here rather than a timer being added so that a criterion has something to point at.
    /// </remarks>
    [Fact]
    public void ThePatienceScalesWithTheVariantAndTheTurnIsDecidedWithoutAClock()
    {
        var timing = OliviaData.Current.Timing!;

        _output.WriteLine("the turn is decided at MainWindowViewModel.ShowPsk31Cards, by Psk31Turn.Read");
        _output.WriteLine("Psk31Turn.Read takes: the channel's finished messages, whether characters have");
        _output.WriteLine("arrived since the last of them, and the operator's callsign. No time at all.");
        _output.WriteLine("rule: " + Psk31Turn.Rule);

        foreach (var variant in new[] { "8/250", "16/500", "32/1000" })
        {
            var perCharacter = timing.SecondsPerCharacter[variant];

            _output.WriteLine(
                $"{variant}: patience {timing.PatienceSeconds(variant):0.0000} s = {timing.PatienceCharacters} characters x {perCharacter:0.00000} s");

            Assert.Equal(timing.PatienceCharacters * perCharacter, timing.PatienceSeconds(variant), 9);
        }

        // **EACH VARIANT'S IS ITS OWN**, which is what makes it scaled rather than fixed.
        Assert.NotEqual(timing.PatienceSeconds("8/250"), timing.PatienceSeconds("16/500"));
        Assert.NotEqual(timing.PatienceSeconds("16/500"), timing.PatienceSeconds("32/1000"));

        // **AND NO APPLICATION PATH CONSUMES A TIME-BASED PATIENCE**, which is decision BE's
        // finding rather than a gap to fill: `AnswerSeconds` is called by tests alone.
        var src = Path.Combine(Root(), "src");

        Assert.DoesNotContain(Sites(src, "AnswerSeconds("), s => !s.Contains("public static double", StringComparison.Ordinal));
    }

    /// <summary>Press something on a listening, armed panel and give back what it left behind.</summary>
    private (FakeSink Sink, FakePort Port, List<string> Lines) Pressed(Action<MainWindowViewModel> press)
    {
        FakeSink sink;
        FakePort port;

        using (var telemetry = new JsonlTelemetry(_folder, "366", _ => true))
        {
            var model = Listening(telemetry);
            var parts = Arm(model, telemetry);

            sink = parts.Sink;
            port = parts.Port;

            press(model);

            Settle(model);
        }

        return (sink, port, Lines());
    }

    /// <summary>What the detector makes of the audio the sound card was handed, told nothing.</summary>
    private (string Variant, double CenterHz) ReadBack(FakeSink sink)
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

        return (variant!, detection.CenterHz);
    }

    /// <summary>A panel on 20 m with the Olivia tab pressed and the tap running.</summary>
    private static MainWindowViewModel Listening(JsonlTelemetry? telemetry)
        => Listening(telemetry, DialOn20m);

    /// <summary>A panel at a named dial with the Olivia tab pressed and the tap running.</summary>
    private static MainWindowViewModel Listening(JsonlTelemetry? telemetry, long dialHz)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN42";
        settings.Operator.OperatorName = "Pat";
        settings.Operator.Location = "Boston MA";

        // **THE LICENCE GATE IS INSIDE THE PATH AND IT IS NOT BYPASSED HERE** (§0.2). 14.0715 MHz
        // is inside a General class data segment, so the gate permits it and the send is measured
        // rather than refused; nothing about the gate is loosened for Olivia.
        settings.Operator.LicenseClass = Hamlet.RadioEngine.Licensing.LicenseClass.General;

        var model = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
            TapForTests = new AudioTap(),
        };

        model.SelectedBand = model.Bands.First(b => b.Band.LowHz <= dialHz && b.Band.HighHz >= dialHz);
        model.FrequencyHz = dialHz;
        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.ChooseDigitalModeCommand.Execute(Mode);

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

    /// <summary>Hand a recording and its tail to the tick in quarter-second pieces, as a sound card would.</summary>
    private static void Feed(MainWindowViewModel model, MonoAudio audio)
    {
        var rate = audio.SampleRate;
        var tail = Noise((int)(TailBlocks * Format.SymbolsPerBlock * Format.Variants.Max(v => v.SymbolSeconds) * rate), TailRms);
        var all = audio.Samples.Concat(tail).ToArray();
        var piece = rate / 4;

        for (var at = 0; at < all.Length; at += piece)
        {
            model.TapForTests!.Take(all.AsSpan(at, Math.Min(piece, all.Length - at)), rate);
            model.LookForASlotForTests();
        }
    }

    /// <summary>Seeded white Gaussian noise, the tail unit 364 feeds after a file.</summary>
    private static float[] Noise(int count, double rms)
    {
        var random = new Random(364);
        var samples = new float[count];

        for (var i = 0; i < count; i++)
        {
            var u1 = 1.0 - random.NextDouble();
            var u2 = random.NextDouble();

            samples[i] = (float)(rms * Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2));
        }

        return samples;
    }

    /// <summary>A fixture from the mode author's set, after its hash has been checked against the manifest.</summary>
    private static MonoAudio Fixture(string file)
    {
        var folder = Path.Combine(Root(), "assets", "fixtures", "olivia");

        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(folder, "manifest.json")));

        var entry = manifest.RootElement.EnumerateArray().Single(e => e.GetProperty("file").GetString() == file);
        var path = Path.Combine(folder, file);

        Assert.Equal(
            entry.GetProperty("sha256").GetString(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant());

        return WavAudio.Read(path);
    }

    private List<string> Lines()
        => Directory.GetFiles(_folder, "*.jsonl").SelectMany(File.ReadAllLines).ToList();

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

    /// <summary>Where a row says its carrier sits, which is what the listener measured.</summary>
    private static double Hz(DigitalDecodeRow row)
        => double.TryParse(row.Hz, NumberStyles.Float, CultureInfo.InvariantCulture, out var hz) ? hz : double.NaN;

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

    /// <summary>Every code line of `src/` holding this text, a doc comment not being one.</summary>
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
                    && lines[i].Contains(needle, StringComparison.Ordinal))
                {
                    found.Add($"{Path.GetRelativePath(src, path)}:{i + 1}: {trimmed}");
                }
            }
        }

        return found;
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
