using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Scan;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE WINDOW'S OWN VIEW** (work instruction 553, task 1): on a standing sender's own window, its key-down and key-up
/// levels, and the depth of every fall under the key-down level, as a share of the sender's contrast. R88 is lifted for the
/// owner's twelve recordings, the three scan catches and W1AW's `cw-2026-10-06-212015`.
/// </summary>
/// <remarks>
/// <para>**AN EXCURSION** is a stretch of hops where the window sits more than 3 dB under the sender's key-down level. One
/// shorter than half the sender's dit is a **dip inside a mark**, since Morse's shortest gap is a whole dit; one of half a dit
/// or longer is a **gap**. The key-up level is the median floor of the gaps, and the contrast is the key-down level less it.</para>
/// <para>Asserts nothing; the distributions are the result.</para>
/// </remarks>
public sealed class TheWindowsOwnViewTests(ITestOutputHelper output)
{
    /// <summary>One fall under the key-down level.</summary>
    internal sealed record Excursion(string Source, double Seconds, double Ms, double Share, bool Dip);

    /// <summary>The window's trace of one recording, read through the app's chain.</summary>
    internal static List<(double Seconds, double LaneDb, double SenderDb, double DitSeconds, double DelaySeconds)> Trace(float[] samples, int rate, double pitchHz, double widthHz)
    {
        using var chain = new CwChain(rate);
        var trace = new List<(double, double, double, double, double)>();

        chain.Detector.LaneTrace = trace;
        chain.Detector.SetPassband(pitchHz, widthHz);

        for (var at = 0; at + (rate / 100) <= samples.Length; at += rate / 100)
        {
            chain.Process(new AudioChunk(at, rate, samples.AsSpan(at, rate / 100)));
        }

        chain.Decoder.Flush();

        return trace;
    }

    /// <summary>Every excursion in a trace, its depth a share of the source's contrast.</summary>
    internal static List<Excursion> Excursions(string source, List<(double Seconds, double LaneDb, double SenderDb, double DitSeconds, double DelaySeconds)> trace)
    {
        var raw = new List<(double Seconds, double Ms, double Floor, double Down, double Dit)>();
        var at = -1;

        for (var i = 0; i <= trace.Count; i++)
        {
            var under = i < trace.Count && double.IsFinite(trace[i].SenderDb) && trace[i].LaneDb < trace[i].SenderDb - 3
                && (i == 0 || trace[i].Seconds - trace[i - 1].Seconds < 0.006);

            if (under && at < 0)
            {
                at = i;
            }
            else if (!under && at >= 0)
            {
                var span = trace.Skip(at).Take(i - at).ToList();

                // Only a fall with the key down on both sides of it: one that runs into the window's edge is not measured.
                if (at > 0 && i < trace.Count)
                {
                    raw.Add((span[0].Seconds, span.Count * 5.0, span.Min(s => s.LaneDb), span.Average(s => s.SenderDb), span[0].DitSeconds));
                }

                at = -1;
            }
        }

        var gaps = raw.Where(r => r.Ms >= 500 * r.Dit).Select(r => r.Floor).Order().ToList();

        if (gaps.Count == 0)
        {
            return [];
        }

        var up = gaps[gaps.Count / 2];

        return raw.Select(r => new Excursion(source, r.Seconds, r.Ms, (r.Down - r.Floor) / (r.Down - up), r.Ms < 500 * r.Dit)).ToList();
    }

    internal static string Cell(IReadOnlyList<double> values)
    {
        if (values.Count == 0)
        {
            return "none";
        }

        var s = values.Order().ToList();

        double P(double q) => s[(int)Math.Min(s.Count - 1, Math.Floor(q * s.Count))];

        return FormattableString.Invariant($"{s.Count}: min {s[0]:0.00}, p1 {P(0.01):0.00}, p5 {P(0.05):0.00}, median {P(0.5):0.00}, p95 {P(0.95):0.00}, p99 {P(0.99):0.00}, max {s[^1]:0.00}");
    }

    /// <summary>The sources, by class: W1AW, the strong catch, the owner's scored stretches, and the synthetic 25 WPM calls.</summary>
    internal static IEnumerable<(string Class, string Source, Func<(float[] Samples, int Rate, double Pitch, double Width)> Audio)> Sources()
    {
        var w1aw = AStationsShadowIsNotAStationTests.W1aw;

        yield return ("W1AW", w1aw, () => Recording(w1aw));
        yield return ("the strong catch", TheStrongStationsOverTests.Main, () =>
        {
            var a = WavAudio.Read(TheStrongStationsOverTests.Wav(TheStrongStationsOverTests.Main));

            return (a.Samples, a.SampleRate, 600, 500);
        });

        foreach (var name in TheRecordingsScoreboardTests.Stretches
            .Where(s => s.Confidence >= TheRecordingsScoreboardTests.Confidence.Low && s.Recording != w1aw)
            .Select(s => s.Recording).Distinct())
        {
            yield return ("the owner's stretches", name, () => Recording(name));
        }

        foreach (var db in new[] { 8, 10, 12, 24 })
        {
            yield return ("25 WPM synthetic", $"25 WPM at {db} dB", () => (Weak(db), 8000, 625, 500));
        }
    }

    /// <summary>A 25 WPM call keyed clean at a strength, with the bench's 1 dB AGC overshoot, through the filter.</summary>
    internal static float[] Weak(int db)
        => NarrownessReadsTheFiltersBandTests.ThroughTheFilter(AHandIsReadAgainstItselfTests.Keyed(
            WeakText, _ => new AHandIsReadAgainstItselfTests.Sending(25, 0), 5530 + db, db, overshootDb: 1));

    /// <summary>The weak call's text: forty dahs in it, counted below.</summary>
    internal const string WeakText = "CQ CQ CQ DE K1ABC K1ABC K1ABC K TEST DE W1XYZ W1XYZ K";

    /// <remarks>
    /// **THE WEAK TABLE** (work instruction 553, task 2, a bench case reported, not a hard limit): the 25 WPM call through the
    /// filter with the bench's AGC at 8, 10, 12 and 24 dB, read through the app's chain: the dahs in the letters printed against
    /// the call's own, and the text.
    /// </remarks>
    [Fact]
    public void TheWeakTable()
    {
        var dahsSent = WeakText.Where(c => c != ' ').Sum(c => Dahs(c.ToString()));

        foreach (var db in new[] { 8, 10, 12, 24 })
        {
            var read = TheRecordingsScoreboardTests.ReadLive(Weak(db), 8000, 625, 500);
            var kept = read.Letters.Sum(l => Dahs(l.Text));

            output.WriteLine(FormattableString.Invariant($"weak | {db} dB | dahs {kept} of {dahsSent} | `{read.Text}`"));
        }

        Assert.True(dahsSent > 0);
    }

    /// <remarks>
    /// A catch read with marks by level off and on: every mark the detector stood near the printed pitch, its length, and the
    /// window's key-down level and the trace's level span over it. Asserts nothing.
    /// </remarks>
    /// <param name="name">The catch.</param>
    [Theory]
    [InlineData("catch-154819-7050903")]
    public void ACatchByLevelOffAndOn(string name)
    {
        var a = WavAudio.Read(TheStrongStationsOverTests.Wav(name));

        foreach (var byLevel in new[] { false, true })
        {
            using var chain = new CwChain(a.SampleRate);
            var trace = new List<(double Seconds, double LaneDb, double SenderDb, double DitSeconds, double DelaySeconds)>();
            var text = new System.Text.StringBuilder();

            chain.Detector.LaneByLevel = byLevel;
            chain.Detector.LaneTrace = trace;
            chain.Detector.SetPassband(600, 500);
            chain.Decoder.Runs.CharacterRead += c => text.Append(c.Text);

            for (var at = 0; at + (a.SampleRate / 100) <= a.Samples.Length; at += a.SampleRate / 100)
            {
                chain.Process(new AudioChunk(at, a.SampleRate, a.Samples.AsSpan(at, a.SampleRate / 100)));
            }

            chain.Decoder.Flush();

            var marks = chain.Detector.MarksSince(0).Marks.OrderBy(m => m.FromSeconds).ToList();
            var open = trace.Count > 0 ? FormattableString.Invariant($"{trace[0].Seconds:0.00} to {trace[^1].Seconds:0.00} s") : "never";

            output.WriteLine($"by level {byLevel}: window open {open}; {marks.Count} marks stood; `{text}`");

            foreach (var m in marks.Where(m => m.FromSeconds > 5).Take(30))
            {
                var over = trace.Where(t => t.Seconds >= m.FromSeconds && t.Seconds <= m.ToSeconds).ToList();

                output.WriteLine(FormattableString.Invariant(
                    $"  {m.FromSeconds:0.000} {m.LengthMs:0} ms at {m.PitchHz:0} Hz, {m.LevelDb:0.0} dB; window {(over.Count > 0 ? $"{over.Min(t => t.LaneDb):0.0} to {over.Max(t => t.LaneDb):0.0}, key-down {over[0].SenderDb:0.0}" : "closed")}"));
            }
        }
    }

    private static int Dahs(string letter)
        => MorseAlphabet.All.FirstOrDefault(p => p.Value == letter).Key?.Count(c => c == '-') ?? 0;

    private static (float[] Samples, int Rate, double Pitch, double Width) Recording(string name)
    {
        var a = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(name));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(name);

        return (a.Samples, a.SampleRate, pitch, width);
    }

    [Fact]
    public void TheLevelsAndTheFallsAsShareOfContrast()
    {
        var all = new List<(string Class, Excursion E, double DownDb, double UpDb)>();
        var jobs = Sources().ToList();
        var results = new (string Class, string Source, List<Excursion> E, double Contrast, double Down)[jobs.Count];

        Parallel.For(0, jobs.Count, i =>
        {
            var (c, source, audio) = jobs[i];
            var (samples, rate, pitch, width) = audio();
            var trace = Trace(samples, rate, pitch, width);
            var e = Excursions(source, trace);
            var down = trace.Where(t => double.IsFinite(t.SenderDb)).Select(t => t.SenderDb).DefaultIfEmpty(double.NaN).Average();

            results[i] = (c, source, e, double.NaN, down);
        });

        foreach (var r in results)
        {
            var gaps = r.E.Where(e => !e.Dip).ToList();
            var dips = r.E.Where(e => e.Dip).ToList();

            output.WriteLine(FormattableString.Invariant(
                $"{r.Class} | {r.Source} | key-down {r.Down:0.0} dB | gaps {Cell(gaps.Select(g => g.Share).ToList())} | dips {Cell(dips.Select(d => d.Share).ToList())}"));
        }

        foreach (var c in results.Select(r => r.Class).Distinct())
        {
            var e = results.Where(r => r.Class == c).SelectMany(r => r.E).ToList();

            output.WriteLine($"CLASS {c} | gaps {Cell(e.Where(x => !x.Dip).Select(x => x.Share).ToList())} | dips {Cell(e.Where(x => x.Dip).Select(x => x.Share).ToList())}");
        }

        var every = results.SelectMany(r => r.E).ToList();

        output.WriteLine($"ALL | gaps {Cell(every.Where(x => !x.Dip).Select(x => x.Share).ToList())} | dips {Cell(every.Where(x => x.Dip).Select(x => x.Share).ToList())}");
        output.WriteLine($"ALL | dip lengths ms {Cell(every.Where(x => x.Dip).Select(x => x.Ms).ToList())} | gap lengths ms {Cell(every.Where(x => !x.Dip).Select(x => x.Ms).ToList())}");

        Assert.NotEmpty(every);
    }
}
