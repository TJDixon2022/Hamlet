using System.Reflection;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// A printer, not a test: every move of the tone tracker's fine bank, on the
/// single-sender reds and on moves that follow a real handover, and #42's
/// fixture given the radio's word (work instruction 406, task 1).
/// </summary>
/// <remarks>
/// <para>**IT ASSERTS NOTHING** and is on no carry-forward line. `Switch` is
/// private, so a call into it is recognized from outside: the tracker's
/// `Retunes` count goes up and a pitch is reported afterwards. A move made from
/// cold, before anything was confirmed, also counts a retune and reports no
/// pitch, and is printed as a cold move rather than a switch. What the survey
/// held is read by reflection one hop before each survey read, in the printer
/// only.</para>
/// </remarks>
public sealed class TheTrackerSwitchTraceTests
{
    private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;

    private static readonly FieldInfo OnReadingField =
        typeof(CwDecoder).GetField("_onReading", Private)!;

    private static readonly FieldInfo FineHzField =
        typeof(CwToneTracker).GetField("_fineHz", Private)!;

    private static readonly FieldInfo HeldSwitchField =
        typeof(CwToneTracker).GetField("_heldSwitchHz", Private)!;

    private static readonly FieldInfo PreviousKeyedField =
        typeof(CwToneTracker).GetField("_previousKeyedHz", Private)!;

    private static readonly FieldInfo ReadingDbField =
        typeof(CwToneTracker).GetField("_readingDb", Private)!;

    private static readonly FieldInfo HopsSinceSurveyField =
        typeof(CwToneTracker).GetField("_hopsSinceSurvey", Private)!;

    private static readonly FieldInfo SurveyField =
        typeof(CwToneTracker).GetField("_survey", Private)!;

    private static readonly FieldInfo FineSurveyField =
        typeof(CwToneTracker).GetField("_fineSurvey", Private)!;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the printer.</summary>
    /// <param name="output">Where the rows are printed.</param>
    public TheTrackerSwitchTraceTests(ITestOutputHelper output) => _output = output;

    /// <summary>What the tracker held one hop before a survey read.</summary>
    private sealed record Before(
        double Seconds,
        double CenterHz,
        double HeldHz,
        double PreviousKeyedHz,
        double ReadingDb,
        bool MidCharacter,
        IReadOnlyList<KeyingCandidate> Coarse,
        KeyingCandidate? Fine,
        double PowerDb,
        double NoiseDb);

    /// <summary>One call into `Switch`, or one cold move.</summary>
    private sealed record Move(
        double Seconds,
        string Caller,
        double FromHz,
        double ToHz,
        Before? Snapshot,
        KeyingCandidate? Keyed,
        ToneInterference? Interference);

    private static double Center(CwToneTracker tracker)
    {
        var fine = (double[])FineHzField.GetValue(tracker)!;

        return fine[fine.Length / 2];
    }

    private static string Candidate(KeyingCandidate? c)
        => c is { } k
            ? $"{k.ToneHz:0.0} Hz lift {k.LiftDb:0.0} keyed {k.KeyedDb:0.0} sep {k.Separation:0.0} "
              + $"marks {k.Marks} dit {k.DitMilliseconds:0} dah {k.DahMilliseconds:0} ({k.WordsPerMinute:0.0} wpm)"
            : "none";

    private static string Candidates(IReadOnlyList<KeyingCandidate> list)
        => list.Count == 0
            ? "none"
            : string.Join(" ; ", list.Select(k =>
                $"{k.ToneHz:0}: lift {k.LiftDb:0.0} keyed {k.KeyedDb:0.0} sep {k.Separation:0.0} m{k.Marks} "
                + $"{k.WordsPerMinute:0.0}wpm"));

    /// <summary>
    /// Decode, recording every move of the fine bank and every settled character.
    /// </summary>
    private (List<Move> Moves, List<CwCharacter> Settled, CwDecoder Decoder) Run(
        string label, MonoAudio audio, double startHz, bool hopFed, Func<double, double> senderAt)
    {
        var decoder = new CwDecoder(audio.SampleRate, startHz);
        var tracker = decoder.Tracker;
        var stream = decoder.Stream;
        var moves = new List<Move>();
        var settled = new List<CwCharacter>();
        var original = (Action<ToneReading>)OnReadingField.GetValue(decoder)!;
        var retunes = tracker.Retunes;
        var lastCenter = Center(tracker);
        var mixHz = double.NaN;
        Before? before = null;
        var heldBefore = double.NaN;

        OnReadingField.SetValue(decoder, (Action<ToneReading>)(r =>
        {
            original(r);

            var seconds = r.SampleIndex / (double)audio.SampleRate;

            if (tracker.Retunes != retunes)
            {
                var center = Center(tracker);
                var held = (double)HeldSwitchField.GetValue(tracker)!;
                var caller = !tracker.HasMeasuredPitch
                    ? "cold 1004"
                    : !double.IsNaN(heldBefore) && double.IsNaN(held)
                        ? "Switch via 959 (the hold)"
                        : "Switch via 1092";

                moves.Add(new Move(
                    seconds, caller, lastCenter, center, before,
                    tracker.Verdict.Keyed, tracker.Verdict.Interference));
                retunes = tracker.Retunes;
                lastCenter = center;
            }

            if (stream.ToneHz != mixHz)
            {
                mixHz = stream.ToneHz;
                _output.WriteLine($"mix | {label} | {seconds:0.000} s | stream mixes at {stream.ToneHz:0.0} Hz "
                    + $"| bank centre {Center(tracker):0.0} | measured {tracker.HasMeasuredPitch} "
                    + $"| sender {senderAt(seconds):0}");
            }

            heldBefore = (double)HeldSwitchField.GetValue(tracker)!;

            // One hop before the tracker reads its survey: what both banks hold.
            if ((int)HopsSinceSurveyField.GetValue(tracker)! == 99)
            {
                var coarse = (CwToneSurvey)SurveyField.GetValue(tracker)!;
                var fine = (CwToneSurvey)FineSurveyField.GetValue(tracker)!;

                before = new Before(
                    seconds,
                    Center(tracker),
                    heldBefore,
                    (double)PreviousKeyedField.GetValue(tracker)!,
                    (double)ReadingDbField.GetValue(tracker)!,
                    tracker.MidCharacter,
                    coarse.Candidates(),
                    fine.Analyze().Keyed,
                    r.PowerDb,
                    r.NoiseDb);
            }
        }));

        decoder.CharacterSettled += settled.Add;

        if (hopFed)
        {
            var hop = tracker.HopSamples;

            for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
            {
                decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
            }
        }
        else
        {
            using var source = new BufferedAudioSource(audio);
            decoder.Listen(source);
            source.PumpAll();
        }

        decoder.Flush();

        return (moves, settled, decoder);
    }

    /// <summary>
    /// Whether the survey read when a held move went still put its best keying
    /// within one coarse bin of the held pitch: the re-confirmation the comment
    /// at the hold promises.
    /// </summary>
    private static string ReAdmitted(Move m)
    {
        if (m.Snapshot is not { } s || !m.Caller.Contains("959", StringComparison.Ordinal))
        {
            return "not a held move";
        }

        var best = s.Coarse
            .OrderByDescending(c => double.IsNaN(c.LiftDb) ? double.NegativeInfinity : c.LiftDb)
            .ThenByDescending(c => c.KeyedDb)
            .Select(c => (KeyingCandidate?)c)
            .FirstOrDefault();

        return best is { } b
            ? $"survey best at execution {b.ToneHz:0}, held {s.HeldHz:0}, apart {Math.Abs(b.ToneHz - s.HeldHz):0} "
              + $"-> {(Math.Abs(b.ToneHz - s.HeldHz) <= 25 ? "RE-ADMITTED" : "STALE")}"
            : $"survey admits nothing at execution, held {s.HeldHz:0} -> STALE";
    }

    private void PrintMoves(string label, List<Move> moves, Func<double, double> senderAt)
    {
        foreach (var m in moves)
        {
            var sender = senderAt(m.Seconds);
            var s = m.Snapshot;
            var moved = Math.Abs(m.ToHz - m.FromHz);
            var confirm = s is null || double.IsNaN(s.PreviousKeyedHz) || m.Keyed is not { } k
                ? "n/a"
                : $"previous survey {s.PreviousKeyedHz:0.0}, this {k.ToneHz:0.0}, apart {Math.Abs(s.PreviousKeyedHz - k.ToneHz):0.0} (within 25)";
            var floor = s is null || double.IsNaN(s.ReadingDb) || m.Keyed is not { } kk
                ? "reading level unknown, HM-DEC-127 floor not asked"
                : $"reading {s.ReadingDb:0.0} dB, candidate keyed {kk.KeyedDb:0.0} dB, {kk.KeyedDb - s.ReadingDb:+0.0;-0.0} (floor -25)";
            var through = m.Caller.Contains("959", StringComparison.Ordinal)
                ? "the hold"
                : m.Caller.StartsWith("cold", StringComparison.Ordinal) ? "cold start, nothing confirmed" : "the confirm, then the reach";

            _output.WriteLine($"move | {label} | {m.Seconds:0.000} s | {m.Caller} | {m.FromHz:0.0} to {m.ToHz:0.0} Hz, "
                + $"moved {moved:0.0} | sender {sender:0} | from off sender {Math.Abs(m.FromHz - sender):0.0}, "
                + $"to off sender {Math.Abs(m.ToHz - sender):0.0} | {(Math.Abs(m.ToHz - sender) > Math.Abs(m.FromHz - sender) + 1 ? "AWAY" : "TOWARD")} "
                + $"| through {through}");
            _output.WriteLine($"move-why | {label} | {m.Seconds:0.000} s | {confirm} | reach {moved:0.0} > 15 | {floor} "
                + $"| mid-character before {s?.MidCharacter} | held before {s?.HeldHz:0.0}");
            _output.WriteLine($"move-keyed | {label} | {m.Seconds:0.000} s | verdict keyed {Candidate(m.Keyed)} "
                + $"| interference {(m.Interference is { } i ? $"{i.ToneHz:0} lift {i.LiftDb:0.0}" : "none")} "
                + $"| reading power {s?.PowerDb:0.0} noise {s?.NoiseDb:0.0} dB");
            _output.WriteLine($"move-coarse | {label} | {m.Seconds:0.000} s | admitted one hop before: "
                + $"{(s is null ? "n/a" : Candidates(s.Coarse))}");
            _output.WriteLine($"move-fine | {label} | {m.Seconds:0.000} s | fine bank at {s?.CenterHz:0.0} one hop before: "
                + $"{(s is null ? "n/a" : Candidate(s.Fine))}");
            _output.WriteLine($"move-hold | {label} | {m.Seconds:0.000} s | {m.Caller} | {ReAdmitted(m)}");

            if (s is not null && m.Keyed is { } chosen)
            {
                var near = s.Coarse.Where(c => Math.Abs(c.ToneHz - chosen.ToneHz) <= 50 && c.ToneHz != chosen.ToneHz).ToList();
                var louder = near.Where(c => c.KeyedDb > chosen.KeyedDb).ToList();

                _output.WriteLine($"move-props | {label} | {m.Seconds:0.000} s | fine bank still keyed at the old centre: "
                    + $"{s.Fine is not null} | admitted within 50 Hz of the chosen bin: {near.Count}, louder key-down: "
                    + $"{louder.Count} {(louder.Count > 0 ? $"(best {louder.Max(c => c.KeyedDb) - chosen.KeyedDb:+0.0} dB at {louder.MaxBy(c => c.KeyedDb).ToneHz:0})" : "")} "
                    + $"| chosen wpm {chosen.WordsPerMinute:0.0} vs fine-at-old {(s.Fine is { } f ? f.WordsPerMinute.ToString("0.0") : "n/a")}");
            }
        }
    }

    private void PrintMisses(string label, List<CwCharacter> settled, string sent, List<Move> moves)
    {
        var matches = CwAlignment.Align(settled, sent);
        var kinds = new StringBuilder();

        foreach (var m in matches.Where(m => !m.Decoded.IsWordGap))
        {
            kinds.Append(m.Kind switch
            {
                CwMatchKind.Correct => m.Decoded.Text,
                CwMatchKind.Wrong => $"[{m.Decoded.Text}/{m.Expected}]",
                _ => $"(+{m.Decoded.Text})",
            });

            if (m.Kind != CwMatchKind.Correct)
            {
                var end = m.Decoded.At.TotalSeconds;
                var last = moves.Where(v => v.Seconds <= end && !v.Caller.StartsWith("cold", StringComparison.Ordinal))
                    .Select(v => (Move?)v).LastOrDefault();

                _output.WriteLine($"miss | {label} | '{m.Decoded.Text}' {m.Kind} expected '{m.Expected}' ends {end:0.000} s "
                    + $"| last switch before it {(last is { } l ? $"{l.Seconds:0.000} s to {l.ToHz:0.0} Hz, {end - l.Seconds:0.00} s earlier" : "none")}");
            }
        }

        _output.WriteLine($"read | {label} | settled '{string.Concat(settled.Select(c => c.Text))}' | kinds {kinds}");
    }

    /// <summary>#6 and #15, every seed, exactly as their test builds them.</summary>
    /// <param name="wordsPerMinute">The sender.</param>
    /// <param name="prefix">The run-up, or none.</param>
    [Theory]
    [InlineData(25, "")]
    [InlineData(12, "VVV ")]
    public void TheAcquisitionReds(int wordsPerMinute, string prefix)
    {
        const string call = "CQ CQ DE N0CALL N0CALL K";
        var total = 0.0;

        foreach (var seed in new[] { 7919, 104729, 15485863 })
        {
            var label = $"{wordsPerMinute}wpm {(prefix.Length == 0 ? "bare" : "run-up")} seed {seed}";
            var audio = CwSignal.Generate(new CwSignalRequest(
                prefix + call, WordsPerMinute: wordsPerMinute, ToneHz: 640, Amplitude: 0.5,
                NoiseAmplitude: CwSensitivity.NoiseFor(18.0), Seed: seed));

            var (moves, settled, decoder) = Run(label, audio, CwSignal.DefaultToneHz, false, _ => 640);

            PrintMoves(label, moves, _ => 640);
            PrintMisses(label, settled, prefix + call, moves);

            var share = (double)CwAlignment.Align(settled, prefix + call).Count(m =>
                m.Kind == CwMatchKind.Correct && !m.Decoded.IsWordGap && m.Expected != "V")
                / call.Count(c => c != ' ');

            total += share;
            _output.WriteLine($"sum | {label} | share {share:0.00} | switches {moves.Count(m => !m.Caller.StartsWith("cold", StringComparison.Ordinal))} "
                + $"| cold moves {moves.Count(m => m.Caller.StartsWith("cold", StringComparison.Ordinal))} | final tone {decoder.Report.ToneHz:0.0} "
                + $"| measured {decoder.Tracker.HasMeasuredPitch}");
        }

        _output.WriteLine($"sum | {wordsPerMinute}wpm {(prefix.Length == 0 ? "bare" : "run-up")} | mean share {total / 3:0.00}");
    }

    /// <summary>The easy tier behind #43 to #45, fed as their test feeds them.</summary>
    /// <param name="name">The fixture.</param>
    [Theory]
    [InlineData("coverage-easy")]
    [InlineData("exchange-easy")]
    [InlineData("tightfist-easy")]
    public void TheEasyTier(string name)
    {
        var recipe = CwFixtureCatalogue.All.Single(r => r.Name == name);
        var sent = recipe.Text.Replace("^", "", StringComparison.Ordinal);
        var audio = WavAudio.Read(Path.Combine(CwFixtureCatalogue.Folder, name + ".wav"));
        var (moves, settled, decoder) = Run(name, audio, 600, false, _ => recipe.ToneHz);

        PrintMoves(name, moves, _ => recipe.ToneHz);
        PrintMisses(name, settled, sent, moves);
        _output.WriteLine($"sum | {name} | sender {recipe.ToneHz:0} | switches {moves.Count(m => !m.Caller.StartsWith("cold", StringComparison.Ordinal))} "
            + $"| final tone {decoder.Report.ToneHz:0.0}");
    }

    /// <summary>A real handover: the two-station fixture, which is also #41's audio.</summary>
    [Fact]
    public void TheTwoStationHandover()
    {
        const string name = CwFixtureCatalogue.TwoStationName;
        var audio = WavAudio.Read(Path.Combine(CwFixtureCatalogue.Folder, name + ".wav"));

        // The sidecar's handover, 23.09 s: the caller at 615 before it, the answerer at 730 after.
        double Sender(double s) => s < 23.09 ? 615 : 730;

        var (moves, settled, decoder) = Run(name, audio, 600, false, Sender);

        PrintMoves(name, moves, Sender);
        PrintMisses(name, settled, "CQ CQ DE N0CALL K N0CALL DE W1XYZ K", moves);
        _output.WriteLine($"sum | {name} | switches {moves.Count(m => !m.Caller.StartsWith("cold", StringComparison.Ordinal))} "
            + $"| final tone {decoder.Report.ToneHz:0.0}");
    }

    /// <summary>Two named captures and every other floor capture, fed as the floors feed them.</summary>
    [Fact]
    public void TheCaptures()
    {
        foreach (var row in TheCapturesThatDecodeKeepDecodingTests.Floors)
        {
            var name = (string)row[0];
            var audio = WavAudio.Read(Path.Combine(CapturedSignalTests.Folder, name + ".wav"));
            var (moves, settled, decoder) = Run(name, audio, 600, true, _ => double.NaN);
            var named = name.EndsWith("021410", StringComparison.Ordinal) || name.EndsWith("013637", StringComparison.Ordinal);

            if (named)
            {
                PrintMoves(name, moves, _ => decoder.Report.ToneHz);
            }

            var switches = moves.Where(m => !m.Caller.StartsWith("cold", StringComparison.Ordinal)).ToList();

            _output.WriteLine($"cap | {name} | characters {decoder.Report.CharactersEmitted} | final tone {decoder.Report.ToneHz:0.0} "
                + $"| cold moves {moves.Count - switches.Count} | switches {switches.Count}: "
                + string.Join(", ", switches.Select(m => $"{m.Seconds:0.0}s {m.FromHz:0}->{m.ToHz:0} "
                    + $"{(m.Caller.Contains("959", StringComparison.Ordinal) ? (ReAdmitted(m).EndsWith("STALE", StringComparison.Ordinal) ? "hold-STALE" : "hold-READMITTED") : "direct")} "
                    + $"fineOld {(m.Snapshot?.Fine is { } f ? f.ToneHz.ToString("0") : "-")} "
                    + $"near-louder {(m.Snapshot is { } s && m.Keyed is { } k ? s.Coarse.Count(c => Math.Abs(c.ToneHz - k.ToneHz) <= 50 && c.ToneHz != k.ToneHz && c.KeyedDb > k.KeyedDb) : -1)}")));

            if (!named && switches.Count > 0)
            {
                PrintMoves(name, moves, _ => decoder.Report.ToneHz);
            }
        }
    }

    /// <summary>
    /// #42's plumbing: the fixture pumped a chunk at a time with the radio's
    /// report of its own transmitter, from the recipe's span.
    /// </summary>
    /// <param name="clock">Which clock is handed to the decoder: audio time, or the wall.</param>
    [Theory]
    [InlineData("audio")]
    [InlineData("wall")]
    [InlineData("none")]
    public void TheRadiosWordOnThePreamble(string clock)
    {
        var recipe = CwFixtureCatalogue.All.Single(r => r.Name == "qsk-preamble");
        var audio = WavAudio.Read(Path.Combine(CwFixtureCatalogue.Folder, "qsk-preamble.wav"));
        var sidecar = File.ReadAllLines(Path.Combine(CwFixtureCatalogue.Folder, "qsk-preamble.txt"));
        var messageStart = double.Parse(
            sidecar.Single(l => l.StartsWith("messageStart", StringComparison.Ordinal))
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)[1],
            System.Globalization.CultureInfo.InvariantCulture);

        // The operator's own sending ends where the message starts, and began
        // the recipe's preamble before it.
        var spanEnd = messageStart;
        var spanStart = messageStart - recipe.PreambleSeconds;

        var decoder = new CwDecoder(audio.SampleRate, 600);
        var chars = new List<CwCharacter>();
        var settled = new List<CwCharacter>();
        var epoch = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var suspendedIn = 0;
        var chunksIn = 0;
        var notSuspendedAfter = double.NaN;

        decoder.CharacterDecoded += chars.Add;
        decoder.CharacterSettled += settled.Add;

        using (var source = new BufferedAudioSource(audio))
        {
            decoder.Listen(source);

            var at = 0L;

            while (!source.IsFinished)
            {
                var from = at / (double)audio.SampleRate;
                var to = Math.Min(audio.Samples.Length, at + BufferedAudioSource.DefaultChunkSamples) / (double)audio.SampleRate;
                var now = clock == "audio" ? epoch.AddSeconds(from) : DateTime.UtcNow;

                if (clock != "none")
                {
                    decoder.RadioIsTransmitting(to > spanStart && from < spanEnd, now);
                }

                if (to > spanStart && from < spanEnd)
                {
                    chunksIn++;

                    if (decoder.DecodingSuspended)
                    {
                        suspendedIn++;
                    }
                }
                else if (from >= spanEnd && !decoder.DecodingSuspended && double.IsNaN(notSuspendedAfter))
                {
                    notSuspendedAfter = from;
                }

                source.PumpOnce();
                at += BufferedAudioSource.DefaultChunkSamples;
            }
        }

        decoder.Flush();

        var during = chars.Count(c => c.At.TotalSeconds < 13);

        _output.WriteLine($"42 | clock {clock} | span {spanStart:0.00} to {spanEnd:0.00} s from the recipe (PreambleSeconds "
            + $"{recipe.PreambleSeconds:0.0}) and the sidecar (messageStart {messageStart:0.00}) | chunks in span {chunksIn}, "
            + $"suspended {suspendedIn} | first chunk decoded after the span {notSuspendedAfter:0.00} s");
        _output.WriteLine($"42 | clock {clock} | during {during} | total {chars.Count} | settled {settled.Count} "
            + $"| own transmit {decoder.Report.OwnTransmitSeconds:0.00} s (assert > 3) | suspended chunks {decoder.SuspendedChunks} "
            + $"| guard transmissions {decoder.Tracker.Guard.Transmissions}");
        _output.WriteLine($"42 | clock {clock} | earliest decoded At {(chars.Count > 0 ? chars.Min(c => c.At.TotalSeconds) : double.NaN):0.000} s "
            + $"| earliest settled At {(settled.Count > 0 ? settled.Min(c => c.At.TotalSeconds) : double.NaN):0.000} s "
            + $"| settled '{string.Concat(settled.Select(c => c.Text))}'");

        foreach (var c in chars.Where(c => c.At.TotalSeconds < 13).Take(10))
        {
            _output.WriteLine($"42-during | clock {clock} | '{c.Text}' At {c.At.TotalSeconds:0.000} s");
        }
    }
}
