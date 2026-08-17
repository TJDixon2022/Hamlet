namespace Hamlet.RadioEngine.Audio;

/// <summary>
/// What the decoder is actually being fed, right now (HM-DEC-088).
/// </summary>
/// <param name="PeakDb">
/// The loudest sample in the last moment, in decibels below full scale. Zero is
/// the loudest the path can carry and a large negative number is silence.
/// </param>
/// <param name="RmsDb">The average level over the same moment.</param>
/// <param name="FloorDb">
/// The quietest the audio has been recently, which is as close to a noise floor
/// as a level meter can honestly get.
/// </param>
/// <param name="Clipping">True when samples are hitting the end of the scale.</param>
/// <param name="Seconds">How much audio the tap is holding.</param>
public readonly record struct AudioLevel(
    double PeakDb, double RmsDb, double FloorDb, bool Clipping, double Seconds)
{
    /// <summary>Full scale, for a level bar that has to end somewhere.</summary>
    public const double FullScaleDb = 0;

    /// <summary>Where a bar starts, below which nothing useful is happening.</summary>
    public const double SilenceDb = -90;

    /// <summary>
    /// Below this the decoder is being handed near-silence.
    /// </summary>
    /// <remarks>
    /// **THE NUMBER THAT SEPARATES THE TWO PATHS.** The operator hears the
    /// speaker and the decoder hears the USB codec, and those are different
    /// signals with different gains. Sixty decibels below full scale is a
    /// sixty-fourth of a percent of the available range: a signal that quiet is
    /// not a decoder problem and no amount of decoding will fix it.
    /// </remarks>
    public const double TooQuietDb = -60;

    /// <summary>How far the loudest moment stood above the quietest.</summary>
    public double SpreadDb => PeakDb - FloorDb;

    /// <summary>True when there is essentially nothing arriving.</summary>
    public bool NearlySilent => PeakDb <= TooQuietDb;

    /// <summary>Nothing measured yet.</summary>
    public static AudioLevel None { get; } =
        new(SilenceDb, SilenceDb, SilenceDb, false, 0);
}

/// <summary>
/// Keeps the last half minute of what the decoder heard, and how loud it was
/// (HM-DEC-088).
/// </summary>
/// <remarks>
/// <para>**THIS EXISTS BEFORE ANY DECODER CHANGE DOES, AND THAT IS THE POINT.**
/// The operator copies signals by ear that produce nothing on screen. Every
/// explanation for that is a hypothesis, and without a recording of one such
/// signal the next three sessions would argue about audio nobody can
/// look at (§0.0.1). A wrong decode with its input attached is a regression
/// test; a wrong decode without one is an argument.</para>
/// <para>**AND THE LEVEL IS THE OTHER HALF.** What reaches the speaker and what
/// reaches the computer are two paths with two gains, and turning one up does
/// nothing for the other. If the decoder is being handed near-silence while the
/// operator is listening to a perfectly good signal, that is the whole diagnosis
/// and it should take one glance.</para>
/// <para>A ring buffer, written on the audio thread, allocating nothing per
/// chunk (§8). Reading it copies, so a capture cannot tear.</para>
/// </remarks>
public sealed class AudioTap
{
    /// <summary>How much audio is kept.</summary>
    /// <remarks>
    /// Thirty seconds. Long enough to hold a full exchange at a slow speed, and
    /// at eight kilohertz it is under a megabyte and a half, which is nothing to
    /// hold and nothing to write.
    /// </remarks>
    public const int SecondsKept = 30;

    /// <summary>Over how long the level is measured.</summary>
    /// <remarks>
    /// A fifth of a second, which is a few Morse elements at any speed. Short
    /// enough to move when the signal does, long enough that a bar drawn from it
    /// does not flicker.
    /// </remarks>
    private const double LevelSeconds = 0.2;

    /// <summary>How fast the floor gives way to a quieter band.</summary>
    private const double FloorFallAlpha = 0.25;

    /// <summary>How fast the floor gives way to a noisier one.</summary>
    private const double FloorRiseAlpha = 0.01;

    private readonly object _lock = new();

    private float[] _ring = Array.Empty<float>();
    private int _write;
    private int _filled;
    private int _sampleRate;

    private double _sumOfSquares;
    private double _peak;
    private int _levelCount;
    private int _levelWanted = 1;
    private bool _clippedInWindow;

    private double _floorDb = AudioLevel.SilenceDb;
    private bool _started;

    /// <summary>The most recent level, or nothing measured yet.</summary>
    public AudioLevel Level { get; private set; } = AudioLevel.None;

    /// <summary>Samples per second, or zero before anything has arrived.</summary>
    public int SampleRate => _sampleRate;

    /// <summary>
    /// How many samples have ever arrived (HM-DEC-090).
    /// </summary>
    /// <remarks>
    /// <para>**WHAT MAKES A CAPTURE ABLE TO PROVE IT IS FRESH.** Three captures
    /// taken within seventy seconds produced byte-identical files with identical
    /// analysis, while the rig state beside them differed on every one, so the
    /// radio was plainly being read while the audio was not moving at all. The
    /// operator drew conclusions from one recording presented as three.</para>
    /// <para>A ring buffer cannot tell whether it is holding thirty fresh seconds
    /// or the same thirty seconds it held a minute ago. This counter can, and it
    /// costs an addition per chunk.</para>
    /// </remarks>
    public long SamplesSeen { get; private set; }

    /// <summary>True once any audio at all has been seen.</summary>
    public bool HasAudio => _filled > 0;

    /// <summary>
    /// Take one chunk of what the decoder is being fed.
    /// </summary>
    /// <param name="samples">The samples.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <remarks>
    /// Runs on whichever thread the audio arrives on, so it allocates nothing
    /// except the once when the rate is first known, and never throws (§8).
    /// </remarks>
    public void Take(ReadOnlySpan<float> samples, int sampleRate)
    {
        if (sampleRate <= 0 || samples.Length == 0)
        {
            return;
        }

        lock (_lock)
        {
            if (_sampleRate != sampleRate)
            {
                _sampleRate = sampleRate;
                _ring = new float[Math.Max(1, sampleRate * SecondsKept)];
                _write = 0;
                _filled = 0;
                _levelWanted = Math.Max(1, (int)(sampleRate * LevelSeconds));
            }

            SamplesSeen += samples.Length;

            for (var i = 0; i < samples.Length; i++)
            {
                var sample = samples[i];

                _ring[_write] = sample;
                _write = (_write + 1) % _ring.Length;

                if (_filled < _ring.Length)
                {
                    _filled++;
                }

                var magnitude = Math.Abs((double)sample);

                _sumOfSquares += magnitude * magnitude;

                if (magnitude > _peak)
                {
                    _peak = magnitude;
                }

                // A SAMPLE AT THE END OF THE SCALE IS THE OPPOSITE FAILURE AND
                // EQUALLY FATAL. Everything past full scale comes back flat, and
                // a flattened tone has harmonics the decoder never sent looking
                // for.
                if (magnitude >= 0.999)
                {
                    _clippedInWindow = true;
                }

                if (++_levelCount < _levelWanted)
                {
                    continue;
                }

                Settle();
            }
        }
    }

    /// <summary>One level measurement is complete.</summary>
    private void Settle()
    {
        var rms = Math.Sqrt(_sumOfSquares / _levelCount);
        var peakDb = ToDb(_peak);
        var rmsDb = ToDb(rms);

        if (!_started)
        {
            _started = true;
            _floorDb = rmsDb;
        }
        else
        {
            // Asymmetric for the same reason the gate's is: a quiet moment is
            // evidence about the floor and a loud one is evidence about the
            // signal, and treating them alike puts the floor on top of the
            // signal it is meant to sit under.
            _floorDb += (rmsDb - _floorDb)
                * (rmsDb < _floorDb ? FloorFallAlpha : FloorRiseAlpha);
        }

        Level = new AudioLevel(
            peakDb, rmsDb, _floorDb, _clippedInWindow,
            _sampleRate <= 0 ? 0 : (double)_filled / _sampleRate);

        _sumOfSquares = 0;
        _peak = 0;
        _levelCount = 0;
        _clippedInWindow = false;
    }

    /// <summary>
    /// Everything the tap is holding, oldest first.
    /// </summary>
    /// <returns>The audio, or null when nothing has arrived.</returns>
    /// <remarks>
    /// A copy, taken under the lock, so a capture cannot come out torn across
    /// the write cursor.
    /// </remarks>
    public MonoAudio? Snapshot()
    {
        lock (_lock)
        {
            if (_filled == 0 || _sampleRate <= 0)
            {
                return null;
            }

            var samples = new float[_filled];
            var start = _filled < _ring.Length ? 0 : _write;

            for (var i = 0; i < _filled; i++)
            {
                samples[i] = _ring[(start + i) % _ring.Length];
            }

            return new MonoAudio(_sampleRate, samples);
        }
    }

    /// <summary>Throw away what is held, so the next capture starts fresh.</summary>
    public void Forget()
    {
        lock (_lock)
        {
            _write = 0;
            _filled = 0;
        }
    }

    /// <summary>
    /// The loudest sample in a recording, in decibels below full scale
    /// (HM-DEC-094).
    /// </summary>
    /// <param name="audio">The recording.</param>
    /// <returns>The peak.</returns>
    /// <remarks>
    /// <para>**THE LIVE METER ANSWERS A DIFFERENT QUESTION AND THE SIDECAR USED
    /// TO WRITE IT DOWN AS THIS ONE.** <see cref="Level"/> is the peak of the
    /// last fifth of a second, which is what a moving bar should show. A capture
    /// is thirty seconds long, and writing the meter's instantaneous reading
    /// beside it reported minus ten where the file itself peaked at minus one
    /// point six.</para>
    /// <para>Eight decibels of under-reporting is not a rounding error on this
    /// surface: it is the difference between "comfortable headroom" and "about to
    /// clip", and clipping flattens the tone edges that Morse timing is made of.
    /// </para>
    /// </remarks>
    public static double PeakOf(MonoAudio audio)
    {
        ArgumentNullException.ThrowIfNull(audio);

        var peak = 0.0;

        foreach (var sample in audio.Samples)
        {
            var magnitude = Math.Abs((double)sample);

            if (magnitude > peak)
            {
                peak = magnitude;
            }
        }

        return ToDb(peak);
    }

    /// <summary>Amplitude in decibels below full scale, with a floor.</summary>
    private static double ToDb(double magnitude)
        => magnitude <= 0
            ? AudioLevel.SilenceDb
            : Math.Max(AudioLevel.SilenceDb, 20 * Math.Log10(magnitude));
}
