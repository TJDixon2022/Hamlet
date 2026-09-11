using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Psk31;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 315 task 3: **pressing PSK31 hears everyone.**
/// </summary>
/// <remarks>
/// <para>**ONE ROW PER SIGNAL, IN THE LIST FT8 USES.** Every carrier the search finds gets
/// its own demodulator and its own row in `DigitalDecodes`, carrying where it is, how
/// strong it is and its text as it arrives - and the row goes when the carrier does.</para>
/// <para>**ROWS ARE TEXT ONLY.** Nothing is parsed out of them, nothing is clickable and
/// nothing on them is a station yet; that is step 3. The operator's callsign here is
/// KC3QIS, which is in the fixtures' text, so a row that was being read as addressed to
/// him would show up.</para>
/// <para>**FED THROUGH THE REAL TAP AND THE REAL TICK**, in quarter-second lumps.
/// **COMPUTED, NOT SEEN**, and every fixture is synthetic (FACT-004, FACT-006).</para>
/// </remarks>
public sealed class ThePsk31HearsEveryoneTests
{
    private const string OwnCall = "KC3QIS";

    /// <summary>How close a row's offset must be to a fixture's carrier to be that carrier's row.</summary>
    private const double MatchHz = 20;

    /// <summary>The fixtures idle 40 bits after the last character before the carrier stops.</summary>
    private static readonly double FixtureIdleTailSeconds = 40 / Psk31Demodulator.Baud;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the rows are printed.</param>
    public ThePsk31HearsEveryoneTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Assertion 1: two signals, two rows, each with its own text.**</summary>
    /// <remarks>
    /// <para>**THE 1500 HZ STATION'S TEXT IS NOT RECORDED ANYWHERE IN THE TREE.**
    /// `manifest.json` says only that it is EI4GNB calling CQ, 3 dB weaker, and
    /// `assets/reference-modem.py` holds the modem but not the script that made the file.
    /// The one EI4GNB CQ line the tree does record is the line `manifest-step2.json`'s `C`
    /// repeats, and measured against `C` as it stands - three lines - this row read
    /// 1.0169, because it holds six.</para>
    /// <para>**SO THE REFERENCE IS DERIVED, AND NOT FROM WHAT WAS DECODED.** The recorded
    /// line is repeated as many whole times as fit in the file once the 40 idle bits at
    /// each end are taken off, counting the line's own varicode bits at 31.25 baud. That
    /// count comes from the file's length and the line alone, so a row that dropped or
    /// invented a whole line would still fail. The arithmetic is printed. **A reference a
    /// session derived is weaker than one the author recorded, and it is reported as
    /// such.**</para>
    /// </remarks>
    [Fact]
    public void TheTwoSignalFixtureYieldsTwoRowsEachWithItsOwnText()
    {
        const string file = "psk31-two-signals-1000-1500hz.wav";

        var heard = Listen(file, "manifest.json", 1000, 1500);

        var qso = heard.Tracks[1000];
        var cq = heard.Tracks[1500];

        Assert.Empty(heard.Strays);
        Assert.Equal(1, qso.Appearances);
        Assert.Equal(1, cq.Appearances);

        var step2 = Step2Text("C");
        var line = step2[..(step2.IndexOf('\n', StringComparison.Ordinal) + 1)];
        var lineSeconds = Varicode.Encode(line).Length / Psk31Demodulator.Baud;
        var fileSeconds = WavAudio.Read(Fixture(file)).Duration.TotalSeconds;
        var lines = (int)Math.Floor((fileSeconds - (80 / Psk31Demodulator.Baud)) / lineSeconds);
        var derived = string.Concat(Enumerable.Repeat(line, lines));

        _output.WriteLine("1500 Hz reference: the recorded line, "
            + Varicode.Encode(line).Length + " bits, " + Seconds(lineSeconds) + ", fits "
            + lines + " whole times in " + Seconds(fileSeconds) + " less 80 idle bits");

        var qsoCer = ErrorRate(qso.Text, QsoText());
        var cqCer = ErrorRate(cq.Text, derived);

        _output.WriteLine("1000 Hz CER " + Rate(qsoCer) + " against qso-text.txt");
        _output.WriteLine("1500 Hz CER " + Rate(cqCer) + " against the derived reference, "
            + Rate(ErrorRate(cq.Text, step2)) + " against manifest-step2.json C as it stands");

        Assert.True(qsoCer <= 0.05, "1000 Hz CER " + Rate(qsoCer));
        Assert.True(cqCer <= 0.05, "1500 Hz CER " + Rate(cqCer));

        // **AND NOTHING OF ONE IN THE OTHER.**
        Assert.DoesNotContain("EI4GNB", qso.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("W1AW", cq.Text, StringComparison.Ordinal);
    }

    /// <summary>**Assertion 2: four signals, four rows, each read over the span it was on.**</summary>
    /// <remarks>
    /// **"OVER THE SPAN ITS CARRIER WAS ON" IS THE ROW'S OWN TEXT**, because a row only
    /// ever holds what was read while the search vouched for its carrier. The author's
    /// unsquelched reference read 0.125, 0.386, 0.183 and 0.000 when it kept reading after
    /// the carriers stopped; nothing here is truncated to flatter a number.
    /// </remarks>
    [Fact]
    public void TheFourSignalFixtureYieldsFourRowsEachReadOverItsSpan()
    {
        var heard = Listen("psk31-four-signals.wav", "manifest-step2.json", 700, 1100, 1600, 2200);

        Assert.Empty(heard.Strays);

        foreach (var (hz, key) in new[] { (700.0, "A"), (1100.0, "B"), (1600.0, "C"), (2200.0, "D") })
        {
            var track = heard.Tracks[hz];
            var cer = ErrorRate(track.Text, Step2Text(key));

            _output.WriteLine(hz.ToString("0", CultureInfo.InvariantCulture) + " Hz CER " + Rate(cer)
                + " against " + key + ", appearances " + track.Appearances);

            Assert.Equal(1, track.Appearances);
            Assert.True(cer <= 0.10, hz.ToString("0", CultureInfo.InvariantCulture) + " Hz CER " + Rate(cer));
        }
    }

    /// <summary>**Assertion 3: noise, no rows at all.**</summary>
    [Fact]
    public void TheNoiseOnlyFixtureYieldsNoRows()
    {
        var heard = Listen("psk31-noise-only-30s.wav", "manifest.json");

        Assert.Equal(0, heard.MostRowsAtOnce);
        Assert.Empty(heard.Strays);
    }

    /// <summary>**Assertion 4: a carrier that stops is retired within the stated time.**</summary>
    /// <remarks>
    /// <para>**THE RULE AND ITS NUMBER LIVE IN `Psk31Listener`**, and are printed.</para>
    /// <para>**MEASURED FROM THE LAST TIME THE ROW GREW.** That is later than the last
    /// character was keyed, never earlier, so a row gone within the fixture's idle tail
    /// plus <see cref="Psk31Listener.RetiredWithinSeconds"/> of it was gone within that of
    /// the carrier stopping.</para>
    /// </remarks>
    [Fact]
    public void ACarrierThatStopsIsRetiredWithinTheStatedTime()
    {
        _output.WriteLine("rule   : " + Psk31Listener.RetireRule);
        _output.WriteLine("number : " + Psk31Listener.RetiredWithinSeconds.ToString("0.0", CultureInfo.InvariantCulture)
            + " s after the carrier stops");

        Assert.False(string.IsNullOrWhiteSpace(Psk31Listener.RetireRule));

        var heard = Listen("psk31-four-signals.wav", "manifest-step2.json", 700, 1100, 1600, 2200);

        var bound = FixtureIdleTailSeconds + Psk31Listener.RetiredWithinSeconds;
        var retired = heard.Tracks.Values.Where(t => t.RemovedAt is not null).ToList();

        foreach (var track in heard.Tracks.Values)
        {
            _output.WriteLine(track.Expected.ToString("0", CultureInfo.InvariantCulture) + " Hz: last grew "
                + Seconds(track.LastGrewAt) + ", removed "
                + (track.RemovedAt is { } gone ? Seconds(gone) + ", after " + Seconds(gone - track.LastGrewAt) : "never, still on at the end"));
        }

        // **THREE OF THE FOUR STOP BEFORE THE FILE ENDS**, measured by the search in task 2.
        Assert.NotEmpty(retired);

        foreach (var track in retired)
        {
            var after = track.RemovedAt!.Value - track.LastGrewAt;

            Assert.True(
                after <= bound,
                track.Expected.ToString("0", CultureInfo.InvariantCulture) + " Hz was retired "
                + Seconds(after) + " after it last grew, over the bound of " + Seconds(bound));
        }
    }

    /// <summary>**Assertion 6: no row can be answered, and nothing is parsed out of one.**</summary>
    /// <remarks>
    /// **THE TAB STAYS INERT** - `ThePsk31TabIsInertTests` is re-run after this - and a
    /// row whose text happens to be three words is still not a message with an addressee,
    /// a sender and a payload. Without that a row reading `CQ CQ CQ` for a moment would be
    /// picked out by the CQ filter and a row reading `KC3QIS de W1AW` would be put on the
    /// operator's own side.
    /// </remarks>
    [Fact]
    public void NoRowCanBeAnsweredAndNothingIsParsedOutOfOne()
    {
        var heard = Listen("psk31-four-signals.wav", "manifest-step2.json", 700, 1100, 1600, 2200);

        _output.WriteLine("rows at most at once : " + heard.MostRowsAtOnce);
        _output.WriteLine("could be answered    : " + heard.EverAnswerable);
        _output.WriteLine("parsed into fields   : " + heard.EverParsed);
        _output.WriteLine("put on his side      : " + heard.EverForHim);

        Assert.True(heard.MostRowsAtOnce >= 2, "never more than " + heard.MostRowsAtOnce + " row at once");
        Assert.False(heard.EverAnswerable);
        Assert.False(heard.EverParsed);
        Assert.False(heard.EverForHim);
    }

    /// <summary>One carrier's row over a whole file.</summary>
    private sealed class Track
    {
        public Track(double expected) => Expected = expected;

        public double Expected { get; }

        public string Text { get; set; } = "";

        public double LastGrewAt { get; set; }

        public double? RemovedAt { get; set; }

        public int Appearances { get; set; }

        public bool Present { get; set; }
    }

    /// <summary>Everything the list did over one file.</summary>
    private sealed class Heard
    {
        public Dictionary<double, Track> Tracks { get; } = new();

        public HashSet<string> Strays { get; } = new();

        public int MostRowsAtOnce { get; set; }

        public bool EverAnswerable { get; set; }

        public bool EverParsed { get; set; }

        public bool EverForHim { get; set; }
    }

    /// <summary>Play a fixture through the real tap and tick and watch the list.</summary>
    private Heard Listen(string file, string manifest, params double[] carriers)
    {
        HashMatches(file, manifest);

        var audio = WavAudio.Read(Fixture(file));
        var model = Listening();
        var heard = new Heard();

        foreach (var hz in carriers)
        {
            heard.Tracks[hz] = new Track(hz);
        }

        var chunk = audio.SampleRate / 4;

        for (var at = 0; at < audio.Samples.Length; at += chunk)
        {
            var count = Math.Min(chunk, audio.Samples.Length - at);

            model.TapForTests!.Take(audio.Samples.AsSpan(at, count), audio.SampleRate);
            model.LookForASlotForTests();

            Watch(model, heard, (double)(at + count) / audio.SampleRate);
        }

        _output.WriteLine(file + ": most rows at once " + heard.MostRowsAtOnce
            + ", strays " + heard.Strays.Count);

        foreach (var track in heard.Tracks.Values)
        {
            _output.WriteLine("  " + track.Expected.ToString("0", CultureInfo.InvariantCulture) + " Hz: "
                + track.Text.Length + " characters: " + Shown(track.Text));
        }

        foreach (var stray in heard.Strays)
        {
            _output.WriteLine("  stray: " + stray);
        }

        return heard;
    }

    private static void Watch(MainWindowViewModel model, Heard heard, double seconds)
    {
        var rows = model.DigitalDecodes.ToList();

        heard.MostRowsAtOnce = Math.Max(heard.MostRowsAtOnce, rows.Count);

        if (rows.Count > 0)
        {
            heard.EverAnswerable |= model.CanAnswerRowsForTests;
            heard.EverParsed |= rows.Any(r => r.HasFields);
            heard.EverForHim |= model.DigitalMineDecodes.Count > 0;
        }

        var seen = new HashSet<double>();

        foreach (var row in rows)
        {
            var hz = double.TryParse(row.Hz, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : double.NaN;

            var track = heard.Tracks.Values.FirstOrDefault(t => Math.Abs(t.Expected - hz) <= MatchHz);

            if (track is null || !seen.Add(track.Expected))
            {
                heard.Strays.Add(row.Hz + " Hz: " + Shown(row.Message));
                continue;
            }

            if (!track.Present)
            {
                track.Present = true;
                track.Appearances++;
                track.RemovedAt = null;
            }

            if (row.Message.Length > track.Text.Length)
            {
                track.Text = row.Message;
                track.LastGrewAt = seconds;
            }
        }

        foreach (var track in heard.Tracks.Values.Where(t => t.Present && !seen.Contains(t.Expected)))
        {
            track.Present = false;
            track.RemovedAt = seconds;
        }
    }

    private MainWindowViewModel Listening()
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = OwnCall;
        settings.Operator.GridSquare = "FN00DJ";

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            TapForTests = new AudioTap(),
            ClockOffset = new ClockOffset(0.033, DateTime.UtcNow),
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        model.ChooseDigitalModeCommand.Execute("PSK31");

        return model;
    }

    /// <summary>**Assertion 5: every fixture is the file its manifest says, before use.**</summary>
    private static void HashMatches(string file, string manifest)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Fixture(manifest)));

        var want = document.RootElement.EnumerateArray()
            .Single(e => e.GetProperty("file").GetString() == file)
            .GetProperty("sha256").GetString();

        using var stream = File.OpenRead(Fixture(file));

        Assert.Equal(want, Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant());
    }

    private static string Step2Text(string key)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Fixture("manifest-step2.json")));

        return document.RootElement.EnumerateArray()
            .First(e => e.GetProperty("texts").TryGetProperty(key, out _))
            .GetProperty("texts").GetProperty(key).GetString() ?? "";
    }

    private static string QsoText()
        => Encoding.Latin1.GetString(File.ReadAllBytes(Fixture("qso-text.txt")));

    /// <summary>Edit distance over the reference length, idle trimmed.</summary>
    private static double ErrorRate(string got, string want)
    {
        var a = got.Trim();
        var b = want.Trim();

        if (b.Length == 0)
        {
            return a.Length == 0 ? 0 : 1;
        }

        var previous = new int[b.Length + 1];
        var current = new int[b.Length + 1];

        for (var j = 0; j <= b.Length; j++)
        {
            previous[j] = j;
        }

        for (var i = 1; i <= a.Length; i++)
        {
            current[0] = i;

            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;

                current[j] = Math.Min(
                    Math.Min(current[j - 1] + 1, previous[j] + 1),
                    previous[j - 1] + cost);
            }

            (previous, current) = (current, previous);
        }

        return (double)previous[b.Length] / b.Length;
    }

    private static string Rate(double cer) => cer.ToString("0.0000", CultureInfo.InvariantCulture);

    private static string Seconds(double s) => s.ToString("0.00", CultureInfo.InvariantCulture) + " s";

    private static string Shown(string text)
    {
        var flat = text.Replace("\r", "", StringComparison.Ordinal).Replace("\n", " / ", StringComparison.Ordinal);

        return flat.Length <= 120 ? flat : flat[..120] + "…";
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
