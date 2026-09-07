using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 270, task 3: **after the operator's own last transmission,
/// the Send area under the waterfall says where that contact stands.**
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THIS WOULD HAVE CAUGHT: THE SCREEN SAYING *your move* ON
/// A CONTACT THE LEDGER HAS ALREADY RECORDED COMPLETE.** A great many real
/// contacts end on the operator's own message - he answers a CQ, the station
/// comes back `KC3QIS W1ABC R-09`, which is a report and a roger in one field,
/// and his `RRR` is what satisfies <see cref="Ft8ContactStates.IsComplete"/>.
/// `PlaceRow` computes a row's contact cell once, at that row's own slot
/// (`MainWindowViewModel.cs:7840`), and `RecordSent` (`:8474`) writes into a
/// private ledger and touches nothing already on the table - **so the newest row
/// on screen is the one placed *before* his last transmission and it still reads
/// *your move*.** If the station then goes quiet, nothing in Hamlet ever says the
/// contact completed. At the rig that is a man re-sending into a contact that
/// finished, or waiting for a station that has finished with him. **Nothing in
/// the tree failed when that happened**, which is what
/// <c>TheWholeContactWalksThroughTheApplicationTests.AfterHisOwnLastMessageTheLedgerIsCompleteAndTheNewestRowIsNot</c>
/// measured before a line of this was built.</para>
/// <para>**THE PRESENT STATE GOES INTO A LINE ABOUT THE PRESENT, NOT INTO THE
/// PAST.** No row already on the table is rewritten, and the measurement test
/// named above asserts that it is not. A table of moments that edits its own
/// moments is a worse instrument than one that is merely incomplete.</para>
/// <para>**WHY THESE TESTS ARE HERE AND NOT IN THE WALK'S FILE.** Task 2's
/// measurement was added to
/// <c>TheWholeContactWalksThroughTheApplicationTests</c> so that nothing was
/// copied. These four need a **realized window** - the line is read off the
/// <c>TextBlock</c> where the operator would read it, not off a view-model
/// string, and one of them asserts where that control sits and that
/// <c>DigitalStopButton</c> is still usable beside it - and that file builds no
/// window at all. The scene below follows
/// <c>TheReadoutSaysWhatTheCardWasHandedTests.Scene</c>, unit 269's, which is
/// already the shape for a window with a fake port and a fake sink behind
/// <c>BuildTheArmedSend</c>.</para>
/// <para>**NOTHING IS OPENED, NOTHING IS KEYED, AND NO SOUND IS MADE**
/// (`SHACK_FACTS.md` FACT-004). The port is <see cref="FakePort"/>, the sink
/// arrives through the substituted factory, and no render endpoint is enumerated
/// or opened. Every figure printed was measured on the development machine,
/// which has never had a radio attached to it.</para>
/// </remarks>
public sealed class TheContactStandsAfterHisLastTransmissionTests
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

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where every measured figure is printed.</param>
    public TheContactStandsAfterHisLastTransmissionTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// **After his own last message completes the exchange, the line under the
    /// waterfall names the station, reads complete, and says which slot it was
    /// read at** - and nothing already on the table moved.
    /// </summary>
    /// <remarks>
    /// <para>**THE WORDS ARE READ OFF THE LEDGER, NOT WRITTEN OUT HERE.** The
    /// expected text comes from <c>Ft8ContactStates.Read(record, slot).Text</c>
    /// against the record the application itself kept, so a second copy of the
    /// completeness rule or of the four state words inside the view model could
    /// not pass this.</para>
    /// <para>**AND IT SAYS WHEN IT WAS READ.** A screen that says *complete* with
    /// no idea when is the same class of fault as a readout that shows a setting
    /// and calls it a measurement - unit 269's version of this.</para>
    /// <para>**NOTHING ELSE MOVED**, asserted two ways: every contact cell on
    /// <c>DigitalDecodes</c> is captured before the last send and compared after
    /// it, and a second boundary with nothing armed leaves the sink's call count
    /// and the port's frames where they were. **One click, one message**, and the
    /// new reader of the ledger is not a second way to reach the sink.</para>
    /// </remarks>
    [AvaloniaFact]
    public async Task AfterHisOwnLastMessageTheLineNamesTheStationAndReadsComplete()
    {
        var scene = Scene();
        var walk = await HisOwnLastMessageAsync(scene);

        // ---- 1. WHAT THE LEDGER THE APPLICATION KEPT SAYS.
        var record = scene.Panel.ContactRecordForTests(His);

        Assert.NotNull(record);

        var read = Ft8ContactStates.Read(record!, walk.Second.SlotUtc);

        _output.WriteLine("MEASURED, development machine, FACT-004:");
        _output.WriteLine("the ledger, read at " + Stamp(walk.Second.SlotUtc)
            + " - his own send slot:");
        _output.WriteLine("  IsComplete : " + Ft8ContactStates.IsComplete(record!));
        _output.WriteLine("  Read().Text: \"" + read.Text + "\"");

        Assert.Equal(Ft8ContactState.Complete, read.State);

        // ---- 2. WHAT THE LINE UNDER THE WATERFALL SAYS.
        var line = Named<TextBlock>(scene.Window, "DigitalContactStandsText").Text ?? "";

        _output.WriteLine("the line under the waterfall reads:");
        _output.WriteLine("  " + line);

        Assert.Contains(His, line, StringComparison.Ordinal);
        Assert.Contains(read.Text, line, StringComparison.Ordinal);
        Assert.Contains(Stamp(walk.Second.SlotUtc), line, StringComparison.Ordinal);

        // ---- 3. NOTHING ON THE TABLE MOVED.
        _output.WriteLine("the contact cells across his last send:");

        foreach (var (message, before) in walk.CellsBefore)
        {
            var now = scene.Panel.DigitalDecodes.First(r => r.Message == message).Contact;

            _output.WriteLine("  \"" + message + "\"  before: \"" + before
                + "\"  after: \"" + now + "\"");

            Assert.Equal(before, now);
        }

        // ---- 4. AND A SECOND BOUNDARY WITH NOTHING ARMED CHANGES NOTHING.
        var callsBefore = scene.Sink.TimesCalled;
        var framesBefore = scene.Port.Written.Count;

        var empty = await scene.Panel.AtSlotBoundaryAsync(walk.Second.SlotUtc.AddSeconds(15));

        Pump(scene.Window);

        var after = Named<TextBlock>(scene.Window, "DigitalContactStandsText").Text ?? "";

        _output.WriteLine("a second boundary with nothing armed:");
        _output.WriteLine("  outcome     : " + (empty is null ? "null" : empty.Outcome.ToString()));
        _output.WriteLine("  sink calls  : " + callsBefore + " -> " + scene.Sink.TimesCalled);
        _output.WriteLine("  port frames : " + framesBefore + " -> " + scene.Port.Written.Count);
        _output.WriteLine("  the line    : " + after);

        Assert.Equal(callsBefore, scene.Sink.TimesCalled);
        Assert.Equal(framesBefore, scene.Port.Written.Count);
        Assert.Equal(line, after);

        scene.Window.Close();
    }

    /// <summary>
    /// **The line is inside the reserved Send area under the waterfall, and the
    /// Stop button is still usable beside it.**
    /// </summary>
    /// <remarks>
    /// The guard is <c>TheDriveIsSetWhereHeIsLookingTests.cs:109-115</c>, copied:
    /// **a fourth line in the Send area that pushes the always-pressable Stop off
    /// the visible area has broken the first of the three things no unit may
    /// reason past.** The new line is the last child of the area's
    /// <c>StackPanel</c>, below <c>DigitalTransmitLevelText</c>, so nothing before
    /// the Stop button changed.
    /// </remarks>
    [AvaloniaFact]
    public void TheLineIsInTheSendAreaAndTheStopButtonIsStillUsable()
    {
        var scene = Scene();

        var stands = Named<TextBlock>(scene.Window, "DigitalContactStandsText");
        var reserved = Named<Border>(scene.Window, "DigitalSendReserved");

        _output.WriteLine("nothing has been sent, and the line reads:");
        _output.WriteLine("  " + stands.Text);

        Assert.True(
            stands.GetVisualAncestors().Contains(reserved),
            "the contact line is on the window but not inside DigitalSendReserved, "
            + "which is the area under the waterfall.");

        // **AND IT CLAIMS NOTHING BEFORE ANYTHING HAS BEEN SENT** (§0.0). None of
        // the four state words is on the screen until a send has been booked.
        foreach (var words in EveryStateWord())
        {
            Assert.DoesNotContain(words, stands.Text ?? "", StringComparison.OrdinalIgnoreCase);
        }

        // **AND THE STOP BUTTON IS STILL THERE AND STILL ON THE VISIBLE AREA.**
        var stop = Named<Button>(scene.Window, "DigitalStopButton");
        var stopAt = stop.TranslatePoint(default, scene.Window)!.Value;

        _output.WriteLine("DigitalStopButton at : " + stopAt + "  size " + stop.Bounds.Size);

        Assert.True(stop.IsVisible && stop.IsEffectivelyEnabled, "the Stop button is not usable.");
        Assert.True(
            stopAt.Y >= 0 && stopAt.Y + stop.Bounds.Height <= scene.Window.Bounds.Height,
            "the Stop button was pushed off the visible area, at y " + stopAt.Y
            + " on a window " + scene.Window.Bounds.Height + " high.");

        scene.Window.Close();
    }

    /// <summary>
    /// **A CQ books nobody, so the line names no station and invents no
    /// contact.**
    /// </summary>
    /// <remarks>
    /// <para>Task 3's fifth assertion. <c>Ft8ContactLedger.RecordSent</c> at
    /// <c>Ft8ContactLedger.cs:229</c> already refuses a call to anyone, and the
    /// line asks <c>Ft8MessageSplit.IsCallToAnyone</c> the same question of the
    /// same field rather than deciding it a second time.</para>
    /// <para>**A STATION IS ON THE TABLE WHEN THE CQ GOES OUT**, deliberately: a
    /// line that reached for the newest station it could find would pass a
    /// version of this test with an empty ledger and be wrong at the radio.</para>
    /// </remarks>
    [AvaloniaFact]
    public async Task ACallToAnyoneBooksNobodyAndTheLineNamesNoStation()
    {
        var scene = Scene();

        // A station is heard, so the ledger exists and W1ABC is in it.
        Heard(scene, Ft8Slots.SlotStart(DateTime.UtcNow), "-14", "CQ " + His + " EM12");

        Assert.NotNull(scene.Panel.ContactRecordForTests(His));

        scene.Panel.SendCallToAnyoneCommand.Execute(null);

        var slot = scene.Panel.ArmedForSlotUtc;

        Assert.True(slot is not null, "the CQ armed nothing: " + scene.Panel.DigitalSendLine);

        var result = await scene.Panel.AtSlotBoundaryAsync(slot!.Value);

        Assert.NotNull(result);
        Assert.Equal(Ft8ArmOutcome.Ran, result!.Outcome);
        Assert.True(result.Run!.Sent, result.Run.Reason);

        Pump(scene.Window);

        var line = Named<TextBlock>(scene.Window, "DigitalContactStandsText").Text ?? "";

        _output.WriteLine("MEASURED, development machine, FACT-004:");
        _output.WriteLine("what went out : \"" + scene.Panel.CallToAnyoneText + "\"");
        _output.WriteLine("the line reads:");
        _output.WriteLine("  " + line);

        // **IT NAMES NO STATION**, and W1ABC in particular - the one station the
        // ledger holds - is not reached for.
        Assert.DoesNotContain(His, line, StringComparison.Ordinal);

        // **AND IT CLAIMS NO STATE**, because a call to anyone is not a contact
        // with anybody yet.
        foreach (var words in EveryStateWord())
        {
            Assert.DoesNotContain(words, line, StringComparison.OrdinalIgnoreCase);
        }

        scene.Window.Close();
    }

    /// <summary>
    /// **A later decode from that station does not move the line, and the row it
    /// places carries the state instead** - which is what the line's own wording
    /// covers by saying which slot it was read at.
    /// </summary>
    /// <remarks>
    /// <para>Task 3's sixth assertion, and the answer is *it does not move, and
    /// here is why that is right*. The line is written in one place - the
    /// <c>Dispatcher.UIThread.Post</c> at
    /// <c>MainWindowViewModel.cs:8487-8492</c>, after a boundary has returned -
    /// so a decode arriving later leaves it exactly where it was. **That is not a
    /// gap**: a decode from the station places a row, and that row's own contact
    /// cell is read at its own slot and says `complete`. The table carries the
    /// news the line was written for; the line carries the moment his own
    /// transmission left, which is the moment nothing else in Hamlet could
    /// report.</para>
    /// <para>**AND THE LINE SAYS WHEN IT WAS READ**, so it cannot be mistaken for
    /// a live reading afterwards. That wording is what makes standing still
    /// honest rather than stale.</para>
    /// </remarks>
    [AvaloniaFact]
    public async Task ALaterDecodeLeavesTheLineWhereItWasAndTheNewRowCarriesTheState()
    {
        var scene = Scene();
        var walk = await HisOwnLastMessageAsync(scene);

        var before = Named<TextBlock>(scene.Window, "DigitalContactStandsText").Text ?? "";

        // He comes back after all, one slot later.
        var late = Heard(
            scene, walk.Second.SlotUtc.AddSeconds(15), MeasuredHim, Mine + " " + His + " 73");

        Pump(scene.Window);

        var after = Named<TextBlock>(scene.Window, "DigitalContactStandsText").Text ?? "";

        _output.WriteLine("MEASURED, development machine, FACT-004:");
        _output.WriteLine("the line, before the later decode:");
        _output.WriteLine("  " + before);
        _output.WriteLine("the line, after it:");
        _output.WriteLine("  " + after);
        _output.WriteLine("the row the later decode placed:");
        _output.WriteLine("  " + Stamp(late.SlotStartUtc) + "  \"" + late.Message
            + "\"  contact: \"" + late.Contact + "\"");

        // IT DOES NOT MOVE, AND IT STILL SAYS WHICH SLOT IT WAS READ AT.
        Assert.Equal(before, after);
        Assert.Contains(Stamp(walk.Second.SlotUtc), after, StringComparison.Ordinal);

        // AND THE NEWS IS ON THE TABLE, IN THE ROW'S OWN SLOT.
        Assert.Contains("complete", late.Contact, StringComparison.Ordinal);

        scene.Window.Close();
    }

    /// <summary>The four state words, asked of the enum rather than written out.</summary>
    /// <returns>Every state's words, so a fifth or a renamed one cannot slip past.</returns>
    private static List<string> EveryStateWord()
        => Enum.GetValues<Ft8ContactState>()
            .Select(s => new Ft8ContactRead(His, s, 0).Words)
            .ToList();

    /// <summary>What the exchange left behind.</summary>
    /// <param name="First">The operator's grid, and the slot it went out in.</param>
    /// <param name="Second">His `RRR`, the message that completes the exchange.</param>
    /// <param name="CellsBefore">Every row's message and contact cell before it.</param>
    private sealed record Walk(
        (DateTime SlotUtc, Ft8BoundaryResult Result) First,
        (DateTime SlotUtc, Ft8BoundaryResult Result) Second,
        IReadOnlyList<(string Message, string Contact)> CellsBefore);

    /// <summary>
    /// **The ending this unit exists for**: he answers a CQ, the station comes
    /// back with a report and a roger in one field, and his own `RRR` is the
    /// message that completes the exchange.
    /// </summary>
    /// <param name="scene">The window and the fakes behind it.</param>
    /// <returns>The two sends and the table as it stood before the last one.</returns>
    /// <remarks>
    /// **IT WAITS FOR A REAL SLOT BOUNDARY**, for the reason
    /// <c>TheWholeContactWalksThroughTheApplicationTests.WaitForSlotAsync</c>
    /// gives: <c>SendMessage</c> reads the real clock at the click, so the only
    /// honest way to have the second message book into a later slot is for a
    /// later slot to have arrived. It costs up to thirty seconds of wall time and
    /// it is bounded.
    /// </remarks>
    private async Task<Walk> HisOwnLastMessageAsync(Built scene)
    {
        var slot0 = Ft8Slots.SlotStart(DateTime.UtcNow);

        // SLOT 0 - he calls CQ.
        var cq = Heard(scene, slot0, "-14", "CQ " + His + " EM12");

        // SLOT 1 - the operator answers a CQ with his grid.
        var first = await ClickAsync(scene, cq, Ft8SendShape.Grid);

        var slot2 = await WaitForSlotAsync(slot0.AddSeconds(30));

        // SLOT 2 - a report and a roger in one field.
        var report = Heard(scene, slot2, MeasuredHim, Mine + " " + His + " R-09");

        // The table exactly as it stands before his last transmission.
        var cellsBefore = scene.Panel.DigitalDecodes
            .Select(r => (r.Message, r.Contact))
            .ToList();

        // SLOT 3 - **his own RRR, which is what satisfies IsComplete.**
        var second = await ClickAsync(scene, report, Ft8SendShape.Acknowledge);

        Assert.True(
            second.SlotUtc > first.SlotUtc,
            "the two sends booked into the same slot: " + Stamp(first.SlotUtc));

        // SLOT 4 - NOTHING IS HEARD.

        Pump(scene.Window);

        return new Walk(first, second, cellsBefore);
    }

    /// <summary>One heard message, on the table, through the app's own door.</summary>
    private DigitalDecodeRow Heard(
        Built scene, DateTime slotUtc, string snr, string message)
    {
        var row = scene.Panel.AddDecodeRowForTests(
            slotUtc.ToString("HHmmss", CultureInfo.InvariantCulture),
            snr, "0.2", "1240", message, slotUtc);

        _output.WriteLine("heard  " + Stamp(slotUtc) + "  \"" + message
            + "\"  contact: \"" + row.Contact + "\"");

        return row;
    }

    /// <summary>One transmission, chosen off the row's own menu and clicked.</summary>
    /// <remarks>
    /// **THE TEXT IS THE MENU'S, NOT THIS TEST'S**, the same rule the committed
    /// walk keeps: what goes out is what the application offered the operator.
    /// </remarks>
    private async Task<(DateTime SlotUtc, Ft8BoundaryResult Result)> ClickAsync(
        Built scene, DigitalDecodeRow row, Ft8SendShape shape)
    {
        var menu = scene.Panel.SendMenuFor(row);

        Assert.True(menu is not null, "no menu for the row \"" + row.Message + "\".");

        var option = menu!.Options.FirstOrDefault(o => o.Shape == shape);

        Assert.True(option is not null, "the menu offered no " + shape + " option.");

        scene.Panel.SendMessageCommand.Execute(option!.Text);

        var slot = scene.Panel.ArmedForSlotUtc;

        Assert.True(slot is not null,
            "nothing was armed for \"" + option.Text + "\": " + scene.Panel.DigitalSendLine);

        var result = await scene.Panel.AtSlotBoundaryAsync(slot!.Value);

        Assert.NotNull(result);

        _output.WriteLine("click  " + Stamp(slot.Value) + "  \"" + option.Text
            + "\"  -> " + result!.Outcome + " / sent=" + result.Run?.Sent);

        Assert.Equal(Ft8ArmOutcome.Ran, result.Outcome);
        Assert.True(result.Run!.Sent, result.Run.Reason);

        return (slot.Value, result);
    }

    /// <summary>Waits for the wall clock to reach a slot, and says which it is.</summary>
    private async Task<DateTime> WaitForSlotAsync(DateTime wantedUtc)
    {
        var clock = System.Diagnostics.Stopwatch.StartNew();
        var limit = TimeSpan.FromSeconds(60);

        while (Ft8Slots.SlotStart(DateTime.UtcNow) < wantedUtc && clock.Elapsed < limit)
        {
            await Task.Delay(50).ConfigureAwait(true);
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

    /// <summary>A slot boundary as a reader sees it.</summary>
    private static string Stamp(DateTime utc)
        => utc.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

    /// <summary>One realized control, found on the window by its own name.</summary>
    private static T Named<T>(MainWindow window, string name)
        where T : Control
    {
        Pump(window);

        var found = window.GetVisualDescendants().OfType<T>()
            .FirstOrDefault(c => c.Name == name);

        Assert.True(
            found is not null,
            "there is no " + typeof(T).Name + " called \"" + name
            + "\" on the realized window.");

        return found!;
    }

    /// <summary>Runs the dispatcher queue and lays the window out.</summary>
    private static void Pump(Window window)
    {
        for (var i = 0; i < 5; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    /// <summary>The window, the panel and the fakes behind them.</summary>
    private sealed record Built(
        MainWindow Window, MainWindowViewModel Panel, AppSettings Settings,
        FakePort Port, FakeSink Sink);

    /// <summary>
    /// The real window on the Digital tab, on 20 m, with a fake port and a fake
    /// sink reached through the application's own <c>BuildTheArmedSend</c>.
    /// </summary>
    /// <returns>The window and everything behind it.</returns>
    private static Built Scene()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN00";
        settings.Operator.LicenseClass = LicenseClass.General;
        settings.AudioOutputDeviceId = NamedEndpoint;

        var panel = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalDecodedExpanded = true,
        };

        // **TALL ENOUGH THAT THE SEND AREA IS ON THE SCREEN**, the size unit 268
        // measured it needs.
        var window = new MainWindow { DataContext = panel, Width = 1400, Height = 1400 };

        window.Show();
        Pump(window);

        panel.SelectedBand = panel.Bands.First(
            b => b.Band.LowHz <= Ft8On20m && b.Band.HighHz >= Ft8On20m);
        panel.FrequencyHz = Ft8On20m;

        var port = new FakePort();
        var sink = new FakeSink();

        panel.TransmitSinkFactory = _ => sink;
        panel.BuildTheArmedSend(port);

        Assert.True(panel.HasSomethingToTransmitThrough, panel.DigitalSendLine);

        Pump(window);

        return new Built(window, panel, settings, port, sink);
    }
}
