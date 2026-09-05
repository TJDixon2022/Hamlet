using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Rig;

namespace Hamlet.RadioEngine.Audio;

/// <summary>What the audio actually came through, as the machine reported it.</summary>
/// <param name="DeviceName">What the operating system calls the input, or "".</param>
/// <param name="SampleRate">The rate the source reported, or null when unread.</param>
/// <param name="ChannelCount">
/// How many channels the device delivers, or null when the source cannot say.
/// </param>
/// <param name="Encoding">
/// The device's encoding and bit depth as the driver reports it, or "" when unread.
/// </param>
/// <param name="IsSimulated">
/// True where the samples were synthesized rather than received off the air
/// (HM-DEC-026), or null where there was no source at all.
/// </param>
/// <param name="Health">What Windows was doing to the input (HM-DEC-088).</param>
/// <remarks>
/// **A RECORD THAT DOES NOT DESCRIBE THE PATH IT RAN ON IS BROKEN IN THE PLACE
/// NOBODY CHECKS** (`CLAUDE.md` §0.0.1). The same absence left the CW filter
/// anomaly of 2026-08-31 unresolvable, and on 2026-09-03 an FT8 bench check
/// produced an empty table with nothing on disk saying which sound card it had
/// been listening to.
/// </remarks>
public sealed record AudioPath(
    string DeviceName,
    int? SampleRate,
    int? ChannelCount,
    string Encoding,
    bool? IsSimulated,
    CaptureHealth Health)
{
    /// <summary>Nothing about the path was read.</summary>
    public static AudioPath Unknown { get; } =
        new("", null, null, "", null, CaptureHealth.Unknown);
}

/// <summary>
/// The sheet written beside a digital capture, so a later reader can tell
/// whether a fault was in the signal, the radio, or Hamlet.
/// </summary>
/// <remarks>
/// <para>**THIS IS §0.0.1 AND NOTHING ELSE** (work instruction 041, task 1). The
/// operator photographed a waterfall on 2026-08-28 and the picture was read as a
/// signal problem twice, wrongly, because **the radio's mode and filter existed
/// in no file Hamlet wrote.** He was in CW at 500 Hz under a three-kilohertz
/// block, and no amount of looking at the screenshot could have said so.</para>
/// <para>**MODE AND THE DATA FLAG ARE SEPARATE LINES ON PURPOSE.** `USB` and
/// `USB-D` are different radios to an operator, and folding them into one line is
/// exactly the ambiguity that cost an hour. A sheet that says `USB` when the flag
/// was never read is the guess §0.0 forbids.</para>
/// <para>**EVERY ROW SAYS MEASURED OR UNKNOWN AND NOTHING IS DEFAULTED.** A value
/// nobody read says so, the way the "What the radio is doing" window already
/// does; a plausible number in its place is worse than a gap, because it will be
/// believed.</para>
/// <para>**AND IT SAYS WHICH AUDIO PATH IT RAN ON, AND WHAT THE SLOTS DID** (unit
/// 233). Mode and filter were the gap of 2026-08-28; the sound card, the slot
/// geometry and the stage each candidate reached were the gap of 2026-09-03, when
/// an FT8 bench check produced an empty table and no file that could say
/// why.</para>
/// <para>**AND IT IS NOT `CwCaseRoster`.** That roster scores the CW decoder and
/// every row of it asserts the operator heard a station Hamlet failed to read
/// (Tim's ruling of 2026-08-28). A digital press is not a CW case.</para>
/// </remarks>
public static class DigitalCaptureSheet
{
    /// <summary>How the sheet reports a value that was never read.</summary>
    public const string Unread = "unknown (not read)";

    /// <summary>Compose the sheet.</summary>
    /// <param name="capturedUtc">
    /// When the press happened, which is the **end** of the window.
    /// </param>
    /// <param name="seconds">How much audio was kept.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="state">The radio as Hamlet believed it at the press.</param>
    /// <param name="clock">The clock offset and its age.</param>
    /// <param name="nowUtc">The moment, for the clock's age.</param>
    /// <param name="neighborhood">Where the dial is, in words, or "".</param>
    /// <param name="needsHz">
    /// How much passband the block needs, or null where none is stated.
    /// </param>
    /// <param name="audioPath">
    /// What the audio came through, or null where nothing was read. **§0.0.1**: a
    /// sheet that does not describe the path it ran on cannot separate a deaf
    /// decoder from the wrong sound card.
    /// </param>
    /// <param name="census">
    /// How far each slot's candidates got, or null where nothing was decoded.
    /// **The press already decodes before it writes** — `CaptureDigital` calls
    /// `ShowDecodes` first, deliberately — so this is passed in rather than
    /// decoded a second time.
    /// </param>
    /// <param name="arrival">
    /// What the audio path delivered, as counts. Default is nothing measured,
    /// which the sheet says rather than guessing.
    /// </param>
    /// <param name="refusal">
    /// Why the reading produced nothing, in the reader's own words, or "".
    /// </param>
    /// <param name="decodes">
    /// The messages the slots gave up, or null where nobody handed any over
    /// (unit 251). **Passed in rather than decoded again**, on the same reasoning
    /// as <paramref name="census"/>. Null and empty are different facts and the
    /// sheet prints different lines for them.
    /// </param>
    /// <returns>The sheet, ready to write.</returns>
    public static string Compose(
        DateTime capturedUtc,
        double seconds,
        int sampleRate,
        RigState state,
        ClockOffset clock,
        DateTime nowUtc,
        string neighborhood,
        long? needsHz,
        AudioPath? audioPath = null,
        IReadOnlyList<Ft8SlotCensus>? census = null,
        string refusal = "",
        AudioArrival arrival = default,
        IReadOnlyList<Ft8Decode>? decodes = null)
    {
        ArgumentNullException.ThrowIfNull(state);

        var sheet = new StringBuilder();

        // Eleven, which every key written before unit 233 fits inside. A longer key
        // takes one space rather than running into its own value, which is what
        // `audioIsReal` did the first time it was written.
        void Line(string key, string value)
            => sheet
                .Append(key.PadRight(Math.Max(11, key.Length + 1)))
                .Append(value)
                .Append('\n');

        // **BOTH ENDS, LABELLED, AND WHICH ONE THE PRESS IS** (work instruction
        // 042, task 5). The sheet used to carry one line reading
        // `captured 20:47:20` and nothing saying whether that was the start of
        // the thirty seconds or the moment of the button. Analysis of the
        // operator's own file found FT8 keying on a clean fifteen-second cycle
        // sitting 2.4 seconds off where a slot boundary should fall, **and the
        // ambiguity is the reason that could not be resolved**: a window read
        // from the wrong end is out by its own length.
        //
        // The press is the end. The button keeps audio that has already
        // arrived, so the window runs backwards from it.
        var startUtc = capturedUtc.AddSeconds(-seconds);

        Line("windowFrom", Stamp(startUtc));
        Line("windowTo", Stamp(capturedUtc));
        Line("press", Stamp(capturedUtc)
            + "  (the END of the window: the button keeps audio that had "
            + "already arrived, so the window runs backwards from it)");
        Line("seconds", seconds.ToString("0.0", CultureInfo.InvariantCulture));
        Line("sampleRate", sampleRate.ToString(CultureInfo.InvariantCulture));

        // **THE FILE IS NOT TRIMMED AND SAYS SO** (Tim's ruling of 2026-08-28).
        // A thirty-second grab starting mid-slot leaves WSJT-X two partial slots
        // it cannot score, so these are diagnostic material rather than corpus,
        // and a later scoring run has to be able to tell without opening the
        // audio.
        Line("trimmed", "no  (raw ring, not aligned to slot boundaries; this is "
            + "diagnostic material and not scoring corpus)");

        sheet.Append('\n');

        // **THE THREE FIELDS WHOSE ABSENCE COST TWO HOURS**, first and apart.
        Line("mode", Describe(state[RigField.Mode]));
        Line("dataMode", DescribeDataMode(state));
        Line("filterSlot", Describe(state[RigField.FilterSelection]));
        Line("filterHz", DescribeWidth(state, needsHz));

        sheet.Append('\n');

        Line("frequency", Describe(state[RigField.Frequency]));
        Line("block", neighborhood.Length == 0 ? "not on the map" : neighborhood);
        Line("needsHz", needsHz is { } n
            ? n.ToString(CultureInfo.InvariantCulture)
              + "  (every signal here is an audio tone above the dial)"
            : "no passband requirement stated for this block");

        sheet.Append('\n');

        Line("clock", clock.Describe(nowUtc));

        sheet.Append('\n');

        // The rest of what the "What the radio is doing" window reads. That
        // window is §0.0.1 working; the sheet holds the same set so a capture is
        // as diagnosable as the screen was at the moment of the press.
        foreach (var field in new[]
        {
            RigField.SMeter, RigField.Overflow, RigField.Preamp,
            RigField.Attenuator, RigField.Agc, RigField.NoiseBlanker,
            RigField.NoiseReduction, RigField.RfGain, RigField.Squelch,
            RigField.TransmitStatus,
        })
        {
            Line(Name(field), Describe(state[field]));
        }

        sheet.Append('\n');

        // **THE PATH THE AUDIO ACTUALLY CAME THROUGH** (§0.0.1). Nothing above
        // this line says which sound card was open, at what rate, in how many
        // channels, or whether Windows was holding the level down before Hamlet
        // saw anything. On 2026-09-03 an empty table was the only artefact of a
        // bench check, and none of these could be answered afterwards.
        var path = audioPath ?? AudioPath.Unknown;

        Line("device", path.DeviceName.Length == 0 ? Unread : path.DeviceName);
        Line("deviceRate", path.SampleRate is { } deviceRate
            ? deviceRate.ToString(CultureInfo.InvariantCulture)
              + " Hz  (what the device reported; the file above was written at "
              + sampleRate.ToString(CultureInfo.InvariantCulture) + ")"
            : Unread);
        Line("channels", path.ChannelCount is { } channels
            ? channels.ToString(CultureInfo.InvariantCulture)
            : Unread + "  (the source does not report it)");
        Line("encoding", path.Encoding.Length == 0
            ? Unread + "  (the source does not report it)"
            : path.Encoding);
        Line("audioIsReal", path.IsSimulated switch
        {
            null => Unread + "  (no source was open)",
            true => "NO  (these samples were synthesized, not received off the air)",
            false => "yes  (received off the air)",
        });
        Line("windowsGain", path.Health.Gain is { } gain
            ? ((int)Math.Round(gain * 100)).ToString(CultureInfo.InvariantCulture)
              + " percent  (applied by Windows before Hamlet sees anything)"
            : Unread);
        Line("windowsMuted", path.Health.Muted switch
        {
            null => Unread,
            true => "YES  (nothing is reaching Hamlet at all)",
            false => "no",
        });

        sheet.Append('\n');

        // **THE SLOT GEOMETRY.** *Nothing decoded* and *nothing decodable was
        // captured* are different statements and only one of them is about the
        // decoder. A press lands wherever the operator's thumb lands, so a
        // thirty-second grab holds one whole slot, or two, or none.
        AppendGeometry(Line, sheet, startUtc, capturedUtc, clock, arrival);

        sheet.Append('\n');

        AppendCensus(Line, sheet, census, refusal);

        // **THE PER-MESSAGE LINES, AFTER THE CENSUS AND NOT INSIDE IT** (unit
        // 251). The census counts candidates through the stages; these are the
        // rows the operator was looking at. A refusal has no messages behind it,
        // and a sheet that printed "messages none" under a refusal would be
        // saying the band was empty rather than that nothing ran.
        if (refusal.Length == 0)
        {
            AppendDecodes(Line, sheet, decodes);
        }

        return sheet.ToString();
    }

    /// <summary>Where the slot boundaries fell inside the window.</summary>
    private static void AppendGeometry(
        Action<string, string> line,
        StringBuilder sheet,
        DateTime startPcUtc,
        DateTime endPcUtc,
        ClockOffset clock,
        AudioArrival arrival)
    {
        if (Ft8Slots.TrueUtc(startPcUtc, clock) is not { } from
            || Ft8Slots.TrueUtc(endPcUtc, clock) is not { } to)
        {
            line("slotGrid", Unread
                + "  (the clock offset has not been measured, so where the "
                + "fifteen-second boundaries fall is not known)");
            line("wholeSlots", Unread);
            return;
        }

        var boundaries = Ft8Slots.BoundariesBetween(from, to);
        var whole = 0;

        foreach (var boundary in boundaries)
        {
            if (boundary.AddSeconds(Ft8Slots.TransmissionSeconds) <= to)
            {
                whole++;
            }
        }

        // **WHETHER THIS FILE IS A RECORDING OR A COLLAGE**, which is the one
        // thing every line above assumes and none of them checks. On 2026-09-03
        // four consecutive press captures were byte-identical prefixes of one
        // another: the tap was filling at 13% of real time, so a thirty-second
        // file held about four minutes of fragments and every figure on the
        // sheet described it as though it were thirty seconds of band.
        //
        // **NOT MEASURED IS SAID AND NEVER GUESSED** (HM-DEC-009). A capture
        // taken before the tap had a slot's worth of history has no ratio, and
        // a zero there would read as a dead sound card.
        line("arrival", double.IsNaN(arrival.RecentRatio)
            ? Unread + "  (no arrival ratio was taken for this capture)"
            : arrival.RecentText
              + "  (samples the sound card delivered over the last fifteen "
              + "seconds, divided by the samples a continuous stream would "
              + "have delivered in the same wall clock; below 100% this file "
              + "is fragments with gaps in it rather than a recording)");

        line("audioPathDrops", string.Format(
            CultureInfo.InvariantCulture,
            "{0} chunks / {1} samples dropped by the decode queue, "
            + "{2} callback failure(s), {3} empty buffer(s), "
            + "longest callback {4:0} us, {5}, {6}",
            arrival.QueueDroppedChunks,
            arrival.QueueDroppedSamples,
            arrival.CallbackFailures,
            arrival.EmptyBuffers,
            arrival.LongestCallbackMicroseconds,
            // **THE LONGEST CALLBACK ON ITS OWN SAYS NOTHING WITHOUT A BUDGET.**
            // A reader months from now has no way to know what this device's
            // period was, so the figure that made the longest callback either
            // alarming or unremarkable travels with it (unit 239 task 4).
            arrival.CallbackBudgetText,
            // **THE PICTURE'S OWN COST, BESIDE THE AUDIO'S** (unit 240). The
            // waterfall used to do its whole transform on the device callback,
            // so a slow frame was lost audio; it is a lost row now, and a reader
            // months from now needs to see which of the two happened.
            arrival.FrameWorkerText));

        line("slotGrid", boundaries.Count == 0
            ? "no fifteen-second boundary falls inside this window at all"
            : $"{boundaries.Count} boundaries, corrected to UTC");

        // **ITS OWN FIELD, BECAUSE ZERO IS THE ANSWER THAT MATTERS.** A capture
        // holding no whole transmission decodes nothing however good the decoder
        // is, and a reader who cannot see that will blame the decoder.
        line("wholeSlots", whole == 0
            ? "0  (NO WHOLE TRANSMISSION IS INSIDE THIS AUDIO, so nothing here "
              + "could decode whatever was on the air)"
            : whole.ToString(CultureInfo.InvariantCulture)
              + $"  (of {boundaries.Count} boundaries, this many are followed by "
              + $"the whole {Ft8Slots.TransmissionSeconds:0.00} s transmission "
              + "inside the audio)");

        foreach (var boundary in boundaries)
        {
            // **THE SAME FUNCTION THE CUTTER USES.** These two lines printed
            // contradictory answers on ft8-2026-09-03-210644 because each had
            // its own arithmetic; now there is one.
            var fits = Ft8Slots.TransmissionFits((to - boundary).TotalSeconds);

            sheet
                .Append("  slot     ")
                .Append(Stamp(boundary))
                .Append(fits
                    ? "  whole transmission inside the audio"
                    : "  CUT SHORT: the audio ends before the transmission does")
                .Append('\n');
        }
    }

    /// <summary>How far each slot's candidates got.</summary>
    /// <remarks>
    /// **IT COUNTS AND IT DOES NOT INTERPRET** (`CLAUDE.md` §12.1). The rows say
    /// how many places were looked at and how many became words. They do not say
    /// what anybody said, and they do not conclude that the band was quiet.
    /// **A COSTAS MATCH COUNT IS NOT A SIGNAL-TO-NOISE RATIO** (§0.0) and the
    /// column is labelled for what it is. **Neither is the level on the audio
    /// line** (unit 236), which says how loud the slot's audio was and nothing
    /// about how strong a signal in it was; its own label says so on every row.
    /// </remarks>
    private static void AppendCensus(
        Action<string, string> line,
        StringBuilder sheet,
        IReadOnlyList<Ft8SlotCensus>? census,
        string refusal)
    {
        if (refusal.Length > 0)
        {
            // Verbatim. A paraphrase of a refusal is a different refusal, and the
            // operator read this sentence on the screen at the time.
            line("refusal", refusal);
            line("census", "nothing was decoded, so there is no census");
            return;
        }

        if (census is null)
        {
            line("refusal", "none");
            line("census", Unread + "  (no decode was handed to this sheet)");
            return;
        }

        // **WHICH DECODER READ THIS, AND WHAT WAS ON IN IT** (unit 249).
        // From tonight there is more than one decoder this project might have
        // used, and a candidate count means a different thing depending on
        // which produced it. Six sidecars from 2026-09-03 are readable today
        // only because they recorded their own conditions.
        line("decoder", census.Count == 0
            ? Unread + "  (no slot ran, so nothing read this)"
            : census[0].Decoder.ToString());

        // **WHAT THE PORT MADE OF THE SAME SLOTS, WHERE ANYBODY ASKED.** Off
        // by default, so "not run" is the ordinary line - and it says that
        // rather than printing zeroes, because nobody asked and the port found
        // nothing are opposite facts (§0.0).
        line("portComparison", census.Count == 0 || census[0].PortComparison is null
            ? "not run"
            : census[0].PortComparison!.Value.ToString());

        line("refusal", "none");
        line("census", census.Count == 0
            ? "no slot was cut and run"
            : $"{census.Count} slots, counts below");

        foreach (var slot in census)
        {
            sheet
                .Append("  slot     ")
                .Append(Stamp(slot.SlotStartUtc))
                .Append("  candidates ")
                .Append(slot.CandidateCount.ToString(CultureInfo.InvariantCulture))
                .Append("  parity ")
                .Append(slot.ParitySatisfiedCount.ToString(CultureInfo.InvariantCulture))
                .Append("  checksum ")
                .Append(slot.ChecksumPassedCount.ToString(CultureInfo.InvariantCulture))
                .Append("  text ")
                .Append(slot.BecameTextCount.ToString(CultureInfo.InvariantCulture))
                .Append("  duplicate ")
                .Append(slot.DuplicateCount.ToString(CultureInfo.InvariantCulture))
                .Append("  at ")
                .Append(slot.SampleRate.ToString(CultureInfo.InvariantCulture))
                .Append(" Hz  top Costas match counts ")
                .Append(slot.TopSyncScores.Count == 0
                    ? "none"
                    : string.Join(
                        ", ",
                        slot.TopSyncScores.Select(
                            s => s.ToString(CultureInfo.InvariantCulture))))
                .Append('\n');

            // **AND HOW LOUD THE AUDIO IN THE SLOT WAS** (unit 236). Every figure
            // on the line above describes the decode, so a muted sound card and a
            // quiet band wrote the same row — which is the fork the bench check of
            // 2026-09-03 died on. Its own line, because a level and a candidate
            // count are different kinds of thing and folding them together is the
            // ambiguity §0.0.1 exists to prevent.
            sheet
                .Append("  audio    ")
                .Append(Stamp(slot.SlotStartUtc))
                .Append("  ")
                .Append(DescribeLevel(slot.Level))
                .Append('\n');

            // **AND HOW STRONG THE STATIONS IN IT WERE** (unit 251). Its own
            // line for the same reason the level has one: a signal-to-noise
            // ratio and a level are different kinds of thing, and the line
            // above says in its own words that it is NOT one. This one is.
            sheet
                .Append("  snr      ")
                .Append(Stamp(slot.SlotStartUtc))
                .Append("  ")
                .Append(slot.SignalToNoise.ToString())
                .Append("  (2500 Hz reference bandwidth)")
                .Append('\n');
        }
    }

    /// <summary>
    /// One line per message, carrying the ratio, the offset and the tone (unit 251).
    /// </summary>
    /// <remarks>
    /// <para>**THE THIRD SURFACE.** The panel shows the operator the ratio while
    /// he is watching; the census line above summarises the slot; this is the row
    /// somebody reads six months later beside the WAV it was cut from, which is
    /// the whole reason the capture folder exists.</para>
    /// <para>**THE OFFSET AND THE TONE, AND NOT THE MESSAGE.** A station is
    /// identified inside its slot by where it sat — the dt and the hz are unique
    /// to it — and the audio is in the file beside this one. **Putting the
    /// decoded text here would start a second file recording who the operator
    /// heard**, and HM-DEC-018 rules that out for telemetry in exactly those
    /// words. It does not govern the capture folder, so this is a judgement and
    /// not a ruling, and it is left for Tim rather than taken quietly: the lines
    /// carry the measurement and the place, and adding the words is one edit if
    /// he wants them.</para>
    /// <para>**A DASH WHERE NOTHING WAS MEASURED**, which is the same token the
    /// panel shows and means the same thing: no ratio, rather than a floored one.
    /// </para>
    /// </remarks>
    private static void AppendDecodes(
        Action<string, string> line,
        StringBuilder sheet,
        IReadOnlyList<Ft8Decode>? decodes)
    {
        if (decodes is null)
        {
            line("messages", Unread + "  (no decode was handed to this sheet)");
            return;
        }

        if (decodes.Count == 0)
        {
            line("messages", "none");
            return;
        }

        line("messages", $"{decodes.Count}, one line each below");

        foreach (var decode in decodes)
        {
            sheet
                .Append("  message  ")
                .Append(Stamp(decode.SlotStartUtc))
                .Append("  snr ")
                .Append(decode.SignalToNoiseDb is { } decibels
                    ? decibels.ToString("+0.0;-0.0;+0.0", CultureInfo.InvariantCulture)
                    : "-")
                .Append(" dB  dt ")
                .Append(decode.OffsetSeconds.ToString("0.00", CultureInfo.InvariantCulture))
                .Append(" s  hz ")
                .Append(decode.FrequencyHz.ToString("0", CultureInfo.InvariantCulture))
                .Append("  Costas match ")
                .Append(decode.SyncScore.ToString(CultureInfo.InvariantCulture))
                .Append('\n');
        }
    }

    /// <summary>
    /// How loud one slot's audio was, or what is said instead (unit 236).
    /// </summary>
    /// <param name="level">What was measured.</param>
    /// <returns>One row's worth of level.</returns>
    /// <remarks>
    /// <para>**THREE STATES AND THEY ARE NOT THE SAME FACT** (`CLAUDE.md` §0.0).
    /// A slot nobody measured, a slot measured and found to be literally nought,
    /// and a slot with a level in it are three different answers, and collapsing
    /// any two of them is how the record came to be unable to tell a dead sound
    /// card from a quiet band in the first place.</para>
    /// <para>**AND AN ALL-ZERO SLOT HAS NO LOGARITHM** (HM-DEC-009). A floor
    /// written in its place is a plausible number in a column somebody will
    /// average, and the zero count beside it is what says why instead.</para>
    /// </remarks>
    private static string DescribeLevel(Ft8SlotLevel level)
    {
        if (level.SampleCount == 0)
        {
            return "level " + Unread + "  (no audio was handed to this sheet)";
        }

        var levels = level.PeakDbFullScale is { } peak
                     && level.RmsDbFullScale is { } rms
            ? "peak " + peak.ToString("0.00", CultureInfo.InvariantCulture)
                + "  rms " + rms.ToString("0.00", CultureInfo.InvariantCulture)
                + "  (dB relative to full scale, NOT a signal-to-noise ratio)"
            : "peak and rms none - every sample in this slot was exactly zero";

        return levels
            + "  samples "
            + level.SampleCount.ToString(CultureInfo.InvariantCulture)
            + "  exactly zero "
            + level.ZeroSampleCount.ToString(CultureInfo.InvariantCulture)
            + (level.ZeroSampleFraction is { } fraction
                ? "  ("
                    + fraction.ToString("0.000000", CultureInfo.InvariantCulture)
                    + " of the slot)"
                : "");
    }

    private static string Stamp(DateTime utc)
        => utc.ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture);

    /// <summary>The data variant, on its own line, never folded into the mode.</summary>
    private static string DescribeDataMode(RigState state)
    {
        var value = state[RigField.DataMode];

        if (!value.IsKnown)
        {
            return Unread
                + "  (so whether this is USB or USB-D is NOT established here)";
        }

        return value.Number is 1
            ? "on   (this is the -D variant: the computer's audio is routed)"
            : "off  (this is the plain voice or Morse variant)";
    }

    /// <summary>The passband in hertz, against what the block needs.</summary>
    private static string DescribeWidth(RigState state, long? needsHz)
    {
        var value = state[RigField.FilterBandwidth];

        if (!value.IsKnown || value.Number is not { } hertz)
        {
            return Unread + "  (so the passband is not established here)";
        }

        var width = ((int)hertz).ToString(CultureInfo.InvariantCulture) + " Hz";

        if (needsHz is not { } needed)
        {
            return $"{width}  (measured, {Age(value)})";
        }

        return hertz >= needed
            ? $"{width}  (measured, {Age(value)}; wide enough for the {needed} Hz "
              + "this block occupies)"
            : $"{width}  (measured, {Age(value)}; TOO NARROW for the {needed} Hz "
              + "this block occupies, so most of it cannot be heard)";
    }

    private static string Describe(RigValue value)
        => value.IsKnown
            ? $"{value.Text}  (measured, {Age(value)})"
            : $"{Unread}  ({value.Source})";

    private static string Age(RigValue value)
        => value.AtUtc is { } at
            ? $"read {at:HH:mm:ss} UTC via {value.Source}"
            : $"via {value.Source}";

    private static string Name(RigField field) => field switch
    {
        RigField.SMeter => "sMeter",
        RigField.Overflow => "overflow",
        RigField.Preamp => "preamp",
        RigField.Attenuator => "attenuator",
        RigField.Agc => "agc",
        RigField.NoiseBlanker => "noiseBlank",
        RigField.NoiseReduction => "noiseRed",
        RigField.RfGain => "rfGain",
        RigField.Squelch => "squelch",
        RigField.TransmitStatus => "transmit",
        _ => field.ToString(),
    };
}
