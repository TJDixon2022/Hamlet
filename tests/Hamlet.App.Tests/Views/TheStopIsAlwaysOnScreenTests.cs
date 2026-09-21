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
    /// <remarks>
    /// <para>**WORK INSTRUCTION 375 TASK 2, UNDER R12: THE MOMENT THE WIRE IS READ IS NOW FIXED.** This name
    /// was red in 3 of 7 runs for unit 372, 1 of 4 for unit 373, 0 of 5 for unit 374 and **4 of 10** for unit
    /// 375, always the same way - the abort pair on the wire twice. It was reading the wire once, immediately
    /// after <c>Click</c>, and asserting an exact snapshot of the list; but <c>Click</c> ends in <c>Pump</c>,
    /// and whether the transmit sequence's own teardown has been pumped by the time that read happens is a
    /// race. It was a timing assertion written as an equality.</para>
    /// <para>**WHAT UNIT 375's TRACE MEASURED** (<see cref="Unit375TraceTests.WhoseSecondAbortPairIsIt"/>,
    /// eight runs, with the stack recorded behind every frame). **The second pair is the sequence's, 8 of 8**:
    /// every frame of it was written by <c>Ft8TransmitSequence.RunAsync</c> -> <c>PlayOverTimeAsync</c> ->
    /// <c>TransmitAbort.Fire</c>, coming off the token the click cancelled. The click's own pair is written
    /// by <c>MainWindowViewModel.StopSending</c> -> <c>Ft8ArmedSend.StopNow</c> -> <c>TransmitAbort.Fire</c>,
    /// same-thread and no-await (CLAUDE.md section 0.2), **exactly once, 8 of 8**. So the click alone never
    /// writes two pairs, the abort path is not at fault, and it is not touched.</para>
    /// <para>**AND THERE IS NO MOMENT BEFORE SETTLE THAT IS FIXED**, which is the part worth writing down
    /// because it is what a reader would get wrong. The sequence's teardown is not a dispatcher continuation
    /// that a <c>Pump</c> would bound - it runs on the sequence's own task - so it races the click in real
    /// time: the trace caught its pair **already on the wire the instant <c>MouseUp</c> returned** on 2 of 8
    /// runs, landing inside the click's own pump on 4 more, and only after <c>await running</c> on the last
    /// 2. Reading the wire after the press is therefore no more fixed than reading it after the pump was.
    /// The only moment that does not race is **after the sequence has finished unwinding**, and 8 of 8 the
    /// wire there was the same.</para>
    /// <para>**SO THE WIRE IS READ WHERE IT HAS STOPPED CHANGING, AND WHAT IS ASSERTED IS THE RULE**, which
    /// is strictly more than the snapshot it replaces. What the old line claimed about Hamlet - that the
    /// click's abort is on the wire, behind the keying, as <c>CwStop</c> then <c>PttOff</c> - is kept whole.
    /// What it also claimed - that nothing else had happened yet at that instant - was never a fact about
    /// Hamlet but about thread scheduling, was false on a third of runs, and is gone. In its place the
    /// settled wire is asserted, which the old test never looked at at all: **nothing keys again**, every
    /// frame after the keying is an abort frame and nothing else, and they arrive in whole
    /// <c>CwStop, PttOff</c> pairs, so the radio is never left keyed by a lone CW stop. That the pair is
    /// there **twice** is asserted as the rule and not as a count of five, because whether Hamlet should put
    /// it on the wire twice at all is unit 375 section 4's finding for the owner and not this test's to
    /// bless.</para>
    /// <para>**NOTHING IS OPENED** (<c>SHACK_FACTS.md</c> FACT-004): every frame here is a byte array handed
    /// to a <see cref="FakePort"/>, and the audio ends at a <see cref="FakeSink"/> that makes no sound.</para>
    /// </remarks>
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

            // **WHAT THE PRESS ITSELF PUT ON THE WIRE.** The abort is same-thread and no-await, so by the
            // time this returns the click's own pair is on the wire, behind the keying and in order. The
            // sequence's teardown may or may not have added its own by now - that is the race - so this
            // reads the front of the wire, which the press fixes, and not its length, which it does not.
            Press(scene.Window, stop);

            var byTheClick = Frames(scene.Port);

            _output.WriteLine("wire the instant the press returns: " + string.Join(" | ", byTheClick));

            Assert.True(byTheClick.Length >= 3, "the press left only " + string.Join(" | ", byTheClick) + " on the wire");
            Assert.Equal(new[] { KeyOn, CwStop, PttOff }, byTheClick.Take(3));

            // **THE MOMENT THAT DOES NOT RACE**, and the one the old assertion was reaching for and missing:
            // the sequence has finished unwinding and the wire has stopped changing.
            Pump(scene.Window);
            await running;
            Pump(scene.Window);

            var settled = Frames(scene.Port);
            var afterTheKeying = settled.Skip(1).ToArray();

            _output.WriteLine("wire settled: " + string.Join(" | ", settled));
            _output.WriteLine("sound stopped by the token " + scene.Sink.StoppedByTheToken + ", " + scene.Sink.PlayedSoFar + " samples played");

            // It keyed once and never again: the abort is the end of the transmission, not a pause in it.
            Assert.Equal(KeyOn, settled[0]);
            Assert.DoesNotContain(KeyOn, afterTheKeying);

            // Nothing after the keying is anything but the abort pair, and the click's own pair is still the
            // first thing on the wire after it, in that order.
            Assert.All(afterTheKeying, frame => Assert.True(
                frame == CwStop || frame == PttOff,
                "the wire carries " + frame + " after the abort, which is neither the CW stop nor the unkey: " + string.Join(" | ", settled)));
            Assert.Equal(new[] { CwStop, PttOff }, afterTheKeying.Take(2));

            // And they arrive in whole pairs - a lone CW stop with no unkey behind it would leave the radio
            // keyed, which is the one shape of this wire that would matter at an antenna.
            Assert.True(afterTheKeying.Length % 2 == 0, "the abort frames do not pair up: " + string.Join(" | ", settled));

            for (var i = 0; i < afterTheKeying.Length; i += 2)
            {
                Assert.Equal(new[] { CwStop, PttOff }, afterTheKeying.Skip(i).Take(2));
            }

            Assert.True(scene.Sink.StoppedByTheToken, "the sound ran to its end after the click");
        }
        finally
        {
            scene.Window.Close();
        }
    }

    /// <summary>
    /// **At each of the nine sizes, the whole send area is on the window**: CQ, the mode tabs, Stop
    /// and the drive note, each drawn and each inside the window's own bounds.
    /// </summary>
    /// <remarks>
    /// <para>**WORK INSTRUCTION 356 TASK 1, AND WHAT IT IS FOR.** Hamlet opens at 1100 x 780. After
    /// unit 355 Stop is on the status bar and visible there, and **CQ and the mode tabs are drawn at
    /// y 800 to 830, below the window's bottom edge** (unit 355 item 3). A new operator opens the
    /// application and cannot see the button that calls CQ.</para>
    /// <para>**THE RULE THIS ASSERTS, WHICH IS THE UNIT'S OWN** (the instruction states it and marks
    /// it as such): where the window is too short for the top row, the working panels and the send
    /// area together, **the panels give up height first, then the top row, and the send area is
    /// never the thing that leaves.** The send area's drawn height is therefore the same at every
    /// size, which is asserted below rather than described.</para>
    /// <para>**COMPUTED, NOT SEEN.** These are `Bounds` read off a realized headless window; nothing
    /// here looks at a pixel.</para>
    /// </remarks>
    [AvaloniaFact]
    public void AtEachOfTheNineSizesTheSendAreaIsWholeOnTheWindow()
    {
        var misses = new List<string>();
        var heights = new List<(string Where, double Height)>();

        foreach (var (width, height) in Sizes)
        {
            var window = TheTopRowTests.Realized(width, height, null, null);

            try
            {
                Pump(window);

                var bounds = window.Bounds;
                var label = Size(width, height);

                foreach (var name in new[]
                {
                    "DigitalSendCqButton", "DigitalModeChipStrip",
                    "DigitalStopButton", "DigitalTransmitDriveNote",
                })
                {
                    var control = TheTopRowTests.Named<Control>(window, name);
                    var at = TheTopRowTests.RectIn(control, window);

                    var whole = at.Width > 0 && at.Height > 0
                        && at.Left >= -0.5 && at.Top >= -0.5
                        && at.Right <= bounds.Width + 0.5
                        && at.Bottom <= bounds.Height + 0.5;

                    _output.WriteLine(
                        label + "  " + name.PadRight(26) + Box(at)
                        + "  visible " + control.IsEffectivelyVisible + "  whole " + whole);

                    if (!control.IsEffectivelyVisible)
                    {
                        misses.Add(label + ": " + name + " is not visible");
                    }

                    if (!whole)
                    {
                        misses.Add(
                            label + ": " + name + " " + Box(at) + " is not whole on the "
                            + Box(bounds) + " window");
                    }
                }

                heights.Add((
                    label,
                    TheTopRowTests.RectIn(
                        TheTopRowTests.Named<Control>(window, "DigitalSendReserved"), window).Height));

                // **AND THE ROW THAT WAS EATING THE WINDOW**, printed beside them so the
                // cap can be read rather than taken on trust.
                _output.WriteLine(
                    "   " + label + "  " + "TopRow".PadRight(26)
                    + Box(TheTopRowTests.RectIn(
                        TheTopRowTests.Named<Control>(window, "TopRow"), window)));
            }
            finally
            {
                window.Close();
            }
        }

        _output.WriteLine("");

        foreach (var (where, tall) in heights)
        {
            _output.WriteLine("the send area is " + Px(tall) + " tall at " + where);
        }

        // **THE SEND AREA IS THE SAME HEIGHT EVERYWHERE**, which is what *never the thing that
        // leaves* means in a number: if it were giving up height it would shrink somewhere.
        var first = heights[0].Height;

        foreach (var (where, tall) in heights)
        {
            if (Math.Abs(tall - first) > 1)
            {
                misses.Add(
                    "the send area is " + Px(tall) + " tall at " + where + " and "
                    + Px(first) + " at " + heights[0].Where);
            }
        }

        Assert.True(misses.Count == 0, string.Join(Environment.NewLine, misses));
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
        Press(window, button);

        Pump(window);
    }

    /// <summary>
    /// <see cref="Click"/> without the pump that follows it, for a test that has to read something the press
    /// itself did before any continuation has had a chance to run.
    /// </summary>
    /// <remarks>
    /// The same real click and the same off-window guard; it stops one line earlier. Work instruction 375
    /// task 2 split it out because the abort is same-thread and no-await (CLAUDE.md section 0.2), so the
    /// instant this returns is a moment that cannot race, and the pump at the end of <see cref="Click"/> is
    /// a moment that can - which is what made
    /// <see cref="KeyedAtTheOpeningSizeAClickOnTheBarFiresTheAbortWhileItRuns"/> flake for four units.
    /// </remarks>
    private static void Press(Window window, Button button)
    {
        Pump(window);

        var centre = Centre(button, window);

        Assert.True(
            centre.X >= 0 && centre.Y >= 0 && centre.X <= window.Bounds.Width && centre.Y <= window.Bounds.Height,
            "Stop's centre " + centre + " is off the " + Box(window.Bounds) + " window, where no mouse can press it");

        window.MouseMove(centre);
        window.MouseDown(centre, MouseButton.Left);
        window.MouseUp(centre, MouseButton.Left);
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
