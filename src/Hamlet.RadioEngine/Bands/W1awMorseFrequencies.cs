using System.Globalization;
using System.Reflection;
using System.Text.Json;

namespace Hamlet.RadioEngine.Bands;

/// <summary>One scheduled W1AW Morse run on one day, in the schedule's own time zone (work instruction 497).</summary>
/// <param name="Day">The day it runs.</param>
/// <param name="From">When it starts, on that day, in the schedule's zone.</param>
/// <param name="To">When it ends.</param>
/// <param name="Kind">"code practice" or "code bulletin", as the file writes it.</param>
/// <param name="Speed">"fast", "slow" or "18 WPM", as the file writes it for that day.</param>
public sealed record W1awRun(DayOfWeek Day, TimeSpan From, TimeSpan To, string Kind, string Speed);

/// <summary>Whether a run is scheduled now, and which run is scheduled now or next (work instruction 497).</summary>
/// <param name="Scheduled">True while a run is scheduled.</param>
/// <param name="Run">The run scheduled now, or the next one.</param>
/// <param name="StartUtc">When it starts, in UTC.</param>
/// <param name="EndUtc">When it ends, in UTC.</param>
public sealed record W1awScheduleState(bool Scheduled, W1awRun Run, DateTime StartUtc, DateTime EndUtc);

/// <summary>One of W1AW's Morse code frequencies.</summary>
/// <param name="Band">The band as Hamlet spells it, e.g. "40 m".</param>
/// <param name="FrequencyHz">The frequency W1AW sends Morse on there, in hertz.</param>
public sealed record W1awMorseRow(string Band, long FrequencyHz);

/// <summary>
/// **Where W1AW sends Morse, read from <c>data/bands/w1aw-morse.json</c>** (work instruction 494,
/// HM-DEC-199).
/// </summary>
/// <remarks>
/// <para>**THE ARRL'S OWN SCHEDULE IS THE SOURCE**, cited in the file, and there is no frequency
/// literal in this class: a correction is one edit to the file (§0).</para>
/// <para>**FREQUENCIES AND NOTHING ABOUT WHEN.** W1AW's times are US Central, the same all year,
/// so they move against UTC with daylight saving; the file carries no times and nothing here
/// computes one, because a wrong time on screen is a false sentence (§0.0).</para>
/// <para>**EVERY ROW THE ARRL GIVES**, 2 m included. Which of them a surface offers is the
/// surface's to decide, from what the radio can tune and what Hamlet knows of the spectrum.</para>
/// </remarks>
public sealed class W1awMorseFrequencies
{
    /// <summary>The embedded resource the file is built into.</summary>
    public const string ResourceName = "Hamlet.RadioEngine.Data.Bands.w1aw-morse.json";

    private static readonly Lazy<W1awMorseFrequencies> Shipped = new(Load);

    private W1awMorseFrequencies(
        string about,
        string source,
        string speeds,
        IReadOnlyList<W1awMorseRow> rows,
        string timeZoneId,
        IReadOnlyList<W1awRun> schedule)
    {
        About = about;
        Source = source;
        Speeds = speeds;
        Rows = rows;
        TimeZoneId = timeZoneId;
        Schedule = schedule;
    }

    /// <summary>The time zone the schedule is kept in, as the file names it: "America/Chicago".</summary>
    public string TimeZoneId { get; }

    /// <summary>
    /// Every Morse run the schedule gives, one per day it runs, in the schedule's own time zone
    /// (work instruction 497).
    /// </summary>
    public IReadOnlyList<W1awRun> Schedule { get; }

    /// <summary>
    /// Whether a Morse run is scheduled at a moment, and which run is scheduled now or next (work
    /// instruction 497).
    /// </summary>
    /// <param name="utcNow">The moment, in UTC.</param>
    /// <returns>The run scheduled now, or the next one, with its times in UTC.</returns>
    /// <remarks>
    /// **THROUGH THE TIME ZONE DATABASE, NEVER A FIXED OFFSET.** The schedule is in US Central,
    /// which moves against UTC with daylight saving, so each run's start and end are converted from
    /// the named zone on its own date; an offset baked in would be an hour wrong half the year
    /// (§0.0). **Scheduled is not transmitting**: the ARRL leaves out legal holidays, which Hamlet
    /// does not know.
    /// </remarks>
    public W1awScheduleState At(DateTime utcNow)
    {
        var zone = FindZone(TimeZoneId);
        var here = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc), zone);
        W1awScheduleState? next = null;

        for (var day = -1; day <= 8; day++)
        {
            var date = here.Date.AddDays(day);

            foreach (var run in Schedule.Where(r => r.Day == date.DayOfWeek))
            {
                var start = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(date + run.From, DateTimeKind.Unspecified), zone);
                var end = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(date + run.To, DateTimeKind.Unspecified), zone);

                if (start <= utcNow && utcNow < end)
                {
                    return new W1awScheduleState(true, run, start, end);
                }

                if (start > utcNow && (next is null || start < next.StartUtc))
                {
                    next = new W1awScheduleState(false, run, start, end);
                }
            }
        }

        return next ?? throw new InvalidOperationException("W1AW's schedule has no Morse run in the coming week");
    }

    /// <summary>
    /// The run scheduled now, or the last one that started before now: what a score taken after a
    /// bulletin is a score of (work instruction numbered 509, run as unit 510, task 3).
    /// </summary>
    /// <param name="utcNow">The moment, in UTC.</param>
    /// <returns>The run, with its times in UTC, or null where none started in the week before.</returns>
    public W1awScheduleState? Latest(DateTime utcNow)
    {
        var zone = FindZone(TimeZoneId);
        var here = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc), zone);
        W1awScheduleState? latest = null;

        for (var day = -8; day <= 1; day++)
        {
            var date = here.Date.AddDays(day);

            foreach (var run in Schedule.Where(r => r.Day == date.DayOfWeek))
            {
                var start = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(date + run.From, DateTimeKind.Unspecified), zone);
                var end = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(date + run.To, DateTimeKind.Unspecified), zone);

                if (start <= utcNow && (latest is null || start > latest.StartUtc))
                {
                    latest = new W1awScheduleState(utcNow < end, run, start, end);
                }
            }
        }

        return latest;
    }

    /// <summary>A time zone by its IANA name, through the Windows name where the system needs it.</summary>
    /// <param name="id">"America/Chicago".</param>
    /// <returns>The zone.</returns>
    public static TimeZoneInfo FindZone(string id)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (TimeZoneNotFoundException) when (TimeZoneInfo.TryConvertIanaIdToWindowsId(id, out var windows))
        {
            return TimeZoneInfo.FindSystemTimeZoneById(windows);
        }
    }

    /// <summary>The table the build carries.</summary>
    public static W1awMorseFrequencies Default => Shipped.Value;

    /// <summary>What the file says it is, in its own words.</summary>
    public string About { get; }

    /// <summary>Where the rows were read from, as the file states it.</summary>
    public string Source { get; }

    /// <summary>What W1AW sends and how fast, as the file states it.</summary>
    public string Speeds { get; }

    /// <summary>Every row, lowest band first, as the file gives them.</summary>
    public IReadOnlyList<W1awMorseRow> Rows { get; }

    /// <summary>
    /// The amateur band a frequency is in, named as Hamlet names it, or null where it is in none the
    /// privileges data carries (work instruction 495).
    /// </summary>
    /// <param name="frequencyHz">A frequency.</param>
    /// <returns>"40 m", or null.</returns>
    /// <remarks>
    /// Read from the Extra class's allocations in the privileges data, the widest there are, so every
    /// amateur band is named whether Hamlet maps it or not. The regulation's 75 m is the top of 80 m,
    /// joined as <see cref="HfBands"/> joins them.
    /// </remarks>
    public static string? AmateurBandFor(long frequencyHz)
    {
        if (!Licensing.PrivilegeData.Current.ClassBands.TryGetValue(Licensing.LicenseClass.Extra, out var ranges))
        {
            return null;
        }

        foreach (var range in ranges)
        {
            if (frequencyHz >= range.LowHz && frequencyHz <= range.HighHz)
            {
                return range.Band == "75 m" ? "80 m" : range.Band;
            }
        }

        return null;
    }

    /// <summary>W1AW's row for the band a frequency is in, or null where it has none there.</summary>
    /// <param name="frequencyHz">Where the dial is.</param>
    /// <returns>The row, or null.</returns>
    public W1awMorseRow? RowForBandOf(long frequencyHz)
        => AmateurBandFor(frequencyHz) is { } band
            ? Rows.FirstOrDefault(r => string.Equals(r.Band, band, StringComparison.Ordinal))
            : null;

    /// <summary>Parse the table from the file's text.</summary>
    /// <param name="json">The file's contents.</param>
    /// <returns>The table.</returns>
    /// <exception cref="InvalidDataException">The file is not what it should be, in words.</exception>
    /// <remarks>Strict: a row skipped is a band silently without a button.</remarks>
    public static W1awMorseFrequencies Parse(string json)
    {
        JsonDocument document;

        try
        {
            document = JsonDocument.Parse(json);
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("it is not readable JSON (line " + (error.LineNumber + 1) + ")", error);
        }

        using (document)
        {
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidDataException("it does not hold a table");
            }

            var source = Text(root, "source");

            if (string.IsNullOrWhiteSpace(source))
            {
                throw new InvalidDataException("it does not say where its rows came from");
            }

            var about = Text(root, "_about");

            if (string.IsNullOrWhiteSpace(about))
            {
                throw new InvalidDataException("it does not say what kind of table it is");
            }

            if (!root.TryGetProperty("rows", out var rowsElement) || rowsElement.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidDataException("it carries no rows");
            }

            var rows = new List<W1awMorseRow>();

            foreach (var element in rowsElement.EnumerateArray())
            {
                var band = element.ValueKind == JsonValueKind.Object ? Text(element, "band") : "";

                if (string.IsNullOrWhiteSpace(band))
                {
                    throw new InvalidDataException("row " + (rows.Count + 1) + " names no band");
                }

                if (!element.TryGetProperty("hz", out var hzElement)
                    || hzElement.ValueKind != JsonValueKind.Number
                    || !hzElement.TryGetInt64(out var hz)
                    || hz <= 0)
                {
                    throw new InvalidDataException("the " + band + " row's frequency is missing or not a whole number of hertz");
                }

                rows.Add(new W1awMorseRow(band, hz));
            }

            if (rows.Count == 0)
            {
                throw new InvalidDataException("it carries no rows");
            }

            var zone = Text(root, "timeZone");

            if (string.IsNullOrWhiteSpace(zone))
            {
                throw new InvalidDataException("it does not name the time zone its schedule is kept in");
            }

            var schedule = new List<W1awRun>();

            if (root.TryGetProperty("schedule", out var scheduleElement) && scheduleElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in scheduleElement.EnumerateArray())
                {
                    var kind = element.ValueKind == JsonValueKind.Object ? Text(element, "kind") : "";

                    if (string.IsNullOrWhiteSpace(kind)
                        || !TimeSpan.TryParse(Text(element, "from"), CultureInfo.InvariantCulture, out var from)
                        || !TimeSpan.TryParse(Text(element, "to"), CultureInfo.InvariantCulture, out var to)
                        || to <= from
                        || !element.TryGetProperty("days", out var days)
                        || days.ValueKind != JsonValueKind.Object)
                    {
                        throw new InvalidDataException("schedule row " + (schedule.Count + 1) + " is not a kind, a from, a to and its days");
                    }

                    foreach (var day in days.EnumerateObject())
                    {
                        if (!DayNames.TryGetValue(day.Name, out var dayOfWeek)
                            || day.Value.ValueKind != JsonValueKind.String
                            || string.IsNullOrWhiteSpace(day.Value.GetString()))
                        {
                            throw new InvalidDataException("the " + kind + " row at " + Text(element, "from") + " has a day that is not a day and its speed");
                        }

                        schedule.Add(new W1awRun(dayOfWeek, from, to, kind, day.Value.GetString()!));
                    }
                }
            }

            return new W1awMorseFrequencies(about, source, Text(root, "speeds"), rows, zone, schedule);
        }
    }

    private static W1awMorseFrequencies Load()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException(
                "W1AW's frequencies are missing: the build did not include data/bands/w1aw-morse.json");
        using var reader = new StreamReader(stream);

        return Parse(reader.ReadToEnd());
    }

    private static readonly IReadOnlyDictionary<string, DayOfWeek> DayNames = new Dictionary<string, DayOfWeek>(StringComparer.Ordinal)
    {
        ["Sun"] = DayOfWeek.Sunday,
        ["Mon"] = DayOfWeek.Monday,
        ["Tue"] = DayOfWeek.Tuesday,
        ["Wed"] = DayOfWeek.Wednesday,
        ["Thu"] = DayOfWeek.Thursday,
        ["Fri"] = DayOfWeek.Friday,
        ["Sat"] = DayOfWeek.Saturday,
    };

    private static string Text(JsonElement element, string name)
        => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? ""
            : "";
}
