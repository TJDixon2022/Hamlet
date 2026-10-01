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
/// <para>**A HAND'S WIDEST IS THE LIMIT** (unit 513's 0.25 in log-length): a cluster as wide as the
/// widest fist scores nought for tightness and a machine's scores near one, so a fist ranks under a
/// machine sender and noise, whose lengths spread wider than any hand, under both.</para>
/// </remarks>
public sealed record CwSequenceShape(
    double Rectangle, double Dits, double Dahs, double Separation, double ElementGaps, double LetterGaps, double Consistency, double Evidence)
{
    /// <summary>A cluster's spread in log-length at which its tightness reaches nought: a hand's widest (unit 513).</summary>
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
    /// <returns>The shape; <see cref="None"/> where its marks do not split into two lengths.</returns>
    public static CwSequenceShape Of(IReadOnlyList<CwMark> marks, int count)
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

        for (var i = 0; i < 8 && widest < CwRunReader.TwoKindsRatio && dits.Count > 0 && dahs.Count > 0; i++)
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

        if (dits.Count == 0 || dahs.Count == 0 || Centre(dahs) / Centre(dits) < CwRunReader.TwoKindsRatio)
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
        var between = LowestCluster(gaps.Where(g => g >= CwPatternGate.InsideLetterShare * dit).ToList());

        var consistent = lengths.Count(l =>
            Math.Min(Math.Abs(Math.Log(l / dit)), Math.Abs(Math.Log(l / dah))) <= Math.Log(CwPatternGate.LengthRatio));

        return new CwSequenceShape(
            rectangles.Count > 0 ? rectangles.Average() : 1,
            Tightness(dits),
            Tightness(dahs),
            Math.Clamp((Math.Log(dah / dit) - Math.Log(2)) / (Math.Log(3) - Math.Log(2)), 0, 1),
            inside.Count >= 2 ? Tightness(inside) : 1,
            between.Count >= 2 ? Tightness(between) : 1,
            lengths.Count > 0 ? consistent / (double)lengths.Count : 0,
            1 - Math.Exp(-count / EvidenceMarks));
    }

    private static double Centre(IReadOnlyList<double> lengths) => Math.Exp(lengths.Average(l => Math.Log(l)));

    private static double Tightness(IReadOnlyList<double> lengths)
    {
        var logs = lengths.Select(l => Math.Log(l)).ToList();
        var mu = logs.Average();
        var sd = Math.Sqrt(logs.Sum(l => (l - mu) * (l - mu)) / logs.Count);

        return Math.Clamp(1 - (sd / WidestSpread), 0, 1);
    }

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
