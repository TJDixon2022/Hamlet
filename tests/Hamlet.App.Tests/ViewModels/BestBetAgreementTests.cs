using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// The best-bet badge, the pips and the lead card all read one ranking, so
/// they cannot disagree about which band is best (HM-DEC-046).
/// </summary>
/// <remarks>
/// Same class of test as the banned-phrase sweep: the point is to make
/// disagreement impossible rather than unlikely, so these walk many
/// distributions rather than checking one.
/// </remarks>
public sealed class BestBetAgreementTests
{
    private static readonly DateTime Now = new(2026, 8, 14, 20, 0, 0, DateTimeKind.Utc);

    private static readonly string[] BandNames =
        HfBands.Bands.Select(b => b.Name).ToArray();

    private static ActivitySpot On(
        string band, int agoMinutes, bool activation, int seed)
    {
        var plan = HfBands.Bands.Single(b => b.Name == band);

        return new ActivitySpot(
            $"W{seed}ABC is on the air",
            plan.LowHz + 5_000 + (seed * 300),
            "CW",
            activation ? "POTA" : "RBN",
            Now.AddMinutes(-agoMinutes),
            15)
        {
            DxCall = $"W{seed}ABC",
            IsActivation = activation,
            CallType = SpotCallType.Cq,
        };
    }

    /// <summary>
    /// Deterministic pseudo-random spot distributions, so the sweep covers
    /// real shapes without depending on luck.
    /// </summary>
    private static IReadOnlyList<ActivitySpot> Distribution(int seed)
    {
        var rng = new Random(seed);
        var spots = new List<ActivitySpot>();
        var id = 0;

        foreach (var band in BandNames)
        {
            var count = rng.Next(0, 6);

            for (var i = 0; i < count; i++)
            {
                var activation = rng.Next(0, 2) == 0;

                // Ages either side of both lifetimes, so some spots are live
                // and some are not, and which ones differs by source.
                spots.Add(On(band, rng.Next(0, 70), activation, id++));
            }
        }

        return spots;
    }

    private static BandRanking Rank(IReadOnlyList<ActivitySpot> spots, int hour = 3)
        => BandOpportunities.Rank(HfBands.Bands, spots, Now, hour);

    /// <remarks>
    /// THE BADGE LANDS WHERE THE RANKING SAYS. Proves the badge is never on a
    /// band the ranking would not have chosen, across a hundred distributions.
    /// It used to come from a clock lookup table set at construction, which is
    /// how it ended up on a band with no pips.
    /// </remarks>
    [Fact]
    public void TheBadgeAlwaysLandsOnTheTopRankedBand()
    {
        for (var seed = 0; seed < 100; seed++)
        {
            var spots = Distribution(seed);
            var ranking = Rank(spots);

            Assert.NotNull(ranking.Best);

            // Nothing outranks it on the axes the ranking uses.
            foreach (var other in ranking.Bands.Skip(1))
            {
                Assert.True(
                    ranking.Best!.Count >= other.Count,
                    $"seed {seed}: {other.BandName} has {other.Count} against "
                    + $"{ranking.Best.BandName}'s {ranking.Best.Count}");
            }
        }
    }

    /// <remarks>
    /// Proves the badge lands on exactly one band, and that it is the one the
    /// ranking named. The ViewModel does nothing but copy this answer, so
    /// testing the answer tests the badge.
    /// </remarks>
    [Fact]
    public void TheBadgeGoesOnExactlyOneBand()
    {
        for (var seed = 0; seed < 100; seed++)
        {
            var ranking = Rank(Distribution(seed));

            var badged = BandNames.Where(ranking.BadgeGoesOn).ToList();

            Assert.Single(badged);
            Assert.Equal(ranking.BestBandName, badged[0]);
        }
    }

    /// <remarks>
    /// THE TEST THAT MAKES DISAGREEMENT IMPOSSIBLE. Proves the badge and the
    /// lead card never name different bands: whenever the card suggests an
    /// alternative, that band is the badge's band, unless the badge is sitting
    /// on the band the operator is already looking at.
    /// </remarks>
    [Fact]
    public void TheBadgeAndTheLeadCardNeverNameDifferentBands()
    {
        for (var seed = 0; seed < 100; seed++)
        {
            var spots = Distribution(seed);
            var ranking = Rank(spots);

            // Wherever the badge actually landed, from the same call the
            // ViewModel makes.
            var badgeBand = BandNames.Single(ranking.BadgeGoesOn);

            foreach (var current in BandNames)
            {
                var lead = LeadCard.Choose(
                    Array.Empty<RankedSpot>(), current, ranking: ranking);

                var named = BandNames.FirstOrDefault(
                    b => lead.Headline.Contains(b, StringComparison.Ordinal));

                if (named is null)
                {
                    // The card declined to name a band. That is allowed; what
                    // is not allowed is naming a different one.
                    continue;
                }

                // Never the band already on screen.
                Assert.NotEqual(current, named);

                // And it is the badge's band, unless the badge is on the band
                // the operator is already looking at, in which case the card
                // is offering the best alternative to it.
                if (!string.Equals(badgeBand, current, StringComparison.Ordinal))
                {
                    Assert.Equal(badgeBand, named);
                }
                else
                {
                    Assert.Equal(ranking.BestOtherThan(current)?.BandName, named);
                }
            }
        }
    }

    /// <remarks>
    /// Proves the badge is never on a band with nothing on it while another
    /// band has something. That is the exact screenshot that started this: the
    /// badge on 80 m with zero pips, 40 m with four.
    /// </remarks>
    [Fact]
    public void TheBadgeIsNeverOnAnEmptyBandWhileAnotherHasTraffic()
    {
        for (var seed = 0; seed < 100; seed++)
        {
            var ranking = Rank(Distribution(seed));

            if (!ranking.FromObservation)
            {
                continue;
            }

            Assert.True(
                ranking.Best!.Count > 0,
                $"seed {seed}: badge on {ranking.Best.BandName} with nothing on it");
        }
    }

    /// <remarks>
    /// Proves the badge and the pips agree, since both are counted from the
    /// same live spots. A badge on a band the pip scale draws as empty is the
    /// contradiction that was visible on screen.
    /// </remarks>
    [Fact]
    public void TheBadgeAgreesWithThePips()
    {
        for (var seed = 0; seed < 60; seed++)
        {
            var spots = Distribution(seed);
            var ranking = Rank(spots);

            if (!ranking.FromObservation)
            {
                continue;
            }

            var readings = BandActivity.Summarize(
                HfBands.Bands, spots, new[] { Ok("POTA") }, Now);

            var badged = readings.Single(r => r.BandName == ranking.BestBandName);

            Assert.True(
                badged.Pips > 0,
                $"seed {seed}: badge on {ranking.BestBandName} which draws {badged.Pips} pips");
        }
    }

    /// <remarks>
    /// THE CLOCK DROPS TO A TIEBREAKER. Proves it is used only when no band
    /// has anything at all, and that the ranking says so rather than passing a
    /// guess off as an observation.
    /// </remarks>
    [Fact]
    public void TheClockIsOnlyUsedWhenNothingHasBeenHeard()
    {
        var empty = Rank(Array.Empty<ActivitySpot>(), hour: 2);

        Assert.False(empty.FromObservation);
        Assert.Equal("80 m", empty.BestBandName);

        // One live spot anywhere is enough to stop guessing.
        var heard = Rank(new[] { On("15 m", 3, activation: true, seed: 1) }, hour: 2);

        Assert.True(heard.FromObservation);
        Assert.Equal("15 m", heard.BestBandName);
    }

    /// <remarks>
    /// A GUESS HAS TO ADMIT TO BEING ONE (§0.0). Proves the badge does not
    /// wear the same words for a clock guess as for an observation, and that
    /// the hover says what it rests on.
    /// </remarks>
    [Fact]
    public void AGuessNeverLooksLikeAnObservation()
    {
        var guess = Rank(Array.Empty<ActivitySpot>(), hour: 2);
        var observed = Rank(new[] { On("40 m", 3, activation: true, seed: 1) });

        Assert.NotEqual(observed.BadgeLabel, guess.BadgeLabel);

        Assert.Equal("best bet now", observed.BadgeLabel);
        Assert.DoesNotContain("best bet", guess.BadgeLabel, StringComparison.OrdinalIgnoreCase);

        // And it says what it is going on, in words.
        Assert.Contains("hour", guess.BadgeLabel, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("time of day", guess.BadgeTooltip, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Nothing has been heard", guess.BadgeTooltip, StringComparison.Ordinal);

        // The observed one carries its evidence instead.
        Assert.Contains("busiest band", observed.BadgeTooltip, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves the clock guess never sends somebody to an empty band through
    /// the lead card. The badge may stand on a hunch; the card, which is a
    /// direct instruction, may not.
    /// </remarks>
    [Fact]
    public void AClockGuessNeverBecomesALeadCardSuggestion()
    {
        var ranking = Rank(Array.Empty<ActivitySpot>(), hour: 2);

        foreach (var current in BandNames)
        {
            var lead = LeadCard.Choose(
                Array.Empty<RankedSpot>(), current, ranking: ranking);

            Assert.Equal(LeadCard.NothingHeadline, lead.Headline);
        }
    }

    /// <remarks>
    /// Proves activations break a tie ahead of raw count, because a park
    /// operator wanting contacts is worth more to a newcomer than the same
    /// number of bare skimmer reports.
    /// </remarks>
    [Fact]
    public void ActivationsBreakATie()
    {
        var spots = new[]
        {
            On("40 m", 3, activation: false, seed: 1),
            On("40 m", 4, activation: false, seed: 2),
            On("20 m", 3, activation: true, seed: 3),
            On("20 m", 4, activation: true, seed: 4),
        };

        Assert.Equal("20 m", Rank(spots).BestBandName);
    }

    /// <remarks>
    /// Proves the ranking is pure (§5): the same spots and hour always give
    /// the same order, with no clock read inside it.
    /// </remarks>
    [Fact]
    public void TheRankingIsDeterministic()
    {
        var spots = Distribution(7);

        Assert.Equal(
            Rank(spots).Bands.Select(b => b.BandName),
            Rank(spots).Bands.Select(b => b.BandName));
    }

    private static SourceStatus Ok(string name)
        => new(name, SourceState.Ok, 1, Now, null);
}
