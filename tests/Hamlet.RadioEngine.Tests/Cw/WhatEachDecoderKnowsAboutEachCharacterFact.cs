using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Cw.Second;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// What each decoder already knows about each character it emits, beside the
/// scorer's verdict on it, over every keyed recording and every synthetic case
/// (work instruction 464, task 1; HM-REQ-124; PHASE_PLAN.md 9.5).
/// </summary>
/// <remarks>
/// <para>**A PRINTER. IT ASSERTS NOTHING ABOUT EITHER DECODER.** Nothing under
/// `src` changes for it. It prints one row per emitted character, then how each
/// feature separates right from wrong or added, by quintile. No map is fitted
/// here and no calibration figure is printed: the features and the map's form are
/// fixed from this print before any calibration number is seen (V-14).</para>
/// <para>**BOTH DECODERS COME FROM ONE HARNESS** (V-11):
/// <see cref="BothDecodersAreScoredAlikeTests.Rows"/>, whose stretches and scorer
/// the parity table is written from. Ours' features are the character's own
/// fields. The port's are read only from what it already exposes, its
/// <see cref="FldigiCwDecoder.KeyEvents"/> and
/// <see cref="FldigiCwDecoder.Emissions"/>, on a second run of the port over the
/// same input, checked to print exactly what the harness's run printed. Nothing
/// under `Cw/Second/` is changed to expose anything (HM-REQ-122, 129).</para>
/// <para>**RIGHT IS THE SCORER'S VERDICT** (R85): a sure or dim character aligned
/// to the same key character is right, to a different one wrong, to none added.
/// A placeholder is unscored, as MET-CAL's placeholder bin is, and so is anything
/// outside every scored stretch. **EVERY REAL KEY IS INFERRED** (R61, V-13);
/// **EVERY SYNTHETIC KEY IS EXACT**.</para>
/// </remarks>
public sealed class WhatEachDecoderKnowsAboutEachCharacterFact
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private static readonly Lazy<IReadOnlyList<CharacterRow>> AllCharacters = new(Build);

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fact.</summary>
    /// <param name="output">Where the rows are printed.</param>
    public WhatEachDecoderKnowsAboutEachCharacterFact(ITestOutputHelper output)
        => _output = output;

    /// <summary>One emitted character of one decoder, with its verdict and features.</summary>
    /// <param name="Decoder">ours or port.</param>
    /// <param name="Recording">The recording or case.</param>
    /// <param name="Real">Real, inferred key; or synthetic, exact key.</param>
    /// <param name="Condition">Its condition as parity.md section 3 states it.</param>
    /// <param name="Kind">The key's kind.</param>
    /// <param name="Index">The character's index in what the decoder put out.</param>
    /// <param name="Text">What was emitted.</param>
    /// <param name="Class">sure, dim or placeholder.</param>
    /// <param name="Verdict">right, wrong, added, placeholder or outside (no scored stretch covers it).</param>
    /// <param name="Character">The character as the scorer took it.</param>
    /// <param name="MarginLlr">Ours: <see cref="CwCharacter.MarginLlr"/>. NaN for the port.</param>
    /// <param name="MarginShare">Ours: <see cref="CwCharacter.MarginShareForRecord"/>. NaN for the port.</param>
    /// <param name="LevelDb">The port: 20 log10(sig_avg / noise_floor) at the character's last up event. NaN for ours.</param>
    /// <param name="TimingDits">The port: the least distance of any element's length from two_dots, in two_dots / 2. NaN for ours.</param>
    /// <param name="Elements">The port: the key events found for the character. Ours: its pattern's length.</param>
    /// <param name="Emission">The port's emission, or null for ours.</param>
    internal sealed record CharacterRow(
        string Decoder, string Recording, bool Real, string Condition, CwKeyKind Kind, int Index,
        string Text, string Class, string Verdict, CwCharacter Character,
        double MarginLlr, double MarginShare, double LevelDb, double TimingDits, int Elements,
        FldigiCwEmission? Emission)
    {
        /// <summary>Right, wrong or added: the characters calibration is measured on.</summary>
        public bool Scored => Verdict is "right" or "wrong" or "added";

        /// <summary>Right, as the map's target.</summary>
        public bool Right => Verdict == "right";
    }

    /// <summary>Every emitted character of both decoders on all 35, once per process; a character two stretches cover appears once per stretch.</summary>
    internal static IReadOnlyList<CharacterRow> Characters => AllCharacters.Value;

    /// <summary>Every character's row, and each feature's separation of right from wrong or added (HM-REQ-124).</summary>
    [Fact]
    public void EveryCharacterWithWhatItsDecoderKnew()
    {
        var rows = Characters;

        Print("source | ours | MarginLlr: CwCharacter.MarginLlr, set by CwProbabilisticStream.cs:768 from CwProbabilisticDecoder.RivalMargin, the path's score for the reading emitted less its best rival's, natural log; MarginShareForRecord beside it, MarginLlr over the span's own log-likelihood ratio");
        Print("source | port | level: 20 log10(sig_avg / noise_floor) at the character's last up event - sig_avg cw.cxx:610, noise_floor 612-616, the same ratio fldigi's own squelch metric reads at 635-636; FldigiCwKeyEvent.SigAvg and .NoiseFloor");
        Print("source | port | timing: min over the character's elements of |element - two_dots| / (two_dots / 2) - the element's length cw.cxx:811-812, the dot/dash split 847 (element_usec <= two_dots), fldigi's dit two_dots / 2 at 502; FldigiCwKeyEvent.Element and .TwoDots at each up event");
        Print("source | port | elements: the up events found for the character, the last Representation.Length up events before its emission, the last carrying the emission's representation; FldigiCwEmission.Representation, FldigiCwKeyEvent.Representation");
        Print("source | port | order: TraceDecisions on, and the key events and emissions interleaved by the decision row each was raised in (FldigiCwDecisionRow.Events, .Printed), because the port's filter hands out 1024 samples at once and InputSample alone cannot order an emission against the next character's first events; the second run's text is checked equal to the harness's");

        foreach (var decoder in new[] { "ours", "port" })
        {
            var mine = rows.Where(r => r.Decoder == decoder).ToList();

            Print(string.Create(Invariant,
                $"count | {decoder} | rows {mine.Count} | right {mine.Count(r => r.Verdict == "right")} | wrong {mine.Count(r => r.Verdict == "wrong")} | added {mine.Count(r => r.Verdict == "added")} | placeholder {mine.Count(r => r.Verdict == "placeholder")} | outside every scored stretch {mine.Count(r => r.Verdict == "outside")} | dim {mine.Count(r => r.Class == "dim")}"));

            foreach (var real in new[] { true, false })
            {
                var set = mine.Where(r => r.Real == real).ToList();

                Print(string.Create(Invariant,
                    $"count | {decoder} | {(real ? "real, inferred" : "synthetic, exact")} | scored {set.Count(r => r.Scored)} (sure {set.Count(r => r.Scored && r.Class == "sure")}, dim {set.Count(r => r.Scored && r.Class == "dim")}) | right {set.Count(r => r.Right)} | outside {set.Count(r => r.Verdict == "outside")}"));
            }
        }

        var twice = rows.GroupBy(r => (r.Decoder, r.Recording, r.Index)).Count(g => g.Count() > 1);
        Print($"count | characters two scored stretches cover, each scored once per stretch as the metrics count them | {twice}");

        Print("row | decoder | recording | key | condition | index | text | class | verdict | MarginLlr | MarginShare | level dB | timing dits | elements");

        foreach (var r in rows)
        {
            Print(string.Create(Invariant,
                $"row | {r.Decoder} | {r.Recording} | {CwMetrics.KindWord(r.Kind)} | {r.Condition} | {r.Index} | {r.Text} | {r.Class} | {r.Verdict} | {F(r.MarginLlr)} | {F(r.MarginShare)} | {F(r.LevelDb)} | {F(r.TimingDits)} | {r.Elements}"));
        }

        var features = new (string Decoder, string Name, Func<CharacterRow, double> Read)[]
        {
            ("ours", "MarginLlr", r => r.MarginLlr),
            ("ours", "MarginShare", r => r.MarginShare),
            ("ours", "elements", r => r.Elements),
            ("port", "level dB", r => r.LevelDb),
            ("port", "timing dits", r => r.TimingDits),
            ("port", "elements", r => r.Elements),
        };

        Print("separation | decoder | feature | set | quintile | from | to | right | wrong | added | right share");

        foreach (var (decoder, name, read) in features)
        {
            foreach (var set in new[] { "all", "real", "synthetic" })
            {
                var scored = rows.Where(r => r.Decoder == decoder && r.Scored
                    && (set == "all" || r.Real == (set == "real"))).ToList();
                var valued = scored.Where(r => !double.IsNaN(read(r))).OrderBy(read).ToList();

                for (var q = 0; q < 5; q++)
                {
                    var from = q * valued.Count / 5;
                    var to = (q + 1) * valued.Count / 5;
                    var part = valued.Skip(from).Take(to - from).ToList();

                    if (part.Count == 0)
                    {
                        Print($"separation | {decoder} | {name} | {set} | {q + 1} | empty");
                        continue;
                    }

                    Print(string.Create(Invariant,
                        $"separation | {decoder} | {name} | {set} | {q + 1} | {F(read(part[0]))} | {F(read(part[^1]))} | {part.Count(r => r.Verdict == "right")} | {part.Count(r => r.Verdict == "wrong")} | {part.Count(r => r.Verdict == "added")} | {part.Count(r => r.Right) / (double)part.Count:0.000}"));
                }

                var nan = scored.Where(r => double.IsNaN(read(r))).ToList();

                Print(string.Create(Invariant,
                    $"separation | {decoder} | {name} | {set} | NaN | {nan.Count} scored | right {nan.Count(r => r.Right)} | wrong {nan.Count(r => r.Verdict == "wrong")} | added {nan.Count(r => r.Verdict == "added")}"));
            }
        }

        foreach (var (decoder, name, read) in features)
        {
            foreach (var g in rows.Where(r => r.Decoder == decoder && double.IsNaN(read(r)))
                         .GroupBy(r => NanWhy(r, name)).OrderBy(g => g.Key, StringComparer.Ordinal))
            {
                Print($"nan | {decoder} | {name} | {g.Count()} rows, {g.Count(r => r.Scored)} scored | {g.Key}");
            }

            var infinite = rows.Where(r => r.Decoder == decoder && double.IsInfinity(read(r))).ToList();

            if (infinite.Count > 0)
            {
                Print($"infinite | {decoder} | {name} | {infinite.Count} rows, {infinite.Count(r => r.Scored)} scored, {infinite.Count(r => r.Right)} right | patterns {string.Join(", ", infinite.Select(r => r.Character.Pattern).Distinct())} | rival reading {string.Join(", ", infinite.Select(r => r.Character.RivalReading ?? "none").Distinct())}");
            }
        }

        // The map, fixed here from the print above and before any calibration number was seen (V-14, 464 DECIDED (2)).
        Print("map | both | p = 1 / (1 + e^-(a + b.x)), fitted by maximum likelihood (Newton's method from zero, at most 100 steps, stopping under 1e-10, 1e-9 on the Hessian's diagonal only to keep it invertible) to right = 1 and wrong or added = 0, over every scored character of the real and synthetic sets together, each scored once per stretch as the metrics count it; one map per decoder, never one per condition; placeholders and characters outside every stretch are not in the fit");
        Print("map | ours | one feature, MarginLlr, taken as x = sign(m) ln(1 + |m|): the raw margin runs from under 1 to the hundreds and to +Infinity, the port's level is already a logarithm (dB), and the compression is monotone, so it orders characters exactly as MarginLlr does");
        Print("map | ours | a margin that is not finite - +Infinity, the 'infinite' line above (short letters, most with no rival reading on the lattice), or NaN, which no row carries - takes one constant p, the share right of the scored non-finite characters of the fit's pool (the constant's own maximum likelihood); where the pool holds none, the pool's share right");
        Print("map | port | two features, the level margin x1 = 20 log10(sig_avg / noise_floor) in dB at the character's last up event (cw.cxx:610, 612-616; fldigi's own squelch metric reads the same ratio at 635-636) and the timing margin x2 = min over its elements of |element - two_dots| / (two_dots / 2) (cw.cxx:811-812, 847, 502), both read from FldigiCwKeyEvent outside Cw/Second: p = 1 / (1 + e^-(a + b.x1 + c.x2)); both are readable on every character here");
        Print("map | port | a character where either feature is not finite takes one constant p, the share right of the pool's scored characters so placed, else the pool's share right; the port itself carries no confidence (fldigi has none) and nothing under Cw/Second changes");
        Print("map | port | the timing margin's separation above is not monotone: right share falls at both ends, near the split and past one dit from it; the form is the instruction's, linear in each feature, and is kept, not bent to fit the print");
        Print("map | ours | dim characters are scored like sure ones; ours emits none on the 35, so every scored character of ours is sure");
    }

    /// <summary>Why a feature is NaN on a row, read from the row itself.</summary>
    private static string NanWhy(CharacterRow r, string feature)
    {
        if (r.Decoder == "ours")
        {
            if (r.Class == "placeholder")
            {
                return "a placeholder: the stream sets no rival margin on a pattern the alphabet does not know";
            }

            if (double.IsNaN(r.Character.MarginLlr))
            {
                return $"MarginLlr NaN on a named character (rival reading {(r.Character.RivalReading ?? "none")})";
            }

            return "MarginShare NaN: the span's own ratio is NaN or under 1e-6 in magnitude";
        }

        return r.Elements == 0
            ? "no key events found for the character: the last up event before its emission does not carry its representation"
            : "the level or timing could not be formed from the events found";
    }

    private static IReadOnlyList<CharacterRow> Build()
    {
        var rows = new List<CharacterRow>();

        foreach (var r in BothDecodersAreScoredAlikeTests.Rows)
        {
            var ours = Verdicts(r.OursMeasured);

            for (var i = 0; i < r.Ours.Settled.Count; i++)
            {
                var c = r.Ours.Settled[i];

                if (c.IsWordGap)
                {
                    continue;
                }

                foreach (var v in ours[i])
                {
                    rows.Add(new CharacterRow("ours", r.Name, r.Real, r.Condition, r.Kind, i, c.Text, ClassOf(c), v, c,
                        c.MarginLlr, c.MarginShareForRecord, double.NaN, double.NaN, c.Pattern.Length, null));
                }
            }

            if (r.Port is null || r.PortMeasured is null)
            {
                continue;
            }

            var port = RunPort(r);
            var ups = UpsBeforeEachEmission(port);
            var verdicts = Verdicts(r.PortMeasured);

            for (var i = 0; i < r.Port.Settled.Count; i++)
            {
                var c = r.Port.Settled[i];

                if (c.IsWordGap)
                {
                    continue;
                }

                var e = port.Emissions[i];
                var (level, timing, elements) = PortFeatures(ups[i], e);

                foreach (var v in verdicts[i])
                {
                    rows.Add(new CharacterRow("port", r.Name, r.Real, r.Condition, r.Kind, i, c.Text, ClassOf(c), v, c,
                        double.NaN, double.NaN, level, timing, elements, e));
                }
            }
        }

        return rows;
    }

    /// <summary>
    /// The port over the same input the harness gave it, with
    /// <see cref="FldigiCwDecoder.TraceDecisions"/> on, its text checked against
    /// the harness's run.
    /// </summary>
    /// <remarks>
    /// The trace is on because the port's filter hands out 1024 samples at a time,
    /// so a character's emission and the next character's first key events can
    /// carry the same <see cref="FldigiCwKeyEvent.InputSample"/>; only the
    /// decision rows put the two lists in their true order. The trace records and
    /// changes nothing the receiver reads, and the check below proves it here.
    /// </remarks>
    internal static FldigiCwDecoder RunPort(BothDecodersAreScoredAlikeTests.Row r)
    {
        var audio = WavAudio.Read(Path.Combine(CwToneSurveyTests.RepositoryRoot(), r.AudioFile));
        var port = new FldigiCwDecoder(r.GivenPitchHz) { TraceDecisions = true };

        port.rx_process(FldigiRateAdapter.ToFldigiRate(audio.Samples, audio.SampleRate));

        if (port.Text != r.Port!.Raw || port.Emissions.Count != r.Port.Settled.Count)
        {
            throw new InvalidOperationException($"{r.Name}: the second run of the port printed differently from the harness's");
        }

        return port;
    }

    /// <summary>
    /// For each emission, the up events that came before it, in the order the
    /// receiver raised them: the key events and emissions interleaved by the
    /// decision rows each was raised in.
    /// </summary>
    /// <param name="port">A port run with its decisions traced.</param>
    /// <returns>One list per emission, oldest first, of every up event since the emission before it.</returns>
    internal static IReadOnlyList<IReadOnlyList<FldigiCwKeyEvent>> UpsBeforeEachEmission(FldigiCwDecoder port)
    {
        var result = new List<IReadOnlyList<FldigiCwKeyEvent>>();
        var ups = new List<FldigiCwKeyEvent>();
        var k = 0;

        foreach (var row in port.Decisions)
        {
            foreach (var token in row.Events.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (token is not ("down" or "up" or "spike" or "overflow"))
                {
                    continue;
                }

                var ev = port.KeyEvents[k++];

                if (ev.Kind != token)
                {
                    throw new InvalidOperationException($"key event {k - 1} is {ev.Kind} where the decision row says {token}");
                }

                if (ev.Kind == "up")
                {
                    ups.Add(ev);
                }
            }

            if (row.Printed.Length > 0)
            {
                result.Add(ups);
                ups = new List<FldigiCwKeyEvent>();
            }
        }

        if (k != port.KeyEvents.Count || result.Count != port.Emissions.Count)
        {
            throw new InvalidOperationException($"the decision rows account for {k} of {port.KeyEvents.Count} key events and {result.Count} of {port.Emissions.Count} emissions");
        }

        return result;
    }

    /// <summary>
    /// The port's level and timing margins for one emission, read from its key
    /// events only (cw.cxx:610-616, 811-812, 847, 502).
    /// </summary>
    /// <param name="upsBefore">The up events since the emission before it, oldest first.</param>
    /// <param name="e">The emission.</param>
    /// <returns>The level in dB, the timing margin in fldigi's dits, and the elements found; NaN and nought where none were found.</returns>
    internal static (double LevelDb, double TimingDits, int Elements) PortFeatures(
        IReadOnlyList<FldigiCwKeyEvent> upsBefore, FldigiCwEmission e)
    {
        var want = e.Representation.Length;
        var ups = upsBefore.Skip(Math.Max(0, upsBefore.Count - want)).ToList();

        if (want == 0 || ups.Count != want || ups[^1].Representation != e.Representation)
        {
            return (double.NaN, double.NaN, 0);
        }

        var last = ups[^1];
        var level = last.NoiseFloor > 0 && last.SigAvg > 0
            ? 20 * Math.Log10(last.SigAvg / last.NoiseFloor)
            : double.NaN;
        var timing = ups.Min(u => u.TwoDots > 0 ? Math.Abs(u.Element - u.TwoDots) / (u.TwoDots / 2.0) : double.NaN);

        return (level, timing, ups.Count);
    }

    /// <summary>For each settled character, the scorer's verdict once per scored stretch covering it, or outside.</summary>
    private static List<string>[] Verdicts(TheRequirementsAreMeasuredTests.Measured m)
    {
        var verdicts = m.Settled.Select(_ => new List<string>()).ToArray();

        if (m.NotComputable is null)
        {
            for (var s = 0; s < m.Stretches.Count; s++)
            {
                var covered = TheRequirementsAreMeasuredTests.Covered(m.Settled, m.Scores[s]).Where(c => !c.IsWordGap).ToList();
                var d = 0;

                foreach (var st in m.Stretches[s].Steps)
                {
                    if (st.Decoded is not { } sym)
                    {
                        continue;
                    }

                    var i = IndexOf(m.Settled, covered[d++]);

                    verdicts[i].Add(sym.Class == CwSymbolClass.Placeholder ? "placeholder"
                        : st.Key is null ? "added"
                        : st.Key == sym.Text ? "right"
                        : "wrong");
                }
            }
        }

        for (var i = 0; i < verdicts.Length; i++)
        {
            if (verdicts[i].Count == 0)
            {
                verdicts[i].Add(m.Settled[i].IsUnreadable ? "placeholder" : "outside");
            }
        }

        return verdicts;
    }

    private static string ClassOf(CwCharacter c) => CwSymbol.Of(c).Class switch
    {
        CwSymbolClass.Sure => "sure",
        CwSymbolClass.NotSure => "dim",
        _ => "placeholder",
    };

    private static int IndexOf(IReadOnlyList<CwCharacter> list, CwCharacter c)
    {
        for (var i = 0; i < list.Count; i++)
        {
            if (ReferenceEquals(list[i], c))
            {
                return i;
            }
        }

        return -1;
    }

    private static string F(double x) => double.IsNaN(x) ? "NaN" : x.ToString("0.###", Invariant);

    private void Print(string line) => _output.WriteLine(line);
}
