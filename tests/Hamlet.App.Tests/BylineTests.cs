using Hamlet.App;
using Xunit;

namespace Hamlet.App.Tests;

/// <summary>
/// The rotating byline under the wordmark (HM-DEC-039).
/// </summary>
public sealed class BylineTests
{
    private static IReadOnlyList<Byline> Lines(int count)
        => Enumerable.Range(0, count)
            .Select(i => new Byline($"line {i}", "Hamlet"))
            .ToList();

    /// <remarks>
    /// Proves the file ships and parses. It is embedded rather than read off
    /// disk, so a missing file would be a build problem and not a runtime one —
    /// but only if something checks.
    /// </remarks>
    [Fact]
    public void TheFileShipsAndParses()
    {
        Assert.NotEmpty(Bylines.All);
        Assert.All(Bylines.All, b => Assert.False(string.IsNullOrWhiteSpace(b.Text)));
        Assert.All(Bylines.All, b => Assert.False(string.IsNullOrWhiteSpace(b.Source)));
    }

    /// <remarks>
    /// Proves the same line never comes up twice running. It is meant to be a
    /// small surprise, and a surprise that repeats is a fixture.
    /// </remarks>
    [Fact]
    public void TheSameLineNeverComesUpTwiceRunning()
    {
        var lines = Bylines.All;
        var last = -1;

        // Every possible draw from every possible previous state, rather than
        // a sample: with a bounded set there is no reason to leave it to luck.
        for (var round = 0; round < lines.Count * 3; round++)
        {
            for (var draw = 0; draw < lines.Count - 1; draw++)
            {
                var picked = Bylines.Pick(lines, last, _ => draw);

                Assert.NotNull(picked);
                Assert.NotEqual(last, picked!.Value.Index);
            }

            last = Bylines.Pick(lines, last, n => round % n)!.Value.Index;
        }
    }

    /// <remarks>
    /// Proves every line is reachable. A draw that skipped past the previous
    /// index without wrapping would quietly make the last line unreachable, and
    /// nobody would ever notice.
    /// </remarks>
    [Fact]
    public void EveryLineIsReachable()
    {
        var lines = Lines(5);
        var seen = new HashSet<int>();

        for (var last = -1; last < lines.Count; last++)
        {
            for (var draw = 0; draw < lines.Count - 1; draw++)
            {
                var picked = Bylines.Pick(lines, last, _ => draw);
                seen.Add(picked!.Value.Index);
            }
        }

        Assert.Equal(lines.Count, seen.Count);
    }

    /// <remarks>
    /// Proves the index returned always addresses the line returned, so the
    /// value saved to settings is the one that will be avoided next time.
    /// </remarks>
    [Fact]
    public void TheIndexMatchesTheLine()
    {
        var lines = Lines(7);

        for (var last = -1; last < lines.Count; last++)
        {
            for (var draw = 0; draw < lines.Count - 1; draw++)
            {
                var picked = Bylines.Pick(lines, last, _ => draw)!.Value;
                Assert.Same(lines[picked.Index], picked.Line);
            }
        }
    }

    /// <remarks>
    /// Proves a one-line file terminates. Avoiding the repeat by re-rolling
    /// would spin forever here, and a file with one line in it is exactly the
    /// sort of thing somebody hand-edits.
    /// </remarks>
    [Fact]
    public void ASingleLineFileDoesNotSpin()
    {
        var picked = Bylines.Pick(Lines(1), 0, _ => 0);

        Assert.NotNull(picked);
        Assert.Equal(0, picked!.Value.Index);
    }

    /// <remarks>
    /// Proves an empty set yields no byline rather than an exception or a
    /// placeholder. This runs while the main window is being constructed, so a
    /// decorative feature that could stop the app opening would be a
    /// spectacularly bad trade (§8).
    /// </remarks>
    [Fact]
    public void NoLinesYieldsNoBylineAndNoCrash()
        => Assert.Null(Bylines.Pick(Array.Empty<Byline>(), -1, _ => 0));

    /// <remarks>
    /// Proves a stored index from a longer file — a settings.json carried
    /// across an edit that removed lines — does not throw or return garbage.
    /// </remarks>
    [Theory]
    [InlineData(-5)]
    [InlineData(99)]
    [InlineData(int.MaxValue)]
    public void AnOutOfRangeStoredIndexIsHarmless(int stored)
    {
        var lines = Lines(4);
        var picked = Bylines.Pick(lines, stored, _ => 2);

        Assert.NotNull(picked);
        Assert.InRange(picked!.Value.Index, 0, lines.Count - 1);
    }

    /// <remarks>
    /// Proves every line names the play it was bent out of, since that is what
    /// the hover tooltip shows and a blank one would be a tooltip that appears
    /// and says nothing.
    /// </remarks>
    [Fact]
    public void EveryLineNamesItsPlay()
        => Assert.All(Bylines.All, b => Assert.NotEqual("", b.Source));

    /// <summary>
    /// The longest line that still fits on one row at the smallest window
    /// Hamlet opens at.
    /// </summary>
    /// <remarks>
    /// The window's MinWidth is 900 and the header keeps about 860 of it after
    /// margins. At 12.5px italic the average advance is a shade under six
    /// pixels, so roughly 145 characters reach the edge. 130 leaves room for a
    /// line of unusually wide letters without being so tight that it starts
    /// editing Tim's jokes for him.
    /// </remarks>
    private const int LongestThatFits = 130;

    /// <remarks>
    /// Proves the lines sit under the wordmark on one row at the narrowest
    /// window. Nothing enforces this at layout time, so it is enforced here —
    /// a line that wrapped would push the whole band row down.
    /// </remarks>
    [Fact]
    public void TheLinesFitUnderTheWordmark()
        => Assert.All(
            Bylines.All,
            b => Assert.True(
                b.Text.Length <= LongestThatFits,
                $"'{b.Text}' is {b.Text.Length} characters"));
}
