using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// The lead card (HM-DEC-025), including the case that matters most: having
/// nothing to recommend and saying so.
/// </summary>
public sealed class LeadCardTests
{
    private static readonly DateTime Now = new(2026, 8, 13, 15, 0, 0, DateTimeKind.Utc);

    private static ActivitySpot Spot(
        string call,
        SpotCallType callType = SpotCallType.Cq,
        int? wpm = null,
        bool activation = false,
        SpotProximity proximity = SpotProximity.Unknown,
        int ageMinutes = 1,
        string mode = "CW",
        string? place = null)
        => new($"{call} is on the air", 7_032_000, mode, activation ? "POTA" : "RBN",
            Now.AddMinutes(-ageMinutes), wpm)
        {
            DxCall = call,
            CallType = callType,
            IsActivation = activation,
            Proximity = proximity,
            PlaceLabel = place,
        };

    private static IReadOnlyList<RankedSpot> Rank(params ActivitySpot[] spots)
        => SpotRanking.Rank(spots, Now);

    /// <remarks>
    /// Proves a good candidate is chosen and written up in plain language,
    /// with the frequency to tune to and the same evidence the ranked card
    /// carries.
    /// </remarks>
    [Fact]
    public void Chooses_TheBestCandidate()
    {
        var lead = LeadCard.Choose(
            Rank(
                Spot("K3ABC", wpm: 12, activation: true,
                    proximity: SpotProximity.Local, place: "US-PA"),
                Spot("W9FAST", wpm: 30, proximity: SpotProximity.Distant)),
            "40 m");

        Assert.True(lead.HasSuggestion);
        Assert.Equal(7_032_000, lead.TuneHz);
        Assert.Equal("7.032", lead.FrequencyLabel);
        Assert.Contains("12 WPM", lead.Headline, StringComparison.Ordinal);
        Assert.NotEmpty(lead.Reason);
    }

    /// <remarks>
    /// Proves the card says what to expect on arrival, which is the half a
    /// newcomer cannot get anywhere else. Somebody who has never answered a
    /// CQ does not know to listen through a couple of repeats first.
    /// </remarks>
    [Fact]
    public void Tells_TheOperatorWhatToExpect()
    {
        var lead = LeadCard.Choose(
            Rank(Spot("K3ABC", wpm: 12, activation: true, proximity: SpotProximity.Local)),
            "40 m");

        Assert.Contains("listen", lead.Body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("repeats", lead.Body, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves the refusal case. When nothing clears the bar the card says so
    /// and says what to do instead — inventing an encouraging suggestion out
    /// of a beacon and a contest run would be exactly the failure HM-DEC-009
    /// forbids, and it would cost this operator another wasted evening.
    /// </remarks>
    [Fact]
    public void Refuses_WhenNothingIsWorthRecommending()
    {
        var lead = LeadCard.Choose(
            Rank(
                Spot("K4JEE/B", SpotCallType.Beacon, wpm: 15,
                    proximity: SpotProximity.Local),
                Spot("W9FAST", SpotCallType.Contest, wpm: 34,
                    proximity: SpotProximity.Distant, ageMinutes: 30)),
            "40 m");

        Assert.False(lead.HasSuggestion);
        Assert.Equal(0, lead.TuneHz);
        Assert.Contains("2 spots", lead.Body, StringComparison.Ordinal);

        // It still says what to do instead. With no other band offered it no
        // longer says "try another band", because by then Hamlet has looked at
        // all of them and that advice would be a shrug (HM-DEC-045).
        Assert.Contains("different time of day", lead.Body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("training radio", lead.Body, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves an empty band is answered honestly rather than left blank, and
    /// that the refusal is framed as a useful result. "Nothing here, try
    /// there" is what saves an hour of tuning across a dead band.
    /// </remarks>
    [Fact]
    public void Refuses_OnAnEmptyBandAndSaysItIsAnAnswer()
    {
        var lead = LeadCard.Choose(Array.Empty<RankedSpot>(), "20 m");

        Assert.False(lead.HasSuggestion);
        Assert.Contains("20 m", lead.Body, StringComparison.Ordinal);

        // The framing is the point, not the exact wording: an empty band has to
        // read as a result the operator can act on rather than as the app
        // shrugging.
        Assert.Contains("a real answer", lead.Body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("rather than a failure", lead.Body, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves a beacon is never the lead however well it scores on every other
    /// axis. It transmits to nobody, so it can never be the answer to "who can
    /// I talk to".
    /// </remarks>
    [Fact]
    public void Never_LeadsWithABeacon()
    {
        var strongLocalBeacon = Spot(
            "K4JEE/B", SpotCallType.Beacon, wpm: 12, proximity: SpotProximity.Local)
            with
            { SignalDb = 30, ReportCount = 20 };

        var ranked = Rank(strongLocalBeacon);

        Assert.False(LeadCard.IsSuitable(ranked[0]));
        Assert.False(LeadCard.Choose(ranked, "40 m").HasSuggestion);
    }

    /// <remarks>
    /// Proves the no-sources case is worded differently from the empty-band
    /// case. A silent band and a broken feed produce identical spot counts,
    /// and telling the operator the band is empty when Hamlet simply cannot
    /// see it would be inventing calm (HM-DEC-025).
    /// </remarks>
    [Fact]
    public void Says_WhenItCannotSeeRatherThanBlamingTheBand()
    {
        var lead = LeadCard.Choose(
            Array.Empty<RankedSpot>(), "40 m", anySourceAnswering: false);

        Assert.False(lead.HasSuggestion);
        Assert.Contains("cannot see", lead.Headline, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "Hamlet's problem", lead.Body, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves the bar has both sides tested: a fresh nearby slow CQ clears it,
    /// and a bare unlabeled spot does not.
    /// </remarks>
    [Fact]
    public void Threshold_SeparatesARecommendationFromAShrug()
    {
        var good = Rank(Spot("K3ABC", wpm: 12, proximity: SpotProximity.Local))[0];
        var bare = Rank(Spot("W0XYZ", SpotCallType.Unknown, ageMinutes: 12))[0];

        Assert.True(LeadCard.IsSuitable(good));
        Assert.False(LeadCard.IsSuitable(bare));
    }

    /// <remarks>
    /// Proves an FT8 spot is described honestly: Hamlet cannot decode it until
    /// phase 3, so the card says it is a sign the band is open rather than a
    /// contact to go and make.
    /// </remarks>
    [Fact]
    public void Ft8_IsDescribedAsSomethingHamletCannotYetDo()
    {
        var lead = LeadCard.Choose(
            Rank(Spot("K3ABC", wpm: null, activation: true, mode: "FT8",
                proximity: SpotProximity.Local)),
            "40 m");

        Assert.True(lead.HasSuggestion);
        Assert.Contains("phase 3", lead.Body, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// ONE RANKING, READ BY BOTH (HM-DEC-046, HM-DEC-058). The lead card takes
    /// the first spot in the ranked list it is willing to recommend, so it can
    /// never point at one station while the list beneath it leads with another.
    /// Where the top of the list is suitable, the lead card IS the top of the
    /// list.
    /// </remarks>
    [Fact]
    public void TheLeadCardIsTheTopOfTheListWheneverTheTopIsWorthRecommending()
    {
        var now = new DateTime(2026, 8, 15, 15, 0, 0, DateTimeKind.Utc);

        var spots = new[]
        {
            // A fresh park activation calling CQ in Morse: the best thing here.
            new ActivitySpot("W3ABC is activating a park", 7_032_000, "CW", "POTA",
                now.AddMinutes(-8), 15)
            {
                DxCall = "W3ABC", CallType = SpotCallType.Cq,
                IsActivation = true, PlaceLabel = "US-PA",
            },

            // An activation an hour old, on voice: still on the list, well down.
            new ActivitySpot("DL1XYZ is activating a park", 7_179_000, "SSB", "POTA",
                now.AddMinutes(-58), null)
            {
                DxCall = "DL1XYZ", CallType = SpotCallType.Cq,
                IsActivation = true, PlaceLabel = "DE-BY",
            },
        };

        var ranked = SpotRanking.Rank(spots, now);
        var lead = LeadCard.Choose(ranked, "40 m");

        Assert.Equal("W3ABC", ranked[0].Spot.DxCall);
        Assert.True(lead.HasSuggestion);
        Assert.Equal(ranked[0].Spot.FrequencyHz, lead.TuneHz);

        // And the nearly spent one really is below it, which is the liveness
        // slope doing its job rather than a threshold.
        Assert.Equal("DL1XYZ", ranked[1].Spot.DxCall);
        Assert.True(ranked[1].Score < LeadCard.MinimumScore);
    }
}
