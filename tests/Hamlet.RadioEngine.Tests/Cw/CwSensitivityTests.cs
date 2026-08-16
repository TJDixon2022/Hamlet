using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The decoder's sensitivity, measured rather than argued about (HM-DEC-088).
/// </summary>
public sealed class CwSensitivityTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the sweep is printed.</param>
    public CwSensitivityTests(ITestOutputHelper output) => _output = output;

    /// <remarks>
    /// <para>Proves HM-DEC-088. Sweeps the decoder from a comfortable signal down
    /// into noise it cannot possibly read, and prints the whole table so the
    /// figure in the report can be checked rather than believed.</para>
    /// <para>The assertion is a floor, so a change that reads less far down fails
    /// here rather than being discovered on the air. It is set below the measured
    /// figure on purpose: this is a regression guard, not a target to tune
    /// against, and a bar set exactly at today's number would fail on noise draws
    /// rather than on defects.</para>
    /// </remarks>
    [Fact]
    public void TheDecoderReadsAsFarDownAsItDidBefore()
    {
        var sweep = CwSensitivity.Sweep();
        var threshold = CwSensitivity.Threshold(sweep);

        _output.WriteLine(CwSensitivity.Report(sweep));
        _output.WriteLine("");
        _output.WriteLine(
            threshold is { } db
                ? $"reads 80% of the message down to {db:0.0} dB"
                : "never read 80% of the message at any level");

        Assert.NotNull(threshold);
        Assert.True(
            threshold <= 6.0,
            $"the decoder gave up at {threshold:0.0} dB, which is worse than it was");
    }

    /// <remarks>
    /// <para>Proves HM-DEC-088 and §0.0 together, and it is the more important of
    /// the two assertions. **Reading further into the noise is worthless if what
    /// comes back is invented.** Below the point where it can read, the decoder
    /// must go quiet rather than produce confident wrong letters.</para>
    /// <para>So across the whole sweep, at every level including ones far below
    /// anything readable, wrong characters stay a small share of what was sent. A
    /// change that buys sensitivity by guessing fails here.</para>
    /// </remarks>
    [Fact]
    public void ItGoesQuietRatherThanInventingLettersInTheNoise()
    {
        var sweep = CwSensitivity.Sweep();
        var worst = sweep.OrderByDescending(p => p.Wrong).First();

        _output.WriteLine(CwSensitivity.Report(sweep));
        _output.WriteLine("");
        _output.WriteLine($"worst wrong share {worst.Wrong:0.00} at {worst.SnrDb:0.0} dB");

        Assert.True(
            worst.Wrong <= 0.35,
            $"at {worst.SnrDb:0.0} dB it returned {worst.Wrong:0.00} of the message "
            + "as the wrong characters, which is a decoder guessing");
    }
}
