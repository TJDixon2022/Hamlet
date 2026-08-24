using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The operator can hold the decoder's pitch, the lock comes from a measured
/// peak rather than a bin, and the tracker stops steering while it is held.
/// </summary>
/// <remarks>
/// <para>**THE TRACKER IS MEASURABLY THE LARGEST SOURCE OF SOUP IN THIS
/// DECODER.** Unit 002 put a clean generated station through the production path
/// and got twenty-two characters that were never sent, and through the same
/// window with the pitch nailed it got none. This does not change what the
/// tracker does; it lets the operator take it out of the path.</para>
/// <para>**AND THE LOCK IS ONLY AS GOOD AS THE PITCH IT HOLDS.** A bin centre is
/// a measurement of a bin, so holding one means holding a filter pointed up to
/// half a spacing off for as long as the lock lasts, with nothing left to
/// correct it.</para>
/// </remarks>
public sealed class ThePitchCanBeHeldTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the pitches are printed.</param>
    public ThePitchCanBeHeldTests(ITestOutputHelper output) => _output = output;

    private static CwDecoder Fed(MonoAudio audio, double startAt, double seconds)
    {
        var decoder = new CwDecoder(audio.SampleRate, startAt);
        var hop = decoder.Tracker.HopSamples;
        var stop = Math.Min(audio.Samples.Length, (int)(seconds * audio.SampleRate));

        for (var at = 0L; at + hop <= stop; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        return decoder;
    }

    private static MonoAudio Generated(double toneHz) => CwSignal.Generate(
        new CwSignalRequest(
            "CQ CQ DE W1AW W1AW K",
            WordsPerMinute: 18,
            ToneHz: toneHz,
            Amplitude: 0.5,
            NoiseAmplitude: CwSensitivity.NoiseFor(18),
            Seed: 4));

    /// <remarks>
    /// **THE PEAK IS READ BETWEEN THE BINS.** The fine bank is five hertz apart,
    /// so a signal deliberately placed off a bin centre must still be found
    /// closer than half a spacing, which a bin centre cannot manage by
    /// construction.
    /// </remarks>
    [Fact]
    public void TheMeasuredPeakLandsBetweenTheBins()
    {
        // Deliberately not a multiple of the fine spacing.
        const double sent = 613.7;

        var decoder = Fed(Generated(sent), sent, 8);
        var peak = decoder.Tracker.MeasuredPeakHz;

        _output.WriteLine(
            $"sent {sent:0.0} Hz, tracker bin {decoder.Tracker.ToneHz:0.0} Hz, "
            + $"measured peak {peak:0.00} Hz");

        Assert.False(double.IsNaN(peak), "no peak was measured at all.");

        Assert.True(
            Math.Abs(peak - sent) < 2.5,
            $"the measured peak came back {peak:0.00} Hz for a station at "
            + $"{sent:0.0} Hz, which is further off than half a bin.");
    }

    /// <remarks>
    /// Proves the lock engages from that peak, and reports the pitch it took, so
    /// nothing has to be inferred from the tracker afterwards.
    /// </remarks>
    [Fact]
    public void LockingTakesTheMeasuredPeakAndSaysWhatItTook()
    {
        const double sent = 613.7;

        var decoder = Fed(Generated(sent), sent, 8);

        Assert.False(decoder.IsLocked);
        Assert.True(double.IsNaN(decoder.LockedToneHz));

        var locked = decoder.Lock();

        _output.WriteLine($"locked to {locked:0.00} Hz");

        Assert.True(decoder.IsLocked);
        Assert.Equal(locked, decoder.LockedToneHz);
        Assert.Equal(decoder.Tracker.MeasuredPeakHz, locked, 6);
    }

    /// <remarks>
    /// **THE POINT OF THE WHOLE FEATURE.** While locked the mixdown stays where
    /// it was put, whatever the tracker decides. Proved by moving the tracker
    /// somewhere else and watching the decoder's own pitch not follow.
    /// </remarks>
    [Fact]
    public void WhileLockedTheTrackerDoesNotSteerTheDecoder()
    {
        const double sent = 613.7;

        var audio = Generated(sent);
        var decoder = Fed(audio, sent, 8);

        var locked = decoder.Lock();
        var hop = decoder.Tracker.HopSamples;

        // Push the tracker off deliberately, by feeding it a different station.
        var elsewhere = Generated(500);

        for (var at = 0L; at + hop <= elsewhere.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, elsewhere.SampleRate, elsewhere.Samples.AsSpan((int)at, hop)));
        }

        _output.WriteLine(
            $"locked at {locked:0.00} Hz, tracker now says "
            + $"{decoder.Tracker.ToneHz:0.0} Hz, decoder is reading "
            + $"{decoder.Stream.ToneHz:0.00} Hz");

        Assert.True(decoder.IsLocked);
        Assert.Equal(locked, decoder.Stream.ToneHz, 6);
    }

    /// <remarks>
    /// And unlocking hands it back, so the lock is a state the operator holds
    /// rather than a door that shuts.
    /// </remarks>
    [Fact]
    public void UnlockingLetsTheTrackerSteerAgain()
    {
        const double sent = 613.7;

        var audio = Generated(sent);
        var decoder = Fed(audio, sent, 8);

        decoder.Lock();
        decoder.Unlock();

        Assert.False(decoder.IsLocked);
        Assert.True(double.IsNaN(decoder.LockedToneHz));

        var hop = decoder.Tracker.HopSamples;

        decoder.Process(new AudioChunk(
            0, audio.SampleRate, audio.Samples.AsSpan(0, hop)));

        _output.WriteLine(
            $"unlocked; tracker {decoder.Tracker.ToneHz:0.0} Hz, "
            + $"decoder {decoder.Stream.ToneHz:0.0} Hz");

        Assert.Equal(decoder.Tracker.ToneHz, decoder.Stream.ToneHz, 6);
    }

    /// <remarks>
    /// **IT REFUSES RATHER THAN GUESSING** (§0.0). With too little audio to have
    /// measured a peak, nothing is locked and the tracker keeps steering. A lock
    /// to a pitch nobody measured would tell the operator the decoder is held on
    /// a station when it is held on nothing.
    /// </remarks>
    [Fact]
    public void WithNothingMeasuredNothingIsLocked()
    {
        var audio = Generated(613.7);
        var decoder = new CwDecoder(audio.SampleRate, 600);

        var locked = decoder.Lock();

        _output.WriteLine(
            locked is double.NaN ? "refused, which is right" : $"locked {locked}");

        Assert.True(double.IsNaN(locked));
        Assert.False(decoder.IsLocked);
    }
}
