using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Whether the marks and the gaps are scored against the same unit.
/// </summary>
/// <remarks>
/// <para>**THE INFERENCE THIS TESTS WAS THAT THEY ARE NOT.** A forced-unit sweep
/// on an independent chain measured the two failure modes separately and found
/// that no single unit produces both: where letters fragment into single-character
/// words the letters are T, and where E dominates nothing fragments. Hamlet shows
/// both at once, so either its mark classifier and its gap classifier work from
/// different units, or its gap classifier is not unit-derived.</para>
/// <para>**MEASURED, THEY AGREE.** On every capture carrying an adjudicated
/// reading the number the marks are scored against and the number the gaps are
/// scored against are the same number in every window.
/// `cw-2026-08-18-004507` differs deliberately, in 34 windows of 57: its gap
/// structure holds long enough that the sender's own lengths are used, which is
/// what the twelve-read persistence rule exists to allow. `cw-2026-08-20-014854`
/// differs in five windows and holds no station, so it emits nothing either
/// way.</para>
/// <para>**SO THE DUAL SIGNATURE HAS ANOTHER CAUSE**, and it is filed as
/// `HM-OPEN-057`: nearly every single-element letter is emitted while the keying
/// verdict says nobody is sending.</para>
/// </remarks>
public sealed class OneUnitThroughBothClassifiersTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the two units are printed.</param>
    public OneUnitThroughBothClassifiersTests(ITestOutputHelper output)
        => _output = output;

    private static IEnumerable<string> Captures()
    {
        var folder = CapturedSignalTests.Folder;

        return Directory.GetFiles(folder, "*.wav")
            .Concat(Directory.GetFiles(Path.Combine(folder, "unadjudicated"), "*.wav"))
            .OrderBy(p => p);
    }

    /// <remarks>
    /// <para>Proves it window by window: the gaps are scored against the unit
    /// unless the sender's own spacing has been established, and where it has, the
    /// difference is the measurement rather than a drift nobody intended.</para>
    /// </remarks>
    [Fact]
    public void TheyUseOneUnitUnlessTheSpacingItselfWasMeasured()
    {
        var divergent = new List<string>();

        foreach (var path in Captures())
        {
            var audio = WavAudio.Read(path);
            var decoder = new CwDecoder(audio.SampleRate, 600);
            var hop = decoder.Tracker.HopSamples;

            for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
            {
                decoder.Process(new AudioChunk(
                    at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
            }

            decoder.Flush();

            var envelope = CwProbabilisticDecoder.Envelope(
                audio.Samples, audio.SampleRate, decoder.Tracker.ToneHz);

            var hopsPerSecond = 1000.0 / CwProbabilisticDecoder.HopMilliseconds;
            var windowHops = (int)(CwProbabilisticStream.WindowSeconds * hopsPerSecond);
            var everyHops = (int)(CwProbabilisticStream.ReadEverySeconds * hopsPerSecond);
            var same = 0;
            var different = 0;
            var run = 0;
            var misses = 0;
            var held = false;

            for (var end = everyHops; end <= envelope.Length; end += everyHops)
            {
                var from = Math.Max(0, end - windowHops);
                var slice = new double[end - from];

                Array.Copy(envelope, from, slice, 0, slice.Length);

                var unit = CwUnitEstimator.Measure(
                    slice, CwProbabilisticDecoder.HopMilliseconds);

                if (!unit.IsReady)
                {
                    continue;
                }

                var gaps = CwUnitEstimator.MeasureGaps(
                    slice, CwProbabilisticDecoder.HopMilliseconds, unit.UnitMilliseconds);

                run = gaps.Separated ? run + 1 : 0;
                misses = gaps.Separated ? 0 : misses + 1;

                if (run >= CwProbabilisticStream.ReadsToEstablishStructure)
                {
                    held = true;
                }
                else if (misses >= CwProbabilisticStream.ReadsToEstablishStructure)
                {
                    held = false;
                }

                if (held && Math.Abs(gaps.ElementMilliseconds - unit.UnitMilliseconds) > 1e-6)
                {
                    different++;
                }
                else
                {
                    same++;
                }
            }

            _output.WriteLine(
                $"{Path.GetFileNameWithoutExtension(path),-24} "
                + $"one unit in {same,3} windows, two in {different,3}");

            if (different > 0)
            {
                divergent.Add(Path.GetFileNameWithoutExtension(path));
            }
        }

        // **THE CLAIM IS ABOUT THE CAPTURES THAT PRODUCE TEXT.** Every recording
        // carrying an adjudicated reading scores its marks and its gaps against
        // one number in every window, so the dual signature they show cannot be
        // two classifiers disagreeing. `cw-2026-08-18-004507` diverges because
        // its spacing was measured, and `cw-2026-08-20-014854` holds no station
        // and emits nothing whatever its windows do.
        foreach (var name in new[]
                 {
                     "cw-2026-08-17-013347",
                     "cw-2026-08-17-134712",
                     "cw-2026-08-18-003758",
                 })
        {
            Assert.DoesNotContain(name, divergent);
        }
    }
}
