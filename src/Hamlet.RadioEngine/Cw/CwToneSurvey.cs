namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// A pitch that looks like somebody keying, and the measurements that say so.
/// </summary>
/// <param name="ToneHz">Where it is.</param>
/// <param name="DitMilliseconds">The shorter of the two mark lengths found.</param>
/// <param name="DahMilliseconds">The longer of them.</param>
/// <param name="Ratio">The longer over the shorter. Morse is about three.</param>
/// <param name="Separation">
/// How far apart the two mark lengths sit, measured in their own scatter. This
/// is the number that separates keying from noise.
/// </param>
/// <param name="LiftDb">How far the keyed level stands over the band beside it.</param>
/// <param name="Marks">How many clean marks the measurement rests on.</param>
/// <param name="KeyedDb">
/// How loud this bin is while the key is down, in the same units the bins are
/// measured in. Where the lift cannot be worked out, this is what ranks one bin
/// against another.
/// </param>
public readonly record struct KeyingCandidate(
    double ToneHz,
    double DitMilliseconds,
    double DahMilliseconds,
    double Ratio,
    double Separation,
    double LiftDb,
    int Marks,
    double KeyedDb = double.NaN)
{
    /// <summary>The sending speed these element lengths imply.</summary>
    public double WordsPerMinute
        => DitMilliseconds > 0 ? 1200.0 / DitMilliseconds : 0;
}

/// <summary>
/// Something strong enough to matter that is not somebody sending Morse.
/// </summary>
/// <param name="ToneHz">Where it was measured.</param>
/// <param name="LiftDb">How far it stands over the band beside it.</param>
/// <param name="PresentFraction">
/// How much of the time it was above the band at all, between nought and one.
/// </param>
/// <remarks>
/// **THIS SAYS WHAT WAS MEASURED AND NOTHING ABOUT WHOSE IT IS** (§0.0). Hamlet
/// has no way to tell a stuck carrier from a switching power supply from a
/// station running a mode it cannot read, and the operator has a receiver and
/// forty years of ears. What is useful is the frequency and the strength, which
/// are facts, and the reason it matters, which is that anything loud inside the
/// filter sets the receiver's gain for everything quieter.
/// </remarks>
public readonly record struct ToneInterference(
    double ToneHz, double LiftDb, double PresentFraction);

/// <summary>What the survey found across the whole range it listens to.</summary>
/// <param name="Keyed">The best keying candidate, or null when there is none.</param>
/// <param name="Interference">The strongest thing that is not keying, if any.</param>
/// <param name="Strongest">
/// The loudest thing found, keyed or not, so a caller that rejects the keying
/// verdict does not also lose the fact that something is there.
/// </param>
/// <remarks>
/// **KEEPING THESE APART IS NOT TIDINESS.** The tracker refuses a keying
/// candidate that has not been seen twice, and while it is refusing one the
/// signal underneath it is still sitting in the passband setting the receiver's
/// gain. Folding the two together meant a rejected candidate took the report of
/// its own existence down with it, and the recording with a carrier plainly in it
/// reported nothing at all (HM-DEC-095).
/// </remarks>
public readonly record struct ToneVerdict(
    KeyingCandidate? Keyed,
    ToneInterference? Interference,
    ToneInterference? Strongest = null)
{
    /// <summary>Nothing found either way.</summary>
    public static ToneVerdict Empty { get; } = new(null, null, null);
}

/// <summary>
/// Chooses which note to decode by how it is keyed, not by how loud it is
/// (HM-DEC-095).
/// </summary>
/// <remarks>
/// <para>**THE OLD TRACKER PICKED THE STRONGEST BIN AND WAS WRONG ON EVERY
/// RECORDING THIS PROJECT HAS.** It reported 600 Hz where the station was at 613,
/// 575 where it was at 612, and 375 on a file whose loudest signal is at 500. The
/// last of those is the diagnostic one: a detector that lands on none of the
/// strongest thing, the real thing, or the configured pitch is not measuring the
/// audio at all.</para>
/// <para>Two faults sat underneath it. Bins were spaced twenty-five hertz apart,
/// so an exact answer was arithmetically impossible; and the tie-break preferred
/// whichever bin was nearest where the tracker already sat, which was seeded from
/// the operator's own pitch setting. That is a measurement pulled toward the
/// answer somebody typed in, which is the shape of error §0.0 exists to
/// prevent.</para>
/// <para>**WHAT REPLACES IT IS THE ONE QUESTION THAT ACTUALLY DISTINGUISHES
/// MORSE: are the mark lengths two clusters or one smear?** Everything cheaper
/// was tried against the three recordings first and every one of them failed:
/// </para>
/// <list type="bullet">
/// <item>Loudness picks a carrier over a station, which is the reported fault.</item>
/// <item>Duty cycle does not separate them. The keyed station in the 01:33
/// recording holds the band for seventy-nine percent of the time it is on and
/// the unkeyed signal in the 13:47 one for forty.</item>
/// <item>The one-to-three ratio on its own passes almost every empty bin.
/// Cutting any smooth spread of durations in two yields a short group and a long
/// group whose means land near one to three by construction, so noise scores as
/// well as Morse.</item>
/// <item>Absolute element lengths help and are not enough: noise routinely
/// produces twenty-five millisecond marks, which is a legal dit at forty-eight
/// words a minute.</item>
/// </list>
/// <para>What noise has never got is a **gap** between the two groups. A real
/// fist sends dits within a few percent of each other and dahs likewise, so the
/// two clusters sit apart by many times their own scatter. Measured across these
/// recordings the keyed station scores fifteen and the best empty bin scores
/// under three, which is a margin wide enough to set a limit inside without
/// tuning it to one file.</para>
/// <para>**AND THE BRIEF'S OWN HYPOTHESIS WAS TESTED AND DOES NOT HOLD.** It
/// proposed disqualifying any bin that is continuously on. Measured, the signal
/// it was written about is not continuously on, and the rule would have been a
/// branch that never ran while the real fault went unfixed.</para>
/// </remarks>
public sealed class CwToneSurvey
{
    /// <summary>
    /// How far apart the two mark lengths must sit, in their own scatter.
    /// </summary>
    /// <remarks>
    /// **FOUR, AND BOTH SIDES OF IT ARE MEASURED** (HM-DEC-095). Against the
    /// three recordings in this repository the keyed station scores between
    /// eleven and sixteen depending on which bin is asked, and the best bin of
    /// the two recordings holding no readable station scores two point eight.
    /// Four sits between them with room on both sides, and it is a ratio rather
    /// than a level, so it does not move when a signal fades.
    /// </remarks>
    public const double MinimumSeparation = 4.0;

    /// <summary>The smallest dah-to-dit ratio that is still Morse.</summary>
    /// <remarks>
    /// Three is the standard and real fists wander either side of it. Below two
    /// and a half the two lengths are not being sent as different things at all.
    /// </remarks>
    public const double MinimumRatio = 2.5;

    /// <summary>The largest dah-to-dit ratio that is still Morse.</summary>
    public const double MaximumRatio = 3.8;

    /// <summary>The shortest dit anybody could be sending, in milliseconds.</summary>
    /// <remarks>Twenty-five, which is forty-eight words a minute, the fastest
    /// this radio's own keyer will go (`14 0C`, p. 19-3).</remarks>
    public const double ShortestDitMs = 25;

    /// <summary>The longest dit worth calling Morse, in milliseconds.</summary>
    /// <remarks>Two hundred, which is six words a minute, slower than anybody
    /// sends on the air.</remarks>
    public const double LongestDitMs = 200;

    /// <summary>How many clean marks a verdict has to rest on.</summary>
    public const int MinimumMarks = 8;

    /// <summary>
    /// How far over the band something has to stand to be worth naming when it
    /// is not keying.
    /// </summary>
    /// <remarks>
    /// Ten decibels. Below that it is band noise having a good moment; above it
    /// there is something there, and anything there inside the filter is setting
    /// the receiver's gain for everything quieter (HM-DEC-095).
    /// </remarks>
    public const double InterferenceLiftDb = 10;

    /// <summary>Half the hysteresis span, in decibels.</summary>
    /// <remarks>
    /// Three either way, so six across, which is the figure the validated
    /// receive chain uses. Without it a signal resting on the threshold chatters
    /// into a run of imaginary dits and takes the cluster measurement with it.
    /// </remarks>
    private const double HysteresisDb = 3.0;

    /// <summary>How far away a bin has to be to count as "the band" rather than
    /// this signal leaking sideways.</summary>
    private const double NoiseSeparationHz = 125;

    private readonly double[] _binHz;
    private readonly float[] _history;
    private readonly bool[] _blocked;
    private readonly int _capacity;
    private readonly int _bins;
    private readonly double _hopSeconds;
    private readonly int _deglitch;

    private readonly bool[] _mask;
    private readonly bool[] _voted;
    private readonly double[] _marks;
    private readonly double[] _scratch;
    private readonly double[] _baseline;
    private readonly double[] _bandNoise;

    private int _write;
    private int _filled;

    /// <summary>Creates a survey.</summary>
    /// <param name="binHz">The pitches watched, ascending.</param>
    /// <param name="hopSeconds">How long one measurement covers.</param>
    /// <param name="seconds">How much history to keep.</param>
    public CwToneSurvey(double[] binHz, double hopSeconds, double seconds = 3.0)
    {
        ArgumentNullException.ThrowIfNull(binHz);

        _binHz = binHz;
        _bins = binHz.Length;
        _hopSeconds = hopSeconds > 0 ? hopSeconds : 0.01;
        _capacity = Math.Max(32, (int)Math.Round(seconds / _hopSeconds));

        _history = new float[_bins * _capacity];
        _blocked = new bool[_capacity];
        _mask = new bool[_capacity];
        _voted = new bool[_capacity];
        _marks = new double[_capacity];
        _scratch = new double[_capacity];
        _baseline = new double[_bins];
        _bandNoise = new double[_bins];

        // Twenty milliseconds, which is what the validated chain de-glitches at
        // before it knows the speed. Odd, so a median has a middle.
        var hops = Math.Max(1, (int)Math.Round(0.020 / _hopSeconds));
        _deglitch = hops % 2 == 0 ? hops + 1 : hops;
    }

    /// <summary>How much history has been gathered, in seconds.</summary>
    public double HistorySeconds => _filled * _hopSeconds;

    /// <summary>True when there is enough history to say anything.</summary>
    public bool IsReady => _filled >= _capacity / 2;

    /// <summary>
    /// Record one measurement across every bin.
    /// </summary>
    /// <param name="binDb">Energy in each bin, in decibels, in bin order.</param>
    /// <param name="blocked">
    /// True when this measurement covers the operator's own transmission, which
    /// is not evidence about anybody else (HM-DEC-095).
    /// </param>
    public void Observe(ReadOnlySpan<double> binDb, bool blocked)
    {
        if (binDb.Length != _bins)
        {
            return;
        }

        var at = _write * _bins;

        for (var b = 0; b < _bins; b++)
        {
            _history[at + b] = (float)binDb[b];
        }

        _blocked[_write] = blocked;
        _write = (_write + 1) % _capacity;
        _filled = Math.Min(_filled + 1, _capacity);
    }

    /// <summary>Forget everything, because the bins now mean different pitches.</summary>
    public void Reset()
    {
        _write = 0;
        _filled = 0;
    }

    /// <summary>
    /// Read what is out there.
    /// </summary>
    /// <returns>The best keying candidate and the strongest thing that is not.</returns>
    /// <remarks>
    /// Runs over stored history rather than per measurement, so it costs nothing
    /// on the audio thread between calls and allocates nothing at all (§8).
    /// </remarks>
    public ToneVerdict Analyze()
    {
        if (!IsReady)
        {
            return ToneVerdict.Empty;
        }

        MeasureBandNoise();

        KeyingCandidate? best = null;
        ToneInterference? unkeyed = null;
        ToneInterference? strongest = null;

        for (var b = 0; b < _bins; b++)
        {
            var candidate = Examine(b, _bandNoise[b], out var lift, out var present);

            if (!double.IsNaN(lift)
                && (strongest is not { } peak || lift > peak.LiftDb))
            {
                strongest = new ToneInterference(_binHz[b], lift, present);
            }

            if (candidate is { } keyed)
            {
                if (Beats(keyed, best))
                {
                    best = keyed;
                }

                continue;
            }

            // Not keying. Loud enough to be worth naming?
            if (lift >= InterferenceLiftDb
                && (unkeyed is not { } other || lift > other.LiftDb))
            {
                unkeyed = new ToneInterference(_binHz[b], lift, present);
            }
        }

        // A candidate that IS the loudest thing is not also interference with
        // itself. Only something else on the band counts.
        if (best is { } found && unkeyed is { } noise
            && Math.Abs(noise.ToneHz - found.ToneHz) < NoiseSeparationHz)
        {
            unkeyed = null;
        }

        if (strongest is { } loudest && loudest.LiftDb < InterferenceLiftDb)
        {
            strongest = null;
        }

        return new ToneVerdict(best, unkeyed, strongest);
    }

    /// <summary>
    /// Every bin the survey would admit as keying, for diagnosis only.
    /// </summary>
    /// <returns>One entry per admitted bin, in bin order.</returns>
    /// <remarks>
    /// **THE VERDICT SAYS WHICH BIN WON AND NEVER WHAT IT BEAT** (§0.0.1). Three
    /// times on the sensitivity sweep the tracker left a station it was reading
    /// for a bin holding noise, and there was no way to see what the losing bins
    /// scored without adding one. This changes nothing the survey decides; it
    /// runs the same examination and hands back what it found.
    /// </remarks>
    public IReadOnlyList<KeyingCandidate> Candidates()
    {
        if (!IsReady)
        {
            return Array.Empty<KeyingCandidate>();
        }

        MeasureBandNoise();

        var found = new List<KeyingCandidate>();

        for (var b = 0; b < _bins; b++)
        {
            if (Examine(b, _bandNoise[b], out _, out _) is { } candidate)
            {
                found.Add(candidate);
            }
        }

        return found;
    }

    /// <summary>
    /// Is this candidate better than the best so far?
    /// </summary>
    /// <remarks>
    /// <para>**TWO RANKINGS, BECAUSE THE TWO STAGES ARE ASKING DIFFERENT
    /// QUESTIONS.** Across the whole range the question is which of several
    /// signals to read, and the answer is the loudest one that is being keyed.
    /// Within thirty hertz of one signal the question is which bin resolves that
    /// signal's keying most cleanly, and loudness barely varies across bins that
    /// close.</para>
    /// <para>It also has to be this way round for a duller reason worth recording,
    /// because it cost an afternoon. A bank only sixty hertz wide has no bin far
    /// enough from any other to sample the band beside it, so every lift in it is
    /// unmeasurable. Comparing those with a greater-than silently kept whichever
    /// bin came first, and the fine stage returned the bottom of its own range
    /// every time: a reported pitch of 595 Hz on a station at 613.</para>
    /// </remarks>
    private static bool Beats(KeyingCandidate candidate, KeyingCandidate? best)
    {
        if (best is not { } current)
        {
            return true;
        }

        var known = !double.IsNaN(candidate.LiftDb);
        var wasKnown = !double.IsNaN(current.LiftDb);

        if (known && wasKnown)
        {
            return candidate.LiftDb > current.LiftDb;
        }

        // **THE LOUDEST KEY-DOWN, NOT THE SHARPEST CLUSTERING** (HM-DEC-095).
        // Inside a bank thirty hertz wide every bin hears the same signal, so
        // they all cluster about equally well and ranking on it picks
        // essentially at random: a clean tone at exactly 600 Hz was reported at
        // 570, the bottom of its own bank. How much of the signal a bin actually
        // catches is what says which one is pointed at it.
        return known
            || (!wasKnown && candidate.KeyedDb > current.KeyedDb);
    }

    /// <summary>
    /// What the band is doing beside each bin, separately for each one.
    /// </summary>
    /// <remarks>
    /// <para>**ONE NUMBER FOR THE WHOLE RANGE IS NOT GOOD ENOUGH, AND THE
    /// SYMPTOM WAS A CARRIER NOBODY REPORTED.** The receiver's own filter is five
    /// hundred hertz wide and the survey listens across six hundred, so the bins
    /// at either end sit on the filter's skirt and are ten or fifteen decibels
    /// quieter than the middle for reasons that have nothing to do with what is on
    /// the air. A single median across all of them lands between the two and
    /// overstates the floor in the middle, which is exactly where signals are.</para>
    /// <para>Each bin's own baseline is its middle level over time, and the band
    /// beside it is the middle of the baselines far enough away to be somebody
    /// else. Both are medians, so a strong signal in one or two bins moves
    /// neither.</para>
    /// </remarks>
    private void MeasureBandNoise()
    {
        for (var b = 0; b < _bins; b++)
        {
            var count = 0;

            for (var i = 0; i < _filled; i++)
            {
                if (!_blocked[i])
                {
                    _scratch[count++] = _history[(i * _bins) + b];
                }
            }

            if (count == 0)
            {
                _baseline[b] = double.NaN;
                continue;
            }

            Array.Sort(_scratch, 0, count);
            _baseline[b] = _scratch[count / 2];
        }

        for (var b = 0; b < _bins; b++)
        {
            var count = 0;

            for (var other = 0; other < _bins; other++)
            {
                if (Math.Abs(_binHz[other] - _binHz[b]) >= NoiseSeparationHz
                    && !double.IsNaN(_baseline[other]))
                {
                    _scratch[count++] = _baseline[other];
                }
            }

            if (count == 0)
            {
                _bandNoise[b] = double.NaN;
                continue;
            }

            Array.Sort(_scratch, 0, count);
            _bandNoise[b] = _scratch[count / 2];
        }
    }

    /// <summary>
    /// Everything about one bin: is it keying, and how far over the band is it.
    /// </summary>
    private KeyingCandidate? Examine(
        int bin, double bandDb, out double liftDb, out double presentFraction)
    {
        liftDb = double.NaN;
        presentFraction = 0;

        // Two clusters in the bin's own levels give the threshold, which is what
        // makes this follow a fade instead of being stranded above one.
        if (!Clusters(bin, out var low, out var high, out var midpoint))
        {
            return null;
        }

        liftDb = double.IsNaN(bandDb) ? double.NaN : high - bandDb;

        // **THE HALF-AMPLITUDE CORRECTION IS NOT APPLIED HERE, AND THAT IS A
        // MEASUREMENT RATHER THAN AN OVERSIGHT** (HM-DEC-105). Deciding at half
        // amplitude is right where a mark's *length* is the answer, which is why
        // the settled pass does it and gained a third fewer unresolved
        // characters by it.
        //
        // This survey is not measuring lengths to report them; it is deciding
        // whether a bin holds somebody keying at all, and it does that on the
        // separation between two clusters of mark durations. Moving the decision
        // up the leading edge shortens every mark and tightens that separation,
        // and applied here it cost five noiseless fixtures and, decisively, the
        // real 13:47 capture: the tone is no longer found in an off-air
        // recording where it was found before.
        //
        // A correction that improves one measurement and breaks another is not
        // one correction, and the second half is Tim's to rule on rather than
        // Claude's to force through (§12.1).
        var up = midpoint + HysteresisDb;
        var down = midpoint - HysteresisDb;
        var on = false;
        var above = 0;
        var live = 0;

        for (var i = 0; i < _filled; i++)
        {
            if (_blocked[i])
            {
                _mask[i] = false;
                on = false;
                continue;
            }

            live++;
            var value = _history[(i * _bins) + bin];

            if (on && value < down)
            {
                on = false;
            }
            else if (!on && value > up)
            {
                on = true;
            }

            _mask[i] = on;

            if (on)
            {
                above++;
            }
        }

        presentFraction = live > 0 ? (double)above / live : 0;

        Deglitch();

        var marks = CollectMarks();

        return marks < MinimumMarks ? null : Judge(bin, marks, liftDb, high);
    }

    /// <summary>Two levels in this bin's history, and where they meet.</summary>
    private bool Clusters(int bin, out double low, out double high, out double midpoint)
    {
        low = high = midpoint = 0;

        var count = 0;

        for (var i = 0; i < _filled; i++)
        {
            if (!_blocked[i])
            {
                _scratch[count++] = _history[(i * _bins) + bin];
            }
        }

        if (count < 8)
        {
            return false;
        }

        var split = 0.0;
        for (var i = 0; i < count; i++)
        {
            split += _scratch[i];
        }

        split /= count;

        // Two means, settled. Fifteen passes is far more than this ever needs
        // and bounds the work regardless of what the data does.
        for (var pass = 0; pass < 15; pass++)
        {
            double belowSum = 0, aboveSum = 0;
            int belowCount = 0, aboveCount = 0;

            for (var i = 0; i < count; i++)
            {
                if (_scratch[i] < split)
                {
                    belowSum += _scratch[i];
                    belowCount++;
                }
                else
                {
                    aboveSum += _scratch[i];
                    aboveCount++;
                }
            }

            if (belowCount == 0 || aboveCount == 0)
            {
                return false;
            }

            low = belowSum / belowCount;
            high = aboveSum / aboveCount;

            var next = (low + high) / 2;

            if (Math.Abs(next - split) < 1e-9)
            {
                break;
            }

            split = next;
        }

        midpoint = split;
        return true;
    }

    /// <summary>
    /// Remove anything too short to be an element, by majority vote.
    /// </summary>
    /// <remarks>
    /// A median over the key state. Noise crosses any threshold constantly and
    /// produces runs of one and two measurements, and left alone those become
    /// marks and then letters out of an empty band.
    /// </remarks>
    private void Deglitch()
    {
        if (_deglitch < 3)
        {
            return;
        }

        var half = _deglitch / 2;

        for (var i = 0; i < _filled; i++)
        {
            var down = 0;
            var seen = 0;

            for (var k = -half; k <= half; k++)
            {
                var at = i + k;

                if (at < 0 || at >= _filled)
                {
                    continue;
                }

                seen++;

                if (_mask[at])
                {
                    down++;
                }
            }

            _voted[i] = down * 2 > seen;
        }

        Array.Copy(_voted, _mask, _filled);
    }

    /// <summary>
    /// Every mark that is whole, in milliseconds.
    /// </summary>
    /// <remarks>
    /// **A MARK TOUCHING THE OPERATOR'S OWN TRANSMISSION IS NOT A MARK**
    /// (HM-DEC-095). What is audible between his own elements is a sliver of
    /// somebody else's, cut at both ends by his keying rather than by theirs, and
    /// its length is a fact about him. Measuring it as an element is how half a
    /// minute of a real contact turned into a confident string of E and T.
    /// </remarks>
    private int CollectMarks()
    {
        var count = 0;
        var i = 0;

        while (i < _filled && count < _marks.Length)
        {
            if (!_mask[i])
            {
                i++;
                continue;
            }

            var start = i;

            while (i < _filled && _mask[i])
            {
                i++;
            }

            var truncated = (start > 0 && _blocked[start - 1])
                || (i < _filled && _blocked[i])
                || start == 0
                || i >= _filled;

            if (!truncated)
            {
                _marks[count++] = (i - start) * _hopSeconds * 1000;
            }
        }

        return count;
    }

    /// <summary>
    /// Are these two lengths, or one spread that has been cut in half?
    /// </summary>
    private KeyingCandidate? Judge(int bin, int count, double liftDb, double keyedDb)
    {
        var split = 0.0;

        for (var i = 0; i < count; i++)
        {
            split += _marks[i];
        }

        split /= count;

        double dit = 0, dah = 0;
        int dits = 0, dahs = 0;

        for (var pass = 0; pass < 15; pass++)
        {
            double shortSum = 0, longSum = 0;
            dits = dahs = 0;

            for (var i = 0; i < count; i++)
            {
                if (_marks[i] < split)
                {
                    shortSum += _marks[i];
                    dits++;
                }
                else
                {
                    longSum += _marks[i];
                    dahs++;
                }
            }

            if (dits == 0 || dahs == 0)
            {
                return null;
            }

            dit = shortSum / dits;
            dah = longSum / dahs;

            var next = (dit + dah) / 2;

            if (Math.Abs(next - split) < 1e-9)
            {
                break;
            }

            split = next;
        }

        // Three of each, or the "clusters" are one outlier and everything else.
        if (dits < 3 || dahs < 3 || dit <= 0)
        {
            return null;
        }

        var ratio = dah / dit;

        if (ratio < MinimumRatio || ratio > MaximumRatio
            || dit < ShortestDitMs || dit > LongestDitMs)
        {
            return null;
        }

        // **THE MEASUREMENT EVERYTHING ELSE FAILED AT.** How far the two lengths
        // sit apart, counted in their own scatter. Morse sends two lengths and
        // noise sends a continuum, and this is the only statistic tried that
        // tells them apart on all three recordings.
        var spread = Scatter(count, split, dit, true) + Scatter(count, split, dah, false);
        var separation = spread > 1e-6 ? (dah - dit) / spread : 0;

        return separation < MinimumSeparation
            ? null
            : new KeyingCandidate(
                _binHz[bin], dit, dah, ratio, separation, liftDb, count, keyedDb);
    }

    /// <summary>How far one cluster's members sit from its own middle.</summary>
    private double Scatter(int count, double split, double mean, bool below)
    {
        var sum = 0.0;
        var members = 0;

        for (var i = 0; i < count; i++)
        {
            if (_marks[i] < split != below)
            {
                continue;
            }

            var delta = _marks[i] - mean;
            sum += delta * delta;
            members++;
        }

        return members == 0 ? 0 : Math.Sqrt(sum / members);
    }
}
