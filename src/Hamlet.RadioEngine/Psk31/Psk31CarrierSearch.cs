using System.Numerics;
using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Psk31;

/// <summary>One PSK31 carrier the search is sure of.</summary>
/// <param name="Id">Which carrier this is, the same for as long as it stays listed.</param>
/// <param name="OffsetHz">Where it sits in the passband now, in hertz.</param>
/// <param name="StrengthDb">
/// Its power over the noise in <see cref="Psk31CarrierSearch.ReferenceBandwidthHz"/>, in
/// decibels, or NaN where the spectrum could not measure it.
/// </param>
/// <param name="KeyedUntilSample">
/// The sample count at the last reading that found it keyed. **A reading, not a
/// promise**: the measure takes most of <see cref="Psk31CarrierSearch.MeasureSymbols"/>
/// symbols to let go after keying stops, so the keying really stopped up to that long
/// before this.
/// </param>
public sealed record Psk31Carrier(int Id, double OffsetHz, double StrengthDb, long KeyedUntilSample = 0);

/// <summary>
/// **Every PSK31 carrier in the passband, found by measurement.**
/// </summary>
/// <remarks>
/// <para>**IT KNOWS NOTHING ABOUT TABS, ROWS OR RADIOS** (§0.1). Samples go in; the
/// carriers present come out, each with an offset and a strength, updated as audio
/// arrives. **Nobody tells it where to look.** The range it searches is derived from the
/// sample rate it is handed, and no offset, band or dial frequency is written in here.</para>
/// <para>**TWO MEASUREMENTS, BECAUSE ONE OF THEM ALONE WOULD LIE.** A spectrum finds where
/// the energy is, cheaply and across the whole passband at once - but a noise peak, a
/// wider signal and a steady whistle all put energy in a spectrum. So each place the
/// spectrum points at gets a **probe**, which mixes down there and measures the thing
/// that makes PSK31 PSK31: **keying shape**, the same property unit 314's squelch
/// measures. Only a probe that passes is a carrier.</para>
/// <para>**THE KEYING-SHAPE MEASURE DOES NOT CARE WHERE EXACTLY THE PROBE IS.** Unit 314's
/// squelch takes the differential product `d` and asks how much of it is real, which is
/// only true once the channel is tuned to within a hertz or two. This takes `d` as a unit
/// phasor and **squares** it: both of BPSK's two states square to the same place, so what
/// is left is twice the frequency error, and its average magnitude is 1.0 on clean keying
/// at any error up to half the baud and about 0.1 on noise. Its angle is the error
/// itself, which is how the probe finds the carrier's offset to a fraction of a hertz
/// without being told it.</para>
/// <para>**AND A WHISTLE IS NOT A STATION.** An unmodulated carrier squares just as
/// cleanly - it is BPSK sending nothing but ones. PSK31 cannot do that: every character
/// ends in `00` and idle is all reversals. So a listed carrier must also be reversing at
/// least one symbol in ten, which is <see cref="MostlyOneWay"/>.</para>
/// <para>**COMPUTED, AND EVERY FIXTURE BEHIND IT IS SYNTHETIC** (FACT-004, FACT-006).</para>
/// </remarks>
public sealed class Psk31CarrierSearch
{
    /// <summary>**The search rule, in one place.**</summary>
    public const string SearchRule =
        "every half spectrum window, a place where the power summed over plus and minus "
        + nameof(SignalHalfWidthHz) + " stands at least " + nameof(CandidateRatio)
        + " times the floor's over the same width is a candidate, the floor being the median "
        + "bin or " + nameof(DynamicRangeDb) + " under the strongest place, whichever is "
        + "higher; a probe mixed down "
        + "at its power centroid takes the differential product of each symbol with the one "
        + "before as a unit phasor, squares it, and averages over " + nameof(MeasureSymbols)
        + " symbols; the probe is listed as a carrier when that average's magnitude is at "
        + "least " + nameof(CoherenceToAppear) + ", the keying is no more one-way than "
        + nameof(MostlyOneWay) + " and at least " + nameof(NarrowEnough) + " of the power "
        + "over the floor within three half-widths is inside one, stays listed while it is at least "
        + nameof(CoherenceToStay) + ", and is retired after " + nameof(RetireAfterSeconds)
        + " without a pass. Its offset is the probe's frequency plus the error the squared "
        + "phasor's angle measures.";

    /// <summary>**The bottom of the passband PSK31 is worked in, in hertz.**</summary>
    /// <remarks>
    /// <para>**IT COMES FROM THE MODE, NOT FROM THE SAMPLE RATE** (work instruction 324
    /// task 2). Until unit 324 the range was *whatever the samples can carry*, less a
    /// signal's width at each end, and on the operator's own evening that wrote
    /// `passbandLowHz: 64, passbandHighHz: 23936` into the record - a claim to be
    /// searching twenty-four kilohertz of a receiver that passes three.</para>
    /// <para>**200 TO 3000 IS THE SSB PASSBAND A PSK31 STATION IS WORKED THROUGH**, the
    /// same sliver FT8 lives in (`Ft8Resample`), and it is a property of the mode and the
    /// receiver rather than of the sound card. Nothing outside it can be a station Hamlet
    /// could work, and everything outside it is somewhere for a probe to waste itself.</para>
    /// </remarks>
    public const double PassbandLowHz = 200;

    /// <summary>**The top of it, in hertz.**</summary>
    public const double PassbandHighHz = 3000;

    /// <summary>The coarsest spectrum bin the search will accept, in hertz.</summary>
    /// <remarks>
    /// **ABOUT AN EIGHTH OF A PSK31 SIGNAL'S WIDTH**, so the power centroid lands within a
    /// hertz or two of the carrier and nowhere near the next one. The window is the
    /// smallest power of two that achieves it: 2048 samples at 8 kHz, 3.9 Hz a bin.
    /// </remarks>
    public const double SpectrumBinHz = 4;

    /// <summary>How long the spectrum remembers, in seconds.</summary>
    /// <remarks>
    /// **LONG ENOUGH THAT ONE NOISY WINDOW CANNOT MAKE A CANDIDATE, SHORT ENOUGH THAT A
    /// STATION STARTING IS SEEN WITHIN A FEW WINDOWS.** A candidate costs only a probe, and
    /// the probe is what decides, so this errs towards quick.
    /// </remarks>
    public const double SpectrumSeconds = 1.0;

    /// <summary>Half the width a PSK31 signal occupies, in hertz.</summary>
    /// <remarks>
    /// **ONE BAUD EITHER SIDE.** Raised-cosine BPSK at 31.25 baud puts its main lobe inside
    /// about 31 Hz of the carrier; summing that far catches the signal and stops well
    /// short of a neighbour 100 Hz away.
    /// </remarks>
    public const double SignalHalfWidthHz = 32;

    /// <summary>How far over the floor a place must stand to be worth a probe.</summary>
    /// <remarks>
    /// **TWICE, WHICH IS 3 DB, AND IT IS DELIBERATELY LOW.** The spectrum only nominates;
    /// the probe decides. Averaged for a second over seventeen bins, noise alone sits near
    /// one with a spread of about a tenth, so twice is far outside it, while a signal ten
    /// decibels under the noise in 2500 Hz still stands about five times over the floor in
    /// its own 65 Hz.
    /// </remarks>
    public const double CandidateRatio = 2.0;

    /// <summary>**How far under the strongest place a candidate may be, in decibels.**</summary>
    /// <remarks>
    /// <para>**70 DB, BECAUSE A LOUD SIGNAL BRINGS ITS OWN GHOSTS.** Every spectrum has
    /// window leakage and every audio chain has distortion products, a fixed distance
    /// under whatever is strongest. Where there is real noise it buries them and the
    /// median floor is the answer; where there is none they stand over the floor, and they
    /// are keyed - they are copies of the station's own keying. **Measured on the clean
    /// fixture, which is noiseless**: with the median floor alone the search listed its
    /// carrier and nineteen products of it, 95 to 110 dB under the carrier.</para>
    /// <para>**IT DOES NOT HIDE A WEAK STATION ON THE AIR.** Seventy decibels under a
    /// station's own 65 Hz is still under the noise floor until that station is more than
    /// 50 dB over the noise in 2500 Hz, which no HF signal a receiver is set up for gets
    /// near - so on real audio the median floor is always the higher of the two.</para>
    /// </remarks>
    public const double DynamicRangeDb = 70;

    /// <summary>How many symbols the keying-shape measure averages over.</summary>
    /// <remarks>
    /// **THE SAME THIRTY-TWO AS UNIT 314'S SQUELCH**, for its reason: longer than a
    /// character, shorter than a word, about a second.
    /// </remarks>
    public const int MeasureSymbols = 32;

    /// <summary>How many places inside a symbol the probe tries for the symbol centre.</summary>
    public const int TimingPhases = 8;

    /// <summary>**The keying-shape number a place needs to be listed as a carrier.**</summary>
    /// <remarks>
    /// <para>**0.6, AND HERE IS WHY THAT NUMBER.** Averaged over 32 symbols, the measure on
    /// noise has an expected magnitude near 0.11 and exceeds 0.6 with a probability of
    /// about e^-22 - never, in any evening. Clean keying gives 1.0, and a PSK31 signal
    /// ten decibels under the noise in 2500 Hz still gives well over 0.8.</para>
    /// <para>**IT IS CHOSEN FROM THE ARITHMETIC AND CONFIRMED BY THE FIXTURES**, not tuned
    /// until they passed - the noise-only fixture must list nothing and every signal
    /// fixture must list its carriers.</para>
    /// </remarks>
    public const double CoherenceToAppear = 0.6;

    /// <summary>**The keying-shape number a listed carrier needs to stay.**</summary>
    /// <remarks>
    /// **LOWER THAN TO APPEAR, SO A CARRIER DOES NOT FLICKER.** A station fading for a
    /// moment should not leave the list and come back as somebody new. Noise exceeds 0.4
    /// with a probability of about e^-10 on any one reading.
    /// </remarks>
    public const double CoherenceToStay = 0.4;

    /// <summary>**How one-way the keying may be and still be PSK31.**</summary>
    /// <remarks>
    /// **0.8 IS AT LEAST ONE REVERSAL IN TEN SYMBOLS.** The measure is the average of the
    /// differential product's real part once the frequency error is taken out: +1 is a
    /// steady whistle, -1 is idle, text sits near the middle. Every PSK31 character ends
    /// in two reversals, so real text never gets near +1.
    /// </remarks>
    public const double MostlyOneWay = 0.8;

    /// <summary>**How narrow a place must be to be listed as a PSK31 carrier.**</summary>
    /// <remarks>
    /// <para>**0.6 OF THE POWER OVER THE FLOOR WITHIN THREE HALF-WIDTHS MUST BE INSIDE
    /// ONE.** Keying shape tells PSK31 from noise; it does not tell it from everything
    /// wide. **Measured, not supposed**: fed the four-signal fixture raised to 48 kHz,
    /// the search without this listed a fifth carrier at 3999.9 Hz for 27 s and its
    /// channel read 78 characters of nothing - the old Nyquist edge, where folded noise
    /// has only two phases and so squares as cleanly as BPSK.</para>
    /// <para>**WHY THAT NUMBER.** Something flat across the window puts a third inside and
    /// is rejected. A carrier at these fixtures' levels puts over nine tenths inside even
    /// where the floor sits under the noise and the noise is counted with it, and one ten
    /// decibels under the noise in 2500 Hz still about seven tenths - so 0.6 sits between
    /// them. **It is asked only of a place not yet listed**; a listed carrier is not
    /// dropped because something wide starts beside it.</para>
    /// </remarks>
    public const double NarrowEnough = 0.6;

    /// <summary>How long a probe may try before it is given up, in seconds.</summary>
    public const double TrialSeconds = 2.0;

    /// <summary>**How long a listed carrier may fail before it is retired, in seconds.**</summary>
    /// <remarks>
    /// <para>**ONE SECOND WITHOUT A PASS, AFTER A MEASURE THAT ITSELF TAKES ABOUT A SECOND
    /// TO LET GO.** When keying stops, the 32-symbol average loses the signal as it fills
    /// with noise, and it crosses <see cref="CoherenceToStay"/> about 29 symbols - 0.93 s -
    /// later. The second on top is the anti-flicker hold: a station fading for a moment
    /// is not retired and reborn under a new id.</para>
    /// <para>**A STATION SITTING ON IDLE IS NOT RETIRED.** Idle is continuous reversals,
    /// which is keying, and it passes.</para>
    /// </remarks>
    public const double RetireAfterSeconds = 1.0;

    /// <summary>How much audio a new probe is given from before it was made, in seconds.</summary>
    /// <remarks>
    /// **ENOUGH FOR ONE WHOLE MEASURE AND A LITTLE OVER**, so a probe does not have to wait
    /// a second of live audio before it can say anything.
    /// </remarks>
    public const double HistorySeconds = 1.5;

    /// <summary>How far a probe may be off before it moves, in hertz.</summary>
    public const double RetuneHz = 2.0;

    /// <summary>The bandwidth strength is quoted against, in hertz.</summary>
    /// <remarks>
    /// **2500, THE WAY AN FT8 REPORT AND THESE FIXTURES' OWN SNR ARE QUOTED**, so the
    /// number sits in the same column on the same terms as the rows beside it. It is a
    /// reference for a ratio and not the width of anything being searched.
    /// </remarks>
    public const double ReferenceBandwidthHz = 2500;

    private readonly int _sampleRate;
    private readonly RealFft _fft;
    private readonly int _hop;
    private readonly double _binHz;
    private readonly float[] _hann;
    private readonly float[] _window;
    private readonly double[] _magnitudes;
    private readonly double[] _real;
    private readonly double[] _imaginary;
    private readonly double[] _power;
    private readonly double[] _sums;
    private readonly double[] _sorted;
    private readonly int _halfBins;
    private readonly int _lowBin;
    private readonly int _highBin;
    private bool _spectrumStarted;
    private double _floor;
    private double _nominationFloor;

    private readonly float[] _history;
    private int _historyWrite;
    private int _historyFilled;

    private readonly List<Probe> _probes = new();
    private IReadOnlyList<Psk31Carrier> _carriers = Array.Empty<Psk31Carrier>();
    private int _nextId = 1;
    private int _toHop;

    /// <summary>What happened on each pass, for something outside to write down.</summary>
    /// <remarks>
    /// **THE SEARCH KNOWS NOTHING ABOUT A RECORD** (§0.1, work instruction 322 task 2).
    /// It fills a list of facts and the shell drains it; nothing here mentions
    /// telemetry, a file or a category.
    /// </remarks>
    public Psk31Watch Watch { get; } = new();

    /// <summary>Passes measured since this candidate was made, by probe.</summary>
    private readonly Dictionary<Probe, int> _passesSeen = new();

    /// <summary>Opens a search over whatever passband the samples carry.</summary>
    /// <param name="sampleRate">Samples a second.</param>
    /// <exception cref="ArgumentOutOfRangeException">The rate is unusable.</exception>
    public Psk31CarrierSearch(int sampleRate)
    {
        if (sampleRate < 1000)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleRate));
        }

        _sampleRate = sampleRate;

        var size = 4;

        while ((double)sampleRate / size > SpectrumBinHz)
        {
            size <<= 1;
        }

        _fft = new RealFft(size);
        _hop = size / 2;
        _binHz = (double)sampleRate / size;

        _hann = new float[size];

        for (var i = 0; i < size; i++)
        {
            _hann[i] = (float)(0.5 - (0.5 * Math.Cos(2 * Math.PI * i / size)));
        }

        _window = new float[size];
        _magnitudes = new double[_fft.BinCount];
        _real = new double[size];
        _imaginary = new double[size];
        _power = new double[_fft.BinCount];
        _sums = new double[_fft.BinCount];
        _sorted = new double[_fft.BinCount];

        _halfBins = Math.Max(1, (int)Math.Round(SignalHalfWidthHz / _binHz));

        // **THE RANGE IS THE MODE'S PASSBAND**, and the rate only ever narrows it: a rate
        // too low to carry 3 000 Hz stops at what it can carry, less one signal's width,
        // so a probe is never asked to mix down where a whole PSK31 signal could not fit.
        _lowBin = (int)Math.Ceiling(PassbandLowHz / _binHz);
        _highBin = Math.Min(
            (int)Math.Floor(PassbandHighHz / _binHz),
            (int)Math.Floor(((sampleRate / 2.0) - (2 * SignalHalfWidthHz)) / _binHz));

        _history = new float[Math.Max(size, (int)(HistorySeconds * sampleRate))];
        _toHop = _hop;
    }

    /// <summary>The carriers listed now, lowest offset first.</summary>
    public IReadOnlyList<Psk31Carrier> Carriers => _carriers;

    /// <summary>How many samples the search has been fed.</summary>
    public long SamplesSeen { get; private set; }

    /// <summary>The bottom of the passband this search looks at, in hertz.</summary>
    /// <remarks>
    /// **READ BACK FROM THE BINS IT REALLY USES, NOT TYPED** (work instruction 322 task
    /// 2). A record that named a passband the search was not actually looking at would be
    /// worse than naming none. The bins come from <see cref="PassbandLowHz"/> and
    /// <see cref="PassbandHighHz"/>, so this lands within one bin of them.
    /// </remarks>
    public double LowestHz => _lowBin * _binHz;

    /// <summary>The top of it, in hertz.</summary>
    public double HighestHz => _highBin * _binHz;

    /// <summary>How many probes are running, listed or on trial.</summary>
    public int ProbeCount => _probes.Count;

    /// <summary>Feed the search some audio.</summary>
    /// <param name="samples">Samples in [-1, 1].</param>
    /// <remarks>
    /// **IT IS A STREAM AND THE CALLER MAY CUT IT ANYWHERE.** The spectrum is taken on its
    /// own cadence, whatever lumps the audio arrives in.
    /// </remarks>
    public void Add(ReadOnlySpan<float> samples)
    {
        while (samples.Length > 0)
        {
            var count = Math.Min(samples.Length, _toHop);
            var slice = samples[..count];

            Remember(slice);

            foreach (var probe in _probes)
            {
                probe.Add(slice);
            }

            SamplesSeen += count;
            _toHop -= count;
            samples = samples[count..];

            if (_toHop == 0)
            {
                Update();
                _toHop = _hop;
            }
        }
    }

    /// <summary>Keep the recent audio, for the spectrum and for new probes.</summary>
    private void Remember(ReadOnlySpan<float> samples)
    {
        foreach (var sample in samples)
        {
            _history[_historyWrite] = sample;
            _historyWrite = (_historyWrite + 1) % _history.Length;

            if (_historyFilled < _history.Length)
            {
                _historyFilled++;
            }
        }
    }

    /// <summary>The newest `count` samples of history, oldest first, into a buffer.</summary>
    private void Recent(Span<float> into)
    {
        var count = into.Length;
        var start = (_historyWrite - count + _history.Length) % _history.Length;

        for (var i = 0; i < count; i++)
        {
            into[i] = _history[(start + i) % _history.Length];
        }
    }

    /// <summary>One look: the spectrum, every probe's verdict, and new probes.</summary>
    private void Update()
    {
        if (_historyFilled < _window.Length)
        {
            return;
        }

        var began = System.Diagnostics.Stopwatch.GetTimestamp();
        var before = _carriers.Count;

        Spectrum();

        Judge();

        Nominate();

        Merge();

        _carriers = _probes
            .Where(p => p.Listed)
            .Select(p => new Psk31Carrier(p.Id, p.OffsetHz, StrengthAt(p.OffsetHz), p.LastPassAt))
            .OrderBy(c => c.OffsetHz)
            .ToList();

        Note(began, before);
    }

    /// <summary>Write down what this pass measured.</summary>
    /// <param name="began">The timestamp the pass started at.</param>
    /// <param name="before">How many carriers were held when it started.</param>
    /// <remarks>
    /// <para>**EVERY PROBE, CROSSED OR NOT, AND THAT IS THE POINT** (§0.0). A pass with no
    /// candidates says the band was quiet; a pass with candidates that all failed to
    /// cross says there was something there and Hamlet would not take it. **On a screen
    /// those two are the same empty list**, and the operator asked which of them he was
    /// looking at.</para>
    /// <para>**IT MEASURES NOTHING OF ITS OWN.** Every number here was already worked out
    /// by the pass; this reads them back rather than running the arithmetic a second
    /// time, so the record cannot come to disagree with the decision it describes.</para>
    /// </remarks>
    private void Note(long began, int before)
    {
        var candidates = new List<Psk31Candidate>(_probes.Count);

        foreach (var probe in _probes)
        {
            var reading = probe.LastReading;

            candidates.Add(new Psk31Candidate(
                Math.Round(probe.OffsetHz, 1),
                Math.Round(StrengthAt(probe.OffsetHz), 1),
                reading.Measured ? Math.Round(reading.Coherence, 3) : 0,
                probe.Listed));
        }

        candidates.Sort((a, b) => b.StrengthDb.CompareTo(a.StrengthDb));

        var milliseconds =
            (System.Diagnostics.Stopwatch.GetTimestamp() - began) * 1000.0
            / System.Diagnostics.Stopwatch.Frequency;

        Watch.Add(new Psk31Pass(
            Math.Round(milliseconds, 2),
            candidates,
            _carriers.Count,
            _carriers.Count != before));
    }

    /// <summary>Take one windowed spectrum into the running average, and its floor.</summary>
    private void Spectrum()
    {
        Recent(_window);

        for (var i = 0; i < _window.Length; i++)
        {
            _window[i] *= _hann[i];
        }

        _fft.Magnitudes(_window, _magnitudes, _real, _imaginary);

        var alpha = Math.Min(1.0, _hop / (_sampleRate * SpectrumSeconds));

        for (var bin = 0; bin < _power.Length; bin++)
        {
            var power = _magnitudes[bin] * _magnitudes[bin];

            _power[bin] = _spectrumStarted
                ? _power[bin] + ((power - _power[bin]) * alpha)
                : power;
        }

        _spectrumStarted = true;

        // **THE FLOOR IS THE MEDIAN BIN**, which a handful of stations each a few bins
        // wide cannot move, where a mean would be dragged up by every one of them.
        var count = _highBin - _lowBin + 1;

        Array.Copy(_power, _lowBin, _sorted, 0, count);
        Array.Sort(_sorted, 0, count);

        _floor = _sorted[count / 2];

        for (var bin = 0; bin < _sums.Length; bin++)
        {
            var sum = 0.0;

            for (var i = bin - _halfBins; i <= bin + _halfBins; i++)
            {
                if (i >= 0 && i < _power.Length)
                {
                    sum += _power[i];
                }
            }

            _sums[bin] = sum;
        }

        var strongest = 0.0;

        for (var bin = _lowBin; bin <= _highBin; bin++)
        {
            strongest = Math.Max(strongest, _sums[bin]);
        }

        _nominationFloor = Math.Max(
            _floor, strongest / ((2 * _halfBins) + 1) * Math.Pow(10, -DynamicRangeDb / 10));
    }

    /// <summary>Every probe's verdict: listed, kept, moved, given up or retired.</summary>
    private void Judge()
    {
        var trial = (long)(TrialSeconds * _sampleRate);
        var retire = (long)(RetireAfterSeconds * _sampleRate);

        for (var i = _probes.Count - 1; i >= 0; i--)
        {
            var probe = _probes[i];
            var reading = probe.Measure();

            probe.LastReading = reading;

            var shaped = reading.Measured && reading.OneWay <= MostlyOneWay;

            if (!probe.Listed)
            {
                _passesSeen[probe] = _passesSeen.GetValueOrDefault(probe) + 1;

                if (shaped
                    && reading.Symbols >= MeasureSymbols
                    && reading.Coherence >= CoherenceToAppear
                    && WidthFraction(probe.Hz + reading.ErrorHz) >= NarrowEnough)
                {
                    probe.Listed = true;
                    probe.Id = _nextId++;
                    probe.LastPassAt = SamplesSeen;
                    probe.ListedAt = SamplesSeen;

                    Watch.Add(new Psk31CarrierChange(
                        probe.Id,
                        Appeared: true,
                        Math.Round(probe.Hz + reading.ErrorHz, 1),
                        Math.Round(StrengthAt(probe.Hz + reading.ErrorHz), 1),
                        Math.Round(reading.Coherence, 3),
                        _passesSeen.GetValueOrDefault(probe),
                        LifetimeSeconds: 0,
                        Why: null));
                }
                else if (SamplesSeen - probe.MadeAt >= trial)
                {
                    // **A CANDIDATE THAT NEVER CROSSED IS NOT WRITTEN DOWN ONE BY
                    // ONE.** On a noisy band the search makes and drops a great many of
                    // them, and an event each would bury the ones that matter. They are
                    // in every pass's candidate list with `crossed: false`, which is
                    // where a reader looks for them.
                    _passesSeen.Remove(probe);
                    _probes.RemoveAt(i);
                    continue;
                }
            }
            else if (shaped && reading.Coherence >= CoherenceToStay)
            {
                probe.LastPassAt = SamplesSeen;
                probe.LastGoodReading = reading;
            }
            else if (SamplesSeen - probe.LastPassAt >= retire)
            {
                // **WHY IT WENT, NOT JUST THAT IT WENT.** One test with three ways to
                // fail: nothing measurable there any more, still measurable but no
                // longer keyed like BPSK, or keyed one way only. A record that said
                // only *retired* would collapse a station that finished, a station that
                // faded, and a decoder that lost the thread.
                var why = !reading.Measured
                    ? Psk31Retirement.Silence
                    : reading.OneWay > MostlyOneWay
                        ? Psk31Retirement.OneWay
                        : Psk31Retirement.LostLock;

                Watch.Add(new Psk31CarrierChange(
                    probe.Id,
                    Appeared: false,
                    Math.Round(probe.OffsetHz, 1),
                    Math.Round(StrengthAt(probe.OffsetHz), 1),
                    reading.Measured ? Math.Round(reading.Coherence, 3) : 0,
                    _passesSeen.GetValueOrDefault(probe),
                    Math.Round(
                        (double)(SamplesSeen - probe.ListedAt) / _sampleRate, 1),
                    why));

                _passesSeen.Remove(probe);
                _probes.RemoveAt(i);
                continue;
            }

            probe.OffsetHz = reading.Measured ? probe.Hz + reading.ErrorHz : probe.Hz;

            // **IT ONLY MOVES ON WHAT IT CAN HEAR**, for the reason unit 314's AFC does:
            // on noise the angle is uniform, and a probe that followed it would wander.
            if (shaped
                && reading.Coherence >= CoherenceToStay
                && Math.Abs(reading.ErrorHz) > RetuneHz)
            {
                probe.Retune(reading.ErrorHz);
            }
        }
    }

    /// <summary>Put a probe on every place the spectrum points at and nobody is.</summary>
    private void Nominate()
    {
        if (_floor <= 0)
        {
            return;
        }

        var lowHz = _lowBin * _binHz;
        var highHz = _highBin * _binHz;

        var wanted = CandidateRatio * _nominationFloor * ((2 * _halfBins) + 1);

        for (var bin = _lowBin; bin <= _highBin; bin++)
        {
            if (_sums[bin] < wanted || !IsPeak(bin))
            {
                continue;
            }

            var hz = Centroid(bin);

            if (hz <= lowHz || hz >= highHz)
            {
                continue;
            }

            if (_probes.Any(p => Math.Abs(p.Hz - hz) < Psk31Demodulator.Baud / 2))
            {
                continue;
            }

            var probe = new Probe(_sampleRate, hz, SamplesSeen);

            // **A NEW PROBE HEARS THE LAST SECOND AND A HALF FIRST**, so its first verdict
            // is not a second of live audio away.
            var replay = new float[Math.Min(_historyFilled, (int)(HistorySeconds * _sampleRate))];

            Recent(replay);
            probe.Add(replay);

            _probes.Add(probe);
        }
    }

    /// <summary>Whether a bin's summed power is the most within one signal's width.</summary>
    private bool IsPeak(int bin)
    {
        for (var i = bin - _halfBins; i <= bin + _halfBins; i++)
        {
            if (i < 0 || i >= _sums.Length || i == bin)
            {
                continue;
            }

            if (i < bin ? _sums[i] >= _sums[bin] : _sums[i] > _sums[bin])
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Where the power above the floor balances, around a bin.</summary>
    /// <remarks>
    /// **A BPSK SPECTRUM IS SYMMETRIC ABOUT ITS CARRIER WHATEVER IS BEING SENT.** Idle is
    /// two lines 15.6 Hz either side with nothing in the middle and text is a hump, and
    /// the balance point of both is the carrier, where the tallest bin is not.
    /// </remarks>
    private double Centroid(int bin)
    {
        var weighted = 0.0;
        var total = 0.0;

        for (var i = bin - _halfBins; i <= bin + _halfBins; i++)
        {
            if (i < 0 || i >= _power.Length)
            {
                continue;
            }

            var above = Math.Max(0, _power[i] - _floor);

            weighted += above * i * _binHz;
            total += above;
        }

        return total > 0 ? weighted / total : bin * _binHz;
    }

    /// <summary>A carrier's power over the noise in the reference bandwidth, in decibels.</summary>
    /// <remarks>
    /// **THE SIGNAL'S BINS ABOVE THE FLOOR, OVER THE FLOOR'S BINS IN 2500 HZ.** The window
    /// scales both alike, so it drops out of the ratio. NaN where the floor or the excess
    /// is nothing, because a strength nobody measured is not a strength of zero.
    /// </remarks>
    private double StrengthAt(double hz)
    {
        var centre = (int)Math.Round(hz / _binHz);
        var excess = 0.0;

        for (var i = centre - _halfBins; i <= centre + _halfBins; i++)
        {
            if (i >= 0 && i < _power.Length)
            {
                excess += Math.Max(0, _power[i] - _floor);
            }
        }

        var noise = _floor * ReferenceBandwidthHz / _binHz;

        return excess > 0 && noise > 0 ? 10 * Math.Log10(excess / noise) : double.NaN;
    }

    /// <summary>How much of what stands over the floor near a place is inside one signal's width.</summary>
    /// <remarks>
    /// **THE POWER OVER THE NOMINATION FLOOR WITHIN ONE HALF-WIDTH, OVER THE SAME WITHIN
    /// THREE.** A PSK31 carrier puts nearly all of it in the inner third; something as wide
    /// as the whole window spreads it evenly, a third in each.
    /// </remarks>
    private double WidthFraction(double hz)
    {
        var centre = (int)Math.Round(hz / _binHz);

        double Excess(int half)
        {
            var sum = 0.0;

            for (var i = centre - half; i <= centre + half; i++)
            {
                if (i >= 0 && i < _power.Length)
                {
                    sum += Math.Max(0, _power[i] - _nominationFloor);
                }
            }

            return sum;
        }

        var wide = Excess(3 * _halfBins);

        return wide > 0 ? Excess(_halfBins) / wide : 0;
    }

    /// <summary>Two probes on one carrier are one probe.</summary>
    /// <remarks>
    /// **THE ONE ALREADY LISTED WINS, THEN THE OLDER.** A carrier drifting into the place
    /// a newer probe was nominated for must not come back as a second station.
    /// </remarks>
    private void Merge()
    {
        _probes.Sort((a, b) => a.Hz.CompareTo(b.Hz));

        for (var i = _probes.Count - 1; i > 0; i--)
        {
            var upper = _probes[i];
            var lower = _probes[i - 1];

            if (Math.Abs(upper.OffsetHz - lower.OffsetHz) >= Psk31Demodulator.Baud / 2)
            {
                continue;
            }

            var keepLower = lower.Listed != upper.Listed
                ? lower.Listed
                : lower.Listed
                    ? lower.Id < upper.Id
                    : lower.MadeAt <= upper.MadeAt;

            _probes.RemoveAt(keepLower ? i : i - 1);
        }
    }

    /// <summary>What a probe measured.</summary>
    private readonly record struct Reading(
        bool Measured, double Coherence, double ErrorHz, double OneWay, int Symbols);

    /// <summary>One place in the passband, mixed down and asked whether it is keyed.</summary>
    private sealed class Probe
    {
        private readonly int _sampleRate;
        private readonly int _samplesPerSymbol;
        private readonly int _samplesPerPhase;
        private readonly Complex[] _period;
        private readonly Complex[] _previous = new Complex[TimingPhases];
        private readonly bool[] _hasPrevious = new bool[TimingPhases];
        private readonly Complex[] _squares = new Complex[TimingPhases];
        private readonly Complex[] _products = new Complex[TimingPhases];
        private readonly double[] _weight = new double[TimingPhases];
        private readonly double[] _energy = new double[TimingPhases];
        private readonly int[] _symbols = new int[TimingPhases];
        private readonly double _keep = 1.0 - (1.0 / MeasureSymbols);
        private readonly double _take = 1.0 / MeasureSymbols;

        private double _phase;
        private double _runningReal;
        private double _runningImaginary;
        private int _inFilter;
        private int _index;
        private int _position;

        public Probe(int sampleRate, double hz, long madeAt)
        {
            _sampleRate = sampleRate;
            _samplesPerSymbol = (int)Math.Round(sampleRate / Psk31Demodulator.Baud);
            _samplesPerPhase = Math.Max(1, _samplesPerSymbol / TimingPhases);
            _period = new Complex[_samplesPerSymbol];

            Hz = hz;
            OffsetHz = hz;
            MadeAt = madeAt;
        }

        /// <summary>Where the probe mixes down.</summary>
        public double Hz { get; private set; }

        /// <summary>Where the carrier was last measured to be.</summary>
        public double OffsetHz { get; set; }

        public long MadeAt { get; }

        public bool Listed { get; set; }

        public int Id { get; set; }

        public long LastPassAt { get; set; }

        /// <summary>When this probe became a carrier, in samples.</summary>
        public long ListedAt { get; set; }

        /// <summary>What the last measurement said, for the record to read back.</summary>
        /// <remarks>
        /// **THE PASS ALREADY MEASURED THIS AND MEASURING AGAIN WOULD COST A SECOND
        /// ANSWER** (§0). `Measure` walks thirty-two symbols; the record wants the same
        /// numbers the decision used, not a fresh reading that could differ.
        /// </remarks>
        public Reading LastReading { get; set; }

        /// <summary>The last reading good enough to keep it listed.</summary>
        public Reading LastGoodReading { get; set; }

        public void Add(ReadOnlySpan<float> samples)
        {
            var step = 2 * Math.PI * Hz / _sampleRate;

            foreach (var sample in samples)
            {
                _phase -= step;

                if (_phase < -2 * Math.PI)
                {
                    _phase += 2 * Math.PI;
                }

                var real = sample * Math.Cos(_phase);
                var imaginary = sample * Math.Sin(_phase);

                // **ONE SYMBOL'S BOXCAR, THE MATCHED FILTER UNIT 314'S DEMODULATOR USES.**
                var leaving = _period[_index];

                _runningReal += real;
                _runningImaginary += imaginary;

                if (_inFilter < _samplesPerSymbol)
                {
                    _inFilter++;
                }
                else
                {
                    _runningReal -= leaving.Real;
                    _runningImaginary -= leaving.Imaginary;
                }

                _period[_index] = new Complex(real, imaginary);
                _index = (_index + 1) % _samplesPerSymbol;

                if (_inFilter == _samplesPerSymbol && _position % _samplesPerPhase == 0)
                {
                    var phase = _position / _samplesPerPhase;

                    if (phase < TimingPhases)
                    {
                        Take(phase, new Complex(_runningReal, _runningImaginary));
                    }
                }

                if (++_position >= _samplesPerSymbol)
                {
                    _position = 0;
                }
            }
        }

        /// <summary>One symbol-spaced sample at one timing phase.</summary>
        private void Take(int phase, Complex filtered)
        {
            if (_hasPrevious[phase])
            {
                var d = filtered * Complex.Conjugate(_previous[phase]);
                var magnitude = d.Magnitude;

                if (magnitude > 0)
                {
                    var unit = d / magnitude;

                    _squares[phase] = (_squares[phase] * _keep) + (unit * unit * _take);
                    _products[phase] = (_products[phase] * _keep) + (unit * _take);
                    _weight[phase] = (_weight[phase] * _keep) + _take;
                    _symbols[phase]++;
                }
            }

            _energy[phase] = (_energy[phase] * _keep)
                + (((filtered.Real * filtered.Real) + (filtered.Imaginary * filtered.Imaginary)) * _take);

            _previous[phase] = filtered;
            _hasPrevious[phase] = true;
        }

        /// <summary>The keying shape at the timing phase with the most energy.</summary>
        public Reading Measure()
        {
            var best = 0;

            for (var phase = 1; phase < TimingPhases; phase++)
            {
                if (_energy[phase] > _energy[best])
                {
                    best = phase;
                }
            }

            if (_weight[best] <= 0)
            {
                return default;
            }

            var mean = _squares[best] / _weight[best];

            // **HALF THE SQUARED ANGLE IS THE ERROR PER SYMBOL**, unambiguous within half
            // the baud either side, which the centroid is always far inside.
            var omega = Math.Atan2(mean.Imaginary, mean.Real) / 2;
            var errorHz = omega * Psk31Demodulator.Baud / (2 * Math.PI);

            var turned = _products[best] / _weight[best] * Complex.FromPolarCoordinates(1, -omega);

            return new Reading(true, mean.Magnitude, errorHz, turned.Real, _symbols[best]);
        }

        /// <summary>Move the probe, and turn what it has measured to match.</summary>
        /// <remarks>
        /// **THE AVERAGES ARE ROTATED RATHER THAN THROWN AWAY.** Moving by δ changes each
        /// product's rotation by 2πδ/baud, so the products turn by that and the squares by
        /// twice it, and the measure carries on as though the probe had been there all
        /// along.
        /// </remarks>
        public void Retune(double byHz)
        {
            Hz += byHz;

            var once = Complex.FromPolarCoordinates(1, -2 * Math.PI * byHz / Psk31Demodulator.Baud);
            var twice = once * once;

            for (var phase = 0; phase < TimingPhases; phase++)
            {
                _squares[phase] *= twice;
                _products[phase] *= once;
            }
        }
    }
}
