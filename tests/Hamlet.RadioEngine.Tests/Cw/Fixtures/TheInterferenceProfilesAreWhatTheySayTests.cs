using System.Globalization;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>
/// Each INT-* interference profile CW_SPEC.md section 9.3 names - INT-ADJ, INT-COCHAN,
/// INT-CARRIER and INT-PILEUP - is laid over the wanted station the way its definition
/// states, at every parameter HM-REQ-060, 061, 062 and 066 are measured at (work
/// instruction 469, task 1; PHASE_PLAN.md 7.3).
/// </summary>
/// <remarks>
/// <para>**MEASURED FROM THE RENDERED AUDIO, BEFORE THE BAND IS ADDED, NEVER FROM THE
/// RECIPE** (CLAUDE.md 12.5). Each station's tone is measured as rendered - the wanted
/// station's and every layer's - and the mix is checked to be exactly their sum over the
/// band. A pitch is the rate of upward zero crossings inside the tone's own key-down
/// runs; a level is the mean power of 25 ms windows at key-down; keyed or steady is how
/// that power moves across the whole case. Two tones 10 Hz apart cannot be told apart in
/// time on the mix itself, so each is measured on its own rendered layer. The code was
/// written for this test and shares nothing with <c>CwToneTracker</c>, <c>CwToneSurvey</c>
/// or any decoder front end; it reads one constant from the decoder, the detector
/// bandwidth INT-COCHAN is defined against.</para>
/// <para>**WATCHED FAILING FIRST.** With <c>HAMLET_UNIT469_INTERFERENCE=nominal</c> in the
/// environment the mixer ignores every profile and lays the wanted station over itself -
/// its pitch, its level, its keying - and every case must go red.</para>
/// </remarks>
public sealed class TheInterferenceProfilesAreWhatTheySayTests
{
    /// <summary>The wanted station's speed in every proof case: the middle of <see cref="SyntheticCq.Speeds"/>.</summary>
    internal const double WantedWpm = 18;

    /// <summary>How far a measured offset may sit from the stated one (the instruction's 2 Hz).</summary>
    private const double OffsetToleranceHz = 2;

    /// <summary>How far a measured level may sit from the stated one (the instruction's 0.5 dB).</summary>
    private const double LevelToleranceDb = 0.5;

    /// <summary>How far a carrier's power may move over the whole case (the instruction's 1 dB).</summary>
    private const double SteadyDb = 1;

    /// <summary>The window every power is measured over: 25 ms, fifteen cycles at 600 Hz.</summary>
    private const double WindowSeconds = 0.025;

    /// <summary>The windows' hop.</summary>
    private const double HopSeconds = 0.005;

    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each check is printed.</param>
    public TheInterferenceProfilesAreWhatTheySayTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// Every case task 2 measures, at one wanted speed: INT-ADJ at 060's, 061's and 066's
    /// parameters, INT-COCHAN, INT-CARRIER at 062's twelve, and INT-PILEUP.
    /// </summary>
    internal static IReadOnlyList<CwInterferenceSpec> Specs { get; } = new[]
    {
        CwInterference.Adjacent(100, 0, 25),
        CwInterference.Adjacent(-100, 0, 25),
        CwInterference.Adjacent(50, -6, 25),
        CwInterference.Adjacent(-50, -6, 25),
        CwInterference.Adjacent(100, -2.4, 25),
        CwInterference.Adjacent(100, -6, 25),
        CwInterference.CoChannel(),
    }
    .Concat(new[] { 50.0, 100, 200, 500 }.SelectMany(f => new[] { 0.0, 10, 20 }.Select(db => CwInterference.Carrier(f, db))))
    .Append(CwInterference.Pileup())
    .ToList();

    /// <summary>Every case, by its label.</summary>
    public static TheoryData<string> Cases
    {
        get
        {
            var data = new TheoryData<string>();

            foreach (var s in Specs)
            {
                data.Add(s.Label);
            }

            return data;
        }
    }

    /// <summary>Whether this run lays the wanted station over itself, to watch the proof fail.</summary>
    private static bool Nominal
        => string.Equals(Environment.GetEnvironmentVariable("HAMLET_UNIT469_INTERFERENCE"), "nominal", StringComparison.Ordinal);

    /// <remarks>
    /// Proves 7.3's first condition in part: section 9.3's four INT-* ids are all produced,
    /// each by a case below.
    /// </remarks>
    [Fact]
    public void EverySectionNinePointThreeInterferenceProfileIsProduced()
    {
        foreach (var p in CwInterference.Profiles)
        {
            var cases = Specs.Count(s => s.ProfileId == p.Id);
            _output.WriteLine($"profile | {p.Id} | {p.Term} | \"{p.Definition}\" | {cases} case(s) proved below");
        }

        Assert.Equal(new[] { "INT-ADJ", "INT-COCHAN", "INT-CARRIER", "INT-PILEUP" }, CwInterference.Profiles.Select(p => p.Id));
        Assert.All(CwInterference.Profiles, p => Assert.Contains(Specs, s => s.ProfileId == p.Id));
    }

    /// <remarks>
    /// Proves 7.3's second condition: CW_SPEC.md 9.3's INT-ADJ, INT-COCHAN, INT-CARRIER and
    /// INT-PILEUP, at HM-REQ-060's, 061's, 062's and 066's parameters, measured on the
    /// rendered audio - each tone's offset from the wanted within 2 Hz, its key-down power
    /// against the wanted's within 0.5 dB, a carrier steady within 1 dB over the whole case
    /// and a station keying on and off, INT-COCHAN inside the detector bandwidth and
    /// INT-PILEUP three or more keyed stations inside the passband; the mix exactly the
    /// band plus every tone, never clipped, never silent (V-06); and the wanted key the
    /// text keyed (R61, V-13).
    /// </remarks>
    [Theory]
    [MemberData(nameof(Cases))]
    public void EachProfileIsWhatSectionNinePointThreeSays(string label)
    {
        var spec = Specs.Single(s => s.Label == label);
        var parts = CwInterference.Render(spec, WantedWpm, CwInterference.Seed(WantedWpm), Nominal);
        var rate = parts.Audio.SampleRate;
        var failures = new List<string>();

        void Check(bool ok, string what)
        {
            _output.WriteLine($"{(ok ? "ok  " : "FAIL")} | {label} | {what}");

            if (!ok)
            {
                failures.Add(what);
            }
        }

        _output.WriteLine($"case | {label} | {spec.ProfileId} | wanted TX-ITU {WantedWpm:0} wpm at {CwInterference.WantedHz:0} Hz, {CwInterference.SnrDb:0} dB reference on {CwInterference.Channel} | seed {CwInterference.Seed(WantedWpm)} | mixer {(Nominal ? "NOMINAL (watched red)" : "CwInterference")}");

        foreach (var line in parts.Sidecar.Split('\n').Where(l => l.StartsWith("int", StringComparison.Ordinal)
            || l.StartsWith("layer", StringComparison.Ordinal) || l.StartsWith("headroom", StringComparison.Ordinal)
            || l.StartsWith("seed", StringComparison.Ordinal) || l.StartsWith("snr", StringComparison.Ordinal)))
        {
            _output.WriteLine($"sidecar | {label} | {line.TrimEnd()}");
        }

        // The key is the wanted text, and the wanted tone is measured like any other.
        Check(parts.Key == SyntheticCq.Text, $"wanted key `{parts.Key}` is SyntheticCq.Text, exact by construction");

        var wanted = Measure(parts.Wanted.Signal, rate);
        _output.WriteLine(string.Create(Invariant,
            $"measured | {label} | wanted | {wanted.Hz:0.000} Hz | key-down power {10 * Math.Log10(wanted.KeyDownPower):0.000} dB | key-ups {wanted.KeyUps} | swing {wanted.SwingDb:0.00} dB | first key-down {wanted.FirstOnSeconds:0.000} s"));

        var stations = new List<(double Hz, bool Keyed)>();

        foreach (var (layer, n) in parts.Layers.Select((l, n) => (l, n + 1)))
        {
            var x = layer.Interferer;
            var m = Measure(layer.Signal, rate);
            var offset = m.Hz - wanted.Hz;
            var level = 10 * Math.Log10(m.KeyDownPower / wanted.KeyDownPower);
            var keyed = m.KeyUps >= 5 && m.FloorRatio <= 0.01;
            var steady = m.SwingDb <= SteadyDb && m.KeyUps == 0;

            _output.WriteLine(string.Create(Invariant,
                $"measured | {label} | layer {n} | {m.Hz:0.000} Hz, offset {offset:+0.000;-0.000} Hz (stated {x.OffsetHz:+0.#;-0.#}) | level {level:+0.000;-0.000;0.000} dB (stated {x.LevelDb:+0.0;-0.0;0.0}) | key-ups {m.KeyUps}, floor {10 * Math.Log10(Math.Max(m.FloorRatio, 1e-30)):0.0} dB | swing {m.SwingDb:0.00} dB | first key-down {m.FirstOnSeconds:0.000} s"));

            Check(Math.Abs(offset - x.OffsetHz) <= OffsetToleranceHz,
                string.Create(Invariant, $"layer {n} offset {offset:+0.000;-0.000} Hz within {OffsetToleranceHz} Hz of {x.OffsetHz:+0.#;-0.#}"));
            Check(Math.Abs(level - x.LevelDb) <= LevelToleranceDb,
                string.Create(Invariant, $"layer {n} key-down power {level:+0.000;-0.000;0.000} dB within {LevelToleranceDb} dB of {x.LevelDb:+0.0;-0.0;0.0} against the wanted"));

            if (x.Keyed)
            {
                Check(keyed, string.Create(Invariant, $"layer {n} keys on and off: {m.KeyUps} key-ups (at least 5), floor {10 * Math.Log10(Math.Max(m.FloorRatio, 1e-30)):0.0} dB under key-down (at least 20)"));
            }
            else
            {
                Check(steady, string.Create(Invariant, $"layer {n} is steady from the first sample to the last: swing {m.SwingDb:0.000} dB (at most {SteadyDb}), {m.KeyUps} key-ups (none), first window at {m.FirstOnSeconds:0.000} s"));
            }

            stations.Add((m.Hz, x.Keyed && keyed));
        }

        if (spec.ProfileId == "INT-COCHAN")
        {
            // The co-channel profile was defined against the retired probabilistic decoder's 45 Hz Hann integrator, kept
            // here as the profile's own figure (work instruction 545).
            const double integratorBandwidthHz = 45.0;
            var half = integratorBandwidthHz / 2;
            var offset = stations[0].Hz - wanted.Hz;
            Check(Math.Abs(offset) < half && Math.Abs(offset) > OffsetToleranceHz,
                string.Create(Invariant, $"INT-COCHAN: the second station {offset:+0.000;-0.000} Hz from the wanted, inside the detector bandwidth (Hann, {integratorBandwidthHz:0} Hz, so within ±{half:0.0} Hz) and not on the wanted's own pitch"));
        }

        if (spec.ProfileId == "INT-PILEUP")
        {
            var inside = stations
                .Where(s => s.Keyed && s.Hz >= CwFixtureGenerator.PassbandLowHz && s.Hz <= CwFixtureGenerator.PassbandHighHz)
                .Select(s => s.Hz)
                .ToList();
            var distinct = inside.Where((hz, k) => Math.Abs(hz - wanted.Hz) > OffsetToleranceHz
                && inside.Take(k).All(o => Math.Abs(o - hz) > OffsetToleranceHz)).Count();

            Check(distinct >= 3,
                string.Create(Invariant, $"INT-PILEUP: {distinct} keyed stations inside the {CwFixtureGenerator.PassbandLowHz:0}-{CwFixtureGenerator.PassbandHighHz:0} Hz passband at distinct pitches besides the wanted (at least 3): {string.Join(", ", inside.Select(h => h.ToString("0.0", Invariant)))} Hz"));
        }

        // The mix is the band plus every tone, scaled by one factor, and nothing else.
        var worst = 0.0;
        var silent = 0;
        var noise = parts.Wanted.Noise;
        var window = (int)(WindowSeconds * rate);

        for (var i = 0; i < parts.Audio.Samples.Length; i++)
        {
            var sum = noise[i] + parts.Wanted.Signal[i] + parts.Layers.Sum(l => l.Signal[i]);
            worst = Math.Max(worst, Math.Abs(parts.Audio.Samples[i] - (sum * parts.Scale)));
        }

        for (var at = 0; at + window <= noise.Length; at += window)
        {
            var any = false;

            for (var i = at; i < at + window && !any; i++)
            {
                any = noise[i] != 0;
            }

            silent += any ? 0 : 1;
        }

        var peak = parts.Audio.Samples.Max(v => Math.Abs(v));
        Check(worst < 1e-6, string.Create(Invariant, $"the mix is band + wanted + every layer, x {parts.Scale:0.######}: worst sample {worst:0.0e0} from the sum"));
        Check(peak <= CwInterference.HeadroomPeak + 1e-6, string.Create(Invariant, $"no sample clipped: peak {peak:0.0000} FS"));
        Check(silent == 0, $"the band is under every 25 ms of the case, never digital silence (V-06): {silent} silent windows");

        _output.WriteLine($"verdict | {label} | {(failures.Count == 0 ? "is what 9.3 says" : "NOT what 9.3 says: " + string.Join("; ", failures))}");

        Assert.True(failures.Count == 0, $"{label}: {string.Join("; ", failures)}");
    }

    /// <summary>What one tone measures as, from its samples alone.</summary>
    /// <param name="Hz">Its pitch: upward zero crossings over time, inside its key-down runs.</param>
    /// <param name="KeyDownPower">The median power of the windows at key-down.</param>
    /// <param name="KeyUps">How often its power fell from key-down to a hundredth of it.</param>
    /// <param name="FloorRatio">Its quietest window over its key-down power.</param>
    /// <param name="SwingDb">Its loudest window over its quietest, whole case, in dB.</param>
    /// <param name="FirstOnSeconds">Where its first key-down window starts.</param>
    private readonly record struct Tone(double Hz, double KeyDownPower, int KeyUps, double FloorRatio, double SwingDb, double FirstOnSeconds);

    private static Tone Measure(double[] x, int rate)
    {
        var length = (int)Math.Round(WindowSeconds * rate);
        var hop = (int)Math.Round(HopSeconds * rate);
        var power = new List<double>();

        for (var at = 0; at + length <= x.Length; at += hop)
        {
            double sum = 0;

            for (var i = at; i < at + length; i++)
            {
                sum += x[i] * x[i];
            }

            power.Add(sum / length);
        }

        var loudest = power.Max();
        var on = power.Where(p => p >= 0.5 * loudest).OrderBy(p => p).ToList();
        var keyDown = on[on.Count / 2];

        // Key-ups with hysteresis: on at half the key-down power, off at a hundredth.
        var keyUps = 0;
        var state = false;
        var first = double.NaN;

        for (var k = 0; k < power.Count; k++)
        {
            if (!state && power[k] >= 0.5 * keyDown)
            {
                state = true;
                first = double.IsNaN(first) ? (double)k * hop / rate : first;
            }
            else if (state && power[k] <= 0.01 * keyDown)
            {
                state = false;
                keyUps++;
            }
        }

        // Pitch: upward crossings, interpolated, inside runs of samples whose window is at key-down.
        double cycles = 0, seconds = 0;
        var run = new List<double>();

        void Close()
        {
            if (run.Count >= 3)
            {
                cycles += run.Count - 1;
                seconds += run[^1] - run[0];
            }

            run.Clear();
        }

        for (var i = 1; i < x.Length; i++)
        {
            var k = Math.Clamp((i - (length / 2)) / hop, 0, power.Count - 1);

            if (power[k] < 0.9 * keyDown)
            {
                Close();
                continue;
            }

            if (x[i - 1] < 0 && x[i] >= 0)
            {
                run.Add((i - 1 + (-x[i - 1] / (x[i] - x[i - 1]))) / rate);
            }
        }

        Close();

        var quietest = power.Min();

        return new Tone(
            seconds > 0 ? cycles / seconds : double.NaN,
            keyDown,
            keyUps,
            quietest / keyDown,
            quietest > 0 ? 10 * Math.Log10(loudest / quietest) : double.PositiveInfinity,
            first);
    }
}
