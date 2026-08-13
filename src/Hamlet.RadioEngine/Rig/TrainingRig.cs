namespace Hamlet.RadioEngine.Rig;

/// <summary>
/// An <see cref="IRig"/> with no hardware behind it. Backs UI development
/// and engine tests when no radio is attached, and stands in for the
/// IC-7300 until the CI-V implementation lands.
/// </summary>
public sealed class FakeRig : IRig
{
    private long _frequencyHz;

    /// <summary>Starts in the 40 m CW segment — a deliberate nod to phase 1.</summary>
    public FakeRig(long initialFrequencyHz = 7_030_000)
    {
        _frequencyHz = initialFrequencyHz;
    }

    /// <inheritdoc/>
    public bool IsConnected { get; private set; }

    /// <inheritdoc/>
    public event EventHandler<FrequencyChangedEventArgs>? FrequencyChanged;

    /// <inheritdoc/>
    public Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        IsConnected = true;
        return Task.FromResult(true);
    }

    /// <inheritdoc/>
    public Task DisconnectAsync()
    {
        IsConnected = false;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<long> GetFrequencyHzAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Interlocked.Read(ref _frequencyHz));

    /// <inheritdoc/>
    public Task SetFrequencyHzAsync(long frequencyHz, CancellationToken cancellationToken = default)
    {
        Interlocked.Exchange(ref _frequencyHz, frequencyHz);
        FrequencyChanged?.Invoke(this, new FrequencyChangedEventArgs(frequencyHz));
        return Task.CompletedTask;
    }

    /// <summary>
    /// Test and demo hook: simulate the operator spinning the physical VFO
    /// knob, which the IC-7300 reports unsolicited over CI-V.
    /// </summary>
    public void SimulateKnobTurn(long newFrequencyHz)
    {
        Interlocked.Exchange(ref _frequencyHz, newFrequencyHz);
        FrequencyChanged?.Invoke(this, new FrequencyChangedEventArgs(newFrequencyHz));
    }
}
