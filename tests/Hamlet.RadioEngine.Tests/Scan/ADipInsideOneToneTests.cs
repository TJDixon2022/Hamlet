using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Scan;

/// <summary>
/// **A DIP INSIDE ONE TONE IS NOT A GAP** (work instruction 547, task 4): on the sender's own window, how far the nine dips
/// inside the main catch's dahs fall, and how far every real gap inside a letter on the owner's fists falls, each as a share
/// of that sender's own contrast down towards its key-up level.
/// </summary>
/// <remarks>
/// <para>R88 is lifted for the owner's twelve recordings and the three scan catches in the tree.</para>
/// <para>**THE WINDOW** is the detector's own <see cref="CwSenderLane"/>, tuned to the stretch's pitch and the plain read's
/// dit, its level every 5 ms and its delay taken off. **THE SENDER'S KEY-UP LEVEL** is the median of its level at the middle
/// of every gap the plain read finds, and **ITS TOP** the median at the middle of every mark, so its contrast is the one
/// minus the other. A gap's or a dip's **DEPTH** is how far its lowest point falls below the top, over that contrast: one
/// is all the way to key-up, nought is no dip at all.</para>
/// <para>**THE PLAIN READ IS NOT GROUND TRUTH** (<see cref="TheStrongStationsOverTests"/>): it says where the marks and the
/// gaps inside letters sit.</para>
/// </remarks>
public sealed class ADipInsideOneToneTests(ITestOutputHelper output)
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    /// <summary>A stretch's window: its level every hop, its key-up level and its top, in dB.</summary>
    private sealed record Window(List<(double At, double Db)> Levels, double KeyUp, double Top)
    {
        public double Contrast => Top - KeyUp;

        public double Lowest(double from, double to)
            => Levels.Where(l => l.At >= from && l.At <= to).Select(l => l.Db).DefaultIfEmpty(double.NaN).Min();

        public double Depth(double from, double to) => (Top - Lowest(from, to)) / Contrast;
    }

    private static Window WindowOf(float[] x, int rate, double pitchHz, double ditSeconds, IReadOnlyList<TheStrongStationsOverTests.PlainMark> marks)
    {
        var lane = new CwSenderLane(rate);
        var hop = rate / 200;
        var levels = new List<(double, double)>();

        lane.Tune(pitchHz, ditSeconds);

        for (var n = 0; n < x.Length; n++)
        {
            lane.Push(x[n]);

            if ((n + 1) % hop == 0)
            {
                levels.Add((((n + 1) / (double)rate) - lane.DelaySeconds, lane.LevelDb));
            }
        }

        double At(double t) => levels.MinBy(l => Math.Abs(l.Item1 - t)).Item2;

        var tops = marks.Select(m => At((m.From + m.To) / 2)).Order().ToList();
        var ups = marks.Skip(1).Select((m, i) => At((m.From + marks[i].To) / 2)).Order().ToList();

        return new Window(levels, ups[ups.Count / 2], tops[tops.Count / 2]);
    }

    /// <remarks>
    /// Task 4: the depth of each of the nine dips in the main catch and of every gap inside a letter in the plain read of each
    /// of the owner's stretches with a reference; both distributions printed, and how many of each fall under each share.
    /// Asserts only that both were measured.
    /// </remarks>
    [Fact]
    public void TheDipsAndTheGapsAgainstKeyUp()
    {
        var dips = new List<double>();
        var gaps = new List<double>();

        // The nine dahs of the main catch.
        {
            var audio = WavAudio.Read(TheStrongStationsOverTests.Wav(TheStrongStationsOverTests.Main));
            var plain = TheStrongStationsOverTests.ReadPlain(audio.Samples, audio.SampleRate, 860);
            var w = WindowOf(audio.Samples, audio.SampleRate, 860, plain.Dit, plain.Marks);

            output.WriteLine(string.Create(Invariant, $"main catch: key-up {w.KeyUp:0.0} dB, top {w.Top:0.0} dB, contrast {w.Contrast:0.0} dB"));

            foreach (var t in TheBrokenDahsTests.Times)
            {
                var dah = plain.Marks.Where(m => m.Kind == '-' && Math.Abs(m.From - t) < 0.08).MinBy(m => Math.Abs(m.From - t));

                if (dah is null)
                {
                    continue;
                }

                var depth = w.Depth(dah.From + 0.02, dah.To - 0.02);

                dips.Add(depth);
                output.WriteLine(string.Create(Invariant, $"  dip in the dah at {t:0.00} s: lowest {w.Lowest(dah.From + 0.02, dah.To - 0.02):0.0} dB, depth {depth:0.00} of the contrast"));
            }

            // And the catch's own real gaps inside letters, for the same sender.
            var own = InsideGaps(plain.Marks).Select(g => w.Depth(g.From, g.To)).ToList();

            output.WriteLine($"  the catch's own gaps inside letters: {own.Count}, depth {Summary(own)}");
            gaps.AddRange(own);
        }

        foreach (var s in Cw.TheRecordingsScoreboardTests.Stretches.Where(s => s.Confidence != Cw.TheRecordingsScoreboardTests.Confidence.None))
        {
            var audio = WavAudio.Read(Cw.TheOwnersRecordingReadsTests.Wav(s.Recording));
            var plain = TheStrongStationsOverTests.ReadPlain(audio.Samples, audio.SampleRate, s.PitchHz, s.From, s.To);

            if (plain.Marks.Count < 10)
            {
                continue;
            }

            var w = WindowOf(audio.Samples, audio.SampleRate, s.PitchHz, plain.Dit, plain.Marks);
            var these = InsideGaps(plain.Marks).Select(g => w.Depth(g.From, g.To)).Where(double.IsFinite).ToList();

            gaps.AddRange(these);
            output.WriteLine(string.Create(Invariant, $"{s.Recording} at {s.PitchHz:0} Hz: contrast {w.Contrast:0.0} dB, {these.Count} gaps inside letters, depth {Summary(these)}"));
        }

        output.WriteLine($"the nine dips: {Summary(dips)}; values {string.Join(" ", dips.Select(d => d.ToString("0.00", Invariant)))}");
        output.WriteLine($"every real gap inside a letter: {Summary(gaps)}");

        foreach (var share in new[] { 0.3, 0.4, 0.5, 0.6, 0.7, 0.8 })
        {
            output.WriteLine(string.Create(Invariant, $"  under {share:0.0} of the contrast: dips {dips.Count(d => d < share)} of {dips.Count}, real gaps {gaps.Count(g => g < share)} of {gaps.Count}"));
        }

        Assert.NotEmpty(dips);
        Assert.NotEmpty(gaps);
    }

    private static IEnumerable<(double From, double To)> InsideGaps(IReadOnlyList<TheStrongStationsOverTests.PlainMark> marks)
        => marks.Skip(1).Select((m, i) => (Prev: marks[i], Next: m)).Where(p => p.Next.Letter == p.Prev.Letter).Select(p => (p.Prev.To, p.Next.From));

    private static string Summary(IReadOnlyCollection<double> values)
    {
        if (values.Count == 0)
        {
            return "none";
        }

        var sorted = values.Order().ToList();

        return string.Create(Invariant, $"min {sorted[0]:0.00}, 5th percentile {sorted[(int)(0.05 * (sorted.Count - 1))]:0.00}, median {sorted[sorted.Count / 2]:0.00}, max {sorted[^1]:0.00}");
    }
}
