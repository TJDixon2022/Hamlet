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
            home = new Sequence();
            _sequences.Add(home);
        }
        else if (home.Crowds(candidate))
        {
            // **THE SAME TONE READ TWICE, OR A PIECE OF IT**: dropped, not begun again elsewhere.
            return Array.Empty<CwMark>();
        }

        var standing = home.Add(candidate);

        Stood += standing.Count;

        return standing;
    }

    /// <summary>Forget sequences silent past <see cref="SilenceSeconds"/>.</summary>
    /// <param name="nowSeconds">The detector's audio clock.</param>
    public void Prune(double nowSeconds)
        => _sequences.RemoveAll(s => nowSeconds - s.LastToSeconds > SilenceSeconds);

    /// <summary>One sender's marks.</summary>
    private sealed class Sequence
    {
        private readonly List<CwMark> _held = new();
        private readonly List<CwMark> _recent = new();

        public bool Standing { get; private set; }

        public int Count { get; private set; }

        public double LastToSeconds { get; private set; } = double.NegativeInfinity;

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
