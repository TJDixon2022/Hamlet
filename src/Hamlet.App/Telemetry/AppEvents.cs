using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Rig;
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

    /// <summary>A mode family was switched on or off in the spot list.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="family">"Cw", "Digital" or "Phone".</param>
    /// <param name="isOn">Whether it is now shown.</param>
    public static void SpotFamilyToggled(ITelemetry? telemetry, string family, bool isOn)
        => telemetry?.Write(TelemetryCategory.Explore, "spot_family_toggled",
            new Dictionary<string, object?> { ["family"] = family, ["on"] = isOn });

    /// <summary>A frequency was saved as a favorite (HM-DEC-060).</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="bandName">Which band it is on.</param>
    /// <remarks>
    /// The band and nothing else. A frequency somebody saved is closer to a
    /// habit than a setting, and the band answers "which bands does this person
    /// use" without recording where they sit (HM-DEC-018).
    /// </remarks>
    public static void FavoriteSaved(ITelemetry? telemetry, string bandName)
        => telemetry?.Write(TelemetryCategory.Tuning, "favorite_saved",
            new Dictionary<string, object?> { ["band"] = bandName });

    /// <summary>The calling cycle was armed (HM-DEC-098, §0.0.1).</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="rounds">How many rounds it was armed for.</param>
    /// <param name="intervalSeconds">How long each round is.</param>
    /// <param name="messageLength">
    /// How many characters the message is. **The length and never the text**: the
    /// message carries the operator's callsign and HM-DEC-018 keeps that out of
    /// the record without exception.
    /// </param>
    public static void AutoCallArmed(
        ITelemetry? telemetry, int rounds, double intervalSeconds, int messageLength)
        => telemetry?.Write(TelemetryCategory.Rig, "auto_call_armed",
            new Dictionary<string, object?>
            {
                ["rounds"] = rounds,
                ["intervalSeconds"] = intervalSeconds,
                ["messageLength"] = messageLength,
            });

    /// <summary>The calling cycle started transmitting.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="rounds">How many rounds it will run at most.</param>
    public static void AutoCallStarted(ITelemetry? telemetry, int rounds)
        => telemetry?.Write(TelemetryCategory.Rig, "auto_call_started",
            new Dictionary<string, object?> { ["rounds"] = rounds });

    /// <summary>One round of the cycle went out.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="round">Which round, counting from one.</param>
    /// <param name="hz">Where the radio was.</param>
    /// <remarks>
    /// **THIS IS WHAT TELLS A CYCLE ROUND FROM THREE MANUAL PRESSES.** Before it
    /// existed the record carried the same three send events either way, so
    /// nobody could say from a log whether the cycle had run at all (§0.0.1).
    /// </remarks>
    public static void AutoCallRound(ITelemetry? telemetry, int round, long hz)
        => telemetry?.Write(TelemetryCategory.Rig, "auto_call_round",
            new Dictionary<string, object?> { ["round"] = round, ["hz"] = hz });

    /// <summary>The cycle stopped, and why.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="cause">The stop, as a stable token.</param>
    /// <param name="rounds">How many rounds went out before it stopped.</param>
    /// <param name="answered">
    /// True only where what was heard was shaped like somebody answering.
    /// </param>
    /// <remarks>
    /// **THE REASON IS THE POINT OF THE EVENT** (HM-DEC-077). Every interlock in
    /// `BENCH_CARD.md` has a sentence it is supposed to give, and this carries the
    /// same one as a stable token, so an evening at the dummy load produces
    /// evidence rather than a notebook. A cause is never a display string.
    /// </remarks>
    public static void AutoCallStopped(
        ITelemetry? telemetry, string cause, int rounds, bool answered)
        => telemetry?.Write(TelemetryCategory.Rig, "auto_call_stopped",
            new Dictionary<string, object?>
            {
                ["cause"] = cause,
                ["rounds"] = rounds,
                ["answered"] = answered,
            });

    /// <summary>A place was written into the recent list (HM-OPEN-039).</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="hz">Where it was.</param>
    /// <param name="named">
    /// Whether a station was identified there. **Whether and never who**
    /// (HM-DEC-073, HM-DEC-018).
    /// </param>
    public static void RecentRemembered(ITelemetry? telemetry, long hz, bool named)
        => telemetry?.Write(TelemetryCategory.Tuning, "recent_remembered",
            new Dictionary<string, object?> { ["hz"] = hz, ["named"] = named });

    /// <summary>A return folded into an entry already in the list.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="hz">Where the operator arrived.</param>
    /// <param name="existingHz">The entry it folded into.</param>
    /// <param name="gapHz">How far apart they were.</param>
    /// <param name="visits">How many visits that entry now carries.</param>
    /// <remarks>
    /// **THE ONE THAT PROVES THE TOLERANCE.** HM-DEC-072 rules two hundred hertz
    /// is one place and HM-DEC-134 rests on it, and neither could be checked
    /// against a live session while the fold left no trace: a list with no
    /// duplicates and a list that never folded anything look identical.
    /// </remarks>
    public static void RecentFolded(
        ITelemetry? telemetry, long hz, long existingHz, long gapHz, int visits)
        => telemetry?.Write(TelemetryCategory.Tuning, "recent_folded",
            new Dictionary<string, object?>
            {
                ["hz"] = hz,
                ["existingHz"] = existingHz,
                ["gapHz"] = gapHz,
                ["visits"] = visits,
            });

    /// <summary>The dial moved on before the dwell was met.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="hz">Where it was.</param>
    /// <param name="shortBySeconds">How far short of the dwell it fell.</param>
    /// <remarks>
    /// **A LIST THAT STAYS EMPTY WHILE SOMEBODY SITS STILL LOOKS IDENTICAL TO A
    /// BROKEN ONE**, which is why the near miss is recorded and not only the hit.
    /// </remarks>
    public static void RecentDwellShort(
        ITelemetry? telemetry, long hz, double shortBySeconds)
        => telemetry?.Write(TelemetryCategory.Tuning, "recent_dwell_short",
            new Dictionary<string, object?>
            {
                ["hz"] = hz,
                ["shortBySeconds"] = shortBySeconds,
            });

    /// <summary>An entry fell off the end of the list.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="hz">Where it was.</param>
    public static void RecentDropped(ITelemetry? telemetry, long hz)
        => telemetry?.Write(TelemetryCategory.Tuning, "recent_dropped",
            new Dictionary<string, object?> { ["hz"] = hz });

    /// <summary>The operator removed a recent entry, or all of them (HM-DEC-134).</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="all">True for a clear, false for one entry.</param>
    /// <param name="removed">How many went.</param>
    public static void RecentRemoved(ITelemetry? telemetry, bool all, int removed)
        => telemetry?.Write(TelemetryCategory.Tuning, "recent_removed",
            new Dictionary<string, object?> { ["all"] = all, ["removed"] = removed });

    /// <summary>The frequency write itself, which had no event at all.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="hz">Where the radio was told to go.</param>
    /// <param name="outcome">`proceeded` or `failed`.</param>
    /// <param name="seconds">How long the command took, or null.</param>
    /// <remarks>
    /// **`tune_requested` SAID SOMEBODY ASKED AND NOTHING SAID WHAT HAPPENED.**
    /// The one command in the middle of every tune left no trace, so a display
    /// disagreeing with the radio could not be placed on either side of it: it
    /// was impossible to tell a write that never went from one that went and was
    /// then undone by a stale reading (§0.0.1).
    /// </remarks>
    public static void TuneWritten(
        ITelemetry? telemetry, long hz, string outcome, double? seconds)
        => telemetry?.Write(
            TelemetryCategory.Tuning,
            "tune_written",
            new Dictionary<string, object?>
            {
                ["outcome"] = outcome,
                ["reason"] = outcome == "proceeded" ? "radio_took_it" : "write_failed",
                ["hz"] = hz,
                ["seconds"] = seconds is { } t ? Math.Round(t, 3) : null,
            },
            outcome == "proceeded" ? TelemetryLevel.Info : TelemetryLevel.Warn);

    /// <summary>The displayed frequency went back to where it had been.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="hz">The value it went back to.</param>
    /// <param name="askedFor">Where the operator had just tuned.</param>
    /// <param name="source">What produced the reading that did it.</param>
    /// <param name="sinceTuneSeconds">How long after the tune, or null.</param>
    /// <remarks>
    /// **THIS IS THE SIGNATURE OF THE WHOLE DEFECT AND IT IS ONE LINE.** A
    /// frequency returning to a value it held moments ago, right after a write,
    /// is not an ordinary observation: the radio does not tune itself backwards.
    /// The operator watched it happen on every tune from the app for two builds
    /// and nothing in the record could have been searched for it, because nothing
    /// emitted it.
    /// </remarks>
    public static void FrequencyWentBackwards(
        ITelemetry? telemetry, long hz, long askedFor, string source,
        double? sinceTuneSeconds)
        => telemetry?.Write(
            TelemetryCategory.Tuning,
            "frequency_went_backwards",
            new Dictionary<string, object?>
            {
                ["outcome"] = "degraded",
                ["reason"] = "reading_older_than_the_tune",
                ["hz"] = hz,
                ["askedFor"] = askedFor,
                ["determinedBy"] = source,
                ["secondsSinceTune"] = sinceTuneSeconds is { } t
                    ? Math.Round(t, 3)
                    : null,
            },
            TelemetryLevel.Warn);

    /// <summary>A favorite was removed.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="bandName">Which band it was on.</param>
    public static void FavoriteRemoved(ITelemetry? telemetry, string bandName)
        => telemetry?.Write(TelemetryCategory.Tuning, "favorite_removed",
            new Dictionary<string, object?> { ["band"] = bandName });

    /// <summary>A favorite was tuned to.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="bandName">Which band it is on.</param>
    public static void FavoriteTuned(ITelemetry? telemetry, string bandName)
        => telemetry?.Write(TelemetryCategory.Tuning, "favorite_tuned",
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
    /// <param name="deviceChoice">
    /// Which of the five rules picked the capture device, or null where no device
    /// was chosen at all — the training radio, which opens none (unit 236).
    /// </param>
    /// <param name="looksLikeRadio">
    /// Whether the chosen device's name matches the radio's USB codec, or null
    /// where nothing was chosen.
    /// </param>
    /// <param name="captureDevicesOffered">
    /// How many capture devices the machine offered, or null where the list was
    /// never asked for.
    /// </param>
    /// <remarks>
    /// <para>The parameters a decode ran with, which §0.0.1 asks for by name. What
    /// is NOT here, and never will be, is anything that was decoded: the category's
    /// own description says decoder runs and confidence statistics, never
    /// message content, and a transcript names two stations who did not agree to
    /// be in this file (HM-DEC-018).</para>
    /// <para>**AND SINCE UNIT 236 IT SAYS WHY THIS DEVICE** — never which one.
    /// Hamlet chooses the capture device for the operator on every launch, from a
    /// remembered id unit 235 measured to be absent from his settings, and until
    /// now no file anywhere said which of the five rules had run. A machine with no
    /// USB codec in the list and nothing remembered falls through to the system
    /// default, which is Hamlet listening to a laptop microphone all morning while
    /// every layer below it works perfectly.</para>
    /// <para>**THE BRANCH IS RECORDED AND THE DEVICE IS NOT** (HM-DEC-018), which
    /// is the shape <c>AudioDeviceChosen</c> above already has and for the same
    /// reason: a device name can carry a computer's name, a person's name or the
    /// model of somebody's headset.</para>
    /// <para>**THE THREE NEW PARAMETERS ARE OPTIONAL SO EVERY EXISTING CALLER
    /// KEEPS COMPILING AND KEEPS ITS MEANING**, which is unit 233's precedent for
    /// adding beside rather than substituting. This was extended rather than given
    /// an event of its own because it is one fact about one moment: a separate line
    /// could be present when this one is missing, and a reader joining two lines by
    /// timestamp is exactly the ambiguity §0.0.1 exists to prevent.</para>
    /// </remarks>
    public static void DecoderStarted(
        ITelemetry? telemetry,
        bool simulated,
        int sampleRate,
        int pitchHz,
        AudioDeviceChoiceReason? deviceChoice = null,
        bool? looksLikeRadio = null,
        int? captureDevicesOffered = null)
        => telemetry?.Write(TelemetryCategory.Decode, "decoder_started",
            new Dictionary<string, object?>
            {
                ["simulated"] = simulated,
                ["sampleRate"] = sampleRate,
                ["pitchHz"] = pitchHz,
                ["deviceChoice"] = deviceChoice?.ToString(),
                ["looksLikeRadio"] = looksLikeRadio,
                ["captureDevicesOffered"] = captureDevicesOffered,
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

    // ---- Decisions, not only completions (HM-DEC-077) --------------------

    /// <summary>
    /// Hamlet evaluated whether a send could reach the air.
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="readiness">The verdict, carrying what decided it.</param>
    /// <param name="state">Rig state as Hamlet believed it.</param>
    /// <param name="trigger">What caused the evaluation, as a stable token.</param>
    /// <remarks>
    /// <para>THE EVENT THE EVENING THIS WAS WRITTEN NEEDED AND DID NOT HAVE. A
    /// disabled button fires no handler, so nothing was written, so the record
    /// could not tell "Hamlet refused" from "Hamlet is broken" from "nobody
    /// pressed it". This fires when readiness recomputes rather than when
    /// something is pressed, which is what makes the absence of a press
    /// visible.</para>
    /// <para>NO MESSAGE TEXT, EVER (HM-DEC-018). It names preconditions and
    /// carries numbers. There is no parameter here that could hold what was
    /// being sent.</para>
    /// </remarks>
    public static void TransmitReadinessEvaluated(
        ITelemetry? telemetry, CwReadiness readiness, RigState state, string trigger)
    {
        if (telemetry is null || readiness is null || state is null)
        {
            return;
        }

        var body = readiness.AsEvent();

        telemetry.Write(
            TelemetryCategory.Rig, "transmit_readiness",
            body.ToBag(Merge(
                new Dictionary<string, object?>
                {
                    ["trigger"] = trigger,
                    ["state"] = readiness.State.ToString(),
                },
                RigSnapshot.Full(state, DateTime.UtcNow))),
            body.Level);
    }

    /// <summary>
    /// The rig state as Hamlet holds it, on a slow heartbeat.
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="previous">What was reported last time, or null.</param>
    /// <param name="state">What Hamlet knows now.</param>
    /// <remarks>
    /// A DELTA, SO A QUIET SESSION STILL HAS A SPINE. The long session that
    /// prompted this ran nearly two hours with its last human action in the
    /// first five minutes, and nothing in between said what the radio was doing.
    /// </remarks>
    public static void RigHeartbeat(
        ITelemetry? telemetry, RigState? previous, RigState state)
    {
        if (telemetry is null || state is null)
        {
            return;
        }

        telemetry.Write(
            TelemetryCategory.Rig, "rig_heartbeat",
            RigSnapshot.Delta(previous, state, DateTime.UtcNow));
    }

    /// <summary>
    /// A connection outcome, with the state it produced.
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="outcome">What happened.</param>
    /// <param name="reason">A stable token for why.</param>
    /// <param name="port">Port name or the simulated-rig entry.</param>
    /// <param name="rigType">"simulated" or the model.</param>
    /// <param name="state">Rig state after it, or null.</param>
    /// <param name="unrequested">
    /// True when nobody asked for this, which makes it a warning however well
    /// it went (HM-DEC-077).
    /// </param>
    /// <remarks>
    /// A RECONNECT NOBODY ASKED FOR IS A WARNING. The evening this was written,
    /// a second connect fired thirteen seconds after the first with the decoder
    /// restarting alongside it, and it was logged identically to a healthy one.
    /// </remarks>
    public static void ConnectOutcome(
        ITelemetry? telemetry, Outcome outcome, string reason,
        string port, string rigType, RigState? state, bool unrequested = false)
    {
        if (telemetry is null)
        {
            return;
        }

        var body = new OutcomeEvent(outcome, reason, Array.Empty<DeterminedBy>());

        var extra = new Dictionary<string, object?>
        {
            ["port"] = port,
            ["rigType"] = rigType,
            ["unrequested"] = unrequested,
        };

        if (state is not null)
        {
            foreach (var pair in RigSnapshot.Full(state, DateTime.UtcNow))
            {
                extra[pair.Key] = pair.Value;
            }
        }

        var level = unrequested && body.Level == TelemetryLevel.Info
            ? TelemetryLevel.Warn
            : body.Level;

        telemetry.Write(
            TelemetryCategory.Rig, "connect_outcome", body.ToBag(extra), level);
    }

    /// <summary>
    /// A rig read did not come back.
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="field">Which field, as its name.</param>
    /// <param name="command">The CI-V command, e.g. "16 47".</param>
    public static void RigReadTimedOut(
        ITelemetry? telemetry, string field, string command)
        => telemetry?.Write(
            TelemetryCategory.Rig, "rig_read_timeout",
            new Dictionary<string, object?>
            {
                ["outcome"] = "failed",
                ["reason"] = "timeout",
                ["field"] = field,
                ["command"] = command,
            },
            TelemetryLevel.Warn);

    /// <summary>
    /// What the decoder has been hearing, aggregated over an interval.
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="window">The counts and measurements.</param>
    /// <remarks>
    /// COUNTS AND MEASUREMENTS, NEVER TEXT (HM-DEC-018). The window carries how
    /// many characters were emitted at each confidence and why things were
    /// rejected. It has no member that can hold a decoded character, which is
    /// deliberate: the shape refuses rather than the call site remembering.
    /// </remarks>
    public static void DecodeWindow(ITelemetry? telemetry, DecodeWindow? window)
    {
        if (telemetry is null || window is null || window.IsEmpty)
        {
            return;
        }

        telemetry.Write(
            TelemetryCategory.Decode, "decode_window", window.ToBag(),
            window.Level);
    }
    /// <summary>A ratio rounded for the record, or null where none was taken.</summary>
    /// <remarks>
    /// **NULL AND NOT NaN, AND NOT ZERO.** A JSON NaN is not valid JSON in most
    /// readers, and a zero would read as a sound card that delivered nothing
    /// (HM-DEC-009). Absent is the honest entry for a measurement nobody took.
    /// </remarks>
    private static object? Ratio(double value)
        => double.IsNaN(value) ? null : Math.Round(value, 4);



    /// <summary>
    /// What every FT8 slot the tab decoded gave up, one line each (unit 233).
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="slots">The census, one entry per slot that was cut and run.</param>
    /// <param name="refusal">
    /// Why nothing could be run, in the reader's own words, or "" where it ran.
    /// </param>
    /// <param name="offset">The clock offset the reading was cut against.</param>
    /// <param name="nowUtc">
    /// <param name="arrival">What the audio path delivered, as counts.</param>
    /// The moment the line is written, by the PC clock, used only to age the clock
    /// measurement. Passed rather than read, so the payload is a function of its
    /// arguments.
    /// </param>
    /// <remarks>
    /// <para>**THIS IS THE ARTEFACT THAT SURVIVES AN UNATTENDED SESSION**, and it
    /// is the whole reason unit 233 exists. On 2026-09-03 the owner sat at 14.074,
    /// got an empty table, and the machine kept no record of it at all: the capture
    /// folder was never created, and the only thing telemetry had ever said about a
    /// decode was a CW confidence count. Since unit 225 the tab decodes every slot
    /// with no press, so a morning at the radio can produce 240 of these an hour or
    /// none — and *none* is itself readable.</para>
    /// <para>**A SLOT THAT REFUSED IS AN EVENT, NOT A SILENCE.** Where the reader
    /// could not cut anything, its sentence is written verbatim and no slot lines
    /// are. Where it cut slots, there is one line per slot whether or not the slot
    /// produced text.</para>
    /// <para>**COUNTS AND MEASUREMENTS, NEVER TEXT** (HM-DEC-018), and the signature
    /// is what enforces it. <see cref="Ft8Reception"/> is deliberately *not* the
    /// parameter here even though the call site holds one: it carries
    /// <see cref="Ft8Decode"/> rows, and an FT8 message is very often a pair of
    /// callsigns. <see cref="Ft8SlotCensus"/> has no member that can hold a
    /// character, so the shape refuses rather than this method remembering — the
    /// same reasoning that keeps <see cref="DecodeWindow"/> unable to hold a decoded
    /// letter. The refusal sentence is Hamlet's own words about its own state and is
    /// not anything that was on the air.</para>
    /// <para>**A COSTAS MATCH COUNT IS NOT A SIGNAL-TO-NOISE RATIO** (`CLAUDE.md`
    /// §0.0), and the key says so. There is no `snr` in this payload and none is
    /// invented: a plausible number under that heading would be read as a
    /// measurement by everyone downstream.</para>
    /// <para>**AND SINCE UNIT 236 IT SAYS WHETHER THERE WAS ANY AUDIO AT ALL.**
    /// Every field above describes the decode, so a muted sound card, a radio with
    /// its USB cable out, a laptop microphone in a quiet room and a band with no
    /// decodable FT8 in it produced byte-identical lines — which is the fork the
    /// bench check of 2026-09-03 died on. The level fields are a measurement of the
    /// audio the slot was cut from, taken at the rate it arrived, and the count of
    /// samples that were exactly zero is what separates a dead input from a quiet
    /// one. **A level is not a signal-to-noise ratio** (`CLAUDE.md` §0.0) and an
    /// all-zero slot writes null rather than a floor (HM-DEC-009).</para>
    /// <para>**IT COUNTS AND IT DOES NOT INTERPRET** (`CLAUDE.md` §12.1). Nothing
    /// here concludes that the band was quiet or that a station was weak.</para>
    /// </remarks>
    public static void Ft8SlotsRead(
        ITelemetry? telemetry,
        IReadOnlyList<Ft8SlotCensus>? slots,
        string refusal,
        ClockOffset offset,
        DateTime nowUtc,
        AudioArrival arrival = default)
    {
        if (telemetry is null)
        {
            return;
        }

        var age = offset.Age(nowUtc);

        if (!string.IsNullOrEmpty(refusal))
        {
            telemetry.Write(
                TelemetryCategory.Decode,
                "ft8_slot",
                new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["outcome"] = "refused",

                    // Verbatim, because a paraphrase of a refusal is a different
                    // refusal and the operator is reading the same sentence.
                    ["refusal"] = refusal,
                    ["clockOffsetSeconds"] = offset.OffsetSeconds is { } known
                        ? Math.Round(known, 3)
                        : null,
                    ["clockOffsetAgeSeconds"] = age is { } old
                        ? Math.Round(old.TotalSeconds, 1)
                        : null,

                    // **THE AUDIO PATH'S OWN NUMBERS, ON THE REFUSAL TOO.** A
                    // refusal that cannot say whether the audio arrived is the
                    // line that read `nothing decoded yet` for an evening while
                    // the tap was filling at 13% of real time.
                    ["arrivalRatio"] = Ratio(arrival.RecentRatio),
                    ["slotArrivalRatio"] = Ratio(arrival.SlotRatio),
                    ["queueDroppedChunks"] = arrival.QueueDroppedChunks,
                    ["queueDroppedSamples"] = arrival.QueueDroppedSamples,
                    ["callbackFailures"] = arrival.CallbackFailures,
                    ["emptyBuffers"] = arrival.EmptyBuffers,
                    ["longestCallbackMicroseconds"] =
                        Math.Round(arrival.LongestCallbackMicroseconds, 0),

                    // **THE BUDGET THAT FIGURE IS MEASURED AGAINST, AND HOW OFTEN
                    // IT WAS MISSED.** Unit 238 recorded the longest callback and
                    // nothing to compare it to, so 91,372 us read as a
                    // catastrophe against a budget the device never had. The
                    // count of overruns is the number that means something on its
                    // own; the count over half the period is the one that moves
                    // first, while the machine is close and still working.
                    ["bufferPeriodMicroseconds"] =
                        Math.Round(arrival.BufferPeriodMicroseconds, 0),
                    ["callbacksOverPeriod"] = arrival.CallbacksOverPeriod,
                    ["callbacksOverHalfPeriod"] = arrival.CallbacksOverHalfPeriod,
                    ["callbacksTimed"] = arrival.CallbacksTimed,

                    // Unit 240: the waterfall's transform moved off the device
                    // callback, so a slow frame costs a row rather than audio.
                    // Both numbers travel together, because a drop count with no
                    // frame duration cannot say whether the machine is coping.
                    ["droppedFrames"] = arrival.DroppedFrames,
                    ["longestFrameMicroseconds"] =
                        Math.Round(arrival.LongestFrameMicroseconds, 0),
                },
                TelemetryLevel.Warn);

            return;
        }

        if (slots is null)
        {
            return;
        }

        foreach (var slot in slots)
        {
            // **THE ONE CASE WORTH FINDING BY SCANNING**: the search found places
            // and not one of them became words. A slot with nothing in it is the
            // ordinary state of a receiver and is written at info, in the manner
            // DecodeWindow already levels a quiet band.
            var foundAndNotRead =
                slot.CandidateCount > 0 && slot.BecameTextCount == 0;

            telemetry.Write(
                TelemetryCategory.Decode,
                "ft8_slot",
                new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["outcome"] = "decoded",
                    ["slotStartUtc"] = slot.SlotStartUtc.ToString(
                        "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                    ["candidates"] = slot.CandidateCount,
                    ["paritySatisfied"] = slot.ParitySatisfiedCount,
                    ["checksumPassed"] = slot.ChecksumPassedCount,
                    ["becameText"] = slot.BecameTextCount,
                    ["duplicates"] = slot.DuplicateCount,
                    ["topCostasMatchCounts"] = slot.TopSyncScores,

                    // The same counters on a slot that decoded, because a slot
                    // that produced candidates and no text is exactly the case
                    // where the question is whether it was ever whole audio.
                    ["arrivalRatio"] = Ratio(arrival.RecentRatio),
                    ["slotArrivalRatio"] = Ratio(arrival.SlotRatio),
                    ["queueDroppedChunks"] = arrival.QueueDroppedChunks,
                    ["queueDroppedSamples"] = arrival.QueueDroppedSamples,
                    ["callbackFailures"] = arrival.CallbackFailures,
                    ["emptyBuffers"] = arrival.EmptyBuffers,
                    ["longestCallbackMicroseconds"] =
                        Math.Round(arrival.LongestCallbackMicroseconds, 0),

                    // **THE BUDGET THAT FIGURE IS MEASURED AGAINST, AND HOW OFTEN
                    // IT WAS MISSED.** Unit 238 recorded the longest callback and
                    // nothing to compare it to, so 91,372 us read as a
                    // catastrophe against a budget the device never had. The
                    // count of overruns is the number that means something on its
                    // own; the count over half the period is the one that moves
                    // first, while the machine is close and still working.
                    ["bufferPeriodMicroseconds"] =
                        Math.Round(arrival.BufferPeriodMicroseconds, 0),
                    ["callbacksOverPeriod"] = arrival.CallbacksOverPeriod,
                    ["callbacksOverHalfPeriod"] = arrival.CallbacksOverHalfPeriod,
                    ["callbacksTimed"] = arrival.CallbacksTimed,

                    // Unit 240: the waterfall's transform moved off the device
                    // callback, so a slow frame costs a row rather than audio.
                    // Both numbers travel together, because a drop count with no
                    // frame duration cannot say whether the machine is coping.
                    ["droppedFrames"] = arrival.DroppedFrames,
                    ["longestFrameMicroseconds"] =
                        Math.Round(arrival.LongestFrameMicroseconds, 0),
                    ["sampleRate"] = slot.SampleRate,

                    // Unit 249: which decoder read this slot and which stages
                    // were on. An unrecorded one writes null rather than naming
                    // the port, because naming a decoder nobody recorded would
                    // put a false attribution in the record that exists to
                    // settle attribution.
                    ["decoder"] = slot.Decoder.IsRecorded
                        ? slot.Decoder.Name
                        : null,
                    ["decoderFineSync"] = slot.Decoder.FineSync,
                    ["decoderOrderedStatistics"] = slot.Decoder.OrderedStatistics,

                    // The port's own counts beside them, where the comparison
                    // was asked for. Null is "nobody asked" and never a zero,
                    // which would read as "the port found nothing".
                    ["portMessages"] = slot.PortComparison?.Messages,
                    ["portCandidates"] = slot.PortComparison?.CandidateCount,
                    ["portParitySatisfied"] = slot.PortComparison?.ParitySatisfiedCount,
                    ["portChecksumPassed"] = slot.PortComparison?.ChecksumPassedCount,
                    ["portBecameText"] = slot.PortComparison?.BecameTextCount,
                    ["portMilliseconds"] = slot.PortComparison is null
                        ? null
                        : Math.Round(slot.PortComparison.Value.Milliseconds, 0),

                    // **HOW LOUD THE AUDIO WAS, WHICH NONE OF THE ABOVE COULD
                    // SAY** (unit 236). Everything before this line describes the
                    // decode. On 2026-09-03 the phase's closing line was performed
                    // at the radio and produced nothing, and four units could not
                    // establish whether any audio had reached the decoder at all,
                    // because a muted sound card and a quiet band write the same
                    // line here.
                    //
                    // **A LEVEL IS NOT A SIGNAL-TO-NOISE RATIO** (`CLAUDE.md`
                    // §0.0) and the keys say what they are: decibels relative to
                    // full scale. There is no `snr` in this payload, no `signal`
                    // and no `strength`, and nothing here is comparable with this
                    // mode's published sensitivity figure.
                    //
                    // **NULL WHERE THE SLOT WAS ALL ZEROS** (HM-DEC-009). The
                    // logarithm of nought is not a number, and a floor written in
                    // its place is a measurement somebody will average.
                    // `audioZeroSamples` standing at `audioSamples` is what says
                    // why, and it is the one field that separates a dead input
                    // from a quiet one.
                    ["audioPeakDbFullScale"] = slot.Level.PeakDbFullScale is { } peak
                        ? Math.Round(peak, 2)
                        : null,
                    ["audioRmsDbFullScale"] = slot.Level.RmsDbFullScale is { } rms
                        ? Math.Round(rms, 2)
                        : null,
                    ["audioSamples"] = slot.Level.SampleCount,
                    ["audioZeroSamples"] = slot.Level.ZeroSampleCount,
                    ["audioZeroSampleFraction"] =
                        slot.Level.ZeroSampleFraction is { } zeros
                            ? Math.Round(zeros, 6)
                            : null,

                    ["clockOffsetSeconds"] = offset.OffsetSeconds is { } known
                        ? Math.Round(known, 3)
                        : null,
                    ["clockOffsetAgeSeconds"] = age is { } old
                        ? Math.Round(old.TotalSeconds, 1)
                        : null,
                },
                foundAndNotRead ? TelemetryLevel.Warn : TelemetryLevel.Info);
        }
    }

    /// <summary>
    /// Hamlet asked the radio to send its spectrum (HM-DEC-092).
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="outcome">What the radio answered.</param>
    /// <param name="baudRate">The rate the link is running at.</param>
    /// <param name="unanswered">Commands the link has not answered.</param>
    /// <remarks>
    /// **THERE WAS NO ATTEMPT ANYWHERE IN THE LOG.** The panel read the setting,
    /// found it off, printed advice and stopped, and nothing recorded that it had
    /// declined to act. A refusal is an outcome and is as loggable as a success
    /// (§8.1).
    /// </remarks>
    public static void ScopeOutputRequested(
        ITelemetry? telemetry, string outcome, int baudRate, long unanswered)
        => telemetry?.Write(
            TelemetryCategory.Rig,
            "scope_output_requested",
            new Dictionary<string, object?>
            {
                // **THE TWO FIELDS SAID OPPOSITE THINGS IN THE SAME EVENT.**
                // It logged `outcome: failed` with `reason: confirmed` and no
                // unanswered commands, while 2,748 scope frames were arriving:
                // the write plainly worked and the record called it a failure.
                // The caller was changed to pass the stable token and this line
                // was still comparing against the enum's name, which is exactly
                // what a stable token is for and exactly how it gets missed.
                ["outcome"] = string.Equals(outcome, "confirmed", StringComparison.Ordinal)
                    ? "proceeded"
                    : "failed",

                // **THE REASON IS THE RUNG OF THE LADDER** (FACT-003). Silence
                // and a readback that disagreed are different faults in
                // different places, and they arrived under one word for six
                // connects.
                ["reason"] = outcome,
                ["command"] = "27 11",
                ["baudRate"] = baudRate,
                ["unansweredCommands"] = unanswered,
            },
            string.Equals(outcome, "Confirmed", StringComparison.Ordinal)
                ? TelemetryLevel.Info
                : TelemetryLevel.Warn);

    /// <summary>
    /// How the CI-V conversation is going (HM-DEC-092).
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="link">What the link reports about itself.</param>
    /// <param name="sweeps">Spectrum sweeps that have arrived.</param>
    /// <param name="dropped">Sweeps that arrived incomplete and were dropped.</param>
    /// <remarks>
    /// A panel that has data and a panel that has never had data are different
    /// states, and so are a link answering everything and one dropping commands.
    /// On this station radio frequency energy knocks USB devices off the bus, so
    /// a link that stops answering mid-send is expected rather than mysterious.
    /// </remarks>
    public static void CivLink(
        ITelemetry? telemetry, CivLinkHealth link, long sweeps, long dropped)
        => telemetry?.Write(
            TelemetryCategory.Rig,
            "civ_link",
            new Dictionary<string, object?>
            {
                ["outcome"] = link.IsHealthy ? "proceeded" : "degraded",
                ["reason"] = link.IsHealthy ? "answering" : "commands_unanswered",
                ["port"] = link.PortName,
                ["baudRate"] = link.BaudRate,
                ["sent"] = link.Sent,
                ["answered"] = link.Answered,
                ["unanswered"] = link.Unanswered,
                ["lastUnansweredCommand"] = link.LastUnansweredCommand is { } c
                    ? $"{c:X2}"
                    : null,
                ["sweeps"] = sweeps,
                ["sweepsDropped"] = dropped,

                // **WHAT ARRIVED, BEFORE ANYTHING FILTERED IT.** Everything above
                // is about commands Hamlet sent. Nothing was about what the radio
                // says on its own, which is what decides whether the frequency on
                // screen follows the dial in a tenth of a second or in thirty.
                ["inbound"] = link.Inbound,
                ["inboundFromRadio"] = link.InboundFromRadio,
                ["inboundBroadcast"] = link.InboundBroadcast,
                ["inboundTransceive"] = link.InboundTransceive,
                ["inboundScope"] = link.InboundScope,
                ["inboundBytes"] = link.InboundBytes,
                ["scopeShare"] = link.ScopeShare is { } share
                    ? Math.Round(share, 3)
                    : null,

                // Null before anything has arrived at all, because a quiet link
                // and a radio that is not broadcasting are different facts.
                ["radioIsBroadcasting"] = link.IsRadioBroadcasting,
                ["secondsSinceBroadcast"] = link.LastBroadcastUtc is { } b
                    ? Math.Round((DateTime.UtcNow - b).TotalSeconds, 1)
                    : null,
            },
            link.IsHealthy ? TelemetryLevel.Info : TelemetryLevel.Warn);

    /// <summary>
    /// How well the decoder is doing, and on what (HM-DEC-088).
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="report">What the decoder measured.</param>
    /// <param name="reason">Why this was written, as a stable token.</param>
    /// <remarks>
    /// <para>**THE OPERATOR ASKED FOR THIS EXPLICITLY: the decoder work is
    /// experimental and has to be measurable, or nobody can tell whether it
    /// helped.** So every figure a change could move is here, and every one of
    /// them is a measurement of the audio rather than a claim about a station
    /// (§0.0). No speed, no callsign, no text.</para>
    /// <para>**RATE-LIMITED BY THE CALLER, NOT HERE**, and the reason is a real
    /// fault: the file that prompted HM-DEC-077 wrote the same unchanged state
    /// twice per Morse element and buried everything that mattered. What decides
    /// when this is worth writing is which of these numbers moved, and only the
    /// caller knows that.</para>
    /// </remarks>
    public static void DecodeQuality(
        ITelemetry? telemetry, CwDecodeReport report, string reason)
        => telemetry?.Write(
            TelemetryCategory.Decode,
            "decode_quality",
            new Dictionary<string, object?>
            {
                ["reason"] = reason,
                ["inputPeakDb"] = Math.Round(report.Level.PeakDb, 1),
                ["inputRmsDb"] = Math.Round(report.Level.RmsDb, 1),
                ["inputFloorDb"] = Math.Round(report.Level.FloorDb, 1),
                ["clipping"] = report.Clipping,
                ["nearlySilent"] = report.NearlySilent,

                // **AN UNSPACED TRANSCRIPT AND AN EMPTY ONE ARE DIFFERENT FACTS**
                // (HM-DEC-142). One is a callsign read correctly by a decoder that
                // measured no word breaks; the other is a decoder producing
                // nothing. Counting characters alone cannot tell them apart, and
                // the difference is what somebody reading this file months later
                // would most want to know.
                ["wordSpacingUnmeasured"] = report.WordSpacingUnmeasured,

                // Unknown stays unknown rather than becoming a zero that reads
                // like a measurement (HM-DEC-050).
                ["toneHz"] = report.HasTone ? Math.Round(report.ToneHz) : null,
                ["snrDb"] = double.IsNaN(report.SnrDb)
                    ? null
                    : Math.Round(report.SnrDb, 1),

                ["elementsSeen"] = report.ElementsSeen,
                ["elementsResolved"] = report.ElementsResolved,
                ["charactersEmitted"] = report.CharactersEmitted,
                ["charactersUnsure"] = report.CharactersUnsure,
            });

    /// <summary>
    /// The operator kept a recording of what the decoder was hearing
    /// (HM-DEC-088).
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="seconds">How much audio was written.</param>
    /// <param name="frequencyHz">Where the radio was.</param>
    /// <param name="worked">Whether the file was actually written.</param>
    /// <remarks>
    /// The file name is not here and neither is anything on the recording. A
    /// capture may contain somebody's callsign being sent, which §2.1 makes
    /// Tim's to review before it goes anywhere, and HM-DEC-018 keeps it out of
    /// the record regardless.
    /// </remarks>
    public static void AudioCaptured(
        ITelemetry? telemetry, double seconds, long frequencyHz, bool worked)
        => telemetry?.Write(
            TelemetryCategory.Decode,
            "audio_captured",
            new Dictionary<string, object?>
            {
                ["outcome"] = worked ? "proceeded" : "failed",
                ["reason"] = worked ? "written" : "could_not_write",
                ["seconds"] = Math.Round(seconds, 1),
                ["frequencyHz"] = frequencyHz,
            },
            worked ? TelemetryLevel.Info : TelemetryLevel.Warn);

    /// <summary>
    /// The send buttons became usable, or stopped being (HM-DEC-078).
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="enabled">Whether the operator can press them now.</param>
    /// <param name="readiness">The verdict behind it, or null.</param>
    /// <remarks>
    /// <para>WHAT THE OPERATOR SAW, BESIDE WHAT THE ENGINE DECIDED. The record
    /// said the engine reached Ready while the screen showed dead buttons, and
    /// nothing anywhere could show that disagreement. An event describing the
    /// engine is not a record of the application (§0.0.1); it is a record of
    /// half of it, and it was the half that was working.</para>
    /// <para>A button that is off while readiness says Ready is a warning,
    /// because that combination is the failure itself and somebody should be
    /// able to find it by scanning (HM-DEC-077).</para>
    /// </remarks>
    public static void SendButtonsEnabledChanged(
        ITelemetry? telemetry, bool enabled, CwReadiness? readiness)
    {
        if (telemetry is null)
        {
            return;
        }

        var verdict = readiness?.State.ToString() ?? "unknown";
        var disagrees = !enabled && readiness is { MaySend: true };

        telemetry.Write(
            TelemetryCategory.Rig, "send_buttons_enabled",
            new Dictionary<string, object?>
            {
                ["outcome"] = enabled ? "proceeded" : "refused",
                ["reason"] = readiness?.Reason ?? "no_verdict",
                ["enabled"] = enabled,
                ["readinessState"] = verdict,
                ["readinessMaySend"] = readiness?.MaySend,
                ["disagreesWithEngine"] = disagrees,
            },
            disagrees ? TelemetryLevel.Error : TelemetryLevel.Info);
    }

    /// <summary>
    /// A message went to the radio (HM-DEC-079).
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="characters">How long the message was.</param>
    /// <param name="pieces">How many keyer messages it takes.</param>
    /// <param name="frequencyHz">Where.</param>
    /// <param name="mode">The mode, or "".</param>
    /// <remarks>
    /// <para>THE RECORD HAD EVERY DECISION THE GATE MADE AND NOTHING ABOUT WHAT
    /// THE RADIO DID. Two successful transmissions became invisible, and had to
    /// be reconstructed afterward from the shape of a status line flapping
    /// (§0.0.1).</para>
    /// <para>**THE TEXT ITSELF IS NOT WRITTEN, AND THAT IS NOT AN OVERSIGHT.**
    /// A CQ is the operator's own callsign twice over, and HM-DEC-018 forbids a
    /// callsign in telemetry without exception. The length, the piece count, the
    /// frequency, the mode and the duration make a transmission fully visible
    /// and identify nobody, which is everything the diagnosis needed and
    /// nothing it did not.</para>
    /// </remarks>
    public static void SendStarted(
        ITelemetry? telemetry, int characters, int pieces, long frequencyHz, string mode)
        => telemetry?.Write(
            TelemetryCategory.Rig, "cw_send_started",
            new Dictionary<string, object?>
            {
                ["outcome"] = "proceeded",
                ["reason"] = "ok",
                ["characters"] = characters,
                ["pieces"] = pieces,
                ["frequencyHz"] = frequencyHz,
                ["mode"] = mode,
            });

    /// <summary>
    /// A message finished, one way or another (HM-DEC-079).
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="characters">How long the message was.</param>
    /// <param name="outcome">What became of it.</param>
    /// <param name="piecesSent">How many pieces actually went.</param>
    /// <param name="piecesTotal">How many it needed.</param>
    /// <param name="seconds">How long the radio really keyed.</param>
    /// <param name="frequencyHz">Where.</param>
    /// <param name="establishedBy">How the end of it was established.</param>
    /// <remarks>
    /// <para>The duration is the number that proves it: eighteen seconds is a
    /// full CQ at twenty words a minute, and a send that returned in a tenth of a
    /// second never keyed anything. No text here either, for the same reason.
    /// </para>
    /// <para>**AND FOR A WHILE THIS FILE SAID EXACTLY THAT AND RECORDED A
    /// HUNDREDTH OF A SECOND ANYWAY** (HM-DEC-085), because it was measured
    /// across the send call, and command `17` hands the message to the radio's
    /// keyer and returns. So the record proved the opposite of the truth on every
    /// row. How the end was established goes in beside it, because a duration
    /// watched and a duration calculated are different kinds of fact and a file
    /// that cannot tell them apart cannot settle an argument (§0.0.1).</para>
    /// </remarks>
    public static void SendFinished(
        ITelemetry? telemetry, int characters, string outcome,
        int piecesSent, int piecesTotal, double seconds, long frequencyHz,
        string establishedBy = "")
    {
        var worked = string.Equals(outcome, "Sent", StringComparison.Ordinal);

        telemetry?.Write(
            TelemetryCategory.Rig,
            worked ? "cw_send_completed" : "cw_send_ended",
            new Dictionary<string, object?>
            {
                ["outcome"] = worked ? "proceeded" : "failed",
                ["reason"] = outcome.ToLowerInvariant(),
                ["characters"] = characters,
                ["piecesSent"] = piecesSent,
                ["piecesTotal"] = piecesTotal,
                ["seconds"] = Math.Round(seconds, 2),
                ["establishedBy"] = establishedBy,
                ["frequencyHz"] = frequencyHz,
            },
            worked ? TelemetryLevel.Info : TelemetryLevel.Warn);
    }

    /// <summary>
    /// The whole chain of one transmission, kept (HM-DEC-082).
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="brokeAt">Which link failed, or "none".</param>
    /// <param name="keyedSeconds">How long it keyed, or null.</param>
    /// <param name="powerReading">The Po meter's peak, or null.</param>
    /// <param name="swrReading">The SWR meter's worst, or null.</param>
    /// <param name="skimmers">How many were reporting, or null.</param>
    /// <param name="reports">How many reported this operator.</param>
    /// <remarks>
    /// EVERY LINK, NOT JUST THE OUTCOME, so a later history of "times you were
    /// heard" can be built from what is already on disk. A null stays null: a
    /// meter that was not read and a meter that read zero are different facts and
    /// the file has to keep them apart (§0.0). No text, as ever (HM-DEC-018).
    /// </remarks>
    public static void TransmitChain(
        ITelemetry? telemetry, string brokeAt, double? keyedSeconds,
        int? powerReading, int? swrReading, int? skimmers, int reports)
        => telemetry?.Write(
            TelemetryCategory.Rig, "transmit_chain",
            new Dictionary<string, object?>
            {
                ["outcome"] = brokeAt == "none" ? "proceeded" : "degraded",
                ["reason"] = brokeAt,
                ["keyedSeconds"] = keyedSeconds is { } s ? Math.Round(s, 2) : null,
                ["powerReading"] = powerReading,
                ["swrReading"] = swrReading,
                ["skimmersReporting"] = skimmers,
                ["reports"] = reports,
            },
            brokeAt == "none" ? TelemetryLevel.Info : TelemetryLevel.Warn);

    /// <summary>
    /// Hamlet changed a setting on the radio (HM-DEC-084).
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="field">Which setting.</param>
    /// <param name="command">The CI-V command, for the citation.</param>
    /// <param name="was">What it was, or null when it was never read.</param>
    /// <param name="now">What it is.</param>
    /// <param name="outcome">Whether the read-back confirmed it.</param>
    /// <remarks>
    /// EVERY WRITE IS IN THE RECORD. Hamlet changing somebody's radio without
    /// the file saying so would be the one thing worse than not changing it, and
    /// the before value is what makes the record an undo rather than a note.
    /// Null stays null: never read and read as zero are different facts (§0.0).
    /// </remarks>
    public static void SettingChanged(
        ITelemetry? telemetry, string field, string command,
        int? was, int now, string outcome)
        => telemetry?.Write(
            TelemetryCategory.Rig, "setting_changed",
            new Dictionary<string, object?>
            {
                ["outcome"] = outcome == "Confirmed" ? "proceeded" : "failed",
                ["reason"] = outcome.ToLowerInvariant(),
                ["field"] = field,
                ["command"] = command,
                ["was"] = was,
                ["now"] = now,
            },
            outcome == "Confirmed" ? TelemetryLevel.Info : TelemetryLevel.Warn);

    /// <summary>
    /// A digital capture press produced no files, and why.
    /// </summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="reason">Which of the four ways it produced nothing.</param>
    /// <remarks>
    /// <para>**A PRESS THAT PRODUCED NOTHING IS ALWAYS WORTH FINDING**, so it is
    /// <see cref="TelemetryLevel.Warn"/>. Scanning a morning's file for warnings
    /// is how the owner finds out what happened at the radio, and a refused
    /// capture at info level is a line nobody reads.</para>
    /// <para>**THE PAYLOAD IS A REASON CODE AND NOTHING ELSE** (HM-DEC-018). No
    /// message text, no path, no callsign, no free text of any kind — see
    /// <see cref="DigitalCaptureRefusal"/> for why the parameter is an enum.</para>
    /// <para>**IT IS NOT A SCREEN CLAIM** (`CLAUDE.md` §12.1). The status bar
    /// keeps the sentence it always had, unchanged and in Tim's words; this adds
    /// a line to a log file and nothing to any tab.</para>
    /// </remarks>
    public static void DigitalCaptureRefused(
        ITelemetry? telemetry, DigitalCaptureRefusal reason)
        => telemetry?.Write(
            TelemetryCategory.Decode,
            "digital_capture_refused",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["reason"] = reason.ToString(),
            },
            TelemetryLevel.Warn);

    private static IReadOnlyDictionary<string, object?> Merge(
        IReadOnlyDictionary<string, object?> a, IReadOnlyDictionary<string, object?> b)
    {
        var merged = new Dictionary<string, object?>(StringComparer.Ordinal);

        foreach (var pair in a)
        {
            merged[pair.Key] = pair.Value;
        }

        foreach (var pair in b)
        {
            merged[pair.Key] = pair.Value;
        }

        return merged;
    }
}
