namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// Whether an admitted station's elements were sent by one person or two, judged
/// by the pitch of the elements themselves.
/// </summary>
/// <remarks>
/// <para>**TWO OPERATORS A FEW HERTZ APART ARE ONE STATION TO EVERYTHING ELSE IN
/// THIS ENGINE.** The survey admits a bin, the peak refines a pitch, the decoder
/// mixes down at it, and both senders come through the same 45 Hz integrator into
/// one envelope. Their marks then interleave in time and the clock fit sees one
/// fist with impossible scatter — on `cw-2026-08-31-002829` the dit coefficient
/// of variation is 0.47, which is not a fist at all.</para>
/// <para>**AND THE MEASUREMENT THAT SEPARATES THEM DID NOT EXIST UNTIL NOW.**
/// Every pitch in this engine was a pitch for a whole recording
/// (<see cref="CwElementPitch"/>), so the question could not be asked. It can be
/// asked per element, and the answer on that capture is two heaps 12.3 Hz apart
/// with the lower one confined to a 1.9 second burst.</para>
/// <para>**CONSERVATIVE, AND THE ASYMMETRY IS DELIBERATE.** Splitting one wobbly
/// sender in two is worse than the collision it would fix: a collision reads
/// badly and looks like it reads badly, where a bad split reads cleanly and is
/// wrong, which is what §0.0 exists to prevent. So every threshold here is set to
/// refuse first, and the survey behind them is in the report rather than in a
/// comment.</para>
/// <para>**IT LABELS BY PITCH AND CLAIMS NOTHING ELSE** (Tim's ruling with work
/// instruction 056): two streams may be a conversation or two people who cannot
/// hear each other, and nothing here says which.</para>
/// <para>**AND AS OF WORK INSTRUCTION 056 IT DOES NOT YET LABEL ANYTHING, BECAUSE
/// NO CRITERION SURVIVED ITS OWN SURVEY.** <see cref="Divide"/> measures and
/// returns every quantity below and returns `Split: false` unconditionally. This
/// is not a stub and it is not caution for its own sake: four criteria were
/// measured across every capture in the tree and each one either missed the case
/// the work order names or fired on a recording known to hold one operator.</para>
/// <para><list type="bullet">
/// <item>**Separation in hertz.** With only marks of 100 ms or longer voting, the
/// clean captures separate by 0.1 to 0.8 Hz and `002829` reaches 9.0 — but so do
/// `cw-2026-08-23-001831` at 7.2 and `cw-2026-08-28-005051` at 8.7, and the two
/// captures that clear 15 Hz are `003212` and `003229`, which the work order
/// treats as one refused CQ rather than as two senders.</item>
/// <item>**Separation over the pooled scatter.** `002829` scores 4.2 and
/// `cw-2026-08-23-001831` scores 17.2 on 7.2 Hz of separation, because a very
/// steady single sender has almost no scatter for the ratio to divide by.</item>
/// <item>**Handovers.** A bisected single heap crosses its own boundary
/// constantly — 20 to 90 times on the clean captures — where a real burst crosses
/// twice, so the test has the sign the data does not.</item>
/// <item>**The trough in the sorted pitches.** The clean captures show 0.10 Hz
/// and `002829` shows 1.30, which is the wrong way round for a threshold: its
/// second sender is mostly dits, and letting dits vote lets every noise-fitted
/// short mark vote with them.</item>
/// </list></para>
/// <para>**THE SECOND SENDER IS VISIBLE AND IS NOT IN DISPUTE.** Read in time
/// order at a mixdown of 608.5 Hz, `002829` puts thirteen consecutive marks at 599
/// to 605 Hz between 13.58 and 15.45 seconds with 613 Hz either side of them. The
/// eye finds it at once. What does not exist yet is a rule that finds it without
/// also finding one in `cw-2026-08-18-003758`, which Hamlet reads at a precision
/// of 1.000.</para>
/// <para>**SO NOTHING IS SPLIT.** Splitting one sender in two is worse than the
/// collision it fixes — a collision reads badly and looks like it reads badly,
/// where a bad split reads cleanly and is wrong (§0.0) — and the work order's own
/// rule is that in doubt there is one sender.</para>
/// </remarks>
public static class CwStreamSplit
{
    /// <summary>
    /// The coarsest per-element resolution allowed to vote on whether there are
    /// two senders, in hertz.
    /// </summary>
    /// <remarks>
    /// **TEN, WHICH IS A MARK OF A HUNDRED MILLISECONDS OR LONGER.** A dit
    /// resolves to about 18 Hz (<see cref="CwElementPitch.ResolutionHz"/>), so a
    /// dit cannot settle a 13 Hz question and a vote taken over dits would be a
    /// vote on the interpolator's noise. Short marks are still *assigned* to a
    /// stream once the boundary exists; they are not allowed to decide that it
    /// does.
    /// </remarks>
    public static double TrustedResolutionHz { get; set; } = 10.0;

    /// <summary>How many trusted marks each side must hold to be a sender.</summary>
    /// <remarks>
    /// **FIVE, BECAUSE FOUR IS A CALLSIGN'S WORTH OF ACCIDENT.** One long mark at
    /// the edge of the passband is a fade or a splatter; five of them, all on the
    /// same side of a boundary, is somebody sending.
    /// </remarks>
    public const int LeastTrustedMarks = 5;

    /// <summary>
    /// How many times the measurement error the two centres must stand apart.
    /// </summary>
    /// <remarks>
    /// **ONE AND A HALF, MEASURED RATHER THAN CHOSEN.** The margin is over
    /// <see cref="TrustedResolutionHz"/>, so a split needs 15 Hz between the
    /// centres. Surveyed across every capture in the tree, the single-sender
    /// recordings produce a two-means separation of a few hertz — the spread of
    /// one fist through one filter — and `002829` produces 12 to 13. The figure
    /// sits above the first and is reached by the second only when the burst is
    /// there to reach it with.
    /// </remarks>
    public const double MarginOverError = 1.5;

    /// <summary>
    /// The widest two centres may stand apart and still both be inside the
    /// detector, in hertz.
    /// </summary>
    /// <remarks>
    /// **A CEILING AS WELL AS A FLOOR, AND THE CEILING IS THE ONE PEOPLE FORGET.**
    /// Two peaks a hundred hertz apart in an envelope taken through a 45 Hz filter
    /// are not two senders in the passband; one of them is leakage, an image, or
    /// the filter's own skirt, and unit 055 lost a whole measurement to exactly
    /// that mistake. Beyond this, the answer is one sender and a marked
    /// measurement rather than two streams.
    /// </remarks>
    public const double WidestApartHz = 45.0;

    /// <summary>
    /// How many pooled standard deviations the two centres must stand apart.
    /// </summary>
    /// <remarks>
    /// **THE FIGURE IS SET FROM THE SURVEY AND THE SURVEY IS IN THE REPORT**
    /// (work instruction 056, task 3). Its value is stated in
    /// <see cref="MarginOverError"/>'s neighbour rather than argued here.
    /// </remarks>
    public const double LeastSeparation = 4.0;

    /// <summary>
    /// How many times the minority cluster must hand back to the majority.
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS THE TEST THAT WORKS, AND THE TWO BEFORE IT ARE NOT ENOUGH
    /// ON THEIR OWN.** Surveyed across every capture in the tree, no threshold on
    /// separation alone divides the two-sender case from the clean ones: at a
    /// trust of 25 Hz `cw-2026-08-31-002829` separates by 10.4 Hz and
    /// `cw-2026-08-18-003758`, which Hamlet reads at a precision of 1.000 and
    /// which holds one operator, separates by 22.4. Loosening the trust enough to
    /// admit a fast sender's dits admits every noise-fitted short mark with them.
    /// </para>
    /// <para>**WHAT SEPARATES THEM IS TIME AND NOT HERTZ.** A single sender whose
    /// pitch wanders — a drifting oscillator, a fading path, a hand on the RIT —
    /// crosses any boundary you draw **once**, because drift is monotonic over the
    /// half-minute a capture lasts. Two operators alternate, so the second one's
    /// marks are *bracketed*: the first sender is there before the burst and there
    /// again after it. On `002829` that is thirteen consecutive marks at 599 to
    /// 605 Hz between 13.58 and 15.45 seconds, with 613 Hz either side.</para>
    /// <para>**TWO HANDOVERS, WHICH IS THE FEWEST THAT CAN BRACKET ANYTHING.**
    /// One handover is drift. Two is a burst that began and ended while somebody
    /// else held the frequency, and no amount of drift produces that in one
    /// direction and back.</para>
    /// </remarks>
    public const int LeastHandovers = 2;

    /// <summary>What a two-way split of one element stream found.</summary>
    /// <param name="Split">
    /// True where the elements divide into two senders. **Always false today**,
    /// and the type's own remarks say why: no criterion measured across this
    /// corpus divides the two-sender case from the clean ones. It is a field
    /// rather than a removed concept so that the unit which proves a criterion
    /// has one place to set it.
    /// </param>
    /// <param name="Trusted">
    /// How many marks were long enough to vote. Below
    /// <see cref="LeastTrustedMarks"/> twice over there is nothing to divide, and
    /// that is reported rather than resolved.
    /// </param>
    /// <param name="LowerHz">The lower cluster's centre, or NaN.</param>
    /// <param name="UpperHz">The upper cluster's centre, or NaN.</param>
    /// <param name="LowerCount">Trusted marks in the lower cluster.</param>
    /// <param name="UpperCount">Trusted marks in the upper cluster.</param>
    /// <param name="ApartHz">
    /// How far the centres stand apart, or NaN where there was nothing to
    /// measure. **Reported whether or not the split was taken**, because a
    /// separation just under the margin is the thing a later ruling would want to
    /// see.
    /// </param>
    /// <param name="ScatterHz">
    /// The scatter inside the clusters, pooled, or NaN where there was nothing to
    /// measure.
    /// </param>
    /// <param name="Separation">
    /// <see cref="ApartHz"/> in units of <see cref="ScatterHz"/>. **This is the
    /// figure that tells two heaps from one heap bisected**, and the raw hertz
    /// alone does not: a two-means run always returns two centres, so on a single
    /// sender it returns the two halves of his own scatter and calls them
    /// clusters. Infinity where the scatter is nought, which is a real answer for
    /// a signal that never wobbled.
    /// </param>
    /// <param name="Handovers">
    /// How many times the marks cross the boundary in the order they were sent.
    /// **One is drift and two is a second operator** — see
    /// <see cref="LeastHandovers"/>.
    /// </param>
    public readonly record struct CwStreamDivision(
        bool Split,
        int Trusted,
        double LowerHz,
        double UpperHz,
        int LowerCount,
        int UpperCount,
        double ApartHz,
        double ScatterHz = double.NaN,
        double Separation = double.NaN,
        int Handovers = 0)
    {
        /// <summary>Nothing measured.</summary>
        public static CwStreamDivision None { get; } = new(
            false, 0, double.NaN, double.NaN, 0, 0, double.NaN);
    }

    /// <summary>Divide an element stream by the pitch of its own marks.</summary>
    /// <param name="elements">The stream, with per-element pitch measured.</param>
    /// <returns>What the division found.</returns>
    public static CwStreamDivision Divide(IReadOnlyList<CwElement> elements)
    {
        ArgumentNullException.ThrowIfNull(elements);

        var trusted = new List<double>();

        foreach (var element in elements)
        {
            if (!element.IsMark || double.IsNaN(element.PitchHz))
            {
                continue;
            }

            var milliseconds =
                element.Hops * CwProbabilisticDecoder.HopMilliseconds;

            if (CwElementPitch.ResolutionHz(milliseconds) <= TrustedResolutionHz)
            {
                trusted.Add(element.PitchHz);
            }
        }

        // Kept in the order they were sent, which is what the handover test reads.
        var inTime = trusted.ToArray();

        if (trusted.Count < 2 * LeastTrustedMarks)
        {
            return CwStreamDivision.None with { Trusted = trusted.Count };
        }

        var (lower, upper) = TwoMeans(trusted);
        var boundary = (lower + upper) / 2;

        var low = trusted.Where(v => v <= boundary).ToArray();
        var high = trusted.Where(v => v > boundary).ToArray();
        var apart = upper - lower;
        var scatter = PooledScatter(low, high);
        var separation = scatter <= 0 ? double.PositiveInfinity : apart / scatter;

        var handovers = Handovers(inTime, boundary);

        // **THE VERDICT IS WITHHELD, AND THAT IS THE FINDING** (work instruction
        // 056, task 3). Every statistic below is real and is reported; what is
        // not here is a `true`. See the type's own remarks for the survey that
        // refused it.
        return new CwStreamDivision(
            Split: false, trusted.Count, lower, upper, low.Length, high.Length,
            apart, scatter, separation, handovers);
    }

    /// <summary>Assign every element to a stream, by the boundary between them.</summary>
    /// <param name="elements">The stream, with per-element pitch measured.</param>
    /// <param name="division">What <see cref="Divide"/> found.</param>
    /// <returns>
    /// Two lists, lower stream first, or one list holding everything where the
    /// division was refused.
    /// </returns>
    /// <remarks>
    /// <para>**A GAP BELONGS TO WHOEVER WAS SENDING EITHER SIDE OF IT.** A gap has
    /// no pitch of its own (<see cref="CwElementPitch"/>), so it cannot be
    /// assigned by measurement, and putting every gap in both streams would give
    /// each sender the other's silence as evidence about his own spacing. It goes
    /// to the stream its preceding mark went to, which is the only assignment that
    /// keeps each stream's gaps its own.</para>
    /// <para>**AND A SHORT MARK IS ASSIGNED THOUGH IT COULD NOT VOTE.** The
    /// boundary was settled by marks that could measure it; once it exists, a dit
    /// 18 Hz uncertain still falls on one side of it more often than the other.
    /// That is a weaker claim than the boundary itself, and it is why the split is
    /// refused unless the boundary is well clear of the error.</para>
    /// </remarks>
    public static IReadOnlyList<IReadOnlyList<CwElement>> Apart(
        IReadOnlyList<CwElement> elements, CwStreamDivision division)
    {
        ArgumentNullException.ThrowIfNull(elements);

        if (!division.Split)
        {
            return new[] { elements };
        }

        var boundary = (division.LowerHz + division.UpperHz) / 2;
        var lower = new List<CwElement>();
        var upper = new List<CwElement>();
        var lastWasLower = true;

        foreach (var element in elements)
        {
            if (element.IsMark && !double.IsNaN(element.PitchHz))
            {
                lastWasLower = element.PitchHz <= boundary;
            }

            (lastWasLower ? lower : upper).Add(element);
        }

        return new[] { lower, upper };
    }

    /// <summary>How many times the marks cross the boundary, in time order.</summary>
    /// <remarks>
    /// **DRIFT CROSSES ONCE AND A SECOND OPERATOR CROSSES TWICE.** See
    /// <see cref="LeastHandovers"/> for why that is the whole test.
    /// </remarks>
    private static int Handovers(IReadOnlyList<double> inTime, double boundary)
    {
        var crossings = 0;
        var above = inTime[0] > boundary;

        foreach (var value in inTime)
        {
            var now = value > boundary;

            if (now != above)
            {
                crossings++;
                above = now;
            }
        }

        return crossings;
    }

    /// <summary>The scatter inside the two clusters, pooled.</summary>
    /// <remarks>
    /// A pooled standard deviation, so a cluster of five and a cluster of fifty
    /// contribute in proportion to what each of them measured. Nought where every
    /// member of both sits at one frequency, which the caller reads as an infinite
    /// separation rather than as a division by zero.
    /// </remarks>
    private static double PooledScatter(
        IReadOnlyList<double> low, IReadOnlyList<double> high)
    {
        var total = 0.0;
        var count = 0;

        foreach (var side in new[] { low, high })
        {
            if (side.Count < 2)
            {
                continue;
            }

            var mean = side.Average();

            foreach (var value in side)
            {
                total += (value - mean) * (value - mean);
            }

            count += side.Count - 1;
        }

        return count == 0 ? 0 : Math.Sqrt(total / count);
    }

    /// <summary>Two clusters on the pitches, returned lowest first.</summary>
    /// <remarks>
    /// Seeded at the tenth and ninetieth percentiles and run for a fixed number of
    /// passes, so the same elements always give the same answer — the same shape
    /// <see cref="CwUnitEstimator"/> uses on durations, on linear hertz rather
    /// than on logarithms because a few hertz out of six hundred is a difference
    /// and not a ratio.
    /// </remarks>
    private static (double Lower, double Upper) TwoMeans(IReadOnlyList<double> values)
    {
        var sorted = values.OrderBy(v => v).ToArray();
        var lower = sorted[sorted.Length / 10];
        var upper = sorted[sorted.Length * 9 / 10];

        for (var pass = 0; pass < 24; pass++)
        {
            var boundary = (lower + upper) / 2;
            var below = sorted.Where(v => v <= boundary).ToArray();
            var above = sorted.Where(v => v > boundary).ToArray();

            if (below.Length == 0 || above.Length == 0)
            {
                break;
            }

            lower = below.Average();
            upper = above.Average();
        }

        return (lower, upper);
    }
}
