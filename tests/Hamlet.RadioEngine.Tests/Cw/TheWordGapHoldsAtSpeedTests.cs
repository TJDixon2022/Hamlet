using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **The word gap holds at speed on a weak signal** (work instruction numbered 509, run as unit 510,
/// task 7, HM-DEC-214).
/// </summary>
/// <remarks>
/// <para>**SYNTHETIC AUDIO WRITTEN HERE, NOTHING READ FROM DISK** (R96): unit 507's case, the call at
/// 35 WPM, 10 dB over the noise on unit 502's scale, seed 5090.</para>
/// <para>**WHAT WAS WRONG, MEASURED**: at 24 dB the reader measured the sender's letter gap at 110 ms
/// and its word boundary at 248, and read the call; at 10 dB it measured the letter gap at 46 ms - the
/// gap inside a letter - and the word boundary at 110, so every letter gap, 110 ms, read as a word.
/// Gaps inside letters had closed runs before the sender's dit was known, and they stayed among the
/// gaps between runs the letter gap is measured from.</para>
/// </remarks>
public sealed class TheWordGapHoldsAtSpeedTests
{
    private const string Call = "CQ CQ DE N0CALL N0CALL K";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the reading is printed.</param>
    public TheWordGapHoldsAtSpeedTests(ITestOutputHelper output) => _output = output;

    /// <remarks>Task 7: the call at 35 WPM and 10 dB reads `CQ CQ DE N0CALL N0CALL K`, not a letter a word.</remarks>
    [Fact]
    public void TheCallAt35WpmAndTenDecibelsReadsInWords()
    {
        var samples = CwSignal.Generate(new CwSignalRequest(
            Call, WordsPerMinute: 35, ToneHz: 625, SampleRate: 8000, Amplitude: ThePatternIsTheGateTests.Over(10),
            NoiseAmplitude: 0.04, LeadInSeconds: 3, TailSeconds: 3, Seed: 5090)).Samples;
        var r = ThePatternIsTheGateTests.Read(samples);

        _output.WriteLine($"35 WPM at 10 dB: {r.Candidates} candidates at the pitch, {r.Stood} stood, {r.Printed} printed; reads `{r.Text}`");

        Assert.Equal(Call, r.Text);
    }
}
