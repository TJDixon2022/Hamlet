using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Psk31;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Psk31;

/// <summary>
/// Work instruction 327 task 1: **a station stays on the list while he is idling.**
/// </summary>
/// <remarks>
/// <para>**THE FAULT THIS EXISTS FOR WAS READ OUT OF THE OPERATOR'S OWN RECORD.** On
/// 2026-09-12, 7.070, 00:54 to 00:57, a station at **2073 Hz** with the squelch open at
/// **quality 0.99** - as clean as any fixture - appeared **six times in two minutes** and
/// was retired each time as `SignalGone` after one to five seconds, zero characters. Two
/// hundred and six seconds, twelve carriers, **nought lines**.</para>
/// <para>**AND THE PHYSICS SAYS WHY.** A PSK31 station between words sends continuous phase
/// reversals, and **an idling BPSK signal has no energy at its carrier frequency at all**:
/// it splits into two lines 15.6 Hz either side. A search that asks the spectrum *is there
/// a signal at 2073 Hz* gets **no** every time he stops typing, and the station vanishes
/// and reappears in step with his keyboard. His demodulator, which measures keying shape
/// rather than energy at a point, rates the same signal 0.99 straight through.</para>
/// <para>**SO A CARRIER IS NOW KEPT BY THE SECOND OPINION.** The spectrum and the
/// demodulator must **both** have lost it before it is retired
/// (<see cref="Psk31CarrierSearch.SearchRule"/>), and the peak the spectrum nominates on is
/// walked to the balance point of the two sidebands so an idling station is **found** and
/// not merely kept.</para>
/// <para>**COMPUTED, NOT SEEN.** Every fixture behind every number here is synthetic and no
/// radio was involved (FACT-004, FACT-006).</para>
/// </remarks>
public sealed class ThePsk31StationIdlesTests
{
    /// <summary>The fixture this unit made: type ten, idle eight, type ten.</summary>
    private const string IdleEight = "psk31-idle-8s-1000hz.wav";

    /// <summary>How the audio is handed over, in seconds, as the shell hands it.</summary>
    private const double ChunkSeconds = 0.25;

    /// <summary>How close a reading must be to the fixture's carrier to be it, in hertz.</summary>
    private const double Within = 15;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the carriers and the measurements are printed.</param>
    public ThePsk31StationIdlesTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **Assertion 1: eight seconds of idle is one carrier, retired once, at the end.**
    /// </summary>
    /// <remarks>
    /// **THIS IS THE WHOLE UNIT IN ONE TEST.** One id for the whole file, not two and not
    /// six; the retirement after the station actually stops, not during the gap; and it is
    /// held right across the gap rather than dropped and found again at the same offset,
    /// which is what the operator's record shows happening on the air.
    /// </remarks>
    [Fact]
    public void TheEightSecondIdleYieldsOneCarrierForTheWholeFile()
    {
        var heard = Listen(IdleEight, out var seconds, out var stops);

        Assert.Single(heard);

        var one = heard.Values.Single();

        Assert.InRange(one.MedianHz, 1000 - Within, 1000 + Within);

        // **HELD FROM THE FIRST WORDS TO PAST THE LAST**, which is what *across the gap*
        // means in numbers: the gap runs from about 11 s to about 19 s and the carrier is
        // listed on both sides of it and at every reading in between.
        Assert.True(
            one.FirstSeconds < 5,
            "first listed at " + one.FirstSeconds.ToString("0.0", CultureInfo.InvariantCulture) + " s");

        Assert.True(
            one.LastSeconds > stops,
            "last listed at " + one.LastSeconds.ToString("0.0", CultureInfo.InvariantCulture)
            + " s, and the station keyed until " + stops.ToString("0.0", CultureInfo.InvariantCulture));

        // **AND IT WAS NEVER ABSENT IN BETWEEN.** A carrier that left and came back with
        // the same id is impossible - ids are never reused - but one that left and came
        // back as a second id would have shown up as a second entry above.
        Assert.Equal(one.Readings, one.LastReadingIndex - one.FirstReadingIndex + 1);

        // **AND IT IS GONE BEFORE THE FILE IS.** The tail is ten seconds; the bound is 6.0.
        Assert.True(
            one.LastSeconds < seconds - 1,
            "still listed at " + one.LastSeconds.ToString("0.0", CultureInfo.InvariantCulture)
            + " s of " + seconds.ToString("0.0", CultureInfo.InvariantCulture));

        Assert.True(
            one.LastSeconds - stops <= Psk31Listener.RetiredWithinSeconds,
            "the row outlived its signal by "
            + (one.LastSeconds - stops).ToString("0.0", CultureInfo.InvariantCulture) + " s");
    }

    /// <summary>
    /// **Assertion 2: both CQ lines are read, at or under a character error rate of 0.05.**
    /// </summary>
    /// <remarks>
    /// **THE GAP MUST NOT COST THE SECOND HALF.** A carrier held across the idle keeps one
    /// demodulator with its bit clock and its AFC already settled, so the words after the
    /// gap should read as well as the words before it - which a carrier retired and remade
    /// in the middle could not do, because the first two seconds of any new channel are
    /// spent settling.
    /// </remarks>
    [Fact]
    public void BothHalvesAreReadAtOrUnderFiveHundredthsError()
    {
        var text = Read(IdleEight);

        _output.WriteLine("read: \"" + text.Replace("\r\n", "\\r\\n", StringComparison.Ordinal) + "\"");

        var want = Texts(IdleEight);
        var whole = string.Concat(want);

        var error = Distance(Squeeze(whole), Squeeze(text)) / (double)Squeeze(whole).Length;

        _output.WriteLine(
            "expected " + Squeeze(whole).Length + " characters, character error rate "
            + error.ToString("0.000", CultureInfo.InvariantCulture));

        Assert.True(error <= 0.05, "character error rate " + error);

        // **AND BOTH HALVES ARE THERE**, not one of them twice as well as it should be.
        Assert.True(
            Occurrences(Squeeze(text), "KC3QIS") >= 6,
            "only " + Occurrences(Squeeze(text), "KC3QIS") + " of the six callsigns");
    }

    /// <summary>
    /// **Assertion 3: an idling event and a typing event bracket the gap.**
    /// </summary>
    /// <remarks>
    /// **THE FILE HAS TO SHOW THE IDLE GAP AS AN IDLE GAP** (§0.0). Without these two the
    /// record of a station who paused and the record of a station who left and came back
    /// are the same three facts in the same order.
    /// </remarks>
    [Fact]
    public void AnIdlingEventAndATypingEventBracketTheGap()
    {
        var activity = Play(IdleEight);

        foreach (var one in activity)
        {
            _output.WriteLine(
                (one.Idling ? "idling " : "typing ")
                + "id " + one.Id
                + " at " + one.OffsetHz.ToString("0.0", CultureInfo.InvariantCulture) + " Hz"
                + ", quality " + one.Quality.ToString("0.000", CultureInfo.InvariantCulture)
                + ", after " + one.Seconds.ToString("0.0", CultureInfo.InvariantCulture) + " s");
        }

        var idling = activity.Where(a => a.Idling).ToList();
        var typing = activity.Where(a => !a.Idling).ToList();

        Assert.NotEmpty(idling);
        Assert.NotEmpty(typing);

        // **THE ORDER IS THE CLAIM.** He stopped, then he started again, and the typing
        // event comes after the idling one rather than in some other arrangement.
        Assert.True(
            activity.IndexOf(idling[0]) < activity.IndexOf(typing[0]),
            "typing was recorded before idling");

        // **AND HIS SQUELCH WAS OPEN WHILE HE IDLED**, which is what says the carrier was
        // there and keying rather than gone. Idle is continuous reversals, so the keying
        // measure reads near its clean-keying end straight through the gap.
        Assert.True(
            idling[0].Quality >= Psk31CarrierSearch.KeepReadableQuality,
            "he idled at quality " + idling[0].Quality);
    }

    /// <summary>
    /// **Assertion 4: the four-signal fixture still yields four and the noise-only none.**
    /// </summary>
    /// <remarks>
    /// **A KEEP RULE THAT KEPT ANYTHING WOULD PASS EVERY TEST ABOVE.** The two fixtures
    /// that bound it from the other side are run here in the same session as the ones
    /// above, so a change that bought the idle by loosening the search is caught in the
    /// same run rather than in a later unit.
    /// </remarks>
    [Fact]
    public void TheFourSignalFixtureStillYieldsFourAndTheNoiseOnlyNone()
    {
        var four = Listen("psk31-four-signals.wav", out _, out _);

        Assert.Equal(4, four.Count);

        var noise = Listen("psk31-noise-only-30s.wav", out _, out _, "manifest.json");

        Assert.Empty(noise);
    }

    /// <summary>
    /// **Assertion 5: the rule names both halves, and the numbers live in one place.**
    /// </summary>
    [Fact]
    public void TheKeepRuleAndItsNumbersLiveInOneNamedPlace()
    {
        _output.WriteLine("search  : " + Psk31CarrierSearch.SearchRule);
        _output.WriteLine("retire  : " + Psk31Listener.RetireRule);
        _output.WriteLine(
            "quality : " + Psk31CarrierSearch.KeepReadableQuality.ToString(
                "0.00", CultureInfo.InvariantCulture)
            + "  (noise 0.637, clean 1.0, squelch "
            + Psk31Demodulator.SquelchQuality.ToString("0.00", CultureInfo.InvariantCulture) + ")");
        _output.WriteLine(
            "within  : " + Psk31CarrierSearch.KeepReadableSeconds.ToString(
                "0.000", CultureInfo.InvariantCulture) + " s");

        // **THE BAR SITS BETWEEN NOISE AND THE SQUELCH**, which is the whole argument for
        // the number: above it nothing but a real signal can be, below it a station whose
        // squelch has just shut still is.
        Assert.InRange(
            Psk31CarrierSearch.KeepReadableQuality, 0.70, Psk31Demodulator.SquelchQuality);

        Assert.Equal(
            Psk31CarrierSearch.MeasureSymbols / Psk31Demodulator.Baud,
            Psk31CarrierSearch.KeepReadableSeconds,
            6);

        foreach (var rule in new[] { Psk31CarrierSearch.SearchRule, Psk31Listener.RetireRule })
        {
            Assert.Contains(nameof(Psk31CarrierSearch.KeepReadableQuality), rule, StringComparison.Ordinal);
            Assert.Contains(nameof(Psk31CarrierSearch.KeepReadableSeconds), rule, StringComparison.Ordinal);
        }

        Assert.Contains("idling", Psk31Listener.RetireRule, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// **Assertion 6: an idling station is found at its center, not sixteen hertz low.**
    /// </summary>
    /// <remarks>
    /// <para>**FOUND, NOT ONLY KEPT** (the work instruction asks for both, and this is the
    /// answer to the *found* half). An idling BPSK signal has **no energy at its carrier
    /// frequency at all** - it is two lines 15.6 Hz either side - so a search that looked
    /// for a spectral peak at the center would find nothing. Two things stop that, and both
    /// were already in place before this unit: the power the candidate test sums covers
    /// <see cref="Psk31CarrierSearch.SignalHalfWidthHz"/>, which is 32 Hz and takes in both
    /// lines, so the place raises a candidate; and the nomination is the **balance point**
    /// of the power over the floor rather than the tallest bin, so it lands between them.
    /// </para>
    /// <para>**THIS TEST IS THAT CLAIM MEASURED, AND IT IS THE EVIDENCE FOR IT.** Five
    /// seconds cut out of the middle of the gap - nothing in it but reversals, no words at
    /// either end to help - and the search is asked what it finds.</para>
    /// </remarks>
    [Fact]
    public void AnIdlingStationIsNominatedAtTheBalancePointOfItsTwoSidebands()
    {
        var audio = WavAudio.Read(Fixture(IdleEight));
        var search = new Psk31CarrierSearch(audio.SampleRate);

        // **THE MIDDLE OF THE GAP**, where there is nothing but reversals: the file's gap
        // runs from about 11.3 s to about 19.3 s, so 15 s is inside it by three seconds
        // either way and the probe has a full measure of pure idle to work on.
        var from = (int)(13.0 * audio.SampleRate);
        var to = (int)(18.0 * audio.SampleRate);

        search.Add(audio.Samples.AsSpan(from, to - from));

        var listed = search.Carriers.ToList();

        foreach (var carrier in listed)
        {
            _output.WriteLine(
                "idle-only: id " + carrier.Id + " at "
                + carrier.OffsetHz.ToString("0.0", CultureInfo.InvariantCulture) + " Hz");
        }

        var found = Assert.Single(listed);

        // **WITHIN A FEW HERTZ OF 1000, NOT AT 984.** The old plateau rule put it a whole
        // half-baud low, where the squared phasor's angle stops telling which way the error
        // runs.
        Assert.InRange(found.OffsetHz, 1000 - 5, 1000 + 5);
    }

    /// <summary>
    /// **Assertion 7: how long the demodulator goes on vouching after the carrier stops.**
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS THE PRICE OF THE KEEP RULE AND IT IS MEASURED, NOT ASSUMED** (§0.0).
    /// <see cref="Psk31Demodulator.Quality"/> is a mean of |Re(d)| over a mean of |d|, both
    /// rolling over <see cref="Psk31Demodulator.QualityWindow"/> symbols, so it is
    /// **weighted by magnitude**: when a loud carrier stops, its own loud symbols hold the
    /// ratio up while they decay out of the window, and the louder it was the longer that
    /// takes. **A row can therefore outlive its signal by that long plus
    /// <see cref="Psk31CarrierSearch.KeepReadableSeconds"/>**, and
    /// <see cref="Psk31Listener.RetiredWithinSeconds"/> is the bound that has to cover
    /// it.</para>
    /// <para>**IT IS ASSERTED AGAINST THE BOUND AND NOT AGAINST A FIGURE TYPED HERE**, so
    /// the test says *the number the class promises is the number the audio gives* rather
    /// than pinning today's measurement as a fact about PSK31.</para>
    /// </remarks>
    [Fact]
    public void TheDemodulatorLetsGoInsideTheBoundTheListenerPromises()
    {
        var audio = WavAudio.Read(Fixture(IdleEight));
        var stops = Unit327Fixtures.StopsAtSeconds();
        var listener = new Psk31Listener(audio.SampleRate);
        var chunk = (int)(audio.SampleRate * ChunkSeconds);

        double? letGo = null;
        double? shut = null;

        for (var at = 0; at < audio.Samples.Length; at += chunk)
        {
            listener.Add(audio.Samples.AsSpan(at, Math.Min(chunk, audio.Samples.Length - at)));

            var now = (double)(at + chunk) / audio.SampleRate;
            var state = listener.States.FirstOrDefault();

            if (state.Id == 0 || now <= stops)
            {
                continue;
            }

            if (shut is null && !state.Open)
            {
                shut = now - stops;
            }

            if (letGo is null && state.Quality < Psk31CarrierSearch.KeepReadableQuality)
            {
                letGo = now - stops;

                _output.WriteLine(
                    "quality fell under " + Psk31CarrierSearch.KeepReadableQuality.ToString(
                        "0.00", CultureInfo.InvariantCulture)
                    + " at " + letGo.Value.ToString("0.00", CultureInfo.InvariantCulture)
                    + " s after the carrier stopped; the squelch shut at "
                    + (shut?.ToString("0.00", CultureInfo.InvariantCulture) ?? "never"));
            }
        }

        Assert.NotNull(letGo);

        // **THE VOUCH IS WAITED OUT AFTER THAT**, so the whole reprieve is the two added,
        // and the bound the listener promises has to cover the pair.
        Assert.True(
            letGo!.Value + Psk31CarrierSearch.KeepReadableSeconds
                <= Psk31Listener.RetiredWithinSeconds,
            "the demodulator vouched for " + letGo.Value + " s after the carrier stopped, plus "
            + Psk31CarrierSearch.KeepReadableSeconds + " s of vouch, against a promised bound of "
            + Psk31Listener.RetiredWithinSeconds + " s");
    }

    /// <summary>What the search said about one carrier over a whole file.</summary>
    private sealed record Heard(
        int Id,
        List<double> Hz,
        double FirstSeconds,
        double LastSeconds,
        int FirstReadingIndex,
        int LastReadingIndex,
        int Readings)
    {
        public double MedianHz
        {
            get
            {
                var sorted = Hz.OrderBy(v => v).ToList();

                return sorted.Count == 0 ? double.NaN : sorted[sorted.Count / 2];
            }
        }
    }

    /// <summary>Stream a fixture through the listener and gather every carrier it listed.</summary>
    private Dictionary<int, Heard> Listen(
        string file, out double seconds, out double stops, string manifest = "manifest-step2.json")
    {
        HashMatches(file, manifest);

        var audio = WavAudio.Read(Fixture(file));
        var listener = new Psk31Listener(audio.SampleRate);
        var heard = new Dictionary<int, Heard>();

        var chunk = (int)(audio.SampleRate * ChunkSeconds);
        var index = 0;

        seconds = audio.Samples.Length / (double)audio.SampleRate;
        stops = file == IdleEight ? Unit327Fixtures.StopsAtSeconds() : seconds;

        for (var at = 0; at < audio.Samples.Length; at += chunk, index++)
        {
            var count = Math.Min(chunk, audio.Samples.Length - at);

            listener.Add(audio.Samples.AsSpan(at, count));

            var now = (double)(at + count) / audio.SampleRate;

            foreach (var channel in listener.Channels)
            {
                if (!heard.TryGetValue(channel.Id, out var one))
                {
                    one = new Heard(
                        channel.Id, new List<double>(), now, now, index, index, 0);
                }

                one.Hz.Add(channel.OffsetHz);

                heard[channel.Id] = one with
                {
                    LastSeconds = now,
                    LastReadingIndex = index,
                    Readings = one.Readings + 1,
                };
            }
        }

        _output.WriteLine(
            file + ": " + heard.Count + " carrier(s) over "
            + seconds.ToString("0.0", CultureInfo.InvariantCulture) + " s, station stops at "
            + stops.ToString("0.0", CultureInfo.InvariantCulture) + " s");

        foreach (var one in heard.Values.OrderBy(h => h.MedianHz))
        {
            _output.WriteLine(
                "  id " + one.Id
                + "  median " + one.MedianHz.ToString("0.0", CultureInfo.InvariantCulture) + " Hz"
                + "  listed " + one.FirstSeconds.ToString("0.00", CultureInfo.InvariantCulture)
                + " to " + one.LastSeconds.ToString("0.00", CultureInfo.InvariantCulture) + " s"
                + "  on " + one.Readings + " of "
                + (one.LastReadingIndex - one.FirstReadingIndex + 1) + " readings in between");
        }

        return heard;
    }

    /// <summary>Everything the listener read out of a fixture, in order.</summary>
    private static string Read(string file)
    {
        var audio = WavAudio.Read(Fixture(file));
        var listener = new Psk31Listener(audio.SampleRate);
        var chunk = (int)(audio.SampleRate * ChunkSeconds);
        var text = new Dictionary<int, string>();

        for (var at = 0; at < audio.Samples.Length; at += chunk)
        {
            listener.Add(audio.Samples.AsSpan(at, Math.Min(chunk, audio.Samples.Length - at)));

            foreach (var channel in listener.Channels)
            {
                text[channel.Id] = channel.Text;
            }
        }

        return string.Concat(text.OrderBy(p => p.Key).Select(p => p.Value));
    }

    /// <summary>Every idling and typing event a fixture produced, in order.</summary>
    private static List<Psk31CarrierActivity> Play(string file)
    {
        var audio = WavAudio.Read(Fixture(file));
        var listener = new Psk31Listener(audio.SampleRate);
        var chunk = (int)(audio.SampleRate * ChunkSeconds);
        var activity = new List<Psk31CarrierActivity>();

        for (var at = 0; at < audio.Samples.Length; at += chunk)
        {
            listener.Add(audio.Samples.AsSpan(at, Math.Min(chunk, audio.Samples.Length - at)));

            activity.AddRange(listener.Watch.DrainActivity());
        }

        return activity;
    }

    private static string Squeeze(string text)
        => text.Replace("\r", "", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal);

    private static int Occurrences(string text, string word)
    {
        var count = 0;
        var at = 0;

        while ((at = text.IndexOf(word, at, StringComparison.Ordinal)) >= 0)
        {
            count++;
            at += word.Length;
        }

        return count;
    }

    /// <summary>Levenshtein distance, which is what a character error rate counts.</summary>
    private static int Distance(string want, string got)
    {
        var previous = new int[got.Length + 1];
        var current = new int[got.Length + 1];

        for (var j = 0; j <= got.Length; j++)
        {
            previous[j] = j;
        }

        for (var i = 1; i <= want.Length; i++)
        {
            current[0] = i;

            for (var j = 1; j <= got.Length; j++)
            {
                current[j] = Math.Min(
                    Math.Min(previous[j] + 1, current[j - 1] + 1),
                    previous[j - 1] + (want[i - 1] == got[j - 1] ? 0 : 1));
            }

            (previous, current) = (current, previous);
        }

        return previous[got.Length];
    }

    private static List<string> Texts(string file)
    {
        using var document = JsonDocument.Parse(
            File.ReadAllText(Fixture("manifest-step2.json")));

        return document.RootElement.EnumerateArray()
            .Single(e => e.GetProperty("file").GetString() == file)
            .GetProperty("texts")
            .EnumerateObject()
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .Select(p => p.Value.GetString() ?? "")
            .ToList();
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
