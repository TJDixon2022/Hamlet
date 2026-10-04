using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE OWNER'S RECORDINGS ARE THE SCOREBOARD** (work instruction 534, HM-DEC-238). The owner, 2026-10-03: *"Right now we
/// suck."* And: *"I want to run it against all the recordings that we've done over the last two days."*
/// </summary>
/// <remarks>
/// <para>R88 is lifted for the owner's twelve recordings named in <see cref="Stretches"/> and no other. Each is read through
/// the live path as the app wires it - the detector, the sender's window, the gate and the reader - at the radio's state
/// from its own sheet, and what printed is scored against the web session's references, letters right with spaces
/// ignored, by the scorer's edit distance with free ends.</para>
/// <para>**THE REFERENCES ARE NOT CERTAIN.** They were read offline by the web session with thresholds set by hand per
/// station. Each stretch is also read here, offline and non-causally at its pitch through a narrow low-pass, and where that
/// reading differs from the reference both element lists are printed; the reference is not changed, and the owner settles
/// it by ear. The scoreboard is a yardstick for change, not an exam: a unit is better if the total rises.</para>
/// <para>**TWO HARD LIMITS, WHATEVER THE SCORE**: loud noise, 30 s and three minutes, prints nothing; and the first
/// recording reads `FER C HAT&lt;BT&gt; BEST 7V 73 &lt;SK&gt; KC4ZGP DEWA`.</para>
/// </remarks>
public sealed class TheRecordingsScoreboardTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the scoreboard is printed.</param>
    public TheRecordingsScoreboardTests(ITestOutputHelper output) => _output = output;

    /// <summary>How sure the web session was of a reference.</summary>
    internal enum Confidence
    {
        /// <summary>No reference: reported only.</summary>
        None,

        /// <summary>Scored and reported, not totalled.</summary>
        Low,

        /// <summary>Totalled.</summary>
        Medium,

        /// <summary>Totalled.</summary>
        High,

        /// <summary>Totalled; a test already asserts it.</summary>
        Verified,
    }

    /// <summary>One station's stretch of one recording, with the web session's reference and its own lines.</summary>
    internal sealed record Stretch(
        string Recording, double PitchHz, double From, double To, string Reference, Confidence Confidence,
        double SplitMs, double LetterMs, double WordMs, string Elements);

    /// <summary>The first recording's text, a hard limit.</summary>
    internal const string FirstRecording = "FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA";

    /// <summary>Work instruction 534's section 8, as written.</summary>
    internal static readonly Stretch[] Stretches =
    [
        new("cw-2026-10-02-200157", 662.8, 0, 30, FirstRecording, Confidence.Verified, 140, 110, 560,
            "..-. [280] . [170] .-. [925] -.-. [580] .... [290] .- [390] - [430] -...- [2295] -... [270] . [255] ... [300] - [670] -- [115] ... [360] ...- [1230] --... [290] ...-- [965] ...-.- [830] -.- [330] -.-. [365] ....- [420] --.. [400] --. [285] .--. [590] -.. [195] . [215] .-- [180] .- [135]"),
        new("cw-2026-10-03-143906", 514.2, 0, 30, string.Empty, Confidence.None, 0, 0, 0, string.Empty),
        new("cw-2026-10-03-143951", 499.5, 14.5, 30, "O WAEIIEURD U AGN ES", Confidence.Low, 145, 130, 600,
            "--- [615] .-- [250] .- [325] . [590] .. [160] .. [190] . [335] ..- [355] .-. [525] -.. [815] ..- [815] .- [450] --. [385] -. [1045] . [335] ... [715]"),
        new("cw-2026-10-03-144020", 499.5, 0, 12.5, "ES OK ON PA <BT>", Confidence.Medium, 145, 130, 600,
            ". [335] ... [1010] --- [410] -.- [1070] --- [270] -. [960] .--. [330] .- [1005] -...- [1200]"),
        new("cw-2026-10-03-144020", 599.9, 9.5, 30, "WX IN NETAGIT IUN TEMP E", Confidence.Medium, 145, 130, 600,
            ".-- [300] -..- [880] .. [290] -. [990] -. [490] . [390] - [200] .- [455] --. [350] .. [420] - [1015] .. [420] ..- [250] -. [910] - [485] . [510] -- [185] .--. [1120] ."),
        new("cw-2026-10-03-144045", 599.9, 0, 30, "N TEMP 57 57<BT> BTU BOB DE KG8V K", Confidence.High, 145, 130, 800,
            "-. [910] - [485] . [510] -- [185] .--. [1120] ..... [570] --... [1075] ..... [450] --... [710] -...- [1155] -... [415] - [270] ..- [940] -... [335] --- [290] -... [1015] -.. [255] . [635] -.- [395] --. [535] ---.. [340] ...- [1120] -.- [2405]"),
        new("cw-2026-10-03-221502", 491.5, 0, 30, "ED OF ITS OWN HEE BK BK WHAT BUG AE US E ENIE EE ITS A 66 K", Confidence.Low, 110, 100, 340,
            ". [170] -.. [520] --- [115] ..-. [520] .. [160] - [120] ... [570] --- [160] .-- [200] -. [665] .... [170] . [165] . [635] -... [190] -.- [385] -... [180] -.- [1330] .-- [215] .... [160] .- [145] - [220] -... [150] ..- [120] --. [190] .- [120] . [685] ..- [265] ... [375] . [355] . [150] -. [170] .. [130] . [375] . [130] . [3230] .. [155] - [115] ... [555] .- [1115] -.... [275] -.... [620] -.-"),
        new("cw-2026-10-03-221530", 491.5, 0, 9.5, "6 CHAMPION BK", Confidence.Medium, 110, 100, 340,
            "-.... [620] -.-. [210] .... [135] .- [545] -- [175] .--. [275] .. [255] --- [175] -. [1270] -... [195] -.- [330]"),
        new("cw-2026-10-03-221530", 598.4, 9, 30, "EN FB WHEN I WAS AGE 12 I LEARNED CW USING A V", Confidence.Medium, 110, 100, 340,
            ". [205] -. [220] ..-. [155] -... [450] .-- [190] .... [180] . [195] -. [210] .. [160] .--.- [130] ... [370] .- [175] --. [125] . [255] .---- [140] ..--- [225] .. [265] .-.. [130] . [155] .- [110] .-. [195] . [185] -.. [1355] -.-. [145] .-- [440] ..- [195] ... [155] .. [130] -. [185] --. [190] .- [620] ...- [650]"),
        new("cw-2026-10-03-221548", 597.7, 0, 18.5, "2 I LEARNED CW USING A V BPLX Z EPS", Confidence.Low, 110, 100, 340,
            "[140] ..--- [225] .. [265] .-.. [130] . [150] .- [110] .-. [195] . [185] -.. [1355] -.-. [145] .-- [440] ..- [195] ... [155] .. [130] -. [185] --. [190] .- [620] ...- [655] -... [185] .--. [150] .-.. [180] -..- [695] --.. [490] . [230] .--. [180] ... [640]"),
        new("cw-2026-10-03-221548", 498.0, 18, 30, "YRHEE MY SCOUT MASTER", Confidence.Medium, 110, 100, 340,
            "-.-- [135] .-. [270] .... [200] . [185] . [660] -- [170] -.-- [280] ... [220] -.-. [160] --- [145] . [125] - [450] - [385] -- [105] .- [165] ... [185] - [150] . [170] .-. [1345]"),
        new("cw-2026-10-03-221745", 501.7, 0, 28, "E E DAND ON 40M TONITE . EUR EE H RD TOO", Confidence.Medium, 150, 125, 330,
            ". [1310] . [440] -.. [180] .- [170] -. [185] -.. [680] --- [280] -. [400] ....- [215] ----- [250] -- [390] - [225] --- [180] -. [165] .. [215] - [175] . [485] .-.-.- [910] . [230] ..- [160] .-. [175] .---.-. [135] . [210] . [930] .... [745] .-. [185] -.. [545] - [200] --- [310] --- [1665]"),
        new("cw-2026-10-03-221805", 601.3, 5, 30, "ET ON 40T S THESE DAYS . TNX FER ANOTHER FT", Confidence.Medium, 150, 125, 330,
            ". [205] - [415] --- [190] -. [375] ....- [210] ----- [290] - [660] ... [670] - [190] .... [265] . [225] ... [255] . [400] -.. [245] .- [180] -.-- [140] ... [355] .-.-.- [710] - [165] -. [170] -..- [305] ..-. [190] . [160] .-. [370] .- [205] -. [215] --- [210] - [135] .... [270] . [180] .-. [420] ..-. [205] -"),
        new("cw-2026-10-03-221828", 601.3, 0, 30, "FER ANOTHER FB QSO ES HOPE U HAVE AGN ED ES BEST", Confidence.Medium, 150, 125, 330,
            "[310] ..-. [190] . [160] .-. [370] .- [205] -. [215] --- [210] - [135] .... [270] . [180] .-. [420] ..-. [205] -... [350] --.- [185] ... [205] --- [305] . [155] ... [505] .... [225] --- [140] .--. [275] . [565] ..- [445] .... [275] .- [245] ...- [180] . [425] .- [185] --. [195] -. [550] . [215] -.. [545] . [185] ... [285] -... [220] . [195] ... [225] - [790] . [145] . [195] ...- [510] -.-. [205] .-- [175] ..--"),
        new("cw-2026-10-03-221851", 601.3, 0, 30, "BEST EEV CW 2L CQ DE NA8SB K", Confidence.Low, 150, 125, 330,
            ".. [285] -... [220] . [195] ... [225] - [790] . [145] . [195] ...- [510] -.-. [205] .-- [175] ..--- [160] .-.. [230] -.-. [200] --.- [345] -.. [250] . [485] -. [180] .- [165] ---.. [245] ... [320] -... [155] -.- [1970] . [1070] . [370] . [475] -.....- [650] . [545] ... [900] . [235] ... [960] . [350] .. [250] .-. [265] .- [225] ...- [570] ... [160] . [190] . [675]"),
    ];

    /// <summary>One letter the reader printed: when its last mark ended, its pitch, its text.</summary>
    internal readonly record struct Printed(double Seconds, double PitchHz, string Text);

    /// <summary>A stretch's score: what printed there, its letters, and how many of the reference's letters were read right.</summary>
    internal sealed record Scored(Stretch Stretch, string PrintedText, int ReferenceLetters, int Right);

    /// <summary>The whole board: each stretch, the total over the stretches of medium confidence or better, and the hard limits.</summary>
    internal sealed record Board(IReadOnlyList<Scored> Stretches, IReadOnlyDictionary<string, string> Unassigned, int Total, int OutOf, string FirstReads, IReadOnlyList<(string What, string Reads)> Noise)
    {
        /// <summary>Whether the first recording reads as it must and every noise run printed nothing.</summary>
        public bool LimitsHold => FirstReads == FirstRecording && Noise.All(n => n.Reads.Length == 0);
    }

    private static string Sheet(string recording)
        => Path.ChangeExtension(TheOwnersRecordingReadsTests.Wav(recording), ".txt");

    /// <summary>The radio's CW pitch and filter width from a recording's own sheet.</summary>
    internal static (double PitchHz, double WidthHz) RadioState(string recording)
    {
        double Field(string name)
        {
            var line = File.ReadLines(Sheet(recording)).First(l => l.StartsWith(name + " ", StringComparison.Ordinal));
            var value = line[name.Length..].Trim().Split(' ')[0];

            return double.Parse(value, CultureInfo.InvariantCulture);
        }

        return (Field("CwPitch"), Field("FilterBandwidth"));
    }

    /// <summary>Reads one recording through the live path, as the app wires it, and returns every letter printed and the text.</summary>
    internal static (IReadOnlyList<Printed> Letters, string Text) ReadLive(string recording)
    {
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(recording));
        var (pitch, width) = RadioState(recording);

        return ReadLive(audio.Samples, audio.SampleRate, pitch, width);
    }

    /// <summary>Reads audio through the live path.</summary>
    internal static (IReadOnlyList<Printed> Letters, string Text) ReadLive(float[] samples, int rate, double pitchHz = 600, double widthHz = 500)
    {
        var detector = new CwEnvelopeDetector(rate);

        detector.SetPassband(pitchHz, widthHz);

        var gate = new CwSenderGate();

        detector.PrintedPitch = () => gate.StationPitchHz;

        var letters = new List<Printed>();
        var characters = new List<CwCharacter>();
        var sequence = 0L;
        var chunk = rate / 100;

        gate.CharacterRead += characters.Add;
        gate.RunRead += (c, run) => letters.Add(new Printed(run[^1].ToSeconds, run.Average(m => m.PitchHz), c.Text));

        for (var at = 0; at + chunk <= samples.Length; at += chunk)
        {
            detector.Process(samples.AsSpan(at, chunk));

            var batch = detector.MarksSince(sequence);

            sequence = batch.Marks.Count > 0 ? batch.Marks.Max(m => m.Sequence) : sequence;
            gate.Read(batch);
        }

        gate.Flush();

        var text = string.Join(' ', string.Concat(characters.Select(c => c.Text)).Split(' ', StringSplitOptions.RemoveEmptyEntries));

        return (letters, text);
    }

    /// <summary>
    /// A text as one character per letter, spaces dropped: a prosign is one character, so the scorer counts it as one
    /// letter, as the reference does.
    /// </summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, char> Prosigns = new(StringComparer.Ordinal);

    internal static string Letters(string text)
    {
        var sb = new StringBuilder();

        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == ' ')
            {
                continue;
            }

            if (text[i] == '<' && text.IndexOf('>', i) is var end and > 0)
            {
                sb.Append(Prosigns.GetOrAdd(text.Substring(i, end - i + 1), _ => (char)(0xE000 + Prosigns.Count)));
                i = end;
                continue;
            }

            sb.Append(text[i]);
        }

        return sb.ToString();
    }

    /// <summary>Letters right: the reference's letters less the edits, with free ends on the printed side.</summary>
    internal static int Right(string printed, string reference)
    {
        var decode = Letters(printed);
        var key = Letters(reference);

        if (decode.Length == 0 || key.Length == 0)
        {
            return 0;
        }

        return Math.Max(0, key.Length - CwScorer.Within(decode, key, CwKeyKind.Exact).Edits);
    }

    /// <summary>Scores every recording, and runs the two hard limits.</summary>
    internal static Board Score(bool limits = true)
    {
        var scored = new List<Scored>();
        var unassigned = new Dictionary<string, string>();

        foreach (var recording in Stretches.Select(s => s.Recording).Distinct())
        {
            var mine = Stretches.Where(s => s.Recording == recording).ToList();
            var (letters, _) = ReadLive(recording);
            var by = mine.ToDictionary(s => s, _ => new StringBuilder());
            var none = new StringBuilder();

            foreach (var letter in letters.OrderBy(l => l.Seconds))
            {
                var home = mine
                    .Where(s => Math.Abs(s.PitchHz - letter.PitchHz) <= 60 && letter.Seconds >= s.From - 1 && letter.Seconds <= s.To + 1)
                    .OrderBy(s => Math.Abs(s.PitchHz - letter.PitchHz))
                    .FirstOrDefault();

                (home is null ? none : by[home]).Append(letter.Text);
            }

            foreach (var s in mine)
            {
                var printed = by[s].ToString();

                scored.Add(new Scored(s, printed, Letters(s.Reference).Length, Right(printed, s.Reference)));
            }

            if (none.Length > 0)
            {
                unassigned[recording] = none.ToString();
            }
        }

        var counted = scored.Where(s => s.Stretch.Confidence >= Confidence.Medium).ToList();
        var first = limits ? TheFirst() : FirstRecording;
        var noise = limits ? NoiseRuns() : [];

        return new Board(scored, unassigned, counted.Sum(s => s.Right), counted.Sum(s => s.ReferenceLetters), first, noise);
    }

    /// <summary>What the first recording reads, spaces as printed.</summary>
    internal static string TheFirst() => ReadLive("cw-2026-10-02-200157").Text;

    /// <summary>Loud noise, 30 s and three minutes, at the two seeds the noise tests use.</summary>
    internal static List<(string What, string Reads)> NoiseRuns()
    {
        var runs = new List<(string, string)>();

        foreach (var (seconds, seed) in new[] { (30, 5190 + 30), (180, 5190 + 180), (30, 5100 + 30), (180, 5100 + 180) })
        {
            var noise = CwSignal.Generate(new CwSignalRequest(
                " ", SampleRate: 8000, Amplitude: 0, NoiseAmplitude: 0.3, LeadInSeconds: seconds / 2.0, TailSeconds: seconds / 2.0, Seed: seed)).Samples;

            runs.Add(($"{seconds} s of loud noise, seed {seed}", ReadLive(noise, 8000).Text));
        }

        return runs;
    }

    /// <summary>The board as the markdown rows of docs\cw-scoreboard.md.</summary>
    internal static string Table(Board board)
    {
        var sb = new StringBuilder();

        sb.AppendLine("| recording | pitch | stretch | confidence | reference | printed | right |");
        sb.AppendLine("|---|---|---|---|---|---|---|");

        foreach (var s in board.Stretches)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"| `{s.Stretch.Recording}` | {s.Stretch.PitchHz:0.0} | {s.Stretch.From:0.#}-{s.Stretch.To:0.#} s | {s.Stretch.Confidence.ToString().ToLowerInvariant()} | `{s.Stretch.Reference}` | `{s.PrintedText}` | {s.Right} of {s.ReferenceLetters} |");
        }

        sb.AppendLine(CultureInfo.InvariantCulture, $"total (medium or better): **{board.Total} of {board.OutOf}**");

        return sb.ToString();
    }

    /// <remarks>
    /// Task 1: the scoreboard. Prints every stretch, reference beside what printed, letters right, and the total over the
    /// stretches of medium confidence or better; then the hard limits, which it asserts.
    /// </remarks>
    [Fact]
    public void TheRecordingsScoreboard()
    {
        var board = Score();

        _output.WriteLine(Table(board));

        foreach (var (recording, text) in board.Unassigned)
        {
            _output.WriteLine($"printed in `{recording}` outside every stretch: `{text}`");
        }

        _output.WriteLine($"the first recording reads `{board.FirstReads}`");

        foreach (var (what, reads) in board.Noise)
        {
            _output.WriteLine($"{what} reads `{reads}`");
        }

        Assert.Equal(FirstRecording, board.FirstReads);
        Assert.All(board.Noise, n => Assert.Equal(string.Empty, n.Reads));
    }

    /// <summary>Scores the board with some rules off, and prints one row of the rule table.</summary>
    private Board OffRow(string label, params string[] rules)
    {
        using var off = CwRules.Off(rules);
        var board = Score();

        _output.WriteLine($"| {label} | {board.Total} | limits {(board.LimitsHold ? "hold" : "BROKEN")} | first reads `{board.FirstReads}` | noise `{string.Join("/", board.Noise.Select(n => n.Reads))}` |");
        _output.WriteLine("  " + string.Join("; ", board.Stretches.Where(s => s.Stretch.Confidence != Confidence.None).Select(s => $"{s.Stretch.Recording[^6..]}@{s.Stretch.PitchHz:0} {s.Right}/{s.ReferenceLetters} `{s.PrintedText}`")));

        return board;
    }

    private void OffAlone(int from, int count)
    {
        foreach (var rule in CwRules.All.Skip(from).Take(count))
        {
            OffRow(rule + " off", rule);
        }
    }

    /// <remarks>
    /// Task 2: what each rule kept is worth, switched off alone, the first six. Thirteen rules came out of the tree in
    /// work instruction 534, each because the total held or rose without it; these stay because it fell, or a hard limit
    /// broke. Asserts nothing; the table is the result.
    /// </remarks>
    [Fact]
    public void EachKeptRuleOffAloneFirst() => OffAlone(0, 6);

    /// <remarks>Task 2: the last five.</remarks>
    [Fact]
    public void EachKeptRuleOffAloneSecond() => OffAlone(6, 5);

    /// <summary>
    /// A stretch read offline and non-causally: mixed to nought at its pitch, through a two-pole low-pass run forward and
    /// back, its level cut at the midpoint of its own two level clusters with a little hysteresis, and its marks and gaps
    /// classed by the stretch's own split and lines from section 8.
    /// </summary>
    internal static (string Elements, string Letters) ReadOffline(Stretch s, float[] x, int rate, double cutoffHz = 40)
    {
        var from = (int)(s.From * rate);
        var to = Math.Min(x.Length, (int)(s.To * rate));
        var n = to - from;
        var i0 = new double[n];
        var q0 = new double[n];

        for (var k = 0; k < n; k++)
        {
            var phase = 2 * Math.PI * s.PitchHz * (from + k) / rate;

            i0[k] = x[from + k] * Math.Cos(phase);
            q0[k] = x[from + k] * Math.Sin(phase);
        }

        var a = 1 - Math.Exp(-2 * Math.PI * cutoffHz / rate);

        void Pass(double[] v, bool back)
        {
            var y = 0.0;

            for (var j = 0; j < v.Length; j++)
            {
                var k = back ? v.Length - 1 - j : j;

                y += a * (v[k] - y);
                v[k] = y;
            }
        }

        foreach (var v in new[] { i0, q0 })
        {
            Pass(v, false);
            Pass(v, false);
            Pass(v, true);
            Pass(v, true);
        }

        // One level a millisecond.
        var step = rate / 1000;
        var db = new double[n / step];

        for (var k = 0; k < db.Length; k++)
        {
            var j = k * step;

            db[k] = 10 * Math.Log10((i0[j] * i0[j]) + (q0[j] * q0[j]) + 1e-20);
        }

        // Two clusters of level, by 2-means from the 10th and 95th percentiles.
        var sorted = db.Order().ToArray();
        var lo = sorted[(int)(0.10 * (sorted.Length - 1))];
        var hi = sorted[(int)(0.95 * (sorted.Length - 1))];

        for (var it = 0; it < 20; it++)
        {
            var mid = (lo + hi) / 2;
            var below = db.Where(d => d < mid).ToList();
            var above = db.Where(d => d >= mid).ToList();

            if (below.Count == 0 || above.Count == 0)
            {
                break;
            }

            lo = below.Average();
            hi = above.Average();
        }

        var cut = (lo + hi) / 2;
        var hyst = Math.Min(1.5, (hi - lo) / 8);
        var runs = new List<(int From, int To)>();
        var on = false;
        var start = 0;

        for (var k = 0; k < db.Length; k++)
        {
            if (!on && db[k] > cut + hyst)
            {
                on = true;
                start = k;
            }
            else if (on && db[k] < cut - hyst)
            {
                on = false;
                runs.Add((start, k));
            }
        }

        // A gap under 15 ms joins its marks; a mark under 20 ms is not one.
        var joined = new List<(int From, int To)>();

        foreach (var r in runs)
        {
            if (joined.Count > 0 && r.From - joined[^1].To < 15)
            {
                joined[^1] = (joined[^1].From, r.To);
            }
            else
            {
                joined.Add(r);
            }
        }

        joined.RemoveAll(r => r.To - r.From < 20);

        var elements = new StringBuilder();
        var letters = new StringBuilder();
        var pattern = new StringBuilder();

        void EndLetter()
        {
            if (pattern.Length > 0)
            {
                letters.Append(MorseAlphabet.Lookup(pattern.ToString()) ?? "■");
                pattern.Clear();
            }
        }

        for (var k = 0; k < joined.Count; k++)
        {
            if (k > 0)
            {
                var gap = joined[k].From - joined[k - 1].To;

                if (gap >= s.LetterMs)
                {
                    EndLetter();
                    elements.Append(CultureInfo.InvariantCulture, $" [{gap}] ");

                    if (gap >= s.WordMs)
                    {
                        letters.Append(' ');
                    }
                }
            }

            var mark = joined[k].To - joined[k].From >= s.SplitMs ? '-' : '.';

            elements.Append(mark);
            pattern.Append(mark);
        }

        EndLetter();

        return (elements.ToString(), letters.ToString());
    }

    /// <summary>Section 8's element list decoded by its own letter breaks.</summary>
    internal static string Decode(string elements)
        => string.Concat(elements.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(t => !t.StartsWith('['))
            .Select(t => MorseAlphabet.Lookup(t) ?? "■"));

    /// <remarks>
    /// Task 1: each stretch read here, offline, beside section 8's element list. Where the letters differ, both element
    /// lists are printed. Asserts nothing: the reference is not changed, and the owner settles it by ear.
    /// </remarks>
    [Fact]
    public void EachStretchReadOffline()
    {
        foreach (var s in Stretches.Where(s => s.Confidence != Confidence.None))
        {
            var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(s.Recording));
            var (elements, letters) = ReadOffline(s, audio.Samples, audio.SampleRate);
            var theirs = Decode(s.Elements);
            var mine = Letters(letters);
            var same = mine == Letters(theirs);

            _output.WriteLine($"{s.Recording} at {s.PitchHz:0.0} Hz, {s.From:0.#}-{s.To:0.#} s: reference `{s.Reference}`; section 8 elements read `{theirs}`; read here `{letters.Trim()}`{(same ? ", the same letters" : string.Empty)}");

            if (!same)
            {
                _output.WriteLine($"  section 8: {s.Elements}");
                _output.WriteLine($"  here:      {elements}");
            }
        }
    }
}
