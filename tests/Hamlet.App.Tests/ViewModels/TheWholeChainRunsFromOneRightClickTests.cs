using System.Diagnostics;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Ft8Sharp.Dsp;
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
/// **THE WHOLE CHAIN, FROM ONE RIGHT-CLICK, AT THE BENCH.** Work instruction 268,
/// `PHASE_PLAN.md` step C.
/// </summary>
/// <remarks>
/// <para>**THE SEAM NOBODY HAD MEASURED.** The real endpoint is proved in a plain
/// <c>[Fact]</c> that sends a literal the test chose for itself
/// (<c>TheLoopbackThroughTheApplicationsSendPathTests.cs:84</c>,
/// <c>const string Message = "CQ KC3QIS FN00"</c>), and the real right-click is
/// proved in an <c>[AvaloniaFact]</c> that never keys anything
/// (<c>TheMenuIsUnderTheMouseTests.cs:114</c>). **So no test in this tree would fail
/// if the menu offered one string and the send path transmitted another**, and the
/// operator would be told he sent one thing while another went out over the band.
/// That is the breakage this class exists to catch.</para>
/// <para>**IT IS AN AVALONIA HEADLESS WINDOW THAT ALSO OPENS A WASAPI RENDER
/// ENDPOINT AND A LOOPBACK CAPTURE**, which nothing in the tree did before unit 268
/// - `docs/unit267-what-step-c-needs.md` named it the most expensive thing step C
/// needs.</para>
/// <para>**NOTHING HERE OPENS A SERIAL PORT** (`SHACK_FACTS.md` FACT-004). The
/// device is a sound card and the port is <see cref="FakePort"/>. A real port beside
/// a real sink would be a real transmission and this machine has no radio to make
/// one on. **Nothing measured here says anything about the IC-7300.**</para>
/// <para>**A MACHINE WITH NO RENDER ENDPOINT IS A NORMAL MACHINE.** Where there is
/// none this says so and stops, which is unit 256's precedent carried through
/// <c>TheLoopbackThroughTheApplicationsSendPathTests.cs:69-79</c>. It is a fact about
/// the machine, printed, and not a failure.</para>
/// <para>**IT MAKES REAL SOUND ON A REAL COMPUTER**, one 12.64-second FT8 slot per
/// transmission, with a second of pre-roll and a second of post-roll around it.</para>
/// </remarks>
public sealed class TheWholeChainRunsFromOneRightClickTests : IDisposable
{
    /// <summary>How long to let the capture settle before the click.</summary>
    private static readonly TimeSpan PreRoll = TimeSpan.FromSeconds(1);

    /// <summary>How long to keep capturing after the transmission returns.</summary>
    private static readonly TimeSpan PostRoll = TimeSpan.FromSeconds(1);

    /// <summary>One FT8 slot, which is what the decoder's geometry describes.</summary>
    private const double SlotSeconds = 15.0;

    /// <summary>FT8's watering hole on 20 m, where a General may send data.</summary>
    private const long Ft8On20m = 14_074_000;

    /// <summary>The operator.</summary>
    private const string Mine = "KC3QIS";

    /// <summary>The station he right-clicks.</summary>
    private const string His = "W1ABC";

    /// <summary>What the row says he was heard at, so the menu can offer a report.</summary>
    private const string Report = "-10";

    /// <summary>
    /// **Where the application's own telemetry writer is pointed.**
    /// </summary>
    private readonly string _folder = Path.Combine(
        Path.GetTempPath(), "hamlet-unit268-" + Guid.NewGuid().ToString("N")[..8]);

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where every measured figure is printed.</param>
    public TheWholeChainRunsFromOneRightClickTests(ITestOutputHelper output)
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
    /// **One click through the application's own send path makes a sound on a real
    /// card, inside a headless Avalonia window.**
    /// </summary>
    /// <remarks>
    /// <para>**THE BREAKAGE THIS WOULD HAVE CAUGHT** (`CLAUDE.md`: a unit may not add
    /// a test without naming it): **a send path that keys the radio and plays
    /// nothing**, or plays to an endpoint other than the one named in Settings.
    /// Every app-side send test today runs on <see cref="FakeSink"/>, whose
    /// bookkeeping says a play happened because it was asked to. **Only a card and a
    /// capture can say a sound was made.**</para>
    /// <para>**AND IT IS THE HOST QUESTION, ANSWERED BY BUILDING IT.** Nothing in the
    /// tree was both a headless Avalonia window and an open WASAPI render endpoint,
    /// so this is where that is found out - deliberately before the goal task rather
    /// than inside it.</para>
    /// </remarks>
    [AvaloniaFact]
    public async Task OneClickInAHeadlessWindowMakesARealSoundOnARealCard()
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
        _output.WriteLine("host             : Avalonia headless window, real MainWindow shown");

        using var capture = new Capture(endpoint);

        var clock = Stopwatch.StartNew();

        capture.Start();
        await Task.Delay(PreRoll);

        // **THE CLICK.** Task 3 replaces this with the operator's own right-click
        // on a row and an invoke of the menu item itself; this is the send path,
        // proving the host and the card.
        var message = His + " " + Mine + " " + Report;

        scene.Panel.SendMessageCommand.Execute(message);

        var slot = scene.Panel.ArmedForSlotUtc;

        Assert.True(slot is not null, scene.Panel.DigitalSendLine);

        var result = await scene.Panel.AtSlotBoundaryAsync(slot!.Value);

        await Task.Delay(PostRoll);
        capture.Stop();
        clock.Stop();

        Pump(scene.Window);

        var run = result!.Run;
        var played = run?.Played ?? new PlayedAudio(0, TimeSpan.Zero);

        _output.WriteLine(string.Empty);
        _output.WriteLine("message clicked  : \"" + message + "\"");
        _output.WriteLine("outcome          : " + run?.Outcome);
        _output.WriteLine("reason           : " + run?.Reason);
        _output.WriteLine("came out of tx   : " + run?.CameOutOfTransmit);
        _output.WriteLine("samples played   : " + played.SamplesPlayed);
        _output.WriteLine("peak written     : " + scene.Sink.PeakWritten.ToString("F4"));
        _output.WriteLine("frames on wire   : " + scene.Port.Written.Count);
        _output.WriteLine("wall clock       : " + clock.Elapsed.TotalSeconds.ToString("F2") + " s");

        Assert.Equal(Ft8ArmOutcome.Ran, result.Outcome);

        // **THE RUN'S OUTCOME, NOT THE ARM OUTCOME** - unit 267's recorded trap.
        // `Ft8ArmedSend.cs` returns `Ran` for anything the sequence ran at all.
        Assert.True(run!.Sent, run.Reason);
        Assert.Equal(UnkeyRoute.OrdinaryUnkey, run.CameOutOfTransmit);

        // ---- AND THE CARD MADE A SOUND -----------------------------------------
        var captured = capture.Snapshot();

        Assert.True(
            captured is not null && captured.Samples.Length > 0,
            "the loopback capture started and delivered nothing while the endpoint was "
            + $"rendering - {capture.SamplesSeen} samples reached the tap");

        var level = Level(captured!.Samples);

        _output.WriteLine(string.Empty);
        _output.WriteLine("captured at      : " + captured.SampleRate + " Hz");
        _output.WriteLine("captured samples : " + captured.Samples.Length);
        _output.WriteLine("CAPTURED PEAK    : " + Db(level.PeakDb) + " dBFS ("
            + level.Peak.ToString("F6", CultureInfo.InvariantCulture) + ")");
        _output.WriteLine("captured rms     : " + Db(level.RmsDb) + " dBFS");
        _output.WriteLine("not silent means : peak above " + AudioLevel.TooQuietDb
            + " dBFS (AudioLevel.TooQuietDb)");

        Assert.True(
            level.PeakDb > AudioLevel.TooQuietDb,
            $"the run said it sent and the card was silent: the loopback captured "
            + $"{captured.Samples.Length} samples off {scene.Sink.DeviceName} peaking at "
            + $"{Db(level.PeakDb)} dBFS, which is at or below "
            + $"{AudioLevel.TooQuietDb} dBFS - nothing was played, or it was played "
            + "somewhere else.");
    }

    // -------------------------------------------------------------------------
    // The scene: a real window, a real card, a fake wire.
    // -------------------------------------------------------------------------

    /// <summary>Everything one run needs, and the two things that must be closed.</summary>
    private sealed record Built(
        MainWindow Window,
        MainWindowViewModel Panel,
        AppSettings Settings,
        FakePort Port,
        WasapiTransmitSink Sink,
        JsonlTelemetry Telemetry) : IDisposable
    {
        /// <inheritdoc/>
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

            Telemetry.Dispose();
            Sink.Dispose();
        }
    }

    /// <summary>
    /// The real window, the real sink through the panel's own factory, the fake
    /// wire, and the application's own telemetry writer.
    /// </summary>
    /// <remarks>
    /// <para>**BOTH THE PORT CALLS ARE THERE ON PURPOSE**
    /// (`docs/unit268-the-chain-trace.md` question 5). <c>BuildTheArmedSend</c> puts
    /// the port inside the sequence; <c>UseRigPortForTests</c> is what
    /// <c>StopSending</c> reads - <c>_armedSend?.StopNow(_rigPort)</c> - and it is
    /// assigned nowhere else outside the real connect. **Without the second call the
    /// Stop button fires <c>StopNow(null)</c>, no abort frame is written, and
    /// criterion 3's carrier half measures nothing while looking green.**</para>
    /// <para>**THE SINK IS A REAL <c>WasapiTransmitSink</c>, BUILT THE WAY THE
    /// APPLICATION BUILDS IT** - from the endpoint named in Settings, through
    /// <c>TransmitSinkFactory</c>. This is the second test in this project that lets
    /// one be constructed.</para>
    /// </remarks>
    private Built Scene(RenderEndpoint endpoint)
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN00";
        settings.Operator.LicenseClass = LicenseClass.General;
        settings.AudioOutputDeviceId = endpoint.Id;

        // **THE APPLICATION'S OWN WRITER, WITH ITS OWN ENABLED-CATEGORY PREDICATE**,
        // built the way `App.axaml.cs` builds it, so what lands on disk lands
        // through the operator's real switches and not a permissive stand-in.
        var telemetry = new JsonlTelemetry(
            _folder,
            "1.12.122",
            category => settings.IsTelemetryEnabled(category),
            settings.TelemetryMaxMegabytes * 1024L * 1024L);

        var panel = new MainWindowViewModel(settings, telemetry)
        {
            // The digital workspace is collapsed unless this tab is showing, and
            // nothing under it is realized while it is - so a right-click would
            // find no row at all (`TheMenuIsUnderTheMouseTests` paid for this).
            OperatingMode = "Digital",
            DigitalDecodedExpanded = true,
        };

        var window = new MainWindow { DataContext = panel };

        window.Show();

        // **THE ROWS GO IN AFTER `Show`.** Showing raises `Opened`, which starts the
        // reconnect, which clears the decoded table.
        Pump(window);

        // 20 m FIRST, THEN THE FREQUENCY - `OnFrequencyHzChanged` clamps to the
        // selected band's map window and clears the table on a retune.
        panel.SelectedBand = panel.Bands.First(
            b => b.Band.LowHz <= Ft8On20m && b.Band.HighHz >= Ft8On20m);
        panel.FrequencyHz = Ft8On20m;

        var port = new FakePort();
        WasapiTransmitSink? sink = null;

        panel.TransmitSinkFactory = name =>
        {
            sink = new WasapiTransmitSink(name);

            return sink;
        };

        panel.UseRigPortForTests(port);
        panel.BuildTheArmedSend(port);

        Assert.True(panel.HasSomethingToTransmitThrough, panel.DigitalSendLine);
        Assert.NotNull(sink);

        Pump(window);

        return new Built(window, panel, settings, port, sink!, telemetry);
    }

    /// <summary>What the run says on a machine with no card, and why that is fine.</summary>
    private void NoEndpoint(string why)
    {
        _output.WriteLine("NOT ATTEMPTED: " + why + " (SHACK_FACTS.md FACT-004)");
        _output.WriteLine(
            "That is a fact about this machine and not a failure. The wiring is proved "
            + "against the faithful fake by " + nameof(TheSendPathReachesARealRadioTests)
            + " and the menu by TheMenuIsUnderTheMouseTests; what cannot be proved "
            + "without a card is that a sound was made.");
    }

    // -------------------------------------------------------------------------
    // The card, the capture, and what silence is.
    // -------------------------------------------------------------------------

    /// <summary>A loopback capture on one endpoint, into the engine's own tap.</summary>
    /// <remarks>
    /// **THE ENGINE'S OWN DOWNMIX, NOT A SECOND ONE** - the same conversion the
    /// receive path uses on every buffer the radio delivers, and the same
    /// <see cref="AudioTap"/>, whose write side is built for a device callback.
    /// Lifted from <c>TheLoopbackThroughTheApplicationsSendPathTests.cs:114-143</c>
    /// so that two runs in one test can share it.
    /// </remarks>
    private sealed class Capture : IDisposable
    {
        private readonly MMDeviceEnumerator _enumerator = new();
        private readonly MMDevice _device;
        private readonly NAudio.Wave.WasapiLoopbackCapture _capture;
        private readonly ManualResetEventSlim _stopped = new(false);
        private readonly AudioTap _tap = new();
        private float[] _mono = [];

        /// <summary>Opens the capture without starting it.</summary>
        /// <param name="endpoint">The render endpoint to listen to.</param>
        public Capture(RenderEndpoint endpoint)
        {
            _device = _enumerator.GetDevice(endpoint.Id);
            _capture = new NAudio.Wave.WasapiLoopbackCapture(_device);

            _capture.DataAvailable += (_, e) =>
            {
                if (e.BytesRecorded <= 0)
                {
                    // An idle endpoint delivers empty packets; they are not silence
                    // to be recorded, they are nothing arriving.
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

        /// <summary>What the endpoint is delivering at.</summary>
        public int SampleRate => _capture.WaveFormat.SampleRate;

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
    /// <param name="Peak">The loudest absolute sample, as a fraction of full scale.</param>
    /// <param name="PeakDb">The same, in decibels below full scale.</param>
    /// <param name="RmsDb">The average over the same stretch, in dBFS.</param>
    private readonly record struct CapturedLevel(double Peak, double PeakDb, double RmsDb);

    /// <summary>
    /// **What says a card made a sound, and what says it did not.**
    /// </summary>
    /// <remarks>
    /// <para>**A SAMPLE COUNT CANNOT SAY THIS** (`docs/unit268-the-chain-trace.md`
    /// question 6). WASAPI loopback delivers buffers of zeroes on a silent endpoint,
    /// so <c>SamplesSeen</c> counts silence as eagerly as tones - which is exactly
    /// the untouched counter step C's criterion 4 forbids.</para>
    /// <para>**THE THRESHOLD IS THE TREE'S OWN AND NOT AN INVENTED ONE.**
    /// <see cref="AudioLevel.TooQuietDb"/> is -60 dBFS, and
    /// <see cref="AudioLevel.NearlySilent"/> is already written against it: "a signal
    /// that quiet is not a decoder problem". The floor printed for a peak of exactly
    /// zero is <see cref="AudioLevel.SilenceDb"/> rather than an infinity.</para>
    /// </remarks>
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
        => value <= 0 ? AudioLevel.SilenceDb : Math.Max(AudioLevel.SilenceDb, 20 * Math.Log10(value));

    /// <summary>A decibel figure as a reader sees it.</summary>
    private static string Db(double db)
        => db <= AudioLevel.SilenceDb
            ? "<= " + AudioLevel.SilenceDb.ToString("F1", CultureInfo.InvariantCulture)
            : db.ToString("F1", CultureInfo.InvariantCulture);

    /// <summary>
    /// The endpoint this test will make sound on, and why it was chosen.
    /// </summary>
    /// <remarks>
    /// **CHOSEN, NOT DEFAULTED TO**, on unit 256's reasoning and carried verbatim in
    /// shape from <c>TheLoopbackThroughTheApplicationsSendPathTests.Preferred</c>:
    /// this plays FT8 tones out of a computer somebody owns and may be asleep near,
    /// so the endpoint is picked deliberately and named in the output.
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

    /// <summary>Runs the dispatcher and lays the window out.</summary>
    private static void Pump(Window window)
    {
        for (var i = 0; i < 5; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }
}
