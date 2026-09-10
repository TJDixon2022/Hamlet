using System.Globalization;

namespace Hamlet.RadioEngine.Telemetry;

/// <summary>
/// **One picture of what state this machine was in, written once per session.**
/// </summary>
/// <remarks>
/// <para>**TIM'S RULING, 2026-09-10**: *"I want you to collect the information
/// necessary in the telemetry. I'm not typing in a bunch of commands for you. The idea
/// is telemetry should be able to diagnose any issue."*</para>
/// <para>**THE THREE FAILURES OF THAT DAY WERE STATES NOBODY COULD SEE, NOT EVENTS
/// NOBODY LOGGED.** The application writes 2,600 events on an ordinary day and almost
/// all of them are *things that happened*; what was missing each time was what was
/// **true** - which sound card was selected, whether a clock reading existed, whether
/// a port write reached a radio. **A stream of events cannot answer a question about a
/// state unless something wrote the state down.**</para>
/// <para>**ONE STABLE EVENT NAME**, so a reader has one line to find rather than a
/// search through thousands. `docs/telemetry-diagnoses-it.md` is where the list of
/// facts came from, and it was written before a line of this file.</para>
/// <para>**A FACT THAT CANNOT BE DETERMINED SAYS SO, WITH WHY** - never absent, never
/// a plausible default. That is the day's own lesson: 21 records said `Sent` for
/// transmissions that never keyed a transmitter, and a wrong diagnostic cost an hour
/// where a missing one would have cost a question.</para>
/// <para>**NOTHING PERSONAL** (§2.1). No callsign, no name, no grid, no location. A
/// device name, a driver, a port, a version and an offset are not personal, and the
/// About window already promises exactly that.</para>
/// <para>**IT IS WRITTEN EVEN WHEN IT IS BAD NEWS, ESPECIALLY THEN.** Every reader
/// below is wrapped, so a machine broken enough to throw while being described still
/// gets described - with the throw in place of the fact.</para>
/// </remarks>
public static class StartupSnapshot
{
    /// <summary>The one name a reader looks for.</summary>
    public const string EventName = "startup_snapshot";

    /// <summary>Which of the two snapshots this is.</summary>
    /// <remarks>
    /// <para>**THE EARLY ONE FIRES BEFORE THE FACTS EXIST AND THAT IS NOT A DEFECT
    /// TO REMOVE** (work instruction 305 task 2). Measured on this machine: the
    /// snapshot is written at 246 ms and the clock query answers 264 ms after it; on
    /// the shack machine the gap was 1.0 to 1.3 seconds. **The one event designed to
    /// say what state the machine is in could not see the state of the machine.**
    /// </para>
    /// <para>**BOTH ARE WRITTEN, AND EACH IS READABLE ON ITS OWN.** The early one is
    /// what a machine that dies during startup leaves behind, which is exactly the
    /// machine somebody needs a record of; the settled one is what a reader wants
    /// when the application got going. **Waiting instead of writing twice would
    /// trade the first for the second**, and the instruction's own rule is not to
    /// wait indefinitely for a fact that may never arrive - a radio nobody plugged
    /// in never arrives at all.</para>
    /// <para>**ONE STABLE EVENT NAME EITHER WAY**, so a reader still has one line to
    /// find; this field says which of the two they are looking at.</para>
    /// </remarks>
    public const string WhenField = "when";

    /// <summary>Written before the application had a chance to learn anything.</summary>
    public const string AtStart = "at_start";

    /// <summary>Written once the facts that arrive on their own have arrived.</summary>
    public const string Settled = "settled";

    /// <summary>What a fact says when it could not be determined.</summary>
    /// <remarks>
    /// **`unknown` IS A REAL ANSWER AND A DEFAULT IS NOT** (§0.0). It is always
    /// followed by a `…Why` field saying what stopped it being known.
    /// </remarks>
    public const string Unknown = "unknown";

    /// <summary>Build the snapshot from whatever each part of the app can say.</summary>
    /// <param name="parts">The facts, each already gathered or already failed.</param>
    /// <returns>One flat bag, ready to write.</returns>
    /// <exception cref="ArgumentNullException">There are no parts.</exception>
    /// <remarks>
    /// **FLAT ON PURPOSE.** A reader greps one line out of a day's file and reads it;
    /// nesting would make that line need a parser.
    /// </remarks>
    public static IReadOnlyDictionary<string, object?> Compose(SnapshotParts parts)
    {
        ArgumentNullException.ThrowIfNull(parts);

        var bag = new Dictionary<string, object?>(StringComparer.Ordinal);

        // **WHICH OF THE TWO THIS IS, FIRST**, so a reader knows before anything
        // else whether an `unknown` below means *not yet* or *not at all*.
        bag[WhenField] = parts.When;

        Add(bag, "appVersion", parts.AppVersion);
        Add(bag, "ft8SharpVersion", parts.Ft8SharpVersion);
        Add(bag, "ft8SharpDeepVersion", parts.Ft8SharpDeepVersion);
        Add(bag, "framework", parts.Framework);
        Add(bag, "osBuild", parts.OsBuild);
        Add(bag, "processBits", parts.ProcessBits);

        // **THE AUDIO BLOCK IS THE ONE THAT WOULD HAVE ANSWERED THE CQ MYSTERY.**
        // 56 refusals were written, each naming the field that decided it, and none
        // named the missing device - because it was upstream of what readiness looks
        // at.
        Add(bag, "audioInputCount", parts.AudioInputs?.Count);
        Add(bag, "audioInputs", Join(parts.AudioInputs));
        Add(bag, "audioInputSelected", parts.AudioInputSelected);
        Add(bag, "audioInputPresent", parts.AudioInputPresent);
        Add(bag, "audioInputChosenBy", parts.AudioInputChosenBy);

        Add(bag, "audioOutputCount", parts.AudioOutputs?.Count);
        Add(bag, "audioOutputs", Join(parts.AudioOutputs));
        Add(bag, "transmitDeviceSelected", parts.TransmitDeviceSelected);
        Add(bag, "transmitDevicePresent", parts.TransmitDevicePresent);

        Add(bag, "radioConnected", parts.RadioConnected);
        Add(bag, "radioPort", parts.RadioPort);
        Add(bag, "radioBaud", parts.RadioBaud);
        Add(bag, "radioCivAddress", parts.RadioCivAddress);
        Add(bag, "radioModel", parts.RadioModel);
        Add(bag, "radioLastAnsweredSecondsAgo", parts.RadioLastAnsweredSecondsAgo);

        Add(bag, "clockOffsetKnown", parts.ClockOffsetKnown);
        Add(bag, "clockOffsetSeconds", parts.ClockOffsetSeconds);
        Add(bag, "clockOffsetAgeSeconds", parts.ClockOffsetAgeSeconds);
        Add(bag, "clockLastQueryReason", parts.ClockLastQueryReason);

        Add(bag, "transmitReadiness", parts.TransmitReadiness);
        Add(bag, "transmitReadinessDecidedBy", parts.TransmitReadinessDecidedBy);

        // **WHICH MODE AND WHICH TAB THE APPLICATION WAS ON** (work instruction 305
        // task 4). The file from 2026-09-10 could not say: the only hint anywhere in
        // seventeen events that the operator was on CW was a `neighborhood_clicked`
        // reading *CW main street*, which is where he tuned rather than what Hamlet
        // was running. **Nothing else in the record can be read without it** - an
        // absence of slots means one thing on the Digital tab and nothing at all
        // anywhere else.
        Add(bag, "appOperatingMode", parts.AppOperatingMode);
        Add(bag, "appDigitalMode", parts.AppDigitalMode);

        Add(bag, "settingsLoaded", parts.SettingsLoaded);
        Add(bag, "settingsPathExists", parts.SettingsPathExists);
        Add(bag, "settingsNamedButAbsent", Join(parts.SettingsNamedButAbsent));

        // **WHICH CATEGORIES ARE ON, SO A READER KNOWS WHAT IS MISSING ON PURPOSE.**
        // Measured in `JsonlTelemetry.Write`: a category that is off returns before
        // serialising, so its events leave no trace at all. Without this line a
        // reader cannot tell *nothing happened* from *not recorded*.
        Add(bag, "telemetryCategoriesOn", Join(parts.TelemetryCategoriesOn));
        Add(bag, "telemetryCategoriesOff", Join(parts.TelemetryCategoriesOff));
        Add(bag, "telemetryEventsDropped", parts.TelemetryEventsDropped);

        foreach (var (key, why) in parts.NotKnown)
        {
            bag[key] = Unknown;
            bag[key + "Why"] = why;
        }

        return bag;
    }

    /// <summary>Put a fact in, or say it is unknown and why nothing said why.</summary>
    /// <remarks>
    /// **A NULL HERE IS A BUG RATHER THAN AN ANSWER**, so it is written as unknown
    /// with a reason saying nobody supplied one. A silently absent field is the shape
    /// this whole unit exists to remove.
    /// </remarks>
    private static void Add(
        Dictionary<string, object?> bag, string key, object? value)
    {
        if (value is null)
        {
            bag[key] = Unknown;
            bag[key + "Why"] = "nothing supplied this fact";

            return;
        }

        bag[key] = value;
    }

    private static string? Join(IReadOnlyList<string>? items)
        => items is null ? null : items.Count == 0 ? "(none)" : string.Join(" | ", items);

    /// <summary>A number, rounded so a reader is not given false precision.</summary>
    /// <param name="seconds">The figure.</param>
    /// <returns>The figure to three decimals, or null.</returns>
    public static double? Round(double? seconds)
        => seconds is { } value ? Math.Round(value, 3) : null;

    /// <summary>How long ago something was, in whole seconds, or null.</summary>
    /// <param name="at">When it was.</param>
    /// <param name="nowUtc">Now.</param>
    /// <returns>The age in seconds, or null where there is no moment.</returns>
    public static double? AgeSeconds(DateTime? at, DateTime nowUtc)
        => at is { } moment
            ? Math.Round((nowUtc - moment).TotalSeconds, 1)
            : null;

    /// <summary>A count as a string, for a field that is otherwise a name.</summary>
    /// <param name="count">The count.</param>
    /// <returns>The count in the invariant culture.</returns>
    public static string Count(int count)
        => count.ToString(CultureInfo.InvariantCulture);
}

/// <summary>
/// Everything the snapshot can say, each part already gathered or already failed.
/// </summary>
/// <remarks>
/// <para>**THE GATHERING IS THE CALLER'S AND THE SHAPE IS THIS TYPE'S** (§0.1). The
/// engine holds no reference to a view model and does not know how to enumerate a
/// sound card; what it knows is what a diagnostic has to contain.</para>
/// <para>**EVERY MEMBER IS NULLABLE AND NULL MEANS UNKNOWN**, which
/// <see cref="StartupSnapshot.Compose"/> turns into the word and a reason. That is
/// deliberate: it is impossible to leave a fact silently out.</para>
/// </remarks>
public sealed class SnapshotParts
{
    /// <summary>Which of the two snapshots this is.</summary>
    public string When { get; init; } = StartupSnapshot.AtStart;

    /// <summary>Facts that are known to be unknowable, with why.</summary>
    /// <remarks>
    /// **A READER NEEDS THE DIFFERENCE BETWEEN *NOBODY ASKED* AND *IT CANNOT BE
    /// ASKED*.** A caller that knows why a fact is out of reach says so here rather
    /// than leaving a null for the generic reason.
    /// </remarks>
    public Dictionary<string, string> NotKnown { get; } = new(StringComparer.Ordinal);

    /// <summary>The application's version.</summary>
    public string? AppVersion { get; init; }

    /// <summary>The port's version.</summary>
    public string? Ft8SharpVersion { get; init; }

    /// <summary>The sibling's version.</summary>
    public string? Ft8SharpDeepVersion { get; init; }

    /// <summary>The framework this is running on.</summary>
    public string? Framework { get; init; }

    /// <summary>The operating-system build.</summary>
    public string? OsBuild { get; init; }

    /// <summary>32 or 64.</summary>
    public int? ProcessBits { get; init; }

    /// <summary>Every audio input device present, as `id name`.</summary>
    public IReadOnlyList<string>? AudioInputs { get; init; }

    /// <summary>The device selected for receive.</summary>
    public string? AudioInputSelected { get; init; }

    /// <summary>Whether the selected receive device is present on this machine.</summary>
    public bool? AudioInputPresent { get; init; }

    /// <summary>How the receive device was chosen: remembered, matched, defaulted.</summary>
    public string? AudioInputChosenBy { get; init; }

    /// <summary>Every audio output device present.</summary>
    public IReadOnlyList<string>? AudioOutputs { get; init; }

    /// <summary>The device selected for transmit.</summary>
    public string? TransmitDeviceSelected { get; init; }

    /// <summary>
    /// **Whether the selected transmit device is present on this machine.**
    /// </summary>
    /// <remarks>
    /// **THIS ONE FIELD IS THE CQ MYSTERY.** A reboot left the settings naming a
    /// device that was gone; pressing CQ did nothing, readiness refused 56 times, and
    /// nothing anywhere said the device was missing.
    /// </remarks>
    public bool? TransmitDevicePresent { get; init; }

    /// <summary>Whether the radio is connected.</summary>
    public bool? RadioConnected { get; set; }

    /// <summary>The serial port.</summary>
    public string? RadioPort { get; set; }

    /// <summary>The baud rate.</summary>
    public int? RadioBaud { get; init; }

    /// <summary>The CI-V address, as the radio answers at.</summary>
    public string? RadioCivAddress { get; init; }

    /// <summary>The model as read from the radio, never as assumed.</summary>
    public string? RadioModel { get; set; }

    /// <summary>How long ago the radio last answered anything.</summary>
    public double? RadioLastAnsweredSecondsAgo { get; set; }

    /// <summary>Whether an offset is held at all.</summary>
    public bool? ClockOffsetKnown { get; set; }

    /// <summary>The offset in seconds, where one is held.</summary>
    public double? ClockOffsetSeconds { get; set; }

    /// <summary>How old the offset is.</summary>
    public double? ClockOffsetAgeSeconds { get; set; }

    /// <summary>Unit 303's token for the last query, or that none has finished.</summary>
    public string? ClockLastQueryReason { get; set; }

    /// <summary>The readiness state.</summary>
    public string? TransmitReadiness { get; set; }

    /// <summary>The fields readiness was decided from.</summary>
    public string? TransmitReadinessDecidedBy { get; set; }

    /// <summary>Which tab the application is on - CW, Digital, Voice.</summary>
    public string? AppOperatingMode { get; set; }

    /// <summary>Which digital mode the Digital tab is running.</summary>
    public string? AppDigitalMode { get; set; }

    /// <summary>Whether the settings file was read successfully.</summary>
    public bool? SettingsLoaded { get; init; }

    /// <summary>Whether a settings file exists at all.</summary>
    public bool? SettingsPathExists { get; init; }

    /// <summary>Everything the settings named that this machine has not got.</summary>
    public IReadOnlyList<string>? SettingsNamedButAbsent { get; init; }

    /// <summary>The telemetry categories that are on.</summary>
    public IReadOnlyList<string>? TelemetryCategoriesOn { get; init; }

    /// <summary>The telemetry categories that are off, so absence can be read.</summary>
    public IReadOnlyList<string>? TelemetryCategoriesOff { get; init; }

    /// <summary>How many events the sink has dropped.</summary>
    public long? TelemetryEventsDropped { get; init; }
}
