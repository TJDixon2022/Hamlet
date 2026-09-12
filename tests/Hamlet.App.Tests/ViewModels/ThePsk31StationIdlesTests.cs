using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Hamlet.App.Settings;
using Hamlet.App.Telemetry;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 327 task 1: **the panel keeps the row, and the record says why.**
/// </summary>
/// <remarks>
/// <para>**THE ENGINE'S HALF OF THIS IS PROVED IN `ThePsk31StationIdlesTests` IN THE ENGINE
/// PROJECT.** This is the other end of the same fault: the operator was not reading a
/// carrier list, he was reading **rows on a screen and lines in a file**, and on 2026-09-12
/// the file gave him 206 seconds, 12 carriers and 0 lines with nothing in it to say that
/// the station at 2073 Hz was sitting there with his squelch open waiting for an answer.
/// </para>
/// <para>**COMPUTED, NOT SEEN** (FACT-004, FACT-006). Every claim here comes from a
/// synthetic fixture played through the real tap; no radio was involved and nothing was
/// looked at on a screen.</para>
/// </remarks>
public sealed class ThePsk31StationIdlesTests : IDisposable
{
    private const string OwnCall = "KC3QIS";

    /// <summary>The fixture this unit made: type ten, idle eight, type ten.</summary>
    private const string IdleEight = "psk31-idle-8s-1000hz.wav";

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the record is printed.</param>
    public ThePsk31StationIdlesTests(ITestOutputHelper output)
    {
        _output = output;

        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-psk31-idles-" + Guid.NewGuid().ToString("N"));

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
            // A left-over temp folder is not a test failure.
        }
    }

    /// <summary>
    /// **Assertion 1: one row for the whole file, and one retirement, at the end.**
    /// </summary>
    /// <remarks>
    /// **THE ROW IS THE THING THE OPERATOR HAS.** Six appearances and six retirements in
    /// two minutes is what his own record showed, and it is the same station each time; one
    /// appearance and one retirement is what a station who paused should leave behind.
    /// </remarks>
    [Fact]
    public void TheIdlingStationIsOneRowAndOneRetirement()
    {
        var lines = Play(IdleEight);

        var appeared = Events(lines, "psk31_carrier_appeared");
        var retired = Events(lines, "psk31_carrier_retired");

        foreach (var one in retired)
        {
            _output.WriteLine(
                "retired at " + Hz(one) + " Hz, " + Text(one, "reason")
                + ", lifetime " + Number(one, "lifetimeSeconds").ToString(
                    "0.0", CultureInfo.InvariantCulture)
                + " s, " + Number(one, "charactersEmitted") + " characters");
        }

        Assert.Single(appeared);
        Assert.Single(retired);

        Assert.Equal(nameof(Psk31Retirement.SignalGone), Text(retired[0], "reason"));

        // **AND IT READ BOTH HALVES**, so the same channel crossed the gap rather than two
        // that happened to land on the same frequency.
        Assert.True(
            Number(retired[0], "charactersEmitted") >= 70,
            "only " + Number(retired[0], "charactersEmitted") + " characters");
    }

    /// <summary>
    /// **Assertion 2: the record brackets the gap with an idling and a typing event.**
    /// </summary>
    /// <remarks>
    /// **AND IT NAMES THE REASON THE OPERATOR WAS LOOKING FOR.** *No characters* is what a
    /// station who left and a station who is thinking both write into a decode log. These
    /// two events are the difference, in his own file, after the fact.
    /// </remarks>
    [Fact]
    public void TheRecordSaysHeWentIdleAndThenStartedTypingAgain()
    {
        var lines = Play(IdleEight);

        var idling = Events(lines, "psk31_carrier_idling");
        var typing = Events(lines, "psk31_carrier_typing");

        foreach (var one in idling.Concat(typing))
        {
            _output.WriteLine(
                one.GetProperty("event").GetString()
                + " at " + Hz(one) + " Hz, quality "
                + Number(one, "quality").ToString("0.000", CultureInfo.InvariantCulture)
                + ", after " + Number(one, "afterSeconds").ToString(
                    "0.0", CultureInfo.InvariantCulture) + " s");
        }

        Assert.Single(idling);
        Assert.Single(typing);

        // **HIS KEYING WAS CLEAN THE WHOLE TIME HE SAID NOTHING**, which is the fact that
        // makes retiring him wrong.
        Assert.True(
            Number(idling[0], "quality") >= Psk31CarrierSearch.KeepReadableQuality,
            "he idled at quality " + Number(idling[0], "quality"));

        // **AND THE TYPING EVENT CARRIES THE LENGTH OF THE GAP IT ENDED**, which is the
        // eight seconds the fixture was made with, less the two it takes to call it idle.
        Assert.InRange(Number(typing[0], "afterSeconds"), 3.0, 9.0);
    }

    /// <summary>
    /// **Assertion 3: nothing personal is in either new event.**
    /// </summary>
    /// <remarks>
    /// **HM-DEC-018 AND §2.1, AND THE SCAN IS OVER EVERY BYTE OF EVERY LINE.** The
    /// fixture's own callsign is what would show up, and the station was typing it at the
    /// moment the typing event was written.
    /// </remarks>
    [Fact]
    public void NeitherNewEventCarriesACallsignOrAWord()
    {
        var lines = Play(IdleEight);

        foreach (var one in Events(lines, "psk31_carrier_idling")
            .Concat(Events(lines, "psk31_carrier_typing")))
        {
            var data = one.GetProperty("data");

            foreach (var field in data.EnumerateObject())
            {
                Assert.True(
                    field.Value.ValueKind is JsonValueKind.Number or JsonValueKind.True
                        or JsonValueKind.False,
                    "the field " + field.Name + " is a " + field.Value.ValueKind);
            }
        }

        foreach (var word in new[] { "KC3QIS", "FN00DJ", "CQ CQ", "pse" })
        {
            var found = lines
                .Where(l => l.Contains("psk31_carrier_idling", StringComparison.Ordinal)
                    || l.Contains("psk31_carrier_typing", StringComparison.Ordinal))
                .FirstOrDefault(l => Regex.IsMatch(
                    l, @"\b" + Regex.Escape(word) + @"\b", RegexOptions.IgnoreCase));

            Assert.True(found is null, word + " is in the record: " + found);
        }
    }

    /// <summary>
    /// **Assertion 4: the row does not flicker while he is idling.**
    /// </summary>
    /// <remarks>
    /// **THIS IS THE OPERATOR'S OWN COMPLAINT IN ONE NUMBER.** The panel is ticked the way
    /// the app ticks it, and the count of rows is taken at every tick: one row, from the
    /// first words to the last, with no tick in between where the list was empty.
    /// </remarks>
    [Fact]
    public void TheRowIsNeverEmptyBetweenTheFirstWordsAndTheLast()
    {
        HashMatches(IdleEight, "manifest-step2.json");

        var audio = WavAudio.Read(Fixture(IdleEight));
        var model = Listening(null);
        var chunk = audio.SampleRate / 4;
        var counts = new List<int>();
        var seconds = new List<double>();

        for (var at = 0; at < audio.Samples.Length; at += chunk)
        {
            model.TapForTests!.Take(
                audio.Samples.AsSpan(at, Math.Min(chunk, audio.Samples.Length - at)),
                audio.SampleRate);

            model.LookForASlotForTests();

            counts.Add(model.DigitalDecodes.Count);
            seconds.Add((double)(at + chunk) / audio.SampleRate);
        }

        var first = counts.FindIndex(c => c > 0);
        var last = counts.FindLastIndex(c => c > 0);

        _output.WriteLine(
            "a row from " + seconds[first].ToString("0.0", CultureInfo.InvariantCulture)
            + " s to " + seconds[last].ToString("0.0", CultureInfo.InvariantCulture)
            + " s, most rows at once " + counts.Max());

        Assert.True(first >= 0, "no row was ever put up");

        // **NOT ONE EMPTY TICK IN BETWEEN**, which is what *stays on the list* means.
        var gap = counts.GetRange(first, last - first + 1).FindIndex(c => c == 0);

        Assert.True(
            gap < 0,
            "the list was empty at " + seconds[first + Math.Max(gap, 0)].ToString(
                "0.0", CultureInfo.InvariantCulture) + " s");

        // **AND NEVER TWO ROWS**, which is what a retire-and-rediscover looks like once the
        // second id is on the screen beside the first.
        Assert.Equal(1, counts.Max());
    }

    /// <summary>Play a fixture through the real tap and tick with the record running.</summary>
    private List<string> Play(string fixture)
    {
        HashMatches(fixture, "manifest-step2.json");

        var audio = WavAudio.Read(Fixture(fixture));

        using (var telemetry = new JsonlTelemetry(_folder, "327", _ => true))
        {
            var model = Listening(telemetry);
            var chunk = audio.SampleRate / 4;

            for (var at = 0; at < audio.Samples.Length; at += chunk)
            {
                model.TapForTests!.Take(
                    audio.Samples.AsSpan(at, Math.Min(chunk, audio.Samples.Length - at)),
                    audio.SampleRate);

                model.LookForASlotForTests();
            }

            // **LEAVING THE TAB IS WHAT CLOSES A CARRIER THAT IS STILL THERE**, which is how
            // every appearance is accounted for inside a test that cannot wait.
            model.ChooseDigitalModeCommand.Execute("FT8");
        }

        return Directory.GetFiles(_folder, "*.jsonl").SelectMany(File.ReadAllLines).ToList();
    }

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

    private static List<JsonElement> Events(IEnumerable<string> lines, string name)
        => lines
            .Where(l => l.Contains("\"" + name + "\"", StringComparison.Ordinal))
            .Select(l => JsonDocument.Parse(l).RootElement)
            .Where(e => e.GetProperty("event").GetString() == name)
            .ToList();

    private static double Number(JsonElement e, string field)
    {
        var data = e.GetProperty("data");

        return data.TryGetProperty(field, out var value)
            && value.ValueKind == JsonValueKind.Number
                ? value.GetDouble()
                : 0;
    }

    private static string Text(JsonElement e, string field)
    {
        var data = e.GetProperty("data");

        return data.TryGetProperty(field, out var value)
            && value.ValueKind == JsonValueKind.String
                ? value.GetString() ?? ""
                : "";
    }

    private static string Hz(JsonElement e)
        => Number(e, "offsetHz").ToString("0.0", CultureInfo.InvariantCulture);

    private static void HashMatches(string file, string manifest)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Fixture(manifest)));

        var want = document.RootElement.EnumerateArray()
            .Single(e => e.GetProperty("file").GetString() == file)
            .GetProperty("sha256").GetString();

        using var stream = File.OpenRead(Fixture(file));

        Assert.Equal(want, Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant());
    }

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
