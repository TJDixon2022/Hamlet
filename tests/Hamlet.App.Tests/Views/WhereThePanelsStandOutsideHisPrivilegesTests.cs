using System.Globalization;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Licensing;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>Where one of the three top-row panels stands, in window coordinates.</summary>
/// <param name="Left">Its left edge.</param>
/// <param name="Top">Its top edge.</param>
/// <param name="Width">Its width.</param>
/// <param name="Height">Its height.</param>
internal sealed record PanelBounds(double Left, double Top, double Width, double Height);

/// <summary>What the window showed at one frequency: the three panels, the privilege panel and the row's decision.</summary>
/// <param name="FrequencyHz">Where the model says the dial is.</param>
/// <param name="Card">The neighborhood card, `TopRowCardScroller`.</param>
/// <param name="Map">The sun map, `GreenZoneMap`.</param>
/// <param name="Rig">The rig face, `BandRow`'s third child.</param>
/// <param name="Tone">The privilege panel's tone.</param>
/// <param name="Background">The green block's background as drawn.</param>
/// <param name="Sentences">Every sentence the green block draws, with its drawn and its wanted width.</param>
/// <param name="RowHeight">The rig face's desired height, which sets the row.</param>
/// <param name="MapHeight">The map height `BandGovernsTheMapPanel` set.</param>
/// <param name="AtTheLeftEdge">Whether the map was laid out at the band's left edge.</param>
/// <param name="PillsReach">How far above the row the map reaches.</param>
/// <param name="CardSlot">The width the card was offered, its margin counted.</param>
/// <param name="GaveBack">The map width given back to the card, against the map at the row's height.</param>
/// <param name="CardExtent">The card's content height, against which the scroller's viewport is set.</param>
/// <param name="CardViewport">The card scroller's viewport height.</param>
internal sealed record TopRowReading(
    long FrequencyHz,
    PanelBounds Card,
    PanelBounds Map,
    PanelBounds Rig,
    PrivilegeTone Tone,
    string Background,
    IReadOnlyList<string> Sentences,
    double RowHeight,
    double MapHeight,
    bool AtTheLeftEdge,
    double PillsReach,
    double CardSlot,
    double GaveBack,
    double CardExtent,
    double CardViewport);

/// <summary>
/// **THE TOP ROW, TUNED THE WAY HIS HAND TUNES IT** (work instruction 423, step 6 criterion 6.3).
/// </summary>
/// <remarks>
/// <para>**THE TUNE IS THE RIG FACE'S OWN WRITE.** Rolling the wheel over a digit sets
/// <see cref="RigDisplayControl.FrequencyHzProperty"/> with <c>SetCurrentValue</c>; the property binds
/// two-way to <c>MainWindowViewModel.FrequencyHz</c>, whose change handler runs <c>UpdateModeLine</c> and
/// so <c>UpdatePrivileges</c>. This writes the same property the same way, so the panel's words change
/// the way they change under his hand.</para>
/// <para>**THE CLASS IS HIS SETTING**, <c>AppSettings.Operator.LicenseClass</c>, and the lookup that
/// confirms it at construction is handed the same answer, so nothing reaches the network.</para>
/// </remarks>
internal static class TheTopRowTuned
{
    /// <summary>Covered: 14.050 MHz, General's 20 m row 14025000 to 14150000, 97.301(d).</summary>
    public const long Covered = 14_050_000;

    /// <summary>Not covered: 14.010 MHz, on 20 m in Extra's row 14000000 to 14350000 only, 97.301(b).</summary>
    public const long NotCovered = 14_010_000;

    /// <summary>Outside amateur spectrum altogether, the <c>OutOfBandTests</c> case.</summary>
    public const long OutOfBand = 14_360_000;

    /// <summary>The size <c>MainWindow.axaml</c> opens at.</summary>
    public static readonly (double Width, double Height) DefaultSize = (1100, 780);

    /// <summary>Opens the main window on the CW tab on 20 m for an operator of <paramref name="cls"/>.</summary>
    public static (Window Window, MainWindowViewModel Panel) Open(double width, double height, LicenseClass cls)
        => Open(width, height, cls, "CW");

    /// <summary>Opens the main window on the <paramref name="mode"/> tab on 20 m for an operator of <paramref name="cls"/>.</summary>
    public static (Window Window, MainWindowViewModel Panel) Open(double width, double height, LicenseClass cls, string mode)
    {
        var settings = TheTopRowTests.FixtureSettings();
        settings.Operator.LicenseClass = cls;

        var lookup = new FixedLicenseLookup(
            new Dictionary<string, LicenseClass>(StringComparer.OrdinalIgnoreCase) { ["KC3QIS"] = cls });

        var panel = new MainWindowViewModel(settings, null, lookup)
        {
            OperatingMode = mode,
        };

        panel.SelectBandCommand.Execute(panel.Bands.First(b => b.Band.Name == "20 m"));

        var window = new MainWindow { DataContext = panel, Width = width, Height = height };

        window.Show();
        Settle(window);

        return (window, panel);
    }

    /// <summary>Lets the layout settle.</summary>
    public static void Settle(Window window)
    {
        for (var i = 0; i < 6; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    /// <summary>The rig face's frequency readout.</summary>
    public static RigDisplayControl Face(Window window)
        => window.GetVisualDescendants().OfType<RigDisplayControl>().First();

    /// <summary>Tunes by the rig face's own write, then lets the layout settle.</summary>
    public static void Tune(Window window, long hz)
    {
        Face(window).SetCurrentValue(RigDisplayControl.FrequencyHzProperty, hz);
        Settle(window);
    }

    /// <summary>
    /// Tunes as <see cref="Tune"/> does, but returns the layout's complaint instead of throwing it, so
    /// a trace can print a size where the layout never settles and go on to the next.
    /// </summary>
    public static string? TryTune(Window window, long hz)
    {
        try
        {
            Tune(window, hz);

            return null;
        }
        catch (Exception e)
        {
            var inner = e is AggregateException all ? all.Flatten().InnerExceptions[0] : e;

            try
            {
                Settle(window);
            }
            catch (Exception)
            {
                // The complaint above is the one reported.
            }

            return inner.GetType().Name + ": " + inner.Message;
        }
    }

    /// <summary>Reads the three panels, the green block and the row's decision off the laid-out window.</summary>
    public static TopRowReading Read(Window window, MainWindowViewModel panel)
    {
        var row = window.GetVisualDescendants().OfType<BandGovernsTheMapPanel>().First(p => p.Name == "BandRow");
        var card = (Control)row.Children[0];
        var map = (Control)row.Children[1];
        var rig = (Control)row.Children[2];
        var scroller = (ScrollViewer)card;
        var picture = map.GetVisualDescendants().OfType<GrayLineMapControl>().First();

        var block = window.GetVisualDescendants().OfType<Border>().First(b => b.Name == "GreenZoneBlock");
        var sentences = new List<string>();

        foreach (var text in block.GetVisualDescendants().OfType<TextBlock>())
        {
            if (!text.IsEffectivelyVisible || string.IsNullOrEmpty(text.Text))
            {
                continue;
            }

            sentences.Add(
                (text.Name is { Length: > 0 } name ? name : "(unnamed)")
                + " \"" + text.Text + "\" drawn " + Px(text.Bounds.Width) + " x " + Px(text.Bounds.Height)
                + ", wants " + Px(Wanted(text)) + " on one line");
        }

        var rowHeight = rig.DesiredSize.Height;
        var aspect = picture.Bounds.Height > 0 ? picture.Bounds.Width / picture.Bounds.Height : 0;
        var natural = aspect * (row.MapIsAtTheLeftEdge ? rowHeight + row.PillsReach : Math.Max(BandGovernsTheMapPanel.MapFloor, rowHeight));

        return new TopRowReading(
            panel.FrequencyHz,
            Where(card, window),
            Where(map, window),
            Where(rig, window),
            panel.PrivilegeStatus.Tone,
            block.Background is ISolidColorBrush solid ? solid.Color.ToString() : block.Background?.ToString() ?? "none",
            sentences,
            rowHeight,
            row.MapHeight,
            row.MapIsAtTheLeftEdge,
            row.PillsReach,
            card.Bounds.Width + card.Margin.Left + card.Margin.Right,
            natural - picture.Bounds.Width,
            scroller.Extent.Height,
            scroller.Viewport.Height);
    }

    /// <summary>Where a control stands in window coordinates.</summary>
    public static PanelBounds Where(Visual visual, Visual window)
    {
        var at = visual.TranslatePoint(default, window) ?? default;

        return new PanelBounds(at.X, at.Y, visual.Bounds.Width, visual.Bounds.Height);
    }

    /// <summary>One panel's bounds, to a tenth of a pixel.</summary>
    public static string Box(PanelBounds b)
        => "left " + Px(b.Left) + ", top " + Px(b.Top) + ", width " + Px(b.Width) + ", height " + Px(b.Height);

    /// <summary>A pixel figure to a tenth.</summary>
    public static string Px(double value)
        => value.ToString("0.0", CultureInfo.InvariantCulture);

    /// <summary>What a sentence wants on one line, measured on a detached copy so the window is not disturbed.</summary>
    private static double Wanted(TextBlock text)
    {
        var copy = new TextBlock
        {
            Text = text.Text,
            FontSize = text.FontSize,
            FontWeight = text.FontWeight,
            FontFamily = text.FontFamily,
            FontStyle = text.FontStyle,
        };

        copy.Measure(Size.Infinity);

        return copy.DesiredSize.Width;
    }
}

/// <summary>
/// **WHERE THE PANELS STAND OUTSIDE HIS PRIVILEGES - A FACT THAT ASSERTS NOTHING** (work instruction
/// 423 task 1).
/// </summary>
/// <remarks>
/// Tim reported it at the radio (R62): tuned to a frequency his license does not cover, the window
/// rearranges itself. This prints, at every size the existing top-row tests use and at the size the
/// window opens at, the three panels' bounds at 14.050 and at 14.010 MHz for a General operator on
/// the CW tab, what the privilege panel says and wants, what <see cref="BandGovernsTheMapPanel"/>
/// decided, and the difference. Then it widens the search: every class, out of band, and connected
/// to the training radio.
/// </remarks>
public sealed class WhereThePanelsStandOutsideHisPrivilegesTests
{
    /// <summary>Every size an existing top-row test measures at, and the size the window opens at.</summary>
    public static readonly (double Width, double Height)[] Sizes =
    {
        (1100, 780), (1920, 1040), (1400, 1040), (900, 620), (1280, 720), (1366, 728), (1536, 824),
        (1920, 1017), (2560, 1400), (1200, 900), (1400, 900), (1100, 620), (1100, 580), (1100, 540), (1100, 500),
    };

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fact.</summary>
    /// <param name="output">Where the trace is printed.</param>
    public WhereThePanelsStandOutsideHisPrivilegesTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The trace. Asserts nothing.</summary>
    [AvaloniaFact]
    public async Task TraceThePanelsAtBothFrequencies()
    {
        var summary = new StringBuilder();

        foreach (var mode in new[] { "CW", "Digital" })
        {
            foreach (var (width, height) in Sizes)
            {
                summary.AppendLine(Trace(width, height, mode));
            }
        }

        _output.WriteLine("==== THE DIFFERENCE, 14.050 TO 14.010, GENERAL, NOT CONNECTED");
        _output.WriteLine(summary.ToString());

        // **WIDER: EVERY CLASS THE PRIVILEGES FILE NAMES, AND OUT OF BAND**, at the size it opens at.
        _output.WriteLine("==== WIDER, AT 1100 x 780: EVERY CLASS, 14.050 AGAINST 14.010 AND AGAINST 14.360 (OUT OF BAND)");

        foreach (var cls in new[] { LicenseClass.Novice, LicenseClass.Technician, LicenseClass.General, LicenseClass.Advanced, LicenseClass.Extra, LicenseClass.Unknown })
        {
            var (window, panel) = TheTopRowTuned.Open(TheTopRowTuned.DefaultSize.Width, TheTopRowTuned.DefaultSize.Height, cls);

            var ca = TheTopRowTuned.TryTune(window, TheTopRowTuned.Covered);
            var a = TheTopRowTuned.Read(window, panel);
            var cb = TheTopRowTuned.TryTune(window, TheTopRowTuned.NotCovered);
            var b = TheTopRowTuned.Read(window, panel);
            var cc = TheTopRowTuned.TryTune(window, TheTopRowTuned.OutOfBand);
            var c = TheTopRowTuned.Read(window, panel);

            _output.WriteLine($"-- class {cls} (the model reads {panel.LicenseClass}); tones {a.Tone}, {b.Tone}, {c.Tone}{Said(ca)}{Said(cb)}{Said(cc)}");
            _output.WriteLine("   " + Difference("14.050 -> 14.010", a, b));
            _output.WriteLine("   " + Difference("14.050 -> 14.360", a, c));
            Print($"{cls} at 14.360 (dial reads {c.FrequencyHz})", c);

            window.Close();
        }

        // **WIDER: CONNECTED TO THE TRAINING RADIO**, through ToggleConnectCommand, at the size it opens at
        // and at 1920 x 1040.
        foreach (var (width, height) in new[] { TheTopRowTuned.DefaultSize, (1920.0, 1040.0) })
        {
            var (window, panel) = TheTopRowTuned.Open(width, height, LicenseClass.General);

            await TheControlsTimCanPress.ConnectAsync(window, panel);
            _output.WriteLine($"==== CONNECTED at {width} x {height} to {panel.SelectedPort}: IsConnected {panel.IsConnected}, dial {panel.FrequencyHz}, band {panel.SelectedBand.Band.Name}");

            // The training radio answers on 40 m; the band pill is pressed back to 20 m, as he would.
            panel.SelectBandCommand.Execute(panel.Bands.First(b => b.Band.Name == "20 m"));
            await Wait(window);
            _output.WriteLine($"   after pressing 20 m: dial {panel.FrequencyHz}, band {panel.SelectedBand.Band.Name}");

            var ca = TheTopRowTuned.TryTune(window, TheTopRowTuned.Covered);
            await Wait(window);
            var a = TheTopRowTuned.Read(window, panel);
            var cb = TheTopRowTuned.TryTune(window, TheTopRowTuned.NotCovered);
            await Wait(window);
            var b = TheTopRowTuned.Read(window, panel);

            Print("connected, covered" + Said(ca), a);
            Print("connected, not covered" + Said(cb), b);
            _output.WriteLine(Difference($"connected {width} x {height}", a, b));

            await TheControlsTimCanPress.DisconnectAsync(window, panel);
            window.Close();
        }
    }

    /// <summary>One size on one tab: covered, not covered, covered again; returns the difference line.</summary>
    private string Trace(double width, double height, string mode)
    {
        var label = $"{mode} {width} x {height}";

        _output.WriteLine($"==== {label}, General");

        try
        {
            var (window, panel) = TheTopRowTuned.Open(width, height, LicenseClass.General, mode);

            var complaint = TheTopRowTuned.TryTune(window, TheTopRowTuned.Covered);
            var covered = TheTopRowTuned.Read(window, panel);
            Print("covered 14.050" + Said(complaint), covered);

            complaint = TheTopRowTuned.TryTune(window, TheTopRowTuned.NotCovered);
            var outside = TheTopRowTuned.Read(window, panel);
            Print("not covered 14.010" + Said(complaint), outside);

            complaint = TheTopRowTuned.TryTune(window, TheTopRowTuned.Covered);
            var back = TheTopRowTuned.Read(window, panel);
            Print("back to 14.050" + Said(complaint), back);

            window.Close();

            return Difference(label, covered, outside);
        }
        catch (Exception e)
        {
            var inner = e is AggregateException all ? all.Flatten().InnerExceptions[0] : e;
            _output.WriteLine($"  the window could not be traced: {inner.GetType().Name}: {inner.Message}");

            return label + ": not traced - " + inner.Message;
        }
    }

    private static string Said(string? complaint)
        => complaint is null ? "" : " [THE LAYOUT SAID: " + complaint + "]";

    /// <summary>Gives the rig timers a moment, settling as it goes.</summary>
    private static async Task Wait(Window window)
    {
        for (var i = 0; i < 10; i++)
        {
            await Task.Delay(50);

            try
            {
                TheTopRowTuned.Settle(window);
            }
            catch (Exception)
            {
                // The tune's own complaint is the one reported.
            }
        }
    }

    private void Print(string label, TopRowReading r)
    {
        _output.WriteLine($"  -- {label}: dial {r.FrequencyHz}, tone {r.Tone}, block background {r.Background}");
        _output.WriteLine("     card " + TheTopRowTuned.Box(r.Card));
        _output.WriteLine("     map  " + TheTopRowTuned.Box(r.Map));
        _output.WriteLine("     rig  " + TheTopRowTuned.Box(r.Rig));
        _output.WriteLine(
            "     row: rig height " + TheTopRowTuned.Px(r.RowHeight) + ", map height set " + TheTopRowTuned.Px(r.MapHeight)
            + ", at the left edge " + r.AtTheLeftEdge + ", pills reach " + TheTopRowTuned.Px(r.PillsReach)
            + ", card slot offered " + TheTopRowTuned.Px(r.CardSlot) + ", map gave back " + TheTopRowTuned.Px(r.GaveBack)
            + ", card content " + TheTopRowTuned.Px(r.CardExtent) + " in a viewport of " + TheTopRowTuned.Px(r.CardViewport));

        foreach (var s in r.Sentences)
        {
            _output.WriteLine("       " + s);
        }
    }

    private static string Difference(string label, TopRowReading a, TopRowReading b)
    {
        string One(string name, PanelBounds x, PanelBounds y)
            => $"{name} left {TheTopRowTuned.Px(y.Left - x.Left)} width {TheTopRowTuned.Px(y.Width - x.Width)} top {TheTopRowTuned.Px(y.Top - x.Top)} height {TheTopRowTuned.Px(y.Height - x.Height)}";

        return label + ": " + One("card", a.Card, b.Card) + "; " + One("map", a.Map, b.Map) + "; " + One("rig", a.Rig, b.Rig)
            + $"; map height {TheTopRowTuned.Px(a.MapHeight)} -> {TheTopRowTuned.Px(b.MapHeight)}, left edge {a.AtTheLeftEdge} -> {b.AtTheLeftEdge}"
            + $", card content {TheTopRowTuned.Px(a.CardExtent)} -> {TheTopRowTuned.Px(b.CardExtent)}, tone {a.Tone} -> {b.Tone}";
    }
}
