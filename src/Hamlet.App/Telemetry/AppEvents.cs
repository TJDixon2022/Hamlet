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
    /// <param name="seconds">How long it took.</param>
    /// <param name="frequencyHz">Where.</param>
    /// <remarks>
    /// The duration is the number that proves it: eighteen seconds is a full CQ
    /// at twenty words a minute, and a send that returned in a tenth of a second
    /// never keyed anything. No text here either, for the same reason.
    /// </remarks>
    public static void SendFinished(
        ITelemetry? telemetry, int characters, string outcome,
        int piecesSent, int piecesTotal, double seconds, long frequencyHz)
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
