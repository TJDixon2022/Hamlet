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
            // **A MARK THAT STOOD BY ITS SENDER'S PATTERN IS MATCHED ON PITCH ALONE** (work instruction
            // 511, task 2, HM-DEC-215). The gate judged its level already, against that sender's own
            // marks of its kind and within twice the tolerance; judged again here against dits and dahs
            // mixed, a dit 6 dB down read a hair past it and began a sender of its own. No other mark
            // is matched this way.
            var levelTolerance = LevelToleranceDb(contrast);

            if (pitchOff > PitchToleranceHz || (!mark.BySendersPattern && levelOff > levelTolerance))
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
        else if (best.Open.Count > 0 && best.Lines.KindOf(mark.FromSeconds - best.Open[^1].ToSeconds) != CwGapKind.Element)
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

    /// <summary>
    /// The printed sender's clusters as last measured on this thread, in words: for the tests' report
    /// (work instruction 513).
    /// </summary>
    [ThreadStatic]
    private static string? _lastClusters;

    /// <summary>The printed sender's clusters as last measured on this thread: for the tests' report (work instruction 513).</summary>
    internal static string LastClusters => _lastClusters ?? "not measured";

    /// <summary>The centre and spread of some lengths in log-length (unit 513): shared with the gate's gap clusters (work instruction 525).</summary>
    internal static (double Mu, double Sd) LogStats(IReadOnlyCollection<double> lengths) => Sender.LogStats(lengths);

    /// <summary>The boundary between two clusters of lengths (unit 513): shared with the gate's gap clusters (work instruction 525).</summary>
    internal static double Boundary((double Mu, double Sd) low, (double Mu, double Sd) high) => Sender.Boundary(low, high);

    /// <summary>Two clusters of lengths settled by the nearer centre (unit 513): shared with the gate's gap clusters (work instruction 525).</summary>
    internal static (List<double> Low, List<double> High) Refine(List<double> low, List<double> high) => Sender.Refine(low, high);

    /// <summary>
    /// Whether some mark lengths with no clean jump of <see cref="TwoKindsRatio"/> are a hand's two kinds (work
    /// instructions 513 and 523): the reader's own test, for the pattern gate.
    /// </summary>
    /// <param name="lengths">The lengths.</param>
    /// <returns>True where they split as a hand's dits and dahs.</returns>
    internal static bool TwoKindsOfAHand(IEnumerable<double> lengths)
    {
        var sorted = lengths.OrderBy(l => l).ToList();
        var at = -1;
        var widest = 0.0;

        for (var i = 1; i < sorted.Count; i++)
        {
            if (sorted[i] / sorted[i - 1] > widest)
            {
                widest = sorted[i] / sorted[i - 1];
                at = i;
            }
        }

        return at > 0 && Sender.HandKinds(sorted, at) is not null;
    }

    /// <summary>Every sender's pitch and shape, and whether it is the one printed: for the tests' report (work instruction 519).</summary>
    internal IReadOnlyList<(double PitchHz, CwSequenceShape Shape, bool Printed, int Marks)> SenderShapes
        => _senders.Select(s => (s.Reference.Pitch, s.Shape, s == _station, s.Marks)).ToList();

    /// <summary>
    /// Whether the shape picks the sender printed (work instruction 519); on by default. Off, unit 490's rule
    /// returns - the most marks, the louder on a tie, held until silent - so the tests can print the before.
    /// </summary>
    internal bool ShapePicks { get; set; } = true;

    // Where the wait for the first pick began, and how long it is (work instruction 520).
    private double _waitingSince = double.NaN;
    private double _waitSeconds;

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

        // **A SENDER IS PRINTED ONLY WHILE IT SOUNDS LIKE CODE** (work instruction 524, HM-DEC-228): a random carrier's
        // first marks can score the line by chance and stand, and as more arrive its lengths and gaps do not hold
        // together and its shape falls, where a sender's rises with the evidence. A printed sender whose shape is under
        // the line the light uses is let go, as a silent one is, and its letters not yet printed are not printed.
        if (ShapePicks && _station is not null && _station.Shape.Score < CwShapeLights.GreenScore)
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

        // **SHAPE PICKS THE SENDER; LOUDNESS PICKS NOTHING** (work instruction 519, R116, HM-DEC-223). Of the
        // senders that qualify - two keyed runs, two kinds, their own letter gaps shown - the one printed is
        // the one whose marks and gaps sound most like code. Unit 490 printed the one with the most marks,
        // and the louder on a tie. Which one, when, and how it is held follows below (work instruction 520). It
        // is released as before, silent past twice its word gap, a dah and the lag in calling it (unit 500). A
        // shape of nought prints nothing.
        var qualified = _senders
            .Where(s => s.Ended.Count(r => r.Length >= 2 && r.Any(m => m.Keyed)) >= QualifyingRuns && s.TwoKindsSeen && s.LetterGapSeconds is not null)
            .Select(s => (Sender: s, Score: s.Shape.Score))

            // A sender whose marks and gaps do not sound like code - under the line the light uses - is not a sender
            // to print (work instructions 519 and 524): a carrier keyed at random qualified by its lengths and was held.
            .Where(s => !ShapePicks || s.Score >= CwShapeLights.GreenScore)
            .OrderByDescending(s => ShapePicks ? s.Score : s.Sender.Marks)
            .ThenByDescending(s => ShapePicks ? 0 : s.Sender.Reference.Level)
            .ToList();

        // **THE BEST SHAPE GETS THE TERMINAL** (work instruction 520, HM-DEC-224). A printed sender silent for its
        // own word gap and a dah - a mark is seen only once it has ended, so a letter gap and the dah after it fall
        // short of this and only a gap between words passes - gives the terminal to a better-shaped sender standing
        // then. Never inside a word, and never to a worse shape.
        if (ShapePicks
            && _station is not null
            && _station.Open.Count == 0
            && double.IsFinite(heardSeconds)
            && heardSeconds - _station.LastToSeconds > _station.WordGapSeconds + _station.LongestMarkSeconds + CallingLagSeconds
            && qualified.FirstOrDefault(q => q.Sender != _station) is { Sender: not null } better
            && better.Score > _station.Shape.Score)
        {
            _station = better.Sender;
        }

        // **AND THE FIRST PICK WAITS ONE WORD GAP** (work instruction 520): when the first sender qualifies, the
        // reader waits one of its word gaps for others to qualify, then prints the best-shaped. Its letters are
        // banked meanwhile and print a word late; at the end of the audio nothing waits.
        if (_station is null && qualified.Count > 0)
        {
            if (!ShapePicks || !double.IsFinite(heardSeconds))
            {
                _station = qualified[0].Sender;
            }
            else if (double.IsNaN(_waitingSince))
            {
                _waitingSince = heardSeconds;
                _waitSeconds = qualified[0].Sender.WordGapSeconds;
            }
            else if (heardSeconds - _waitingSince >= _waitSeconds)
            {
                _station = qualified[0].Sender;
            }
        }

        if (_station is not null)
        {
            _waitingSince = double.NaN;
        }

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

            if (lastSender != sender || sender.Lines.KindOf(run[0].FromSeconds - lastRun[^1].ToSeconds) == CwGapKind.Word)
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

        _lastClusters = sender.Describe();

        CharacterRead?.Invoke(character);
        RunRead?.Invoke(character, run);

        _printed.Add((sender, run));
    }

    /// <summary>The runs that agree with one another on pitch and level: one sender.</summary>
    private sealed class Sender
    {
        private readonly List<CwMark> _recent = new();
        private readonly List<double> _elementGaps = new();

        // Each gap between runs with the time it ended, the start of the mark after it (work instruction 524).
        private readonly List<(double At, double Seconds)> _runGapsAt = new();

        // How much longer each gap inside a letter read than the dit of the marks at the time, once
        // the sender has shown two kinds (work instruction 500).
        private readonly List<double> _gapOverDit = new();

        /// <summary>
        /// The sender's gaps between runs at its speed now (work instruction 524, case 1).
        /// </summary>
        /// <remarks>
        /// **THE GAP CLUSTERS AT THE SPEED NOW ARE THE NEWER MARKS' GAPS.** Where the sender's recent marks are two
        /// speeds and its dit and dah are taken from the newer ones (unit 523), its letter and word gaps are taken from
        /// the same stretch: those that ended after the first of those marks began. A 10 WPM letter gap kept among a 20
        /// WPM sender's put the word line at 555 ms, above its new 420 ms word gap, and `TEST DE` read as one word.
        /// Until the newer stretch has shown its own letter gaps the line is counted in its gap dits, as for any sender.
        /// The gaps inside letters are not taken again: a run closed at the old speed's line holds the new speed's
        /// letter gaps among them, and they are what the run is split again by.
        /// </remarks>
        private List<double> RunGaps
        {
            get
            {
                var now = MarksNow();
                var since = ReferenceEquals(now, _recent) || now.Count == 0 ? double.NegativeInfinity : now[0].FromSeconds;

                return _runGapsAt.Where(g => g.At > since).Select(g => g.Seconds).ToList();
            }
        }

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

            var lines = Lines;
            var marks = Ended.Skip(from).SelectMany(r => r).OrderBy(m => m.FromSeconds).ToList();
            var runs = new List<CwMark[]>();
            var current = new List<CwMark> { marks[0] };

            foreach (var mark in marks.Skip(1))
            {
                if (lines.KindOf(mark.FromSeconds - current[^1].ToSeconds) != CwGapKind.Element)
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

        /// <summary>The clusters as measured, in words: for the tests' report (work instruction 513).</summary>
        public string Describe()
        {
            static string Of(IReadOnlyCollection<double>? lengths)
            {
                if (lengths is null || lengths.Count == 0)
                {
                    return "none";
                }

                var logs = lengths.Select(l => Math.Log(l)).ToList();
                var mu = logs.Average();
                var sd = Math.Sqrt(logs.Sum(l => (l - mu) * (l - mu)) / logs.Count);

                return string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{Math.Exp(mu) * 1000:0} ms ±{sd:0.00}");
            }

            var (shorts, split) = Kinds();
            var longs = split is { } s ? _recent.Select(m => m.ToSeconds - m.FromSeconds).Where(l => l >= s).ToList() : null;
            var lines = Lines;
            var (letter, word) = (lines.LetterGaps, lines.WordGaps);
            var inside = lines.InsideGaps;

            return string.Create(
                System.Globalization.CultureInfo.InvariantCulture,
                $"dit {Of(split is null ? null : shorts)}, dah {Of(longs)}, split {SplitSeconds * 1000:0} ms; gaps element {Of(inside)}, letter {Of(letter)}, word {Of(word)}; boundaries {lines.CharacterSeconds * 1000:0} and {lines.WordSeconds * 1000:0} ms");
        }

        /// <summary>
        /// How much this sender's marks at its speed now sound like code (work instructions 519 and 524): judged over the
        /// marks its dit and dah are taken from, and with the evidence of those marks alone. Where its recent forty are
        /// two speeds, the newer few are all that is known of the speed now, and a random carrier's newest handful must
        /// not borrow the evidence of the forty.
        /// </summary>
        public CwSequenceShape Shape
        {
            get
            {
                var now = MarksNow();

                return CwSequenceShape.Of(now, ReferenceEquals(now, _recent) ? Marks : now.Count);
            }
        }

        public int PrintedRuns { get; set; }

        public int Marks { get; private set; }

        public double LastToSeconds { get; private set; } = double.NegativeInfinity;

        /// <summary>What a mark must agree with: the open run's mean, or the sender's recent marks'.</summary>
        public (double Pitch, double Level, double Contrast) Reference
        {
            get
            {
                // A quieter mark taken by the sender's pattern is the sender's, and not its level: it would
                // drag the reference under the sender's next mark (work instruction 511, task 2).
                var open = Open.Where(m => !m.BySendersPattern).ToList();

                // **THE REFERENCE IS LOCAL TO THE MARK** (work instruction 525, task 3, HM-DEC-229). A mark's level is judged
                // against the mean of the sender's last three marks (CwPatternGate.LevelMarks, a letter's worth, inside a
                // letter or across a gap), and no longer against the open run's mean or the last eight marks'. Its pitch,
                // which a fade does not move, is still read over them. Eight marks are two seconds at 18 WPM, and a slow
                // fade of 6 dB over four seconds moves a station by up to 4.7 dB in a second, so their mean sat several
                // decibels behind the next letter, past the level tolerance: a second sender began at the J of JUMPS and
                // the bulletin's letters were dealt between the two, and a digit's five elements lagged their own mean the
                // same way. The last mark alone was local enough, and at 12 dB its own noise split N0CALL; three marks
                // are half a second at 18 WPM, across which such a fade moves their mean about a decibel.
                var recent = _recent.Where(m => !m.BySendersPattern).TakeLast(8).ToList();
                var level = recent.Count > 0 ? recent.TakeLast(CwPatternGate.LevelMarks).Average(m => m.LevelDb)
                    : open.Count > 0 ? open.Average(m => m.LevelDb)
                    : double.NaN;
                var over = open.Count > 0 ? open
                    : recent.Count > 0 ? recent
                    : Open.Count > 0 ? (IReadOnlyList<CwMark>)Open : _recent.TakeLast(8).ToList();
                var contrasts = over.Select(m => m.ContrastDb).Where(c => !double.IsNaN(c)).ToList();

                return (over.Average(m => m.PitchHz), double.IsNaN(level) ? over.Average(m => m.LevelDb) : level, contrasts.Count > 0 ? contrasts.Average() : double.NaN);
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

                // One length only: the dit from its gaps, where it has shown any (the gate's, work instruction 525).
                var fromGaps = CwPatternGate.DitFromGaps(_elementGaps, RunGaps);

                return double.IsNaN(fromGaps) ? Median(_recent.Select(m => m.ToSeconds - m.FromSeconds).ToList()) : fromGaps;
            }
        }

        /// <summary>Short against long: the geometric mean of the sender's short and long marks.</summary>
        public double SplitSeconds => Kinds().Split ?? (DitSeconds * Math.Sqrt(3));

        /// <summary>
        /// **THE GATE DECIDES WHAT KIND EACH GAP IS** (work instruction 525, HM-DEC-229): the lines between the sender's
        /// gaps inside letters, between letters and between words, and the letter and word gaps it has shown, from its
        /// dit, its gaps inside letters and its gaps between runs at its speed now. The reader places its letters and
        /// spaces from <see cref="CwGapLines.KindOf"/> and holds no gap arithmetic of its own; units 500, 501, 504, 510
        /// and 513's moved to <see cref="CwPatternGate.GapLines"/> unchanged.
        /// </summary>
        public CwGapLines Lines => CwPatternGate.GapLines(DitSeconds, _elementGaps, _gapOverDit, RunGaps);

        /// <summary>One dit of gap, in seconds: the marks' dit and the detector's smear on a gap (unit 500).</summary>
        public double GapDitSeconds => Lines.GapDitSeconds;

        /// <summary>Between a gap inside a letter and one between letters, in seconds.</summary>
        public double CharacterGapSeconds => Lines.CharacterSeconds;

        /// <summary>Between a gap between letters and one between words, in seconds.</summary>
        public double WordGapSeconds => Lines.WordSeconds;

        /// <summary>The mean of the sender's letter gaps, in seconds, or null before it has shown enough of them.</summary>
        public double? LetterGapSeconds => Lines.LetterSeconds;

        /// <summary>The sender's measured letter gap and, where it has shown one, word gap: for the tests' report.</summary>
        public (double? Letter, double? Word) LetterAndWordGaps()
        {
            var lines = Lines;

            return (lines.LetterSeconds, lines.WordClusterSeconds);
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
            ? Math.Max(SilenceSeconds, CwPatternGate.SlowestFarnsworthWordGapSeconds + LongestMarkSeconds + CallingLagSeconds)
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
                _runGapsAt.Add((mark.FromSeconds, mark.FromSeconds - LastToSeconds));
            }

            Open.Add(mark);
            _recent.Add(mark);
            Marks++;
            LastToSeconds = Math.Max(LastToSeconds, mark.ToSeconds);

            Trim(_recent);
            Trim(_elementGaps);
            Trim(_runGapsAt);
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

        /// <summary>
        /// The least spread a cluster is given, as the standard deviation of its lengths' logarithm: a
        /// tenth (work instruction 513, R113, HM-DEC-217).
        /// </summary>
        /// <remarks>
        /// **THE DETECTOR'S OWN READING ERROR, NOT A FIST'S.** A mark or a gap is read to one five
        /// millisecond hop, smeared by the ten millisecond window, which is a tenth to a fifth of a dit
        /// from 12 to 35 WPM. A machine sender's clusters measure narrower than that on a handful of
        /// marks only by chance, and a cluster ruled that narrow would pull every boundary against it.
        /// A hand's scatter, tens of percent, is wider and is measured as it is. The author's.
        /// </remarks>
        public const double SpreadFloor = 0.1;

        /// <summary>
        /// How many of their spreads two clusters' centres must stand apart to be two kinds: two each
        /// side of the boundary (work instruction 513). The author's.
        /// </summary>
        /// <remarks>
        /// Two spreads each side put the boundary where nineteen in twenty of either cluster fall on its
        /// own side, which a fist a third either way still clears; noise's lengths, spread evenly, split
        /// into two halves whose centres stand about three of their spreads apart, and fail it.
        /// </remarks>
        public const double SeparationSpreads = 2;

        /// <summary>
        /// The widest a cluster of one kind can be, as the standard deviation of its lengths' logarithm:
        /// a quarter (work instruction 513). The author's.
        /// </summary>
        /// <remarks>
        /// **WHAT A HAND DOES, AND NO MORE.** The widest fist the order names scatters a third either way
        /// (unit 504), a spread of about 0.19 in log-length, and the detector's own reading error of 0.1
        /// adds to it to about 0.22; a quarter holds that. A cluster wider than any hand makes is two
        /// speeds, not one kind: a sender who goes from 10 to 20 WPM leaves dits of 120 and 60 ms and
        /// dahs of 360 and 180 among its recent marks, and 60, 120 and 180 together are 0.45 wide. It
        /// is not taken as two kinds, and the dit is read from the gaps inside letters, which follow a
        /// change of speed at once, as before this unit. The speed-change case turning red is what showed
        /// the need; the figure is the hand's.
        /// </remarks>
        public const double HandSpread = 0.25;

        /// <summary>The centre and spread of some lengths, in log-length, the spread floored.</summary>
        internal static (double Mu, double Sd) LogStats(IReadOnlyCollection<double> lengths)
        {
            var logs = lengths.Select(l => Math.Log(l)).ToList();
            var mu = logs.Average();
            var sd = Math.Sqrt(logs.Sum(l => (l - mu) * (l - mu)) / logs.Count);

            return (mu, Math.Max(SpreadFloor, sd));
        }

        /// <summary>
        /// Where a length is as many of one cluster's spreads from its centre as of the other's from its
        /// own: the boundary between two kinds, in seconds (work instruction 513, R113).
        /// </summary>
        /// <remarks>
        /// **THE SPREAD DECIDES, NOT THE MIDPOINT.** With equal spreads it is the geometric mean of the
        /// two centres, the old line; where one cluster is wider, a length is the kind it sits fewer of
        /// that kind's spreads from, which is the ear's reading of a fist whose dahs wander more than
        /// its dits.
        /// </remarks>
        internal static double Boundary((double Mu, double Sd) low, (double Mu, double Sd) high)
            => Math.Exp(((low.Mu * high.Sd) + (high.Mu * low.Sd)) / (low.Sd + high.Sd));

        /// <summary>
        /// Two clusters settled: each length goes to the side of the boundary between the two it falls
        /// on, and the boundary is measured again, until nothing moves (work instruction 513).
        /// </summary>
        internal static (List<double> Low, List<double> High) Refine(List<double> low, List<double> high)
        {
            var all = low.Concat(high).OrderBy(l => l).ToList();

            for (var i = 0; i < 8 && low.Count > 0 && high.Count > 0; i++)
            {
                var boundary = Boundary(LogStats(low), LogStats(high));
                var nextLow = all.Where(l => l < boundary).ToList();

                if (nextLow.Count == low.Count)
                {
                    break;
                }

                low = nextLow;
                high = all.Skip(nextLow.Count).ToList();
            }

            return (low, high);
        }

        // **THE SENDER'S MARKS ARE TWO CLUSTERS, DIT AND DAH** (work instruction 513, R113, HM-DEC-217).
        // Sorted lengths are first split at the widest ratio between neighbors, then settled as two
        // clusters in log-length, each mark on the side of the boundary it sits fewer spreads from. They
        // are two kinds when their centres stand at least TwoKindsRatio apart and SeparationSpreads of
        // their spreads each side of the boundary. The short side, and the boundary.
        private (List<double> Shorts, double? Split) Kinds()
        {
            var kinds = KindsOf(MarksNow());

            return (kinds.Shorts, kinds.Split);
        }

        /// <summary>
        /// The marks that are the sender's speed now: its recent forty, or where those are two speeds the newest
        /// half, quarter and so on whose split is as tight as a hand makes (work instruction 523, case 3).
        /// </summary>
        private IReadOnlyList<CwMark> MarksNow()
        {
            // **TWO SPEEDS: THE NEWER IS THE SENDER'S NOW** (work instruction 523, case 3). A sender who steps from 10 to
            // 20 WPM leaves 60, 120, 180 and 360 ms marks among its recent forty, two jumps of two, and the widest put
            // 60, 120 and 180 together as dits, so a 20 WPM dah read short. A side wider than a hand makes is two speeds
            // (unit 513); where the split over the recent marks has one, it is taken again over the newest half, then the
            // newest quarter, down to the five marks a sequence needs to stand, and the first split as tight as a hand
            // makes is the sender's now.
            if (!KindsOf(_recent).TwoSpeeds)
            {
                return _recent;
            }

            for (var n = _recent.Count / 2; n >= CwPatternGate.MarksToStand; n /= 2)
            {
                var marks = _recent.Skip(_recent.Count - n).ToList();
                var newer = KindsOf(marks);

                if (!newer.TwoSpeeds && newer.Split is not null && newer.Shorts.Count > 0)
                {
                    return marks;
                }
            }

            return _recent;
        }

        private static (List<double> Shorts, double? Split, bool TwoSpeeds) KindsOf(IReadOnlyList<CwMark> marks)
        {
            var lengths = marks.Select(m => m.ToSeconds - m.FromSeconds).OrderBy(l => l).ToList();
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

            if (at < 0)
            {
                return (lengths, null, false);
            }

            // **A CLEAN GAP BETWEEN THE LENGTHS IS TWO KINDS, AS BEFORE.** Where two neighbors differ by
            // TwoKindsRatio, the marks fall into two kinds there, as they have since unit 490; only the
            // line between them moves, to where a mark sits as many spreads from either centre.
            if (widest >= TwoKindsRatio)
            {
                var shortSide = lengths.Take(at).ToList();
                var longSide = lengths.Skip(at).ToList();

                var shortStats = LogStats(shortSide);
                var longStats = LogStats(longSide);

                return (shortSide, Boundary(shortStats, longStats), shortStats.Sd > HandSpread || longStats.Sd > HandSpread);
            }

            // **A FIST'S LENGTHS OVERLAP, AND NO CLEAN GAP IS LEFT** (work instruction 513, R113). Its
            // dits run long and its dahs short until no two neighbors differ by two, so the two kinds are
            // found as two clusters settled by the nearer centre, and taken only where they are what a
            // hand makes: centres TwoKindsRatio apart, SeparationSpreads each side of the boundary, and
            // neither wider than HandSpread. A mix of two speeds is wider, and is not taken.
            var hand = HandKinds(lengths, at);

            // Refused as a hand's two kinds, a mix of lengths is two speeds as much as a wide side of a clean jump is.
            return hand is { } h ? (h.Shorts, h.Split, false) : (lengths, null, true);
        }

        /// <summary>
        /// Sorted lengths with no clean jump of <see cref="TwoKindsRatio"/>, split at the widest as a hand's two
        /// kinds: two clusters settled by the nearer centre, taken where they are what a hand makes, or null (work
        /// instruction 513; shared with the pattern gate by work instruction 523).
        /// </summary>
        /// <param name="lengths">The lengths, shortest first.</param>
        /// <param name="at">Where the widest ratio between neighbours falls.</param>
        /// <returns>The short side and the boundary, or null.</returns>
        public static (List<double> Shorts, double? Split)? HandKinds(List<double> lengths, int at)
        {
            var (shorts, longs) = Refine(lengths.Take(at).ToList(), lengths.Skip(at).ToList());

            if (shorts.Count == 0 || longs.Count == 0)
            {
                return null;
            }

            var dit = LogStats(shorts);
            var dah = LogStats(longs);

            if (Math.Exp(dah.Mu - dit.Mu) < TwoKindsRatio || dah.Mu - dit.Mu < SeparationSpreads * (dit.Sd + dah.Sd)
                || dit.Sd > HandSpread || dah.Sd > HandSpread)
            {
                return null;
            }

            return (shorts, Boundary(dit, dah));
        }
    }
}
