namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// Where a station is, from how far each bin swings between keyed and quiet.
/// </summary>
/// <remarks>
/// <para>**A STATION IS A BIN THAT SWINGS. IT IS NOT THE LOUDEST AVERAGE BIN**
/// (work instruction 055, task 2). On `cw-2026-08-31-002443` Hamlet chose 510 Hz,
/// the loudest average, and emitted forty-eight `E`s from it; the keyed carriers
/// were at 775, 703 and 562. **On a signal keyed a third of the time the average
/// is dominated by the two thirds in which nobody is sending**, so the loudest
/// average is whichever part of the band has the most noise in it.</para>
/// <para>**AND ON `cw-2026-08-31-003229` THE SURVEY FOUND NOTHING AT ALL** while
/// a station called CQ. With the squelch wired, an admission failure turns the
/// whole screen to blocks — the sidecar reads `unkeyed YES` beside forty-three
/// characters, every one of them refused. Swing finds that station at 588 Hz,
/// where an independent decoder reads 583.5.</para>
/// <para>**THIS SEES MORE AND REQUIRES NOTHING LESS** (HM-DEC-120, tightened
/// only). A noise bin does not swing twenty decibels while standing at the top of
/// the band; the silence fixtures bound the threshold from below and the margin is
/// measured rather than assumed.</para>
/// </remarks>
public static class CwSwingSurvey
{
    /// <summary>The lowest pitch considered, in hertz.</summary>
    public const double LowHz = 400;

    /// <summary>The highest pitch considered, in hertz.</summary>
    public const double HighHz = 1000;

    /// <summary>How far apart the candidate bins sit, in hertz.</summary>
    /// <remarks>
    /// Half the tone tracker's own 25 Hz spacing, so a station between two of its
    /// bins still lands on one of these.
    /// </remarks>
    public const double SpacingHz = 12.5;

    /// <summary>The percentile taken as a bin's keyed level.</summary>
    public const double KeyedPercentile = 95;

    /// <summary>The percentile taken as a bin's quiet level.</summary>
    /// <remarks>
    /// Twenty rather than the minimum: a single hop of deep fade is not the gap
    /// level, and a percentile is robust to it where a minimum is not.
    /// </remarks>
    public const double QuietPercentile = 20;

    /// <summary>One candidate bin and what it measured.</summary>
    /// <param name="Hz">The bin.</param>
    /// <param name="SwingDb">Keyed level less quiet level.</param>
    /// <param name="KeyedDb">What it reaches when somebody is sending.</param>
    public readonly record struct Candidate(double Hz, double SwingDb, double KeyedDb);

    /// <summary>Rank the band by swing, strongest first.</summary>
    /// <param name="samples">The audio.</param>
    /// <param name="sampleRate">Its sample rate.</param>
    /// <returns>Candidates, best first, or empty where nothing could be measured.</returns>
    /// <remarks>
    /// <para>**THE STOPBAND SWINGS HARDEST AND HOLDS NOTHING.** Above about 800 Hz
    /// the receiver's own filter rolls the level from −57 dB to −85, and a decibel
    /// swing on a near-zero signal is the logarithm stretching noise: ranked
    /// without a guard, every bin from 850 to 1000 Hz beat the station on every
    /// capture of 2026-08-31.</para>
    /// <para>**SO A CANDIDATE HAS TO BE LOUD WHEN KEYED, NOT MERELY VARIABLE**, and
    /// the reference for that is the band's own median keyed level — which is
    /// inside the passband by construction, where a figure taken against the
    /// filter skirt would be the receiver's shape rather than the signal's. On
    /// `003229` the station sits 10 dB above that median and the whole stopband
    /// 20 dB below it.</para>
    /// </remarks>
    public static IReadOnlyList<Candidate> Rank(float[] samples, int sampleRate)
    {
        ArgumentNullException.ThrowIfNull(samples);

        var measured = new List<Candidate>();

        for (var hz = LowHz; hz <= HighHz; hz += SpacingHz)
        {
            var envelope = CwProbabilisticDecoder.Envelope(samples, sampleRate, hz);

            if (envelope.Length < 32)
            {
                continue;
            }

            var db = new double[envelope.Length];

            for (var i = 0; i < envelope.Length; i++)
            {
                db[i] = 20 * Math.Log10(Math.Max(envelope[i], 1e-12));
            }

            Array.Sort(db);

            var quiet = Percentile(db, QuietPercentile);
            var keyed = Percentile(db, KeyedPercentile);

            measured.Add(new Candidate(hz, keyed - quiet, keyed));
        }

        if (measured.Count == 0)
        {
            return Array.Empty<Candidate>();
        }

        var medianKeyed = measured
            .Select(c => c.KeyedDb)
            .OrderBy(v => v)
            .ElementAt(measured.Count / 2);

        return measured
            .Where(c => c.KeyedDb >= medianKeyed)
            .OrderByDescending(c => c.SwingDb)
            .ThenByDescending(c => c.KeyedDb)
            .ToList();
    }

    /// <summary>The best candidate, or null where none swings enough.</summary>
    /// <param name="samples">The audio.</param>
    /// <param name="sampleRate">Its sample rate.</param>
    /// <param name="leastSwingDb">How far a bin must swing to be a candidate.</param>
    /// <returns>The winner, or null.</returns>
    /// <remarks>
    /// **NULL IS "NOBODY IS KEYING HERE" AND IT IS THE ANSWER AN EMPTY BAND
    /// DESERVES** (§0.0, HM-DEC-120). The threshold is bounded from below by what
    /// a station-free recording produces, and that margin is measured in the
    /// tests rather than asserted here.
    /// </remarks>
    public static Candidate? Best(
        float[] samples, int sampleRate, double leastSwingDb)
    {
        var ranked = Rank(samples, sampleRate);

        return ranked.Count > 0 && ranked[0].SwingDb >= leastSwingDb
            ? ranked[0]
            : null;
    }

    /// <summary>One percentile of a sorted array.</summary>
    private static double Percentile(double[] sorted, double share)
    {
        var at = (share / 100.0) * (sorted.Length - 1);
        var low = (int)Math.Floor(at);
        var high = Math.Min(low + 1, sorted.Length - 1);

        return sorted[low] + ((sorted[high] - sorted[low]) * (at - low));
    }
}
