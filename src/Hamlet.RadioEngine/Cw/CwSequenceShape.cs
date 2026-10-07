namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// **HOW MUCH A SEQUENCE OF MARKS SOUNDS LIKE CODE**: one score from the shape of its marks and gaps
/// together, nought to one, with no level and no height in any of it (work instruction 519, R116,
/// HM-DEC-223; rebuilt by work instruction 550, HM-DEC-254).
/// </summary>
/// <param name="Rectangle">The mean of its marks' own shape with the height left out (<see cref="CwMarkShape.ShapeOnly"/>), or a fitted mark's fit score. Reported; not in the score.</param>
/// <param name="Dits">How tightly its dits cluster, scored against a hand's widest.</param>
/// <param name="Dahs">The same for its dahs.</param>
/// <param name="Separation">How far apart the two centres stand: nought at two to one, one at three to one or wider. Reported; not in the score.</param>
/// <param name="Consistency">The share of its marks within √2 of the nearer of its two centres.</param>
/// <param name="Evidence">How many marks have stood, saturating: one less e to the minus count over ten. Reported; not in the score.</param>
/// <remarks>
/// <para>**THE OWNER, R116**: *"I don't care what the pitch is. You should find the shape in the noise.
/// It's there. It was audible. Let's defocus pitch and emphasize shape."* And, 2026-10-07: *"I'm thinking our shape
/// isn't good enough."*</para>
/// <para>**THE SCORE RANKS PERFECT KEYING FIRST AND JUNK LAST** (work instruction 550, HM-DEC-254): each kind tight,
/// every mark one of the two kinds, and the gaps inside letters tight, the four of CW's own terms that ranked real
/// keying above junk when each was measured on W1AW, the owner's hands, synthetic calls and junk. W1AW scores 0.94,
/// junk's median 0.11. **THREE TERMS CAME OUT OF IT AND ARE STILL REPORTED**: the rectangle ranked junk above real
/// keying (a keyed carrier's edges are sharp and the filter and AGC round W1AW's, junk 0.88 to W1AW's 0.73), and the
/// separation and the evidence did not rank (junk 1.00 and 0.93, real hands 0.89 and 0.92).</para>
/// <para>**THE GAPS INSIDE LETTERS ONLY** (task 2): the old gap terms came out because a hand spaces its letters
/// unevenly (work instruction 534, HM-DEC-238). The gaps under √3 dits are the ones a key makes between the elements of
/// one letter, where a hand is as tight as with its dits; pauses between letters, words and sections are not scored.</para>
/// <para>**A PRODUCT, AS UNIT 502 CHOSE FOR A MARK.** Something crisp on three and wrong on one is not a keyed tone; a
/// sum would let the three outvote the one, and a product does not.</para>
/// <para>**A HAND'S WIDEST IS THE MEASURE** (unit 513's 0.25 in log-length, work instruction 520): a cluster
/// as wide as the widest fist scores four-fifths for tightness, a machine's near one, and only past what any hand
/// makes does it fall toward nought at twice that, so a machine ranks over a fist and a fist over noise.</para>
/// </remarks>
public sealed record CwSequenceShape(
    double Rectangle, double Dits, double Dahs, double Separation, double Consistency, double Evidence)
{
    /// <summary>A hand's widest spread in log-length (unit 513): tightness is four-fifths there and nought at twice it.</summary>
    public const double WidestSpread = 0.25;

    /// <summary>The marks at which the evidence reaches 63%: two sequences' worth of standing, ten.</summary>
    public const double EvidenceMarks = 2 * CwPatternGate.MarksToStand;

    /// <summary>Nothing that splits in two: no score.</summary>
    public static CwSequenceShape None { get; } = new(0, 0, 0, 0, 0, 0) { InsideGaps = 0, KeyDownShare = 1 };

    /// <summary>How much it sounds like code, nought to one: its dits, its dahs and its gaps inside letters each tight, and every mark one of its two kinds (work instruction 550).</summary>
    public double Score => Dits * Dahs * InsideGaps * Consistency * KeyDown;

    /// <summary>
    /// **A CARRIER KEYS DOWN TOO MUCH TO BE MORSE** (work instruction 552, task 1): one while the key is down no more of its
    /// sending than Morse ever keeps it, falling straight to nought at a key that never comes up. Morse's ceiling is the letter
    /// `0` sent over and over, five dahs and their gaps and a letter gap: 15 units down of 22, 68%; ordinary text sits well under
    /// it. Judged over the same recent marks as the other terms, its pauses past a word gap left out.
    /// </summary>
    public double KeyDown => double.IsFinite(KeyDownShare) ? Math.Clamp((1 - KeyDownShare) / (1 - MorseKeyDownCeiling), 0, 1) : 1;

    /// <summary>The most of its sending Morse keeps the key down: 15 of 22, a run of zeros (work instruction 552).</summary>
    public const double MorseKeyDownCeiling = 15.0 / 22;

    /// <summary>The terms and the score, for the tests' report.</summary>
    public override string ToString()
        => string.Create(
            System.Globalization.CultureInfo.InvariantCulture,
            $"shape {Score:0.000} (dits {Dits:0.00}, dahs {Dahs:0.00}, gaps inside letters {InsideGaps:0.00}, consistent {Consistency:0.00}; not scored: rectangle {Rectangle:0.00}, apart {Separation:0.00}, evidence {Evidence:0.00})");

    /// <summary>
    /// Score some marks of one sender, in time order, and how many it has had standing in all.
    /// </summary>
    /// <param name="marks">Its recent marks, oldest first.</param>
    /// <param name="count">How many marks it has had in all, for the evidence.</param>
    /// <param name="againstAHand">Whether tightness is scored against a hand (work instruction 520), or, false, unit 519's machine scale, kept for the tests' before.</param>
    /// <returns>The shape; <see cref="None"/> where its marks do not split into two lengths.</returns>
    public static CwSequenceShape Of(IReadOnlyList<CwMark> marks, int count, bool againstAHand = true)
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

        var consistent = lengths.Count(l =>
            Math.Min(Math.Abs(Math.Log(l / dit)), Math.Abs(Math.Log(l / dah))) <= Math.Log(CwPatternGate.LengthRatio));

        // **THE GAPS, MEASURED BESIDE THE SCORE** (work instruction 550, task 1): the gaps between consecutive marks, those
        // under √3 dits taken as gaps inside a letter, the rest as gaps between letters, words or sections.
        var ordered = marks.OrderBy(m => m.FromSeconds).ToList();
        var gaps = ordered.Zip(ordered.Skip(1), (a, b) => b.FromSeconds - a.ToSeconds).Where(g => g > 0).ToList();
        var inside = gaps.Where(g => g < Math.Sqrt(3) * dit).ToList();
        var inKinds = gaps.Count(g => Math.Abs(Math.Log(g / dit)) <= Math.Log(CwPatternGate.LengthRatio) || g >= 2 * dit);

        // **HOW MUCH OF ITS SENDING THE KEY IS DOWN** (work instruction 552, task 1): its marks over its marks and the gaps between
        // them up to a word gap, seven dits, and its pauses past that left out.
        var sending = gaps.Where(g => g <= WordGapDits * dit).Sum();
        var down = ordered.Sum(m => m.ToSeconds - m.FromSeconds);

        return new CwSequenceShape(
            rectangles.Count > 0 ? rectangles.Average() : 1,
            Tightness(dits, againstAHand),
            Tightness(dahs, againstAHand),
            Math.Clamp((Math.Log(dah / dit) - Math.Log(2)) / (Math.Log(3) - Math.Log(2)), 0, 1),
            lengths.Count > 0 ? consistent / (double)lengths.Count : 0,
            1 - Math.Exp(-count / EvidenceMarks))
        {
            Ratio = dah / dit,
            InsideGaps = Tightness(inside, againstAHand),
            InsideGapRatio = inside.Count > 0 ? Centre(inside) / dit : double.NaN,
            GapsInKinds = gaps.Count > 0 ? inKinds / (double)gaps.Count : double.NaN,
            KeyDownShare = down + sending > 0 ? down / (down + sending) : double.NaN,
            LevelSpreadDb = ordered.Count > 1 ? Math.Sqrt(ordered.Average(m => Math.Pow(m.LevelDb - ordered.Average(o => o.LevelDb), 2))) : double.NaN,
            LevelStepDb = ordered.Count > 1 ? Median(ordered.Zip(ordered.Skip(1), (a, b) => Math.Abs(b.LevelDb - a.LevelDb)).ToList()) : double.NaN,
            ContrastDb = Median(ordered.Select(m => m.ContrastDb).Where(double.IsFinite).ToList()),
        };
    }

    /// <summary>The dah over the dit, centre to centre (work instruction 550, task 1).</summary>
    public double Ratio { get; init; } = double.NaN;

    /// <summary>How tightly its gaps inside letters, those under √3 dits, cluster, scored as the marks' tightness is: in the score (work instruction 550).</summary>
    public double InsideGaps { get; init; } = double.NaN;

    /// <summary>The centre of its gaps inside letters over its dit: one for CW (work instruction 550, task 1).</summary>
    public double InsideGapRatio { get; init; } = double.NaN;

    /// <summary>The spread of its marks' levels, in dB (work instruction 550, task 1).</summary>
    public double LevelSpreadDb { get; init; } = double.NaN;

    /// <summary>The median step in level from one mark to the next, in dB (work instruction 550, task 1).</summary>
    public double LevelStepDb { get; init; } = double.NaN;

    /// <summary>The median of its marks' contrast over their gaps, in dB (work instruction 550, task 1).</summary>
    public double ContrastDb { get; init; } = double.NaN;

    /// <summary>The share of its gaps within √2 of a dit or at least two dits: CW's kinds (work instruction 550, task 1).</summary>
    public double GapsInKinds { get; init; } = double.NaN;

    /// <summary>The share of its sending the key is down: its marks over its marks and the gaps between them up to a word gap (work instruction 552).</summary>
    public double KeyDownShare { get; init; } = double.NaN;

    /// <summary>A word gap, in dits: seven. A gap longer is a pause, not sending.</summary>
    public const double WordGapDits = 7;

    private static double Median(IReadOnlyList<double> values)
        => values.Count == 0 ? double.NaN : values.Order().ElementAt(values.Count / 2);

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
}
