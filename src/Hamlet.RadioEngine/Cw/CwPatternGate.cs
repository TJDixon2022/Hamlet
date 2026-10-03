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
/// level tolerance of its height (<see cref="CwRunReader.LevelToleranceDb"/>, unit 490's figure,
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
    /// Whether a sequence may also stand on a hand's two kinds (work instruction 523): on for the shape-first path,
    /// whose lengths are a rectangle's true ones, and off for the per-bin path, whose short ragged bars make ten noisy
    /// marks look like a hand - noise stood 159 marks in three minutes there where it stood 80.
    /// </summary>
    public bool HandKinds { get; set; }

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

    /// <summary>
    /// **A WORD GAP IS FIVE DITS OR MORE** where a sender's gaps above the element gap form only one cluster (work
    /// instruction 525, HM-DEC-229).
    /// </summary>
    /// <remarks>
    /// Five sits between Morse's three and seven, and it is what fldigi and CW Skimmer use. A hand's three-dit letter
    /// gaps scattered by a sixth reach 3.5 dits and never five, so no space lands inside a callsign; a sender who
    /// pauses four dits between words still runs together, and that is the honest limit of what timing can tell. The
    /// owner chose the literal five over the nearer of three and five, 2026-10-02.
    /// </remarks>
    public const double WordGapDits = 5;

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
    /// reader unchanged, and the five-dit word line is new.
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
    /// <para>**LETTER AGAINST WORD.** Where three clusters show, the sender's letter gap times √(7/3), its geometric
    /// mean with a word gap in a Farnsworth sender's stretched units (units 501 and 513). Where only the letter
    /// cluster shows and it sits under five dits, a word gap is five dits or more (work instruction 525). Five dits
    /// are counted on the true dit: a mark reads short and a gap long by the same smear, so the true dit is the
    /// marks' dit and half of what a gap inside a letter runs over it, and a gap of five dits reads five of those
    /// and half a smear. A Farnsworth sender's letter gaps are twenty dits and more, so the five-dit line would sit
    /// inside them (W1AW at 5 WPM read every letter as a word); there the √(7/3) line stands. Before any letter gap
    /// is measured, three and seven gap dits at their geometric mean.</para>
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
        var fiveDits = (WordGapDits * (ditSeconds + (smear / 2))) + (smear / 2);
        // **FIVE DITS STAYS A FLOOR** (work instruction 526, task 2, HM-DEC-230): where three clusters show and the letter
        // gaps sit under five dits, a gap of five dits is a word whatever the letter cluster says. A hand that drifts from
        // 13 to 18 WPM carries its 13 WPM letter gaps for a while, their √(7/3) line sat over its new 5.8-dit word gaps, and
        // BROWN FOX read as one word.
        //
        // **AND RETIRES WHERE THE SENDER'S OWN WORD CLUSTER IS TRUSTED** (work instruction 529, task 2, HM-DEC-233): from
        // three word gaps, its pauses out, the line is the boundary between its own letter and word clusters; before that the
        // floor and √(7/3) of its letter centre stand. The owner's sender spaced letters to 5.7 dits and words from 7.7, and
        // five dits split KC4ZGP; with the floor gone everywhere, the SKCC straight key's first letters split.
        //
        // **NEVER UNDER MORSE'S OWN MIDPOINT UNTIL THEN** (work instruction 531, task 2, HM-DEC-235): until the word cluster is
        // trusted, the line is never under √21 of the sender's element gaps (their centre once three show, its gap dits
        // before), the midpoint between a letter gap of three and a word
        // gap of seven. In a transmission's first seconds it was drawn from one or two letter gaps and landed low, and the
        // owner's 3.8-dit gap after the F of FER read as a word.
        var wordLine = letterMean is not { } l ? gapDit * UnmeasuredWordRatio
            : word is { Count: >= MeasuredRunGaps } ? CwRunReader.Boundary(CwRunReader.LogStats(letter!), CwRunReader.LogStats(word))
            : Math.Max((inside.Count >= MeasuredRunGaps ? Centre(inside) : gapDit) * UnmeasuredWordRatio, word is null && l < fiveDits ? fiveDits
            : l < fiveDits ? Math.Min(fiveDits, l * LetterWordRatio)
            : l * LetterWordRatio);

        return new CwGapLines(gapDit, character, wordLine, inside, letter, word);
    }

    /// <summary>How many gaps either side a gap is judged against: three, a letter's worth (work instruction 526).</summary>
    public const int NeighbourGaps = 3;

    /// <summary>
    /// **A GAP IS JUDGED AGAINST ITS NEIGHBOURS** (work instruction 526, task 2, HM-DEC-230): inside a letter or between
    /// letters, by the gaps around it in the same sender.
    /// </summary>
    /// <param name="gaps">The sender's gaps in time order.</param>
    /// <param name="index">The gap judged.</param>
    /// <param name="lines">The sender's lines, which decide a word, and decide the rest where the neighbours cannot.</param>
    /// <returns>The gap's kind.</returns>
    /// <remarks>
    /// <para>**THE OWNER, 2026-10-02**: *"Our biggest struggle is in changes of words per minute. Hand keyers are going to
    /// be all over the place."* The sender's clusters are measured over its last forty marks, eight letters, a long
    /// memory for a hand: just after a step from 35 back to 25 WPM a 25 WPM gap inside the J of JUMPS read longer than the
    /// line the 35 WPM gaps had drawn, and the J printed as W and T.</para>
    /// <para>So the gaps within <see cref="NeighbourGaps"/> either side, word gaps left out, are split where two
    /// neighbours in length differ by <see cref="CwRunReader.TwoKindsRatio"/> or more, the clean jump the marks are
    /// split at, and the gap is the kind it sits nearer by the two sides' spreads. Gaps inside a letter and gaps between
    /// letters are one and three dits; a hand scattered so far that no clean jump is left, or a stretch of one kind,
    /// falls back to the sender's own lines. A word is still decided by the lines, five dits staying a floor.</para>
    /// </remarks>
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

        // Each side at least two: one odd gap is not a cluster (unit 504).
        for (var i = 2; i < window.Count - 1; i++)
        {
            if (window[i] / window[i - 1] > widest)
            {
                widest = window[i] / window[i - 1];
                at = i;
            }
        }

        if (at < 0 || widest < CwRunReader.TwoKindsRatio)
        {
            return kind;
        }

        var boundary = Midpoint(window.Take(at).ToList(), window.Skip(at).ToList());

        return gap > boundary ? CwGapKind.Letter : CwGapKind.Element;
    }

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
    /// **A GAP LONGER THAN THREE OF THE SENDER'S WORD GAPS IS A PAUSE** (work instruction 529, task 2, HM-DEC-233): the
    /// sender stopping, not spacing, and it is not counted in its word cluster.
    /// </summary>
    /// <remarks>
    /// The sender's word gap is seven units where its letter gap is three, Farnsworth stretched or not (unit 501), so it
    /// is 7/3 of the centre of the sender's own letter gaps. Nothing in Morse spacing is longer than a word gap, and a
    /// hand stretches one kind of gap by less than a factor of two (unit 513's widest fist, a quarter in log-length either
    /// side); three of them is past anything a hand spaces with. On the owner's recording of 2026-10-02 the 2.3 s silence
    /// before BEST was taken as the sender's only word gap and pulled the word line to 1.6 s.
    /// </remarks>
    public const double PauseWordGaps = 3;

    /// <summary>
    /// A sender's letter gaps and word gaps as two clusters, the gaps given being those above its element line; nulls
    /// before it has shown <see cref="MeasuredRunGaps"/>, and a null word cluster where none shows (work instruction 529,
    /// task 2).
    /// </summary>
    /// <param name="gaps">The gaps above the element line, in any order.</param>
    /// <returns>The letter cluster, and the word cluster where one shows.</returns>
    /// <remarks>
    /// <para>**SPLIT, THEN THE PAUSES OUT, THEN SETTLED AGAIN.** The gaps are split as before (<see cref="Split"/>); the
    /// split's letter centre gives the sender's word gap, and every gap longer than <see cref="PauseWordGaps"/> of those is
    /// a pause, taken out before the two clusters are settled again from the first split.</para>
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
        var pause = PauseWordGaps * LetterWordRatio * LetterWordRatio * Centre(letter);
        var kept = sorted.Where(g => g <= pause).ToList();

        if (kept.Count < sorted.Count && kept.Count >= MeasuredRunGaps)
        {
            // Settled again from the first split, its pauses out: the walk alone finds no jump in a hand whose letter and
            // word gaps run into each other, and the first split already parted them.
            var words = word.Where(g => g <= pause).ToList();

            (letter, word) = words.Count > 0 ? Settle(letter.Where(g => g <= pause).ToList(), words) : Split(kept);
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
            return (clusters[at], new List<double>());
        }

        var (letter, word) = Settle(clusters[at], clusters[at + 1]);

        return letter.Count == 0 ? (clusters[at], clusters[at + 1]) : (letter, word);
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
                ? CwRunReader.Boundary(CwRunReader.LogStats(low), CwRunReader.LogStats(high))
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

        if (home is null)
        {
            // **THE SENDER'S OWN QUIETER MARK, HELD** (work instruction 511, task 2, HM-DEC-215): a
            // candidate at a standing sender's pitch and of its lengths, quieter than it by more than the
            // tolerance and no more than twice it, waits for the sender's next mark to say whether it
            // sits inside one of its letters.
            var sender = _sequences
                .Where(s => s.Standing && s.TakesQuieter(candidate))
                .OrderByDescending(s => s.Count)
                .FirstOrDefault();

            if (sender is not null)
            {
                // Inside a letter already by the gap before it, it stands now; the first mark of a
                // letter waits for the next, since only that gap can place it inside one.
                if (sender.AdmitNow(candidate) is { } now)
                {
                    Stood++;
                    return new[] { now };
                }

                sender.Hold(candidate);
                return Array.Empty<CwMark>();
            }

            home = new Sequence(++_nextId, this);
            _sequences.Add(home);
        }

        var admitted = home.Resolve(candidate);

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
    /// How far under a standing sender's level its own mark may sit inside one of its letters, as a
    /// multiple of the level tolerance: two (work instruction 511, task 2, HM-DEC-215).
    /// </summary>
    /// <remarks>
    /// Unit 510 measured a strong sender's dit 6 dB down inside a letter found by the blind stage and
    /// dropped here, 6 dB being outside the 3 dB tolerance at that contrast, so `SEPTEMBER` read
    /// `SINHSPMBR`. Twice the tolerance, and only for a mark at the sender's pitch, of its lengths,
    /// inside its letter; between letters, between senders and at any other pitch the tolerance
    /// stands.
    /// </remarks>
    public const double QuieterShare = 2;

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
    private sealed class Sequence(int id, CwPatternGate owner)
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
            return TwoLengths(recent, owner.HandKinds) ? recent.Count : 0;
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

            return Math.Abs(m.PitchHz - pitch) <= CwEnvelopeDetector.BinSpacingHz
                && Math.Abs(m.LevelDb - level) <= CwRunReader.LevelToleranceDb(height);
        }

        // A quieter mark of this sender's, waiting for its next mark (work instruction 511, task 2).
        private CwMark? _quieter;

        /// <summary>
        /// Whether a candidate that does not agree is this standing sender's quieter mark: within a bin
        /// of its pitch, under its level by more than the tolerance and no more than twice it, within
        /// √2 of its dit or its dah, and not sitting on its last mark (work instruction 511, task 2).
        /// </summary>
        public bool TakesQuieter(CwMark m)
        {
            if (_recent.Count == 0 || m.FromSeconds - LastToSeconds > SilenceSeconds || Crowds(m))
            {
                return false;
            }

            var pitch = _recent.Average(r => r.PitchHz);

            if (Math.Abs(m.PitchHz - pitch) > CwEnvelopeDetector.BinSpacingHz)
            {
                return false;
            }

            var dit = Dit(m);
            var dahs = _recent.Where(r => r.ToSeconds - r.FromSeconds >= CwRunReader.TwoKindsRatio * dit).ToList();
            var dits = _recent.Where(r => r.ToSeconds - r.FromSeconds < CwRunReader.TwoKindsRatio * dit).ToList();
            var length = m.ToSeconds - m.FromSeconds;

            bool Near(double of) => length >= of / LengthRatio && length <= of * LengthRatio;

            // **AGAINST THE SENDER'S OWN MARKS OF ITS KIND.** The detector reads a short mark a little
            // under a long one, a dit a tenth or two of a decibel under a dah here, so a dit is held to
            // the sender's dits and a dah to its dahs.
            var kind = Near(dit) && dits.Count > 0 ? dits
                : dahs.Count > 0 && Near(dahs.Average(r => r.ToSeconds - r.FromSeconds)) ? dahs
                : null;

            if (kind is null)
            {
                return false;
            }

            var level = kind.TakeLast(LevelMarks).Average(r => r.LevelDb);
            var heights = _recent.Select(r => r.OwnContrastDb).Where(double.IsFinite).OrderBy(c => c).ToList();
            var tolerance = CwRunReader.LevelToleranceDb(heights.Count > 0 ? heights[heights.Count / 2] : double.NaN);
            var under = level - m.LevelDb;

            // **THE QUIETER MARK'S OWN LEVEL WOBBLES AS ANY TONE'S DOES** (work instruction 524, case 2): a tone S dB over
            // its gap reads up to 20·log10(1 + 10^(-S/20)) from its true level (unit 479), and a mark within that of
            // the line cannot be told from it. On the shape-first path a mark's level is the fit's height, near exact:
            // a dit taken 6.02 dB down read 6.012 under the sender's dits against a line of 6, where the per-bin path,
            // its level lifted by the noise in its bin, read it inside.
            var wobble = double.IsFinite(m.OwnContrastDb) && m.OwnContrastDb > 0
                ? 20 * Math.Log10(1 + Math.Pow(10, -m.OwnContrastDb / 20))
                : 0;

            return under > tolerance && under - wobble <= QuieterShare * tolerance;
        }

        /// <summary>
        /// Take a quieter mark now where the gap from the sender's last mark already places it inside a
        /// letter: under two dits and not under half of one. Null where it does not.
        /// </summary>
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
        /// The sender's next mark has come: the held quieter mark stands if it sits inside one of the
        /// sender's letters - a gap under two dits to the mark before it or to this one, and neither gap
        /// under half a dit. Taken into the sequence and returned, or dropped and null.
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

        /// <summary>The mean dit and dah of the recent marks where they split in two, or null (work instruction 516).</summary>
        public (double DitSeconds, double DahSeconds)? Lengths()
        {
            var lengths = _recent.Select(r => r.ToSeconds - r.FromSeconds).OrderBy(l => l).ToList();

            for (var i = 1; i < lengths.Count; i++)
            {
                if (lengths[i] / lengths[i - 1] >= CwRunReader.TwoKindsRatio)
                {
                    return (lengths.Take(i).Average(), lengths.Skip(i).Average());
                }
            }

            return null;
        }

        /// <summary>Whether the mark sits on the last one, closer than half the sender's dit.</summary>
        public bool Crowds(CwMark m)
            => Last is { } last
               && m.FromSeconds - last.ToSeconds < LeastGapShare * Dit(m);

        /// <summary>
        /// The sender's dit, in seconds: the mean of its short marks once its lengths split in two, and
        /// the shortest of its recent marks and this one until then.
        /// </summary>
        private double Dit(CwMark m)
        {
            var lengths = _recent.Select(r => r.ToSeconds - r.FromSeconds).Append(m.ToSeconds - m.FromSeconds).OrderBy(l => l).ToList();

            for (var i = 1; i < lengths.Count; i++)
            {
                if (lengths[i] / lengths[i - 1] >= CwRunReader.TwoKindsRatio)
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
            // consistency - shows only across marks, in the sequence's shape score. It stands at 0.2 or better, the line the
            // light uses: above anything noise has produced (0.173) and under a rough fist's 0.374.
            if (_held.Count < MarksToStand || !TwoLengths(_held, owner.HandKinds)
                || CwSequenceShape.Of(_held.Count > RecentMarks ? _held.GetRange(_held.Count - RecentMarks, RecentMarks) : _held, _held.Count).Score < CwShapeLights.GreenScore)
            {
                return Array.Empty<CwMark>();
            }

            Standing = true;

            var released = _held.ToArray();

            _held.Clear();

            return released;
        }

        /// <summary>Whether the lengths split in two at a ratio of <see cref="CwRunReader.TwoKindsRatio"/> or wider.</summary>
        private static bool TwoLengths(IReadOnlyList<CwMark> marks, bool handKinds)
        {
            var lengths = marks.Select(m => m.ToSeconds - m.FromSeconds).OrderBy(l => l).ToList();

            for (var i = 1; i < lengths.Count; i++)
            {
                if (lengths[i] / lengths[i - 1] >= CwRunReader.TwoKindsRatio)
                {
                    return true;
                }
            }

            // **OR A HAND'S TWO KINDS** (work instruction 523): a fist scattered by a fifth sends dits to 1.2 dits and
            // dahs down to 2.4, and the clean jump of two is gone though the two kinds stand three to one; the reader
            // learned this in unit 513, and the gate takes its test - two clusters by the nearer centre, two to one
            // apart, neither wider than a hand makes.
            // **ON TEN MARKS, NOT FIVE**: two clusters of five lengths always look tight, and noise stood on them - eight
            // marks in thirty seconds where none stood before. A clean jump is evidence on five marks; overlapping
            // clusters need twice that before they are a hand rather than chance.
            return handKinds && lengths.Count >= 2 * MarksToStand && CwRunReader.TwoKindsOfAHand(lengths);
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
