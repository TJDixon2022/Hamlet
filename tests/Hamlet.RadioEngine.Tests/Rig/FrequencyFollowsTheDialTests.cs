using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// The frequency on screen follows the dial, and Hamlet says so out loud.
/// </summary>
/// <remarks>
/// <para>**THE OPERATOR TURNED HIS DIAL AND WATCHED HAMLET FOLLOW THIRTY SECONDS
/// LATER**, repeatedly, on his own radio. The frequency was on session cadence
/// (HM-DEC-109) while the broadcast that was supposed to make it instant was not
/// arriving, and the screen repainted four times a second holding a value a
/// minute old. Nothing in the suite failed, because nothing in the suite asked
/// how old the number was.</para>
/// <para>These are the tests that would have caught it.</para>
/// </remarks>
public sealed class FrequencyFollowsTheDialTests
{
    private static readonly DateTime Now = new(2026, 8, 18, 21, 0, 0, DateTimeKind.Utc);

    /// <remarks>
    /// Proves the fix that cannot fail: the frequency is asked for at the live
    /// rate, so a radio that announces nothing is still followed four times a
    /// second rather than twice a minute.
    /// </remarks>
    [Fact]
    public void TheFrequencyIsAskedForAtLiveRate()
    {
        Assert.Equal(RigPollRate.Live, RigPollPlan.RateFor(RigField.Frequency));
        Assert.True(RigPollPlan.LiveInterval <= TimeSpan.FromMilliseconds(250));
    }

    /// <remarks>
    /// Proves the frequency's freshness window is now a live one, so a reading
    /// older than a second and a half is stale in the record and on screen rather
    /// than counting as current for two minutes.
    /// </remarks>
    [Fact]
    public void AFrequencyOlderThanASecondAndAHalfIsNotCurrent()
    {
        Assert.Equal(RigPollPlan.LiveFreshFor, RigPollPlan.FreshFor(RigField.Frequency));

        var value = RigValue.Known(
            RigField.Frequency, 7_030_000, "7.030", Now, "CI-V 03");

        Assert.False(value.IsStale(Now.AddSeconds(1), RigPollPlan.LiveFreshFor));
        Assert.True(value.IsStale(Now.AddSeconds(30), RigPollPlan.LiveFreshFor));
    }

    /// <remarks>
    /// Proves the radio announcing still wins: while its own pushes keep the
    /// value inside the window, Hamlet asks nothing and puts nothing on the bus,
    /// which is the behavior HM-DEC-050 wanted.
    /// </remarks>
    [Fact]
    public void AFreshBroadcastMeansHamletStaysQuiet()
    {
        var pushed = RigValue.Known(
            RigField.Frequency, 7_030_000, "7.030", Now, "transceive 00");

        Assert.True(RigPollPlan.SkipLiveRead(
            RigField.Frequency, pushed, Now.AddMilliseconds(250)));
    }

    /// <remarks>
    /// Proves silence is not a reason to wait: once the announcements stop for
    /// longer than the window, the poll takes over. This is the half that makes
    /// the fix hold on a radio with transceive switched off.
    /// </remarks>
    [Fact]
    public void SilenceForLongerThanTheWindowStartsAsking()
    {
        var pushed = RigValue.Known(
            RigField.Frequency, 7_030_000, "7.030", Now, "transceive 00");

        Assert.False(RigPollPlan.SkipLiveRead(
            RigField.Frequency, pushed, Now + RigPollPlan.BroadcastCoversFor));
    }

    /// <remarks>
    /// Proves a polled value never suppresses the next poll, whatever its age.
    /// Only the radio's own announcement earns Hamlet's silence.
    /// </remarks>
    [Fact]
    public void APolledValueNeverSuppressesTheNextRead()
    {
        var asked = RigValue.Known(
            RigField.Frequency, 7_030_000, "7.030", Now, "CI-V 03");

        Assert.False(RigPollPlan.SkipLiveRead(RigField.Frequency, asked, Now));
        Assert.False(RigPollPlan.SkipLiveRead(RigField.Frequency, null, Now));
    }

    /// <remarks>
    /// Proves nothing else was quietly given the same exemption: the rule is the
    /// frequency's, because it is the field with a push behind it and the field
    /// whose age was the failure.
    /// </remarks>
    [Fact]
    public void NoOtherFieldSkipsItsRead()
    {
        var pushed = RigValue.Known(RigField.Mode, 3, "CW", Now, "transceive 01");

        Assert.False(RigPollPlan.SkipLiveRead(RigField.Mode, pushed, Now));
        Assert.False(RigPollPlan.SkipLiveRead(RigField.SMeter, pushed, Now));
    }

    /// <remarks>
    /// **THE TEST THAT FAILS IF THIS EVER COMES BACK.** A frequency that has not
    /// been confirmed within a live interval, on a connected radio, is a defect
    /// and not a display detail: it is the state the app shipped in twice with
    /// nothing red.
    /// </remarks>
    [Fact]
    public void AFrequencyNotConfirmedWithinALiveIntervalIsAFailure()
    {
        var state = RigState.Empty.With(new[]
        {
            RigValue.Known(RigField.Frequency, 7_030_000, "7.030", Now, "CI-V 03"),
        });

        // Thirty seconds later, which is one session sweep and what the operator
        // actually sat through.
        var check = LinkSelfCheck.Describe(
            null, state, Now.AddSeconds(30), isConnected: true);

        Assert.False(
            check.TracksTheDial,
            "a frequency this old is not tracking the dial, and the app has to "
            + "know that about itself before it can say it");

        Assert.Contains("old", check.Headline, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves Hamlet can tell the operator which world he is in: a radio that
    /// announces, one that does not, and one nobody has heard from yet are three
    /// different sentences (§0.0).
    /// </remarks>
    [Fact]
    public void TheThreeWorldsGetThreeDifferentAnswers()
    {
        var announcing = RigState.Empty.With(new[]
        {
            RigValue.Known(
                RigField.Frequency, 7_030_000, "7.030", Now, "transceive 00"),
        });

        var quiet = RigState.Empty.With(new[]
        {
            RigValue.Known(RigField.CivTransceive, 0, "off", Now, "CI-V 1A 05 0071"),
            RigValue.Known(RigField.Frequency, 7_030_000, "7.030", Now, "CI-V 03"),
        });

        var announced = LinkSelfCheck.Describe(
            CivLinkHealth.Unknown with { Inbound = 40, InboundTransceive = 12 },
            announcing, Now, isConnected: true);

        var asked = LinkSelfCheck.Describe(
            CivLinkHealth.Unknown with { Inbound = 40 }, quiet, Now, isConnected: true);

        var nothing = LinkSelfCheck.Describe(
            null, RigState.Empty, Now, isConnected: true);

        Assert.True(announced.IsAnnouncing);
        Assert.False(asked.IsAnnouncing);
        Assert.Null(nothing.IsAnnouncing);

        Assert.NotEqual(announced.Headline, asked.Headline);
        Assert.NotEqual(asked.Headline, nothing.Headline);
        Assert.All(
            new[] { announced.Headline, asked.Headline, nothing.Headline },
            h => Assert.NotEmpty(h));
    }

    /// <remarks>
    /// Proves nothing is claimed with nothing connected: no radio, no sentence.
    /// </remarks>
    [Fact]
    public void NothingConnectedSaysNothing()
    {
        var check = LinkSelfCheck.Describe(
            null, RigState.Empty, Now, isConnected: false);

        Assert.Equal("", check.Headline);
        Assert.Equal("", check.Detail);
        Assert.Null(check.IsAnnouncing);
    }

    /// <remarks>
    /// Proves an arrived frame outranks a setting that was read: if transceive
    /// announcements are arriving, Hamlet says the radio announces whatever the
    /// setting last read as, because one is the thing and the other is a claim
    /// about it.
    /// </remarks>
    [Fact]
    public void AnArrivedFrameOutranksTheSetting()
    {
        var state = RigState.Empty.With(new[]
        {
            RigValue.Known(RigField.CivTransceive, 0, "off", Now, "CI-V 1A 05 0071"),
        });

        var check = LinkSelfCheck.Describe(
            CivLinkHealth.Unknown with { Inbound = 9, InboundTransceive = 3 },
            state, Now, isConnected: true);

        Assert.True(check.IsAnnouncing);
    }

    /// <remarks>
    /// Proves the detail carries the numbers the diagnostics screen needs,
    /// including what share of the cable the spectrum picture is taking.
    /// </remarks>
    [Fact]
    public void TheDetailCarriesTheCountsBehindTheSentence()
    {
        var link = CivLinkHealth.Unknown with
        {
            Inbound = 1000,
            InboundTransceive = 4,
            InboundScope = 900,
            InboundBytes = 550_000,
        };

        var state = RigState.Empty.With(new[]
        {
            RigValue.Known(
                RigField.Frequency, 7_030_000, "7.030", Now, "transceive 00"),
        });

        var detail = LinkSelfCheck.Describe(link, state, Now, isConnected: true).Detail;

        Assert.Contains("1000", detail, StringComparison.Ordinal);
        Assert.Contains("90%", detail, StringComparison.Ordinal);
        Assert.Contains("the radio announced it", detail, StringComparison.Ordinal);
    }
}
