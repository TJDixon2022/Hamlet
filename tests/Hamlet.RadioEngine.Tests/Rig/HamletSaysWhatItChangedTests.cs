using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// After a tune-in the operator is told what changed and why, in the app's
/// voice.
/// </summary>
/// <remarks>
/// <para>**TASK 4 OF WORK INSTRUCTION 042.** A radio that changes itself
/// silently is the "is it broken" confusion relocated rather than removed, and
/// this operator has had enough of machines doing things without saying so
/// (HM-DEC-056).</para>
/// <para>**THE THREE RULES ARE ALL ABOUT NOT OVERCLAIMING.** Only what actually
/// changed is mentioned; anything unconfirmed is said as unconfirmed rather than
/// left out; anything his own hand is holding is said as his. The last is the
/// smallest of the three and the one most easily got wrong, because taking
/// credit for a switch he set himself reads as harmless right up until he
/// notices.</para>
/// </remarks>
public sealed class HamletSaysWhatItChangedTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the sentences are printed.</param>
    public HamletSaysWhatItChangedTests(ITestOutputHelper output) => _output = output;

    private static IReadOnlyList<ReceiverCondition> Ft8()
        => ReceiverConditions.ForBlock(
            NeighborhoodPlan
                .ForBand(HfBands.Bands.First(b => b.Name == "20 m"))
                .First(n => n.Name == "FT8 city"));

    private static ReceiverCondition Of(string control)
        => Ft8().Single(c => c.Control == control);

    /// <remarks>
    /// <para>**THE ORDER'S OWN EXAMPLE, AS CLOSE AS THE DATA ALLOWS.** Two
    /// controls changed, joined into one sentence with their reasons attached
    /// rather than listed as register writes.</para>
    /// <para>**AND THE ONE THAT WAS ALREADY CORRECT IS NOT IN IT.** Narrating a
    /// control nobody touched teaches him to stop reading the line, and then the
    /// one that matters goes past unread.</para>
    /// </remarks>
    [Fact]
    public void OnlyWhatChangedIsMentionedAndTheReasonComesWithIt()
    {
        var said = ReceiverSetupVoice.Say(new[]
        {
            new ConditionResult(
                Of("noise blanker"), ConditionOutcome.Changed, "on", "off"),
            new ConditionResult(
                Of("noise reduction"), ConditionOutcome.AlreadyRight, "off", "off"),
            new ConditionResult(
                Of("auto notch"), ConditionOutcome.Changed, "on", "off"),
        });

        _output.WriteLine("  " + said);

        Assert.Contains("turned the noise blanker off", said, StringComparison.Ordinal);
        Assert.Contains("turned the auto notch off", said, StringComparison.Ordinal);

        // The reason travels with the fact, which is what makes it teach (§0.7).
        Assert.Contains("because it chops up", said, StringComparison.Ordinal);
        Assert.Contains("hunts steady carriers", said, StringComparison.Ordinal);

        // **THE ONE THAT WAS ALREADY RIGHT IS ABSENT.**
        Assert.DoesNotContain("noise reduction", said, StringComparison.Ordinal);

        // One sentence, joined, rather than two stacked declarations.
        Assert.StartsWith("I ", said, StringComparison.Ordinal);
        Assert.Contains(", and turned", said, StringComparison.Ordinal);

        // **AND NO COMMA BEFORE THE REASON.** With one, the clauses join as
        // "off, because it chops up the tones and turned the auto notch off",
        // and the "and" reads as part of the reason rather than as the next
        // thing Hamlet did. It only shows up when you read it aloud.
        Assert.DoesNotContain("off, because", said, StringComparison.Ordinal);
    }

    /// <remarks>
    /// **A CONTROL HE MOVED IS SAID TO BE HIS.** Not omitted, because he should
    /// know Hamlet saw it and stood back, and not narrated as something Hamlet
    /// did, because it is not.
    /// </remarks>
    [Fact]
    public void WhatHisOwnHandIsHoldingIsSaidToBeHis()
    {
        var said = ReceiverSetupVoice.Say(new[]
        {
            new ConditionResult(
                Of("noise blanker"), ConditionOutcome.LeftToTheOperator, "on", "on"),
        });

        _output.WriteLine("  " + said);

        Assert.Contains("Your noise blanker", said, StringComparison.Ordinal);
        Assert.Contains("you moved it", said, StringComparison.Ordinal);

        // Hamlet does not claim it.
        Assert.DoesNotContain("I turned", said, StringComparison.Ordinal);
        Assert.DoesNotContain("I set", said, StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>**WHAT COULD NOT BE CONFIRMED IS SAID, NOT OMITTED** (§0.0). A
    /// write the radio acknowledged and did not read back leaves the control in
    /// an unknown state, and an operator who is not told that will believe the
    /// setting took.</para>
    /// <para>**AND THE TWO KINDS OF CANNOT ARE DIFFERENT ADMISSIONS.** The span
    /// has no cited command, so Hamlet cannot reach it at all. The AGC has one
    /// and its value has not been settled here, so Hamlet will not use it. Those
    /// are a gap in what Hamlet can do and a gap in what Hamlet knows, and
    /// blurring them would misdescribe both.</para>
    /// </remarks>
    [Fact]
    public void WhatCouldNotBeDoneIsSaidRatherThanLeftOut()
    {
        var said = ReceiverSetupVoice.Say(new[]
        {
            new ConditionResult(
                Of("noise blanker"), ConditionOutcome.NotConfirmed, "on"),
            new ConditionResult(
                Of("noise reduction"), ConditionOutcome.NotRead),
            new ConditionResult(Of("AGC"), ConditionOutcome.SpokenOnly),
            new ConditionResult(Of("scope span"), ConditionOutcome.SpokenOnly),
        });

        _output.WriteLine("  " + said);

        Assert.Contains("did not confirm", said, StringComparison.Ordinal);
        Assert.Contains("I do not know where it is now", said, StringComparison.Ordinal);
        Assert.Contains("could not read the noise reduction", said, StringComparison.Ordinal);

        // The span: Hamlet cannot reach it, and the width is the block's own.
        Assert.Contains("3 kHz across", said, StringComparison.Ordinal);
        Assert.Contains("cannot set from here", said, StringComparison.Ordinal);

        // The AGC: reachable, and not settled.
        Assert.Contains("not settled well enough", said, StringComparison.Ordinal);
    }

    /// <remarks>
    /// **A RADIO ALREADY SET UP CORRECTLY PRODUCES SILENCE.** Not a
    /// reassurance: a line that says everything is fine on every tune-in is one
    /// nobody reads by the third time, and then the tune-in that did something
    /// looks the same as the ones that did not.
    /// </remarks>
    [Fact]
    public void NothingToSayIsARealAnswer()
    {
        var said = ReceiverSetupVoice.Say(new[]
        {
            new ConditionResult(
                Of("noise blanker"), ConditionOutcome.AlreadyRight, "off", "off"),
            new ConditionResult(
                Of("auto notch"), ConditionOutcome.AlreadyRight, "off", "off"),
        });

        Assert.Equal("", said);
        Assert.Equal("", ReceiverSetupVoice.Say(Array.Empty<ConditionResult>()));
    }

    /// <remarks>
    /// <para>**THE VOICE RULE, ON THE SENTENCE THAT IS ACTUALLY BUILT**
    /// (HM-DEC-040, §0.7). `VoiceTests` sweeps the literals in the source, and
    /// this is composed at run time from a data file and four outcome branches,
    /// so no sweep of literals can see the result.</para>
    /// <para>At most one em dash in a passage, and this one has none.</para>
    /// </remarks>
    [Fact]
    public void TheComposedSentenceKeepsTheVoiceRule()
    {
        var everything = Ft8()
            .Select(c => new ConditionResult(c, ConditionOutcome.Changed, "on", "off"))
            .Concat(Ft8().Select(
                c => new ConditionResult(c, ConditionOutcome.LeftToTheOperator, "on")))
            .Concat(Ft8().Select(
                c => new ConditionResult(c, ConditionOutcome.SpokenOnly)))
            .ToList();

        var said = ReceiverSetupVoice.Say(everything);

        _output.WriteLine("  " + said);

        var dashes = said.Count(c => c == '—');

        Assert.True(dashes <= 1, $"the sentence carries {dashes} em dashes");

        // Not a register echo: no command bytes reach the operator (HM-DEC-056).
        Assert.DoesNotContain("CI-V", said, StringComparison.Ordinal);
        Assert.DoesNotContain("0x", said, StringComparison.Ordinal);
        Assert.DoesNotContain("16 22", said, StringComparison.Ordinal);
    }
}
