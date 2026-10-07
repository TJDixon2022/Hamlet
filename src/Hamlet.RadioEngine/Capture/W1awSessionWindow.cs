using System.Globalization;
using Hamlet.RadioEngine.Bands;

namespace Hamlet.RadioEngine.Capture;

/// <summary>
/// **WHEN AND WHERE A W1AW SESSION IS CAPTURED** (work instruction 549, task 1): from a minute before a scheduled run starts
/// to two minutes after it is scheduled to end, on one of W1AW's Morse frequencies.
/// </summary>
/// <remarks>
/// <para>**THE SCHEDULE AND THE FREQUENCIES ARE THE FILE'S** (<c>data/bands/w1aw-morse.json</c>, through
/// <see cref="W1awMorseFrequencies"/>); nothing here carries a time or a frequency of its own (§0).</para>
/// <para>**A MINUTE BEFORE AND TWO AFTER**, the owner's figures in the work instruction: the ARRL starts on the minute by
/// its clock and Hamlet's may differ, and a bulletin that runs over is still the bulletin.</para>
/// <para>**TWO RUNS BACK TO BACK** (15:00 code practice, then 16:00 bulletin) overlap by three minutes once widened. The later
/// one wins from the moment its window opens, so each capture holds one schedule entry and its sheets name one.</para>
/// </remarks>
public static class W1awSessionWindow
{
    /// <summary>How long before a run's scheduled start its capture begins.</summary>
    public static readonly TimeSpan Before = TimeSpan.FromMinutes(1);

    /// <summary>How long after a run's scheduled end its capture runs on.</summary>
    public static readonly TimeSpan After = TimeSpan.FromMinutes(2);

    /// <summary>
    /// How far the dial may sit from W1AW's frequency and still be on it, where the radio's filter is unread: 250 Hz.
    /// </summary>
    /// <remarks>
    /// **THE W1AW BUTTON HAS NO TOLERANCE**: it tunes to the frequency exactly, and the order's "tolerance the W1AW button
    /// already uses" names none. So the figure is the one that decides whether W1AW can be heard at all: its tone must stay
    /// inside the radio's filter, which it does while the dial is within half the filter's width of its frequency. Where the
    /// width is unread, half of 500 Hz, the narrowest filter the CW tab is normally set to.
    /// </remarks>
    public const double UnreadToleranceHz = 250;

    /// <summary>The run whose capture window holds a moment, with its times, or null.</summary>
    /// <param name="table">W1AW's schedule.</param>
    /// <param name="utcNow">The moment.</param>
    /// <returns>The run, or null where no window is open.</returns>
    public static W1awScheduleState? At(W1awMorseFrequencies table, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(table);

        // The later run first: one whose window has opened (its start a minute or less away) outranks one still in its tail.
        var opening = table.At(utcNow + Before);

        if (opening.Scheduled)
        {
            return opening with { Scheduled = true };
        }

        var latest = table.Latest(utcNow);

        return latest is not null && utcNow < latest.EndUtc + After
            ? latest with { Scheduled = true }
            : null;
    }

    /// <summary>How far the dial may be from W1AW's frequency: half the filter, or <see cref="UnreadToleranceHz"/>.</summary>
    /// <param name="filterWidthHz">The radio's filter width, or null where unread.</param>
    /// <returns>Hertz.</returns>
    public static double ToleranceHz(double? filterWidthHz)
        => filterWidthHz is > 0 ? filterWidthHz.Value / 2 : UnreadToleranceHz;

    /// <summary>W1AW's Morse frequency the dial is on, or null.</summary>
    /// <param name="table">W1AW's frequencies.</param>
    /// <param name="dialHz">Where the dial is.</param>
    /// <param name="filterWidthHz">The radio's filter width, or null where unread.</param>
    /// <returns>The row, or null.</returns>
    public static W1awMorseRow? RowAt(W1awMorseFrequencies table, long dialHz, double? filterWidthHz)
    {
        ArgumentNullException.ThrowIfNull(table);

        var tolerance = ToleranceHz(filterWidthHz);

        return table.Rows.FirstOrDefault(r => Math.Abs(r.FrequencyHz - dialHz) <= tolerance);
    }

    /// <summary>The schedule entry, as a sheet writes it.</summary>
    /// <param name="table">W1AW's schedule, for its zone.</param>
    /// <param name="run">The run.</param>
    /// <returns>"code bulletin · 18 WPM · scheduled 2026-10-07 20:00 to 21:00 UTC (15:00 to 16:00 America/Chicago)".</returns>
    public static string Entry(W1awMorseFrequencies table, W1awScheduleState run)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(run);

        return string.Format(
            CultureInfo.InvariantCulture,
            "{0} · {1} · scheduled {2:yyyy-MM-dd HH:mm} to {3:HH:mm} UTC ({4:hh\\:mm} to {5:hh\\:mm} {6})",
            run.Run.Kind,
            run.Run.Speed,
            run.StartUtc,
            run.EndUtc,
            run.Run.From,
            run.Run.To,
            table.TimeZoneId);
    }
}
