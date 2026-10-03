namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// **HOW MUCH A SEQUENCE OF MARKS SOUNDS LIKE CODE**: one score from the shape of its marks and gaps
/// together, nought to one, with no level and no height in any of it (work instruction 519, R116,
/// HM-DEC-223).
/// </summary>
/// <param name="Rectangle">The mean of its marks' own shape with the height left out (<see cref="CwMarkShape.ShapeOnly"/>), or a fitted mark's fit score.</param>
/// <param name="Dits">How tightly its dits cluster: one less their spread in log-length over a hand's widest.</param>
/// <param name="Dahs">The same for its dahs.</param>
/// <param name="Separation">How far apart the two centres stand: nought at two to one, one at three to one or wider.</param>
/// <param name="ElementGaps">How tightly its gaps inside letters cluster, as for the dits; one where it has shown fewer than two.</param>
/// <param name="LetterGaps">The same for its gaps between letters.</param>
/// <param name="Consistency">The share of its marks within √2 of the nearer of its two centres.</param>
/// <param name="Evidence">How many marks have stood, saturating: one less e to the minus count over ten.</param>
/// <remarks>
/// <para>**THE OWNER, R116**: *"I don't care what the pitch is. You should find the shape in the noise.
/// It's there. It was audible. Let's defocus pitch and emphasize shape."* And: *"I want this to be so
/// much shape that I'm shocked."*</para>
/// <para>**A PRODUCT, AS UNIT 502 CHOSE FOR A MARK.** A keyed tone is all of these at once: flat-topped
/// marks, two lengths that each hold, three to one apart, a dit of silence inside a letter and three
/// between, and every mark one of the two. Something crisp on four and wrong on one is not a keyed
/// tone; a sum would let the four outvote the one, and a product does not. The author's, from what a
/// keyed tone is, not from any result.</para>
/// <para>**A HAND'S WIDEST IS THE MEASURE** (unit 513's 0.25 in log-length, work instruction 520): a cluster
/// as wide as the widest fist scores four-fifths for tightness, a machine's near one, and only past what any hand
/// makes does it fall toward nought at twice that, so a machine ranks over a fist and a fist over noise.</para>
/// </remarks>
public sealed record CwSequenceShape(
    double Rectangle, double Dits, double Dahs, double Separation, double ElementGaps, double LetterGaps, double Consistency, double Evidence)
{
    /// <summary>A hand's widest spread in log-length (unit 513): tightness is four-fifths there and nought at twice it.</summary>
    public const double WidestSpread = 0.25;

    /// <summary>The marks at which the evidence reaches 63%: two sequences' worth of standing, ten.</summary>
    public const double EvidenceMarks = 2 * CwPatternGate.MarksToStand;

    /// <summary>Nothing that splits in two: no score.</summary>
    public static CwSequenceShape None { get; } = new(0, 0, 0, 0, 0, 0, 0, 0);

    /// <summary>The product of all eight: how much it sounds like code, nought to one.</summary>
    public double Score => Rectangle * Dits * Dahs * Separation * ElementGaps * LetterGaps * Consistency * Evidence;

    /// <summary>The eight and the score, for the tests' report.</summary>
    public override string ToString()
        => string.Create(
            System.Globalization.CultureInfo.InvariantCulture,
            $"shape {Score:0.000} (rectangle {Rectangle:0.00}, dits {Dits:0.00}, dahs {Dahs:0.00}, apart {Separation:0.00}, element gaps {ElementGaps:0.00}, letter gaps {LetterGaps:0.00}, consistent {Consistency:0.00}, evidence {Evidence:0.00})");

    /// <summary>
    /// Score some marks of one sender, in time order, and how many it has had standing in all.
    /// </summary>
    /// <param name="marks">Its recent marks, oldest first.</param>
    /// <param name="count">How many marks it has had in all, for the evidence.</param>
    /// <param name="againstAHand">Whether tightness is scored against a hand (work instruction 520), or, false, unit 519's machine scale, kept for the tests' before.</param>
    /// <param name="wordLineSeconds">
    /// The sender's own word line (work instruction 530, HM-DEC-234): its letter gaps are its longer gaps under it, walked
    /// as before; NaN, and all its longer gaps are walked, where no word line has been drawn.
    /// </param>
    /// <returns>The shape; <see cref="None"/> where its marks do not split into two lengths.</returns>
    public static CwSequenceShape Of(IReadOnlyList<CwMark> marks, int count, bool againstAHand = true, double wordLineSeconds = double.NaN)
    {
        var lengths = marks.Select(m => m.ToSeconds - m.FromSeconds).ToList();
        var sorted = lengths.OrderBy(l => l).ToList();
        var at = -1;
        var widest = 0.0;

        for (var i = 1; i < sorted.Count; i++)
        {
            var ratio = sorted[i] / sorted[i - 1];

            if (ratio > widest)
            {
                widest = ratio;
                at = i;
            }
        }

        if (at < 0)
        {
            return None;
        }

        // **A FIST'S LENGTHS OVERLAP** (unit 513): with no clean jump of two between neighbours, the two kinds
        // are settled by the nearer centre in log-length, from the widest jump, and are two kinds where their
        // centres stand two to one apart. How wide each is falls to the tightness, not to this.
        var dits = sorted.Take(at).ToList();
        var dahs = sorted.Skip(at).ToList();

        for (var i = 0; i < 8 && widest < CwSenderGate.TwoKindsRatio && dits.Count > 0 && dahs.Count > 0; i++)
        {
            var line = Math.Sqrt(Centre(dits) * Centre(dahs));
            var next = sorted.Where(l => l < line).ToList();

            if (next.Count == dits.Count)
            {
                break;
            }

            dits = next;
            dahs = sorted.Skip(next.Count).ToList();
        }

        if (dits.Count == 0 || dahs.Count == 0 || Centre(dahs) / Centre(dits) < CwSenderGate.TwoKindsRatio)
        {
            return None;
        }

        var dit = Centre(dits);
        var dah = Centre(dahs);

        var rectangles = marks
            .Select(m => m.Shape is { } shape ? shape.ShapeOnly : m.Fitted ? m.FitScore : double.NaN)
            .Where(double.IsFinite)
            .ToList();

        var gaps = marks.Zip(marks.Skip(1), (a, b) => b.FromSeconds - a.ToSeconds)
            .Where(g => g > 0 && g < CwPatternGate.SilenceSeconds)
            .OrderBy(g => g)
            .ToList();
        var inside = gaps.Where(g => g < CwPatternGate.InsideLetterShare * dit).ToList();
        // **THE SENDER'S OWN LETTER CLUSTER** (work instruction 530, HM-DEC-234): where its word line is drawn, its letter gaps
        // are its longer gaps under that line, walked as before. A hand whose letter gaps run from two dits to 5.7 with no jump
        // to its words read all its longer gaps as one cluster, scored nought for tightness and was let go before its last
        // word.
        var between = LowestCluster(gaps.Where(g => g >= CwPatternGate.InsideLetterShare * dit && !(g >= wordLineSeconds)).ToList());

        var consistent = lengths.Count(l =>
            Math.Min(Math.Abs(Math.Log(l / dit)), Math.Abs(Math.Log(l / dah))) <= Math.Log(CwPatternGate.LengthRatio));

        return new CwSequenceShape(
            rectangles.Count > 0 ? rectangles.Average() : 1,
            Tightness(dits, againstAHand),
            Tightness(dahs, againstAHand),
            Math.Clamp((Math.Log(dah / dit) - Math.Log(2)) / (Math.Log(3) - Math.Log(2)), 0, 1),
            !againstAHand && inside.Count < 2 ? 1 : Tightness(inside, againstAHand),
            !againstAHand && between.Count < 2 ? 1 : Tightness(between, againstAHand),
            lengths.Count > 0 ? consistent / (double)lengths.Count : 0,
            1 - Math.Exp(-count / EvidenceMarks));
    }

    private static double Centre(IReadOnlyList<double> lengths) => Math.Exp(lengths.Average(l => Math.Log(l)));

    private static double Tightness(IReadOnlyList<double> lengths, bool againstAHand)
    {
        var logs = lengths.Select(l => Math.Log(l)).ToList();
        var mu = logs.Count > 0 ? logs.Average() : 0;
        var squares = logs.Sum(l => (l - mu) * (l - mu));

        if (!againstAHand)
        {
            return logs.Count < 2 ? 1 : Math.Clamp(1 - (Math.Sqrt(squares / logs.Count) / WidestSpread), 0, 1);
        }

        // **A FEW LENGTHS SHOW NO TIGHTNESS** (work instruction 520, HM-DEC-224): two lengths at a hand's widest
        // stand in until the cluster shows its own, so a cluster of two that happen to agree, or none at all, is
        // scored as a hand's and not as a machine's.
        var sd = Math.Sqrt((squares + (PriorLengths * WidestSpread * WidestSpread)) / (logs.Count + PriorLengths));

        // **SCORED AGAINST WHAT A HAND DOES**: up to a hand's widest the score falls gently, to four-fifths at the
        // widest fist unit 513 measured, so four such terms leave a widest fist about two-fifths of a machine's
        // score - under a machine, and a sender. Past it, toward nought at twice a hand's widest, which no hand
        // makes and where unit 513's two speeds mixed in one cluster sit (0.45).
        var x = sd / WidestSpread;

        return x <= 1 ? 1 - (0.2 * x * x) : 0.8 * Math.Pow(Math.Max(0, 2 - x), 2);
    }

    /// <summary>The lengths at a hand's widest that stand in for a cluster until it shows its own: two (work instruction 520). The author's.</summary>
    public const double PriorLengths = 2;

    // The gaps between letters are the lowest cluster of the longer gaps, walked up and split where two
    // neighbours differ by √(7/3), half of three to seven in log-length, as the reader finds them (unit 501).
    private static List<double> LowestCluster(List<double> sorted)
    {
        var cluster = new List<double>();

        foreach (var g in sorted)
        {
            if (cluster.Count > 0 && g / cluster[^1] >= Math.Sqrt(7.0 / 3))
            {
                break;
            }

            cluster.Add(g);
        }

        return cluster;
    }
}
