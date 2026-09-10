using System.Globalization;
using System.Text.Json;
using Avalonia.Headless.XUnit;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 264, task 2: **a whole contact walks through the application
/// - two clicks, two transmissions, the ledger, the row and the telemetry file -
/// and the row ends up reading "complete".**
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THIS WOULD HAVE CAUGHT.** Tim completes a QSO on the air
/// and the row never says `complete`, or the slots he transmitted are not in the
/// telemetry file - so step 6's second criterion fails **after the one event in
/// this phase that cannot be repeated.** A contact cannot be made again to
/// collect the evidence it should have left behind.</para>
/// <para>**EVERY PART OF THIS CHAIN EXISTED AND EACH WAS PROVED ALONE. NOTHING
/// HAD EVER JOINED THEM** (`docs/unit264-whole-contact-trace.md`, question 7).
/// The nearest existing test drives two boundaries against **one** arming and
/// never reads a row's contact cell; the nearest that sends twice never places a
/// row afterwards. So whether a completed exchange actually reads `complete` on
/// the table was unknown rather than proved.</para>
/// <para>**THE OPERATOR'S TWO MESSAGES GO THROUGH THE MENU AND THE ARMING.**
/// <c>SendMenuFor</c> is asked what is valid, the text is taken **off the menu
/// option**, and it goes out through <c>SendMessageCommand</c> and
/// <c>AtSlotBoundaryAsync</c>. Nothing here calls <c>RecordSent</c> by hand: the
/// subject is the join, and a test that books the send itself proves nothing
/// about the application.</para>
/// <para>**NOTHING IS OPENED, NOTHING IS KEYED, AND NO SOUND IS MADE**
/// (`SHACK_FACTS.md` FACT-004, work instruction 264 *What not to do* 1). The port
/// is <see cref="FakePort"/> and the sink arrives through the substituted factory
/// units 260 and 262 left on the view model. **Every figure below was measured on
/// the development machine, which has never had a radio attached to it**, and
/// none of it says anything about the IC-7300.</para>
/// <para>**THE SENDS BOOK AT THE SLOT THE CLOCK OFFERED, NOT AT ONE THIS TEST
/// CHOSE.** <c>MainWindowViewModel.cs:8174</c> reads the real clock at the click
/// and <c>:8279</c> books <c>result.Send!.SlotStartUtc</c>. So the armed slot is
/// **recorded** from <c>ArmedForSlotUtc</c> and telemetry is asserted against
/// that, rather than against a number invented here.</para>
/// </remarks>
public sealed class TheWholeContactWalksThroughTheApplicationTests : IDisposable
{
    /// <summary>An endpoint id of the shape Windows uses. Never opened.</summary>
    private const string NamedEndpoint = "{0.0.0.00000000}.{a-render-endpoint}";

    /// <summary>FT8's watering hole on 20 m, where a General may send data.</summary>
    private const long Ft8On20m = 14_074_000;

    /// <summary>The operator.</summary>
    private const string Mine = "KC3QIS";

    /// <summary>The station he works.</summary>
    private const string His = "W1ABC";

    /// <summary>What he measured W1ABC at, and therefore what he reports.</summary>
    private const string MeasuredHim = "-11";

    private readonly string _folder = Path.Combine(
        Path.GetTempPath(), "hamlet-unit264-" + Guid.NewGuid().ToString("N")[..8]);

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where every measured figure is printed.</param>
    public TheWholeContactWalksThroughTheApplicationTests(ITestOutputHelper output)
        => _output = output;

    /// <inheritdoc/>
    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (Exception)
        {
            // Test cleanup only.
        }
    }

    /// <summary>
    /// **The whole contact, end to end, through the application's own path.**
    /// </summary>
    /// <remarks>
    /// <para>The exchange, five slots, in the order a real one runs in:</para>
    /// <list type="table">
    /// <item><description>0 - `CQ W1ABC EM12`, heard</description></item>
    /// <item><description>1 - `W1ABC KC3QIS FN00`, the operator, clicked</description></item>
    /// <item><description>2 - `KC3QIS W1ABC -09`, heard</description></item>
    /// <item><description>3 - `W1ABC KC3QIS R-11`, the operator, clicked</description></item>
    /// <item><description>4 - `KC3QIS W1ABC RR73`, heard</description></item>
    /// </list>
    /// <para>**THE ROW READ IS THE NEWEST ONE FOR THE STATION**, which is the one
    /// at the top of the table under newest-first ordering and therefore the one
    /// the operator is looking at. It is read off
    /// <see cref="DigitalDecodeRow.Contact"/>, the way the UI reads it, and not
    /// off the ledger.</para>
    /// </remarks>
    [Fact]
    public async Task AWholeContactWalksThroughAndTheRowReadsComplete()
    {
        var (panel, settings, factory, telemetry) = Panel();
        var port = new FakePort();

        settings.AudioOutputDeviceId = NamedEndpoint;
        panel.BuildTheArmedSend(port);

        Assert.True(panel.HasSomethingToTransmitThrough, panel.DigitalSendLine);

        // The slot the clock is really in, so the heard slots and the
        // clock-driven armed slots belong to the same evening.
        var slot0 = Ft8Slots.SlotStart(DateTime.UtcNow);

        // SLOT 0 - he calls CQ.
        var cq = Heard(panel, slot0, "-14", "CQ " + His + " EM12");

        // SLOT 1 - the operator answers, through the menu.
        var first = await ClickAsync(panel, cq, Ft8SendShape.Grid);

        // **AND THEN THE CLOCK IS ALLOWED TO REACH SLOT 2**, rather than the two
        // clicks landing in one fifteen-second slot and booking one slot time
        // twice. This is what makes assertion 4 say what step 6's criterion 2
        // says - *the transmitted slotS* - instead of one slot counted twice.
        var slot2 = await WaitForSlotAsync(slot0.AddSeconds(30));

        // SLOT 2 - he comes back with a report.
        var report = Heard(panel, slot2, MeasuredHim, Mine + " " + His + " -09");

        // SLOT 3 - the operator rogers it and reports back, through the menu.
        var second = await ClickAsync(panel, report, Ft8SendShape.RogerAndReport);

        Assert.True(
            second.SlotUtc > first.SlotUtc,
            "the two sends booked into the same slot: " + Stamp(first.SlotUtc));

        // SLOT 4 - he signs off. **This is the row placed after both sends**, and
        // by the trace's question 2 it is the only kind of row that can move.
        Heard(panel, second.SlotUtc.AddSeconds(15), MeasuredHim, Mine + " " + His + " RR73");

        telemetry.Dispose();

        // ---- 1. BOTH BOUNDARIES RAN AND BOTH REPORTED THE TRANSMISSION SENT.
        _output.WriteLine("ASSERTION 1 - the two boundaries");
        _output.WriteLine("  first  : " + first.Result.Outcome
            + " / sent=" + first.Result.Run?.AudioWentOut + " / slot=" + Stamp(first.SlotUtc));
        _output.WriteLine("  second : " + second.Result.Outcome
            + " / sent=" + second.Result.Run?.AudioWentOut + " / slot=" + Stamp(second.SlotUtc));
        _output.WriteLine("  sink calls  : " + factory.Sink.TimesCalled);
        _output.WriteLine("  port frames : " + port.Written.Count);

        Assert.Equal(Ft8ArmOutcome.Ran, first.Result.Outcome);
        Assert.Equal(Ft8ArmOutcome.Ran, second.Result.Outcome);
        Assert.True(first.Result.Run!.AudioWentOut, first.Result.Run.Reason);
        Assert.True(second.Result.Run!.AudioWentOut, second.Result.Run.Reason);

        // ---- 2. THE LEDGER HOLDS TWO SENT AND THREE HEARD AGAINST W1ABC.
        var record = panel.ContactRecordForTests(His);

        Assert.NotNull(record);

        _output.WriteLine("ASSERTION 2 - the ledger the application kept");
        _output.WriteLine("  sent  : " + string.Join(" | ", record!.Sent.Select(m => m.Message)));
        _output.WriteLine("  heard : " + string.Join(" | ", record.Heard.Select(m => m.Message)));
        _output.WriteLine("  heard to us : "
            + string.Join(" | ", record.HeardToUs.Select(m => m.Message)));

        Assert.Equal(2, record.Sent.Count);
        Assert.Equal(3, record.Heard.Count);

        // ---- 3. THE ROW FOR W1ABC READS "complete".
        var rows = panel.DigitalDecodes.Where(r => r.Sender == His).ToList();

        _output.WriteLine("ASSERTION 3 - what the table says, newest first");

        foreach (var row in rows)
        {
            _output.WriteLine("  " + Stamp(row.SlotStartUtc) + "  \"" + row.Message
                + "\"  contact: \"" + row.Contact + "\"");
        }

        Assert.Equal(3, rows.Count);

        // The top of the table under newest-first ordering: the row the operator
        // is looking at when the contact ends.
        var newest = rows[0];

        Assert.Contains(
            "complete",
            newest.Contact,
            StringComparison.Ordinal);

        // ---- 4. BOTH TRANSMITTED SLOTS APPEAR IN TELEMETRY.
        var lines = TransmitLines();

        _output.WriteLine("ASSERTION 4 - the application's own telemetry file");
        _output.WriteLine("  folder : " + _folder);

        foreach (var line in lines)
        {
            _output.WriteLine("  " + line);
        }

        Assert.Equal(2, lines.Count);

        var slotsOnDisk = lines
            .Select(line => JsonDocument.Parse(line).RootElement
                .GetProperty("data").GetProperty("slotStartUtc").GetString())
            .ToList();

        _output.WriteLine("  slots armed  : "
            + Stamp(first.SlotUtc) + ", " + Stamp(second.SlotUtc));
        _output.WriteLine("  slots on disk: " + string.Join(", ", slotsOnDisk));

        Assert.Contains(Iso(first.SlotUtc), slotsOnDisk);
        Assert.Contains(Iso(second.SlotUtc), slotsOnDisk);
    }

    /// <summary>
    /// **The transmitted slot, read back out of the application's own file - and
    /// it names nobody.**
    /// </summary>
    /// <remarks>
    /// <para>Work instruction 264, task 4. **The breakage this would have caught:**
    /// the transmit record is written to a telemetry instance the application
    /// never gives the sequence, or to a category the operator's settings switch
    /// off, so the file Tim sends back after his contact has nothing in it.</para>
    /// <para>**THE WRITER IS THE APPLICATION'S**, built by <see cref="Panel"/>
    /// with the four arguments <c>App.axaml.cs:38-43</c> passes, including the
    /// enabled-category predicate off a real <see cref="AppSettings"/> and the
    /// byte cap - **not** the `_ => true` the engine's own
    /// <c>WhereTheTransmissionStartsAndWhatTheRecordSaysTests</c> uses, which
    /// cannot refuse and therefore says nothing about the operator's switches
    /// (`docs/unit264-whole-contact-trace.md`, question 5).</para>
    /// <para>**AND IT ASSERTS HM-DEC-018 RATHER THAN TRUSTING IT.** The raw text
    /// of the line is searched for both callsigns, the grid and the whole message,
    /// case-insensitively, and each must be absent. <see cref="TransmitRecord"/>'s
    /// own rule is that the record carries when a slot went out, where, how long
    /// for and how the radio came out of transmit - **never message content and
    /// never a callsign** - and a rule nobody has watched hold is not a rule.</para>
    /// <para>**FACT-004. Every figure below was measured on the development
    /// machine**, which has never had a radio attached to it. Nothing was opened,
    /// nothing was keyed and no sound was made: the port is a fake and the sink
    /// arrives through the substituted factory. **None of it says anything about
    /// the IC-7300's USB codec.**</para>
    /// </remarks>
    [Fact]
    public async Task OneSendLeavesOneLineOnDiskAndTheLineNamesNobody()
    {
        var (panel, settings, _, telemetry) = Panel();
        var port = new FakePort();

        settings.AudioOutputDeviceId = NamedEndpoint;
        panel.BuildTheArmedSend(port);

        Assert.True(panel.HasSomethingToTransmitThrough, panel.DigitalSendLine);

        var cq = Heard(
            panel, Ft8Slots.SlotStart(DateTime.UtcNow), "-14", "CQ " + His + " EM12");

        var sent = await ClickAsync(panel, cq, Ft8SendShape.Grid);

        Assert.Equal(Ft8ArmOutcome.Ran, sent.Result.Outcome);
        Assert.True(sent.Result.Run!.AudioWentOut, sent.Result.Run.Reason);

        telemetry.Dispose();

        // ---- 1. THE LINE IS ON DISK, FOUND BY ITS EVENT NAME.
        var lines = TransmitLines();

        _output.WriteLine("MEASURED, development machine, FACT-004:");
        _output.WriteLine("  folder : " + _folder);
        _output.WriteLine("  files  : " + string.Join(", ",
            Directory.GetFiles(_folder, "*.jsonl").Select(Path.GetFileName)));

        var line = Assert.Single(lines);

        _output.WriteLine("  line   : " + line);

        using var doc = JsonDocument.Parse(line);
        var root = doc.RootElement;

        Assert.Equal(TransmitRecord.EventName, root.GetProperty("event").GetString());
        Assert.Equal("transmit", root.GetProperty("category").GetString());

        // ---- 2. THE SLOT TIME AND THE DURATION ARE THE ONES THAT WENT OUT.
        var data = root.GetProperty("data");

        var slotOnDisk = data.GetProperty("slotStartUtc").GetString();
        var secondsOnDisk = data.GetProperty("durationSeconds").GetDouble();
        var samplesOnDisk = data.GetProperty("sampleCount").GetInt32();
        var rateOnDisk = data.GetProperty("sampleRate").GetInt32();

        _output.WriteLine("  slot armed     : " + Iso(sent.SlotUtc));
        _output.WriteLine("  slot on disk   : " + slotOnDisk);
        _output.WriteLine("  seconds offered: " + sent.Result.Run.SecondsOffered);
        _output.WriteLine("  seconds on disk: " + secondsOnDisk);
        _output.WriteLine("  samples offered: " + sent.Result.Run.SamplesOffered);
        _output.WriteLine("  samples on disk: " + samplesOnDisk);
        _output.WriteLine("  rate on disk   : " + rateOnDisk);

        Assert.Equal(Iso(sent.SlotUtc), slotOnDisk);
        Assert.Equal(sent.Result.Run.SecondsOffered, secondsOnDisk, 6);
        Assert.Equal(sent.Result.Run.SamplesOffered, samplesOnDisk);

        // ---- 3. IT NAMES NOBODY AND REPEATS NOTHING. HM-DEC-018.
        string[] mustNotAppear =
        [
            Mine, His, "FN00", "EM12", His + " " + Mine + " FN00", "CQ " + His + " EM12",
        ];

        foreach (var forbidden in mustNotAppear)
        {
            _output.WriteLine("  absent \"" + forbidden + "\": "
                + !line.Contains(forbidden, StringComparison.OrdinalIgnoreCase));

            Assert.DoesNotContain(forbidden, line, StringComparison.OrdinalIgnoreCase);
        }

        // AND THE ONE THING IT DOES SAY ABOUT THE MESSAGE IS A COUNT, WHICH THE
        // RULING NAMES EXPLICITLY.
        Assert.Equal(
            (His + " " + Mine + " FN00").Length,
            data.GetProperty("messageLength").GetInt32());
    }

    /// <summary>
    /// **The ending nobody has driven: the operator's own last message is what
    /// completes the exchange, and then the station goes quiet.**
    /// </summary>
    /// <remarks>
    /// <para>Work instruction 270, task 2. **This is a measurement and not a
    /// red.** It is added to this file rather than copied into a new one because
    /// every part of the harness it needs - <c>Panel()</c>, <c>Heard</c>,
    /// <c>ClickAsync</c>, <c>WaitForSlotAsync</c> - is already here and private,
    /// and a second copy of a harness is a second thing to drift. **Not one line
    /// of any existing method in this file was changed to make room for it**; the
    /// only other edit is the <c>Avalonia.Headless.XUnit</c> using above.</para>
    /// <para>**WHY IT IS AN `[AvaloniaFact]` WHERE THE WALK IS A `[Fact]`.** It
    /// reads <c>DigitalSendLine</c> and <c>DigitalTransmitLevelLine</c>, which
    /// <c>AtSlotBoundaryAsync</c> sets inside a
    /// <c>Dispatcher.UIThread.Post</c> (`MainWindowViewModel.cs:8487-8491`). With
    /// no dispatcher running, that post never executes and the two lines quoted
    /// below would be the ones set at the click - which is a quotation of
    /// something the operator never sees.</para>
    /// <para>The exchange, four slots, and then silence:</para>
    /// <list type="table">
    /// <item><description>0 - `CQ W1ABC EM12`, heard</description></item>
    /// <item><description>1 - `W1ABC KC3QIS FN00`, the operator, clicked</description></item>
    /// <item><description>2 - `KC3QIS W1ABC R-09`, heard - **a report and a
    /// roger in one field**</description></item>
    /// <item><description>3 - `W1ABC KC3QIS RRR`, the operator, clicked -
    /// **his own message is what completes the exchange**</description></item>
    /// <item><description>4 - **nothing.** The station has gone.</description></item>
    /// </list>
    /// <para>**THE BREAKAGE THIS WOULD HAVE CAUGHT: A FUTURE UNIT "FIXING" THE
    /// STALE CELL BY REWRITING ROWS ALREADY ON THE TABLE.** The obvious repair
    /// for what this measures is to walk <c>DigitalDecodes</c> after a send and
    /// recompute every <c>Contact</c> cell. That would make a table of moments
    /// lie about its own moments: <c>ContactTextFor</c>'s own contract
    /// (`MainWindowViewModel.cs:7885-7888`) is that a row shows where the contact
    /// stood **in its own slot** and never restates itself, which is what lets a
    /// reader watch a contact progress down the table. **So the stale cell
    /// asserted here is not a defect to be repaired and these assertions must go
    /// on passing unchanged.** The present state belongs in a line about the
    /// present, which is work instruction 270 task 3's subject and is asserted
    /// somewhere else.</para>
    /// <para>**NOTHING IS OPENED, NOTHING IS KEYED, AND NO SOUND IS MADE**
    /// (`SHACK_FACTS.md` FACT-004). The port is <see cref="FakePort"/> and the
    /// sink arrives through the substituted factory. Every figure printed below
    /// was measured on the development machine, which has never had a radio
    /// attached to it, and none of it says anything about the IC-7300.</para>
    /// </remarks>
    [AvaloniaFact]
    public async Task AfterHisOwnLastMessageTheLedgerIsCompleteAndTheNewestRowIsNot()
    {
        var (panel, settings, _, telemetry) = Panel();
        var port = new FakePort();

        settings.AudioOutputDeviceId = NamedEndpoint;
        panel.BuildTheArmedSend(port);

        Assert.True(panel.HasSomethingToTransmitThrough, panel.DigitalSendLine);

        var slot0 = Ft8Slots.SlotStart(DateTime.UtcNow);

        // SLOT 0 - he calls CQ.
        var cq = Heard(panel, slot0, "-14", "CQ " + His + " EM12");

        // SLOT 1 - the operator answers a CQ with his grid.
        var first = await ClickAsync(panel, cq, Ft8SendShape.Grid);

        // The real clock, for the same reason the walk above waits for it: two
        // clicks in one fifteen-second slot book one slot time twice.
        var slot2 = await WaitForSlotAsync(slot0.AddSeconds(30));

        // SLOT 2 - **a report and a roger in one field**, which
        // `Ft8ContactState.cs:182-184` says counts as both.
        var report = Heard(panel, slot2, MeasuredHim, Mine + " " + His + " R-09");

        // SLOT 3 - **the operator's own RRR, which is the message that satisfies
        // `IsComplete`.** Nothing is heard after it.
        var second = await ClickAsync(panel, report, Ft8SendShape.Acknowledge);

        Assert.True(
            second.SlotUtc > first.SlotUtc,
            "the two sends booked into the same slot: " + Stamp(first.SlotUtc));

        // SLOT 4 - NOTHING IS HEARD. He got what he came for and moved on.

        telemetry.Dispose();

        // The posted lines, so what is quoted is what the operator would read.
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.Equal(Ft8ArmOutcome.Ran, first.Result.Outcome);
        Assert.Equal(Ft8ArmOutcome.Ran, second.Result.Outcome);
        Assert.True(first.Result.Run!.AudioWentOut, first.Result.Run.Reason);
        Assert.True(second.Result.Run!.AudioWentOut, second.Result.Run.Reason);

        // ---- 1. THE LEDGER THE APPLICATION KEPT SAYS THE CONTACT IS COMPLETE.
        var record = panel.ContactRecordForTests(His);

        Assert.NotNull(record);

        var complete = Ft8ContactStates.IsComplete(record!);
        var read = Ft8ContactStates.Read(record!, second.SlotUtc, SlotGrid.Ft8);

        _output.WriteLine("MEASURED, development machine, FACT-004:");
        _output.WriteLine("MEASUREMENT 1 - the ledger, after his own last message");
        _output.WriteLine("  sent  : " + string.Join(" | ", record!.Sent.Select(m => m.Message)));
        _output.WriteLine("  heard : " + string.Join(" | ", record.Heard.Select(m => m.Message)));
        _output.WriteLine("  IsComplete             : " + complete);
        _output.WriteLine("  Read at " + Stamp(second.SlotUtc) + ", his send slot : \""
            + read.Text + "\"");

        Assert.True(
            complete,
            "the exchange does not satisfy IsComplete, so this test is not "
            + "measuring the ending it was written for.");

        Assert.Equal(Ft8ContactState.Complete, read.State);

        // ---- 2. AND THE NEWEST ROW ON THE TABLE DOES NOT.
        var rows = panel.DigitalDecodes.Where(r => r.Sender == His).ToList();

        _output.WriteLine("MEASUREMENT 2 - what the table says, newest first");

        foreach (var row in rows)
        {
            _output.WriteLine("  " + Stamp(row.SlotStartUtc) + "  \"" + row.Message
                + "\"  contact: \"" + row.Contact + "\"");
        }

        // Two rows and no more: nothing was heard after his last transmission,
        // so no row was placed after it.
        Assert.Equal(2, rows.Count);

        var newest = rows[0];

        Assert.Equal(Mine + " " + His + " R-09", newest.Message);
        Assert.Equal(slot2, newest.SlotStartUtc);

        // **THE ROW IS A RECORD OF ITS OWN SLOT AND STAYS ONE.** At slot 2 the
        // operator had not yet sent his RRR, so *your move* is what was true
        // then and is what the cell must keep saying.
        Assert.DoesNotContain("complete", newest.Contact, StringComparison.Ordinal);
        Assert.Contains("your move", newest.Contact, StringComparison.Ordinal);

        // ---- 3. AND THE SEND AREA NEVER SAYS WHERE THE CONTACT STANDS.
        _output.WriteLine("MEASUREMENT 3 - the Send area under the waterfall, quoted whole");
        _output.WriteLine("  DigitalSendLine          : \"" + panel.DigitalSendLine + "\"");
        _output.WriteLine("  DigitalTransmitLevelLine : \"" + panel.DigitalTransmitLevelLine + "\"");

        // **THE FOUR WORDS, ASKED OF THE ENUM RATHER THAN WRITTEN OUT**, so a
        // fifth state or a renamed one could not slip past this.
        var everyState = Enum.GetValues<Ft8ContactState>()
            .Select(s => new Ft8ContactRead(His, s, 0).Words)
            .ToList();

        _output.WriteLine("  the four state words     : " + string.Join(" | ", everyState));

        foreach (var words in everyState)
        {
            Assert.DoesNotContain(words, panel.DigitalSendLine, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(
                words, panel.DigitalTransmitLevelLine, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>Waits for the wall clock to reach a slot, and says which it is.</summary>
    /// <param name="wantedUtc">The slot boundary to wait for.</param>
    /// <returns>The slot the clock is in once it has arrived.</returns>
    /// <remarks>
    /// <para>**IT WAITS FOR THE REAL CLOCK RATHER THAN PRETENDING TIME PASSED.**
    /// <c>MainWindowViewModel.SendMessage</c> reads
    /// <c>Ft8Slots.TrueUtc(DateTime.UtcNow, ClockOffset)</c> at the click
    /// (`:8174`), so the only honest way to have the operator's second message
    /// book into a later slot is for a later slot to have arrived. Setting
    /// <c>ClockOffset</c> would move the same arithmetic by hand and would prove
    /// the arithmetic rather than the join.</para>
    /// <para>**IT COSTS UP TO THIRTY SECONDS OF WALL TIME AND IT IS BOUNDED.**
    /// Two slot boundaries at fifteen seconds each. The bound is generous, and it
    /// fails with the figures rather than hanging.</para>
    /// </remarks>
    private async Task<DateTime> WaitForSlotAsync(DateTime wantedUtc)
    {
        var clock = System.Diagnostics.Stopwatch.StartNew();
        var limit = TimeSpan.FromSeconds(60);

        while (Ft8Slots.SlotStart(DateTime.UtcNow) < wantedUtc && clock.Elapsed < limit)
        {
            await Task.Delay(50).ConfigureAwait(false);
        }

        var reached = Ft8Slots.SlotStart(DateTime.UtcNow);

        _output.WriteLine("waited " + clock.ElapsedMilliseconds + " ms for slot "
            + Stamp(wantedUtc) + "; the clock is in " + Stamp(reached));

        Assert.True(
            reached >= wantedUtc,
            "the clock never reached " + Stamp(wantedUtc) + " in "
            + clock.ElapsedMilliseconds + " ms.");

        return reached;
    }

    /// <summary>One heard message, on the table, through the app's own door.</summary>
    /// <param name="panel">The view model.</param>
    /// <param name="slotUtc">The slot boundary it arrived in, in true UTC.</param>
    /// <param name="snr">The measured ratio cell - what the menu reports back with.</param>
    /// <param name="message">The message, as it was on the air.</param>
    /// <returns>The row as it was placed.</returns>
    private DigitalDecodeRow Heard(
        MainWindowViewModel panel, DateTime slotUtc, string snr, string message)
    {
        var row = panel.AddDecodeRowForTests(
            slotUtc.ToString("HHmmss", CultureInfo.InvariantCulture),
            snr, "0.2", "1240", message, slotUtc);

        _output.WriteLine("heard  " + Stamp(slotUtc) + "  \"" + message
            + "\"  contact: \"" + row.Contact + "\"");

        return row;
    }

    /// <summary>
    /// One transmission, chosen off the row's own menu and sent by clicking.
    /// </summary>
    /// <param name="panel">The view model.</param>
    /// <param name="row">The row the operator right-clicked.</param>
    /// <param name="shape">Which of the menu's options he picked.</param>
    /// <returns>The slot it was armed for and what the boundary did.</returns>
    /// <remarks>
    /// **THE TEXT IS THE MENU'S, NOT THIS TEST'S.** The message string is read
    /// off <see cref="Ft8SendOption.Text"/>, so what goes out is what the
    /// application offered the operator - which is the half of the join that a
    /// hand-written string would skip.
    /// </remarks>
    private async Task<(DateTime SlotUtc, Ft8BoundaryResult Result)> ClickAsync(
        MainWindowViewModel panel, DigitalDecodeRow row, Ft8SendShape shape)
    {
        var menu = panel.SendMenuFor(row);

        Assert.True(menu is not null, "no menu for the row \"" + row.Message + "\".");

        _output.WriteLine("menu on \"" + row.Message + "\": "
            + string.Join(" | ", menu!.Options.Select(o => o.Shape + " " + o.Text)));

        if (menu.Absent.Count > 0)
        {
            _output.WriteLine("  absent: " + string.Join(" | ", menu.Absent));
        }

        var option = menu.Options.FirstOrDefault(o => o.Shape == shape);

        Assert.True(option is not null, "the menu offered no " + shape + " option.");

        panel.SendMessageCommand.Execute(option!.Text);

        var slot = panel.ArmedForSlotUtc;

        Assert.True(slot is not null,
            "nothing was armed for \"" + option.Text + "\": " + panel.DigitalSendLine);

        var result = await panel.AtSlotBoundaryAsync(slot!.Value);

        Assert.NotNull(result);

        _output.WriteLine("click  " + Stamp(slot.Value) + "  \"" + option.Text
            + "\"  -> " + result!.Outcome + " / sent=" + result.Run?.AudioWentOut);

        return (slot.Value, result);
    }

    /// <summary>Every `ft8_transmission` line the application's writer left.</summary>
    /// <returns>The raw lines, in the order they were written.</returns>
    /// <remarks>
    /// **READ BACK OFF DISK, NOT OFF A DOUBLE** (`CLAUDE.md` §0.0). A `Write`
    /// call returning is not evidence a line was written: `JsonlTelemetry.Write`
    /// swallows every exception, returns `void`, and appends on a background
    /// thread. The file is what is asserted.
    /// </remarks>
    private List<string> TransmitLines()
        => !Directory.Exists(_folder)
            ? []
            : Directory.GetFiles(_folder, "*.jsonl")
                .OrderBy(f => f, StringComparer.Ordinal)
                .SelectMany(File.ReadAllLines)
                .Where(l => l.Contains("\"" + TransmitRecord.EventName + "\"", StringComparison.Ordinal))
                .ToList();

    /// <summary>A slot boundary as a reader sees it.</summary>
    private static string Stamp(DateTime utc)
        => utc.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

    /// <summary>A slot boundary the way `TransmitRecord.ToBag` writes it.</summary>
    private static string Iso(DateTime utc)
        => utc.ToString("O", CultureInfo.InvariantCulture);

    /// <summary>
    /// A panel on 20 m, a licence that permits, a fake sink and the application's
    /// own telemetry writer pointed at a temporary folder.
    /// </summary>
    /// <returns>The panel, its settings, the recording factory and the writer.</returns>
    /// <remarks>
    /// **THE WRITER IS BUILT THE WAY <c>App.axaml.cs:38-43</c> BUILDS IT**,
    /// including the enabled-category predicate off a real
    /// <see cref="AppSettings"/> and the byte cap, so that what is asserted is
    /// the application's own path to disk and not a permissive stand-in.
    /// </remarks>
    private (MainWindowViewModel Panel, AppSettings Settings,
        RecordingSinkFactory Factory, JsonlTelemetry Telemetry) Panel()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN00";
        settings.Operator.LicenseClass = LicenseClass.General;

        var telemetry = new JsonlTelemetry(
            _folder,
            "1.12.90",
            category => settings.IsTelemetryEnabled(category),
            settings.TelemetryMaxMegabytes * 1024L * 1024L);

        var panel = new MainWindowViewModel(settings, telemetry);

        // 20 m FIRST, THEN THE FREQUENCY - `OnFrequencyHzChanged` clamps to the
        // selected band's map window and clears the table on a retune, so both
        // happen before a single row is placed.
        panel.SelectedBand = panel.Bands.First(
            b => b.Band.LowHz <= Ft8On20m && b.Band.HighHz >= Ft8On20m);
        panel.FrequencyHz = Ft8On20m;

        var factory = new RecordingSinkFactory();

        panel.TransmitSinkFactory = factory.Make;

        return (panel, settings, factory, telemetry);
    }

    /// <summary>A sink factory that hands back a fake and opens nothing.</summary>
    private sealed class RecordingSinkFactory
    {
        /// <summary>Every endpoint name it was asked for, in order.</summary>
        public List<string> Calls { get; } = [];

        /// <summary>The one fake it hands back.</summary>
        public FakeSink Sink { get; } = new();

        /// <summary>The factory itself.</summary>
        public ITransmitAudioSink Make(string endpoint)
        {
            Calls.Add(endpoint);

            return Sink;
        }
    }
}
