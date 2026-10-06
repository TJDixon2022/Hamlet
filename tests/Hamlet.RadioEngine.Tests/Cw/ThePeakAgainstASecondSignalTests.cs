using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// What the spectral peak does when a second station shares the passband.
/// </summary>
/// <remarks>
/// <para>**THIS IS SYNTHETIC AND IT IS NOT CORPUS EVIDENCE** (work instruction
/// 053, task 3, which requires that to be said). Every capture in the tree has one
/// dominant station, so **the corpus cannot see this failure at all** — which is
/// the reason the suspect survived unit 050's measurement. A real forty-metre
/// evening puts more than one signal in a five-hundred-hertz passband; these
/// recordings never do.</para>
/// <para>**THE INPUT IS TWO REAL CAPTURES SUMMED**, not two generated tones, so
/// the keying, the fading and the noise are the operator's own rather than a
/// model of them (HM-DEC-091: the only measurement is against real data). One is
/// scaled down and mixed into the other.</para>
/// <para>**MEASURE ONLY. NOTHING HERE CHANGES THE PEAK**, which the order
/// forbids.</para>
/// </remarks>
public sealed class ThePeakAgainstASecondSignalTests
{
    private readonly ITestOutputHelper _output;

    public ThePeakAgainstASecondSignalTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The two captures mixed, and where each one's station sits.</summary>
    /// <remarks>
    /// `013347` holds a station near 613.6 Hz and `012403` one near 439.8 Hz,
    /// measured by the peak itself on each capture alone in unit 052. They are far
    /// enough apart to tell which one the peak has chosen and close enough to sit
    /// in one passband.
    /// </remarks>
    private const string StrongName = "captured/cw-2026-08-17-013347";
    private const string WeakName = "captured/unadjudicated/cw-2026-08-24-012403";

    private const double StrongHz = 613.6;
    private const double WeakHz = 439.8;

    /// <summary>At what level difference does the peak stop holding the stronger?</summary>
    /// <remarks>
    /// **THE QUESTION THE OPERATOR'S REPORT TURNS ON.** The tone tracker held its
    /// pitch once locked. The peak re-measures from scratch and takes the loudest
    /// bin in the range, so it has nothing to hold with — and if a second signal
    /// rising past the first is enough to move it, that is a mechanism for the
    /// garbage he describes.
    /// </remarks>
    [Fact]
    public void WhereThePeakSwitchesFromOneStationToTheOther()
    {
        var strong = WavAudio.Read(PathOf(StrongName));
        var weak = WavAudio.Read(PathOf(WeakName));

        _output.WriteLine("secondDb\tpeakHz\tholding");

        double? switchedAt = null;

        foreach (var db in new[] { -30.0, -24, -18, -12, -9, -6, -3, 0, 3, 6 })
        {
            var mixed = Mix(strong, weak, db);
            var peak = CwSpectralPeak.Find(mixed.Samples, mixed.SampleRate);

            if (peak is null)
            {
                continue;
            }

            var holdingStrong = Math.Abs(peak.Value - StrongHz)
                                < Math.Abs(peak.Value - WeakHz);

            _output.WriteLine(
                $"{db:+0;-0}\t{peak:0.0}\t{(holdingStrong ? "613 (first)" : "440 (second)")}");

            if (!holdingStrong && switchedAt is null)
            {
                switchedAt = db;
            }
        }

        _output.WriteLine("");
        _output.WriteLine(switchedAt is { } at
            ? $"the peak leaves the first station once the second is within {at:+0;-0} dB of it"
            : "the peak never left the first station across the whole sweep");

        // No assertion on where it switches: the number is the deliverable and
        // fixing it here would be asserting a behaviour nobody has ruled on.
        Assert.True(true);
    }

    /// <summary>Does it walk between them inside one recording?</summary>
    /// <remarks>
    /// **A SWITCH BETWEEN FILES IS SURVIVABLE AND A WALK INSIDE ONE IS NOT.** The
    /// decoder mixes down to whatever the peak last said, so a pitch that moves
    /// mid-recording decodes the first half at one station and the second half at
    /// another, and everything in between is nonsense. This asks the peak the same
    /// question the live decoder asks it: once a second, over the trailing eight
    /// seconds.
    /// </remarks>
    [Fact]
    public void WhetherThePeakWalksBetweenThemWithinOneRecording()
    {
        var strong = WavAudio.Read(PathOf(StrongName));
        var weak = WavAudio.Read(PathOf(WeakName));

        // Level them, which is the hardest case rather than a fair one.
        var mixed = Mix(strong, weak, -3);
        var rate = mixed.SampleRate;
        var window = 8 * rate;

        var chose = new List<double>();

        _output.WriteLine("atSecond\tpeakHz\tholding");

        for (var at = window; at <= mixed.Samples.Length; at += rate)
        {
            var slice = mixed.Samples.AsSpan(at - window, window).ToArray();
            var peak = CwSpectralPeak.Find(slice, rate);

            if (peak is null)
            {
                continue;
            }

            chose.Add(peak.Value);

            var holdingStrong = Math.Abs(peak.Value - StrongHz)
                                < Math.Abs(peak.Value - WeakHz);

            _output.WriteLine(
                $"{at / rate}\t{peak:0.0}\t{(holdingStrong ? "613" : "440")}");
        }

        Assert.NotEmpty(chose);

        var switches = 0;

        for (var i = 1; i < chose.Count; i++)
        {
            var was = Math.Abs(chose[i - 1] - StrongHz) < Math.Abs(chose[i - 1] - WeakHz);
            var now = Math.Abs(chose[i] - StrongHz) < Math.Abs(chose[i] - WeakHz);

            if (was != now)
            {
                switches++;
            }
        }

        _output.WriteLine("");
        _output.WriteLine(
            $"{switches} switches between stations across {chose.Count} readings");
    }

    /// <summary>What the old tone tracker does on the same input.</summary>
    /// <remarks>
    /// **THE ORDER ASKS FOR THIS AND THE TRACKER IS STILL IN THE TREE.** It is
    /// what `CwSpectralPeak` displaced in unit 050, and the difference the order
    /// suspects is that it **held** its pitch once locked where the peak
    /// re-measures from scratch every time. If the tracker holds through the same
    /// mix that moves the peak, that is the mechanism named.
    /// </remarks>
    [Fact]
    public void WhatTheOldTrackerDoesOnTheSameMix()
    {
        var strong = WavAudio.Read(PathOf(StrongName));
        var weak = WavAudio.Read(PathOf(WeakName));

        _output.WriteLine("secondDb	trackerHz	measured	holding");

        foreach (var db in new[] { -30.0, -18, -6, -3, 0, 6 })
        {
            var mixed = Mix(strong, weak, db);
            var tracker = new CwToneTracker(mixed.SampleRate, 600);
            var hop = tracker.HopSamples;

            for (var at = 0L; at + hop <= mixed.Samples.Length; at += hop)
            {
                tracker.Process(
                    mixed.Samples.AsSpan((int)at, hop), at, _ => { });
            }

            var hz = tracker.ToneHz;
            var holdingStrong = Math.Abs(hz - StrongHz) < Math.Abs(hz - WeakHz);

            _output.WriteLine(
                $"{db:+0;-0}	{hz:0.0}	{tracker.HasMeasuredPitch}	"
                + $"{(holdingStrong ? "613 (first)" : "440 (second)")}");
        }
    }

    /// <summary>The full path of a fixture.</summary>    /// <summary>The full path of a fixture.</summary>
    private static string PathOf(string name)
        => Path.Combine(
            CwFixtures.Folder,
            name.Replace('/', Path.DirectorySeparatorChar) + ".wav");

    /// <summary>One capture with a second mixed into it at a stated level.</summary>
    /// <remarks>
    /// The level is relative to the first capture's own root-mean-square, so
    /// "−6 dB" means the second signal's energy is a quarter of the first's rather
    /// than some absolute figure neither recording shares.
    /// </remarks>
    private static MonoAudio Mix(MonoAudio first, MonoAudio second, double secondDb)
    {
        var count = Math.Min(first.Samples.Length, second.Samples.Length);
        var scale = Rms(first, count) / Math.Max(Rms(second, count), 1e-12)
                    * Math.Pow(10, secondDb / 20);

        var samples = new float[count];

        for (var i = 0; i < count; i++)
        {
            samples[i] = (float)(first.Samples[i] + (second.Samples[i] * scale));
        }

        return new MonoAudio(first.SampleRate, samples);
    }

    /// <summary>Root mean square over the leading stretch.</summary>
    private static double Rms(MonoAudio audio, int count)
    {
        var sum = 0.0;

        for (var i = 0; i < count; i++)
        {
            sum += (double)audio.Samples[i] * audio.Samples[i];
        }

        return Math.Sqrt(sum / Math.Max(count, 1));
    }
}
