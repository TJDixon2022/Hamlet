using System.Reflection;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// A printer, not a test: whether the pitch the tracker is reading is still
/// keying when a held move goes, on every move of the reds, the handovers and the
/// captures; #15 seed 104729's reading level; and #45's tail given the air's
/// trailing silence (work instruction 407, task 1).
/// </summary>
/// <remarks>
/// <para>**IT ASSERTS NOTHING** and is on no carry-forward line. Private state is
/// read by reflection here only.</para>
/// <para>**THE PROPERTY, AS MEASURED.** Every hop, the fine bank's centre bin is
/// read against the band beside it, the reading's own `NoiseDb`, which is taken
/// through the same filter. A mark starts when the centre stands
/// <see cref="CwToneSurvey.InterferenceLiftDb"/> over the band and ends when it
/// falls six decibels below that, the survey's own hysteresis either side, and
/// a majority over twenty-five milliseconds removes anything shorter than the
/// shortest element. *Still keying* is at least one key-up between two marks
/// inside the span the hold waited through. Nothing is compared between two
/// pitches (HM-DEC-095). The survey's own mark rule, the centre bin's two-level
/// split over its last three seconds, is printed beside it and decides nothing.</para>
/// </remarks>
public sealed class TheStationStillKeyingTraceTests
{
    private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;

    /// <summary>The survey's own hysteresis, three decibels either side.</summary>
    private const double HysteresisDb = 3.0;

    /// <summary>Twenty-five milliseconds of five millisecond hops.</summary>
    private const int DeglitchHops = 5;

    private static readonly FieldInfo OnReadingField =
        typeof(CwDecoder).GetField("_onReading", Private)!;

    private static readonly FieldInfo FineHzField =
        typeof(CwToneTracker).GetField("_fineHz", Private)!;

    private static readonly FieldInfo FineDbField =
        typeof(CwToneTracker).GetField("_fineDb", Private)!;

    private static readonly FieldInfo HeldSwitchField =
        typeof(CwToneTracker).GetField("_heldSwitchHz", Private)!;

    private static readonly FieldInfo ReadingDbField =
        typeof(CwToneTracker).GetField("_readingDb", Private)!;

    private static readonly FieldInfo HopsSinceSurveyField =
        typeof(CwToneTracker).GetField("_hopsSinceSurvey", Private)!;

    private static readonly FieldInfo SurveyField =
        typeof(CwToneTracker).GetField("_survey", Private)!;

    private static readonly FieldInfo HistoryField =
        typeof(CwToneSurvey).GetField("_history", Private)!;

    private static readonly FieldInfo FilledField =
        typeof(CwToneSurvey).GetField("_filled", Private)!;

    private static readonly FieldInfo BinHzField =
        typeof(CwToneSurvey).GetField("_binHz", Private)!;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the printer.</summary>
    /// <param name="output">Where the rows are printed.</param>
    public TheStationStillKeyingTraceTests(ITestOutputHelper output) => _output = output;

    /// <summary>One hop as the tracker saw it.</summary>
    private sealed record Hop(double Seconds, double CenterHz, double CenterDb, double NoiseDb, bool Mid);

    /// <summary>
    /// A held move, a hold dropped without moving, a direct switch or a cold move.
    /// </summary>
    private sealed record Move(
        int AtHop, double Seconds, string Caller, double FromHz, double ToHz, int SpanFrom, double ReadingDb);

    /// <summary>What one run left behind.</summary>
    private sealed record Trace(
        List<Hop> Hops, List<Move> Moves, List<CwCharacter> Settled, CwDecoder Decoder, List<(double Seconds, string Row)> Confirms);

    private static double Center(CwToneTracker tracker)
    {
        var fine = (double[])FineHzField.GetValue(tracker)!;

        return fine[fine.Length / 2];
    }

    private static double CenterDb(CwToneTracker tracker)
    {
        var fine = (double[])FineDbField.GetValue(tracker)!;

        return fine[fine.Length / 2];
    }

    /// <summary>
    /// The keyed level of one coarse bin: the upper of two levels in its history,
    /// found the way the survey finds them.
    /// </summary>
    private static double KeyedLevel(CwToneSurvey survey, double hz)
    {
        var history = (float[])HistoryField.GetValue(survey)!;
        var filled = (int)FilledField.GetValue(survey)!;
        var bins = (double[])BinHzField.GetValue(survey)!;
        var bin = Array.FindIndex(bins, b => Math.Abs(b - hz) < 0.5);

        if (bin < 0 || filled < 8)
        {
            return double.NaN;
        }

        var values = Enumerable.Range(0, filled).Select(i => (double)history[(i * bins.Length) + bin]).ToList();

        return TwoLevels(values).High;
    }

    private static (double Low, double High, double Split) TwoLevels(IReadOnlyList<double> values)
    {
        var split = values.Average();
        double low = split, high = split;

        for (var pass = 0; pass < 15; pass++)
        {
            var below = values.Where(v => v < split).ToList();
            var above = values.Where(v => v >= split).ToList();

            if (below.Count == 0 || above.Count == 0)
            {
                break;
            }

            low = below.Average();
            high = above.Average();

            var next = (low + high) / 2;

            if (Math.Abs(next - split) < 1e-9)
            {
                break;
            }

            split = next;
        }

        return (low, high, split);
    }

    /// <summary>Key state per hop, with hysteresis and a majority vote.</summary>
    private static bool[] KeyState(IReadOnlyList<double> lift, double onAbove, double offBelow)
    {
        var raw = new bool[lift.Count];
        var on = false;

        for (var i = 0; i < lift.Count; i++)
        {
            if (on && lift[i] < offBelow)
            {
                on = false;
            }
            else if (!on && lift[i] > onAbove)
            {
                on = true;
            }

            raw[i] = on;
        }

        var voted = new bool[raw.Length];
        var half = DeglitchHops / 2;

        for (var i = 0; i < raw.Length; i++)
        {
            int down = 0, seen = 0;

            for (var k = -half; k <= half; k++)
            {
                if (i + k >= 0 && i + k < raw.Length)
                {
                    seen++;
                    down += raw[i + k] ? 1 : 0;
                }
            }

            voted[i] = down * 2 > seen;
        }

        return voted;
    }

    /// <summary>Marks touching the span, and key-ups lying between two of them.</summary>
    private static (int Marks, int KeyUps) Count(bool[] state, int from, int to)
    {
        var marks = 0;
        var keyUps = 0;
        var sawMark = false;
        var pendingGap = false;

        for (var i = from; i < to; i++)
        {
            var starts = state[i] && (i == from || !state[i - 1]);

            if (starts)
            {
                marks++;

                if (sawMark && pendingGap)
                {
                    keyUps++;
                }

                sawMark = true;
                pendingGap = false;
            }
            else if (!state[i] && sawMark)
            {
                pendingGap = true;
            }
        }

        return (marks, keyUps);
    }

    /// <summary>The property over hops [from, to) of one trace, both rules.</summary>
    private static (int Marks, int KeyUps, int SelfMarks, int SelfKeyUps, double MeanLift) Keying(
        List<Hop> hops, int from, int to)
    {
        from = Math.Clamp(from, 0, hops.Count);
        to = Math.Clamp(to, from, hops.Count);

        var lift = hops.Select(h => h.CenterDb - h.NoiseDb).Select(v => double.IsNaN(v) ? -99 : v).ToList();
        var noiseRule = KeyState(
            lift, CwToneSurvey.InterferenceLiftDb, CwToneSurvey.InterferenceLiftDb - (2 * HysteresisDb));
        var (marks, keyUps) = Count(noiseRule, from, to);

        // The survey's own rule: the centre bin's own two levels over the three
        // seconds ending with the span.
        var historyFrom = Math.Max(0, to - 600);
        var levels = hops.Skip(historyFrom).Take(to - historyFrom).Select(h => h.CenterDb).ToList();
        var selfMarks = 0;
        var selfKeyUps = 0;

        if (levels.Count >= 8)
        {
            var split = TwoLevels(levels).Split;
            var self = KeyState(levels, split + HysteresisDb, split - HysteresisDb);
            (selfMarks, selfKeyUps) = Count(self, Math.Max(0, from - historyFrom), to - historyFrom);
        }

        var meanLift = to > from ? lift.Skip(from).Take(to - from).Average() : double.NaN;

        return (marks, keyUps, selfMarks, selfKeyUps, meanLift);
    }

    /// <summary>
    /// Decode, recording every hop at the bank's centre and every move.
    /// </summary>
    private Trace Run(MonoAudio audio, double startHz, string feed, int padSamples = 0, bool flush = true)
    {
        var decoder = new CwDecoder(audio.SampleRate, startHz);
        var tracker = decoder.Tracker;
        var hops = new List<Hop>();
        var moves = new List<Move>();
        var settled = new List<CwCharacter>();
        var confirms = new List<(double Seconds, string Row)>();
        var original = (Action<ToneReading>)OnReadingField.GetValue(decoder)!;
        var retunes = tracker.Retunes;
        var lastCenter = Center(tracker);
        var heldPrev = double.NaN;
        var holdFrom = -1;
        var readingBefore = double.NaN;

        OnReadingField.SetValue(decoder, (Action<ToneReading>)(r =>
        {
            original(r);

            var seconds = r.SampleIndex / (double)audio.SampleRate;
            var at = hops.Count;

            // The centre bin was measured this hop, before any move this hop made.
            hops.Add(new Hop(seconds, lastCenter, CenterDb(tracker), r.NoiseDb, tracker.MidCharacter));

            var held = (double)HeldSwitchField.GetValue(tracker)!;
            var moved = tracker.Retunes != retunes;
            var center = Center(tracker);
            var released = !double.IsNaN(heldPrev) && double.IsNaN(held);

            if (moved)
            {
                var caller = !tracker.HasMeasuredPitch
                    ? "cold"
                    : released ? "hold" : "direct";
                var spanFrom = caller == "hold" ? holdFrom : Math.Max(0, at - 100);

                moves.Add(new Move(at, seconds, caller, lastCenter, center, spanFrom, readingBefore));
                retunes = tracker.Retunes;
                lastCenter = center;
            }
            else if (released)
            {
                moves.Add(new Move(at, seconds, "hold-dropped", lastCenter, heldPrev, holdFrom, readingBefore));
            }

            if (double.IsNaN(heldPrev) && !double.IsNaN(held))
            {
                holdFrom = at;
            }

            heldPrev = held;

            if ((int)HopsSinceSurveyField.GetValue(tracker)! == 99)
            {
                readingBefore = (double)ReadingDbField.GetValue(tracker)!;
            }
            else if ((int)HopsSinceSurveyField.GetValue(tracker)! == 0 && tracker.Verdict.Keyed is { } k)
            {
                var survey = (CwToneSurvey)SurveyField.GetValue(tracker)!;

                confirms.Add((seconds, $"{seconds:0.000} s | keyed {k.ToneHz:0} at {k.KeyedDb:0.0} dB | reading level before "
                    + $"{readingBefore:0.0}, after {(double)ReadingDbField.GetValue(tracker)!:0.0} dB | sender bins "
                    + $"625 keyed {KeyedLevel(survey, 625):0.0}, 650 keyed {KeyedLevel(survey, 650):0.0} dB "
                    + $"| bank {center:0.0}"));
            }
        }));

        decoder.CharacterSettled += settled.Add;

        if (feed == "hop")
        {
            var hop = tracker.HopSamples;

            for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
            {
                decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
            }
        }
        else if (feed == "quarter")
        {
            var chunk = audio.SampleRate / 4;

            for (var at = 0; at < audio.Samples.Length; at += chunk)
            {
                var take = Math.Min(chunk, audio.Samples.Length - at);
                decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan(at, take)));
            }
        }
        else
        {
            using var source = new BufferedAudioSource(audio);
            decoder.Listen(source);
            source.PumpAll();
        }

        if (padSamples > 0)
        {
            var silence = new float[padSamples];
            var chunk = BufferedAudioSource.DefaultChunkSamples;

            for (var at = 0; at < padSamples; at += chunk)
            {
                var take = Math.Min(chunk, padSamples - at);
                decoder.Process(new AudioChunk(audio.Samples.Length + at, audio.SampleRate, silence.AsSpan(at, take)));
            }
        }

        if (flush)
        {
            decoder.Flush();
        }

        return new Trace(hops, moves, settled, decoder, confirms);
    }

    /// <summary>One row per move, with the property.</summary>
    private void PrintMoves(string label, Trace trace, Func<double, double> senderAt, string role)
    {
        foreach (var m in trace.Moves)
        {
            var (marks, keyUps, selfMarks, selfKeyUps, lift) = Keying(trace.Hops, m.SpanFrom, m.AtHop + 1);
            var (lastMarks, lastKeyUps, _, _, _) = Keying(trace.Hops, m.AtHop - 99, m.AtHop + 1);
            var sender = senderAt(m.Seconds);
            var way = double.IsNaN(sender)
                ? "sender unknown"
                : Math.Abs(m.ToHz - sender) > Math.Abs(m.FromHz - sender) + 1
                    ? "AWAY"
                    : Math.Abs(m.ToHz - sender) + 1 < Math.Abs(m.FromHz - sender) ? "TOWARD" : "level";
            var span = (m.AtHop + 1 - Math.Max(0, m.SpanFrom)) * 0.005;

            _output.WriteLine($"move | {label} | {role} | {m.Seconds:0.000} s | {m.Caller} | {m.FromHz:0.0} to {m.ToHz:0.0} Hz "
                + $"| sender {sender:0} | {way} | span {span:0.00} s from {trace.Hops[Math.Clamp(m.SpanFrom, 0, trace.Hops.Count - 1)].Seconds:0.000} "
                + $"| centre marks {marks} key-ups {keyUps} mean lift {lift:0.0} dB -> STILL KEYING {(keyUps >= 1 ? "yes" : "no")} "
                + $"| last 0.5 s marks {lastMarks} key-ups {lastKeyUps} | survey rule marks {selfMarks} key-ups {selfKeyUps}");
        }
    }

    private static string Text(IEnumerable<CwCharacter> characters)
        => string.Concat(characters.Select(c => c.IsUnreadable ? "*" : c.Text));

    /// <summary>#6 and #15, every seed, as their test builds them.</summary>
    /// <param name="wordsPerMinute">The sender.</param>
    /// <param name="prefix">The run-up, or none.</param>
    [Theory]
    [InlineData(25, "")]
    [InlineData(12, "VVV ")]
    public void TheAcquisitionReds(int wordsPerMinute, string prefix)
    {
        const string call = "CQ CQ DE N0CALL N0CALL K";
        var red = wordsPerMinute == 25 ? "#6" : "#15";

        foreach (var seed in new[] { 7919, 104729, 15485863 })
        {
            var label = $"{red} seed {seed}";
            var audio = CwSignal.Generate(new CwSignalRequest(
                prefix + call, WordsPerMinute: wordsPerMinute, ToneHz: 640, Amplitude: 0.5,
                NoiseAmplitude: CwSensitivity.NoiseFor(18.0), Seed: seed));
            var trace = Run(audio, CwSignal.DefaultToneHz, "pump");

            PrintMoves(label, trace, _ => 640, red == "#6" ? "gate, held green" : "red, single sender");

            var share = (double)CwAlignment.Align(trace.Settled, prefix + call).Count(m =>
                m.Kind == CwMatchKind.Correct && !m.Decoded.IsWordGap && m.Expected != "V")
                / call.Count(c => c != ' ');

            _output.WriteLine($"sum | {label} | share {share:0.00} | settled '{Text(trace.Settled)}'");

            if (red == "#15" && seed == 104729)
            {
                foreach (var c in trace.Confirms.Where(c => c.Seconds is >= 17.5 and <= 21.5))
                {
                    _output.WriteLine($"confirm | {label} | {c.Row}");
                }
            }
        }
    }

    /// <summary>The easy tier behind #43 to #45, as their test feeds them.</summary>
    /// <param name="name">The fixture.</param>
    [Theory]
    [InlineData("coverage-easy")]
    [InlineData("exchange-easy")]
    [InlineData("tightfist-easy")]
    public void TheEasyTier(string name)
    {
        var recipe = CwFixtureCatalogue.All.Single(r => r.Name == name);
        var audio = WavAudio.Read(Path.Combine(CwFixtureCatalogue.Folder, name + ".wav"));
        var trace = Run(audio, 600, "pump");

        PrintMoves(name, trace, _ => recipe.ToneHz, "red, single sender");
        _output.WriteLine($"sum | {name} | sender {recipe.ToneHz:0} | settled '{Text(trace.Settled)}'");
    }

    /// <summary>The real handovers: the two-station fixture, and #41 fed its quarter seconds.</summary>
    [Fact]
    public void TheHandovers()
    {
        const string name = CwFixtureCatalogue.TwoStationName;
        var audio = WavAudio.Read(Path.Combine(CwFixtureCatalogue.Folder, name + ".wav"));

        // The sidecar's handover, 23.09 s: the caller at 615 before it, the answerer at 730 after.
        static double Sender(double s) => s < 23.09 ? 615 : 730;

        PrintMoves("two-station", Run(audio, 600, "pump"), Sender, "known-right, handover");
        PrintMoves("#41 two-station quarter", Run(audio, 600, "quarter"), Sender, "known-right, handover");
    }

    /// <summary>Every floor capture, fed as the floors feed them.</summary>
    [Fact]
    public void TheCaptures()
    {
        var costH1 = new[] { "003016", "031838", "031905", "032113", "032129" };

        foreach (var row in TheCapturesThatDecodeKeepDecodingTests.Floors)
        {
            var name = (string)row[0];
            var audio = WavAudio.Read(Path.Combine(CapturedSignalTests.Folder, name + ".wav"));
            var trace = Run(audio, 600, "hop");
            var final = trace.Decoder.Report.ToneHz;
            var role = costH1.Any(c => name.EndsWith(c, StringComparison.Ordinal))
                ? "known-right, a row H1 cost"
                : "capture";

            PrintMoves(name, trace, _ => final, role);
            _output.WriteLine($"cap | {name} | characters {trace.Decoder.Report.CharactersEmitted} | final tone {final:0.0} "
                + $"| moves {trace.Moves.Count(m => m.Caller != "hold-dropped")}");
        }
    }

    /// <summary>
    /// The easy tier's tail: the file as it is, then with the air's trailing
    /// silence before the flush.
    /// </summary>
    /// <param name="name">The fixture.</param>
    [Theory]
    [InlineData("tightfist-easy")]
    [InlineData("coverage-easy")]
    [InlineData("exchange-easy")]
    public void TheTail(string name)
    {
        var recipe = CwFixtureCatalogue.All.Single(r => r.Name == name);
        var audio = WavAudio.Read(Path.Combine(CwFixtureCatalogue.Folder, name + ".wav"));
        var delay = CwProbabilisticStream.DecisionDelaySeconds;
        var pad = (int)Math.Round((delay + 1.0) * audio.SampleRate);
        var length = audio.Samples.Length / (double)audio.SampleRate;

        _output.WriteLine($"tail | {name} | file {length:0.000} s | settle delay DecisionDelaySeconds {delay:0.0} s, "
            + $"reads every {CwProbabilisticStream.ReadEverySeconds:0.0} s | padding {pad} samples of digital zero, "
            + $"{pad / (double)audio.SampleRate:0.000} s");

        var sent = recipe.Text.Replace("^", "", StringComparison.Ordinal).Replace(" ", "", StringComparison.Ordinal).ToUpperInvariant();
        var expected = recipe.Text
            .Replace(CwFixtureCatalogue.RunUp, "", StringComparison.Ordinal)
            .Replace("^", "", StringComparison.Ordinal)
            .Replace(" ", "", StringComparison.Ordinal)
            .ToUpperInvariant();

        // The key state at the bank's centre over the last three seconds of the file.
        var bare = Run(audio, 600, "pump", 0, flush: false);
        var hops = bare.Hops;
        var lift = hops.Select(h => h.CenterDb - h.NoiseDb).Select(v => double.IsNaN(v) ? -99 : v).ToList();
        var state = KeyState(lift, CwToneSurvey.InterferenceLiftDb, CwToneSurvey.InterferenceLiftDb - (2 * HysteresisDb));
        var marks = new List<string>();

        for (var i = 0; i < hops.Count; i++)
        {
            if (hops[i].Seconds < length - 3 || !state[i] || (i > 0 && state[i - 1]))
            {
                continue;
            }

            var end = i;

            while (end < hops.Count && state[end])
            {
                end++;
            }

            marks.Add($"{hops[i].Seconds:0.000}+{(end - i) * 5}ms");
        }

        _output.WriteLine($"tail | {name} | last 3 s at the centre {hops[^1].CenterHz:0.0} Hz, marks: {string.Join(" ", marks)}");
        _output.WriteLine($"tail | {name} | settled before any flush '{Text(bare.Settled)}' | last settled At "
            + $"{(bare.Settled.Count > 0 ? bare.Settled[^1].At.TotalSeconds : double.NaN):0.000} s");

        foreach (var (label, padding) in new[] { ("as the file ends", 0), ("with the air's trailing silence", pad) })
        {
            var trace = Run(audio, 600, "pump", padding);
            var letters = trace.Settled.Where(c => !c.IsWordGap).ToList();
            var placeholders = letters.Count(c => c.IsUnreadable);
            var strangers = letters
                .Where(c => !c.IsUnreadable && c.Text.Length == 1)
                .Where(c => !sent.Contains(c.Text[0], StringComparison.Ordinal))
                .Select(c => c.Text)
                .ToList();
            var got = string.Concat(letters.Select(c => c.Text)).Replace("<", "", StringComparison.Ordinal).Replace(">", "", StringComparison.Ordinal);

            _output.WriteLine($"tail | {name} | {label} | settled {placeholders} + {strangers.Count} "
                + $"({string.Join(",", strangers)}) | ends with expected {got.EndsWith(expected, StringComparison.Ordinal)} "
                + $"| own transmit {trace.Decoder.Report.OwnTransmitSeconds:0.00} s | suspended chunks {trace.Decoder.SuspendedChunks}");
            _output.WriteLine($"tail | {name} | {label} | got '{got}' | wanted '{expected}'");

            foreach (var c in letters.Where(c => c.At.TotalSeconds >= length - 3))
            {
                _output.WriteLine($"tail-char | {name} | {label} | '{c.Text}' pattern '{c.Pattern}' At {c.At.TotalSeconds:0.000} s "
                    + $"| {c.Confidence} | score {c.Score:0.00} | {c.WordsPerMinute} wpm");
            }
        }
    }
}
