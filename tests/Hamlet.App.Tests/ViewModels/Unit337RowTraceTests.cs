using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Hamlet.App.Settings;
using Hamlet.App.Telemetry;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 337 task 0: **where an emitted PSK31 character stops before the screen.**
/// </summary>
/// <remarks>
/// <para>**A TRACE, NOT A CRITERION.** It plays a fixture through the real tap and tick and
/// prints, for each carrier, the most text its row carried on the whole table
/// (`DigitalDecodes`), on the list the panel binds (`DigitalVisibleDecodes`) and on the operator's
/// own side (`DigitalMineDecodes`), with the CQ filter off and on - then the two character
/// counters as the record writes them.</para>
/// <para>**THE DRIFT AND IDLE-GAP FIXTURES ARE THE AIR'S TWO DIFFERENCES** the instruction names -
/// AFC movement and a station holding the carrier between words - played the same way.</para>
/// <para>**COMPUTED, NOT SEEN**, and every fixture is synthetic (FACT-004, FACT-006).</para>
/// </remarks>
public sealed class Unit337RowTraceTests : IDisposable
{
    private const string OwnCall = "KC3QIS";

    private const double MatchHz = 25;

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where the numbers are printed.</param>
    public Unit337RowTraceTests(ITestOutputHelper output)
    {
        _output = output;

        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-unit337-trace-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_folder);
    }

    /// <summary>Removes the telemetry folder.</summary>
    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, true);
        }
        catch (IOException)
        {
            // A left-over temp folder is not a trace failure.
        }
    }

    /// <summary>The four-signal fixture, the CQ filter off and on, every list and both counters.</summary>
    [Fact]
    public void TraceTheFourSignalFixtureToTheBoundList()
    {
        Trace("psk31-four-signals.wav", cqOnly: false, 700, 1100, 1600, 2200);
        Trace("psk31-four-signals.wav", cqOnly: true, 700, 1100, 1600, 2200);
    }

    /// <summary>The drifting and the idling carrier, the CQ filter off and on.</summary>
    [Fact]
    public void TraceTheAirsDifferencesToTheBoundList()
    {
        Trace("psk31-drift-1000-to-1020hz.wav", cqOnly: false, 1010);
        Trace("psk31-drift-1000-to-1020hz.wav", cqOnly: true, 1010);
        Trace("psk31-idle-gap-1000hz.wav", cqOnly: false, 1000);
        Trace("psk31-idle-gap-1000hz.wav", cqOnly: true, 1000);
    }

    private void Trace(string fixture, bool cqOnly, params double[] carriers)
    {
        var most = new Dictionary<string, Dictionary<double, int>>
        {
            ["table"] = carriers.ToDictionary(hz => hz, _ => 0),
            ["visible"] = carriers.ToDictionary(hz => hz, _ => 0),
            ["mine"] = carriers.ToDictionary(hz => hz, _ => 0),
        };

        var dimmedTicks = 0;
        var hiddenWithTextTicks = 0;

        using (var telemetry = new JsonlTelemetry(_folder, "337", _ => true))
        {
            var model = Listening(telemetry, cqOnly);
            var audio = WavAudio.Read(Fixture(fixture));
            var chunk = audio.SampleRate / 4;

            for (var at = 0; at < audio.Samples.Length; at += chunk)
            {
                var count = Math.Min(chunk, audio.Samples.Length - at);

                model.TapForTests!.Take(audio.Samples.AsSpan(at, count), audio.SampleRate);
                model.LookForASlotForTests();

                Note(most["table"], carriers, model.DigitalDecodes);
                Note(most["visible"], carriers, model.DigitalVisibleDecodes);
                Note(most["mine"], carriers, model.DigitalMineDecodes);

                dimmedTicks += model.DigitalDecodes.Any(r => r.HeardNotReadable) ? 1 : 0;

                hiddenWithTextTicks += model.DigitalDecodes.Any(r => r.IsTextOnly
                    && !r.HeardNotReadable
                    && !model.DigitalVisibleDecodes.Contains(r)
                    && !model.DigitalMineDecodes.Contains(r))
                        ? 1
                        : 0;
            }

            // **LEAVING THE TAB CLOSES THE CARRIERS**, as ThePsk31TelemetryTests does.
            model.ChooseDigitalModeCommand.Execute("FT8");
        }

        var lines = Read();

        _output.WriteLine(fixture + ", CQ filter " + (cqOnly ? "ON" : "off"));

        foreach (var list in most)
        {
            _output.WriteLine("  " + list.Key.PadRight(8) + string.Join(", ", list.Value.Select(
                p => p.Key.ToString("0", CultureInfo.InvariantCulture) + " Hz " + p.Value))
                + "  = " + list.Value.Values.Sum());
        }

        _output.WriteLine("  ticks with a dimmed row         : " + dimmedTicks);
        _output.WriteLine("  ticks with a text row on no list: " + hiddenWithTextTicks);

        var retired = Events(lines, "psk31_carrier_retired");

        _output.WriteLine("  retires " + retired.Count + ": characters "
            + string.Join("+", retired.Select(e => Number(e, "charactersEmitted")))
            + " = " + retired.Sum(e => Number(e, "charactersEmitted"))
            + ", lines " + retired.Sum(e => Number(e, "linesParsed")));

        foreach (var stop in Events(lines, "psk31_listening_stopped"))
        {
            _output.WriteLine("  stopped: characters " + Number(stop, "charactersEmitted")
                + ", lines " + Number(stop, "linesParsed"));
        }

        _output.WriteLine("  line parsed events: " + Events(lines, "psk31_line_parsed").Count);
    }

    private static void Note(
        Dictionary<double, int> most, double[] carriers, IEnumerable<DigitalDecodeRow> rows)
    {
        foreach (var row in rows.Where(r => r.IsTextOnly && !r.HeardNotReadable))
        {
            if (!double.TryParse(row.Hz, NumberStyles.Float, CultureInfo.InvariantCulture, out var hz))
            {
                continue;
            }

            var carrier = carriers.FirstOrDefault(c => Math.Abs(c - hz) <= MatchHz, double.NaN);

            if (!double.IsNaN(carrier))
            {
                most[carrier] = Math.Max(most[carrier], row.Message.Length);
            }
        }
    }

    private static MainWindowViewModel Listening(JsonlTelemetry telemetry, bool cqOnly)
    {
        var settings = new AppSettings { ReconnectOnStartup = false, DecodedShowCq = cqOnly };

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

    private List<string> Read()
    {
        var files = Directory.GetFiles(_folder, "*.jsonl");
        var lines = files.SelectMany(File.ReadAllLines).ToList();

        foreach (var file in files)
        {
            File.Delete(file);
        }

        return lines;
    }

    private static List<JsonElement> Events(IEnumerable<string> lines, string name)
        => lines
            .Where(l => l.Contains("\"" + name + "\"", StringComparison.Ordinal))
            .Select(l => JsonDocument.Parse(l).RootElement)
            .Where(e => e.GetProperty("event").GetString() == name)
            .ToList();

    private static double Number(JsonElement e, string field)
        => e.GetProperty("data").TryGetProperty(field, out var value)
            && value.ValueKind == JsonValueKind.Number
                ? value.GetDouble()
                : 0;

    private static string Fixture(string file)
        => Path.Combine(Root(), "assets", "fixtures", file);

    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }
}
