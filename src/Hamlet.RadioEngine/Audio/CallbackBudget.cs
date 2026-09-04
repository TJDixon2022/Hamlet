using System.Diagnostics;

namespace Hamlet.RadioEngine.Audio;

/// <summary>
/// The device's buffer period, and how many callbacks ran past it.
/// </summary>
/// <remarks>
/// <para>**THE PERIOD IS THE BUDGET, AND IT IS THE ONLY HONEST ONE.** WASAPI
/// hands over one buffer per period and expects the callback back before the
/// next one is ready. A callback that runs longer than the period has not merely
/// been slow: the device filled the next buffer while it was still working, and
/// whatever the driver does about that, samples are at risk. So the interesting
/// question is not how long the longest callback took but how many exceeded the
/// time they had, which is a count and not a judgement.</para>
/// <para>**UNIT 238 MEASURED AGAINST 20,000 MICROSECONDS AND THAT WAS NEVER THE
/// BUDGET.** It came from the 960 samples at 48 kHz that `BufferedAudioSource`
/// hands the decoder, which is a different quantity in a different part of the
/// pipeline. Against the real figure the shack machine's worst callback of
/// 91,372 µs is 91% of its budget rather than four and a half times it, and only
/// one of those two readings could lead anybody anywhere useful.</para>
/// <para>**HALF THE PERIOD IS COUNTED TOO, AND IT IS THE MORE USEFUL NUMBER.**
/// A count of full overruns only moves once the damage is done. A callback
/// consistently over half its budget has no headroom left for a garbage
/// collection or a scheduling hiccup, so the half count is what rises first and
/// what says a machine is close to the edge while it is still working.</para>
/// <para>**NOTHING HERE INTERPRETS.** It does not say the sound card is failing,
/// does not name a cause, and does not decide anything. It counts (HM-DEC-093).</para>
/// </remarks>
public sealed class CallbackBudget
{
    private long _overPeriod;
    private long _overHalfPeriod;
    private long _measured;
    private double _longestMicroseconds;

    /// <summary>Creates a budget against a device period.</summary>
    /// <param name="periodMicroseconds">
    /// The device's buffer period. Zero or less means the period is not known,
    /// and then nothing is ever counted as an overrun.
    /// </param>
    public CallbackBudget(double periodMicroseconds)
        => PeriodMicroseconds = periodMicroseconds;

    /// <summary>The device's buffer period, or zero where it is not known.</summary>
    /// <remarks>
    /// **ZERO IS NOT KNOWN AND IS NOT A BUDGET OF NOTHING** (§0.0). A source with
    /// no period reports its longest callback and refuses to say whether that
    /// was too long, because against an unknown budget nobody can say.
    /// </remarks>
    public double PeriodMicroseconds { get; }

    /// <summary>Callbacks that took longer than the whole buffer period.</summary>
    public long OverPeriod => _overPeriod;

    /// <summary>Callbacks that took longer than half the buffer period.</summary>
    /// <remarks>
    /// **EVERY FULL OVERRUN IS ALSO COUNTED HERE**, because it is also over half.
    /// Two counts that overlap are easier to read than two that partition: this
    /// one answers "how often was it close" without the reader having to add.
    /// </remarks>
    public long OverHalfPeriod => _overHalfPeriod;

    /// <summary>How many callbacks have been timed at all.</summary>
    /// <remarks>
    /// **WITHOUT IT, ZERO OVERRUNS IS AMBIGUOUS.** No overruns in a million
    /// callbacks and no overruns because nothing has run yet are the same number
    /// and opposite facts, and this project has already shipped that mistake once
    /// (HM-DEC-093).
    /// </remarks>
    public long Measured => _measured;

    /// <summary>The longest single callback seen, in microseconds.</summary>
    public double LongestMicroseconds => _longestMicroseconds;

    /// <summary>Take one callback's duration.</summary>
    /// <param name="microseconds">How long it ran.</param>
    /// <remarks>
    /// **CALLED FROM THE AUDIO CALLBACK, SO IT ALLOCATES NOTHING AND THROWS
    /// NOTHING** (§8's never-throw discipline). Four fields and two comparisons.
    /// </remarks>
    public void Record(double microseconds)
    {
        _measured++;

        if (microseconds > _longestMicroseconds)
        {
            _longestMicroseconds = microseconds;
        }

        if (PeriodMicroseconds <= 0)
        {
            return;
        }

        if (microseconds > PeriodMicroseconds)
        {
            _overPeriod++;
        }

        if (microseconds > PeriodMicroseconds / 2)
        {
            _overHalfPeriod++;
        }
    }

    /// <summary>Time one piece of work and take its duration.</summary>
    /// <param name="work">The work.</param>
    /// <exception cref="ArgumentNullException">No work.</exception>
    /// <remarks>
    /// **FOR TESTS AND FOR CALLERS THAT ARE NOT ALREADY TIMING THEMSELVES.** The
    /// real callback times itself in its own `finally`, because it has to record
    /// the duration whether the work threw or not.
    /// </remarks>
    public void Time(Action work)
    {
        ArgumentNullException.ThrowIfNull(work);

        var started = Stopwatch.GetTimestamp();

        try
        {
            work();
        }
        finally
        {
            Record((Stopwatch.GetTimestamp() - started) * 1_000_000.0
                / Stopwatch.Frequency);
        }
    }

    /// <summary>What the counts say, for a line somebody reads.</summary>
    /// <returns>One phrase, always naming how many callbacks were timed.</returns>
    public string Describe()
        => PeriodMicroseconds <= 0
            ? $"{_measured} callbacks timed, buffer period not read so no "
              + "overrun can be counted"
            : $"{_overPeriod} over the {PeriodMicroseconds:0} us period, "
              + $"{_overHalfPeriod} over half of it, in {_measured} timed";
}
