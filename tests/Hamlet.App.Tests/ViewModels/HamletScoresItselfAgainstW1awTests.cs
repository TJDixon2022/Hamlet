using System.Text.Json;
using Avalonia.Headless.XUnit;
using Hamlet.App.Settings;
using Hamlet.App.Tests.Views;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **Hamlet scores itself against W1AW** (work instruction numbered 509, run as unit 510, task 3,
/// HM-DEC-214).
/// </summary>
/// <remarks>
/// A call decoded from synthetic audio written here into the view model's own transcript, nothing
/// read from disk (R96); the pasted text is typed here. Telemetry goes to a temporary folder and is
/// read back from it.
/// </remarks>
public sealed class HamletScoresItselfAgainstW1awTests : IDisposable
{
    private const string Call = "CQ CQ DE N0CALL N0CALL K";

    private readonly string _telemetryFolder = Path.Combine(
        Path.GetTempPath(), "hamlet-unit510-w1aw-" + Guid.NewGuid().ToString("N")[..8]);

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the score lines are printed.</param>
    public HamletScoresItselfAgainstW1awTests(ITestOutputHelper output) => _output = output;

    /// <summary>Removes the temporary telemetry folder.</summary>
    public void Dispose()
    {
        try
        {
            Directory.Delete(_telemetryFolder, recursive: true);
        }
        catch (Exception)
        {
            // Nothing to clean.
        }
    }

    /// <remarks>
    /// Task 3: the clean call scored against its own text, in lower case, reads 100%; against the
    /// same text with one letter changed reads one wrong; each score writes one `w1aw_score` row.
    /// </remarks>
    [AvaloniaFact]
    public void TheCallScoredAgainstItsOwnTextReadsWhole()
    {
        var settings = new AppSettings();
        var telemetry = new JsonlTelemetry(_telemetryFolder, "1.13.196", settings.IsTelemetryEnabled);
        var panel = new MainWindowViewModel(settings, telemetry) { OperatingMode = "CW" };

        TheTerminalCopiesTests.Decode(panel.Transcript);
        _output.WriteLine($"terminal `{panel.Transcript.PlainText}`");

        panel.W1awPasted = Call.ToLowerInvariant();
        panel.ScoreW1awCommand.Execute(null);
        var whole = panel.W1awScoreLine;

        panel.W1awPasted = "CQ CQ DE N0CALX N0CALL K";
        panel.ScoreW1awCommand.Execute(null);
        var oneWrong = panel.W1awScoreLine;

        telemetry.Dispose();

        var lines = Directory.GetFiles(_telemetryFolder, "*.jsonl").SelectMany(File.ReadAllLines).ToList();

        _output.WriteLine($"{lines.Count} telemetry lines; the last `{lines.LastOrDefault()}`");

        var rows = lines
            .Select(l => JsonDocument.Parse(l).RootElement)
            .Where(e => e.TryGetProperty("event", out var ev) && ev.GetString() == MainWindowViewModel.W1awScoreEvent)
            .ToList();

        _output.WriteLine("whole    : " + whole);
        _output.WriteLine("one wrong: " + oneWrong);

        foreach (var row in rows)
        {
            _output.WriteLine("row      : " + row.GetRawText());
        }

        Assert.EndsWith(": 100% of characters, 0 wrong, 0 missing, 0 extra", whole);
        Assert.Contains(", 1 wrong, 0 missing, 0 extra", oneWrong);
        Assert.Equal(2, rows.Count);
        Assert.Equal("cw", rows[0].GetProperty("category").GetString());
        Assert.Equal(100, rows[0].GetProperty("data").GetProperty("percent").GetInt32());
        Assert.Equal(1, rows[1].GetProperty("data").GetProperty("wrong").GetInt32());
    }
}
