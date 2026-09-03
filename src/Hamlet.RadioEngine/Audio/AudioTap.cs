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

    /// <summary>Held by WRITERS ONLY. No reader ever waits on it.</summary>
    /// <remarks>
    /// <para>**IT IS NOT THE READ LOCK IT USED TO BE, AND THAT IS THE WHOLE OF
    /// THIS CHANGE.** `Take`, `Snapshot`, `Window` and `Tail` all took one lock,
    /// so a reader copying thirty seconds out of the ring held the audio
    /// callback off for the duration of that copy. Measured on this machine at
    /// unit 239 task 1: with a reader running, the writer's 99th-percentile
    /// `Take` went from 176 us to 1,831 us — tenfold, and in the ordinary case
    /// rather than as an outlier.</para>
    /// <para>**IT SURVIVES FOR WRITERS BECAUSE THERE CAN BE MORE THAN ONE.** The
    /// device callback is one writer, and `CwDecoder.Process` taps directly on
    /// the fixture path, so two threads can call `Take` in a test even though
    /// the application has only ever had one. A sequence number alone would
    /// corrupt the ring there; this keeps writers serialised with each other
    /// while readers wait for nobody.</para>
    /// </remarks>
    private readonly object _writeGate = new();

    /// <summary>Even between writes, odd during one. Readers retry on a change.</summary>
    /// <remarks>
    /// <para>**A SEQLOCK, AND THE TEAR GUARANTEE IS WHY IT IS NOT JUST A LOOSER
    /// READ.** Unit 238's own remark on `Snapshot` says a capture must not come
    /// out torn across the write cursor, and this project has already spent two
    /// evenings on audio that was not what it claimed to be. Dropping the lock
    /// without detecting a tear would buy the callback's time with the
    /// recording's honesty.</para>
    /// <para>**HOW IT WORKS.** The writer bumps this to odd, writes, and bumps
    /// it to even. A reader takes the count, copies, and takes it again: if it
    /// changed or was odd, the copy may straddle a write and is thrown away and
    /// retried. The writer never looks at the reader at all.</para>
    /// </remarks>
    private long _sequence;


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

    /// <summary>Wall-clock marks, one per Take, for the arrival ratio.</summary>
    /// <remarks>
    /// <para>**THE ONLY DEVICE-INDEPENDENT WAY TO SAY THE AUDIO IS SHORT.**
    /// NAudio 2.2.1 defines `AudioClientBufferFlags.DataDiscontinuity` and
    /// `WasapiCapture` never surfaces it: `WaveInEventArgs` carries `Buffer` and
    /// `BytesRecorded` and nothing else. So there is no overrun signal to read,
    /// and arrival has to be inferred from how many samples turned up against
    /// how much time passed - which is exactly what work instruction 238's own
    /// table did by hand from four press captures.</para>
    /// <para>A small ring of marks rather than a running average, because the
    /// slot refusal needs the ratio across **one slot's own wall-clock span**
    /// and not a smoothed figure that a good minute can hide a bad slot inside.
    /// </para>
    /// </remarks>
    private readonly (DateTime AtUtc, long SamplesSeen)[] _marks = new (DateTime, long)[512];

    private int _markWrite;
    private int _marksFilled;

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

        lock (_writeGate)
        {
            // **ODD WHILE WRITING.** Everything between here and the matching
            // bump below may be seen half-finished by a reader, which is exactly
            // what the reader checks for.
            System.Threading.Volatile.Write(ref _sequence, _sequence + 1);

            if (_sampleRate != sampleRate)
            {
                _sampleRate = sampleRate;
                _ring = new float[Math.Max(1, sampleRate * SecondsKept)];
                _write = 0;
                _filled = 0;
                _levelWanted = Math.Max(1, (int)(sampleRate * LevelSeconds));
            }

            SamplesSeen += samples.Length;

            // **THE MARK IS TAKEN HERE, WHERE THE AUDIO ACTUALLY ARRIVES.**
            // Timing it anywhere else would measure when something asked about
            // the audio rather than when the device delivered it.
            _marks[_markWrite] = (DateTime.UtcNow, SamplesSeen);
            _markWrite = (_markWrite + 1) % _marks.Length;

            if (_marksFilled < _marks.Length)
            {
                _marksFilled++;
            }

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

            // **EVEN AGAIN: THE RING AND ITS CURSORS AGREE ONCE MORE.** A reader
            // that took an odd count, or a different one, throws its copy away
            // and tries again.
            System.Threading.Volatile.Write(ref _sequence, _sequence + 1);
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
        var count = _filled;

        if (count <= 0)
        {
            return null;
        }

        var samples = new float[count];

        return TryRead(samples, count, Newest, out var rate)
            ? new MonoAudio(rate, samples)
            : null;
    }

    /// <summary>Everything held, into a caller's buffer.</summary>
    /// <param name="destination">
    /// Where to put it. Must hold at least <see cref="SamplesHeld"/> samples.
    /// </param>
    /// <param name="written">How many samples were written into it.</param>
    /// <param name="rate">The rate they were taken at.</param>
    /// <returns>True where a clean copy was made.</returns>
    /// <remarks>
    /// **FOR THE CALLERS THAT REPEAT.** 5.7 MB of `float[]` several times a
    /// second is large-object-heap traffic, and its collections pause every
    /// thread including the one carrying the audio. A caller that reads on a
    /// timer owns one buffer and reuses it.
    /// </remarks>
    public bool Snapshot(float[] destination, out int written, out int rate)
    {
        ArgumentNullException.ThrowIfNull(destination);

        written = 0;
        var count = _filled;

        if (count <= 0 || destination.Length < count)
        {
            rate = 0;

            return false;
        }

        if (!TryRead(destination, count, Newest, out rate))
        {
            return false;
        }

        written = count;

        return true;
    }

    /// <summary>How many samples the ring is holding right now.</summary>
    public int SamplesHeld => _filled;

    /// <summary>The samples between two places on the audio clock.</summary>
    /// <param name="firstSample">Where to start, counted from the first sample ever taken.</param>
    /// <param name="count">How many to give back.</param>
    /// <returns>The samples, or null where the tap no longer holds all of them.</returns>
    /// <remarks>
    /// **ADDRESSED BY THE CLOCK RATHER THAN BY "THE LAST N"**, because the thing
    /// asking may be behind the tap. The tap takes a whole chunk at once and the
    /// decoder walks it a hop at a time, so a re-read firing inside that walk
    /// wants the audio the decoder has seen and not the audio that has arrived —
    /// and asking for the last N would hand it hops from the future, which would
    /// make the replay depend on the size of the chunk it fired inside.
    /// </remarks>
    public MonoAudio? Window(long firstSample, int count)
    {
        if (count <= 0)
        {
            return null;
        }

        var samples = new float[count];

        return TryRead(samples, count, firstSample, out var rate)
            ? new MonoAudio(rate, samples)
            : null;
    }

    /// <summary>One span, into a caller's buffer.</summary>
    /// <param name="firstSample">The first sample wanted.</param>
    /// <param name="count">How many.</param>
    /// <param name="destination">Where to put them.</param>
    /// <param name="rate">The rate they were taken at.</param>
    /// <returns>True where a clean copy was made.</returns>
    public bool Window(long firstSample, int count, float[] destination, out int rate)
    {
        ArgumentNullException.ThrowIfNull(destination);

        rate = 0;

        return count > 0
            && destination.Length >= count
            && TryRead(destination, count, firstSample, out rate);
    }

    /// <summary>
    /// The most recent stretch of what the tap is holding, oldest first.
    /// </summary>
    /// <param name="wanted">How much to take.</param>
    /// <returns>
    /// The audio, or null when nothing has arrived or less than <paramref
    /// name="wanted"/> is held. **Short is not padded**: a meter that measures
    /// six seconds must not be handed four and told they are six.
    /// </returns>
    /// <remarks>
    /// Snapshot copies the whole ring, which is half a minute, and something that
    /// wants six seconds of it once a second would copy five times what it reads.
    /// Taken under the same lock and for the same reason.
    /// </remarks>
    public MonoAudio? Tail(TimeSpan wanted)
    {
        var rate = _sampleRate;

        if (rate <= 0)
        {
            return null;
        }

        var count = (int)Math.Round(wanted.TotalSeconds * rate);

        if (count <= 0 || count > _filled)
        {
            return null;
        }

        var samples = new float[count];

        // The newest `count`, which `TryRead` expresses as "start `count` back
        // from the newest" - the same span `Snapshot` takes when count == filled.
        return TryRead(samples, count, SamplesSeen - count, out var taken)
            ? new MonoAudio(taken, samples)
            : null;
    }

    /// <summary>The newest span, into a caller's buffer.</summary>
    /// <param name="wanted">How much.</param>
    /// <param name="destination">Where to put it.</param>
    /// <param name="written">How many samples were written.</param>
    /// <param name="rate">The rate they were taken at.</param>
    /// <returns>True where a clean copy was made.</returns>
    public bool Tail(TimeSpan wanted, float[] destination, out int written, out int rate)
    {
        ArgumentNullException.ThrowIfNull(destination);

        written = 0;
        rate = _sampleRate;

        if (rate <= 0)
        {
            return false;
        }

        var count = (int)Math.Round(wanted.TotalSeconds * rate);

        if (count <= 0 || count > _filled || destination.Length < count)
        {
            return false;
        }

        if (!TryRead(destination, count, SamplesSeen - count, out rate))
        {
            return false;
        }

        written = count;

        return true;
    }

    /// <summary>
    /// What fraction of real time the device actually delivered, over a window.
    /// </summary>
    /// <param name="window">How far back to look.</param>
    /// <returns>
    /// Samples delivered divided by samples a continuous stream would have
    /// delivered in the same wall-clock span, or NaN where there is not enough
    /// history to say.
    /// </returns>
    /// <remarks>
    /// <para>**IT IS A COUNT OVER A COUNT AND IT IS LABELLED AS ONE** (§0.0).
    /// It is not a signal-to-noise ratio, not a quality figure and not a
    /// judgement about the band. It says: this many samples arrived, that much
    /// time passed, here is the fraction.</para>
    /// <para>**NaN IS NOT ZERO.** Too little history to divide by is *nobody
    /// measured*, and a zero there would read as *the sound card delivered
    /// nothing*, which is a different and much louder claim.</para>
    /// <para>One above is possible and is not clamped: a device whose clock runs
    /// slightly fast, or a burst arriving after a stall, genuinely delivers more
    /// than the nominal rate for a moment. Hiding that would be hiding the same
    /// class of fact this exists to show.</para>
    /// </remarks>
    public double ArrivalRatio(TimeSpan window)
    {
        var now = DateTime.UtcNow;

        return ArrivalRatioBetween(now - window, now);
    }

    /// <summary>The same fraction across one stated wall-clock span.</summary>
    /// <param name="fromUtc">The start of the span.</param>
    /// <param name="toUtc">The end of the span.</param>
    /// <returns>The fraction, or NaN where the marks do not cover it.</returns>
    /// <remarks>
    /// **THE SLOT'S OWN RATIO**, which is what the refusal needs: a slot is
    /// fifteen seconds of wall clock and the question is how much audio arrived
    /// inside it, not how the last minute averaged.
    /// </remarks>
    public double ArrivalRatioBetween(DateTime fromUtc, DateTime toUtc)
    {
        // **READ THROUGH THE SEQUENCE, NOT A LOCK** (work instruction 239 task 2:
        // "a reader asking for the arrival ratio must not take a lock the
        // callback needs either"). The marks are a small ring the writer appends
        // to, so the same rule applies as to the samples: copy, then check the
        // writer did not move under the copy.
        for (var attempt = 0; attempt < ReadAttempts; attempt++)
        {
            var before = System.Threading.Volatile.Read(ref _sequence);

            if ((before & 1) != 0)
            {
                continue;
            }

            var answer = RatioBetween(fromUtc, toUtc);

            if (System.Threading.Volatile.Read(ref _sequence) == before)
            {
                return answer;
            }

            TornReads++;
        }

        AbandonedReads++;

        // **NaN IS NOBODY MEASURED**, which is exactly what a read that could not
        // get a clean look has to say (§0.0).
        return double.NaN;
    }

    /// <summary>The arithmetic, with the lock already held.</summary>
    private double RatioBetween(DateTime fromUtc, DateTime toUtc)
    {
        var span = (toUtc - fromUtc).TotalSeconds;

        if (_sampleRate <= 0 || _marksFilled == 0 || span <= 0)
        {
            return double.NaN;
        }

        long? atStart = null;
        long? atEnd = null;
        var oldest = DateTime.MaxValue;

        for (var i = 0; i < _marksFilled; i++)
        {
            var mark = _marks[i];

            if (mark.AtUtc < oldest)
            {
                oldest = mark.AtUtc;
            }

            // The newest mark at or before the start, and at or before the end.
            if (mark.AtUtc <= fromUtc && (atStart is null || mark.SamplesSeen > atStart))
            {
                atStart = mark.SamplesSeen;
            }

            if (mark.AtUtc <= toUtc && (atEnd is null || mark.SamplesSeen > atEnd))
            {
                atEnd = mark.SamplesSeen;
            }
        }

        // **NO MARK BEFORE THE SPAN BEGAN MEANS THE HISTORY DOES NOT REACH IT.**
        // Treating the first mark as the start would divide real audio by a
        // shorter span and report a ratio near one for a stream that had only
        // just begun - flattering exactly the case this is here to catch.
        if (atEnd is null || (atStart is null && oldest > fromUtc))
        {
            return double.NaN;
        }

        var delivered = atEnd.Value - (atStart ?? 0);
        var expected = span * _sampleRate;

        return expected <= 0 ? double.NaN : delivered / expected;
    }


    /// <summary>How many reads had to be retried because a write intervened.</summary>
    /// <remarks>
    /// **A RETRY IS NOT A FAULT AND IT IS STILL COUNTED** (HM-DEC-093). It is the
    /// tear guarantee doing its job, and the number says how often a reader and
    /// the callback met. A count that climbed steeply would mean readers are
    /// reading far too often for the ring they are reading, which is a fact
    /// worth having rather than one to discover later.
    /// </remarks>
    public long TornReads { get; private set; }

    /// <summary>How many reads gave up after retrying and answered null.</summary>
    /// <remarks>
    /// **NULL RATHER THAN A TORN BUFFER** (§0.0). A reader that cannot get a
    /// clean copy says it has nothing, which every caller already handles,
    /// because `Window` has always been able to answer null for audio the ring
    /// no longer holds.
    /// </remarks>
    public long AbandonedReads { get; private set; }

    /// <summary>How many times a read is retried before it gives up.</summary>
    /// <remarks>
    /// **EIGHT, WHICH IS FAR MORE THAN THE ARITHMETIC NEEDS.** A device writes
    /// one buffer per period - 100 ms on this machine, measured - and a block
    /// copy of the whole thirty-second ring takes on the order of a millisecond.
    /// A reader has to be unlucky twice to retry once and eight times running to
    /// fail, and each retry re-reads a cursor that has already moved past it.
    /// </remarks>
    private const int ReadAttempts = 8;

    /// <summary>Ask for the newest samples rather than an absolute index.</summary>
    private const long Newest = long.MinValue;

    /// <summary>
    /// Copy `count` samples ending at the newest, starting `fromNewest` back,
    /// into a caller's buffer, without ever making the writer wait.
    /// </summary>
    /// <param name="destination">Where to put them.</param>
    /// <param name="count">How many.</param>
    /// <param name="firstSample">
    /// The absolute index of the first sample wanted, or <see cref="Newest"/>
    /// to mean "the newest `count` the ring holds".
    /// </param>
    /// <param name="rate">The rate the samples were taken at.</param>
    /// <returns>True where a clean copy was made.</returns>
    /// <remarks>
    /// <para>**THE COPY HAPPENS OUTSIDE ANY LOCK.** That is the point of the
    /// whole change: the writer is never behind a reader, however big the read
    /// or however slow the machine.</para>
    /// <para>**TWO BLOCK COPIES, NOT A MODULO PER SAMPLE.** The ring is
    /// contiguous either side of the write cursor, so a read is at most two
    /// spans. The old form walked 1,440,000 samples with a `%` on each one while
    /// holding the lock the callback needed.</para>
    /// </remarks>
    private bool TryRead(float[] destination, int count, long firstSample, out int rate)
    {
        rate = 0;

        for (var attempt = 0; attempt < ReadAttempts; attempt++)
        {
            var before = System.Threading.Volatile.Read(ref _sequence);

            if ((before & 1) != 0)
            {
                // A write is in progress. Nothing read now can be trusted.
                continue;
            }

            var ring = _ring;
            var filled = _filled;
            var write = _write;
            var seen = SamplesSeen;
            var sampleRate = _sampleRate;

            if (ring.Length == 0 || filled == 0 || sampleRate <= 0 || count <= 0)
            {
                return false;
            }

            var oldest = seen - filled;
            // **THE SENTINEL IS long.MinValue AND NOT `NEGATIVE`, BECAUSE A
            // NEGATIVE INDEX IS A REAL QUESTION WITH A REAL ANSWER.** The first
            // cut of this used `firstSample < 0` to mean "the newest", so
            // `Window(-1000000, n)` - a caller asking for audio long before
            // anything the ring holds - came back with the NEWEST audio instead
            // of null. That is a confident wrong answer in place of an honest
            // refusal, and the refusal test caught it.
            var start = firstSample == Newest ? filled - count : firstSample - oldest;

            if (count > filled || start < 0 || start + count > filled)
            {
                // The ring no longer holds what was asked for. That is a real
                // answer and not a torn read, so it does not retry.
                return false;
            }

            var from = (int)(((filled < ring.Length ? 0 : write) + start) % ring.Length);
            var first = Math.Min(count, ring.Length - from);

            Array.Copy(ring, from, destination, 0, first);

            if (first < count)
            {
                Array.Copy(ring, 0, destination, first, count - first);
            }

            if (System.Threading.Volatile.Read(ref _sequence) == before)
            {
                rate = sampleRate;

                return true;
            }

            // The writer moved under the copy. Everything above may straddle it.
            TornReads++;
        }

        AbandonedReads++;

        return false;
    }

    /// <summary>Throw away what is held, so the next capture starts fresh.</summary>
    public void Forget()
    {
        // **A WRITER, SO IT TAKES THE WRITER'S GATE** and opens the sequence
        // window: it moves the same cursors `Take` does, and a reader mid-copy
        // must see that and retry rather than return a ring that was emptied
        // underneath it.
        lock (_writeGate)
        {
            System.Threading.Volatile.Write(ref _sequence, _sequence + 1);

            _write = 0;
            _filled = 0;

            System.Threading.Volatile.Write(ref _sequence, _sequence + 1);
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
