using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Scan;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE DAH THAT COMES APART** (work instruction 566, HM-DEC-270): on 2026-10-08, AGC on SLOW, W1AW read live almost whole,
/// and fifteen of its twenty-five wrong were one fault, `INFMERMATION` for INFORMATION: a dah came apart, one piece read as a
/// dit and the gap beside it as a letter gap. These draw each specimen in the tree through the sender's own window.
/// </summary>
/// <remarks>R88 is lifted for the four W1AW pieces of 2026-10-07 and the three scan catches; tonight's capture is not in the tree.</remarks>
public sealed class TheDahThatComesApartTests(ITestOutputHelper output)
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>One specimen: where to look, the letters printed and the letter meant.</summary>
    internal sealed record Specimen(string Recording, double From, double To, string Printed, string Meant, string Note);

    /// <summary>The W1AW table's wrong letters at HEAD that are kin to the signature: a dah lost from a letter.</summary>
    internal static readonly Specimen[] W1aw =
    [
        new("w1aw-2026-10-07/piece-04", 120.9, 148.5, "LMW", "LOW", "O read as M"),
        new("w1aw-2026-10-07/piece-04", 248.3, 272.3, "TINK", "TINY", "Y read as K"),
        new("w1aw-2026-10-07/piece-04", 248.3, 272.3, "ULTRE", "ULTRA", "A read as E"),
    ];

    /// <summary>One hop of the sender's own window: audio time, its height between key-up (0) and key-down (1), and the dit.</summary>
    internal readonly record struct Hop(double At, double Height, double Dit);

    /// <summary>One printed mark: its span, its kind and the index of the letter it is in.</summary>
    internal readonly record struct Mark(double From, double To, char Kind, int Letter);

    /// <summary>A reading of a recording: the window every hop, the printed marks, and the letters with their text.</summary>
    internal sealed record Reading(List<Hop> Window, List<Mark> Marks, List<(int Index, string Text, double From, double To)> Letters);

    /// <summary>Reads a recording through the app's chain with the sender's own window traced.</summary>
    internal static Reading Read(float[] samples, int rate, double pitchHz, double widthHz)
    {
        using var chain = new CwChain(rate);
        var gate = chain.Decoder.Runs;
        var trace = new List<(double Seconds, double LaneDb, double SenderDb, double DitSeconds, double DelaySeconds)>();
        var keyUp = new List<(double Seconds, double? KeyUpDb)>();
        var marks = new List<Mark>();
        var letters = new List<(int, string, double, double)>();
        var letter = 0;

        chain.Detector.LaneTrace = trace;
        chain.Detector.LaneKeyUpTrace = keyUp;
        chain.Detector.SetPassband(pitchHz, widthHz);
        gate.RunRead += (c, run) =>
        {
            var split = gate.StationSplitSeconds;

            foreach (var m in run)
            {
                marks.Add(new Mark(m.FromSeconds, m.ToSeconds, m.ToSeconds - m.FromSeconds >= split ? '-' : '.', letter));
            }

            letters.Add((letter, c.Text, run[0].FromSeconds, run[^1].ToSeconds));
            letter++;
        };

        var chunk = rate / 100;

        for (var at = 0; at + chunk <= samples.Length; at += chunk)
        {
            chain.Process(new AudioChunk(at, rate, samples.AsSpan(at, chunk)));
        }

        chain.Decoder.Flush();

        var window = trace.Zip(keyUp, (t, k) => new Hop(
            t.Seconds - t.DelaySeconds,
            k.KeyUpDb is { } up && t.SenderDb - up > 0 ? (t.LaneDb - up) / (t.SenderDb - up) : double.NaN,
            t.DitSeconds)).ToList();

        return new Reading(window, marks, letters);
    }

    /// <summary>The window between two times, a character a hop: `.` under 0.4 of the contrast, `#` from 0.4, `+` from 0.6.</summary>
    internal static string Picture(Reading r, double from, double to)
        => string.Concat(r.Window.Where(h => h.At >= from && h.At <= to).Select(h => !double.IsFinite(h.Height) ? '?' : h.Height >= 0.6 ? '+' : h.Height >= 0.4 ? '#' : '.'));

    /// <summary>The sender's figures over the thirty seconds before a time, from his printed marks.</summary>
    internal static (double InsideP10, double InsideCentre, double InsideTop, double LetterBottom, double DahCentre, double Dit) Clusters(Reading r, double at)
    {
        var recent = r.Marks.Where(m => m.To <= at && m.To > at - 30).OrderBy(m => m.From).ToList();
        var inside = new List<double>();
        var between = new List<double>();

        for (var i = 1; i < recent.Count; i++)
        {
            var gap = recent[i].From - recent[i - 1].To;

            (recent[i].Letter == recent[i - 1].Letter ? inside : between).Add(gap);
        }

        var dahs = recent.Where(m => m.Kind == '-').Select(m => m.To - m.From).Order().ToList();
        var dits = recent.Where(m => m.Kind == '.').Select(m => m.To - m.From).Order().ToList();
        inside.Sort();
        between.Sort();

        double P(List<double> x, double p) => x.Count == 0 ? double.NaN : x[(int)Math.Min(x.Count - 1, Math.Floor(p * x.Count))];

        return (P(inside, 0.1), P(inside, 0.5), P(inside, 0.9), P(between, 0.1), P(dahs, 0.5), P(dits, 0.5));
    }

    /// <summary>One specimen's row: the window, the printed pieces, the gaps between them, and the sender's figures.</summary>
    internal static string Row(Reading r, string label, double from, double to)
    {
        var pieces = r.Marks.Where(m => m.To > from && m.From < to).OrderBy(m => m.From).ToList();
        var c = Clusters(r, from);
        var sb = new StringBuilder();

        sb.Append(string.Create(Inv, $"| {label} | `{Picture(r, from, to)}` | "));
        sb.Append(string.Join(" ", pieces.Select(p => string.Create(Inv, $"{(p.To - p.From) * 1000:0}{p.Kind}"))));
        sb.Append(" | ");
        sb.Append(string.Join(" ", pieces.Skip(1).Select((p, i) => string.Create(Inv, $"{(p.From - pieces[i].To) * 1000:0}{(p.Letter == pieces[i].Letter ? "i" : "L")}"))));
        sb.Append(string.Create(Inv, $" | {c.InsideP10 * 1000:0} / {c.InsideCentre * 1000:0} / {c.InsideTop * 1000:0} | {c.LetterBottom * 1000:0} | {c.DahCentre * 1000:0} | {c.Dit * 1000:0} | {(c.Dit > 0 ? 1.2 / c.Dit : double.NaN):0} |"));

        return sb.ToString();
    }

    private static string Header =>
        "| specimen | window, a hop a character | pieces printed, ms | gaps, ms (i inside, L letter) | his inside gaps p10 / centre / p90 | letter gap p10 | dah centre | dit | WPM |"
        + Environment.NewLine + "|---|---|---|---|---|---|---|---|---|";

    /// <summary>What the synthetic sender sends: a CQ and a few words with plenty of dahs in them.</summary>
    internal const string SyntheticText = "CQ CQ DE W1AW INFORMATION FROM QST USING VARIOUS MODES K";

    /// <summary>
    /// A synthetic sender at 18 WPM, 16 dB over the noise at 600 Hz, every fifth dah broken: with a 40 ms dip to 0.3 of the
    /// contrast in its middle (<paramref name="frontLost"/> false), or with its first 60 ms not keyed at all (true).
    /// </summary>
    internal static float[] BrokenDahs(int seed, bool frontLost, int rate = 8000)
    {
        const double Wpm = 18;
        const double Db = 16;
        var dit = 1.2 / Wpm;
        var keyed = new List<(double Seconds, double Amplitude)> { (2.0, 0) };
        var morse = new Dictionary<char, string>
        {
            ['A'] = ".-", ['C'] = "-.-.", ['D'] = "-..", ['E'] = ".", ['F'] = "..-.", ['I'] = "..", ['K'] = "-.-", ['M'] = "--",
            ['N'] = "-.", ['O'] = "---", ['Q'] = "--.-", ['R'] = ".-.", ['S'] = "...", ['T'] = "-", ['U'] = "..-", ['V'] = "...-",
            ['W'] = ".--", ['G'] = "--.", ['1'] = ".----",
        };
        var dahs = 0;
        var words = SyntheticText.Split(' ');

        for (var w = 0; w < words.Length; w++)
        {
            for (var c = 0; c < words[w].Length; c++)
            {
                var code = morse[words[w][c]];

                for (var e = 0; e < code.Length; e++)
                {
                    if (code[e] == '.')
                    {
                        keyed.Add((dit, 1));
                    }
                    else if (++dahs % 5 != 0)
                    {
                        keyed.Add((3 * dit, 1));
                    }
                    else if (frontLost)
                    {
                        keyed.Add((0.060, 0));
                        keyed.Add(((3 * dit) - 0.060, 1));
                    }
                    else
                    {
                        var dip = 0.040;
                        var each = ((3 * dit) - dip) / 2;
                        var dipAmplitude = Math.Pow(10, -0.7 * Db / 20);

                        keyed.Add((each, 1));
                        keyed.Add((dip, dipAmplitude));
                        keyed.Add((each, 1));
                    }

                    if (e + 1 < code.Length)
                    {
                        keyed.Add((dit, 0));
                    }
                }

                keyed.Add((c + 1 < words[w].Length ? 3 * dit : 7 * dit, 0));
            }
        }

        keyed.Add((2.0, 0));

        var samples = new float[(int)(keyed.Sum(k => k.Seconds) * rate) + rate];
        var amplitude = ThePatternIsTheGateTests.Over(Db);
        var at = 0;

        foreach (var (seconds, level) in keyed)
        {
            var n = (int)Math.Round(seconds * rate);

            for (var i = 0; i < n && level > 0; i++)
            {
                samples[at + i] = (float)(amplitude * level * Math.Sin(2 * Math.PI * 600 * (at + i) / rate));
            }

            at += n;
        }

        var noise = new Random(seed);

        for (var i = 0; i < samples.Length; i++)
        {
            var u1 = 1.0 - noise.NextDouble();
            var u2 = noise.NextDouble();

            samples[i] += (float)(0.04 * Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2));
        }

        return samples;
    }

    /// <remarks>Task 2's synthetic senders read through the app's chain at three seeds: letters right of the text. Asserts nothing.</remarks>
    [Fact]
    public void TheSyntheticBrokenDahs()
    {
        foreach (var frontLost in new[] { false, true })
        {
            foreach (var seed in new[] { 5661, 5662, 5663 })
            {
                var text = TheRecordingsScoreboardTests.ReadLive(BrokenDahs(seed, frontLost), 8000).Text;
                var right = TheRecordingsScoreboardTests.Right(text, SyntheticText);

                output.WriteLine($"{(frontLost ? "front lost" : "dip")} seed {seed}: {right} of {TheRecordingsScoreboardTests.Letters(SyntheticText).Length} `{text}`{Extra(seed, frontLost)}");
            }
        }
    }

    // Filled in only while a candidate rule is measured with it off and on.
    private static string Extra(int seed, bool frontLost) => string.Empty;

    /// <remarks>
    /// Task 1, steps 1 to 3: every specimen in the tree, through the sender's own window. The W1AW kin are found by the letters
    /// printed inside their stretch; the catch's nine at the times already named. Asserts nothing.
    /// </remarks>
    [Fact]
    public void EachSpecimenThroughTheSendersWindow()
    {
        output.WriteLine(Header);

        foreach (var group in W1aw.GroupBy(s => s.Recording))
        {
            var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(group.Key));
            var (pitch, width) = TheRecordingsScoreboardTests.RadioState(group.Key);
            var r = Read(audio.Samples, audio.SampleRate, pitch, width);

            foreach (var s in group)
            {
                var inStretch = r.Letters.Where(l => l.To > s.From && l.From < s.To).ToList();
                var text = string.Concat(inStretch.Select(l => l.Text));
                var at = text.IndexOf(s.Printed, StringComparison.Ordinal);

                if (at < 0)
                {
                    output.WriteLine($"| {s.Note} | `{s.Printed}` not found in `{text}` | | | | | | | |");
                    continue;
                }

                // The letters printed, their text joined, may span several entries; find the entries that make the match.
                var chars = 0;
                var first = -1;
                var last = -1;

                for (var i = 0; i < inStretch.Count; i++)
                {
                    if (chars + inStretch[i].Text.Length > at && first < 0)
                    {
                        first = i;
                    }

                    chars += inStretch[i].Text.Length;

                    if (chars >= at + s.Printed.Length)
                    {
                        last = i;
                        break;
                    }
                }

                output.WriteLine(Row(r, string.Create(Inv, $"W1AW {group.Key[^8..]} {inStretch[first].From:0.00} s, `{s.Printed}` for `{s.Meant}` ({s.Note})"), inStretch[first].From - 0.03, inStretch[last].To + 0.03));
            }
        }

        var main = WavAudio.Read(TheStrongStationsOverTests.Wav(TheStrongStationsOverTests.Main));
        var catchRead = Read(main.Samples, main.SampleRate, 600, 500);

        foreach (var t in TheBrokenDahsTests.Times)
        {
            output.WriteLine(Row(catchRead, string.Create(Inv, $"catch-153810 {t:0.00} s"), t - 0.06, t + 0.24));
        }
    }
}
