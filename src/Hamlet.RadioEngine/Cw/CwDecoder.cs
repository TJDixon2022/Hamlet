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

    /// <summary>The fldigi port reading the same hops, or null where only ours reads.</summary>
    private readonly CwSecondReader? _second;

    /// <summary>Creates a decoder.</summary>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="expectedToneHz">
    /// The operator's CW pitch, as a place to start looking. The tracker hunts
    /// either side of it, since nobody tunes exactly.
    /// </param>
    /// <param name="secondReader">
    /// Whether the fldigi port reads the same samples beside ours and every
    /// character passes through <see cref="CwArbiter"/> (HM-REQ-120; work
    /// instruction 465). The application sets it; a harness that measures our
    /// decoder alone leaves it off.
    /// </param>
    public CwDecoder(int sampleRate, double expectedToneHz = 600, bool secondReader = false)
    {
        SampleRate = Math.Max(1_000, sampleRate);
        _tracker = new CwToneTracker(SampleRate, expectedToneHz);
        _onReading = OnReading;
        _probabilistic = new CwProbabilisticStream(SampleRate);
        _second = secondReader ? new CwSecondReader(SampleRate) : null;

        // **EVERY SEAM THE CW TAB READS PASSES THROUGH THE ARBITER** (HM-REQ-121,
        // work instruction 465). With the second reader on, a settled character
        // is emitted only once the port has read the same samples, as the arbiter
        // decides it; the leading edge is arbitrated against what the port has
        // printed so far and stays provisional, as it always was.
        _probabilistic.CharacterSettled += c =>
        {
            foreach (var emitted in Arbitrated(c))
            {
                Settle(emitted);
            }
        };

        // What the runs read is what reaches the screen while they are read (work instruction 490).
        _runs.CharacterRead += c =>
        {
            if (RunsRead)
            {
                Emit(c);

                // And it is what the decoder decoded: the timing-only path, which raised this, now
                // reaches no surface, and the app times its quiet offer and the evidence that the
                // operator is working Morse (HM-DEC-149) from it (work instruction 493).
                if (!c.IsWordGap)
                {
                    CharacterDecoded?.Invoke(c);
                }
            }
        };

        _probabilistic.LeadingEdgeChanged += e =>
        {
            _lastEdge = e;

            // Nor its leading edge: a letter appears only once its run exists (work instruction 490).
            if (RunsRead)
            {
                return;
            }

            var edge = _second is null ? e : ArbitratedEdge(e);

            // No detection, no letters (R97), and a letter needs blocks (R99): see KeyingGate.
            // **PRINTED STAYS PRINTED** (R100): while keying is false nothing is offered, not even
            // an empty edge, because an empty offer takes the tip already on the screen away.
            if (KeyingGated)
            {
                if (!_gateOpen)
                {
                    return;
                }

                edge = edge.Where(Admitted).ToList();
                _shownEdge = edge;
            }
            else if (BlocksGated)
            {
                edge = edge.Where(Admitted).ToList();
            }

            LeadingEdge?.Invoke(edge);

            foreach (var character in edge)
            {
                CharacterDecoded?.Invoke(character);
            }
        };
    }

    /// <summary>The port's readings not yet paired with one of ours, oldest first.</summary>
    private readonly List<CwSecondReading> _pendingSecond = new();

    /// <summary>Our leading edge as the stream last handed it over, unarbitrated.</summary>
    private IReadOnlyList<CwCharacter> _lastEdge = Array.Empty<CwCharacter>();

    /// <summary>Spans only the port read, emitted or not, since the decoder was made.</summary>
    public int SecondOnlySpans { get; private set; }

    /// <summary>
    /// Each reading the port completes, as it is harvested and before the arbiter
    /// meets it: the port alone as it reads live, for the sheet and for scoring it
    /// beside the arbitrated transcript (HM-REQ-128; work instruction 466). Never
    /// the CW tab's (HM-REQ-121).
    /// </summary>
    public event Action<CwSecondReading>? SecondRead;

    // The port's readings, raised and then held for the arbiter.
    private void Pend(IReadOnlyList<CwSecondReading> readings)
    {
        foreach (var s in readings)
        {
            SecondRead?.Invoke(s);
        }

        _pendingSecond.AddRange(readings);
    }

    private void Settle(CwCharacter c)
    {
        // While runs are read, the timing-only path's letters do not reach the screen (work
        // instruction 490).
        if (RunsRead)
        {
            return;
        }

        // No detection, no letters (R97); a letter needs blocks (R99); and nothing already
        // promoted from the screen is settled a second time (R100): see KeyingGate.
        if (!Admitted(c) || (KeyingGated && c.At.TotalSeconds <= _promotedThrough))
        {
            return;
        }

        Emit(c);
    }

    // What the settled pass lets out, counted as it reaches the screen.
    private void Emit(CwCharacter c)
    {
        if (!c.IsWordGap)
        {
            _lastEmittedAt = Math.Max(_lastEmittedAt, c.At.TotalSeconds);
        }

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
    }

    /// <summary>
    /// One of our settled characters through the arbiter, with any span only the
    /// port read that it has now passed.
    /// </summary>
    /// <remarks>
    /// <para>**THE SPAN RULE IS <see cref="CwArbiter.SameSpan"/>, TAKEN AS IT
    /// ARRIVES** (465 DECIDED (2)): the port reading on this character's span with
    /// the largest overlap, unless a character still on our leading edge overlaps
    /// that reading more, in which case it is left for that one, as the arbiter's
    /// largest-overlap-first rule over the whole stream would leave it.</para>
    /// <para>**A PORT READING OUR SETTLED PASS HAS GONE PAST IS ONE-SIDED.** Ours
    /// settles in order, so a port reading that ends where this character starts
    /// can meet none of ours; it is emitted only where the port votes.</para>
    /// </remarks>
    private IReadOnlyList<CwCharacter> Arbitrated(CwCharacter c)
    {
        if (_second is null)
        {
            return new[] { c };
        }

        // HM-REQ-128: under the port alone, ours' word boundaries are not emitted; the port's are.
        if (c.IsWordGap)
        {
            return Switch == CwArbitrationSwitch.PortAlone ? Array.Empty<CwCharacter>() : new[] { c };
        }

        var emitted = new List<CwCharacter>();
        var span = CwArbiter.SpanOf(c);
        var from = double.IsFinite(span.Start) ? span.Start : span.End;

        for (var i = 0; i < _pendingSecond.Count;)
        {
            var s = _pendingSecond[i];

            if (!s.HasSpan || s.End <= from)
            {
                _pendingSecond.RemoveAt(i);
                SecondOnlySpans++;
                OneSided(s, emitted);

                continue;
            }

            i++;
        }

        var best = -1;
        var bestOverlap = 0.0;

        for (var i = 0; i < _pendingSecond.Count; i++)
        {
            var s = _pendingSecond[i];

            if (CwArbiter.SameSpan(span, (s.Start, s.End)) is not { } overlap || overlap <= bestOverlap)
            {
                continue;
            }

            var later = _lastEdge.Any(x => !x.IsWordGap && x.At > c.At
                && CwArbiter.SameSpan(CwArbiter.SpanOf(x), (s.Start, s.End)) is { } theirs && theirs > overlap);

            if (!later)
            {
                best = i;
                bestOverlap = overlap;
            }
        }

        CwSecondReading? partner = null;

        if (best >= 0)
        {
            partner = _pendingSecond[best];
            _pendingSecond.RemoveAt(best);
        }

        if (Switch != CwArbitrationSwitch.PortAlone)
        {
            emitted.Add(CwArbiter.Decide(c, partner, Vote, Switch));

            return emitted;
        }

        // The port alone: its readings in the order it read them. Ours with no
        // partner is not emitted; a partner goes out after any earlier port reading.
        if (partner is not null)
        {
            foreach (var s in _pendingSecond.Where(s => s.HasSpan && s.Start < partner.Start).ToList())
            {
                _pendingSecond.Remove(s);
                SecondOnlySpans++;
                OneSided(s, emitted);
            }

            if (CwArbiter.GapBefore(partner) is { } gap)
            {
                emitted.Add(gap);
            }

            emitted.Add(CwArbiter.Alone(partner, c, Vote));
        }

        return emitted;
    }

    // A span only the port read, under the switch in force; under the port alone with the space it printed before it.
    private void OneSided(CwSecondReading s, List<CwCharacter> emitted)
    {
        if (CwArbiter.DecideAlone(s, Vote, Switch).Emitted is not { } alone)
        {
            return;
        }

        if (Switch == CwArbitrationSwitch.PortAlone && CwArbiter.GapBefore(s) is { } gap)
        {
            emitted.Add(gap);
        }

        emitted.Add(alone);
    }

    /// <summary>Our leading edge through the arbiter against what the port has printed so far; nothing is taken from the pending readings.</summary>
    /// <remarks>Under the port alone (HM-REQ-128), the edge shows the port's reading of each of our provisional spans it has read, and nothing of ours.</remarks>
    private IReadOnlyList<CwCharacter> ArbitratedEdge(IReadOnlyList<CwCharacter> edge)
        => edge.Select(x =>
            {
                if (x.IsWordGap)
                {
                    return Switch == CwArbitrationSwitch.PortAlone ? null : x;
                }

                var span = CwArbiter.SpanOf(x);
                var partner = _pendingSecond
                    .Select(s => (s, Overlap: CwArbiter.SameSpan(span, (s.Start, s.End))))
                    .Where(p => p.Overlap is not null)
                    .OrderByDescending(p => p.Overlap)
                    .Select(p => p.s)
                    .FirstOrDefault();

                return Switch != CwArbitrationSwitch.PortAlone ? CwArbiter.Decide(x, partner, Vote, Switch)
                    : partner is null ? null
                    : CwArbiter.Alone(partner, x, Vote);
            })
            .OfType<CwCharacter>()
            .ToList();

    /// <summary>Samples per second.</summary>
    public int SampleRate { get; }

    /// <summary>The second reader, or null where ours reads alone.</summary>
    public CwSecondReader? SecondReader => _second;

    /// <summary>Who votes on the condition in force (HM-REQ-124): <see cref="CwVoteTable.Live"/> unless a harness says otherwise.</summary>
    public CwVote Vote { get; set; } = CwVoteTable.Live;

    /// <summary>
    /// What is emitted on the condition in force (HM-REQ-128): <see cref="CwSwitchTable.Live"/>
    /// unless a harness says otherwise. Read where the port's readings meet ours; never shown on the CW tab.
    /// </summary>
    public CwArbitrationSwitch Switch { get; set; } = CwSwitchTable.Live;

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
    /// The pitch the decoder is mixing at now: the operator's lock, the detector's pitch while it
    /// says keying while DetectorSteersPitch is on, or the tracker's, in that order (work instructions 488, 489).
    /// </summary>
    /// <remarks>
    /// **NOT <see cref="CwDecodeReport.ToneHz"/>**, which is the tracker's own pitch. The panel's
    /// *mixing* number was fed that, so on 2026-09-28 it said *mixing 584* while the decoder may
    /// have been mixing somewhere else; the number beside *tone* is this one from work instruction
    /// 488 on.
    /// </remarks>
    public double MixingHz => _probabilistic.ToneHz;

    /// <summary>
    /// The pitch of the station whose letters reach the screen: the run reader's printed sender
    /// while runs are read, NaN when it is printing nobody; the timing-only path's mixing pitch
    /// otherwise (work instruction 493).
    /// </summary>
    /// <remarks>
    /// **ONE DECODER, ONE TRUTH** (R106, HM-DEC-198). While runs are read, <see cref="MixingHz"/> is
    /// the pitch of a path whose letters reach no surface, and a panel reading it would say
    /// something untrue. Safe to read from the screen's thread.
    /// </remarks>
    public double PrintingHz => RunsRead ? _runs.StationPitchHz : MixingHz;

    /// <summary>
    /// The pitch of the sender the run reader is printing, or NaN when it prints nothing or runs are not read
    /// (work instruction 519): what the detector's reading follows. Safe to read from any thread.
    /// </summary>
    public double RunsPrintingHz => RunsRead ? _runs.StationPitchHz : double.NaN;

    /// <summary>
    /// The pitch of the sender the run reader has qualified and is waiting out its first word gap to print, or NaN (work
    /// instruction 535): with <see cref="RunsPrintingHz"/>, all the hold-still light may claim. Safe to read from any thread.
    /// </summary>
    public double RunsWaitingHz => RunsRead ? _runs.WaitingPitchHz : double.NaN;

    /// <summary>
    /// What the shape side held at the end of its last batch (work instruction 537): the printed sender, its letters and
    /// marks, and every sender the gate holds; nothing where runs are not read. Safe to read from any thread.
    /// </summary>
    public CwShapeSideReading ShapeSide => RunsRead ? _runs.ShapeReading : CwShapeSideReading.Nothing;

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
        PitchProof: _tracker.PitchProof,
        SpeedProof: SpeedProof,
        WordsPerMinute: WordsPerMinute);

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

    /// <summary>
    /// True while the operator is in a Digital mode, so no CW is decoded.
    /// </summary>
    /// <remarks>
    /// **865e66d8's GATE, CARRIED ACROSS THE RESTORE** (work instruction 392, a
    /// seam for today's application). In Digital the CW decode is arithmetic
    /// nobody reads; the audio still reaches the tap and the decode takes the
    /// suspended arm below. It is set from the mode, never inferred from the
    /// audio.
    /// </remarks>
    public bool DigitalMode { get; set; }

    /// <summary>How many chunks of audio were dropped rather than decoded.</summary>
    public long SuspendedChunks => _suspendedChunks;

    /// <summary>Chunks a decode queue could not hold. This decoder has no queue.</summary>
    /// <remarks>
    /// **NOUGHT BECAUSE THERE IS NO QUEUE TO DROP FROM** (work instruction 392, a
    /// seam for today's application). This decoder reads on the thread that
    /// delivers the audio, as it did on 2026-08-25; the queue came on 2026-09-03
    /// and HEAD's own figure was nought whenever it was not running.
    /// </remarks>
    public long DecodeQueueDroppedChunks => 0;

    /// <summary>Samples those chunks carried. This decoder has no queue.</summary>
    public long DecodeQueueDroppedSamples => 0;

    /// <summary>The operator has moved the dial.</summary>
    /// <remarks>
    /// **THE LOCK GOES, BECAUSE A LOCK IS ABOUT A FREQUENCY** (work instruction
    /// 392, a seam for today's application). That is the part of HEAD's
    /// `Retuned` this decoder has the state for; the held pitch and peak it also
    /// dropped arrived after this decoder was written.
    /// </remarks>
    public void Retuned() => Unlock();

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

    /// <summary>What can be said about the speed now (HM-REQ-034).</summary>
    /// <remarks>
    /// <para>**PROVED ONLY WHERE <see cref="WordsPerMinute"/> NAMES A NUMBER, THE
    /// DIT BEHIND IT WAS MEASURED, AND ITS KEYING IS STILL ARRIVING** (work
    /// instruction 451). The number is the guard's above. Measured is the
    /// stream's (<see cref="CwProbabilisticStream.UnitWasMeasured"/>): the
    /// estimator's dit or the marks' overrule, not the grid's winner. Still
    /// arriving is the tracker's own recent span, six surveys
    /// (<see cref="CwToneTracker.KeyingRecently"/>), at a pitch within half the
    /// mixdown filter of the one being read, the line the follow above already
    /// uses for the same sender. **Six surveys and not the latest one**, because
    /// the survey does not confirm keying on every half second of a slow sender
    /// and this reading trails it (see <see cref="CwToneTracker.KeyingRecently"/>).</para>
    /// <para>**A HYPOTHESIS WHERE THE WINDOW HOLDS A READING AND ANY OF THOSE
    /// FAILS**: the clock re-acquiring, the unit won on the grid, or no keying at
    /// the pitch for six surveys, which is a speed held in a window whose sender
    /// has stopped. **None where nothing has been read**, or the gate refused the
    /// whole window, whose grid winner describes nobody.</para>
    /// <para>**IT DESCRIBES THE SPEED AND NOTHING READS IT** in the decode, the
    /// tracker or the pitch. Proved is a subset of a named number, so no speed is
    /// stated with more certainty than before, only less.</para>
    /// </remarks>
    public CwSpeedProof SpeedProof
    {
        get
        {
            var reading = _probabilistic.Last;

            if (reading.WordsPerMinute <= 0 || reading.Text.Length == 0)
            {
                return CwSpeedProof.None;
            }

            var lastKeyedHz = _tracker.LastKeyedHz;

            return WordsPerMinute is not null
                   && _probabilistic.UnitWasMeasured
                   && _tracker.KeyingRecently
                   && !double.IsNaN(lastKeyedHz)
                   && Math.Abs(lastKeyedHz - _probabilistic.ToneHz)
                      < CwProbabilisticDecoder.BandwidthHz / 2
                ? CwSpeedProof.Proved
                : CwSpeedProof.Hypothesis;
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

    private long _samplesHeard;

    /// <summary>How much audio has been handed to the decoder, on the clock <see cref="CwCharacter.At"/> uses.</summary>
    /// <remarks>
    /// **SO A SETTLED CHARACTER CAN BE PUT OVER THE BARS THAT MADE IT** (work instruction 480
    /// task 3): the training graph reads this when a character settles, and the character's
    /// end is that far behind now.
    /// </remarks>
    public TimeSpan Heard => TimeSpan.FromSeconds(Interlocked.Read(ref _samplesHeard) / (double)SampleRate);

    /// <summary>
    /// Whether the detector says somebody is keying, or null to emit as the decoder always has.
    /// </summary>
    /// <remarks>
    /// <para>**NO DETECTION, NO LETTERS** (work instruction 485, R97, HM-DEC-190). The decoder and
    /// <see cref="CwEnvelopeDetector"/> were never wired together, so the decoder went on spelling
    /// letters out of noise while the detector said nobody was there.</para>
    /// <para>**JUDGED BY THE SCREEN AT THAT MOMENT** (work instruction 486). A character reaches the
    /// transcript, the leading edge or the scope only while the detector says keying, and only if
    /// its audio was heard in the stretch that is keying now - from
    /// <see cref="CwEnvelopeDetector.KeyingSeconds"/> before the gate opened, the window in which the
    /// detector saw the bars that opened it. Work instruction 485 judged by when the audio was heard,
    /// so the last letters of an over kept landing for seconds under a panel saying no keying; the
    /// owner saw exactly that. **What this costs**: the tail of an over, whose last letters settle
    /// after the detector lets go, is dropped rather than flushed, and so is anything held from
    /// before a silence. When the gate closes, the leading edge is cleared in the same moment.
    /// Nothing about how the decoder reads is changed - only whether what it read is let out.</para>
    /// </remarks>
    public Func<bool>? KeyingGate { get; set; }

    /// <summary>The pitch the detector hears a station at while it says keying, NaN otherwise; null for none.</summary>
    /// <remarks>
    /// **THE DECODER LISTENS WHERE THE DETECTOR HEARS** (work instruction 486; unit 477's task 2,
    /// never built until now). While this answers a pitch, the decoder mixes there from the next
    /// hop, ahead of the tracker's own survey and behind only the operator's lock. The detector
    /// holds the pitch through a station's gaps, so this holds with it; when it answers NaN the
    /// tracker steers as it always has. The tracker's own choices are untouched.
    /// </remarks>
    public Func<double>? DetectorPitch { get; set; }

    /// <summary>
    /// How many blocks the detector called whose middle lies between two moments on the audio
    /// clock, in seconds; null to let a character out without asking.
    /// </summary>
    /// <remarks>
    /// <para>**A LETTER NEEDS BLOCKS** (work instruction 487, R99, HM-DEC-192). The owner: *"We
    /// should get no letters unless we have a flat-topped signal with a duration that matches CW."*
    /// With this set, a character reaches a surface only if the blocks the detector called under
    /// its span are exactly as many as its elements - one block for each dit and dah. None, fewer
    /// or more and it is not let out at all, not as a letter and not as a placeholder. The scope's
    /// drawing rule since work instruction 485, applied to emitting.</para>
    /// <para>**THE TOLERANCE IS ONE ENVELOPE WINDOW, TWO HOPS, TEN MILLISECONDS.** The detector
    /// reads each hop through a window two hops long, so it can place a block's edge up to one
    /// window from where the decoder places the same element; the character's span is widened by
    /// that much at each end and a block counts where its middle falls inside. Derived from the
    /// hop, not from any recording.</para>
    /// </remarks>
    public Func<double, double, int>? DetectorBlocks { get; set; }

    /// <summary>How far a character's span is widened at each end when its blocks are counted, in seconds.</summary>
    public const double BlockToleranceSeconds = 2 * CwProbabilisticDecoder.HopMilliseconds / 1000;

    /// <summary>Whether <see cref="DetectorPitch"/> steers where the decoder mixes; off by default.</summary>
    /// <remarks>
    /// **THE DETECTOR STOPS STEERING THE DECODER UNTIL ITS PITCH IS FIT** (work instruction 489,
    /// R102, HM-DEC-194). Unit 488 measured the detector calling a 625 Hz station's bars in the bins
    /// 50 Hz to either side two hops in three, so the decoder was pointed at a shoulder: a clean call
    /// read 5 characters where the same decoder unbound read 19. Off, the rung is the operator's lock
    /// and then the tracker, as before work instruction 486. The switch exists so it can be turned
    /// back on when the detector's pitch is fit; nothing it steered was deleted.
    /// </remarks>
    public bool DetectorSteersPitch { get; set; }

    /// <summary>Whether <see cref="KeyingGate"/> decides what is let out; off by default.</summary>
    /// <remarks>
    /// **THE DETECTOR STOPS GATING THE DECODER UNTIL ITS PITCH IS FIT** (work instruction 489, R102,
    /// HM-DEC-194): unit 486's keying gate and unit 487's promotion on its falling edge. Off, the
    /// decoder emits as it always has. Unit 488's measurement is the reason, and the switch is the
    /// route back.
    /// </remarks>
    public bool DetectorGatesKeying { get; set; }

    /// <summary>Whether <see cref="DetectorBlocks"/> decides what is let out; off by default.</summary>
    /// <remarks>
    /// **A LETTER NO LONGER NEEDS BLOCKS TO REACH THE TERMINAL** (work instruction 489, R102,
    /// HM-DEC-194). Unit 487's block rule counts blocks the detector called where it was watching,
    /// and unit 488 measured that at a shoulder of the station, so a decoder reading the station
    /// was refused for not matching them. The scope's own drawing rule is untouched. The switch is
    /// the route back when the detector's pitch is fit.
    /// </remarks>
    public bool DetectorGatesBlocks { get; set; }

    /// <summary>
    /// The detector's marks called since a sequence number, with its audio clock; null to read
    /// by timing alone, as the decoder always has.
    /// </summary>
    /// <remarks>
    /// **A CHARACTER IS A RUN OF MARKS THAT AGREE** (work instruction 490, R103, HM-DEC-195). While
    /// this is set and <see cref="ReadsRuns"/> is on, what reaches the terminal and the scope is
    /// what <see cref="CwSenderGate"/> reads from the marks - pitch, level and length together -
    /// and the timing-only path's letters and leading edge are not let out. A letter appears only
    /// if the run that made it exists, so the terminal and the scope agree by construction; the
    /// keying gate and the block rule are not needed for this path and their switches stay off
    /// as work instruction 489 left them.
    /// </remarks>
    public Func<long, CwMarkBatch>? DetectorMarks { get; set; }

    /// <summary>Whether the terminal reads runs of marks when <see cref="DetectorMarks"/> is set; on by default.</summary>
    /// <remarks>
    /// **THE TIMING-ONLY PATH STAYS, BEHIND THIS** (work instruction 490): off, the decoder lets
    /// out what <see cref="CwProbabilisticDecoder"/> reads, exactly as before, so the two can be
    /// compared. Nothing of that path was changed or deleted.
    /// </remarks>
    public bool ReadsRuns { get; set; } = true;

    private bool RunsRead => ReadsRuns && DetectorMarks is not null;

    private readonly CwSenderGate _runs = new();

    // The last mark sequence number taken from the detector.
    private long _markSequence;

    /// <summary>Take the marks the detector called since last time; drop them while nothing may be decoded.</summary>
    private void PullRuns(bool drop)
    {
        if (DetectorMarks is not { } marks)
        {
            return;
        }

        var batch = marks(_markSequence);

        if (batch.Marks.Count > 0)
        {
            _markSequence = batch.Marks.Max(m => m.Sequence);
        }

        if (!drop)
        {
            _runs.Read(batch);
        }
    }

    // The gates in force: each needs its switch on and its function set.
    private bool KeyingGated => DetectorGatesKeying && KeyingGate is not null;

    private bool BlocksGated => DetectorGatesBlocks && DetectorBlocks is not null;

    // Where on the audio clock the stretch that is keying now began, in seconds.
    private double _openFrom = double.PositiveInfinity;

    private bool _gateOpen;

    // The leading edge as last offered to the screen, and the last moment promoted from it.
    private IReadOnlyList<CwCharacter> _shownEdge = Array.Empty<CwCharacter>();

    private double _promotedThrough = double.NegativeInfinity;

    private double _lastEmittedAt = double.NegativeInfinity;

    /// <summary>Whether a character may reach a surface now: keying now, heard in this stretch, and standing on blocks.</summary>
    private bool Admitted(CwCharacter c)
        => (!KeyingGated || (_gateOpen && c.At.TotalSeconds >= _openFrom))
            && (!BlocksGated || StandsOnBlocks(c));

    /// <summary>Whether the detector called one block for each of the character's elements.</summary>
    private bool StandsOnBlocks(CwCharacter c)
    {
        if (DetectorBlocks is not { } blocks || c.IsWordGap || c.Pattern.Length == 0)
        {
            return true;
        }

        var to = c.At.TotalSeconds;
        var from = to - (c.SpanHops * CwProbabilisticDecoder.HopMilliseconds / 1000);

        return c.SpanHops > 0
            && blocks(from - BlockToleranceSeconds, to + BlockToleranceSeconds) == c.Pattern.Length;
    }

    /// <summary>Reads the gate once per chunk, on the clock the characters are stamped on.</summary>
    private void ReadTheGate()
    {
        if (!KeyingGated || KeyingGate is not { } gate)
        {
            return;
        }

        var open = gate();

        if (open && !_gateOpen)
        {
            _openFrom = Heard.TotalSeconds - CwEnvelopeDetector.KeyingSeconds;
        }

        var closing = !open && _gateOpen;

        _gateOpen = open;

        // **PRINTED STAYS PRINTED** (work instruction 487, R100). What the tip was showing when
        // keying went false has been seen, so it is settled into the transcript rather than taken
        // off the screen; what the decoder held and had not yet shown is dropped. Nothing promoted
        // is settled a second time when the settled pass reaches it.
        if (closing)
        {
            foreach (var shown in _shownEdge.Where(s => !s.IsWordGap && s.At.TotalSeconds > Math.Max(_promotedThrough, _lastEmittedAt)).ToList())
            {
                Emit(shown);
                _promotedThrough = shown.At.TotalSeconds;
            }

            _shownEdge = Array.Empty<CwCharacter>();
        }
    }

    /// <summary>Feed samples directly, without a source.</summary>
    /// <param name="chunk">The samples.</param>
    public void Process(in AudioChunk chunk)
    {
        ReadTheGate();

        // **THE AUDIO CLOCK A SETTLED CHARACTER'S `At` IS READ ON** (work instruction 480
        // task 3): every sample handed here, suspended or not, since the stream's own clock runs
        // through a suspension too.
        Interlocked.Add(ref _samplesHeard, chunk.Samples.Length);

        // **THE TAP STILL TAKES IT.** A capture is the raw evidence of what
        // arrived at the sound card, and audio the operator made himself is part
        // of that: a recording that quietly omitted his own sending would be
        // worth less, not more (§0.0.1). What it does not do is reach a decoder.
        Tap.Take(chunk.Samples, chunk.SampleRate);

        // **THE RUNS, FROM THE DETECTOR'S MARKS** (work instruction 490). While the radio is sending
        // or a digital mode is up the marks are taken and dropped, never read (HM-DEC-147).
        if (RunsRead)
        {
            PullRuns(drop: DecodingSuspended || DigitalMode);
        }

        if (DecodingSuspended || DigitalMode)
        {
            // **NOT DECODED, NOT HELD, NOT RELEASED LATER** (HM-DEC-147). The
            // sidetone of the operator's own transmission is not something
            // anybody sent to him. The tracker is skipped along with everything
            // else, so the survey cannot retune to a sidetone and the pitch that
            // was being read is still there when he stops.
            _suspendedChunks++;

            // **AND THE GUARD STILL HEARS THE RECEIVER GO QUIET** (work
            // instruction 406). What the guard answers is whether the receiver
            // is muted, which is not the radio's question and decides nothing
            // here: the radio has already said it is sending. Skipping the
            // guard with the survey made the report say the operator's own
            // transmission lasted no time at all while he sent for twelve
            // seconds. Only the guard is fed; the survey still hears none of it.
            if (DecodingSuspended)
            {
                ObserveOwnTransmission(chunk.Samples);
            }

            // **BUT THE AUDIO CLOCK KEEPS RUNNING.** Dropping the samples without
            // letting time pass would stamp every character read afterwards as
            // though the transmission had never happened. The port's clock runs
            // on with it, and it starts afresh after the gap.
            if (_second is not null)
            {
                Pend(_second.Skip(chunk.Samples.Length));
            }

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

    /// <summary>Hand the transmit guard the level of each hop, and nothing else.</summary>
    /// <param name="samples">Audio heard while decoding is suspended.</param>
    private void ObserveOwnTransmission(ReadOnlySpan<float> samples)
    {
        var hop = _tracker.HopSamples;

        for (var offset = 0; offset < samples.Length; offset += hop)
        {
            var take = Math.Min(hop, samples.Length - offset);
            var sumSquares = 0.0;

            foreach (var s in samples.Slice(offset, take))
            {
                sumSquares += s * s;
            }

            _tracker.Guard.Observe(20 * Math.Log10(Math.Sqrt(sumSquares / take) + 1e-12));
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
        // **THEN THE DETECTOR'S PITCH, WHILE IT SAYS KEYING** (work instruction 486): the decoder
        // listens where the detector hears, from this hop, behind only the operator's lock -
        // **ONLY WHILE DetectorSteersPitch IS ON**, which it is not by default (work instruction 489, R102).
        var heard = DetectorSteersPitch ? DetectorPitch?.Invoke() ?? double.NaN : double.NaN;

        _probabilistic.ToneHz = !double.IsNaN(_lockedToneHz)
            ? _lockedToneHz
            : double.IsFinite(heard) && heard > 0
                ? heard
                : double.IsNaN(_lastMeasuredToneHz)
                    ? _tracker.ToneHz
                    : _lastMeasuredToneHz;

        // **THE PORT READS THE SAME HOP FIRST, AT THE SAME PITCH** (HM-REQ-120),
        // so whatever ours settles out of this hop meets the port's reading of
        // the same samples.
        if (_second is not null)
        {
            Pend(_second.Read(samples, _probabilistic.ToneHz));
        }

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
    /// <remarks>
    /// The port is finished first, so ours' last characters meet its last
    /// readings; what the port read that ours never met is then one-sided.
    /// </remarks>
    public void Flush()
    {
        if (_second is not null)
        {
            Pend(_second.Flush());
        }

        _probabilistic.Flush();

        var rest = new List<CwCharacter>();

        foreach (var s in _pendingSecond)
        {
            SecondOnlySpans++;
            OneSided(s, rest);
        }

        _pendingSecond.Clear();

        foreach (var c in rest)
        {
            Settle(c);
        }

        if (RunsRead)
        {
            PullRuns(drop: DecodingSuspended || DigitalMode);
            _runs.Flush();
        }
    }

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

        var fromHz = _lastPitchHz;
        _lastPitchHz = pitch;

        // **THE TRACKER MOVED, SO THE WINDOW HOLDS SOMEBODY ELSE** (HM-DEC-095).
        // Nobody tunes exactly, and a signal found two or three hundred hertz
        // from where Hamlet started listening spends its first seconds being
        // measured through a filter pointed at empty band. A refinement within
        // the station being read is not a move in that sense (HM-DEC-123), which
        // is why this counts follows rather than every retune.
        //
        // **AND A FOLLOW THAT KEEPS THE OLD PITCH INSIDE THE PASSBAND IS NOT A
        // HANDOVER EITHER.** The mixdown filter is
        // <see cref="CwProbabilisticDecoder.BandwidthHz"/> wide about the pitch,
        // so a move under half of that leaves where it was listening inside what
        // it now hears, and the held window is still about the same sender.
        // Counting those held the speed unnamed through a whole 18 wpm call that
        // had followed 600 to 625 Hz, and through the first station of the
        // two-station fixture, which follows 25 Hz three times before the real
        // 100 Hz handover. The full width was measured first and let a 50 Hz
        // follow on `exchange-easy` name 21 words a minute for a 12 wpm sender,
        // which is a speed no character supports (work instruction 402,
        // HM-DEC-090, §0.0).
        if (_tracker.Follows != _lastFollows)
        {
            _lastFollows = _tracker.Follows;

            if (double.IsNaN(fromHz)
                || Math.Abs(pitch - fromHz) >= CwProbabilisticDecoder.BandwidthHz / 2)
            {
                _samplesAtDiscontinuity = reading.SampleIndex;
                _hasFollowed = true;
            }

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
