namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// Reads characters from the detector's marks: a character is a run of consecutive marks that
/// agree on pitch and on level, and the lengths and gaps inside it say which letter it is (work
/// instruction 490, R103, HM-DEC-195).
/// </summary>
/// <remarks>
/// <para>**THE OWNER, 2026-09-28**: *"An E followed by a T, if it's a real person doing CW, they
/// will have the same amplitude. They will have the same pitch or frequency. They'll have a
/// different duration. A dot or a dash is the only thing that varies."*</para>
/// <para>**A SECOND, SIMPLER READER, BESIDE THE FIRST.** <see cref="CwProbabilisticDecoder"/> reads
/// one mixed stream and fits letters to timing alone; it was never given a mark's pitch or
/// level. This reads the marks <see cref="CwEnvelopeDetector"/> calls, each with all three, and
/// nothing else. The lattice, the speed grid, the unit estimator and the emission gate are not
/// touched and not used.</para>
/// <para>**A RUN** is marks at one pitch, within one bin, and one level, within
/// <see cref="LevelToleranceDb"/> of the run's own mean. A mark that breaks either agreement is
/// never folded into the run: it starts a run of its own, which may be another sender's, while
/// the run it did not join goes on. **A run ends** at a gap longer than its sender's character
/// gap. **The letter** is its marks' lengths, short against long split at the geometric mean of
/// the sender's short and long marks, and a word gap is a gap longer than the sender's word
/// gap, both measured from the sender's own dit.</para>
/// <para>**ONE SENDER IS PRINTED** (work instruction 490 section 3's third case): a sender is the
/// runs that agree with one another on pitch and level; one is printed when it has made
/// <see cref="QualifyingRuns"/> runs, the one with the most marks when several have, and it is
/// held until it has been silent <see cref="ReleaseSeconds"/>.</para>
/// <para>**NOTHING PRINTED IS TAKEN BACK** (R100). A character is raised once, when its run has
/// ended, and never revised.</para>
/// </remarks>
public sealed class CwRunReader
{
    /// <summary>How far a mark's pitch may sit from its run's and be the same sender's, in Hz: one bin.</summary>
    /// <remarks>
    /// **AUTHOR'S, FROM WHAT A SENDER'S MARKS DO, NOT FITTED.** A human's keying holds one pitch;
    /// the detector places each mark on the bin at the peak of its lobe, so a tone that sits
    /// between two bins lands on either one. One bin is that rounding and nothing more.
    /// </remarks>
    public const double PitchToleranceHz = CwEnvelopeDetector.BinSpacingHz;

    /// <summary>The ratio at or above which the longer of two mark lengths is a different element.</summary>
    /// <remarks>
    /// **A DAH IS AT LEAST TWICE A DIT** in every fist this project has measured - 2.73 and 4.24
    /// (HM-DEC-144, HM-DEC-145) - and a sender's own dits do not scatter that far. Author's.
    /// </remarks>
    public const double TwoKindsRatio = 2;

    /// <summary>How many runs of two marks or more a sender makes, with dits and dahs among them, before it is printed.</summary>
    /// <remarks>
    /// <para>**A SENDER REPEATS HIMSELF; NOISE THAT AGREES WITH ITSELF ONCE IS A COINCIDENCE.** One run
    /// can be a lone blip; two runs agreeing on pitch and level are somebody keying. Author's.</para>
    /// <para>**AND A RUN MUST EARN ITS LETTERS** (work instruction 492, R105, HM-DEC-197). Marks are now
    /// handed out the moment they end, unpaired, so the noise guard the pairing gave is here: a run
    /// of one mark never counts toward a sender - a lone bar, however clean, is not a character - and
    /// a sender whose marks are all one length, never two kinds at <see cref="TwoKindsRatio"/>, is
    /// not a sender. A letter of one mark, E or T, is banked until the same sender confirms it
    /// with another letter (work instruction 498).</para>
    /// <para>**AND THE RUNS THAT MAKE A SENDER ARE KEYED** (work instruction 493, R106, HM-DEC-198).
    /// Measured on three minutes of noise with nothing else guarding, a noise sender made its two
    /// runs by chance, was printed, and never fell silent for a second, so it printed every noise
    /// mark at its pitch as an E: 53 to 81 letters, which is the owner's terminal full of E's. Each
    /// of the two runs must hold a mark the detector called while it was keying there - bars paired
    /// and clear of their gaps' wander, the same test that decides the blocks the scope draws - so a
    /// letter in the terminal is a letter over the blocks.</para>
    /// </remarks>
    public const int QualifyingRuns = 2;

    /// <summary>How long a printed sender may be silent before another may be printed, in seconds.</summary>
    /// <remarks>The detector's own hold (<see cref="CwEnvelopeDetector.HoldSeconds"/>): the longest gap in ordinary sending.</remarks>
    public const double ReleaseSeconds = CwEnvelopeDetector.HoldSeconds;

    // A mark arrives only once the level after it has dropped, a hop or two after it ends: two
    // envelope windows, twenty milliseconds.
    private const double CallingLagSeconds = 0.02;

    // The marks a sender's length and gap figures are read over.
    private const int RecentMarks = 40;

    /// <summary>
    /// The word gap of the slowest Farnsworth sending the ARRL's code practice uses, 18 WPM letters at
    /// 5 WPM overall, in seconds: 3.66 (work instruction 501).
    /// </summary>
    /// <remarks>
    /// **FROM PARIS, NOT FROM ANY RECORDING.** PARIS is fifty units: thirty-one in its letters and the
    /// gaps inside them, sent at the letter speed, and nineteen in its spaces, four letter gaps of
    /// three and one word gap of seven, stretched so the word takes 60/5 seconds. So one stretched
    /// unit is (60/5 - 31 × 1.2/18) / 19 seconds, and a word gap is seven of them.
    /// </remarks>
    internal const double SlowestFarnsworthWordGapSeconds = 7 * ((60.0 / 5) - (31 * 1.2 / 18)) / 19;

    private readonly List<Sender> _senders = new();

    /// <summary>Raised once for each character read, in time order, and never revised.</summary>
    public event Action<CwCharacter>? CharacterRead;

    /// <summary>Raised with each letter, beside <see cref="CharacterRead"/>, with the marks its run was made of.</summary>
    /// <remarks>The evidence travels with the letter (§0.0.1): what it was read from, pitch, level and length.</remarks>
    public event Action<CwCharacter, IReadOnlyList<CwMark>>? RunRead;

    /// <summary>
    /// How far a mark's level may sit from its run's mean and be the same sender's, in dB, for a
    /// sender that many dB over its gaps.
    /// </summary>
    /// <param name="contrastDb">The run's level over its gaps; NaN when unknown.</param>
    /// <returns>Twice the detector's own flatness tolerance at that contrast.</returns>
    /// <remarks>
    /// **AUTHOR'S, FROM WHAT A SENDER'S MARKS DO, NOT FITTED.** A human's keying holds one level,
    /// and the wobble is the detector's own measurement noise: every hop of a mark sits within
    /// <see cref="CwEnvelopeDetector.ToleranceDb"/> of the mark's mean (R93), so a mark's mean and
    /// the run's mean, each measured that way, can differ by twice it and no more.
    /// </remarks>
    public static double LevelToleranceDb(double contrastDb) => 2 * CwEnvelopeDetector.ToleranceDb(contrastDb);

    /// <summary>Take the marks called since the last batch, and end every run whose gap has passed.</summary>
    /// <param name="batch">The marks and the detector's clock.</param>
    public void Read(CwMarkBatch batch)
    {
        ArgumentNullException.ThrowIfNull(batch);

        foreach (var mark in batch.Marks.OrderBy(m => m.FromSeconds))
        {
            Take(mark);
        }

        foreach (var sender in _senders)
        {
            if (sender.Open.Count > 0
                && batch.HeardSeconds - sender.Open[^1].ToSeconds > sender.CharacterGapSeconds + sender.LongestMarkSeconds + CallingLagSeconds)
            {
                End(sender);
            }
        }

        Print(batch.HeardSeconds);
    }

    /// <summary>End every run and print what they read: the audio is over.</summary>
    public void Flush()
    {
        foreach (var sender in _senders)
        {
            if (sender.Open.Count > 0)
            {
                End(sender);
            }
        }

        Print(double.PositiveInfinity);
    }

    private void Take(CwMark mark)
    {
        Sender? best = null;
        var bestDistance = double.PositiveInfinity;

        foreach (var sender in _senders)
        {
            var (pitch, level, contrast) = sender.Reference;
            var pitchOff = Math.Abs(mark.PitchHz - pitch);
            var levelOff = Math.Abs(mark.LevelDb - level);
            var levelTolerance = LevelToleranceDb(contrast);

            if (pitchOff > PitchToleranceHz || levelOff > levelTolerance)
            {
                continue;
            }

            var distance = (pitchOff / PitchToleranceHz) + (levelOff / levelTolerance);

            if (distance < bestDistance)
            {
                best = sender;
                bestDistance = distance;
            }
        }

        if (best is null)
        {
            best = new Sender();
            _senders.Add(best);
        }
        else if (best.Open.Count > 0 && mark.FromSeconds - best.Open[^1].ToSeconds > best.CharacterGapSeconds)
        {
            End(best);
        }

        best.Add(mark);
    }

    private static void End(Sender sender)
    {
        sender.Ended.Add(sender.Open.ToArray());
        sender.Open.Clear();
    }

    private readonly List<(Sender Sender, CwMark[] Run)> _printed = new();

    // The sender being printed, and where on the clock the last printed run ended.
    private Sender? _station;

    // Written by Print on the audio thread, read by StationPitchHz on any.
    private double _stationHz = double.NaN;

    /// <summary>The pitch of the sender being printed, or NaN when none is (work instruction 493).</summary>
    /// <remarks>Safe to read from any thread: one double, written once per batch on the audio thread.</remarks>
    public double StationPitchHz => Volatile.Read(ref _stationHz);

    /// <summary>The dit of the sender being printed, in seconds, or NaN when none is: for the tests' report (work instruction 500).</summary>
    internal double StationDitSeconds => _station?.DitSeconds ?? double.NaN;

    /// <summary>The printed sender's dit of gap, in seconds, or NaN: for the tests' report (work instruction 500).</summary>
    internal double StationGapDitSeconds => _station?.GapDitSeconds ?? double.NaN;

    /// <summary>The printed sender's measured letter and word gaps, in seconds, or nulls: for the tests' report (work instruction 501).</summary>
    internal (double? Letter, double? Word) StationGaps => _station?.LetterAndWordGaps() ?? (null, null);

    private double _printedThrough = double.NegativeInfinity;

    private void Print(double heardSeconds)
    {
        // A printed sender silent longer than the hold lets another be printed; at the end of the
        // audio nobody is released, since nothing after it can arrive.
        if (_station is not null
            && double.IsFinite(heardSeconds)
            && _station.Open.Count == 0
            && heardSeconds - _station.LastToSeconds > _station.SilenceSeconds)
        {
            _station = null;
        }

        // Senders long silent and not printed are forgotten: one that never made two runs of two marks
        // after its own silence (work instruction 501: at least the slowest Farnsworth word gap until
        // it has shown its own gaps), one that did after the minute the detector keeps its marks.
        if (double.IsFinite(heardSeconds))
        {
            _senders.RemoveAll(s => s != _station && s.LastToSeconds < heardSeconds - (s.Ended.Count(r => r.Length >= 2) >= QualifyingRuns ? CwEnvelopeDetector.CalledSeconds : s.ForgetSeconds));
        }

        // The sender with the most marks among those that have made two runs, the louder on a tie.
        // And that has shown its own gaps between letters (work instruction 501): printed before, a
        // Farnsworth sender's first two letters were spaced by a boundary built on its dit and read C Q.
        _station ??= _senders
            .Where(s => s.Ended.Count(r => r.Length >= 2 && r.Any(m => m.Keyed)) >= QualifyingRuns && s.TwoKindsSeen && s.LetterGapSeconds is not null)
            .OrderByDescending(s => s.Marks)
            .ThenByDescending(s => s.Reference.Level)
            .FirstOrDefault();

        // The pitch of the sender being printed, for the panel on the screen's thread (work
        // instruction 493): one double, written here on the audio thread and read with Volatile.Read.
        // On the bin grid, as each mark's pitch is (work instruction 496): a mean over marks that
        // sit either side of a tone between bins would otherwise name no bin at all, 666 for 650
        // and 675, and never match the tone the panel reports beside it.
        Volatile.Write(
            ref _stationHz,
            _station is { } printed
                ? Math.Round(printed.Reference.Pitch / CwEnvelopeDetector.BinSpacingHz) * CwEnvelopeDetector.BinSpacingHz
                : double.NaN);

        if (_station is not { } station)
        {
            return;
        }

        // **THE UNPRINTED RUNS ARE READ AGAIN AT THE DIT THE SENDER HAS NOW SHOWN** (work instruction
        // 498). A sender's first runs were closed before its dit was known - on TEST, the T's dah was
        // taken as a dit, the gap after it as inside a letter, and T and E became N. By the time a
        // sender is printed it has shown dits and dahs, so what is still unprinted is split again.
        station.Resplit(station.PrintedRuns);

        while (station.PrintedRuns < station.Ended.Count)
        {
            var run = station.Ended[station.PrintedRuns];

            // **A LONE LETTER MUST BELONG TO SOMETHING** (work instruction 501, R109, HM-DEC-205). A
            // letter of one mark, E or T, is banked until it is known whether a letter of two marks or
            // more from the same sender stands beside it, before or after, within ConfirmSeconds; if
            // one does it is released in its own place, and if none does it is dropped and never
            // printed. Another lone letter does not confirm it, and three or more lone letters in a
            // row are not sending and are dropped together. It replaces unit 498's rule, under which
            // any following letter confirmed it, so a T confirmed an E confirmed a T and the whole
            // string printed.
            if (run.Length == 1)
            {
                var belongs = LoneLetterBelongs(station, station.PrintedRuns, heardSeconds);

                if (belongs is null)
                {
                    break;
                }

                if (belongs == false)
                {
                    station.PrintedRuns++;
                    continue;
                }
            }

            station.PrintedRuns++;

            // What another sender was printed over is not printed afterward: the terminal runs
            // forward in time and nothing is inserted behind what it shows.
            if (run[0].FromSeconds <= _printedThrough)
            {
                continue;
            }

            Raise(station, run);
            _printedThrough = run[^1].ToSeconds;
        }
    }

    /// <summary>
    /// How far from a one-mark letter a letter of two marks or more from the same sender may stand
    /// and still confirm it, in seconds: twice the sender's word-gap boundary (work instructions 498
    /// and 501).
    /// </summary>
    /// <remarks>
    /// <para>**FROM THE SENDER'S OWN SPACING, NOT FROM ANY RECORDING.** A lone E or T inside a word
    /// has a neighbor a character gap away, and one at the end of a word, like the E of DE, one a
    /// character gap before it; twice the sender's word-gap boundary holds both with room for a
    /// sender who stretches his gaps. The author's, overrulable.</para>
    /// <para>**THE BOUNDARY IS THE SENDER'S OWN** (work instruction 501): measured on its gaps between
    /// letters once it has shown them, so a Farnsworth sender's second and a half between letters
    /// is inside the window, and on the gaps inside its letters until then.</para>
    /// </remarks>
    private static double ConfirmSeconds(Sender sender) => 2 * sender.WordGapSeconds;

    /// <summary>
    /// Whether the one-mark letter at <paramref name="index"/> belongs to something: true to print
    /// it, false to drop it, null to wait (work instruction 501, R109).
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="index">The run, a letter of one mark, among the sender's ended runs.</param>
    /// <param name="heardSeconds">The detector's clock.</param>
    /// <returns>The verdict, or null while what follows is still to come.</returns>
    /// <remarks>
    /// <para>**THE OWNER, R109**: *"They very rarely, almost never, will stand on their own. They need
    /// to be part of something. And T, T, T, T, T or E, E, E, E, E is not part of something."*</para>
    /// <para>**THE BLOCK** is the lone letters in a row around this one, up to the letters of two marks
    /// or more either side. It is settled once a letter of two marks or more follows it, the run in
    /// progress already holds two marks, or nothing has come within the window, the sender's longest
    /// mark and the lag in calling it, since a mark reaches the reader only once it has ended.</para>
    /// <para>**THREE OR MORE LONE LETTERS IN A ROW ARE DROPPED TOGETHER**: English does not put EEE,
    /// TTT or TET in a row. **FEWER ARE PRINTED WHEN A LETTER OF TWO MARKS OR MORE STANDS WITHIN THE
    /// WINDOW** of this one, before or after - the S of TEST for its T and E, the D of DE for its E.
    /// Nothing printed is taken back (R100): a banked letter has not been printed.</para>
    /// </remarks>
    private static bool? LoneLetterBelongs(Sender sender, int index, double heardSeconds)
    {
        var runs = sender.Ended;
        var run = runs[index];
        var window = ConfirmSeconds(sender);
        var first = index;
        var last = index;

        while (first > 0 && runs[first - 1].Length == 1)
        {
            first--;
        }

        while (last + 1 < runs.Count && runs[last + 1].Length == 1)
        {
            last++;
        }

        double? nextFrom = last + 1 < runs.Count ? runs[last + 1][0].FromSeconds
            : sender.Open.Count >= 2 ? sender.Open[0].FromSeconds
            : null;

        if (nextFrom is null
            && (sender.Open.Count > 0
                || heardSeconds - runs[last][^1].ToSeconds <= window + sender.LongestMarkSeconds + CallingLagSeconds))
        {
            return null;
        }

        if (last - first + 1 >= 3)
        {
            return false;
        }

        var before = first > 0 && run[0].FromSeconds - runs[first - 1][^1].ToSeconds <= window;
        var after = nextFrom is { } from && from - run[^1].ToSeconds <= window;

        return before || after;
    }

    private void Raise(Sender sender, CwMark[] run)
    {
        var dit = sender.DitSeconds;
        var split = sender.SplitSeconds;
        var at = run[^1].ToSeconds;
        var wpm = (int)Math.Round(1.2 / dit);

        if (_printed.Count > 0)
        {
            var (lastSender, lastRun) = _printed[^1];

            if (lastSender != sender || run[0].FromSeconds - lastRun[^1].ToSeconds > sender.WordGapSeconds)
            {
                CharacterRead?.Invoke(new CwCharacter(
                    MorseAlphabet.WordGap, CwConfidence.High, 1, string.Empty, double.NaN, wpm,
                    TimeSpan.FromSeconds(run[0].FromSeconds))
                {
                    Stage = CwReadingStage.Settled,
                });
            }
        }

        var pattern = string.Concat(run.Select(m => m.ToSeconds - m.FromSeconds >= split ? "-" : "."));

        // How far the most doubtful mark sits from the split, against the half-width of a
        // textbook three-to-one fist in log length: one at a clean dit or dah, nought at the split.
        var score = run.Min(m => Math.Clamp(Math.Abs(Math.Log((m.ToSeconds - m.FromSeconds) / split)) / Math.Log(Math.Sqrt(3)), 0, 1));
        var text = MorseAlphabet.Lookup(pattern);
        var confidence = text is null ? CwConfidence.Unreadable
            : sender.TwoKindsSeen && score >= Math.Log(1.25) / Math.Log(Math.Sqrt(3)) ? CwConfidence.High
            : CwConfidence.Low;
        var contrasts = run.Select(m => m.ContrastDb).Where(c => !double.IsNaN(c)).ToList();

        var character = new CwCharacter(
            text ?? MorseAlphabet.Unreadable,
            confidence,
            score,
            pattern,
            contrasts.Count > 0 ? contrasts.Min() : double.NaN,
            wpm,
            TimeSpan.FromSeconds(at))
        {
            Stage = CwReadingStage.Settled,
            SpanHops = (int)Math.Round((at - run[0].FromSeconds) * 1000 / CwProbabilisticDecoder.HopMilliseconds),
        };

        CharacterRead?.Invoke(character);
        RunRead?.Invoke(character, run);

        _printed.Add((sender, run));
    }

    /// <summary>The runs that agree with one another on pitch and level: one sender.</summary>
    private sealed class Sender
    {
        private readonly List<CwMark> _recent = new();
        private readonly List<double> _elementGaps = new();
        private readonly List<double> _runGaps = new();

        // How much longer each gap inside a letter read than the dit of the marks at the time, once
        // the sender has shown two kinds (work instruction 500).
        private readonly List<double> _gapOverDit = new();

        public List<CwMark> Open { get; } = new();

        public List<CwMark[]> Ended { get; } = new();

        /// <summary>Split the ended runs from one index on again, at the sender's character gap now.</summary>
        /// <param name="from">The first run not yet printed.</param>
        public void Resplit(int from)
        {
            if (from >= Ended.Count)
            {
                return;
            }

            var gap = CharacterGapSeconds;
            var marks = Ended.Skip(from).SelectMany(r => r).OrderBy(m => m.FromSeconds).ToList();
            var runs = new List<CwMark[]>();
            var current = new List<CwMark> { marks[0] };

            foreach (var mark in marks.Skip(1))
            {
                if (mark.FromSeconds - current[^1].ToSeconds > gap)
                {
                    runs.Add(current.ToArray());
                    current = new List<CwMark>();
                }

                current.Add(mark);
            }

            runs.Add(current.ToArray());
            Ended.RemoveRange(from, Ended.Count - from);
            Ended.AddRange(runs);
        }

        public int PrintedRuns { get; set; }

        public int Marks { get; private set; }

        public double LastToSeconds { get; private set; } = double.NegativeInfinity;

        /// <summary>What a mark must agree with: the open run's mean, or the sender's recent marks'.</summary>
        public (double Pitch, double Level, double Contrast) Reference
        {
            get
            {
                var over = Open.Count > 0 ? (IReadOnlyList<CwMark>)Open : _recent.TakeLast(8).ToList();
                var contrasts = over.Select(m => m.ContrastDb).Where(c => !double.IsNaN(c)).ToList();

                return (over.Average(m => m.PitchHz), over.Average(m => m.LevelDb), contrasts.Count > 0 ? contrasts.Average() : double.NaN);
            }
        }

        /// <summary>Whether this sender's marks fall into two lengths, dits and dahs.</summary>
        public bool TwoKindsSeen => Kinds().Split is not null;

        /// <summary>The sender's dit, in seconds.</summary>
        public double DitSeconds
        {
            get
            {
                var (shorts, split) = Kinds();

                if (split is not null)
                {
                    return shorts.Average();
                }

                // One length only: the gaps inside a letter are a dit each, and the gaps between
                // letters three.
                return _elementGaps.Count > 0 ? Median(_elementGaps)
                    : _runGaps.Count > 0 ? _runGaps.Min() / 3
                    : Median(_recent.Select(m => m.ToSeconds - m.FromSeconds).ToList());
            }
        }

        /// <summary>Short against long: the geometric mean of the sender's short and long marks.</summary>
        public double SplitSeconds => Kinds().Split ?? (DitSeconds * Math.Sqrt(3));

        /// <summary>
        /// One dit of gap, in seconds: the dit of the sender's marks, plus how much longer the
        /// detector reads a gap inside a letter than that dit (work instruction 500).
        /// </summary>
        /// <remarks>
        /// <para>**A GAP IS COUNTED IN DITS OF GAP.** Morse makes the gap inside a letter one dit, and the
        /// detector reads a mark short and the gap after it long, each by about its window's smear
        /// (work instruction 498). At 35 WPM that is a fifth of a dit: a boundary built on the marks'
        /// dit of 28 ms against a true 34 landed at 48 ms, inside the 50 ms gaps inside a C, which
        /// read as K and E. A gap between letters or words carries the same smear, so the unit they
        /// are counted in is a gap inside a letter.</para>
        /// <para>**THE SMEAR IS FIXED; THE DIT IS NOT.** The smear is the detector's and does not move
        /// with the sender's speed - measured here at 10 to 16 ms from 5 to 35 WPM - while the dit
        /// follows the sender's latest marks. So the unit is the marks' dit plus the median of what
        /// the sender's gaps inside letters have run over it, which follows a sender who speeds up
        /// as fast as its marks do, where a median of the gaps themselves held a 10 WPM unit through
        /// a 20 WPM answer. Before the sender has shown dits and dahs the marks' dit stands alone.</para>
        /// </remarks>
        public double GapDitSeconds => DitSeconds + (_gapOverDit.Count > 0 ? Math.Max(0, Median(_gapOverDit)) : 0);

        /// <summary>Between a gap inside a letter, one dit, and one between letters, three: the geometric mean.</summary>
        public double CharacterGapSeconds => GapDitSeconds * Math.Sqrt(3);

        /// <summary>
        /// Between a gap between letters and one between words, in seconds: the sender's own letter
        /// gap times the square root of seven thirds once it has shown <see cref="MeasuredRunGaps"/>
        /// gaps between runs, and three and seven of its gap dit, at their geometric mean, until then
        /// (work instruction 501).
        /// </summary>
        /// <remarks>
        /// <para>**A FARNSWORTH SENDER'S LETTERS ARE FAST AND ITS SPACES SLOW.** The ARRL's slow code
        /// practice sends the letters at 18 WPM and stretches the spaces to make 5 to 15 WPM overall,
        /// so inside a letter the gap is a dit and between letters it can be over a second: Morse's
        /// 1:3:7 on the dit holds inside a letter and nowhere else. Every case before this unit had
        /// its spaces scaled to its dit.</para>
        /// <para>**THE SPACES KEEP THREE TO SEVEN AMONG THEMSELVES.** Stretching spreads PARIS's nineteen
        /// spacing units - four gaps of three between its letters, one of seven after the word -
        /// evenly, so a letter gap and a word gap are still three and seven of one stretched unit.
        /// So the boundary is the sender's own letter gap times √(7/3), their geometric mean in
        /// stretched units, exactly as the dit is the unit of the gaps inside a letter.</para>
        /// </remarks>
        public double WordGapSeconds => LetterGapSeconds is { } letter
            ? letter * Math.Sqrt(7.0 / 3)
            : GapDitSeconds * Math.Sqrt(21);

        /// <summary>How many gaps between runs a sender shows before its own letter gap is used.</summary>
        /// <remarks>Author's: three, so no single gap sets the boundary alone.</remarks>
        public const int MeasuredRunGaps = 3;

        /// <summary>
        /// The mean of the sender's gaps between letters, in seconds, or null before it has shown
        /// <see cref="MeasuredRunGaps"/> gaps between runs (work instruction 501).
        /// </summary>
        /// <remarks>
        /// **THE LOWEST CLUSTER OF THE GAPS BETWEEN RUNS.** Sorted, they are walked up from the
        /// shortest until two neighbors differ by √(7/3) or more, half of three-to-seven in log
        /// length; what is below is the letter gaps. The word gaps are the next cluster up, and
        /// anything longer - a pause between two calls - is above them, so it cannot pull the letter
        /// gap the way a split at the widest ratio would.
        /// <para>**AND NOTHING SHORTER THAN THEM MEASURES THEM EITHER** (work instruction 504). A
        /// hesitation inside a letter splits it, and the gap between the halves can sit far enough
        /// under the sender's letter gaps to be a cluster of its own; walked up from the shortest,
        /// that one gap was the letter gap, the word boundary fell under every real letter gap, and
        /// every letter printed as a word for the forty gaps the sender remembers. So the letter
        /// gaps are the lowest cluster of at least <see cref="MeasuredRunGaps"/>, the same count
        /// a sender shows before its letter gap is used at all: a sender who really slows or
        /// speeds up makes a new cluster of three within three letters, and one odd gap never
        /// does. How far apart two clusters are is unchanged, √(7/3): a hand's gaps scatter by tens
        /// of percent, and a factor of 1.53 is past that scatter.</para>
        /// </remarks>
        public double? LetterGapSeconds => LetterAndWordGaps().Letter;

        /// <summary>The sender's measured letter gap and, where it has shown one, word gap: for the tests' report.</summary>
        public (double? Letter, double? Word) LetterAndWordGaps()
        {
            // **A GAP UNDER THE SENDER'S OWN LETTER BOUNDARY IS NOT A LETTER GAP** (work instruction
            // numbered 509, run as unit 510, task 7, HM-DEC-214). Runs closed before the sender's dit
            // was known can end at a gap inside a letter; at 35 WPM and 10 dB three of those, 46 ms,
            // were the lowest cluster, the letter gap was measured as the gap inside a letter, and every
            // 110 ms letter gap read as a word. What sits under the boundary between a gap inside a
            // letter and one between letters, gap dits times √3, measured now, measures neither.
            var boundary = CharacterGapSeconds;
            var sorted = _runGaps.Where(g => !(g < boundary)).OrderBy(g => g).ToList();

            if (sorted.Count < MeasuredRunGaps)
            {
                return (null, null);
            }
            var jump = Math.Sqrt(7.0 / 3);
            var clusters = new List<List<double>> { new() { sorted[0] } };

            for (var i = 1; i < sorted.Count; i++)
            {
                if (sorted[i] / sorted[i - 1] >= jump)
                {
                    clusters.Add(new List<double>());
                }

                clusters[^1].Add(sorted[i]);
            }

            // **ONE ODD GAP IS NOT A CLUSTER** (work instruction 504): the letter gaps are the lowest
            // cluster holding MeasuredRunGaps gaps or more, and a smaller one under it is read against
            // them rather than measuring them. Where none holds that many, the lowest stands, as before.
            var at = clusters.FindIndex(c => c.Count >= MeasuredRunGaps);

            at = at < 0 ? 0 : at;

            return (clusters[at].Average(), at + 1 < clusters.Count ? clusters[at + 1].Average() : null);
        }

        /// <summary>
        /// How long this sender may be silent and still be sending, in seconds: the detector's hold,
        /// or twice its word-gap boundary and the wait for its next mark where that is longer (work
        /// instruction 500).
        /// </summary>
        /// <remarks>
        /// <para>**A SLOW SENDER'S WORD GAP IS LONGER THAN THE HOLD.** Seven dits is 840 ms at 10 WPM and
        /// 1.68 s at 5, and a one-second hold forgot a 5 WPM sender between every word before it had
        /// made its two runs, so the call's first words were never printed. Twice the word-gap
        /// boundary, about nine dits, is the same span a lone letter waits for its confirmation in.</para>
        /// <para>**AND THE NEXT MARK IS SEEN ONLY ONCE IT HAS ENDED**, so the silence also covers the
        /// sender's longest mark and the lag in calling it, as the banking's wait does: at 10 WPM the D
        /// after TEST began 845 ms after the last T and reached the reader 1.2 s after it, and a sender
        /// forgotten at 1.15 s took that T with it.</para>
        /// </remarks>
        public double SilenceSeconds => Math.Max(ReleaseSeconds, (2 * WordGapSeconds) + LongestMarkSeconds + CallingLagSeconds);

        /// <summary>
        /// How long a sender not yet printed may be silent before it is forgotten, in seconds: its
        /// <see cref="SilenceSeconds"/>, and before it has shown its own gaps between runs, at least
        /// the slowest Farnsworth word gap and the wait for its next mark (work instruction 501).
        /// </summary>
        /// <remarks>
        /// **A SENDER CANNOT MEASURE GAPS IT IS FORGOTTEN IN.** A 5 WPM Farnsworth sender leaves a
        /// second and a half between letters and 3.66 s between words, and a sender forgotten at its
        /// dit's silence, about a second, began again at every letter and never made two runs.
        /// </remarks>
        public double ForgetSeconds => LetterGapSeconds is null
            ? Math.Max(SilenceSeconds, SlowestFarnsworthWordGapSeconds + LongestMarkSeconds + CallingLagSeconds)
            : SilenceSeconds;

        /// <summary>The median gap inside the sender's letters, in seconds; NaN before it has shown one.</summary>
        public double ElementGapSeconds => _elementGaps.Count > 0 ? Median(_elementGaps) : double.NaN;

        public double LongestMarkSeconds => _recent.Count > 0 ? _recent.Max(m => m.ToSeconds - m.FromSeconds) : 0;

        public void Add(CwMark mark)
        {
            if (Open.Count > 0)
            {
                var gap = mark.FromSeconds - Open[^1].ToSeconds;

                _elementGaps.Add(gap);

                if (TwoKindsSeen)
                {
                    _gapOverDit.Add(gap - DitSeconds);
                }
            }
            else if (!double.IsNegativeInfinity(LastToSeconds))
            {
                _runGaps.Add(mark.FromSeconds - LastToSeconds);
            }

            Open.Add(mark);
            _recent.Add(mark);
            Marks++;
            LastToSeconds = Math.Max(LastToSeconds, mark.ToSeconds);

            Trim(_recent);
            Trim(_elementGaps);
            Trim(_runGaps);
            Trim(_gapOverDit);
        }

        private static void Trim<T>(List<T> list)
        {
            if (list.Count > RecentMarks)
            {
                list.RemoveRange(0, list.Count - RecentMarks);
            }
        }

        private static double Median(List<double> values)
        {
            var sorted = values.OrderBy(v => v).ToList();

            return sorted.Count == 0 ? double.NaN : sorted[sorted.Count / 2];
        }

        // Sorted lengths split at the widest ratio between neighbors, where that ratio is two
        // kinds; the short side, and the geometric mean of the two sides' means.
        private (List<double> Shorts, double? Split) Kinds()
        {
            var lengths = _recent.Select(m => m.ToSeconds - m.FromSeconds).OrderBy(l => l).ToList();
            var at = -1;
            var widest = 0.0;

            for (var i = 1; i < lengths.Count; i++)
            {
                var ratio = lengths[i] / lengths[i - 1];

                if (ratio > widest)
                {
                    widest = ratio;
                    at = i;
                }
            }

            if (at < 0 || widest < TwoKindsRatio)
            {
                return (lengths, null);
            }

            var shorts = lengths.Take(at).ToList();
            var longs = lengths.Skip(at).ToList();

            return (shorts, Math.Sqrt(shorts.Average() * longs.Average()));
        }
    }
}
