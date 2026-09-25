using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;
using static Hamlet.RadioEngine.Tests.Cw.WhatTheStrayLettersRestOnTests;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Every emitted character's span against the median span of the characters
/// around it, and whether that ratio parts the letters the key calls added from
/// the ones it calls right (work instruction 428, task 1; PHASE_PLAN.md criterion
/// 3.6, HM-DEC-181).
/// </summary>
/// <remarks>
/// <para>**A PRINTER. IT ASSERTS NOTHING.** HM-DEC-181 rules that the window and
/// the fraction are measured, not assumed, and chosen from the traced
/// distributions. This is the trace, and nothing is decided in it.</para>
/// <para>**THE FIGURE IS THE RAW SPAN**, <see cref="CwCharacter.SpanLogLikelihoodRatio"/>,
/// the one the sheet prints first under `spanLlr` and the one R71's bar reads.</para>
/// <para>**THE WINDOW IS THE NAMED CHARACTERS BESIDE IT, NOT A STRETCH OF TIME**
/// (author's, overrulable). A stretch of seconds holds three letters at a slow
/// fist and ten at a fast one; counting characters compares a letter with the
/// same number of neighbors whatever the speed. Word gaps and placeholders carry
/// no span and are skipped; the character itself is left out of its own median.
/// Where the recording ends on one side, the window takes what there is on that
/// side and does not borrow from the other.</para>
/// </remarks>
public sealed class WhatTheNeighborsSayTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tables are printed.</param>
    public WhatTheNeighborsSayTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// How many named characters each side make the window task 1 traces.
    /// </summary>
    /// <remarks>
    /// Three each side, six in all (author's, overrulable). A median of six stands
    /// when one or two of them are themselves litter, and three each side is less
    /// than a callsign, so the window stays inside the passage the letter was sent
    /// in, where the band's condition is the same.
    /// </remarks>
    internal const int Side = 3;

    /// <summary>One named character, its label, and the median of its window.</summary>
    /// <param name="Name">The recording.</param>
    /// <param name="IsKeyed">Whether the recording has an inferred key.</param>
    /// <param name="Label">What the key made of it.</param>
    /// <param name="Character">What settled.</param>
    /// <param name="Neighbors">The median figure of the named characters in its window.</param>
    /// <param name="Span">Its own figure: the raw span, or the raw span per mark.</param>
    internal sealed record Placed(string Name, bool IsKeyed, Label Label, CwCharacter Character, double Neighbors, double Span)
    {
        /// <summary>One element, a dit or a dah.</summary>
        public bool Single => Character.Pattern.Length == 1;

        /// <summary>Its span over its neighbors' median.</summary>
        public double Ratio => Neighbors > 0 ? Span / Neighbors : double.NaN;

        /// <summary>Which heap it belongs to.</summary>
        public string Heap => !IsKeyed ? "unkeyed" : Label switch
        {
            Label.Right => "right",
            Label.Wrong => "wrong",
            Label.Added => "added",
            _ => "keyed, unscored",
        };
    }

    /// <summary>The figure a character is traced on: its raw span, or that over its marks.</summary>
    /// <param name="c">What settled.</param>
    /// <param name="perMark">Divide by the dits and dahs in its pattern.</param>
    /// <returns>The figure.</returns>
    internal static double Figure(CwCharacter c, bool perMark)
        => perMark
            ? c.SpanLogLikelihoodRatio / Math.Max(1, c.Pattern.Count(e => e is '.' or '-'))
            : c.SpanLogLikelihoodRatio;

    /// <summary>The median figure of up to <paramref name="side"/> named characters either side of each named one.</summary>
    /// <param name="settled">What settled, word gaps included.</param>
    /// <param name="side">How many named characters each side.</param>
    /// <param name="perMark">Trace the raw span per mark rather than the raw span.</param>
    /// <returns>One median per settled character; NaN for a word gap or a placeholder.</returns>
    internal static double[] NeighborMedians(IReadOnlyList<CwCharacter> settled, int side, bool perMark = false)
    {
        var named = Enumerable.Range(0, settled.Count)
            .Where(i => IsNamed(settled[i]) && !double.IsNaN(settled[i].SpanLogLikelihoodRatio))
            .ToList();
        var medians = Enumerable.Repeat(double.NaN, settled.Count).ToArray();

        for (var n = 0; n < named.Count; n++)
        {
            medians[named[n]] = Median(
                named.Take(n).TakeLast(side).Concat(named.Skip(n + 1).Take(side))
                    .Select(j => Figure(settled[j], perMark)));
        }

        return medians;
    }

    /// <summary>The middle value, the mean of the two middle ones for an even count.</summary>
    internal static double Median(IEnumerable<double> values)
    {
        var sorted = values.Where(v => !double.IsNaN(v)).OrderBy(v => v).ToArray();

        if (sorted.Length == 0)
        {
            return double.NaN;
        }

        var middle = sorted.Length / 2;

        return sorted.Length % 2 == 1 ? sorted[middle] : (sorted[middle - 1] + sorted[middle]) / 2;
    }

    private static string Spread(IEnumerable<double> values)
    {
        var sorted = values.Where(v => !double.IsNaN(v)).OrderBy(v => v).ToArray();

        if (sorted.Length == 0)
        {
            return "none";
        }

        double At(double share) => sorted[(int)Math.Round(share * (sorted.Length - 1))];

        return $"n {sorted.Length}, min {sorted[0]:0.000}, tenth {At(0.1):0.000}, quartile {At(0.25):0.000}, "
               + $"median {At(0.5):0.000}, quartile {At(0.75):0.000}, ninetieth {At(0.9):0.000}, max {sorted[^1]:0.000}";
    }

    /// <summary>Every named character on the 51 capture rows and 17:37, labeled and placed in its window.</summary>
    /// <param name="side">How many named characters each side.</param>
    /// <param name="edits">The keyed edits the labels were taken from.</param>
    /// <param name="length">The keyed scored length.</param>
    /// <param name="perMark">Trace the raw span per mark rather than the raw span.</param>
    /// <returns>Every named character with a measured span.</returns>
    internal static List<Placed> PlaceAll(int side, out int edits, out int length, bool perMark = false)
    {
        var names = TheCapturesThatDecodeKeepDecodingTests.Floors.Select(row => (string)row[0])
            .Append(TheSeventeenThirtySevenCaptureTests.Name)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var placed = new List<Placed>();

        edits = 0;
        length = 0;

        foreach (var name in names)
        {
            var settled = TheSeventeenThirtySevenCaptureTests.Settle(name);
            var keyed = KeyedRecordings.SingleOrDefault(k => k.Name == name);
            var scores = keyed is null ? Array.Empty<CwScore>() : keyed.Score(CwReading.Of(settled));
            var labels = Labels(settled, scores);
            var medians = NeighborMedians(settled, side, perMark);

            edits += scores.Sum(s => s.Edits);
            length += scores.Sum(s => s.ScoredLength);

            for (var i = 0; i < settled.Count; i++)
            {
                if (IsNamed(settled[i]) && !double.IsNaN(settled[i].SpanLogLikelihoodRatio))
                {
                    placed.Add(new Placed(
                        name, keyed is not null, labels[i], settled[i], medians[i], Figure(settled[i], perMark)));
                }
            }
        }

        return placed;
    }

    /// <summary>Prints the character table, both distributions, the heaps and the ladder for one window.</summary>
    /// <param name="side">How many named characters each side.</param>
    /// <param name="perMark">Trace the raw span per mark rather than the raw span.</param>
    internal void Trace(int side, bool perMark = false)
    {
        var placed = PlaceAll(side, out var edits, out var length, perMark);

        _output.WriteLine(
            $"check | {edits} edits over {length} characters against inferred keys, {KeyedRecordings.Count} keyed recordings | "
            + $"window {side} named characters each side, the character itself left out | "
            + $"figure {(perMark ? "raw span per mark" : "raw span")} | "
            + $"named with a measured span: {placed.Count}, on keyed recordings {placed.Count(p => p.IsKeyed)}");

        // ---- EVERY CHARACTER ON THE KEYED RECORDINGS ----
        _output.WriteLine("");
        _output.WriteLine("CHARACTERS | every named character on the keyed recordings, in order");
        _output.WriteLine("char | recording | at | heap | text | pattern | span | neighbors' median | ratio");

        foreach (var p in placed.Where(p => p.IsKeyed))
        {
            _output.WriteLine(
                $"char | {p.Name} | {p.Character.At.TotalSeconds:0.000} s | {p.Heap} | `{p.Character.Text}` | {p.Character.Pattern} | "
                + $"{p.Span:0.0} | {p.Neighbors:0.0} | {p.Ratio:0.000}");
        }

        var heaps = new[] { "added", "right", "wrong", "keyed, unscored", "unkeyed" };
        var cuts = new (string Label, Func<Placed, bool> In)[]
        {
            ("all named", _ => true),
            ("single-element, E and T", p => p.Single),
            ("longer", p => !p.Single),
        };

        // ---- THE DISTRIBUTIONS ----
        _output.WriteLine("");
        _output.WriteLine("DISTRIBUTIONS | the ratio, span over the neighbors' median, by heap");

        foreach (var (cut, @in) in cuts)
        {
            foreach (var heap in heaps)
            {
                _output.WriteLine(
                    $"spread | {cut} | {heap} | {Spread(placed.Where(p => p.Heap == heap && @in(p)).Select(p => p.Ratio))}");
            }
        }

        // A histogram on eighth decades, the heaps side by side.
        foreach (var (cut, @in) in cuts)
        {
            var of = placed.Where(@in).Where(p => !double.IsNaN(p.Ratio) && p.Ratio > 0).ToList();

            _output.WriteLine($"heap | {cut} | from | to | " + string.Join(" | ", heaps));

            var low = Math.Floor(Math.Log10(of.Min(p => p.Ratio)) * 8) / 8;
            var high = Math.Log10(of.Max(p => p.Ratio));

            for (var q = low; q <= high; q += 0.125)
            {
                var from = Math.Pow(10, q);
                var to = Math.Pow(10, q + 0.125);

                _output.WriteLine(
                    $"heap | {cut} | {from:0.000} | {to:0.000} | "
                    + string.Join(" | ", heaps.Select(h => of.Count(p => p.Heap == h && p.Ratio >= from && p.Ratio < to))));
            }
        }

        // ---- EVERY ADDED ONE, AND WHAT SITS AT OR UNDER ITS RATIO ----
        _output.WriteLine("");
        _output.WriteLine("LADDER | a bar at each added character's ratio: what falls at or under it, by heap");

        foreach (var (cut, @in) in cuts.Take(2))
        {
            foreach (var a in placed.Where(p => p.Heap == "added" && @in(p)).OrderBy(p => p.Ratio))
            {
                var bar = a.Ratio;

                _output.WriteLine(
                    $"ladder | {cut} | bar {bar:0.000} at {a.Name} {a.Character.At.TotalSeconds:0.000} s `{a.Character.Text}` span {a.Span:0.0} over {a.Neighbors:0.0} | "
                    + string.Join(" | ", heaps.Select(h => $"{h} {placed.Count(p => p.Heap == h && @in(p) && p.Ratio <= bar)} of {placed.Count(p => p.Heap == h && @in(p))}")));
            }
        }

        // The lowest right characters, named, since they are what a bar would cost first.
        _output.WriteLine("");
        _output.WriteLine("LOWEST | the twenty lowest ratios the key calls right, and the twenty lowest anywhere on an unkeyed row");

        foreach (var p in placed.Where(p => p.Heap == "right").OrderBy(p => p.Ratio).Take(20)
                     .Concat(placed.Where(p => p.Heap == "unkeyed").OrderBy(p => p.Ratio).Take(20)))
        {
            _output.WriteLine(
                $"lowest | {p.Heap} | {p.Name} | {p.Character.At.TotalSeconds:0.000} s | `{p.Character.Text}` | {p.Character.Pattern} | "
                + $"span {p.Span:0.0} | neighbors {p.Neighbors:0.0} | ratio {p.Ratio:0.000}");
        }

        // ---- THE VERDICT THE COUNTS GIVE ----
        _output.WriteLine("");

        foreach (var (cut, @in) in cuts.Take(2))
        {
            var added = placed.Where(p => p.Heap == "added" && @in(p)).OrderBy(p => p.Ratio).ToList();
            var others = placed.Where(p => p.Heap != "added" && @in(p) && !double.IsNaN(p.Ratio)).ToList();
            var right = placed.Where(p => p.Heap == "right" && @in(p) && !double.IsNaN(p.Ratio)).ToList();
            var cleanOfAll = added.TakeWhile(a => !others.Any(o => o.Ratio <= a.Ratio)).Count();
            var cleanOfRight = added.TakeWhile(a => !right.Any(r => r.Ratio <= a.Ratio)).Count();

            _output.WriteLine(
                $"verdict | {cut} | added {added.Count} | taken before any right one falls at or under the bar: {cleanOfRight} | "
                + $"taken before any other character of any heap does: {cleanOfAll} | "
                + $"lowest right {(right.Count == 0 ? double.NaN : right.Min(r => r.Ratio)):0.000}, "
                + $"highest added {(added.Count == 0 ? double.NaN : added.Max(a => a.Ratio)):0.000}");
        }
    }

    /// <remarks>
    /// Proves task 1 of work instruction 428: every named character on the keyed
    /// recordings with its raw span, the median raw span of the three named
    /// characters each side and the ratio; the ratio's distribution for the added,
    /// right, wrong, keyed-but-unscored and unkeyed heaps, over all characters and
    /// over single elements alone; a ladder at every added character's ratio; the
    /// lowest right and unkeyed ratios by name. Asserts nothing.
    /// </remarks>
    [Fact]
    public void EachLetterAgainstTheLettersAroundIt()
        => Trace(Side);

    /// <summary>
    /// How many named characters each side make the second window, task 2's.
    /// </summary>
    /// <remarks>
    /// Eight each side, sixteen in all (author's, overrulable). Task 1's trace put
    /// three of 17:37's added letters within a second of each other, at 26.250,
    /// 26.515 and 26.820 s, with more litter beside them, so a window of three each
    /// side was half filled with the thing it was meant to stand against. Eight is
    /// longer than any run of added or wrong letters on the keyed recordings, so
    /// the median is the passage's letters and not the run's.
    /// </remarks>
    internal const int WideSide = 8;

    /// <remarks>
    /// Proves task 2 of work instruction 428: task 1's trace again, every table,
    /// with <see cref="WideSide"/> named characters each side in place of
    /// <see cref="Side"/>. Asserts nothing.
    /// </remarks>
    [Fact]
    public void EachLetterAgainstAWiderWindow()
        => Trace(WideSide);

    /// <remarks>
    /// Proves task 3 of work instruction 428: task 2's trace on the raw span per
    /// mark, the raw span over the dits and dahs in the character's pattern, for
    /// the character and for its neighbors alike. **Why a second figure** (author's,
    /// overrulable): the raw span sums over a character's marks, so a lone right `E`
    /// carries a quarter of what a right `H` beside it carries on the same signal,
    /// and tasks 1 and 2 found the right single elements sitting lower against their
    /// neighbors than the added ones. Per mark, a right `E` and a right `H` on one
    /// signal stand level. Asserts nothing.
    /// </remarks>
    [Fact]
    public void EachMarkAgainstTheMarksAroundIt()
        => Trace(WideSide, perMark: true);
}
