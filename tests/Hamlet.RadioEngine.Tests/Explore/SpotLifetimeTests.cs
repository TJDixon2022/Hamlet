using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// Per-source lifetimes and the voice of age (HM-DEC-045).
/// </summary>
public sealed class SpotLifetimeTests
{
    private static readonly DateTime Now = new(2026, 8, 14, 20, 0, 0, DateTimeKind.Utc);

    private static ActivitySpot Park(int agoMinutes)
        => new("Somebody is activating a park", 7_032_000, "CW", "POTA",
            Now.AddMinutes(-agoMinutes), 15)
        {
            DxCall = "W1ABC",
            IsActivation = true,
            CallType = SpotCallType.Cq,
        };

    private static ActivitySpot Skimmer(int agoMinutes)
        => new("W1ABC is calling CQ", 7_032_000, "CW", "RBN",
            Now.AddMinutes(-agoMinutes), 15)
        {
            DxCall = "W1ABC",
            CallType = SpotCallType.Cq,
        };

    private static ActivitySpot Contest(int agoMinutes)
        => new("W1ABC is working a contest run", 7_032_000, "CW", "RBN",
            Now.AddMinutes(-agoMinutes), 32)
        {
            DxCall = "W1ABC",
            CallType = SpotCallType.Contest,
        };

    /// <remarks>
    /// THE CENTRAL CLAIM. Proves the three kinds age at different rates,
    /// because an activator who hauled gear to a park stays put and a skimmer
    /// only ever saw one moment.
    /// </remarks>
    [Fact]
    public void EachKindOfSpotHasItsOwnLifetime()
    {
        Assert.True(SpotLifetime.For(Park(0)) > SpotLifetime.For(Skimmer(0)));
        Assert.True(SpotLifetime.For(Contest(0)) > SpotLifetime.For(Park(0)));

        Assert.Equal(SpotLifetime.ActivationDefault, SpotLifetime.For(Park(0)));
        Assert.Equal(SpotLifetime.SkimmerDefault, SpotLifetime.For(Skimmer(0)));
        Assert.Equal(SpotLifetime.ContestDefault, SpotLifetime.For(Contest(0)));
    }

    /// <remarks>
    /// THE BUG THIS FIXES. Proves a twenty-minute-old park activation is still
    /// live while a skimmer report of the same age is not. Under the old flat
    /// ten-minute window both were thrown away, which is how the app came to
    /// say "nothing here" while holding perfectly good invitations.
    /// </remarks>
    [Fact]
    public void ATwentyMinuteParkSpotIsLiveAndASkimmerReportIsNot()
    {
        Assert.True(SpotLifetime.IsLive(Park(20), Now));
        Assert.False(SpotLifetime.IsLive(Skimmer(25), Now));

        // And the old flat window would have discarded both.
        Assert.True(Now - Park(20).HeardAtUtc > TimeSpan.FromMinutes(10));
    }

    /// <remarks>
    /// POTA AND RBN OF IDENTICAL AGE PRODUCE DIFFERENT WORDING. Proves the
    /// likelihood language tracks the source rather than a flat rule: the park
    /// spot may say somebody is probably still there, and the skimmer report
    /// may not.
    /// </remarks>
    [Fact]
    public void IdenticalAgesReadDifferentlyBySource()
    {
        var park = SpotLifetime.DescribeOpportunity(Park(20), TimeSpan.FromMinutes(20));
        var skimmer = SpotLifetime.DescribeOpportunity(Skimmer(20), TimeSpan.FromMinutes(20));

        Assert.NotEqual(park, skimmer);

        Assert.Contains("still there", park, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("still there", skimmer, StringComparison.OrdinalIgnoreCase);

        // The skimmer phrasing says what it actually saw and no more.
        Assert.Contains("skimmer", skimmer, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// THE HONESTY CONSTRAINT, SWEPT. Proves no skimmer report at any age ever
    /// claims somebody is still on frequency. What a skimmer saw is a fact;
    /// whether they are still calling is not, and no phrasing may pretend
    /// otherwise (§0.0).
    /// </remarks>
    [Fact]
    public void NoSkimmerReportEverClaimsSomebodyIsStillThere()
    {
        string[] banned =
        {
            "still there", "still working", "still calling", "probably still",
            "very likely still", "stays put", "will be there",
        };

        for (var minutes = 0; minutes <= 240; minutes++)
        {
            var text = SpotLifetime.DescribeOpportunity(
                Skimmer(minutes), TimeSpan.FromMinutes(minutes));

            foreach (var phrase in banned)
            {
                Assert.DoesNotContain(phrase, text, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    /// <remarks>
    /// Proves an activation only claims somebody is still there while that is
    /// defensible, and stops well before its lifetime runs out. Past it the
    /// wording turns to packing up rather than staying optimistic.
    /// </remarks>
    [Fact]
    public void AnActivationStopsClaimingItOnceItIsOld()
    {
        Assert.Contains(
            "still there",
            SpotLifetime.DescribeOpportunity(Park(10), TimeSpan.FromMinutes(10)),
            StringComparison.OrdinalIgnoreCase);

        var old = SpotLifetime.DescribeOpportunity(Park(90), TimeSpan.FromMinutes(90));

        Assert.DoesNotContain("still there", old, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("finished", old, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves a beacon is never dressed as an opportunity however fresh it is.
    /// It transmits to nobody, so "it will still be there" is a reason not to
    /// bother rather than encouragement.
    /// </remarks>
    [Fact]
    public void ABeaconIsNeverAnInvitation()
    {
        var beacon = new ActivitySpot(
            "A beacon", 7_032_000, "CW", "RBN", Now.AddMinutes(-5), null)
        {
            CallType = SpotCallType.Beacon,
        };

        var text = SpotLifetime.DescribeOpportunity(beacon, TimeSpan.FromMinutes(5));

        Assert.Contains("nobody is listening", text, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// AGE IS SPOKEN, NOT COUNTED (§0.7). Proves the card never carries a
    /// minute count, since nobody says "17 min ago" out loud. The exact figure
    /// stays available on hover.
    /// </remarks>
    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    [InlineData(17)]
    [InlineData(38)]
    [InlineData(95)]
    [InlineData(400)]
    public void AgeIsSpokenRatherThanCounted(int minutes)
    {
        var text = SpotLifetime.DescribeAge(TimeSpan.FromMinutes(minutes));

        Assert.False(string.IsNullOrWhiteSpace(text));
        Assert.DoesNotContain(text, char.IsDigit);
    }

    /// <remarks>
    /// Proves the phrasing never reads as brand new once it is not. Presenting
    /// an old spot as if it had just arrived is the failure this whole ruling
    /// exists to stop.
    /// </remarks>
    [Fact]
    public void AnOldSpotNeverReadsAsIfItJustArrived()
    {
        for (var minutes = 3; minutes <= 240; minutes++)
        {
            var text = SpotLifetime.DescribeAge(TimeSpan.FromMinutes(minutes));
            Assert.DoesNotContain("just now", text, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <remarks>
    /// Proves the lifetimes are settable, since the right answer depends on
    /// how somebody operates and ten minutes was never a considered figure.
    /// </remarks>
    [Fact]
    public void LifetimesAreSettable()
    {
        var tight = SpotLifetimeSettings.FromMinutes(15, 5, 30);

        Assert.Equal(TimeSpan.FromMinutes(15), SpotLifetime.For(Park(0), tight));
        Assert.Equal(TimeSpan.FromMinutes(5), SpotLifetime.For(Skimmer(0), tight));
        Assert.False(SpotLifetime.IsLive(Park(20), Now, tight));
    }

    /// <remarks>
    /// Proves an absurd setting is refused rather than obeyed. A zero would
    /// empty the panel permanently and look exactly like a broken feed, which
    /// is a worse failure than ignoring a bad number.
    /// </remarks>
    [Theory]
    [InlineData(0)]
    [InlineData(-30)]
    [InlineData(100000)]
    public void AnAbsurdLifetimeFallsBackToTheDefault(int minutes)
    {
        var settings = SpotLifetimeSettings.FromMinutes(minutes, minutes, minutes);

        Assert.Equal(SpotLifetime.ActivationDefault, settings.Activation);
        Assert.Equal(SpotLifetime.SkimmerDefault, settings.Skimmer);
        Assert.Equal(SpotLifetime.ContestDefault, settings.Contest);
    }

    /// <remarks>
    /// Proves the longest lifetime is reported correctly, since that is what
    /// bounds how far back the display has to look.
    /// </remarks>
    [Fact]
    public void TheLongestLifetimeBoundsTheLookback()
    {
        Assert.Equal(SpotLifetime.ContestDefault, SpotLifetimeSettings.Defaults.Longest);
        Assert.Equal(
            TimeSpan.FromMinutes(90),
            SpotLifetimeSettings.FromMinutes(90, 5, 30).Longest);
    }

    /// <remarks>
    /// Proves contest activity is only claimed where the source said so.
    /// Guessing "this is a contest" from a busy band would be exactly the kind
    /// of inference the prime directive forbids.
    /// </remarks>
    [Fact]
    public void ContestIsOnlyClaimedWhereTheSourceSaidSo()
    {
        var busy = Skimmer(5) with { CallType = SpotCallType.Cq };

        Assert.Equal(SpotLifetime.SkimmerDefault, SpotLifetime.For(busy));
        Assert.Equal(SpotLifetime.ContestDefault, SpotLifetime.For(Contest(5)));
    }

    /// <remarks>
    /// Proves it is pure (§5): the same spot and elapsed time always give the
    /// same words, with no clock read anywhere.
    /// </remarks>
    [Fact]
    public void DescriptionsAreDeterministic()
        => Assert.Equal(
            SpotLifetime.DescribeOpportunity(Park(20), TimeSpan.FromMinutes(20)),
            SpotLifetime.DescribeOpportunity(Park(20), TimeSpan.FromMinutes(20)));
}
