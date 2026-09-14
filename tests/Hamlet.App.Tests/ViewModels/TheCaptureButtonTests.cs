using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 344 task 1: **a press keeps the receive audio, so the demodulator can
/// be proved against real air.**
/// </summary>
/// <remarks>
/// <para>**WHY IT EXISTS.** On 2026-09-13 the first PSK31 row off 7.070 reached the screen
/// and it was garbled - `dt  oe epe Ae peey@teI` - the shape a decoder makes when most bits
/// are right and a few are wrong. Nothing in the tree can say why, because **no audio off
/// the air has ever been through the demodulator**: every fixture under `assets/fixtures/`
/// was made by `reference-modem.py` on a machine with no radio (FACT-004, FACT-006).</para>
/// <para>**THE DEVICE STREAM, NOT HAMLET'S VERSION OF IT** (the instruction, §10). The PSK31
/// path resamples 48 kHz down to 8 before a demodulator sees it, so a capture taken below
/// that step can only prove the resampler right. **The first assertion is the sample rate of
/// the written file**, and it is the one that would catch the tap being moved.</para>
/// <para>**NOTHING HERE TRANSMITS** (§0.2). The capture is receive only, and pressing it
/// keys, arms and composes nothing.</para>
/// <para>**COMPUTED, NOT SEEN.** Nothing here looks at a pixel; the button's own binding is
/// `BindingHealthTests`' question.</para>
/// </remarks>
public sealed class TheCaptureButtonTests : IDisposable
{
    private const string OwnCall = "KC3QIS";

    /// <summary>The device rate this test feeds, which is the one the radio delivers.</summary>
    private const int DeviceRate = 48_000;

    private readonly ITestOutputHelper _output;
    private readonly string _folder;
    private readonly string _captures;
    private readonly string _wasCaptureFolder;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the numbers are printed.</param>
    public TheCaptureButtonTests(ITestOutputHelper output)
    {
        _output = output;

        var root = Path.Combine(
            Path.GetTempPath(), "hamlet-capture-" + Guid.NewGuid().ToString("N"));

        _folder = Path.Combine(root, "telemetry");
        _captures = Path.Combine(root, "captures");

        Directory.CreateDirectory(_folder);

        // **THE OPERATOR'S OWN CAPTURES ARE NOT A TEST'S TO WRITE INTO**, which is what
        // this seam already exists for.
        _wasCaptureFolder = MainWindowViewModel.CaptureFolder;
        MainWindowViewModel.CaptureFolder = _captures;
    }

    /// <summary>Puts the capture folder back and removes the temporary one.</summary>
    public void Dispose()
    {
        MainWindowViewModel.CaptureFolder = _wasCaptureFolder;

        try
        {
            Directory.Delete(Path.GetDirectoryName(_folder)!, true);
        }
        catch (IOException)
        {
            // A left-over temp folder is not a test failure.
        }
    }

    /// <summary>**A press keeps the device stream, at the device rate, before the resampler.**</summary>
    /// <remarks>
    /// **THE AUDIO GOES IN THROUGH THE TAP**, the way it does on the air, so this asserts
    /// the wiring and not only the recorder. Were the feed taken after `Psk31Resampler` the
    /// file would read 8000 Hz and a quarter of the samples, and both assertions below
    /// would fail.
    /// </remarks>
    [Fact]
    public void APressKeepsTheDeviceStreamAndNotHamletsVersionOfIt()
    {
        using var telemetry = new JsonlTelemetry(_folder, "344", _ => true);

        var model = Listening(telemetry);

        model.CapturePsk31Command.Execute(null);

        Assert.StartsWith("capturing", model.Psk31CaptureLine, StringComparison.Ordinal);

        // **TWO SECONDS OF A REAL 48 kHz TONE**, fed a quarter of a second at a time the
        // way the device delivers it.
        Feed(model, seconds: 2.0);

        // **THE SECOND PRESS KEEPS WHAT IT HAS**, which is the early stop.
        model.CapturePsk31Command.Execute(null);

        var file = Assert.Single(Directory.GetFiles(_captures, "*.wav"));
        var kept = WavAudio.Read(file);

        _output.WriteLine(
            "wrote " + Path.GetFileName(file)
            + ": " + kept.SampleRate.ToString(CultureInfo.InvariantCulture) + " Hz, "
            + kept.Samples.Length.ToString(CultureInfo.InvariantCulture) + " samples, "
            + kept.Duration.TotalSeconds.ToString("0.00", CultureInfo.InvariantCulture) + " s, "
            + new FileInfo(file).Length.ToString(CultureInfo.InvariantCulture) + " bytes");

        // **THE DEVICE RATE, WHICH IS THE WHOLE POINT OF THE TAP'S PLACE.**
        Assert.Equal(DeviceRate, kept.SampleRate);

        // **AND ALL OF IT**, within one tick of the two seconds fed.
        Assert.InRange(kept.Duration.TotalSeconds, 1.7, 2.05);

        // **16-BIT PCM MONO**, read off the header rather than assumed.
        Assert.Equal(2 * kept.Samples.Length + 44, new FileInfo(file).Length);

        // **THE NAME IS A TIMESTAMP AND NOTHING ELSE** (HM-DEC-018, §2.1).
        Assert.StartsWith("psk31-", Path.GetFileName(file), StringComparison.Ordinal);
        Assert.DoesNotContain(OwnCall, Path.GetFileName(file), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>**It stops at two minutes on its own, and the label says so.**</summary>
    [Fact]
    public void ItStopsAtTwoMinutesOnItsOwn()
    {
        using var telemetry = new JsonlTelemetry(_folder, "344", _ => true);

        var model = Listening(telemetry);

        model.CapturePsk31Command.Execute(null);

        // **PAST THE END ON PURPOSE.** Three minutes are offered and two are kept, so the
        // ceiling is the recorder's and not the caller's.
        var lump = new float[DeviceRate];

        for (var second = 0; second < 180; second++)
        {
            model.FeedPsk31CaptureForTests(lump.AsSpan(), DeviceRate);
        }

        var file = Assert.Single(Directory.GetFiles(_captures, "*.wav"));
        var kept = WavAudio.Read(file);

        _output.WriteLine(
            "offered 180 s, kept " + kept.Duration.TotalSeconds.ToString(
                "0.00", CultureInfo.InvariantCulture) + " s; the press reads '"
            + model.Psk31CaptureLine + "'");

        Assert.Equal(Psk31Capture.Seconds, kept.Duration.TotalSeconds, 3);
        Assert.Equal("captured · 2:00", model.Psk31CaptureLine);
    }

    /// <summary>**The two events carry what the instruction asks, and nothing personal.**</summary>
    [Fact]
    public void TheEventsCarryWhatWasAskedAndNothingPersonal()
    {
        string[] lines;

        using (var telemetry = new JsonlTelemetry(_folder, "344", _ => true))
        {
            var model = Listening(telemetry);

            // **AUDIO FIRST, THE WAY HE PRESSES IT.** The resampler is made on the first
            // tick, so a press before any audio has arrived has no device rate to write -
            // which this test found, and which the started event now falls back to the
            // tap for and leaves absent where neither knows it (section 4).
            Feed(model, seconds: 0.5);

            model.CapturePsk31Command.Execute(null);

            Feed(model, seconds: 1.0);

            model.CapturePsk31Command.Execute(null);
        }

        // **READ AFTER THE SINK IS CLOSED.** `JsonlTelemetry` buffers, so a read inside the
        // block misses the last line - which is the one this test is about.
        lines = Directory.GetFiles(_folder, "*.jsonl").SelectMany(File.ReadAllLines).ToArray();

        var started = Assert.Single(Events(lines, "psk31_capture_started"));
        var finished = Assert.Single(Events(lines, "psk31_capture_finished"));

        _output.WriteLine("started : " + started.GetRawText());
        _output.WriteLine("finished: " + finished.GetRawText());

        Assert.Equal(DeviceRate, started.GetProperty("deviceSampleRate").GetInt32());
        Assert.Equal(Psk31Capture.Seconds, started.GetProperty("seconds").GetDouble(), 3);

        Assert.InRange(finished.GetProperty("seconds").GetDouble(), 0.7, 1.05);
        Assert.Equal(DeviceRate, finished.GetProperty("deviceSampleRate").GetInt32());
        Assert.True(finished.GetProperty("bytes").GetInt64() > 44);
        Assert.True(finished.GetProperty("earlyStop").GetBoolean());

        // **THE FINGERPRINT IS THE FILE'S OWN**, so a capture and a record can be matched
        // a year later without the record naming a path.
        var sha = finished.GetProperty("sha256").GetString();
        var file = Assert.Single(Directory.GetFiles(_captures, "*.wav"));

        Assert.Equal(64, sha!.Length);
        Assert.Equal(Psk31Capture.Fingerprint(file), sha);

        // **THE CARRIERS HELD WHEN IT STARTED**, present as a field even where none were.
        Assert.True(finished.TryGetProperty("carriersHeld", out _));
        Assert.True(finished.TryGetProperty("carriers", out _));

        // **NOTHING PERSONAL, AND NOT THE PATH EITHER** (HM-DEC-018, §2.1). A capture folder
        // under `%AppData%` carries the account name, which is a person.
        var whole = string.Join("\n", lines);

        Assert.DoesNotContain(OwnCall, whole, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("FN00DJ", whole, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(_captures, whole, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".wav", whole, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>**The panel says where the file went, and what to do with it.**</summary>
    [Fact]
    public void ThePanelSaysWhereTheFileWent()
    {
        using var telemetry = new JsonlTelemetry(_folder, "344", _ => true);

        var model = Listening(telemetry);

        Assert.Equal(MainWindowViewModel.Psk31CaptureIdle, model.Psk31CaptureLine);
        Assert.False(model.HasPsk31CaptureWhere);

        model.CapturePsk31Command.Execute(null);

        Feed(model, seconds: 1.0);

        model.CapturePsk31Command.Execute(null);

        var file = Assert.Single(Directory.GetFiles(_captures, "*.wav"));

        _output.WriteLine("the panel says: " + model.Psk31CaptureWhere);

        Assert.True(model.HasPsk31CaptureWhere);
        Assert.Contains(file, model.Psk31CaptureWhere, StringComparison.Ordinal);

        // **AND WHAT TO DO WITH IT**, which is the one line the instruction asks be told.
        Assert.Contains(
            "assets\\fixtures\\captured\\",
            model.Psk31CaptureWhere,
            StringComparison.Ordinal);
    }

    /// <summary>**The press belongs to PSK31 and shows on no other mode.**</summary>
    [Fact]
    public void ThePressBelongsToPsk31()
    {
        var model = Listening(null);

        Assert.True(model.HasPsk31Capture);

        model.ChooseDigitalModeCommand.Execute("FT8");

        Assert.False(model.HasPsk31Capture);
    }

    /// <summary>**Pressing under Olivia writes the WAV and the events, with `mode: olivia`.**</summary>
    /// <remarks>
    /// <para>**THE SAME BUTTON ON THE OLIVIA PANEL** (work instruction 358 task 3,
    /// `PHASE_PLAN.md` R30). Nothing reads Olivia yet, so the first evening's air is the
    /// fixture the next unit is proved against; the press has to work before the
    /// demodulator exists.</para>
    /// <para>**THE DEVICE STREAM WITH NO DECODER UNDER IT.** Under Olivia the tick hands a
    /// running capture the tap's audio and does nothing else, so the file is at 48 kHz and
    /// no PSK31 listener starts to supply it.</para>
    /// </remarks>
    [Fact]
    public void PressingUnderOliviaWritesTheWavAndTheEventsWithModeOlivia()
    {
        string[] lines;
        string where;

        using (var telemetry = new JsonlTelemetry(_folder, "358", _ => true))
        {
            var model = Listening(telemetry);

            model.ChooseDigitalModeCommand.Execute("Olivia");

            Assert.True(model.HasPsk31Capture);

            model.CapturePsk31Command.Execute(null);

            Feed(model, seconds: 2.0);

            model.CapturePsk31Command.Execute(null);

            where = model.Psk31CaptureWhere;
        }

        lines = Directory.GetFiles(_folder, "*.jsonl").SelectMany(File.ReadAllLines).ToArray();

        var file = Assert.Single(Directory.GetFiles(_captures, "*.wav"));
        var kept = WavAudio.Read(file);

        _output.WriteLine(
            "wrote " + Path.GetFileName(file) + ": "
            + kept.SampleRate.ToString(CultureInfo.InvariantCulture) + " Hz, "
            + kept.Duration.TotalSeconds.ToString("0.00", CultureInfo.InvariantCulture) + " s");
        _output.WriteLine("the panel says: " + where);

        Assert.Equal(DeviceRate, kept.SampleRate);
        Assert.InRange(kept.Duration.TotalSeconds, 1.7, 2.05);
        Assert.StartsWith("olivia-", Path.GetFileName(file), StringComparison.Ordinal);
        Assert.Contains(file, where, StringComparison.Ordinal);

        var started = Assert.Single(Events(lines, "olivia_capture_started"));
        var finished = Assert.Single(Events(lines, "olivia_capture_finished"));

        _output.WriteLine("started : " + started.GetRawText());
        _output.WriteLine("finished: " + finished.GetRawText());

        Assert.Equal("olivia", started.GetProperty("mode").GetString());
        Assert.Equal("olivia", finished.GetProperty("mode").GetString());
        Assert.Equal(DeviceRate, finished.GetProperty("deviceSampleRate").GetInt32());
        Assert.Equal(Psk31Capture.Fingerprint(file), finished.GetProperty("sha256").GetString());

        // **NOT RECORDED AS A PSK31 CAPTURE, AND NO PSK31 LISTENER SUPPLIED IT.**
        Assert.Empty(Events(lines, "psk31_capture_started"));
        Assert.Empty(Events(lines, "psk31_listening_started"));

        var whole = string.Join("\n", lines);

        Assert.DoesNotContain(OwnCall, whole, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".wav", whole, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Feed a tone through the tap the way the device delivers it.</summary>
    private static void Feed(MainWindowViewModel model, double seconds)
    {
        var chunk = DeviceRate / 4;
        var total = (int)(seconds * DeviceRate);
        var lump = new float[chunk];
        var phase = 0.0;

        for (var at = 0; at < total; at += chunk)
        {
            var count = Math.Min(chunk, total - at);

            for (var i = 0; i < count; i++)
            {
                // A plain 1000 Hz tone. What is being asserted is the rate and the
                // length of what was kept, not what any decoder makes of it.
                lump[i] = (float)(0.5 * Math.Sin(phase));
                phase += 2 * Math.PI * 1000 / DeviceRate;
            }

            model.TapForTests!.Take(lump.AsSpan(0, count), DeviceRate);
            model.LookForASlotForTests();
        }
    }

    private static IReadOnlyList<JsonElement> Events(string[] lines, string name)
        => lines
            .Select(line => JsonDocument.Parse(line).RootElement)
            .Where(e => e.GetProperty("event").GetString() == name)
            .Select(e => e.GetProperty("data"))
            .ToList();

    private static MainWindowViewModel Listening(JsonlTelemetry? telemetry)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = OwnCall;
        settings.Operator.GridSquare = "FN00DJ";

        var model = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
            TapForTests = new AudioTap(),
            ClockOffset = new ClockOffset(0.033, DateTime.UtcNow),
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        model.ChooseDigitalModeCommand.Execute("PSK31");

        return model;
    }
}
