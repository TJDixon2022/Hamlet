using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **Once the sender is known, its weak marks are found by its pattern** (work instruction 511,
/// task 2, measured first by unit 510, HM-DEC-215).
/// </summary>
/// <remarks>
/// **SYNTHETIC AUDIO WRITTEN HERE, NOTHING READ FROM DISK** (R96): the call at 20 WPM, 24 dB over the
/// noise on unit 502's scale, keyed without noise so its elements can be found, one dit taken 6 dB
/// down, and the noise added after.
/// </remarks>
public sealed class TheSendersPatternFindsItsMarksTests
{
    private const int Rate = 8000;
    private const double Pitch = 625;
    private const string Call = "CQ CQ DE N0CALL N0CALL K";

    /// <summary>The element weakened: the first dit of the first call's L, the 34th element sent.</summary>
    private const int Weakened = 33;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the reading is printed.</param>
    public TheSendersPatternFindsItsMarksTests(ITestOutputHelper output) => _output = output;

    /// <summary>The call with one element taken down by some decibels, and the noise added after.</summary>
    internal static float[] OneElementDown(int element, double downDb)
    {
        var keyed = CwSignal.Generate(new CwSignalRequest(
            Call, WordsPerMinute: 20, ToneHz: Pitch, SampleRate: Rate, Amplitude: ThePatternIsTheGateTests.Over(24),
            NoiseAmplitude: 0, LeadInSeconds: 3, TailSeconds: 3, Seed: 5120)).Samples;
        var noise = CwSignal.Generate(new CwSignalRequest(
            " ", SampleRate: Rate, Amplitude: 0, NoiseAmplitude: 0.04, LeadInSeconds: keyed.Length / (double)Rate,
            TailSeconds: 1, Seed: 5121)).Samples;
        var gain = (float)Math.Pow(10, -downDb / 20);
        var quietRun = Rate;
        var index = -1;
        var samples = new float[keyed.Length];

        for (var i = 0; i < keyed.Length; i++)
        {
            var on = Math.Abs(keyed[i]) > 1e-6;

            // An element starts after at least two milliseconds of silence.
            if (on && quietRun > Rate / 500)
            {
                index++;
            }

            quietRun = on ? 0 : Math.Min(quietRun + 1, Rate);
            samples[i] = (index == element && quietRun < Rate / 500 ? keyed[i] * gain : keyed[i]) + noise[i];
        }

        return samples;
    }

    /// <remarks>
    /// Task 2: the strong call with the first dit of its first L 6 dB down reads whole, the dit found by
    /// the sender's pattern at the sender's pitch.
    /// </remarks>
    [Fact]
    public void AWeakDitInsideALetterIsFoundByTheSendersPattern()
    {
        var whole = ThePatternIsTheGateTests.Read(OneElementDown(Weakened, 0));
        var weak = ThePatternIsTheGateTests.Read(OneElementDown(Weakened, 6));

        _output.WriteLine($"as sent       : {whole.Candidates} candidates at the pitch, {whole.Stood} stood, {whole.Printed} printed; reads `{whole.Text}`");
        _output.WriteLine($"one dit -6 dB : {weak.Candidates} candidates at the pitch, {weak.Stood} stood, {weak.Printed} printed; reads `{weak.Text}`");

        Assert.Equal(Call, whole.Text);
        Assert.Equal(Call, weak.Text);

        // Unit 510's helper counted the first element as none, so its case took the L's dah down, one
        // element later; that case is read too, as a dah inside a letter.
        var dah = ThePatternIsTheGateTests.Read(OneElementDown(Weakened + 1, 6));

        _output.WriteLine($"its dah -6 dB : {dah.Candidates} candidates at the pitch, {dah.Stood} stood, {dah.Printed} printed; reads `{dah.Text}`");

        Assert.Equal(Call, dah.Text);
    }
}
