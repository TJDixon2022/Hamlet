using System;
using System.Collections.Generic;
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
/// Work instruction 359 task 3: **Hamlet's own RSID burst (criterion 1.4).**
/// </summary>
/// <remarks>
/// <para>**THREE PROOFS, AND ONLY ONE OF THEM IS HAMLET AGREEING WITH ITSELF.** The loopback
/// through the detector is the self-agreement. The other two are against the file - the tones
/// measured in what the generator makes are the file's sequence - and against fldigi's own
/// encoder, whose bursts are in front of the mode author's fixtures.</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004).</para>
/// </remarks>
public sealed class TheRsidBurstTests
{
    private const double WithinHz = 5.0;
    private const float Peak = 0.5f;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public TheRsidBurstTests(ITestOutputHelper output) => _output = output;

    /// <summary>**The sequence for every code that has one is the file's, and it is what the samples carry.**</summary>
    [Fact]
    public void EveryCodeWithASequenceMakesTheFilesTones()
    {
        Assert.NotEmpty(Codes.ToneSequences);

        foreach (var (name, tones) in Codes.ToneSequences)
        {
            var code = Codes.Codes[name];

            Assert.Equal(tones, RsidBurst.TonesFor(Codes, code));

            var samples = RsidBurst.Samples(Codes, code, 1000, 8000, Peak);
            var measured = Strongest(samples, 8000, 1000);

            _output.WriteLine($"{name,-16} {code,3}  file {string.Join(" ", tones)}  measured {string.Join(" ", measured)}");

            Assert.Equal(tones, measured);
        }

        // **AND NOW EVERY CODE THE FILE LISTS HAS ONE** (work instruction 377 task 3, criterion
        // 2.2, R12). Until unit 377 the engine read a file carrying four sequences for eight
        // codes, and the loop that stood here asserted that the four codes with no sequence made
        // nothing and that `Samples` threw - decision C's *none is derived*. The one RSID file
        // carries a sequence for every code it lists, so that loop iterated ZERO times and
        // asserted nothing at all. **It is replaced by the assertion 2.2 actually makes**, which
        // is stronger and not weaker: every code in the file has a sequence, of the file's own
        // length, and a code added without one fails here instead of passing silently.
        _output.WriteLine($"codes {Codes.Codes.Count}, tone sequences {Codes.ToneSequences.Count}, symbols {Codes.Symbols}");

        foreach (var (name, code) in Codes.Codes.OrderBy(c => c.Value))
        {
            var tones = RsidBurst.TonesFor(Codes, code);

            _output.WriteLine($"{name,-16} {code,3}  {(tones is null ? "NO SEQUENCE IN THE FILE" : tones.Count + " tones")}");

            Assert.True(Codes.ToneSequences.ContainsKey(name), name + " is a code the file lists with no tone sequence");
            Assert.NotNull(tones);
            Assert.Equal(Codes.Symbols, tones!.Count);

            // **AND THE BURST IS MADE**, rather than the sequence merely being present: the old
            // loop's other half asserted `Samples` threw where there was none, and this asserts
            // the other side of the same rule for every code there is.
            Assert.NotEmpty(RsidBurst.Samples(Codes, code, 1000, 8000, Peak));
        }

        Assert.Equal(Codes.Codes.Count, Codes.ToneSequences.Count);

        // **A CODE THE FILE DOES NOT CARRY STILL MAKES NOTHING, AND NONE IS DERIVED** (decision
        // C). The rule the old loop existed for is kept whole and is now asserted where it can
        // still be true: on a code that is not in the file at all.
        var unknown = Enumerable.Range(0, 256).First(c => !Codes.Codes.Values.Contains(c));

        _output.WriteLine($"code {unknown} is in no row of the file: TonesFor is null and Samples throws");

        Assert.Null(RsidBurst.TonesFor(Codes, unknown));
        Assert.Throws<ArgumentException>(() => RsidBurst.Samples(Codes, unknown, 1000, 8000, Peak));
    }

    /// <summary>**Loopback: each burst, at three centers across the passband, reads back as itself.**</summary>
    [Fact]
    public void EachBurstReadsBackAtThreeCentersAcrossThePassband()
    {
        const int Rate = 12_000;

        var centers = new[] { 500.0, 1500.0, 2500.0 };
        var read = 0;
        var tried = 0;

        foreach (var (name, _) in Codes.ToneSequences)
        {
            var code = Codes.Codes[name];

            foreach (var center in centers)
            {
                tried++;

                var samples = RsidBurst.Samples(Codes, code, center, Rate, Peak);
                var heard = RsidDetector.Detect(
                    Codes, new MonoAudio(Rate, samples), Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz);

                _output.WriteLine(
                    $"{name,-16} at {center,6:0} Hz -> "
                    + (heard.Count == 0
                        ? "none"
                        : string.Join("; ", heard.Select(h => $"{h.Name} ({h.Code}) at {h.CenterHz:0.00} Hz, tones right {h.TonesRight}"))));

                if (heard.Count == 1 && heard[0].Code == code && Math.Abs(heard[0].CenterHz - center) <= WithinHz)
                {
                    read++;
                }
            }
        }

        _output.WriteLine($"read back {read} of {tried}");

        Assert.Equal(tried, read);
    }

    /// <summary>**Against fldigi's own encoder: the shipped burst's strongest tones are Hamlet's sequence, 15 of 15.**</summary>
    /// <param name="file">A fixture with a burst in front of it.</param>
    [Theory]
    [InlineData("psk31-cq-rsid.wav")]
    [InlineData("olivia-8-250-cq-rsid.wav")]
    public void TheShippedBurstMatchesHamletsSequenceSymbolForSymbol(string file)
    {
        var (path, variant, center) = Fixture(file);
        var name = variant == "PSK31" ? "BPSK31" : "OLIVIA_" + variant.Replace('/', '_');
        var code = Codes.CodeOf(name);

        Assert.NotNull(code);

        var audio = WavAudio.Read(path);
        var shipped = Strongest(audio.Samples, audio.SampleRate, center);
        var hamlets = RsidBurst.TonesFor(Codes, code!.Value)!;
        var made = Strongest(RsidBurst.Samples(Codes, code.Value, center, audio.SampleRate, Peak), audio.SampleRate, center);

        var matched = shipped.Zip(hamlets, (a, b) => a == b).Count(m => m);

        _output.WriteLine($"{file}: {name} ({code}) at {center} Hz, {audio.SampleRate} Hz");
        _output.WriteLine("  shipped : " + string.Join(" ", shipped));
        _output.WriteLine("  Hamlet's: " + string.Join(" ", hamlets));
        _output.WriteLine("  made    : " + string.Join(" ", made));
        _output.WriteLine($"  matched {matched} of {Codes.Symbols}");

        Assert.Equal(Codes.Symbols, matched);
        Assert.Equal(shipped, made);
    }

    /// <summary>**The length is the file's: the silence it names, then its symbols over its rate, to the sample.**</summary>
    /// <param name="rate">Samples a second.</param>
    [Theory]
    [InlineData(8_000)]
    [InlineData(12_000)]
    [InlineData(44_100)]
    [InlineData(48_000)]
    public void TheLengthIsTheFilesSymbolsOverItsRateToTheSample(int rate)
    {
        var code = Codes.Codes[Codes.ToneSequences.Keys.First()];
        var samples = RsidBurst.Samples(Codes, code, 1000, rate, Peak);
        var toneStart = (int)Math.Round(Codes.SilenceSymbolsBefore * rate / Codes.SymbolRateHz);
        var tones = (int)Math.Round(Codes.Symbols * rate / Codes.SymbolRateHz);

        _output.WriteLine(
            $"{rate} Hz: {samples.Length} samples, silence {toneStart}, tones {samples.Length - toneStart} "
            + $"(the file's {Codes.Symbols} over {Codes.SymbolRateHz} Hz is {tones}), "
            + $"{RsidBurst.Seconds(Codes):0.0000} s");

        Assert.Equal(toneStart, RsidBurst.ToneStartSample(Codes, rate));
        Assert.Equal(tones, samples.Length - toneStart);
        Assert.Equal(samples.Length, RsidBurst.LengthInSamples(Codes, rate));
        Assert.All(samples.Take(toneStart), s => Assert.Equal(0f, s));
        Assert.True(samples.Skip(toneStart).Max(s => Math.Abs(s)) <= Peak);
        Assert.True(samples.Skip(toneStart).Max(s => Math.Abs(s)) >= 0.99f * Peak);
    }

    private static RsidCodes Codes
        => OliviaData.Current.Rsid ?? throw new InvalidOperationException(OliviaData.Current.Problem);

    /// <summary>
    /// The strongest tone in each symbol, measured over the middle three quarters of each so a
    /// start a little early or late does not decide it.
    /// </summary>
    private static int[] Strongest(float[] x, int sampleRate, double centerHz)
    {
        var perSymbol = sampleRate / Codes.SymbolRateHz;
        var toneStart = Codes.SilenceSymbolsBefore * perSymbol;
        var span = Codes.ToneSequences.Values.Max(t => t.Max()) + 1;
        var found = new int[Codes.Symbols];

        for (var i = 0; i < Codes.Symbols; i++)
        {
            var from = (int)Math.Round(toneStart + ((i + 0.125) * perSymbol));
            var count = (int)Math.Round(0.75 * perSymbol);
            var best = 0.0;

            for (var k = 0; k < span; k++)
            {
                var hz = centerHz + ((Codes.FirstToneOffsetSymbols + k) * Codes.SymbolRateHz);
                var e = Energy(x, from, count, hz, sampleRate);

                if (e > best)
                {
                    best = e;
                    found[i] = k;
                }
            }
        }

        return found;
    }

    private static double Energy(float[] x, int start, int n, double hz, int sampleRate)
    {
        var coefficient = 2 * Math.Cos(2 * Math.PI * hz / sampleRate);
        double s1 = 0;
        double s2 = 0;

        for (var i = start; i < start + n && i < x.Length; i++)
        {
            var s0 = x[i] + (coefficient * s1) - s2;
            s2 = s1;
            s1 = s0;
        }

        return (s1 * s1) + (s2 * s2) - (coefficient * s1 * s2);
    }

    /// <summary>The fixture's path, variant and center, after its hash has been checked.</summary>
    private static (string Path, string Variant, double Center) Fixture(string file)
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        var folder = Path.Combine(at!.FullName, "assets", "fixtures", "olivia");

        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(folder, "manifest.json")));

        var entry = manifest.RootElement.EnumerateArray().Single(e => e.GetProperty("file").GetString() == file);
        var path = Path.Combine(folder, file);

        Assert.Equal(
            entry.GetProperty("sha256").GetString(),
            Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant());

        return (path, entry.GetProperty("variant").GetString()!, entry.GetProperty("center_hz").GetDouble());
    }
}
