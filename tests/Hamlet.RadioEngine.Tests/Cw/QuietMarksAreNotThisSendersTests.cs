using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Marks too quiet to be this sender's are left out of the estimate, and only
/// where the heights say there are two of them (HM-DEC-144).
/// </summary>
/// <remarks>
/// <para>**LENGTH CANNOT TELL A MERGED ELEMENT FROM A SLIVER OF NOISE AND HEIGHT
/// CAN.** Measured on `cw-2026-08-17-134712`, where HM-DEC-144 settles which
/// marks belong to the station: its eleven elements stand 24.4 to 24.7 dB above
/// the envelope floor and its nine chatter slivers stand at 8.1 to 14.2. On
/// `tightfist-easy`, where every mark is real, there is one height population and
/// no low group at all.</para>
/// <para>**THE DANGEROUS FAILURE IS THE NO-OP CASE AND THAT IS WHAT MOST OF THIS
/// FILE TESTS.** A rule that quietly discards the quiet end of everything would
/// help one recording and cost every fixture in the suite, which is exactly how
/// the previous two attempts at this line failed. So the tests below spend more
/// effort on proving that nothing is dropped than on proving that something
/// is.</para>
/// </remarks>
public sealed class QuietMarksAreNotThisSendersTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the counts are printed.</param>
    public QuietMarksAreNotThisSendersTests(ITestOutputHelper output)
        => _output = output;

    private const int Rate = 48_000;

    private static double Ms(double milliseconds) => Rate * milliseconds / 1000;

    /// <remarks>
    /// Proves HM-DEC-144: a station's elements and the gate's slivers, at the
    /// heights measured on `cw-2026-08-17-134712`, and the slivers are left out.
    /// </remarks>
    [Fact]
    public void TheQuietOnesAreLeftOut()
    {
        var speed = new CwSpeedEstimator(Rate);

        // Nine slivers at the chatter's own measured heights, then the eleven
        // elements of N4L at the station's.
        foreach (var length in new[] { 35, 15, 20, 5, 40, 15, 15, 25, 45 })
        {
            speed.AddMark(Ms(length), 11.8, 16.1);
        }

        foreach (var length in new[] { 225, 55, 55, 55, 60, 55, 245, 60, 245, 55, 55 })
        {
            speed.AddMark(Ms(length), 24.6, 24.9);
        }

        _output.WriteLine(
            $"{speed.MarkCount} marks, {speed.KeptMarks} kept, "
            + $"dit {speed.DitSamples * 1000 / Rate:0.0} ms, "
            + $"coherence {speed.Coherence:0.00}");

        Assert.Equal(20, speed.MarkCount);
        Assert.Equal(11, speed.KeptMarks);
    }

    /// <remarks>
    /// <para>Proves the no-op: **a sender whose marks are all one height loses
    /// none of them**, however short some of them are. This is `tightfist-easy`
    /// and every clean fixture in the suite, and a rule that failed it would be
    /// discarding the quiet end of everything.</para>
    /// </remarks>
    [Fact]
    public void ASenderWhoseMarksAreAllOneHeightLosesNone()
    {
        var speed = new CwSpeedEstimator(Rate);
        var random = new Random(7300);

        for (var i = 0; i < 24; i++)
        {
            // Ordinary scatter on the height, an ordinary decibel either way,
            // which is what a real signal gives even when nothing is wrong.
            var height = 24 + ((random.NextDouble() - 0.5) * 2);

            speed.AddMark(Ms(i % 3 == 2 ? 180 : 60), height, height + 0.4);
        }

        _output.WriteLine($"{speed.MarkCount} marks, {speed.KeptMarks} kept");

        Assert.Equal(speed.MarkCount, speed.KeptMarks);
    }

    /// <remarks>
    /// <para>Proves §0.0: **a window that is entirely chatter has one height
    /// population too**, so nothing is dropped and the estimator behaves exactly
    /// as it always did. This rule cannot rescue an empty band and does not
    /// try.</para>
    /// </remarks>
    [Fact]
    public void AnEmptyBandLosesNothingEither()
    {
        var speed = new CwSpeedEstimator(Rate);
        var random = new Random(4242);

        for (var i = 0; i < 24; i++)
        {
            var height = 12 + ((random.NextDouble() - 0.5) * 4);

            speed.AddMark(
                Ms(10 - (30 * Math.Log(1 - random.NextDouble()))), height, height + 3);
        }

        _output.WriteLine($"{speed.MarkCount} marks, {speed.KeptMarks} kept");

        Assert.Equal(speed.MarkCount, speed.KeptMarks);
        Assert.False(speed.LooksLikeMorse);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-144: **a real mark the gate held open across a dip is
    /// kept.** On `cw-2026-08-18-004507` there are elements of 90 to 275 ms whose
    /// average height sags to 11 to 15 dB while their loudest moment stays with
    /// the plateau at 18 to 26, and reading the average alone would discard
    /// them.</para>
    /// </remarks>
    [Fact]
    public void AMarkTheGateHeldOpenAcrossADipIsKept()
    {
        var speed = new CwSpeedEstimator(Rate);

        foreach (var length in new[] { 35, 15, 20, 5, 40, 15, 15, 25, 45 })
        {
            speed.AddMark(Ms(length), 11.8, 16.1);
        }

        foreach (var length in new[] { 60, 60, 180, 60, 60, 180, 60, 60, 180, 60 })
        {
            speed.AddMark(Ms(length), 25.9, 26.1);
        }

        // The stretched one: its average is down among the chatter and its
        // loudest moment is not.
        speed.AddMark(Ms(205), 13.0, 23.2);

        _output.WriteLine($"{speed.MarkCount} marks, {speed.KeptMarks} kept");

        Assert.Equal(11, speed.KeptMarks);
    }

    /// <remarks>
    /// Proves HM-DEC-091: **a mark whose height is not known is not given a
    /// plausible one.** Every caller that has no amplitude to offer gets exactly
    /// the behavior the estimator had before this rule existed.
    /// </remarks>
    [Fact]
    public void MarksWithNoHeightAreAllKept()
    {
        var speed = new CwSpeedEstimator(Rate);

        for (var i = 0; i < 24; i++)
        {
            speed.AddMark(Ms(i % 4 == 0 ? 15 : 60));
        }

        Assert.Equal(speed.MarkCount, speed.KeptMarks);
    }

    /// <remarks>
    /// <para>Proves it end to end on the recording it came from: **inside `N4L`
    /// the rule keeps about half the window and the dit rises toward the truth.**
    /// HM-DEC-144 gives that dit as 56.3 ms; before this rule the estimator read
    /// 35 to 40 there.</para>
    /// <para>The bound is deliberately loose. What is asserted is that the dit
    /// moved a long way toward a known number and that the window was genuinely
    /// filtered, not that either figure is exact, because `Refine` still holds
    /// the dit short by about a fifth and is a separate question.</para>
    /// </remarks>
    [Fact]
    public void InsideTheCallsignHalfTheWindowIsSetAside()
    {
        var audio = WavAudio.Read(
            Path.Combine(CapturedSignalTests.Folder, "cw-2026-08-17-134712.wav"));

        var decoder = new CwDecoder(audio.SampleRate, 500);
        var hop = decoder.Tracker.HopSamples;
        var keptLow = int.MaxValue;
        var ditHigh = 0.0;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));

            var seconds = at / (double)audio.SampleRate;

            if (seconds is < 21.4 or > 23.1)
            {
                continue;
            }

            keptLow = Math.Min(keptLow, decoder.Timing.KeptMarks);
            ditHigh = Math.Max(
                ditHigh, decoder.Timing.DitSamples * 1000 / audio.SampleRate);
        }

        _output.WriteLine(
            $"inside the callsign: as few as {keptLow} marks of "
            + $"{decoder.Timing.MarkCount} kept, dit reached {ditHigh:0.0} ms "
            + "against a known 56.3");

        Assert.InRange(keptLow, 5, 14);
        Assert.InRange(ditHigh, 44, 60);
    }
}
