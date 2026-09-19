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
/// Work instruction 363 task 1: **the trace before the listener is built.**
/// </summary>
/// <remarks>
/// <para>**THIS IS A MEASUREMENT, NOT AN ASSERTION**, the shape of `Unit362Trace`: the -14 dB audio
/// of decision W as it reads today, the two-signal file through what exists today, and what a
/// block costs in time. **It asserts nothing**, so it cannot become a wall. It runs in
/// <see cref="CpuMeasuredAlone"/> so the CPU it prints is its own.</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004).</para>
/// </remarks>
[Collection(CpuMeasuredAlone.Name)]
public sealed class Unit363Trace
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public Unit363Trace(ITestOutputHelper output) => _output = output;

    private static RsidCodes Codes => OliviaData.Current.Rsid!;

    private static OliviaFormat Format => OliviaData.Current.Format!;

    /// <summary>Item 1: the -14 dB fixture, made by decision W's recipe, read before any change.</summary>
    [Fact]
    public void TheBelowTheNoiseFixtureBeforeAnyChange()
    {
        var tenDb = OliviaFixtures.Load("olivia-16-500-qso-snr-10db.wav");
        var tenAudio = WavAudio.Read(tenDb.Path);
        var tenBurst = RsidDetector.Detect(Codes, tenAudio, Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz).First();

        _output.WriteLine(
            $"the method's check: olivia-16-500-qso-snr-10db.wav (manifest -10 dB) measures "
            + $"{OliviaBelowTheNoise.MeasureSnrDb(tenAudio, BurstEnd(tenBurst)):0.00} dB");

        foreach (var seed in new[] { OliviaBelowTheNoise.StatedSeed }.Concat(OliviaBelowTheNoise.FurtherSeeds))
        {
            var made = OliviaBelowTheNoise.Make(seed);
            var measured = OliviaBelowTheNoise.MeasureSnrDb(made.Audio, made.BurstStartSeconds + (Codes.Symbols / Codes.SymbolRateHz));

            if (seed == OliviaBelowTheNoise.StatedSeed)
            {
                _output.WriteLine(
                    $"recipe: burst's first tone at {made.BurstStartSeconds:0.000} s by the detector; slice ends at sample {made.SliceEndSample} "
                    + $"= {made.SliceEndSeconds:0.0000} s (first tone + ({Codes.Symbols} tones + {Codes.SilenceSymbolsBefore} silence) / {Codes.SymbolRateHz} Hz); "
                    + $"QSO {made.Audio.Samples.Length - made.SliceEndSample} samples; signal power {made.SignalPower:0.000000}, noise variance {made.NoiseVariance:0.000000}");
                SliceCheck(made);
            }

            var heard = RsidDetector.Detect(Codes, made.Audio, Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz);

            _output.WriteLine(
                $"seed {seed}{(seed == OliviaBelowTheNoise.StatedSeed ? " (asserted in task 2)" : "")}: SNR set {OliviaBelowTheNoise.SnrDb:0.00} dB, "
                + $"measured back {measured:0.00} dB; detections {heard.Count}: "
                + string.Join("; ", heard.Select(d => $"{d.Name} at {d.CenterHz:0.00} Hz, {d.TonesRight} tones right, first tone {d.StartSeconds:0.000} s")));

            var d = heard.FirstOrDefault(h => h.Mode == "OLIVIA");

            if (d is null)
            {
                continue;
            }

            var demodulator = new OliviaDemodulator(Format, Format.Variant(d.Variant)!, d.CenterHz, made.Audio.SampleRate);
            var before = Process.GetCurrentProcess().TotalProcessorTime;
            var decoding = demodulator.Decode(made.Audio, BurstEnd(d));
            var cpu = (Process.GetCurrentProcess().TotalProcessorTime - before).TotalSeconds;
            var cer = OliviaFixtures.CharacterErrorRate(decoding.Text, made.Qso.Text);

            _output.WriteLine(
                $"   Decode at the detection: CER {cer:0.0000} (3.0's ceiling 0.10), characters {decoding.CharactersOut} of {made.Qso.Text.Length}, "
                + $"blocks decoded {decoding.BlocksDecoded}, rejected {decoding.BlocksRejected}, sync snr {decoding.SyncSnr:0.00}, "
                + $"lowest accepted {decoding.BlockSnrs.Where(s => s >= OliviaDemodulator.SyncThreshold).DefaultIfEmpty(0).Min():0.00}, "
                + $"highest rejected {decoding.BlockSnrs.Where(s => s < OliviaDemodulator.SyncThreshold).DefaultIfEmpty(0).Max():0.00}, cpu {cpu:0.000} s");

            if (cer > 0)
            {
                _output.WriteLine("   decoded : " + JsonSerializer.Serialize(decoding.Text));
            }
        }
    }

    /// <summary>Item 2: the two-signal file through what exists today.</summary>
    [Fact]
    public void TheTwoSignalFileThroughWhatExists()
    {
        var fixture = OliviaFixtures.Load("olivia-two-signals-rsid.wav");
        var audio = WavAudio.Read(fixture.Path);
        var halves = fixture.Text.Split(" | ");
        var calls = halves.Select(CallsignOf).ToArray();

        _output.WriteLine($"halves (the check, not the input): [{string.Join("] [", halves)}]; callsigns {string.Join(", ", calls)}");

        foreach (var d in RsidDetector.Detect(Codes, audio, Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz))
        {
            var before = Process.GetCurrentProcess().TotalProcessorTime;
            var decoding = new OliviaDemodulator(Format, Format.Variant(d.Variant)!, d.CenterHz, audio.SampleRate).Decode(audio, BurstEnd(d));
            var cpu = (Process.GetCurrentProcess().TotalProcessorTime - before).TotalSeconds;

            _output.WriteLine(
                $"{d.Variant} at {d.CenterHz:0.00} Hz: blocks decoded {decoding.BlocksDecoded}, rejected {decoding.BlocksRejected}, "
                + $"sync snr {decoding.SyncSnr:0.00}, first block {decoding.FirstBlockSeconds:0.000} s, cpu {cpu:0.000} s");

            for (var h = 0; h < halves.Length; h++)
            {
                _output.WriteLine(
                    $"   against half {h} ({calls[h]}): CER {OliviaFixtures.CharacterErrorRate(decoding.Text, halves[h]):0.0000}; "
                    + $"{calls[h]} appears {Count(decoding.Text, calls[h])} times");
            }

            _output.WriteLine("   decoded : " + JsonSerializer.Serialize(decoding.Text));
        }

        var started = Process.GetCurrentProcess().TotalProcessorTime;
        var search = new OliviaBlindSearch(Format).Search(audio, Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz);
        var searchCpu = (Process.GetCurrentProcess().TotalProcessorTime - started).TotalSeconds;

        _output.WriteLine(
            $"blind search, told nothing: candidates {search.Candidates.Count}, audio {search.AudioSeconds:0.000} s, cpu {searchCpu:0.000} s: "
            + string.Join("; ", search.Candidates.Select(c => $"{c.Variant.Name} at {c.CenterHz:0.00} Hz, confidence {c.Confidence:0.00}, weighed "
                + string.Join(", ", c.Trials.Select(t => $"{t.Variant} {t.SyncSnr:0.00}/{t.BlocksDecoded}")))));
    }

    /// <summary>Item 5: what a block costs in time, and the first accepted block after a burst's end.</summary>
    [Fact]
    public void WhatABlockCostsInTime()
    {
        foreach (var v in Format.Variants)
        {
            _output.WriteLine($"{v.Name,-8}: block {Format.SymbolsPerBlock} symbols x {v.SymbolSeconds * 1000:0.0} ms = {Format.SymbolsPerBlock * v.SymbolSeconds:0.000} s");
        }

        foreach (var file in new[] { "olivia-8-250-cq-rsid.wav", "olivia-16-500-qso-rsid.wav", "olivia-32-1000-qso-rsid.wav" })
        {
            var audio = WavAudio.Read(OliviaFixtures.Load(file).Path);
            var d = Assert.Single(RsidDetector.Detect(Codes, audio, Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz));
            var variant = Format.Variant(d.Variant)!;
            var decoding = new OliviaDemodulator(Format, variant, d.CenterHz, audio.SampleRate).Decode(audio, BurstEnd(d));
            var block = Format.SymbolsPerBlock * variant.SymbolSeconds;

            _output.WriteLine(
                $"{file}: burst's end {BurstEnd(d):0.000} s, first accepted block begins {decoding.FirstBlockSeconds:0.000} s, "
                + $"{decoding.FirstBlockSeconds - BurstEnd(d):0.000} s after it, and ends {decoding.FirstBlockSeconds + block - BurstEnd(d):0.000} s after it");
        }
    }

    private void SliceCheck(BelowTheNoiseAudio made)
    {
        // The CQ file's own samples, before the noise: where the Olivia tones start after the burst.
        var cq = WavAudio.Read(OliviaFixtures.Load("olivia-8-250-cq-rsid.wav").Path);
        var rate = cq.SampleRate;
        var burstEnd = (int)Math.Round((made.BurstStartSeconds + (Codes.Symbols / Codes.SymbolRateHz)) * rate);
        var peak = cq.Samples.Max(s => Math.Abs(s));
        var onset = Array.FindIndex(cq.Samples, burstEnd, s => Math.Abs(s) > peak / 100);
        var quiet = cq.Samples.Skip(burstEnd).Take(made.SliceEndSample - burstEnd).Select(s => s * (double)s).DefaultIfEmpty(0).Average();
        var loud = cq.Samples.Skip(made.SliceEndSample).Take(rate).Select(s => s * (double)s).Average();
        var qso = WavAudio.Read(made.Qso.Path);
        var qsoOnset = Array.FindIndex(qso.Samples, s => Math.Abs(s) > peak / 100);

        _output.WriteLine(
            $"   the check on the slice: the burst's tones end at sample {burstEnd}; the CQ file's first sample over 1% of its peak after that is {onset} "
            + $"({onset / (double)rate:0.0000} s, {onset - made.SliceEndSample} samples from the slice end); mean square between the burst's end and the slice end "
            + $"{quiet:0.000000000}, over the second after the slice end {loud:0.000000}; the QSO file's first sample over that level is {qsoOnset}");
    }

    private static double BurstEnd(RsidDetection d) => d.StartSeconds + (Codes.Symbols / Codes.SymbolRateHz);

    private static string CallsignOf(string half)
    {
        var words = half.Split(' ');
        var at = Array.IndexOf(words, "de");

        return at >= 0 && at + 1 < words.Length ? words[at + 1] : "";
    }

    private static int Count(string text, string word)
        => word.Length == 0 ? 0 : (text.Length - text.Replace(word, "", StringComparison.Ordinal).Length) / word.Length;
}
