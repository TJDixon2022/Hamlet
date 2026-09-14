using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 355 task 1: **Stop lives in the status bar, always** (Tim, 2026-09-14, ruling A).
/// </summary>
/// <remarks>
/// <para>**WHY IT MOVED.** Unit 354 measured the main window at nine sizes and found
/// <c>DigitalStopButton</c> drawn below the window at 1100 x 780, the size Hamlet opens at
/// (<c>docs/unit349-what-tim-looks-at.md</c> section 4 item 28). It was in the send area in the tab row,
/// and the tab row is inside the canvas row, which gives up its height first. The status bar is the
/// root grid's last <c>Auto</c> row and never scrolls or collapses.</para>
/// <para>**PRESSABLE AT REST, NOT DISABLED - THE ONE PLACE THIS DEPARTS FROM THE INSTRUCTION.** Task 1
/// asks for the control disabled with nothing keyed. <c>docs/phase-send/PHASE_PLAN.md</c> step 1 says
/// the abort *cannot be disabled, deferred, or made conditional* (must-pass), and
/// <c>TheOperatorCanStopItTests.TheStopIsOnScreenAndPressableBeforeAnythingHappens</c> guards it; a
/// press before the slot boundary is also how an armed send is taken off, and nothing is keyed then.
/// The tree's rule is kept and the clause is raised for Tim in unit 355's report, section 4. What
/// changes with a send is the word on the face and the <c>hm-live</c> class, as before.</para>
/// <para>**NOTHING IS OPENED** (<c>SHACK_FACTS.md</c> FACT-004): <see cref="FakePort"/> and
/// <see cref="FakeSink"/>. Nothing is pressed on the layout windows (§0.2).</para>
/// </remarks>
public sealed class TheStopIsAlwaysOnScreenTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fixture.</summary>
    /// <param name="output">Where each window's numbers are printed.</param>
    public TheStopIsAlwaysOnScreenTests(ITestOutputHelper output) => _output = output;

    /// <summary>FT8's watering hole on 20 m, where a General may send data.</summary>
    private const long Ft8On20m = 14_074_000;

    /// <summary>The abort's first frame.</summary>
    private const string CwStop = "FE FE 94 E0 17 FF FD";

    /// <summary>The abort's second frame, and the ordinary unkey.</summary>
    private const string PttOff = "FE FE 94 E0 1C 00 00 FD";

    /// <summary>What the radio sees when it is keyed.</summary>
    private const string KeyOn = "FE FE 94 E0 1C 00 01 FD";

    /// <summary>Unit 354's nine sizes, as <c>TheTopRowTests.Unit354TraceTheMainWindowAtTheSizesTimCanOpen</c> lists them.</summary>
    private static readonly (double Width, double Height)[] Sizes =
    {
        (1920, 1040), (900, 620), (1100, 780), (1280, 720), (1366, 728),
        (1536, 824), (1400, 1040), (1920, 1017), (2560, 1400),
    };

    /// <summary>
    /// **At each of the nine sizes, on FT8 and PSK31, Stop is in the status bar and whole on the window**,
    /// the bar's height, right of the bar's centre, in no collapsible panel, the only button bound to the
    /// stop command, and the bar no taller for it.
    /// </summary>
    [AvaloniaFact]
    public void AtEachOf354sNineSizesStopIsInTheStatusBarAndOnTheWindow()
    {
        var misses = new List<string>();

        foreach (var mode in new[] { "FT8", "PSK31" })
        {
            foreach (var (width, height) in Sizes)
            {
                var window = TheTopRowTests.Realized(width, height, null, null);

                try
                {
                    ((MainWindowViewModel)window.DataContext!).ChosenDigitalMode = mode;
                    Pump(window);
                    misses.AddRange(Read(window, mode + " " + Size(width, height) + " licensed"));
                }
                finally
                {
                    window.Close();
                }
            }
        }

        // **THE PLAIN WINDOW AT THE TWO SIZES 354 READ IT AT**, and on the other two tabs at the opening
        // size: the bar is the window's, not the Digital tab's, so Stop is there whatever tab is up.
        foreach (var (width, height, tab) in new[] { (900.0, 620.0, "Digital"), (1100.0, 780.0, "Digital"), (1100.0, 780.0, "CW"), (1100.0, 780.0, "Voice") })
        {
            var window = TheWorkingPanelsTests.Realized(width, height, null);

            try
            {
                ((MainWindowViewModel)window.DataContext!).OperatingMode = tab;
                Pump(window);
                misses.AddRange(Read(window, tab + " tab " + Size(width, height) + " plain"));
            }
            finally
            {
                window.Close();
            }
        }

        Assert.True(misses.Count == 0, string.Join(Environment.NewLine, misses));
    }

    /// <summary>
    /// **With nothing armed and nothing keyed, at the opening size, Stop says Stop, is not live, and is
    /// pressable where it is drawn** - the tree's step 1 rule, kept.
    /// </summary>
    [AvaloniaFact]
    public void WithNothingKeyedItSaysStopAndIsStillPressable()
    {
        var scene = Scene(1100, 780);

        try
        {
            var stop = StopButton(scene.Window);
            var centre = Centre(stop, scene.Window);
            var hit = scene.Window.InputHitTest(centre) as Visual;

            _output.WriteLine("1100 x 780, nothing armed: [" + stop.Content + "] live " + stop.Classes.Contains("hm-live")
                + ", enabled " + stop.IsEffectivelyEnabled + ", can execute " + (stop.Command?.CanExecute(null) ?? false)
                + ", centre " + centre + ", hit " + (hit?.GetType().Name ?? "(nothing)"));

            Assert.Equal("Stop", stop.Content);
            Assert.DoesNotContain("hm-live", stop.Classes);
            Assert.False(scene.Panel.HasSomethingToStop);
            Assert.True(stop.IsEffectivelyEnabled);
            Assert.True(stop.Command!.CanExecute(null));
            Assert.Same(scene.Panel.StopSendingCommand, stop.Command);
            Assert.True(
                hit is not null && (ReferenceEquals(hit, stop) || hit.GetVisualAncestors().Contains(stop)),
                "a click at Stop's centre " + centre + " lands on " + (hit?.GetType().Name ?? "nothing") + ", not on Stop");
        }
        finally
        {
            scene.Window.Close();
        }
    }

    /// <summary>
    /// **Armed and waiting for its slot, at the opening size: Stop is live, and a real click on it in the
    /// status bar un-arms the send and puts the abort on the wire** - what <c>StopNow</c> did from the send
    /// area, word for word and frame for frame.
    /// </summary>
    [AvaloniaFact]
    public async Task ArmedAtTheOpeningSizeAClickOnTheBarUnarmsItAsBefore()
    {
        var scene = Scene(1100, 780);

        try
        {
            scene.Panel.SendMessageCommand.Execute("W1ABC KC3QIS -10");
            Pump(scene.Window);

            Assert.NotNull(scene.Panel.ArmedForSlotUtc);
            var slot = scene.Panel.ArmedForSlotUtc!.Value;
            var stop = StopButton(scene.Window);

            _output.WriteLine("armed: [" + stop.Content + "] live " + stop.Classes.Contains("hm-live") + " at " + Box(TheTopRowTests.RectIn(stop, scene.Window)));

            Assert.Equal("Stop transmitting", stop.Content);
            Assert.Contains("hm-live", stop.Classes);
            Assert.True(stop.IsEffectivelyEnabled);

            Click(scene.Window, stop);

            _output.WriteLine("after the click: " + scene.Panel.DigitalSendLine);
            _output.WriteLine("wire: " + string.Join(" | ", Frames(scene.Port)));

            Assert.Null(scene.Panel.ArmedForSlotUtc);
            Assert.Contains("will not go out", scene.Panel.DigitalSendLine, StringComparison.Ordinal);
            Assert.Contains("the radio was told to stop transmitting", scene.Panel.DigitalSendLine, StringComparison.Ordinal);
            Assert.Equal(new[] { CwStop, PttOff }, Frames(scene.Port));

            var boundary = await scene.Panel.AtSlotBoundaryAsync(slot);

            Assert.Equal(Ft8ArmOutcome.NothingArmed, boundary!.Outcome);
            Assert.True(scene.Sink.WasNeverTouched);
            Assert.DoesNotContain(KeyOn, Frames(scene.Port));
        }
        finally
        {
            scene.Window.Close();
        }
    }

    /// <summary>
    /// **Keyed and mid-transmission, at the opening size: Stop is enabled in the status bar, and a real click
    /// fires the abort while the tones are still going and stops the sound.**
    /// </summary>
    [AvaloniaFact]
    public async Task KeyedAtTheOpeningSizeAClickOnTheBarFiresTheAbortWhileItRuns()
    {
        var scene = Scene(1100, 780);

        try
        {
            scene.Sink.PlaysOver = TimeSpan.FromSeconds(20);
            scene.Panel.SendMessageCommand.Execute("W1ABC KC3QIS -10");

            var slot = scene.Panel.ArmedForSlotUtc!.Value;
            var running = scene.Panel.AtSlotBoundaryAsync(slot);

            Assert.True(scene.Sink.Entered.Wait(TimeSpan.FromSeconds(10)), "the sink was never reached");

            var stop = StopButton(scene.Window);

            _output.WriteLine("keyed: [" + stop.Content + "] enabled " + stop.IsEffectivelyEnabled + " at " + Box(TheTopRowTests.RectIn(stop, scene.Window))
                + "; wire " + string.Join(" | ", Frames(scene.Port)));

            Assert.True(stop.IsEffectivelyVisible);
            Assert.True(stop.IsEffectivelyEnabled);
            Assert.Equal(new[] { KeyOn }, Frames(scene.Port));

            Click(scene.Window, stop);

            var whileRunning = Frames(scene.Port);

            _output.WriteLine("wire while running: " + string.Join(" | ", whileRunning));

            Assert.Equal(new[] { KeyOn, CwStop, PttOff }, whileRunning);

            await running;
            Pump(scene.Window);

            _output.WriteLine("sound stopped by the token " + scene.Sink.StoppedByTheToken + ", " + scene.Sink.PlayedSoFar + " samples played");

            Assert.True(scene.Sink.StoppedByTheToken, "the sound ran to its end after the click");
        }
        finally
        {
            scene.Window.Close();
        }
    }

    /// <summary>Reads one window and returns what does not hold, printing every number.</summary>
    private List<string> Read(Window window, string label)
    {
        var model = (MainWindowViewModel)window.DataContext!;
        var misses = new List<string>();
        var stop = TheTopRowTests.Named<Button>(window, "DigitalStopButton");
        var bar = TheTopRowTests.Named<Border>(window, "StatusBar");
        var at = TheTopRowTests.RectIn(stop, window);
        var barAt = TheTopRowTests.RectIn(bar, window);
        var inner = barAt.Height - bar.Padding.Top - bar.Padding.Bottom - bar.BorderThickness.Top - bar.BorderThickness.Bottom;
        var bounds = window.Bounds;
        var bound = window.GetVisualDescendants().OfType<Button>().Count(b => ReferenceEquals(b.Command, model.StopSendingCommand));
        var inBar = stop.GetVisualAncestors().Contains(bar);
        var folding = stop.GetVisualAncestors().OfType<CollapsiblePanel>().Count();
        var whole = at.Width > 0 && at.Height > 0 && at.Left >= -0.5 && at.Top >= -0.5 && at.Right <= bounds.Width + 0.5 && at.Bottom <= bounds.Height + 0.5;

        stop.IsVisible = false;
        Pump(window);
        var without = TheTopRowTests.RectIn(bar, window).Height;
        stop.IsVisible = true;
        Pump(window);

        _output.WriteLine(
            label + ": window " + Box(bounds) + "; status bar " + Box(barAt) + " (inside " + Px(inner) + ", " + Px(without) + " tall without Stop); Stop ["
            + stop.Content + "] " + Box(at) + " in the bar " + inBar + ", visible " + stop.IsEffectivelyVisible + ", whole on the window " + whole
            + ", collapsible ancestors " + folding + ", buttons bound to the stop command " + bound);

        void Miss(bool holds, string what)
        {
            if (!holds)
            {
                misses.Add(label + ": " + what);
            }
        }

        Miss(inBar, "Stop is not in the status bar");
        Miss(stop.IsEffectivelyVisible, "Stop is not visible");
        Miss(whole, "Stop " + Box(at) + " is not whole on the " + Box(bounds) + " window");
        Miss(Math.Abs(at.Height - inner) <= 1, "Stop is " + Px(at.Height) + " tall and the bar's inside is " + Px(inner));
        Miss(at.Center.X > barAt.Center.X, "Stop's centre x " + Px(at.Center.X) + " is not right of the bar's " + Px(barAt.Center.X));
        Miss(folding == 0, "Stop is inside a collapsible panel");
        Miss(bound == 1, bound + " buttons are bound to the stop command, not 1");
        Miss(Math.Abs(barAt.Height - without) <= 0.5, "the bar is " + Px(barAt.Height) + " with Stop and " + Px(without) + " without it");

        return misses;
    }

    /// <summary>The window, the panel and the fakes behind them.</summary>
    private sealed record Built(MainWindow Window, MainWindowViewModel Panel, FakePort Port, FakeSink Sink);

    /// <summary>
    /// The real window on the Digital tab at <paramref name="width"/> x <paramref name="height"/>, a General
    /// on 20 m FT8 with a fake radio and a fake sink, the shape of <c>TheOperatorCanStopItTests.Scene</c>
    /// with the network sources off as <c>TheTopRowTests</c> has them.
    /// </summary>
    private static Built Scene(double width, double height)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = "FN00";
        settings.Operator.LicenseClass = LicenseClass.General;

        foreach (var name in TheTopRowTests.NetworkSources)
        {
            settings.SetSourceEnabled(name, false);
        }

        var panel = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalDecodedExpanded = true,
        };

        var window = new MainWindow { DataContext = panel, Width = width, Height = height };

        window.Show();
        Pump(window);

        panel.SelectedBand = panel.Bands.First(b => b.Band.LowHz <= Ft8On20m && b.Band.HighHz >= Ft8On20m);
        panel.FrequencyHz = Ft8On20m;

        var port = new FakePort();
        var sink = new FakeSink();

        panel.UseRigPortForTests(port);
        panel.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(port, sink)));
        Pump(window);

        return new Built(window, panel, port, sink);
    }

    /// <summary>The realized stop button.</summary>
    private static Button StopButton(Window window)
    {
        Pump(window);

        return TheTopRowTests.Named<Button>(window, "DigitalStopButton");
    }

    /// <summary>A control's centre in window coordinates.</summary>
    private static Point Centre(Control control, Window window)
        => TheTopRowTests.RectIn(control, window).Center;

    /// <summary>
    /// A real left click at the button's place on the window, refused where that place is off the window,
    /// because a mouse cannot reach it there.
    /// </summary>
    private static void Click(Window window, Button button)
    {
        Pump(window);

        var centre = Centre(button, window);

        Assert.True(
            centre.X >= 0 && centre.Y >= 0 && centre.X <= window.Bounds.Width && centre.Y <= window.Bounds.Height,
            "Stop's centre " + centre + " is off the " + Box(window.Bounds) + " window, where no mouse can press it");

        window.MouseMove(centre);
        window.MouseDown(centre, MouseButton.Left);
        window.MouseUp(centre, MouseButton.Left);

        Pump(window);
    }

    private static string[] Frames(FakePort port)
        => port.Written.Select(f => string.Join(' ', f.Select(b => b.ToString("X2", CultureInfo.InvariantCulture)))).ToArray();

    /// <summary>Runs the dispatcher and lays the window out.</summary>
    private static void Pump(Window window)
    {
        for (var i = 0; i < 5; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    private static string Size(double width, double height)
        => Px(width) + " x " + Px(height);

    private static string Px(double value)
        => value.ToString("0.#", CultureInfo.InvariantCulture);

    private static string Box(Rect r)
        => Px(r.X) + "," + Px(r.Y) + " " + Px(r.Width) + " x " + Px(r.Height);
}
