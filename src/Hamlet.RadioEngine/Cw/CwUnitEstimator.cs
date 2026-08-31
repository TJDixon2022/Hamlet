namespace Hamlet.RadioEngine.Cw;

/// <summary>What a stretch of envelope says the sender's timing is.</summary>
/// <param name="UnitMilliseconds">
/// The dit length, or nought when the envelope did not hold enough keying to
/// measure one.
/// </param>
/// <param name="DitMarkMilliseconds">The middle of the short mark cluster.</param>
/// <param name="ElementGapMilliseconds">The middle of the short gap cluster.</param>
/// <param name="Marks">How many marks the measurement rests on.</param>
public readonly record struct CwUnitReading(
    double UnitMilliseconds,
    double DitMarkMilliseconds,
    double ElementGapMilliseconds,
    int Marks)
{
    /// <summary>Nothing measured.</summary>
    public static CwUnitReading None { get; } = new(0, 0, 0, 0);

    /// <summary>True when there was enough keying to say anything.</summary>
    public bool IsReady => UnitMilliseconds > 0;

    /// <summary>The sending speed the unit implies.</summary>
    public double WordsPerMinute => IsReady ? 1200.0 / UnitMilliseconds : 0;
}

/// <summary>
/// Measures a sender's dit from the audio rather than searching for it.
/// </summary>
/// <remarks>
/// <para>**THE BIAS CANCELS BETWEEN A MARK AND THE GAP BESIDE IT, AND THAT IS THE
/// WHOLE IDEA.** Any level a decoder cuts the envelope at catches the rising and
/// falling skirt of every mark, so a mark reads long by some amount and the gap
/// next to it reads short by the same amount. Measured on a machine keyer sending
/// eighteen words a minute: dit marks at 82 milliseconds, which alone says 14.6
/// words a minute, and element gaps at 54, which alone says 22. **The average is
/// 68 and the bias has gone.**
/// </para>
/// <para>**BOTH CLUSTERS ARE TAKEN ON THE LOGARITHM**, because a dah is three
/// times a dit rather than three units longer than one, and a mean taken on raw
/// milliseconds is pulled by whichever end is longer.
/// </para>
/// <para>**THE ONE NUMBER HERE NOT MEASURED FROM THE AUDIO IS THE HYSTERESIS
/// DEPTH.** Everything else is a percentile or a centroid of what arrived.
/// </para>
/// </remarks>
public static class CwUnitEstimator
{
    /// <summary>
    /// How far the envelope must rise above the cut to count as key-down, and
    /// fall below it to count as key-up, in decibels.
    /// </summary>
    /// <remarks>
    /// <para>**A SINGLE LEVEL IS CROSSED AND RE-CROSSED ON EVERY EDGE AND IN
    /// EVERY SHALLOW FADE INSIDE A MARK**, and each crossing becomes another
    /// element for everything downstream to compute statistics on. Two levels
    /// with a gap between them cost nothing, need no time constant, and delay
    /// both edges by about the same amount, so the mark lengths survive.
    /// </para>
    /// <para>**SIX DECIBELS, AND THE OPTIMUM IS BROAD.** A minimum run length,
    /// the usual repair, is a millisecond constant that has to be retuned for
    /// every speed. This is not. **It is the one constant in the estimator not
    /// derived from the audio**, and the plateau around it is measured rather
    /// than asserted.
    /// </para>
    /// </remarks>
    public const double HysteresisDb = 6.0;

    /// <summary>The shortest run, in hops, that is taken as an element at all.</summary>
    private const int ShortestRunHops = 2;

    /// <summary>Measure the sender's timing from an envelope.</summary>
    /// <param name="envelope">Envelope magnitudes, one every hop.</param>
    /// <param name="hopMilliseconds">How long one hop lasts.</param>
    /// <param name="hysteresisDb">How deep the trigger is, for measurement.</param>
    /// <returns>What the envelope says, or nothing.</returns>
    public static CwUnitReading Measure(
        IReadOnlyList<double> envelope,
        double hopMilliseconds,
        double hysteresisDb = HysteresisDb)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        if (envelope.Count < 16)
        {
            return CwUnitReading.None;
        }

        var (marks, gaps) = Elements(envelope, hopMilliseconds, hysteresisDb);

        if (marks.Count < 8 || gaps.Count < 8)
        {
            return CwUnitReading.None;
        }

        var shortMark = ShortClusterMedian(marks);
        var shortGap = ShortClusterMedian(gaps);

        if (shortMark <= 0 || shortGap <= 0)
        {
            return CwUnitReading.None;
        }

        return new CwUnitReading(
            (shortMark + shortGap) / 2, shortMark, shortGap, marks.Count);
    }

    /// <summary>
    /// Where this sender's three gap lengths actually sit, in milliseconds.
    /// </summary>
    /// <param name="ElementMilliseconds">The gap inside a character.</param>
    /// <param name="CharacterMilliseconds">The gap between two characters.</param>
    /// <param name="WordMilliseconds">The gap between two words.</param>
    /// <param name="Separated">
    /// True when the three heaps are far enough apart to be three heaps.
    /// </param>
    /// <param name="CharacterBoundaryClipped">
    /// True when the unit's sanity clip, rather than the gaps, decided where an
    /// element gap stops being one.
    /// </param>
    /// <param name="WordBoundaryClipped">
    /// True when the clip decided the boundary between a character gap and a
    /// word gap.
    /// </param>
    public readonly record struct CwGapLengths(
        double ElementMilliseconds,
        double CharacterMilliseconds,
        double WordMilliseconds,
        bool Separated,
        bool CharacterBoundaryClipped = false,
        bool WordBoundaryClipped = false)
    {
        /// <summary>The length that divides an element gap from a character gap.</summary>
        /// <remarks>
        /// The geometric mean of the two, which is where the decoder's own ratio
        /// penalty makes the two readings cost the same, so it is the boundary
        /// whether or not anybody computes it.
        /// </remarks>
        public double CharacterBoundaryMilliseconds
            => Math.Sqrt(ElementMilliseconds * CharacterMilliseconds);

        /// <summary>The length that divides a character gap from a word gap.</summary>
        public double WordBoundaryMilliseconds
            => Math.Sqrt(CharacterMilliseconds * WordMilliseconds);
    }

    /// <summary>
    /// The three gap lengths this sender is actually using, clustered from the
    /// gaps rather than derived from the unit.
    /// </summary>
    /// <param name="envelope">Envelope magnitudes, one every hop.</param>
    /// <param name="hopMilliseconds">How long one hop lasts.</param>
    /// <param name="unitMilliseconds">The dit, used only as a sanity clip.</param>
    /// <param name="hysteresisDb">How deep the trigger is.</param>
    /// <returns>The three lengths, and whether they separated.</returns>
    /// <remarks>
    /// <para>**THE GAP DISTRIBUTION DOES NOT NEED THE SPEED.** A boundary placed at
    /// a multiple of the estimated unit couples two independent failures: get the
    /// unit wrong and the letter spacing dies with it. Measured on
    /// `cw-2026-08-18-004507`, whose unit came out fifty milliseconds on a sender
    /// working near sixty-seven, a boundary at twice the unit lands at a hundred,
    /// **inside that sender's own element-gap cluster**, and every gap becomes a
    /// letter break. The gaps themselves have two empty regions in them and a
    /// boundary in dead space cannot misclassify anything.</para>
    /// <para>**CLUSTERED ON THE LOGARITHMS**, because a word gap is seven times a
    /// dit rather than six units longer than one.</para>
    /// <para>**THE UNIT SURVIVES AS A CLIP AND NOT AS THE ESTIMATE.** Word gaps
    /// are rare enough in thirty seconds that their cluster cannot be trusted on
    /// its own, so the boundary between a character gap and a word gap is held
    /// inside three and a half to six and a half dits, and the boundary below it
    /// inside one and three tenths to two and six tenths. **Those two ranges are
    /// constants and they are the only ones here.**</para>
    /// </remarks>
    public static CwGapLengths MeasureGaps(
        IReadOnlyList<double> envelope,
        double hopMilliseconds,
        double unitMilliseconds,
        double hysteresisDb = HysteresisDb)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var textbook = new CwGapLengths(
            unitMilliseconds, unitMilliseconds * 3, unitMilliseconds * 7, false);

        if (unitMilliseconds <= 0)
        {
            return textbook;
        }

        var (_, gaps) = Elements(envelope, hopMilliseconds, hysteresisDb);

        if (gaps.Count < 12)
        {
            return textbook;
        }

        var centroids = ThreeMeansOnLogs(gaps);

        // Three heaps or two: a sender who never leaves a word gap has no third
        // cluster to find, and inventing one is the guess HM-DEC-142 forbids.
        var separated = centroids[1] / centroids[0] >= 1.5
            && centroids[2] / centroids[1] >= 1.5;

        if (!separated)
        {
            return textbook;
        }

        // **AND THE BOUNDARY HAS TO LAND WHERE NOTHING IS.** Three centroids can
        // always be found; what makes them worth using is a trough between them,
        // because a boundary in the middle of a heap misclassifies whatever is
        // standing there. So each boundary is checked for being emptier than the
        // two clusters it divides, which needs no threshold: it is a comparison
        // of this sender's own counts.
        if (!IsTrough(gaps, centroids[0], centroids[1])
            || !IsTrough(gaps, centroids[1], centroids[2]))
        {
            return textbook;
        }

        var element = centroids[0];
        var character = centroids[1];
        var word = centroids[2];

        // The clip, applied to the boundary and carried back into the centroid
        // that sets it, so the boundary is the thing held inside the range.
        var wantedCharacter = Math.Sqrt(element * character);
        var characterBoundary = Math.Clamp(
            wantedCharacter, 1.3 * unitMilliseconds, 2.6 * unitMilliseconds);

        character = characterBoundary * characterBoundary / element;

        var wantedWord = Math.Sqrt(character * word);
        var wordBoundary = Math.Clamp(
            wantedWord, 3.5 * unitMilliseconds, 6.5 * unitMilliseconds);

        word = wordBoundary * wordBoundary / character;

        return new CwGapLengths(
            element,
            character,
            word,
            true,
            Math.Abs(characterBoundary - wantedCharacter) > 1e-6,
            Math.Abs(wordBoundary - wantedWord) > 1e-6);
    }

    /// <summary>
    /// True when the geometric mean of two centroids is emptier than either of
    /// them.
    /// </summary>
    /// <remarks>
    /// **THE EVIDENCE THAT A BOUNDARY IS WORTH HAVING IS THAT NOTHING IS
    /// STANDING ON IT.** Counted in equal windows on the logarithm, so the
    /// comparison is between equal ratios rather than equal milliseconds, and
    /// nothing has to be chosen.
    /// </remarks>
    private static bool IsTrough(
        IReadOnlyList<double> values, double low, double high)
    {
        var boundary = Math.Sqrt(low * high);
        var width = Math.Pow(high / low, 0.15);

        int Near(double centre)
            => values.Count(v => v >= centre / width && v <= centre * width);

        var atBoundary = Near(boundary);

        return atBoundary < Near(low) && atBoundary < Near(high);
    }

    /// <summary>Three clusters on the logarithm of the durations, shortest first.</summary>
    private static double[] ThreeMeansOnLogs(IReadOnlyList<double> values)
    {
        var logs = values.Select(v => Math.Log(Math.Max(v, 1e-6))).ToArray();

        Array.Sort(logs);

        // **SEEDED ACROSS THE RANGE, NOT ACROSS THE COUNT.** Most of a sender's
        // gaps are inside characters, so seeding at the sixth, the half and the
        // five-sixths of the sorted list puts two of the three centres inside the
        // element heap and the other two heaps are never found. Spreading the
        // seeds evenly across the span in the log domain gives each heap
        // somewhere to attract from, which on textbook spacing recovers one,
        // three and seven.
        var span = logs[^1] - logs[0];
        var centres = new[]
        {
            logs[0] + (span / 6),
            logs[0] + (span / 2),
            logs[0] + (span * 5 / 6),
        };

        for (var pass = 0; pass < 30; pass++)
        {
            var sums = new double[3];
            var counts = new int[3];

            foreach (var value in logs)
            {
                var best = 0;

                for (var c = 1; c < 3; c++)
                {
                    if (Math.Abs(value - centres[c]) < Math.Abs(value - centres[best]))
                    {
                        best = c;
                    }
                }

                sums[best] += value;
                counts[best]++;
            }

            for (var c = 0; c < 3; c++)
            {
                if (counts[c] > 0)
                {
                    centres[c] = sums[c] / counts[c];
                }
            }
        }

        Array.Sort(centres);

        return centres.Select(Math.Exp).ToArray();
    }

    /// <summary>Every mark and every gap the trigger produces.</summary>
    /// <param name="envelope">Envelope magnitudes.</param>
    /// <param name="hopMilliseconds">How long one hop lasts.</param>
    /// <param name="hysteresisDb">How deep the trigger is.</param>
    /// <returns>Mark lengths and gap lengths, in milliseconds.</returns>
    public static (IReadOnlyList<double> Marks, IReadOnlyList<double> Gaps) Elements(
        IReadOnlyList<double> envelope,
        double hopMilliseconds,
        double hysteresisDb = HysteresisDb)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var db = new double[envelope.Count];

        for (var i = 0; i < envelope.Count; i++)
        {
            db[i] = 20 * Math.Log10(Math.Max(envelope[i], 1e-12));
        }

        return Runs(db, Cut(db), hysteresisDb, hopMilliseconds);
    }

    /// <summary>
    /// How far below the loudest signal the key-down threshold sits, in decibels,
    /// or NaN to keep the Otsu split.
    /// </summary>
    /// <remarks>
    /// <para>**REFERENCED TO THE LOUDEST SIGNAL RATHER THAN TO A SPLIT OF
    /// EVERYTHING** (work instruction 054, task 2). Otsu asks where the two
    /// classes divide, and on a passband holding two stations the second station
    /// is one of the classes — so the cut lands in the middle of it and its fades
    /// and peaks are counted as elements of the first. A setback from the high
    /// percentile has no opinion about anything but the loudest thing present.</para>
    /// <para>**MEASURED AND REFUSED ON THREE GROUNDS, AND NaN SHIPS** (work
    /// instruction 054, task 2). The reasoning is right about what it does to dah
    /// scatter and the decoder reads worse anyway.</para>
    /// <list type="number">
    /// <item>**A setback under six decibels cannot work at all, and that is
    /// arithmetic rather than a corpus accident.** The Schmitt trigger opens at
    /// the cut plus `HysteresisDb`, which is 6 dB, so a setback of 5 puts the
    /// opening threshold one decibel *above* the envelope's own ninety-eighth
    /// percentile and nothing is ever key-down. **Setbacks of 3, 4 and 5 produce
    /// fewer than eight marks on every capture in the corpus**, so the order's
    /// recommended peak − 5 dB is unreachable while the hysteresis is ±6.</item>
    /// <item>**The usable range is not monotonic.** On `cw-2026-08-17-013347` dah
    /// CV runs 0.444 at Otsu, **0.113 at −6**, 0.413 at −8, 0.531 at −10 and
    /// 0.486 at −12. It dips and comes back, and §12.5's standard is not to adopt
    /// off a curve like that.</item>
    /// <item>**The one candidate that helps costs precision.** At −6 dB the corpus
    /// reads **0.840 against a floor of 0.888**, yield 0.630 against 0.745, and
    /// substitutions 33 against 17.</item>
    /// </list>
    /// <para>**THE FINDING IS REAL AND IS THE REASON THIS IS KEPT.** At −6 dB the
    /// worst captures improve exactly as the order predicted — `013347` 0.444 to
    /// 0.113, `134712` 0.647 to 0.275, worst-in-corpus 0.647 to 0.275 — while the
    /// captures that already read cleanly get worse, `004507` going 0.015 to
    /// 0.113. **Referencing to the peak buys the hard captures and sells the easy
    /// ones**, which is the trade this project has been making by accident and
    /// must not make on purpose.</para>
    /// </remarks>
    public static double PeakSetbackDb { get; set; } = double.NaN;

    /// <summary>The key-down threshold for this envelope, in decibels.</summary>
    /// <param name="db">The envelope in decibels.</param>
    /// <returns>The cut.</returns>
    /// <remarks>
    /// **THE HYSTERESIS IS APPLIED AROUND WHATEVER THIS RETURNS** and is not
    /// changed by the reference. Only where the trigger sits moves.
    /// </remarks>
    public static double Cut(double[] db)
    {
        ArgumentNullException.ThrowIfNull(db);

        if (double.IsNaN(PeakSetbackDb) || db.Length == 0)
        {
            return Otsu(db);
        }

        var sorted = (double[])db.Clone();

        Array.Sort(sorted);

        // The ninety-eighth percentile rather than the maximum: a click is louder
        // than the station and is not the station.
        var at = 0.98 * (sorted.Length - 1);
        var low = (int)Math.Floor(at);
        var high = Math.Min(low + 1, sorted.Length - 1);
        var peak = sorted[low] + ((sorted[high] - sorted[low]) * (at - low));

        return peak - PeakSetbackDb;
    }

    /// <summary>How far above the noise floor the key-down threshold sits.</summary>
    /// <remarks>
    /// **BUILT, SWEPT, AND NOT ADOPTED** (work instruction 051, task 3). Kept
    /// with its numbers so the next session finds the measurement rather than
    /// spending an evening rebuilding it; see <see cref="Threshold"/> for the
    /// three independent reasons it was refused.
    /// </remarks>
    public static double Fraction { get; set; } = 0.5;

    /// <summary>
    /// The smallest swing between noise and signal that counts as a station.
    /// </summary>
    /// <remarks>
    /// **THIS IS THE "NO STATION HERE" TEST AND IT IS LOAD-BEARING NOW.** Otsu
    /// always returned a number, so an empty band got a threshold through the
    /// middle of its own hiss and every bin read 45 to 69 per cent duty. A
    /// percentile threshold has to say when there is nothing to put a threshold
    /// between.
    /// </remarks>
    public static double MinimumSwingDb { get; set; } = 6.0;

    /// <summary>
    /// The key-down threshold, from the envelope's own percentiles.
    /// </summary>
    /// <param name="db">The envelope in decibels.</param>
    /// <returns>The threshold in decibels.</returns>
    /// <remarks>
    /// <para>**OTSU ASSUMES TWO CLASSES OF COMPARABLE MASS AND A BAND MOSTLY
    /// SILENT HAS ONE** (work instruction 051, task 3). On
    /// `cw-2026-08-30-001650` the station occupies about twelve per cent of the
    /// file, so Otsu split the noise distribution down the middle and returned a
    /// threshold inside the hiss. Measured over the last fifteen seconds, every
    /// bin from 450 to 775 Hz then read 45 to 69 per cent duty and nothing stood
    /// out; with a threshold above the noise exactly one fifty-hertz band lit up
    /// and the rest of the passband went to zero. **Same audio, opposite
    /// verdicts.**</para>
    /// <para>**THE PERCENTILES ARE CHOSEN TO BE ROBUST TO THE THING THAT BROKE
    /// OTSU.** The twentieth is noise even when a signal is busy; the
    /// ninety-eighth is signal even when a click is louder than it. Neither
    /// depends on the two having comparable mass, which is the assumption that
    /// failed.</para>
    /// <para>**AND THE HYSTERESIS IS UNTOUCHED.** The ±6 dB Schmitt trigger is
    /// measured and it works; only where the trigger sits has changed.</para>
    /// <para>**IT IS NOT WIRED IN, AND THREE INDEPENDENT MEASUREMENTS REFUSED
    /// IT.** The reasoning above is sound about the failing case and the corpus
    /// says it is wrong about every other one.</para>
    /// <list type="number">
    /// <item>**The fraction sweep is not monotonic**, and the order forbids
    /// adopting off a curve that is not: precision runs 0.601, 0.728, 0.751,
    /// 0.703, 0.770, 0.787, 0.742, 0.738 across fractions 0.20 to 0.60. It goes
    /// up, down, up, down.</item>
    /// <item>**Every candidate is far below the floor.** The best is 0.787 at a
    /// fraction of 0.50, against 0.888 with Otsu and a hard floor of 0.858.</item>
    /// <item>**It fails its own acceptance criterion**, which was that on
    /// known-good captures the threshold lands within a decibel or two of where
    /// it lands today. Measured, it lands **0.6 to 4.9 dB higher**, median about
    /// 3.0, and higher on every single capture — which is why yield collapsed.
    /// On `cw-2026-08-17-013347` the twentieth percentile falls at −110 dB,
    /// because that recording is mostly digital silence and a percentile of
    /// silence is not a noise floor.</item>
    /// </list>
    /// <para>**SO OTSU IS RIGHT EXACTLY WHERE THE ORDER PREDICTED IT WOULD BE** —
    /// where signal and noise have comparable mass — and the fault it has is real
    /// and is confined to the case where they do not. **What could not be done
    /// this unit is verify a repair**, because the two captures the fault was
    /// measured on are not in this repository.</para>
    /// </remarks>
    public static double Threshold(double[] db)
    {
        ArgumentNullException.ThrowIfNull(db);

        if (db.Length == 0)
        {
            return 0;
        }

        var sorted = (double[])db.Clone();

        Array.Sort(sorted);

        var floor = Percentile(sorted, 20);
        var peak = Percentile(sorted, 98);

        if (peak - floor < MinimumSwingDb)
        {
            // **NOTHING HERE.** Returning a threshold above everything means no
            // hop is ever key-down, which is the honest answer for a band with
            // no station in it — and the answer Otsu could not give (§0.0).
            return sorted[^1] + 1;
        }

        return floor + (Fraction * (peak - floor));
    }

    /// <summary>One percentile of an already-sorted array.</summary>
    private static double Percentile(double[] sorted, double share)
    {
        var at = (share / 100.0) * (sorted.Length - 1);
        var low = (int)Math.Floor(at);
        var high = Math.Min(low + 1, sorted.Length - 1);

        return sorted[low] + ((sorted[high] - sorted[low]) * (at - low));
    }

    /// <summary>
    /// The level that splits the envelope into two classes with the least
    /// variance inside them.
    /// </summary>
    /// <remarks>
    /// <para>Otsu's method over a histogram of the envelope in decibels. It is a
    /// measurement of this recording rather than a level anybody chose, which is
    /// what lets the trigger depth be the only constant here.</para>
    /// <para>**PUBLIC SO THE TWO THRESHOLDS CAN BE COMPARED SIDE BY SIDE** (work
    /// instruction 051, task 3), which is the acceptance criterion that decided
    /// whether the percentile threshold could be adopted.</para>
    /// </remarks>
    /// <param name="db">The envelope in decibels.</param>
    /// <returns>The split, in decibels.</returns>
    public static double Otsu(double[] db)
    {
        var low = double.MaxValue;
        var high = double.MinValue;

        foreach (var value in db)
        {
            low = Math.Min(low, value);
            high = Math.Max(high, value);
        }

        if (high - low < 1e-6)
        {
            return low;
        }

        const int bins = 256;
        var counts = new int[bins];
        var width = (high - low) / bins;

        foreach (var value in db)
        {
            var bin = (int)((value - low) / width);

            counts[Math.Clamp(bin, 0, bins - 1)]++;
        }

        double total = db.Length;
        var sum = 0.0;

        for (var b = 0; b < bins; b++)
        {
            sum += b * (double)counts[b];
        }

        var below = 0.0;
        var weightBelow = 0.0;
        var best = -1.0;
        var bestBin = 0;

        for (var b = 0; b < bins; b++)
        {
            weightBelow += counts[b];

            if (weightBelow == 0)
            {
                continue;
            }

            var weightAbove = total - weightBelow;

            if (weightAbove == 0)
            {
                break;
            }

            below += b * (double)counts[b];

            var meanBelow = below / weightBelow;
            var meanAbove = (sum - below) / weightAbove;
            var between = weightBelow * weightAbove
                * (meanBelow - meanAbove) * (meanBelow - meanAbove);

            if (between > best)
            {
                best = between;
                bestBin = b;
            }
        }

        return low + ((bestBin + 0.5) * width);
    }

    /// <summary>
    /// How long a key-down may dip below the trigger without ending, in
    /// milliseconds.
    /// </summary>
    /// <remarks>
    /// <para>**SIZED FROM THE FADING AND BOUNDED BY THE SHORTEST REAL GAP** (work
    /// instruction 054, task 3). Unit 053 measured dropouts inside key-down at 32
    /// to 53 ms on this corpus, and this unit measured that the existing bridging
    /// absorbs 20 ms and gives out at 30 — so the fading sits just past what is
    /// already handled.</para>
    /// <para>**THE BOUND IS THE INTER-ELEMENT GAP AND IT IS ASSERTED, NOT
    /// REMEMBERED.** At the fastest speed the decoder considers, one dit is the
    /// gap, so the hold-over must be shorter than a dit at
    /// <see cref="CwProbabilisticDecoder.FastestWpm"/> or it bridges a real gap
    /// and welds two elements into one. That bound is what stops this being sized
    /// to the fading alone.</para>
    /// <para>**TWELVE MILLISECONDS, ADOPTED ON A MONOTONIC REGION AND BOUNDED BY
    /// A FLOOR.** Swept over the whole corpus, precision reads 0.888 at nought,
    /// 0.888 at 8, **0.894 at 12**, 0.905 at 16 and 0.898 at 24. Nought through
    /// sixteen is non-decreasing, so sixteen is the top of the monotonic region
    /// and was the first candidate.</para>
    /// <para>**SIXTEEN COST AN ANCHOR AND IS NOT TAKEN.** It broke
    /// `cw-2026-08-22-031838`'s adjudicated run `, AND` in
    /// `TheAdjudicatedReadingsKeepReadingTests`, and §12.5 does not let a floor be
    /// lowered to fit a change. Twelve holds every floor in the suite, and it is
    /// the better point on two of the three numbers anyway: yield **0.750**
    /// against 0.745 at nought and 0.742 at sixteen, and substitutions **15**
    /// against 17 and 18. The 1.1 points of precision given up against sixteen buy
    /// an anchor that stays.</para>
    /// <para>**IT IS THREE HOPS, AND IT STILL DOES NOT REACH THE FADING.** The
    /// dropouts unit 053 measured run 32 to 53 ms. The safe bound was 30 when
    /// this was written and is 40 now, because
    /// <see cref="CwProbabilisticDecoder.FastestWpm"/> came down to thirty (work
    /// instruction 056, task 1) — so for the first time the bound reaches the
    /// lower half of the fading, and the hold-over still does not, because
    /// twelve is where the locks put it rather than where the bound does.</para>
    /// <para>**RE-SWEPT ACROSS THE WHOLE NEWLY LEGAL RANGE AND TWELVE SURVIVED**
    /// (work instruction 056, task 1). Precision reads **0.901 at 12, 0.926 at
    /// 16, 0.939 at 20, 0.939 at 24**, then falls: 0.930 at 28, 0.920 at 32,
    /// 0.910 at 36 and 40. Yield is flat at 0.878 through 24 and drops to 0.841
    /// by 36. On the average alone the answer would be 20 — monotonic to it,
    /// tied with 24, and 3.8 points of precision better than 12.</para>
    /// <para>**AND IT COSTS THE SAME ANCHOR SIXTEEN COST, WHICH IS WHY IT IS NOT
    /// TAKEN.** `cw-2026-08-22-031838`'s adjudicated `, AND` survives at twelve
    /// and does not at sixteen or at twenty: the read goes `, 2, 2, AND 2` to
    /// `, 2, 2,■AND■2■` to `, 2, 2,■■AND■■■`. **The mechanism is visible in that
    /// progression** — bridging inside a key-down lengthens the mark and shortens
    /// the gap after it, so the character gaps this sender leaves fall below what
    /// separates them from element gaps and the spacing collapses into blocks.
    /// Tim's ruling with this order settles what to do about it: the average
    /// floor may move only when every individual lock holds, and a change that
    /// drops one is reverted regardless of what it does to the average.</para>
    /// <para>**WHAT THE OLD REMARKS CLAIMED FOR TWELVE NO LONGER HOLDS AND IS
    /// CORRECTED HERE.** It said twelve bought 0.6 points of precision over
    /// nought; that was measured at a ceiling of forty words a minute. What
    /// twelve buys now is measured above and is nothing at all over sixteen or
    /// twenty on the average — it buys one adjudicated anchor, and that is the
    /// whole of its case.</para>
    /// <para>**DIT SCATTER BARELY MOVED AND THE DECODE IMPROVED ANYWAY**, which is
    /// worth recording because dit CV was the measure this change was expected to
    /// be judged on. Across the whole sweep it changes by hundredths and not
    /// always downward — `134712` runs 0.432, 0.432, 0.432, 0.428, 0.441, 0.462.
    /// **The scatter was a poor proxy for the reading**, and the reading is what
    /// the goal is stated in.</para>
    /// </remarks>
    public static double HoldOverMilliseconds { get; set; } = 12.0;

    /// <summary>The longest hold-over that cannot bridge a real gap.</summary>
    /// <remarks>
    /// A dit at the fastest speed the decoder will consider. At 30 words a minute
    /// that is 40 ms, and the inter-element gap is one dit, so anything at or
    /// above it can weld two elements together.
    /// </remarks>
    public static double LongestSafeHoldOverMs
        => 1200.0 / CwProbabilisticDecoder.FastestWpm;

    /// <summary>Mark and gap lengths from a two-level trigger.</summary>
    /// <remarks>
    /// **THE HOLD-OVER APPLIES ONLY INSIDE A KEY-DOWN THAT HAS ALREADY BEEN
    /// ADMITTED.** A dip while the key is up is not extended into an element —
    /// that would turn a noise crossing into a mark, which is the opposite of
    /// what this is for (§0.0, HM-DEC-120).
    /// </remarks>
    private static (List<double> Marks, List<double> Gaps) Runs(
        double[] db, double cut, double hysteresisDb, double hopMilliseconds)
    {
        var on = cut + hysteresisDb;
        var off = cut - hysteresisDb;
        var marks = new List<double>();
        var gaps = new List<double>();
        var keyDown = db[0] > on;
        var runStart = 0;

        var holdHops = (int)Math.Floor(
            Math.Min(HoldOverMilliseconds, LongestSafeHoldOverMs - 1e-9)
            / hopMilliseconds);

        for (var i = 1; i < db.Length; i++)
        {
            var changed = keyDown ? db[i] < off : db[i] > on;

            if (!changed)
            {
                continue;
            }

            // **A KEY-DOWN THAT COMES BACK INSIDE THE HOLD-OVER NEVER ENDED.**
            // Look ahead: if the trigger reopens within the hold, this dip is a
            // fade in the middle of one element rather than the end of it.
            if (keyDown && holdHops > 0 && ReopensWithin(db, i, holdHops, on))
            {
                continue;
            }

            var hops = i - runStart;

            if (hops >= ShortestRunHops)
            {
                (keyDown ? marks : gaps).Add(hops * hopMilliseconds);
            }

            keyDown = !keyDown;
            runStart = i;
        }

        return (marks, gaps);
    }

    /// <summary>Whether the trigger reopens within the hold-over.</summary>
    /// <param name="db">The envelope in decibels.</param>
    /// <param name="from">Where the dip began.</param>
    /// <param name="holdHops">How many hops the hold-over covers.</param>
    /// <param name="on">The level that reopens the trigger.</param>
    /// <returns>True where the key comes back before the hold-over runs out.</returns>
    private static bool ReopensWithin(double[] db, int from, int holdHops, double on)
    {
        var last = Math.Min(from + holdHops, db.Length - 1);

        for (var i = from + 1; i <= last; i++)
        {
            if (db[i] > on)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// The middle of the short cluster, which is a median of its members rather
    /// than the centroid.
    /// </summary>
    /// <remarks>
    /// **A CENTROID IS PULLED BY WHATEVER ELSE LANDED IN THE CLUSTER.** Real
    /// off-air audio puts a few very short crossings in the short heap however
    /// deep the trigger is, and a mean over the logarithms follows them down. The
    /// median of the members does not.
    /// </remarks>
    private static double ShortClusterMedian(IReadOnlyList<double> values)
    {
        var (low, high) = TwoMeansOnLogs(values);

        if (high <= low)
        {
            return low;
        }

        var boundary = Math.Sqrt(low * high);
        var members = values.Where(v => v <= boundary).OrderBy(v => v).ToArray();

        return members.Length == 0 ? low : members[members.Length / 2];
    }

    /// <summary>
    /// Two clusters on the logarithm of the durations, returned shortest first.
    /// </summary>
    /// <remarks>
    /// Seeded at the tenth and ninetieth percentiles, which is enough on a
    /// distribution with two heaps in it, and run for a fixed number of passes so
    /// the same audio always gives the same answer.
    /// </remarks>
    private static (double Short, double Long) TwoMeansOnLogs(
        IReadOnlyList<double> values)
    {
        var logs = values.Select(v => Math.Log(Math.Max(v, 1e-6))).ToArray();

        Array.Sort(logs);

        var low = logs[logs.Length / 10];
        var high = logs[logs.Length * 9 / 10];

        if (high - low < 1e-9)
        {
            var only = Math.Exp(logs[logs.Length / 2]);

            return (only, only);
        }

        for (var pass = 0; pass < 20; pass++)
        {
            var lowSum = 0.0;
            var lowCount = 0;
            var highSum = 0.0;
            var highCount = 0;

            foreach (var value in logs)
            {
                if (Math.Abs(value - low) <= Math.Abs(value - high))
                {
                    lowSum += value;
                    lowCount++;
                }
                else
                {
                    highSum += value;
                    highCount++;
                }
            }

            if (lowCount == 0 || highCount == 0)
            {
                break;
            }

            low = lowSum / lowCount;
            high = highSum / highCount;
        }

        return (Math.Exp(low), Math.Exp(high));
    }
}
