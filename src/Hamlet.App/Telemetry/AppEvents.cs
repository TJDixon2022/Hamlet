using Hamlet.RadioEngine.Telemetry;

namespace Hamlet.App.Telemetry;

/// <summary>
/// Every telemetry event the shell can emit, and the only place their data
/// bags are built.
/// </summary>
/// <remarks>
/// <para>HM-DEC-018 forbids callsigns, machine identifiers and message
/// content in telemetry. HM-DEC-019 puts the operator's callsign into the
/// same settings file as the telemetry switches, which makes that rule easy
/// to break by accident at a call site.</para>
/// <para>So there are no call sites: ViewModels call these methods, the
/// payloads are constructed here, and one test walks every method on this
/// class with a profile loaded and asserts the callsign never reaches a
/// written line. A rule with one place to hold is a rule that can be
/// proved.</para>
/// </remarks>
public static class AppEvents
{
    /// <summary>The app started.</summary>
    /// <param name="telemetry">Sink, or null when telemetry is unavailable.</param>
    public static void AppStart(ITelemetry? telemetry)
        => telemetry?.Write(TelemetryCategory.Diagnostics, "app_start");

    /// <summary>The app is shutting down.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    public static void AppStop(ITelemetry? telemetry)
        => telemetry?.Write(TelemetryCategory.Diagnostics, "app_stop");

    /// <summary>The About window was opened.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    public static void AboutOpened(ITelemetry? telemetry)
        => telemetry?.Write(TelemetryCategory.Diagnostics, "about_opened");

    /// <summary>Diagnostics were copied to the clipboard for a bug report.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    public static void DiagnosticsCopied(ITelemetry? telemetry)
        => telemetry?.Write(TelemetryCategory.Diagnostics, "diagnostics_copied");

    /// <summary>The settings dialog was opened.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    public static void SettingsOpened(ITelemetry? telemetry)
        => telemetry?.Write(TelemetryCategory.Diagnostics, "settings_opened");

    /// <summary>
    /// An operator-profile field was edited. Records WHICH field changed and
    /// never the value — the value is the identifying part (HM-DEC-019).
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="field">Field name, e.g. "callsign".</param>
    public static void ProfileEdited(ITelemetry? telemetry, string field)
        => telemetry?.Write(TelemetryCategory.Diagnostics, "profile_edited",
            new Dictionary<string, object?> { ["field"] = field });

    /// <summary>A rig connection succeeded.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="port">Port name or the simulated-rig entry.</param>
    /// <param name="rigType">"simulated" or "IC-7300".</param>
    public static void ConnectOk(ITelemetry? telemetry, string port, string rigType)
        => telemetry?.Write(TelemetryCategory.Rig, "connect_ok",
            new Dictionary<string, object?> { ["port"] = port, ["rigType"] = rigType });

    /// <summary>A rig connection failed.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="port">Port name or the simulated-rig entry.</param>
    /// <param name="rigType">"simulated" or "IC-7300".</param>
    /// <param name="reason">Short machine-readable reason, e.g. "no_response".</param>
    public static void ConnectFailed(
        ITelemetry? telemetry, string port, string rigType, string reason)
        => telemetry?.Write(TelemetryCategory.Rig, "connect_failed",
            new Dictionary<string, object?>
            {
                ["port"] = port,
                ["rigType"] = rigType,
                ["reason"] = reason,
            },
            TelemetryLevel.Warn);

    /// <summary>The selected band changed.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="bandName">Band name, e.g. "40 m".</param>
    public static void BandChanged(ITelemetry? telemetry, string bandName)
        => telemetry?.Write(TelemetryCategory.Tuning, "band_changed",
            new Dictionary<string, object?> { ["band"] = bandName });

    /// <summary>A tune was requested from a story card or a spot.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="hz">Target frequency.</param>
    /// <param name="source">Where the click came from, e.g. "story_or_spot".</param>
    public static void TuneRequested(ITelemetry? telemetry, long hz, string source)
        => telemetry?.Write(TelemetryCategory.Tuning, "tune_requested",
            new Dictionary<string, object?> { ["hz"] = hz, ["source"] = source });

    /// <summary>A neighborhood on the map was opened.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="name">Neighborhood name.</param>
    public static void NeighborhoodClicked(ITelemetry? telemetry, string name)
        => telemetry?.Write(TelemetryCategory.Explore, "neighborhood_clicked",
            new Dictionary<string, object?> { ["name"] = name });

    /// <summary>A field-guide card was opened.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="mode">Mode name, e.g. "FT8".</param>
    public static void ModeCardOpened(ITelemetry? telemetry, string mode)
        => telemetry?.Write(TelemetryCategory.Explore, "mode_card_opened",
            new Dictionary<string, object?> { ["mode"] = mode });

    /// <summary>
    /// The happening-now feed reloaded. Counts and the trigger, never the
    /// content of a spot — a spot names a station (HM-DEC-018).
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="trigger">"manual", "timer" or "resume".</param>
    /// <param name="spotCount">Spots in the new set.</param>
    /// <param name="newCount">How many of them were not in the previous set.</param>
    public static void SpotsRefreshed(
        ITelemetry? telemetry, string trigger, int spotCount, int newCount)
        => telemetry?.Write(TelemetryCategory.Explore, "spots_refreshed",
            new Dictionary<string, object?>
            {
                ["trigger"] = trigger,
                ["spotCount"] = spotCount,
                ["newCount"] = newCount,
            });

    /// <summary>Auto-refresh paused or resumed with window visibility.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="running">True when the timer is running.</param>
    /// <param name="intervalMinutes">Configured interval; 0 is off.</param>
    public static void SpotTimerChanged(
        ITelemetry? telemetry, bool running, int intervalMinutes)
        => telemetry?.Write(TelemetryCategory.Explore, "spot_timer_changed",
            new Dictionary<string, object?>
            {
                ["running"] = running,
                ["intervalMinutes"] = intervalMinutes,
            });

    /// <summary>An activity source was switched on or off (HM-DEC-022).</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="sourceName">Source name, e.g. "POTA".</param>
    /// <param name="enabled">True when the source is now on.</param>
    public static void SourceToggled(
        ITelemetry? telemetry, string sourceName, bool enabled)
        => telemetry?.Write(TelemetryCategory.Explore, "source_toggled",
            new Dictionary<string, object?>
            {
                ["source"] = sourceName,
                ["enabled"] = enabled,
            });

    /// <summary>
    /// A source failed a refresh. Records which network and how it failed,
    /// never the request — a spot query carries the operator's callsign in
    /// its User-Agent, and that stays out of the log (HM-DEC-024).
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="sourceName">Source name, e.g. "POTA".</param>
    /// <param name="state">The state it landed in, e.g. "Degraded".</param>
    public static void SourceUnhealthy(
        ITelemetry? telemetry, string sourceName, string state)
        => telemetry?.Write(TelemetryCategory.Explore, "source_unhealthy",
            new Dictionary<string, object?>
            {
                ["source"] = sourceName,
                ["state"] = state,
            },
            TelemetryLevel.Warn);

    /// <summary>
    /// The lead card was rebuilt. Records whether there was anything to
    /// suggest and the winning score, never the station it named.
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="hasSuggestion">True when a spot cleared the bar.</param>
    /// <param name="score">The chosen spot's score, or 0.</param>
    public static void LeadCardBuilt(
        ITelemetry? telemetry, bool hasSuggestion, int score)
        => telemetry?.Write(TelemetryCategory.Explore, "lead_card_built",
            new Dictionary<string, object?>
            {
                ["hasSuggestion"] = hasSuggestion,
                ["score"] = score,
            });

    /// <summary>A dot on the neighborhood map was clicked to tune.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="hz">Target frequency.</param>
    public static void MapDotTuned(ITelemetry? telemetry, long hz)
        => telemetry?.Write(TelemetryCategory.Tuning, "map_dot_tuned",
            new Dictionary<string, object?> { ["hz"] = hz });

    /// <summary>A panel was expanded or collapsed (HM-DEC-021).</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="panelKey">Stable panel id, e.g. "spots".</param>
    /// <param name="expanded">True when the panel is now open.</param>
    public static void PanelToggled(ITelemetry? telemetry, string panelKey, bool expanded)
        => telemetry?.Write(TelemetryCategory.Diagnostics, "panel_toggled",
            new Dictionary<string, object?>
            {
                ["panel"] = panelKey,
                ["expanded"] = expanded,
            });
}
