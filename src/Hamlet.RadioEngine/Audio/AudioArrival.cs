namespace Hamlet.RadioEngine.Audio;

/// <summary>
/// What the audio path actually delivered, as counts, for the surfaces the
/// operator reads.
/// </summary>
/// <param name="RecentRatio">
/// Samples delivered over the last fifteen seconds divided by the samples a
/// continuous stream would have delivered in the same wall clock. NaN where
/// there is not enough history to say.
/// </param>
/// <param name="SlotRatio">
/// The same fraction across one slot's own wall-clock span, or NaN.
/// </param>
/// <param name="QueueDroppedChunks">Chunks the decode queue could not hold.</param>
/// <param name="QueueDroppedSamples">Samples those chunks carried.</param>
/// <param name="CallbackFailures">Device callbacks that threw and were counted.</param>
/// <param name="EmptyBuffers">Device buffers that downmixed to no samples.</param>
/// <param name="LongestCallbackMicroseconds">
/// The longest a single device callback has taken.
/// </param>
/// <param name="DeliveredSamples">Samples the device has delivered in total.</param>
/// <param name="BufferPeriodMicroseconds">
/// The device buffer period every callback is measured against, or zero where
/// it was not read.
/// </param>
/// <param name="CallbacksOverPeriod">
/// Callbacks that ran longer than the whole period, so the device filled the
/// next buffer while they were still working.
/// </param>
/// <param name="CallbacksOverHalfPeriod">
/// Callbacks that ran longer than half the period. It is the number that rises
/// first, while a machine is close to the edge and still working.
/// </param>
/// <param name="CallbacksTimed">
/// How many callbacks were timed at all, so a zero overrun count can be told
/// from a path nothing has run down yet.
/// </param>
/// <remarks>
/// <para>**EVERY FIELD IS A COUNT OR A COUNT OVER A COUNT** (`CLAUDE.md` §0.0).
/// There is no quality figure here and no signal-to-noise ratio. A ratio says
/// this many samples arrived while that much time passed, and nothing about the
/// band.</para>
/// <para>**NaN IS NOBODY MEASURED AND IS NOT ZERO.** A watch that has just
/// started has no history reaching back a slot, and a zero there would read as
/// *the sound card delivered nothing* about a device that is working.</para>
/// <para>**WHY IT EXISTS.** On 2026-09-03 the tap filled at 13% of real time for
/// an entire evening and not one of the three surfaces the operator reads could
/// say so — the slot telemetry, the capture sidecar and the census line all
/// described the decode, so a starved sound card and an empty band wrote
/// identical output. That is HM-DEC-093 exactly: the path was uncounted, so
/// nothing could tell the two apart.</para>
/// </remarks>
public readonly record struct AudioArrival(
    double RecentRatio,
    double SlotRatio,
    long QueueDroppedChunks,
    long QueueDroppedSamples,
    long CallbackFailures,
    long EmptyBuffers,
    double LongestCallbackMicroseconds,
    long DeliveredSamples,
    double BufferPeriodMicroseconds = 0,
    long CallbacksOverPeriod = 0,
    long CallbacksOverHalfPeriod = 0,
    long CallbacksTimed = 0)
{
    /// <summary>Nothing measured.</summary>
    public static AudioArrival None { get; } = new(
        double.NaN, double.NaN, 0, 0, 0, 0, 0, 0);

    /// <summary>What the callback budget says, for a line somebody reads.</summary>
    /// <remarks>
    /// **IT NAMES HOW MANY WERE TIMED, ALWAYS.** No overruns in a million
    /// callbacks and no overruns because nothing has run are the same number and
    /// opposite facts (HM-DEC-093).
    /// </remarks>
    public string CallbackBudgetText
        => BufferPeriodMicroseconds <= 0
            ? CallbacksTimed + " callbacks timed, buffer period not read so no "
              + "overrun can be counted"
            : string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "{0} over the {1:0} us buffer period, {2} over half of it, "
                + "in {3} timed",
                CallbacksOverPeriod,
                BufferPeriodMicroseconds,
                CallbacksOverHalfPeriod,
                CallbacksTimed);

    /// <summary>The recent ratio as a percentage, or "not measured".</summary>
    public string RecentText => Describe(RecentRatio);

    /// <summary>The slot's ratio as a percentage, or "not measured".</summary>
    public string SlotText => Describe(SlotRatio);

    /// <summary>True where a ratio was measured and fell short.</summary>
    /// <param name="least">The threshold to compare against.</param>
    /// <returns>False when nothing was measured, which is never a fault.</returns>
    public bool FellShort(double least)
        => !double.IsNaN(RecentRatio) && RecentRatio < least;

    private static string Describe(double ratio)
        => double.IsNaN(ratio)
            ? "not measured"
            : ratio.ToString("P0", System.Globalization.CultureInfo.InvariantCulture);
}
