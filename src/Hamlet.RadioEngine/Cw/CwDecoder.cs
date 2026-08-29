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

    private bool _asserted;

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
        : this(sampleRate, expectedToneHz, null, null)
    {
    }

    /// <summary>Listen, with the two constants a sweep needs to vary.</summary>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="expectedToneHz">Where to point the bank before anything is measured.</param>
    /// <param name="integratorHz">
    /// The integrator's equivalent noise bandwidth, or null for
    /// <see cref="CwProbabilisticDecoder.IntegratorBandwidthHz"/>.
    /// </param>
    /// <param name="confirmWithinSurveys">
    /// How far back a candidate may look for its second agreeing survey, or null
    /// for <see cref="CwToneTracker.ConfirmWithinSurveys"/>.
    /// </param>
    /// <remarks>
    /// **NOTHING IN THE APPLICATION PASSES EITHER OF THESE.** They exist so a
    /// constant can be swept through the whole decoder and judged by the
    /// characters it produces, which is the only judge this project accepts for
    /// a number that decides what the display asserts (§0.0). A width measured
    /// through the offline envelope alone is a fact about that envelope
    /// (HM-DEC-119).
    /// </remarks>
    public CwDecoder(
        int sampleRate,
        double expectedToneHz,
        double? integratorHz,
        int? confirmWithinSurveys)
    {
        SampleRate = Math.Max(1_000, sampleRate);
        _tracker = new CwToneTracker(
            SampleRate, expectedToneHz, confirmWithinSurveys);
        _onReading = OnReading;
        _probabilistic = new CwProbabilisticStream(
            SampleRate,
            integratorHz ?? CwProbabilisticDecoder.IntegratorBandwidthHz);

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
    /// <remarks>
    /// **`ToneHz` IS THE PITCH THE DECODE ACTUALLY USED AND NOT THE TRACKER'S**
    /// (Tim's ruling of 2026-08-28). It used to be `_tracker.ToneHz` throughout,
    /// which was the same number until the ranking started supplying the mixdown;
    /// leaving it there would have the sheet, the duty line and the panel all
    /// report one pitch while the letters on the screen were read at another.
    /// **That is the `tonePeak` fault a third time** (HM-DEC-111): a figure that
    /// is not about the thing beside it.
    /// </remarks>
    public CwDecodeReport Report => new(
        Tap.Level,
        MixdownToneHz,
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
        PitchWasMeasured: _tracker.HasMeasuredPitch,
        PitchWasAsserted: _asserted,
        PitchChoice: _asserted
            ? CwPitchChoice.OperatorAssertion
            : _ranked.Ranked && double.IsNaN(_lockedToneHz)
                ? CwPitchChoice.Ranked
                : _tracker.PitchChoice,
        Rank: _ranked.Ranked && double.IsNaN(_lockedToneHz) ? _ranked : null);

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
    /// The operator has moved the dial, so everything measured about the old
    /// frequency stops being a claim about what is on the air.
    /// </summary>
    /// <remarks>
    /// <para>**A HELD PITCH DOES NOT OUTLIVE ITS EVIDENCE** (Tim's ruling of
    /// 2026-08-26; HM-DEC-009's principle). The tracker keeps the last pitch it
    /// actually measured and mixes at it whenever the survey's three seconds of
    /// history run dry, which is most of the time between characters on a slow
    /// sender — that hold is what made the W1AW nights work and it is untouched
    /// while the radio stays put.</para>
    /// <para>**WHAT IT COULD NOT DO WAS LET GO.** On 2026-08-26 the operator
    /// tuned to 14.0275 MHz and the sidecar written there reported a pitch of
    /// 300 Hz measured twenty-four minutes and one QSY earlier, from audio that
    /// no longer existed. The decoder mixed at 300 while the station sat above
    /// 400, so the window guard saw two tenths against a bar of 1.40 and rightly
    /// refused — a correct refusal of a demodulation at a number nobody was
    /// keying at.</para>
    /// <para>**IT IS THE FREQUENCY AND NOT A TIMER**, because a measurement's
    /// evidence is gone the moment the dial moves and is not gone at all while
    /// it does not, however long that is. A station is entitled to pause.</para>
    /// <para>The held peak goes with it for the same reason: a figure whose own
    /// caveat says it is not about this recording must not survive into a
    /// recording of somewhere else.</para>
    /// </remarks>
    public void Retuned()
    {
        // **AN ASSERTION IS ABOUT A FREQUENCY TOO.** The operator said he could
        // hear a station on the frequency he was on; the dial has moved and that
        // sentence is no longer about anything.
        Unlock();

        _lastMeasuredToneHz = double.NaN;

        // **THE HELD PEAK GOES WITH IT, FOR THE SAME REASON AND NO OTHER.** It
        // rises at once and falls about a decibel a second (HM-DEC-090), so it
        // survives a station's gaps by design and it survived this QSY by
        // accident: the sheet written on 14.0275 MHz reported 50.2 dB, a peak
        // measured on another frequency. Its own caveat already says it is not a
        // figure about this recording; carrying it here made it not a figure
        // about this frequency either, which is a claim nothing on the sheet
        // qualified.
        _lastSnrDb = double.NaN;

        // **AND THE RANKING GOES WITH THEM, FOR THE SAME REASON AND NO OTHER.**
        // A pitch chosen because it read best on the old frequency is not a
        // finding about this one, and holding it would point the mixer at a
        // number whose whole evidence is audio that no longer exists.
        _ranked = CwPitchRank.None;
        _rankedAtSample = long.MinValue;
        _belowGateSince = long.MinValue;

        // **AND THE READING ITSELF, WHICH USED TO SURVIVE THE MOVE.** Clearing
        // the pitch and leaving the twelve-second window full of the last
        // station's audio leaves the decoder fitting a speed to one frequency
        // and demodulating another. The speed hypothesis, the settled mark and
        // the envelope all live in the stream, so restarting it is what makes
        // "the state it has when it first begins listening" true rather than
        // nearly true (Tim's ruling of 2026-08-29).
        _probabilistic.Restart();

        // **THE COUNTERS ARE ABOUT A FREQUENCY TOO.** A sidecar written after a
        // QSY reported elements and characters accumulated somewhere else, which
        // is the same defect as the held peak and was missed with it: the sheet
        // said what Hamlet had done that evening while every other field on it
        // described this frequency (§0.0.1).
        _charactersEmitted = 0;
        _charactersUnsure = 0;
        _elementsResolved = 0;

        // What the tracker had concluded about following a station is a fact
        // about the station it was following.
        _toneLatched = false;
        _hasFollowed = false;
        _lastFollows = 0;
        _lastPitchHz = double.NaN;
        _reReadAt = double.NaN;
        _lastMeasuredForReRead = double.NaN;

        Array.Clear(_snrHistory);
        _snrWrite = 0;
        _snrFilled = 0;

        _tracker.Forget();
    }

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
    public void Unlock()
    {
        _lockedToneHz = double.NaN;
        _asserted = false;
    }

    /// <summary>
    /// True while the pitch being decoded at was chosen by the operator saying
    /// he can hear a station, rather than found by the survey.
    /// </summary>
    /// <remarks>
    /// **NO CAPTURE MAY EVER IMPLY HAMLET FOUND WHAT A HUMAN FOUND** (§0.0).
    /// <see cref="CwDecodeReport.PitchWasMeasured"/> stays false throughout, so
    /// every sheet and every panel that already asks the honest question keeps
    /// getting the honest answer. This says the separate thing: not that the
    /// pitch is unmeasured, but who supplied it.
    /// </remarks>
    public bool PitchWasAsserted => _asserted;

    /// <summary>
    /// Whether <see cref="CwJointCutter"/> decides where characters are cut.
    /// </summary>
    /// <remarks>
    /// Behind `AppSettings.UseJointDecoder` in the application, by Tim's ruling
    /// of 2026-08-27: the operator is at the radio and a switch he can throw is
    /// worth more than a change he cannot compare against.
    /// </remarks>
    public bool UseJointCutter
    {
        get => _probabilistic.UseJointCutter;
        set => _probabilistic.UseJointCutter = value;
    }

    /// <summary>
    /// The operator says he can hear a station; decode at the loudest bin in the
    /// band and hold it.
    /// </summary>
    /// <returns>The pitch taken, or NaN where the band held nothing to take.</returns>
    /// <remarks>
    /// <para>**HM-DEC-095 IS NOT AMENDED AND THIS IS WHY.** That ruling forbids
    /// Hamlet choosing a note by how loud it is, because loudness is not evidence
    /// of keying — a carrier is louder than a station and says nothing. It does
    /// not forbid the operator supplying the evidence of keying himself. He is
    /// the one detector in this system that has never been wrong about whether
    /// somebody is sending; what he cannot do is name the frequency to a hertz,
    /// and that is exactly what the survey can do. **He supplies the keying and
    /// Hamlet supplies the number.**</para>
    /// <para>**IT TAKES THE LOUDEST BIN AND NOT THE BEST KEYING CANDIDATE**,
    /// because after six units of measurement there is no keying candidate to
    /// take: admission refuses every station in this corpus, which is the fault
    /// this route exists to get around rather than to wait for.</para>
    /// <para>**AND IT BYPASSES ADMISSION RATHER THAN LOOSENING IT.** The
    /// automatic path is untouched, so the empty band still produces nothing
    /// when nobody has pressed anything. An operator who presses this on a dead
    /// frequency gets whatever the audio contains, which is his choice to
    /// make.</para>
    /// <para>Released by <see cref="Unlock"/>, and by <see cref="Retuned"/> when
    /// the dial moves, because a pitch asserted on one frequency is not evidence
    /// about the next one.</para>
    /// </remarks>
    public double AssertStation()
    {
        // **THE STRONGEST *KEYED* BIN, WHICH IS NOT THE LOUDEST ONE.** Taking the
        // loudest was built first and measured on 2026-08-26: it lands 121 Hz off
        // on `cw-2026-08-26-125941`, 118 off on `cw-2026-08-22-014113` and 100 off
        // on `cw-2026-08-25-012823`. HM-DEC-095 said exactly this — a carrier is
        // louder than a station and says nothing — and the ruling's own wording
        // says keyed.
        //
        // **THE SWEEP IS THE ONE THE CAPTURE SHEET ALREADY REPORTS**, scoring
        // each pitch by how much of the stretch was spent keyed down for an
        // element's length and how many of those key-downs were elements rather
        // than a gate chattering. It shares nothing with the survey's admission,
        // which is the point: admission is what has refused every station in this
        // corpus for six units, and this route exists to get around it rather
        // than to wait for it.
        var audio = Tap?.Snapshot();

        if (audio is null)
        {
            return double.NaN;
        }

        var found = KeyingEnvelope.Best(audio);

        if (found is not { } sighting)
        {
            // Nothing in the band looks keyed at any pitch. Refusing is the
            // honest answer; pointing at the bank centre and calling it his
            // choice would put Hamlet's own default behind his assertion.
            return double.NaN;
        }

        AssertAt(sighting.ToneHz);

        return sighting.ToneHz;
    }

    /// <summary>Decode at a pitch the operator supplied, and hold it.</summary>
    /// <param name="toneHz">The pitch.</param>
    /// <remarks>
    /// Separated from <see cref="AssertStation"/> so the choosing and the holding
    /// can be measured apart, and so a caller that already knows the pitch — a
    /// test, or a future control that lets him type one — need not go through the
    /// sweep to use it.
    /// </remarks>
    public void AssertAt(double toneHz)
    {
        if (double.IsNaN(toneHz))
        {
            return;
        }

        _lockedToneHz = toneHz;
        _asserted = true;
    }

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

        // **THE AUDIO IS WALKED A HOP AT A TIME, WHATEVER SIZE IT ARRIVED IN.**
        // What follows sets the mixer's pitch from the tracker and then mixes the
        // whole chunk down at that one pitch, so with a chunk four hops long the
        // first three hops are mixed at a pitch the tracker only reached at the
        // end of the fourth. **The decode was a function of the sound card's
        // buffer size**, which is not a fact about the audio.
        //
        // Measured on `cw-2026-08-22-032113`: fed 240 samples at a time the
        // decoder tracks 650 Hz, fed 960 it tracks 500, and the text differs with
        // it. The application feeds 960 through `BufferedAudioSource` and the
        // floors harness feeds 240, so the suite and the operator were reading
        // two different decoders (§12.5, HM-DEC-119).
        //
        // Stepping at the tracker's own hop makes the two agree by construction:
        // the pitch handed to the mixer is the pitch that was in force for the
        // audio being mixed. A chunk that is not a whole number of hops leaves a
        // remainder, which is handed over as it is — both the tracker and the
        // mixer buffer internally, so alignment is theirs to keep and this only
        // decides how often the pitch is refreshed.
        var hop = _tracker.HopSamples;

        for (var offset = 0; offset < chunk.Samples.Length; offset += hop)
        {
            var take = Math.Min(hop, chunk.Samples.Length - offset);

            Step(
                chunk.Samples.Slice(offset, take),
                chunk.FirstSampleIndex + offset);
        }
    }

    /// <summary>One hop of audio, through the tracker and then the decoder.</summary>
    /// <param name="samples">The hop.</param>
    /// <param name="firstSampleIndex">Where it sits on the audio clock.</param>
    private void Step(ReadOnlySpan<float> samples, long firstSampleIndex)
    {
        _tracker.Process(samples, firstSampleIndex, _onReading);

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

            ReadHeldAudioAgain();
        }

        MaybeRank(firstSampleIndex + samples.Length);

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
        //
        // **AND THE RANKING SITS BETWEEN THE LOCK AND THE LAST MEASURED PITCH**
        // (Tim's ruling of 2026-08-28). The operator still wins: a pitch he
        // supplied is evidence of keying from the one detector here that has
        // never been wrong about it. Below him, a pitch chosen by decoding at
        // every candidate and keeping the best beats one the survey happened to
        // admit — measured at 34 of 44 captures against 1 (`CwPitchRanking`).
        _probabilistic.ToneHz = MixdownToneHz;
        _probabilistic.Process(samples);

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

    /// <summary>
    /// Read the audio the decoder is still holding again, at a pitch it has
    /// since measured.
    /// </summary>
    /// <remarks>
    /// <para>**THE FIRST SECONDS OF EVERY STATION ARE DEMODULATED AT A GUESS.**
    /// The stream mixes each sample as it arrives, at whatever the tracker
    /// believed then, and until the survey admits a candidate the tracker answers
    /// with the middle of the bank it is pointed at. Measured across this
    /// repository's thirty-six captures, the first measured pitch lands two to
    /// seven seconds in on half of them, and the window is still holding every
    /// sample since the start when it does — mixed at a number nobody keyed at.</para>
    /// <para>**WHAT IT IS WORTH WAS MEASURED BEFORE IT WAS BUILT.** Read whole at
    /// the station's own note instead of the operator's 600 Hz,
    /// `cw-2026-08-22-032113` gives back 22 characters of its adjudicated line
    /// rather than 4, `032012` 43 rather than 22 and `032050` 24 rather than 17.
    /// Hamlet cannot know the note in advance; it knows it a few seconds in, and
    /// nothing re-read what it already had.</para>
    /// <para>**ONCE, ON A MEASURED PITCH, AND ONLY BACKWARD.** A re-read at a
    /// bank centre would be decoding at a number nobody keyed at, which is the
    /// fault this exists to remove rather than to repeat. It fires at most once
    /// for each pitch it settles on, so a tracker walking a few hertz cannot
    /// replay the window every hop.</para>
    /// <para>**AND IT ASKS THE TAP FOR THE AUDIO THE STREAM HAS SEEN**, not for
    /// the last N samples. The tap takes a whole chunk at once and this walks it
    /// a hop at a time, so "the last N" would hand the replay hops from the
    /// future and make it depend on the size of the chunk it fired inside —
    /// reintroducing the two-decoders fault `OneDecoderNotTwoTests` closed.</para>
    /// </remarks>
    private void ReadHeldAudioAgain()
    {
        var measured = _tracker.ToneHz;

        // **THE SAME TWO-READINGS RULE THE TRACKER ITSELF OBEYS** (HM-DEC-095).
        // The first pitch the survey admits is not always the one it settles on:
        // on `cw-2026-08-18-004507` it answers 475 Hz two seconds in and 500 a
        // moment later, and 500 is where the station is. Replaying at the first
        // answer is replaying at a number that is about to be corrected, so this
        // waits for two readings that agree to within the survey's own
        // resolution before it replays anything.
        var confirmed = !double.IsNaN(_lastMeasuredForReRead)
            && Math.Abs(measured - _lastMeasuredForReRead)
                < CwToneTracker.CoarseSpacingHz;

        _lastMeasuredForReRead = measured;

        if (!confirmed)
        {
            return;
        }

        if (Math.Abs(measured - _reReadAt) < CwToneTracker.CoarseSpacingHz)
        {
            // Already read the window at this pitch, or near enough that the
            // survey could not have told the two apart.
            return;
        }

        // **ONLY WHILE THERE IS A FIRST EMISSION LEFT TO GET RIGHT.** That is
        // what the re-read is for, and once characters have been announced,
        // replaying can only risk what is already on the screen — the settled
        // mark stops them being said twice, so a replay that reads the same
        // stretch worse costs the tip and buys nothing back.
        //
        // **MEASURED, AND IT IS THE DIFFERENCE BETWEEN A GAIN AND A TRADE.**
        // Without this condition the corpus moves 153 adjudicated characters to
        // 164 and two of them go backwards: `cw-2026-08-22-031905` from 11 to 10
        // and `032129` from 9 to 7, both re-read after their tracker had already
        // announced 7 and 21 characters, and `032129`'s measured pitch is 650 for
        // a station at 500. With it the corpus reaches the same 164 and nothing
        // goes backwards at all.
        if (_probabilistic.SettledCharacters > 0)
        {
            return;
        }

        var held = _probabilistic.HeldHops;

        if (held <= 0 || double.IsNaN(_probabilistic.MixedAtHz))
        {
            return;
        }

        if (_probabilistic.MixedSpreadFrom(measured) < CwToneTracker.CoarseSpacingHz)
        {
            // Every hop in the window was already mixed within one bin of the
            // measured pitch, so a replay would be the same demodulation twice.
            _reReadAt = measured;
            return;
        }

        var samples = held * _tracker.HopSamples;
        var audio = Tap.Window(_probabilistic.SamplesSeen - samples, samples);

        if (audio is null)
        {
            return;
        }

        _reReadAt = measured;
        _probabilistic.ReadAgain(audio.Samples, measured);
    }

    private double _reReadAt = double.NaN;

    private double _lastMeasuredForReRead = double.NaN;

    /// <summary>What the ranking last chose, or <see cref="CwPitchRank.None"/>.</summary>
    private CwPitchRank _ranked = CwPitchRank.None;

    /// <summary>Where on the audio clock the ranking last ran.</summary>
    private long _rankedAtSample = long.MinValue;

    /// <summary>
    /// Where on the audio clock the reading first fell below the gate, or
    /// <see cref="long.MinValue"/> while it is above it.
    /// </summary>
    private long _belowGateSince = long.MinValue;

    /// <summary>How many times the ranking has run.</summary>
    /// <remarks>
    /// **A PASS NOBODY COUNTS IS A PASS NOBODY CAN SAY RAN** (§0.0.1). The whole
    /// claim of the ruling is that this happens once on tune-in rather than
    /// continuously, and that is not checkable from outside without this.
    /// </remarks>
    public int Rankings { get; private set; }

    /// <summary>What the ranking chose, and what it beat.</summary>
    public CwPitchRank Ranked => _ranked;

    /// <summary>Whether the ranking supplies the mixdown pitch.</summary>
    /// <remarks>
    /// <para>**OFF, AND THE MACHINERY STAYS** — `ClearOnAStationChange`'s
    /// precedent, and for the same kind of reason. Unit 044 built the ranking to
    /// drive the live decode and measured what that costs: **two adjudicated
    /// anchors lose their callsigns.** `cw-2026-08-17-013347` falls from
    /// `VA3VRR` to nothing, and `cw-2026-08-24-012403` from `DE KD0UN KD0UN K`
    /// to `DE XD0UN KD0`. The unit's own acceptance requires all twelve anchors
    /// green, so it does not go on by this session's hand.</para>
    /// <para>**THE TWO FAILURES HAVE DIFFERENT CAUSES AND ONLY ONE IS ABOUT THE
    /// RANKING BEING WRONG.** On `012403` the ranking picks the right bin, 450,
    /// and the station sits at 440: the candidates are the tracker's coarse grid
    /// and a ranked pitch is only ever a bin centre, while the survey
    /// interpolates to the hertz. On `013347` the opening four seconds hold no
    /// station, so every bin's floor is tiny, the common pedestal is tiny with
    /// them, and the scale invariance the pedestal exists to remove comes
    /// straight back: the winner scores **5,521,967** at 775 Hz. **A degenerate
    /// pitch looks maximally healthy**, so the collapse test that would re-rank
    /// it never fires.</para>
    /// <para>**AND THE WINDOW'S POSITION MATTERS MORE THAN ITS LENGTH**, which
    /// no measurement before this one separated. Ranking over the tail of a
    /// recording picks the station on 34 of 44 captures at four seconds and 34
    /// at twelve; ranking over the opening four seconds, which is what tune-in
    /// actually sees, picks it on **27**.</para>
    /// <para>It is a property rather than a constant so the before and the after
    /// can be measured on one build (§0.0.1); the shape is
    /// <see cref="UseJointCutter"/>'s.</para>
    /// </remarks>
    public bool RankThePitch { get; set; }

    /// <summary>The pitch the mixer is actually being run at.</summary>
    /// <remarks>
    /// **THE OPERATOR'S LOCK, THEN THE RANKING, THEN THE LAST MEASURED PITCH,
    /// THEN THE BANK.** One property rather than an expression at the call site,
    /// because the sheet has to report the pitch the decode used and the two
    /// drifting apart is the `tonePeak` fault a third time (HM-DEC-111).
    /// </remarks>
    private double MixdownToneHz
        => !double.IsNaN(_lockedToneHz) ? _lockedToneHz
            : _ranked.Ranked ? _ranked.ToneHz
            : !double.IsNaN(_lastMeasuredToneHz) ? _lastMeasuredToneHz
            : _tracker.ToneHz;

    /// <summary>
    /// How long the reading stays under the gate before the ranking runs again.
    /// </summary>
    /// <remarks>
    /// <para>**RANKING RUNS ONCE ON TUNE-IN AND AGAIN IF THE WINNER'S SCORE
    /// COLLAPSES** (Tim's ruling of 2026-08-28). It does not run continuously:
    /// that matches how the lock already behaves, costs nothing while the
    /// operator sits on a station, and still recovers when it lands wrong.</para>
    /// <para>**SIX SECONDS, AND WHAT IT IS MEASURED AGAINST IS A SENDER'S OWN
    /// GAPS.** A station pausing between overs takes the reading under the gate
    /// for a second or two, and re-ranking on that would be re-ranking
    /// continuously in all but name. Six seconds is longer than any gap inside a
    /// message at any speed this decoder considers — a word gap at eight words a
    /// minute is one second — so it is a station that has stopped rather than a
    /// station that is breathing.</para>
    /// </remarks>
    private const double CollapseSeconds = 6.0;

    /// <summary>
    /// Rank the band, on tune-in and when the reading has collapsed.
    /// </summary>
    /// <param name="atSample">Where on the audio clock this hop ends.</param>
    /// <remarks>
    /// **IT DOES NOTHING AT ALL WHILE THE OPERATOR HOLDS THE PITCH.** A lock or
    /// an assertion is his answer, and re-deriving one over the top of it would
    /// make the sheet's account of who chose the number false (§0.0).
    /// </remarks>
    private void MaybeRank(long atSample)
    {
        if (!RankThePitch || !double.IsNaN(_lockedToneHz))
        {
            return;
        }

        var window = (int)(CwPitchRanking.WindowSeconds * SampleRate);

        if (atSample < window)
        {
            // Less audio has arrived than the ranking reads. Nothing is chosen
            // and the tracker keeps steering, which is what it did before.
            return;
        }

        if (_ranked.Ranked && !HasCollapsed(atSample, window))
        {
            return;
        }

        var audio = Tap.Window(atSample - window, window);

        if (audio is null)
        {
            return;
        }

        var ranked = CwPitchRanking.Rank(audio.Samples, audio.SampleRate);

        if (!ranked.Ranked)
        {
            // Nothing could be ranked from this stretch. Keeping whatever was
            // already in force is right: a refusal is not a finding that the
            // previous answer was wrong.
            return;
        }

        _ranked = ranked;
        _rankedAtSample = atSample;
        _belowGateSince = long.MinValue;
        Rankings++;
    }

    /// <summary>Whether the reading has been under the gate long enough to re-rank.</summary>
    /// <param name="atSample">Where on the audio clock this hop ends.</param>
    /// <param name="window">How many samples the ranking reads.</param>
    /// <returns>True where the winner's score has collapsed.</returns>
    private bool HasCollapsed(long atSample, int window)
    {
        if (_probabilistic.Last.LikelihoodRatio >= CwProbabilisticDecoder.Gate)
        {
            _belowGateSince = long.MinValue;

            return false;
        }

        if (_belowGateSince == long.MinValue)
        {
            _belowGateSince = atSample;

            return false;
        }

        if (atSample - _belowGateSince < CollapseSeconds * SampleRate)
        {
            return false;
        }

        // **AND NEVER TWICE OVER THE SAME AUDIO.** A ranking that ran on this
        // window already looked at exactly these samples and would reach exactly
        // this answer again, so re-running before the window has turned over
        // spends the whole sweep to learn nothing.
        return atSample - _rankedAtSample >= window;
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
