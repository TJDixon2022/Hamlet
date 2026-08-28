using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Tuning into a block changes what would stop the operator hearing it, and
/// leaves everything else alone.
/// </summary>
/// <remarks>
/// <para>**TASK 3 OF WORK INSTRUCTION 042.** The ruling it is built from: tuning
/// into a neighborhood changes only what would stop the operator hearing or
/// seeing the block, leaves everything else alone, and says in plain words what
/// it changed and why.</para>
/// <para>**TWO THINGS WERE REJECTED AND BOTH ARE ASSERTED HERE.** Setting only
/// the mode and filter leaves the noise controls where the last operator left
/// them, and a noise blanker chops FT8 tones. Setting the whole family every
/// time overrides deliberate, skilled choices, so a control already right is not
/// written and one the operator has moved himself is his.</para>
/// <para>**AND THIS IS NOT A RIG-CONTROL PANEL** (HM-DEC-050). There is no row
/// of switches. He states an intent by tuning somewhere, and the settings are
/// what has to be true for that intent to work.</para>
/// </remarks>
public sealed class TheTuneInSetsOnlyWhatIsInTheWayTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tune-ins are printed.</param>
    public TheTuneInSetsOnlyWhatIsInTheWayTests(ITestOutputHelper output)
        => _output = output;

    private const byte Agc = 0x12;
    private const byte NoiseBlanker = 0x22;
    private const byte NoiseReduction = 0x40;
    private const byte AutoNotch = 0x41;

    private static Neighborhood Ft8City()
        => NeighborhoodPlan
            .ForBand(HfBands.Bands.First(b => b.Name == "20 m"))
            .First(n => n.Name == "FT8 city");

    private static async Task<(ScriptedRadio Radio, Ic7300Rig Rig)> ConnectAsync()
    {
        var radio = new ScriptedRadio { FrequencyHz = 14_074_000 };
        var rig = new Ic7300Rig(radio);

        Assert.True(await rig.ConnectAsync());
        return (radio, rig);
    }

    private void Print(IReadOnlyList<ConditionResult> results)
    {
        foreach (var result in results)
        {
            _output.WriteLine(
                $"  {result.Condition.Control,-15} {result.Outcome,-18} "
                + $"was={result.WasText ?? "-"} now={result.NowText ?? "-"}");
        }
    }

    /// <remarks>
    /// **THE OPERATOR'S RADIO ON 2026-08-28, ROUGHLY.** The noise blanker on, the
    /// auto notch on, noise reduction already off. Two get changed, one is
    /// recorded as already right and is not written, and the AGC is spoken and
    /// not touched because its value has not been established.
    /// </remarks>
    [Fact]
    public async Task WhatIsInTheWayIsChangedAndWhatIsAlreadyRightIsNot()
    {
        var (radio, rig) = await ConnectAsync();
        using var _ = rig;

        radio.OperatorTurnsASwitch(NoiseBlanker, 1);
        radio.OperatorTurnsASwitch(AutoNotch, 1);
        radio.OperatorTurnsASwitch(NoiseReduction, 0);

        var (results, memory) = await ReceiverSetup.ApplyAsync(
            rig, ReceiverConditions.ForBlock(Ft8City()), ReceiverSetupMemory.Empty);

        Print(results);

        Assert.Equal(0, radio.Switches[NoiseBlanker]);
        Assert.Equal(0, radio.Switches[AutoNotch]);

        // **NOTHING WAS SENT FOR THE ONE THAT WAS ALREADY RIGHT.** Two writes,
        // named, and not a third.
        Assert.Equal(2, radio.SwitchWrites.Count);
        Assert.Contains((NoiseBlanker, (byte)0), radio.SwitchWrites);
        Assert.Contains((AutoNotch, (byte)0), radio.SwitchWrites);
        Assert.DoesNotContain(radio.SwitchWrites, w => w.Sub == NoiseReduction);

        // **AND THE AGC WAS NOT TOUCHED AT ALL**, because the file states it and
        // does not confirm it. It was slow before and it is slow now.
        Assert.DoesNotContain(radio.SwitchWrites, w => w.Sub == Agc);
        Assert.Equal(3, radio.Switches[Agc]);

        Assert.Equal(
            ConditionOutcome.SpokenOnly,
            results.Single(r => r.Condition.Control == "AGC").Outcome);

        Assert.Equal(
            ConditionOutcome.AlreadyRight,
            results.Single(r => r.Condition.Control == "noise reduction").Outcome);

        Assert.Equal(
            ConditionOutcome.SpokenOnly,
            results.Single(r => r.Condition.Control == "scope span").Outcome);

        // The memory carries what Hamlet set and nothing it did not.
        Assert.Equal(2, memory.LastSet.Count);
        Assert.Equal(0, memory.LastSet[RigField.NoiseBlanker]);
        Assert.Equal(0, memory.LastSet[RigField.AutoNotch]);
    }

    /// <remarks>
    /// <para>**HIS OWN HAND WINS** (HM-DEC-056). Hamlet turns the noise blanker
    /// off on arriving; he switches it back on because he can hear an electric
    /// fence; he tunes away and comes back. It stays where he put it.</para>
    /// <para>**AND THE SUSPENSION IS ONLY ON THE ONE HE MOVED.** Turning the
    /// blanker back on does not hand him the auto notch as well, because he said
    /// something about the blanker and nothing about anything else.</para>
    /// </remarks>
    [Fact]
    public async Task AControlTheOperatorMovedHimselfIsLeftAlone()
    {
        var (radio, rig) = await ConnectAsync();
        using var _ = rig;

        radio.OperatorTurnsASwitch(NoiseBlanker, 1);
        radio.OperatorTurnsASwitch(AutoNotch, 1);

        var conditions = ReceiverConditions.ForBlock(Ft8City());

        var (_, memory) = await ReceiverSetup.ApplyAsync(
            rig, conditions, ReceiverSetupMemory.Empty);

        Assert.Equal(0, radio.Switches[NoiseBlanker]);

        // He reaches over and puts it back on, for a reason Hamlet cannot see.
        radio.OperatorTurnsASwitch(NoiseBlanker, 1);

        // And the auto notch drifts on from somewhere Hamlet did not set it.
        radio.Switches[AutoNotch] = 1;
        radio.SwitchWrites.Clear();

        var (results, _) = await ReceiverSetup.ApplyAsync(rig, conditions, memory);

        Print(results);

        Assert.Equal(
            ConditionOutcome.LeftToTheOperator,
            results.Single(r => r.Condition.Control == "noise blanker").Outcome);

        Assert.Equal(1, radio.Switches[NoiseBlanker]);
        Assert.DoesNotContain(radio.SwitchWrites, w => w.Sub == NoiseBlanker);

        // The auto notch is Hamlet's to set again: it also disagrees with what
        // Hamlet last wrote, so it is his too.
        Assert.Equal(
            ConditionOutcome.LeftToTheOperator,
            results.Single(r => r.Condition.Control == "auto notch").Outcome);
    }

    /// <remarks>
    /// **A CONTROL THE RADIO WILL NOT REPORT IS NOT WRITTEN.** Without a reading
    /// Hamlet cannot tell an operator who set this on purpose from a radio nobody
    /// has touched, and the operator's own hand wins. Silence is a stop, exactly
    /// as it is for the scanner (§0.2.1).
    /// </remarks>
    [Fact]
    public async Task AControlTheRadioWillNotReportIsNotWritten()
    {
        var (radio, rig) = await ConnectAsync();
        using var _ = rig;

        radio.OperatorTurnsASwitch(NoiseBlanker, 1);
        radio.Deaf.Add(NoiseBlanker);

        var (results, memory) = await ReceiverSetup.ApplyAsync(
            rig, ReceiverConditions.ForBlock(Ft8City()), ReceiverSetupMemory.Empty);

        Print(results);

        Assert.Equal(
            ConditionOutcome.NotRead,
            results.Single(r => r.Condition.Control == "noise blanker").Outcome);

        Assert.Equal(1, radio.Switches[NoiseBlanker]);
        Assert.DoesNotContain(radio.SwitchWrites, w => w.Sub == NoiseBlanker);
        Assert.DoesNotContain(RigField.NoiseBlanker, memory.LastSet.Keys);
    }

    /// <remarks>
    /// **ONCE PER TUNE-IN, THEN HANDS OFF.** Arriving a second time with nothing
    /// having changed sends nothing at all: every control reads back as already
    /// right, so there is no traffic and nothing to narrate.
    /// </remarks>
    [Fact]
    public async Task ArrivingAgainWithNothingChangedSendsNothing()
    {
        var (radio, rig) = await ConnectAsync();
        using var _ = rig;

        radio.OperatorTurnsASwitch(NoiseBlanker, 1);
        radio.OperatorTurnsASwitch(AutoNotch, 1);

        var conditions = ReceiverConditions.ForBlock(Ft8City());

        var (_, memory) = await ReceiverSetup.ApplyAsync(
            rig, conditions, ReceiverSetupMemory.Empty);

        Assert.Equal(2, radio.SwitchWrites.Count);

        radio.SwitchWrites.Clear();

        var (results, _) = await ReceiverSetup.ApplyAsync(rig, conditions, memory);

        Print(results);

        Assert.Empty(radio.SwitchWrites);

        Assert.All(
            results.Where(r => r.Condition.CanBeWritten),
            r => Assert.Equal(ConditionOutcome.AlreadyRight, r.Outcome));
    }

    /// <remarks>
    /// **A BLOCK THAT STATES NOTHING PRODUCES NO WRITE**, which is the other half
    /// of task 2's rule and the one that keeps this from becoming a set of
    /// settings applied everywhere. The dial sits in a Morse block and the radio
    /// is not touched.
    /// </remarks>
    [Fact]
    public async Task AMorseBlockProducesNoWriteAtAll()
    {
        var (radio, rig) = await ConnectAsync();
        using var _ = rig;

        radio.OperatorTurnsASwitch(NoiseBlanker, 1);

        var morse = NeighborhoodPlan
            .ForBand(HfBands.Bands.First(b => b.Name == "20 m"))
            .First(n => n.Family == ModeFamily.Cw);

        _output.WriteLine($"  the dial is in {morse.Name}");

        var (results, memory) = await ReceiverSetup.ApplyAsync(
            rig, ReceiverConditions.ForBlock(morse), ReceiverSetupMemory.Empty);

        Assert.Empty(results);
        Assert.Empty(radio.SwitchWrites);
        Assert.Equal(0, radio.SwitchReads);
        Assert.Equal(1, radio.Switches[NoiseBlanker]);
        Assert.Empty(memory.LastSet);
    }
}
