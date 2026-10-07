namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// **THE GATE'S SENDER STAGE** (work instruction 532, HM-DEC-236): from the detector's marks, which sender is printed and
/// when it is let go, which marks make a letter, whether each mark is a dot or a dash, whether a one-mark letter is
/// printed, and where words end; handed on as one sender's stream of dots, dashes, letter ends and word ends, which the
/// reader looks up. A character is a run of consecutive marks that agree on pitch and on level (work instruction 490, R103,
/// HM-DEC-195).
/// </summary>
/// <remarks>
/// <para>**THE OWNER, 2026-10-02**: *"I want to really get to this lookup table and get rid of the decoder logic."* Every
/// decision the reader made moved here unchanged, and the reader keeps the table alone.</para>
/// <para>**THE OWNER, 2026-09-28**: *"An E followed by a T, if it's a real person doing CW, they
/// will have the same amplitude. They will have the same pitch or frequency. They'll have a
/// different duration. A dot or a dash is the only thing that varies."*</para>
/// <para>**A SECOND, SIMPLER READER, BESIDE THE FIRST.** The probabilistic decoder, retired by work instruction 545, read
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
public sealed class CwSenderGate
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

    /// <summary>The shape under which a printed sender is let go: half the 0.2 it stood at (work instruction 526, task 5).</summary>
    internal const double ReleaseScore = CwShapeLights.GreenScore / 2;

    /// <summary>
    /// How many of a sender's last marks its two kinds must hold over before it may print: ten, two sequences' worth (work
    /// instruction 535). Five marks stand a sequence, and overlapping clusters needed twice that before they were a hand
    /// rather than chance (work instruction 523). Over the sixteen a sequence's lengths are read over, real hands showed
    /// a side wider than a hand makes and the scoreboard fell from 185 to 156.
    /// </summary>
    internal const int KindsHeldMarks = 2 * CwPatternGate.MarksToStand;

    // Morse's midpoint between a gap inside a letter and one between letters, one and three units (work instruction 536).
    private static readonly double ElementLetterRatio = Math.Sqrt(3);

    // The marks a sender's length and gap figures are read over.
    private const int RecentMarks = 40;

    private readonly List<Sender> _senders = new();

    /// <summary>Raised once for each character read, in time order, and never revised.</summary>
    public event Action<CwCharacter>? CharacterRead
    {
        add => _table.CharacterRead += value;
        remove => _table.CharacterRead -= value;
    }

    /// <summary>Raised with each letter, beside <see cref="CharacterRead"/>, with the marks its run was made of.</summary>
    /// <remarks>The evidence travels with the letter (§0.0.1): what it was read from, pitch, level and length.</remarks>
    public event Action<CwCharacter, IReadOnlyList<CwMark>>? RunRead
    {
        add => _table.RunRead += value;
        remove => _table.RunRead -= value;
    }

    /// <summary>Creates the gate, counting each letter the table prints for the shape side's reading.</summary>
    public CwSenderGate()
    {
        _table.RunRead += (c, run) =>
        {
            _lettersPrinted++;
            if (c.Text == MorseAlphabet.Unreadable)
            {
                _lettersUnreadable++;
                _unreadableTimes.Add(run[^1].ToSeconds);
            }

            _letterTimes.Add(run[^1].ToSeconds);
        };
    }

    // The shape side's counts and recent times (work instruction 537), written and read on the audio thread.
    private readonly List<double> _letterTimes = new();
    private readonly List<double> _markTimes = new();
    private readonly List<double> _unreadableTimes = new();
    private long _lettersPrinted;
    private long _lettersUnreadable;
    private long _marksStood;
    private double _heard = double.NaN;

    // Written by Publish on the audio thread, read by ShapeReading on any.
    private CwShapeSideReading _shapeReading = CwShapeSideReading.Nothing;

    /// <summary>
    /// **WHAT THE SHAPE SIDE HELD AT THE END OF THE LAST BATCH** (work instruction 537, HM-DEC-241): the printed sender's
    /// pitch and dit, letters and marks with their times, and every sender held. Safe to read from any thread: one
    /// reference, replaced whole on the audio thread.
    /// </summary>
    public CwShapeSideReading ShapeReading => Volatile.Read(ref _shapeReading);

    private void Publish(double heardSeconds)
    {
        if (double.IsFinite(heardSeconds))
        {
            _heard = heardSeconds;
            _letterTimes.RemoveAll(t => t < heardSeconds - CwShapeSideReading.KeptSeconds);
            _markTimes.RemoveAll(t => t < heardSeconds - CwShapeSideReading.KeptSeconds);
            _unreadableTimes.RemoveAll(t => t < heardSeconds - CwShapeSideReading.KeptSeconds);
        }

        var senders = _senders
            .Select(s => new CwSenderStanding(s.Reference.Pitch, s.Shape.Score, s.Marks, s == _station) { Qualified = Qualifies(s) })
            .ToList();

        Volatile.Write(
            ref _shapeReading,
            new CwShapeSideReading(
                _heard,
                _station?.Reference.Pitch ?? double.NaN,
                _station?.DitSeconds ?? double.NaN,
                _lettersPrinted,
                _lettersUnreadable,
                _marksStood,
                _letterTimes.ToArray(),
                _markTimes.ToArray(),
                _unreadableTimes.ToArray(),
                senders));
    }

    // A sender qualifies on two keyed runs of two marks or more, two kinds seen, and its own letter gaps shown: the first test
    // the pick makes, and what the telemetry reports (work instruction 549, task 3, moved here unchanged).
    private static bool Qualifies(Sender s)
        => s.Ended.Count(r => r.Length >= 2 && r.Any(m => m.Keyed)) >= QualifyingRuns && s.TwoKindsSeen && s.LetterGapSeconds is not null;

    // **THE GATE HANDS THE READER ONE SENDER'S STREAM** (work instruction 532): the table it is read through.
    private readonly CwRunReader _table = new();

    /// <summary>The reader the gate's stream is read through: the Morse table alone (work instruction 532).</summary>
    public CwRunReader Reader => _table;

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

        // **WHAT HAS BEEN CALLED, NOT WHAT HAS BEEN HEARD** (work instruction 529): a sender read through its own window
        // has its marks called later than a bin calls them, by the window's delay, so a silence is judged that much later.
        var heard = batch.HeardSeconds - batch.LateSeconds;

        foreach (var mark in batch.Marks.OrderBy(m => m.FromSeconds))
        {
            Take(mark);
        }

        foreach (var sender in _senders)
        {
            if (sender.Open.Count > 0
                && heard - sender.Open[^1].ToSeconds > sender.CharacterGapSeconds + sender.LongestMarkSeconds + CallingLagSeconds)
            {
                End(sender);
            }
        }

        Print(heard);
        Publish(heard);
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
        Publish(_heard);
    }

    private void Take(CwMark mark)
    {
        _marksStood++;
        _markTimes.Add(mark.ToSeconds);
        TakeMark(mark);
    }

    private void TakeMark(CwMark mark)
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
        else if (best.Open.Count > 0 && best.KindOfNext(mark.FromSeconds - best.Open[^1].ToSeconds) != CwGapKind.Element)
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

    // Written by Print on the audio thread, read by WaitingPitchHz on any.
    private double _waitingHz = double.NaN;

    /// <summary>
    /// The pitch of the sender that has qualified to print and is waiting out its first word gap, or NaN when none is
    /// (work instruction 535, HM-DEC-239): with <see cref="StationPitchHz"/>, everything the gate would print, which is
    /// all the hold-still light may claim.
    /// </summary>
    /// <remarks>Safe to read from any thread: one double, written once per batch on the audio thread.</remarks>
    public double WaitingPitchHz => Volatile.Read(ref _waitingHz);

    /// <summary>The dit of the sender being printed, in seconds, or NaN when none is: for the tests' report (work instruction 500).</summary>
    internal double StationDitSeconds => _station?.DitSeconds ?? double.NaN;

    /// <summary>The printed sender's line between dot and dash, in seconds, or NaN: for the tests' report (work instruction 544).</summary>
    internal double StationSplitSeconds => _station?.SplitSeconds ?? double.NaN;

    /// <summary>The level a mark must agree with to be the printed sender's, in dB, or NaN: for the tests' report (work instruction 544).</summary>
    internal double StationLevelDb => _station?.Reference.Level ?? double.NaN;

    /// <summary>The printed sender's dit of gap, in seconds, or NaN: for the tests' report (work instruction 500).</summary>
    internal double StationGapDitSeconds => _station?.GapDitSeconds ?? double.NaN;

    /// <summary>The printed sender's measured letter and word gaps, in seconds, or nulls: for the tests' report (work instruction 501).</summary>
    internal (double? Letter, double? Word) StationGaps => _station?.LetterAndWordGaps() ?? (null, null);

    /// <summary>The printed sender's word line, in seconds, or NaN: for the tests' report (work instruction 528).</summary>
    internal double StationWordLineSeconds => _station?.WordGapSeconds ?? double.NaN;

    /// <summary>The printed sender's gap lines and clusters, or null: for the tests' report (work instruction 528).</summary>
    internal CwGapLines? StationLines => _station?.Lines;

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
        //
        // **AND IT IS HELD UNTIL ITS SHAPE FALLS TO HALF THAT** (work instruction 526, task 5, HM-DEC-230): at 12:10:15 on
        // 7.031 a sender stood at shape 0.50 with the gauge at 0.88 and the reader printed nobody. The gauge reads the gate's
        // sequence; the reader released on its own sender's shape, judged since unit 524 over the marks at its speed now
        // with only their evidence, and where a hand's forty marks look like two speeds that is its newest ten or five,
        // whose evidence alone takes a 0.5 shape under 0.2. A sender stands at 0.2 and is let go only under 0.1: hysteresis,
        // so one dip does not drop a station that plainly stands. Found by reading, not reproduced on the bench.
        if (ShapePicks && CwRules.On(CwRules.Release) && _station is not null && _station.Shape.Score < ReleaseScore)
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
            .Where(Qualifies)
            // **A SENDER SILENT PAST ITS RELEASE IS NOT A CANDIDATE** (work instruction 533, HM-DEC-237): the silence that let
            // it go keeps it from being picked again. On the owner's QSO of 2026-10-03 the first station, released, still
            // outranked the reply on shape, was picked again, released again, and the reply never had the terminal.
            // **AND ITS TWO KINDS HOLD** (work instructions 535 and 536, HM-DEC-240): a carrier keyed at random qualified on two kinds seen over its
            // newest five marks and printed.
            .Where(s => s == _station || !CwRules.On(CwRules.KindsHeld) || s.KindsHeld)
            .Where(s => s == _station || !CwRules.On(CwRules.SilentNotCandidate) || !(double.IsFinite(heardSeconds) && s.Open.Count == 0 && heardSeconds - s.LastToSeconds > s.SilenceSeconds))
            .Select(s => (Sender: s, Score: s.Shape.Score))

            // A sender whose marks and gaps do not sound like code at all - a shape of nought - is not a sender to print.
            // **THE 0.2 STANDING LINE CAME OUT** (work instruction 534, HM-DEC-238): on the owner's recordings it held back
            // real stations whose letter spacing a hand makes uneven, and without it the scoreboard rose from 169 to 182 with
            // loud noise still printing nothing. The light still turns green at 0.2.
            .Where(s => !ShapePicks || s.Score > 0)
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
            if (!ShapePicks || !double.IsFinite(heardSeconds) || !CwRules.On(CwRules.FirstPickWait))
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

        Volatile.Write(
            ref _waitingHz,
            _station is null && !double.IsNaN(_waitingSince) && qualified.Count > 0
                ? Math.Round(qualified[0].Sender.Reference.Pitch / CwEnvelopeDetector.BinSpacingHz) * CwEnvelopeDetector.BinSpacingHz
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
            // printed. Another lone letter does not confirm it; three or more in a row are no longer
            // dropped together (work instruction 534). It replaces unit 498's rule, under which
            // any following letter confirmed it, so a T confirmed an E confirmed a T and the whole
            // string printed.
            if (run.Length == 1 && CwRules.On(CwRules.LoneLetter))
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

            // **THE NEW SENDER'S LETTERS ARE KEPT** (work instruction 533, HM-DEC-237): a sender given the terminal prints the
            // letters it had already sent since it stood, in order, after what is printed, then its live letters. They used to
            // be dropped as printed over, and in a QSO that fell at the start of every reply. Nothing printed is revised: the
            // backlog follows the first sender's text.
            if (!CwRules.On(CwRules.Backlog) && run[0].FromSeconds <= _printedThrough)
            {
                continue;
            }

            Raise(station, run);
            _printedThrough = Math.Max(_printedThrough, run[^1].ToSeconds);
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

        // **THREE LONE LETTERS IN A ROW ARE DROPPED TOGETHER** (removed in work instruction 534; back in 541, HM-DEC-245, when
        // the score counted what is wrong and invented as well as what is right).
        if (CwRules.On(CwRules.ThreeLone) && last - first + 1 >= 3)
        {
            return false;
        }

        var before = first > 0 && run[0].FromSeconds - runs[first - 1][^1].ToSeconds <= window;
        var after = nextFrom is { } from && from - run[^1].ToSeconds <= window;

        return before || after;
    }

    /// <summary>
    /// **EVERY MARK LEAVES LABELLED** (work instruction 532, task 1, HM-DEC-236): the run's marks as dots and dashes, by the
    /// sender's own line between its two lengths, its letter end with the reading of everything but the letter, and a word
    /// end before it where the gap before it is a word. The labels are decided here and the letter is looked up from them.
    /// </summary>
    private void Raise(Sender sender, CwMark[] run)
    {
        var symbols = new List<CwSymbol>();
        var dit = sender.DitSeconds;
        // **A MARK IS SPLIT AT ITS SENDER'S OWN LINE** (work instruction 534, HM-DEC-238): the split against its neighbours
        // came out when the owner's recordings read as well without it.
        var split = sender.SplitSeconds;
        var at = run[^1].ToSeconds;
        var wpm = (int)Math.Round(1.2 / dit);

        if (_printed.Count > 0)
        {
            var (lastSender, lastRun) = _printed[^1];

            if (lastSender != sender || sender.Lines.KindOf(run[0].FromSeconds - lastRun[^1].ToSeconds) == CwGapKind.Word)
            {
                symbols.Add(new CwSymbol(CwSymbolKind.WordEnd)
                {
                    Reading = new CwCharacter(
                        MorseAlphabet.WordGap, CwConfidence.High, 1, string.Empty, double.NaN, wpm,
                        TimeSpan.FromSeconds(run[0].FromSeconds))
                    {
                        Stage = CwReadingStage.Settled,
                    },
                });
            }
        }

        symbols.AddRange(run.Select(m => new CwSymbol(m.ToSeconds - m.FromSeconds >= split ? CwSymbolKind.Dash : CwSymbolKind.Dot)));

        var pattern = string.Concat(run.Select(m => m.ToSeconds - m.FromSeconds >= split ? "-" : "."));

        // How far the most doubtful mark sits from the split, against the half-width of a
        // textbook three-to-one fist in log length: one at a clean dit or dah, nought at the split.
        var score = run.Min(m => Math.Clamp(Math.Abs(Math.Log((m.ToSeconds - m.FromSeconds) / split)) / Math.Log(Math.Sqrt(3)), 0, 1));
        var sure = sender.TwoKindsSeen && score >= Math.Log(1.25) / Math.Log(Math.Sqrt(3));
        var contrasts = run.Select(m => m.ContrastDb).Where(c => !double.IsNaN(c)).ToList();

        symbols.Add(new CwSymbol(CwSymbolKind.LetterEnd)
        {
            Reading = new CwCharacter(
                MorseAlphabet.Unreadable,
                sure ? CwConfidence.High : CwConfidence.Low,
                score,
                pattern,
                contrasts.Count > 0 ? contrasts.Min() : double.NaN,
                wpm,
                TimeSpan.FromSeconds(at))
            {
                Stage = CwReadingStage.Settled,
                SpanHops = (int)Math.Round((at - run[0].FromSeconds) * 1000 / CwCharacter.HopMilliseconds),
            },
            Marks = run,
        });

        _lastClusters = sender.Describe();

        _table.Take(symbols);

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

                // The spacing now includes the first of the three gaps that showed it; the speed now begins after its own.
                var spacing = CwRules.On(CwRules.SpacingNow) ? _spacingSince : double.NegativeInfinity;

                return _runGapsAt.Where(g => g.At > since && g.At >= spacing).Select(g => g.Seconds).ToList();
            }
        }

        // Where the sender's spacing now began: the first of three gaps in a row past its word line (work instruction 546).
        private double _spacingSince = double.NegativeInfinity;

        /// <summary>
        /// **THREE WORDS IN A ROW ARE A NEW SPACING** (work instruction 546, task 2): a sender whose last three gaps between
        /// runs all sit past its word line, each by √(7/3), has either sent three one-letter words running, which the gate already does not
        /// take as sending (three lone letters are dropped together), or has stretched its spacing, as W1AW does going from
        /// its ordinary sending to a Farnsworth section. Its gaps are then taken from the first of the three, so its letter
        /// and word clusters are its own spacing now, however far both are from its dit. Shipped at the owner's word (work
        /// instruction 547, task 2, HM-DEC-251).
        /// </summary>
        private void NoteSpacing()
        {
            if (!CwRules.On(CwRules.SpacingNow))
            {
                return;
            }

            var since = _runGapsAt.Where(g => g.At >= _spacingSince).ToList();

            if (since.Count <= SpacingRun)
            {
                return;
            }

            var line = Lines.WordSeconds;
            var last = since.TakeLast(SpacingRun).ToList();

            if (last.All(g => g.Seconds > line * StretchJump))
            {
                _spacingSince = last[0].At;
            }
        }

        /// <summary>How many gaps in a row past the word line show a new spacing: three, as three lone letters are not sending.</summary>
        private const int SpacingRun = 3;

        /// <summary>
        /// How far past the word line each of the three must sit: the jump the gate splits a sender's gap clusters at, √(7/3),
        /// so a hand's ordinary word gaps beside its line are not a new spacing and a stretched letter gap is.
        /// </summary>
        private static readonly double StretchJump = Math.Sqrt(7.0 / 3);

        public List<CwMark> Open { get; } = new();

        public List<CwMark[]> Ended { get; } = new();

        /// <summary>Split the ended runs from one index on again, at the sender's character gap now.</summary>
        /// <summary>
        /// What kind the gap before a mark just heard is, judged against the sender's gaps before it (work instruction 526;
        /// removed in 534, back in 541, HM-DEC-245).
        /// </summary>
        /// <param name="gap">The gap from the sender's last mark to the new one.</param>
        public CwGapKind KindOfNext(double gap)
        {
            if (CwRules.On(CwRules.NeighbourGaps))
            {
                var gaps = _recent.Skip(1).Select((m, i) => m.FromSeconds - _recent[i].ToSeconds).Append(gap).ToList();

                return CwPatternGate.KindAmongNeighbours(gaps, gaps.Count - 1, Lines);
            }

            return Lines.KindOf(gap);
        }

        /// <param name="from">The first run not yet printed.</param>
        public void Resplit(int from)
        {
            if (from >= Ended.Count)
            {
                return;
            }

            var lines = Lines;
            var marks = Ended.Skip(from).SelectMany(r => r).OrderBy(m => m.FromSeconds).ToList();

            // Each gap judged by the sender's own lines (the judgement against its neighbours came out, work instruction 534).
            var gaps = marks.Skip(1).Select((m, i) => m.FromSeconds - marks[i].ToSeconds).ToList();
            var runs = new List<CwMark[]>();
            var current = new List<CwMark> { marks[0] };

            for (var i = 1; i < marks.Count; i++)
            {
                if ((CwRules.On(CwRules.NeighbourGaps) ? CwPatternGate.KindAmongNeighbours(gaps, i - 1, lines) : lines.KindOf(gaps[i - 1])) != CwGapKind.Element)
                {
                    runs.Add(current.ToArray());
                    current = new List<CwMark>();
                }

                current.Add(marks[i]);
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
                var open = Open.ToList();

                // **THE REFERENCE IS LOCAL TO THE MARK** (work instruction 525, task 3, HM-DEC-229). A mark's level is judged
                // against the mean of the sender's last three marks (CwPatternGate.LevelMarks, a letter's worth, inside a
                // letter or across a gap), and no longer against the open run's mean or the last eight marks'. Its pitch,
                // which a fade does not move, is still read over them. Eight marks are two seconds at 18 WPM, and a slow
                // fade of 6 dB over four seconds moves a station by up to 4.7 dB in a second, so their mean sat several
                // decibels behind the next letter, past the level tolerance: a second sender began at the J of JUMPS and
                // the bulletin's letters were dealt between the two, and a digit's five elements lagged their own mean the
                // same way. The last mark alone was local enough, and at 12 dB its own noise split N0CALL; three marks
                // are half a second at 18 WPM, across which such a fade moves their mean about a decibel.
                var recent = _recent.TakeLast(8).ToList();
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

        /// <summary>
        /// **ITS TWO KINDS HOLD OVER ITS LAST TEN MARKS** (work instruction 536, HM-DEC-240): a sender keys two lengths, dit and
        /// dah, across every stretch of its sending, so its last <see cref="KindsHeldMarks"/> marks split with a clean jump of
        /// <see cref="TwoKindsRatio"/>, neither side is wider than a hand makes, and each kind recurs, two marks or more. A carrier
        /// keyed at random shows two kinds only over a handful of its newest marks, where the speed retry looks, or by one odd
        /// mark against the rest.
        /// </summary>
        public bool KindsHeld
        {
            get
            {
                if (_recent.Count < KindsHeldMarks)
                {
                    return false;
                }

                var kinds = KindsOf(_recent.Skip(_recent.Count - KindsHeldMarks).ToList());

                // Each kind recurs: one mark is not a kind, and a carrier keyed at random qualified on one 45 ms mark against
                // nine spread from 135 to 275.
                // **AND ITS GAPS FALL INTO KINDS** (work instruction 536, task 2, HM-DEC-240): among the nine gaps between those
                // marks a clean jump of √3, Morse's own midpoint between a gap inside a letter (one unit) and one between
                // letters (three), at least two gaps either side. A carrier keyed at random spaces its marks evenly from 30 to
                // 400 ms. A jump of 2, the marks' own, refused a sender on 22:15:48 whose letter gaps sit 1.87 times its gaps
                // inside letters, and cost 13 letters.
                var marks = _recent.Skip(_recent.Count - KindsHeldMarks).ToList();
                var gaps = marks.Skip(1).Select((m, i) => m.FromSeconds - marks[i].ToSeconds).OrderBy(g => g).ToList();
                var gapKinds = !CwRules.On(CwRules.GapKinds) || Enumerable.Range(2, Math.Max(0, gaps.Count - 3)).Any(i => gaps[i] / gaps[i - 1] >= ElementLetterRatio);

                return gapKinds && kinds.Split is not null && !kinds.TwoSpeeds
                    && kinds.Shorts.Count >= 2 && KindsHeldMarks - kinds.Shorts.Count >= 2;
            }
        }

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

                // **THE LINE IT LAST SHOWED** (work instruction 544): its dit is the mean of its marks under that line while a
                // few marks hide the jump between its two kinds.
                if (KeptSplit is { } kept)
                {
                    var under = _recent.Select(m => m.ToSeconds - m.FromSeconds).Where(l => l < kept).ToList();

                    if (under.Count > 0)
                    {
                        return under.Average();
                    }
                }

                // One length only: the dit from its gaps, where it has shown any (the gate's, work instruction 525).
                var fromGaps = CwPatternGate.DitFromGaps(_elementGaps, RunGaps);

                return double.IsNaN(fromGaps) ? Median(_recent.Select(m => m.ToSeconds - m.FromSeconds).ToList()) : fromGaps;
            }
        }

        /// <summary>Short against long: the geometric mean of the sender's short and long marks.</summary>
        public double SplitSeconds => Kinds().Split ?? KeptSplit ?? (DitSeconds * Math.Sqrt(3));

        private double? _lastSplit;

        /// <summary>
        /// **A SENDER KEEPS ITS LINE WHILE A FEW MARKS HIDE IT** (work instruction 544, task 1, HM-DEC-248): the line between
        /// dot and dash it last showed, or null where it has shown none or the rule is off.
        /// </summary>
        /// <remarks>
        /// **FOUND ON THE SCAN'S STRONG STATION** (`catch-153810-7033367`): a heavy fist, dits of 65 ms and dahs of 150, had
        /// dahs broken by the detector into 85 and 107 ms marks that filled the jump between its two kinds, so no two
        /// neighbouring lengths among its last forty differed by twice. The gate then took the dit from its gaps inside
        /// letters, 36 ms where its dits are 65, and the line fell to 67 ms: fourteen letters had dits read as dahs. Nothing
        /// said the sender had changed speed; where it has, the newer marks show their own jump and the line moves with them.
        /// </remarks>
        private double? KeptSplit
        {
            get
            {
                if (!CwRules.On(CwRules.KeptSplit) || _lastSplit is not { } kept)
                {
                    return null;
                }

                // **THE KEPT LINE SORTS THE MARKS NOW; THE MARKS NOW PLACE THE LINE**: the line kept only says which of the
                // marks now are dits and which dahs, and the line between them is drawn again from the two, where a mark sits
                // as many spreads from either centre, as a clean split's is. A straight key's lengths wander, and a line kept
                // from a stretch where they sat elsewhere read the C of its second N0CALL as an F.
                var lengths = MarksNow().Select(m => m.ToSeconds - m.FromSeconds).ToList();
                var shorts = lengths.Where(l => l < kept).ToList();
                var longs = lengths.Where(l => l >= kept).ToList();

                return shorts.Count == 0 || longs.Count == 0 ? kept : Boundary(LogStats(shorts), LogStats(longs));
            }
        }

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
                NoteSpacing();
            }

            Open.Add(mark);
            _recent.Add(mark);
            Marks++;
            LastToSeconds = Math.Max(LastToSeconds, mark.ToSeconds);

            Trim(_recent);
            Trim(_elementGaps);
            Trim(_runGapsAt);
            Trim(_gapOverDit);

            if (Kinds().Split is { } split)
            {
                _lastSplit = split;
            }
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

        // **THE SENDER'S MARKS ARE TWO CLUSTERS, DIT AND DAH** (work instruction 513, R113, HM-DEC-217).
        // Sorted lengths are split at the widest ratio between neighbors, where two neighbors differ by
        // TwoKindsRatio, with the boundary where a mark sits as many spreads from either centre; a hand's
        // overlapping clusters are no longer taken as two kinds (work instruction 534, HM-DEC-238).
        // The short side, and the boundary.
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
            if (!CwRules.On(CwRules.SpeedRetry) || !KindsOf(_recent).TwoSpeeds)
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

            // **NO CLEAN JUMP IS ONE KIND** (work instruction 534, HM-DEC-238): a hand's overlapping lengths were taken as two
            // kinds since unit 513, and that rule came out when the owner's recordings read better without it. Where no two
            // neighbours differ by TwoKindsRatio, the marks are one kind, and a mix of lengths is two speeds.
            return (lengths, null, true);
        }
    }
}
