using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Work instruction 419 task 2 item 3, criteria 7.5 and 6.8: a value the radio
/// already holds is not written, and a write is judged on the scale its read-back
/// comes back on.
/// </summary>
/// <remarks>
/// <para>**UNIT 411'S 255 AGAINST 100** (R65, HM-DEC-172). The CW row asks for the
/// RF gain at 255 on the radio's own scale and the read decodes it as a percent,
/// so the two never compared equal: every CW tune-in wrote 255 to a radio already
/// at full, the rig filed the read-back of exactly that as disagreement, and the
/// setup filed it NotConfirmed. Task 1's table counted one field written on a CW
/// tune-in when the radio was already right, at both frequencies.</para>
/// <para>**ONE SCALE: THE READ'S.** The wanted value is put on the scale the field
/// is read on before it is compared, which is the scale the memory and the operator
/// see. Nothing that keys, transmits or sets power is touched: the rig's
/// comparison moves only for receive-tier writes.</para>
/// </remarks>
public sealed class AValueAlreadyRightIsNotWrittenTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the writes are printed.</param>
    public AValueAlreadyRightIsNotWrittenTests(ITestOutputHelper output) => _output = output;

    /// <summary>RF gain already at full: no byte is sent and it is filed already right.</summary>
    [Fact]
    public async Task TheRfGainAtFullIsNotWritten()
    {
        var (writes, outcome) = await RfGainTuneInAsync(255);

        Assert.Empty(writes);
        Assert.Equal(ConditionOutcome.AlreadyRight, outcome);
    }

    /// <summary>RF gain at 42 percent: written once, and the read-back confirms it.</summary>
    [Fact]
    public async Task TheRfGainBelowFullIsWrittenAndConfirmed()
    {
        var (writes, outcome) = await RfGainTuneInAsync(108);

        Assert.Equal(new[] { 255 }, writes);
        Assert.Equal(ConditionOutcome.Changed, outcome);
    }

    /// <summary>
    /// **THE UNIT'S NUMBER.** A radio already at every value the CW row asks for:
    /// a tune-in writes nothing.
    /// </summary>
    /// <param name="hz">Where the dial is.</param>
    [Theory]
    [InlineData(14_050_000)]
    [InlineData(7_030_000)]
    public async Task ACwTuneInOnARadioAlreadyRightWritesNothing(long hz)
    {
        var radio = ModeEntryBench.AlreadyRightForCw(hz);
        using var rig = await ModeEntryBench.ConnectAsync(radio);

        var (results, _) = await ReceiverSetup.ApplyAsync(
            rig, ReceiverConditions.ForMode("CW"), ReceiverSetupMemory.Empty);

        var writes = ModeEntryBench.Writes(radio);

        foreach (var r in results)
        {
            _output.WriteLine($"  {r.Condition.Control,-16} {r.Outcome,-14} was={r.WasText}");
        }

        _output.WriteLine($"fields written: {writes.Select(w => w.Field).Distinct().Count()}");

        Assert.Empty(writes);
        Assert.All(results, r => Assert.Equal(ConditionOutcome.AlreadyRight, r.Outcome));
    }

    /// <summary>The rig confirms a receive level on the scale it reads it back on.</summary>
    [Fact]
    public async Task TheRigConfirmsALevelOnTheScaleItReads()
    {
        var radio = ModeEntryBench.AsLeft(14_050_000, data: false);
        radio.Levels[ModeEntryBench.RfGain] = 108;
        using var rig = await ModeEntryBench.ConnectAsync(radio);

        var result = await rig.SetSettingAsync(CivWrites.RfGain, 255);

        _output.WriteLine($"outcome {result.Outcome}, read back {result.ReadBack?.Text ?? "-"}");

        Assert.True(result.Worked);
    }

    private async Task<(int[] Writes, ConditionOutcome Outcome)> RfGainTuneInAsync(int raw)
    {
        var radio = ModeEntryBench.AsLeft(14_050_000, data: false);
        radio.Levels[ModeEntryBench.RfGain] = raw;
        using var rig = await ModeEntryBench.ConnectAsync(radio);

        var (results, _) = await ReceiverSetup.ApplyAsync(
            rig,
            ReceiverConditions.ForMode("CW").Where(c => c.Field == RigField.RfGain).ToList(),
            ReceiverSetupMemory.Empty);

        var writes = radio.LevelWrites.Select(w => w.Value).ToArray();
        var outcome = results.Single().Outcome;

        _output.WriteLine(
            $"radio at raw {raw}: wrote [{string.Join(",", writes)}], filed {outcome}, "
            + $"was {results.Single().WasText}, now {results.Single().NowText}");

        return (writes, outcome);
    }
}
