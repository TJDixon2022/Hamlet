using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// The noise blanker, the noise reduction and the filter width are named when
/// they are in the way and left alone when they are not.
/// </summary>
/// <remarks>
/// <para>**HAMLET READ ALL THREE FROM THE RADIO FOR MONTHS AND MENTIONED
/// NONE.** HM-DEC-148 is the ruling that a thing Hamlet knows and does not say
/// is the same defect as a decode with no signal behind it, and it named the
/// preamp and the attenuator and stopped there. These three are the same class
/// of fault.</para>
/// <para>**THE NEGATIVES ARE THE POINT.** A setting already in the right
/// position must produce nothing, and a setting that could not be read must say
/// so rather than being reported as harmless. Those two are what separate a
/// panel that teaches from a panel that gets read past.</para>
/// </remarks>
public sealed class WhatIsInTheWayOnTheReceiveSideTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the sentences are printed.</param>
    public WhatIsInTheWayOnTheReceiveSideTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// A radio with the three settings at stated values; null means never read.
    /// </summary>
    private static RigState Radio(
        double? blanker = 0,
        double? reduction = 0,
        double? filter = ReceiveAdvice.CwFilterIndex)
    {
        var values = new List<RigValue>();

        Set(values, RigField.NoiseBlanker, blanker);
        Set(values, RigField.NoiseReduction, reduction);
        Set(values, RigField.FilterBandwidth, filter);

        return RigState.Empty.With(values);
    }

    private static void Set(List<RigValue> into, RigField field, double? number)
        => into.Add(number is { } value
            ? RigValue.Known(
                field,
                value,
                value.ToString("0", System.Globalization.CultureInfo.InvariantCulture),
                DateTime.UnixEpoch,
                "test")
            : RigValue.Unknown(field));

    private void Print(IReadOnlyList<ReceiveObstruction> found)
    {
        if (found.Count == 0)
        {
            _output.WriteLine("(nothing in the way)");

            return;
        }

        foreach (var one in found)
        {
            _output.WriteLine($"{one.Setting}: {one.Says}");
        }
    }

    /// <remarks>
    /// **Proves the whole reason this exists.** All three off or sensible, and
    /// the panel stays quiet. Advice about a knob already in the right place is
    /// noise, and a panel that talks when it has nothing to say is one the
    /// operator learns to look past.
    /// </remarks>
    [Fact]
    public void AReceivePathAlreadyRightSaysNothingAtAll()
    {
        var found = ReceiveObstructions.For(
            Radio(), inMorse: true, competitorInside: false);

        Print(found);

        Assert.Empty(found);
    }

    /// <remarks>
    /// Proves the noise blanker is named with the control, not merely reported.
    /// </remarks>
    [Fact]
    public void TheNoiseBlankerIsNamedWithTheButtonThatTurnsItOff()
    {
        var found = ReceiveObstructions.For(
            Radio(blanker: 1), inMorse: true, competitorInside: false);

        Print(found);

        var one = Assert.Single(found);

        Assert.Equal("noise blanker", one.Setting);
        Assert.Contains("NB", one.Says, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the noise reduction likewise, and that the reason given is about
    /// what the decoder measures rather than a general complaint about the
    /// setting.
    /// </remarks>
    [Fact]
    public void TheNoiseReductionIsNamedWithTheButtonThatTurnsItOff()
    {
        var found = ReceiveObstructions.For(
            Radio(reduction: 3), inMorse: true, competitorInside: false);

        Print(found);

        var one = Assert.Single(found);

        Assert.Equal("noise reduction", one.Setting);
        Assert.Contains("NR", one.Says, StringComparison.Ordinal);
        Assert.Contains("dit from a dah", one.Says, StringComparison.Ordinal);
    }

    /// <remarks>
    /// **Proves the second negative, which is the one HM-DEC-009 turns on.** A
    /// setting Hamlet could not read says so. Hamlet not having looked and Hamlet
    /// having looked and found nothing are different facts, and a quiet panel
    /// would report the first as the second.
    /// </remarks>
    [Fact]
    public void ASettingThatCouldNotBeReadSaysSoRatherThanNothing()
    {
        var found = ReceiveObstructions.For(
            Radio(blanker: null), inMorse: true, competitorInside: false);

        Print(found);

        var one = Assert.Single(found);

        Assert.Equal("noise blanker", one.Setting);
        Assert.Contains("could not read", one.Says, StringComparison.Ordinal);
        Assert.DoesNotContain("is on", one.Says, StringComparison.Ordinal);
    }

    /// <remarks>
    /// **Proves the filter is named on a measurement and not on a width.** With
    /// nobody else in the passband a wide filter is not in anybody's way, and
    /// asserting that some width is too wide for a signal Hamlet has not measured
    /// would be a judgement nobody ruled (§0.0).
    /// </remarks>
    [Fact]
    public void AWideFilterWithNobodyElseInItIsNotInTheWay()
    {
        var found = ReceiveObstructions.For(
            Radio(filter: ReceiveAdvice.WidestUsefulIndex + 8),
            inMorse: true,
            competitorInside: false);

        Print(found);

        Assert.Empty(found);
    }

    /// <remarks>
    /// And the other half: once somebody else is measurably inside it, the filter
    /// is what let them in and the control is named.
    /// </remarks>
    [Fact]
    public void AWideFilterWithSomebodyElseInItIsNamed()
    {
        var found = ReceiveObstructions.For(
            Radio(filter: ReceiveAdvice.WidestUsefulIndex + 8),
            inMorse: true,
            competitorInside: true);

        Print(found);

        var one = Assert.Single(found);

        Assert.Equal("filter width", one.Setting);
        Assert.Contains("FILTER", one.Says, StringComparison.Ordinal);
        Assert.Contains("TWIN PBT", one.Says, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves a filter already narrow is left alone even with a competitor in
    /// the passband: there is nothing left to take, and telling somebody to
    /// narrow a knob that is already down is advice about a knob in the right
    /// place.
    /// </remarks>
    [Fact]
    public void AFilterAlreadyNarrowIsNotMentionedEvenWithSomebodyElseInThere()
    {
        var found = ReceiveObstructions.For(
            Radio(filter: ReceiveAdvice.NarrowestUsefulIndex),
            inMorse: true,
            competitorInside: true);

        Print(found);

        Assert.Empty(found);
    }

    /// <remarks>
    /// Proves nothing here writes to the radio. The type carries a setting and a
    /// sentence and has nowhere to put a command, which is stronger than every
    /// call site remembering (HM-DEC-084's reasoning about payload shapes).
    /// </remarks>
    [Fact]
    public void NothingHereCanWriteToTheRadio()
    {
        var properties = typeof(ReceiveObstruction).GetProperties()
            .Select(p => p.Name)
            .ToList();

        _output.WriteLine(string.Join(", ", properties));

        Assert.Equal(new[] { "Setting", "Says" }, properties);
    }
}
