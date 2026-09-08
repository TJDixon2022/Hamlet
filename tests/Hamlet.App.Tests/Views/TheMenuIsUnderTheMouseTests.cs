using System.Globalization;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 260, task 2: **an operator right-clicks a decoded row with a
/// mouse and the menu appears under it.**
/// </summary>
/// <remarks>
/// <para>**STEP 5's CRITERION 2, IN ITS LETTER.** The criterion is written as a
/// gesture - *right-click a decoded row and the menu offers every message valid
/// at that point* - and until this unit there was no `ContextFlyout` and no
/// `ContextRequested` handler anywhere in `src/`
/// (`docs/unit260-route-trace.md` question 1), so the gesture had never happened.
/// A method no view calls does not meet a criterion about a mouse.</para>
/// <para>**THE BREAKAGE THESE WOULD HAVE CAUGHT, AND IT IS THE REASON UNIT 259
/// DROPPED THIS.** A flyout built once when the row arrived and hung on the
/// control shows the repeat counts the row was born with. An operator who has
/// already sent `RRR` twice is then offered it as a first send, and he sends a
/// third believing it is his first. <c>TheRepeatCountBelongsToTheClick</c> is
/// that fault, watched failing before the handler was moved.</para>
/// <para>**THE MENUS ARE COMPARED AGAINST A PREDICTION THAT PREDATES THE OPTION
/// LIST** - `docs/unit259-send-path-trace.md` 6, committed before
/// <c>Ft8SendOptions</c> existed. That is what makes this a measurement rather
/// than a restatement of what the code does.</para>
/// <para>**NOTHING IS OPENED.** No serial port, no audio device. The five sends
/// the corpus's ledger needs go through the application's own send path over a
/// fake port and a fake sink (`SHACK_FACTS.md` FACT-004).</para>
/// </remarks>
public sealed class TheMenuIsUnderTheMouseTests
{
    /// <summary>The boundary slot 0 opened on, as the scene has it.</summary>
    private static readonly DateTime SlotZero =
        new(2026, 9, 6, 18, 0, 0, DateTimeKind.Utc);

    /// <summary>FT8's watering hole on 20 m, where a General may send data.</summary>
    private const long Ft8On20m = 14_074_000;

    /// <summary>
    /// **The report supplied uniformly, exactly as the prediction states it.**
    /// </summary>
    /// <remarks>
    /// `docs/unit259-send-path-trace.md` 6 says the prediction is read with a
    /// report of -10 dB throughout, so the predicted texts are exact. **A report
    /// is a measurement and the menu may not invent one** (0.0): it arrives from
    /// the row the operator right-clicked, so the rows carry -10.
    /// </remarks>
    private const string Report = "-10";

    /// <summary>Everything the scene heard from somebody other than the operator.</summary>
    private static readonly (int Slot, string Message)[] Heard =
    [
        (0, "JA1ZZ G4XYZ -08"),
        (2, "KC3QIS VK2PQ QF56"),
        (2, "KC3QIS K9RST EM12"),
        (2, "KC3QIS G4XYZ IO91"),
        (4, "DL1QQ G4XYZ -12"),
        (4, "KC3QIS K9RST R-09"),
        (4, "TNX BOB 73 GL"),
        (6, "JA1ZZ G4XYZ RR73"),
        (6, "KC3QIS K9RST 73"),
        (6, "CQ W1ABC FN42"),
        (8, "DL1QQ G4XYZ RRR"),
        (8, "KC3QIS N5TT EM10"),
        (8, "KC3QIS W1ABC R-15"),
        (10, "CQ G4XYZ IO91"),
        (10, "ABCDEFGHIJKLM"),
    ];

    /// <summary>Everything the operator himself sent, in the scene's order.</summary>
    /// <remarks>
    /// **REPLAYED THROUGH THE APPLICATION'S OWN SEND PATH**, not written into the
    /// ledger by hand. <c>Ft8ContactLedger.RecordSent</c> is called from exactly
    /// one line in `src/` - `MainWindowViewModel.AtSlotBoundaryAsync` - and only
    /// where a run says the whole transmission went out, so this is the only way
    /// the application can come to believe it has sent anything.
    /// </remarks>
    private static readonly string[] Sent =
    [
        "K9RST KC3QIS -13",
        "K9RST KC3QIS RRR",
        "W1ABC KC3QIS -12",
        "W1ABC KC3QIS RRR",
        "G4XYZ KC3QIS -14",
    ];

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the menus are printed, station by station.</param>
    public TheMenuIsUnderTheMouseTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// **All five stations: the menu under the mouse is the predicted one.**
    /// </summary>
    /// <remarks>
    /// A real `ContextRequested`, raised on the real row control in a real window,
    /// answered by the handler the markup names. The texts, the mark and the
    /// counts are compared against `docs/unit259-send-path-trace.md` 6.
    /// </remarks>
    [AvaloniaFact]
    public async Task EveryStationsPredictedMenuAppearsUnderTheMouse()
    {
        var scene = await SceneAsync();

        var predicted = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["G4XYZ"] =
            [
                "G4XYZ KC3QIS FN00   grid",
                "G4XYZ KC3QIS -10   report - the one that comes next",
                "G4XYZ KC3QIS R-10   roger and report",
                "G4XYZ KC3QIS RRR   acknowledge",
                "G4XYZ KC3QIS 73   73",
            ],
            ["VK2PQ"] =
            [
                "VK2PQ KC3QIS FN00   grid",
                "VK2PQ KC3QIS -10   report - the one that comes next",
                "VK2PQ KC3QIS R-10   roger and report",
                "VK2PQ KC3QIS RRR   acknowledge",
                "VK2PQ KC3QIS 73   73",
            ],
            ["K9RST"] =
            [
                "K9RST KC3QIS FN00   grid",
                "K9RST KC3QIS -10   report",
                "K9RST KC3QIS R-10   roger and report",
                "K9RST KC3QIS RRR   acknowledge, 2nd time",
                "K9RST KC3QIS 73   73 - the one that comes next",
            ],
            ["W1ABC"] =
            [
                "W1ABC KC3QIS FN00   grid",
                "W1ABC KC3QIS -10   report",
                "W1ABC KC3QIS R-10   roger and report",
                "W1ABC KC3QIS RRR   acknowledge, 2nd time - the one that comes next",
                "W1ABC KC3QIS 73   73",
            ],
            ["N5TT"] =
            [
                "N5TT KC3QIS FN00   grid",
                "N5TT KC3QIS -10   report - the one that comes next",
                "N5TT KC3QIS R-10   roger and report",
                "N5TT KC3QIS RRR   acknowledge",
                "N5TT KC3QIS 73   73",
            ],
        };

        var matched = 0;

        foreach (var (station, expected) in predicted)
        {
            var flyout = RightClick(scene, station);

            Assert.NotNull(flyout);

            var headers = Headers(flyout!);

            _output.WriteLine("");
            _output.WriteLine(station + " - under the mouse:");

            foreach (var header in headers)
            {
                _output.WriteLine("    " + header);
            }

            // **THE SEND OPTIONS ARE ASSERTED AGAINST THE PREDICTION AND THE LOG
            // ITEM IS ASSERTED SEPARATELY**, because they are different kinds of
            // thing: one arms a transmission and one opens a dialog. Unit 274
            // added Log after these five predictions were written, and folding it
            // into them would bury the distinction the next reader needs most.
            Assert.Equal(
                expected,
                headers.Where(h => h != LogHeader).ToArray());

            // **AND IT IS LAST, UNDER A RULE.** Every one of these rows is
            // addressed to the operator, so every one of them offers it - which
            // is the whole of what work instruction 276 restored.
            Assert.Equal(LogHeader, headers[^1]);
            Assert.NotNull(LogItem(flyout!));

            // NOTHING IS GREYED, EVER (ruled 2026-09-06).
            Assert.All(Options(flyout!), item => Assert.True(item.IsEnabled));

            matched++;
        }

        _output.WriteLine("");
        _output.WriteLine("NUMBER: " + matched + " of 5 stations matched the prediction");

        Assert.Equal(5, matched);

        scene.Window.Close();
    }

    /// <summary>
    /// **The repeat count belongs to the click, not to the row.**
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS THE RED THAT MATTERS.** Built the other way - the flyout
    /// captured once when the row arrived and reopened on the next right-click -
    /// `VK2PQ KC3QIS 73` came back reading *73* after it had already gone out,
    /// instead of *73, 2nd time*. **A menu that lies about what has already gone
    /// out** is what would have an operator send a third `RRR` believing it was
    /// his first.</para>
    /// <para>It is asserted on a station the scene never sent anything to, so the
    /// count moving from nought to one can only have come from the send this test
    /// made.</para>
    /// </remarks>
    [AvaloniaFact]
    public async Task TheRepeatCountBelongsToTheClickAndNotToTheRow()
    {
        var scene = await SceneAsync();

        var before = Headers(RightClick(scene, "VK2PQ")!);

        _output.WriteLine("before the send:");
        Assert.All(before, h => _output.WriteLine("    " + h));

        Assert.Contains("VK2PQ KC3QIS 73   73", before);

        await SendAsync(scene, "VK2PQ KC3QIS 73");

        var after = Headers(RightClick(scene, "VK2PQ")!);

        _output.WriteLine("after the send, right-clicking the same row again:");
        Assert.All(after, h => _output.WriteLine("    " + h));

        // **THE COUNT MOVED, AND IT MOVED BECAUSE THE MENU WAS BUILT AT THE
        // CLICK.** The row object is the same instance either way.
        Assert.Contains("VK2PQ KC3QIS 73   73, 2nd time", after);
        Assert.DoesNotContain("VK2PQ KC3QIS 73   73", after);

        // AND NOTHING WAS TAKEN AWAY BY HAVING BEEN SENT.
        Assert.Equal(before.Count, after.Count);

        scene.Window.Close();
    }

    /// <summary>
    /// **Choosing one goes through the one command that arms, and nothing else.**
    /// </summary>
    /// <remarks>
    /// No confirmation, no dialog, no second click (ruled 2026-09-06). Every item
    /// in the menu carries <c>SendMessageCommand</c> and that message's own text,
    /// and there is no second route to <c>Ft8ArmedSend.Arm</c> anywhere in `src/`
    /// (`docs/unit260-route-trace.md` question 5).
    /// </remarks>
    [AvaloniaFact]
    public async Task ChoosingOneGoesThroughTheOneCommandThatArms()
    {
        var scene = await SceneAsync();

        var flyout = RightClick(scene, "N5TT")!;

        foreach (var item in Options(flyout))
        {
            Assert.Same(scene.Panel.SendMessageCommand, item.Command);
            Assert.IsType<string>(item.CommandParameter);
        }

        var chosen = Options(flyout).Single(i => Header(i).StartsWith(
            "N5TT KC3QIS -10", StringComparison.Ordinal));

        chosen.Command!.Execute(chosen.CommandParameter);

        _output.WriteLine(scene.Panel.DigitalSendLine);

        Assert.NotNull(scene.Armed.Armed);
        Assert.Contains(
            "N5TT KC3QIS -10", scene.Panel.DigitalSendLine, StringComparison.Ordinal);

        scene.Window.Close();
    }

    /// <summary>
    /// **A row that names no station puts nothing under the mouse.**
    /// </summary>
    /// <remarks>
    /// `ABCDEFGHIJKLM` is not booked by the ledger at all, so there is no station
    /// to send anything to. The breakage: an empty box appearing over free text,
    /// which reads as *Hamlet has nothing to offer this station* rather than
    /// *this is not a station*.
    /// </remarks>
    [AvaloniaFact]
    public async Task ARowThatNamesNoStationPutsNothingUnderTheMouse()
    {
        var scene = await SceneAsync();

        var flyout = RightClickRow(scene, r => r.Message == "ABCDEFGHIJKLM");

        _output.WriteLine("flyout for a row with no station: "
            + (flyout is null ? "none" : flyout.Items.Count + " items"));

        Assert.Null(flyout);

        scene.Window.Close();
    }

    /// <summary>
    /// **An absent message is said as a note, and no option is taken away.**
    /// </summary>
    /// <remarks>
    /// With no grid in Settings there is no grid message to send - **absent, not
    /// withheld** (0.0). The four that remain are all still clickable and the
    /// reason is on screen.
    /// </remarks>
    [AvaloniaFact]
    public async Task WithNoGridTheReasonIsANoteAndTheRestStayClickable()
    {
        var scene = await SceneAsync();

        // **THE GRID IS CLEARED AFTER THE SCENE IS BUILT, AND THAT IS A FINDING
        // AS MUCH AS A CONVENIENCE.** A window opened with the grid unset does
        // not stay that way: the startup grid resolution looks the operator's own
        // callsign up and writes what it finds back into the profile, so a scene
        // built with `GridSquare` empty comes back with a six-character square in
        // it. Clearing it here asserts the menu against the grid **as it is at
        // the moment of the click**, which is the whole point of building the
        // menu in the handler.
        scene.Settings.Operator.GridSquare = "";

        var flyout = RightClick(scene, "N5TT")!;

        foreach (var header in Headers(flyout))
        {
            _output.WriteLine("    " + header);
        }

        var options = Options(flyout);

        Assert.Equal(4, options.Count);
        Assert.DoesNotContain(options, o => Header(o).Contains("FN00", StringComparison.Ordinal));
        Assert.All(options, o => Assert.True(o.IsEnabled));

        // THE NOTE IS THERE, IT SAYS WHY, AND IT CANNOT BE CLICKED.
        var note = Notes(flyout).Single();

        Assert.Contains("grid square is not set", Header(note), StringComparison.Ordinal);
        Assert.Null(note.Command);
        Assert.False(note.IsHitTestVisible);

        // **AND IT IS NOT GREYED.** A greyed line reads as a message Hamlet took
        // away; this is a message that does not exist.
        Assert.True(note.IsEnabled);

        scene.Window.Close();
    }

    /// <summary>
    /// **The licence appears as a note and every option stays clickable.**
    /// </summary>
    /// <remarks>
    /// Criterion 6's first half, in the place criterion 6 asks for it. *Nothing is
    /// forbidden in the menu* is a ruling about the contact state and it holds
    /// here too: what the menu gains is a line, not a shorter list. **The refusal
    /// that actually stops a transmission is inside
    /// <c>Ft8TransmitSequence.RunAsync</c>** and is not touched.
    /// </remarks>
    [AvaloniaFact]
    public async Task OutOfPrivilegesTheMenuSaysSoAndForbidsNothing()
    {
        var scene = await SceneAsync();

        // **THE CLASS CHANGES AFTER THE SCENE IS BUILT**, so the licence line is
        // read at the click and not at startup - and so the five replayed
        // messages, which need a licence that permits, still went out.
        scene.Settings.Operator.LicenseClass = LicenseClass.Technician;

        var flyout = RightClick(scene, "N5TT")!;

        foreach (var header in Headers(flyout))
        {
            _output.WriteLine("    " + header);
        }

        Assert.True(scene.Panel.HasDigitalSendLicenceLine);

        var note = Notes(flyout).Single();

        Assert.Equal(scene.Panel.DigitalSendLicenceLine, Header(note));
        Assert.False(note.IsHitTestVisible);

        // ALL FIVE, STILL, AND ALL FIVE STILL CLICKABLE.
        Assert.Equal(5, Options(flyout).Count);
        Assert.All(Options(flyout), o => Assert.True(o.IsEnabled));

        scene.Window.Close();
    }

    // -------------------------------------------------------------------------
    // The scene, and the gesture.
    // -------------------------------------------------------------------------

    /// <summary>A shown window, its panel, the corpus on the table, and the fakes.</summary>
    private sealed record Scene(
        MainWindow Window,
        MainWindowViewModel Panel,
        AppSettings Settings,
        Ft8ArmedSend Armed,
        FakePort Port,
        FakeSink Sink);

    /// <summary>
    /// Builds the real window, puts the scene's decodes on it, and replays the
    /// operator's own five messages through the application's send path.
    /// </summary>
    /// <remarks>
    /// **THE SCENE IS ALWAYS BUILT ON A LICENCE THAT PERMITS**, because the five
    /// replayed messages have to actually go out for the ledger to book them -
    /// `RecordSent` is called only where a run says the whole transmission went.
    /// A test that wants a refusing licence changes it afterwards, which is also
    /// closer to what an operator does.
    /// </remarks>
    private async Task<Scene> SceneAsync()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = "FN00";
        settings.Operator.LicenseClass = LicenseClass.General;

        var panel = new MainWindowViewModel(settings, null)
        {
            // **THE DIGITAL WORKSPACE IS COLLAPSED UNLESS THIS TAB IS SHOWING**,
            // and nothing under it is realized while it is - so a right-click
            // would find no row at all. `TheDecodedColumnsLineUpTests` paid for
            // this line first.
            OperatingMode = "Digital",
            DigitalDecodedExpanded = true,
        };

        var window = new MainWindow { DataContext = panel };

        window.Show();

        // **THE ROWS GO IN AFTER `Show`.** Showing raises `Opened`, which starts
        // the reconnect, which clears the decoded table.
        Pump(window);

        // 20 m FIRST, THEN THE FREQUENCY. `OnFrequencyHzChanged` clamps to the
        // selected band's map window and clears the decoded table on a retune, so
        // both happen before a single row is placed.
        panel.SelectedBand = panel.Bands.First(
            b => b.Band.LowHz <= Ft8On20m && b.Band.HighHz >= Ft8On20m);
        panel.FrequencyHz = Ft8On20m;

        Assert.Equal(Ft8On20m, panel.FrequencyHz);

        var port = new FakePort();
        var sink = new FakeSink();
        var armed = new Ft8ArmedSend(new Ft8TransmitSequence(port, sink));

        panel.UseArmedSendForTests(armed);

        foreach (var (slot, message) in Heard)
        {
            Add(panel, slot, message);
        }

        var scene = new Scene(window, panel, settings, armed, port, sink);

        foreach (var message in Sent)
        {
            await SendAsync(scene, message);
        }

        Pump(window);

        return scene;
    }

    /// <summary>One message out through the app's own path, over the fakes.</summary>
    private async Task SendAsync(Scene scene, string message)
    {
        scene.Panel.SendMessageCommand.Execute(message);

        var armed = scene.Armed.Armed;

        Assert.True(
            armed is not null,
            "nothing was armed for \"" + message + "\": " + scene.Panel.DigitalSendLine);

        var result = await scene.Panel.AtSlotBoundaryAsync(armed!.SlotStartUtc);

        Assert.NotNull(result);
        Assert.True(
            result!.Run?.Sent == true,
            "\"" + message + "\" did not go out: " + result.Outcome + " / "
            + result.Run?.Reason);
    }

    /// <summary>
    /// **Raises a real context request on the row that names one station.**
    /// </summary>
    /// <remarks>
    /// **THE CONVERSATION IS SWITCHED TO THAT STATION FIRST, SINCE UNIT 277.** The
    /// For you panel shows one conversation with the others waiting above it (Tim's
    /// ruling, 2026-09-08), so a station that is not the one on show has no
    /// realized row to right-click. Clicking its waiting row is exactly what the
    /// operator does, and doing it here means this class proves the switch works as
    /// well as the menu.
    /// </remarks>
    private MenuFlyout? RightClick(Scene scene, string station)
    {
        scene.Panel.ShowConversationCommand.Execute(station);
        Pump(scene.Window);

        return RightClickRow(scene, r => r.Sender == station && r.Addressee == "KC3QIS");
    }

    /// <summary>Raises a real context request on the first row that matches.</summary>
    /// <remarks>
    /// **THE EVENT IS RAISED ON THE ROW'S OWN CONTROL**, the `Grid` the
    /// `DataTemplate` builds, whose `DataContext` is the row - which is the same
    /// instance the view model holds (`docs/unit260-route-trace.md` question 2).
    /// Nothing here calls the handler directly.
    /// </remarks>
    private MenuFlyout? RightClickRow(Scene scene, Func<DigitalDecodeRow, bool> which)
    {
        Pump(scene.Window);

        // **BOTH LISTS, SINCE WORK INSTRUCTION 276.** Unit 273 split the decoded
        // area and moved everything addressed to the operator to the right-hand
        // list; this searched `DigitalDecodedRows` alone, so from that day it
        // could not find the very rows it asks for and every test here went red.
        // **It stayed red for three units** because it lives in the `Views`
        // namespace and HM-DEC-155 means a unit runs only the tests it writes.
        //
        // **THE SEARCH IS OVER BOTH RATHER THAN OVER THE RIGHT ONE**, so a later
        // unit moving a row between the lists does not silently take this test's
        // subject away again. What it asserts is that the row can be
        // right-clicked wherever it is drawn.
        var lists = scene.Window.GetVisualDescendants()
            .OfType<ItemsControl>()
            .Where(c => c.Name is "DigitalDecodedRows" or "DigitalMineRows")
            .ToList();

        Assert.True(
            lists.Count == 2,
            "expected both decoded lists in the window, found "
            + string.Join(", ", lists.Select(l => l.Name)));

        var grid = lists
            .SelectMany(l => l.GetVisualDescendants().OfType<Grid>())
            .FirstOrDefault(g => g.DataContext is DigitalDecodeRow row && which(row));

        Assert.True(
            grid is not null,
            "no realized row matched. Left rows: "
            + scene.Panel.DigitalVisibleDecodes.Count
            + "; mine rows: " + scene.Panel.DigitalMineDecodes.Count
            + "; realized grids with a row DataContext: "
            + lists.SelectMany(l => l.GetVisualDescendants().OfType<Grid>())
                .Count(g => g.DataContext is DigitalDecodeRow));

        grid!.RaiseEvent(new ContextRequestedEventArgs
        {
            RoutedEvent = Control.ContextRequestedEvent,
            Source = grid,
        });

        return scene.Window.SendFlyoutUnderTheMouse;
    }

    /// <summary>
    /// Work instruction 276 task 3: a row on each list has a menu, and Log is on
    /// the one it belongs on.
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS THE ASSERTION THAT WAS MISSING** — not that a menu opens,
    /// which the tests above have always done, but that it opens on **each list**.
    /// Unit 273 split the decoded area and carried the menu to neither; it stayed
    /// on the left, and the right-hand list — the only place a reply is ever sent
    /// from — had nothing to right-click for three units.</para>
    /// <para>**AND LOG IS ASSERTED ABSENT AS WELL AS PRESENT.** It is gated on the
    /// message being addressed to the operator, which on the mine side is every
    /// row and on the left side is none. A test that only checked it was there
    /// would pass on a gate that had been removed altogether.</para>
    /// </remarks>
    [AvaloniaFact]
    public async Task BothListsCarryTheMenuAndLogIsOnTheRightOne()
    {
        var scene = await SceneAsync();

        // **A ROW ON EACH SIDE, FOUND IN THE LISTS THEMSELVES** rather than
        // assumed, so this fails loudly if a later unit moves either.
        var mine = Assert.Single(
            scene.Panel.DigitalMineDecodes, r => r.Sender == "N5TT");
        var left = scene.Panel.DigitalVisibleDecodes.First(
            r => r.Addressee.StartsWith("CQ", StringComparison.Ordinal));

        _output.WriteLine("mine row : " + mine.Message);
        _output.WriteLine("left row : " + left.Message);
        _output.WriteLine("");

        // ---- the mine side -------------------------------------------------
        var onMine = RightClickRow(scene, r => ReferenceEquals(r, mine));

        // **THE ONE ASSERTION THAT WAS MISSING FOR THREE UNITS.** Against the
        // tree as the operator found it this is null: the row is realized, the
        // context request is raised on it, and no flyout comes back, because the
        // handler was never carried to this list.
        Assert.True(
            onMine is not null,
            "right-clicking a row on the mine list produced no menu. The list "
            + "carries every message addressed to the operator, so it is the one "
            + "place a reply is ever sent from.");

        _output.WriteLine("right-click on the mine row:");

        foreach (var header in Headers(onMine!))
        {
            _output.WriteLine("    " + header);
        }

        Assert.NotEmpty(Options(onMine!));
        Assert.All(Options(onMine!), o => Assert.True(o.IsEnabled));

        var log = LogItem(onMine!);

        Assert.NotNull(log);
        Assert.Equal(LogHeader, Header(log!));

        // **IT TRANSMITS NOTHING**, which is checked by what it carries rather
        // than by trusting the header: a send item hands the command a message,
        // and this hands it the row.
        Assert.IsType<DigitalDecodeRow>(log!.CommandParameter);
        Assert.Same(mine, log.CommandParameter);

        // ---- the left side -------------------------------------------------
        var onLeft = RightClickRow(scene, r => ReferenceEquals(r, left));

        Assert.NotNull(onLeft);

        _output.WriteLine("");
        _output.WriteLine("right-click on the left row (" + left.Message + "):");

        foreach (var header in Headers(onLeft!))
        {
            _output.WriteLine("    " + header);
        }

        // **A CQ IS NOT A CONTACT**, so there is nothing to log and the item is
        // absent rather than present and disabled — grey is reserved for what
        // genuinely cannot be used (§0.5.1).
        Assert.Null(LogItem(onLeft!));
        Assert.DoesNotContain(LogHeader, Headers(onLeft!));

        scene.Window.Close();
    }

    /// <summary>
    /// A third-party exchange on the left has a menu and no Log.
    /// </summary>
    /// <remarks>
    /// **TWO OTHER STATIONS WORKING EACH OTHER ARE NOT HIS CONTACT**, so the menu
    /// still offers what he could send them and offers nothing to write down.
    /// </remarks>
    [AvaloniaFact]
    public async Task AThirdPartyExchangeHasAMenuAndNoLog()
    {
        var scene = await SceneAsync();

        Add(scene.Panel, 40, "K9TC KJ6IX RRR");

        Pump(scene.Window);

        var row = scene.Panel.DigitalVisibleDecodes.Single(
            r => r.Message == "K9TC KJ6IX RRR");

        var flyout = RightClickRow(scene, r => ReferenceEquals(r, row));

        Assert.NotNull(flyout);

        _output.WriteLine("right-click on K9TC KJ6IX RRR:");

        foreach (var header in Headers(flyout!))
        {
            _output.WriteLine("    " + header);
        }

        // The menu is there and it offers messages, because nothing is forbidden.
        Assert.NotEmpty(Options(flyout!));

        // And there is nothing of his to log.
        Assert.Null(LogItem(flyout!));

        scene.Window.Close();
    }

    /// <summary>What unit 274's Log item reads as.</summary>
    private const string LogHeader = "Log this contact...";

    /// <summary>What the operator reads, item by item, in the order shown.</summary>
    private static List<string> Headers(MenuFlyout flyout)
        => flyout.Items.OfType<MenuItem>().Select(Header).ToList();

    /// <summary>The clickable messages - everything that would transmit.</summary>
    /// <remarks>
    /// **A SEND OPTION IS DISCRIMINATED BY WHAT IT CARRIES, NOT BY POSITION.**
    /// Every send item hands `SendMessageCommand` the message text, so its
    /// parameter is a `string`; unit 274's Log item hands `LogContactCommand` the
    /// row, so its parameter is a `DigitalDecodeRow`. **That distinction is the
    /// one this file exists to police**: this unit adds a second place to click
    /// and must not add a second way to transmit, and counting "items with a
    /// command" would have blurred exactly that.
    /// </remarks>
    private static List<MenuItem> Options(MenuFlyout flyout)
        => flyout.Items.OfType<MenuItem>()
            .Where(i => i.Command is not null && i.CommandParameter is string)
            .ToList();

    /// <summary>The Log item, or null where the menu does not offer one.</summary>
    /// <remarks>
    /// **IT TRANSMITS NOTHING**, and it is found by its parameter being the row
    /// rather than a message - see <see cref="Options"/>.
    /// </remarks>
    private static MenuItem? LogItem(MenuFlyout flyout)
        => flyout.Items.OfType<MenuItem>()
            .FirstOrDefault(i => i.CommandParameter is DigitalDecodeRow);

    /// <summary>The notes - everything that says something and cannot be clicked.</summary>
    private static List<MenuItem> Notes(MenuFlyout flyout)
        => flyout.Items.OfType<MenuItem>().Where(i => i.Command is null).ToList();

    private static string Header(MenuItem item)
        => item.Header as string ?? "";

    /// <summary>Runs the dispatcher and lays the window out.</summary>
    private static void Pump(Window window)
    {
        for (var i = 0; i < 5; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    private static void Add(MainWindowViewModel panel, int slot, string message)
    {
        var utc = SlotZero.AddSeconds(slot * 15);

        panel.AddDecodeRowForTests(
            utc.ToString("HHmmss", CultureInfo.InvariantCulture),
            Report, "0.2", "1240", message, utc);
    }
}
