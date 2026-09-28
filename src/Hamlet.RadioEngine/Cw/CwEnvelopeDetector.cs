using System.Globalization;
using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Cw;

/// <summary>One hop as the scope draws it.</summary>
/// <param name="EnvelopeDb">The energy across the passband this hop.</param>
/// <param name="FloorDb">The tracked noise floor under it.</param>
/// <param name="ThresholdDb">The floor plus the margin: the one number that decides a mark.</param>
/// <param name="Mark">Whether the envelope stood over the threshold.</param>
public readonly record struct CwScopeHop(double EnvelopeDb, double FloorDb, double ThresholdDb, bool Mark);

/// <summary>What the envelope detector says at its last hop.</summary>
/// <param name="EnvelopeDb">The energy across the passband, in dB below full scale.</param>
/// <param name="FloorDb">The tracked noise floor.</param>
/// <param name="ThresholdDb">The floor plus <see cref="CwEnvelopeDetector.ThresholdMarginDb"/>.</param>
/// <param name="Mark">Whether the envelope is over the threshold.</param>
/// <param name="RunMs">How long the current mark or gap has lasted.</param>
/// <param name="PitchHz">While a mark is up, the strongest bin in the passband; NaN otherwise.</param>
/// <param name="ContrastDb">While a mark is up, that bin over the passband's median bin; NaN otherwise.</param>
/// <param name="PassbandLowHz">The low edge the envelope is summed from.</param>
/// <param name="PassbandHighHz">The high edge it is summed to.</param>
/// <param name="PassbandFromRig">Whether the edges came from the radio's pitch and filter.</param>
/// <param name="MarksLast4s">How many marks the last four seconds hold.</param>
public sealed record CwEnvelopeReading(
    double EnvelopeDb,
    double FloorDb,
    double ThresholdDb,
    bool Mark,
    double RunMs,
    double PitchHz,
    double ContrastDb,
    double PassbandLowHz,
    double PassbandHighHz,
    bool PassbandFromRig,
    int MarksLast4s)
{
    /// <summary>Nothing heard.</summary>
    public static CwEnvelopeReading None { get; } = new(
        double.NaN, double.NaN, double.NaN, false, 0, double.NaN, double.NaN,
        CwEnvelopeDetector.WholeBandLowHz, CwEnvelopeDetector.WholeBandHighHz, false, 0);
}

/// <summary>
/// **THE OSCILLOSCOPE: A MARK IS THE ENVELOPE OVER A THRESHOLD, AT ANY PITCH** (work
/// instruction 476 task 1, step 12 criterion 12.1, R90, HM-DEC-185).
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-28**: *"Think you're an oscilloscope. Once we hit a certain
/// amplitude, regardless of frequency, that's probably a character. Everything else is
/// noise."*</para>
/// <para>**THE OLDEST CW DETECTOR THERE IS.** Per hop: the energy across the passband the
/// radio's filter passes, as one number in dB; a floor tracked under it; a threshold a fixed
/// margin above the floor; and a mark wherever the envelope stands over the threshold.
/// **Frequency is not needed to decide that somebody is keying** - it is read afterward,
/// from where the energy sits in the spectrum while the mark is up, across the whole
/// passband and not 300 to 900 Hz.</para>
/// <para>**IT OBSERVES AND DRIVES NOTHING.** Nothing in the decoder, the tracker, the survey
/// or the keying meter reads it; it is on the screen so the owner's ear can judge it before
/// it is trusted with anything (R90: the wiring is the unit after).</para>
/// <para>**THREAD**: fed on whichever thread the audio arrives on and read from the UI's,
/// so every hop is committed and every read is taken under one lock.</para>
/// </remarks>
public sealed class CwEnvelopeDetector
{
    /// <summary>
    /// How far over the floor the envelope must stand to be a mark, in dB.
    /// </summary>
    /// <remarks>
    /// **FROM THE OWNER'S SEVEN VERDICT ROWS OF 2026-09-28 AND NOTHING ELSE** (work
    /// instruction 476). Under the AGC FAST the app sets on a CW tune-in, every station on
    /// those rows swung between 17.5 and 21.8 dB; nine is half the smallest of them, so a
    /// station as weak as the weakest he heard clears it with room. **No recording was
    /// measured to choose it** (R88), and it is the one number that decides a mark.
    /// Author's, overrulable.
    /// </remarks>
    public const double ThresholdMarginDb = 9;

    /// <summary>
    /// The time constant of the floor's average over the hops that are not marks, in seconds.
    /// </summary>
    /// <remarks>
    /// <para>**THE FLOOR IS THE NOISE'S MEAN BETWEEN MARKS, AND A RULE, NOT A FIT.** Every
    /// hop that is not a mark is averaged into it in power with this time constant, and no
    /// mark ever is, so a station cannot drag it up. A quarter second settles well inside
    /// one word space at five words a minute (1.68 s, step 5's slowest), so the floor is
    /// back on the noise before the next word however slowly he sends.</para>
    /// <para>**WHY NOT THE NOISE'S TROUGHS.** Built first as *down at once to any quieter
    /// hop, up slowly*, it sat on the deepest dips of a ten-millisecond envelope in a
    /// 500 Hz filter, and ordinary noise then stood nine decibels over it: 127 of 800 hops
    /// of synthetic noise alone were marks. Over the mean, the same noise marks none.</para>
    /// <para>Author's, overrulable; no recording chose it.</para>
    /// </remarks>
    public const double FloorSeconds = 0.25;

    /// <summary>
    /// How fast the floor may still climb while a mark is up, in dB per second.
    /// </summary>
    /// <remarks>
    /// **SO A CARRIER, OR A JUMP IN THE BAND'S NOISE, IS NOT A MARK FOREVER.** Marks are kept
    /// out of the floor's average, so without this a noise floor that rose by more than the
    /// margin would be marked from then on. The longest element the phase names, a dah at
    /// five words a minute (720 ms, step 5), lifts it 2.2 dB, a quarter of the margin; a
    /// steady carrier is climbed past in about three seconds, which is right: a carrier is
    /// not keying. Author's, overrulable; no recording chose it.
    /// </remarks>
    public const double FloorRiseDbPerSecond = 3;

    /// <summary>How much history the scope holds and the mark count covers, in seconds.</summary>
    public const double HistorySeconds = 4;

    /// <summary>The low edge summed when the radio's pitch or filter is not known.</summary>
    /// <remarks>
    /// **THE WHOLE AUDIO BAND A RECEIVER PASSES, NOT A RADIO FACT.** Where the rig has not
    /// said its CW pitch and filter width, the envelope is summed from here to
    /// <see cref="WholeBandHighHz"/>, so a station anywhere a voice filter would pass is
    /// still seen, at the cost of more noise in the sum. Author's, overrulable.
    /// </remarks>
    public const double WholeBandLowHz = 100;

    /// <summary>The high edge summed when the radio's pitch or filter is not known.</summary>
    public const double WholeBandHighHz = 3000;

    private readonly object _gate = new();
    private readonly RealFft _envelopeFft;
    private readonly RealFft _pitchFft;
    private readonly float[] _ring;
    private readonly float[] _window;
    private readonly float[] _envelopeHann;
    private readonly float[] _pitchHann;
    private readonly double[] _magnitudes;
    private readonly double[] _real;
    private readonly double[] _imaginary;
    private readonly double[] _scratch;
    private readonly double _envelopeHannPower;
    private readonly CwScopeHop[] _history;

    private int _ringWrite;
    private int _ringFill;
    private int _hopFill;
    private int _historyWrite;
    private int _historyFill;
    private int _runHops;
    private double _floorDb = double.NaN;
    private long _hopsSeen;
    private double _lowHz = WholeBandLowHz;
    private double _highHz = WholeBandHighHz;
    private bool _fromRig;
    private CwEnvelopeReading _reading = CwEnvelopeReading.None;

    private IAudioSource? _attached;

    /// <summary>Creates a detector for one sample rate.</summary>
    /// <param name="sampleRate">Samples per second of the audio it will be fed.</param>
    public CwEnvelopeDetector(int sampleRate)
    {
        SampleRate = Math.Max(1_000, sampleRate);

        // **THE DECODER'S OWN HOP**, five milliseconds, so a mark here and a hop in
        // `CwToneTracker` are the same slice of time.
        HopSamples = Math.Max(4, SampleRate / 200);

        // The envelope is read over about two hops, ten milliseconds, so a dit at forty
        // words a minute (30 ms) is several hops long; the pitch over about forty, which
        // resolves a bin of 25 Hz or finer and is only computed while a mark is up.
        EnvelopeWindowSamples = PowerOfTwoAtLeast(2 * HopSamples);
        PitchWindowSamples = PowerOfTwoAtLeast(SampleRate / 25);

        _envelopeFft = new RealFft(EnvelopeWindowSamples);
        _pitchFft = new RealFft(PitchWindowSamples);
        _ring = new float[PitchWindowSamples];
        _window = new float[PitchWindowSamples];
        _envelopeHann = Hann(EnvelopeWindowSamples);
        _pitchHann = Hann(PitchWindowSamples);
        _magnitudes = new double[_pitchFft.BinCount];
        _real = new double[PitchWindowSamples];
        _imaginary = new double[PitchWindowSamples];
        _scratch = new double[_pitchFft.BinCount];
        _envelopeHannPower = _envelopeHann.Sum(w => (double)w * w);

        _history = new CwScopeHop[(int)Math.Ceiling(HistorySeconds * SampleRate / HopSamples)];
    }

    /// <summary>Samples per second.</summary>
    public int SampleRate { get; }

    /// <summary>Samples per hop.</summary>
    public int HopSamples { get; }

    /// <summary>One hop, in milliseconds.</summary>
    public double HopMs => 1000.0 * HopSamples / SampleRate;

    /// <summary>Samples the envelope is read over.</summary>
    public int EnvelopeWindowSamples { get; }

    /// <summary>Samples the pitch is read over.</summary>
    public int PitchWindowSamples { get; }

    /// <summary>The width of one pitch bin, in hertz.</summary>
    public double PitchBinHz => (double)SampleRate / PitchWindowSamples;

    /// <summary>How many hops the history holds.</summary>
    public int HistoryHops => _history.Length;

    /// <summary>What the detector says at its last hop.</summary>
    public CwEnvelopeReading Reading
    {
        get
        {
            lock (_gate)
            {
                return _reading;
            }
        }
    }

    /// <summary>
    /// Where the envelope is summed: the radio's CW pitch plus and minus half its filter
    /// width, or the whole audio band where either is unknown.
    /// </summary>
    /// <param name="pitchHz">The radio's CW pitch, or null.</param>
    /// <param name="widthHz">The radio's filter width, or null.</param>
    public void SetPassband(double? pitchHz, double? widthHz)
    {
        lock (_gate)
        {
            if (pitchHz is > 0 and var pitch && widthHz is > 0 and var width)
            {
                _lowHz = Math.Max(0, pitch - (width / 2));
                _highHz = Math.Min(SampleRate / 2.0, pitch + (width / 2));
                _fromRig = true;
            }
            else
            {
                _lowHz = WholeBandLowHz;
                _highHz = Math.Min(SampleRate / 2.0, WholeBandHighHz);
                _fromRig = false;
            }

            _reading = _reading with
            {
                PassbandLowHz = _lowHz,
                PassbandHighHz = _highHz,
                PassbandFromRig = _fromRig,
            };
        }
    }

    /// <summary>Listen to a source beside whatever else listens to it. Replaces any previous one.</summary>
    /// <param name="source">The source, or null to stop listening.</param>
    public void Listen(IAudioSource? source)
    {
        if (ReferenceEquals(_attached, source))
        {
            return;
        }

        if (_attached is not null)
        {
            _attached.SamplesReady -= OnSamples;
        }

        _attached = source;

        if (_attached is not null)
        {
            _attached.SamplesReady += OnSamples;
        }
    }

    /// <summary>Feed audio, in any size; it is walked a hop at a time.</summary>
    /// <param name="samples">Mono samples at <see cref="SampleRate"/>.</param>
    public void Process(ReadOnlySpan<float> samples)
    {
        lock (_gate)
        {
            foreach (var s in samples)
            {
                _ring[_ringWrite] = s;
                _ringWrite = (_ringWrite + 1) % _ring.Length;
                _ringFill = Math.Min(_ringFill + 1, _ring.Length);

                if (++_hopFill == HopSamples)
                {
                    _hopFill = 0;

                    if (_ringFill >= EnvelopeWindowSamples)
                    {
                        Hop();
                    }
                }
            }
        }
    }

    /// <summary>The last four seconds, oldest first.</summary>
    /// <returns>One entry per hop.</returns>
    public IReadOnlyList<CwScopeHop> History()
    {
        lock (_gate)
        {
            var copy = new CwScopeHop[_historyFill];
            var start = (_historyWrite - _historyFill + _history.Length) % _history.Length;

            for (var i = 0; i < _historyFill; i++)
            {
                copy[i] = _history[(start + i) % _history.Length];
            }

            return copy;
        }
    }

    /// <summary>The tone line the scope shows.</summary>
    /// <param name="reading">A reading.</param>
    /// <returns><c>tone 742 Hz, 24 dB over the band</c>, or <c>no tone</c> when no mark is up.</returns>
    public static string ToneLine(CwEnvelopeReading reading)
    {
        ArgumentNullException.ThrowIfNull(reading);

        return reading.Mark && !double.IsNaN(reading.PitchHz)
            ? string.Create(
                CultureInfo.InvariantCulture,
                $"tone {reading.PitchHz:0} Hz, {reading.ContrastDb:0} dB over the band")
            : "no tone";
    }

    private void OnSamples(in AudioChunk chunk) => Process(chunk.Samples);

    private void Hop()
    {
        var envelopeDb = Envelope();

        // **THE FLOOR: THE MEAN OF THE GAPS.** The mark is judged against the floor as it
        // stood before this hop, then the floor moves. **THE FIRST QUARTER SECOND ONLY
        // BUILDS IT**, as a plain mean, and judges nothing: seeded from one hop it sat in
        // whatever trough that hop fell in, and the next noise peak was a mark.
        _hopsSeen++;
        var settled = _hopsSeen * HopMs > FloorSeconds * 1000;
        var floorDb = double.IsNaN(_floorDb) ? envelopeDb : _floorDb;
        var thresholdDb = floorDb + ThresholdMarginDb;
        var mark = settled && envelopeDb > thresholdDb;

        _runHops = mark == _reading.Mark && _historyFill > 0 ? _runHops + 1 : 1;

        var pitchHz = double.NaN;
        var contrastDb = double.NaN;

        if (mark)
        {
            (pitchHz, contrastDb) = Pitch();
        }

        _history[_historyWrite] = new CwScopeHop(envelopeDb, floorDb, thresholdDb, mark);
        _historyWrite = (_historyWrite + 1) % _history.Length;
        _historyFill = Math.Min(_historyFill + 1, _history.Length);

        if (mark)
        {
            _floorDb = Math.Min(envelopeDb, floorDb + (FloorRiseDbPerSecond * HopMs / 1000));
        }
        else
        {
            var floorPower = Math.Pow(10, floorDb / 10);
            var envelopePower = Math.Pow(10, envelopeDb / 10);
            var share = settled
                ? 1 - Math.Exp(-HopMs / 1000 / FloorSeconds)
                : 1.0 / _hopsSeen;

            _floorDb = 10 * Math.Log10(floorPower + (share * (envelopePower - floorPower)) + 1e-20);
        }

        // The reading carries the floor and threshold this hop was judged against, the
        // same pair the scope draws for it.
        _reading = new CwEnvelopeReading(
            envelopeDb,
            floorDb,
            thresholdDb,
            mark,
            _runHops * HopMs,
            pitchHz,
            contrastDb,
            _lowHz,
            _highHz,
            _fromRig,
            MarksInHistory());
    }

    private double Envelope()
    {
        var n = EnvelopeWindowSamples;
        Latest(n, _envelopeHann);

        _envelopeFft.Magnitudes(_window.AsSpan(0, n), _magnitudes, _real, _imaginary);

        var power = 0.0;
        var bins = 0;

        for (var bin = 1; bin < _envelopeFft.BinCount; bin++)
        {
            var hz = _envelopeFft.BinHz(bin, SampleRate);

            if (hz >= _lowHz && hz <= _highHz)
            {
                power += _magnitudes[bin] * _magnitudes[bin];
                bins++;
            }
        }

        // A filter narrower than one bin still gets the bin nearest its centre.
        if (bins == 0)
        {
            var centre = (int)Math.Round((_lowHz + _highHz) / 2 / _envelopeFft.BinHz(1, SampleRate));
            centre = Math.Clamp(centre, 1, _envelopeFft.BinCount - 1);
            power = _magnitudes[centre] * _magnitudes[centre];
        }

        // Mean square, one-sided, window-corrected: a full-scale sine reads -3 dB.
        var meanSquare = 2 * power / (n * _envelopeHannPower);

        return 10 * Math.Log10(meanSquare + 1e-20);
    }

    private (double PitchHz, double ContrastDb) Pitch()
    {
        if (_ringFill < PitchWindowSamples)
        {
            return (double.NaN, double.NaN);
        }

        Latest(PitchWindowSamples, _pitchHann);
        _pitchFft.Magnitudes(_window, _magnitudes, _real, _imaginary);

        var lowBin = Math.Max(1, (int)Math.Ceiling(_lowHz / PitchBinHz));
        var highBin = Math.Min(_pitchFft.BinCount - 1, (int)Math.Floor(_highHz / PitchBinHz));

        if (highBin < lowBin)
        {
            return (double.NaN, double.NaN);
        }

        var peak = lowBin;

        for (var bin = lowBin; bin <= highBin; bin++)
        {
            if (_magnitudes[bin] > _magnitudes[peak])
            {
                peak = bin;
            }
        }

        // **THE CONTRAST IS OVER THE BAND**: the peak's power over the median bin of the
        // passband, the peak's own main lobe (two bins either side) left out.
        var others = 0;

        for (var bin = lowBin; bin <= highBin; bin++)
        {
            if (Math.Abs(bin - peak) > 2)
            {
                _scratch[others++] = _magnitudes[bin] * _magnitudes[bin];
            }
        }

        var contrastDb = double.NaN;

        if (others > 0)
        {
            Array.Sort(_scratch, 0, others);
            var median = _scratch[others / 2];
            var peakPower = _magnitudes[peak] * _magnitudes[peak];
            contrastDb = 10 * Math.Log10((peakPower + 1e-20) / (median + 1e-20));
        }

        return (peak * PitchBinHz, contrastDb);
    }

    /// <summary>Copy the latest samples, tapered, into the front of the window buffer.</summary>
    private void Latest(int count, float[] taper)
    {
        var start = (_ringWrite - count + _ring.Length) % _ring.Length;

        for (var i = 0; i < count; i++)
        {
            _window[i] = _ring[(start + i) % _ring.Length] * taper[i];
        }
    }

    /// <summary>Marks that began inside the history, and one already up at its start.</summary>
    private int MarksInHistory()
    {
        var start = (_historyWrite - _historyFill + _history.Length) % _history.Length;
        var count = 0;
        var before = false;

        for (var i = 0; i < _historyFill; i++)
        {
            var mark = _history[(start + i) % _history.Length].Mark;

            if (mark && !before)
            {
                count++;
            }

            before = mark;
        }

        return count;
    }

    private static int PowerOfTwoAtLeast(int n)
    {
        var size = 4;

        while (size < n)
        {
            size <<= 1;
        }

        return size;
    }

    private static float[] Hann(int n)
    {
        var w = new float[n];

        for (var i = 0; i < n; i++)
        {
            w[i] = (float)(0.5 - (0.5 * Math.Cos(2 * Math.PI * i / n)));
        }

        return w;
    }
}
