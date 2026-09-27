using System.Globalization;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>
/// Each TX-* sender profile CW_SPEC.md section 10 gives numbers for is keyed the way
/// its definition states - TX-ITU, TX-KEYER-W, TX-FARNS, TX-TIGHT and TX-BUG - and
/// TX-STRAIGHT and TX-SLOPPY are refused by name (work instruction 468, task 1;
/// PHASE_PLAN.md 7.2; HM-REQ-050, which needs the four must-tier fists to exist).
/// </summary>
/// <remarks>
/// <para>**MEASURED FROM THE RENDERED TONE, BEFORE THE BAND IS ADDED, NEVER FROM THE
/// RECIPE** (CLAUDE.md 12.5). A mark is a run of samples the tone is keyed in; the
/// dit is the mean of the marks the text spells as dits; every length is then in
/// units of that measured dit. The code below was written for this test and shares
/// nothing with <c>CwUnitEstimator</c>, the lattice or any decoder timing code. The
/// text is used only to say which mark is a dit and which gap ends a word, which is
/// what a key is for.</para>
/// <para>**WATCHED FAILING FIRST.** With <c>HAMLET_UNIT468_SENDER=nominal</c> in the
/// environment every case is keyed 1:3:1:3:7 whatever its profile says, and every
/// produced profile but TX-ITU must go red.</para>
/// </remarks>
public sealed class TheSenderProfilesAreWhatTheySayTests
{
    /// <summary>The level every case is proved at: HM-REQ-050's 15 dB in the 2500 Hz reference.</summary>
    internal const double Snr = 15;

    /// <summary>The channel: CH-AWGN, one unfaded path over the shaped band.</summary>
    internal const string Channel = "CH-AWGN";

    /// <summary>The least a measured length may sit from its stated one, in units of the measured dit.</summary>
    /// <remarks>
    /// **THE SAMPLING SETS THE REST.** An edge between two samples is found to one
    /// sample, so a length of G units measured over a dit of D samples can be out by
    /// a sample at each of its two edges and the dit by a sample at each of its: 2 (1 + G) / D.
    /// At 25 WPM a word gap of 7.8 units may be out by 0.046, which the first green
    /// run showed as 7.790 against 7.812; the floor is 0.02 wherever that is smaller.
    /// </remarks>
    private const double Tolerance = 0.02;

    /// <summary>How far a measured speed may sit from its stated one, as a fraction.</summary>
    private const double SpeedTolerance = 0.005;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each check is printed.</param>
    public TheSenderProfilesAreWhatTheySayTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>Every produced profile at every speed, by id and speed index.</summary>
    public static TheoryData<string, int> Cases
    {
        get
        {
            var data = new TheoryData<string, int>();

            foreach (var p in CwSender.Profiles.Where(p => p.Produced))
            {
                for (var s = 0; s < CwSender.Speeds.Count; s++)
                {
                    data.Add(p.Id, s);
                }
            }

            return data;
        }
    }

    /// <summary>Whether this run keys every case 1:3:1:3:7, to watch the proof fail.</summary>
    private static bool Nominal
        => string.Equals(Environment.GetEnvironmentVariable("HAMLET_UNIT468_SENDER"), "nominal", StringComparison.Ordinal);

    /// <remarks>
    /// Proves 7.2's first condition: one row per section 10 profile in its order,
    /// the four must-tier profiles HM-REQ-050 names produced, TX-BUG produced, and
    /// TX-STRAIGHT and TX-SLOPPY refused by name with the reason.
    /// </remarks>
    [Fact]
    public void EverySectionTenProfileIsProducedOrRefusedByName()
    {
        foreach (var p in CwSender.Profiles)
        {
            _output.WriteLine($"profile | {p.Id} | {p.Tier} | {(p.Produced ? "produced" : "refused: " + p.Refused)}");
        }

        Assert.Equal(
            new[] { "TX-ITU", "TX-KEYER-W", "TX-FARNS", "TX-TIGHT", "TX-BUG", "TX-STRAIGHT", "TX-SLOPPY" },
            CwSender.Profiles.Select(p => p.Id));

        foreach (var id in new[] { "TX-ITU", "TX-KEYER-W", "TX-FARNS", "TX-TIGHT" })
        {
            Assert.Equal("must", CwSender.Profile(id).Tier);
            Assert.True(CwSender.Profile(id).Produced, id);
        }

        Assert.True(CwSender.Profile("TX-BUG").Produced);

        foreach (var id in new[] { "TX-STRAIGHT", "TX-SLOPPY" })
        {
            var refused = Assert.Throws<NotSupportedException>(() => CwSender.Profile(id));
            Assert.Contains(id + " is refused: section 10 gives no number", refused.Message, StringComparison.Ordinal);
        }
    }

    /// <remarks>
    /// Proves 7.2's second condition for one produced profile at one speed: the
    /// dah-to-dit ratio, the element, character and word gaps in units of the
    /// measured dit and, for TX-FARNS, the character speed against the overall
    /// speed, each measured from the rendered tone and held against the
    /// definition's number or range and against the case's own drawn values; every
    /// mark and gap of a kind alike; the key the text; the band under the keying.
    /// </remarks>
    /// <param name="id">The profile.</param>
    /// <param name="speedIndex">Into <see cref="CwSender.Speeds"/>.</param>
    [Theory]
    [MemberData(nameof(Cases))]
    public void EachProducedProfileIsKeyedAsItsDefinitionStates(string id, int speedIndex)
    {
        var wpm = CwSender.Speeds[speedIndex];
        var seed = CwSender.Seed(id, speedIndex);
        var timing = CwSender.Timing(id, wpm, seed);
        var text = SyntheticCq.Text;

        var parts = Nominal
            ? CwSender.Keyed(timing, SyntheticCq.Recipe(wpm, 0, seed) with { Text = text, ToneHz = CwSender.PitchHz, DriftHz = 0 }, Channel, Snr)
            : CwSender.Render(id, Channel, Snr, text, wpm, seed).Parts;

        var checks = Checks(id, timing, text, parts);

        foreach (var c in checks)
        {
            _output.WriteLine(string.Create(CultureInfo.InvariantCulture,
                $"proof | {id} | {wpm:0} wpm | seed {seed} | {(Nominal ? "nominal sender" : "CwSender")} | {c.What} | {c.Measured} | {c.Stated} | {(c.Pass ? "pass" : "FAIL")}"));
        }

        _output.WriteLine($"sidecar | {id} | {wpm:0} wpm");

        foreach (var line in parts.Sidecar.Split('\n'))
        {
            _output.WriteLine("sidecar | " + line.TrimEnd('\r'));
        }

        var failed = checks.Where(c => !c.Pass).Select(c => c.What).ToList();
        Assert.True(failed.Count == 0, $"{id} at {wpm} wpm is not keyed as section 10 states: {string.Join("; ", failed)}");
    }

    /// <summary>One check: what, what was measured, what it is held against, and whether it holds.</summary>
    private sealed record Check(string What, string Measured, string Stated, bool Pass);

    private static List<Check> Checks(string id, CwSenderTiming timing, string text, CwChannelParts parts)
    {
        var i = CultureInfo.InvariantCulture;
        var checks = new List<Check>();
        var rate = parts.Audio.SampleRate;

        // The key is the text, and the sidecar carries what rebuilds the case.
        checks.Add(new Check("key is the text", parts.Key, text, parts.Key == text));

        foreach (var needle in new[] { "txProfile     " + id, "txSeed        " + timing.Seed.ToString(i), "txSnr         15.0 dB", "txSpeed       " }
                     .Concat(timing.Parameters.Select(p => "txParameter   " + p.Name)))
        {
            checks.Add(new Check("sidecar carries " + needle.Trim(), parts.Sidecar.Contains(needle, StringComparison.Ordinal) ? "yes" : "no", "yes",
                parts.Sidecar.Contains(needle, StringComparison.Ordinal)));
        }

        // The band is under the keying: no sample of the output is digital silence (V-06).
        var silent = parts.Audio.Samples.Count(s => s == 0f);
        checks.Add(new Check("digital-silence samples in the output", silent.ToString(i), "0", silent == 0));

        var runs = Runs(parts.Signal, rate);
        var expected = Spelled(text);

        checks.Add(new Check("marks keyed", runs.Count.ToString(i), expected.Marks.Count.ToString(i), runs.Count == expected.Marks.Count));

        if (runs.Count != expected.Marks.Count)
        {
            return checks;
        }

        var ms = 1000.0 / rate;
        var marks = runs.Select(r => (r.End - r.Start) * ms).ToList();
        var gaps = runs.Zip(runs.Skip(1), (a, b) => (b.Start - a.End) * ms).ToList();

        var dits = marks.Where((_, k) => !expected.Marks[k]).ToList();
        var dahs = marks.Where((_, k) => expected.Marks[k]).ToList();
        var dit = dits.Average();

        List<double> Gaps(char kind) => gaps.Where((_, k) => expected.Gaps[k] == kind).Select(g => g / dit).ToList();

        var ratio = dahs.Average() / dit;
        var element = Gaps('e');
        var character = Gaps('c');
        var word = Gaps('w');

        string U(double v) => v.ToString("0.0000", i);

        var ditSamples = dit * rate / 1000.0;
        double Tol(double units) => Math.Max(Tolerance, 2 * (1 + units) / ditSamples);

        void Near(string what, double measured, double stated, string source)
            => checks.Add(new Check(what, U(measured), $"{U(stated)} +- {U(Tol(stated))} ({source})", Math.Abs(measured - stated) <= Tol(stated)));

        void Within(string what, double measured, double low, double high, string source)
            => checks.Add(new Check(what, U(measured), $"{U(low)} to {U(high)}, +- {U(Tol(high))} ({source})",
                measured >= low - Tol(low) && measured <= high + Tol(high)));

        void Alike(string what, IReadOnlyList<double> units)
        {
            var spread = units.Count == 0 ? 0 : units.Max() - units.Min();
            var allowed = units.Count == 0 ? Tolerance : Tol(units.Max());
            checks.Add(new Check(what + " alike (max - min)", U(spread), $"at most {U(allowed)}", spread <= allowed));
        }

        // The speed: the measured dit against the case's.
        var measuredWpm = 1200.0 / dit;
        checks.Add(new Check("character speed, wpm", measuredWpm.ToString("0.000", i), timing.CharacterWpm.ToString("0.000", i),
            Math.Abs(measuredWpm - timing.CharacterWpm) <= SpeedTolerance * timing.CharacterWpm));

        Alike("dits", dits.Select(d => d / dit).ToList());
        Alike("dahs", dahs.Select(d => d / dit).ToList());
        Alike("element gaps", element);
        Alike("character gaps", character);
        Alike("word gaps", word);

        // Against the case's own timing: what the sidecar says was keyed.
        Near("dah / dit", ratio, timing.Dah, "this case's timing");
        Near("element gap, units", element.Average(), timing.ElementGap, "this case's timing");
        Near("character gap, units", character.Average(), timing.CharacterGap, "this case's timing");
        Near("word gap, units", word.Average(), timing.WordGap, "this case's timing");

        // The overall speed: the text's span at 1:3:1:3:7 over its measured span, both in measured dits.
        var span = (runs[^1].End - runs[0].Start) * ms / dit;
        var (m, d, e, c, w) = CwSender.Counts(text);
        var nominalSpan = m + (2 * d) + e + (3 * c) + (7 * w);
        var overall = measuredWpm * nominalSpan / span;
        var statedOverall = timing.OverallWpm(text);

        checks.Add(new Check("overall speed, wpm", overall.ToString("0.000", i), statedOverall.ToString("0.000", i) + " (this case's timing)",
            Math.Abs(overall - statedOverall) <= SpeedTolerance * statedOverall));

        // Against section 10's definition.
        switch (id)
        {
            case "TX-ITU":
                Near("TX-ITU dah / dit", ratio, 3, "section 10, exactly 1:3:1:3:7");
                Near("TX-ITU element gap", element.Average(), 1, "section 10");
                Near("TX-ITU character gap", character.Average(), 3, "section 10");
                Near("TX-ITU word gap", word.Average(), 7, "section 10");
                break;

            case "TX-KEYER-W":
                Within("TX-KEYER-W dah / dit", ratio, 2.5, 3.5, "section 10, ratio 2.5-3.5");
                Within("TX-KEYER-W element gap", element.Average(), 0.7, 1.3, "section 10, gaps +-30 %");
                Within("TX-KEYER-W character gap", character.Average(), 2.1, 3.9, "section 10, gaps +-30 %");
                Within("TX-KEYER-W word gap", word.Average(), 4.9, 9.1, "section 10, gaps +-30 %");
                break;

            case "TX-FARNS":
                Within("TX-FARNS character gap, units at character speed", character.Average(), 3, 7, "section 10, 3-7 units");
                checks.Add(new Check("TX-FARNS word gap longer than the character gap", U(word.Average()), "above " + U(character.Average()) + " (section 10)",
                    word.Average() > character.Average() + Tolerance));
                checks.Add(new Check("TX-FARNS character speed above overall speed", $"{measuredWpm:0.000} against {overall:0.000}", "character above overall (section 10)",
                    measuredWpm > overall * (1 + SpeedTolerance)));
                break;

            case "TX-TIGHT":
                checks.Add(new Check("TX-TIGHT element gaps shorter than the dit", U(element.Average()), "below 1 (section 10)", element.Average() < 1 - Tolerance));
                checks.Add(new Check("TX-TIGHT character gaps compressed", U(character.Average()), "below 3 (section 10)", character.Average() < 3 - Tolerance));
                Near("TX-TIGHT dah / dit", ratio, 283.0 / 105, "013347 as HM-DEC-101 fitted it, 283 / 105");
                Near("TX-TIGHT element gap", element.Average(), 65.0 / 105, "013347, 65 / 105");
                Near("TX-TIGHT character gap", character.Average(), 130.0 / 105, "013347, 130 / 105");
                Near("TX-TIGHT word gap", word.Average(), 280.0 / 105, "013347, 280 / 105");
                break;

            case "TX-BUG":
                Within("TX-BUG dah / dit", ratio, 3.5, 5, "section 10, ratio 3.5-5");
                break;

            default:
                checks.Add(new Check("profile has a definition check", "no", "yes", false));
                break;
        }

        return checks;
    }

    /// <summary>
    /// The keyed runs of a tone with no noise under it: every sample the tone is not
    /// nought belongs to a mark, and a run of noughts shorter than two milliseconds is
    /// the tone crossing zero inside one.
    /// </summary>
    /// <remarks>
    /// **A RUN STARTS AT THE LAST NOUGHT BEFORE ITS FIRST KEYED SAMPLE.** The raised
    /// cosine is exactly nought at the instant the key goes down, so where an edge
    /// falls on a sample that sample is silent: counting keyed samples alone reads
    /// every mark one sample short and every gap one long, which the red run showed
    /// as TX-ITU's word gap at 7.021 units at 25 WPM. From the silent sample, an edge
    /// on the grid is measured exactly and one between samples at most one sample early.
    /// </remarks>
    private static List<(int Start, int End)> Runs(double[] tone, int rate)
    {
        var floor = tone.Max(Math.Abs) * 1e-9;
        var bridge = (int)Math.Round(0.002 * rate);
        var runs = new List<(int Start, int End)>();
        var start = -1;
        var last = -1;

        for (var n = 0; n < tone.Length; n++)
        {
            if (Math.Abs(tone[n]) <= floor)
            {
                continue;
            }

            if (start < 0)
            {
                start = n - 1;
            }
            else if (n - last > bridge)
            {
                runs.Add((start, last + 1));
                start = n - 1;
            }

            last = n;
        }

        if (start >= 0)
        {
            runs.Add((start, last + 1));
        }

        return runs;
    }

    /// <summary>What the text spells: each mark a dah or not, and each gap between marks e, c or w.</summary>
    private static (List<bool> Marks, List<char> Gaps) Spelled(string text)
    {
        var marks = new List<bool>();
        var gaps = new List<char>();
        var firstWord = true;

        foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var firstCharacter = true;
            var joining = false;

            foreach (var ch in word)
            {
                if (ch == '^')
                {
                    joining = true;
                    continue;
                }

                var pattern = MorseCode.Spell(ch) ?? "";

                for (var k = 0; k < pattern.Length; k++)
                {
                    if (marks.Count > 0)
                    {
                        gaps.Add(k > 0 ? 'e' : firstCharacter && !firstWord ? 'w' : joining ? 'e' : 'c');
                    }

                    marks.Add(pattern[k] != '.');
                }

                firstCharacter = false;
            }

            firstWord = false;
        }

        return (marks, gaps);
    }
}
