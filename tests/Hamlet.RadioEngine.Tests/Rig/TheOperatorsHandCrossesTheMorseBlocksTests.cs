using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Work instruction 420 task 2 item 5: the operator's hand across the blocks of the
/// CW family on one band (HM-DEC-056, HM-DEC-174, R70).
/// </summary>
/// <remarks>
/// <para>**THE AUTHOR'S RULING: THE THREE ARE ONE MODE.** Stepping from a `CW` block
/// to a `QRP` block on the same band is not a new mode, so a change he made by hand
/// stands until the band changes, as HM-DEC-056 already says within one block.</para>
/// <para>**WHAT THE CODE KEYS IT BY, READ BEFORE THIS WAS WRITTEN.** Neither the
/// short name nor the mode: <see cref="ReceiverSetupMemory"/> is keyed by field alone,
/// and <c>MainWindowViewModel</c> carries one memory across every tune-in and re-arms
/// it only in <c>OnSelectedBandChanged</c>. The per-block guard,
/// <c>_conditionsSetForBlockHz</c>, decides only that a block gets one tune-in. So the
/// ruling needs no change to the rule, and these carry the memory from one block's
/// tune-in into the next exactly as the view model does.</para>
/// <para>**IT COULD NOT BE WATCHED FAILING.** Before R70 the `QRP` block stated
/// nothing, so the second tune-in wrote nothing and his hand stood by default; after
/// it, the memory keeps it standing. Every result is an indication against
/// <see cref="ScriptedRadio"/> (FACT-006).</para>
/// </remarks>
public sealed class TheOperatorsHandCrossesTheMorseBlocksTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the two tune-ins are printed.</param>
    public TheOperatorsHandCrossesTheMorseBlocksTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// A tune-in in the `CW` block, his own preamp, then a step into the `QRP` block
    /// on the same band: the second tune-in leaves his preamp where he put it.
    /// </summary>
    /// <param name="fromHz">The `CW` block's dial.</param>
    /// <param name="toHz">The `QRP` block's dial, the same band.</param>
    /// <param name="byHand">What he sets the preamp to after the first tune-in.</param>
    /// <remarks>
    /// On both bands the CW row's preamp is 1 since HM-DEC-177 (the manual, page 4-3),
    /// so his hand is off, which is the instruction's case. Work instruction 420 had
    /// his hand at preamp 1 on 40 m, where the old row said off.
    /// </remarks>
    [Theory]
    [InlineData(7_025_000L, 7_030_000L, (byte)0)]
    [InlineData(14_050_000L, 14_060_000L, (byte)0)]
    public async Task HisPreampStandsFromTheCwBlockIntoTheQrpBlock(
        long fromHz, long toHz, byte byHand)
    {
        var from = ModeEntryBench.BlockAt(fromHz)!;
        var to = ModeEntryBench.BlockAt(toHz)!;

        Assert.Equal("CW", from.ShortName);
        Assert.Equal("QRP", to.ShortName);

        var radio = ModeEntryBench.AsLeftWithThePreampOn(fromHz);
        using var rig = await ModeEntryBench.ConnectAsync(radio);

        var (first, memory) = await ReceiverSetup.ApplyAsync(
            rig, ReceiverConditions.ForBlock(from), ReceiverSetupMemory.Empty);

        radio.OperatorTurnsASwitch(ModeEntryBench.Preamp, byHand);
        radio.FrequencyHz = toHz;
        ModeEntryBench.ClearWrites(radio);

        var (second, _) = await ReceiverSetup.ApplyAsync(
            rig, ReceiverConditions.ForBlock(to), memory);

        var preampFirst = first.Single(r => r.Condition.Field == RigField.Preamp);
        var preampSecond = second.Single(r => r.Condition.Field == RigField.Preamp);

        _output.WriteLine(
            $"{from.Name} at {fromHz / 1e6:0.000}: preamp {preampFirst.Outcome}, now "
            + $"{preampFirst.NowText}; he sets it to {byHand}; {to.Name} at "
            + $"{toHz / 1e6:0.000}: preamp {preampSecond.Outcome}, radio now "
            + $"{radio.Switches[ModeEntryBench.Preamp]}, writes "
            + $"{string.Join(",", ModeEntryBench.Writes(radio).Select(w => $"{w.Field}={w.Value}"))}");

        Assert.Equal(ConditionOutcome.LeftToTheOperator, preampSecond.Outcome);
        Assert.Equal(byHand, radio.Switches[ModeEntryBench.Preamp]);
        Assert.DoesNotContain(ModeEntryBench.Writes(radio), w => w.Field == RigField.Preamp);
    }

    /// <summary>
    /// **AND A BAND CHANGE STILL RE-ARMS IT.** The same step with the memory re-armed,
    /// as <c>OnSelectedBandChanged</c> does, sets the CW row's value again.
    /// </summary>
    [Fact]
    public async Task ABandChangeStillReArmsIt()
    {
        var radio = ModeEntryBench.AsLeftWithThePreampOn(7_025_000);
        using var rig = await ModeEntryBench.ConnectAsync(radio);

        var (_, memory) = await ReceiverSetup.ApplyAsync(
            rig, ReceiverConditions.ForBlock(ModeEntryBench.BlockAt(7_025_000)), ReceiverSetupMemory.Empty);

        // His hand is off, against the row's preamp 1 on 40 m since HM-DEC-177.
        radio.OperatorTurnsASwitch(ModeEntryBench.Preamp, 0);
        radio.FrequencyHz = 7_030_000;

        var (second, _) = await ReceiverSetup.ApplyAsync(
            rig, ReceiverConditions.ForBlock(ModeEntryBench.BlockAt(7_030_000)), memory.Rearmed());

        Assert.Equal(
            ConditionOutcome.Changed,
            second.Single(r => r.Condition.Field == RigField.Preamp).Outcome);
        Assert.Equal(1, radio.Switches[ModeEntryBench.Preamp]);
    }
}
