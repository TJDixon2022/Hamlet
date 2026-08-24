using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

public sealed class ZzPitch
{
    private readonly ITestOutputHelper _output;

    public ZzPitch(ITestOutputHelper output) => _output = output;

    private static MonoAudio Read(string name) => WavAudio.Read(
        Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

    private static IEnumerable<string> Corpus()
    {
        var folder = CapturedSignalTests.Folder;

        foreach (var p in Directory.GetFiles(folder, "*.wav").OrderBy(x => x))
        {
            yield return Path.GetFileNameWithoutExtension(p);
        }

        foreach (var p in Directory
            .GetFiles(Path.Combine(folder, "unadjudicated"), "*.wav")
            .OrderBy(x => x))
        {
            yield return "unadjudicated/" + Path.GetFileNameWithoutExtension(p);
        }
    }

    /// <summary>
    /// The true pitch: a full-length Goertzel sweep, peak interpolated between
    /// bins. Measured over the whole recording so keying does not move it.
    /// </summary>
    public static double TruePitch(MonoAudio a, double from = 300, double to = 1200)
    {
        const double step = 1.0;

        var best = from;
        var bestP = -1.0;
        var levels = new List<(double Hz, double Power)>();

        for (var hz = from; hz <= to; hz += step)
        {
            var p = Power(a, hz);

            levels.Add((hz, p));

            if (p > bestP)
            {
                bestP = p;
                best = hz;
            }
        }

        var at = levels.FindIndex(l => l.Hz == best);

        if (at <= 0 || at >= levels.Count - 1)
        {
            return best;
        }

        var l = Math.Log(Math.Max(levels[at - 1].Power, 1e-30));
        var c = Math.Log(Math.Max(levels[at].Power, 1e-30));
        var r = Math.Log(Math.Max(levels[at + 1].Power, 1e-30));

        var curve = l - (2 * c) + r;

        if (Math.Abs(curve) < 1e-12)
        {
            return best;
        }

        return best + (Math.Clamp(0.5 * (l - r) / curve, -0.5, 0.5) * step);
    }

    private static double Power(MonoAudio a, double hz)
    {
        var w = 2 * Math.PI * hz / a.SampleRate;
        var cw = 2 * Math.Cos(w);
        double s1 = 0, s2 = 0;

        foreach (var x in a.Samples)
        {
            var s0 = x + (cw * s1) - s2;
            s2 = s1;
            s1 = s0;
        }

        return (s1 * s1) + (s2 * s2) - (cw * s1 * s2);
    }

    [Fact]
    public void TheToneTable()
    {
        foreach (var name in Corpus())
        {
            var audio = Read(name);
            var truth = TruePitch(audio);

            // What Hamlet reports through the production path.
            var decoder = new CwDecoder(audio.SampleRate, 600);
            var settled = new List<CwCharacter>();
            var hop = decoder.Tracker.HopSamples;

            decoder.CharacterSettled += settled.Add;

            for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
            {
                decoder.Process(new AudioChunk(
                    at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
            }

            decoder.Flush();

            var reported = decoder.Tracker.ToneHz;
            var peak = decoder.Tracker.MeasuredPeakHz;

            // And what the decoder reads if it is simply pointed at the truth.
            var atTruth = CwProbabilisticDecoder.Decode(audio, truth);
            var atReported = CwProbabilisticDecoder.Decode(audio, reported);

            _output.WriteLine(
                $"{Path.GetFileName(name),-22} true {truth,7:0.00} | "
                + $"reported {reported,6:0.0} (err {reported - truth,7:0.0}) | "
                + $"peak {(double.IsNaN(peak) ? "     -" : peak.ToString("0.00")),8} | "
                + $"prod {settled.Count(c => !c.IsWordGap),3} ch | "
                + $"@true {atTruth.Characters.Count(c => c.Text != " "),3} ch "
                + $"r{atTruth.LikelihoodRatio,8:0.0} | "
                + $"@rep {atReported.Characters.Count(c => c.Text != " "),3} ch");
        }
    }

    [Fact]
    public void WhyDoTheyNotRead()
    {
        foreach (var name in new[]
        {
            "unadjudicated/cw-2026-08-22-014113",
            "cw-2026-08-18-004507",
        })
        {
            var audio = Read(name);
            var truth = TruePitch(audio);
            var env = CwProbabilisticDecoder.Envelope(
                audio.Samples, audio.SampleRate, truth);

            var sorted = env.OrderBy(x => x).ToArray();

            double At(double q) => sorted[(int)(q * (sorted.Length - 1))];

            var (down, up) = CwProbabilisticDecoder.LogLikelihoods(env);
            var better = down.Zip(up, (d, u) => d > u).Count(x => x);

            _output.WriteLine(
                $"{Path.GetFileName(name)} @ {truth:0.00} Hz: {env.Length} hops");
            _output.WriteLine(
                $"    envelope P05 {At(0.05):0.000000} P25 {At(0.25):0.000000} "
                + $"P50 {At(0.50):0.000000} P75 {At(0.75):0.000000} "
                + $"P97 {At(0.97):0.000000} max {At(1):0.000000}");
            _output.WriteLine(
                $"    P97/P25 = {At(0.97) / Math.Max(At(0.25), 1e-12):0.0} "
                + $"({20 * Math.Log10(At(0.97) / Math.Max(At(0.25), 1e-12)):0.0} dB)");
            _output.WriteLine(
                $"    hops where key-down scores better than key-up: {better} "
                + $"({100.0 * better / env.Length:0.0} %)");
        }
    }

    [Fact]
    public void DoTheTwoUnreadCapturesRead()
    {
        foreach (var name in new[]
        {
            "unadjudicated/cw-2026-08-22-014113",
            "unadjudicated/cw-2026-08-22-014308",
        })
        {
            var audio = Read(name);
            var truth = TruePitch(audio);

            _output.WriteLine($"=== {Path.GetFileName(name)}  true pitch {truth:0.00} Hz");

            foreach (var hz in new[] { truth, 606.0, 600.0 })
            {
                var r = CwProbabilisticDecoder.Decode(audio, hz);

                _output.WriteLine(
                    $"    @ {hz,7:0.00} Hz: ratio {r.LikelihoodRatio,8:0.00}, "
                    + $"{r.Characters.Count(c => c.Text != " "),3} chars, "
                    + $"{r.WordsPerMinute:0} wpm: "
                    + $"'{string.Concat(r.Characters.Select(c => c.Text))}'");
            }
        }
    }
}
