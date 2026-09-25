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

    /// <summary>
    /// 7.3, task 2: for every character settled in an opening stretch and in a
    /// locked stretch of the same length, what the decoder held when it settled it.
    /// </summary>
    /// <param name="run">`stream` for the spliced session, or one recording decoded cold.</param>
    /// <param name="openingFrom">Where the opening stretch starts on the run's clock, in seconds.</param>
    /// <param name="openingTo">Where it ends.</param>
    /// <param name="lockedFrom">Where the locked stretch starts.</param>
    /// <param name="lockedTo">Where it ends.</param>
    /// <remarks>
    /// <para>**THE STRETCHES ARE THE STREAM'S, BECAUSE THE COLD DECODE DOES NOT
    /// REPRODUCE THE OPENING** (task 1). The opening is 003919's new 16.2 s, where
    /// the stream carried warm from 003901 reads `UIEH EE E E T I NIEEE E`; the
    /// locked stretch is the same 16.2 s of 004108, 13.8 s into it as the opening
    /// is into 003919, so the window holds nothing from across the jump before it
    /// (author's, overrulable).</para>
    /// <para>**AND EACH STRETCH OF THE STREAM IS DECODED A SECOND TIME, COLD, ON THE
    /// SAME AUDIO**: the recording it came from, from its own start. The audio is
    /// identical sample for sample, so whatever differs between the two readings of
    /// one stretch is something the decoder carried in, not something it heard.</para>
    /// </remarks>
    [Theory]
    [InlineData("stream", 30.0, 46.2, 90.0, 106.2)]
    [InlineData("stream", 46.2, 62.4, 90.0, 106.2)] // task 3: the next recording's opening, 004027 0 to 16.2 s, after the 36.6 s never kept
    [InlineData("stream", 316.3, 332.5, 90.0, 106.2)] // task 3, beyond the instruction's choice: 004535, the one later stretch where stream and live both read a run of E
    public void WhatTheDecoderHeldAtEachCharacter(
        string run, double openingFrom, double openingTo, double lockedFrom, double lockedTo)
    {
        Traced traced;
        IReadOnlyList<Piece> pieces;
        int rate;

        if (run == "stream")
        {
            var (samples, sampleRate, joined) = Splice(Session);
            traced = Trace(samples, sampleRate);
            pieces = joined;
            rate = sampleRate;
        }
        else
        {
            var audio = WavAudio.Read(Path.Combine(Folder, run + ".wav"));
            traced = Trace(audio.Samples, audio.SampleRate);
            pieces = new[] { new Piece(run, 0, 0, audio.Samples.Length, "cold") };
            rate = audio.SampleRate;
        }

        _output.WriteLine(string.Create(
            Invariant,
            $"check | run {run}; opening {openingFrom:0.0} to {openingTo:0.0} s, locked {lockedFrom:0.0} to {lockedTo:0.0} s; Gate {CwProbabilisticDecoder.Gate:0.00}, CharacterMargin {CwProbabilisticDecoder.CharacterMargin:0.0}, StrayElementSpan {CwProbabilisticDecoder.StrayElementSpan:0.0}, grid {CwProbabilisticDecoder.SlowestWpm:0} to {CwProbabilisticDecoder.FastestWpm:0} wpm"));

        var stretches = new List<(string Label, IReadOnlyList<Heard> Settled, IReadOnlyList<Read> Reads)>();

        foreach (var (label, from, to) in new[] { ("opening", openingFrom, openingTo), ("locked", lockedFrom, lockedTo) })
        {
            var piece = pieces.Last(p => p.StreamStart / (double)rate <= from);
            var offset = (piece.Skip - piece.StreamStart) / (double)rate;
            var survey = Sidecar(piece.Name)["toneHz"].Split(' ')[0];

            _output.WriteLine("");
            _output.WriteLine(string.Create(
                Invariant,
                $"{label.ToUpperInvariant()} | {run} {from:0.0} to {to:0.0} s, which is {piece.Name} {from + offset:0.0} to {to + offset:0.0} s; the survey's tone on that recording, live, {survey} Hz"));
            _output.WriteLine($"{label} | text | {traced.Text(from, to)}");

            var settled = InStretch(traced, from, to);
            Print(label, run, settled, piece, offset, survey);
            stretches.Add((label, settled, ReadsIn(traced, from, to)));

            if (run == "stream")
            {
                // The same audio, cold: the recording this stretch came from, from its own start.
                var audio = WavAudio.Read(Path.Combine(Folder, piece.Name + ".wav"));
                var cold = Trace(audio.Samples, audio.SampleRate);
                var coldLabel = label + ", same audio cold";
                var coldSettled = InStretch(cold, from + offset, to + offset);

                _output.WriteLine($"{coldLabel} | text | {cold.Text(from + offset, to + offset)}");
                Print(coldLabel, piece.Name, coldSettled, new Piece(piece.Name, 0, 0, audio.Samples.Length, "cold"), 0, survey);
                stretches.Add((coldLabel, coldSettled, ReadsIn(cold, from + offset, to + offset)));
            }
        }

        _output.WriteLine("");
        _output.WriteLine("SUMMARY | each figure over the characters settled in each stretch: n | min | quartile | median | quartile | max");

        var figures = new (string Name, Func<Heard, double> Of)[]
        {
            ("held speed wpm", h => h.By.HeldWpm),
            ("held unit ms", h => h.By.HeldUnitMs),
            ("estimator unit ms", h => h.By.EstimatorUnitMs > 0 ? h.By.EstimatorUnitMs : double.NaN),
            ("speed from the estimator, 1 or 0", h => h.By.FromEstimator ? 1 : 0),
            ("gap structure held, 1 or 0", h => h.By.StructureHeld ? 1 : 0),
            ("mix pitch Hz", h => h.MixHz),
            ("tracker pitch Hz", h => h.TrackerHz),
            ("window ratio per hop", h => h.By.WindowRatio),
            ("span margin per hop", h => h.Character.SpanMarginForRecord),
            ("raw span", h => h.Character.SpanLogLikelihoodRatio),
            ("elements", h => h.Elements),
            ("single element, 1 or 0", h => h.Elements == 1 ? 1 : 0),
        };

        foreach (var (name, of) in figures)
        {
            foreach (var (label, settled, _) in stretches)
            {
                _output.WriteLine($"summary | {name} | {label} | {Spread(settled.Where(h => !h.Character.IsWordGap).Select(of))}");
            }
        }

        _output.WriteLine("");
        _output.WriteLine("SUMMARY OF READS | each figure over every read made inside each stretch, twice a second");

        var readFigures = new (string Name, Func<Read, double> Of)[]
        {
            ("held speed wpm", r => r.HeldWpm),
            ("estimator unit ms", r => r.EstimatorUnitMs > 0 ? r.EstimatorUnitMs : double.NaN),
            ("speed from the estimator, 1 or 0", r => r.FromEstimator ? 1 : 0),
            ("gap structure held, 1 or 0", r => r.StructureHeld ? 1 : 0),
            ("mix pitch Hz", r => r.MixHz),
            ("tracker pitch Hz", r => r.TrackerHz),
            ("window ratio per hop", r => r.WindowRatio),
        };

        foreach (var (name, of) in readFigures)
        {
            foreach (var (label, _, reads) in stretches)
            {
                _output.WriteLine($"reads | {name} | {label} | {Spread(reads.Select(of))}");
            }
        }

        _output.WriteLine("");
        _output.WriteLine("TIMELINE | every read from the start of the run to the end of the locked stretch");
        _output.WriteLine("read | s | held wpm | from | estimator unit ms | estimator marks | structure | held gaps ms | mix Hz | tracker Hz | measured | window ratio | kept");

        foreach (var r in traced.Reads.Where(r => r.Seconds <= lockedTo))
        {
            _output.WriteLine(string.Create(
                Invariant,
                $"read | {r.Seconds:0.0} | {r.HeldWpm:0} | {r.SpeedFrom} | {r.EstimatorUnitMs:0.0} | {r.EstimatorMarks} | {(r.StructureHeld ? "held" : "-")} | {r.HeldGaps} | {r.MixHz:0.0} | {r.TrackerHz:0.0} | {(r.Measured ? "yes" : "no")} | {r.WindowRatio:0.000} | {r.Named}"));
        }
    }

    private static readonly FieldInfo FineHzField = TrackerField("_fineHz");
    private static readonly FieldInfo ReportedField = TrackerField("_reportedHz");
    private static readonly FieldInfo HeldSwitchField = TrackerField("_heldSwitchHz");
    private static readonly FieldInfo LastKeyedField = TrackerField("_lastKeyedHz");
    private static readonly FieldInfo PreviousKeyedField = TrackerField("_previousKeyedHz");
    private static readonly FieldInfo ReadingDbField = TrackerField("_readingDb");
    private static readonly FieldInfo HopsSinceSurveyField = TrackerField("_hopsSinceSurvey");
    private static readonly FieldInfo SurveyField = TrackerField("_survey");

    private static readonly FieldInfo LastMeasuredField =
        typeof(CwDecoder).GetField("_lastMeasuredToneHz", BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new InvalidOperationException("CwDecoder has no field _lastMeasuredToneHz");

    private static FieldInfo TrackerField(string name)
        => typeof(CwToneTracker).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)
           ?? throw new InvalidOperationException($"CwToneTracker has no field {name}");

    private static double TrackerConstant(string name)
        => Convert.ToDouble(
            (typeof(CwToneTracker).GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
             ?? throw new InvalidOperationException($"CwToneTracker has no constant {name}")).GetValue(null),
            Invariant);

    /// <summary>The tracker's and the decoder's pitch state at one instant, read and never written.</summary>
    private sealed record PitchState(
        double CenterHz,
        double ReportedHz,
        double HeldSwitchHz,
        double LastKeyedHz,
        double PreviousKeyedHz,
        double ReadingDb,
        bool MidCharacter,
        bool Measured,
        double LastMeasuredHz,
        double MixHz,
        int Retunes,
        int StationChanges);

    private static PitchState PitchOf(CwDecoder decoder)
    {
        var t = decoder.Tracker;
        var fine = (double[])FineHzField.GetValue(t)!;

        return new PitchState(
            fine[fine.Length / 2],
            (double)ReportedField.GetValue(t)!,
            (double)HeldSwitchField.GetValue(t)!,
            (double)LastKeyedField.GetValue(t)!,
            (double)PreviousKeyedField.GetValue(t)!,
            (double)ReadingDbField.GetValue(t)!,
            t.MidCharacter,
            t.HasMeasuredPitch,
            (double)LastMeasuredField.GetValue(decoder)!,
            decoder.Stream.ToneHz,
            t.Retunes,
            t.StationChanges);
    }

    /// <summary>
    /// Which branch of `CwToneTracker.ReadSurvey` acted on one survey, named from
    /// the state before it and the coarse verdict it read, in the order the
    /// method tests them.
    /// </summary>
    private static string RuleOf(PitchState before, PitchState after, KeyingCandidate? keyed)
    {
        var reach = TrackerConstant("FineReachHz");
        var confirm = TrackerConstant("ConfirmWithinHz");
        var rejection = TrackerConstant("FilterRejectionDb");
        var center = before.CenterHz;
        var said = new List<string>();

        if (!double.IsNaN(before.HeldSwitchHz) && !before.MidCharacter)
        {
            var backHere = keyed is { } k
                && Math.Abs(k.ToneHz - center) <= confirm
                && Math.Abs(k.ToneHz - before.HeldSwitchHz) > confirm;

            said.Add(backHere
                ? string.Create(Invariant, $"the held move to {before.HeldSwitchHz:0.0} dropped, keying back where it listens (CwToneTracker.cs:966-977)")
                : string.Create(Invariant, $"the held move to {before.HeldSwitchHz:0.0} made (CwToneTracker.cs:966-975, Switch)"));

            if (!backHere)
            {
                center = before.HeldSwitchHz;
            }
        }

        if (keyed is not { } found)
        {
            said.Add(after.CenterHz != center && double.IsNaN(before.LastKeyedHz)
                ? string.Create(Invariant, $"nothing admitted; from cold the bank pointed at the loudest thing, {after.CenterHz:0.0} (CwToneTracker.cs:1014-1025)")
                : "nothing admitted; the tracker stays (CwToneTracker.cs:988-1027)");
        }
        else if (double.IsNaN(before.PreviousKeyedHz) || Math.Abs(before.PreviousKeyedHz - found.ToneHz) > confirm)
        {
            said.Add(string.Create(Invariant, $"{found.ToneHz:0.0} admitted once, the survey before admitted {before.PreviousKeyedHz:0.0}; not confirmed, no move (CwToneTracker.cs:1044-1050, HM-DEC-095's twice)"));
        }
        else if (!double.IsNaN(before.ReadingDb)
                 && !double.IsNaN(found.KeyedDb)
                 && found.KeyedDb < before.ReadingDb - rejection
                 && Math.Abs(found.ToneHz - center) > reach)
        {
            said.Add(string.Create(Invariant, $"{found.ToneHz:0.0} confirmed but {before.ReadingDb - found.KeyedDb:0.0} dB below the station read; refused (CwToneTracker.cs:1077-1088, HM-DEC-127)"));
        }
        else if (Math.Abs(found.ToneHz - center) > reach)
        {
            said.Add(before.MidCharacter
                ? string.Create(Invariant, $"{found.ToneHz:0.0} confirmed outside the bank's reach of {center:0.0}; held until the character ends (CwToneTracker.cs:1097-1104)")
                : string.Create(Invariant, $"{found.ToneHz:0.0} confirmed outside the bank's reach of {center:0.0}; Switch (CwToneTracker.cs:1107), the survey's choice by keying (HM-DEC-095)"));
        }
        else
        {
            said.Add(string.Create(Invariant, $"{found.ToneHz:0.0} confirmed inside the bank's reach of {center:0.0}; the fine survey's reading reported, {after.ReportedHz:0.0} (CwToneTracker.cs:1114-1128)"));
        }

        return string.Join("; then ", said);
    }

    /// <summary>
    /// 7.4, work instruction 430 task 1: why the mix moved. For every survey the
    /// tracker read inside a stretch, what it admitted bin by bin, which rule acted
    /// on it, and what the decoder then mixed at.
    /// </summary>
    /// <param name="run">`stream` for the spliced session, or one recording decoded cold.</param>
    /// <param name="from">Where the stretch starts on the run's clock, in seconds.</param>
    /// <param name="to">Where it ends.</param>
    /// <remarks>
    /// <para>**A PRINTER. IT ASSERTS NOTHING, AND IT WRITES NOTHING.** Seven private
    /// fields of <see cref="CwToneTracker"/> and one of <see cref="CwDecoder"/> are
    /// read by reflection before and after every hop. The survey's own verdict is
    /// asked again after the read with `CwToneSurvey.Analyze`, over the history the
    /// tracker has just read and before anything more is observed, so it is the
    /// answer the tracker acted on; like <see cref="CwToneTracker.CoarseCandidates"/>
    /// it recomputes the band beside each bin and decides nothing.</para>
    /// <para>**THE RULE IS NAMED FROM THE STATE, IN THE ORDER `ReadSurvey` TESTS IT**,
    /// and each line carries the file and line it names.</para>
    /// </remarks>
    [Theory]
    [InlineData("stream", 28.0, 38.0)]
    [InlineData("stream", 90.0, 106.2)] // the locked stretch: 004108 13.8 to 30.0 s, warm
    [InlineData("cw-2026-09-24-004108", 13.8, 30.0)] // the same stretch cold
    [InlineData("cw-2026-09-24-003919", 11.8, 21.8)] // beyond the instruction's list: the opening's own audio cold, stream 28 to 38 s less 16.2
    [InlineData("cw-2026-08-22-032113", 0.0, 60.0)] // task 2: the one recording whose floor the change cost, the whole file
    public void WhyTheMixMoved(string run, double from, double to)
    {
        float[] samples;
        int rate;

        if (run == "stream")
        {
            (samples, rate, _) = Splice(Session);
        }
        else
        {
            var audio = WavAudio.Read(Path.Combine(Folder, run + ".wav"));
            samples = audio.Samples;
            rate = audio.SampleRate;
        }

        var decoder = new CwDecoder(rate, 600);
        var hop = decoder.Tracker.HopSamples;
        var surveyEvery = (int)TrackerConstant("SurveyEveryHops");
        var survey = (CwToneSurvey)SurveyField.GetValue(decoder.Tracker)!;

        _output.WriteLine(string.Create(
            Invariant,
            $"check | run {run}, {from:0.0} to {to:0.0} s; a survey read every {surveyEvery} hops of {hop} samples; FineReachHz {TrackerConstant("FineReachHz"):0}, ConfirmWithinHz {TrackerConstant("ConfirmWithinHz"):0}, FilterRejectionDb {TrackerConstant("FilterRejectionDb"):0}"));
        _output.WriteLine("survey | s | rule | bank centre before -> after | reported before -> after | measured | _lastMeasuredToneHz | mix written at CwDecoder.cs:617 | mid-character before | held move before | survey before admitted | reading dB before | held wpm | retunes | station changes");
        _output.WriteLine("admitted | s | Hz | dit ms | dah ms | ratio | separation | lift dB | marks | keyed dB | wpm | the survey's best");

        var lastMix = double.NaN;

        for (var at = 0L; at + hop <= samples.Length && at / (double)rate <= to; at += hop)
        {
            var seconds = (at + hop) / (double)rate;
            var before = PitchOf(decoder);

            decoder.Process(new AudioChunk(at, rate, samples.AsSpan((int)at, hop)));

            var after = PitchOf(decoder);
            var read = (int)HopsSinceSurveyField.GetValue(decoder.Tracker)! == 0;

            if (seconds < from)
            {
                lastMix = after.MixHz;
                continue;
            }

            if (read)
            {
                var verdict = survey.Analyze();
                var admitted = decoder.Tracker.CoarseCandidates();

                _output.WriteLine(string.Create(
                    Invariant,
                    $"survey | {seconds:0.00} | {RuleOf(before, after, verdict.Keyed)} | {before.CenterHz:0.0} -> {after.CenterHz:0.0} | {before.ReportedHz:0.0} -> {after.ReportedHz:0.0} | {(after.Measured ? "yes" : "no")} | {after.LastMeasuredHz:0.0} | {after.MixHz:0.0} | {(before.MidCharacter ? "yes" : "no")} | {before.HeldSwitchHz:0.0} | {before.PreviousKeyedHz:0.0} | {before.ReadingDb:0.0} | {decoder.Reading.WordsPerMinute:0} | {after.Retunes} | {after.StationChanges}"));

                foreach (var c in admitted)
                {
                    var best = verdict.Keyed is { } k && k.ToneHz == c.ToneHz;

                    _output.WriteLine(string.Create(
                        Invariant,
                        $"admitted | {seconds:0.00} | {c.ToneHz:0.0} | {c.DitMilliseconds:0.0} | {c.DahMilliseconds:0.0} | {c.Ratio:0.00} | {c.Separation:0.00} | {c.LiftDb:0.0} | {c.Marks} | {c.KeyedDb:0.0} | {c.WordsPerMinute:0.0} | {(best ? "best" : "-")}"));
                }

                if (admitted.Count == 0)
                {
                    _output.WriteLine(string.Create(Invariant, $"admitted | {seconds:0.00} | none"));
                }

                if (verdict.Strongest is { } loud)
                {
                    _output.WriteLine(string.Create(
                        Invariant,
                        $"strongest | {seconds:0.00} | {loud.ToneHz:0.0} Hz, lift {loud.LiftDb:0.0} dB, present {loud.PresentFraction:0.00}; interference {(verdict.Interference is { } i ? string.Create(Invariant, $"{i.ToneHz:0.0} Hz, lift {i.LiftDb:0.0} dB") : "none")}"));
                }
            }

            if (after.MixHz != lastMix)
            {
                _output.WriteLine(string.Create(
                    Invariant,
                    $"mix moved | {seconds:0.00} | {lastMix:0.0} -> {after.MixHz:0.0} | on a survey read {(read ? "yes" : "no")} | tracker {decoder.Tracker.ToneHz:0.0}, measured {(after.Measured ? "yes" : "no")}, _lastMeasuredToneHz {after.LastMeasuredHz:0.0}"));
            }

            lastMix = after.MixHz;
        }
    }

    /// <summary>
    /// 7.4, work instruction 432 task 1: when each of four release rules would have
    /// moved the mix, replayed over the tracker's own record.
    /// </summary>
    /// <param name="run">`stream` for the spliced session, or one recording decoded cold.</param>
    /// <param name="from">Where the stretch starts on the run's clock, in seconds.</param>
    /// <param name="to">Where it ends.</param>
    /// <remarks>
    /// <para>**A PRINTER. IT ASSERTS NOTHING, AND IT WRITES NOTHING.** The decoder is
    /// driven a hop at a time as <see cref="WhyTheMixMoved"/> drives it, and after
    /// every hop the tracker's pitch, whether it is measured, its `Verdict` and
    /// whether that hop read the survey are kept. The four rules are then run over
    /// that one record: the entry's (follow at once), `5b6b704c`'s (wait for a later
    /// read confirming keying there), `ec76051e`'s (follow at once if the read just
    /// before confirmed it) and P39's (follow at once if any read since the mix's
    /// pitch last changed confirmed keying within 25 Hz of the new pitch, and not
    /// counting the read that made the move). Each rule tells a new read from the
    /// last by its verdict, as `5b6b704c` did.</para>
    /// <para>**A REPLAY, NOT A DECODE.** The tracker record is the entry decoder's;
    /// under another rule the mix, and so the decoder's `InsideCharacter` and the
    /// tracker's hold, could differ after the first difference.</para>
    /// </remarks>
    [Theory]
    [InlineData("stream", 28.0, 38.0)]
    [InlineData("cw-2026-08-22-032113", 0.0, 1000.0)] // whole
    [InlineData("cw-2026-08-22-031905", 0.0, 1000.0)] // whole: it gained under 5b6b704c
    public void WhenEachRuleFollows(string run, double from, double to)
    {
        const double same = 25;
        float[] samples;
        int rate;

        if (run == "stream")
        {
            (samples, rate, _) = Splice(Session);
        }
        else
        {
            var audio = WavAudio.Read(Path.Combine(Folder, run + ".wav"));
            samples = audio.Samples;
            rate = audio.SampleRate;
        }

        var decoder = new CwDecoder(rate, 600);
        var hop = decoder.Tracker.HopSamples;
        var hops = new List<TrackerHop>();

        for (var at = 0L; at + hop <= samples.Length && at / (double)rate <= to; at += hop)
        {
            decoder.Process(new AudioChunk(at, rate, samples.AsSpan((int)at, hop)));

            var t = decoder.Tracker;

            hops.Add(new TrackerHop(
                (at + hop) / (double)rate,
                (int)HopsSinceSurveyField.GetValue(t)! == 0,
                t.HasMeasuredPitch,
                t.ToneHz,
                t.Verdict,
                decoder.Stream.ToneHz));
        }

        var rules = new ReleaseRule[] { new EntryRule(), new LaterReadRule(), new ReadBeforeRule(), new AnyReadSinceSetRule() };
        var mixes = rules.Select(_ => new double[hops.Count]).ToArray();
        var setAt = new double[hops.Count];

        for (var i = 0; i < hops.Count; i++)
        {
            setAt[i] = ((AnyReadSinceSetRule)rules[3]).SetAt;

            for (var r = 0; r < rules.Length; r++)
            {
                mixes[r][i] = rules[r].Step(hops[i]);
            }
        }

        var reads = hops.Count(h => h.Read);
        var blind = hops.Where(h => h.Read).Zip(hops.Where(h => h.Read).Skip(1), (a, b) => a.Verdict.Equals(b.Verdict)).Count(x => x);
        var entryAgrees = hops.Count(h => h.Measured && Math.Abs(h.StreamHz - h.TrackerHz) > 0.001);

        _output.WriteLine(string.Create(
            Invariant,
            $"replay check | run {run}, {from:0.0} to {to:0.0} s, {hops.Count} hops of {hop} samples, {reads} survey reads; reads whose verdict equals the read before's, which a rule telling reads by verdict would miss: {blind}; measured hops where the entry decoder's mix differs from the entry rule's: {entryAgrees}"));
        _output.WriteLine("replay move | s | tracker from -> to | a Switch on the read that confirmed it | P39's mix last set at s");
        _output.WriteLine("replay read | s | the survey's Verdict: keyed Hz or none | confirmed keying within 25 Hz of the target | note");
        _output.WriteLine("replay follows | s of the move | entry | 5b6b704c | ec76051e | P39");

        var moves = new List<int>();

        for (var i = 1; i < hops.Count; i++)
        {
            if (hops[i].Measured && hops[i - 1].Measured && Math.Abs(hops[i].TrackerHz - hops[i - 1].TrackerHz) > same)
            {
                moves.Add(i);
            }
        }

        for (var m = 0; m < moves.Count; m++)
        {
            var i = moves[m];
            var move = hops[i];
            var target = move.TrackerHz;

            if (move.Seconds < from || move.Seconds > to)
            {
                continue;
            }

            var confirmsOwn = move.Read && move.Verdict.Keyed is { } own && Math.Abs(own.ToneHz - target) <= same;

            _output.WriteLine(string.Create(
                Invariant,
                $"replay move | {move.Seconds:0.00} | {hops[i - 1].TrackerHz:0.0} -> {target:0.0} | {(confirmsOwn ? "yes" : "no")} | {setAt[i]:0.00}"));

            for (var j = 0; j <= i; j++)
            {
                if (!hops[j].Read || hops[j].Seconds < setAt[i])
                {
                    continue;
                }

                var keyed = hops[j].Verdict.Keyed;
                var confirms = keyed is { } k && Math.Abs(k.ToneHz - target) <= same;

                _output.WriteLine(string.Create(
                    Invariant,
                    $"replay read | {hops[j].Seconds:0.00} | {(keyed is { } kk ? kk.ToneHz.ToString("0.0", Invariant) : "none")} | {(confirms ? "yes" : "no")} | {(j == i ? "the read that made the move" : "-")}"));
            }

            var end = m + 1 < moves.Count ? moves[m + 1] : hops.Count;
            var follows = mixes.Select(mix =>
            {
                for (var j = i; j < end; j++)
                {
                    if (!double.IsNaN(mix[j]) && Math.Abs(mix[j] - target) <= same)
                    {
                        return hops[j].Seconds.ToString("0.00", Invariant);
                    }
                }

                return end < hops.Count
                    ? string.Create(Invariant, $"not before the next move at {hops[end].Seconds:0.00}")
                    : "never";
            });

            _output.WriteLine(string.Create(Invariant, $"replay follows | {move.Seconds:0.00} | {string.Join(" | ", follows)}"));
        }

        // Where P39's mix and 5b6b704c's differ at all, stretch by stretch.
        _output.WriteLine("replay differs | from s | to s | 5b6b704c's mix | P39's mix");

        for (var i = 0; i < hops.Count; i++)
        {
            if (Differ(mixes[1][i], mixes[3][i]) && (i == 0 || !Differ(mixes[1][i - 1], mixes[3][i - 1])))
            {
                var j = i;

                while (j + 1 < hops.Count && Differ(mixes[1][j + 1], mixes[3][j + 1]))
                {
                    j++;
                }

                _output.WriteLine(string.Create(
                    Invariant,
                    $"replay differs | {hops[i].Seconds:0.00} | {hops[j].Seconds:0.00} | {mixes[1][i]:0.0} | {mixes[3][i]:0.0}"));
            }
        }

        static bool Differ(double a, double b) => !(double.IsNaN(a) && double.IsNaN(b)) && !(Math.Abs(a - b) <= 0.001);
    }

    /// <summary>What the tracker held after one hop, read and never written.</summary>
    private sealed record TrackerHop(double Seconds, bool Read, bool Measured, double TrackerHz, ToneVerdict Verdict, double StreamHz);

    /// <summary>One way of deciding when the mixdown follows the tracker; answers the mix's measured pitch, or NaN.</summary>
    private abstract class ReleaseRule
    {
        protected const double Same = 25;

        protected double Last = double.NaN;

        public abstract double Step(TrackerHop hop);
    }

    /// <summary>The entry: follow every measured pitch at once.</summary>
    private sealed class EntryRule : ReleaseRule
    {
        public override double Step(TrackerHop hop)
        {
            if (hop.Measured)
            {
                Last = hop.TrackerHz;
            }

            return Last;
        }
    }

    /// <summary>`5b6b704c`: a move further than 25 Hz waits for a later read confirming keying there.</summary>
    private sealed class LaterReadRule : ReleaseRule
    {
        private double _pending = double.NaN;
        private ToneVerdict _atMove;

        public override double Step(TrackerHop hop)
        {
            if (hop.Measured)
            {
                var heard = hop.TrackerHz;

                if (double.IsNaN(Last) || Math.Abs(heard - Last) <= Same)
                {
                    Last = heard;
                    _pending = double.NaN;
                }
                else if (double.IsNaN(_pending) || Math.Abs(heard - _pending) > Same)
                {
                    _pending = heard;
                    _atMove = hop.Verdict;
                }
                else if (hop.Verdict != _atMove
                         && hop.Verdict.Keyed is { } again
                         && Math.Abs(again.ToneHz - heard) <= Same)
                {
                    Last = heard;
                    _pending = double.NaN;
                }
            }

            return Last;
        }
    }

    /// <summary>`ec76051e`: as `5b6b704c`, but a move the read just before confirmed is followed at once.</summary>
    private sealed class ReadBeforeRule : ReleaseRule
    {
        private double _pending = double.NaN;
        private ToneVerdict _atMove;
        private ToneVerdict _now;
        private ToneVerdict _before;

        public override double Step(TrackerHop hop)
        {
            if (!hop.Verdict.Equals(_now))
            {
                _before = _now;
                _now = hop.Verdict;
            }

            if (hop.Measured)
            {
                var heard = hop.TrackerHz;

                if (double.IsNaN(Last) || Math.Abs(heard - Last) <= Same)
                {
                    Last = heard;
                    _pending = double.NaN;
                }
                else if (double.IsNaN(_pending) || Math.Abs(heard - _pending) > Same)
                {
                    if (_before.Keyed is { } earlier && Math.Abs(earlier.ToneHz - heard) <= Same)
                    {
                        Last = heard;
                        _pending = double.NaN;
                    }
                    else
                    {
                        _pending = heard;
                        _atMove = _now;
                    }
                }
                else if (!_now.Equals(_atMove)
                         && _now.Keyed is { } again
                         && Math.Abs(again.ToneHz - heard) <= Same)
                {
                    Last = heard;
                    _pending = double.NaN;
                }
            }

            return Last;
        }
    }

    /// <summary>
    /// P39: a move further than 25 Hz is followed at once if any read since the mix's
    /// pitch last changed, other than the read that made the move, confirmed keying
    /// within 25 Hz of it; otherwise it waits for a later read that does.
    /// </summary>
    private sealed class AnyReadSinceSetRule : ReleaseRule
    {
        private readonly List<double> _confirmed = new();
        private double _pending = double.NaN;
        private ToneVerdict _seen;
        private double _now;

        /// <summary>When the mix's pitch last changed, in seconds.</summary>
        public double SetAt { get; private set; }

        public override double Step(TrackerHop hop)
        {
            _now = hop.Seconds;

            var read = !hop.Verdict.Equals(_seen);

            _seen = hop.Verdict;

            if (hop.Measured)
            {
                var heard = hop.TrackerHz;

                if (double.IsNaN(Last) || Math.Abs(heard - Last) <= Same)
                {
                    Set(heard);
                }
                else if (double.IsNaN(_pending) || Math.Abs(heard - _pending) > Same)
                {
                    if (_confirmed.Any(c => Math.Abs(c - heard) <= Same))
                    {
                        Set(heard);
                    }
                    else
                    {
                        _pending = heard;
                    }
                }
                else if (read && hop.Verdict.Keyed is { } again && Math.Abs(again.ToneHz - heard) <= Same)
                {
                    Set(heard);
                }
            }

            if (read && hop.Verdict.Keyed is { } found)
            {
                _confirmed.Add(found.ToneHz);
            }

            return Last;
        }

        private void Set(double heard)
        {
            if (!(Math.Abs(heard - Last) <= 0.001))
            {
                _confirmed.Clear();
                SetAt = _now;
            }

            Last = heard;
            _pending = double.NaN;
        }
    }

    /// <summary>
    /// 7.4, work instruction 433 task 1: when the mix would follow a move if it asked
    /// whether the sender keys harder at the new pitch than at the mix, replayed over
    /// the tracker's own record beside the entry.
    /// </summary>
    /// <param name="run">`stream` for the spliced session, or one recording decoded cold.</param>
    /// <param name="from">Where the stretch starts on the run's clock, in seconds.</param>
    /// <param name="to">Where it ends.</param>
    /// <remarks>
    /// <para>**A PRINTER. IT ASSERTS NOTHING, AND IT WRITES NOTHING.** The decoder is
    /// driven a hop at a time as <see cref="WhenEachRuleFollows"/> drives it, and the
    /// same record is kept. Two rules are run over it: the entry's, and section 6's.
    /// Under section 6's, a move of more than 25 Hz from the mix is followed only when
    /// the keying contrast of <see cref="CwProbabilisticDecoder.Envelope(IReadOnlyList{float}, int, double)"/>
    /// over the last 3.0 s, the 90th percentile of its per-hop magnitudes over the
    /// 10th in dB, is greater at the target than at the mix. It is compared again on
    /// every survey read while the move is pending, and a move of more than 25 Hz from
    /// the pending pitch starts a new comparison. Under 3.0 s heard, it follows as at
    /// entry.</para>
    /// <para>**A REPLAY, NOT A DECODE**, with <see cref="WhenEachRuleFollows"/>'s
    /// caveat: the tracker record is the entry decoder's.</para>
    /// </remarks>
    [Theory]
    [InlineData("stream", 28.0, 38.0)]
    [InlineData("cw-2026-08-22-032113", 0.0, 1000.0)] // whole
    [InlineData("cw-2026-08-22-031905", 0.0, 1000.0)] // whole
    public void WhereTheSenderKeysHarder(string run, double from, double to)
    {
        const double same = 25;
        float[] samples;
        int rate;

        if (run == "stream")
        {
            (samples, rate, _) = Splice(Session);
        }
        else
        {
            var audio = WavAudio.Read(Path.Combine(Folder, run + ".wav"));
            samples = audio.Samples;
            rate = audio.SampleRate;
        }

        var decoder = new CwDecoder(rate, 600);
        var hop = decoder.Tracker.HopSamples;
        var survey = (CwToneSurvey)SurveyField.GetValue(decoder.Tracker)!;
        var hops = new List<TrackerHop>();
        var best = new Dictionary<double, int>();

        for (var at = 0L; at + hop <= samples.Length && at / (double)rate <= to; at += hop)
        {
            decoder.Process(new AudioChunk(at, rate, samples.AsSpan((int)at, hop)));

            var t = decoder.Tracker;
            var seconds = (at + hop) / (double)rate;
            var read = (int)HopsSinceSurveyField.GetValue(t)! == 0;

            hops.Add(new TrackerHop(seconds, read, t.HasMeasuredPitch, t.ToneHz, t.Verdict, decoder.Stream.ToneHz));

            // The survey's best, as WhyTheMixMoved marks it: CwToneSurvey.Analyze
            // over the history the tracker has just read (P47).
            if (read && seconds >= from && survey.Analyze().Keyed is { } keyed)
            {
                best[keyed.ToneHz] = best.GetValueOrDefault(keyed.ToneHz) + 1;
            }
        }

        double Contrast(double seconds, double toneHz)
        {
            var length = (int)Math.Round(KeysHarderRule.Seconds * rate);
            var end = (int)Math.Min(samples.Length, Math.Round(seconds * rate));

            if (end < length)
            {
                return double.NaN;
            }

            var envelope = CwProbabilisticDecoder.Envelope(new ArraySegment<float>(samples, end - length, length), rate, toneHz);
            var sorted = envelope.OrderBy(v => v).ToArray();

            double At(double q) => sorted[(int)Math.Round(q * (sorted.Length - 1))];

            var low = At(0.10);

            return low > 0 ? 20 * Math.Log10(At(0.90) / low) : double.NaN;
        }

        var entry = new EntryRule();
        var harder = new KeysHarderRule(Contrast);
        var entryMix = new double[hops.Count];
        var harderMix = new double[hops.Count];

        for (var i = 0; i < hops.Count; i++)
        {
            entryMix[i] = entry.Step(hops[i]);
            harderMix[i] = harder.Step(hops[i]);
        }

        var entryAgrees = hops.Count(h => h.Measured && Math.Abs(h.StreamHz - h.TrackerHz) > 0.001);
        var sender = string.Join(", ", best.OrderByDescending(b => b.Value).ThenBy(b => b.Key).Take(4)
            .Select(b => string.Create(Invariant, $"{b.Key:0.0} Hz on {b.Value} reads")));

        _output.WriteLine(string.Create(
            Invariant,
            $"contrast check | run {run}, {from:0.0} to {to:0.0} s, {hops.Count} hops of {hop} samples, {hops.Count(h => h.Read)} survey reads; Envelope at {CwProbabilisticDecoder.IntegratorBandwidthHz:0} Hz over the last {KeysHarderRule.Seconds:0.0} s, 90th over 10th percentile in dB; measured hops where the entry decoder's mix differs from the entry rule's: {entryAgrees}"));
        _output.WriteLine(string.Create(
            Invariant,
            $"contrast sender | {run} | the survey's best as WhyTheMixMoved marks it, most reads first: {(sender.Length == 0 ? "none" : sender)}"));
        _output.WriteLine("contrast move | s | mix -> target Hz | tracker before | beside the rule, deciding nothing: the contrast in dB at the survey's best pitches");
        _output.WriteLine("contrast read | s | target Hz | target dB | mix Hz | mix dB | greater | note");
        _output.WriteLine("contrast follows | s of the move | target Hz | entry | this rule");

        for (var m = 0; m < harder.Moves.Count; m++)
        {
            var (i, mixHz, target) = harder.Moves[m];

            if (hops[i].Seconds < from || hops[i].Seconds > to)
            {
                continue;
            }

            var aside = string.Join(", ", best.Keys.OrderBy(k => k)
                .Select(k => string.Create(Invariant, $"{k:0.0} Hz {Contrast(hops[i].Seconds, k):0.00}")));

            _output.WriteLine(string.Create(
                Invariant,
                $"contrast move | {hops[i].Seconds:0.00} | {mixHz:0.0} -> {target:0.0} | {(i > 0 ? hops[i - 1].TrackerHz : double.NaN):0.0} | {aside}"));

            foreach (var c in harder.Compared.Where(c => c.Move == m))
            {
                var greater = double.IsNaN(c.TargetDb) || double.IsNaN(c.MixDb)
                    ? "cannot compute; follow as at entry"
                    : c.TargetDb > c.MixDb ? "target" : "mix";

                _output.WriteLine(string.Create(
                    Invariant,
                    $"contrast read | {c.Seconds:0.00} | {c.TargetHz:0.0} | {c.TargetDb:0.00} | {c.MixHz:0.0} | {c.MixDb:0.00} | {greater} | {(Math.Abs(c.Seconds - hops[i].Seconds) < 1e-9 ? "at the move" : "a survey read while pending")}"));
            }

            // Until the rule's next move or the tracker's, whichever comes first.
            var end = m + 1 < harder.Moves.Count ? harder.Moves[m + 1].Hop : hops.Count;

            for (var j = i + 1; j < end; j++)
            {
                if (hops[j].Measured && hops[j - 1].Measured && Math.Abs(hops[j].TrackerHz - hops[j - 1].TrackerHz) > same)
                {
                    end = j;
                    break;
                }
            }

            string Follows(double[] mix)
            {
                for (var j = i; j < end; j++)
                {
                    if (!double.IsNaN(mix[j]) && Math.Abs(mix[j] - target) <= same)
                    {
                        return hops[j].Seconds.ToString("0.00", Invariant);
                    }
                }

                return end < hops.Count
                    ? string.Create(Invariant, $"not before the tracker's next move at {hops[end].Seconds:0.00}")
                    : "never";
            }

            _output.WriteLine(string.Create(Invariant, $"contrast follows | {hops[i].Seconds:0.00} | {target:0.0} | {Follows(entryMix)} | {Follows(harderMix)}"));
        }

        // Where this rule's mix and the entry's differ at all, stretch by stretch.
        _output.WriteLine("contrast differs | from s | to s | entry's mix | this rule's mix");

        for (var i = 0; i < hops.Count; i++)
        {
            if (Differ(entryMix[i], harderMix[i]) && (i == 0 || !Differ(entryMix[i - 1], harderMix[i - 1])))
            {
                var j = i;

                while (j + 1 < hops.Count && Differ(entryMix[j + 1], harderMix[j + 1]))
                {
                    j++;
                }

                _output.WriteLine(string.Create(
                    Invariant,
                    $"contrast differs | {hops[i].Seconds:0.00} | {hops[j].Seconds:0.00} | {entryMix[i]:0.0} | {harderMix[i]:0.0}"));
            }
        }

        static bool Differ(double a, double b) => !(double.IsNaN(a) && double.IsNaN(b)) && !(Math.Abs(a - b) <= 0.001);
    }

    /// <summary>
    /// Work instruction 433's rule: a move further than 25 Hz from the mix is followed
    /// only when the envelope keys harder at the target than at the mix, compared at
    /// the move and on every survey read while it is pending.
    /// </summary>
    private sealed class KeysHarderRule : ReleaseRule
    {
        /// <summary>The survey's own history length, `CwToneSurvey`'s default.</summary>
        public const double Seconds = 3.0;

        private readonly Func<double, double, double> _contrast;
        private double _pending = double.NaN;
        private int _hop = -1;

        public KeysHarderRule(Func<double, double, double> contrast)
            => _contrast = contrast;

        /// <summary>Each move the rule saw: its hop, the mix then, and the target.</summary>
        public List<(int Hop, double MixHz, double TargetHz)> Moves { get; } = new();

        /// <summary>Each comparison, by the index of its move in <see cref="Moves"/>.</summary>
        public List<(int Move, double Seconds, double TargetHz, double TargetDb, double MixHz, double MixDb)> Compared { get; } = new();

        public override double Step(TrackerHop hop)
        {
            _hop++;

            if (hop.Measured)
            {
                var heard = hop.TrackerHz;

                if (double.IsNaN(Last) || Math.Abs(heard - Last) <= Same)
                {
                    Last = heard;
                    _pending = double.NaN;
                }
                else if (double.IsNaN(_pending) || Math.Abs(heard - _pending) > Same)
                {
                    _pending = heard;
                    Moves.Add((_hop, Last, heard));
                    Compare(hop, heard);
                }
                else if (hop.Read)
                {
                    Compare(hop, heard);
                }
            }

            return Last;
        }

        private void Compare(TrackerHop hop, double heard)
        {
            var target = _contrast(hop.Seconds, heard);
            var mix = _contrast(hop.Seconds, Last);

            Compared.Add((Moves.Count - 1, hop.Seconds, heard, target, Last, mix));

            if (double.IsNaN(target) || double.IsNaN(mix) || target > mix)
            {
                Last = heard;
                _pending = double.NaN;
            }
        }
    }

    private static readonly MethodInfo TwoMeansMethod = EstimatorMethod("TwoMeansOnLogs");
    private static readonly MethodInfo ShortMedianMethod = EstimatorMethod("ShortClusterMedian");

    private static MethodInfo EstimatorMethod(string name)
        => typeof(CwUnitEstimator).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)
           ?? throw new InvalidOperationException($"CwUnitEstimator has no method {name}");

    /// <summary>Work instruction 436's ratio: a long mark centroid further than this above the short one fires the rule.</summary>
    private const double DahOverDitBar = 4.5;

    /// <summary>What one read's window said about the dit and the dah, and what the rule would make of it.</summary>
    /// <param name="Seconds">Seconds on the audio clock.</param>
    /// <param name="Marks">How many marks the trigger produced.</param>
    /// <param name="ShortCentroidMs">The short mark centroid, NaN when there were too few marks or gaps.</param>
    /// <param name="LongCentroidMs">The long mark centroid.</param>
    /// <param name="ShortMedianMs">The short mark cluster's median, the entry's dit mark.</param>
    /// <param name="LongMedianMs">The long mark cluster's median.</param>
    /// <param name="ShortGapMs">The short gap cluster's median.</param>
    /// <param name="EntryUnitMs">What <see cref="CwUnitEstimator.Measure"/> returns at entry, nought when not ready.</param>
    /// <param name="HeldWpm">The speed the read decoded at.</param>
    /// <param name="Fires">Whether the long centroid stands more than 4.5 times the short one.</param>
    /// <param name="RuleUnitMs">The rule's unit: the long median over 3 when it fires, the entry's otherwise.</param>
    /// <param name="Mark">Every mark in the window, milliseconds.</param>
    /// <param name="Gap">Every gap in the window, milliseconds.</param>
    internal sealed record DitAgainstDah(
        double Seconds,
        int Marks,
        double ShortCentroidMs,
        double LongCentroidMs,
        double ShortMedianMs,
        double LongMedianMs,
        double ShortGapMs,
        double EntryUnitMs,
        double HeldWpm,
        bool Fires,
        double RuleUnitMs,
        IReadOnlyList<double> Mark,
        IReadOnlyList<double> Gap)
    {
        /// <summary>The long centroid over the short.</summary>
        public double Ratio => LongCentroidMs / ShortCentroidMs;

        /// <summary>The speed the entry's unit implies.</summary>
        public double EntryWpm => EntryUnitMs > 0 ? 1200.0 / EntryUnitMs : 0;

        /// <summary>The speed the rule's unit implies.</summary>
        public double RuleWpm => RuleUnitMs > 0 ? 1200.0 / RuleUnitMs : 0;

        /// <summary>Whether the stream takes the entry's speed rather than searching the grid.</summary>
        public bool EntryTaken => InRange(EntryWpm);

        /// <summary>Whether the stream would take the rule's speed.</summary>
        public bool RuleTaken => InRange(RuleWpm);

        private static bool InRange(double wpm)
            => wpm > 0 && wpm >= CwProbabilisticDecoder.SlowestWpm && wpm <= CwProbabilisticDecoder.FastestWpm;
    }

    /// <summary>
    /// Drive the decoder a hop at a time and, on every read, take the window
    /// apart the way <see cref="CwUnitEstimator.Measure"/> does.
    /// </summary>
    /// <remarks>
    /// The two private cluster functions are called by reflection and nothing is
    /// written; the long cluster's median is taken above the same boundary
    /// `ShortClusterMedian` cuts at, member `n / 2` as it takes its own.
    /// </remarks>
    private static IReadOnlyList<DitAgainstDah> DitsAgainstDahs(float[] samples, int sampleRate, double toSeconds = double.PositiveInfinity)
    {
        var decoder = new CwDecoder(sampleRate, 600);
        var stream = decoder.Stream;
        var hop = decoder.Tracker.HopSamples;
        var rows = new List<DitAgainstDah>();

        for (var at = 0L; at + hop <= samples.Length && at / (double)sampleRate <= toSeconds; at += hop)
        {
            decoder.Process(new AudioChunk(at, sampleRate, samples.AsSpan((int)at, hop)));

            if (stream.HopsSinceAnswer != 0
                || stream.EnvelopeHops * CwProbabilisticDecoder.HopMilliseconds < CwProbabilisticStream.RefillSeconds * 1000.0)
            {
                continue;
            }

            var seconds = (long)HopsSeenField.GetValue(stream)! * CwProbabilisticDecoder.HopMilliseconds / 1000.0;
            var envelope = (double[])EnvelopeField.GetValue(stream)!;
            var window = envelope.Take(stream.EnvelopeHops).ToArray();
            var entry = CwUnitEstimator.Measure(window, CwProbabilisticDecoder.HopMilliseconds);
            var (marks, gaps) = CwUnitEstimator.Elements(window, CwProbabilisticDecoder.HopMilliseconds);
            var held = decoder.Reading.WordsPerMinute;

            if (marks.Count < 8 || gaps.Count < 8)
            {
                rows.Add(new DitAgainstDah(seconds, marks.Count, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN,
                    entry.UnitMilliseconds, held, false, entry.UnitMilliseconds, marks, gaps));
                continue;
            }

            var (low, high) = ((double, double))TwoMeansMethod.Invoke(null, new object[] { marks })!;
            var shortMedian = (double)ShortMedianMethod.Invoke(null, new object[] { marks })!;
            var shortGap = (double)ShortMedianMethod.Invoke(null, new object[] { gaps })!;
            var boundary = Math.Sqrt(low * high);
            var longMembers = marks.Where(v => v > boundary).OrderBy(v => v).ToArray();
            var longMedian = high <= low || longMembers.Length == 0 ? high : longMembers[longMembers.Length / 2];
            var fires = entry.IsReady && high > DahOverDitBar * low;

            rows.Add(new DitAgainstDah(seconds, marks.Count, low, high, shortMedian, longMedian, shortGap,
                entry.UnitMilliseconds, held, fires, fires ? longMedian / 3 : entry.UnitMilliseconds, marks, gaps));
        }

        return rows;
    }

    /// <summary>
    /// 7.4, work instruction 436 task 1: every read's short and long mark clusters
    /// through the opening, through `003919` cold and through the locked stretch,
    /// and the speed the dit-against-dah rule would give each.
    /// </summary>
    /// <remarks>
    /// <para>**A PRINTER. IT ASSERTS NOTHING AND WRITES NOTHING.** The rule is
    /// the instruction's, fixed before this ran: when the long mark centroid
    /// stands more than 4.5 times the short one, the unit is the long cluster's
    /// median over 3.</para>
    /// <para>The locked stretch is the stream's 90.0 to 106.2 s, which is
    /// `004108` 13.8 to 30.0 s, read warm as 7.3 read it.</para>
    /// </remarks>
    /// <param name="run">`stream` for the spliced session, or a recording cold.</param>
    [Theory]
    [InlineData("stream")]
    [InlineData("cw-2026-09-24-003919")]
    public void WhereTheDitMeetsTheDah(string run)
    {
        var stretches = run == "stream"
            ? new[] { ("opening", 27.0, 46.2), ("locked, 004108 13.8 to 30.0 s", 90.0, 106.2) }
            : new[] { ("cold", 0.0, 1000.0) };
        float[] samples;
        int rate;

        if (run == "stream")
        {
            (samples, rate, _) = Splice(Session);
        }
        else
        {
            var audio = WavAudio.Read(Path.Combine(Folder, run + ".wav"));
            samples = audio.Samples;
            rate = audio.SampleRate;
        }

        var rows = DitsAgainstDahs(samples, rate, stretches.Max(s => s.Item3));

        _output.WriteLine(string.Create(
            Invariant,
            $"dah check | run {run}; {rows.Count} reads; the rule fires above {DahOverDitBar:0.0} and divides the long median by 3; grid {CwProbabilisticDecoder.SlowestWpm:0} to {CwProbabilisticDecoder.FastestWpm:0} wpm"));

        foreach (var (label, from, to) in stretches)
        {
            var inside = rows.Where(r => r.Seconds >= from && r.Seconds < to).ToList();

            _output.WriteLine("");
            _output.WriteLine(string.Create(Invariant, $"DAH {label.ToUpperInvariant()} | {run} {from:0.0} to {to:0.0} s | {inside.Count} reads"));
            _output.WriteLine($"dah {label} | s | marks | short centroid ms | long centroid ms | ratio | short median ms | long median ms | short gap ms | entry unit ms | entry wpm | entry from | held wpm | fires | rule unit ms | rule wpm | rule from");

            foreach (var r in inside)
            {
                _output.WriteLine(string.Create(
                    Invariant,
                    $"dah {label} | {r.Seconds:0.0} | {r.Marks} | {r.ShortCentroidMs:0.0} | {r.LongCentroidMs:0.0} | {r.Ratio:0.00} | {r.ShortMedianMs:0.0} | {r.LongMedianMs:0.0} | {r.ShortGapMs:0.0} | {r.EntryUnitMs:0.0} | {r.EntryWpm:0.0} | {(r.EntryTaken ? "stream" : "grid")} | {r.HeldWpm:0} | {(r.Fires ? "yes" : "no")} | {r.RuleUnitMs:0.0} | {r.RuleWpm:0.0} | {(r.RuleTaken ? "stream" : "grid")}"));
            }

            _output.WriteLine(string.Create(
                Invariant,
                $"dah {label} summary | reads {inside.Count} | entry to the grid {inside.Count(r => !r.EntryTaken)} | fires {inside.Count(r => r.Fires)} | fires with the rule's speed inside the grid {inside.Count(r => r.Fires && r.RuleTaken)} | grid reads the rule hands to the stream {inside.Count(r => !r.EntryTaken && r.RuleTaken)} | stream reads the rule hands to the grid {inside.Count(r => r.EntryTaken && !r.RuleTaken)}"));

            // What the halved readings' clusters actually held: every read the grid decided.
            foreach (var r in inside.Where(r => !r.EntryTaken))
            {
                _output.WriteLine(string.Create(
                    Invariant,
                    $"dah {label} held | {r.Seconds:0.0} | marks {string.Join(" ", r.Mark.OrderBy(v => v).Select(v => v.ToString("0", Invariant)))} | gaps {string.Join(" ", r.Gap.OrderBy(v => v).Select(v => v.ToString("0", Invariant)))}"));
            }
        }
    }

    /// <summary>
    /// 7.4, work instruction 436 task 1: over every capture row and every keyed
    /// recording, how many reads the dit-against-dah rule fires on.
    /// </summary>
    /// <remarks>
    /// **A FORECAST FOR THE FOUR TESTS, NOT A GATE, AND IT ASSERTS NOTHING.** Each
    /// recording is driven cold from 600 Hz, as the captures type drives it.
    /// </remarks>
    [Fact]
    public void WhereTheDahRuleFires()
    {
        var names = TheCapturesThatDecodeKeepDecodingTests.Floors
            .Select(row => (string)row[0])
            .Concat(WhatTheStrayLettersRestOnTests.KeyedRecordings.Select(k => k.Name))
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var touched = new List<string>();

        _output.WriteLine($"fires | recording | reads | entry to the grid | fires | fires inside the grid | grid reads handed to the stream | stream reads handed to the grid | stream reads whose speed moves | {names.Count} recordings");

        foreach (var name in names)
        {
            var audio = WavAudio.Read(Path.Combine(CapturedSignalTests.Folder, name + ".wav"));
            var rows = DitsAgainstDahs(audio.Samples, audio.SampleRate);
            var fires = rows.Count(r => r.Fires);

            if (fires > 0)
            {
                touched.Add(string.Create(Invariant, $"{name} {fires}"));
            }

            _output.WriteLine(string.Create(
                Invariant,
                $"fires | {name} | {rows.Count} | {rows.Count(r => !r.EntryTaken)} | {fires} | {rows.Count(r => r.Fires && r.RuleTaken)} | {rows.Count(r => !r.EntryTaken && r.RuleTaken)} | {rows.Count(r => r.EntryTaken && !r.RuleTaken)} | {rows.Count(r => r.Fires && r.EntryTaken && r.RuleTaken)}"));
        }

        _output.WriteLine($"fires | touched | {touched.Count} of {names.Count} | {string.Join(", ", touched)}");
    }

    private static readonly FieldInfo MixedIField = Private("_mixedI");
    private static readonly FieldInfo MixedQField = Private("_mixedQ");
    private static readonly FieldInfo MixWriteField = Private("_mixWrite");
    private static readonly FieldInfo MixFilledField = Private("_mixFilled");
    private static readonly FieldInfo PhaseField = Private("_phase");
    private static readonly FieldInfo EnvelopeCountField = Private("_envelopeCount");
    private static readonly FieldInfo TaperField = Private("_taper");
    private static readonly FieldInfo TaperWeightField = Private("_taperWeight");
    private static readonly FieldInfo WindowSamplesField = Private("_windowSamples");
    private static readonly FieldInfo HopSamplesField = Private("_hopSamples");

    /// <summary>Work instruction 437's move: a mix this far from the pitch the held window was last mixed at re-mixes it.</summary>
    private const double RemixFromHz = 25;

    /// <summary>
    /// Re-mix a stream's held window at a new pitch, the way work instruction 437
    /// would build it: the mixed arms and every envelope hop recomputed from the
    /// raw audio behind them, with the stream's own taper and integrator.
    /// </summary>
    /// <param name="stream">A shadow stream this test owns; the decoder's own is never touched.</param>
    /// <param name="samples">The raw audio the stream has been fed.</param>
    /// <param name="end">How many samples it has been fed.</param>
    /// <param name="toneHz">The new pitch.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <remarks>
    /// The arms are mixed with the phase running backwards from the stream's own,
    /// so the next sample it mixes continues them without a step.
    /// </remarks>
    private static void Remix(CwProbabilisticStream stream, float[] samples, long end, double toneHz, int sampleRate)
    {
        var width = (int)WindowSamplesField.GetValue(stream)!;
        var hopSamples = (int)HopSamplesField.GetValue(stream)!;
        var mixedI = (float[])MixedIField.GetValue(stream)!;
        var mixedQ = (float[])MixedQField.GetValue(stream)!;
        var mixWrite = (int)MixWriteField.GetValue(stream)!;
        var mixFilled = (int)MixFilledField.GetValue(stream)!;
        var phase = (double)PhaseField.GetValue(stream)!;
        var envelope = (double[])EnvelopeField.GetValue(stream)!;
        var count = (int)EnvelopeCountField.GetValue(stream)!;
        var hopsSeen = (long)HopsSeenField.GetValue(stream)!;
        var taper = (double[])TaperField.GetValue(stream)!;
        var weight = (double)TaperWeightField.GetValue(stream)!;
        var step = 2 * Math.PI * toneHz / sampleRate;

        var firstHop = hopsSeen - count;
        var from = Math.Max(0, Math.Min(((firstHop + 1) * hopSamples) - width, end - mixFilled));
        var armI = new float[end - from];
        var armQ = new float[end - from];

        for (var k = 0; k < armI.Length; k++)
        {
            var back = end - (from + k);
            var at = phase - (back * step);

            armI[k] = (float)(samples[from + k] * Math.Cos(at));
            armQ[k] = (float)(samples[from + k] * -Math.Sin(at));
        }

        for (var j = 0; j < count; j++)
        {
            var hopEnd = (firstHop + j + 1) * hopSamples;
            var filled = (int)Math.Min(width, hopEnd);
            var start = hopEnd - filled - from;
            var taperFrom = width - filled;
            double i = 0;
            double q = 0;

            for (var m = 0; m < filled; m++)
            {
                i += armI[start + m] * taper[taperFrom + m];
                q += armQ[start + m] * taper[taperFrom + m];
            }

            envelope[j] = Math.Sqrt((i * i) + (q * q)) / weight;
        }

        for (var back = 1; back <= mixFilled; back++)
        {
            var ring = ((mixWrite - back) % width + width) % width;

            mixedI[ring] = armI[end - back - from];
            mixedQ[ring] = armQ[end - back - from];
        }
    }

    /// <summary>One read of the opening, at entry and with the held window re-mixed.</summary>
    /// <param name="Seconds">Seconds on the audio clock.</param>
    /// <param name="MixHz">The pitch the stream mixed at.</param>
    /// <param name="FiredFromHz">The pitch the held window was re-mixed from since the read before, or NaN.</param>
    /// <param name="OffPitchSeconds">How much of the entry's window was mixed 25 Hz or more off the current pitch.</param>
    /// <param name="Entry">The entry's read.</param>
    /// <param name="Remixed">The same read with the window re-mixed.</param>
    internal sealed record RemixRead(
        double Seconds, double MixHz, double FiredFromHz, double OffPitchSeconds, ShadowRead Entry, ShadowRead Remixed);

    /// <summary>What one shadow stream's read measured and settled.</summary>
    /// <param name="UnitMs">The estimator's unit on the window, nought when not ready.</param>
    /// <param name="Wpm">The speed the read decoded at.</param>
    /// <param name="FromEstimator">Whether the estimator set the speed rather than the grid.</param>
    /// <param name="Settled">The characters the read settled.</param>
    internal sealed record ShadowRead(double UnitMs, double Wpm, bool FromEstimator, string Settled)
    {
        /// <summary>Where the speed came from.</summary>
        public string SpeedFrom => FromEstimator ? "estimator" : "grid";
    }

    /// <summary>A replay: the decoder as it runs, and two shadow streams fed its pitch hop for hop.</summary>
    /// <param name="Reads">Every read, both ways.</param>
    /// <param name="Entry">Every character the unchanged shadow settled.</param>
    /// <param name="Remixed">Every character the re-mixing shadow settled.</param>
    /// <param name="Decoder">Every character the decoder itself settled, to check the unchanged shadow against.</param>
    /// <param name="Fires">Every re-mix: seconds, from Hz, to Hz.</param>
    /// <param name="Shadow">The re-mixing shadow, left as the run ended.</param>
    /// <param name="End">How many samples were fed.</param>
    internal sealed record Replayed(
        IReadOnlyList<RemixRead> Reads,
        IReadOnlyList<CwCharacter> Entry,
        IReadOnlyList<CwCharacter> Remixed,
        IReadOnlyList<CwCharacter> Decoder,
        IReadOnlyList<(double Seconds, double FromHz, double ToHz)> Fires,
        CwProbabilisticStream Shadow,
        long End);

    /// <summary>
    /// Drive the decoder a hop at a time and feed two shadow streams the same
    /// audio at the pitch the decoder mixed each hop at; the second has its held
    /// window re-mixed whenever that pitch stands 25 Hz or more from the one it
    /// was last mixed at.
    /// </summary>
    /// <remarks>
    /// **THE PITCH IS THE DECODER'S, SO THE TRACKER NEVER SEES THE RE-MIX.** The
    /// interlock hands the tracker the decoder's own reads, and in this replay
    /// those are the entry's; a built change could move the tracker differently
    /// through it, which only the build shows.
    /// </remarks>
    private static Replayed Replay(float[] samples, int sampleRate, double toSeconds = double.PositiveInfinity)
    {
        var decoder = new CwDecoder(sampleRate, 600);
        var hop = decoder.Tracker.HopSamples;
        var entry = new CwProbabilisticStream(sampleRate);
        var remixed = new CwProbabilisticStream(sampleRate);
        var entrySettled = new List<CwCharacter>();
        var remixSettled = new List<CwCharacter>();
        var decoderSettled = new List<CwCharacter>();
        var entryRead = new List<CwCharacter>();
        var remixRead = new List<CwCharacter>();
        var mix = new List<double>();
        var fires = new List<(double, double, double)>();
        var reads = new List<RemixRead>();
        var lastMixedAt = double.NaN;
        var firedFrom = double.NaN;
        var at = 0L;

        entry.CharacterSettled += c => { entrySettled.Add(c); entryRead.Add(c); };
        remixed.CharacterSettled += c => { remixSettled.Add(c); remixRead.Add(c); };
        decoder.CharacterSettled += decoderSettled.Add;

        ShadowRead Measured(CwProbabilisticStream stream, List<CwCharacter> settled)
        {
            var window = ((double[])EnvelopeField.GetValue(stream)!).Take(stream.EnvelopeHops).ToArray();
            var measured = CwUnitEstimator.Measure(window, CwProbabilisticDecoder.HopMilliseconds);
            var read = new ShadowRead(
                measured.UnitMilliseconds,
                stream.Last.WordsPerMinute,
                measured.IsReady
                    && measured.WordsPerMinute >= CwProbabilisticDecoder.SlowestWpm
                    && measured.WordsPerMinute <= CwProbabilisticDecoder.FastestWpm,
                string.Concat(settled.Select(c => c.Text)));

            settled.Clear();

            return read;
        }

        for (; at + hop <= samples.Length && at / (double)sampleRate <= toSeconds; at += hop)
        {
            var chunk = samples.AsSpan((int)at, hop);

            decoder.Process(new AudioChunk(at, sampleRate, chunk));

            var tone = decoder.Stream.ToneHz;

            mix.Add(tone);
            entry.ToneHz = tone;
            remixed.ToneHz = tone;

            if (double.IsNaN(lastMixedAt))
            {
                lastMixedAt = tone;
            }
            else if (Math.Abs(tone - lastMixedAt) >= RemixFromHz)
            {
                Remix(remixed, samples, at, tone, sampleRate);
                fires.Add((at / (double)sampleRate, lastMixedAt, tone));
                firedFrom = double.IsNaN(firedFrom) ? lastMixedAt : firedFrom;
                lastMixedAt = tone;
            }

            entry.Process(chunk);
            remixed.Process(chunk);

            if (remixed.HopsSinceAnswer != 0
                || remixed.EnvelopeHops * CwProbabilisticDecoder.HopMilliseconds < CwProbabilisticStream.RefillSeconds * 1000.0)
            {
                continue;
            }

            var hopsSeen = (long)HopsSeenField.GetValue(remixed)!;
            var held = remixed.EnvelopeHops;
            var off = Enumerable.Range((int)(hopsSeen - held), held)
                .Count(h => h < mix.Count && Math.Abs(mix[h] - tone) >= RemixFromHz);

            reads.Add(new RemixRead(
                hopsSeen * CwProbabilisticDecoder.HopMilliseconds / 1000.0,
                tone,
                firedFrom,
                off * CwProbabilisticDecoder.HopMilliseconds / 1000.0,
                Measured(entry, entryRead),
                Measured(remixed, remixRead)));
            firedFrom = double.NaN;
        }

        if (double.IsPositiveInfinity(toSeconds))
        {
            decoder.Flush();
            entry.Flush();
            remixed.Flush();
        }

        return new Replayed(reads, entrySettled, remixSettled, decoderSettled, fires, remixed, at);
    }

    /// <summary>Settled text between two moments, word gaps as spaces.</summary>
    private static string TextOf(IEnumerable<CwCharacter> settled, double from, double to)
        => string.Concat(settled
            .Where(c => c.At.TotalSeconds >= from && c.At.TotalSeconds < to)
            .Select(c => c.Text)).Trim();

    /// <summary>How many characters between two moments were named: not a word gap, not unreadable.</summary>
    private static int NamedIn(IEnumerable<CwCharacter> settled, double from, double to)
        => settled.Count(c => c.At.TotalSeconds >= from && c.At.TotalSeconds < to
            && !c.IsWordGap && c.Text != MorseAlphabet.Unreadable);

    /// <summary>
    /// 7.4, work instruction 437 task 1: every read of the opening, at entry and
    /// with the held window re-mixed at the new pitch whenever the mix moves 25 Hz
    /// or more.
    /// </summary>
    /// <remarks>
    /// <para>**A PRINTER. IT ASSERTS NOTHING AND WRITES NOTHING.** The change is
    /// the instruction's, fixed before this ran. Two streams this test owns are
    /// fed the decoder's audio at the pitch the decoder mixed each hop at; the
    /// first is left alone and must read exactly as the decoder does, the second
    /// has its window re-mixed. Nothing under `src` changes.</para>
    /// <para>The stream runs to 50 s so every character before 46.2 s has passed
    /// the decision delay; the recordings cold run whole and are flushed.</para>
    /// </remarks>
    /// <param name="run">`stream` for the spliced session, or a recording cold.</param>
    [Theory]
    [InlineData("stream")]
    [InlineData("cw-2026-09-24-003901")]
    [InlineData("cw-2026-09-24-003919")]
    public void WhenTheWindowIsRemixed(string run)
    {
        float[] samples;
        int rate;
        double from;
        double to;
        double through;

        if (run == "stream")
        {
            (samples, rate, _) = Splice(Session);
            (from, to, through) = (27.0, 46.2, 50.0);
        }
        else
        {
            var audio = WavAudio.Read(Path.Combine(Folder, run + ".wav"));
            samples = audio.Samples;
            rate = audio.SampleRate;
            (from, to, through) = (0.0, 60.0, double.PositiveInfinity);
        }

        var replay = Replay(samples, rate, through);

        _output.WriteLine(string.Create(
            Invariant,
            $"remix check | run {run}; {replay.Reads.Count} reads; a re-mix fires when the mix stands {RemixFromHz:0} Hz or more from the pitch the held window was last mixed at; {replay.Fires.Count} fired"));
        _output.WriteLine($"remix check | the unchanged shadow against the decoder, whole run | {(TextOf(replay.Entry, 0, 1e9) == TextOf(replay.Decoder, 0, 1e9) ? "identical" : "DIFFERENT")}");

        foreach (var (seconds, fromHz, toHz) in replay.Fires)
        {
            _output.WriteLine(string.Create(Invariant, $"remix fired | {seconds:0.000} s | {fromHz:0.0} to {toHz:0.0} Hz"));
        }

        var stretches = run == "stream"
            ? new[] { ("opening", 30.0, 46.2), ("before", 27.0, 30.0) }
            : new[] { ("cold", from, to) };

        foreach (var (label, a, b) in stretches)
        {
            _output.WriteLine(string.Create(Invariant, $"remix text | {label} {a:0.0} to {b:0.0} s | entry | {NamedIn(replay.Entry, a, b)} named | {TextOf(replay.Entry, a, b)}"));
            _output.WriteLine(string.Create(Invariant, $"remix text | {label} {a:0.0} to {b:0.0} s | remixed | {NamedIn(replay.Remixed, a, b)} named | {TextOf(replay.Remixed, a, b)}"));
        }

        _output.WriteLine("");
        _output.WriteLine("remix read | s | mix Hz | fired | off-pitch s at entry | entry unit ms | entry wpm | entry from | remix unit ms | remix wpm | remix from | entry settles | remix settles | same");

        var inside = replay.Reads.Where(r => r.Seconds >= from && r.Seconds < to + 1.0).ToList();

        foreach (var r in inside)
        {
            _output.WriteLine(string.Create(
                Invariant,
                $"remix read | {r.Seconds:0.0} | {r.MixHz:0.0} | {(double.IsNaN(r.FiredFromHz) ? "-" : $"from {r.FiredFromHz:0.0}")} | {r.OffPitchSeconds:0.00} | {r.Entry.UnitMs:0.0} | {r.Entry.Wpm:0.0} | {r.Entry.SpeedFrom} | {r.Remixed.UnitMs:0.0} | {r.Remixed.Wpm:0.0} | {r.Remixed.SpeedFrom} | `{r.Entry.Settled}` | `{r.Remixed.Settled}` | {(r.Entry.Settled == r.Remixed.Settled ? "same" : "DIFFERS")}"));
        }

        var judged = replay.Reads.Where(r => r.Seconds >= (run == "stream" ? 30.0 : from) && r.Seconds < to).ToList();

        _output.WriteLine(string.Create(
            Invariant,
            $"remix stop | {run} | reads {judged.Count} | reads that settle differently {judged.Count(r => r.Entry.Settled != r.Remixed.Settled)} | reads whose speed differs {judged.Count(r => Math.Abs(r.Entry.Wpm - r.Remixed.Wpm) > 0.05)}"));

        if (run != "stream")
        {
            return;
        }

        // **THE COST OF ONE RE-MIX OF A FULL WINDOW, ON THIS MACHINE.** The shadow
        // is re-mixed at the pitch it already stands at, so each pass rewrites the
        // same values; the first pass is left out as the warm-up.
        var timings = new List<double>();
        var clock = new System.Diagnostics.Stopwatch();

        for (var pass = 0; pass < 21; pass++)
        {
            clock.Restart();
            Remix(replay.Shadow, samples, replay.End, replay.Shadow.ToneHz, rate);
            clock.Stop();

            if (pass > 0)
            {
                timings.Add(clock.Elapsed.TotalMilliseconds);
            }
        }

        _output.WriteLine(string.Create(
            Invariant,
            $"remix cost | one full window, {replay.Shadow.EnvelopeHops} hops | ms over 20 passes | {Spread(timings)} | the stream's hop budget is {CwProbabilisticDecoder.HopMilliseconds:0.0} ms"));
    }

    /// <summary>
    /// 7.4, work instruction 437 task 1: over every capture row and every keyed
    /// recording, how many mix moves of 25 Hz or more occur.
    /// </summary>
    /// <remarks>
    /// **A FORECAST FOR THE FOUR TESTS, NOT A GATE, AND IT ASSERTS NOTHING.** Each
    /// recording is driven cold from 600 Hz, as the captures type drives it, and a
    /// move is counted exactly where the re-mix would fire.
    /// </remarks>
    [Fact]
    public void WhereTheMixMovesFar()
    {
        var names = TheCapturesThatDecodeKeepDecodingTests.Floors
            .Select(row => (string)row[0])
            .Concat(WhatTheStrayLettersRestOnTests.KeyedRecordings.Select(k => k.Name))
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var touched = new List<string>();

        _output.WriteLine($"moves | recording | moves of {RemixFromHz:0} Hz or more | at s, from Hz to Hz | {names.Count} recordings");

        foreach (var name in names)
        {
            var audio = WavAudio.Read(Path.Combine(CapturedSignalTests.Folder, name + ".wav"));
            var decoder = new CwDecoder(audio.SampleRate, 600);
            var hop = decoder.Tracker.HopSamples;
            var lastMixedAt = double.NaN;
            var moves = new List<string>();

            for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
            {
                decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));

                var tone = decoder.Stream.ToneHz;

                if (double.IsNaN(lastMixedAt))
                {
                    lastMixedAt = tone;
                }
                else if (Math.Abs(tone - lastMixedAt) >= RemixFromHz)
                {
                    moves.Add(string.Create(Invariant, $"{at / (double)audio.SampleRate:0.00} {lastMixedAt:0} to {tone:0}"));
                    lastMixedAt = tone;
                }
            }

            if (moves.Count > 0)
            {
                touched.Add(string.Create(Invariant, $"{name} {moves.Count}"));
            }

            _output.WriteLine($"moves | {name} | {moves.Count} | {string.Join(", ", moves)}");
        }

        _output.WriteLine($"moves | touched | {touched.Count} of {names.Count} | {string.Join(", ", touched)}");
    }

    private static readonly MethodInfo OtsuMethod = EstimatorMethod("Otsu");

    private static readonly MethodInfo SpacedMethod =
        typeof(CwProbabilisticStream).GetMethod("Spaced", BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new InvalidOperationException("CwProbabilisticStream has no method Spaced");

    private static readonly FieldInfo SettledThroughField = Private("_settledThrough");
    private static readonly FieldInfo TroughRunField = Private("_troughRun");
    private static readonly FieldInfo TroughMissesField = Private("_troughMisses");
    private static readonly FieldInfo DelayHopsField = Private("_delayHops");

    private static readonly int ShortestRun = (int)(typeof(CwUnitEstimator)
        .GetField("ShortestRunHops", BindingFlags.NonPublic | BindingFlags.Static)
        ?.GetRawConstantValue()
        ?? throw new InvalidOperationException("CwUnitEstimator has no ShortestRunHops"));

    /// <summary>Work instruction 438's block: one cut per half second, the stream's read cadence.</summary>
    private const double CutBlockSeconds = 0.5;

    /// <summary>Work instruction 438's span: each block's cut is taken over the 3 s around it.</summary>
    private const double CutSpanSeconds = 3.0;

    /// <summary>
    /// Otsu's level over an envelope in decibels, and the mean of each class it
    /// splits, the way `CwUnitEstimator.Otsu` bins it.
    /// </summary>
    /// <remarks>
    /// The level is `Otsu`'s own to the bin, and the check below prints any read
    /// where it is not. The class means are the bin centres at the class's mean
    /// bin, taken from the same histogram.
    /// </remarks>
    private static (double Cut, double MeanBelow, double MeanAbove) OtsuSplit(double[] db)
    {
        var low = db.Min();
        var high = db.Max();

        if (high - low < 1e-6)
        {
            return (low, low, low);
        }

        const int bins = 256;
        var counts = new int[bins];
        var width = (high - low) / bins;

        foreach (var value in db)
        {
            counts[Math.Clamp((int)((value - low) / width), 0, bins - 1)]++;
        }

        double total = db.Length;
        var sum = 0.0;

        for (var b = 0; b < bins; b++)
        {
            sum += b * (double)counts[b];
        }

        var below = 0.0;
        var weightBelow = 0.0;
        var best = -1.0;
        var bestBin = 0;
        var bestBelow = 0.0;
        var bestAbove = 0.0;

        for (var b = 0; b < bins; b++)
        {
            weightBelow += counts[b];

            if (weightBelow == 0)
            {
                continue;
            }

            var weightAbove = total - weightBelow;

            if (weightAbove == 0)
            {
                break;
            }

            below += b * (double)counts[b];

            var meanBelow = below / weightBelow;
            var meanAbove = (sum - below) / weightAbove;
            var between = weightBelow * weightAbove * (meanBelow - meanAbove) * (meanBelow - meanAbove);

            if (between > best)
            {
                best = between;
                bestBin = b;
                bestBelow = meanBelow;
                bestAbove = meanAbove;
            }
        }

        return (low + ((bestBin + 0.5) * width), low + ((bestBelow + 0.5) * width), low + ((bestAbove + 0.5) * width));
    }

    /// <summary>The envelope in decibels, exactly as `CwUnitEstimator.Elements` takes it.</summary>
    private static double[] Decibels(IReadOnlyList<double> envelope)
    {
        var db = new double[envelope.Count];

        for (var i = 0; i < envelope.Count; i++)
        {
            db[i] = 20 * Math.Log10(Math.Max(envelope[i], 1e-12));
        }

        return db;
    }

    /// <summary>Work instruction 438's cut for every hop, and what each block did.</summary>
    /// <param name="Hop">The cut each hop is triggered at, in decibels.</param>
    /// <param name="Whole">The whole window's cut, the entry's.</param>
    /// <param name="Lowest">The lowest block cut.</param>
    /// <param name="Highest">The highest block cut.</param>
    /// <param name="Blocks">How many blocks there were.</param>
    /// <param name="FellBack">How many blocks' spans held no keying and took the whole window's cut.</param>
    /// <param name="OtsuDiffers">How many spans' levels differed from `CwUnitEstimator.Otsu`'s, which should be none.</param>
    internal sealed record LocalCut(double[] Hop, double Whole, double Lowest, double Highest, int Blocks, int FellBack, int OtsuDiffers);

    /// <summary>
    /// The cut work instruction 438 fixes: per 0.5 s block, Otsu over the 3.0 s
    /// of hops centred on it, slid inward at the window's ends; a span whose two
    /// classes' means stand less than twice the hysteresis depth apart holds no
    /// keying and takes the whole window's cut.
    /// </summary>
    private static LocalCut LocalCuts(double[] db, double spanSeconds, double hysteresisDb = CwUnitEstimator.HysteresisDb)
    {
        var blockHops = (int)Math.Round(CutBlockSeconds * 1000.0 / CwProbabilisticDecoder.HopMilliseconds);
        var spanHops = (int)Math.Round(spanSeconds * 1000.0 / CwProbabilisticDecoder.HopMilliseconds);
        var whole = (double)OtsuMethod.Invoke(null, new object[] { db })!;
        var cuts = new double[db.Length];
        var blockCuts = new List<double>();
        var fellBack = 0;
        var differs = 0;

        for (var start = 0; start < db.Length; start += blockHops)
        {
            var end = Math.Min(db.Length, start + blockHops);
            var from = db.Length <= spanHops
                ? 0
                : Math.Clamp(((start + end) / 2) - (spanHops / 2), 0, db.Length - spanHops);
            var span = db[from..Math.Min(db.Length, from + spanHops)];
            var split = OtsuSplit(span);

            if (Math.Abs(split.Cut - (double)OtsuMethod.Invoke(null, new object[] { span })!) > 1e-9)
            {
                differs++;
            }

            var cut = split.MeanAbove - split.MeanBelow < 2 * hysteresisDb ? whole : split.Cut;

            if (split.MeanAbove - split.MeanBelow < 2 * hysteresisDb)
            {
                fellBack++;
            }

            blockCuts.Add(cut);
            Array.Fill(cuts, cut, start, end - start);
        }

        return new LocalCut(cuts, whole, blockCuts.Min(), blockCuts.Max(), blockCuts.Count, fellBack, differs);
    }

    /// <summary>One run of the two-level trigger.</summary>
    /// <param name="Start">Its first hop in the window.</param>
    /// <param name="Hops">How long it ran.</param>
    /// <param name="Mark">Key-down or key-up.</param>
    internal sealed record Run(int Start, int Hops, bool Mark)
    {
        /// <summary>One past its last hop.</summary>
        public int End => Start + Hops;

        /// <summary>Whether `CwUnitEstimator.Runs` keeps it as an element.</summary>
        public bool Kept => Hops >= ShortestRun;
    }

    /// <summary>
    /// `CwUnitEstimator.Runs` with each hop's own cut, the hysteresis state carried
    /// across blocks, keeping every run and where it stands.
    /// </summary>
    private static IReadOnlyList<Run> RunsAt(double[] db, double[] cuts, double hysteresisDb = CwUnitEstimator.HysteresisDb)
    {
        var runs = new List<Run>();
        var keyDown = db[0] > cuts[0] + hysteresisDb;
        var runStart = 0;

        for (var i = 1; i < db.Length; i++)
        {
            var changed = keyDown ? db[i] < cuts[i] - hysteresisDb : db[i] > cuts[i] + hysteresisDb;

            if (!changed)
            {
                continue;
            }

            runs.Add(new Run(runStart, i - runStart, keyDown));
            keyDown = !keyDown;
            runStart = i;
        }

        return runs;
    }

    /// <summary>What one way of cutting measured on a window.</summary>
    /// <param name="Marks">How many marks were kept.</param>
    /// <param name="ShortMarkMs">The short mark cluster's median, NaN when too few.</param>
    /// <param name="ShortGapMs">The short gap cluster's median, NaN when too few.</param>
    /// <param name="UnitMs">The unit `Measure` would return, nought when not ready.</param>
    internal sealed record Cut(int Marks, double ShortMarkMs, double ShortGapMs, double UnitMs)
    {
        /// <summary>The speed the unit implies.</summary>
        public double Wpm => UnitMs > 0 ? 1200.0 / UnitMs : 0;

        /// <summary>The stream's own speed choice: the estimator's speed inside the grid's range, else the grid.</summary>
        public double? Speed => UnitMs > 0 && Wpm >= CwProbabilisticDecoder.SlowestWpm && Wpm <= CwProbabilisticDecoder.FastestWpm
            ? Wpm
            : null;
    }

    /// <summary>`CwUnitEstimator.Measure` from here on, on runs already cut.</summary>
    private static Cut Measured(IReadOnlyList<Run> runs)
    {
        var marks = runs.Where(r => r.Kept && r.Mark).Select(r => r.Hops * CwProbabilisticDecoder.HopMilliseconds).ToList();
        var gaps = runs.Where(r => r.Kept && !r.Mark).Select(r => r.Hops * CwProbabilisticDecoder.HopMilliseconds).ToList();

        if (marks.Count < 8 || gaps.Count < 8)
        {
            return new Cut(marks.Count, double.NaN, double.NaN, 0);
        }

        var shortMark = (double)ShortMedianMethod.Invoke(null, new object[] { marks })!;
        var shortGap = (double)ShortMedianMethod.Invoke(null, new object[] { gaps })!;

        return shortMark <= 0 || shortGap <= 0
            ? new Cut(marks.Count, shortMark, shortGap, 0)
            : new Cut(marks.Count, shortMark, shortGap, (shortMark + shortGap) / 2);
    }

    /// <summary>What the stream held just before a hop, read and never written.</summary>
    private sealed record StreamBefore(long SettledThrough, int TroughRun, int TroughMisses, bool StructureHeld, CwUnitEstimator.CwGapLengths HeldGaps);

    /// <summary>
    /// What one read would settle if its unit were <paramref name="cut"/>'s: the
    /// stream's own speed choice, gap structure, decode, spacing and settling, from
    /// the state the stream held before the read.
    /// </summary>
    /// <remarks>
    /// Where the speed and the structure come out as the stream's own, its own
    /// result is used and nothing is decoded again.
    /// </remarks>
    private static string WouldSettle(
        CwProbabilisticStream stream, double[] window, StreamBefore before, Cut cut, double? entrySpeed, bool entryHeld, CwUnitEstimator.CwGapLengths entryGaps)
    {
        var hopMs = CwProbabilisticDecoder.HopMilliseconds;
        var gaps = cut.UnitMs > 0 ? CwUnitEstimator.MeasureGaps(window, hopMs, cut.UnitMs) : default;
        var troughRun = gaps.Separated ? before.TroughRun + 1 : 0;
        var troughMisses = gaps.Separated ? 0 : before.TroughMisses + 1;
        var heldGaps = gaps.Separated ? gaps : before.HeldGaps;
        var held = troughRun >= CwProbabilisticStream.ReadsToEstablishStructure
            || (troughMisses < CwProbabilisticStream.ReadsToEstablishStructure && before.StructureHeld);
        var same = cut.Speed == entrySpeed && held == entryHeld && (!held || heldGaps == entryGaps);
        var result = same
            ? stream.Last
            : CwProbabilisticDecoder.Decode(
                window,
                stream.ToneHz,
                cut.Speed,
                held ? new[] { heldGaps.ElementMilliseconds, heldGaps.CharacterMilliseconds, heldGaps.WordMilliseconds } : null);
        var characterGap = cut.UnitMs > 0 ? CwUnitEstimator.MeasureCharacterGap(window, hopMs, cut.UnitMs) : null;
        var unitMs = result.WordsPerMinute > 0 ? 1200.0 / result.WordsPerMinute : 0;
        var wordFrom = held ? Math.Sqrt(heldGaps.CharacterMilliseconds * heldGaps.WordMilliseconds) : Math.Sqrt(3 * 7) * unitMs;
        var windowStart = (long)HopsSeenField.GetValue(stream)! - window.Length;
        var settleBefore = window.Length - (int)DelayHopsField.GetValue(stream)!;
        var through = before.SettledThrough;
        var text = new System.Text.StringBuilder();
        var spaced = (IEnumerable<(CwProbabilisticCharacter Character, bool Removed)>)SpacedMethod.Invoke(
            null, new object?[] { result, characterGap, wordFrom })!;

        foreach (var (character, removed) in spaced)
        {
            var absolute = windowStart + character.EndHop;

            if (character.EndHop >= settleBefore || absolute <= through)
            {
                continue;
            }

            through = absolute;

            if (!removed)
            {
                text.Append(character.Text == "#" ? MorseAlphabet.Unreadable : character.Text);
            }
        }

        return text.ToString();
    }

    /// <summary>One read, cut the entry's way and the local way.</summary>
    /// <param name="Seconds">Seconds on the audio clock.</param>
    /// <param name="MixHz">The pitch the stream mixed at.</param>
    /// <param name="Cuts">The local cut's blocks.</param>
    /// <param name="Entry">The whole window's cut, measured.</param>
    /// <param name="Local">The local cut, measured.</param>
    /// <param name="EntrySettles">What the stream itself settled on this read.</param>
    /// <param name="EntryReplayed">What the replay says the entry settles, to check the replay against the stream.</param>
    /// <param name="LocalSettles">What the read would settle under the local cut.</param>
    /// <param name="EntryRuns">Every run the whole window's cut made.</param>
    /// <param name="LocalRuns">Every run the local cut made.</param>
    /// <param name="WindowStart">The window's first hop on the audio clock.</param>
    /// <param name="MeasureAgrees">Whether the entry's replayed unit equals `CwUnitEstimator.Measure`'s.</param>
    internal sealed record CutRead(
        double Seconds,
        double MixHz,
        LocalCut Cuts,
        Cut Entry,
        Cut Local,
        string EntrySettles,
        string EntryReplayed,
        string LocalSettles,
        IReadOnlyList<Run> EntryRuns,
        IReadOnlyList<Run> LocalRuns,
        long WindowStart,
        bool MeasureAgrees);

    /// <summary>
    /// Drive the decoder a hop at a time and, on every read, cut its own window
    /// both ways and follow each unit through the stream's speed choice.
    /// </summary>
    /// <remarks>
    /// **ONE READ AT A TIME, FROM THE STREAM'S OWN STATE.** Each read's local
    /// column starts from what the unchanged stream held before that read, so it
    /// answers what that one read would do; a built change carries its own reads
    /// forward, which only the build shows.
    /// </remarks>
    private static IReadOnlyList<CutRead> CutReplay(
        float[] samples, int sampleRate, double toSeconds, double spanSeconds, Func<double, bool> keepRuns)
    {
        var decoder = new CwDecoder(sampleRate, 600);
        var stream = decoder.Stream;
        var hop = decoder.Tracker.HopSamples;
        var reads = new List<CutRead>();
        var settled = new System.Text.StringBuilder();

        stream.CharacterSettled += c => settled.Append(c.Text);

        for (var at = 0L; at + hop <= samples.Length && at / (double)sampleRate <= toSeconds; at += hop)
        {
            var before = new StreamBefore(
                (long)SettledThroughField.GetValue(stream)!,
                (int)TroughRunField.GetValue(stream)!,
                (int)TroughMissesField.GetValue(stream)!,
                (bool)StructureField.GetValue(stream)!,
                (CwUnitEstimator.CwGapLengths)HeldGapsField.GetValue(stream)!);

            decoder.Process(new AudioChunk(at, sampleRate, samples.AsSpan((int)at, hop)));

            if (stream.HopsSinceAnswer != 0
                || stream.EnvelopeHops * CwProbabilisticDecoder.HopMilliseconds < CwProbabilisticStream.RefillSeconds * 1000.0)
            {
                continue;
            }

            var hopsSeen = (long)HopsSeenField.GetValue(stream)!;
            var seconds = hopsSeen * CwProbabilisticDecoder.HopMilliseconds / 1000.0;
            var window = ((double[])EnvelopeField.GetValue(stream)!).Take(stream.EnvelopeHops).ToArray();
            var db = Decibels(window);
            var cuts = LocalCuts(db, spanSeconds);
            var entryRuns = RunsAt(db, Enumerable.Repeat(cuts.Whole, db.Length).ToArray());
            var localRuns = RunsAt(db, cuts.Hop);
            var entry = Measured(entryRuns);
            var local = Measured(localRuns);
            var actual = CwUnitEstimator.Measure(window, CwProbabilisticDecoder.HopMilliseconds);
            var entryHeld = (bool)StructureField.GetValue(stream)!;
            var entryGaps = (CwUnitEstimator.CwGapLengths)HeldGapsField.GetValue(stream)!;

            reads.Add(new CutRead(
                seconds,
                stream.ToneHz,
                cuts,
                entry,
                local,
                settled.ToString(),
                WouldSettle(stream, window, before, entry, entry.Speed, entryHeld, entryGaps),
                WouldSettle(stream, window, before, local, entry.Speed, entryHeld, entryGaps),
                keepRuns(seconds) ? entryRuns : Array.Empty<Run>(),
                keepRuns(seconds) ? localRuns : Array.Empty<Run>(),
                hopsSeen - window.Length,
                Math.Abs(actual.UnitMilliseconds - entry.UnitMs) < 1e-9 && actual.Marks == (entry.UnitMs > 0 ? entry.Marks : actual.Marks)));
            settled.Clear();
        }

        return reads;
    }

    /// <summary>
    /// 7.4, work instruction 438 task 1: every read of the opening, of the locked
    /// stretch and of the two recordings cold, cut once over the whole window as
    /// at entry and once per half second over the 3 s around it.
    /// </summary>
    /// <remarks>
    /// <para>**A PRINTER. IT ASSERTS NOTHING AND WRITES NOTHING.** The cut is the
    /// instruction's, fixed before this ran; nothing under `src` changes. The
    /// entry column is checked against the stream twice: the replayed unit
    /// against `CwUnitEstimator.Measure`, and the replayed settling against what
    /// the stream settled.</para>
    /// <para>The locked stretch is the stream's 90.0 to 106.2 s, `004108` 13.8 to
    /// 30.0 s, read warm as 7.3 read it.</para>
    /// </remarks>
    /// <param name="run">`stream` for the spliced session, or a recording cold.</param>
    [Theory]
    [InlineData("stream")]
    [InlineData("cw-2026-09-24-003901")]
    [InlineData("cw-2026-09-24-003919")]
    public void WhereTheTriggerCuts(string run)
    {
        float[] samples;
        int rate;

        if (run == "stream")
        {
            (samples, rate, _) = Splice(Session);
        }
        else
        {
            var audio = WavAudio.Read(Path.Combine(Folder, run + ".wav"));
            samples = audio.Samples;
            rate = audio.SampleRate;
        }

        var stretches = run == "stream"
            ? new[] { ("opening", 27.0, 46.2), ("locked, 004108 13.8 to 30.0 s", 90.0, 106.2) }
            : new[] { ("cold", 0.0, 60.0) };
        var reads = CutReplay(samples, rate, stretches.Max(s => s.Item3), CutSpanSeconds, s => run == "stream" && s >= 34.5 && s <= 44.5);

        _output.WriteLine(string.Create(
            Invariant,
            $"cut check | run {run}; {reads.Count} reads; blocks {CutBlockSeconds:0.0} s, spans {CutSpanSeconds:0.0} s, a span falls back when its classes stand under {2 * CwUnitEstimator.HysteresisDb:0.0} dB apart; hysteresis {CwUnitEstimator.HysteresisDb:0.0} dB either side"));
        _output.WriteLine(string.Create(
            Invariant,
            $"cut check | the entry's replayed unit against CwUnitEstimator.Measure: {reads.Count(r => !r.MeasureAgrees)} reads differ | the entry's replayed settling against the stream's: {reads.Count(r => r.EntryReplayed != r.EntrySettles)} reads differ | span levels unlike Otsu's: {reads.Sum(r => r.Cuts.OtsuDiffers)}"));

        foreach (var (label, from, to) in stretches)
        {
            var inside = reads.Where(r => r.Seconds >= from && r.Seconds < to).ToList();

            _output.WriteLine("");
            _output.WriteLine(string.Create(Invariant, $"CUT {label.ToUpperInvariant()} | {run} {from:0.0} to {to:0.0} s | {inside.Count} reads"));
            _output.WriteLine($"cut {label} | s | mix Hz | whole cut dB | lowest block dB | highest block dB | fell back of blocks | entry marks | entry short mark ms | entry short gap ms | entry unit ms | entry wpm | entry from | local marks | local short mark ms | local short gap ms | local unit ms | local wpm | local from | entry settles | local settles | same");

            foreach (var r in inside)
            {
                _output.WriteLine(string.Create(
                    Invariant,
                    $"cut {label} | {r.Seconds:0.0} | {r.MixHz:0.0} | {r.Cuts.Whole:0.0} | {r.Cuts.Lowest:0.0} | {r.Cuts.Highest:0.0} | {r.Cuts.FellBack} of {r.Cuts.Blocks} | {r.Entry.Marks} | {r.Entry.ShortMarkMs:0.0} | {r.Entry.ShortGapMs:0.0} | {r.Entry.UnitMs:0.0} | {r.Entry.Wpm:0.0} | {(r.Entry.Speed is null ? "grid" : "estimator")} | {r.Local.Marks} | {r.Local.ShortMarkMs:0.0} | {r.Local.ShortGapMs:0.0} | {r.Local.UnitMs:0.0} | {r.Local.Wpm:0.0} | {(r.Local.Speed is null ? "grid" : "estimator")} | `{r.EntrySettles}` | `{r.LocalSettles}` | {(r.EntrySettles == r.LocalSettles ? "same" : "DIFFERS")}"));
            }

            _output.WriteLine(string.Create(
                Invariant,
                $"cut {label} summary | reads {inside.Count} | entry unit {Spread(inside.Select(r => r.Entry.UnitMs))} | local unit {Spread(inside.Select(r => r.Local.UnitMs))} | local speed from the estimator {inside.Count(r => r.Local.Speed is not null)}, entry {inside.Count(r => r.Entry.Speed is not null)} | reads that settle differently {inside.Count(r => r.EntrySettles != r.LocalSettles)}"));
        }

        if (run != "stream")
        {
            return;
        }

        // **THE PIECES.** Every mark under 40 ms the whole window's cut made on the
        // reads from 34.5 to 44.5 s, and what the local cut made of the same hops.
        _output.WriteLine("");
        _output.WriteLine("piece | read s | at s | length ms | gap after ms | whole cut dB | its block's cut dB | local marks over it | local length ms | joined");

        var pieces = 0;
        var joined = 0;

        foreach (var r in reads.Where(r => r.Seconds >= 34.5 && r.Seconds <= 44.5))
        {
            var runs = r.EntryRuns;

            for (var k = 0; k < runs.Count; k++)
            {
                var m = runs[k];

                if (!m.Mark || !m.Kept || m.Hops * CwProbabilisticDecoder.HopMilliseconds >= 40)
                {
                    continue;
                }

                var over = r.LocalRuns.Where(l => l.Mark && l.Start < m.End && l.End > m.Start).ToList();
                var before = runs.Take(k).LastOrDefault(x => x.Mark && x.Kept);
                var after = runs.Skip(k + 1).FirstOrDefault(x => x.Mark && x.Kept);
                var toBefore = before is not null && over.Any(l => l.Start < before.End);
                var toAfter = after is not null && over.Any(l => l.End > after.Start);
                var verdict = over.Count == 0
                    ? "gone"
                    : toBefore && toAfter ? "joined both sides" : toBefore ? "joined to the one before" : toAfter ? "joined to the one after" : "stands alone";
                var gapAfter = k + 1 < runs.Count && !runs[k + 1].Mark ? runs[k + 1].Hops * CwProbabilisticDecoder.HopMilliseconds : double.NaN;

                pieces++;
                joined += toBefore || toAfter ? 1 : 0;

                _output.WriteLine(string.Create(
                    Invariant,
                    $"piece | {r.Seconds:0.0} | {(r.WindowStart + m.Start) * CwProbabilisticDecoder.HopMilliseconds / 1000.0:0.000} | {m.Hops * CwProbabilisticDecoder.HopMilliseconds:0} | {gapAfter:0} | {r.Cuts.Whole:0.0} | {r.Cuts.Hop[m.Start]:0.0} | {over.Count} | {string.Join("+", over.Select(l => (l.Hops * CwProbabilisticDecoder.HopMilliseconds).ToString("0", Invariant)))} | {verdict}"));
            }
        }

        _output.WriteLine(string.Create(Invariant, $"piece summary | {pieces} marks under 40 ms at entry over the reads 34.5 to 44.5 s | {joined} joined to a neighbor by the local cut"));

        // **THE COST OF ONE LOCAL-CUT MEASURE OVER A FULL WINDOW, ON THIS MACHINE**,
        // beside the entry's, the first pass of each left out as the warm-up.
        var full = CutFullWindow(samples, rate);
        var entryTimes = new List<double>();
        var localTimes = new List<double>();
        var clock = new System.Diagnostics.Stopwatch();

        for (var pass = 0; pass < 21; pass++)
        {
            clock.Restart();
            CwUnitEstimator.Measure(full, CwProbabilisticDecoder.HopMilliseconds);
            clock.Stop();

            if (pass > 0)
            {
                entryTimes.Add(clock.Elapsed.TotalMilliseconds);
            }

            clock.Restart();
            var db = Decibels(full);
            Measured(RunsAt(db, LocalCuts(db, CutSpanSeconds).Hop));
            clock.Stop();

            if (pass > 0)
            {
                localTimes.Add(clock.Elapsed.TotalMilliseconds);
            }
        }

        _output.WriteLine(string.Create(
            Invariant,
            $"cut cost | one full window, {full.Length} hops | entry ms over 20 passes | {Spread(entryTimes)} | local ms over 20 passes | {Spread(localTimes)} | the stream reads every {CwProbabilisticStream.ReadEverySeconds * 1000:0} ms"));

        // **THE ONE STOP, AS THE INSTRUCTION STATES IT.**
        var middle = reads.Where(r => r.Seconds >= 34.5 && r.Seconds <= 44.5).ToList();
        var opening = reads.Where(r => r.Seconds >= 30.0 && r.Seconds < 46.2).ToList();
        var staysLow = middle.All(r => r.Local.UnitMs < 40);
        var sameText = opening.All(r => r.EntrySettles == r.LocalSettles);

        _output.WriteLine(string.Create(
            Invariant,
            $"cut stop | 34.5 to 44.5 s: {middle.Count} reads, local unit under 40 ms on {middle.Count(r => r.Local.UnitMs < 40)}, so it stays below 40 on every read: {(staysLow ? "yes" : "no")} | 30 to 46.2 s: {opening.Count} reads, {opening.Count(r => r.EntrySettles != r.LocalSettles)} settle differently, so the same on every read: {(sameText ? "yes" : "no")} | the stop {(staysLow || sameText ? "HOLDS, build nothing" : "does not hold, task 2 builds")}"));
    }

    /// <summary>The stream's window the first time it holds a full 12 s, for timing.</summary>
    private static double[] CutFullWindow(float[] samples, int sampleRate)
    {
        var decoder = new CwDecoder(sampleRate, 600);
        var stream = decoder.Stream;
        var hop = decoder.Tracker.HopSamples;
        var full = (int)Math.Round(CwProbabilisticStream.WindowSeconds * 1000.0 / CwProbabilisticDecoder.HopMilliseconds);

        for (var at = 0L; at + hop <= samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, sampleRate, samples.AsSpan((int)at, hop)));

            if (stream.EnvelopeHops >= full)
            {
                return ((double[])EnvelopeField.GetValue(stream)!).Take(stream.EnvelopeHops).ToArray();
            }
        }

        return ((double[])EnvelopeField.GetValue(stream)!).Take(stream.EnvelopeHops).ToArray();
    }

    private static IReadOnlyList<Heard> InStretch(Traced traced, double from, double to)
        => traced.Settled
            .Where(h => h.Character.At.TotalSeconds >= from && h.Character.At.TotalSeconds < to)
            .ToList();

    private static IReadOnlyList<Read> ReadsIn(Traced traced, double from, double to)
        => traced.Reads.Where(r => r.Seconds >= from && r.Seconds < to).ToList();

    private void Print(string label, string run, IReadOnlyList<Heard> settled, Piece piece, double offset, string survey)
    {
        _output.WriteLine($"{label} | at s | in {piece.Name} s | text | pattern | elements | held wpm | held unit ms | from | estimator unit ms | estimator wpm | structure | held gaps ms | mix Hz | tracker Hz | survey Hz | window ratio | span margin | raw span | settled at s");

        foreach (var h in settled.Where(h => !h.Character.IsWordGap))
        {
            var c = h.Character;
            var r = h.By;

            _output.WriteLine(string.Create(
                Invariant,
                $"{label} | {c.At.TotalSeconds:0.000} | {c.At.TotalSeconds + offset:0.000} | `{c.Text}` | {c.Pattern} | {h.Elements} | {r.HeldWpm:0} | {r.HeldUnitMs:0.0} | {r.SpeedFrom} | {r.EstimatorUnitMs:0.0} | {(r.EstimatorUnitMs > 0 ? 1200.0 / r.EstimatorUnitMs : 0):0.0} | {(r.StructureHeld ? "held" : "-")} | {r.HeldGaps} | {h.MixHz:0.0} | {h.TrackerHz:0.0} | {survey} | {r.WindowRatio:0.000} | {c.SpanMarginForRecord:0.000} | {c.SpanLogLikelihoodRatio:0.0} | {r.Seconds:0.0}"));
        }
    }

    private static string Spread(IEnumerable<double> values)
    {
        var sorted = values.Where(v => !double.IsNaN(v)).OrderBy(v => v).ToArray();

        if (sorted.Length == 0)
        {
            return "0 | - | - | - | - | -";
        }

        double At(double q) => sorted[(int)Math.Round(q * (sorted.Length - 1))];

        return string.Create(
            Invariant,
            $"{sorted.Length} | {sorted[0]:0.###} | {At(0.25):0.###} | {Median(sorted):0.###} | {At(0.75):0.###} | {sorted[^1]:0.###}");
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
