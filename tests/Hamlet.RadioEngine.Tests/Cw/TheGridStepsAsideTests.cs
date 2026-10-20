using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE GRID STEPS ASIDE FOR A LANE THAT READS A STATION WHOLE** (work instruction 578, HM-DEC-282). A bank lane that reads a
/// station at a shape of 0.4 or better is not handed on while the grid holds anything within 50 Hz, and on Ken's reply on
/// <c>cw-2026-10-19-141708</c> and on the five working receiver fixtures the grid holds pieces of that station. These trace it
/// second by second, and read the fixtures and Ken as shipped and with the grid stepping aside.
/// </summary>
public sealed class TheGridStepsAsideTests(ITestOutputHelper output)
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>The working receiver fixtures, the yardstick: each has a truth, all at 615 Hz, 5 dB, 12 dB of fading.</summary>
    internal static readonly string[] Working = ["coverage-working", "exchange-working", "fast-working", "prosigns-working", "tightfist-working"];

    /// <summary>A working fixture's path.</summary>
    internal static string Fixture(string name) => Path.Combine(CwFixtures.Folder, "receiver", name + ".wav");

    /// <summary>A working fixture's truth, its sheet's <c>text</c> line.</summary>
    internal static string Truth(string name)
        => System.Text.RegularExpressions.Regex.Replace(File.ReadAllLines(Path.ChangeExtension(Fixture(name), ".txt")).First(l => l.StartsWith("text", StringComparison.Ordinal))[4..].Trim(), @"\^([A-Z]{2})", "<$1>");

    /// <summary>The easy receiver fixtures, which must not read worse than 77 of 90 (the owner's ruling on HM-DEC-285; 78 before it).</summary>
    internal static readonly string[] Easy = ["coverage-easy", "exchange-easy", "fast-easy", "prosigns-easy", "tightfist-easy"];

    private static string Ms(double seconds) => double.IsFinite(seconds) ? (seconds * 1000).ToString("0", Inv) : "-";

    /// <summary>Per grid sequence: when it was first seen standing, when a sender at its pitch first qualified, and its median mark then.</summary>
    private sealed class Seen
    {
        public double PitchHz { get; set; }

        public double StoodAt { get; set; } = double.NaN;

        public double QualifiedAt { get; set; } = double.NaN;

        public double FirstMedian { get; set; } = double.NaN;
    }

    /// <summary>
    /// The trace of one stretch: every second where a lane within 50 Hz of the true pitch stands at shape 0.4 or better, what the
    /// grid holds near it and whether the lane is handed on; then the share of the station's keyed seconds a whole-reading lane
    /// was blocked, and every grid sequence that stood with how long it took to qualify.
    /// </summary>
    private (int Blocked, int Keyed, int Reading) Trace(string name, float[] samples, int rate, double pitch, double width, double truePitch, double from, double to, Func<double, bool> keyedAt)
    {
        using var chain = new CwChain(rate);
        var gate = chain.Decoder.Runs;
        var chunk = rate / 100;
        var seen = new Dictionary<int, Seen>();
        var (blocked, keyed, reading) = (0, 0, 0);
        var handover = new List<string>();
        var printedLetters = new List<string>();

        chain.Detector.SetPassband(pitch, width);
        chain.Detector.HandoverTrace = handover;
        gate.RunRead += (c, run) => printedLetters.Add(string.Create(Inv, $"{run[^1].ToSeconds:0.0}/{run.Average(m => m.PitchHz):0}/{c.Text}"));
        output.WriteLine(string.Create(Inv, $"## {name}, {from:0} to {to:0} s, true pitch {truePitch:0} Hz"));
        output.WriteLine("| s | lane: pitch/dit shape | grid within 50 Hz: kind pitch, median mark against the lane's dit, shape, qualified, stood unqualified | handed on |");
        output.WriteLine("|---|---|---|---|");

        for (var at = 0; at + chunk <= samples.Length && at < (to + 1) * rate; at += chunk)
        {
            chain.Process(new AudioChunk(at, rate, samples.AsSpan(at, chunk)));

            var now = (at + chunk) / (double)rate;

            if ((at / chunk) % 100 != 99)
            {
                continue;
            }

            // Every grid sequence standing anywhere, and whether a sender at its pitch has qualified.
            var senders = gate.SenderShapes;

            foreach (var g in chain.Detector.GridNear(truePitch, now).Concat(chain.Detector.GridNear(truePitch + 100, now)).Concat(chain.Detector.GridNear(truePitch - 100, now)).Where(g => g.Kind == "standing"))
            {
                if (!seen.TryGetValue(g.Id, out var s))
                {
                    seen[g.Id] = s = new Seen { PitchHz = g.PitchHz, StoodAt = now, FirstMedian = g.MedianSeconds };
                }

                if (double.IsNaN(s.QualifiedAt) && senders.Any(x => x.Qualified && Math.Abs(x.PitchHz - g.PitchHz) <= CwSenderGate.PitchToleranceHz))
                {
                    s.QualifiedAt = now;
                }
            }

            if (now < from || now > to)
            {
                continue;
            }

            var isKeyed = keyedAt(now);
            keyed += isKeyed ? 1 : 0;

            if (chain.Detector.Bank is not { } bank
                || bank.Standing(now).Where(l => Math.Abs(l.Sequence.PitchHz - truePitch) <= CwEnvelopeDetector.HandoverHz && l.Sequence.Shape.Score >= 0.4).OrderByDescending(l => l.Sequence.Shape.Score).FirstOrDefault() is not { Sequence: not null } lane)
            {
                continue;
            }

            var dit = CwLaneBank.DitSeconds[lane.Dit];
            var grid = chain.Detector.GridNear(lane.Sequence.PitchHz, now);
            var handed = Math.Abs(chain.Detector.WeakStationPitchHz - lane.Sequence.PitchHz) <= CwEnvelopeDetector.HandoverHz;
            var cells = grid.Select(g =>
            {
                var s = g.Kind == "standing" && seen.TryGetValue(g.Id, out var x) ? x : null;
                var unqualified = s is null ? "-" : double.IsNaN(s.QualifiedAt) ? $"never, {now - s.StoodAt:0} s" : $"after {s.QualifiedAt - s.StoodAt:0} s";

                return string.Create(Inv, $"{g.Kind} {g.PitchHz:0}, {Ms(g.MedianSeconds)} ms ({g.MedianSeconds / dit:0.00} dit), {g.Shape:0.00}, {unqualified}");
            });

            reading += isKeyed ? 1 : 0;
            blocked += isKeyed && !handed && grid.Count > 0 ? 1 : 0;

            output.WriteLine(string.Create(Inv, $"| {now:0} | {lane.Sequence.PitchHz:0}/{Ms(dit)} {lane.Sequence.Shape.Score:0.00} | {(grid.Count == 0 ? "nothing" : string.Join("; ", cells))} | {(handed ? "yes" : "no")} |"));
        }

        output.WriteLine(string.Create(Inv, $"**{name}:** a whole-reading lane in {reading} of the station's {keyed} keyed seconds; blocked by the grid in **{blocked}** ({(keyed == 0 ? 0 : blocked * 100.0 / keyed):0}% of its keying)"));
        output.WriteLine("handover: " + string.Join("; ", handover.Where(h => double.TryParse(h.Split(' ')[0], NumberStyles.Float, Inv, out var t) && t >= from - 5 && t <= to)));
        output.WriteLine("printed, s/Hz/letter: " + string.Join(" ", printedLetters));
        output.WriteLine("grid sequences that stood: " + string.Join("; ", seen.Values.OrderBy(s => s.StoodAt).Select(s => string.Create(Inv, $"{s.PitchHz:0} Hz at {s.StoodAt:0} s, median {Ms(s.FirstMedian)} ms, {(double.IsNaN(s.QualifiedAt) ? "never qualified" : $"qualified after {s.QualifiedAt - s.StoodAt:0} s")}"))));
        output.WriteLine(string.Empty);

        return (blocked, keyed, reading);
    }

    /// <remarks>Work instruction 578, section 2: the five working fixtures, second by second, as shipped. Asserts nothing.</remarks>
    [Fact]
    public void TheWorkingFixturesTraced()
    {
        foreach (var name in Working)
        {
            var audio = WavAudio.Read(Fixture(name));
            var map = TheKeyingMapTests.Read(name, audio.Samples, audio.SampleRate);

            Trace($"`{name}`", audio.Samples, audio.SampleRate, 600, 500, 615, 0, audio.Samples.Length / (double)audio.SampleRate, t => map.Keyed(615, t - 1, t));
        }
    }

    /// <remarks>
    /// Work instruction 578, section 2: Ken's reply, 272 to 300 s, and his first over, 80 to 100 s, as shipped, keyed where a lane
    /// opened by hand at his pitch finds a mark; and W1AW's piece 2 from 41 to 95 s, a grid sequence reading the station itself.
    /// Asserts nothing.
    /// </remarks>
    [Fact]
    public void KenAndW1awTraced()
    {
        var ken = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(TheFoundWeakStationTests.Ken));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(TheFoundWeakStationTests.Ken);

        foreach (var (from, to) in new[] { (80.0, 100.0), (272.0, 300.0) })
        {
            var (hand, _, hz) = TheFoundWeakStationTests.ByHand(ken.Samples, ken.SampleRate, from, to, 430, CwLaneBank.DitSeconds[0]);

            Trace($"Ken, by hand at {hz:0} Hz", ken.Samples, ken.SampleRate, pitch, width, 430, from, to, t => hand.Any(m => m.To > t - 1 && m.From < t));
        }

        var w1aw = WavAudio.Read(TheOwnersRecordingReadsTests.Wav("w1aw-2026-10-07/piece-02"));
        var w1awMap = TheKeyingMapTests.Read("w1aw-2026-10-07/piece-02", w1aw.Samples, w1aw.SampleRate);

        Trace("W1AW, piece 2", w1aw.Samples, w1aw.SampleRate, 600, 500, 600, 41, 95, t => w1awMap.Keyed(600, t - 1, t));
    }

    /// <remarks>
    /// Work instruction 578, section 4: the five working fixtures and Ken's two overs read with the grid stepping aside and without
    /// (<see cref="CwRules.GridStepsAside"/>). Asserts nothing.
    /// </remarks>
    /// <param name="aside">Whether the grid steps aside.</param>
    [Theory]
    /// <param name="corrected">Whether the hold also waits for the station and the lane that fits is handed on (work instruction 579).</param>
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void TheWorkingFixturesAndKenRead(bool aside, bool corrected)
    {
        using var rule = corrected ? CwRules.Choose(CwRules.GridStepsAside, CwRules.HoldWaitsAndFits) : aside ? CwRules.Choose(CwRules.GridStepsAside) : CwRules.Off(CwRules.GridStepsAside);
        var total = 0;
        var of = 0;

        output.WriteLine(corrected ? "the grid steps aside, the hold waits and the lane that fits is handed on" : aside ? "the grid steps aside" : "the grid does not step aside");

        foreach (var name in Working.Concat(Easy))
        {
            if (name == Easy[0])
            {
                output.WriteLine($"working fixtures: {total} of {of}");
                (total, of) = (0, 0);
            }

            var audio = WavAudio.Read(Fixture(name));
            var read = Read(audio.Samples, audio.SampleRate, 600, 500);
            var truth = Truth(name);
            var right = TheRecordingsScoreboardTests.Right(read, truth);
            var letters = TheRecordingsScoreboardTests.Letters(truth).Length;

            total += right;
            of += letters;
            output.WriteLine($"`{name}`: `{read}` against `{truth}`, {right} of {letters}");
        }

        output.WriteLine($"easy fixtures: {total} of {of}");

        var ken = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(TheFoundWeakStationTests.Ken));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(TheFoundWeakStationTests.Ken);
        var (letters2, _) = TheRecordingsScoreboardTests.ReadLive(ken.Samples, ken.SampleRate, pitch, width);

        foreach (var (from, to) in new[] { (80.0, 100.0), (272.0, 300.0) })
        {
            var mine = letters2.Where(l => l.Seconds >= from && l.Seconds <= to + 1).ToList();
            var near = TheRecordingsScoreboardTests.Text(mine.Where(l => Math.Abs(l.PitchHz - 430) <= 45));
            var words = near.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            output.WriteLine(string.Create(Inv, $"Ken, {from:0} to {to:0} s: all `{TheRecordingsScoreboardTests.Text(mine)}`; at his pitch `{near}`; words of three letters or more {words.Count(w => w.Length >= 3)}; lone one- and two-element words {words.Count(w => w is "E" or "T" or "I" or "A" or "N" or "M") * 10 / (to - from):0.0} per 10 s"));
        }
    }

    /// <summary>One working fixture or Ken's over read through the chain, the letters with their pitch and spaces.</summary>
    internal static string Read(float[] samples, int rate, double pitch, double width, double from = 0, double to = double.PositiveInfinity, double nearHz = double.NaN)
    {
        var (letters, _) = TheRecordingsScoreboardTests.ReadLive(samples, rate, pitch, width);
        var mine = letters.Where(l => l.Seconds >= from && l.Seconds <= to + 1 && (double.IsNaN(nearHz) || Math.Abs(l.PitchHz - nearHz) <= 45)).ToList();

        return TheRecordingsScoreboardTests.Text(mine);
    }
}
