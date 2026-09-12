using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Telemetry;

namespace Hamlet.App.Telemetry;

/// <summary>
/// **What the PSK31 path writes down, so an empty list can be diagnosed from the file.**
/// </summary>
/// <remarks>
/// <para>**THE MEASUREMENT THIS EXISTS FOR** (work instruction 322). Across ten sessions
/// and 12,587 events on 2026-09-11 the FT8 path wrote 3,448 `ft8_slot`, 3,419
/// `decodes_drawn` and 2,317 `decode_quality`, and **the PSK31 path wrote nothing at
/// all** - not from the search, the squelch, a demodulator or the parser, at any version,
/// including the ones where the whole receive path was running. The operator sat on the
/// tab for five minutes and asked afterwards whether the band had been empty or the
/// squelch had been shut, and **nothing in the file could answer him.**</para>
/// <para>**SO THE ONE LINE THAT ANSWERS IT IS `psk31_search_pass`.** A pass with no
/// candidates says the band was quiet. A pass with candidates that all read
/// `crossed: false` says the opposite: something was there and Hamlet would not take it.
/// On the screen those two are the same empty list.</para>
/// <para>**NOTHING PERSONAL, AND NARROWER THAN THE RULING REQUIRES** (HM-DEC-018, §2.1).
/// An offset, a score, a kind, a count, a reason. **No callsign, no decoded text, no
/// grid** - and `ThePsk31TelemetryTests` proves it by scanning the serialised JSON rather
/// than trusting each call site here to have remembered.</para>
/// <para>**AND NOTHING HERE TOUCHES THE PATH IT DESCRIBES** (§0.2). Every method takes
/// values already measured and writes them; none of them keys, arms, composes or decodes
/// anything, and removing the whole file would change no behaviour.</para>
/// </remarks>
public static class Psk31Events
{
    /// <summary>How many candidates one pass may name.</summary>
    /// <remarks>
    /// **THE TWENTY STRONGEST** (the instruction). A noisy band nominates far more than
    /// that, and a line listing all of them would be long enough that nobody reads any of
    /// it. The count before the cap is written beside them, so a reader can tell a quiet
    /// band from a busy one whose tail was trimmed (§0.0).
    /// </remarks>
    public const int MostCandidates = 20;

    /// <summary>How often a pass is written down when nothing changed.</summary>
    /// <remarks>
    /// **THE SAMPLE RULE, IN ONE PLACE** (the instruction asks for it stated once).
    /// Passes run about once a second; writing every one would be 3,600 lines an hour of
    /// a band doing nothing. **A pass on which the carrier set changed is always
    /// written**, whatever the clock says, because that is the pass somebody will go
    /// looking for.
    /// </remarks>
    public static readonly TimeSpan PassInterval = TimeSpan.FromSeconds(10);

    /// <summary>The tab was pressed and the listener started.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="dialHz">Where the radio was, or 0 where it is unknown.</param>
    /// <param name="lowHz">The bottom of the passband searched.</param>
    /// <param name="highHz">The top of it.</param>
    /// <param name="sampleRate">The audio rate the path is decoded at.</param>
    /// <param name="deviceSampleRate">The rate the audio device is handing over.</param>
    /// <param name="resampleRatio">Input samples per output sample, 1 where it passes through.</param>
    /// <param name="squelch">The squelch threshold the demodulators use.</param>
    /// <param name="retirePasses">How many passes in a row a carrier may be missing from.</param>
    /// <param name="retireSeconds">What that comes to in seconds at this rate.</param>
    /// <remarks>
    /// <para>**IT WRITES THE THRESHOLDS, NOT ONLY THE FACT OF STARTING.** Every number in
    /// this path was chosen against synthetic fixtures, and the first question about a
    /// session that heard nothing is what the thresholds were that evening.</para>
    /// <para>**AND BOTH RATES, SINCE UNIT 324** (§R13). The operator's evening of
    /// 2026-09-11 wrote `sampleRate: 48000` and there was nothing in the line to say
    /// whether that was the device or the decoder - it was both, and that was the fault.
    /// Two fields and the ratio between them cannot be read the same way twice.</para>
    /// </remarks>
    public static void ListeningStarted(
        ITelemetry? telemetry,
        long dialHz,
        double lowHz,
        double highHz,
        int sampleRate,
        int deviceSampleRate,
        double resampleRatio,
        double squelch,
        int retirePasses,
        double retireSeconds)
        => telemetry?.Write(
            TelemetryCategory.Psk31,
            "psk31_listening_started",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                // **UNKNOWN STAYS ABSENT** (§0.0). With no radio connected the dial is
                // not zero hertz, it is unmeasured.
                ["dialHz"] = dialHz > 0 ? dialHz : null,
                ["passbandLowHz"] = Math.Round(lowHz),
                ["passbandHighHz"] = Math.Round(highHz),
                ["sampleRate"] = sampleRate,
                ["deviceSampleRate"] = deviceSampleRate,
                ["resampleRatio"] = Math.Round(resampleRatio, 4),
                ["squelchQuality"] = squelch,

                // **THE RETIRE RULE IS COUNTED IN PASSES SINCE UNIT 324**, and a pass is
                // a different length at a different rate, so both go in: the rule, and
                // what it comes to on this evening's audio.
                ["retirePasses"] = retirePasses,
                ["retireSeconds"] = Math.Round(retireSeconds, 3),
                ["retireRule"] = Psk31Listener.RetireRule,
                ["searchRule"] = Psk31CarrierSearch.SearchRule,

                // **THE BUILD IS NOT REPEATED HERE.** Schema B stamps `appVersion` on
                // every line of the file (HM-DEC-018), so a copy in the data bag would
                // be a second place for it to disagree with itself. The instruction
                // asks for it; the envelope already has it, and that is reported.
            });

    /// <summary>The tab was left, or the application stopped.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="seconds">How long it listened.</param>
    /// <param name="carriers">How many carriers appeared in that time.</param>
    /// <param name="characters">How many characters were emitted.</param>
    /// <param name="lines">How many lines the parser returned a verdict for.</param>
    public static void ListeningStopped(
        ITelemetry? telemetry,
        double seconds,
        int carriers,
        int characters,
        int lines)
        => telemetry?.Write(
            TelemetryCategory.Psk31,
            "psk31_listening_stopped",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["seconds"] = Math.Round(seconds, 1),
                ["carriersSeen"] = carriers,
                ["charactersEmitted"] = characters,
                ["linesParsed"] = lines,
            });

    /// <summary>One pass of the search over the passband.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="pass">What the pass measured.</param>
    /// <param name="reason">Why this pass was written: `changed` or `sampled`.</param>
    /// <remarks>
    /// **THE CANDIDATES THAT DID NOT CROSS ARE THE POINT.** A reader looking at a quiet
    /// evening wants to know whether there was nothing there or whether there was
    /// something Hamlet would not take, and only the failed candidates can tell him.
    /// </remarks>
    public static void SearchPass(ITelemetry? telemetry, Psk31Pass pass, string reason)
    {
        ArgumentNullException.ThrowIfNull(pass);

        telemetry?.Write(
            TelemetryCategory.Psk31,
            "psk31_search_pass",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["reason"] = reason,
                ["passMs"] = pass.Milliseconds,
                ["candidateCount"] = pass.Candidates.Count,
                ["carriersHeld"] = pass.CarriersHeld,
                ["candidates"] = pass.Candidates
                    .Take(MostCandidates)
                    .Select(c => new Dictionary<string, object?>(StringComparer.Ordinal)
                    {
                        ["offsetHz"] = c.OffsetHz,
                        ["strengthDb"] = c.StrengthDb,
                        ["quality"] = c.Coherence,
                        ["crossed"] = c.Crossed,
                    })
                    .ToList(),
            });
    }

    /// <summary>A candidate crossed and a demodulator was made for it.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="change">What the search recorded.</param>
    public static void CarrierAppeared(ITelemetry? telemetry, Psk31CarrierChange change)
    {
        ArgumentNullException.ThrowIfNull(change);

        telemetry?.Write(
            TelemetryCategory.Psk31,
            "psk31_carrier_appeared",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["carrierId"] = change.Id,
                ["offsetHz"] = change.OffsetHz,
                ["strengthDb"] = change.StrengthDb,
                ["quality"] = change.Coherence,
                ["passesAsCandidate"] = change.Passes,
            });
    }

    /// <summary>A carrier stopped being listed.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="change">What the search recorded.</param>
    /// <param name="characters">How many characters it emitted while it was held.</param>
    /// <param name="lines">How many lines the parser made of them.</param>
    /// <param name="sinceLastCharacter">Seconds since its last character, or null.</param>
    /// <remarks>
    /// **THE REASON IS THE WHOLE VALUE OF THIS EVENT.** A station that finished, one that
    /// faded, and a decoder that lost the thread all leave the list the same way, and
    /// they are three different evenings.
    /// </remarks>
    public static void CarrierRetired(
        ITelemetry? telemetry,
        Psk31CarrierChange change,
        int characters,
        int lines,
        double? sinceLastCharacter)
    {
        ArgumentNullException.ThrowIfNull(change);

        telemetry?.Write(
            TelemetryCategory.Psk31,
            "psk31_carrier_retired",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["carrierId"] = change.Id,
                ["offsetHz"] = change.OffsetHz,
                ["reason"] = (change.Why ?? Psk31Retirement.SignalGone).ToString(),
                ["lifetimeSeconds"] = change.LifetimeSeconds,
                ["charactersEmitted"] = characters,
                ["linesParsed"] = lines,

                // **ABSENT WHERE IT NEVER SAID ANYTHING**, rather than zero, which would
                // read as *it spoke just now* (§0.0).
                ["secondsSinceLastCharacter"] = sinceLastCharacter is { } seconds
                    ? Math.Round(seconds, 1)
                    : null,
            });
    }

    /// <summary>A held carrier's squelch opened or closed.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="offsetHz">Where the carrier sits.</param>
    /// <param name="open">True where it opened.</param>
    /// <param name="quality">What the measure read at that moment.</param>
    public static void Squelch(
        ITelemetry? telemetry, double offsetHz, bool open, double quality)
        => telemetry?.Write(
            TelemetryCategory.Psk31,
            "psk31_squelch",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["offsetHz"] = Math.Round(offsetHz, 1),
                ["open"] = open,
                ["quality"] = Math.Round(quality, 3),
                ["threshold"] = Psk31Demodulator.SquelchQuality,
            });

    /// <summary>A held carrier started or stopped being readable, or its AFC moved.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="offsetHz">Where the carrier sits.</param>
    /// <param name="reading">True where characters are coming out of it.</param>
    /// <param name="afcHz">How far the AFC has pulled from the first estimate.</param>
    /// <remarks>
    /// <para>**IT WAS CALLED `psk31_lock` AND IT NEVER MEASURED LOCK** (work instruction
    /// 324 task 3). What it reads is the demodulator's squelch: whether this carrier is
    /// **producing characters**. A reader of the file took `locked: false` for a decoder
    /// that had lost the thread, when it says only that Hamlet is hearing a carrier it
    /// cannot yet read - which is a state a station can sit in for as long as it likes,
    /// and which the panel now says in words.</para>
    /// <para>**AND IT IS NOT A RETIREMENT.** Since unit 324 a carrier is retired when the
    /// signal goes, never because it stopped being readable, so this event and
    /// `psk31_carrier_retired` no longer describe the same moment.</para>
    /// </remarks>
    public static void Reading(
        ITelemetry? telemetry, double offsetHz, bool reading, double afcHz)
        => telemetry?.Write(
            TelemetryCategory.Psk31,
            "psk31_reading",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["offsetHz"] = Math.Round(offsetHz, 1),
                ["reading"] = reading,
                ["afcHz"] = Math.Round(afcHz, 1),
            });

    /// <summary>The parser returned a verdict for one line.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="offsetHz">Which carrier the line came from.</param>
    /// <param name="kind">What the parser made of it.</param>
    /// <param name="certain">True where it was sure rather than inferring.</param>
    /// <param name="turnover">True where the line handed over.</param>
    /// <param name="toOperator">True where it was addressed to this station.</param>
    /// <param name="characters">How long the line was.</param>
    /// <remarks>
    /// **THE VERDICT AND NOT THE LINE** (HM-DEC-018, §2.1). A kind, two flags and a
    /// count. **The text itself never leaves the screen**, and neither does the callsign
    /// the parser read out of it - which is why `toOperator` is a flag rather than a
    /// name.
    /// </remarks>
    public static void LineParsed(
        ITelemetry? telemetry,
        double offsetHz,
        string kind,
        bool certain,
        bool turnover,
        bool toOperator,
        int characters)
        => telemetry?.Write(
            TelemetryCategory.Psk31,
            "psk31_line_parsed",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["offsetHz"] = Math.Round(offsetHz, 1),
                ["kind"] = kind,
                ["certain"] = certain,
                ["turnover"] = turnover,
                ["toOperator"] = toOperator,
                ["characters"] = characters,
            });

    /// <summary>What the audio on the PSK31 path looks like.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="level">What the tap measured.</param>
    /// <param name="reason">Why this reading was written.</param>
    /// <remarks>
    /// **THE SAME FIVE NUMBERS `decode_quality` CARRIES**, sampled by the same rule, so a
    /// reader who knows one knows the other. A path that hears nothing because the audio
    /// device is silent and a path that hears nothing because the band is quiet are
    /// different faults, and this is what separates them.
    /// </remarks>
    public static void AudioHeard(
        ITelemetry? telemetry, AudioLevel level, string reason)
        => telemetry?.Write(
            TelemetryCategory.Psk31,
            "psk31_audio_level",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["reason"] = reason,
                ["inputPeakDb"] = Math.Round(level.PeakDb, 1),
                ["inputRmsDb"] = Math.Round(level.RmsDb, 1),
                ["inputFloorDb"] = Math.Round(level.FloorDb, 1),
                ["clipping"] = level.PeakDb >= Hamlet.RadioEngine.Audio.AudioLevel.FullScaleDb,
                ["nearlySilent"] = level.NearlySilent,
            });

    /// <summary>A macro was composed for sending.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="kind">Which macro: cq, answer, report or confirm.</param>
    /// <param name="characters">How long the text is.</param>
    /// <param name="seconds">How long it takes at 31.25 baud.</param>
    /// <param name="capSeconds">The longest a single send may run.</param>
    /// <param name="offsetHz">Where it would go out, or null where unknown.</param>
    /// <remarks>
    /// **THE LENGTH AND NOT THE TEXT** (§2.1). A macro carries the operator's callsign
    /// twice over, and a count says everything a diagnosis needs.
    /// </remarks>
    public static void SendComposed(
        ITelemetry? telemetry,
        string kind,
        int characters,
        double seconds,
        double capSeconds,
        double? offsetHz)
        => telemetry?.Write(
            TelemetryCategory.Psk31,
            "psk31_send_composed",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["macro"] = kind,
                ["characters"] = characters,
                ["seconds"] = Math.Round(seconds, 2),
                ["capSeconds"] = capSeconds,
                ["withinCap"] = seconds <= capSeconds,
                ["offsetHz"] = offsetHz is { } hz ? Math.Round(hz, 1) : null,
            });

    /// <summary>A send was refused before anything went out.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="reason">A stable token: `bolt`, `cap`, `uncertain`, `mode`.</param>
    /// <param name="kind">Which macro, where it is known.</param>
    /// <param name="stage">How far it got before the refusal.</param>
    /// <remarks>
    /// **A REFUSAL IS AN OUTCOME** (§8.1). It is as loggable as a transmission and more
    /// useful, because a send that worked is the case nobody has to diagnose.
    /// </remarks>
    public static void SendRefused(
        ITelemetry? telemetry, string reason, string? kind, string stage)
        => telemetry?.Write(
            TelemetryCategory.Psk31,
            "psk31_send_refused",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["reason"] = reason,
                ["macro"] = string.IsNullOrWhiteSpace(kind) ? null : kind,
                ["stage"] = stage,
            },
            TelemetryLevel.Warn);

    /// <summary>The transmitter was keyed, or let go.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="keyed">True on keying, false on unkeying.</param>
    /// <param name="seconds">Seconds keyed so far, or planned.</param>
    /// <param name="plannedSeconds">How long it was meant to run.</param>
    /// <param name="aborted">True where it was stopped early.</param>
    /// <remarks>
    /// **THIS WRITES AND DOES NOT KEY** (§0.2). It is called from beside the one keying
    /// site and the `finally` that lets go; it reaches neither.
    /// </remarks>
    public static void SendKeying(
        ITelemetry? telemetry,
        bool keyed,
        double seconds,
        double plannedSeconds,
        bool aborted)
        => telemetry?.Write(
            TelemetryCategory.Psk31,
            keyed ? "psk31_send_keyed" : "psk31_send_unkeyed",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["seconds"] = Math.Round(seconds, 2),
                ["plannedSeconds"] = Math.Round(plannedSeconds, 2),
                ["aborted"] = aborted,
            });

    /// <summary>What the radio said after a send.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="transmitting">What `1C 00` reported, or null where unanswered.</param>
    /// <param name="transmittingAgeMs">How old that reading is, or null.</param>
    /// <param name="powerPercent">What `15 11` reported, or null where unanswered.</param>
    /// <param name="powerAgeMs">How old that reading is, or null.</param>
    /// <remarks>
    /// <para>**THIS IS ASK 1 TURNED INTO A MEASUREMENT** (unit 303's proposal, still the
    /// owner's to rule on). `Played` says what the audio path did. This says what the
    /// **radio** did, read from the two commands Hamlet already polls four times a
    /// second.</para>
    /// <para>**AND WHERE THE RADIO DID NOT ANSWER, THE EVENT SAYS SO.** On a machine with
    /// no radio every field is absent and `answered` is false. **That absence is the
    /// finding**, written down rather than left as a silence somebody has to infer from
    /// a missing line (§0.0, HM-DEC-111 on a reading carrying its age).</para>
    /// </remarks>
    public static void RadioAfterSend(
        ITelemetry? telemetry,
        bool? transmitting,
        double? transmittingAgeMs,
        double? powerPercent,
        double? powerAgeMs)
        => telemetry?.Write(
            TelemetryCategory.Psk31,
            "psk31_radio_after_send",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["answered"] = transmitting is not null || powerPercent is not null,
                ["transmitting"] = transmitting,
                ["transmittingAgeMs"] = transmittingAgeMs is { } a
                    ? Math.Round(a)
                    : null,
                ["powerPercent"] = powerPercent is { } p ? Math.Round(p, 1) : null,
                ["powerAgeMs"] = powerAgeMs is { } b ? Math.Round(b) : null,
            });

    /// <summary>What the ALC read during a send, and whether it was past the zone.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="reading">The meter's own number, or null where it was not read.</param>
    /// <param name="ageMs">How old that reading is, or null.</param>
    /// <param name="pastTheZone">True where it is past where it should sit.</param>
    /// <param name="zone">The top of the zone, or null where none has been learned.</param>
    /// <param name="scaleTop">The top of the meter's own scale, from the manual.</param>
    /// <param name="reference">What a clean send read, or null where none has been seen.</param>
    /// <param name="margin">How far above the reference is allowed, or null.</param>
    /// <param name="referenceMode">Which mode the reference came from, or null.</param>
    /// <remarks>
    /// <para>**§R11's HALF THAT IS A MEASUREMENT.** The panel gets a sentence a person with
    /// no shack years can act on; the file gets the number, its age (HM-DEC-111) and the
    /// figure it was judged against, so somebody reading it afterwards can check the
    /// judgement rather than take it.</para>
    /// <para>**A READING NOBODY TOOK IS ABSENT, NOT ZERO** (§0.0). With no radio, or with no
    /// documented read for this field, `reading` is null and `pastTheZone` is false - which
    /// says *not measured*, not *it was fine*.</para>
    /// </remarks>
    public static void AlcRead(
        ITelemetry? telemetry,
        double? reading,
        double? ageMs,
        bool pastTheZone,
        double? zone,
        double scaleTop,
        double? reference = null,
        double? margin = null,
        string? referenceMode = null)
        => telemetry?.Write(
            TelemetryCategory.Psk31,
            "psk31_send_alc",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["measured"] = reading is not null,
                ["alc"] = reading,
                ["scaleTop"] = scaleTop,
                ["ageMs"] = ageMs is { } age ? Math.Round(age) : null,

                // **A ZONE NOBODY RULED IS ABSENT, NOT A NUMBER** (§0.0, work
                // instruction 324 task 4b). Unit 323 wrote 128 here, which it had
                // invented; the manual gives the scale and not the line on it, so
                // `zone` is null and `judged` says plainly that nothing was compared.
                // **SINCE §R15 THE ZONE IS LEARNED RATHER THAN RULED** - it is the
                // reference plus the margin, both written beside it - and it is still
                // absent until an FT8 send has been observed.
                ["judged"] = zone is not null,
                ["zone"] = zone,
                ["pastTheZone"] = pastTheZone,

                // **THE VERDICT'S WORKING, NOT JUST ITS ANSWER** (§R13, §0.0.1). A
                // record that carried only *hot* could not be argued with afterwards;
                // with the reading, the reference, the margin and the mode it came
                // from, the arithmetic can be redone from the file.
                ["reference"] = reference,
                ["margin"] = margin,
                ["referenceMode"] = referenceMode,
            },
            pastTheZone ? TelemetryLevel.Warn : TelemetryLevel.Info);

    /// <summary>**A reference for the ALC was taken from a clean send** (§R15).</summary>
    /// <param name="telemetry">The sink, or null.</param>
    /// <param name="reading">What the meter read, on its own scale.</param>
    /// <param name="takenUtc">When the poll took it.</param>
    /// <param name="mode">Which mode was sending: `FT8` or `FT4`.</param>
    /// <param name="scaleTop">The top of that scale, from the manual.</param>
    /// <param name="margin">How far above it a PSK31 send may read.</param>
    /// <remarks>
    /// <para>**EVERY NEW STAGE WRITES ITS EVENT** (§R13). Learning a reference is the
    /// moment Hamlet's behaviour changes from reporting to judging, and a change in
    /// behaviour that leaves no line in the file cannot be told afterwards from a
    /// change in the radio.</para>
    /// <para>**NOTHING PERSONAL** (HM-DEC-018, §2.1): a meter reading, a clock time,
    /// a mode name and two numbers. No callsign, no grid, no text.</para>
    /// </remarks>
    public static void AlcReferenceLearned(
        ITelemetry? telemetry,
        double reading,
        DateTime takenUtc,
        string mode,
        double scaleTop,
        double margin)
        => telemetry?.Write(
            TelemetryCategory.Psk31,
            "psk31_alc_reference",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["alc"] = reading,
                ["scaleTop"] = scaleTop,
                ["takenUtc"] = takenUtc.ToString("O", CultureInfo.InvariantCulture),
                ["mode"] = mode,
                ["margin"] = margin,
            },
            TelemetryLevel.Info);

    /// <summary>A number as the record spells it.</summary>
    internal static string Say(double value)
        => value.ToString("0.###", CultureInfo.InvariantCulture);
}
