using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Work instruction 419 task 2 item 2, criterion 7.5: exactly one component decides
/// each field, and no other asks the operator to change a field the setup owns.
/// </summary>
/// <remarks>
/// <para>**TIM HEARD THREE VOICES ON ONE KNOB.** The setup writes the preamp on a
/// tune-in, the advice asks him to switch it on whenever it reads off, and the
/// observations object when it is on beside the attenuator. Task 1's table
/// printed the advice asking for the preamp on at 7.030 right after the CW row
/// left it off, and for AGC fast on FT8 where the row states slow.</para>
/// <para>**THE VOICES ARE SCOPED, NOT DELETED.** With no tune-in behind them they
/// say exactly what they said before, and the last two facts hold that.</para>
/// </remarks>
public sealed class OneVoicePerFieldTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the voices are printed.</param>
    public OneVoicePerFieldTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// After a tune-in, the advice proposes no change to any field the mode states.
    /// </summary>
    /// <param name="hz">Where the dial is.</param>
    /// <param name="mode">The mode's row.</param>
    [Theory]
    [InlineData(7_030_000, "CW")]
    [InlineData(14_050_000, "CW")]
    [InlineData(14_074_000, "FT8")]
    public async Task TheAdviceLeavesTheModesFieldsAlone(long hz, string mode)
    {
        var (state, owned) = await TuneInAsync(hz, mode);

        var advice = ReceiveAdvice.For(state, owned);

        foreach (var a in advice)
        {
            _output.WriteLine($"  {a.Write.Field,-16} would change {a.WouldChange,-5} {a.Says}");
        }

        Assert.DoesNotContain(advice, a => owned.Contains(a.Write.Field) && a.WouldChange);
    }

    /// <summary>
    /// The observations do not object to the preamp and attenuator the setup set
    /// together.
    /// </summary>
    [Fact]
    public void TheObservationsDoNotObjectToWhatTheSetupOwns()
    {
        var owned = new HashSet<RigField> { RigField.Attenuator, RigField.Preamp };

        var said = RigObservations.For(BothOn(), owned);

        foreach (var line in said)
        {
            _output.WriteLine($"  {line}");
        }

        Assert.DoesNotContain(said, s => s.Contains("attenuator", StringComparison.Ordinal));
    }

    /// <summary>With no tune-in behind it, the advice still asks for the preamp on.</summary>
    [Fact]
    public void WithNoTuneInTheAdviceStillSpeaks()
    {
        var off = RigState.Empty.With(RigValue.Known(
            RigField.Preamp, 0, "off", DateTime.UtcNow, "test"));

        Assert.Contains(
            ReceiveAdvice.For(off),
            a => a.Write.Field == RigField.Preamp && a.WouldChange);
    }

    /// <summary>With no tune-in behind it, the observation still speaks.</summary>
    [Fact]
    public void WithNoTuneInTheObservationStillSpeaks()
    {
        Assert.Contains(
            RigObservations.For(BothOn()),
            s => s.Contains("attenuator", StringComparison.Ordinal));
    }

    private static RigState BothOn()
        => RigState.Empty
            .With(RigValue.Known(RigField.Attenuator, 20, "20 dB", DateTime.UtcNow, "test"))
            .With(RigValue.Known(RigField.Preamp, 1, "preamp 1", DateTime.UtcNow, "test"));

    private static async Task<(RigState State, IReadOnlySet<RigField> Owned)> TuneInAsync(
        long hz, string mode)
    {
        var radio = ModeEntryBench.AsLeft(hz, data: mode != "CW");
        using var rig = await ModeEntryBench.ConnectAsync(radio);

        var (results, _) = await ReceiverSetup.ApplyAsync(
            rig, ReceiverConditions.ForMode(mode), ReceiverSetupMemory.Empty);

        return (await ModeEntryBench.ReadAllAsync(rig), ReceiverSetup.Owns(results));
    }
}
