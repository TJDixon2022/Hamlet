using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Whether the spectral peak's error on a keyed signal is a bias that could be
/// corrected or a floor that cannot.
/// </summary>
/// <remarks>
/// <para>**THIS DECIDES WHETHER `N4L` COMES BACK** (work instruction 051, task 7).
/// `cw-2026-08-17-134712` holds a station at 500.09 Hz and `CwSpectralPeak`
/// measures 501.2. Unit 050 could not settle whether that 1.1 Hz is inherent —
/// keying spreads a tone into sidebands, so the peak of an averaged spectrum is
/// not exactly the carrier — or a systematic offset somebody could subtract.</para>
/// <para>**THE GROUND TRUTH IS SYNTHETIC, AND THAT IS THE POINT.** On a real
/// capture nobody knows the carrier to a tenth of a hertz, so an error against it
/// cannot be measured at all; that is why the question survived unit 050. A
/// generated signal has a carrier known exactly, and the keying that spreads it
/// is the same keying.</para>
/// <para>**MEASURE ONLY. NOTHING HERE CHANGES THE PEAK**, which the order forbids.
/// </para>
/// </remarks>
public sealed class IsTheHertzABiasOrAFloorTests
{
    private readonly ITestOutputHelper _output;

    public IsTheHertzABiasOrAFloorTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The error at one carrier, one speed, one message.</summary>
    private double? ErrorHz(double toneHz, int wpm, string text, double noise)
    {
        var audio = CwSignal.Generate(new CwSignalRequest(
            text, WordsPerMinute: wpm, ToneHz: toneHz, NoiseAmplitude: noise));

        var found = CwSpectralPeak.Find(audio.Samples, audio.SampleRate);

        return found is null ? null : found.Value - toneHz;
    }

    /// <summary>Is the error the same sign and size everywhere, or scattered?</summary>
    /// <remarks>
    /// **A CONSISTENT OFFSET IS A BIAS AND CORRECTABLE. SCATTER IS THE FLOOR.**
    /// The test asserts only that a measurement was taken; what it is for is the
    /// table it prints.
    /// </remarks>
    [Fact]
    public void TheErrorAcrossCarriersSpeedsAndDuties()
    {
        var errors = new List<double>();

        _output.WriteLine("carrierHz\twpm\tduty\terrorHz");

        foreach (var tone in new[] { 400.0, 500.09, 600.0, 700.0, 800.0 })
        {
            foreach (var wpm in new[] { 12, 18, 25, 30 })
            {
                // Two duty cycles: a message running almost continuously, and one
                // with long gaps. Keying sidebands depend on both.
                foreach (var (label, text) in new[]
                         {
                             ("busy", "CQ CQ CQ DE W1AW W1AW W1AW K"),
                             ("sparse", "E E E"),
                         })
                {
                    if (ErrorHz(tone, wpm, text, 0.03) is not { } error)
                    {
                        continue;
                    }

                    errors.Add(error);

                    _output.WriteLine(
                        $"{tone:0.00}\t{wpm}\t{label}\t{error:+0.00;-0.00}");
                }
            }
        }

        Assert.NotEmpty(errors);

        var mean = errors.Average();
        var spread = Math.Sqrt(
            errors.Sum(e => (e - mean) * (e - mean)) / errors.Count);
        var worst = errors.Max(Math.Abs);

        _output.WriteLine("");
        _output.WriteLine($"mean error   {mean:+0.000;-0.000} Hz");
        _output.WriteLine($"spread       {spread:0.000} Hz");
        _output.WriteLine($"worst        {worst:0.000} Hz");
        _output.WriteLine(
            Math.Abs(mean) > 2 * spread
                ? "SYSTEMATIC — the mean dominates the scatter, so it is a bias"
                : "SCATTERED — the scatter dominates the mean, so it is a floor");
    }

    /// <summary>
    /// The station this actually decides: 500.09 Hz, the fist on `134712`.
    /// </summary>
    /// <remarks>
    /// About twenty-two words a minute with a heavy fist, per HM-DEC-144. If the
    /// peak can find a synthetic carrier at 500.09 to well inside a hertz, then
    /// the 1.1 Hz seen on the real recording is not inherent to keying and
    /// something else on that capture explains it.
    /// </remarks>
    [Fact]
    public void TheCarrierThatRetiredN4L()
    {
        foreach (var wpm in new[] { 18, 22, 25 })
        {
            var error = ErrorHz(500.09, wpm, "N4L N4L N4L", 0.03);

            _output.WriteLine($"500.09 Hz at {wpm} WPM: {error:+0.000;-0.000} Hz");
        }

        var clean = ErrorHz(500.09, 22, "N4L N4L N4L", 0.0);

        _output.WriteLine($"500.09 Hz at 22 WPM, no noise: {clean:+0.000;-0.000} Hz");

        Assert.NotNull(clean);
    }
}
