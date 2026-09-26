using System.Diagnostics;
using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Hamlet.RadioEngine.Tests.Cw.Instruments;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The pitch instrument lands within one of its own bins on tones whose pitch is
/// known by construction (work instruction 447; PHASE_PLAN.md 4.1, R76; HM-REQ-092,
/// HM-REQ-090).
/// </summary>
/// <remarks>
/// <para>**HM-REQ-092 ASKS FOR THE DEMODULATED PITCH WITHIN N HZ OF TRUE, AND N IS
/// TBD.** Nothing here judges N. This proves the ruler that MET-PITCH-ERR will be
/// read with: every case within <see cref="CwPitchInstrument.BinHz"/> of the truth,
/// in every window where the instrument found keying.</para>
/// <para>**THE CASES**, every one from <see cref="CwFixtureGenerator"/> over its
/// shaped noise band, never digital silence (V-06), with `SyntheticCq`'s CQ at
/// PARIS timing as the exact key: nine pitches from HM-REQ-090's 300 to its 900,
/// off-grid values and the decision log's 615/675 and 625/525 pairs among them, at
/// 12, 20 and 30 WPM and 15 dB; 625 Hz at 20 WPM and 5 dB; 625 Hz keyed under a
/// steady carrier at 525 Hz six decibels louder; and a note sliding 20 Hz over 30 s.
/// The generator's default 3 Hz swing is set to nought so the truth is exact; the
/// slide's truth is the note's pitch at the instant the instrument says its
/// window's weight sits.</para>
/// <para>**WHAT THESE CASES DO NOT PROVE** (CLAUDE.md 12.5): a transmitter's chirp,
/// a real band's QRM, fading, or a hand key. The instrument shares no code with the
/// tracker, so a pass here is not the tracker's misunderstanding agreeing with
/// itself; it is also not a claim about any real capture.</para>
/// </remarks>
public sealed class ThePitchInstrumentIsProvedTests
{
    /// <summary>
    /// Added to every truth. Nought; set to one hertz once, to watch the test fail
    /// against a deliberately wrong truth before it passed.
    /// </summary>
    internal const double TruthOffsetHz = 0;

    /// <summary>The pitches, in hertz.</summary>
    internal static IReadOnlyList<double> Pitches { get; } = new[] { 300.0, 400, 523, 600, 625, 640, 675, 750, 900 };

    /// <summary>The speeds, in words a minute.</summary>
    internal static IReadOnlyList<double> Speeds { get; } = new[] { 12.0, 20, 30 };

    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the table is printed.</param>
    public ThePitchInstrumentIsProvedTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>One case: its audio and its truth at any instant.</summary>
    /// <param name="Name">What the table calls it.</param>
    /// <param name="Audio">The rendering.</param>
    /// <param name="Truth">The keyed note's pitch at an instant, in hertz.</param>
    /// <param name="Described">The truth, as the table prints it.</param>
    internal sealed record PitchCase(string Name, MonoAudio Audio, Func<double, double> Truth, string Described);

    /// <summary>One case measured.</summary>
    /// <param name="Case">The case.</param>
    /// <param name="Windows">Every window the instrument measured.</param>
    /// <param name="WorstHz">The largest error over those windows, signed.</param>
    internal sealed record PitchResult(PitchCase Case, IReadOnlyList<PitchWindow> Windows, double WorstHz)
    {
        /// <summary>Whether the instrument found keying anywhere.</summary>
        public bool Admitted => Windows.Count > 0;

        /// <summary>Whether every window landed within one bin.</summary>
        public bool Inside => Admitted && Math.Abs(WorstHz) <= CwPitchInstrument.BinHz;
    }

    /// <summary>Every case, in the table's order.</summary>
    /// <returns>The cases.</returns>
    /// <remarks>The seeds are 20260447 upward in order, author's; none was chosen from a measurement.</remarks>
    internal static IReadOnlyList<PitchCase> Cases()
    {
        var cases = new List<PitchCase>();
        var seed = 20260447;

        foreach (var wpm in Speeds)
        {
            foreach (var hz in Pitches)
            {
                var recipe = SyntheticCq.Recipe(wpm, CwFixtureCatalogue.EasyDb, seed++) with { ToneHz = hz, DriftHz = 0 };
                cases.Add(Steady(string.Create(Invariant, $"{hz:0} Hz, {wpm:0} WPM, 15 dB"), recipe));
            }
        }

        var weak = SyntheticCq.Recipe(20, CwFixtureCatalogue.WorkingDb, seed++) with { ToneHz = 625, DriftHz = 0 };
        cases.Add(Steady("625 Hz, 20 WPM, 5 dB", weak));

        // Two stations: the keyed 625 under a steady 525 six decibels louder, on
        // from the first sample to the last so it is never keyed even once. The
        // generator sets the tone's peak at root two times the band's RMS times the
        // signal-to-noise; the band's RMS is read here from the lead-in, before any mark.
        var keyedRecipe = SyntheticCq.Recipe(20, CwFixtureCatalogue.EasyDb, seed++) with { ToneHz = 625, DriftHz = 0 };
        var (keyed, _) = CwFixtureGenerator.Generate(keyedRecipe);
        var lead = (int)(0.9 * keyed.SampleRate);
        var bandRms = Math.Sqrt(keyed.Samples.Take(lead).Sum(v => (double)v * v) / lead);
        var carrierPeak = Math.Sqrt(2) * bandRms * Math.Pow(10, (CwFixtureCatalogue.EasyDb + 6) / 20);
        var both = new float[keyed.Samples.Length];

        for (var n = 0; n < both.Length; n++)
        {
            both[n] = keyed.Samples[n] + (float)(carrierPeak * Math.Sin(2 * Math.PI * 525 * n / keyed.SampleRate));
        }

        cases.Add(new PitchCase(
            "625 Hz keyed, 525 Hz carrier +6 dB unkeyed, 20 WPM",
            new MonoAudio(keyed.SampleRate, both),
            _ => 625,
            "625"));

        // The slide: 615 Hz at the start of the file, 20 Hz more every 30 s, over
        // the CQ sent twice so the message itself spans more than 30 s.
        const double rate = 20.0 / 30.0;
        var slide = SyntheticCq.Recipe(20, CwFixtureCatalogue.EasyDb, seed++) with
        {
            Text = SyntheticCq.Text + " " + SyntheticCq.Text,
            ToneHz = 615,
            DriftHz = 0,
            DriftHzPerSecond = rate,
        };
        var (sliding, _) = CwFixtureGenerator.Generate(slide);
        cases.Add(new PitchCase(
            "615 Hz sliding +20 Hz per 30 s, 20 WPM, 15 dB",
            sliding,
            t => 615 + (rate * t),
            "615 + 0.667 t"));

        return cases;

        static PitchCase Steady(string name, CwFixtureRecipe recipe)
        {
            var (audio, _) = CwFixtureGenerator.Generate(recipe);

            return new PitchCase(name, audio, _ => recipe.ToneHz, string.Create(Invariant, $"{recipe.ToneHz:0}"));
        }
    }

    /// <summary>Measure one case against its truth.</summary>
    /// <param name="pitchCase">The case.</param>
    /// <returns>Every window and the worst error.</returns>
    internal static PitchResult Measure(PitchCase pitchCase)
    {
        var windows = CwPitchInstrument.Measure(pitchCase.Audio.Samples, pitchCase.Audio.SampleRate);
        var worst = 0.0;

        foreach (var w in windows)
        {
            var error = w.Hz - (pitchCase.Truth(w.EffectiveSeconds) + TruthOffsetHz);

            if (Math.Abs(error) > Math.Abs(worst))
            {
                worst = error;
            }
        }

        return new PitchResult(pitchCase, windows, worst);
    }

    /// <summary>
    /// HM-REQ-092, PHASE_PLAN.md 4.1: on every case known by construction, every
    /// window the instrument measured lands within one of its own bins of the truth.
    /// </summary>
    [Fact]
    public void EveryCaseLandsWithinOneOfItsOwnBinsOfTheTruth()
    {
        var results = Cases().Select(Measure).ToList();

        _output.WriteLine(string.Create(Invariant,
            $"instrument | bin {CwPitchInstrument.BinHz:0.00} Hz | window {CwPitchInstrument.WindowSeconds:0.0} s every {CwPitchInstrument.HopSeconds:0.0} s | keying swing at least {CwPitchInstrument.ContrastFloorDb:0} dB | at least {CwPitchInstrument.LeastKeyedSeconds:0.0} s keyed | truth offset {TruthOffsetHz:0.0} Hz"));
        _output.WriteLine("case | truth Hz | estimate Hz (median of windows; first to last on the slide) | worst error Hz | bin Hz | inside one bin | windows | coarse Hz | keyed s");

        foreach (var r in results)
        {
            var estimates = r.Windows.Select(w => w.Hz).OrderBy(v => v).ToList();
            var estimate = r.Case.Described.Contains('t', StringComparison.Ordinal) && r.Windows.Count > 0
                ? string.Create(Invariant, $"{r.Windows[0].Hz:0.00} to {r.Windows[^1].Hz:0.00}")
                : estimates.Count == 0 ? "none" : string.Create(Invariant, $"{estimates[estimates.Count / 2]:0.00}");
            var coarse = string.Join(",", r.Windows.Select(w => w.CoarseHz.ToString("0.0", Invariant)).Distinct());

            _output.WriteLine(string.Create(Invariant,
                $"case | {r.Case.Name} | {r.Case.Described} | {estimate} | {r.WorstHz:+0.000;-0.000;0.000} | {CwPitchInstrument.BinHz:0.00} | {(r.Inside ? "yes" : "NO")} | {r.Windows.Count} | {coarse} | {r.Windows.Sum(w => w.KeyedSeconds):0.0}"));
        }

        _output.WriteLine(string.Create(Invariant,
            $"total | {results.Count(r => r.Admitted)} of {results.Count} cases admitted | {results.Count(r => r.Inside)} of {results.Count} inside one bin | worst {results.Max(r => Math.Abs(r.WorstHz)):0.000} Hz"));

        foreach (var r in results.Where(r => !r.Inside))
        {
            foreach (var w in r.Windows)
            {
                _output.WriteLine(string.Create(Invariant,
                    $"missed window | {r.Case.Name} | {w.StartSeconds:0.0} to {w.EndSeconds:0.0} s | at {w.EffectiveSeconds:0.00} s | {w.Hz:0.000} Hz against {r.Case.Truth(w.EffectiveSeconds) + TruthOffsetHz:0.000} | coarse {w.CoarseHz:0.0} | swing {w.ContrastDb:0.0} dB | keyed {w.KeyedSeconds:0.00} s"));
            }
        }

        Assert.All(results, r => Assert.True(
            r.Inside,
            string.Create(Invariant, $"{r.Case.Name}: {r.Windows.Count} windows, worst error {r.WorstHz:0.000} Hz against a bin of {CwPitchInstrument.BinHz:0.00} Hz")));
    }

    /// <summary>
    /// The instrument's cost, printed: the median of three runs over one fixed
    /// real recording, the 7.052 opening's `cw-2026-09-24-003919`. Asserts nothing.
    /// </summary>
    [Fact]
    public void ItsCostPerHopIsPrinted()
    {
        const string name = "cw-2026-09-24-003919";
        var audio = WavAudio.Read(Path.Combine(CapturedSignalTests.Folder, "unadjudicated", name + ".wav"));
        var runs = new List<double>();
        IReadOnlyList<PitchWindow> windows = Array.Empty<PitchWindow>();

        for (var run = 0; run < 3; run++)
        {
            var clock = Stopwatch.StartNew();
            windows = CwPitchInstrument.Measure(audio.Samples, audio.SampleRate);
            runs.Add(clock.Elapsed.TotalMilliseconds * 1000);
        }

        runs.Sort();
        var median = runs[1];
        var seconds = (double)audio.Samples.Length / audio.SampleRate;
        var instrumentHops = Math.Floor((seconds - CwPitchInstrument.WindowSeconds) / CwPitchInstrument.HopSeconds) + 1;
        var decoderHops = seconds * 1000 / Hamlet.RadioEngine.Cw.CwProbabilisticDecoder.HopMilliseconds;

        _output.WriteLine(string.Create(Invariant,
            $"cost | {name} | {audio.SampleRate} samples a second | {seconds:0.00} s | runs {runs[0] / 1000:0.0}, {runs[1] / 1000:0.0}, {runs[2] / 1000:0.0} ms | median {median / 1000:0.0} ms | {instrumentHops:0} instrument hops of {CwPitchInstrument.HopSeconds:0.0} s: {median / instrumentHops:0} us per instrument hop | {decoderHops:0} decoder hops of {Hamlet.RadioEngine.Cw.CwProbabilisticDecoder.HopMilliseconds:0} ms: {median / decoderHops:0.0} us per decoder hop | {windows.Count} windows keyed"));
    }

    /// <summary>
    /// R76's independence, checked from the instrument's own source: no line of
    /// code (comments aside) names the tracker, the survey, the pitch choice, a
    /// Goertzel, or any namespace of the product. Every identifier it could reach
    /// is printed as the list of `using` lines, which is empty.
    /// </summary>
    [Fact]
    public void ItSharesNoCodeWithTheTracker()
    {
        var root = Path.GetFullPath(Path.Combine(CapturedSignalTests.Folder, "..", "..", "..", ".."));
        var file = Path.Combine(root, "tests", "Hamlet.RadioEngine.Tests", "Cw", "Instruments", "CwPitchInstrument.cs");
        var code = File.ReadAllLines(file)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0 && !l.StartsWith("//", StringComparison.Ordinal))
            .ToList();
        var usings = code.Where(l => l.StartsWith("using ", StringComparison.Ordinal)).ToList();
        var forbidden = new[] { "CwToneTracker", "CwToneSurvey", "CwPitchChoice", "Goertzel", "Hamlet.RadioEngine.Cw", "Hamlet.RadioEngine.Audio", "CwDecoder", "CwProbabilistic", "CwCompetitor", "ToneReading", "ToneVerdict" };
        var hits = code.Where(l => forbidden.Any(f => l.Contains(f, StringComparison.Ordinal))).ToList();

        _output.WriteLine($"source | {file} | {code.Count} lines of code");
        _output.WriteLine($"usings | {(usings.Count == 0 ? "none" : string.Join("; ", usings))}");
        _output.WriteLine($"forbidden names on a code line | {hits.Count}");

        foreach (var hit in hits)
        {
            _output.WriteLine($"hit | {hit}");
        }

        Assert.Empty(usings);
        Assert.Empty(hits);
    }
}
