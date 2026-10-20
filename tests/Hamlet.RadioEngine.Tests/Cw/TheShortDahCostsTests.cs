using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE SHORT DAH'S THREE COSTS, TRACED** (work instruction 581, HM-DEC-285). Two kinds from stable clusters under a jump of
/// two (<see cref="CwRules.ShortDah"/>) reads the short-dah fists of the bank, and cost a stretch of the twelve-plus-three, a
/// 5 dB CQ and a carrier keyed at random. Each is read on and off, and every judgement the rule took is caught at the moment it
/// is made through <see cref="CwSenderGate.ShortDahJudged"/>. Asserts nothing.
/// </summary>
public sealed class TheShortDahCostsTests(ITestOutputHelper output)
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>One letter printed: when it ended, its pitch, its text, and its marks' lengths in ms.</summary>
    internal sealed record Letter(double At, double PitchHz, string Text, string Marks);

    /// <summary>One judgement the rule took, with the time of the chunk it was made in.</summary>
    internal sealed record Judged(double At, CwSenderGate.ShortDahJudgement J)
    {
        public double Ratio => J.DahSeconds / J.DitSeconds;

        public double Spreads => Math.Log(Ratio) / Math.Max(J.DitSd, J.DahSd);

        public double Widest => Math.Max(J.DitSd, J.DahSd);

        public string Row => string.Create(
            Inv,
            $"{At:0.00} s {J.Caller} at {J.PitchHz:0} Hz: {J.Sorted.Count} marks, {J.At} dits centre {J.DitSeconds * 1000:0} ms, {J.Sorted.Count - J.At} dahs centre {J.DahSeconds * 1000:0} ms, dah over dit {Ratio:0.00}, {Spreads:0.0} spreads apart, widths {J.DitSd:0.000} and {J.DahSd:0.000}{(J.Taken ? string.Empty : ", refused")}; sorted {string.Join(" ", J.Sorted.Select(l => (l * 1000).ToString("0", Inv)))}");
    }

    /// <summary>Reads audio through the app's chain, catching every judgement and every letter.</summary>
    internal static (List<Letter> Letters, List<Judged> Judged, string Text) Read(float[] samples, int rate, double pitchHz, double widthHz, bool rule)
    {
        using var off = rule ? null : CwRules.Off(CwRules.ShortDah);
        using var chain = new CwChain(rate);
        var gate = chain.Decoder.Runs;
        var letters = new List<Letter>();
        var judged = new List<Judged>();
        var text = new System.Text.StringBuilder();
        var now = 0.0;

        chain.Detector.SetPassband(pitchHz, widthHz);
        gate.CharacterRead += c => text.Append(c.Text);
        gate.RunRead += (c, run) => letters.Add(new Letter(run[^1].ToSeconds, run.Average(m => m.PitchHz), c.Text, string.Join(" ", run.Select(m => ((m.ToSeconds - m.FromSeconds) * 1000).ToString("0", Inv)))));
        CwSenderGate.ShortDahJudged = j => judged.Add(new Judged(now, j with { Sorted = j.Sorted.ToArray() }));

        try
        {
            var chunk = rate / 100;

            for (var at = 0; at + chunk <= samples.Length; at += chunk)
            {
                now = (at + chunk) / (double)rate;
                chain.Process(new AudioChunk(at, rate, samples.AsSpan(at, chunk)));
            }

            chain.Decoder.Flush();
        }
        finally
        {
            CwSenderGate.ShortDahJudged = null;
        }

        return (letters, judged, string.Join(' ', text.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries)));
    }

    /// <summary>The carrier keyed at random at one seed, as the scoreboard's hard limit keys it.</summary>
    internal static float[] Carrier(int seed)
    {
        var carrier = TheShapePicksTheSenderTests.Noise(25, 5192);

        TheShapePicksTheSenderTests.Key(carrier, TheShapePicksTheSenderTests.RandomKeying(2.5, 23, seed), 625, 24);

        return carrier;
    }

    /// <summary>The three costs.</summary>
    public static TheoryData<string> Costs => new() { "cw-2026-10-08-121357", "cq-25wpm-5db", "carrier-5207" };

    private (float[] Samples, int Rate, double PitchHz, double WidthHz, double TargetHz, double From, double To) Case(string name)
    {
        if (name == "cw-2026-10-08-121357")
        {
            var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(name));
            var (pitch, width) = TheRecordingsScoreboardTests.RadioState(name);

            // The board's reference keying: the station's marks on the keying map, and the dah over dit two means find.
            var map = TheKeyingMapTests.Of(name);
            var station = map.Stations.OrderBy(s => Math.Abs(s.PitchHz - 800)).First();
            var lengths = station.Marks.Where(m => m.From >= 14.3 && m.To <= 29.3).Select(m => m.To - m.From).Order().ToList();
            var (dits, dahs, ratio, spreads) = TheStraightKeyReadsTests.TwoMeans(lengths);

            output.WriteLine(string.Create(Inv, $"the keying map's station at {station.PitchHz:0} Hz, 14.3 to 29.3 s: {lengths.Count} marks, {dits.Count} dits centre {Math.Exp(dits.Average(x => Math.Log(x))) * 1000:0} ms, {dahs.Count} dahs centre {Math.Exp(dahs.Average(x => Math.Log(x))) * 1000:0} ms, true dah over dit {ratio:0.00}, {spreads:0.0} spreads apart; sorted {string.Join(" ", lengths.Select(l => (l * 1000).ToString("0", Inv)))}"));

            return (audio.Samples, audio.SampleRate, pitch, width, 800, 0, 31);
        }

        if (name == "cq-25wpm-5db")
        {
            var (_, wav, recipe) = TheWeakEndTests.Files().First(f => f.Name == name);
            var audio = WavAudio.Read(wav);
            var sent = TheWeakEndTests.Sent(recipe).Select(m => m.To - m.From).Order().ToList();
            var (dits, dahs, ratio, _) = TheStraightKeyReadsTests.TwoMeans(sent);

            output.WriteLine(string.Create(Inv, $"the recipe at {recipe.ToneHz:0} Hz: {sent.Count} marks, dit {Math.Exp(dits.Average(x => Math.Log(x))) * 1000:0} ms, dah {Math.Exp(dahs.Average(x => Math.Log(x))) * 1000:0} ms, true dah over dit {ratio:0.00}"));

            return (audio.Samples, audio.SampleRate, 600, 500, recipe.ToneHz, 0, 999);
        }

        return (Carrier(5207), 8000, 600, 500, 550, 0, 999);
    }

    /// <remarks>
    /// Work instruction 581, section 3: each cost read off and on, the letters printed where they differ, and every judgement
    /// the rule took within 40 Hz of the station, at the moment it was made, deduplicated while the marks it judged stay the same.
    /// </remarks>
    /// <param name="name">The cost.</param>
    [Theory]
    [MemberData(nameof(Costs))]
    public void EachCostTraced(string name)
    {
        var (samples, rate, pitch, width, target, from, to) = Case(name);
        var off = Read(samples, rate, pitch, width, false);
        var on = Read(samples, rate, pitch, width, true);

        output.WriteLine($"## {name}");
        output.WriteLine($"off: `{off.Text}`");
        output.WriteLine($"on:  `{on.Text}`");

        foreach (var (what, r) in new[] { ("off", off), ("on", on) })
        {
            output.WriteLine($"letters {what}:");

            foreach (var l in r.Letters.Where(l => l.At >= from && l.At <= to))
            {
                output.WriteLine(string.Create(Inv, $"   {l.At:0.00} s {l.PitchHz:0} Hz {l.Text} [{l.Marks}]"));
            }
        }

        output.WriteLine("taken, on:");
        var last = string.Empty;

        foreach (var j in on.Judged.Where(j => j.J.Taken && (double.IsNaN(j.J.PitchHz) || Math.Abs(j.J.PitchHz - target) <= 40)))
        {
            var key = j.J.Caller + string.Join(",", j.J.Sorted.Select(l => Math.Round(l * 1000)));

            if (key != last)
            {
                output.WriteLine("   " + j.Row);
                last = key;
            }
        }

        var taken = on.Judged.Where(j => j.J.Taken).ToList();
        output.WriteLine(string.Create(Inv, $"taken in all: {taken.Count}, by caller {string.Join(", ", taken.GroupBy(j => j.J.Caller).Select(g => $"{g.Key} {g.Count()}"))}"));
    }

    /// <remarks>
    /// Work instruction 581, section 3, the carrier's row: every judgement the rule takes on the carrier at its twenty seeds and on
    /// the bank's 1.6 and 2.0 fists, by each of the five conditions, so the margin a hand leaves above the carrier shows.
    /// </remarks>
    [Fact]
    public void WhereTheCarrierAndTheHandsStand()
    {
        static string P(IEnumerable<double> v)
        {
            var s = v.Where(double.IsFinite).Order().ToList();

            return s.Count == 0 ? "none" : string.Create(Inv, $"least {s[0]:0.00}, p5 {s[(int)(0.05 * s.Count)]:0.00}, median {s[s.Count / 2]:0.00}, p95 {s[(int)(0.95 * (s.Count - 1))]:0.00}, most {s[^1]:0.00}");
        }

        var carrier = Enumerable.Range(5193, 20).AsParallel().Select(seed => (Seed: seed, Read(Carrier(seed), 8000, 600, 500, true).Judged)).ToList();
        var hands = CwFistBank.All.Where(f => f.Ratio < 2.5).AsParallel()
            .Select(f =>
            {
                var a = CwFistBank.Audio(f);

                return (Fist: f, Read(a.Samples, a.SampleRate, 600, 500, true).Judged);
            }).ToList();

        void Show(string what, IReadOnlyList<Judged> taken)
        {
            output.WriteLine(string.Create(Inv, $"{what}, {taken.Count} taken: dit centre ms {P(taken.Select(j => j.J.DitSeconds * 1000))}; marks {P(taken.Select(j => (double)j.J.Sorted.Count))}; fewer of a kind {P(taken.Select(j => (double)Math.Min(j.J.At, j.J.Sorted.Count - j.J.At)))}; widest {P(taken.Select(j => j.Widest))}; dah over dit {P(taken.Select(j => j.Ratio))}; spreads {P(taken.Select(j => j.Spreads))}"));

            foreach (var c in taken.GroupBy(j => j.J.Caller))
            {
                output.WriteLine(string.Create(Inv, $"   by {c.Key}: {c.Count()}"));
            }
        }

        foreach (var (seed, judged) in carrier.OrderBy(c => c.Seed))
        {
            var taken = judged.Where(j => j.J.Taken).ToList();

            if (taken.Count > 0)
            {
                Show(string.Create(Inv, $"the carrier, seed {seed}"), taken);
            }
        }

        Show("the carrier, twenty seeds", carrier.SelectMany(c => c.Judged).Where(j => j.J.Taken).ToList());

        foreach (var ratio in new[] { 1.6, 2.0 })
        {
            Show(string.Create(Inv, $"the bank's {ratio:0.0} fists"), hands.Where(h => h.Fist.Ratio == ratio).SelectMany(h => h.Judged).Where(j => j.J.Taken && Math.Abs(j.J.PitchHz - CwFistBank.ToneHz) <= 40).ToList());
        }

        // Each condition tightened alone: how many carrier judgements and how many of the hands' would still pass.
        var carrierAll = carrier.SelectMany(c => c.Judged).Where(j => j.J.Taken).ToList();
        var handsAll = hands.SelectMany(h => h.Judged).Where(j => j.J.Taken && Math.Abs(j.J.PitchHz - CwFistBank.ToneHz) <= 40).ToList();

        foreach (var (what, keep) in new (string, Func<Judged, bool>)[]
        {
            ("marks 16", j => j.J.Sorted.Count >= 16),
            ("marks 20", j => j.J.Sorted.Count >= 20),
            ("each kind 4", j => Math.Min(j.J.At, j.J.Sorted.Count - j.J.At) >= 4),
            ("each kind 5", j => Math.Min(j.J.At, j.J.Sorted.Count - j.J.At) >= 5),
            ("widest 0.10", j => j.Widest <= 0.10),
            ("widest 0.08", j => j.Widest <= 0.08),
            ("ratio 1.55", j => j.Ratio >= 1.55),
            ("spreads 4.5", j => j.Spreads >= 4.5),
            ("spreads 5", j => j.Spreads >= 5),
            ("dit 25 ms, Morse's fastest", j => j.J.DitSeconds >= 0.025),
            ("dah over dit under 2", j => j.Ratio < 2),
            ("dah over dit under 2.2", j => j.Ratio < 2.2),
        })
        {
            var seeds = carrier.Count(c => c.Judged.Any(j => j.J.Taken && keep(j)));
            var fists = hands.Count(h => h.Judged.Any(j => j.J.Taken && Math.Abs(j.J.PitchHz - CwFistBank.ToneHz) <= 40 && keep(j)));

            output.WriteLine(string.Create(Inv, $"tightened to {what}: carrier judgements {carrierAll.Count(keep)} of {carrierAll.Count} at {seeds} seeds; hands' {handsAll.Count(keep)} of {handsAll.Count}, {fists} of {hands.Count} fists still taken"));
        }
    }
}
