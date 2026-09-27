using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Proves HM-REQ-128: the output emitted on every condition is no worse than the
/// better single decoder on every metric of sections B and I, and where the
/// arbitration loses to either decoder alone it is switched off there and the
/// better decoder alone is used (work instruction 466, task 3; PHASE_PLAN.md 9.7).
/// </summary>
/// <remarks>
/// <para>**(a) THE RULE, ON FIGURES BUILT BY HAND FROM HM-REQ-128'S WORDS** (CLAUDE.md
/// 12.5), never from the switch's output: one condition where the arbitration
/// beats both, one where it loses to ours, one where it loses to the port, and
/// two where the decoders split so 466 DECIDED (4)'s order decides. Each asserts
/// the switch <see cref="CwSwitchTable.Choose"/> picks and what
/// <see cref="CwArbiter.Arbitrate(IReadOnlyList{CwCharacter}, IReadOnlyList{CwSecondReading}, CwVote, CwArbitrationSwitch)"/>
/// then emits. Watched red against a <c>Choose</c> that always answered
/// arbitrate.</para>
/// <para>**(b) THE CORPUS**, through <see cref="TheArbitrationEarnsItsPlaceFact"/>'s
/// one scorer: on every row the switch in force is the one the rule reaches on
/// the path that emits under it (466 DECIDED (5)), and the output emitted under
/// it is no worse than the better decoder alone on every metric compared there.
/// Watched red with <see cref="CwSwitchTable.Rows"/> empty.</para>
/// <para>**(b) ASSERTS THE CONDITION ROWS AND THE LIVE ROW ONLY** (work instruction
/// 467, DECIDED (2) and (3)): <see cref="Decided2And3Rows"/> is the one list. A
/// condition is one named profile at one level, as `CW_SPEC.md` section 4 defines
/// it; a union across levels or profiles is a summary, still computed and printed
/// here and by the fact, and not asserted. Re-watched red against an empty
/// <see cref="Table"/>.</para>
/// </remarks>
public sealed class TheArbitrationEarnsItsPlaceTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the rows are printed.</param>
    public TheArbitrationEarnsItsPlaceTests(ITestOutputHelper output)
        => _output = output;

    // --- (a) ---

    // Two of ours and three of the port's on one clock: the same A, B against C, and an E only the port read after a space.
    private static readonly CwCharacter[] Ours =
    {
        Character("A", 0.10, 0.90),
        Character("B", 0.30, 0.60),
    };

    private static readonly CwSecondReading[] Port =
    {
        new("A", CwConfidence.High, ".-", 0.80, 0.02, 0.10, 20),
        new("C", CwConfidence.High, "-.-.", 0.95, 0.22, 0.30, 20),
        new("E", CwConfidence.High, ".", 0.70, 0.60, 0.64, 20) { WordGapBefore = true },
    };

    // The port votes and ours does not, so arbitrating, ours alone and the port alone each emit a different text.
    private static readonly CwVote PortVotes = new(false, true);

    private static CwCharacter Character(string text, double endSeconds, double p)
        => new(text, CwConfidence.High, 1, text == "A" ? ".-" : "-...", double.NaN, 20, TimeSpan.FromSeconds(endSeconds))
        {
            SpanHops = 16,
            Probability = p,
        };

    private static Dictionary<string, CwFigure> Figures(int invented, int sureWrong, int sureEmitted, int coverage, int wbe)
        => new(StringComparer.Ordinal)
        {
            ["011"] = new(invented, 100),
            ["010"] = new(sureWrong, sureEmitted),
            ["012"] = new(coverage, 100),
            ["081"] = new(wbe, 20),
        };

    private static string Emitted(CwArbitrationSwitch s)
        => string.Concat(CwArbiter.Arbitrate(Ours, Port, PortVotes, s).Characters.Select(c => c.Text));

    private void Case(string name, Dictionary<string, CwFigure> arbitrated, Dictionary<string, CwFigure> ours, Dictionary<string, CwFigure> port,
        CwArbitrationSwitch expected, string text)
    {
        var verdict = CwSwitchTable.Choose(arbitrated, ours, port);
        var emitted = Emitted(verdict.Switch);

        _output.WriteLine($"HM-REQ-128 (a) | {name} | loses on {(verdict.Loses ? string.Join(", ", verdict.LostOn) : "nothing")} | better alone {verdict.Better} by {verdict.BetterBy} | switch {verdict.Switch}, expected {expected} | emits `{emitted}`, expected `{text}`");

        Assert.Equal(expected, verdict.Switch);
        Assert.Equal(text, emitted);
    }

    /// <summary>HM-REQ-128 (a): the arbitration beats both decoders alone, so it stays on, and the arbiter's own text is emitted.</summary>
    [Fact]
    public void WhereTheArbitrationBeatsBothItStaysOn()
        => Case("beats both",
            Figures(1, 1, 90, 90, 1), Figures(2, 2, 90, 88, 2), Figures(3, 3, 80, 70, 3),
            CwArbitrationSwitch.Arbitrate, "ACE");

    /// <summary>HM-REQ-128 (a): the arbitration invents more than ours alone, which is no worse than the port anywhere; ours alone is emitted.</summary>
    [Fact]
    public void WhereItLosesToOursOursAloneIsUsed()
        => Case("loses to ours",
            Figures(4, 4, 90, 88, 2), Figures(2, 2, 90, 88, 2), Figures(5, 5, 80, 70, 6),
            CwArbitrationSwitch.OursAlone, "AB");

    /// <summary>HM-REQ-128 (a): the arbitration is ours and the port alone is better on every metric; the port alone is emitted, its space kept.</summary>
    [Fact]
    public void WhereItLosesToThePortThePortAloneIsUsed()
        => Case("loses to the port",
            Figures(4, 4, 90, 80, 5), Figures(4, 4, 90, 80, 5), Figures(1, 1, 90, 85, 2),
            CwArbitrationSwitch.PortAlone, "AC E");

    /// <summary>HM-REQ-128 (a): the decoders split - ours invents less, the port covers more - so 011, first in the order, decides for ours.</summary>
    [Fact]
    public void WhereTheDecodersSplitTheOrderDecidesForOurs()
        => Case("split, ours first at 011",
            Figures(3, 3, 90, 70, 2), Figures(3, 3, 90, 70, 2), Figures(5, 5, 90, 85, 1),
            CwArbitrationSwitch.OursAlone, "AB");

    /// <summary>HM-REQ-128 (a): the decoders split - the port invents less, ours covers more - so 011 decides for the port.</summary>
    [Fact]
    public void WhereTheDecodersSplitTheOrderDecidesForThePort()
        => Case("split, the port first at 011",
            Figures(5, 5, 90, 85, 1), Figures(5, 5, 90, 85, 1), Figures(3, 3, 90, 70, 2),
            CwArbitrationSwitch.PortAlone, "AC E");

    // --- (b) ---

    /// <summary>What a row of the three-way table is, for HM-REQ-128 (work instruction 467, DECIDED (2) and (3)).</summary>
    internal enum RowClass
    {
        /// <summary>One named sender or channel profile at one level: asserted on the harness.</summary>
        Condition,

        /// <summary>The row the live product runs under: asserted on the live path.</summary>
        Live,

        /// <summary>A union across levels or profiles: computed and printed, not asserted.</summary>
        Summary,
    }

    /// <summary>
    /// Every row of arbitration.md section 2 and parity.md section 3, its class,
    /// its SNR as the tree states it, and why (work instruction 467, DECIDED (2)
    /// and (3)). The one copy; the fact prints it into arbitration.md section 8.
    /// </summary>
    /// <remarks>
    /// `CW_SPEC.md` section 4: "Every condition is a named profile from this file
    /// (`CH-*`, `TX-*`, `INT-*`, `IMP-*`) and an SNR in the reference bandwidth
    /// (§8), never prose." `CW_REQUIREMENTS.md`'s verification table, row 010: "one
    /// row per condition profile". A row that mixes levels or profiles is not one
    /// profile at one SNR, so it is a summary. The arbiter's reading, overrulable.
    /// </remarks>
    internal static readonly IReadOnlyList<(string Row, RowClass Class, string Snr, string Reason)> Decided2And3Rows = new[]
    {
        ("real HF, all", RowClass.Live, "not measured",
            "the row the live product runs under (CwVoteTable.LiveCondition); the union of all 23 real recordings, so judged on the live path only (DECIDED (3)) and printed from the harness beside it"),
        ("real HF, no CH-* profile, SNR_2500 not measured, sender TX-FARNS (CW_SPEC.md 6.4 and 10, HM-DEC-115's traffic net)", RowClass.Condition, "not measured",
            "one named sender profile, TX-FARNS, one recording"),
        ("real HF, no CH-* profile, SNR_2500 not measured, sender TX-ITU (CW_SPEC.md 10, the KD0UN capture)", RowClass.Condition, "not measured",
            "one named sender profile, TX-ITU, one recording"),
        ("real HF, no CH-* profile, SNR_2500 not measured, sender TX-TIGHT (CW_SPEC.md 10, HM-DEC-101)", RowClass.Condition, "not measured",
            "one named sender profile, TX-TIGHT, one recording"),
        ("real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md", RowClass.Condition, "not measured",
            "the real per-sender row for the 20 recordings whose sender CW_SPEC.md section 10 does not name; a condition row by DECIDED (3), though it names no profile, and asserting it cannot loosen the test"),
        ("synthetic, all", RowClass.Summary, "mixed: 0, 5 and 15 dB in the passband",
            "the union of the six synthetic rows, two senders at three levels: not one profile at one SNR"),
        ("synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 0 dB in the passband (not restated in the 2500 Hz reference)", RowClass.Condition, "0 dB in the passband",
            "one sender, TX-ITU, at one level"),
        ("synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference)", RowClass.Condition, "15 dB in the passband",
            "one sender, TX-ITU, at one level"),
        ("synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 5 dB in the passband (not restated in the 2500 Hz reference)", RowClass.Condition, "5 dB in the passband",
            "one sender, TX-ITU, at one level"),
        ("synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 0 dB in the passband (not restated in the 2500 Hz reference)", RowClass.Condition, "0 dB in the passband",
            "one sender, character gap 5 inside TX-FARNS, at one level"),
        ("synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference)", RowClass.Condition, "15 dB in the passband",
            "one sender, character gap 5 inside TX-FARNS, at one level"),
        ("synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 5 dB in the passband (not restated in the 2500 Hz reference)", RowClass.Condition, "5 dB in the passband",
            "one sender, character gap 5 inside TX-FARNS, at one level"),
    };

    /// <summary>A row's class from <see cref="Decided2And3Rows"/>; null for a row the list does not name.</summary>
    internal static RowClass? ClassOf(string row)
        => Decided2And3Rows.Where(r => r.Row == row).Select(r => (RowClass?)r.Class).SingleOrDefault();

    // The switch table part (b) reads: CwSwitchTable.Rows. Emptied, and only emptied, to watch (b) fail first (CLAUDE.md 12.5).
    private static readonly IReadOnlyDictionary<string, CwArbitrationSwitch> Table = CwSwitchTable.Rows;

    private static CwArbitrationSwitch For(string condition)
        => Table.TryGetValue(condition, out var s) ? s : CwArbitrationSwitch.Arbitrate;

    /// <summary>
    /// HM-REQ-128 (b), the harness: every condition row of
    /// <see cref="Decided2And3Rows"/>, each recording under its own row, the output
    /// emitted under the switch table scored beside the arbitrated output, ours
    /// alone and the port alone. The summary rows are computed and printed, not
    /// asserted (work instruction 467).
    /// </summary>
    [Fact]
    public void EveryHarnessRowIsNoWorseThanTheBetterDecoderAlone()
    {
        var harness = TheArbitrationEarnsItsPlaceFact.Harness();
        var emitted = harness.Select(h => h.Three with
            {
                Emitted = new IReadOnlyList<CwCharacter>?[]
                {
                    CwArbiter.Arbitrate(h.Row.Ours.Settled, h.Second, CwVoteTable.For(h.Row.Condition), For(h.Row.Condition)).Characters,
                    h.Three.Emitted[1],
                    h.Three.Emitted[2],
                },
            })
            .ToList();
        var (arbSpans, emittedSpans) = Spans("harness");
        var before = TheArbitrationEarnsItsPlaceFact.Rows(harness.Select(h => h.Three).ToList(), new Dictionary<string, (int, int)[]> { ["x"] = arbSpans });
        var after = TheArbitrationEarnsItsPlaceFact.Rows(emitted, new Dictionary<string, (int, int)[]> { ["x"] = emittedSpans });

        Check("harness", before, after, RowClass.Condition);
    }

    /// <summary>
    /// HM-REQ-128 (b), the live path: real HF, all, the row the product runs under,
    /// through <see cref="CwDecoder"/> with the second reader, arbitrating and then
    /// under the switch table at <see cref="CwVoteTable.LiveCondition"/>, as
    /// <see cref="CwSwitchTable.Live"/> reads it.
    /// </summary>
    [Fact]
    public void TheLiveRowIsNoWorseThanTheBetterDecoderAlone()
    {
        var threes = new List<TheArbitrationEarnsItsPlaceFact.Three>();
        var switched = new List<TheArbitrationEarnsItsPlaceFact.Three>();

        foreach (var k in WhatTheStrayLettersRestOnTests.KeyedRecordings)
        {
            var audio = WavAudio.Read(Path.Combine(CapturedSignalTests.Folder, k.Name + ".wav"));
            var condition = TheRequirementsAreMeasuredTests.RealCondition(k.Name);
            var (ours, oursLive, _) = TheArbitrationEarnsItsPlaceFact.Drive(audio.Samples, audio.SampleRate, 600, false);
            var (arbitrated, arbLive, readings) = TheArbitrationEarnsItsPlaceFact.Drive(audio.Samples, audio.SampleRate, 600, true);
            var (emitted, emittedLive, _) = TheArbitrationEarnsItsPlaceFact.Drive(audio.Samples, audio.SampleRate, 600, true, For(CwVoteTable.LiveCondition));
            var port = TheArbitrationEarnsItsPlaceFact.PortAlone(readings);

            threes.Add(new(k.Name, true, condition, CwKeyKind.Inferred, new[] { arbitrated, ours, port }, new[] { arbLive, oursLive }));
            switched.Add(new(k.Name, true, condition, CwKeyKind.Inferred, new[] { emitted, ours, port }, new[] { emittedLive, oursLive }));
        }

        var (arbSpans, emittedSpans) = Spans("live");
        var before = TheArbitrationEarnsItsPlaceFact.Rows(threes, new Dictionary<string, (int, int)[]> { ["x"] = arbSpans });
        var after = TheArbitrationEarnsItsPlaceFact.Rows(switched, new Dictionary<string, (int, int)[]> { ["x"] = emittedSpans });

        Check("live", before.Where(r => r.Label == CwVoteTable.LiveCondition).ToList(), after, RowClass.Live);
    }

    /// <summary>
    /// HM-REQ-128 on the live path under the port alone: <see cref="CwDecoder"/>
    /// with the second reader and <see cref="CwArbitrationSwitch.PortAlone"/>
    /// emits exactly the port's own live reading, its spaces included, and none of
    /// ours, on the recording of the one row the table sets to the port alone. A
    /// check of the live emission; the table does not put the live product there.
    /// </summary>
    [Fact]
    public void ThePortAloneOnTheLivePathEmitsThePortsOwnReading()
    {
        var row = CwSwitchTable.Rows.Single(r => r.Value == CwArbitrationSwitch.PortAlone).Key;
        var recipe = SyntheticCq.All.Single(c => TheRequirementsAreMeasuredTests.SyntheticCondition(c) == row);
        var audio = WavAudio.Read(Path.Combine(SyntheticCq.Folder, recipe.Name + ".wav"));
        var (emitted, _, readings) = TheArbitrationEarnsItsPlaceFact.Drive(audio.Samples, audio.SampleRate, SyntheticCq.StartingPitchHz, true, CwArbitrationSwitch.PortAlone);
        var port = TheArbitrationEarnsItsPlaceFact.PortAlone(readings);

        _output.WriteLine($"HM-REQ-128 live port alone | {recipe.Name} | emitted `{string.Concat(emitted.Select(c => c.Text))}` | the port read `{string.Concat(port.Select(c => c.Text))}`");

        Assert.NotEmpty(port);
        Assert.Equal(CwMetrics.Symbols(port), CwMetrics.Symbols(emitted));
        Assert.All(emitted.Where(c => !c.IsWordGap), c => Assert.Equal(CwReader.Second, c.Arbitration!.Emitted));
        Assert.All(emitted.Where(c => !c.IsWordGap), c => Assert.Equal(CwArbitrationSwitch.PortAlone, c.Arbitration!.Switch));
    }

    // HM-REQ-084's spans on one path: the arbitrated output's counts and the switched output's, ours and the port the same in both.
    private static ((int, int)[] Arbitrated, (int, int)[] Emitted) Spans(string path)
    {
        var arbitrated = new (int Exact, int Measurable)[3];
        var emitted = new (int Exact, int Measurable)[3];

        foreach (var recording in WhatTheNamedWordsReadTests.Recordings)
        {
            var plain = TheArbitrationEarnsItsPlaceFact.SpanReadings(recording)[path];
            var switched = TheArbitrationEarnsItsPlaceFact.SpanReadings(recording, true)[path];

            foreach (var span in WhatTheNamedWordsReadTests.Spans.Where(s => s.Recording == recording))
            {
                for (var o = 0; o < 3; o++)
                {
                    var a = TheArbitrationEarnsItsPlaceFact.ReadSpan(span, plain[o]).Met ? 1 : 0;
                    var e = TheArbitrationEarnsItsPlaceFact.ReadSpan(span, o == 0 ? switched[0] : plain[o]).Met ? 1 : 0;

                    arbitrated[o] = (arbitrated[o].Exact + a, arbitrated[o].Measurable + 1);
                    emitted[o] = (emitted[o].Exact + e, emitted[o].Measurable + 1);
                }
            }
        }

        return (arbitrated, emitted);
    }

    // On each row of the class this path asserts: the switch in force is the rule's, and what it emits is no worse than the better decoder alone.
    // Every row is printed; a row of another class is marked and not asserted. Every row computed must be classified, and every row the list gives this path must be computed.
    private void Check(string path, IReadOnlyList<TheArbitrationEarnsItsPlaceFact.PathRow> before, IReadOnlyList<TheArbitrationEarnsItsPlaceFact.PathRow> after, RowClass asserted)
    {
        var failures = new List<string>();

        failures.AddRange(before.Where(r => ClassOf(r.Label) is null).Select(r => $"{r.Label}: not classified in Decided2And3Rows"));
        failures.AddRange(Decided2And3Rows.Where(r => r.Class == asserted && before.All(b => b.Label != r.Row)).Select(r => $"{r.Row}: listed as {asserted}, not computed on the {path} path"));

        foreach (var row in before)
        {
            var verdict = row.Verdict;
            var inForce = For(row.Label);
            var cls = ClassOf(row.Label);
            var emitted = after.Single(a => a.Label == row.Label).Figures[0];
            var better = row.Figures[verdict.Better == CwReader.Ours ? 1 : 2];
            var worse = CwSwitchTable.Order
                .Where(m => emitted.ContainsKey(m) && better.ContainsKey(m) && CwSwitchTable.Compare(m, emitted[m], better[m]) > 0)
                .ToList();

            _output.WriteLine($"HM-REQ-128 (b) | {path} | {row.Label} | {(cls == asserted ? $"{cls}, asserted" : $"{cls?.ToString() ?? "unclassified"}, printed, not asserted")} | loses on {(verdict.Loses ? string.Join(", ", verdict.LostOn) : "nothing")} | better alone {verdict.Better} by {verdict.BetterBy} | "
                              + $"rule {verdict.Switch}, table {inForce} | emitted worse than the better alone on {(worse.Count == 0 ? "nothing" : string.Join(", ", worse))} | "
                              + string.Join(", ", CwSwitchTable.Order.Where(emitted.ContainsKey).Select(m => $"{m} {TheArbitrationEarnsItsPlaceFact.Text(emitted, m)}")));

            if (cls != asserted)
            {
                continue;
            }

            if (inForce != verdict.Switch)
            {
                failures.Add($"{row.Label}: the table says {inForce}, the rule on the {path} path says {verdict.Switch}");
            }

            if (worse.Count > 0)
            {
                failures.Add($"{row.Label}: emitted worse than the better decoder alone ({verdict.Better}) on {string.Join(", ", worse)}");
            }
        }

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }
}
