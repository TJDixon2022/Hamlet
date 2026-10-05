namespace Hamlet.RadioEngine.Scan;

/// <summary>One place the radio's scope shows energy: where, how high, and the floor it stands on.</summary>
/// <param name="FrequencyHz">The peak's centre: the centroid of its excess over the floor, across its bins and sweeps.</param>
/// <param name="Level">The peak's highest value on the scope's own scale.</param>
/// <param name="Floor">The floor on the same scale: the sweeps' median bin, nought where the radio clips its noise.</param>
/// <param name="Spread">The floor's spread on the same scale: 1.4826 times the bins' median deviation, never under one.</param>
/// <param name="Repeats">In how many of the sweeps watched it stood, at the same place.</param>
/// <param name="Sweeps">How many sweeps were watched.</param>
/// <param name="WidthHz">Its width at half its own height over the floor, the middle of the sweeps it showed in (work instruction 543).</param>
public sealed record ScopePeak(long FrequencyHz, double Level, double Floor, double Spread, int Repeats = 0, int Sweeps = 0, double WidthHz = 0);

/// <summary>What became of a peak the survey considered.</summary>
public enum ScopeVerdict
{
    /// <summary>It repeats and is narrow: a station, listed to visit.</summary>
    Listed,

    /// <summary>It did not stand in enough of the sweeps watched.</summary>
    NotRepeating,

    /// <summary>It repeats, and is wider at half its height than a keyed CW signal is.</summary>
    TooWide,

    /// <summary>It is a station, within <see cref="ScopeWatch.MergeHz"/> of a stronger one, and is kept as that one.</summary>
    Merged,
}

/// <summary>A peak the survey considered, and what became of it (work instruction 543, task 3).</summary>
/// <param name="FrequencyHz">Where.</param>
/// <param name="Level">Its highest value on the scope's own scale.</param>
/// <param name="WidthHz">Its width at half its own height over the floor, the middle of the sweeps it showed in.</param>
/// <param name="Stood">In how many sweeps it stood narrow enough, within a bin of its place.</param>
/// <param name="Seen">In how many sweeps it showed at all, within a bin of its place.</param>
/// <param name="Verdict">Listed or skipped.</param>
/// <param name="Why">Why, in words.</param>
public sealed record ScopeCandidate(long FrequencyHz, double Level, double WidthHz, int Stood, int Seen, ScopeVerdict Verdict, string Why);

/// <summary>What the scope showed over the sweeps watched (work instruction 543).</summary>
/// <param name="Sweeps">How many sweeps were watched.</param>
/// <param name="Floor">The floor on the scope's own scale.</param>
/// <param name="Line">The line a bin must stand at or over to count.</param>
/// <param name="BinHz">How wide a bin is.</param>
/// <param name="Considered">Every peak considered, lowest first.</param>
/// <param name="Listed">The stations, lowest first.</param>
public sealed record ScopeSurvey(int Sweeps, double Floor, double Line, double BinHz, IReadOnlyList<ScopeCandidate> Considered, IReadOnlyList<ScopePeak> Listed)
{
    /// <summary>A survey of nothing.</summary>
    public static ScopeSurvey None { get; } = new(0, 0, 0, 0, [], []);
}

/// <summary>
/// **A PEAK IS JUDGED BY WHAT THE SCOPE REALLY SHOWS** (work instruction 542, task 2, HM-DEC-246; its width at half its own
/// height, work instruction 543, HM-DEC-247): the sweeps the radio's scope sends while the scan watches, and the places in
/// them that stand above the floor, repeat at the same place, and are narrow.
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
/// the floor's own spreads over it: band noise does that about once in a billion bins. **Where the median is nought** - the
/// radio has clipped its noise - any value above nought is energy the scope chose to show, and the two tests below
/// decide.</item>
/// <item>**It repeats** (<see cref="RepeatShare"/>, <see cref="LeastRepeats"/>): it stands in a quarter or more of the sweeps
/// watched, and in three at least, within a bin of the same place. A keyed CW signal is up for around half its time, so even
/// a sweep taken at random catches it in a quarter of sweeps with room to spare; a noise blip stands in one sweep and is gone
/// from the next.</item>
/// <item>**It is narrow, measured at half its own height** (<see cref="WidestHz"/>, <see cref="WidestBins"/>). **Work
/// instruction 543 found the clear stations refused here**: the width was counted at the line, which on a clipped floor is
/// the foot of a signal's skirts, so a strong station drew eleven bins and a moderate one five against a limit of three.
/// Half its own height over the floor is where a signal's width is its own and not its strength's. A keyed CW signal
/// occupies about four times its speed in hertz, 160 Hz at 40 WPM, and the scope adds its own resolution; a phone signal is
/// 2.4 kHz and a static crash covers many bins. So it is 250 Hz at most at half its height, or three bins where a bin is
/// wider than a third of that.</item>
/// </list>
/// <para>**A PEAK IS EVERY TOP**, not one per run of bins over the line: on a clipped floor two stations' skirts can touch, and
/// one run would hide the second. A top's width stops where the bins rise again, so a neighbour's slope is not counted as
/// its own; two such tops within <see cref="MergeHz"/> are then one station.</para>
/// <para>**ONE STATION PER HALF A FILTER** (<see cref="MergeHz"/>): two stations closer than half the radio's 500 Hz CW filter
/// land in one passband whichever the dial is tuned to, so the one that stood more is kept.</para>
/// </remarks>
public sealed class ScopeWatch
{
    /// <summary>Where the median is above nought, how many of the floor's own spreads a bin must stand over it: six.</summary>
    public const double MarginSpreads = 6;

    /// <summary>The share of sweeps watched a peak must stand in: a quarter.</summary>
    public const double RepeatShare = 0.25;

    /// <summary>The fewest sweeps a peak must stand in: three.</summary>
    public const int LeastRepeats = 3;

    /// <summary>The widest a peak may be at half its own height, in hertz: 250.</summary>
    public const double WidestHz = 250;

    /// <summary>The widest a peak may be at half its own height, in bins, where three bins are wider than <see cref="WidestHz"/>: three.</summary>
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

    /// <summary>The stations the sweeps watched show inside a stretch of frequency, lowest first.</summary>
    /// <param name="fromHz">The stretch's lower edge.</param>
    /// <param name="toHz">Its upper edge.</param>
    /// <returns>The stations, one per <see cref="MergeHz"/>.</returns>
    public IReadOnlyList<ScopePeak> Peaks(long fromHz, long toHz) => Survey(fromHz, toHz).Listed;

    /// <summary>Every peak the sweeps watched show inside a stretch of frequency, what became of each, and the stations.</summary>
    /// <param name="fromHz">The stretch's lower edge.</param>
    /// <param name="toHz">Its upper edge.</param>
    /// <returns>The survey.</returns>
    public ScopeSurvey Survey(long fromHz, long toHz)
    {
        if (_sweeps.Count == 0 || _highHz <= _lowHz || _sweeps[0].Length < 3)
        {
            return ScopeSurvey.None with { Sweeps = _sweeps.Count };
        }

        var count = _sweeps[0].Length;
        var binHz = (_highHz - _lowHz) / (double)count;

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
        var widest = Math.Max(WidestHz, WidestBins * binHz);

        long Centre(int i) => _lowHz + (long)((i + 0.5) / count * (_highHz - _lowHz));

        // In each sweep, every top over the line, and its width at half its own height.
        var seen = new int[count];
        var stood = new int[count];
        var weight = new double[count];
        var seenWeight = new double[count];
        var seenSum = new double[count];
        var sum = new double[count];
        var highest = new double[count];
        var widths = new List<double>[count];

        foreach (var sweep in _sweeps)
        {
            for (var i = 0; i < count; i++)
            {
                if (sweep[i] < line || (i > 0 && sweep[i] <= sweep[i - 1]) || (i < count - 1 && sweep[i] < sweep[i + 1]))
                {
                    continue;
                }

                var half = Math.Max(line, floor + ((sweep[i] - floor) / 2.0));
                var a = i;
                var b = i;

                // Out from the top while the bins stand at half its height and keep falling: where they rise again, a neighbour
                // begins, and its slope is not this peak's width.
                while (a > 0 && sweep[a - 1] >= half && sweep[a - 1] <= sweep[a])
                {
                    a--;
                }

                while (b < count - 1 && sweep[b + 1] >= half && sweep[b + 1] <= sweep[b])
                {
                    b++;
                }

                var width = (b - a + 1) * binHz;

                seen[i]++;
                highest[i] = Math.Max(highest[i], sweep[i]);
                (widths[i] ??= new List<double>()).Add(width);

                var narrow = width <= widest;

                stood[i] += narrow ? 1 : 0;

                // A station's place is the centroid of its narrow sweeps; a skipped peak's, of every sweep it showed in.
                for (var k = a; k <= b; k++)
                {
                    var excess = Math.Max(0, sweep[k] - floor);

                    seenWeight[i] += excess;
                    seenSum[i] += excess * Centre(k);

                    if (narrow)
                    {
                        weight[i] += excess;
                        sum[i] += excess * Centre(k);
                    }
                }
            }
        }

        int Near(int[] counts, int i) => counts[i] + (i > 0 ? counts[i - 1] : 0) + (i < count - 1 ? counts[i + 1] : 0);
        double NearSum(double[] values, int i) => values[i] + (i > 0 ? values[i - 1] : 0) + (i < count - 1 ? values[i + 1] : 0);

        var least = Math.Max(LeastRepeats, (int)Math.Ceiling(RepeatShare * _sweeps.Count));
        var considered = new List<ScopeCandidate>();
        var stations = new List<(ScopeCandidate Candidate, ScopePeak Peak)>();
        var n = _sweeps.Count;

        for (var i = 0; i < count; i++)
        {
            // A place speaks for itself and a bin either side, where it showed more than the bin below it and no fewer
            // than the bin above.
            if (seen[i] == 0 || (i > 0 && seen[i] <= seen[i - 1]) || (i < count - 1 && seen[i] < seen[i + 1]))
            {
                continue;
            }

            var at = Centre(i);

            if (at < fromHz || at > toHz)
            {
                continue;
            }

            var showed = Near(seen, i);
            var narrow = Near(stood, i);
            var level = Math.Max(highest[i], Math.Max(i > 0 ? highest[i - 1] : 0, i < count - 1 ? highest[i + 1] : 0));
            var all3 = Enumerable.Range(i - 1, 3).Where(k => k >= 0 && k < count && widths[k] is not null).SelectMany(k => widths[k]).OrderBy(w => w).ToList();
            var width = all3.Count > 0 ? all3[all3.Count / 2] : 0;
            var w = NearSum(weight, i);
            var sw = NearSum(seenWeight, i);
            var place = narrow > 0 && w > 0 ? (long)Math.Round(NearSum(sum, i) / w) : sw > 0 ? (long)Math.Round(NearSum(seenSum, i) / sw) : at;

            if (narrow >= least)
            {
                var candidate = new ScopeCandidate(place, level, width, narrow, showed, ScopeVerdict.Listed, $"stood narrow in {narrow} of {n} sweeps");

                stations.Add((candidate, new ScopePeak(place, level, floor, spread, narrow, n, width)));
            }
            else if (showed >= least)
            {
                considered.Add(new ScopeCandidate(place, level, width, narrow, showed, ScopeVerdict.TooWide,
                    $"showed in {showed} of {n} sweeps, {width:0} Hz wide at half its height, over the {widest:0} Hz a keyed CW signal fills, narrow in only {narrow}"));
            }
            else
            {
                considered.Add(new ScopeCandidate(place, level, width, narrow, showed, ScopeVerdict.NotRepeating,
                    $"showed in {showed} of {n} sweeps, under the {least} needed"));
            }
        }

        // One station per half a filter: the one that stood more, then the higher.
        var kept = new List<(ScopeCandidate Candidate, ScopePeak Peak)>();

        foreach (var station in stations.OrderByDescending(s => s.Peak.Repeats).ThenByDescending(s => s.Peak.Level))
        {
            if (kept.FirstOrDefault(k => Math.Abs(k.Peak.FrequencyHz - station.Peak.FrequencyHz) <= MergeHz) is { Peak: not null } keeper)
            {
                considered.Add(station.Candidate with
                {
                    Verdict = ScopeVerdict.Merged,
                    Why = $"within {MergeHz} Hz of the station at {keeper.Peak.FrequencyHz / 1e6:0.0000} MHz, kept as that one",
                });
            }
            else
            {
                kept.Add(station);
                considered.Add(station.Candidate);
            }
        }

        return new ScopeSurvey(
            n, floor, line, binHz,
            considered.OrderBy(c => c.FrequencyHz).ToList(),
            kept.Select(k => k.Peak).OrderBy(p => p.FrequencyHz).ToList());
    }
}
