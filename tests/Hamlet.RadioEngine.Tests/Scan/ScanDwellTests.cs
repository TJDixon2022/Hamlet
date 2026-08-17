using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Scan;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Scan;

/// <summary>
/// How long a scan waits at one frequency, and what makes it leave
/// (HM-DEC-107, phase 7).
/// </summary>
/// <remarks>
/// <para>**THE LENGTH IS SET BY WHAT A CQ SOUNDS LIKE.** A relaxed call runs
/// eight to ten seconds and the caller then listens for about as long again, so
/// a dwell shorter than one cycle can sit entirely inside the silence between
/// two calls and report an empty frequency that had somebody on it.</para>
/// </remarks>
public sealed class ScanDwellTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the dwells are printed.</param>
    public ScanDwellTests(ITestOutputHelper output) => _output = output;

    private static CwCharacter Character(string text, double score = 0.9)
        => new(text, score >= 0.7 ? CwConfidence.High : CwConfidence.Low,
            score, ".-", 20, 18, TimeSpan.Zero);

    private static void Feed(ScanDwell dwell, string text)
    {
        foreach (var c in text)
        {
            dwell.Take(c == ' '
                ? new CwCharacter(
                    MorseAlphabet.WordGap, CwConfidence.High, 1, "", 20, 18,
                    TimeSpan.Zero)
                : Character(c.ToString()));
        }
    }

    /// <remarks>
    /// Proves HM-DEC-107 phase 7: **a dwell is at least one CQ cycle long.**
    /// Anything shorter lands in the gap between two calls and reports an empty
    /// frequency that had somebody on it, which is the specific failure a
    /// beginner reads as "the band is dead" (§0.0).
    /// </remarks>
    [Theory]
    [InlineData(0.5, ScanDwell.ShortestSeconds)]
    [InlineData(5, ScanDwell.ShortestSeconds)]
    [InlineData(15, 15)]
    [InlineData(60, ScanDwell.LongestSeconds)]
    public void ADwellStaysInsideTheWindowTheBriefSets(double asked, double expected)
    {
        var dwell = new ScanDwell(7_030_000, asked);

        _output.WriteLine($"asked {asked:0.#} s, got {dwell.Seconds:0.#} s");

        Assert.Equal(expected, dwell.Seconds);
    }

    /// <remarks>
    /// Proves HM-DEC-107 phase 7: **a call heard early ends the dwell early.**
    /// The question is settled, and going on listening costs the scan the rest
    /// of the band.
    /// </remarks>
    [Fact]
    public void ACallHeardEarlyEndsTheDwellThere()
    {
        var dwell = new ScanDwell(7_030_000);

        Assert.Equal(DwellAction.KeepListening, dwell.Decide(elapsedSeconds: 3));

        Feed(dwell, "CQ CQ DE W1AW");

        Assert.Equal(DwellAction.Stay, dwell.Decide(elapsedSeconds: 3));
        Assert.Equal(ScanStopReason.Calling, dwell.Verdict.Reason);

        _output.WriteLine(dwell.Describe());
    }

    /// <remarks>
    /// Proves HM-DEC-107 phase 7: **nothing recognized inside the whole window
    /// is the answer, and not grounds for another window.** A scan that keeps
    /// giving a frequency one more chance never reaches the rest of the band.
    /// </remarks>
    [Fact]
    public void AWindowThatRanOutMovesOn()
    {
        var dwell = new ScanDwell(7_030_000, seconds: 12);

        Feed(dwell, "XZ QJ WY");

        Assert.Equal(DwellAction.KeepListening, dwell.Decide(elapsedSeconds: 11.9));
        Assert.Equal(DwellAction.MoveOn, dwell.Decide(elapsedSeconds: 12));
    }

    /// <remarks>
    /// <para>Proves HM-DEC-107 phase 7 and §0.0.1: **a dwell that found nothing
    /// still reports.** A scan whose record holds only its stops cannot be told
    /// from one that never ran, and the frequencies it passed over are half of
    /// what it measured.</para>
    /// </remarks>
    [Fact]
    public void ADwellThatFoundNothingStillSaysWhereItWasAndWhatItHeard()
    {
        var quiet = new ScanDwell(7_028_000, seconds: 10);
        var line = quiet.Describe();

        _output.WriteLine(line);

        Assert.Contains("7.028", line, StringComparison.Ordinal);
        Assert.Contains("nothing resolved", line, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves HM-DEC-107 phase 7: the verdict's confidence reaches the dwell, so
    /// a stop made on dim letters can be drawn differently from one made on
    /// solid ones (§0.0).
    /// </remarks>
    [Fact]
    public void TheConfidenceReachesTheDwell()
    {
        var dwell = new ScanDwell(7_030_000);

        foreach (var c in "CQ DE W1AW")
        {
            dwell.Take(c == ' '
                ? new CwCharacter(
                    MorseAlphabet.WordGap, CwConfidence.High, 1, "", 20, 18,
                    TimeSpan.Zero)
                : Character(c.ToString(), score: 0.35));
        }

        _output.WriteLine(dwell.Describe());

        Assert.True(dwell.Verdict.Stop);
        Assert.True(dwell.Verdict.Confidence < 0.5);
        Assert.Contains("not at all sure", dwell.Describe(), StringComparison.Ordinal);
    }
}
