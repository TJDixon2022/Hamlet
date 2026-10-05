namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// **The pattern across marks is the gate** (work instruction 507, R112, HM-DEC-210): a candidate mark
/// stands only when it belongs to a sequence with the shape of a keyed tone, and one that fits none is
/// dropped - never handed to the reader, the scope or the mark count.
/// </summary>
/// <remarks>
/// <para>**THE OWNER, R112**: *"You're still focused on dB. We need to be focused on the shapes in the
/// noise. They're predictable. They're full of good patterns. Chaos and noise have no patterns."*</para>
/// <para>**A SEQUENCE IS THE MARKS OF ONE SENDER**: within one bin of its pitch, within the reader's own
/// level tolerance of its height (<see cref="CwSenderGate.LevelToleranceDb"/>, unit 490's figure,
/// restated as a ratio of heights), and near enough in time to be the same sending. Every test is a
/// ratio between marks; none is a decibel figure, so a station 8 dB over the noise satisfies them as
/// well as one 38 dB over.</para>
/// <para>**A MARK THAT SITS ON THE ONE BEFORE IT IS NOT A SECOND MARK OF THIS SENDER.** A key comes up
/// between elements, for at least a dit in Morse and never less than about half of one in a hand's
/// scatter, so a candidate whose gap from the sequence's last mark is under half the sender's dit is the
/// same tone read twice, or a piece of it. Unit 507 measured four real marks called twice at
/// 16 dB, overlapping at one pitch and two levels; they were the letters that read wrong.</para>
/// <para>**A SEQUENCE STANDS AT <see cref="MarksToStand"/> MARKS OF TWO LENGTHS.** Before then its marks
/// are held; when it stands they are handed on in order, and every agreeing mark after them at once.
/// Marks of one length alone are not yet Morse: every letter has a dah or a dit beside its opposite
/// within a few letters, and noise's short bars are all of one kind.</para>
/// </remarks>
internal sealed class CwPatternGate
{
    /// <summary>
    /// **How many agreeing marks a sequence needs before it stands**: five (work instruction 507).
    /// </summary>
    /// <remarks>
    /// **WHAT THE COUNT BUYS IS NOISE'S CHANCE OF MAKING THAT MANY.** Noise hands the detector a bar at a
    /// given bin now and then, at a height of its own; five in a row at one pitch, at one height, none
    /// crowding the one before, and in two lengths at two to one or wider, is the owner's *"noise cannot
    /// make five marks that agree"*. Unit 493's two runs of two marks, four, is the floor, and five is
    /// one past it so the pattern is always wider than a single letter of four. The author's, from what
    /// the count excludes; not tuned after a result.
    /// </remarks>
    public const int MarksToStand = 5;

    /// <summary>
    /// The least gap between two marks of one sender, as a share of the sender's dit: a half (work
    /// instruction 507).
    /// </summary>
    /// <remarks>
    /// Morse puts a dit of silence between elements; a hand scatters that by tens of percent, and the
    /// detector reads gaps long, never short. Half a dit is under any of it and over any two readings of
    /// one tone. The author's.
    /// </remarks>
    public const double LeastGapShare = 0.5;

    /// <summary>
    /// How long a sequence may be silent and still be the same sending, in seconds: the slowest
    /// Farnsworth word gap the ARRL sends and a dah at 5 WPM after it (units 500 and 501).
    /// </summary>
    public const double SilenceSeconds = SlowestFarnsworthWordGapSeconds + (3 * 1.2 / 5);

    /// <summary>
    /// The word gap of the slowest Farnsworth sending the ARRL's code practice uses, 18 WPM letters at
    /// 5 WPM overall, in seconds: 3.66 (work instruction 501; moved here from the reader by work instruction 525).
    /// </summary>
    /// <remarks>
    /// **FROM PARIS, NOT FROM ANY RECORDING.** PARIS is fifty units: thirty-one in its letters and the
    /// gaps inside them, sent at the letter speed, and nineteen in its spaces, four letter gaps of
    /// three and one word gap of seven, stretched so the word takes 60/5 seconds. So one stretched
    /// unit is (60/5 - 31 × 1.2/18) / 19 seconds, and a word gap is seven of them.
    /// </remarks>
    public const double SlowestFarnsworthWordGapSeconds = 7 * ((60.0 / 5) - (31 * 1.2 / 18)) / 19;

    /// <summary>How many gaps of a kind a sender shows before its own cluster of them is used.</summary>
    /// <remarks>Author's: three, so no single gap sets the boundary alone (unit 501).</remarks>
    public const int MeasuredRunGaps = 3;

    /// <summary>
    /// **A MARK'S LEVEL IS JUDGED AGAINST THE SENDER'S LAST LETTER** (work instruction 525, task 3, HM-DEC-229): the
    /// mean of a sequence's last three marks, where it was eight.
    /// </summary>
    /// <remarks>
    /// Eight marks are two seconds at 18 WPM, and a slow fade of 6 dB over four seconds moves a station by up to 4.7 dB
    /// in a second, so the eight's mean sat behind the next mark by more than the level tolerance and the mark began a
    /// sequence of its own. Three is a letter's worth: the average Morse letter is about three elements, half a second
    /// at 18 WPM, across which such a fade moves the mean by about a decibel. The author's, from what a letter is; not
    /// tuned after a result.
    /// </remarks>
    public const int LevelMarks = 3;

    // Gaps inside letters and gaps between them, one and three, part at their geometric mean.
    private static readonly double ElementLetterRatio = Math.Sqrt(3);

    // A letter gap and a word gap are three and seven of one unit, Farnsworth stretched or not (unit 501).
    private static readonly double LetterWordRatio = Math.Sqrt(7.0 / 3);

    // Before a sender has shown letter gaps: three and seven gap dits at their geometric mean (unit 501).
    private static readonly double UnmeasuredWordRatio = Math.Sqrt(21);

    /// <summary>
    /// **THE GATE LABELS EVERY GAP OF A SENDER: ELEMENT, LETTER OR WORD** (work instruction 525, HM-DEC-229): the
    /// lines between the three kinds, from the sender's own dit and gaps; the reader places its letters and spaces
    /// from <see cref="CwGapLines.KindOf"/>. The arithmetic of units 500, 501, 504, 510 and 513 moved here from the
    /// reader unchanged.
    /// </summary>
    /// <param name="ditSeconds">The sender's dit as its marks read it.</param>
    /// <param name="elementGaps">Its gaps inside letters.</param>
    /// <param name="gapOverDit">How much longer each gap inside a letter read than the dit of the marks then.</param>
    /// <param name="runGaps">Its gaps between runs, at its speed now (unit 524).</param>
    /// <returns>The lines, and the letter and word gaps measured.</returns>
    /// <remarks>
    /// <para>**A GAP IS COUNTED IN DITS OF GAP** (unit 500): the detector reads a mark short and the gap after it
    /// long, each by its window's smear, so the unit a gap is counted in is the marks' dit plus the median of what
    /// the sender's gaps inside letters have run over it.</para>
    /// <para>**ELEMENT AGAINST LETTER** (units 500 and 513): gap dits times √3 until the sender has shown its letter
    /// gaps and three gaps inside letters, then the midpoint of those two clusters of its own in log-length, each side
    /// weighed alike (work instruction 531, HM-DEC-235): Morse's own 1:3 puts it at √3. Weighed by their spreads, as the
    /// dit-or-dah line still is, a sender read through its own window had gaps inside letters as tight as the detector
    /// reads, the line was pulled to 1.4 dits, and the 1.6-dit gap inside the owner's 7 read as a letter gap.</para>
    /// <para>**LETTER AGAINST WORD.** Where the sender has shown three word gaps, the boundary between its own letter
    /// and word clusters; before that, the sender's letter gap times √(7/3), its geometric mean with a word gap in a
    /// Farnsworth sender's stretched units (units 501 and 513), and never under √21 of its element gaps. Before any
    /// letter gap is measured, three and seven gap dits at their geometric mean. The five-dit floor of work instruction
    /// 525 came out in work instruction 534 (HM-DEC-238).</para>
    /// </remarks>
    public static CwGapLines GapLines(double ditSeconds, IReadOnlyList<double> elementGaps, IReadOnlyList<double> gapOverDit, IReadOnlyList<double> runGaps)
    {
        var smear = gapOverDit.Count > 0 ? Math.Max(0, Median(gapOverDit)) : 0;
        var gapDit = ditSeconds + smear;
        var line = gapDit * ElementLetterRatio;
        var (letter, word) = GapClusters(runGaps, line);
        var inside = elementGaps.Where(g => g < line).ToList();
        var character = letter is null || inside.Count < MeasuredRunGaps
            ? line
            : Midpoint(inside, letter);
        var letterMean = letter?.Average();
        // **THE SENDER'S OWN WORD CLUSTER, ONCE TRUSTED** (work instruction 529, task 2, HM-DEC-233): from three word gaps the
        // line is the boundary between its own letter and word clusters; before that √(7/3) of its letter centre.
        //
        // **NEVER UNDER MORSE'S OWN MIDPOINT UNTIL THEN** (work instruction 531, task 2, HM-DEC-235): until the word cluster is
        // trusted, the line is never under √21 of the sender's element gaps (their centre once three show, its gap dits
        // before), the midpoint between a letter gap of three and a word gap of seven. In a transmission's first seconds it
        // was drawn from one or two letter gaps and landed low, and the owner's 3.8-dit gap after the F of FER read as a word.
        //
        // **THE FIVE-DIT FLOOR CAME OUT** (work instruction 534, HM-DEC-238): with the standing line and the other rules the
        // owner's recordings measured as worth nothing gone, the scoreboard held without it, and the √21 line already keeps
        // a word line off the letter gaps in a transmission's first seconds.
        var wordLine = letterMean is not { } l ? gapDit * UnmeasuredWordRatio
            : word is { Count: >= MeasuredRunGaps } ? (CwRules.On(CwRules.GapCrossing) ? Crossing(letter!, word) : CwSenderGate.Boundary(CwSenderGate.LogStats(letter!), CwSenderGate.LogStats(word)))
            : Math.Max(CwRules.On(CwRules.ColdStartWordLine) ? (inside.Count >= MeasuredRunGaps ? Centre(inside) : gapDit) * UnmeasuredWordRatio : 0, CwRules.On(CwRules.FiveDitFloor) ? FiveDitLine(l, word is null, ditSeconds, smear) : l * LetterWordRatio);

        return new CwGapLines(gapDit, character, wordLine, inside, letter, word);
    }

    /// <summary>
    /// **A WORD GAP IS FIVE DITS OR MORE** where a sender's gaps above the element gap form only one cluster (work
    /// instruction 525, HM-DEC-229). Removed in work instruction 534, restored behind a switch by work instruction 539.
    /// </summary>
    public const double WordGapDits = 5;

    // The word line from the letter cluster with the five-dit floor, as before work instruction 534: five dits on the true dit,
    // the word line where only the letter cluster shows under it, and never over it where three clusters show.
    private static double FiveDitLine(double letterMean, bool noWords, double ditSeconds, double smear)
    {
        var fiveDits = (WordGapDits * (ditSeconds + (smear / 2))) + (smear / 2);

        return noWords && letterMean < fiveDits ? fiveDits
            : letterMean < fiveDits ? Math.Min(fiveDits, letterMean * LetterWordRatio)
            : letterMean * LetterWordRatio;
    }

    /// <summary>How many gaps either side a gap is judged against: three, a letter's worth (work instruction 526).</summary>
    public const int NeighbourGaps = 3;

    /// <summary>
    /// **A GAP IS JUDGED AGAINST ITS NEIGHBOURS** (work instruction 526, task 2, HM-DEC-230): inside a letter or between
    /// letters, by the gaps around it in the same sender, where they show a clean jump; a word is still decided by the
    /// lines. Removed in work instruction 534, restored behind a switch by work instruction 539.
    /// </summary>
    public static CwGapKind KindAmongNeighbours(IReadOnlyList<double> gaps, int index, CwGapLines lines)
    {
        var gap = gaps[index];
        var kind = lines.KindOf(gap);

        if (kind == CwGapKind.Word)
        {
            return kind;
        }

        var window = new List<double>();

        for (var i = Math.Max(0, index - NeighbourGaps); i <= Math.Min(gaps.Count - 1, index + NeighbourGaps); i++)
        {
            if (lines.KindOf(gaps[i]) != CwGapKind.Word)
            {
                window.Add(gaps[i]);
            }
        }

        window.Sort();

        var at = -1;
        var widest = 0.0;

        for (var i = 2; i < window.Count - 1; i++)
        {
            if (window[i] / window[i - 1] > widest)
            {
                widest = window[i] / window[i - 1];
                at = i;
            }
        }

        if (at < 0 || widest < CwSenderGate.TwoKindsRatio)
        {
            return kind;
        }

        var boundary = Midpoint(window.Take(at).ToList(), window.Skip(at).ToList());

        return gap > boundary ? CwGapKind.Letter : CwGapKind.Element;
    }

    /// <summary>
    /// **A GAP LONGER THAN THREE OF THE SENDER'S WORD GAPS IS A PAUSE** (work instruction 529, task 2, HM-DEC-233), not
    /// counted in its word cluster. Removed in work instruction 534, restored behind a switch by work instruction 539.
    /// </summary>
    public const double PauseWordGaps = 3;

    /// <summary>
    /// A sender's dit from its gaps alone, where its marks show only one length: a gap inside a letter is a dit and a
    /// gap between letters three (unit 500). NaN where it has shown no gap.
    /// </summary>
    public static double DitFromGaps(IReadOnlyList<double> elementGaps, IReadOnlyList<double> runGaps)
        => elementGaps.Count > 0 ? Median(elementGaps)
            : runGaps.Count > 0 ? runGaps.Min() / 3
            : double.NaN;

    private static (List<double>? Letter, List<double>? Word) GapClusters(IReadOnlyList<double> runGaps, double line)
        => LetterAndWordGaps(runGaps.Where(g => !(g < line)).ToList());

    /// <summary>
    /// A sender's letter gaps and word gaps as two clusters, the gaps given being those above its element line; nulls
    /// before it has shown <see cref="MeasuredRunGaps"/>, and a null word cluster where none shows (work instruction 529,
    /// task 2).
    /// </summary>
    /// <param name="gaps">The gaps above the element line, in any order.</param>
    /// <returns>The letter cluster, and the word cluster where one shows.</returns>
    /// <remarks>
    /// <para>**SPLIT.** The gaps are split as before (<see cref="Split"/>). The pause taken out of the word cluster (work
    /// instruction 529) came out in work instruction 534 (HM-DEC-238), when the owner's recordings read as well without
    /// it.</para>
    /// <para>**A WORD CLUSTER IS TRUSTED FROM THREE GAPS** (<see cref="MeasuredRunGaps"/>), and that is decided where the
    /// word line is drawn (<see cref="GapLines"/>): two gaps that agree are not yet a sender's word spacing.</para>
    /// </remarks>
    internal static (List<double>? Letter, List<double>? Word) LetterAndWordGaps(IReadOnlyList<double> gaps)
    {
        var sorted = gaps.OrderBy(g => g).ToList();

        if (sorted.Count < MeasuredRunGaps)
        {
            return (null, null);
        }

        var (letter, word) = Split(sorted);

        // **THE PAUSE, BEHIND A SWITCH** (restored by work instruction 539 to be measured): every gap longer than three of the
        // sender's word gaps is taken out, and the clusters settled again from the first split.
        if (CwRules.On(CwRules.Pause))
        {
            var pause = PauseWordGaps * LetterWordRatio * LetterWordRatio * Centre(letter);
            var kept = sorted.Where(g => g <= pause).ToList();

            if (kept.Count < sorted.Count && kept.Count >= MeasuredRunGaps)
            {
                var words = word.Where(g => g <= pause).ToList();

                (letter, word) = words.Count > 0 ? Settle(letter.Where(g => g <= pause).ToList(), words) : Split(kept);
            }
        }

        return word.Count > 0 ? (letter, word) : (letter, null);
    }

    /// <summary>
    /// The gaps, sorted, as a letter and a word cluster (work instructions 501, 504, 510 and 513; moved here from the reader
    /// by work instruction 525): the word cluster empty where none shows.
    /// </summary>
    /// <remarks>
    /// <para>**A GAP UNDER THE SENDER'S OWN LETTER LINE IS NOT A LETTER GAP** (unit 510, HM-DEC-214): runs closed
    /// before the sender's dit was known can end at a gap inside a letter; what sits under gap dits times √3 measures
    /// neither.</para>
    /// <para>**THE LOWEST CLUSTER OF AT LEAST THREE, THEN THE NEXT** (unit 504): sorted, the gaps are walked up and
    /// split where two neighbors differ by √(7/3), and the letter gaps are the lowest cluster holding
    /// <see cref="MeasuredRunGaps"/> or more, the word gaps the next one up; a pause between two calls is above
    /// them, and one odd gap under them is read against them rather than measuring them.</para>
    /// <para>**THEN SETTLED BY THE NEARER CLUSTER** (unit 513, HM-DEC-217): a fist's letter and word gaps overlap
    /// where a walk finds no clean jump, so the two clusters are settled so each gap is the kind it sits fewer
    /// spreads from.</para>
    /// </remarks>
    private static (List<double> Letter, List<double> Word) Split(List<double> sorted)
    {
        var clusters = new List<List<double>> { new() { sorted[0] } };

        for (var i = 1; i < sorted.Count; i++)
        {
            if (sorted[i] / sorted[i - 1] >= LetterWordRatio)
            {
                clusters.Add(new List<double>());
            }

            clusters[^1].Add(sorted[i]);
        }

        var at = clusters.FindIndex(c => c.Count >= MeasuredRunGaps);

        at = at < 0 ? 0 : at;

        if (at + 1 >= clusters.Count)
        {
            return CwRules.On(CwRules.OverlapSplit) ? SplitOverlap(clusters[at]) : (clusters[at], new List<double>());
        }

        var (letter, word) = Settle(clusters[at], clusters[at + 1]);

        return letter.Count == 0 ? (clusters[at], clusters[at + 1]) : (letter, word);
    }

    /// <summary>
    /// **A HAND'S LETTER AND WORD GAPS OVERLAP, AND ARE STILL TWO KINDS** (work instruction 538, task 2, HM-DEC-242): where the
    /// walk finds no clean jump, the gaps are split in two by log-length 2-means and settled at the crossing (<see
    /// cref="Crossing"/>); they are taken as two kinds only where the two centres sit the walk's own jump, √(7/3), apart.
    /// </summary>
    /// <remarks>
    /// On the owner's 14:40:45 recording the hand's letter gaps run to 709 ms and its word gaps start at 639, no two
    /// neighbours differ by √(7/3), and every gap was one letter cluster with a word line at 958 ms, above every word gap he
    /// sent. A sender's letter gaps alone, split by 2-means, part at about 0.8 of their spread either side of their centre,
    /// a hand's 0.19 in log-length leaving centres 1.36 apart, under the jump.
    /// </remarks>
    private static (List<double> Letter, List<double> Word) SplitOverlap(List<double> sorted)
    {
        var none = (sorted, new List<double>());

        if (sorted.Count < MeasuredRunGaps + 1)
        {
            return none;
        }

        var logs = sorted.Select(g => Math.Log(g)).ToList();
        var lo = logs[0];
        var hi = logs[^1];
        var count = 0;

        for (var i = 0; i < 30; i++)
        {
            var mid = (lo + hi) / 2;
            var next = logs.Count(l => l < mid);

            if (next == 0 || next == logs.Count)
            {
                return none;
            }

            lo = logs.Take(next).Average();
            hi = logs.Skip(next).Average();

            if (next == count)
            {
                break;
            }

            count = next;
        }

        var (letter, word) = Settle(sorted.Take(count).ToList(), sorted.Skip(count).ToList());

        return letter.Count >= MeasuredRunGaps && word.Count > 0 && Centre(word) / Centre(letter) >= LetterWordRatio
            ? (letter, word)
            : none;
    }

    /// <summary>
    /// **THE SENDER'S OWN EQUAL-ERROR LINE** (work instruction 538, task 2, HM-DEC-242): where in log-length a gap is as likely
    /// to be of one cluster as of the other, given each one's centre and spread, the spread floored as the sender's.
    /// </summary>
    /// <remarks>
    /// The boundary it replaces for gaps set a length as many of one cluster's spreads from its centre as of the other's,
    /// and a wide word cluster pulled it down onto the letter gaps: on the owner's 22:15:30 station, letter gaps of 111 to
    /// 171 ms and a word cluster centred at 274 put it at 160 ms, and every letter printed spaced. Where the two densities
    /// cross twice the crossing between the centres is taken; where they do not cross between them, the geometric middle.
    /// </remarks>
    internal static double Crossing(IReadOnlyCollection<double> low, IReadOnlyCollection<double> high)
    {
        var (m1, s1) = CwSenderGate.LogStats(low);
        var (m2, s2) = CwSenderGate.LogStats(high);
        var a = (1 / (s1 * s1)) - (1 / (s2 * s2));
        var b = -2 * ((m1 / (s1 * s1)) - (m2 / (s2 * s2)));
        var c = (m1 * m1 / (s1 * s1)) - (m2 * m2 / (s2 * s2)) + (2 * Math.Log(s1 / s2));
        var middle = (m1 + m2) / 2;
        double? x = null;

        if (Math.Abs(a) < 1e-9)
        {
            x = Math.Abs(b) < 1e-12 ? null : -c / b;
        }
        else if ((b * b) - (4 * a * c) is var d and >= 0)
        {
            var r1 = (-b + Math.Sqrt(d)) / (2 * a);
            var r2 = (-b - Math.Sqrt(d)) / (2 * a);

            x = r1 > m1 && r1 < m2 ? r1 : r2 > m1 && r2 < m2 ? r2 : null;
        }

        return Math.Exp(x is { } v && v > m1 && v < m2 ? v : middle);
    }

    // The centre of some lengths: their geometric mean.
    private static double Centre(IReadOnlyList<double> lengths) => Math.Exp(lengths.Average(l => Math.Log(l)));

    /// <summary>
    /// **THE LINE INSIDE A LETTER SITS AT THE MIDPOINT** (work instruction 531, HM-DEC-235): between a gap inside a letter and
    /// one between letters, the geometric mean of the two clusters' centres, each weighed alike, as Morse's own 1:3 sits at
    /// √3. It supersedes the spread-weighted boundary for these gaps; the dit-or-dah line keeps it.
    /// </summary>
    private static double Midpoint(IReadOnlyList<double> inside, IReadOnlyList<double> letter)
        => Math.Sqrt(Centre(inside) * Centre(letter));

    /// <summary>
    /// Two clusters of gaps settled by the nearer centre (unit 513), each side weighed by its own spread once it has shown
    /// <see cref="MeasuredRunGaps"/> gaps and by none before (work instruction 529): a pause alone above a hand's gaps has no
    /// spread of its own, and weighed by the floor spread it drew the boundary to itself and parted nothing.
    /// </summary>
    private static (List<double> Low, List<double> High) Settle(List<double> low, List<double> high)
    {
        var all = low.Concat(high).OrderBy(g => g).ToList();

        for (var i = 0; i < 8 && low.Count > 0 && high.Count > 0; i++)
        {
            var boundary = low.Count >= MeasuredRunGaps && high.Count >= MeasuredRunGaps
                ? (CwRules.On(CwRules.GapCrossing) ? Crossing(low, high) : CwSenderGate.Boundary(CwSenderGate.LogStats(low), CwSenderGate.LogStats(high)))
                : Math.Sqrt(Centre(low) * Centre(high));
            var next = all.Where(g => g < boundary).ToList();

            if (next.Count == low.Count)
            {
                break;
            }

            low = next;
            high = all.Skip(next.Count).ToList();
        }

        return (low, high);
    }

    private static double Median(IReadOnlyList<double> values)
    {
        var sorted = values.OrderBy(v => v).ToList();

        return sorted.Count == 0 ? double.NaN : sorted[sorted.Count / 2];
    }

    /// <summary>The marks a sequence's height and lengths are read over.</summary>
    private const int RecentMarks = 16;

    private readonly List<Sequence> _sequences = new();

    /// <summary>How many candidates were offered.</summary>
    public int Offered { get; private set; }

    /// <summary>How many stood and were handed on.</summary>
    public int Stood { get; private set; }

    /// <summary>
    /// Offer one candidate: the marks that stand because of it, in order - none while its sequence is
    /// short of standing or it fits none, the held ones and it when its sequence first stands, and it
    /// alone after that.
    /// </summary>
    /// <param name="candidate">The mark the single-mark gates passed.</param>
    /// <returns>The marks that stand now.</returns>
    public IReadOnlyList<CwMark> Offer(CwMark candidate)
    {
        Offered++;

        var home = _sequences
            .Where(s => s.Agrees(candidate))
            .OrderByDescending(s => s.Standing)
            .ThenByDescending(s => s.Count)
            .FirstOrDefault();

        // **NO QUIETER MARK IS HELD FOR A STANDING SENDER** (work instruction 534, HM-DEC-238): a candidate a standing sender's
        // pitch and lengths but quieter than it by up to twice the tolerance was held for the sender's next mark and admitted
        // inside its letter (work instruction 511). The owner's recordings read better without it, and it came out.
        // Restored behind a switch by work instruction 539, to be measured on the new score.
        var quieter = CwRules.On(CwRules.QuieterMarks);

        if (home is null)
        {
            var sender = quieter
                ? _sequences.Where(s => s.Standing && s.TakesQuieter(candidate)).OrderByDescending(s => s.Count).FirstOrDefault()
                : null;

            if (sender is not null)
            {
                // Inside a letter already by the gap before it, it stands now; the first mark of a letter waits for the next.
                if (sender.AdmitNow(candidate) is { } now)
                {
                    Stood++;
                    return new[] { now };
                }

                sender.Hold(candidate);
                return Array.Empty<CwMark>();
            }

            home = new Sequence(++_nextId);
            _sequences.Add(home);
        }

        var admitted = quieter ? home.Resolve(candidate) : null;

        if (admitted is null && home.Crowds(candidate))
        {
            // **THE SAME TONE READ TWICE, OR A PIECE OF IT**: dropped, not begun again elsewhere.
            return Array.Empty<CwMark>();
        }

        var standing = home.Add(candidate);

        if (admitted is not null)
        {
            standing = standing.Prepend(admitted).ToArray();
        }

        Stood += standing.Count;

        return standing;
    }

    /// <summary>
    /// How far under a standing sender's level its own mark may sit inside one of its letters, as a multiple of the level
    /// tolerance: two (work instruction 511, task 2, HM-DEC-215). Removed in work instruction 534, restored behind a switch
    /// by work instruction 539.
    /// </summary>
    public const double QuieterShare = 2;

    /// <summary>
    /// How far a mark's pitch may sit from its sequence's own and agree with it: half a bin either side, one bin's width
    /// around the sequence's pitch (work instruction 532, task 5, HM-DEC-236).
    /// </summary>
    /// <remarks>
    /// A bin either side was right while every mark's pitch was a bin's centre: it took the bin and its two neighbours.
    /// Measured to the hertz, a mark's pitch fills that whole span, twice a bin wide, and noise marks scattered across it
    /// were grouped into sequences that stood: 44 in thirty seconds of loud noise. A keyed station's own marks sit within
    /// a few hertz of one another.
    /// </remarks>
    public const double AgreeHz = CwEnvelopeDetector.BinSpacingHz / 2;

    /// <summary>
    /// How far a gap inside a letter may run, as a share of the sender's dit: two, between Morse's one
    /// dit inside a letter and three between letters, with the detector's smear on the long side (work
    /// instruction 511). The author's, overrulable.
    /// </summary>
    public const double InsideLetterShare = 2;

    /// <summary>How far a quieter mark's length may be from the sender's dit or dah, as a ratio: √2. The author's.</summary>
    public static readonly double LengthRatio = Math.Sqrt(2);

    /// <summary>
    /// The sequences that stand and have had a mark within the hold: which one, its pitch, level, last mark
    /// and shape (work instructions 515 and 519). Reads only; the gate's rules are unchanged.
    /// </summary>
    /// <param name="nowSeconds">The detector's audio clock.</param>
    /// <param name="holdSeconds">The least hold, the longest gap in ordinary sending; each sequence adds its longest mark, and a slower sender its own longest gap.</param>
    /// <returns>One entry per standing sequence, in no order.</returns>
    public IReadOnlyList<StandingSequence> Standing(double nowSeconds, double holdSeconds)
        => _sequences
            .Where(s => s.Standing && nowSeconds - s.LastToSeconds <= s.HoldSeconds(holdSeconds))
            .Select(s => new StandingSequence(s.Id, s.PitchHz, s.LevelDb, s.LastToSeconds, s.Shape))
            .ToList();

    /// <summary>
    /// How many marks within the window the fullest sequence not yet standing holds: what the
    /// light counts toward <see cref="MarksToStand"/> (work instruction 521). Reads only.
    /// </summary>
    /// <param name="nowSeconds">The detector's audio clock.</param>
    /// <param name="windowSeconds">How recent its last mark must be.</param>
    /// <returns>Nought where none is forming.</returns>
    public int Forming(double nowSeconds, double windowSeconds)
        => _sequences
            .Where(s => !s.Standing && nowSeconds - s.LastToSeconds <= windowSeconds)
            .Select(s => s.HeldSince(nowSeconds - windowSeconds))
            .DefaultIfEmpty(0)
            .Max();

    /// <summary>
    /// A standing sequence the gate still keeps, by id, silent within the hold or not: how many marks it has taken, its dit,
    /// its level and its recent marks, for the window that fits it (work instruction 529). Null once it is gone or never stood.
    /// </summary>
    /// <param name="id">The sequence.</param>
    /// <returns>Its count, dit in seconds, pitch and recent marks; or null.</returns>
    public (int Count, double DitSeconds, double PitchHz, double LevelDb, IReadOnlyList<CwMark> Recent)? Sender(int id)
        => _sequences.FirstOrDefault(s => s.Id == id && s.Standing) is { } s
            ? (s.Count, s.DitSeconds, s.PitchHz, s.LevelDb, s.RecentList)
            : null;

    /// <summary>One standing sequence as <see cref="Standing"/> reports it (work instruction 519).</summary>
    /// <param name="Id">Which sequence, the same for as long as it lives.</param>
    /// <param name="PitchHz">The mean of its recent marks' pitch.</param>
    /// <param name="LevelDb">Its recent level; reported, and ranks nothing.</param>
    /// <param name="LastToSeconds">Where its last mark ended.</param>
    /// <param name="Shape">How much it sounds like code.</param>
    public sealed record StandingSequence(int Id, double PitchHz, double LevelDb, double LastToSeconds, CwSequenceShape Shape);

    // The next sequence's id.
    private int _nextId;

    /// <summary>
    /// The dit and dah lengths of the sequences that stand, in seconds: where the rectangle fit looks
    /// (work instruction 516). Reads only; the gate's rules are unchanged.
    /// </summary>
    /// <param name="nowSeconds">The detector's audio clock.</param>
    /// <param name="holdSeconds">The least hold, as for <see cref="Standing"/>.</param>
    /// <returns>One pair per standing sequence whose marks fall into two lengths.</returns>
    public IReadOnlyList<(double DitSeconds, double DahSeconds)> StandingLengths(double nowSeconds, double holdSeconds)
        => _sequences
            .Where(s => s.Standing && nowSeconds - s.LastToSeconds <= s.HoldSeconds(holdSeconds))
            .Select(s => s.Lengths())
            .Where(l => l is not null)
            .Select(l => l!.Value)
            .ToList();

    /// <summary>Forget sequences silent past <see cref="SilenceSeconds"/>.</summary>
    /// <param name="nowSeconds">The detector's audio clock.</param>
    public void Prune(double nowSeconds)
        => _sequences.RemoveAll(s => nowSeconds - s.LastToSeconds > SilenceSeconds);

    /// <summary>One sender's marks.</summary>
    private sealed class Sequence(int id)
    {
        private readonly List<CwMark> _held = new();
        private readonly List<CwMark> _recent = new();

        public int Id { get; } = id;

        /// <summary>How much its recent marks sound like code (work instruction 519, R116).</summary>
        public CwSequenceShape Shape => CwSequenceShape.Of(_recent, Count);

        public bool Standing { get; private set; }

        /// <summary>How many marks it holds while it has not stood (work instruction 521).</summary>
        public int HeldCount => _held.Count;

        /// <summary>How many of the marks it holds ended at or after a time (work instruction 521).</summary>
        public int HeldSince(double seconds)
        {
            var recent = _held.Where(m => m.ToSeconds >= seconds).ToList();

            // A shape is forming only where its recent marks already come in two lengths, a dah beside a dit: noise's
            // short bars agree in ones and twos at every pitch, and are all of one kind.
            return TwoLengths(recent) ? recent.Count : 0;
        }

        public int Count { get; private set; }

        public double LastToSeconds { get; private set; } = double.NegativeInfinity;

        /// <summary>
        /// How long the sequence may be silent and still be keying, given the least hold: the longer of that hold and
        /// its own longest gap between recent marks, and then its longest recent mark (work instruction 515).
        /// </summary>
        /// <param name="holdSeconds">The least hold: the longest gap in ordinary sending.</param>
        /// <returns>Seconds from the last mark's end.</returns>
        /// <remarks>
        /// A mark reaches the gate only once it has ended, so from one mark's end the next is seen a gap and a
        /// mark later: a word gap and a dah at 9 WPM is 1.33 s. A slow sender's own gaps hold it longer.
        /// </remarks>
        public double HoldSeconds(double holdSeconds)
        {
            var longestGap = _recent.Count < 2 ? 0
                : _recent.Zip(_recent.Skip(1), (a, b) => b.FromSeconds - a.ToSeconds).Where(g => g < SilenceSeconds).DefaultIfEmpty(0).Max();

            return Math.Max(holdSeconds, longestGap) + (_recent.Count > 0 ? _recent.Max(r => r.ToSeconds - r.FromSeconds) : 0);
        }

        /// <summary>The sequence's pitch: the mean of its recent marks' (work instruction 515).</summary>
        public double PitchHz => _recent.Count > 0 ? _recent.Average(r => r.PitchHz) : double.NaN;

        /// <summary>The sequence's level: the mean of its last eight marks', the quieter marks it took by its pattern left out (work instruction 515).</summary>
        public double LevelDb => _recent.Where(r => !r.BySendersPattern).TakeLast(8).Select(r => r.LevelDb).DefaultIfEmpty(double.NaN).Average();

        private CwMark? Last => _recent.Count > 0 ? _recent[^1] : null;

        // A quieter mark of this sender's, waiting for its next mark (work instruction 511, task 2; restored by 539).
        private CwMark? _quieter;

        /// <summary>
        /// Whether a candidate that does not agree is this standing sender's quieter mark: within a bin of its pitch, under
        /// its level by more than the tolerance and no more than twice it, within √2 of its dit or its dah, and not sitting on
        /// its last mark (work instruction 511, task 2).
        /// </summary>
        public bool TakesQuieter(CwMark m)
        {
            if (_recent.Count == 0 || m.FromSeconds - LastToSeconds > SilenceSeconds || Crowds(m))
            {
                return false;
            }

            var pitch = _recent.Average(r => r.PitchHz);

            if (Math.Abs(m.PitchHz - pitch) > AgreeHz)
            {
                return false;
            }

            var dit = Dit(m);
            var dahs = _recent.Where(r => r.ToSeconds - r.FromSeconds >= CwSenderGate.TwoKindsRatio * dit).ToList();
            var dits = _recent.Where(r => r.ToSeconds - r.FromSeconds < CwSenderGate.TwoKindsRatio * dit).ToList();
            var length = m.ToSeconds - m.FromSeconds;

            bool Near(double of) => length >= of / LengthRatio && length <= of * LengthRatio;

            var kind = Near(dit) && dits.Count > 0 ? dits
                : dahs.Count > 0 && Near(dahs.Average(r => r.ToSeconds - r.FromSeconds)) ? dahs
                : null;

            if (kind is null)
            {
                return false;
            }

            var level = kind.TakeLast(LevelMarks).Average(r => r.LevelDb);
            var heights = _recent.Select(r => r.OwnContrastDb).Where(double.IsFinite).OrderBy(c => c).ToList();
            var tolerance = CwSenderGate.LevelToleranceDb(heights.Count > 0 ? heights[heights.Count / 2] : double.NaN);
            var under = level - m.LevelDb;
            var wobble = double.IsFinite(m.OwnContrastDb) && m.OwnContrastDb > 0
                ? 20 * Math.Log10(1 + Math.Pow(10, -m.OwnContrastDb / 20))
                : 0;

            return under > tolerance && under - wobble <= QuieterShare * tolerance;
        }

        /// <summary>Take a quieter mark now where the gap from the sender's last mark already places it inside a letter.</summary>
        public CwMark? AdmitNow(CwMark m)
        {
            if (Last is not { } before)
            {
                return null;
            }

            var dit = Dit(m);
            var gap = m.FromSeconds - before.ToSeconds;

            if (gap < LeastGapShare * dit || gap >= InsideLetterShare * dit)
            {
                return null;
            }

            _quieter = null;

            return Take(m with { BySendersPattern = true });
        }

        /// <summary>Hold a quieter mark until the sender's next mark; a later one replaces it.</summary>
        public void Hold(CwMark m) => _quieter = m;

        /// <summary>
        /// The sender's next mark has come: the held quieter mark stands if it sits inside one of the sender's letters.
        /// </summary>
        public CwMark? Resolve(CwMark next)
        {
            if (_quieter is not { } q || !Standing || Last is not { } before)
            {
                _quieter = null;
                return null;
            }

            _quieter = null;

            var dit = Dit(next);
            var gapBefore = q.FromSeconds - before.ToSeconds;
            var gapAfter = next.FromSeconds - q.ToSeconds;

            if (gapBefore < LeastGapShare * dit || gapAfter < LeastGapShare * dit
                || Math.Min(gapBefore, gapAfter) >= InsideLetterShare * dit)
            {
                return null;
            }

            return Take(q with { BySendersPattern = true });
        }

        /// <summary>Take a quieter mark into the sequence as one of its own.</summary>
        private CwMark Take(CwMark admitted)
        {
            _recent.Add(admitted);
            Count++;
            LastToSeconds = Math.Max(LastToSeconds, admitted.ToSeconds);

            if (_recent.Count > RecentMarks)
            {
                _recent.RemoveAt(0);
            }

            return admitted;
        }

        /// <summary>Its recent marks, oldest first (work instruction 529).</summary>
        public IReadOnlyList<CwMark> RecentList => _recent.ToArray();

        /// <summary>Its dit: the mean short mark where its lengths split in two, its shortest recent mark until then (work instruction 529).</summary>
        public double DitSeconds => Lengths()?.DitSeconds ?? _recent.Select(r => r.ToSeconds - r.FromSeconds).DefaultIfEmpty(double.NaN).Min();

        /// <summary>Same pitch within a bin, same height within the reader's level tolerance, and not silent too long.</summary>
        public bool Agrees(CwMark m)
        {
            if (_recent.Count == 0 || m.FromSeconds - LastToSeconds > SilenceSeconds)
            {
                return false;
            }

            var pitch = _recent.Average(r => r.PitchHz);
            var level = _recent.TakeLast(LevelMarks).Average(r => r.LevelDb);
            var heights = _recent.Select(r => r.OwnContrastDb).Where(double.IsFinite).OrderBy(c => c).ToList();
            var height = heights.Count > 0 ? heights[heights.Count / 2] : double.NaN;

            return Math.Abs(m.PitchHz - pitch) <= AgreeHz
                && Math.Abs(m.LevelDb - level) <= CwSenderGate.LevelToleranceDb(height);
        }

        /// <summary>The mean dit and dah of the recent marks where they split in two, or null (work instruction 516).</summary>
        public (double DitSeconds, double DahSeconds)? Lengths()
        {
            var lengths = _recent.Select(r => r.ToSeconds - r.FromSeconds).OrderBy(l => l).ToList();

            for (var i = 1; i < lengths.Count; i++)
            {
                if (lengths[i] / lengths[i - 1] >= CwSenderGate.TwoKindsRatio)
                {
                    return (lengths.Take(i).Average(), lengths.Skip(i).Average());
                }
            }

            return null;
        }

        /// <summary>
        /// Whether the mark is the last one read again, or a piece of it: it begins before the last one ended, or it sits
        /// closer than half the sender's dit and is itself shorter than half a dit.
        /// </summary>
        /// <remarks>
        /// **A FULL MARK AFTER A KEY-UP IS A MARK** (work instruction 535, HM-DEC-239): heavy keying leaves gaps inside a letter
        /// of a quarter to a half of its dit, and the second dit of the F of FER, 94 ms after a 27 ms gap, was dropped as a
        /// second reading of the first, so FER read ENER. A tone read twice overlaps or touches itself, and a piece of a tone
        /// is short; neither is a dit-long tone after the key came up.
        /// </remarks>
        public bool Crowds(CwMark m)
            => Last is { } last
               && ((CwRules.On(CwRules.CrowdsNarrowed) && m.FromSeconds <= last.ToSeconds)
                   || (m.FromSeconds - last.ToSeconds < LeastGapShare * Dit(m) && (!CwRules.On(CwRules.CrowdsNarrowed) || m.ToSeconds - m.FromSeconds < LeastGapShare * Dit(m))));

        /// <summary>
        /// The sender's dit, in seconds: the mean of its short marks once its lengths split in two, and
        /// the shortest of its recent marks and this one until then.
        /// </summary>
        private double Dit(CwMark m)
        {
            var lengths = _recent.Select(r => r.ToSeconds - r.FromSeconds).Append(m.ToSeconds - m.FromSeconds).OrderBy(l => l).ToList();

            for (var i = 1; i < lengths.Count; i++)
            {
                if (lengths[i] / lengths[i - 1] >= CwSenderGate.TwoKindsRatio)
                {
                    return lengths.Take(i).Average();
                }
            }

            return lengths[0];
        }

        /// <summary>Take the mark; the marks that stand because of it.</summary>
        public IReadOnlyList<CwMark> Add(CwMark m)
        {
            _recent.Add(m);
            Count++;
            LastToSeconds = Math.Max(LastToSeconds, m.ToSeconds);

            if (_recent.Count > RecentMarks)
            {
                _recent.RemoveAt(0);
            }

            if (Standing)
            {
                return new[] { m };
            }

            _held.Add(m);

            // **A SEQUENCE STANDS ONLY ON ITS SHAPE** (work instruction 524, HM-DEC-228): a random carrier is flat-topped and
            // sharp-edged and passes every test on one mark; what makes it not Morse - no two lengths, no 1:3:7, no
            // consistency - shows only across marks, in the sequence's shape score. It stands where that score is above
            // nought. **THE 0.2 LINE CAME OUT** (work instruction 534, HM-DEC-238): measured on the owner's recordings it held
            // back real stations, and loud noise still stands nothing without it.
            if (_held.Count < MarksToStand || !TwoLengths(_held)
                || !(CwSequenceShape.Of(_held.Count > RecentMarks ? _held.GetRange(_held.Count - RecentMarks, RecentMarks) : _held, _held.Count).Score is var score && (CwRules.On(CwRules.StandingLine) ? score >= CwShapeLights.GreenScore : score > 0)))
            {
                return Array.Empty<CwMark>();
            }

            Standing = true;

            var released = _held.ToArray();

            _held.Clear();

            return released;
        }

        /// <summary>Whether the lengths split in two at a ratio of <see cref="CwSenderGate.TwoKindsRatio"/> or wider.</summary>
        // Two lengths: a clean jump of two between neighbours. **A HAND'S TWO KINDS CAME OUT** (work instruction 534,
        // HM-DEC-238): the test for overlapping clusters, kept since unit 523, measured on the owner's recordings as worth
        // nothing once the standing line was gone, and the scoreboard rose without it.
        private static bool TwoLengths(IReadOnlyList<CwMark> marks)
        {
            var lengths = marks.Select(m => m.ToSeconds - m.FromSeconds).OrderBy(l => l).ToList();

            for (var i = 1; i < lengths.Count; i++)
            {
                if (lengths[i] / lengths[i - 1] >= CwSenderGate.TwoKindsRatio)
                {
                    return true;
                }
            }

            return CwRules.On(CwRules.HandTwoKinds) && lengths.Count >= 2 * MarksToStand && CwSenderGate.TwoKindsOfAHand(lengths);
        }
    }
}

/// <summary>What kind of gap a gap between two marks of one sender is (work instruction 525, HM-DEC-229).</summary>
internal enum CwGapKind
{
    /// <summary>Inside a letter.</summary>
    Element,

    /// <summary>Between two letters of one word.</summary>
    Letter,

    /// <summary>Between two words.</summary>
    Word,
}

/// <summary>
/// The lines the gate draws between a sender's three kinds of gap, and the gaps it measured them from (work instruction
/// 525, HM-DEC-229).
/// </summary>
/// <param name="GapDitSeconds">One dit of gap: the marks' dit and the detector's smear on a gap (unit 500).</param>
/// <param name="CharacterSeconds">Between a gap inside a letter and one between letters.</param>
/// <param name="WordSeconds">Between a gap between letters and one between words.</param>
/// <param name="InsideGaps">The sender's gaps inside letters, under the element line.</param>
/// <param name="LetterGaps">Its letter gaps as a cluster, or null before it has shown enough of them.</param>
/// <param name="WordGaps">Its word gaps as a cluster, or null where none shows.</param>
internal sealed record CwGapLines(
    double GapDitSeconds,
    double CharacterSeconds,
    double WordSeconds,
    IReadOnlyList<double> InsideGaps,
    IReadOnlyList<double>? LetterGaps,
    IReadOnlyList<double>? WordGaps)
{
    /// <summary>The mean letter gap, or null.</summary>
    public double? LetterSeconds => LetterGaps?.Average();

    /// <summary>The mean of the word gap cluster, or null.</summary>
    public double? WordClusterSeconds => WordGaps?.Average();

    /// <summary>The kind of a gap, from these lines: past the word line a word, past the letter line a letter.</summary>
    /// <param name="gapSeconds">The gap.</param>
    /// <returns>Its kind.</returns>
    public CwGapKind KindOf(double gapSeconds)
        => gapSeconds > WordSeconds ? CwGapKind.Word
            : gapSeconds > CharacterSeconds ? CwGapKind.Letter
            : CwGapKind.Element;
}
