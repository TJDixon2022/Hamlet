namespace Hamlet.RadioEngine.Rig;

/// <summary>
/// The training radio: an <see cref="IRig"/> with no hardware behind it.
/// </summary>
/// <remarks>
/// <para>This began as a test double and is now a product feature
/// (HM-DEC-026). Somebody licensed for years who still cannot tell one
/// signal from another needs to practice, and practicing on the air means
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
    /// <remarks>
    /// Declared and never raised, which the compiler is right to notice. The
    /// training radio has no front panel for anybody to touch, so there is
    /// nothing for it to volunteer, and pretending otherwise by inventing
    /// changes would put synthetic readings on screen (§0.0).
    /// </remarks>
#pragma warning disable CS0067
    public event EventHandler<RigValuesReportedEventArgs>? ValuesReported;
#pragma warning restore CS0067

    /// <inheritdoc/>
    /// <remarks>
    /// <para>DEGRADES HONESTLY, which is the whole of HM-DEC-030. The training
    /// radio has no receiver, so it has no AGC to be fast or slow, no preamp to
    /// be on, and no S-meter to read. Every one of those comes back
    /// <see cref="RigValueState.Unsupported"/>, which the diagnostics screen
    /// shows as "this radio does not have it" rather than leaving somebody
    /// waiting for a number that is never coming.</para>
    /// <para>It would be easy, and wrong, to invent plausible values here so the
    /// screen looked full. A synthesized S-meter reading is a measurement of
    /// nothing presented as a measurement of something, which is the prime
    /// directive broken for the sake of a tidier demonstration (§0.0). The two
    /// fields it does answer are the two it genuinely models: the frequency it
    /// is tuned to, and CW, which is the mode it synthesizes.</para>
    /// </remarks>
    public Task<IReadOnlyList<RigValue>> ReadAsync(
        RigField field, RigState context, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        const string source = "training radio";

        IReadOnlyList<RigValue> answer = field switch
        {
            RigField.Frequency => new[]
            {
                RigValue.Known(
                    field, Interlocked.Read(ref _frequencyHz),
                    Ic7300Rig.FrequencyText(Interlocked.Read(ref _frequencyHz)), now, source),
            },

            // It synthesizes Morse and nothing else, so this is a fact about it
            // rather than a stand-in for a radio it is pretending to be.
            RigField.Mode => new[]
            {
                RigValue.Known(field, (int)Civ.CivMode.Cw, "CW", now, source),
            },

            _ => new[]
            {
                RigValue.Unsupported(field, "training radio: no receiver"),
            },
        };

        return Task.FromResult(answer);
    }

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
