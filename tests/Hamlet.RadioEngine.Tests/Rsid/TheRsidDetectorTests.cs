using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Rsid;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Rsid;

/// <summary>
/// Work instruction 359 task 2: **Hamlet hears an RSID burst anywhere in the passband.**
/// </summary>
/// <remarks>
/// <para>**THE FIXTURES ARE THE MODE AUTHOR'S, AND THE BURSTS ARE FLDIGI'S OWN ENCODER**
/// (`PHASE_PLAN.md` R30). Nothing Hamlet thinks RSID is made them, so a detector that agrees
/// with itself cannot pass here.</para>
/// <para>**EVERY FILE IS HASHED AGAINST THE MANIFEST BEFORE IT IS READ** (criterion 2.5, met
/// early). A fixture that changed underneath the test is a different test.</para>
/// <para>**THE CENTER IS HELD TO 5 HZ AND THAT IS NEVER LOOSENED** (§6). The expected code is
/// read from `rsid-codes.json` by name; no number here is an RSID code.</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004).</para>
/// </remarks>
public sealed class TheRsidDetectorTests
{
    private const double WithinHz = 5.0;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each fixture's reading is printed.</param>
    public TheRsidDetectorTests(ITestOutputHelper output) => _output = output;

    /// <summary>**1.1: each single-burst fixture yields exactly one detection, right code, right center.**</summary>
    /// <param name="file">The fixture.</param>
    [Theory]
    [InlineData("olivia-8-250-cq-rsid.wav")]
    [InlineData("olivia-16-500-qso-rsid.wav")]
    [InlineData("olivia-32-1000-qso-rsid.wav")]
    [InlineData("olivia-16-500-qso-snr-10db.wav")]
    [InlineData("olivia-16-500-qso-snr-16db.wav")]
    [InlineData("psk31-cq-rsid.wav")]
    public void EachSingleBurstFixtureYieldsExactlyOneDetectionWithItsCodeAndCenter(string file)
    {
        var entry = Fixture(file);
        var name = NameFor(entry.Variants[0]);
        var code = Codes.CodeOf(name);

        Assert.NotNull(code);

        var heard = Hear(entry, (name, entry.Centers[0]));
        var one = Assert.Single(heard);

        Assert.Equal(code, one.Code);
        Assert.Equal(name, one.Name);
        Assert.InRange(one.CenterHz, entry.Centers[0] - WithinHz, entry.Centers[0] + WithinHz);
    }

    /// <summary>**1.1: a file with no burst in it yields nothing.**</summary>
    /// <param name="file">The fixture.</param>
    [Theory]
    [InlineData("olivia-8-250-qso-norsid.wav")]
    [InlineData("olivia-noise-only-30s.wav")]
    public void AFileWithNoBurstYieldsNone(string file)
    {
        var heard = Hear(Fixture(file));

        Assert.Empty(heard);
    }

    /// <summary>**1.2: the two-signal fixture yields two, 8/250 at 1000 Hz and 16/500 at 2000 Hz.**</summary>
    [Fact]
    public void TheTwoSignalFixtureYieldsTwoDetections()
    {
        var entry = Fixture("olivia-two-signals-rsid.wav");
        var expected = entry.Variants.Zip(entry.Centers, (v, c) => (NameFor(v), c)).ToArray();

        Assert.Equal(2, expected.Length);

        var heard = Hear(entry, expected);

        Assert.Equal(2, heard.Count);

        foreach (var (name, center) in expected)
        {
            var one = Assert.Single(heard, d => d.Name == name);

            Assert.Equal(Codes.CodeOf(name), one.Code);
            Assert.InRange(one.CenterHz, center - WithinHz, center + WithinHz);
        }
    }

    /// <summary>**1.3: the -16 dB fixture's burst is detected**, named on its own.</summary>
    [Fact]
    public void TheBurstSixteenDecibelsBelowTheNoiseIsDetected()
    {
        var entry = Fixture("olivia-16-500-qso-snr-16db.wav");
        var name = NameFor(entry.Variants[0]);
        var heard = Hear(entry, (name, entry.Centers[0]));

        Assert.Contains(
            heard,
            d => d.Code == Codes.CodeOf(name)
                 && Math.Abs(d.CenterHz - entry.Centers[0]) <= WithinHz);
    }

    private static RsidCodes Codes
        => OliviaData.Current.Rsid ?? throw new InvalidOperationException(OliviaData.Current.Problem);

    /// <summary>The fldigi name a manifest variant is announced under.</summary>
    private static string NameFor(string variant)
        => variant == "PSK31" ? "BPSK31" : "OLIVIA_" + variant.Replace('/', '_');

    private IReadOnlyList<RsidDetection> Hear(Entry entry, params (string Name, double Hz)[] expected)
    {
        var audio = WavAudio.Read(entry.Path);
        var cpu = Process.GetCurrentProcess().TotalProcessorTime;
        var clock = Stopwatch.StartNew();

        var heard = RsidDetector.Detect(
            Codes, audio, Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz);

        clock.Stop();

        var cpuSeconds = (Process.GetCurrentProcess().TotalProcessorTime - cpu).TotalSeconds;
        var audioSeconds = audio.Duration.TotalSeconds;

        _output.WriteLine(
            $"{System.IO.Path.GetFileName(entry.Path)}: {audio.SampleRate} Hz, {audioSeconds:0.00} s, "
            + $"passband {Psk31CarrierSearch.PassbandLowHz:0}-{Psk31CarrierSearch.PassbandHighHz:0} Hz, "
            + $"wall {clock.Elapsed.TotalSeconds:0.000} s, process cpu {cpuSeconds:0.000} s, "
            + $"cpu/audio {cpuSeconds / audioSeconds:0.000}");

        foreach (var (name, hz) in expected)
        {
            _output.WriteLine($"  expected {name} ({Codes.CodeOf(name)}) at {hz:0} Hz");
        }

        if (heard.Count == 0)
        {
            _output.WriteLine("  detected none");
        }

        foreach (var d in heard)
        {
            var nearest = expected.Length == 0 ? double.NaN : expected.Min(e => Math.Abs(e.Hz - d.CenterHz));

            _output.WriteLine(
                $"  detected {d.Name} ({d.Code}) mode {d.Mode} variant \"{d.Variant}\" at {d.CenterHz:0.00} Hz, "
                + $"error {nearest:0.00} Hz, quality {d.Quality:0.000}, tones right {d.TonesRight}, "
                + $"first tone at {d.StartSeconds:0.000} s");
        }

        return heard;
    }

    private sealed record Entry(string Path, string[] Variants, double[] Centers);

    /// <summary>The manifest's entry for a file, after its hash has been checked.</summary>
    private static Entry Fixture(string file)
    {
        var folder = System.IO.Path.Combine(Root(), "assets", "fixtures", "olivia");

        using var manifest = JsonDocument.Parse(File.ReadAllText(System.IO.Path.Combine(folder, "manifest.json")));

        var entry = manifest.RootElement.EnumerateArray()
            .Single(e => e.GetProperty("file").GetString() == file);

        var path = System.IO.Path.Combine(folder, file);
        var hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

        Assert.Equal(entry.GetProperty("sha256").GetString(), hash);

        var variant = entry.GetProperty("variant");
        var center = entry.GetProperty("center_hz");

        var variants = variant.ValueKind == JsonValueKind.String
            ? variant.GetString()!.Split(" + ")
            : [];

        var centers = center.ValueKind switch
        {
            JsonValueKind.Number => [center.GetDouble()],
            JsonValueKind.String => center.GetString()!.Split(',')
                .Select(c => double.Parse(c, CultureInfo.InvariantCulture)).ToArray(),
            _ => Array.Empty<double>(),
        };

        return new Entry(path, variants, centers);
    }

    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(System.IO.Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }
}
