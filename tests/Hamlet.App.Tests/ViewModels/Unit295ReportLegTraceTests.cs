using System.Diagnostics;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using Ft8Sharp;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Transmit;
using NAudio.CoreAudioApi;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **Work instruction 295, task 1a - the trace. How far does a measured FT4 signal
/// report actually get?**
/// </summary>
/// <remarks>
/// <para>**THIS FILE CHANGES NOTHING AND PROVES NOTHING BY ITSELF.** It drives the
/// report leg at HEAD and prints every stage, so that the starting position is on the
/// record before task 3 touches anything. Unit 294's trace led its report for the same
/// reason: once the thing is changed, where it stood is unrecoverable.</para>
/// <para>**THE ROW IS MADE THE WAY THE BAND MAKES ONE.** No row is seeded. FT4 audio
/// carrying a station calling the operator **with a grid** is synthesized at a
/// commanded ratio, put through <c>ShowDecodes</c> - the tab's own path, in FT4 - and
/// whatever ratio that decode measures is what the <c>snr</c> cell shows. A seeded
/// ratio would be this file deciding the number the whole trace is about.</para>
/// <para>**HE SENT A GRID, SO WHAT CONVENTIONALLY COMES NEXT IS A REPORT** - not the
/// <c>RRR</c> unit 293 was confined to. That is the whole difference between this
/// trace and unit 293's proof.</para>
/// <para>**NOTHING HERE OPENS A SERIAL PORT** (<c>SHACK_FACTS.md</c> FACT-004). The
/// wire is <see cref="FakePort"/>. Where a render endpoint exists the sound is real
/// and the capture is a real WASAPI loopback; where there is none, that is said
/// plainly and no fake run is dressed as a loopback.</para>
/// </remarks>
public sealed class Unit295ReportLegTraceTests : IDisposable
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

    /// <summary>His grid, which is what makes a report the next thing to send.</summary>
    private const string HisGrid = "FN31";

    /// <summary>What the operator hears from him: a call to him, carrying a grid.</summary>
    private const string HeardFromHim = Mine + " " + His + " " + HisGrid;

    /// <summary>What a sound card delivers, and what the slot is synthesized at.</summary>
    private const int DeviceRate = 12_000;

    /// <summary>Where the synthesized transmission is put, in the passband.</summary>
    private const float PlacedAtHz = 1240.0f;

    /// <summary>The ratio commanded into the synthesized slot, in 2500 Hz.</summary>
    private const double Commanded = -8.0;

    /// <summary>The moment the received recording ended, on an FT4 boundary.</summary>
    private static readonly DateTime HeardEndedAt =
        new(2026, 9, 9, 14, 22, 45, DateTimeKind.Utc);

    /// <summary>The clock, measured at no drift.</summary>
    private static ClockOffset Measured =>
        new(0, new DateTime(2026, 9, 9, 14, 20, 0, DateTimeKind.Utc));

    /// <summary>What the radio sees when it is keyed.</summary>
    private const string KeyOn = "FE FE 94 E0 1C 00 01 FD";

    /// <summary>The ordinary unkey.</summary>
    private const string KeyOff = "FE FE 94 E0 1C 00 00 FD";

    /// <summary>Where the application's own telemetry writer is pointed.</summary>
    private readonly string _folder = Path.Combine(
        Path.GetTempPath(), "hamlet-unit295-" + Guid.NewGuid().ToString("N")[..8]);

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where every measured figure is printed.</param>
    public Unit295ReportLegTraceTests(ITestOutputHelper output) => _output = output;

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
    /// **The report leg, driven end to end at HEAD, with every stage printed.**
    /// </summary>
    [AvaloniaFact]
    public async Task HowFarAMeasuredFt4ReportGets()
    {
        var endpoint = Preferred(out var why);

        _output.WriteLine("---- TASK 1a: THE REPORT LEG, DRIVEN ----");
        _output.WriteLine("endpoint chosen  : "
            + (endpoint is null ? "NONE - " + why : endpoint.Name + " (" + why + ")"));

        using var scene = Scene(endpoint);

        // ---- 0. THE ROW, MADE THE WAY THE BAND MAKES ONE ----------------------
        var received = SynthesizeOneFt4Slot(out var delivered);
        var incoming = scene.Panel.ShowDecodes(received, HeardEndedAt, Measured);

        Pump(scene.Window);

        var texts = incoming.Decodes.Select(d => d.Message).ToList();

        _output.WriteLine("mode the tab runs: " + scene.Panel.DigitalMode
            + " on " + scene.Panel.DigitalGrid.Describe());
        _output.WriteLine("dial             : " + scene.Panel.FrequencyHz + " Hz");
        _output.WriteLine("synthesized      : \"" + HeardFromHim + "\" at "
            + PlacedAtHz.ToString("F0", CultureInfo.InvariantCulture) + " Hz");
        _output.WriteLine("delivered ratio  : "
            + delivered.ToString("F2", CultureInfo.InvariantCulture) + " dB in 2500 Hz");
        _output.WriteLine("decoded back     : "
            + (texts.Count == 0 ? "nothing" : "\"" + string.Join("\", \"", texts) + "\""));

        var row = scene.Panel.DigitalDecodes.FirstOrDefault(
            r => r.Sender == His && r.Addressee == Mine);

        if (row is null)
        {
            _output.WriteLine(string.Empty);
            _output.WriteLine("STOPPED AT STAGE 0: no row from " + His + " to " + Mine
                + " reached the table. Rows on the table: "
                + scene.Panel.DigitalDecodes.Count);
            Assert.Fail("the incoming slot produced no row to right-click");
        }

        // ---- 1. THE RATIO THE ROW'S snr CELL SHOWS -----------------------------
        _output.WriteLine(string.Empty);
        _output.WriteLine("1. THE snr CELL  : \"" + row!.Snr + "\"");
        _output.WriteLine("   the row reads : " + row.Utc + " | " + row.Snr + " | "
            + row.Dt + " | " + row.Hz + " | " + row.Message);

        // ---- 2. THE MENU, AND THE EXACT CHARACTERS ON THE ITEM -----------------
        var flyout = RightClick(scene);
        var options = Options(flyout);
        var item = TheReportItem(flyout, out var found);

        _output.WriteLine(string.Empty);
        _output.WriteLine("2. THE MENU      : " + options.Count + " clickable messages");

        foreach (var option in options)
        {
            _output.WriteLine("     " + (option.Header as string ?? "")
                + "   ->   \"" + (option.CommandParameter as string ?? "") + "\"");
        }

        if (item is null)
        {
            _output.WriteLine(string.Empty);
            _output.WriteLine("STOPPED AT STAGE 2: " + found);
            Assert.Fail("the menu carried no report item: " + found);
        }

        var header = item!.Header as string ?? "";
        var clicked = item.CommandParameter as string ?? "";

        _output.WriteLine(string.Empty);
        _output.WriteLine("   HE CLICKED    : " + header);
        _output.WriteLine("   WHICH CARRIES : \"" + clicked + "\" ("
            + clicked.Length + " characters)");

        // ---- 3. THE COMPOSER, THE GRID AND THE BOUNDARY ------------------------
        using var capture = endpoint is null ? null : new Capture(endpoint);

        capture?.Start();

        if (capture is not null)
        {
            await Task.Delay(PreRoll);
        }

        var clock = Stopwatch.StartNew();

        item.Command!.Execute(clicked);

        var slot = scene.Panel.ArmedForSlotUtc;

        if (slot is null)
        {
            _output.WriteLine(string.Empty);
            _output.WriteLine("STOPPED AT STAGE 3: nothing armed. The send line reads: "
                + scene.Panel.DigitalSendLine);
            Assert.Fail("the click armed nothing: " + scene.Panel.DigitalSendLine);
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("3. ARMED         : " + scene.Panel.DigitalSendLine);
        _output.WriteLine("   armed for slot: "
            + slot!.Value.ToString("O", CultureInfo.InvariantCulture));

        var result = await scene.Panel.AtSlotBoundaryAsync(slot.Value);

        if (capture is not null)
        {
            await Task.Delay(PostRoll);
            capture.Stop();
        }

        clock.Stop();
        Pump(scene.Window);

        var run = result!.Run;
        var sent = result.Send;
        var played = run?.Played ?? new PlayedAudio(0, TimeSpan.Zero);

        _output.WriteLine("   outcome       : " + result.Outcome);

        if (sent is null)
        {
            _output.WriteLine(string.Empty);
            _output.WriteLine("STOPPED AT STAGE 3: the boundary composed nothing. "
                + "Reason: " + run?.Reason);
            Assert.Fail("the boundary composed nothing: " + run?.Reason);
        }

        _output.WriteLine("   COMPOSER GOT  : \"" + sent!.Transmission.Text + "\"");
        _output.WriteLine("   grid on send  : " + sent.Grid.Describe()
            + ", named " + sent.Grid.Name);
        _output.WriteLine("   composed at   : " + sent.Transmission.SampleRate + " Hz, "
            + sent.Transmission.Samples.Length + " samples, "
            + sent.Transmission.SlotSeconds.ToString("0.###", CultureInfo.InvariantCulture)
            + " s");

        // ---- 4. THE WIRE -------------------------------------------------------
        _output.WriteLine(string.Empty);
        _output.WriteLine("4. KEYED         : " + run?.Keyed);
        _output.WriteLine("   came out of tx: " + run?.CameOutOfTransmit);
        _output.WriteLine("   frames        : " + Wire(scene));
        _output.WriteLine("   samples played: " + played.SamplesPlayed);
        _output.WriteLine("   reason        : " + run?.Reason);
        _output.WriteLine("   wall clock    : "
            + clock.Elapsed.TotalSeconds.ToString("F2", CultureInfo.InvariantCulture) + " s");

        // ---- 5. WHAT CAME BACK -------------------------------------------------
        string[] back;
        string half;

        if (capture is not null)
        {
            var captured = capture.Snapshot();

            if (captured is null || captured.Samples.Length == 0)
            {
                _output.WriteLine(string.Empty);
                _output.WriteLine("STOPPED AT STAGE 5: the loopback delivered nothing - "
                    + capture.SamplesSeen + " samples reached the tap.");
                Assert.Fail("the loopback capture delivered nothing");
            }

            var level = Level(captured!.Samples);
            var slotAudio = OneFt4SlotAround(captured, out var onset);

            _output.WriteLine(string.Empty);
            _output.WriteLine("5. CAPTURED      : " + captured.Samples.Length
                + " samples at " + captured.SampleRate + " Hz");
            _output.WriteLine("   captured peak : " + Db(level.PeakDb) + " dBFS");
            _output.WriteLine("   tone onset    : " + onset + " samples");

            var readBack = scene.Panel.ShowDecodes(
                slotAudio,
                new DateTime(2026, 9, 9, 12, 0, 0, DateTimeKind.Utc),
                new ClockOffset(0, new DateTime(2026, 9, 9, 11, 59, 59, DateTimeKind.Utc)));

            back = readBack.Decodes.Select(d => d.Message).ToArray();
            half = "A REAL WASAPI RENDER ENDPOINT AND A REAL LOOPBACK CAPTURE on "
                + scene.Sink.DeviceName;
        }
        else
        {
            var rate = sent.Transmission.SampleRate;
            var whole = new float[(int)Math.Round(SlotGrid.Ft4.SlotSeconds * rate)];
            var offsetSamples = (int)Math.Round(
                MainWindowViewModel.StartSecondsIntoSlot * rate);

            Array.Copy(
                scene.FakeSink!.LastSamples, 0, whole, offsetSamples,
                Math.Min(scene.FakeSink.LastSamples.Length, whole.Length - offsetSamples));

            var readBack = scene.Panel.ShowDecodes(
                new MonoAudio(rate, whole),
                new DateTime(2026, 9, 9, 12, 0, 0, DateTimeKind.Utc),
                new ClockOffset(0, new DateTime(2026, 9, 9, 11, 59, 59, DateTimeKind.Utc)));

            back = readBack.Decodes.Select(d => d.Message).ToArray();
            half = "THE FAKE SINK. NO SOUND WAS MADE AND THIS IS NOT A LOOPBACK RUN.";
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("6. DECODED BACK  : "
            + (back.Length == 0 ? "nothing" : "\"" + string.Join("\", \"", back) + "\""));
        _output.WriteLine("   WHICH HALF    : " + half);

        // ---- THE NUMBER THAT MATTERS ------------------------------------------
        var arrived = back.FirstOrDefault(t => t.StartsWith(His + " " + Mine, StringComparison.Ordinal))
            ?? back.FirstOrDefault() ?? "";

        _output.WriteLine(string.Empty);
        _output.WriteLine("---- THE NUMBER ----");
        _output.WriteLine("   the row showed: " + row.Snr);
        _output.WriteLine("   the menu sent : \"" + clicked + "\"");
        _output.WriteLine("   the air gave  : \"" + arrived + "\"");
        _output.WriteLine("   the same text : "
            + string.Equals(clicked, arrived, StringComparison.Ordinal));

        Assert.Equal(clicked, arrived);
    }

    // -------------------------------------------------------------------------
    // Task 1b - the three tooltips, quoted verbatim from the tree.
    // -------------------------------------------------------------------------

    /// <summary>**The three decode-column tooltips as the tree holds them.**</summary>
    [Fact]
    public void TheThreeTooltipsAsTheTreeHoldsThem()
    {
        var root = Root();
        var app = Path.Combine(root, "src", "Hamlet.App", "App.axaml");
        var window = Path.Combine(root, "src", "Hamlet.App", "Views", "MainWindow.axaml");
        var lines = File.ReadAllLines(app);
        var windowLines = File.ReadAllLines(window);

        foreach (var key in new[]
                 { "HmDecodeUtcHelp", "HmDecodeSnrHelp", "HmDecodeContactHelp" })
        {
            var at = Array.FindIndex(lines, l => l.Contains("x:Key=\"" + key + "\"", StringComparison.Ordinal));
            var bound = new List<int>();

            for (var i = 0; i < windowLines.Length; i++)
            {
                if (windowLines[i].Contains(key, StringComparison.Ordinal))
                {
                    bound.Add(i + 1);
                }
            }

            _output.WriteLine(key);
            _output.WriteLine("  App.axaml line   : " + (at < 0 ? "NOT FOUND" : (at + 1).ToString(CultureInfo.InvariantCulture)));
            _output.WriteLine("  bound in MainWindow.axaml at: "
                + (bound.Count == 0 ? "NOWHERE" : string.Join(", ", bound)));
            _output.WriteLine("  says             : " + (at < 0 ? "" : lines[at].Trim()));
            _output.WriteLine(string.Empty);
        }

        // Every other place in src/ that names one of the three.
        _output.WriteLine("every other file under src/ naming any of the three:");

        foreach (var file in Directory.EnumerateFiles(
                     Path.Combine(root, "src"), "*.*", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                || file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            {
                continue;
            }

            if (Path.GetExtension(file) is not (".cs" or ".axaml"))
            {
                continue;
            }

            var text = File.ReadAllText(file);

            foreach (var key in new[]
                     { "HmDecodeUtcHelp", "HmDecodeSnrHelp", "HmDecodeContactHelp" })
            {
                if (text.Contains(key, StringComparison.Ordinal))
                {
                    _output.WriteLine("  " + key + "  in  "
                        + Path.GetRelativePath(root, file));
                }
            }
        }
    }

    /// <summary>The repository root, found by walking up to the solution.</summary>
    private static string Root()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Hamlet.sln")))
        {
            dir = dir.Parent;
        }

        Assert.True(dir is not null, "no Hamlet.sln above " + AppContext.BaseDirectory);

        return dir!.FullName;
    }

    // -------------------------------------------------------------------------
    // The incoming slot.
    // -------------------------------------------------------------------------

    /// <summary>Fifteen seconds of audio with one FT4 transmission in it.</summary>
    /// <param name="delivered">What ratio the drawn noise actually delivered.</param>
    /// <remarks>
    /// **THE SAME ARITHMETIC UNIT 294'S OWN LADDER USES**, so the ratio is a fact
    /// about this buffer rather than about the sigma that was asked for.
    /// </remarks>
    private static MonoAudio SynthesizeOneFt4Slot(out double delivered)
    {
        var message = new byte[Ft8Payload.MessageBytes];

        Assert.Equal(
            Ft8PackResult.Ok,
            Ft8StandardMessage.TryPack(Mine, His, HisGrid, message));

        var symbols = Ft4SymbolEncoder.Encode(message);
        var signal = Ft4Waveform.Synthesize(symbols, DeviceRate, PlacedAtHz);

        var samples = new float[DeviceRate * 15];
        var lead = (DeviceRate * 15 / 2) + Ft4Waveform.PaddingSampleCount(DeviceRate);

        signal.CopyTo(samples.AsSpan(lead));

        var signalPower = MeanSquare(signal);
        var sigma = NoiseAmplitudeFor(signalPower, Commanded, DeviceRate);
        var random = new Random(295);
        var noisePower = 0.0;

        for (var i = 0; i < samples.Length; i++)
        {
            var draw = Gaussian(random) * sigma;
            noisePower += draw * draw;
            samples[i] = (float)(samples[i] + draw);
        }

        noisePower /= samples.Length;
        delivered = DecibelsFor(signalPower, noisePower, DeviceRate);

        return new MonoAudio(DeviceRate, samples);
    }

    private static double MeanSquare(ReadOnlySpan<float> samples)
    {
        var sum = 0.0;

        foreach (var sample in samples)
        {
            sum += (double)sample * sample;
        }

        return sum / samples.Length;
    }

    private static double NoiseAmplitudeFor(double signalPower, double decibels, int sampleRate)
    {
        var noiseInReference = signalPower / Math.Pow(10.0, decibels / 10.0);

        return Math.Sqrt(noiseInReference * (sampleRate / 2.0) / 2500.0);
    }

    private static double DecibelsFor(double signalPower, double totalNoisePower, int sampleRate)
        => 10.0 * Math.Log10(
            signalPower / (totalNoisePower * 2500.0 / (sampleRate / 2.0)));

    /// <summary>One standard normal, Box-Muller in its polar form.</summary>
    private static double Gaussian(Random random)
    {
        double u, v, s;

        do
        {
            u = (random.NextDouble() * 2) - 1;
            v = (random.NextDouble() * 2) - 1;
            s = (u * u) + (v * v);
        }
        while (s >= 1.0 || s == 0.0);

        return u * Math.Sqrt(-2.0 * Math.Log(s) / s);
    }

    // -------------------------------------------------------------------------
    // The scene, lifted in shape from unit 293's own.
    // -------------------------------------------------------------------------

    private sealed class Built(
        MainWindow window,
        MainWindowViewModel panel,
        FakePort port,
        WasapiTransmitSink? card,
        FakeSink? fakeSink,
        JsonlTelemetry telemetry) : IDisposable
    {
        public MainWindow Window { get; } = window;

        public MainWindowViewModel Panel { get; } = panel;

        public FakePort Port { get; } = port;

        public WasapiTransmitSink Sink => card!;

        public FakeSink? FakeSink { get; } = fakeSink;

        public void Dispose()
        {
            try
            {
                Window.Close();
            }
            catch (Exception)
            {
                // The window is scenery.
            }

            telemetry.Dispose();
            card?.Dispose();
        }
    }

    /// <summary>The real window, the fake wire, and either a real card or a fake one.</summary>
    private Built Scene(RenderEndpoint? endpoint)
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN00";
        settings.Operator.LicenseClass = LicenseClass.General;
        settings.AudioOutputDeviceId = endpoint?.Id ?? "{0.0.0.00000000}.{a-fake-endpoint}";

        var telemetry = new JsonlTelemetry(
            _folder,
            "1.12.246",
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

    /// <summary>Raises a real context request on the row that names him.</summary>
    private static MenuFlyout RightClick(Built scene)
    {
        Pump(scene.Window);

        var lists = scene.Window.GetVisualDescendants()
            .OfType<ItemsControl>()
            .Where(c => c.Name is "DigitalDecodedRows" or "DigitalMineRows")
            .ToList();

        Assert.True(lists.Count > 0, "neither decoded list is on the realized window");

        var grid = lists
            .SelectMany(l => l.GetVisualDescendants().OfType<Panel>())
            .FirstOrDefault(g => g.DataContext is DigitalDecodeRow row
                && row.Sender == His && row.Addressee == Mine);

        Assert.True(
            grid is not null,
            "no realized row from " + His + " addressed to " + Mine
            + ". Left rows: " + scene.Panel.DigitalVisibleDecodes.Count
            + "; mine rows: " + scene.Panel.DigitalMineDecodes.Count);

        grid!.RaiseEvent(new ContextRequestedEventArgs
        {
            RoutedEvent = Control.ContextRequestedEvent,
            Source = grid,
        });

        var flyout = scene.Window.SendFlyoutUnderTheMouse;

        Assert.True(flyout is not null, "the right-click produced no menu");

        return flyout!;
    }

    private static List<MenuItem> Options(MenuFlyout flyout)
        => flyout.Items.OfType<MenuItem>().Where(i => i.Command is not null).ToList();

    /// <summary>The item carrying a signal report, and why not where there is none.</summary>
    private static MenuItem? TheReportItem(MenuFlyout flyout, out string why)
    {
        // **THE HEADER IS THE MESSAGE FOLLOWED BY THE LABEL** - `MainWindow.axaml.cs:274`
        // composes `<text>   <label>` and appends " - the one that comes next" to the
        // expected one. The report item is the one whose label is exactly `report`,
        // which is the label `Ft8SendOptions.LabelFor` gives `Ft8SendShape.Report`.
        var options = Options(flyout);
        var report = options.FirstOrDefault(
            i => (i.Header as string ?? "").Contains("   report", StringComparison.Ordinal)
                && !(i.Header as string ?? "").Contains("roger and report", StringComparison.Ordinal));

        why = report is null
            ? "no item labelled \"report\" among "
                + string.Join(", ", options.Select(o => "\"" + (o.Header as string ?? "") + "\""))
            : "found, and it is "
                + ((report.Header as string ?? "").Contains(
                    "the one that comes next", StringComparison.Ordinal)
                    ? "marked as the one that conventionally comes next"
                    : "NOT marked as the one that comes next");

        return report;
    }

    // -------------------------------------------------------------------------
    // The card, the capture, and what silence is - unit 293's own helpers.
    // -------------------------------------------------------------------------

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

    private sealed class Capture : IDisposable
    {
        private readonly MMDeviceEnumerator _enumerator = new();
        private readonly MMDevice _device;
        private readonly NAudio.Wave.WasapiLoopbackCapture _capture;
        private readonly ManualResetEventSlim _stopped = new(false);
        private readonly AudioTap _tap = new();
        private float[] _mono = [];

        public Capture(RenderEndpoint endpoint)
        {
            _device = _enumerator.GetDevice(endpoint.Id);
            _capture = new NAudio.Wave.WasapiLoopbackCapture(_device);

            _capture.DataAvailable += (_, e) =>
            {
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

        public long SamplesSeen => _tap.SamplesSeen;

        public void Start() => _capture.StartRecording();

        public void Stop()
        {
            _capture.StopRecording();
            _stopped.Wait(TimeSpan.FromSeconds(5));
        }

        public MonoAudio? Snapshot() => _tap.Snapshot();

        public void Dispose()
        {
            _capture.Dispose();
            _device.Dispose();
            _enumerator.Dispose();
            _stopped.Dispose();
        }
    }

    private readonly record struct CapturedLevel(double Peak, double PeakDb, double RmsDb);

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

    private static double Decibels(double value)
        => value <= 0
            ? AudioLevel.SilenceDb
            : Math.Max(AudioLevel.SilenceDb, 20 * Math.Log10(value));

    private static string Db(double db)
        => db <= AudioLevel.SilenceDb
            ? "<= " + AudioLevel.SilenceDb.ToString("F1", CultureInfo.InvariantCulture)
            : db.ToString("F1", CultureInfo.InvariantCulture);

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
            why = "a display-audio endpoint chosen from " + endpoints.Count
                + " active render endpoints, and it is "
                + (quiet.IsDefault ? "also" : "not") + " the default";

            return quiet;
        }

        var chosen = endpoints.FirstOrDefault(e => !e.IsDefault) ?? endpoints[0];

        why = "no display-audio endpoint on this machine, so "
            + (chosen.IsDefault ? "the default" : "the first that is not the default")
            + " was taken from " + endpoints.Count
            + " active render endpoints - THIS MAY BE AUDIBLE";

        return chosen;
    }

    private static string[] Frames(Built scene)
        => scene.Port.Written
            .Select(f => string.Join(' ', f.Select(b => b.ToString("X2", CultureInfo.InvariantCulture))))
            .ToArray();

    private static string Wire(Built scene) => string.Join(" | ", Frames(scene));

    private static void Pump(Window window)
    {
        for (var i = 0; i < 5; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }
}
