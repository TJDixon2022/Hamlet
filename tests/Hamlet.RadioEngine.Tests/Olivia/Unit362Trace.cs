using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Rsid;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Olivia;

/// <summary>
/// Work instruction 362 tasks 0 and 1: **the blind case's ground, and the trace before the search
/// is built.**
/// </summary>
/// <remarks>
/// <para>**THIS IS A MEASUREMENT, NOT AN ASSERTION**, the shape of `Unit361Trace`. It prints what
/// the no-RSID file and the noise look like to something that knows nothing about them, the
/// format's seven rows, what a trial decode costs at a right and a wrong variant, and where drift
/// will bite. **It asserts nothing**, so it cannot become a wall.</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004).</para>
/// </remarks>
public sealed class Unit362Trace
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public Unit362Trace(ITestOutputHelper output) => _output = output;

    /// <summary>Task 0: the no-RSID file hashes as its manifest says, and the RSID detector hears nothing in it.</summary>
    [Fact]
    public void TheBlindFileAnnouncesNothing()
    {
        var fixture = OliviaFixtures.Load("olivia-8-250-qso-norsid.wav");
        var audio = WavAudio.Read(fixture.Path);
        var before = Process.GetCurrentProcess().TotalProcessorTime;
        var heard = RsidDetector.Detect(OliviaData.Current.Rsid!, audio, Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz);
        var cpu = (Process.GetCurrentProcess().TotalProcessorTime - before).TotalSeconds;

        _output.WriteLine(
            $"{Path.GetFileName(fixture.Path)}: hash matches the manifest; {audio.SampleRate} Hz, "
            + $"{audio.Samples.Length / (double)audio.SampleRate:0.00} s; RSID detections {heard.Count} "
            + $"over {Psk31CarrierSearch.PassbandLowHz}-{Psk31CarrierSearch.PassbandHighHz} Hz; detector cpu {cpu:0.000} s");

        foreach (var d in heard)
        {
            _output.WriteLine($"   detection: {d.Name} code {d.Code} at {d.CenterHz:0.00} Hz, {d.StartSeconds:0.000} s");
        }
    }

    /// <summary>Task 1: the five items, printed.</summary>
    [Fact]
    public void TheBlindSearchsTrace()
    {
        var format = OliviaData.Current.Format!;
        var blind = OliviaFixtures.Load("olivia-8-250-qso-norsid.wav");
        var noise = OliviaFixtures.Load("olivia-noise-only-30s.wav");
        var blindAudio = WavAudio.Read(blind.Path);
        var noiseAudio = WavAudio.Read(noise.Path);

        _output.WriteLine("== 1 and 2. each file measured as if nothing were known about it, from 1 s on");

        var measured = Measure(format, blindAudio, "olivia-8-250-qso-norsid.wav");

        _output.WriteLine($"   the check, not the input: manifest variant {blind.Variant}, center {blind.CenterHz} Hz");
        Measure(format, noiseAudio, "olivia-noise-only-30s.wav");

        _output.WriteLine("");
        _output.WriteLine("== 3. format.json's rows, and which the two measurements separate");

        foreach (var v in format.Variants)
        {
            _output.WriteLine(
                $"   {v.Name,-8} tones {v.Tones,2}, bandwidth {v.BandwidthHz,4} Hz, spacing {v.ToneSpacingHz,6:0.00} Hz, "
                + $"symbol {v.SymbolSeconds * 1000,5:0.0} ms, first tone {v.FirstToneOffsetHz,8:0.000} Hz");
        }

        foreach (var a in format.Variants)
        {
            foreach (var b in format.Variants.Where(b => string.CompareOrdinal(a.Name, b.Name) < 0))
            {
                var spacing = a.ToneSpacingHz == b.ToneSpacingHz;
                var band = a.BandwidthHz == b.BandwidthHz;

                if (spacing || band)
                {
                    _output.WriteLine(
                        $"   {a.Name} and {b.Name}: same {(spacing ? "spacing" : "bandwidth")}, "
                        + $"{(spacing && band ? "NOT SEPARATED" : "separated by the " + (spacing ? "bandwidth" : "spacing"))}");
                }
            }
        }

        _output.WriteLine("");
        _output.WriteLine($"== 4. a trial decode of the no-RSID file at the measured center {measured:0.00} Hz, at each of the phase's three");

        foreach (var name in new[] { "8/250", "16/500", "32/1000" })
        {
            var v = format.Variant(name)!;
            var before = Process.GetCurrentProcess().TotalProcessorTime;
            var d = new OliviaDemodulator(format, v, measured, blindAudio.SampleRate).Decode(blindAudio, 0);
            var cpu = (Process.GetCurrentProcess().TotalProcessorTime - before).TotalSeconds;

            _output.WriteLine(
                $"   as {name,-8}: CER {OliviaFixtures.CharacterErrorRate(d.Text, blind.Text):0.0000}, blocks decoded {d.BlocksDecoded}, "
                + $"rejected {d.BlocksRejected}, sync snr {d.SyncSnr:0.00}, highest block snr {d.BlockSnrs.DefaultIfEmpty(0).Max():0.00}, "
                + $"median block snr {Median(d.BlockSnrs):0.00}, offset {d.FrequencyOffsetHz:0.00} Hz, cpu {cpu:0.000} s");
        }

        foreach (var seconds in new[] { 4.0, 8.0, 16.0 })
        {
            var cut = new MonoAudio(blindAudio.SampleRate, blindAudio.Samples.Take((int)(seconds * blindAudio.SampleRate)).ToArray());

            foreach (var name in new[] { "8/250", "16/500", "32/1000" })
            {
                var before = Process.GetCurrentProcess().TotalProcessorTime;
                var d = new OliviaDemodulator(format, format.Variant(name)!, measured, cut.SampleRate).Decode(cut, 0);
                var cpu = (Process.GetCurrentProcess().TotalProcessorTime - before).TotalSeconds;

                _output.WriteLine(
                    $"   first {seconds,4:0.0} s as {name,-8}: blocks decoded {d.BlocksDecoded}, rejected {d.BlocksRejected}, "
                    + $"highest block snr {d.BlockSnrs.DefaultIfEmpty(0).Max():0.00}, cpu {cpu:0.000} s");
            }

            var n = new MonoAudio(noiseAudio.SampleRate, noiseAudio.Samples.Take((int)(seconds * noiseAudio.SampleRate)).ToArray());
            var dn = new OliviaDemodulator(format, format.Variant("8/250")!, measured, n.SampleRate).Decode(n, 0);

            _output.WriteLine($"   first {seconds,4:0.0} s of noise as 8/250: highest block snr {dn.BlockSnrs.DefaultIfEmpty(0).Max():0.00}");
        }

        _output.WriteLine("");
        _output.WriteLine("== 5. where drift will bite");
        _output.WriteLine("   OliviaDemodulator.Decode step 2 (OliviaDemodulator.cs:241-266) scores every offset within half a tone of the");
        _output.WriteLine("   center over EVERY frame of the recording and keeps the one best: one offset for the whole file.");

        foreach (var file in new[] { "olivia-16-500-qso-rsid.wav", "olivia-8-250-cq-rsid.wav" })
        {
            var f = OliviaFixtures.Load(file);
            var a = WavAudio.Read(f.Path);
            var seconds = a.Samples.Length / (double)a.SampleRate;
            var hz = 20.0 * seconds / 60;
            var spacing = format.Variant(f.Variant!)!.ToneSpacingHz;

            _output.WriteLine(
                $"   {file}: {seconds:0.00} s at 20 Hz a minute moves {hz:0.00} Hz, {hz / spacing:0.000} tone spacings of {spacing} Hz; "
                + $"one offset at the middle leaves each end {hz / 2:0.00} Hz off, {hz / 2 / spacing:0.000} spacings");
        }
    }

    private double Measure(OliviaFormat format, MonoAudio audio, string name)
    {
        var rate = audio.SampleRate;
        var skip = rate;
        var narrowest = format.Variants.Min(v => v.ToneSpacingHz);
        var longest = format.Variants.Max(v => v.SymbolSeconds);
        var shortest = format.Variants.Min(v => v.SymbolSeconds);

        // The long view: a Welch average with bins an eighth of the narrowest spacing in the file.
        var size = (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)Math.Ceiling(rate / (narrowest / 8)));
        var average = Welch(audio.Samples.AsSpan(skip), size, size / 2);
        var binHz = (double)rate / size;
        var low = (int)Math.Ceiling(Psk31CarrierSearch.PassbandLowHz / binHz);
        var high = (int)Math.Floor(Psk31CarrierSearch.PassbandHighHz / binHz);
        var band = average[low..(high + 1)];
        var floor = Median(band);
        var peakBin = low + Array.IndexOf(band, band.Max());
        var peak = average[peakBin];
        var lowEdge = peakBin;
        var highEdge = peakBin;

        while (lowEdge > low && average[lowEdge - 1] >= peak / 100)
        {
            lowEdge--;
        }

        while (highEdge < high && average[highEdge + 1] >= peak / 100)
        {
            highEdge++;
        }

        _output.WriteLine(
            $"   {name}: bins {binHz:0.000} Hz; floor (median over the passband) {10 * Math.Log10(floor):0.0} dB, "
            + $"peak {10 * Math.Log10(peak):0.0} dB at {peakBin * binHz:0.00} Hz, {10 * Math.Log10(peak / floor):0.00} dB over the floor; "
            + $"band 20 dB down from the peak {lowEdge * binHz:0.00} to {highEdge * binHz:0.00} Hz = {(highEdge - lowEdge + 1) * binHz:0.00} Hz wide, "
            + $"middle {(lowEdge + highEdge) / 2.0 * binHz:0.00} Hz");

        var within3 = band.Count(p => p >= floor * 2);

        _output.WriteLine($"      bins more than 3 dB over the floor: {within3} of {band.Length}; top of the noise-only band ratio max/median {band.Max() / floor:0.000}");

        // The ripple: the spectrum's own autocorrelation across the occupied band.
        var slice = average[lowEdge..(highEdge + 1)].Select(p => 10 * Math.Log10(p)).ToArray();
        var bestLag = PeriodAfterFirstDip(slice, slice.Length / 2);

        _output.WriteLine(
            $"      spectral ripple period (autocorrelation, first peak after its first dip, across the band): {bestLag * binHz:0.00} Hz; "
            + $"band over it {(highEdge - lowEdge + 1) / (double)Math.Max(bestLag, 1):0.00} spacings");

        // The short view: one window two of the longest symbols long, hopped a quarter of the shortest.
        var window = 2 * (int)Math.Round(longest * rate);
        var hop = Math.Max(1, (int)Math.Round(shortest * rate / 4));
        var shortSize = 4 * (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)window);
        var shortHz = (double)rate / shortSize;
        var fft = new RealFft(shortSize);
        var magnitudes = new double[fft.BinCount];
        var real = new double[shortSize];
        var imaginary = new double[shortSize];
        var shaped = new float[shortSize];
        var frames = (Math.Min(audio.Samples.Length, 40 * rate) - window) / hop;
        var peaks = new double[frames];
        var levels = new double[frames];
        var flux = new double[frames];
        double[]? previous = null;
        var sLow = (int)Math.Floor(lowEdge * binHz / shortHz);
        var sHigh = (int)Math.Ceiling(highEdge * binHz / shortHz);

        for (var f = 0; f < frames; f++)
        {
            for (var i = 0; i < window; i++)
            {
                shaped[i] = (float)(audio.Samples[(f * hop) + i] * 0.5 * (1 - Math.Cos(2 * Math.PI * i / window)));
            }

            fft.Magnitudes(shaped, magnitudes, real, imaginary);

            var row = new double[sHigh - sLow + 1];
            var top = 0;

            for (var b = 0; b < row.Length; b++)
            {
                row[b] = magnitudes[sLow + b] * magnitudes[sLow + b];

                if (row[b] > row[top])
                {
                    top = b;
                }
            }

            var delta = 0.0;

            if (top > 0 && top < row.Length - 1)
            {
                var (l, c, r) = (Math.Log(row[top - 1] + 1e-30), Math.Log(row[top] + 1e-30), Math.Log(row[top + 1] + 1e-30));
                var d = l - (2 * c) + r;

                delta = d == 0 ? 0 : 0.5 * (l - r) / d;
            }

            peaks[f] = (sLow + top + delta) * shortHz;
            levels[f] = row.Sum();

            if (previous is not null)
            {
                for (var b = 0; b < row.Length; b++)
                {
                    flux[f] += Math.Abs(Math.Sqrt(row[b]) - Math.Sqrt(previous[b]));
                }
            }

            previous = row;
        }

        var quiet = Median(levels);
        var begins = Array.FindIndex(levels, l => l > quiet / 10 && l > 0) * hop / (double)rate;

        _output.WriteLine(
            $"      short view: window {window} samples, hop {hop}, bins {shortHz:0.000} Hz, over the first {frames * hop / (double)rate:0.0} s; "
            + $"band power first reaches a tenth of its median at {begins + 0:0.000} s from the start (the first second included)");

        // Tone clusters: where the loudest frequency sits, frame by frame.
        var sorted = peaks.Where((p, i) => levels[i] > quiet / 10).OrderBy(p => p).ToArray();
        var clusters = new System.Collections.Generic.List<(double Hz, int Count)>();
        var start = 0;

        for (var i = 1; i <= sorted.Length; i++)
        {
            if (i == sorted.Length || sorted[i] - sorted[i - 1] > narrowest / 4)
            {
                clusters.Add((sorted[start..i].Average(), i - start));
                start = i;
            }
        }

        var strong = clusters.Where(c => c.Count >= sorted.Length / 100).ToArray();

        _output.WriteLine(
            $"      loudest-frequency clusters holding 1% of frames or more: {strong.Length} "
            + $"({string.Join(", ", strong.Select(c => $"{c.Hz:0.0}"))}); all clusters {clusters.Count}");

        // The same frames as a histogram a quarter of a short bin wide: its peaks are the tones.
        var histHz = shortHz / 4;
        var histogram = new double[(int)Math.Ceiling((sHigh - sLow + 1) * shortHz / histHz) + 1];

        foreach (var p in sorted)
        {
            histogram[Math.Clamp((int)Math.Round((p - (sLow * shortHz)) / histHz), 0, histogram.Length - 1)]++;
        }

        var histLag = PeriodAfterFirstDip(histogram, histogram.Length / 2);
        var tops = new System.Collections.Generic.List<double>();
        var tall = histogram.Max() / 4;

        for (var i = 1; i < histogram.Length - 1; i++)
        {
            if (histogram[i] >= tall && histogram[i] >= histogram[i - 1] && histogram[i] > histogram[i + 1])
            {
                tops.Add((sLow * shortHz) + (i * histHz));
            }
        }

        _output.WriteLine(
            $"      loudest-frequency histogram ({histHz:0.000} Hz cells): period {histLag * histHz:0.00} Hz; "
            + $"peaks a quarter of the tallest or more: {tops.Count} ({string.Join(", ", tops.Select(t => $"{t:0.0}"))})");

        if (tops.Count >= 2)
        {
            _output.WriteLine(
                $"      lowest tone {tops[0]:0.00} Hz, highest {tops[^1]:0.00} Hz, mean spacing {(tops[^1] - tops[0]) / (tops.Count - 1):0.000} Hz, "
                + $"middle of the tones {(tops[0] + tops[^1]) / 2:0.00} Hz");
        }

        // The symbol period: the lag at which the spectral change repeats.
        var fm = flux.Skip(1).Average();
        var ac = new double[Math.Min(frames / 4, (int)(4 * longest * rate / hop))];

        for (var lag = 1; lag < ac.Length; lag++)
        {
            double r = 0;

            for (var i = 1; i + lag < frames; i++)
            {
                r += (flux[i] - fm) * (flux[i + lag] - fm);
            }

            ac[lag] = r;
        }

        var first = 1;

        while (first + 1 < ac.Length && ac[first + 1] < ac[first])
        {
            first++;
        }

        var period = first;

        for (var lag = first; lag < ac.Length; lag++)
        {
            if (ac[lag] > ac[period])
            {
                period = lag;
            }
        }

        _output.WriteLine($"      symbol period (spectral change autocorrelation): {period * hop / (double)rate * 1000:0.00} ms (hop {hop / (double)rate * 1000:0.00} ms)");

        return (lowEdge + highEdge) / 2.0 * binHz;
    }

    private static int PeriodAfterFirstDip(double[] series, int maxLag)
    {
        var mean = series.Average();
        var r = new double[Math.Max(maxLag, 2)];

        for (var lag = 1; lag < r.Length; lag++)
        {
            for (var i = 0; i + lag < series.Length; i++)
            {
                r[lag] += (series[i] - mean) * (series[i + lag] - mean);
            }

            r[lag] /= series.Length - lag;
        }

        var first = 1;

        while (first + 1 < r.Length && r[first + 1] <= r[first])
        {
            first++;
        }

        var best = first;

        for (var lag = first; lag < r.Length; lag++)
        {
            if (r[lag] > r[best])
            {
                best = lag;
            }
        }

        return best;
    }

    private static double[] Welch(ReadOnlySpan<float> samples, int size, int hop)
    {
        var fft = new RealFft(size);
        var magnitudes = new double[fft.BinCount];
        var real = new double[size];
        var imaginary = new double[size];
        var shaped = new float[size];
        var sum = new double[fft.BinCount];
        var count = 0;

        for (var at = 0; at + size <= samples.Length; at += hop)
        {
            for (var i = 0; i < size; i++)
            {
                shaped[i] = (float)(samples[at + i] * 0.5 * (1 - Math.Cos(2 * Math.PI * i / size)));
            }

            fft.Magnitudes(shaped, magnitudes, real, imaginary);

            for (var b = 0; b < sum.Length; b++)
            {
                sum[b] += magnitudes[b] * magnitudes[b];
            }

            count++;
        }

        return sum.Select(s => s / Math.Max(count, 1)).ToArray();
    }

    private static double Median(System.Collections.Generic.IEnumerable<double> values)
    {
        var sorted = values.OrderBy(v => v).ToArray();

        return sorted.Length == 0 ? 0 : sorted[sorted.Length / 2];
    }
}
