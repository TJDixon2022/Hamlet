namespace Hamlet.RadioEngine.Scan;

/// <summary>One place the radio's scope shows energy: where, how high, and the floor it stands on.</summary>
/// <param name="FrequencyHz">The peak's centre: the centroid of its excess over the floor, across its bins and sweeps.</param>
/// <param name="Level">The peak's highest value on the scope's own scale.</param>
/// <param name="Floor">The floor on the same scale: the sweeps' median bin, nought where the radio clips its noise.</param>
/// <param name="Spread">The floor's spread on the same scale: 1.4826 times the bins' median deviation, never under one.</param>
/// <param name="Repeats">In how many of the sweeps watched it stood, at the same place.</param>
/// <param name="Sweeps">How many sweeps were watched.</param>
public sealed record ScopePeak(long FrequencyHz, double Level, double Floor, double Spread, int Repeats = 0, int Sweeps = 0);

/// <summary>
/// **A PEAK IS JUDGED BY WHAT THE SCOPE REALLY SHOWS** (work instruction 542, task 2, HM-DEC-246): the sweeps the radio's
/// scope sends while the scan holds still, and the places in them that stand above the floor, repeat at the same place, and
/// are narrow.
/// </summary>
/// <remarks>
/// <para>**THE FIRST REAL SCAN**, 40 m, 2026-10-05: the scope's floor read nought in every catch. The IC-7300 clips its
/// noise to zero, so a margin of six spreads over a median of nought became six scope units, eighteen empty stops sat at 6
/// or 7, and one real CQ also sat at 6. A level alone cannot tell them apart. What a keyed signal does and a noise blip does
/// not is come back, at the same place, sweep after sweep.</para>
/// <para>**THE THREE TESTS, AND THEIR FIGURES** - the author's, from what the scope is and what a keyed signal does, and not
/// from that scan:</para>
/// <list type="bullet">
/// <item>**It stands over the floor.** Where the sweeps' median bin is above nought, it stands <see cref="MarginSpreads"/> of
/// the floor's own spreads over it, as before: band noise does that about once in a billion bins. **Where the median is
/// nought** - the radio has clipped its noise - any value above nought is energy the scope chose to show, and the two tests
/// below decide; a margin over a median of nought measures nothing.</item>
/// <item>**It repeats** (<see cref="RepeatShare"/>, <see cref="LeastRepeats"/>): it stands in a quarter or more of the sweeps
/// watched, and in three at least, within a bin of the same place. A keyed CW signal is up for around half its time, since a
/// dah is three dits and a letter's gaps are dits too, so even a sweep taken at random catches it in a quarter of sweeps
/// with room to spare; a noise blip stands in one sweep and is gone from the next.</item>
/// <item>**It is narrow** (<see cref="WidestBins"/>): three bins or fewer stand together where it stands. A CW signal is
/// tens of hertz wide, one bin or two at the scan's span where a bin is a few hundred hertz, three where it straddles two
/// and its key clicks show; a static crash or a splatter covers many bins at once.</item>
/// </list>
/// <para>**ONE PEAK PER FILTER WIDTH** (<see cref="MergeHz"/>): two stations closer than half the radio's 500 Hz CW filter
/// land in one passband whichever the dial is tuned to, so the higher is kept.</para>
/// </remarks>
public sealed class ScopeWatch
{
    /// <summary>Where the median is above nought, how many of the floor's own spreads a bin must stand over it: six.</summary>
    public const double MarginSpreads = 6;

    /// <summary>The share of sweeps watched a peak must stand in: a quarter.</summary>
    public const double RepeatShare = 0.25;

    /// <summary>The fewest sweeps a peak must stand in: three.</summary>
    public const int LeastRepeats = 3;

    /// <summary>The widest a peak may be, in bins standing together: three.</summary>
    public const int WidestBins = 3;

    /// <summary>How close two peaks may be and be one: 250 Hz, half the radio's CW filter.</summary>
    public const long MergeHz = 250;

    private readonly List<byte[]> _sweeps = new();
    private long _lowHz;
    private long _highHz;

    /// <summary>How many sweeps are watched now.</summary>
    public int Sweeps => _sweeps.Count;

    /// <summary>The span the sweeps watched cover.</summary>
    public (long LowHz, long HighHz) Span => (_lowHz, _highHz);

    /// <summary>Forget the sweeps watched; begin again.</summary>
    public void Clear() => _sweeps.Clear();

    /// <summary>Watch one more sweep; a sweep over another span, or of another length, begins again.</summary>
    /// <param name="lowHz">The sweep's lower edge.</param>
    /// <param name="highHz">Its upper edge.</param>
    /// <param name="bins">Its bins.</param>
    public void Add(long lowHz, long highHz, ReadOnlySpan<byte> bins)
    {
        if (lowHz != _lowHz || highHz != _highHz || (_sweeps.Count > 0 && _sweeps[0].Length != bins.Length))
        {
            _sweeps.Clear();
            _lowHz = lowHz;
            _highHz = highHz;
        }

        _sweeps.Add(bins.ToArray());
    }

    /// <summary>The peaks the sweeps watched show inside a stretch of frequency, lowest first.</summary>
    /// <param name="fromHz">The stretch's lower edge.</param>
    /// <param name="toHz">Its upper edge.</param>
    /// <returns>The peaks, one per <see cref="MergeHz"/>.</returns>
    public IReadOnlyList<ScopePeak> Peaks(long fromHz, long toHz)
    {
        if (_sweeps.Count == 0 || _highHz <= _lowHz)
        {
            return [];
        }

        var count = _sweeps[0].Length;

        if (count < 3)
        {
            return [];
        }

        // The floor: the median of every bin of every sweep. Its spread: from the bins over it, where the floor is clipped
        // to nought, and from all of them otherwise.
        var all = _sweeps.SelectMany(s => s).Select(b => (double)b).OrderBy(b => b).ToArray();
        var floor = all[all.Length / 2];
        var over = floor > 0 ? all : all.Where(b => b > floor).ToArray();
        var spread = 1.0;

        if (over.Length > 0)
        {
            var middle = over[over.Length / 2];
            var deviations = over.Select(b => Math.Abs(b - middle)).OrderBy(d => d).ToArray();

            spread = Math.Max(1, 1.4826 * deviations[deviations.Length / 2]);
        }

        var line = floor > 0 ? floor + (MarginSpreads * spread) : floor + 1;

        long Centre(int i) => _lowHz + (long)((i + 0.5) / count * (_highHz - _lowHz));

        // In each sweep, the narrow places standing over the line, by their highest bin.
        var stood = new int[count];
        var weight = new double[count];
        var sum = new double[count];
        var highest = new double[count];

        foreach (var sweep in _sweeps)
        {
            var i = 0;

            while (i < count)
            {
                if (sweep[i] < line)
                {
                    i++;
                    continue;
                }

                var start = i;

                while (i < count && sweep[i] >= line)
                {
                    i++;
                }

                if (i - start > WidestBins)
                {
                    continue;
                }

                var top = start;

                for (var k = start; k < i; k++)
                {
                    if (sweep[k] > sweep[top])
                    {
                        top = k;
                    }
                }

                stood[top]++;
                highest[top] = Math.Max(highest[top], sweep[top]);

                for (var k = Math.Max(0, start - 1); k < Math.Min(count, i + 1); k++)
                {
                    var excess = Math.Max(0, sweep[k] - floor);

                    weight[top] += excess;
                    sum[top] += excess * Centre(k);
                }
            }
        }

        // A place repeats where it, or a bin either side, stood in enough sweeps.
        var least = Math.Max(LeastRepeats, (int)Math.Ceiling(RepeatShare * _sweeps.Count));
        var raw = new List<ScopePeak>();

        for (var i = 0; i < count; i++)
        {
            if (stood[i] == 0)
            {
                continue;
            }

            var at = Centre(i);

            if (at < fromHz || at > toHz)
            {
                continue;
            }

            var repeats = stood[i] + (i > 0 ? stood[i - 1] : 0) + (i < count - 1 ? stood[i + 1] : 0);

            // The place's own bin speaks for it only where it is the highest of the three.
            var isTop = (i == 0 || stood[i] >= stood[i - 1]) && (i == count - 1 || stood[i] >= stood[i + 1]);

            if (repeats < least || !isTop)
            {
                continue;
            }

            var w = weight[i] + (i > 0 ? weight[i - 1] : 0) + (i < count - 1 ? weight[i + 1] : 0);
            var s = sum[i] + (i > 0 ? sum[i - 1] : 0) + (i < count - 1 ? sum[i + 1] : 0);
            var level = Math.Max(highest[i], Math.Max(i > 0 ? highest[i - 1] : 0, i < count - 1 ? highest[i + 1] : 0));

            raw.Add(new ScopePeak(w > 0 ? (long)Math.Round(s / w) : at, level, floor, spread, repeats, _sweeps.Count));
        }

        var kept = new List<ScopePeak>();

        foreach (var peak in raw.OrderByDescending(p => p.Repeats).ThenByDescending(p => p.Level))
        {
            if (kept.All(k => Math.Abs(k.FrequencyHz - peak.FrequencyHz) > MergeHz))
            {
                kept.Add(peak);
            }
        }

        return kept.OrderBy(p => p.FrequencyHz).ToList();
    }
}
