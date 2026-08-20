using Hamlet.RadioEngine.Cw;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// A count written beside a recording has to be a count of the recording
/// (HM-DEC-091).
/// </summary>
/// <remarks>
/// <para>**THE DECODER'S COUNTERS ARE CUMULATIVE AND NOTHING EVER SAID SO.**
/// They run from the moment listening starts until it stops. A capture sidecar
/// printed them beside thirty seconds of audio, and one written seven hours into
/// an evening carried a character count earned hours earlier on another band.
/// Two captures on different nights reported the same figures because neither
/// figure was about either night's audio.</para>
/// <para>These pin the derivation that replaces the bare numbers: the counters
/// only go up, so what happened across a stretch is the difference between the
/// readings at its two ends, and **a stretch the history cannot cover returns
/// nothing rather than a zero** (§0.0).</para>
/// </remarks>
public sealed class CwCounterTrailTests
{
    private const int Rate = 48_000;

    /// <remarks>
    /// Proves HM-DEC-091: the figure quoted for a window is the window's own,
    /// not the running total, and the two differ by everything that happened
    /// before it.
    /// </remarks>
    [Fact]
    public void AWindowCountsOnlyWhatHappenedInsideIt()
    {
        var trail = new CwCounterTrail(Rate * 60);

        // An hour of listening compressed into four readings: a busy stretch
        // early, then a quiet one, then the half minute a capture would keep.
        trail.Note(new CwCounterSample(Rate * 10, 700, 200, 60, 20));
        trail.Note(new CwCounterSample(Rate * 20, 740, 230, 69, 23));
        trail.Note(new CwCounterSample(Rate * 30, 740, 230, 69, 23));
        trail.Note(new CwCounterSample(Rate * 40, 812, 254, 76, 25));

        var window = trail.Over(Rate * 40, Rate * 20);

        Assert.NotNull(window);
        Assert.Equal(7, window!.Value.CharactersEmitted);
        Assert.Equal(2, window.Value.CharactersUnsure);
        Assert.Equal(72, window.Value.ElementsSeen);
        Assert.Equal(24, window.Value.ElementsResolved);
    }

    /// <remarks>
    /// Proves HM-DEC-091: **this is the shape of the fault as it happened.** The
    /// running totals stand at 69 and 233 while nothing at all was decoded across
    /// the recording, and the two numbers sat side by side with nothing to tell
    /// them apart.
    /// </remarks>
    [Fact]
    public void ARecordingWithNothingInItReportsNothing()
    {
        var trail = new CwCounterTrail(Rate * 60);

        trail.Note(new CwCounterSample(Rate * 100, 340_000, 233, 69, 23));
        trail.Note(new CwCounterSample(Rate * 130, 359_837, 233, 69, 23));

        var window = trail.Over(Rate * 130, Rate * 30);

        Assert.NotNull(window);
        Assert.Equal(0, window!.Value.CharactersEmitted);
        Assert.Equal(0, window.Value.ElementsResolved);

        // The elements kept climbing the whole time, which is a threshold being
        // crossed by noise rather than anybody keying.
        Assert.Equal(19_837, window.Value.ElementsSeen);
    }

    /// <remarks>
    /// Proves HM-DEC-091: the start of listening is a real reading, so a capture
    /// taken in the first half minute is covered rather than refused.
    /// </remarks>
    [Fact]
    public void TheStartOfListeningIsAMeasurementLikeAnyOther()
    {
        var trail = new CwCounterTrail(Rate * 60);

        trail.Note(new CwCounterSample(Rate * 8, 90, 30, 9, 2));

        var window = trail.Over(Rate * 8, Rate * 30);

        Assert.NotNull(window);
        Assert.Equal(9, window!.Value.CharactersEmitted);
        Assert.Equal(90, window.Value.ElementsSeen);
    }

    /// <remarks>
    /// Proves §0.0: a window the history cannot reach across is an unknown and
    /// not a zero. A zero here would say the decoder read nothing from audio it
    /// never had a reading for.
    /// </remarks>
    [Fact]
    public void AWindowTheHistoryCannotCoverIsNotGivenANumber()
    {
        var trail = new CwCounterTrail(Rate * 30);

        for (var second = 0; second <= 300; second += 1)
        {
            trail.Note(new CwCounterSample(
                (long)Rate * second, second * 4, second, second, 0));
        }

        Assert.Null(trail.Over((long)Rate * 300, (long)Rate * 120));
    }

    /// <remarks>
    /// Proves HM-DEC-091: the reading just behind the horizon is kept, because it
    /// is the one every window starts from. Trimming it would leave a trail that
    /// nominally covers half a minute and cannot answer about half a minute.
    /// </remarks>
    [Fact]
    public void TheReadingAWindowStartsFromSurvivesTrimming()
    {
        var trail = new CwCounterTrail(Rate * 30);

        for (var tick = 0; tick <= 4_000; tick++)
        {
            trail.Note(new CwCounterSample(
                (long)(Rate * 0.25 * tick), tick * 3, tick, tick, 0));
        }

        var window = trail.Over((long)(Rate * 0.25 * 4_000), Rate * 30);

        Assert.NotNull(window);
        Assert.Equal(120, window!.Value.CharactersEmitted);

        // And the history stays short: this runs on the same timer as the
        // readouts and must not grow for the length of an evening.
        Assert.InRange(trail.Count, 2, 400);
    }

    /// <remarks>
    /// Proves §0.0: the audio clock only goes forward. A reading from behind the
    /// newest one is dropped rather than woven in, because a decoder that was
    /// restarted gets a fresh trail and not a repaired one.
    /// </remarks>
    [Fact]
    public void AReadingFromTheStartOfTimeIsNotWovenIn()
    {
        var trail = new CwCounterTrail(Rate * 60);

        trail.Note(new CwCounterSample(Rate * 20, 400, 100, 30, 5));
        trail.Note(new CwCounterSample(Rate * 5, 10, 2, 1, 0));

        var window = trail.Over(Rate * 20, Rate * 20);

        Assert.NotNull(window);
        Assert.Equal(30, window!.Value.CharactersEmitted);
    }
}
