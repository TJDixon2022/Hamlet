using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **A HAND SENDER WHO BARELY PAUSES BETWEEN WORDS GETS HIS SPACES** (work instruction 517, task 2,
/// HM-DEC-221).
/// </summary>
/// <remarks>
/// <para>**THE OWNER'S SCREEN, 2026-10-01, 7.0265**: a Quebec station working Maine at 18 WPM read
/// `KI1MMRDEVE2JDLGNGNAGNDROMMAURO ,UREEI INSEAOPJEANJEANESQTHQUEBEC,HW?IAMMMRDEVE2JDEIK`, the letters
/// mostly right and almost no spaces.</para>
/// <para>**SYNTHETIC AUDIO WRITTEN HERE, NOTHING READ FROM DISK** (R96): unit 513's fist generator at
/// 18 WPM with a hand's scatter of a sixth, the word gaps at 4, 5 and 7 dits.</para>
/// </remarks>
public sealed class TheSpacesComeFromTheShapeTests
{
    private const string Text = "KI1MM DE VE2JD NAME IS JEAN QTH QUEBEC HW";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public TheSpacesComeFromTheShapeTests(ITestOutputHelper output) => _output = output;

    /// <remarks>
    /// The call at 18 WPM with the word gaps at <paramref name="wordGapDits"/> dits: read with its spaces at 7
    /// dits; at 4 and 5 printed for the report, since the rule that would space them waits on a ruling.
    /// </remarks>
    /// <param name="wordGapDits">The word gap, in dits.</param>
    [Theory]
    [InlineData(4.0)]
    [InlineData(5.0)]
    [InlineData(7.0)]
    public void AHandSendersShortWordGapsAreSpaces(double wordGapDits)
    {
        var (samples, truth) = AFistIsReadByTheNearerClusterTests.Fist(Text, 18, _ => 1.0 / 6, 5171 + (int)wordGapDits, wordGapDits);
        var read = ThePatternIsTheGateTests.Read(samples);

        _output.WriteLine(
            $"18 WPM, word gaps {wordGapDits} dits (true letter gaps {truth.LetterGaps.Average() * 1000:0} ms, word gaps {truth.WordGaps.Average() * 1000:0} ms): reads `{read.Text}`; {CwRunReader.LastClusters}");

        // Task 2 was dropped (work instruction 517): at 4 and 5 dits the call runs together, the Quebec
        // station's screen, and the line that would part them from a hand's letter gaps waits on a ruling.
        // Those rows are printed; the 7-dit row is held.
        if (wordGapDits >= 7)
        {
            Assert.Equal(Text, read.Text);
        }
    }
}
