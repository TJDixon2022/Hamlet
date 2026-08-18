namespace Hamlet.RadioEngine.Cw;

/// <summary>What a measured run of key-down or key-up turned out to be.</summary>
public enum CwElement
{
    /// <summary>A short mark.</summary>
    Dit,

    /// <summary>A long mark, three times a dit.</summary>
    Dah,

    /// <summary>The one-dit gap between elements of the same character.</summary>
    ElementGap,

    /// <summary>The three-dit gap between characters.</summary>
    CharacterGap,

    /// <summary>The seven-dit gap between words.</summary>
    WordGap,
}

/// <summary>
/// Tracks how fast the other operator is sending, and keeps tracking it,
/// because nobody is a metronome.
/// </summary>
/// <remarks>
/// <para>The dit length is re-derived from a rolling window of recent elements
/// rather than nudged along by a running average. That choice is the difference
/// between following a speed change and being destroyed by one. An average with
/// a fixed dit-or-dah boundary at twice the current estimate fails in a
/// specific and nasty way when somebody speeds up: the new dahs land under the
/// old boundary, get counted as dits, and drag the estimate the wrong way.
/// Re-clustering what was actually heard has no such trap.</para>
/// <para>Marks split cleanly in two, since a fist produces dits near one dit
/// and dahs near three. Gaps do not, since they come in ones, threes and sevens,
/// so only the shortest cluster of them is used and only when it agrees with
/// what the marks say. Averaging the two also cancels a real bias: a keyed
/// element measured against a threshold partway up its rising edge comes out
/// long by exactly as much as the gap after it comes out short.</para>
/// <para>Some text has no dits, and some has no dahs. "MOT" is three dahs and
/// "SEE" is nearly all dits, and either one alone gives a single cluster that
/// could be either. The gaps between elements settle it, because an
/// element gap is exactly one dit whatever else is going on.</para>
/// </remarks>
public sealed class CwSpeedEstimator
{
    /// <summary>How many recent marks and gaps the estimate is drawn from.</summary>
    /// <remarks>
    /// Twenty is about five characters of normal text. Long enough that one
    /// clumsy element cannot move it, short enough that a speed change is
    /// followed within a few characters rather than a paragraph.
    /// </remarks>
    public const int WindowSize = 20;

    /// <summary>How many marks are needed before the estimate is worth using.</summary>
    /// <remarks>
    /// Twelve is roughly three characters. Below that a run of similar marks is
    /// as likely to be three dahs as three dits, and a decoder that committed
    /// on that evidence would be guessing (§0.0). Characters heard before this
    /// are held and decoded once there is something to decode them against.
    /// </remarks>
    public const int MinimumMarks = 12;

    /// <summary>The dit length assumed when there is nothing else to go on.</summary>
    private const int FallbackWpm = 20;

    /// <summary>
    /// Average error, in dits, at which the marks stop looking like Morse at
    /// all.
    /// </summary>
    /// <remarks>
    /// Real sending lands its marks within about a tenth of a dit of one or
    /// three, even from a clumsy fist. Noise chopped up by a gate lands
    /// anywhere, because the run lengths are exponential and have no
    /// preferred value. Half a dit of average error is comfortably outside
    /// anything a person produces and comfortably inside what an empty band
    /// produces.
    /// </remarks>
    private const double IncoherentErrorDits = 0.5;

    /// <summary>Below this coherence nothing is claimed to have been heard.</summary>
    public const double MinimumCoherence = 0.35;

    /// <summary>The slowest sending anybody does, in words a minute.</summary>
    /// <remarks>
    /// Below about five words a minute a person is not sending Morse, they are
    /// operating a switch. Beginners on a straight key work at seven or eight,
    /// so this leaves room underneath even that.
    /// </remarks>
    public const int SlowestPlausibleWpm = 4;

    /// <summary>The fastest sending anybody does, in words a minute.</summary>
    /// <remarks>
    /// Sixty is faster than almost anyone on the air, and well past what this
    /// decoder can resolve. The point of the bound is not the ceiling, it is the
    /// floor it puts under nonsense: noise chopped up by a gate routinely comes
    /// out reading two hundred words a minute, and nothing on any band has ever
    /// been sent at two hundred words a minute.
    /// </remarks>
    public const int FastestPlausibleWpm = 60;

    private readonly double[] _marks = new double[WindowSize];

    /// <summary>Every mark this sender has produced, newest overwriting oldest.</summary>
    /// <remarks>
    /// **PER SIGNAL, NOT PER WINDOW** (HM-DEC-112 part 3). The twenty-mark
    /// window is right for following a change of speed and too short to fit two
    /// clusters against: a window holding a run of dits and one dah has nothing
    /// to separate. Two hundred and fifty-six is about a minute of sending, long
    /// enough to hold both in quantity and short enough that a different fist is
    /// forgotten within an over. It goes with the sender, like the gap classes.
    /// </remarks>
    private readonly double[] _markHistory = new double[256];

    private readonly double[] _markScratch = new double[256];

    private int _markHistoryWrite;
    private int _marksRemembered;
    private double _markBoundary;
    private readonly double[] _gaps = new double[WindowSize];

    private int _markCount;
    private int _markWrite;
    private int _gapCount;
    private int _gapWrite;

    /// <summary>Creates an estimator.</summary>
    /// <param name="sampleRate">Samples per second, for speaking in words a minute.</param>
    public CwSpeedEstimator(int sampleRate)
    {
        SampleRate = Math.Max(1_000, sampleRate);
        DitSamples = SampleRate * MorseCodeTiming.DitSeconds(FallbackWpm);
    }

    /// <summary>Samples per second.</summary>
    public int SampleRate { get; }

    /// <summary>The current dit length, in samples.</summary>
    public double DitSamples { get; private set; }

    /// <summary>How many marks the estimate currently rests on.</summary>
    public int MarkCount => _markCount;

    /// <summary>
    /// How many marks have ever been recorded, without a ceiling (HM-DEC-107).
    /// </summary>
    /// <remarks>
    /// <see cref="MarkCount"/> stops at <see cref="WindowSize"/>, which is what
    /// makes the estimate a rolling one and what makes it useless for asking
    /// "has every mark from the previous station left the window yet". This
    /// answers that, and answering it is how the speed knows to stay quiet while
    /// a new clock is being acquired.
    /// </remarks>
    public long MarksSeen { get; private set; }

    /// <summary>True once there is enough evidence to classify anything.</summary>
    public bool IsReady => _markCount >= MinimumMarks;

    /// <summary>
    /// How closely the recent marks sit to one dit or three, from 0 to 1.
    /// </summary>
    /// <remarks>
    /// THE THING THAT TELLS MORSE FROM AN EMPTY BAND. Everything else in the
    /// chain measures how strong the tone is, and none of it can tell a keyed
    /// signal from noise that happened to cross a threshold: noise crosses
    /// thresholds constantly, and a gate handed nothing at all will chop it into
    /// runs and hand them on as letters. What noise cannot do is land those runs
    /// near one dit and three dits over and over, because there is nothing
    /// making it prefer any length.
    /// </remarks>
    public double Coherence { get; private set; }

    /// <summary>
    /// True when what is being heard is actually shaped like Morse.
    /// </summary>
    /// <remarks>
    /// The decoder emits nothing at all while this is false. Not an unreadable
    /// mark, which would be saying something was heard, but nothing, because on
    /// an empty band nothing was (§0.0).
    /// </remarks>
    public bool LooksLikeMorse
        => IsReady
           && Coherence >= MinimumCoherence
           && WordsPerMinute >= SlowestPlausibleWpm
           && WordsPerMinute <= FastestPlausibleWpm;

    /// <summary>The sending speed in words a minute, PARIS standard.</summary>
    public int WordsPerMinute
    {
        get
        {
            var ditSeconds = DitSamples / SampleRate;
            return ditSeconds <= 0
                ? 0
                : (int)Math.Round(MorseCodeTiming.DitMillisecondsAt1Wpm / (ditSeconds * 1000));
        }
    }

    /// <summary>
    /// Throw away everything measured so far (HM-DEC-095).
    /// </summary>
    /// <remarks>
    /// Called when the tracker moves to a different part of the band, because
    /// every length in these windows was measured through a filter pointed
    /// somewhere else and averaging it with what comes next would carry the old
    /// mistake into the new signal.
    /// </remarks>
    public void Forget()
    {
        _markCount = 0;
        _markWrite = 0;

        // The clusters belong to a sender and go with the sender, exactly as the
        // gap classes do: two fists averaged together describe neither.
        _marksRemembered = 0;
        _markHistoryWrite = 0;
        _markBoundary = 0;
        _gapCount = 0;
        _gapWrite = 0;
        _gapCutsKnown = false;
        Coherence = 0;
    }

    /// <summary>Record a mark and re-derive the speed.</summary>
    /// <param name="samples">How long the key was down.</param>
    public void AddMark(double samples)
    {
        _markHistory[_markHistoryWrite] = samples;
        _markHistoryWrite = (_markHistoryWrite + 1) % _markHistory.Length;
        _marksRemembered = Math.Min(_marksRemembered + 1, _markHistory.Length);

        _marks[_markWrite] = samples;
        _markWrite = (_markWrite + 1) % WindowSize;
        _markCount = Math.Min(_markCount + 1, WindowSize);
        MarksSeen++;
        Recompute();
    }

    /// <summary>Record a completed gap and re-derive the speed.</summary>
    /// <param name="samples">How long the key was up.</param>
    /// <remarks>
    /// Only completed gaps, meaning ones that ended because a mark started.
    /// The silence at the end of a transmission never ends, and feeding its
    /// growing length in would convince the estimator that everybody had slowed
    /// to a crawl.
    /// </remarks>
    public void AddGap(double samples)
    {
        _gaps[_gapWrite] = samples;
        _gapWrite = (_gapWrite + 1) % WindowSize;
        _gapCount = Math.Min(_gapCount + 1, WindowSize);
        Recompute();
    }

    /// <summary>Which element a mark of this length is.</summary>
    /// <param name="samples">How long the key was down.</param>
    /// <returns>Dit or dah.</returns>
    public CwElement ClassifyMark(double samples)
        => samples < MarkBoundary ? CwElement.Dit : CwElement.Dah;

    /// <summary>
    /// Where a mark stops being a dit, in samples (HM-DEC-112 part 3).
    /// </summary>
    /// <remarks>
    /// <para>**FITTED BETWEEN THE TWO MEASURED CLUSTERS, NOT TAKEN AS A MULTIPLE
    /// OF ONE OF THEM.** Splitting at two dits is the same fault HM-DEC-115
    /// found in the gaps one level up: a boundary read off a multiple instead of
    /// off the data. It matters as soon as the dit itself moves, and taking the
    /// mark at half amplitude moves it — correcting the mark and the gap while
    /// leaving this at two dits took the suite from nine failures to
    /// twenty-three, because the dit moved under a multiple that did not move
    /// with it.</para>
    /// <para>Falls back to two dits only while there is no fit to be had, which
    /// is the acquisition period and the one place a multiple is the honest
    /// guess: there is nothing else to guess from.</para>
    /// </remarks>
    public double MarkBoundary => _markBoundary > 0 ? _markBoundary : 2 * DitSamples;

    /// <summary>Which gap a silence of this length is.</summary>
    /// <param name="samples">How long the key was up.</param>
    /// <returns>The gap kind.</returns>
    /// <remarks>
    /// <para>**THE GAPS ARE CLASSIFIED BY CLUSTERING THE GAPS, NOT BY COUNTING
    /// DITS** (HM-DEC-095). Textbook Morse spaces elements one dit apart,
    /// characters three and words seven, and almost nobody sends that way. The
    /// station recorded answering a call on 40 m sends dits of about a hundred
    /// milliseconds with element gaps of seventy, which is shorter than its own
    /// dit, and character gaps of about a hundred and forty, which is one and a
    /// half dits rather than three.</para>
    /// <para>Against fixed multiples every one of those gaps is an element gap,
    /// so the whole transmission arrives as a single run of thirty-odd elements
    /// and decodes to nothing. That is precisely what it did: one fifteen-element
    /// pattern, marked unreadable, out of a station whose callsign is plainly
    /// there.</para>
    /// <para>The clusters are used only when the gaps actually fall into
    /// separated groups. A transmission that never pauses has one group and
    /// nothing to learn from, and then the textbook multiples are the better
    /// guess and are what this falls back to.</para>
    /// </remarks>
    public CwElement ClassifyGap(double samples)
    {
        if (!_gapCutsKnown)
        {
            return samples < 2 * DitSamples
                ? CwElement.ElementGap
                : samples < 5 * DitSamples
                    ? CwElement.CharacterGap
                    : CwElement.WordGap;
        }

        return samples < _elementGapCut
            ? CwElement.ElementGap
            : samples < _characterGapCut
                ? CwElement.CharacterGap
                : CwElement.WordGap;
    }

    /// <summary>Where a gap stops being an element gap, in samples.</summary>
    /// <remarks>Measured from the gaps themselves where they separate cleanly,
    /// and two dits otherwise.</remarks>
    public double ElementGapBoundary
        => _gapCutsKnown ? _elementGapCut : 2 * DitSamples;

    /// <summary>True when the gaps fell into groups the estimator could measure.</summary>
    public bool GapsAreClustered => _gapCutsKnown;

    /// <summary>
    /// How far a measurement sat from the decision that was made about it, from
    /// 0 at the boundary to 1 at the textbook length.
    /// </summary>
    /// <param name="element">What it was classified as.</param>
    /// <param name="samples">How long it actually was.</param>
    /// <returns>Clarity from 0 to 1.</returns>
    /// <remarks>
    /// THIS IS HALF THE CONFIDENCE MODEL, and it is a measurement rather than a
    /// feeling. An element landing exactly on the dit-or-dah boundary was a
    /// coin toss, and the reader has to be told that. One landing on the
    /// textbook length was not, and the reader deserves to know that too. What
    /// this cannot do is round the first case up to make the transcript look
    /// tidier (§0.0).
    /// </remarks>
    public double Clarity(CwElement element, double samples)
    {
        var dit = DitSamples;
        if (dit <= 0)
        {
            return 0;
        }

        var units = samples / dit;

        // **A GAP IS SCORED AGAINST THE BOUNDARY IT WAS JUDGED BY** (HM-DEC-095).
        // Where this sender's own gaps were measured, the textbook multiples are
        // not the decision that was taken, so scoring against them reports a
        // confidence for a judgement nothing made. On a compressed fist every
        // character gap read as zero and every letter came out unreadable while
        // the classification above it was perfectly correct.
        if (_gapCutsKnown && element is CwElement.ElementGap
            or CwElement.CharacterGap or CwElement.WordGap)
        {
            // **NOUGHT AT THE BOUNDARY AND ONE AT THE MIDDLE OF ITS OWN GROUP**,
            // which is the same meaning the textbook version has and the only
            // scale that survives a sender whose spacing is nothing like the
            // textbook. Normalizing against the boundary's own value instead
            // scored every character gap in a clean recording at four tenths and
            // marked a perfect decode uncertain (HM-DEC-095).
            return element switch
            {
                CwElement.ElementGap => Toward(
                    samples, _elementGapCut, _elementGapMean),

                CwElement.CharacterGap => Math.Min(
                    Toward(samples, _elementGapCut, _characterGapMean),
                    Toward(samples, _characterGapCut, _characterGapMean)),

                _ => Toward(samples, _characterGapCut, _wordGapMean),
            };
        }

        return element switch
        {
            // Ideal 1, boundary 2.
            CwElement.Dit => Clamp(2 - units),

            // Ideal 3, boundary 2. Anything past 4 is unambiguously long.
            CwElement.Dah => Clamp(units - 2),

            // Ideal 1, boundary 2.
            CwElement.ElementGap => Clamp(2 - units),

            // Ideal 3, boundaries 2 and 5, so the nearer one decides.
            CwElement.CharacterGap => Clamp(Math.Min(units - 2, 5 - units)),

            // Ideal 7, boundary 5, and the silence at the end of a
            // transmission is as unambiguous as a gap gets.
            CwElement.WordGap => Clamp((units - 5) / 2),

            _ => 0,
        };

        static double Clamp(double v) => Math.Clamp(v, 0, 1);
    }

    /// <summary>
    /// How far a measurement has travelled from a boundary toward the middle of
    /// the group it was placed in.
    /// </summary>
    /// <param name="samples">The measurement.</param>
    /// <param name="boundary">The decision it was on one side of.</param>
    /// <param name="center">The middle of that group.</param>
    /// <returns>Nought at the boundary, one at the center or beyond.</returns>
    private static double Toward(double samples, double boundary, double center)
    {
        var reach = Math.Abs(center - boundary);

        return reach <= 0
            ? 0
            : Math.Clamp(Math.Abs(samples - boundary) / reach, 0, 1);
    }

    /// <summary>
    /// How sure the decoder is that a gap really ended the character, from 0 at
    /// the boundary to 1 well past it.
    /// </summary>
    /// <param name="samples">How long the key was up.</param>
    /// <returns>Clarity from 0 to 1.</returns>
    /// <remarks>
    /// ONLY THE LOW BOUNDARY BEARS ON THE CHARACTER, which is why this is not
    /// the same as scoring the gap. A silence halfway between one dit and three
    /// is the difference between hearing "U" and hearing "IT", so it belongs in
    /// the character's confidence. A silence halfway between three dits and
    /// seven only decides whether a space follows, and letting that drag a
    /// perfectly clear letter down would be marking the reader's copy wrong for
    /// something that was never about the letter.
    /// </remarks>
    public double EndOfCharacterClarity(double samples)
    {
        if (DitSamples <= 0)
        {
            return 0;
        }

        // Against the boundary that was actually used, for the reason given on
        // Clarity above (HM-DEC-095).
        return _gapCutsKnown
            ? Toward(samples, _elementGapCut, _characterGapMean)
            : Math.Clamp((samples / DitSamples) - 2, 0, 1);
    }

    /// <summary>
    /// Re-derive the dit length from the rolling windows.
    /// </summary>
    private void Recompute()
    {
        if (_markCount == 0)
        {
            return;
        }

        RecomputeMarkBoundary();

        var (markLow, markHigh) = TwoMeans(_marks, _markCount);
        var shortestGap = ShortestGap();

        double markDit;

        if (markHigh >= 2 * markLow)
        {
            // **THE MIDDLE OF THE SHORT CLUSTER, NOT ITS AVERAGE** (HM-DEC-095).
            // A handful of very short marks survive the gate on any real signal,
            // and an average is defenseless against them: on the recording that
            // prompted this, marks of fifteen and twenty milliseconds among dits
            // of a hundred pulled the estimate to seventy-two and put the speed
            // at seventeen words a minute against a true twelve.
            //
            // That is not a small error in a number. The dit is what every later
            // judgement is measured against, so a dit thirty percent short makes
            // every element look long, drives the coherence check below its own
            // limit, and the decoder discards a message it has correctly heard
            // because it no longer believes the timings are Morse.
            markDit = MedianOfShortCluster((markLow + markHigh) / 2);
        }
        else if (shortestGap > 0 && markLow >= 2 * shortestGap)
        {
            // One cluster, and it is three times the shortest gap there is.
            // These are all dahs, which is what "MOT" or a run of Os looks
            // like, and the element gaps inside them are what gives it away.
            markDit = markLow / 3;
        }
        else
        {
            // One cluster and nothing to contradict it: these are dits.
            markDit = markLow;
        }

        if (markDit <= 0)
        {
            return;
        }

        DitSamples = Refine(markDit);
        Coherence = MeasureCoherence();
        RecomputeGapCuts();
    }

    /// <summary>
    /// The middle mark of those shorter than a threshold.
    /// </summary>
    /// <param name="threshold">Where the dits stop and the dahs begin.</param>
    /// <returns>The median, or zero when nothing is below it.</returns>
    private double MedianOfShortCluster(double threshold)
    {
        var count = 0;

        for (var i = 0; i < _markCount; i++)
        {
            if (_marks[i] < threshold)
            {
                _sorted[count++] = _marks[i];
            }
        }

        if (count == 0)
        {
            return 0;
        }

        Array.Sort(_sorted, 0, count);
        return _sorted[count / 2];
    }

    private readonly double[] _sorted = new double[WindowSize];

    /// <summary>How many gaps before their own grouping is worth believing.</summary>
    private const int MinimumGapsForCuts = 8;

    /// <summary>
    /// How far apart two groups must sit before they count as two groups.
    /// </summary>
    /// <remarks>
    /// Half as long again. Below that it is one spread of gaps being cut in half,
    /// which would invent a character boundary in the middle of a letter.
    /// </remarks>
    private const double GroupSeparation = 1.6;

    private readonly double[] _longGaps = new double[WindowSize];

    private double _elementGapCut;
    private double _characterGapCut;
    private double _elementGapMean;
    private double _characterGapMean;
    private double _wordGapMean;
    private bool _gapCutsKnown;

    /// <summary>
    /// Find where this sender's own gaps divide (HM-DEC-095).
    /// </summary>
    private void RecomputeGapCuts()
    {
        _gapCutsKnown = false;

        if (_gapCount < MinimumGapsForCuts)
        {
            return;
        }

        var (low, high, elementCut) = SplitAtMean(_gaps, _gapCount);

        // One group of gaps means somebody sending without pausing, and there is
        // nothing here to learn from.
        if (low <= 0 || high < GroupSeparation * low)
        {
            return;
        }

        var longCount = 0;

        for (var i = 0; i < _gapCount; i++)
        {
            if (_gaps[i] >= elementCut)
            {
                _longGaps[longCount++] = _gaps[i];
            }
        }

        // The longer group splits again into character gaps and word gaps, when
        // there are enough of them and they genuinely separate. A transmission
        // with no word gaps in it has one group up here, and inventing a split
        // would put spaces inside somebody's callsign.
        //
        // **THE FALLBACK IS THE CONVENTION, NOT INFINITY.** Saying "no word gaps
        // are possible" whenever too few have been heard yet deleted every space
        // from a clean recording: "CQ DE W1AW K" came back as "CQDEW1AWK". Where
        // there is no measurement, the textbook five dits is the honest guess,
        // and it is exactly what this code used before any of it was measured.
        // Where the long gaps have not separated yet, the whole judgement up here
        // is the textbook one, so the centers have to be the textbook ones too.
        // Using the measured mean of every long gap put the character center up
        // among the word gaps and scored perfectly ordinary character gaps at a
        // fifth, which marked the first letters of a clean recording uncertain.
        var characterCut = 5 * DitSamples;
        var characterMean = 3 * DitSamples;
        var wordMean = 7 * DitSamples;

        if (longCount >= 4)
        {
            var (wordLow, wordHigh, wordCut) = SplitAtMean(_longGaps, longCount);

            if (wordLow > 0 && wordHigh >= GroupSeparation * wordLow)
            {
                characterCut = wordCut;
                characterMean = wordLow;
                wordMean = wordHigh;
            }
        }

        _elementGapCut = elementCut;
        _characterGapCut = characterCut;
        _elementGapMean = low;
        _characterGapMean = characterMean;
        _wordGapMean = wordMean;
        _gapCutsKnown = true;
    }

    /// <summary>
    /// Split values in two, seeded from their own mean.
    /// </summary>
    /// <param name="values">The values.</param>
    /// <param name="count">How many of them.</param>
    /// <returns>The two centers and the level between them.</returns>
    /// <remarks>
    /// <para>**THE SEEDING IS THE WHOLE DIFFERENCE, AND GETTING IT WRONG LOOKED
    /// EXACTLY LIKE GETTING IT RIGHT** (HM-DEC-095). <see cref="TwoMeans"/> seeds
    /// from the smallest and largest value and assigns each point to the nearer
    /// center, which is correct for marks, where dits and dahs arrive in
    /// comparable numbers.</para>
    /// <para>Gaps are not shaped like that. A transmission is mostly element
    /// gaps, with a scattering of character gaps and one or two word gaps, so
    /// seeding from the extremes puts the character gaps in with the element gaps
    /// and lands the boundary between "everything short" and "the word gap".
    /// Every character then runs into the next one. Seeding from the mean instead
    /// lets the crowded short cluster hold the low center and the boundary falls
    /// where the characters divide.</para>
    /// <para>Measured on the real recording: seeded from the extremes the
    /// boundary lands near 195 ms and nothing separates, because that fist's
    /// character gaps are 140. Seeded from the mean it lands near 122 and the
    /// callsign comes out.</para>
    /// </remarks>
    private static (double Low, double High, double Threshold) SplitAtMean(
        double[] values, int count)
    {
        var threshold = 0.0;

        for (var i = 0; i < count; i++)
        {
            threshold += values[i];
        }

        threshold /= count;

        var low = threshold;
        var high = threshold;

        for (var pass = 0; pass < 12; pass++)
        {
            double lowSum = 0, highSum = 0;
            int lowCount = 0, highCount = 0;

            for (var i = 0; i < count; i++)
            {
                if (values[i] < threshold)
                {
                    lowSum += values[i];
                    lowCount++;
                }
                else
                {
                    highSum += values[i];
                    highCount++;
                }
            }

            if (lowCount == 0 || highCount == 0)
            {
                return (threshold, threshold, threshold);
            }

            low = lowSum / lowCount;
            high = highSum / highCount;

            var next = (low + high) / 2;

            if (Math.Abs(next - threshold) < 1e-9)
            {
                break;
            }

            threshold = next;
        }

        return (low, high, threshold);
    }

    /// <summary>
    /// How far the recent marks sit, on average, from the nearest textbook
    /// length.
    /// </summary>
    private double MeasureCoherence()
    {
        if (_markCount == 0 || DitSamples <= 0)
        {
            return 0;
        }

        var error = 0.0;

        for (var i = 0; i < _markCount; i++)
        {
            var units = _marks[i] / DitSamples;
            var ideal = units < 2 ? 1.0 : 3.0;
            error += Math.Abs(units - ideal);
        }

        return Math.Clamp(1 - (error / _markCount / IncoherentErrorDits), 0, 1);
    }

    /// <summary>
    /// Sharpen the dit length using the gaps the marks have just identified.
    /// </summary>
    /// <remarks>
    /// <para>The marks set the scale, so a gap can now be recognized as an
    /// element gap rather than clustered blindly. That order matters and getting
    /// it backwards was a real bug: gaps come in ones, threes and sevens, so
    /// splitting them in two puts the ones and the threes together and yields a
    /// dit a quarter too long. Every dah then lands halfway to the boundary,
    /// every character comes out marked uncertain, and the speed reads low by
    /// exactly that much.</para>
    /// <para>Averaging the two estimates cancels a small real bias. A mark
    /// measured against a threshold partway up its rising edge comes out long by
    /// the same amount the gap after it comes out short, so the mean of the two
    /// is the truth even when neither is.</para>
    /// <para>It takes a few element gaps to be worth doing. A message whose
    /// characters are all one element long has none at all, and there is nothing
    /// there to refine with.</para>
    /// </remarks>
    private double Refine(double markDit)
    {
        var sum = 0.0;
        var count = 0;

        for (var i = 0; i < _gapCount; i++)
        {
            if (_gaps[i] < 2 * markDit)
            {
                sum += _gaps[i];
                count++;
            }
        }

        return count >= 3 ? (markDit + (sum / count)) / 2 : markDit;
    }

    /// <summary>The shortest gap in the window, or zero when there are none.</summary>
    private double ShortestGap()
    {
        var shortest = 0.0;

        for (var i = 0; i < _gapCount; i++)
        {
            if (shortest == 0 || _gaps[i] < shortest)
            {
                shortest = _gaps[i];
            }
        }

        return shortest;
    }

    /// <summary>
    /// Split values into two clusters and return their centers.
    /// </summary>
    /// <remarks>
    /// Lloyd's algorithm on one dimension, seeded from the smallest and largest
    /// values, which converges in a handful of passes on data this shaped.
    /// Deliberately allocation-free and deterministic: no random seeding, no
    /// sorting, and the same window always gives the same answer (§5).
    /// </remarks>
    /// <summary>
    /// Fit the dit and dah clusters and put the boundary between them
    /// (HM-DEC-112 part 3).
    /// </summary>
    /// <remarks>
    /// <para>**THE SAME SHAPE AS THE GAP FIT, AND FOR THE SAME REASONS.** Log
    /// space, because a mark twice as long as another is the same distance apart
    /// whatever the sender's speed. Seeded on percentiles rather than on the
    /// smallest and largest mark, because seeding on the extremes puts a centre
    /// on whatever the shortest sliver of noise was and leaves it there — the
    /// finding that made the gap fit work, and marks want it too. Dits outnumber
    /// dahs in ordinary English, so a quarter and four fifths land one in each
    /// cluster.</para>
    /// <para>**AND IT REFUSES RATHER THAN INVENTING A BOUNDARY.** Two clusters
    /// less than half as long again apart are one spread of marks being cut in
    /// half, and a sender who has only sent dits so far has no dah to find. The
    /// caller falls back to two dits, which is honest while there is nothing
    /// else to go on (§0.0).</para>
    /// </remarks>
    private void RecomputeMarkBoundary()
    {
        _markBoundary = 0;

        var usable = 0;

        for (var i = 0; i < _marksRemembered; i++)
        {
            if (_markHistory[i] > 0)
            {
                _markScratch[usable++] = Math.Log(_markHistory[i]);
            }
        }

        if (usable < MinimumMarksForBoundary)
        {
            return;
        }

        Array.Sort(_markScratch, 0, usable);

        var low = _markScratch[usable / 4];
        var high = _markScratch[Math.Min(usable - 1, usable * 4 / 5)];

        for (var pass = 0; pass < 24; pass++)
        {
            double lowSum = 0, highSum = 0;
            int lowCount = 0, highCount = 0;

            for (var i = 0; i < usable; i++)
            {
                if (Math.Abs(_markScratch[i] - low) <= Math.Abs(_markScratch[i] - high))
                {
                    lowSum += _markScratch[i];
                    lowCount++;
                }
                else
                {
                    highSum += _markScratch[i];
                    highCount++;
                }
            }

            if (lowCount == 0 || highCount == 0)
            {
                return;
            }

            low = lowSum / lowCount;
            high = highSum / highCount;
        }

        if (high - low < MarkSeparation)
        {
            return;
        }

        // Halfway between the centres in log space, which on the wire is their
        // geometric mean: the same place the settled pass already cuts.
        _markBoundary = Math.Exp((low + high) / 2);
    }

    /// <summary>How many marks are needed before the clusters can be fitted.</summary>
    /// <remarks>
    /// Sixteen. Fewer and a handful of dits with one dah among them will fit two
    /// clusters that are really one, and the fallback is the better answer until
    /// there is something to measure.
    /// </remarks>
    private const int MinimumMarksForBoundary = 16;

    /// <summary>How far apart the two mark clusters must sit, in log units.</summary>
    /// <remarks>
    /// About half as long again, the same separation the gap classes demand. A
    /// textbook one to three clears it comfortably, and so does every fist this
    /// project has measured: the tightest was 2.79.
    /// </remarks>
    private const double MarkSeparation = 0.405;

    private static (double Low, double High) TwoMeans(double[] values, int count)
    {
        var low = double.MaxValue;
        var high = double.MinValue;

        for (var i = 0; i < count; i++)
        {
            low = Math.Min(low, values[i]);
            high = Math.Max(high, values[i]);
        }

        if (high <= low)
        {
            return (low, low);
        }

        for (var pass = 0; pass < 12; pass++)
        {
            var lowSum = 0.0;
            var lowCount = 0;
            var highSum = 0.0;
            var highCount = 0;

            for (var i = 0; i < count; i++)
            {
                if (Math.Abs(values[i] - low) <= Math.Abs(values[i] - high))
                {
                    lowSum += values[i];
                    lowCount++;
                }
                else
                {
                    highSum += values[i];
                    highCount++;
                }
            }

            var nextLow = lowCount > 0 ? lowSum / lowCount : low;
            var nextHigh = highCount > 0 ? highSum / highCount : high;

            if (Math.Abs(nextLow - low) < 1e-6 && Math.Abs(nextHigh - high) < 1e-6)
            {
                return (nextLow, nextHigh);
            }

            low = nextLow;
            high = nextHigh;
        }

        return (low, high);
    }
}

/// <summary>
/// The PARIS timing standard, in the engine's decode half.
/// </summary>
/// <remarks>
/// The same arithmetic the sending side uses, kept here so the decoder does
/// not reach across into the training namespace for a constant. One dit is
/// 1200/WPM milliseconds, a dah is three, the gap inside a character is one,
/// between characters three, between words seven.
/// </remarks>
public static class MorseCodeTiming
{
    /// <summary>Milliseconds in one dit at one word per minute.</summary>
    public const double DitMillisecondsAt1Wpm = 1200.0;

    /// <summary>One dit's length in seconds at a given speed.</summary>
    /// <param name="wordsPerMinute">Speed in words a minute.</param>
    /// <returns>Seconds.</returns>
    public static double DitSeconds(int wordsPerMinute)
        => DitMillisecondsAt1Wpm / Math.Max(1, wordsPerMinute) / 1000.0;
}
