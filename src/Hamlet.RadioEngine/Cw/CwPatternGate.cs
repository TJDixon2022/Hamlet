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
    public const double SilenceSeconds = CwRunReader.SlowestFarnsworthWordGapSeconds + (3 * 1.2 / 5);

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

            home = new Sequence(++_nextId);
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

            return under > tolerance && under <= QuieterShare * tolerance;
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

            if (_held.Count < MarksToStand || !TwoLengths(_held))
            {
                return Array.Empty<CwMark>();
            }

            Standing = true;

            var released = _held.ToArray();

            _held.Clear();

            return released;
        }

        /// <summary>Whether the lengths split in two at a ratio of <see cref="CwRunReader.TwoKindsRatio"/> or wider.</summary>
        private static bool TwoLengths(IReadOnlyList<CwMark> marks)
        {
            var lengths = marks.Select(m => m.ToSeconds - m.FromSeconds).OrderBy(l => l).ToList();

            for (var i = 1; i < lengths.Count; i++)
            {
                if (lengths[i] / lengths[i - 1] >= CwRunReader.TwoKindsRatio)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
