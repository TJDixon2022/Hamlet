using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
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
/// Work instruction 261, task 4: **a real button, on the real window, under a
/// real mouse, at both of the two moments.**
/// </summary>
/// <remarks>
/// <para>**WHAT WAS MISSING.** The window has had a "Stop sending" button since
/// long before this phase, and it belongs to <c>CwTransmitViewModel</c> - a
/// different view model, on a parked path, bound inside
/// <c>&lt;StackPanel DataContext="{Binding Transmit}"&gt;</c> at
/// `MainWindow.axaml:1157` and visible only while a *CW* send is running
/// (`docs/unit261-stop-trace.md` Q6). **The FT8 operator had no button at all.**
/// He right-clicked a row, one click keyed his radio for 12.64 seconds, and his
/// only recourse was the radio's front panel or the USB cable.</para>
/// <para>**BOTH MOMENTS, AND THEY ARE DIFFERENT MOMENTS.** Before the slot
/// boundary something is armed and nothing is keyed; during the transmission
/// nothing is armed - <c>Ft8ArmedSend.cs:148</c> clears the field before the
/// await - and the radio is on the air. A control that appeared only in one of
/// them would be useless in the other, so this one is visible and enabled in
/// both, and in neither is it conditional on anything.</para>
/// <para>**NOTHING IS OPENED** (`SHACK_FACTS.md` FACT-004): <c>FakePort</c>,
/// <c>FakeSink</c>, and a sink that parks a transmission in memory rather than
/// playing 12.64 seconds of anything.</para>
/// </remarks>
public sealed class TheOperatorCanStopItTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fixture.</summary>
    /// <param name="output">Where what the operator sees is quoted.</param>
    public TheOperatorCanStopItTests(ITestOutputHelper output) => _output = output;

    /// <summary>FT8's watering hole on 20 m, where a General may send data.</summary>
    private const long Ft8On20m = 14_074_000;

    /// <summary>The abort's first frame.</summary>
    private const string CwStop = "FE FE 94 E0 17 FF FD";

    /// <summary>The abort's second frame, and the ordinary unkey.</summary>
    private const string PttOff = "FE FE 94 E0 1C 00 00 FD";

    /// <summary>What the radio sees when it is keyed.</summary>
    private const string KeyOn = "FE FE 94 E0 1C 00 01 FD";

    // ---- moment zero: it is there before anything happens -----------------

    /// <summary>
    /// **The button is on the window, visible and enabled, with nothing armed and
    /// nothing sending.**
    /// </summary>
    /// <remarks>
    /// The precondition for both of the two moments: a stop the operator has to
    /// wait for is not reachable at the moment he reaches for it. Asserted on the
    /// realized control, not on the markup.
    /// </remarks>
    [AvaloniaFact]
    public void TheStopIsOnScreenAndPressableBeforeAnythingHappens()
    {
        var scene = Scene();
        var button = StopButton(scene);

        _output.WriteLine("button content    : " + button.Content);
        _output.WriteLine("is visible        : " + button.IsVisible);
        _output.WriteLine("is enabled        : " + button.IsEffectivelyEnabled);
        _output.WriteLine("command can run   : "
            + (button.Command?.CanExecute(null) ?? false));

        Assert.Equal("Stop", button.Content);
        Assert.True(button.IsVisible);
        Assert.True(button.IsEffectivelyEnabled);
        Assert.True(button.Command!.CanExecute(null));

        var centre = button.TranslatePoint(
            new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), scene.Window);

        _output.WriteLine("window client     : " + scene.Window.ClientSize);
        _output.WriteLine("button bounds     : " + button.Bounds);
        _output.WriteLine("centre on window  : " + centre);
        _output.WriteLine("hit test there    : "
            + (centre.HasValue
                ? scene.Window.InputHitTest(centre.Value)?.GetType().Name ?? "(nothing)"
                : "(no point)"));
    }

    // ---- moment one: armed, waiting for the boundary ----------------------

    /// <summary>
    /// **Armed and waiting for its slot: a real click un-arms it, and the slot
    /// then produces nothing.**
    /// </summary>
    /// <remarks>
    /// The fifteen seconds in which an operator realises he clicked the wrong
    /// station. **The proof is the boundary afterwards**, not a flag: it returns
    /// <see cref="Ft8ArmOutcome.NothingArmed"/>, the sink was never touched, and
    /// no keying frame is anywhere on the wire.
    /// </remarks>
    [AvaloniaFact]
    public async Task AClickBeforeTheBoundaryUnarmsItAndNothingGoesOut()
    {
        var scene = Scene();

        scene.Panel.SendMessageCommand.Execute("W1ABC KC3QIS -10");

        Assert.NotNull(scene.Panel.ArmedForSlotUtc);
        var slot = scene.Panel.ArmedForSlotUtc!.Value;

        _output.WriteLine("armed, and the area says: " + scene.Panel.DigitalSendLine);

        Click(scene, StopButton(scene));

        _output.WriteLine("after the click         : " + scene.Panel.DigitalSendLine);
        _output.WriteLine("wire                    : " + Wire(scene));

        Assert.Null(scene.Panel.ArmedForSlotUtc);
        Assert.Contains("W1ABC KC3QIS -10", scene.Panel.DigitalSendLine, StringComparison.Ordinal);
        Assert.Contains("will not go out", scene.Panel.DigitalSendLine, StringComparison.Ordinal);
        Assert.Contains(
            "the radio was told to stop transmitting",
            scene.Panel.DigitalSendLine,
            StringComparison.Ordinal);

        // The frames the radio took, and nothing that keys anything.
        Assert.Equal(new[] { CwStop, PttOff }, Frames(scene));

        var boundary = await scene.Panel.AtSlotBoundaryAsync(slot);

        _output.WriteLine("the boundary said       : " + boundary!.Outcome);

        Assert.Equal(Ft8ArmOutcome.NothingArmed, boundary.Outcome);
        Assert.True(scene.Sink.WasNeverTouched);
        Assert.DoesNotContain(KeyOn, Frames(scene));
    }

    // ---- moment two: the radio is keyed and transmitting ------------------

    /// <summary>
    /// **The radio is keyed and the tones are going out: a real click puts both
    /// abort frames on the wire while it is still running.**
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS THE ONE THE UNIT WAS WRITTEN FOR.** The transmission is
    /// parked inside the sink, which is where the 12.64 seconds are spent, and
    /// the click happens on the dispatcher thread exactly as an operator's does -
    /// which is possible at all only because nothing on the send path holds that
    /// thread (`docs/unit261-stop-trace.md` Q2).</para>
    /// <para>**THE FRAMES ARE COUNTED BEFORE THE TRANSMISSION IS RELEASED.** An
    /// abort that landed after the audio had finished would be indistinguishable
    /// from no abort at all.</para>
    /// </remarks>
    [AvaloniaFact]
    public async Task AClickWhileTheRadioIsKeyedFiresTheAbortWhileItIsStillRunning()
    {
        var parking = new ParkingSink();
        var scene = Scene(parking);

        scene.Panel.SendMessageCommand.Execute("W1ABC KC3QIS -10");

        var slot = scene.Panel.ArmedForSlotUtc!.Value;
        var running = scene.Panel.AtSlotBoundaryAsync(slot);

        Assert.True(parking.Entered.Wait(TimeSpan.FromSeconds(10)), "the sink was never reached");

        // The radio is on the air, and the button is still there and still live.
        var button = StopButton(scene);

        _output.WriteLine("while keyed, visible    : " + button.IsVisible);
        _output.WriteLine("while keyed, enabled    : " + button.IsEffectivelyEnabled);
        _output.WriteLine("wire before the click   : " + Wire(scene));

        Assert.True(button.IsVisible);
        Assert.True(button.IsEffectivelyEnabled);
        Assert.Equal(new[] { KeyOn }, Frames(scene));

        Click(scene, button);

        var whileRunning = Frames(scene);

        _output.WriteLine("after the click         : " + scene.Panel.DigitalSendLine);
        _output.WriteLine("wire while still running: " + string.Join(" | ", whileRunning));

        Assert.Equal(new[] { KeyOn, CwStop, PttOff }, whileRunning);
        Assert.Contains(
            "the radio was told to stop transmitting",
            scene.Panel.DigitalSendLine,
            StringComparison.Ordinal);

        parking.Release();

        var boundary = await running;

        Pump(scene.Window);

        _output.WriteLine("the run said            : " + boundary!.Run!.Outcome);
        _output.WriteLine("wire at the end         : " + Wire(scene));

        // The sequence's own finally then ran, redundantly and harmlessly.
        Assert.Equal(new[] { KeyOn, CwStop, PttOff, PttOff }, Frames(scene));
    }

    /// <summary>
    /// **A real click stops the sound as well as the carrier, through the
    /// application's own send path.**
    /// </summary>
    /// <remarks>
    /// <para>**THE BREAKAGE THIS WOULD HAVE CAUGHT, AND DID** (work instruction
    /// 263). Its neighbour above proves the frames reach the wire and then
    /// releases the transmission to finish - because until this unit nothing could
    /// have stopped it. The application handed
    /// <see cref="Ft8ArmedSend.AtBoundaryAsync"/> no token at all
    /// (<c>MainWindowViewModel.cs:8258</c>), so the whole chain ran on
    /// <c>CancellationToken.None</c>, and **the operator's stop unkeyed his radio
    /// and left Hamlet feeding it the rest of the slot** -
    /// <c>docs/unit263-stop-audio-trace.md</c> Q1.</para>
    /// <para>**AND IT IS PROVED HERE, NOT ONLY IN THE ENGINE**, because this is
    /// the fake the application's send path actually runs against and unit 262
    /// found it the more permissive of the two. A green in
    /// <c>Hamlet.RadioEngine.Tests</c> over an application that never reaches the
    /// same code is the shape of that unit's whole finding.</para>
    /// <para>**THE VIEW MODEL PASSES NO TOKEN AND STILL DOES NOT NEED TO.**
    /// <c>Ft8ArmedSend</c> makes its own source at the boundary, so
    /// <c>AtSlotBoundaryAsync</c> is unchanged and **there is still exactly one
    /// stop entry point in the view model** - <c>StopSendingCommand</c>, which is
    /// what this clicks.</para>
    /// </remarks>
    [AvaloniaFact]
    public async Task AClickWhileTheToneIsPlayingStopsTheSoundAndNotJustTheCarrier()
    {
        var playing = new FakeSink { PlaysOver = TimeSpan.FromSeconds(2) };
        var scene = Scene(playing);

        scene.Panel.SendMessageCommand.Execute("W1ABC KC3QIS -10");

        var slot = scene.Panel.ArmedForSlotUtc!.Value;
        var running = scene.Panel.AtSlotBoundaryAsync(slot);

        Assert.True(playing.Entered.Wait(TimeSpan.FromSeconds(10)), "the sink was never reached");

        var total = playing.SamplesHandedOver;
        var rate = playing.RateAskedFor;

        // Let some of it go out, so there is something left to stop.
        var deadline = System.Diagnostics.Stopwatch.StartNew();

        while (playing.PlayedSoFar < total / 4 && deadline.Elapsed < TimeSpan.FromSeconds(30))
        {
            await Task.Delay(2, CancellationToken.None);
        }

        var playedAtTheClick = playing.PlayedSoFar;

        Assert.Equal(new[] { KeyOn }, Frames(scene));

        Click(scene, StopButton(scene));

        var whileRunning = Frames(scene);
        var boundary = await running;
        var playedInTheEnd = boundary!.Run!.Played!.Value.SamplesPlayed;
        var afterTheClick = (playedInTheEnd - playedAtTheClick) * 1000.0 / rate;

        Pump(scene.Window);

        _output.WriteLine("the slot was        : " + total + " samples at " + rate + " Hz");
        _output.WriteLine("played at the click : " + playedAtTheClick);
        _output.WriteLine("played in the end   : " + playedInTheEnd + " of " + total);
        _output.WriteLine("wire while running  : " + string.Join(" | ", whileRunning));
        _output.WriteLine("the run said        : " + boundary.Run.Outcome);
        _output.WriteLine("the operator reads  : " + scene.Panel.DigitalSendLine);
        _output.WriteLine("AUDIO AFTER THE CLICK: " + afterTheClick.ToString("F0") + " ms");

        // ---- the carrier, which already worked --------------------------------
        Assert.Equal(new[] { KeyOn, CwStop, PttOff }, whileRunning);

        // ---- the sound, which is what this unit is for ------------------------
        Assert.True(
            playing.StoppedByTheToken,
            $"the sink played all {total} samples: the click never reached it");

        Assert.True(
            playedInTheEnd < total,
            $"the whole slot went out anyway: {playedInTheEnd} of {total} samples");

        Assert.Equal(Ft8TransmitOutcome.Cancelled, boundary.Run.Outcome);
    }

    // ---- no radio: not a crash and not a lie ------------------------------

    /// <summary>
    /// **With no radio connected the button still presses, and it says what is
    /// missing rather than claiming to have stopped anything.**
    /// </summary>
    /// <remarks>
    /// The state this machine is actually in (`SHACK_FACTS.md` FACT-004).
    /// <c>_rigPort</c> is null and so is the armed send, because
    /// <see cref="Ft8TransmitSequence"/> is reachable through nothing else -
    /// so there is nothing this application can have keyed, and the line names
    /// the missing halves instead of pretending a frame went out.
    /// </remarks>
    [AvaloniaFact]
    public void WithNoRadioTheStopIsNotACrashAndNotALie()
    {
        var scene = Scene(attachRadio: false);
        var button = StopButton(scene);

        Assert.True(button.IsVisible);
        Assert.True(button.IsEffectivelyEnabled);

        Click(scene, button);

        _output.WriteLine("with no radio           : " + scene.Panel.DigitalSendLine);

        Assert.StartsWith(
            "There was nothing to stop:",
            scene.Panel.DigitalSendLine,
            StringComparison.Ordinal);

        Assert.Contains(
            "no radio is connected",
            scene.Panel.DigitalSendLine,
            StringComparison.Ordinal);

        // It did not claim anything reached a radio.
        Assert.DoesNotContain(
            "was told to stop", scene.Panel.DigitalSendLine, StringComparison.Ordinal);
    }

    /// <summary>
    /// **A second press is not a lie either: it says nothing was waiting.**
    /// </summary>
    /// <remarks>
    /// **AND IT STILL TELLS THE RADIO.** The stop is not conditional on the
    /// application believing there is something to stop - the two extra frames
    /// cost nothing and the one time they matter is the time the application is
    /// wrong about what the radio is doing.
    /// </remarks>
    [AvaloniaFact]
    public void PressingItTwiceSaysWhatTheSecondPressFoundAndStillTellsTheRadio()
    {
        var scene = Scene();

        scene.Panel.SendMessageCommand.Execute("W1ABC KC3QIS -10");

        Click(scene, StopButton(scene));
        var first = scene.Panel.DigitalSendLine;

        Click(scene, StopButton(scene));
        var second = scene.Panel.DigitalSendLine;

        _output.WriteLine("first press             : " + first);
        _output.WriteLine("second press            : " + second);
        _output.WriteLine("wire                    : " + Wire(scene));

        Assert.Contains("W1ABC KC3QIS -10", first, StringComparison.Ordinal);
        Assert.Contains("nothing was waiting for a slot", second, StringComparison.Ordinal);
        Assert.Contains(
            "the radio was told to stop transmitting", second, StringComparison.Ordinal);

        Assert.Equal(new[] { CwStop, PttOff, CwStop, PttOff }, Frames(scene));
    }

    /// <summary>
    /// **The stop is not the CW path's button, and the CW path's button is not on
    /// this tab.**
    /// </summary>
    /// <remarks>
    /// Named because the window has two things called stop and only one of them
    /// reaches a keyed FT8 transmitter. `MainWindow.axaml:1242`'s
    /// <c>AbortCommand</c> resolves on <c>CwTransmitViewModel</c> and its
    /// <c>IsVisible</c> is that view model's <c>IsSending</c>, which an FT8
    /// transmission never sets.
    /// </remarks>
    [AvaloniaFact]
    public void TheFt8StopIsADifferentButtonFromTheCwOne()
    {
        var scene = Scene();
        var button = StopButton(scene);

        var cw = scene.Window.GetVisualDescendants()
            .OfType<Button>()
            .Where(b => (b.Content as string) == "Stop sending")
            .ToList();

        _output.WriteLine("FT8 stop command  : " + button.Command!.GetType().Name);
        _output.WriteLine("\"Stop sending\" buttons realized on the Digital tab: " + cw.Count);

        // The FT8 stop's command is the panel's own, not the CW view model's.
        Assert.Same(scene.Panel.StopSendingCommand, button.Command);
        Assert.NotSame(scene.Panel.Transmit.AbortCommand, button.Command);

        // And the CW one is not visible here even if it is realized.
        Assert.All(cw, b => Assert.False(b.IsVisible));
    }

    // ---- the phase's one unrecoverable fault -----------------------------

    /// <summary>
    /// **The stop added no way to transmit: one arming call site, and it is still
    /// the operator's own click.**
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS THE FAULT THE PHASE CANNOT RECOVER FROM** - a transmission
    /// the operator did not ask for goes out over other people's band and cannot
    /// be taken back. A unit that put a new button on the transmit surface owes
    /// this evidence, and work instruction 261 task 5 asks for it as a grep. It
    /// is kept here as a standing guard instead, so the next unit inherits the
    /// count rather than having to remember to run it.</para>
    /// <para>**THE THREE NUMBERS.** One line in `src/` calls
    /// <c>Ft8ArmedSend.Arm</c> and it is inside <c>SendMessage</c>; one line
    /// constructs an <c>Ft8ArmedSend</c>; one line constructs the
    /// <c>Ft8TransmitSequence</c> behind it. <c>StopNow</c> is on the far side of
    /// all three - it is a route out and there is no way through it into a
    /// transmission.</para>
    /// </remarks>
    [Fact]
    public void TheStopAddedNoNewRouteToATransmission()
    {
        var source = Directory
            .EnumerateFiles(Path.Combine(RepositoryRoot(), "src"), "*.*", SearchOption.AllDirectories)
            .Where(p => p.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                || p.EndsWith(".axaml", StringComparison.OrdinalIgnoreCase))
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
                StringComparison.OrdinalIgnoreCase))
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}",
                StringComparison.OrdinalIgnoreCase))
            .ToList();

        var arms = Hits(source, "_armedSend.Arm(");
        var builds = Hits(source, "new Ft8ArmedSend");
        var sequences = Hits(source, "new Ft8TransmitSequence");

        foreach (var hit in arms.Concat(builds).Concat(sequences))
        {
            _output.WriteLine(hit);
        }

        Assert.Single(arms);
        Assert.Single(builds);
        Assert.Single(sequences);

        // AND THE ONE ARMING LINE IS STILL THE OPERATOR'S OWN CLICK.
        var panel = File.ReadAllText(Path.Combine(
            RepositoryRoot(), "src", "Hamlet.App", "ViewModels", "MainWindowViewModel.cs"));

        Assert.Contains("_armedSend.Arm(", MethodBody(panel, "private void SendMessage("),
            StringComparison.Ordinal);

        // And the stop's own method arms nothing, composes nothing, keys nothing.
        var stop = MethodBody(panel, "private void StopSending()");

        _output.WriteLine("StopSending's body:" + Environment.NewLine + stop);

        foreach (var forbidden in new[] { ".Arm(", "Compose", "RunAsync", "AtBoundary", "new " })
        {
            Assert.DoesNotContain(forbidden, stop, StringComparison.Ordinal);
        }
    }

    /// <summary>Every <c>path:line</c> in <paramref name="files"/> holding a needle.</summary>
    private static List<string> Hits(IEnumerable<string> files, string needle)
    {
        var found = new List<string>();

        foreach (var file in files)
        {
            var lines = File.ReadAllLines(file);

            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(needle, StringComparison.Ordinal))
                {
                    found.Add($"{needle} -> {Path.GetFileName(file)}:{i + 1}");
                }
            }
        }

        return found;
    }

    /// <summary>One method's body, by brace matching from its signature.</summary>
    private static string MethodBody(string source, string signature)
    {
        var start = source.IndexOf(signature, StringComparison.Ordinal);

        Assert.True(start >= 0, signature + " is not in the source");

        var open = source.IndexOf('{', start);
        var depth = 0;
        var at = open;

        for (; at < source.Length; at++)
        {
            if (source[at] == '{')
            {
                depth++;
            }
            else if (source[at] == '}' && --depth == 0)
            {
                break;
            }
        }

        return source[start..(at + 1)];
    }

    /// <summary>The repository root, found from the test assembly's location.</summary>
    private static string RepositoryRoot()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }

    // ---- the scene -------------------------------------------------------

    /// <summary>The window, the panel and the fakes behind them.</summary>
    private sealed record Built(
        MainWindow Window, MainWindowViewModel Panel, FakePort Port, FakeSink Sink);

    /// <summary>
    /// Builds the real window on the Digital tab, with or without a radio.
    /// </summary>
    /// <param name="parking">A sink that parks, where a test needs one.</param>
    /// <param name="attachRadio">
    /// False leaves <c>_rigPort</c> and the armed send null, which is this
    /// machine's real state.
    /// </param>
    private static Built Scene(ITransmitAudioSink? parking = null, bool attachRadio = true)
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = "FN00";
        settings.Operator.LicenseClass = LicenseClass.General;

        var panel = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalDecodedExpanded = true,
        };

        // **A WINDOW TALL ENOUGH TO SHOW THE SEND AREA WITHOUT SCROLLING.** At
        // the default height the reserved Send area sits below the fold, and a
        // mouse cannot reach what is not on screen - which is a fact about the
        // window size, not about the button.
        var window = new MainWindow { DataContext = panel, Width = 1400, Height = 1400 };

        window.Show();
        Pump(window);

        panel.SelectedBand = panel.Bands.First(
            b => b.Band.LowHz <= Ft8On20m && b.Band.HighHz >= Ft8On20m);
        panel.FrequencyHz = Ft8On20m;

        var port = new FakePort();
        var sink = new FakeSink();

        if (attachRadio)
        {
            panel.UseRigPortForTests(port);
            panel.UseArmedSendForTests(
                new Ft8ArmedSend(new Ft8TransmitSequence(port, parking ?? sink)));
        }

        Pump(window);

        return new Built(window, panel, port, sink);
    }

    /// <summary>The realized FT8 stop button, found on the window itself.</summary>
    private static Button StopButton(Built scene)
    {
        Pump(scene.Window);

        var button = scene.Window.GetVisualDescendants()
            .OfType<Button>()
            .FirstOrDefault(b => b.Name == "DigitalStopButton");

        Assert.True(button is not null, "DigitalStopButton is not on the realized window");

        return button!;
    }

    /// <summary>
    /// **A real left click, at the button's own place on the window.**
    /// </summary>
    /// <remarks>
    /// Headless pointer input rather than an invoked command: the point is that a
    /// mouse can reach it. The centre of its bounds is translated into window
    /// coordinates, so a control that had been laid out somewhere unreachable
    /// would fail here rather than pass on a command call.
    /// </remarks>
    private static void Click(Built scene, Button button)
    {
        Pump(scene.Window);

        var centre = button.TranslatePoint(
            new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), scene.Window);

        Assert.True(centre.HasValue, "the button has no place on the window");

        scene.Window.MouseMove(centre!.Value);
        scene.Window.MouseDown(centre.Value, MouseButton.Left);
        scene.Window.MouseUp(centre.Value, MouseButton.Left);

        Pump(scene.Window);
    }

    /// <summary>Every frame the fake port took, as hex, in order.</summary>
    private static string[] Frames(Built scene)
        => scene.Port.Written.Select(Hex).ToArray();

    /// <summary>The wire as one readable line.</summary>
    private static string Wire(Built scene)
    {
        var frames = Frames(scene);

        return frames.Length == 0 ? "(nothing)" : string.Join(" | ", frames);
    }

    private static string Hex(IEnumerable<byte> bytes)
        => string.Join(' ', bytes.Select(b => b.ToString("X2")));

    /// <summary>Runs the dispatcher and lays the window out.</summary>
    private static void Pump(Window window)
    {
        for (var i = 0; i < 5; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    /// <summary>
    /// A sink that holds a transmission open until a test lets it go, and plays
    /// nothing.
    /// </summary>
    /// <remarks>
    /// **NO DEVICE IS OPENED AND NO SOUND IS MADE.** It exists so that "the radio
    /// is keyed and mid-transmission" can be reached on a build machine without
    /// waiting 12.64 seconds, and so that the click lands while the radio is
    /// still on the air rather than after it has come back.
    /// </remarks>
    private sealed class ParkingSink : ITransmitAudioSink
    {
        private readonly ManualResetEventSlim _release = new(false);

        /// <summary>Set once something is actually inside the play.</summary>
        public ManualResetEventSlim Entered { get; } = new(false);

        /// <summary>Let the parked transmission finish.</summary>
        public void Release() => _release.Set();

        /// <inheritdoc/>
        /// <remarks>
        /// The decoder's rate, which is what these tests compose at. **This sink
        /// refuses nothing** - what it stands for is a transmission in progress,
        /// not an endpoint.
        /// </remarks>
        public int EndpointSampleRate => Ft8Composer.DefaultSampleRate;

        /// <inheritdoc/>
        public async Task<PlayedAudio> PlayAsync(
            ReadOnlyMemory<float> samples, int sampleRate, CancellationToken cancellationToken)
        {
            var count = samples.Length;

            return await Task.Run(
                () =>
                {
                    Entered.Set();

                    Assert.True(
                        _release.Wait(TimeSpan.FromSeconds(30)),
                        "the parked transmission was never released");

                    return new PlayedAudio(count, TimeSpan.FromSeconds(12.64));
                },
                CancellationToken.None).ConfigureAwait(false);
        }
    }
}
