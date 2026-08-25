using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// While the keying meter is coasting through a gap it prints the verdict and no
/// numbers.
/// </summary>
/// <remarks>
/// <para>**THE FIGURES BESIDE A HELD VERDICT ARE MEASUREMENTS OF THE GAP.** The
/// meter holds its word for fifteen windows so it does not drop to "no keying"
/// between overs, which is right and is not changed here. What it also did was
/// keep printing the newest window's numbers beside the held word, and the
/// newest window during a gap is the gap: on the evening of 2026-08-20 that put
/// `9 ms key down` on screen and in a capture sidecar for a station the other
/// recordings of the same operator measure at about ninety milliseconds. **A
/// work order was written from that reading.**</para>
/// <para>The verdict is the thing being held and it is still worth printing. The
/// numbers are not, because they are not about what the word says (§0.0).</para>
/// </remarks>
public sealed class AHeldVerdictPrintsNoMeasurementsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the lines are printed.</param>
    public AHeldVerdictPrintsNoMeasurementsTests(ITestOutputHelper output)
        => _output = output;

    /// <remarks>
    /// Proves it for the record a person reads months later, which is the more
    /// dangerous of the two places.
    /// </remarks>
    [Fact]
    public void TheRecordLineCarriesNoFiguresWhileItIsHeld()
    {
        var held = new KeyingReading(
            KeyingVerdict.Keying, 825, 9, 21, 40, 0.3, Held: true);

        var line = MainWindowViewModel.KeyingLine(held);

        _output.WriteLine(line);

        Assert.Contains("keying", line, StringComparison.Ordinal);
        Assert.Contains("held through a quiet stretch", line, StringComparison.Ordinal);
        Assert.DoesNotContain("9 ms", line, StringComparison.Ordinal);
        Assert.DoesNotContain("825", line, StringComparison.Ordinal);
        Assert.DoesNotContain("key-downs", line, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the no-op: a fresh reading is unchanged and still carries every
    /// figure, because that is what the operator watches while he turns a knob.
    /// </remarks>
    [Fact]
    public void AFreshReadingStillCarriesEveryFigure()
    {
        var fresh = new KeyingReading(
            KeyingVerdict.Keying, 825, 6, 22, 40, 0.3, Held: false,
            ElementMedianMs: 91);

        var line = MainWindowViewModel.KeyingLine(fresh);

        _output.WriteLine(line);

        Assert.Contains("825 Hz", line, StringComparison.Ordinal);
        Assert.Contains("91 ms key down", line, StringComparison.Ordinal);
        Assert.Contains("22 dB swing", line, StringComparison.Ordinal);
        Assert.Contains("40 key-downs", line, StringComparison.Ordinal);

        // **AND THE SIX MILLISECONDS IT WAS ALSO HANDED IS NOWHERE ON THE LINE.**
        // That figure is the middle of every threshold crossing, which the
        // verdict is calibrated against and which nobody can send: a dit at sixty
        // words a minute is twenty milliseconds and sixty is faster than a hand
        // goes. It stays in the reading because the verdict rests on it, and it
        // stays out of the record because a reader takes what is written beside a
        // recording for a fact about the station (§0.0).
        Assert.DoesNotContain("6 ms", line, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves §0.0 where the meter has nothing usable to say: a sweep that found
    /// no key-down long enough to be an element says that, rather than printing
    /// the middle of its own chatter as though somebody had keyed it.
    /// </remarks>
    [Fact]
    public void APitchWithNoElementLengthKeyDownSaysSoRatherThanPrintingChatter()
    {
        var chatter = new KeyingReading(
            KeyingVerdict.Keying, 575, 4, 17, 449, 0.2, Held: false,
            ElementMedianMs: 0);

        var line = MainWindowViewModel.KeyingLine(chatter);

        _output.WriteLine(line);

        Assert.Contains("no key-down was element length", line, StringComparison.Ordinal);
        Assert.DoesNotContain("4 ms", line, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves nothing measured stays nothing measured, rather than becoming a
    /// held verdict with a reassuring sentence attached (§0.0).
    /// </remarks>
    [Fact]
    public void NothingMeasuredStaysNothingMeasured()
        => Assert.Equal("not measured", MainWindowViewModel.KeyingLine(KeyingReading.None));
}
