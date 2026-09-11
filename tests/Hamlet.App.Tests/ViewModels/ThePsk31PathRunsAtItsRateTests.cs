using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 324 task 2: **the PSK31 path runs at the rate it was proved at.**
/// </summary>
/// <remarks>
/// <para>**THE FAULT THIS EXISTS FOR WAS MEASURED ON THE AIR, NOT SUPPOSED.** On
/// 2026-09-11 the operator sat on 14.070 with the application up.
/// `psk31_listening_started` recorded `sampleRate: 48000, passbandLowHz: 64,
/// passbandHighHz: 23936`: the path had taken the sound card's own rate and derived the
/// passband from it. A real carrier at 893 Hz, 62 to 66 dB over the floor, was found
/// three times in a minute; each time it was retired after 1.9 s having emitted **no
/// characters at all**, and fourteen minutes earlier 17 carriers produced 0 characters.
/// **Every number in the PSK31 path was chosen and measured at 8 000 Hz.**</para>
/// <para>**SO THE RULE IS: THE DEVICE'S RATE IS RESAMPLED AT THE BOUNDARY AND NOTHING
/// PAST IT KNOWS WHAT THE DEVICE DID.** <see cref="Psk31Demodulator"/> and
/// <see cref="Psk31CarrierSearch"/> are not taught 48 kHz; they are fed the 8 kHz they
/// were proved at, and neither file is edited by this unit.</para>
/// <para>**AND THE PASSBAND IS THE MODE'S, 200 TO 3000 HZ**, which is a property of the
/// mode and the receiver rather than of the sound card.</para>
/// <para>**COMPUTED, NOT SEEN.** Every fixture here is synthetic and no radio was
/// involved (FACT-004, FACT-006). What this phase actually wants next is two minutes of
/// the operator's own 14.070.</para>
/// </remarks>
public sealed class ThePsk31PathRunsAtItsRateTests : IDisposable
{
    private const string OwnCall = "KC3QIS";

    /// <summary>The fixture unit 324 made, the four-signal file raised to the device rate.</summary>
    private const string AtFortyEight = "psk31-four-signals-48k.wav";

    /// <summary>How close a row must be to a fixture's carrier to be that carrier's row.</summary>
    private const double MatchHz = 20;

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the rows and the record are printed.</param>
    public ThePsk31PathRunsAtItsRateTests(ITestOutputHelper output)
    {
        _output = output;

        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-psk31-rate-" + Guid.NewGuid().ToString("N"));

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
    /// **Assertion 1: the four-signal fixture at 48 kHz gives the same four, each read.**
    /// </summary>
    /// <remarks>
    /// <para>**THE SAME FOUR CARRIERS WITHIN 5 HZ AND THE SAME TEXT AT OR UNDER 0.10**,
    /// which is the bar `ThePsk31HearsEveryoneTests` already holds the 8 kHz file to.</para>
    /// <para>**AND IT WAS WATCHED, AND IT DID NOT FAIL, AND THAT IS A FINDING** (§0.0).
    /// The work instruction expected this case to fail before the resampler was wired in.
    /// It was run with the wiring reverted - the listener opened at the tap's own 48 000 Hz
    /// and the samples handed to it unfiltered - and **all four rows still read their text
    /// at CER 0.0000**. The search and the demodulator derive their samples-per-symbol
    /// from the rate they are handed, so they scale, and unit 322 had already found the
    /// same thing from the other side when it fed this fixture raised to 48 kHz. **So the
    /// rate alone is not what silenced the operator's 893 Hz station**; what fails without
    /// the resampler is assertion 3, the record, which read `sampleRate: 48000`. The
    /// zero-characters fault is accounted for by the retire rule and is task 3's.</para>
    /// <para>**THE RESAMPLER IS STILL RIGHT AND IS NOT KEPT ON A GUESS.** The path is run
    /// at the one rate its thresholds were measured at rather than at whatever the sound
    /// card chose; the passband stops being a claim to search twenty-four kilohertz of a
    /// receiver that passes three; and the search does a sixth of the arithmetic per
    /// second of audio.</para>
    /// </remarks>
    [Fact]
    public void TheFourSignalFixtureAtFortyEightKilohertzIsReadLikeTheEightKilohertzOne()
    {
        var heard = Listen(AtFortyEight, "manifest-step2.json", 700, 1100, 1600, 2200);

        Assert.Empty(heard.Strays);

        foreach (var (hz, key) in new[] { (700.0, "A"), (1100.0, "B"), (1600.0, "C"), (2200.0, "D") })
        {
            var track = heard.Tracks[hz];
            var cer = ErrorRate(track.Text, Step2Text(key));

            _output.WriteLine(hz.ToString("0", CultureInfo.InvariantCulture) + " Hz CER "
                + Rate(cer) + " against " + key + ", appearances " + track.Appearances
                + ", " + track.Text.Length + " characters");

            Assert.Equal(1, track.Appearances);
            Assert.True(
                cer <= 0.10,
                hz.ToString("0", CultureInfo.InvariantCulture) + " Hz CER " + Rate(cer));
        }
    }

    /// <summary>**Assertion 2: noise at 48 kHz gives no carriers at all.**</summary>
    /// <remarks>
    /// **THE NOISE FIXTURE IS RAISED HERE RATHER THAN KEPT AS A THIRD FILE.** It is the
    /// file the manifest pins, its hash is checked before it is touched, and the raising is
    /// the same band-limited filter the 48 kHz four-signal file was made with - so a
    /// carrier found in it would be one the path invented.
    /// </remarks>
    [Fact]
    public void NoiseAtFortyEightKilohertzGivesNoRows()
    {
        const string file = "psk31-noise-only-30s.wav";

        HashMatches(file, "manifest.json");

        var eight = WavAudio.Read(Fixture(file));
        var raised = new MonoAudio(
            48_000, Ft8Resample.Resample(eight.Samples, eight.SampleRate, 48_000));

        var heard = Play(raised, new Heard(), file + " raised to 48 kHz");

        Assert.Equal(0, heard.MostRowsAtOnce);
        Assert.Empty(heard.Strays);
    }

    /// <summary>
    /// **Assertion 3: the record says both rates and the mode's passband.**
    /// </summary>
    /// <remarks>
    /// **TWO FIELDS, BECAUSE ONE COULD BE READ TWO WAYS** (§R13). The evening that found
    /// this fault wrote `sampleRate: 48000` and there was nothing in the line to say
    /// whether that was the device or the decoder. It was both, and that was the fault.
    /// </remarks>
    [Fact]
    public void TheRecordSaysBothRatesAndTheModesPassband()
    {
        var lines = WithTheRecord(AtFortyEight, seconds: 4);
        var started = Events(lines, "psk31_listening_started");

        Assert.Single(started);

        var one = started[0];

        _output.WriteLine("psk31_listening_started: "
            + "deviceSampleRate " + Number(one, "deviceSampleRate")
            + ", sampleRate " + Number(one, "sampleRate")
            + ", resampleRatio " + Number(one, "resampleRatio")
            + ", passband " + Number(one, "passbandLowHz")
            + " to " + Number(one, "passbandHighHz") + " Hz");

        Assert.Equal(48_000, Number(one, "deviceSampleRate"));
        Assert.Equal(Psk31Resampler.TargetSampleRate, Number(one, "sampleRate"));
        Assert.Equal(6, Number(one, "resampleRatio"));

        // **WITHIN ONE SPECTRUM BIN OF THE MODE'S OWN 200 AND 3000**, because the search
        // reads its passband back off the bins it really uses rather than printing the
        // numbers it was given (§0.0).
        var bin = Psk31Resampler.TargetSampleRate / 2048.0;

        Assert.InRange(
            Number(one, "passbandLowHz"),
            Psk31CarrierSearch.PassbandLowHz,
            Psk31CarrierSearch.PassbandLowHz + bin);

        Assert.InRange(
            Number(one, "passbandHighHz"),
            Psk31CarrierSearch.PassbandHighHz - bin,
            Psk31CarrierSearch.PassbandHighHz);
    }

    /// <summary>
    /// **Assertion 4, the unit's own: what is above the new Nyquist does not fold in.**
    /// </summary>
    /// <remarks>
    /// <para>**ASSERTION 1 CANNOT CATCH A NAIVE DECIMATOR AND THIS SAYS SO OUT LOUD**
    /// (§0.0). The 48 kHz fixture was raised with a band-limited filter, so it holds
    /// nothing above 4 kHz at all, and a path that came down to 8 kHz by keeping every
    /// sixth sample and filtering nothing would read it perfectly. **Real receiver audio
    /// at 48 kHz is not like that**: it carries hiss all the way up, and every bit of it
    /// above 4 kHz would fold onto the 200-3000 Hz the mode lives in. Folded noise has
    /// only two phases, so it squares as cleanly as BPSK and the search lists it as a
    /// station - which is how unit 322 got a fifth carrier at 3999.9 Hz reading 78
    /// characters of nothing.</para>
    /// <para>**SO THE FILTER IS PROVED DIRECTLY.** A 6 kHz tone at 48 kHz would fold to
    /// 2 kHz. What comes out at 2 kHz is measured against what a 2 kHz tone of the same
    /// size puts there.</para>
    /// </remarks>
    [Fact]
    public void WhatIsAboveTheNewNyquistDoesNotFoldIntoThePassband()
    {
        const int Device = 48_000;
        const double FoldsTo = 2_000;
        const double Above = 6_000;

        var wanted = Power(Through(Tone(FoldsTo, Device), Device), FoldsTo);
        var folded = Power(Through(Tone(Above, Device), Device), FoldsTo);

        var down = 10 * Math.Log10(folded / wanted);

        _output.WriteLine(
            "a " + Above.ToString("0", CultureInfo.InvariantCulture) + " Hz tone at "
            + Device + " Hz arrives at " + FoldsTo.ToString("0", CultureInfo.InvariantCulture)
            + " Hz " + down.ToString("0.0", CultureInfo.InvariantCulture)
            + " dB under a real one there");

        Assert.True(down < -60, "only " + down.ToString("0.0", CultureInfo.InvariantCulture) + " dB down");
    }

    /// <summary>
    /// **Assertion 5: at 8 kHz the resampler is not in the way at all.**
    /// </summary>
    /// <remarks>
    /// **A DEVICE ALREADY AT THE RIGHT RATE IS HANDED STRAIGHT BACK**, sample for sample,
    /// so nothing measured over five steps against the 8 kHz fixtures is now being read
    /// through a filter nobody asked for.
    /// </remarks>
    [Fact]
    public void AtTheRightRateNothingIsFiltered()
    {
        var resampler = new Psk31Resampler(Psk31Resampler.TargetSampleRate);
        var tone = Tone(1_000, Psk31Resampler.TargetSampleRate);

        Assert.True(resampler.PassesThrough);
        Assert.Equal(1, resampler.Ratio);
        Assert.Equal(Psk31Resampler.TargetSampleRate, resampler.SampleRate);

        var back = resampler.Take(tone).ToArray();

        Assert.Equal(tone.Length, back.Length);
        Assert.Equal(tone, back);
    }

    /// <summary>
    /// **Assertion 6: where the stream is cut makes no difference to what comes out.**
    /// </summary>
    /// <remarks>
    /// **THE TICK HANDS AUDIO OVER IN WHATEVER LUMPS THE DEVICE FILLED**, and a resampler
    /// that started its filter afresh on each lump would splice every quarter second. The
    /// same second of audio is pushed through in even quarter-second lumps and in ragged
    /// ones, and the two runs are compared sample for sample.
    /// </remarks>
    [Fact]
    public void TheAnswerDoesNotDependOnWhereTheStreamIsCut()
    {
        const int Device = 48_000;

        var tone = Tone(1_234, Device, seconds: 2);

        var even = Gather(tone, _ => Device / 4);
        var ragged = Gather(tone, at => 1 + ((at * 7919) % (Device / 3)));

        _output.WriteLine("even lumps " + even.Count + " samples, ragged lumps " + ragged.Count);

        Assert.Equal(even.Count, ragged.Count);

        for (var i = 0; i < even.Count; i++)
        {
            Assert.Equal(even[i], ragged[i]);
        }
    }

    /// <summary>Push audio through a resampler in lumps a rule chooses, and gather it.</summary>
    private static List<float> Gather(float[] samples, Func<int, int> lump)
    {
        var resampler = new Psk31Resampler(48_000);
        var got = new List<float>();

        for (var at = 0; at < samples.Length;)
        {
            var count = Math.Min(lump(at), samples.Length - at);

            got.AddRange(resampler.Take(samples.AsSpan(at, count)).ToArray());

            at += count;
        }

        return got;
    }

    /// <summary>A whole run of samples put on the PSK31 grid.</summary>
    private static float[] Through(float[] samples, int deviceRate)
        => new Psk31Resampler(deviceRate).Take(samples).ToArray();

    /// <summary>A unit-amplitude tone.</summary>
    private static float[] Tone(double hz, int rate, double seconds = 1)
    {
        var samples = new float[(int)(rate * seconds)];

        for (var n = 0; n < samples.Length; n++)
        {
            samples[n] = (float)Math.Sin(2 * Math.PI * hz * n / rate);
        }

        return samples;
    }

    /// <summary>How much power a run of samples holds at one frequency.</summary>
    /// <remarks>
    /// **A SINGLE-BIN GOERTZEL RATHER THAN A WHOLE SPECTRUM**, because one frequency is
    /// all that is being asked about and a window would only put its own skirts into the
    /// answer.
    /// </remarks>
    private static double Power(float[] samples, double hz)
    {
        double real = 0;
        double imaginary = 0;

        for (var n = 0; n < samples.Length; n++)
        {
            var angle = 2 * Math.PI * hz * n / Psk31Resampler.TargetSampleRate;

            real += samples[n] * Math.Cos(angle);
            imaginary += samples[n] * Math.Sin(angle);
        }

        var count = Math.Max(1, samples.Length);

        return ((real * real) + (imaginary * imaginary)) / ((double)count * count);
    }

    /// <summary>Play a fixture with the record running and hand back what it wrote.</summary>
    private List<string> WithTheRecord(string file, double seconds)
    {
        HashMatches(file, "manifest-step2.json");

        var audio = WavAudio.Read(Fixture(file));

        using (var telemetry = new JsonlTelemetry(_folder, "324", _ => true))
        {
            var model = Listening(telemetry);
            var chunk = audio.SampleRate / 4;
            var take = Math.Min(audio.Samples.Length, (int)(seconds * audio.SampleRate));

            for (var at = 0; at < take; at += chunk)
            {
                model.TapForTests!.Take(
                    audio.Samples.AsSpan(at, Math.Min(chunk, take - at)), audio.SampleRate);

                model.LookForASlotForTests();
            }
        }

        return Directory.GetFiles(_folder, "*.jsonl").SelectMany(File.ReadAllLines).ToList();
    }

    /// <summary>One carrier's row over a whole file.</summary>
    private sealed class Track
    {
        public Track(double expected) => Expected = expected;

        public double Expected { get; }

        public string Text { get; set; } = "";

        public int Appearances { get; set; }

        public bool Present { get; set; }
    }

    /// <summary>Everything the list did over one file.</summary>
    private sealed class Heard
    {
        public Dictionary<double, Track> Tracks { get; } = new();

        public HashSet<string> Strays { get; } = new();

        public int MostRowsAtOnce { get; set; }
    }

    /// <summary>Play a fixture through the real tap and tick and watch the list.</summary>
    private Heard Listen(string file, string manifest, params double[] carriers)
    {
        HashMatches(file, manifest);

        var heard = new Heard();

        foreach (var hz in carriers)
        {
            heard.Tracks[hz] = new Track(hz);
        }

        return Play(WavAudio.Read(Fixture(file)), heard, file);
    }

    private Heard Play(MonoAudio audio, Heard heard, string what)
    {
        var model = Listening(null);
        var chunk = audio.SampleRate / 4;

        for (var at = 0; at < audio.Samples.Length; at += chunk)
        {
            var count = Math.Min(chunk, audio.Samples.Length - at);

            model.TapForTests!.Take(audio.Samples.AsSpan(at, count), audio.SampleRate);
            model.LookForASlotForTests();

            Watch(model, heard);
        }

        _output.WriteLine(what + " at " + audio.SampleRate + " Hz: most rows at once "
            + heard.MostRowsAtOnce + ", strays " + heard.Strays.Count);

        foreach (var track in heard.Tracks.Values)
        {
            _output.WriteLine("  " + track.Expected.ToString("0", CultureInfo.InvariantCulture)
                + " Hz: " + track.Text.Length + " characters: " + Shown(track.Text));
        }

        foreach (var stray in heard.Strays)
        {
            _output.WriteLine("  stray: " + stray);
        }

        return heard;
    }

    private static void Watch(MainWindowViewModel model, Heard heard)
    {
        var rows = model.DigitalDecodes.ToList();

        heard.MostRowsAtOnce = Math.Max(heard.MostRowsAtOnce, rows.Count);

        var seen = new HashSet<double>();

        foreach (var row in rows)
        {
            var hz = double.TryParse(
                row.Hz, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
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
            }

            if (row.Message.Length > track.Text.Length)
            {
                track.Text = row.Message;
            }
        }

        foreach (var track in heard.Tracks.Values.Where(t => t.Present && !seen.Contains(t.Expected)))
        {
            track.Present = false;
        }
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
            .First(e => e.GetProperty("file").GetString() == "psk31-four-signals.wav")
            .GetProperty("texts").GetProperty(key).GetString() ?? "";
    }

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

    private static string Shown(string text)
    {
        var flat = text
            .Replace("\r", "", StringComparison.Ordinal)
            .Replace("\n", " / ", StringComparison.Ordinal);

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
