using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The transmission latch (HM-DEC-085).
/// </summary>
/// <remarks>
/// <para>**THESE TESTS EXIST BECAUSE THE LAST ONES PASSED.** A latch was built,
/// it was tested, and it failed on the radio, because no test crossed the one
/// boundary that mattered: the send call returns in about thirteen milliseconds
/// and the radio then keys for eighteen seconds on its own.</para>
/// <para>So the line here is not toggled by hand. It is driven by
/// <see cref="MorseCode.IsKeyDown"/> from the real key pattern of the real
/// message, sampled at the rate the rig is really polled, which is the closest a
/// test can get to the radio without one being plugged in.</para>
/// </remarks>
public sealed class TransmissionWatchTests
{
    private static readonly DateTime Start = new(2026, 8, 15, 20, 0, 0, DateTimeKind.Utc);

    private const string Cq = "CQ CQ CQ DE KC3QIS KC3QIS K";
    private const int Wpm = 20;

    /// <summary>How often the rig state is polled while the window is visible.</summary>
    private static readonly TimeSpan Poll = TimeSpan.FromMilliseconds(250);

    /// <remarks>
    /// <para>Proves HM-DEC-085, and it is the test the previous attempt did not
    /// have. A whole CQ is sent with the transmit line driven by the message's
    /// own keying, so it drops and rises many dozens of times, and the send state
    /// changes **exactly once in each direction**.</para>
    /// <para>**COUNTING THE CHANGES IS NOT ENOUGH, AND FINDING THAT OUT IS WHY
    /// THIS TEST LOOKS LIKE THIS.** Written as a count alone it passed against
    /// plain edge detection, which ends the transmission at the first gap between
    /// the first two dits. It passed because the latch can only change twice by
    /// construction: once it is down it is never raised again, so counting its
    /// changes cannot tell a latch that held from a latch that let go
    /// immediately.</para>
    /// <para>So what is asserted is that it was still latched at every single
    /// sample up to the moment the message could possibly have ended, which is
    /// the property the operator actually cares about. The line's own transition
    /// count is asserted too, so a simulation that stopped flapping could not let
    /// this pass by becoming easy.</para>
    /// </remarks>
    [Fact]
    public void AFlappingTransmitLineHoldsTheStateThroughTheWholeMessage()
    {
        var watch = new TransmissionWatch();
        var changes = 0;

        watch.Begin(Cq, Wpm, Start);
        Assert.True(watch.IsSending);
        var was = true;

        var (line, transitions) = Keying(Cq, Wpm);
        var expected = CwDuration.Of(Cq, Wpm);
        var heldThrough = 0;

        for (var i = 0; i < line.Count; i++)
        {
            var now = Start + Poll * (i + 1);
            watch.Observe(line[i], now);

            if (watch.IsSending != was)
            {
                changes++;
                was = watch.IsSending;
            }

            // Not one sample early. This is the assertion edge detection fails.
            if (now - Start < expected)
            {
                Assert.True(
                    watch.IsSending,
                    $"the latch let go {expected - (now - Start)} before the end");
                heldThrough++;
            }
        }

        // The line really did flap: this is the condition that broke the panel.
        Assert.True(transitions > 20, $"the simulated line only moved {transitions} times");
        Assert.True(heldThrough > 50, "the message was too short to be a real test");

        Assert.Equal(1, changes);
        Assert.False(watch.IsSending);
        Assert.Equal(TransmissionEnd.Finished, watch.Outcome);
    }

    /// <remarks>
    /// Proves HM-DEC-085: the reported duration is the transmission and not the
    /// handover. The old figure was a hundredth of a second for this exact
    /// message, and it reached the operator as "the radio keyed for 0 seconds".
    /// </remarks>
    [Fact]
    public void TheReportedSecondsAreTheTransmissionAndNotTheHandover()
    {
        var watch = new TransmissionWatch();
        watch.Begin(Cq, Wpm, Start);

        var (line, _) = Keying(Cq, Wpm);

        for (var i = 0; i < line.Count; i++)
        {
            watch.Observe(line[i], Start + Poll * (i + 1));
        }

        // Not a hundredth of a second, and not the hold-off tacked on either.
        Assert.InRange(watch.Elapsed.TotalSeconds, 15.0, 19.0);
        Assert.True(watch.Keyed);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-085 against the measurement that shaped it. Sampling
    /// the transmit line four times a second against sixty-millisecond dits does
    /// not watch the keying, it beats against it, and a real CQ contains a
    /// stretch of **a second and a half** where no sample catches the key down.
    /// </para>
    /// <para>This test asserts that quiet is really in there, so that if a future
    /// change makes the simulation stop reproducing it, the hold-off tests
    /// afterward cannot pass by having become easy.</para>
    /// </remarks>
    [Fact]
    public void SamplingAliasesIntoLongStretchesOfApparentQuiet()
    {
        var (line, _) = Keying(Cq, Wpm);
        var longest = 0;
        var run = 0;

        // Only inside the message; the tail afterward is real quiet.
        var during = (int)(CwDuration.Of(Cq, Wpm).TotalMilliseconds / Poll.TotalMilliseconds);

        for (var i = 0; i < during && i < line.Count; i++)
        {
            run = line[i] == true ? 0 : run + 1;
            longest = Math.Max(longest, run);
        }

        var quiet = Poll * longest;

        Assert.True(
            quiet > TimeSpan.FromSeconds(1),
            $"the aliasing is gone: longest apparent quiet was only {quiet}");

        // And the hold-off is deliberately shorter than that, which is why the
        // computed duration has to be a floor rather than a suggestion.
        Assert.True(CwDuration.Silence(Wpm) < quiet);
    }

    /// <remarks>
    /// Proves HM-DEC-085 and §0.2: stopping ends the state on the spot, with no
    /// hold-off, no poll and nothing awaited.
    /// </remarks>
    [Fact]
    public void StoppingEndsItImmediately()
    {
        var watch = new TransmissionWatch();
        watch.Begin(Cq, Wpm, Start);

        var atTwoSeconds = Start + TimeSpan.FromSeconds(2);
        watch.Observe(true, atTwoSeconds);

        Assert.True(watch.Stop(atTwoSeconds));

        Assert.False(watch.IsSending);
        Assert.Equal(TransmissionEnd.Stopped, watch.Outcome);
        Assert.Equal(2.0, watch.Elapsed.TotalSeconds, precision: 3);

        // And it cannot end twice.
        Assert.False(watch.Stop(atTwoSeconds));
    }

    /// <remarks>
    /// Proves HM-DEC-085: the longest legitimate silence in a message does not
    /// end it. A word gap at thirteen words a minute is well over half a second,
    /// and this is the failure that would put the blinking back.
    /// </remarks>
    [Fact]
    public void AWordGapDoesNotEndIt()
    {
        foreach (var wpm in new[] { 5, 13, 20, 30 })
        {
            var watch = new TransmissionWatch();
            watch.Begin(Cq, wpm, Start);
            watch.Observe(true, Start);

            var wordGap = CwDuration.Dit(wpm) * 7;
            watch.Observe(false, Start + wordGap);

            Assert.True(watch.IsSending, $"a word gap ended it at {wpm} words a minute");
        }
    }

    /// <remarks>
    /// Proves HM-DEC-085: a radio whose transmit line cannot be read still ends
    /// the state, on the arithmetic, and says that is what happened. Unknown is a
    /// state rather than a number (HM-DEC-050), so the outcome distinguishes what
    /// Hamlet watched from what it worked out (§0.0).
    /// </remarks>
    [Fact]
    public void AnUnreadableTransmitLineStillEndsOnTheArithmetic()
    {
        var watch = new TransmissionWatch();
        watch.Begin(Cq, Wpm, Start);

        for (var i = 1; i < 200; i++)
        {
            watch.Observe(null, Start + Poll * i);
        }

        Assert.False(watch.IsSending);
        Assert.Equal(TransmissionEnd.Expected, watch.Outcome);
        Assert.False(watch.Keyed);
        Assert.True(watch.Elapsed >= watch.Expected);
    }

    /// <remarks>
    /// Proves HM-DEC-085: a keyer slower than the reading does not end the state
    /// early. The computed duration describes; the hold-off decides.
    /// </remarks>
    [Fact]
    public void AKeyerSlowerThanTheReadingHoldsTheStateOpen()
    {
        var watch = new TransmissionWatch();

        // Told twenty, sending at twelve.
        watch.Begin(Cq, Wpm, Start);
        var (line, _) = Keying(Cq, 12);

        var i = 0;
        for (; i < line.Count; i++)
        {
            watch.Observe(line[i], Start + Poll * (i + 1));
        }

        Assert.Equal(TransmissionEnd.Finished, watch.Outcome);

        // Well past the computed eighteen seconds, and it stayed latched.
        Assert.True(
            watch.Elapsed > watch.Expected,
            $"ended at {watch.Elapsed} against an expected {watch.Expected}");
    }

    /// <remarks>
    /// Proves HM-DEC-085: what the operator reads runs forward and never
    /// backward, and never past the end.
    /// </remarks>
    [Fact]
    public void ProgressRunsForwardAndStops()
    {
        var watch = new TransmissionWatch();
        watch.Begin(Cq, Wpm, Start);

        var last = -1.0;
        for (var i = 0; i < 120; i++)
        {
            var now = Start + Poll * i;
            var progress = watch.Progress(now);

            Assert.InRange(progress, 0.0, 1.0);
            Assert.True(progress >= last, "progress went backward");
            last = progress;

            Assert.True(watch.Remaining(now) >= TimeSpan.Zero);
        }
    }

    /// <summary>
    /// The transmit line as the radio would really drive it, sampled at the poll
    /// rate, with a tail of quiet after the message.
    /// </summary>
    private static (List<bool?> Line, int Transitions) Keying(string message, int wpm)
    {
        var pattern = MorseCode.KeyPattern(message);
        var dits = MorseCode.LengthInDits(message);
        var dit = MorseCode.Dit(wpm).TotalMilliseconds;
        var total = dits * dit;

        var line = new List<bool?>();
        var transitions = 0;
        var was = false;

        for (var t = Poll.TotalMilliseconds; t < total + 4000; t += Poll.TotalMilliseconds)
        {
            // A repeat gap far longer than the message, so IsKeyDown's wrap
            // never puts a second copy of the message inside this window.
            var down = t <= total
                && MorseCode.IsKeyDown(pattern, dits, dits * 10, t / dit);

            if (down != was)
            {
                transitions++;
                was = down;
            }

            line.Add(down);
        }

        return (line, transitions);
    }
}
