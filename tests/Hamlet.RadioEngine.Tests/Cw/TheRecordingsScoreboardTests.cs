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
        // The sign-off corrected (work instruction 535): the web session wrote EEV CW where the audio is 73 <AR> W2; the 7 is a
        // 175 ms dah, a 147 ms gap, a 20 ms fragment, then 119, 87, 62 and 80 ms. The 2 is cut by the end of the recording.
        new("cw-2026-10-03-221828", 601.3, 0, 30, "FER ANOTHER FB QSO ES HOPE U HAVE AGN ED ES BEST 73 <AR> W", Confidence.Medium, 150, 125, 330,
            "[310] ..-. [190] . [160] .-. [370] .- [205] -. [215] --- [210] - [135] .... [270] . [180] .-. [420] ..-. [205] -... [350] --.- [185] ... [205] --- [305] . [155] ... [505] .... [225] --- [140] .--. [275] . [565] ..- [445] .... [275] .- [245] ...- [180] . [425] .- [185] --. [195] -. [550] . [215] -.. [545] . [185] ... [285] -... [220] . [195] ... [225] - [222] --... [190] ...-- [128] .-.-. [195] .-- [168] ..--"),
        // The sign-off corrected (work instruction 535), as above: 73 <AR> W2L where the web session wrote EEV CW 2L.
        new("cw-2026-10-03-221851", 601.3, 0, 30, "BEST 73 <AR> W2L CQ DE NA8SB K", Confidence.Low, 150, 125, 330,
            ".. [285] -... [220] . [195] ... [225] - [223] --... [190] ...-- [129] .-.-. [195] .-- [169] ..--- [160] .-.. [230] -.-. [200] --.- [345] -.. [250] . [485] -. [180] .- [165] ---.. [245] ... [320] -... [155] -.- [1970] . [1070] . [370] . [475] -.....- [650] . [545] ... [900] . [235] ... [960] . [350] .. [250] .-. [265] .- [225] ...- [570] ... [160] . [190] . [675]"),
    ];

    /// <summary>One letter the reader printed: when its last mark ended, its pitch, its text.</summary>
    internal readonly record struct Printed(double Seconds, double PitchHz, string Text, bool SpaceBefore = false, CwGapLines? Lines = null, double From = double.NaN);

    /// <summary>A stretch's score: what printed there, its letters, and how many of the reference's letters were read right.</summary>
    internal sealed record Scored(Stretch Stretch, string PrintedText, int ReferenceLetters, int Right, Spaces Spaces = default, IReadOnlyList<Printed>? Letters = null, int Wrong = 0, int Invented = 0);

    /// <summary>The whole board: each stretch, the total over the stretches of medium confidence or better, and the hard limits.</summary>
    internal sealed record Board(IReadOnlyList<Scored> Stretches, IReadOnlyDictionary<string, string> Unassigned, int Total, int OutOf, string FirstReads, IReadOnlyList<(string What, string Reads)> Noise)
    {
        /// <summary>Whether the first recording reads as it must and every noise run printed nothing.</summary>
        public bool LimitsHold => FirstReads == FirstRecording && Noise.All(n => n.Reads.Length == 0) && SilencePrints.Count == 0;

        /// <summary>Spaces right over the stretches of medium confidence or better (work instruction 538).</summary>
        public int SpacesRight => Stretches.Where(s => s.Stretch.Confidence >= Confidence.Medium).Sum(s => s.Spaces.Right);

        /// <summary>The reference's spaces over the same stretches.</summary>
        public int SpacesOutOf => Stretches.Where(s => s.Stretch.Confidence >= Confidence.Medium).Sum(s => s.Spaces.OfReference);

        /// <summary>Spaces printed where the reference has none, over the same stretches.</summary>
        public int SpacesAdded => Stretches.Where(s => s.Stretch.Confidence >= Confidence.Medium).Sum(s => s.Spaces.Added);

        /// <summary>Printed letters not the reference's - wrong or extra - over the stretches of medium confidence or better (work instruction 539).</summary>
        public int Wrong { get; init; }

        /// <summary>Printed letters over no keying at their pitch, in every recording (work instruction 539).</summary>
        public int Invented { get; init; }

        /// <summary>The invented letters, per recording.</summary>
        public IReadOnlyDictionary<string, List<Printed>> InventedBy { get; init; } = new Dictionary<string, List<Printed>>();

        /// <summary>Every letter printed inside a silence of two seconds or more on the keying map: a hard limit (task 2).</summary>
        public IReadOnlyList<(string Recording, (double From, double To) Silence, Printed Letter)> SilencePrints { get; init; } = [];

        /// <summary>**THE SCORE**: letters right less wrong less invented (work instruction 539, HM-DEC-243).</summary>
        public int Score => Total - Wrong - Invented;
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

        // A word end comes before the letter it opens, so it is carried to that letter (work instruction 538).
        var space = false;

        gate.CharacterRead += c =>
        {
            characters.Add(c);
            space |= c.Text == MorseAlphabet.WordGap;
        };
        gate.RunRead += (c, run) =>
        {
            letters.Add(new Printed(run[^1].ToSeconds, run.Average(m => m.PitchHz), c.Text, space, gate.StationLines, run[0].FromSeconds));
            space = false;
        };

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

    /// <summary>A stretch's spaces: a space in both, one in the reference only, one printed only (work instruction 538).</summary>
    internal readonly record struct Spaces(int Right, int Missing, int Added, int OfReference);

    /// <summary>A text's letters, one character each as <see cref="Letters"/> has them, and whether a space follows each.</summary>
    internal static (string Letters, bool[] SpaceAfter) Breaks(string text)
    {
        var letters = new StringBuilder();
        var after = new List<bool>();

        foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (after.Count > 0)
            {
                after[^1] = true;
            }

            var w = Letters(word);

            letters.Append(w);
            after.AddRange(Enumerable.Repeat(false, w.Length));
        }

        return (letters.ToString(), after.ToArray());
    }

    /// <summary>
    /// **THE SCOREBOARD SCORES SPACES** (work instruction 538, task 1): the printed letters are aligned to the reference's
    /// letter by letter, as the letters score aligns them, and each boundary between two reference letters is read on the
    /// printed side between the letters aligned to them. A space in both is right; in the reference only, missing. A space
    /// printed between two aligned letters where the reference has none is added.
    /// </summary>
    internal static Spaces SpacesOf(string printed, string reference)
    {
        var (key, keyAfter) = Breaks(reference);
        var (decode, decodeAfter) = Breaks(printed);
        var ofReference = keyAfter.Count(b => b);

        if (decode.Length == 0 || key.Length == 0)
        {
            return new Spaces(0, ofReference, 0, ofReference);
        }

        var score = CwScorer.Within(decode, key, CwKeyKind.Exact);

        // Each reference letter's printed letter, where the alignment gives it one.
        var at = new int?[key.Length];
        var ki = 0;
        var di = score.Start;

        foreach (var step in score.Steps)
        {
            switch (step.Edit)
            {
                case CwEdit.Same:
                case CwEdit.Wrong:
                    at[ki++] = di++;
                    break;

                case CwEdit.Missing:
                    ki++;
                    break;

                default:
                    di++;
                    break;
            }
        }

        int right = 0, missing = 0, added = 0;

        for (var i = 0; i + 1 < key.Length; i++)
        {
            // The printed letters either side of this boundary: the nearest aligned on each side.
            var left = Enumerable.Range(0, i + 1).Reverse().Select(j => at[j]).FirstOrDefault(d => d is not null);
            var rightOf = Enumerable.Range(i + 1, key.Length - i - 1).Select(j => at[j]).FirstOrDefault(d => d is not null);
            var printedSpace = left is { } l && rightOf is { } r && r > l && Enumerable.Range(l, r - l).Any(d => decodeAfter[d]);

            if (keyAfter[i])
            {
                if (printedSpace)
                {
                    right++;
                }
                else
                {
                    missing++;
                }
            }
            else if (printedSpace && at[i] is { } a && at[i + 1] is { } b && b == a + 1)
            {
                added++;
            }
        }

        return new Spaces(right, missing, added, ofReference);
    }

    /// <summary>A stretch's printed letters, a space before each that opened a word, the stretch's first excepted.</summary>
    internal static string Text(IEnumerable<Printed> letters)
    {
        var sb = new StringBuilder();

        foreach (var letter in letters)
        {
            if (sb.Length > 0 && letter.SpaceBefore)
            {
                sb.Append(' ');
            }

            sb.Append(letter.Text);
        }

        return sb.ToString();
    }

    /// <summary>Scores every recording, and runs the two hard limits.</summary>
    internal static Board Score(bool limits = true)
    {
        var scored = new List<Scored>();
        var unassigned = new Dictionary<string, string>();
        var inventedBy = new Dictionary<string, List<Printed>>();
        var silencePrints = new List<(string, (double, double), Printed)>();

        // Read in parallel, each worker on this thread's rule switches (work instruction 539): one board took 44 s on one thread.
        var rules = CwRules.Current;
        var reads = new System.Collections.Concurrent.ConcurrentDictionary<string, IReadOnlyList<Printed>>(StringComparer.Ordinal);
        var limitsRead = limits ? Task.Run(() => { using var _ = CwRules.Use(rules); return (TheFirst(), NoiseRuns()); }) : null;

        Parallel.ForEach(Stretches.Select(s => s.Recording).Distinct(), r =>
        {
            using var _ = CwRules.Use(rules);
            reads[r] = ReadLive(r).Letters;
        });

        foreach (var recording in Stretches.Select(s => s.Recording).Distinct())
        {
            var mine = Stretches.Where(s => s.Recording == recording).ToList();
            var letters = reads[recording];
            var by = mine.ToDictionary(s => s, _ => new List<Printed>());
            var none = new StringBuilder();

            foreach (var letter in letters.OrderBy(l => l.Seconds))
            {
                var home = mine
                    .Where(s => Math.Abs(s.PitchHz - letter.PitchHz) <= 60 && letter.Seconds >= s.From - 1 && letter.Seconds <= s.To + 1)
                    .OrderBy(s => Math.Abs(s.PitchHz - letter.PitchHz))
                    .FirstOrDefault();

                if (home is null)
                {
                    none.Append(letter.Text);
                }
                else
                {
                    by[home].Add(letter);
                }
            }

            // **WHAT WAS NEVER SENT** (work instruction 539, task 1): a letter whose marks overlap no keying by any station within
            // one bin of its pitch, on the keying map; and a letter printed inside a silence of two seconds or more.
            var map = TheKeyingMapTests.Of(recording);
            var invented = letters.Where(l => !map.Keyed(l.PitchHz, l.From, l.Seconds)).ToList();

            inventedBy[recording] = invented;

            foreach (var l in letters)
            {
                if (map.SilenceAround(l.From, l.Seconds) is { } quiet)
                {
                    silencePrints.Add((recording, quiet, l));
                }
            }

            foreach (var s in mine)
            {
                var printed = Text(by[s]);
                var wrong = WrongAt(printed, s.Reference).Count(i => i < by[s].Count && !invented.Contains(by[s][i]));

                scored.Add(new Scored(s, printed, Letters(s.Reference).Length, Right(printed, s.Reference), SpacesOf(printed, s.Reference), by[s], wrong, by[s].Count(invented.Contains)));
            }

            if (none.Length > 0)
            {
                unassigned[recording] = none.ToString();
            }
        }

        var counted = scored.Where(s => s.Stretch.Confidence >= Confidence.Medium).ToList();
        var (first, noise) = limitsRead is null ? (FirstRecording, new List<(string What, string Reads)>()) : limitsRead.Result;

        return new Board(scored, unassigned, counted.Sum(s => s.Right), counted.Sum(s => s.ReferenceLetters), first, noise)
        {
            Wrong = counted.Sum(s => s.Wrong),
            Invented = inventedBy.Values.Sum(v => v.Count),
            InventedBy = inventedBy,
            SilencePrints = silencePrints,
        };
    }

    /// <summary>
    /// The printed letters the alignment counts as not the reference's: a wrong letter in its place, or an extra (work
    /// instruction 539, task 1). Indices into the printed letters, spaces dropped; the free ends are not counted.
    /// </summary>
    internal static IReadOnlyList<int> WrongAt(string printed, string reference)
    {
        var decode = Letters(printed);
        var key = Letters(reference);

        if (decode.Length == 0 || key.Length == 0)
        {
            return [];
        }

        var score = CwScorer.Within(decode, key, CwKeyKind.Exact);
        var at = new List<int>();
        var di = score.Start;

        foreach (var step in score.Steps)
        {
            switch (step.Edit)
            {
                case CwEdit.Same:
                    di++;
                    break;

                case CwEdit.Wrong:
                case CwEdit.Added:
                    at.Add(di++);
                    break;
            }
        }

        return at;
    }

    /// <summary>What the first recording reads, spaces as printed.</summary>
    internal static string TheFirst() => ReadLive("cw-2026-10-02-200157").Text;

    /// <summary>Loud noise, 30 s and three minutes, at the two seeds the noise tests use; and a carrier keyed at random at five seeds.</summary>
    internal static List<(string What, string Reads)> NoiseRuns()
    {
        var rules = CwRules.Current;
        var jobs = new List<(string What, Func<float[]> Audio)>();

        foreach (var (seconds, seed) in new[] { (30, 5190 + 30), (180, 5190 + 180), (30, 5100 + 30), (180, 5100 + 180) })
        {
            jobs.Add(($"{seconds} s of loud noise, seed {seed}", () => CwSignal.Generate(new CwSignalRequest(
                " ", SampleRate: 8000, Amplitude: 0, NoiseAmplitude: 0.3, LeadInSeconds: seconds / 2.0, TailSeconds: seconds / 2.0, Seed: seed)).Samples));
        }

        // **A CARRIER KEYED AT RANDOM PRINTS NOTHING** (work instruction 535, the owner's third hard limit): alone at 24 dB, marks
        // of 30 to 300 ms at gaps of 30 to 400 ms, at the twenty seeds of its own test (five until work instruction 539, which found a
        // change that printed at a seed the five did not hold).
        foreach (var seed in Enumerable.Range(5193, 20))
        {
            jobs.Add(($"a carrier keyed at random, seed {seed},", () =>
            {
                var carrier = TheShapePicksTheSenderTests.Noise(25, 5192);

                TheShapePicksTheSenderTests.Key(carrier, TheShapePicksTheSenderTests.RandomKeying(2.5, 23, seed), 625, 24);

                return carrier;
            }));
        }

        // Each run on the caller's rule switches, in parallel, kept in order (work instruction 539).
        var reads = new string[jobs.Count];

        Parallel.For(0, jobs.Count, i =>
        {
            using var _ = CwRules.Use(rules);
            reads[i] = ReadLive(jobs[i].Audio(), 8000).Text;
        });

        return jobs.Select((j, i) => (j.What, reads[i])).ToList();
    }

    /// <summary>The board as the markdown rows of docs\cw-scoreboard.md.</summary>
    internal static string Table(Board board)
    {
        var sb = new StringBuilder();

        sb.AppendLine("| recording | pitch | stretch | confidence | reference | printed | right | spaces right | missing | added | wrong | invented |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|---|---|");

        foreach (var s in board.Stretches)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"| `{s.Stretch.Recording}` | {s.Stretch.PitchHz:0.0} | {s.Stretch.From:0.#}-{s.Stretch.To:0.#} s | {s.Stretch.Confidence.ToString().ToLowerInvariant()} | `{s.Stretch.Reference}` | `{s.PrintedText}` | {s.Right} of {s.ReferenceLetters} | {s.Spaces.Right} of {s.Spaces.OfReference} | {s.Spaces.Missing} | {s.Spaces.Added} | {s.Wrong} | {s.Invented} |");
        }

        sb.AppendLine(CultureInfo.InvariantCulture, $"total (medium or better): **{board.Total} of {board.OutOf}** letters right, **{board.Wrong}** wrong; **{board.Invented}** invented in every recording; **score {board.Score}**; **{board.SpacesRight} of {board.SpacesOutOf}** spaces, {board.SpacesAdded} added");

        foreach (var (recording, letters) in board.InventedBy.Where(p => p.Value.Count > 0))
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"invented in `{recording}`: {letters.Count}, `{string.Concat(letters.Select(l => l.Text))}` at {string.Join(", ", letters.Take(8).Select(l => FormattableString.Invariant($"{l.From:0.0} s {l.PitchHz:0} Hz")))}{(letters.Count > 8 ? ", ..." : string.Empty)}");
        }

        foreach (var (recording, quiet, letter) in board.SilencePrints)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"printed in silence: `{recording}` {quiet.From:0.0}-{quiet.To:0.0} s `{letter.Text}` at {letter.From:0.00} s {letter.PitchHz:0} Hz");
        }

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

        // **REAL SILENCE PRINTS NOTHING** (work instruction 539, task 2): a hard limit.
        Assert.Empty(board.SilencePrints);
    }

    /// <summary>A stretch's word line beside its letter and word clusters, as the gate held them when its last letter printed.</summary>
    internal static string LineRow(Scored s)
    {
        var lines = s.Letters?.Where(l => l.Lines is not null).Select(l => l.Lines!).ToList() ?? [];

        if (lines.Count == 0)
        {
            return $"| `{s.Stretch.Recording}` | {s.Stretch.PitchHz:0} | no lines | | | |";
        }

        static string Cluster(IReadOnlyList<double>? c)
            => c is { Count: > 0 } ? FormattableString.Invariant($"{c.Count} gaps, {c.Min() * 1000:0}-{c.Max() * 1000:0} ms, centre {Math.Exp(c.Average(g => Math.Log(g))) * 1000:0}") : "none";

        var last = lines[^1];
        var words = lines.Select(l => l.WordSeconds * 1000).Order().ToList();

        return FormattableString.Invariant(
            $"| `{s.Stretch.Recording}` | {s.Stretch.PitchHz:0} | {Cluster(last.LetterGaps)} | {Cluster(last.WordGaps)} | {last.WordSeconds * 1000:0} ms (median {words[words.Count / 2]:0}, {words[0]:0}-{words[^1]:0}) | {s.Spaces.Right} of {s.Spaces.OfReference}, {s.Spaces.Added} added |");
    }

    /// <remarks>
    /// Work instruction 538, task 2: each stretch's word line beside its letter and word clusters, as the gate held them at
    /// its last letter, with the line's median and range over the stretch. Asserts nothing; the table is the result.
    /// </remarks>
    [Fact]
    public void EachStretchsWordLineBesideItsClusters()
    {
        var board = Score(limits: false);

        _output.WriteLine("| recording | pitch | letter gaps | word gaps | word line at the last letter | spaces |");
        _output.WriteLine("|---|---|---|---|---|---|");

        foreach (var s in board.Stretches.Where(s => s.Stretch.Confidence != Confidence.None))
        {
            _output.WriteLine(LineRow(s));
        }

        _output.WriteLine($"letters {board.Total} of {board.OutOf}; spaces {board.SpacesRight} of {board.SpacesOutOf}, {board.SpacesAdded} added");
    }

    /// <remarks>
    /// Work instruction 538, task 3: the sender who spaces every letter. On the owner's 22:15:30 station at 598 Hz the
    /// letter gaps run 111 to 171 ms and the word cluster is wide, centred at 274 ms; the boundary that set a gap as many of
    /// one cluster's spreads from its centre as of the other's put the word line at 160 ms, under its own letter gaps, and
    /// it printed `AGE 12ILEAR E D CW U SIN G A V`. At the crossing the line is above every letter gap and no space is added.
    /// </remarks>
    [Fact]
    public void TheSenderWhoSpacedItsLettersIsNotSplit()
    {
        Spaces Read(params string[] off)
        {
            using var _ = CwRules.Off(off);
            var s = Score(limits: false).Stretches.Single(x => x.Stretch.Recording == "cw-2026-10-03-221530" && x.Stretch.PitchHz > 590);

            _output.WriteLine($"off: {(off.Length == 0 ? "none" : string.Join(", ", off))}: `{s.PrintedText}`; {LineRow(s)}");

            return s.Spaces;
        }

        var before = Read(CwRules.GapCrossing);
        var after = Read();

        Assert.True(before.Added >= 3, $"before: {before.Added} added");
        Assert.Equal(0, after.Added);
    }

    /// <remarks>
    /// Work instruction 538, tasks 2 and 3: the board with the two word-line rules on and off, letters and spaces and the
    /// hard limits for each. Asserts nothing; the table is the result.
    /// </remarks>
    [Fact]
    public void TheWordLineRulesOnAndOff()
    {
        foreach (var off in new[] { Array.Empty<string>(), [CwRules.OverlapSplit], [CwRules.GapCrossing], new[] { CwRules.OverlapSplit, CwRules.GapCrossing } })
        {
            using var _ = CwRules.Off(off);
            var board = Score();
            var carriers = board.Noise.Count(n => n.What.StartsWith("a carrier", StringComparison.Ordinal) && n.Reads.Length > 0);

            _output.WriteLine($"| off: {(off.Length == 0 ? "none" : string.Join(", ", off))} | letters {board.Total} of {board.OutOf} | spaces {board.SpacesRight} of {board.SpacesOutOf}, {board.SpacesAdded} added | first `{board.FirstReads}` | noise prints {board.Noise.Count(n => !n.What.StartsWith("a carrier", StringComparison.Ordinal) && n.Reads.Length > 0)} | carriers print {carriers} of 20 |");

            foreach (var s in board.Stretches.Where(s => s.Stretch.Confidence != Confidence.None))
            {
                _output.WriteLine($"    {LineRow(s)} `{s.PrintedText}` {s.Right}/{s.ReferenceLetters}");
            }
        }
    }

    /// <remarks>
    /// Work instruction 538, task 1: the spaces score counts a space in both as right, one in the reference only as missing,
    /// and one printed between two letters the reference runs together as added, through the letters' own alignment.
    /// </remarks>
    [Fact]
    public void TheSpacesScoreCountsWhereTheWordsBreak()
    {
        Assert.Equal(new Spaces(2, 0, 0, 2), SpacesOf("CQ DE W1AW", "CQ DE W1AW"));
        Assert.Equal(new Spaces(0, 2, 0, 2), SpacesOf("CQDEW1AW", "CQ DE W1AW"));
        Assert.Equal(new Spaces(2, 0, 3, 2), SpacesOf("C Q DE W 1 AW", "CQ DE W1AW"));
        Assert.Equal(new Spaces(1, 1, 0, 2), SpacesOf("CQ DEXW1AW", "CQ DE W1AW"));
        Assert.Equal(new Spaces(1, 0, 0, 1), SpacesOf("57<BT> B", "57<BT> B"));
        Assert.Equal(new Spaces(0, 3, 0, 3), SpacesOf(string.Empty, "A B C D"));
    }

    /// <summary>One board's row on the new score: right, wrong, invented, the score, spaces and the hard limits.</summary>
    internal static string ScoreRow(string label, Board board)
    {
        var carriers = board.Noise.Count(n => n.What.StartsWith("a carrier", StringComparison.Ordinal) && n.Reads.Length > 0);
        var noise = board.Noise.Count(n => !n.What.StartsWith("a carrier", StringComparison.Ordinal) && n.Reads.Length > 0);

        return $"| {label} | {board.Total} | {board.Wrong} | {board.Invented} | **{board.Score}** | {board.SpacesRight} | {(board.FirstReads == FirstRecording ? "reads" : $"`{board.FirstReads}`")} | {noise} | {carriers} of 20 | {board.SilencePrints.Count} |";
    }

    /// <summary>The rules a search starts from, from the environment: kept rules taken out, separated by |.</summary>
    private static string[] SearchBase()
        => (Environment.GetEnvironmentVariable("HAMLET_RULES_OFF") ?? string.Empty)
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <remarks>
    /// Work instruction 539, task 3: from the rules in <c>HAMLET_RULES_OFF</c>, every single change - each kept rule taken
    /// out, and each one already out put back - scored on the score of right less wrong less invented, best first. Asserts
    /// nothing; the table is the result. The ten rules removed in work instruction 541 are no longer in the tree to bring
    /// back; the tags <c>before-scoreboard</c> and <c>before-false-characters</c> hold them.
    /// </remarks>
    [Fact]
    public void EveryChangeFromTheSearchBase()
    {
        var off = SearchBase();
        Board Measure(string[] o)
        {
            using var a = CwRules.Off(o);

            return Score();
        }

        var start = Measure(off);
        var rows = new List<(int Score, string Row)>();

        _output.WriteLine($"base: off [{string.Join(", ", off)}]");
        _output.WriteLine("| change | right | wrong | invented | score | spaces | first | noise prints | carriers print | silence prints |");
        _output.WriteLine("|---|---|---|---|---|---|---|---|---|---|");
        _output.WriteLine(ScoreRow("none (the base)", start));

        foreach (var rule in CwRules.All)
        {
            var board = off.Contains(rule) ? Measure(off.Where(r => r != rule).ToArray()) : Measure(off.Append(rule).ToArray());

            rows.Add((board.Score, ScoreRow((off.Contains(rule) ? "back in: " : "out: ") + rule, board)));
        }

        foreach (var (_, row) in rows.OrderByDescending(r => r.Score))
        {
            _output.WriteLine(row);
        }
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
    /// The shape side's reading, which the capture sheet now takes its pitch, speed, counts and senders from (work
    /// instruction 537): on the first recording, every letter the gate printed is counted, the printed sender is the one
    /// at its pitch, and the gate holds it.
    /// </remarks>
    [Fact]
    public void TheShapeSideReadingCountsWhatThePrinterPrinted()
    {
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav("cw-2026-10-02-200157"));
        var detector = new CwEnvelopeDetector(audio.SampleRate);

        detector.SetPassband(600, 500);

        var gate = new CwSenderGate();
        var letters = 0;
        var sequence = 0L;
        var chunk = audio.SampleRate / 100;

        detector.PrintedPitch = () => gate.StationPitchHz;
        gate.RunRead += (_, _) => letters++;

        for (var at = 0; at + chunk <= audio.Samples.Length; at += chunk)
        {
            detector.Process(audio.Samples.AsSpan(at, chunk));

            var batch = detector.MarksSince(sequence);

            sequence = batch.Marks.Count > 0 ? batch.Marks.Max(m => m.Sequence) : sequence;
            gate.Read(batch);
        }

        var reading = gate.ShapeReading;

        _output.WriteLine($"letters {reading.LettersPrinted} ({letters} raised), marks {reading.MarksStood}, printed {reading.PrintedPitchHz:0.0} Hz at {reading.PrintedWpm} WPM; senders {string.Join("; ", reading.Senders.Select(s => $"{s.PitchHz:0} {s.ShapeScore:0.00} {s.Marks}{(s.Printed ? " printed" : "")}"))}");

        Assert.Equal(letters, reading.LettersPrinted);
        Assert.InRange(reading.PrintedPitchHz, 640, 685);
        Assert.Contains(reading.Senders, s => s.Printed);
        Assert.True(reading.MarksStood >= reading.LettersPrinted);
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

    [Fact]
    public void DiagSignOff()
    {
        foreach (var (name, from, to) in new[] { ("cw-2026-10-03-221828", 24.0, 30.0), ("cw-2026-10-03-221851", 0.0, 8.0) })
        {
            var s = Stretches.First(x => x.Recording == name);
            var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(name));
            var (elements, letters) = ReadOffline(s with { From = from, To = to }, audio.Samples, audio.SampleRate);
            _output.WriteLine($"{name} {from}-{to}: {elements} => {letters}");
            var (e2, l2) = ReadOffline(s with { From = from, To = to, LetterMs = 300, WordMs = 600 }, audio.Samples, audio.SampleRate);
            _output.WriteLine($"  letter line 300: {e2} => {l2}");
        }
    }

    /// <summary>
    /// An offline element list's word breaks drawn from its own gaps: the gaps between letters split in two by log-length
    /// 2-means, the word line at the geometric middle of the two centres. Returns the line and the text with its spaces.
    /// </summary>
    internal static (double WordLineMs, double LetterCentreMs, double WordCentreMs, string Text) OwnWordBreaks(string elements)
    {
        var tokens = elements.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var gaps = tokens.Where(t => t.StartsWith('[')).Select(t => double.Parse(t.Trim('[', ']'), CultureInfo.InvariantCulture)).Where(g => g > 0).ToList();

        if (gaps.Count < 2)
        {
            return (double.NaN, double.NaN, double.NaN, Decode(elements));
        }

        var logs = gaps.Select(g => Math.Log(g)).Order().ToArray();
        var lo = logs[0];
        var hi = logs[^1];

        for (var it = 0; it < 30; it++)
        {
            var mid = (lo + hi) / 2;
            var below = logs.Where(v => v < mid).ToList();
            var above = logs.Where(v => v >= mid).ToList();

            if (below.Count == 0 || above.Count == 0)
            {
                break;
            }

            lo = below.Average();
            hi = above.Average();
        }

        var line = Math.Exp((lo + hi) / 2);
        var sb = new StringBuilder();

        foreach (var t in tokens)
        {
            if (t.StartsWith('['))
            {
                if (double.Parse(t.Trim('[', ']'), CultureInfo.InvariantCulture) >= line && sb.Length > 0)
                {
                    sb.Append(' ');
                }
            }
            else
            {
                sb.Append(MorseAlphabet.Lookup(t) ?? "■");
            }
        }

        return (line, Math.Exp(lo), Math.Exp(hi), sb.ToString());
    }

    /// <remarks>
    /// Work instruction 538, task 1: each stretch's word gaps read here, offline, from section 8's element list with a word
    /// line drawn from its own gaps, beside the reference's spaces. Where they differ both are printed; the reference is not
    /// changed and is what the score uses.
    /// </remarks>
    [Fact]
    public void EachStretchsWordBreaksReadOffline()
    {
        foreach (var s in Stretches.Where(s => s.Confidence != Confidence.None))
        {
            var (line, letter, word, text) = OwnWordBreaks(s.Elements);
            var against = SpacesOf(text, s.Reference);
            var same = against.Missing == 0 && against.Added == 0;

            _output.WriteLine($"{s.Recording} at {s.PitchHz:0}: letter gaps near {letter:0} ms, word gaps near {word:0} ms, line {line:0} ms; read `{text}`; reference `{s.Reference}`{(same ? ", the same breaks" : $", {against.Right} of {against.OfReference} reference spaces here, {against.Missing} not, {against.Added} more")}");
        }
    }

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
