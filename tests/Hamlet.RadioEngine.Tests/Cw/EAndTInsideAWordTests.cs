using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **E AND T INSIDE A WORD ARE NOT NOISE** (work instruction 554, task 1, HM-DEC-258). Live on W1AW's fast code practice of
/// 2026-10-07 Hamlet printed `D CT` for DETECT, `D RMINED` for DETERMINED, `KILOM A S` for KILOMETERS and `A MPT` for
/// ATTEMPT: each time three one-mark letters in a row inside a word, dropped by the rule that drops three in a row as noise.
/// English puts ETE and TTE inside words all day; noise prints its one-mark letters alone.
/// </summary>
/// <remarks>Synthetic: 30 WPM, clean, through the filter with the bench's AGC, read through the app's chain.</remarks>
public sealed class EAndTInsideAWordTests(ITestOutputHelper output)
{
    private static string Read(string text, int seed)
    {
        var samples = AHandIsReadAgainstItselfTests.Under(
            "agc-filter", text, _ => new AHandIsReadAgainstItselfTests.Sending(30, 0), seed);

        return TheRecordingsScoreboardTests.ReadLive(samples, 8000, 625, 500).Text;
    }

    /// <remarks>Each line reads whole: its runs of E and T inside words print.</remarks>
    /// <param name="sent">The line.</param>
    [Theory]
    [InlineData("DETECT THE RADIATED SIGNAL")]
    [InlineData("PERFORMANCE IS DETERMINED BY")]
    [InlineData("HUNDREDS OF KILOMETERS")]
    [InlineData("INSPIRED TO ATTEMPT A TRANSVERTER")]
    [InlineData("A BETTER LETTER")]
    public void TheWordReadsWhole(string sent)
    {
        var read = Read(sent, 5540 + sent.Length);

        output.WriteLine($"`{sent}`: read `{read}`");

        Assert.Equal(sent, read);
    }

    /// <remarks>Five one-mark letters each printed alone as a word, the way noise prints, are still dropped.</remarks>
    [Fact]
    public void FiveLoneLettersAsWordsAreStillDropped()
    {
        var read = Read("E T E T E", 5554);

        output.WriteLine($"`E T E T E`: read `{read}`");

        Assert.Equal(string.Empty, read);
    }
}
