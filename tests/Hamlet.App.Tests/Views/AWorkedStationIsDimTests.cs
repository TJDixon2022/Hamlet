using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 279, task 1: a station already in the log fades, and the word
/// `worked` is gone.
/// </summary>
/// <remarks>
/// <para>**TIM'S RULING, 2026-09-08**, replacing unit 274's green `worked` beside
/// the callsign: dim the whole row and delete the word.</para>
/// <para>**FADED IS NOT DISABLED, AND THIS CLASS ASSERTS BOTH HALVES.** The row
/// keeps the full right-click menu with the same items, stays clickable, and nothing
/// about it is forbidden, because working the same station again on another band or
/// another day is his choice.</para>
/// <para>**GREY IS THIS PROJECT'S RESERVED SIGNAL FOR WHAT GENUINELY CANNOT BE
/// USED** (§0.5.1, HM-DEC-087), which is why the mechanism is opacity rather than a
/// greyed foreground: opacity keeps every colour the row already has where greying
/// would replace them with the disabled palette.</para>
/// </remarks>
public sealed class AWorkedStationIsDimTests
{
    private const string HisCall = "KC3QIS";

    /// <summary>The station already in the log.</summary>
    private const string Worked = "IK4LZH";

    /// <summary>A station he has never worked.</summary>
    private const string Fresh = "W1ABC";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the rows are printed.</param>
    public AWorkedStationIsDimTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// **A logged callsign fades and an unlogged one does not.**
    /// </summary>
    [AvaloniaFact]
    public void ALoggedCallsignDimsAndAnUnloggedOneDoesNot()
    {
        var scene = Scene();

        var worked = scene.Row(Worked);
        var fresh = scene.Row(Fresh);

        _output.WriteLine(Worked + " opacity " + worked.RowOpacity);
        _output.WriteLine(Fresh + "  opacity " + fresh.RowOpacity);

        Assert.True(worked.HasWorkedBefore);
        Assert.False(fresh.HasWorkedBefore);

        Assert.True(
            worked.RowOpacity < 1.0,
            "a station already in the log is drawn at full strength");

        Assert.Equal(1.0, fresh.RowOpacity);

        // **AND IT IS STILL READABLE.** This is the list he reads callsigns off,
        // and a callsign he has to lean in for is worse than a label he can ignore.
        Assert.True(
            worked.RowOpacity >= 0.5,
            "the fade is deep enough to make a callsign hard to read");
    }

    /// <summary>
    /// **The word `worked` is gone from both lists.**
    /// </summary>
    /// <remarks>
    /// Asserted against the realized window rather than by reading the markup,
    /// because a label left behind in one of the two templates is exactly the kind
    /// of thing a source search finds and a screen does not.
    /// </remarks>
    [AvaloniaFact]
    public void TheWordIsGoneFromBothLists()
    {
        var scene = Scene();

        var labels = scene.Window.GetVisualDescendants()
            .OfType<TextBlock>()
            .Where(t => string.Equals(t.Text, "worked", StringComparison.Ordinal))
            .ToList();

        _output.WriteLine("text blocks reading \"worked\": " + labels.Count);

        Assert.Empty(labels);
    }

    /// <summary>
    /// **A faded row opens the same menu with the same items.**
    /// </summary>
    /// <remarks>
    /// **THIS IS THE HALF THAT MATTERS** (Tim, 2026-09-08: dimmed is not disabled).
    /// A row that faded and then refused to offer a reply would have taken a choice
    /// away from him that is his to make.
    /// </remarks>
    [AvaloniaFact]
    public void AFadedRowOffersEverythingAnUndimmedOneDoes()
    {
        var scene = Scene();

        var onWorked = scene.RightClick(Worked);
        var onFresh = scene.RightClick(Fresh);

        Assert.NotNull(onWorked);
        Assert.NotNull(onFresh);

        var workedItems = Headers(onWorked!);
        var freshItems = Headers(onFresh!);

        _output.WriteLine("menu on the faded " + Worked + ":");

        foreach (var header in workedItems)
        {
            _output.WriteLine("    " + header);
        }

        _output.WriteLine("");
        _output.WriteLine("menu on " + Fresh + ":");

        foreach (var header in freshItems)
        {
            _output.WriteLine("    " + header);
        }

        // **THE SAME OPTIONS, ALLOWING FOR THE CALLSIGN IN EACH.** Comparing the
        // count and the shape rather than the strings, because every message names
        // the station it is addressed to.
        Assert.NotEmpty(workedItems);
        Assert.Equal(freshItems.Count, workedItems.Count);

        // **AND NOTHING IS DISABLED.** A greyed item on a faded row is exactly the
        // confusion this ruling exists to prevent.
        Assert.All(
            onWorked!.Items.OfType<MenuItem>().Where(i => i.Command is not null),
            i => Assert.True(i.IsEnabled, "an item on a faded row is disabled"));
    }

    /// <summary>
    /// **The hover text is on the row, in unit 274's own wording.**
    /// </summary>
    /// <remarks>
    /// Moved onto the row rather than onto a marker that no longer exists, and
    /// **null rather than empty on a row with nothing to say**, because Avalonia
    /// draws an empty tooltip for `""` and a blank box following the pointer down a
    /// list of unworked stations would be worse than the label this replaced.
    /// </remarks>
    [AvaloniaFact]
    public void TheHoverTextIsOnTheRow()
    {
        var scene = Scene();

        var worked = scene.Row(Worked);
        var fresh = scene.Row(Fresh);

        _output.WriteLine(Worked + ": " + worked.WorkedTip);
        _output.WriteLine(Fresh + ":  " + (fresh.WorkedTip ?? "(none)"));

        Assert.NotNull(worked.WorkedTip);

        // **TIM'S OWN WORDING, 2026-09-08**, replacing unit 274's, and asserted as
        // the whole string rather than in pieces because the shape is the ruling.
        // **The band came out with it**: a date is what he wants when a callsign
        // looks familiar, and the band was a third clause on a two-clause hover.
        Assert.Equal(
            "You logged a contact with " + Worked + " on 09/07/26",
            worked.WorkedTip);

        Assert.DoesNotContain("20m", worked.WorkedTip!, StringComparison.Ordinal);

        Assert.Null(fresh.WorkedTip);

        // **AND IT REACHES THE REALIZED ROW**, rather than living only on the
        // record. The grid the template builds is what the pointer is over.
        var grid = scene.Grid(Worked);

        Assert.Equal(worked.WorkedTip, ToolTip.GetTip(grid) as string);
        Assert.Null(ToolTip.GetTip(scene.Grid(Fresh)));
    }

    /// <summary>The clickable option headers, notes excluded.</summary>
    private static System.Collections.Generic.List<string> Headers(MenuFlyout flyout)
        => flyout.Items.OfType<MenuItem>()
            .Where(i => i.CommandParameter is string)
            .Select(i => i.Header as string ?? "")
            .ToList();

    /// <summary>A window with one worked station and one fresh one on the list.</summary>
    /// <remarks>
    /// <para>**THE DIGITAL WORKSPACE IS COLLAPSED UNLESS THAT TAB IS SHOWING**, and
    /// nothing under it is realized while it is, so a right-click would find no row
    /// at all. `TheDecodedColumnsLineUpTests` paid for that line first.</para>
    /// <para>**AND THE ROWS GO IN AFTER `Show`**, because showing raises `Opened`,
    /// which starts the reconnect, which clears the decoded table. Built the other
    /// way round this class found nothing and said `Sequence contains no matching
    /// element`, which is the shape work instruction 279 exists to end.</para>
    /// </remarks>
    private static Built Scene()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = HisCall;
        settings.Operator.GridSquare = "FN00";

        var panel = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalDecodedExpanded = true,
        };

        var window = new MainWindow { DataContext = panel };

        window.Show();
        Pump(window);

        // **THE LOG IS HANDED IN RATHER THAN READ**, because the development
        // machine has no contact log and never will: contacts are logged where the
        // radio is (`SHACK_FACTS.md`, Tim's ruling of 2026-09-08). A test that
        // needed the real file could not run here at all.
        // **BEFORE THE ROWS**, so each is marked as it is placed.
        panel.UseWorkedBeforeForTests(new()
        {
            [Worked] = new AdifContact
            {
                Call = Worked,
                StartedUtc = new DateTime(2026, 9, 7, 21, 41, 30, DateTimeKind.Utc),
                Band = "20m",
                Mode = "FT8",
            },
        });

        // **WITH THEIR SLOTS.** `PlaceRow` feeds the contact ledger only for a row
        // that carries one, and without a ledger record there is no send menu to
        // right-click. Left out, this class reported *produced no menu* and the
        // finder had to say `Ledger record: none` before that was obvious.
        var slot = new DateTime(2026, 9, 8, 21, 41, 30, DateTimeKind.Utc);

        panel.AddDecodeRowForTests(
            "214130", "-11", "0.2", "1240", "CQ " + Worked + " JN54", slot, 14_074_000);
        panel.AddDecodeRowForTests(
            "214130", "-13", "0.3", "1310", "CQ " + Fresh + " FN31", slot, 14_074_000);

        Pump(window);

        return new Built(panel, window);
    }

    /// <summary>Let the layout settle so rows are realized.</summary>
    private static void Pump(Window window)
    {
        window.UpdateLayout();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
    }

    /// <summary>The window, the panel, and the ways of reaching one row.</summary>
    private sealed record Built(MainWindowViewModel Panel, Window Window)
    {
        /// <summary>The row record for one station.</summary>
        /// <remarks>
        /// **IT SAYS WHAT IT WAS LOOKING FOR AND WHAT WAS THERE** (task 5). The
        /// first draft of this helper used `First`, and when it found nothing it
        /// threw `Sequence contains no matching element` from inside LINQ, naming
        /// neither the station nor the lists. That is the shape this whole unit is
        /// about, produced by its own test on the first run.
        /// </remarks>
        public DigitalDecodeRow Row(string station)
        {
            var all = Panel.DigitalVisibleDecodes.Concat(Panel.DigitalMineDecodes).ToList();
            var row = all.FirstOrDefault(r => r.Sender == station);

            Assert.True(
                row is not null,
                "no row whose sender is " + station + ". Left rows: "
                + Panel.DigitalVisibleDecodes.Count + "; mine rows: "
                + Panel.DigitalMineDecodes.Count + "; whole table: "
                + Panel.DigitalDecodes.Count + "; senders seen: ["
                + string.Join(", ", all.Select(r => "\"" + r.Sender + "\"")) + "]");

            return row!;
        }

        /// <summary>The realized grid the template built for that row.</summary>
        public Grid Grid(string station)
        {
            var row = Row(station);

            var grid = Window.GetVisualDescendants()
                .OfType<Grid>()
                .FirstOrDefault(g => ReferenceEquals(g.DataContext, row));

            Assert.True(
                grid is not null,
                "no realized grid for " + station + ". Left rows: "
                + Panel.DigitalVisibleDecodes.Count + "; mine rows: "
                + Panel.DigitalMineDecodes.Count + "; realized grids with a row "
                + "DataContext: "
                + Window.GetVisualDescendants().OfType<Grid>()
                    .Count(g => g.DataContext is DigitalDecodeRow));

            return grid!;
        }

        /// <summary>Raise a real context request on that row's own control.</summary>
        public MenuFlyout? RightClick(string station)
        {
            var grid = Grid(station);

            grid.RaiseEvent(new ContextRequestedEventArgs
            {
                RoutedEvent = Control.ContextRequestedEvent,
                Source = grid,
            });

            var flyout = ((MainWindow)Window).SendFlyoutUnderTheMouse;

            Assert.True(
                flyout is not null,
                "right-clicking " + station + " produced no menu. Ledger record: "
                + (Panel.ContactRecordForTests(station) is null ? "none" : "present")
                + "; send menu: "
                + (Panel.SendMenuFor(Row(station)) is null ? "null" : "built"));

            return flyout;
        }
    }
}
