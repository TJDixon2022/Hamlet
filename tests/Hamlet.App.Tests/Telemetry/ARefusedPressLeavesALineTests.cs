using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Hamlet.App.Settings;
using Hamlet.App.Telemetry;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Telemetry;

/// <summary>
/// All four ways a digital capture press can produce nothing leave a line in the
/// telemetry file saying which one it was.
/// </summary>
/// <remarks>
/// <para>**BEFORE UNIT 234 ALL FOUR WERE SILENT.** Each set `StatusText` and
/// returned, and the next status message overwrote it. On 2026-09-03 the owner
/// pressed this button at the radio, nothing appeared, and afterwards no artefact
/// on the machine could say which of the four it had been — or whether the button
/// had been pressed at all. That question was put to the owner by unit 233 and
/// this class makes it moot: from now on a press that refuses records why.</para>
/// <para>**THE EVIDENCE IS A FILE ON DISK WITH A LINE IN IT** (`CLAUDE.md` §0.0).
/// `JsonlTelemetry.Write` swallows every exception and returns void, so a
/// capturing double asserting that a method was called would prove less than it
/// looks. A real writer is pointed at a temporary folder and the file is read
/// back.</para>
/// <para>**AND THE PAYLOAD IS A REASON CODE AND NOTHING ELSE** (HM-DEC-018): the
/// exception paths carry the exception's type name and never its message, because
/// an IO exception's message holds a file path and a Windows file path holds a
/// person's name.</para>
/// </remarks>
public sealed class ARefusedPressLeavesALineTests : IDisposable
{
    private const int Rate = 12000;

    private readonly string _telemetryFolder = Path.Combine(
        Path.GetTempPath(), "hamlet-unit234-tel-" + Guid.NewGuid().ToString("N")[..8]);

    private readonly string _captureRoot = Path.Combine(
        Path.GetTempPath(), "hamlet-unit234-ref-" + Guid.NewGuid().ToString("N")[..8]);

    private readonly ITestOutputHelper _output;

    public ARefusedPressLeavesALineTests(ITestOutputHelper output)
        => _output = output;

    public void Dispose()
    {
        foreach (var folder in new[] { _telemetryFolder, _captureRoot })
        {
            try
            {
                Directory.Delete(folder, recursive: true);
            }
            catch (Exception)
            {
                // Test cleanup only.
            }
        }
    }

    /// <summary>Nothing is listening: there is no tap at all.</summary>
    [Fact]
    public void NoTapLeavesALine()
        => PressAndExpect(
            DigitalCaptureRefusal.NothingIsListening,
            _ => { });

    /// <summary>A source is open and no samples have arrived through it.</summary>
    [Fact]
    public void NoAudioYetLeavesALine()
        => PressAndExpect(
            DigitalCaptureRefusal.NoAudioYet,
            model => GiveItAnEmptyTap(model));

    /// <summary>
    /// The write fails with an <see cref="System.IO.IOException"/>, because a
    /// file is sitting where the `digital` folder has to go.
    /// </summary>
    [Fact]
    public void AnIoFailureLeavesALine()
        => PressAndExpect(
            DigitalCaptureRefusal.IOException,
            model =>
            {
                GiveItATapHoldingAudio(model);

                Directory.CreateDirectory(_captureRoot);
                File.WriteAllText(Path.Combine(_captureRoot, "digital"), "occupied");
            });

    /// <summary>
    /// The write fails with an
    /// <see cref="System.UnauthorizedAccessException"/>, because a directory is
    /// sitting where the WAV has to go.
    /// </summary>
    /// <remarks>
    /// **THE STAMP IS NOT INJECTABLE**, so the collision is laid for a few
    /// seconds either side of the press rather than for one exact name.
    /// `CaptureDigital` reads `DateTime.UtcNow` inside its own `try`, a fraction
    /// of a millisecond after the line below; the window is generous so that a
    /// slow machine does not turn this into a flake, and if the press ever landed
    /// outside it the test fails loudly rather than passing on the wrong branch.
    /// </remarks>
    [Fact]
    public void AnAccessFailureLeavesALine()
        => PressAndExpect(
            DigitalCaptureRefusal.UnauthorizedAccessException,
            model =>
            {
                GiveItATapHoldingAudio(model);

                var digital = Path.Combine(_captureRoot, "digital");
                Directory.CreateDirectory(digital);

                var now = DateTime.UtcNow;
                for (var second = -1; second <= 3; second++)
                {
                    Directory.CreateDirectory(Path.Combine(
                        digital,
                        "ft8-" + now.AddSeconds(second).ToString(
                            "yyyy-MM-dd-HHmmss", CultureInfo.InvariantCulture)
                        + ".wav"));
                }
            });

    /// <summary>
    /// Set the press up so it refuses the given way, press once, and read the
    /// telemetry file back.
    /// </summary>
    private void PressAndExpect(
        DigitalCaptureRefusal expected, Action<MainWindowViewModel> arrange)
    {
        var was = MainWindowViewModel.CaptureFolder;

        try
        {
            MainWindowViewModel.CaptureFolder = _captureRoot;

            var settings = new AppSettings();
            var telemetry = new JsonlTelemetry(
                _telemetryFolder,
                "1.12.38",
                category => settings.IsTelemetryEnabled(category),
                settings.TelemetryMaxMegabytes * 1024L * 1024L);

            var model = new MainWindowViewModel(settings, telemetry);
            arrange(model);

            model.CaptureDigitalCommand.Execute(null);

            telemetry.Dispose();

            var refusals = RefusalLines();

            _output.WriteLine("MEASURED status  " + model.StatusText);

            foreach (var line in refusals)
            {
                _output.WriteLine("MEASURED line    " + line);
            }

            // **EXACTLY ONE**, because a press is one press: a path that wrote
            // twice would double-count every morning's warnings, and a path that
            // wrote none is the silence this whole unit exists to end.
            var only = Assert.Single(refusals);

            using var doc = JsonDocument.Parse(only);
            var root = doc.RootElement;

            Assert.Equal("warn", root.GetProperty("level").GetString());
            Assert.Equal("decode", root.GetProperty("category").GetString());
            Assert.Equal(
                expected.ToString(),
                root.GetProperty("data").GetProperty("reason").GetString());

            // **THE REASON IS THE WHOLE PAYLOAD.** Anything else here would be a
            // place for an exception message, and an exception message from the
            // file system carries a path (HM-DEC-018).
            Assert.Single(root.GetProperty("data").EnumerateObject());

            // And nothing that could have come out of a message or a path.
            Assert.DoesNotContain("\\", only, StringComparison.Ordinal);
        }
        finally
        {
            MainWindowViewModel.CaptureFolder = was;
        }

        Assert.Equal(was, MainWindowViewModel.CaptureFolder);
    }

    /// <summary>Every refusal line the writer actually put on disk.</summary>
    private IReadOnlyList<string> RefusalLines()
        => Directory.Exists(_telemetryFolder)
            ? Directory.GetFiles(_telemetryFolder, "*.jsonl")
                .SelectMany(File.ReadAllLines)
                .Where(l => l.Contains("\"event\":\"digital_capture_refused\""))
                .ToList()
            : Array.Empty<string>();

    /// <summary>A decoder whose tap has never been handed a sample.</summary>
    private static void GiveItAnEmptyTap(MainWindowViewModel model)
        => SetDecoder(model, new CwDecoder(Rate));

    /// <summary>A decoder whose tap holds a second of audio.</summary>
    private static void GiveItATapHoldingAudio(MainWindowViewModel model)
    {
        model.ClockOffset = new ClockOffset(0, DateTime.UtcNow);

        var decoder = new CwDecoder(Rate);
        decoder.Tap.Take(new float[Rate], Rate);

        SetDecoder(model, decoder);

        Assert.NotNull(decoder.Tap.Snapshot());
    }

    /// <summary>
    /// The seam: `_decoder` is private and set only when a sound card opens, and
    /// nothing here opens one.
    /// </summary>
    private static void SetDecoder(MainWindowViewModel model, CwDecoder decoder)
    {
        var field = typeof(MainWindowViewModel).GetField(
            "_decoder", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(field);
        field!.SetValue(model, decoder);
    }
}
