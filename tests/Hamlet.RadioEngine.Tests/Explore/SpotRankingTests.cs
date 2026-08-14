using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// The workability ranking: what a newcomer can actually work, not what is
/// nearest (HM-DEC-058, HM-DEC-025).
/// </summary>
/// <remarks>
/// Distance does not run in a straight line with workability on HF. There is a
/// skip zone, so on 20 m a station two hundred miles off is often unreachable
/// while somebody two thousand miles out is easy. Sorting nearest-first would
/// put the hardest contacts at the top and call them the best chance.
/// </remarks>
public sealed class SpotRankingTests
{
    private static readonly DateTime Now = new(2026, 8, 15, 15, 0, 0, DateTimeKind.Utc);

    private static ActivitySpot Spot(
        string call,
        SpotCallType callType = SpotCallType.Cq,
        int? wpm = null,
        bool activation = false,
        SpotProximity proximity = SpotProximity.Unknown,
        int? signalDb = null,
        int? reports = null,
        int ageMinutes = 1,
        string mode = "CW",
        string source = "test")
        => new($"{call} is on the air", 7_032_000, mode, source,
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
        => SpotRanking.Evaluate(
            spot, SpotLensView.Liveness(spot, Now - spot.HeardAtUtc)).Score;

    /// <remarks>
    /// Proves a CQ outranks a contest exchange and a beacon. A CQ is an open
    /// invitation; a contest run is a closed loop at speed; a beacon answers
    /// nobody at all.
    /// </remarks>
    [Fact]
    public void CallingCqOutranksAContestExchangeAndABeacon()
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
    /// A FINISHED ACTIVATION NEVER OUTRANKS A LIVE STATION CALLING CQ
    /// (HM-DEC-058). Somebody who packed up an hour and a half ago is not a
    /// contact, however good the spot looked when it arrived, and liveness is
    /// weighted to say so.
    /// </remarks>
    [Fact]
    public void AFinishedActivationNeverOutranksALiveStationCallingCq()
    {
        // Past its hour, so the activator has most likely gone home.
        var finished = Spot("W3GONE", activation: true, wpm: 12, ageMinutes: 75);

        // A skimmer heard this one calling a minute ago.
        var live = Spot("K2HERE", wpm: 22, ageMinutes: 1, source: "RBN");

        Assert.True(
            ScoreOf(live) > ScoreOf(finished),
            $"live {ScoreOf(live)} must beat finished {ScoreOf(finished)}");

        // And a fresh activation still beats them both, which is the order a
        // person would put these in.
        var fresh = Spot("W3HERE", activation: true, wpm: 12, ageMinutes: 2);
        Assert.True(ScoreOf(fresh) > ScoreOf(live));
    }

    /// <remarks>
    /// AN RBN SPOT WITH NO DISTANCE IS NOT PENALIZED FOR LACKING ONE
    /// (HM-DEC-038, HM-DEC-058). RBN states where a RECEIVER is and so carries
    /// no station location at all, and a ranking that wanted one would bury
    /// every skimmer report, which is where "somebody is calling CQ right this
    /// second" comes from.
    /// </remarks>
    [Fact]
    public void ASkimmerSpotWithNoStationLocationIsNotPenalized()
    {
        var skimmer = Spot("K2XYZ", wpm: 20, ageMinutes: 1, source: "RBN");
        var located = Spot("W3ABC", wpm: 20, ageMinutes: 1, source: "RBN") with
        {
            StationLocation = new LatLon(40.4, -79.7),
        };

        Assert.Null(skimmer.StationLocation);
        Assert.Equal(ScoreOf(located), ScoreOf(skimmer));
    }

    /// <remarks>
    /// DISTANCE DOES NOT VOTE, which is the ruling this file exists for
    /// (HM-DEC-058). Two park activations alike in every way Hamlet can judge
    /// score the same, whether one is in the next state and the other is across
    /// an ocean. Hamlet cannot say which is workable without knowing what is
    /// open, and it will not pretend. The card still shows the distance, because
    /// that is what teaches which ranges are plausible on which band.
    /// </remarks>
    [Fact]
    public void TwoActivationsAlikeInEveryJudgeableWayScoreTheSame()
    {
        var near = Spot("W3NEAR", activation: true, wpm: 15,
            proximity: SpotProximity.Local, source: "POTA");
        var far = Spot("DL1FAR", activation: true, wpm: 15,
            proximity: SpotProximity.Distant, source: "POTA");

        Assert.Equal(ScoreOf(near), ScoreOf(far));
    }

    /// <remarks>
    /// Proves the one proximity that DOES vote, and why it is not a distance. A
    /// skimmer report says a machine that decoded the signal is standing in the
    /// operator's own corner of the country, which is the closest thing to "your
    /// receiver will hear it too" that a spot network can honestly offer.
    /// </remarks>
    [Fact]
    public void ANearbyReceiverCountsBecauseItIsEvidenceAboutHearingRatherThanDistance()
    {
        var nearReceiver = Spot("K2XYZ", proximity: SpotProximity.Local, source: "RBN");
        var farReceiver = Spot("K3XYZ", proximity: SpotProximity.Distant, source: "RBN");

        Assert.True(ScoreOf(nearReceiver) > ScoreOf(farReceiver));

        // The phrase says what the fact is, and does not claim to know where
        // the transmitter is.
        var reason = SpotRanking.Evaluate(nearReceiver, 1.0).Reason;
        Assert.Contains("receiver near you", reason, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves activations are weighted up. That operator carried a radio
    /// somewhere on purpose to be called and needs contacts, which makes them
    /// the friendliest contact a newcomer can find.
    /// </remarks>
    [Fact]
    public void ActivationsOutrankOrdinarySpots()
        => Assert.True(ScoreOf(Spot("A", activation: true)) > ScoreOf(Spot("B")));

    /// <remarks>Proves measured strength and agreement between receivers vote.</remarks>
    [Fact]
    public void StrongerAndMoreWidelyHeardOutrankMarginal()
    {
        Assert.True(ScoreOf(Spot("C", signalDb: 25)) > ScoreOf(Spot("D", signalDb: 2)));
        Assert.True(ScoreOf(Spot("E", reports: 12)) > ScoreOf(Spot("F", reports: 1)));
    }

    /// <remarks>
    /// Proves liveness moves a spot, and that it runs on the source's own
    /// lifetime rather than a flat clock (HM-DEC-045, HM-DEC-057).
    /// </remarks>
    [Fact]
    public void LivelierOutranksOlderOnTheSourcesOwnClock()
    {
        Assert.True(
            ScoreOf(Spot("A", ageMinutes: 1)) > ScoreOf(Spot("B", ageMinutes: 20)));

        // Half an hour into an hour is halfway for an activation and long spent
        // for a skimmer report, so the same age is not the same score. The
        // skimmer is deliberately not flagged as an activation, because the
        // lifetime rule reads intent before source: somebody who carried a radio
        // to a park stays put whoever reported them (HM-DEC-045).
        var activation = ScoreOf(Spot("C", activation: true, ageMinutes: 30, source: "POTA"));
        var skimmer = ScoreOf(Spot("D", ageMinutes: 30, source: "RBN"));

        Assert.True(activation > skimmer, $"activation {activation}, skimmer {skimmer}");
    }

    /// <remarks>
    /// SPEED DESCRIBES THE STATION AND NEVER MATCHES THE OPERATOR (HM-DEC-058,
    /// HM-OPEN-006). Hamlet has never asked what speed this person can copy, so
    /// no phrase may claim a speed suits them. It may say how fast the sending
    /// is, because that is a measurement the source reported.
    /// </remarks>
    [Fact]
    public void SpeedDescribesTheSendingAndNeverClaimsItSuitsTheOperator()
    {
        var relaxed = SpotRanking.Evaluate(Spot("A", wpm: 12), 1.0).Reason;
        var quick = SpotRanking.Evaluate(Spot("B", wpm: 24), 1.0).Reason;
        var fast = SpotRanking.Evaluate(Spot("C", wpm: 32), 1.0).Reason;

        Assert.Contains("12 WPM", relaxed, StringComparison.Ordinal);
        Assert.Contains("24 WPM", quick, StringComparison.Ordinal);
        Assert.Contains("32 WPM", fast, StringComparison.Ordinal);

        foreach (var line in new[] { relaxed, quick, fast })
        {
            foreach (var claim in new[]
                     {
                         "enough to copy", "you can copy", "suits you",
                         "your speed", "for you",
                     })
            {
                Assert.DoesNotContain(claim, line, StringComparison.OrdinalIgnoreCase);
            }
        }

        // A modest preference remains, and it is a fact about Morse rather than
        // about this person: slower sending is easier for anybody still
        // learning, which is why the slow-speed clubs exist.
        Assert.True(ScoreOf(Spot("A", wpm: 12)) > ScoreOf(Spot("C", wpm: 32)));
    }

    /// <remarks>
    /// Proves the whole ordering holds at once on a crafted set: the live park
    /// CQ leads, and the beacon comes last even though it is close, strong and
    /// slow. A beacon scores well on every axis except the only one that
    /// matters, whether anybody will answer, so its penalty has to outweigh all
    /// of them together.
    /// </remarks>
    [Fact]
    public void ACraftedSetComesOutInTheRightOrder()
    {
        var spots = new[]
        {
            Spot("BEACON", SpotCallType.Beacon, wpm: 15, signalDb: 25,
                proximity: SpotProximity.Local, source: "RBN"),
            Spot("CONTEST", SpotCallType.Contest, wpm: 32, source: "RBN",
                proximity: SpotProximity.Continent),
            Spot("PARK", SpotCallType.Cq, wpm: 12, activation: true,
                signalDb: 22, ageMinutes: 1, source: "POTA"),
            Spot("PLAIN", SpotCallType.Unknown, ageMinutes: 15),
            Spot("FASTCQ", SpotCallType.Cq, wpm: 28, source: "RBN",
                proximity: SpotProximity.Distant),
        };

        var ranked = SpotRanking.Rank(spots, Now);

        Assert.Equal("PARK", ranked[0].Spot.DxCall);
        Assert.Equal("BEACON", ranked[^1].Spot.DxCall);

        var beacon = ranked.Single(r => r.Spot.DxCall == "BEACON").Score;
        var plain = ranked.Single(r => r.Spot.DxCall == "PLAIN").Score;
        Assert.True(beacon < plain, $"beacon {beacon} must rank below plain {plain}");
    }

    /// <remarks>
    /// THE REASON LINE EXPLAINS THE TOP CARD'S POSITION (HM-DEC-058). The
    /// ordering has to be explainable from the screen, so the phrases printed
    /// are the components that actually moved the spot and the strongest of them
    /// are the ones that survive the trim.
    /// </remarks>
    [Fact]
    public void TheTopCardsReasonExplainsWhyItIsThere()
    {
        var spots = new[]
        {
            Spot("PARK", SpotCallType.Cq, wpm: 12, activation: true,
                ageMinutes: 2, source: "POTA"),
            Spot("PLAIN", SpotCallType.Unknown, ageMinutes: 15),
        };

        var top = SpotRanking.Rank(spots, Now)[0];

        Assert.Equal("PARK", top.Spot.DxCall);

        // The three things that put it there, on its face.
        Assert.Contains("park activation", top.Reason, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("calling CQ", top.Reason, StringComparison.Ordinal);
        Assert.Contains("12 WPM", top.Reason, StringComparison.Ordinal);

        // And whether that person is probably still there, which is the clause
        // three parts used to lose on exactly the cards that needed it.
        Assert.Contains("still there", top.Reason, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves the core promise of HM-DEC-025: every card carries a reason. A
    /// card ranked highly with nothing said about why is a guess presented as a
    /// decode, so even the emptiest spot has to account for itself.
    /// </remarks>
    [Fact]
    public void EveryCardCarriesAReason()
    {
        var spots = new[]
        {
            Spot("PARK", SpotCallType.Cq, wpm: 12, activation: true),
            Spot("BARE", SpotCallType.Unknown),
            new ActivitySpot("something", 7_030_000, "CW", "test", Now, null),
        };

        Assert.All(SpotRanking.Rank(spots, Now), r => Assert.False(
            string.IsNullOrWhiteSpace(r.Reason),
            $"{r.Spot.DxCall ?? r.Spot.Story} was ranked with no stated reason"));
    }

    /// <remarks>
    /// Proves the mode weighting, which exists because of what the live feeds
    /// actually did: FT8 park activations swamped the top of the list, and
    /// Hamlet cannot decode FT8 until phase 3, so the app was recommending that
    /// a beginner go and watch a waterfall.
    /// </remarks>
    [Fact]
    public void WorkableModesOutrankOnesHamletCannotDecode()
    {
        var cw = ScoreOf(Spot("A", activation: true, mode: "CW"));
        var ssb = ScoreOf(Spot("B", activation: true, mode: "SSB"));
        var ft8 = ScoreOf(Spot("C", activation: true, mode: "FT8"));

        Assert.True(cw > ssb, $"CW {cw} should beat SSB {ssb}");
        Assert.True(ssb > ft8, $"SSB {ssb} should beat FT8 {ft8}");

        Assert.Contains(
            "phase 3",
            SpotRanking.Evaluate(Spot("C", activation: true, mode: "FT8"), 1.0).Reason,
            StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// RANKING IS A PURE FUNCTION OVER THE SPOT SET AND THE LENS (HM-DEC-058,
    /// §5). No clock is read inside it, so the same spots at the same moment
    /// rank identically every call and a live feed's ordering can be reproduced
    /// from a fixture.
    /// </remarks>
    [Fact]
    public void RankingIsDeterministicAndReadsNoClock()
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

        // The same set an hour later ranks differently, which is what proves
        // the moment is an input rather than something read inside.
        var later = SpotRanking.Rank(spots, Now.AddHours(1));
        Assert.NotEqual(
            SpotRanking.Rank(spots, Now)[0].Score, later[0].Score);
    }

    /// <remarks>
    /// Proves the lens hands its own liveness to the rank rather than the rank
    /// measuring the clock a second time (HM-DEC-057). One clock is what keeps a
    /// card from looking faded and ranking fresh on the same screen.
    /// </remarks>
    [Fact]
    public void TheRankReadsTheLensesOwnLiveness()
    {
        var spot = Spot("W3ABC", activation: true, ageMinutes: 30, source: "POTA");
        var history = new[] { new StoredSpot(spot, Now.AddMinutes(-30), Now) };

        var lensed = SpotLensView.Apply(
            SpotLens.BestChance, history, SpotAttention.Fresh, Now);

        var viaLens = SpotRanking.Rank(lensed);
        var viaClock = SpotRanking.Rank(new[] { spot }, Now);

        Assert.Equal(viaClock[0].Score, viaLens[0].Score);
        Assert.Equal(viaClock[0].Reason, viaLens[0].Reason);
    }
}
