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
/// One mode's slot grid: how long a slot runs, and how long the transmission
/// inside it runs.
/// </summary>
/// <param name="SlotSeconds">How long one slot runs, boundary to boundary.</param>
/// <param name="TransmissionSeconds">How long the signal inside it occupies.</param>
/// <remarks>
/// <para>**THE TWO NUMBERS TRAVEL TOGETHER BECAUSE THEY ARE ONE FACT** (work
/// instruction 290 task 2). A slot length without its occupancy answers *did a
/// whole slot fit* and cannot answer *did a whole transmission fit*, and those are
/// the two questions a sidecar has already been caught printing different answers
/// to on consecutive lines. Handing them around as a pair is what stops a caller
/// pairing FT4's slot with FT8's transmission.</para>
/// <para>**THE ARITHMETIC IS IN TICKS, ANCHORED ON THE MINUTE.** Whole seconds
/// cannot hold a 7.5-second boundary at all - the old
/// <c>new DateTime(y, m, d, h, min, second)</c> had no field for the half, and
/// <c>(int)7.5</c> is <c>7</c>, which is a seven-second grid wearing a
/// 7.5-second grid's name. The minute is the anchor because it is the largest
/// unit both lengths divide exactly: four FT8 slots to the minute, eight FT4
/// slots, so no error accumulates across an hour or a day, and
/// <see cref="DateTime.Ticks"/> is exact for both.</para>
/// <para>**FT8'S NUMBERS COME OUT OF THIS SAME ARITHMETIC AND ARE UNCHANGED BY
/// IT.** <see cref="Ft8Slots"/> is a thin forwarder onto <see cref="Ft8"/>, which
/// is unit 289's own device for <c>Ft8WaterfallGeometry</c>: one implementation,
/// the mode's own constants handed in, so a divergence between the two grids
/// cannot be a second copy of the arithmetic drifting. If an FT8 boundary moves by
/// a tick that is a defect and not a rounding difference.</para>
/// <para>**IT IS NOT A SCHEDULE AND IT READS NO CLOCK.** Every function here is
/// pure over a corrected moment. Nothing decides when to transmit, and reaching a
/// boundary does nothing at all.</para>
/// </remarks>
public readonly record struct SlotGrid(double SlotSeconds, double TransmissionSeconds)
{
    /// <summary>FT8's grid: fifteen seconds, four to the minute, 12.64 s of tones.</summary>
    public static SlotGrid Ft8 { get; } =
        new(Ft8Slots.SlotSeconds, Ft8Slots.TransmissionSeconds);

    /// <summary>FT4's grid, read from the port and typed nowhere in this assembly.</summary>
    /// <remarks>
    /// <para>**BOTH NUMBERS COME FROM <c>Ft8Sharp.Ft4Timing</c>** and neither is
    /// written down here. Unit 289 put FT4's timing in one file precisely so that
    /// the open 4.48-against-5.04 question costs one edit to settle, and a copy in
    /// this assembly would make it cost two and let them disagree in between. The
    /// question is the owner's and nothing here settles it.</para>
    /// <para>The slot is 7.5 s on either figure, which is why the grid can be built
    /// while the occupancy is still open.</para>
    /// </remarks>
    public static SlotGrid Ft4 { get; } =
        new(Ft8Sharp.Ft4Timing.SlotSeconds, Ft8Sharp.Ft4Timing.OccupancySeconds);

    /// <summary>The slot length in ticks, which is what the arithmetic runs on.</summary>
    private long SlotTicks => (long)Math.Round(SlotSeconds * TimeSpan.TicksPerSecond);

    /// <summary>How many slots fall inside one minute of UTC.</summary>
    /// <remarks>
    /// Four for FT8 and eight for FT4. **Both are even**, which is what lets
    /// <c>Ft8Turn.ParityOf</c> keep working: the alternating halves do not swap over
    /// at a minute or an hour boundary on either grid.
    /// </remarks>
    public int SlotsPerMinute => (int)(TimeSpan.TicksPerMinute / SlotTicks);

    /// <summary>
    /// Whether a whole transmission fits after a boundary, given how much audio
    /// follows it.
    /// </summary>
    /// <param name="secondsAfterBoundary">Audio available after the boundary.</param>
    /// <returns>True where the whole transmission is inside the audio.</returns>
    /// <remarks>
    /// <para>**ONE FUNCTION, BECAUSE TWO ANSWERS DISAGREED IN CONSECUTIVE
    /// LINES.** On capture `ft8-2026-09-03-210644` the sidecar wrote
    /// `wholeSlots 1 ... whole transmission inside the audio` and, on the line
    /// under it, `refusal no whole slot fits inside the recording`. The sheet
    /// measured the 12.64 s a transmission occupies; the cutter required a full
    /// 15 s slot. Both were reasonable and they cannot both be printed.</para>
    /// <para>**AND THE OCCUPANCY IS THE RIGHT ONE**, because it is what the signal
    /// actually occupies. A boundary with 13 s of audio after it holds the whole
    /// FT8 transmission; refusing it discards a decodable slot for the sake of
    /// 2.36 s of silence that carries nothing.</para>
    /// <para>**TWO GRIDS MUST NOT BECOME TWO ANSWERS AGAIN.** It is still one
    /// function; what changed is that the number it measures against arrives with
    /// the grid rather than from a `const`.</para>
    /// <para>**THE EPSILON IS FOR ROUNDING AND NOTHING ELSE.** A boundary
    /// computed in ticks and a sample count computed by rounding disagree in the
    /// last decimal place, and a slot refused for a nanosecond is a slot refused
    /// for arithmetic.</para>
    /// </remarks>
    public bool TransmissionFits(double secondsAfterBoundary)
        => secondsAfterBoundary + 1e-6 >= TransmissionSeconds;

    /// <summary>The start of the slot a moment falls in.</summary>
    /// <param name="trueUtc">A corrected moment.</param>
    /// <returns>The boundary at or before it, always <see cref="DateTimeKind.Utc"/>.</returns>
    /// <remarks>
    /// **FLOOR ONTO THE GRID, IN TICKS, FROM THE TOP OF THE MINUTE.** On FT8 this
    /// is the quarter-minute and is the same value the whole-second arithmetic gave;
    /// on FT4 it is one of the eight boundaries at `:00`, `:07.5`, `:15`, `:22.5`,
    /// `:30`, `:37.5`, `:45` and `:52.5`, four of which no whole-second return type
    /// could have expressed.
    /// </remarks>
    public DateTime SlotStart(DateTime trueUtc)
    {
        var sinceMinute = trueUtc.Ticks % TimeSpan.TicksPerMinute;
        var slot = SlotTicks;

        return new DateTime(
            trueUtc.Ticks - sinceMinute + (sinceMinute / slot * slot),
            DateTimeKind.Utc);
    }

    /// <summary>How far into its slot a moment is.</summary>
    /// <param name="trueUtc">A corrected moment.</param>
    /// <returns>Seconds since the slot began, 0 up to <see cref="SlotSeconds"/>.</returns>
    public double IntoSlot(DateTime trueUtc)
        => (trueUtc - SlotStart(trueUtc)).TotalSeconds;

    /// <summary>
    /// Every slot boundary inside a stretch of time, oldest first.
    /// </summary>
    /// <param name="fromTrueUtc">The start of the stretch, corrected.</param>
    /// <param name="toTrueUtc">The end of it, corrected.</param>
    /// <returns>The boundaries, which may be empty.</returns>
    /// <remarks>
    /// <para>**THIS IS WHAT THE WATERFALL DRAWS ITS RULES FROM.** It is a list of
    /// moments rather than pixel positions, so the control decides where they
    /// land on the screen and this decides nothing about drawing.</para>
    /// <para>**IT STEPS IN TICKS RATHER THAN BY `AddSeconds`.** A half-second step
    /// added a thousand times in floating point is a boundary list that drifts off
    /// its own grid, and the drift would be invisible on FT8 and silent on FT4.</para>
    /// </remarks>
    public IReadOnlyList<DateTime> BoundariesBetween(
        DateTime fromTrueUtc, DateTime toTrueUtc)
    {
        if (toTrueUtc <= fromTrueUtc)
        {
            return Array.Empty<DateTime>();
        }

        var found = new List<DateTime>();
        var slot = SlotTicks;
        var at = SlotStart(fromTrueUtc);

        if (at < fromTrueUtc)
        {
            at = new DateTime(at.Ticks + slot, DateTimeKind.Utc);
        }

        while (at <= toTrueUtc)
        {
            found.Add(at);
            at = new DateTime(at.Ticks + slot, DateTimeKind.Utc);
        }

        return found;
    }

    /// <summary>What this grid is, in one phrase, for a capture to carry.</summary>
    /// <returns>A phrase such as <c>7.50 s slots, 5.04 s transmission</c>.</returns>
    /// <remarks>
    /// **A CAPTURE READ A YEAR FROM NOW SAYS WHAT IT WAS CUT ON** rather than
    /// leaving it to be inferred from the boundary spacing. Two decimal places on
    /// both, because 12.64 and 5.04 both need them and a grid printed as `8 s` is
    /// the same §0.0 exposure one rounding along.
    /// </remarks>
    public string Describe()
        => string.Format(
            System.Globalization.CultureInfo.InvariantCulture,
            "{0:0.00} s slots, {1:0.00} s transmission",
            SlotSeconds,
            TransmissionSeconds);
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

    /// <summary>How long the transmission inside a slot runs.</summary>
    /// <remarks>
    /// 12.64 seconds of tones inside a fifteen-second slot — 79 symbols at 0.16 s
    /// each. Named here because *a whole slot was cut* and *the whole transmission
    /// was captured* are different questions, and a capture that holds neither
    /// should be able to say which.
    /// </remarks>
    public const double TransmissionSeconds = 12.64;

    /// <summary>
    /// Whether a whole FT8 transmission fits after a boundary, given how much
    /// audio follows it.
    /// </summary>
    /// <param name="secondsAfterBoundary">Audio available after the boundary.</param>
    /// <returns>True where the whole transmission is inside the audio.</returns>
    /// <remarks>
    /// <para>**ONE FUNCTION, BECAUSE TWO ANSWERS DISAGREED IN CONSECUTIVE
    /// LINES.** On capture `ft8-2026-09-03-210644` the sidecar wrote
    /// `wholeSlots 1 ... whole transmission inside the audio` and, on the line
    /// under it, `refusal no whole slot fits inside the recording`. The sheet
    /// measured the 12.64 s a transmission occupies; the cutter required a full
    /// 15 s slot. Both were reasonable and they cannot both be printed.</para>
    /// <para>**AND 12.64 IS THE RIGHT ONE**, because it is what the signal
    /// actually occupies. A boundary with 13 s of audio after it holds the whole
    /// transmission; refusing it discards a decodable slot for the sake of 2.36 s
    /// of silence that carries nothing. The cutter pads the remainder rather than
    /// changing what `Ft8Sharp` wants - its waterfall asks for 93 blocks and
    /// zeros after the transmission are harmless to it, and changing `Ft8Sharp`
    /// is not this unit's to do.</para>
    /// <para>**THE EPSILON IS FOR ROUNDING AND NOTHING ELSE.** A boundary
    /// computed in ticks and a sample count computed by rounding disagree in the
    /// last decimal place, and a slot refused for a nanosecond is a slot refused
    /// for arithmetic.</para>
    /// </remarks>
    public static bool TransmissionFits(double secondsAfterBoundary)
        => SlotGrid.Ft8.TransmissionFits(secondsAfterBoundary);

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
    /// <remarks>
    /// **THE ARITHMETIC MOVED TO TICKS AND THIS ANSWER DID NOT** (work instruction
    /// 290 task 2). It used to floor the second onto a multiple of fifteen and
    /// rebuild the <see cref="DateTime"/> from whole seconds; it now floors the
    /// tick count onto a multiple of the slot from the top of the minute, which is
    /// the same value for every moment on a fifteen-second grid and is the only one
    /// of the two that can express a half-second boundary.
    /// </remarks>
    public static DateTime SlotStart(DateTime trueUtc)
        => SlotGrid.Ft8.SlotStart(trueUtc);

    /// <summary>How far into its slot a moment is.</summary>
    /// <param name="trueUtc">A corrected moment.</param>
    /// <returns>Seconds since the slot began, 0 to 15.</returns>
    public static double IntoSlot(DateTime trueUtc)
        => SlotGrid.Ft8.IntoSlot(trueUtc);

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
        => SlotGrid.Ft8.BoundariesBetween(fromTrueUtc, toTrueUtc);
}
