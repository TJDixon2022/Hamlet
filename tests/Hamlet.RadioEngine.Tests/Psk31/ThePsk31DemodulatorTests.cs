using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Psk31;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Psk31;

/// <summary>
/// Work instruction 314 task 3: **the demodulator, proved against the fixtures.**
/// </summary>
/// <remarks>
/// <para>**ONE CHANNEL AT ONE OFFSET.** Samples at 8 kHz and an offset in hertz go in,
/// characters come out. It knows nothing about tabs, radios or panels (§0.1), and
/// finding signals across the passband is the next step's work.</para>
/// <para>**THE FIXTURES ARE THE AUTHOR'S, MADE ON A MACHINE WITH NO RADIO** (FACT-004,
/// FACT-006). Every number here is an indication, never a finding, and the SNR figures
/// are referenced to 2500 Hz the way an FT8 report is - so `-3 dB` is about `+16 dB`
/// inside PSK31's own 31 Hz. **These are not weak-signal fixtures.**</para>
/// <para>**CHARACTER ERROR RATE** is edit distance against `qso-text.txt` over the
/// reference length, after trimming leading and trailing idle. The author's offline
/// reference decoder scored 0.0000 on every QSO fixture; the ceilings here are looser
/// because a squelched streaming decoder with a live AFC loop is a harder thing.</para>
/// <para>**COMPUTED, NOT SEEN**, and no radio was involved in any of it.</para>
/// </remarks>
public sealed class ThePsk31DemodulatorTests
{
    private const double Offset = 1000;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the error rates are printed.</param>
    public ThePsk31DemodulatorTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Every fixture is the file the manifest says it is.**</summary>
    /// <remarks>
    /// **A FIXTURE BUILT FROM THE SAME MISUNDERSTANDING AS THE CODE PROVES NOTHING**
    /// (§12.5), and a fixture that has quietly changed proves less. The hash is checked
    /// before a single sample is read.
    /// </remarks>
    [Fact]
    public void EveryFixtureIsTheFileTheManifestSaysItIs()
    {
        foreach (var entry in Manifest())
        {
            var path = Fixture(entry.File);

            Assert.True(File.Exists(path), "no fixture " + entry.File);

            using var stream = File.OpenRead(path);

            var hash = Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();

            _output.WriteLine(entry.File.PadRight(34) + hash[..16] + "…");

            Assert.Equal(entry.Sha256, hash);
        }
    }

    /// <summary>**The squelch rule and its number live in one named place.**</summary>
    [Fact]
    public void TheSquelchRuleAndItsNumberLiveInOneNamedPlace()
    {
        _output.WriteLine("rule   : " + Psk31Demodulator.SquelchRule);
        _output.WriteLine("number : " + Psk31Demodulator.SquelchQuality
            .ToString("0.00", CultureInfo.InvariantCulture));

        Assert.False(string.IsNullOrWhiteSpace(Psk31Demodulator.SquelchRule));

        // **THE NUMBER HAS TO SIT BETWEEN THE TWO ENDS OF THE MEASURE**: above what
        // uniform noise phase averages and below what clean keying gives, or it is
        // either a gate that never opens or one that never shuts.
        const double noise = 2.0 / Math.PI;

        _output.WriteLine("uniform noise phase averages "
            + noise.ToString("0.000", CultureInfo.InvariantCulture)
            + ", clean keying 1.000");

        Assert.True(
            Psk31Demodulator.SquelchQuality > noise,
            "the squelch number is at or below what noise alone produces");

        Assert.True(
            Psk31Demodulator.SquelchQuality < 1.0,
            "the squelch number is at or above what only a perfect signal reaches");
    }

    /// <summary>**The clean fixture decodes.**</summary>
    [Fact]
    public void TheCleanFixtureDecodes() => Decodes("psk31-clean-1000hz.wav", 0.01);

    /// <summary>**And with noise on it.**</summary>
    [Fact]
    public void AtTenDecibels() => Decodes("psk31-snr+10db-1000hz.wav", 0.02);

    /// <summary>**And with more noise on it.**</summary>
    [Fact]
    public void AtThreeDecibels() => Decodes("psk31-snr+3db-1000hz.wav", 0.05);

    /// <summary>**And with the noise above the signal in 2500 Hz.**</summary>
    [Fact]
    public void AtMinusThreeDecibels() => Decodes("psk31-snr-3db-1000hz.wav", 0.10);

    /// <summary>**A carrier that drifts 20 Hz in a minute is held.**</summary>
    [Fact]
    public void ACarrierThatDriftsIsHeld()
        => Decodes("psk31-drift-1000-to-1020hz.wav", 0.05);

    /// <summary>**The squelch produces nothing from nothing.**</summary>
    /// <remarks>
    /// **THE AUTHOR'S UNSQUELCHED REFERENCE EMITTED 112 GARBAGE CHARACTERS** from this
    /// file. §0.0 is not a preference here: text on a panel that nobody sent is the
    /// whole of what this project is written against, and for a text mode the squelch
    /// **is** the prime directive.
    /// </remarks>
    [Fact]
    public void TheSquelchProducesNothingFromNothing()
    {
        var (text, seconds) = Run("psk31-noise-only-30s.wav", Offset);

        _output.WriteLine("noise-only: " + text.Length + " characters in "
            + seconds.ToString("0.00", CultureInfo.InvariantCulture) + " s");

        if (text.Length > 0)
        {
            _output.WriteLine("it emitted: [" + text + "]");
        }

        Assert.Equal("", text);
    }

    /// <summary>**A channel at 1000 Hz ignores a station at 1500 Hz.**</summary>
    [Fact]
    public void AChannelAtOneThousandIgnoresAStationAtFifteenHundred()
    {
        var entry = Manifest().Single(e => e.File.Contains("two-signals", StringComparison.Ordinal));

        var (text, seconds) = Run(entry.File, Offset);

        var cer = ErrorRate(text, Reference());

        _output.WriteLine("two signals, channel at 1000 Hz");
        _output.WriteLine("  CER   : " + cer.ToString("0.0000", CultureInfo.InvariantCulture)
            + "   reference " + entry.ReferenceCer.ToString("0.0000", CultureInfo.InvariantCulture));
        _output.WriteLine("  time  : " + seconds.ToString("0.00", CultureInfo.InvariantCulture) + " s");
        _output.WriteLine("  text  : " + Shown(text));

        Assert.True(cer <= 0.05, "CER is " + cer.ToString("0.0000", CultureInfo.InvariantCulture));

        // **AND NOT ONE WORD OF THE OTHER STATION.** The manifest says the second
        // signal carries EI4GNB's text; a channel that let it in would be reporting a
        // station it is not tuned to.
        Assert.DoesNotContain("EI4GNB", text, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>**The weak-signal fixture, measured and not gated.**</summary>
    /// <remarks>
    /// <para>**TASK 5, AND IT ASSERTS NO ERROR RATE ON PURPOSE.** One rung below the
    /// weakest fixture that already existed: -10 dB in 2500 Hz, which is about +9 dB
    /// inside PSK31's own 31 Hz. **It is a measurement rather than a gate**, because a
    /// ceiling invented here would be a number nobody has any evidence for, and the
    /// honest thing to do with a new reading is report it.</para>
    /// <para>**IT IS STILL NOT WEAK-SIGNAL WORK.** That wants real off-air audio, which
    /// only the operator can record, and it is asked for in the report.</para>
    /// </remarks>
    [Fact]
    public void TheWeakSignalFixtureIsMeasuredAndNotGated()
    {
        const string file = "psk31-snr-10db-1000hz.wav";

        var entry = Manifest().SingleOrDefault(e => e.File == file);

        Assert.NotNull(entry);

        using (var stream = File.OpenRead(Fixture(file)))
        {
            var hash = Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();

            Assert.Equal(entry!.Sha256, hash);
        }

        var (text, seconds) = Run(file, Offset);

        var cer = ErrorRate(text, Reference());

        _output.WriteLine(file);
        _output.WriteLine("  CER     : " + cer.ToString("0.0000", CultureInfo.InvariantCulture)
            + "   NO CEILING - this is a measurement, not a gate");
        _output.WriteLine("  length  : " + text.Length + " of " + Reference().Length);
        _output.WriteLine("  seconds : " + seconds.ToString("0.00", CultureInfo.InvariantCulture));
        _output.WriteLine("  text    : " + Shown(text));
    }

    /// <summary>Decode one fixture and hold it to a ceiling.</summary>
    private void Decodes(string file, double ceiling)
    {
        var entry = Manifest().Single(e => e.File == file);

        var (text, seconds) = Run(file, Offset);

        var cer = ErrorRate(text, Reference());

        _output.WriteLine(file);
        _output.WriteLine("  CER     : " + cer.ToString("0.0000", CultureInfo.InvariantCulture)
            + "   reference " + entry.ReferenceCer.ToString("0.0000", CultureInfo.InvariantCulture)
            + "   ceiling " + ceiling.ToString("0.00", CultureInfo.InvariantCulture));
        _output.WriteLine("  length  : " + text.Length + " of " + Reference().Length);
        _output.WriteLine("  seconds : " + seconds.ToString("0.00", CultureInfo.InvariantCulture));
        _output.WriteLine("  text    : " + Shown(text));

        // **EVERY FIXTURE DECODES IN UNDER FIVE SECONDS** on this machine, reported as
        // a number. Half a million samples is not a long test, and one that took
        // longer would be a finding about the code.
        Assert.True(
            seconds < 5.0,
            file + " took " + seconds.ToString("0.00", CultureInfo.InvariantCulture) + " s");

        Assert.True(
            cer <= ceiling,
            file + ": CER " + cer.ToString("0.0000", CultureInfo.InvariantCulture)
            + " is over the ceiling of " + ceiling.ToString("0.00", CultureInfo.InvariantCulture));
    }

    /// <summary>Run the demodulator over a fixture, timed.</summary>
    private static (string Text, double Seconds) Run(string file, double offset)
    {
        var audio = WavAudio.Read(Fixture(file));

        var demodulator = new Psk31Demodulator(audio.SampleRate, offset);

        var watch = Stopwatch.StartNew();

        var text = demodulator.Add(audio.Samples);

        watch.Stop();

        return (text, watch.Elapsed.TotalSeconds);
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

    private static string Shown(string text)
        => text.Length <= 96
            ? text.Replace("\r", "", StringComparison.Ordinal)
                .Replace("\n", " / ", StringComparison.Ordinal)
            : text[..96].Replace("\r", "", StringComparison.Ordinal)
                .Replace("\n", " / ", StringComparison.Ordinal) + "…";

    private static string Reference()
        => Encoding.Latin1.GetString(
            File.ReadAllBytes(Path.Combine(Root(), "assets", "fixtures", "qso-text.txt")));

    private static string Fixture(string file)
        => Path.Combine(Root(), "assets", "fixtures", file);

    private sealed record Entry(string File, string Sha256, double ReferenceCer);

    private static IReadOnlyList<Entry> Manifest()
    {
        var path = Path.Combine(Root(), "assets", "fixtures", "manifest.json");

        using var stream = File.OpenRead(path);

        var document = JsonDocument.Parse(stream);

        return document.RootElement.EnumerateArray()
            .Select(e => new Entry(
                e.GetProperty("file").GetString() ?? "",
                e.GetProperty("sha256").GetString() ?? "",
                // **`null` IS A REAL ANSWER IN THIS FILE.** The noise-only fixture has
                // no reference error rate because it has no reference text, and -1
                // says *not stated* rather than pretending to a number.
                e.TryGetProperty("reference_cer", out var cer)
                    && cer.ValueKind == JsonValueKind.Number
                        ? cer.GetDouble()
                        : -1))
            .ToList();
    }

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
