using System.Globalization;
using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Cw;

/// <summary>One hop as the scope draws it.</summary>
/// <param name="EnvelopeDb">The level of the bin being watched this hop.</param>
/// <param name="FloorDb">That bin's gap level, measured over the last second; NaN when it has no gaps.</param>
/// <param name="ThresholdDb">Midway between its gap level and its bar level, measured; NaN when either is.</param>
/// <param name="Mark">Whether this hop sat inside a bar that has a partner across a gap.</param>
public readonly record struct CwScopeHop(double EnvelopeDb, double FloorDb, double ThresholdDb, bool Mark);

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
/// <param name="PitchHz">While a mark is up, the bin with the most bars in the last second; NaN otherwise.</param>
/// <param name="ContrastDb">While a mark is up, that bin's bar level over its gap level; NaN otherwise.</param>
/// <param name="PassbandLowHz">The lowest bin's edge of the sweep.</param>
/// <param name="PassbandHighHz">The highest.</param>
/// <param name="PassbandFromRig">Whether the edges came from the radio's pitch and filter.</param>
/// <param name="MarksLast4s">How many marks the last four seconds hold.</param>
/// <param name="Keying">Whether any bin in the passband made two bars and a gap in the last second.</param>
/// <param name="Pointed">Whether the watched bin is the one the radio's scope points at, not the sweep's.</param>
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
    bool Pointed = false)
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
    /// **THE SURVEY'S OWN FLOOR** (<see cref="CwToneSurvey.ShortestDitMs"/>): twenty-five
    /// milliseconds is forty-eight words a minute, the fastest the radio's own keyer goes. A
    /// run's length is the audio its windows cover: the first window's start to the last one's
    /// end.
    /// </remarks>
    public const double ShortestBarMs = CwToneSurvey.ShortestDitMs;

    /// <summary>How far apart the bins are, in hertz: the keying meter's step.</summary>
    public const double BinSpacingHz = KeyingEnvelope.ToneStepHz;

    /// <summary>How recent two bars and their gap must be for a bin to be keying, in seconds.</summary>
    /// <remarks>
    /// **A DIT AND A DIT, NOT EIGHT MARKS AND THREE SECONDS** (work instruction 477). Two bars
    /// in a second is a dit, a space and a dit at any speed from five words a minute up, which
    /// is what the owner hears as somebody sending.
    /// </remarks>
    public const double KeyingSeconds = 1;

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
    private int _ringWrite;
    private int _ringFill;
    private int _hopFill;
    private long _hop;
    private double _lowHz = double.NaN;
    private double _highHz = double.NaN;
    private bool _fromRig;
    private double _loudestGapDb = double.NaN;
    private double _pointedHz = double.NaN;
    private CwEnvelopeReading _reading = CwEnvelopeReading.None;

    private IAudioSource? _attached;

    /// <summary>Creates a detector for one sample rate.</summary>
    /// <param name="sampleRate">Samples per second of the audio it will be fed.</param>
    public CwEnvelopeDetector(int sampleRate)
    {
        SampleRate = Math.Max(1_000, sampleRate);

        // **THE DECODER'S OWN HOP**, five milliseconds, so a hop here and a hop in
        // `CwToneTracker` are the same slice of time.
        HopSamples = Math.Max(4, SampleRate / 200);

        // Each bin's level is read over two hops, ten milliseconds: a 25 ms dit still holds
        // three or four whole windows, and the window is short enough that its edges smear a
        // keying edge by one hop and no more.
        EnvelopeWindowSamples = 2 * HopSamples;

        _ring = new float[EnvelopeWindowSamples];
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

        SetPassband(null, null);
    }

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
                .Select(k => new Bin(k * BinSpacingHz, SampleRate, HistoryHops + (2 * _keyingHops), _keyingHops))
                .ToArray();
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

    /// <summary>
    /// **THE RADIO POINTS** (work instruction 480 task 2, R94, HM-DEC-188): watch the bin nearest
    /// the pitch the radio's scope reports, or sweep again when it reports none.
    /// </summary>
    /// <param name="pitchHz">The pitch <see cref="CwScopePointer"/> gives, or null while the scope is quiet.</param>
    /// <remarks>
    /// <para>**WHILE THE SCOPE POINTS, THERE IS NO SWEEP.** The watched bin is the pointed one on
    /// every hop, so the bars, the contrast and the pitch all come from it; the other bins go on
    /// being measured, and are what the sweep picks from again the moment the pointer is null.</para>
    /// <para>A pitch outside the bins, beyond the passband's edge by more than half a bin, is not
    /// one the detector hears, and it sweeps as if nothing pointed.</para>
    /// </remarks>
    public void PointAt(double? pitchHz)
    {
        lock (_gate)
        {
            _pointedHz = pitchHz is > 0 and var hz ? hz : double.NaN;
        }
    }

    /// <summary>The pitch of the bin being watched, pointed or swept.</summary>
    public double WatchedHz
    {
        get
        {
            lock (_gate)
            {
                return _bins.Length > 0 ? _bins[_watched].Hz : double.NaN;
            }
        }
    }

    /// <summary>The pitch the radio's scope points at, or NaN while it points nowhere.</summary>
    public double PointedHz
    {
        get
        {
            lock (_gate)
            {
                return _pointedHz;
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
                _ringWrite = (_ringWrite + 1) % _ring.Length;
                _ringFill = Math.Min(_ringFill + 1, _ring.Length);

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

    /// <summary>The last four seconds of the bin being watched, oldest first.</summary>
    /// <returns>One entry per hop.</returns>
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
            var bars = Evaluate(bin).Marked;
            var oldest = _hop - fill;
            var b = 0;

            for (var i = 0; i < fill; i++)
            {
                var hop = oldest + i;

                while (b < bars.Count && bars[b].End < hop)
                {
                    b++;
                }

                var mark = b < bars.Count && bars[b].Start <= hop;

                copy[i] = new CwScopeHop(bin.Level(hop), _reading.FloorDb, _reading.ThresholdDb, mark);
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

            bin.Add(hop, db, bin.ContrastDb, under);
            bin.Prune(hop - HistoryHops - _keyingHops);
        }

        // **THE PITCH IS THE KEYING BIN WITH THE MOST BARS IN THE LAST SECOND, AMONG THE BINS
        // THAT HEAR THE BARS AT THEIR FULL LEVEL.** A tone lights every bin its ten millisecond
        // window reaches, about two hundred hertz either side, with the same bars; the ones off
        // the tone hear it quieter, where the noise splits a bar in two and counts it twice.
        // So only bins whose bars sit within the flat tolerance of the loudest keying bin's are
        // counted, and the louder breaks a tie. Where nothing is keying the watched bin stays
        // where it was, so the trace goes on showing where the last station was.
        var evals = new Evaluation[_bins.Length];
        var keying = false;
        _loudestGapDb = double.NaN;

        for (var i = 0; i < _bins.Length; i++)
        {
            var e = evals[i] = Evaluate(_bins[i]);
            keying |= e.Keying;

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
                keying |= e.Keying;
            }

            _bins[i].ContrastDb = contrast;

            if (!double.IsNaN(gap) && !(gap <= _loudestGapDb))
            {
                _loudestGapDb = gap;
            }
        }

        // How far a tone reaches across bins: the Hann window's main lobe, two bins of its
        // own resolution either side - two hundred hertz at ten milliseconds.
        var reach = (int)Math.Ceiling(2.0 * SampleRate / EnvelopeWindowSamples / BinSpacingHz);
        var best = -1;
        Evaluation? bestEval = null;

        // **THE RADIO POINTS; THE SWEEP IS THE FALLBACK** (work instruction 480, R94). While the
        // scope names a pitch inside the bins, that bin is watched and nothing is searched.
        var pointed = Pointed();

        for (var i = 0; pointed < 0 && i < _bins.Length; i++)
        {
            var e = evals[i];

            if (!e.Keying)
            {
                continue;
            }

            var loudest = double.NegativeInfinity;

            for (var j = Math.Max(0, i - reach); j <= Math.Min(_bins.Length - 1, i + reach); j++)
            {
                loudest = Math.Max(loudest, evals[j].LoudestBarDb);
            }

            if (e.BarDb < loudest - FlatToleranceDb)
            {
                continue;
            }

            if (bestEval is not { } top
                || e.BarsLastSecond > top.BarsLastSecond
                || (e.BarsLastSecond == top.BarsLastSecond && e.BarDb > top.BarDb))
            {
                best = i;
                bestEval = e;
            }
        }

        if (pointed >= 0)
        {
            _watched = pointed;
        }
        else if (best >= 0)
        {
            _watched = best;
        }

        var watched = _bins[_watched];
        var eval = evals[_watched];
        var open = watched.Open!;
        var up = eval.Marked.Count > 0 && eval.Marked[^1].End == hop;

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
            up ? watched.Hz : double.NaN,
            up && !double.IsNaN(gapDb) ? barDb - gapDb : double.NaN,
            _lowHz,
            _highHz,
            _fromRig,
            eval.Marked.Count(m => m.End > hop - HistoryHops),
            keying,
            pointed >= 0);
    }

    /// <summary>The bin nearest the pointed pitch, or -1 where nothing points or it is off the bins.</summary>
    private int Pointed()
    {
        if (double.IsNaN(_pointedHz) || _bins.Length == 0)
        {
            return -1;
        }

        var nearest = 0;

        for (var i = 1; i < _bins.Length; i++)
        {
            if (Math.Abs(_bins[i].Hz - _pointedHz) < Math.Abs(_bins[nearest].Hz - _pointedHz))
            {
                nearest = i;
            }
        }

        return Math.Abs(_bins[nearest].Hz - _pointedHz) <= BinSpacingHz / 2 ? nearest : -1;
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

        // **A GAP**: every run between two consecutive bars wholly below the lower of them -
        // it dropped and stayed down - and the two bars at one level, their bands overlapping,
        // because a station holds the same level from one element to the next. A pair further
        // apart than a second is two things heard, not one sender.
        // Measured only where there are two bars to pair: most bins, most hops, have none.
        var waves = bars.Count >= 2 ? Waves(bin, runs, bars, recent, now) : (Level: double.PositiveInfinity, Wander: 0.0);
        var marked = new List<Span>();
        var gaps = 0;
        var keying = false;
        var barsLastSecond = 0;
        var barSum = 0.0;
        var barCount = 0;
        var gapPower = 0.0;
        var gapHops = 0;
        var loudestGap = double.NaN;
        var edge = EnvelopeWindowSamples / HopSamples;
        var lastMarked = -1;

        for (var k = 1; k < bars.Count; k++)
        {
            var a = runs[bars[k - 1]];
            var b = runs[bars[k]];

            // The space between two elements is a dit at the least, the same shortest dit a bar
            // is held to; and the second bar holds the first one's level.
            var gapLength = b.Start - a.End - 1;

            if (gapLength < _minBarHops
                || b.Start - a.End > _keyingHops
                || Math.Abs(a.Mean - b.Mean) > FlatToleranceDb)
            {
                continue;
            }

            var lower = Math.Min(a.Mean, b.Mean);
            var ceiling = lower - FlatToleranceDb;
            var dropped = true;

            for (var i = bars[k - 1] + 1; i < bars[k]; i++)
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

            if (lastMarked != bars[k - 1])
            {
                marked.Add(new Span(a.Start, a.End, a.Mean));
            }

            marked.Add(new Span(b.Start, b.End, b.Mean));
            lastMarked = bars[k];

            if (b.End > recent)
            {
                keying = true;

                for (var i = bars[k - 1] + 1; i < bars[k]; i++)
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
            loudestGap);
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

    private static CwBarBin Summary(Bin bin, Evaluation e)
        => new(bin.Hz, e.Bars, e.Gaps, e.BarsLastSecond, e.Keying, e.BarDb, e.GapDb);

    private readonly record struct Span(long Start, long End, double MeanDb);

    // LoudestGapDb: the loudest gap hop the bars were held against, while keying; NaN otherwise.
    private readonly record struct Evaluation(
        List<Span> Marked, int Bars, int Gaps, int BarsLastSecond, bool Keying, double BarDb, double GapDb, double LoudestBarDb, double LoudestGapDb);

    /// <summary>A stretch of hops held at one level.</summary>
    private sealed class Run
    {
        public long Start;
        public int Count;
        public double Sum;
        public double Min = double.PositiveInfinity;
        public double Max = double.NegativeInfinity;
        public double PowerSum;

        public long End => Start + Count - 1;

        public double Mean => Sum / Count;

        /// <summary>Take a hop if the run still holds one level with it; say whether it did.</summary>
        /// <param name="db">The hop's level.</param>
        /// <param name="contrastDb">The bin's bars' measured contrast, or NaN before it has one.</param>
        /// <param name="underDb">Before it has one, the level the run's own contrast is read over.</param>
        public bool TryAdd(double db, double contrastDb, double underDb)
        {
            if (Count > 0)
            {
                var mean = (Sum + db) / (Count + 1);
                var toleranceDb = ToleranceDb(!double.IsNaN(contrastDb) ? contrastDb : mean - underDb);

                if (Math.Max(Max, db) - mean > toleranceDb || mean - Math.Min(Min, db) > toleranceDb)
                {
                    return false;
                }
            }

            Count++;
            Sum += db;
            Min = Math.Min(Min, db);
            Max = Math.Max(Max, db);
            PowerSum += Math.Pow(10, db / 10);

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
            _secondHops = Math.Max(1, secondHops);
            _lowHop = new long[_secondHops + 1];
            _lowDb = new double[_secondHops + 1];
        }

        public double Hz { get; }

        public double Coefficient { get; }

        /// <summary>Its bars' level over the loudest hop of its gaps, while it is keying; NaN otherwise.</summary>
        public double ContrastDb { get; set; } = double.NaN;

        /// <summary>The runs, oldest first; the last is still open.</summary>
        public List<Run> Runs { get; } = new();

        public Run? Open => Runs.Count > 0 ? Runs[^1] : null;

        public double Level(long hop) => _levels[(int)(hop % _levels.Length)];

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

        public void Add(long hop, double db, double contrastDb, double underDb)
        {
            _levels[(int)(hop % _levels.Length)] = db;

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
                var run = new Run { Start = hop };
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
                    var run = new Run { Start = hop };
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
