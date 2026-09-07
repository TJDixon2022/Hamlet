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

        // And the literal above is the constant the composer actually defaults
        // to, so this test cannot pass by agreeing with a number nobody uses.
        Assert.Equal(expected, Ft8Composer.DefaultDrivePeak);
    }

    /// <summary>
    /// **The default is at least 6 dB below full scale, asserted as a number.**
    /// </summary>
    /// <remarks>
    /// A comment saying a default is conservative is not a property of the
    /// software. This is the bound the unit was required to leave, checked
    /// against the constant a caller who forgets the argument will get.
    /// </remarks>
    [Fact]
    public void TheDefaultDriveIsAtLeastSixDecibelsBelowFullScale()
    {
        var dbfs = 20.0 * Math.Log10(Ft8Composer.DefaultDrivePeak);

        _output.WriteLine($"default drive  : {Ft8Composer.DefaultDrivePeak:F6}");
        _output.WriteLine($"in dBFS        : {dbfs:F2}");
        _output.WriteLine($"below full scale by : {-dbfs:F2} dB");

        Assert.True(
            Ft8Composer.DefaultDrivePeak > 0.0f && Ft8Composer.DefaultDrivePeak <= 1.0f,
            $"the default drive {Ft8Composer.DefaultDrivePeak} is not a usable peak amplitude.");

        Assert.True(
            dbfs <= -6.0,
            $"the default drive is {dbfs:F2} dBFS, which is not at least 6 dB below full scale.");
    }

    /// <summary>
    /// **`PeakSample` equals the asked-for level, at more than one sample rate.**
    /// </summary>
    /// <remarks>
    /// <para>The tolerance is 0.005 of full scale and it is not a fudge: the peak
    /// of a sampled sine is the largest sample that happens to land near a crest,
    /// which is a hair under the true amplitude and depends on the rate. It is
    /// stated here rather than tuned until it passed.</para>
    /// <para>**Both compose routes**, because both funnel through the same
    /// private builder and the point of putting the scale there is that neither
    /// can be reached without it.</para>
    /// </remarks>
    [Theory]
    [InlineData(12000, 0.25f)]
    [InlineData(12000, 0.1f)]
    [InlineData(12000, 1.0f)]
    [InlineData(48000, 0.25f)]
    [InlineData(48000, 0.5f)]
    [InlineData(48000, 0.03f)]
    public void AComposedTransmissionCarriesThePeakItWasAskedFor(int rate, float drive)
    {
        const float tolerance = 0.005f;

        var signal = Ft8Composer.ComposeSignal(
            Message, rate, Ft8Composer.DefaultBaseFrequencyHz, drive);
        var slot = Ft8Composer.Compose(
            Message, rate, Ft8Composer.DefaultBaseFrequencyHz, drive);

        Assert.True(signal.Composed, signal.Explanation);
        Assert.True(slot.Composed, slot.Explanation);

        var signalPeak = signal.Transmission!.PeakSample;
        var slotPeak = slot.Transmission!.PeakSample;

        _output.WriteLine($"rate           : {rate}");
        _output.WriteLine($"drive asked    : {drive:F6}  ({20.0 * Math.Log10(drive):F2} dBFS)");
        _output.WriteLine(
            $"signal peak    : {signalPeak:F6}  ({20.0 * Math.Log10(signalPeak):F2} dBFS)");
        _output.WriteLine(
            $"slot peak      : {slotPeak:F6}  ({20.0 * Math.Log10(slotPeak):F2} dBFS)");
        _output.WriteLine($"tolerance      : {tolerance:F6}");

        Assert.True(
            Math.Abs(signalPeak - drive) <= tolerance,
            $"ComposeSignal was asked for a peak of {drive:F6} at {rate} Hz and produced "
            + $"{signalPeak:F6}.");

        Assert.True(
            Math.Abs(slotPeak - drive) <= tolerance,
            $"Compose was asked for a peak of {drive:F6} at {rate} Hz and produced "
            + $"{slotPeak:F6}.");

        // NOTHING LEAVES THE RAILS, whatever the drive. This is the property
        // WhatTheTransmissionLooksLikeAsAudioTests guards for the default, held
        // here across every level.
        foreach (var sample in signal.Transmission.Samples)
        {
            Assert.InRange(sample, -1.0f, 1.0f);
        }
    }

    /// <summary>
    /// **A level that is not a level is refused with words, not thrown.**
    /// </summary>
    /// <remarks>
    /// Beside the four refusals that were already on this path. A drive of zero
    /// is a keyed transmitter sending silence and a drive above full scale is
    /// distortion the sink would clamp; neither is quietly rounded into a level
    /// the operator did not set. **A refusal carries no transmission at all**, so
    /// there is no audio to read off it by forgetting a flag.
    /// </remarks>
    [Theory]
    [InlineData(0.0f)]
    [InlineData(-0.25f)]
    [InlineData(1.0001f)]
    [InlineData(2.0f)]
    [InlineData(float.NaN)]
    public void ADriveLevelThatIsNotALevelComesBackAsASentence(float drive)
    {
        var result = Ft8Composer.ComposeSignal(
            Message, Ft8Composer.DefaultSampleRate, Ft8Composer.DefaultBaseFrequencyHz, drive);

        _output.WriteLine($"drive asked    : {drive}");
        _output.WriteLine($"refusal        : {result.Refusal}");
        _output.WriteLine($"explanation    : {result.Explanation}");

        Assert.False(result.Composed);
        Assert.Null(result.Transmission);
        Assert.Equal(Ft8ComposeRefusal.DriveLevelRefused, result.Refusal);
        Assert.NotEqual(string.Empty, result.Explanation);

        // WORDS, NOT A CODE. The operator reads this sentence, so it says what is
        // wrong and where the usable range is.
        Assert.Contains("drive", result.Explanation, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// **The message still reads back as itself at the new default.**
    /// </summary>
    /// <remarks>
    /// Through the composer's own round trip - the one that already refuses any
    /// packing that does not reproduce its own words - rather than a second
    /// decoder written here. If the scale had reached the bits rather than the
    /// samples, this is what would say so.
    /// </remarks>
    [Theory]
    [InlineData("CQ KC3QIS FN00")]
    [InlineData("W1ABC KC3QIS -11")]
    [InlineData("W1ABC KC3QIS RR73")]
    [InlineData("CQ DX G4ABC IO91")]
    public void TheMessageStillReadsBackAsItselfAtTheNewDefault(string text)
    {
        var result = Ft8Composer.ComposeSignal(text);

        Assert.True(result.Composed, result.Explanation);

        var transmission = result.Transmission!;

        _output.WriteLine($"asked to send  : \"{transmission.Text}\"");
        _output.WriteLine($"reads back as  : \"{transmission.ReadsBackAs}\"");
        _output.WriteLine($"type           : {transmission.Type}");
        _output.WriteLine($"peak           : {transmission.PeakSample:F6}");

        Assert.Equal(text, transmission.Text);
        Assert.Equal(text, transmission.ReadsBackAs);
        Assert.False(transmission.CarriesHashedCallsign);
    }
}
