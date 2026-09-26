using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Hamlet.RadioEngine.Tests.Cw.Instruments;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Every sure letter the key calls wrong or added at HEAD, with the pitch the
/// instrument hears beside the decoder's, and the signals the decoder itself
/// computes counted over right letters beside wrong ones (work instruction 449,
/// task 1; PHASE_PLAN.md 2.4; HM-REQ-010).
/// </summary>
/// <remarks>
/// <para>**A PRINTER. IT ASSERTS NOTHING ABOUT THE DECODER.** It asserts only that
/// it found the count <see cref="TheRequirementsAreMeasuredTests"/> counts, so the
/// trace and MET-CER-SURE are over the same letters.</para>
/// <para>**A SIBLING OF <see cref="WhereTheSureWrongLettersComeFromTests"/>, NOT AN
/// EXTENSION.** That printer traces substitutions only, on the real set; this one
/// takes the added letters and the synthetic set too, and drives the decoder hop by
/// hop to read the tracker, so it decodes once per recording in its own way.</para>
/// <para>**THE INSTRUMENT IS A RULER HERE AND NOTHING MORE** (447's DECIDED (3)):
/// <see cref="CwPitchInstrument"/> is run on the audio beside the decoder, and
/// nothing it measures is handed to the decoder. **EVERY REAL KEY IS INFERRED**
/// (V-13); **EVERY SYNTHETIC KEY IS EXACT** and never sole evidence (12.5).</para>
/// <para>**WHETHER A LETTER CAME BEFORE THE TRACKER'S FIRST KEYED VERDICT IS
/// PRINTED AS A FACT ONLY.** Acquisition is parked with the owner (448); nothing
/// here proposes gating on it.</para>
/// </remarks>
public sealed class WhatTheFortySureWrongLettersRestOnTests
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tables are printed.</param>
    public WhatTheFortySureWrongLettersRestOnTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>A decode driven hop by hop, with what the tracker and the stream held on every hop.</summary>
    internal sealed record Heard(
        float[] Samples, int SampleRate, IReadOnlyList<CwCharacter> Settled,
        double[] Tracker, double[] Mix, double[] Envelope, int FirstKeyedHop,
        IReadOnlyList<(double UnitMs, double WindowMarksUnitMs, double WindowRatio, bool AtEdge)> Reading);

    /// <summary>One sure letter with everything task 1 asks for.</summary>
    internal sealed record Letter(
        string Name, string Set, CwCharacter Character, string Sent, string Class,
        int StartHop, int EndHop, double UnitMs, double WindowMarksUnitMs, double WindowRatio, bool AtEdge,
        double MixHz, double TrackerHz, double InstrumentHz, string Retune, double RetuneHz,
        IReadOnlyList<double> Marks, IReadOnlyList<double> Gaps, IReadOnlyList<double> EnvelopeGaps,
        bool BeforeVerdict, bool Isolated, double TruePitchHz, double TrueWpm, bool Dim, double Peak, double ModalMixHz)
    {
        /// <summary>Right against the key.</summary>
        public bool IsRight => Class == "right";

        /// <summary>The decoder's pitch less the instrument's, or NaN where the instrument found nothing.</summary>
        public double PitchDiffHz => MixHz - InstrumentHz;

        /// <summary>The worst mark's distance from the nearer of a dit and a dah (445).</summary>
        public double WorstMark => Marks.Count == 0 ? double.PositiveInfinity
            : Marks.Max(m => WhatTheSureLettersMarksLookLikeTests.MarkDistance(m / UnitMs));

        /// <summary>The worst inner gap's distance from one unit (445).</summary>
        public double WorstGap => Gaps.Count == 0 ? 1
            : Gaps.Max(g => WhatTheSureLettersMarksLookLikeTests.Distance(g / UnitMs, 1));

        /// <summary>The unit this letter's own marks imply over the path's (441's printer).</summary>
        public double LocalRatio => WhereTheSureWrongLettersComeFromTests.MarksUnit(Marks, Character.Pattern) / UnitMs;

        /// <summary>The character's own evidence per hop.</summary>
        public double PerHop => Character.SpanMarginForRecord;
    }

    /// <summary>Drives the decoder over one file as the floors do, reading and changing nothing.</summary>
    /// <param name="path">The WAV.</param>
    /// <param name="pitchHz">Where the decoder starts.</param>
    /// <returns>The decode.</returns>
    internal static Heard Drive(string path, double pitchHz)
    {
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var windowField = typeof(CwProbabilisticStream).GetField("_envelope", flags)!;
        var countField = typeof(CwProbabilisticStream).GetField("_envelopeCount", flags)!;
        var keyedField = typeof(CwToneTracker).GetField("_lastKeyedHz", flags)
            ?? throw new InvalidOperationException("CwToneTracker has no field _lastKeyedHz");
        var audio = WavAudio.Read(path);
        var decoder = new CwDecoder(audio.SampleRate, pitchHz);
        var settled = new List<CwCharacter>();
        var reading = new List<(double, double, double, bool)>();
        var tracker = new List<double>();
        var mix = new List<double>();
        var envelope = new List<double>();
        var hopMs = CwProbabilisticDecoder.HopMilliseconds;
        var firstKeyed = -1;

        decoder.CharacterSettled += c =>
        {
            settled.Add(c);

            var last = decoder.Stream.Last;
            var window = ((double[])windowField.GetValue(decoder.Stream)!).Take((int)countField.GetValue(decoder.Stream)!).ToList();
            var windowUnit = WhereTheSureWrongLettersComeFromTests.MarksUnit(CwUnitEstimator.Elements(window, hopMs).Marks, null);
            var unitMs = c.WordsPerMinute > 0 ? 1200.0 / c.WordsPerMinute : double.NaN;

            reading.Add((unitMs, windowUnit, windowUnit / unitMs, last.SpeedIsAtTheEdge));
        };

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
            tracker.Add(decoder.Tracker.ToneHz);
            mix.Add(decoder.Stream.ToneHz);
            envelope.Add(decoder.Stream.NewestEnvelope);

            if (firstKeyed < 0 && !double.IsNaN((double)keyedField.GetValue(decoder.Tracker)!))
            {
                firstKeyed = tracker.Count - 1;
            }
        }

        decoder.Flush();

        return new Heard(audio.Samples, audio.SampleRate, settled, tracker.ToArray(), mix.ToArray(), envelope.ToArray(), firstKeyed, reading);
    }

    private static double Median(IEnumerable<double> values)
    {
        var s = values.Where(v => !double.IsNaN(v)).OrderBy(v => v).ToArray();

        return s.Length == 0 ? double.NaN : s[s.Length / 2];
    }

    /// <summary>Every sure letter over the scored stretches, right, wrong and added.</summary>
    internal static IEnumerable<Letter> Letters(
        string name, string set, Heard heard, IEnumerable<CwScore> scores, CwKeyKind kind, double truePitchHz, double trueWpm)
    {
        var hopMs = CwProbabilisticDecoder.HopMilliseconds;
        var hopsPerSecond = 1000.0 / hopMs;
        var settled = heard.Settled.ToList();
        var windows = CwPitchInstrument.Measure(heard.Samples, heard.SampleRate);

        // The pitch the decoder mixed at on more hops than any other, over the whole file.
        var modal = heard.Mix.GroupBy(hz => Math.Round(hz)).OrderByDescending(g => g.Count()).First().Key;

        foreach (var score in scores)
        {
            var covered = TheRequirementsAreMeasuredTests.Covered(settled, score);
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

                var at = settled.IndexOf(c);
                var (unitMs, windowUnit, windowRatio, atEdge) = heard.Reading[at];
                var endHop = Math.Min(heard.Mix.Length, (int)Math.Round(c.At.TotalMilliseconds / hopMs));
                var startHop = Math.Max(0, endHop - Math.Max(1, c.SpanHops));
                var span = Enumerable.Range(startHop, Math.Max(1, endHop - startHop)).Where(i => i < heard.Mix.Length).ToList();
                var mixHz = Median(span.Select(i => heard.Mix[i]));
                var trackerHz = Median(span.Select(i => heard.Tracker[i]));

                // The instrument window over the letter's middle, else the nearest within a second.
                var mid = (startHop + endHop) / 2.0 / hopsPerSecond;
                var window = windows.FirstOrDefault(w => w.StartSeconds <= mid && w.EndSeconds > mid)
                    ?? windows.Where(w => Math.Min(Math.Abs(w.StartSeconds - mid), Math.Abs(w.EndSeconds - mid)) <= 1)
                        .OrderBy(w => Math.Min(Math.Abs(w.StartSeconds - mid), Math.Abs(w.EndSeconds - mid)))
                        .FirstOrDefault();

                // A retune: a hop on which the tracker's pitch changed by more than half a hertz.
                var near = (int)Math.Round(hopsPerSecond);
                var inside = 0.0;
                var within = 0.0;

                for (var i = Math.Max(1, startHop - near); i < Math.Min(heard.Tracker.Length, endHop + near); i++)
                {
                    var moved = Math.Abs(heard.Tracker[i] - heard.Tracker[i - 1]);

                    if (moved <= 0.5)
                    {
                        continue;
                    }

                    if (i >= startHop && i < endHop)
                    {
                        inside = Math.Max(inside, moved);
                    }
                    else
                    {
                        within = Math.Max(within, moved);
                    }
                }

                var retune = inside > 0 ? "inside" : within > 0 ? "within 1 s" : "no";
                var unitHops = double.IsNaN(unitMs) ? 0 : (int)Math.Round(unitMs / hopMs);
                var from = Math.Max(0, startHop - unitHops);
                var to = Math.Min(heard.Envelope.Length, endHop + unitHops);
                var piece = heard.Envelope.Skip(from).Take(Math.Max(0, to - from)).ToList();
                var (marks, gaps) = CwUnitEstimator.InnerElements(piece, hopMs);
                var (_, envelopeGaps) = CwUnitEstimator.Elements(piece, hopMs);
                var cls = step.Key is null ? "added"
                    : string.Equals(step.Key, step.Decoded.Value.Text, StringComparison.Ordinal) ? "right" : "wrong";
                var previous = at > 0 ? settled[at - 1] : null;
                var next = at + 1 < settled.Count ? settled[at + 1] : null;
                var isolated = (previous is null || previous.IsWordGap) && (next is null || next.IsWordGap);

                yield return new Letter(
                    name, set, c, step.Key ?? "-", cls, startHop, endHop, unitMs, windowUnit, windowRatio, atEdge,
                    mixHz, trackerHz, window?.Hz ?? double.NaN, retune, Math.Max(inside, within),
                    marks, gaps, envelopeGaps, heard.FirstKeyedHop < 0 || endHop <= heard.FirstKeyedHop, isolated,
                    truePitchHz, trueWpm, step.Decoded.Value.Class != CwSymbolClass.Sure,
                    span.Select(i => heard.Envelope[i]).DefaultIfEmpty(double.NaN).Max(), modal);
            }
        }
    }

    /// <summary>The real keyed recordings' sure letters.</summary>
    internal static IReadOnlyList<Letter> Real()
        => WhatTheStrayLettersRestOnTests.KeyedRecordings
            .SelectMany(k =>
            {
                var heard = Drive(Path.Combine(CapturedSignalTests.Folder, k.Name + ".wav"), 600);

                return Letters(k.Name, "real", heard, k.Score(CwReading.Of(heard.Settled)), CwKeyKind.Inferred, double.NaN, double.NaN).ToList();
            })
            .ToList();

    /// <summary>The synthetic set's sure letters.</summary>
    internal static IReadOnlyList<Letter> Synthetic()
        => SyntheticCq.All
            .SelectMany(recipe =>
            {
                var heard = Drive(Path.Combine(SyntheticCq.Folder, recipe.Name + ".wav"), SyntheticCq.StartingPitchHz);
                var reading = CwReading.Of(heard.Settled);
                var from = reading.Text.TakeWhile(char.IsWhiteSpace).Count();

                return Letters(recipe.Name, "synthetic", heard, new[] { SyntheticCq.Whole(reading) with { Start = from } },
                    CwKeyKind.Exact, recipe.ToneHz, 1200.0 / recipe.DitMilliseconds).ToList();
            })
            .ToList();

    /// <summary>A signal the decoder computes, or one of the excluded routes, as a test on a letter.</summary>
    /// <param name="Name">What it is.</param>
    /// <param name="Route">The excluded route it is, or null where it is none of them.</param>
    /// <param name="Test">Whether the letter shows it.</param>
    internal sealed record Signal(string Name, string? Route, Func<Letter, bool> Test);

    /// <summary>The excluded routes of work instruction 449 section 3, each as the test that route used.</summary>
    internal static IReadOnlyList<Signal> Routes(IReadOnlyList<Letter> letters)
    {
        // 445's rule: past the farthest any right letter lies.
        var rights = letters.Where(l => l.IsRight).ToList();
        var markEdge = rights.Where(l => !double.IsInfinity(l.WorstMark)).Select(l => l.WorstMark).DefaultIfEmpty(double.NaN).Max();
        var gapEdge = rights.Select(l => l.WorstGap).DefaultIfEmpty(double.NaN).Max();

        return new[]
        {
            new Signal("rival margin under 1 nat", "442 rival margin", l => l.Character.MarginLlr < 1),
            new Signal("window's marks over 1.25 times the path's unit, or the letter's own", "441 marks' speed",
                l => l.WindowRatio > 1.25 || l.LocalRatio > 1.25),
            new Signal("a key-up under half a unit inside the letter", "444 gap duration",
                l => l.EnvelopeGaps.Any(g => g / l.UnitMs < 0.5)),
            new Signal("worst mark or inner gap past the farthest right letter's", "445 mark-shape edge",
                l => (!double.IsInfinity(l.WorstMark) && l.WorstMark > markEdge) || l.WorstGap > gapEdge),
            new Signal("before the tracker's first keyed verdict", "448 from cold (acquisition, parked)", l => l.BeforeVerdict),
            new Signal("speed at the grid's edge", "446 speed bounds", l => l.AtEdge),
        };
    }

    /// <summary>Signals the decoder already computes that no excluded route used.</summary>
    internal static IReadOnlyList<Signal> Signals { get; } = new[]
    {
        new Signal("mixed 100 Hz or more from the pitch the decoder mixed at longest in the file", null, l => Math.Abs(l.MixHz - l.ModalMixHz) >= 100),
        new Signal("the tracker retuned inside the letter or within 1 s of it", null, l => l.Retune != "no"),
        new Signal("the tracker retuned inside the letter", null, l => l.Retune == "inside"),
        new Signal("a single element (E or T)", null, l => l.Character.Pattern.Length == 1),
        new Signal("alone between two word gaps", null, l => l.Isolated),
        new Signal("single element alone between two word gaps", null, l => l.Character.Pattern.Length == 1 && l.Isolated),
        new Signal("envelope's whole marks differ in number from the pattern's", null, l => l.Marks.Count != l.Character.Pattern.Length),
        new Signal("evidence per hop under 1", null, l => l.PerHop < 1),
        new Signal("evidence per hop under 2", null, l => l.PerHop < 2),
        new Signal("window's marks under 0.80 times the path's unit", null, l => l.WindowRatio < 0.8),
    };

    private static string Ms(IEnumerable<double> ms, double unitMs)
        => string.Join(" ", ms.Select(m => string.Create(Invariant, $"{m:0}/{m / unitMs:0.0}u")));

    private static string Covering(IReadOnlyList<Signal> routes, Letter l)
    {
        var names = routes.Where(r => r.Test(l)).Select(r => r.Route!).ToList();

        return names.Count == 0 ? "none" : string.Join("; ", names);
    }

    private void Print(string set, IReadOnlyList<Letter> all)
    {
        var dim = all.Where(l => l.Dim).ToList();

        _output.WriteLine(string.Create(Invariant,
            $"dim | {set} | dim precision (HM-REQ-014): {dim.Count(l => l.IsRight)} right of {dim.Count} dim, {(dim.Count == 0 ? double.NaN : dim.Count(l => l.IsRight) / (double)dim.Count):0.0000} | "
            + $"dim rate: {dim.Count} dim of {all.Count} named letters emitted in the scored stretches, {(all.Count == 0 ? double.NaN : dim.Count / (double)all.Count):0.0000}"));

        var letters = all.Where(l => !l.Dim).ToList();
        var routes = Routes(letters);
        var bad = letters.Where(l => !l.IsRight).ToList();

        _output.WriteLine(
            "trace | set | recording | at s | span s | sent | emitted | class | pattern | rival | margin nats | wpm in force | wpm the window's marks imply | wpm its own marks imply | "
            + "decoder mix Hz | tracker Hz | instrument Hz | decoder less instrument Hz | tracker retune | largest step Hz | marks ms/u | inner gaps ms/u | "
            + "worst mark | worst gap | 445 verdict | before first keyed verdict | evidence per hop | true pitch Hz | true wpm | excluded routes covering it");

        foreach (var l in bad)
        {
            var c = l.Character;
            var hop = CwProbabilisticDecoder.HopMilliseconds / 1000.0;

            _output.WriteLine(string.Create(Invariant,
                $"trace | {set} | {l.Name} | {c.At.TotalSeconds:0.000} | {l.StartHop * hop:0.00} to {l.EndHop * hop:0.00} | `{l.Sent}` | `{c.Text}` | {l.Class} | {c.Pattern} | "
                + $"{c.RivalReading ?? "-"} | {c.MarginLlr:G4} | {c.WordsPerMinute} | {1200.0 / l.WindowMarksUnitMs:0.0} | {1200.0 / (l.LocalRatio * l.UnitMs):0.0} | "
                + $"{l.MixHz:0.0} | {l.TrackerHz:0.0} | {l.InstrumentHz:0.0} | {l.PitchDiffHz:+0.0;-0.0;0.0} | {l.Retune} | {l.RetuneHz:0.0} | "
                + $"{Ms(l.Marks, l.UnitMs)} | {Ms(l.Gaps, l.UnitMs)} | {l.WorstMark:0.00} | {l.WorstGap:0.00} | "
                + $"{(l.WorstGap <= CwProbabilisticStream.InnerGapEdge ? "inside 6.5, sure" : "past 6.5")} | {(l.BeforeVerdict ? "yes" : "no")} | "
                + $"{l.PerHop:0.00} | {l.TruePitchHz:0} | {l.TrueWpm:0.0} | {Covering(routes, l)}"));
        }

        _output.WriteLine($"route | set | excluded route | its test | wrong or added it covers | right it covers");

        foreach (var r in routes)
        {
            _output.WriteLine($"route | {set} | {r.Route} | {r.Name} | {bad.Count(r.Test)} | {letters.Count(l => l.IsRight && r.Test(l))}");
        }

        var open = bad.Where(l => routes.All(r => !r.Test(l))).ToList();

        _output.WriteLine($"group | set | signal the decoder computes | wrong or added | of them, no excluded route covers | right | right, no excluded route covers");

        foreach (var s in Signals
            .Select(s => (Signal: s, Bad: bad.Count(s.Test), Open: open.Count(s.Test), Right: letters.Count(l => l.IsRight && s.Test(l))))
            .OrderByDescending(x => x.Open).ThenByDescending(x => x.Bad))
        {
            var rightOpen = letters.Count(l => l.IsRight && s.Signal.Test(l) && routes.All(r => !r.Test(l)));

            _output.WriteLine($"group | {set} | {s.Signal.Name} | {s.Bad} | {s.Open} | {s.Right} | {rightOpen}");
        }

        // Where the decoder mixed, recording by recording: right beside wrong at each pitch it held.
        _output.WriteLine("mixed | set | recording | decoder mix Hz | instrument Hz (median) | right | wrong or added | span s");

        foreach (var g in letters
            .GroupBy(l => (l.Name, Hz: Math.Round(l.MixHz)))
            .Where(g => g.Any(l => !l.IsRight) || letters.Any(o => o.Name == g.Key.Name && !o.IsRight))
            .OrderBy(g => g.Key.Name, StringComparer.Ordinal).ThenBy(g => g.Min(l => l.StartHop)))
        {
            var hop = CwProbabilisticDecoder.HopMilliseconds / 1000.0;

            _output.WriteLine(string.Create(Invariant,
                $"mixed | {set} | {g.Key.Name} | {g.Key.Hz:0} | {Median(g.Select(l => l.InstrumentHz)):0.0} | {g.Count(l => l.IsRight)} | {g.Count(l => !l.IsRight)} | "
                + $"{g.Min(l => l.StartHop) * hop:0.0} to {g.Max(l => l.EndHop) * hop:0.0}"));
        }

        // The letter's peak envelope over the median peak of the sure letters in the 10 s before it, same file.
        double LevelRatio(Letter l)
        {
            var hopsPerTen = 10000 / CwProbabilisticDecoder.HopMilliseconds;
            var before = letters.Where(o => o.Name == l.Name && o.EndHop <= l.StartHop && o.EndHop > l.StartHop - hopsPerTen).Select(o => o.Peak).ToList();

            return before.Count < 3 ? double.NaN : l.Peak / Median(before);
        }

        _output.WriteLine("level | set | peak over the last 10 s's median peak | wrong or added | right");

        foreach (var (label, test) in new (string, Func<double, bool>)[]
        {
            ("fewer than 3 letters before", double.IsNaN),
            ("under 0.25", r => r < 0.25),
            ("0.25 to 0.5", r => r >= 0.25 && r < 0.5),
            ("0.5 to 2", r => r >= 0.5 && r < 2),
            ("2 to 4", r => r >= 2 && r < 4),
            ("4 and over", r => r >= 4),
        })
        {
            _output.WriteLine($"level | {set} | {label} | {bad.Count(l => test(LevelRatio(l)))} | {letters.Count(l => l.IsRight && test(LevelRatio(l)))}");
        }

        // The pitch the decoder is on, against the instrument, binned: a signal the decoder does not have.
        _output.WriteLine("pitch | set | decoder less instrument | wrong or added | right");

        foreach (var (label, test) in new (string, Func<Letter, bool>)[]
        {
            ("no instrument window", l => double.IsNaN(l.PitchDiffHz)),
            ("within 12.5 Hz", l => Math.Abs(l.PitchDiffHz) <= 12.5),
            ("12.5 to 25 Hz", l => Math.Abs(l.PitchDiffHz) > 12.5 && Math.Abs(l.PitchDiffHz) <= 25),
            ("more than 25 Hz", l => Math.Abs(l.PitchDiffHz) > 25),
        })
        {
            _output.WriteLine($"pitch | {set} | {label} | {bad.Count(test)} | {letters.Count(l => l.IsRight && test(l))}");
        }

        foreach (var (label, pick, low) in new (string, Func<Letter, double>, bool)[]
        {
            ("evidence per hop", l => l.PerHop, true),
            ("rival margin", l => l.Character.MarginLlr, true),
            ("window ratio", l => l.WindowRatio, false),
        })
        {
            var rights = letters.Where(l => l.IsRight).Select(pick).Where(v => !double.IsNaN(v)).ToList();
            var edge = rights.Count == 0 ? double.NaN : low ? rights.Min() : rights.Max();
            var past = bad.Where(l => low ? pick(l) < edge : pick(l) > edge).ToList();

            var which = string.Join(", ", past.Select(l => string.Create(Invariant,
                $"{l.Name} {l.Character.At.TotalSeconds:0.000} `{l.Sent}`->`{l.Character.Text}` {pick(l):G4}")));

            _output.WriteLine(string.Create(Invariant,
                $"edge | {set} | {label} | farthest right letter {edge:G4} | wrong or added past it {past.Count} | {which}"));
        }

        _output.WriteLine(
            $"total | {set} | {letters.Count} sure: {letters.Count(l => l.IsRight)} right, {letters.Count(l => l.Class == "wrong")} wrong, "
            + $"{letters.Count(l => l.Class == "added")} added | {open.Count} of the {bad.Count} covered by no excluded route | "
            + $"{bad.Count(l => l.BeforeVerdict)} before the first keyed verdict | {bad.Count(l => l.Retune != "no")} with a retune inside or within 1 s | "
            + $"{bad.Count(l => Math.Abs(l.PitchDiffHz) > 25)} mixed more than 25 Hz off the instrument");
    }

    /// <remarks>
    /// Proves nothing about the decoder: every move of the tracker of more than
    /// 15 Hz on the recordings where the trace found it mixing far from the
    /// instrument's pitch, with what its verdict held after the move, as 447's
    /// <see cref="WhatPitchTheDecoderIsOnTests.WhatTheTrackerWeighedWhereItMoved"/>
    /// prints it for its eleven.
    /// </remarks>
    [Fact]
    public void WhereTheTrackerLeftTheStation()
    {
        foreach (var name in new[]
        {
            "unadjudicated/cw-2026-08-22-031905", "unadjudicated/cw-2026-08-22-032050", "unadjudicated/cw-2026-08-22-031838",
        })
        {
            var audio = WavAudio.Read(Path.Combine(CapturedSignalTests.Folder, name + ".wav"));
            var decoder = new CwDecoder(audio.SampleRate, 600);
            var hop = decoder.Tracker.HopSamples;
            var last = decoder.Tracker.ToneHz;

            for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
            {
                decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));

                var now = decoder.Tracker.ToneHz;
                var verdict = decoder.Tracker.Verdict;

                if (verdict.Keyed is { } seen && (at / hop) % 100 == 99)
                {
                    _output.WriteLine(string.Create(Invariant,
                        $"keyed | {name} | {(at + hop) / (double)audio.SampleRate:0.00} | on {now:0.0} | keyed {seen.ToneHz:0} Hz lift {seen.LiftDb:0.0} keyed {seen.KeyedDb:0.0} dB separation {seen.Separation:0.00} marks {seen.Marks} | "
                        + $"strongest {verdict.Strongest?.ToneHz:0} Hz lift {verdict.Strongest?.LiftDb:0.0} present {verdict.Strongest?.PresentFraction:0.00}"));
                }

                if (Math.Abs(now - last) > 15)
                {
                    var v = decoder.Tracker.Verdict;
                    var k = v.Keyed;

                    _output.WriteLine(string.Create(Invariant,
                        $"move | {name} | {(at + hop) / (double)audio.SampleRate:0.00} | {last:0.0} -> {now:0.0} | keyed {(k is not { } c ? "none" : string.Create(Invariant, $"{c.ToneHz:0} Hz lift {c.LiftDb:0.0} dB separation {c.Separation:0.00} ratio {c.Ratio:0.00} marks {c.Marks} keyed {c.KeyedDb:0.0} dB"))} | "
                        + $"verdict {v}"));
                }

                last = now;
            }
        }
    }

    /// <remarks>
    /// Proves nothing about the decoder; prints task 1 of work instruction 449:
    /// every sure-wrong and sure-added letter on the real keyed recordings, then on
    /// the synthetic set, with the groups by signal, right beside wrong. Asserts
    /// only that the real count is MET-CER-SURE's own.
    /// </remarks>
    [Fact]
    public void EverySureWrongLetterWithThePitchBesideIt()
    {
        var real = Real();
        var synthetic = Synthetic();

        Print("real", real);
        Print("synthetic", synthetic);

        var metric = TheRequirementsAreMeasuredTests.Real
            .Where(m => m.NotComputable is null)
            .SelectMany(m => m.Stretches)
            .Sum(a => CwMetrics.Invented(a).SureAdded + CwMetrics.Invented(a).SureWrong);

        Assert.Equal(metric, real.Count(l => !l.Dim && !l.IsRight));
    }
}
