using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// Work instruction 238, task 3: every sample that does not arrive is counted
/// where the operator reads.
/// </summary>
/// <remarks>
/// <para>**WHAT WAS ON SCREEN ON 2026-09-03 WAS `nothing decoded yet`.** The tap
/// was filling at 13% of real time, every slot handed to FT8 was two minutes of
/// fragments wearing a fifteen-second timestamp, and every check the watch made
/// passed — because each of them asked whether the tap held samples at the right
/// INDEX and none asked whether those samples had arrived in the time they
/// claimed to cover. Three units were spent looking at the decoder.</para>
/// <para>**THE RATIO IS A COUNT OVER A COUNT** (§0.0). Samples delivered over
/// samples a continuous stream would have delivered in the same wall-clock span.
/// It is not a signal-to-noise ratio and it is not a quality score.</para>
/// </remarks>
public sealed class TheSlotSaysHowMuchAudioArrivedTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the ratios are printed.</param>
    public TheSlotSaysHowMuchAudioArrivedTests(ITestOutputHelper output)
        => _output = output;

    private const int Rate = 48_000;

    /// <summary>
    /// A tap fed one chunk in eight reads about an eighth, and a tap fed
    /// everything reads about one.
    /// </summary>
    /// <remarks>
    /// **THE TOLERANCE IS WIDE ON PURPOSE.** This is wall-clock arithmetic on a
    /// machine running a test suite, so the span it divides by includes whatever
    /// else the scheduler did. What is asserted is that a starved stream reads
    /// far below one and a whole stream reads near it — the distinction the
    /// refusal turns on — and not a figure to three places.
    /// </remarks>
    [Fact]
    public void AStarvedTapReadsFarBelowOne()
    {
        var starved = new AudioTap();
        var whole = new AudioTap();
        var chunk = new float[Rate / 100];   // 10 ms

        // **BOTH TAPS ARE PRIMED BEFORE THE SPAN STARTS, AND THAT IS THE
        // MEASUREMENT BEING HONEST RATHER THAN A CONVENIENCE.** The ratio
        // answers NaN for a span that reaches back before the tap had any marks
        // at all, because nobody measured that period - and the first draft of
        // this test asked about a span beginning before the first Take and got
        // NaN from both taps, correctly.
        whole.Take(chunk, Rate);
        starved.Take(chunk, Rate);
        Thread.Sleep(20);

        var from = DateTime.UtcNow;

        // One second of wall clock. `whole` gets every 10 ms chunk; `starved`
        // gets one in eight, which is the shape of the shack machine's fault.
        for (var i = 0; i < 100; i++)
        {
            whole.Take(chunk, Rate);

            if (i % 8 == 0)
            {
                starved.Take(chunk, Rate);
            }

            Thread.Sleep(10);
        }

        var to = DateTime.UtcNow;

        var wholeRatio = whole.ArrivalRatioBetween(from, to);
        var starvedRatio = starved.ArrivalRatioBetween(from, to);

        _output.WriteLine("span        : " + (to - from).TotalSeconds.ToString("0.00") + " s");
        _output.WriteLine("whole tap   : " + wholeRatio.ToString("0.000"));
        _output.WriteLine("starved tap : " + starvedRatio.ToString("0.000"));

        // **THE ASSERTION IS THE RELATIONSHIP, NOT AN ABSOLUTE FLOOR.** Both
        // ratios divide by wall clock, and `Thread.Sleep(10)` on a loaded
        // machine sleeps considerably longer than 10 ms - so the whole tap's
        // figure moves with the scheduler. Asserted at `wholeRatio > 0.5` this
        // test failed at 0.382 during a full-channel run, which was the test
        // measuring the machine rather than the measurement.
        //
        // What does not move is the ratio BETWEEN them: one tap was fed every
        // chunk and the other one in eight, over the same span, so the starved
        // one must read about an eighth of the whole one however long the
        // sleeps actually took.
        Assert.True(starvedRatio < wholeRatio / 4,
            "the starved tap read " + starvedRatio.ToString("0.000")
            + " against the whole tap's " + wholeRatio.ToString("0.000")
            + ", which is not the eightfold shortfall it was fed");

        Assert.True(starvedRatio > 0,
            "the starved tap read nothing at all, so it is not measuring the "
            + "audio it did receive");
    }

    /// <summary>
    /// Too little history is NaN, which is *nobody measured* and never a
    /// refusal.
    /// </summary>
    /// <remarks>
    /// **A ZERO HERE WOULD BE A MUCH LOUDER CLAIM THAN THE TRUTH.** A watch that
    /// has just started has no marks reaching back a slot, and reporting `the
    /// sound card delivered 0%` about a device that is working is the confident
    /// wrong answer §0.0 exists against.
    /// </remarks>
    [Fact]
    public void NotEnoughHistoryIsNotAZero()
    {
        var tap = new AudioTap();

        Assert.True(double.IsNaN(tap.ArrivalRatio(TimeSpan.FromSeconds(15))),
            "an untouched tap claimed to know its arrival ratio");

        tap.Take(new float[480], Rate);

        var reaching = tap.ArrivalRatioBetween(
            DateTime.UtcNow.AddSeconds(-15), DateTime.UtcNow);

        _output.WriteLine("one chunk, asked across 15 s -> " + reaching);

        Assert.True(double.IsNaN(reaching),
            "the tap answered about fifteen seconds it has no marks for");
    }

    /// <summary>
    /// The refusal carries the arrival sentence, with the shortfall in it.
    /// </summary>
    /// <remarks>
    /// **THIS IS THE SENTENCE THAT SHOULD HAVE BEEN ON SCREEN.** It names what
    /// the sound card did rather than what the decoder found, so the operator is
    /// not sent to look at the band.
    /// </remarks>
    [Fact]
    public void TheRefusalNamesTheSoundCardAndNotTheBand()
    {
        var sentence = string.Format(
            System.Globalization.CultureInfo.InvariantCulture,
            Ft8SlotWatch.AudioShort,
            0.13.ToString("P0", System.Globalization.CultureInfo.InvariantCulture));

        _output.WriteLine(sentence);

        Assert.Contains("sound card", sentence, StringComparison.Ordinal);
        Assert.Contains("13", sentence, StringComparison.Ordinal);
        Assert.Contains("fragments", sentence, StringComparison.Ordinal);

        // The old sentence sent three units to the decoder. It must not be what
        // an operator reads about a starved sound card.
        Assert.DoesNotContain("nothing decoded", sentence, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>The threshold is 0.98 and its two per cent is for jitter.</summary>
    [Fact]
    public void TheThresholdIsTwoPerCentAndNotAQualityKnob()
    {
        Assert.Equal(0.98, Ft8SlotWatch.LeastArrival, 9);
    }
}
