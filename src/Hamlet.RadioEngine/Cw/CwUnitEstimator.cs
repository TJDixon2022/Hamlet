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

        // **A CLIPPED READING THAT PUTS THE CHARACTER GAP PAST THE WORD GAP IS NOT
        // SEPARATED** (unit 405's G1, built by work instructions 413 and 431 and
        // re-applied by 440 under R78). The clip carries a short element centroid
        // back into the character gap as boundary squared over element, and the
        // kinds then cost backwards: every gap nearer the word want than the
        // character want becomes a space, and a letter breaks where its own element
        // gap sat. Unit 440 traced 3 of the 27 split sure-wrong letters on the keyed
        // recordings to exactly this reading. Refused, the stream keeps the last gaps
        // it stood behind. It was taken out twice on character counts, which R78 no
        // longer judges by.
        if (character >= word)
        {
            return textbook;
        }

        return new CwGapLengths(
            element,
            character,
            word,
            true,
            Math.Abs(characterBoundary - wantedCharacter) > 1e-6,
            Math.Abs(wordBoundary - wantedWord) > 1e-6);
    }

    /// <summary>
    /// The gap this sender leaves between characters, when the element and
    /// character heaps can be told apart, whether or not a word heap can.
    /// </summary>
    /// <param name="envelope">Envelope magnitudes, one every hop.</param>
    /// <param name="hopMilliseconds">How long one hop lasts.</param>
    /// <param name="unitMilliseconds">The dit, used only to test the boundary.</param>
    /// <param name="hysteresisDb">How deep the trigger is.</param>
    /// <returns>The character centroid in milliseconds, or null.</returns>
    /// <remarks>
    /// <para>**THE WORD TROUGH IS WHAT FAILS, AND THIS DOES NOT ASK FOR IT**
    /// (work instruction 415). Twelve seconds hold a handful of word gaps, so
    /// <see cref="MeasureGaps"/> refuses most windows at its second trough and
    /// the path reads the textbook three and seven units; this sender's own
    /// character gap is near five. The character heap is found in those same
    /// windows, and it is all the space decision needs.</para>
    /// <para>**THE BOUNDARY UNDER IT HAS TO LAND WHERE MEASUREGAPS WOULD TAKE
    /// IT UNMOVED**, 1.3 to 2.6 units. Measured on `cw-2026-08-17-013347`, the
    /// three heaps otherwise put the element gaps in the middle one at 1.1
    /// units and every character gap reads as a word gap. That range is the one
    /// <see cref="MeasureGaps"/> already clips to, so nothing new is chosen.</para>
    /// <para><see cref="MeasureGaps"/> is not changed and nothing it returns
    /// moves.</para>
    /// </remarks>
    public static double? MeasureCharacterGap(
        IReadOnlyList<double> envelope,
        double hopMilliseconds,
        double unitMilliseconds,
        double hysteresisDb = HysteresisDb)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        if (unitMilliseconds <= 0)
        {
            return null;
        }

        var (_, gaps) = Elements(envelope, hopMilliseconds, hysteresisDb);

        if (gaps.Count < 12)
        {
            return null;
        }

        var centroids = ThreeMeansOnLogs(gaps);

        if (centroids[1] / centroids[0] < 1.5
            || !IsTrough(gaps, centroids[0], centroids[1]))
        {
            return null;
        }

        var boundary = Math.Sqrt(centroids[0] * centroids[1]) / unitMilliseconds;

        return boundary is >= 1.3 and <= 2.6 ? centroids[1] : null;
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

    /// <summary>The unit the marks alone imply, dits and dahs taken apart.</summary>
    /// <param name="marks">Mark lengths in milliseconds.</param>
    /// <returns>The unit in milliseconds, or NaN where the marks are all one kind.</returns>
    /// <remarks>
    /// **NO GAP IS ASKED** (work instruction 441). <see cref="Measure"/> averages the
    /// short marks with the short gaps to cancel the cut's skirt, and a fade that
    /// drops out inside a mark puts a fifteen-millisecond gap in the short heap and
    /// drags the unit down with it: `031838` measured a 55 ms dit mark beside a
    /// 15 ms element gap and was read at 34 words a minute with dahs of 165 to 220
    /// ms. Where the longest mark is at least twice the shortest both kinds are
    /// there; they are split at the geometric mean, and the unit is the geometric
    /// mean of the dits' median and a third of the dahs' median. It reads long by
    /// the skirt.
    /// </remarks>
    public static double MarkUnit(IReadOnlyList<double> marks)
    {
        ArgumentNullException.ThrowIfNull(marks);

        if (marks.Count == 0)
        {
            return double.NaN;
        }

        var min = marks.Min();
        var max = marks.Max();

        if (max < 2 * min)
        {
            return double.NaN;
        }

        var boundary = Math.Sqrt(min * max);
        var dits = marks.Where(m => m <= boundary).OrderBy(m => m).ToArray();
        var dahs = marks.Where(m => m > boundary).OrderBy(m => m).ToArray();

        return Math.Sqrt(dits[dits.Length / 2] * dahs[dahs.Length / 2] / 3);
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

        return Runs(db, Otsu(db), hysteresisDb, hopMilliseconds);
    }

    /// <summary>
    /// The marks that begin and end inside a stretch and the gaps between them,
    /// cut exactly as <see cref="Elements"/> cuts.
    /// </summary>
    /// <param name="envelope">The stretch's envelope magnitudes.</param>
    /// <param name="hopMilliseconds">How long one hop lasts.</param>
    /// <returns>Mark and gap lengths, in milliseconds.</returns>
    /// <remarks>
    /// **A LETTER'S OWN ELEMENTS, NOT ITS PADDING** (work instruction 445). The
    /// first run touches the stretch's start and the last never ends inside it,
    /// so neither is whole; a gap before the first whole mark or after the last
    /// is the space beside the letter, not a gap inside it.
    /// </remarks>
    internal static (IReadOnlyList<double> Marks, IReadOnlyList<double> Gaps) InnerElements(
        IReadOnlyList<double> envelope, double hopMilliseconds)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        if (envelope.Count < 2)
        {
            return (Array.Empty<double>(), Array.Empty<double>());
        }

        var db = new double[envelope.Count];

        for (var i = 0; i < envelope.Count; i++)
        {
            db[i] = 20 * Math.Log10(Math.Max(envelope[i], 1e-12));
        }

        var cut = Otsu(db);
        var on = cut + HysteresisDb;
        var off = cut - HysteresisDb;
        var runs = new List<(bool Mark, int Hops)>();
        var keyDown = db[0] > on;
        var runStart = 0;

        for (var i = 1; i < db.Length; i++)
        {
            if (!(keyDown ? db[i] < off : db[i] > on))
            {
                continue;
            }

            var hops = i - runStart;

            if (runStart > 0 && hops >= ShortestRunHops)
            {
                runs.Add((keyDown, hops));
            }

            keyDown = !keyDown;
            runStart = i;
        }

        var first = runs.FindIndex(r => r.Mark);
        var last = runs.FindLastIndex(r => r.Mark);
        var marks = runs.Where(r => r.Mark).Select(r => r.Hops * hopMilliseconds).ToList();
        var gaps = first < 0
            ? new List<double>()
            : runs.Skip(first).Take(last - first + 1).Where(r => !r.Mark).Select(r => r.Hops * hopMilliseconds).ToList();

        return (marks, gaps);
    }

    /// <summary>
    /// The level that splits the envelope into two classes with the least
    /// variance inside them.
    /// </summary>
    /// <remarks>
    /// Otsu's method over a histogram of the envelope in decibels. It is a
    /// measurement of this recording rather than a level anybody chose, which is
    /// what lets the trigger depth be the only constant here.
    /// </remarks>
    private static double Otsu(double[] db)
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

    /// <summary>Mark and gap lengths from a two-level trigger.</summary>
    private static (List<double> Marks, List<double> Gaps) Runs(
        double[] db, double cut, double hysteresisDb, double hopMilliseconds)
    {
        var on = cut + hysteresisDb;
        var off = cut - hysteresisDb;
        var marks = new List<double>();
        var gaps = new List<double>();
        var keyDown = db[0] > on;
        var runStart = 0;

        for (var i = 1; i < db.Length; i++)
        {
            var changed = keyDown ? db[i] < off : db[i] > on;

            if (!changed)
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
