using System.Text;
using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Cw;

/// <summary>Everything the decoder is currently working from (§0.0.1).</summary>
/// <param name="ToneHz">The pitch it is following.</param>
/// <param name="NoiseFloorDb">Where it believes the noise sits.</param>
/// <param name="PeakDb">Where it believes a keyed signal sits.</param>
/// <param name="ThresholdDb">The level it is deciding against.</param>
/// <param name="WordsPerMinute">The sending speed it believes it is tracking.</param>
/// <param name="HasSignal">Whether there is enough separation to decide anything.</param>
/// <param name="Elapsed">How much audio has gone through, from sample counts.</param>
public readonly record struct CwDecoderState(
    double ToneHz,
    double NoiseFloorDb,
    double PeakDb,
    double ThresholdDb,
    int WordsPerMinute,
    bool HasSignal,
    TimeSpan Elapsed);

/// <summary>
/// Reads Morse out of receive audio, and says how sure it is about every
/// character.
/// </summary>
/// <remarks>
/// <para>WHY THIS FEATURE IS THE ONE THAT MATTERS. CW is the last part of this
/// hobby still guarded by the claim that you have to develop an ear for it, and
/// that if you cannot read twenty words a minute you are not really doing it. A
/// decoder that works turns that from a gate into a preference. It does not
/// stop anybody learning to copy by ear, and it lets somebody who has held a
/// license for six years without making a contact read what is on the air
/// tonight.</para>
/// <para>The chain is the standard one and there is no cleverness in it,
/// deliberately. A bank of Goertzel filters finds the note and follows it
/// (<see cref="CwToneTracker"/>). An adaptive gate decides where the key is
/// down and keeps adapting so a fade does not silently end the decode
/// (<see cref="CwGate"/>). Runs of key-down and key-up are clustered into dits,
/// dahs and the three gap lengths by re-deriving the speed from what was
/// actually heard (<see cref="CwSpeedEstimator"/>). Patterns become characters
/// through a table that is allowed to say no
/// (<see cref="MorseAlphabet"/>).</para>
/// <para>WHAT IS NOT STANDARD IS THE CONFIDENCE, and it is the feature rather
/// than a decoration on it. Every character carries a score the decoder can
/// actually justify: how far its elements sat from the decisions made about
/// them, and how far the weakest of them stood above the noise and above any
/// station near enough to be confused with it. On top of that sits one veto,
/// for a character that arrived while somebody else was within a few decibels
/// of the same note. Nothing anywhere rounds a score up
/// (<see cref="CwConfidenceModel"/>).</para>
/// <para>NO CLOCK IS READ ANYWHERE BELOW THIS LINE. Elapsed time comes from
/// counting samples, so the same audio always decodes to the same text, on any
/// machine, at any speed, forever. That is what makes a WAV fixture worth
/// committing and what turns a decoder bug into a regression test rather than
/// an anecdote (HM-DEC-007, §5).</para>
/// <para>Characters are held back until there is enough evidence to decode them
/// against. The first dozen marks of a transmission are as likely to be three
/// dahs as three dits, so they are buffered and decoded once the speed is
/// known, which is why a decode arrives a couple of characters after the audio
/// rather than instantly.</para>
/// </remarks>
public sealed class CwDecoder
{
    /// <summary>How long a silence forces a decision on a short transmission.</summary>
    /// <remarks>
    /// Somebody who sends two characters and stops never produces the twelve
    /// marks the speed estimator wants. After this much silence the decoder
    /// works with what it has, and the confidence says what that was worth.
    /// </remarks>
    private const double ForcedFlushSeconds = 1.5;

    /// <summary>Marks below which a forced flush cannot name anything.</summary>
    /// <remarks>
    /// With fewer than this there is genuinely no way to tell a dit from a dah,
    /// because there is nothing to compare them against. The honest output is
    /// the placeholder (§0.0).
    /// </remarks>
    private const int MinimumForcedMarks = 4;

    /// <summary>
    /// How many dits of silence mean the sender has stopped rather than paused.
    /// </summary>
    /// <remarks>
    /// A CHARACTER IS NORMALLY HELD UNTIL ITS GAP ENDS, which is to say until
    /// the next element starts. That costs about three dits of latency and buys
    /// the thing the confidence model needs most: the actual length of the gap
    /// that ended the character. Whether a silence was one dit or three is what
    /// separates "U" from "IT", and a decoder that committed at the boundary
    /// would have no way to say how close a call it was.
    /// <para>Which leaves the case where nothing follows, because the sender
    /// stopped. Past this much silence the gap is unambiguous however long it
    /// eventually turns out to be, so the character is released and the
    /// terminating gap counts as certain.</para>
    /// </remarks>
    private const double EndOfTransmissionDits = 8;

    private readonly CwToneTracker _tracker;
    private readonly CwGate _gate = new();
    private readonly CwSpeedEstimator _speed;
    private readonly CwProbabilisticStream _probabilistic;

    private bool _transmitting;
    private DateTime _transmitEndedUtc = DateTime.MinValue;
    private long _suspendedChunks;

    /// <summary>
    /// How long decoding stays suspended after the radio stops transmitting.
    /// </summary>
    /// <remarks>
    /// <para>**HALF A SECOND, AND THE EVIDENCE FOR IT IS THE POLL AND NOT THE
    /// KEYING.** Transmit status is a live field, asked for four times a second
    /// (<see cref="Rig.RigPollPlan.LiveInterval"/>), so the state Hamlet holds
    /// can be a quarter of a second old before the reply is even parsed. Full
    /// break-in switches between elements, which is tens of milliseconds: **the
    /// poll cannot see that at all**, and nothing in this hold-off is measured
    /// against it because nothing here can measure it.</para>
    /// <para>What the figure is measured against is two things that are known.
    /// Two poll intervals, so one dropped reply cannot resume decoding in the
    /// middle of a transmission. And the receiver's own recovery, which
    /// <see cref="CwTransmitGuard"/> measured at about twenty-four milliseconds
    /// of transmit-receive hang with a ramp behind it, and holds a hundred and
    /// fifty milliseconds for.</para>
    /// <para>**IT IS ASYMMETRIC ON PURPOSE.** Suspension is immediate, because a
    /// late suspension puts the operator's own sending on the screen as somebody
    /// else's; resumption waits, because an early resumption does the same thing
    /// with the tail of it.</para>
    /// </remarks>
    public static TimeSpan ResumeAfter { get; } = TimeSpan.FromMilliseconds(500);

    /// <summary>True while the radio is transmitting or has just stopped.</summary>
    public bool DecodingSuspended { get; private set; }

    /// <summary>How many chunks of audio were dropped rather than decoded.</summary>
    /// <remarks>
    /// Diagnostic, and nothing reads it to decide anything (§0.0.1). A terminal
    /// that went quiet and a terminal that was told to stop listening are
    /// different facts and only one of them is a fault.
    /// </remarks>
    public long SuspendedChunks => _suspendedChunks;

    /// <summary>
    /// Tell the decoder what the radio says about its own transmitter.
    /// </summary>
    /// <param name="transmitting">
    /// True when the radio reports the transmitter keyed, false when it reports
    /// it not, and null when nobody knows.
    /// </param>
    /// <param name="nowUtc">The clock.</param>
    /// <remarks>
    /// <para>**THE RADIO SAYS SO AND THE AUDIO NEVER DOES** (HM-DEC-091). Not the
    /// level, not the sidetone's pitch, not a change in the noise floor: every
    /// one of those is a guess about the transmitter made from the thing the
    /// transmitter is drowning out. `CwTransmitGuard` does read the audio, and it
    /// answers a different question — whether the receiver is muted — which is a
    /// fact about the receiver and is used to stop the gate's trackers learning
    /// from silence.</para>
    /// <para>**AND NOT KNOWING IS NOT TRANSMITTING.** An unknown transmit state
    /// leaves decoding running, because a decoder silenced by a link that has
    /// gone quiet is a band that reads as empty (§0.0). The cost of being wrong
    /// that way is text the operator can see is his own; the cost the other way
    /// is a screen that stops without a reason.</para>
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
    private readonly Action<ToneReading> _onReading;

    private readonly StringBuilder _pattern = new();
    private readonly List<PendingElement> _pending = new();
    private readonly List<double> _clarities = new();

    private IAudioSource? _attached;

    private bool _keyDown;
    private double _runSamples;
    private double _runSnrSum;

    /// <summary>The loudest this mark got, in decibels over the noise.</summary>
    private double _runSnrMax = double.MinValue;
    private int _runHops;
    private double _worstSnrDb = double.MaxValue;
    private double _runContestedDb = double.MinValue;
    private double _contestedDb = double.MaxValue;
    private bool _silenceFlushed;
    private bool _sawAnyMark;
    private long _lastSample;

    /// <summary>
    /// How many measurements since the operator was last transmitting.
    /// </summary>
    /// <remarks>
    /// **A COUNTER RATHER THAN A FLAG, BECAUSE THE GATE ANSWERS LATE.** Asking
    /// only whether the measurement immediately before a mark was blocked caught
    /// nothing at all: the gate de-glitches over as much as nine measurements, so
    /// by the time a mark is declared the block it grew out of is already several
    /// measurements behind. Every sliver of the operator's own transmission
    /// passed the test and went into the clock fit (HM-DEC-095).
    /// </remarks>
    private int _hopsSinceBlocked = int.MaxValue / 2;

    /// <summary>Did the run now ending begin out of one of his own mutes?</summary>
    private bool _runFollowedBlock;

    /// <summary>Has anything in the character being built been cut short?</summary>
    private bool _patternTruncated;

    /// <summary>How many times the tracker had moved, last time this looked.</summary>
    /// <summary>Moves to a different station, which are the costly ones.</summary>
    private int _lastFollows;

    /// <summary>Every second measurement goes to the settled pass.</summary>
    private const int SettledDecimation = 2;

    /// <summary>How often the settled pass is asked to read, in its own points.</summary>
    /// <remarks>
    /// Fifty, which is half a second. The pass reads a trailing window ending
    /// half a second behind the leading edge, so asking more often re-reads
    /// almost the same audio.
    /// </remarks>
    private const int SettledEveryPoints = 50;

    private readonly CwSettledPass _settled;
    private readonly List<SettledCharacter> _settledOut = new();
    private readonly List<CwCharacter> _awaitingSettlement = new();
    private readonly List<CwRevision> _revisions = new();

    private int _settledPhase;
    private int _settledSinceRun;
    private bool _settledStale = true;
    private SettledOutcome _lastOutcome;

    /// <summary>Creates a decoder.</summary>
    /// <param name="sampleRate">Samples per second of the audio it will be fed.</param>
    /// <param name="expectedToneHz">
    /// The operator's CW pitch, as a place to start looking. The tracker hunts
    /// either side of it, since nobody tunes exactly.
    /// </param>
    /// <param name="refusalFloorDb">
    /// The margin below which nothing is emitted, or null for the ruled floor
    /// (HM-DEC-097, HM-DEC-117). A parameter so the number can be measured
    /// rather than translated a second time.
    /// </param>
    public CwDecoder(
        int sampleRate, double expectedToneHz = 600, double? refusalFloorDb = null)
    {
        RefusalFloorDb = refusalFloorDb ?? CwConfidenceModel.RefusalFloorDb;
        SampleRate = Math.Max(1_000, sampleRate);
        _tracker = new CwToneTracker(SampleRate, expectedToneHz);

        // **THE ONLY THING THAT READS CHARACTERS NOW.** The old path still runs
        // its gate and its clock fit, because the tone tracker, the transmit
        // guard, the element counters and the audio tap all hang off it and the
        // capture press depends on every one of them. What it no longer does is
        // put a character on a screen: `CharacterDecoded` and `CharacterSettled`
        // are raised from here and from nowhere else.
        _probabilistic = new CwProbabilisticStream(SampleRate);
        _probabilistic.CharacterSettled += c =>
        {
            // **THE COUNTERS COUNT WHAT REACHED THE SCREEN** (HM-DEC-091). They
            // were incremented on the old path's own emit, which has raised
            // nothing since the decoder was replaced, so a capture sidecar said
            // `0 characters emitted` and `nothing read` about an instant when the
            // terminal was showing text. **Two readouts of one instant giving two
            // answers is the fault that ruling exists for**, and it made the
            // evening's roster unusable for scoring.
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

            // **AND THE OLD EVENT NOW CARRIES THE NEW DECODER'S TIP.** Everything
            // that used to subscribe to the provisional reading still gets one;
            // what it gets is this decoder's, because there is no other.
            foreach (var character in e)
            {
                CharacterDecoded?.Invoke(character);
            }
        };
        _speed = new CwSpeedEstimator(SampleRate);
        _onReading = OnReading;
        _settled = new CwSettledPass(
            SampleRate,
            (double)_tracker.HopSamples * SettledDecimation / SampleRate);
    }

    /// <summary>Samples per second.</summary>
    public int SampleRate { get; }

    /// <summary>What the working decoder last made of the audio.</summary>
    /// <remarks>
    /// **THE ONE SOURCE FOR EVERY NUMBER ABOUT THE READING** (HM-DEC-091). The
    /// sidecar and the panel were quoting `CwSpeedEstimator`, which fits a clock
    /// from run lengths and has decoded nothing since the decoder was replaced.
    /// That is how a sheet came to report a dah of 15.7 dits beside text a
    /// completely different decoder had produced.
    /// </remarks>
    public CwProbabilisticResult Reading => _probabilistic.Last;

    /// <summary>
    /// The margin below which this decoder emits nothing (HM-DEC-097).
    /// </summary>
    /// <remarks>
    /// **A PARAMETER SO THE NUMBER CAN BE MEASURED RATHER THAN GUESSED AGAIN**
    /// (HM-DEC-117). Seventeen was reasoned from the offset between broadband
    /// and in-filter measurement and was four decibels out, and the way to
    /// settle where it belongs is to sweep it rather than to translate it a
    /// second time. Defaults to the ruled value, so nothing outside a
    /// measurement ever sees anything else.
    /// </remarks>
    public double RefusalFloorDb { get; }

    /// <summary>Raised for every character, space and placeholder, in order.</summary>
    /// <remarks>
    /// **IT IS THE NEW DECODER'S LEADING EDGE, ONE CHARACTER AT A TIME.** It used
    /// to be the old path's provisional tip, arriving as each element completed.
    /// Nothing on the old path raises it now. A consumer that wants to replace
    /// the edge rather than append to it should take <see cref="LeadingEdge"/>
    /// instead, because the whole tail can change when the next character
    /// arrives.
    /// </remarks>
    public event Action<CwCharacter>? CharacterDecoded;

    /// <summary>
    /// Everything inside the decision delay, handed over whole on every read.
    /// </summary>
    /// <remarks>
    /// **IT IS REPLACED, NOT APPENDED TO.** The whole point of deciding late is
    /// that a letter can be read differently once the next one arrives, so a
    /// consumer takes this list as the current state of the leading edge rather
    /// than as news.
    /// </remarks>
    public event Action<IReadOnlyList<CwCharacter>>? LeadingEdge;

    /// <summary>
    /// Raised for each character the second pass has read and stands behind.
    /// </summary>
    /// <remarks>
    /// These consume the provisional readings covering the same audio. A reader
    /// showing one line puts these behind the cursor and the provisional tip in
    /// front of it.
    /// </remarks>
    public event Action<CwCharacter>? CharacterSettled;

    /// <summary>
    /// What the two passes each made of the same audio, in memory only.
    /// </summary>
    /// <remarks>
    /// **NOT PERSISTED, BY RULING** (HM-DEC-096). Diagnostic under §0.0.1, so it
    /// is exportable on demand and never written to disk, because a growing
    /// on-disk log needs a retention policy nobody has designed.
    /// </remarks>
    public IReadOnlyList<CwRevision> Revisions => _revisions;

    /// <summary>What the settled pass last did, including why it read nothing.</summary>
    public SettledOutcome SettledState => _lastOutcome;

    /// <summary>
    /// The note tracker, so what it decided can be read from outside (§0.0.1).
    /// </summary>
    /// <remarks>
    /// **A DECISION NOBODY CAN OBSERVE CANNOT BE TESTED, AND FOUR OF THEM WERE
    /// SHIPPED THAT WAY** (HM-DEC-104). Clock loss, the retained previous clock,
    /// tracker switching and the speed-change annotation were all built on
    /// rulings, and none had a committed test, partly because the state that
    /// proves them was not reachable. Exposed read-only: there is no setter here
    /// and nothing outside the engine can move the tracker.
    /// </remarks>
    public CwToneTracker Tracker => _tracker;

    /// <summary>
    /// What the decoder has worked out about this sender's timing.
    /// </summary>
    /// <remarks>
    /// Read-only, so a surface can say what the measured spacing is rather than
    /// only acting on it (HM-DEC-115). A Farnsworth sender should be visible,
    /// not merely survived.
    /// </remarks>
    public CwSpeedEstimator Timing => _speed;

    /// <summary>
    /// This sender's measured gap classes, or null (HM-DEC-115).
    /// </summary>
    /// <remarks>
    /// From the settled pass, which fits them over the whole signal rather than
    /// over a window: a window holds plenty of element gaps and often no word
    /// gap at all, and three classes cannot be found in it however cleanly they
    /// separate over the transmission.
    /// </remarks>
    public CwGapClasses? GapClasses => _settled.Classes;

    /// <summary>How many gaps the settled pass has to cluster (HM-DEC-115).</summary>
    public int SettledGapsRemembered => _settled.GapsRemembered;

    /// <summary>
    /// True when the sender left no word gaps long enough to measure
    /// (HM-DEC-142).
    /// </summary>
    /// <remarks>
    /// **AN UNSPACED TRANSCRIPT AND AN EMPTY ONE ARE DIFFERENT FACTS**, and the
    /// record has to be able to tell them apart: one is a callsign read
    /// correctly, the other is the decoder producing nothing. False while no
    /// classes have been fitted at all, because nothing has been measured either
    /// way yet.
    /// </remarks>
    public bool WordSpacingUnmeasured
        => _settled.Classes is { } classes && !classes.WordSpacingMeasured;

    /// <summary>
    /// True when nothing is coming along behind the provisional tip to confirm
    /// it (HM-DEC-096, phase 4).
    /// </summary>
    public bool SettledIsRefusing => _settledStale || !_lastOutcome.Read;

    /// <summary>What the decoder is currently working from.</summary>
    public CwDecoderState State => new(
        _tracker.ToneHz,
        _gate.NoiseFloorDb,
        _gate.PeakDb,
        _gate.NoiseFloorDb + ((_gate.PeakDb - _gate.NoiseFloorDb) * 0.5),
        _speed.IsReady ? _speed.WordsPerMinute : 0,
        _gate.HasSignal,
        TimeSpan.FromSeconds((double)_lastSample / SampleRate));

    /// <summary>How the signal has been behaving, for the plain-language note.</summary>
    public CwSignalWatch Watch { get; } = new();

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
    public CwDecodeReport Report => new(
        Tap.Level,
        _tracker.ToneHz,
        _lastSnrDb,
        HasTone: _toneLatched,
        _elementsSeen,
        _elementsResolved,
        _charactersEmitted,
        _charactersUnsure,
        _tracker.HasKeying,
        _tracker.Verdict.Interference,
        (double)_tracker.Guard.BlockedHops * _tracker.HopSamples / SampleRate,
        WordSpacingUnmeasured);

    private double _lastSnrDb = double.NaN;

    /// <summary>Whether a tone has been located and has not gone away.</summary>
    private bool _toneLatched;
    private int _elementsSeen;
    private int _elementsResolved;
    private int _charactersEmitted;
    private int _charactersUnsure;

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
            // **NOT DECODED, NOT HELD, NOT RELEASED LATER.** The sidetone of the
            // operator's own transmission is not something anybody sent to him,
            // and presenting it as received text is HM-DEC-009 in its purest
            // form: on full break-in it decodes as a page of isolated letters
            // that look exactly like a weak station being read.
            //
            // The tracker is skipped along with everything else, so the survey
            // cannot retune to a sidetone and the pitch that was being read is
            // still there when the operator stops.
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
        // for this chunk, so the pitch handed over is the current one rather than
        // the one before it.
        _probabilistic.ToneHz = _tracker.ToneHz;
        _probabilistic.Process(chunk.Samples);
    }

    /// <summary>
    /// Finish: settle anything the new decoder is still holding inside its
    /// decision delay, and let the old path run its counters out.
    /// </summary>
    /// <remarks>
    /// Called at the end of a fixture, so the last character of a recording is
    /// not silently dropped. Live audio reaches the same place through the
    /// silence after the last element.
    /// </remarks>
    public void Flush()
    {
        // **THE NEW DECODER FIRST**, because it is the one whose output anybody
        // reads: everything still inside its decision delay is settled, since
        // nothing more is coming to revise it.
        _probabilistic.Flush();

        Drain(force: true);

        // The recording simply ran out. Whatever silence it ended on is the
        // best evidence there is about the gap that closed the last character,
        // so it counts rather than being thrown away.
        if (!_keyDown && _runSamples > 0 && _speed.DitSamples > 0)
        {
            _clarities.Add(_speed.EndOfCharacterClarity(_runSamples));
        }

        FlushCharacter(force: true);
        _pending.Clear();

        // The recording ended, so everything still behind the settled cursor is
        // read now rather than being dropped with the last few seconds of audio.
        for (var pass = 0; pass < 16; pass++)
        {
            var before = _settled.SettledThrough;

            RunSettledPass(drain: true);

            if (_settled.SettledThrough == before)
            {
                break;
            }
        }
    }

    private void OnSamples(in AudioChunk chunk) => Process(in chunk);

    /// <summary>
    /// One measurement of the tone, five milliseconds after the last.
    /// </summary>
    private void OnReading(ToneReading reading)
    {
        _lastSample = reading.SampleIndex;

        // **THE TRACKER MOVED, SO EVERYTHING HELD WAS HEARD SOMEWHERE ELSE**
        // (HM-DEC-095). Nobody tunes exactly, and a signal found two or three
        // hundred hertz from where Hamlet started listening spends its first
        // seconds being measured through a filter pointed at empty band. Those
        // elements are real measurements of nothing, and carrying them into the
        // decode produced a row of placeholders in front of every message found
        // off-frequency.
        // **AND A REFINEMENT IS NOT A MOVE IN THAT SENSE** (HM-DEC-123). The
        // tracker settling one bin over on the station it is already reading has
        // not pointed the filter at empty band: the elements in flight were
        // measured on the same signal through a filter a hundred hertz wide, and
        // the settled pass's window is full of the station it is still reading.
        // Throwing either away costs a callsign and buys nothing, which is what
        // two twenty-five hertz moves on a thirty second capture were doing.
        if (_tracker.Follows != _lastFollows)
        {
            _lastFollows = _tracker.Follows;
            _pending.Clear();
            _pattern.Clear();
            _clarities.Clear();
            _patternTruncated = false;
            _worstSnrDb = double.MaxValue;
            _contestedDb = double.MaxValue;

            // **A TRACKER SWITCH IS A CLOCK-LOSS EVENT** (HM-DEC-096, phase 3).
            // Operationally a pitch change and a speed change are the same thing:
            // somebody else started transmitting. The settled pass has to forget
            // its window, because that window is full of a different station.
            _settled.Reset();
            _settledStale = true;

            // The speed stays silent until the window holds only marks from the
            // station now being read (HM-DEC-107, phase 1).
            _marksAtDiscontinuity = _speed.MarksSeen;

            // **THE SPEED SURVIVES THE MOVE, AND THE ELEMENTS DO NOT.** Throwing
            // the speed away too was tried and it is what a retune costs that is
            // hardest to earn back: twelve marks, which on a slow fist is several
            // seconds during which nothing can be decoded at all. It also made
            // strong signals decode worse than weak ones, because a strong signal
            // gets the tracker moving sooner and so paid the price more often.
            //
            // Lengths are lengths whatever bin measured them, so a speed learned
            // just before a move is still the speed. What cannot survive is the
            // elements themselves, since those were measured through a filter
            // pointed at empty band.
        }

        // THE DE-GLITCH IS SIZED FROM THE ELEMENT (HM-DEC-088). The speed
        // estimator already knows how long a dit is here, and telling the gate
        // lets it integrate over a third of one instead of over a fixed
        // twenty-five milliseconds that is too short at twelve words a minute
        // and too long at sixty.
        _gate.FollowSpeed(_speed.DitSamples / _tracker.HopSamples);

        // **AND THE TRACKER LISTENS AS NARROWLY AS THE SPEED ALLOWS**
        // (HM-DEC-095). Forty milliseconds of window is the difference between
        // half a callsign and all of it on the recording that prompted this, and
        // it would smear the edges off a fast fist, so it follows the speed
        // rather than being chosen once.
        _tracker.FollowSpeed(_speed.IsReady ? _speed.WordsPerMinute : 0);

        // **THE TRACKER MAY NOT MOVE PART-WAY THROUGH A CHARACTER** (HM-DEC-096,
        // phase 3). Only the decoder knows whether one is part-read, so it says
        // so, and the tracker holds the move until the character closes. A held
        // switch is not an abandoned one: the candidate keeps being re-confirmed
        // while it waits.
        _tracker.MidCharacter = _pattern.Length > 0 || _pending.Count > 0;

        // **THE SECOND PASS IS FED THE SAME ENVELOPE, DECIMATED** (HM-DEC-096,
        // phase 1). Every second measurement is a ten millisecond grid, which is
        // what the validated reference chain fits thresholds over, and half the
        // points to sort.
        if (++_settledPhase >= SettledDecimation)
        {
            _settledPhase = 0;
            _settled.Observe(reading.PowerDb, reading.Blocked, reading.SampleIndex);

            if (++_settledSinceRun >= SettledEveryPoints)
            {
                _settledSinceRun = 0;
                RunSettledPass();
            }
        }

        // **HOW FAR THE TONE STANDS ABOVE THE BAND WHILE IT IS KEYED**, which is
        // not the same question as how far it stands above it on average, and
        // the difference is why real stations were being missed (HM-DEC-090).
        //
        // A station answering a call keys for a second and a half in thirty
        // seconds. Averaged across all of it, a signal fifty decibels out of the
        // noise reported minus nought point six, because for ninety-six percent
        // of the time the bin holds nothing but noise and the mean is the answer
        // to a question nobody asked.
        //
        // So it is a held peak: up at once, down over about ten seconds. Three
        // measurements have to agree before it counts, which is what stops one
        // burst of static setting it.
        if (reading.HasNoise)
        {
            _snrHistory[_snrWrite] = reading.SnrDb;
            _snrWrite = (_snrWrite + 1) % _snrHistory.Length;
            _snrFilled = Math.Min(_snrFilled + 1, _snrHistory.Length);

            if (_snrFilled == _snrHistory.Length)
            {
                var sustained = Median(_snrHistory);

                _lastSnrDb = double.IsNaN(_lastSnrDb) || sustained > _lastSnrDb
                    ? sustained
                    : _lastSnrDb - SnrDecayDbPerHop;

                // Opens high and closes low, so a marginal signal is not dropped
                // in the quiet parts of its own message (HM-DEC-090).
                _toneLatched = _lastSnrDb >= (_toneLatched
                    ? CwDecodeReport.ToneReleaseDb
                    : CwDecodeReport.ToneThresholdDb);
            }
        }

        var gate = _gate.Judge(reading.PowerDb, reading.NoiseDb, reading.Blocked);
        Watch.Observe(gate, _tracker.HopSamples, SampleRate);

        if (gate.KeyDown != _keyDown)
        {
            if (_keyDown)
            {
                // **A MARK CUT OFF BY HIS OWN TRANSMITTER IS NOT A MARK**
                // (HM-DEC-095). What is audible between his own elements is a
                // sliver of somebody else's signal, cut at both ends by his
                // keying rather than by theirs, so its length is a fact about
                // him. Measured as elements, those slivers decode into a
                // confident string of E and T, which is the most seductive wrong
                // output this feature can produce: it looks exactly like a weak
                // station being read (§0.0).
                OnMarkEnded(_runFollowedBlock || reading.Blocked);
            }
            else
            {
                OnGapEnded();
            }

            _keyDown = gate.KeyDown;
            _runSamples = 0;
            _runSnrSum = 0;
            _runSnrMax = double.MinValue;
            _runHops = 0;
            _runContestedDb = double.MinValue;

            // Anything beginning within the guard's own hold of his last mute
            // grew out of it, whatever the gate's de-glitch delay happens to be.
            _runFollowedBlock =
                (double)_hopsSinceBlocked * _tracker.HopSamples / SampleRate
                    <= CwTransmitGuard.HoldSeconds;
        }

        _hopsSinceBlocked = reading.Blocked
            ? 0
            : Math.Min(_hopsSinceBlocked + 1, int.MaxValue / 2);

        _runSamples += _tracker.HopSamples;
        _runHops++;

        if (gate.KeyDown)
        {
            // HOW FAR ABOVE EVERYTHING ELSE, not just above the noise. A second
            // station a couple of hundred hertz away is not noise, and a decoder
            // that only measured its margin over the noise floor would report a
            // beautiful signal while quietly merging somebody else's dits into
            // the character. Taking the worse of the two margins is what turns
            // that from a confident wrong letter into an honest uncertain one.
            var height = Math.Min(
                gate.SignalToNoiseDb, reading.MarginOverCompetitorDb);

            _runSnrSum += height;
            _runSnrMax = Math.Max(_runSnrMax, height);

            // THE STRONGEST MOMENT OF THE MARK, not the weakest. A keyed
            // element's rising and falling edges are amplitude transients, and a
            // transient throws energy right across the band, so the quietest
            // instant of any mark always looks contested even on an empty one.
            // What the veto is asking is whether another station was comparable
            // in strength to this one, and the fair place to ask that is where
            // this one was at full height.
            _runContestedDb = Math.Max(
                _runContestedDb, reading.RawMarginOverCompetitorDb);

            return;
        }

        // Still key-up. A gap that has grown past the character boundary means
        // whatever was pending is finished, whether or not the operator ever
        // sends anything else.
        CheckSilence();
    }

    /// <summary>
    /// How fast the held signal-to-noise figure falls away, per measurement.
    /// </summary>
    /// <remarks>
    /// Measurements arrive two hundred times a second, so this decays about ten
    /// decibels in ten seconds: long enough to hold across the gaps inside a
    /// message and short enough that a station going away is noticed.
    /// </remarks>
    private const double SnrDecayDbPerHop = 0.005;

    /// <summary>
    /// How many measurements have to agree before a level counts.
    /// </summary>
    /// <remarks>
    /// Three, taken as a median. A held peak is defenseless against a single
    /// spike, and noise in a narrow filter throws one every few seconds.
    /// </remarks>
    private readonly double[] _snrHistory = new double[5];

    private int _snrWrite;
    private int _snrFilled;

    /// <summary>The middle of three, without allocating.</summary>
    private static double Median(double[] values)
    {
        Span<double> copy = stackalloc double[values.Length];
        values.CopyTo(copy);
        copy.Sort();
        return copy[copy.Length / 2];
    }

    private void OnMarkEnded(bool truncated)
    {
        var snr = _runHops > 0 ? _runSnrSum / _runHops : 0;

        _sawAnyMark = true;
        _elementsSeen++;

        // EXCLUDED FROM THE CLOCK FIT, which is the half of this rule that
        // decides whether anything else works. A speed estimator fed the lengths
        // of somebody's own keying gaps settles on a speed nobody is sending at,
        // and every character after it is measured against that.
        //
        // **AND IT GOES IN WITH HOW LOUD IT WAS** (HM-DEC-144). Length alone
        // cannot tell a merged element from a sliver the gate chopped out of
        // band noise, and height can: on `cw-2026-08-17-134712` the station's
        // elements stand about ten decibels above its chatter. The average and
        // the loudest moment are both carried, because a real mark the gate held
        // open across a dip has an average that sags and a loudest moment that
        // does not.
        if (!truncated)
        {
            _speed.AddMark(
                _runSamples,
                snr,
                _runHops > 0 ? _runSnrMax : double.NaN);
        }

        _pending.Add(new PendingElement(
            IsMark: true, _runSamples, snr, _runContestedDb, truncated));

        Drain();
    }

    private void OnGapEnded()
    {
        if (!_sawAnyMark)
        {
            // Leading silence before anything has been heard. There is nothing
            // to measure and nothing to say.
            return;
        }

        _elementsSeen++;
        _speed.AddGap(_runSamples);
        _pending.Add(
            new PendingElement(IsMark: false, _runSamples, 0, double.MaxValue, false));

        Drain();
    }

    /// <summary>
    /// Emit whatever the growing silence has settled.
    /// </summary>
    private void CheckSilence()
    {
        if (!_sawAnyMark)
        {
            return;
        }

        if (!_speed.IsReady)
        {
            // A short transmission that will never reach the evidence the
            // estimator wants. Decide on what there is once the silence is long
            // enough that nothing more is coming.
            if (!_silenceFlushed
                && _runSamples >= ForcedFlushSeconds * SampleRate
                && _pending.Count > 0)
            {
                _silenceFlushed = true;
                Drain(force: true);
                FlushCharacter(force: true);
            }

            return;
        }

        if (_silenceFlushed || _runSamples < EndOfTransmissionDits * _speed.DitSamples)
        {
            return;
        }

        // The sender has stopped. However long this silence eventually runs it
        // is already past every boundary there is, so the gap that ended this
        // character is as certain as a gap gets.
        _silenceFlushed = true;
        _clarities.Add(1.0);
        FlushCharacter(force: false);
        EmitWordGap(1.0);
    }

    /// <summary>
    /// Process everything held back, once there is enough to process it
    /// against.
    /// </summary>
    /// <param name="force">
    /// Decide on thin evidence because the transmission has ended.
    /// </param>
    private void Drain(bool force = false)
    {
        if (!_speed.IsReady && !force)
        {
            return;
        }

        var nameable = _speed.IsReady || _speed.MarkCount >= MinimumForcedMarks;

        foreach (var element in _pending)
        {
            if (element.IsMark)
            {
                if (element.Truncated)
                {
                    // Nothing is appended, because there is nothing honest to
                    // append. The character carries the fact that part of it was
                    // lost and is emitted as a placeholder rather than as
                    // whatever the surviving fragment happens to spell.
                    _patternTruncated = true;
                    _silenceFlushed = false;
                    continue;
                }

                var mark = _speed.ClassifyMark(element.Samples);
                _pattern.Append(mark == CwElement.Dit ? '.' : '-');
                _clarities.Add(nameable ? _speed.Clarity(mark, element.Samples) : 0);
                _worstSnrDb = Math.Min(_worstSnrDb, element.SignalToNoiseDb);

                _contestedDb = Math.Min(_contestedDb, element.ContestedMarginDb);
                _silenceFlushed = false;
                continue;
            }

            var gap = _speed.ClassifyGap(element.Samples);
            var clarity = nameable ? _speed.Clarity(gap, element.Samples) : 0;

            if (gap == CwElement.ElementGap)
            {
                _clarities.Add(clarity);
                continue;
            }

            if (_silenceFlushed)
            {
                // The sender stopped, this character was already released, and
                // the gap has only now ended because they started again.
                _silenceFlushed = false;
                continue;
            }

            // The gap ended the character, and how sure the decoder is about
            // that is part of how sure it is about the character: a silence
            // halfway between one dit and three is the difference between "U"
            // and "IT". How far past three it went is a separate question and
            // belongs to the space, not to the letter.
            _clarities.Add(
                nameable ? _speed.EndOfCharacterClarity(element.Samples) : 0);

            // The force flag has to travel down here too. Without it a short
            // transmission being decided on at the end of its silence would
            // hold every buffered character back and run them together into one
            // impossible pattern.
            FlushCharacter(force);

            if (gap == CwElement.WordGap)
            {
                EmitWordGap(clarity);
            }
        }

        _pending.Clear();
    }

    /// <summary>
    /// Turn the pattern gathered so far into a character and emit it.
    /// </summary>
    private void FlushCharacter(bool force)
    {
        // **TRUNCATED EVIDENCE IS NOT EVIDENCE** (HM-DEC-095). A character built
        // partly out of slivers heard between the operator's own elements is
        // emitted as the placeholder and never as a letter, however plausible
        // what survived happens to look.
        if (_patternTruncated)
        {
            var lost = _pattern.Length > 0 || force;

            _pattern.Clear();
            _clarities.Clear();
            _patternTruncated = false;
            _worstSnrDb = double.MaxValue;
            _contestedDb = double.MaxValue;

            if (lost && _speed.LooksLikeMorse)
            {
                Emit(new CwCharacter(
                    MorseAlphabet.Unreadable,
                    CwConfidence.Unreadable,
                    0,
                    string.Empty,
                    0,
                    _speed.WordsPerMinute,
                    TimeSpan.FromSeconds((double)_lastSample / SampleRate)));
            }

            return;
        }

        if (_pattern.Length == 0)
        {
            return;
        }

        if (!_speed.IsReady && !force)
        {
            return;
        }

        var pattern = _pattern.ToString();
        var text = MorseAlphabet.Lookup(pattern);
        var timing = WorstClarity();
        var snr = _worstSnrDb == double.MaxValue ? 0 : _worstSnrDb;

        var score = CwConfidenceModel.Score(timing, snr);
        var confidence = CwConfidenceModel.Rate(score, text is not null, _contestedDb);

        _pattern.Clear();
        _clarities.Clear();
        _worstSnrDb = double.MaxValue;
        _contestedDb = double.MaxValue;

        // **BELOW THE FLOOR THE DECODER SAYS NOTHING** (HM-DEC-097, built at
        // last). That ruling was made on a sweep and never implemented: from
        // 18 dB down to 1 the whole message came back with nothing wrong, at
        // minus one a character in five was wrong, and at minus two it emitted a
        // full message of which 44 percent was invented. A trained ear copies to
        // about nought decibels, so refusing there meets the goal rather than
        // falling short of it, and a plausible wrong callsign on screen is
        // actionable however it is labelled.
        //
        // Seventeen is that ruling's nought decibels in the units this decoder
        // can actually measure, which is inside its own tone filter.
        if (snr < RefusalFloorDb)
        {
            return;
        }

        // NOTHING IS CLAIMED WHEN THE TIMINGS DO NOT LOOK LIKE MORSE. Noise
        // makes runs of key-down and key-up too, and a gate will happily chop
        // an empty band into letters if nothing is watching for whether they
        // are actually spaced like Morse. Suppressing the whole character is
        // right rather than marking it unreadable: an unreadable mark says
        // something was heard, and on an empty band nothing was (§0.0).
        if (!_speed.LooksLikeMorse)
        {
            return;
        }

        Emit(new CwCharacter(
            // A character the decoder will not stand behind shows the
            // placeholder and NOT the letter its pattern happened to spell.
            // Showing the letter dimmed would still be putting a guess on
            // screen, and the reader would copy it down.
            confidence == CwConfidence.Unreadable ? MorseAlphabet.Unreadable : text!,
            confidence,
            score,
            pattern,
            snr,
            _speed.WordsPerMinute,
            TimeSpan.FromSeconds((double)_lastSample / SampleRate)));
    }

    /// <summary>
    /// The worst any element of this character managed.
    /// </summary>
    /// <remarks>
    /// The worst rather than the average, because a character is only as
    /// certain as its least certain element. One element on a decision boundary
    /// is enough to make the whole letter a different letter, and averaging it
    /// away would hide exactly the case the reader needs to see.
    /// </remarks>
    private double WorstClarity()
    {
        if (_clarities.Count == 0)
        {
            return 0;
        }

        var worst = 1.0;
        foreach (var c in _clarities)
        {
            worst = Math.Min(worst, c);
        }

        return worst;
    }

    /// <summary>
    /// Emit the space between two words.
    /// </summary>
    /// <param name="clarity">
    /// How clearly the silence was a word gap rather than a long pause between
    /// characters. This is where that ambiguity belongs, since it decides the
    /// spacing and nothing about the letters either side of it.
    /// </param>
    private void EmitWordGap(double clarity)
    {
        if (!_speed.LooksLikeMorse)
        {
            return;
        }

        Emit(new CwCharacter(
            MorseAlphabet.WordGap,
            clarity >= CwConfidenceModel.HighAbove ? CwConfidence.High : CwConfidence.Low,
            clarity,
            string.Empty,
            0,
            _speed.WordsPerMinute,
            TimeSpan.FromSeconds((double)_lastSample / SampleRate)));
    }

    /// <summary>
    /// The slowest and fastest this radio's keyer can go (`14 0C`, p. 19-3).
    /// </summary>
    /// <remarks>
    /// **A BACKSTOP AND NOT THE FIX.** Sixty-four words a minute was reported
    /// with nothing on frequency, and this radio cannot send above forty-eight,
    /// so the number could not have come from a station under any circumstances.
    /// What produces it is arithmetic over noise-derived elements, which the tone
    /// gate now prevents at source; this is the second line (HM-DEC-090).
    /// </remarks>
    public const int SlowestPlausibleWpm = 6;

    /// <summary>The fastest the radio's own keyer sends.</summary>
    public const int FastestPlausibleWpm = 48;

    /// <summary>
    /// The sending speed, or null when nothing has earned the right to name one
    /// (HM-DEC-090).
    /// </summary>
    /// <remarks>
    /// <para>**ONE GUARDED ANSWER, READ BY EVERY SURFACE.** The speed reached
    /// three separate screens as a settled fact while nothing was being received:
    /// a badge beside the filter width, the terminal's summary, and a sentence in
    /// the send panel saying what "they" were sending at, with nobody sending.
    /// Guarding each of them would have left the fourth.</para>
    /// <para>Three conditions, all required. A tone has to have been located, or
    /// the elements behind the estimate came out of noise. Characters have to be
    /// resolving, because a speed is a fact about somebody's keying and without
    /// letters there is no evidence any keying happened. And it has to be a speed
    /// this radio could produce.</para>
    /// </remarks>
    public int? WordsPerMinute
    {
        get
        {
            var report = Report;

            if (!report.HasTone || report.CharactersEmitted == 0)
            {
                return null;
            }

            // **NO NUMBER WHILE THE CLOCK IS BEING RE-ACQUIRED** (HM-DEC-107,
            // phase 1). Across a change of station the estimate passes through
            // speeds belonging to neither of them: measured on eleven words a
            // minute handing over to twenty-two, it named sixteen and eighteen,
            // which is their average and a fact about nobody, and wandered as far
            // as forty-four before coming to rest correctly.
            //
            // Marking it unsettled was rejected, because an unsettled forty-four
            // is still forty-four on the screen a beginner uses to decide whether
            // he could have copied the exchange, and he concludes it was beyond
            // him when it was not. Showing the last proved speed was rejected as
            // asserting a stale fact as a current one.
            if (SpeedIsReacquiring)
            {
                return null;
            }

            // **THE SPEED THAT REACHES A SURFACE IS THE ONE THE SETTLED PASS
            // PROVED**, not the one the rolling estimator is currently tracking.
            // The estimator has to keep moving to follow a fist; that is its job
            // and it is why it wanders across a handover. The settled pass fits a
            // clock and refuses it unless the dah-to-dit ratio and the element
            // lengths are ones somebody could actually send, and it keeps the
            // previous clock as a candidate so a fade does not count as a change.
            // A number that has been through that is a fact about somebody's
            // keying; the tracking estimate on its own is not.
            // **AND THE GUARD IN FRONT OF IT IS WHY THIS IS NOT THE WORKING
            // DECODER'S NUMBER.** Pointing this at the probabilistic pass was
            // built and measured and it breaks a §0.0 protection: that pass reads
            // a twelve second window, so across a handover it names a speed
            // between the two stations, and `NoSpeedBetweenTwoStationsIsEverNamed`
            // caught it naming 18 where one sends 16 and the other something
            // else. A number describing neither station is worse than no number.
            //
            // So the badge keeps this guard, and **the working decoder's own
            // speed is reported separately and says what it is** — the `reading`
            // line on the capture sidecar carries it with the grid it won out of
            // (HM-DEC-091: one source, and it says which).
            var dit = _lastOutcome.Read ? _lastOutcome.DitMilliseconds : 0;

            if (dit <= 0)
            {
                return null;
            }

            var wpm = (int)Math.Round(1200.0 / dit);

            return wpm >= SlowestPlausibleWpm && wpm <= FastestPlausibleWpm
                ? wpm
                : null;
        }
    }

    /// <summary>
    /// True while the speed estimate still rests partly on the previous station
    /// (HM-DEC-107, phase 1).
    /// </summary>
    /// <remarks>
    /// <para>**THE TEST IS WHETHER ANY MARK FROM BEFORE THE DISCONTINUITY IS
    /// STILL IN THE WINDOW**, which is exact rather than a settling delay picked
    /// by hand. The estimate is drawn from a rolling window of twenty marks, so
    /// it is trustworthy again precisely when twenty marks have arrived since the
    /// tracker moved or the clock was lost, and not one mark sooner.</para>
    /// <para>A surface showing the speed leaves the field blank while this holds,
    /// or says it is re-acquiring. It does not carry a number.</para>
    /// </remarks>
    public bool SpeedIsReacquiring
        => !_lastOutcome.Read
           || _speed.MarksSeen - _marksAtDiscontinuity < CwSpeedEstimator.WindowSize;

    /// <summary>How many marks had been seen when the clock was last lost.</summary>
    private long _marksAtDiscontinuity;

    /// <summary>
    /// Ask the second pass to read whatever is now far enough behind
    /// (HM-DEC-096, phase 1).
    /// </summary>
    private void RunSettledPass(bool drain = false)
    {
        _settledOut.Clear();

        var dit = _speed.IsReady
            ? _speed.DitSamples / SampleRate * 1000
            : 0;

        _lastOutcome = _settled.Settle(dit, _settledOut, drain);

        if (_lastOutcome.Refusal == SettledRefusal.ClockLost
            || _lastOutcome.SpeedChanged)
        {
            // A clock lost, or a clock that has moved by a quarter, is a
            // different station as far as the speed is concerned.
            _marksAtDiscontinuity = _speed.MarksSeen;
        }

        if (!_lastOutcome.Read)
        {
            // **THE PROVISIONAL TIP KEEPS RUNNING** (phase 4). The moment
            // somebody answers is the worst possible moment for the live feed to
            // go dark, so a refusal here marks the leading edge unstable rather
            // than silencing it.
            _settledStale = true;
            return;
        }

        _settledStale = false;

        // **NOTHING IS SETTLED WITHOUT SOMEBODY KEYING** (HM-DEC-095, §0.0). A
        // window of band noise or of a steady carrier will eventually fit two
        // levels and a plausible clock, and the settled pass is more dangerous
        // than the provisional one when it does: settled text is what the
        // transcript keeps. On the recording holding a carrier and no station it
        // produced two hundred characters of confident nonsense before a gate
        // went in.
        //
        // The gate is the survey's own keying verdict, which is the measurement
        // that already decides whether anybody is sending at all.
        //
        // **HM-DEC-143 RULED THIS REPLACED AND IT DID NOT MEET ITS OWN
        // CONDITION.** That ruling has the settled pass judge for itself from the
        // marks it extracted, and makes the carrier recording the gate on
        // shipping. Removing this test let that recording produce 33 characters
        // where it produces none, and a tightness test on the mark clusters left
        // it at 33 as well: a carrier chopped by noise fits two centres and sits
        // near them. The ruling stands and is unbuilt, which is recorded in
        // HM-OPEN-054 rather than worked around here.
        if (!_tracker.KeyingRecently)
        {
            _settledStale = true;
            return;
        }

        foreach (var settled in _settledOut)
        {
            Settle(settled);
        }
    }

    /// <summary>
    /// Turn one settled reading into a character, reconciled against whatever
    /// the leading edge said about the same audio.
    /// </summary>
    /// <remarks>
    /// <para>**THE CONFIDENCE RULE IS RULED AND IS NOT A JUDGEMENT CALL**
    /// (HM-DEC-096). Where the two passes agree on the character, the settled
    /// score stands: two independent readings arriving at the same letter is
    /// corroboration, not the decoder talking itself into something. Where they
    /// disagree, the lower of the two scores stands, because disagreement is
    /// itself evidence that the character was marginal and it has to cost
    /// something.</para>
    /// <para>Both scores are kept either way, which is what makes a wrong settled
    /// reading diagnosable rather than arguable (§0.0.1).</para>
    /// </remarks>
    private void Settle(SettledCharacter settled)
    {
        var at = TimeSpan.FromSeconds((double)settled.LastSample / SampleRate);

        // A truncated character may never be rendered as a letter, whatever its
        // pattern spelled (HM-DEC-095).
        var text = settled.Truncated ? MorseAlphabet.Unreadable : settled.Text;
        var score = settled.Score;

        var provisional = TakeProvisional(settled);
        var agreed = provisional is not null
            && string.Equals(provisional.Text, text, StringComparison.Ordinal);

        if (provisional is not null && !agreed)
        {
            score = Math.Min(score, provisional.Score);
        }

        var resolved = text != MorseAlphabet.Unreadable && !settled.Truncated;

        var character = new CwCharacter(
            resolved ? text : MorseAlphabet.Unreadable,
            CwConfidenceModel.Rate(score, resolved),
            score,
            settled.Pattern,
            0,
            settled.WordsPerMinute,
            at)
        {
            Stage = CwReadingStage.Settled,
        };

        if (provisional is not null)
        {
            _revisions.Add(new CwRevision(provisional, character, agreed));
        }

        // **THE OLD PATH NO LONGER PUTS A CHARACTER ON A SCREEN.** Everything
        // above still runs, because the counters, the revisions record and the
        // watch all hang off it and the capture sidecar reads all three. What it
        // does not do is speak: `CharacterSettled` is raised by
        // `CwProbabilisticStream` and by nothing else.
        _oldPathSettled++;
    }

    /// <summary>How many characters the old path would have settled.</summary>
    /// <remarks>
    /// Kept as a number rather than a transcript, so a session comparing the two
    /// architectures has something to compare and no operator ever sees the old
    /// one's reading (§0.0.1).
    /// </remarks>
    public int OldPathCharacters => _oldPathSettled;

    private int _oldPathSettled;

    /// <summary>
    /// The provisional reading covering the same audio, removed from the queue.
    /// </summary>
    private CwCharacter? TakeProvisional(SettledCharacter settled)
    {
        var endedAt = (double)settled.LastSample / SampleRate;

        for (var i = 0; i < _awaitingSettlement.Count; i++)
        {
            var candidate = _awaitingSettlement[i];
            var difference = Math.Abs(candidate.At.TotalSeconds - endedAt);

            // Within a quarter second of the same moment is the same character.
            // The two passes measure an element's end from different thresholds,
            // so they never land on exactly the same sample.
            if (difference > 0.25)
            {
                continue;
            }

            _awaitingSettlement.RemoveRange(0, i + 1);
            return candidate;
        }

        // Anything older than this settled reading will never be matched now.
        _awaitingSettlement.RemoveAll(c => c.At.TotalSeconds < endedAt - 0.25);
        return null;
    }

    private void Emit(CwCharacter character)
    {
        // **NOTHING IS EMITTED WITHOUT A TONE TO EMIT IT FROM** (HM-DEC-090).
        // Seventeen hundred characters came out of half a minute of band noise,
        // seventeen hundred of them marked unsure, and marking them was not
        // enough: a screen full of blocks and dimmed letters reads as a signal
        // being fought over rather than as nothing being there.
        //
        // This gate is safe only because the measurement under it was fixed
        // first. Gating on the old figure would have suppressed a real decode,
        // which is the same §0.0 failure wearing a quieter coat.
        if (!Report.HasTone)
        {
            return;
        }

        // COUNTED HERE, WHERE EVERY CHARACTER GOES PAST, so the metrics cannot
        // drift away from what actually reached the screen (HM-DEC-088). Word
        // gaps are spacing rather than copy and are not counted as either.
        // **NOT COUNTED HERE ANY MORE.** This path has raised nothing since the
        // decoder was replaced, and counting what it would have said produced a
        // sidecar describing a decoder nobody can see the output of. The counters
        // are incremented where the characters actually leave, beside
        // `CwProbabilisticStream`.

        // **MARKED UNSTABLE WHEN NOTHING IS COMING BEHIND IT** (HM-DEC-096,
        // phase 4). While the settled pass is refusing or re-acquiring, the
        // leading edge is all there is, and the reader has to be able to see that
        // no second opinion is on its way (§0.0).
        var tip = character with
        {
            Stage = SettledIsRefusing
                ? CwReadingStage.Unstable
                : CwReadingStage.Provisional,
        };

        if (!tip.IsWordGap)
        {
            _awaitingSettlement.Add(tip);

            // The queue only ever holds what has not yet been overtaken. A
            // decoder left running for an evening must not accumulate a
            // transcript in a field nobody reads (§8).
            if (_awaitingSettlement.Count > 256)
            {
                _awaitingSettlement.RemoveRange(0, 128);
            }
        }

        Watch.Observe(tip);
    }

    /// <summary>A measured run, waiting for something to be measured against.</summary>
    /// <param name="IsMark">True when the key was down.</param>
    /// <param name="Samples">How long it lasted.</param>
    /// <param name="SignalToNoiseDb">
    /// How far it stood above the noise and above any rival station, averaged
    /// across the run.
    /// </param>
    /// <param name="ContestedMarginDb">
    /// The closest another station came during this run, before the filter is
    /// credited with rejecting any of it.
    /// </param>
    /// <param name="Truncated">
    /// True when the operator's own transmission cut this run short, so its
    /// length is a fact about him rather than about anybody on the band
    /// (HM-DEC-095).
    /// </param>
    private readonly record struct PendingElement(
        bool IsMark, double Samples, double SignalToNoiseDb, double ContestedMarginDb,
        bool Truncated);
}
