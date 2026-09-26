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
    {
        _sampleRate = Math.Max(1_000, sampleRate);
        _hopSamples = Math.Max(
            1, (int)(_sampleRate * CwProbabilisticDecoder.HopMilliseconds / 1000.0));

        // **THE SAME INTEGRATOR THE OFFLINE PATH USES, DERIVED THE SAME WAY.**
        // Two envelope paths that disagree about their own filter is how the
        // centred-versus-trailing difference survived unnoticed; the length and
        // the taper both come from one place now.
        _windowSamples = CwProbabilisticDecoder.IntegratorWindow(
            _sampleRate, CwProbabilisticDecoder.IntegratorBandwidthHz);

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
    /// True when the speed behind <see cref="Last"/> was measured from the
    /// window's keying rather than won on the grid.
    /// </summary>
    /// <remarks>
    /// **MEASURED IS THE ESTIMATOR'S DIT OR THE MARKS' OVERRULE; THE GRID IS A
    /// WINNER** (work instruction 451, HM-REQ-034). Where the window holds too
    /// little keying to cluster, the path is searched across every speed and the
    /// best one is kept, which is a hypothesis and not a measurement. It is
    /// recorded as the read makes it and nothing in the decode reads it.
    /// </remarks>
    public bool UnitWasMeasured { get; private set; }

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
        UnitWasMeasured = false;

        // The tip belonged to the station that is no longer being read.
        LeadingEdgeChanged?.Invoke(Array.Empty<CwCharacter>());
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
            _envelope[_envelopeCount++] = magnitude;
        }
        else
        {
            Array.Copy(_envelope, 1, _envelope, 0, _windowHops - 1);
            _envelope[_windowHops - 1] = magnitude;
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

        var gapMilliseconds = _structureHeld
            ? new[]
            {
                _heldGaps.ElementMilliseconds,
                _heldGaps.CharacterMilliseconds,
                _heldGaps.WordMilliseconds,
            }
            : null;

        var result = CwProbabilisticDecoder.Decode(window, ToneHz, speed, gapMilliseconds);

        // **WHERE THE MARKS SAY THE SENDER IS SLOWER THAN THE PATH WAS TIMED, THE
        // MARKS' SPEED IS USED** (work instruction 441, task 2). A clock that runs
        // fast makes a letter's own element gap long enough to end it, and the
        // first piece prints as a sure `E` or `T`. Unit 441's trace put 8 of the
        // 51 sure-wrong letters and 2 of the 363 sure-right ones in windows whose
        // marks imply a unit more than 1.25 times the path's, against 28 and 263
        // between 0.80 and 1.25; that edge is the ratio, taken from the trace. The
        // other side did not separate the two and is left alone.
        var marks = CwUnitEstimator.Elements(window, CwProbabilisticDecoder.HopMilliseconds).Marks;
        var marksUnit = CwUnitEstimator.MarkUnit(marks);
        var unitMeasured = speed is not null;

        if (result.WordsPerMinute > 0
            && marksUnit * result.WordsPerMinute / 1200.0 > MarksOverruleRatio)
        {
            var marksWpm = 1200.0 / marksUnit;

            if (marksWpm >= CwProbabilisticDecoder.SlowestWpm
                && marksWpm <= CwProbabilisticDecoder.FastestWpm)
            {
                result = CwProbabilisticDecoder.Decode(window, ToneHz, marksWpm, gapMilliseconds);
                unitMeasured = true;
            }
        }

        // **AND WHERE THE SENDER'S DOT-DASH PAIRS SAY HE IS SLOWER STILL, THEIR
        // SPEED IS USED** (work instruction 459, HM-REQ-129: fldigi's speed tracking,
        // cw.cxx:524-535 and 831-843, taken into ours). Bursts and dropouts of 10 to
        // 45 ms fill the short clusters both measures above rest on, and a sender
        // read at twice his speed has his dits read as dahs and his letters cut:
        // task 1 traced six sure-wrong letters to it. A burst never pairs with the
        // marks beside it. Past the same edge as the marks' overrule, on the same
        // side, and nothing else.
        var pairUnit = CwUnitEstimator.PairUnit(marks);

        if (result.WordsPerMinute > 0
            && pairUnit * result.WordsPerMinute / 1200.0 > MarksOverruleRatio)
        {
            var pairWpm = 1200.0 / pairUnit;

            if (pairWpm >= CwProbabilisticDecoder.SlowestWpm
                && pairWpm <= CwProbabilisticDecoder.FastestWpm)
            {
                result = CwProbabilisticDecoder.Decode(window, ToneHz, pairWpm, gapMilliseconds);
                unitMeasured = true;
            }
        }

        Last = result;
        UnitWasMeasured = unitMeasured;

        // **THIS SENDER'S CHARACTER GAP, FOR THE SPACES ONLY** (work instruction
        // 415). Measured from the same window whether or not the word trough
        // held; the path above never sees it.
        var characterGap = measured.IsReady
            ? CwUnitEstimator.MeasureCharacterGap(
                window,
                CwProbabilisticDecoder.HopMilliseconds,
                measured.UnitMilliseconds)
            : null;

        // **THE PATH'S OWN WORD BOUNDARY, FOR WHEN NO CHARACTER GAP WAS MEASURED**
        // (work instruction 416): midway on a log scale between the character and
        // word gaps the path was given, held or textbook.
        var unitMs = result.WordsPerMinute > 0 ? 1200.0 / result.WordsPerMinute : 0;
        var wordFrom = _structureHeld
            ? Math.Sqrt(_heldGaps.CharacterMilliseconds * _heldGaps.WordMilliseconds)
            : Math.Sqrt(3 * 7) * unitMs;

        // **WITH NOTHING MEASURED AND NOTHING HELD, THE LETTER SPACES THE PATH
        // ALREADY PLACED** (work instruction 452, task 2). The textbook boundary
        // kept 21 letter spaces as word gaps on the real keyed recordings and 18
        // on the synthetic set, each 4.6 to 7 units and shorter than that
        // sender's own word spaces.
        if (!_structureHeld && characterGap is null)
        {
            wordFrom = Math.Max(wordFrom, LetterSpaceBoundary(result));
        }

        // Where the window starts on the audio clock, so a character's hop can be
        // turned into a moment somebody can point at (§0.0.1).
        var windowStartHop = _hopsSeen - _envelopeCount;
        var settleBefore = settleEverything
            ? _envelopeCount + 1
            : _envelopeCount - _delayHops;

        var edge = new List<CwCharacter>();

        foreach (var (character, removed) in Spaced(result, characterGap, wordFrom))
        {
            var absolute = windowStartHop + character.EndHop;
            var at = TimeSpan.FromSeconds(
                absolute * CwProbabilisticDecoder.HopMilliseconds / 1000.0);

            // **AT THE FLUSH, AN UNREADABLE CHARACTER STILL INSIDE THE DELAY IS
            // NOT SETTLED** (work instruction 408, unit 405's B2). Nothing more is
            // coming to confirm it, and what it holds is the tail of the file's own
            // noise; settling it prints a placeholder the audio after it would
            // never have supported (R57).
            if (settleEverything
                && character.Text == "#"
                && character.EndHop >= _envelopeCount - _delayHops)
            {
                continue;
            }

            if (character.EndHop < settleBefore)
            {
                // **ALREADY SAID, AND IT DOES NOT MOVE AGAIN.** Only characters
                // this stream has not settled before are announced, so a window
                // re-read twice a second does not repeat itself.
                if (absolute <= _settledThrough)
                {
                    continue;
                }

                // **THE MARK MOVES EXACTLY AS THE PATH'S OWN CHARACTERS MOVE IT**,
                // a space the relabel took out included, so every letter settles
                // where and when it did before the relabel existed.
                _settledThrough = absolute;
                _settledCount++;

                if (!removed)
                {
                    CharacterSettled?.Invoke(Character(character, result, at, window));
                }

                continue;
            }

            if (!removed)
            {
                edge.Add(Character(character, result, at, window));
            }
        }

        LeadingEdgeChanged?.Invoke(edge);
    }

    /// <summary>
    /// How much longer than the path's unit the window's marks must imply before
    /// the path is re-read at the marks' speed.
    /// </summary>
    /// <remarks>
    /// The bin edge in unit 441's trace past which sure-wrong letters outnumber
    /// sure-right ones, 8 to 2. Author's, from the trace, not tuned after it.
    /// </remarks>
    internal const double MarksOverruleRatio = 1.25;

    /// <summary>
    /// How many times longer or shorter than one unit a gap inside a letter may
    /// be before the letter is shown dimmed rather than sure.
    /// </summary>
    /// <remarks>
    /// The farthest any sure-right letter's worst inner gap lay in unit 445's
    /// trace over the real keyed recordings, 6.5; past it lay 2 sure-wrong letters
    /// and no right one. Author's, from the trace, not moved after R78's numbers.
    /// </remarks>
    internal const double InnerGapEdge = 6.5;

    /// <summary>How far past the character gap a gap must run to be a word gap.</summary>
    /// <remarks>
    /// **THE GEOMETRIC MEAN OF THE CHARACTER GAP AND SEVEN THIRDS OF IT**, the
    /// textbook ratio of the two carried onto what this sender actually does
    /// (work instruction 415, author's, overrulable). Chosen from the heaps in
    /// `docs/phase-correctness/unit415-trace.md` and not from a score: it sits on
    /// the character heap's falling shoulder, and past it to twice the gap is a
    /// low flat plateau with no emptier place to move to.
    /// </remarks>
    internal static readonly double WordGapShare = Math.Sqrt(7.0 / 3.0);

    /// <summary>How many gaps between letters a read must hold before their median is taken for the sender's.</summary>
    /// <remarks>The count <see cref="CwUnitEstimator.MeasureGaps"/> asks for before it clusters.</remarks>
    internal const int LetterSpacesToMeasure = 12;

    /// <summary>
    /// The relabel's boundary from the gaps the path placed between letters, or
    /// nought where the read holds too few.
    /// </summary>
    /// <param name="result">What the read made of the window.</param>
    /// <returns>The median gap times <see cref="WordGapShare"/>, in milliseconds.</returns>
    /// <remarks>
    /// <para>**THE MEDIAN GAP BETWEEN LETTERS IS THE SENDER'S CHARACTER GAP**
    /// (work instruction 452, task 2, author's, overrulable). Letters outnumber
    /// words several to one, so the middle of the gaps between them is a letter
    /// space whichever way the path labelled each. Used only where
    /// <see cref="CwUnitEstimator.MeasureCharacterGap"/> found no character heap
    /// and no gaps are held: unit 452's trace found the envelope's three heaps
    /// splitting the element gaps in two there, by a dropout or by an empty middle
    /// heap, so the letter spaces fell in with the word spaces.</para>
    /// <para>**DURATIONS ONLY** (R72). It never reads which letters they are, and
    /// it only feeds the relabel, which can only take a space out.</para>
    /// </remarks>
    private static double LetterSpaceBoundary(CwProbabilisticResult result)
    {
        if (result.Gaps.Count < LetterSpacesToMeasure)
        {
            return 0;
        }

        var spans = result.Gaps.Select(g => g.SpanHops).OrderBy(s => s).ToArray();

        return spans[spans.Length / 2] * CwProbabilisticDecoder.HopMilliseconds * WordGapShare;
    }

    /// <summary>
    /// The read's characters in order, each marked where the sender's character
    /// gap says a space the path read between two letters is not one.
    /// </summary>
    /// <param name="result">What the read made of the window.</param>
    /// <param name="characterGap">This sender's character gap, or null.</param>
    /// <param name="wordFrom">The path's own word boundary in milliseconds, used only when no character gap was measured.</param>
    /// <returns>Every character, and whether its space is taken out.</returns>
    /// <remarks>
    /// <para>**IT ONLY EVER TAKES A SPACE OUT** (work instruction 415, task 3).
    /// The relabel that also put spaces in was built and taken out under 3.2: it
    /// added one inside the adjudicated `N4L`. This one cannot add a space, and it
    /// is aimed at the spaces the textbook boundary inserts inside a word.</para>
    /// <para>**THE LETTERS ARE THE PATH'S, BY CONSTRUCTION.** A removed space still
    /// carries its hop so the caller keeps its bookkeeping identical.</para>
    /// <para>**WHERE NO CHARACTER GAP WAS MEASURED, THE PATH'S OWN WORD BOUNDARY
    /// DECIDES** (work instruction 416, task 3). Unit 416's trace found 21 of the
    /// 24 inserted spaces left on the keyed recordings in windows with no
    /// character gap, and all 13 on `cw-2026-09-23-173723` under held structure
    /// at a median of 0.66 of the boundary the path was given: the path read a
    /// space on a gap shorter than its own word boundary. Such a space is taken
    /// out; one at or past the boundary stands.</para>
    /// </remarks>
    private static IEnumerable<(CwProbabilisticCharacter Character, bool Removed)> Spaced(
        CwProbabilisticResult result, double? characterGap, double wordFrom)
    {
        if (result.Characters.Count == 0 || (characterGap is null && !(wordFrom > 0)))
        {
            return result.Characters.Select(c => (c, false));
        }

        var boundaryHops = characterGap is { } gap
            ? gap * WordGapShare / CwProbabilisticDecoder.HopMilliseconds
            : wordFrom / CwProbabilisticDecoder.HopMilliseconds;
        var byEnd = result.Gaps.ToDictionary(g => g.EndHop);

        return result.Characters
            .Select(c => (c, c.Pattern.Length == 0
                && byEnd.TryGetValue(c.EndHop, out var read)
                && read.IsWordGap
                && (characterGap is not null
                    ? read.SpanHops <= boundaryHops
                    : read.SpanHops < boundaryHops)))
            .ToList();
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
        CwProbabilisticCharacter character, CwProbabilisticResult result, TimeSpan at, double[] window)
    {
        var known = character.Text != "#";

        return new CwCharacter(
            known ? character.Text : MorseAlphabet.Unreadable,
            !known ? CwConfidence.Unreadable
                : GapsFitTheUnit(character, result, window) ? CwConfidence.High
                : CwConfidence.Low,
            result.LikelihoodRatio,
            character.Pattern,
            double.NaN,
            (int)Math.Round(result.WordsPerMinute),
            at)
        {
            SpanLogLikelihoodRatio = character.SpanLogLikelihoodRatio,
            SpanHops = character.SpanHops,
            MarginLlr = character.RivalMargin,
            RivalReading = character.RivalReading,
        };
    }

    /// <summary>
    /// Whether every gap inside the letter lies within <see cref="InnerGapEdge"/>
    /// times one unit of the speed the path was read at, either way.
    /// </summary>
    /// <remarks>
    /// **READ FROM THE MARKS THE WINDOW HOLDS AT EMISSION** (HM-REQ-015, work
    /// instruction 445): the letter's span one unit either side, cut as
    /// <see cref="CwUnitEstimator.Elements"/> cuts, and only the gaps between two
    /// whole marks. It never reads the letters beside it (R72). A letter with no
    /// such gap fits.
    /// </remarks>
    private static bool GapsFitTheUnit(CwProbabilisticCharacter character, CwProbabilisticResult result, double[] window)
    {
        if (result.WordsPerMinute <= 0)
        {
            return true;
        }

        var hopMs = CwProbabilisticDecoder.HopMilliseconds;
        var unitMs = 1200.0 / result.WordsPerMinute;
        var unitHops = (int)Math.Round(unitMs / hopMs);
        var from = Math.Max(0, character.EndHop - character.SpanHops - unitHops);
        var to = Math.Min(window.Length, character.EndHop + unitHops);

        if (to - from < 2)
        {
            return true;
        }

        var (_, gaps) = CwUnitEstimator.InnerElements(new ArraySegment<double>(window, from, to - from), hopMs);

        return gaps.All(g => Math.Max(g / unitMs, unitMs / g) <= InnerGapEdge);
    }
}
