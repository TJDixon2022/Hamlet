using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **A HAND SENDER WHO PAUSES FIVE DITS BETWEEN WORDS GETS HIS SPACES** (work instruction 517, task 2, HM-DEC-221;
/// work instruction 525, task 1, HM-DEC-229).
/// </summary>
/// <remarks>
/// <para>**THE OWNER'S SCREEN, 2026-10-01, 7.0265**: a Quebec station working Maine at 18 WPM read
/// `KI1MMRDEVE2JDLGNGNAGNDROMMAURO ,UREEI INSEAOPJEANJEANESQTHQUEBEC,HW?IAMMMRDEVE2JDEIK`, the letters
/// mostly right and almost no spaces.</para>
/// <para>**SYNTHETIC AUDIO WRITTEN HERE, NOTHING READ FROM DISK** (R96): unit 513's fist generator at
/// 18 WPM with a hand's scatter of a sixth, the word gaps at 4, 5 and 7 dits.</para>
/// <para>**THE WORD GAP IS FIVE DITS, DECIDED IN THE GATE** (work instruction 525). The generator scatters the word gaps
/// by a sixth too, so the 5-dit row's word gaps arrive at 4.2 to 5.8 dits, and those under five read as letter gaps;
/// the owner chose the literal five dits over a line between three and five, 2026-10-02, so that row is printed.</para>
/// </remarks>
public sealed class TheSpacesComeFromTheShapeTests
{
    private const string Text = "KI1MM DE VE2JD NAME IS JEAN QTH QUEBEC HW";

    private static readonly string[] Callsigns = ["KI1MM", "VE2JD"];

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public TheSpacesComeFromTheShapeTests(ITestOutputHelper output) => _output = output;

    /// <remarks>
    /// The call at 18 WPM with the word gaps at <paramref name="wordGapDits"/> dits: no space inside either callsign at
    /// any of them (HM-DEC-229, §0.0); read whole at 7 dits; at 4 and 5 printed for the report.
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
        var dit = 1.2 / 18;

        _output.WriteLine(
            $"18 WPM, word gaps {wordGapDits} dits (true letter gaps up to {truth.LetterGaps.Max() / dit:0.00} dits, word gaps {string.Join(" ", truth.WordGaps.Select(g => (g / dit).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)))} dits): reads `{read.Text}`; {CwRunReader.LastClusters}");

        foreach (var call in Callsigns)
        {
            for (var k = 1; k < call.Length; k++)
            {
                Assert.DoesNotContain(call[..k] + " " + call[k..], read.Text, StringComparison.Ordinal);
            }
        }

        if (wordGapDits >= 7)
        {
            Assert.Equal(Text, read.Text);
        }
    }
}
