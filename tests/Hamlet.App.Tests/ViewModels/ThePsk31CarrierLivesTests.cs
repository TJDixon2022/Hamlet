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
/// Work instruction 324 task 3: **a carrier lives while the signal does.**
/// </summary>
/// <remarks>
/// <para>**THE FAULT THIS EXISTS FOR WAS MEASURED ON THE AIR.** On 2026-09-11 the
/// operator sat on 14.070 and a real, strong carrier at 893 Hz appeared three times in a
/// minute. **Each time it was retired after 1.9 s, reason `LostLock`, having emitted no
/// characters at all.** His words: *"PSK is showing something in decoded but for only
/// 2ish seconds then it all disappears."*</para>
/// <para>**AND THE ARITHMETIC SAYS NO CARRIER THAT SHORT CAN EVER SPEAK.** The
/// demodulator's squelch measures <see cref="Psk31Demodulator.QualityWindow"/> symbols -
/// 1.02 s - before it opens, and a character is held a further
/// <see cref="Psk31Listener.LetGoSeconds"/> until the search has vouched for the carrier
/// after it. That is about two seconds before a first character may be shown, and the
/// carrier was being killed at 1.9.</para>
/// <para>**SO THE RETIRE RULE IS NOW ABOUT THE SIGNAL AND NOT ABOUT THE TEXT.** A carrier
/// goes when the search stops finding it, on
/// <see cref="Psk31CarrierSearch.RetireAfterPasses"/> passes in a row. **Not producing
/// characters is a state of a live carrier**, shown on the panel in words and written to
/// the record as `psk31_reading`.</para>
/// <para>**COMPUTED, NOT SEEN.** Every fixture is synthetic and no radio was involved
/// (FACT-004, FACT-006).</para>
/// </remarks>
public sealed class ThePsk31CarrierLivesTests : IDisposable
{
    private const string OwnCall = "KC3QIS";

    /// <summary>The fixture unit 324 made: a station, six seconds of idle, the station again.</summary>
    private const string IdleGap = "psk31-idle-gap-1000hz.wav";

    /// <summary>How close a row must be to a fixture's carrier to be that carrier's row.</summary>
    private const double MatchHz = 20;

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the carriers and the record are printed.</param>
    public ThePsk31CarrierLivesTests(ITestOutputHelper output)
    {
        _output = output;

        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-psk31-lives-" + Guid.NewGuid().ToString("N"));

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
    /// **Assertion 1: six seconds of idle in the middle of a signal is one carrier.**
    /// </summary>
    /// <remarks>
    /// **IDLE IS KEYING, NOT SILENCE.** The gap is 188 bits of continuous phase reversals -
    /// exactly what a PSK31 operator's transmitter does between words - and no character
    /// comes out of any of it. One carrier, one id, retired once, at the end, because the
    /// signal went and not because the text stopped.
    /// </remarks>
    [Fact]
    public void SixSecondsOfIdleInTheMiddleOfASignalKeepsOneCarrier()
    {
        var lines = Play(IdleGap);

        var appeared = Events(lines, "psk31_carrier_appeared");
        var retired = Events(lines, "psk31_carrier_retired");

        foreach (var one in appeared)
        {
            _output.WriteLine("appeared at " + Hz(one) + " Hz, id " + Number(one, "carrierId"));
        }

        foreach (var one in retired)
        {
            _output.WriteLine("retired at " + Hz(one) + " Hz, id " + Number(one, "carrierId")
                + ", " + Text(one, "reason")
                + ", lifetime " + Number(one, "lifetimeSeconds").ToString(
                    "0.0", CultureInfo.InvariantCulture)
                + " s, " + Number(one, "charactersEmitted") + " characters");
        }

        Assert.Single(appeared);
        Assert.Single(retired);
        Assert.Equal(nameof(Psk31Retirement.SignalGone), Text(retired[0], "reason"));

        // **AND IT WAS THERE FOR THE WHOLE SIGNAL**, gap included. The file runs 29.3 s
        // and the carrier is keyed from the first idle bit to the last.
        Assert.True(
            Number(retired[0], "lifetimeSeconds") > 20,
            "lifetime only " + Number(retired[0], "lifetimeSeconds") + " s");

        // **AND IT READ BOTH HALVES**, so the gap was crossed by the same channel rather
        // than by two that happened to land on the same frequency.
        Assert.True(
            Number(retired[0], "charactersEmitted") >= 70,
            "only " + Number(retired[0], "charactersEmitted") + " characters");
    }

    /// <summary>
    /// **Assertion 2: the four-signal fixture retires four, each because the signal went.**
    /// </summary>
    /// <remarks>
    /// **NONE OF THEM SHORTER THAN ITS OWN SIGNAL.** Every station in that file sends for
    /// most of the file, so a carrier retired after a few seconds would be a carrier
    /// killed while its station was still transmitting - which is the fault this unit
    /// exists for.
    /// </remarks>
    [Fact]
    public void TheFourSignalFixtureRetiresFourCarriersBecauseTheSignalWent()
    {
        var lines = Play("psk31-four-signals.wav");

        var appeared = Events(lines, "psk31_carrier_appeared");
        var retired = Events(lines, "psk31_carrier_retired");

        foreach (var one in retired)
        {
            _output.WriteLine("retired at " + Hz(one) + " Hz, " + Text(one, "reason")
                + ", lifetime " + Number(one, "lifetimeSeconds").ToString(
                    "0.0", CultureInfo.InvariantCulture)
                + " s, " + Number(one, "charactersEmitted") + " characters");
        }

        Assert.Equal(4, appeared.Count);
        Assert.Equal(4, retired.Count);

        foreach (var one in retired)
        {
            var reason = Text(one, "reason");

            // **`ListeningStopped` IS ALLOWED AND `LostLock` IS NOT.** A carrier still on
            // the air when the audio runs out left because the listening did; nothing
            // leaves because it went quiet.
            Assert.True(
                reason == nameof(Psk31Retirement.SignalGone)
                || reason == nameof(Psk31Retirement.ListeningStopped),
                "retired for " + reason);

            Assert.True(
                Number(one, "lifetimeSeconds") >= 20,
                "a carrier lived only " + Number(one, "lifetimeSeconds") + " s");
        }

        Assert.Contains(
            retired, e => Text(e, "reason") == nameof(Psk31Retirement.SignalGone));
    }

    /// <summary>
    /// **Assertion 3: a held carrier with the squelch shut is a dimmed row that says so.**
    /// </summary>
    /// <remarks>
    /// <para>**AUTHOR'S CHOICE, MARKED AS ONE** (the work instruction says so, and the
    /// owner may overrule it). It exists so that an empty list and a band full of signals
    /// Hamlet cannot read do not look identical (§0.0, HM-DEC-092).</para>
    /// <para>**THE ROW IS FED, NOT SYNTHESISED FROM AUDIO.** What is being asserted is
    /// what the panel does with a channel that is held and not readable, and
    /// `ShowPsk31ChannelsForTests` is the path the tick takes from a channel onwards -
    /// the same idiom units 316 and 323 used for a question about text.</para>
    /// </remarks>
    [Fact]
    public void AHeldCarrierWithTheSquelchShutIsADimmedRowThatSaysSo()
    {
        var model = Listening(null);

        model.ShowPsk31ChannelsForTests(
            new[] { new Psk31Channel(1, 893, 24.5, "", Readable: false) });

        var heard = model.DigitalDecodes.Single();

        _output.WriteLine("shut  : \"" + heard.Message + "\" at opacity "
            + heard.RowOpacity.ToString("0.00", CultureInfo.InvariantCulture));

        Assert.Equal(MainWindowViewModel.HeardNotReadableYet, heard.Message);
        Assert.Equal(0.55, heard.RowOpacity);
        Assert.True(heard.HeardNotReadable);

        // **AND NOTHING IS READ OUT OF THE WORDS.** They are a sentence Hamlet wrote, so
        // no callsign, no grid and no turn may come out of them (§0.0).
        Assert.True(string.IsNullOrEmpty(heard.Sender), "sender \"" + heard.Sender + "\"");
        Assert.False(heard.HasFields);
        Assert.Null(model.SendMenuFor(heard));

        // **THEN THE SQUELCH OPENS AND THE ROW LIFTS AND FILLS.**
        model.ShowPsk31ChannelsForTests(
            new[]
            {
                new Psk31Channel(1, 893, 24.5, "CQ CQ de W1AW W1AW K", Readable: true),
            });

        var read = model.DigitalDecodes.Single();

        _output.WriteLine("open  : \"" + read.Message + "\" at opacity "
            + read.RowOpacity.ToString("0.00", CultureInfo.InvariantCulture));

        Assert.Equal("CQ CQ de W1AW W1AW K", read.Message);
        Assert.Equal(1.0, read.RowOpacity);
        Assert.False(read.HeardNotReadable);

        // **AND WHAT WAS READ IS NOT TAKEN BACK OFF THE SCREEN.** A station that stops
        // between overs closes the squelch again; the words he already sent stay.
        model.ShowPsk31ChannelsForTests(
            new[]
            {
                new Psk31Channel(1, 893, 24.5, "CQ CQ de W1AW W1AW K", Readable: false),
            });

        var still = model.DigitalDecodes.Single();

        Assert.Equal("CQ CQ de W1AW W1AW K", still.Message);
        Assert.Equal(1.0, still.RowOpacity);
    }

    /// <summary>
    /// **Assertion 4: `psk31_reading` exists, `psk31_lock` does not, and nothing personal.**
    /// </summary>
    /// <remarks>
    /// **THE NAME WAS THE CLAIM.** `psk31_lock` read as a decoder that had lost the
    /// thread; what it measures is whether characters are coming out, which is a state a
    /// station may sit in for as long as it likes.
    /// </remarks>
    [Fact]
    public void TheRecordSaysReadingAndNoLongerSaysLock()
    {
        var lines = Play("psk31-four-signals.wav");

        var reading = Events(lines, "psk31_reading");

        _output.WriteLine("psk31_reading " + reading.Count + " event(s), psk31_lock "
            + Events(lines, "psk31_lock").Count);

        Assert.NotEmpty(reading);
        Assert.Empty(Events(lines, "psk31_lock"));

        foreach (var one in reading)
        {
            Assert.True(one.GetProperty("data").TryGetProperty("reading", out _));
            Assert.False(one.GetProperty("data").TryGetProperty("locked", out _));
        }

        // **AND THE PRIVACY SCAN, OVER EVERY BYTE OF EVERY LINE** (HM-DEC-018, §2.1). The
        // fixtures' own callsigns, grids and words are what would show up.
        //
        // **WHOLE WORDS, AND THAT IS NOT A LOOPHOLE.** A plain substring scan reported
        // `Lyon` against `MostlyOneWay`, the constant the search rule names in the
        // `psk31_listening_started` line - `Most-lyOn-eWay`. Anything the record actually
        // leaked would be a token of its own inside quotes, which a boundary matches.
        foreach (var word in new[]
        {
            "KC3QIS", "W1AW", "EI4GNB", "F4DIA", "VE3XN", "Trafford", "Lyon", "FN00DJ",
        })
        {
            var found = lines.FirstOrDefault(line => Regex.IsMatch(
                line, @"\b" + Regex.Escape(word) + @"\b", RegexOptions.IgnoreCase));

            Assert.True(found is null, word + " is in the record: " + found);
        }
    }

    /// <summary>
    /// **Assertion 5: the rule and its number live in one named place.**
    /// </summary>
    [Fact]
    public void TheRetireRuleAndItsNumberLiveInOneNamedPlace()
    {
        _output.WriteLine("rule   : " + Psk31Listener.RetireRule);
        _output.WriteLine("passes : " + Psk31CarrierSearch.RetireAfterPasses);
        _output.WriteLine("seconds: " + new Psk31CarrierSearch(Psk31Resampler.TargetSampleRate)
            .RetireAfterSeconds.ToString("0.000", CultureInfo.InvariantCulture)
            + " at " + Psk31Resampler.TargetSampleRate + " Hz");

        Assert.False(string.IsNullOrWhiteSpace(Psk31Listener.RetireRule));
        Assert.True(Psk31CarrierSearch.RetireAfterPasses > 0);

        // **ABOUT A SECOND, WHICH IS ONE WHOLE SPECTRUM AVERAGE.** Shorter and one
        // unlucky window retires a station; much longer and a row outlives its signal.
        var seconds = new Psk31CarrierSearch(Psk31Resampler.TargetSampleRate).RetireAfterSeconds;

        Assert.InRange(seconds, 0.5, 2.0);

        // **AND THE RULE IS NOT A RULE ABOUT CHARACTERS.**
        Assert.DoesNotContain("LostLock", Psk31Listener.RetireRule, StringComparison.Ordinal);
        Assert.Contains("never retired for saying nothing", Psk31Listener.RetireRule
            .Replace("**", "", StringComparison.Ordinal), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// **Assertion 6: a row put up for a held carrier is retired when the carrier is.**
    /// </summary>
    /// <remarks>
    /// **A ROW THAT SAYS *heard, not readable yet* IS STILL A ROW WITH A CARRIER UNDER
    /// IT**, and when the carrier goes so does it. Otherwise the words would be a way for
    /// a station that has gone to stay on the list for ever.
    /// </remarks>
    [Fact]
    public void AHeardRowGoesWhenItsCarrierGoes()
    {
        var model = Listening(null);

        model.ShowPsk31ChannelsForTests(
            new[] { new Psk31Channel(1, 893, 24.5, "", Readable: false) });

        Assert.Single(model.DigitalDecodes);

        model.ShowPsk31ChannelsForTests(Array.Empty<Psk31Channel>());

        Assert.Empty(model.DigitalDecodes);
    }

    /// <summary>Play a fixture through the real tap and tick with the record running.</summary>
    private List<string> Play(string fixture)
    {
        HashMatches(fixture, "manifest-step2.json");

        var audio = WavAudio.Read(Fixture(fixture));

        using (var telemetry = new JsonlTelemetry(_folder, "324", _ => true))
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

            // **LEAVING THE TAB IS WHAT CLOSES A CARRIER THAT IS STILL THERE**, which is
            // how every appearance is accounted for inside a test that cannot wait.
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
