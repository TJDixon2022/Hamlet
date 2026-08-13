using Hamlet.RadioEngine.Training;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Training;

/// <summary>
/// The field guide's generated audio (HM-DEC-027): the right length, actually
/// audible, reproducible, and different where the ear should hear a
/// difference.
/// </summary>
public sealed class ModeAudioTests
{
    private static float Rms(float[] samples)
    {
        var sum = 0.0;
        foreach (var s in samples)
        {
            sum += s * (double)s;
        }

        return (float)Math.Sqrt(sum / Math.Max(1, samples.Length));
    }

    /// <remarks>
    /// Proves every mode produces a buffer of the stated length at the stated
    /// rate. A sample that ran short would cut a demonstration off mid-word.
    /// </remarks>
    [Theory]
    [InlineData(TrainingMode.Cw)]
    [InlineData(TrainingMode.Ft8)]
    [InlineData(TrainingMode.Rtty)]
    [InlineData(TrainingMode.Psk31)]
    [InlineData(TrainingMode.Ssb)]
    public void Generate_ProducesTheStatedDuration(TrainingMode mode)
    {
        var samples = ModeAudio.Generate(new AudioSampleRequest(mode));
        var expected = (int)(ModeAudio.DurationFor(mode).TotalSeconds * ModeAudio.SampleRate);

        Assert.Equal(expected, samples.Length);
    }

    /// <remarks>
    /// Proves the samples are audible and in range. A silent buffer would
    /// still pass a length check while teaching nothing, and a buffer over
    /// full scale would clip on the way out.
    /// </remarks>
    [Theory]
    [InlineData(TrainingMode.Cw)]
    [InlineData(TrainingMode.Ft8)]
    [InlineData(TrainingMode.Rtty)]
    [InlineData(TrainingMode.Psk31)]
    [InlineData(TrainingMode.Ssb)]
    public void Generate_IsAudibleAndWithinFullScale(TrainingMode mode)
    {
        var samples = ModeAudio.Generate(new AudioSampleRequest(mode));

        Assert.True(Rms(samples) > 0.01f, $"{mode} produced near-silence");
        Assert.All(samples, s => Assert.InRange(s, -1.0f, 1.0f));
    }

    /// <remarks>
    /// Proves generated audio is reproducible (§5), which is the argument for
    /// generating it rather than shipping recordings: the same request gives
    /// byte-identical output, so it can be asserted on at all.
    /// </remarks>
    [Fact]
    public void Generate_IsDeterministic()
    {
        var a = ModeAudio.Generate(new AudioSampleRequest(TrainingMode.Psk31));
        var b = ModeAudio.Generate(new AudioSampleRequest(TrainingMode.Psk31));

        Assert.Equal(a, b);
    }

    /// <remarks>
    /// Proves CW speed reaches the audio. Three speeds is how somebody finds
    /// their own copy speed, which is the groundwork FG-002 needs — and a
    /// slower sender fits fewer keying transitions into the same seconds.
    /// </remarks>
    [Fact]
    public void Cw_SpeedChangesTheKeyingRate()
    {
        static int Transitions(int wpm)
        {
            var samples = ModeAudio.Generate(
                new AudioSampleRequest(TrainingMode.Cw, wpm));

            // Count envelope crossings on a coarse grid: how often the note
            // starts and stops.
            var block = ModeAudio.SampleRate / 100;
            var transitions = 0;
            var wasOn = false;

            for (var i = 0; i + block < samples.Length; i += block)
            {
                var peak = 0f;
                for (var j = i; j < i + block; j++)
                {
                    peak = Math.Max(peak, Math.Abs(samples[j]));
                }

                var on = peak > 0.05f;
                if (on != wasOn)
                {
                    transitions++;
                    wasOn = on;
                }
            }

            return transitions;
        }

        var slow = Transitions(12);
        var fast = Transitions(25);

        Assert.True(slow > 0 && fast > 0);
        Assert.True(fast > slow,
            $"25 WPM ({fast} transitions) should key more often than 12 WPM ({slow})");
    }

    /// <remarks>
    /// Proves FT8's silence is part of the sample. Half of what identifies
    /// FT8 by ear is that it stops — a continuous warble would teach the
    /// wrong thing.
    /// </remarks>
    [Fact]
    public void Ft8_GoesQuietForTheRestOfItsSlot()
    {
        var samples = ModeAudio.Generate(new AudioSampleRequest(TrainingMode.Ft8));
        var transmitEnd = (int)(SignalSynthesizer.Ft8Transmission.TotalSeconds
                                * ModeAudio.SampleRate);

        var during = samples[..(transmitEnd - ModeAudio.SampleRate)];
        var after = samples[(transmitEnd + (ModeAudio.SampleRate / 4))..];

        Assert.True(Rms(during) > 0.05f, "FT8 should be transmitting early in the slot");
        Assert.True(Rms(after) < 0.001f, "FT8 should be silent after its transmission window");
    }

    /// <remarks>
    /// Proves the mistuned sample really differs from the tuned one. Hearing
    /// those two back to back is the fastest way a newcomer learns what
    /// tuning an SSB signal is for — it is the "ducks until you get it right"
    /// moment, and it only works if the two are genuinely different.
    /// </remarks>
    [Fact]
    public void Ssb_MistunedDiffersFromTuned()
    {
        var tuned = ModeAudio.Generate(
            new AudioSampleRequest(TrainingMode.Ssb, Mistuned: false));
        var mistuned = ModeAudio.Generate(
            new AudioSampleRequest(TrainingMode.Ssb, Mistuned: true));

        Assert.Equal(tuned.Length, mistuned.Length);
        Assert.NotEqual(tuned, mistuned);

        // Both are speech-shaped and audible; the difference is where the
        // energy sits, not whether there is any.
        Assert.True(Rms(tuned) > 0.01f);
        Assert.True(Rms(mistuned) > 0.01f);

        var differing = 0;
        for (var i = 0; i < tuned.Length; i++)
        {
            if (Math.Abs(tuned[i] - mistuned[i]) > 0.01f)
            {
                differing++;
            }
        }

        Assert.True(differing > tuned.Length / 10,
            "the mistuned sample should differ audibly across the sample, not in a corner");
    }

    /// <remarks>
    /// Proves samples start and end quietly, so playback does not click.
    /// </remarks>
    [Theory]
    [InlineData(TrainingMode.Cw)]
    [InlineData(TrainingMode.Rtty)]
    [InlineData(TrainingMode.Ssb)]
    public void Generate_FadesItsEdges(TrainingMode mode)
    {
        var samples = ModeAudio.Generate(new AudioSampleRequest(mode));

        Assert.Equal(0f, samples[0]);
        Assert.Equal(0f, samples[^1]);
    }
}
