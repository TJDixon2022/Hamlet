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
/// Work instruction 355 task 2: **a PSK31 station's words stay on the list after he stops.**
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-14:** *I've seen a few PSK31 phrases. In the past, they disappear. There's
/// no record of them.* True: a PSK31 row belonged to its carrier and was retired with it, text and
/// all (unit 324's retire rule), while an FT8 message stays until cleared. **An ended row now lives
/// by the FT8 rule**: it stays, marked ended in a word and in its state, at the worked fade, in
/// arrival order with the FT8 rows, until the clear or the 500-row cap removes it.</para>
/// <para>**A ROW THAT NEVER READ A CHARACTER STILL GOES WITH ITS CARRIER** - the unit's choice.
/// *heard, not readable yet* is Hamlet's sentence, not his words, and kept it would be a way for a
/// station that has gone to stay on the list for ever; <c>ThePsk31CarrierLivesTests.AHeardRowGoesWhenItsCarrierGoes</c>
/// guards it, unedited.</para>
/// <para>**COMPUTED, NOT SEEN** (FACT-004, FACT-006). A synthetic fixture through the real tap, and
/// channels handed to the panel's own path; no radio, no window.</para>
/// </remarks>
public sealed class ThePsk31RowStaysTests : IDisposable
{
    private const string OwnCall = "KC3QIS";

    private const double MatchHz = 20;

    private const string FourSignals = "psk31-four-signals.wav";

    private static readonly double[] FourCarriers = [700, 1100, 1600, 2200];

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the rows and the record are printed.</param>
    public ThePsk31RowStaysTests(ITestOutputHelper output)
    {
        _output = output;
        _folder = Path.Combine(Path.GetTempPath(), "hamlet-psk31-row-stays-" + Guid.NewGuid().ToString("N"));

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
    /// **The four-signal fixture leaves four ended rows with all their text once every carrier is
    /// gone, and they are still there after he leaves the tab.**
    /// </summary>
    [Fact]
    public void TheFourSignalFixtureLeavesFourEndedRowsWithTheirText()
    {
        var longest = FourCarriers.ToDictionary(c => c, _ => "");
        List<DigitalDecodeRow> ended;
        List<DigitalDecodeRow> afterLeaving;
        double quietSeconds;

        using (var telemetry = new JsonlTelemetry(_folder, "355", _ => true))
        {
            var model = Listening(telemetry);
            var audio = WavAudio.Read(Fixture(FourSignals));
            var chunk = audio.SampleRate / 4;

            for (var at = 0; at < audio.Samples.Length; at += chunk)
            {
                model.TapForTests!.Take(audio.Samples.AsSpan(at, Math.Min(chunk, audio.Samples.Length - at)), audio.SampleRate);
                model.LookForASlotForTests();
                Longest(model, longest);
            }

            // **THEN THE BAND GOES QUIET**, a quarter second at a time, until no PSK31 row has a
            // carrier under it or twenty seconds have passed. A faint fixed noise, not silence, so the
            // search has a floor to measure against.
            var noise = new Random(355);
            var quiet = new float[chunk];

            quietSeconds = 0;

            while (quietSeconds < 20 && model.DigitalDecodes.Any(r => r.IsTextOnly && !r.Ended && !r.HeardNotReadable))
            {
                for (var i = 0; i < quiet.Length; i++)
                {
                    quiet[i] = (float)((noise.NextDouble() - 0.5) * 2e-4);
                }

                model.TapForTests!.Take(quiet, audio.SampleRate);
                model.LookForASlotForTests();
                Longest(model, longest);
                quietSeconds += 0.25;
            }

            // **THE ROWS WHOSE CARRIERS WENT ON THE TAB**, by the search's own retire rule.
            afterLeaving = model.DigitalDecodes.Where(r => r.IsTextOnly && r.Ended).ToList();

            _output.WriteLine("quiet for " + quietSeconds.ToString("0.00", CultureInfo.InvariantCulture) + " s; ended on the tab "
                + afterLeaving.Count + " of " + model.DigitalDecodes.Count(r => r.IsTextOnly) + " (" + string.Join(", ", afterLeaving.Select(r => r.Hz + " Hz"))
                + "); longest text per carrier " + string.Join(", ", longest.Select(p => p.Key + " Hz " + p.Value.Length)));

            // **LEAVING THE TAB RETIRES WHAT THE SEARCH STILL HOLDS** (`ListeningStopped`), which is how
            // every carrier is gone inside a test that cannot wait. Measured here: after twenty seconds
            // of faint noise the search still holds one carrier by its keep-readable rule, which this
            // unit does not touch. **And leaving is not a clear**: the rows that ended on the tab are
            // the same rows after it.
            model.ChooseDigitalModeCommand.Execute("FT8");

            ended = model.DigitalDecodes.Where(r => r.IsTextOnly).ToList();

            foreach (var row in ended)
            {
                _output.WriteLine(row.Hz + " Hz [" + row.EndedWord + "] opacity " + row.RowOpacity + ", " + row.Message.Length
                    + " characters, shown " + Shown(model, row));
            }

            Assert.Equal(4, ended.Count);

            foreach (var row in ended)
            {
                var carrier = CarrierOf(row);

                Assert.True(row.Ended, row.Hz + " Hz is not marked ended");
                Assert.Equal("ended", row.EndedWord);
                Assert.Equal(0.55, row.RowOpacity);
                Assert.Equal(longest[carrier], row.Message);
                Assert.True(Shown(model, row), row.Hz + " Hz is not on a list the panel binds");
            }
        }

        Assert.NotEmpty(afterLeaving);
        Assert.All(afterLeaving, r => Assert.Contains(ended, a => ReferenceEquals(a, r)));

        var events = Events("psk31_row_ended");

        foreach (var one in events)
        {
            _output.WriteLine("psk31_row_ended " + one.GetProperty("data").GetRawText());
        }

        Assert.Equal(4, events.Count);
        Assert.Equal(ended.Sum(r => r.Message.Length), events.Sum(e => (int)Number(e, "characters")));
        Assert.All(events, e => Assert.True(Number(e, "lifetimeSeconds") > 0, "a row ended with no lifetime"));
        Assert.All(events, e => Assert.Contains(FourCarriers, c => Math.Abs(c - Number(e, "offsetHz")) <= MatchHz));
    }

    /// <summary>**The clear removes ended rows, and the record says it did.**</summary>
    [Fact]
    public void TheClearRemovesEndedRowsAndSaysSo()
    {
        using (var telemetry = new JsonlTelemetry(_folder, "355", _ => true))
        {
            var model = Listening(telemetry);

            model.ShowPsk31ChannelsForTests(
            [
                new Psk31Channel(1, 900, 12, "CQ CQ de W1AW W1AW pse k\n", Readable: true),
                new Psk31Channel(2, 1500, 9, "KC3QIS de EI4GNB ur 599 btu\n", Readable: true),
            ]);
            model.ShowPsk31ChannelsForTests([]);

            Assert.Equal(2, model.DigitalDecodes.Count(r => r.IsTextOnly && r.Ended));

            model.ClearDigitalDecodesCommand.Execute(null);

            _output.WriteLine("after the clear: " + model.DigitalDecodes.Count + " rows");

            Assert.Empty(model.DigitalDecodes);
        }

        var cleared = Events("psk31_row_cleared");

        foreach (var one in cleared)
        {
            _output.WriteLine("psk31_row_cleared " + one.GetProperty("data").GetRawText());
        }

        Assert.Equal(2, cleared.Count);
        Assert.All(cleared, e => Assert.Equal("clear", Text(e, "reason")));
    }

    /// <summary>**A station who comes back at his offset inside the window resumes his row; one elsewhere is a new row.**</summary>
    [Fact]
    public void AStationReturningAtHisOffsetResumesHisRow()
    {
        var model = Listening(null);
        const string Before = "CQ CQ de W1AW W1AW pse k\n";
        const string After = "CQ CQ de W1AW W1AW pse k\n";

        model.ShowPsk31ChannelsForTests([new Psk31Channel(1, 1085, 12, Before, Readable: true)]);
        model.ShowPsk31ChannelsForTests([]);

        var kept = Assert.Single(model.DigitalDecodes);

        Assert.True(kept.Ended);

        // **HE COMES BACK A FEW HERTZ AWAY**, on a new channel, as the search lists a carrier it finds again.
        model.ShowPsk31ChannelsForTests([new Psk31Channel(2, 1085 + MainWindowViewModel.Psk31ResumeWithinHz / 2, 12, After, Readable: true)]);

        var resumed = Assert.Single(model.DigitalDecodes);

        _output.WriteLine("resumed: " + resumed.Hz + " Hz [" + resumed.EndedWord + "] '" + resumed.Message.Replace("\n", "|", StringComparison.Ordinal) + "'");

        Assert.False(resumed.Ended);
        Assert.Equal(Before + After, resumed.Message);
        Assert.Equal(1.0, resumed.RowOpacity);

        // **SOMEBODY ELSE, WELL AWAY FROM HIM, IS SOMEBODY ELSE.**
        model.ShowPsk31ChannelsForTests(
        [
            new Psk31Channel(2, 1085 + MainWindowViewModel.Psk31ResumeWithinHz / 2, 12, After, Readable: true),
            new Psk31Channel(3, 1600, 12, "CQ de EI4GNB k\n", Readable: true),
        ]);

        Assert.Equal(2, model.DigitalDecodes.Count(r => r.IsTextOnly));

        _output.WriteLine("resume window: " + MainWindowViewModel.Psk31ResumeWithinHz + " Hz, "
            + MainWindowViewModel.Psk31ResumeWithinSeconds + " s of audio");
    }

    /// <summary>**The 500-row cap drops an ended row the way it drops an FT8 row, and the order button keeps it.**</summary>
    [Fact]
    public void TheCapAppliesToEndedRowsAsToFt8Rows()
    {
        using (var telemetry = new JsonlTelemetry(_folder, "355", _ => true))
        {
            var model = Listening(telemetry);

            model.ShowPsk31ChannelsForTests([new Psk31Channel(1, 900, 12, "CQ CQ de W1AW W1AW pse k\n", Readable: true)]);
            model.ShowPsk31ChannelsForTests([]);
            model.ChooseDigitalModeCommand.Execute("FT8");

            var kept = Assert.Single(model.DigitalDecodes);

            // **THE ORDER BUTTON REBUILDS THE LIST FROM THE ARRIVALS**, so an ended row not among them would vanish here.
            model.ToggleDigitalOrderCommand.Execute(null);
            model.ToggleDigitalOrderCommand.Execute(null);

            Assert.Contains(model.DigitalDecodes, r => ReferenceEquals(r, kept));

            var opened = new DateTime(2026, 9, 14, 12, 0, 0, DateTimeKind.Utc);

            for (var i = 0; i < MainWindowViewModel.MaxDigitalDecodes; i++)
            {
                model.NoteSlot(new Ft8Reception(
                    [new Ft8Decode(opened.AddSeconds(15 * i), 1.4, 1240, 12, $"CQ K{i}ABC FN42")], 1, 1, ""));
            }

            _output.WriteLine("rows " + model.DigitalDecodes.Count + ", PSK31 rows " + model.DigitalDecodes.Count(r => r.IsTextOnly)
                + ", summary " + model.DigitalDecodedSummary);

            Assert.Equal(MainWindowViewModel.MaxDigitalDecodes, model.DigitalDecodes.Count);
            Assert.DoesNotContain(model.DigitalDecodes, r => ReferenceEquals(r, kept));
            Assert.Contains("oldest 1 dropped", model.DigitalDecodedSummary, StringComparison.Ordinal);
        }

        var cleared = Events("psk31_row_cleared");

        Assert.Single(cleared);
        Assert.Equal("cap", Text(cleared[0], "reason"));
    }

    private static void Longest(MainWindowViewModel model, Dictionary<double, string> longest)
    {
        foreach (var row in model.DigitalDecodes.Where(r => r.IsTextOnly && !r.HeardNotReadable))
        {
            var carrier = CarrierOf(row);

            if (!double.IsNaN(carrier) && row.Message.Length > longest[carrier].Length)
            {
                longest[carrier] = row.Message;
            }
        }
    }

    private static double CarrierOf(DigitalDecodeRow row)
        => double.TryParse(row.Hz, NumberStyles.Float, CultureInfo.InvariantCulture, out var hz)
            ? FourCarriers.FirstOrDefault(c => Math.Abs(c - hz) <= MatchHz, double.NaN)
            : double.NaN;

    private static bool Shown(MainWindowViewModel model, DigitalDecodeRow row)
        => model.DigitalVisibleDecodes.Any(r => ReferenceEquals(r, row))
            || model.DigitalMineDecodes.Any(r => ReferenceEquals(r, row));

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
            DigitalNewestFirst = false,
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());
        model.ChooseDigitalModeCommand.Execute("PSK31");

        return model;
    }

    private List<JsonElement> Events(string name)
        => Directory.GetFiles(_folder, "*.jsonl")
            .SelectMany(File.ReadAllLines)
            .Where(l => l.Contains("\"" + name + "\"", StringComparison.Ordinal))
            .Select(l => JsonDocument.Parse(l).RootElement)
            .Where(e => e.GetProperty("event").GetString() == name)
            .ToList();

    private static double Number(JsonElement e, string field)
        => e.GetProperty("data").TryGetProperty(field, out var value) && value.ValueKind == JsonValueKind.Number ? value.GetDouble() : 0;

    private static string Text(JsonElement e, string field)
        => e.GetProperty("data").TryGetProperty(field, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() ?? "" : "";

    private static string Fixture(string file)
        => Path.Combine(Root(), "assets", "fixtures", file);

    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }
}
