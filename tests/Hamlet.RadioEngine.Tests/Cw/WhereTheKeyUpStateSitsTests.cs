using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Whether the key-up state is the noise floor, measured with Hamlet's own
/// instruments.
/// </summary>
/// <remarks>
/// <para>**THE GATE ON WORK INSTRUCTION 035, AND IT EXISTS TO DISPROVE ITS OWN
/// ORDER.** `ANALYSIS-cw-key-up-is-not-noise-2026-08-27.md` was measured outside
/// Hamlet, from the WAV files, and says three things: the two unread stations are
/// 16.6 and 25.7 dB out of the noise, they are Morse, and **the key-up state on
/// all three captures sits 18 to 32 decibels above the band noise floor**.</para>
/// <para>**WHERE HAMLET'S INSTRUMENTS DISAGREE, HAMLET'S NUMBERS ARE THE TRUTH
/// ABOUT HAMLET.** The order says so itself. This reproduces the three findings
/// through `CwProbabilisticDecoder.Envelope`, which is the thing the decoder
/// actually sees, rather than through a second signal chain that could agree with
/// the analysis and tell us nothing about the decoder.</para>
/// <para>**WHY IT MATTERS.** `CwProbabilisticDecoder` scores key-up as noise with
/// a scale taken from the envelope's own lower quartile. If key-up is thirty
/// decibels above the band floor, the model is being asked to explain a state
/// that is not noise — which is HM-DEC-090's fault arriving in a third place,
/// where a figure was assumed rather than measured.</para>
/// </remarks>
public sealed class WhereTheKeyUpStateSitsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tables are printed.</param>
    public WhereTheKeyUpStateSitsTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The repository root, walking up from the test binary.</summary>
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

    private static MonoAudio Capture(string name)
        => WavAudio.Read(Path.Combine(
            Root(), "tests", "fixtures", "cw", "captured", "unadjudicated",
            name + ".wav"));

    private static MonoAudio Tail(MonoAudio audio, double seconds)
    {
        var want = (int)(audio.SampleRate * seconds);

        if (audio.Samples.Length <= want)
        {
            return audio;
        }

        var slice = new float[want];

        for (var i = 0; i < want; i++)
        {
            slice[i] = audio.Samples[audio.Samples.Length - want + i];
        }

        return new MonoAudio(audio.SampleRate, slice);
    }

    /// <summary>The three the analysis names, with the control last.</summary>
    private static (string Name, double ToneHz, string What)[] Cases { get; } =
    {
        ("cw-2026-08-22-014113", 606.0, "unread"),
        ("cw-2026-08-22-014308", 606.0, "unread"),
        ("cw-2026-08-24-012403", 439.8, "READS - the control"),
        ("cw-2026-08-20-014854", 600.0, "holds nothing"),
        ("cw-2026-08-20-014935", 825.0, "holds nothing"),
    };

    /// <summary>The mean power an envelope carries, in decibels.</summary>
    private static double MeanDb(IReadOnlyList<double> envelope)
    {
        var sum = 0.0;

        foreach (var v in envelope)
        {
            sum += v * v;
        }

        return 10 * Math.Log10(Math.Max(sum / Math.Max(envelope.Count, 1), 1e-24));
    }

    /// <summary>Two levels fitted to a decibel envelope, low first.</summary>
    /// <param name="db">One level per hop.</param>
    /// <returns>The two fitted means and the share of hops in the upper one.</returns>
    /// <remarks>
    /// **THE SHAPE BOTH PUBLISHED DECODERS USE.** `cwdecoder.py` in this
    /// repository's root fits two means to the decibel envelope per window and
    /// thresholds between them; RSCW places its threshold so the mean distance to
    /// the samples above equals the mean distance to those below, which is the
    /// same fixed point. Neither assumes either state.
    /// </remarks>
    private static (double Low, double High, double UpperShare) TwoStates(
        IReadOnlyList<double> db)
    {
        var cut = (db.Min() + db.Max()) / 2;
        var low = db.Min();
        var high = db.Max();

        for (var pass = 0; pass < 40; pass++)
        {
            double hi = 0, lo = 0;
            int hiN = 0, loN = 0;

            foreach (var v in db)
            {
                if (v >= cut)
                {
                    hi += v;
                    hiN++;
                }
                else
                {
                    lo += v;
                    loN++;
                }
            }

            if (hiN == 0 || loN == 0)
            {
                break;
            }

            low = lo / loN;
            high = hi / hiN;

            var next = (low + high) / 2;

            if (Math.Abs(next - cut) < 1e-9)
            {
                break;
            }

            cut = next;
        }

        var above = 0;

        foreach (var v in db)
        {
            if (v >= cut)
            {
                above++;
            }
        }

        return (low, high, db.Count == 0 ? 0 : (double)above / db.Count);
    }

    /// <remarks>
    /// <para>**FINDING ONE: ARE THE STATIONS WEAK?** Unit 1.11.31 concluded they
    /// sit below the decoder's sensitivity. That was a statement about the
    /// window ratio, and this asks the separate question the analysis asks: how
    /// far the station's own pitch stands above the band beside it.</para>
    /// <para>Measured through the decoder's own envelope at the station's pitch
    /// against the same envelope taken 250 and 300 hertz away, which is the band
    /// with no station in it.</para>
    /// </remarks>
    [Fact]
    public void HowFarTheStationStandsOutOfItsOwnBand()
    {
        var window = CwProbabilisticStream.WindowSeconds;

        _output.WriteLine(
            "  capture                | pitch |  at the | beside it | band SNR | what");
        _output.WriteLine(
            "  -----------------------|-------|---------|-----------|----------|-----");

        foreach (var (name, toneHz, what) in Cases)
        {
            var slice = Tail(Capture(name), window);

            var onStation = MeanDb(CwProbabilisticDecoder.Envelope(
                slice.Samples, slice.SampleRate, toneHz));

            // Two clean offsets either side, averaged, so one unlucky neighbour
            // cannot pass for the band.
            var away = new[] { -300.0, -250.0, 250.0, 300.0 }
                .Select(d => MeanDb(CwProbabilisticDecoder.Envelope(
                    slice.Samples, slice.SampleRate, toneHz + d)))
                .Average();

            _output.WriteLine(
                $"  {name,-22} | {toneHz,5:0} | {onStation,7:0.0} | {away,9:0.0} | "
                + $"{onStation - away,8:0.0} | {what}");
        }

        _output.WriteLine("");
        _output.WriteLine(
            "  band SNR is the decoder's own envelope on the station against the "
            + "same");
        _output.WriteLine(
            "  envelope 250 to 300 Hz away, in the decoder's own integrator "
            + "bandwidth");

        Assert.Equal(5, Cases.Length);
    }

    /// <remarks>
    /// <para>**FINDING TWO AND THREE TOGETHER: IS IT MORSE, AND WHERE IS
    /// KEY-UP?** The autocorrelation peak says whether the envelope repeats at a
    /// Morse element's cadence; the two fitted states say what the decoder is
    /// being asked to explain.</para>
    /// <para>**THE QUESTION THE WHOLE UNIT TURNS ON is the last column**: how far
    /// the fitted key-up level sits above the band beside the station. If it is
    /// tens of decibels, key-up is not noise and the model is wrong. If it is
    /// near zero, the model is right and this order's premise is not.</para>
    /// </remarks>
    [Fact]
    public void TheTwoStatesAndWhereKeyUpSits()
    {
        var window = CwProbabilisticStream.WindowSeconds;
        var hopMs = CwProbabilisticDecoder.HopMilliseconds;

        _output.WriteLine(
            "  capture                | acf 1st | key-down | key-up | sep  | "
            + "band floor | key-up over floor | what");
        _output.WriteLine(
            "  -----------------------|---------|----------|--------|------|"
            + "------------|-------------------|-----");

        foreach (var (name, toneHz, what) in Cases)
        {
            var slice = Tail(Capture(name), window);

            var envelope = CwProbabilisticDecoder.Envelope(
                slice.Samples, slice.SampleRate, toneHz);

            var db = envelope
                .Select(v => 20 * Math.Log10(Math.Max(v, 1e-12)))
                .ToArray();

            var (keyUp, keyDown, _) = TwoStates(db);

            // The band with no station in it, at the same bandwidth.
            var floor = new[] { -300.0, -250.0, 250.0, 300.0 }
                .Select(d =>
                {
                    var other = CwProbabilisticDecoder.Envelope(
                        slice.Samples, slice.SampleRate, toneHz + d);

                    return other
                        .Select(v => 20 * Math.Log10(Math.Max(v, 1e-12)))
                        .Average();
                })
                .Average();

            // The envelope's own autocorrelation, first peak after the origin.
            var mean = envelope.Average();
            var centred = envelope.Select(v => v - mean).ToArray();
            var bestLag = 0;
            var best = double.NegativeInfinity;

            var from = (int)(30.0 / hopMs);
            var to = Math.Min((int)(400.0 / hopMs), centred.Length / 2);

            for (var lag = from; lag < to; lag++)
            {
                var sum = 0.0;

                for (var i = 0; i + lag < centred.Length; i++)
                {
                    sum += centred[i] * centred[i + lag];
                }

                if (sum > best)
                {
                    best = sum;
                    bestLag = lag;
                }
            }

            _output.WriteLine(
                $"  {name,-22} | {bestLag * hopMs,6:0} ms | {keyDown,8:0.0} | "
                + $"{keyUp,6:0.0} | {keyDown - keyUp,4:0.0} | {floor,10:0.0} | "
                + $"{keyUp - floor,17:0.0} | {what}");
        }

        _output.WriteLine("");
        _output.WriteLine(
            "  'key-up over floor' is the question: if key-up sits tens of dB "
            + "above the");
        _output.WriteLine(
            "  band beside the station, the decoder is scoring it as noise and it "
            + "is not noise.");

        Assert.Equal(5, Cases.Length);
    }

    /// <remarks>
    /// <para>**WHAT HAMLET MEASURES, WHICH IS THE QUESTION THE ORDER ACTUALLY
    /// TURNS ON.** The analysis says key-up is pinned to the noise floor. Reading
    /// `CwProbabilisticDecoder.Estimate`, it is not: **sigma comes from the
    /// envelope's own 25th percentile**, divided by the Rayleigh quarter point,
    /// and on a signal with roughly even duty that percentile sits *inside* the
    /// key-up population. So the scale already follows an elevated key-up
    /// level.</para>
    /// <para>**WHICH MOVES THE SUSPICION ONE STEP ALONG, AND THIS MEASURES
    /// THERE.** Sigma serves twice: it is the key-up Rayleigh's scale **and the
    /// key-down Gaussian's width**. An elevated key-up level inflates sigma, a
    /// wide key-down Gaussian discriminates poorly, and the quantity that decides
    /// whether two states can be told apart at all is their separation **in units
    /// of that width**.</para>
    /// <para>**SO THE LAST COLUMN IS THE ONE TO READ.** If the captures that read
    /// and the captures that do not differ there, the observation model is the
    /// fault and task 3 has its target. If they do not differ, the premise is
    /// wrong wherever it points and this unit stops.</para>
    /// </remarks>
    [Fact]
    public void WhatTheModelItselfIsWorkingWith()
    {
        var window = CwProbabilisticStream.WindowSeconds;

        _output.WriteLine(
            "  capture                | sigma dB | keyUp dB | ampl dB | "
            + "(A-up)/sigma | what");
        _output.WriteLine(
            "  -----------------------|----------|----------|---------|"
            + "--------------|-----");

        foreach (var (name, toneHz, what) in Cases)
        {
            var slice = Tail(Capture(name), window);

            var envelope = CwProbabilisticDecoder.Envelope(
                slice.Samples, slice.SampleRate, toneHz);

            // The decoder's own estimator, reproduced exactly: sigma from the
            // 25th percentile over the Rayleigh quarter point, the keyed level
            // from the 97th.
            var sorted = envelope.OrderBy(v => v).ToArray();

            double Pct(double p)
            {
                var at = (p / 100.0) * (sorted.Length - 1);
                var below = (int)at;
                var above = Math.Min(below + 1, sorted.Length - 1);

                return sorted[below]
                    + ((sorted[above] - sorted[below]) * (at - below));
            }

            var sigma = Pct(25) / CwProbabilisticDecoder.RayleighQuarterPoint;
            var amplitude = Math.Max(Pct(97), sigma * 1.05);

            var db = envelope
                .Select(v => 20 * Math.Log10(Math.Max(v, 1e-12)))
                .ToArray();

            var (keyUpDb, _, _) = TwoStates(db);
            var keyUpLinear = Math.Pow(10, keyUpDb / 20);

            var separation = sigma > 0 ? (amplitude - keyUpLinear) / sigma : 0;

            _output.WriteLine(
                $"  {name,-22} | {20 * Math.Log10(Math.Max(sigma, 1e-12)),8:0.0} | "
                + $"{keyUpDb,8:0.0} | "
                + $"{20 * Math.Log10(Math.Max(amplitude, 1e-12)),7:0.0} | "
                + $"{separation,12:0.00} | {what}");
        }

        _output.WriteLine("");
        _output.WriteLine(
            "  (A-up)/sigma is how far apart the two states are in units of the "
            + "width the");
        _output.WriteLine(
            "  model uses for both of them. Two states a fraction of a sigma "
            + "apart cannot be");
        _output.WriteLine("  told apart by any threshold placed between them.");

        Assert.Equal(5, Cases.Length);
    }
}
