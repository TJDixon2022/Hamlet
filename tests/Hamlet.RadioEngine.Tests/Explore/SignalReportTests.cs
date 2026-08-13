using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// Signal reports, made legible wherever they appear (HM-DEC-042).
/// </summary>
public sealed class SignalReportTests
{
    /// <remarks>
    /// Proves each band of the scale gets its own word, so a figure means
    /// something on the first read rather than after the operator has built
    /// their own scale from scratch.
    /// </remarks>
    [Theory]
    [InlineData(35, "strong")]
    [InlineData(20, "strong")]
    [InlineData(19, "fair")]
    [InlineData(10, "fair")]
    [InlineData(9, "workable")]
    [InlineData(5, "workable")]
    [InlineData(4, "weak")]
    [InlineData(-3, "weak")]
    public void EachBandOfTheScaleHasItsOwnWord(int db, string expected)
        => Assert.Equal(expected, SignalReport.Strength(db));

    /// <remarks>
    /// THE POINT OF THE FEATURE. Proves the number survives beside the word.
    /// Dropping it would leave "strong" unexplained, and keeping only the
    /// number leaves a newcomer no way to tell whether 24 is good news
    /// (§0.0.1).
    /// </remarks>
    [Fact]
    public void TheFigureTravelsWithItsMeaning()
    {
        var text = SignalReport.Describe(24);

        Assert.Contains("24", text, StringComparison.Ordinal);
        Assert.Contains("dB", text, StringComparison.Ordinal);
        Assert.Contains("strong", text, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves a negative figure reads as prose rather than as a stray minus
    /// sign. RBN reports these routinely, because a computer decodes below the
    /// noise floor where an ear cannot.
    /// </remarks>
    [Fact]
    public void ANegativeFigureStillReads()
    {
        var text = SignalReport.Describe(-6);

        Assert.Contains("-6 dB", text, StringComparison.Ordinal);
        Assert.Contains("weak", text, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the guidance never promises the operator will hear it. A skimmer
    /// measured its own receiver on its own antenna, and turning that into
    /// "you will hear this" is exactly the overreach HM-DEC-009 forbids.
    /// </remarks>
    [Theory]
    [InlineData(30)]
    [InlineData(15)]
    [InlineData(7)]
    [InlineData(-5)]
    public void TheGuidanceNeverPromisesYouWillHearIt(int db)
    {
        var text = SignalReport.WhatItMeansForYou(db);

        Assert.False(string.IsNullOrWhiteSpace(text));
        Assert.DoesNotContain("you will hear", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("you'll hear", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("guaranteed", text, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves the RST explanation says the thing nobody says out loud: that
    /// most of it is a polite fiction. Leaving that out would let a newcomer
    /// believe they had failed some measurement when they were handed a 59.
    /// </remarks>
    [Fact]
    public void TheRstExplanationAdmitsThePoliteFiction()
    {
        var text = SignalReport.RstExplained;

        Assert.Contains("readability", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("strength", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("tone", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("polite fiction", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("59", text, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the explanation obeys the voice standard it was written under
    /// (HM-DEC-040).
    /// </remarks>
    [Fact]
    public void TheExplanationObeysTheDashRule()
        => Assert.True(SignalReport.RstExplained.Count(c => c == '—') <= 1);

    /// <remarks>
    /// Proves it is pure (§5): the same figure always gives the same words.
    /// </remarks>
    [Fact]
    public void DescriptionIsDeterministic()
        => Assert.Equal(SignalReport.Describe(18), SignalReport.Describe(18));
}
