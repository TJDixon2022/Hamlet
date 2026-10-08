using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Scan;

/// <summary>
/// **A DAH THAT BREAKS IN TWO** (work instruction 546, task 1): the nine dahs of the main scan catch that an offline read
/// hears whole and the chain printed as two dits, traced through the detector's stood marks, the gate's printed marks and the
/// sender's own window, so the break is found where it happens.
/// </summary>
/// <remarks>
/// <para>R88 is lifted for the three scan catches in the tree; this reads the main one.</para>
/// <para>**THE PLAIN READ IS NOT GROUND TRUTH** (see <see cref="TheStrongStationsOverTests"/>). It says where a dah of about
/// 150 ms sits; the chain's marks around it are the measurement.</para>
/// </remarks>
public sealed class TheBrokenDahsTests
{
    /// <summary>Where the nine dahs start, in seconds, as the work instruction names them.</summary>
    internal static readonly double[] Times = [3.54, 31.62, 32.65, 40.69, 49.17, 52.14, 53.63, 69.54, 72.11];

    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the trace is printed.</param>
    public TheBrokenDahsTests(ITestOutputHelper output) => _output = output;

    /// <remarks>
    /// Task 1: at each of the nine times, the plain read's marks, every mark the detector stood within 30 Hz of the station,
    /// the marks the gate printed with their kind, and the sender's own window over the stretch, its level every hop and the
    /// deepest dip inside the plain read's dah against the dah's own top. Asserts only that the station is heard.
    /// </remarks>
    [Fact]
    public void WhereTheNineDahsBreak()
    {
        var audio = WavAudio.Read(TheStrongStationsOverTests.Wav(TheStrongStationsOverTests.Main));
        var chain = TheStrongStationsOverTests.ReadChain(audio.Samples, audio.SampleRate);
        var station = chain.PitchHz;
        var plain = TheStrongStationsOverTests.ReadPlain(audio.Samples, audio.SampleRate, station);
        var lane = Lane(audio, station, plain.Dit);
        var broken = 0;
        var whole = 0;

        _output.WriteLine(string.Create(Invariant, $"station {station:0} Hz; plain dit {plain.Dit * 1000:0} ms, dah {plain.Dah * 1000:0} ms"));

        foreach (var t in Times)
        {
            var dah = plain.Marks.Where(m => m.Kind == '-' && m.From > t - 0.08 && m.From < t + 0.08).OrderBy(m => Math.Abs(m.From - t)).FirstOrDefault();
            var from = (dah?.From ?? t) - 0.06;
            var to = (dah?.To ?? t + 0.16) + 0.06;
            var sb = new StringBuilder();

            sb.Append(string.Create(Invariant, $"{t:0.00} s: plain "));
            sb.Append(dah is null ? "no dah" : string.Create(Invariant, $"{dah.From:0.000}-{dah.To:0.000} ({(dah.To - dah.From) * 1000:0} ms)"));

            var stood = chain.Marks.Where(m => Math.Abs(m.PitchHz - station) <= 30 && m.ToSeconds > from && m.FromSeconds < to).OrderBy(m => m.FromSeconds).ToList();

            sb.Append("; stood ");
            sb.Append(string.Join(", ", stood.Select(m => string.Create(Invariant, $"{m.FromSeconds:0.000}-{m.ToSeconds:0.000} ({m.LengthMs:0} ms, {m.LevelDb:0.0} dB)"))));

            for (var i = 1; i < stood.Count; i++)
            {
                sb.Append(string.Create(Invariant, $" gap {(stood[i].FromSeconds - stood[i - 1].ToSeconds) * 1000:0} ms"));
            }

            var printed = chain.Printed.Where(m => m.To > from && m.From < to).OrderBy(m => m.From).ToList();

            sb.Append("; printed ");
            sb.Append(string.Join(", ", printed.Select(m => string.Create(Invariant, $"{(m.To - m.From) * 1000:0} ms '{m.Kind}' letter {m.Letter}"))));

            if (printed.Count > 0)
            {
                var p = printed[0];

                sb.Append(string.Create(Invariant, $"; sender dit {p.Dit * 1000:0} ms, split {p.Split * 1000:0} ms, letter line {p.CharacterLine * 1000:0} ms, level {p.LevelRef:0.0} dB"));
            }

            if (dah is not null)
            {
                var inside = lane.Where(l => l.At >= dah.From + 0.02 && l.At <= dah.To - 0.02).ToList();

                if (inside.Count > 0)
                {
                    var top = inside.Select(l => l.Db).OrderByDescending(d => d).ElementAt(inside.Count / 4);

                    sb.Append(string.Create(Invariant, $"; own window: top {top:0.0} dB, deepest {inside.Min(l => l.Db):0.0} dB at {inside.MinBy(l => l.Db).At:0.000} s, a dip of {top - inside.Min(l => l.Db):0.0} dB"));
                }
            }

            if (stood.Count(m => dah is not null && m.ToSeconds > dah.From && m.FromSeconds < dah.To) >= 2)
            {
                broken++;
            }
            else
            {
                whole++;
            }

            _output.WriteLine(sb.ToString());
        }

        _output.WriteLine($"stood in two or more pieces: {broken} of {Times.Length}; whole or missing: {whole}");

        Assert.NotEmpty(chain.Text);
    }

    /// <remarks>
    /// Work instruction 556, task 4: the detector's own window over the catch, its level each hop as a share of the station's
    /// contrast under the key-down level (key-up the window's own median between marks). At each of the nine dahs: the
    /// deepest share inside it, and whether it passes the 0.6 down line, the stood pieces beside. And the sender's own gaps on
    /// the same window, every fall under a third of the contrast lasting half a dit or more, with their depths' percentiles.
    /// Asserts only that the station is heard.
    /// </remarks>
    [Fact]
    public void TheDownCrossingAtEachDah()
    {
        var audio = WavAudio.Read(TheStrongStationsOverTests.Wav(TheStrongStationsOverTests.Main));
        var chain = TheStrongStationsOverTests.ReadChain(audio.Samples, audio.SampleRate);
        var plain = TheStrongStationsOverTests.ReadPlain(audio.Samples, audio.SampleRate, chain.PitchHz);

        using var own = new CwChain(audio.SampleRate);
        var trace = new List<(double Seconds, double LaneDb, double SenderDb, double DitSeconds, double DelaySeconds)>();
        var keyUp = new List<(double Seconds, double? KeyUpDb)>();

        own.Detector.LaneTrace = trace;
        own.Detector.LaneKeyUpTrace = keyUp;
        own.Detector.SetPassband(600, 500);

        for (var at = 0; at + (audio.SampleRate / 100) <= audio.Samples.Length; at += audio.SampleRate / 100)
        {
            own.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan(at, audio.SampleRate / 100)));
        }

        // Each hop: audio time, the share under the key-down level, and the dit.
        var share = trace.Zip(keyUp, (t, k) => (At: t.Seconds - t.DelaySeconds, Share: k.KeyUpDb is { } up && t.SenderDb - up > 0 ? (t.SenderDb - t.LaneDb) / (t.SenderDb - up) : double.NaN, t.DitSeconds)).ToList();

        foreach (var t in Times)
        {
            var dah = plain.Marks.Where(m => m.Kind == '-' && m.From > t - 0.08 && m.From < t + 0.08).OrderBy(m => Math.Abs(m.From - t)).FirstOrDefault();

            if (dah is null)
            {
                _output.WriteLine(string.Create(Invariant, $"{t:0.00} s: no dah in the plain read"));
                continue;
            }

            var inside = share.Where(s => s.At >= dah.From + 0.015 && s.At <= dah.To - 0.015 && double.IsFinite(s.Share)).ToList();
            var deepest = inside.Count > 0 ? inside.Max(s => s.Share) : double.NaN;
            var stood = chain.Marks.Where(m => Math.Abs(m.PitchHz - chain.PitchHz) <= 30 && m.ToSeconds > dah.From && m.FromSeconds < dah.To).ToList();
            var profile = string.Concat(share.Where(s => s.At >= dah.From - 0.02 && s.At <= dah.To + 0.02).Select(s => !double.IsFinite(s.Share) ? '?' : s.Share < 0.4 ? '#' : s.Share < 0.6 ? '+' : '.'));

            _output.WriteLine(string.Create(Invariant, $"dah {t:0.00} s ({(dah.To - dah.From) * 1000:0} ms): deepest share {deepest:0.00}, {(deepest >= 0.6 ? "passes" : "short of")} the 0.6 down line; window {profile}; stood {stood.Count} piece(s)"));
        }

        // The sender's own gaps on its window: falls past a third of the contrast lasting half a dit or more.
        var gaps = new List<double>();
        var run = new List<(double At, double Share, double Dit)>();

        foreach (var s in share.Append((At: double.PositiveInfinity, Share: 0.0, DitSeconds: 0.0)))
        {
            if (double.IsFinite(s.Share) && s.Share > 0.33 && (run.Count == 0 || s.At - run[^1].At < 0.006))
            {
                run.Add((s.At, s.Share, s.DitSeconds));
                continue;
            }

            if (run.Count > 0 && run[^1].At - run[0].At + 0.005 >= 0.5 * run[0].Dit)
            {
                gaps.Add(Math.Min(1, run.Max(r => r.Share)));
            }

            run.Clear();
        }

        var sorted = gaps.Order().ToList();

        if (sorted.Count > 0)
        {
            _output.WriteLine(string.Create(Invariant, $"the sender's own gaps: {sorted.Count}; depth p1 {sorted[sorted.Count / 100]:0.00}, p5 {sorted[sorted.Count / 20]:0.00}, p10 {sorted[sorted.Count / 10]:0.00}, median {sorted[sorted.Count / 2]:0.00}"));
        }

        Assert.NotEmpty(chain.Text);
    }

    /// <summary>The sender's own window as the detector builds it, tuned to the station and its dit: its level every 5 ms.</summary>
    private static List<(double At, double Db)> Lane(MonoAudio audio, double pitchHz, double ditSeconds)
    {
        var lane = new CwSenderLane(audio.SampleRate);
        var hop = audio.SampleRate / 200;
        var levels = new List<(double, double)>();

        lane.Tune(pitchHz, ditSeconds);

        for (var n = 0; n < audio.Samples.Length; n++)
        {
            lane.Push(audio.Samples[n]);

            if ((n + 1) % hop == 0)
            {
                levels.Add((((n + 1) / (double)audio.SampleRate) - lane.DelaySeconds, lane.LevelDb));
            }
        }

        return levels;
    }
}
