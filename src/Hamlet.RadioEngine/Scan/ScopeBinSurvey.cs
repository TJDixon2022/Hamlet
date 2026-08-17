using Hamlet.RadioEngine.Training;

namespace Hamlet.RadioEngine.Scan;

/// <summary>
/// What the waterfall has seen at one place on the band.
/// </summary>
/// <param name="CenterHz">Where it is.</param>
/// <param name="Presence">
/// How much of the time something stood above the band there, from nought to
/// one.
/// </param>
/// <param name="Variability">
/// How much the level moves, in amplitude counts. Keying moves; a carrier does
/// not.
/// </param>
/// <param name="LiftCounts">How far its loud moments stand over the band.</param>
/// <param name="Sweeps">How many sweeps this rests on.</param>
/// <remarks>
/// **NOTHING HERE SAYS THIS IS MORSE, AND NOTHING HERE COULD.** The scope
/// delivers about four and a half sweeps a second and a dit at twenty words a
/// minute is sixty milliseconds, so seeing an element would need something like
/// thirty sweeps a second. The keying is aliased away completely. What survives
/// aliasing is how often a bin is occupied and how much its level moves, and
/// those two are enough to sort a band into places worth listening to and places
/// that are not (§0.0).
/// </remarks>
public readonly record struct ScopeBin(
    long CenterHz, double Presence, double Variability, double LiftCounts, int Sweeps)
{
    /// <summary>
    /// How promising this place is, higher being better.
    /// </summary>
    /// <remarks>
    /// <para>**INTERMITTENCY, WITH STEADY CARRIERS EXPLICITLY DEMOTED.** An
    /// operator sending occupies a bin something like half the time and swings
    /// between two levels. A carrier occupies it always and swings not at all,
    /// and is the loudest thing on many bands, so anything ranking by strength
    /// tours the birdies and never reaches a person.</para>
    /// <para>Empty spectrum scores nothing on either count, which is what makes
    /// this a ranking rather than a detector: it says where to point the
    /// receiver, and the audio decoder says whether anybody is there.</para>
    /// </remarks>
    public double Score
    {
        get
        {
            if (Sweeps < ScopeBinSurvey.LeastSweeps || LiftCounts <= 0)
            {
                return 0;
            }

            // Furthest from always-on and from never-on at once.
            var intermittent = 1 - (Math.Abs(Presence - 0.55) / 0.55);

            if (intermittent <= 0)
            {
                return 0;
            }

            // A carrier is loud and still. Its variability is what disqualifies
            // it, not its strength, because strength is the thing it wins on.
            var moving = Math.Min(1, Variability / ScopeBinSurvey.KeyedSwingCounts);

            return intermittent * moving * Math.Min(1, LiftCounts / 40.0);
        }
    }

    /// <summary>
    /// True when this looks like something switched on and left on.
    /// </summary>
    /// <remarks>
    /// Reported rather than merely demoted, because a carrier inside the receive
    /// passband sets the gain for everything quieter and the operator can do
    /// something about it (§1.4, HM-DEC-096 phase 5).
    /// </remarks>
    public bool LooksSteady
        => Sweeps >= ScopeBinSurvey.LeastSweeps
           && Presence > 0.9
           && Variability < ScopeBinSurvey.SteadySwingCounts
           && LiftCounts > 0;
}

/// <summary>
/// Accumulates what the waterfall says about each part of the band, so a scan
/// has somewhere to start (HM-DEC-107, phase 6).
/// </summary>
/// <remarks>
/// <para>**THE WATERFALL PROPOSES AND THE AUDIO DECODER CONFIRMS.** The scope
/// span is around five hundred kilohertz against a five hundred hertz receive
/// passband, so this surveys roughly a thousand times more spectrum than the
/// operator can hear at once. That is its whole value: it turns a linear crawl
/// across a band into a ranked list of places worth stopping.</para>
/// <para>**AND IT CANNOT IDENTIFY MORSE.** Four and a half sweeps a second
/// against a sixty millisecond dit means the keying is aliased completely. Every
/// number here is occupancy or movement over ten to thirty seconds, and none of
/// them is a claim that a signal is CW. A scan built on this stops the receiver
/// somewhere; only the decoder says whether anybody was there.</para>
/// <para>Nothing in this class touches the radio. It listens to frames that are
/// arriving anyway and adds no traffic to the bus.</para>
/// </remarks>
public sealed class ScopeBinSurvey
{
    /// <summary>How many sweeps a bin needs before it is worth ranking.</summary>
    /// <remarks>
    /// Forty-five, which is about ten seconds at the rate the radio sends them.
    /// Below that a bin has not been watched long enough to tell an operator
    /// pausing between calls from an empty patch of band.
    /// </remarks>
    public const int LeastSweeps = 45;

    /// <summary>How far above the band floor a bin counts as occupied.</summary>
    /// <remarks>
    /// Ten amplitude counts out of the radio's nought-to-a-hundred-and-sixty
    /// scale (p. 19-12). Below that the scope's own noise moves this much.
    /// </remarks>
    public const double OccupiedOverFloor = 10;

    /// <summary>The swing a keyed signal shows, in amplitude counts.</summary>
    public const double KeyedSwingCounts = 15;

    /// <summary>Below this swing a bin is not being keyed by anybody.</summary>
    public const double SteadySwingCounts = 6;

    /// <summary>How much history each bin keeps, in sweeps.</summary>
    /// <remarks>
    /// A hundred and thirty-five, about thirty seconds, which is the far end of
    /// the window the survey is meant to work over. Longer and a station that has
    /// finished goes on being recommended.
    /// </remarks>
    public const int HistorySweeps = 135;

    private readonly object _gate = new();

    private long _lowHz;
    private long _highHz;
    private int _binCount;

    private byte[] _history = Array.Empty<byte>();
    private int _write;
    private int _filled;

    private readonly List<double> _scratch = new();

    /// <summary>How many sweeps have been taken in.</summary>
    public int Sweeps => _filled;

    /// <summary>The span the survey currently covers, in hertz.</summary>
    public (long LowHz, long HighHz) Span
    {
        get
        {
            lock (_gate)
            {
                return (_lowHz, _highHz);
            }
        }
    }

    /// <summary>
    /// Take in one sweep.
    /// </summary>
    /// <param name="frame">The sweep, as the radio computed it.</param>
    /// <remarks>
    /// **A CHANGE OF SPAN THROWS THE HISTORY AWAY.** The bins mean different
    /// frequencies afterwards, so carrying the old counts across would report
    /// occupancy at places nothing was ever measured (§0.0). That happens
    /// whenever the operator changes band, which a scan does deliberately.
    /// </remarks>
    public void Observe(in SpectrumFrame frame)
    {
        lock (_gate)
        {
            if (frame.LowHz != _lowHz
                || frame.HighHz != _highHz
                || frame.Bins.Length != _binCount)
            {
                _lowHz = frame.LowHz;
                _highHz = frame.HighHz;
                _binCount = frame.Bins.Length;
                _history = new byte[_binCount * HistorySweeps];
                _write = 0;
                _filled = 0;
            }

            if (_binCount == 0)
            {
                return;
            }

            frame.Bins.CopyTo(_history.AsSpan(_write * _binCount, _binCount));
            _write = (_write + 1) % HistorySweeps;
            _filled = Math.Min(_filled + 1, HistorySweeps);
        }
    }

    /// <summary>Forget everything, because the radio has moved.</summary>
    public void Reset()
    {
        lock (_gate)
        {
            _write = 0;
            _filled = 0;
        }
    }

    /// <summary>
    /// What every bin has been doing, ranked with the most promising first.
    /// </summary>
    /// <param name="most">How many to return.</param>
    /// <returns>The candidates, best first, or an empty list.</returns>
    public IReadOnlyList<ScopeBin> Ranked(int most = 16)
    {
        var all = Describe();

        return all
            .Where(b => b.Score > 0)
            .OrderByDescending(b => b.Score)
            .Take(Math.Max(0, most))
            .ToList();
    }

    /// <summary>
    /// Bins that look like something switched on and left on.
    /// </summary>
    public IReadOnlyList<ScopeBin> Steady()
        => Describe().Where(b => b.LooksSteady).ToList();

    /// <summary>Every bin, measured.</summary>
    public IReadOnlyList<ScopeBin> Describe()
    {
        lock (_gate)
        {
            if (_filled < LeastSweeps || _binCount == 0)
            {
                return Array.Empty<ScopeBin>();
            }

            // The band's own floor, taken across the whole sweep so one loud bin
            // cannot raise it. A median, for the same reason the decoder uses one.
            _scratch.Clear();

            for (var i = 0; i < _filled; i++)
            {
                _scratch.Add(_history[(i * _binCount) + (i % _binCount)]);
            }

            _scratch.Sort();
            var floor = _scratch[_scratch.Count / 2];

            var bins = new List<ScopeBin>(_binCount);
            var step = (double)(_highHz - _lowHz) / Math.Max(1, _binCount - 1);

            for (var b = 0; b < _binCount; b++)
            {
                double sum = 0, sumSquares = 0;
                var occupied = 0;
                double loudest = 0;

                for (var i = 0; i < _filled; i++)
                {
                    double value = _history[(i * _binCount) + b];

                    sum += value;
                    sumSquares += value * value;
                    loudest = Math.Max(loudest, value);

                    if (value - floor >= OccupiedOverFloor)
                    {
                        occupied++;
                    }
                }

                var mean = sum / _filled;
                var variance = Math.Max(0, (sumSquares / _filled) - (mean * mean));

                bins.Add(new ScopeBin(
                    _lowHz + (long)Math.Round(b * step),
                    (double)occupied / _filled,
                    Math.Sqrt(variance),
                    loudest - floor,
                    _filled));
            }

            return bins;
        }
    }
}
