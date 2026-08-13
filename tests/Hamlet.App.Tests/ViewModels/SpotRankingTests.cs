using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// The ranking rule of HM-DEC-025, and the promise that every card says why
/// it is where it is.
/// </summary>
public sealed class SpotRankingTests
{
    private static readonly DateTime Now = new(2026, 8, 13, 15, 0, 0, DateTimeKind.Utc);

    private static ActivitySpot Spot(
        string call,
        SpotCallType callType = SpotCallType.Cq,
        int? wpm = null,
        bool activation = false,
        SpotProximity proximity = SpotProximity.Unknown,
        int? signalDb = null,
        int? reports = null,
        int ageMinutes = 1,
        string mode = "CW")
        => new($"{call} is on the air", 7_032_000, mode, "test",
            Now.AddMinutes(-ageMinutes), wpm)
        {
            DxCall = call,
            CallType = callType,
            IsActivation = activation,
            Proximity = proximity,
            SignalDb = signalDb,
            ReportCount = reports,
        };

    private static int ScoreOf(ActivitySpot spot)
        => SpotRanking.Evaluate(spot, Now - spot.HeardAtUtc).Score;

    /// <remarks>
    /// Proves a CQ outranks a contest exchange and a beacon. A CQ is an open
    /// invitation; a contest run is a closed loop at speed; a beacon answers
    /// nobody at all.
    /// </remarks>
    [Fact]
    public void Cq_OutranksContestAndBeacon()
    {
        var cq = ScoreOf(Spot("A", SpotCallType.Cq));
        var contest = ScoreOf(Spot("B", SpotCallType.Contest));
        var beacon = ScoreOf(Spot("C", SpotCallType.Beacon));
        var unknown = ScoreOf(Spot("D", SpotCallType.Unknown));

        Assert.True(cq > contest, $"CQ {cq} should beat contest {contest}");
        Assert.True(contest > beacon, $"contest {contest} should beat beacon {beacon}");
        Assert.True(cq > unknown, $"CQ {cq} should beat unlabeled {unknown}");
    }

    /// <remarks>
    /// Proves slower CW outranks faster. Under about 18 WPM is copyable by
    /// somebody still counting dits; over 24 is a wall.
    /// </remarks>
    [Fact]
    public void SlowerCw_Outranks_Faster()
    {
        var slow = ScoreOf(Spot("A", wpm: 12));
        var middling = ScoreOf(Spot("B", wpm: 17));
        var quick = ScoreOf(Spot("C", wpm: 22));
        var blistering = ScoreOf(Spot("D", wpm: 32));

        Assert.True(slow > middling);
        Assert.True(middling > quick);
        Assert.True(quick > blistering);
    }

    /// <remarks>
    /// Proves activations are weighted up. That operator carried a radio
    /// somewhere on purpose to be called and needs contacts, which makes them
    /// the friendliest contact a newcomer can find.
    /// </remarks>
    [Fact]
    public void Activations_OutrankOrdinarySpots()
    {
        var park = ScoreOf(Spot("A", activation: true));
        var plain = ScoreOf(Spot("B"));

        Assert.True(park > plain, $"activation {park} should beat plain {plain}");
    }

    /// <remarks>Proves closer and stronger outrank marginal.</remarks>
    [Fact]
    public void CloserAndStronger_Outrank_Marginal()
    {
        Assert.True(
            ScoreOf(Spot("A", proximity: SpotProximity.Local))
            > ScoreOf(Spot("B", proximity: SpotProximity.Distant)));

        Assert.True(
            ScoreOf(Spot("C", signalDb: 25)) > ScoreOf(Spot("D", signalDb: 2)));

        Assert.True(
            ScoreOf(Spot("E", reports: 12)) > ScoreOf(Spot("F", reports: 1)));
    }

    /// <remarks>Proves fresher outranks older.</remarks>
    [Fact]
    public void Fresher_Outranks_Older()
    {
        Assert.True(
            ScoreOf(Spot("A", ageMinutes: 1)) > ScoreOf(Spot("B", ageMinutes: 7)));
        Assert.True(
            ScoreOf(Spot("C", ageMinutes: 7)) > ScoreOf(Spot("D", ageMinutes: 40)));
    }

    /// <remarks>
    /// Proves the whole ordering holds at once on a crafted set: the slow
    /// nearby park CQ leads, and the beacon comes last even though it is
    /// close, strong and slow. A beacon scores well on every axis except the
    /// only one that matters — whether anybody will answer — so its penalty
    /// has to outweigh all of them together.
    /// </remarks>
    [Fact]
    public void Rank_OrdersACraftedSet()
    {
        var spots = new[]
        {
            Spot("BEACON", SpotCallType.Beacon, wpm: 15, signalDb: 25,
                proximity: SpotProximity.Local),
            Spot("CONTEST", SpotCallType.Contest, wpm: 32,
                proximity: SpotProximity.Continent),
            Spot("PARK", SpotCallType.Cq, wpm: 12, activation: true,
                proximity: SpotProximity.Local, signalDb: 22, ageMinutes: 1),
            Spot("PLAIN", SpotCallType.Unknown, ageMinutes: 15),
            Spot("FASTCQ", SpotCallType.Cq, wpm: 28, proximity: SpotProximity.Distant),
        };

        var ranked = SpotRanking.Rank(spots, Now);

        Assert.Equal("PARK", ranked[0].Spot.DxCall);
        Assert.Equal("BEACON", ranked[^1].Spot.DxCall);

        // Explicitly: the beacon sits below the bare unlabeled spot, which
        // at least might be a person.
        var beacon = ranked.Single(r => r.Spot.DxCall == "BEACON").Score;
        var plain = ranked.Single(r => r.Spot.DxCall == "PLAIN").Score;
        Assert.True(beacon < plain, $"beacon {beacon} must rank below plain {plain}");
    }

    /// <remarks>
    /// Proves the core promise of HM-DEC-025: every card carries a reason. A
    /// card ranked highly with nothing said about why is a guess presented as
    /// a decode (HM-DEC-009), so even the emptiest spot has to account for
    /// itself.
    /// </remarks>
    [Fact]
    public void EveryCard_CarriesAReason()
    {
        var spots = new[]
        {
            Spot("PARK", SpotCallType.Cq, wpm: 12, activation: true,
                proximity: SpotProximity.Local),
            Spot("BARE", SpotCallType.Unknown),
            new ActivitySpot("something", 7_030_000, "CW", "test", Now, null),
        };

        var ranked = SpotRanking.Rank(spots, Now);

        Assert.All(ranked, r => Assert.False(
            string.IsNullOrWhiteSpace(r.Reason),
            $"{r.Spot.DxCall ?? r.Spot.Story} was ranked with no stated reason"));
    }

    /// <remarks>
    /// Proves the reason reports what actually moved the spot up the list,
    /// rather than being written separately from the score and drifting away
    /// from it.
    /// </remarks>
    [Fact]
    public void Reason_NamesTheThingsThatScored()
    {
        var park = SpotRanking.Evaluate(
            Spot("K3ABC", SpotCallType.Cq, wpm: 12, activation: true,
                proximity: SpotProximity.Local),
            TimeSpan.FromMinutes(1));

        Assert.Contains("park activation", park.Reason, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("12 WPM", park.Reason, StringComparison.Ordinal);

        var beacon = SpotRanking.Evaluate(
            Spot("K4JEE/B", SpotCallType.Beacon), TimeSpan.FromMinutes(1));

        Assert.Contains("beacon", beacon.Reason, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves the mode weighting, which exists because of what the live feeds
    /// actually did: FT8 park activations swamped the top of the list, and
    /// Hamlet cannot decode FT8 until phase 3, so the app was recommending
    /// that a beginner go and watch a waterfall. CW is what Hamlet is for and
    /// is lifted; FT8 is pushed below workable modes and says why on the card.
    /// </remarks>
    [Fact]
    public void WorkableModes_OutrankOnesHamletCannotDecode()
    {
        var cw = ScoreOf(Spot("A", activation: true, mode: "CW"));
        var ssb = ScoreOf(Spot("B", activation: true, mode: "SSB"));
        var ft8 = ScoreOf(Spot("C", activation: true, mode: "FT8"));

        Assert.True(cw > ssb, $"CW {cw} should beat SSB {ssb}");
        Assert.True(ssb > ft8, $"SSB {ssb} should beat FT8 {ft8}");

        var reason = SpotRanking
            .Evaluate(Spot("C", activation: true, mode: "FT8"), TimeSpan.FromMinutes(1))
            .Reason;

        Assert.Contains("phase 3", reason, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves a distant activation cannot outrank a local one, which is the
    /// other thing the live feeds exposed: POTA carries European park
    /// activations that score beautifully on every axis except being audible
    /// from Pennsylvania on 40 m in daylight.
    /// </remarks>
    [Fact]
    public void DistantActivations_RankBelowLocalOnes()
    {
        var local = ScoreOf(Spot("A", activation: true, wpm: 17,
            proximity: SpotProximity.Local));
        var distant = ScoreOf(Spot("B", activation: true, wpm: 17,
            proximity: SpotProximity.Distant));

        Assert.True(local > distant, $"local {local} should beat distant {distant}");
    }

    /// <remarks>
    /// Proves determinism (§5): the same spots and the same elapsed time rank
    /// identically every call, because no clock is read inside the ranking.
    /// </remarks>
    [Fact]
    public void Ranking_IsDeterministic()
    {
        var spots = new[]
        {
            Spot("A", wpm: 12, activation: true),
            Spot("B", wpm: 25),
            Spot("C", SpotCallType.Beacon),
        };

        var first = SpotRanking.Rank(spots, Now).Select(r => r.Spot.DxCall).ToArray();
        var second = SpotRanking.Rank(spots, Now).Select(r => r.Spot.DxCall).ToArray();

        Assert.Equal(first, second);
    }
}
