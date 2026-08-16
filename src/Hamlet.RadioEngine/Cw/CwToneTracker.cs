namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// One measurement of the tone: how much energy is sitting at the pitch the
/// tracker is following, and where that pitch is.
/// </summary>
/// <param name="PowerDb">Energy at the tracked pitch, in decibels.</param>
/// <param name="CompetitorDb">
/// Energy at the strongest other note far enough away to be a different
/// station. This is how the decoder knows somebody else is on top of it.
/// </param>
/// <param name="ToneHz">The pitch being followed.</param>
/// <param name="SampleIndex">Index of the last sample in this measurement.</param>
/// <param name="NoiseDb">
/// What the band is doing either side of the tone, measured at this instant
/// (HM-DEC-088).
/// </param>
public readonly record struct ToneReading(
    double PowerDb, double CompetitorDb, double ToneHz, long SampleIndex,
    double NoiseDb = double.NaN)
{
    /// <summary>True when the noise beside the tone was actually measured.</summary>
    public bool HasNoise => !double.IsNaN(NoiseDb);

    /// <summary>
    /// How far the tone stands above the noise beside it, right now.
    /// </summary>
    /// <remarks>
    /// **MEASURED AT THE SAME INSTANT, IN OTHER BINS** (HM-DEC-088). The old
    /// estimate came from watching the signal's own bin during the gaps between
    /// elements, which is the only place a single-bin decoder can look. That
    /// works and it has two faults: it cannot tell a fade from a gap, and it is
    /// always one element behind. Taking the noise from either side instead is
    /// free, unbiased, and follows a fade without ever chasing the signal.
    /// </remarks>
    public double SnrDb => HasNoise ? PowerDb - NoiseDb : double.NaN;

    /// <summary>
    /// How far the tracked note stands above whatever a rival station is
    /// actually managing to put into the same filter.
    /// </summary>
    /// <remarks>
    /// NOT SIMPLY HOW MUCH LOUDER WE ARE THAN THEM. A station a couple of
    /// hundred hertz away can be stronger than the one being read and do no
    /// harm at all, because the filter rejects it, and a decoder that marked
    /// every character uncertain whenever somebody louder was on the band would
    /// be useless on a busy evening. What matters is how much of them gets
    /// through, which is their strength less what the filter takes off it.
    /// </remarks>
    public double MarginOverCompetitorDb
        => PowerDb - (CompetitorDb - CwToneTracker.FilterRejectionDb);

    /// <summary>
    /// How far the tracked note stands above the nearest rival before the
    /// filter is credited with anything.
    /// </summary>
    /// <remarks>
    /// This is the one the veto reads. A rival this close is a problem even
    /// when the filter is rejecting it cleanly, because it is not the leakage
    /// that does the damage: it is the gate's idea of where a signal sits being
    /// pulled up by somebody else's keying.
    /// </remarks>
    public double RawMarginOverCompetitorDb => PowerDb - CompetitorDb;
}

/// <summary>
/// Finds the CW note in the audio and keeps following it.
/// </summary>
/// <remarks>
/// <para>A bank of Goertzel filters across the pitches the radio can produce,
/// evaluated over a short sliding window. Goertzel rather than an FFT because
/// this needs a couple of dozen known frequencies rather than a whole spectrum,
/// and it is a handful of multiplies per bin per sample with nothing to
/// allocate. The waterfall has its own reasons for wanting a full transform;
/// a decoder listening for one note does not.</para>
/// <para>NOBODY TUNES EXACTLY, so the pitch is hunted rather than assumed. The
/// operator's setting says where to start, and the tracker follows the strongest
/// note across the range the IC-7300 can put a signal at, 300 to 900 Hz (Full
/// Manual p. 4-14). Somebody two hundred hertz off frequency still gets a
/// decode, which matters because being off frequency is what beginners do and
/// a decoder that punished it would be teaching the wrong lesson.</para>
/// <para>The tracker is deliberately sticky. When two notes are within a
/// decibel of one another it stays with whichever is closer to where it already
/// was, so a louder station arriving nearby does not drag the decode off the
/// signal the operator was reading. It only moves when the other signal is
/// genuinely stronger.</para>
/// <para>The window is twenty milliseconds and it advances five, which sets both
/// the frequency selectivity and the timing resolution. At forty words a minute
/// a dit is thirty milliseconds, so four measurements land inside the shortest
/// element there is.</para>
/// </remarks>
public sealed class CwToneTracker
{
    /// <summary>Lowest pitch searched, in hertz.</summary>
    public const double MinimumToneHz = 300;

    /// <summary>Highest pitch searched, in hertz.</summary>
    public const double MaximumToneHz = 900;

    /// <summary>Spacing between candidate pitches, in hertz.</summary>
    private const double BinSpacingHz = 25;

    /// <summary>How often the tracked pitch is reconsidered, in hops.</summary>
    private const int RetuneEveryHops = 40;

    /// <summary>
    /// How much stronger a rival note must be before the tracker moves to it.
    /// </summary>
    private const double StickinessDb = 1.0;

    /// <summary>Smoothing on the per-bin averages the retune decision reads.</summary>
    private const double BinAverageAlpha = 0.05;

    /// <summary>
    /// How far from the tracked note another one has to be before it counts as
    /// a different station rather than the same one leaking sideways.
    /// </summary>
    /// <remarks>
    /// The window is twenty milliseconds, so the filter's own response reaches
    /// about a hundred hertz either side of where it is pointed. Past that,
    /// energy belongs to something else. Inside it, energy is the signal being
    /// read and calling it competition would have the decoder reporting itself
    /// as interference.
    /// </remarks>
    private const double CompetitorSeparationHz = 125;

    /// <summary>
    /// How much of a rival station this far off the tracked note the filter
    /// takes off, in decibels.
    /// </summary>
    /// <remarks>
    /// A conservative reading of what a Hann-tapered twenty-millisecond window
    /// does past a hundred and twenty-five hertz of separation. Conservative on
    /// purpose: understating the rejection makes the decoder mark characters
    /// uncertain that it could have read, which costs the operator some dimmed
    /// text. Overstating it lets somebody else's dits into a character that
    /// still looks clean, which costs them the truth (§0.0).
    /// </remarks>
    public const double FilterRejectionDb = 25;

    /// <summary>
    /// How many hops the window spans.
    /// </summary>
    /// <remarks>
    /// FOUR, WHICH IS TWENTY MILLISECONDS, and the number is a trade rather than
    /// a preference. A longer window hears pitch more finely and so tells one
    /// station from another close by; a shorter one sees the keying more
    /// sharply. Measured against a fixture with a second station a hundred and
    /// fifty hertz away, two hops leaves the tracker pulled twenty-five hertz off
    /// the signal it is reading and four lands on it exactly. Six is finer still
    /// and starts blurring the keying, which costs more than the pitch is worth.
    /// Four also still puts four measurements inside a thirty-millisecond dit,
    /// which is the shortest anybody sends.
    /// </remarks>
    private const int WindowHops = 4;

    private readonly double[] _binHz;
    private readonly double[] _binCoefficient;
    private readonly double[] _binAverage;
    private readonly float[] _window;
    private readonly float[] _hann;
    private readonly float[] _scratch;
    private readonly double[] _neighbors;

    private int _windowFill;
    private int _windowWrite;
    private int _hopFill;
    private int _hopsSinceRetune;
    private long _samplesSeen;
    private int _tracked;

    /// <summary>Creates a tracker.</summary>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="startingToneHz">Where to begin looking, from the operator's setting.</param>
    public CwToneTracker(int sampleRate, double startingToneHz)
    {
        SampleRate = Math.Max(1_000, sampleRate);
        HopSamples = Math.Max(4, SampleRate / 200);
        WindowSamples = HopSamples * WindowHops;

        var count = (int)Math.Round((MaximumToneHz - MinimumToneHz) / BinSpacingHz) + 1;
        _binHz = new double[count];
        _binCoefficient = new double[count];
        _binAverage = new double[count];

        for (var i = 0; i < count; i++)
        {
            _binHz[i] = MinimumToneHz + (i * BinSpacingHz);
            _binCoefficient[i] = 2 * Math.Cos(2 * Math.PI * _binHz[i] / SampleRate);
        }

        _neighbors = new double[count];
        _window = new float[WindowSamples];
        _scratch = new float[WindowSamples];
        _hann = new float[WindowSamples];

        for (var i = 0; i < WindowSamples; i++)
        {
            _hann[i] = (float)(0.5 - (0.5 * Math.Cos(2 * Math.PI * i / (WindowSamples - 1))));
        }

        _tracked = NearestBin(startingToneHz);
    }

    /// <summary>Samples per second.</summary>
    public int SampleRate { get; }

    /// <summary>How many samples one measurement advances by.</summary>
    public int HopSamples { get; }

    /// <summary>How many samples each measurement looks at.</summary>
    public int WindowSamples { get; }

    /// <summary>How long one hop lasts.</summary>
    public TimeSpan HopDuration => TimeSpan.FromSeconds((double)HopSamples / SampleRate);

    /// <summary>The pitch currently being followed, in hertz.</summary>
    public double ToneHz => _binHz[_tracked];

    /// <summary>
    /// Feed samples, calling back once per hop.
    /// </summary>
    /// <param name="samples">The samples.</param>
    /// <param name="firstSampleIndex">Index of the first sample in the stream.</param>
    /// <param name="onReading">Called for each completed measurement.</param>
    public void Process(
        ReadOnlySpan<float> samples, long firstSampleIndex, Action<ToneReading> onReading)
    {
        for (var i = 0; i < samples.Length; i++)
        {
            _window[_windowWrite] = samples[i];
            _windowWrite = (_windowWrite + 1) % WindowSamples;

            if (_windowFill < WindowSamples)
            {
                _windowFill++;
            }

            _samplesSeen = firstSampleIndex + i + 1;
            _hopFill++;

            if (_hopFill < HopSamples || _windowFill < WindowSamples)
            {
                continue;
            }

            _hopFill = 0;
            onReading(Measure());
        }
    }

    /// <summary>The nearest candidate bin to a pitch.</summary>
    private int NearestBin(double hz)
    {
        var clamped = Math.Clamp(hz, MinimumToneHz, MaximumToneHz);
        var index = (int)Math.Round((clamped - MinimumToneHz) / BinSpacingHz);
        return Math.Clamp(index, 0, _binHz.Length - 1);
    }

    /// <summary>
    /// One measurement over the current window.
    /// </summary>
    /// <remarks>
    /// Copies the ring into a linear scratch buffer with a Hann taper applied,
    /// then runs every Goertzel over it. The taper is what stops a strong
    /// station a couple of hundred hertz away from smearing into the bin being
    /// read, which is the whole reason this can decode through interference at
    /// all. Both buffers are allocated once and reused; this runs two hundred
    /// times a second for as long as the app is open.
    /// </remarks>
    private ToneReading Measure()
    {
        var start = _windowWrite;

        for (var i = 0; i < WindowSamples; i++)
        {
            _scratch[i] = _window[(start + i) % WindowSamples] * _hann[i];
        }

        var trackedPower = 0.0;
        var competitorPower = 0.0;
        var trackedHz = _binHz[_tracked];
        var neighbors = 0;

        for (var b = 0; b < _binHz.Length; b++)
        {
            var power = Goertzel(_binCoefficient[b]);
            _binAverage[b] += (power - _binAverage[b]) * BinAverageAlpha;

            if (b == _tracked)
            {
                trackedPower = power;
            }
            else if (Math.Abs(_binHz[b] - trackedHz) >= CompetitorSeparationHz)
            {
                if (power > competitorPower)
                {
                    competitorPower = power;
                }

                // Far enough out that the tone itself does not reach, which is
                // what makes these a sample of the band rather than of the
                // signal.
                _neighbors[neighbors++] = power;
            }
        }

        if (++_hopsSinceRetune >= RetuneEveryHops)
        {
            _hopsSinceRetune = 0;
            Retune();
        }

        return new ToneReading(
            ToDb(trackedPower), ToDb(competitorPower), trackedHz, _samplesSeen,
            ToDb(NoiseFrom(neighbors)));
    }

    /// <summary>Goertzel power over the scratch buffer at one coefficient.</summary>
    private double Goertzel(double coefficient)
    {
        var s1 = 0.0;
        var s2 = 0.0;

        for (var i = 0; i < _scratch.Length; i++)
        {
            var s0 = _scratch[i] + (coefficient * s1) - s2;
            s2 = s1;
            s1 = s0;
        }

        var power = (s1 * s1) + (s2 * s2) - (coefficient * s1 * s2);
        return Math.Max(0, power) / ((double)WindowSamples * WindowSamples);
    }

    /// <summary>
    /// Reconsider which note to follow.
    /// </summary>
    /// <remarks>
    /// Reads the smoothed per-bin averages rather than the instant powers,
    /// because a decision taken during the gap between two dits would follow
    /// whatever the noise happened to be doing. Ties inside the stickiness
    /// margin go to the bin nearest where the tracker already is.
    /// </remarks>
    private void Retune()
    {
        var best = 0;

        for (var b = 1; b < _binAverage.Length; b++)
        {
            if (_binAverage[b] > _binAverage[best])
            {
                best = b;
            }
        }

        if (_binAverage[best] <= 0)
        {
            return;
        }

        var floor = ToDb(_binAverage[best]) - StickinessDb;
        var chosen = best;

        for (var b = 0; b < _binAverage.Length; b++)
        {
            if (_binAverage[b] > 0
                && ToDb(_binAverage[b]) >= floor
                && Math.Abs(b - _tracked) < Math.Abs(chosen - _tracked))
            {
                chosen = b;
            }
        }

        _tracked = chosen;
    }

    /// <summary>
    /// What the band is doing beside the tone (HM-DEC-088).
    /// </summary>
    /// <param name="count">How many neighboring bins were collected.</param>
    /// <returns>A noise power, or NaN when there was nothing to look at.</returns>
    /// <remarks>
    /// <para>**THE MEDIAN, NOT THE MEAN**, and that is the whole reason this
    /// works on a busy band. A second station sitting in one or two of these bins
    /// drags a mean upward and takes the threshold with it, which would make the
    /// decoder deaf exactly when somebody is calling nearby. A median ignores any
    /// minority of loud bins entirely.</para>
    /// <para>Selection is by partial sort into a scratch array that is allocated
    /// once. This runs two hundred times a second for as long as the application
    /// is open, and §8 is explicit that the decoder's own measurements may not
    /// allocate per element.</para>
    /// </remarks>
    private double NoiseFrom(int count)
    {
        if (count == 0)
        {
            return double.NaN;
        }

        Array.Sort(_neighbors, 0, count);

        return _neighbors[count / 2];
    }

    /// <summary>Power in decibels, with a floor so silence is a number.</summary>
    private static double ToDb(double power) => 10 * Math.Log10(power + 1e-14);
}
