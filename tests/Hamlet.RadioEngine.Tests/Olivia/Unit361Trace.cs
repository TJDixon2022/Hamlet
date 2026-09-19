using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Rsid;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Olivia;

/// <summary>
/// Work instruction 361 task 1: **the trace, before the demodulator is built.**
/// </summary>
/// <remarks>
/// <para>**THIS IS A MEASUREMENT, NOT AN ASSERTION**, the shape of `Unit360Trace`. It prints each
/// variant's parameters as `pj_mfsk.h`'s formulas give them and as the audio shows them, the
/// spectrum of each clean fixture after its burst, the four format constants with the lines they
/// stand on, where a demodulator plugs in, and the RSID detector's own CPU per fixture. **It
/// asserts nothing**, so it cannot become a wall.</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004).</para>
/// </remarks>
public sealed class Unit361Trace
{
    private static readonly (string File, int Tones, int Bandwidth)[] Clean =
    [
        ("olivia-8-250-cq-rsid.wav", 8, 250),
        ("olivia-16-500-qso-rsid.wav", 16, 500),
        ("olivia-32-1000-qso-rsid.wav", 32, 1000),
    ];

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public Unit361Trace(ITestOutputHelper output) => _output = output;

    /// <summary>Print the five items of task 1.</summary>
    [Fact]
    public void StepTwosTrace()
    {
        var root = Root();
        var fixtures = Path.Combine(root, "assets", "fixtures", "olivia");
        var codes = OliviaData.Current.Rsid!;
        var rate = Psk31Resampler.TargetSampleRate;

        _output.WriteLine($"== 1. the variants, from pj_mfsk.h's formulas at the rate the RSID path runs at ({rate} Hz)");
        _output.WriteLine("   SymbolLen = 2^(BitsPerSymbol + 7 - log2(Bandwidth/125)) at 8000 Hz (:2060), SymbolSepar = SymbolLen/2 (:804),");
        _output.WriteLine("   CarrierSepar = 2 FFT bins of SymbolLen (:717), SymbolsPerBlock = 2^(BitsPerCharacter-1) (:1124), BitsPerCharacter 7 (:1121)");

        foreach (var (file, tones, bandwidth) in Clean)
        {
            var bits = (int)Math.Log2(tones);
            var symbolLen8k = 1 << (bits + 7 - (int)Math.Log2(bandwidth / 125));
            var separ8k = symbolLen8k / 2;
            var spacing = 2.0 * 8000 / symbolLen8k;
            var symbolSeconds = separ8k / 8000.0;
            var perBlock = 1 << (7 - 1);
            var blockSeconds = perBlock * symbolSeconds;

            _output.WriteLine(
                $"   {tones}/{bandwidth}: tones {tones}, spacing {spacing:0.000} Hz, symbol {symbolSeconds * 1000:0.000} ms, "
                + $"bits/symbol {bits}, symbols/block {perBlock}, characters/block {bits}, block {blockSeconds:0.000} s, "
                + $"{blockSeconds / bits:0.0000} s/character, samples/symbol at {rate} Hz {symbolSeconds * rate:0.00}, "
                + $"analysis window (SymbolLen) {symbolLen8k * rate / 8000.0:0.00} samples");
        }

        _output.WriteLine("");
        _output.WriteLine("== 1 and 2. each clean fixture after its burst: the detector, the spectrum, the symbol period");

        foreach (var (file, tones, bandwidth) in Clean)
        {
            var audio = WavAudio.Read(Path.Combine(fixtures, file));
            var cpu = Process.GetCurrentProcess().TotalProcessorTime;
            var heard = RsidDetector.Detect(codes, audio, Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz);
            var cpuSeconds = (Process.GetCurrentProcess().TotalProcessorTime - cpu).TotalSeconds;
            var d = heard.First();
            var burstEnd = d.StartSeconds + (codes.Symbols / codes.SymbolRateHz);

            _output.WriteLine($"-- {file}: {audio.SampleRate} Hz, {audio.Samples.Length / (double)audio.SampleRate:0.00} s");
            _output.WriteLine(
                $"   detector: {d.Name} ({d.Code}) variant \"{d.Variant}\" at {d.CenterHz:0.00} Hz, first tone {d.StartSeconds:0.000} s, "
                + $"tones end {burstEnd:0.000} s; detector cpu {cpuSeconds:0.000} s");

            var start = (int)Math.Ceiling((burstEnd + 5 / codes.SymbolRateHz) * audio.SampleRate);
            var body = audio.Samples.AsSpan(start);

            Spectrum(body, audio.SampleRate, d.CenterHz, tones, bandwidth);
            SymbolPeriod(body, audio.SampleRate, d.CenterHz, bandwidth);
        }

        _output.WriteLine("");
        _output.WriteLine("== 3. the four format constants, read from the headers at the lines named");

        var mfsk = File.ReadAllLines(Path.Combine(root, "assets", "reference", "jalocha", "pj_mfsk.h"));
        var gray = File.ReadAllLines(Path.Combine(root, "assets", "reference", "jalocha", "pj_gray.h"));
        var fht = File.ReadAllLines(Path.Combine(root, "assets", "reference", "jalocha", "pj_fht.h"));
        var source = File.ReadAllLines(Path.Combine(root, "assets", "reference", "SOURCE.md"));

        Line("scrambling code", "pj_mfsk.h", mfsk, 1076, 1235);
        Line("scrambling applied", "pj_mfsk.h", mfsk, 1170, 1171, 1174, 1175);
        Line("shift 13 per character", "pj_mfsk.h", mfsk, 1185, 1192, 1335, 1337);
        Line("character to Walsh index", "pj_mfsk.h", mfsk, 1135, 1156, 1161, 1162, 1163, 1164, 1165);
        Line("Walsh bit to tone bit", "pj_mfsk.h", mfsk, 1195, 1196, 1197, 1202);
        Line("inverse Walsh butterfly", "pj_fht.h", fht, 40, 41, 42, 43);
        Line("Gray code", "pj_mfsk.h", mfsk, 168);
        Line("Gray code", "pj_gray.h", gray, 11, 12);
        Line("tone position", "pj_mfsk.h", mfsk, 172, 1722, 1723);
        Line("the pin", "SOURCE.md", source, 3, 4, 5);

        _output.WriteLine("");
        _output.WriteLine("== 4. where a demodulator plugs in");
        _output.WriteLine("   RsidDetection: Code, Name, Mode, Variant (\"16/500\"), CenterHz, Quality, TonesRight, StartSeconds (first tone).");
        _output.WriteLine($"   burst end = StartSeconds + Symbols / SymbolRateHz = StartSeconds + {codes.Symbols} / {codes.SymbolRateHz} = +{codes.Symbols / codes.SymbolRateHz:0.000} s");
        _output.WriteLine($"   OliviaData.Current.Problem: {OliviaData.Current.Problem ?? "(none)"}; malformed file -> null value and a sentence naming it");
        _output.WriteLine($"   resampler: Psk31Resampler, TargetSampleRate {Psk31Resampler.TargetSampleRate} (the RSID path's _rsidResampler in MainWindowViewModel)");
        _output.WriteLine($"   telemetry: ITelemetry.Write(TelemetryCategory.{TelemetryCategory.Psk31}, ...) as Psk31Events writes the PSK31 receive events");

        _output.WriteLine("");
        _output.WriteLine("== 5. the RSID detector's CPU alone, per fixture used by this unit");

        foreach (var file in new[]
                 {
                     "olivia-8-250-cq-rsid.wav", "olivia-16-500-qso-rsid.wav", "olivia-32-1000-qso-rsid.wav",
                     "olivia-16-500-qso-snr-10db.wav", "olivia-16-500-qso-snr-16db.wav", "olivia-noise-only-30s.wav",
                 })
        {
            var audio = WavAudio.Read(Path.Combine(fixtures, file));
            var cpu = Process.GetCurrentProcess().TotalProcessorTime;
            var heard = RsidDetector.Detect(codes, audio, Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz);
            var cpuSeconds = (Process.GetCurrentProcess().TotalProcessorTime - cpu).TotalSeconds;

            _output.WriteLine(
                $"   {file}: {audio.Samples.Length / (double)audio.SampleRate:0.00} s of audio, detector cpu {cpuSeconds:0.000} s, "
                + $"bursts {heard.Count}" + string.Concat(heard.Select(h => $"; {h.Name} at {h.CenterHz:0.00} Hz")));
        }
    }

    private void Line(string what, string file, string[] lines, params int[] numbers)
    {
        foreach (var n in numbers)
        {
            _output.WriteLine($"   {what,-26} {file}:{n,-5} {lines[n - 1].Trim()}");
        }
    }

    /// <summary>The averaged spectrum of the body: its tones, band and edges against the center.</summary>
    /// <remarks>
    /// Windows one analysis window (512 samples at 8000 Hz) long, raised-cosine shaped as the
    /// transmitter shapes a symbol, zero-padded to 2048 so the bins are an eighth of a tone apart.
    /// A tone is a local maximum within 10 dB of the strongest and the strongest within 10 Hz either side.
    /// </remarks>
    private void Spectrum(ReadOnlySpan<float> body, int rate, double centerHz, int tones, int bandwidth)
    {
        const int size = 2048;
        const int length = 512;
        var fft = new RealFft(size);
        var sum = new double[fft.BinCount];
        var mags = new double[fft.BinCount];
        var re = new double[size];
        var im = new double[size];
        var window = new float[size];
        var frames = 0;

        for (var at = 0; at + length <= body.Length; at += length / 2)
        {
            for (var i = 0; i < length; i++)
            {
                window[i] = (float)(body[at + i] * 0.5 * (1 - Math.Cos(2 * Math.PI * i / length)));
            }

            fft.Magnitudes(window, mags, re, im);

            for (var b = 0; b < sum.Length; b++)
            {
                sum[b] += mags[b] * mags[b];
            }

            frames++;
        }

        var binHz = (double)rate / size;
        var max = sum.Max();
        var near = (int)Math.Ceiling(10 / binHz);
        var peaks = new List<double>();

        for (var b = near; b < sum.Length - near; b++)
        {
            var isPeak = sum[b] > max * 0.1;

            for (var k = 1; k <= near && isPeak; k++)
            {
                isPeak = sum[b] >= sum[b - k] && sum[b] > sum[b + k];
            }

            if (isPeak)
            {
                // A parabola through the peak and its neighbors puts it between bins.
                var a = sum[b - 1];
                var c = sum[b + 1];
                var shift = 0.5 * (a - c) / (a - (2 * sum[b]) + c);
                peaks.Add((b + shift) * binHz);
            }
        }

        var low20 = Enumerable.Range(0, sum.Length).First(b => sum[b] > max * 0.01) * binHz;
        var high20 = Enumerable.Range(0, sum.Length).Last(b => sum[b] > max * 0.01) * binHz;
        var spacings = peaks.Zip(peaks.Skip(1), (a, b) => b - a).ToArray();

        _output.WriteLine(
            $"   spectrum ({frames} windows of {length} padded to {size}, {binHz:0.000} Hz bins): {peaks.Count} tones (expected {tones}); "
            + $"lowest {peaks.FirstOrDefault():0.00} Hz, highest {peaks.LastOrDefault():0.00} Hz, "
            + $"midpoint {(peaks.FirstOrDefault() + peaks.LastOrDefault()) / 2:0.00} Hz against center {centerHz:0.00}; "
            + $"mean spacing {(spacings.Length == 0 ? 0 : spacings.Average()):0.000} Hz");
        _output.WriteLine(
            $"   occupied within 20 dB of the strongest bin: {low20:0.0} to {high20:0.0} Hz = {high20 - low20:0.0} Hz against bandwidth {bandwidth} "
            + $"({centerHz - bandwidth / 2.0:0.0} to {centerHz + bandwidth / 2.0:0.0})");
    }

    /// <summary>The symbol period the audio shows: the period at which the tone changes.</summary>
    /// <remarks>
    /// Every 16 samples, the power in the band is measured over a 256-sample raised-cosine window
    /// at 32 frequencies spread across it; the change from one measurement to the next is largest
    /// where one symbol hands over to the next. The first local maximum at or past 90% of the largest value of the autocorrelation of
    /// that change is the period.
    /// </remarks>
    private void SymbolPeriod(ReadOnlySpan<float> body, int rate, double centerHz, int bandwidth)
    {
        const int hop = 16;
        const int length = 256;
        const int points = 32;
        var n = (Math.Min(body.Length, rate * 20) - length) / hop;
        var previous = new double[points];
        var flux = new double[n];

        for (var i = 0; i < n; i++)
        {
            double change = 0;

            for (var p = 0; p < points; p++)
            {
                var hz = centerHz - (bandwidth / 2.0) + ((p + 0.5) * bandwidth / points);
                double re = 0, im = 0;

                for (var k = 0; k < length; k++)
                {
                    var w = body[(i * hop) + k] * 0.5 * (1 - Math.Cos(2 * Math.PI * k / length));
                    var angle = 2 * Math.PI * hz * k / rate;
                    re += w * Math.Cos(angle);
                    im -= w * Math.Sin(angle);
                }

                var power = (re * re) + (im * im);

                change += Math.Abs(power - previous[p]);
                previous[p] = power;
            }

            flux[i] = i == 0 ? 0 : change;
        }

        var mean = flux.Average();

        for (var i = 0; i < n; i++)
        {
            flux[i] -= mean;
        }

        var lags = new List<(int Lag, double Value)>();

        for (var lag = 64 / hop; lag <= 1024 / hop; lag++)
        {
            double s = 0;

            for (var i = 0; i + lag < n; i++)
            {
                s += flux[i] * flux[i + lag];
            }

            lags.Add((lag, s));
        }

        var top = lags.Max(l => l.Value);
        var index = lags.FindIndex(l => l.Value >= 0.9 * top);

        while (index + 1 < lags.Count && lags[index + 1].Value > lags[index].Value)
        {
            index++;
        }

        var first = lags[index].Lag;

        _output.WriteLine(
            $"   symbol period from the audio: {first * hop} samples = {first * hop * 1000.0 / rate:0.000} ms "
            + $"(tone-change autocorrelation, {hop}-sample resolution, first 20 s after the burst)");
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
