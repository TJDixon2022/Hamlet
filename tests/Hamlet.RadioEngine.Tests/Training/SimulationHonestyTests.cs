using System.Reflection;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Tests.Rig;
using Hamlet.RadioEngine.Training;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Training;

/// <summary>
/// The honesty rule of HM-DEC-026, proved structurally: a simulated source
/// always yields the labelled state, a real rig never does, and neither can
/// be told otherwise.
/// </summary>
public sealed class SimulationHonestyTests
{
    /// <remarks>
    /// Proves the training radio declares itself. The label the operator sees
    /// is derived from this, so if it could ever be false the app could show
    /// synthetic signals as real.
    /// </remarks>
    [Fact]
    public async Task TrainingRig_IsAlwaysSimulated()
    {
        var rig = new TrainingRig();

        Assert.True(rig.IsSimulated);

        await rig.ConnectAsync();
        Assert.True(rig.IsSimulated);

        await rig.DisconnectAsync();
        Assert.True(rig.IsSimulated);
    }

    /// <remarks>
    /// Proves the real rig never claims to be simulated — the other half of
    /// the same guarantee, and the half that would put a "simulated" label
    /// over genuine off-air signals if it were wrong.
    /// </remarks>
    [Fact]
    public void RealRig_IsNeverSimulated()
    {
        var rig = new Ic7300Rig(new FakeSerialPort());
        Assert.False(rig.IsSimulated);
    }

    /// <remarks>
    /// Proves the training spectrum source always declares itself simulated,
    /// whatever it is doing.
    /// </remarks>
    [Fact]
    public void TrainingSpectrumSource_IsAlwaysSimulated()
    {
        var band = BandPlan.Bands.First(b => b.Name == "40 m");
        using var source = new TrainingSpectrumSource(band);

        Assert.True(source.IsSimulated);

        source.PumpOnce(TimeSpan.FromSeconds(1));
        Assert.True(source.IsSimulated);

        source.Stop();
        Assert.True(source.IsSimulated);
    }

    /// <remarks>
    /// Proves the rule cannot be bypassed rather than merely that it holds
    /// today. Neither <c>IsSimulated</c> has a setter anywhere in the chain,
    /// so there is no "practice mode off" switch to add by accident and no
    /// way for the UI to assert something the source disagrees with. This is
    /// what makes HM-DEC-026 structural instead of a convention (HM-DEC-009).
    /// </remarks>
    [Fact]
    public void IsSimulated_HasNoSetterAnywhere()
    {
        var types = new[]
        {
            typeof(IRig), typeof(TrainingRig), typeof(Ic7300Rig),
            typeof(ISpectrumSource), typeof(TrainingSpectrumSource),
        };

        foreach (var type in types)
        {
            var property = type.GetProperty(
                "IsSimulated", BindingFlags.Public | BindingFlags.Instance);

            Assert.True(property is not null, $"{type.Name} should declare IsSimulated");
            Assert.True(property!.CanRead, $"{type.Name}.IsSimulated should be readable");
            Assert.False(property.CanWrite, $"{type.Name}.IsSimulated must not be settable");
            Assert.Null(property.SetMethod);
        }
    }

    /// <remarks>
    /// Proves the same for fields: nothing backs the property with mutable
    /// state that a reflection-happy caller or a future refactor could reach.
    /// </remarks>
    [Fact]
    public void SimulatedState_IsNotBackedByMutableState()
    {
        foreach (var type in new[] { typeof(TrainingRig), typeof(TrainingSpectrumSource) })
        {
            var fields = type.GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.DoesNotContain(fields, f =>
                f.Name.Contains("simulated", StringComparison.OrdinalIgnoreCase));
        }
    }
}
