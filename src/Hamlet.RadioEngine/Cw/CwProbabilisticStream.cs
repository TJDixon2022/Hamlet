namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// The probabilistic decoder run over live audio, with a decision delay.
/// </summary>
/// <remarks>
/// <para>**THE REFERENCE RUNS OFFLINE AND THE TERMINAL CANNOT.** What it needs is
/// a sliding window that is re-decoded as audio arrives, and a delay before
/// anything is called final, because deciding late is the entire point of the
/// architecture: a letter read one way can be read another when the next
/// character arrives and the boundaries are all chosen together.</para>
/// <para>**ONE SECOND OF DELAY, WHICH IS BELL'S OWN FIGURE.** Anything already
/// behind it is settled and never changes under the operator; anything inside it
/// is offered as provisional and may be revised. The terminal already draws those
/// two differently, so the difference is visible without inventing a new
/// display.</para>
/// <para>**AND IT IS AFFORDABLE, MEASURED RATHER THAN HOPED.** The whole speed
/// search over twelve hypotheses reads thirty seconds of audio in about a fifth
/// of a second, so a twelve second window re-decoded twice a second costs a few
/// per cent of one core. That was the one piece the reference said nobody had
/// measured.</para>
/// </remarks>
public sealed class CwProbabilisticStream
{
    /// <summary>How much audio the decoder looks back over, in seconds.</summary>
    /// <remarks>
    /// Twelve. Long enough to hold several characters of a slow fist, so the
    /// word-gap hypothesis has something to work with, and short enough that the
    /// noise scale is taken from a stretch of band that has not changed.
    /// </remarks>
    public const double WindowSeconds = 12.0;

    /// <summary>How often the window is read again, in seconds.</summary>
    public const double ReadEverySeconds = 0.5;

    /// <summary>
    /// How far back a character has to be before it stops being revisable.
    /// </summary>
    /// <remarks>
    /// **ONE SECOND, WHICH IS WHAT BELL USED.** Shorter and the last letter of
    /// every character group settles before the gap after it has been seen, which
    /// is exactly the evidence that decides where the character ended. Longer and
    /// the operator watches text sit provisional while he is trying to read it.
    /// </remarks>
    public const double DecisionDelaySeconds = 1.0;

    /// <summary>
    /// How many consecutive reads must find a trough between a sender's gap
    /// clusters before their measured lengths are used.
    /// </summary>
    /// <remarks>
    /// <para>**TWELVE, WHICH IS SIX SECONDS OF NEW AUDIO.** Reads are half a
    /// second apart, so twelve of them is the structure holding while six seconds
    /// of audio the first read never saw enters the window. That is longer than
    /// any single gap at any speed this decoder considers — a word gap at eight
    /// words a minute is a second — so it is evidence from many characters rather
    /// than from one stretch of quiet.</para>
    /// <para>**AND THE MEASUREMENT LEAVES ROOM ON BOTH SIDES.** Counted read by
    /// read: `cw-2026-08-18-004507` holds a trough for 36 consecutive reads and
    /// generated Morse for 23 to 52, while the captures that must not change hold
    /// one, three, four and six. Nothing measured here sits between ten and
    /// twenty-three.</para>
    /// <para>**IT IS USED BOTH WAYS.** The same count of consecutive reads
    /// without a trough abandons the structure, because a sender's spacing is a
    /// fact about the sender and a single window that caught a pause is not
    /// evidence that it changed.</para>
    /// </remarks>
    public const int ReadsToEstablishStructure = 12;

    /// <summary>
    /// How much audio the window has to hold again after being emptied before
    /// anything is read from it.
    /// </summary>
    /// <remarks>
    /// **LESS EVIDENCE HAS TO MEAN SILENCE RATHER THAN GUESSES** (HM-DEC-120).
    /// The per-hop likelihood ratio is an average, and the noise scale and the
    /// signal amplitude behind it are taken from the window's own lower quartile
    /// and upper tail. On two seconds of audio those estimates rest on a handful
    /// of elements and can be badly wrong in either direction, so a short window
    /// does not merely read less: it reads confidently and incorrectly.
    /// </remarks>
    /// <para>**AND IT IS A CONSTANT AGAIN.** It was briefly settable so a sweep
    /// could measure what each length was worth; the answer was nothing at any
    /// length from half a second to twelve, and a mutable static that the whole
    /// suite shares is a way for one test to change another test's numbers
    /// without either of them saying so.</para>
    public const double RefillSeconds = 3.0;

    private readonly int _sampleRate;
    private readonly int _hopSamples;
    private readonly int _windowSamples;
    private readonly int _windowHops;
    private readonly int _readEveryHops;
    private readonly int _delayHops;

    private readonly float[] _mixedI;
    private readonly float[] _mixedQ;
    private readonly double[] _envelope;

    /// <summary>What each held hop was demodulated at, beside the envelope itself.</summary>
    private readonly double[] _mixedAt;
    private readonly double[] _taper;
    private readonly double _taperWeight;

    private int _mixWrite;
    private int _mixFilled;
    private int _sampleInHop;
    private int _envelopeCount;
    private long _samplesSeen;
    private long _hopsSeen;
    private int _hopsSinceRead;
    private int _refillHops;

    /// <summary>How many consecutive reads have found a trough between the gaps.</summary>
    private int _troughRun;

    /// <summary>How many consecutive reads have not.</summary>
    private int _troughMisses;

    /// <summary>True once the sender's own spacing is established.</summary>
    private bool _structureHeld;

    /// <summary>The last spacing a read was willing to stand behind.</summary>
    private CwUnitEstimator.CwGapLengths _heldGaps;
    private double _phase;

    /// <summary>How many characters have been settled since this stream started.</summary>
    private long _settledCount;

    /// <summary>Creates a stream.</summary>
    /// <param name="sampleRate">Samples per second.</param>
    public CwProbabilisticStream(int sampleRate)
        : this(sampleRate, CwProbabilisticDecoder.IntegratorBandwidthHz)
    {
    }

    /// <summary>Listen at a stated integrator width.</summary>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="integratorHz">The integrator's equivalent noise bandwidth.</param>
    /// <remarks>
    /// **THE WIDTH IS A PARAMETER HERE AND A CONSTANT IN PRODUCTION**, exactly as
    /// it already is on <see cref="CwProbabilisticDecoder.Envelope(
    /// IReadOnlyList{float}, int, double, double)"/>. It is open so the trade
    /// between rejecting a competing station and rounding the top of a fast dit
    /// can be swept through the whole decoder rather than through the offline
    /// envelope alone, which is the only form of the sweep that can say what a
    /// width does to a character.
    /// </remarks>
    public CwProbabilisticStream(int sampleRate, double integratorHz)
    {
        _sampleRate = Math.Max(1_000, sampleRate);
        _hopSamples = Math.Max(
            1, (int)(_sampleRate * CwProbabilisticDecoder.HopMilliseconds / 1000.0));

        // **THE SAME INTEGRATOR THE OFFLINE PATH USES, DERIVED THE SAME WAY.**
        // Two envelope paths that disagree about their own filter is how the
        // centred-versus-trailing difference survived unnoticed; the length and
        // the taper both come from one place now.
        _windowSamples = CwProbabilisticDecoder.IntegratorWindow(
            _sampleRate, integratorHz);

        _taper = CwProbabilisticDecoder.IntegratorTaper(_windowSamples);
        _taperWeight = _taper.Sum();

        _windowHops = Math.Max(64, (int)(WindowSeconds * 1000.0
            / CwProbabilisticDecoder.HopMilliseconds));

        _readEveryHops = Math.Max(1, (int)(ReadEverySeconds * 1000.0
            / CwProbabilisticDecoder.HopMilliseconds));

        _delayHops = Math.Max(1, (int)(DecisionDelaySeconds * 1000.0
            / CwProbabilisticDecoder.HopMilliseconds));

        _mixedI = new float[_windowSamples];
        _mixedQ = new float[_windowSamples];
        _envelope = new double[_windowHops];
        _mixedAt = new double[_windowHops];

        // **A GUARD SET ONLY IN A METHOD NOTHING CALLS IS A GUARD THAT DOES NOT
        // EXIST.** This was assigned in `Restart()` alone, and `Restart()` is
        // reachable only behind `CwDecoder.ClearOnAStationChange`, which has been
        // `const false` since the window clear was ruled off. So a stream built
        // fresh carried nought here, `_envelopeCount < 0` is never true, and the
        // refill guard has never run on a first fill in production — which is the
        // one fill every session begins with.
        //
        // What that guard is for is `RefillSeconds`' own remarks: on two seconds
        // of audio the noise scale and the signal amplitude rest on a handful of
        // elements, so a short window does not merely read less, it reads
        // confidently and incorrectly. **Less evidence has to mean silence rather
        // than guesses** (HM-DEC-120), and that has been the stated intent while
        // the code did not do it.
        _refillHops = Math.Max(
            1, (int)(RefillSeconds * 1000.0 / CwProbabilisticDecoder.HopMilliseconds));
    }

    /// <summary>Where the station is, from the tone tracker.</summary>
    /// <remarks>
    /// **THE TRACKER STILL FINDS THE STATION.** Nothing here searches for a pitch;
    /// finding one is the survey's job and it is the one part of the old chain
    /// that works.
    /// </remarks>
    public double ToneHz { get; set; } = 600;

    /// <summary>What the last read made of the window.</summary>
    public CwProbabilisticResult Last { get; private set; } = CwProbabilisticResult.None;

    /// <summary>
    /// True when the last read left the winning path inside a character.
    /// </summary>
    /// <remarks>
    /// **THE INTERLOCK'S QUESTION, ANSWERED BY THE PATH ITSELF** (HM-DEC-096
    /// phase 3). The decoder chooses where every element and every character
    /// begins and ends, over the whole window up to the newest audio it has, so
    /// the last segment of that choice says what the newest audio is inside of.
    /// Nothing is inferred and no threshold is formed.
    /// <para>**IT IS AS OLD AS THE LAST READ AND NO OLDER**, which is half a
    /// second, and the tracker asks the question on the same half second, so the
    /// answer is never more than one survey behind the question. The one second
    /// decision delay does not apply: that governs which characters are settled
    /// enough to emit, not how far the path reaches.</para>
    /// </remarks>
    public bool InsideCharacter => Last.EndsInsideCharacter;

    /// <summary>How many hops have gone by since that answer was worked out.</summary>
    public int HopsSinceAnswer => _hopsSinceRead;

    /// <summary>How many characters have been settled.</summary>
    public long SettledCharacters => _settledCount;

    /// <summary>How many hops of envelope the window is holding.</summary>
    public int EnvelopeHops => _envelopeCount;

    /// <summary>The newest envelope magnitude this stream produced.</summary>
    /// <remarks>
    /// **SO THE TWO ENVELOPE PATHS CAN BE COMPARED AT ALL** (§0.0.1). The
    /// streaming path keeps only its own rolling window, so without this the
    /// filter it actually runs cannot be read from outside and a change to it can
    /// only be judged by what the decoder made of the result. It reads state and
    /// changes none.
    /// </remarks>
    public double NewestEnvelope
        => _envelopeCount == 0 ? 0 : _envelope[_envelopeCount - 1];

    /// <summary>A character that is final and will not be revised.</summary>
    public event Action<CwCharacter>? CharacterSettled;

    /// <summary>
    /// Everything inside the decision delay, offered again on every read.
    /// </summary>
    /// <remarks>
    /// Handed over whole rather than one at a time, because the whole tail can
    /// change when the next character arrives and a consumer needs to replace it
    /// rather than append to it.
    /// </remarks>
    public event Action<IReadOnlyList<CwCharacter>>? LeadingEdgeChanged;

    /// <summary>Feed audio.</summary>
    /// <param name="samples">The samples.</param>
    public void Process(ReadOnlySpan<float> samples)
    {
        var step = 2 * Math.PI * ToneHz / _sampleRate;

        // What the audio now going into the window is being demodulated at, so a
        // re-read can tell whether the window is stale (<see cref="MixedAtHz"/>).
        if (samples.Length > 0)
        {
            MixedAtHz = ToneHz;
        }

        foreach (var sample in samples)
        {
            // Quadrature mixdown, then a boxcar over the arms, which is what a
            // filter of this bandwidth amounts to. The phase is carried rather
            // than recomputed from the sample index so it stays exact over hours.
            _mixedI[_mixWrite] = (float)(sample * Math.Cos(_phase));
            _mixedQ[_mixWrite] = (float)(sample * -Math.Sin(_phase));

            _phase += step;

            if (_phase > 2 * Math.PI)
            {
                _phase -= 2 * Math.PI;
            }

            _mixWrite = (_mixWrite + 1) % _windowSamples;
            _mixFilled = Math.Min(_mixFilled + 1, _windowSamples);
            _samplesSeen++;

            if (++_sampleInHop < _hopSamples)
            {
                continue;
            }

            _sampleInHop = 0;
            PushEnvelope();
        }
    }

    /// <summary>
    /// Let audio time pass without decoding any of it.
    /// </summary>
    /// <param name="samples">How many samples went by.</param>
    /// <remarks>
    /// **THE CLOCK IS THE AUDIO'S AND IT MUST NOT STOP WHEN THE DECODER DOES.**
    /// While the operator is transmitting his own sending is dropped rather than
    /// decoded, and if the hop count stopped with it, every character read
    /// afterwards would be stamped as though the transmission had never
    /// happened. A moment somebody could point at has to be a moment (§0.0.1).
    /// **The envelope is untouched**, so the evidence either side of a
    /// transmission is still there when it ends and the station being read is not
    /// lost to a few seconds of keying.
    /// </remarks>
    public void Skip(int samples)
    {
        _samplesSeen += samples;
        _sampleInHop += samples;

        var hops = _sampleInHop / _hopSamples;

        _sampleInHop -= hops * _hopSamples;
        _hopsSeen += hops;
        _settledThrough += hops;
    }

    /// <summary>
    /// Drop the held audio and start listening afresh at the new pitch.
    /// </summary>
    /// <remarks>
    /// <para>**THE WINDOW HELD TWELVE SECONDS MIXED DOWN AT WHATEVER PITCH THE
    /// TRACKER WAS ON AT THE TIME.** When the tracker follows somebody else, the
    /// earlier hops were taken at the old pitch and the later ones at the new,
    /// and the decode is made over the mixture. Measured on the sensitivity
    /// sweep: one such move at every level, and from eleven decibels down it
    /// costs characters and produces wrong ones — 0.06 of the message wrong at
    /// eleven, 0.19 at three, 0.64 at minus four. A confident wrong character at
    /// the moment somebody answers a call is exactly what HM-DEC-009 exists to
    /// prevent.</para>
    /// <para>**NOTHING ALREADY SETTLED IS RETRACTED.** The audio clock and the
    /// settled mark both keep running, so characters read before the move stand
    /// and are not read again; what goes is the envelope, and the leading edge
    /// with it, because the tip was read through a filter pointed somewhere
    /// else.</para>
    /// <para>The cost is up to twelve seconds of reach every time Hamlet follows
    /// somebody, and it is said on screen rather than hidden.</para>
    /// </remarks>
    public void Restart()
    {
        _envelopeCount = 0;
        _mixWrite = 0;
        _mixFilled = 0;
        _hopsSinceRead = 0;
        _phase = 0;
        _troughRun = 0;
        _troughMisses = 0;
        _structureHeld = false;
        _heldGaps = default;
        _refillHops = Math.Max(
            1, (int)(RefillSeconds * 1000.0 / CwProbabilisticDecoder.HopMilliseconds));

        Last = CwProbabilisticResult.None;

        // The tip belonged to the station that is no longer being read.
        LeadingEdgeChanged?.Invoke(Array.Empty<CwCharacter>());
    }

    /// <summary>How many hops of audio this stream is currently holding.</summary>
    /// <remarks>
    /// What a re-read has to replay to put the window back where it was. It is a
    /// count of hops rather than of seconds so that the replay is hop-aligned by
    /// construction and cannot depend on the shape of arriving chunks.
    /// </remarks>
    public int HeldHops => _envelopeCount;

    /// <summary>How many samples this stream has taken in.</summary>
    /// <remarks>
    /// The stream's own place on the audio clock, which is behind the tap's
    /// whenever a chunk holds more than one hop. A re-read has to ask the tap for
    /// the audio *the stream* has seen, not the audio that has arrived, or the
    /// replay would depend on the shape of the chunk it happened to fire inside.
    /// </remarks>
    public long SamplesSeen => _samplesSeen;

    /// <summary>The pitch the newest hop in the window was mixed at.</summary>
    public double MixedAtHz { get; private set; } = double.NaN;

    /// <summary>
    /// How far the pitch a held hop was mixed at can be from a given pitch,
    /// across everything the window is holding.
    /// </summary>
    /// <param name="pitchHz">The pitch to compare against.</param>
    /// <returns>The largest difference in hertz, or nought where nothing is held.</returns>
    /// <remarks>
    /// <para>**THE NEWEST HOP'S PITCH IS THE WRONG QUESTION, AND ASKING IT MADE
    /// THE RE-READ NEVER FIRE ON ANY CAPTURE IN THE TREE.** The tracker walks its
    /// bank long before its survey admits a candidate, so by the time a pitch is
    /// *measured* the newest audio is usually already being mixed at something
    /// close to it — while the front of the same window is still at the bank
    /// centre the decoder started from. Comparing against the newest hop said
    /// "already close enough" every time.</para>
    /// <para>What decides whether a window is worth reading again is whether
    /// **any** of the audio in it was demodulated somewhere else, so that is what
    /// this measures.</para>
    /// </remarks>
    public double MixedSpreadFrom(double pitchHz)
    {
        var worst = 0.0;

        for (var i = 0; i < _envelopeCount; i++)
        {
            var gap = Math.Abs(_mixedAt[i] - pitchHz);

            if (gap > worst)
            {
                worst = gap;
            }
        }

        return worst;
    }

    /// <summary>
    /// How many times this stream has re-read audio it already held, at a pitch
    /// it learned afterwards.
    /// </summary>
    public int ReReads { get; private set; }

    /// <summary>
    /// Read audio the stream has already seen again, at a pitch it has since
    /// measured.
    /// </summary>
    /// <param name="audio">
    /// Exactly the samples the window is holding, oldest first, from the tap.
    /// </param>
    /// <param name="toneHz">The measured pitch to read them at.</param>
    /// <remarks>
    /// <para>**THE FIRST SECONDS OF EVERY STATION ARE DEMODULATED AT A GUESS,
    /// AND UNTIL NOW THEY STAYED THAT WAY FOR THE REST OF THE CONTACT.** The
    /// stream mixes each sample as it arrives, at whatever pitch the tracker
    /// believed at that moment, and the tracker believes the middle of a bank
    /// until its survey admits a candidate. Measured across this repository's
    /// captures, that first measurement lands two to seven seconds in on half of
    /// them, and the window is still holding every sample from the start when it
    /// does.</para>
    /// <para>**WHAT IT COSTS IN MEMORY IS NOTHING, BECAUSE THE AUDIO IS ALREADY
    /// KEPT.** `AudioTap` holds thirty seconds of raw samples for the capture
    /// button and the keying meter, so a re-read reads what the decoder already
    /// has rather than retaining anything new.</para>
    /// <para>**NOTHING ALREADY SAID IS SAID AGAIN OR TAKEN BACK** (§0.0). The
    /// settled mark and the settled count are carried across untouched, so the
    /// replay re-derives characters that have already been announced and drops
    /// them on the same test that stops a window being re-read twice a second
    /// from repeating itself. What the re-read is for is the characters that have
    /// *not* settled yet: it makes the first emission right rather than editing
    /// history.</para>
    /// <para>**AND THE AUDIO CLOCK IS REWOUND BEFORE THE REPLAY AND LANDS BACK
    /// WHERE IT WAS.** Every character's moment, and the settled mark itself, are
    /// counted in hops since the stream started; replaying without rewinding
    /// would stamp the replayed audio as though it were new and put every
    /// character after it in the wrong place.</para>
    /// </remarks>
    public void ReadAgain(ReadOnlySpan<float> audio, double toneHz)
    {
        var hops = audio.Length / _hopSamples;

        if (hops <= 0 || hops != _envelopeCount)
        {
            // The tap could not give back exactly what the window is holding, so
            // there is nothing to re-read against. Saying nothing is right here:
            // a partial replay would be a window built from two pitches.
            return;
        }

        _envelopeCount = 0;
        _mixWrite = 0;
        _mixFilled = 0;
        _hopsSinceRead = 0;
        _sampleInHop = 0;
        _phase = 0;
        _troughRun = 0;
        _troughMisses = 0;
        _structureHeld = false;
        _heldGaps = default;

        // **THE REFILL GUARD IS STOOD DOWN FOR THE REPLAY AND ONLY FOR IT.** It
        // exists to stop a window that was emptied on a station change being read
        // back before it holds one sender's audio, and this window is being
        // refilled with the same sender's audio it already held.
        _refillHops = 1;

        _hopsSeen -= hops;
        _samplesSeen -= audio.Length;

        ToneHz = toneHz;
        ReReads++;

        Process(audio);

        _refillHops = Math.Max(
            1, (int)(RefillSeconds * 1000.0 / CwProbabilisticDecoder.HopMilliseconds));
    }

    /// <summary>Settle everything still inside the delay, because nothing else is coming.</summary>
    public void Flush()
    {
        if (_envelopeCount == 0)
        {
            return;
        }

        Read(settleEverything: true);
    }

    private void PushEnvelope()
    {
        double i = 0;
        double q = 0;

        // **THE TAPER HAS TO FOLLOW THE AUDIO AND NOT THE ARRAY.** The mixed
        // arms live in a ring buffer, so the oldest sample is wherever the write
        // pointer is about to overwrite and the newest is just behind it. A
        // boxcar could be summed in any order and this cannot: weighting by array
        // index would rotate the window against the signal once per fill and put
        // the taper's peak somewhere different every hop.
        var oldest = _mixFilled < _windowSamples ? 0 : _mixWrite;

        // While the buffer is still filling there is less audio than window, and
        // the taper's newest weights are the ones that have samples under them.
        var from = _windowSamples - _mixFilled;

        for (var n = 0; n < _mixFilled; n++)
        {
            var at = (oldest + n) % _windowSamples;
            var w = _taper[from + n];

            i += _mixedI[at] * w;
            q += _mixedQ[at] * w;
        }

        var magnitude = Math.Sqrt((i * i) + (q * q)) / _taperWeight;

        if (_envelopeCount < _windowHops)
        {
            _mixedAt[_envelopeCount] = ToneHz;
            _envelope[_envelopeCount++] = magnitude;
        }
        else
        {
            Array.Copy(_envelope, 1, _envelope, 0, _windowHops - 1);
            Array.Copy(_mixedAt, 1, _mixedAt, 0, _windowHops - 1);
            _envelope[_windowHops - 1] = magnitude;
            _mixedAt[_windowHops - 1] = ToneHz;
        }

        _hopsSeen++;

        if (++_hopsSinceRead < _readEveryHops)
        {
            return;
        }

        _hopsSinceRead = 0;

        // **NOTHING IS READ FROM A WINDOW THAT HAS NOT REFILLED.** Emptying it on
        // a station change is what stops two pitches being decoded as one, and
        // reading the first two seconds back would trade that for a different
        // wrong answer (HM-DEC-120).
        if (_envelopeCount < _refillHops)
        {
            return;
        }

        Read(settleEverything: false);
    }

    private void Read(bool settleEverything)
    {
        var window = new double[_envelopeCount];

        Array.Copy(_envelope, window, _envelopeCount);

        // **THE UNIT IS MEASURED FROM THE WINDOW RATHER THAN SEARCHED FOR.**
        // The speed grid was scored 0.05 apart out of 33 across its whole range
        // on a real capture, so which hypothesis won was decided in the fourth
        // significant figure. The same information is sitting in two medians:
        // any level the envelope is cut at makes a mark read long and the gap
        // beside it read short by the same amount, so the average of the two
        // short clusters is the dit with the bias cancelled. Measured against
        // generated audio of known speed it returns 12.0, 18.0 and 25.0 for
        // true 12, 18 and 25.
        //
        // **THE GRID IS STILL THERE FOR WHEN THE WINDOW HOLDS TOO LITTLE
        // KEYING** to cluster, which is what a window holding noise looks like,
        // and it is what decides in that case.
        var measured = CwUnitEstimator.Measure(
            window, CwProbabilisticDecoder.HopMilliseconds);

        var speed = measured.IsReady
            && measured.WordsPerMinute >= CwProbabilisticDecoder.SlowestWpm
            && measured.WordsPerMinute <= CwProbabilisticDecoder.FastestWpm
                ? measured.WordsPerMinute
                : (double?)null;

        // **THE SENDER'S OWN GAP LENGTHS, ONCE THE STRUCTURE HAS SURVIVED SIX
        // SECONDS OF NEW AUDIO.** A boundary at a multiple of the estimated unit
        // ties the letter spacing to the speed, so one wrong number breaks both:
        // on `cw-2026-08-18-004507`, whose unit measures fifty milliseconds,
        // twice the unit lands inside that sender's own element-gap cluster and
        // every letter comes apart.
        //
        // **ONE WINDOW IS NOT ENOUGH EVIDENCE AND THAT WAS MEASURED.** Taking a
        // single window's trough cost `VA3VRR` and broke `AA4MP/4 QNIK`, two of
        // the three adjudicated readings, because twelve seconds can show
        // structure the recording does not. Counted read by read across every
        // capture, the longest run of consecutive troughs is 36 on `004507` and
        // 23 to 52 on generated Morse, against 1 on `013347`, 4 on `003758` and 6
        // on `134712`. **There is a wide empty stretch between those two groups
        // and the requirement sits in it.**
        var gaps = measured.IsReady
            ? CwUnitEstimator.MeasureGaps(
                window,
                CwProbabilisticDecoder.HopMilliseconds,
                measured.UnitMilliseconds)
            : default;

        if (gaps.Separated)
        {
            _troughRun++;
            _troughMisses = 0;
            _heldGaps = gaps;
        }
        else
        {
            _troughMisses++;
            _troughRun = 0;
        }

        // Established and abandoned on the same weight of evidence, because a
        // sender's spacing is a fact about the sender and one window that caught
        // a pause is not evidence that it changed.
        if (_troughRun >= ReadsToEstablishStructure)
        {
            _structureHeld = true;
        }
        else if (_troughMisses >= ReadsToEstablishStructure)
        {
            _structureHeld = false;
        }

        var result = CwProbabilisticDecoder.Decode(
            window,
            ToneHz,
            speed,
            _structureHeld
                ? new[]
                {
                    _heldGaps.ElementMilliseconds,
                    _heldGaps.CharacterMilliseconds,
                    _heldGaps.WordMilliseconds,
                }
                : null);

        Last = result;

        // Where the window starts on the audio clock, so a character's hop can be
        // turned into a moment somebody can point at (§0.0.1).
        var windowStartHop = _hopsSeen - _envelopeCount;
        var settleBefore = settleEverything
            ? _envelopeCount + 1
            : _envelopeCount - _delayHops;

        var edge = new List<CwCharacter>();

        foreach (var character in result.Characters)
        {
            var absolute = windowStartHop + character.EndHop;
            var at = TimeSpan.FromSeconds(
                absolute * CwProbabilisticDecoder.HopMilliseconds / 1000.0);

            if (character.EndHop < settleBefore)
            {
                // **ALREADY SAID, AND IT DOES NOT MOVE AGAIN.** Only characters
                // this stream has not settled before are announced, so a window
                // re-read twice a second does not repeat itself.
                if (absolute <= _settledThrough)
                {
                    continue;
                }

                _settledThrough = absolute;
                _settledCount++;
                CharacterSettled?.Invoke(Character(character, result, at));
                continue;
            }

            edge.Add(Character(character, result, at));
        }

        LeadingEdgeChanged?.Invoke(edge);
    }

    private long _settledThrough = -1;

    /// <summary>
    /// One character, in the shape every surface in this application already
    /// reads.
    /// </summary>
    /// <remarks>
    /// **THE CONFIDENCE IS THE LIKELIHOOD RATIO AND IT SAYS SO** (HM-DEC-091).
    /// It is not the old decoder's clarity, which measured how far an element sat
    /// from a boundary that had been guessed at; it is how much better this
    /// reading explains the audio than silence does. A character the alphabet
    /// does not know is unreadable and renders as a placeholder rather than a
    /// guessed letter (HM-DEC-048).
    /// </remarks>
    private CwCharacter Character(
        CwProbabilisticCharacter character, CwProbabilisticResult result, TimeSpan at)
    {
        var known = character.Text != "#";

        return new CwCharacter(
            known ? character.Text : MorseAlphabet.Unreadable,
            known ? CwConfidence.High : CwConfidence.Unreadable,
            result.LikelihoodRatio,
            character.Pattern,
            double.NaN,
            (int)Math.Round(result.WordsPerMinute),
            at)
        {
            SpanLogLikelihoodRatio = character.SpanLogLikelihoodRatio,
            MarginLlr = character.MarginLlr,
            SpanHops = character.SpanHops,
        };
    }
}
