using System.Diagnostics;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using Ft8Sharp;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Transmit;
using NAudio.CoreAudioApi;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **The whole FT4 chain, from one right-click, at the bench.** Work instruction
/// 293, task 6 - step 4's criterion 4.
/// </summary>
/// <remarks>
/// <para>**UNIT 268'S FT8 TEST IS THE MODEL AND ITS SHAPE IS FOLLOWED
/// DELIBERATELY** (`TheWholeChainRunsFromOneRightClickTests.cs:267`). That is one
/// transmission from one right click on a seeded row - **not a five-leg QSO** - and
/// this is the FT4 twin of it: a real `ContextRequested` on a real row in a real
/// window, the menu item invoked through its own command with its own parameter,
/// compose, key, play, unkey, and the audio that comes back off the card decoded
/// and compared against the text the item was carrying.</para>
/// <para>**THE BREAKAGE IT CATCHES** (`CLAUDE.md`: a unit may not add a test
/// without naming it): **the menu offering one string and the send path
/// transmitting another** - unit 268's own words for why it exists - **now with two
/// composers in the tree instead of one**, which is exactly the condition under
/// which that failure returns.</para>
/// <para>**THE SEEDED ROW IS CHOSEN SO THE REPLY NEEDS NO MEASURED RATIO.** He
/// rogered and reported, so what conventionally comes next is an acknowledgement -
/// `RRR`, which carries no signal report. **Every FT4 row's ratio is null** because
/// `Ft8DeepSignalToNoise.Estimate` is welded to FT8's symbol count and tone count
/// and there is no FT4 equivalent in this tree, so the FT4 menu is short the
/// `Report` and `RogerAndReport` shapes. **That is a named gap and not a failure of
/// this test**, and no ratio is invented or substituted for one here.</para>
/// <para>**NOTHING HERE OPENS A SERIAL PORT** (`SHACK_FACTS.md` FACT-004). The
/// device is a sound card and the port is <see cref="FakePort"/>. **Nothing
/// measured here says anything about the IC-7300.**</para>
/// <para>**A MACHINE WITH NO RENDER ENDPOINT IS A NORMAL MACHINE** - unit 256's
/// precedent, carried through
/// <c>TheLoopbackThroughTheApplicationsSendPathTests.cs:69-79</c>. Where there is
/// none, the endpoints there are get printed, the sound-card half is said plainly
/// to be unreachable, and the chain is proved through
/// <see cref="FakeSink"/> instead. **A fake-sink run is never reported as a
/// loopback run.**</para>
/// <para>**IT MAKES REAL SOUND ON A REAL COMPUTER**, one 5.04-second FT4
/// transmission, with a second of pre-roll and a second of post-roll around it.
/// </para>
/// </remarks>
public sealed class TheWholeFt4ChainRunsFromOneRightClickTests : IDisposable
{
    /// <summary>How long to let the capture settle before the click.</summary>
    private static readonly TimeSpan PreRoll = TimeSpan.FromSeconds(1);

    /// <summary>How long to keep capturing after the transmission returns.</summary>
    private static readonly TimeSpan PostRoll = TimeSpan.FromSeconds(1);

    /// <summary>FT4's watering hole on 20 m, from the cited band data.</summary>
    private const long Ft4On20m = 14_080_000;

    /// <summary>The operator.</summary>
    private const string Mine = "KC3QIS";

    /// <summary>The station he right-clicks.</summary>
    private const string His = "W1ABC";

    /// <summary>The slot the row on the table was heard in.</summary>
    private static readonly DateTime RowSlot =
        new(2026, 9, 9, 18, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// What the operator heard from him, which is what puts a row up.
    /// </summary>
    /// <remarks>
    /// **HE ROGERED AND REPORTED, SO AN ACKNOWLEDGEMENT COMES NEXT** and the reply
    /// carries no ratio. Unit 268 seeded the same words on FT8.
    /// </remarks>
    private const string HeardFromHim = Mine + " " + His + " R-15";

    /// <summary>What the radio sees when it is keyed.</summary>
    private const string KeyOn = "FE FE 94 E0 1C 00 01 FD";

    /// <summary>The ordinary unkey.</summary>
    private const string KeyOff = "FE FE 94 E0 1C 00 00 FD";

    /// <summary>Where the application's own telemetry writer is pointed.</summary>
    private readonly string _folder = Path.Combine(
        Path.GetTempPath(), "hamlet-unit293-" + Guid.NewGuid().ToString("N")[..8]);

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where every measured figure is printed.</param>
    public TheWholeFt4ChainRunsFromOneRightClickTests(ITestOutputHelper output)
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
    /// **One right-click on an FT4 row, and the sound that comes back off the card
    /// decodes as the text on the item he clicked.**
    /// </summary>
    [AvaloniaFact]
    public async Task OneRightClickOnAnFt4RowDrivesTheWholeChainAndTheAudioDecodesBack()
    {
        var endpoint = Preferred(out var why);

        if (endpoint is null)
        {
            NoEndpoint(why);

            return;
        }

        using var scene = Scene(endpoint);

        _output.WriteLine("chosen because   : " + why);
        _output.WriteLine("endpoint         : " + endpoint.Name);
        _output.WriteLine("endpoint declares: " + endpoint.SampleRate + " Hz");
        _output.WriteLine("sink opened      : " + scene.Sink.DeviceName);
        _output.WriteLine("sink will accept : " + scene.Sink.EndpointSampleRate + " Hz");
        _output.WriteLine("mode chosen      : " + scene.Panel.DigitalMode);
        _output.WriteLine("grid the tab runs: " + scene.Panel.DigitalGrid.Describe());
        _output.WriteLine("dial             : " + scene.Panel.FrequencyHz + " Hz");
        _output.WriteLine("row on the table : \"" + HeardFromHim
            + "\", with no measured ratio");
        _output.WriteLine("host             : Avalonia headless window, real MainWindow shown");

        using var capture = new Capture(endpoint);

        capture.Start();
        await Task.Delay(PreRoll);

        // ---- 1. THE RIGHT-CLICK, AND THE ITEM ITSELF ---------------------------
        var flyout = RightClick(scene);
        var options = Options(flyout);
        var item = TheOneThatComesNext(flyout);

        var header = item.Header as string ?? "";
        var clicked = item.CommandParameter as string;

        Assert.True(
            clicked is not null,
            "the menu item under the mouse carried no message: header \"" + header + "\"");

        _output.WriteLine(string.Empty);
        _output.WriteLine("the menu offered : " + options.Count + " clickable messages");

        foreach (var option in options)
        {
            _output.WriteLine("    " + (option.Header as string ?? ""));
        }

        _output.WriteLine("HE CLICKED       : " + header);
        _output.WriteLine("WHICH CARRIES    : \"" + clicked + "\"");

        Assert.True(item.Command!.CanExecute(clicked), "the item could not be executed");

        var clock = Stopwatch.StartNew();

        item.Command.Execute(clicked);

        var slot = scene.Panel.ArmedForSlotUtc;

        Assert.True(slot is not null, scene.Panel.DigitalSendLine);

        // ---- WHAT THE OPERATOR SEES WHILE IT IS ARMED --------------------------
        _output.WriteLine(string.Empty);
        _output.WriteLine("---- what he sees, armed ----");
        _output.WriteLine("send line        : " + scene.Panel.DigitalSendLine);
        _output.WriteLine("stop control     : \"" + scene.Panel.StopLabel
            + "\", something to stop: " + scene.Panel.HasSomethingToStop);
        _output.WriteLine("stop tip         : " + scene.Panel.StopTip);
        _output.WriteLine("turn line        : " + scene.Panel.DigitalTurnLine);

        var result = await scene.Panel.AtSlotBoundaryAsync(slot!.Value);

        await Task.Delay(PostRoll);
        capture.Stop();
        clock.Stop();

        Pump(scene.Window);

        var run = result!.Run;
        var sent = result.Send!;
        var played = run?.Played ?? new PlayedAudio(0, TimeSpan.Zero);

        _output.WriteLine(string.Empty);
        _output.WriteLine("armed for slot   : "
            + slot.Value.ToString("O", CultureInfo.InvariantCulture));
        _output.WriteLine("grid on the send : " + sent.Grid.Describe()
            + ", named " + sent.Grid.Name);
        _output.WriteLine("composed at      : " + sent.Transmission.SampleRate + " Hz");
        _output.WriteLine("samples composed : " + sent.Transmission.Samples.Length);
        _output.WriteLine("which is         : "
            + sent.Transmission.SlotSeconds.ToString("0.###", CultureInfo.InvariantCulture) + " s");
        _output.WriteLine("outcome          : " + run?.Outcome);
        _output.WriteLine("reason           : " + run?.Reason);
        _output.WriteLine("keyed            : " + run?.Keyed);
        _output.WriteLine("came out of tx   : " + run?.CameOutOfTransmit);
        _output.WriteLine("samples played   : " + played.SamplesPlayed);
        _output.WriteLine("peak written     : " + scene.Sink.PeakWritten.ToString("F4"));
        _output.WriteLine("clipped samples  : " + scene.Sink.ClippedSamples);
        _output.WriteLine("wire             : " + Wire(scene));
        _output.WriteLine("wall clock       : "
            + clock.Elapsed.TotalSeconds.ToString("F2", CultureInfo.InvariantCulture) + " s");
        _output.WriteLine("the operator reads: " + scene.Panel.DigitalSendLine);

        // ---- 2. COMPOSE, KEY, PLAY, UNKEY --------------------------------------
        Assert.Equal(Ft8ArmOutcome.Ran, result.Outcome);
        Assert.True(run!.Sent, run.Reason);
        Assert.True(run.Keyed);
        Assert.Equal(UnkeyRoute.OrdinaryUnkey, run.CameOutOfTransmit);
        Assert.Equal(new[] { KeyOn, KeyOff }, Frames(scene));

        // **IT IS FT4'S WAVEFORM AND FT4'S GRID.**
        Assert.Equal(SlotGrid.Ft4, sent.Grid);
        Assert.Equal(
            Ft8Sharp.Encode.Ft4Waveform.SymbolCount
                * Ft8Sharp.Encode.Ft4Waveform.SamplesPerSymbol(sent.Transmission.SampleRate),
            sent.Transmission.Samples.Length);

        // AND WHAT WENT OUT IS WHAT WAS CLICKED, ON THE WAY IN AS WELL AS BACK.
        Assert.Equal(clicked, sent.Transmission.Text);
        Assert.Equal(endpoint.SampleRate, sent.Transmission.SampleRate);
        Assert.Equal(sent.Transmission.Samples.Length, played.SamplesPlayed);

        // ---- 3. THE DECODE, THROUGH THE TAB'S OWN PATH -------------------------
        var captured = capture.Snapshot();

        Assert.True(
            captured is not null && captured.Samples.Length > 0,
            "the loopback capture started and delivered nothing while the endpoint was "
            + $"rendering - {capture.SamplesSeen} samples reached the tap");

        var level = Level(captured!.Samples);

        _output.WriteLine(string.Empty);
        _output.WriteLine("captured at      : " + captured.SampleRate + " Hz");
        _output.WriteLine("captured samples : " + captured.Samples.Length);
        _output.WriteLine("CAPTURED PEAK    : " + Db(level.PeakDb) + " dBFS");
        _output.WriteLine("captured rms     : " + Db(level.RmsDb) + " dBFS");

        Assert.True(
            level.PeakDb > AudioLevel.TooQuietDb,
            "the run said it sent and the card was silent: the loopback captured "
            + $"{captured.Samples.Length} samples off {scene.Sink.DeviceName} peaking at "
            + $"{Db(level.PeakDb)} dBFS, which is at or below {AudioLevel.TooQuietDb} dBFS.");

        var slotAudio = OneFt4SlotAround(captured, out var onset);

        _output.WriteLine("tone onset at    : " + onset + " samples ("
            + (onset / (double)captured.SampleRate).ToString("0.###", CultureInfo.InvariantCulture)
            + " s into the capture)");
        _output.WriteLine("slot handed over : " + slotAudio.Samples.Length + " samples, "
            + (slotAudio.Samples.Length / (double)slotAudio.SampleRate)
                .ToString("0.###", CultureInfo.InvariantCulture) + " s");

        // **THE TAB'S OWN PATH, AND THE TAB IS IN FT4.** `ShowDecodes` is what the
        // Digital tab calls on every closed slot; it cuts on `DigitalGrid` and reads
        // with the decoder `DigitalMode` names. Nothing here decodes a second way.
        var endedAt = new DateTime(2026, 9, 9, 12, 0, 0, DateTimeKind.Utc);
        var heard = scene.Panel.ShowDecodes(
            slotAudio, endedAt, new ClockOffset(0, endedAt.AddSeconds(-1)));

        var texts = heard.Decodes.Select(d => d.Message).ToList();
        var matched = texts.Count(t => string.Equals(t, clicked, StringComparison.Ordinal));
        var wrong = texts.Count - matched;
        var missed = matched > 0 ? 0 : 1;

        _output.WriteLine(string.Empty);
        _output.WriteLine("---- the click and the sound, side by side ----");
        _output.WriteLine("slots cut        : " + heard.SlotsDecoded
            + " on " + scene.Panel.DigitalGrid.Describe());
        _output.WriteLine("candidates       : " + heard.CandidatesFound);
        _output.WriteLine("decoder named    : "
            + string.Join(", ", heard.Slots.Select(s => s.Decoder.Name).Distinct()));
        _output.WriteLine("CLICKED          : \"" + clicked + "\"");
        _output.WriteLine("DECODED          : "
            + (texts.Count == 0 ? "nothing" : "\"" + string.Join("\", \"", texts) + "\""));
        _output.WriteLine("transmissions in : 1");
        _output.WriteLine("MISSED           : " + missed);
        _output.WriteLine("WRONG            : " + wrong);

        // **THE DECODER IS THE PORT AND NEVER `Ft8Sharp.Deep`**, which has no FT4
        // decoder at all.
        Assert.All(heard.Slots, s => Assert.Equal("Ft8Sharp", s.Decoder.Name));
        Assert.DoesNotContain(heard.Slots, s => s.Decoder.Name.Contains("Deep", StringComparison.Ordinal));

        // **§0.0. A decode that did not happen is reported as one that did not
        // happen**, against the text on the item that was clicked and not a literal.
        Assert.True(
            texts.Contains(clicked!, StringComparer.Ordinal),
            $"the operator clicked a menu item reading \"{header}\", which carries "
            + $"\"{clicked}\". That went out of {scene.Sink.DeviceName} at "
            + $"{sent.Transmission.SampleRate} Hz, peaking at {scene.Sink.PeakWritten:F4}, "
            + "and the FT4 decoder returned "
            + $"{(texts.Count == 0 ? "nothing" : "\"" + string.Join("\", \"", texts) + "\"")} "
            + "from the captured audio. The message on the menu and the message on the "
            + "air are not the same message.");

        Assert.Equal(0, wrong);
        Assert.Equal(0, missed);

        // ---- 4. AND NOTHING TRANSMITS THAT WAS NOT CLICKED ---------------------
        var framesBefore = scene.Port.Written.Count;
        var nothing = await scene.Panel.AtSlotBoundaryAsync(
            slot.Value.AddSeconds(SlotGrid.Ft4.SlotSeconds));

        _output.WriteLine(string.Empty);
        _output.WriteLine("---- the FT4 boundary nobody clicked ----");
        _output.WriteLine("outcome          : " + nothing!.Outcome);
        _output.WriteLine("frames on wire   : " + scene.Port.Written.Count
            + " (unchanged from " + framesBefore + ")");

        Assert.Equal(Ft8ArmOutcome.NothingArmed, nothing.Outcome);
        Assert.Null(nothing.Run);
        Assert.Equal(framesBefore, scene.Port.Written.Count);

        _output.WriteLine(string.Empty);
        _output.WriteLine(
            "WHICH HALF IS EVIDENCE: a real WASAPI render endpoint and a real loopback "
            + "capture. The sound was made on " + scene.Sink.DeviceName
            + " and read back off it.");
    }

    /// <summary>
    /// **The same chain end to end through the fake sink and the fake wire.**
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS NOT A LOOPBACK RUN AND IS NEVER REPORTED AS ONE.** No card
    /// is opened and no sound is made: what it proves is that the menu, the compose,
    /// the arm, the boundary, the keying, the play, the unkey and the decode are one
    /// chain, by decoding **the samples the sink was actually handed**. The sound-card
    /// half is the test above.</para>
    /// <para>**IT RUNS ON EVERY MACHINE**, which is why it is here: it is the half
    /// that still answers on a machine with no render endpoint at all.</para>
    /// </remarks>
    [AvaloniaFact]
    public async Task TheSameChainThroughTheFakeSinkDecodesTheSamplesItWasHanded()
    {
        using var scene = Scene(endpoint: null);

        var flyout = RightClick(scene);
        var options = Options(flyout);
        var item = TheOneThatComesNext(flyout);
        var clicked = item.CommandParameter as string;

        Assert.True(clicked is not null, "the menu item carried no message");

        _output.WriteLine("sink             : FakeSink - no device is opened and no sound is made");
        _output.WriteLine("the menu offered : " + options.Count + " clickable messages");

        foreach (var option in options)
        {
            _output.WriteLine("    " + (option.Header as string ?? ""));
        }

        _output.WriteLine("HE CLICKED       : " + (item.Header as string ?? ""));
        _output.WriteLine("WHICH CARRIES    : \"" + clicked + "\"");

        item.Command!.Execute(clicked);

        var slot = scene.Panel.ArmedForSlotUtc;

        Assert.True(slot is not null, scene.Panel.DigitalSendLine);

        var result = await scene.Panel.AtSlotBoundaryAsync(slot!.Value);
        var run = result!.Run;
        var sent = result.Send!;

        _output.WriteLine(string.Empty);
        _output.WriteLine("armed for slot   : "
            + slot.Value.ToString("O", CultureInfo.InvariantCulture));
        _output.WriteLine("grid on the send : " + sent.Grid.Describe());
        _output.WriteLine("outcome          : " + run?.Outcome);
        _output.WriteLine("wire             : " + string.Join(" | ", Frames(scene)));
        _output.WriteLine("samples to sink  : " + scene.FakeSink!.SamplesHandedOver);
        _output.WriteLine("the operator reads: " + scene.Panel.DigitalSendLine);

        Assert.Equal(Ft8ArmOutcome.Ran, result.Outcome);
        Assert.True(run!.Sent, run.Reason);
        Assert.Equal(new[] { KeyOn, KeyOff }, Frames(scene));
        Assert.Equal(clicked, sent.Transmission.Text);
        Assert.Equal(SlotGrid.Ft4, sent.Grid);

        // **THE SAMPLES THE SINK WAS HANDED, DECODED THROUGH THE TAB'S OWN PATH.**
        // Placed 0.5 s into a 7.5 s slot, which is where the sequence was told the
        // transmission starts.
        var rate = sent.Transmission.SampleRate;
        var whole = new float[(int)Math.Round(SlotGrid.Ft4.SlotSeconds * rate)];
        var offsetSamples = (int)Math.Round(MainWindowViewModel.StartSecondsIntoSlot * rate);

        Array.Copy(
            scene.FakeSink.LastSamples, 0, whole, offsetSamples,
            Math.Min(scene.FakeSink.LastSamples.Length, whole.Length - offsetSamples));

        var endedAt = new DateTime(2026, 9, 9, 12, 0, 0, DateTimeKind.Utc);
        var heard = scene.Panel.ShowDecodes(
            new MonoAudio(rate, whole), endedAt, new ClockOffset(0, endedAt.AddSeconds(-1)));

        var texts = heard.Decodes.Select(d => d.Message).ToList();
        var matched = texts.Count(t => string.Equals(t, clicked, StringComparison.Ordinal));

        _output.WriteLine(string.Empty);
        _output.WriteLine("slots cut        : " + heard.SlotsDecoded
            + " on " + scene.Panel.DigitalGrid.Describe());
        _output.WriteLine("decoder named    : "
            + string.Join(", ", heard.Slots.Select(s => s.Decoder.Name).Distinct()));
        _output.WriteLine("CLICKED          : \"" + clicked + "\"");
        _output.WriteLine("DECODED          : "
            + (texts.Count == 0 ? "nothing" : "\"" + string.Join("\", \"", texts) + "\""));
        _output.WriteLine("transmissions in : 1");
        _output.WriteLine("MISSED           : " + (matched > 0 ? 0 : 1));
        _output.WriteLine("WRONG            : " + (texts.Count - matched));

        Assert.All(heard.Slots, s => Assert.Equal("Ft8Sharp", s.Decoder.Name));
        Assert.Contains(clicked!, texts, StringComparer.Ordinal);
        Assert.Equal(texts.Count, matched);

        _output.WriteLine(string.Empty);
        _output.WriteLine(
            "WHICH HALF IS EVIDENCE: FakeSink and FakePort. **No sound was made and "
            + "this is not a loopback run.**");
    }

    // -------------------------------------------------------------------------
    // The scene.
    // -------------------------------------------------------------------------

    /// <summary>The window, the panel, the wire and whichever sink was built.</summary>
    private sealed class Built(
        MainWindow window,
        MainWindowViewModel panel,
        FakePort port,
        WasapiTransmitSink? card,
        FakeSink? fakeSink,
        JsonlTelemetry telemetry) : IDisposable
    {
        private bool _telemetryClosed;

        public MainWindow Window { get; } = window;

        public MainWindowViewModel Panel { get; } = panel;

        public FakePort Port { get; } = port;

        /// <summary>The real card, where one was asked for.</summary>
        public WasapiTransmitSink Sink => card!;

        /// <summary>The fake card, where no card was asked for.</summary>
        public FakeSink? FakeSink { get; } = fakeSink;

        public void CloseTelemetry()
        {
            if (_telemetryClosed)
            {
                return;
            }

            _telemetryClosed = true;
            telemetry.Dispose();
        }

        public void Dispose()
        {
            try
            {
                Window.Close();
            }
            catch (Exception)
            {
                // The window is scenery; a failure closing it must not mask the run.
            }

            CloseTelemetry();
            card?.Dispose();
        }
    }

    /// <summary>
    /// The real window, the fake wire, one FT4 row, and either a real card or a
    /// fake one.
    /// </summary>
    private Built Scene(RenderEndpoint? endpoint)
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN00";
        settings.Operator.LicenseClass = LicenseClass.General;
        settings.AudioOutputDeviceId = endpoint?.Id ?? "{0.0.0.00000000}.{a-fake-endpoint}";

        var telemetry = new JsonlTelemetry(
            _folder,
            "1.12.238",
            category => settings.IsTelemetryEnabled(category),
            settings.TelemetryMaxMegabytes * 1024L * 1024L);

        var panel = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
            DigitalDecodedExpanded = true,
        };

        panel.UseModeForTests(DigitalMode.Ft4);

        var window = new MainWindow { DataContext = panel, Width = 1400, Height = 1400 };

        window.Show();

        Pump(window);

        panel.SelectedBand = panel.Bands.First(
            b => b.Band.LowHz <= Ft4On20m && b.Band.HighHz >= Ft4On20m);
        panel.FrequencyHz = Ft4On20m;

        // **ONE ROW, WITH NO MEASURED RATIO**, which is every FT4 row in this tree.
        panel.AddDecodeRowForTests(
            RowSlot.ToString("HHmmss", CultureInfo.InvariantCulture),
            DigitalDecodeRow.NoMeasurement, "0.2", "1240", HeardFromHim, RowSlot);

        var port = new FakePort();
        WasapiTransmitSink? real = null;
        FakeSink? fake = null;

        if (endpoint is null)
        {
            fake = new FakeSink();
            panel.TransmitSinkFactory = _ => fake;
        }
        else
        {
            panel.TransmitSinkFactory = name =>
            {
                real = new WasapiTransmitSink(name);

                return real;
            };
        }

        panel.UseRigPortForTests(port);
        panel.BuildTheArmedSend(port);

        Assert.True(panel.HasSomethingToTransmitThrough, panel.DigitalSendLine);

        Pump(window);

        return new Built(window, panel, port, real, fake, telemetry);
    }

    // -------------------------------------------------------------------------
    // The gesture, and the item under the mouse.
    // -------------------------------------------------------------------------

    /// <summary>Raises a real context request on the row that names him.</summary>
    private static MenuFlyout RightClick(Built scene)
    {
        Pump(scene.Window);

        var lists = scene.Window.GetVisualDescendants()
            .OfType<ItemsControl>()
            .Where(c => c.Name is "DigitalDecodedRows" or "DigitalMineRows")
            .ToList();

        Assert.True(lists.Count > 0, "neither decoded list is on the realized window");

        // **`Panel` AND NOT `Grid`, SINCE UNIT 280.** The For-you side became a
        // conversation of bubbles that day and its row root is a `StackPanel`, so a
        // `Grid` search finds the left list and silently misses the other - which is
        // where a row addressed to the operator is drawn. `Panel` covers both roots.
        var grid = lists
            .SelectMany(l => l.GetVisualDescendants().OfType<Panel>())
            .FirstOrDefault(g => g.DataContext is DigitalDecodeRow row
                && row.Sender == His && row.Addressee == Mine);

        Assert.True(
            grid is not null,
            "no realized row from " + His + " addressed to " + Mine
            + ". Left rows: " + scene.Panel.DigitalVisibleDecodes.Count
            + "; mine rows: " + scene.Panel.DigitalMineDecodes.Count
            + "; realized row roots with a row DataContext: "
            + lists.SelectMany(l => l.GetVisualDescendants().OfType<Panel>())
                .Count(p => p.DataContext is DigitalDecodeRow));

        grid!.RaiseEvent(new ContextRequestedEventArgs
        {
            RoutedEvent = Control.ContextRequestedEvent,
            Source = grid,
        });

        var flyout = scene.Window.SendFlyoutUnderTheMouse;

        Assert.True(flyout is not null, "the right-click produced no menu");

        return flyout!;
    }

    /// <summary>The clickable messages - everything that carries the command.</summary>
    private static List<MenuItem> Options(MenuFlyout flyout)
        => flyout.Items.OfType<MenuItem>().Where(i => i.Command is not null).ToList();

    /// <summary>The item an operator would click, and what it is carrying.</summary>
    private static MenuItem TheOneThatComesNext(MenuFlyout flyout)
    {
        var options = Options(flyout);

        Assert.NotEmpty(options);

        var marked = options.FirstOrDefault(
            i => (i.Header as string ?? "").Contains(
                "the one that comes next", StringComparison.Ordinal));

        return marked ?? options[0];
    }

    /// <summary>What the run says on a machine with no card, and why that is fine.</summary>
    private void NoEndpoint(string why)
    {
        _output.WriteLine("NOT ATTEMPTED: " + why + " (SHACK_FACTS.md FACT-004)");
        _output.WriteLine(
            "That is a fact about this machine and not a failure - unit 256's precedent. "
            + "The endpoints this machine does have are printed by "
            + nameof(WhatRenderEndpointsThisMachineHas) + ", and the whole chain is proved "
            + "through FakeSink and FakePort by "
            + nameof(TheSameChainThroughTheFakeSinkDecodesTheSamplesItWasHanded)
            + " - which is NOT a loopback run and is not reported as one.");
    }

    /// <summary>**Which render endpoints this machine actually has.**</summary>
    /// <remarks>
    /// **A MACHINE WITH NO RENDER ENDPOINT IS A NORMAL MACHINE** (unit 256's
    /// precedent). Which half of criterion 4 is evidence turns on this, so it is
    /// printed rather than assumed either way.
    /// </remarks>
    [Fact]
    public void WhatRenderEndpointsThisMachineHas()
    {
        var endpoints = WasapiTransmitSink.Endpoints();

        _output.WriteLine("active render endpoints: " + endpoints.Count);

        foreach (var endpoint in endpoints)
        {
            _output.WriteLine(
                "    " + endpoint.Name + " at " + endpoint.SampleRate + " Hz"
                + (endpoint.IsDefault ? "  (default)" : ""));
        }
    }

    // -------------------------------------------------------------------------
    // The card, the capture, and what silence is.
    // -------------------------------------------------------------------------

    /// <summary>
    /// **One FT4 slot of captured audio with the tones 0.5 s in.**
    /// </summary>
    /// <param name="captured">Everything the loopback tap holds.</param>
    /// <param name="onset">Where the tones start, in samples from the capture's start.</param>
    /// <returns>7.5 s of audio, cut so the signal sits where it was transmitted.</returns>
    /// <remarks>
    /// **THE ONSET IS MEASURED AND PRINTED, NOT ASSUMED.** The capture starts a
    /// second before the click and the transmission begins whenever the sequence
    /// hands the sink its samples; placing the slot from a guess would be this test
    /// deciding where a signal is, which is the fault §0.0 exists for. The threshold
    /// is a tenth of the capture's own peak, so it is relative to what was actually
    /// heard and not to a level this file chose.
    /// </remarks>
    private static MonoAudio OneFt4SlotAround(MonoAudio captured, out int onset)
    {
        var peak = 0.0f;

        foreach (var sample in captured.Samples)
        {
            peak = Math.Max(peak, Math.Abs(sample));
        }

        var threshold = peak * 0.1f;

        onset = 0;

        for (var i = 0; i < captured.Samples.Length; i++)
        {
            if (Math.Abs(captured.Samples[i]) >= threshold)
            {
                onset = i;
                break;
            }
        }

        var lead = (int)Math.Round(
            MainWindowViewModel.StartSecondsIntoSlot * captured.SampleRate);
        var from = Math.Max(0, onset - lead);
        var length = (int)Math.Round(SlotGrid.Ft4.SlotSeconds * captured.SampleRate);
        var slot = new float[length];

        Array.Copy(
            captured.Samples, from, slot, 0,
            Math.Min(length, captured.Samples.Length - from));

        return new MonoAudio(captured.SampleRate, slot);
    }

    /// <summary>A loopback capture on one endpoint, into the engine's own tap.</summary>
    /// <remarks>
    /// **THE ENGINE'S OWN DOWNMIX, NOT A SECOND ONE** - the same conversion the
    /// receive path uses on every buffer the radio delivers, and the same
    /// <see cref="AudioTap"/>. Lifted in shape from unit 268's own
    /// <c>TheWholeChainRunsFromOneRightClickTests</c>.
    /// </remarks>
    private sealed class Capture : IDisposable
    {
        private readonly MMDeviceEnumerator _enumerator = new();
        private readonly MMDevice _device;
        private readonly NAudio.Wave.WasapiLoopbackCapture _capture;
        private readonly ManualResetEventSlim _stopped = new(false);
        private readonly AudioTap _tap = new();
        private float[] _mono = [];
        private long _packets;

        public Capture(RenderEndpoint endpoint)
        {
            _device = _enumerator.GetDevice(endpoint.Id);
            _capture = new NAudio.Wave.WasapiLoopbackCapture(_device);

            _capture.DataAvailable += (_, e) =>
            {
                Interlocked.Increment(ref _packets);

                if (e.BytesRecorded <= 0)
                {
                    return;
                }

                var frames = WasapiAudioSource.Downmix(
                    e.Buffer, e.BytesRecorded, _capture.WaveFormat, ref _mono);

                if (frames > 0)
                {
                    _tap.Take(_mono.AsSpan(0, frames), _capture.WaveFormat.SampleRate);
                }
            };

            _capture.RecordingStopped += (_, _) => _stopped.Set();
        }

        /// <summary>How many samples have reached the tap.</summary>
        public long SamplesSeen => _tap.SamplesSeen;

        /// <summary>Starts listening.</summary>
        public void Start() => _capture.StartRecording();

        /// <summary>Stops listening and waits for the device to say so.</summary>
        public void Stop()
        {
            _capture.StopRecording();
            _stopped.Wait(TimeSpan.FromSeconds(5));
        }

        /// <summary>Everything the tap is holding.</summary>
        public MonoAudio? Snapshot() => _tap.Snapshot();

        /// <inheritdoc/>
        public void Dispose()
        {
            _capture.Dispose();
            _device.Dispose();
            _enumerator.Dispose();
            _stopped.Dispose();
        }
    }

    /// <summary>The loudest and the average of a stretch of captured audio.</summary>
    private readonly record struct CapturedLevel(double Peak, double PeakDb, double RmsDb);

    /// <summary>What says a card made a sound, and what says it did not.</summary>
    private static CapturedLevel Level(ReadOnlySpan<float> samples)
    {
        var peak = 0.0;
        var squares = 0.0;

        foreach (var sample in samples)
        {
            var value = Math.Abs((double)sample);

            if (value > peak)
            {
                peak = value;
            }

            squares += (double)sample * sample;
        }

        var rms = samples.Length > 0 ? Math.Sqrt(squares / samples.Length) : 0.0;

        return new CapturedLevel(peak, Decibels(peak), Decibels(rms));
    }

    /// <summary>A fraction of full scale in dBFS, floored rather than infinite.</summary>
    private static double Decibels(double value)
        => value <= 0
            ? AudioLevel.SilenceDb
            : Math.Max(AudioLevel.SilenceDb, 20 * Math.Log10(value));

    /// <summary>A decibel figure as a reader sees it.</summary>
    private static string Db(double db)
        => db <= AudioLevel.SilenceDb
            ? "<= " + AudioLevel.SilenceDb.ToString("F1", CultureInfo.InvariantCulture)
            : db.ToString("F1", CultureInfo.InvariantCulture);

    /// <summary>The endpoint this test will make sound on, and why it was chosen.</summary>
    /// <remarks>
    /// **CHOSEN, NOT DEFAULTED TO**, on unit 256's reasoning: this plays tones out of
    /// a computer somebody owns and may be asleep near, so the endpoint is picked
    /// deliberately and named in the output.
    /// </remarks>
    private static RenderEndpoint? Preferred(out string why)
    {
        var endpoints = WasapiTransmitSink.Endpoints();

        if (endpoints.Count == 0)
        {
            why = "this machine has no active render endpoint at all";

            return null;
        }

        var quiet = endpoints.FirstOrDefault(
            e => e.Name.Contains("Display Audio", StringComparison.OrdinalIgnoreCase));

        if (quiet is not null)
        {
            why = "a display-audio endpoint - a monitor's audio path rather than the "
                + $"machine's speakers - chosen from {endpoints.Count} active render endpoints, "
                + $"and it is {(quiet.IsDefault ? "also" : "not")} the default";

            return quiet;
        }

        var chosen = endpoints.FirstOrDefault(e => !e.IsDefault) ?? endpoints[0];

        why = "no display-audio endpoint on this machine, so "
            + $"{(chosen.IsDefault ? "the default" : "the first endpoint that is not the default")} "
            + $"was taken from {endpoints.Count} active render endpoints - THIS MAY BE AUDIBLE";

        return chosen;
    }

    /// <summary>Every frame the wire took, as the record shows them.</summary>
    private static string[] Frames(Built scene)
        => scene.Port.Written
            .Select(f => string.Join(' ', f.Select(b => b.ToString("X2", CultureInfo.InvariantCulture))))
            .ToArray();

    /// <summary>The wire in one line.</summary>
    private static string Wire(Built scene) => string.Join(" | ", Frames(scene));

    /// <summary>Lets the window lay itself out.</summary>
    private static void Pump(Window window)
    {
        for (var i = 0; i < 5; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }
}
