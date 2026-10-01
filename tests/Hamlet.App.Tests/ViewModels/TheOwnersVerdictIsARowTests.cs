using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Tests.Views;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Telemetry;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **EACH PRESS WRITES ONE ROW: THE OWNER'S EAR BESIDE THE DETECTOR'S STATE** (work
/// instruction 474 task 3, step 11 criterion 11.3, HM-DEC-184).
/// </summary>
/// <remarks>
/// <para>**THE NEXT UNIT READS THESE ROWS AND NOTHING ELSE**, so every field is asserted by
/// name and the set is closed: a field missing is a question the next unit cannot answer,
/// and a field added is something the instruction did not ask the row to carry.</para>
/// <para>**NO AUDIO** (R88). The press captures none and writes no sidecar; the view model
/// is driven with the detector's state directly.</para>
/// </remarks>
public sealed class TheOwnersVerdictIsARowTests
{
    private static readonly string[] Fields =
    {
        "verdict", "light",
        // Work instruction 515: the three tracker fields left the row with the watched bin (R114), and
        // mixingHz, the pitch the decoder prints at since unit 488, is named here at last.
        "mixingHz",
        "meterVerdict", "meterHz", "meterScore", "meterMedianMs", "meterSwingDb",
        "survey",
        "frequency", "mode", "agc", "preamp",
        "inputPeakDb", "inputFloorDb",
        "sinceVerdictMs",

        // Work instruction 476 task 3 extends the row with the scope's state at the press;
        // TheVerdictCarriesTheScopeTests asserts these eight by value.
        "scopeEnvelopeDb", "scopeFloorDb", "scopeThresholdDb", "scopeMark",
        "scopeRunMs", "scopePitchHz", "scopeContrastDb", "scopeMarksLast4s", "shapeScore", "sequencesStanding", "shapeLight", "shapeFill",

        // Work instruction 480 task 2: where the radio's own scope said the signal was.
        "scopePeakHz", "scopePeakDb", "scopePeakLevel", "scopeFramesLast4s",
    };

    /// <remarks>Proves "I agree with you" writes one cw owner_verdict row with every field.</remarks>
    [Fact]
    public void AgreeWritesOneRowCarryingEveryField()
    {
        var (hearing, rows, _) = Driven();

        hearing.AgreeCommand.Execute(null);

        var row = Assert.Single(rows);

        Assert.Equal(TelemetryCategory.Cw, row.Category);
        Assert.Equal("owner_verdict", row.Event);
        Assert.Equal(Fields.OrderBy(f => f), row.Data.Keys.OrderBy(k => k));

        Assert.Equal("agree", row.Data["verdict"]);
        // Work instruction 478: the light is gone and the field carries the bars' verdict.
        // Nothing has fed the scope here, so the bars say no keying.
        Assert.Equal("the bars say no keying", row.Data["light"]);
        Assert.Equal("keying", row.Data["meterVerdict"]);
        Assert.Equal(610.0, row.Data["meterHz"]);
        Assert.Equal(0.21, row.Data["meterScore"]);
        Assert.Equal(70.0, row.Data["meterMedianMs"]);
        Assert.Equal(22.0, row.Data["meterSwingDb"]);
        Assert.Equal(7_030_000L, row.Data["frequency"]);
        Assert.Equal("CW", row.Data["mode"]);
        Assert.Equal("FAST", row.Data["agc"]);
        Assert.Equal("OFF", row.Data["preamp"]);
        Assert.Equal(-12.5, row.Data["inputPeakDb"]);
        Assert.Equal(-61.0, row.Data["inputFloorDb"]);
        Assert.Equal(2500L, row.Data["sinceVerdictMs"]);

        var survey = Assert.IsAssignableFrom<IReadOnlyList<IReadOnlyDictionary<string, object?>>>(row.Data["survey"]);
        var bin = Assert.Single(survey);

        Assert.Equal(575.0, bin["hz"]);
        Assert.Equal(-40.0, bin["levelDb"]);
    }

    /// <remarks>
    /// Proves the row's <c>light</c> field is the scope's keying verdict in words, and that
    /// <c>sinceVerdictMs</c> counts from when that verdict last changed (work instruction 478).
    /// </remarks>
    [Fact]
    public void TheLightFieldIsTheBarsVerdictInWords()
    {
        var (hearing, rows, at) = Driven();

        hearing.ObserveScope(CwScopeFrame.Empty with
        {
            Reading = CwEnvelopeReading.None with { Keying = true },
        });
        hearing.AgreeCommand.Execute(null);

        Assert.Equal("the bars say keying", rows[^1].Data["light"]);
        Assert.Equal(0L, rows[^1].Data["sinceVerdictMs"]);

        hearing.ObserveScope(CwScopeFrame.Empty);
        hearing.IdiotCommand.Execute(null);

        Assert.Equal("the bars say no keying", rows[^1].Data["light"]);
        Assert.NotEqual(at, hearing.LightChangedUtc);
    }

    /// <remarks>Proves "You're an idiot" writes the same row with its own verdict.</remarks>
    [Fact]
    public void IdiotWritesOneRowWithItsOwnVerdict()
    {
        var (hearing, rows, _) = Driven();

        hearing.IdiotCommand.Execute(null);

        var row = Assert.Single(rows);

        Assert.Equal("owner_verdict", row.Event);
        Assert.Equal("idiot", row.Data["verdict"]);
        Assert.Equal(Fields.OrderBy(f => f), row.Data.Keys.OrderBy(k => k));
    }

    /// <remarks>
    /// Proves a figure the detector has not measured is written as null and never as NaN,
    /// which the writer's serializer refuses.
    /// </remarks>
    [Fact]
    public void AnUnmeasuredFigureIsNullNeverNaN()
    {
        var rows = new List<(TelemetryCategory Category, string Event, IReadOnlyDictionary<string, object?> Data)>();
        var hearing = new CwHearingViewModel(new Recording(rows), () => CwHearingRig.Unknown);

        hearing.IdiotCommand.Execute(null);

        var row = Assert.Single(rows);

        Assert.Null(row.Data["mixingHz"]);
        Assert.Null(row.Data["meterHz"]);
        Assert.Null(row.Data["frequency"]);
        Assert.DoesNotContain(row.Data.Values, v => v is double d && double.IsNaN(d));
    }

    /// <remarks>
    /// Proves the row goes through the existing `.jsonl` writer's serializer as one line,
    /// category `cw`, event `owner_verdict`, survey and all - measured and unmeasured.
    /// </remarks>
    [Fact]
    public void TheRowIsOneLineOfTheExistingWriterUnderCw()
    {
        var (hearing, _, _) = Driven();
        var empty = new CwHearingViewModel(null, () => CwHearingRig.Unknown);

        foreach (var (row, surveyed) in new[] { (hearing.VerdictRow("agree"), 1), (empty.VerdictRow("idiot"), 0) })
        {
            var line = JsonlTelemetry.Serialize(new TelemetryEvent(
                DateTime.UtcNow, "s", TelemetryLevel.Info, "test",
                TelemetryCategory.Cw, CwHearingViewModel.VerdictEvent, row));

            using var json = System.Text.Json.JsonDocument.Parse(line);
            var root = json.RootElement;

            Assert.DoesNotContain('\n', line);
            Assert.Equal("cw", root.GetProperty("category").GetString());
            Assert.Equal("owner_verdict", root.GetProperty("event").GetString());
            Assert.Equal(Fields.Length, root.GetProperty("data").EnumerateObject().Count());
            Assert.Equal(surveyed, root.GetProperty("data").GetProperty("survey").GetArrayLength());
        }
    }

    /// <remarks>Proves each button's hover says what it records.</remarks>
    [Fact]
    public void EachButtonSaysWhatItRecords()
    {
        Assert.Contains("writes one row", CwHearingViewModel.AgreeTip, StringComparison.Ordinal);
        Assert.Contains("right", CwHearingViewModel.AgreeTip, StringComparison.Ordinal);
        Assert.Contains("writes one row", CwHearingViewModel.IdiotTip, StringComparison.Ordinal);
        Assert.Contains("wrong", CwHearingViewModel.IdiotTip, StringComparison.Ordinal);
        Assert.Contains("No audio", CwHearingViewModel.AgreeTip, StringComparison.Ordinal);
        Assert.Contains("No audio", CwHearingViewModel.IdiotTip, StringComparison.Ordinal);
    }

    /// <remarks>Proves both buttons are on the CW tab, with their words and hover, wired.</remarks>
    [AvaloniaFact]
    public void BothButtonsAreOnTheCwTab()
    {
        var (window, _) = TheControlsTimCanPress.Open();

        try
        {
            var cw = TheTopRowTests.Named<Grid>(window, "CwWorkspace");
            var buttons = cw.GetVisualDescendants().OfType<Button>().ToList();
            var agree = buttons.Single(b => b.Content as string == "I agree with you");
            var idiot = buttons.Single(b => b.Content as string == "You're an idiot");

            Assert.NotNull(agree.Command);
            Assert.NotNull(idiot.Command);
            Assert.Equal(CwHearingViewModel.AgreeTip, ToolTip.GetTip(agree));
            Assert.Equal(CwHearingViewModel.IdiotTip, ToolTip.GetTip(idiot));
        }
        finally
        {
            window.Close();
        }
    }

    private static (CwHearingViewModel Hearing, List<(TelemetryCategory Category, string Event, IReadOnlyDictionary<string, object?> Data)> Rows, DateTime At) Driven()
    {
        var rows = new List<(TelemetryCategory Category, string Event, IReadOnlyDictionary<string, object?> Data)>();
        var now = new DateTime(2026, 9, 27, 21, 0, 0, DateTimeKind.Utc);
        var clock = now;
        var rig = new CwHearingRig(7_030_000, "CW", "FAST", "OFF", -12.5, -61);
        var hearing = new CwHearingViewModel(new Recording(rows), () => rig, () => clock);

        hearing.Observe(new CwHearingState(
            new KeyingReading(KeyingVerdict.Keying, 610, 70, 22, 30, 0.21, false),
            612,
            true,
            false,
            new[] { new KeyingCandidate(575, 60, 180, 3, 6, 18, 12, -40) }));

        clock = now.AddMilliseconds(2500);

        return (hearing, rows, now);
    }

    private sealed class Recording : ITelemetry
    {
        private readonly List<(TelemetryCategory Category, string Event, IReadOnlyDictionary<string, object?> Data)> _rows;

        public Recording(List<(TelemetryCategory Category, string Event, IReadOnlyDictionary<string, object?> Data)> rows)
            => _rows = rows;

        public long DroppedEventCount => 0;

        public void Write(
            TelemetryCategory category, string eventName,
            IReadOnlyDictionary<string, object?>? data = null,
            TelemetryLevel level = TelemetryLevel.Info)
            => _rows.Add((category, eventName, data ?? new Dictionary<string, object?>()));
    }
}
