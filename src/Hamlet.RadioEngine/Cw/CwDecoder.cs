using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// Finds a station, feeds it to the decoder, and keeps the record of what
/// arrived.
/// </summary>
/// <remarks>
/// <para>**THIS CLASS USED TO BE A DECODER AND IS NOW A HOST.** The decoding is
/// `CwProbabilisticStream`'s; what is left here is the tone tracker that finds a
/// station, the tap that keeps the audio a capture is written from, the counters
/// a roster row is scored against, and the rule that Hamlet stops listening while
/// the operator is sending (HM-DEC-147).</para>
/// <para>**WHAT WAS REMOVED AND WHY IT MATTERED.** The old decode path — the
/// thresholding gate, the run-length clock fit and the settled second pass — was
/// ruled out on 2026-08-21 and kept running for its counters. It decoded nothing
/// anybody could see, and it went on producing numbers that read as measurements
/// of the reading: a capture sheet said `clockFit dah 15.72 dits`, `decoderWpm
/// rolling 50` and `chars 0 emitted` beside text on screen that a completely
/// different decoder had produced, and a whole work order was written from them.
/// **Two decoders in one tree is two answers to every question and no way to tell
/// which one a sheet is about** (HM-DEC-091).</para>
/// </remarks>
public sealed class CwDecoder
{
    private readonly CwToneTracker _tracker;
    private readonly CwProbabilisticStream _probabilistic;
    private readonly Action<ToneReading> _onReading;

    private IAudioSource? _attached;
    private long _lastSample;
    /// <summary>Where the tracker last moved to a different station.</summary>
    private long _samplesAtDiscontinuity;

    /// <summary>
    /// Where the window was last emptied for a station change, or long.MinValue.
    /// </summary>
    private long _followedAt = long.MinValue;

    /// <summary>The station-change count the window was last emptied for.</summary>
    private int _lastStationChanges;

    /// <summary>Where the tracker was listening at the previous reading.</summary>
    private double _lastPitchHz = double.NaN;


    /// <summary>
    /// True once the tracker has moved to a different station at least once.
    /// </summary>
    /// <remarks>
    /// **BEFORE THE FIRST MOVE, NOUGHT IS NOT A MOMENT.** Reading the sample
    /// index alone makes a fresh decoder look as though somebody else had just
    /// started transmitting at sample nought, which held the speed unnamed for
    /// the first twelve seconds of every recording. Not having heard anything yet
    /// and having just lost the station are both states in which no speed may be
    /// named, and they are not the same state (§0.0).
    /// </remarks>
    private bool _hasFollowed;
    private int _lastFollows;

    private double _lastSnrDb = double.NaN;
    private bool _toneLatched;
    /// <summary>The pitch the mixdown is held at, or NaN when it follows.</summary>
    private double _lockedToneHz = double.NaN;

    /// <summary>The last pitch the survey actually measured, or NaN.</summary>
    /// <remarks>
    /// **A MEASURED PITCH IS HELD UNTIL A BETTER ONE ARRIVES, WITHOUT ANYBODY
    /// PRESSING ANYTHING.** The tracker answers with the middle of its bank
    /// whenever the survey has nothing admitted, which is every gap between
    /// overs and the whole of a slow sender's spacing. Following that answer
    /// swings the mixdown off a station that is still there and back again, and
    /// unit 002 measured what that costs: twenty-two invented characters against
    /// none with the pitch held still.
    /// </remarks>
    private double _lastMeasuredToneHz = double.NaN;

    private int _charactersEmitted;
    private int _charactersUnsure;
    private int _elementsResolved;

    private bool _transmitting;
    private DateTime _transmitEndedUtc = DateTime.MinValue;
    private long _suspendedChunks;

    private readonly double[] _snrHistory = new double[5];
    private int _snrWrite;
    private int _snrFilled;

    /// <summary>
    /// How fast the held signal-to-noise figure falls away, per measurement.
    /// </summary>
    /// <remarks>
    /// Measurements arrive two hundred times a second, so this decays about ten
    /// decibels in ten seconds: long enough to hold across the gaps inside a
    /// message and short enough that a station going away is noticed.
    /// </remarks>
    private const double SnrDecayDbPerHop = 0.005;

    /// <summary>Creates a decoder.</summary>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="expectedToneHz">
    /// The operator's CW pitch, as a place to start looking. The tracker hunts
    /// either side of it, since nobody tunes exactly.
    /// </param>
    public CwDecoder(int sampleRate, double expectedToneHz = 600)
    {
        SampleRate = Math.Max(1_000, sampleRate);
        _tracker = new CwToneTracker(SampleRate, expectedToneHz);
        _onReading = OnReading;
        _probabilistic = new CwProbabilisticStream(SampleRate);

        _probabilistic.CharacterSettled += c =>
        {
            // **THE COUNTERS COUNT WHAT REACHED THE SCREEN** (HM-DEC-091). They
            // used to be incremented on the old path's own emit, which raised
            // nothing anybody could see, so a capture sidecar said `0 characters
            // emitted` about an instant when the terminal was showing text.
            if (!c.IsWordGap)
            {
                _charactersEmitted++;

                if (c.IsUnreadable || c.Confidence != CwConfidence.High)
                {
                    _charactersUnsure++;
                }

                _elementsResolved += Math.Max(1, c.Pattern.Length);
            }

            CharacterSettled?.Invoke(c);
        };

        _probabilistic.LeadingEdgeChanged += e =>
        {
            LeadingEdge?.Invoke(e);

            foreach (var character in e)
            {
                CharacterDecoded?.Invoke(character);
            }
        };
    }

    /// <summary>Samples per second.</summary>
    public int SampleRate { get; }

    /// <summary>
    /// Whether the held window is emptied when the tracker crosses to somebody
    /// else.
    /// </summary>
    /// <remarks>
    /// **OFF BY RULING, WITH THE MACHINERY KEPT.** Everything behind it is built
    /// and tested: the line, the emptying, what survives the emptying, and the
    /// sentence the terminal shows while it refills. What is missing is a tracker
    /// whose moves mean what the line assumes they mean.
    /// </remarks>
    public const bool ClearOnAStationChange = false;

    /// <summary>How many times the held window has been emptied for a move.</summary>
    /// <remarks>
    /// Counted so the corpus can be swept and the answer stated rather than
    /// assumed: across every recording and fixture in this repository it is
    /// nought, because none of them holds a second sender the tracker reaches.
    /// </remarks>
    public int WindowClears { get; private set; }

    /// <summary>
    /// Whether a move from one pitch to another empties the held window.
    /// </summary>
    /// <param name="fromHz">Where the tracker was listening.</param>
    /// <param name="toHz">Where it is listening now.</param>
    /// <param name="reading">True when the decoder currently has text.</param>
    /// <returns>True when the window is no longer about the station being read.</returns>
    /// <remarks>
    /// <para>**THE LINE IS THE DECODER'S OWN FILTER, NOT A NUMBER CHOSEN FOR
    /// IT.** The stream mixes down to the tracked pitch through a filter
    /// <see cref="CwProbabilisticDecoder.BandwidthHz"/> wide, so a move shorter
    /// than that lands inside the passband the held audio was already taken
    /// through and cannot have put a different sender in it. **It is read from
    /// the decoder** rather than written here, so if that filter ever widens the
    /// line widens with it.</para>
    /// <para>**AND ONLY WHILE SOMEBODY WAS BEING READ.** Hamlet hunting for a
    /// station it has not found yet moves a long way and leaves nobody behind:
    /// on `cw-2026-08-18-004507` it goes 600 to 475 hertz in the first two
    /// seconds with nothing read, and emptying the window there throws away the
    /// opening of the message it is about to read, which is where the callsign
    /// lives. The test for "was being read" is the decoder's own text, because
    /// that is the thing being protected; a keying verdict or a signal margin
    /// would be a proxy for it and each has its own way of being wrong.</para>
    /// <para>**MEASURED ON EVERY RECORDING AND FIXTURE HERE AND IT FIRES ON
    /// NONE**, because four hold single senders and the one two-station fixture
    /// reaches its second station through the acquiring branch. Its first real
    /// test is an evening at the radio, and the failure mode is that it never
    /// fires, which is where the tree already was.</para>
    /// </remarks>
    public static bool ShouldClearWindow(double fromHz, double toHz, bool reading)
        => reading
           && !double.IsNaN(fromHz)
           && Math.Abs(toHz - fromHz) >= CwProbabilisticDecoder.BandwidthHz;

    /// <summary>What the decoder last made of the audio.</summary>
    /// <remarks>
    /// **THE ONE SOURCE FOR EVERY NUMBER ABOUT THE READING** (HM-DEC-091): the
    /// winning speed hypothesis, how much better than silence that reading is,
    /// and the text itself.
    /// </remarks>
    public CwProbabilisticResult Reading => _probabilistic.Last;

    /// <summary>The tone tracker, which is what finds a station.</summary>
    public CwToneTracker Tracker => _tracker;

    /// <summary>The pass that reads the audio, so its pitch can be checked.</summary>
    /// <remarks>
    /// **A LOCK THAT CANNOT BE OBSERVED CANNOT BE TESTED.** Whether the mixdown
    /// followed the tracker or held where it was put is the whole claim of the
    /// lock, and it is invisible from outside without this. It reads state and
    /// changes none.
    /// </remarks>
    public CwProbabilisticStream Stream => _probabilistic;

    /// <summary>
    /// The last half minute of exactly what the decoder was fed (HM-DEC-088).
    /// </summary>
    /// <remarks>
    /// **THE TAP IS HERE RATHER THAN AT THE SOUND CARD** so that what a capture
    /// contains is what the decoder received, not what something upstream
    /// believes it sent. A recording of a nearly-but-not-quite identical signal
    /// would settle nothing.
    /// </remarks>
    public AudioTap Tap { get; } = new();

    /// <summary>What is arriving, and what can be seen in it.</summary>
    /// <remarks>
    /// **THE ELEMENT COUNTS ARE THE SAME NUMBER TWICE, DELIBERATELY.** The gate
    /// that counted elements it could not resolve is gone, and this decoder never
    /// commits to an element it does not use: every dit and dah it counts is one
    /// that became part of a character. A pair of figures where the gap between
    /// them used to mean something now says the gap is nought, which is true.
    /// </remarks>
    public CwDecodeReport Report => new(
        Tap.Level,
        _tracker.ToneHz,
        _lastSnrDb,
        HasTone: _toneLatched,
        _elementsResolved,
        _elementsResolved,
        _charactersEmitted,
        _charactersUnsure,
        _tracker.HasKeying,
        _tracker.Verdict.Interference,
        (double)_tracker.Guard.BlockedHops * _tracker.HopSamples / SampleRate,
        Competitor: _tracker.Competitor,
        PitchWasMeasured: _tracker.HasMeasuredPitch);

    /// <summary>Everything inside the decision delay, handed over whole.</summary>
    /// <remarks>
    /// **IT IS REPLACED, NOT APPENDED TO.** The whole point of deciding late is
    /// that a letter can be read differently once the next one arrives, so a
    /// consumer takes this list as the current state of the leading edge rather
    /// than as news.
    /// </remarks>
    public event Action<IReadOnlyList<CwCharacter>>? LeadingEdge;

    /// <summary>The same leading edge, one character at a time.</summary>
    public event Action<CwCharacter>? CharacterDecoded;

    /// <summary>A character that is final and will not be revised.</summary>
    public event Action<CwCharacter>? CharacterSettled;

    /// <summary>The pitch the mixdown is held at, or NaN when it is following.</summary>
    /// <remarks>
    /// <para>**A LOCK THE OPERATOR CANNOT SEE IS A LOCK HE CANNOT TRUST**, and a
    /// wandering decode and a held one look identical on the screen today. This
    /// is what the panel reads to say which it is (HM-DEC-148's precedent: the
    /// state and the control, in the advisory area).</para>
    /// </remarks>
    public double LockedToneHz => _lockedToneHz;

    /// <summary>True while the mixdown is held at a fixed pitch.</summary>
    public bool IsLocked => !double.IsNaN(_lockedToneHz);

    /// <summary>
    /// Hold the mixdown at the strongest tone measured right now.
    /// </summary>
    /// <returns>The pitch it locked to, or NaN if there was nothing to lock to.</returns>
    /// <remarks>
    /// <para>**FROM THE INTERPOLATED PEAK AND NOT FROM A BIN, AND NEVER FROM THE
    /// RADIO'S OWN CW PITCH.** A capture taken on 2026-08-24 carries `CwPitch
    /// 600 Hz` in its sidecar while the station it holds sat at 439.81, so a lock
    /// to the radio's setting would have pointed the filter at empty spectrum and
    /// held it there. That is measured, not supposed.</para>
    /// <para>**IT REFUSES RATHER THAN GUESSING.** Where no peak can be measured —
    /// too little audio, or a peak at the edge of the bank where interpolating
    /// would be extrapolating — nothing is locked and the tracker keeps
    /// steering. A lock to a pitch nobody measured is worse than no lock,
    /// because the operator would be told the decoder is held on a station
    /// (§0.0).</para>
    /// <para>The tracker is not stopped. It goes on measuring, surveying and
    /// reporting, so the panel can still say where it thinks the station is and
    /// the operator can see the lock disagreeing with it.</para>
    /// </remarks>
    public double Lock()
    {
        // **THE MEASURED PITCH IF THERE IS ONE.** The tracker's interpolated
        // peak is a reading of whatever bin the bank is on, and where the survey
        // has admitted a station the refined pitch it reported is the better
        // number. A lock fed a bank centre locks onto the error.
        var peak = _tracker.HasMeasuredPitch
            ? _tracker.ToneHz
            : _tracker.MeasuredPeakHz;

        if (double.IsNaN(peak))
        {
            return double.NaN;
        }

        _lockedToneHz = peak;

        return peak;
    }

    /// <summary>Let the tracker steer the mixdown again.</summary>
    public void Unlock() => _lockedToneHz = double.NaN;

    /// <summary>
    /// How long decoding stays suspended after the radio stops transmitting.
    /// </summary>
    /// <remarks>
    /// <para>**HALF A SECOND, AND THE EVIDENCE FOR IT IS THE POLL AND NOT THE
    /// KEYING** (HM-DEC-147). Transmit status is a live field, asked for four
    /// times a second, so the state Hamlet holds can be a quarter of a second old
    /// before the reply is parsed. Full break-in switches between elements, which
    /// is tens of milliseconds: **the poll cannot see that at all**.</para>
    /// <para>What the figure is measured against is two poll intervals, so one
    /// dropped reply cannot resume decoding mid-transmission, and the receiver's
    /// own recovery, which <see cref="CwTransmitGuard"/> measured at about
    /// twenty-four milliseconds of transmit-receive hang with a ramp behind
    /// it.</para>
    /// <para>**IT IS ASYMMETRIC ON PURPOSE.** Suspension is immediate, because a
    /// late suspension puts the operator's own sending on the screen as somebody
    /// else's; resumption waits, because an early one does the same with the tail
    /// of it.</para>
    /// </remarks>
    public static TimeSpan ResumeAfter { get; } = TimeSpan.FromMilliseconds(500);

    /// <summary>True while the radio is transmitting or has just stopped.</summary>
    public bool DecodingSuspended { get; private set; }

    /// <summary>How many chunks of audio were dropped rather than decoded.</summary>
    public long SuspendedChunks => _suspendedChunks;

    /// <summary>The slowest speed anybody would call a speed.</summary>
    public const int SlowestPlausibleWpm = 6;

    /// <summary>The fastest the radio's own keyer sends.</summary>
    public const int FastestPlausibleWpm = 48;

    /// <summary>
    /// True while the decoder is refilling an emptied window after following
    /// somebody else.
    /// </summary>
    /// <remarks>
    /// **A TERMINAL THAT GOES QUIET WITHOUT SAYING WHY IS ITS OWN CONFIDENT
    /// WRONG ANSWER** (§0.0). Emptying the window costs up to twelve seconds of
    /// reading, and it happens at the exact moment somebody answers a call, which
    /// is when an unexplained silence reads as nobody being there. So the state
    /// is published rather than left to be inferred from an empty screen, and it
    /// ends the moment text comes back.
    /// </remarks>
    public bool ListeningAfresh
        => _followedAt != long.MinValue
           && _probabilistic.Last.Text.Length == 0
           && _lastSample - _followedAt
              < (long)(CwProbabilisticStream.WindowSeconds * SampleRate);

    /// <summary>
    /// True while the decoder's window still holds the station before this one.
    /// </summary>
    /// <remarks>
    /// **THE TEST IS WHETHER THE TRACKER HAS MOVED WITHIN A WINDOW'S WORTH OF
    /// AUDIO**, which is exact rather than a settling delay picked by hand. It
    /// used to be counted in marks the old estimator had seen; the tracker knows
    /// the same thing and survives the removal. A surface showing the speed
    /// leaves the field blank while this holds (§0.0).
    /// </remarks>
    public bool SpeedIsReacquiring
        => _hasFollowed
            ? _lastSample - _samplesAtDiscontinuity
              < (long)(CwProbabilisticStream.WindowSeconds * SampleRate)
            : _probabilistic.Last.Text.Length == 0;

    /// <summary>
    /// The sending speed, or null when nothing has earned the right to name one.
    /// </summary>
    /// <remarks>
    /// <para>**ONE GUARDED ANSWER, READ BY EVERY SURFACE** (HM-DEC-090). The
    /// speed reached three separate screens as a settled fact while nothing was
    /// being received, and guarding each of them would have left the fourth.</para>
    /// <para>**AND IT IS NOT NAMED ACROSS A HANDOVER.** The decoder reads a
    /// window several seconds long, so while that window still holds audio from
    /// the station before this one it names a speed between the two, which
    /// describes neither: measured, it named 18 where one station sends 16.</para>
    /// </remarks>
    public int? WordsPerMinute
    {
        get
        {
            var reading = _probabilistic.Last;

            if (reading.Text.Length == 0 || SpeedIsReacquiring)
            {
                return null;
            }

            var wpm = (int)Math.Round(reading.WordsPerMinute);

            return wpm >= SlowestPlausibleWpm && wpm <= FastestPlausibleWpm
                ? wpm
                : null;
        }
    }

    /// <summary>
    /// Listen to a source. Replaces any previous one.
    /// </summary>
    /// <param name="source">The source, or null to stop listening.</param>
    /// <exception cref="ArgumentException">The source runs at a different rate.</exception>
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

        if (source is not null && source.SampleRate != SampleRate)
        {
            throw new ArgumentException(
                $"the decoder was built for {SampleRate} Hz and this source runs at "
                + $"{source.SampleRate} Hz",
                nameof(source));
        }

        _attached = source;

        if (_attached is not null)
        {
            _attached.SamplesReady += OnSamples;
        }
    }

    /// <summary>Feed samples directly, without a source.</summary>
    /// <param name="chunk">The samples.</param>
    public void Process(in AudioChunk chunk)
    {
        // **THE TAP STILL TAKES IT.** A capture is the raw evidence of what
        // arrived at the sound card, and audio the operator made himself is part
        // of that: a recording that quietly omitted his own sending would be
        // worth less, not more (§0.0.1). What it does not do is reach a decoder.
        Tap.Take(chunk.Samples, chunk.SampleRate);

        if (DecodingSuspended)
        {
            // **NOT DECODED, NOT HELD, NOT RELEASED LATER** (HM-DEC-147). The
            // sidetone of the operator's own transmission is not something
            // anybody sent to him. The tracker is skipped along with everything
            // else, so the survey cannot retune to a sidetone and the pitch that
            // was being read is still there when he stops.
            _suspendedChunks++;

            // **BUT THE AUDIO CLOCK KEEPS RUNNING.** Dropping the samples without
            // letting time pass would stamp every character read afterwards as
            // though the transmission had never happened.
            _probabilistic.Skip(chunk.Samples.Length);
            return;
        }

        _tracker.Process(chunk.Samples, chunk.FirstSampleIndex, _onReading);

        // **AND THE SAME AUDIO GOES TO THE DECODER THAT READS IT** (HM-DEC-091:
        // one source). The tracker has already moved to wherever the station is
        // for this chunk, so the pitch handed over is the current one — unless
        // the operator has locked it, in which case the tracker carries on
        // measuring and reporting and stops steering.
        // **WHETHER A PITCH HAS BEEN MEASURED IS NOW ASKABLE, AND NOTHING HERE
        // ACTS ON IT YET.** Refusing to decode until the survey admits a
        // candidate was built and measured, and it costs `N4L` on
        // `cw-2026-08-17-134712` along with six other captures' text. The reason
        // is worth keeping: that recording's fallback bank centre is 500.0 and
        // its station sits at 500.09, so the callsign was only ever read because
        // an unmeasured number happened to land on it. Honesty and that callsign
        // are in tension and the ruling is Tim's (§0.0, HM-DEC-009).
        if (_tracker.HasMeasuredPitch)
        {
            _lastMeasuredToneHz = _tracker.ToneHz;
        }

        // **THE OPERATOR'S LOCK FIRST, THEN THE LAST MEASURED PITCH, THEN THE
        // BANK.** The middle rung is new and it is what stops the mixdown
        // swinging back to a bank centre every time the survey's three seconds
        // of history run dry — which on a slow sender is most of the time
        // between characters.
        //
        // **IT HOLDS AND IT DOES NOT CHOOSE.** Where the survey admits a
        // candidate the tracker's own rules decide which one and this follows
        // whatever they decided (HM-DEC-095, HM-DEC-127, both untouched). What
        // it changes is only what happens when nothing is admitted at all, which
        // is task 3's scope: the answer is the last thing actually measured
        // rather than the middle of a bank.
        _probabilistic.ToneHz = double.IsNaN(_lockedToneHz)
            ? double.IsNaN(_lastMeasuredToneHz)
                ? _tracker.ToneHz
                : _lastMeasuredToneHz
            : _lockedToneHz;
        _probabilistic.Process(chunk.Samples);

        // **ASKED AFTER THE DECODER HAS READ THIS AUDIO, NOT BEFORE.** The
        // tracker consults the interlock when it reads its survey, and both it
        // and the decoder work on the same half second, so setting it here rather
        // than while the tracker is still working through the chunk is the
        // difference between an answer about now and an answer about the previous
        // half second — a whole character at eighteen words a minute.
        _tracker.MidCharacter = _probabilistic.InsideCharacter;
    }

    /// <summary>
    /// Finish: settle anything still inside the decision delay, because nothing
    /// more is coming to revise it.
    /// </summary>
    public void Flush() => _probabilistic.Flush();

    /// <summary>
    /// Tell the decoder what the radio says about its own transmitter.
    /// </summary>
    /// <param name="transmitting">
    /// True when the radio reports the transmitter keyed, false when it reports
    /// it not, and null when nobody knows.
    /// </param>
    /// <param name="nowUtc">The clock.</param>
    /// <remarks>
    /// <para>**THE RADIO SAYS SO AND THE AUDIO NEVER DOES** (HM-DEC-091,
    /// HM-DEC-147). Not the level, not the sidetone's pitch, not a change in the
    /// noise floor: each of those is a guess about the transmitter made from the
    /// thing the transmitter is drowning out.</para>
    /// <para>**AND NOT KNOWING IS NOT TRANSMITTING.** An unknown state leaves
    /// decoding running, because a decoder silenced by a link that has gone quiet
    /// is a band that reads as empty (§0.0).</para>
    /// </remarks>
    public void RadioIsTransmitting(bool? transmitting, DateTime nowUtc)
    {
        if (transmitting == true)
        {
            _transmitting = true;
            DecodingSuspended = true;
            return;
        }

        if (_transmitting)
        {
            _transmitting = false;
            _transmitEndedUtc = nowUtc;
        }

        DecodingSuspended = _transmitEndedUtc != DateTime.MinValue
            && nowUtc - _transmitEndedUtc < ResumeAfter;
    }

    private void OnSamples(in AudioChunk chunk) => Process(chunk);

    private void OnReading(ToneReading reading)
    {
        _lastSample = reading.SampleIndex;

        // **THE WINDOW STOPS HOLDING ONE SENDER'S AUDIO WHILE IT READS
        // ANOTHER'S** (HM-DEC-009). The stream keeps twelve seconds of envelope
        // and the decoder fits one speed and one stream of characters across all
        // of it, so when the tracker crosses to somebody else part-way through,
        // the reading afterwards is made over two people at once and comes out as
        // clean-looking letters neither of them sent. That happens at the exact
        // moment somebody answers a call.
        var pitch = _tracker.ToneHz;

        // **RULED OFF, AND THE MACHINERY STAYS.** It fired three times across
        // the corpus where the order that shipped it predicted nought, and every
        // one of the three was the tracker leaving a station it was reading for a
        // bin holding noise. The clear was right to fire on moves that should
        // never have been made; what is wrong is upstream of it. Fifteen decibels
        // paid 0.08 of the message in invented characters for a feature that
        // fires only on a bug, and HM-DEC-120 is the one property that has not
        // bent in four days. It comes back on when the tracker is right.
        if (ClearOnAStationChange
            && ShouldClearWindow(_lastPitchHz, pitch, _probabilistic.Last.Text.Length > 0))
        {
            _probabilistic.Restart();
            _followedAt = reading.SampleIndex;
            WindowClears++;
        }

        _lastPitchHz = pitch;

        // **THE TRACKER MOVED, SO THE WINDOW HOLDS SOMEBODY ELSE** (HM-DEC-095).
        // Nobody tunes exactly, and a signal found two or three hundred hertz
        // from where Hamlet started listening spends its first seconds being
        // measured through a filter pointed at empty band. A refinement within
        // the station being read is not a move in that sense (HM-DEC-123), which
        // is why this counts follows rather than every retune.
        if (_tracker.Follows != _lastFollows)
        {
            _lastFollows = _tracker.Follows;
            _samplesAtDiscontinuity = reading.SampleIndex;
            _hasFollowed = true;

            // **THE TRACKER'S OWN CLASSIFICATION IS NOT WHAT DECIDES THE
            // CLEAR.** `StationChanges` is left exactly as it was and nothing
            // reads it: measured last session, it fires twice on `004507` in the
            // first three seconds with nothing read and not once on the
            // two-station fixture, so it is the wrong subset in both directions.
            // What decides is the size of the move and whether anybody was being
            // read, which is the line Tim ruled and is applied below.
            _lastStationChanges = _tracker.StationChanges;
        }

        // **THE INTERLOCK IS FED BY THE DECODER THAT READS THE TEXT**
        // (HM-DEC-096 phase 3, HM-DEC-091). The tracker may not jump to another
        // part of the band while a character is part-read, because the rest of
        // that character is then assembled from a different station and comes out
        // as a letter nobody sent with clean timing.
        //
        // The removed gate answered this from the elements it had in flight, by
        // thresholding. The working decoder answers it better: it has already
        // chosen where every element and every character begins and ends, over
        // the whole window up to the newest audio, and the last segment of that
        // choice is what the newest audio is inside of. A mark or the gap between
        // two marks of one character holds the tracker; the gap between
        // characters or between words lets it go.
        //
        // **WITH NOTHING FEEDING IT THE DECODER INVENTED TEXT**: 0.11 of the
        // message at eighteen decibels where it had invented none, which is
        // HM-DEC-120 broken. **With a constant it went deaf**: a constant holds
        // every move, not only the ones inside a character, so the tracker never
        // reached a station that was not already at the operator's own pitch and
        // all four station recordings emitted nothing.

        // `FollowSpeed` chose the survey's analysis window from the fitted speed
        // and went with the same decoder. Feeding it the working decoder's own
        // speed was built and measured and is not obviously right either: that
        // speed is slower than the old clock fit's, so the window comes out
        // longer, and it cost the tone on a low duty cycle fixture and two retune
        // classifications while fixing a similar number elsewhere. Left uncalled,
        // the survey stays at its acquiring width. It is not this unit's (§12.6).

        // **HOW FAR THE TONE STANDS ABOVE THE BAND WHILE IT IS KEYED**, which is
        // not the same question as how far it stands above it on average, and the
        // difference is why real stations were being missed (HM-DEC-090).
        //
        // A station answering a call keys for a second and a half in thirty
        // seconds. Averaged across all of it, a signal fifty decibels out of the
        // noise reported minus nought point six, because for ninety-six per cent
        // of the time the bin holds nothing but noise.
        //
        // So it is a held peak: up at once, down over about ten seconds. Three
        // measurements have to agree before it counts, which is what stops one
        // burst of static setting it.
        if (!reading.HasNoise)
        {
            return;
        }

        _snrHistory[_snrWrite] = reading.SnrDb;
        _snrWrite = (_snrWrite + 1) % _snrHistory.Length;
        _snrFilled = Math.Min(_snrFilled + 1, _snrHistory.Length);

        if (_snrFilled < _snrHistory.Length)
        {
            return;
        }

        var sustained = Median(_snrHistory);

        _lastSnrDb = double.IsNaN(_lastSnrDb) || sustained > _lastSnrDb
            ? sustained
            : _lastSnrDb - SnrDecayDbPerHop;

        // Opens high and closes low, so a marginal signal is not dropped in the
        // quiet parts of its own message (HM-DEC-090).
        _toneLatched = _lastSnrDb >= (_toneLatched
            ? CwDecodeReport.ToneReleaseDb
            : CwDecodeReport.ToneThresholdDb);
    }

    /// <summary>The middle of five, without allocating.</summary>
    private static double Median(double[] values)
    {
        Span<double> copy = stackalloc double[values.Length];

        values.CopyTo(copy);
        copy.Sort();

        return copy[copy.Length / 2];
    }
}
