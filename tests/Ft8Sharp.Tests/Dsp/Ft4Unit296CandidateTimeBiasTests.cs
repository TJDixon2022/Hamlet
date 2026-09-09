using Ft8Sharp.Deep;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>Unit 296 task 6 — what moving FT4's analysis grid did to
/// <see cref="Ft4DeepSignalToNoise.Ft4CandidateTimeBiasSeconds"/>, which is a shipped constant that
/// was measured on a decoder that no longer exists.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>THIS IS A FINDING AND NOT A FIX, AND THE DISTINCTION IS DELIBERATE.</b> The constant lives in
/// <c>src/Ft8Sharp.Deep/</c>, which work instruction 296 forbids this unit to touch: <em>the
/// permitted <c>src/</c> change tonight is <c>Ft4WaterfallGeometry.cs</c> and task 2a's one
/// sentence, and that is all of it.</em> So this test measures the discrepancy, names it, and
/// guards the property that has to hold for the estimator to keep working — it does not correct
/// anything, and the correction is named in unit 296's report as wanting a ruling.
/// </para>
/// <para>
/// <b>WHAT THE DISCREPANCY IS.</b> <c>Ft4CandidateTimeBiasSeconds</c> is <c>-0.048</c> s, minus one
/// FT4 symbol period, and unit 294 measured exactly that on every one of its on-grid trials at
/// <c>Ft4WaterfallGeometry</c>'s then-default 2 analyses per symbol. At the 4 unit 296 adopted the
/// measured bias is <b>-0.060 s</b> — one further sub-block, because a sub-block at 4 is 144
/// samples rather than 288. The estimator's window therefore starts <b>12 ms late</b>, and
/// <see cref="Ft4DeepSignalToNoise.TimeSearchSeconds"/> is <b>±12 ms</b>, so the alignment search
/// recovers it <em>exactly at its own limit and with no margin left</em>. That it still works is
/// visible in <c>Ft4Unit294SnrAgreementTests</c>'s figures — 0.56 dB mean absolute error over 1048
/// messages, better than the 0.58 it read before — and it works for a reason nobody chose.
/// </para>
/// <para>
/// <b>AND UNIT 294'S OWN TEST IS RED BECAUSE OF THIS, AND IT IS LEFT RED ON PURPOSE.</b> It asserts
/// the measured bias equals the constant, which was a true statement about one geometry and is now
/// a false one about the shipping geometry. Weakening that assertion to green would hide a shipped
/// constant that no longer matches the decoder it describes, and that is the fault this project
/// keeps a card against. The red is unit 296's, it is not inherited, and it is named.
/// </para>
/// </remarks>
public class Ft4Unit296CandidateTimeBiasTests(ITestOutputHelper output)
{
    private const int Rate = Ft4Unit296PlacementLattice.Rate;

    /// <summary>
    /// <b>The bias measured at both grids, with the shipped constant and the search window beside
    /// it.</b>
    /// </summary>
    /// <remarks>
    /// <b>The breakage it catches</b> is the estimator's alignment window drifting outside its own
    /// search range, which would put a wrong signal-to-noise ratio into another operator's log with
    /// nothing failing. It is measured in clear air over a small sample because a bias is a
    /// quantisation fact rather than a statistic: it is the same value on every trial, or it is not
    /// a bias.
    /// </remarks>
    [Fact]
    public void TheCandidateTimeBiasMovedWithTheGridAndTheSearchWindowOnlyJustCoversIt()
    {
        var corpus = Ft4Unit296PlacementLattice.Subset(Ft4RoundTripCorpus.Build(), 10);
        var onGrid = Ft4Unit296PlacementLattice.Build().Single(p => p.OnGrid);

        output.WriteLine("THE SHIPPED CONSTANT AND THE WINDOW IT HAS TO LAND IN");
        output.WriteLine(
            $"  Ft4DeepSignalToNoise.Ft4CandidateTimeBiasSeconds  "
            + $"{Ft4DeepSignalToNoise.Ft4CandidateTimeBiasSeconds:F6} s");
        output.WriteLine(
            $"  Ft4DeepSignalToNoise.TimeSearchSeconds            "
            + $"+/-{Ft4DeepSignalToNoise.TimeSearchSeconds:F6} s");
        output.WriteLine(string.Empty);
        output.WriteLine(
            $"{"grid",-8}{"sub-block",-11}{"trials",-8}{"distinct biases",-34}{"error",-12}margin");

        foreach (var (time, frequency, label) in new[]
                 {
                     (2, 2, "2x2"),
                     (Ft4WaterfallGeometry.DefaultTimeOversampling,
                         Ft4WaterfallGeometry.DefaultFrequencyOversampling,
                         "4x2"),
                 })
        {
            var geometry = new Ft4WaterfallGeometry(
                timeOversampling: time, frequencyOversampling: frequency);
            var decoder = new Ft4SlotDecoder(geometry);
            var biases = new List<double>();

            foreach (var entry in corpus)
            {
                var symbols = Ft4SymbolEncoder.Encode(entry.Message);
                var signal = Ft4Waveform.Synthesize(symbols, Rate, (float)onGrid.FrequencyHz);
                var slot = Ft4Unit296PlacementLattice.Slot(signal, onGrid.LeadSamples);
                var result = decoder.Decode(slot);

                foreach (var message in result.Messages)
                {
                    if (string.Equals(message.Text, entry.Text, StringComparison.Ordinal))
                    {
                        // The true start is the lead this test wrote, so the bias is a subtraction
                        // with no estimator anywhere in the loop.
                        biases.Add(
                            (onGrid.LeadSamples / (double)Rate) - message.TimeSeconds(geometry));
                    }
                }
            }

            Assert.NotEmpty(biases);

            var distinct = biases.Distinct().OrderBy(b => b).ToList();
            var worst = biases
                .Select(b => Math.Abs(b - Ft4DeepSignalToNoise.Ft4CandidateTimeBiasSeconds))
                .Max();

            output.WriteLine(
                $"{label,-8}{geometry.SubblockSize,-11}{biases.Count,-8}"
                + $"{string.Join(", ", distinct.Select(b => $"{b:F6}")),-34}"
                + $"{worst * 1000.0,-12:F1}{(Ft4DeepSignalToNoise.TimeSearchSeconds - worst) * 1000.0:F1} ms");

            // THE PROPERTY THAT HAS TO HOLD, AND IT IS NOT THE ONE UNIT 294 ASSERTED. The estimator
            // does not need the constant to be exactly right; it needs the truth to be inside the
            // window the constant opens. At 4 analyses per symbol that is true with nothing to
            // spare, which is the finding rather than the reassurance.
            // The tolerance is one microsecond, which is a thousandth of the window and is there
            // for the float-derived symbol period alone: at 4x2 the error IS the window, to eleven
            // nanoseconds. This assertion is green at zero margin and red the moment anything makes
            // it worse, which is the only useful shape for a guard on a discrepancy that is
            // reported rather than fixed.
            Assert.True(
                worst <= Ft4DeepSignalToNoise.TimeSearchSeconds + 1e-6,
                $"at {label} the candidate time bias is off the shipped constant by "
                + $"{worst * 1000.0:F1} ms and the alignment search only reaches "
                + $"{Ft4DeepSignalToNoise.TimeSearchSeconds * 1000.0:F1} ms, so the estimator would "
                + "be measuring a window the signal is not in and would put a wrong report in "
                + "somebody else's log with nothing failing.");
        }

        output.WriteLine(string.Empty);
        output.WriteLine("WHAT THIS MEANS, PLAINLY");
        output.WriteLine(
            "  The constant is minus one FT4 symbol period and it was right at two analyses per");
        output.WriteLine(
            "  symbol. At four it is short by exactly one sub-block, 144 samples, 12 ms - and the");
        output.WriteLine(
            "  alignment search reaches exactly 12 ms. It works, at its own limit, for a reason");
        output.WriteLine(
            "  nobody chose. Correcting it means deriving the bias from the geometry inside");
        output.WriteLine(
            "  Ft4DeepSignalToNoise, which is a src/Ft8Sharp.Deep/ file work instruction 296");
        output.WriteLine(
            "  forbids this unit to touch. It is reported and it is the next unit's.");
    }
}
