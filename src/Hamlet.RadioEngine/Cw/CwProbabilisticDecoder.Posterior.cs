namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// The forward–backward pass, and the posterior it yields.
/// </summary>
/// <remarks>
/// <para>**THIS IS DIFFERENT IN KIND FROM EVERY QUANTITY MEASURED SO FAR.** Five
/// have been tested against correctness and all five were negative — the fit
/// ratio at −0.179 and −0.203, `MarginLlr` at −0.351, `MarginShareForRecord` at
/// −0.345, `SpanMarginForRecord` at −0.190. Each is a difference of path scores,
/// and the scores carry an unbounded `−e²/2σ²` term, so **a margin between two
/// numbers that both scale with loudness still scales with loudness.**</para>
/// <para>**A POSTERIOR IS A RATIO OVER THE SUM OF ALL PATHS**, so the level terms
/// cancel in the normalisation and it cannot grow with loudness by construction.
/// That is why a sixth quantity of the old family would have been a known dead
/// end and this is not.</para>
/// <para>**IT IS ONLY COMPUTABLE BECAUSE THE LATTICE IS NOW INDEXED BY
/// (HOP, KIND).** A sum has to range over all paths reaching a node, and while
/// alternation was checked against the winning path's parity those paths did not
/// share a state — there was nothing well-defined to sum.</para>
/// <para>**LOG DOMAIN THROUGHOUT.** The failure mode of this algorithm is
/// underflow and overflow, and this decoder has produced 5,521,967, 17.2 million
/// and quadrillions on degenerate bins. Nothing here exponentiates a raw score:
/// <see cref="LogSum"/> factors out the larger term so the exponential only ever
/// sees a non-positive number.</para>
/// </remarks>
public static partial class CwProbabilisticDecoder
{
    /// <summary>
    /// How much of a log-domain gap is worth adding before it is dropped.
    /// </summary>
    /// <remarks>
    /// Beyond about seven hundred `Math.Exp` underflows to zero anyway, so
    /// dropping the term is exact rather than approximate.
    /// </remarks>
    private const double LogSumFloor = -700;

    /// <summary>The scaling exponent on the path score.</summary>
    /// <remarks>
    /// <para>**ONE, WHICH IS NO TEMPERATURE AT ALL, UNTIL A SWEEP SAYS
    /// OTHERWISE.** Unit 049 measured the over-counting the evidence term commits
    /// and it is 2.22, not the eighty-nine an order supposed: the sum runs over
    /// five-millisecond hops rather than raw samples, so two hundred terms a
    /// second stand against ninety independent degrees of freedom at 45 Hz. That
    /// implies an alpha near 0.45.</para>
    /// <para>**BUT THE OVER-COUNT IS NOT WHAT MAKES THE MODEL OVERCONFIDENT.**
    /// Measured on the same corpus, the evidence per element runs 4.9 to 467 nats
    /// against a duration penalty of 0.136 for a span a fifth off its expected
    /// length — a ratio near two thousand. A 2.22 over-count does not explain
    /// that, so the figure this constant should take is a measurement rather than
    /// a derivation, and it is swept.</para>
    /// </remarks>
    public const double Temperature = 1.0;

    /// <summary>
    /// The probability of each state, marginalised over every path through the
    /// lattice.
    /// </summary>
    /// <param name="count">How many hops there are.</param>
    /// <param name="downTo">Cumulative key-down log-likelihood.</param>
    /// <param name="upTo">Cumulative key-up log-likelihood.</param>
    /// <param name="unit">The dit, in hops.</param>
    /// <param name="gapHops">This sender's own gap lengths, or null.</param>
    /// <returns>
    /// The posterior at each state in [0,1], or null where the lattice reaches
    /// no end at all.
    /// </returns>
    public static double[,]? Posterior(
        int count, double[] downTo, double[] upTo, double unit, double[]? gapHops)
        => Posterior(count, downTo, upTo, unit, gapHops, Temperature);

    /// <summary>
    /// The same, at a stated temperature, so the exponent can be swept.
    /// </summary>
    /// <param name="count">How many hops there are.</param>
    /// <param name="downTo">Cumulative key-down log-likelihood.</param>
    /// <param name="upTo">Cumulative key-up log-likelihood.</param>
    /// <param name="unit">The dit, in hops.</param>
    /// <param name="gapHops">This sender's own gap lengths, or null.</param>
    /// <param name="alpha">
    /// The scaling exponent on the path score. One leaves it untouched; below one
    /// flattens the distribution.
    /// </param>
    /// <returns>The posterior at each state, or null.</returns>
    /// <remarks>
    /// **THE TEMPERATURE MULTIPLIES THE WHOLE PATH SCORE AND SO CANNOT MOVE THE
    /// ARGMAX.** Scaling every path by one positive constant leaves the largest
    /// largest, which is why this changes the posterior and not one character of
    /// the decode. Scaling the evidence term alone is a different question and a
    /// different task, because it shifts the balance against the duration penalty
    /// and does change what is read.
    /// </remarks>
    public static double[,]? Posterior(
        int count, double[] downTo, double[] upTo, double unit,
        double[]? gapHops, double alpha)
    {
        ArgumentNullException.ThrowIfNull(downTo);
        ArgumentNullException.ThrowIfNull(upTo);

        if (count < 1)
        {
            return null;
        }

        var kinds = Kinds.Length;
        var forward = new double[count + 1, kinds];
        var beta = new double[count + 1, kinds];

        for (var i = 0; i <= count; i++)
        {
            for (var k = 0; k < kinds; k++)
            {
                forward[i, k] = double.NegativeInfinity;
                beta[i, k] = double.NegativeInfinity;
            }
        }

        // **FORWARD: the evidence of every path ending in this state.**
        for (var i = 1; i <= count; i++)
        {
            for (var k = 0; k < kinds; k++)
            {
                var total = double.NegativeInfinity;
                var (shortest, ceiling) = SpanRange(i, k, unit, gapHops);

                for (var span = shortest; span <= ceiling; span++)
                {
                    var j = i - span;
                    var step = alpha
                        * StepOf(i, j, k, downTo, upTo, unit, gapHops);

                    if (j == 0)
                    {
                        // Nothing precedes the first element, so there is no
                        // parity to alternate against.
                        total = LogSum(total, step);

                        continue;
                    }

                    for (var kj = 0; kj < kinds; kj++)
                    {
                        if (Kinds[kj].IsKeyDown == Kinds[k].IsKeyDown
                            || double.IsNegativeInfinity(forward[j, kj]))
                        {
                            continue;
                        }

                        total = LogSum(total, forward[j, kj] + step);
                    }
                }

                forward[i, k] = total;
            }
        }

        // **BACKWARD: the evidence of every path from this state to the end.**
        for (var k = 0; k < kinds; k++)
        {
            beta[count, k] = 0;
        }

        for (var j = count - 1; j >= 1; j--)
        {
            for (var kj = 0; kj < kinds; kj++)
            {
                var total = double.NegativeInfinity;

                for (var k = 0; k < kinds; k++)
                {
                    if (Kinds[kj].IsKeyDown == Kinds[k].IsKeyDown)
                    {
                        continue;
                    }

                    var (shortest, longest) = SpanBounds(k, unit, gapHops);

                    for (var span = shortest; span <= longest; span++)
                    {
                        var i = j + span;

                        if (i > count || double.IsNegativeInfinity(beta[i, k]))
                        {
                            continue;
                        }

                        total = LogSum(
                            total,
                            (alpha * StepOf(i, j, k, downTo, upTo, unit, gapHops))
                            + beta[i, k]);
                    }
                }

                beta[j, kj] = total;
            }
        }

        // The evidence of everything, which is what the ratio is taken against.
        var all = double.NegativeInfinity;

        for (var k = 0; k < kinds; k++)
        {
            all = LogSum(all, forward[count, k]);
        }

        if (double.IsNegativeInfinity(all) || double.IsNaN(all))
        {
            // No path reaches the end at all. Saying nothing is right; a
            // normalisation by nothing would be a number with no meaning (§0.0).
            return null;
        }

        var posterior = new double[count + 1, kinds];

        for (var i = 0; i <= count; i++)
        {
            for (var k = 0; k < kinds; k++)
            {
                var joint = forward[i, k] + beta[i, k];

                // Clamped at the top because floating error can put a ratio a
                // whisker over one, and a probability above one is a number
                // nobody can read.
                posterior[i, k] = double.IsNegativeInfinity(joint) || double.IsNaN(joint)
                    ? 0
                    : Math.Exp(Math.Min(0, joint - all));
            }
        }

        return posterior;
    }

    /// <summary>How long a segment of this kind may be, in hops.</summary>
    private static (int Shortest, int Longest) SpanBounds(
        int k, double unit, double[]? gapHops)
    {
        var kind = Kinds[k];
        var want = gapHops is not null && !kind.IsKeyDown
            ? gapHops[k - 2]
            : kind.Units * unit;
        var shortest = Math.Max(1, (int)(want * ShortestShare));

        return (shortest, Math.Max(shortest + 1, (int)(want * LongestShare)));
    }

    /// <summary>The spans of this kind that can end at this hop.</summary>
    private static (int Shortest, int Ceiling) SpanRange(
        int i, int k, double unit, double[]? gapHops)
    {
        var (shortest, longest) = SpanBounds(k, unit, gapHops);

        return (shortest, Math.Min(longest, i));
    }

    /// <summary>What one segment scores: its evidence, less its length penalty.</summary>
    /// <remarks>
    /// The same expression the Viterbi uses, so the two passes cannot drift
    /// apart (§0: one source of truth).
    /// </remarks>
    private static double StepOf(
        int i, int j, int k, double[] downTo, double[] upTo,
        double unit, double[]? gapHops)
    {
        var kind = Kinds[k];
        var want = gapHops is not null && !kind.IsKeyDown
            ? gapHops[k - 2]
            : kind.Units * unit;

        var evidence = kind.IsKeyDown
            ? downTo[i] - downTo[j]
            : upTo[i] - upTo[j];

        var off = Math.Log(Math.Max(i - j, 1e-9) / want) / LengthToleranceShare;

        return evidence - (0.5 * off * off);
    }

    /// <summary>Add two log-domain quantities without leaving the log domain.</summary>
    /// <remarks>
    /// **NOTHING IS EXPONENTIATED THAT COULD OVERFLOW.** The larger term is
    /// factored out, so `Math.Exp` only ever sees a non-positive number; a gap
    /// wide enough to underflow contributes nothing measurable and is dropped,
    /// which is exact rather than approximate.
    /// </remarks>
    internal static double LogSum(double a, double b)
    {
        if (double.IsNegativeInfinity(a))
        {
            return b;
        }

        if (double.IsNegativeInfinity(b))
        {
            return a;
        }

        var hi = Math.Max(a, b);
        var gap = Math.Min(a, b) - hi;

        return gap < LogSumFloor ? hi : hi + Math.Log(1 + Math.Exp(gap));
    }
}
