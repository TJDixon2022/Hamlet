using System.Globalization;
using System.Reflection;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Every letter emitted while acquiring, with the first read after it at which both
/// proof states were proved, whether it was still unsettled then, and what its span
/// reads when the window at that read is decoded again at the proved pitch and speed
/// (work instruction 471, task 1; PHASE_PLAN.md 2.4; HM-REQ-010, HM-REQ-102).
/// </summary>
/// <remarks>
/// <para>**A PRINTER. IT ASSERTS NOTHING.**</para>
/// <para>**EMITTED WHILE ACQUIRING IS 470's SENSE**: either
/// <see cref="CwToneTracker.PitchProof"/> or <see cref="CwDecoder.SpeedProof"/> was not
/// proved in the <see cref="CwDecoder.CharacterSettled"/> handler, the read that settled
/// the letter. The states are read again after every read, from
/// <see cref="CwDecoder.LeadingEdge"/>, which the stream raises at the end of each read
/// after it has settled what that read settles; nothing here changes how either is
/// computed.</para>
/// <para>**THE RE-READ IS BUILT OUTSIDE src FROM THE STREAM'S OWN PIECES**: the window the
/// stream held at that read, mixed down again from the raw audio at the proved pitch
/// through the stream's trailing integrator (<see cref="CwProbabilisticDecoder.IntegratorWindow"/>,
/// <see cref="CwProbabilisticDecoder.IntegratorTaper"/>), decoded at the proved speed with
/// the gap lengths that read used, and classed by the stream's own
/// <c>GapsFitTheUnit</c>. The gaps and the class are read by reflection, so nothing
/// under src is changed or copied. It uses no key and no text (R72).</para>
/// <para>**THE SAME HARNESS AND THE SAME ARITHMETIC AS
/// <see cref="TheRequirementsAreMeasuredTests"/>**: the decoder driven hop by hop, flushed,
/// the baseline's scored stretches, <see cref="CwMetrics"/> through
/// <see cref="WhatTheSureLettersWerePrintedUnderFact.Letters"/>. **EVERY REAL KEY IS
/// INFERRED** (V-13); **EVERY SYNTHETIC KEY IS EXACT** and never sole evidence (12.5).</para>
/// </remarks>
public sealed class WhatTheAcquiringLettersReadAtTheProvedValuesFact
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private static readonly FieldInfo HeldField =
        typeof(CwProbabilisticStream).GetField("_structureHeld", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static readonly FieldInfo HeldGapsField =
        typeof(CwProbabilisticStream).GetField("_heldGaps", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static readonly MethodInfo GapsFitTheUnit =
        typeof(CwProbabilisticStream).GetMethod("GapsFitTheUnit", BindingFlags.NonPublic | BindingFlags.Static)!;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fact.</summary>
    /// <param name="output">Where the trace is printed.</param>
    public WhatTheAcquiringLettersReadAtTheProvedValuesFact(ITestOutputHelper output)
        => _output = output;

    /// <summary>One read of the stream, with the states after it.</summary>
    /// <param name="Index">Which read, from nought.</param>
    /// <param name="Hops">Hops the stream had taken in when it read.</param>
    /// <param name="EnvelopeHops">Hops the window held.</param>
    /// <param name="Pitch">The pitch's proof state after the read.</param>
    /// <param name="Speed">The speed's proof state after the read.</param>
    /// <param name="TrackerHz">The pitch the tracker reported.</param>
    /// <param name="StreamHz">The pitch the stream was mixing at.</param>
    /// <param name="Wpm">The speed the read decoded at.</param>
    /// <param name="GapMilliseconds">The gap lengths the read used, or null for one, three and seven units.</param>
    /// <param name="Edge">The leading edge the read offered.</param>
    internal sealed record ReadAt(
        int Index, long Hops, int EnvelopeHops, CwPitchProof Pitch, CwSpeedProof Speed, double TrackerHz, double StreamHz,
        double Wpm, double[]? GapMilliseconds, IReadOnlyList<CwCharacter> Edge)
    {
        /// <summary>Both states proved.</summary>
        public bool Proved => Pitch == CwPitchProof.Proved && Speed == CwSpeedProof.Proved;

        /// <summary>The seconds on the audio clock at the read.</summary>
        public double Seconds => Hops * CwProbabilisticDecoder.HopMilliseconds / 1000.0;
    }

    /// <summary>A decode with every read and, for each settled character, the read that settled it.</summary>
    internal sealed record Drove(
        float[] Samples, int SampleRate, WhatTheSureLettersWerePrintedUnderFact.Run Run, IReadOnlyList<int> SettledAt,
        IReadOnlyList<ReadAt> Reads, IReadOnlyList<double> MixHz);

    /// <summary>A span decoded again at the proved values.</summary>
    /// <param name="Text">What the span reads, the letters in order, or empty where nothing does.</param>
    /// <param name="Letters">How many letters overlap the span.</param>
    /// <param name="Sure">Whether every one of them is sure.</param>
    /// <param name="AnySure">Whether any of them is sure.</param>
    internal readonly record struct Reread(string Text, int Letters, bool Sure, bool AnySure);

    /// <summary>Drives the decoder over audio as the harness does, keeping every read.</summary>
    internal static Drove Drive(float[] samples, int sampleRate, double pitchHz)
    {
        var decoder = new CwDecoder(sampleRate, pitchHz);
        var settled = new List<CwCharacter>();
        var pitch = new List<CwPitchProof>();
        var speed = new List<CwSpeedProof>();
        var settledAt = new List<int>();
        var reads = new List<ReadAt>();
        var mixHz = new List<double>();
        var hopsIn = 0L;

        decoder.CharacterSettled += c =>
        {
            settled.Add(c);
            pitch.Add(decoder.Tracker.PitchProof);
            speed.Add(decoder.SpeedProof);
            settledAt.Add(reads.Count);
        };

        decoder.LeadingEdge += e =>
        {
            var stream = decoder.Stream;
            var held = (bool)HeldField.GetValue(stream)!;
            var gaps = (CwUnitEstimator.CwGapLengths)HeldGapsField.GetValue(stream)!;

            reads.Add(new ReadAt(
                reads.Count, hopsIn, stream.EnvelopeHops, decoder.Tracker.PitchProof, decoder.SpeedProof,
                decoder.Tracker.ToneHz, stream.ToneHz, decoder.Reading.WordsPerMinute,
                held ? new[] { gaps.ElementMilliseconds, gaps.CharacterMilliseconds, gaps.WordMilliseconds } : null,
                e.ToList()));
        };

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= samples.Length; at += hop)
        {
            hopsIn = (at + hop) / hop;
            decoder.Process(new AudioChunk(at, sampleRate, samples.AsSpan((int)at, hop)));
            mixHz.Add(decoder.Stream.ToneHz);
        }

        decoder.Flush();

        return new Drove(samples, sampleRate, new WhatTheSureLettersWerePrintedUnderFact.Run(settled, pitch, speed, double.NaN),
            settledAt, reads, mixHz);
    }

    /// <summary>
    /// The window the stream held at a read, mixed down again from the raw audio at one
    /// pitch through the stream's trailing integrator: hop k is the taper over the newest
    /// integrator length of samples up to the end of hop k, as <c>PushEnvelope</c> takes it.
    /// </summary>
    internal static double[] Remixed(float[] samples, int sampleRate, long hops, int envelopeHops, double toneHz)
    {
        var hopSamples = Math.Max(1, (int)(sampleRate * CwProbabilisticDecoder.HopMilliseconds / 1000.0));
        var length = CwProbabilisticDecoder.IntegratorWindow(sampleRate, CwProbabilisticDecoder.IntegratorBandwidthHz);
        var taper = CwProbabilisticDecoder.IntegratorTaper(length);
        var weight = taper.Sum();
        var first = hops - envelopeHops;
        var from = Math.Max(0, (first + 1) * hopSamples - length);
        var to = hops * hopSamples;
        var mixedI = new float[to - from];
        var mixedQ = new float[to - from];
        var step = 2 * Math.PI * toneHz / sampleRate;

        for (var j = from; j < to; j++)
        {
            var phase = step * j % (2 * Math.PI);

            mixedI[j - from] = (float)(samples[j] * Math.Cos(phase));
            mixedQ[j - from] = (float)(samples[j] * -Math.Sin(phase));
        }

        var envelope = new double[envelopeHops];

        for (var k = 0; k < envelopeHops; k++)
        {
            var end = (first + k + 1) * hopSamples;
            var filled = (int)Math.Min(end, length);
            var oldest = end - filled;
            double i = 0, q = 0;

            for (var n = 0; n < filled; n++)
            {
                var w = taper[length - filled + n];

                i += mixedI[oldest + n - from] * w;
                q += mixedQ[oldest + n - from] * w;
            }

            envelope[k] = Math.Sqrt((i * i) + (q * q)) / weight;
        }

        return envelope;
    }

    private static long EndHop(CwCharacter c)
        => (long)Math.Round(c.At.TotalMilliseconds / CwProbabilisticDecoder.HopMilliseconds);

    private static (long Start, long End) Span(CwCharacter c)
    {
        var end = EndHop(c);

        return (end - Math.Max(1, c.SpanHops), end);
    }

    private static bool Overlaps((long Start, long End) a, (long Start, long End) b)
        => Math.Min(a.End, b.End) - Math.Max(a.Start, b.Start) > 0;

    /// <summary>What a span reads in a decode of the window at a read, at one pitch and speed.</summary>
    internal static Reread ReadAgain(Drove drove, ReadAt read, (long Start, long End) span, Dictionary<int, (double[] Window, CwProbabilisticResult Result)> cache)
    {
        if (!cache.TryGetValue(read.Index, out var decoded))
        {
            var window = Remixed(drove.Samples, drove.SampleRate, read.Hops, read.EnvelopeHops, read.TrackerHz);

            decoded = (window, CwProbabilisticDecoder.Decode(window, read.TrackerHz, read.Wpm, read.GapMilliseconds));
            cache[read.Index] = decoded;
        }

        var first = read.Hops - read.EnvelopeHops;
        var over = decoded.Result.Characters
            .Where(c => c.Pattern.Length > 0)
            .Where(c => Overlaps(span, (first + c.EndHop - Math.Max(1, c.SpanHops), first + c.EndHop)))
            .ToList();

        var sure = over
            .Select(c => c.Text != "#" && (bool)GapsFitTheUnit.Invoke(null, new object[] { c, decoded.Result, decoded.Window })!)
            .ToList();

        return new Reread(
            string.Concat(over.Select(c => c.Text == "#" ? MorseAlphabet.Unreadable : c.Text)),
            over.Count, over.Count > 0 && sure.All(s => s), sure.Any(s => s));
    }

    /// <summary>What HEAD's own read offered on the span at that read, on its leading edge.</summary>
    private static string EdgeAt(ReadAt read, (long Start, long End) span)
        => string.Concat(read.Edge.Where(c => !c.IsWordGap && Overlaps(span, Span(c))).Select(c =>
            CwSymbol.Of(c).Class == CwSymbolClass.Sure ? c.Text : $"({c.Text})"));

    /// <summary>One letter emitted while acquiring, traced to the first proof after it.</summary>
    internal sealed record Traced(
        WhatTheSureLettersWerePrintedUnderFact.Letter Letter, int SettledAt, ReadAt? Proof, bool FirstInRecording,
        bool Unsettled, bool InWindow, Reread? Again, string HeadEdge, double MixHz, double MixOffHz, double ReadWpm)
    {
        /// <summary>The re-read is the one letter the key sent.</summary>
        public bool AgainRight => Again is { Letters: 1 } a && a.Text == Letter.Sent;
    }

    private static List<Traced> Trace(
        string name, string set, string condition, CwKeyKind kind, Drove drove, IEnumerable<CwScore> scores)
    {
        var index = new Dictionary<CwCharacter, int>(ReferenceEqualityComparer.Instance);

        for (var i = 0; i < drove.Run.Settled.Count; i++)
        {
            index[drove.Run.Settled[i]] = i;
        }

        var firstProof = drove.Reads.FirstOrDefault(r => r.Proved);
        var cache = new Dictionary<int, (double[], CwProbabilisticResult)>();
        var traced = new List<Traced>();

        foreach (var l in WhatTheSureLettersWerePrintedUnderFact.Letters(name, set, condition, kind, drove.Run, scores))
        {
            if (!l.Acquiring)
            {
                continue;
            }

            var i = index[l.Character];
            var span = Span(l.Character);
            var settledAt = drove.SettledAt[i];
            var proof = drove.Reads.FirstOrDefault(r => r.Proved && r.Hops > span.End);
            var unsettled = proof is not null && proof.Index < settledAt;
            var inWindow = proof is not null && span.Start >= proof.Hops - proof.EnvelopeHops;
            var again = proof is not null && inWindow ? ReadAgain(drove, proof, span, cache) : (Reread?)null;
            var readAt = drove.Reads[Math.Min(settledAt, drove.Reads.Count - 1)];
            var mixed = drove.MixHz[(int)Math.Clamp(span.End - 1, 0, drove.MixHz.Count - 1)];
            var off = proof is null || !inWindow ? double.NaN
                : Enumerable.Range((int)(proof.Hops - proof.EnvelopeHops), proof.EnvelopeHops)
                    .Max(h => Math.Abs(drove.MixHz[Math.Min(h, drove.MixHz.Count - 1)] - proof.TrackerHz));

            traced.Add(new Traced(
                l, settledAt, proof, proof is not null && ReferenceEquals(proof, firstProof), unsettled, inWindow, again,
                proof is null ? "-" : EdgeAt(proof, span), mixed, off, readAt.Wpm));
        }

        return traced;
    }

    private static string State(CwPitchProof p) => p.ToString().ToLowerInvariant();

    private static string State(CwSpeedProof s) => s.ToString().ToLowerInvariant();

    private void Print(string set, IReadOnlyList<Traced> traced)
    {
        _output.WriteLine(
            "letter | set | recording | span s | sent | HEAD emitted | HEAD class | against key | read at (mix Hz, WPM) | states at emission | "
            + "first both proved after it (read s, pitch Hz, WPM) | first in recording | unsettled then | HEAD's edge then | window mix off proved, max Hz | re-read at proved | re-read class");

        foreach (var t in traced)
        {
            var l = t.Letter;
            var c = l.Character;
            var (start, end) = Span(c);
            var hop = CwProbabilisticDecoder.HopMilliseconds / 1000.0;
            var proof = t.Proof is { } p
                ? string.Create(Invariant, $"{p.Seconds:0.00} s, {p.TrackerHz:0.0} Hz, {p.Wpm:0.0}")
                : "never";
            var again = t.Again is { } a
                ? (a.Letters == 0 ? "(nothing)" : $"`{a.Text}`")
                : t.Proof is null ? "-" : "outside the window";
            var againClass = t.Again is { } b
                ? (b.Letters == 0 ? "-" : b.Sure ? "sure" : b.AnySure ? "mixed" : "dim")
                : "-";

            _output.WriteLine(string.Create(Invariant,
                $"letter | {set} | {l.Name} | {start * hop:0.000} to {end * hop:0.000} | `{l.Sent}` | `{c.Text}` | {(l.Sure ? "sure" : "dim")} | {l.Class} | "
                + $"{t.MixHz:0.0} Hz, {t.ReadWpm:0.0} | pitch {State(l.Pitch)}, speed {State(l.Speed)} | {proof} | {(t.FirstInRecording ? "yes" : "no")} | "
                + $"{(t.Unsettled ? "yes" : "no")} | {(t.Unsettled ? $"`{t.HeadEdge}`" : "-")} | {(double.IsNaN(t.MixOffHz) ? "-" : t.MixOffHz.ToString("0.0", Invariant))} | {again} | {againClass}"));
        }
    }

    private void Tables(string set, IReadOnlyList<Traced> traced)
    {
        _output.WriteLine(
            "reach | set | condition | key | unsettled at proof | of them at the recording's first proof | wrong to right | right to wrong | right to right | wrong to wrong | "
            + "sure to dim | dim to sure | letters added | letters lost");

        foreach (var g in Groups(traced.Where(t => t.Unsettled).ToList()))
        {
            var rows = g.Rows;

            _output.WriteLine(
                $"reach | {set} | {g.Condition} | {g.Kind} | {rows.Count} | {rows.Count(t => t.FirstInRecording)} | "
                + $"{rows.Count(t => !t.Letter.IsRight && t.AgainRight)} | {rows.Count(t => t.Letter.IsRight && !t.AgainRight)} | "
                + $"{rows.Count(t => t.Letter.IsRight && t.AgainRight)} | {rows.Count(t => !t.Letter.IsRight && !t.AgainRight)} | "
                + $"{rows.Count(t => t.Letter.Sure && t.Again is { Letters: > 0, Sure: false })} | {rows.Count(t => !t.Letter.Sure && t.Again is { Sure: true })} | "
                + $"{rows.Sum(t => Math.Max(0, (t.Again?.Letters ?? 0) - 1))} | {rows.Count(t => t.Again is null or { Letters: 0 })}");
        }

        _output.WriteLine(
            "out of reach | set | condition | key | settled before proof | sure right | sure wrong or added | dim | never proved after it | "
            + "of them, what an unbounded re-read would give: wrong to right | right to wrong | outside the window at proof");

        foreach (var g in Groups(traced.Where(t => !t.Unsettled).ToList()))
        {
            var rows = g.Rows;

            _output.WriteLine(
                $"out of reach | {set} | {g.Condition} | {g.Kind} | {rows.Count} | {rows.Count(t => t.Letter.Sure && t.Letter.IsRight)} | "
                + $"{rows.Count(t => t.Letter.Sure && !t.Letter.IsRight)} | {rows.Count(t => !t.Letter.Sure)} | {rows.Count(t => t.Proof is null)} | "
                + $"{rows.Count(t => !t.Letter.IsRight && t.AgainRight)} | {rows.Count(t => t.Letter.IsRight && t.Again is not null && !t.AgainRight)} | "
                + $"{rows.Count(t => t.Proof is not null && !t.InWindow)}");
        }
    }

    private static IEnumerable<(string Condition, string Kind, IReadOnlyList<Traced> Rows)> Groups(IReadOnlyList<Traced> rows)
    {
        foreach (var g in rows.GroupBy(t => t.Letter.Condition).OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            yield return (g.Key, string.Join(" and ", g.Select(t => CwMetrics.KindWord(t.Letter.Kind)).Distinct()), g.ToList());
        }

        yield return ("all", string.Join(" and ", rows.Select(t => CwMetrics.KindWord(t.Letter.Kind)).Distinct()), rows);
    }

    private void Proofs(string set, string name, Drove drove)
    {
        var reads = drove.Reads;
        var first = reads.FirstOrDefault(r => r.Proved);
        var entries = reads.Where((r, i) => r.Proved && (i == 0 || !reads[i - 1].Proved)).Count();
        var settledBefore = first is null ? drove.Run.Settled.Count(c => !c.IsWordGap)
            : drove.SettledAt.Where((s, i) => !drove.Run.Settled[i].IsWordGap && s < first.Index).Count();

        _output.WriteLine(string.Create(Invariant,
            $"proof | {set} | {name} | reads {reads.Count} | first both proved {(first is null ? "never" : $"{first.Seconds:0.00} s at {first.TrackerHz:0.0} Hz, {first.Wpm:0.0} WPM")} | "
            + $"times both became proved {entries} | reads both proved {reads.Count(r => r.Proved)} | letters settled before the first proof {settledBefore}"));
    }

    /// <remarks>
    /// Proves nothing about the decoder; prints task 1 of work instruction 471: every letter
    /// the 23 real keyed recordings and the 12 synthetic cases emitted while acquiring, over
    /// the scored stretches, with the first read after it at which both proof states were
    /// proved, whether it was still unsettled then, and what its span reads when that read's
    /// window is decoded again at the proved pitch and speed; the table of what the re-read
    /// can reach and the count of what settled before proof, per condition with the key's
    /// kind; and where unit 449's thirteen and unit 470's twenty-nine fall.
    /// </remarks>
    [Fact]
    public void EveryAcquiringLetterAtTheFirstProofAfterIt()
    {
        var real = new List<Traced>();
        var realSure = (Emitted: 0, Wrong: 0);

        foreach (var k in WhatTheStrayLettersRestOnTests.KeyedRecordings)
        {
            var audio = WavAudio.Read(Path.Combine(CapturedSignalTests.Folder, k.Name + ".wav"));
            var drove = Drive(audio.Samples, audio.SampleRate, 600);
            var scores = k.Score(CwReading.Of(drove.Run.Settled)).ToList();
            var condition = TheRequirementsAreMeasuredTests.RealCondition(k.Name);
            var all = WhatTheSureLettersWerePrintedUnderFact.Letters(k.Name, "real", condition, CwKeyKind.Inferred, drove.Run, scores)
                .Where(l => l.Sure).ToList();

            realSure = (realSure.Emitted + all.Count, realSure.Wrong + all.Count(l => !l.IsRight));
            real.AddRange(Trace(k.Name, "real", condition, CwKeyKind.Inferred, drove, scores));
            Proofs("real", k.Name, drove);
        }

        var synthetic = new List<Traced>();
        var synthSure = (Emitted: 0, Wrong: 0);

        foreach (var recipe in SyntheticCq.All)
        {
            var audio = WavAudio.Read(Path.Combine(SyntheticCq.Folder, recipe.Name + ".wav"));
            var drove = Drive(audio.Samples, audio.SampleRate, SyntheticCq.StartingPitchHz);
            var reading = CwReading.Of(drove.Run.Settled);
            var from = reading.Text.TakeWhile(char.IsWhiteSpace).Count();
            var scores = new[] { SyntheticCq.Whole(reading) with { Start = from } };
            var condition = TheRequirementsAreMeasuredTests.SyntheticCondition(recipe);
            var all = WhatTheSureLettersWerePrintedUnderFact.Letters(recipe.Name, "synthetic", condition, CwKeyKind.Exact, drove.Run, scores)
                .Where(l => l.Sure).ToList();

            synthSure = (synthSure.Emitted + all.Count, synthSure.Wrong + all.Count(l => !l.IsRight));
            synthetic.AddRange(Trace(recipe.Name, "synthetic", condition, CwKeyKind.Exact, drove, scores));
            Proofs("synthetic", recipe.Name, drove);
        }

        Print("real", real);
        Print("synthetic", synthetic);
        Tables("real", real);
        Tables("synthetic", synthetic);

        _output.WriteLine("449 | recording | at s | sent | emitted | found | unsettled at proof | first both proved after it | re-read at proved");

        foreach (var t in WhatTheSureLettersWerePrintedUnderFact.FourFortyNinesThirteen)
        {
            var hit = real.FirstOrDefault(r => r.Letter.Name == t.Name
                && Math.Abs(r.Letter.Character.At.TotalSeconds - t.At) < 0.0005 && r.Letter.Character.Text == t.Emitted);

            var found = hit is null ? "no | - | - | -"
                : string.Create(Invariant,
                    $"yes | {(hit.Unsettled ? "yes" : "no")} | {(hit.Proof is { } p ? string.Create(Invariant, $"{p.Seconds:0.00} s") : "never")} | {(hit.Again is { } a ? $"`{a.Text}`" : "-")}");

            _output.WriteLine(string.Create(Invariant, $"449 | {t.Name} | {t.At:0.000} | `{t.Sent}` | `{t.Emitted}` | {found}"));
        }

        var twentyNine = real.Where(t => t.Letter.Sure && !t.Letter.IsRight).ToList();

        _output.WriteLine("470 | recording | at s | sent | emitted | class | unsettled at proof | first both proved after it | re-read at proved");

        foreach (var t in twentyNine)
        {
            _output.WriteLine(string.Create(Invariant,
                $"470 | {t.Letter.Name} | {t.Letter.Character.At.TotalSeconds:0.000} | `{t.Letter.Sent}` | `{t.Letter.Character.Text}` | {t.Letter.Class} | "
                + $"{(t.Unsettled ? "yes" : "no")} | {(t.Proof is { } p ? $"{p.Seconds:0.00} s" : "never")} | {(t.Again is { } a ? $"`{a.Text}`" : "-")}"));
        }

        foreach (var (set, traced, sure) in new[] { ("real", real, realSure), ("synthetic", synthetic, synthSure) })
        {
            var reach = traced.Where(t => t.Unsettled).ToList();

            _output.WriteLine(
                $"total | {set} | sure over the scored stretches {sure.Emitted}, wrong or added {sure.Wrong} (cross-check against TheRequirementsAreMeasuredTests) | "
                + $"letters emitted while acquiring {traced.Count} ({traced.Count(t => t.Letter.Sure)} sure, {traced.Count(t => t.Letter.Sure && !t.Letter.IsRight)} of them wrong or added) | "
                + $"unsettled at the first proof after them {reach.Count} ({reach.Count(t => t.Letter.Sure && !t.Letter.IsRight)} sure wrong or added) | "
                + $"settled before it {traced.Count - reach.Count - traced.Count(t => t.Proof is null)} | never proved after them {traced.Count(t => t.Proof is null)}");
        }

        _output.WriteLine(
            $"total | 470's sure wrong or added inside acquiring, real | {twentyNine.Count} | unsettled at proof {twentyNine.Count(t => t.Unsettled)} | "
            + $"settled before proof {twentyNine.Count(t => !t.Unsettled && t.Proof is not null)} | never proved after them {twentyNine.Count(t => t.Proof is null)}");
    }
}
