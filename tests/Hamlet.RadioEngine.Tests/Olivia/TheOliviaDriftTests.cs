using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Rsid;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Olivia;

/// <summary>
/// Work instruction 362 task 4: **a carrier drifting 20 Hz a minute, which the demodulator holds**
/// (step 2 criterion 2.7).
/// </summary>
/// <remarks>
/// <para>**THE DRIFTED AUDIO IS MADE HERE AND WRITTEN NOWHERE** (decision S). The recipe: the clean
/// 16/500 fixture, hash checked, made analytic by zeroing the negative half of its spectrum (one
/// transform over the whole file, zero-padded to a power of two), multiplied by
/// exp(j 2 pi (r / 2) t^2) with r = 20 Hz a minute and t from 0 at the first sample, and the real
/// part kept. Its tones rise from where the file has them by 20/60 Hz each second: 43.79 Hz by the
/// end of the file's 131.38 s, 1.40 of its tone spacings. Nothing is written into
/// `assets\fixtures\olivia\` and the manifest is not touched.</para>
/// <para>**HOLDS MEANS CER 0.05 OR UNDER, WITH THE TRACKED OFFSET REPORTED** (decision T). The
/// variant and center come from the RSID detector, whose burst is at the start of the file where
/// the ramp has barely begun, never from the test (decision I).</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004).</para>
/// </remarks>
[Collection(CpuMeasuredAlone.Name)]
public sealed class TheOliviaDriftTests
{
    private const double HzPerSecond = 20.0 / 60.0;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the drifted reading is printed.</param>
    public TheOliviaDriftTests(ITestOutputHelper output) => _output = output;

    /// <summary>**2.7: the 16/500 file drifting 20 Hz a minute decodes at CER 0.05 or under, and the offset tracked follows the ramp.**</summary>
    [Fact]
    public void ADriftingCarrierIsHeld()
    {
        var fixture = OliviaFixtures.Load("olivia-16-500-qso-rsid.wav");
        var drifted = Drift(WavAudio.Read(fixture.Path), HzPerSecond);
        var codes = OliviaData.Current.Rsid!;
        var format = OliviaData.Current.Format!;
        var heard = Assert.Single(RsidDetector.Detect(codes, drifted, Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz));

        Assert.Equal(fixture.Variant, heard.Variant);

        var demodulator = new OliviaDemodulator(format, format.Variant(heard.Variant)!, heard.CenterHz, drifted.SampleRate);
        var before = Process.GetCurrentProcess().TotalProcessorTime;
        var decoding = demodulator.Decode(drifted, heard.StartSeconds + (codes.Symbols / codes.SymbolRateHz));
        var cpu = (Process.GetCurrentProcess().TotalProcessorTime - before).TotalSeconds;
        var cer = OliviaFixtures.CharacterErrorRate(decoding.Text, fixture.Text);
        var track = decoding.OffsetTrack;
        var last = track.Count == 0 ? double.NaN : track[^1].OffsetHz;
        var lastSeconds = track.Count == 0 ? double.NaN : track[^1].Seconds;

        _output.WriteLine(
            $"drifted {fixture.Variant} at {HzPerSecond * 60:0} Hz a minute: detector {heard.Variant} at {heard.CenterHz:0.00} Hz, burst at {heard.StartSeconds:0.000} s; "
            + $"CER {cer:0.0000} (ceiling 0.05), characters {decoding.CharactersOut} of {fixture.Text.Length}, blocks decoded {decoding.BlocksDecoded}, "
            + $"rejected {decoding.BlocksRejected}, sync snr {decoding.SyncSnr:0.00}; cpu {cpu:0.000} s");
        _output.WriteLine(
            "offset tracked (s: Hz, against the ramp's center less the detector's): "
            + string.Join(", ", track.Where((_, i) => i % 8 == 0 || i == track.Count - 1)
                .Select(p => $"{p.Seconds:0.0}: {p.OffsetHz:0.00} ({(p.Seconds * HzPerSecond) + fixture.CenterHz - heard.CenterHz:0.00})")));

        if (cer > 0)
        {
            _output.WriteLine("decoded : " + JsonSerializer.Serialize(decoding.Text));
            _output.WriteLine("manifest: " + JsonSerializer.Serialize(fixture.Text));
        }

        Assert.True(cer <= 0.05, $"drifted: CER {cer:0.0000} over 0.05");

        // **THE TRACK FOLLOWS THE RAMP**: at the last segment, within one tone spacing's eighth of it.
        var expected = (lastSeconds * HzPerSecond) + fixture.CenterHz - heard.CenterHz;

        Assert.InRange(last, expected - (demodulator.Variant.ToneSpacingHz / 8), expected + (demodulator.Variant.ToneSpacingHz / 8));
    }

    /// <summary>The recipe of decision S: a linear frequency ramp from 0 at the first sample.</summary>
    private static MonoAudio Drift(MonoAudio audio, double hzPerSecond)
    {
        var n = audio.Samples.Length;
        var size = (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)n);
        var re = new double[size];
        var im = new double[size];

        for (var i = 0; i < n; i++)
        {
            re[i] = audio.Samples[i];
        }

        Fft(re, im, false);

        // The analytic signal: the positive frequencies doubled, the negative ones gone.
        for (var k = 1; k < size / 2; k++)
        {
            re[k] *= 2;
            im[k] *= 2;
        }

        for (var k = (size / 2) + 1; k < size; k++)
        {
            re[k] = 0;
            im[k] = 0;
        }

        Fft(re, im, true);

        var samples = new float[n];

        for (var i = 0; i < n; i++)
        {
            var t = i / (double)audio.SampleRate;
            var phase = 2 * Math.PI * (hzPerSecond / 2) * t * t;

            samples[i] = (float)((re[i] * Math.Cos(phase)) - (im[i] * Math.Sin(phase)));
        }

        return new MonoAudio(audio.SampleRate, samples);
    }

    /// <summary>An in-place radix-2 transform; the inverse is scaled by 1/N.</summary>
    private static void Fft(double[] re, double[] im, bool inverse)
    {
        var n = re.Length;

        for (int i = 1, j = 0; i < n; i++)
        {
            var bit = n >> 1;

            for (; (j & bit) != 0; bit >>= 1)
            {
                j ^= bit;
            }

            j ^= bit;

            if (i < j)
            {
                (re[i], re[j]) = (re[j], re[i]);
                (im[i], im[j]) = (im[j], im[i]);
            }
        }

        for (var length = 2; length <= n; length <<= 1)
        {
            var angle = 2 * Math.PI / length * (inverse ? 1 : -1);
            var (wr, wi) = (Math.Cos(angle), Math.Sin(angle));

            for (var i = 0; i < n; i += length)
            {
                var (cr, ci) = (1.0, 0.0);

                for (var j = 0; j < length / 2; j++)
                {
                    var a = i + j;
                    var b = a + (length / 2);
                    var tr = (re[b] * cr) - (im[b] * ci);
                    var ti = (re[b] * ci) + (im[b] * cr);

                    re[b] = re[a] - tr;
                    im[b] = im[a] - ti;
                    re[a] += tr;
                    im[a] += ti;

                    (cr, ci) = ((cr * wr) - (ci * wi), (cr * wi) + (ci * wr));
                }
            }
        }

        if (inverse)
        {
            for (var i = 0; i < n; i++)
            {
                re[i] /= n;
                im[i] /= n;
            }
        }
    }
}
