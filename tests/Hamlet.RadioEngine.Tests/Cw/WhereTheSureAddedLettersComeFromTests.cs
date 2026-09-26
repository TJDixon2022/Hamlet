using System.Globalization;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Every character the decoder emitted sure where the key says nothing was sent,
/// traced and grouped by what they share (work instruction 443, task 1;
/// PHASE_PLAN.md 3.1; HM-REQ-011). Written beside
/// <see cref="WhereTheSureWrongLettersComeFromTests"/>, which traces the wrong ones
/// and is left as it is.
/// </summary>
/// <remarks>
/// <para>**A PRINTER. IT ASSERTS NOTHING ABOUT THE DECODER.** It asserts only that
/// it found the same sure-added count <see cref="TheRequirementsAreMeasuredTests"/>
/// counts, so the trace and the metric are over the same characters.</para>
/// <para>**SELF-CONTAINED ON PURPOSE.** It reads nothing the decoder gained after
/// 1fb0bad6, so the same file runs against the decoder as it stood there, where
/// unit 439 measured 13 added, and at HEAD, and the two printouts can be matched
/// line by line.</para>
/// <para>**REAL KEYS ARE INFERRED** (R61): an added character against one is an
/// indication and not proof (V-13). **SYNTHETIC KEYS ARE EXACT** and are never
/// sole evidence (12.5).</para>
/// <para>**ENERGY AGAINST NOISE: THE DECODER DOES NOT CARRY THAT NUMBER AT THE
/// HOP.** Its noise scale is taken inside <see cref="CwProbabilisticDecoder.LogLikelihoods(IReadOnlyList{double}, double)"/>
/// and not exposed, and the probabilistic path leaves
/// <see cref="CwCharacter.SignalToNoiseDb"/> at NaN. What it does carry is the
/// span's log-likelihood ratio against silence, printed as the decoder's own
/// figure. Beside it the printer takes its own: the envelope's mean over the
/// character's marks against a noise scale it recomputes by the decoder's
/// quarter-point identity (<see cref="CwProbabilisticDecoder.RayleighQuarterPoint"/>)
/// over <see cref="CwProbabilisticDecoder.NoiseSpanSeconds"/> centred on the
/// character. That second figure is the printer's, not the decoder's.</para>
/// </remarks>
public sealed class WhereTheSureAddedLettersComeFromTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tables are printed.</param>
    public WhereTheSureAddedLettersComeFromTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>A decode with its per-hop pitch and envelope.</summary>
    private sealed record Heard(IReadOnlyList<CwCharacter> Settled, double[] Tone, double[] Envelope);

    /// <summary>One source of keyed decodes: its settled characters and its scores.</summary>
    private sealed record Source(string Name, string Set, CwKeyKind Kind, Heard Heard, IReadOnlyList<CwScore> Scores);

    /// <summary>One sure added character, with everything task 1 asks for.</summary>
    private sealed record Added(
        string Name, string Set, CwKeyKind Kind, CwCharacter Character,
        string SentBefore, string SentAfter, string EmittedBefore, string EmittedAfter,
        double SpanBefore, double SpanAfter, double GapBeforeMs, double GapAfterMs,
        double ToneHz, double PitchHz, IReadOnlyList<double> Marks, IReadOnlyList<double> Gaps,
        string Boundary, double LevelOverNoiseDb, double RecordingLevelOverNoiseDb, int RightBefore)
    {
        public double UnitMs => Character.WordsPerMinute > 0 ? 1200.0 / Character.WordsPerMinute : double.NaN;
    }

    private static Heard Decode(string path, double pitchHz)
    {
        var audio = Hamlet.RadioEngine.Audio.WavAudio.Read(path);
        var decoder = new CwDecoder(audio.SampleRate, pitchHz);
        var settled = new List<CwCharacter>();
        var tone = new List<double>();
        var envelope = new List<double>();

        decoder.CharacterSettled += settled.Add;

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new Hamlet.RadioEngine.Audio.AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
            tone.Add(decoder.Stream.ToneHz);
            envelope.Add(decoder.Stream.NewestEnvelope);
        }

        decoder.Flush();

        return new Heard(settled, tone.ToArray(), envelope.ToArray());
    }

    private static IEnumerable<Source> Sources()
    {
        foreach (var keyed in WhatTheStrayLettersRestOnTests.KeyedRecordings)
        {
            var heard = Decode(Path.Combine(CapturedSignalTests.Folder, keyed.Name + ".wav"), 600);

            yield return new Source(keyed.Name, "real", CwKeyKind.Inferred, heard, keyed.Score(CwReading.Of(heard.Settled)));
        }

        foreach (var recipe in SyntheticCq.All)
        {
            var heard = Decode(Path.Combine(SyntheticCq.Folder, recipe.Name + ".wav"), SyntheticCq.StartingPitchHz);
            var reading = CwReading.Of(heard.Settled);
            var from = reading.Text.TakeWhile(char.IsWhiteSpace).Count();

            yield return new Source(recipe.Name, "synthetic", CwKeyKind.Exact, heard, new[] { SyntheticCq.Whole(reading) with { Start = from } });
        }
    }

    private static double Median(IEnumerable<double> values)
    {
        var sorted = values.Where(v => !double.IsNaN(v)).OrderBy(v => v).ToArray();

        return sorted.Length == 0 ? double.NaN : sorted[sorted.Length / 2];
    }

    private static List<Added> Trace()
    {
        var hopMs = CwProbabilisticDecoder.HopMilliseconds;
        var added = new List<Added>();

        foreach (var source in Sources())
        {
            var heard = source.Heard;
            var settled = heard.Settled;

            int EndHop(CwCharacter c) => (int)Math.Round(c.At.TotalMilliseconds / hopMs);
            int StartHop(CwCharacter c) => EndHop(c) - c.SpanHops;

            double ToneOver(CwCharacter c)
            {
                var from = Math.Max(0, StartHop(c));
                var to = Math.Min(heard.Tone.Length, Math.Max(from + 1, EndHop(c)));

                return to > from ? Median(heard.Tone.Skip(from).Take(to - from)) : double.NaN;
            }

            // The printer's own level: the envelope over the character's marks against
            // the decoder's quarter-point noise scale, recomputed over the decoder's span.
            double LevelOverNoiseDb(CwCharacter c)
            {
                var start = Math.Max(0, StartHop(c));
                var end = Math.Min(heard.Envelope.Length, EndHop(c));

                if (end <= start)
                {
                    return double.NaN;
                }

                var span = (int)(CwProbabilisticDecoder.NoiseSpanSeconds * 1000.0 / hopMs);
                var middle = (start + end) / 2;
                var from = Math.Clamp(middle - (span / 2), 0, Math.Max(0, heard.Envelope.Length - span));
                var around = heard.Envelope.Skip(from).Take(span).OrderBy(e => e).ToArray();

                if (around.Length == 0)
                {
                    return double.NaN;
                }

                var sigma = around[around.Length / 4] / CwProbabilisticDecoder.RayleighQuarterPoint;
                var own = heard.Envelope.Skip(start).Take(end - start).ToArray();
                var threshold = own.Max() / 2;
                var marks = own.Where(e => e >= threshold).ToArray();

                return sigma > 0 && marks.Length > 0 ? 20 * Math.Log10(marks.Average() / sigma) : double.NaN;
            }

            var named = settled.Where(WhatTheStrayLettersRestOnTests.IsNamed).ToList();
            var pitch = Median(named.Select(ToneOver));
            var recordingLevel = Median(named.Select(LevelOverNoiseDb));

            foreach (var score in source.Scores)
            {
                var covered = TheRequirementsAreMeasuredTests.Covered(settled, score);
                var characters = covered.Where(c => !c.IsWordGap).ToList();
                var alignment = CwMetrics.Align(CwMetrics.Symbols(covered), score.Key, source.Kind);
                var keyBoundaries = alignment.KeyBoundaries.ToHashSet();
                var keyText = CwMetrics.KeySymbols(score.Key);
                var d = 0;
                var rightSoFar = 0;

                for (var s = 0; s < alignment.Steps.Count; s++)
                {
                    var step = alignment.Steps[s];

                    if (step.Decoded is null)
                    {
                        continue;
                    }

                    var c = characters[d++];

                    if (step.Decoded.Value.Class == CwSymbolClass.Sure
                        && step.Key is not null
                        && string.Equals(step.Key, step.Decoded.Value.Text, StringComparison.Ordinal))
                    {
                        rightSoFar++;
                    }

                    if (step.Decoded.Value.Class != CwSymbolClass.Sure || step.Key is not null)
                    {
                        continue;
                    }

                    var at = settled.ToList().IndexOf(c);
                    var before = settled.Take(at).LastOrDefault(WhatTheStrayLettersRestOnTests.IsNamed);
                    var after = settled.Skip(at + 1).FirstOrDefault(WhatTheStrayLettersRestOnTests.IsNamed);

                    // The key's text either side of the point the added character sits at, spaces kept.
                    var (sentBefore, sentAfter) = KeyAround(keyText, step.KeyBefore, 8);

                    // The decoder's text either side of it, word gaps as spaces.
                    var emittedBefore = string.Concat(settled.Take(at).Reverse().Take(10).Reverse().Select(x => x.IsWordGap ? " " : x.Text));
                    var emittedAfter = string.Concat(settled.Skip(at + 1).Take(10).Select(x => x.IsWordGap ? " " : x.Text));

                    // A boundary beside it: one the decoder put where the key has none, or one
                    // the key has at this point where the decoder put none on either side.
                    var next = alignment.Steps.Skip(s + 1).FirstOrDefault(x => x.Decoded is not null);
                    var gapAfter = next.Decoded is not null && next.GapBefore;
                    var inserted = (step.GapBefore && !keyBoundaries.Contains(step.KeyBefore))
                        || (gapAfter && !keyBoundaries.Contains(next.KeyBefore));
                    var missed = keyBoundaries.Contains(step.KeyBefore) && !step.GapBefore && !gapAfter;
                    var boundary = inserted && missed ? "inserted and missed"
                        : inserted ? "beside an inserted space"
                        : missed ? "beside a missed space"
                        : "no boundary error beside it";

                    var unitHops = c.WordsPerMinute > 0 ? (int)Math.Round(1200.0 / c.WordsPerMinute / hopMs) : 0;
                    var from = Math.Max(0, StartHop(c) - unitHops);
                    var to = Math.Min(heard.Envelope.Length, EndHop(c) + unitHops);
                    var (marks, gaps) = to > from
                        ? CwUnitEstimator.Elements(heard.Envelope.Skip(from).Take(to - from).ToList(), hopMs)
                        : (Array.Empty<double>(), Array.Empty<double>());

                    var gapBeforeMs = before is null ? double.NaN : (StartHop(c) - EndHop(before)) * hopMs;
                    var gapAfterMs = after is null ? double.NaN : (StartHop(after) - EndHop(c)) * hopMs;

                    added.Add(new Added(
                        source.Name, source.Set, source.Kind, c, sentBefore, sentAfter, emittedBefore, emittedAfter,
                        before?.SpanLogLikelihoodRatio ?? double.NaN, after?.SpanLogLikelihoodRatio ?? double.NaN,
                        gapBeforeMs, gapAfterMs, ToneOver(c), pitch, marks, gaps, boundary,
                        LevelOverNoiseDb(c), recordingLevel, rightSoFar));
                }
            }
        }

        return added;
    }

    private static (string Before, string After) KeyAround(IReadOnlyList<CwSymbol> key, int keyBefore, int count)
    {
        var index = new List<int>();

        for (var i = 0; i < key.Count; i++)
        {
            if (key[i].Class != CwSymbolClass.WordGap)
            {
                index.Add(i);
            }
        }

        // The position in the key's symbols the added character sits before.
        var cut = keyBefore < index.Count ? index[keyBefore] : key.Count;

        // Take a following space into "before", so a gap in the key shows where it is.
        var before = key.Take(cut).Reverse().Take(count).Reverse().Select(x => x.Text);
        var after = key.Skip(cut).Take(count).Select(x => x.Text);

        return (string.Concat(before), string.Concat(after));
    }

    private static string Units(IEnumerable<double> ms, double unit)
        => string.Join(" ", ms.Select(m => string.Create(CultureInfo.InvariantCulture, $"{m:0}ms/{m / unit:0.0}u")));

    private static string Shape(Added a) => a.Character.Pattern.Length == 1 ? "one element" : $"{a.Character.Pattern.Length} elements";

    private static string Overlap(Added a)
        => a.GapBeforeMs < 0 || a.GapAfterMs < 0 ? "its span overlaps a neighbor's: the same marks read twice" : "its span is its own";

    private static string Isolation(Added a)
    {
        var shortest = Math.Min(
            double.IsNaN(a.GapBeforeMs) ? double.PositiveInfinity : a.GapBeforeMs,
            double.IsNaN(a.GapAfterMs) ? double.PositiveInfinity : a.GapAfterMs) / a.UnitMs;

        return shortest >= 5 ? "alone in a word-length gap both sides" : shortest >= 2 ? "character gaps both sides" : "crowded, under two units to a neighbor";
    }

    private static string Level(Added a)
    {
        var below = a.RecordingLevelOverNoiseDb - a.LevelOverNoiseDb;

        return double.IsNaN(below) ? "level unmeasured" : below >= 6 ? "6 dB or more below the recording's letters" : "level with the recording's letters";
    }

    private static string Acquisition(Added a) => a.RightBefore == 0 ? "before the first sure-right letter" : "after acquisition";

    private static string Off(Added a) => Math.Abs(a.ToneHz - a.PitchHz) >= 25 ? "25 Hz or more off the sender" : "on the sender's pitch";

    /// <remarks>
    /// Proves nothing about the decoder; prints what the operator reads on every
    /// capture the floor holds and every synthetic case, settled text with word
    /// gaps as spaces, placeholders as they print, and every letter not sure in
    /// brackets, so a change's before and after can be read recording by
    /// recording (work instruction 443, task 2). Asserts only that it printed a
    /// line for every floor row.
    /// </remarks>
    [Fact]
    public void EveryRecordingAsTheOperatorReadsIt()
    {
        var names = TheCapturesThatDecodeKeepDecodingTests.Floors.Select(row => (string)row[0]).ToList();
        var printed = 0;

        foreach (var name in names)
        {
            var heard = Decode(Path.Combine(CapturedSignalTests.Folder, name + ".wav"), 600);

            _output.WriteLine($"text | {name} | {Text(heard.Settled)}");
            printed++;
        }

        foreach (var recipe in SyntheticCq.All)
        {
            var heard = Decode(Path.Combine(SyntheticCq.Folder, recipe.Name + ".wav"), SyntheticCq.StartingPitchHz);

            _output.WriteLine($"text | {recipe.Name} | {Text(heard.Settled)}");
        }

        Assert.Equal(names.Count, printed);
    }

    private static string Text(IEnumerable<CwCharacter> settled)
        => string.Concat(settled.Select(c =>
            c.IsWordGap ? " "
            : c.IsUnreadable || c.Confidence == CwConfidence.High ? c.Text
            : $"[{c.Text}]"));

    /// <remarks>
    /// Proves 3.1: every sure character MET-INVENTED counts as added, on the real
    /// keyed recordings and the synthetic set, with its recording and time, the
    /// key's text and the decoder's either side, what was emitted and its
    /// pattern, its span and its neighbors', the gaps to them, the speed and unit,
    /// the pitch at the hop and the sender's, the envelope's marks and gaps under
    /// it in milliseconds and units, whether it sits beside an inserted or a
    /// missed word space, and its energy against the noise as the decoder carries
    /// it and as the printer measures it. Then the groups by what they share.
    /// Asserts only that it traced the metric's own count on each set.
    /// </remarks>
    [Fact]
    public void EverySureAddedCharacterAndWhatItShares()
    {
        var added = Trace();

        _output.WriteLine(
            "added | recording | set | key | at s | sent before | sent after | emitted before | emitted | emitted after | pattern | "
            + "span | span before | span after | gap before ms/u | gap after ms/u | wpm | unit ms | pitch at hop Hz | sender pitch Hz | off Hz | "
            + "word space | envelope marks | envelope gaps | decoder SignalToNoiseDb | span per hop | printer level over noise dB | recording median dB | sure-right before it");

        foreach (var a in added)
        {
            var c = a.Character;

            _output.WriteLine(string.Create(CultureInfo.InvariantCulture,
                $"added | {a.Name} | {a.Set} | {CwMetrics.KindWord(a.Kind)} | {c.At.TotalSeconds:0.000} | `{a.SentBefore}` | `{a.SentAfter}` | "
                + $"`{a.EmittedBefore}` | `{c.Text}` | `{a.EmittedAfter}` | {c.Pattern} | "
                + $"{c.SpanLogLikelihoodRatio:G4} | {a.SpanBefore:G4} | {a.SpanAfter:G4} | "
                + $"{a.GapBeforeMs:0}/{a.GapBeforeMs / a.UnitMs:0.0}u | {a.GapAfterMs:0}/{a.GapAfterMs / a.UnitMs:0.0}u | "
                + $"{c.WordsPerMinute} | {a.UnitMs:0.0} | {a.ToneHz:0.0} | {a.PitchHz:0.0} | {Math.Abs(a.ToneHz - a.PitchHz):0.0} | {a.Boundary} | "
                + $"{Units(a.Marks, a.UnitMs)} | {Units(a.Gaps, a.UnitMs)} | "
                + $"{(double.IsNaN(c.SignalToNoiseDb) ? "NaN, not carried" : c.SignalToNoiseDb.ToString("0.0", CultureInfo.InvariantCulture))} | "
                + $"{c.SpanLogLikelihoodRatio / Math.Max(1, c.SpanHops):G4} | {a.LevelOverNoiseDb:0.0} | {a.RecordingLevelOverNoiseDb:0.0} | {a.RightBefore}"));
        }

        foreach (var (label, pick) in new (string, Func<Added, string>)[]
        {
            ("shape", Shape),
            ("word space", a => a.Boundary),
            ("overlap", Overlap),
            ("isolation", Isolation),
            ("level", Level),
            ("acquisition", Acquisition),
            ("pitch", Off),
        })
        {
            foreach (var g in added.GroupBy(a => (a.Set, Value: pick(a))).OrderBy(g => g.Key.Set).ThenByDescending(g => g.Count()))
            {
                _output.WriteLine($"feature | {g.Key.Set} | {label} | {g.Key.Value} | {g.Count()}");
            }
        }

        foreach (var set in new[] { "real", "synthetic" })
        {
            var groups = added.Where(a => a.Set == set)
                .GroupBy(a => $"{Overlap(a)}; {Shape(a)}; {a.Boundary}; {Isolation(a)}; {Level(a)}; {Acquisition(a)}")
                .OrderByDescending(g => g.Count())
                .ToList();

            foreach (var g in groups)
            {
                _output.WriteLine(
                    $"group | {set} | {g.Key} | {g.Count()} | "
                    + string.Join(", ", g.Select(a => string.Create(CultureInfo.InvariantCulture, $"{a.Name}@{a.Character.At.TotalSeconds:0.000} `{a.Character.Text}`"))));
            }

            _output.WriteLine(groups.Count == 0
                ? $"largest | {set} | none: nothing added"
                : $"largest | {set} | {groups[0].Key} | {groups[0].Count()} of {groups.Sum(g => g.Count())}"
                  + (groups.Count > 1 && groups[1].Count() == groups[0].Count() ? " | tied with the next" : string.Empty));
        }

        var realMetric = TheRequirementsAreMeasuredTests.Real
            .Where(m => m.NotComputable is null)
            .SelectMany(m => m.Stretches)
            .Sum(a => CwMetrics.Invented(a).SureAdded);
        var syntheticMetric = TheRequirementsAreMeasuredTests.Synthetic
            .Where(m => m.NotComputable is null)
            .SelectMany(m => m.Stretches)
            .Sum(a => CwMetrics.Invented(a).SureAdded);

        _output.WriteLine(
            $"total | {added.Count(a => a.Set == "real")} sure added on the real keyed recordings, inferred keys (metric {realMetric}) | "
            + $"{added.Count(a => a.Set == "synthetic")} on the synthetic set, exact keys (metric {syntheticMetric})");

        Assert.Equal(realMetric, added.Count(a => a.Set == "real"));
        Assert.Equal(syntheticMetric, added.Count(a => a.Set == "synthetic"));
    }
}
