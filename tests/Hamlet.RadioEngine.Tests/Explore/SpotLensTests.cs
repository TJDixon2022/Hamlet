using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// The happening-now panel's two lenses (HM-DEC-057).
/// </summary>
/// <remarks>
/// TWO QUESTIONS, AND THEY ARE NOT THE SAME ONE. "Best chance" is the arrival
/// question and ranks over everything alive; "what's new" is the
/// between-contacts question and is a delta since the operator last looked. A
/// refresh button answers neither, because it conflates "show me the good ones"
/// with "show me the fresh ones".
/// </remarks>
public sealed class SpotLensTests
{
    private static readonly DateTime Now = new(2026, 8, 15, 18, 0, 0, DateTimeKind.Utc);

    private static ActivitySpot Activation(string call, int minutesAgo)
        => new($"{call} is activating a park", 7_032_000, "CW", "POTA",
            Now.AddMinutes(-minutesAgo), 15)
        {
            IsActivation = true,
            CallType = SpotCallType.Cq,
            DxCall = call,
            PlaceLabel = "US-PA",
        };

    private static ActivitySpot Skimmer(string call, int minutesAgo, long hz = 7_040_000)
        => new($"{call} called CQ", hz, "CW", "RBN", Now.AddMinutes(-minutesAgo), 22)
        {
            CallType = SpotCallType.Cq,
            DxCall = call,
        };

    private static StoredSpot Stored(
        ActivitySpot spot, int firstSeenMinutesAgo, DateTime? actedOn = null)
        => new(spot, Now.AddMinutes(-firstSeenMinutesAgo),
            Now.AddMinutes(-firstSeenMinutesAgo), actedOn);

    private static SpotAttention LookedAgo(int minutes, params string[] actedOn)
        => new(
            Now.AddMinutes(-minutes),
            new HashSet<string>(actedOn, StringComparer.OrdinalIgnoreCase));

    /// <remarks>
    /// SWITCHING LENSES CHANGES THE ORDERING AND DELETES NOTHING (HM-DEC-057).
    /// This is a view over the store, which is what HM-DEC-045 built the store
    /// for. The history handed in comes back untouched, and the wider lens still
    /// shows everything the narrower one hid.
    /// </remarks>
    [Fact]
    public void SwitchingLensesChangesWhatIsShownAndNothingElse()
    {
        var history = new[]
        {
            Stored(Activation("W3ABC", minutesAgo: 40), firstSeenMinutesAgo: 40),
            Stored(Skimmer("K2XYZ", minutesAgo: 3), firstSeenMinutesAgo: 3),
        };

        var attention = LookedAgo(10);

        var best = SpotLensView.Apply(SpotLens.BestChance, history, attention, Now);
        var fresh = SpotLensView.Apply(SpotLens.WhatsNew, history, attention, Now);

        // Everything alive under one lens; only the arrival under the other.
        Assert.Equal(2, best.Count);
        Assert.Single(fresh);
        Assert.Equal("K2XYZ", fresh[0].Spot.DxCall);

        // And the history itself is exactly as it was handed in.
        Assert.Equal(2, history.Length);
        Assert.Equal("W3ABC", history[0].Spot.DxCall);
        Assert.Null(history[0].ActedOnUtc);
    }

    /// <remarks>
    /// A SOLID OLD PARK ACTIVATION IS STILL A FINE CONTACT. Somebody is standing
    /// in that park and an hour is what an activation is (HM-DEC-045), so "best
    /// chance" keeps it while "what's new" does not. The two lenses are allowed
    /// to disagree; that is what makes them two lenses.
    /// </remarks>
    [Fact]
    public void AnOldActivationSurvivesBestChanceAndIsNotNew()
    {
        var history = new[] { Stored(Activation("W3ABC", 40), firstSeenMinutesAgo: 40) };
        var attention = LookedAgo(10);

        Assert.Single(SpotLensView.Apply(SpotLens.BestChance, history, attention, Now));
        Assert.Empty(SpotLensView.Apply(SpotLens.WhatsNew, history, attention, Now));
    }

    /// <remarks>
    /// WHAT'S NEW DOES NOT RE-OFFER WHAT THEY HAVE ALREADY ACTED ON. Somebody
    /// who tuned to a station and came back a minute later does not need telling
    /// about it again, and the card is still there under the other lens because
    /// it is still a live station they may want to go back to.
    /// </remarks>
    [Fact]
    public void WhatsNewExcludesWhatTheOperatorAlreadyWorked()
    {
        var worked = Skimmer("K2XYZ", minutesAgo: 2);
        var other = Skimmer("N1QRP", minutesAgo: 2, hz: 7_055_000);

        var history = new[]
        {
            Stored(worked, firstSeenMinutesAgo: 2),
            Stored(other, firstSeenMinutesAgo: 2),
        };

        var attention = LookedAgo(10, SpotIdentity.KeyFor(worked));

        var fresh = SpotLensView.Apply(SpotLens.WhatsNew, history, attention, Now);

        Assert.Single(fresh);
        Assert.Equal("N1QRP", fresh[0].Spot.DxCall);

        // Still on the other lens: acting on it removes it from the delta and
        // from nothing else.
        Assert.Equal(2, SpotLensView.Apply(SpotLens.BestChance, history, attention, Now).Count);
    }

    /// <remarks>
    /// Proves the store's own mark counts as well as this session's set. The
    /// durable one survives a restart, so somebody who worked a station last
    /// night is not offered it again this morning while it is still live.
    /// </remarks>
    [Fact]
    public void AVisitRecordedOnDiskCountsTheSameAsOneInThisSession()
    {
        var spot = Skimmer("K2XYZ", minutesAgo: 2);
        var history = new[] { Stored(spot, firstSeenMinutesAgo: 2, actedOn: Now.AddMinutes(-1)) };

        Assert.Empty(SpotLensView.Apply(
            SpotLens.WhatsNew, history, SpotAttention.Fresh, Now));
    }

    /// <remarks>
    /// Proves an operator who has never looked sees everything as new. A delta
    /// against nothing is everything, which is the honest answer rather than an
    /// empty panel on a first run.
    /// </remarks>
    [Fact]
    public void SomebodyWhoHasNeverLookedSeesEverythingAsNew()
    {
        var history = new[]
        {
            Stored(Activation("W3ABC", 40), firstSeenMinutesAgo: 40),
            Stored(Skimmer("K2XYZ", 3), firstSeenMinutesAgo: 3),
        };

        Assert.Equal(
            2,
            SpotLensView.Apply(SpotLens.WhatsNew, history, SpotAttention.Fresh, Now).Count);
    }

    /// <remarks>
    /// FIRST SEEN, NEVER LAST SEEN. A station spotted again twenty minutes later
    /// did not start calling twenty minutes later, and treating a re-sighting as
    /// an arrival is the "presented as if it just arrived" failure HM-DEC-045
    /// forbids, arriving by a different door.
    /// </remarks>
    [Fact]
    public void ARepeatedSightingIsNotAnArrival()
    {
        var spot = Activation("W3ABC", minutesAgo: 30);

        // First seen half an hour ago, reported again a moment ago.
        var history = new[]
        {
            new StoredSpot(spot, Now.AddMinutes(-30), Now, null),
        };

        Assert.Empty(SpotLensView.Apply(
            SpotLens.WhatsNew, history, LookedAgo(10), Now));
    }

    /// <remarks>
    /// Neither lens shows a spot past its source's ruled lifetime, and the
    /// lifetime is the source's own: an hour-old park activation is still live
    /// and an hour-old skimmer report is long gone (HM-DEC-045).
    /// </remarks>
    [Fact]
    public void BothLensesRespectEachSourcesOwnLifetime()
    {
        var history = new[]
        {
            Stored(Activation("W3ABC", minutesAgo: 50), firstSeenMinutesAgo: 1),
            Stored(Skimmer("K2XYZ", minutesAgo: 50), firstSeenMinutesAgo: 1),
        };

        var shown = SpotLensView.Apply(SpotLens.BestChance, history, SpotAttention.Fresh, Now);

        Assert.Single(shown);
        Assert.Equal("W3ABC", shown[0].Spot.DxCall);
    }

    /// <remarks>
    /// AGE FADES THE DISPLAY, and never to nothing. A card faded away is a card
    /// removed, and removing one is what this whole design exists not to do. The
    /// fade is stronger under "what's new", where it is the point, than under
    /// "best chance", where liveness is one input to the rank rather than the
    /// whole answer.
    /// </remarks>
    [Fact]
    public void TheFadeIsStrongerUnderWhatsNewAndNeverReachesNothing()
    {
        var spot = Skimmer("K2XYZ", minutesAgo: 19);
        var age = TimeSpan.FromMinutes(19);
        var liveness = SpotLensView.Liveness(spot, age);

        Assert.InRange(liveness, 0.0, 0.2);

        var sharp = SpotLensView.Prominence(SpotLens.WhatsNew, liveness);
        var soft = SpotLensView.Prominence(SpotLens.BestChance, liveness);

        Assert.True(sharp < soft, $"what's new faded to {sharp}, best chance to {soft}");
        Assert.True(sharp >= SpotLensView.FadeFloor, $"faded to {sharp}");

        // A spot heard this second is drawn in full under either lens.
        Assert.Equal(1.0, SpotLensView.Prominence(SpotLens.WhatsNew, 1.0), 3);
        Assert.Equal(1.0, SpotLensView.Prominence(SpotLens.BestChance, 1.0), 3);
    }

    /// <remarks>
    /// Proves liveness runs across the source's own lifetime rather than a flat
    /// clock. This is the one measure the display fade and the workability rank
    /// both read, so a spot can never look faded and rank fresh on the same
    /// screen (HM-DEC-058).
    /// </remarks>
    [Fact]
    public void LivenessRunsAcrossTheSourcesOwnLifetime()
    {
        var age = TimeSpan.FromMinutes(30);

        var activation = SpotLensView.Liveness(Activation("W3ABC", 30), age);
        var skimmer = SpotLensView.Liveness(Skimmer("K2XYZ", 30), age);

        // Half an hour into an hour is halfway; half an hour into twenty
        // minutes is spent.
        Assert.Equal(0.5, activation, 2);
        Assert.Equal(0.0, skimmer, 2);
    }

    /// <remarks>
    /// THE COLLAPSED SUMMARY NAMES THE ACTIVE LENS (§0.5). A shut panel that has
    /// silently changed which question it is answering is the prime directive
    /// broken by omission: the operator reads a count and takes it for a count
    /// of everything.
    /// </remarks>
    [Fact]
    public void TheCollapsedSummaryNamesTheLens()
    {
        Assert.StartsWith("Best chance", SpotLensView.Summary(SpotLens.BestChance, 7),
            StringComparison.Ordinal);
        Assert.StartsWith("What's new", SpotLensView.Summary(SpotLens.WhatsNew, 2),
            StringComparison.Ordinal);

        Assert.Contains("7 spots", SpotLensView.Summary(SpotLens.BestChance, 7),
            StringComparison.Ordinal);
        Assert.Contains("1 spot", SpotLensView.Summary(SpotLens.BestChance, 1),
            StringComparison.Ordinal);

        // An empty list says which kind of empty it is, because "nothing new"
        // and "nothing at all" are different facts.
        Assert.Contains("nothing new", SpotLensView.Summary(SpotLens.WhatsNew, 0),
            StringComparison.Ordinal);
        Assert.Contains("nothing on this band",
            SpotLensView.Summary(SpotLens.BestChance, 0), StringComparison.Ordinal);
    }

    /// <remarks>
    /// INFERENCE GUESSES FROM REAL STATE OR NOT AT ALL (HM-DEC-057). It opens on
    /// "what's new" only when the operator was here within the last twenty
    /// minutes, so this is a return rather than an arrival, and something has
    /// actually turned up since. Absent either, it opens on "best chance" and
    /// leaves it.
    /// </remarks>
    [Theory]
    [InlineData(null, 5, SpotLens.BestChance)]      // never looked: an arrival
    [InlineData(5, 0, SpotLens.BestChance)]         // back soon, nothing new
    [InlineData(5, 3, SpotLens.WhatsNew)]           // back soon, three arrivals
    [InlineData(90, 8, SpotLens.BestChance)]        // gone an hour and a half
    public void TheOpeningLensIsGuessedOnlyFromRealState(
        int? lookedMinutesAgo, int unseen, SpotLens expected)
    {
        var looked = lookedMinutesAgo is { } m ? Now.AddMinutes(-m) : (DateTime?)null;

        Assert.Equal(expected, SpotLensView.OpeningLens(looked, Now, unseen));
    }

    /// <remarks>
    /// Proves the two lenses read differently in words as well as in what they
    /// show. The question on hover is the teaching: a newcomer who reads that
    /// hunting again after a contact is normal has learned something nobody
    /// tells them.
    /// </remarks>
    [Fact]
    public void EachLensSaysWhatItIsFor()
    {
        var best = SpotLensView.Question(SpotLens.BestChance);
        var fresh = SpotLensView.Question(SpotLens.WhatsNew);

        Assert.NotEqual(best, fresh);
        Assert.Contains("after a contact", fresh, StringComparison.Ordinal);
        Assert.Contains("best shot", best, StringComparison.Ordinal);

        foreach (var line in new[] { best, fresh })
        {
            Assert.DoesNotContain("—", line, StringComparison.Ordinal);
        }
    }
}
