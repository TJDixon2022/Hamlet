using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Olivia;

/// <summary>A carrier the streaming search named, and the reader that confirmed it.</summary>
/// <param name="Candidate">What was measured and weighed, as <see cref="OliviaBlindSearch"/> reports it.</param>
/// <param name="Stream">The confirming row's reader, fed from <paramref name="StartSample"/> to now.</param>
/// <param name="StartSample">The sample, counted from the first one the search was fed, the reader began at.</param>
public sealed record OliviaFound(OliviaCandidate Candidate, OliviaStream Stream, long StartSample);

/// <summary>A place already read, which the search does not open a second time.</summary>
/// <param name="CenterHz">Its center.</param>
/// <param name="BandwidthHz">Its variant's bandwidth.</param>
public readonly record struct OliviaOccupied(double CenterHz, double BandwidthHz);

/// <summary>
/// **The blind search, fed as the audio arrives**: an Olivia carrier that never announced itself
/// is found and named without re-reading the recording.
/// </summary>
/// <remarks>
/// <para>**THE SAME MEASURE, KEPT AND ADVANCED** (work instruction 363 decision Z, unit 362 item 4).
/// <see cref="OliviaBlindSearch.Search"/> takes a block of the slowest row at a time and looks
/// again at everything from the start, decoding it all again for every row it weighs. Here the
/// averaged spectrum is a running set of the last <see cref="AverageBlocks"/> blocks' power
/// spectra, a new one added as each half-window arrives; the regions, their occupied band and the
/// rows that fit are <see cref="OliviaBlindSearch"/>'s own; the tones are measured over the last two
/// blocks, as the whole search first measures them; and **each row weighed is an
/// <see cref="OliviaStream"/> that keeps reading** from the start of the kept audio, rather than a
/// decode run again at every look.</para>
/// <para>**NEVER TOLD THE ANSWER** (decision O): it is handed audio, a passband and the places the
/// listener already reads, and nothing else.</para>
/// <para>**THE SAME TWO GATES** (decision R): a region must clear
/// <see cref="OliviaBlindSearch.GateSigmas"/> over the median and be half the narrowest row wide,
/// and a row must put <see cref="OliviaBlindSearch.ConfirmBlocks"/> blocks over the demodulator's
/// threshold; of the rows that do, the one whose blocks stand furthest out is named.</para>
/// <para>**IT HEARS AND DOES NOTHING ELSE** (§0.2).</para>
/// </remarks>
public sealed class OliviaSearchStream
{
    /// <summary>How many of the slowest row's blocks the averaged spectrum spans.</summary>
    /// <remarks>
    /// **THE UNIT'S NUMBER** (work instruction 363 task 3): the whole search named the two-signal
    /// file's carriers after four such blocks, 8.192 s, and the gate tightens as the root of the
    /// spectra averaged, so a longer span buys nothing against noise that the gate does not already.
    /// </remarks>
    public const int AverageBlocks = 4;

    /// <summary>How many of the slowest row's blocks of audio are kept, and handed to a new reader.</summary>
    /// <remarks>
    /// **THE UNIT'S NUMBER, DERIVED FROM THE BLOCK AND NOT FROM SECONDS** (decision Y). The first look
    /// comes after two blocks, and a carrier that started just before the audio began must still
    /// have its first block whole in what a trial reader is handed: three.
    /// </remarks>
    public const int ReplayBlocks = 3;

    /// <summary>How many of the slowest row's blocks a set of trial readers may run without a row confirming.</summary>
    /// <remarks>**THE UNIT'S NUMBER**: two to confirm, and two more for a region found as its carrier was still starting.</remarks>
    public const int TrialBlocks = 4;

    private readonly OliviaFormat _format;
    private readonly OliviaBlindSearch _search;
    private readonly int _rate;
    private readonly double _lowestHz;
    private readonly double _highestHz;
    private readonly int _step;
    private readonly int _keep;
    private readonly int _size;
    private readonly int _maxSpectra;
    private readonly double _narrowestBand;
    private readonly RealFft _fft;
    private readonly double[] _magnitudes;
    private readonly double[] _real;
    private readonly double[] _imaginary;
    private readonly float[] _shaped;
    private readonly Queue<double[]> _spectra = new();
    private readonly List<TrialSet> _sets = new();

    private float[] _history;
    private long _historyStart;
    private int _historyCount;
    private long _samples;
    private long _nextSpectrum;
    private long _nextLook;

    /// <summary>A search over one stream of audio.</summary>
    /// <param name="format">The format's facts, whose rows are the only variants it can name.</param>
    /// <param name="sampleRate">The stream's samples a second.</param>
    /// <param name="lowestHz">The passband's low edge.</param>
    /// <param name="highestHz">The passband's high edge.</param>
    public OliviaSearchStream(OliviaFormat format, int sampleRate, double lowestHz, double highestHz)
    {
        ArgumentNullException.ThrowIfNull(format);

        _format = format;
        _search = new OliviaBlindSearch(format);
        _rate = sampleRate;
        _lowestHz = lowestHz;
        _highestHz = highestHz;
        _step = Math.Max(1, (int)Math.Round(format.Variants.Max(v => v.SymbolSeconds) * format.SymbolsPerBlock * sampleRate));
        _narrowestBand = format.Variants.Min(v => v.BandwidthHz);

        // The same long view as the whole search: bins an eighth of the narrowest spacing.
        _size = (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)Math.Ceiling(sampleRate / (format.Variants.Min(v => v.ToneSpacingHz) / 8)));
        _maxSpectra = Math.Max(1, (((AverageBlocks * _step) - _size) / (_size / 2)) + 1);
        _keep = Math.Max(ReplayBlocks * _step, Math.Max(2 * _step, _size));
        _history = new float[_keep * 2];
        _fft = new RealFft(_size);
        _magnitudes = new double[_fft.BinCount];
        _real = new double[_size];
        _imaginary = new double[_size];
        _shaped = new float[_size];
        _nextLook = 2L * _step;
    }

    /// <summary>How much audio a reader the search opens is handed from before it was opened, in seconds.</summary>
    public double ReplaySeconds => ReplayBlocks * _step / (double)_rate;

    /// <summary>Samples fed.</summary>
    public long SamplesSeen => _samples;

    /// <summary>How many sets of trial readers are running now.</summary>
    public int TrialSetsRunning => _sets.Count;

    /// <summary>Hand the search the next piece of audio.</summary>
    /// <param name="samples">The samples.</param>
    /// <param name="occupied">The places already read, which are not searched.</param>
    /// <returns>The carriers named on this piece, usually none.</returns>
    public IReadOnlyList<OliviaFound> Add(ReadOnlySpan<float> samples, IReadOnlyList<OliviaOccupied> occupied)
    {
        Remember(samples);
        _samples += samples.Length;

        foreach (var set in _sets)
        {
            foreach (var trial in set.Trials)
            {
                trial.Stream.Add(samples);
            }
        }

        while (_nextSpectrum + _size <= _samples)
        {
            Spectrum();
        }

        var found = new List<OliviaFound>();

        while (_samples >= _nextLook)
        {
            Weigh(occupied, found, false);
            Look(occupied);
            _nextLook += _step;
        }

        return found;
    }

    /// <summary>The audio has ended: the trial readers read what is left, and are weighed once more.</summary>
    /// <param name="occupied">The places already read.</param>
    /// <returns>The carriers named.</returns>
    public IReadOnlyList<OliviaFound> Flush(IReadOnlyList<OliviaOccupied> occupied)
    {
        foreach (var set in _sets)
        {
            foreach (var trial in set.Trials)
            {
                trial.Stream.Flush();
            }
        }

        var found = new List<OliviaFound>();

        Weigh(occupied, found, true);

        return found;
    }

    private void Remember(ReadOnlySpan<float> samples)
    {
        if (_historyCount + samples.Length > _history.Length)
        {
            var drop = Math.Min(_historyCount, Math.Max(0, _historyCount + samples.Length - _keep));

            Array.Copy(_history, drop, _history, 0, _historyCount - drop);
            _historyCount -= drop;
            _historyStart += drop;

            if (_historyCount + samples.Length > _history.Length)
            {
                Array.Resize(ref _history, _historyCount + samples.Length);
            }
        }

        samples.CopyTo(_history.AsSpan(_historyCount));
        _historyCount += samples.Length;
    }

    private void Spectrum()
    {
        var at = (int)(_nextSpectrum - _historyStart);

        for (var i = 0; i < _size; i++)
        {
            _shaped[i] = (float)(_history[at + i] * 0.5 * (1 - Math.Cos(2 * Math.PI * i / _size)));
        }

        _fft.Magnitudes(_shaped, _magnitudes, _real, _imaginary);

        var power = new double[_magnitudes.Length];

        for (var b = 0; b < power.Length; b++)
        {
            power[b] = _magnitudes[b] * _magnitudes[b];
        }

        _spectra.Enqueue(power);

        while (_spectra.Count > _maxSpectra)
        {
            _spectra.Dequeue();
        }

        _nextSpectrum += _size / 2;
    }

    private void Look(IReadOnlyList<OliviaOccupied> occupied)
    {
        var count = _spectra.Count;

        if (count == 0)
        {
            return;
        }

        var average = new double[_fft.BinCount];

        foreach (var spectrum in _spectra)
        {
            for (var b = 0; b < average.Length; b++)
            {
                average[b] += spectrum[b] / count;
            }
        }

        var binHz = (double)_rate / _size;
        var low = Math.Max(1, (int)Math.Ceiling(_lowestHz / binHz));
        var high = Math.Min(average.Length - 2, (int)Math.Floor(_highestHz / binHz));

        if (high <= low)
        {
            return;
        }

        var band = average[low..(high + 1)];

        Array.Sort(band);

        var floor = band[band.Length / 2];

        if (floor <= 0)
        {
            return;
        }

        var gate = floor * (1 + (OliviaBlindSearch.GateSigmas / Math.Sqrt(count)));

        foreach (var (occupiedLow, occupiedHigh) in OliviaBlindSearch.Occupied(average, low, high, gate, binHz, _narrowestBand))
        {
            var guess = (occupiedLow + occupiedHigh) / 2;

            if (Inside(guess, occupied) || _sets.Any(s => Math.Abs(s.MiddleHz - guess) < _narrowestBand / 2))
            {
                continue;
            }

            // The tones, over the last two blocks of the slowest row.
            var taken = (int)Math.Min(_historyCount, 2L * _step);
            var recent = _history.AsSpan(_historyCount - taken, taken).ToArray();
            var (tones, spacing, middle) = _search.Tones(recent, _rate, occupiedLow, occupiedHigh);

            if (double.IsNaN(middle))
            {
                middle = guess;
            }

            if (Inside(middle, occupied))
            {
                continue;
            }

            // Each row that fits gets a reader of its own, handed the kept audio.
            var from = Math.Max(_historyStart, _samples - (ReplayBlocks * (long)_step));
            var kept = _history.AsSpan((int)(from - _historyStart), (int)(_samples - from));
            var trials = new List<Trial>();

            foreach (var row in _search.Rank(occupiedHigh - occupiedLow, spacing))
            {
                OliviaDemodulator demodulator;

                try
                {
                    demodulator = new OliviaDemodulator(_format, row, middle, _rate);
                }
                catch (ArgumentOutOfRangeException)
                {
                    // The row's tones would not fit under the rate at this middle; it cannot be this.
                    continue;
                }

                var stream = demodulator.Open();

                stream.Add(kept);
                trials.Add(new Trial(row, stream));
            }

            if (trials.Count > 0)
            {
                _sets.Add(new TrialSet(middle, spacing, tones, occupiedLow, occupiedHigh, from, _samples, trials));
            }
        }
    }

    private void Weigh(IReadOnlyList<OliviaOccupied> occupied, List<OliviaFound> found, bool ending)
    {
        foreach (var set in _sets.ToArray())
        {
            Trial? chosen = null;

            foreach (var trial in set.Trials)
            {
                // **A ROW NO BETTER THAN NOISE IS NOT A DETECTION**: it must clear the threshold.
                if (trial.Stream.BlocksDecoded >= OliviaBlindSearch.ConfirmBlocks && (chosen is null || trial.Stream.SyncSnr > chosen.Stream.SyncSnr))
                {
                    chosen = trial;
                }
            }

            if (chosen is null)
            {
                if (ending || _samples - set.OpenedAt >= TrialBlocks * (long)_step)
                {
                    _sets.Remove(set);
                }

                continue;
            }

            _sets.Remove(set);

            var center = set.MiddleHz + chosen.Stream.OffsetHz;

            // **ONE STATION, ONE CHANNEL** (decision AA): within half the narrower bandwidth of a
            // place already read, it is that place.
            if (occupied.Any(o => Math.Abs(o.CenterHz - center) < Math.Min(o.BandwidthHz, chosen.Row.BandwidthHz) / 2.0)
                || found.Any(f => Math.Abs(f.Candidate.CenterHz - center) < Math.Min(f.Candidate.Variant.BandwidthHz, chosen.Row.BandwidthHz) / 2.0))
            {
                continue;
            }

            found.Add(new OliviaFound(
                new OliviaCandidate(
                    chosen.Row,
                    center,
                    chosen.Stream.SyncSnr,
                    _samples / (double)_rate,
                    set.MiddleHz,
                    set.SpacingHz,
                    set.Tones,
                    set.OccupiedLowHz,
                    set.OccupiedHighHz,
                    set.Trials
                        .Select(t => new OliviaTrial(t.Row.Name, t.Stream.SyncSnr, t.Stream.BlocksDecoded, t.Stream.BlocksRejected, t.Stream.HighestBlockSnr))
                        .ToArray()),
                chosen.Stream,
                set.StartSample));
        }
    }

    private static bool Inside(double hz, IReadOnlyList<OliviaOccupied> occupied)
        => occupied.Any(o => Math.Abs(o.CenterHz - hz) < o.BandwidthHz / 2.0);

    private sealed record Trial(OliviaVariant Row, OliviaStream Stream);

    private sealed record TrialSet(
        double MiddleHz,
        double SpacingHz,
        int Tones,
        double OccupiedLowHz,
        double OccupiedHighHz,
        long StartSample,
        long OpenedAt,
        IReadOnlyList<Trial> Trials);
}
