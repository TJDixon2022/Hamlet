using HamManager.RadioEngine.Rig;
using Xunit;

namespace HamManager.RadioEngine.Tests.Rig;

public sealed class FakeRigTests
{
    /// <remarks>
    /// Proves: a set frequency reads back exactly — the engine's most basic
    /// contract, and the smoke test that the solution wiring works at all.
    /// </remarks>
    [Fact]
    public async Task SetFrequency_ReadsBackExactly()
    {
        var rig = new FakeRig();
        await rig.ConnectAsync();

        await rig.SetFrequencyHzAsync(14_074_000);

        Assert.Equal(14_074_000, await rig.GetFrequencyHzAsync());
    }

    /// <remarks>
    /// Proves: an unsolicited frequency change (the operator's VFO knob,
    /// which the IC-7300 reports over CI-V) raises FrequencyChanged with the
    /// new value. The UI's echo-suppression logic will depend on this event
    /// firing for knob turns.
    /// </remarks>
    [Fact]
    public void KnobTurn_RaisesFrequencyChanged()
    {
        var rig = new FakeRig(initialFrequencyHz: 7_030_000);
        long? reported = null;
        rig.FrequencyChanged += (_, e) => reported = e.FrequencyHz;

        rig.SimulateKnobTurn(7_040_000);

        Assert.Equal(7_040_000, reported);
    }

    /// <remarks>
    /// Proves: connect/disconnect round-trips IsConnected, and disconnect is
    /// safe when never connected — the "unreachable rig is a condition, not
    /// an exception" rule in IRig's contract.
    /// </remarks>
    [Fact]
    public async Task ConnectDisconnect_RoundTripsAndNeverThrows()
    {
        var rig = new FakeRig();
        await rig.DisconnectAsync();
        Assert.False(rig.IsConnected);

        await rig.ConnectAsync();
        Assert.True(rig.IsConnected);

        await rig.DisconnectAsync();
        Assert.False(rig.IsConnected);
    }
}
