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
    /// gaps and three gaps inside letters, then the boundary between those two clusters of its own.</para>
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
            : CwRunReader.Boundary(CwRunReader.LogStats(inside), CwRunReader.LogStats(letter));
        var letterMean = letter?.Average();
        var fiveDits = (WordGapDits * (ditSeconds + (smear / 2))) + (smear / 2);
        var wordLine = letterMean is not { } l ? gapDit * UnmeasuredWordRatio
            : word is null && l < fiveDits ? fiveDits
            : l * LetterWordRatio;

        return new CwGapLines(gapDit, character, wordLine, inside, letter, word);
    }

    /// <summary>
    /// A sender's dit from its gaps alone, where its marks show only one length: a gap inside a letter is a dit and a
    /// gap between letters three (unit 500). NaN where it has shown no gap.
    /// </summary>
    public static double DitFromGaps(IReadOnlyList<double> elementGaps, IReadOnlyList<double> runGaps)
        => elementGaps.Count > 0 ? Median(elementGaps)
            : runGaps.Count > 0 ? runGaps.Min() / 3
            : double.NaN;

    /// <summary>
    /// A sender's letter gaps and word gaps as two clusters, or nulls before it has shown <see cref="MeasuredRunGaps"/>
    /// gaps between runs (work instructions 501, 504, 510 and 513; moved here from the reader by work instruction 525).
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
    private static (List<double>? Letter, List<double>? Word) GapClusters(IReadOnlyList<double> runGaps, double line)
    {
        var sorted = runGaps.Where(g => !(g < line)).OrderBy(g => g).ToList();

        if (sorted.Count < MeasuredRunGaps)
        {
            return (null, null);
        }

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
            return (clusters[at], null);
        }

        var (letter, word) = CwRunReader.Refine(clusters[at], clusters[at + 1]);

        return letter.Count == 0 ? (clusters[at], clusters[at + 1])
            : word.Count == 0 ? (letter, null)
            : (letter, word);
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

        /// <summary>Same pitch within a bin, same height within the reader's level tolerance, and not silent too long.</summary>
        public bool Agrees(CwMark m)
        {
            if (_recent.Count == 0 || m.FromSeconds - LastToSeconds > SilenceSeconds)
            {
                return false;
            }

            var pitch = _recent.Average(r => r.PitchHz);
            var level = _recent.TakeLast(8).Average(r => r.LevelDb);
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

            var level = kind.TakeLast(8).Average(r => r.LevelDb);
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
