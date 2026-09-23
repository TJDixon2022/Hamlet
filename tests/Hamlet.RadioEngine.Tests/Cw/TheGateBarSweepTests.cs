using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Every settled character's own evidence, across every capture, so the
/// emission gate's bar is chosen from the spans (work instruction 408, 3.7).
/// </summary>
/// <remarks>
/// A printer. It asserts nothing and is on neither carry-forward line.
/// </remarks>
public sealed class TheGateBarSweepTests
{
    private static readonly double[] TotalBars = { 5, 10, 15, 20, 30, 50 };
    private static readonly double[] PerHopBars = { 1.0, 1.047, 1.25, 1.5, 2.0 };

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the sweep is printed.</param>
    public TheGateBarSweepTests(ITestOutputHelper output) => _output = output;

    private enum Kind
    {
        Named,
        BelowMargin,
        UnknownPattern,
    }

    private static Kind KindOf(CwCharacter c)
        => !c.IsUnreadable
            ? Kind.Named
            : MorseAlphabet.Lookup(c.Pattern) is null
                ? Kind.UnknownPattern
                : Kind.BelowMargin;

    /// <remarks>Prints the sweep; asserts nothing.</remarks>
    [Fact]
    public void EveryCaptureIsSwept()
    {
        var names = TheCapturesThatDecodeKeepDecodingTests.Floors
            .Select(row => (string)row[0])
            .Append(TheSeventeenThirtySevenCaptureTests.Name)
            .ToList();

        var anchors = TheAdjudicatedReadingsKeepReadingTests.All
            .Where(r => r.Retired.Length == 0)
            .ToDictionary(r => r.Name, r => r.Anchor, StringComparer.Ordinal);

        var all = new List<(string Name, Kind Kind, double Total, double PerHop, bool InAnchor, string Text)>();

        foreach (var name in names)
        {
            var settled = TheSeventeenThirtySevenCaptureTests.Settle(name);
            var text = string.Concat(settled.Select(c => c.Text));

            // Which settled characters sit inside the anchor's first match.
            var inAnchor = new bool[settled.Count];

            if (anchors.TryGetValue(name, out var anchor))
            {
                var at = text.IndexOf(anchor, StringComparison.Ordinal);
                var offset = 0;

                for (var i = 0; i < settled.Count; i++)
                {
                    var length = settled[i].Text.Length;

                    if (at >= 0 && offset >= at && offset < at + anchor.Length)
                    {
                        inAnchor[i] = true;
                    }

                    offset += length;
                }
            }

            for (var i = 0; i < settled.Count; i++)
            {
                var c = settled[i];

                if (c.IsWordGap)
                {
                    continue;
                }

                all.Add((name, KindOf(c), c.SpanLogLikelihoodRatio, c.SpanMarginForRecord, inAnchor[i], c.Text));
            }

            var mine = all.Where(r => r.Name == name).ToList();
            var named = mine.Where(r => r.Kind == Kind.Named).ToList();
            var margin = mine.Where(r => r.Kind == Kind.BelowMargin).ToList();
            var unknown = mine.Where(r => r.Kind == Kind.UnknownPattern).ToList();
            var anchored = mine.Where(r => r.InAnchor && r.Kind == Kind.Named).ToList();

            _output.WriteLine(
                $"rec | {name} | named {named.Count} min total {Min(named, r => r.Total)} min per hop {Min(named, r => r.PerHop)} "
                + $"| below-margin {margin.Count} max total {Max(margin, r => r.Total)} "
                + $"| unknown {unknown.Count} min total {Min(unknown, r => r.Total)} max total {Max(unknown, r => r.Total)} "
                + $"| anchor chars {anchored.Count} min total {Min(anchored, r => r.Total)} min per hop {Min(anchored, r => r.PerHop)}");
        }

        foreach (var bar in TotalBars)
        {
            Print("total", bar, all.Select(r => (r.Name, r.Kind, r.Total, r.InAnchor)).ToList());
        }

        foreach (var bar in PerHopBars)
        {
            Print("per-hop", bar, all.Select(r => (r.Name, r.Kind, r.PerHop, r.InAnchor)).ToList());
        }

        // The weakest named characters anywhere, so the bottom of the named
        // distribution can be read rather than summarised.
        foreach (var r in all.Where(r => r.Kind == Kind.Named).OrderBy(r => r.Total).Take(25))
        {
            _output.WriteLine(
                $"weak-named | {r.Name} | {r.Text} | total {r.Total:0.00} | per hop {r.PerHop:0.000} | anchor {r.InAnchor}");
        }

        foreach (var r in all.Where(r => r.Kind != Kind.Named).OrderByDescending(r => r.Total).Take(15))
        {
            _output.WriteLine(
                $"strong-placeholder | {r.Name} | {r.Kind} | total {r.Total:0.00} | per hop {r.PerHop:0.000}");
        }
    }

    private void Print(
        string scale, double bar, IReadOnlyList<(string Name, Kind Kind, double Score, bool InAnchor)> rows)
    {
        var namedLost = rows.Where(r => r.Kind == Kind.Named && r.Score < bar).ToList();
        var marginGone = rows.Count(r => r.Kind == Kind.BelowMargin && r.Score < bar);
        var unknownGone = rows.Count(r => r.Kind == Kind.UnknownPattern && r.Score < bar);
        var anchorLost = namedLost.Count(r => r.InAnchor);
        var recordingsLosing = namedLost.Select(r => r.Name).Distinct().Count();

        _output.WriteLine(
            $"bar | {scale} {bar} | named lost {namedLost.Count} over {recordingsLosing} recordings, "
            + $"anchor characters lost {anchorLost} | below-margin placeholders under it {marginGone} of "
            + $"{rows.Count(r => r.Kind == Kind.BelowMargin)} | unknown-pattern placeholders under it {unknownGone} of "
            + $"{rows.Count(r => r.Kind == Kind.UnknownPattern)}");
    }

    private static string Min<T>(IReadOnlyList<T> rows, Func<T, double> f)
        => rows.Count == 0 ? "-" : rows.Min(f).ToString("0.000");

    private static string Max<T>(IReadOnlyList<T> rows, Func<T, double> f)
        => rows.Count == 0 ? "-" : rows.Max(f).ToString("0.000");
}
