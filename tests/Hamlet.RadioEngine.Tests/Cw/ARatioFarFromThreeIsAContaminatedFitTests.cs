using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// What a fitted dah-to-dit ratio far from three actually means.
/// </summary>
/// <remarks>
/// <para>**A FIT RETURNING 3.94 IS FITTING SOMETHING THAT IS NOT DITS AND
/// DAHS**, and the question is which part of it is wrong. Measured here on
/// `cw-2026-08-17-013347`, whose dit and dah are read by hand at 100.4 and 274.3
/// milliseconds, a ratio of 2.73 (HM-DEC-145): through the same envelope the
/// keying meter uses, with the fixed twenty millisecond floor that instrument
/// carries, the fit returns **dit 65.7, dah 268.1, ratio 4.08**. **The dah is
/// right to within three per cent and the dit is thirty-five per cent short.**
/// </para>
/// <para>**SO THE LOW CLUSTER IS NOT DITS.** It is dits with everything between
/// twenty milliseconds and a real dit mixed into it, and a fixed floor of twenty
/// is far too low for a fist working at fourteen words a minute, where half a
/// dit is fifty. Lifting the floor to half the fitted unit and letting it settle
/// excludes twenty-one more runs and returns **dit 87.7, dah 268.1, ratio
/// 3.06**, with the two clusters standing more than twice as far apart in their
/// own scatter.</para>
/// <para>**AND IT DOES NOTHING WHERE THERE IS NOTHING TO DO.** On the recordings
/// whose fit is already sound it excludes one run and moves the ratio by three
/// hundredths, and on the two holding no keying at any pitch (HM-DEC-090) it
/// excludes none at all, so it cannot tidy an empty band into a fist.</para>
/// <para>**NOTHING IN `src` DOES THIS.** These tests are the measurement, kept so
/// a later session does not have to take it on trust, and so the claim can be
/// falsified. Whether the decoder's own fit should carry a floor derived from its
/// unit is a ruling that has not been made.</para>
/// </remarks>
public sealed class ARatioFarFromThreeIsAContaminatedFitTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the fits are printed.</param>
    public ARatioFarFromThreeIsAContaminatedFitTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>One fit over a set of key-down lengths.</summary>
    /// <param name="Dit">The short cluster's centre, in milliseconds.</param>
    /// <param name="Dah">The long cluster's centre, in milliseconds.</param>
    /// <param name="Kept">How many runs the fit was over.</param>
    private readonly record struct Fit(double Dit, double Dah, int Kept)
    {
        /// <summary>The sender's dah in the sender's own dits.</summary>
        public double Ratio => Dit > 0 ? Dah / Dit : 0;
    }

    private static (double Low, double High) TwoMeans(IReadOnlyList<double> values)
    {
        if (values.Count < 2)
        {
            return (0, 0);
        }

        var low = values.Min();
        var high = values.Max();

        for (var pass = 0; pass < 60; pass++)
        {
            var cut = (low + high) / 2;
            var next = values.Where(v => v < cut).DefaultIfEmpty(low).Average();
            var top = values.Where(v => v >= cut).DefaultIfEmpty(high).Average();

            if (Math.Abs(next - low) < 1e-9 && Math.Abs(top - high) < 1e-9)
            {
                break;
            }

            low = next;
            high = top;
        }

        return (low, high);
    }

    /// <summary>Every key-down length the keying envelope found, in milliseconds.</summary>
    private static IReadOnlyList<double> Runs(string relative)
    {
        var audio = WavAudio.Read(
            Path.Combine(CapturedSignalTests.Folder, relative));

        var best = KeyingEnvelope.Best(audio);

        return best?.Profile.RunsMs ?? Array.Empty<double>();
    }

    /// <summary>The fit with the instrument's own fixed floor.</summary>
    private static Fit AtTheFixedFloor(IReadOnlyList<double> runs)
    {
        var elements = runs
            .Where(r => r >= KeyingEnvelope.ShortestElementMs)
            .ToList();

        var (dit, dah) = TwoMeans(elements);

        return new Fit(dit, dah, elements.Count);
    }

    /// <summary>The fit with the floor derived from the unit it is fitting.</summary>
    /// <remarks>
    /// A dit is the shortest element anybody sends, so nothing under half of one
    /// is an element. The floor is a share of the fitted unit rather than a
    /// number of milliseconds, so it moves with the fist, and it settles because
    /// each pass fits a subset of the one before.
    /// </remarks>
    private static Fit AtAFloorFromTheUnit(IReadOnlyList<double> runs)
    {
        var kept = runs
            .Where(r => r >= KeyingEnvelope.ShortestElementMs)
            .ToList();

        var (dit, dah) = TwoMeans(kept);

        for (var pass = 0; pass < 6; pass++)
        {
            var next = kept.Where(r => r >= dit / 2).ToList();

            if (next.Count == kept.Count || next.Count < 12)
            {
                break;
            }

            kept = next;
            (dit, dah) = TwoMeans(kept);
        }

        return new Fit(dit, dah, kept.Count);
    }

    private void Print(string name, Fit fixedFloor, Fit fitted)
    {
        _output.WriteLine(
            $"{name,-42} fixed floor: dit {fixedFloor.Dit,5:0.0} dah {fixedFloor.Dah,6:0.0} "
            + $"ratio {fixedFloor.Ratio:0.00} over {fixedFloor.Kept} runs");
        _output.WriteLine(
            $"{"",-42} from the unit: dit {fitted.Dit,5:0.0} dah {fitted.Dah,6:0.0} "
            + $"ratio {fitted.Ratio:0.00} over {fitted.Kept} runs");
    }

    /// <remarks>
    /// <para>Proves the mechanism on the one recording whose timing is
    /// adjudicated: **the ratio is far from three because the low cluster is
    /// contaminated, and the dah was right all along.**</para>
    /// </remarks>
    [Fact]
    public void TheDitIsWrongAndTheDahIsRight()
    {
        var runs = Runs("cw-2026-08-17-013347.wav");
        var fixedFloor = AtTheFixedFloor(runs);
        var fitted = AtAFloorFromTheUnit(runs);

        Print("cw-2026-08-17-013347 (VA3VRR)", fixedFloor, fitted);

        // HM-DEC-145 read this fist by hand: dit 100.4 ms, dah 274.3, ratio 2.73.
        Assert.InRange(fixedFloor.Dah, 250, 290);
        Assert.InRange(fitted.Dah, 250, 290);

        // The fixed floor puts the dit a third short and the ratio past four.
        Assert.True(
            fixedFloor.Dit < 75,
            $"the fixed floor no longer shortens the dit; it read {fixedFloor.Dit:0.0}");

        Assert.True(
            fixedFloor.Ratio > 3.8,
            $"the fixed floor no longer distorts the ratio; it read {fixedFloor.Ratio:0.00}");

        // A floor taken from the unit recovers both.
        Assert.InRange(fitted.Dit, 80, 105);
        Assert.InRange(fitted.Ratio, 2.7, 3.3);
    }

    /// <remarks>
    /// <para>Proves the no-op: where the fit is already sound the floor from the
    /// unit changes almost nothing, so it is not a rule that reshapes every fit
    /// into the answer it wants.</para>
    /// </remarks>
    /// <param name="name">The recording.</param>
    [Theory]
    [InlineData("cw-2026-08-18-004507.wav")]
    [InlineData("unadjudicated/cw-2026-08-18-003016.wav")]
    [InlineData("unadjudicated/cw-2026-08-18-003126.wav")]
    public void WhereTheFitIsSoundItChangesAlmostNothing(string name)
    {
        var runs = Runs(name);
        var fixedFloor = AtTheFixedFloor(runs);
        var fitted = AtAFloorFromTheUnit(runs);

        Print(name, fixedFloor, fitted);

        Assert.True(
            Math.Abs(fitted.Ratio - fixedFloor.Ratio) < 0.1,
            $"the ratio moved from {fixedFloor.Ratio:0.00} to {fitted.Ratio:0.00}");

        Assert.True(
            fixedFloor.Kept - fitted.Kept <= 3,
            $"{fixedFloor.Kept - fitted.Kept} runs were excluded from a sound fit");
    }

    /// <remarks>
    /// <para>Proves §0.0: **it excludes nothing from a recording holding no
    /// keying at any pitch**, so it cannot tidy an empty band into something that
    /// looks like a fist (HM-DEC-090).</para>
    /// </remarks>
    /// <param name="name">The recording.</param>
    [Theory]
    [InlineData("unadjudicated/cw-2026-08-20-014854.wav")]
    [InlineData("unadjudicated/cw-2026-08-20-014935.wav")]
    public void ItTakesNothingOutOfAnEmptyBand(string name)
    {
        var runs = Runs(name);
        var fixedFloor = AtTheFixedFloor(runs);
        var fitted = AtAFloorFromTheUnit(runs);

        Print(name, fixedFloor, fitted);

        Assert.Equal(fixedFloor.Kept, fitted.Kept);
    }

    /// <remarks>
    /// <para>Proves the correlation the instruction rested on **does not hold in
    /// this repository**, which is why it was checked. The count of runs under
    /// twenty milliseconds does not predict a distorted ratio: the recording with
    /// the most of them by far fits at 2.87, and one with a quarter as many fits
    /// at 4.08.</para>
    /// </remarks>
    [Fact]
    public void TheCountOfShortRunsDoesNotPredictTheRatio()
    {
        var worst = Runs("cw-2026-08-17-013347.wav");
        var most = Runs("unadjudicated/cw-2026-08-20-014935.wav");

        var worstShort = worst.Count(r => r < KeyingEnvelope.ShortestElementMs);
        var mostShort = most.Count(r => r < KeyingEnvelope.ShortestElementMs);

        var worstFit = AtTheFixedFloor(worst);
        var mostFit = AtTheFixedFloor(most);

        _output.WriteLine(
            $"013347: {worstShort} runs under 20 ms, ratio {worstFit.Ratio:0.00}");
        _output.WriteLine(
            $"014935: {mostShort} runs under 20 ms, ratio {mostFit.Ratio:0.00}");

        Assert.True(mostShort > worstShort * 3);
        Assert.True(
            mostFit.Ratio < worstFit.Ratio,
            "the recording with far more short runs now fits a worse ratio, so "
            + "the count may predict the distortion after all");
    }
}
