using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// <para>**THE TAB STAYS INERT** - `ThePsk31TabIsInertTests` is re-run after this - and a
    /// row whose text happens to be three words is still not an FT8 message with an addressee,
    /// a sender and a payload: no row ever has FT8 fields.</para>
    /// <para>**ONE ASSERTION HERE WAS STEP 2'S PLACEHOLDER, AND WORK INSTRUCTION 316 REPLACED
    /// IT WITH STEP 3'S RULE.** Unit 315 asserted that no row was ever put on the operator's
    /// side, because nothing read PSK31 yet and a row reading `KC3QIS de W1AW` would otherwise
    /// have been put there on the strength of three FT8 fields; `IsTextOnly`'s own remark said
    /// *until the step that parses PSK31 says otherwise*. Step 3's exit criterion is that a row
    /// addressed to his callsign **is** on his side, and this fixture's 1100 Hz row carries
    /// exactly that line. **What is asserted now is the rule, not its absence**: a row is on his
    /// side only while its latest message, as the PSK31 parser read it, is addressed to him. The
    /// count of rows put there is printed.</para>
    /// </remarks>
    [Fact]
    public void NoRowCanBeAnsweredAndNothingIsParsedOutOfOne()
    {
        var heard = Listen("psk31-four-signals.wav", "manifest-step2.json", 700, 1100, 1600, 2200);

        _output.WriteLine("rows at most at once          : " + heard.MostRowsAtOnce);
        _output.WriteLine("could be answered             : " + heard.EverAnswerable);
        _output.WriteLine("parsed into FT8 fields        : " + heard.EverParsed);
        _output.WriteLine("put on his side               : " + heard.EverForHim);
        _output.WriteLine("on his side, not addressed him: " + heard.EverForHimWithoutBeingAddressed);

        Assert.True(heard.MostRowsAtOnce >= 2, "never more than " + heard.MostRowsAtOnce + " row at once");
        Assert.False(heard.EverAnswerable);
        Assert.False(heard.EverParsed);
        Assert.False(heard.EverForHimWithoutBeingAddressed);
    }

    /// <summary>**Task 5: two carriers a hundred hertz apart, both read.**</summary>
    /// <remarks>
    /// <para>**THE NICE-TO-PASS.** Equal level at 1000 and 1100 Hz; the author's reference
    /// read them at 0.0000 and 0.0083. `manifest-step2.json` gives the file two texts, `A`
    /// and `C`, and keys its error rates by 1000 and 1100 without saying which text is on
    /// which; `A` at 1000 and `C` at 1100 is the order they are listed in and the order
    /// the four-signal file puts them in, and it is printed as the pairing used.</para>
    /// <para>**NOT WATCHED FAILING**: the listener it runs was built and committed in task
    /// 3 before this case was written.</para>
    /// </remarks>
    [Fact]
    public void TwoCarriersAHundredHertzApartAreBothRead()
    {
        var heard = Listen("psk31-two-signals-100hz-apart.wav", "manifest-step2.json", 1000, 1100);

        var low = heard.Tracks[1000];
        var high = heard.Tracks[1100];

        Assert.Empty(heard.Strays);
        Assert.Equal(1, low.Appearances);
        Assert.Equal(1, high.Appearances);

        var lowCer = ErrorRate(low.Text, Step2Text("A"));
        var highCer = ErrorRate(high.Text, Step2Text("C"));

        _output.WriteLine("1000 Hz CER " + Rate(lowCer) + " against A, reference 0.0000");
        _output.WriteLine("1100 Hz CER " + Rate(highCer) + " against C, reference 0.0083");

        Assert.True(lowCer <= 0.05, "1000 Hz CER " + Rate(lowCer));
        Assert.True(highCer <= 0.05, "1100 Hz CER " + Rate(highCer));

        Assert.DoesNotContain("EI4GNB", low.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("KC3QIS", high.Text, StringComparison.Ordinal);
    }

    /// <summary>**Task 4: the search and every demodulator keep up with the audio.**</summary>
    /// <remarks>
    /// <para>**THE FOUR-SIGNAL FIXTURE, END TO END, AS A STREAM**, through the real tap and
    /// the real tick in quarter-second lumps, exactly as the panel runs it - search, four
    /// demodulators, the rows and the list. The ratio is the time that took over the
    /// audio's own length; under 1.0 is keeping up.</para>
    /// <para>**THE NUMBER IS ELAPSED TIME ON ONE THREAD**, which is never less than that
    /// thread's CPU time, so a ratio under one here is a CPU ratio under one. The
    /// process's CPU is printed beside it and is larger, because xUnit runs other test
    /// classes at the same time.</para>
    /// <para>**WHAT DOMINATES IS MEASURED, NOT GUESSED**: the listener alone, the search
    /// alone, and four demodulators alone over the same audio. **And the same listener at
    /// 48 kHz**, which is what a sound card commonly delivers and what the tap is fed at
    /// in the application, over the fixture raised to that rate with a windowed-sinc
    /// interpolator - printed, not asserted, because no fixture was made at that rate.</para>
    /// <para>**AN INDICATION ON THIS MACHINE, NEVER A FINDING** (FACT-004). This test was
    /// not watched failing: the code it times was built and committed in task 3, and a
    /// timing that fails only against a slower build is not something that can honestly
    /// be staged.</para>
    /// </remarks>
    [Fact]
    public void TheSearchAndEveryDemodulatorKeepUpWithTheAudio()
    {
        const string file = "psk31-four-signals.wav";

        HashMatches(file, "manifest-step2.json");

        var audio = WavAudio.Read(Fixture(file));
        var audioSeconds = audio.Duration.TotalSeconds;
        var chunk = audio.SampleRate / 4;
        var model = Listening();

        var process = Process.GetCurrentProcess();
        var cpuBefore = process.TotalProcessorTime;
        var watch = Stopwatch.StartNew();

        for (var at = 0; at < audio.Samples.Length; at += chunk)
        {
            var count = Math.Min(chunk, audio.Samples.Length - at);

            model.TapForTests!.Take(audio.Samples.AsSpan(at, count), audio.SampleRate);
            model.LookForASlotForTests();
        }

        watch.Stop();
        process.Refresh();

        var panelSeconds = watch.Elapsed.TotalSeconds;
        var processCpu = (process.TotalProcessorTime - cpuBefore).TotalSeconds;
        var ratio = panelSeconds / audioSeconds;

        var listenerSeconds = Time(() =>
        {
            var listener = new Psk31Listener(audio.SampleRate);
            Feed(audio.Samples, chunk, s => listener.Add(s));
        });

        var searchSeconds = Time(() =>
        {
            var search = new Psk31CarrierSearch(audio.SampleRate);
            Feed(audio.Samples, chunk, s => search.Add(s));
        });

        var demodulatorSeconds = Time(() =>
        {
            foreach (var hz in new[] { 700.0, 1100.0, 1600.0, 2200.0 })
            {
                var demodulator = new Psk31Demodulator(audio.SampleRate, hz);
                Feed(audio.Samples, chunk, s => demodulator.Add(s));
            }
        });

        const int Rise = 6;
        var raised = Raise(audio.Samples, Rise);
        var raisedRate = audio.SampleRate * Rise;
        var carriersAt48 = 0;
        var channelsAt48 = new Dictionary<int, (double Hz, int Looks, string Text)>();

        var at48Seconds = Time(() =>
        {
            var listener = new Psk31Listener(raisedRate);
            Feed(raised, raisedRate / 4, s =>
            {
                listener.Add(s);
                carriersAt48 = Math.Max(carriersAt48, listener.Channels.Count);

                foreach (var channel in listener.Channels)
                {
                    var looks = channelsAt48.TryGetValue(channel.Id, out var seen) ? seen.Looks : 0;
                    channelsAt48[channel.Id] = (channel.OffsetHz, looks + 1, channel.Text);
                }
            });
        });

        foreach (var (id, seen) in channelsAt48.OrderBy(pair => pair.Value.Hz))
        {
            _output.WriteLine("  at 48 kHz, id " + id + " at " + seen.Hz.ToString("0.0", CultureInfo.InvariantCulture)
                + " Hz, listed for " + Seconds(seen.Looks / 4.0) + ", read " + seen.Text.Length + ": " + Shown(seen.Text));
        }

        _output.WriteLine("audio                      : " + Seconds(audioSeconds) + " at " + audio.SampleRate + " Hz");
        _output.WriteLine("the panel, end to end      : " + Seconds(panelSeconds) + ", ratio " + Ratio(ratio));
        _output.WriteLine("process CPU over that span : " + Seconds(processCpu) + " (every thread in the test host)");
        _output.WriteLine("the listener alone         : " + Seconds(listenerSeconds) + ", ratio " + Ratio(listenerSeconds / audioSeconds));
        _output.WriteLine("  the search alone         : " + Seconds(searchSeconds) + ", ratio " + Ratio(searchSeconds / audioSeconds));
        _output.WriteLine("  four demodulators alone  : " + Seconds(demodulatorSeconds) + ", ratio " + Ratio(demodulatorSeconds / audioSeconds));
        _output.WriteLine("the listener at " + raisedRate + " Hz : " + Seconds(at48Seconds) + ", ratio "
            + Ratio(at48Seconds / audioSeconds) + ", most channels at once " + carriersAt48 + " (printed, not asserted)");

        Assert.True(ratio < 1.0, "the panel took " + Seconds(panelSeconds) + " for " + Seconds(audioSeconds) + " of audio");
    }

    private static double Time(Action work)
    {
        var watch = Stopwatch.StartNew();
        work();
        return watch.Elapsed.TotalSeconds;
    }

    private delegate void SpanAction(ReadOnlySpan<float> samples);

    private static void Feed(float[] samples, int chunk, SpanAction add)
    {
        for (var at = 0; at < samples.Length; at += chunk)
        {
            add(samples.AsSpan(at, Math.Min(chunk, samples.Length - at)));
        }
    }

    /// <summary>Raise audio to a whole multiple of its rate, with a windowed-sinc interpolator.</summary>
    /// <remarks>
    /// **EIGHT ZERO CROSSINGS A SIDE, HANN-WINDOWED.** The kernel is one at its centre and
    /// zero at every other original sample, so every original sample comes through
    /// unchanged, and the images a cruder interpolator leaves above the old Nyquist - each
    /// a keyed copy of a PSK31 signal - are pushed down into the noise rather than left
    /// for the search to find.
    /// </remarks>
    private static float[] Raise(float[] samples, int factor)
    {
        var half = 8 * factor;
        var kernel = new double[(2 * half) + 1];

        for (var j = -half; j <= half; j++)
        {
            var x = Math.PI * j / factor;
            var sinc = j == 0 ? 1.0 : Math.Sin(x) / x;
            var window = 0.5 + (0.5 * Math.Cos(Math.PI * j / half));

            kernel[j + half] = sinc * window;
        }

        var raised = new float[samples.Length * factor];

        for (var k = 0; k < samples.Length; k++)
        {
            var sample = samples[k];

            for (var j = -half; j <= half; j++)
            {
                var n = (k * factor) + j;

                if (n >= 0 && n < raised.Length)
                {
                    raised[n] += (float)(sample * kernel[j + half]);
                }
            }
        }

        return raised;
    }

    private static string Ratio(double ratio) => ratio.ToString("0.000", CultureInfo.InvariantCulture);

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

        /// <summary>A row on his side whose latest message was not addressed to him.</summary>
        public bool EverForHimWithoutBeingAddressed { get; set; }
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
            // **WHETHER A ROW OFFERS A SEND, NOT WHETHER THE MODE CAN SEND** (§R12, work
            // instruction 323 task 1c). This read `CanAnswerRowsForTests`, which is
            // `CanTransmitIn` - the shut door - and the door is open from this unit on.
            // What the assertion is named for is that **no click on a row here reaches a
            // send path**, and that is what `SendMenuFor` answers.
            heard.EverAnswerable |= rows.Any(r => model.SendMenuFor(r) is not null);
            heard.EverParsed |= rows.Any(r => r.HasFields);
            heard.EverForHim |= model.DigitalMineDecodes.Count > 0;
            heard.EverForHimWithoutBeingAddressed |=
                model.DigitalMineDecodes.Any(r => r.Reading is not { IsForOperator: true });
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
