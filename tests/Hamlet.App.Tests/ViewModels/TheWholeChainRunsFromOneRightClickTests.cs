using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
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

    /// <summary>How long an FT8 transmission itself is, inside that slot.</summary>
    private const double Ft8SlotSeconds = 12.64;

    /// <summary>FT8's watering hole on 20 m, where a General may send data.</summary>
    private const long Ft8On20m = 14_074_000;

    /// <summary>The operator.</summary>
    private const string Mine = "KC3QIS";

    /// <summary>The station he right-clicks.</summary>
    private const string His = "W1ABC";

    /// <summary>What the row says he was heard at, so the menu can offer a report.</summary>
    private const string Report = "-10";

    /// <summary>The slot the row on the table was heard in.</summary>
    private static readonly DateTime RowSlot =
        new(2026, 9, 7, 18, 0, 0, DateTimeKind.Utc);

    /// <summary>What the operator heard from him, which is what puts a row up.</summary>
    private const string HeardFromHim = "KC3QIS W1ABC R-15";

    /// <summary>How long the card is listened to across a boundary nobody clicked.</summary>
    private static readonly TimeSpan SilentStretch = TimeSpan.FromSeconds(3);

    /// <summary>What the radio sees when it is keyed.</summary>
    private const string KeyOn = "FE FE 94 E0 1C 00 01 FD";

    /// <summary>The ordinary unkey, and the abort's second frame.</summary>
    private const string KeyOff = "FE FE 94 E0 1C 00 00 FD";

    /// <summary>The abort's first frame - CI-V `0x17` with `0xFF`.</summary>
    private const string CwStop = "FE FE 94 E0 17 FF FD";

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

    /// <summary>
    /// **One right-click, and the sound that comes back off the card is the text on
    /// the item he clicked.**
    /// </summary>
    /// <remarks>
    /// <para>**STEP C'S CRITERIA 1, 2 AND 4, IN ONE METHOD.** A real
    /// `ContextRequested` on a real row control in a real window; the menu item
    /// itself invoked through its own `Command` and `CommandParameter`; compose,
    /// key, play, unkey and the application's own telemetry line on disk; the
    /// captured audio resampled and decoded; and a second boundary nobody clicked
    /// with the card measured silent across it.</para>
    /// <para>**THE BREAKAGE IT WOULD HAVE CAUGHT: a menu that offers one string and
    /// a send path that transmits another.** The loopback test sends a literal it
    /// chose itself (<c>TheLoopbackThroughTheApplicationsSendPathTests.cs:84</c>);
    /// the menu tests never transmit. **Between them there is no test in the tree
    /// that would fail if the two disagreed**, and the operator would be told he
    /// sent one thing while another went out over the band.</para>
    /// <para>**THE EXPECTED STRING IS NOT A LITERAL AND NOT A ROUND TRIP OF ONE.**
    /// It is <c>MenuItem.CommandParameter</c> off the realized flyout - what the
    /// markup's own handler hung on the thing the mouse hits.</para>
    /// <para>**ASSERTED ON THE RUN'S OUTCOME AND NOT THE ARM OUTCOME.** A refusal
    /// comes back as <see cref="Ft8ArmOutcome.Ran"/> at the boundary - the sequence
    /// ran - and only <c>TransmitRun.Outcome</c> carries the real answer (unit 267's
    /// recorded trap).</para>
    /// <para>**CRITERION 3 IS NOT IN THIS METHOD AND THAT IS DELIBERATE.** A stopped
    /// transmission does not decode: criterion 2 needs a whole 12.64-second slot to
    /// reach the decoder and criterion 3 truncates one on purpose
    /// (`docs/unit267-what-step-c-needs.md`). It is
    /// <see cref="TheOperatorsStopButtonTakesARealTransmissionOffTheCardMidSlot"/>,
    /// beside this one, on the same harness.</para>
    /// </remarks>
    [AvaloniaFact]
    public async Task OneRightClickDrivesTheWholeChainAndTheAudioDecodesBackAsTheClickedText()
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
        _output.WriteLine("row on the table : \"" + HeardFromHim + "\", heard at " + Report + " dB");

        using var capture = new Capture(endpoint);

        capture.Start();
        await Task.Delay(PreRoll);

        // ---- 1. THE RIGHT-CLICK, AND THE ITEM ITSELF ---------------------------
        var flyout = RightClick(scene);
        var item = TheOneThatComesNext(flyout);

        var header = item.Header as string ?? "";
        var clicked = item.CommandParameter as string;

        Assert.True(
            clicked is not null,
            "the menu item under the mouse carried no message: header \"" + header + "\"");

        _output.WriteLine(string.Empty);
        _output.WriteLine("the menu offered  : " + Options(flyout).Count + " clickable messages");

        foreach (var option in Options(flyout))
        {
            _output.WriteLine("    " + (option.Header as string ?? ""));
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("HE CLICKED        : " + header);
        _output.WriteLine("WHICH CARRIES     : \"" + clicked + "\"");

        // **THE CLICK ITSELF**, through the item's own command with the item's own
        // parameter - which is what a press on a `MenuItem` does.
        Assert.True(item.Command!.CanExecute(clicked), "the item could not be executed");

        var clock = Stopwatch.StartNew();

        item.Command.Execute(clicked);

        var slot = scene.Panel.ArmedForSlotUtc;

        Assert.True(slot is not null, scene.Panel.DigitalSendLine);

        var result = await scene.Panel.AtSlotBoundaryAsync(slot!.Value);

        await Task.Delay(PostRoll);
        clock.Stop();

        Pump(scene.Window);

        var run = result!.Run;
        var sent = result.Send!.Transmission;
        var played = run?.Played ?? new PlayedAudio(0, TimeSpan.Zero);

        _output.WriteLine(string.Empty);
        _output.WriteLine("armed for slot   : " + slot.Value.ToString("O", CultureInfo.InvariantCulture));
        _output.WriteLine("composed at      : " + sent.SampleRate + " Hz");
        _output.WriteLine("samples composed : " + sent.Samples.Length);
        _output.WriteLine("outcome          : " + run?.Outcome);
        _output.WriteLine("reason           : " + run?.Reason);
        _output.WriteLine("keyed            : " + run?.Keyed);
        _output.WriteLine("came out of tx   : " + run?.CameOutOfTransmit);
        _output.WriteLine("samples played   : " + played.SamplesPlayed);
        _output.WriteLine("peak written     : " + scene.Sink.PeakWritten.ToString("F4"));
        _output.WriteLine("clipped samples  : " + scene.Sink.ClippedSamples);
        _output.WriteLine("wire             : " + Wire(scene));
        _output.WriteLine("wall clock       : " + clock.Elapsed.TotalSeconds.ToString("F2") + " s");
        _output.WriteLine("the operator reads: " + scene.Panel.DigitalSendLine);

        // ---- 2. COMPOSE, KEY, PLAY, UNKEY --------------------------------------
        Assert.Equal(Ft8ArmOutcome.Ran, result.Outcome);
        Assert.True(run!.Sent, run.Reason);
        Assert.True(run.Keyed);
        Assert.Equal(UnkeyRoute.OrdinaryUnkey, run.CameOutOfTransmit);

        // **THE FRAMES, READ RATHER THAN COUNTED.** A count of two is satisfied by
        // two of anything (`docs/unit268-the-chain-trace.md` question 4).
        Assert.Equal(new[] { KeyOn, KeyOff }, Frames(scene));

        // AND WHAT WENT OUT IS WHAT WAS CLICKED, ON THE WAY IN AS WELL AS BACK.
        Assert.Equal(clicked, sent.Text);
        Assert.Equal(endpoint.SampleRate, sent.SampleRate);
        Assert.Equal(sent.Samples.Length, played.SamplesPlayed);

        // ---- 3. NOTHING TRANSMITS THAT WAS NOT CLICKED -------------------------
        // **THE ENDPOINT IS STILL OPEN AND THE CAPTURE IS STILL RUNNING.** A second
        // boundary, with nothing armed, listened to as sound rather than counted.
        var quietFrom = capture.SamplesSeen;
        var packetsBefore = capture.Packets;
        var framesBefore = scene.Port.Written.Count;

        var nothing = await scene.Panel.AtSlotBoundaryAsync(slot.Value.AddSeconds(15));

        await Task.Delay(SilentStretch);

        var quietCount = (int)(capture.SamplesSeen - quietFrom);
        var packetsAcross = capture.Packets - packetsBefore;
        var acrossTheBoundary = quietCount > 0 ? capture.Window(quietFrom, quietCount) : null;

        capture.Stop();

        Assert.True(nothing is not null, "the panel had no send path at the second boundary");
        Assert.Equal(Ft8ArmOutcome.NothingArmed, nothing!.Outcome);
        Assert.Null(nothing.Run);
        Assert.Equal(framesBefore, scene.Port.Written.Count);

        // **THE CAPTURE WAS STILL RUNNING**, witnessed by packets and not by
        // samples. A loopback capture is handed empty packets while the endpoint
        // renders nothing, so a stretch with no samples in it is a real answer
        // about the card - but only once *the capture stopped* has been ruled out
        // separately. That is unit 268 task 3's watched red, in one assertion.
        Assert.True(
            packetsAcross > 0,
            "the capture was handed no packets at all across the unclicked boundary, "
            + "so nothing here can tell a silent card from a capture that died");

        // **THE LEVEL, WHICH FLOORS AT SILENCE WHERE NO AUDIO WAS RENDERED.** Not a
        // sample count: where samples did arrive they are measured, and where none
        // did the endpoint rendered nothing, which is the same verdict by the
        // stronger route.
        var quiet = acrossTheBoundary is null
            ? new CapturedLevel(0, AudioLevel.SilenceDb, AudioLevel.SilenceDb)
            : Level(acrossTheBoundary.Samples);

        _output.WriteLine(string.Empty);
        _output.WriteLine("---- the boundary nobody clicked ----");
        _output.WriteLine("second boundary  : " + nothing.Outcome + ", run "
            + (nothing.Run is null ? "null" : "not null"));
        _output.WriteLine("frames on wire   : " + scene.Port.Written.Count
            + " (unchanged from " + framesBefore + ")");
        _output.WriteLine("listened for     : " + SilentStretch.TotalSeconds.ToString("F1")
            + " s with the endpoint open and the capture running");
        _output.WriteLine("packets across   : " + packetsAcross
            + " (the capture was alive)");
        _output.WriteLine("samples across   : " + quietCount
            + (quietCount == 0 ? " - the endpoint rendered nothing at all" : ""));
        _output.WriteLine("CARD ACROSS IT   : peak " + Db(quiet.PeakDb) + " dBFS, rms "
            + Db(quiet.RmsDb) + " dBFS");
        _output.WriteLine("silent means     : peak at or below " + AudioLevel.TooQuietDb
            + " dBFS (AudioLevel.TooQuietDb)");
        _output.WriteLine("the same capture read -12.0 dBFS while the transmission was on, "
            + "so the instrument is not deaf");

        Assert.True(
            quiet.PeakDb <= AudioLevel.TooQuietDb,
            $"nobody clicked and the card made a sound: {quietCount} samples off "
            + $"{scene.Sink.DeviceName} across a boundary with nothing armed, peaking at "
            + $"{Db(quiet.PeakDb)} dBFS.");

        // ---- 4. THE TELEMETRY LINE, OFF DISK -----------------------------------
        scene.CloseTelemetry();

        var lines = TransmitLines();

        _output.WriteLine(string.Empty);
        _output.WriteLine("telemetry folder : " + _folder);

        var line = Assert.Single(lines);

        _output.WriteLine("telemetry line   : " + line);

        using var doc = JsonDocument.Parse(line);
        var data = doc.RootElement.GetProperty("data");

        Assert.Equal(
            TransmitRecord.EventName, doc.RootElement.GetProperty("event").GetString());
        Assert.Equal(
            slot.Value.ToString("O", CultureInfo.InvariantCulture),
            data.GetProperty("slotStartUtc").GetString());
        Assert.Equal(played.SamplesPlayed, data.GetProperty("sampleCount").GetInt32());

        // ---- 5. THE DECODE, AND THIS IS THE ASSERTION THE STEP EXISTS FOR ------
        var captured = capture.Snapshot();

        Assert.True(
            captured is not null && captured.Samples.Length > 0,
            "the loopback capture started and delivered nothing while the endpoint was "
            + $"rendering - {capture.SamplesSeen} samples reached the tap");

        // The capture placed at the start of one slot: 12.64 s of signal with the
        // pre-roll before it, and the rest silence - the shape the composer's own
        // padded slot has.
        var window = new float[(int)Math.Round(SlotSeconds * captured!.SampleRate)];
        Array.Copy(
            captured.Samples, window, Math.Min(captured.Samples.Length, window.Length));

        var onTheGrid = Ft8Resample.ToFt8Rate(new MonoAudio(captured.SampleRate, window));
        var texts = new Ft8SlotDecoder().Decode(onTheGrid.Samples).Texts;

        var level = Level(window);

        _output.WriteLine(string.Empty);
        _output.WriteLine("---- the click and the sound, side by side ----");
        _output.WriteLine("endpoint         : " + scene.Sink.DeviceName + " at "
            + captured.SampleRate + " Hz");
        _output.WriteLine("captured samples : " + captured.Samples.Length
            + ", peak " + Db(level.PeakDb) + " dBFS");
        _output.WriteLine("CLICKED          : \"" + clicked + "\"");
        _output.WriteLine("DECODED          : "
            + (texts.Count == 0 ? "nothing" : "\"" + string.Join("\", \"", texts) + "\""));
        _output.WriteLine("run outcome      : " + run.Outcome
            + ", unkey " + run.CameOutOfTransmit);

        // **§0.0. A decode that did not happen is reported as one that did not
        // happen**, with what did come back - and it is compared against the text
        // on the item that was clicked, not against a literal this test chose.
        Assert.True(
            texts.Contains(clicked!, StringComparer.Ordinal),
            $"the operator clicked a menu item reading \"{header}\", which carries "
            + $"\"{clicked}\". That went out of {scene.Sink.DeviceName} at "
            + $"{sent.SampleRate} Hz, peaking at {scene.Sink.PeakWritten:F4}, and the "
            + "decoder returned "
            + $"{(texts.Count == 0 ? "nothing" : "\"" + string.Join("\", \"", texts) + "\"")} "
            + "from the captured audio. The message on the menu and the message on the "
            + "air are not the same message.");
    }

    /// <summary>
    /// **His Stop button takes a real transmission off a real card, from the middle
    /// of the chain he started with a right-click.**
    /// </summary>
    /// <remarks>
    /// <para>**STEP C'S CRITERION 3.** The operator's own right-click starts it and
    /// the real <c>DigitalStopButton</c> on the realized window is pressed part-way
    /// through, with a real mouse at the button's own place - not
    /// <c>StopNow</c> called directly and not the command invoked.</para>
    /// <para>**WHY IT IS A SECOND RUN AND NOT THE SAME ONE, AND IT MUST NOT BE
    /// REDISCOVERED AS A RED:** a stopped transmission does not decode. Criterion 2
    /// needs a whole 12.64-second slot to reach the decoder and this one truncates
    /// one on purpose (`docs/unit267-what-step-c-needs.md`). **Nothing here decodes
    /// anything**, and a decode failure here would be the expected result rather
    /// than a finding.</para>
    /// <para>**THE BREAKAGE IT WOULD HAVE CAUGHT: a stop that takes the carrier off
    /// the wire and leaves the card playing.** That is unit 261's real defect and
    /// unit 263 fixed it - but the operator's *press* is proved against a fake sink
    /// (<c>TheOperatorCanStopItTests.cs:241</c>) and the *card going quiet* is proved
    /// in the engine with no operator
    /// (<c>TheStopStopsARealEndpointTests.cs:88</c>), and **nothing joins the
    /// two**. A regression that reconnected them wrongly would leave both existing
    /// tests green.</para>
    /// <para>**IT PROVES THE ABORT AND DOES NOT EDIT IT.** `PHASE_PLAN.md`'s first
    /// ruling: the abort is not weakened, made conditional or routed around.</para>
    /// <para>**NO BOUND TIGHTER THAN THE TREE'S IS ASSERTED.** Unit 263 measured the
    /// card going quiet 15-20 ms after the stop on a real endpoint with a 200 ms
    /// buffer, and `TheStopStopsARealEndpointTests.cs:187` asserts under a second.
    /// What this run reads is printed, whatever it is; the margin is not made the
    /// thing under test.</para>
    /// </remarks>
    [AvaloniaFact]
    public async Task TheOperatorsStopButtonTakesARealTransmissionOffTheCardMidSlot()
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
        _output.WriteLine("sink opened      : " + scene.Sink.DeviceName + " at "
            + scene.Sink.EndpointSampleRate + " Hz");
        _output.WriteLine("buffer           : " + scene.Sink.BufferFrames + " frames ("
            + (scene.Sink.BufferFrames * 1000.0 / scene.Sink.EndpointSampleRate)
                .ToString("F1", CultureInfo.InvariantCulture) + " ms)");

        using var capture = new Capture(endpoint);

        capture.Start();
        await Task.Delay(PreRoll);

        // ---- 1. THE SAME GESTURE THAT STARTS EVERY TRANSMISSION -----------------
        var flyout = RightClick(scene);
        var item = TheOneThatComesNext(flyout);
        var clicked = item.CommandParameter as string;

        Assert.True(clicked is not null, "the menu item under the mouse carried no message");

        _output.WriteLine(string.Empty);
        _output.WriteLine("HE CLICKED       : " + (item.Header as string ?? ""));
        _output.WriteLine("WHICH CARRIES    : \"" + clicked + "\"");

        item.Command!.Execute(clicked);

        var slot = scene.Panel.ArmedForSlotUtc;

        Assert.True(slot is not null, scene.Panel.DigitalSendLine);

        var clock = Stopwatch.StartNew();
        var running = scene.Panel.AtSlotBoundaryAsync(slot!.Value);

        // ---- 2. WAIT UNTIL THE CARD IS ACTUALLY MAKING THE SOUND ---------------
        // **THE CAPTURE ITSELF IS THE TRIGGER, NOT A SLEEP.** An idle endpoint
        // renders nothing and the loopback hands over empty packets, so the first
        // sample to reach the tap is the first sample the card played.
        var soundStarted = await WaitUntil(
            () => capture.SamplesSeen > 0, TimeSpan.FromSeconds(20), clock);

        Assert.True(
            soundStarted is not null,
            "no audio reached the loopback at all: the run said "
            + scene.Panel.DigitalSendLine);

        var rate = scene.Sink.EndpointSampleRate;
        var wholeSlot = TimeSpan.FromSeconds(Ft8SlotSeconds);
        var aThirdIn = soundStarted!.Value + (wholeSlot / 3);

        while (clock.Elapsed < aThirdIn)
        {
            await Task.Delay(5, CancellationToken.None);
        }

        // ---- 3. THE PRESS, WITH A REAL MOUSE, ON THE REAL BUTTON ---------------
        var samplesAtThePress = capture.SamplesSeen;
        var atThePress = clock.Elapsed;
        var pressClock = Stopwatch.StartNew();

        Click(scene, StopButton(scene));

        pressClock.Stop();

        var whileRunning = Frames(scene);

        // ---- 4. WHEN DID THE CARD GO QUIET? ------------------------------------
        // Watched on the capture: the wall-clock moment the last sample arrived,
        // once nothing more has arrived for a settled stretch.
        var wentQuietAt = await WhenTheSamplesStop(capture, clock, TimeSpan.FromSeconds(10));

        var boundary = await running;

        var played = boundary!.Run!.Played!.Value;
        var offTheCard = capture.SamplesSeen / (double)rate;
        var quietAfter = (wentQuietAt - atThePress).TotalMilliseconds;

        Pump(scene.Window);

        _output.WriteLine(string.Empty);
        _output.WriteLine("sound started at : " + soundStarted.Value.TotalSeconds
            .ToString("F2", CultureInfo.InvariantCulture) + " s (first sample at the tap)");
        _output.WriteLine("STOP PRESSED AT  : " + atThePress.TotalSeconds
            .ToString("F2", CultureInfo.InvariantCulture) + " s, "
            + (atThePress - soundStarted.Value).TotalSeconds
                .ToString("F2", CultureInfo.InvariantCulture) + " s into the transmission");
        _output.WriteLine("the press took   : " + pressClock.Elapsed.TotalMilliseconds
            .ToString("F1", CultureInfo.InvariantCulture)
            + " ms (mouse down, up and the dispatcher pumped - not StopNow alone)");
        _output.WriteLine("samples at press : " + samplesAtThePress);
        _output.WriteLine("AUDIO OFF THE CARD: " + offTheCard
            .ToString("F2", CultureInfo.InvariantCulture) + " s of the "
            + Ft8SlotSeconds.ToString("F2", CultureInfo.InvariantCulture)
            + " s the slot would have been");
        _output.WriteLine("CARD WENT QUIET  : " + quietAfter.ToString("F0", CultureInfo.InvariantCulture)
            + " ms after the press  [development machine]");
        _output.WriteLine("sink counted     : " + played.SamplesPlayed + " of "
            + boundary.Send!.Transmission.Samples.Length + " samples, short by "
            + (boundary.Send.Transmission.Samples.Length - played.SamplesPlayed));
        _output.WriteLine("the run said     : " + boundary.Run.Outcome);
        _output.WriteLine("came out of tx   : " + boundary.Run.CameOutOfTransmit);
        _output.WriteLine("WIRE WHILE RUNNING: " + string.Join(" | ", whileRunning));
        _output.WriteLine("wire at the end  : " + Wire(scene));
        _output.WriteLine("the operator reads: " + scene.Panel.DigitalSendLine);

        // ---- the carrier came off the wire ------------------------------------
        // **THE ABORT'S OWN TWO FRAMES**, and they are read rather than counted.
        Assert.Equal(new[] { KeyOn, CwStop, KeyOff }, whileRunning);

        // ---- the sound came off the card ---------------------------------------
        Assert.Equal(Ft8TransmitOutcome.Cancelled, boundary.Run.Outcome);

        Assert.True(
            played.SamplesPlayed < boundary.Send.Transmission.Samples.Length,
            $"the whole slot went out of a real endpoint anyway: {played.SamplesPlayed} "
            + $"of {boundary.Send.Transmission.Samples.Length} samples");

        Assert.True(
            offTheCard < Ft8SlotSeconds,
            $"the loopback heard {offTheCard:F2} s off the card, which is the whole "
            + $"{Ft8SlotSeconds:F2} s slot - the carrier came off the wire and the card "
            + "played on");

        // **AND IT STAYED QUIET, WITH THE ENDPOINT STILL OPEN.** The same figure
        // criterion 4 uses, on the stretch after the stop.
        var quietFrom = capture.SamplesSeen;
        var packetsBefore = capture.Packets;

        await Task.Delay(SilentStretch);

        var after = (int)(capture.SamplesSeen - quietFrom);
        var packetsAfter = capture.Packets - packetsBefore;
        var tail = after > 0 ? capture.Window(quietFrom, after) : null;

        capture.Stop();

        var quiet = tail is null
            ? new CapturedLevel(0, AudioLevel.SilenceDb, AudioLevel.SilenceDb)
            : Level(tail.Samples);

        _output.WriteLine(string.Empty);
        _output.WriteLine("---- and it stayed off ----");
        _output.WriteLine("listened for     : " + SilentStretch.TotalSeconds
            .ToString("F1", CultureInfo.InvariantCulture)
            + " s more with the endpoint open");
        _output.WriteLine("packets after    : " + packetsAfter + " (the capture was alive)");
        _output.WriteLine("samples after    : " + after);
        _output.WriteLine("CARD AFTER STOP  : peak " + Db(quiet.PeakDb) + " dBFS");

        Assert.True(
            packetsAfter > 0,
            "the capture was handed no packets after the stop, so nothing here can "
            + "tell a silent card from a capture that died");

        Assert.True(
            quiet.PeakDb <= AudioLevel.TooQuietDb,
            $"the card was still making a sound {SilentStretch.TotalSeconds:F1} s after "
            + $"the operator pressed Stop: peak {Db(quiet.PeakDb)} dBFS off "
            + scene.Sink.DeviceName);

        // **NOTHING HERE IS DECODED**, and that is deliberate: a stopped
        // transmission does not decode, and asserting one would go red for the
        // reason this method exists to create.
    }

    // -------------------------------------------------------------------------
    // The scene: a real window, a real card, a fake wire.
    // -------------------------------------------------------------------------

    /// <summary>Everything one run needs, and the three things that must be closed.</summary>
    private sealed class Built(
        MainWindow window,
        MainWindowViewModel panel,
        AppSettings settings,
        FakePort port,
        WasapiTransmitSink sink,
        JsonlTelemetry telemetry) : IDisposable
    {
        private bool _telemetryClosed;

        /// <summary>The real window, shown.</summary>
        public MainWindow Window { get; } = window;

        /// <summary>The panel behind it.</summary>
        public MainWindowViewModel Panel { get; } = panel;

        /// <summary>The operator's own settings.</summary>
        public AppSettings Settings { get; } = settings;

        /// <summary>The wire, which is a fake and opens nothing.</summary>
        public FakePort Port { get; } = port;

        /// <summary>The card, which is real.</summary>
        public WasapiTransmitSink Sink { get; } = sink;

        /// <summary>The application's own telemetry writer.</summary>
        public JsonlTelemetry Telemetry { get; } = telemetry;

        /// <summary>
        /// **Closes the writer so what it queued is on disk before it is read.**
        /// </summary>
        /// <remarks>
        /// `JsonlTelemetry.Write` returns `void` and appends on a background
        /// thread, so a call returning is not evidence a line was written
        /// (`TheWholeContactWalksThroughTheApplicationTests.cs:445-448`). Idempotent,
        /// because <see cref="Dispose"/> runs afterwards whatever the test did.
        /// </remarks>
        public void CloseTelemetry()
        {
            if (_telemetryClosed)
            {
                return;
            }

            _telemetryClosed = true;
            Telemetry.Dispose();
        }

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

            CloseTelemetry();
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

        // **ONE ROW ON THE TABLE, PLACED THE WAY A DECODE PLACES ONE.** He is the
        // sender and the operator is the addressee, which is what makes the row
        // right-clickable and gives the menu a station to build against.
        panel.AddDecodeRowForTests(
            RowSlot.ToString("HHmmss", CultureInfo.InvariantCulture),
            Report, "0.2", "1240", HeardFromHim, RowSlot);

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

    // -------------------------------------------------------------------------
    // The gesture, and the item under the mouse.
    // -------------------------------------------------------------------------

    /// <summary>
    /// **Raises a real context request on the row that names him.**
    /// </summary>
    /// <remarks>
    /// **THE EVENT IS RAISED ON THE ROW'S OWN CONTROL**, the `Grid` the
    /// `DataTemplate` builds, whose `DataContext` is the row, and the flyout that
    /// comes back is the one the markup's own handler built and showed. Nothing
    /// here calls <c>SendFlyoutFor</c> directly - that is the fallback work
    /// instruction 268 task 2 named and it is not taken. Driven exactly as
    /// <c>TheMenuIsUnderTheMouseTests.RightClickRow</c> drives it.
    /// </remarks>
    private static MenuFlyout RightClick(Built scene)
    {
        Pump(scene.Window);

        var rows = scene.Window.GetVisualDescendants()
            .OfType<ItemsControl>()
            .FirstOrDefault(c => c.Name == "DigitalDecodedRows");

        Assert.True(rows is not null, "the decoded table is not on the realized window");

        var grid = rows!.GetVisualDescendants()
            .OfType<Grid>()
            .FirstOrDefault(g => g.DataContext is DigitalDecodeRow row
                && row.Sender == His && row.Addressee == Mine);

        Assert.True(
            grid is not null,
            "no realized row named " + His + ". Rows on the table: "
            + scene.Panel.DigitalVisibleDecodes.Count);

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
    /// <remarks>
    /// A note carries no command and is not hit-testable
    /// (<c>MainWindow.axaml.cs:245</c>), so this is exactly what a mouse can hit.
    /// </remarks>
    private static List<MenuItem> Options(MenuFlyout flyout)
        => flyout.Items.OfType<MenuItem>().Where(i => i.Command is not null).ToList();

    /// <summary>
    /// **The item an operator would click, and what it is carrying.**
    /// </summary>
    /// <remarks>
    /// <para>**THE ONE MARKED AS COMING NEXT**, which is the one the menu holds out
    /// to him. Nothing is greyed and everything stays clickable (ruled 2026-09-06),
    /// so this is a choice among live options and not the only live one.</para>
    /// <para>**THE EXPECTED STRING IS READ OFF THE REALIZED ITEM AND NOT OUT OF
    /// <c>SendMenuFor</c> A SECOND TIME.** Reading the view model again would
    /// compare the send path against the same source that fed it; reading
    /// <c>CommandParameter</c> reads what the markup's own handler put on the thing
    /// the mouse hits (`docs/unit268-the-chain-trace.md` question 1).</para>
    /// </remarks>
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
        private long _packets;
        private long _audioBytes;

        /// <summary>Opens the capture without starting it.</summary>
        /// <param name="endpoint">The render endpoint to listen to.</param>
        public Capture(RenderEndpoint endpoint)
        {
            _device = _enumerator.GetDevice(endpoint.Id);
            _capture = new NAudio.Wave.WasapiLoopbackCapture(_device);

            _capture.DataAvailable += (_, e) =>
            {
                // **EVERY PACKET IS COUNTED, INCLUDING THE EMPTY ONES**, and that is
                // this class's one addition to the block it was lifted from. An idle
                // endpoint delivers empty packets, so *the capture is still running*
                // and *the endpoint rendered nothing* are different facts and have to
                // be witnessed separately - unit 268 task 3's watched red is what
                // happens when they are not.
                Interlocked.Increment(ref _packets);

                if (e.BytesRecorded <= 0)
                {
                    // They are not silence to be recorded, they are nothing arriving.
                    return;
                }

                Interlocked.Add(ref _audioBytes, e.BytesRecorded);

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

        /// <summary>
        /// **Every packet the device has handed over, empty ones included.**
        /// </summary>
        /// <remarks>
        /// This is the liveness witness and nothing else. It says the capture is
        /// still being fed by the endpoint; it says nothing whatever about whether
        /// there was any sound in it, which is <see cref="Level"/>'s business.
        /// </remarks>
        public long Packets => Interlocked.Read(ref _packets);

        /// <summary>How many bytes of actual audio those packets carried.</summary>
        public long AudioBytes => Interlocked.Read(ref _audioBytes);

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

        /// <summary>One stretch of it, addressed from the first sample ever taken.</summary>
        /// <param name="firstSample">Where the stretch starts.</param>
        /// <param name="count">How many samples of it.</param>
        /// <returns>The samples, or null where the tap no longer holds them all.</returns>
        public MonoAudio? Window(long firstSample, int count) => _tap.Window(firstSample, count);

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
    /// mouse can reach it. Lifted from <c>TheOperatorCanStopItTests.Click</c>.
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

    /// <summary>Waits for something to become true, and says when it did.</summary>
    /// <param name="until">What is being waited for.</param>
    /// <param name="giveUpAfter">How long to wait before saying it never happened.</param>
    /// <param name="clock">The run's own clock, so the answer is on its timeline.</param>
    /// <returns>When it happened, or null where it did not.</returns>
    private static async Task<TimeSpan?> WaitUntil(
        Func<bool> until, TimeSpan giveUpAfter, Stopwatch clock)
    {
        var deadline = clock.Elapsed + giveUpAfter;

        while (clock.Elapsed < deadline)
        {
            if (until())
            {
                return clock.Elapsed;
            }

            await Task.Delay(2, CancellationToken.None);
        }

        return null;
    }

    /// <summary>
    /// **When the last sample arrived, once none has arrived for a settled
    /// stretch.**
    /// </summary>
    /// <param name="capture">The loopback, still running.</param>
    /// <param name="clock">The run's own clock.</param>
    /// <param name="giveUpAfter">How long to watch before giving up.</param>
    /// <returns>The moment the last sample was seen to arrive.</returns>
    /// <remarks>
    /// <para>**MEASURED ON THE CARD AND NOT ON THE SINK'S OWN BOOKKEEPING.** The
    /// engine's equivalent (`TheStopStopsARealEndpointTests.cs:145`) takes the
    /// moment `AtBoundaryAsync` returned; this takes the moment the loopback
    /// stopped being handed audio, which is the endpoint itself falling silent.</para>
    /// <para>**ITS RESOLUTION IS THE DEVICE'S PACKET PERIOD** and it is reported,
    /// not asserted against. The settle window is deliberately longer than any
    /// packet period this endpoint has shown.</para>
    /// </remarks>
    private static async Task<TimeSpan> WhenTheSamplesStop(
        Capture capture, Stopwatch clock, TimeSpan giveUpAfter)
    {
        var settle = TimeSpan.FromMilliseconds(400);
        var deadline = clock.Elapsed + giveUpAfter;

        var seen = capture.SamplesSeen;
        var lastChange = clock.Elapsed;

        while (clock.Elapsed < deadline)
        {
            await Task.Delay(2, CancellationToken.None);

            var now = capture.SamplesSeen;

            if (now != seen)
            {
                seen = now;
                lastChange = clock.Elapsed;

                continue;
            }

            if (clock.Elapsed - lastChange > settle)
            {
                return lastChange;
            }
        }

        return lastChange;
    }

    /// <summary>Every frame the fake wire took, as hex, in order.</summary>
    private static string[] Frames(Built scene)
        => scene.Port.Written.Select(Hex).ToArray();

    /// <summary>The wire as one readable line.</summary>
    private static string Wire(Built scene)
    {
        var frames = Frames(scene);

        return frames.Length == 0 ? "nothing" : string.Join(" | ", frames);
    }

    /// <summary>One frame, as a reader sees it.</summary>
    private static string Hex(IEnumerable<byte> bytes)
        => string.Join(' ', bytes.Select(b => b.ToString("X2", CultureInfo.InvariantCulture)));

    /// <summary>Every `ft8_transmission` line the application's writer left.</summary>
    /// <remarks>
    /// **READ BACK OFF DISK, NOT OFF A DOUBLE** (`CLAUDE.md` §0.0), the way
    /// <c>TheWholeContactWalksThroughTheApplicationTests.TransmitLines</c> reads it.
    /// </remarks>
    private List<string> TransmitLines()
        => !Directory.Exists(_folder)
            ? []
            : Directory.GetFiles(_folder, "*.jsonl")
                .OrderBy(f => f, StringComparer.Ordinal)
                .SelectMany(File.ReadAllLines)
                .Where(l => l.Contains(
                    "\"" + TransmitRecord.EventName + "\"", StringComparison.Ordinal))
                .ToList();

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
