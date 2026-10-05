namespace Hamlet.RadioEngine.Scan;

/// <summary>One place the radio's scope shows energy: where, how high, and the floor it stands on.</summary>
/// <param name="FrequencyHz">The peak bin's centre.</param>
/// <param name="Level">The peak's height on the scope's own scale.</param>
/// <param name="Floor">The sweep's floor on the same scale: the median of its bins.</param>
/// <param name="Spread">The floor's own spread on the same scale: 1.4826 times the bins' median deviation, never under one.</param>
public sealed record ScopePeak(long FrequencyHz, double Level, double Floor, double Spread);

/// <summary>
/// **WHERE THE SCOPE SHOWS SOMETHING** (work instruction 540, HM-DEC-244): peaks in a sweep of the radio's own scope
/// that stand a stated margin over the sweep's own floor, inside a CW segment.
/// </summary>
/// <remarks>
/// <para>**THE MARGIN IS SIX OF THE FLOOR'S OWN SPREADS** (<see cref="MarginSpreads"/>), read from the sweep and not
/// from a decibel figure, since the scope's scale is the radio's and is not stated in the manual. A sweep's bins are
/// mostly band noise, so their median is the floor and their median deviation, times 1.4826, is the floor's spread as a
/// standard deviation would be; a bin of noise stands six of those over the floor about once in a billion, so a sweep of
/// 475 noise bins shows no peak. The author's, from what a floor is, not from a result.</para>
/// <para>**ONE PEAK PER FILTER WIDTH** (<see cref="MergeHz"/>): a keyed tone spreads over a few bins, and two stations
/// closer than half the radio's 500 Hz CW filter land in one passband whichever the dial is tuned to, so the higher
/// is kept.</para>
/// <para>Sweeps are taken with their highest bin held over a second or two (<see cref="MaxHold"/>): a keyed station is
/// silent between its marks, and a single sweep taken in a key-up shows nothing where it is.</para>
/// </remarks>
public static class ScopePeaks
{
    /// <summary>How many of the floor's own spreads a peak must stand over it: six.</summary>
    public const double MarginSpreads = 6;

    /// <summary>How close two peaks may be and be one: 250 Hz, half the radio's CW filter.</summary>
    public const long MergeHz = 250;

    /// <summary>Holds each bin's highest value across sweeps of one span.</summary>
    /// <param name="into">The held bins, grown to the sweep's length; a new span starts again.</param>
    /// <param name="bins">This sweep.</param>
    public static void MaxHold(List<byte> into, ReadOnlySpan<byte> bins)
    {
        ArgumentNullException.ThrowIfNull(into);

        if (into.Count != bins.Length)
        {
            into.Clear();

            foreach (var b in bins)
            {
                into.Add(b);
            }

            return;
        }

        for (var i = 0; i < bins.Length; i++)
        {
            if (bins[i] > into[i])
            {
                into[i] = bins[i];
            }
        }
    }

    /// <summary>The peaks of a sweep inside a stretch of frequency, lowest first.</summary>
    /// <param name="bins">The sweep, its first bin's lower edge at <paramref name="lowHz"/>.</param>
    /// <param name="lowHz">The sweep's lower edge.</param>
    /// <param name="highHz">The sweep's upper edge.</param>
    /// <param name="fromHz">The stretch's lower edge: the CW segment, or the part of it this sweep covers.</param>
    /// <param name="toHz">The stretch's upper edge.</param>
    /// <returns>The peaks, one per <see cref="MergeHz"/>.</returns>
    public static IReadOnlyList<ScopePeak> Find(IReadOnlyList<byte> bins, long lowHz, long highHz, long fromHz, long toHz)
    {
        ArgumentNullException.ThrowIfNull(bins);

        if (bins.Count < 3 || highHz <= lowHz)
        {
            return [];
        }

        var sorted = bins.Select(b => (double)b).OrderBy(b => b).ToArray();
        var floor = sorted[sorted.Length / 2];
        var deviations = bins.Select(b => Math.Abs(b - floor)).OrderBy(d => d).ToArray();
        var spread = Math.Max(1, 1.4826 * deviations[deviations.Length / 2]);
        var line = floor + (MarginSpreads * spread);

        long Centre(int i) => lowHz + (long)((i + 0.5) / bins.Count * (highHz - lowHz));

        var raw = new List<ScopePeak>();

        for (var i = 0; i < bins.Count; i++)
        {
            var at = Centre(i);

            if (at < fromHz || at > toHz || bins[i] < line)
            {
                continue;
            }

            var left = i > 0 ? bins[i - 1] : (byte)0;
            var right = i < bins.Count - 1 ? bins[i + 1] : (byte)0;

            if (bins[i] >= left && bins[i] >= right)
            {
                // **THE PEAK'S OWN CENTRE, NOT ITS BIN'S** (work instruction 540): a bin spans a few hundred hertz at a wide
                // span, so the peak is placed at the centroid of its excess over the floor across the two bins either side.
                double weight = 0, sum = 0;

                for (var k = Math.Max(0, i - 2); k <= Math.Min(bins.Count - 1, i + 2); k++)
                {
                    var excess = Math.Max(0, bins[k] - floor);

                    weight += excess;
                    sum += excess * Centre(k);
                }

                raw.Add(new ScopePeak(weight > 0 ? (long)Math.Round(sum / weight) : at, bins[i], floor, spread));
            }
        }

        // One per filter width, the highest kept.
        var kept = new List<ScopePeak>();

        foreach (var peak in raw.OrderByDescending(p => p.Level))
        {
            if (kept.All(k => Math.Abs(k.FrequencyHz - peak.FrequencyHz) > MergeHz))
            {
                kept.Add(peak);
            }
        }

        return kept.OrderBy(p => p.FrequencyHz).ToList();
    }
}
