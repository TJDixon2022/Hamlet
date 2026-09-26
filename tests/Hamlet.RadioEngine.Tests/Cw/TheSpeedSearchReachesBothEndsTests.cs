using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// A station sending at either end of HM-REQ-030's range is read, acquired cold
/// (work instruction 446; PHASE_PLAN.md 5.1; HM-REQ-030, 031, 033).
/// </summary>
/// <remarks>
/// <para>**FOUR SPEEDS, ONE TEXT, ONE LEVEL.** `SyntheticCq`'s CQ at PARIS timing,
/// 15 dB in the passband over the generator's shaped noise, never digital silence
/// (V-06), at 5 and 45 words a minute - the ends HM-REQ-030 rules hard - and at 8
/// and 40, the ends of the search at HEAD, as controls. The decoder is started at
/// 600 Hz and given no speed (HM-REQ-033).</para>
/// <para>**WHAT THESE CASES DO NOT PROVE** (CLAUDE.md 12.5): the generator and the
/// decoder share the one-three-seven model, so a clean read here says nothing
/// about a real sender at five or forty-five, a real channel, a hand key's
/// scatter, or Farnsworth spacing; one tone, no second station, no fading. No
/// synthetic case is ever the sole evidence for keeping a change.</para>
/// <para>**THE SPEED IS THE ONE THE OPERATOR IS SHOWN**,
/// <see cref="CwDecoder.WordsPerMinute"/>, sampled once a second of audio, and it
/// is judged over the samples taken after the first sure character settled: its
/// median within 10 % of true (HM-REQ-031). The median and the "after the first
/// sure character" are author's, overrulable.</para>
/// </remarks>
public sealed class TheSpeedSearchReachesBothEndsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the cases are printed.</param>
    public TheSpeedSearchReachesBothEndsTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The four speeds, in words a minute.</summary>
    internal static IReadOnlyList<double> Speeds { get; } = new[] { 5.0, 8.0, 40.0, 45.0 };

    /// <summary>One sample of the speed, taken once a second of audio.</summary>
    /// <param name="Seconds">Where in the audio.</param>
    /// <param name="Shown">What the operator is shown, or null.</param>
    /// <param name="Searched">What the last read's path was timed at.</param>
    /// <param name="AtTheEdge">Whether that read's speed sat at an end of the search.</param>
    internal readonly record struct SpeedSample(double Seconds, int? Shown, double Searched, bool AtTheEdge);

    /// <summary>One speed case, decoded.</summary>
    /// <param name="Wpm">The true speed.</param>
    /// <param name="Recipe">What the generator was given.</param>
    /// <param name="Reading">What settled, as text.</param>
    /// <param name="Settled">What settled, character by character.</param>
    /// <param name="Samples">The speed, once a second.</param>
    /// <param name="FirstSureSeconds">When the first sure character settled, or null.</param>
    /// <param name="Measured">The metrics against the exact key.</param>
    /// <param name="DecodeMilliseconds">How long the decode took on the wall clock.</param>
    internal sealed record SpeedCase(
        double Wpm,
        CwFixtureRecipe Recipe,
        CwReading Reading,
        IReadOnlyList<CwCharacter> Settled,
        IReadOnlyList<SpeedSample> Samples,
        double? FirstSureSeconds,
        TheRequirementsAreMeasuredTests.Measured Measured,
        double DecodeMilliseconds)
    {
        /// <summary>The operator's speed after the first sure character, where shown.</summary>
        public IReadOnlyList<int> Acquired => FirstSureSeconds is { } from
            ? Samples.Where(s => s.Seconds >= from && s.Shown is not null).Select(s => s.Shown!.Value).ToList()
            : Array.Empty<int>();

        /// <summary>The median of <see cref="Acquired"/>, or null.</summary>
        public double? AcquiredMedian
        {
            get
            {
                var sorted = Acquired.OrderBy(v => v).ToList();

                return sorted.Count == 0
                    ? null
                    : sorted.Count % 2 == 1
                        ? sorted[sorted.Count / 2]
                        : (sorted[(sorted.Count / 2) - 1] + sorted[sorted.Count / 2]) / 2.0;
            }
        }

        /// <summary>How many sure characters settled.</summary>
        public int SureEmitted => CwMetrics.Symbols(Settled).Count(s => s.Class == CwSymbolClass.Sure);
    }

    private static readonly Lazy<IReadOnlyDictionary<double, SpeedCase>> AllCases = new(
        () => Speeds.ToDictionary(w => w, Run));

    /// <summary>Every case, decoded once per process.</summary>
    internal static IReadOnlyDictionary<double, SpeedCase> Cases => AllCases.Value;

    /// <summary>The recipe for one speed: textbook CQ at 15 dB, its own seed.</summary>
    /// <param name="wpm">Words a minute.</param>
    /// <returns>The recipe.</returns>
    /// <remarks>The seeds are 20260926 plus the speed, author's; none was chosen from a decode.</remarks>
    internal static CwFixtureRecipe Recipe(double wpm)
        => SyntheticCq.Recipe(wpm, CwFixtureCatalogue.EasyDb, 20260926 + (int)wpm);

    private static SpeedCase Run(double wpm)
    {
        var recipe = Recipe(wpm);
        var (audio, _) = CwFixtureGenerator.Generate(recipe);
        var decoder = new CwDecoder(audio.SampleRate, SyntheticCq.StartingPitchHz);
        var settled = new List<CwCharacter>();
        var samples = new List<SpeedSample>();
        double? firstSure = null;

        decoder.CharacterSettled += c =>
        {
            settled.Add(c);

            if (firstSure is null && CwSymbol.Of(c).Class == CwSymbolClass.Sure)
            {
                firstSure = c.At.TotalSeconds;
            }
        };

        var hop = decoder.Tracker.HopSamples;
        var nextSample = (long)audio.SampleRate;
        var clock = System.Diagnostics.Stopwatch.StartNew();

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));

            if (at + hop >= nextSample)
            {
                var reading = decoder.Reading;

                samples.Add(new SpeedSample(
                    (at + hop) / (double)audio.SampleRate,
                    decoder.WordsPerMinute,
                    reading.WordsPerMinute,
                    reading.SpeedIsAtTheEdge));
                nextSample += audio.SampleRate;
            }
        }

        decoder.Flush();
        clock.Stop();

        var read = CwReading.Of(settled);
        var from = read.Text.TakeWhile(char.IsWhiteSpace).Count();
        var measured = TheRequirementsAreMeasuredTests.Measure(
            recipe.Name, "synthetic", TheRequirementsAreMeasuredTests.SyntheticCondition(recipe), CwKeyKind.Exact,
            settled, new[] { SyntheticCq.Whole(read) with { Start = from } });

        return new SpeedCase(wpm, recipe, read, settled, samples, firstSure, measured, clock.Elapsed.TotalMilliseconds);
    }

    private static string Num(double value, string format = "0.0")
        => value.ToString(format, CultureInfo.InvariantCulture);

    private static string Share(int count, int of) => of == 0
        ? "no number: nothing to divide by"
        : Num(count / (double)of, "0.0000");

    /// <summary>Prints one case as task 1 asks: text, speed and when, and the three metrics with the key.</summary>
    /// <param name="c">The case.</param>
    private void Print(SpeedCase c)
    {
        var tag = $"case {Num(c.Wpm, "0")} wpm";
        var m = c.Measured;

        _output.WriteLine($"{tag} | key (exact) | {c.Recipe.Text}");
        _output.WriteLine($"{tag} | text | {(c.Reading.Text.Trim().Length == 0 ? "nothing read" : c.Reading.Text.Trim())}");
        _output.WriteLine(
            $"{tag} | audio {Num(c.Samples.Count, "0")} s | decode {Num(c.DecodeMilliseconds, "0")} ms | "
            + $"first sure character at {(c.FirstSureSeconds is { } f ? Num(f) + " s" : "none")}");

        var firstShown = c.Samples.FirstOrDefault(s => s.Shown is not null);
        var firstRight = c.Samples.FirstOrDefault(s => s.Shown is { } v && Math.Abs(v - c.Wpm) <= 0.1 * c.Wpm);

        _output.WriteLine(
            $"{tag} | speed shown first {(firstShown.Shown is { } s0 ? $"{s0} at {Num(firstShown.Seconds, "0")} s" : "never")} | "
            + $"first within 10 % {(firstRight.Shown is { } s1 ? $"{s1} at {Num(firstRight.Seconds, "0")} s" : "never")} | "
            + $"after the first sure character: {c.Acquired.Count} samples shown, median {(c.AcquiredMedian is { } md ? Num(md) : "none")}, "
            + $"error {(c.AcquiredMedian is { } me ? Num(100 * Math.Abs(me - c.Wpm) / c.Wpm) + " %" : "no number")}");
        _output.WriteLine(
            $"{tag} | speed by second, shown/searched{{edge}} | "
            + string.Join(" ", c.Samples.Select(s =>
                $"{Num(s.Seconds, "0")}:{(s.Shown is { } v ? v.ToString(CultureInfo.InvariantCulture) : "-")}/{Num(s.Searched, "0")}{(s.AtTheEdge ? "e" : "")}")));

        if (m.NotComputable is { } why)
        {
            _output.WriteLine($"{tag} | metrics | no number: {why}");
            return;
        }

        var inv = m.Stretches.Select(CwMetrics.Invented).ToList();
        var err = m.Stretches.Select(CwMetrics.SureErrors).ToList();
        var cov = m.Stretches.Select(CwMetrics.Coverage).ToList();
        var wbe = m.Stretches.Select(CwMetrics.WordBoundaries).ToList();
        int sent = inv.Sum(p => p.Sent), invented = inv.Sum(p => p.Count);
        int sure = err.Sum(p => p.SureEmitted), wrong = err.Sum(p => p.Errors), right = cov.Sum(p => p.SureRight);
        int words = wbe.Sum(p => p.WordsSent), boundaries = wbe.Sum(p => p.Inserted + p.Deleted);

        _output.WriteLine(
            $"{tag} | metrics, exact key | MET-CER-SURE {wrong} of {sure} = {Share(wrong, sure)} | "
            + $"sure-and-right coverage {right} over {sent} = {Share(right, sent)} | "
            + $"MET-INVENTED {invented} over {sent} = {Share(invented, sent)} | "
            + $"MET-WBE {boundaries} over {words} = {Share(boundaries, words)}");
    }

    /// <summary>A bound on the speed search, where it is, and what either end asks of it.</summary>
    /// <param name="File">The source file under `src`.</param>
    /// <param name="Pattern">Text that finds its line.</param>
    /// <param name="Needs">What 5 and 45 words a minute need of it, as numbers.</param>
    /// <param name="Stops">Which end it stops: "5", "45", "both" or "neither".</param>
    private sealed record Bound(string File, string Pattern, string Needs, string Stops);

    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }

    /// <remarks>
    /// Serves 5.1 and asserts nothing: every bound the speed search has, with its
    /// file, line and value as the source holds it, what 5 and 45 words a minute
    /// need of each as numbers, and which end each one stops. Read off the source
    /// text, so the line numbers are the tree's own.
    /// </remarks>
    [Fact]
    public void EveryBoundOnTheSpeedSearch()
    {
        const double hop = CwProbabilisticDecoder.HopMilliseconds;
        double slow = 1200.0 / 5, fast = 1200.0 / 45;
        var integratorMs = 1000.0 * CwProbabilisticDecoder.IntegratorWindow(8_000, CwProbabilisticDecoder.IntegratorBandwidthHz) / 8_000;
        var windowHops = CwProbabilisticStream.WindowSeconds * 1000 / hop;
        var slowLongestSpan = (int)(7 * slow / hop * 2.2);

        _output.WriteLine(
            $"needs | unit {Num(slow)} ms at 5 wpm, {Num(fast)} ms at 45 | {Num(slow / hop)} and {Num(fast / hop)} hops of {Num(hop)} ms | "
            + $"longest dah at 5 wpm {Num(3 * slow, "0")} ms = {Num(3 * slow / hop, "0")} hops | word gap at 5 wpm {Num(7 * slow, "0")} ms = {Num(7 * slow / hop, "0")} hops | "
            + $"shortest element at 45 wpm {Num(fast)} ms = {Num(fast / hop)} hops");

        var bounds = new[]
        {
            new Bound("Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs", "public const double SlowestWpm",
                "the grid's floor; 5 wpm is 3 below it and a 5 wpm sender is fitted at 8, 60 % fast", "5"),
            new Bound("Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs", "public const double FastestWpm",
                "the grid's ceiling; 45 wpm is 5 above it and a 45 wpm sender is fitted at 40, 11 % slow", "45"),
            new Bound("Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs", "public const double WpmStep",
                "2 wpm apart, anchored at SlowestWpm; from 8 it lands on even speeds only, so 5 and 45 are on no grid point", "both"),
            new Bound("Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs", "var from = atWordsPerMinute ?? SlowestWpm",
                "the search loop's start, SlowestWpm", "5"),
            new Bound("Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs", "var to = atWordsPerMinute ?? FastestWpm",
                "the search loop's end, FastestWpm", "45"),
            new Bound("Hamlet.RadioEngine/Cw/CwProbabilisticStream.cs", "&& measured.WordsPerMinute >= CwProbabilisticDecoder.SlowestWpm",
                "the measured unit is used only inside the grid's range; a 5 wpm unit of 240 ms is refused and the grid decides", "5"),
            new Bound("Hamlet.RadioEngine/Cw/CwProbabilisticStream.cs", "&& measured.WordsPerMinute <= CwProbabilisticDecoder.FastestWpm",
                "a 45 wpm unit of 26.7 ms is refused and the grid decides, at 40 at most", "45"),
            new Bound("Hamlet.RadioEngine/Cw/CwProbabilisticStream.cs", "if (marksWpm >= CwProbabilisticDecoder.SlowestWpm",
                "the marks' re-read is taken only inside the grid's range; a 5 wpm marks' speed is refused", "5"),
            new Bound("Hamlet.RadioEngine/Cw/CwProbabilisticStream.cs", "&& marksWpm <= CwProbabilisticDecoder.FastestWpm)",
                "only ever fires when the marks are slower than the path, so it never asks for 45", "neither"),
            new Bound("Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs", "WordsPerMinute <= CwProbabilisticDecoder.SlowestWpm + 1e-9",
                "SpeedIsAtTheEdge reads the two constants; it follows them and stops nothing", "neither"),
            new Bound("Hamlet.RadioEngine/Cw/CwDecoder.cs", "public const int SlowestPlausibleWpm",
                "the speed the operator is shown is withheld below it; 5 wpm rounds to 5 and is never shown", "5"),
            new Bound("Hamlet.RadioEngine/Cw/CwDecoder.cs", "public const int FastestPlausibleWpm",
                "shown up to 48; 45 is inside it", "neither"),
            new Bound("Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs", "private const double ShortestShare",
                $"a segment's span runs 0.45 to 2.2 of its want; at 45 wpm a dit's floor is {(int)(fast / hop * 0.45)} hops, at 5 wpm a word gap's ceiling is {slowLongestSpan} hops against a window of {Num(windowHops, "0")}; a share, not a speed", "neither"),
            new Bound("Hamlet.RadioEngine/Cw/CwProbabilisticStream.cs", "public const double WindowSeconds",
                $"12 s holds {Num(12000 / slow, "0")} units at 5 wpm, one PARIS word; the estimator needs 8 marks and 8 gaps in it, which a CQ at 5 wpm gives", "neither"),
            new Bound("Hamlet.RadioEngine/Cw/CwProbabilisticStream.cs", "public const double DecisionDelaySeconds",
                $"1 s is {Num(1000 / slow)} units at 5 wpm, longer than the 3-unit gap after a letter, so the gap is seen before the letter settles", "neither"),
            new Bound("Hamlet.RadioEngine/Cw/CwProbabilisticStream.cs", "public const int ReadsToEstablishStructure",
                $"12 reads, 6 s, is {Num(6000 / slow)} units at 5 wpm; the textbook gaps are used until then, which is what a PARIS sender sends", "neither"),
            new Bound("Hamlet.RadioEngine/Cw/CwProbabilisticStream.cs", "public const double RefillSeconds",
                $"3 s is {Num(3000 / slow)} units at 5 wpm; not sized from a speed", "neither"),
            new Bound("Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs", "public const double NoiseSpanSeconds",
                $"2.5 s is {Num(2500 / slow)} units at 5 wpm; the quarter point needs key-up hops, which a slow sender gives more of; not sized from a speed", "neither"),
            new Bound("Hamlet.RadioEngine/Cw/CwUnitEstimator.cs", "private const int ShortestRunHops",
                $"2 hops, 10 ms, under the 45 wpm dit of {Num(fast / hop)} hops", "neither"),
            new Bound("Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs", "public const double IntegratorBandwidthHz",
                $"the Hann integrator spans {Num(integratorMs)} ms against a 45 wpm dit of {Num(fast)} ms, so it rounds every short mark; it is a bandwidth, not a speed bound, and is not moved", "neither"),
            new Bound("Hamlet.RadioEngine/Cw/CwToneTracker.cs", "public const double FastFistWpm",
                "the tone tracker's own speed split, 18 wpm; not a bound on the search, and the tracker is left alone", "neither"),
            new Bound("Hamlet.RadioEngine/Cw/AutoCall.cs", "Math.Clamp((int)wpm, 5, 60)",
                "the transmit side's own keyer range; shared with nothing here and not touched (CLAUDE.md 0.2)", "neither"),
        };

        var src = Path.Combine(Root(), "src");

        foreach (var b in bounds)
        {
            var lines = File.ReadAllLines(Path.Combine(src, b.File));
            var index = Array.FindIndex(lines, l => l.Contains(b.Pattern, StringComparison.Ordinal));
            var where = index < 0 ? "not found" : $"{index + 1}";
            var text = index < 0 ? "" : lines[index].Trim();

            _output.WriteLine($"bound | src/{b.File}:{where} | {text} | stops {b.Stops} | {b.Needs}");
        }

        _output.WriteLine(
            $"grid | {Num(CwProbabilisticDecoder.SlowestWpm, "0")} to {Num(CwProbabilisticDecoder.FastestWpm, "0")} wpm, "
            + $"step {Num(CwProbabilisticDecoder.WpmStep, "0")}");

        foreach (var c in Speeds.Select(w => Cases[w]))
        {
            Print(c);
        }
    }

    /// <remarks>
    /// Proves HM-REQ-030 at its slow end, with HM-REQ-033 and HM-REQ-031: a CQ
    /// sent at 5 words a minute, from cold, emits sure characters, and the speed
    /// the operator is shown after the first of them has a median within 10 % of
    /// 5.
    /// </remarks>
    [Fact]
    public void AStationSendingFiveWordsAMinuteIsRead() => AssertRead(5);

    /// <remarks>
    /// Proves HM-REQ-030 at its fast end, with HM-REQ-033 and HM-REQ-031: a CQ
    /// sent at 45 words a minute, from cold, emits sure characters, and the speed
    /// the operator is shown after the first of them has a median within 10 % of
    /// 45.
    /// </remarks>
    [Fact]
    public void AStationSendingFortyFiveWordsAMinuteIsRead() => AssertRead(45);

    /// <remarks>
    /// The controls for HM-REQ-030's two tests: the same assertion at 8 and 40,
    /// the ends of the search at HEAD. If one fails at HEAD, the matching end's
    /// failure is not the bound's.
    /// </remarks>
    /// <param name="wpm">Words a minute.</param>
    [Theory]
    [InlineData(8.0)]
    [InlineData(40.0)]
    public void TheOldEndsOfTheSearchAreRead(double wpm) => AssertRead(wpm);

    /// <remarks>
    /// Serves 5.1's "decode time reported before and after" and asserts nothing:
    /// one fixed real recording, `cw-2026-08-18-004507`, decoded three times the
    /// way the floors decode it, in milliseconds per minute of audio, with the
    /// median. The recording is author's: a keyed capture of ordinary length that
    /// no end of the speed range is about.
    /// </remarks>
    [Fact]
    public void OneRecordingsDecodeTime()
    {
        const string name = "cw-2026-08-18-004507";
        var audio = WavAudio.Read(Path.Combine(CapturedSignalTests.Folder, name + ".wav"));
        var minutes = audio.Samples.Length / (double)audio.SampleRate / 60;
        var runs = new List<double>();

        for (var run = 0; run < 3; run++)
        {
            var clock = System.Diagnostics.Stopwatch.StartNew();

            TheSeventeenThirtySevenCaptureTests.Settle(name);
            clock.Stop();
            runs.Add(clock.Elapsed.TotalMilliseconds / minutes);
        }

        var median = runs.OrderBy(v => v).ElementAt(1);

        _output.WriteLine(
            $"decode time | {name} | {Num(minutes, "0.00")} min of audio | runs {string.Join(", ", runs.Select(r => Num(r, "0")))} ms/min | median {Num(median, "0")} ms/min");
    }

    private void AssertRead(double wpm)
    {
        var c = Cases[wpm];

        Print(c);

        Assert.True(c.SureEmitted > 0, $"{wpm} wpm emitted no sure character");
        Assert.NotNull(c.AcquiredMedian);
        Assert.InRange(c.AcquiredMedian!.Value, 0.9 * wpm, 1.1 * wpm);
    }
}
