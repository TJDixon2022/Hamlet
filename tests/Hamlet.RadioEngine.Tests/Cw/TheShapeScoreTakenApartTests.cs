using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Scan;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE SHAPE SCORE TAKEN APART** (work instruction 550, task 1): every term of the sequence shape score, measured on four
/// classes - perfect keying, real hands, junk, and synthetic clean calls - median and range, sampled once a second of
/// audio through the app's own chain.
/// </summary>
/// <remarks>
/// R88 is lifted for the owner's twelve recordings, the three scan catches in the tree and W1AW's
/// <c>cw-2026-10-06-212015</c>, and no other. Asserts nothing: the table is the result.
/// </remarks>
public sealed class TheShapeScoreTakenApartTests(ITestOutputHelper output)
{
    internal const string W1aw = "cw-2026-10-06-212015";

    /// <summary>One sample of one sender or sequence: its shape, and which class and source it is from.</summary>
    internal sealed record Sample(string Class, string Source, double Seconds, double PitchHz, CwSequenceShape Shape, bool Printed, bool Detector);

    /// <summary>The four classes, every sample of each.</summary>
    internal static IReadOnlyList<Sample> Measure()
    {
        var jobs = new List<Func<IEnumerable<Sample>>>();

        // 1. Perfect keying: W1AW's printed sender, over the whole recording.
        // 3. Junk, in part: every other sender and standing sequence on W1AW's recording.
        jobs.Add(() =>
        {
            var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(W1aw));
            var (pitch, width) = TheRecordingsScoreboardTests.RadioState(W1aw);

            return Run(audio.Samples, audio.SampleRate, pitch, width, W1aw).SelectMany(s =>
                s.Printed && !s.Detector ? [s with { Class = "perfect keying" }]
                : !s.Detector && Math.Abs(s.PitchHz - 600) > 30 ? [s with { Class = "junk" }]
                : s.Detector && Math.Abs(s.PitchHz - 600) > 30 ? [s with { Class = "junk" }]
                : Array.Empty<Sample>());
        });

        // 2. Real hands: the printed sender of every scoreboard stretch, inside the stretch, near its pitch.
        foreach (var group in TheRecordingsScoreboardTests.Stretches
            .Where(s => s.Confidence != TheRecordingsScoreboardTests.Confidence.None && s.Recording != W1aw)
            .GroupBy(s => s.Recording))
        {
            jobs.Add(() =>
            {
                var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(group.Key));
                var (pitch, width) = TheRecordingsScoreboardTests.RadioState(group.Key);

                return Run(audio.Samples, audio.SampleRate, pitch, width, group.Key)
                    .Where(s => s.Printed && !s.Detector && group.Any(g => s.Seconds >= g.From && s.Seconds <= g.To && Math.Abs(s.PitchHz - g.PitchHz) <= 30))
                    .Select(s => s with { Class = "real hands" });
            });
        }

        // 2. Real hands: both station catches, their printed sender. 3. Junk: the carrier catch, every sender and sequence.
        foreach (var (name, kind) in new[] { (TheStrongStationsOverTests.Main, "real hands"), (TheStrongStationsOverTests.Second, "real hands"), (TheStrongStationsOverTests.Carrier, "junk") })
        {
            jobs.Add(() =>
            {
                var audio = WavAudio.Read(TheStrongStationsOverTests.Wav(name));

                return Run(audio.Samples, audio.SampleRate, 600, 500, name)
                    .Where(s => kind == "junk" || (s.Printed && !s.Detector))
                    .Select(s => s with { Class = kind });
            });
        }

        // 3. Junk: loud noise, 30 s and three minutes, and the random carrier at its twenty seeds; every sender and sequence.
        foreach (var (seconds, seed) in new[] { (30, 5190 + 30), (180, 5190 + 180), (30, 5100 + 30), (180, 5100 + 180) })
        {
            jobs.Add(() => Run(
                CwSignal.Generate(new CwSignalRequest(" ", SampleRate: 8000, Amplitude: 0, NoiseAmplitude: 0.3, LeadInSeconds: seconds / 2.0, TailSeconds: seconds / 2.0, Seed: seed)).Samples,
                8000, 600, 500, $"noise {seconds} s seed {seed}").Select(s => s with { Class = "junk" }));
        }

        foreach (var seed in Enumerable.Range(5193, 20))
        {
            jobs.Add(() =>
            {
                var carrier = TheShapePicksTheSenderTests.Noise(25, 5192);

                TheShapePicksTheSenderTests.Key(carrier, TheShapePicksTheSenderTests.RandomKeying(2.5, 23, seed), 625, 24);

                return Run(carrier, 8000, 600, 500, $"carrier seed {seed}").Select(s => s with { Class = "junk" });
            });
        }

        // 4. Synthetic clean calls, through the filter with the bench's AGC, at 24 and 12 dB.
        foreach (var wpm in new[] { 15, 20, 25, 35 })
        {
            foreach (var condition in new[] { "agc-filter", "weak-agc-filter" })
            {
                jobs.Add(() =>
                {
                    var samples = AHandIsReadAgainstItselfTests.Under(
                        condition, "CQ CQ CQ DE K1ABC K1ABC K1ABC K TEST DE W1XYZ W1XYZ K", _ => new AHandIsReadAgainstItselfTests.Sending(wpm, 0), 5500 + wpm);

                    return Run(samples, 8000, 625, 500, $"{wpm} WPM {condition}")
                        .Where(s => s.Printed && !s.Detector)
                        .Select(s => s with { Class = "synthetic clean" });
                });
            }
        }

        var all = new ConcurrentBag<(int, Sample[])>();

        Parallel.For(0, jobs.Count, i => all.Add((i, jobs[i]().ToArray())));

        return all.OrderBy(a => a.Item1).SelectMany(a => a.Item2).ToList();
    }

    /// <summary>Reads audio through the app's chain and samples every sender and standing sequence once a second of audio.</summary>
    internal static List<Sample> Run(float[] samples, int rate, double pitchHz, double widthHz, string source)
    {
        using var chain = new CwChain(rate);
        var gate = chain.Decoder.Runs;
        var chunk = rate / 100;
        var list = new List<Sample>();

        chain.Detector.SetPassband(pitchHz, widthHz);

        for (var at = 0; at + chunk <= samples.Length; at += chunk)
        {
            chain.Process(new AudioChunk(at, rate, samples.AsSpan(at, chunk)));

            if ((at + chunk) % rate == 0)
            {
                var t = (at + chunk) / (double)rate;

                list.AddRange(gate.SenderShapes.Select(s => new Sample("", source, t, s.PitchHz, s.Shape, s.Printed, false)));
                list.AddRange(chain.Detector.StandingNow.Select(s => new Sample("", source, t, s.PitchHz, s.Shape, false, true)));
            }
        }

        return list;
    }

    internal static readonly string[] Classes = ["perfect keying", "real hands", "synthetic clean", "junk"];

    internal static readonly (string Name, Func<CwSequenceShape, double> Of)[] Terms =
    [
        ("rectangle", s => s.Rectangle),
        ("dits", s => s.Dits),
        ("dahs", s => s.Dahs),
        ("separation", s => s.Separation),
        ("consistency", s => s.Consistency),
        ("evidence", s => s.Evidence),
        ("ratio dah:dit", s => s.Ratio),
        ("inside-gap tightness", s => s.InsideGaps),
        ("inside gap over dit", s => s.InsideGapRatio),
        ("gaps in kinds", s => s.GapsInKinds),
        ("level spread dB", s => s.LevelSpreadDb),
        ("level step dB", s => s.LevelStepDb),
        ("contrast dB", s => s.ContrastDb),
        ("score", s => s.Score),
        ("candidate", s => s.Ranked),
    ];

    /// <summary>Median and range of a list, as the table writes them.</summary>
    internal static string Cell(IReadOnlyList<double> values)
    {
        if (values.Count == 0)
        {
            return "none";
        }

        var sorted = values.Where(double.IsFinite).Order().ToList();

        if (sorted.Count == 0)
        {
            return "none";
        }

        return string.Create(CultureInfo.InvariantCulture, $"{sorted[sorted.Count / 2]:0.00} ({sorted[0]:0.00}-{sorted[^1]:0.00})");
    }

    /// <summary>The term-by-class table: rows are terms, columns classes, the gate's senders and the detector's sequences apart.</summary>
    internal static string Table(IReadOnlyList<Sample> samples, IReadOnlyList<(string Name, Func<CwSequenceShape, double> Of)> terms)
    {
        var sb = new StringBuilder();

        foreach (var detector in new[] { false, true })
        {
            var columns = Classes.Select(c => samples.Where(s => s.Class == c && s.Detector == detector).ToList()).ToList();

            sb.AppendLine(detector ? "**The detector's standing sequences**" : "**The gate's senders**");
            sb.AppendLine();
            sb.AppendLine("| term | " + string.Join(" | ", Classes.Select((c, i) => $"{c} ({columns[i].Count} samples, {columns[i].Select(s => s.Source).Distinct().Count()} sources)")) + " |");
            sb.AppendLine("|---|" + string.Concat(Classes.Select(_ => "---|")));

            foreach (var (name, of) in terms)
            {
                sb.AppendLine($"| {name} | " + string.Join(" | ", columns.Select(c => Cell(c.Select(s => of(s.Shape)).ToList()))) + " |");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    [Fact]
    public void EveryTermOnEveryClass()
    {
        var samples = Measure();

        output.WriteLine(Table(samples, Terms));

        // Per source, the score's median, for the report's naming of what pulls W1AW down and lifts junk up.
        foreach (var g in samples.Where(s => !s.Detector).GroupBy(s => (s.Class, s.Source)).OrderBy(g => g.Key.Class))
        {
            output.WriteLine($"{g.Key.Class} | {g.Key.Source} | {g.Count()} | " + string.Join(" | ", Terms.Select(t => Cell(g.Select(s => t.Of(s.Shape)).ToList()))));
        }

        Assert.NotEmpty(samples);
    }
}
