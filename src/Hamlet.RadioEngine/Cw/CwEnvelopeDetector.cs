using System.Globalization;
using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Cw;

/// <summary>One hop as the scope draws it.</summary>
/// <param name="EnvelopeDb">The level of the bin being watched this hop.</param>
/// <param name="FloorDb">That bin's gap level, measured over the last second; NaN when it has no gaps.</param>
/// <param name="ThresholdDb">Midway between its gap level and its bar level, measured; NaN when either is.</param>
/// <param name="Mark">Whether this hop sat inside a bar that has a partner across a gap.</param>
/// <param name="ShapeScore">
/// The shape score of the mark called over this hop within two bins of the watched pitch, or NaN
/// where no mark was called there (work instruction 502): what the scope's hover says of a block.
/// </param>
public readonly record struct CwScopeHop(double EnvelopeDb, double FloorDb, double ThresholdDb, bool Mark, double ShapeScore = double.NaN);

/// <summary>What one bin shows of bars.</summary>
/// <param name="Hz">The bin's pitch.</param>
/// <param name="Bars">Bars that ended in the last second, paired or not: noise makes a lone one now and then.</param>
/// <param name="Gaps">Gaps that ended in the last second between two bars at one level: a bar, a drop and a bar again.</param>
/// <param name="BarsLastSecond">Paired bars - marks - that ended in the last second.</param>
/// <param name="Keying">Whether two bars and the gap between them ended in the last second.</param>
/// <param name="BarDb">The paired bars' level over the last second, or NaN.</param>
/// <param name="GapDb">The gaps' level over the last second, or NaN.</param>
public sealed record CwBarBin(
    double Hz, int Bars, int Gaps, int BarsLastSecond, bool Keying, double BarDb, double GapDb);

/// <summary>What the detector says at its last hop.</summary>
/// <param name="EnvelopeDb">The level of the bin being watched, in dB below full scale.</param>
/// <param name="FloorDb">That bin's gap level over the last second, measured; NaN when it has none.</param>
/// <param name="ThresholdDb">Midway between the gap level and the bar level; drawn, and decides nothing.</param>
/// <param name="Mark">Whether a paired bar is up in that bin now.</param>
/// <param name="RunMs">How long the current mark or gap has lasted.</param>
/// <param name="PitchHz">While keying, the station's own bin: the top of the lobe over the watched bin's latest bar, held through the gaps; NaN otherwise (work instructions 488, 496).</param>
/// <param name="ContrastDb">While a mark is up, that bin's bar level over its gap level; NaN otherwise.</param>
/// <param name="PassbandLowHz">The lowest bin's edge of the sweep.</param>
/// <param name="PassbandHighHz">The highest.</param>
/// <param name="PassbandFromRig">Whether the edges came from the radio's pitch and filter.</param>
/// <param name="MarksLast4s">How many marks stood in the last four seconds: at the standing pitch while a sequence stands, at any pitch otherwise (work instruction 515).</param>
/// <param name="Keying">Whether a sequence the pattern gate stands has had a mark within the hold, at any pitch (work instruction 515, R114).</param>
/// <param name="ShapeScore">The shape score of the standing sequence the reading follows, nought to one; NaN while none stands (work instruction 519, R116).</param>
/// <param name="SequencesStanding">How many sequences stand now, the one followed among them (work instruction 519).</param>
/// <param name="ShapeLight">What the hold-still light shows (work instruction 521).</param>
/// <param name="ShapeForming">How many marks the fullest sequence not yet standing holds, within the last two seconds (work instruction 521).</param>
/// <param name="ShapeFill">How full the gauge is, nought to one (work instruction 522, task 3).</param>
public sealed record CwEnvelopeReading(
    double EnvelopeDb,
    double FloorDb,
    double ThresholdDb,
    bool Mark,
    double RunMs,
    double PitchHz,
    double ContrastDb,
    double PassbandLowHz,
    double PassbandHighHz,
    bool PassbandFromRig,
    int MarksLast4s,
    bool Keying = false,
    double ShapeScore = double.NaN,
    int SequencesStanding = 0,
    CwShapeLight ShapeLight = CwShapeLight.Listening,
    int ShapeForming = 0,
    double ShapeFill = 0)
{
    /// <summary>Nothing heard.</summary>
    public static CwEnvelopeReading None { get; } = new(
        double.NaN, double.NaN, double.NaN, false, 0, double.NaN, double.NaN,
        CwEnvelopeDetector.WholeBandLowHz, CwEnvelopeDetector.WholeBandHighHz, false, 0);
}

/// <summary>
/// **BARS, NOT WAVES: A KEYED SIGNAL IS A LEVEL THAT HOLDS, DROPS AND HOLDS AGAIN** (work
/// instruction 477 task 1, step 12 criterion 12.1 rewritten, R91, HM-DEC-186).
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-28**: *"We can tune out garbage by seeing if the signal stays at an
/// amplitude for a period. Real signals will be bars, not waves."*</para>
/// <para>**WHAT IT REPLACED.** Unit 476 summed the passband into one envelope, tracked a floor
/// under it and called a mark nine decibels over the floor. On the owner's rows of 2026-09-28
/// the floor sat at the middle of a keyed station's swing and the threshold above every mark:
/// a floor that averages a keyed signal is at the marks. **The floor and the margin are gone**
/// and nothing here compares a level with a fixed number of decibels.</para>
/// <para>**WHAT IT DOES.** Every <see cref="BinSpacingHz"/> across the passband - the radio's
/// CW pitch plus and minus half its filter, or the whole audio band when either is unknown -
/// each hop's level is read over a ten millisecond window. Consecutive hops whose levels stay
/// within <see cref="ToleranceDb"/> of their own mean - the wobble a tone at the bin's measured
/// contrast has, never less than <see cref="FlatToleranceDb"/> (R93) - are one **run**. A run at least
/// <see cref="ShortestBarMs"/> long that stands above the runs either side is a **bar**. The
/// hops between two bars, every one of them below both, are a **gap**. **A bin is keying when
/// it made two bars at one level with a gap between them in the last second** - a dit, a
/// space, a dit. Noise never holds a level, so it makes no bars at any loudness; a carrier is
/// one bar that never drops, so it makes no gap.</para>
/// <para>**THE PITCH IS THE BIN WITH THE MOST BARS**, and the scope watches it: its level is
/// the trace, its gap level the dashed line, the midpoint between gap and bar the solid line.
/// Both lines are measured from the bars and gaps and neither decides anything.</para>
/// <para>**IT DRIVES NOTHING.** Nothing in the decoder, the tracker or the survey reads it;
/// it is on the screen (R90).</para>
/// <para>**THREAD**: fed on whichever thread the audio arrives on and read from the UI's,
/// so every hop is committed and every read is taken under one lock.</para>
/// </remarks>
public sealed class CwEnvelopeDetector
{
    /// <summary>
    /// The least a hop may sit from its run's mean and still be the same level, in dB either
    /// way: the floor under <see cref="ToleranceDb"/>, which is what a run is actually held to.
    /// </summary>
    /// <remarks>
    /// <para>**THE FLOOR, FOR LOUD SIGNALS; THE TOLERANCE FOLLOWS THE SIGNAL** (work instruction
    /// 479, R93, HM-DEC-187). A tone read in a bin where the noise sits S decibels under it
    /// moves by 20·log10(1 + 10^(-S/20)) from hop to hop, as the noise adds to it or takes from
    /// it: 0.8 dB at 20, 1.4 at 15, 2.4 at 10. A run is held to that wobble at its bin's measured
    /// contrast, and never to less than this - so a loud bar may wobble little and a weak bar
    /// wobbles more, and one number for both is not a gate against the weak ones. There is no
    /// ceiling.</para>
    /// <para>**WHY ONE AND A HALF IS THE FLOOR, MEASURED ON SYNTHETIC AUDIO ONLY** (unit 477).
    /// Two was written first. Thirty seconds of seeded loud noise over the whole audio band read
    /// as keying in 11 of 1500 twenty-millisecond reads at two, and in none at one and a half:
    /// the per-hop level of noise in a bin scatters about 5.6 dB, and the narrower the band a
    /// run must stay in, the rarer noise holds it. Noise earns no more than the floor, because
    /// it has no contrast over itself to earn it.</para>
    /// <para>**WHY IT STOPPED BEING THE WHOLE RULE.** Held fixed, it admitted only signals
    /// wobbling less than one and a half - by the formula, signals more than about fourteen
    /// decibels over their gaps - and 477 named the cost: a tone 15 dB over the noise split its
    /// bars. On 2026-09-28 at 15:38 to 15:39 UTC the owner pressed *You're an idiot* on four
    /// stations he heard, 10 to 15 dB weaker than the morning's, and the bars found none of
    /// them. Those rows are the reason the formula is applied; they did not choose it.</para>
    /// <para>**WHAT S IS HERE - AUTHOR'S, OVERRULABLE.** A keying bin's bars' mean level over the
    /// loudest hop of its paired gaps in the last second, clear of the key edges: the formula's
    /// noise is the noise that adds to or takes from the tone at one hop, and a run has to hold
    /// through the loudest it meets. Over the gaps' power mean, the 15 dB tone reads about 16 dB,
    /// the formula gives 1.28 and the floor holds - that case did not move at all. Before a bin
    /// has a contrast of its own, S is the run's level over the loudest such gap any bin has, or
    /// where none has one, over the bin's own lowest level in the last second; a run at or under
    /// that has no contrast and holds the floor. When a bin's contrast is first measured, its
    /// stored history is read again at it, so the first dah is judged as the later ones are.</para>
    /// <para>**WHAT IT DID, SYNTHETIC AUDIO ONLY.** The 15 dB tone's unmarked key-down hops went
    /// from 51 of 208 to 40, every element from the second on unbroken; seeded loud noise still
    /// read keying in 0 of 1500. **A tone 10 dB over the noise still makes no bars**: at the
    /// floor it never makes a first pair, so its contrast is never measured, and the bin's own
    /// lowest level - a trough about twenty decibels under the noise - reads its contrast as
    /// thirty. Its test is red and is left red.</para>
    /// <para>**NO RECORDING AND NO VERDICT ROW CHOSE EITHER NUMBER.** The floor is 477's
    /// measurement on seeded noise; the formula is the physics of a tone plus noise.</para>
    /// </remarks>
    public const double FlatToleranceDb = 1.5;

    /// <summary>
    /// How far a hop may sit from its run's mean and still be the same level, for a bar S dB
    /// over its gap: max(<see cref="FlatToleranceDb"/>, 20·log10(1 + 10^(-S/20))).
    /// </summary>
    /// <param name="contrastDb">The run's level over its gap, S, in dB; NaN when not known.</param>
    /// <returns>
    /// The tolerance in dB either way; the floor when the contrast is not known, or is none - a
    /// run at or under the gap is the noise, and noise has no contrast over itself to earn more.
    /// </returns>
    public static double ToleranceDb(double contrastDb)
        => double.IsNaN(contrastDb) || contrastDb <= 0
            ? FlatToleranceDb
            : Math.Max(FlatToleranceDb, 20 * Math.Log10(1 + Math.Pow(10, -contrastDb / 20)));

    /// <summary>The shortest run that can be a bar, in ms: the shortest dit anyone sends.</summary>
    /// <remarks>
    /// **THE SURVEY'S OWN FLOOR**, moved here when the survey came out (work instruction 545): twenty-five
    /// milliseconds is forty-eight words a minute, the fastest the radio's own keyer goes (`14 0C`, p. 19-3). A
    /// run's length is the audio its windows cover: the first window's start to the last one's
    /// end.
    /// </remarks>
    public const double ShortestBarMs = 25;

    /// <summary>How far apart the bins are, in hertz: the keying meter's step.</summary>
    public const double BinSpacingHz = KeyingEnvelope.ToneStepHz;

    /// <summary>How recent two bars and their gap must be for a bin to be keying, in seconds.</summary>
    /// <remarks>
    /// **A DIT AND A DIT, NOT EIGHT MARKS AND THREE SECONDS** (work instruction 477). Two bars
    /// in a second is a dit, a space and a dit at any speed from five words a minute up, which
    /// is what the owner hears as somebody sending.
    /// </remarks>
    public const double KeyingSeconds = 1;

    /// <summary>How long keying is held at a station's pitch after its last bar, in seconds.</summary>
    /// <remarks>
    /// **THE LONGEST GAP IN ORDINARY SENDING** (work instruction 485, R97): a word gap is seven
    /// units, 840 ms at ten words a minute and a second at about eight, so a station still sending
    /// has put another bar down within it. Chosen from the timing Morse defines, not fitted to any
    /// recording.
    /// </remarks>
    public const double HoldSeconds = 1;

    /// <summary>How much history the scope holds and the mark count covers, in seconds.</summary>
    public const double HistorySeconds = 4;

    /// <summary>The low edge swept when the radio's pitch or filter is not known.</summary>
    /// <remarks>
    /// **THE WHOLE AUDIO BAND A RECEIVER PASSES, NOT A RADIO FACT.** Where the rig has not
    /// said its CW pitch and filter width, the bins run from here to
    /// <see cref="WholeBandHighHz"/>, so a station anywhere a voice filter would pass is
    /// still seen. Author's, overrulable.
    /// </remarks>
    public const double WholeBandLowHz = 100;

    /// <summary>The high edge swept when the radio's pitch or filter is not known.</summary>
    public const double WholeBandHighHz = 3000;

    private readonly object _gate = new();
    private readonly float[] _ring;
    private readonly float[] _window;
    private readonly float[] _hann;
    private readonly double _hannSum;
    private readonly int _minBarHops;
    private readonly int _keyingHops;

    private Bin[] _bins = Array.Empty<Bin>();
    private int _watched;

    /// <summary>Whether the shape picks the sequence the reading follows (work instruction 519); off, unit 515's loudest returns, for the tests' before.</summary>
    internal bool ShapePicks { get; set; } = true;

    /// <summary>
    /// The pitch the terminal is printing, or NaN when it prints nothing (work instruction 519, R116): while a
    /// standing sequence sits there the reading - its pitch, the light and the scope - follows it, so the
    /// screen shows the sender the terminal reads. Read on the audio thread; the reader writes it there too.
    /// </summary>
    public Func<double>? PrintedPitch { get; set; }

    /// <summary>
    /// The pitch of the sender the terminal has qualified and is waiting out its first word gap to print, or NaN (work
    /// instruction 535, HM-DEC-239): with <see cref="PrintedPitch"/>, all the hold-still light may claim. Read on the
    /// audio thread.
    /// </summary>
    public Func<double>? WaitingPitch { get; set; }

    // The shape score of the standing sequence the reading follows (work instruction 519).
    private double _readingShape = double.NaN;

    /// <summary>The highest shape score any standing sequence has had: for the tests' report (work instruction 519).</summary>
    internal double HighestStandingShape { get; private set; }

    /// <summary>The shape that scored <see cref="HighestStandingShape"/>, term by term: for the tests' report (work instruction 520).</summary>
    internal CwSequenceShape? HighestStandingShapeOf { get; private set; }
    private int _ringWrite;

    // The raw audio of the last two seconds, by absolute sample index: where a mark's pitch is measured over its
    // own samples (work instruction 522).
    private readonly float[] _raw;
    private int _ringFill;
    private int _hopFill;
    private long _hop;

    // Every sample handed in since the detector was made: the audio clock the decoder stamps
    // its characters on, which a new passband does not reset (work instruction 487).
    private long _samplesSeen;

    // The blocks called at the watched pitch, on that clock, in seconds, by where each began.
    private readonly SortedDictionary<long, (double From, double To)> _called = new();

    // Every mark called at any pitch, with its pitch and level, in the order called (work
    // instruction 490, R103), and the last sequence number handed out.
    private readonly List<CwMark> _marks = new();
    private long _markSequence;

    // Every candidate the single-mark gates passed, stood or not, for the one-call rule, and the
    // pattern gate that decides which stand (work instruction 507).
    private readonly List<CwMark> _candidates = new();
    private readonly CwPatternGate _pattern = new();
    private long _candidateSequence;
    private double _lowHz = double.NaN;
    private double _highHz = double.NaN;
    private bool _fromRig;
    private double _loudestGapDb = double.NaN;
    private CwEnvelopeReading _reading = CwEnvelopeReading.None;

    private IAudioSource? _attached;

    // **THE SENDER'S OWN WINDOW** (work instruction 529, HM-DEC-233): once a sender stands, its envelope is taken at its
    // own pitch through a filter that fits its dit, and its marks are found there. Null while no sender stands.
    private readonly CwSenderLane _lane;
    private Bin? _laneBin;
    private long _laneScan;
    private long _laneSpan = -1;
    private int _laneId = -1;
    private int _laneCount;
    private long _laneFromHop;
    private double _laneFromSeconds;
    private int _laneEdgeHops;
    private int _laneRiseHops;

    /// <summary>
    /// Whether a standing sender is read through its own window (work instruction 529); on by default. Off, every mark is
    /// found on the grid as before, so the difference can be counted.
    /// </summary>
    internal bool OwnWindow { get => _ownWindow && CwRules.On(CwRules.OwnWindow); set => _ownWindow = value; }

    // Set by tests; the rule is on unless a test turns it off (work instruction 534).
    private bool _ownWindow = true;

    /// <summary>The pitch the sender's own window is mixed at, or NaN while none is open: for the tests' report (work instruction 529).</summary>
    internal double OwnWindowPitchHz => _laneBin is null ? double.NaN : _lane.PitchHz;

    /// <summary>The sender's own window as last tuned, open or not: for the tests' report (work instruction 529).</summary>
    internal CwSenderLane OwnWindowLane => _lane;

    /// <summary>Creates a detector for one sample rate.</summary>
    /// <param name="sampleRate">Samples per second of the audio it will be fed.</param>
    public CwEnvelopeDetector(int sampleRate)
    {
        SampleRate = Math.Max(1_000, sampleRate);

        // **THE DECODER'S OWN HOP**, five milliseconds, so a hop here and a hop in
        // `CwCharacter.HopMilliseconds` are the same slice of time.
        HopSamples = Math.Max(4, SampleRate / 200);

        // Each bin's level is read over two hops, ten milliseconds: a 25 ms dit still holds
        // three or four whole windows, and the window is short enough that its edges smear a
        // keying edge by one hop and no more.
        EnvelopeWindowSamples = 2 * HopSamples;

        _ring = new float[EnvelopeWindowSamples];
        _raw = new float[2 * sampleRate];
        _lane = new CwSenderLane(SampleRate);
        _window = new float[EnvelopeWindowSamples];
        _hann = new float[EnvelopeWindowSamples];

        for (var i = 0; i < EnvelopeWindowSamples; i++)
        {
            _hann[i] = (float)(0.5 - (0.5 * Math.Cos(2 * Math.PI * (i + 0.5) / EnvelopeWindowSamples)));
            _hannSum += _hann[i];
        }

        // A run of n hops covers (n - 1) hops plus one window of audio.
        _minBarHops = Math.Max(1, (int)Math.Ceiling(((ShortestBarMs / HopMs) - (EnvelopeWindowSamples / (double)HopSamples)) + 1));
        _keyingHops = (int)Math.Round(KeyingSeconds * 1000 / HopMs);
        HistoryHops = (int)Math.Ceiling(HistorySeconds * 1000 / HopMs);
        SettleHops = (int)Math.Round((ShortestBarMs + (1000.0 * EnvelopeWindowSamples / SampleRate)) / HopMs);

        SetPassband(null, null);
    }

    /// <summary>
    /// How many hops after its rise a mark's top may settle before it is held to one level: the shortest dit anyone
    /// sends and the detector's own window, 35 ms, seven hops at five milliseconds (work instruction 525, task 2).
    /// </summary>
    /// <remarks>
    /// The IC-7300's AGC attack time is not stated in the Full Manual, `A7292-4EX-6`, so the figure is not taken from
    /// the radio. It is the author's, from what a keyed tone has to do: an AGC that has not settled by the end of the
    /// shortest dit, <see cref="ShortestBarMs"/>, leaves no dit with a level at all, and the window smears the rise
    /// over one more window's length after that. A slower AGC would still break a dah, and this does not claim
    /// otherwise.
    /// </remarks>
    public int SettleHops { get; }

    /// <summary>Samples per second.</summary>
    public int SampleRate { get; }

    /// <summary>Samples per hop.</summary>
    public int HopSamples { get; }

    /// <summary>One hop, in milliseconds.</summary>
    public double HopMs => 1000.0 * HopSamples / SampleRate;

    /// <summary>Samples each bin's level is read over.</summary>
    public int EnvelopeWindowSamples { get; }

    /// <summary>How many hops the history holds.</summary>
    public int HistoryHops { get; }

    /// <summary>What the detector says at its last hop.</summary>
    public CwEnvelopeReading Reading
    {
        get
        {
            lock (_gate)
            {
                return _reading;
            }
        }
    }

    /// <summary>
    /// Where the bins run: the radio's CW pitch plus and minus half its filter width, or the
    /// whole audio band where either is unknown.
    /// </summary>
    /// <param name="pitchHz">The radio's CW pitch, or null.</param>
    /// <param name="widthHz">The radio's filter width, or null.</param>
    public void SetPassband(double? pitchHz, double? widthHz)
    {
        lock (_gate)
        {
            double low;
            double high;
            bool fromRig;

            if (pitchHz is > 0 and var pitch && widthHz is > 0 and var width)
            {
                low = Math.Max(0, pitch - (width / 2));
                high = Math.Min(SampleRate / 2.0, pitch + (width / 2));
                fromRig = true;
            }
            else
            {
                low = WholeBandLowHz;
                high = Math.Min(SampleRate / 2.0, WholeBandHighHz);
                fromRig = false;
            }

            if (low == _lowHz && high == _highHz && fromRig == _fromRig)
            {
                return;
            }

            _lowHz = low;
            _highHz = high;
            _fromRig = fromRig;

            // A new passband is new bins; what the old ones held is about other pitches.
            var first = Math.Max(1, (int)Math.Ceiling(low / BinSpacingHz));
            var last = (int)Math.Floor(high / BinSpacingHz);

            if (last < first)
            {
                first = last = Math.Max(1, (int)Math.Round((low + high) / 2 / BinSpacingHz));
            }

            _bins = Enumerable.Range(first, last - first + 1)
                .Select(k => new Bin(k * BinSpacingHz, SampleRate, HistoryHops + (2 * _keyingHops), _keyingHops) { SettleHops = SettleHops, RiseHops = (EnvelopeWindowSamples / HopSamples) + 1 })
                .ToArray();
            _laneBin = null;
            _laneId = -1;
            _watched = _bins.Length / 2;
            _hop = 0;
            _loudestGapDb = double.NaN;
            _reading = CwEnvelopeReading.None with
            {
                PassbandLowHz = _lowHz,
                PassbandHighHz = _highHz,
                PassbandFromRig = _fromRig,
            };
        }
    }

    /// <summary>How long the called blocks are kept, in seconds: far longer than the settled pass runs behind.</summary>
    public const double CalledSeconds = 60;

    /// <summary>
    /// How many blocks this detector called whose middle lies between two moments, on the audio
    /// clock of every sample it was handed (work instruction 487, R99).
    /// </summary>
    /// <param name="fromSeconds">The earlier moment.</param>
    /// <param name="toSeconds">The later moment.</param>
    /// <returns>The count.</returns>
    public int BlocksBetween(double fromSeconds, double toSeconds)
    {
        lock (_gate)
        {
            return _called.Values.Count(b => (b.From + b.To) / 2 >= fromSeconds && (b.From + b.To) / 2 <= toSeconds);
        }
    }

    /// <summary>
    /// The marks called since a sequence number, with their pitch, level and length, at every
    /// pitch in the passband (work instruction 490, R103).
    /// </summary>
    /// <param name="sequence">The last sequence number the reader has; nought for all kept.</param>
    /// <returns>The marks after it, in the order called, and the detector's audio clock now.</returns>
    /// <remarks>
    /// <para>**WHAT WAS ALREADY HERE AND WHAT HAD TO BE ADDED.** Every bin already called its own
    /// bars, paired them only at one level (<see cref="FlatToleranceDb"/>), and kept each hop's
    /// level for <see cref="HistorySeconds"/>; and <see cref="BlocksBetween"/> kept the watched
    /// bin's bars on the audio clock (work instruction 487). What was handed out was a count of
    /// the watched bin's blocks, with no pitch and no level. Added: every completed bar in every
    /// bin becomes one mark, placed at the peak of its tone's lobe - the neighboring bin with the
    /// higher mean level over the mark's own hops, climbed until none is higher - so a tone that
    /// lights bins two hundred hertz either side is one mark at its own pitch, not sixteen. A
    /// mark overlapping one already called within one bin of its pitch is that mark again and
    /// is not called twice. Kept for <see cref="CalledSeconds"/>.</para>
    /// </remarks>
    public CwMarkBatch MarksSince(long sequence)
    {
        lock (_gate)
        {
            return new CwMarkBatch(
                _marks.Where(m => m.Sequence > sequence).ToArray(),
                _samplesSeen / (double)SampleRate,
                _laneBin is null ? 0 : _lane.DelaySeconds + ((_laneEdgeHops - EdgeHops) * HopMs / 1000));
        }
    }


    /// <summary>How many marks have stood and been handed on since the detector was made (work instruction 507).</summary>
    public int StoodCount
    {
        get
        {
            lock (_gate)
            {
                return (int)_markSequence;
            }
        }
    }

    /// <summary>Listen to a source beside whatever else listens to it. Replaces any previous one.</summary>
    /// <param name="source">The source, or null to stop listening.</param>
    public void Listen(IAudioSource? source)
    {
        if (ReferenceEquals(_attached, source))
        {
            return;
        }

        if (_attached is not null)
        {
            _attached.SamplesReady -= OnSamples;
        }

        _attached = source;

        if (_attached is not null)
        {
            _attached.SamplesReady += OnSamples;
        }
    }

    /// <summary>Feed audio, in any size; it is walked a hop at a time.</summary>
    /// <param name="samples">Mono samples at <see cref="SampleRate"/>.</param>
    public void Process(ReadOnlySpan<float> samples)
    {
        lock (_gate)
        {
            foreach (var s in samples)
            {
                _ring[_ringWrite] = s;
                _raw[(int)(_samplesSeen % _raw.Length)] = s;
                _ringWrite = (_ringWrite + 1) % _ring.Length;
                _samplesSeen++;
                _ringFill = Math.Min(_ringFill + 1, _ring.Length);

                if (_laneBin is not null)
                {
                    _lane.Push(s);
                }

                if (++_hopFill == HopSamples)
                {
                    _hopFill = 0;

                    if (_ringFill >= EnvelopeWindowSamples)
                    {
                        Hop();
                    }
                }
            }
        }
    }

    /// <summary>What every bin in the passband shows of bars, lowest pitch first.</summary>
    /// <returns>One entry per bin.</returns>
    public IReadOnlyList<CwBarBin> Bins()
    {
        lock (_gate)
        {
            return _bins.Select(b => Summary(b, Evaluate(b))).ToArray();
        }
    }

    /// <summary>The last four seconds at the standing pitch, oldest first: each hop's level, and whether a mark that stood covers it.</summary>
    /// <returns>One entry per hop.</returns>
    /// <remarks>
    /// **THE SCOPE READS WHAT STOOD** (work instruction 515, R114, HM-DEC-219). A hop is marked where a mark
    /// that stood near this pitch covers it, carrying that mark's shape score (work instruction 502); the bin's
    /// own paired bars no longer decide it, because the station's own bin pairs them rarely (unit 496) and the
    /// shoulder the old watched bin sat on is not watched any more.
    /// </remarks>
    public IReadOnlyList<CwScopeHop> History()
    {
        lock (_gate)
        {
            var fill = (int)Math.Min(_hop, HistoryHops);
            var copy = new CwScopeHop[fill];

            if (fill == 0)
            {
                return copy;
            }

            var bin = _bins[_watched];
            var oldest = _hop - fill;
            var nowSeconds = _samplesSeen / (double)SampleRate;

            for (var i = 0; i < fill; i++)
            {
                var hop = oldest + i;

                // On the clock the mark was called on: a hop's middle, seconds back from now as many hops as it is old.
                var middle = nowSeconds - ((_hop - 1 - hop + 0.5) * HopMs / 1000);
                var called = _marks.LastOrDefault(k => k.FromSeconds <= middle && k.ToSeconds >= middle && Math.Abs(k.PitchHz - bin.Hz) <= 2 * BinSpacingHz);

                copy[i] = new CwScopeHop(bin.Level(hop), _reading.FloorDb, _reading.ThresholdDb, called is not null, called?.Shape?.Score ?? double.NaN);
            }

            return copy;
        }
    }

    /// <summary>The tone line the scope shows.</summary>
    /// <param name="reading">A reading.</param>
    /// <returns><c>tone 750 Hz, 24 dB over the band</c>, or <c>no tone</c> when no mark is up.</returns>
    public static string ToneLine(CwEnvelopeReading reading)
    {
        ArgumentNullException.ThrowIfNull(reading);

        return reading.Mark && !double.IsNaN(reading.PitchHz)
            ? string.Create(
                CultureInfo.InvariantCulture,
                $"tone {reading.PitchHz:0} Hz, {reading.ContrastDb:0} dB over the band")
            : "no tone";
    }

    private void OnSamples(in AudioChunk chunk) => Process(chunk.Samples);

    private void Hop()
    {
        var start = _ringWrite;

        for (var i = 0; i < _window.Length; i++)
        {
            _window[i] = _ring[(start + i) % _ring.Length] * _hann[i];
        }

        var hop = _hop++;

        // **THE TOLERANCE FOLLOWS THE SIGNAL** (R93): each hop joins its run or starts a new one
        // under the wobble a tone at the bar's contrast has. Where the bin is keying, that is
        // its bars' level over its gaps' loudest hop, measured at the last hop. Where it is not
        // yet - a station's first bar, before any gap of its own - it is the run's level, with
        // this hop in it, over the loudest gap any bin measured in the last second, or where no
        // bin has one, over this bin's own lowest level in the last second. The run's level and
        // not the hop's: a hop fallen to the noise is judged at the bar's contrast, so it cannot
        // widen the bar's tolerance and join it; and a run at or under the gap holds the floor.
        foreach (var bin in _bins)
        {
            var db = Level(bin);
            var under = !double.IsNaN(_loudestGapDb) ? _loudestGapDb : bin.LowestLastSecond(hop, db);

            bin.Add(hop, db, bin.ContrastDb, under, !double.IsNaN(bin.ContrastDb) ? bin.ContrastDb : db - _loudestGapDb);
            bin.Prune(hop - HistoryHops - _keyingHops);
        }

        // Every bin is evaluated: its bars, their pairs and the gaps' level, which the mark rules read.
        var evals = new Evaluation[_bins.Length];
        _loudestGapDb = double.NaN;

        for (var i = 0; i < _bins.Length; i++)
        {
            var e = evals[i] = Evaluate(_bins[i]);


            // The gap a run is measured over is the loudest hop of the gaps: the formula's noise
            // is the noise that adds to or takes from the tone at one hop, and a run has to hold
            // through the loudest it meets, not the average one.
            var gap = !double.IsNaN(e.LoudestGapDb) ? e.LoudestGapDb : e.GapDb;

            var contrast = e.Keying && !double.IsNaN(e.BarDb) && !double.IsNaN(gap)
                ? e.BarDb - gap
                : double.NaN;

            // A station's first bars were read before its contrast was known, at whatever
            // tolerance the estimate gave; once it is measured, the bin's history is read again
            // at the tolerance it has earned, so its first dah is judged as its later ones are.
            if (double.IsNaN(_bins[i].ContrastDb) && !double.IsNaN(contrast))
            {
                _bins[i].ContrastDb = contrast;
                _bins[i].Reread(Math.Max(0, hop - HistoryHops - _keyingHops), hop);
                evals[i] = e = Evaluate(_bins[i]);
            }

            _bins[i].ContrastDb = contrast;

            if (!double.IsNaN(gap) && !(gap <= _loudestGapDb))
            {
                _loudestGapDb = gap;
            }
        }

        // **THE SENDER'S OWN WINDOW** (work instruction 529): its level this hop, kept as a bin keeps its levels.
        if (_laneBin is { } lane)
        {
            lane.Add(hop, _lane.LevelDb, double.NaN, double.NaN, double.NaN);
            lane.Prune(hop - HistoryHops - _keyingHops);
        }

        // **THE SHAPE IS FOUND WHEREVER IT APPEARS; THE WATCHED BIN RETIRES** (work instruction 515,
        // R114, HM-DEC-219). Tim: *"Pitch almost doesn't matter. It's shape."* Every bin on every hop
        // already offers its bars as marks and the pattern gate already stands them from any pitch; now
        // the verdict reads the same thing. It is keying when a sequence stands with a mark within
        // the hold, and its pitch is that sequence's, the loudest where several stand. Nothing points
        // it and nothing follows: unit 476's survey choice, 496's verdict bin, 507's follow-the-reader
        // and 514's follow-the-meter are gone.
        var nowSeconds = _samplesSeen / (double)SampleRate;

        // **ONE FRONT END** (work instruction 530, HM-DEC-234): the grid finds the marks; shape-first, its rectangles fitted to
        // the whole passband, came out when it read the owner's recording as nothing. The tag before-one-front-end holds it.
        // **NO RECTANGLE FIT** (work instruction 534, HM-DEC-238): the fit that filled what the per-hop tests left came out
        // when the owner's recordings read as well without it. The tag before-scoreboard holds it.
        CallMarks(evals, hop, nowSeconds);

        // **SHAPE PICKS THE SENDER; LOUDNESS PICKS NOTHING** (work instruction 519, R116, HM-DEC-223). The
        // reading - its pitch, the light and the scope - follows the standing sequence whose marks and gaps
        // sound most like code, at every hop. Unit 515 took the loudest. The terminal holds its sender through
        // the sending, and the reading follows what it prints (<see cref="PrintedPitch"/>) while that stands,
        // and the best shape otherwise.
        var standing = _pattern.Standing(nowSeconds, HoldSeconds);
        var keying = standing.Count > 0;
        var printed = PrintedPitch?.Invoke() ?? double.NaN;
        var chosen = standing.FirstOrDefault(s => ShapePicks && double.IsFinite(printed) && Math.Abs(s.PitchHz - printed) <= BinSpacingHz)
            ?? standing
            .OrderByDescending(s => ShapePicks ? s.Shape.Score : s.LevelDb).FirstOrDefault();

        _readingShape = chosen?.Shape.Score ?? double.NaN;

        FollowSender(chosen, hop);

        foreach (var s in standing)
        {
            if (s.Shape.Score > HighestStandingShape)
            {
                HighestStandingShape = s.Shape.Score;
                HighestStandingShapeOf = s.Shape;
            }
        }

        // **THE HOLD-STILL LIGHT** (work instruction 521, HM-DEC-225): green while a sender is printed and keying,
        // green while a sequence stands, amber while one is forming with marks in the last two seconds, dark else.
        var forming = _pattern.Forming(nowSeconds, CwShapeLights.FormingSeconds);

        // **THE LIGHT CLAIMS NO MORE THAN THE PRINTER** (work instruction 535, HM-DEC-239; the owner's answer, option A): green
        // only while the gate would print the sender - `reading` while it prints one, `shape found` while one has qualified
        // and waits out its first word gap - and only while a mark has stood within the hold. It read a sequence's shape
        // score before, and with the standing line gone a noise sequence scored over 0.2 and the light said hold here for
        // 377 steps while nothing printed. Amber while marks stand or form; no number of its own.
        var waiting = WaitingPitch?.Invoke() ?? double.NaN;
        var light = keying && double.IsFinite(printed) ? CwShapeLight.Reading
            : keying && double.IsFinite(waiting) ? CwShapeLight.Found
            : keying || forming > 0 ? CwShapeLight.Forming
            : CwShapeLight.Listening;
        var sure = light is CwShapeLight.Reading or CwShapeLight.Found;

        if (keying && !sure)
        {
            forming = CwPatternGate.MarksToStand;
        }

        var pitchHz = chosen is not null
            ? Math.Round(chosen.PitchHz / BinSpacingHz) * BinSpacingHz
            : double.NaN;

        // The bin the scope's trace and the reading's levels come from: the one nearest the standing
        // pitch, derived from it and never steered; where nothing stands it stays where it was.
        if (keying)
        {
            var nearest = 0;

            for (var i = 1; i < _bins.Length; i++)
            {
                if (Math.Abs(_bins[i].Hz - pitchHz) < Math.Abs(_bins[nearest].Hz - pitchHz))
                {
                    nearest = i;
                }
            }

            _watched = nearest;
        }

        var watched = _bins[_watched];
        var eval = evals[_watched];
        var open = watched.Open!;
        var up = eval.Marked.Count > 0 && eval.Marked[^1].End == hop;

        // **THE BLOCKS THIS DETECTOR CALLED, KEPT ON THE DECODER'S CLOCK** (work instruction 487,
        // R99): a hop's time is the end of the audio it read, so a span's hops are placed back
        // from now. A span read again at a later hop replaces the first reading of it.
        foreach (var m in eval.Marked)
        {
            var from = nowSeconds - ((hop - m.Start + 1) * HopMs / 1000);
            var to = nowSeconds - ((hop - m.End) * HopMs / 1000);
            var key = (long)Math.Round(from * 1000 / HopMs);

            _called.Remove(key - 1);
            _called.Remove(key + 1);
            _called[key] = (from, to);
        }

        foreach (var old in _called.Where(b => b.Value.To < nowSeconds - CalledSeconds).Select(b => b.Key).ToList())
        {
            _called.Remove(old);
        }

        var gapDb = eval.GapDb;
        var barDb = eval.BarDb;
        var midDb = double.IsNaN(gapDb) || double.IsNaN(barDb) ? double.NaN : (gapDb + barDb) / 2;

        double runMs;

        if (up)
        {
            runMs = open.Count * HopMs;
        }
        else
        {
            var lastEnd = eval.Marked.Count > 0 ? eval.Marked[^1].End : -1;
            runMs = Math.Min(hop - lastEnd, HistoryHops) * HopMs;
        }

        _reading = new CwEnvelopeReading(
            watched.Level(hop),
            gapDb,
            midDb,
            up,
            runMs,
            pitchHz,
            up && !double.IsNaN(gapDb) ? barDb - gapDb : double.NaN,
            _lowHz,
            _highHz,
            _fromRig,

            // The marks that stood in the last four seconds: at the standing pitch while one stands,
            // which is the printed sender's while it is printed, and at any pitch otherwise.
            _marks.Count(k => k.ToSeconds > nowSeconds - HistorySeconds && (!keying || Math.Abs(k.PitchHz - pitchHz) <= BinSpacingHz)),
            keying,
            _readingShape,
            standing.Count,
            light,
            forming,
            CwShapeLights.Fill(light, forming, _readingShape));
    }

    /// <summary>
    /// **A STANDING SENDER IS MEASURED THROUGH ITS OWN WINDOW** (work instruction 529, HM-DEC-233): the window opens on the
    /// standing sender the terminal prints; while that sender is silent it stays on it, for as long as the gate keeps
    /// it; and it closes when the gate has let it go.
    /// </summary>
    /// <remarks>
    /// Its pitch is measured again from the sender's own samples, and its dit read again, each time the sender takes a
    /// mark, so it follows a sender whose pitch or speed moves as unit 524 follows one on its own bin. Before any sender
    /// stands nothing here runs, and the per-bin path finds marks exactly as before.
    /// </remarks>
    private void FollowSender(CwPatternGate.StandingSequence? chosen, long hop)
    {
        if (!OwnWindow)
        {
            _laneBin = null;
            _laneId = -1;
            return;
        }

        // **ONLY THE SENDER THE TERMINAL PRINTS** (work instruction 529): a sequence of noise can stand now and then, and a
        // narrow low-pass smooths noise into humps a dit long, so the window opens on a sender the reader has taken as one,
        // and on nothing it has not. Noise never prints, so noise is read as before.
        var printed = PrintedPitch?.Invoke() ?? double.NaN;
        var id = chosen is not null && double.IsFinite(printed) && Math.Abs(chosen.PitchHz - printed) <= BinSpacingHz ? chosen.Id : _laneId;

        if (id < 0 || _pattern.Sender(id) is not { } sender || !(sender.DitSeconds > 0))
        {
            _laneBin = null;
            _laneId = -1;
            return;
        }

        if (_laneBin is not null && id == _laneId && sender.Count == _laneCount)
        {
            return;
        }

        var pitch = OwnPitch(sender.Recent, sender.PitchHz);

        if (_laneBin is not null && id == _laneId)
        {
            _laneCount = sender.Count;
            _lane.Tune(pitch, sender.DitSeconds);
            return;
        }

        OpenLane(id, sender.Count, pitch, sender.DitSeconds, hop);
    }

    /// <summary>
    /// Open the sender's own window: tune it, and read the last two seconds of audio through it, so its first marks are
    /// judged against the level and the gaps that came before them. Only marks ending after this hop are called from it.
    /// </summary>
    private void OpenLane(int id, int count, double pitchHz, double ditSeconds, long hop)
    {
        _lane.Reset();
        _lane.Tune(pitchHz, ditSeconds);
        _laneId = id;
        _laneCount = count;
        _laneFromHop = hop;
        _laneFromSeconds = _samplesSeen / (double)SampleRate;

        // The filter's rise in hops sets how far a key's edge is smeared on this trace, as the window's two hops do on a bin; an
        // edge has fallen to a mark's half height in dB, its whole step, within two rises, which is how far its edges are read.
        _laneRiseHops = Math.Max(1, (int)Math.Ceiling(_lane.RiseSeconds * 1000 / HopMs));
        _laneEdgeHops = Math.Max(EdgeHops, (2 * _laneRiseHops) + 1);

        var lane = new Bin(pitchHz, SampleRate, HistoryHops + (2 * _keyingHops), _keyingHops);
        var from = Math.Max(0, _samplesSeen - _raw.Length);

        for (var n = from; n < _samplesSeen; n++)
        {
            _lane.Push(_raw[(int)(n % _raw.Length)]);

            if ((n + 1) % HopSamples == 0 && ((n + 1) / HopSamples) - 2 is var h && h >= 0 && h <= hop)
            {
                lane.Add(h, _lane.LevelDb, double.NaN, double.NaN, double.NaN);
            }
        }

        // The hops the backfill covers are scanned again, so a mark under way when the window opened is found whole; only
        // one ending after the window opened is called from it.
        _laneScan = Math.Max(0, hop - (long)Math.Round(KeyingSeconds * 1000 / HopMs));
        _laneSpan = -1;
        _laneBin = lane;
    }

    /// <summary>
    /// The sender's pitch, measured on its own samples (work instruction 522's centroid): over each of its last four marks
    /// still in the raw audio, the excess peak within 50 Hz of its pitch nearest it, and the median of those. Its marks'
    /// mean pitch where none can be read.
    /// </summary>
    private double OwnPitch(IReadOnlyList<CwMark> recent, double pitchHz)
    {
        var peaks = new List<double>();
        var hopSeconds = HopSamples / (double)SampleRate;

        foreach (var m in recent.TakeLast(4))
        {
            var start = (long)Math.Round(m.FromSeconds / hopSeconds) - 1;
            var end = (long)Math.Round(m.ToSeconds / hopSeconds) - 1;

            if (start < 1 || end <= start)
            {
                continue;
            }

            var near = SamplePeaks(pitchHz - (2 * BinSpacingHz), pitchHz + (2 * BinSpacingHz), start, end);

            if (near.Count > 0)
            {
                peaks.Add(near.MinBy(p => Math.Abs(p - pitchHz)));
            }
        }

        return peaks.Count == 0 ? pitchHz : peaks.OrderBy(p => p).ElementAt(peaks.Count / 2);
    }

    /// <summary>Whether a grid mark sits where the sender's own window now calls marks: within two bins of it, ending after it opened.</summary>
    private bool AtOwnWindow(double pitchHz, double toSeconds)
        => _laneBin is not null && Math.Abs(pitchHz - _lane.PitchHz) <= 2 * BinSpacingHz && toSeconds > _laneFromSeconds;

    /// <summary>Half amplitude, in dB of power: where a key's edge crosses on an envelope that rises and falls about it.</summary>
    private static readonly double HalfAmplitudeDb = 20 * Math.Log10(0.5);

    /// <summary>
    /// **THE SENDER'S MARKS, FOUND ON ITS OWN ENVELOPE** (work instruction 529): every stretch where the sender's own
    /// window stands over half the sender's amplitude, put through the tests a bin's bar meets - its length, the key-up,
    /// its own height over the gaps beside it, the edges, the narrowness beside it, and the shape, whose flatness term is
    /// the flatness test - and timed where it crosses half its own amplitude, less the filter's delay.
    /// </summary>
    /// <remarks>
    /// <para>**WHY A STRETCH OVER HALF AMPLITUDE AND NOT A RUN.** The per-bin path finds a bar as a run held to one level,
    /// standing wholly above the runs either side, which suits a window whose edges take two hops. The sender's window rises
    /// over its filter's rise, three or four hops, and the last hops of a rise sit inside the top's own range, so a run
    /// built hop by hop does not stand wholly above them and a whole dit is refused (the A of CHAT, measured). A low-pass of
    /// a keyed rectangle crosses half its amplitude at the key's edges, late by the filter's own delay, so the stretch
    /// over half the sender's level is the mark.</para>
    /// <para>The line is half the amplitude of the sender's own level, from the gate's mean of its recent marks. Each
    /// stretch is then timed at half its own top, so a mark a little quieter than the sender's is timed as truly as one
    /// at its level.</para>
    /// </remarks>
    private void LaneMarks(long hop, double nowSeconds)
    {
        if (_pattern.Sender(_laneId) is not { } sender || !double.IsFinite(sender.LevelDb))
        {
            return;
        }

        var lane = _laneBin!;

        // Half the sender's amplitude, less the wobble a flat top has (FlatToleranceDb): a mark of its own read a wobble quieter,
        // or a dit sent quieter that the gate takes by the sender's pattern, still crosses it.
        var line = sender.LevelDb + HalfAmplitudeDb - FlatToleranceDb;
        var last = hop - _laneEdgeHops;

        for (var h = _laneScan; h <= last; h++)
        {
            var above = lane.Level(h) >= line;

            if (above && _laneSpan < 0)
            {
                _laneSpan = h;
            }
            else if (!above && _laneSpan >= 0)
            {
                OfferLaneSpan(_laneSpan, h - 1, hop, nowSeconds);
                _laneSpan = -1;
            }
        }

        _laneScan = Math.Max(_laneScan, last + 1);
    }

    /// <summary>One stretch of the sender's window over half its amplitude, offered as a candidate if it passes a mark's tests.</summary>
    private void OfferLaneSpan(long a, long b, long hop, double nowSeconds)
    {
        var lane = _laneBin!;
        var hopSeconds = HopMs / 1000;

        if (b <= _laneFromHop)
        {
            return;
        }

        double Time(long h) => nowSeconds - ((hop - h) * hopSeconds);

        // Its top: in from either crossing by half the filter's rise, where the edge has finished rising.
        var inset = (_laneRiseHops + 1) / 2;
        var start = a + inset;
        var end = b - inset;

        if (end < start)
        {
            start = end = (a + b) / 2;
        }

        var level = MeanLevel(lane, start, end);
        var half = level + HalfAmplitudeDb;
        var from0 = start;

        while (from0 > 0 && from0 > start - _laneEdgeHops && lane.Level(from0 - 1) >= half)
        {
            from0--;
        }

        var to0 = end;

        while (to0 < hop && to0 < end + _laneEdgeHops && lane.Level(to0 + 1) >= half)
        {
            to0++;
        }

        double Between(double under, double over) => over > under ? Math.Clamp((half - under) / (over - under), 0, 1) : 0;

        var rise = from0 > 0 ? Time(from0 - 1) + (Between(lane.Level(from0 - 1), lane.Level(from0)) * hopSeconds) : Time(from0);
        var fall = to0 < hop ? Time(to0) + ((1 - Between(lane.Level(to0 + 1), lane.Level(to0))) * hopSeconds) : Time(to0);
        var from = rise - _lane.DelaySeconds;
        var to = fall - _lane.DelaySeconds;

        // The per-bin path's own shortest bar, as long as it reports one: a run of that many hops is that many hops long.
        if (to - from < _minBarHops * HopMs / 1000)
        {
            return;
        }

        var own = OwnContrast(lane, start, end, level, hop, (2 * inset) + 1, (2 * inset) + (2 * EdgeHops));

        if (MarksNeedEdges && !HasEdges(lane, start, end, level, EdgeDepth(own.ContrastDb), _laneEdgeHops))
        {
            return;
        }

        var grid = Nearest(_lane.PitchHz);

        // Narrowness (work instruction 541, HM-DEC-245).
        if (MarksNeedNarrowness && CwRules.On(CwRules.Narrowness) && (!IsNarrow(grid, start, end, level, own.ContrastDb, hop) || !IsToneHere(from, to, _lane.PitchHz)))
        {
            return;
        }

        var shape = Shape(lane, grid, start, end, level, own, hop, _laneEdgeHops, _laneRiseHops);

        if (MarksNeedShape && shape.Score < ShapeThreshold)
        {
            return;
        }

        var pitch = _lane.PitchHz;

        if (MarksNeedOneCall && AlreadyCalled(from, to, pitch, level))
        {
            return;
        }

        var candidate = new CwMark(
            ++_candidateSequence, from, to, pitch, level, double.IsFinite(own.ContrastDb) ? own.ContrastDb : double.NaN)
        {
            Keyed = true,
            Shape = shape,
            OwnContrastDb = own.ContrastDb,
            Stood = false,
        };

        _candidates.Add(candidate);

        foreach (var stood in MarksNeedPattern ? _pattern.Offer(candidate) : new[] { candidate })
        {
            _marks.Add(stood with { Sequence = ++_markSequence, Stood = true, Keyed = true });
        }
    }

    /// <summary>One bin's level this hop, as mean square: a full-scale sine reads -3 dB.</summary>
    private double Level(Bin bin)
    {
        double s1 = 0;
        double s2 = 0;

        foreach (var x in _window)
        {
            var s0 = x + (bin.Coefficient * s1) - s2;
            s2 = s1;
            s1 = s0;
        }

        var power = (s1 * s1) + (s2 * s2) - (bin.Coefficient * s1 * s2);
        var meanSquare = 2 * power / (_hannSum * _hannSum);

        return 10 * Math.Log10(meanSquare + 1e-20);
    }

    /// <summary>
    /// Find the bars in one bin, pair them across their gaps, and say whether it is keying.
    /// </summary>
    private Evaluation Evaluate(Bin bin)
    {
        var runs = bin.Runs;
        var now = _hop - 1;
        var recent = now - _keyingHops;

        // **A BAR**: a run a dit long or longer that stands wholly above the runs either side,
        // every hop of theirs under every hop of its own - it rose to a level, held it, and left
        // it downward. The run still open has no neighbour after it yet, so while it is long
        // enough and rose, it is a bar so far.
        var bars = new List<int>();

        for (var i = 1; i < runs.Count; i++)
        {
            var run = runs[i];

            if (run.Count < _minBarHops
                || runs[i - 1].Max >= run.Min
                || (i + 1 < runs.Count && runs[i + 1].Max >= run.Min))
            {
                continue;
            }

            bars.Add(i);
        }

        // **A GAP**: every run between two paired bars wholly below the lower of them -
        // it dropped and stayed down - and the two bars at one level, their bands overlapping,
        // because a station holds the same level from one element to the next. A pair further
        // apart than a second is two things heard, not one sender.
        // Measured only where there are two bars to pair: most bins, most hops, have none.
        var waves = bars.Count >= 2 ? Waves(bin, runs, bars, recent, now) : (Level: double.PositiveInfinity, Wander: 0.0);
        var gaps = 0;
        var keying = false;
        var barsLastSecond = 0;
        var barSum = 0.0;
        var barCount = 0;
        var gapPower = 0.0;
        var gapHops = 0;
        var loudestGap = double.NaN;
        var edge = EnvelopeWindowSamples / HopSamples;
        var markedBars = new SortedSet<int>();

        for (var k = 1; k < bars.Count; k++)
        {
            var b = runs[bars[k]];

            // **A BAR PAIRS WITH THE NEAREST BAR AT ITS OWN LEVEL** (work instruction 491, R104,
            // HM-DEC-196). It used to be compared only with the bar just before it, so a burst in a
            // gap that made a short bar of its own in this bin, at another level, stood between two
            // of a station's bars: each was compared with the burst, the levels disagreed, and the
            // station's bars were never paired. On unit 490's call with bursts in its gaps that
            // lost every mark of the first CQ. A bar at another level between two at one level is
            // part of the gap between them, which still has to have dropped below them both and
            // clear its own wander; the agreement asked of the pair is unchanged.
            var p = -1;

            for (var j = k - 1; j >= 0; j--)
            {
                var c = runs[bars[j]];

                if (b.Start - c.End > _keyingHops)
                {
                    break;
                }

                if (Math.Abs(c.Mean - b.Mean) <= FlatToleranceDb)
                {
                    p = j;
                    break;
                }
            }

            if (p < 0)
            {
                continue;
            }

            var a = runs[bars[p]];

            // The space between two elements is a dit at the least, the same shortest dit a bar
            // is held to; and the second bar holds the first one's level.
            var gapLength = b.Start - a.End - 1;

            if (gapLength < _minBarHops)
            {
                continue;
            }

            var lower = Math.Min(a.Mean, b.Mean);
            var ceiling = lower - FlatToleranceDb;
            var dropped = true;

            for (var i = bars[p] + 1; i < bars[k]; i++)
            {
                if (runs[i].Max >= ceiling)
                {
                    dropped = false;
                    break;
                }
            }

            // **AND THE BARS CLEAR THE GAPS' OWN WAVES.** The gaps' level, as a power mean,
            // must sit under the bars by more than the gap hops wander about their mean, in
            // decibels, measured in this bin over the last second. Noise's chance flat runs
            // sit at the noise's own level, so the noise around them is not under them by more
            // than it wanders: a crest, not a bar. A station's gaps are the noise well under
            // its bars. No constant sets this; the gaps measure it.
            if (!dropped || lower - waves.Level <= waves.Wander)
            {
                continue;
            }

            if (b.End > recent)
            {
                gaps++;
            }

            markedBars.Add(bars[p]);
            markedBars.Add(bars[k]);

            if (b.End > recent)
            {
                keying = true;

                for (var i = bars[p] + 1; i < bars[k]; i++)
                {
                    gapPower += runs[i].PowerSum;
                    gapHops += runs[i].Count;
                }

                // The loudest hop of the gap, clear of the two key edges, where the window
                // straddles them: the loudest noise a bar at this bin has to hold through.
                for (var hop = a.End + 1 + edge; hop < b.Start - edge; hop++)
                {
                    var db = bin.Level(hop);

                    if (!(db <= loudestGap))
                    {
                        loudestGap = db;
                    }
                }
            }
        }

        var marked = markedBars.Select(i => new Span(runs[i].Start, runs[i].End, runs[i].Mean)).ToList();

        foreach (var m in marked)
        {
            if (m.End > recent)
            {
                barsLastSecond++;
                barSum += m.MeanDb;
                barCount++;
            }
        }

        return new Evaluation(
            marked,
            bars.Count(i => runs[i].End > recent),
            gaps,
            barsLastSecond,
            keying,
            barCount > 0 ? barSum / barCount : double.NaN,
            // The gap level drawn is the edge-free one the bars were held against, where it
            // was measured; the gaps between pairs, edges and all, otherwise.
            !keying ? double.NaN
                : double.IsFinite(waves.Level) ? waves.Level
                : gapHops > 0 ? 10 * Math.Log10(gapPower / gapHops) : double.NaN,
            bars.Where(i => runs[i].End > recent).Select(i => runs[i].Mean).DefaultIfEmpty(double.NegativeInfinity).Max(),
            loudestGap,
            bars.Select(i => new Span(runs[i].Start, runs[i].End, runs[i].Mean)).ToList());
    }

    /// <summary>
    /// The level and the wander of one bin's gap hops over the last second: every hop that is
    /// not in a bar, nor within one window of a bar's key edge, where the window straddles it.
    /// </summary>
    private (double Level, double Wander) Waves(Bin bin, List<Run> runs, List<int> bars, long from, long now)
    {
        var edge = EnvelopeWindowSamples / HopSamples;
        var b = 0;
        var n = 0;
        var power = 0.0;
        var sum = 0.0;
        var sumSq = 0.0;

        for (var hop = Math.Max(from + 1, runs.Count > 0 ? runs[0].Start : from + 1); hop <= now; hop++)
        {
            while (b < bars.Count && runs[bars[b]].End + edge < hop)
            {
                b++;
            }

            if (b < bars.Count && runs[bars[b]].Start - edge <= hop)
            {
                continue;
            }

            var db = bin.Level(hop);
            power += Math.Pow(10, db / 10);
            sum += db;
            sumSq += db * db;
            n++;
        }

        // Too few hops to measure a wander on is no licence: nothing clears it.
        if (n < _minBarHops)
        {
            return (double.PositiveInfinity, 0);
        }

        var mean = sum / n;

        return (10 * Math.Log10(power / n), Math.Sqrt(Math.Max(0, (sumSq / n) - (mean * mean))));
    }

    /// <summary>
    /// Every bar completed in any bin becomes one mark at the peak of its lobe, with its level and
    /// contrast (work instruction 490, R103), on the hop it ends (work instruction 492, R105).
    /// </summary>
    /// <remarks>
    /// **A MARK IS A DOT OR A DASH THE MOMENT IT ENDS** (work instruction 492, R105, HM-DEC-197). A
    /// bar - a flat top held within the flatness tolerance, at least the shortest bar long, standing
    /// above the runs either side - is handed out when it is complete, judged on itself. It used to
    /// wait to be paired, and pairing waits on the check that the bars clear their gaps' wander over
    /// the last second, so a burst in that second held a station's dit back 435 ms past its end
    /// (unit 491) and the reader had closed its letter. Pairing and the wander check are untouched
    /// and still decide the keying verdict, the light and the scope; they no longer decide
    /// delivery. The noise guard for letters is in <see cref="CwSenderGate"/>.
    /// </remarks>
    private void CallMarks(Evaluation[] evals, long hop, double nowSeconds)
    {
        if (_laneBin is not null)
        {
            LaneMarks(hop, nowSeconds);
        }

        for (var i = 0; i < _bins.Length; i++)
        {
            var bin = _bins[i];

            foreach (var m in evals[i].AllBars)
            {
                // A bar still running has no end yet, and one already taken is not taken again.
                if (m.End >= hop || bin.Recorded.Contains(m.Start))
                {
                    continue;
                }

                // One envelope window after its end, so the key-up can show at the peak.
                var edgeHops = EnvelopeWindowSamples / HopSamples;

                // With edges asked for, until the hops the falling edge is read over exist (work
                // instruction 497): four, still inside the three-window bound below.
                if (hop < m.End + (MarksNeedEdges ? EdgeHops : edgeHops))
                {
                    continue;
                }

                bin.Recorded.Add(m.Start);

                // **HANDED OUT WHEN IT ENDS, OR NOT AT ALL** (work instruction 492): within two
                // windows of its end. A bar found later, when a bin's history is read again at a
                // newly measured contrast, was not a bar when it ended, and handing it out seconds
                // late would split the letter it belongs to.
                if (MarksNeedPromptness && hop - m.End > 3 * edgeHops)
                {
                    continue;
                }

                var apex = Apex(i, m.Start, m.End);
                var level = MeanLevel(_bins[apex], m.Start, m.End);

                // **AND IT BEGINS WHERE ITS TONE ROSE.** The key-down edge can join the front of a
                // dah's top as a run of its own and split it, so the bar that ends with the tone may
                // be only its back half. The mark reaches back over the hops before it where the peak
                // stayed at its level, within the flatness tolerance, to the whole flat top.
                var start = m.Start;

                // Never past hop zero (work instruction 504): before the first hop there is no level,
                // and reading hop -1 threw in the first seconds of audio.
                while (start > 0 && start > m.End - HistoryHops && _bins[apex].Level(start - 1) >= level - FlatToleranceDb)
                {
                    start--;
                }

                if (start < m.Start)
                {
                    level = MeanLevel(_bins[apex], start, m.End);
                }

                // **ITS OWN HEIGHT OVER THE GAP BESIDE IT** (work instruction 507, R112): every single-mark
                // test below is a fraction of this, never a decibel figure.
                var own = OwnContrast(_bins[apex], start, m.End, level, hop);

                // **A MARK RISES AND FALLS LIKE A KEY** (work instruction 497, R107, HM-DEC-201).
                if (MarksNeedEdges && !HasEdges(_bins[apex], start, m.End, level, EdgeDepth(own.ContrastDb)))
                {
                    continue;
                }

                // **AND IT IS NARROW** (work instruction 498, R108, HM-DEC-202; back in 541, HM-DEC-245).
                if (MarksNeedNarrowness && CwRules.On(CwRules.Narrowness) && !IsNarrow(apex, start, m.End, level, own.ContrastDb, hop))
                {
                    continue;
                }

                var gap =!double.IsNaN(evals[apex].GapDb) ? evals[apex].GapDb : evals[i].GapDb;

                // **ONE SHAPE, ONE SCORE** (work instruction 502, R110, HM-DEC-206): how far the bar
                // sits inside the shape of a keyed tone, from all its properties together.
                var shape = Shape(apex, start, m.End, level, own, hop);

                if (MarksNeedShape && shape.Score < ShapeThreshold)
                {
                    continue;
                }

                var from = nowSeconds - ((hop - start + 1) * HopMs / 1000);
                var to = nowSeconds - ((hop - m.End) * HopMs / 1000);
                var pitch = MarkPitch(StationBin(apex, start, m.End), start, m.End);

                // **THE SENDER'S OWN WINDOW CALLS ITS OWN MARKS** (work instruction 529): within two bins of it, after it
                // opened, the grid is not asked again.
                if (AtOwnWindow(pitch, to))
                {
                    continue;
                }

                if (MarksNeedOneCall && AlreadyCalled(from, to, pitch, level))
                {
                    continue;
                }

                // **KEYED: WHETHER THE DETECTOR WAS KEYING AT ITS PEAK** (work instruction 493): the
                // peak or a bin within two of it - fifty hertz, the shoulders unit 488 measured - had
                // paired bars clearing their gaps' wander in the last second. Noise never is.
                var keyed = false;

                for (var k = Math.Max(0, apex - 2); k <= Math.Min(_bins.Length - 1, apex + 2) && !keyed; k++)
                {
                    keyed = evals[k].Keying;
                }

                var candidate = new CwMark(
                    ++_candidateSequence, from, to, pitch, level, double.IsNaN(gap) ? double.NaN : level - gap)
                {
                    Keyed = keyed,
                    Shape = shape,
                    OwnContrastDb = own.ContrastDb,
                    Stood = false,
                };

                _candidates.Add(candidate);

                // **THE PATTERN ACROSS MARKS IS THE GATE** (work instruction 507, R112, HM-DEC-210): a
                // candidate is handed on only when it stands in a sequence with the shape of a keyed tone.
                // **A MARK THAT STANDS IS KEYED** (work instruction 515, R114, HM-DEC-219): keying is a
                // sequence standing, at any pitch, so a mark handed on is a keyed mark. It used to be keyed
                // only where the bins around its peak paired their bars, which through the radio's 500 Hz
                // filter they did not at 700 Hz, so 65 marks stood, none was keyed and nothing printed
                // (unit 514). Noise stands a few marks and still prints nothing; the reader's own rules
                // hold it.
                foreach (var stood in MarksNeedPattern ? _pattern.Offer(candidate) : new[] { candidate })
                {
                    _marks.Add(stood with { Sequence = ++_markSequence, Stood = true, Keyed = true });
                }
            }

            bin.Recorded.RemoveWhere(s => s < hop - HistoryHops - _keyingHops);
        }

        _pattern.Prune(nowSeconds);
        _candidates.RemoveAll(k => k.ToSeconds < nowSeconds - CalledSeconds);
        _marks.RemoveAll(k => k.ToSeconds < nowSeconds - CalledSeconds);
    }

    /// <summary>
    /// Whether a candidate must stand in a sequence with the shape of a keyed tone to be handed on; on
    /// by default (work instruction 507). Off, every candidate is handed on, so the difference can be
    /// counted.
    /// </summary>
    public bool MarksNeedPattern { get; set; } = true;

    /// <summary>
    /// **WHERE THE ENERGY WAS, OVER THE MARK'S OWN SAMPLES** (work instruction 522): between two pitches, the excess
    /// power at five hertz steps over the mark's whole length, every sample weighed alike, less the gaps
    /// either side; and the centroid of the half-power stretch around its peak. NaN where the samples are gone.
    /// </summary>
    /// <remarks>
    /// The bins are ten millisecond windows, and two stations two hundred hertz apart beat in step with the five
    /// millisecond hop, so between them the bins read power keyed with the mark (unit 521): a bin centroid of a dah
    /// beside a steady carrier at 825 fell at 693. Over a dah's 180 ms the two are separate tones, six hertz wide.
    /// </remarks>
    private List<double> SamplePeaks(double lowHz, double highHz, long start, long end)
    {
        var markFrom = ((start + 1) * HopSamples) - (HopSamples / 2);
        var markTo = ((end + 1) * HopSamples) + (HopSamples / 2);
        var beforeFrom = (start - (2 * EdgeHops) + 1) * HopSamples;
        var beforeTo = (start - EdgeHops + 1) * HopSamples;
        var afterFrom = (end + EdgeHops + 1) * HopSamples;
        var afterTo = Math.Min(_samplesSeen, (end + (2 * EdgeHops) + 1) * HopSamples);
        var oldest = Math.Max(0, _samplesSeen - _raw.Length);

        if (markFrom < oldest || markTo > _samplesSeen || markTo - markFrom < 8)
        {
            return new List<double>();
        }

        const double step = 5;
        var count = (int)Math.Floor((highHz - lowHz) / step) + 1;
        var excess = new double[count];

        for (var k = 0; k < count; k++)
        {
            var hz = lowHz + (k * step);
            var gaps = 0.0;
            var parts = 0;

            if (beforeFrom >= oldest && beforeTo - beforeFrom >= 8)
            {
                gaps += Power(hz, beforeFrom, beforeTo);
                parts++;
            }

            if (afterTo - afterFrom >= 8)
            {
                gaps += Power(hz, afterFrom, afterTo);
                parts++;
            }

            excess[k] = Power(hz, markFrom, markTo) - (parts > 0 ? gaps / parts : 0);
        }

        var peaks = new List<double>();
        var top = excess.Max();

        if (!(top > 0))
        {
            return peaks;
        }

        // Every local peak of at least a quarter of the strongest, fifty hertz or more from a stronger one: two
        // stations keying at once over this span. Fifty hertz is three times the width a dit's own samples resolve.
        var order = Enumerable.Range(0, count)
            .Where(k => excess[k] >= top / 4 && (k == 0 || excess[k] >= excess[k - 1]) && (k == count - 1 || excess[k] >= excess[k + 1]))
            .OrderByDescending(k => excess[k])
            .ToList();
        var taken = new List<int>();

        foreach (var peak in order)
        {
            if (taken.Any(t => Math.Abs(t - peak) * step < 50))
            {
                continue;
            }

            taken.Add(peak);

            var half = excess[peak] / 2;
            var from = peak;
            var to = peak;

            while (from > 0 && excess[from - 1] >= half && excess[from - 1] <= excess[from])
            {
                from--;
            }

            while (to < count - 1 && excess[to + 1] >= half && excess[to + 1] <= excess[to])
            {
                to++;
            }

            var weight = 0.0;
            var moment = 0.0;

            for (var k = from; k <= to; k++)
            {
                weight += excess[k];
                moment += excess[k] * (lowHz + (k * step));
            }

            peaks.Add(moment / weight);
        }

        return peaks;
    }

    /// <summary>
    /// The mean-square power of the raw samples between two absolute indexes at a frequency, every sample weighed alike
    /// (work instruction 523): a Hann window weighs a span's ends to nothing, and a dit at the end of a span another
    /// sender's dah began lost its peak. A flat window's sidelobes, a twentieth in power, stay under the quarter that
    /// makes a second sender.
    /// </summary>
    private double Power(double hz, long fromSample, long toSample)
    {
        var n = (int)(toSample - fromSample);
        var coefficient = 2 * Math.Cos(2 * Math.PI * hz / SampleRate);
        double s1 = 0;
        double s2 = 0;
        double weights = 0;

        for (var i = 0; i < n; i++)
        {
            const double w = 1;
            var x = _raw[(int)((fromSample + i) % _raw.Length)] * w;
            var s0 = x + (coefficient * s1) - s2;

            s2 = s1;
            s1 = s0;
            weights += w;
        }

        return 2 * ((s1 * s1) + (s2 * s2) - (coefficient * s1 * s2)) / (weights * weights);
    }

    /// <summary>The bin nearest a pitch.</summary>
    private int Nearest(double hz)
    {
        var nearest = 0;

        for (var i = 1; i < _bins.Length; i++)
        {
            if (Math.Abs(_bins[i].Hz - hz) < Math.Abs(_bins[nearest].Hz - hz))
            {
                nearest = i;
            }
        }

        return nearest;
    }

    // **NARROWNESS, BACK IN THE TREE** (work instruction 541, HM-DEC-245): removed in work instruction 534 and restored when
    // the score counted what is wrong and invented as well as what is right. Copied from the tag before-scoreboard.

    /// <summary>
    /// How far either side of the sender's pitch the band beside a mark is read on its own samples, in Hz (work instruction
    /// 529): 150, past a dit's own spread at any speed this reads (a 25 ms dit spreads about 40 Hz) and inside the radio's
    /// 500 Hz filter around a pitch near its middle. The author's.
    /// </summary>
    public const double ToneSideHz = 150;

    /// <summary>
    /// **A MARK'S ENERGY IS AT ITS PITCH, ON ITS OWN SAMPLES** (work instruction 529): the power at the sender's pitch over
    /// the mark's own span stands at least <see cref="NarrowDepthDb"/> over the power <see cref="ToneSideHz"/> either side
    /// of it, the louder side, every sample weighed alike; true where the span has left the raw audio.
    /// </summary>
    /// <remarks>
    /// The sender's window low-passes at a few tens of hertz, so a click of five or ten milliseconds comes out of it as a
    /// hump a dit long, and the grid's narrowness, its side bins averaged over the hump's hops, barely sees a click one hop
    /// long. On the mark's own samples a click is as loud beside the pitch as at it, and a keyed tone is not.
    /// </remarks>
    private bool IsToneHere(double fromSeconds, double toSeconds, double pitchHz)
    {
        var from = (long)Math.Round(fromSeconds * SampleRate);
        var to = (long)Math.Round(toSeconds * SampleRate);

        if (from < Math.Max(0, _samplesSeen - _raw.Length) || to > _samplesSeen || to - from < 8)
        {
            return true;
        }

        var at = Power(pitchHz, from, to);
        var beside = Math.Max(Power(pitchHz - ToneSideHz, from, to), Power(pitchHz + ToneSideHz, from, to));

        return 10 * Math.Log10((at + 1e-30) / (beside + 1e-30)) >= NarrowDepthDb;
    }


    /// <summary>
    /// **What share of its own height over its gap a mark's bin must stand above the bins
    /// <see cref="NarrowBins"/> either side** (work instruction 507): a half, for the edges' reason.
    /// </summary>
    /// <remarks>
    /// A keyed tone's energy is in its own bin, and 300 Hz away the band reads what the gap beside the
    /// mark reads, so a tone stands the whole of its height over its neighbours; noise stands over them
    /// by nothing. Half the height is the line between. It replaces unit 498's fixed 6 dB. The author's.
    /// </remarks>
    public const double NarrowShare = 0.5;


    /// <summary>How far a mark must stand above the bins <see cref="NarrowBins"/> either side, in dB.</summary>
    /// <remarks>Half amplitude, as for the edges (<see cref="EdgeDepthDb"/>). The author's.</remarks>
    public const double NarrowDepthDb = EdgeDepthDb;

    /// <summary>Whether a bar must be narrow to be handed out as a mark; on by default.</summary>
    /// <remarks>
    /// **NOTHING CHECKED THAT A MARK'S ENERGY IS IN ONE BIN** (work instruction 498, R108,
    /// HM-DEC-202). A keyed tone stands well above the band beside it while it is up; noise is
    /// everywhere, so the band beside a noise bar is as loud as the bar. Off, a bar is handed out as
    /// before, so the difference can be counted. It gates only the marks.
    /// </remarks>
    public bool MarksNeedNarrowness { get; set; } = true;

    /// <summary>
    /// Whether the mark is narrow: the bins <see cref="NarrowBins"/> either side do not both rise with it by more
    /// than <see cref="NarrowShare"/> of its own height over its gap (work instruction 507); a side off the
    /// bins, or with no gap readable beside the mark, is not read.
    /// </summary>
    /// <remarks>
    /// <para>**THE NEIGHBOURS' RISE, NOT THEIR LEVEL** (work instruction 507). Noise is broadband, so when a
    /// noise bar rises the band beside it rises with it; a keyed tone's energy is in its own bin, and the
    /// band 300 Hz away does not follow its key. Unit 498 compared the neighbours' level with the mark's,
    /// so a second station keying on its own near a probe bin made a loud mark look broad: unit 507's two
    /// stations, 200 Hz apart, lost a dah of the loud one that way once the line became half the mark's
    /// height. Its rise over the same neighbours' own gap beside the mark is a ratio a rectangle keeps at
    /// any height, and a station keying to its own timing does not move it.</para>
    /// <para>Where the mark's own height cannot be read, the first hops of audio, it falls back to unit
    /// 498's level test at <see cref="NarrowDepthDb"/>.</para>
    /// </remarks>
    private bool IsNarrow(int bin, long start, long end, double level, double ownDb, long hop)
    {
        var rises = new List<double>();

        foreach (var side in new[] { bin - NarrowBins, bin + NarrowBins })
        {
            if (side < 0 || side >= _bins.Length)
            {
                continue;
            }

            if (!(double.IsFinite(ownDb) && ownDb > 0))
            {
                if (MeanLevel(_bins[side], start, end) > level - NarrowDepthDb)
                {
                    return false;
                }

                continue;
            }

            if (SideRise(_bins[side], start, end, hop) is var rise && double.IsFinite(rise))
            {
                rises.Add(rise);
            }
        }

        // **BROAD ONLY WHERE EVERY NEIGHBOUR READ RISES WITH IT.** Noise is broadband and lifts both sides at
        // once; another station keying on its own sits on one side and lifts that one only. With no
        // neighbour to read at all - a passband narrower than six hundred hertz - nothing here can say a
        // mark is broad, and it is not turned away on a test that was not taken.
        return rises.Count == 0 || rises.Any(r => r <= NarrowShare * ownDb);
    }


    /// <summary>The candidates of the last minute, stood or not, in the order called: for the tests' count (work instruction 507).</summary>
    internal IReadOnlyList<CwMark> CandidatesKept
    {
        get
        {
            lock (_gate)
            {
                return _candidates.ToArray();
            }
        }
    }

    /// <summary>How many candidates the single-mark gates have passed since the detector was made (work instruction 507).</summary>
    public int CandidateCount
    {
        get
        {
            lock (_gate)
            {
                return (int)_candidateSequence;
            }
        }
    }

    /// <summary>How many hops a keyed edge may take to cross from gap to top, or from top to gap.</summary>
    /// <remarks>
    /// <para>**THE AUTHOR'S, FROM WHAT A KEY AND THIS WINDOW DO** (work instruction 497). The level
    /// is read through a window two hops long, so a step in the audio is spread across two hops by
    /// the window alone; a keyer shapes its edge over a few milliseconds more, allowed another two
    /// hops, ten. Four hops, twenty milliseconds. Not fitted to any recording.</para>
    /// </remarks>
    public const int EdgeHops = 4;

    /// <summary>
    /// How far below a mark's top its edge must reach within <see cref="EdgeHops"/>, in dB, **only where
    /// no gap beside the mark can be read** - the first hops of audio - since work instruction 507.
    /// </summary>
    /// <remarks>
    /// Half amplitude, the point a keyed element's edge is conventionally timed at. The author's.
    /// </remarks>
    public const double EdgeDepthDb = 6;

    /// <summary>
    /// **What share of its own height over the gap beside it a mark's edge must fall within
    /// <see cref="EdgeHops"/>** (work instruction 507, R112, HM-DEC-210): a half.
    /// </summary>
    /// <remarks>
    /// <para>**A RECTANGLE KEEPS ITS PROPORTIONS AT ANY HEIGHT.** A key going up or down takes the level
    /// from the gap to the top, the whole of the mark's height, within the window's spread; a mark at
    /// 8 dB over its gap falls 8 dB and one at 38 falls 38. Half of it, the midpoint of the edge in
    /// decibels, is what any keyed edge crosses within the four hops whatever the mark's height, and
    /// what a bump in noise, which slopes into its neighbours, does not. The author's, from what a
    /// keyed tone is; not fitted to any recording and not tuned after a result.</para>
    /// <para>**IT REPLACES A FIXED 6 dB** (unit 497), which asked a mark 8 dB over its gap for three
    /// quarters of its height and one 38 dB over for a sixth.</para>
    /// </remarks>
    public const double EdgeShare = 0.5;

    /// <summary>The edge depth for a mark of this height over its gap, in dB.</summary>
    private static double EdgeDepth(double contrastDb)
        => double.IsFinite(contrastDb) && contrastDb > 0 ? EdgeShare * contrastDb : EdgeDepthDb;

    /// <summary>
    /// **How far a mark stands over the gap beside it, measured on the mark alone** (work instruction
    /// 507): its level over the median of the peak bin's hops just outside its edges, before it and after
    /// it, and each side's own figure; NaN where neither side can be read.
    /// </summary>
    /// <remarks>
    /// <para>**THE HOPS BESIDE, NOT THE BIN'S KEYING FIGURE.** The bin's gap level exists only once it is
    /// keying, which a station 8 dB over the noise never reached on the bench (unit 507 task 1), so the
    /// weakest marks were judged against a fixed figure. The hops beside the mark exist for every mark.</para>
    /// <para>**WHICH HOPS**: from two past the edge, where the window's spread has passed, to
    /// <see cref="EdgeHops"/> times two, before the mark and after it as far as audio has been heard.
    /// At 35 WPM a gap inside a letter is seven hops, so the far hops can reach the next mark; the median
    /// keeps a hop or two of it from moving the figure.</para>
    /// </remarks>
    private static (double ContrastDb, double BeforeDb, double AfterDb) OwnContrast(Bin peak, long start, long end, double level, long hop, int near = 3, int far = 2 * EdgeHops)
    {
        static double Median(List<double> values)
            => values.Count == 0 ? double.NaN : values.OrderBy(v => v).ElementAt(values.Count / 2);

        var before = new List<double>();
        var after = new List<double>();

        for (var k = near; k <= far; k++)
        {
            if (start - k >= 0)
            {
                before.Add(peak.Level(start - k));
            }

            if (end + k <= hop)
            {
                after.Add(peak.Level(end + k));
            }
        }

        var both = Median(before.Concat(after).ToList());

        return (level - both, level - Median(before), level - Median(after));
    }

    /// <summary>Whether a bar must rise and fall like a key to be handed out as a mark; on by default.</summary>
    /// <remarks>
    /// **HEIGHT, DURATION AND CONSISTENCY WERE TESTED; SHAPE WAS NOT** (work instruction 497, R107,
    /// HM-DEC-201). A bar's top was held to the flatness tolerance and nothing tested its edges, so a
    /// stretch of noise that drifted up, sat flat long enough, and drifted back passed every test.
    /// Off, a bar is handed out as before, so the difference can be counted. It gates only the
    /// marks: the pairing, the keying verdict, the light and the scope see every bar as before.
    /// </remarks>
    public bool MarksNeedEdges { get => _marksNeedEdges && CwRules.On(CwRules.Edges); set => _marksNeedEdges = value; }

    // Set by tests; the rule is on unless a test turns it off (work instruction 534).
    private bool _marksNeedEdges = true;

    /// <summary>Whether a bar found more than three windows after its end is turned away; on by default.</summary>
    /// <remarks>
    /// Work instruction 492's first delivery rule. The switch exists so work instruction 504 could
    /// count, one gate at a time, what each turns away; off, a late bar is handed out. It gates
    /// only the marks.
    /// </remarks>
    public bool MarksNeedPromptness { get; set; } = true;

    /// <summary>Whether a bar overlapping a mark already called at its pitch and level is not called again; on by default.</summary>
    /// <remarks>Work instruction 492's third delivery rule, switchable for the same count (work instruction 504).</remarks>
    public bool MarksNeedOneCall { get; set; } = true;

    /// <summary>
    /// Whether the level at the peak falls at least <see cref="EdgeDepthDb"/> below the mark's top
    /// within <see cref="EdgeHops"/> before its first hop, and again within as many after its last.
    /// </summary>
    private static bool HasEdges(Bin peak, long start, long end, double level, double depthDb, int edgeHops = EdgeHops)
    {
        var rose = false;
        var fell = false;

        for (var k = 1; k <= edgeHops && !(rose && fell); k++)
        {
            // Before the first hop there is no level, so no rise can be seen there.
            rose |= start - k >= 0 && peak.Level(start - k) <= level - depthDb;
            fell |= peak.Level(end + k) <= level - depthDb;
        }

        return rose && fell;
    }

    /// <summary>How far either side of a mark's bin its neighbours are read for narrowness, in bins.</summary>
    /// <remarks>
    /// **THREE HUNDRED HERTZ: OUTSIDE THE TONE'S OWN LOBE AND ITS KEYING** (work instruction 498).
    /// The ten millisecond window's main lobe reaches two hundred hertz either side of a tone, and a
    /// keyed tone's necessary bandwidth at the 45 WPM ceiling of `CW_SPEC.md` §7 is about 190 Hz,
    /// 95 either side. Three hundred is past both, so what is read there is the band beside the tone
    /// and not the tone itself. The author's; not fitted to any recording.
    /// </remarks>
    public const int NarrowBins = 12;

    /// <summary>How far a neighbouring bin rose over the mark's hops above its own gap beside the mark, in dB.</summary>
    private static double SideRise(Bin side, long start, long end, long hop)
        => OwnContrast(side, start, end, MeanLevel(side, start, end), hop).ContrastDb;

    /// <summary>Whether a bar must score inside the shape of a keyed tone to be handed out as a mark; on by default.</summary>
    /// <remarks>
    /// **FIVE LINES, EACH CROSSED NARROWLY, IS NOT THE SHAPE** (work instruction 502, R110,
    /// HM-DEC-206). The wander check, the shortest bar, the flat top, the edges and the narrowness
    /// each let through the noise that happens to pass it; what survived all five was noise that
    /// passed each by a little. Off, a bar that passed the five is handed out as before, so the
    /// difference can be counted. It gates only the marks.
    /// </remarks>
    public bool MarksNeedShape { get => _marksNeedShape && CwRules.On(CwRules.MarkShape); set => _marksNeedShape = value; }

    // Set by tests; the rule is on unless a test turns it off (work instruction 534).
    private bool _marksNeedShape = true;

    /// <summary>The longest bar that is plausibly a dah, in ms: a dah at 5 WPM, three dits of 240 ms.</summary>
    /// <remarks>
    /// The slowest code practice the ARRL sends is 5 WPM (work instruction 500), and no sender this
    /// reads sends a longer element. The detector knows no sender, so every length from the
    /// shortest bar to this is a plausible dit or dah, and longer is out of the shape in proportion.
    /// </remarks>
    public const double LongestDahMs = 3 * 1200.0 / 5;

    /// <summary>
    /// The lowest shape score a bar may have and be handed on as a candidate (work instructions 502 and
    /// 507).
    /// </summary>
    /// <remarks>
    /// <para>**SET ON THE WEAKEST SYNTHETIC STATION, NOT THE STRONGEST** (work instruction 507, R112). On
    /// the call at 20 WPM 8 dB over the noise, with the edges and narrowness tested and the shape not,
    /// the lowest score a real mark earns - one lying over a true key-down - is 0.004, the tenth 0.292
    /// and the median 0.453, the score now built from ratios a rectangle keeps at any height. The
    /// threshold sits a quarter under the lowest, 0.003.</para>
    /// <para>**WHAT IT STILL DOES AND WHAT IT NO LONGER DOES.** A product is nought where any one term is,
    /// so a bar with no edge within four hops, not standing over its neighbours at all, or not over one
    /// of its own gaps is still turned away. Past that it separates nothing on a weak station, and it is
    /// not asked to: unit 502 set 0.25 with a call 22 dB over the noise in front of it, and at 8 dB real
    /// marks score under that. **The pattern across marks now turns noise away**
    /// (<see cref="CwPatternGate"/>).</para>
    /// </remarks>
    public const double ShapeThreshold = 0.003;

    /// <summary>
    /// How far a bar sits inside the shape of a keyed tone: five properties, each scored from nought
    /// to one as a distance from the ideal, and their product (work instruction 502, R110).
    /// </summary>
    /// <param name="apex">The bar's bin.</param>
    /// <param name="start">Its first hop.</param>
    /// <param name="end">Its last hop.</param>
    /// <param name="level">Its mean level over its hops, in dB.</param>
    /// <param name="own">Its own height over the gap beside it, and over each side's gap, in dB (work instruction 507).</param>
    /// <param name="hop">The hop now: how far after the mark audio has been heard.</param>
    /// <returns>The five and the score.</returns>
    /// <remarks>
    /// <para>**THE PRODUCT, BECAUSE A KEYED TONE IS ALL FIVE AT ONCE.** A key down is a steady tone:
    /// flat on top, stepping up and down within the window's own spread, standing above the band
    /// beside it and above its own gaps, a dit or a dah long. A bar that is perfect on four and bad
    /// on one is not that shape, and a product says so where a sum or a mean would let four good
    /// properties carry a bad one. Each property is weighed the same; none was weighted by what
    /// makes a test pass.</para>
    /// <para>**FLATNESS**: one less the top's RMS wander from its own mean over the flatness tolerance
    /// at the bar's contrast (R93) - nought at the tolerance a bar may just pass, one when flat.
    /// **EDGES**: for the rise and for the fall, the hops taken to reach half amplitude below the
    /// top; two hops is the window's own spread and scores one, and each hop more is slower than a
    /// key, two over the hops taken; a rise before the first hop cannot be read and is not scored.
    /// **NARROWNESS**: the bins <see cref="NarrowBins"/> either
    /// side not rising with it: one less the smaller neighbour rise over the bar's own height above the gap
    /// beside it, at most one; no bin either side, or no
    /// gap readable, is not scored.
    /// **CONTRAST**: how evenly it stands over its gaps, the lower of its height over the gap before
    /// it and over the gap after it, over the higher; nought where it does not stand over one of them,
    /// and not scored where either side cannot be read.
    /// **LENGTH**: one up to <see cref="LongestDahMs"/>, and that over the length beyond it.</para>
    /// <para>**NO DECIBEL FIGURE IN IT SINCE WORK INSTRUCTION 507** (R112): narrowness and contrast
    /// were over a fixed 15 dB, so a mark 8 dB over the noise could never score more than about a
    /// half on either, whatever its shape. Every term is now a ratio a rectangle keeps at any height.</para>
    /// </remarks>
    private CwMarkShape Shape(int apex, long start, long end, double level, (double ContrastDb, double BeforeDb, double AfterDb) own, long hop)
        => Shape(_bins[apex], apex, start, end, level, own, hop, EdgeHops, 2);

    /// <summary>The shape of a bar on any trace: <paramref name="apex"/> is the grid bin its neighbours are read beside, and the edges are read over <paramref name="edgeHops"/>, scoring one within <paramref name="keyHops"/> (work instruction 529).</summary>
    private CwMarkShape Shape(Bin peak, int apex, long start, long end, double level, (double ContrastDb, double BeforeDb, double AfterDb) own, long hop, int edgeHops, int keyHops)
    {
        var hops = end - start + 1;
        var tolerance = ToleranceDb(own.ContrastDb);

        // **THE TOP, WITHOUT ITS EDGES.** The window spreads a key's step over its hops, so a bar can
        // carry one hop of its own rise or fall at an end, 12 dB under its top; that hop is the edge,
        // which the edge score judges, and counted in the top's wander it made a real dah read as
        // unflat. The top runs from the first hop within the tolerance of the bar's median to the last.
        var levels = new List<double>();

        for (var h = start; h <= end; h++)
        {
            levels.Add(peak.Level(h));
        }

        var median = levels.OrderBy(l => l).ElementAt(levels.Count / 2);
        var first = levels.FindIndex(l => Math.Abs(l - median) <= tolerance);
        var last = levels.FindLastIndex(l => Math.Abs(l - median) <= tolerance);
        var top = levels.GetRange(first, last - first + 1);
        var mean = top.Average();
        var wander = Math.Sqrt(top.Average(l => (l - mean) * (l - mean)));
        var flat = Math.Clamp(1 - (wander / tolerance), 0, 1);

        double EdgeScore(Func<int, long> at, bool readable)
        {
            if (!readable)
            {
                return 1;
            }

            for (var k = 1; k <= edgeHops; k++)
            {
                var h = at(k);

                if (h >= 0 && peak.Level(h) <= level - EdgeDepth(own.ContrastDb))
                {
                    return Math.Min(1, keyHops / (double)k);
                }
            }

            return 0;
        }

        var edges = EdgeScore(k => start - k, start - 1 >= 0) * EdgeScore(k => end + k, true);

        // **NARROWNESS: HOW LITTLE THE NEIGHBOURS RISE WITH IT** (work instruction 507): one less the smaller
        // of the two neighbours' rise over the mark's own height, so a half at IsNarrow's line.
        var rise = double.NegativeInfinity;

        foreach (var side in new[] { apex - NarrowBins, apex + NarrowBins })
        {
            if (side >= 0 && side < _bins.Length && SideRise(_bins[side], start, end, hop) is var r && double.IsFinite(r))
            {
                rise = double.IsNegativeInfinity(rise) ? r : Math.Min(rise, r);
            }
        }

        var height = double.IsFinite(own.ContrastDb) && own.ContrastDb > 0 ? own.ContrastDb : double.NaN;
        var narrow = double.IsNegativeInfinity(rise) || double.IsNaN(height) ? 1 : Math.Clamp(1 - (rise / height), 0, 1);
        var contrast = double.IsFinite(own.BeforeDb) && double.IsFinite(own.AfterDb)
            ? (own.BeforeDb <= 0 || own.AfterDb <= 0 ? 0 : Math.Min(own.BeforeDb, own.AfterDb) / Math.Max(own.BeforeDb, own.AfterDb))
            : 1;
        var lengthMs = hops * HopMs;
        var length = lengthMs <= LongestDahMs ? 1 : LongestDahMs / lengthMs;

        return new CwMarkShape(flat, edges, narrow, contrast, length);
    }

    /// <summary>
    /// Whether a mark the per-hop tests found, overlapping this span within one bin of its pitch, at its level,
    /// is already called. A fitted mark is never counted (work instruction 517): the fit only fills what the
    /// per-hop tests left, and a mark they find is never refused because a fit got there first.
    /// </summary>
    private bool AlreadyCalled(double from, double to, double pitchHz, double levelDb)
    {
        for (var k = _candidates.Count - 1; k >= 0; k--)
        {
            var called = _candidates[k];

            if (called.ToSeconds < from - KeyingSeconds)
            {
                break;
            }

            // At one level too (work instruction 492): a key-edge fragment twenty decibels under a
            // dah is not the dah, and must not stand in for it.
            if (!called.Fitted && called.ToSeconds > from && called.FromSeconds < to && Math.Abs(called.PitchHz - pitchHz) <= BinSpacingHz
                && Math.Abs(called.LevelDb - levelDb) <= 2 * FlatToleranceDb)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// **A MARK'S PITCH IS ITS ENERGY'S CENTROID** (work instruction 530, HM-DEC-234): over the mark's own samples, the
    /// strongest excess peak within two bins of the bin its bar peaked in, placed at its centroid; that bin's pitch only
    /// where the mark's samples have left the raw audio.
    /// </summary>
    /// <remarks>
    /// The walk to the louder neighbouring bin decided a mark's pitch to the nearest 25 Hz, and a station between two bins
    /// was called at one or the other mark by mark. Its marks' own energy places it to a fraction of a bin, as the
    /// shape-first path and the sender's window already place it. The walk still finds the bin a mark's level and edges are
    /// read in; it no longer names the pitch.
    /// </remarks>
    private double MarkPitch(int apex, long start, long end)
    {
        var hz = _bins[apex].Hz;
        var peaks = SamplePeaks(hz - (2 * BinSpacingHz), hz + (2 * BinSpacingHz), start, end);

        return peaks.Count > 0 ? peaks[0] : hz;
    }

    /// <summary>
    /// The station's bin for a mark: the bin nearest the top of its tone's lobe, over the mark's own
    /// hops (work instruction 496, HM-DEC-200).
    /// </summary>
    /// <remarks>
    /// <para>**THE LOUDEST WHILE THE KEY IS DOWN, NOT THE QUIETEST GAPS.** The walk to the apex
    /// (<see cref="Apex"/>) lands on the loudest bin, and the lobe of a ten millisecond window is
    /// four hundred hertz wide, so its top is nearly flat across a few bins and noise can tip the
    /// walk one bin either way. The top is placed between bins by a parabola through the levels, in
    /// dB, two bins either side of the apex - fifty hertz, where the lobe has fallen by a decibel or
    /// so and the curve is well defined, and where a second station two hundred hertz away adds
    /// almost nothing - and the nearest bin to that top is the station's.</para>
    /// <para>**AT THE SAME TIME** means over the same hops: every level compared is the mean over the
    /// mark's own span, so a bin is judged by what it did while this key was down and by nothing
    /// else. Nothing here is fitted to a recording.</para>
    /// </remarks>
    private int StationBin(int from, long start, long end)
    {
        var apex = Apex(from, start, end);

        if (apex < 2 || apex > _bins.Length - 3)
        {
            return apex;
        }

        var below = MeanLevel(_bins[apex - 2], start, end);
        var top = MeanLevel(_bins[apex], start, end);
        var above = MeanLevel(_bins[apex + 2], start, end);
        var curve = below - (2 * top) + above;

        if (!(curve < 0))
        {
            return apex;
        }

        // The vertex, in bins from the apex: two bins per unit of the parabola's own step.
        var offset = Math.Clamp(2 * 0.5 * (below - above) / curve, -2, 2);

        return Math.Clamp(apex + (int)Math.Round(offset), 0, _bins.Length - 1);
    }

    /// <summary>From one bin, climb to the neighbor with the higher mean level over the span until none is higher.</summary>
    private int Apex(int i, long from, long to)
    {
        var at = i;
        var best = MeanLevel(_bins[i], from, to);

        while (true)
        {
            var next = at;

            for (var j = at - 1; j <= at + 1; j += 2)
            {
                if (j < 0 || j >= _bins.Length)
                {
                    continue;
                }

                var level = MeanLevel(_bins[j], from, to);

                if (level > best)
                {
                    best = level;
                    next = j;
                }
            }

            if (next == at)
            {
                return at;
            }

            at = next;
        }
    }

    private static double MeanLevel(Bin bin, long from, long to)
    {
        var sum = 0.0;

        for (var h = from; h <= to; h++)
        {
            sum += bin.Level(h);
        }

        return sum / Math.Max(1, to - from + 1);
    }

    private static CwBarBin Summary(Bin bin, Evaluation e)
        => new(bin.Hz, e.Bars, e.Gaps, e.BarsLastSecond, e.Keying, e.BarDb, e.GapDb);

    private readonly record struct Span(long Start, long End, double MeanDb);

    // LoudestGapDb: the loudest gap hop the bars were held against, while keying; NaN otherwise.
    // AllBars: every bar in the bin, paired or not, oldest first (work instruction 492).
    private readonly record struct Evaluation(
        List<Span> Marked, int Bars, int Gaps, int BarsLastSecond, bool Keying, double BarDb, double GapDb, double LoudestBarDb, double LoudestGapDb, List<Span> AllBars);

    /// <summary>A stretch of hops held at one level.</summary>
    /// <remarks>
    /// <para>**A MARK'S TOP IS JUDGED FROM WHERE IT SETTLES, NOT FROM WHERE IT STARTS** (work instruction 525, task 2,
    /// HM-DEC-229). The IC-7300's AGC lets the first milliseconds of a mark through at full strength and then pulls the
    /// gain down, so a top starts a few dB high and settles. Held to the flatness tolerance from its first hop, the
    /// settling ended the run at the overshoot's end and a dah read as a fragment and a dit: at 3 dB of overshoot
    /// `THE QUICK BROWN FOX` read `HE E I INEE SE E I E U E`.</para>
    /// <para>So for a run that began by rising, its first <see cref="Bin.SettleHops"/> may step down, each no further
    /// under the hop before it than a flat top's wobble; those hops count toward the run's length and not toward its
    /// level, which is read from the hops after them. A step larger than that is the fall, judged as before, and a
    /// rise is judged as before. A top that does not overshoot is within the tolerance from its first hop and reads
    /// as it did, its level now read from the hops after its first few.</para>
    /// </remarks>
    private sealed class Run
    {
        public long Start;
        public int Count;

        // How many more hops the run may settle for: set when it began by rising (work instruction 525).
        public int SettleLeft;

        // The level, read from the hops after the settling where there are any, and from the settling hops where not.
        private double _sum;
        private int _levelCount;
        private double _min = double.PositiveInfinity;
        private double _max = double.NegativeInfinity;
        private double _settleSum;
        private int _settleCount;
        private double _settleMin = double.PositiveInfinity;
        private double _settleMax = double.NegativeInfinity;
        private double _settleFirst = double.NaN;

        // How many of the settling hops the window is still rising over the key-down edge (work instruction 528).
        public int SettleRiseHops;
        private double _last = double.NaN;

        public double PowerSum;

        public long End => Start + Count - 1;

        public double Mean => _levelCount > 0 ? _sum / _levelCount : _settleSum / _settleCount;

        public double Min => _levelCount > 0 ? _min : _settleMin;

        public double Max => _levelCount > 0 ? _max : _settleMax;

        /// <summary>Take a hop if the run still holds one level with it; say whether it did.</summary>
        /// <param name="db">The hop's level.</param>
        /// <param name="contrastDb">The bin's bars' measured contrast, or NaN before it has one.</param>
        /// <param name="underDb">Before it has one, the level the run's own contrast is read over.</param>
        public bool TryAdd(double db, double contrastDb, double underDb)
        {
            var settling = SettleLeft > 0;
            var (sum, count, min, max) = settling ? (_settleSum, _settleCount, _settleMin, _settleMax) : (_sum, _levelCount, _min, _max);

            if (count > 0)
            {
                var mean = (sum + db) / (count + 1);
                var toleranceDb = ToleranceDb(!double.IsNaN(contrastDb) ? contrastDb : mean - underDb);

                // **A SETTLING TOP ONLY COMES DOWN** (work instruction 528, HM-DEC-232): an overshoot's first hop is its
                // highest once the window has risen over the edge (its first SettleRiseHops hops). On the owner's recording a
                // dah rose over six hops from 13 dB under its top, each step inside the
                // tolerance, and settled on that rise its start moved 40 ms early, onto the dit before it, and the gate dropped
                // it as the same tone read twice: CHAT read CHET. A hop more than the tolerance over the first is a rise.
                if (Math.Max(max, db) - mean > toleranceDb || (settling && _settleCount >= SettleRiseHops && db - _settleFirst > toleranceDb))
                {
                    return false;
                }

                // Settling: a step down no larger than a flat top's wobble is the AGC pulling the gain in; the level
                // the run settles to is read after it. Anything else under the band is the fall, as before.
                if (mean - Math.Min(min, db) > toleranceDb && !(settling && _last - db <= toleranceDb))
                {
                    return false;
                }
            }
            else if (_settleCount > 0)
            {
                // The first hop after the settling is held to the last settling hop and to the settling's top: a fall
                // there is still the fall.
                var toleranceDb = ToleranceDb(!double.IsNaN(contrastDb) ? contrastDb : db - underDb);

                if (_last - db > toleranceDb || db - _settleMax > toleranceDb)
                {
                    return false;
                }
            }

            Count++;
            _last = db;
            PowerSum += Math.Pow(10, db / 10);

            if (settling)
            {
                SettleLeft--;
                _settleFirst = _settleCount < SettleRiseHops ? Math.Max(double.IsNaN(_settleFirst) ? db : _settleFirst, db) : _settleFirst;
                _settleSum += db;
                _settleCount++;
                _settleMin = Math.Min(_settleMin, db);
                _settleMax = Math.Max(_settleMax, db);
            }
            else
            {
                _sum += db;
                _levelCount++;
                _min = Math.Min(_min, db);
                _max = Math.Max(_max, db);
            }

            return true;
        }
    }

    /// <summary>One pitch: its levels over the history and the runs they made.</summary>
    private sealed class Bin
    {
        private readonly double[] _levels;

        // The lowest level over the last second, kept as a queue of hops each lower than every
        // one before it, oldest first, so reading it costs nothing per hop.
        private readonly int _secondHops;
        private readonly long[] _lowHop;
        private readonly double[] _lowDb;
        private int _lowHead;
        private int _lowCount;

        public Bin(double hz, int sampleRate, int historyHops, int secondHops)
        {
            Hz = hz;
            Coefficient = 2 * Math.Cos(2 * Math.PI * hz / sampleRate);
            _levels = new double[historyHops];
            KeepSums(historyHops);
            _secondHops = Math.Max(1, secondHops);
            _lowHop = new long[_secondHops + 1];
            _lowDb = new double[_secondHops + 1];
        }

        public double Hz { get; }

        /// <summary>How many hops after a rise a run may settle for (work instruction 525): the detector's <see cref="CwEnvelopeDetector.SettleHops"/>.</summary>
        public int SettleHops { get; init; }

        /// <summary>
        /// Whether a hop that ends the open run is a key-down: a rise, from the lowest level over the window before it,
        /// of at least half the bin's measured keying contrast, or where it has none yet, of its height over the loudest
        /// gap any keying bin measured (work instruction 525). A mark rises from its gap by its contrast, and the window
        /// smears the rise over a hop or two, so it is measured from before the window began to rise. Where nobody is
        /// keying there is neither figure, so noise never settles, and its small rises are not key-downs.
        /// </summary>
        private bool KeyDown(long hop, double db, double contrastDb)
        {
            if (Open is null)
            {
                return false;
            }

            var low = double.PositiveInfinity;

            for (var k = 1; k <= RiseHops && hop - k >= 0; k++)
            {
                low = Math.Min(low, Level(hop - k));
            }

            if (double.IsFinite(contrastDb) && contrastDb > 0)
            {
                return db - low >= contrastDb / 2;
            }

            // **THE FIRST RISE ABOVE THE FLOOR IS A KEY-DOWN** (work instruction 526, task 7, HM-DEC-230). Before anybody
            // is keying there is no contrast to measure a rise against, so a transmission's first marks were held to one
            // level from their first hop and an AGC overshoot broke them: `TEXT` and `CQ CQ` were lost. A rise above
            // everything this bin heard in the second before the window began to rise, by more than a flat top's wobble,
            // is a key-down: a station's first mark always makes one, and noise seldom beats its own second-long top.
            var high = double.NegativeInfinity;

            for (var h = Math.Max(0, hop - _secondHops); h < hop - RiseHops; h++)
            {
                high = Math.Max(high, Level(h));
            }

            return double.IsFinite(high) && db - high > FlatToleranceDb;
        }

        /// <summary>How many hops a rise is measured back over: the detector's window in hops, and one (work instruction 525).</summary>
        public int RiseHops { get; init; } = 1;

        public double Coefficient { get; }

        /// <summary>Its bars' level over the loudest hop of its gaps, while it is keying; NaN otherwise.</summary>
        public double ContrastDb { get; set; } = double.NaN;

        /// <summary>The runs, oldest first; the last is still open.</summary>
        public List<Run> Runs { get; } = new();

        /// <summary>The first hops of the bars already called as marks (work instruction 490).</summary>
        public HashSet<long> Recorded { get; } = new();

        public Run? Open => Runs.Count > 0 ? Runs[^1] : null;

        public double Level(long hop) => _levels[(int)(hop % _levels.Length)];

        private double[] _cum = Array.Empty<double>();
        private double[] _cumSq = Array.Empty<double>();
        private double _sum;
        private double _sumSq;
        private long _added = -1;

        /// <summary>How many hops back the running sums reach.</summary>
        public int SumsKept => _cum.Length - 1;

        /// <summary>The count, sum and sum of squares of the levels from one hop to another, both included (work instruction 516).</summary>
        public (int N, double Sum, double SumSq) Sums(long from, long to)
        {
            double Cum(long h) => h < 0 ? 0 : _cum[(int)(h % _cum.Length)];
            double CumSq(long h) => h < 0 ? 0 : _cumSq[(int)(h % _cum.Length)];

            return ((int)(to - from + 1), Cum(to) - Cum(from - 1), CumSq(to) - CumSq(from - 1));
        }

        /// <summary>Make room for the running sums: one more than the hops they must reach back over.</summary>
        public void KeepSums(int hops)
        {
            _cum = new double[hops + 1];
            _cumSq = new double[hops + 1];
        }

        /// <summary>The last hop added.</summary>
        public long Added => _added;

        /// <summary>The lowest level over the last second, counting a hop about to be added.</summary>
        public double LowestLastSecond(long hop, double db)
        {
            for (var i = 0; i < _lowCount; i++)
            {
                var k = (_lowHead + i) % _lowHop.Length;

                if (_lowHop[k] > hop - _secondHops)
                {
                    return Math.Min(_lowDb[k], db);
                }
            }

            return db;
        }

        public void Add(long hop, double db, double contrastDb, double underDb, double keyDownContrastDb)
        {
            _levels[(int)(hop % _levels.Length)] = db;

            // Running sums of the level and its square, for the rectangle fit (work instruction 516).
            _sum += db;
            _sumSq += db * db;
            _cum[(int)(hop % _cum.Length)] = _sum;
            _cumSq[(int)(hop % _cum.Length)] = _sumSq;
            _added = hop;

            while (_lowCount > 0 && _lowDb[(_lowHead + _lowCount - 1) % _lowHop.Length] >= db)
            {
                _lowCount--;
            }

            while (_lowCount > 0 && _lowHop[_lowHead] <= hop - _secondHops)
            {
                _lowHead = (_lowHead + 1) % _lowHop.Length;
                _lowCount--;
            }

            var tail = (_lowHead + _lowCount) % _lowHop.Length;
            _lowHop[tail] = hop;
            _lowDb[tail] = db;
            _lowCount++;

            if (Open is not { } open || !open.TryAdd(db, contrastDb, underDb))
            {
                var run = new Run { Start = hop, SettleLeft = CwRules.On(CwRules.Settle) && KeyDown(hop, db, keyDownContrastDb) ? SettleHops : 0, SettleRiseHops = RiseHops - 1 };
                run.TryAdd(db, contrastDb, underDb);
                Runs.Add(run);
            }
        }

        /// <summary>Build the runs again from the stored levels, at the bin's measured contrast.</summary>
        public void Reread(long from, long to)
        {
            Runs.Clear();

            for (var hop = from; hop <= to; hop++)
            {
                var db = Level(hop);

                if (Open is not { } open || !open.TryAdd(db, ContrastDb, double.NaN))
                {
                    var run = new Run { Start = hop, SettleLeft = CwRules.On(CwRules.Settle) && KeyDown(hop, db, ContrastDb) ? SettleHops : 0, SettleRiseHops = RiseHops - 1 };
                    run.TryAdd(db, ContrastDb, double.NaN);
                    Runs.Add(run);
                }
            }
        }

        public void Prune(long before)
        {
            var drop = 0;

            while (drop < Runs.Count - 1 && Runs[drop].End < before)
            {
                drop++;
            }

            if (drop > 0)
            {
                Runs.RemoveRange(0, drop);
            }
        }
    }
}
