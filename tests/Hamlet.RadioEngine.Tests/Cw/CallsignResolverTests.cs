using Hamlet.RadioEngine.Cw;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Claiming a callsign, and refusing to nearly claim one (HM-DEC-073).
/// </summary>
/// <remarks>
/// The tests that matter most here are the ones that assert nothing is claimed.
/// A wrong callsign in front of the operator is worse than no callsign, and
/// worse still on a day he is using it to decide whether anybody answered him.
/// </remarks>
public sealed class CallsignResolverTests
{
    /// <summary>Build a transcript, marking named characters as uncertain.</summary>
    /// <param name="text">The text, with spaces as word gaps.</param>
    /// <param name="dimAt">Indexes into the text to mark low or unreadable.</param>
    /// <param name="confidence">What to mark them.</param>
    private static List<CwCharacter> Heard(
        string text,
        IEnumerable<int>? dimAt = null,
        CwConfidence confidence = CwConfidence.Low)
    {
        var dim = new HashSet<int>(dimAt ?? Array.Empty<int>());
        var characters = new List<CwCharacter>();

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            characters.Add(new CwCharacter(
                c == ' ' ? MorseAlphabet.WordGap : c.ToString(),
                c == ' '
                    ? CwConfidence.High
                    : dim.Contains(i) ? confidence : CwConfidence.High,
                Score: 1.0,
                Pattern: ".-",
                SignalToNoiseDb: 20,
                WordsPerMinute: 15,
                At: TimeSpan.FromSeconds(i)));
        }

        return characters;
    }

    // ---- Structure: only where the ritual puts a callsign ----------------

    /// <remarks>
    /// Proves HM-DEC-073: the station after DE is the one transmitting, which is
    /// the position the whole hobby agrees on.
    /// </remarks>
    [Fact]
    public void TheStationAfterDeIsClaimed()
    {
        var heard = Heard("CQ CQ CQ DE W1AW W1AW K");

        Assert.Equal("W1AW", CallsignResolver.StationHeard(heard));

        var claims = CallsignResolver.Resolve(heard);

        // Sent twice and folded into one claim rather than reported as two
        // stations.
        Assert.Single(claims, c => c.Role == CallsignRole.Sender);
        Assert.Equal("DE", claims[0].Marker);
    }

    /// <remarks>
    /// Proves HM-DEC-073: the token before DE is who they are calling, which is
    /// the whole of how Hamlet can tell somebody is answering rather than
    /// calling anybody.
    /// </remarks>
    [Fact]
    public void TheStationBeforeDeIsWhoTheyAreCalling()
    {
        var heard = Heard("KC3QIS DE W1AW W1AW K");

        var claims = CallsignResolver.Resolve(heard);

        Assert.Contains(claims, c =>
            c.Role == CallsignRole.Addressed && c.Callsign == "KC3QIS");
        Assert.Contains(claims, c =>
            c.Role == CallsignRole.Sender && c.Callsign == "W1AW");

        Assert.Equal("W1AW", CallsignResolver.AnsweringYou(heard, "KC3QIS"));
    }

    /// <remarks>
    /// Proves HM-DEC-073: somebody answering a different station is not
    /// somebody answering this one. This is the false positive that would hurt
    /// most today, because it would tell him he had a contact when he does not.
    /// </remarks>
    [Fact]
    public void SomebodyAnsweringAnotherStationIsNotAnsweringYou()
    {
        var heard = Heard("N0CALL DE W1AW W1AW K");

        Assert.Null(CallsignResolver.AnsweringYou(heard, "KC3QIS"));

        // And the station is still named, because it really was heard.
        Assert.Equal("W1AW", CallsignResolver.StationHeard(heard));
    }

    /// <remarks>
    /// Proves HM-DEC-073: the station signing in front of a closing prosign is
    /// claimed, since nobody puts anything but their own call there.
    /// </remarks>
    [Theory]
    [InlineData("W1AW K")]
    [InlineData("W1AW KN")]
    [InlineData("W1AW SK")]
    public void TheStationSigningInFrontOfAProsignIsClaimed(string text)
    {
        Assert.Equal("W1AW", CallsignResolver.StationHeard(Heard(text)));
    }

    /// <remarks>
    /// Proves HM-DEC-073: a callsign-shaped string in loose text is not
    /// claimed. The shape of a callsign is also the shape of plenty of things
    /// that are not one, and structure is what tells them apart.
    /// </remarks>
    [Theory]
    [InlineData("SOME TEXT W1AW MORE TEXT")]
    [InlineData("QRM HR W1AW QSB")]
    [InlineData("W1AW")]
    public void ACallsignShapedStringInLooseTextIsNotClaimed(string text)
    {
        Assert.Null(CallsignResolver.StationHeard(Heard(text)));
        Assert.Empty(CallsignResolver.Resolve(Heard(text)));
    }

    /// <remarks>
    /// Proves HM-DEC-073: ritual words are never claimed however they are
    /// placed, so a report or an abbreviation cannot be read as a station.
    /// </remarks>
    [Theory]
    [InlineData("DE CQ K")]
    [InlineData("DE 599 K")]
    [InlineData("DE 5NN K")]
    [InlineData("DE QSL K")]
    [InlineData("DE TU K")]
    [InlineData("UR RST 579 579 BK")]
    public void RitualWordsAndReportsAreNeverClaimed(string text)
    {
        Assert.Null(CallsignResolver.StationHeard(Heard(text)));
    }

    // ---- Cleanliness: every character solid ------------------------------

    /// <remarks>
    /// Proves HM-DEC-073: one dimmed character and the callsign is not claimed.
    /// KC3QIS with one uncertain character is also a plausible reading of other
    /// real callsigns belonging to other people, and there is no version of
    /// showing that which does not get read as fact.
    /// </remarks>
    [Fact]
    public void OneDimmedCharacterAndNothingIsClaimed()
    {
        const string text = "CQ CQ DE W1AW K";

        // Solid, it resolves.
        Assert.Equal("W1AW", CallsignResolver.StationHeard(Heard(text)));

        // Dim any one character of the callsign and it does not.
        for (var i = text.IndexOf("W1AW", StringComparison.Ordinal);
             i < text.IndexOf("W1AW", StringComparison.Ordinal) + 4;
             i++)
        {
            Assert.Null(
                CallsignResolver.StationHeard(Heard(text, new[] { i })));
        }
    }

    /// <remarks>
    /// Proves HM-DEC-073: an unresolved character taints the token it lands in,
    /// which is the whole reason the decoder marks it rather than guessing.
    /// </remarks>
    [Fact]
    public void OneBlockedCharacterAndNothingIsClaimed()
    {
        var heard = Heard(
            "CQ CQ DE W1AW K",
            new[] { 9 },
            CwConfidence.Unreadable);

        Assert.Null(CallsignResolver.StationHeard(heard));
        Assert.Empty(CallsignResolver.Resolve(heard));
    }

    /// <remarks>
    /// Proves HM-DEC-073: a dimmed character somewhere else does not stop a
    /// clean callsign being claimed. The rule is about the callsign, not about
    /// the transmission, and refusing on any noise anywhere would make the
    /// feature useless on exactly the signals it exists for.
    /// </remarks>
    [Fact]
    public void NoiseElsewhereDoesNotStopACleanCallsign()
    {
        var heard = Heard("CQ CQ DE W1AW K", new[] { 0, 1 });

        Assert.Equal("W1AW", CallsignResolver.StationHeard(heard));
    }

    /// <remarks>
    /// Proves HM-DEC-073: nothing heard is nothing claimed, and that is the
    /// ordinary answer rather than a failure.
    /// </remarks>
    [Fact]
    public void NothingHeardIsNothingClaimed()
    {
        Assert.Empty(CallsignResolver.Resolve(null));
        Assert.Empty(CallsignResolver.Resolve(Array.Empty<CwCharacter>()));
        Assert.Null(CallsignResolver.StationHeard(Heard("   ")));
        Assert.Null(CallsignResolver.AnsweringYou(Heard("KC3QIS DE W1AW K"), ""));
    }

    /// <remarks>
    /// Proves HM-DEC-073: real callsign shapes are accepted and near-misses are
    /// not, so the shape gate does real work rather than waving everything
    /// through.
    /// </remarks>
    [Theory]
    [InlineData("W1AW", true)]
    [InlineData("KC3QIS", true)]
    [InlineData("G0ABC", true)]
    [InlineData("2E0ABC", true)]
    [InlineData("VE3XYZ", true)]
    [InlineData("W1AW/P", true)]
    [InlineData("ABCDEF", false)]
    [InlineData("123456", false)]
    [InlineData("W1", false)]
    [InlineData("HELLO", false)]
    public void TheShapeGateAcceptsCallsignsAndRejectsNearMisses(
        string token, bool claimed)
    {
        var heard = Heard($"CQ DE {token} K");

        Assert.Equal(claimed ? token : null, CallsignResolver.StationHeard(heard));
    }
}
