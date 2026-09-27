using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// What fldigi's detection front end would lift on the marks ours never keyed,
/// beside ours as it stands (work instruction 463, task 1; PHASE_PLAN.md 9.4;
/// HM-REQ-129).
/// </summary>
/// <remarks>
/// <para>**A PRINTER. IT ASSERTS NOTHING ABOUT EITHER DECODER.** Nothing under
/// `src` changes for it. It prints the two front ends side by side, then every
/// mark around unit 459's group (a) departures in noise sigmas under three
/// envelopes, then the empty band's ratio against the gate, then the two forms,
/// fixed here before any screen.</para>
/// <para>**THE TWO FORMS ARE COMPUTED HERE AS TASK 1 FIXES THEM**, from `cw.cxx`
/// at `61b97f41`: (A1) a 1024-tap Blackman-windowed sinc low-pass at 5 x WPM /
/// 1.2 Hz on the baseband at 8 kHz (352-356, 396, 696; `fftfilt.cxx` 128-160),
/// its magnitude every 16th sample (704-707), and a moving average of
/// symbollen / (2 x 16) of those (358-361, 428-431, 708); (A2) the signal,
/// noise-floor and peak trackers at the default attack and decay (599-623), run
/// over our 45 Hz envelope and dividing it (629-632). Both are centred on our hop
/// grid, as <see cref="CwProbabilisticDecoder.Envelope(IReadOnlyList{float}, int, double)"/>
/// is. This is a prediction printed for the record; it ranks nothing.</para>
/// </remarks>
public sealed class WhatFldigisFrontEndWouldLiftFact
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private const double HopMs = CwProbabilisticDecoder.HopMilliseconds;

    /// <summary>fldigi's own rate, `CW_SAMPLERATE` (`cw.h` 41).</summary>
    private const int FrontRate = 8000;

    /// <summary>`DEC_RATIO` (`cw.h` 90).</summary>
    private const int DecRatio = 16;

    /// <summary>`CW_FFT_SIZE` / 2, the taps `fftfilt` lays down (`cw.cxx` 81, `fftfilt.cxx` 136-148).</summary>
    private const int LowPassTaps = 1024;

    /// <summary>`cwrx_attack` case 1, 200 decimated samples at 500 a second, 0.4 s, in 5 ms hops.</summary>
    private const int AttackHops = 80;

    /// <summary>`cwrx_decay` case 1, 1000 decimated samples at 500 a second, 2 s, in 5 ms hops.</summary>
    private const int DecayHops = 400;

    /// <summary>Unit 459's group (a), from `.run-unit/unit459-trace.txt` lines 78, 87, 140, 176 and 203: where ours read, at the speed and pitch the read used.</summary>
    private static readonly (string Name, bool Real, string Key, string Ours, double At, double Wpm, double Tone)[] GroupA =
    {
        ("unadjudicated/cw-2026-09-23-173723", true, "R .-.", "E", 29.195, 17.1, 600.0),
        ("unadjudicated/cw-2026-09-23-173723", true, "D -..", "I", 30.000, 17.1, 600.0),
        ("unadjudicated/cw-2026-08-22-032012", true, "O ---", "T", 1.765, 22.9, 500.0),
        ("cq-18wpm-15db-char5", false, "L .-..", "E", 14.145, 17.8, 625.0),
        ("cq-18wpm-5db-char5", false, "0 -----", "U", 13.215, 18.5, 620.0),
    };

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fact.</summary>
    /// <param name="output">Where the trace is printed.</param>
    public WhatFldigisFrontEndWouldLiftFact(ITestOutputHelper output)
        => _output = output;

    /// <summary>The two chains, the lift on group (a)'s marks and the empty band's margin (HM-REQ-129).</summary>
    [Fact]
    public void TheLiftOnTheMarksOursNeverKeyed()
    {
        Chains();

        foreach (var d in GroupA)
        {
            Lift(d);
        }

        EmptyBand();
        Forms();
    }

    /// <summary>
    /// The empty band through the stream as the tree builds it: the highest ratio
    /// any read scored, against the gate (HM-REQ-011; work instruction 463,
    /// section 4, check 4). Run before and with each form.
    /// </summary>
    [Fact]
    public void TheEmptyBandThroughTheStream()
    {
        foreach (var (name, tone) in new[] { ("unadjudicated/cw-2026-08-20-014854", 600.0), ("unadjudicated/cw-2026-08-20-014935", 825.0) })
        {
            var audio = Load(name, true);
            var stream = new CwProbabilisticStream(audio.SampleRate) { ToneHz = tone };
            var chunk = (int)(audio.SampleRate * CwProbabilisticStream.ReadEverySeconds);
            var best = double.NegativeInfinity;
            var reads = 0;
            var emitted = 0;

            stream.CharacterSettled += _ => emitted++;

            for (var at = 0; at < audio.Samples.Length; at += chunk)
            {
                stream.Process(audio.Samples.AsSpan(at, Math.Min(chunk, audio.Samples.Length - at)));

                if (stream.Last.WordsPerMinute > 0)
                {
                    best = Math.Max(best, stream.Last.LikelihoodRatio);
                    reads++;
                }
            }

            stream.Flush();

            Print(string.Create(Invariant,
                $"gate-stream | {name} at {tone:0} Hz | {reads} reads | highest ratio {best:0.000} against the gate {CwProbabilisticDecoder.Gate:0.00}, margin {CwProbabilisticDecoder.Gate - best:0.000} | characters settled {emitted}"));
        }
    }

    private void Chains()
    {
        Print("chain | stage | ours, file:line at HEAD | fldigi, cw.cxx at 61b97f41");
        Print("chain | mixer | quadrature mixdown at the audio's own rate: offline CwProbabilisticDecoder.cs:930-940, stream CwProbabilisticStream.cs:256-270; no low-pass follows it other than the integrator | complex mixdown at 8000 Hz (the audio resampled to CW_SAMPLERATE, cw.h:41) 688-692, then fftfilt: a 1024-tap Blackman-windowed sinc low-pass (fftfilt.cxx:128-160, fftfilt.h:47-55), unity peak gain (fftfilt.cxx:162-171), at CWbandwidth - 150 Hz by default, 5 x WPM / 1.2 Hz only under the matched filter CWmfilt, default off (352-356, 395-398; configuration.h 525, 540)");
        Print("chain | integrator | a Hann of IntegratorWindow(fs, 45.0) samples, odd: 33.4 ms at any rate, equivalent noise bandwidth 45 Hz (1.5 fs / N), set by the constant IntegratorBandwidthHz and never by the speed: CwProbabilisticDecoder.cs:416, 593-598, 618-635, 916-974 (centred), CwProbabilisticStream.cs:139-143, 364-400 (trailing) | decimate by DEC_RATIO 16 to 500 a second (704), magnitude (707), then Cmovavg of bfv = symbollen / (2 x 16) decimated samples, symbollen = round(8000 x 1.2 / WPM): a boxcar half a dit long, equivalent noise bandwidth 1 / (bfv x 2 ms), set by CWspeed (326, 358-361, 416, 428-431, 708); at 17.1 WPM 17 samples, 34 ms, 29 Hz; at 22.9 WPM 13, 26 ms, 38 Hz; at 40 WPM 7, 14 ms, 71 Hz; at 8 WPM 37, 74 ms, 14 Hz");
        Print("chain | level | no normalisation of the envelope: each hop is scored against a noise scale sigma = the envelope's quarter point / 0.7585 (Rayleigh, RayleighQuarterPoint) and a keyed level = its 97th percentile (at least 1.05 sigma), both over a 2.5 s span centred on the hop, re-taken every span / 8 = 62 hops (310 ms): a sliding window, not a time constant (CwProbabilisticDecoder.cs:457, 1004-1083, 1104-1154) | three decayavg trackers on the boxcar's output, avg += (x - avg) / weight (misc.h:59-63), once per decimated sample (704-710, so 500 a second): sig_avg at decay; noise_floor below sig_avg, attack when falling under it and decay otherwise; agc_peak above sig_avg, attack when rising over it and decay otherwise (610-623); attack 200 samples = 0.4 s, decay 1000 = 2 s at cwrx_attack and cwrx_decay case 1, the defaults (599-608; FldigiProgdefaults.cs:79, 82); the value is divided by agc_peak (629-632) and thresholded between the normalised floor and signal (638-641, 649-656). The instruction's 0.4 s and 2 s are confirmed");
        Print("chain | speed | chosen from the same 45 Hz envelope the lattice scores: the window's measured unit (CwProbabilisticStream.cs:442-449, CwUnitEstimator.Measure), else the 8 to 40 WPM grid over that envelope's likelihoods (CwProbabilisticDecoder.cs:768-783), then the marks' overrule past 1.25 (CwProbabilisticStream.cs:516-531); offline, the grid alone (641-647) | tracked after detection from key events (524-535, 831-843); the front end's own speed is progdefaults.CWspeed, the operator's setting, not the tracked receive speed (395-396, 416, 428; CWspeed is set only from the operator's controls and macros, 1286-1288, 1796, 1884)");
        Print("chain | hops | ours samples the envelope every 5 ms (HopMilliseconds); fldigi's trackers step every 2 ms, so 0.4 s is 80 of our hops and 2 s is 400");
    }

    private void Lift((string Name, bool Real, string Key, string Ours, double At, double Wpm, double Tone) d)
    {
        var audio = Load(d.Name, d.Real);
        var e45 = CwProbabilisticDecoder.Envelope(audio.Samples, audio.SampleRate, d.Tone);
        var a1 = SpeedMatched(audio, d.Tone, d.Wpm, e45.Length);
        var a2 = Agc(e45);
        var envelopes = new (string Name, double[] Env)[] { ("ours", e45), ("A1", a1), ("A2", a2) };

        var at = (int)Math.Round(d.At * 1000 / HopMs);
        var from = Math.Max(0, at - (int)(1600 / HopMs));
        var to = Math.Min(e45.Length, at + (int)(800 / HopMs));

        Print(string.Create(Invariant,
            $"lift | {d.Name} | {(d.Real ? "inferred" : "exact")} | key {d.Key} | ours {d.Ours} at {d.At:0.000} s | read at {d.Wpm:0.0} WPM, {d.Tone:0.0} Hz | stretch {from * HopMs / 1000:0.000} to {to * HopMs / 1000:0.000} s"));

        var scales = new List<(double Sigma, double Keyed)>();

        foreach (var (name, env) in envelopes)
        {
            var (sigma, keyed) = Scale(env, at);
            scales.Add((sigma, keyed));

            var strip = new StringBuilder();

            for (var h = from; h < to; h += 2)
            {
                var level = Math.Max(env[h], h + 1 < env.Length ? env[h + 1] : 0) / sigma;
                strip.Append(level >= 10 ? '+' : (char)('0' + (int)Math.Floor(level)));
            }

            Print(string.Create(Invariant,
                $"lift-strip | {name,-4} | keyed {keyed / sigma:0.0} sigma | per 10 ms, floor(sigma), + at 10 and over | {strip}"));
        }

        // Every mark any of the three envelopes shows: a run above half-way between
        // the Rayleigh mean (1.2533 sigma) and that envelope's keyed level.
        var runs = new List<(int Start, int End)>();

        for (var k = 0; k < envelopes.Length; k++)
        {
            var (sigma, keyed) = scales[k];
            var cut = 0.5 * ((1.2533 * sigma) + keyed);
            var env = envelopes[k].Env;
            int? start = null;

            for (var h = from; h <= to; h++)
            {
                var up = h < to && env[h] > cut;

                if (up && start is null)
                {
                    start = h;
                }
                else if (!up && start is { } s)
                {
                    if (h - s >= 2)
                    {
                        runs.Add((s, h));
                    }

                    start = null;
                }
            }
        }

        var merged = new List<(int Start, int End)>();

        foreach (var r in runs.OrderBy(r => r.Start))
        {
            if (merged.Count > 0 && r.Start <= merged[^1].End)
            {
                merged[^1] = (merged[^1].Start, Math.Max(merged[^1].End, r.End));
            }
            else
            {
                merged.Add(r);
            }
        }

        foreach (var (s, e) in merged)
        {
            var cells = new List<string>();

            for (var k = 0; k < envelopes.Length; k++)
            {
                var levels = envelopes[k].Env.Skip(s).Take(e - s).Select(v => v / scales[k].Sigma).OrderBy(v => v).ToArray();
                cells.Add(string.Create(Invariant,
                    $"{envelopes[k].Name} median {levels[levels.Length / 2]:0.0} peak {levels[^1]:0.0} of keyed {scales[k].Keyed / scales[k].Sigma:0.0}"));
            }

            Print(string.Create(Invariant,
                $"lift-mark | {s * HopMs / 1000:0.000} to {e * HopMs / 1000:0.000} s | {(e - s) * HopMs:0} ms, {(e - s) * HopMs / (1200 / d.Wpm):0.00} units | {string.Join(" | ", cells)}"));
        }
    }

    private void EmptyBand()
    {
        const string name = "unadjudicated/cw-2026-08-20-014854";
        const double tone = 600;
        var audio = Load(name, true);
        var e45 = CwProbabilisticDecoder.Envelope(audio.Samples, audio.SampleRate, tone);
        var ours = CwProbabilisticDecoder.Decode(e45, tone);
        var a1 = CwProbabilisticDecoder.Decode(
            SpeedMatched(audio, tone, ours.WordsPerMinute, e45.Length), tone, ours.WordsPerMinute);
        var a2 = CwProbabilisticDecoder.Decode(Agc(e45), tone);

        Print(string.Create(Invariant,
            $"gate | {name} at {tone:0} Hz, whole file, as TheIntegratorBandwidthTable measures it | gate {CwProbabilisticDecoder.Gate:0.00} | ours {ours.LikelihoodRatio:0.000} (margin {CwProbabilisticDecoder.Gate - ours.LikelihoodRatio:0.000}, grid {ours.WordsPerMinute:0.0} WPM, '{ours.Text}') | A1 at {ours.WordsPerMinute:0.0} WPM {a1.LikelihoodRatio:0.000} (margin {CwProbabilisticDecoder.Gate - a1.LikelihoodRatio:0.000}, '{a1.Text}') | A2 {a2.LikelihoodRatio:0.000} (margin {CwProbabilisticDecoder.Gate - a2.LikelihoodRatio:0.000}, '{a2.Text}')"));

        // The stream reads twelve seconds every half second: the highest any such
        // window scores is the room the gate has.
        var windowHops = (int)(CwProbabilisticStream.WindowSeconds * 1000 / HopMs);
        var stepHops = (int)(CwProbabilisticStream.ReadEverySeconds * 1000 / HopMs);
        var a2Whole = Agc(e45);
        double best45 = double.NegativeInfinity, bestA1 = double.NegativeInfinity, bestA2 = double.NegativeInfinity;
        var reads = 0;
        var atSpeed = new Dictionary<double, double[]>();

        for (var end = Math.Min(windowHops, e45.Length); end <= e45.Length; end += stepHops)
        {
            var start = Math.Max(0, end - windowHops);
            var w45 = e45[start..end];
            var r45 = CwProbabilisticDecoder.Decode(w45, tone);

            if (!atSpeed.TryGetValue(r45.WordsPerMinute, out var whole))
            {
                whole = SpeedMatched(audio, tone, r45.WordsPerMinute, e45.Length);
                atSpeed[r45.WordsPerMinute] = whole;
            }

            var wA1 = whole[start..end];
            var rA1 = CwProbabilisticDecoder.Decode(wA1, tone, r45.WordsPerMinute);
            var rA2 = CwProbabilisticDecoder.Decode(a2Whole[start..end], tone);

            best45 = Math.Max(best45, r45.LikelihoodRatio);
            bestA1 = Math.Max(bestA1, rA1.LikelihoodRatio);
            bestA2 = Math.Max(bestA2, rA2.LikelihoodRatio);
            reads++;
        }

        Print(string.Create(Invariant,
            $"gate | {name} at {tone:0} Hz, every 12 s window stepped 0.5 s ({reads} windows), the highest ratio | gate {CwProbabilisticDecoder.Gate:0.00} | ours {best45:0.000} (margin {CwProbabilisticDecoder.Gate - best45:0.000}) | A1 {bestA1:0.000} (margin {CwProbabilisticDecoder.Gate - bestA1:0.000}) | A2 {bestA2:0.000} (margin {CwProbabilisticDecoder.Gate - bestA2:0.000})"));
    }

    private void Forms()
    {
        Print("form | A1 | where: CwProbabilisticDecoder carries the one front end, SpeedMatchedEnvelope, which both paths call: the audio mixed to the pitch and averaged down to 8000 a second, a 1024-tap Blackman-windowed sinc low-pass at 5 x WPM / 1.2 Hz (cw.cxx 352-356, 396, 696; fftfilt.cxx 128-160; unity gain at DC), its magnitude every 16th sample (704-707), and a moving average of max(1, round(9600 / WPM) / 32) of those (358-361, 428-431, 708); centred on each hop so its marks sit where the 45 Hz envelope's do | speed: the one the read is decoded at - the stream decides it exactly as today from the 45 Hz window (the measured unit, else the grid, then the marks' overrule), and the offline read takes the grid's winner on the 45 Hz envelope; the read is then scored once more on the A1 envelope at that one speed with the same gaps, and that result is the read | stream: CwProbabilisticStream keeps the 8 kHz baseband for its window beside the 45 Hz envelope and forms the A1 window at each read; the 45 Hz window still feeds the speed, the gap measurements, the relabel and GapsFitTheUnit | offline: Decode(MonoAudio, toneHz); the App's pitch-line read is a diagnostic and is left on the 45 Hz envelope | nothing in the speed choice reads the A1 envelope");
        Print("form | A2 | where: CwProbabilisticDecoder carries the trackers, one AGC stepped once per hop: sig_avg at decay, noise_floor and agc_peak at attack or decay as cw.cxx 610-623 has them, starting at 0, 1 and 1 as the constructor does (347, 376-377), with attack 80 and decay 400 hops (200 and 1000 decimated samples at 500 a second, 0.4 s and 2 s, converted to 5 ms hops); each hop's envelope divided by agc_peak, or 0 where it is 0 (629-632) | where the division goes: between the envelope and LogLikelihoods - the stream steps the AGC on every pushed hop and keeps the divided window beside the raw one, the lattice (grid and imposed speed alike) is scored on the divided window, and the speed measurement, gaps, relabel and GapsFitTheUnit read the raw one; offline, Decode(MonoAudio, toneHz) divides the whole file's envelope from its first hop | the likelihoods' noise: the same quarter point / 0.7585 and 97th percentile over 2.5 s, now of the divided envelope; nothing else in the model changes");
    }

    /// <summary>fldigi's front end at one speed, centred on each of our hops (A1 as fixed).</summary>
    private static double[] SpeedMatched(MonoAudio audio, double tone, double wpm, int hops)
    {
        var fs = audio.SampleRate;
        var count8 = (int)((long)audio.Samples.Length * FrontRate / fs);
        var i8 = new double[count8];
        var q8 = new double[count8];
        var n8 = new int[count8];
        var omega = -2 * Math.PI * tone / fs;

        for (var n = 0; n < audio.Samples.Length; n++)
        {
            var k = (int)((long)n * FrontRate / fs);

            if (k >= count8)
            {
                break;
            }

            i8[k] += audio.Samples[n] * Math.Cos(omega * n);
            q8[k] += audio.Samples[n] * Math.Sin(omega * n);
            n8[k]++;
        }

        for (var k = 0; k < count8; k++)
        {
            if (n8[k] > 0)
            {
                i8[k] /= n8[k];
                q8[k] /= n8[k];
            }
        }

        var fc = 5.0 * wpm / 1.2 / FrontRate;
        var taps = new double[LowPassTaps];
        const int half = LowPassTaps / 2;

        for (var t = 0; t < LowPassTaps; t++)
        {
            var sinc = t == half ? 2.0 * fc : Math.Sin(2 * Math.PI * fc * (t - half)) / (Math.PI * (t - half));
            var blackman = 0.42 - (0.50 * Math.Cos(2.0 * Math.PI * t / LowPassTaps)) + (0.08 * Math.Cos(4.0 * Math.PI * t / LowPassTaps));
            taps[t] = sinc * blackman;
        }

        var gain = taps.Sum();

        var points = (count8 + DecRatio - 1) / DecRatio;
        var mags = new double[points];

        for (var p = 0; p < points; p++)
        {
            var centre = p * DecRatio;
            double i = 0, q = 0;

            for (var t = 0; t < LowPassTaps; t++)
            {
                var at = centre + t - half;

                if (at < 0 || at >= count8)
                {
                    continue;
                }

                i += taps[t] * i8[at];
                q += taps[t] * q8[at];
            }

            mags[p] = Math.Sqrt((i * i) + (q * q)) / gain;
        }

        var bfv = Math.Max(1, (int)Math.Round(FrontRate * 1.2 / wpm) / (2 * DecRatio));
        var step = Math.Max(1, (int)(fs * HopMs / 1000.0));
        var env = new double[hops];

        for (var h = 0; h < hops; h++)
        {
            var c8 = (((h * (double)step) + 0.5) * FrontRate / fs) - 0.5;
            var first = (int)Math.Round(c8 / DecRatio) - ((bfv - 1) / 2);
            double sum = 0;

            for (var b = 0; b < bfv; b++)
            {
                var p = first + b;

                if (p >= 0 && p < points)
                {
                    sum += mags[p];
                }
            }

            env[h] = sum / bfv;
        }

        return env;
    }

    /// <summary>fldigi's trackers over an envelope, dividing it by the peak (A2 as fixed).</summary>
    private static double[] Agc(IReadOnlyList<double> envelope)
    {
        double sig = 0, noise = 1, peak = 1;
        var output = new double[envelope.Count];

        for (var h = 0; h < envelope.Count; h++)
        {
            var v = envelope[h];

            sig = DecayAvg(sig, v, DecayHops);

            if (v < sig)
            {
                noise = v < noise ? DecayAvg(noise, v, AttackHops) : DecayAvg(noise, v, DecayHops);
            }

            if (v > sig)
            {
                peak = v > peak ? DecayAvg(peak, v, AttackHops) : DecayAvg(peak, v, DecayHops);
            }

            output[h] = peak != 0 ? v / peak : 0;
        }

        return output;
    }

    private static double DecayAvg(double average, double input, int weight)
        => weight <= 1 ? input : ((input - average) / weight) + average;

    /// <summary>The noise scale and keyed level LogLikelihoods would take around one hop.</summary>
    private static (double Sigma, double Keyed) Scale(double[] env, int at)
    {
        var span = (int)(CwProbabilisticDecoder.NoiseSpanSeconds * 1000 / HopMs);
        var take = Math.Min(span, env.Length);
        var from = Math.Clamp(at - (span / 2), 0, Math.Max(0, env.Length - take));
        var sorted = env.Skip(from).Take(take).OrderBy(v => v).ToArray();
        var sigma = Percentile(sorted, 25) / CwProbabilisticDecoder.RayleighQuarterPoint;

        return (sigma, Math.Max(Percentile(sorted, 97), sigma * 1.05));
    }

    private static double Percentile(double[] sorted, double percent)
    {
        var at = percent / 100.0 * (sorted.Length - 1);
        var below = (int)at;
        var above = Math.Min(below + 1, sorted.Length - 1);

        return (sorted[below] * (1 - (at - below))) + (sorted[above] * (at - below));
    }

    private static MonoAudio Load(string name, bool real)
        => WavAudio.Read(Path.Combine(real ? CapturedSignalTests.Folder : SyntheticCq.Folder, name + ".wav"));

    private void Print(string line) => _output.WriteLine(line);
}
