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
    /// not a sender. Once a sender is printed, a letter of one mark, E or T, is printed with it;
    /// otherwise every DE would read D.</para>
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

    private double _printedThrough = double.NegativeInfinity;

    private void Print(double heardSeconds)
    {
        // A printed sender silent longer than the hold lets another be printed; at the end of the
        // audio nobody is released, since nothing after it can arrive.
        if (_station is not null
            && double.IsFinite(heardSeconds)
            && _station.Open.Count == 0
            && heardSeconds - _station.LastToSeconds > ReleaseSeconds)
        {
            _station = null;
        }

        // Senders long silent and not printed are forgotten: one that never made two runs of two marks
        // after the hold, one that did after the minute the detector keeps its marks.
        if (double.IsFinite(heardSeconds))
        {
            _senders.RemoveAll(s => s != _station && s.LastToSeconds < heardSeconds - (s.Ended.Count(r => r.Length >= 2) >= QualifyingRuns ? CwEnvelopeDetector.CalledSeconds : ReleaseSeconds));
        }

        // The sender with the most marks among those that have made two runs, the louder on a tie.
        _station ??= _senders
            .Where(s => s.Ended.Count(r => r.Length >= 2 && r.Any(m => m.Keyed)) >= QualifyingRuns && s.TwoKindsSeen)
            .OrderByDescending(s => s.Marks)
            .ThenByDescending(s => s.Reference.Level)
            .FirstOrDefault();

        // The pitch of the sender being printed, for the panel on the screen's thread (work
        // instruction 493): one double, written here on the audio thread and read with Volatile.Read.
        Volatile.Write(ref _stationHz, _station?.Reference.Pitch ?? double.NaN);

        if (_station is not { } station)
        {
            return;
        }

        foreach (var run in station.Ended.Skip(station.PrintedRuns).ToList())
        {
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

        public List<CwMark> Open { get; } = new();

        public List<CwMark[]> Ended { get; } = new();

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

        /// <summary>Between a gap inside a letter, one dit, and one between letters, three.</summary>
        public double CharacterGapSeconds => DitSeconds * Math.Sqrt(3);

        /// <summary>Between a gap between letters, three dits, and one between words, seven.</summary>
        public double WordGapSeconds => DitSeconds * Math.Sqrt(21);

        public double LongestMarkSeconds => _recent.Count > 0 ? _recent.Max(m => m.ToSeconds - m.FromSeconds) : 0;

        public void Add(CwMark mark)
        {
            if (Open.Count > 0)
            {
                _elementGaps.Add(mark.FromSeconds - Open[^1].ToSeconds);
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
