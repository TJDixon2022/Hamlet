using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Telemetry;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **THE OWNER'S VERDICT CARRIES WHAT THE SCOPE SAW AT THE PRESS** (work instruction 476
/// task 3, step 12 criterion 12.3, R90, HM-DEC-185).
/// </summary>
/// <remarks>
/// <para>**SO THE NEXT UNIT CAN READ WHETHER THE SCOPE SAW KEYING WHERE HE HEARD IT.** Eight
/// fields join 474's row, each asserted by name, and the set stays closed: 474's eighteen
/// and these eight, nothing else.</para>
/// <para>**A DRIVEN DETECTOR, NO RECORDING** (R88): the scope is fed a keyed tone made here
/// and stopped mid-dah, so a mark is up at the press.</para>
/// </remarks>
public sealed class TheVerdictCarriesTheScopeTests
{
    private static readonly string[] ScopeFields =
    {
        "scopeEnvelopeDb", "scopeFloorDb", "scopeThresholdDb", "scopeMark",
        "scopeRunMs", "scopePitchHz", "scopeContrastDb", "scopeMarksLast4s", "shapeScore", "sequencesStanding", "shapeLight", "shapeFill",
    };

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
        "scopeEnvelopeDb", "scopeFloorDb", "scopeThresholdDb", "scopeMark",
        "scopeRunMs", "scopePitchHz", "scopeContrastDb", "scopeMarksLast4s", "shapeScore", "sequencesStanding", "shapeLight", "shapeFill",
        "scopePeakHz", "scopePeakDb", "scopePeakLevel", "scopeFramesLast4s",
    };

    /// <remarks>
    /// Proves work instruction 480's four fields carry what the radio's scope said at the
    /// press - the pointed pitch, its level on the radio's own scale and the frames of the last
    /// four seconds - and that the decibels are null, because the radio's scale carries none.
    /// </remarks>
    [Fact]
    public void APressCarriesWhereTheRadiosScopePointed()
    {
        var rows = new List<(TelemetryCategory Category, string Event, IReadOnlyDictionary<string, object?> Data)>();
        var hearing = new CwHearingViewModel(
            new Recording(rows),
            () => CwHearingRig.Unknown with { ScopePeakHz = 850, ScopePeakLevel = 125, ScopeFramesLast4s = 18 });

        hearing.IdiotCommand.Execute(null);

        var row = Assert.Single(rows);

        Assert.Equal(Fields.OrderBy(f => f), row.Data.Keys.OrderBy(k => k));
        Assert.Equal(850.0, row.Data["scopePeakHz"]);
        Assert.Null(row.Data["scopePeakDb"]);
        Assert.Equal(125, row.Data["scopePeakLevel"]);
        Assert.Equal(18, row.Data["scopeFramesLast4s"]);
    }

    /// <remarks>
    /// Proves a press mid-dah writes the scope's state as it stood: a mark up, its run, the
    /// pitch and contrast, the envelope over the threshold, and the marks of the last four
    /// seconds - and that the row's key set is closed.
    /// </remarks>
    [Fact]
    public void APressMidDahCarriesTheScopeAsItStood()
    {
        var rows = new List<(TelemetryCategory Category, string Event, IReadOnlyDictionary<string, object?> Data)>();
        var hearing = new CwHearingViewModel(new Recording(rows), () => CwHearingRig.Unknown);

        // C and Q sent (Q ends 2.42 s), then the second C stopped 90 ms into its first dah (from 2.60 s):
        // eight marks stood (work instruction 515: what stood, at the standing pitch), the ninth up.
        var detector = Keyed(2.69);
        var reading = detector.Reading;

        hearing.ObserveScope(CwScopeFrame.From(detector.History(), detector.HopMs, reading));
        hearing.AgreeCommand.Execute(null);

        var row = Assert.Single(rows);

        Assert.Equal("owner_verdict", row.Event);
        Assert.Equal(Fields.OrderBy(f => f), row.Data.Keys.OrderBy(k => k));

        Assert.True(reading.Mark);
        Assert.Equal(true, row.Data["scopeMark"]);
        Assert.Equal(reading.EnvelopeDb, row.Data["scopeEnvelopeDb"]);
        Assert.Equal(reading.FloorDb, row.Data["scopeFloorDb"]);
        Assert.Equal(reading.ThresholdDb, row.Data["scopeThresholdDb"]);
        Assert.Equal(reading.RunMs, row.Data["scopeRunMs"]);
        Assert.Equal(reading.PitchHz, row.Data["scopePitchHz"]);
        Assert.Equal(reading.ContrastDb, row.Data["scopeContrastDb"]);
        Assert.Equal(8, row.Data["scopeMarksLast4s"]);

        Assert.InRange((double)row.Data["scopeRunMs"]!, 70, 110);
        Assert.InRange((double)row.Data["scopePitchHz"]!, 717, 767);
        Assert.True((double)row.Data["scopeEnvelopeDb"]! > (double)row.Data["scopeThresholdDb"]!);
    }

    /// <remarks>
    /// Proves a press with nothing listening writes the scope's fields as null, false and
    /// nought, never NaN, and a press in a gap writes no pitch.
    /// </remarks>
    [Fact]
    public void NothingMeasuredIsNullAndAGapHasNoPitch()
    {
        var rows = new List<(TelemetryCategory Category, string Event, IReadOnlyDictionary<string, object?> Data)>();
        var hearing = new CwHearingViewModel(new Recording(rows), () => CwHearingRig.Unknown);

        hearing.IdiotCommand.Execute(null);

        var empty = rows[^1].Data;

        Assert.Null(empty["scopeEnvelopeDb"]);
        Assert.Null(empty["scopeFloorDb"]);
        Assert.Null(empty["scopeThresholdDb"]);
        Assert.Equal(false, empty["scopeMark"]);
        Assert.Equal(0.0, empty["scopeRunMs"]);
        Assert.Null(empty["scopePitchHz"]);
        Assert.Null(empty["scopeContrastDb"]);
        Assert.Equal(0, empty["scopeMarksLast4s"]);
        Assert.DoesNotContain(empty.Values, v => v is double d && double.IsNaN(d));

        // After the Q, 120 ms into the gap before the second C: eight marks stood, none up, and the verdict holds
        // through the gap with its pitch (work instructions 488 and 515).
        var detector = Keyed(2.54);
        hearing.ObserveScope(CwScopeFrame.From(detector.History(), detector.HopMs, detector.Reading));
        hearing.IdiotCommand.Execute(null);

        var gap = rows[^1].Data;

        Assert.Equal(false, gap["scopeMark"]);
        Assert.InRange((double)gap["scopePitchHz"]!, 717, 767);
        Assert.Null(gap["scopeContrastDb"]);
        Assert.IsType<double>(gap["scopeEnvelopeDb"]);
        Assert.Equal(8, gap["scopeMarksLast4s"]);
    }

    /// <remarks>Proves the row still goes through the existing writer as one line, 26 fields.</remarks>
    [Fact]
    public void TheRowIsStillOneLineOfTheExistingWriter()
    {
        var hearing = new CwHearingViewModel(null, () => CwHearingRig.Unknown);
        var detector = Keyed(2.69);

        hearing.ObserveScope(CwScopeFrame.From(detector.History(), detector.HopMs, detector.Reading));

        var line = JsonlTelemetry.Serialize(new TelemetryEvent(
            DateTime.UtcNow, "s", TelemetryLevel.Info, "test",
            TelemetryCategory.Cw, CwHearingViewModel.VerdictEvent, hearing.VerdictRow("agree")));

        using var json = System.Text.Json.JsonDocument.Parse(line);
        var data = json.RootElement.GetProperty("data");

        Assert.DoesNotContain('\n', line);
        Assert.Equal(Fields.Length, data.EnumerateObject().Count());
        Assert.All(ScopeFields, f => Assert.True(data.TryGetProperty(f, out _), f));
        Assert.True(data.GetProperty("scopeMark").GetBoolean());
        Assert.Equal(8, data.GetProperty("scopeMarksLast4s").GetInt32());
    }

    /// <summary>C, Q, C at 20 words a minute after 0.8 s of noise (work instruction 515: long enough that the C and the Q stand).</summary>
    private static readonly (bool On, double Ms)[] Keying =
    {
        (false, 800),
        (true, 180), (false, 60), (true, 60), (false, 60), (true, 180), (false, 60), (true, 60),
        (false, 180),
        (true, 180), (false, 60), (true, 180), (false, 60), (true, 60), (false, 60), (true, 180),
        (false, 180),
        (true, 180), (false, 60), (true, 60), (false, 60), (true, 180), (false, 60), (true, 60),
        (false, 3000),
    };

    private static CwEnvelopeDetector Keyed(double seconds)
    {
        const int rate = 48_000;

        var detector = new CwEnvelopeDetector(rate);
        detector.SetPassband(742, 500);

        var random = new Random(4763);
        var total = (int)(seconds * rate);
        var audio = new float[total];
        var n = 0;

        foreach (var (on, ms) in Keying)
        {
            for (var i = 0; i < (int)Math.Round(ms * rate / 1000) && n < total; i++, n++)
            {
                var u1 = 1.0 - random.NextDouble();
                var u2 = random.NextDouble();
                var noise = 0.06 * Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
                var tone = on ? 0.3 * Math.Sin(2 * Math.PI * 742 * n / rate) : 0;

                audio[n] = (float)(tone + noise);
            }
        }

        for (var offset = 0; offset < total; offset += 960)
        {
            detector.Process(audio.AsSpan(offset, Math.Min(960, total - offset)));
        }

        return detector;
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
