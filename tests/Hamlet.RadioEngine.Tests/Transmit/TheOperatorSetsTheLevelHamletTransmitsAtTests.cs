using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Transmit;

/// <summary>
/// **The level Hamlet transmits at is the level it was asked for.**
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THIS WOULD HAVE CAUGHT.** Tim's first transmission goes
/// into the radio's USB input at 0 dBFS, a heavily overdriven FT8 signal goes out
/// over other people's band, and there is no control in Hamlet to turn it down.
/// Measured at unit 265 task 1 and written down in
/// `docs/unit265-the-level-trace.md`: <c>Ft8Waveform.cs:199</c> builds a sine of
/// unit amplitude, there is no amplitude parameter on either compose route,
/// nothing between the composer and the endpoint multiplies a sample by anything
/// except the PCM16 conversion constant, and <c>PeakSample</c> read back
/// 1.000000 at both 12000 and 48000 Hz.</para>
/// <para>**Nothing here says anything about the radio.** `SHACK_FACTS.md`
/// FACT-004: no radio has ever been attached to this machine, and every figure
/// here is arithmetic over an array in this process. What the IC-7300's USB
/// modulation input expects is not in this repository and is not claimed
/// here.</para>
/// </remarks>
public sealed class TheOperatorSetsTheLevelHamletTransmitsAtTests
{
    private readonly ITestOutputHelper _output;

    public TheOperatorSetsTheLevelHamletTransmitsAtTests(ITestOutputHelper output) =>
        _output = output;

    private const string Message = "CQ KC3QIS FN00";

    /// <summary>
    /// **The composed peak is the level that was configured, and not full scale.**
    /// </summary>
    /// <remarks>
    /// Written to be watched red against the tree as it stood: with no drive
    /// anywhere on the compose route this reports a peak of about 1.0 against an
    /// expectation of 0.25. The literal is deliberate at the red - the constant
    /// it becomes does not exist yet, and an assertion that will not compile is a
    /// build error rather than a red.
    /// </remarks>
    [Fact]
    public void WhatTheComposerProducesIsAtTheDriveLevelAndNotAtFullScale()
    {
        const float expected = 0.25f;

        var composed = Ft8Composer.ComposeSignal(Message);

        Assert.True(composed.Composed, composed.Explanation);

        var peak = composed.Transmission!.PeakSample;

        _output.WriteLine($"message        : {Message}");
        _output.WriteLine($"drive asked    : {expected:F6}  ({20.0 * Math.Log10(expected):F2} dBFS)");
        _output.WriteLine($"peak composed  : {peak:F6}  ({20.0 * Math.Log10(peak):F2} dBFS)");

        Assert.True(
            Math.Abs(peak - expected) <= 0.01f,
            $"the composer was configured for a peak of {expected:F6} "
            + $"({20.0 * Math.Log10(expected):F2} dBFS) and produced {peak:F6} "
            + $"({20.0 * Math.Log10(peak):F2} dBFS).");
    }
}
