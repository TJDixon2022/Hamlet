using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// No card says the same thing twice (HM-DEC-068).
/// </summary>
/// <remarks>
/// <para>THE CLASS OF BUG, NOT THE ONE THAT WAS FOUND. A park activation's card
/// ended its green line with "activators stay a while, so they are probably
/// still there" and then said it again in the gray line underneath, because the
/// ranking and the provenance line both ask the same function for the same
/// sentence. Fixing that one card would have left the next one to be found by
/// somebody reading the screen.</para>
/// <para>So this sweeps every card family over the shapes a spot can take, and
/// it fails on any repeated clause rather than on any particular sentence. It
/// matters beyond tidiness: a thing said twice reads as two pieces of evidence
/// when it is one, and that is a confidence the input does not justify
/// (§0.0).</para>
/// </remarks>
public sealed class CardRepetitionTests
{
    private static readonly DateTime Now = new(2026, 8, 14, 21, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Every shape a spot card comes in: the sources, the call types, the
    /// activation, the ages that change how a card talks about itself.
    /// </summary>
    private static IEnumerable<ActivitySpot> EveryKind()
    {
        foreach (var source in new[] { "POTA", "SOTA", "RBN", "Sample" })
        {
            foreach (var call in new[]
                     {
                         SpotCallType.Cq, SpotCallType.Dx, SpotCallType.Contest,
                         SpotCallType.Beacon, SpotCallType.Unknown,
                     })
            {
                foreach (var minutes in new[] { 0, 1, 5, 20, 45, 90, 240 })
                {
                    foreach (var activation in new[] { true, false })
                    {
                        foreach (var mode in new[] { "CW", "SSB", "FT8", "RTTY" })
                        {
                            yield return new ActivitySpot(
                                "W1AW calling CQ", 7_030_000, mode, source,
                                Now.AddMinutes(-minutes), mode == "CW" ? 15 : null)
                            {
                                CallType = call,
                                IsActivation = activation,
                                PlaceLabel = activation ? "US-PA" : "",
                                DxCall = "W1AW",
                                SignalDb = source == "RBN" ? 24 : null,
                                ReportCount = source == "RBN" ? 7 : null,
                                Proximity = source == "RBN"
                                    ? SpotProximity.Local
                                    : SpotProximity.Unknown,
                            };
                        }
                    }
                }
            }
        }
    }

    /// <remarks>
    /// Proves HM-DEC-068 for the happening-now cards, which is where the bug
    /// was found. The two lines are composed together, so the reason keeps
    /// everything and the line under it loses whatever was already read.
    /// </remarks>
    [Fact]
    public void NoSpotCardSaysAnythingTwice()
    {
        foreach (var spot in EveryKind())
        {
            var ranked = SpotRanking.Evaluate(spot, liveness: 0.5, null, 13);

            var card = new SpotViewModel(
                spot, Now, isNew: false, ranked.Reason, "412 miles northeast");

            var whole = card.Reason + CardText.Separator + card.Provenance;

            Assert.False(
                CardText.RepeatsItself(whole),
                $"this card says something twice: '{whole}'");
        }
    }

    /// <remarks>
    /// Proves HM-DEC-068 for the lead card, whose headline, body and evidence
    /// line are written by three pieces of code that cannot see one another and
    /// all reach for the same facts about the same station.
    /// </remarks>
    [Fact]
    public void TheLeadCardSaysNothingTwice()
    {
        foreach (var spot in EveryKind())
        {
            var ranked = new[] { SpotRanking.Evaluate(spot, liveness: 0.9, null, 13) };

            var lead = LeadCard.Choose(ranked, "40 m");

            var whole = lead.Headline + CardText.Separator + lead.Body
                + CardText.Separator + lead.Reason;

            Assert.False(
                CardText.RepeatsItself(whole),
                $"the lead card says something twice: '{whole}'");
        }
    }

    /// <remarks>
    /// Proves the sweep above can actually fail. A test that never fires is a
    /// test that proves nothing, and this one would pass on a card family that
    /// was silently returning empty strings.
    /// </remarks>
    [Fact]
    public void TheCheckCatchesARepeatWhenThereIsOne()
    {
        Assert.True(CardText.RepeatsItself(
            "an hour ago, and activators stay a while · CW · activators stay a while"));

        Assert.True(CardText.RepeatsItself(
            "they are probably still there · So they are probably still there."));

        Assert.False(CardText.RepeatsItself(
            "park activation in US-PA · calling CQ · 15 WPM · an hour ago"));
    }

    /// <remarks>
    /// Proves HM-DEC-068 keeps the reason line whole. It carries why the card is
    /// on screen at all, and thinning it out from something written underneath
    /// would be the tail wagging the dog (HM-DEC-025).
    /// </remarks>
    [Fact]
    public void TheReasonLineIsNeverThinnedByWhatIsUnderIt()
    {
        var lines = CardText.Compose(
            "park activation in US-PA · an hour ago, and activators stay a while",
            "CW · POTA · an hour ago, and activators stay a while · 412 miles");

        Assert.Equal(
            "park activation in US-PA · an hour ago, and activators stay a while",
            lines[0]);
        Assert.Equal("CW · POTA · 412 miles", lines[1]);
    }
}
