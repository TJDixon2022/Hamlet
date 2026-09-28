using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Work instruction 419 task 2 item 4, criterion 7.5: a value the operator sets
/// himself is not overwritten by a later tune-in of the same mode (HM-DEC-056).
/// </summary>
/// <remarks>
/// <para>**WHAT TIM SAW.** He set the preamp off by hand and Hamlet turned it back
/// on. Since R98 (work instruction 486, HM-DEC-191) the CW row wants it off, so the
/// preamp cases here have his hand on instead; the rule is the same. Task 1's table printed how: a preamp Hamlet had to write was remembered, so
/// his change stood, but a preamp the first tune-in found already right was not,
/// because the memory held only writes. With nothing remembered there was nothing
/// for his change to disagree with, and the next tune-in wrote it back.</para>
/// <para>**THE RULE IMPLEMENTED.** A tune-in remembers every field it leaves at the
/// mode's value, whether it wrote it or found it there. A later tune-in that reads
/// something else has found somebody's hand and leaves it. **It holds until the band
/// changes**, which re-arms the memory exactly as it re-arms mode-follow
/// (HM-DEC-056); the last fact holds that half.</para>
/// </remarks>
public sealed class TheOperatorsHandStandsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tune-ins are printed.</param>
    public TheOperatorsHandStandsTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// The preamp found already off or written off, then switched on by hand: the next
    /// CW tune-in leaves it on.
    /// </summary>
    /// <param name="startedAt">Where the preamp was before the first tune-in.</param>
    /// <remarks>
    /// **R98 (WORK INSTRUCTION 486, HM-DEC-191) TURNED THE CASE ROUND.** The CW row
    /// used to want preamp 1 (HM-DEC-177), so his hand was off; the owner has ruled
    /// it off for Morse, so his hand is now on. The rule under test is unchanged
    /// (HM-DEC-056): a preamp the first tune-in found at off is remembered as well
    /// as one it wrote to off, and his change stands either way.
    /// </remarks>
    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    public async Task ThePreampHeSetOnStaysOn(byte startedAt)
    {
        var radio = ModeEntryBench.AsLeft(14_050_000, data: false);
        radio.Switches[ModeEntryBench.Preamp] = startedAt;
        using var rig = await ModeEntryBench.ConnectAsync(radio);

        var cw = ReceiverConditions.ForMode("CW");
        var (_, memory) = await ReceiverSetup.ApplyAsync(rig, cw, ReceiverSetupMemory.Empty);

        Assert.Equal(0, radio.Switches[ModeEntryBench.Preamp]);

        radio.OperatorTurnsASwitch(ModeEntryBench.Preamp, 1);
        ModeEntryBench.ClearWrites(radio);

        var (second, _) = await ReceiverSetup.ApplyAsync(rig, cw, memory);
        var preamp = second.Single(r => r.Condition.Field == RigField.Preamp);

        _output.WriteLine(
            $"preamp {startedAt} at first; on by hand; second tune-in {preamp.Outcome}, "
            + $"radio now {radio.Switches[ModeEntryBench.Preamp]}");

        Assert.Equal(ConditionOutcome.LeftToTheOperator, preamp.Outcome);
        Assert.Equal(1, radio.Switches[ModeEntryBench.Preamp]);
        Assert.DoesNotContain(ModeEntryBench.Writes(radio), w => w.Field == RigField.Preamp);
    }

    /// <summary>
    /// The RF gain found at full, then backed off by hand: the next CW tune-in leaves
    /// it where he put it.
    /// </summary>
    [Fact]
    public async Task TheGainHeBackedOffStaysBackedOff()
    {
        var radio = ModeEntryBench.AsLeft(14_050_000, data: false);
        using var rig = await ModeEntryBench.ConnectAsync(radio);

        var cw = ReceiverConditions.ForMode("CW");
        var (_, memory) = await ReceiverSetup.ApplyAsync(rig, cw, ReceiverSetupMemory.Empty);

        radio.Levels[ModeEntryBench.RfGain] = 108;
        ModeEntryBench.ClearWrites(radio);

        var (second, _) = await ReceiverSetup.ApplyAsync(rig, cw, memory);

        Assert.Equal(
            ConditionOutcome.LeftToTheOperator,
            second.Single(r => r.Condition.Field == RigField.RfGain).Outcome);
        Assert.Empty(radio.LevelWrites);
        Assert.Equal(108, radio.Levels[ModeEntryBench.RfGain]);
    }

    /// <summary>
    /// **HOW LONG IT HOLDS.** A band change re-arms the memory, and the next tune-in
    /// sets the mode's value again.
    /// </summary>
    /// <remarks>
    /// Since R98 (work instruction 486, HM-DEC-191) the CW row's preamp is off, so
    /// his hand is on and the re-armed tune-in writes it back to off.
    /// </remarks>
    [Fact]
    public async Task ABandChangeReArmsIt()
    {
        var radio = ModeEntryBench.AsLeft(14_050_000, data: false);
        radio.Switches[ModeEntryBench.Preamp] = 1;
        using var rig = await ModeEntryBench.ConnectAsync(radio);

        var cw = ReceiverConditions.ForMode("CW");
        var (_, memory) = await ReceiverSetup.ApplyAsync(rig, cw, ReceiverSetupMemory.Empty);

        radio.OperatorTurnsASwitch(ModeEntryBench.Preamp, 1);

        var (second, _) = await ReceiverSetup.ApplyAsync(rig, cw, memory.Rearmed());

        Assert.Equal(
            ConditionOutcome.Changed,
            second.Single(r => r.Condition.Field == RigField.Preamp).Outcome);
        Assert.Equal(0, radio.Switches[ModeEntryBench.Preamp]);
    }
}
