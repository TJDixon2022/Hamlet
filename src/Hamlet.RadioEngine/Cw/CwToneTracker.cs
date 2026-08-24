namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// One measurement of the tone: how much energy is sitting at the pitch the
/// tracker is following, and where that pitch is.
/// </summary>
/// <param name="PowerDb">Energy at the tracked pitch, in decibels.</param>
/// <param name="CompetitorDb">
/// Energy at the strongest other note far enough away to be a different
/// station. This is how the decoder knows somebody else is on top of it.
/// </param>
/// <param name="ToneHz">The pitch being followed.</param>
/// <param name="SampleIndex">Index of the last sample in this measurement.</param>
/// <param name="NoiseDb">
/// What the band is doing either side of the tone, measured at this instant
/// (HM-DEC-088).
/// </param>
/// <param name="BroadbandDbfs">
/// Total power across the whole audio band, which is how the operator's own
/// transmission is recognized (HM-DEC-095).
/// </param>
/// <param name="Blocked">
/// True when this measurement covers the operator transmitting, and so may not
/// be learned from (HM-DEC-095).
/// </param>
public readonly record struct ToneReading(
    double PowerDb, double CompetitorDb, double ToneHz, long SampleIndex,
    double NoiseDb = double.NaN,
    double BroadbandDbfs = double.NaN,
    bool Blocked = false)
{
    /// <summary>True when the noise beside the tone was actually measured.</summary>
    public bool HasNoise => !double.IsNaN(NoiseDb);

    /// <summary>
    /// How far the tone stands above the noise beside it, right now.
    /// </summary>
    /// <remarks>
    /// **MEASURED AT THE SAME INSTANT, IN OTHER BINS** (HM-DEC-088). The old
    /// estimate came from watching the signal's own bin during the gaps between
    /// elements, which is the only place a single-bin decoder can look. That
    /// works and it has two faults: it cannot tell a fade from a gap, and it is
    /// always one element behind. Taking the noise from either side instead is
    /// free, unbiased, and follows a fade without ever chasing the signal.
    /// </remarks>
    public double SnrDb => HasNoise ? PowerDb - NoiseDb : double.NaN;

    /// <summary>
    /// How far the tracked note stands above whatever a rival station is
    /// actually managing to put into the same filter.
    /// </summary>
    /// <remarks>
    /// NOT SIMPLY HOW MUCH LOUDER WE ARE THAN THEM. A station a couple of
    /// hundred hertz away can be stronger than the one being read and do no
    /// harm at all, because the filter rejects it, and a decoder that marked
    /// every character uncertain whenever somebody louder was on the band would
    /// be useless on a busy evening. What matters is how much of them gets
    /// through, which is their strength less what the filter takes off it.
    /// </remarks>
    public double MarginOverCompetitorDb
        => PowerDb - (CompetitorDb - CwToneTracker.FilterRejectionDb);

    /// <summary>
    /// How far the tracked note stands above the nearest rival before the
    /// filter is credited with anything.
    /// </summary>
    /// <remarks>
    /// This is the one the veto reads. A rival this close is a problem even
    /// when the filter is rejecting it cleanly, because it is not the leakage
    /// that does the damage: it is the gate's idea of where a signal sits being
    /// pulled up by somebody else's keying.
    /// </remarks>
    public double RawMarginOverCompetitorDb => PowerDb - CompetitorDb;
}

/// <summary>
/// Finds the CW note in the audio and keeps following it.
/// </summary>
/// <remarks>
/// <para>A bank of Goertzel filters across the pitches the radio can produce,
/// evaluated over a short sliding window. Goertzel rather than an FFT because
/// this needs a couple of dozen known frequencies rather than a whole spectrum,
/// and it is a handful of multiplies per bin per sample with nothing to
/// allocate.</para>
/// <para>**TWO STAGES, AND THE REASON IS MEASURED RATHER THAN CITED**
/// (HM-DEC-095). A coarse bank surveys the whole range the radio can put a note
/// in and decides, by keying structure alone, which region holds somebody
/// sending (<see cref="CwToneSurvey"/>). A fine bank five hertz apart then reads
/// the exact pitch inside that region. The old single bank was spaced twenty-five
/// hertz, so an exact answer was arithmetically impossible, and it chose by
/// loudness, so it landed on whatever was strongest whether or not anybody was
/// keying it.</para>
/// <para>**THE WINDOW LENGTH IS THE DIFFERENCE BETWEEN A DECODE AND NOTHING.**
/// Swept against the real recording of a station answering a call, the message
/// resolves like this:</para>
/// <list type="bullet">
/// <item>20 ms, about 75 Hz of bandwidth: `M ? ?3VRA`</item>
/// <item>30 ms, about 50 Hz: `M ?R ?3VRA`</item>
/// <item>40 ms, about 38 Hz: `M VRR VA3VRA`</item>
/// <item>50 ms and beyond: the same, and no better</item>
/// </list>
/// <para>Twenty milliseconds is what this tracker used to run at, and it loses
/// half the callsign. The band is five hundred hertz wide and the signal occupies
/// a few tens, so most of what the filter passes is noise the decoder never
/// needed to hear.</para>
/// <para>**AND A NARROW WINDOW IS NOT FREE, WHICH IS WHY IT MOVES.** A window
/// that spans an appreciable part of an element smears its edges, and edge
/// positions are the entire content of Morse. Forty milliseconds is a fraction of
/// a dit at twelve words a minute and most of one at forty, so above eighteen
/// words a minute the window shortens again and trades the sensitivity back for
/// timing. Nobody needs both at once: a fast fist is a strong fist far more often
/// than not, and a signal at the edge of readability is almost never being sent
/// at forty.</para>
/// <para>NOBODY TUNES EXACTLY, so the pitch is hunted rather than assumed, across
/// the range the IC-7300 can put a signal at, 300 to 900 Hz (Full Manual p. 4-14).
/// **What is no longer done is hunting toward the operator's own setting.** The
/// old tie-break preferred whichever bin was nearest where the tracker already
/// sat, and the tracker was seeded from the configured pitch, so the answer was
/// pulled toward the number somebody typed in. A measurement that leans on the
/// expected answer is not a measurement (§0.0).</para>
/// </remarks>
public sealed class CwToneTracker
{
    /// <summary>Lowest pitch searched, in hertz.</summary>
    public const double MinimumToneHz = 300;

    /// <summary>Highest pitch searched, in hertz.</summary>
    public const double MaximumToneHz = 900;

    /// <summary>Spacing of the survey bank, in hertz.</summary>
    private const double CoarseSpacingHz = 25;

    /// <summary>Spacing of the reading bank, in hertz.</summary>
    /// <remarks>
    /// Five, which is what makes an exact answer possible at all. The reported
    /// pitch was never right on any recording this project holds, and a quarter
    /// of that error was the old twenty-five hertz grid on its own.
    /// </remarks>
    private const double FineSpacingHz = 5;

    /// <summary>How far either side of the survey's choice the fine bank reaches.</summary>
    /// <remarks>
    /// Fifteen, so seven bins five hertz apart. Taken from the validated
    /// reference chain, and narrower than the thirty this started at because the
    /// bank is now read all at once rather than one bin at a time.
    /// </remarks>
    private const double FineReachHz = 15;

    /// <summary>How often the survey is re-read, in hops.</summary>
    /// <remarks>
    /// <para>Twice a second. The survey works over three seconds of history, so
    /// asking more often returns nearly the same answer for the same work.</para>
    /// <para>**AND THE INTERVAL IS THE CONFIRMATION'S INDEPENDENCE, WHICH IS WHY
    /// IT WAS NOT SHORTENED.** A candidate has to be seen twice running before the
    /// tracker moves to it. Polling four times a second was tried, to cut the
    /// delay before an off-frequency signal is acquired, and it makes the two
    /// readings a quarter of a second apart over three seconds of shared history:
    /// nearly the same measurement asked twice, which is not a second opinion. It
    /// cost six more failures across the suite than it fixed.
    /// </para>
    /// </remarks>
    private const int SurveyEveryHops = 100;

    /// <summary>Every second hop goes to the survey, which is a ten millisecond grid.</summary>
    private const int SurveyDecimation = 2;

    /// <summary>
    /// How far from the tracked note another one has to be before it counts as
    /// a different station rather than the same one leaking sideways.
    /// </summary>
    /// <remarks>
    /// **ONE COPY, AND IT LIVES ON <see cref="CwCompetitor"/>.** The same
    /// question is asked in three places — here, by the survey's own noise
    /// separation, and by what the operator is told — and three copies of a
    /// number drift silently (§0).
    /// </remarks>
    private const double CompetitorSeparationHz = CwCompetitor.SeparationHz;

    /// <summary>
    /// How far either side of an admitted candidate its true pitch is looked for.
    /// </summary>
    /// <remarks>
    /// <para>**THE FINE BANK'S OWN REACH.** That bank already treats everything
    /// within this distance of its centre as the same station, so searching the
    /// same neighbourhood cannot turn a refinement into a choice between two
    /// candidates (HM-DEC-095). Tied to <see cref="FineReachHz"/> rather than
    /// written down again, so the two cannot drift apart.</para>
    /// <para>**TWELVE WAS TRIED FIRST AND WAS EXACTLY TOO NARROW.** The two
    /// recordings with the largest measured pitch error, `cw-2026-08-17-013347`
    /// and `cw-2026-08-17-013622`, are both out by 11.9 hertz, which put the
    /// peak on the boundary — and a peak at the boundary is refused, because
    /// interpolating there would be extrapolating. The guard was right and the
    /// reach was wrong.</para>
    /// </remarks>
    private const double RefineReachHz = FineReachHz;

    /// <summary>How finely the refinement searches, before interpolating.</summary>
    private const double RefineStepHz = 1;

    /// <summary>
    /// How much of a rival station this far off the tracked note the filter
    /// takes off, in decibels.
    /// </summary>
    /// <remarks>
    /// A conservative reading of what a Hann-tapered window does past a hundred
    /// and twenty-five hertz of separation. Conservative on purpose: understating
    /// the rejection makes the decoder mark characters uncertain that it could
    /// have read, which costs the operator some dimmed text. Overstating it lets
    /// somebody else's dits into a character that still looks clean, which costs
    /// them the truth (§0.0).
    /// </remarks>
    public const double FilterRejectionDb = 25;

    /// <summary>Window used while acquiring, and for the survey, in hops.</summary>
    /// <remarks>Eight hops is forty milliseconds, about thirty-eight hertz of
    /// bandwidth, which is where the real recording starts resolving.</remarks>
    private const int AcquireWindowHops = 8;

    /// <summary>Window used once the speed is known and slow, in hops.</summary>
    /// <remarks>Ten hops is fifty milliseconds, about thirty hertz.</remarks>
    private const int NarrowWindowHops = 10;

    /// <summary>Window used for a fast fist, in hops.</summary>
    /// <remarks>Four hops is twenty milliseconds, about seventy-five hertz, which
    /// still puts four measurements inside a dit at forty words a minute.</remarks>
    private const int FastWindowHops = 4;

    /// <summary>Above this speed the window shortens to keep the edges.</summary>
    public const double FastFistWpm = 18;

    /// <summary>
    /// How far past the speed limit the estimate has to go before the window
    /// changes, in words a minute.
    /// </summary>
    /// <remarks>
    /// Four either way, so a fist at the limit has to be read as fourteen or
    /// twenty-two before anything moves. Eighteen is squarely where most people
    /// send, which is the worst possible place to put a bare threshold.
    /// </remarks>
    private const double SpeedHysteresisWpm = 4;

    /// <summary>
    /// How close two consecutive surveys have to agree to count as the same
    /// signal.
    /// </summary>
    /// <remarks>One coarse bin either way, which is a station drifting or the
    /// survey preferring its neighbor, rather than a different signal.</remarks>
    private const double ConfirmWithinHz = CoarseSpacingHz;

    private readonly double[] _coarseHz;
    private readonly double[] _coarseCoefficient;
    private readonly double[] _coarseDb;

    private readonly double[] _fineHz;
    private readonly double[] _fineCoefficient;
    private readonly double[] _fineDb;

    private readonly float[] _ring;
    private readonly float[] _scratch;
    private readonly float[] _hann;
    private readonly double[] _neighbors;

    /// <summary>The gate's own tapered buffer, when it wants a different width.</summary>
    /// <remarks>
    /// <para>**THE SURVEY AND THE GATE ASKED ONE QUESTION THROUGH ONE FILTER AND
    /// THEY ARE NOT ONE QUESTION.** The survey searches frequency: it wants a
    /// taper that separates one bin from its neighbours across the whole sweep
    /// from three hundred to nine hundred hertz. The gate measures time: it wants
    /// a taper short enough to keep an element's edges and narrow enough to leave
    /// the noise outside. Sharing one buffer meant a width chosen for one was
    /// imposed on the other, and because that width is chosen from the fitted
    /// speed, **the search was being narrowed by an estimate the search itself
    /// produced.**</para>
    /// <para>**IT IS ALLOCATED AND UNUSED UNTIL THE TWO WIDTHS DIFFER.** With
    /// <see cref="GateWindowHops"/> unset the gate takes the survey's window, both
    /// passes are the same arithmetic over the same samples, and nothing is
    /// tapered twice.</para>
    /// </remarks>
    private readonly float[] _gateScratch;

    private readonly float[] _gateHann;

    /// <summary>The fine bank as the gate reads it, through the gate's window.</summary>
    private readonly double[] _gateFineDb;

    private readonly CwToneSurvey _survey;
    private readonly CwToneSurvey _fineSurvey;

    private int _ringWrite;
    private int _ringFill;
    private int _hopFill;
    private int _hopsSinceSurvey;
    private int _surveyPhase;
    private long _samplesSeen;
    private int _tracked;
    private int _windowHops;
    private int _hannHops;
    private int _gateHannHops;

    /// <summary>What the previous survey said, so a fluke has to happen twice.</summary>
    private double _previousKeyedHz = double.NaN;

    /// <summary>The last pitch keying was actually found at.</summary>
    private double _lastKeyedHz = double.NaN;

    /// <summary>
    /// How loud the station being read is while it is keyed (HM-DEC-127).
    /// </summary>
    /// <remarks>
    /// The level of the last candidate the tracker actually believed, which is
    /// what a displacing candidate is measured against. Unknown until something
    /// has been confirmed, and unknown is not a level: nothing is refused on the
    /// strength of a number nobody has.
    /// </remarks>
    private double _readingDb = double.NaN;

    /// <summary>What is reported as the tone, which is not the instant winner.</summary>
    private double _reportedHz = double.NaN;

    /// <summary>A move that is waiting for the character in progress to end.</summary>
    private double _heldSwitchHz = double.NaN;

    /// <summary>
    /// How many more surveys the last keying finding may go on protecting its
    /// own frequency from being called interference.
    /// </summary>
    /// <remarks>
    /// **IT HAS TO EXPIRE, AND FINDING THAT OUT COST A REAL DEFECT.** Without a
    /// countdown, one transient keying finding at six hundred hertz silenced the
    /// interference report at five hundred for the rest of the session, because
    /// they are inside one filter width of each other. The recording with an
    /// obvious carrier in it reported nothing at all.
    /// </remarks>
    private int _keyedProtects;

    /// <summary>Creates a tracker.</summary>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="startingToneHz">Where to begin looking, from the operator's setting.</param>
    public CwToneTracker(int sampleRate, double startingToneHz)
    {
        SampleRate = Math.Max(1_000, sampleRate);
        HopSamples = Math.Max(4, SampleRate / 200);
        MaximumWindowSamples = HopSamples * NarrowWindowHops;
        _windowHops = AcquireWindowHops;

        var coarse = (int)Math.Round((MaximumToneHz - MinimumToneHz) / CoarseSpacingHz) + 1;
        _coarseHz = new double[coarse];
        _coarseCoefficient = new double[coarse];
        _coarseDb = new double[coarse];

        for (var i = 0; i < coarse; i++)
        {
            _coarseHz[i] = MinimumToneHz + (i * CoarseSpacingHz);
            _coarseCoefficient[i] = Coefficient(_coarseHz[i]);
        }

        var fine = (int)Math.Round(2 * FineReachHz / FineSpacingHz) + 1;
        _fineHz = new double[fine];
        _fineCoefficient = new double[fine];
        _fineDb = new double[fine];

        _neighbors = new double[coarse];
        _ring = new float[MaximumWindowSamples];
        _scratch = new float[MaximumWindowSamples];
        _hann = new float[MaximumWindowSamples];
        _gateScratch = new float[MaximumWindowSamples];
        _gateHann = new float[MaximumWindowSamples];
        _gateFineDb = new double[fine];

        var surveyHop = (double)HopSamples * SurveyDecimation / SampleRate;

        _survey = new CwToneSurvey(_coarseHz, surveyHop);

        // **THE SECOND STAGE HAS TO ASK THE SAME QUESTION, NOT A CHEAPER ONE.**
        // Taking the coarse winner and reading the loudest fine bin under it puts
        // the answer back on the twenty-five hertz grid the coarse bank was
        // spaced at, which is the error this whole change exists to remove. The
        // fine bank scores keying structure exactly as the coarse one does.
        _fineSurvey = new CwToneSurvey(_fineHz, surveyHop);

        BuildHann();
        CenterFineBank(Math.Clamp(startingToneHz, MinimumToneHz, MaximumToneHz));
        _tracked = _fineHz.Length / 2;

        Guard = new CwTransmitGuard((double)HopSamples / SampleRate);
    }

    /// <summary>Samples per second.</summary>
    public int SampleRate { get; }

    /// <summary>How many samples one measurement advances by.</summary>
    public int HopSamples { get; }

    /// <summary>The longest window the tracker can look through.</summary>
    public int MaximumWindowSamples { get; }

    /// <summary>How many samples the current measurement looks at.</summary>
    public int WindowSamples => HopSamples * _windowHops;

    /// <summary>
    /// How many hops the gate looks through, or null to take the survey's.
    /// </summary>
    /// <remarks>
    /// <para>**THE ONE KNOB THAT SEPARATING THE TWO EXISTS TO PROVIDE.** Unset,
    /// this tracker behaves exactly as it always has: the gate measures through
    /// whichever window the fitted speed selected, which is the loop that opens
    /// the filter to seventy-five hertz on a sender working at fourteen words a
    /// minute. Set, the gate's width stops depending on a number the gate's own
    /// width helped produce, and **the survey is untouched either way** — which
    /// is the whole of what this separation delivers and the reason it was worth
    /// building before anybody chose a width.</para>
    /// <para>**IT IS UNSET IN `src`, DELIBERATELY.** Every width was swept and no
    /// single fixed one is right: thirty reads most of the widths that invent
    /// nothing, and it costs the easy tier, which sends at twelve words a minute
    /// and had a fifty millisecond window before. The choice is a judgement
    /// between real captures and synthesized fixtures and it is Tim's.</para>
    /// <para>Bounded by <see cref="MaximumWindowSamples"/>, which is what the ring
    /// holds. A gate asking for more audio than has been kept would be reading
    /// whatever was in the buffer before, so the request is clamped rather than
    /// honoured (§0.0).</para>
    /// </remarks>
    public int? GateWindowHops { get; set; }

    /// <summary>How many samples the gate actually looks through.</summary>
    public int GateWindowSamples => HopSamples * GateHops;

    private int GateHops => GateWindowHops is { } hops
        ? Math.Clamp(hops, 1, MaximumWindowSamples / HopSamples)
        : _windowHops;

    /// <summary>How long one hop lasts.</summary>
    public TimeSpan HopDuration => TimeSpan.FromSeconds((double)HopSamples / SampleRate);

    /// <summary>The pitch currently being followed, in hertz.</summary>
    /// <remarks>
    /// **NOT THE BIN THAT WON THIS INSTANT.** The bank is read every measurement
    /// and the loudest bin follows the signal's drift, which is what the gate
    /// wants and is far too twitchy to put on a screen: between two elements the
    /// winner is whichever bin the noise favored. What is reported is where the
    /// survey found the keying, which is a measurement over three seconds, and
    /// the middle of the bank until it has one.
    /// </remarks>
    public double ToneHz => double.IsNaN(_reportedHz)
        ? _fineHz[_fineHz.Length / 2]
        : _reportedHz;

    /// <summary>Watches for the operator's own transmissions (HM-DEC-095).</summary>
    public CwTransmitGuard Guard { get; }

    /// <summary>
    /// True while a character is part-read, so the tracker may not move
    /// (HM-DEC-096, phase 3).
    /// </summary>
    /// <remarks>
    /// **NEVER SWITCH MID-CHARACTER.** Moving the filter part-way through a
    /// character assembles the rest of it from a different station, and what
    /// comes out is a letter nobody sent with clean timing and a healthy
    /// margin. That is the same class of confident wrong reading the
    /// truncated-evidence rule exists to prevent, and it costs at most one
    /// character to avoid (§0.0).
    /// </remarks>
    public bool MidCharacter { get; set; }

    /// <summary>What the survey last found, keying and interference alike.</summary>
    public ToneVerdict Verdict { get; private set; } = ToneVerdict.Empty;

    /// <summary>
    /// Every bin the coarse survey would admit as keying right now, for
    /// diagnosis only.
    /// </summary>
    /// <returns>One entry per admitted bin.</returns>
    public IReadOnlyList<KeyingCandidate> CoarseCandidates() => _survey.Candidates();

    /// <summary>
    /// Where the strongest tone in the fine bank actually sits, between the bins.
    /// </summary>
    /// <remarks>
    /// <para>**A BIN IS NOT A MEASUREMENT OF A PITCH, IT IS A MEASUREMENT OF A
    /// BIN.** The fine bank is five hertz apart and the coarse bank
    /// twenty-five, so the best either can say on its own is "somewhere within
    /// half a spacing". A quadratic through the strongest bin and its two
    /// neighbours, in decibels, puts the peak where the three levels say it is,
    /// which is the standard reading of a transform peak and costs three
    /// numbers.</para>
    /// <para>**IT MATTERS BECAUSE A LOCK IS ONLY AS GOOD AS THE PITCH IT
    /// HOLDS.** Holding a bin centre means holding a filter pointed up to two
    /// and a half hertz off, for as long as the lock lasts, with nothing left to
    /// correct it.</para>
    /// <para>Returns <see cref="double.NaN"/> where the bank has not been filled
    /// or the peak sits at its edge, because an edge peak has no neighbour on one
    /// side and the interpolation would be an extrapolation (§0.0).</para>
    /// </remarks>
    public double MeasuredPeakHz
    {
        get
        {
            if (_ringFill < _windowHops * HopSamples)
            {
                return double.NaN;
            }

            var best = 0;

            for (var f = 1; f < _fineDb.Length; f++)
            {
                if (_fineDb[f] > _fineDb[best])
                {
                    best = f;
                }
            }

            if (best == 0 || best == _fineDb.Length - 1)
            {
                return double.NaN;
            }

            var left = _fineDb[best - 1];
            var here = _fineDb[best];
            var right = _fineDb[best + 1];

            var curvature = left - (2 * here) + right;

            if (Math.Abs(curvature) < 1e-9)
            {
                return _fineHz[best];
            }

            // The vertex of the parabola through the three, in bins, clamped to
            // half a bin either way: a larger offset means the three levels do
            // not describe a peak and the reading is not to be trusted.
            var offset = Math.Clamp(0.5 * (left - right) / curvature, -0.5, 0.5);

            return _fineHz[best] + (offset * FineSpacingHz);
        }
    }

    /// <summary>
    /// Somebody else keying inside the same passband, if the survey found one.
    /// </summary>
    /// <remarks>
    /// <para>**PLUMBING, NOT DETECTION.** The survey has admitted every keyed bin
    /// for as long as it has existed and <see cref="CoarseCandidates"/> has handed
    /// them out; what was missing was any caller. This picks the loudest admitted
    /// bin that is far enough from the tracked station to be somebody else, and
    /// says how far away and how loud, relative to what is being read.</para>
    /// <para>**NULL MEANS THE SURVEY DID NOT FIND ONE AND NOTHING MORE**
    /// (HM-DEC-009). It is not a report that the frequency is clear: the survey
    /// needs three seconds of history and eight clean marks before it admits
    /// anything at all, so a station that has just started, or one sending too
    /// little to cluster, is absent here and present on the air.</para>
    /// <para>**AND IT IS SILENT WHILE NOTHING IS BEING READ.** A competitor is
    /// defined against the station Hamlet is reading, so with no keyed verdict
    /// there is nothing for an offset to be an offset from, and the loudest bin
    /// on an empty band would otherwise be announced as somebody in the way.</para>
    /// </remarks>
    public CwCompetitor? Competitor
    {
        get
        {
            if (Verdict.Keyed is not { } reading)
            {
                return null;
            }

            CwCompetitor? worst = null;

            foreach (var candidate in _survey.Candidates())
            {
                var offset = candidate.ToneHz - reading.ToneHz;

                if (Math.Abs(offset) < CwCompetitor.SeparationHz
                    || double.IsNaN(candidate.LiftDb)
                    || double.IsNaN(reading.LiftDb))
                {
                    continue;
                }

                var relative = candidate.LiftDb - reading.LiftDb;

                if (relative < CwCompetitor.QuietestWorthSayingDb)
                {
                    continue;
                }

                if (worst is not { } loudest || relative > loudest.RelativeDb)
                {
                    worst = new CwCompetitor(offset, relative, candidate.ToneHz);
                }
            }

            return worst;
        }
    }

    /// <summary>
    /// How many times the tracker has moved to a different part of the band.
    /// </summary>
    /// <remarks>
    /// **WHAT WAS MEASURED AT THE OLD PITCH IS NOT EVIDENCE ABOUT THE NEW ONE**
    /// (HM-DEC-095). Everything the decoder is holding when this changes was
    /// measured through a filter pointed somewhere else, and letting it through
    /// turns the first seconds of a signal found off-frequency into a row of
    /// placeholders. Counting the moves is how the decoder knows to drop it.
    /// </remarks>
    public int Retunes { get; private set; }

    /// <summary>
    /// How many times the tracker left a station it had found for another one.
    /// </summary>
    /// <remarks>
    /// **NOT THE SAME AS A FOLLOW, AND THE DIFFERENCE IS ACQUISITION.** The first
    /// jump off the operator's configured pitch counts as a follow, correctly:
    /// the filter moved to a different part of the band. But nothing had been
    /// found yet, so nobody was abandoned. This counts only the moves made after
    /// keying has been confirmed somewhere, which are the ones that mean somebody
    /// else has started sending.
    /// <para>The decoder empties its window on this and not on a follow, because
    /// emptying it on acquisition throws away the opening of the message it has
    /// just found (HM-DEC-009 cuts both ways: the callsign at the front of a call
    /// is exactly what the operator needs).</para>
    /// </remarks>
    public int StationChanges { get; private set; }

    /// <summary>
    /// How many of those moves were to a different station (HM-DEC-123).
    /// </summary>
    /// <remarks>
    /// <para>**A REFINEMENT AND A FOLLOW ARE NOT THE SAME EVENT, AND ONE LINE
    /// TREATED THEM AS ONE.** Every move used to throw the settled pass's window
    /// away, because HM-DEC-096 put it there and a move usually does mean
    /// somebody else started transmitting. Sometimes it means the survey
    /// preferred its neighbouring bin on the station already being read, and on
    /// a thirty second capture two of those cost the callsign.</para>
    /// <para>**THE CRITERION IS MEASURED AND IT IS THE SURVEY'S OWN GRID.** Across
    /// every recording this repository holds, a move within one station is
    /// exactly one coarse bin — twenty-five hertz — and the one genuine station
    /// change, the caller at 615 handing over to the answerer at 730 in the
    /// two-station fixture, is a hundred. There is nothing between them to choose
    /// from. <see cref="ConfirmWithinHz"/> already carries this number with this
    /// meaning for a different question: two consecutive surveys agreeing within
    /// one bin are the same signal, "a station drifting or the survey preferring
    /// its neighbor, rather than a different signal". The distinction did not
    /// need inventing, only reading.</para>
    /// <para>**AND IT ONLY APPLIES TO A TRACKER THAT WAS READING SOMETHING.**
    /// Before a pitch has been reported there is nothing to refine and everything
    /// held was measured through a filter pointed at empty band, so an
    /// acquisition move is a follow.</para>
    /// </remarks>
    public int Follows { get; private set; }

    /// <summary>
    /// True when something on the band is actually being keyed (HM-DEC-095).
    /// </summary>
    /// <remarks>
    /// Distinct from there being energy at the tracked pitch. A carrier, a
    /// switching supply and an empty band all put energy somewhere; only a person
    /// sending puts it there in two lengths.
    /// </remarks>
    public bool HasKeying => Verdict.Keyed is not null;

    /// <summary>
    /// True when keying was found within the last few seconds (HM-DEC-096).
    /// </summary>
    /// <remarks>
    /// **THE SETTLED PASS READS AUDIO THE SURVEY HAS ALREADY MOVED PAST**, by
    /// design: it trails the leading edge so it can fit a threshold to a stretch
    /// it has heard all of. Gating it on whether somebody is keying *right now*
    /// therefore asks the wrong question and answers it wrongly in both
    /// directions, and in practice suppressed the whole of a real decode.
    /// </remarks>
    public bool KeyingRecently => _keyedProtects > 0;

    /// <summary>
    /// Follow the sending speed, which decides how finely the tracker listens.
    /// </summary>
    /// <param name="wordsPerMinute">The speed, or zero when it is not known.</param>
    public void FollowSpeed(double wordsPerMinute)
    {
        // **HYSTERESIS, BECAUSE THE SPEED ESTIMATE JITTERS AND THE WINDOW MUST
        // NOT** (HM-DEC-095). A bare threshold at eighteen words a minute sat
        // exactly where the commonest sending speed is, so an estimate wandering
        // a word either side of it rebuilt the filter every few characters and
        // changed the scale every measurement was being judged against. Signals
        // eighteen decibels out of the noise came back a third wrong, which is
        // worse than the same decoder managed at ten.
        var wanted = _windowHops;

        if (wordsPerMinute <= 0)
        {
            wanted = AcquireWindowHops;
        }
        else if (wordsPerMinute > FastFistWpm + SpeedHysteresisWpm)
        {
            wanted = FastWindowHops;
        }
        else if (wordsPerMinute < FastFistWpm - SpeedHysteresisWpm)
        {
            wanted = NarrowWindowHops;
        }

        if (wanted != _windowHops)
        {
            _windowHops = wanted;
            BuildHann();
        }
    }

    /// <summary>
    /// Feed samples, calling back once per hop.
    /// </summary>
    /// <param name="samples">The samples.</param>
    /// <param name="firstSampleIndex">Index of the first sample in the stream.</param>
    /// <param name="onReading">Called for each completed measurement.</param>
    public void Process(
        ReadOnlySpan<float> samples, long firstSampleIndex, Action<ToneReading> onReading)
    {
        for (var i = 0; i < samples.Length; i++)
        {
            _ring[_ringWrite] = samples[i];
            _ringWrite = (_ringWrite + 1) % MaximumWindowSamples;

            if (_ringFill < MaximumWindowSamples)
            {
                _ringFill++;
            }

            _samplesSeen = firstSampleIndex + i + 1;
            _hopFill++;

            if (_hopFill < HopSamples || _ringFill < WindowSamples)
            {
                continue;
            }

            _hopFill = 0;
            onReading(Measure());
        }
    }

    /// <summary>The Goertzel coefficient for one pitch.</summary>
    private double Coefficient(double hz) => 2 * Math.Cos(2 * Math.PI * hz / SampleRate);

    /// <summary>Point the fine bank at a region.</summary>
    private void CenterFineBank(double centerHz)
    {
        var clamped = Math.Clamp(centerHz, MinimumToneHz, MaximumToneHz);

        for (var i = 0; i < _fineHz.Length; i++)
        {
            _fineHz[i] = clamped - FineReachHz + (i * FineSpacingHz);
            _fineCoefficient[i] = Coefficient(_fineHz[i]);
        }
    }

    /// <summary>Build the gate's taper for a width of its own.</summary>
    /// <param name="length">How many samples it spans.</param>
    private void BuildGateHann(int length)
    {
        for (var i = 0; i < length; i++)
        {
            _gateHann[i] = (float)(0.5 - (0.5 * Math.Cos(2 * Math.PI * i / (length - 1))));
        }

        _gateHannHops = length;
    }

    /// <summary>Rebuild the taper for the current window length.</summary>
    private void BuildHann()
    {
        var length = WindowSamples;

        for (var i = 0; i < length; i++)
        {
            _hann[i] = (float)(0.5 - (0.5 * Math.Cos(2 * Math.PI * i / (length - 1))));
        }

        _hannHops = _windowHops;
    }

    /// <summary>
    /// One measurement over the current window.
    /// </summary>
    private ToneReading Measure()
    {
        if (_hannHops != _windowHops)
        {
            BuildHann();
        }

        var window = WindowSamples;
        var sumSquares = Taper(_scratch, _hann, window);

        // **THE GATE'S OWN BUFFER, WHEN IT WANTS A DIFFERENT WIDTH.** Unset, this
        // is the survey's buffer and every figure below is the arithmetic this
        // tracker has always done. Set, the gate reads its own taper and the
        // survey keeps the one the fitted speed gave it.
        var gateWindow = GateWindowSamples;
        var gateScratch = _scratch;

        if (gateWindow != window)
        {
            if (_gateHannHops != gateWindow)
            {
                BuildGateHann(gateWindow);
            }

            Taper(_gateScratch, _gateHann, gateWindow);
            gateScratch = _gateScratch;
        }

        // **BROADBAND, NOT AT THE TONE.** A receiver muting takes the whole audio
        // band down together and a signal fading takes one note down, so the only
        // measurement that recognizes the operator's own transmission is the one
        // that looks at everything (HM-DEC-095).
        var broadband = 20 * Math.Log10(Math.Sqrt(sumSquares / window) + 1e-12);
        var blocked = Guard.Observe(broadband);

        // **THE LOUDEST OF THE WHOLE FINE BANK, NOT ONE BIN OF IT** (HM-DEC-095).
        // The station in the recording drifts a few hertz across its own
        // transmission, which is ordinary for a radio warming up, and a single bin
        // watches it walk away. Reading the bank and taking whichever bin is
        // loudest follows the drift for nothing, and the bin that won is also the
        // best available reading of where the note actually is.
        //
        // Taken from the validated reference chain, where it is what makes the
        // envelope usable at all on a signal at the edge of readability.
        // **WHICH BIN WINS IS THE SURVEY'S QUESTION AND NOT THE GATE'S.** Where
        // the station sits is a fact about frequency, and reading it through a
        // taper chosen for its length in time was measured and it costs the
        // tracker its aim: station-finding goes red across the whole displacement
        // suite. So the bank is read through the survey's window, exactly as it
        // always was, and the gate is told which bin to measure.
        var strongest = 0.0;

        for (var f = 0; f < _fineHz.Length; f++)
        {
            var power = Goertzel(_scratch, _fineCoefficient[f], window);

            _fineDb[f] = ToDb(power);

            if (power > strongest)
            {
                strongest = power;
                _tracked = f;
            }
        }

        // **AND HOW LOUD IT IS RIGHT NOW IS THE GATE'S.** That is a fact about
        // this instant, and it is the one measurement the whole separation exists
        // to free from a window chosen by the fitted speed.
        var trackedPower = gateWindow == window
            ? strongest
            : Goertzel(gateScratch, _fineCoefficient[_tracked], gateWindow);

        var trackedHz = _fineHz[_tracked];
        var competitorPower = 0.0;
        var neighbors = 0;

        // The survey runs on a ten millisecond grid, which is what the validated
        // receive chain uses and half the work of running it every hop.
        var surveying = ++_surveyPhase >= SurveyDecimation;

        if (surveying)
        {
            _surveyPhase = 0;
        }

        for (var b = 0; b < _coarseHz.Length; b++)
        {
            var near = Math.Abs(_coarseHz[b] - trackedHz) < CompetitorSeparationHz;

            if (!surveying && near)
            {
                continue;
            }

            // **THE SURVEY'S BINS COME FROM THE SURVEY'S WINDOW**, so what it is
            // handed is exactly what it was handed before this separation
            // existed.
            var power = Goertzel(_scratch, _coarseCoefficient[b], window);

            if (surveying)
            {
                _coarseDb[b] = ToDb(power);
            }

            if (near)
            {
                continue;
            }

            // **AND THE NOISE BESIDE THE TONE COMES FROM THE GATE'S**, because
            // the two sides of `SnrDb` have to be measured through one filter.
            // Taking the tone at one bandwidth and the noise at another gives a
            // difference that is a fact about the two filters rather than about
            // the band, and it would read as signal where there is none (§0.0).
            var gatePower = gateWindow == window
                ? power
                : Goertzel(gateScratch, _coarseCoefficient[b], gateWindow);

            if (gatePower > competitorPower)
            {
                competitorPower = gatePower;
            }

            // Far enough out that the tone itself does not reach, which is what
            // makes these a sample of the band rather than of the signal.
            _neighbors[neighbors++] = gatePower;
        }

        if (surveying)
        {
            _survey.Observe(_coarseDb, blocked);
            _fineSurvey.Observe(_fineDb, blocked);
        }

        if (++_hopsSinceSurvey >= SurveyEveryHops)
        {
            _hopsSinceSurvey = 0;
            ReadSurvey();
        }

        return new ToneReading(
            ToDb(trackedPower), ToDb(competitorPower), trackedHz, _samplesSeen,
            ToDb(NoiseFrom(neighbors)), broadband, blocked);
    }

    /// <summary>
    /// Take the survey's answer and point the fine bank at it.
    /// </summary>
    /// <remarks>
    /// **NOTHING MOVES WHEN NOTHING IS KEYING.** The old tracker retuned on every
    /// pass to whichever bin was loudest, so on an empty band it wandered and
    /// reported a pitch for noise. If there is no keying candidate the tracker
    /// stays exactly where it is and says so, which is what lets the decoder
    /// tell an empty band from a signal it cannot read (§0.0).
    /// </remarks>
    private void ReadSurvey()
    {
        // A move that was waiting for a character to finish goes now.
        if (!double.IsNaN(_heldSwitchHz) && !MidCharacter)
        {
            Switch(_heldSwitchHz);
            _heldSwitchHz = double.NaN;
        }

        var coarse = _survey.Analyze();
        var previous = _previousKeyedHz;

        _previousKeyedHz = coarse.Keyed?.ToneHz ?? double.NaN;

        if (_keyedProtects > 0)
        {
            _keyedProtects--;
        }

        if (coarse.Keyed is not { } keyed)
        {
            // Nothing is keying anywhere. Keep the interference finding, because
            // a band with a carrier and nobody sending is exactly the case worth
            // reporting.
            Verdict = new ToneVerdict(
                null, Filtered(coarse.Interference ?? coarse.Strongest), coarse.Strongest);

            // **FROM COLD, POINT AT THE LOUDEST THING AND LET THE DECODER LOOK.**
            // Deciding somebody is keying takes three seconds of evidence and it
            // should, but refusing even to listen anywhere else until then leaves
            // the decoder pointed at the operator's configured pitch through the
            // opening of every signal that is not on it. That is most of a short
            // call.
            //
            // This is not a claim and does not set the verdict: it moves where
            // the filter points, nothing else, and only while nothing has ever
            // been confirmed. Once a signal has been found, the confirmation rule
            // below owns every subsequent move, because being dragged off a
            // working decode by a loud carrier is the fault this whole survey
            // exists to prevent.
            //
            // **AND NOT MID-CHARACTER EITHER** (phase 3). The rule is about
            // moving the filter, not about why it is being moved: a character
            // finished from a different part of the band is a letter nobody sent
            // however the move came to be made.
            if (!MidCharacter
                && double.IsNaN(_lastKeyedHz)
                && coarse.Strongest is { } loudest
                && Math.Abs(loudest.ToneHz - _fineHz[_fineHz.Length / 2]) > FineReachHz)
            {
                CenterFineBank(loudest.ToneHz);
                _fineSurvey.Reset();
                _tracked = _fineHz.Length / 2;
                _reportedHz = double.NaN;
                Retunes++;
                Follows++;
            }

            return;
        }

        // **A CANDIDATE HAS TO SURVIVE TWICE BEFORE THE TRACKER ACTS ON IT**
        // (HM-DEC-095). Three seconds of noise occasionally produces eight marks
        // that cluster convincingly, and one such fluke was enough to announce
        // keying on a recording that has none. Two agreeing surveys half a second
        // apart rest on six seconds of evidence, and noise does not repeat itself
        // in the same bin (§0.0).
        //
        // **MOVING THE FILTER BEFORE CONFIRMING WAS TRIED AND IS WORSE**, on the
        // reasoning that where the tracker listens is not a claim and only the
        // verdict is. It acquires an off-pitch signal two to four seconds sooner
        // and it also follows every fluke, and each move discards what the speed
        // estimator has learned. Measured across the suite it fixed four tests and
        // broke ten, including decodes that had nothing wrong with them. The delay
        // is the price of not being dragged around by noise.
        if (double.IsNaN(previous) || Math.Abs(previous - keyed.ToneHz) > ConfirmWithinHz)
        {
            // Refusing to believe it is keying does not make it stop existing.
            Verdict = new ToneVerdict(
                null, Filtered(coarse.Interference ?? coarse.Strongest), coarse.Strongest);
            return;
        }

        // **A CONFIRMED STATION IS NOT ABANDONED FOR A CANDIDATE FAR BELOW IT**
        // (HM-DEC-127). The survey scores every bin for keying structure, and a
        // station's own image in a distant bin has the station's dit, the
        // station's dah and the station's timing, because it is the station. On
        // the reads where the real bin happens not to score, that image is the
        // only candidate left and it confirms itself twice over with nothing to
        // argue against it: measured on a 400 hertz signal, the tracker left it
        // for 575 and lost the `CQ` while it was away.
        //
        // **THIS IS NOT A PREFERENCE FOR LOUDNESS AND HM-DEC-095 IS NOT AMENDED.**
        // That ruling settled which of several signals to read on an
        // empty-handed survey, where loudness picked a carrier over a station.
        // Nothing is being read then and there is nothing to abandon. Here there
        // is, and the question is different: not "prefer the louder" but "do not
        // abandon what you have for something far below it".
        //
        // **THE FLOOR IS THE FILTER'S OWN REJECTION AND IT IS ALREADY IN THIS
        // FILE.** Past a hundred and twenty-five hertz of separation the window
        // takes at least <see cref="FilterRejectionDb"/> off a rival, so anything
        // that far below the station being read is inside what that station's own
        // leakage could produce, and calling it a different station is a claim the
        // measurement does not support (§0.0). Measured across every recording
        // here, a candidate that legitimately displaces a confirmed station sits
        // between nought point three decibels above it and one and a half below;
        // the image sat thirty-five below. There is nothing in between.
        if (!double.IsNaN(_readingDb)
            && !double.IsNaN(keyed.KeyedDb)
            && keyed.KeyedDb < _readingDb - FilterRejectionDb
            && Math.Abs(keyed.ToneHz - _fineHz[_fineHz.Length / 2]) > FineReachHz)
        {
            // Refusing to follow it does not make it stop existing, and the
            // station being read is still protected by its own countdown.
            Verdict = new ToneVerdict(
                null, Filtered(coarse.Interference ?? coarse.Strongest), coarse.Strongest);

            return;
        }

        KeyingFoundAt(keyed.ToneHz);
        _readingDb = keyed.KeyedDb;

        // Outside what the fine bank can reach, so it has to move, and its
        // history is about different pitches and cannot come with it.
        if (Math.Abs(keyed.ToneHz - _fineHz[_fineHz.Length / 2]) > FineReachHz)
        {
            if (MidCharacter)
            {
                // Held until the character in progress ends (phase 3). The
                // candidate keeps being re-confirmed while it waits, so a switch
                // deferred is not a switch abandoned.
                _heldSwitchHz = keyed.ToneHz;
                Verdict = new ToneVerdict(keyed, Filtered(coarse.Interference));
                return;
            }

            Switch(keyed.ToneHz);
            Verdict = new ToneVerdict(keyed, Filtered(coarse.Interference));
            return;
        }

        // Inside reach: the fine bank's own reading of the keying is the answer,
        // and the coarse one only said where to look.
        var refined = _fineSurvey.Analyze();

        if (refined.Keyed is { } exact)
        {
            _tracked = NearestFine(exact.ToneHz);

            // The survey chose this note by its keying; this only says where it
            // is (HM-DEC-095).
            _reportedHz = Refined(exact.ToneHz);
            KeyingFoundAt(exact.ToneHz);
            Verdict = new ToneVerdict(exact, Filtered(coarse.Interference));
            return;
        }

        _tracked = NearestFine(keyed.ToneHz);
        _reportedHz = Refined(keyed.ToneHz);
        Verdict = new ToneVerdict(keyed, Filtered(coarse.Interference));
    }

    /// <summary>
    /// Drop an interference finding that is really a station that has stopped.
    /// </summary>
    /// <remarks>
    /// **A STATION THAT HAS JUST FINISHED SENDING IS NOT INTERFERENCE.** The
    /// survey looks back three seconds, so for a few seconds after somebody stops
    /// their energy is still in the history with no keying left in it, and naming
    /// the person who was just answering as a source of interference is both
    /// wrong and insulting (§0.0).
    /// </remarks>
    private ToneInterference? Filtered(ToneInterference? found)
        => found is { } noise
            && _keyedProtects > 0
            && !double.IsNaN(_lastKeyedHz)
            && Math.Abs(noise.ToneHz - _lastKeyedHz) < CompetitorSeparationHz
                ? null
                : found;

    /// <summary>Remember that keying was found here, for a while.</summary>
    private void KeyingFoundAt(double toneHz)
    {
        _lastKeyedHz = toneHz;

        // Six surveys is three seconds, which is exactly how long the survey's
        // own history takes to forget a station that has stopped.
        _keyedProtects = 6;
    }

    /// <summary>
    /// Move the fine bank to a different part of the band (HM-DEC-096, phase 3).
    /// </summary>
    /// <remarks>
    /// **A SWITCH IS A CLOCK-LOSS EVENT.** Operationally a pitch change and a
    /// speed change are the same thing: somebody else started transmitting.
    /// Everything the decoder holds was measured through a filter pointed
    /// somewhere else, and the settled pass's window is full of a station that
    /// is no longer being read.
    /// </remarks>
    private void Switch(double toneHz)
    {
        // Measured against the bank the tracker is listening through rather than
        // against the pitch it last reported, because the reported pitch is the
        // fine bank's answer and can sit a few hertz outside its own centre: on
        // the two-station recording the answerer reports 730 through a bank
        // centred at 725, and calling that a thirty hertz move would make the
        // survey's own grid read as one and a bit bins.
        var moved = Math.Abs(toneHz - _fineHz[_fineHz.Length / 2]);
        var refining = !double.IsNaN(_reportedHz) && moved <= ConfirmWithinHz;

        CenterFineBank(toneHz);
        _fineSurvey.Reset();
        _tracked = _fineHz.Length / 2;

        // The survey chose this station by its keying; the reported pitch is
        // where it actually sits rather than which bin found it (HM-DEC-095).
        _reportedHz = Refined(toneHz);
        KeyingFoundAt(toneHz);
        Retunes++;

        if (!refining)
        {
            Follows++;
            StationChanges++;
        }
    }

    /// <summary>The fine bin nearest a pitch.</summary>
    private int NearestFine(double hz)
    {
        var best = 0;

        for (var i = 1; i < _fineHz.Length; i++)
        {
            if (Math.Abs(_fineHz[i] - hz) < Math.Abs(_fineHz[best] - hz))
            {
                best = i;
            }
        }

        return best;
    }

    /// <summary>
    /// Copy the newest samples into a buffer through a taper.
    /// </summary>
    /// <param name="scratch">Where the tapered samples go.</param>
    /// <param name="hann">The taper, already the right length.</param>
    /// <param name="window">How many samples to take.</param>
    /// <returns>The sum of the squares of the untapered samples.</returns>
    private double Taper(float[] scratch, float[] hann, int window)
    {
        var start = (_ringWrite - window + MaximumWindowSamples) % MaximumWindowSamples;
        var sumSquares = 0.0;

        for (var i = 0; i < window; i++)
        {
            var raw = _ring[(start + i) % MaximumWindowSamples];

            sumSquares += (double)raw * raw;
            scratch[i] = raw * hann[i];
        }

        return sumSquares;
    }

    /// <summary>Goertzel power over the scratch buffer at one coefficient.</summary>
    private double Goertzel(double coefficient, int length)
        => Goertzel(_scratch, coefficient, length);

    /// <summary>
    /// Where the admitted candidate actually sits, between the bins.
    /// </summary>
    /// <param name="aroundHz">The pitch the survey admitted.</param>
    /// <returns>The refined pitch, or the same one if it cannot be improved.</returns>
    /// <remarks>
    /// <para>**THIS REFINES A CHOICE ALREADY MADE AND NEVER MAKES ONE**
    /// (HM-DEC-095). That ruling settles that a note is chosen by how it is keyed
    /// and never by how loud it is, and a transform peak is a loudness
    /// measurement. So it is not allowed to pick a candidate: the survey has
    /// already admitted one on its keying structure, and this only asks where
    /// that candidate is, more precisely than a bank five hertz apart can say.
    /// It searches a neighbourhood narrower than the coarse spacing for the same
    /// reason — far enough to find the station the survey admitted, not far
    /// enough to reach a different one.</para>
    /// <para>**A BIN CENTRE IS A MEASUREMENT OF A BIN.** The coarse bank is
    /// twenty-five hertz apart and the fine bank five, so before this the
    /// reported pitch was quantised to a grid nobody transmits on. Measured
    /// across the corpus, the reported pitch was out by up to twelve hertz on
    /// recordings whose station the survey had found perfectly well.</para>
    /// <para>**IT REPORTS WHAT IT MEASURED OR IT LEAVES THE PITCH ALONE.** Where
    /// the peak lands at the edge of the neighbourhood the quadratic would be an
    /// extrapolation, and an extrapolated pitch is a number nobody measured
    /// (§0.0).</para>
    /// </remarks>
    private double Refined(double aroundHz)
    {
        var window = Math.Min(_ringFill, _windowHops * HopSamples);

        if (window < HopSamples * 8)
        {
            return aroundHz;
        }

        Taper(_scratch, _hann, window);

        var best = aroundHz;
        var bestPower = -1.0;
        var below = 0.0;
        var above = 0.0;

        for (var offset = -RefineReachHz; offset <= RefineReachHz; offset += RefineStepHz)
        {
            var hz = aroundHz + offset;

            if (hz < MinimumToneHz || hz > MaximumToneHz)
            {
                continue;
            }

            var power = Goertzel(_scratch, Coefficient(hz), window);

            if (power > bestPower)
            {
                bestPower = power;
                best = hz;
            }
        }

        if (best <= aroundHz - RefineReachHz || best >= aroundHz + RefineReachHz)
        {
            return aroundHz;
        }

        below = Goertzel(_scratch, Coefficient(best - RefineStepHz), window);
        above = Goertzel(_scratch, Coefficient(best + RefineStepHz), window);

        var l = Math.Log(Math.Max(below, 1e-30));
        var c = Math.Log(Math.Max(bestPower, 1e-30));
        var r = Math.Log(Math.Max(above, 1e-30));

        var curve = l - (2 * c) + r;

        if (Math.Abs(curve) < 1e-12)
        {
            return best;
        }

        return best + (Math.Clamp(0.5 * (l - r) / curve, -0.5, 0.5) * RefineStepHz);
    }

    /// <summary>Goertzel power over one tapered buffer at one coefficient.</summary>
    /// <param name="scratch">The tapered samples.</param>
    /// <param name="coefficient">The bin.</param>
    /// <param name="length">How many of them to read.</param>
    /// <returns>Power, normalised by the window length.</returns>
    private static double Goertzel(float[] scratch, double coefficient, int length)
    {
        var s1 = 0.0;
        var s2 = 0.0;

        for (var i = 0; i < length; i++)
        {
            var s0 = scratch[i] + (coefficient * s1) - s2;
            s2 = s1;
            s1 = s0;
        }

        var power = (s1 * s1) + (s2 * s2) - (coefficient * s1 * s2);
        return Math.Max(0, power) / ((double)length * length);
    }

    /// <summary>
    /// What the band is doing beside the tone (HM-DEC-088).
    /// </summary>
    /// <param name="count">How many neighboring bins were collected.</param>
    /// <returns>A noise power, or NaN when there was nothing to look at.</returns>
    /// <remarks>
    /// **THE MEDIAN, NOT THE MEAN**, and that is the whole reason this works on a
    /// busy band. A second station sitting in one or two of these bins drags a
    /// mean upward and takes the threshold with it, which would make the decoder
    /// deaf exactly when somebody is calling nearby. A median ignores any minority
    /// of loud bins entirely.
    /// </remarks>
    private double NoiseFrom(int count)
    {
        if (count == 0)
        {
            return double.NaN;
        }

        Array.Sort(_neighbors, 0, count);

        return _neighbors[count / 2];
    }

    /// <summary>Power in decibels, with a floor so silence is a number.</summary>
    private static double ToDb(double power) => 10 * Math.Log10(power + 1e-14);
}
