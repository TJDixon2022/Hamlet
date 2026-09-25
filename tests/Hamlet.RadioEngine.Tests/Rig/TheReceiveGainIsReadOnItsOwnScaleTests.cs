using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Work instruction 426 task 2, criterion 7.8's third clause for the RF gain: once he has
/// moved out of the CW block, Receive Help does not ask him to open a receive gain the CW
/// row left at full, and does not call full 39 percent.
/// </summary>
/// <remarks>
/// <para>**WHAT TASK 1 FOUND.** The RF gain is read as a percent (`CivDecode.DecodePercent`)
/// and `ReceiveAdvice.Gain` compared the reading to 240 on the 0 to 255 write scale, so a
/// gain at 100 percent read as *about 39 percent* and was asked to open all the way. It is
/// unit 411's scale mismatch (R65, HM-DEC-172), which was repaired in the setup and not in
/// the advice.</para>
/// <para>Read through Hamlet's own rig from <see cref="ScriptedRadio"/>, so the scale is the
/// read's and not one typed here (FACT-006).</para>
/// </remarks>
public sealed class TheReceiveGainIsReadOnItsOwnScaleTests
{
    /// <summary>
    /// A gain the CW row left at full, read back as 100 percent, is already open, with no
    /// tune-in owning it.
    /// </summary>
    [Fact]
    public async Task AGainAtFullIsAlreadyOpenOnceHeHasMovedOn()
    {
        var run = await LivePollBench.TuneInQuietAsync(7_030_000);
        using var rig = run.Rig;
        var state = await LivePollBench.PollAsync(run);

        Assert.Equal(100, state[RigField.RfGain].Number);

        var gain = ReceiveAdvice.For(state, new HashSet<RigField>())
            .Single(a => a.Write.Field == RigField.RfGain);

        Assert.True(gain.AlreadyRight, gain.Says);
        Assert.DoesNotContain("39 percent", gain.Says, StringComparison.Ordinal);
    }

    /// <summary>A gain turned half down is said as the percent the radio reads.</summary>
    [Fact]
    public async Task AGainTurnedDownIsSaidAsThePercentTheRadioReads()
    {
        var radio = ModeEntryBench.AsLeft(7_030_000, data: false);
        radio.Levels[ModeEntryBench.RfGain] = 128;
        using var rig = await ModeEntryBench.ConnectAsync(radio);
        var state = await ModeEntryBench.ReadAllAsync(rig);
        var percent = (int)state[RigField.RfGain].Number!.Value;

        var gain = ReceiveAdvice.For(state, new HashSet<RigField>())
            .Single(a => a.Write.Field == RigField.RfGain);

        Assert.True(gain.WouldChange, gain.Says);
        Assert.Contains($"about {percent} percent", gain.Says, StringComparison.Ordinal);
    }
}
