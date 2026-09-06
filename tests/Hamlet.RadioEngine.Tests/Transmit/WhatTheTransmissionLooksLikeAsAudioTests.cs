using Ft8Sharp.Encode;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Transmit;

/// <summary>
/// Level, clipping and timing, measured off the arrays the seam produces.
/// </summary>
/// <remarks>
/// <para>**Everything here is read from the samples, not from arithmetic in a
/// document.** The lengths are counted, the peak is found by walking the array,
/// and where the signal sits in the slot is found by looking for where the silence
/// stops — then held against what the port says it should be, so the two are
/// checked against each other rather than one being assumed.</para>
/// <para>**Nothing here says anything about the radio.** `SHACK_FACTS.md`
/// FACT-004 rules that the IC-7300's USB codec is not present on this machine and
/// that no measurement of this machine's audio endpoints says anything about it.
/// What the radio's input expects is in `output.md` and in `docs/`, sourced, with
/// the unknowns named — it is not measured here and it could not be.</para>
/// </remarks>
public sealed class WhatTheTransmissionLooksLikeAsAudioTests
{
    private readonly ITestOutputHelper _output;

    public WhatTheTransmissionLooksLikeAsAudioTests(ITestOutputHelper output) => _output = output;

    private const string Message = "CQ KC3QIS FN00";

    /// <summary>
    /// **The peak sample, the headroom, and that nothing can leave full scale.**
    /// </summary>
    /// <remarks>
    /// The synthesis is a sine of unit amplitude, so full scale is what it reaches
    /// and there is no headroom above it by construction. What matters is that
    /// nothing *exceeds* it — a sample outside -1..+1 would be clipped by
    /// <c>Ft8Waveform.ToPcm16</c> and would be distortion on the air. Measured over
    /// a spread of messages, since the peak depends on the symbol sequence.
    /// </remarks>
    [Theory]
    [InlineData(12000)]
    [InlineData(48000)]
    public void NothingTheSeamProducesLeavesFullScale(int rate)
    {
        string[] messages =
        [
            "CQ KC3QIS FN00", "CQ K1ABC FN42", "K1ABC W9XYZ -11", "K1ABC W9XYZ RR73",
            "K1ABC W9XYZ 73", "CQ DX G4ABC IO91", "TNX BOB 73 GL", "CQ PJ4/K1ABC",
        ];

        var worstPeak = 0.0f;
        var worstMessage = string.Empty;
        var outsideRange = 0;

        _output.WriteLine($"{"message",-20} {"peak",10} {"min",10} {"max",10}");

        foreach (var text in messages)
        {
            var composed = Ft8Composer.Compose(text, rate);
            Assert.True(composed.Composed, composed.Explanation);
            var samples = composed.Transmission!.Samples;

            var min = float.PositiveInfinity;
            var max = float.NegativeInfinity;
            foreach (var sample in samples)
            {
                min = Math.Min(min, sample);
                max = Math.Max(max, sample);
                if (sample is < -1.0f or > 1.0f)
                {
                    outsideRange++;
                }
            }

            var peak = composed.Transmission.PeakSample;
            if (peak > worstPeak)
            {
                worstPeak = peak;
                worstMessage = text;
            }

            _output.WriteLine($"{text,-20} {peak,10:F6} {min,10:F6} {max,10:F6}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"rate                       : {rate}");
        _output.WriteLine($"largest peak over the set  : {worstPeak:F6} (\"{worstMessage}\")");
        _output.WriteLine($"headroom below full scale  : {1.0f - worstPeak:F6}");
        _output.WriteLine($"samples outside -1 to +1   : {outsideRange}");

        Assert.Equal(0, outsideRange);
        Assert.True(worstPeak <= 1.0f, $"the peak is {worstPeak} and full scale is 1.0.");

        // A sine of unit amplitude sampled at these rates reaches essentially all
        // of full scale, so a peak far below it would mean the synthesis had
        // quietly changed and the level had gone with it.
        Assert.True(worstPeak > 0.99f, $"the peak is {worstPeak}, which is not a full-scale sine.");
    }

    /// <summary>
    /// **The sixteen-bit conversion of what the seam produces never wraps.**
    /// </summary>
    /// <remarks>
    /// <c>Ft8Waveform.ToPcm16</c> clips before it scales and rounds by adding a
    /// half and truncating toward zero, which is upstream's and is not symmetric.
    /// This is that conversion applied to the seam's own audio rather than to
    /// contrived input, so it says what a caller who asks for PCM16 will actually
    /// get.
    /// </remarks>
    [Fact]
    public void TheSixteenBitConversionOfARealTransmissionStaysInRange()
    {
        var composed = Ft8Composer.Compose(Message);
        Assert.True(composed.Composed, composed.Explanation);

        var pcm = Ft8Waveform.ToPcm16(composed.Transmission!.Samples);

        short min = 0;
        short max = 0;
        foreach (var sample in pcm)
        {
            min = Math.Min(min, sample);
            max = Math.Max(max, sample);
        }

        _output.WriteLine($"samples : {pcm.Length}");
        _output.WriteLine($"min     : {min}");
        _output.WriteLine($"max     : {max}");
        _output.WriteLine($"scale   : 32767 (upstream's, not 32768)");

        Assert.True(min >= short.MinValue);
        Assert.True(max <= short.MaxValue);

        // Nothing wrapped: a wrap turns a positive peak into a large negative one,
        // so the extremes being of opposite sign and comparable size is the check.
        Assert.True(max > 32000, $"max is {max}; a full-scale sine should reach the top of the range.");
        Assert.True(min < -32000, $"min is {min}; a full-scale sine should reach the bottom of it.");
    }

    /// <summary>
    /// **The ends are ramped rather than stepped, and both ramps are measured.**
    /// </summary>
    /// <remarks>
    /// The port raises and lowers a raised-cosine envelope over an eighth of a
    /// symbol at each end of the signal. Without it a transmission begins on a step
    /// and splatters across the band. Measured by taking the largest sample in each
    /// eighth-of-a-symbol window at the two ends of the signal and comparing it
    /// against the body.
    /// </remarks>
    [Fact]
    public void TheTransmissionRampsUpAndDownRatherThanStartingOnAStep()
    {
        var composed = Ft8Composer.Compose(Message);
        Assert.True(composed.Composed, composed.Explanation);

        var slot = composed.Transmission!.Samples;
        var padding = Ft8Waveform.PaddingSampleCount(Ft8Waveform.DefaultSampleRate);
        var signalLength = Ft8Waveform.SampleCount(Ft8Waveform.DefaultSampleRate);
        var ramp = Ft8Waveform.SamplesPerSymbol(Ft8Waveform.DefaultSampleRate) / 8;

        var firstSample = slot[padding];
        var lastSample = slot[padding + signalLength - 1];

        var inTheRampUp = Peak(slot, padding, ramp);
        var inTheRampDown = Peak(slot, padding + signalLength - ramp, ramp);
        var inTheBody = Peak(slot, padding + (signalLength / 2), ramp);

        _output.WriteLine($"ramp length            : {ramp} samples "
            + $"({ramp / (double)Ft8Waveform.DefaultSampleRate * 1000:F2} ms, an eighth of a symbol)");
        _output.WriteLine($"first sample of signal : {firstSample:F9}");
        _output.WriteLine($"last sample of signal  : {lastSample:F9}");
        _output.WriteLine($"peak within the ramp up: {inTheRampUp:F6}");
        _output.WriteLine($"peak within ramp down  : {inTheRampDown:F6}");
        _output.WriteLine($"peak in the body       : {inTheBody:F6}");

        Assert.Equal(0.0f, firstSample);
        Assert.Equal(0.0f, lastSample);
        Assert.True(inTheRampUp < inTheBody, "the ramp up should not reach the body's level.");
        Assert.True(inTheRampDown < inTheBody, "the ramp down should not reach the body's level.");
    }

    /// <summary>
    /// **Timing and slot placement, measured from the arrays.**
    /// </summary>
    /// <remarks>
    /// <para>This is the criterion 5 measurement, and it carries the finding step 3
    /// has to act on. `PHASE_PLAN.md` says *the audio starts on the slot boundary
    /// and runs 12.64 s*. The 12.64 s holds. **The start does not**: the port
    /// centres the signal in the slot, splitting the spare time evenly across both
    /// ends, which is upstream `gen_ft8.c`'s file layout.</para>
    /// <para>**Reported, not repaired.** Nothing in `src/Ft8Sharp/` is changed for
    /// it and nothing here compensates for it — where the transmission belongs
    /// inside a real slot is step 3's, because it is a question about when audio
    /// starts playing rather than about what is in the array.</para>
    /// </remarks>
    [Theory]
    [InlineData(12000)]
    [InlineData(48000)]
    public void WhereTheTransmissionSitsInTheSlotIsMeasuredRatherThanAssumed(int rate)
    {
        var composed = Ft8Composer.Compose(Message, rate);
        Assert.True(composed.Composed, composed.Explanation);

        var slot = composed.Transmission!.Samples;

        // Found by looking, not by asking the port.
        var firstSound = -1;
        var lastSound = -1;
        for (var i = 0; i < slot.Length; i++)
        {
            if (slot[i] != 0.0f)
            {
                if (firstSound < 0)
                {
                    firstSound = i;
                }

                lastSound = i;
            }
        }

        var leadingSilence = firstSound;
        var trailingSilence = slot.Length - 1 - lastSound;
        var padding = Ft8Waveform.PaddingSampleCount(rate);
        var signalLength = Ft8Waveform.SampleCount(rate);

        _output.WriteLine($"rate                        : {rate}");
        _output.WriteLine($"slot, samples               : {slot.Length}");
        _output.WriteLine($"slot, seconds               : {slot.Length / (double)rate:F6}");
        _output.WriteLine($"signal, samples             : {signalLength}");
        _output.WriteLine($"signal, seconds             : {signalLength / (double)rate:F6}");
        _output.WriteLine($"samples a symbol            : {Ft8Waveform.SamplesPerSymbol(rate)}");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"first non-zero sample at    : {firstSound}");
        _output.WriteLine($"last non-zero sample at     : {lastSound}");
        _output.WriteLine($"leading silence, samples    : {leadingSilence}");
        _output.WriteLine($"leading silence, seconds    : {leadingSilence / (double)rate:F6}");
        _output.WriteLine($"trailing silence, samples   : {trailingSilence}");
        _output.WriteLine($"trailing silence, seconds   : {trailingSilence / (double)rate:F6}");
        _output.WriteLine($"the port's PaddingSampleCount: {padding}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("FINDING: the signal is CENTRED in the slot, not started at its boundary.");
        _output.WriteLine("An FT8 transmission on the air begins shortly after the slot boundary.");
        _output.WriteLine("Reported for step 3. The port is not changed.");

        // The transmission's own length is the criterion's 12.64 s, at both rates.
        Assert.Equal(12.64, signalLength / (double)rate, 6);
        Assert.Equal(15.0, slot.Length / (double)rate, 6);

        // The signal starts one sample after the padding and ends one sample before
        // it, because the raised-cosine envelope is exactly zero at the first
        // sample of each ramp — so the silence at each end is one sample longer
        // than the padding the port laid out. Measured, then written down.
        Assert.Equal(padding + 1, firstSound);
        Assert.Equal(padding + 1, trailingSilence);

        // And that is the finding, asserted so that a port change would be noticed
        // here rather than discovered on the air: the leading silence is more than
        // a second, which is not where a transmission belongs in a slot.
        Assert.True(
            leadingSilence / (double)rate > 1.0,
            "the signal is expected to be centred, which puts over a second of silence before it.");
    }

    private static float Peak(float[] samples, int from, int count)
    {
        var peak = 0.0f;
        for (var i = from; i < from + count; i++)
        {
            peak = Math.Max(peak, Math.Abs(samples[i]));
        }

        return peak;
    }
}
