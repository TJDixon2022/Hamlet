using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Whether letting the clock choose where it looks rescues it (HM-OPEN-054).
/// </summary>
/// <remarks>
/// <para>**FOUR CANDIDATES HAVE BEEN REJECTED AND ALL FOUR WERE DIFFERENT TESTS
/// ON THE SAME INHERITED WINDOW** — a rolling span of the decoder's own state
/// holding a station and twenty-six seconds of band noise at once, so the test
/// measures the noise. Fitted to `cw-2026-08-17-134712`'s callsign window the
/// clock agrees at 0.677; fitted to the whole recording it agrees at 0.177,
/// against 0.116 for a recording holding nothing. **The statistic was never the
/// problem.**</para>
/// <para>**THE WINDOW IS CHOSEN FROM THE TRANSITIONS AND FROM NOTHING ELSE**, and
/// in particular not from HM-DEC-144's known boundaries: using the answer to find
/// the answer proves nothing. The gaps between transitions are split into a short
/// group and a long one by their own two means, seeded from the extremes, and a
/// burst is a maximal run of transitions linked by short gaps. No length, count or
/// rate is declared in advance; the only floor is the eight edges the clock fit
/// already refuses to run below.</para>
/// <para>**NOTHING HERE IS A GATE AND NOTHING IN `src` READS IT.** It answers
/// whether the separation exists, not whether the decoder may act on it, which is
/// §0.0's question and is not this file's to settle.</para>
/// </remarks>
public sealed class WhereTheTransitionsClusterTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tables are printed.</param>
    public WhereTheTransitionsClusterTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>One stretch where the transitions came close together.</summary>
    /// <param name="FromSeconds">When it starts.</param>
    /// <param name="ToSeconds">When it ends.</param>
    /// <param name="Edges">How many transitions it holds.</param>
    /// <param name="Agreement">How well a fitted clock explains them.</param>
    /// <param name="IntervalMs">The interval that fitted best.</param>
    private readonly record struct Burst(
        double FromSeconds,
        double ToSeconds,
        int Edges,
        double Agreement,
        double IntervalMs);

    private static MonoAudio Captured(string name) => WavAudio.Read(
        Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

    private static MonoAudio Fixture(string name) => WavAudio.Read(
        Path.Combine(CapturedSignalTests.Folder, "..", "receiver", name + ".wav"));

    /// <summary>Half a minute of shaped band noise with nobody in it.</summary>
    private static MonoAudio Noise(int seed)
    {
        var random = new Random(seed);
        var samples = new float[48_000 * 30];
        var band = 0.0;

        for (var i = 0; i < samples.Length; i++)
        {
            band = (0.965 * band) + (0.035 * ((random.NextDouble() - 0.5) * 2));
            samples[i] = (float)(band * 3);
        }

        return new MonoAudio(48_000, samples);
    }

    /// <summary>
    /// The stretches where transitions came close together, with a clock fitted
    /// to each.
    /// </summary>
    /// <remarks>
    /// The gaps between transitions are split in two by their own means, and a
    /// burst is a maximal run linked by gaps from the short group. **Where every
    /// gap is much the same length there is no short group to speak of**, the
    /// split lands inside one population, and what comes back is a description of
    /// noise rather than of a sender. That is the case worth watching and it is
    /// reported rather than filtered out.
    /// </remarks>
    private static List<Burst> Bursts(IReadOnlyList<double> edges)
    {
        if (edges.Count < 9)
        {
            return new List<Burst>();
        }

        var gaps = new double[edges.Count - 1];

        for (var i = 1; i < edges.Count; i++)
        {
            gaps[i - 1] = edges[i] - edges[i - 1];
        }

        double low = gaps.Min(), high = gaps.Max();

        for (var pass = 0; pass < 24; pass++)
        {
            var l = gaps.Where(g => Math.Abs(g - low) <= Math.Abs(g - high))
                .DefaultIfEmpty(low).Average();
            var h = gaps.Where(g => Math.Abs(g - low) > Math.Abs(g - high))
                .DefaultIfEmpty(high).Average();

            if (Math.Abs(l - low) < 1e-12 && Math.Abs(h - high) < 1e-12)
            {
                break;
            }

            low = l;
            high = h;
        }

        var cut = (low + high) / 2;
        var bursts = new List<Burst>();
        var start = 0;

        for (var i = 0; i <= gaps.Length; i++)
        {
            if (i < gaps.Length && gaps[i] <= cut)
            {
                continue;
            }

            // The run ends here: edges[start] through edges[i].
            var count = i - start + 1;

            if (count >= 8)
            {
                var slice = edges.Skip(start).Take(count).ToList();
                var (agreement, ms) = ACarrierClockDoesNotSeparateTests.Fit(slice);

                bursts.Add(new Burst(slice[0], slice[^1], count, agreement, ms));
            }

            start = i + 1;
        }

        return bursts;
    }

    private void Report(string label, IReadOnlyList<double> edges)
    {
        var bursts = Bursts(edges);

        if (bursts.Count == 0)
        {
            _output.WriteLine(
                $"{label,-42} {edges.Count,4} edges  no burst of eight or more");
            return;
        }

        var best = bursts.OrderByDescending(b => b.Edges).First();
        var strongest = bursts.OrderByDescending(b => b.Agreement).First();

        _output.WriteLine(
            $"{label,-42} {edges.Count,4} edges  {bursts.Count,3} bursts  "
            + $"| biggest {best.FromSeconds,6:0.00}-{best.ToSeconds:0.00}s "
            + $"{best.Edges,3} edges  {best.Agreement:0.000} at {best.IntervalMs,5:0} ms  "
            + $"| strongest {strongest.Agreement:0.000} at {strongest.IntervalMs,5:0} ms "
            + $"({strongest.FromSeconds:0.00}-{strongest.ToSeconds:0.00}s, {strongest.Edges} edges)");
    }

    /// <remarks>
    /// <para>Task 1 and task 2 together: where the transitions cluster in every
    /// real capture, and what a clock fitted to each cluster says.</para>
    /// <para>**THE TWO RECORDINGS HOLDING NOTHING ARE THE POINT.** If the method
    /// confidently proposes a window in `cw-2026-08-20-014854` or `-014935`, that
    /// is the finding.</para>
    /// </remarks>
    [Fact]
    public void EveryRealCapture()
    {
        foreach (var wav in Directory
                     .GetFiles(CapturedSignalTests.Folder, "*.wav", SearchOption.AllDirectories)
                     .OrderBy(p => p, StringComparer.Ordinal))
        {
            var audio = WavAudio.Read(wav);

            Report(
                Path.GetFileNameWithoutExtension(wav),
                ACarrierClockDoesNotSeparateTests.Transitions(audio, 600));
        }

        // **A WEAK CONTROL AND IT IS REPORTED AS ONE.** Shaped noise with no tone
        // in it never latches the tracker, so the gate produces no transitions at
        // all and there is nothing to fit. The two real recordings holding no
        // keying are the meaningful controls here, and they produce hundreds.
        Report(
            "SYNTHESIZED NOISE",
            ACarrierClockDoesNotSeparateTests.Transitions(Noise(7300), 600));

        // **AND THE METHOD PROPOSES WINDOWS IN RECORDINGS HOLDING NOTHING**, more
        // confidently than in one holding a station: `cw-2026-08-20-014935`'s best
        // burst agrees at 0.736 while `cw-2026-08-17-013347`, which decodes a real
        // callsign, manages 0.393.
        var empty = Bursts(ACarrierClockDoesNotSeparateTests.Transitions(
            Captured("unadjudicated/cw-2026-08-20-014935"), 600));
        var station = Bursts(ACarrierClockDoesNotSeparateTests.Transitions(
            Captured("cw-2026-08-17-013347"), 600));

        Assert.NotEmpty(empty);
        Assert.True(
            empty.Max(b => b.Agreement) > station.Max(b => b.Agreement),
            "the recording with a station in it produced a better-fitting burst "
            + "than the one holding nothing, which would mean this works");
    }

    /// <remarks>
    /// Task 2: the same on the easy tier, where every burst is a real sender and
    /// the interval is known.
    /// </remarks>
    [Fact]
    public void TheEasyTier()
    {
        foreach (var wav in Directory
                     .GetFiles(
                         Path.Combine(CapturedSignalTests.Folder, "..", "receiver"),
                         "*-easy.wav")
                     .OrderBy(p => p, StringComparer.Ordinal))
        {
            var audio = WavAudio.Read(wav);

            Report(
                Path.GetFileNameWithoutExtension(wav),
                ACarrierClockDoesNotSeparateTests.Transitions(audio, 600));
        }

        Assert.True(true);
    }

    /// <summary>
    /// The trailing run of transitions linked by short gaps, as it stands now.
    /// </summary>
    /// <remarks>
    /// The causal form of the same split: the short and long gap groups are
    /// fitted over everything heard so far, and the window is the run of
    /// transitions reaching back from the newest one. **Nothing here looks
    /// forward**, so what it returns is a claim about the present (§0.0).
    /// </remarks>
    private static List<double> CurrentBurst(IReadOnlyList<double> edges)
    {
        if (edges.Count < 9)
        {
            return new List<double>();
        }

        var gaps = new double[edges.Count - 1];

        for (var i = 1; i < edges.Count; i++)
        {
            gaps[i - 1] = edges[i] - edges[i - 1];
        }

        double low = gaps.Min(), high = gaps.Max();

        for (var pass = 0; pass < 24; pass++)
        {
            var l = gaps.Where(g => Math.Abs(g - low) <= Math.Abs(g - high))
                .DefaultIfEmpty(low).Average();
            var h = gaps.Where(g => Math.Abs(g - low) > Math.Abs(g - high))
                .DefaultIfEmpty(high).Average();

            low = l;
            high = h;
        }

        var cut = (low + high) / 2;
        var from = gaps.Length;

        while (from > 0 && gaps[from - 1] <= cut)
        {
            from--;
        }

        return edges.Skip(from).ToList();
    }

    /// <summary>
    /// How well a clock fits the burst in progress at each character emitted.
    /// </summary>
    private (List<double> Scores, int Characters) AtEmission(
        MonoAudio audio, double startHz)
    {
        var decoder = new CwDecoder(audio.SampleRate, startHz);
        var hop = decoder.Tracker.HopSamples;
        var edges = new List<double>();
        var scores = new List<double>();
        var characters = 0;
        var seen = 0;

        decoder.CharacterDecoded += _ =>
        {
            characters++;

            var burst = CurrentBurst(edges);

            if (burst.Count >= 8)
            {
                scores.Add(ACarrierClockDoesNotSeparateTests.Fit(burst).Agreement);
            }
        };

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));

            if (decoder.Report.ElementsSeen == seen)
            {
                continue;
            }

            edges.Add(at / (double)audio.SampleRate);
            seen = decoder.Report.ElementsSeen;
        }

        decoder.Flush();
        scores.Sort();

        return (scores, characters);
    }

    /// <remarks>
    /// <para>Task 3: **the pair that killed the last candidate, re-measured on
    /// windows the transitions chose.** Last session a real character came out at
    /// 0.389 and an invented one at 0.470.</para>
    /// </remarks>
    [Fact]
    public void TheFatalPairOnChosenWindows()
    {
        foreach (var name in new[]
                 {
                     "cw-2026-08-17-013347",
                     "unadjudicated/cw-2026-08-18-003016",
                     "cw-2026-08-18-004507",
                     "unadjudicated/cw-2026-08-18-003126",
                     "unadjudicated/cw-2026-08-18-003758",
                     "unadjudicated/cw-2026-08-20-014854",
                     "unadjudicated/cw-2026-08-20-014935",
                 })
        {
            var (scores, characters) = AtEmission(Captured(name), 600);

            _output.WriteLine(scores.Count == 0
                ? $"{Path.GetFileName(name),-24} {characters,3} characters, no burst to fit"
                : $"{Path.GetFileName(name),-24} {characters,3} characters  "
                  + $"lowest {scores[0]:0.000}  tenth {scores[scores.Count / 10]:0.000}  "
                  + $"median {scores[scores.Count / 2]:0.000}  highest {scores[^1]:0.000}");
        }

        foreach (var name in new[] { "prosigns-easy", "tightfist-easy", "exchange-easy" })
        {
            var (scores, characters) = AtEmission(Fixture(name), 600);

            _output.WriteLine(scores.Count == 0
                ? $"{name,-24} {characters,3} characters, no burst to fit"
                : $"{name,-24} {characters,3} characters  "
                  + $"lowest {scores[0]:0.000}  tenth {scores[scores.Count / 10]:0.000}  "
                  + $"median {scores[scores.Count / 2]:0.000}  highest {scores[^1]:0.000}");
        }

        // **THE REASON THIS IS DEAD, ASSERTED SO IT CANNOT BE MISLAID.** A gate
        // needing a fittable burst would silence an easy-tier fixture outright,
        // which HM-DEC-114 makes a hard failure: `tightfist-easy` emits real
        // characters and never once has a burst that can be fitted.
        var tight = AtEmission(Fixture("tightfist-easy"), 600);

        Assert.True(tight.Characters > 0);
        Assert.Empty(tight.Scores);

        // **THE OTHER REASON HAS SINCE BEEN OVERTAKEN AND THAT IS WORTH SAYING.**
        // When this was measured, `cw-2026-08-20-014854` invented a character at
        // an agreement of 0.557 while a real one came out at 0.353, so no line
        // separated them. The mark-separation test now stops that recording
        // emitting at all, so there is no invented character left to compare
        // against. **The finding stands on the easy-tier failure above**, which
        // no later change can talk its way out of.
        var quiet = AtEmission(Captured("unadjudicated/cw-2026-08-20-014854"), 600);

        Assert.Equal(0, quiet.Characters);
    }

    /// <remarks>
    /// <para>Task 1's own test: **does the method find `N4L` without being told
    /// where it is?** HM-DEC-144 puts the callsign at 21.45 to 23.01 s with a dit
    /// of 56.3 ms, and none of that is an input here.</para>
    /// </remarks>
    [Fact]
    public void DoesItFindTheCallsignWithoutBeingTold()
    {
        var edges = ACarrierClockDoesNotSeparateTests.Transitions(
            Captured("cw-2026-08-17-134712"), 500);

        var bursts = Bursts(edges);

        _output.WriteLine($"{bursts.Count} bursts in 134712:");

        foreach (var burst in bursts.OrderBy(b => b.FromSeconds))
        {
            var overlaps = burst.FromSeconds <= 23.01 && burst.ToSeconds >= 21.45;

            _output.WriteLine(
                $"  {burst.FromSeconds,6:0.00}-{burst.ToSeconds:0.00}s  "
                + $"{burst.Edges,3} edges  {burst.Agreement:0.000} at "
                + $"{burst.IntervalMs,5:0} ms" + (overlaps ? "   <- the callsign" : ""));
        }

        // **IT DOES NOT FIND THE CALLSIGN.** The burst covering it runs from
        // 17.82 s to 27.57 s and holds 114 transitions, so `N4L`'s twenty-one are
        // a fifth of a window otherwise made of band noise, and the clock fitted
        // to it agrees at 0.220 against the 0.677 the callsign's own window gives
        // when it is handed over by name.
        var covering = bursts.Single(b => b.FromSeconds <= 23.01 && b.ToSeconds >= 21.45);

        Assert.True(
            covering.ToSeconds - covering.FromSeconds > 5,
            "the burst covering the callsign was short enough to be the callsign, "
            + "which would mean the method found it after all");

        Assert.True(covering.Agreement < 0.4, $"it agreed {covering.Agreement:0.000}");
    }
}
