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

    /// <summary>Record a mark and re-derive the speed.</summary>
    /// <param name="samples">How long the key was down.</param>
    public void AddMark(double samples)
    {
        _marks[_markWrite] = samples;
        _markWrite = (_markWrite + 1) % WindowSize;
        _markCount = Math.Min(_markCount + 1, WindowSize);
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
        => samples < 2 * DitSamples ? CwElement.Dit : CwElement.Dah;

    /// <summary>Which gap a silence of this length is.</summary>
    /// <param name="samples">How long the key was up.</param>
    /// <returns>The gap kind.</returns>
    public CwElement ClassifyGap(double samples)
        => samples < 2 * DitSamples
            ? CwElement.ElementGap
            : samples < 5 * DitSamples
                ? CwElement.CharacterGap
                : CwElement.WordGap;

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
        => DitSamples <= 0
            ? 0
            : Math.Clamp((samples / DitSamples) - 2, 0, 1);

    /// <summary>
    /// Re-derive the dit length from the rolling windows.
    /// </summary>
    private void Recompute()
    {
        if (_markCount == 0)
        {
            return;
        }

        var (markLow, markHigh) = TwoMeans(_marks, _markCount);
        var shortestGap = ShortestGap();

        double markDit;

        if (markHigh >= 2 * markLow)
        {
            // Two clear clusters, so the lower one is the dits.
            markDit = markLow;
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
