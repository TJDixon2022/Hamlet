using Hamlet.RadioEngine.Cw;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Following a contact, and admitting when it has stopped (HM-DEC-076).
/// </summary>
/// <remarks>
/// The lost state is what these mostly prove. A guide that silently keeps
/// guessing after it stopped following sends somebody confidently to the wrong
/// part of a ritual they have never performed, which is worse than saying
/// nothing at all.
/// </remarks>
public sealed class ContactTrackerTests
{
    private static readonly DateTime Now = new(2026, 8, 15, 16, 0, 0, DateTimeKind.Utc);

    private const string Mine = "KC3QIS";

    private static List<CwCharacter> Heard(string text, params int[] dimAt)
    {
        var dim = new HashSet<int>(dimAt);
        var characters = new List<CwCharacter>();

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            characters.Add(new CwCharacter(
                c == ' ' ? MorseAlphabet.WordGap : c.ToString(),
                c == ' ' || !dim.Contains(i) ? CwConfidence.High : CwConfidence.Low,
                1.0, ".-", 20, 15, TimeSpan.FromSeconds(i)));
        }

        return characters;
    }

    // ---- Lost is the default -------------------------------------------

    /// <remarks>
    /// Proves HM-DEC-076: with nothing sent and nothing heard, Hamlet is lost
    /// rather than "calling". It has no reason to believe a contact is happening
    /// at all, and naming a stage would be inventing one.
    /// </remarks>
    [Fact]
    public void WithNothingAtAllHamletIsLost()
    {
        var follow = ContactTracker.Follow(null, null, null, null, Mine, Now);

        Assert.Equal(ContactFollowState.Lost, follow.State);
        Assert.Null(follow.TheirCall);
        Assert.Equal("", follow.Evidence);
        Assert.Equal(ContactTracker.LostSays, follow.Says);
    }

    /// <remarks>
    /// Proves HM-DEC-076: this is the commonest case on a real band. Something
    /// was heard, none of it resolved, and Hamlet says so rather than guessing
    /// from what usually happens next.
    /// </remarks>
    [Fact]
    public void SomethingHeardThatResolvedToNothingIsLost()
    {
        var noise = Heard("ETEE TATE RRR");

        var follow = ContactTracker.Follow(
            ContactStage.Calling, Now, noise, Now, Mine, Now.AddSeconds(5));

        Assert.Equal(ContactFollowState.Lost, follow.State);
    }

    /// <remarks>
    /// Proves HM-DEC-076: evidence goes stale. A contact that has produced
    /// nothing for minutes has ended or moved on without Hamlet, and sitting on
    /// the last stage it saw is exactly the silent guessing this exists to stop.
    /// </remarks>
    [Fact]
    public void AnOldBeliefExpiresRatherThanPersisting()
    {
        var answered = Heard($"{Mine} DE W1AW W1AW K");

        var fresh = ContactTracker.Follow(
            ContactStage.Calling, Now, answered, Now, Mine, Now.AddSeconds(10));

        Assert.Equal(ContactFollowState.TheyAnswered, fresh.State);

        var stale = ContactTracker.Follow(
            ContactStage.Calling, Now, answered, Now, Mine,
            Now + ContactTracker.Staleness);

        Assert.Equal(ContactFollowState.Lost, stale.State);
    }

    /// <remarks>
    /// Proves HM-DEC-076: somebody answering a different station does not move
    /// this contact anywhere. It is the false positive that would hurt most,
    /// because it would tell him he had a contact when he does not.
    /// </remarks>
    [Fact]
    public void SomebodyAnsweringAnotherStationLeavesHamletLost()
    {
        var other = Heard("N0CALL DE W1AW W1AW K");

        var follow = ContactTracker.Follow(
            ContactStage.Calling, Now, other, Now, Mine, Now.AddSeconds(5));

        Assert.Equal(ContactFollowState.Lost, follow.State);
        Assert.Null(follow.TheirCall);
    }

    /// <remarks>
    /// Proves HM-DEC-076 and HM-DEC-073 together: a dimmed character in the
    /// callsign means nothing resolved, so the tracker stays lost. The two rules
    /// compose rather than each having its own idea of what counts as heard.
    /// </remarks>
    [Fact]
    public void ADimmedCallsignDoesNotMoveTheContact()
    {
        const string text = "KC3QIS DE W1AW K";
        var dimmed = Heard(text, text.IndexOf("W1AW", StringComparison.Ordinal) + 1);

        var follow = ContactTracker.Follow(
            ContactStage.Calling, Now, dimmed, Now, Mine, Now.AddSeconds(5));

        Assert.Equal(ContactFollowState.Lost, follow.State);
    }

    /// <remarks>
    /// Proves HM-DEC-073 and HM-DEC-076 compose correctly on the ordinary case:
    /// a callsign sent twice with one copy clean is identified from the clean
    /// copy. The first is half-missed while somebody is still tuning in, which
    /// is exactly why operators send it twice, and refusing both would throw
    /// away the repeat's whole purpose.
    /// </remarks>
    [Fact]
    public void ARepeatedCallsignResolvesFromTheCleanCopy()
    {
        const string text = "KC3QIS DE W1AW W1AW K";
        var half = Heard(text, text.IndexOf("W1AW", StringComparison.Ordinal) + 1);

        var follow = ContactTracker.Follow(
            ContactStage.Calling, Now, half, Now, Mine, Now.AddSeconds(5));

        Assert.Equal(ContactFollowState.TheyAnswered, follow.State);
        Assert.Equal("W1AW", follow.TheirCall);

        // And dimming both copies leaves nothing to claim.
        var both = Heard(
            text,
            text.IndexOf("W1AW", StringComparison.Ordinal) + 1,
            text.LastIndexOf("W1AW", StringComparison.Ordinal) + 1);

        Assert.Equal(
            ContactFollowState.Lost,
            ContactTracker.Follow(
                ContactStage.Calling, Now, both, Now, Mine, Now.AddSeconds(5)).State);
    }

    // ---- The transitions it can justify ---------------------------------

    /// <remarks>
    /// Proves HM-DEC-076: a call with nothing decoded since is the one thing
    /// Hamlet can say from what it sent alone, and it says nothing about any
    /// station because none was identified.
    /// </remarks>
    [Fact]
    public void ACallWithNothingBackIsCalling()
    {
        var follow = ContactTracker.Follow(
            ContactStage.Calling, Now, null, null, Mine, Now.AddSeconds(20));

        Assert.Equal(ContactFollowState.Calling, follow.State);
        Assert.Null(follow.TheirCall);
        Assert.Contains("Listening", follow.Says, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves HM-DEC-076: his own callsign in the addressed position with a
    /// clean callsign after the DE is the one transition Hamlet can be certain
    /// of, and it names who came back.
    /// </remarks>
    [Fact]
    public void SomebodyComingBackByNameIsFollowed()
    {
        var follow = ContactTracker.Follow(
            ContactStage.Calling, Now, Heard($"{Mine} DE W1AW W1AW K"), Now,
            Mine, Now.AddSeconds(5));

        Assert.Equal(ContactFollowState.TheyAnswered, follow.State);
        Assert.Equal("W1AW", follow.TheirCall);
        Assert.Contains("W1AW came back to you", follow.Says, StringComparison.Ordinal);
        Assert.NotEqual("", follow.Evidence);
    }

    /// <remarks>
    /// Proves HM-DEC-076: a report word alongside a resolved answer moves it on,
    /// because both halves are observed rather than assumed.
    /// </remarks>
    [Theory]
    [InlineData("RST")]
    [InlineData("UR")]
    [InlineData("5NN")]
    public void AReportWordMovesItToTheExchange(string word)
    {
        var follow = ContactTracker.Follow(
            ContactStage.Answering, Now,
            Heard($"{Mine} DE W1AW {word} 579 BK"), Now, Mine, Now.AddSeconds(5));

        Assert.Equal(ContactFollowState.Exchanging, follow.State);
        Assert.Equal("W1AW", follow.TheirCall);
    }

    /// <remarks>
    /// Proves HM-DEC-076: a sign-off word ends it, and the sentence says what
    /// makes it a contact rather than leaving somebody wondering whether it
    /// counted.
    /// </remarks>
    [Fact]
    public void ASignOffWordEndsIt()
    {
        var follow = ContactTracker.Follow(
            ContactStage.Confirming, Now,
            Heard($"{Mine} DE W1AW 73 SK"), Now, Mine, Now.AddSeconds(5));

        Assert.Equal(ContactFollowState.SigningOff, follow.State);
        Assert.Contains("73 back", follow.Says, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves HM-DEC-076: a half-read closing word does not end a contact that
    /// is still going. A dimmed "73" is also a dimmed anything else.
    /// </remarks>
    [Fact]
    public void AHalfReadSignOffDoesNotEndTheContact()
    {
        var text = $"{Mine} DE W1AW 73";
        var dimmed = Heard(text, text.Length - 1);

        var follow = ContactTracker.Follow(
            ContactStage.Confirming, Now, dimmed, Now, Mine, Now.AddSeconds(5));

        Assert.Equal(ContactFollowState.TheyAnswered, follow.State);
        Assert.NotEqual(ContactFollowState.SigningOff, follow.State);
    }

    /// <remarks>
    /// Proves HM-DEC-076: every state carries a sentence somebody can read, and
    /// the lost one hands back the terminal rather than apologizing.
    /// </remarks>
    [Fact]
    public void EveryStateSaysSomethingUseful()
    {
        var cases = new[]
        {
            ContactTracker.Follow(null, null, null, null, Mine, Now),
            ContactTracker.Follow(ContactStage.Calling, Now, null, null, Mine, Now),
            ContactTracker.Follow(
                ContactStage.Calling, Now, Heard($"{Mine} DE W1AW K"), Now, Mine, Now),
        };

        foreach (var follow in cases)
        {
            Assert.NotEqual("", follow.Says);
            Assert.False(follow.Says.EndsWith(' '));
        }

        Assert.Contains("terminal", ContactTracker.LostSays, StringComparison.Ordinal);
    }
}
