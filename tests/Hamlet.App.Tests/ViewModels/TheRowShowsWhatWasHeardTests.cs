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
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 337 task 1: **the row shows what the ear heard.**
/// </summary>
/// <remarks>
/// <para>**WHAT IT EXISTS FOR, FROM THE RECORD.** On 2026-09-12 at 14.070 carrier 19 retired
/// with 262 characters emitted, the session stopped saying 0, and the screen showed nothing.
/// Unit 337 task 0 traced it: the CQ filter held every PSK31 row without a parsed call to
/// anyone off the list the panel binds, and the stopped event summed only the carriers still
/// alive when the tab was left.</para>
/// <para>**THE SQUELCH IS THE ONLY GATE** (PSK31 plan R9, §0.0). A character the demodulator is
/// sure of is on a row the operator can see, whatever the CQ toggle says and whatever the
/// character is; the toggle is a question about FT8's to-field, and a PSK31 row has none until
/// a turnover has been read.</para>
/// <para>**SHOWN MEANS ON A LIST THE PANEL BINDS** - `DigitalVisibleDecodes` or
/// `DigitalMineDecodes` - and not merely on `DigitalDecodes`, which is where every earlier
/// PSK31 test looked and why none of them saw this.</para>
/// <para>**COMPUTED, NOT SEEN**, and every fixture is synthetic (FACT-004, FACT-006).</para>
/// </remarks>
public sealed class TheRowShowsWhatWasHeardTests : IDisposable
{
    private const string OwnCall = "KC3QIS";

    private const double MatchHz = 20;

    private const string FourSignals = "psk31-four-signals.wav";

    private static readonly double[] FourCarriers = [700, 1100, 1600, 2200];

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the numbers are printed.</param>
    public TheRowShowsWhatWasHeardTests(ITestOutputHelper output)
    {
        _output = output;

        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-row-shows-" + Guid.NewGuid().ToString("N"));

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

    /// <summary>**The four-signal fixture puts its 483 characters on four shown rows, and the session total says 483.**</summary>
    /// <param name="cqOnly">Whether the CQ toggle is on, as the operator may have left it.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TheFourSignalFixturePutsEveryCharacterOnAShownRow(bool cqOnly)
    {
        var played = Play(FourSignals, cqOnly, FourCarriers);

        var want = FixtureCharacters();
        var shown = played.MostShown.Values.Sum();
        var stopped = played.Stopped.Single();

        _output.WriteLine("CQ toggle " + (cqOnly ? "on" : "off") + ": the fixture's texts " + want
            + ", shown " + shown + " (" + string.Join(", ", played.MostShown.Select(
                p => p.Key.ToString("0", CultureInfo.InvariantCulture) + " Hz " + p.Value)) + ")"
            + ", ticks a text row was on no shown list " + played.TicksHidden
            + ", session total " + stopped.Characters);

        Assert.Equal(483, want);
        Assert.All(played.MostShown.Values, n => Assert.True(n > 0, "a carrier's row was never shown"));
        Assert.Equal(0, played.TicksHidden);
        Assert.Equal(want, shown);
        Assert.Equal(want, stopped.Characters);
    }

    /// <summary>**A carrier that emits a character while its row is dimmed lifts on that character.**</summary>
    /// <param name="cqOnly">Whether the CQ toggle is on.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ARowThatWasDimmedLiftsOnItsFirstCharacter(bool cqOnly)
    {
        var model = Listening(null, cqOnly);

        // **HEARD, SQUELCH SHUT, NOTHING READ** - the dimmed form, and on the screen.
        model.ShowPsk31ChannelsForTests([new Psk31Channel(19, 1085.4, 12, "", Readable: false)]);

        var dimmed = Assert.Single(model.DigitalDecodes);

        _output.WriteLine("before: '" + dimmed.Message + "', dimmed " + dimmed.HeardNotReadable
            + ", shown " + Shown(model, dimmed));

        Assert.True(dimmed.HeardNotReadable);
        Assert.True(Shown(model, dimmed), "the dimmed row was not on a list the panel binds");

        // **THE SQUELCH OPENS AND ONE CHARACTER ARRIVES.**
        model.ShowPsk31ChannelsForTests([new Psk31Channel(19, 1085.4, 12, "e", Readable: true)]);

        var lifted = Assert.Single(model.DigitalDecodes);

        _output.WriteLine("after : '" + lifted.Message + "', dimmed " + lifted.HeardNotReadable
            + ", shown " + Shown(model, lifted));

        Assert.False(lifted.HeardNotReadable);
        Assert.Equal("e", lifted.Message);
        Assert.True(Shown(model, lifted), "the lifted row was not on a list the panel binds");

        // **AND IT STAYS LIFTED WHEN THE SQUELCH SHUTS AGAIN**, with what it read still on it.
        model.ShowPsk31ChannelsForTests([new Psk31Channel(19, 1085.4, 12, "e", Readable: false)]);

        var after = Assert.Single(model.DigitalDecodes);

        Assert.False(after.HeardNotReadable);
        Assert.Equal("e", after.Message);
        Assert.True(Shown(model, after), "the row went off the list when the squelch shut");
    }

    /// <summary>**The text the parser splits is the text the row shows.**</summary>
    /// <remarks>
    /// **ASKED OF EVERY ROW ON EVERY TICK.** A fresh splitter run over the row's own words must
    /// come to the reading the row carries - the same latest message, or none where none has
    /// finished. A reading taken from any other copy of the text would disagree somewhere.
    /// </remarks>
    [Fact]
    public void TheParserReadsTheTextTheRowShows()
    {
        var played = Play(FourSignals, cqOnly: true, FourCarriers);

        _output.WriteLine("rows checked " + played.RowsChecked + ", readings " + played.Readings
            + ", disagreements " + played.Disagreements.Count);

        foreach (var disagreement in played.Disagreements.Take(5))
        {
            _output.WriteLine("  " + disagreement);
        }

        Assert.True(played.Readings > 0, "no row ever carried a reading");
        Assert.Empty(played.Disagreements);
    }

    /// <summary>**`psk31_listening_stopped` totals are the sum of the retires.**</summary>
    /// <remarks>
    /// **ONE SOURCE.** On this fixture the 1100 Hz carrier retires before the tab is left, so a
    /// total summed over the carriers still held when it stops leaves its characters out.
    /// </remarks>
    [Fact]
    public void TheStoppedTotalsAreTheSumOfTheRetires()
    {
        var played = Play(FourSignals, cqOnly: false, FourCarriers);

        var stopped = played.Stopped.Single();
        var characters = played.Retired.Sum(r => r.Characters);
        var lines = played.Retired.Sum(r => r.Lines);

        _output.WriteLine("retires " + played.Retired.Count + ": characters "
            + string.Join("+", played.Retired.Select(r => r.Characters)) + " = " + characters
            + ", lines " + lines + "; stopped: characters " + stopped.Characters
            + ", lines " + stopped.Lines + "; line parsed events " + played.LinesParsedEvents);

        Assert.Equal(characters, stopped.Characters);
        Assert.Equal(lines, stopped.Lines);
        Assert.Equal(played.LinesParsedEvents, stopped.Lines);
    }

    private sealed record Count(int Characters, int Lines);

    private sealed class Played
    {
        public Dictionary<double, int> MostShown { get; } = new();

        public int TicksHidden { get; set; }

        public int RowsChecked { get; set; }

        public int Readings { get; set; }

        public List<string> Disagreements { get; } = new();

        public List<Count> Retired { get; } = new();

        public List<Count> Stopped { get; } = new();

        public int LinesParsedEvents { get; set; }
    }

    /// <summary>Play a fixture through the real tap and tick, watch the shown lists, and leave the tab.</summary>
    private Played Play(string fixture, bool cqOnly, double[] carriers)
    {
        var played = new Played();

        foreach (var hz in carriers)
        {
            played.MostShown[hz] = 0;
        }

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

                Watch(model, played, carriers);
            }

            // **LEAVING THE TAB CLOSES THE CARRIERS STILL HELD.**
            model.ChooseDigitalModeCommand.Execute("FT8");
        }

        var lines = Directory.GetFiles(_folder, "*.jsonl").SelectMany(File.ReadAllLines).ToList();

        played.Retired.AddRange(Events(lines, "psk31_carrier_retired")
            .Select(e => new Count(Number(e, "charactersEmitted"), Number(e, "linesParsed"))));
        played.Stopped.AddRange(Events(lines, "psk31_listening_stopped")
            .Select(e => new Count(Number(e, "charactersEmitted"), Number(e, "linesParsed"))));
        played.LinesParsedEvents = Events(lines, "psk31_line_parsed").Count;

        return played;
    }

    private static void Watch(MainWindowViewModel model, Played played, double[] carriers)
    {
        var hidden = false;

        foreach (var row in model.DigitalDecodes.Where(r => r.IsTextOnly))
        {
            var shown = Shown(model, row);

            if (!row.HeardNotReadable)
            {
                hidden |= !shown;

                if (shown
                    && double.TryParse(row.Hz, NumberStyles.Float, CultureInfo.InvariantCulture, out var hz)
                    && carriers.FirstOrDefault(c => Math.Abs(c - hz) <= MatchHz, double.NaN) is var carrier
                    && !double.IsNaN(carrier))
                {
                    played.MostShown[carrier] = Math.Max(played.MostShown[carrier], row.Message.Length);
                }
            }

            // **THE PARSER'S TEXT AGAINST THE ROW'S TEXT.**
            played.RowsChecked++;

            var splitter = new Psk31MessageSplitter(OwnCall);
            Psk31Message? latest = null;

            foreach (var character in row.HeardNotReadable ? "" : row.Message)
            {
                latest = splitter.Add(character) ?? latest;
            }

            if (row.Reading is not null)
            {
                played.Readings++;
            }

            if (latest?.Exchange != row.Reading)
            {
                played.Disagreements.Add(row.Hz + " Hz: the row's words read "
                    + (latest?.Exchange.ToString() ?? "nothing") + ", the row carries "
                    + (row.Reading?.ToString() ?? "nothing"));
            }
        }

        played.TicksHidden += hidden ? 1 : 0;
    }

    private static bool Shown(MainWindowViewModel model, DigitalDecodeRow row)
        => model.DigitalVisibleDecodes.Any(r => ReferenceEquals(r, row))
            || model.DigitalMineDecodes.Any(r => ReferenceEquals(r, row));

    private static MainWindowViewModel Listening(JsonlTelemetry? telemetry, bool cqOnly)
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

    /// <summary>The four texts the fixture was made from, in characters.</summary>
    private static int FixtureCharacters()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Fixture("manifest-step2.json")));

        return document.RootElement.EnumerateArray()
            .Single(e => e.GetProperty("file").GetString() == FourSignals)
            .GetProperty("texts").EnumerateObject()
            .Sum(t => (t.Value.GetString() ?? "").Length);
    }

    private static List<JsonElement> Events(IEnumerable<string> lines, string name)
        => lines
            .Where(l => l.Contains("\"" + name + "\"", StringComparison.Ordinal))
            .Select(l => JsonDocument.Parse(l).RootElement)
            .Where(e => e.GetProperty("event").GetString() == name)
            .ToList();

    private static int Number(JsonElement e, string field)
        => e.GetProperty("data").TryGetProperty(field, out var value)
            && value.ValueKind == JsonValueKind.Number
                ? value.GetInt32()
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
