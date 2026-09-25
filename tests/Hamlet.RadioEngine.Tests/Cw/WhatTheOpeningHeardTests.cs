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
