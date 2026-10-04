using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **Narrowness reads the band the filter gives it** (work instruction 514, task 2, HM-DEC-218).
/// </summary>
/// <remarks>
/// <para>**ACROSS THE WEEK**: stations at 350, 500, 550 and 700 Hz reached the reader with the detector on
/// another bin; stations at 600 read. The owner runs FIL2 at 500 Hz centred on his 600 Hz pitch.</para>
/// <para>**WHAT IT FOUND** (unit 514): narrowness turns away 0 to 3 of about 70 candidates at every
/// pitch, and the detector's bins already end at the passband, so it is not the fault. Through the filter,
/// 700 Hz stands all 65 marks and keys none, so the reader prints nothing: the bins at and above the tone
/// form bars and do not pair them, while the same station unfiltered keys 64. Red at 700 until that is
/// found.</para>
/// <para>**SYNTHETIC AUDIO WRITTEN HERE, NOTHING READ FROM DISK** (R96): the call at 20 WPM, 24 dB over
/// the noise on unit 502's scale, then the signal and the noise together through the radio's filter as
/// this test approximates it - two band-pass sections at 600 Hz with a Q of 1.2, a 500 Hz passband - and
/// the detector told that passband, as the application tells it from the rig state.</para>
/// </remarks>
public sealed class NarrownessReadsTheFiltersBandTests
{
    private const int Rate = 8000;
    private const string Call = "CQ CQ DE N0CALL N0CALL K";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the table is printed.</param>
    public NarrownessReadsTheFiltersBandTests(ITestOutputHelper output) => _output = output;

    /// <summary>The audio through the filter: two band-pass sections, constant peak gain, at 600 Hz with Q 1.2.</summary>
    internal static float[] ThroughTheFilter(float[] samples, double centreHz = 600, double widthHz = 500)
    {
        var output = samples.ToArray();

        for (var section = 0; section < 2; section++)
        {
            var w0 = 2 * Math.PI * centreHz / Rate;
            var alpha = Math.Sin(w0) / (2 * (centreHz / widthHz));
            var a0 = 1 + alpha;
            double b0 = alpha / a0, b2 = -alpha / a0, a1 = -2 * Math.Cos(w0) / a0, a2 = (1 - alpha) / a0;
            double x1 = 0, x2 = 0, y1 = 0, y2 = 0;

            for (var i = 0; i < output.Length; i++)
            {
                var x = output[i];
                var y = (b0 * x) + (b2 * x2) - (a1 * y1) - (a2 * y2);

                x2 = x1;
                x1 = x;
                y2 = y1;
                y1 = y;
                output[i] = (float)y;
            }
        }

        return output;
    }

    internal static float[] CallAt(double pitchHz, int seed)
        => ThroughTheFilter(CwSignal.Generate(new CwSignalRequest(
            Call, WordsPerMinute: 20, ToneHz: pitchHz, SampleRate: Rate, Amplitude: ThePatternIsTheGateTests.Over(24),
            NoiseAmplitude: 0.04, LeadInSeconds: 3, TailSeconds: 3, Seed: seed)).Samples);

    /// <remarks>
    /// Task 2: the call at 425, 500, 600, 700 and 775 Hz through a 500 Hz passband centred on 600 reads
    /// whole; the candidates at its pitch and what stood are reported. The narrowness test itself came out in work
    /// instruction 534 (HM-DEC-238).
    /// </remarks>
    /// <param name="pitchHz">The station's pitch.</param>
    [Theory]
    [InlineData(425.0)]
    [InlineData(500.0)]
    [InlineData(600.0)]
    [InlineData(700.0)]
    [InlineData(775.0)]
    public void ACallAnywhereInThePassbandReads(double pitchHz)
    {
        var samples = CallAt(pitchHz, 5150 + (int)pitchHz);
        var with = ThePatternIsTheGateTests.Read(samples, pitchHz, d => d.SetPassband(600, 500));

        _output.WriteLine(
            $"{pitchHz:0} Hz: {with.Candidates} candidates; "
            + $"{with.Stood} stood, {with.Printed} printed; reads `{with.Text}`");

        Assert.Equal(Call, with.Text);
    }
}
