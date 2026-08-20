using System.Diagnostics;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The keying meter: what it says, how long it holds, and what it costs
/// (HM-DEC-091).
/// </summary>
/// <remarks>
/// <para>**IT EXISTS TO CONTRADICT THE DECODER, SO IT MUST NOT SHARE ITS
/// OPINION** (§12.5). Nothing here hands it a pitch, a speed or a verdict; it is
/// given audio and nothing else, exactly as it is given audio at the rig.</para>
/// <para>**AND THE HOLDING IS AS IMPORTANT AS THE MEASUREMENT.** A meter that
/// drops to no keying between overs is worse than none, because the operator
/// stops trusting it in the first ten minutes.</para>
/// </remarks>
public sealed class CwKeyingMeterTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public CwKeyingMeterTests(ITestOutputHelper output) => _output = output;

    private static MonoAudio Window(string name, int index)
    {
        var audio = WavAudio.Read(
            Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

        var length = (int)(audio.SampleRate * CwKeyingThresholds.Window.TotalSeconds);
        var slice = new float[length];

        Array.Copy(audio.Samples, index * length, slice, 0, length);

        return new MonoAudio(audio.SampleRate, slice);
    }

    private static MonoAudio Noise(int seed)
    {
        var random = new Random(seed);
        var samples = new float[
            (int)(48_000 * CwKeyingThresholds.Window.TotalSeconds)];

        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = (float)((random.NextDouble() - 0.5) * 0.2);
        }

        return new MonoAudio(48_000, samples);
    }

    /// <remarks>
    /// Proves HM-DEC-091: on audio known to contain a station, the meter says so
    /// from the first window, having chosen the pitch itself.
    /// </remarks>
    [Fact]
    public void ItSaysKeyingOnAudioThatHasKeyingInIt()
    {
        var meter = new CwKeyingMeter();
        var reading = meter.Update(Window("cw-2026-08-18-004507", 0));

        _output.WriteLine(
            $"{reading.Verdict} at {reading.ToneHz:0} Hz, {reading.MedianMs:0} ms, "
            + $"{reading.SwingDb:0.0} dB, score {reading.Score:0.00}");

        Assert.Equal(KeyingVerdict.Keying, reading.Verdict);
        Assert.False(reading.Held);
        Assert.Equal(500, reading.ToneHz);
        Assert.InRange(reading.MedianMs, 40, 80);
    }

    /// <remarks>
    /// Proves §0.0: **noise does not become a station by being looked at for long
    /// enough.** One window of noise is not enough to say anything, which is the
    /// meter refusing rather than the meter failing.
    /// </remarks>
    [Fact]
    public void OneWindowOfNoiseIsNotYetAnAnswer()
    {
        var meter = new CwKeyingMeter();
        var reading = meter.Update(Noise(1));

        Assert.Equal(KeyingVerdict.Listening, reading.Verdict);
        Assert.InRange(reading.MedianMs, 0, 20);
    }

    /// <remarks>
    /// Proves HM-DEC-091: enough windows of nothing and the meter says nothing is
    /// there, which is the answer the operator needs when he is turning a knob to
    /// find out what changed.
    /// </remarks>
    [Fact]
    public void EnoughWindowsOfNothingAndItSaysSo()
    {
        var meter = new CwKeyingMeter();

        for (var i = 0; i < CwKeyingThresholds.QuietWindowsBeforeNoKeying; i++)
        {
            meter.Update(Noise(i + 1));
        }

        Assert.Equal(KeyingVerdict.NoKeying, meter.Reading.Verdict);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-091: **the gap between overs, which is the case that
    /// decides whether this instrument is usable at all.** A station sends, stops
    /// to listen, and comes back. The meter must still say keying throughout, and
    /// must say that it is holding rather than that it just measured it.</para>
    /// <para>Four quiet windows is one short of the run, which is deliberately the
    /// hardest case: one more and it would be entitled to change its mind.</para>
    /// </remarks>
    [Fact]
    public void AGapBetweenOversDoesNotKnockItOutOfKeying()
    {
        var meter = new CwKeyingMeter();

        meter.Update(Window("cw-2026-08-18-004507", 0));

        Assert.Equal(KeyingVerdict.Keying, meter.Reading.Verdict);

        for (var i = 0; i < CwKeyingThresholds.QuietWindowsBeforeNoKeying - 1; i++)
        {
            var quiet = meter.Update(Noise(i + 20));

            _output.WriteLine(
                $"quiet {i + 1}: {quiet.Verdict}, held {quiet.Held}, "
                + $"{quiet.MedianMs:0} ms");

            Assert.Equal(KeyingVerdict.Keying, quiet.Verdict);
            Assert.True(quiet.Held, "the meter did not admit it was holding");
        }

        var back = meter.Update(Window("cw-2026-08-18-004507", 1));

        Assert.Equal(KeyingVerdict.Keying, back.Verdict);
        Assert.False(back.Held);
    }

    /// <remarks>
    /// Proves §0.0: **audio that has not arrived is not audio with nothing in
    /// it.** A stalled pipeline must not be able to talk the meter into saying the
    /// band is dead, so a null window advances nothing.
    /// </remarks>
    [Fact]
    public void AWindowThatNeverArrivedAdvancesNothing()
    {
        var meter = new CwKeyingMeter();

        meter.Update(Window("cw-2026-08-18-004507", 0));

        for (var i = 0; i < CwKeyingThresholds.QuietWindowsBeforeNoKeying * 3; i++)
        {
            meter.Update((MonoAudio?)null);
        }

        Assert.Equal(KeyingVerdict.Keying, meter.Reading.Verdict);
    }

    /// <remarks>
    /// <para>Proves §8: **what one update costs**, because this runs once a second
    /// beside a live decoder on the audio thread's own data. A meter that
    /// stutters the application to report on it has traded the thing for the
    /// record of it.</para>
    /// <para>Printed rather than pinned tightly, since a timing assertion on a
    /// shared build agent is a flaky test. The bound is loose enough to catch a
    /// change of order and nothing finer.</para>
    /// </remarks>
    [Fact]
    public void OneUpdateCostsLittleEnoughToRunEverySecond()
    {
        var window = Window("cw-2026-08-18-004507", 0);
        var meter = new CwKeyingMeter();

        meter.Update(window);

        var clock = Stopwatch.StartNew();

        for (var i = 0; i < 10; i++)
        {
            meter.Update(window);
        }

        clock.Stop();

        var each = clock.Elapsed.TotalMilliseconds / 10;

        _output.WriteLine(
            $"{each:0.0} ms per update, sweeping "
            + $"{(KeyingEnvelope.HighestToneHz - KeyingEnvelope.LowestToneHz) / KeyingEnvelope.ToneStepHz + 1:0} "
            + $"candidates over {CwKeyingThresholds.Window.TotalSeconds:0} seconds");

        Assert.True(each < 500, $"one update took {each:0} ms");
    }
}
