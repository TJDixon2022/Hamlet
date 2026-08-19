using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The gate HM-DEC-142 makes the condition of shipping: with two classes, does a
/// callsign still come apart into characters?
/// </summary>
/// <remarks>
/// <para>**THE RULING SAYS IN TERMS THAT IT DOES NOT SHIP IF THIS FAILS.** A
/// transcript that runs `W4AWH` together, or splits it as `W4AW H`, is
/// HM-DEC-114's defect rather than an improvement, and is worse than the silence
/// it replaces. The word class going missing is only acceptable while the other
/// two still separate.</para>
/// <para>Measured on gaps rather than on audio, because that is where the
/// decision is made: `CwGapFit` is handed the sender's own gaps and returns the
/// boundary between a gap inside a character and a gap between two.</para>
/// </remarks>
public sealed class CwTwoClassGapTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the measured classes are printed.</param>
    public CwTwoClassGapTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// A callsign at twelve words a minute: element gaps of one dit, character
    /// gaps of three, and not one word gap anywhere.
    /// </summary>
    private static double[] Callsign(double ditMs)
    {
        // W4AWH: .--  ....-  .-  .--  ....
        // Written out as the gaps a keyer actually produces, which is what the
        // fit sees: dit-length inside a character, three dits between them.
        var gaps = new List<double>();

        foreach (var elements in new[] { 3, 5, 2, 3, 4, 3, 5, 2, 3, 4 })
        {
            for (var i = 1; i < elements; i++)
            {
                gaps.Add(ditMs);
            }

            gaps.Add(3 * ditMs);
        }

        return gaps.ToArray();
    }

    /// <remarks>
    /// **THE GATE.** Two heaps, no word gaps, and the boundary has to land
    /// between them: above every element gap and below every character gap. If it
    /// does not, characters run together or split, and HM-DEC-142 does not ship.
    /// </remarks>
    [Theory]
    [InlineData(100.0)]
    [InlineData(60.0)]
    [InlineData(48.0)]
    public void ACallsignWithNoWordGapsStillComesApart(double ditMs)
    {
        var gaps = Callsign(ditMs);
        var classes = CwGapFit.Fit(gaps, gaps.Length);

        Assert.NotNull(classes);

        var fit = classes!.Value;

        _output.WriteLine(
            $"dit {ditMs:F0} ms: element {fit.ElementCount} at {fit.ElementMs:F0}, "
            + $"character {fit.CharacterCount} at {fit.CharacterMs:F0}, "
            + $"word {fit.WordCount}, cut {fit.ElementCutMs:F0}");

        Assert.False(fit.WordSpacingMeasured);

        // Above every element gap and below every character gap, which is the
        // whole of "does a callsign come apart".
        Assert.True(
            fit.ElementCutMs > ditMs,
            $"the cut at {fit.ElementCutMs:F0} ms would split inside a character");

        Assert.True(
            fit.ElementCutMs < 3 * ditMs,
            $"the cut at {fit.ElementCutMs:F0} ms would run characters together");
    }

    /// <remarks>
    /// Proves the word boundary is out of reach rather than invented: no gap this
    /// sender produced can be classified as a word break, which asserts exactly
    /// what was measured.
    /// </remarks>
    [Fact]
    public void NoGapCanBeAWordBreakWhenNoneWasMeasured()
    {
        var gaps = Callsign(100);
        var fit = CwGapFit.Fit(gaps, gaps.Length)!.Value;

        Assert.True(double.IsPositiveInfinity(fit.CharacterCutMs));
        Assert.All(gaps, g => Assert.True(g < fit.CharacterCutMs));
    }

    /// <remarks>
    /// Proves the refusal that stays a refusal: without a character class there is
    /// no way to tell one letter from the next, and HM-DEC-142 keeps that as a
    /// refusal because it is the measurement genuinely failing.
    /// </remarks>
    [Fact]
    public void OneHeapIsStillARefusal()
    {
        var flat = Enumerable.Repeat(100.0, 40).ToArray();

        Assert.Null(CwGapFit.Fit(flat, flat.Length));
    }

    /// <remarks>
    /// Proves three heaps are untouched: a sender who does leave word gaps still
    /// gets word spacing, which is HM-DEC-115 working exactly as before.
    /// </remarks>
    [Fact]
    public void ASenderWhoLeavesWordGapsStillGetsThem()
    {
        var gaps = new List<double>();

        for (var word = 0; word < 4; word++)
        {
            foreach (var elements in new[] { 3, 4, 3 })
            {
                for (var i = 1; i < elements; i++)
                {
                    gaps.Add(100);
                }

                gaps.Add(300);
            }

            gaps.Add(700);
        }

        var fit = CwGapFit.Fit(gaps.ToArray(), gaps.Count)!.Value;

        Assert.True(fit.WordSpacingMeasured);
        Assert.True(fit.CharacterCutMs > 300 && fit.CharacterCutMs < 700);
    }
}
