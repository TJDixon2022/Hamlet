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
/// <para>Two trackers, both in decibels, both asymmetric. The noise floor
/// settles to a quieter band over a fraction of a second and creeps back up
/// over several, so a burst of static does not convince it the band got noisy.
/// The peak rises within about one dit and falls over a couple of seconds, so
/// it follows a signal down through a fade instead of leaving the threshold
/// stranded above it. That asymmetry is the whole design: the failure it
/// prevents is a decoder that works beautifully for thirty seconds and then
/// goes silent without ever saying why.</para>
/// <para>NEITHER TRACKER CHASES INDIVIDUAL MEASUREMENTS, and that was learned
/// the hard way. Noise in a narrow filter swings about five decibels either
/// side of its own average from one measurement to the next, so trackers quick
/// enough to follow that end up with the peak sitting on the noise's high
/// points and the floor on its low ones. The gap between them then looks like
/// twenty-five decibels of signal on a band with nothing on it at all, and the
/// decoder gets handed a stream of imaginary dits and dahs the right length to
/// be believed. Slowing both to something a real element could still move
/// leaves noise reading under ten decibels of spread and a real signal reading
/// nearly forty.</para>
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
    public const double MinimumSpreadDb = 10.0;

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

    /// <summary>
    /// How fast the floor follows a noise level that was actually measured.
    /// </summary>
    /// <remarks>
    /// Symmetric and quick, about a tenth of a second, because a measurement
    /// taken beside the tone has no reason to be trusted more in one direction
    /// than the other. The asymmetry below exists only because inferring noise
    /// from the signal's own bin is biased, and a measurement does not need
    /// protecting from a bias it does not have.
    /// </remarks>
    private const double MeasuredNoiseAlpha = 0.05;

    /// <summary>How fast the noise floor settles down to a quieter band.</summary>
    /// <remarks>About a third of a second, which is slower than noise wobbles
    /// and faster than a band changes.</remarks>
    private const double NoiseFallAlpha = 0.03;

    /// <summary>How fast the noise floor gives way to a noisier one.</summary>
    /// <remarks>Several seconds, so a burst of static does not move it.</remarks>
    private const double NoiseRiseAlpha = 0.0008;

    /// <summary>How fast the peak follows a signal up.</summary>
    /// <remarks>About one dit at ordinary speeds, which is as fast as it can be
    /// without also following the noise.</remarks>
    private const double PeakRiseAlpha = 0.12;

    /// <summary>How fast the peak follows a signal down.</summary>
    /// <remarks>A couple of seconds, which is what keeps the threshold with a
    /// signal through a fade instead of stranded above it.</remarks>
    private const double PeakFallAlpha = 0.002;

    /// <summary>
    /// The shortest de-glitch window, in measurements.
    /// </summary>
    /// <remarks>
    /// FIVE, WHICH IS TWENTY-FIVE MILLISECONDS, and it removes any run shorter
    /// than three. Noise crossing a threshold produces runs of one and two
    /// measurements constantly, and left alone they become marks, then elements,
    /// then letters out of an empty band. The shortest thing anybody actually
    /// sends is a dit at sixty words a minute, which is twenty milliseconds and
    /// four measurements, so this throws away what cannot be Morse and keeps
    /// everything that can.
    /// </remarks>
    public const int ShortestVote = 5;

    /// <summary>
    /// The longest de-glitch window, in measurements.
    /// </summary>
    /// <remarks>
    /// **NINE IS WHERE IT STOPS, AND THE REASON IS A MEASUREMENT RATHER THAN A
    /// PREFERENCE** (HM-DEC-088). Widening the window buys sensitivity and then
    /// abruptly stops buying it and starts inventing: measured across a sweep,
    /// seven reads a decibel and a half further into the noise than five, nine is
    /// about the same, and eleven and thirteen read no further while returning
    /// most of the message as the wrong letters. A window that spans a real dit
    /// deletes real dits, and what comes out the other side is the right length
    /// to be believed (§0.0).
    /// </remarks>
    public const int LongestVote = 9;

    /// <summary>
    /// How much of a dit the window is allowed to span.
    /// </summary>
    /// <remarks>
    /// A third. The median deletes runs shorter than half the window, so a window
    /// of a third of a dit deletes nothing longer than a sixth of one, and no
    /// real element is that short. This is what makes the window safe to widen at
    /// twelve words a minute and safe to narrow at forty, where a fixed number
    /// has to be wrong at one end or the other.
    /// </remarks>
    private const double VoteShareOfDit = 1.0 / 3.0;

    private readonly bool[] _votes = new bool[LongestVote];

    private int _voteWindow = ShortestVote;
    private int _voteCount;
    private int _voteWrite;
    private bool _started;
    private bool _keyDown;

    /// <summary>Where the gate believes the noise sits, in decibels.</summary>
    public double NoiseFloorDb { get; private set; }

    /// <summary>How many measurements the de-glitch is currently voting over.</summary>
    public int VoteWindow => _voteWindow;

    /// <summary>
    /// Size the de-glitch from the element it is protecting (HM-DEC-088).
    /// </summary>
    /// <param name="ditHops">How many measurements a dit currently spans.</param>
    /// <remarks>
    /// **THE SINGLE LARGEST GAIN IN SENSITIVITY THIS CHANGE MADE**, and it is
    /// integration over the element in the only place a two-valued signal allows
    /// it. A median over a third of a dit throws away far more of the chatter a
    /// marginal signal produces than a fixed twenty-five milliseconds does at
    /// ordinary speeds, while still being shorter than a dit at forty words a
    /// minute, where a fixed wider window would delete real elements.
    /// </remarks>
    public void FollowSpeed(double ditHops)
    {
        if (ditHops <= 0 || double.IsNaN(ditHops))
        {
            return;
        }

        var wanted = (int)Math.Round(ditHops * VoteShareOfDit);

        // Odd, so a median has a middle and cannot tie.
        if (wanted % 2 == 0)
        {
            wanted++;
        }

        var next = Math.Clamp(wanted, ShortestVote, LongestVote);

        if (next != _voteWindow)
        {
            _voteWindow = next;
            _voteCount = 0;
            _voteWrite = 0;
        }
    }

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
    /// <param name="measuredNoiseDb">
    /// What the band is doing beside the tone at this instant, or NaN when
    /// nothing measured it (HM-DEC-088).
    /// </param>
    /// <returns>The decision and what it was made from.</returns>
    /// <remarks>
    /// **WHERE THE NOISE COMES FROM CHANGED, AND THE REST DID NOT.** Given a
    /// measurement from the bins either side, the floor follows that instead of
    /// being inferred from the signal's own bin during the gaps. It is unbiased,
    /// it is available during a mark as well as between marks, and it cannot be
    /// dragged up by the signal it is supposed to be measuring. Without one, the
    /// old asymmetric tracker still runs, so nothing that fed this before behaves
    /// differently.
    /// </remarks>
    public GateReading Judge(double powerDb, double measuredNoiseDb = double.NaN)
    {
        var measured = !double.IsNaN(measuredNoiseDb);

        if (!_started)
        {
            // The first measurement is all there is to go on, so both trackers
            // start there and are allowed to diverge from real evidence rather
            // than from a number picked in advance.
            _started = true;
            NoiseFloorDb = measured ? measuredNoiseDb : powerDb;
            PeakDb = powerDb;
        }
        else
        {
            if (measured)
            {
                // Smoothed, not taken raw. One measurement of the noise is
                // itself noisy by several decibels, and a threshold that jitters
                // with it produces exactly the imaginary dits this gate was
                // slowed down to prevent.
                NoiseFloorDb += (measuredNoiseDb - NoiseFloorDb) * MeasuredNoiseAlpha;
            }
            else
            {
                NoiseFloorDb += (powerDb - NoiseFloorDb)
                    * (powerDb < NoiseFloorDb ? NoiseFallAlpha : NoiseRiseAlpha);
            }

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
            Vote(false);
            return new GateReading(
                false, powerDb, NoiseFloorDb, PeakDb, threshold, HasSignal: false);
        }

        // The raw decision drives the hysteresis, and the de-glitched one is
        // what the rest of the chain sees. Keeping them apart matters: voting
        // on the hysteresis input would fight it, since both are trying to stop
        // the same chatter and neither would then be doing its job properly.
        _keyDown = powerDb >= threshold;

        return new GateReading(
            Vote(_keyDown), powerDb, NoiseFloorDb, PeakDb, threshold, HasSignal: true);
    }

    /// <summary>
    /// The majority decision across the last few measurements.
    /// </summary>
    /// <remarks>
    /// A median filter over the key state, which is what a de-glitch on a
    /// two-valued signal amounts to. It delays everything by two measurements
    /// and shifts nothing, because a median moves the edges of a long run not
    /// at all while deleting short ones outright. A moving average would have
    /// smeared every edge instead, and edge positions are the entire content of
    /// Morse.
    /// </remarks>
    private bool Vote(bool keyDown)
    {
        _votes[_voteWrite] = keyDown;
        _voteWrite = (_voteWrite + 1) % _voteWindow;
        _voteCount = Math.Min(_voteCount + 1, _voteWindow);

        var down = 0;
        for (var i = 0; i < _voteCount; i++)
        {
            if (_votes[i])
            {
                down++;
            }
        }

        return down * 2 > _voteCount;
    }
}
