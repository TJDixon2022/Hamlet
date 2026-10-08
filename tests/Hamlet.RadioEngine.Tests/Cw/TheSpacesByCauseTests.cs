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
    internal sealed record Place(string Recording, double PitchHz, bool Missing, string Around, double GapMs, double LineMs, double LetterMs, double WordMs, string Cause);

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

            places.Add(new Place(s.Stretch.Recording, s.Stretch.PitchHz, !added, around, gap, line, letterMs, wordMs, cause));
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
}
