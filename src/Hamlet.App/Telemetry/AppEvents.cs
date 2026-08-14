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

    /// <summary>Hamlet set the radio's mode to match the map.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="mode">The mode asked for, e.g. "Usb".</param>
    /// <param name="dataMode">Whether the data variant was asked for.</param>
    /// <param name="outcome">What the radio said: confirmed, refused, no answer.</param>
    /// <remarks>
    /// THE FIRST WRITE THIS APP MAKES, so its record carries what was asked and
    /// what came back rather than only that something happened (§0.0.1,
    /// HM-DEC-056). Nothing identifying: a mode name and an outcome (HM-DEC-018).
    /// </remarks>
    public static void ModeFollowed(
        ITelemetry? telemetry, string mode, bool dataMode, string outcome)
        => telemetry?.Write(TelemetryCategory.Rig, "mode_followed",
            new Dictionary<string, object?>
            {
                ["mode"] = mode,
                ["dataMode"] = dataMode,
                ["outcome"] = outcome,
            });

    /// <summary>The operator switched the happening-now list's lens.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="lens">"BestChance" or "WhatsNew".</param>
    /// <remarks>
    /// Which question somebody asks and how often is the thing worth knowing
    /// about this control, and it is two words with nothing identifying in
    /// either (HM-DEC-018, HM-DEC-057).
    /// </remarks>
    public static void SpotLensChosen(ITelemetry? telemetry, string lens)
        => telemetry?.Write(TelemetryCategory.Explore, "spot_lens_chosen",
            new Dictionary<string, object?> { ["lens"] = lens });

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

    /// <summary>
    /// Old spots were dropped from history, so the store stays bounded
    /// (HM-DEC-045). Counts only; a spot names a station.
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="removed">How many rows went.</param>
    /// <param name="remaining">How many are left.</param>
    public static void SpotHistoryPruned(ITelemetry? telemetry, int removed, int remaining)
        => telemetry?.Write(TelemetryCategory.Explore, "spot_history_pruned",
            new Dictionary<string, object?>
            {
                ["removed"] = removed,
                ["remaining"] = remaining,
            });

    /// <summary>
    /// The history database could not be opened, so this session remembers
    /// spots in memory only (HM-DEC-045).
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <remarks>
    /// Worth recording rather than swallowing: it is the difference between
    /// "the app forgot" and "the disk would not take it", and only the log can
    /// tell them apart afterwards (§0.0.1).
    /// </remarks>
    public static void SpotHistoryUnavailable(ITelemetry? telemetry)
        => telemetry?.Write(TelemetryCategory.Diagnostics, "spot_history_unavailable",
            new Dictionary<string, object?>(),
            TelemetryLevel.Warn);

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

    /// <summary>
    /// A spot marker on the dial tape was clicked to tune.
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="hz">Target frequency.</param>
    /// <remarks>
    /// Kept apart from the map's event rather than folded into it. The two
    /// surfaces show the same spots at two zoom levels, and which one people
    /// reach for is the question that says whether the tape is earning its
    /// space.
    /// </remarks>
    public static void TapeMarkerTuned(ITelemetry? telemetry, long hz)
        => telemetry?.Write(TelemetryCategory.Tuning, "tape_marker_tuned",
            new Dictionary<string, object?> { ["hz"] = hz });

    /// <summary>
    /// The operator chose a capture device to decode from.
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="looksLikeRadio">
    /// Whether the chosen device's name matches the radio's USB codec.
    /// </param>
    /// <remarks>
    /// The device NAME is deliberately not recorded. It can carry a computer's
    /// name, a person's name, or the model of somebody's headset, and none of
    /// that belongs in a file the operator might paste into a public issue
    /// (HM-DEC-018). Whether Hamlet's guess was the one they kept is the part
    /// that would actually help.
    /// </remarks>
    public static void AudioDeviceChosen(ITelemetry? telemetry, bool looksLikeRadio)
        => telemetry?.Write(TelemetryCategory.Decode, "audio_device_chosen",
            new Dictionary<string, object?> { ["looksLikeRadio"] = looksLikeRadio });

    /// <summary>
    /// The diagnostics screen was opened (HM-DEC-050).
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="knownCount">How many values had actually been read.</param>
    /// <remarks>
    /// The count and nothing else. What the radio is tuned to and how its
    /// filters are set are the operator's business, and none of it belongs in a
    /// file they might paste into a public issue (HM-DEC-018). That somebody
    /// needed this screen, and how much Hamlet knew when they did, is the part
    /// worth recording.
    /// </remarks>
    public static void RigDiagnosticsOpened(ITelemetry? telemetry, int knownCount)
        => telemetry?.Write(TelemetryCategory.Rig, "rig_diagnostics_opened",
            new Dictionary<string, object?> { ["knownValues"] = knownCount });

    /// <summary>
    /// The CW decoder started listening (HM-DEC-048).
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="simulated">Whether the audio is synthesized.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="pitchHz">The pitch it was told to start looking at.</param>
    /// <remarks>
    /// The parameters a decode ran with, which §0.0.1 asks for by name. What is
    /// NOT here, and never will be, is anything that was decoded: the category's
    /// own description says decoder runs and confidence statistics, never
    /// message content, and a transcript names two stations who did not agree to
    /// be in this file (HM-DEC-018).
    /// </remarks>
    public static void DecoderStarted(
        ITelemetry? telemetry, bool simulated, int sampleRate, int pitchHz)
        => telemetry?.Write(TelemetryCategory.Decode, "decoder_started",
            new Dictionary<string, object?>
            {
                ["simulated"] = simulated,
                ["sampleRate"] = sampleRate,
                ["pitchHz"] = pitchHz,
            });

    /// <summary>
    /// The waterfall's spectrum source changed (HM-DEC-026). Records whether
    /// the signals are simulated, because "the waterfall was showing
    /// synthetic signals" is the first thing worth knowing when someone
    /// reports that a decode looked wrong.
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="sourceKind">"training", or the radio's scope.</param>
    /// <param name="bandName">Band being swept, e.g. "40 m".</param>
    /// <param name="simulated">True when the frames are synthesised.</param>
    public static void SpectrumSourceChanged(
        ITelemetry? telemetry, string sourceKind, string bandName, bool simulated)
        => telemetry?.Write(TelemetryCategory.Explore, "spectrum_source_changed",
            new Dictionary<string, object?>
            {
                ["source"] = sourceKind,
                ["band"] = bandName,
                ["simulated"] = simulated,
            });

    /// <summary>
    /// A field-guide audio sample was played (HM-DEC-027). The mode and the
    /// speed, which is the part that says whether the copy-speed ladder is
    /// being used at all.
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="mode">Mode demonstrated, e.g. "Cw".</param>
    /// <param name="wordsPerMinute">CW speed; meaningless for other modes.</param>
    public static void ModeSamplePlayed(
        ITelemetry? telemetry, string mode, int wordsPerMinute)
        => telemetry?.Write(TelemetryCategory.Explore, "mode_sample_played",
            new Dictionary<string, object?>
            {
                ["mode"] = mode,
                ["wpm"] = wordsPerMinute,
            });

    /// <summary>
    /// A license class was resolved from a lookup (HM-DEC-028). The class and
    /// which service answered — never the callsign, which is what was sent to
    /// get this answer and still never enters telemetry (HM-DEC-019).
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="licenseClass">Class name, e.g. "General".</param>
    /// <param name="sourceName">Service that answered.</param>
    public static void LicenseClassResolved(
        ITelemetry? telemetry, string licenseClass, string sourceName)
        => telemetry?.Write(TelemetryCategory.Diagnostics, "license_class_resolved",
            new Dictionary<string, object?>
            {
                ["class"] = licenseClass,
                ["source"] = sourceName,
            });

    /// <summary>
    /// A lookup disagreed with a hand-set class, so the operator was asked.
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="found">What the lookup said.</param>
    /// <param name="existing">What the operator had set.</param>
    public static void LicenseClassMismatch(
        ITelemetry? telemetry, string found, string existing)
        => telemetry?.Write(TelemetryCategory.Diagnostics, "license_class_mismatch",
            new Dictionary<string, object?>
            {
                ["found"] = found,
                ["existing"] = existing,
            },
            TelemetryLevel.Warn);

    /// <summary>A license lookup did not produce a class.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="outcome">"NotFound" or "Unavailable".</param>
    public static void LicenseClassLookupFailed(ITelemetry? telemetry, string outcome)
        => telemetry?.Write(TelemetryCategory.Diagnostics, "license_lookup_failed",
            new Dictionary<string, object?> { ["outcome"] = outcome },
            TelemetryLevel.Warn);

    /// <summary>
    /// The transmit guard rail was switched on or off (HM-DEC-029).
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="enabled">True when Hamlet will hold transmissions back.</param>
    public static void TransmitGuardToggled(ITelemetry? telemetry, bool enabled)
        => telemetry?.Write(TelemetryCategory.Diagnostics, "transmit_guard_toggled",
            new Dictionary<string, object?> { ["enabled"] = enabled });

    /// <summary>The upgrade ladder was opened from the band-map status line.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="fromClass">The class the operator holds now.</param>
    public static void UpgradeLadderOpened(ITelemetry? telemetry, string fromClass)
        => telemetry?.Write(TelemetryCategory.Explore, "upgrade_ladder_opened",
            new Dictionary<string, object?> { ["fromClass"] = fromClass });

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
