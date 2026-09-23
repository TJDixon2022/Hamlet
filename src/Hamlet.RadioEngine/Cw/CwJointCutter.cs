namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// One element of the stream the first pass produced: a mark or a gap, with the
/// hops it spans.
/// </summary>
/// <param name="IsMark">True for key down.</param>
/// <param name="StartHop">Where it began.</param>
/// <param name="EndHop">Where it ended.</param>
public readonly record struct CwElement(bool IsMark, int StartHop, int EndHop)
{
    /// <summary>What frequency this element itself was sent at, in hertz.</summary>
    /// <remarks>
    /// <para>**NOT SET BY THE DECODE PATH, BECAUSE THE DECODE PATH HAS NO
    /// AUDIO.** Everything above works from the envelope, which was mixed down at
    /// one pitch before any of it ran, so the pitch of an individual element is
    /// not a thing the dynamic program could know. It is filled in afterwards by
    /// <see cref="CwElementPitch"/> from the samples the element spans, and until
    /// then it is <see cref="double.NaN"/>.</para>
    /// <para>**NaN IS NOBODY MEASURED AND IS NOT A PITCH OF NOUGHT** (§0.0). It
    /// is also what a `default(CwElement)` carries, and a reader that treats the
    /// two the same is right to: neither was measured.</para>
    /// <para>**NOTHING IN THE DECODE READS IT.** It reaches the record and the
    /// stream separation and stops there.</para>
    /// </remarks>
    public double PitchHz { get; init; } = double.NaN;

    /// <summary>How many hops it spans.</summary>
    public int Hops => EndHop - StartHop;
}

/// <summary>
/// Where the characters are cut, decided jointly with what the characters are.
/// </summary>
/// <remarks>
/// <para>**THE LETTERS WERE ALREADY RIGHT AND THE CUTS WERE WRONG.** Measured
/// across the corpus on 2026-08-26: `USEDTOUSEAFIRM`, `OUTOFALT`, `TTHINKING`,
/// `FLENX`, `AB OVE`, `BREE Z E` — every one of them correct elements divided in
/// the wrong places. The element-to-character decision was the only stage of the
/// decode path never revisited since the corpus began.</para>
/// <para>**WHY A PER-GAP THRESHOLD CANNOT BE MADE TO WORK.** It fails at both
/// ends of the range and for opposite reasons. On `cw-2026-08-25-021410` the
/// three gap classes measure 53, 221 and 913 milliseconds — 0.81, 3.36 and 13.9
/// units, with wide dead zones between them, perfectly separable — and the old
/// cutter still split a `W` into `A T E`. On `cw-2026-08-25-013637` at thirty
/// words a minute the element and character gaps sit **four milliseconds apart**,
/// 24 against 28, where no threshold can work even in principle.</para>
/// <para>**WHAT REPLACES IT.** A gap is not classified on its own length. A span
/// of marks is proposed as a character, and then the character's own pattern says
/// what every length inside it should have been: which marks were dits and which
/// were dahs, that the gaps between them were single units, and that the gap
/// closing it was three units or seven. The reading that explains all of those
/// lengths at once wins. **A `W` is `.--` and its two dahs are three units each;
/// `A T E` needs those same two marks to be a dah, then a dah, with a
/// three-unit gap in between that is not there.** That is the argument the old
/// cutter had no way to make.</para>
/// <para>**THE §0.0 GUARD IS THE FIRST CONSTRAINT AND NOT THE LAST.** No language
/// model, no letter-frequency prior, no dictionary, no word list. The only
/// knowledge here is the Morse table and the fitted clock. A decoder carrying an
/// English prior invents plausible words out of marginal audio, which is exactly
/// the confident lie this application exists to prevent — so the one term that is
/// not a duration fit is a flat bonus for being a character at all, it is small
/// against the timing terms, and a span no letter explains emits a block rather
/// than the least-bad letter.</para>
/// </remarks>
public static class CwJointCutter
{
    /// <summary>
    /// How much a span is worth for being a real character rather than a block.
    /// </summary>
    /// <remarks>
    /// <para>**SMALL AGAINST THE TIMING TERMS, DELIBERATELY** (§0.0). A length
    /// penalty of one is a mark about a third off its expected length, and this
    /// sits well under that: it decides between a letter and a block where the
    /// timing evidence is near enough level, and it cannot rescue a letter whose
    /// lengths are wrong.</para>
    /// <para>The order this was built under forbids raising it to reach an
    /// acceptance line. If the timing terms cannot divide a stream, that is the
    /// finding.</para>
    /// </remarks>
    public const double ValidityBonus = 0.35;

    /// <summary>The longest run of marks considered as one character.</summary>
    /// <remarks>
    /// Eight, which covers every entry in the table including the error prosign.
    /// The cost is linear in this and the streams are short.
    /// </remarks>
    public const int LongestPattern = 8;

    /// <summary>The scatter a length may show, as a share of the log ratio.</summary>
    /// <remarks>
    /// **THE SAME NUMBER THE FIRST PASS USES**, deliberately: two stages scoring
    /// the same quantity on two scales cannot be compared, and the second pass is
    /// re-deciding what the first proposed rather than measuring something else.
    /// </remarks>
    public const double LengthToleranceShare = 0.35;

    /// <summary>What one character came out as.</summary>
    /// <param name="Text">The letter, or a block where none explained the span.</param>
    /// <param name="Pattern">The marks it was read from.</param>
    /// <param name="EndHop">Where the last mark ended.</param>
    /// <param name="FirstMark">Index of its first mark in the stream.</param>
    /// <param name="MarkCount">How many marks it spans.</param>
    /// <param name="EndsWord">True where the gap closing it was read as a word gap.</param>
    /// <param name="Margin">
    /// How much better this reading was than the best reading of the **same
    /// span** as a **different character**. See <see cref="Cut"/>.
    /// </param>
    public readonly record struct Cutting(
        string Text,
        string Pattern,
        int EndHop,
        int FirstMark,
        int MarkCount,
        bool EndsWord,
        double Margin);

    private static double Penalty(int hops, double want)
    {
        var off = Math.Log(Math.Max(hops, 1e-9) / Math.Max(want, 1e-9))
            / LengthToleranceShare;

        return 0.5 * off * off;
    }

    /// <summary>
    /// Cut a stream of elements into characters, deciding the cuts and the
    /// characters together.
    /// </summary>
    /// <param name="elements">The marks and gaps the first pass produced, in order.</param>
    /// <param name="unitHops">The fitted clock, in hops per unit.</param>
    /// <param name="gapHops">
    /// This sender's own three gap lengths in hops — inside a character, between
    /// characters, between words — or null where they were not measured.
    /// </param>
    /// <returns>The characters, in order.</returns>
    /// <remarks>
    /// <para>**THE MARGIN THAT FALLS OUT OF THIS IS THE ONE WORTH HAVING.** The
    /// margin the first pass records is a difference between two paths that are
    /// free to re-segment, so there is always a trivially different alternative
    /// arriving at the same hop and the number separates nothing — measured on
    /// 2026-08-26 at 0.1 to 3.4 on right answers and 0.0 to 1.9 on wrong ones.
    /// Here second-best is **the same span read as a different character**, which
    /// is the comparison a reader actually cares about.</para>
    /// <para>Nothing here retracts settled text (§0.0): the caller emits with the
    /// pipeline's existing lag and this only decides what is emitted.</para>
    /// </remarks>
    public static IReadOnlyList<Cutting> Cut(
        IReadOnlyList<CwElement> elements,
        double unitHops,
        IReadOnlyList<double>? gapHops = null)
    {
        ArgumentNullException.ThrowIfNull(elements);

        // **THIS SENDER'S OWN GAPS WHERE THEY WERE MEASURED, AND MULTIPLES OF
        // THE DIT ONLY WHERE THEY WERE NOT** (HM-DEC-115). Real operators send
        // Farnsworth: on a traffic net the character gap runs six times the
        // element gap rather than three, and the word gap is nowhere near seven
        // dits. Scoring a closing gap against 3u and 7u on that audio calls every
        // word gap a character gap, which is exactly what the first build of this
        // did — every space on `cw-2026-08-25-013637` disappeared.
        //
        // The first pass already fits these three classes and steers by them; the
        // second pass has to read the same numbers or the two stages disagree
        // about where the words are (HM-DEC-091: one source).
        var insideWant = gapHops is { Count: 3 } ? gapHops[0] : unitHops;
        var characterWant = gapHops is { Count: 3 } ? gapHops[1] : 3 * unitHops;
        var wordWant = gapHops is { Count: 3 } ? gapHops[2] : 7 * unitHops;

        // Marks in order, and the gap that follows each of them. The gap after
        // the last mark may be absent, which is the end of the stream.
        var marks = new List<CwElement>();
        var gapAfter = new List<int>();

        for (var i = 0; i < elements.Count; i++)
        {
            if (!elements[i].IsMark)
            {
                continue;
            }

            marks.Add(elements[i]);
            gapAfter.Add(
                i + 1 < elements.Count && !elements[i + 1].IsMark
                    ? elements[i + 1].Hops
                    : -1);
        }

        var n = marks.Count;

        if (n == 0 || unitHops <= 0)
        {
            return Array.Empty<Cutting>();
        }

        // best[m] is the score of the best reading of marks 0..m-1 as whole
        // characters. from[m] says where the last character began.
        var best = new double[n + 1];
        var from = new int[n + 1];
        var what = new Cutting[n + 1];

        Array.Fill(best, double.NegativeInfinity);
        Array.Fill(from, -1);
        best[0] = 0;

        for (var end = 1; end <= n; end++)
        {
            var longest = Math.Min(LongestPattern, end);

            for (var take = 1; take <= longest; take++)
            {
                var start = end - take;

                if (double.IsNegativeInfinity(best[start]))
                {
                    continue;
                }

                // The gaps inside the character are single units, whatever the
                // character turns out to be, so they are scored once.
                var inside = 0.0;

                for (var m = start; m < end - 1; m++)
                {
                    inside += Penalty(gapAfter[m], insideWant);
                }

                // The gap closing it is three units or seven, and which one is
                // part of the same decision rather than a separate threshold.
                var closing = gapAfter[end - 1];
                var endsWord = false;
                var closingCost = 0.0;

                if (closing >= 0)
                {
                    var asCharacter = Penalty(closing, characterWant);
                    var asWord = Penalty(closing, wordWant);

                    endsWord = asWord < asCharacter;
                    closingCost = Math.Min(asCharacter, asWord);
                }

                // **THE CHARACTER'S OWN PATTERN SAYS WHAT ITS MARKS SHOULD HAVE
                // BEEN**, which is the whole of the idea. Every entry in the
                // table with this many elements is tried, and the one whose dits
                // and dahs explain these lengths best wins.
                var bestText = "";
                var bestPattern = "";
                var bestMarks = double.PositiveInfinity;
                var secondMarks = double.PositiveInfinity;

                foreach (var (pattern, text) in MorseAlphabet.All)
                {
                    if (pattern.Length != take)
                    {
                        continue;
                    }

                    var cost = 0.0;

                    for (var m = 0; m < take; m++)
                    {
                        cost += Penalty(
                            marks[start + m].Hops,
                            (pattern[m] == '-' ? 3 : 1) * unitHops);
                    }

                    if (cost < bestMarks)
                    {
                        secondMarks = bestMarks;
                        bestMarks = cost;
                        bestText = text;
                        bestPattern = pattern;
                    }
                    else if (cost < secondMarks)
                    {
                        secondMarks = cost;
                    }
                }

                // **AND A SPAN NO LETTER EXPLAINS EMITS A BLOCK.** The block is
                // scored on the same timing terms with the mark classes taken at
                // whichever of one or three unit fits each mark, and without the
                // bonus — so it loses to a letter that fits and beats one that
                // does not (§0.0).
                var blockMarks = 0.0;

                for (var m = start; m < end; m++)
                {
                    blockMarks += Math.Min(
                        Penalty(marks[m].Hops, unitHops),
                        Penalty(marks[m].Hops, 3 * unitHops));
                }

                var letterScore = bestPattern.Length == take
                    ? -(bestMarks + inside + closingCost) + ValidityBonus
                    : double.NegativeInfinity;

                var blockScore = -(blockMarks + inside + closingCost);

                var takeLetter = letterScore >= blockScore;
                var score = best[start] + (takeLetter ? letterScore : blockScore);

                if (score <= best[end])
                {
                    continue;
                }

                // The constrained margin: the same span, the same boundaries,
                // read as the next-best character. Infinite where no second
                // reading of this length exists at all, which is honest — there
                // was nothing to be confused with.
                var margin = takeLetter && !double.IsPositiveInfinity(secondMarks)
                    ? secondMarks - bestMarks
                    : double.NaN;

                best[end] = score;
                from[end] = start;

                what[end] = new Cutting(
                    takeLetter ? bestText : "■",
                    takeLetter ? bestPattern : new string('?', take),
                    marks[end - 1].EndHop,
                    start,
                    take,
                    endsWord,
                    margin);
            }
        }

        var read = new List<Cutting>();
        var at = n;

        while (at > 0 && from[at] >= 0)
        {
            read.Add(what[at]);
            at = from[at];
        }

        read.Reverse();

        return read;
    }
}
