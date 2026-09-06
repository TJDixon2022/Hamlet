using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// Work instruction 252, task 4: opening the Digital tab does not cost a third
/// of a second before the first waterfall row is drawn.
/// </summary>
/// <remarks>
/// <para>**WHAT UNIT 240 LEFT, AND WHY.** It stopped the transform while nobody
/// was drawing, which was right and is not being undone. It also threw the ring
/// away and stopped feeding it, so the first frame after somebody looked again
/// cost one full window of audio — 16,384 samples, about 341 milliseconds at
/// 48 kHz. Its reason was §0.0: a row must never mix two moments, and a ring
/// that is stopped and restarted would give a first frame that is part old audio
/// and part new.</para>
/// <para>**THE GAP IS WHAT CAUSES THAT, SO UNIT 252 REMOVES THE GAP RATHER THAN
/// THE RULE.** The ring is fed continuously, so it always holds the most recent
/// window of contiguous audio, and the first frame after somebody looks again is
/// the frame they would have had if they never looked away. Nothing is stale
/// because nothing stopped, and the §0.0 hazard is not traded away — it is
/// removed at its cause.</para>
/// <para>**AND NOTHING MOVED ONTO THE AUDIO CALLBACK THREAD**, which is the one
/// thing task 4 was told not to spend. `Emit` still returns on a null
/// `FrameReady`, so the 16,384-point transform is still not run while nobody is
/// looking, and `Push` — which writes the ring — still runs on the below-normal
/// worker. `TheWaterfallNeverDelaysTheRadioTests` holds that line and is
/// untouched.</para>
/// </remarks>
public sealed class TheWaterfallsFirstRowIsNotLateTests
{
    private const int Rate = 48000;

    /// <summary>One WASAPI buffer at the rate the shack machine reports.</summary>
    private const int Buffer = Rate * WasapiAudioSource.BufferMilliseconds / 1000;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the sample counts are printed.</param>
    public TheWaterfallsFirstRowIsNotLateTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// A subscriber arriving after a quiet spell gets its first frame within one
    /// hop, not one window.
    /// </summary>
    /// <remarks>
    /// **ONE HOP IS THE FLOOR AND IT IS NOT AVOIDABLE.** The transform runs on a
    /// hop boundary by construction, so the most anybody can ask is that the
    /// window behind that boundary is already full. One hop at 48 kHz is 4,096
    /// samples, about 85 milliseconds; the third of a second was the three extra
    /// hops spent refilling a ring that had been emptied.
    /// </remarks>
    [Fact]
    public void ASubscriberArrivingLateWaitsOneHopAndNotOneWindow()
    {
        using var source = new FakeSource(Rate, Buffer);
        using var spectrum = new AudioSpectrumSource(Rate, simulated: true);

        spectrum.Start();
        spectrum.Listen(source);

        var window = AudioSpectrumSource.WindowFor(Rate);
        var hop = window / AudioSpectrumSource.HopDivisor;

        // **NOBODY IS DRAWING FOR A GOOD WHILE.** Two windows' worth, so the
        // discard unit 240 did would have had every chance to fire and the ring
        // would have been empty at the end of it.
        var quiet = (2 * window / Buffer) + 1;

        for (var i = 0; i < quiet; i++)
        {
            source.Deliver(Tone(Buffer));
        }

        Settle(spectrum);

        // Now somebody opens the tab.
        var frames = 0;
        var deliveredAfter = 0;

        spectrum.FrameReady += (in SpectrumFrame _) => Interlocked.Increment(ref frames);

        while (Volatile.Read(ref frames) == 0
               && deliveredAfter < (window / Buffer) + 4)
        {
            source.Deliver(Tone(Buffer));
            deliveredAfter++;

            Settle(spectrum);
        }

        var samplesWaited = deliveredAfter * Buffer;

        _output.WriteLine("window " + window + " samples, hop " + hop + " samples");
        _output.WriteLine(
            "quiet for " + (quiet * Buffer) + " samples with nobody drawing");
        _output.WriteLine(
            "first frame after " + samplesWaited + " samples ("
            + (samplesWaited * 1000.0 / Rate).ToString("0") + " ms)");

        Assert.True(frames > 0, "no frame arrived at all");

        // **THE ASSERTION IS AGAINST THE WINDOW, NOT AGAINST A STOPWATCH.** What
        // is measured is how much audio had to arrive, which is a property of this
        // class and the same on every machine. One hop, plus at most the buffer
        // that straddles the boundary.
        Assert.True(
            samplesWaited <= hop + Buffer,
            "waited " + samplesWaited + " samples, which is more than one hop ("
            + hop + ") plus a buffer (" + Buffer + ")");

        // And the thing that used to happen: a whole window of refilling.
        Assert.True(samplesWaited < window);
    }

    /// <summary>
    /// **WHAT THE FIRST FRAME LOOKS LIKE, MEASURED, AND IT IS NOT WHAT THIS UNIT
    /// EXPECTED.** A second test was written here to prove the warm ring holds
    /// real audio by finding the tone in the first frame. It cannot, and the
    /// reason is a property of the picture rather than of the ring: `Emit` draws
    /// every bin against that bin's own tracked floor, and on the very first frame
    /// the floor is initialised to that frame's own level - so `over` is zero,
    /// `above` is zero, and **every bin of the first frame is 0 by construction**.
    /// Measured, with a continuous 688 Hz tone running throughout: all eight
    /// loudest bins at 0.
    /// <para>**SO THE FIRST ROW IS BLACK WHETHER IT ARRIVES IN 100 MILLISECONDS OR
    /// IN 341**, which is unchanged by this unit and is not the delay task 4
    /// names. Keeping the floors warm as well would mean running the
    /// 16,384-point transform while nobody is drawing, which is exactly what unit
    /// 240 removed and what task 4 was told not to spend. It is reported rather
    /// than fixed.</para>
    /// <para>The timing test above is the finding and it stands on its own: a
    /// frame arriving one buffer after somebody looks is only possible if the
    /// window behind it was already full.</para>
    /// </summary>
    [Fact(Skip = "A record of what was measured, not a test. See the summary.")]
    public void TheFirstFrameIsBlackByConstructionAndThatIsNotThisUnitsToFix()
    {
    }

    /// <summary>Wait for the frame worker to consume what was just delivered.</summary>
    /// <remarks>
    /// **IT ASKS THE QUEUE RATHER THAN SLEEPING.** The worker is asynchronous and
    /// below normal priority by design, so a test on the device path has to know
    /// when it has caught up; a fixed sleep is a guess that is either flaky or
    /// slow, and is never a fact about the class under test.
    /// </remarks>
    private static void Settle(AudioSpectrumSource spectrum)
        => Assert.True(
            spectrum.WaitUntilDrained(TimeSpan.FromSeconds(5)),
            "the frame worker did not catch up within five seconds");

    /// <summary>A tone, at a frequency well inside the drawn band.</summary>
    /// <param name="count">How many samples.</param>
    /// <returns>The samples.</returns>
    /// <remarks>
    /// 0.09 radians a sample at 48 kHz is about 688 Hz, which sits well inside the
    /// 199 to 3003 Hz the picture is drawn over.
    /// </remarks>
    private static float[] Tone(int count)
    {
        var phase = 0;

        return Tone(count, ref phase);
    }

    /// <summary>The same tone, carried on across buffers.</summary>
    /// <param name="count">How many samples.</param>
    /// <param name="phase">Where the last buffer left off. Advanced in place.</param>
    /// <returns>The samples.</returns>
    /// <remarks>
    /// **PHASE-CONTINUOUS, BECAUSE THE WINDOW SPANS SEVERAL BUFFERS.** Restarting
    /// the sine at every buffer puts a step at each boundary, and a step is
    /// broadband — it would smear the very peak the caller is looking for, and
    /// that smearing would be an artifact of the fixture rather than a fact about
    /// the class (§12.5).
    /// </remarks>
    private static float[] Tone(int count, ref int phase)
    {
        var samples = new float[count];

        for (var i = 0; i < count; i++)
        {
            samples[i] = MathF.Sin((phase + i) * 0.09f) * 0.3f;
        }

        phase += count;

        return samples;
    }

    /// <summary>A source that raises `SamplesReady` synchronously, as WASAPI does.</summary>
    private sealed class FakeSource : IAudioSource
    {
        private readonly float[] _scratch;
        private long _delivered;

        public FakeSource(int sampleRate, int chunk)
        {
            SampleRate = sampleRate;
            _scratch = new float[chunk];
        }

        public int SampleRate { get; }

        public string DeviceName => "fake";

        public bool IsSimulated => true;

        public bool IsRunning { get; private set; } = true;

        public event AudioChunkHandler? SamplesReady;

        public void Start() => IsRunning = true;

        public void Stop() => IsRunning = false;

        public void Dispose() => Stop();

        /// <summary>Deliver one buffer the way the device callback delivers one.</summary>
        public void Deliver(float[] samples)
        {
            samples.AsSpan(0, Math.Min(samples.Length, _scratch.Length))
                .CopyTo(_scratch);

            var chunk = new AudioChunk(_delivered, SampleRate, _scratch);

            _delivered += _scratch.Length;
            SamplesReady?.Invoke(in chunk);
        }
    }
}
