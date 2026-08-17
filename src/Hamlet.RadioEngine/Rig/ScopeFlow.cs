namespace Hamlet.RadioEngine.Rig;

/// <summary>
/// Which stage of the scope path is wrong, or none of them (HM-DEC-093).
/// </summary>
public enum ScopeStage
{
    /// <summary>Every stage is doing its job.</summary>
    Flowing,

    /// <summary>There is no scope stream attached.</summary>
    NotAttached,

    /// <summary>Nothing has ever come down the cable.</summary>
    NothingEverArrived,

    /// <summary>Parts arrive and none of them can be read.</summary>
    NothingRead,

    /// <summary>Parts are read and no complete sweep comes out.</summary>
    NoSweepCompletes,

    /// <summary>It was working and it has stopped.</summary>
    Stopped,
}

/// <summary>
/// What the scope path has actually done, stage by stage (HM-DEC-093).
/// </summary>
/// <remarks>
/// <para>**AN EMPTY WATERFALL IS A CLAIM AND BLACK IS NOT A STATE**
/// (HM-DEC-092). "Receiving frames and the band is quiet" and "no frame has ever
/// arrived" paint exactly the same picture and are completely different facts.
/// This feature was reported working three times while the second was true, and
/// nothing on screen or in the log could have told them apart.</para>
/// <para>**AND THE ROW SAYS NOTHING WHILE EVERYTHING WORKS.** It read the same
/// three numbers permanently once the waterfall started working, which is
/// furniture: a line that never changes is a line people stop reading, and that
/// is exactly the line that has to be read on the evening a stage goes to zero.
/// The counters do not go with it. They are what proved the path was discarding
/// 2,740 parts, so they stay one hover away.</para>
/// <para>Pure: counters in, a verdict out, with the clock passed rather than
/// read, so the same numbers give the same answer every run (§5).</para>
/// </remarks>
public static class ScopeFlow
{
    /// <summary>
    /// How long the scope may go quiet before that is worth saying.
    /// </summary>
    /// <remarks>
    /// Three seconds. The radio delivers about four and a half sweeps a second,
    /// so this is a dozen missed ones: comfortably past a hiccup and well short
    /// of the operator wondering why nothing is moving.
    /// </remarks>
    public static readonly TimeSpan QuietAfter = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Which stage is wrong.
    /// </summary>
    /// <param name="attached">Whether there is a scope stream at all.</param>
    /// <param name="received">Parts that came off the wire.</param>
    /// <param name="parsed">Parts Hamlet could read.</param>
    /// <param name="sweeps">Complete sweeps handed to the drawing.</param>
    /// <param name="lastPartUtc">When the last part arrived, or null.</param>
    /// <param name="nowUtc">The time now.</param>
    /// <returns>The first stage that is wrong, or <see cref="ScopeStage.Flowing"/>.</returns>
    /// <remarks>
    /// **THE FIRST ZERO IS THE ADDRESS OF THE FAULT**, so the order these are
    /// tested in is the order the data travels.
    /// </remarks>
    public static ScopeStage Check(
        bool attached, long received, long parsed, long sweeps,
        DateTime? lastPartUtc, DateTime nowUtc)
    {
        if (!attached)
        {
            return ScopeStage.NotAttached;
        }

        if (received == 0)
        {
            return ScopeStage.NothingEverArrived;
        }

        if (parsed == 0)
        {
            return ScopeStage.NothingRead;
        }

        if (sweeps == 0)
        {
            return ScopeStage.NoSweepCompletes;
        }

        return lastPartUtc is { } last && nowUtc - last > QuietAfter
            ? ScopeStage.Stopped
            : ScopeStage.Flowing;
    }

    /// <summary>
    /// What to say about it, or empty while every stage works.
    /// </summary>
    /// <param name="stage">What <see cref="Check"/> found.</param>
    /// <param name="received">Parts that came off the wire.</param>
    /// <param name="parsed">Parts Hamlet could read.</param>
    /// <param name="quietSeconds">How long since the last part.</param>
    /// <returns>The sentence, or an empty string.</returns>
    public static string Say(
        ScopeStage stage, long received, long parsed, int quietSeconds)
        => stage switch
        {
            ScopeStage.NothingEverArrived
                => "No spectrum data has ever arrived from the radio. This is not "
                   + "a quiet band: nothing at all has come down the cable since "
                   + "Hamlet connected.",

            ScopeStage.NothingRead
                => $"{received} parts have arrived from the radio and Hamlet could "
                   + "not read any of them, so the waterfall is empty because of "
                   + "Hamlet rather than because of the band.",

            ScopeStage.NoSweepCompletes
                => $"{received} parts in and {parsed} read, and not one complete "
                   + "sweep has come out of them. A sweep arrives in eleven parts "
                   + "and Hamlet drops any that is missing one, so something is "
                   + "going astray between them.",

            ScopeStage.Stopped
                => $"Nothing has arrived for {quietSeconds} seconds. The picture "
                   + "below is the last thing the radio sent rather than what is "
                   + "on the band now.",

            _ => "",
        };
}
