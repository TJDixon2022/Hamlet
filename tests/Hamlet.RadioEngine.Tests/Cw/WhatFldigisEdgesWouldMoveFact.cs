using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Where fldigi's three untried edges would move our own reads, over every keyed
/// recording and every synthetic case (work instruction 462, task 1; PHASE_PLAN.md
/// 9.4; HM-REQ-129).
/// </summary>
/// <remarks>
/// <para>**A PRINTER. IT ASSERTS NOTHING ABOUT EITHER DECODER.** Nothing under
/// `src` changes for it. It prints where ours makes each decision beside fldigi's
/// edge, then every event where one of the edges disagrees with ours, then the
/// ranking.</para>
/// <para>**THE EDGES ARE FLDIGI'S, AT ITS OWN RATIOS, AT THE DIT OURS HAS IN FORCE**
/// (`cw.cxx` at `61b97f41`): (B) a mark shorter than half a dit is not an element,
/// 515 and 818-822; (C) a mark up to two dits is a dit and longer is a dah, 846-855;
/// (W) a gap of two dits and up to four ends a character and past four is a word
/// space, 883-914. The dit in force is the unit of the speed the read that settled
/// the letter was decoded at, 1200 / WPM ms.</para>
/// <para>**OURS IS DRIVEN EXACTLY AS THE METRICS DRIVE IT**, hop by hop through
/// <see cref="WhereOursLosesWhatThePortKeepsTests.Drive"/>, and each settled letter
/// is found on unit 459's copy of our lattice at its read's speed and gaps, so every
/// mark and gap printed is the decoder's own. Letters and boundaries carry the
/// scorer's own verdict from <see cref="TheRequirementsAreMeasuredTests"/>'s
/// alignments: **EVERY REAL KEY IS INFERRED** (V-13); **EVERY SYNTHETIC KEY IS EXACT**.</para>
/// </remarks>
public sealed class WhatFldigisEdgesWouldMoveFact
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private const double HopMs = CwProbabilisticDecoder.HopMilliseconds;

    /// <summary>The three reads the pair speed (D) broke, by recording and key word (unit 459, section 4 item 2).</summary>
    private static readonly (string Recording, string Word)[] RefusedD =
    {
        ("031948", "110,"),
        ("004507", "EACH"),
        ("031905", "PREDICTED"),
    };

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fact.</summary>
    /// <param name="output">Where the trace is printed.</param>
    public WhatFldigisEdgesWouldMoveFact(ITestOutputHelper output)
        => _output = output;

    /// <summary>One place an edge of fldigi's disagrees with what ours did.</summary>
    private sealed record Event(
        string Technique, string Recording, string Kind, double AtSeconds, double Ms, double Dits,
        string What, string Where, string Verdict, string KeyWord, string Effect);

    /// <summary>Every event of the three edges over all 35, and their ranking (HM-REQ-129).</summary>
    [Fact]
    public void EveryEventOfTheThreeEdgesOverAll35()
    {
        Print("edges | where ours decides, file:line at HEAD | ours | fldigi, cw.cxx at 61b97f41");
        Print("edges | a mark is formed | CwProbabilisticDecoder.cs:894-986 Envelope, 988-1085 LogLikelihoods (per-hop key-down and key-up log-likelihoods, nothing thresholded), 1257-1304 DecodeAt (a mark is a key-down segment the segmental Viterbi chooses) | by likelihood over whole segments | 610-656: the detector's envelope against its squelch, key-down and key-up events");
        Print("edges | a short mark is discarded | CwProbabilisticDecoder.cs:542 and 1265 (and the rival lattice 1509): no key-down segment shorter than max(1, (int)(0.45 x want)) hops, so a shorter mark is absorbed by the key-up segment around it; 1609-1626 Judged and 1644-1645 IsStrayElement drop whole characters (1.0 per hop, one element under a span of 13), never a mark | 0.45 dit, truncated to whole 5 ms hops | 515, 818-822: a mark under 0.5 dit is ignored (the state goes idle)");
        Print("edges | a mark becomes a dit or a dah | CwProbabilisticDecoder.cs:526-533 Kinds (dit 1 unit, dah 3), 1291-1293 the length penalty ln(span/want)/0.35 squared, 580; spans bounded by 542 and 544 (dit 0.45 to 2.2 units, dah 1.35 to 6.6) | by likelihood: the two penalties cross at sqrt(3) = 1.73 units, evidence being identical | 846-855: 2 dits (a mark up to two_dots is a dit)");
        Print("edges | a gap becomes an element, character or word gap | CwProbabilisticDecoder.cs:530-532 Kinds (1, 3, 7 units) or, with structure held, the sender's own three gaps (CwProbabilisticStream.cs:497-504, CwProbabilisticDecoder.cs:1230-1237), scored by the same penalty; then CwProbabilisticStream.cs:713-734 Spaced takes out a word space shorter than the sender's character gap x sqrt(7/3), or than 549-562's boundary, and never adds one | by likelihood: element|character at sqrt(1 x 3) = 1.73 units, character|word at sqrt(3 x 7) = 4.58 units (textbook) or at the geometric means of the held gaps; then the relabel | 883-914: under 2 dits nothing, 2 to 4 dits a character ends, over 4 dits a word space");
        Print("edges | the dit in force | CwProbabilisticStream.cs:442-449 the window's measured speed, else the 8 to 40 WPM grid at CwProbabilisticDecoder.cs:771-783; 516-531 the marks' overrule past 1.25; the unit is 1200 / WPM ms of the speed the read was decoded at, and the held gaps are the sender's ms, not units | one read's speed per 12 s window | 502-515: cw_receive_dot_length from the tracked two_dots");
        Print("edges | side by side, in dits | spike: ours 0.45 (truncated to a hop), fldigi 0.5 | dit|dah: ours 1.73 by likelihood, fldigi 2 | element|character gap: ours 1.73 by likelihood (or the held gaps' mean), fldigi 2 | character|word gap: ours 4.58 by likelihood (or the held gaps' mean) then the relabel, fldigi 4");

        var events = new List<Event>();
        var reads = 0;
        var readsSame = 0;
        var lettersFound = 0;
        var lettersMissing = 0;
        var gapsSkipped = 0;
        var rows = TheRequirementsAreMeasuredTests.Real.Concat(TheRequirementsAreMeasuredTests.Synthetic).ToList();

        foreach (var m in rows)
        {
            var real = m.Set != "synthetic";
            var file = Path.Combine(real ? CapturedSignalTests.Folder : SyntheticCq.Folder, m.Name + ".wav");
            var (settled, readings) = WhereOursLosesWhatThePortKeepsTests.Drive(file, real ? 600 : SyntheticCq.StartingPitchHz);

            if (!settled.Select(c => c.Text).SequenceEqual(m.Settled.Select(c => c.Text)))
            {
                Print($"mismatch | {m.Name} | the second drive settled differently from the scored one; skipped");
                continue;
            }

            var (verdict, word, boundary) = Score(m);
            var kind = CwMetrics.KindWord(m.Kind);
            var paths = new Dictionary<(long, int, double), IReadOnlyList<WhereOursLosesWhatThePortKeepsTests.PathLetter>>();
            var likelihoods = new Dictionary<(long, int), (double[] Down, double[] Up)>();
            var discardSeen = new HashSet<long>();

            Print($"text-class | {m.Name} | {kind} | {Classed(settled, verdict)}");

            for (var i = 0; i < settled.Count; i++)
            {
                var c = settled[i];

                if (c.IsWordGap)
                {
                    continue;
                }

                var r = readings[i];
                var wpm = r.Last.WordsPerMinute;

                if (wpm <= 0)
                {
                    lettersMissing++;
                    continue;
                }

                var lk = (r.WindowStartHop, r.Window.Length);

                if (!likelihoods.TryGetValue(lk, out var ll))
                {
                    ll = CwProbabilisticDecoder.LogLikelihoods(r.Window);
                    likelihoods[lk] = ll;
                }

                var pk = (r.WindowStartHop, r.Window.Length, wpm);

                if (!paths.TryGetValue(pk, out var letters))
                {
                    letters = WhereOursLosesWhatThePortKeepsTests.Path(ll.Down, ll.Up, wpm, r.GapMs);
                    paths[pk] = letters;
                    reads++;

                    var printed = r.Last.Characters.Where(x => x.Pattern.Length > 0).ToList();
                    var onPath = letters.Where(l => l.Printed).ToList();

                    if (printed.Count == onPath.Count
                        && printed.Zip(onPath).All(z => z.First.Pattern == z.Second.Pattern && z.First.EndHop == z.Second.End))
                    {
                        readsSame++;
                    }
                }

                var endLocal = (int)(Math.Round(c.At.TotalMilliseconds / HopMs) - r.WindowStartHop);
                var n = -1;

                for (var x = 0; x < letters.Count; x++)
                {
                    if (letters[x].End == endLocal && letters[x].Pattern == c.Pattern)
                    {
                        n = x;
                        break;
                    }
                }

                if (n < 0)
                {
                    lettersMissing++;
                    continue;
                }

                lettersFound++;

                var here = letters[n];
                var unit = 1200.0 / wpm / HopMs;
                var where = $"`{c.Text}` {c.Pattern}";
                var v = verdict[i];
                var w = word[i];
                double At(int hop) => (r.WindowStartHop + hop) * HopMs / 1000.0;
                string LetterEffect(string verdictHere) => verdictHere is "wrong" or "added" ? "fixable" : verdictHere == "right" ? "at risk" : "neither";

                foreach (var s in here.Segments)
                {
                    var dits = s.Hops / unit;

                    // (B) a mark under half the dit in force that ours keeps (cw.cxx:515, 818-822).
                    if (s.IsMark && s.Hops < unit / 2)
                    {
                        events.Add(new Event("B", m.Name, kind, At(s.Start), s.Hops * HopMs, dits,
                            $"{(s.Kind == 0 ? "dit" : "dah")} kept under half a dit", where, v, w, LetterEffect(v)));
                    }

                    // (C) a mark ours classes against the two-dit edge (cw.cxx:846-855).
                    if (s.Kind == 1 && s.Hops <= 2 * unit)
                    {
                        events.Add(new Event("C", m.Name, kind, At(s.Start), s.Hops * HopMs, dits,
                            "dah at or under two dits (fldigi: a dit)", where, v, w, LetterEffect(v)));
                    }
                    else if (s.Kind == 0 && s.Hops > 2 * unit)
                    {
                        events.Add(new Event("C", m.Name, kind, At(s.Start), s.Hops * HopMs, dits,
                            "dit over two dits (fldigi: a dah)", where, v, w, LetterEffect(v)));
                    }

                    // (W) a gap inside the letter at or past two dits: fldigi ends the character there (cw.cxx:894-897).
                    if (s.Kind == 2 && s.Hops >= 2 * unit)
                    {
                        events.Add(new Event("W", m.Name, kind, At(s.Start), s.Hops * HopMs, dits,
                            $"element gap at or over two dits (fldigi: the letter ends{(dits > 4 ? ", and a word space" : "")})", where, v, w, LetterEffect(v)));
                    }
                }

                // (B) a mark at or over half a dit that ours takes as key-up, within one dit of this letter:
                // hops where key-down beats key-up that no mark on the path covers, or the marks of a letter the path
                // spelled and did not print. The form never restores these; they are listed, not counted.
                var pathMarks = letters.SelectMany(l => l.Segments).Where(s => s.IsMark).ToList();
                var near = WhereOursLosesWhatThePortKeepsTests.MarkRuns(ll.Down, ll.Up, here.Start - (int)unit, here.End + (int)unit);

                foreach (var (start, end) in near)
                {
                    if (end - start >= unit / 2 && !pathMarks.Any(pm => pm.Start < end && start < pm.End)
                        && discardSeen.Add(r.WindowStartHop + start))
                    {
                        events.Add(new Event("B-", m.Name, kind, At(start), (end - start) * HopMs, (end - start) / unit,
                            "key-down run at or over half a dit that the path took as key-up", where, v, w, "neither"));
                    }
                }

                foreach (var neighbour in new[] { n - 1, n + 1 })
                {
                    if (neighbour < 0 || neighbour >= letters.Count || letters[neighbour].Printed)
                    {
                        continue;
                    }

                    foreach (var s in letters[neighbour].Segments.Where(s => s.IsMark && s.Hops >= unit / 2))
                    {
                        if (discardSeen.Add(r.WindowStartHop + s.Start))
                        {
                            events.Add(new Event("B-", m.Name, kind, At(s.Start), s.Hops * HopMs, s.Hops / unit,
                                $"mark of the unprinted letter {letters[neighbour].Pattern} beside it", where, v, w, "neither"));
                        }
                    }
                }

                // (W) the gap before this letter, from the letter before it on the same path.
                var p = i - 1;
                var spaced = false;

                while (p >= 0 && settled[p].IsWordGap)
                {
                    spaced = true;
                    p--;
                }

                if (p < 0)
                {
                    continue;
                }

                var before = n > 0 ? letters[n - 1] : null;
                var beforeEnd = (int)(Math.Round(settled[p].At.TotalMilliseconds / HopMs) - r.WindowStartHop);

                if (before is null || !before.Printed || before.End != beforeEnd || before.After is not { } gap || gap.End != here.Start)
                {
                    gapsSkipped++;
                    continue;
                }

                var g = gap.Hops / unit;
                var ours = spaced ? "word" : "character";
                var fldigi = g < 2 ? "element" : g <= 4 ? "character" : "word";

                if (ours == fldigi)
                {
                    continue;
                }

                var b = boundary[i];
                string effect;

                if (fldigi == "element")
                {
                    var both = new[] { verdict[p], v };
                    effect = both.Any(x => x is "wrong" or "added") ? "fixable" : both.All(x => x == "right") ? "at risk" : "neither";
                }
                else if (ours == "word")
                {
                    effect = b == "inserted" ? "fixable" : b == "right space" ? "at risk" : "neither";
                }
                else
                {
                    effect = b == "deleted" ? "fixable" : b == "right, no space" ? "at risk" : "neither";
                }

                events.Add(new Event("W", m.Name, kind, At(gap.Start), gap.Hops * HopMs, g,
                    $"gap between letters read as a {ours} gap (path kind {(gap.Kind == 4 ? "word" : "letter")}; fldigi: {fldigi}), boundary {b}",
                    $"`{settled[p].Text}`|{where}", $"{verdict[p]}|{v}", w, effect));
            }
        }

        Print(string.Create(Invariant,
            $"sameness | {readsSame} of {reads} reads: the copied lattice spells exactly the letters the read printed; letters found on the path {lettersFound}, not found {lettersMissing}; gaps between letters not on one path, skipped {gapsSkipped}"));
        Print("event | technique | recording | key | at s | ms | dits at the dit in force | what | letter | scorer | key word | effect");

        foreach (var e in events)
        {
            Print(string.Create(Invariant,
                $"event | {e.Technique} | {e.Recording} | {e.Kind} | {e.AtSeconds:0.000} | {e.Ms:0} | {e.Dits:0.00} | {e.What} | {e.Where} | {e.Verdict} | {e.KeyWord} | {e.Effect}"));
        }

        Print("rank | technique | events | fixable (in wrong or added letters, at wrong boundaries) | at risk (in right letters, at right boundaries) | neither | fixable less at risk");

        foreach (var t in new[] { "B", "C", "W" }.OrderByDescending(Net))
        {
            var mine = events.Where(e => e.Technique == t).ToList();

            Print($"rank | {t} | {mine.Count} | {mine.Count(e => e.Effect == "fixable")} | {mine.Count(e => e.Effect == "at risk")} | {mine.Count(e => e.Effect == "neither")} | {Net(t)}{(mine.Count(e => e.Effect == "fixable") == 0 ? " | zero fixable: not screened" : "")}");
        }

        Print($"rank | B- | {events.Count(e => e.Technique == "B-")} | listed only: marks at or over half a dit that ours discards, which (B)'s form does not restore");

        foreach (var (recording, keyWord) in RefusedD)
        {
            var there = events.Where(e => e.Recording.Contains(recording, StringComparison.Ordinal) && e.KeyWord == keyWord).ToList();

            Print($"refused-D | {recording} `{keyWord}` | {there.Count} event(s){(there.Count == 0 ? "" : ": " + string.Join("; ", there.Select(e => string.Create(Invariant, $"{e.Technique} {e.What} at {e.AtSeconds:0.000} s, {e.Dits:0.00} dits, {e.Effect}"))))}");
        }

        // Each technique's one form, fixed here before any screen (work instruction 462, DECIDED (3)).
        Print("form | C | where: CwProbabilisticDecoder, the span bounds of the two mark kinds, in DecodeAt (1265-1266) and in the rival lattice (1509-1510) alike | edge: 2 dits - a key-down span of at most floor(2 x unit) hops can only be a dit and a longer one only a dah (cw.cxx:846-855, element_usec <= two_dots); the length penalty is unchanged | the dit: the unit of the speed the lattice is run at, 1200 / WPM / 5 hops, the read's own speed whether measured, won on the grid or overruled by the marks");
        Print("form | B | where: CwProbabilisticDecoder, the shortest key-down span, in DecodeAt (1265) and in the rival lattice (1509) alike | edge: 0.5 dit - no key-down segment shorter than ceil(unit / 2) hops, the larger of that and today's floor, so a shorter mark falls to the key-up segment around it (cw.cxx:515, 818-822); fldigi's reset of the character in progress is not taken | the dit: the unit of the speed the lattice is run at, as for C");
        Print("form | W | where: CwProbabilisticDecoder, the span bounds of the three key-up kinds, in DecodeAt (1265-1266) and in the rival lattice (1509-1510) alike; CwProbabilisticStream's relabel (Spaced) is left as it is, since it only takes spaces out | edges: 2 and 4 dits - an element gap only under 2 dits (longest ceil(2 x unit) - 1 hops), a character gap from 2 to 4 dits (ceil(2 x unit) to floor(4 x unit) hops), a word gap only past 4 dits (from floor(4 x unit) + 1 hops) (cw.cxx:883-914); each kind's wanted length and so its penalty unchanged, held gaps included | the dit: the unit of the speed the lattice is run at, as for C, whether or not the sender's gaps are held");

        int Net(string t) => events.Count(e => e.Technique == t && e.Effect == "fixable") - events.Count(e => e.Technique == t && e.Effect == "at risk");
    }

    /// <summary>The scorer's verdict on each settled character, the key word it sits in, and the verdict on the boundary before it.</summary>
    private static (string[] Verdict, string[] Word, string[] Boundary) Score(TheRequirementsAreMeasuredTests.Measured m)
    {
        var count = m.Settled.Count;
        var verdict = Enumerable.Repeat("unscored", count).ToArray();
        var word = Enumerable.Repeat("", count).ToArray();
        var boundary = Enumerable.Repeat("unscored", count).ToArray();

        if (m.NotComputable is not null)
        {
            return (verdict, word, boundary);
        }

        for (var s = 0; s < m.Stretches.Count; s++)
        {
            var a = m.Stretches[s];
            var covered = TheRequirementsAreMeasuredTests.Covered(m.Settled, m.Scores[s]).Where(c => !c.IsWordGap).ToList();
            var steps = a.Steps.Where(st => st.Decoded is not null).ToList();
            var index = covered.Select(c => IndexOf(m.Settled, c)).ToList();
            var words = KeyWords(m.Scores[s].Key);

            for (var d = 0; d < steps.Count; d++)
            {
                var st = steps[d];
                var sym = st.Decoded!.Value;
                var i = index[d];

                verdict[i] = sym.Class switch
                {
                    CwSymbolClass.Sure => st.Key is null ? "added" : st.Key == sym.Text ? "right" : "wrong",
                    CwSymbolClass.NotSure => "dim",
                    CwSymbolClass.Placeholder => "placeholder",
                    _ => "?",
                };

                var at = st.Key is not null ? st.KeyBefore : Math.Max(0, st.KeyBefore - 1);
                word[i] = words.Count == 0 ? "" : words[Math.Clamp(at, 0, words.Count - 1)];
            }

            // CwMetrics.WordBoundaries, walked the same way, with each verdict kept where it falls.
            var unclaimed = new SortedSet<int>(a.KeyBoundaries);
            int? low = null;

            for (var d = 0; d < steps.Count; d++)
            {
                var st = steps[d];

                if (low is { } l)
                {
                    if (st.GapBefore)
                    {
                        var match = unclaimed.GetViewBetween(l, Math.Max(l, st.KeyBefore)).Cast<int?>().FirstOrDefault();

                        if (match is { } found)
                        {
                            unclaimed.Remove(found);
                            boundary[index[d]] = "right space";
                        }
                        else
                        {
                            boundary[index[d]] = "inserted";
                        }
                    }
                    else
                    {
                        boundary[index[d]] = "right, no space";
                    }
                }

                low = st.KeyAfter;
            }

            foreach (var b in unclaimed)
            {
                for (var d = 1; d < steps.Count; d++)
                {
                    if (!steps[d].GapBefore && steps[d - 1].KeyAfter <= b && b <= steps[d].KeyBefore
                        && boundary[index[d]] == "right, no space")
                    {
                        boundary[index[d]] = "deleted";
                        break;
                    }
                }
            }
        }

        return (verdict, word, boundary);
    }

    /// <summary>For each of the key's characters, spaces aside, the word it sits in.</summary>
    private static List<string> KeyWords(string key)
    {
        var symbols = CwMetrics.KeySymbols(key);
        var of = new List<int>();
        var words = new List<string>();
        var current = new StringBuilder();

        foreach (var s in symbols)
        {
            if (s.Class == CwSymbolClass.WordGap)
            {
                if (current.Length > 0)
                {
                    words.Add(current.ToString());
                    current.Clear();
                }

                continue;
            }

            of.Add(words.Count);
            current.Append(s.Text);
        }

        if (current.Length > 0)
        {
            words.Add(current.ToString());
        }

        return of.Select(w => words[w]).ToList();
    }

    /// <summary>Our text, each character followed by its class and the scorer's verdict: S sure, N not sure, P placeholder; r right, w wrong, a added, d dim, p placeholder, u unscored.</summary>
    private static string Classed(IReadOnlyList<CwCharacter> settled, string[] verdict)
    {
        var text = new StringBuilder();

        for (var i = 0; i < settled.Count; i++)
        {
            var c = settled[i];

            if (c.IsWordGap)
            {
                text.Append(' ');
                continue;
            }

            var cls = CwSymbol.Of(c).Class switch { CwSymbolClass.Sure => "S", CwSymbolClass.NotSure => "N", _ => "P" };
            text.Append(c.Text).Append('/').Append(cls).Append(verdict[i][0]).Append(' ');
        }

        return text.ToString().TrimEnd();
    }

    private static int IndexOf(IReadOnlyList<CwCharacter> list, CwCharacter c)
    {
        for (var i = 0; i < list.Count; i++)
        {
            if (ReferenceEquals(list[i], c))
            {
                return i;
            }
        }

        return -1;
    }

    private void Print(string line) => _output.WriteLine(line);
}
