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

namespace Hamlet.App.Tests.Telemetry;

/// <summary>
/// Work instruction 322 task 2: **the PSK31 receive path writes its record.**
/// </summary>
/// <remarks>
/// <para>**WHAT THIS EXISTS FOR, MEASURED.** Across ten sessions and 12,587 events on
/// 2026-09-11, the FT8 path wrote 3,448 `ft8_slot` and 2,317 `decode_quality`, and **the
/// PSK31 path wrote nothing at all.** The operator sat on the tab for five minutes and
/// asked afterwards whether the band had been empty or the squelch had been shut, and
/// nothing in the file could answer him.</para>
/// <para>**THE ONE LINE THAT ANSWERS IT** is `psk31_search_pass`: a pass with no
/// candidates says the band was quiet; a pass whose candidates all read `crossed: false`
/// says something was there and Hamlet would not take it. Assertion 2 is that
/// distinction.</para>
/// <para>**NOTHING PERSONAL, PROVED BY SCANNING RATHER THAN BY TRUST** (HM-DEC-018,
/// §2.1). Assertion 4 reads every byte of every event written by the three assertions
/// above it and looks for the fixtures' own callsigns, grids and words.</para>
/// <para>**COMPUTED, NOT SEEN**, and no radio was involved (FACT-006).</para>
/// </remarks>
public sealed class ThePsk31TelemetryTests : IDisposable
{
    private const string HisCall = "KC3QIS";

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the events are printed.</param>
    public ThePsk31TelemetryTests(ITestOutputHelper output)
    {
        _output = output;

        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-psk31-telemetry-" + Guid.NewGuid().ToString("N"));

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

    /// <summary>**The four-signal fixture gives four appearances and four retirements.**</summary>
    [Fact]
    public void TheFourSignalFixtureGivesFourAppearancesAndFourRetirements()
    {
        var lines = Play("psk31-four-signals.wav", quiet: false);

        var appeared = Events(lines, "psk31_carrier_appeared");
        var retired = Events(lines, "psk31_carrier_retired");

        foreach (var one in appeared)
        {
            _output.WriteLine("appeared at "
                + Number(one, "offsetHz").ToString("0.0", CultureInfo.InvariantCulture)
                + " Hz, strength "
                + Number(one, "strengthDb").ToString("0.0", CultureInfo.InvariantCulture)
                + " dB, quality "
                + Number(one, "quality").ToString("0.000", CultureInfo.InvariantCulture)
                + ", after " + Number(one, "passesAsCandidate") + " passes");
        }

        foreach (var one in retired)
        {
            _output.WriteLine("retired at "
                + Number(one, "offsetHz").ToString("0.0", CultureInfo.InvariantCulture)
                + " Hz, " + Text(one, "reason")
                + ", lifetime "
                + Number(one, "lifetimeSeconds").ToString("0.0", CultureInfo.InvariantCulture)
                + " s, " + Number(one, "charactersEmitted") + " characters, "
                + Number(one, "linesParsed") + " lines");
        }

        Assert.Equal(4, appeared.Count);
        Assert.Equal(4, retired.Count);

        // **AT THE FOUR PLACES THE FIXTURE PUT THEM**, within 5 Hz.
        foreach (var wanted in new double[] { 700, 1100, 1600, 2200 })
        {
            Assert.True(
                appeared.Any(e => Math.Abs(Number(e, "offsetHz") - wanted) <= 5),
                "nothing appeared within 5 Hz of " + wanted);
        }

        // **EVERY RETIREMENT SAYS WHY AND HOW MUCH IT HEARD.**
        foreach (var one in retired)
        {
            Assert.False(string.IsNullOrWhiteSpace(Text(one, "reason")));
            Assert.True(Number(one, "charactersEmitted") >= 0);
        }
    }

    /// <summary>**Noise produces candidates that never cross, and no carriers.**</summary>
    /// <remarks>
    /// **THIS IS THE ASSERTION THE WHOLE UNIT IS FOR.** An empty list on a quiet band and
    /// an empty list on a band Hamlet would not take look identical on the screen. In the
    /// file they must not.
    /// </remarks>
    [Fact]
    public void NoiseProducesCandidatesThatNeverCrossAndNoCarriers()
    {
        var lines = Play("psk31-noise-only-30s.wav", quiet: false);

        var passes = Events(lines, "psk31_search_pass");
        var appeared = Events(lines, "psk31_carrier_appeared");

        var candidates = passes
            .SelectMany(p => p.GetProperty("data").GetProperty("candidates").EnumerateArray())
            .ToList();

        _output.WriteLine("passes written  : " + passes.Count);
        _output.WriteLine("candidates named: " + candidates.Count);
        _output.WriteLine("carriers        : " + appeared.Count);

        foreach (var pass in passes.Take(3))
        {
            _output.WriteLine("  " + pass.GetProperty("data").GetRawText()[..Math.Min(200,
                pass.GetProperty("data").GetRawText().Length)]);
        }

        Assert.NotEmpty(passes);

        // **NOT ONE OF THEM CROSSED.**
        foreach (var candidate in candidates)
        {
            Assert.False(
                candidate.GetProperty("crossed").GetBoolean(),
                "a candidate crossed on noise alone");
        }

        Assert.Empty(appeared);
    }

    /// <summary>**Every parsed line writes its verdict, and it matches the corpus.**</summary>
    [Fact]
    public void EveryParsedLineWritesItsVerdict()
    {
        var corpus = Corpus();

        var wanted = corpus
            .SelectMany(t => t.Expected)
            .ToList();

        var lines = ParseCorpus(corpus);

        var parsed = Events(lines, "psk31_line_parsed");

        _output.WriteLine("lines in the corpus : " + wanted.Count);
        _output.WriteLine("events written      : " + parsed.Count);

        Assert.Equal(wanted.Count, parsed.Count);

        var wrong = 0;

        for (var at = 0; at < wanted.Count; at++)
        {
            var got = parsed[at];
            var kind = Text(got, "kind");
            var certain = got.GetProperty("data").GetProperty("certain").GetBoolean();

            var matches = string.Equals(kind, wanted[at].Kind, StringComparison.OrdinalIgnoreCase)
                && certain == wanted[at].Certain;

            if (!matches)
            {
                wrong++;

                _output.WriteLine("  line " + at + ": wrote " + kind + "/" + certain
                    + ", corpus says " + wanted[at].Kind + "/" + wanted[at].Certain);
            }
        }

        _output.WriteLine("mismatches          : " + wrong);

        Assert.Equal(0, wrong);
    }

    /// <summary>**Nothing personal reaches the file.**</summary>
    /// <remarks>
    /// **SCANNED, NOT TRUSTED** (HM-DEC-018, §2.1). Every byte of every event the three
    /// assertions above write, against every callsign and grid in the fixtures, and
    /// against any run of five letters from the fixture text. A call site that forgot is
    /// caught here rather than on somebody's disk.
    /// </remarks>
    [Fact]
    public void NothingPersonalReachesTheFile()
    {
        var lines = new List<string>();

        lines.AddRange(Play("psk31-four-signals.wav", quiet: false));
        lines.AddRange(Play("psk31-noise-only-30s.wav", quiet: false));
        lines.AddRange(ParseCorpus(Corpus()));

        var mine = lines.Where(l => l.Contains("psk31", StringComparison.Ordinal)).ToList();

        _output.WriteLine("psk31 events scanned: " + mine.Count);

        Assert.NotEmpty(mine);

        foreach (var callsign in new[]
        {
            HisCall, "W1AW", "EI4GNB", "F4DIA", "VE3XN", "G4XYZ", "K3ABC", "N4ZEK",
            "KD8WYT", "JA1XYZ",
        })
        {
            foreach (var line in mine)
            {
                Assert.DoesNotContain(callsign, line, StringComparison.OrdinalIgnoreCase);
            }
        }

        foreach (var grid in new[] { "FN00", "FN31", "IO91", "JN36", "EN52" })
        {
            foreach (var line in mine)
            {
                Assert.DoesNotContain(grid, line, StringComparison.OrdinalIgnoreCase);
            }
        }

        // **AND NO RUN OF FIVE LETTERS FROM THE FIXTURE TEXT.** The words below are in
        // the transcripts and in nothing this path is allowed to write.
        foreach (var word in new[]
        {
            "NEWINGTON", "TRAFFORD", "THANKS", "PLEASE", "TONIGHT", "RADIO",
        })
        {
            foreach (var line in mine)
            {
                Assert.DoesNotContain(word, line, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    /// <summary>**Pressing the tab is written, and so is leaving it.**</summary>
    [Fact]
    public void PressingTheTabIsWrittenAndSoIsLeavingIt()
    {
        List<string> lines;

        using (var telemetry = new JsonlTelemetry(_folder, "322", _ => true))
        {
            var model = Panel(telemetry);

            model.ChooseDigitalModeCommand.Execute("PSK31");

            // A little audio so the listener actually opens.
            Feed(model, "psk31-four-signals.wav", seconds: 6);

            // **AND NOW HE PRESSES THE MODE HE IS ALREADY ON**, which is the press that
            // wrote nothing at all before this unit.
            model.ChooseDigitalModeCommand.Execute("PSK31");

            model.ChooseDigitalModeCommand.Execute("FT8");
        }

        // **READ AFTER THE SINK IS CLOSED, NOT INSIDE IT.** The writer buffers, so a
        // read taken before `Dispose` misses whatever the last few calls wrote - which
        // is exactly the stopped event this assertion is about. My own mistake, and the
        // reason `Play` already reads outside its `using`.
        lines = Read();

        var started = Events(lines, "psk31_listening_started");
        var stopped = Events(lines, "psk31_listening_stopped");

        var subMode = lines
            .Where(l => l.Contains("digital_sub_mode", StringComparison.Ordinal))
            .ToList();

        _output.WriteLine("started : " + started.Count);
        _output.WriteLine("stopped : " + stopped.Count);
        _output.WriteLine("sub-mode: " + subMode.Count);

        foreach (var one in subMode)
        {
            _output.WriteLine("  " + one);
        }

        Assert.Single(started);
        Assert.Single(stopped);

        // **THREE PRESSES, THREE LINES** - including the one that changed nothing, which
        // is the fault task 1b found.
        Assert.Equal(3, subMode.Count);

        Assert.Contains(
            subMode,
            l => l.Contains("\"from\":\"PSK31\"", StringComparison.Ordinal)
                && l.Contains("\"to\":\"PSK31\"", StringComparison.Ordinal));
    }

    /// <summary>**The switch turns them off and changes nothing else.**</summary>
    [Fact]
    public void TheSwitchTurnsThemOffAndChangesNothingElse()
    {
        var loud = Play("psk31-four-signals.wav", quiet: false);
        var quiet = Play("psk31-four-signals.wav", quiet: true);

        var loudPsk = loud.Count(l => l.Contains("\"psk31\"", StringComparison.Ordinal));
        var quietPsk = quiet.Count(l => l.Contains("\"psk31\"", StringComparison.Ordinal));

        _output.WriteLine("with the switch on : " + loudPsk + " psk31 events");
        _output.WriteLine("with it off        : " + quietPsk);

        Assert.True(loudPsk > 0, "the switch on wrote nothing");
        Assert.Equal(0, quietPsk);

        // **AND EVERYTHING ELSE IS STILL WRITTEN.** A category switch is not a way to
        // turn the whole file off.
        Assert.NotEmpty(quiet);
    }

    /// <summary>Play a fixture through the real tick and read the file back.</summary>
    private List<string> Play(string fixture, bool quiet)
    {
        using (var telemetry = new JsonlTelemetry(
            _folder,
            "322",
            category => !quiet || category != TelemetryCategory.Psk31))
        {
            var model = Panel(telemetry);

            model.ChooseDigitalModeCommand.Execute("PSK31");

            Feed(model, fixture, seconds: 0);

            // **LEAVING THE TAB IS WHAT CLOSES THE CARRIERS**, which is how the four
            // retirements happen inside a test that cannot wait a real minute.
            model.ChooseDigitalModeCommand.Execute("FT8");
        }

        return Read();
    }

    /// <summary>Push a fixture in, in quarter-second lumps, with a tick between.</summary>
    private static void Feed(MainWindowViewModel model, string fixture, double seconds)
    {
        var audio = WavAudio.Read(Fixture(fixture));

        var take = seconds > 0
            ? Math.Min(audio.Samples.Length, (int)(seconds * audio.SampleRate))
            : audio.Samples.Length;

        var chunk = audio.SampleRate / 4;

        for (var at = 0; at < take; at += chunk)
        {
            var count = Math.Min(chunk, take - at);

            model.TapForTests!.Take(audio.Samples.AsSpan(at, count), audio.SampleRate);

            model.LookForASlotForTests();
        }
    }

    /// <summary>Feed every corpus line through the parser and write its verdict.</summary>
    /// <remarks>
    /// <para>**THE PARSER, NOT THE SPLITTER, AND THE CORPUS SAYS WHICH.** `corpus.json`
    /// pairs `lines` with `expected` one for one, so it is a parser fixture. The first
    /// draft of this fed the lines through `Psk31MessageSplitter` and got **31 verdicts
    /// for 32 lines**: `05-garbled` line 3 - *yes copy all fine here, nice signal into PA
    /// tonight* - carries no turnover word, so the splitter folds it into the next
    /// message. **That is unit 316's ruled behaviour** (§3.4: a message is what arrived
    /// between two turnovers, not what arrived in a line) and `ThePsk31MessageSplitTests`
    /// holds it there.</para>
    /// <para>**SO ONE EVENT PER LINE IS A FACT ABOUT THE PARSER AND NOT ABOUT THE
    /// STREAM.** On the air the event fires per message, which is what the running path
    /// does; here it fires per line, which is what the corpus can be checked against.
    /// </para>
    /// </remarks>
    private List<string> ParseCorpus(IReadOnlyList<Transcript> corpus)
    {
        using (var telemetry = new JsonlTelemetry(_folder, "322", _ => true))
        {
            foreach (var transcript in corpus)
            {
                foreach (var line in transcript.Lines)
                {
                    var verdict = Psk31ExchangeParser.Read(line, HisCall);

                    Psk31Events.LineParsed(
                        telemetry,
                        1000,
                        verdict.Kind.ToString(),
                        verdict.IsCertain,
                        verdict.HandsOver,
                        verdict.IsForOperator,
                        line.Length);
                }
            }
        }

        return Read();
    }

    private MainWindowViewModel Panel(JsonlTelemetry telemetry)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = HisCall;
        settings.Operator.GridSquare = "FN00DJ";

        var model = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
            TapForTests = new AudioTap(),
            ClockOffset = new ClockOffset(0.033, DateTime.UtcNow),
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        return model;
    }

    private List<string> Read()
    {
        var lines = Directory.GetFiles(_folder, "*.jsonl")
            .SelectMany(File.ReadAllLines)
            .ToList();

        foreach (var file in Directory.GetFiles(_folder, "*.jsonl"))
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
    {
        var data = e.GetProperty("data");

        return data.TryGetProperty(field, out var value)
            && value.ValueKind == JsonValueKind.Number
                ? value.GetDouble()
                : 0;
    }

    private static string Text(JsonElement e, string field)
        => e.GetProperty("data").TryGetProperty(field, out var value)
            && value.ValueKind == JsonValueKind.String
                ? value.GetString() ?? ""
                : "";

    private sealed record Verdict(string Kind, bool Certain);

    private sealed record Transcript(
        string Name, IReadOnlyList<string> Lines, IReadOnlyList<Verdict> Expected);

    private static IReadOnlyList<Transcript> Corpus()
    {
        var path = Path.Combine(
            Root(), "assets", "fixtures", "psk31-transcripts", "corpus.json");

        using var stream = File.OpenRead(path);

        var document = JsonDocument.Parse(stream);

        return document.RootElement.GetProperty("transcripts").EnumerateArray()
            .Select(t => new Transcript(
                t.GetProperty("name").GetString() ?? "",
                t.GetProperty("lines").EnumerateArray()
                    .Select(l => l.GetString() ?? "").ToList(),
                t.GetProperty("expected").EnumerateArray()
                    .Select(e => new Verdict(
                        e.GetProperty("kind").GetString() ?? "",
                        e.GetProperty("certain").GetBoolean()))
                    .ToList()))
            .ToList();
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
