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
        Bands.HfBands.Bands.Select(b => b.Name).ToList());

    /// <inheritdoc/>
    public event EventHandler<FrequencyChangedEventArgs>? FrequencyChanged;

    /// <inheritdoc/>
    /// <remarks>
    /// Raised for the one field this radio genuinely models, which is the
    /// frequency it is tuned to. It has no front panel for anybody to touch and
    /// no receiver, so nothing else is ever volunteered, and inventing changes
    /// to fill the screen out would put synthetic readings where measurements
    /// belong (§0.0).
    /// </remarks>
    public event EventHandler<RigValuesReportedEventArgs>? ValuesReported;

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
                    Civ.CivValues.FrequencyText(Interlocked.Read(ref _frequencyHz)), now, source),
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
    /// <remarks>
    /// DEGRADES HONESTLY, and refuses rather than pretending (HM-DEC-030,
    /// HM-DEC-056). The training radio synthesizes Morse and nothing else, so a
    /// mode it cannot be in is not a mode it can be set to, and answering
    /// "confirmed" would put a mode on the badge that nothing behind it is
    /// producing.
    /// </remarks>
    /// <inheritdoc/>
    /// <remarks>
    /// The training radio has no settings to move, so it says so rather than
    /// pretending a write took (HM-DEC-026, HM-DEC-084). A practice radio that
    /// reported success would teach the one lesson this app must never teach.
    /// </remarks>
    public Task<RigWriteResult> SetSettingAsync(
        Civ.CivWrite write, int value, CancellationToken cancellationToken = default)
        => Task.FromResult(RigWriteResult.NotSupported(
            "the training radio has no settings to change"));

    /// <inheritdoc/>
    public Task<RigWriteResult> SetModeAsync(
        Civ.CivMode mode, bool dataMode, byte? filterSlot = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult(mode == Civ.CivMode.Cw && !dataMode
            ? RigWriteResult.Confirmed("training radio")
            : RigWriteResult.NotSupported(
                "training radio: it only makes Morse, so it cannot be put in "
                + "another mode"));

    /// <inheritdoc/>
    /// <remarks>
    /// THE TRAINING RADIO HAS NO TRANSMITTER AND SAYS SO (HM-DEC-030,
    /// HM-DEC-026). It reports <c>CanTransmit: false</c>, so nothing above it
    /// ever reaches here, and if something did it would refuse rather than
    /// pretend: a practice radio that answered "sent" would teach somebody that
    /// their first call went out when nothing left the house.
    /// </remarks>
    public Task<bool> SendCwAsync(string message, CancellationToken cancellationToken = default)
        => Task.FromResult(false);

    /// <inheritdoc/>
    /// <remarks>Nothing is ever keyed here, so there is nothing to stop.</remarks>
    public void AbortCw()
    {
    }

    /// <inheritdoc/>
    public Task<long> GetFrequencyHzAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Interlocked.Read(ref _frequencyHz));

    /// <inheritdoc/>
    public Task SetFrequencyHzAsync(long frequencyHz, CancellationToken cancellationToken = default)
    {
        Interlocked.Exchange(ref _frequencyHz, frequencyHz);
        Announce(frequencyHz);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Test and demo hook: simulate the operator spinning the physical VFO
    /// knob, which the IC-7300 reports unsolicited over CI-V.
    /// </summary>
    public void SimulateKnobTurn(long newFrequencyHz)
    {
        Interlocked.Exchange(ref _frequencyHz, newFrequencyHz);
        Announce(newFrequencyHz);
    }

    /// <summary>
    /// Tell everybody where this radio is now tuned.
    /// </summary>
    /// <remarks>
    /// The state model hears this the same way it hears a real radio's
    /// transceive report, so the diagnostics screen and the display agree about
    /// the frequency instead of the screen showing whatever the last sweep
    /// happened to catch.
    /// </remarks>
    private void Announce(long frequencyHz)
    {
        FrequencyChanged?.Invoke(this, new FrequencyChangedEventArgs(frequencyHz));

        ValuesReported?.Invoke(this, new RigValuesReportedEventArgs(new[]
        {
            RigValue.Known(
                RigField.Frequency, frequencyHz,
                Civ.CivValues.FrequencyText(frequencyHz), DateTime.UtcNow,
                "training radio"),
        }));
    }
}
