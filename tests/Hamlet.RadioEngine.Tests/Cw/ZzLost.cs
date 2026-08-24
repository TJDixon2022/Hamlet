using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

public sealed class ZzLost
{
    private readonly ITestOutputHelper _output;

    public ZzLost(ITestOutputHelper output) => _output = output;

    private static MonoAudio Read(string name) => WavAudio.Read(
        Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

    [Fact]
    public void GuardOrEstimator()
    {
        var rows = new (string Name, double Tone, string Call)[]
        {
            ("cw-2026-08-17-013347", 600, "VA3VRR"),
            ("cw-2026-08-17-013622", 600, ""),
            ("cw-2026-08-17-134712", 600, "N4L"),
            ("cw-2026-08-18-004507", 501, "ARRL"),
            ("unadjudicated/cw-2026-08-18-003016", 669, "JUST"),
            ("unadjudicated/cw-2026-08-18-003126", 675, "WATCH"),
            ("unadjudicated/cw-2026-08-18-003758", 501, "AA4MP"),
            ("unadjudicated/cw-2026-08-24-012403", 439.81, "KD0UN"),
            ("unadjudicated/cw-2026-08-22-031905", 499.9, "PREDICTED"),
            ("unadjudicated/cw-2026-08-23-001520", 600, ""),
            ("unadjudicated/cw-2026-08-20-014854", 600, ""),
            ("unadjudicated/cw-2026-08-20-014935", 825, ""),
        };

        foreach (var (name, tone, call) in rows)
        {
            var audio = Read(name);
            var env = CwProbabilisticDecoder.Envelope(
                audio.Samples, audio.SampleRate, tone);

            // 1. whole file, guard on
            var wholeGated = CwProbabilisticDecoder.Decode(env, tone);

            // 2. whole file, guard off
            var wholeUngated = CwProbabilisticDecoder.DecodeForMeasurement(
                env, tone, ungated: true, CwProbabilisticDecoder.NoiseSpanSeconds);

            // 3. streaming, guard on (production), and every window's ratio
            var decoder = new CwDecoder(audio.SampleRate, tone);
            var settled = new List<CwCharacter>();
            var ratios = new List<double>();
            var hop = decoder.Tracker.HopSamples;
            var last = double.NaN;

            decoder.CharacterSettled += settled.Add;

            for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
            {
                decoder.Process(new AudioChunk(
                    at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));

                var r = decoder.Stream.Last.LikelihoodRatio;

                if (!r.Equals(last))
                {
                    last = r;
                    ratios.Add(r);
                }
            }

            decoder.Flush();

            var streamText = string.Concat(settled.Select(c => c.Text));
            var scored = ratios.Where(r => r != 0).OrderBy(r => r).ToArray();
            var cleared = scored.Count(r => r >= CwProbabilisticDecoder.Gate);

            _output.WriteLine($"--- {Path.GetFileName(name)} (looking for '{call}')");
            _output.WriteLine(
                $"    whole gated   : ratio {wholeGated.LikelihoodRatio:0.00}, "
                + $"{wholeGated.Characters.Count} chars, "
                + $"call {(Has(wholeGated, call) ? "YES" : "no")}");
            _output.WriteLine(
                $"    whole ungated : ratio {wholeUngated.LikelihoodRatio:0.00}, "
                + $"{wholeUngated.Characters.Count} chars, "
                + $"call {(Has(wholeUngated, call) ? "YES" : "no")}");
            _output.WriteLine(
                $"    streaming     : {settled.Count} chars, "
                + $"call {(call.Length > 0 && streamText.Contains(call, StringComparison.Ordinal) ? "YES" : "no")}");
            string At(double share)
                => scored.Length == 0
                    ? "-"
                    : scored[Math.Clamp(
                        (int)Math.Round(share * (scored.Length - 1)),
                        0,
                        scored.Length - 1)].ToString("0.000");

            _output.WriteLine(
                $"    WINDOWS       : {scored.Length} scored, {cleared} cleared {CwProbabilisticDecoder.Gate:0}, "
                + $"min {At(0)} / P50 {At(0.5)} / P75 {At(0.75)} / P90 {At(0.9)} / "
                + $"max {At(1)}");
            _output.WriteLine($"    streamed      : {Trim(streamText)}");
        }
    }

    [Fact]
    public void WhatTheEstimatorSeesOnTheLostOnes()
    {
        foreach (var (name, tone) in new[]
        {
            ("cw-2026-08-17-013347", 600.0),
            ("cw-2026-08-17-134712", 600.0),
            ("unadjudicated/cw-2026-08-23-001520", 600.0),
            ("cw-2026-08-18-004507", 501.0),
        })
        {
            var audio = Read(name);
            var zeros = audio.Samples.Count(s => s == 0f);

            var longest = 0;
            var run = 0;

            foreach (var s in audio.Samples)
            {
                run = s == 0f ? run + 1 : 0;
                longest = Math.Max(longest, run);
            }

            var env = CwProbabilisticDecoder.Envelope(
                audio.Samples, audio.SampleRate, tone);

            // What sigma and amplitude come to across the file, span by span.
            var span = Math.Max(
                8,
                (int)(CwProbabilisticDecoder.NoiseSpanSeconds * 1000.0
                    / CwProbabilisticDecoder.HopMilliseconds));

            var sigmas = new List<double>();
            var amps = new List<double>();

            for (var at = 0; at < env.Length; at += span / 8)
            {
                var from = Math.Clamp(at - (span / 2), 0, Math.Max(0, env.Length - span));
                var take = Math.Min(span, env.Length - from);
                var w = env.Skip(from).Take(take).OrderBy(x => x).ToArray();

                if (w.Length == 0)
                {
                    continue;
                }

                var q = w[(int)(0.25 * (w.Length - 1))];
                var a = w[(int)(0.97 * (w.Length - 1))];

                sigmas.Add(Math.Max(q / CwProbabilisticDecoder.RayleighQuarterPoint, 1e-9));
                amps.Add(a);
            }

            var clamped = sigmas.Count(s => s <= 1e-9 * 1.0001);

            _output.WriteLine(
                $"{Path.GetFileName(name)}: {100.0 * zeros / audio.Samples.Length:0.0} % exact zeros, "
                + $"longest run {longest} samples ({1000.0 * longest / audio.SampleRate:0} ms)");
            _output.WriteLine(
                $"    sigma over {sigmas.Count} spans: min {sigmas.Min():0.000000000} / "
                + $"median {sigmas.OrderBy(x => x).ElementAt(sigmas.Count / 2):0.000000} / "
                + $"max {sigmas.Max():0.000000}, {clamped} at the 1e-9 floor");
            _output.WriteLine(
                $"    amplitude: min {amps.Min():0.000000} / max {amps.Max():0.000000}, "
                + $"worst A/sigma {amps.Zip(sigmas, (a, s) => a / s).Max():0.0}");
        }
    }

    private static bool Has(CwProbabilisticResult r, string call)
        => call.Length > 0
           && string.Concat(r.Characters.Select(c => c.Text))
               .Contains(call, StringComparison.Ordinal);

    private static string Trim(string s)
        => s.Length <= 110 ? s : s[..110] + "...";
}
