namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// What the gate decided about one measurement, and everything it decided it
/// from.
/// </summary>
/// <param name="KeyDown">Whether the transmitter is judged to be keyed.</param>
/// <param name="PowerDb">The measured energy at the tracked pitch.</param>
/// <param name="NoiseFloorDb">Where the gate believes the noise sits.</param>
/// <param name="PeakDb">Where the gate believes a keyed signal sits.</param>
/// <param name="ThresholdDb">The level it is deciding against right now.</param>
/// <param name="HasSignal">
/// Whether there is enough separation between noise and peak for the decision
/// to mean anything at all.
/// </param>
public readonly record struct GateReading(
    bool KeyDown,
    double PowerDb,
    double NoiseFloorDb,
    double PeakDb,
    double ThresholdDb,
    bool HasSignal)
{
    /// <summary>How far the measurement stood above the noise, in decibels.</summary>
    public double SignalToNoiseDb => PowerDb - NoiseFloorDb;

    /// <summary>How far a keyed signal is standing above the noise, in decibels.</summary>
    public double SpreadDb => PeakDb - NoiseFloorDb;
}

/// <summary>
/// Decides whether the key is down, and keeps adapting so a signal that fades
/// does not quietly stop being decoded.
/// </summary>
/// <remarks>
/// <para>Two trackers, both in decibels, both asymmetric. The noise floor drops
/// quickly to meet a quieter band and creeps up over several seconds, so a
/// burst of static does not convince it the band got noisy. The peak rises
/// quickly and falls over a couple of seconds, so it follows a signal down
/// through a fade instead of leaving the threshold stranded above it. That
/// asymmetry is the whole design: the failure this prevents is a decoder that
/// works beautifully for thirty seconds and then goes silent without ever
/// saying why.</para>
/// <para>Decibels rather than raw power, because a fade is a multiplication and
/// a logarithm turns it into a subtraction. A tracker working in linear power
/// spends its whole life chasing the loud parts.</para>
/// <para>THE THRESHOLD IS PUBLISHED, not buried (§0.0.1). Every reading carries
/// the noise floor, the peak and the level being decided against, so a wrong
/// decode can be explained rather than argued about. A decoder that cannot say
/// what it was deciding from is a decoder nobody can debug.</para>
/// <para>Hysteresis, because a signal sitting exactly on the threshold would
/// otherwise chatter into a run of imaginary dits. It takes more to start a
/// mark than to continue one, which is what a keyed carrier actually does.</para>
/// <para>Below <see cref="MinimumSpreadDb"/> of separation the gate refuses to
/// decide at all and reports the key up. Noise has structure, and a threshold
/// placed in the middle of it produces confident nonsense. Silence is the
/// honest output when there is nothing there (§0.0).</para>
/// </remarks>
public sealed class CwGate
{
    /// <summary>
    /// How far a keyed signal must stand above the noise before the gate will
    /// call anything at all, in decibels.
    /// </summary>
    public const double MinimumSpreadDb = 6.0;

    /// <summary>Where in the gap between noise and peak a mark begins.</summary>
    private const double RisingFraction = 0.50;

    /// <summary>Where in that gap a mark ends. Lower, which is the hysteresis.</summary>
    private const double FallingFraction = 0.35;

    /// <summary>
    /// How far below the peak a mark begins when the signal is strong, in
    /// decibels.
    /// </summary>
    /// <remarks>
    /// SIX DECIBELS IS HALF AMPLITUDE, and that is the whole reason for this
    /// number. A transmitter shapes each element's edges, so the envelope
    /// crosses half its own height exactly at the element's true edge. Deciding
    /// there means a mark measures the length it actually was.
    /// <para>Without a cap the rule would be "halfway between the noise and the
    /// peak, in decibels", which is right for a signal a few decibels out of the
    /// noise and badly wrong for a strong one. A clean signal a hundred decibels
    /// above the floor would put the threshold fifty decibels down, which is a
    /// third of a percent of the amplitude, and the gate would open on the first
    /// breath of the rising edge. Every mark then measures long, every gap
    /// short, and the speed estimate walks away from the truth. That is exactly
    /// what this cap was added to fix.</para>
    /// </remarks>
    private const double MaximumRisingDropDb = 6.0;

    /// <summary>How far below the peak a mark ends. Deeper, which is the hysteresis.</summary>
    private const double MaximumFallingDropDb = 9.0;

    private const double NoiseFallAlpha = 0.25;
    private const double NoiseRiseAlpha = 0.0008;
    private const double PeakRiseAlpha = 0.35;
    private const double PeakFallAlpha = 0.002;

    private bool _started;
    private bool _keyDown;

    /// <summary>Where the gate believes the noise sits, in decibels.</summary>
    public double NoiseFloorDb { get; private set; }

    /// <summary>Where the gate believes a keyed signal sits, in decibels.</summary>
    public double PeakDb { get; private set; }

    /// <summary>How far a keyed signal is standing above the noise.</summary>
    public double SpreadDb => PeakDb - NoiseFloorDb;

    /// <summary>True when there is enough separation to decide anything.</summary>
    public bool HasSignal => _started && SpreadDb >= MinimumSpreadDb;

    /// <summary>
    /// Judge one measurement.
    /// </summary>
    /// <param name="powerDb">Energy at the tracked pitch, in decibels.</param>
    /// <returns>The decision and what it was made from.</returns>
    public GateReading Judge(double powerDb)
    {
        if (!_started)
        {
            // The first measurement is all there is to go on, so both trackers
            // start there and are allowed to diverge from real evidence rather
            // than from a number picked in advance.
            _started = true;
            NoiseFloorDb = powerDb;
            PeakDb = powerDb;
        }
        else
        {
            NoiseFloorDb += (powerDb - NoiseFloorDb)
                * (powerDb < NoiseFloorDb ? NoiseFallAlpha : NoiseRiseAlpha);

            PeakDb += (powerDb - PeakDb)
                * (powerDb > PeakDb ? PeakRiseAlpha : PeakFallAlpha);
        }

        var spread = PeakDb - NoiseFloorDb;

        // Halfway up from the noise while the signal is marginal, and a fixed
        // distance below the peak once it is strong. The two rules meet
        // continuously where the cap bites, so nothing jumps as a signal comes
        // up out of the noise.
        var drop = _keyDown
            ? Math.Min(spread * (1 - FallingFraction), MaximumFallingDropDb)
            : Math.Min(spread * (1 - RisingFraction), MaximumRisingDropDb);

        var threshold = PeakDb - drop;

        if (spread < MinimumSpreadDb)
        {
            _keyDown = false;
            return new GateReading(
                false, powerDb, NoiseFloorDb, PeakDb, threshold, HasSignal: false);
        }

        _keyDown = powerDb >= threshold;

        return new GateReading(
            _keyDown, powerDb, NoiseFloorDb, PeakDb, threshold, HasSignal: true);
    }
}
