using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

public sealed class ZzMarginProbe
{
    private readonly ITestOutputHelper _output;

    public ZzMarginProbe(ITestOutputHelper output) => _output = output;

    private void Dump(string label, CwProbabilisticResult r)
    {
        var letters = r.Characters.Where(c => c.Text != " ").ToList();

        if (letters.Count == 0)
        {
            _output.WriteLine($"{label}: ratio {r.LikelihoodRatio:0.00}, nothing");

            return;
        }

        var margins = letters.Select(c => c.SpanMargin).OrderBy(m => m).ToArray();

        string At(double share)
            => margins[Math.Clamp(
                (int)Math.Round(share * (margins.Length - 1)),
                0,
                margins.Length - 1)].ToString("0.00");

        _output.WriteLine(
            $"{label}: window {r.LikelihoodRatio:0.00}, {letters.Count} chars, "
            + $"margin P10 {At(0.1)} / P50 {At(0.5)} / P90 {At(0.9)}");
        _output.WriteLine("    " + string.Concat(r.Characters.Select(c => c.Text)));
        _output.WriteLine(
            "    " + string.Join(
                " ",
                letters.Take(22).Select(c => $"{c.Text}:{c.SpanMargin:0.0}")));
    }

    /// The same, with the window gate bypassed, so the empty captures can be
    /// asked what their characters would score if they ever reached the test.
    [Fact]
    public void WhatWouldTheEmptyBandsEmitWithNoWindowGate()
    {
        var folder = CapturedSignalTests.Folder;

        var rows = new (string Name, double Tone, string Holds)[]
        {
            ("unadjudicated/cw-2026-08-20-014854", 600, "nothing"),
            ("unadjudicated/cw-2026-08-20-014935", 825, "nothing"),
            ("cw-2026-08-17-134712", 600, "N4L, adjudicated"),
            ("cw-2026-08-18-004507", 501, "a station"),
        };

        foreach (var (name, tone, holds) in rows)
        {
            var audio = WavAudio.Read(Path.Combine(folder, name + ".wav"));

            var env = CwProbabilisticDecoder.Envelope(
                audio.Samples, audio.SampleRate, tone);

            var ungated = CwProbabilisticDecoder.DecodeUngated(env, tone);

            var letters = ungated.Characters.Where(c => c.Text != " ").ToList();
            var margins = letters.Select(c => c.SpanMargin).OrderBy(m => m).ToArray();

            string At(double share)
                => margins.Length == 0
                    ? "-"
                    : margins[Math.Clamp(
                        (int)Math.Round(share * (margins.Length - 1)),
                        0,
                        margins.Length - 1)].ToString("0.00");

            _output.WriteLine(
                $"UNGATED {Path.GetFileName(name)} ({holds}): "
                + $"window {ungated.LikelihoodRatio:0.00}, {letters.Count} chars, "
                + $"margin P10 {At(0.1)} / P50 {At(0.5)} / P90 {At(0.9)} / "
                + $"max {(margins.Length == 0 ? "-" : margins[^1].ToString("0.00"))}");
            _output.WriteLine(
                "    " + string.Concat(ungated.Characters.Select(c => c.Text)));
        }
    }

    [Fact]
    public void WhatDoCharacterMarginsLookLike()
    {
        // A generated signal where every character is known to be right.
        foreach (var db in new[] { 18.0, 3.0 })
        {
            var audio = CwSignal.Generate(new CwSignalRequest(
                "CQ DE W1AW K",
                WordsPerMinute: 18,
                ToneHz: 640,
                Amplitude: 0.5,
                NoiseAmplitude: CwSensitivity.NoiseFor(db),
                Seed: 7919));

            Dump($"generated {db:0} dB", CwProbabilisticDecoder.Decode(audio, 640));
        }

        var folder = CapturedSignalTests.Folder;

        var captures = new (string Name, double Tone)[]
        {
            ("cw-2026-08-18-004507", 501),
            ("cw-2026-08-17-013347", 600),
            ("cw-2026-08-17-134712", 600),
            ("unadjudicated/cw-2026-08-18-003758", 501),
            ("unadjudicated/cw-2026-08-20-014854", 600),
            ("unadjudicated/cw-2026-08-20-014935", 825),
        };

        foreach (var (name, tone) in captures)
        {
            var audio = WavAudio.Read(Path.Combine(folder, name + ".wav"));

            Dump(
                Path.GetFileName(name),
                CwProbabilisticDecoder.Decode(audio, tone));
        }
    }
}
