using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE RADIO POINTS THE DETECTOR** (work instruction 480 task 2, step 12 criterion 12.4, R94,
/// HM-DEC-188).
/// </summary>
/// <remarks>
/// <para>**WHAT THE OWNER'S ROWS SHOWED, 2026-09-28 17:13 TO 17:15 UTC.** On two stations he
/// heard plainly the meter's best pitch sat at 350 to 425 Hz with scores of 0.01 to 0.04: its
/// own sweep had settled on the loudest noise. The rows are why the radio's scope points; they
/// chose nothing here.</para>
/// <para>**NO RECORDING** (R88). The frame is built by hand the way the radio sends one - a span
/// centered on the dial and a run of levels - and the audio is seeded synthetic noise.</para>
/// </remarks>
public sealed class TheRadioPointsTheDetectorTests
{
    private const int Rate = 8_000;
    private const long Dial = 14_030_000;

    // One of the frame's 475 points across five kilohertz: as close as the radio can place a peak.
    private const double ScopeBinHz = 5_000.0 / 475;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the pitches are printed.</param>
    public TheRadioPointsTheDetectorTests(ITestOutputHelper output) => _output = output;

    /// <remarks>
    /// Proves the pointer's own reading of the instruction's case: a peak 250 Hz above the centre with
    /// the CW pitch at 600 is a beat note at 850. Since unit 515 it points nothing (R114, HM-DEC-219):
    /// the detector finds the shape at every pitch, and the peak reaches the row and the sheet only.
    /// </remarks>
    [Fact]
    public void APeak250HzAboveTheDialWithPitch600IsABeatNoteAt850()
    {
        var bins = FrameWithPeakAt(Dial + 250);
        var frame = new SpectrumFrame(Dial - 2_500, Dial + 2_500, DateTime.UtcNow, bins);
        var peak = CwScopePointer.Peak(frame, Dial, 600, 600);

        _output.WriteLine($"peak {peak?.PeakHz - Dial:+0;-0} Hz from the dial, level {peak?.Level}, pitch {peak?.PitchHz:0} Hz");

        Assert.NotNull(peak);
        Assert.InRange(peak!.Value.PitchHz, 850 - ScopeBinHz, 850 + ScopeBinHz);
    }

    /// <remarks>
    /// Proves the pointer's quiet: three seconds without a frame and it says nothing. Since unit 515
    /// nothing is pointed by it, so nothing sweeps on its account (R114).
    /// </remarks>
    [Fact]
    public void ThreeSecondsWithoutAFrameIsTheScopeQuiet()
    {
        var pointer = new CwScopePointer();
        var start = new DateTime(2026, 9, 28, 17, 13, 0, DateTimeKind.Utc);
        var frame = new SpectrumFrame(Dial - 2_500, Dial + 2_500, start, FrameWithPeakAt(Dial + 250));

        pointer.Observe(frame, Dial, 600, 600, start);
        var early = pointer.Pointing(start + TimeSpan.FromSeconds(2.9));
        var late = pointer.Pointing(start + ScopeFlow.QuietAfter + TimeSpan.FromMilliseconds(1));

        _output.WriteLine(
            $"at 2.9 s {early?.PitchHz:0} Hz; past {ScopeFlow.QuietAfter.TotalSeconds} s {(late is null ? "quiet" : "pointing")}; "
            + $"frames in the last four seconds {pointer.FramesLast4s(start + TimeSpan.FromSeconds(1))}");

        Assert.InRange(early!.Value.PitchHz, 850 - ScopeBinHz, 850 + ScopeBinHz);
        Assert.Null(late);
        Assert.Equal(1, pointer.FramesLast4s(start + TimeSpan.FromSeconds(1)));
        Assert.Equal(0, pointer.FramesLast4s(start + TimeSpan.FromSeconds(5)));
    }

    /// <remarks>
    /// Proves the tracker's two candidates: the meter saying keying at 600 and the scope pointing
    /// at keying at 850, the tracker mixes at 850 from the next hop; the scope letting go hands
    /// it back to the meter's 600.
    /// </remarks>
    [Fact]
    public void WhenTheScopeAndTheMeterBothNameAPitchTheScopeWins()
    {
        const int trackerRate = 48_000;
        var tracker = new CwToneTracker(trackerRate, 500);
        var noise = new Noise(483);

        FeedTracker(tracker, noise, trackerRate / 2);
        tracker.FollowMeter(new KeyingReading(KeyingVerdict.Keying, 600, 48, 24, 30, 0.27, false, 48));
        tracker.FollowScope(850);
        FeedTracker(tracker, noise, tracker.HopSamples);
        var both = tracker.ToneHz;

        tracker.FollowScope(null);
        FeedTracker(tracker, noise, tracker.HopSamples);
        var meterOnly = tracker.ToneHz;

        _output.WriteLine($"meter 600 and scope 850: {both:0} Hz, keying {tracker.HasKeying}; scope gone: {meterOnly:0} Hz");

        Assert.Equal(850, both, 0);
        Assert.Equal(600, meterOnly, 0);
    }

    // A flat frame at a low level with one tall bin, 475 points across five kilohertz.
    private static byte[] FrameWithPeakAt(long hz)
    {
        var bins = new byte[475];
        Array.Fill(bins, (byte)40);

        var index = (int)((hz - (Dial - 2_500)) * bins.Length / 5_000.0);
        bins[index] = 200;

        return bins;
    }

    private static void Feed(CwEnvelopeDetector detector, Noise noise, int samples)
    {
        var chunk = new float[samples];

        for (var i = 0; i < samples; i++)
        {
            chunk[i] = noise.Next();
        }

        detector.Process(chunk);
    }

    private static void FeedTracker(CwToneTracker tracker, Noise noise, int samples)
    {
        var chunk = new float[samples];

        for (var i = 0; i < samples; i++)
        {
            chunk[i] = noise.Next();
        }

        tracker.Process(chunk, 0, _ => { });
    }

    /// <summary>Seeded Gaussian noise, Box-Muller.</summary>
    private sealed class Noise
    {
        private readonly Random _random;

        public Noise(int seed) => _random = new Random(seed);

        public float Next()
        {
            var u1 = 1.0 - _random.NextDouble();
            var u2 = _random.NextDouble();

            return (float)(0.03 * Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2));
        }
    }
}
