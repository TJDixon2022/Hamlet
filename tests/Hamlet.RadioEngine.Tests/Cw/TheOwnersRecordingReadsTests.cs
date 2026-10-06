using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE OWNER'S RECORDING READS** (work instruction 528, HM-DEC-232): thirty seconds of a real QSO on 7.0549 MHz at
/// 20:01 UTC on 2026-10-02, which Hamlet read as `FER CHET&lt;BT&gt; BESE7V E ■ &lt;SK&gt; KC4 Z GP DEWA`.
/// </summary>
/// <remarks>
/// <para>**THE ONE TEST THAT READS A RECORDING.** R88 bans reading recordings; the owner lifted it for this one file,
/// 2026-10-02: he made the recording for this and said yes. No other recording is read.</para>
/// <para>Read through the live path as the app wires it - the envelope detector, its pattern gate and the run reader -
/// at the radio's state on the sheet: CW, FIL2 500 Hz, pitch 600, AGC FAST.</para>
/// </remarks>
public sealed class TheOwnersRecordingReadsTests
{
    // The reader's senders and their shapes before the last flush, for the report.
    private static string LastSenders = string.Empty;

    // **THE SENDER'S TIMING, NOT THE WORD** (work instruction 532): he paused 579 ms between the C and the H of CHAT, about
    // 7.4 of his element gaps, longer than a word gap by Morse's 1:3:7 and longer than he leaves between some of his
    // words. The element list showed it (-.-. [580] ....); CHAT was written because the word was known. Timing cannot know it.
    private const string Sent = "FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the reading and its marks are printed.</param>
    public TheOwnersRecordingReadsTests(ITestOutputHelper output) => _output = output;

    internal static string Wav(string name = "cw-2026-10-02-200157", [System.Runtime.CompilerServices.CallerFilePath] string here = "")
        => Path.Combine(Path.GetDirectoryName(here)!, "..", "..", "fixtures", "cw", "captured", name + ".wav");

    /// <summary>The recording read as the app reads it: the text, the characters, and every mark that stood.</summary>
    internal static (string Text, IReadOnlyList<CwCharacter> Characters, IReadOnlyList<CwMark> Marks, IReadOnlyList<(double At, double Line, string Clusters)> Lines, IReadOnlyList<CwMark> Candidates) Read(string name = "cw-2026-10-02-200157")
    {
        var audio = WavAudio.Read(Wav(name));
        // **THE APP'S OWN CHAIN** (work instruction 542, HM-DEC-246), wired as the app wires it.
        using var chain = new CwChain(audio.SampleRate);
        var detector = chain.Detector;

        detector.SetPassband(600, 500);

        var reader = chain.Decoder.Runs;
        var characters = new List<CwCharacter>();
        var chunk = audio.SampleRate / 100;

        var lines = new List<(double At, double Line, string Clusters)>();

        reader.CharacterRead += c =>
        {
            characters.Add(c);
            lines.Add((c.At.TotalSeconds, reader.StationWordLineSeconds, $"letter line {reader.StationLines?.CharacterSeconds * 1000:0} ms, gap dit {reader.StationLines?.GapDitSeconds * 1000:0} ms, inside gaps {Ms(reader.StationLines?.InsideGaps)}, letter gaps {Ms(reader.StationLines?.LetterGaps)}, word gaps {Ms(reader.StationLines?.WordGaps)}"));
        };

        for (var at = 0; at + chunk <= audio.Samples.Length; at += chunk)
        {
            chain.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan(at, chunk)));
        }

        var shapes = string.Join("; ", reader.SenderShapes.Select(s => $"{s.PitchHz:0} Hz {s.Marks} marks{(s.Printed ? " printed" : string.Empty)}: {s.Shape}"));

        chain.Decoder.Flush();
        var own = detector.OwnWindowLane;

        LastSenders = $"{shapes}; own window at {own.PitchHz:0.0} Hz, dit {own.DitSeconds * 1000:0} ms, cutoff {own.CutoffHz:0.0} Hz, rise {own.RiseSeconds * 1000:0.0} ms, delay {own.DelaySeconds * 1000:0.0} ms";

        var text = string.Join(' ', string.Concat(characters.Select(c => c.Text)).Split(' ', StringSplitOptions.RemoveEmptyEntries));

        return (text, characters, detector.MarksSince(0).Marks, lines, detector.CandidatesKept.ToList());
    }

    private static string Ms(IReadOnlyList<double>? gaps)
        => gaps is null ? "none" : "[" + string.Join(", ", gaps.Select(g => (g * 1000).ToString("0", System.Globalization.CultureInfo.InvariantCulture))) + "] ms";

    /// <remarks>
    /// The letters read as sent, no space inside KC4ZGP, and the whole text with its spaces (work instruction 531). The text, the characters and every mark
    /// that stood are printed, and the sender's word line with its two clusters at each letter (task 3).
    /// </remarks>
    [Fact]
    public void TheOwnersRecordingReads()
    {
        var (text, characters, marks, lines, candidates) = Read();

        _output.WriteLine($"sent `FER CHAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA`, spaced by its timing `{Sent}`");
        _output.WriteLine($"read `{text}`");
        _output.WriteLine($"senders before the end: {LastSenders}");
        _output.WriteLine("characters: " + string.Concat(characters.Select(c => $"{c.Text}[{c.Pattern}]@{c.At.TotalSeconds:0.00} ")));

        CwMark? last = null;

        foreach (var m in marks.OrderBy(m => m.FromSeconds))
        {
            var gap = last is { } l ? (m.FromSeconds - l.ToSeconds) * 1000 : double.NaN;

            _output.WriteLine($"  mark {m.FromSeconds:0.000} s, {(m.ToSeconds - m.FromSeconds) * 1000:0} ms, gap before {gap:0} ms, {m.PitchHz:0} Hz, level {m.LevelDb:0.0} dB{(m.Fitted ? ", fitted" : string.Empty)}");
            last = m;
        }

        // Work instruction 529, task 1: the candidates the single-mark gates passed for the 7 after the pause.
        var seven = candidates.Where(c => c.FromSeconds >= 14.9 && c.ToSeconds <= 17.0).OrderBy(c => c.FromSeconds).ToList();

        _output.WriteLine($"candidates 14.9 to 17.0 s: {seven.Count}");

        foreach (var c in seven)
        {
            _output.WriteLine($"  candidate {c.FromSeconds:0.000} s, {(c.ToSeconds - c.FromSeconds) * 1000:0} ms, {c.PitchHz:0.0} Hz, level {c.LevelDb:0.0} dB{(c.Stood ? string.Empty : ", not stood")}");
        }

        // Task 3: the sender's own word line at each printed letter, with its letter and word cluster centres.
        foreach (var (at, line, clusters) in lines)
        {
            _output.WriteLine($"  at {at:0.00} s: word line {line * 1000:0} ms; {clusters}");
        }

        var final = lines[^1].Line * 1000;

        _output.WriteLine($"the word line for this sender: {final:0} ms");

        Assert.Equal(Sent.Replace(" ", string.Empty, StringComparison.Ordinal), text.Replace(" ", string.Empty, StringComparison.Ordinal));
        Assert.Contains("KC4ZGP", text, StringComparison.Ordinal);
        Assert.InRange(final, 430, 580);

        // **AND ITS SPACING** (work instructions 531 and 532): the whole text, spaces included, as the sender's timing spaced it.
        Assert.Equal(Sent, text);
    }

    /// <remarks>
    /// <para>**THE TOP OF ONE DAH THROUGH TWO WINDOWS** (work instruction 529, task 1, HM-DEC-233): the T of BEST, 11.48 to
    /// 11.69 s, read hop by hop through the per-bin path's ten millisecond Hann window at the 650 and 675 Hz bins, and
    /// through <see cref="CwSenderLane"/> tuned to the dah's own pitch with a cutoff from the sender's dit. Its top is
    /// taken 20 ms in from either edge, and the lane's hops are taken back by its delay.</para>
    /// <para>The pitch is measured here from the dah's own samples, the strongest of a 0.5 Hz scan from 600 to 725 Hz;
    /// the dit is the shortest mark the reading found. Nothing is asserted: it is the measurement the unit starts from.</para>
    /// </remarks>
    [Fact]
    public void TheTopOfADahThroughTwoWindows()
    {
        var audio = WavAudio.Read(Wav());
        var rate = audio.SampleRate;
        var x = audio.Samples;
        var hop = rate / 200;
        var window = 2 * hop;

        double Goertzel(double hz, int from, int n, bool hann)
        {
            var c = 2 * Math.Cos(2 * Math.PI * hz / rate);
            double s1 = 0, s2 = 0, sum = 0;

            for (var i = 0; i < n; i++)
            {
                var w = hann ? 0.5 - (0.5 * Math.Cos(2 * Math.PI * (i + 0.5) / n)) : 1;
                var s0 = (x[from + i] * w) + (c * s1) - s2;

                s2 = s1;
                s1 = s0;
                sum += w;
            }

            return 2 * ((s1 * s1) + (s2 * s2) - (c * s1 * s2)) / (sum * sum);
        }

        var marks = Read().Marks;
        var dit = marks.Select(m => m.ToSeconds - m.FromSeconds).Where(l => l > 0.05).Min();

        double Scan(double from, double to)
            => Enumerable.Range(0, 251).Select(k => 600 + (k * 0.5)).MaxBy(hz => Goertzel(hz, (int)(from * rate), (int)((to - from) * rate), false));

        // The lane's level each hop, at its time on the audio: the sample it came out at, taken back by its delay.
        List<(double At, double Db)> Lane(double pitchHz, out CwSenderLane lane)
        {
            lane = new CwSenderLane(rate);
            lane.Tune(pitchHz, dit);

            var levels = new List<(double, double)>();

            for (var i = 0; i < (int)(12.2 * rate); i++)
            {
                lane.Push(x[i]);

                if (i >= (int)(11.0 * rate) && i % hop == 0)
                {
                    levels.Add(((i / (double)rate) - lane.DelaySeconds, lane.LevelDb));
                }
            }

            return levels;
        }

        // The dah: the loudest hop between 11.3 and 11.95 s on the lane, and out from it while within half amplitude.
        (double From, double To) Edges(List<(double At, double Db)> levels)
        {
            var peak = levels.Select((l, k) => (l, k)).Where(p => p.l.At > 11.3 && p.l.At < 11.95).MaxBy(p => p.l.Db).k;
            var line = levels[peak].Db - 6.02;
            var a = peak;
            var b = peak;

            while (a > 0 && levels[a - 1].Db >= line)
            {
                a--;
            }

            while (b < levels.Count - 1 && levels[b + 1].Db >= line)
            {
                b++;
            }

            return (levels[a].At, levels[b].At);
        }

        var rough = Scan(11.3, 11.95);
        var first = Edges(Lane(rough, out _));
        var pitch = Scan(first.From, first.To);
        var laneLevels = Lane(pitch, out var lane);
        var (dahFrom, dahTo) = Edges(laneLevels);
        var topFrom = dahFrom + 0.02;
        var topTo = dahTo - 0.02;

        (double Sd, double Range, int Hops) Top(IEnumerable<double> levels)
        {
            var l = levels.ToList();
            var mean = l.Average();

            return (Math.Sqrt(l.Average(v => (v - mean) * (v - mean))), l.Max() - l.Min(), l.Count);
        }

        // A bin's level is its ten millisecond window; the window's middle is its time.
        IEnumerable<double> Bin(double hz)
        {
            for (var middle = (int)(topFrom * rate); middle <= (int)(topTo * rate); middle += hop)
            {
                yield return 10 * Math.Log10(Goertzel(hz, middle - (window / 2), window, true) + 1e-20);
            }
        }

        var b650 = Top(Bin(650));
        var b675 = Top(Bin(675));
        var own = Top(laneLevels.Where(l => l.At >= topFrom && l.At <= topTo).Select(l => l.Db));

        _output.WriteLine($"the T of BEST, found on the lane from {dahFrom:0.000} to {dahTo:0.000} s ({(dahTo - dahFrom) * 1000:0} ms at half amplitude); its top {topFrom:0.000} to {topTo:0.000} s; its pitch {pitch:0.0} Hz; the sender's dit {dit * 1000:0} ms");
        _output.WriteLine($"  650 Hz bin, 10 ms Hann:   sd {b650.Sd:0.00} dB, range {b650.Range:0.00} dB over {b650.Hops} hops");
        _output.WriteLine($"  675 Hz bin, 10 ms Hann:   sd {b675.Sd:0.00} dB, range {b675.Range:0.00} dB over {b675.Hops} hops");
        _output.WriteLine($"  own window, {lane.CutoffHz:0.0} Hz cutoff: sd {own.Sd:0.00} dB, range {own.Range:0.00} dB over {own.Hops} hops; rise {lane.RiseSeconds * 1000:0.0} ms, delay {lane.DelaySeconds * 1000:0.0} ms");
    }
}
