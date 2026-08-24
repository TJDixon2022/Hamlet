using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

public sealed class ZzPremise
{
    private readonly ITestOutputHelper _output;

    public ZzPremise(ITestOutputHelper output) => _output = output;

    private static MonoAudio Read(string name) => WavAudio.Read(
        Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

    private static MonoAudio Slice(MonoAudio a, double from, double to)
    {
        var f = Math.Clamp((int)(from * a.SampleRate), 0, a.Samples.Length);
        var t = Math.Clamp((int)(to * a.SampleRate), f, a.Samples.Length);

        return new MonoAudio(a.SampleRate, a.Samples[f..t]);
    }

    [Fact]
    public void TheThreeFigures()
    {
        // 012403, 20-30 s, forced to 20 WPM, at the measured pitch.
        var kd = Slice(Read("unadjudicated/cw-2026-08-24-012403"), 20, 30);

        foreach (var pitch in new[] { 439.81, 450.0, 600.0 })
        {
            var env = CwProbabilisticDecoder.Envelope(kd.Samples, kd.SampleRate, pitch);
            var at20 = CwProbabilisticDecoder.Decode(env, pitch, 20.0);
            var free = CwProbabilisticDecoder.Decode(env, pitch);

            _output.WriteLine(
                $"012403 20-30s @ {pitch:0.00} Hz: forced 20 wpm ratio "
                + $"{at20.LikelihoodRatio:0.00} '{Text(at20)}' | free "
                + $"{free.WordsPerMinute:0} wpm ratio {free.LikelihoodRatio:0.00} "
                + $"'{Text(free)}'");
        }

        foreach (var (name, pitch) in new[]
        {
            ("unadjudicated/cw-2026-08-22-031905", 499.9),
            ("unadjudicated/cw-2026-08-23-001520", 600.0),
        })
        {
            var a = Read(name);
            var r = CwProbabilisticDecoder.Decode(a, pitch);

            _output.WriteLine(
                $"{Path.GetFileName(name)} @ {pitch:0.0} Hz: ratio "
                + $"{r.LikelihoodRatio:0.00} ({r.WordsPerMinute:0} wpm) '{Text(r)}'");
        }

        // How much of 001520 is exact zeros.
        var z = Read("unadjudicated/cw-2026-08-23-001520");
        var zeros = z.Samples.Count(s => s == 0f);

        _output.WriteLine(
            $"001520: {zeros} of {z.Samples.Length} samples are exact zeros "
            + $"({100.0 * zeros / z.Samples.Length:0.0} %)");
    }

    [Fact]
    public void TheContradictionInUnit1113()
    {
        var a = Read("cw-2026-08-17-134712");

        // What the whole-file offline read scores.
        var whole = CwProbabilisticDecoder.Decode(a, 600);

        // What the streaming path's LAST window scores, which is what unit
        // 1.11.3's corpus table printed under "window".
        var decoder = new CwDecoder(a.SampleRate, 600);
        var settled = new List<CwCharacter>();
        var ratios = new List<double>();
        var hop = decoder.Tracker.HopSamples;
        var last = double.NaN;

        decoder.CharacterSettled += settled.Add;

        for (var at = 0L; at + hop <= a.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, a.SampleRate, a.Samples.AsSpan((int)at, hop)));

            var r = decoder.Stream.Last.LikelihoodRatio;

            if (!r.Equals(last))
            {
                last = r;
                ratios.Add(r);
            }
        }

        decoder.Flush();

        var scored = ratios.Where(r => r != 0).OrderBy(r => r).ToArray();

        _output.WriteLine(
            $"134712 whole-file offline ratio: {whole.LikelihoodRatio:0.00}, "
            + $"{whole.Characters.Count} characters");
        _output.WriteLine(
            $"134712 streaming: {ratios.Count} reads, "
            + $"min {scored.FirstOrDefault():0.00} / "
            + $"median {(scored.Length == 0 ? 0 : scored[scored.Length / 2]):0.00} / "
            + $"max {scored.LastOrDefault():0.00}, "
            + $"last {decoder.Stream.Last.LikelihoodRatio:0.00}");
        _output.WriteLine(
            $"134712 streaming emitted {settled.Count(c => !c.IsWordGap)} characters");
        _output.WriteLine(
            $"   above the gate of {CwProbabilisticDecoder.Gate:0}: "
            + $"{scored.Count(r => r >= CwProbabilisticDecoder.Gate)} of {scored.Length}");
    }

    private static string Text(CwProbabilisticResult r)
        => string.Concat(r.Characters.Select(c => c.Text));
}
