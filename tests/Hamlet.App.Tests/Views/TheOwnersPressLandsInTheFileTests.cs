using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// **EACH VERDICT BUTTON, PRESSED ON THE CW TAB, PUTS EXACTLY ONE ROW IN THE TELEMETRY FILE**
/// (work instruction 482, step 11 criterion 11.3, R89).
/// </summary>
/// <remarks>
/// <para>Tim, R89: *"write it."* The detector's gates are set from his verdict rows, and a row
/// that never reaches the file sets nothing. <see cref="Hamlet.App.Tests.ViewModels.TheOwnersVerdictIsARowTests"/>
/// proves the row against a recording fake; nothing proved the criterion's own words - a press on
/// the window's button writes one line to the file.</para>
/// <para>**THE REAL WRITER, A TEMPORARY FOLDER, THE FILE READ BACK.** The app hands
/// <see cref="JsonlTelemetry"/> to <see cref="MainWindowViewModel"/> (App.axaml.cs); this does
/// the same, with the category switch read from the settings as the app reads it, pointed at a
/// folder of its own. The owner's telemetry folder is never touched.</para>
/// <para>**THE GATE IS OPENED THE WAY THE APP OPENS IT.** Both buttons are enabled by
/// <c>IsDecoding</c>, which only <c>StartDecoding</c> sets; connecting the training radio starts
/// it on synthesized Morse, with no sound device and no port. The press goes through the
/// <see cref="Button"/> found in the CW tab's visual tree - its <c>IsEnabled</c> and its
/// <c>Command</c> - never through the view model, because the commands carry no gate of their own.</para>
/// <para>**THE SIX THINGS** of the criterion, each by the row keys that carry them (the trace,
/// <c>.run-unit/unit482-trace.txt</c>): the verdict; the light, which R92 retired and whose field
/// now says what the bars say; the tracker; the meter's figures; the survey's bins; the rig state.
/// **NOTHING IS KEYED AND NO RECORDING IS READ** (R88).</para>
/// </remarks>
public sealed class TheOwnersPressLandsInTheFileTests : IDisposable
{
    /// <summary>The criterion's six things, and the row keys that carry each one.</summary>
    public static readonly IReadOnlyList<(string Thing, string[] Keys)> TheSixThings = new[]
    {
        ("the verdict", new[] { "verdict" }),
        ("the light, now the bars", new[] { "light", "sinceVerdictMs" }),
        ("the tracker", new[] { "trackerHz", "trackerHasPitch", "trackerHasKeying" }),
        ("the meter's figures", new[] { "meterVerdict", "meterHz", "meterScore", "meterMedianMs", "meterSwingDb" }),
        ("the survey's bins", new[] { "survey" }),
        ("the rig state", new[] { "frequency", "mode", "agc", "preamp", "inputPeakDb", "inputFloorDb" }),
    };

    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(10);

    private readonly string _folder = Path.Combine(
        Path.GetTempPath(), "hamlet-unit482-tel-" + Guid.NewGuid().ToString("N")[..8]);

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the test.</summary>
    /// <param name="output">Where each row read back from the file is printed.</param>
    public TheOwnersPressLandsInTheFileTests(ITestOutputHelper output)
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
    /// "I agree with you" writes one row, "You're an idiot" one more, and the file holds exactly
    /// those two, each carrying all six things.
    /// </summary>
    [AvaloniaFact]
    public async Task EachPressOnTheCwTabWritesOneRowToTheFile()
    {
        var settings = TheControlsTimCanPress.Fixture();
        var telemetry = new JsonlTelemetry(
            _folder,
            "482",
            category => settings.IsTelemetryEnabled(category),
            settings.TelemetryMaxMegabytes * 1024L * 1024L);
        var panel = new MainWindowViewModel(settings, telemetry) { OperatingMode = "CW" };
        var window = new MainWindow { DataContext = panel, Width = 1400, Height = 900 };

        try
        {
            window.Show();
            TheControlsTimCanPress.Settle(window);

            var agree = ButtonOnTheCwTab(window, "I agree with you");
            var idiot = ButtonOnTheCwTab(window, "You're an idiot");

            // The gate is shut until something is listening.
            Assert.False(panel.IsDecoding);
            Assert.False(agree.IsEffectivelyEnabled, "\"I agree with you\" is pressable with nothing listening");
            Assert.False(idiot.IsEffectivelyEnabled, "\"You're an idiot\" is pressable with nothing listening");

            await TheControlsTimCanPress.ConnectAsync(window, panel);

            Assert.True(panel.IsConnected, "the training radio did not connect");
            Assert.True(panel.IsDecoding, "connecting the training radio did not start decoding");
            Assert.Empty(VerdictLines());

            HearTheTrainingRadio(window, panel);

            // 1. I agree with you.
            Press(agree);

            var afterAgree = WaitForVerdictLines(1);
            var agreed = TheRow(Assert.Single(afterAgree), "agree", panel);

            // 3. You're an idiot.
            Press(idiot);

            var afterIdiot = WaitForVerdictLines(2);

            Assert.Equal(2, afterIdiot.Count);
            Assert.Equal(afterAgree[0], afterIdiot[0]);

            var idiotRow = TheRow(afterIdiot[1], "idiot", panel);

            Assert.Equal(Keys(agreed), Keys(idiotRow));
        }
        finally
        {
            await TheControlsTimCanPress.DisconnectAsync(window, panel);
            window.Close();

            // Drains the writer's queue, so a late second line from either press is on disk now.
            telemetry.Dispose();
        }

        // 4. Exactly two owner_verdict rows, in the order pressed.
        var all = VerdictLines();

        _output.WriteLine("owner_verdict rows in the file after the writer drained: " + all.Count);

        Assert.Equal(2, all.Count);
        Assert.Equal(
            new[] { "agree", "idiot" },
            all.Select(l => Data(l).GetProperty("verdict").GetString()).ToArray());
    }

    /// <summary>
    /// Lets the window's own ticks run on the training radio's synthesized Morse until the hearing
    /// tick has handed the detector's state over, so the row carries what it measured rather than
    /// the state before anything was heard; bounded, and it asserts nothing.
    /// </summary>
    private void HearTheTrainingRadio(Window window, MainWindowViewModel panel)
    {
        var clock = Stopwatch.StartNew();

        while (ReferenceEquals(panel.CwHearing.State, CwHearingState.None) && clock.Elapsed < Patience)
        {
            TheControlsTimCanPress.Settle(window);
            Thread.Sleep(50);
        }

        _output.WriteLine(
            "hearing state handed over: " + !ReferenceEquals(panel.CwHearing.State, CwHearingState.None)
            + " after " + clock.ElapsedMilliseconds + " ms; survey bins held: " + panel.CwHearing.State.Survey.Count);
    }

    /// <summary>The button with these words on the CW tab.</summary>
    private static Button ButtonOnTheCwTab(Window window, string words)
        => TheTopRowTests.Named<Grid>(window, "CwWorkspace")
            .GetVisualDescendants()
            .OfType<Button>()
            .Single(b => b.Content as string == words);

    /// <summary>Presses the button as a click does: only while it is enabled, through its own command.</summary>
    private static void Press(Button button)
    {
        Assert.True(button.IsEffectivelyEnabled, "\"" + button.Content + "\" is not enabled while decoding");

        var command = button.Command;

        Assert.NotNull(command);
        Assert.True(command!.CanExecute(button.CommandParameter), "\"" + button.Content + "\" cannot execute");

        command.Execute(button.CommandParameter);
    }

    /// <summary>
    /// Checks one line from the file: category cw, event owner_verdict, this verdict, every key the
    /// row builds, the six things, and no figure that is not a number or null.
    /// </summary>
    private JsonElement TheRow(string line, string verdict, MainWindowViewModel panel)
    {
        _output.WriteLine("ROW " + verdict + ": " + line);

        using var doc = JsonDocument.Parse(line);
        var root = doc.RootElement;

        Assert.Equal("cw", root.GetProperty("category").GetString());
        Assert.Equal(CwHearingViewModel.VerdictEvent, root.GetProperty("event").GetString());

        var data = root.GetProperty("data").Clone();

        Assert.Equal(verdict, data.GetProperty("verdict").GetString());

        // Every key the row builds reached the file.
        Assert.Equal(
            panel.CwHearing.VerdictRow(verdict).Keys.OrderBy(k => k, StringComparer.Ordinal),
            Keys(data));

        foreach (var (thing, keys) in TheSixThings)
        {
            foreach (var key in keys)
            {
                Assert.True(data.TryGetProperty(key, out _), thing + ": the row has no \"" + key + "\"");
            }
        }

        Assert.Equal(JsonValueKind.String, data.GetProperty("light").ValueKind);
        Assert.Contains(
            data.GetProperty("light").GetString(),
            new[] { CwHearingViewModel.BarsKeyingWords, CwHearingViewModel.BarsNoKeyingWords });

        // **THE SURVEY IS ASSERTED AS AN ARRAY ONLY** (the instruction's drop candidate, taken):
        // the training radio's Morse admits no survey bin in the time a test waits, and admitting
        // one would mean touching the survey, which this unit may not. Each bin's hz and levelDb
        // stay proved against the fake in TheOwnersVerdictIsARowTests.
        var survey = data.GetProperty("survey");

        Assert.Equal(JsonValueKind.Array, survey.ValueKind);

        _output.WriteLine("  survey bins: " + survey.GetArrayLength());

        // A figure not measured is null, never NaN: no field is a number written as a word.
        foreach (var key in new[]
        {
            "trackerHz", "meterHz", "meterScore", "meterMedianMs", "meterSwingDb",
            "frequency", "inputPeakDb", "inputFloorDb", "sinceVerdictMs",
        })
        {
            Assert.True(NumberOrNull(data.GetProperty(key)), key + " is " + data.GetProperty(key));
        }

        return data;
    }

    private static bool NumberOrNull(JsonElement value)
        => value.ValueKind is JsonValueKind.Number or JsonValueKind.Null;

    private static IEnumerable<string> Keys(JsonElement data)
        => data.EnumerateObject().Select(p => p.Name).OrderBy(k => k, StringComparer.Ordinal).ToList();

    private static JsonElement Data(string line)
    {
        using var doc = JsonDocument.Parse(line);

        return doc.RootElement.GetProperty("data").Clone();
    }

    /// <summary>
    /// Waits for the writer's thread to put <paramref name="count"/> owner_verdict rows on disk,
    /// then a moment more so a second line from the same press would be seen too.
    /// </summary>
    private IReadOnlyList<string> WaitForVerdictLines(int count)
    {
        var clock = Stopwatch.StartNew();

        while (VerdictLines().Count < count && clock.Elapsed < Patience)
        {
            Thread.Sleep(20);
        }

        Thread.Sleep(300);

        var lines = VerdictLines();

        Assert.True(
            lines.Count == count,
            "expected " + count + " owner_verdict row(s) in the file after " + clock.ElapsedMilliseconds
            + " ms, found " + lines.Count);

        return lines;
    }

    /// <summary>Every complete owner_verdict line on disk, read while the writer may still hold the file.</summary>
    private IReadOnlyList<string> VerdictLines()
    {
        if (!Directory.Exists(_folder))
        {
            return Array.Empty<string>();
        }

        var lines = new List<string>();

        foreach (var file in Directory.GetFiles(_folder, "*.jsonl").OrderBy(f => f, StringComparer.Ordinal))
        {
            string text;

            try
            {
                using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                using var reader = new StreamReader(stream, Encoding.UTF8);
                text = reader.ReadToEnd();
            }
            catch (IOException)
            {
                continue;
            }

            // A line is complete once its newline is written.
            var complete = text[..(text.LastIndexOf('\n') + 1)];

            lines.AddRange(complete
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(l => l.Contains("\"event\":\"" + CwHearingViewModel.VerdictEvent + "\"", StringComparison.Ordinal)));
        }

        return lines;
    }
}
