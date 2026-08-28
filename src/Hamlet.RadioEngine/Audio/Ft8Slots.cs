namespace Hamlet.RadioEngine.Audio;

/// <summary>
/// How far the PC clock is from UTC, and how old that measurement is.
/// </summary>
/// <param name="OffsetSeconds">
/// What must be added to the PC clock to get UTC. Positive means the PC is slow.
/// </param>
/// <param name="MeasuredAtUtc">When the query answered, or null if it never has.</param>
/// <remarks>
/// <para>**MEASURED AND DISPLAYED, NEVER CORRECTED** (Tim's ruling of
/// 2026-08-28). Nothing in Hamlet adjusts a clock. Two notions of time in one
/// application produce sidecar timestamps that disagree with the file's own
/// modification time, and a silently disciplined clock produces trimmed files
/// that are quietly wrong.</para>
/// <para>**UNKNOWN IS A REAL STATE AND IS NEVER ZERO** (HM-DEC-009). A drifted
/// clock nobody has measured and a clock measured at no drift are different
/// facts, and the second one is a measurement.</para>
/// </remarks>
public readonly record struct ClockOffset(
    double? OffsetSeconds, DateTime? MeasuredAtUtc)
{
    /// <summary>Nothing has been measured.</summary>
    public static ClockOffset Unknown { get; } = new(null, null);

    /// <summary>True once a query has answered.</summary>
    public bool IsKnown => OffsetSeconds is not null && MeasuredAtUtc is not null;

    /// <summary>
    /// Past this, the offset is shown in amber.
    /// </summary>
    /// <remarks>
    /// <para>**HALF A SECOND, AND THE NUMBER COMES FROM THE MODE.** FT8 packs
    /// 12.64 seconds of transmission into a 15-second slot, so a receiver has
    /// roughly 2.3 seconds of slack in total and WSJT-X's own decoders work
    /// comfortably inside about a second of error.</para>
    /// <para>Half of that is the point at which the operator should be told
    /// something is wrong while there is still margin left, rather than at the
    /// point where decodes have already started failing.</para>
    /// </remarks>
    public const double AmberPastSeconds = 0.5;

    /// <summary>
    /// Past this, a measurement is too old to rely on.
    /// </summary>
    /// <remarks>
    /// An hour. A PC clock that has been disciplined by the operating system
    /// does not wander far in an hour, and a figure older than that is a fact
    /// about a previous session.
    /// </remarks>
    public const double StaleAfterSeconds = 3600;

    /// <summary>Whether the offset is far enough out to warn about.</summary>
    /// <param name="offset">The measurement.</param>
    /// <returns>True when it is known and past the threshold.</returns>
    public static bool IsConcerning(ClockOffset offset)
        => offset.OffsetSeconds is { } seconds
           && Math.Abs(seconds) >= AmberPastSeconds;

    /// <summary>How old the measurement is.</summary>
    /// <param name="nowUtc">The moment being asked about.</param>
    /// <returns>The age, or null when nothing has been measured.</returns>
    public TimeSpan? Age(DateTime nowUtc)
        => MeasuredAtUtc is { } at ? nowUtc - at : null;

    /// <summary>Whether the measurement is too old to rely on.</summary>
    /// <param name="nowUtc">The moment being asked about.</param>
    /// <returns>True when it is known and older than the threshold.</returns>
    public bool IsStale(DateTime nowUtc)
        => Age(nowUtc) is { } age && age.TotalSeconds >= StaleAfterSeconds;

    /// <summary>What the operator is told, in words.</summary>
    /// <param name="nowUtc">The moment being asked about.</param>
    /// <returns>One line.</returns>
    /// <remarks>
    /// **IT SAYS UNKNOWN RATHER THAN IMPLYING ZERO** (§0.0). A clock nobody has
    /// checked is not a clock that is right, and the slot grid refuses to draw
    /// on that answer rather than guessing where the boundaries are.
    /// </remarks>
    public string Describe(DateTime nowUtc)
    {
        if (OffsetSeconds is not { } seconds || MeasuredAtUtc is null)
        {
            return "clock not checked yet, so slots cannot be cut";
        }

        var age = Age(nowUtc) ?? TimeSpan.Zero;

        var howLongAgo = age.TotalSeconds < 90
            ? "just now"
            : age.TotalMinutes < 90
                ? $"{age.TotalMinutes:0} minutes ago"
                : $"{age.TotalHours:0} hours ago";

        var sign = seconds >= 0 ? "slow" : "fast";
        var size = Math.Abs(seconds);

        var howFar = size < 0.05
            ? "clock matches UTC"
            : $"clock is {size:0.00} s {sign}";

        return $"{howFar}, checked {howLongAgo}";
    }
}

/// <summary>
/// Where the fifteen-second FT8 slots fall, given a measured clock offset.
/// </summary>
/// <remarks>
/// <para>**THE GRID IS WHAT MAKES FT8 RECOGNISABLE** (work instruction 038):
/// signals start and stop on the lines, and anything that ignores them is not
/// FT8. That is also why it may not be drawn on a guess — **a grid at a guessed
/// boundary is HM-DEC-009 broken in the one place nobody would check it**,
/// because a picture whose lines are wrong still looks like a picture whose
/// lines are right.</para>
/// <para>Pure: an offset and a moment in, a boundary out. No clock is read here,
/// so every threshold and every edge is testable without one.</para>
/// </remarks>
public static class Ft8Slots
{
    /// <summary>How long one FT8 slot runs.</summary>
    /// <remarks>
    /// Fifteen seconds, four to the minute, on the quarter-minutes of UTC. The
    /// transmission inside it is 12.64 s of tones.
    /// </remarks>
    public const double SlotSeconds = 15;

    /// <summary>True UTC, from a PC time and a measured offset.</summary>
    /// <param name="pcUtc">What the machine believes.</param>
    /// <param name="offset">The measurement, which may be unknown.</param>
    /// <returns>The corrected moment, or null when the offset is unknown.</returns>
    /// <remarks>
    /// **THIS CORRECTS A READING, NOT A CLOCK.** Nothing is written anywhere;
    /// the machine's clock is left exactly as it is, and this is the arithmetic
    /// that says where a boundary falls in terms of it.
    /// </remarks>
    public static DateTime? TrueUtc(DateTime pcUtc, ClockOffset offset)
        => offset.OffsetSeconds is { } seconds
            ? pcUtc.AddSeconds(seconds)
            : null;

    /// <summary>The start of the slot a moment falls in.</summary>
    /// <param name="trueUtc">A corrected moment.</param>
    /// <returns>The quarter-minute boundary at or before it.</returns>
    public static DateTime SlotStart(DateTime trueUtc)
    {
        var second = (trueUtc.Second / (int)SlotSeconds) * (int)SlotSeconds;

        return new DateTime(
            trueUtc.Year, trueUtc.Month, trueUtc.Day,
            trueUtc.Hour, trueUtc.Minute, second,
            DateTimeKind.Utc);
    }

    /// <summary>How far into its slot a moment is.</summary>
    /// <param name="trueUtc">A corrected moment.</param>
    /// <returns>Seconds since the slot began, 0 to 15.</returns>
    public static double IntoSlot(DateTime trueUtc)
        => (trueUtc - SlotStart(trueUtc)).TotalSeconds;

    /// <summary>
    /// Every slot boundary inside a stretch of time, oldest first.
    /// </summary>
    /// <param name="fromTrueUtc">The start of the stretch, corrected.</param>
    /// <param name="toTrueUtc">The end of it, corrected.</param>
    /// <returns>The boundaries, which may be empty.</returns>
    /// <remarks>
    /// **THIS IS WHAT THE WATERFALL DRAWS ITS RULES FROM.** It is a list of
    /// moments rather than pixel positions, so the control decides where they
    /// land on the screen and this decides nothing about drawing.
    /// </remarks>
    public static IReadOnlyList<DateTime> BoundariesBetween(
        DateTime fromTrueUtc, DateTime toTrueUtc)
    {
        if (toTrueUtc <= fromTrueUtc)
        {
            return Array.Empty<DateTime>();
        }

        var found = new List<DateTime>();
        var at = SlotStart(fromTrueUtc);

        if (at < fromTrueUtc)
        {
            at = at.AddSeconds(SlotSeconds);
        }

        while (at <= toTrueUtc)
        {
            found.Add(at);
            at = at.AddSeconds(SlotSeconds);
        }

        return found;
    }
}
