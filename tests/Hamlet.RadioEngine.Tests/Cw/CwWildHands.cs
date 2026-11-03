using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Printed = Hamlet.RadioEngine.Tests.Cw.TheRecordingsScoreboardTests.Printed;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **TWO REAL HANDS WITH A DRAFT TRUTH** (work instruction 592, HM-DEC-296): Ken N3AFS and Bryan KB2UZK/M on `cw-2026-10-19-141708`, read
/// through the app's chain and scored as the twelve-plus-three are, against `cw-2026-10-19-141708.truth.md`, with a `?` standing alone
/// matching anything. The truth is a draft, so these are reported and never guarded (HM-DEC-284).
/// </summary>
internal static class CwWildHands
{
    /// <summary>The recording.</summary>
    public const string Recording = "cw-2026-10-19-141708";

    /// <summary>How long each stretch reported is.</summary>
    public const double WindowSeconds = 20;

    /// <summary>A stretch of the truth file: its name, its span, its pitch and its text.</summary>
    public sealed record Truth(string Name, double From, double To, double PitchHz, string Text);

    /// <summary>What the reader held at one moment: the printed lane and what it learned.</summary>
    public sealed record Held(double AtSeconds, double PrintedHz, CwSequenceReader.Learned? Learned);

    /// <summary>What one letter of the decode was, on the alignment.</summary>
    public enum LetterIs
    {
        /// <summary>Before or after the truth: not scored.</summary>
        Outside,

        /// <summary>The truth's own letter.</summary>
        Same,

        /// <summary>In the place of another letter.</summary>
        Wrong,

        /// <summary>Where the truth has none.</summary>
        Added,

        /// <summary>Under a `?`, which matches anything.</summary>
        Unknown,
    }

    /// <summary>One stretch of a hand's score.</summary>
    public sealed record Window(double From, double To, string Printed, int Letters, int Right, int Wrong, int Invented, int Unknown, int Missing, int SpacesRight, int SpacesMissing, int SpacesAdded);

    /// <summary>A hand's whole score.</summary>
    public sealed record Score(
        Truth Truth, IReadOnlyList<Printed> Letters, string Printed, int Right, int OutOf, int Wrong, int Invented, int Silence,
        int SpacesRight, int SpacesOutOf, int SpacesAdded, IReadOnlyList<Window> Windows, IReadOnlyList<string> Unknowns);

    /// <summary>The two stretches, read from the truth file beside the capture, so the draft has one home.</summary>
    public static IReadOnlyList<Truth> Truths()
    {
        var path = Path.ChangeExtension(TheOwnersRecordingReadsTests.Wav(Recording), ".truth.md");
        var inside = false;
        var truths = new List<Truth>();
        var text = new StringBuilder();
        Match? head = null;

        void Close()
        {
            if (head is not null)
            {
                truths.Add(new Truth(
                    head.Groups["name"].Value,
                    double.Parse(head.Groups["from"].Value, CultureInfo.InvariantCulture),
                    double.Parse(head.Groups["to"].Value, CultureInfo.InvariantCulture),
                    double.Parse(head.Groups["hz"].Value, CultureInfo.InvariantCulture),
                    text.ToString().Trim()));
            }

            head = null;
            text.Clear();
        }

        foreach (var line in File.ReadLines(path))
        {
            if (line.StartsWith("```", StringComparison.Ordinal))
            {
                inside = !inside;
                continue;
            }

            if (!inside || line.Length == 0 || line[0] == '(')
            {
                continue;
            }

            var m = Regex.Match(line, @"^(?<name>[^,]+), (?<from>\d+)-(?<to>\d+) s, (?<hz>\d+) Hz");

            if (m.Success)
            {
                Close();
                head = m;
            }
            else
            {
                text.Append(line).Append(' ');
            }
        }

        Close();
        return truths;
    }

    /// <summary>
    /// Audio through the app's chain, exactly as <see cref="TheRecordingsScoreboardTests.ReadLive(float[], int, double, double)"/> reads
    /// it, with the reader's printed lane and what it learned taken at the moments asked for.
    /// </summary>
    public static (IReadOnlyList<Printed> Letters, IReadOnlyList<Held> Held) Read(float[] samples, int rate, double pitchHz, double widthHz, IReadOnlyList<double> moments)
    {
        using var chain = new CwChain(rate);
        var gate = chain.Decoder.Runs;

        chain.Detector.SetPassband(pitchHz, widthHz);

        var letters = new List<Printed>();
        var held = new List<Held>();
        var chunk = rate / 100;
        var space = false;
        var next = 0;

        gate.CharacterRead += c => space |= c.Text == MorseAlphabet.WordGap;
        gate.RunRead += (c, run) =>
        {
            letters.Add(new Printed(run[^1].ToSeconds, run.Average(m => m.PitchHz), c.Text, space, gate.StationLines, run[0].FromSeconds));
            space = false;
        };

        for (var at = 0; at + chunk <= samples.Length; at += chunk)
        {
            chain.Process(new AudioChunk(at, rate, samples.AsSpan(at, chunk)));

            while (next < moments.Count && at / (double)rate >= moments[next])
            {
                held.Add(Take(chain, moments[next]));
                next++;
            }
        }

        chain.Decoder.Flush();

        for (; next < moments.Count; next++)
        {
            held.Add(Take(chain, moments[next]));
        }

        return (letters, held);
    }

    private static Held Take(CwChain chain, double at)
    {
        var reader = chain.Detector.Sequence;
        var printed = reader?.PrintedPitchHz ?? double.NaN;
        var learned = reader is null || double.IsNaN(printed) ? null : reader.LaneReadings.First(l => Math.Abs(l.PitchHz - printed) < 1).Learned;

        return new Held(at, printed, learned);
    }

    /// <summary>
    /// Scores a stretch's letters against its truth, as <see cref="TheRecordingsScoreboardTests.Right"/> and its neighbours score: letters
    /// right is the truth's letters less the edits, a letter its stretch's within 60 Hz and a second as the board has it, wrong and invented on the keying map, with free ends on the printed
    /// side; a `?` standing alone is free for any run of printed letters and counts no letter of the truth.
    /// </summary>
    public static Score ScoreOf(Truth truth, IReadOnlyList<Printed> all, TheKeyingMapTests.Map map)
    {
        var letters = all.Where(l => l.Seconds >= truth.From - 1 && l.Seconds <= truth.To + 1 && Math.Abs(l.PitchHz - truth.PitchHz) <= 60).OrderBy(l => l.Seconds).ToList();
        var printed = TheRecordingsScoreboardTests.Text(letters);

        // The truth, one character a letter as the board has it; a `?` alone is a place, not a letter.
        var key = new StringBuilder();
        var free = new HashSet<int>();
        var spaceAfter = new List<bool>();

        foreach (var word in truth.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (word == "?")
            {
                free.Add(key.Length);

                if (spaceAfter.Count > 0)
                {
                    // Whether a space stands beside what nobody read is unknown: the boundary is not scored.
                    spaceAfter[^1] = false;
                }

                continue;
            }

            if (spaceAfter.Count > 0 && !free.Contains(key.Length))
            {
                spaceAfter[^1] = true;
            }

            var w = TheRecordingsScoreboardTests.Letters(word);

            key.Append(w);
            spaceAfter.AddRange(Enumerable.Repeat(false, w.Length));
        }

        var decode = string.Concat(letters.Select(l => TheRecordingsScoreboardTests.Letters(l.Text)));

        // A letter of the decode is one printed letter: a read prints one letter or one prosign at a time.
        var of = letters.SelectMany((l, i) => Enumerable.Repeat(i, TheRecordingsScoreboardTests.Letters(l.Text).Length)).ToArray();
        var (kinds, at, edits, unknowns) = Align(decode, key.ToString(), free);
        var invented = letters.Where(l => !map.Keyed(l.PitchHz, l.From, l.Seconds)).ToHashSet();

        bool WrongLetter(int d) => kinds[d] is LetterIs.Wrong or LetterIs.Added && !invented.Contains(letters[of[d]]);

        var spaceBefore = letters.Select((l, i) => i > 0 && l.SpaceBefore).ToArray();

        // Each boundary between two letters of the truth, read between the printed letters aligned to them, with the second it falls at.
        var boundaries = new List<(double Second, bool Right, bool Missing, bool Added)>();

        for (var k = 0; k + 1 < key.Length; k++)
        {
            if (free.Contains(k + 1))
            {
                continue;
            }

            var left = Enumerable.Range(0, k + 1).Reverse().Select(j => at[j]).FirstOrDefault(d => d is not null);
            var right = Enumerable.Range(k + 1, key.Length - k - 1).Select(j => at[j]).FirstOrDefault(d => d is not null);

            if (left is not { } l || right is not { } r)
            {
                if (spaceAfter[k])
                {
                    boundaries.Add((left is { } only ? letters[of[only]].Seconds : truth.From, false, true, false));
                }

                continue;
            }

            var space = r > l && Enumerable.Range(of[l] + 1, of[r] - of[l]).Any(i => spaceBefore[i]);

            if (spaceAfter[k])
            {
                boundaries.Add((letters[of[l]].Seconds, space, !space, false));
            }
            else if (space && at[k] is not null && at[k + 1] == at[k] + 1)
            {
                boundaries.Add((letters[of[l]].Seconds, false, false, true));
            }
        }

        // Each letter of the truth with nothing printed for it falls at the last printed letter before it.
        var missingAt = new List<double>();

        for (var k = 0; k < key.Length; k++)
        {
            if (at[k] is null)
            {
                var before = Enumerable.Range(0, k).Reverse().Select(j => at[j]).FirstOrDefault(d => d is not null);

                missingAt.Add(before is { } d ? letters[of[d]].Seconds : truth.From);
            }
        }

        var windows = new List<Window>();

        for (var from = truth.From; from < truth.To; from += WindowSeconds)
        {
            var to = Math.Min(truth.To, from + WindowSeconds);
            var last = to >= truth.To;

            bool In(double s) => (s >= from || from == truth.From) && (s < to || last);

            var indices = Enumerable.Range(0, decode.Length).Where(d => In(letters[of[d]].Seconds)).ToList();
            var here = letters.Where(l => In(l.Seconds)).ToList();
            var same = indices.Count(d => kinds[d] == LetterIs.Same);
            var added = indices.Count(d => kinds[d] == LetterIs.Added);
            var marks = boundaries.Where(b => In(b.Second)).ToList();

            windows.Add(new Window(
                from, to, TheRecordingsScoreboardTests.Text(here), here.Count, same - added, indices.Count(WrongLetter), here.Count(invented.Contains),
                indices.Count(d => kinds[d] == LetterIs.Unknown), missingAt.Count(In), marks.Count(b => b.Right), marks.Count(b => b.Missing), marks.Count(b => b.Added)));
        }

        return new Score(
            truth, letters, printed, Math.Max(0, key.Length - edits), key.Length,
            Enumerable.Range(0, decode.Length).Count(WrongLetter), letters.Count(invented.Contains),
            letters.Count(l => map.SilenceAround(l.From, l.Seconds) is not null),
            boundaries.Count(b => b.Right), boundaries.Count(b => b.Right || b.Missing), boundaries.Count(b => b.Added),
            windows, unknowns);
    }

    /// <summary>
    /// The edit distance of <see cref="CwScorer"/> with free ends, in which a place in <paramref name="free"/> also takes any run of the
    /// decode's letters at no cost. Returns what each decode letter was, each key letter's decode letter, the edits, and the run under each place.
    /// </summary>
    internal static (LetterIs[] Kinds, int?[] At, int Edits, IReadOnlyList<string> Unknowns) Align(string decode, string key, IReadOnlySet<int> free)
    {
        var m = key.Length;
        var n = decode.Length;
        var cost = new int[m + 1, n + 1];

        for (var i = 1; i <= m; i++)
        {
            cost[i, 0] = cost[i - 1, 0] + 1;
        }

        for (var i = 1; i <= m; i++)
        {
            var open = free.Contains(i) ? 0 : 1;

            for (var j = 1; j <= n; j++)
            {
                cost[i, j] = Math.Min(
                    cost[i - 1, j - 1] + (key[i - 1] == decode[j - 1] ? 0 : 1),
                    Math.Min(cost[i - 1, j] + 1, cost[i, j - 1] + open));
            }
        }

        var end = 0;

        for (var j = 1; j <= n; j++)
        {
            if (cost[m, j] < cost[m, end])
            {
                end = j;
            }
        }

        var kinds = new LetterIs[n];
        var at = new int?[m];
        var a = m;
        var b = end;

        while (a > 0)
        {
            if (b > 0 && cost[a, b] == cost[a - 1, b - 1] + (key[a - 1] == decode[b - 1] ? 0 : 1))
            {
                kinds[b - 1] = key[a - 1] == decode[b - 1] ? LetterIs.Same : LetterIs.Wrong;
                at[a - 1] = b - 1;
                a--;
                b--;
            }
            else if (cost[a, b] == cost[a - 1, b] + 1)
            {
                a--;
            }
            else
            {
                kinds[b - 1] = free.Contains(a) ? LetterIs.Unknown : LetterIs.Added;
                b--;
            }
        }

        // The places in order, and what lies under each: the decode between the letters aligned either side of it, kept where it fell.
        var unknowns = new List<string>();

        foreach (var place in free.Order())
        {
            var after = place < m ? Enumerable.Range(place, m - place).Select(k => at[k]).FirstOrDefault(d => d is not null) : null;
            var before = place > 0 ? Enumerable.Range(0, place).Reverse().Select(k => at[k]).FirstOrDefault(d => d is not null) : null;
            var run = new StringBuilder();

            // The first place stands before the truth's first read letter, where the alignment's free start gives nothing a kind.
            for (var d = (before ?? -1) + 1; d < (after ?? end); d++)
            {
                run.Append(decode[d]);
            }

            unknowns.Add(run.ToString());
        }

        return (kinds, at, kinds.Length == 0 ? m : cost[m, end], unknowns);
    }

    /// <summary>The share of a text's letters that are `E`, `T` or the placeholder.</summary>
    public static double EtShare(IReadOnlyList<Printed> letters)
        => letters.Count == 0 ? double.NaN : letters.Count(l => l.Text is "E" or "T" || l.Text == MorseAlphabet.Unreadable) / (double)letters.Count;
}
