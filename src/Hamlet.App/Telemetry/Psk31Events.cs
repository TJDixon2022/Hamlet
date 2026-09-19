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
    /// <param name="characters">How many characters were emitted - the sum of every retire's count.</param>
    /// <param name="lines">How many lines the parser returned a verdict for - the sum of every retire's count.</param>
    /// <remarks>
    /// **THE SUM OF THE RETIRES AND NOTHING ELSE** (work instruction 337 task 1). Until then it
    /// summed only the carriers still held at the moment of stopping, and on 2026-09-12 wrote
    /// 0 beside a retire that said 262.
    /// </remarks>
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

    /// <summary>A PSK31 row outlived its carrier and was kept on the list, marked ended.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="offsetHz">Where the row's carrier was, as the row's cell says.</param>
    /// <param name="characters">How many characters the row shows.</param>
    /// <param name="lines">How many complete messages the parser read on it.</param>
    /// <param name="lifetimeSeconds">Audio seconds from the row going up to its carrier going.</param>
    /// <param name="tag">What an Olivia row adds - see <see cref="Olivia"/> - or null on PSK31's.</param>
    /// <remarks>
    /// **WHAT WAS HEARD STAYS** (Tim, 2026-09-14; work instruction 355 task 2). The carrier's own
    /// retirement is `psk31_carrier_retired`; this is the row's, and it is the fact that the words
    /// are still on his screen. **Counts and a frequency, never the words** (HM-DEC-018, §2.1).
    /// </remarks>
    public static void RowEnded(
        ITelemetry? telemetry, double offsetHz, int characters, int lines, double lifetimeSeconds,
        IReadOnlyDictionary<string, object?>? tag = null)
        => telemetry?.Write(
            TelemetryCategory.Psk31,
            "psk31_row_ended",
            Tagged(
                new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["offsetHz"] = offsetHz,
                    ["characters"] = characters,
                    ["lines"] = lines,
                    ["lifetimeSeconds"] = Math.Round(lifetimeSeconds, 1),
                },
                tag));

    /// <summary>An ended PSK31 row left the list.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="offsetHz">Where the row's carrier was.</param>
    /// <param name="characters">How many characters went with it.</param>
    /// <param name="reason">`clear`, `retune` or `cap` - which of the list's own rules removed it.</param>
    /// <param name="tag">What an Olivia row adds - see <see cref="Olivia"/> - or null on PSK31's.</param>
    /// <remarks>
    /// **THE ONLY WAYS AN ENDED ROW GOES ARE THE WAYS AN FT8 ROW GOES**, and the reason says which,
    /// so a record that shows words appearing also shows what took them off the screen.
    /// </remarks>
    public static void RowCleared(
        ITelemetry? telemetry, double offsetHz, int characters, string reason,
        IReadOnlyDictionary<string, object?>? tag = null)
        => telemetry?.Write(
            TelemetryCategory.Psk31,
            "psk31_row_cleared",
            Tagged(
                new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["offsetHz"] = offsetHz,
                    ["characters"] = characters,
                    ["reason"] = reason,
                },
                tag));

    /// <summary>A held carrier stopped typing, or started again.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="activity">What the listener recorded.</param>
    /// <remarks>
    /// <para>**SO THE IDLE GAP SHOWS UP IN THE FILE AS AN IDLE GAP** (work instruction 327
    /// task 1). On 2026-09-12 the operator's own record ran 206 seconds with 12 carriers and
    /// 0 lines, and there was no way to read out of it that the station at 2073 Hz was
    /// sitting there with the squelch open at 0.99 waiting for an answer. **These two
    /// events are that sentence.**</para>
    /// <para>**AND NOTHING PERSONAL IS IN EITHER** (HM-DEC-018, §2.1). A frequency, a
    /// quality, and how long the state that just ended lasted.</para>
    /// </remarks>
    public static void CarrierActivity(ITelemetry? telemetry, Psk31CarrierActivity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);

        telemetry?.Write(
            TelemetryCategory.Psk31,
            activity.Idling ? "psk31_carrier_idling" : "psk31_carrier_typing",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["carrierId"] = activity.Id,
                ["offsetHz"] = activity.OffsetHz,
                ["quality"] = activity.Quality,

                // **HOW LONG THE STATE THAT JUST ENDED LASTED**, which on a typing event is
                // the length of the idle gap the operator is trying to read out of the file.
                ["afterSeconds"] = activity.Seconds,
                ["idleAfterSeconds"] = Psk31Listener.IdleAfterSeconds,
            });
    }

    /// <summary>A held carrier's squelch opened or closed.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="offsetHz">Where the carrier sits.</param>
    /// <param name="open">True where it opened.</param>
    /// <param name="quality">What the measure read at that moment.</param>
    /// <param name="tag">What an Olivia row adds - see <see cref="Olivia"/> - or null on PSK31's.</param>
    public static void Squelch(
        ITelemetry? telemetry, double offsetHz, bool open, double quality,
        IReadOnlyDictionary<string, object?>? tag = null)
        => telemetry?.Write(
            TelemetryCategory.Psk31,
            "psk31_squelch",
            Tagged(
                new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["offsetHz"] = Math.Round(offsetHz, 1),
                    ["open"] = open,
                    ["quality"] = Math.Round(quality, 3),
                    ["threshold"] = Psk31Demodulator.SquelchQuality,
                },
                tag));

    /// <summary>A held carrier started or stopped being readable, or its AFC moved.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="offsetHz">Where the carrier sits.</param>
    /// <param name="reading">True where characters are coming out of it.</param>
    /// <param name="afcHz">How far the AFC has pulled from the first estimate.</param>
    /// <param name="tag">What an Olivia row adds - see <see cref="Olivia"/> - or null on PSK31's.</param>
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
        ITelemetry? telemetry, double offsetHz, bool reading, double afcHz,
        IReadOnlyDictionary<string, object?>? tag = null)
        => telemetry?.Write(
            TelemetryCategory.Psk31,
            "psk31_reading",
            Tagged(
                new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["offsetHz"] = Math.Round(offsetHz, 1),
                    ["reading"] = reading,
                    ["afcHz"] = Math.Round(afcHz, 1),
                },
                tag));

    /// <summary>The parser returned a verdict for one line.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="offsetHz">Which carrier the line came from.</param>
    /// <param name="kind">What the parser made of it.</param>
    /// <param name="certain">True where it was sure rather than inferring.</param>
    /// <param name="turnover">True where the line handed over.</param>
    /// <param name="toOperator">True where it was addressed to this station.</param>
    /// <param name="characters">How long the line was.</param>
    /// <param name="tag">What an Olivia row adds - see <see cref="Olivia"/> - or null on PSK31's.</param>
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
        int characters,
        IReadOnlyDictionary<string, object?>? tag = null)
        => telemetry?.Write(
            TelemetryCategory.Psk31,
            "psk31_line_parsed",
            Tagged(
                new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["offsetHz"] = Math.Round(offsetHz, 1),
                    ["kind"] = kind,
                    ["certain"] = certain,
                    ["turnover"] = turnover,
                    ["toOperator"] = toOperator,
                    ["characters"] = characters,
                },
                tag));

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
    /// <param name="tag">What an Olivia row adds - see <see cref="Olivia"/> - or null on PSK31's.</param>
    public static void AudioHeard(
        ITelemetry? telemetry, AudioLevel level, string reason,
        IReadOnlyDictionary<string, object?>? tag = null)
        => telemetry?.Write(
            TelemetryCategory.Psk31,
            "psk31_audio_level",
            Tagged(
                new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["reason"] = reason,
                    ["inputPeakDb"] = Math.Round(level.PeakDb, 1),
                    ["inputRmsDb"] = Math.Round(level.RmsDb, 1),
                    ["inputFloorDb"] = Math.Round(level.FloorDb, 1),
                    ["clipping"] = level.PeakDb >= Hamlet.RadioEngine.Audio.AudioLevel.FullScaleDb,
                    ["nearlySilent"] = level.NearlySilent,
                },
                tag));

    /// <summary>**What an Olivia row's event adds to the PSK31 row event of the same name.**</summary>
    /// <param name="variant">The row's variant, or null on an event about the whole listener.</param>
    /// <returns>`mode: olivia`, and the variant where there is one.</returns>
    /// <remarks>
    /// **ONE SET OF ROW EVENTS, TWO MODES** (work instruction 364 decision AK). Olivia rows are drawn
    /// by the PSK31 row path, so they are written by the PSK31 row events - the same names and
    /// fields - with the mode and the variant on top. A reader of the file tells them apart by
    /// `mode`, which a PSK31 row's event does not carry.
    /// </remarks>
    public static IReadOnlyDictionary<string, object?> Olivia(string? variant)
        => variant is null
            ? new Dictionary<string, object?>(StringComparer.Ordinal) { ["mode"] = "olivia" }
            : new Dictionary<string, object?>(StringComparer.Ordinal) { ["mode"] = "olivia", ["variant"] = variant };

    /// <summary>The Olivia listener started under the Olivia tab.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="dialHz">Where the radio was, or 0 where it is unknown.</param>
    /// <param name="lowHz">The bottom of the passband listened to.</param>
    /// <param name="highHz">The top of it.</param>
    /// <param name="sampleRate">The audio rate the listener reads at.</param>
    /// <param name="deviceSampleRate">The rate the audio device is handing over.</param>
    /// <param name="resampleRatio">Input samples per output sample, 1 where it passes through.</param>
    /// <param name="threshold">The block signal-to-noise a block must reach to be shown.</param>
    /// <param name="replaySeconds">How much audio a new channel is handed from before it was opened.</param>
    /// <remarks>
    /// <para>**`psk31_listening_started`'s fields that mean the same thing under Olivia**, and Olivia's
    /// own threshold where PSK31's squelch would be: a PSK31 retire rule written beside an Olivia
    /// listener would be a rule nothing is applying.</para>
    /// <para>**UNDER ITS OWN NAME, NOT PSK31'S** (work instruction 364, the session's decision). The
    /// row events are PSK31's names with `mode: olivia` (decision AK); this is not a row event, and
    /// `psk31_listening_started` is what the record has always meant by *a PSK31 listener started* -
    /// `TheCaptureButtonTests` reads it that way under the Olivia tab.</para>
    /// </remarks>
    public static void OliviaListeningStarted(
        ITelemetry? telemetry,
        long dialHz,
        double lowHz,
        double highHz,
        int sampleRate,
        int deviceSampleRate,
        double resampleRatio,
        double threshold,
        double replaySeconds)
        => telemetry?.Write(
            TelemetryCategory.Psk31,
            "olivia_listening_started",
            Tagged(
                new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["dialHz"] = dialHz > 0 ? dialHz : null,
                    ["passbandLowHz"] = Math.Round(lowHz),
                    ["passbandHighHz"] = Math.Round(highHz),
                    ["sampleRate"] = sampleRate,
                    ["deviceSampleRate"] = deviceSampleRate,
                    ["resampleRatio"] = Math.Round(resampleRatio, 4),
                    ["threshold"] = threshold,
                    ["replaySeconds"] = Math.Round(replaySeconds, 3),
                },
                Olivia(null)));

    /// <summary>An Olivia channel opened, and its row with it.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="id">The channel.</param>
    /// <param name="offsetHz">Where it sits.</param>
    /// <param name="variant">Its variant.</param>
    /// <param name="found">`rsid` or `blind`.</param>
    /// <remarks>
    /// **`psk31_carrier_appeared`, WITH WHAT OLIVIA MEASURES.** There is no search pass to count and no
    /// decibel strength in 2500 Hz, so those two are absent rather than zero (§0.0); how the station
    /// was found is the fact an Olivia reader needs in their place.
    /// </remarks>
    public static void OliviaCarrierAppeared(
        ITelemetry? telemetry, int id, double offsetHz, string variant, string found)
        => telemetry?.Write(
            TelemetryCategory.Psk31,
            "psk31_carrier_appeared",
            Tagged(
                new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["carrierId"] = id,
                    ["offsetHz"] = Math.Round(offsetHz, 1),
                    ["strengthDb"] = null,
                    ["quality"] = null,
                    ["passesAsCandidate"] = null,
                    ["found"] = found,
                },
                Olivia(variant)));

    /// <summary>An Olivia channel stopped being read.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="id">The channel.</param>
    /// <param name="offsetHz">Where it sat as of its last accepted block.</param>
    /// <param name="variant">Its variant.</param>
    /// <param name="reason">Why: `SignalGone`, `VariantChanged` or `ListeningStopped`.</param>
    /// <param name="lifetimeSeconds">Audio seconds from its opening to now.</param>
    /// <param name="characters">How many characters it showed.</param>
    /// <param name="lines">How many lines the parser made of them.</param>
    /// <param name="sinceLastCharacter">Seconds since its last character, or null where it showed none.</param>
    /// <param name="retire">What an Olivia retire adds - its window and factor - or null.</param>
    public static void OliviaCarrierRetired(
        ITelemetry? telemetry,
        int id,
        double offsetHz,
        string variant,
        string reason,
        double lifetimeSeconds,
        int characters,
        int lines,
        double? sinceLastCharacter,
        IReadOnlyDictionary<string, object?>? retire = null)
        => telemetry?.Write(
            TelemetryCategory.Psk31,
            "psk31_carrier_retired",
            Tagged(
                Tagged(
                    new Dictionary<string, object?>(StringComparer.Ordinal)
                    {
                        ["carrierId"] = id,
                        ["offsetHz"] = Math.Round(offsetHz, 1),
                        ["reason"] = reason,
                        ["lifetimeSeconds"] = Math.Round(lifetimeSeconds, 1),
                        ["charactersEmitted"] = characters,
                        ["linesParsed"] = lines,
                        ["secondsSinceLastCharacter"] = sinceLastCharacter is { } seconds
                            ? Math.Round(seconds, 1)
                            : null,
                    },
                    Olivia(variant)),
                retire));

    /// <summary>The Olivia tab was left, or the application stopped.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="seconds">How long it listened.</param>
    /// <param name="carriers">How many channels opened in that time.</param>
    /// <param name="characters">How many characters were shown - the sum of every retire's count.</param>
    /// <param name="lines">How many lines the parser returned a verdict for - the sum of every retire's count.</param>
    /// <remarks>**UNDER ITS OWN NAME**, for <see cref="OliviaListeningStarted"/>'s reason.</remarks>
    public static void OliviaListeningStopped(
        ITelemetry? telemetry, double seconds, int carriers, int characters, int lines)
        => telemetry?.Write(
            TelemetryCategory.Psk31,
            "olivia_listening_stopped",
            Tagged(
                new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["seconds"] = Math.Round(seconds, 1),
                    ["carriersSeen"] = carriers,
                    ["charactersEmitted"] = characters,
                    ["linesParsed"] = lines,
                },
                Olivia(null)));

    /// <summary>A row event's fields with an Olivia row's added, or as they were where there are none.</summary>
    private static Dictionary<string, object?> Tagged(
        Dictionary<string, object?> fields, IReadOnlyDictionary<string, object?>? tag)
    {
        if (tag is not null)
        {
            foreach (var (key, value) in tag)
            {
                fields[key] = value;
            }
        }

        return fields;
    }

    /// <summary>A macro was composed for sending.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="kind">Which macro: cq, answer, report or confirm.</param>
    /// <param name="characters">How long the text is.</param>
    /// <param name="seconds">How long it takes at 31.25 baud.</param>
    /// <param name="capSeconds">The longest a single send may run.</param>
    /// <param name="offsetHz">Where it would go out, or null where unknown.</param>
    /// <param name="rsidCode">The RSID code the audio begins with, or null where it begins with none.</param>
    /// <param name="announcementSeconds">How much of <paramref name="seconds"/> is the announcement.</param>
    /// <remarks>
    /// <para>**THE LENGTH AND NOT THE TEXT** (§2.1). A macro carries the operator's callsign
    /// twice over, and a count says everything a diagnosis needs.</para>
    /// <para>**AND WHETHER IT WAS ANNOUNCED** (work instruction 359 task 4). The seconds include
    /// the burst, so the record says the burst is there and which code it names.</para>
    /// <para>**`withinCap` MEASURES THE TEXT** (`PHASE_PLAN.md` R32 (a); work instruction 360),
    /// the burst taken off, as the sequence's cap does.</para>
    /// </remarks>
    public static void SendComposed(
        ITelemetry? telemetry,
        string kind,
        int characters,
        double seconds,
        double capSeconds,
        double? offsetHz,
        int? rsidCode = null,
        double announcementSeconds = 0)
        => telemetry?.Write(
            TelemetryCategory.Psk31,
            "psk31_send_composed",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["macro"] = kind,
                ["characters"] = characters,
                ["seconds"] = Math.Round(seconds, 2),
                ["capSeconds"] = capSeconds,
                ["withinCap"] = seconds - announcementSeconds <= capSeconds,
                ["offsetHz"] = offsetHz is { } hz ? Math.Round(hz, 1) : null,
                ["announced"] = rsidCode is not null,
                ["rsidCode"] = rsidCode,
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

    /// <summary>**A PSK31 contact was written to the log** (§R13, step 5).</summary>
    /// <param name="telemetry">The sink, or null.</param>
    /// <param name="rstSent">The RST that went out, or null where none did.</param>
    /// <param name="rstReceived">The RST that was read, or null where none was.</param>
    /// <param name="mode">How the record spells the mode: `PSK`.</param>
    /// <param name="submode">How it spells the submode: `PSK31`.</param>
    /// <param name="gridCarried">Whether the record carries the other station's grid.</param>
    /// <remarks>
    /// <para>**WHETHER A GRID WENT IN, AND NEVER WHICH** (work instruction 333 task 3). A
    /// PSK31 grid arrives in prose and is read or not, so a record without one is the first
    /// thing a thin log is diagnosed on - and the grid itself is personal, so it is a flag.
    /// </para>
    /// <para>**THE STAGE STEP 5 ADDS, SO IT WRITES ITS EVENT** (§R13). A contact that
    /// reached the log and a contact that was composed and never saved look identical
    /// from outside, and the difference is the whole subject of the step.</para>
    /// <para>**THE REPORTS ARE NOT PERSONAL AND THE STATION IS** (HM-DEC-018, §2.1).
    /// `599` says how well two radios heard each other; it names nobody, and it is the
    /// field a wrong log entry is diagnosed from. **No callsign, no grid and no text go
    /// in** - the operator has all three on his own screen, and a telemetry file is the
    /// one place they must never accumulate.</para>
    /// <para>**AN UNREAD REPORT IS ABSENT HERE TOO** (§0.0), so a file showing
    /// `rstReceived: null` is a contact whose report Hamlet never read rather than one
    /// it forgot to write down.</para>
    /// </remarks>
    public static void ContactLogged(
        ITelemetry? telemetry,
        string? rstSent,
        string? rstReceived,
        string? mode,
        string? submode,
        bool gridCarried)
        => telemetry?.Write(
            TelemetryCategory.Psk31,
            "psk31_contact_logged",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["rstSent"] = string.IsNullOrWhiteSpace(rstSent) ? null : rstSent,
                ["rstReceived"] = string.IsNullOrWhiteSpace(rstReceived) ? null : rstReceived,
                ["mode"] = mode,
                ["submode"] = submode,
                ["gridCarried"] = gridCarried,
            },
            TelemetryLevel.Info);

    /// <summary>**The mode's records appeared for the first time** (§R13, step 5).</summary>
    /// <param name="telemetry">The sink, or null.</param>
    /// <param name="records">How many records the mode's scope revealed.</param>
    /// <remarks>
    /// <para>**§3.1 IS AN ABSENCE, AND AN ABSENCE LEAVES NO TRACE.** Before the first
    /// PSK31 contact there is no PSK31 anything on the achievements screen, so the one
    /// moment that can be diagnosed afterwards is the moment it appears - and without
    /// this line, a screen that failed to grow and a screen that grew while he was
    /// looking elsewhere are the same silence.</para>
    /// <para>**A COUNT AND NOTHING ELSE** (HM-DEC-018, §2.1). Not which records, not
    /// the station that earned them, not the grid: how many things he can now see.
    /// </para>
    /// </remarks>
    public static void RecordsRevealed(ITelemetry? telemetry, int records)
        => telemetry?.Write(
            TelemetryCategory.Psk31,
            "psk31_records_revealed",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["records"] = records,
            },
            TelemetryLevel.Info);

    /// <summary>The operator pressed capture and the recorder started.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="mode">The mode the tab was on, "psk31" or "olivia".</param>
    /// <param name="dialHz">Where the radio was, or 0 where it is unknown.</param>
    /// <param name="sampleRate">The device rate the audio is being kept at.</param>
    /// <param name="seconds">How long it will run for unless it is stopped.</param>
    /// <remarks>
    /// <para>**THE DEVICE RATE, BECAUSE THAT IS WHAT THE FILE WILL BE** (work instruction 344
    /// task 1). The capture is taken before the resampler, so this is not the rate the
    /// demodulators read at, and a reader matching a file to a session needs the one the
    /// file carries.</para>
    /// <para>**THE MODE NAMES THE EVENT AND TRAVELS IN IT** (work instruction 358 task 3). The
    /// same press is on the Olivia panel, and a capture of Olivia audio written down as a
    /// PSK31 capture would send the next unit to prove the wrong demodulator against it.
    /// PSK31's event keeps its name and its category; Olivia's is `olivia_capture_started`
    /// under diagnostics, because Olivia has no category of its own.</para>
    /// </remarks>
    public static void CaptureStarted(
        ITelemetry? telemetry, string mode, long dialHz, int sampleRate, double seconds)
        => telemetry?.Write(
            CaptureCategory(mode),
            mode + "_capture_started",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["mode"] = mode,
                ["dialHz"] = dialHz > 0 ? dialHz : null,
                ["deviceSampleRate"] = sampleRate > 0 ? sampleRate : null,
                ["seconds"] = seconds,
            },
            TelemetryLevel.Info);

    /// <summary>The recorder stopped and the file was written.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="seconds">How much audio it kept.</param>
    /// <param name="bytes">How large the file is.</param>
    /// <param name="sha256">Its fingerprint, lower-case hexadecimal.</param>
    /// <param name="sampleRate">The rate the file was actually written at.</param>
    /// <param name="earlyStop">True where a second press ended it before its time.</param>
    /// <param name="held">The carriers held when it started, with their offsets and qualities.</param>
    /// <remarks>
    /// <para>**THE CARRIERS AS THEY WERE AT THE START**, so a file and a session can be
    /// matched afterwards: a capture made while four stations were held is evidence about
    /// four stations, and a capture made on an empty band is evidence about the band.
    /// </para>
    /// <para>**THE PATH IS NOT IN THE FILE** (HM-DEC-018, §2.1). It is on the screen, in
    /// words the operator can follow. A capture folder under `%AppData%` carries the
    /// account name, which is a person, and a fingerprint identifies the file without
    /// naming anybody.</para>
    /// </remarks>
    /// <param name="mode">The mode the tab was on when it started, "psk31" or "olivia".</param>
    public static void CaptureFinished(
        ITelemetry? telemetry,
        string mode,
        double seconds,
        long bytes,
        string sha256,
        int sampleRate,
        bool earlyStop,
        IReadOnlyList<Psk31ChannelState> held)
        => telemetry?.Write(
            CaptureCategory(mode),
            mode + "_capture_finished",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["mode"] = mode,
                ["seconds"] = Math.Round(seconds, 2),
                ["bytes"] = bytes,
                ["sha256"] = sha256,

                // **THE RATE THE FILE IS, MEASURED FROM THE FILE.** The started event
                // asks the resampler, which may not exist yet when the press lands; this
                // one is written from the audio that was kept and cannot be absent.
                ["deviceSampleRate"] = sampleRate,
                ["earlyStop"] = earlyStop,
                ["carriersHeld"] = held.Count,
                ["carriers"] = held
                    .Select(c => new Dictionary<string, object?>(StringComparer.Ordinal)
                    {
                        ["offsetHz"] = Math.Round(c.OffsetHz, 1),
                        ["quality"] = Math.Round(c.Quality, 3),
                        ["open"] = c.Open,
                    })
                    .ToList(),
            },
            TelemetryLevel.Info);

    /// <summary>Where a capture event is filed, by the mode it was made on.</summary>
    /// <param name="mode">"psk31" or "olivia".</param>
    /// <returns>PSK31's own category for PSK31, and diagnostics for any other mode.</returns>
    private static TelemetryCategory CaptureCategory(string mode)
        => string.Equals(mode, "psk31", StringComparison.Ordinal)
            ? TelemetryCategory.Psk31
            : TelemetryCategory.Diagnostics;

    /// <summary>A number as the record spells it.</summary>
    internal static string Say(double value)
        => value.ToString("0.###", CultureInfo.InvariantCulture);
}
