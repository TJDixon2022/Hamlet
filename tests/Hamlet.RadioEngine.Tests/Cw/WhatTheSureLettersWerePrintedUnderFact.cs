using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Every sure letter over the scored stretches with the pitch's and the speed's
/// proof state at the moment it was emitted, and the sure letters inside and
/// outside acquiring counted right beside wrong (work instruction 470, task 1;
/// PHASE_PLAN.md 2.4; HM-REQ-102, HM-REQ-010).
/// </summary>
/// <remarks>
/// <para>**A PRINTER. IT ASSERTS NOTHING.**</para>
/// <para>**ACQUIRING IS `PARKED.md`'s RESOLVED LINE OF 2026-09-26**: from the first
/// keyed element at a tracked pitch until both the pitch (HM-REQ-093) and the speed
/// (HM-REQ-034) carry proof state proved; while either is hypothesis or none, no
/// character is emitted sure. The states are the tree's,
/// <see cref="CwToneTracker.PitchProof"/> and <see cref="CwDecoder.SpeedProof"/>,
/// read in the <see cref="CwDecoder.CharacterSettled"/> handler, which is the
/// moment of emission; nothing here changes how either is computed. Beside it,
/// unit 448's acquiring (before the tracker's first keyed verdict) is printed as a
/// fact, so unit 449's thirteen can be placed.</para>
/// <para>**THE SAME HARNESS AND THE SAME ARITHMETIC AS
/// <see cref="TheRequirementsAreMeasuredTests"/>**: the decoder driven hop by hop,
/// flushed, the baseline's scored stretches, <see cref="CwMetrics"/>. **EVERY REAL
/// KEY IS INFERRED** (V-13); **EVERY SYNTHETIC KEY IS EXACT** and never sole
/// evidence (12.5).</para>
/// </remarks>
public sealed class WhatTheSureLettersWerePrintedUnderFact
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fact.</summary>
    /// <param name="output">Where the trace is printed.</param>
    public WhatTheSureLettersWerePrintedUnderFact(ITestOutputHelper output)
        => _output = output;

    /// <summary>A decode with both proof states at each settled character's emission.</summary>
    /// <param name="Settled">What settled.</param>
    /// <param name="Pitch">The pitch's proof state as each settled.</param>
    /// <param name="Speed">The speed's proof state as each settled.</param>
    /// <param name="FirstKeyedSeconds">The tracker's first keyed verdict (unit 448's acquiring), or NaN.</param>
    internal sealed record Run(
        IReadOnlyList<CwCharacter> Settled, IReadOnlyList<CwPitchProof> Pitch, IReadOnlyList<CwSpeedProof> Speed,
        double FirstKeyedSeconds)
    {
        /// <summary>Whether the character at an index was emitted while acquiring.</summary>
        public bool Acquiring(int i) => Pitch[i] != CwPitchProof.Proved || Speed[i] != CwSpeedProof.Proved;
    }

    /// <summary>One named letter over a scored stretch.</summary>
    internal sealed record Letter(
        string Name, string Set, string Condition, CwKeyKind Kind, CwCharacter Character, string Sent, string Class,
        bool Sure, CwPitchProof Pitch, CwSpeedProof Speed, bool BeforeVerdict)
    {
        /// <summary>Either state not proved at emission.</summary>
        public bool Acquiring => Pitch != CwPitchProof.Proved || Speed != CwSpeedProof.Proved;

        /// <summary>Right against the key.</summary>
        public bool IsRight => Class == "right";
    }

    /// <summary>Drives the decoder over audio as the harness does, reading both proof states at each emission.</summary>
    internal static Run Drive(float[] samples, int sampleRate, double pitchHz)
    {
        var decoder = new CwDecoder(sampleRate, pitchHz);
        var settled = new List<CwCharacter>();
        var pitch = new List<CwPitchProof>();
        var speed = new List<CwSpeedProof>();
        var firstKeyed = double.NaN;

        decoder.CharacterSettled += c =>
        {
            settled.Add(c);
            pitch.Add(decoder.Tracker.PitchProof);
            speed.Add(decoder.SpeedProof);
        };

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, sampleRate, samples.AsSpan((int)at, hop)));

            if (double.IsNaN(firstKeyed) && !double.IsNaN(decoder.Tracker.LastKeyedHz))
            {
                firstKeyed = (at + hop) / (double)sampleRate;
            }
        }

        decoder.Flush();

        return new Run(settled, pitch, speed, firstKeyed);
    }

    private static Run DriveFile(string path, double pitchHz)
    {
        var audio = WavAudio.Read(path);

        return Drive(audio.Samples, audio.SampleRate, pitchHz);
    }

    /// <summary>Every named letter over the scored stretches, sure and dim, with its states.</summary>
    internal static IEnumerable<Letter> Letters(
        string name, string set, string condition, CwKeyKind kind, Run run, IEnumerable<CwScore> scores)
    {
        var index = new Dictionary<CwCharacter, int>(ReferenceEqualityComparer.Instance);

        for (var i = 0; i < run.Settled.Count; i++)
        {
            index[run.Settled[i]] = i;
        }

        foreach (var score in scores)
        {
            var covered = TheRequirementsAreMeasuredTests.Covered(run.Settled, score);
            var characters = covered.Where(c => !c.IsWordGap).ToList();
            var alignment = CwMetrics.Align(CwMetrics.Symbols(covered), score.Key, kind);
            var d = 0;

            foreach (var step in alignment.Steps)
            {
                if (step.Decoded is null)
                {
                    continue;
                }

                var c = characters[d++];

                if (step.Decoded.Value.Class is not (CwSymbolClass.Sure or CwSymbolClass.NotSure))
                {
                    continue;
                }

                var at = index[c];
                var cls = step.Key is null ? "added"
                    : string.Equals(step.Key, step.Decoded.Value.Text, StringComparison.Ordinal) ? "right" : "wrong";

                yield return new Letter(
                    name, set, condition, kind, c, step.Key ?? "-", cls, step.Decoded.Value.Class == CwSymbolClass.Sure,
                    run.Pitch[at], run.Speed[at], double.IsNaN(run.FirstKeyedSeconds) || c.At.TotalSeconds <= run.FirstKeyedSeconds);
            }
        }
    }

    /// <summary>
    /// Unit 449's thirteen sure-wrong letters before the tracker's first keyed verdict, from
    /// `.run-unit/unit449-trace449-change.txt`: recording, emission time, sent, emitted.
    /// </summary>
    internal static readonly (string Name, double At, string Sent, string Emitted)[] FourFortyNinesThirteen =
    {
        ("unadjudicated/cw-2026-08-22-032012", 1.765, "O", "T"),
        ("unadjudicated/cw-2026-08-22-032012", 7.425, "-", "."),
        ("unadjudicated/cw-2026-08-22-032012", 8.640, "-", "1"),
        ("unadjudicated/cw-2026-08-22-032050", 11.195, "-", "T"),
        ("unadjudicated/cw-2026-08-22-032050", 11.545, "U", "A"),
        ("unadjudicated/cw-2026-08-22-032050", 16.375, "P", "W"),
        ("unadjudicated/cw-2026-08-22-032050", 17.745, "N", "T"),
        ("unadjudicated/cw-2026-08-22-032113", 6.135, "D", "O"),
        ("unadjudicated/cw-2026-08-22-032129", 4.175, "2", "T"),
        ("unadjudicated/cw-2026-08-22-032129", 4.990, "0", "J"),
        ("unadjudicated/cw-2026-08-22-032129", 7.810, "R", "G"),
        ("unadjudicated/cw-2026-08-22-032129", 20.250, "-", "A"),
        ("unadjudicated/cw-2026-08-22-032129", 20.460, "L", "E"),
    };

    private static string State(CwPitchProof p) => p.ToString().ToLowerInvariant();

    private static string State(CwSpeedProof s) => s.ToString().ToLowerInvariant();

    private void Table(string set, IReadOnlyList<Letter> letters)
    {
        var sure = letters.Where(l => l.Sure).ToList();

        _output.WriteLine("acquiring | set | condition | key | inside: right | inside: wrong or added | outside: right | outside: wrong or added | sure emitted | MET-CER-SURE");

        foreach (var g in sure.GroupBy(l => l.Condition).OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            Row(set, g.Key, string.Join(" and ", g.Select(l => CwMetrics.KindWord(l.Kind)).Distinct()), g.ToList());
        }

        Row(set, "all", string.Join(" and ", sure.Select(l => CwMetrics.KindWord(l.Kind)).Distinct()), sure);

        // Which state was not proved, inside acquiring.
        foreach (var (label, test) in new (string, Func<Letter, bool>)[]
        {
            ("pitch not proved, speed proved", l => l.Pitch != CwPitchProof.Proved && l.Speed == CwSpeedProof.Proved),
            ("speed not proved, pitch proved", l => l.Speed != CwSpeedProof.Proved && l.Pitch == CwPitchProof.Proved),
            ("neither proved", l => l.Pitch != CwPitchProof.Proved && l.Speed != CwSpeedProof.Proved),
        })
        {
            var inside = sure.Where(test).ToList();

            _output.WriteLine($"which | {set} | {label} | right {inside.Count(l => l.IsRight)} | wrong or added {inside.Count(l => !l.IsRight)}");
        }

        foreach (var g in sure.Where(l => l.Acquiring).GroupBy(l => (Pitch: State(l.Pitch), Speed: State(l.Speed))).OrderBy(g => g.Key.Pitch).ThenBy(g => g.Key.Speed))
        {
            _output.WriteLine($"states | {set} | pitch {g.Key.Pitch}, speed {g.Key.Speed} | right {g.Count(l => l.IsRight)} | wrong or added {g.Count(l => !l.IsRight)}");
        }

        // Unit 448's acquiring beside the resolution's.
        var before = sure.Where(l => l.BeforeVerdict).ToList();

        _output.WriteLine(
            $"verdict | {set} | before the tracker's first keyed verdict (448) | right {before.Count(l => l.IsRight)} | wrong or added {before.Count(l => !l.IsRight)} | "
            + $"of them inside acquiring (resolution) {before.Count(l => l.Acquiring)}");

        var dim = letters.Where(l => !l.Sure).ToList();

        _output.WriteLine(string.Create(Invariant,
            $"dim | {set} | dim precision (HM-REQ-014): {dim.Count(l => l.IsRight)} right of {dim.Count} dim, {(dim.Count == 0 ? double.NaN : dim.Count(l => l.IsRight) / (double)dim.Count):0.0000} | "
            + $"dim rate: {dim.Count} dim of {letters.Count} named letters in the scored stretches"));
    }

    private void Row(string set, string condition, string kind, IReadOnlyList<Letter> sure)
    {
        var inside = sure.Where(l => l.Acquiring).ToList();
        var outside = sure.Where(l => !l.Acquiring).ToList();
        var wrong = sure.Count(l => !l.IsRight);

        _output.WriteLine(string.Create(Invariant,
            $"acquiring | {set} | {condition} | {kind} | {inside.Count(l => l.IsRight)} | {inside.Count(l => !l.IsRight)} | {outside.Count(l => l.IsRight)} | {outside.Count(l => !l.IsRight)} | "
            + $"{sure.Count} | {wrong} of {sure.Count}{(sure.Count == 0 ? string.Empty : string.Create(Invariant, $", {wrong / (double)sure.Count:0.0000}"))}"));
    }

    private void Print(string set, IReadOnlyList<Letter> letters)
    {
        var hop = CwProbabilisticDecoder.HopMilliseconds / 1000.0;

        _output.WriteLine("sure | set | recording | at s | span s | sent | emitted | class | pitch proof | speed proof | inside acquiring | before first keyed verdict (448)");

        foreach (var l in letters.Where(l => l.Sure))
        {
            var c = l.Character;

            _output.WriteLine(string.Create(Invariant,
                $"sure | {set} | {l.Name} | {c.At.TotalSeconds:0.000} | {c.At.TotalSeconds - Math.Max(1, c.SpanHops) * hop:0.00} to {c.At.TotalSeconds:0.00} | `{l.Sent}` | `{c.Text}` | {l.Class} | "
                + $"{State(l.Pitch)} | {State(l.Speed)} | {(l.Acquiring ? "yes" : "no")} | {(l.BeforeVerdict ? "yes" : "no")}"));
        }

        Table(set, letters);
    }

    private void FirstSure(string set, string name, Run run)
    {
        var first = run.Settled
            .Select((c, i) => (c, i))
            .FirstOrDefault(p => !p.c.IsWordGap && CwSymbol.Of(p.c).Class == CwSymbolClass.Sure);

        var verdict = double.IsNaN(run.FirstKeyedSeconds) ? "none" : run.FirstKeyedSeconds.ToString("0.00", Invariant);
        var what = first.c is null
            ? "no sure character"
            : string.Create(Invariant, $"`{first.c.Text}` at {first.c.At.TotalSeconds:0.000} s, pitch {State(run.Pitch[first.i])}, speed {State(run.Speed[first.i])}");
        var proved = run.Settled.Select((c, i) => (c, i))
            .Where(p => !p.c.IsWordGap && !run.Acquiring(p.i))
            .Select(p => p.c.At.TotalSeconds.ToString("0.000", Invariant))
            .FirstOrDefault() ?? "never";

        _output.WriteLine($"first sure | {set} | {name} | first keyed verdict {verdict} s | {what} | first character both proved at {proved} s");
    }

    /// <remarks>
    /// Proves nothing about the decoder; prints task 1 of work instruction 470: every
    /// sure letter over the scored stretches of the 23 real keyed recordings and the 12
    /// synthetic cases with both proof states at its emission, the inside-and-outside
    /// acquiring table per condition with the key's kind, where unit 449's thirteen fall,
    /// and each recording's first sure character.
    /// </remarks>
    [Fact]
    public void EverySureLetterWithTheProofStatesAtItsEmission()
    {
        var real = new List<Letter>();
        var runs = new Dictionary<string, Run>(StringComparer.Ordinal);

        foreach (var k in WhatTheStrayLettersRestOnTests.KeyedRecordings)
        {
            var run = DriveFile(Path.Combine(CapturedSignalTests.Folder, k.Name + ".wav"), 600);

            runs[k.Name] = run;
            real.AddRange(Letters(k.Name, "real", TheRequirementsAreMeasuredTests.RealCondition(k.Name), CwKeyKind.Inferred,
                run, k.Score(CwReading.Of(run.Settled))));
            FirstSure("real", k.Name, run);
        }

        var synthetic = new List<Letter>();

        foreach (var recipe in SyntheticCq.All)
        {
            var run = DriveFile(Path.Combine(SyntheticCq.Folder, recipe.Name + ".wav"), SyntheticCq.StartingPitchHz);
            var reading = CwReading.Of(run.Settled);
            var from = reading.Text.TakeWhile(char.IsWhiteSpace).Count();

            synthetic.AddRange(Letters(recipe.Name, "synthetic", TheRequirementsAreMeasuredTests.SyntheticCondition(recipe), CwKeyKind.Exact,
                run, new[] { SyntheticCq.Whole(reading) with { Start = from } }));
            FirstSure("synthetic", recipe.Name, run);
        }

        Print("real", real);
        Print("synthetic", synthetic);

        _output.WriteLine("449 | recording | at s | sent | emitted | found at HEAD | sure | inside acquiring | pitch proof | speed proof | before first keyed verdict (448)");

        var inside = 0;

        foreach (var t in FourFortyNinesThirteen)
        {
            var l = real.FirstOrDefault(l => l.Name == t.Name && Math.Abs(l.Character.At.TotalSeconds - t.At) < 0.0005 && l.Character.Text == t.Emitted);

            if (l is { Sure: true, Acquiring: true })
            {
                inside++;
            }

            var found = l is null ? "no | - | - | - | - | -"
                : $"yes | {(l.Sure ? "yes" : "no")} | {(l.Acquiring ? "yes" : "no")} | {State(l.Pitch)} | {State(l.Speed)} | {(l.BeforeVerdict ? "yes" : "no")}";

            _output.WriteLine(string.Create(Invariant, $"449 | {t.Name} | {t.At:0.000} | `{t.Sent}` | `{t.Emitted}` | {found}"));
        }

        _output.WriteLine($"449 | total | {inside} of unit 449's {FourFortyNinesThirteen.Length} are sure and inside acquiring");

        foreach (var (set, letters) in new[] { ("real", real), ("synthetic", synthetic) })
        {
            var sure = letters.Where(l => l.Sure).ToList();

            _output.WriteLine(
                $"total | {set} | {sure.Count} sure: {sure.Count(l => l.IsRight)} right, {sure.Count(l => l.Class == "wrong")} wrong, {sure.Count(l => l.Class == "added")} added | "
                + $"inside acquiring {sure.Count(l => l.Acquiring && l.IsRight)} right (the coverage the gate costs), {sure.Count(l => l.Acquiring && !l.IsRight)} wrong or added (the MET-CER-SURE it saves) | "
                + $"outside {sure.Count(l => !l.Acquiring && l.IsRight)} right, {sure.Count(l => !l.Acquiring && !l.IsRight)} wrong or added");
        }
    }

    /// <remarks>
    /// Proves nothing about the decoder; prints HM-REQ-102 on the 7.052 opening, `003901`
    /// to `004234` spliced as <see cref="WhatTheOpeningHeardTests"/> splices them and
    /// driven cold from 600 Hz as unit 448 drove it: every character with its class and
    /// both proof states, the text marked, and the sure letters inside acquiring by the
    /// resolution and before the first keyed verdict (unit 448's 0 to 26.04 s), each
    /// labelled against unit 448's reference, each file read alone.
    /// </remarks>
    [Fact]
    public void TheOpeningOfSevenOhFiveTwo()
    {
        var (samples, rate, pieces) = WhatTheOpeningHeardTests.Splice(WhatTheOpeningHeardTests.Session.Take(7).ToList());
        var run = Drive(samples, rate, 600);
        var end = WhatTheDecoderDoesWhileAcquiringTests.OpeningEndSeconds;

        // Unit 448's reference: 003901 and then 003919, each read alone, over what each contributes.
        var reference = new List<(double Seconds, CwCharacter Character)>();

        foreach (var piece in pieces.Take(2))
        {
            var alone = WavAudio.Read(Path.Combine(CapturedSignalTests.Folder, "unadjudicated", piece.Name + ".wav"));
            var aloneRun = WhatTheDecoderDoesWhileAcquiringTests.Drive(alone.Samples, alone.SampleRate);
            var offset = (piece.StreamStart - piece.Skip) / (double)rate;
            var from = piece.StreamStart / (double)rate;
            var to = (piece.StreamStart + piece.Kept) / (double)rate;

            reference.AddRange(aloneRun.Settled
                .Select(c => (c.At.TotalSeconds + offset, c))
                .Where(p => p.Item1 >= from && p.Item1 < Math.Min(to, end)));
        }

        var sent = System.Text.RegularExpressions.Regex.Replace(
            string.Concat(reference.Where(p => !p.Character.IsUnreadable).Select(p => p.Character.Text)).Trim(), " +", " ");
        var heard = run.Settled.Take(run.Settled.Count(c => c.At.TotalSeconds < end)).ToList();
        var alignment = CwMetrics.Align(CwMetrics.Symbols(heard), sent, CwKeyKind.Inferred);
        var labels = new string[heard.Count];
        var named = heard.Select((c, i) => (c, i)).Where(p => !p.c.IsWordGap).Select(p => p.i).ToList();
        var d = 0;

        foreach (var step in alignment.Steps)
        {
            if (step.Decoded is { } decoded)
            {
                labels[named[d++]] = step.Key is null ? "added"
                    : string.Equals(step.Key, decoded.Text, StringComparison.Ordinal) ? "right" : "wrong";
            }
        }

        _output.WriteLine(string.Create(Invariant,
            $"opening | first keyed verdict (448's acquiring end) {run.FirstKeyedSeconds:0.00} s | sent, inferred, each file read alone, 0 to {end} s | {sent}"));
        _output.WriteLine("char | at s | text | class | pitch proof | speed proof | inside acquiring | before first keyed verdict | against 448's reference");

        for (var i = 0; i < run.Settled.Count; i++)
        {
            var c = run.Settled[i];

            if (c.IsWordGap)
            {
                continue;
            }

            _output.WriteLine(string.Create(Invariant,
                $"char | {c.At.TotalSeconds:0.000} | `{c.Text}` | {CwSymbol.Of(c).Class.ToString().ToLowerInvariant()} | {State(run.Pitch[i])} | {State(run.Speed[i])} | "
                + $"{(run.Acquiring(i) ? "yes" : "no")} | {(c.At.TotalSeconds <= run.FirstKeyedSeconds ? "yes" : "no")} | {(i < labels.Length ? labels[i] ?? "gap" : "past 46.2 s")}"));
        }

        // The text: sure as itself, dim in parentheses, braces round what was emitted while acquiring.
        var text = new System.Text.StringBuilder();
        var open = false;

        for (var i = 0; i < heard.Count; i++)
        {
            var c = heard[i];
            var acquiring = !c.IsWordGap && run.Acquiring(i);

            if (!c.IsWordGap && acquiring != open)
            {
                text.Append(acquiring ? '{' : '}');
                open = acquiring;
            }

            text.Append(c.IsWordGap || CwSymbol.Of(c).Class == CwSymbolClass.Sure ? c.Text : $"({c.Text})");
        }

        if (open)
        {
            text.Append('}');
        }

        _output.WriteLine($"opening | text 0 to {end} s, sure as itself, (dim), {{emitted while acquiring}} | {text.ToString().Trim()}");

        foreach (var (label, test) in new (string, Func<int, bool>)[]
        {
            ("inside acquiring (resolution), 0 to 46.2 s", i => run.Acquiring(i)),
            (string.Create(Invariant, $"before the first keyed verdict (448), 0 to {run.FirstKeyedSeconds:0.00} s"), i => heard[i].At.TotalSeconds <= run.FirstKeyedSeconds),
        })
        {
            var sure = Enumerable.Range(0, heard.Count)
                .Where(i => !heard[i].IsWordGap && CwSymbol.Of(heard[i]).Class == CwSymbolClass.Sure && test(i))
                .ToList();

            var which = string.Join(" ", sure.Select(i => string.Create(Invariant, $"{heard[i].Text}@{heard[i].At.TotalSeconds:0.00}/{labels[i]}")));

            _output.WriteLine(
                $"hm-req-102 | opening | sure letters {label}: {sure.Count} ({sure.Count(i => labels[i] == "right")} right, {sure.Count(i => labels[i] != "right")} wrong or added) | {which}");
        }

        var coverage = CwMetrics.Coverage(alignment);

        _output.WriteLine(string.Create(Invariant,
            $"opening | HM-REQ-103 | {coverage.SureRight} of {coverage.Sent} opening characters read sure and right ({coverage.SureEmitted} sure emitted), inferred"));
        FirstSure("opening", "7.052 opening", run);
    }
}
