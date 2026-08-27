using System.Text.Json;
using System.Text.Json.Serialization;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Telemetry;

namespace Hamlet.App.Settings;

/// <summary>
/// Everything Hamlet remembers between runs, in one file:
/// <c>%AppData%\Hamlet\settings.json</c> (HM-DEC-018). A corrupt or
/// unreadable file yields defaults — losing preferences is a nuisance,
/// refusing to start is a bug.
/// </summary>
public sealed class AppSettings
{
    /// <summary>Window left in device-independent pixels; null until saved.</summary>
    public double? WindowX { get; set; }

    /// <summary>Window top; null until saved.</summary>
    public double? WindowY { get; set; }

    /// <summary>Window width; null until saved.</summary>
    public double? WindowWidth { get; set; }

    /// <summary>Window height; null until saved.</summary>
    public double? WindowHeight { get; set; }

    /// <summary>Whether the window was maximized at exit.</summary>
    public bool WindowMaximized { get; set; }

    /// <summary>Last selected port or the simulated-rig entry.</summary>
    public string? LastPort { get; set; }

    /// <summary>
    /// Reconnect to <see cref="LastPort"/> when the app opens.
    /// </summary>
    /// <remarks>
    /// On by default, because clicking Connect is friction on the one action
    /// the operator performs every single time and the app already knows which
    /// port they used. It fails quietly by design: a radio that is switched off
    /// or a port that has renumbered is the normal case rather than an error,
    /// and Hamlet says so in the status line and carries on with the training
    /// radio (HM-DEC-052).
    /// </remarks>
    public bool ReconnectOnStartup { get; set; } = true;

    /// <summary>
    /// Let Hamlet set the radio's mode to match where the dial is pointing.
    /// </summary>
    /// <remarks>
    /// On by default, because the operator this is for does not yet know that
    /// 14.074 wants USB-D and the app does. It is a setting rather than a
    /// constant because it is the first thing this app does to somebody's radio
    /// without being asked, and anybody who would rather drive themselves must
    /// be able to say so and be obeyed (HM-DEC-056).
    /// </remarks>
    public bool ModeFollowsTheMap { get; set; } = true;

    /// <summary>
    /// The frequencies the operator saved, in the order they chose (HM-DEC-060).
    /// </summary>
    /// <remarks>
    /// Kept here rather than in the radio's own memory channels, which are
    /// numbered slots whose meaning you have to remember. Hamlet's carry the
    /// reason: the band, the mode and what the map said lives there.
    /// </remarks>
    public List<SavedFavorite> Favorites { get; set; } = new();

    /// <summary>
    /// Where the operator has been, most recent first (HM-DEC-072).
    /// </summary>
    /// <remarks>
    /// Persisted for the same reason favorites are, and the moment it matters
    /// most is the following evening thinking "where was that station". A list
    /// that emptied on exit would fail exactly then.
    /// </remarks>
    public List<SavedRecentStation> Recent { get; set; } = new();

    /// <summary>
    /// True once a send on this installation produced a real SWR reading
    /// (HM-DEC-081).
    /// </summary>
    /// <remarks>
    /// What retires the note about not being able to see the back of the radio.
    /// It earns its place before a first transmission and becomes furniture
    /// after one, so it goes when Hamlet has measured something about the socket
    /// and the operator has seen the number. Persisted so it does not come back
    /// on restart.
    /// </remarks>
    public bool HasMeasuredSwr { get; set; }

    /// <summary>Last selected band name, e.g. "40 m".</summary>
    public string? LastBand { get; set; }

    /// <summary>Who is operating (HM-DEC-019). Displayed in the app, written
    /// here, and never written to telemetry.</summary>
    public OperatorProfile Operator { get; set; } = new();

    /// <summary>
    /// Which of the happening-now panel's two lenses was last chosen, or null
    /// when the operator has never chosen one (HM-DEC-057).
    /// </summary>
    /// <remarks>
    /// Null is the state that lets Hamlet guess. Once it holds a value the
    /// operator has answered the question themselves, and guessing again after
    /// that is the app arguing with them.
    /// </remarks>
    public string? SpotLens { get; set; }

    /// <summary>
    /// When the operator last finished looking at "what's new", or null.
    /// </summary>
    /// <remarks>
    /// The watermark the delta is measured from. It moves when they leave the
    /// lens rather than when they arrive at it, so the list stays still while
    /// they are reading it and is a fresh delta the next time they come back.
    /// </remarks>
    public DateTime? SpotsLastLookedUtc { get; set; }

    /// <summary>
    /// Which mode families the happening-now panel is showing (HM-DEC-061).
    /// </summary>
    /// <remarks>
    /// Null means all of them, which is where a fresh profile starts. Stored as
    /// names rather than numbers so somebody reading their own settings file can
    /// see what it says.
    /// </remarks>
    public List<string>? SpotFamilies { get; set; }

    /// <summary>Minutes between happening-now refreshes; 0 is off
    /// (HM-DEC-020). Allowed values are in
    /// <see cref="SpotRefreshChoices"/>.</summary>
    public int SpotRefreshMinutes { get; set; } = DefaultSpotRefreshMinutes;

    /// <summary>Per-panel expand/collapse state, keyed by panel id. An absent
    /// key means expanded — a new panel arrives open (HM-DEC-021).</summary>
    public Dictionary<string, bool> PanelExpanded { get; set; } = new();

    /// <summary>
    /// Per-source on/off switches, keyed by source name (HM-DEC-022,
    /// HM-DEC-024). An absent key falls back to
    /// <see cref="DefaultSourceEnabled"/>.
    /// </summary>
    public Dictionary<string, bool> SourceEnabled { get; set; } = new();

    /// <summary>Telemetry category switches. Absent category means enabled —
    /// all categories default on (HM-DEC-018).</summary>
    public Dictionary<string, bool> TelemetryCategories { get; set; } = new();

    /// <summary>Telemetry folder size cap in megabytes.</summary>
    public int TelemetryMaxMegabytes { get; set; } = 50;

    /// <summary>
    /// "Only let me transmit where my license allows" (HM-DEC-029). On by
    /// default.
    /// </summary>
    /// <remarks>
    /// TRANSMIT ONLY. This never restricts tuning, receiving or anything the
    /// band map draws — listening is not regulated and a setting that implied
    /// otherwise would teach a beginner something false about their own
    /// license. It is read at one moment: before Hamlet keys a transmitter.
    /// </remarks>
    public bool RestrictTransmitToPrivileges { get; set; } = true;

    /// <summary>
    /// Whether distances are spoken in miles or kilometers (HM-DEC-038).
    /// </summary>
    /// <remarks>
    /// Miles, because the operator is American, the licence is American and the
    /// regulations are American — the same reasoning as the spelling standard
    /// (HM-DEC-035). It is a setting rather than a constant because the app is
    /// headed for a public release where most of the world counts the other
    /// way, and the default is picked rather than asked (§0.4).
    /// </remarks>
    public DistanceUnits DistanceUnits { get; set; } = DistanceUnits.Miles;

    /// <summary>
    /// Which byline was shown last launch, so the next one differs
    /// (HM-DEC-039). −1 when none has been shown.
    /// </summary>
    public int LastBylineIndex { get; set; } = -1;

    /// <summary>
    /// The capture device the operator chose to decode from, or null to let
    /// Hamlet pick.
    /// </summary>
    /// <remarks>
    /// Stored as the device's own id rather than its name, because names
    /// change when a driver updates and an id does not. A device that has
    /// been unplugged falls back quietly rather than leaving the app with
    /// nothing to listen to.
    /// </remarks>
    public string? AudioInputDeviceId { get; set; }

    /// <summary>
    /// True once the operator has tuned with the scroll wheel (HM-DEC-141).
    /// </summary>
    /// <remarks>
    /// What retires the hint under the frequency readout. A line explaining how
    /// to do something the operator has already done is a line that teaches them
    /// to stop reading that part of the window.
    /// </remarks>
    public bool HasTunedByWheel { get; set; }

    /// <summary>
    /// Whether the independent keying sweep is drawn on the terminal.
    /// </summary>
    /// <remarks>
    /// <para>**OFF, BECAUSE THE INSTRUMENT IS WRONG MORE OFTEN THAN IT IS
    /// RIGHT.** The sweep was built to tell the operator when the decoder is
    /// looking in the wrong place, and measured against independent readings it
    /// disagreed with the truth on fourteen of twenty recordings. Unit 1.11.10
    /// then measured its calibration **inside an overlap** rather than in a gap:
    /// the four recordings holding nothing swing 14.1 to 17.7 decibels while
    /// `cw-2026-08-25-021825`, which holds a station, swings 12.6 — below all of
    /// them. There is no bar that separates them.</para>
    /// <para>**IT KEEPS COMPUTING AND IT KEEPS WRITING TO THE SIDECAR.** What is
    /// wrong with it is that it asserts on screen, where a second panel
    /// contradicting the first sends the operator to the radio for a decoder
    /// condition. The measurements are still worth having beside a recording, and
    /// rebuilding the instrument is its own unit rather than tonight's work.</para>
    /// <para>A setting rather than a deletion, so the person diagnosing it can
    /// still see it.</para>
    /// </remarks>
    public bool ShowKeyingSweep { get; set; }

    /// <summary>
    /// Whether the joint cutter decides where characters are cut.
    /// </summary>
    /// <remarks>
    /// <para>**SHIPS OFF, AND THE RULING SAYS WHEN IT MAY SHIP ON** (Tim,
    /// 2026-08-27): default on if every floor and every anchor is green, default
    /// off and shipped anyway with the measurement reported if they are not. They
    /// are not.</para>
    /// <para>**WHAT IT DOES AND WHAT IT COSTS, MEASURED 2026-08-27.** It repairs
    /// the cuts it was built for — `AB OV E` becomes `ABOVE`, `BR EE Z E` becomes
    /// `BREEZE`, `REV■R` becomes `REVER` — and it loses every word space. On a
    /// compressed fist at thirty words a minute the word gap runs well under one
    /// unit, so scored against three and seven it reads as a character gap every
    /// time, and `cw-2026-08-18-004507`'s anchor `N HANDLING THIS MESSAG` needs
    /// those spaces.</para>
    /// <para>That is HM-DEC-115's finding arriving a second time: gaps have to be
    /// clustered from the sender's own keying and never taken as multiples of the
    /// dit. The cutter accepts this sender's three fitted classes and the
    /// streaming path does not always have them to give.</para>
    /// </remarks>
    public bool UseJointDecoder { get; set; }

    /// <summary>
    /// The pitch the operator hears a CW signal at, in hertz.
    /// </summary>
    /// <remarks>
    /// The IC-7300 sets this between 300 and 900 Hz, and CI-V command
    /// <c>14 09</c> encodes exactly that range with 600 Hz at its midpoint
    /// (Full Manual section 19, p. 19-3, and p. 4-14). So 600 is the middle of
    /// what this radio does rather than a number carried in from elsewhere.
    /// The decoder tracks the tone within a window either side of this, since
    /// nobody tunes exactly.
    /// </remarks>
    public int CwPitchHz { get; set; } = DefaultCwPitchHz;

    /// <summary>The CW pitch the app ships with, in hertz.</summary>
    public const int DefaultCwPitchHz = 600;

    /// <summary>Lowest CW pitch the radio offers, in hertz.</summary>
    public const int MinimumCwPitchHz = 300;

    /// <summary>Highest CW pitch the radio offers, in hertz.</summary>
    public const int MaximumCwPitchHz = 900;

    /// <summary>
    /// The Morse speed the operator would rather work at, in words a minute.
    /// </summary>
    /// <remarks>
    /// <para>A PREFERENCE AND NOT A MEASUREMENT (HM-DEC-066, HM-OPEN-006).
    /// Hamlet has never listened to anybody copy anything, so this number says
    /// what they would rather work at and nothing about what they can do. The
    /// difference decides what the app is allowed to say: it may put a station
    /// at 28 words a minute against the number here and call it far over, since
    /// both are stated figures, and it may never turn that into a verdict about
    /// the person reading it.</para>
    /// <para>Nothing is filtered out by it and nothing is hidden. It is a
    /// preference the ranking weighs, so a station sending at a pace somebody
    /// asked for sits higher up a list they can still scroll past.</para>
    /// </remarks>
    public int CopySpeedWpm { get; set; } = DefaultCopySpeedWpm;

    /// <summary>
    /// The copy speed a fresh install starts at, in words a minute.
    /// </summary>
    /// <remarks>
    /// Thirteen, which is where the ranking has always drawn the line between a
    /// relaxed pace and an ordinary one, so the number is read from that scale
    /// rather than typed again beside it (HM-DEC-066). It is deliberately below
    /// what most of the band runs at. Somebody new is better served by an app
    /// that starts gentle and lets them raise it than by one that starts where
    /// the contest operators live and leaves them wondering why none of this
    /// sounds like the practice files.
    /// </remarks>
    public const int DefaultCopySpeedWpm = SpotRankWeights.RelaxedWpm;

    /// <summary>Slowest copy speed the setting offers, in words a minute.</summary>
    /// <remarks>
    /// Five is where the licensing code tests once sat and where most people
    /// start, so it is the floor rather than a number chosen for roundness.
    /// </remarks>
    public const int MinimumCopySpeedWpm = 5;

    /// <summary>Fastest copy speed the setting offers, in words a minute.</summary>
    /// <remarks>
    /// Forty is comfortably past what a contest runs at, so nobody meets a
    /// ceiling that says more about the app than about them.
    /// </remarks>
    public const int MaximumCopySpeedWpm = 40;

    /// <summary>
    /// How long a park or summit activation stays a live invitation, in
    /// minutes (HM-DEC-045).
    /// </summary>
    /// <remarks>
    /// An activator hauled gear somewhere on purpose and stays put working
    /// whoever calls, often for well over an hour, so an hour is generous
    /// rather than optimistic.
    /// </remarks>
    public int ActivationLifetimeMinutes { get; set; } = 60;

    /// <summary>How long a skimmer report stays a live invitation, in minutes.</summary>
    /// <remarks>
    /// Much shorter, because a skimmer report says somebody called CQ at that
    /// moment and nothing at all about whether they are still calling.
    /// </remarks>
    public int SkimmerLifetimeMinutes { get; set; } = 20;

    /// <summary>How long contest activity stays a live invitation, in minutes.</summary>
    /// <remarks>
    /// Longest of the three: contest stations sit on one frequency for the
    /// whole event. Only applied where the source actually said it was a
    /// contest exchange, never guessed from a busy band.
    /// </remarks>
    public int ContestLifetimeMinutes { get; set; } = 180;

    /// <summary>The configured lifetimes, with absurd values refused.</summary>
    [JsonIgnore]
    public SpotLifetimeSettings Lifetimes => SpotLifetimeSettings.FromMinutes(
        ActivationLifetimeMinutes, SkimmerLifetimeMinutes, ContestLifetimeMinutes);

    /// <summary>The refresh interval the app ships with, in minutes.</summary>
    public const int DefaultSpotRefreshMinutes = 5;

    /// <summary>The offered refresh intervals in minutes; 0 is off
    /// (HM-DEC-020).</summary>
    public static IReadOnlyList<int> SpotRefreshChoices { get; } =
        new[] { 0, 1, 2, 5, 10, 15 };

    /// <summary>True when the category is on. Unknown categories are on.</summary>
    public bool IsTelemetryEnabled(TelemetryCategory category)
        => !TelemetryCategories.TryGetValue(category.ToString(), out var on) || on;

    /// <summary>Turn a category on or off.</summary>
    public void SetTelemetryEnabled(TelemetryCategory category, bool enabled)
        => TelemetryCategories[category.ToString()] = enabled;

    /// <summary>How many telemetry categories are currently on. Derived from
    /// the switches, never stored alongside them.</summary>
    [JsonIgnore]
    public int EnabledTelemetryCategoryCount
        => Enum.GetValues<TelemetryCategory>().Count(IsTelemetryEnabled);

    /// <summary>True when the panel is expanded. Unknown panels are expanded.</summary>
    /// <param name="panelKey">Stable panel id, e.g. "spots".</param>
    public bool IsPanelExpanded(string panelKey)
        => !PanelExpanded.TryGetValue(panelKey, out var open) || open;

    /// <summary>
    /// Whether an activity source ships switched on.
    /// </summary>
    /// <param name="sourceName">Source name, e.g. "POTA".</param>
    /// <returns>True when the source is on by default.</returns>
    /// <remarks>
    /// <para>Two ship off. SOTA is off for a reason that is not technical: its
    /// API's terms of service require the developer to have registered with
    /// the SOTA Reflector's API-consumers group and to have had AI-written
    /// software approved before it connects. Hamlet will not enter into that
    /// on the operator's behalf, so the switch starts off and the reason is
    /// printed next to it (HM-DEC-024).</para>
    /// <para>The sample feed is off because live feeds now work, and mixing
    /// invented spots into a real list is the prime directive broken for the
    /// sake of a fuller-looking panel. It stays one click away, because it is
    /// how the Explorer gets built with no network.</para>
    /// </remarks>
    public static bool DefaultSourceEnabled(string sourceName)
        => !string.Equals(
               sourceName,
               RadioEngine.Explore.SotaActivitySource.SourceName,
               StringComparison.OrdinalIgnoreCase)
           && !string.Equals(
               sourceName,
               RadioEngine.Explore.FakeActivitySource.SourceName,
               StringComparison.OrdinalIgnoreCase);

    /// <summary>True when an activity source is switched on.</summary>
    /// <param name="sourceName">Source name, e.g. "POTA".</param>
    /// <returns>True when the source should be polled.</returns>
    public bool IsSourceEnabled(string sourceName)
        => SourceEnabled.TryGetValue(sourceName, out var on)
            ? on
            : DefaultSourceEnabled(sourceName);

    /// <summary>Switch an activity source on or off.</summary>
    /// <param name="sourceName">Source name, e.g. "POTA".</param>
    /// <param name="enabled">True to poll it.</param>
    public void SetSourceEnabled(string sourceName, bool enabled)
        => SourceEnabled[sourceName] = enabled;

    /// <summary>Record a panel's expand/collapse state.</summary>
    /// <param name="panelKey">Stable panel id, e.g. "spots".</param>
    /// <param name="expanded">True when the panel is open.</param>
    public void SetPanelExpanded(string panelKey, bool expanded)
        => PanelExpanded[panelKey] = expanded;
}

/// <summary>Loads and saves <see cref="AppSettings"/>, and owns the paths
/// every other component asks for.</summary>
public static class SettingsStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,

        // Enums as names, not numbers. Two reasons, and the second is the
        // serious one. "LicenseClass": "General" is legible to somebody
        // reading their own settings file, where "LicenseClass": 3 is not.
        // And a person who hand-edits it to "General" gets what they meant —
        // without this converter that write throws, LoadFrom catches, and
        // EVERY setting silently reverts to defaults, which is a spectacular
        // punishment for a reasonable guess (HM-DEC-028 expects the callsign
        // and class to arrive from hand-edited files).
        // Reading still accepts the numeric form, so files written by earlier
        // builds load unchanged.
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>%AppData%\Hamlet — the one folder Hamlet writes to.</summary>
    public static string DataFolder { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Hamlet");

    /// <summary>%AppData%\Hamlet\telemetry.</summary>
    public static string TelemetryFolder { get; } = Path.Combine(DataFolder, "telemetry");

    /// <summary>%AppData%\Hamlet\settings.json.</summary>
    public static string SettingsPath { get; } = Path.Combine(DataFolder, "settings.json");

    /// <summary>
    /// %AppData%\Hamlet\scan-segments.json — where a scan may move the dial.
    /// </summary>
    /// <remarks>
    /// **THE OPERATOR'S FILE, NOT HAMLET'S** (§0.2.1). Hamlet writes it once, the
    /// first time it has anywhere to put it, and never touches it again. It sits
    /// beside the settings rather than inside them because it is meant to be
    /// opened in an editor, and a stretch of band buried in a settings blob is a
    /// stretch of band nobody will edit.
    /// </remarks>
    public static string ScanSegmentsPath { get; }
        = Path.Combine(DataFolder, "scan-segments.json");

    /// <summary>
    /// %AppData%\Hamlet\scan-home — where the dial was when a scan started.
    /// </summary>
    /// <remarks>
    /// **A FILE RATHER THAN A SETTING, BECAUSE OF WHEN IT IS WRITTEN** (§0.2.1).
    /// It goes down in the moment before the first tune and is deleted the moment
    /// the dial is back, so it exists only while a scan is in flight. Settings
    /// are saved on a clean exit, which is the one exit this has to survive.
    /// </remarks>
    public static string ScanHomePath { get; } = Path.Combine(DataFolder, "scan-home");

    /// <summary>Load settings, or defaults if the file is missing, corrupt or
    /// unreadable. Never throws.</summary>
    public static AppSettings Load() => LoadFrom(SettingsPath);

    /// <summary>Save settings. Never throws; a failed save loses preferences,
    /// nothing more.</summary>
    public static void Save(AppSettings settings) => SaveTo(settings, SettingsPath);

    /// <summary>Load settings from an explicit path. The real load and the
    /// tested load are the same code (§5).</summary>
    /// <param name="path">Settings file path.</param>
    public static AppSettings LoadFrom(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return new AppSettings();
            }

            var json = File.ReadAllText(path);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, Options)
                           ?? new AppSettings();

            // Keys that have been renamed since the file was written are
            // carried forward here, so an upgrade never looks like the app
            // forgetting who the operator is (HM-DEC-035).
            SettingsMigrations.Apply(settings, json);

            return settings;
        }
        catch (Exception)
        {
            return new AppSettings();
        }
    }

    /// <summary>Save settings to an explicit path. Never throws.</summary>
    /// <param name="settings">Settings to write.</param>
    /// <param name="path">Destination file path.</param>
    public static void SaveTo(AppSettings settings, string path)
    {
        try
        {
            var folder = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(path, JsonSerializer.Serialize(settings, Options));
        }
        catch (Exception)
        {
            // Preferences are best-effort.
        }
    }

    /// <summary>Open the data folder in the OS file browser.</summary>
    public static void OpenDataFolder()
    {
        try
        {
            Directory.CreateDirectory(DataFolder);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = DataFolder,
                UseShellExecute = true,
            });
        }
        catch (Exception)
        {
            // Nothing to do if the shell refuses.
        }
    }

    /// <summary>
    /// Open the operator's scan file in whatever edits text here (§0.2.1).
    /// </summary>
    /// <remarks>
    /// **THE FILE IS WRITTEN BEFORE IT IS OPENED, NEVER OVERWRITTEN.** §0.2.1
    /// requires the scanned stretch to come from a file the operator edits, and
    /// a menu entry that opens nothing is not a way to edit anything. Anything
    /// already there is his and is left exactly as it is.
    /// </remarks>
    public static void OpenScanSegments()
    {
        try
        {
            Hamlet.RadioEngine.Scan.ScanSegments.WriteDefaultIfMissing(ScanSegmentsPath);

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = ScanSegmentsPath,
                UseShellExecute = true,
            });
        }
        catch (Exception)
        {
            // Nothing to do if the shell refuses (§8).
        }
    }
}

/// <summary>One visited frequency, as settings.json holds it (HM-DEC-072).</summary>
/// <remarks>
/// A settings shape rather than the engine's record, for the same reason
/// <see cref="SavedFavorite"/> is one: anything persisted has to survive a
/// rename with a migration behind it (§6.1).
/// </remarks>
public sealed class SavedRecentStation
{
    /// <summary>Where it is.</summary>
    public long FrequencyHz { get; set; }

    /// <summary>The callsign if one was identified, or "".</summary>
    public string Station { get; set; } = "";

    /// <summary>The mode at the time.</summary>
    public string Mode { get; set; } = "";

    /// <summary>Which band.</summary>
    public string BandName { get; set; } = "";

    /// <summary>What the map said lives there.</summary>
    public string Neighborhood { get; set; } = "";

    /// <summary>When the visit was recorded.</summary>
    public DateTime VisitedUtc { get; set; }

    /// <summary>
    /// How many times the operator has settled here (HM-DEC-134).
    /// </summary>
    /// <remarks>
    /// **ABSENT IN EVERY PROFILE WRITTEN BEFORE HM-DEC-134**, and absent reads as
    /// one rather than as zero, because an entry is in this list precisely
    /// because somebody was there (§6.1). Nothing is migrated and nothing is
    /// lost: an existing list keeps every place in it and starts counting
    /// returns from the next one.
    /// </remarks>
    public int Visits { get; set; } = 1;

    /// <summary>
    /// How the station came to be known (HM-DEC-073), as its name.
    /// </summary>
    /// <remarks>
    /// Stored as a name rather than a number so somebody reading their own
    /// settings file can see what it says. Absent in a profile written before
    /// provenance existed, and those are read back as a spot feed, because that
    /// was the only way a name could get in there at the time. That is a fact
    /// about the history of the file rather than a guess about the entry.
    /// </remarks>
    public string StationSource { get; set; } = "";
}

/// <summary>One saved frequency, as settings.json holds it (HM-DEC-060).</summary>
/// <remarks>
/// A settings shape rather than the engine's record, because it is persisted and
/// anything persisted has to survive a rename with a migration behind it
/// (§6.1). It converts to and from
/// <see cref="Hamlet.RadioEngine.Explore.Favorite"/> at the edge.
/// </remarks>
public sealed class SavedFavorite
{
    /// <summary>Where it is.</summary>
    public long FrequencyHz { get; set; }

    /// <summary>What the operator calls it.</summary>
    public string Name { get; set; } = "";

    /// <summary>The mode it was saved in.</summary>
    public string Mode { get; set; } = "";

    /// <summary>Which band.</summary>
    public string BandName { get; set; } = "";

    /// <summary>What the map said lives there when it was saved.</summary>
    public string Neighborhood { get; set; } = "";

    /// <summary>When it was saved.</summary>
    public DateTime SavedUtc { get; set; }

    /// <summary>Why this one, in the operator's own words, or "".</summary>
    /// <remarks>
    /// Defaulted rather than required, so every favorite saved before notes
    /// existed still loads and simply has none. That is what an empty note means
    /// and there is nothing to migrate (§6.1).
    /// </remarks>
    public string Note { get; set; } = "";
}
