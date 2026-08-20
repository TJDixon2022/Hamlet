using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The keying meter driven the way the application drives it, with no radio
/// (HM-DEC-091, HM-DEC-093).
/// </summary>
/// <remarks>
/// <para>**THE TAP IS THE SEAM.** In the application the meter reads the last few
/// seconds out of the decoder's own tap, once a second, while audio arrives on
/// another thread. Here the same tap is fed from a recording a chunk at a time
/// and the meter is asked at the same cadence, so what is proved is the path the
/// operator will actually be looking at rather than a function called with a file
/// in its hand.</para>
/// <para>**AND THE DECODER IS NOT IN THE PICTURE AT ALL** (§12.5). Nothing here
/// constructs one. A meter that needed the decoder running to say anything could
/// not tell the operator the one thing he needs on the evening the decoder reads
/// nothing.</para>
/// </remarks>
public sealed class TheMeterRunsOnLiveAudioTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the run of readings is printed.</param>
    public TheMeterRunsOnLiveAudioTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// Play a recording into a tap and ask the meter once a second, as the
    /// application does.
    /// </summary>
    /// <param name="audio">What to play.</param>
    /// <param name="meter">The meter to ask.</param>
    /// <returns>Every reading, in order.</returns>
    private List<KeyingReading> Play(MonoAudio audio, CwKeyingMeter meter)
    {
        var tap = new AudioTap();
        var source = new BufferedAudioSource(audio.Samples, audio.SampleRate);
        var everySecond = audio.SampleRate;
        var readings = new List<KeyingReading>();
        var sincePress = 0;

        source.SamplesReady += (in AudioChunk chunk) =>
        {
            tap.Take(chunk.Samples, chunk.SampleRate);
            sincePress += chunk.Samples.Length;

            if (sincePress < everySecond)
            {
                return;
            }

            sincePress = 0;
            readings.Add(meter.Update(tap));
        };

        source.Start();
        source.PumpAll();

        foreach (var reading in readings)
        {
            _output.WriteLine(
                $"{reading.Verdict,-10} held {reading.Held,-5} "
                + $"{reading.ToneHz,5:0} Hz  {reading.MedianMs,5:0} ms  "
                + $"{reading.SwingDb,5:0.0} dB  score {reading.Score:0.00}");
        }

        return readings;
    }

    private static MonoAudio Captured(string name) => WavAudio.Read(
        Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

    /// <summary>
    /// The four recordings from which Hamlet produced a transcript.
    /// </summary>
    /// <remarks>
    /// Their own sidecars say so: 69, 168, 41 and 177 characters emitted. That is
    /// a fact about what the decoder did and needs nobody's verdict, which is why
    /// it can be used here while adjudicating the recordings stays Tim's.
    /// </remarks>
    public static TheoryData<string> Decoded { get; } = new()
    {
        "unadjudicated/cw-2026-08-18-003016",
        "unadjudicated/cw-2026-08-18-003126",
        "unadjudicated/cw-2026-08-18-003758",
        "cw-2026-08-18-004507",
    };

    /// <summary>The two presses on the 19th that produced nothing at all.</summary>
    public static TheoryData<string> ReadNothing { get; } = new()
    {
        "unadjudicated/cw-2026-08-20-014854",
        "unadjudicated/cw-2026-08-20-014935",
    };

    private static MonoAudio Noise(int seconds, int seed)
    {
        var random = new Random(seed);
        var samples = new float[48_000 * seconds];

        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = (float)((random.NextDouble() - 0.5) * 0.2);
        }

        return new MonoAudio(48_000, samples);
    }

    private static MonoAudio Join(params MonoAudio[] parts)
    {
        var samples = parts.SelectMany(p => p.Samples).ToArray();

        return new MonoAudio(parts[0].SampleRate, samples);
    }

    /// <remarks>
    /// Proves HM-DEC-091: played through a tap at the cadence the application
    /// uses, the meter reaches **keying** on every recording Hamlet produced a
    /// transcript from, having chosen the pitch itself from a sweep.
    /// </remarks>
    /// <param name="name">The recording.</param>
    [Theory]
    [MemberData(nameof(Decoded))]
    public void ItReachesKeyingOnEveryRecordingThatDecoded(string name)
    {
        var readings = Play(Captured(name), new CwKeyingMeter());

        Assert.NotEmpty(readings);
        Assert.Contains(readings, r => r.Verdict == KeyingVerdict.Keying);
        Assert.Equal(KeyingVerdict.Keying, readings[^1].Verdict);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-091: **the two presses on the 19th, which are the
    /// reason this exists.** He heard a station, pressed, and both rows read
    /// nothing. The meter says no keying, which agrees with the decoder and with
    /// the measurement, and is the answer he needed at the time rather than the
    /// next morning.</para>
    /// <para>It asserts nothing about why the station is missing from the audio.
    /// That is not known and this instrument does not claim to know it (§0.0).
    /// </para>
    /// </remarks>
    /// <param name="name">The recording.</param>
    [Theory]
    [MemberData(nameof(ReadNothing))]
    public void ItSaysNoKeyingOnThePressesThatProducedNothing(string name)
    {
        var readings = Play(Captured(name), new CwKeyingMeter());

        Assert.DoesNotContain(readings, r => r.Verdict == KeyingVerdict.Keying);
        Assert.Equal(KeyingVerdict.NoKeying, readings[^1].Verdict);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-091: **the station the decoder missed entirely.**
    /// `cw-2026-08-17-134712` emitted no characters at all, and there is keying in
    /// it: the meter finds a 500 Hz signal with an element length in the CW
    /// range.</para>
    /// <para>This is the case the whole instrument exists for. It asserts nothing
    /// about what that station sent, which nobody knows, only that something in
    /// this audio is being keyed (§0.0).</para>
    /// </remarks>
    [Fact]
    public void ItFindsKeyingTheDecoderReadNothingFrom()
    {
        var readings = Play(Captured("cw-2026-08-17-134712"), new CwKeyingMeter());

        Assert.Contains(readings, r => r.Verdict == KeyingVerdict.Keying);
    }

    /// <remarks>
    /// Proves §12.5: **the control.** Half a minute of noise through the same
    /// path settles on no keying and never claims otherwise, so a pass above is
    /// not the meter agreeing with everything.
    /// </remarks>
    [Fact]
    public void ItSettlesOnNoKeyingWhenThereIsNobodyThere()
    {
        var readings = Play(Noise(30, 4242), new CwKeyingMeter());

        Assert.DoesNotContain(readings, r => r.Verdict == KeyingVerdict.Keying);
        Assert.Equal(KeyingVerdict.NoKeying, readings[^1].Verdict);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-091: **a gap between overs does not knock it out of
    /// keying**, played end to end. A station sends for half a minute, goes quiet
    /// for twelve seconds while somebody else answers off frequency, and comes
    /// back.</para>
    /// <para>Twelve seconds because that is about a short over at thirteen words a
    /// minute, and because eight was measured against the first rule tried and
    /// broke it: the meter changed its mind while the contact was still going
    /// on.</para>
    /// <para>**THIS IS THE ONE THAT DECIDES WHETHER THE INSTRUMENT IS USABLE.** A
    /// meter that says no keying whenever a station pauses will be distrusted
    /// inside ten minutes, and after that it cannot help him at all.</para>
    /// </remarks>
    [Fact]
    public void AGapBetweenOversDoesNotKnockItOut()
    {
        var station = Captured("cw-2026-08-18-004507");
        var readings = Play(
            Join(station, Noise(12, 99), station), new CwKeyingMeter());

        var reached = readings.FindIndex(r => r.Verdict == KeyingVerdict.Keying);

        Assert.True(reached >= 0, "it never reached keying at all");
        Assert.DoesNotContain(
            readings.Skip(reached), r => r.Verdict == KeyingVerdict.NoKeying);

        // And it admits when it is holding rather than measuring, so nothing on
        // the screen claims a freshness it does not have (§0.0).
        Assert.Contains(readings, r => r.Held);
    }

    /// <remarks>
    /// Proves §0.0: before there is a full window of audio the meter says
    /// **listening** rather than no keying, because audio that has not arrived is
    /// not audio with nothing in it.
    /// </remarks>
    [Fact]
    public void BeforeThereIsEnoughAudioItSaysNothing()
    {
        var meter = new CwKeyingMeter();
        var tap = new AudioTap();
        var station = Captured("cw-2026-08-18-004507");
        var short_ = new float[station.SampleRate * 2];

        Array.Copy(station.Samples, short_, short_.Length);
        tap.Take(short_, station.SampleRate);

        var reading = meter.Update(tap);

        Assert.Equal(KeyingVerdict.Listening, reading.Verdict);
        Assert.Equal(0, reading.ToneHz);
    }
}
