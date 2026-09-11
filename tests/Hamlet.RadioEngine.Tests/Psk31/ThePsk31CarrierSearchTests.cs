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
/// Work instruction 315 task 2: **the carrier search.**
/// </summary>
/// <remarks>
/// <para>**THE PASSBAND IN, THE CARRIERS OUT.** Samples go in; the offsets of every PSK31
/// carrier present come out, each with a strength, updated as audio arrives. It knows
/// nothing about tabs, rows or radios (§0.1), and it is not told where to look: nothing
/// in these tests hands it an offset.</para>
/// <para>**"YIELDS" MEANS EVERY CARRIER IT EVER LISTED OVER THE FILE**, counted by the
/// carrier's own id. Carriers stop before the file ends and are retired, so the list at
/// the last sample is not the answer; and a carrier that flickered out and back would
/// come back under a second id, so counting ids is also what asserts it did not.</para>
/// <para>**THE FIXTURES ARE THE AUTHOR'S, MADE ON A MACHINE WITH NO RADIO** (FACT-004,
/// FACT-006). Every number here is an indication, never a finding. **COMPUTED, NOT
/// SEEN.**</para>
/// </remarks>
public sealed class ThePsk31CarrierSearchTests
{
    /// <summary>How close a reported offset must be to where the fixture put it.</summary>
    private const double Within = 5;

    /// <summary>Audio is fed in quarter-second lumps, the way the tick gives it.</summary>
    private const double ChunkSeconds = 0.25;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the carriers are printed.</param>
    public ThePsk31CarrierSearchTests(ITestOutputHelper output) => _output = output;

    /// <summary>**The rule and its numbers live in one named place.**</summary>
    [Fact]
    public void TheRuleAndItsNumbersLiveInOneNamedPlace()
    {
        _output.WriteLine("rule : " + Psk31CarrierSearch.SearchRule);

        Assert.False(string.IsNullOrWhiteSpace(Psk31CarrierSearch.SearchRule));

        // **THE KEYING-SHAPE NUMBERS SIT INSIDE THE MEASURE'S OWN RANGE**, or the search
        // is a gate that never opens or one that never shuts.
        Assert.InRange(Psk31CarrierSearch.CoherenceToStay, 0.0, Psk31CarrierSearch.CoherenceToAppear);
        Assert.InRange(Psk31CarrierSearch.CoherenceToAppear, Psk31CarrierSearch.CoherenceToStay, 1.0);
        Assert.True(Psk31CarrierSearch.RetireAfterSeconds > 0);
    }

    /// <summary>**Assertion 1: two signals, two carriers, no others.**</summary>
    [Fact]
    public void TheTwoSignalFixtureYieldsTwoCarriersAndNoOthers()
    {
        var heard = Listen("psk31-two-signals-1000-1500hz.wav", "manifest.json");

        AssertCarriers(heard, (1000, 0), (1500, 0));
    }

    /// <summary>**Assertion 2: four signals, four carriers, one of them drifting.**</summary>
    /// <remarks>
    /// **THE 1600 HZ CARRIER DRIFTS +8 HZ OVER THE FILE AND IS STILL ONE CARRIER.** Its
    /// every reading must lie between 1600 and 1608, give or take the tolerance, under
    /// one id.
    /// </remarks>
    [Fact]
    public void TheFourSignalFixtureYieldsFourCarriersAndNoOthers()
    {
        var heard = Listen("psk31-four-signals.wav", "manifest-step2.json");

        AssertCarriers(heard, (700, 0), (1100, 0), (1600, 8), (2200, 0));
    }

    /// <summary>**Assertion 3: noise, nothing.**</summary>
    [Fact]
    public void TheNoiseOnlyFixtureYieldsNoCarriers()
    {
        var heard = Listen("psk31-noise-only-30s.wav", "manifest.json");

        Assert.Empty(heard);
    }

    /// <summary>**Assertion 4: one signal, exactly one carrier.**</summary>
    [Fact]
    public void TheCleanFixtureYieldsExactlyOne()
    {
        var heard = Listen("psk31-clean-1000hz.wav", "manifest.json");

        AssertCarriers(heard, (1000, 0));
    }

    /// <summary>**Assertion 5: the strengths come out in the order the fixture made them.**</summary>
    /// <remarks>
    /// **ORDER, NOT DECIBELS.** The manifest gives the file's overall SNR and names which
    /// carrier is strong and which weak; it does not give each carrier's level, so the
    /// order is what can be asserted: 1600 &gt; 700 &gt; 1100 &gt; 2200. Each carrier's
    /// strength is the median of its readings.
    /// </remarks>
    [Fact]
    public void StrengthsAreOrderedAsTheFixtureWasMade()
    {
        var heard = Listen("psk31-four-signals.wav", "manifest-step2.json");

        double StrengthNear(double hz)
            => heard.Values
                .Where(c => Math.Abs(c.MedianHz - hz) <= Within + 8)
                .Select(c => c.MedianDb)
                .DefaultIfEmpty(double.NaN)
                .First();

        var s700 = StrengthNear(700);
        var s1100 = StrengthNear(1100);
        var s1600 = StrengthNear(1600);
        var s2200 = StrengthNear(2200);

        _output.WriteLine("strengths: 1600 " + Db(s1600) + ", 700 " + Db(s700)
            + ", 1100 " + Db(s1100) + ", 2200 " + Db(s2200));

        Assert.True(s1600 > s700, "1600 is not stronger than 700");
        Assert.True(s700 > s1100, "700 is not stronger than 1100");
        Assert.True(s1100 > s2200, "1100 is not stronger than 2200");
    }

    /// <summary>**The strength number against the three fixtures whose SNR is stated.**</summary>
    /// <remarks>
    /// <para>**A MEASUREMENT AND NOT A GATE.** The strength goes in the column FT8's
    /// signal-to-noise figure sits in, quoted the same way - against 2500 Hz - so how far it
    /// is from what the fixture delivered is printed here and reported. No ceiling is
    /// invented for it. What it would catch is a strength that has quietly stopped meaning
    /// decibels over the noise.</para>
    /// <para>**THE CLEAN FIXTURE IS LEFT OUT ON PURPOSE**: it has no noise, so its ratio is
    /// the 16-bit quantisation floor and says nothing about a radio.</para>
    /// </remarks>
    [Fact]
    public void TheStrengthIsMeasuredAgainstTheStatedSnr()
    {
        foreach (var (file, stated) in new[]
        {
            ("psk31-snr+10db-1000hz.wav", 10.0),
            ("psk31-snr+3db-1000hz.wav", 3.0),
            ("psk31-snr-3db-1000hz.wav", -3.0),
        })
        {
            var heard = Listen(file, "manifest.json");

            var one = heard.Values.OrderBy(h => Math.Abs(h.MedianHz - 1000)).FirstOrDefault();

            _output.WriteLine("  stated " + Db(stated) + ", measured "
                + (one is null ? "no carrier" : Db(one.MedianDb))
                + (one is null ? "" : ", off by " + Db(one.MedianDb - stated)));

            Assert.NotNull(one);
        }
    }

    /// <summary>What the search said about one carrier over a whole file.</summary>
    private sealed record Heard(
        int Id, List<double> Hz, List<double> Db, double FirstSeconds, double LastSeconds)
    {
        public double MedianHz => Median(Hz);

        public double MedianDb => Median(Db.Where(d => !double.IsNaN(d)).ToList());
    }

    /// <summary>Stream a fixture through the search and gather every carrier it listed.</summary>
    private Dictionary<int, Heard> Listen(string file, string manifest)
    {
        HashMatches(file, manifest);

        var audio = WavAudio.Read(Fixture(file));
        var search = new Psk31CarrierSearch(audio.SampleRate);
        var heard = new Dictionary<int, Heard>();

        var chunk = (int)(audio.SampleRate * ChunkSeconds);

        for (var at = 0; at < audio.Samples.Length; at += chunk)
        {
            var count = Math.Min(chunk, audio.Samples.Length - at);

            search.Add(audio.Samples.AsSpan(at, count));

            var seconds = (double)(at + count) / audio.SampleRate;

            foreach (var carrier in search.Carriers)
            {
                if (!heard.TryGetValue(carrier.Id, out var one))
                {
                    one = new Heard(carrier.Id, new List<double>(), new List<double>(), seconds, seconds);
                    heard[carrier.Id] = one;
                }

                one.Hz.Add(carrier.OffsetHz);
                one.Db.Add(carrier.StrengthDb);
                heard[carrier.Id] = one with { LastSeconds = seconds };
            }
        }

        _output.WriteLine(file + ": " + heard.Count + " carrier(s) listed over "
            + audio.Duration.TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture) + " s");

        foreach (var one in heard.Values.OrderBy(h => h.MedianHz))
        {
            _output.WriteLine(
                "  id " + one.Id
                + "  median " + one.MedianHz.ToString("0.0", CultureInfo.InvariantCulture) + " Hz"
                + "  range " + one.Hz.Min().ToString("0.0", CultureInfo.InvariantCulture)
                + " to " + one.Hz.Max().ToString("0.0", CultureInfo.InvariantCulture)
                + "  strength " + Db(one.MedianDb)
                + "  listed " + one.FirstSeconds.ToString("0.00", CultureInfo.InvariantCulture)
                + " to " + one.LastSeconds.ToString("0.00", CultureInfo.InvariantCulture) + " s");
        }

        return heard;
    }

    /// <summary>Exactly these carriers, each one id, every reading inside its band.</summary>
    private static void AssertCarriers(
        Dictionary<int, Heard> heard, params (double Hz, double DriftHz)[] expected)
    {
        Assert.Equal(expected.Length, heard.Count);

        foreach (var (hz, drift) in expected)
        {
            var low = hz - Within;
            var high = hz + drift + Within;

            var matching = heard.Values
                .Where(h => h.MedianHz >= low && h.MedianHz <= high)
                .ToList();

            Assert.True(
                matching.Count == 1,
                matching.Count + " carriers near " + hz.ToString("0", CultureInfo.InvariantCulture)
                + " Hz where there should be exactly one");

            var one = matching[0];

            Assert.True(
                one.Hz.All(h => h >= low && h <= high),
                "the carrier near " + hz.ToString("0", CultureInfo.InvariantCulture)
                + " Hz read from " + one.Hz.Min().ToString("0.0", CultureInfo.InvariantCulture)
                + " to " + one.Hz.Max().ToString("0.0", CultureInfo.InvariantCulture) + " Hz");
        }
    }

    private static void HashMatches(string file, string manifest)
    {
        using var document = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(Root(), "assets", "fixtures", manifest)));

        var want = document.RootElement.EnumerateArray()
            .Single(e => e.GetProperty("file").GetString() == file)
            .GetProperty("sha256").GetString();

        using var stream = File.OpenRead(Fixture(file));

        var got = Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();

        Assert.Equal(want, got);
    }

    private static double Median(List<double> values)
    {
        if (values.Count == 0)
        {
            return double.NaN;
        }

        var sorted = values.OrderBy(v => v).ToList();

        return sorted[sorted.Count / 2];
    }

    private static string Db(double db)
        => double.IsNaN(db) ? "not measured" : db.ToString("+0.0;-0.0", CultureInfo.InvariantCulture) + " dB";

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
