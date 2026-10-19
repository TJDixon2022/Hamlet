using System.Globalization;
using Xunit;
using Xunit.Abstractions;
using static Hamlet.RadioEngine.Tests.Cw.TheRecordingsScoreboardTests;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **WORD SPACES ON HAND SENDERS** (work instruction 556, task 3, HM-DEC-260): every space the twelve miss and every one they
/// add, over the stretches the board totals, with the gap the printer saw, the sender's own letter and word clusters and the
/// word line at that moment, grouped by cause.
/// </summary>
public sealed class TheSpacesByCauseTests(ITestOutputHelper output)
{
    /// <summary>One space the board counts as missing or added.</summary>
    internal sealed record Place(string Recording, double PitchHz, bool Missing, string Around, double GapMs, double LineMs, double LetterMs, double WordMs, string Cause)
    {
        /// <summary>The letter cluster's top, its 90th percentile, at the moment the gap was judged (work instruction 565).</summary>
        public double LetterTopMs { get; init; } = double.NaN;

        /// <summary>The word cluster's bottom, its 10th percentile, where one shows (work instruction 565).</summary>
        public double WordBottomMs { get; init; } = double.NaN;

        /// <summary>Where the line between the two clusters falls on the same clusters (work instruction 565).</summary>
        public double BetweenMs { get; init; } = double.NaN;

        /// <summary>How many word gaps the sender's word cluster held.</summary>
        public int WordCount { get; init; }
    }

    /// <summary>The places of one stretch: aligned as <see cref="SpacesOf"/> aligns, with the printed gaps either side.</summary>
    internal static List<Place> PlacesOf(Scored s)
    {
        var places = new List<Place>();
        var letters = s.Letters ?? [];
        var printed = Text(letters);
        var (key, keyAfter) = Breaks(s.Stretch.Reference);
        var (decode, decodeAfter) = Breaks(printed);

        if (decode.Length == 0 || key.Length == 0 || decode.Length != letters.Count)
        {
            return places;
        }

        var score = Hamlet.RadioEngine.Cw.CwScorer.Within(decode, key, Hamlet.RadioEngine.Cw.CwKeyKind.Exact);
        var at = new int?[key.Length];
        var ki = 0;
        var di = score.Start;

        foreach (var step in score.Steps)
        {
            switch (step.Edit)
            {
                case Hamlet.RadioEngine.Cw.CwEdit.Same:
                case Hamlet.RadioEngine.Cw.CwEdit.Wrong:
                    at[ki++] = di++;
                    break;

                case Hamlet.RadioEngine.Cw.CwEdit.Missing:
                    ki++;
                    break;

                default:
                    di++;
                    break;
            }
        }

        for (var i = 0; i + 1 < key.Length; i++)
        {
            var left = Enumerable.Range(0, i + 1).Reverse().Select(j => at[j]).FirstOrDefault(d => d is not null);
            var right = Enumerable.Range(i + 1, key.Length - i - 1).Select(j => at[j]).FirstOrDefault(d => d is not null);
            var printedSpace = left is { } l && right is { } r && r > l && Enumerable.Range(l, r - l).Any(d => decodeAfter[d]);
            var added = !keyAfter[i] && printedSpace && at[i] is { } a && at[i + 1] is { } b && b == a + 1;

            if (!(keyAfter[i] && !printedSpace) && !added)
            {
                continue;
            }

            var around = $"{key[Math.Max(0, i - 3)..(i + 1)]}|{key[(i + 1)..Math.Min(key.Length, i + 4)]}";

            if (left is not { } lo || right is not { } hi)
            {
                places.Add(new Place(s.Stretch.Recording, s.Stretch.PitchHz, !added, around, double.NaN, double.NaN, double.NaN, double.NaN, "a letter either side not printed"));
                continue;
            }

            var next = letters[hi];
            var gap = (next.From - letters[hi - 1].Seconds) * 1000;
            var lines = next.Lines;
            var line = (lines?.WordSeconds ?? double.NaN) * 1000;
            var letterMs = (lines?.LetterSeconds ?? double.NaN) * 1000;
            var wordMs = (lines?.WordClusterSeconds ?? double.NaN) * 1000;
            string cause;

            if (hi - lo != 1)
            {
                cause = added ? "an extra letter between" : "a letter between not in the reference, or missing";
            }
            else if (added)
            {
                cause = lines?.WordGaps is null ? "a letter gap over the line, no word cluster shown" : "a letter gap over the line";
            }
            else if (lines?.WordGaps is null)
            {
                cause = "a word gap under the line, no word cluster shown yet";
            }
            else if (gap <= letterMs * 1.25)
            {
                cause = "a word gap as short as the sender's letter gaps";
            }
            else
            {
                cause = "a word gap under the line, the sender's word cluster shown";
            }

            places.Add(new Place(s.Stretch.Recording, s.Stretch.PitchHz, !added, around, gap, line, letterMs, wordMs, cause)
            {
                LetterTopMs = lines?.LetterGaps is { } lg ? Hamlet.RadioEngine.Cw.CwPatternGate.Percentile(lg, Hamlet.RadioEngine.Cw.CwPatternGate.LetterTopShare) * 1000 : double.NaN,
                WordBottomMs = lines?.WordGaps is { } wg ? Hamlet.RadioEngine.Cw.CwPatternGate.Percentile(wg, Hamlet.RadioEngine.Cw.CwPatternGate.WordBottomShare) * 1000 : double.NaN,
                BetweenMs = lines?.LetterGaps is { } lg2 ? Hamlet.RadioEngine.Cw.CwPatternGate.BetweenClusters(lg2, lines.WordGaps) * 1000 : double.NaN,
                WordCount = lines?.WordGaps?.Count ?? 0,
            });
        }

        return places;
    }

    /// <remarks>Every missing and added space on the board's totalled stretches, then the count by cause. Asserts nothing.</remarks>
    [Fact]
    public void EverySpaceByCause()
    {
        var board = Score(limits: false);
        var places = board.Stretches.Where(s => s.Stretch.Confidence >= Confidence.Medium).SelectMany(PlacesOf).ToList();
        var inv = CultureInfo.InvariantCulture;

        foreach (var p in places)
        {
            output.WriteLine(string.Create(inv, $"space | {(p.Missing ? "missing" : "added")} | `{p.Recording}` {p.PitchHz:0} | `{p.Around}` | gap {p.GapMs:0} ms | line {p.LineMs:0} | letter cluster {p.LetterMs:0} | word cluster {p.WordMs:0} | {p.Cause}"));
        }

        foreach (var g in places.GroupBy(p => (p.Missing, p.Cause)).OrderByDescending(g => g.Count()))
        {
            output.WriteLine($"cause | {(g.Key.Missing ? "missing" : "added")} | {g.Key.Cause} | {g.Count()}");
        }

        output.WriteLine($"board: spaces {board.SpacesRight} of {board.SpacesOutOf}, {board.SpacesAdded} added; places {places.Count(p => p.Missing)} missing, {places.Count(p => !p.Missing)} added");
    }

    /// <remarks>
    /// Work instruction 565, task 1, step 1: with the word line as it was, every space missed under the line and every space
    /// added over it, the sender's clusters at the moment the gap was judged, the line, and where the line between the clusters
    /// would be on the same clusters. Asserts nothing.
    /// </remarks>
    [Fact]
    public void TheGapsAgainstTheLines()
    {
        using var before = Hamlet.RadioEngine.Cw.CwRules.Off(Hamlet.RadioEngine.Cw.CwRules.BetweenClusters);
        var board = Score(limits: false);
        var places = board.Stretches.Where(s => s.Stretch.Confidence >= Confidence.Medium).SelectMany(PlacesOf)
            .Where(p => p.Cause.Contains("under the line", StringComparison.Ordinal) || p.Cause.Contains("over the line", StringComparison.Ordinal))
            .ToList();
        var inv = CultureInfo.InvariantCulture;

        output.WriteLine("| recording | around | kind | gap | letter centre | letter top | word centre | word bottom (gaps) | the line was | between the clusters | then |");
        output.WriteLine("|---|---|---|---|---|---|---|---|---|---|---|");

        foreach (var p in places)
        {
            var then = p.GapMs > p.BetweenMs ? "word" : "letter";
            var right = p.Missing ? then == "word" : then == "letter";

            output.WriteLine(string.Create(inv, $"| `{p.Recording}` | `{p.Around}` | {(p.Missing ? "word gap missed" : "letter gap spaced")} | {p.GapMs:0} | {p.LetterMs:0} | {p.LetterTopMs:0} | {(double.IsNaN(p.WordMs) ? "-" : p.WordMs.ToString("0", inv))} | {(double.IsNaN(p.WordBottomMs) ? "-" : $"{p.WordBottomMs:0} ({p.WordCount})")} | {p.LineMs:0} | {p.BetweenMs:0} | {then}{(right ? ", right" : ", still wrong")} |"));
        }

        output.WriteLine(string.Create(inv, $"board, the line as it was: spaces {board.SpacesRight} of {board.SpacesOutOf}, {board.SpacesAdded} added"));
    }
}
