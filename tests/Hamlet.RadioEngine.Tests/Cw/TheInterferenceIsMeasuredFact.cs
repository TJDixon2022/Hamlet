using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// HM-REQ-060, 061, 062 and 066 measured on their verification rows' conditions, over the
/// wanted TX-ITU station at 15 dB reference on CH-AWGN, judged through HM-REQ-010 and
/// HM-REQ-011 (work instruction 469, task 2; PHASE_PLAN.md 7.3).
/// </summary>
/// <remarks>
/// <para>**A PRINTER. IT ASSERTS NOTHING ABOUT THE NUMBERS.** A requirement not met is a
/// finding for the decoder steps, and 7.3 asks for the measurement (V-08). It asserts only
/// that every case and every control was generated and read.</para>
/// <para>**OURS ALONE IS THE NUMBER**, read as <see cref="TheRequirementsAreMeasuredTests"/>
/// reads the synthetic set - <see cref="CwDecoder"/> from 600 Hz hop by hop, flushed - and
/// scored through the same <see cref="TheRequirementsAreMeasuredTests.Measure"/> and
/// <see cref="CwMetrics"/> calls, against the wanted key alone. The tracked pitch and the
/// report's competitor are read after every hop. Beside ours, for reading only: the
/// transcript the live path emits under <see cref="CwSwitchTable.Live"/> as it stands, and
/// the port alone. No case is a switch row and nothing is switched.</para>
/// <para>**EVERY KEY IS EXACT BY CONSTRUCTION** (R61, V-13): <see cref="SyntheticCq.Text"/>
/// keyed by <see cref="CwInterference"/> at the seeds task 1 fixed. An interferer's text is
/// never a key and never a decoder input (R72).</para>
/// <para>**THE READINGS, EACH THE ARBITER'S AND OVERRULABLE** (work instruction 469 DECIDED
/// (5), (6)): held is every tracked pitch at a wanted word's last key-up within 25 Hz of the
/// wanted's; the keyed station is selected if every tracked pitch from the first sure
/// character to the wanted's last key-up is within 25 Hz of the wanted's; a competitor is
/// reported if at any hop from the second station's first mark to the wanted's last key-up
/// the report names one within 25 Hz of the second station. HM-REQ-010 with nothing sure
/// emitted has no rate to be below 1 %, and is counted not met.</para>
/// </remarks>
public sealed class TheInterferenceIsMeasuredFact
{
    private const double HeldHz = 25;

    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private static readonly string[] Outputs = { "ours", "emitted (live, switch as it stands)", "port alone" };

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fact.</summary>
    /// <param name="output">Where the table is printed.</param>
    public TheInterferenceIsMeasuredFact(ITestOutputHelper output)
        => _output = output;

    /// <summary>One output of one case, scored against the wanted key.</summary>
    private sealed record Scored(
        CwCoverage Coverage, CwSureErrors Sure, CwBoundaryErrors Wbe, CwInvented Invented, string Text)
    {
        public bool Met010 => Sure.SureEmitted > 0 && Sure.Errors < 0.01 * Sure.SureEmitted;

        public bool Met011 => Invented.Count == 0;
    }

    /// <summary>What ours did through one case, hop by hop.</summary>
    private sealed record Run(
        IReadOnlyList<CwCharacter> Settled, double[] Pitch, CwCompetitor?[] Competitor, int Hop, int Rate, CwDecodeReport End)
    {
        public double PitchAt(double seconds)
            => Pitch[Math.Clamp((int)Math.Floor(seconds * Rate / Hop) - 1, 0, Pitch.Length - 1)];

        public IEnumerable<int> Hops(double from, double to)
        {
            var first = Math.Max(0, (int)Math.Floor(from * Rate / Hop));
            var last = Math.Min(Pitch.Length - 1, (int)Math.Floor(to * Rate / Hop) - 1);

            for (var k = first; k <= last; k++)
            {
                yield return k;
            }
        }
    }

    /// <remarks>
    /// Proves 7.3's fourth condition: HM-REQ-060 (INT-ADJ(±100, 0, 25) at 12, 18 and 25 WPM),
    /// HM-REQ-061 (INT-ADJ(±50, −6, 25), the same), HM-REQ-062 (INT-CARRIER at +50, +100,
    /// +200 and +500 Hz by 0, +10 and +20 dB at 18 WPM) and HM-REQ-066 (a second TX-ITU
    /// station at +100 Hz, 25 WPM, at 0, −2.4 and −6 dB, at 18 WPM) each measured, with
    /// HM-REQ-010's MET-CER-SURE and HM-REQ-011's MET-INVENTED on the wanted key, the held
    /// pitch, the station selected and the competitor the tree reports, met or not per case
    /// and per requirement, each beside its control. Asserts only that all 27 cases and 3
    /// controls were read.
    /// </remarks>
    [Fact]
    public void EachRequirementOnItsVerificationRowsCondition()
    {
        var read = 0;
        var controls = new Dictionary<double, (Run Run, Scored Scored)>();

        _output.WriteLine($"key | {SyntheticCq.Text} | synthetic, exact | wanted TX-ITU at {CwInterference.WantedHz:0} Hz, {CwInterference.SnrDb:0} dB in the 2500 Hz reference on {CwInterference.Channel}");
        _output.WriteLine($"readings | held: every tracked pitch at a wanted word's end within {HeldHz:0} Hz of the wanted | selected: every tracked pitch from the first sure character to the wanted's last key-up within {HeldHz:0} Hz of the wanted | competing: the report's Competitor at any hop from the second station's first mark to the wanted's last key-up, within {HeldHz:0} Hz of the second station | HM-REQ-010 with nothing sure is not met");

        foreach (var wpm in SyntheticCq.Speeds)
        {
            var parts = CwInterference.Render(null, wpm, CwInterference.Seed(wpm));
            var run = Settle(parts.Audio);
            var scored = Score($"control-{wpm:0}wpm", run.Settled);

            Assert.Equal(SyntheticCq.Text, parts.Key);
            controls[wpm] = (run, scored);
            read++;

            _output.WriteLine(string.Create(Invariant,
                $"control | {wpm:0} wpm | seed {CwInterference.Seed(wpm)} | the wanted alone | {Line(scored)} | pitch at word ends {Pitches(run, parts)} | competing {Quote(run.End.Competitor)} | read `{scored.Text}` | synthetic, exact"));
        }

        var requirements = new (string Id, string Row, IEnumerable<(CwInterferenceSpec Spec, double Wpm)> Cases)[]
        {
            ("HM-REQ-060", "INT-ADJ(100, 0, 25) | 010, 011 on wanted | held",
                SyntheticCq.Speeds.SelectMany(w => new[] { 100.0, -100 }.Select(f => (CwInterference.Adjacent(f, 0, 25), w)))),
            ("HM-REQ-061", "INT-ADJ(50, -6, 25) | 010, 011 on wanted | held",
                SyntheticCq.Speeds.SelectMany(w => new[] { 50.0, -50 }.Select(f => (CwInterference.Adjacent(f, -6, 25), w)))),
            ("HM-REQ-062", "INT-CARRIER at 0, +10, +20 dB, offsets 50-500 Hz | station selected | keyed station",
                new[] { 50.0, 100, 200, 500 }.SelectMany(f => new[] { 0.0, 10, 20 }.Select(db => (CwInterference.Carrier(f, db), 18.0)))),
            ("HM-REQ-066", "second signal within veto margin (6 dB, DECIDED 6) | competing reported | yes",
                new[] { 0.0, -2.4, -6 }.Select(db => (CwInterference.Adjacent(100, db, 25), 18.0))),
        };

        foreach (var (id, row, cases) in requirements)
        {
            var met = 0;
            var all = 0;

            _output.WriteLine($"requirement | {id} | row: {row}");

            foreach (var (spec, wpm) in cases)
            {
                var seed = CwInterference.Seed(wpm);
                var parts = CwInterference.Render(spec, wpm, seed);
                var name = string.Create(Invariant, $"{spec.Label}-{wpm:0}wpm");
                var run = Settle(parts.Audio);
                var ours = Score(name, run.Settled);
                var (emitted, _, readings) = TheArbitrationEarnsItsPlaceFact.Drive(
                    parts.Audio.Samples, parts.Audio.SampleRate, SyntheticCq.StartingPitchHz, true, CwSwitchTable.Live);
                var port = TheArbitrationEarnsItsPlaceFact.PortAlone(readings);
                var control = controls[wpm];
                var layer = parts.Layers[0];
                var lastKeyUp = parts.WordEnds[^1];

                Assert.Equal(SyntheticCq.Text, parts.Key);
                read++;
                all++;

                var held = parts.WordEnds.All(t => Math.Abs(run.PitchAt(t) - CwInterference.WantedHz) <= HeldHz);
                string verdict;
                bool caseMet;

                switch (id)
                {
                    case "HM-REQ-062":
                    {
                        var firstSure = run.Settled.FirstOrDefault(c => !c.IsWordGap && c.Confidence == CwConfidence.High);
                        var hops = firstSure is null ? new List<int>() : run.Hops(firstSure.At.TotalSeconds, lastKeyUp).ToList();
                        var off = hops.Where(k => Math.Abs(run.Pitch[k] - CwInterference.WantedHz) > HeldHz).ToList();
                        var worst = hops.Count == 0 ? double.NaN : hops.Select(k => run.Pitch[k]).MaxBy(p => Math.Abs(p - CwInterference.WantedHz));

                        caseMet = firstSure is not null && off.Count == 0;
                        verdict = firstSure is null
                            ? "station selected: not decided - no sure character, counted not met"
                            : string.Create(Invariant, $"station selected: {(caseMet ? "the keyed station" : "THE CARRIER")} - {off.Count} of {hops.Count} hops from the first sure character ({firstSure.At.TotalSeconds:0.00} s) to the wanted's last key-up ({lastKeyUp:0.00} s) more than {HeldHz:0} Hz off, furthest {worst:0.0} Hz");
                        break;
                    }

                    case "HM-REQ-066":
                    {
                        var hops = run.Hops(layer.StartSeconds, lastKeyUp).ToList();
                        var named = hops.Where(k => run.Competitor[k] is { } c && Math.Abs(c.ToneHz - layer.PitchHz) <= HeldHz).ToList();
                        var any = hops.Where(k => run.Competitor[k] is not null).ToList();
                        var seen = any.Select(k => Quote(run.Competitor[k])).Distinct().Take(4).ToList();

                        caseMet = named.Count > 0;
                        verdict = string.Create(Invariant,
                            $"competing reported: {(caseMet ? "yes" : "NO")} - the second station at {layer.PitchHz:0} Hz named on {named.Count} of {hops.Count} hops from its first mark ({layer.StartSeconds:0.00} s) to the wanted's last key-up; any competitor on {any.Count} hops{(seen.Count > 0 ? ": " + string.Join(" / ", seen) : "")}; at the end of the case the report's Competitor is {Quote(run.End.Competitor)}");
                        break;
                    }

                    default:
                        caseMet = held && ours.Met010 && ours.Met011;
                        verdict = $"held {(held ? "yes" : "NO")}, HM-REQ-010 {(ours.Met010 ? "met" : "NOT MET")}, HM-REQ-011 {(ours.Met011 ? "met" : "NOT MET")}";
                        break;
                }

                met += caseMet ? 1 : 0;

                _output.WriteLine(string.Create(Invariant,
                    $"case | {id} | {spec.Label} | wanted {wpm:0} wpm | seed {seed} | headroom x {parts.Scale:0.####} | {Line(ours)} | {verdict} | {(caseMet ? "met" : "NOT MET")} | synthetic, exact"));
                _output.WriteLine($"  key       `{SyntheticCq.Text}`");
                _output.WriteLine($"  ours      `{ours.Text}`");
                _output.WriteLine($"  control   `{control.Scored.Text}` | {Line(control.Scored)}");
                _output.WriteLine(string.Create(Invariant,
                    $"  pitch     wanted {CwInterference.WantedHz:0} Hz, interferer {layer.PitchHz:0} Hz{(parts.Layers.Count > 1 ? " and more" : "")} | tracked at the wanted's word ends {Pitches(run, parts)} | control {Pitches(control.Run, parts)}"));
                _output.WriteLine($"  competing at the end: {Quote(run.End.Competitor)} | interference at the end: {Interference(run.End)}");
                _output.WriteLine($"  interferer `{layer.Text}` - in the sidecar, never scored");

                foreach (var (o, x) in new[] { (Outputs[1], Score(name, emitted)), (Outputs[2], Score(name, port)) })
                {
                    _output.WriteLine($"  {o} | {Line(x)} | read `{x.Text}`");
                }
            }

            _output.WriteLine($"requirement | {id} | ours | {(met == all ? "met" : "NOT MET")} | {met} of {all} cases met; met only if all are | synthetic, exact, CH-AWGN 15 dB on the wanted");
        }

        Assert.Equal(27 + 3, read);
    }

    private static string Line(Scored x)
        => string.Create(Invariant,
            $"MET-CER-SURE {x.Sure.Errors}/{x.Sure.SureEmitted} {Share(x.Sure.Errors, x.Sure.SureEmitted)} ({x.Sure.SureWrong} substituted, {x.Sure.SureAdded} added) | "
            + $"MET-INVENTED {x.Invented.Count}/{x.Invented.Sent} | "
            + $"sure and right {x.Coverage.SureRight}/{x.Coverage.Sent} {Share(x.Coverage.SureRight, x.Coverage.Sent)} | "
            + $"MET-WBE {x.Wbe.Errors}/{x.Wbe.WordsSent} {Share(x.Wbe.Errors, x.Wbe.WordsSent)}");

    private static string Pitches(Run run, CwInterferenceParts parts)
        => string.Join(" ", parts.WordEnds.Select(t => run.PitchAt(t).ToString("0", Invariant)));

    // As the app's sidecar writes it (MainWindowViewModel.CompetitorForTheRecord), or null.
    private static string Quote(CwCompetitor? c)
        => c is { } other
            ? string.Create(Invariant, $"`{Math.Abs(other.OffsetHz):0} Hz {other.Side} at {other.RelativeDb:+0.0;-0.0} dB relative ({other.ToneHz:0} Hz)`")
            : "null (the app writes `none admitted ...` or `none found ...`)";

    private static string Interference(CwDecodeReport report)
        => report.Interference is { } t
            ? string.Create(Invariant, $"a tone at {t.ToneHz:0} Hz, {t.LiftDb:+0.0;-0.0} dB over the band floor, present {t.PresentFraction * 100:0}%")
            : "none";

    private static string Share(int count, int of)
        => of == 0 ? "(nothing to divide by)" : (count / (double)of).ToString("0.00", Invariant);

    private static Scored Score(string name, IReadOnlyList<CwCharacter> settled)
    {
        var m = TheRequirementsAreMeasuredTests.Measure(name, "synthetic", "INT-* over TX-ITU at 15 dB reference on CH-AWGN", CwKeyKind.Exact,
            settled, TheArbitrationEarnsItsPlaceFact.ScoresFor(name, false, settled));
        var stretches = m.NotComputable is null ? m.Stretches : Array.Empty<CwMetricAlignment>();

        var c = stretches.Select(CwMetrics.Coverage).ToList();
        var e = stretches.Select(CwMetrics.SureErrors).ToList();
        var w = stretches.Select(CwMetrics.WordBoundaries).ToList();
        var n = stretches.Select(CwMetrics.Invented).ToList();

        // Nothing scored is nothing read: every sent character missed, every word boundary with it.
        var sent = SyntheticCq.Text.Count(ch => ch != ' ');
        var words = SyntheticCq.Text.Split(' ').Length;

        return new Scored(
            c.Count == 0 ? new CwCoverage(0, 0, sent, CwKeyKind.Exact) : new CwCoverage(c.Sum(x => x.SureEmitted), c.Sum(x => x.SureRight), c.Sum(x => x.Sent), CwKeyKind.Exact),
            new CwSureErrors(e.Sum(x => x.SureWrong), e.Sum(x => x.SureAdded), e.Sum(x => x.SureEmitted), CwKeyKind.Exact),
            w.Count == 0 ? new CwBoundaryErrors(0, words - 1, words, CwKeyKind.Exact) : new CwBoundaryErrors(w.Sum(x => x.Inserted), w.Sum(x => x.Deleted), w.Sum(x => x.WordsSent), CwKeyKind.Exact),
            new CwInvented(n.Sum(x => x.SureAdded), n.Sum(x => x.SureWrong), c.Count == 0 ? sent : n.Sum(x => x.Sent), CwKeyKind.Exact),
            CwReading.Of(settled).Text.Trim());
    }

    // TheRequirementsAreMeasuredTests' own read - CwDecoder from 600 Hz, hop by hop, flushed -
    // with the tracked pitch and the report's competitor kept after every hop.
    private static Run Settle(MonoAudio audio)
    {
        var decoder = new CwDecoder(audio.SampleRate, SyntheticCq.StartingPitchHz);
        var settled = new List<CwCharacter>();
        var pitch = new List<double>();
        var competitor = new List<CwCompetitor?>();

        decoder.CharacterSettled += settled.Add;

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
            pitch.Add(decoder.Tracker.ToneHz);
            competitor.Add(decoder.Tracker.Competitor);
        }

        decoder.Flush();

        return new Run(settled, pitch.ToArray(), competitor.ToArray(), hop, audio.SampleRate, decoder.Report);
    }
}
