using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The check that tells Morse from an empty band measures the sender's own two
/// lengths, not a textbook's (HM-DEC-048, HM-DEC-115, HM-DEC-119).
/// </summary>
/// <remarks>
/// <para>**A REAL STATION AT 35 dB WAS DISCARDED FOR SENDING DAHS AT FOUR
/// DITS.** On `cw-2026-08-17-134712` the fist is steady and unremarkable: dits of
/// 55 ms, element gaps of 35, dahs of 235. Measured against a hardcoded three
/// dits, every dah is 1.3 dits out, the average error passes the half-dit limit,
/// and `LooksLikeMorse` is false on every hop of the recording. Isolated on the
/// four seconds where that fist is cleanest, with the dit estimate identical
/// either way, the textbook template scores it **0.00** and the sender's own
/// scores it 0.38.</para>
/// <para>**IT IS THE THIRD PLACE IN THIS DECODER TO ASSUME TEXTBOOK TIMING AND
/// THE OTHER TWO WERE ALREADY FIXED.** HM-DEC-115 clusters the sender's gaps
/// rather than taking multiples of the dit, because real operators send
/// Farnsworth. HM-DEC-119 cuts `ClassifyMark` between the two measured mark
/// clusters, fitted per signal, after finding a fist sending dahs at two and a
/// half. This one had a veto over the whole message and kept it.</para>
/// <para>**NOTHING HERE MAKES THE DECODER WILLING TO EMIT MORE** (HM-DEC-048).
/// The question is the same one and it is still the one that matters: do the
/// marks land on two lengths over and over. The control below is what proves it,
/// because noise has no two lengths to land on.</para>
/// </remarks>
public sealed class AHeavyFistIsStillMorseTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the coherence is printed.</param>
    public AHeavyFistIsStillMorseTests(ITestOutputHelper output) => _output = output;

    private const int Rate = 48_000;

    private static double Ms(double milliseconds) => Rate * milliseconds / 1000;

    /// <summary>Feed a fist of a given shape and see what is made of it.</summary>
    /// <param name="ditMs">The dit.</param>
    /// <param name="dahMs">The dah.</param>
    /// <param name="gapMs">The gap between elements.</param>
    /// <returns>The estimator, having heard twenty-four marks.</returns>
    private static CwSpeedEstimator Fist(double ditMs, double dahMs, double gapMs)
    {
        var speed = new CwSpeedEstimator(Rate);

        // Twenty-four marks, alternating in the proportion an ordinary message
        // has: rather more dits than dahs, and an element gap between each.
        for (var i = 0; i < 24; i++)
        {
            speed.AddMark(Ms(i % 3 == 2 ? dahMs : ditMs));
            speed.AddGap(Ms(gapMs));
        }

        return speed;
    }

    /// <remarks>
    /// <para>Proves HM-DEC-119's principle in the one place it was not applied:
    /// **a fist whose dah is 4.3 dits is Morse.** These are the lengths on
    /// `cw-2026-08-17-134712` measured to the millisecond, with the element gap
    /// left at a dit so that this test asks about the ratio and nothing
    /// else.</para>
    /// <para>Against a hardcoded three this scores nought. Against the sender's
    /// own dah it scores one.</para>
    /// </remarks>
    [Fact]
    public void ADahOfFourDitsIsStillADah()
    {
        var speed = Fist(ditMs: 55, dahMs: 235, gapMs: 55);

        _output.WriteLine(
            $"dit 55 dah 235: coherence {speed.Coherence:0.00}, "
            + $"{speed.WordsPerMinute} wpm, looksLikeMorse {speed.LooksLikeMorse}");

        Assert.True(
            speed.LooksLikeMorse,
            $"a steady fist at 4.3 dits scored {speed.Coherence:0.00}");
    }

    /// <remarks>
    /// <para>**A MEASUREMENT AND NOT A VERDICT, AND IT IS THE NEXT UNIT'S
    /// WORK.** The same fist with the element gap it actually sends, 35 ms
    /// against a 55 ms dit, comes out with the dit reading about forty-five.
    /// `Refine` averages the mark-derived dit with a gap-derived one, on the
    /// premise that a mark measured at a threshold comes out long by the same
    /// amount the gap after it comes out short.</para>
    /// <para>**HM-DEC-119 MEASURED THAT PREMISE AND FOUND IT FALSE**: the gate
    /// reads 100 to 110 ms for a true 100 at every speed, so the mark is not long
    /// and there is nothing to cancel. HM-DEC-115 measured the other half: a real
    /// fist's element gap is genuinely shorter than its dit, 40 ms against 57 on
    /// `cw-2026-08-18-004507`, because that is how people send. Averaging the two
    /// therefore shortens the dit by a fifth on any Farnsworth sender.</para>
    /// <para>This test asserts only the size of that bias, so a later session
    /// that changes `Refine` sees the number move. **It asserts nothing about
    /// whether the current behavior is right**, which it is not.</para>
    /// </remarks>
    [Fact]
    public void TheDitComesOutShortWhenTheGapIsShorterThanIt()
    {
        var speed = Fist(ditMs: 55, dahMs: 235, gapMs: 35);
        var ditMs = speed.DitSamples * 1000 / Rate;

        _output.WriteLine(
            $"true dit 55 ms, element gap 35 ms: estimated dit {ditMs:0.0} ms, "
            + $"{speed.WordsPerMinute} wpm, fitted dah {235 / ditMs:0.00} dits, "
            + $"coherence {speed.Coherence:0.00}");

        Assert.InRange(ditMs, 40, 50);
    }

    /// <remarks>
    /// Proves the same for the light fist HM-DEC-119 measured, at two and a half
    /// dits, so the band is shown to work at both ends rather than at the one
    /// that prompted it.
    /// </remarks>
    [Fact]
    public void ADahOfTwoAndAHalfDitsIsStillADah()
    {
        var speed = Fist(ditMs: 60, dahMs: 150, gapMs: 60);

        _output.WriteLine(
            $"dit 60 dah 150: coherence {speed.Coherence:0.00}, "
            + $"{speed.WordsPerMinute} wpm, looksLikeMorse {speed.LooksLikeMorse}");

        Assert.True(speed.LooksLikeMorse, $"scored {speed.Coherence:0.00}");
    }

    /// <remarks>
    /// Proves nothing was given away at the textbook ratio, which is what almost
    /// every keyer sends and what every other test in this suite is built on.
    /// </remarks>
    [Fact]
    public void TheTextbookFistIsUnaffected()
    {
        var speed = Fist(ditMs: 60, dahMs: 180, gapMs: 60);

        _output.WriteLine(
            $"dit 60 dah 180: coherence {speed.Coherence:0.00}, "
            + $"{speed.WordsPerMinute} wpm");

        Assert.True(speed.LooksLikeMorse);
        Assert.True(speed.Coherence > 0.9, $"scored {speed.Coherence:0.00}");
    }

    /// <remarks>
    /// <para>Proves §0.0 and HM-DEC-048: **the control, and it is the half that
    /// makes the rest safe.** Run lengths with no preferred value must not become
    /// Morse because the two lengths are now fitted rather than assumed. Noise has
    /// no two lengths to land on, and fitting a pair of centers to a smear leaves
    /// every mark a long way from both of them.</para>
    /// <para>The lengths are drawn the way a gate chopping an empty band produces
    /// them, exponentially, which is the distribution that has no preferred
    /// value at all.</para>
    /// </remarks>
    [Fact]
    public void NoiseDoesNotBecomeMorseBecauseTheDahIsFitted()
    {
        var random = new Random(7300);
        var speed = new CwSpeedEstimator(Rate);

        for (var i = 0; i < 40; i++)
        {
            speed.AddMark(Ms(10 - (40 * Math.Log(1 - random.NextDouble()))));
            speed.AddGap(Ms(10 - (40 * Math.Log(1 - random.NextDouble()))));
        }

        _output.WriteLine(
            $"noise: coherence {speed.Coherence:0.00}, {speed.WordsPerMinute} wpm, "
            + $"looksLikeMorse {speed.LooksLikeMorse}");

        Assert.False(
            speed.LooksLikeMorse,
            $"noise scored {speed.Coherence:0.00} against a floor of "
            + $"{CwSpeedEstimator.MinimumCoherence:0.00}");
    }

    /// <remarks>
    /// Proves §0.0: **a long mark that is not a dah is not treated as one.** Past
    /// five dits the long cluster is a carrier, a fade or somebody holding the key
    /// down to tune, and fitting to it would let a held key and a scattering of
    /// noise pass for a fist.
    /// </remarks>
    [Fact]
    public void AHeldKeyIsNotAFistWithAVeryLongDah()
    {
        var speed = new CwSpeedEstimator(Rate);

        for (var i = 0; i < 24; i++)
        {
            speed.AddMark(Ms(i % 3 == 2 ? 900 : 50));
            speed.AddGap(Ms(50));
        }

        _output.WriteLine(
            $"held key: coherence {speed.Coherence:0.00}, {speed.WordsPerMinute} wpm, "
            + $"looksLikeMorse {speed.LooksLikeMorse}");

        Assert.False(speed.LooksLikeMorse, $"scored {speed.Coherence:0.00}");
    }
}
