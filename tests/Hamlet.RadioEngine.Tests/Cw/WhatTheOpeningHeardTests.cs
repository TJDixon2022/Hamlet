using System.Globalization;
using System.Reflection;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// What the decoder held while the 7.052 session of 2026-09-24 read `E ET E E`,
/// and what it held once it had locked and read `DE KA2 G J V` (work instruction
/// 429; PHASE_PLAN.md criterion 7.3, R68).
/// </summary>
/// <remarks>
/// <para>**A PRINTER. IT ASSERTS NOTHING.** 7.3 asks that the difference between
/// the opening and the locked stretch be named from the decoder's own figures.
/// This prints the figures; nothing is decided in it and nothing under `src`
/// changes on its account.</para>
/// <para>**THE DECODER IS DRIVEN A HOP AT A TIME, AS THE CAPTURES TYPE DRIVES IT**
/// (`TheCapturesThatDecodeKeepDecodingTests`), with the operator's 600 Hz as the
/// place to start looking. `CwDecoder.Process` walks any chunk a hop at a time, so
/// this is the path the application takes.</para>
/// <para>**FOUR PRIVATE FIELDS OF <see cref="CwProbabilisticStream"/> ARE READ BY
/// REFLECTION AND NONE IS WRITTEN**: the envelope window each read decodes, whether
/// the sender's gap structure is held, the held gaps, and the audio clock. The
/// estimator's unit is not kept by the stream, so it is measured again here by
/// <see cref="CwUnitEstimator.Measure"/> on the very window the read decoded,
/// which is the call the stream itself makes (`CwProbabilisticStream.Read`).</para>
/// </remarks>
public sealed class WhatTheOpeningHeardTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tables are printed.</param>
    public WhatTheOpeningHeardTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The session's recordings, in the order they were kept.</summary>
    internal static readonly string[] Session =
    {
        "cw-2026-09-24-003901", "cw-2026-09-24-003919", "cw-2026-09-24-004027",
        "cw-2026-09-24-004108", "cw-2026-09-24-004133", "cw-2026-09-24-004205",
        "cw-2026-09-24-004234", "cw-2026-09-24-004322", "cw-2026-09-24-004347",
        "cw-2026-09-24-004405", "cw-2026-09-24-004427", "cw-2026-09-24-004510",
        "cw-2026-09-24-004535", "cw-2026-09-24-004550",
    };

    private static readonly string Folder = Path.Combine(CapturedSignalTests.Folder, "unadjudicated");

    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private static readonly FieldInfo EnvelopeField = Private("_envelope");
    private static readonly FieldInfo StructureField = Private("_structureHeld");
    private static readonly FieldInfo HeldGapsField = Private("_heldGaps");
    private static readonly FieldInfo HopsSeenField = Private("_hopsSeen");

    private static FieldInfo Private(string name)
        => typeof(CwProbabilisticStream).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)
           ?? throw new InvalidOperationException($"CwProbabilisticStream has no field {name}");

    /// <summary>A sidecar's fields, first word the key.</summary>
    internal static Dictionary<string, string> Sidecar(string name)
    {
        var fields = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var line in File.ReadAllLines(Path.Combine(Folder, name + ".txt")))
        {
            var space = line.IndexOf(' ');

            if (space > 0 && !char.IsWhiteSpace(line[0]) && !fields.ContainsKey(line[..space]))
            {
                fields[line[..space]] = line[(space + 1)..].TrimStart();
            }
        }

        return fields;
    }

    /// <summary>The sidecar's `audioSeen`: the live decoder's own sample clock when the file was kept.</summary>
    internal static long AudioSeen(string name)
        => long.Parse(Sidecar(name)["audioSeen"].Split(' ')[0], Invariant);

    /// <summary>What one read of the window held.</summary>
    /// <param name="Hop">The hop of the audio clock the read was made on.</param>
    /// <param name="HeldWpm">The speed the read decoded at.</param>
    /// <param name="WindowRatio">The read's likelihood ratio per hop, against the window gate.</param>
    /// <param name="FromEstimator">True when the estimator's unit set the speed, false when the grid searched.</param>
    /// <param name="EstimatorUnitMs">The estimator's dit on the same window, or nought when it was not ready.</param>
    /// <param name="EstimatorMarks">How many marks the estimator counted.</param>
    /// <param name="StructureHeld">Whether the sender's own gap lengths were handed to the path.</param>
    /// <param name="HeldGaps">The gap lengths held, element, character and word, in milliseconds.</param>
    /// <param name="MixHz">The pitch the stream mixed this hop at.</param>
    /// <param name="TrackerHz">The tracker's pitch at this hop.</param>
    /// <param name="Measured">Whether the tracker's survey had measured a pitch.</param>
    /// <param name="Named">How many characters the read's path kept after judging.</param>
    internal sealed record Read(
        long Hop,
        double HeldWpm,
        double WindowRatio,
        bool FromEstimator,
        double EstimatorUnitMs,
        int EstimatorMarks,
        bool StructureHeld,
        string HeldGaps,
        double MixHz,
        double TrackerHz,
        bool Measured,
        int Named)
    {
        /// <summary>Seconds on the audio clock.</summary>
        public double Seconds => Hop * CwProbabilisticDecoder.HopMilliseconds / 1000.0;

        /// <summary>The dit the held speed implies.</summary>
        public double HeldUnitMs => HeldWpm > 0 ? 1200.0 / HeldWpm : double.NaN;

        /// <summary>Where the speed came from.</summary>
        public string SpeedFrom => FromEstimator ? "estimator" : "grid";
    }

    /// <summary>One settled character and the read that settled it.</summary>
    /// <param name="Character">What settled.</param>
    /// <param name="By">The read that settled it.</param>
    /// <param name="MixHz">The median pitch the stream mixed at across the character's own span.</param>
    /// <param name="TrackerHz">The median tracker pitch across the same span.</param>
    internal sealed record Heard(CwCharacter Character, Read By, double MixHz, double TrackerHz)
    {
        /// <summary>The dits and dahs in it.</summary>
        public int Elements => Character.Pattern.Count(e => e is '.' or '-');
    }

    /// <summary>A whole run: every settled character and every read.</summary>
    internal sealed record Traced(IReadOnlyList<Heard> Settled, IReadOnlyList<Read> Reads, CwDecodeReport Report)
    {
        /// <summary>The settled text, word gaps as spaces.</summary>
        public string Text(double fromSeconds = double.NegativeInfinity, double toSeconds = double.PositiveInfinity)
            => string.Concat(Settled
                .Where(h => h.Character.At.TotalSeconds >= fromSeconds && h.Character.At.TotalSeconds < toSeconds)
                .Select(h => h.Character.Text)).Trim();
    }

    /// <summary>Drive the decoder over audio a hop at a time and keep what it held.</summary>
    /// <param name="samples">The audio.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <returns>The run.</returns>
    internal static Traced Trace(float[] samples, int sampleRate)
    {
        var decoder = new CwDecoder(sampleRate, 600);
        var stream = decoder.Stream;
        var hop = decoder.Tracker.HopSamples;
        var mix = new List<double>();
        var tracked = new List<double>();
        var reads = new List<Read>();
        var settled = new List<(CwCharacter Character, Read By)>();
        Read? current = null;

        Read Snapshot()
        {
            var hopsSeen = (long)HopsSeenField.GetValue(stream)!;

            if (current is not null && current.Hop == hopsSeen)
            {
                return current;
            }

            var envelope = (double[])EnvelopeField.GetValue(stream)!;
            var window = envelope.Take(stream.EnvelopeHops).ToArray();
            var measured = CwUnitEstimator.Measure(window, CwProbabilisticDecoder.HopMilliseconds);
            var reading = decoder.Reading;
            var held = (bool)StructureField.GetValue(stream)!;
            var gaps = (CwUnitEstimator.CwGapLengths)HeldGapsField.GetValue(stream)!;

            current = new Read(
                hopsSeen,
                reading.WordsPerMinute,
                reading.LikelihoodRatio,
                measured.IsReady
                    && measured.WordsPerMinute >= CwProbabilisticDecoder.SlowestWpm
                    && measured.WordsPerMinute <= CwProbabilisticDecoder.FastestWpm,
                measured.UnitMilliseconds,
                measured.Marks,
                held,
                held
                    ? string.Create(Invariant, $"{gaps.ElementMilliseconds:0}/{gaps.CharacterMilliseconds:0}/{gaps.WordMilliseconds:0}")
                    : "-",
                stream.ToneHz,
                decoder.Tracker.ToneHz,
                decoder.Tracker.HasMeasuredPitch,
                reading.Characters.Count(c => c.Pattern.Length > 0));

            reads.Add(current);

            return current;
        }

        decoder.CharacterSettled += c => settled.Add((c, Snapshot()));

        for (var at = 0L; at + hop <= samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, sampleRate, samples.AsSpan((int)at, hop)));

            mix.Add(stream.ToneHz);
            tracked.Add(decoder.Tracker.ToneHz);

            // A read happened on this hop when the answer is fresh and the window has refilled.
            if (stream.HopsSinceAnswer == 0
                && stream.EnvelopeHops * CwProbabilisticDecoder.HopMilliseconds >= CwProbabilisticStream.RefillSeconds * 1000.0)
            {
                Snapshot();
            }
        }

        decoder.Flush();

        var heard = settled
            .Select(s =>
            {
                var endHop = (int)Math.Round(s.Character.At.TotalMilliseconds / CwProbabilisticDecoder.HopMilliseconds);
                var startHop = Math.Max(0, endHop - Math.Max(1, s.Character.SpanHops));
                var span = Enumerable.Range(startHop, Math.Max(1, endHop - startHop))
                    .Where(i => i < mix.Count)
                    .ToList();

                return new Heard(
                    s.Character,
                    s.By,
                    span.Count == 0 ? double.NaN : Median(span.Select(i => mix[i])),
                    span.Count == 0 ? double.NaN : Median(span.Select(i => tracked[i])));
            })
            .ToList();

        return new Traced(heard, reads, decoder.Report);
    }

    /// <summary>One recording's part of a spliced stream.</summary>
    /// <param name="Name">The recording.</param>
    /// <param name="StreamStart">Where its first kept sample sits in the spliced stream.</param>
    /// <param name="Skip">How many of its leading samples were dropped as already in the stream.</param>
    /// <param name="Kept">How many samples it contributed.</param>
    /// <param name="Note">How the join before it was found.</param>
    internal sealed record Piece(string Name, long StreamStart, int Skip, int Kept, string Note);

    /// <summary>
    /// The session as one stream: the recordings in the order kept, each overlap
    /// removed.
    /// </summary>
    /// <remarks>
    /// <para>**WHERE EACH RECORDING SITS IS READ FROM ITS SIDECAR'S `audioSeen`**,
    /// the live decoder's own sample count when the file was kept; the file is
    /// the last 30 s before it. Two consecutive recordings whose spans overlap on
    /// that clock are joined where the later one's opening samples are found,
    /// sample for sample, in the earlier one, searched a second either side of
    /// where `audioSeen` puts them (author's, overrulable). Where they do not
    /// overlap the audio between them was never kept, and the join is a jump.</para>
    /// </remarks>
    internal static (float[] Samples, int SampleRate, IReadOnlyList<Piece> Pieces) Splice(IReadOnlyList<string> names)
    {
        var joined = new List<float>();
        var pieces = new List<Piece>();
        var rate = 0;
        float[]? previous = null;
        long previousStart = 0;

        foreach (var name in names)
        {
            var audio = WavAudio.Read(Path.Combine(Folder, name + ".wav"));
            rate = audio.SampleRate;
            var start = AudioSeen(name) - audio.Samples.Length;
            var skip = 0;
            string note;

            if (previous is null)
            {
                note = "first";
            }
            else
            {
                var expected = start - previousStart;
                var gap = expected - previous.Length;

                if (gap >= 0)
                {
                    note = string.Create(Invariant, $"no overlap on audioSeen: {gap / (double)rate:0.00} s never kept before it, a jump");
                }
                else
                {
                    var lag = FindLag(previous, audio.Samples, expected, rate);

                    if (lag is { } found)
                    {
                        skip = previous.Length - found;
                        var same = 0;

                        for (var k = 0; k < skip; k++)
                        {
                            if (previous[found + k] == audio.Samples[k])
                            {
                                same++;
                            }
                        }

                        note = string.Create(
                            Invariant,
                            $"overlap found at {found / (double)rate:0.000} s into the one before, audioSeen said {expected / (double)rate:0.000} s; {skip / (double)rate:0.000} s dropped, {same} of {skip} samples identical");
                    }
                    else
                    {
                        skip = (int)Math.Min(audio.Samples.Length, -gap);
                        note = string.Create(
                            Invariant,
                            $"audioSeen says {-gap / (double)rate:0.000} s overlap but no sample match was found within a second; {skip / (double)rate:0.000} s dropped on audioSeen alone");
                    }
                }
            }

            pieces.Add(new Piece(name, joined.Count, skip, audio.Samples.Length - skip, note));
            joined.AddRange(audio.Samples.Skip(skip));
            previous = audio.Samples;
            previousStart = start;
        }

        return (joined.ToArray(), rate, pieces);
    }

    /// <summary>Where the later recording's opening samples sit in the earlier one, or null.</summary>
    private static int? FindLag(float[] earlier, float[] later, long expected, int rate)
    {
        const int probe = 4800;
        var from = (int)Math.Max(0, expected - rate);
        var to = (int)Math.Min(earlier.Length - probe, expected + rate);

        // Nearest to what audioSeen says first, outward.
        foreach (var lag in Enumerable.Range(from, Math.Max(0, to - from + 1)).OrderBy(l => Math.Abs(l - expected)))
        {
            var k = 0;

            while (k < probe && earlier[lag + k] == later[k])
            {
                k++;
            }

            if (k == probe)
            {
                return lag;
            }
        }

        return null;
    }

    /// <summary>The sidecar's text, and what it added over the previous capture's.</summary>
    private static (string Text, string Added) LiveText(string name)
    {
        var text = Sidecar(name).GetValueOrDefault("text", "");
        var index = Array.IndexOf(Session, name);

        if (index <= 0)
        {
            return (text, text);
        }

        var before = Sidecar(Session[index - 1]).GetValueOrDefault("text", "");

        return (text, text.StartsWith(before, StringComparison.Ordinal)
            ? text[before.Length..]
            : "(the previous capture's text is not a prefix of this one's)");
    }

    /// <summary>
    /// 7.3, task 1: whether the bench reproduces the opening at all, cold per
    /// recording and as one spliced stream.
    /// </summary>
    [Fact]
    public void TheOpeningOnTheBenchAndLive()
    {
        _output.WriteLine("CLOCK | every recording of the session, from its sidecar");
        _output.WriteLine("clock | recording | captured | audioSeen samples | s since previous on audioSeen | s since previous on captured | overlap on audioSeen s | toneHz | decoderWpm");

        string? last = null;

        foreach (var name in Session)
        {
            var side = Sidecar(name);
            var seen = AudioSeen(name);
            var captured = DateTime.ParseExact(side["captured"][..19], "yyyy-MM-dd HH:mm:ss", Invariant);
            var sinceSeen = last is null ? double.NaN : (seen - AudioSeen(last)) / 48000.0;
            var sinceWall = last is null
                ? double.NaN
                : (captured - DateTime.ParseExact(Sidecar(last)["captured"][..19], "yyyy-MM-dd HH:mm:ss", Invariant)).TotalSeconds;

            _output.WriteLine(string.Create(
                Invariant,
                $"clock | {name} | {side["captured"]} | {seen} | {sinceSeen:0.00} | {sinceWall:0} | {(double.IsNaN(sinceSeen) ? double.NaN : 30 - sinceSeen):0.00} | {side["toneHz"].Split(' ')[0]} | {side["decoderWpm"]}"));
            last = name;
        }

        _output.WriteLine("");
        _output.WriteLine("COLD | each recording decoded alone from a cold start, as the captures type does");

        foreach (var name in Session)
        {
            var audio = WavAudio.Read(Path.Combine(Folder, name + ".wav"));
            var run = Trace(audio.Samples, audio.SampleRate);
            var (live, added) = LiveText(name);

            _output.WriteLine($"cold | {name} | bench | {run.Text()}");
            _output.WriteLine($"cold | {name} | sidecar added since the capture before | {added.Trim()}");

            if (name is "cw-2026-09-24-003901" or "cw-2026-09-24-003919" or "cw-2026-09-24-004108")
            {
                _output.WriteLine($"cold | {name} | sidecar text, whole | {live}");
                _output.WriteLine($"cold | {name} | bench, each character | "
                    + string.Join(" ", run.Settled.Where(h => !h.Character.IsWordGap)
                        .Select(h => string.Create(Invariant, $"{h.Character.Text}@{h.Character.At.TotalSeconds:0.0}"))));
            }
        }

        _output.WriteLine("");
        _output.WriteLine("STREAM | the session spliced into one stream and decoded once, from a cold start at its head");

        var (samples, rate, pieces) = Splice(Session);
        var spliced = Trace(samples, rate);

        foreach (var piece in pieces)
        {
            var from = piece.StreamStart / (double)rate;
            var to = (piece.StreamStart + piece.Kept) / (double)rate;

            _output.WriteLine(string.Create(Invariant, $"join | {piece.Name} | stream {from:0.00} to {to:0.00} s | {piece.Note}"));
        }

        foreach (var piece in pieces)
        {
            var from = piece.StreamStart / (double)rate;
            var to = (piece.StreamStart + piece.Kept) / (double)rate;

            _output.WriteLine(string.Create(Invariant, $"stream | {piece.Name} | {from:0.00} to {to:0.00} s | {spliced.Text(from, to)}"));
        }

        for (var block = 0.0; block < samples.Length / (double)rate; block += 30)
        {
            _output.WriteLine(string.Create(Invariant, $"stream30 | {block:0} to {block + 30:0} s | {spliced.Text(block, block + 30)}"));
        }

        _output.WriteLine(string.Create(Invariant, $"stream | whole | {spliced.Text()}"));
    }

    /// <summary>The middle value, the mean of the two middle ones for an even count.</summary>
    internal static double Median(IEnumerable<double> values)
    {
        var sorted = values.Where(v => !double.IsNaN(v)).OrderBy(v => v).ToArray();

        if (sorted.Length == 0)
        {
            return double.NaN;
        }

        var middle = sorted.Length / 2;

        return sorted.Length % 2 == 1 ? sorted[middle] : (sorted[middle - 1] + sorted[middle]) / 2;
    }
}
