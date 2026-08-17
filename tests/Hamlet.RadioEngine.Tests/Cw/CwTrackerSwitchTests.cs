using Hamlet.RadioEngine.Cw;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Moving to a different station, and when it may not (HM-DEC-096, phase 3).
/// </summary>
/// <remarks>
/// Drift is handled by re-centering the fine bank and is not this. A different
/// station is usually a different pitch entirely, and both failure directions
/// are real: chase eagerly and the tracker abandons a fading station mid-word,
/// chase reluctantly and an answer two hundred hertz away is never heard.
/// </remarks>
public sealed class CwTrackerSwitchTests
{
    /// <remarks>
    /// <para>Proves HM-DEC-096 phase 3 and §0.0: **the tracker holds still while
    /// a character is part-read.** Moving the filter part-way through one
    /// assembles the rest of it from a different station, and what comes out is a
    /// letter nobody sent, with clean timing and a healthy margin above the
    /// noise. That is the same class of confident wrong reading the
    /// truncated-evidence rule exists to prevent, and it costs at most one
    /// character to avoid.</para>
    /// </remarks>
    [Fact]
    public void NoSwitchHappensPartWayThroughACharacter()
    {
        var tracker = new CwToneTracker(48_000, 600);

        Assert.False(tracker.MidCharacter);

        tracker.MidCharacter = true;

        var before = tracker.Retunes;

        // Half a minute of a keyed station two hundred hertz away, which is
        // exactly the case that should move the tracker, arriving while a
        // character is open.
        Feed(tracker, 400, seconds: 12);

        Assert.Equal(before, tracker.Retunes);
    }

    /// <remarks>
    /// Proves HM-DEC-096 phase 3: **a held switch is not an abandoned one.** The
    /// candidate keeps being confirmed while it waits, so the move happens as
    /// soon as the character closes rather than needing to be found again.
    /// </remarks>
    [Fact]
    public void AHeldSwitchHappensOnceTheCharacterCloses()
    {
        var tracker = new CwToneTracker(48_000, 600);

        tracker.MidCharacter = true;
        Feed(tracker, 400, seconds: 12);

        var held = tracker.Retunes;

        tracker.MidCharacter = false;
        Feed(tracker, 400, seconds: 3);

        Assert.True(
            tracker.Retunes > held,
            "the switch was held and then never taken");

        Assert.InRange(tracker.ToneHz, 370, 430);
    }

    /// <summary>
    /// Push keyed Morse at one pitch through the tracker.
    /// </summary>
    private static void Feed(CwToneTracker tracker, double toneHz, double seconds)
    {
        const int rate = 48_000;

        var samples = new float[rate / 10];
        var phase = 0.0;
        var step = 2 * Math.PI * toneHz / rate;
        var written = 0L;

        // A dit of about a hundred milliseconds and a dah of three hundred, with
        // ordinary spacing: something the survey can find a clock in.
        var pattern = new[] { 100, 100, 100, 300, 300, 100, 300, 100 };
        var at = 0;
        var remaining = pattern[0] * rate / 1000;
        var keyed = true;

        for (var block = 0; block < seconds * 10; block++)
        {
            for (var i = 0; i < samples.Length; i++)
            {
                if (remaining-- <= 0)
                {
                    keyed = !keyed;
                    at = keyed ? (at + 1) % pattern.Length : at;
                    remaining = (keyed ? pattern[at] : 120) * rate / 1000;
                }

                phase += step;
                samples[i] = keyed
                    ? (float)(0.30 * Math.Sin(phase))
                    : (float)(0.002 * Math.Sin(phase * 1.7));
            }

            tracker.Process(samples, written, _ => { });
            written += samples.Length;
        }
    }
}
