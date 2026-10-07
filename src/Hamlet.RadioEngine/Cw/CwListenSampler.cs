namespace Hamlet.RadioEngine.Cw;

/// <summary>What the CW listening was doing over ten seconds, for the <c>cw_listen</c> row (work instruction 549, task 3).</summary>
/// <param name="AudioLostMs">Audio lost before the decode since listening began.</param>
/// <param name="AudioLostLast10sMs">Of that, how much in these ten seconds.</param>
/// <param name="LongestStallMs">The longest a capture callback came late, since listening began.</param>
/// <param name="QueuePeak">The deepest the decode's queue got, since listening began.</param>
/// <param name="Senders">Every sender the gate holds now, which is printed and which has qualified.</param>
/// <param name="LettersPrinted">Letters the terminal printed in these ten seconds.</param>
/// <param name="Light">The hold-still light's state, in its own words.</param>
/// <param name="CaptureUnderWay">The folder of an automatic capture under way, its name alone, or null.</param>
public sealed record CwListenSample(
    double AudioLostMs,
    double AudioLostLast10sMs,
    double LongestStallMs,
    int QueuePeak,
    IReadOnlyList<CwSenderStanding> Senders,
    long LettersPrinted,
    string Light,
    string? CaptureUnderWay);

/// <summary>
/// **THE TELEMETRY SAYS WHAT IS HAPPENING, EVERY TEN SECONDS, WHILE LISTENING** (work instruction 549, task 3).
/// </summary>
/// <remarks>
/// <para>**WHY A ROW ON A CLOCK.** The audio-loss counters reached the telemetry only on a verdict press, and the sampled
/// <c>decode_quality</c> row is written only when something it watches moved; so an afternoon of W1AW holding four to nine
/// senders left nothing that said, second by second, how many and which. Every ten seconds, moved or not, is six rows a
/// minute and a little over twenty kilobytes an hour.</para>
/// <para>The deltas - lost in these ten seconds, letters printed in them - are taken against the last row, so the first row
/// after listening starts covers the time since it started.</para>
/// </remarks>
public sealed class CwListenSampler
{
    /// <summary>How often a row is written.</summary>
    public static readonly TimeSpan Interval = TimeSpan.FromSeconds(10);

    private DateTime _last = DateTime.MinValue;
    private double _lostMs;
    private long _letters;

    /// <summary>A sample, where ten seconds have passed since the last; null otherwise.</summary>
    /// <param name="nowUtc">The moment.</param>
    /// <param name="audio">What the live path has received and lost.</param>
    /// <param name="shape">What the shape side holds.</param>
    /// <param name="light">The light's state, in its own words.</param>
    /// <param name="captureUnderWay">An automatic capture under way, its folder's name, or null.</param>
    /// <returns>The sample, or null.</returns>
    public CwListenSample? Sample(
        DateTime nowUtc, AudioContinuity audio, CwShapeSideReading shape, string light, string? captureUnderWay)
    {
        ArgumentNullException.ThrowIfNull(audio);
        ArgumentNullException.ThrowIfNull(shape);

        if (_last != DateTime.MinValue && nowUtc - _last < Interval)
        {
            return null;
        }

        _last = nowUtc;

        var sample = new CwListenSample(
            audio.LostMilliseconds,
            Math.Max(0, audio.LostMilliseconds - _lostMs),
            audio.LongestStallMilliseconds,
            audio.QueuePeak,
            shape.Senders,
            Math.Max(0, shape.LettersPrinted - _letters),
            light,
            captureUnderWay);

        _lostMs = audio.LostMilliseconds;
        _letters = shape.LettersPrinted;

        return sample;
    }
}
