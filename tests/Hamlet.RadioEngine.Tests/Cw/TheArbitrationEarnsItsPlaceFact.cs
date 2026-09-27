using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Cw.Second;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Hamlet.RadioEngine.Tests.Cw.Instruments;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The arbitrated transcript, ours alone and the port alone, scored side by side
/// on every metric of `CW_REQUIREMENTS.md` sections B and I, per condition, on
/// the harness and on the live path (work instruction 466, task 1; HM-REQ-128;
/// PHASE_PLAN.md 9.7).
/// </summary>
/// <remarks>
/// <para>**A PRINTER. IT ASSERTS NOTHING ABOUT ANY OF THE THREE.** Serves
/// HM-REQ-128. Nothing under `src` changes for it.</para>
/// <para>**FIXED HERE AT TASK 1, BEFORE ANY THREE-WAY FIGURE WAS COMPUTED, AND
/// NEVER CHANGED AFTER** (V-14; 466 DECIDED (2) to (5)). The three outputs are
/// scored by one scorer, against one set of keys, by the same calls
/// (V-11, V-13): <see cref="TheRequirementsAreMeasuredTests.Measure"/> and the
/// <see cref="CwMetrics"/> calls, each figure with its key's kind.</para>
///
/// <para>**1. THE METRIC LIST** (466 DECIDED (2), requirement by requirement; a
/// rate is compared as a rate, count over count, and a rate whose denominator is
/// zero for an output is not defined for it):</para>
/// <list type="bullet">
/// <item>**HM-REQ-010, MET-CER-SURE**: sure characters wrong or added over sure
/// characters emitted (`CwMetrics.SureErrors`). Lower is better. Defined for all
/// three; not defined for an output that emits nothing sure on the row.</item>
/// <item>**HM-REQ-011, MET-INVENTED**: sure added plus sure wrong over characters
/// sent (`CwMetrics.Invented`). Lower is better. Defined for all three.</item>
/// <item>**HM-REQ-012, MET-COVERAGE**: sure and right over characters sent (R82,
/// `CwMetrics.Coverage`). Higher is better. Defined for all three.</item>
/// <item>**HM-REQ-013**: sent characters not emitted sure and correct, over
/// characters sent (sent less sure and right). Lower is better. Compared only on
/// the rows the requirement names, the 15 dB rows; no row in the tree is shown
/// to be CH-AWGN, so the synthetic 15 dB rows stand for it and say so. Its
/// per-row verdict is met where the count is zero and MET-CER-SURE is zero, as
/// its rationale reads it. Defined for all three.</item>
/// <item>**HM-REQ-014, dim accuracy**: characters emitted dim (class not sure)
/// and right, over characters emitted dim, from the same alignment's steps.
/// Higher is better; the verdict is met at 0.70 or more. **Not defined for the
/// port**: parity.md section 1 maps every non-space character it prints to sure
/// and its no-match output to a placeholder, so it emits no dim; and not defined
/// for any output that emits no dim on the row. Compared between the
/// arbitrated output and ours only.</item>
/// <item>**HM-REQ-080 and 081, MET-WBE**: word boundaries inserted plus deleted
/// over words sent (`CwMetrics.WordBoundaries`), one metric; 080's verdict (zero)
/// on the 15 dB rows, 081's (at most 0.05) on every row. Lower is better.
/// Defined for all three.</item>
/// <item>**HM-REQ-083, live against settled boundaries**: on the live path only,
/// over every pair of consecutive characters the leading edge ever showed that
/// also settle as consecutive characters (each matched by its end on the audio
/// clock, <c>CwCharacter.At</c>), the share of distinct pairs where some live
/// rendering's word boundary between them differs from the settled one. Lower is
/// better. **Not defined for the port**: it prints once and has one rendering.
/// **Not defined on the harness**, which has no live rendering. Compared between
/// the arbitrated output and ours only, on the live path.</item>
/// <item>**HM-REQ-084, the named spans**: of the six spans DEV_ANALYSIS_2026-08-27
/// section 4 names, those whose audio is in the tree
/// (<see cref="WhatTheNamedWordsReadTests.Spans"/>: `ABOVE`, `BREEZE`, `FLEX`;
/// `WEEKEND`, `THINKING` and `USED TO USE A FIRM` are not measurable here, as
/// <see cref="WhatTheNamedWordsReadTests.NotHere"/> records), each read exactly
/// by that printer's rule: the characters whose spans overlap the span spell the
/// word, with a boundary on each side and none inside, every letter sure. Spans
/// read exactly over spans measurable. Higher is better. Defined for all three,
/// each character placed by its own span. Its recordings name no sender in
/// `CW_SPEC.md` section 10, so it is compared on the rows real HF, all and real
/// HF, sender not stated.</item>
/// <item>**HM-REQ-015 and 082 are properties, stated once for all three.** 015:
/// each character carries its class when it is emitted - ours on both seams
/// (`EveryCharacterCarriesAConfidenceTests`), the port's from the harvester's
/// reading at the moment it prints, and the arbitrated character from
/// <c>CwArbiter.Decide</c> at the moment it is decided; checked by those tests and
/// by inspection. 082: word-boundary errors are counted by
/// <c>CwMetrics.WordBoundaries</c> apart from character errors, for all three,
/// through the one scorer.</item>
/// </list>
///
/// <para>**2. THE THREE RULES.**</para>
/// <list type="bullet">
/// <item>**The loss rule** (466 DECIDED (3)): on a row, the arbitration loses
/// where it is strictly worse than ours alone, or than the port alone, on any
/// metric compared on that row and defined for both of the pair.</item>
/// <item>**The better-decoder rule** (466 DECIDED (4)): the better decoder alone
/// on a row is the one no worse than the other on every metric compared for both;
/// where neither is, the first metric where they differ in the order 011, 010,
/// 013, 012, 014, 081, 083, 084 decides, honesty first as CLAUDE.md 0.0 and
/// HM-REQ-011's "prime directive as a number" rank them; ours on a full tie.</item>
/// <item>**The path rule** (466 DECIDED (5)): a row's switch is set from the
/// path that emits under it. The live product runs only under real HF, all
/// (<c>CwVoteTable.LiveCondition</c>, unit 465), so that row is set from the live
/// path and printed from the harness beside it; every other row from the
/// harness, where each recording runs under its own row.</item>
/// <item>**The switch that follows**: no loss, `arbitrate`; a loss, the better
/// decoder alone - `ours alone` or `port alone`. Where the rule names the port,
/// the port alone is emitted at parity.md section 1's mapping though HM-REQ-124
/// withholds its vote (466 DECIDED (6)).</item>
/// </list>
///
/// <para>**3. WHERE THE SWITCH GOES**, with no line under `Cw/Second/`
/// changing:</para>
/// <list type="bullet">
/// <item>A table beside <c>CwVoteTable</c>, <c>CwSwitchTable</c>
/// (`src/Hamlet.RadioEngine/Cw/CwSwitchTable.cs`): per condition row `arbitrate`,
/// `ours alone` or `port alone`, with <c>For(condition)</c>, <c>Live</c> at
/// <c>CwVoteTable.LiveCondition</c>, and the rule above as <c>Choose</c>.</item>
/// <item>**The harness**: <c>CwArbiter.Arbitrate</c>, <c>Decide</c> and
/// <c>DecideAlone</c> take the switch in force. `arbitrate` is today's code
/// unchanged; `ours alone` emits ours' characters as ours prints them alone;
/// `port alone` emits the port's readings in order, each a
/// <c>CwCharacter</c> at parity.md section 1's mapping as <c>DecideAlone</c>
/// builds it, with a word boundary where the port printed a space. Every span is
/// still paired and recorded, both readings on it, the switch in force on
/// <c>CwArbitration</c> (HM-REQ-121).</item>
/// <item>**The port's spaces**: <c>CwSecondHarvester.Take</c> reads them today
/// and drops them; it marks the next reading <c>WordGapBefore</c>. Nothing the
/// arbiter reads under `arbitrate` changes.</item>
/// <item>**The live path**: <c>CwDecoder.Switch</c>, <c>CwSwitchTable.Live</c> by
/// default, consulted in <c>CwDecoder.Arbitrated</c>, <c>ArbitratedEdge</c> and
/// <c>Flush</c>, where <c>CwSecondReader</c>'s readings meet ours; under `port
/// alone` ours' settled characters and word boundaries are not emitted and the
/// port's characters and spaces go out through the same <c>Settle</c> and
/// <c>CharacterSettled</c> seam the CW tab reads.</item>
/// <item>**The sheet**: <c>MainWindowViewModel.ArbitrationLine</c> writes the
/// switch beside a character only where it is not `arbitrate`. The CW tab never
/// shows a decoder or a switch (HM-REQ-121).</item>
/// </list>
/// </remarks>
public sealed class TheArbitrationEarnsItsPlaceFact
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fact.</summary>
    /// <param name="output">Where the rows are printed.</param>
    public TheArbitrationEarnsItsPlaceFact(ITestOutputHelper output)
        => _output = output;

    /// <summary>The three outputs, in print order.</summary>
    internal static readonly string[] Outputs = { "arbitrated", "ours alone", "port alone" };

    /// <summary>Consecutive characters a live rendering showed, and how their boundary compared with the settled one.</summary>
    /// <param name="Pairs">Distinct pairs that also settled as consecutive characters.</param>
    /// <param name="Differ">Of those, the pairs where some live rendering's boundary differs from the settled one.</param>
    internal sealed record Boundaries(int Pairs, int Differ);

    /// <summary>One recording read three ways on one path.</summary>
    /// <param name="Name">The recording.</param>
    /// <param name="Real">Real, inferred key; or synthetic, exact key.</param>
    /// <param name="Condition">Its own condition row.</param>
    /// <param name="Kind">The key's kind.</param>
    /// <param name="Emitted">The three outputs' characters, in <see cref="Outputs"/> order; null where the port was not run.</param>
    /// <param name="Live">On the live path, the arbitrated output's and ours' boundaries against their own leading edge; null in the harness.</param>
    internal sealed record Three(
        string Name, bool Real, string Condition, CwKeyKind Kind,
        IReadOnlyList<IReadOnlyList<CwCharacter>?> Emitted, IReadOnlyList<Boundaries>? Live);

    /// <summary>
    /// The port's readings as the port alone emits them, at parity.md section 1's
    /// mapping: each character sure, its no-match output a placeholder, and a word
    /// boundary where it printed a space; placed on the clock as
    /// <see cref="CwArbiter.DecideAlone"/> places it.
    /// </summary>
    internal static IReadOnlyList<CwCharacter> PortAlone(IEnumerable<CwSecondReading> readings)
    {
        var list = new List<CwCharacter>();

        foreach (var s in readings)
        {
            var hops = s.HasSpan ? (int)Math.Round((s.End - s.Start) * 1000.0 / CwProbabilisticDecoder.HopMilliseconds) : 0;
            var at = TimeSpan.FromSeconds(double.IsFinite(s.End) ? s.End : 0);

            if (s.WordGapBefore)
            {
                list.Add(new CwCharacter(MorseAlphabet.WordGap, CwConfidence.High, 1, string.Empty, double.NaN, s.WordsPerMinute, at));
            }

            list.Add(new CwCharacter(s.Text, s.Confidence, s.P, s.Pattern, double.NaN, s.WordsPerMinute, at)
            {
                SpanHops = hops,
                Probability = s.P,
            });
        }

        return list;
    }

    /// <summary>The scored stretches for a recording's text, by the rule its own row is scored by.</summary>
    internal static IReadOnlyList<CwScore> ScoresFor(string name, bool real, IReadOnlyList<CwCharacter> settled)
    {
        var reading = CwReading.Of(settled);

        if (real)
        {
            return WhatTheStrayLettersRestOnTests.KeyedRecordings.Single(k => k.Name == name).Score(reading);
        }

        var from = reading.Text.TakeWhile(char.IsWhiteSpace).Count();

        return new[] { SyntheticCq.Whole(reading) with { Start = from } };
    }

    /// <summary>One output of one recording through the one scorer.</summary>
    internal static TheRequirementsAreMeasuredTests.Measured? Score(Three t, int output)
        => t.Emitted[output] is not { } chars
            ? null
            : TheRequirementsAreMeasuredTests.Measure(t.Name, t.Real ? "real" : "synthetic", t.Condition, t.Kind, chars, ScoresFor(t.Name, t.Real, chars));

    /// <summary>A row's figures for one output, by metric, as task 1 fixed them.</summary>
    internal static Dictionary<string, CwFigure> Figures(
        IEnumerable<TheRequirementsAreMeasuredTests.Measured?> measured, int output, bool fifteen,
        Boundaries? live, (int Exact, int Measurable)? spans)
    {
        var stretches = measured.Where(m => m is { NotComputable: null }).SelectMany(m => m!.Stretches).ToList();
        var e = stretches.Select(CwMetrics.SureErrors).ToList();
        var n = stretches.Select(CwMetrics.Invented).ToList();
        var c = stretches.Select(CwMetrics.Coverage).ToList();
        var w = stretches.Select(CwMetrics.WordBoundaries).ToList();
        var dim = stretches.SelectMany(a => a.Steps).Where(s => s.Decoded is { Class: CwSymbolClass.NotSure }).ToList();
        var sent = n.Sum(x => x.Sent);
        var right = c.Sum(x => x.SureRight);
        var f = new Dictionary<string, CwFigure>(StringComparer.Ordinal)
        {
            ["011"] = new(n.Sum(x => x.Count), sent),
            ["010"] = new(e.Sum(x => x.Errors), e.Sum(x => x.SureEmitted)),
            ["012"] = new(right, c.Sum(x => x.Sent)),
            ["081"] = new(w.Sum(x => x.Errors), w.Sum(x => x.WordsSent)),
        };

        if (fifteen)
        {
            f["013"] = new(sent - right, sent);
        }

        // 014 and 083 are not defined for the port (task 1).
        if (output != 2)
        {
            f["014"] = new(dim.Count(s => s.Key == s.Decoded!.Value.Text), dim.Count);

            if (live is not null)
            {
                f["083"] = new(live.Differ, live.Pairs);
            }
        }

        if (spans is { } sp)
        {
            f["084"] = new(sp.Exact, sp.Measurable);
        }

        return f;
    }

    /// <summary>The 15 dB rows HM-REQ-013 and 080 name, as the tree has them.</summary>
    internal static bool Fifteen(string row) => row.Contains(", 15 dB in the passband", StringComparison.Ordinal);

    /// <summary>The rows HM-REQ-084's recordings fall under: real HF, all and sender not stated.</summary>
    internal static bool SpanRow(string row)
        => row == "real HF, all" || row == TheRequirementsAreMeasuredTests.RealCondition(WhatTheNamedWordsReadTests.Spans[0].Recording);

    // --- the harness ---

    /// <summary>All 35 in the harness: each under its own row, the arbiter fed the port's harvested readings.</summary>
    internal static IReadOnlyList<(Three Three, IReadOnlyList<CwSecondReading> Second, BothDecodersAreScoredAlikeTests.Row Row)> Harness()
        => BothDecodersAreScoredAlikeTests.Rows.Select(r =>
            {
                var second = r.Port is null
                    ? Array.Empty<CwSecondReading>()
                    : CwSecondHarvester.Of(WhatEachDecoderKnowsAboutEachCharacterFact.RunPort(r));
                var arbitrated = CwArbiter.Arbitrate(r.Ours.Settled, second, CwVoteTable.For(r.Condition)).Characters;

                return (new Three(r.Name, r.Real, r.Condition, r.Kind,
                    new IReadOnlyList<CwCharacter>?[] { arbitrated, r.Ours.Settled, r.Port?.Settled }, null), second, r);
            })
            .ToList();

    // --- the live path ---

    /// <summary>All 35 through <see cref="CwDecoder"/> as the product runs it: ours alone, and with the second reader and the arbiter.</summary>
    internal static IReadOnlyList<Three> LivePath()
        => WhatTheStrayLettersRestOnTests.KeyedRecordings
            .Select(k => (k.Name, Real: true, Condition: TheRequirementsAreMeasuredTests.RealCondition(k.Name),
                File: Path.Combine(CapturedSignalTests.Folder, k.Name + ".wav"), Hz: 600.0))
            .Concat(SyntheticCq.All.Select(c => (c.Name, Real: false, Condition: TheRequirementsAreMeasuredTests.SyntheticCondition(c),
                File: Path.Combine(SyntheticCq.Folder, c.Name + ".wav"), Hz: SyntheticCq.StartingPitchHz)))
            .Select(x =>
            {
                var audio = WavAudio.Read(x.File);
                var (ours, oursLive, _) = Drive(audio.Samples, audio.SampleRate, x.Hz, false);
                var (arbitrated, arbLive, readings) = Drive(audio.Samples, audio.SampleRate, x.Hz, true);

                return new Three(x.Name, x.Real, x.Condition, x.Real ? CwKeyKind.Inferred : CwKeyKind.Exact,
                    new IReadOnlyList<CwCharacter>?[] { arbitrated, ours, PortAlone(readings) },
                    new[] { arbLive, oursLive });
            })
            .ToList();

    /// <summary>One recording through <see cref="CwDecoder"/> hop by hop, as the metrics feed it, with its leading edge kept.</summary>
    internal static (IReadOnlyList<CwCharacter> Settled, Boundaries Live, IReadOnlyList<CwSecondReading> Readings) Drive(
        float[] samples, int rate, double hz, bool second, CwArbitrationSwitch switchInForce = CwArbitrationSwitch.Arbitrate)
    {
        var decoder = new CwDecoder(rate, hz, second) { Switch = switchInForce };
        var settled = new List<CwCharacter>();
        var readings = new List<CwSecondReading>();
        var shown = new Dictionary<(long, long), (bool Gap, bool NoGap)>();
        var hop = decoder.Tracker.HopSamples;

        decoder.CharacterSettled += settled.Add;
        decoder.SecondRead += readings.Add;
        decoder.LeadingEdge += edge => Shown(edge, shown);

        for (var at = 0L; at + hop <= samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, rate, samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        return (settled, Compare(shown, settled), readings);
    }

    // Every consecutive pair of characters a rendering shows, with whether a boundary stood between them.
    private static void Shown(IReadOnlyList<CwCharacter> edge, Dictionary<(long, long), (bool Gap, bool NoGap)> shown)
    {
        CwCharacter? previous = null;
        var gap = false;

        foreach (var c in edge)
        {
            if (c.IsWordGap)
            {
                gap = true;
                continue;
            }

            if (previous is not null)
            {
                var key = (previous.At.Ticks, c.At.Ticks);
                var was = shown.TryGetValue(key, out var v) ? v : (Gap: false, NoGap: false);

                shown[key] = (was.Gap || gap, was.NoGap || !gap);
            }

            previous = c;
            gap = false;
        }
    }

    // The pairs shown live that settled as consecutive characters, and those whose boundary differs from the settled one.
    private static Boundaries Compare(Dictionary<(long, long), (bool Gap, bool NoGap)> shown, IReadOnlyList<CwCharacter> settled)
    {
        var position = new Dictionary<long, int>();
        var gapBefore = new List<bool>();
        var gap = false;

        foreach (var c in settled)
        {
            if (c.IsWordGap)
            {
                gap = true;
                continue;
            }

            position.TryAdd(c.At.Ticks, gapBefore.Count);
            gapBefore.Add(gap);
            gap = false;
        }

        int pairs = 0, differ = 0;

        foreach (var ((a, b), (sawGap, sawNoGap)) in shown)
        {
            if (!position.TryGetValue(a, out var pa) || !position.TryGetValue(b, out var pb) || pb != pa + 1)
            {
                continue;
            }

            pairs++;

            if (gapBefore[pb] ? sawNoGap : sawGap)
            {
                differ++;
            }
        }

        return new Boundaries(pairs, differ);
    }

    // --- HM-REQ-084 ---

    /// <summary>One named span as an output reads it, by <see cref="WhatTheNamedWordsReadTests"/>' rule: the word, a boundary each side and none inside, every letter sure.</summary>
    internal static (string Text, bool Met) ReadSpan(WhatTheNamedWordsReadTests.Span span, IReadOnlyList<CwCharacter> chars)
    {
        double from = span.FromSeconds, to = span.ToSeconds;
        var inside = Enumerable.Range(0, chars.Count)
            .Where(i => !chars[i].IsWordGap)
            .Where(i =>
            {
                var end = chars[i].At.TotalSeconds;
                var start = end - (chars[i].SpanHops * CwProbabilisticDecoder.HopMilliseconds / 1000.0);

                return start < to && end > from;
            })
            .ToList();

        if (inside.Count == 0)
        {
            return (string.Empty, false);
        }

        int first = inside[0], last = inside[^1];
        var over = chars.Skip(first).Take(last - first + 1).ToList();
        var inner = WhatTheNamedWordsReadTests.AsRead(over);
        var before = first == 0 || chars[first - 1].IsWordGap;
        var after = last == chars.Count - 1 || chars[last + 1].IsWordGap;
        var sure = over.Where(c => !c.IsWordGap).All(c => !c.IsUnreadable && c.Confidence == CwConfidence.High);

        return ((before ? " " : string.Empty) + inner + (after ? " " : string.Empty), inner == span.Word && before && after && sure);
    }

    /// <summary>The span recordings read three ways on both paths: harness and live, each as the three outputs.</summary>
    /// <param name="recording">One of <see cref="WhatTheNamedWordsReadTests.Recordings"/>.</param>
    /// <param name="switched">True for the output emitted under <see cref="CwSwitchTable"/> in place of the arbitrated one (work instruction 466, task 3).</param>
    internal static IReadOnlyDictionary<string, IReadOnlyList<IReadOnlyList<CwCharacter>>> SpanReadings(string recording, bool switched = false)
    {
        var path = Path.Combine(CapturedSignalTests.Folder, recording + ".wav");
        var audio = WavAudio.Read(path);
        var (ours, _, _) = Drive(audio.Samples, audio.SampleRate, 600, false);
        var windows = CwPitchInstrument.Measure(audio.Samples, audio.SampleRate);
        var given = windows.Select(w => w.Hz).OrderBy(h => h).ElementAt(windows.Count / 2);
        var port = new FldigiCwDecoder(given) { TraceDecisions = true };

        port.rx_process(FldigiRateAdapter.ToFldigiRate(audio.Samples, audio.SampleRate));

        var second = CwSecondHarvester.Of(port);
        var condition = TheRequirementsAreMeasuredTests.RealCondition(recording);
        var arbitrated = CwArbiter.Arbitrate(ours, second, CwVoteTable.For(condition),
            switched ? CwSwitchTable.For(condition) : CwArbitrationSwitch.Arbitrate).Characters;
        var (live, _, readings) = Drive(audio.Samples, audio.SampleRate, 600, true, switched ? CwSwitchTable.Live : CwArbitrationSwitch.Arbitrate);

        return new Dictionary<string, IReadOnlyList<IReadOnlyList<CwCharacter>>>(StringComparer.Ordinal)
        {
            ["harness"] = new[] { arbitrated, ours, PortAlone(second) },
            ["live"] = new[] { live, ours, PortAlone(readings) },
        };
    }

    // --- the table ---

    /// <summary>A figure as printed: count over denominator and the rate, or why there is none.</summary>
    internal static string Text(IReadOnlyDictionary<string, CwFigure> f, string metric)
        => !f.TryGetValue(metric, out var x) ? "not compared"
            : !x.Defined ? $"{x.Count} / 0 (not defined)"
            : string.Create(Invariant, $"{x.Count} / {x.Of} ({x.Count / (double)x.Of:0.000})");

    private static string Versus(string metric, IReadOnlyDictionary<string, CwFigure> a, IReadOnlyDictionary<string, CwFigure> b)
        => a.TryGetValue(metric, out var x) && b.TryGetValue(metric, out var y) && CwSwitchTable.Compare(metric, x, y) is { } s
            ? s < 0 ? "better" : s > 0 ? "WORSE" : "equal"
            : "not compared";

    private static string SwitchWord(CwArbitrationSwitch s) => s switch
    {
        CwArbitrationSwitch.OursAlone => "ours alone",
        CwArbitrationSwitch.PortAlone => "port alone",
        _ => "arbitrate",
    };

    private static string Reader(CwReader r) => r == CwReader.Ours ? "ours" : "port";

    /// <summary>One path's rows: the label, the key's kind, and the three outputs' figures.</summary>
    internal sealed record PathRow(string Label, string Kind, int Recordings, IReadOnlyList<Dictionary<string, CwFigure>> Figures)
    {
        public CwSwitchVerdict Verdict => CwSwitchTable.Choose(Figures[0], Figures[1], Figures[2]);
    }

    /// <summary>A path's rows: the two sets, then each condition in ordinal order, the three outputs' figures on each; the named spans' counts under key "x".</summary>
    internal static IReadOnlyList<PathRow> Rows(IReadOnlyList<Three> threes, IReadOnlyDictionary<string, (int Exact, int Measurable)[]> spans)
    {
        var rows = new List<PathRow>();

        foreach (var real in new[] { true, false })
        {
            var set = threes.Where(t => t.Real == real).ToList();
            var groups = new List<(string Label, List<Three> Members)> { (real ? "real HF, all" : "synthetic, all", set) };

            groups.AddRange(set.GroupBy(t => t.Condition).OrderBy(g => g.Key, StringComparer.Ordinal).Select(g => (g.Key, g.ToList())));

            foreach (var (label, members) in groups)
            {
                var figures = Enumerable.Range(0, 3).Select(o =>
                {
                    var measured = members.Select(t => Score(t, o)).ToList();
                    var live = members.All(t => t.Live is not null) && o < 2
                        ? new Boundaries(members.Sum(t => t.Live![o].Pairs), members.Sum(t => t.Live![o].Differ))
                        : null;
                    (int, int)? sp = SpanRow(label) && spans.TryGetValue("x", out var s) ? s[o] : null;

                    return Figures(measured, o, Fifteen(label), live, sp);
                }).ToList();

                rows.Add(new PathRow(label, real ? "inferred" : "exact", members.Count, figures));
            }
        }

        return rows;
    }

    /// <remarks>
    /// Work instruction 466 task 2: per condition row, the arbitrated output, ours
    /// alone and the port alone on every metric task 1 fixed, on the harness and on
    /// the live path; per row whether the arbitration loses, the better decoder
    /// alone and the switch that follows, set by the path rule; HM-REQ-084's spans
    /// as each reads them; HM-REQ-013, 080 and 014's verdicts. Writes
    /// `docs/phase-requirements/arbitration.md`. Asserts only that all 35 were read
    /// on both paths.
    /// </remarks>
    [Fact]
    public void TheThreeWayTable()
    {
        var harness = Harness();

        foreach (var (t, second, r) in harness)
        {
            if (r.Port is null)
            {
                Print($"check | {t.Name} | port not run: {r.PortNotRun}");
                continue;
            }

            var mine = CwMetrics.Symbols(PortAlone(second)).Reverse().SkipWhile(s => s.Class == CwSymbolClass.WordGap).Reverse();
            var theirs = CwMetrics.Symbols(r.Port.Settled).Reverse().SkipWhile(s => s.Class == CwSymbolClass.WordGap).Reverse();

            Print($"check | {t.Name} | the port alone from its harvested readings against parity.md's mapping of what it printed | {(mine.SequenceEqual(theirs) ? "same symbols" : "DIFFERS")}");
        }

        var live = LivePath();

        // HM-REQ-084 on both paths.
        var spanCounts = new Dictionary<string, (int, int)[]>(StringComparer.Ordinal);
        var spanLines = new List<string>();

        foreach (var path in new[] { "harness", "live" })
        {
            spanCounts[path] = new (int, int)[3];
        }

        foreach (var recording in WhatTheNamedWordsReadTests.Recordings)
        {
            var readings = SpanReadings(recording);

            foreach (var span in WhatTheNamedWordsReadTests.Spans.Where(s => s.Recording == recording))
            {
                foreach (var path in new[] { "harness", "live" })
                {
                    for (var o = 0; o < 3; o++)
                    {
                        var (text, met) = ReadSpan(span, readings[path][o]);
                        var (exact, measurable) = spanCounts[path][o];

                        spanCounts[path][o] = (exact + (met ? 1 : 0), measurable + 1);
                        spanLines.Add($"span084 | {path} | {Outputs[o]} | {span.Word} | {recording} | reads `{text}` | {(met ? "met" : "not met")}");
                    }
                }
            }
        }

        foreach (var (word, recording, why, _) in WhatTheNamedWordsReadTests.NotHere)
        {
            spanLines.Add($"span084 | both | all three | {word} | {recording} | {why}");
        }

        var harnessRows = Rows(harness.Select(h => h.Three).ToList(), new Dictionary<string, (int, int)[]> { ["x"] = spanCounts["harness"] });
        var liveRows = Rows(live, new Dictionary<string, (int, int)[]> { ["x"] = spanCounts["live"] });

        foreach (var (path, rows) in new[] { ("harness", harnessRows), ("live", liveRows) })
        {
            Print($"threeway | path | row | key | recordings | metric | arbitrated | ours alone | port alone | arbitrated against ours | arbitrated against the port | ours against the port");

            foreach (var row in rows)
            {
                foreach (var m in CwSwitchTable.Order.Where(m => row.Figures.Any(f => f.ContainsKey(m))))
                {
                    Print($"threeway | {path} | {row.Label} | {row.Kind} | {row.Recordings} | {m} | {Text(row.Figures[0], m)} | {Text(row.Figures[1], m)} | {Text(row.Figures[2], m)} | "
                          + $"{Versus(m, row.Figures[0], row.Figures[1])} | {Versus(m, row.Figures[0], row.Figures[2])} | {Versus(m, row.Figures[1], row.Figures[2])}");
                }

                var v = row.Verdict;

                Print($"verdict | {path} | {row.Label} | {(v.Loses ? "loses on " + string.Join(", ", v.LostOn) : "does not lose")} | better alone {Reader(v.Better)} by {v.BetterBy} | switch on this path {SwitchWord(v.Switch)}");
            }
        }

        // The path rule (466 DECIDED (5)).
        var decided = harnessRows.Select(h =>
        {
            var fromLive = h.Label == CwVoteTable.LiveCondition;
            var row = fromLive ? liveRows.Single(l => l.Label == h.Label) : h;

            return (h.Label, Path: fromLive ? "live" : "harness", row.Verdict);
        }).ToList();

        foreach (var (label, path, v) in decided)
        {
            Print($"switch | {label} | {SwitchWord(v.Switch)} | set from the {path} path | {(v.Loses ? "loses on " + string.Join(", ", v.LostOn) : "does not lose")} | better alone {Reader(v.Better)} by {v.BetterBy}");
        }

        foreach (var line in spanLines)
        {
            Print(line);
        }

        // HM-REQ-013, 080 and 014 per row.
        var verdictLines = new List<string>();

        foreach (var (path, rows) in new[] { ("harness", harnessRows), ("live", liveRows) })
        {
            foreach (var row in rows)
            {
                for (var o = 0; o < 3; o++)
                {
                    var f = row.Figures[o];
                    var parts = new List<string>();

                    if (Fifteen(row.Label))
                    {
                        parts.Add($"013 {(f["013"].Count == 0 && f["010"] is { Defined: true, Count: 0 } ? "met" : "not met")}");
                        parts.Add($"080 {(f["081"].Count == 0 ? "met" : "not met")}");
                    }

                    parts.Add(f.TryGetValue("014", out var d)
                        ? d.Defined ? $"014 {(d.Count * 10 >= d.Of * 7 ? "met" : "not met")} ({Text(f, "014")})" : "014 no dim emitted"
                        : "014 not defined (emits no dim)");

                    var line = $"req | {path} | {row.Label} | {Outputs[o]} | {string.Join(" | ", parts)}";

                    verdictLines.Add(line);
                    Print(line);
                }
            }
        }

        var md = Markdown(harnessRows, liveRows, decided, spanLines, verdictLines, harness.Count(h => h.Row.Port is not null));

        File.WriteAllText(Path.Combine(CwToneSurveyTests.RepositoryRoot(), "docs", "phase-requirements", "arbitration.md"), md);

        Assert.Equal(BothDecodersAreScoredAlikeTests.Rows.Count, live.Count);
    }

    /// <remarks>
    /// Work instruction 466 task 3, item 4: the output emitted in the harness under
    /// <see cref="CwSwitchTable.For"/>, each recording under its own row, printed in
    /// task 0's save format, so each row's text is compared with task 0's save of
    /// the arbitrated transcript line by line. Asserts only that all 35 were read.
    /// </remarks>
    [Fact]
    public void TheEmittedTranscriptWithClassAndP()
    {
        var harness = Harness();

        foreach (var (t, second, r) in harness)
        {
            var s = CwSwitchTable.For(r.Condition);
            var emitted = CwArbiter.Arbitrate(r.Ours.Settled, second, CwVoteTable.For(r.Condition), s).Characters;

            Print($"switch in force | {t.Name} | {SwitchWord(s)}");

            for (var i = 0; i < emitted.Count; i++)
            {
                var c = emitted[i];

                Print(string.Create(Invariant,
                    $"save | ours | {t.Name} | {i} | {WhereTheTwoReadingsMeetFact.Visible(c.Text)} | {WhereTheTwoReadingsMeetFact.ClassOf(c)} | {WhereTheTwoReadingsMeetFact.P(c.Probability)}"));
            }
        }

        Assert.Equal(BothDecodersAreScoredAlikeTests.Rows.Count, harness.Count);
    }

    private static string Markdown(
        IReadOnlyList<PathRow> harnessRows, IReadOnlyList<PathRow> liveRows,
        IReadOnlyList<(string Label, string Path, CwSwitchVerdict Verdict)> decided,
        IReadOnlyList<string> spanLines, IReadOnlyList<string> verdictLines, int portRun)
    {
        var md = new StringBuilder();

        md.Append("# Arbitration: the arbitrated transcript, ours alone and the port alone (HM-REQ-128)\n\n");
        md.Append("Written by `TheArbitrationEarnsItsPlaceFact.TheThreeWayTable` (work instruction 466, task 2; PHASE_PLAN.md 9.7). ");
        md.Append("The metric list and the loss, better-decoder and path rules were fixed in that fact's header at `9eef6850`, before any ");
        md.Append("three-way figure was computed (V-14), and are `CwSwitchTable.Choose`. Every figure goes through ");
        md.Append("`TheRequirementsAreMeasuredTests.Measure` and the same `CwMetrics` calls, each with its key's kind (V-11, V-13). ");
        md.Append($"The port was run on {portRun} of {harnessRows.Where(r => r.Label.EndsWith(", all", StringComparison.Ordinal)).Sum(r => r.Recordings)} recordings in the harness.\n\n");

        md.Append("## 1. The metrics and the rules\n\n");
        md.Append("- **011** MET-INVENTED, **010** MET-CER-SURE, **012** MET-COVERAGE (sure and right over sent), **081** MET-WBE: all three outputs. ");
        md.Append("Lower is better except coverage.\n");
        md.Append("- **013** sent characters not emitted sure and correct, over sent: the 15 dB rows only (synthetic; not shown to be CH-AWGN).\n");
        md.Append("- **014** dim right over dim emitted, higher better: not defined for the port, which emits no dim (parity.md section 1).\n");
        md.Append("- **083** live against settled boundaries, over distinct consecutive pairs the leading edge showed: live path only; not defined for the port, which has one rendering.\n");
        md.Append("- **084** named spans read exactly (the word, a boundary each side, none inside, every letter sure), over the 3 of 6 whose audio is in the tree: rows real HF, all and real HF, sender not stated.\n");
        md.Append("- **015 and 082** are properties of all three (the class travels with each character when it is emitted; boundary errors are scored apart by `CwMetrics.WordBoundaries`).\n");
        md.Append("- **Loses**: strictly worse than either decoder alone on any metric defined for both, a rate compared as a rate. ");
        md.Append("**Better alone**: no worse on every metric defined for both, else the first difference in the order 011, 010, 013, 012, 014, 081, 083, 084, ours on a full tie. ");
        md.Append("**Switch**: no loss, arbitrate; else the better alone. **Path**: real HF, all from the live path (the product runs under it); every other row from the harness.\n\n");

        md.Append("## 2. The switch per condition\n\n");
        md.Append("| condition | set from | arbitration loses on | better alone | by | switch |\n|---|---|---|---|---|---|\n");

        foreach (var (label, path, v) in decided)
        {
            md.Append($"| {label} | {path} | {(v.Loses ? string.Join(", ", v.LostOn) : "none")} | {Reader(v.Better)} | {v.BetterBy} | **{SwitchWord(v.Switch)}** |\n");
        }

        foreach (var (title, rows) in new[] { ("3. The harness, each recording under its own row", harnessRows), ("4. The live path, CwDecoder with the second reader, grouped by the same rows", liveRows) })
        {
            md.Append($"\n## {title}\n\n");
            md.Append("| condition | key | recordings | metric | arbitrated | ours alone | port alone | arbitrated against ours | against the port |\n|---|---|---|---|---|---|---|---|---|\n");

            foreach (var row in rows)
            {
                foreach (var m in CwSwitchTable.Order.Where(m => row.Figures.Any(f => f.ContainsKey(m))))
                {
                    md.Append($"| {row.Label} | {row.Kind} | {row.Recordings} | {m} | {Text(row.Figures[0], m)} | {Text(row.Figures[1], m)} | {Text(row.Figures[2], m)} | ");
                    md.Append($"{Versus(m, row.Figures[0], row.Figures[1])} | {Versus(m, row.Figures[0], row.Figures[2])} |\n");
                }

                var v = row.Verdict;

                md.Append($"| {row.Label} | | | **verdict** | {(v.Loses ? "loses on " + string.Join(", ", v.LostOn) : "does not lose")} | better alone: {Reader(v.Better)} by {v.BetterBy} | | switch on this path: {SwitchWord(v.Switch)} | |\n");
            }
        }

        md.Append("\n## 5. HM-REQ-084's named spans as each reads them\n\n");

        foreach (var line in spanLines)
        {
            md.Append("- ").Append(line.Replace("span084 | ", string.Empty, StringComparison.Ordinal)).Append('\n');
        }

        md.Append("\n## 6. HM-REQ-013, 080 and 014 per row\n\n");

        foreach (var line in verdictLines)
        {
            md.Append("- ").Append(line.Replace("req | ", string.Empty, StringComparison.Ordinal)).Append('\n');
        }

        md.Append("\n## 7. What the table does not prove\n\n");
        md.Append("- **Every real key is inferred** (V-13), and **the synthetic set is never sole evidence** (CLAUDE.md 12.5).\n");
        md.Append("- **No row is a CH-* condition**, so no must-tier verdict here is the requirement's own; each is the tree's nearest row, labelled.\n");
        md.Append("- **The port's figures are at parity.md section 1's all-sure mapping**; its MET-CER-SURE is its whole character error.\n");
        md.Append("- **Who votes is `CwVoteTable`'s**, calibration.md section 2's held-out verdicts, unchanged here; the switch sits beside it and does not edit it.\n");

        return md.ToString();
    }

    private void Print(string line) => _output.WriteLine(line);
}
