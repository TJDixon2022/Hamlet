using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;
using Row = Hamlet.RadioEngine.Tests.Cw.WhatEachDecoderKnowsAboutEachCharacterFact.CharacterRow;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Measures HM-REQ-124 per decoder and per condition over the keyed corpus,
/// held-out by recording, and writes `docs/phase-requirements/calibration.md`
/// (work instruction 464, task 3; PHASE_PLAN.md 9.5).
/// </summary>
/// <remarks>
/// <para>**THE VERDICT IS HELD-OUT** (464 DECIDED (3), V-13, CLAUDE.md 12.5): for
/// each of the 35 recordings and cases, the map is fitted on the other 34 and the
/// held-out one's characters take their p from that fit. The in-sample figure, from
/// the whole-pool fit the decoders ship, is printed beside it and judges
/// nothing.</para>
/// <para>**ITS ASSERTIONS ARE ABOUT THE HARNESS, NEVER ABOUT A VERDICT**: that the
/// constants shipped are the whole pool's fit, that the p each decoder attaches is
/// the p measured here, and that the file was written. A decoder calibrated nowhere
/// is a finding, not a failure.</para>
/// </remarks>
public sealed class EachDecodersConfidenceIsMeasuredTests
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private static readonly Lazy<Measurement> Measured = new(Measure);

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tables are printed.</param>
    public EachDecodersConfidenceIsMeasuredTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>One decoder's map, as fitted over some pool.</summary>
    /// <param name="Decoder">ours or port.</param>
    /// <param name="Beta">The intercept, then one coefficient per feature.</param>
    /// <param name="NotFinite">The constant for a character whose features are not finite.</param>
    /// <param name="Fitted">Scored characters the logistic was fitted on.</param>
    /// <param name="NonFinite">Scored characters the constant was fitted on; nought where it fell back to the pool's share right.</param>
    internal sealed record DecoderMap(string Decoder, double[] Beta, double NotFinite, int Fitted, int NonFinite);

    /// <summary>Every scored character with its held-out and in-sample p.</summary>
    /// <param name="Whole">Each decoder's whole-pool map, the one shipped.</param>
    /// <param name="Scored">Every scored character of both decoders.</param>
    /// <param name="HeldOut">Its p from the map fitted without its recording.</param>
    /// <param name="InSample">Its p from the whole-pool map.</param>
    internal sealed record Measurement(
        IReadOnlyDictionary<string, DecoderMap> Whole, IReadOnlyList<Row> Scored,
        IReadOnlyDictionary<Row, double> HeldOut, IReadOnlyDictionary<Row, double> InSample);

    /// <summary>The map's features for a row, or null where they are not finite.</summary>
    internal static double[]? Features(Row r)
    {
        if (r.Decoder == "ours")
        {
            return double.IsFinite(r.MarginLlr) ? new[] { CwCharacterProbability.Feature(r.MarginLlr) } : null;
        }

        return double.IsFinite(r.LevelDb) && double.IsFinite(r.TimingDits) ? new[] { r.LevelDb, r.TimingDits } : null;
    }

    /// <summary>Fits one decoder's map over the scored rows given (464 task 1's form).</summary>
    internal static DecoderMap FitMap(string decoder, IEnumerable<Row> rows)
    {
        var scored = rows.Where(r => r.Decoder == decoder && r.Scored).ToList();
        var finite = scored.Select(r => (X: Features(r), r.Right)).Where(p => p.X is not null).Select(p => (p.X!, p.Right)).ToList();
        var nonFinite = scored.Where(r => Features(r) is null).ToList();
        var constant = nonFinite.Count > 0
            ? nonFinite.Count(r => r.Right) / (double)nonFinite.Count
            : scored.Count(r => r.Right) / (double)scored.Count;

        return new DecoderMap(decoder, CwCalibration.Fit(finite), constant, finite.Count, nonFinite.Count);
    }

    /// <summary>A row's p under a map.</summary>
    internal static double P(DecoderMap map, Row r)
        => Features(r) is { } x ? CwCalibration.Logistic(map.Beta, x) : map.NotFinite;

    /// <summary>The conditions, parity.md section 3's rows in its order.</summary>
    internal static IReadOnlyList<(string Label, string Key, Func<Row, bool> Holds)> Conditions(IReadOnlyList<Row> rows)
    {
        var list = new List<(string, string, Func<Row, bool>)>();

        foreach (var real in new[] { true, false })
        {
            list.Add((real ? "real HF, all" : "synthetic, all", real ? "inferred" : "exact", r => r.Real == real));

            foreach (var c in rows.Where(r => r.Real == real).Select(r => r.Condition).Distinct().OrderBy(c => c, StringComparer.Ordinal))
            {
                list.Add((c, real ? "inferred" : "exact", r => r.Condition == c));
            }
        }

        return list;
    }

    private static Measurement Measure()
    {
        var all = WhatEachDecoderKnowsAboutEachCharacterFact.Characters;
        var scored = all.Where(r => r.Scored).ToList();
        var whole = new[] { "ours", "port" }.ToDictionary(d => d, d => FitMap(d, scored));
        var heldOut = new Dictionary<Row, double>(ReferenceEqualityComparer.Instance);
        var inSample = new Dictionary<Row, double>(ReferenceEqualityComparer.Instance);

        foreach (var decoder in whole.Keys)
        {
            foreach (var recording in scored.Where(r => r.Decoder == decoder).Select(r => r.Recording).Distinct())
            {
                var map = FitMap(decoder, scored.Where(r => r.Recording != recording));

                foreach (var r in scored.Where(r => r.Decoder == decoder && r.Recording == recording))
                {
                    heldOut[r] = P(map, r);
                    inSample[r] = P(whole[decoder], r);
                }
            }
        }

        return new Measurement(whole, scored, heldOut, inSample);
    }

    /// <remarks>
    /// HM-REQ-124, the harness: prints each decoder's whole-pool fit, and asserts
    /// that the constants <see cref="CwCharacterProbability"/> and
    /// <see cref="FldigiConfidence"/> ship are that fit, that every character of
    /// ours carries <see cref="CwCharacter.Probability"/> equal to the map's p for
    /// it, and that <see cref="FldigiConfidence.For(Hamlet.RadioEngine.Cw.Second.FldigiCwDecoder)"/>
    /// gives every character of the port the map's p for it.
    /// </remarks>
    [Fact]
    public void TheShippedConstantsAreTheWholePoolFit()
    {
        var m = Measured.Value;
        var ours = m.Whole["ours"];
        var port = m.Whole["port"];

        _output.WriteLine(string.Create(Invariant,
            $"fit | ours | a {ours.Beta[0]:R} | b {ours.Beta[1]:R} | not finite {ours.NotFinite:R} | fitted on {ours.Fitted}, constant on {ours.NonFinite}"));
        _output.WriteLine(string.Create(Invariant,
            $"fit | port | a {port.Beta[0]:R} | b level {port.Beta[1]:R} | c timing {port.Beta[2]:R} | not finite {port.NotFinite:R} | fitted on {port.Fitted}, constant on {port.NonFinite}"));

        var failures = new List<string>();

        void Same(string what, double shipped, double fitted)
        {
            if (!(Math.Abs(shipped - fitted) <= 1e-9 * Math.Max(1, Math.Abs(fitted))))
            {
                failures.Add(string.Create(Invariant, $"{what}: shipped {shipped:R}, fitted {fitted:R}"));
            }
        }

        Same("ours a", CwCharacterProbability.Intercept, ours.Beta[0]);
        Same("ours b", CwCharacterProbability.Slope, ours.Beta[1]);
        Same("ours not finite", CwCharacterProbability.NotFinite, ours.NotFinite);
        Same("port a", FldigiConfidence.Intercept, port.Beta[0]);
        Same("port b", FldigiConfidence.LevelSlope, port.Beta[1]);
        Same("port c", FldigiConfidence.TimingSlope, port.Beta[2]);
        Same("port not finite", FldigiConfidence.NotFinite, port.NotFinite);

        var all = WhatEachDecoderKnowsAboutEachCharacterFact.Characters;
        var oursChecked = 0;

        foreach (var r in all.Where(r => r.Decoder == "ours"))
        {
            oursChecked++;
            Same($"ours {r.Recording} #{r.Index} `{r.Text}` p", r.Character.Probability, P(ours, r));
        }

        var portChecked = 0;

        foreach (var row in BothDecodersAreScoredAlikeTests.Rows.Where(x => x.Port is not null))
        {
            var p = FldigiConfidence.For(WhatEachDecoderKnowsAboutEachCharacterFact.RunPort(row));

            foreach (var r in all.Where(r => r.Decoder == "port" && r.Recording == row.Name))
            {
                portChecked++;
                Same($"port {r.Recording} #{r.Index} `{r.Text}` p", p[r.Index], P(port, r));
            }
        }

        _output.WriteLine($"shipped | ours {oursChecked} characters, port {portChecked} characters checked against the map; {failures.Count} differ");

        foreach (var f in failures.Take(20))
        {
            _output.WriteLine("differs | " + f);
        }

        Assert.True(failures.Count == 0, $"{failures.Count} differ; first: {failures.FirstOrDefault()}");
    }

    /// <remarks>
    /// HM-REQ-124, the measure: the held-out reliability table per decoder and per
    /// condition, the in-sample figure beside it, the verdicts, MET-CAL's three bins
    /// for ours, and `docs/phase-requirements/calibration.md` written. Asserts only
    /// that the file was written with its parts.
    /// </remarks>
    [Fact]
    public void HeldOutPerConditionAndTheFileWritten()
    {
        var m = Measured.Value;
        var conditions = Conditions(m.Scored);
        var md = new StringBuilder();
        var verdictRows = new StringBuilder();
        var tables = new StringBuilder();
        var lists = new Dictionary<(string, CwCalibrationVerdict), List<string>>();

        foreach (var decoder in new[] { "ours", "port" })
        {
            foreach (var (label, key, holds) in conditions)
            {
                var here = m.Scored.Where(r => r.Decoder == decoder && holds(r)).ToList();
                var held = here.Select(r => new CwCalibrationPoint(m.HeldOut[r], r.Right)).ToList();
                var ins = here.Select(r => new CwCalibrationPoint(m.InSample[r], r.Right)).ToList();
                var verdict = CwCalibration.Judge(held);
                var inVerdict = CwCalibration.Judge(ins);

                if (!lists.TryGetValue((decoder, verdict), out var list))
                {
                    lists[(decoder, verdict)] = list = new List<string>();
                }

                list.Add(label);

                var line = string.Create(Invariant,
                    $"| {label} | {key} | {decoder} | {here.Count} | {Mean(held)} | {Right(held)} | {Worst(held)} | **{Word(verdict)}** | {Mean(ins)} | {Right(ins)} | {Worst(ins)} | {Word(inVerdict)} | {Gap(held, ins)} |\n");

                verdictRows.Append(line);
                _output.WriteLine("verdict " + line.TrimEnd());

                tables.Append(string.Create(Invariant, $"\n**{decoder}, {label}** ({key} keys; {here.Count} scored; held-out {Word(verdict)}, in-sample {Word(inVerdict)})\n\n"));
                tables.Append("| bin | p from | held-out characters | held-out mean p | held-out share right | held-out points | in-sample characters | in-sample mean p | in-sample share right | in-sample points |\n");
                tables.Append("|---|---|---|---|---|---|---|---|---|---|\n");

                var ht = CwCalibration.Table(held);
                var it = CwCalibration.Table(ins);

                for (var b = 0; b < CwCalibration.Bins; b++)
                {
                    if (ht[b].Count == 0 && it[b].Count == 0)
                    {
                        continue;
                    }

                    var bin = string.Create(Invariant,
                        $"| {b} | {b / 10.0:0.0} | {ht[b].Count} | {N(ht[b].MeanP)} | {N(ht[b].ShareRight)} | {Pt(ht[b].DifferencePoints)}{(ht[b].Count >= CwCalibration.BinFloor ? "" : " (under 10)")} | {it[b].Count} | {N(it[b].MeanP)} | {N(it[b].ShareRight)} | {Pt(it[b].DifferencePoints)} |\n");

                    tables.Append(bin);
                    _output.WriteLine($"bin | {decoder} | {label} " + bin.TrimEnd());
                }
            }
        }

        var perDecoder = new StringBuilder();

        foreach (var decoder in new[] { "ours", "port" })
        {
            perDecoder.Append($"- **{(decoder == "ours" ? "Ours" : "The port")}**\n");

            foreach (var v in new[] { CwCalibrationVerdict.Calibrated, CwCalibrationVerdict.NotCalibrated, CwCalibrationVerdict.NotMeasurable })
            {
                var names = lists.TryGetValue((decoder, v), out var l) ? l : new List<string>();

                perDecoder.Append($"  - {Word(v)} ({names.Count}): {(names.Count == 0 ? "none" : string.Join("; ", names))}\n");
                _output.WriteLine($"list | {decoder} | {Word(v)} | {names.Count} | {string.Join("; ", names)}");
            }
        }

        var gaps = new StringBuilder();

        foreach (var decoder in new[] { "ours", "port" })
        {
            var rows = m.Scored.Where(r => r.Decoder == decoder).ToList();
            var meanAbs = rows.Average(r => Math.Abs(m.HeldOut[r] - m.InSample[r])) * 100;
            var largest = rows.Max(r => Math.Abs(m.HeldOut[r] - m.InSample[r])) * 100;

            gaps.Append(string.Create(Invariant,
                $"- {decoder}: a character's held-out p differs from its in-sample p by {meanAbs:0.00} points on average over {rows.Count} scored characters, {largest:0.00} at most.\n"));
            _output.WriteLine(string.Create(Invariant, $"gap | {decoder} | mean {meanAbs:0.00} points | largest {largest:0.00} points | {rows.Count} scored"));
        }

        var metCal = MetCal();
        var ours = m.Whole["ours"];
        var port = m.Whole["port"];

        md.Append("# Calibration: each decoder's confidence against the keys (HM-REQ-124)\n\n");
        md.Append("Written by `EachDecodersConfidenceIsMeasuredTests.HeldOutPerConditionAndTheFileWritten` (work instruction 464, PHASE_PLAN.md 9.5). ");
        md.Append("Every character either decoder emits carries a p, the probability it is right. The characters are those of `parity.md`: both decoders ");
        md.Append("through the same harness, the same stretches and the same scorer (V-11). Right is the scorer's verdict; wrong and added are wrong; ");
        md.Append("placeholders and anything outside every scored stretch are unscored; a character two stretches cover is scored once per stretch, as the metrics count it. ");
        md.Append("No letter and no class of either decoder changed for this. **Every real key is inferred (R61, V-13); every synthetic key is exact.**\n\n");

        md.Append("## 1. The maps\n\n");
        md.Append("Both fixed at task 1 from the separation print in `.run-unit/unit464-features.txt`, before any calibration number was seen (V-14): ");
        md.Append("a logistic p = 1 / (1 + e^-(a + b.x)) per decoder, fitted by maximum likelihood (Newton's method) to right = 1 against wrong or added = 0 over ");
        md.Append("the real and synthetic sets together; one map per decoder, never one per condition.\n\n");
        md.Append(string.Create(Invariant,
            $"- **Ours** (`CwCharacterProbability`, set on `CwCharacter.Probability` by the stream where `MarginLlr` is set): x = sign(m) ln(1 + |m|) of the rival margin m, `CwCharacter.MarginLlr`. a = {ours.Beta[0]:0.######}, b = {ours.Beta[1]:0.######}, fitted on {ours.Fitted} characters. A margin that is not finite (+Infinity, short letters with no rival reading on the lattice) takes p = {ours.NotFinite:0.######}, the share right of the {ours.NonFinite} such scored characters.\n"));
        md.Append(string.Create(Invariant,
            $"- **The port** (`FldigiConfidence`, outside `Cw/Second/`; fldigi carries no confidence and nothing under `Cw/Second/` changed): x1 the level margin, 20 log10(sig_avg / noise_floor) in dB at the character's last up event (`cw.cxx:610`, `612-616`, the ratio fldigi's own squelch metric reads at `635-636`); x2 the timing margin, the least distance of any of its element lengths from the dot/dash split, in fldigi's own dit (`cw.cxx:811-812`, `847`, `502`). Both read from `FldigiCwKeyEvent`, ordered against the emissions by the decision rows (`TraceDecisions` on), because the port's filter hands out 1024 samples at once. p = 1 / (1 + e^-(a + b.x1 + c.x2)), a = {port.Beta[0]:0.######}, b = {port.Beta[1]:0.######}, c = {port.Beta[2]:0.######}, fitted on {port.Fitted} characters; both margins were readable on every character, so the constant for an unreadable one, {port.NotFinite:0.######}, is the pool's share right.\n"));
        md.Append("- The timing margin does not separate right from wrong monotonically (task 1's print: the right share falls both near the split and past one dit from it). The form was fixed as linear before any number and is kept.\n\n");

        md.Append("## 2. The verdict per condition\n\n");
        md.Append("Held-out by recording (464 DECIDED (3)): for each of the 35, the map fitted on the other 34. **Calibrated** means every bin of p 0.1 wide holding at least 10 characters, ");
        md.Append("and the condition overall, has its share right within 5 points of its mean p, on at least 30 scored characters; under 30 is **not measurable** (DECIDED (4)). ");
        md.Append("The in-sample columns use the whole-pool map the decoders ship, and judge nothing. The worst bin is the populated bin (10 or more) furthest from its mean p, in points, share right less mean p. ");
        md.Append("The last column is the held-out overall difference (share right less mean p) less the in-sample one, in points.\n\n");
        md.Append("| condition | key | decoder | scored | held-out mean p | held-out share right | held-out worst bin | held-out verdict | in-sample mean p | in-sample share right | in-sample worst bin | in-sample verdict | held-out less in-sample |\n");
        md.Append("|---|---|---|---|---|---|---|---|---|---|---|---|---|\n");
        md.Append(verdictRows);
        md.Append("\n**Named, per decoder** (held-out):\n\n");
        md.Append(perDecoder);
        md.Append("\n**How much the fit learned the corpus:**\n\n");
        md.Append(gaps);

        md.Append("\n## 3. The held-out reliability tables\n\n");
        md.Append("Ten bins of p; empty bins left out; `(under 10)` marks a bin that does not count toward the verdict.\n");
        md.Append(tables);

        md.Append("\n## 4. MET-CAL, ours (`CW_SPEC.md` section 11)\n\n");
        md.Append(metCal);

        md.Append("\n## 5. What this does not prove\n\n");
        md.Append("- **The real set's keys are inferred** (R61, V-13): reasoned from the form of a call, adjudicated, or differenced from consecutive transcripts, never transcribed. A character the key calls wrong may be right, and the real rows' share right is stated against those keys, every time.\n");
        md.Append("- **No condition here is a `CH-*` condition** (PHASE_PLAN.md 7.4). The real rows are HF recordings with no channel profile and no measured SNR_2500; the synthetic rows are one generator's shaped band noise, not shown to be `CH-AWGN`, and never sole evidence (CLAUDE.md 12.5). HM-REQ-124's \"per condition\" is met here only over the conditions the tree has.\n");
        md.Append("- **A map fitted on 35 recordings is not proven on the 36th.** The held-out verdict is the best the corpus can say about a recording the map has not seen, and the corpus is small: most finer conditions hold under 30 scored characters and are not measurable.\n");
        md.Append("- **Ours emits no dim character on the 35**, so dim characters, which HM-REQ-014 is about, are not measured at all; every scored character of ours is sure. The port's characters are all sure under `parity.md`'s mapping.\n");
        md.Append("- **The port runs on fldigi's shipped defaults** at 18 WPM with tracking on, given the instrument's pitch, as in `parity.md`. Its p says nothing about fldigi at other settings.\n");
        md.Append("- **Nothing here votes.** Which decoder may vote where is 9.6's, read from section 2's verdicts under HM-REQ-124.\n");

        var path = Path.Combine(CwToneSurveyTests.RepositoryRoot(), "docs", "phase-requirements", "calibration.md");
        var text = md.ToString();

        File.WriteAllText(path, text);
        _output.WriteLine(text);

        foreach (var part in new[] { "## 1. ", "## 2. ", "## 3. ", "## 4. ", "## 5. " })
        {
            Assert.Contains(part, text, StringComparison.Ordinal);
        }
    }

    private static string MetCal()
    {
        var ours = WhatEachDecoderKnowsAboutEachCharacterFact.Characters
            .Where(r => r.Decoder == "ours" && r.Verdict != "outside").ToList();
        var sb = new StringBuilder();

        sb.Append("Observed accuracy per class over every stretch scored, against the rate each class states. Placeholders are unscored, as the spec has them.\n\n");
        sb.Append("| class | set | key | emitted in stretches | right | wrong | added | observed accuracy | stated rate |\n|---|---|---|---|---|---|---|---|---|\n");

        foreach (var cls in new[] { "sure", "dim", "placeholder" })
        {
            foreach (var real in new[] { true, false })
            {
                var here = ours.Where(r => r.Class == cls && r.Real == real).ToList();
                var scored = here.Count(r => r.Scored);
                var stated = cls switch
                {
                    "sure" => "asserted as sent (HM-REQ-013 on its conditions: every sure character correct)",
                    "dim" => "at least 0.70 (HM-REQ-014)",
                    _ => "unscored",
                };
                var accuracy = cls == "placeholder" ? "unscored" : scored == 0 ? "no number: none emitted" : (here.Count(r => r.Right) / (double)scored).ToString("0.000", Invariant);

                sb.Append($"| {cls} | {(real ? "real" : "synthetic")} | {(real ? "inferred" : "exact")} | {here.Count} | {here.Count(r => r.Right)} | {here.Count(r => r.Verdict == "wrong")} | {here.Count(r => r.Verdict == "added")} | {accuracy} | {stated} |\n");
            }
        }

        return sb.ToString();
    }

    private static string Mean(IReadOnlyList<CwCalibrationPoint> p) => p.Count == 0 ? "-" : p.Average(x => x.P).ToString("0.000", Invariant);

    private static string Right(IReadOnlyList<CwCalibrationPoint> p) => p.Count == 0 ? "-" : (p.Count(x => x.Right) / (double)p.Count).ToString("0.000", Invariant);

    private static string Worst(IReadOnlyList<CwCalibrationPoint> p) => Pt(CwCalibration.WorstPopulatedBin(p));

    private static string Gap(IReadOnlyList<CwCalibrationPoint> held, IReadOnlyList<CwCalibrationPoint> ins)
        => held.Count == 0 ? "-" : Pt(100 * ((held.Count(x => x.Right) / (double)held.Count - held.Average(x => x.P)) - (ins.Count(x => x.Right) / (double)ins.Count - ins.Average(x => x.P))));

    private static string N(double x) => double.IsNaN(x) ? "-" : x.ToString("0.000", Invariant);

    private static string Pt(double x) => double.IsNaN(x) ? "none populated" : x.ToString("+0.0;-0.0;0.0", Invariant);

    private static string Word(CwCalibrationVerdict v) => v switch
    {
        CwCalibrationVerdict.Calibrated => "calibrated",
        CwCalibrationVerdict.NotCalibrated => "not calibrated",
        _ => "not measurable",
    };
}
