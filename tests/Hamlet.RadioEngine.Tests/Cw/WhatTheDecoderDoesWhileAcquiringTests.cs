using System.Globalization;
using System.Reflection;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Instruments;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// What the decoder does while it is acquiring: where the tracker points the
/// filter before anything is confirmed, what that choice rested on, and what the
/// decoder printed sure meanwhile (work instruction 448, task 1; PHASE_PLAN.md 4.5;
/// HM-REQ-091, 102, 103).
/// </summary>
/// <remarks>
/// <para>**A PRINTER. IT ASSERTS NOTHING.**</para>
/// <para>**ACQUIRING, AS THIS PRINTER USES IT.** Neither `CW_SPEC.md` nor
/// `CW_REQUIREMENTS.md` defines it (MET-TACQ ends at the first sure character, which
/// would make HM-REQ-102 true by definition). So, per the work instruction: from the
/// start of the recording to the tracker's first keyed verdict, HM-DEC-095's
/// twice-confirmation, which is the first hop on which the tracker's
/// `_lastKeyedHz` is set. It is read by reflection and written nowhere. The tracker
/// has no state for keying resumed after silence - once set, `_lastKeyedHz` is never
/// cleared - so a recording has one acquiring span, from its start.</para>
/// <para>**A FROM-COLD MOVE** is a hop on which the tracker retuned
/// (<see cref="CwToneTracker.Retunes"/> rose) while nothing had been confirmed, the
/// move at `CwToneTracker.cs`'s "FROM COLD" comment. Beside it: the survey's
/// <see cref="ToneVerdict.Strongest"/> after the hop, which is what the move went
/// to at HEAD, ranked by lift over the band alone; every bin the survey admitted as
/// keying on the same history (<see cref="CwToneTracker.CoarseCandidates"/>); and
/// what every bin showed of keying whether admitted or not
/// (<see cref="CwToneTracker.CoarseStructures"/>). At HEAD no bin is ever admitted
/// at a from-cold move - the move runs only when the verdict holds no keying - so
/// the keying score is <see cref="BestKeying"/>: of the bins 10 dB over the band
/// with eight clean marks, the one keyed deepest. A move is by **level alone** when
/// it went to a bin that is not that one, or when no bin shows keying.</para>
/// <para>**THE INSTRUMENT IS NEVER HANDED TO THE DECODER**; its windows are printed
/// beside the span.</para>
/// </remarks>
public sealed class WhatTheDecoderDoesWhileAcquiringTests
{
    /// <summary>Where the opening's stretch ends, in seconds of the spliced stream.</summary>
    internal const double OpeningEndSeconds = 46.2;

    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private static readonly FieldInfo LastKeyedField =
        typeof(CwToneTracker).GetField("_lastKeyedHz", BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new InvalidOperationException("CwToneTracker has no field _lastKeyedHz");

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the trace is printed.</param>
    public WhatTheDecoderDoesWhileAcquiringTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>One from-cold move.</summary>
    /// <param name="Seconds">When, on the audio clock.</param>
    /// <param name="FromHz">Where the filter pointed before.</param>
    /// <param name="ToHz">Where it pointed after.</param>
    /// <param name="Strongest">The survey's loudest bin after the hop, or null.</param>
    /// <param name="Keyed">The survey's keying verdict after the hop, or null.</param>
    /// <param name="Admitted">Every bin the survey admitted as keying on that history.</param>
    /// <param name="Structures">What every bin showed of keying on that history.</param>
    internal sealed record ColdMove(
        double Seconds, double FromHz, double ToHz, ToneInterference? Strongest,
        KeyingCandidate? Keyed, IReadOnlyList<KeyingCandidate> Admitted, IReadOnlyList<KeyedStructure> Structures)
    {
        /// <summary>The admitted bin with the best separation, or null when none was admitted.</summary>
        public KeyingCandidate? BestAdmitted => Admitted.Count == 0
            ? null
            : Admitted.OrderByDescending(c => c.Separation).First();

        /// <summary>The bin showing the deepest keying, or null when none shows any.</summary>
        public KeyedStructure? BestKeyed => BestKeying(Structures);

        /// <summary>What the bin moved to showed.</summary>
        public KeyedStructure? MovedTo => Structures
            .Where(s => Math.Abs(s.ToneHz - ToHz) <= 12.5)
            .Cast<KeyedStructure?>()
            .FirstOrDefault();

        /// <summary>True when the move went to a bin that is not the best-keyed one, or no bin showed keying.</summary>
        public bool LevelAlone => BestKeyed is not { } best || Math.Abs(best.ToneHz - ToHz) > 12.5;
    }

    /// <summary>
    /// This printer's keying score: among the bins standing at least
    /// <see cref="CwToneSurvey.InterferenceLiftDb"/> over the band with at least
    /// <see cref="CwToneSurvey.MinimumMarks"/> clean marks, the one keyed deepest,
    /// key-down over its own key-up.
    /// </summary>
    internal static KeyedStructure? BestKeying(IEnumerable<KeyedStructure> structures)
        => structures
            .Where(s => s.LiftDb >= CwToneSurvey.InterferenceLiftDb && s.Marks >= CwToneSurvey.MinimumMarks)
            .OrderByDescending(s => s.ContrastDb)
            .Cast<KeyedStructure?>()
            .FirstOrDefault();

    /// <summary>A recording driven cold: its settled characters, the tracker per hop and every cold move.</summary>
    /// <param name="Settled">What settled.</param>
    /// <param name="TrackerHz">The tracker's pitch after each hop.</param>
    /// <param name="HopSeconds">One hop, in seconds.</param>
    /// <param name="ConfirmedSeconds">When the first keyed verdict came, or NaN when none came.</param>
    /// <param name="ConfirmedHz">Where it was.</param>
    /// <param name="FirstAdmitted">The first keying candidate the survey admitted, with its time, or null.</param>
    /// <param name="Moves">Every from-cold move.</param>
    internal sealed record Acquired(
        IReadOnlyList<CwCharacter> Settled, double[] TrackerHz, double HopSeconds,
        double ConfirmedSeconds, double ConfirmedHz,
        (double Seconds, KeyingCandidate Candidate)? FirstAdmitted, IReadOnlyList<ColdMove> Moves)
    {
        /// <summary>The end of the acquiring span: the first keyed verdict, or the end of the audio.</summary>
        public double SpanEnd => double.IsNaN(ConfirmedSeconds) ? TrackerHz.Length * HopSeconds : ConfirmedSeconds;

        /// <summary>The tracker's pitch at a time.</summary>
        public double TrackerAt(double seconds)
            => TrackerHz[Math.Clamp((int)(seconds / HopSeconds) - 1, 0, TrackerHz.Length - 1)];
    }

    /// <summary>Drive the decoder over audio, cold from 600 Hz, as the captures type does.</summary>
    /// <param name="samples">The audio.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <returns>The run.</returns>
    internal static Acquired Drive(float[] samples, int sampleRate)
    {
        var decoder = new CwDecoder(sampleRate, 600);
        var tracker = decoder.Tracker;
        var hop = tracker.HopSamples;
        var hopSeconds = hop / (double)sampleRate;
        var settled = new List<CwCharacter>();
        var pitch = new List<double>();
        var moves = new List<ColdMove>();
        var confirmed = double.NaN;
        var confirmedHz = double.NaN;
        (double, KeyingCandidate)? admitted = null;

        decoder.CharacterSettled += settled.Add;

        for (var at = 0L; at + hop <= samples.Length; at += hop)
        {
            var before = tracker.ToneHz;
            var retunes = tracker.Retunes;
            var cold = double.IsNaN((double)LastKeyedField.GetValue(tracker)!);

            decoder.Process(new AudioChunk(at, sampleRate, samples.AsSpan((int)at, hop)));

            var seconds = (at + hop) / (double)sampleRate;
            var lastKeyed = (double)LastKeyedField.GetValue(tracker)!;

            pitch.Add(tracker.ToneHz);

            // An admitted candidate that is not yet confirmed never reaches the
            // verdict, so the survey's own list is asked, every twentieth hop.
            if (cold && admitted is null && pitch.Count % 20 == 0
                && tracker.CoarseCandidates() is { Count: > 0 } found)
            {
                admitted = (seconds, found.OrderByDescending(c => c.Separation).First());
            }

            if (cold && double.IsNaN(lastKeyed) && tracker.Retunes > retunes)
            {
                moves.Add(new ColdMove(
                    seconds, before, tracker.ToneHz, tracker.Verdict.Strongest, tracker.Verdict.Keyed,
                    tracker.CoarseCandidates().ToList(), tracker.CoarseStructures().ToList()));
            }

            if (cold && !double.IsNaN(lastKeyed))
            {
                confirmed = seconds;
                confirmedHz = lastKeyed;
            }
        }

        decoder.Flush();

        return new Acquired(settled, pitch.ToArray(), hopSeconds, confirmed, confirmedHz, admitted, moves);
    }

    /// <summary>
    /// The 7.052 opening, `003901` to `004234` spliced as
    /// <see cref="WhatTheOpeningHeardTests"/> splices them: the acquiring span, every
    /// cold move in it, the instrument beside it, every sure character inside it, and
    /// the opening's text from 0 to 46.2 s against the same stretch as the decoder
    /// reads each file alone (HM-REQ-102, 103).
    /// </summary>
    [Fact]
    public void OnTheOpening()
    {
        var (samples, rate, pieces) = WhatTheOpeningHeardTests.Splice(WhatTheOpeningHeardTests.Session.Take(7).ToList());
        var run = Drive(samples, rate);
        var windows = CwPitchInstrument.Measure(samples, rate);

        foreach (var piece in pieces)
        {
            _output.WriteLine(string.Create(Invariant,
                $"join | {piece.Name} | stream {piece.StreamStart / (double)rate:0.00} to {(piece.StreamStart + piece.Kept) / (double)rate:0.00} s | file starts at stream {(piece.StreamStart - piece.Skip) / (double)rate:0.00} s"));
        }

        PrintSpan("opening", run, windows);

        // The stretch as each file reads it alone: 003901 for what it contributes,
        // then 003919 for what it contributes after it (from stream 30.00 s).
        var reference = new List<(double Seconds, CwCharacter Character)>();

        foreach (var piece in pieces.Take(2))
        {
            var alone = WavAudio.Read(Path.Combine(CapturedSignalTests.Folder, "unadjudicated", piece.Name + ".wav"));
            var aloneRun = Drive(alone.Samples, alone.SampleRate);
            var offset = (piece.StreamStart - piece.Skip) / (double)rate;
            var from = piece.StreamStart / (double)rate;
            var to = (piece.StreamStart + piece.Kept) / (double)rate;

            _output.WriteLine(string.Create(Invariant,
                $"alone | {piece.Name} | acquiring 0.00 to {aloneRun.SpanEnd:0.00} s of the file (stream {offset:0.00} to {offset + aloneRun.SpanEnd:0.00}) | confirmed at {aloneRun.ConfirmedHz:0.0} Hz | text {Text(aloneRun.Settled)}"));

            reference.AddRange(aloneRun.Settled
                .Select(c => (c.At.TotalSeconds + offset, c))
                .Where(p => p.Item1 >= from && p.Item1 < Math.Min(to, OpeningEndSeconds)));
        }

        var sent = string.Concat(reference
            .Where(p => !p.Character.IsUnreadable)
            .Select(p => p.Character.Text)).Trim();
        var sentText = System.Text.RegularExpressions.Regex.Replace(sent, " +", " ");

        _output.WriteLine($"opening | sent, inferred: each file read alone, 0 to {OpeningEndSeconds} s, placeholders dropped | {sentText}");

        var heard = run.Settled.Where(c => c.At.TotalSeconds < OpeningEndSeconds).ToList();
        var alignment = CwMetrics.Align(CwMetrics.Symbols(heard), sentText, CwKeyKind.Inferred);
        var labels = LabelsFrom(heard, alignment);
        var coverage = CwMetrics.Coverage(alignment);

        PrintSure("opening", run, heard, labels, "against each file read alone");

        // A second reference for what 003901 and 003919 share: 003919's own
        // decoding of the same audio, from where it starts in the stream.
        var second = pieces[1];
        var secondStart = (second.StreamStart - second.Skip) / (double)rate;
        var secondAudio = WavAudio.Read(Path.Combine(CapturedSignalTests.Folder, "unadjudicated", second.Name + ".wav"));
        var secondRun = Drive(secondAudio.Samples, secondAudio.SampleRate);
        var shared = heard.Where(c => c.At.TotalSeconds >= secondStart && c.At.TotalSeconds < second.StreamStart / (double)rate).ToList();
        var theirs = string.Concat(secondRun.Settled
            .Where(c => !c.IsUnreadable && c.At.TotalSeconds + secondStart < second.StreamStart / (double)rate)
            .Select(c => c.Text)).Trim();
        var sharedLabels = LabelsFrom(shared, CwMetrics.Align(CwMetrics.Symbols(shared), System.Text.RegularExpressions.Regex.Replace(theirs, " +", " "), CwKeyKind.Inferred));
        var sharedSure = shared.Select((c, i) => (c, i))
            .Where(p => p.c.At.TotalSeconds < run.SpanEnd && CwSymbol.Of(p.c).Class == CwSymbolClass.Sure)
            .ToList();

        _output.WriteLine(string.Create(Invariant,
            $"opening | {second.Name} alone over stream {secondStart:0.00} to {second.StreamStart / (double)rate:0.00} s | {theirs}"));
        _output.WriteLine(string.Create(Invariant,
            $"hm-req-102 | opening against {second.Name}'s own decoding | sure characters while acquiring in stream {secondStart:0.00} to {second.StreamStart / (double)rate:0.00} s: {sharedSure.Count} | {string.Join(" ", sharedSure.Select(p => $"{p.c.Text}@{p.c.At.TotalSeconds:0.00}/{sharedLabels[p.i]}"))}"));
        _output.WriteLine($"opening | text 0 to {OpeningEndSeconds} s, marked | {Marked(heard, labels, run.SpanEnd)}");
        _output.WriteLine(string.Create(Invariant,
            $"opening | HM-REQ-103 | {coverage.SureRight} of {coverage.Sent} opening characters read sure and right ({coverage.SureEmitted} sure emitted), inferred"));
        _output.WriteLine(string.Create(Invariant,
            $"opening | HM-REQ-103, after the acquiring span | {labels.Where((l, i) => l == "right" && heard[i].At.TotalSeconds >= run.SpanEnd).Count()} sure and right after {run.SpanEnd:0.00} s"));
        _output.WriteLine(string.Create(Invariant,
            $"total | opening | cold moves {run.Moves.Count} | by level alone {run.Moves.Count(m => m.LevelAlone)}"));
    }

    /// <summary>
    /// The eleven files 447 found more than 25 Hz off: the acquiring span, every cold
    /// move and whether it went by level alone, and the sure characters inside the
    /// span, labelled against the key where the file has one.
    /// </summary>
    [Fact]
    public void OnTheElevenFilesOff()
    {
        var moves = 0;
        var alone = 0;

        foreach (var name in WhatPitchTheDecoderIsOnTests.OffAtEntry)
        {
            var (m, a) = OneFile(name);
            moves += m;
            alone += a;
        }

        _output.WriteLine(string.Create(Invariant,
            $"total | the eleven | cold moves {moves} | by level alone {alone}"));
    }

    /// <summary>
    /// The keyed 23: the same, with the sure characters inside the span labelled
    /// against the inferred key and the first scored stretch's sure-and-right count.
    /// </summary>
    [Fact]
    public void OnTheKeyedRecordings()
    {
        var moves = 0;
        var alone = 0;
        var sureInside = 0;

        foreach (var keyed in WhatTheStrayLettersRestOnTests.KeyedRecordings)
        {
            var (m, a, s) = OneFile(keyed.Name, keyed);
            moves += m;
            alone += a;
            sureInside += s;
        }

        _output.WriteLine(string.Create(Invariant,
            $"total | keyed {WhatTheStrayLettersRestOnTests.KeyedRecordings.Count} | cold moves {moves} | by level alone {alone} | sure characters while acquiring {sureInside}"));
    }

    private (int Moves, int Alone) OneFile(string name)
    {
        var keyed = WhatTheStrayLettersRestOnTests.KeyedRecordings.SingleOrDefault(k => k.Name == name);
        var (m, a, _) = OneFile(name, keyed);

        return (m, a);
    }

    private (int Moves, int Alone, int SureInside) OneFile(string name, WhatTheStrayLettersRestOnTests.Keyed? keyed)
    {
        var audio = WavAudio.Read(Path.Combine(CapturedSignalTests.Folder, name + ".wav"));
        var run = Drive(audio.Samples, audio.SampleRate);
        var windows = CwPitchInstrument.Measure(audio.Samples, audio.SampleRate);

        PrintSpan(name, run, windows);

        string[] labels;
        var how = "no key";

        if (keyed is not null)
        {
            var scores = keyed.Score(CwReading.Of(run.Settled));
            labels = WhatTheStrayLettersRestOnTests.Labels(run.Settled, scores)
                .Select(l => l.ToString().ToLowerInvariant())
                .ToArray();
            how = "against the inferred key";

            if (scores.Count > 0)
            {
                var first = CwMetrics.Align(
                    CwMetrics.Symbols(TheRequirementsAreMeasuredTests.Covered(run.Settled, scores[0])),
                    scores[0].Key, CwKeyKind.Inferred);
                var coverage = CwMetrics.Coverage(first);

                _output.WriteLine(string.Create(Invariant,
                    $"hm-req-103 | {name} | first scored stretch: {coverage.SureRight} of {coverage.Sent} sent read sure and right ({coverage.SureEmitted} sure emitted), inferred key"));
            }
        }
        else
        {
            labels = Enumerable.Repeat("unscored", run.Settled.Count).ToArray();
        }

        var inside = PrintSure(name, run, run.Settled, labels, how);

        return (run.Moves.Count, run.Moves.Count(m => m.LevelAlone), inside);
    }

    private void PrintSpan(string name, Acquired run, IReadOnlyList<PitchWindow> windows)
    {
        var confirmed = double.IsNaN(run.ConfirmedSeconds)
            ? "never confirmed"
            : string.Create(Invariant, $"first keyed verdict at {run.ConfirmedSeconds:0.00} s, {run.ConfirmedHz:0.0} Hz");
        var admitted = run.FirstAdmitted is { } f
            ? string.Create(Invariant, $"first admitted at {f.Seconds:0.00} s, {f.Candidate.ToneHz:0} Hz, lift {f.Candidate.LiftDb:0.0} dB, separation {f.Candidate.Separation:0.00}")
            : "nothing admitted while acquiring";

        _output.WriteLine(string.Create(Invariant,
            $"span | {name} | acquiring 0.00 to {run.SpanEnd:0.00} s | {confirmed} | {admitted} | cold moves {run.Moves.Count}, by level alone {run.Moves.Count(m => m.LevelAlone)}"));

        foreach (var m in run.Moves)
        {
            var best = m.BestKeyed is { } b
                ? string.Create(Invariant, $"best-keyed {b.ToneHz:0} Hz lift {b.LiftDb:0.0} dB contrast {b.ContrastDb:0.0} dB marks {b.Marks}")
                : "best-keyed none: no bin shows keying";
            var to = m.MovedTo is { } t
                ? string.Create(Invariant, $"moved-to lift {t.LiftDb:0.0} dB contrast {t.ContrastDb:0.0} dB marks {t.Marks} {(t.Admitted ? "admitted" : "not admitted")}")
                : "moved-to no reading";
            var strongest = m.Strongest is { } s
                ? string.Create(Invariant, $"loudest {s.ToneHz:0} Hz lift {s.LiftDb:0.0} dB")
                : "loudest none over 10 dB";
            var survey = m.BestAdmitted is { } a
                ? string.Create(Invariant, $"admitted {m.Admitted.Count}, best {a.ToneHz:0} Hz separation {a.Separation:0.00}")
                : "admitted none";

            _output.WriteLine(string.Create(Invariant,
                $"cold move | {name} | {m.Seconds:0.00} s | {m.FromHz:0.0} -> {m.ToHz:0.0} | {strongest} | {to} | {best} | {survey} | {(m.LevelAlone ? "by level alone" : "to the best-keyed bin")}"));
        }

        foreach (var w in windows.Where(w => w.StartSeconds < run.SpanEnd))
        {
            _output.WriteLine(string.Create(Invariant,
                $"instrument | {name} | {w.StartSeconds:0} to {w.EndSeconds:0} s | {w.Hz:0.0} Hz | tracker there {run.TrackerAt((w.StartSeconds + w.EndSeconds) / 2):0.0} | swing {w.ContrastDb:0.0} dB"));
        }
    }

    private int PrintSure(string name, Acquired run, IReadOnlyList<CwCharacter> settled, IReadOnlyList<string> labels, string how)
    {
        var inside = settled
            .Select((c, i) => (c, i))
            .Where(p => p.c.At.TotalSeconds < run.SpanEnd && CwSymbol.Of(p.c).Class == CwSymbolClass.Sure)
            .ToList();

        _output.WriteLine(string.Create(Invariant,
            $"hm-req-102 | {name} | sure characters while acquiring {inside.Count} | {string.Join(" ", inside.Select(p => $"{p.c.Text}@{p.c.At.TotalSeconds:0.00}/{run.TrackerAt(p.c.At.TotalSeconds):0}/{labels[p.i]}"))} | {how}"));

        return inside.Count;
    }

    /// <summary>Per settled character, right, wrong, added or not scored, from an alignment of those characters.</summary>
    private static string[] LabelsFrom(IReadOnlyList<CwCharacter> settled, CwMetricAlignment alignment)
    {
        var labels = new string[settled.Count];
        var named = settled.Select((c, i) => (c, i)).Where(p => !p.c.IsWordGap).Select(p => p.i).ToList();
        var d = 0;

        foreach (var step in alignment.Steps)
        {
            if (step.Decoded is not { } decoded)
            {
                continue;
            }

            labels[named[d++]] = decoded.Class != CwSymbolClass.Sure ? "not sure"
                : step.Key is null ? "added"
                : string.Equals(step.Key, decoded.Text, StringComparison.Ordinal) ? "right"
                : "wrong";
        }

        for (var i = 0; i < labels.Length; i++)
        {
            labels[i] ??= "gap";
        }

        return labels;
    }

    /// <summary>
    /// The text marked: `{` and `}` around what settled while acquiring, a
    /// character read sure and right as itself, anything else in parentheses.
    /// </summary>
    private static string Marked(IReadOnlyList<CwCharacter> settled, IReadOnlyList<string> labels, double spanEnd)
    {
        var text = new System.Text.StringBuilder();
        var open = false;

        for (var i = 0; i < settled.Count; i++)
        {
            var c = settled[i];
            var acquiring = c.At.TotalSeconds < spanEnd;

            if (acquiring && !open)
            {
                text.Append('{');
                open = true;
            }
            else if (!acquiring && open)
            {
                text.Append('}');
                open = false;
            }

            text.Append(c.IsWordGap || labels[i] == "right" ? c.Text : $"({c.Text})");
        }

        if (open)
        {
            text.Append('}');
        }

        return text.ToString().Trim();
    }

    private static string Text(IEnumerable<CwCharacter> settled) => string.Concat(settled.Select(c => c.Text)).Trim();
}
