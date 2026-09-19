using System;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Rsid;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Olivia;

/// <summary>The -14 dB 8/250 audio of criterion 3.0, made in memory, and how it was made.</summary>
/// <param name="Audio">The made audio: the burst slice, the QSO, and the noise over both.</param>
/// <param name="Qso">The no-RSID file's manifest entry, whose text is the one read against.</param>
/// <param name="BurstStartSeconds">Where the detector put the burst's first tone in the CQ file.</param>
/// <param name="SliceEndSample">The sample the burst slice ends at, where the QSO begins.</param>
/// <param name="Seed">The noise generator's seed.</param>
/// <param name="SignalPower">The QSO's mean square, over the QSO part alone.</param>
/// <param name="NoiseVariance">The noise's variance.</param>
public sealed record BelowTheNoiseAudio(
    MonoAudio Audio,
    OliviaFixture Qso,
    double BurstStartSeconds,
    int SliceEndSample,
    int Seed,
    double SignalPower,
    double NoiseVariance)
{
    /// <summary>Where the slice ends, in seconds.</summary>
    public double SliceEndSeconds => SliceEndSample / (double)Audio.SampleRate;
}

/// <summary>
/// **Work instruction 363 decision W: the -14 dB 8/250 fixture, made in the test from the mode
/// author's shipped audio, and written nowhere.**
/// </summary>
/// <remarks>
/// <para>**THE RECIPE.** Both files hash-checked first. The RSID burst is sliced from the front of
/// `olivia-8-250-cq-rsid.wav`: from the first sample to the burst's end plus its trailing silence,
/// the burst's first tone located by the detector and its length and silence taken from
/// `rsid-codes.json`'s symbol count, silence count and rate - no literal. The whole of
/// `olivia-8-250-qso-norsid.wav` follows it, both at their shipped levels. White Gaussian noise
/// from a seeded generator is added over all of it, scaled so the QSO's power over the QSO part
/// alone stands <see cref="SnrDb"/> against the noise's power in <see cref="ReferenceHz"/>: the
/// noise's total variance times 2500 over the Nyquist 4000.</para>
/// <para>**THE SEEDS ARE STATED HERE, BEFORE ANY RESULT WAS SEEN** (decision W, §10): the asserted
/// one and four more that are printed and never asserted.</para>
/// <para>**THE SNR IS MEASURED BACK** by unit 361's method (`Unit361Trace.BelowTheNoise`): the noise
/// density from 1600 to 2400 Hz, where no tone is, and the signal as the power from 700 to 1300 Hz
/// less that density across 600 Hz, referenced to 2500 Hz, from a second after the burst to a
/// second before the end.</para>
/// </remarks>
public static class OliviaBelowTheNoise
{
    /// <summary>The seed 3.0 is asserted on.</summary>
    public const int StatedSeed = 363;

    /// <summary>The four seeds printed beside it and not asserted.</summary>
    public static readonly int[] FurtherSeeds = [3631, 3632, 3633, 3634];

    /// <summary>The SNR the plan sets, in dB.</summary>
    public const double SnrDb = -14;

    /// <summary>The bandwidth the SNR is referenced to, in hertz.</summary>
    public const double ReferenceHz = 2500;

    /// <summary>Make the audio on one seed.</summary>
    /// <param name="seed">The noise generator's seed.</param>
    /// <returns>The audio and its recipe.</returns>
    public static BelowTheNoiseAudio Make(int seed)
    {
        var codes = OliviaData.Current.Rsid!;
        var cq = OliviaFixtures.Load("olivia-8-250-cq-rsid.wav");
        var qso = OliviaFixtures.Load("olivia-8-250-qso-norsid.wav");
        var cqAudio = WavAudio.Read(cq.Path);
        var qsoAudio = WavAudio.Read(qso.Path);

        Assert.Equal(cqAudio.SampleRate, qsoAudio.SampleRate);

        var burst = Assert.Single(RsidDetector.Detect(codes, cqAudio, Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz));
        var endSeconds = burst.StartSeconds + ((codes.Symbols + codes.SilenceSymbolsBefore) / codes.SymbolRateHz);
        var sliceEnd = (int)Math.Round(endSeconds * cqAudio.SampleRate);
        var rate = cqAudio.SampleRate;
        var samples = new float[sliceEnd + qsoAudio.Samples.Length];

        Array.Copy(cqAudio.Samples, samples, sliceEnd);
        Array.Copy(qsoAudio.Samples, 0, samples, sliceEnd, qsoAudio.Samples.Length);

        double power = 0;

        foreach (var s in qsoAudio.Samples)
        {
            power += s * (double)s;
        }

        power /= qsoAudio.Samples.Length;

        // Noise power in the reference band is the variance times its share of the Nyquist band.
        var variance = power / (Math.Pow(10, SnrDb / 10) * (ReferenceHz / (rate / 2.0)));
        var sigma = Math.Sqrt(variance);
        var random = new Random(seed);

        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] += (float)(sigma * Gaussian(random));
        }

        return new BelowTheNoiseAudio(new MonoAudio(rate, samples), qso, burst.StartSeconds, sliceEnd, seed, power, variance);
    }

    /// <summary>Unit 361's measure of the SNR in 2500 Hz, from a point in the audio on.</summary>
    /// <param name="audio">The audio.</param>
    /// <param name="fromSeconds">Where the signal is known to be on: the burst's end.</param>
    /// <returns>The SNR in dB.</returns>
    public static double MeasureSnrDb(MonoAudio audio, double fromSeconds)
    {
        var start = (int)Math.Ceiling((fromSeconds + 1) * audio.SampleRate);
        var body = audio.Samples.AsSpan(start, audio.Samples.Length - start - audio.SampleRate);
        const int size = 8192;
        var fft = new RealFft(size);
        var sum = new double[fft.BinCount];
        var mags = new double[fft.BinCount];
        var re = new double[size];
        var im = new double[size];
        var window = new float[size];
        var frames = 0;
        double shapeEnergy = 0;

        for (var i = 0; i < size; i++)
        {
            var w = 0.5 * (1 - Math.Cos(2 * Math.PI * i / size));
            shapeEnergy += w * w;
        }

        for (var at = 0; at + size <= body.Length; at += size / 2)
        {
            for (var i = 0; i < size; i++)
            {
                window[i] = (float)(body[at + i] * 0.5 * (1 - Math.Cos(2 * Math.PI * i / size)));
            }

            fft.Magnitudes(window, mags, re, im);

            for (var b = 0; b < sum.Length; b++)
            {
                sum[b] += mags[b] * mags[b];
            }

            frames++;
        }

        var binHz = (double)audio.SampleRate / size;

        double Sum(double low, double high)
        {
            double total = 0;

            for (var b = (int)Math.Ceiling(low / binHz); b <= (int)Math.Floor(high / binHz); b++)
            {
                total += sum[b] / frames * 2 / (shapeEnergy * audio.SampleRate) * binHz;
            }

            return total;
        }

        var density = Sum(1600, 2400) / 800;
        var signal = Sum(700, 1300) - (density * 600);

        return 10 * Math.Log10(Math.Max(signal, 1e-30) / (density * ReferenceHz));
    }

    /// <summary>One draw of a standard normal, by Box and Muller.</summary>
    private static double Gaussian(Random random)
    {
        var u = 1.0 - random.NextDouble();
        var v = random.NextDouble();

        return Math.Sqrt(-2 * Math.Log(u)) * Math.Cos(2 * Math.PI * v);
    }
}
