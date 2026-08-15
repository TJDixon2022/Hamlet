using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Cw;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The question this application exists to answer (HM-DEC-082).
/// </summary>
/// <remarks>
/// <para>"Am I speaking into the void, as in nothing is going out, or am I on
/// the air and nobody is listening?" Six years of not being able to tell those
/// apart is the problem Hamlet was built for.</para>
/// <para>A failure at link 3 and a failure at link 5 are completely different
/// facts about the world, and they used to look identical to the operator:
/// silence. These tests exist to keep them apart, and to keep every number in
/// the sentence attached to something that was actually measured.</para>
/// </remarks>
public sealed class TransmitChainTests
{
    private static TransmitEvidence Good(
        bool acknowledged = true,
        double? keyed = 18.6,
        int? power = 143,
        int? swr = 30,
        int reports = 0,
        int? skimmers = 41,
        string band = "20 m")
        => new(acknowledged, keyed, power, swr, reports, skimmers, band);

    // ---- The two failures that must never look alike ---------------------

    /// <remarks>
    /// Proves HM-DEC-082, and this is the whole point. **A station making no
    /// power and a band with nobody listening produce different sentences**,
    /// and neither of them is "nothing called yet".
    /// </remarks>
    [Fact]
    public void NoPowerAndNoListenersAreDifferentSentences()
    {
        var silent = TransmitChain.Describe(Good(power: 0));
        var unheard = TransmitChain.Describe(Good(power: 143, skimmers: 41));

        Assert.NotEqual(silent, unheard);

        // The broken station is told its station is the problem, by measurement.
        Assert.Contains("made no power", silent, StringComparison.Ordinal);
        Assert.Contains("Nothing went on the air", silent, StringComparison.Ordinal);

        // The working station is told it worked and nobody answered.
        Assert.Contains("Your call went out", unheard, StringComparison.Ordinal);
        Assert.Contains("41 skimmers", unheard, StringComparison.Ordinal);
        Assert.DoesNotContain("Nothing went on the air", unheard, StringComparison.Ordinal);

        // And neither is the silence that started all this.
        foreach (var said in new[] { silent, unheard })
        {
            Assert.DoesNotContain("nothing called yet", said, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Equal(TransmitLink.PowerMade, TransmitChain.BrokeAt(Good(power: 0)));
        Assert.Equal(TransmitLink.Copied, TransmitChain.BrokeAt(Good()));
    }

    /// <remarks>
    /// Proves HM-DEC-082: each link's failure is named as itself, and a whole
    /// chain names nothing.
    /// </remarks>
    [Theory]
    [InlineData(false, 18.6, 143, 1, null)]
    [InlineData(true, 0.0, 143, 1, TransmitLink.RadioKeyed)]
    [InlineData(true, 18.6, 0, 1, TransmitLink.PowerMade)]
    [InlineData(true, 18.6, 143, 0, TransmitLink.Copied)]
    [InlineData(true, 18.6, 143, 1, null)]
    public void EachLinkFailsAsItself(
        bool acknowledged, double keyed, int power, int reports, TransmitLink? expected)
    {
        var evidence = Good(acknowledged, keyed, power, reports: reports);

        // The unacknowledged case is its own link, checked separately because
        // the theory cannot express two different expectations for one row.
        if (!acknowledged)
        {
            Assert.Equal(TransmitLink.CommandSent, TransmitChain.BrokeAt(evidence));
            return;
        }

        Assert.Equal(expected, TransmitChain.BrokeAt(evidence));
    }

    // ---- Every number is measured or it is not shown ---------------------

    /// <remarks>
    /// Proves HM-DEC-082 and §0.0: **a link Hamlet could not read says so and
    /// invents nothing.** "Hamlet could not read the power meter" is honest and
    /// useful; a plausible figure is a guess dressed as a decode and destroys the
    /// only thing this feature is for.
    /// </remarks>
    [Fact]
    public void AnUnreadLinkSaysSoAndInventsNothing()
    {
        var said = TransmitChain.Describe(Good(power: null));

        Assert.Contains("could not read the power meter", said, StringComparison.Ordinal);
        Assert.Contains(
            "cannot say whether anything left the antenna", said, StringComparison.Ordinal);

        // NOT A FAILURE. Not knowing whether power was made is different from
        // knowing none was, and reporting the first as the second would tell
        // somebody their station is broken on the strength of a read that did
        // not come back.
        Assert.NotEqual(TransmitLink.PowerMade, TransmitChain.BrokeAt(Good(power: null)));
        Assert.DoesNotContain("made no power", said, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves HM-DEC-082: an unread SWR meter drops the match clause rather than
    /// filling it in, and the power still gets said.
    /// </remarks>
    [Fact]
    public void AnUnreadMeterDropsItsClauseRatherThanGuessing()
    {
        var said = TransmitChain.Describe(Good(swr: null));

        Assert.Contains("percent of full power", said, StringComparison.Ordinal);
        Assert.DoesNotContain("match", said, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves HM-DEC-082 and HM-DEC-074: **a percentage and never a wattage.**
    /// The meter reports a position on its own scale and Icom's faces are not
    /// linear in watts, so the obvious arithmetic is wrong and §4 has no
    /// citation for the real curve. A figure in watts here would be an invented
    /// number underwriting the one claim this whole feature makes.
    /// </remarks>
    [Fact]
    public void ThePowerIsAPercentageAndNeverAWattage()
    {
        for (var reading = 0; reading <= 255; reading += 5)
        {
            var said = TransmitChain.Describe(Good(power: reading)).ToLowerInvariant();

            Assert.DoesNotContain("watt", said, StringComparison.Ordinal);
        }
    }

    // ---- Zero listeners and unknown listeners --------------------------

    /// <remarks>
    /// Proves HM-DEC-082: **a zero skimmer count and an unavailable one produce
    /// different sentences.** An absent number reads as zero to somebody who has
    /// been disappointed before, and those are opposite facts about the evening.
    /// </remarks>
    [Fact]
    public void ZeroListenersAndUnknownListenersAreDifferent()
    {
        var none = TransmitChain.Describe(Good(skimmers: 0));
        var unknown = TransmitChain.Describe(Good(skimmers: null));
        var many = TransmitChain.Describe(Good(skimmers: 41));

        Assert.NotEqual(none, unknown);
        Assert.NotEqual(none, many);
        Assert.NotEqual(unknown, many);

        Assert.Contains("no machine listening at all", none, StringComparison.Ordinal);
        Assert.Contains("could not find out how many", unknown, StringComparison.Ordinal);
        Assert.Contains("41 skimmers were reporting", many, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves HM-DEC-082: where somebody did copy him, that is the sentence, and
    /// it is stated plainly without decoration.
    /// </remarks>
    [Fact]
    public void BeingHeardIsSaidPlainly()
    {
        Assert.Contains(
            "One skimmer reported hearing you",
            TransmitChain.Describe(Good(reports: 1)),
            StringComparison.Ordinal);

        Assert.Contains(
            "7 skimmers reported hearing you",
            TransmitChain.Describe(Good(reports: 7)),
            StringComparison.Ordinal);

        Assert.Null(TransmitChain.BrokeAt(Good(reports: 1)));
    }

    /// <remarks>
    /// Proves HM-DEC-082: the honesty already written for the heard panel lives
    /// here too. Skimmer coverage is uneven, and a band can be wide open to
    /// people while empty of machines, so no report is not proof nobody heard
    /// him.
    /// </remarks>
    [Fact]
    public void SilenceIsNeverProofOfAnything()
    {
        Assert.Contains(
            "not proof nobody heard you",
            TransmitChain.Describe(Good(skimmers: 41)),
            StringComparison.Ordinal);

        Assert.Contains(
            "not the same as nobody hearing you",
            TransmitChain.Describe(Good(skimmers: 0)),
            StringComparison.Ordinal);
    }

    // ---- It never diagnoses the station ---------------------------------

    /// <remarks>
    /// Proves HM-DEC-082 and §0.0: **nothing in the chain output diagnoses the
    /// station or names a cause.** "Made no power" is a reading. "Your antenna is
    /// disconnected" is a guess about somebody's equipment, and the prohibition
    /// that governs the SWR report governs the whole chain. Swept across every
    /// combination the way the SWR test is.
    /// </remarks>
    [Fact]
    public void NothingDiagnosesTheStation()
    {
        foreach (var acknowledged in new[] { true, false })
        foreach (var keyed in new double?[] { null, 0, 18.6 })
        foreach (var power in new int?[] { null, 0, 40, 143, 213, 255 })
        foreach (var swr in new int?[] { null, 0, 48, 120, 200 })
        foreach (var reports in new[] { 0, 3 })
        foreach (var skimmers in new int?[] { null, 0, 41 })
        {
            var said = TransmitChain
                .Describe(new TransmitEvidence(
                    acknowledged, keyed, power, swr, reports, skimmers, "20 m"))
                .ToLowerInvariant();

            foreach (var claim in new[]
                     {
                         "your antenna", "antenna is disconnected", "dummy load",
                         "is broken", "your radio is", "check your", "faulty",
                         "something is wrong", "you should", "you must",
                         "is misconfigured", "bad connection", "try again",
                     })
            {
                Assert.False(said.Contains(claim, StringComparison.Ordinal),
                    $"'{claim}' appeared in: {said}");
            }
        }
    }

    /// <remarks>
    /// Proves HM-DEC-082: the count is described as machines that reported
    /// somebody, not as machines that were listening. A skimmer hearing nothing
    /// publishes nothing, so it cannot be counted, and "41 were listening" would
    /// claim more than the wire supports.
    /// </remarks>
    [Fact]
    public void TheCountIsDescribedAsWhatItActuallyMeasures()
    {
        var said = TransmitChain.Describe(Good(skimmers: 41));

        Assert.Contains("reporting other stations", said, StringComparison.Ordinal);
        Assert.DoesNotContain("41 skimmers were listening", said, StringComparison.Ordinal);
    }

    // ---- The scale ------------------------------------------------------

    /// <remarks>
    /// Proves HM-DEC-082: the Po scale converts correctly at its three cited
    /// points (p. 19-3), read as the manual's decimal column and not as
    /// hexadecimal.
    /// </remarks>
    [Theory]
    [InlineData(0, 0.0)]
    [InlineData(143, 50.0)]
    [InlineData(213, 100.0)]
    [InlineData(255, 100.0)]
    public void ThePowerScaleConvertsAtTheCitedPoints(int reading, double percent)
        => Assert.Equal(percent, CivPowerOut.Percent(reading));

    /// <remarks>
    /// Proves HM-DEC-082: only a genuine zero counts as no power, so a low but
    /// real reading is never reported as a dead transmitter.
    /// </remarks>
    [Fact]
    public void OnlyARealZeroCountsAsNoPower()
    {
        Assert.True(CivPowerOut.IsSilent(0));
        Assert.False(CivPowerOut.IsSilent(3));
        Assert.False(CivPowerOut.IsSilent(143));
        Assert.Equal("no power at all", CivPowerOut.Describe(0));
    }
}
