namespace Hamlet.RadioEngine.Rig;

/// <summary>
/// The training radio: an <see cref="IRig"/> with no hardware behind it.
/// </summary>
/// <remarks>
/// <para>This began as a test double and is now a product feature
/// (HM-DEC-026). Somebody licensed for years who still cannot tell one
/// signal from another needs to practise, and practising on the air means
/// owning a radio, having an antenna up, and hoping the band is open. Here
/// they can learn the waterfall and the sound of each mode with nothing
/// plugged in.</para>
/// <para>It still backs UI development and engine tests, and it still stands
/// in for the IC-7300 until CI-V lands. What changed is that it is now
/// something the operator chooses on purpose, so it says so in the port list
/// and <see cref="IsSimulated"/> makes the app say so on screen.</para>
/// </remarks>
public sealed class TrainingRig : IRig
{
    private long _frequencyHz;

    /// <summary>Starts in the 40 m CW segment — a deliberate nod to phase 1.</summary>
    public TrainingRig(long initialFrequencyHz = 7_030_000)
    {
        _frequencyHz = initialFrequencyHz;
    }

    /// <inheritdoc/>
    public bool IsConnected { get; private set; }

    /// <inheritdoc/>
    /// <remarks>Always true, with no way to say otherwise. See HM-DEC-026.</remarks>
    public bool IsSimulated => true;

    /// <inheritdoc/>
    /// <remarks>
    /// The training radio claims a scope because it genuinely has one — the
    /// synthesiser — and refuses transmit because it genuinely cannot: there
    /// is no transmitter behind it, and saying otherwise would be the one
    /// claim this class must never make.
    /// </remarks>
    public RigCapabilities Capabilities { get; } = new(
        "Training radio",
        HasSpectrumScope: true,
        HasBuiltInCwKeyer: false,
        HasUsbAudio: false,
        CanTransmit: false,
        Bands.BandPlan.Bands.Select(b => b.Name).ToList());

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
