using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// The worked contact, both sides, annotated (HM-DEC-043).
/// </summary>
public sealed class ContactShapeTests
{
    /// <remarks>
    /// Proves both styles run the whole way through, from the first CQ to the
    /// sign-off. A half-written example would leave somebody stranded exactly
    /// where the nerve runs out.
    /// </remarks>
    [Theory]
    [InlineData(ContactStyle.Cw)]
    [InlineData(ContactStyle.Ssb)]
    public void BothStylesRunFromTheFirstCallToTheSignOff(ContactStyle style)
    {
        var steps = ContactShape.Steps(style, "KC3QIS");

        Assert.True(steps.Count >= 5, $"only {steps.Count} steps");
        Assert.Contains("CQ", steps[0].Sent, StringComparison.Ordinal);

        // Morse sends the digits; voice says them. Either way the contact ends
        // with best regards rather than trailing off.
        var signOff = steps[^1].Sent;
        Assert.True(
            signOff.Contains("73", StringComparison.Ordinal)
            || signOff.Contains("seventy three", StringComparison.OrdinalIgnoreCase),
            $"the sign-off carries no 73: {signOff}");
    }

    /// <remarks>
    /// Proves the panel never writes "73s". The glossary says the number is
    /// already plural, and an app that contradicted its own glossary two
    /// panels apart would undo the trust the glossary is there to build.
    /// </remarks>
    [Fact]
    public void NothingEverPluralizes73()
    {
        // What actually goes on the air, in both styles. The notes are allowed
        // to quote the wrong form in order to warn against it, which is
        // checked separately below.
        var sent = string.Join(
            " ",
            new[] { ContactStyle.Cw, ContactStyle.Ssb }
                .SelectMany(s => ContactShape.Steps(s, "KC3QIS"))
                .SelectMany(s => new[] { s.Sent, s.Meaning }));

        Assert.DoesNotContain("73s", sent, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("73's", sent, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("seventy threes", sent, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("seventy-threes", sent, StringComparison.OrdinalIgnoreCase);

        // And the rule is stated where somebody would otherwise guess wrong.
        var notes = string.Join(
            " ", ContactShape.Steps(ContactStyle.Ssb, "KC3QIS").Select(s => s.Note));

        Assert.Contains("already plural", notes, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves the two sides alternate, starting and ending with the operator.
    /// A worked example where the same person speaks twice running would teach
    /// the wrong rhythm.
    /// </remarks>
    [Theory]
    [InlineData(ContactStyle.Cw)]
    [InlineData(ContactStyle.Ssb)]
    public void TheTwoSidesAlternate(ContactStyle style)
    {
        var steps = ContactShape.Steps(style, "KC3QIS");

        Assert.Equal(ContactSpeaker.You, steps[0].Speaker);
        Assert.Equal(ContactSpeaker.You, steps[^1].Speaker);

        for (var i = 1; i < steps.Count; i++)
        {
            Assert.NotEqual(steps[i - 1].Speaker, steps[i].Speaker);
        }
    }

    /// <remarks>
    /// THE THING THAT MAKES IT A REHEARSAL RATHER THAN A MANUAL. Proves the
    /// operator's own callsign is in the example, everywhere they would send
    /// it.
    /// </remarks>
    [Fact]
    public void TheExampleUsesTheOperatorsOwnCallsign()
    {
        var steps = ContactShape.Steps(ContactStyle.Cw, "w2xyz");

        Assert.All(
            steps.Where(s => s.Speaker == ContactSpeaker.You),
            s => Assert.Contains("W2XYZ", s.Sent, StringComparison.Ordinal));

        // And never the placeholder, once a real call is known.
        Assert.DoesNotContain(
            steps, s => s.Sent.Contains(ContactShape.DefaultYourCall, StringComparison.Ordinal));
    }

    /// <remarks>
    /// Proves an empty callsign falls back to the example one rather than
    /// leaving a hole where the call should be. Somebody reading this before
    /// they have filled in Settings still gets a complete example.
    /// </remarks>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AMissingCallsignFallsBackToTheExample(string? callsign)
    {
        var steps = ContactShape.Steps(ContactStyle.Cw, callsign);

        Assert.Contains(
            steps, s => s.Sent.Contains(ContactShape.DefaultYourCall, StringComparison.Ordinal));
    }

    /// <remarks>
    /// Proves the mechanical shorthand is explained where it is used. These
    /// are the four that stop a newcomer dead, and every one of them is
    /// meaningless unless somebody says what it stands for.
    /// </remarks>
    [Theory]
    [InlineData("DE")]
    [InlineData("BK")]
    [InlineData("SK")]
    [InlineData("73")]
    public void TheShorthandIsExplainedWhereItIsUsed(string token)
    {
        var notes = string.Join(
            " ", ContactShape.Steps(ContactStyle.Cw, "KC3QIS").Select(s => s.Note));

        Assert.Contains(token, notes, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the two things a beginner most needs told are actually said: why
    /// a callsign goes out twice, and that a ninety-second contact is normal
    /// rather than a brush-off.
    /// </remarks>
    [Fact]
    public void TheReassurancesAreActuallyThere()
    {
        var all = ContactShape.Preamble + " "
            + string.Join(" ", ContactShape.Steps(ContactStyle.Cw, "KC3QIS")
                .Select(s => s.Note))
            + " " + ContactShape.Closing;

        Assert.Contains("twice", all, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ninety seconds", all, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("brushing you off", all, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// TONE MATTERS MORE HERE THAN ANYWHERE ELSE. Proves nothing in the panel
    /// reads as a test or a rule the operator could fail. Nobody should finish
    /// this feeling examined.
    /// </remarks>
    [Fact]
    public void NothingReadsLikeATest()
    {
        var all = ContactShape.Preamble + " " + ContactShape.Closing + " "
            + string.Join(
                " ",
                new[] { ContactStyle.Cw, ContactStyle.Ssb }
                    .SelectMany(s => ContactShape.Steps(s, "KC3QIS"))
                    .SelectMany(s => new[] { s.Meaning, s.Note }));

        foreach (var phrase in new[]
                 {
                     "you must", "make sure you", "be careful", "do not forget",
                     "required", "mandatory", "correctly", "mistake",
                 })
        {
            Assert.DoesNotContain(phrase, all, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <remarks>
    /// Proves every step says what it means, so nobody is left staring at a
    /// line of shorthand with no translation under it.
    /// </remarks>
    [Theory]
    [InlineData(ContactStyle.Cw)]
    [InlineData(ContactStyle.Ssb)]
    public void EveryStepIsTranslated(ContactStyle style)
        => Assert.All(ContactShape.Steps(style, "KC3QIS"), s =>
        {
            Assert.False(string.IsNullOrWhiteSpace(s.Sent));
            Assert.False(string.IsNullOrWhiteSpace(s.Meaning));
        });

    /// <remarks>
    /// Proves the voice version spells callsigns phonetically, since the whole
    /// difference between the two styles is that one is spoken.
    /// </remarks>
    [Fact]
    public void TheVoiceVersionUsesPhonetics()
    {
        var first = ContactShape.Steps(ContactStyle.Ssb, "KC3QIS")[0].Sent;

        Assert.Contains("Kilo Charlie Three Quebec India Sierra", first, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the phonetic alphabet is the standard one, letters and digits
    /// alike, so somebody who learns it here is learning what everybody else
    /// uses.
    /// </remarks>
    [Theory]
    [InlineData("W1ABC", "Whiskey One Alfa Bravo Charlie")]
    [InlineData("K0Z", "Kilo Zero Zulu")]
    [InlineData("kc3qis", "Kilo Charlie Three Quebec India Sierra")]
    public void ThePhoneticAlphabetIsTheStandardOne(string callsign, string expected)
        => Assert.Equal(expected, ContactShape.Spell(callsign));

    /// <remarks>
    /// Proves the copy obeys the voice standard it was written under
    /// (HM-DEC-040).
    /// </remarks>
    [Fact]
    public void TheCopyObeysTheDashRule()
    {
        var passages = new List<string> { ContactShape.Preamble, ContactShape.Closing };

        foreach (var style in new[] { ContactStyle.Cw, ContactStyle.Ssb })
        {
            foreach (var step in ContactShape.Steps(style, "KC3QIS"))
            {
                passages.Add(step.Meaning);
                passages.Add(step.Note);
            }
        }

        Assert.All(passages, p =>
            Assert.True(p.Count(c => c == '—') <= 1, $"too many dashes: {p}"));
    }

    /// <remarks>
    /// Proves it is pure (§5): the same callsign and style always produce the
    /// same script.
    /// </remarks>
    [Fact]
    public void TheScriptIsDeterministic()
        => Assert.Equal(
            ContactShape.Steps(ContactStyle.Cw, "KC3QIS").Select(s => s.Sent),
            ContactShape.Steps(ContactStyle.Cw, "KC3QIS").Select(s => s.Sent));
}
