using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Work instruction 480 task 1, criterion 12.4: in CW Hamlet turns the IC-7300's scope
/// output on as a receive condition, reads it back, and the scope stream listens once
/// the read-back says on (R94, HM-DEC-188).
/// </summary>
/// <remarks>
/// <para>**WHAT WAS WRONG.** Every capture sheet read "ScopeOn on, ScopeOutput off".
/// The tree parsed the radio's scope stream and nothing in CW mode asked the radio to
/// send it, so the detector had only its own sweep.</para>
/// <para>**EACH CASE DOES WHAT THE APP DOES**: the block's own conditions handed to the
/// real <see cref="ReceiverSetup"/> over a real <see cref="Ic7300Rig"/>, against
/// <see cref="ScriptedRadio"/>, and the tune-in's results handed to the real
/// <see cref="RigSpectrumSource"/>. Every result is an indication against the scripted
/// radio (FACT-006), not a measurement of the IC-7300.</para>
/// </remarks>
public sealed class TheScopeOutputIsACwConditionTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each tune-in's outcome is printed.</param>
    public TheScopeOutputIsACwConditionTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **A CW TUNE-IN WITH THE SCOPE OUTPUT OFF WRITES `27 11 01`, READS IT BACK ON,
    /// AND THE STREAM IS LISTENING.**
    /// </summary>
    [Fact]
    public async Task ACwTuneInTurnsTheScopeOutputOnAndTheStreamListens()
    {
        var radio = ModeEntryBench.AsLeftWithThePreampOn(14_050_000);
        radio.ScopeOutput = 0;
        using var rig = await ModeEntryBench.ConnectAsync(radio);
        using var stream = new RigSpectrumSource(rig);

        var (results, _) = await ReceiverSetup.ApplyAsync(
            rig, ReceiverConditions.ForBlock(ModeEntryBench.BlockAt(14_050_000)),
            ReceiverSetupMemory.Empty);

        var scope = results.FirstOrDefault(r => r.Condition.Field == RigField.ScopeOutput);
        var listening = stream.FollowTheSetup(results);

        _output.WriteLine(
            $"scope output: {scope?.Outcome.ToString() ?? "not stated"}, was {scope?.WasText ?? "-"}, "
            + $"now {scope?.NowText ?? "-"}; writes {string.Join(",", radio.ScopeOutputWrites)}; "
            + $"radio holds {radio.ScopeOutput}; stream running {stream.IsRunning}");

        Assert.True(scope is not null, "the CW block states no scope output, so nothing asks the radio to send its spectrum");
        Assert.Equal(new byte[] { 1 }, radio.ScopeOutputWrites);
        Assert.Equal(ConditionOutcome.Changed, scope!.Outcome);
        Assert.Equal("on", scope.NowText);
        Assert.True(listening);
        Assert.True(stream.IsRunning);
    }

    /// <summary>
    /// **HIS HAND WINS** (HM-DEC-056): turned off by hand after Hamlet set it on, the
    /// next tune-in of the same mode leaves it off and writes nothing, and the stream is
    /// not started on a read-back that says off.
    /// </summary>
    [Fact]
    public async Task TheOperatorsOffStandsAtTheNextTuneIn()
    {
        var radio = ModeEntryBench.AsLeftWithThePreampOn(14_050_000);
        radio.ScopeOutput = 0;
        using var rig = await ModeEntryBench.ConnectAsync(radio);
        var block = ReceiverConditions.ForBlock(ModeEntryBench.BlockAt(14_050_000));

        var (_, memory) = await ReceiverSetup.ApplyAsync(rig, block, ReceiverSetupMemory.Empty);
        radio.ScopeOutput = 0;
        ModeEntryBench.ClearWrites(radio);

        var (again, _) = await ReceiverSetup.ApplyAsync(rig, block, memory);
        var scope = again.Single(r => r.Condition.Field == RigField.ScopeOutput);
        using var stream = new RigSpectrumSource(rig);

        _output.WriteLine($"second tune-in: {scope.Outcome}, writes {radio.ScopeOutputWrites.Count}");

        Assert.Equal(ConditionOutcome.LeftToTheOperator, scope.Outcome);
        Assert.Empty(radio.ScopeOutputWrites);
        Assert.False(stream.FollowTheSetup(again));
        Assert.False(stream.IsRunning);
    }

    /// <summary>
    /// **DATA MODES ARE UNTOUCHED**: an FT8 tune-in with the scope output off states
    /// nothing about it and writes nothing.
    /// </summary>
    [Fact]
    public async Task AnFt8TuneInLeavesTheScopeOutputAlone()
    {
        var radio = ModeEntryBench.AsLeftWithThePreampOn(14_074_000);
        radio.ScopeOutput = 0;
        using var rig = await ModeEntryBench.ConnectAsync(radio);

        var (results, _) = await ReceiverSetup.ApplyAsync(
            rig, ReceiverConditions.ForBlock(ModeEntryBench.BlockAt(14_074_000)),
            ReceiverSetupMemory.Empty);

        Assert.DoesNotContain(results, r => r.Condition.Field == RigField.ScopeOutput);
        Assert.Empty(radio.ScopeOutputWrites);
        Assert.Equal((byte)0, radio.ScopeOutput);
    }
}
