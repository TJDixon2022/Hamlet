using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// The empty case, which is the one that decides whether somebody keeps going
/// or goes to bed (HM-DEC-045).
/// </summary>
public sealed class EmptyBandTests
{
    private static readonly DateTime Now = new(2026, 8, 14, 20, 0, 0, DateTimeKind.Utc);

    private static ActivitySpot On(
        string band, int agoMinutes, bool activation = false, string call = "W1ABC")
    {
        var hz = band switch
        {
            "80 m" => 3_540_000L,
            "40 m" => 7_032_000L,
            "20 m" => 14_030_000L,
            _ => 21_030_000L,
        };

        return new ActivitySpot(
            $"{call} is on the air", hz, "CW", activation ? "POTA" : "RBN",
            Now.AddMinutes(-agoMinutes), 15)
        {
            DxCall = call,
            IsActivation = activation,
            CallType = SpotCallType.Cq,
        };
    }

    /// <summary>The shared ranking, at an hour the tiebreaker would not pick.</summary>
    private static BandRanking Elsewhere(params ActivitySpot[] spots)
        => BandOpportunities.Rank(BandPlan.Bands, spots, Now, localHour: 3);

    /// <remarks>
    /// THE BUG, IN ONE TEST. Proves an empty band points at a busy one instead
    /// of declaring nothing. This is the screenshot Tim sent: "Nothing here
    /// worth your next ten minutes" on a band where other bands were holding
    /// perfectly good invitations.
    /// </remarks>
    [Fact]
    public void AnEmptyBandReachesForABusyOne()
    {
        var elsewhere = Elsewhere(
            On("40 m", 5, activation: true, call: "W1AAA"),
            On("40 m", 8, activation: true, call: "W1BBB"),
            On("40 m", 12, call: "W1CCC"),
            On("20 m", 3, call: "W2AAA"));

        var lead = LeadCard.Choose(
            Array.Empty<RankedSpot>(), "80 m", ranking: elsewhere);

        Assert.False(lead.HasSuggestion);
        Assert.Contains("40 m", lead.Headline, StringComparison.Ordinal);
        Assert.Contains("three stations", lead.Body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("80 m", lead.Body, StringComparison.Ordinal);

        // And it never reaches the give-up sentence while holding those.
        Assert.NotEqual(LeadCard.NothingHeadline, lead.Headline);
    }

    /// <remarks>
    /// Proves the busiest band wins, so the suggestion is the best one
    /// available rather than the first one checked.
    /// </remarks>
    [Fact]
    public void TheBusiestBandIsTheOneOffered()
    {
        var elsewhere = Elsewhere(
            On("20 m", 4, call: "W2AAA"),
            On("40 m", 6, call: "W1AAA"),
            On("40 m", 7, call: "W1BBB"),
            On("40 m", 9, call: "W1CCC"));

        var lead = LeadCard.Choose(
            Array.Empty<RankedSpot>(), "80 m", ranking: elsewhere);

        Assert.Contains("40 m", lead.Headline, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the band on screen is never offered as the alternative to
    /// itself, which would read as the app malfunctioning.
    /// </remarks>
    [Fact]
    public void TheCurrentBandIsNeverOfferedAsTheAlternative()
    {
        // 40 m holds only spots too stale to recommend, but they are still
        // inside their lifetime and so still counted by the summary.
        var elsewhere = Elsewhere(On("40 m", 15, call: "W1AAA"));

        var lead = LeadCard.Choose(
            Array.Empty<RankedSpot>(), "40 m", ranking: elsewhere);

        Assert.Equal(LeadCard.NothingHeadline, lead.Headline);
    }

    /// <remarks>
    /// THE GIVE-UP SENTENCE IS REACHABLE ONLY AFTER AN ACTUAL SEARCH. Proves
    /// it appears when every band really is empty, and that it then says what
    /// was looked at and how far back (HM-DEC-025).
    /// </remarks>
    [Fact]
    public void OnlyAGenuinelyEmptySearchDeclaresNothing()
    {
        var lead = LeadCard.Choose(
            Array.Empty<RankedSpot>(), "40 m",
            ranking: Elsewhere(),
            lookedBack: TimeSpan.FromHours(3));

        Assert.Equal(LeadCard.NothingHeadline, lead.Headline);
        Assert.Contains("every band", lead.Body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("few hours", lead.Body, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves the old sentence is gone. It named ten minutes, which was never
    /// a considered figure, and it fired while the app held usable spots.
    /// </remarks>
    [Fact]
    public void NothingEverSaysTenMinutesAgain()
    {
        var lead = LeadCard.Choose(Array.Empty<RankedSpot>(), "40 m");

        Assert.DoesNotContain("ten minutes", lead.Headline, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ten minutes", lead.Body, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves a source outage still reads as Hamlet's problem rather than the
    /// ionosphere's, and is not confused with an empty band.
    /// </remarks>
    [Fact]
    public void ASourceOutageIsStillItsOwnAnswer()
    {
        var lead = LeadCard.Choose(
            Array.Empty<RankedSpot>(), "40 m",
            anySourceAnswering: false,
            ranking: Elsewhere(On("20 m", 4)));

        Assert.Contains("cannot see the bands", lead.Headline, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Hamlet's problem", lead.Body, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves each band is judged with its own spots' lifetimes, so a park
    /// activation counts for an hour and a skimmer report does not. Without
    /// this the counts would mean different things on different rows.
    /// </remarks>
    [Fact]
    public void EachBandIsJudgedByItsOwnSpotsLifetimes()
    {
        var summary = BandOpportunities.Summarize(
            BandPlan.Bands,
            new[]
            {
                On("40 m", 45, activation: true, call: "W1AAA"),
                On("20 m", 45, call: "W2AAA"),
            },
            Now);

        var forty = summary.Single(b => b.BandName == "40 m");
        var twenty = summary.Single(b => b.BandName == "20 m");

        // The park activator is still live at forty-five minutes.
        Assert.Equal(1, forty.Count);
        Assert.Equal(1, forty.Activations);

        // The skimmer report is not.
        Assert.Equal(0, twenty.Count);
    }

    /// <remarks>
    /// Proves a beacon never makes a band look busy, on any row. It transmits
    /// to nobody, so counting it would send somebody to a band with nothing to
    /// work on it.
    /// </remarks>
    [Fact]
    public void BeaconsNeverMakeABandLookBusy()
    {
        var beacon = On("20 m", 2) with { CallType = SpotCallType.Beacon };

        var summary = BandOpportunities.Summarize(BandPlan.Bands, new[] { beacon }, Now);

        Assert.Equal(0, summary.Single(b => b.BandName == "20 m").Count);
    }

    /// <remarks>
    /// Proves the summary counts in words and names activations separately,
    /// since "three of them park activators" is the part that tells a newcomer
    /// the band is worth their nerve (§0.7).
    /// </remarks>
    [Fact]
    public void TheSummaryCountsInWordsAndNamesActivations()
    {
        var summary = Elsewhere(
            On("40 m", 4, activation: true, call: "W1AAA"),
            On("40 m", 6, activation: true, call: "W1BBB"),
            On("40 m", 9, call: "W1CCC"));

        var text = summary.Bands.Single(b => b.BandName == "40 m").Describe();

        Assert.Contains("three stations", text, StringComparison.Ordinal);
        Assert.Contains("two of them park or summit activators", text, StringComparison.Ordinal);
        Assert.DoesNotContain("3", text, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the summary obeys the voice standard it was written under
    /// (HM-DEC-040).
    /// </remarks>
    [Fact]
    public void TheCopyObeysTheDashRule()
    {
        var passages = new List<string>
        {
            LeadCard.Choose(Array.Empty<RankedSpot>(), "40 m").Body,
            LeadCard.Choose(
                Array.Empty<RankedSpot>(), "80 m",
                ranking: Elsewhere(On("40 m", 5, activation: true))).Body,
            Elsewhere(On("40 m", 5)).Bands.Single(b => b.BandName == "40 m").Describe(),
        };

        Assert.All(passages, p => Assert.True(p.Count(c => c == '—') <= 1, p));
    }
}
