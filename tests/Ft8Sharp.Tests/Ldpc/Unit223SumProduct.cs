namespace Ft8Sharp.Tests.Ldpc;

/// <summary>
/// <b>Unit 223 task 4: an independent soft decoder, to find out by measurement whether the true
/// codeword is reachable from these ratios by anything at all.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>The claim it exists to test, and it is load-bearing for the whole phase.</b> Unit 222 concluded
/// that <em>the information is not in the ratios</em>. It reached that by putting the hard decisions
/// at -21 dB — about 31 errors in 174 — against a correcting power unit 215 measured as reaching zero
/// at 17. <b>Those two numbers are not the same kind of thing.</b> Unit 215's 17 was measured over
/// <em>hard bit flips</em>: equal confidence everywhere, no reliability information at all. A soft
/// decoder is given a magnitude per bit and routinely closes error counts well past its
/// hard-decision limit — that is the entire reason log-likelihood ratios exist rather than bits. So
/// the comparison does not establish what it was read as establishing, and this decoder settles it by
/// measurement.
/// </para>
/// <para>
/// <b>NOT A COPY OF <c>LdpcDecoder</c>, and deliberately different arithmetic at every choice.</b>
/// This is the leg-B pattern this phase used at steps 3, 4 and 5 — a second instrument that shares no
/// arithmetic with the first, so that agreement means something.
/// </para>
/// <list type="bullet">
///   <item><description>
///     <b>The domain.</b> Upstream multiplies hyperbolic tangents. This works in the
///     <b>log domain</b>, through Gallager's <c>φ(x) = -log tanh(x/2)</c>, which is its own inverse:
///     the check update becomes a <em>sum</em> of <c>φ</c> values and a sign count, and no product of
///     tangents is ever formed. Public literature, and the standard alternative formulation of the
///     same sum-product rule.
///   </description></item>
///   <item><description>
///     <b>The sign convention.</b> Upstream and this port carry
///     <c>λ = log(P(1)/P(0))</c>. This decoder works internally in the textbook
///     <c>L = log(P(0)/P(1))</c>, negating on the way in and deciding on the opposite sign, so a
///     convention error in either would show as total disagreement rather than as a small one.
///   </description></item>
///   <item><description>
///     <b>The exclusion.</b> Upstream re-multiplies the other edges. This uses <b>prefix and suffix
///     sums</b>, so a message never has its own term subtracted back out and there is no
///     cancellation anywhere.
///   </description></item>
///   <item><description>
///     <b>The graph.</b> Upstream's decoder reads both <c>Nm</c> and <c>Mn</c> — the two views of the
///     same Tanner graph. This one reads <b>only <c>LdpcNm</c></b> and builds the variable-side
///     incidence itself by counting, so a fault in <c>LdpcMn</c> could not be shared between them.
///   </description></item>
///   <item><description>
///     <b>The precision.</b> Double throughout, and the exact <see cref="Math.Exp"/> and
///     <see cref="Math.Log"/> rather than any rational approximation.
///   </description></item>
///   <item><description>
///     <b>The schedule.</b> Messages are initialised to the channel ratios and the first check update
///     runs before any decision is taken, where upstream decides first and passes messages second.
///   </description></item>
/// </list>
/// <para>
/// <b>NOTHING HERE IS PROPOSED FOR THE LIBRARY.</b> It is an instrument for one question and it is
/// not a decoder Hamlet would ever run.
/// </para>
/// <para>
/// <b>And it takes ratios and nothing else.</b> No message, no codeword, no frequency, no time. A
/// decoder with a truth parameter cannot be shown not to have used it, so the parameter does not
/// exist — the same prohibition every decoding surface in this tree works under.
/// </para>
/// </remarks>
internal static class Unit223SumProduct
{
    /// <summary>
    /// <b>Four times upstream's 25</b>, which is the bound task 4 requires and is also row G's, so
    /// this decoder and the exact-arithmetic row are separated by the algorithm alone.
    /// </summary>
    internal const int DefaultMaxIterations = 100;

    /// <summary>What one decode reached.</summary>
    internal readonly record struct Outcome(int UnsatisfiedChecks, int Iterations, bool RefusedAllZero)
    {
        /// <summary>
        /// Whether the bits form a codeword <b>that is not the all-zero one</b>. Every linear code
        /// has the all-zero word and it satisfies every check, so a decoder that returned it would
        /// hand the gate above a payload of 77 zero bits whose checksum is also zero — a perfect
        /// decode of nothing. Upstream refuses it in those terms and so does this.
        /// </summary>
        internal bool ParitySatisfied => UnsatisfiedChecks == 0 && !RefusedAllZero;
    }

    // The Tanner graph, built once from LdpcNm alone and shared. Immutable after construction, so a
    // decode is still a pure function of its ratios.
    private static readonly int[] EdgeVariable;
    private static readonly int[] CheckEdgeStart;
    private static readonly int[] CheckEdgeCount;
    private static readonly int[] VariableEdgeStart;
    private static readonly int[] VariableEdgeCount;
    private static readonly int[] VariableEdges;

    static Unit223SumProduct()
    {
        var nm = Ft8Tables.LdpcNm;
        var numRows = Ft8Tables.LdpcNumRows;

        CheckEdgeStart = new int[Ft8Tables.LdpcM];
        CheckEdgeCount = new int[Ft8Tables.LdpcM];

        var edges = new List<int>();
        for (var m = 0; m < Ft8Tables.LdpcM; m++)
        {
            CheckEdgeStart[m] = edges.Count;
            CheckEdgeCount[m] = numRows[m];
            for (var i = 0; i < numRows[m]; i++)
            {
                // The one place upstream's 1-based index comes off, and the only one.
                edges.Add(nm[(m * Ft8Tables.LdpcNmRowWidth) + i] - 1);
            }
        }

        EdgeVariable = edges.ToArray();

        // The variable-side incidence, COUNTED rather than read out of LdpcMn. Two tables that agree
        // are evidence; one table read twice is not.
        VariableEdgeCount = new int[Ft8Tables.LdpcN];
        foreach (var n in EdgeVariable)
        {
            VariableEdgeCount[n]++;
        }

        VariableEdgeStart = new int[Ft8Tables.LdpcN];
        var running = 0;
        for (var n = 0; n < Ft8Tables.LdpcN; n++)
        {
            VariableEdgeStart[n] = running;
            running += VariableEdgeCount[n];
        }

        VariableEdges = new int[EdgeVariable.Length];
        var filled = new int[Ft8Tables.LdpcN];
        for (var e = 0; e < EdgeVariable.Length; e++)
        {
            var n = EdgeVariable[e];
            VariableEdges[VariableEdgeStart[n] + filled[n]] = e;
            filled[n]++;
        }
    }

    /// <summary>How many edges the Tanner graph this decoder built has.</summary>
    internal static int EdgeCount => EdgeVariable.Length;

    /// <summary>The degrees this decoder counted, for the shape of the graph to be reported.</summary>
    internal static (int Lowest, int Highest) VariableDegrees =>
        (VariableEdgeCount.Min(), VariableEdgeCount.Max());

    /// <inheritdoc cref="VariableDegrees"/>
    internal static (int Lowest, int Highest) CheckDegrees =>
        (CheckEdgeCount.Min(), CheckEdgeCount.Max());

    /// <summary>
    /// <b>The sum-product algorithm in the log domain, over the ratios and nothing else.</b>
    /// </summary>
    /// <param name="ratios">
    /// 174 log-likelihood ratios in the library's convention: <b>positive means the bit is more
    /// likely one</b>.
    /// </param>
    /// <param name="bits">174 bytes, written in full, each 0 or 1.</param>
    /// <param name="maxIterations">How hard to try.</param>
    internal static Outcome Decode(
        ReadOnlySpan<float> ratios,
        Span<byte> bits,
        int maxIterations = DefaultMaxIterations)
    {
        if (ratios.Length != Ft8Tables.LdpcN || bits.Length != Ft8Tables.LdpcN)
        {
            throw new ArgumentException(
                $"This decoder takes and returns exactly {Ft8Tables.LdpcN} of each.");
        }

        bits.Clear();

        // Into the textbook convention: L = log(P(0)/P(1)), which is the NEGATIVE of the library's.
        var channel = new double[Ft8Tables.LdpcN];
        for (var n = 0; n < Ft8Tables.LdpcN; n++)
        {
            channel[n] = -(double)ratios[n];
        }

        var toCheck = new double[EdgeVariable.Length];
        var toVariable = new double[EdgeVariable.Length];
        for (var e = 0; e < toCheck.Length; e++)
        {
            toCheck[e] = channel[EdgeVariable[e]];
        }

        // Scratch for the prefix and suffix sums, sized to the largest check the graph has.
        var widest = CheckEdgeCount.Max();
        var magnitudes = new double[widest];
        var prefix = new double[widest + 1];
        var suffix = new double[widest + 1];

        var minErrors = Ft8Tables.LdpcM;
        var iterations = 0;
        var refusedAllZero = false;

        for (var iteration = 0; iteration < maxIterations; iteration++)
        {
            iterations = iteration + 1;

            // ---- CHECK NODES, by sums of phi and a parity of signs. No product is ever formed. ----
            for (var m = 0; m < Ft8Tables.LdpcM; m++)
            {
                var start = CheckEdgeStart[m];
                var degree = CheckEdgeCount[m];

                var negatives = 0;
                for (var i = 0; i < degree; i++)
                {
                    var value = toCheck[start + i];
                    if (value < 0.0)
                    {
                        negatives++;
                    }

                    magnitudes[i] = Phi(Math.Abs(value));
                }

                // Prefix and suffix sums, so each message's own term is EXCLUDED rather than
                // subtracted back out. There is no cancellation anywhere in this.
                prefix[0] = 0.0;
                for (var i = 0; i < degree; i++)
                {
                    prefix[i + 1] = prefix[i] + magnitudes[i];
                }

                suffix[degree] = 0.0;
                for (var i = degree - 1; i >= 0; i--)
                {
                    suffix[i] = suffix[i + 1] + magnitudes[i];
                }

                for (var i = 0; i < degree; i++)
                {
                    var others = prefix[i] + suffix[i + 1];
                    var sign = (negatives - (toCheck[start + i] < 0.0 ? 1 : 0)) % 2 == 0 ? 1.0 : -1.0;
                    toVariable[start + i] = sign * Phi(others);
                }
            }

            // ---- THE DECISION, and it is taken on the OPPOSITE sign to the library's. ----
            var ones = 0;
            for (var n = 0; n < Ft8Tables.LdpcN; n++)
            {
                var total = channel[n];
                var at = VariableEdgeStart[n];
                for (var i = 0; i < VariableEdgeCount[n]; i++)
                {
                    total += toVariable[VariableEdges[at + i]];
                }

                // L = log(P(0)/P(1)), so NEGATIVE means one.
                var bit = (byte)(total < 0.0 ? 1 : 0);
                bits[n] = bit;
                ones += bit;
            }

            var errors = Unsatisfied(bits);
            if (errors < minErrors)
            {
                minErrors = errors;
            }

            if (errors == 0)
            {
                refusedAllZero = ones == 0;
                break;
            }

            // ---- VARIABLE NODES, each message leaving out the check it is sent to. ----
            for (var n = 0; n < Ft8Tables.LdpcN; n++)
            {
                var at = VariableEdgeStart[n];
                var degree = VariableEdgeCount[n];
                for (var i = 0; i < degree; i++)
                {
                    var target = VariableEdges[at + i];
                    var total = channel[n];
                    for (var j = 0; j < degree; j++)
                    {
                        if (j != i)
                        {
                            total += toVariable[VariableEdges[at + j]];
                        }
                    }

                    toCheck[target] = total;
                }
            }
        }

        return new Outcome(minErrors, iterations, refusedAllZero);
    }

    /// <summary>
    /// <b>Gallager's <c>φ(x) = -log tanh(x/2) = log((1 + e^-x) / (1 - e^-x))</c>, which is its own
    /// inverse.</b>
    /// </summary>
    /// <remarks>
    /// <b>Three regimes, each written the way that does not lose the answer.</b> Near zero the
    /// denominator cancels catastrophically, so the asymptote <c>log(2/x)</c> is used instead — the
    /// two agree to eleven figures at the crossover. Far out, <c>e^-x</c> underflows and the value is
    /// <c>2e^-x</c> to full precision. In between, the expression is evaluated as written. <b>The
    /// input is clamped away from zero</b>, because <c>φ(0)</c> is infinite and a message of no
    /// information at all is a real thing for a bit whose symbol fell off the end of the waterfall.
    /// </remarks>
    internal static double Phi(double x)
    {
        if (x < 1e-300)
        {
            x = 1e-300;
        }

        if (x < 1e-6)
        {
            return Math.Log(2.0 / x);
        }

        if (x > 40.0)
        {
            return 2.0 * Math.Exp(-x);
        }

        var e = Math.Exp(-x);
        return Math.Log((1.0 + e) / (1.0 - e));
    }

    /// <summary>
    /// How many parity checks a set of hard bits fails, over the graph this decoder built for itself.
    /// </summary>
    internal static int Unsatisfied(ReadOnlySpan<byte> bits)
    {
        var errors = 0;
        for (var m = 0; m < Ft8Tables.LdpcM; m++)
        {
            var start = CheckEdgeStart[m];
            var x = 0;
            for (var i = 0; i < CheckEdgeCount[m]; i++)
            {
                x ^= bits[EdgeVariable[start + i]];
            }

            if (x != 0)
            {
                errors++;
            }
        }

        return errors;
    }
}
