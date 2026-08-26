namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// What every admission test said about one bin on one survey pass.
/// </summary>
/// <param name="ToneHz">The bin.</param>
/// <param name="Refused">
/// Which test refused it, or `null` where it was admitted.
/// </param>
/// <param name="Marks">How many whole marks the bin's gate produced.</param>
/// <param name="Dits">How many of them fell in the short cluster.</param>
/// <param name="Dahs">How many fell in the long one.</param>
/// <param name="DitMilliseconds">The short cluster's mean.</param>
/// <param name="DahMilliseconds">The long cluster's mean.</param>
/// <param name="Ratio">The two clusters' quotient.</param>
/// <param name="Separation">How far apart they sit in their own scatter.</param>
/// <param name="LiftDb">How far the bin stands over the band.</param>
/// <param name="KeyedDb">The level the marks were measured at.</param>
/// <param name="SpreadDb">
/// How far the bin's own two level-means sit apart, which is the quantity the
/// gate's threshold is currently placed halfway between.
/// </param>
/// <param name="Duty">
/// What share of this bin's history the gate held open, from nought to one.
/// </param>
/// <remarks>
/// <para>**A VERDICT IS NOT A MEASUREMENT, AND FOR FOUR DAYS ONLY THE VERDICT
/// EXISTED.** The survey applies seven tests to each bin and reported one
/// answer: admitted, or nothing. So a station the operator could hear was
/// refused and no instrument in this tree could say which test refused it or by
/// how much it missed. Three consecutive units built a mechanism downstream of
/// that decision, measured it dead, and correctly shipped nothing — each of them
/// reasoning about a candidate that was never nominated.</para>
/// <para>**IT RECORDS EVERY BIN, INCLUDING THE ONES THAT HOLD NOTHING.** A
/// refusal is only legible beside the refusals of bins nobody claims hold a
/// station, which is the same reason the empty captures are controls rather
/// than a formality (§0.0.1).</para>
/// <para>Nothing reads this in the application. It is switched on by handing the
/// survey somewhere to put it, and off by not doing so, so the cost on the audio
/// thread is one null check per bin (§8).</para>
/// </remarks>
public readonly record struct BinReading(
    double ToneHz,
    string? Refused,
    int Marks,
    int Dits,
    int Dahs,
    double DitMilliseconds,
    double DahMilliseconds,
    double Ratio,
    double Separation,
    double LiftDb,
    double KeyedDb,
    double SpreadDb = double.NaN,
    double Duty = double.NaN)
{
    /// <summary>
    /// True where the gate produced no marks because it never opened.
    /// </summary>
    /// <remarks>
    /// **A GATE THAT NEVER CLOSES ALSO PRODUCES NO MARKS**, because a run
    /// touching either end of the history is truncated rather than counted, and
    /// a run that spans the whole history touches both. So a count of nought
    /// marks says nothing on its own about whether a bin was quiet — measured on
    /// 2026-08-26, a candidate threshold placed three decibels over the band
    /// floor drove `cw-2026-08-25-012823` to 95% of bins producing no marks
    /// while the gate at its station's own pitch was open essentially all the
    /// time. Reading that as silence would have been exactly backwards.
    /// </remarks>
    /// <remarks>
    /// **A BIN WITH NO GATE AT ALL COUNTS AS SHUT**, and that is the strongest
    /// form of it: where the two-level test or the spread test refuses, no gate
    /// is ever built, so there is no duty to read and the field is `NaN`. Reading
    /// `NaN` as "not shut" would score the one mechanism that genuinely stops a
    /// bin at nought.
    /// </remarks>
    public bool Shut
        => Marks == 0 && (double.IsNaN(Duty) || Duty < 0.05);

    /// <summary>
    /// True where the gate produced no marks because it never closed.
    /// </summary>
    public bool StuckOpen => Marks == 0 && Duty > 0.95;

    /// <summary>
    /// True where the gate opened and closed but no run survived being counted.
    /// </summary>
    /// <remarks>
    /// A run touching either end of the history is truncated rather than counted,
    /// so a bin can toggle and still yield nothing. This is the case that made
    /// `cw-2026-08-17-013347` look like a quiet gate when it is not one.
    /// </remarks>
    public bool Truncated => Marks == 0 && !Shut && !StuckOpen;

    /// <summary>True where every test passed.</summary>
    public bool Admitted => Refused is null;

    /// <summary>
    /// How far the refusing test missed by, in that test's own units.
    /// </summary>
    /// <remarks>
    /// **IN THE TEST'S OWN UNITS AND NOT NORMALISED**, because the question a
    /// reader asks is "how much would this bound have to move", and a figure
    /// scaled to make two tests comparable answers a question nobody asked.
    /// `NaN` where the bin was admitted, or where the refusing test has no
    /// distance — a bin with no two clusters is not a near miss.
    /// </remarks>
    public double MissedBy => Refused switch
    {
        "marks" => CwToneSurvey.MinimumMarks - Marks,
        "dits" => 3 - Dits,
        "dahs" => 3 - Dahs,
        "ratio" => Ratio < CwToneSurvey.MinimumRatio
            ? CwToneSurvey.MinimumRatio - Ratio
            : Ratio - CwToneSurvey.MaximumRatio,
        "dit" => DitMilliseconds < CwToneSurvey.ShortestDitMs
            ? CwToneSurvey.ShortestDitMs - DitMilliseconds
            : DitMilliseconds - CwToneSurvey.LongestDitMs,
        "separation" => CwToneSurvey.MinimumSeparation - Separation,
        _ => double.NaN,
    };
}

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
    /// <remarks>
    /// <para>**AND THE ROOM ON BOTH SIDES IS GONE ON THE WIDER CORPUS.** Measured
    /// on 2026-08-26 with the per-bin instrument, at each capture's own claimed
    /// pitch, against the two recordings HM-DEC-120 protects:</para>
    /// <para>The four stations Hamlet cannot read reach a best separation of
    /// 3.82, 3.03, 5.87 and 7.02 at their own pitches, with medians of 1.70 to
    /// 2.32. **The two recordings holding nothing reach 3.58 and 4.92 somewhere
    /// in the band** — higher than three of those four stations. Swept as a
    /// bound: at 4.0 it takes two of the four and `cw-2026-08-20-014935` already
    /// leaks a bin; at 3.0 it takes all four and the two controls leak nine and
    /// eight; at 2.0, a hundred and sixteen and a hundred and eleven. **No bound
    /// on this axis admits the stations and refuses the noise.**</para>
    /// <para>**THE STATISTIC IS NOT THE FAULT — WHAT IT IS FED IS.** The gate's
    /// threshold comes from each bin's own two levels, so a bin holding only
    /// noise has its noise cut in half and yields a stream of structureless
    /// marks. Counted: on `cw-2026-08-17-013347`, which reads `VA3VRR`, **926 of
    /// 1,425 bin readings produce no marks at all** and the gate stays shut where
    /// nothing is keyed. On every other capture measured — the failing stations
    /// and both silence controls alike — **not one bin produces zero marks**, and
    /// the median where the gate opens is nineteen or twenty. Separation then
    /// correctly reports a continuum, about 1.7, for the noise and for the
    /// station, because on those captures it is looking at the same thing in
    /// both.</para>
    /// <para>**SO THIS NUMBER MUST NOT BE MOVED**, and moving it was measured
    /// rather than argued about. What needs fixing is the threshold the marks are
    /// cut at, which is a ruling and not a session's change.</para>
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

    /// <summary>
    /// Where to write what every admission test said, or null to measure nothing.
    /// </summary>
    /// <remarks>
    /// **OFF UNLESS SOMEBODY IS LOOKING** (§8). Nothing in the application sets
    /// it; a test does, reads the list, and drops it. On the audio thread it is
    /// one null check per bin per pass.
    /// </remarks>
    public List<BinReading>? Readings { get; set; }

    /// <summary>
    /// Place the gate this far above the band's own noise floor, instead of
    /// halfway between the bin's own two levels. Null keeps the old placement.
    /// </summary>
    /// <remarks>
    /// **CANDIDATE A OF TIM'S RULING OF 2026-08-26.** A bin holding only noise
    /// has no two levels to be halfway between, so the old placement splits the
    /// noise and manufactures marks. The band floor is a fact about the band
    /// rather than about the bin, so a quiet bin cannot argue itself up to it.
    /// Falls back to the old placement where the band floor could not be worked
    /// out, because a threshold from an unread number is worse than one from the
    /// wrong number (§0.0).
    /// </remarks>
    public double? GateAboveBandFloorDb { get; set; }

    /// <summary>
    /// A bin whose two levels sit closer than this produces no marks at all.
    /// Null admits any spread, which is what the old gate did.
    /// </summary>
    /// <remarks>
    /// **CANDIDATE B OF THE SAME RULING.** Two levels fitted to a continuum are
    /// not two things, and the fit always succeeds — measured across eleven
    /// thousand bin readings on 2026-08-26, the two-level test refused nothing at
    /// all, on any capture.
    /// </remarks>
    /// <remarks>
    /// <para>**MEASURED 2026-08-26 AND NOT SHIPPED, THOUGH IT CAME CLOSE.** Swept
    /// at 12, 15, 20 and 25 decibels. The mechanism works as intended: it shuts
    /// bins outright, with **no stuck-open bins at any setting**, which is the
    /// half candidate A could not deliver. At twelve decibels the two silence
    /// controls go 49.6% and 56.9% genuinely shut, emitting nothing, and
    /// **eleven of the twelve anchors survive**.</para>
    /// <para>**IT FAILS ON THE TWELFTH ANCHOR AND ON THE STATIONS.**
    /// `cw-2026-08-24-012403` loses `DE KD0UN KD0UN K` at every setting, and the
    /// acceptance admits no shortfall. At fifteen it also costs
    /// `cw-2026-08-18-004507`; at twenty it costs ten of twelve. And it shuts the
    /// stations it was built to rescue — `cw-2026-08-22-014113` goes 90.5% shut
    /// at twelve and 99.7% at fifteen, with no marks at all at its own
    /// pitch.</para>
    /// <para>**THE COMMON FINDING IS WORTH MORE THAN EITHER SWEEP.** Today's gate
    /// already produces marks at all four stations' own pitches — 15, 17, 10 and
    /// 19. Neither candidate improves on that at any setting; both reduce marks
    /// everywhere rather than selectively, on stations and noise alike. The gate
    /// is not failing to find the stations. It is finding everything.</para>
    /// </remarks>
    public double? MinimumLevelSpreadDb { get; set; }

    private double _spreadDb = double.NaN;

    private double _duty = double.NaN;

    private void Record(
        int bin,
        string? refused,
        int marks = 0,
        int dits = 0,
        int dahs = 0,
        double dit = double.NaN,
        double dah = double.NaN,
        double ratio = double.NaN,
        double separation = double.NaN,
        double liftDb = double.NaN,
        double keyedDb = double.NaN)
        => Readings?.Add(new BinReading(
            _binHz[bin], refused, marks, dits, dahs,
            dit, dah, ratio, separation, liftDb, keyedDb, _spreadDb, _duty));

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
        _spreadDb = double.NaN;
        _duty = double.NaN;

        if (!Clusters(bin, out var low, out var high, out var midpoint))
        {
            Record(bin, "clusters");

            return null;
        }

        _spreadDb = high - low;

        // **CANDIDATE B: TWO LEVELS THAT ARE NOT TWO THINGS ARE NOT A GATE.**
        if (MinimumLevelSpreadDb is { } least && high - low < least)
        {
            Record(bin, "spread", liftDb: double.IsNaN(bandDb) ? double.NaN : high - bandDb, keyedDb: high);

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
        // **CANDIDATE A: THE THRESHOLD COMES FROM THE BAND, NOT FROM THE BIN.**
        // Where the band floor could not be worked out the old placement stands,
        // because a threshold derived from an unread number asserts more than a
        // threshold derived from the wrong one (§0.0).
        if (GateAboveBandFloorDb is { } over && !double.IsNaN(bandDb))
        {
            midpoint = bandDb + over;
        }

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
        _duty = presentFraction;

        Deglitch();

        var marks = CollectMarks();

        if (marks < MinimumMarks)
        {
            Record(bin, "marks", marks, liftDb: liftDb, keyedDb: high);

            return null;
        }

        return Judge(bin, marks, liftDb, high);
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
                Record(
                    bin, dits == 0 ? "dits" : "dahs", count, dits, dahs,
                    liftDb: liftDb, keyedDb: keyedDb);

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
            Record(
                bin, dits < 3 ? "dits" : dahs < 3 ? "dahs" : "dit", count,
                dits, dahs, dit, dah, liftDb: liftDb, keyedDb: keyedDb);

            return null;
        }

        var ratio = dah / dit;

        if (ratio < MinimumRatio || ratio > MaximumRatio
            || dit < ShortestDitMs || dit > LongestDitMs)
        {
            // **THE RATIO IS NAMED FIRST WHERE BOTH FAIL**, because it is the
            // test the band was written for and the dit bound is a sanity check
            // around it. A reader wants the reason, not the first line that
            // happened to be false.
            //
            // **AND THE SEPARATION IS MEASURED ANYWAY WHEN SOMEBODY IS
            // WATCHING.** A refusal here normally returns before the scatter is
            // computed, which left the one statistic that decides keying from
            // noise absent from exactly the rows a reader most wants to compare.
            // It costs nothing in production, where `Readings` is null.
            Record(
                bin,
                ratio < MinimumRatio || ratio > MaximumRatio ? "ratio" : "dit",
                count, dits, dahs, dit, dah, ratio,
                Readings is null ? double.NaN : Spread(count, split, dit, dah),
                liftDb, keyedDb);

            return null;
        }

        // **THE MEASUREMENT EVERYTHING ELSE FAILED AT.** How far the two lengths
        // sit apart, counted in their own scatter. Morse sends two lengths and
        // noise sends a continuum, and this is the only statistic tried that
        // tells them apart on all three recordings.
        var separation = Spread(count, split, dit, dah);

        Record(
            bin, separation < MinimumSeparation ? "separation" : null,
            count, dits, dahs, dit, dah, ratio, separation, liftDb, keyedDb);

        return separation < MinimumSeparation
            ? null
            : new KeyingCandidate(
                _binHz[bin], dit, dah, ratio, separation, liftDb, count, keyedDb);
    }

    /// <summary>
    /// How far the two mark lengths sit apart, counted in their own scatter.
    /// </summary>
    /// <param name="count">How many marks were collected.</param>
    /// <param name="split">Where the two clusters were cut apart.</param>
    /// <param name="dit">The short cluster's mean.</param>
    /// <param name="dah">The long cluster's mean.</param>
    /// <returns>The separation, or nought where there is no scatter to divide by.</returns>
    private double Spread(int count, double split, double dit, double dah)
    {
        var spread = Scatter(count, split, dit, true)
            + Scatter(count, split, dah, false);

        return spread > 1e-6 ? (dah - dit) / spread : 0;
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
