using Hamlet.RadioEngine.Rig;

namespace Hamlet.RadioEngine.Civ;

/// <summary>
/// How much a write can cost if it is wrong (HM-DEC-084).
/// </summary>
/// <remarks>
/// **THE TIER IS THE SAFETY DESIGN**, rather than a confirmation dialog on
/// everything. Asking permission four times for four changes that cannot put a
/// signal on the air is exactly the protectiveness this ruling exists to remove:
/// it trains somebody to click through prompts, which is worse than not having
/// them. What earns a prompt is what can be heard by other people.
/// </remarks>
public enum RigWriteTier
{
    /// <summary>
    /// Receive side. Cannot put anything on the air, so Hamlet does it and says
    /// what it did.
    /// </summary>
    Receive,

    /// <summary>
    /// Changes what the operator sounds like to everybody else. Offered, never
    /// simply done.
    /// </summary>
    Transmitted,

    /// <summary>
    /// **This one keys the radio.** Same gate, same visibility and same record
    /// as a CW send (§0.2), and never automatic.
    /// </summary>
    Keys,
}

/// <summary>
/// One documented CI-V write: the bytes that set it, and where the manual says
/// so.
/// </summary>
/// <param name="Field">What it sets.</param>
/// <param name="Command">The command byte.</param>
/// <param name="Page">The Full Manual page this comes from (HM-DEC-049).</param>
/// <param name="Note">What the manual says the data means.</param>
/// <param name="Tier">How much it can cost if it is wrong.</param>
/// <param name="SubCommand">The sub-command bytes, or empty.</param>
public sealed record CivWrite(
    RigField Field, byte Command, string Page, string Note,
    RigWriteTier Tier = RigWriteTier.Receive,
    byte[]? SubCommand = null)
{
    /// <summary>How this write is named in the diagnostics screen and the log.</summary>
    public string Label => SubCommand is { Length: > 0 } sub
        ? $"CI-V {Command:X2} {Convert.ToHexString(sub)}"
        : $"CI-V {Command:X2}";

    /// <summary>The sub-command bytes, never null.</summary>
    public byte[] Sub => SubCommand ?? Array.Empty<byte>();
}

/// <summary>
/// Every write Hamlet performs, with its manual citation.
/// </summary>
/// <remarks>
/// <para>THE FIRST TIME THIS APP CHANGES SOMEBODY'S RADIO (HM-DEC-056).
/// HM-DEC-050 was reads only and said in as many words that writing gets its own
/// ruling, because a read that goes wrong shows a wrong number and a write that
/// goes wrong moves a control the operator did not ask to have moved.</para>
/// <para>NOTHING HERE KEYS A TRANSMITTER. §0.2 is untouched: this is mode
/// selection and nothing else, and the CW message command stays where it is,
/// unused, until transmit gets its own conversation.</para>
/// <para>VERIFIED WITH A COLUMN-AWARE READ, which is the lesson HM-DEC-050 paid
/// for when a flattened two-column extraction put the CW pitch on <c>14 08</c>
/// instead of <c>14 09</c>. The same flattened text, still on disk from that
/// session, puts "Send/read CW pitch" against 08 to this day. Read again from
/// <c>IC-7300_Full_English v6</c> with <c>pdftotext -table</c>, and the pages
/// below are that edition's own.</para>
/// </remarks>
public static class CivWrites
{
    /// <summary>
    /// Set the operating mode, its data variant, and its filter.
    /// </summary>
    /// <remarks>
    /// <para>COMMAND 26 AND NOT 06, and the difference is the whole point.
    /// Command 06 sets a mode and a filter and has no way at all to say whether
    /// the data variant is wanted (p. 19-8). Command 26 carries the mode, a data
    /// mode flag and the filter, for the selected or unselected VFO (p. 19-11),
    /// and USB and USB-D are different facts to this radio: one is a voice
    /// setting with the microphone live and the other routes the computer's
    /// audio, which is the difference between hearing FT8 and hearing nothing
    /// useful.</para>
    /// <para>Data layout: VFO selector, then the operating mode, then the data
    /// mode setting, then the filter (p. 19-11). "Both data and filter settings
    /// can be skipped. In that case, DATA OFF and the default filter setting of
    /// the operating mode are automatically selected." Hamlet sends the data
    /// flag and skips the filter, so the radio picks the filter it would have
    /// picked for that mode itself, which is a better answer than any Hamlet
    /// could invent for somebody else's rig.</para>
    /// </remarks>
    public static CivWrite Mode { get; } = new(
        RigField.Mode, 0x26, "19-11",
        "00=selected VFO; then 00=LSB, 01=USB, 02=AM, 03=CW, 04=RTTY, 05=FM, "
        + "07=CW-R, 08=RTTY-R; then 00=data mode off, 01=data mode on; filter "
        + "skipped so the mode's own default is used");

    /// <summary>Which VFO a write applies to (p. 19-11).</summary>
    public const byte SelectedVfo = 0x00;

    /// <summary>Data mode off, i.e. the plain voice or Morse variant.</summary>
    public const byte DataModeOff = 0x00;

    /// <summary>Data mode on, i.e. the -D variant a computer talks through.</summary>
    public const byte DataModeOn = 0x01;


    // ---- Tier 1: the receive side -------------------------------------
    //
    // NOTHING BELOW THIS LINE CAN PUT A SIGNAL ON THE AIR, which is what makes
    // it the tier Hamlet may simply do (HM-DEC-084). Every row was read
    // column-aware from the Full Manual, publication A7292-4EX-6, on the pages
    // named, because a wrong sub-command on a WRITE moves somebody's control
    // instead of returning a bad number, and `14 08` is the standing warning
    // (HM-DEC-050).

    /// <summary>Auto notch, which hunts a steady tone and removes it.</summary>
    /// <remarks>
    /// THE ONE THAT PROMPTED THIS WHOLE RULING. A Morse note is a steady tone
    /// switching on and off, so in CW the auto notch hunts the signal being
    /// decoded. Hamlet has printed that on a diagnostics screen for weeks and
    /// could not act on it.
    /// </remarks>
    public static CivWrite AutoNotch { get; } = new(
        RigField.AutoNotch, 0x16, "19-3", "00=off, 01=on", RigWriteTier.Receive,
        new byte[] { 0x41 });

    /// <summary>Manual notch.</summary>
    public static CivWrite ManualNotch { get; } = new(
        RigField.ManualNotch, 0x16, "19-3", "00=off, 01=on", RigWriteTier.Receive,
        new byte[] { 0x48 });

    /// <summary>Where the manual notch sits.</summary>
    public static CivWrite NotchPosition { get; } = new(
        RigField.NotchPosition, 0x14, "19-3",
        "0000=max CCW, 0128=center, 0255=max CW", RigWriteTier.Receive,
        new byte[] { 0x0D });

    /// <summary>The noise blanker.</summary>
    /// <remarks>
    /// It mutes the instant a sharp tick arrives, and on a busy band a strong
    /// nearby signal looks like a tick.
    /// </remarks>
    public static CivWrite NoiseBlanker { get; } = new(
        RigField.NoiseBlanker, 0x16, "19-3", "00=off, 01=on", RigWriteTier.Receive,
        new byte[] { 0x22 });

    /// <summary>How hard the noise blanker works.</summary>
    public static CivWrite NoiseBlankerLevel { get; } = new(
        RigField.NoiseBlankerLevel, 0x14, "19-3", "0000 to 0255 = 0 to 100%",
        RigWriteTier.Receive, new byte[] { 0x12 });

    /// <summary>Noise reduction, which softens the edges a decoder times by.</summary>
    public static CivWrite NoiseReduction { get; } = new(
        RigField.NoiseReduction, 0x16, "19-3", "00=off, 01=on", RigWriteTier.Receive,
        new byte[] { 0x40 });

    /// <summary>How hard the noise reduction works.</summary>
    public static CivWrite NoiseReductionLevel { get; } = new(
        RigField.NoiseReductionLevel, 0x14, "19-3", "0000 to 0255 = 0 to 100%",
        RigWriteTier.Receive, new byte[] { 0x06 });

    /// <summary>
    /// The AGC speed.
    /// </summary>
    /// <remarks>
    /// **THE MANUAL HAS FOUR VALUES AND NOT THREE.** The row reads "00 to 03"
    /// with `*(00=OFF, 01=FAST, 02=MID, 03=SLOW)`, so AGC can be switched off
    /// entirely. A table that started at FAST would have no way to say off and
    /// no way to put it back if somebody had it off (HM-DEC-084).
    /// </remarks>
    public static CivWrite Agc { get; } = new(
        RigField.Agc, 0x16, "19-3", "00=off, 01=fast, 02=mid, 03=slow",
        RigWriteTier.Receive, new byte[] { 0x12 });

    /// <summary>The preamp.</summary>
    public static CivWrite Preamp { get; } = new(
        RigField.Preamp, 0x16, "19-3", "00=off, 01=P.AMP1, 02=P.AMP2",
        RigWriteTier.Receive, new byte[] { 0x02 });

    /// <summary>The attenuator.</summary>
    /// <remarks>Two values and they are not 0 and 1: 00 is off and 20 is 20 dB.</remarks>
    public static CivWrite Attenuator { get; } = new(
        RigField.Attenuator, 0x11, "19-3", "00=off, 20=20 dB", RigWriteTier.Receive);

    /// <summary>The receive gain.</summary>
    /// <remarks>
    /// THE ONE THAT COST TWO HOURS. It sat at 42 percent and the receiver was
    /// deaf all evening, and nothing on screen said so.
    /// </remarks>
    public static CivWrite RfGain { get; } = new(
        RigField.RfGain, 0x14, "19-3", "0000 to 0255", RigWriteTier.Receive,
        new byte[] { 0x02 });

    /// <summary>The squelch threshold.</summary>
    public static CivWrite Squelch { get; } = new(
        RigField.Squelch, 0x14, "19-3", "0000 to 0255", RigWriteTier.Receive,
        new byte[] { 0x03 });

    /// <summary>The IF filter width, as an index on the radio's own scale.</summary>
    /// <remarks>
    /// The command table gives the endpoints and the steps are on p. 4-6
    /// (HM-DEC-071). The one that read garbage all evening because the filter
    /// was wide open in CW.
    /// </remarks>
    public static CivWrite FilterWidth { get; } = new(
        RigField.FilterBandwidth, 0x1A, "19-4",
        "00 to 49; other than AM 00=50 Hz to 40=3600 Hz, scale on p. 4-6",
        RigWriteTier.Receive, new byte[] { 0x03 });

    /// <summary>The DSP filter shape.</summary>
    public static CivWrite FilterShape { get; } = new(
        RigField.FilterShape, 0x16, "19-4", "00=sharp, 01=soft", RigWriteTier.Receive,
        new byte[] { 0x56 });

    /// <summary>The pitch a Morse signal is heard at.</summary>
    /// <remarks>`14 09` and not `14 08`, which is the outer Twin PBT (HM-DEC-050).</remarks>
    public static CivWrite CwPitch { get; } = new(
        RigField.CwPitch, 0x14, "19-3",
        "0000=300 Hz, 0128=600 Hz, 0255=900 Hz, 5 Hz steps", RigWriteTier.Receive,
        new byte[] { 0x09 });

    /// <summary>The audio level out of the speaker.</summary>
    public static CivWrite AfLevel { get; } = new(
        RigField.AfLevel, 0x14, "19-3", "0000 to 0255", RigWriteTier.Receive,
        new byte[] { 0x01 });

    /// <summary>Which signal leaves the ACC and USB sockets.</summary>
    public static CivWrite AccUsbOutput { get; } = new(
        RigField.AccUsbOutputSelect, 0x1A, "19-4", "00=AF, 01=IF",
        RigWriteTier.Receive, new byte[] { 0x05, 0x00, 0x59 });

    /// <summary>How loud the audio to the computer is.</summary>
    public static CivWrite AccUsbAfLevel { get; } = new(
        RigField.AccUsbAfLevel, 0x1A, "19-4", "0000 to 0255", RigWriteTier.Receive,
        new byte[] { 0x05, 0x00, 0x60 });

    /// <summary>Whether the squelch gates the audio to the computer.</summary>
    public static CivWrite AccUsbSquelch { get; } = new(
        RigField.AccUsbSquelch, 0x1A, "19-5", "00=off, 01=on", RigWriteTier.Receive,
        new byte[] { 0x05, 0x00, 0x61 });

    /// <summary>
    /// What the RF/SQL knob does.
    /// </summary>
    /// <remarks>
    /// **SET TO 01 THE KNOB DOES SQUELCH ONLY AND THE RF GAIN IS FIXED AT
    /// MAXIMUM**, which makes the two-hour deaf-receiver failure impossible. It
    /// is still offered once and explained rather than set silently, because it
    /// changes what a physical knob on somebody's radio does, and a control that
    /// stops behaving the way its owner expects is worse than the problem it
    /// solves (HM-DEC-084).
    /// </remarks>
    public static CivWrite RfSqlFunction { get; } = new(
        RigField.RfSqlFunction, 0x1A, "19-4",
        "00=auto, 01=SQL only with RF gain fixed at maximum, 02=RF+SQL",
        RigWriteTier.Receive, new byte[] { 0x05, 0x00, 0x25 });

    // ---- Tier 2: what the operator sounds like ------------------------

    /// <summary>How much power goes out.</summary>
    public static CivWrite RfPower { get; } = new(
        RigField.RfPower, 0x14, "19-3", "0000 to 0255", RigWriteTier.Transmitted,
        new byte[] { 0x0A });

    /// <summary>How fast the keyer sends.</summary>
    public static CivWrite KeyerSpeed { get; } = new(
        RigField.KeyerSpeed, 0x14, "19-3", "0000=6 WPM, 0255=48 WPM",
        RigWriteTier.Transmitted, new byte[] { 0x0C });

    /// <summary>Break-in, which is what lets a keyer message reach the air.</summary>
    public static CivWrite BreakIn { get; } = new(
        RigField.BreakIn, 0x16, "19-3", "00=off, 01=semi, 02=full",
        RigWriteTier.Transmitted, new byte[] { 0x47 });

    /// <summary>How long break-in waits before dropping back to receive.</summary>
    public static CivWrite BreakInDelay { get; } = new(
        RigField.BreakInDelay, 0x14, "19-3", "0000 to 0255",
        RigWriteTier.Transmitted, new byte[] { 0x0F });

    // ---- Tier 3: this one keys the radio ------------------------------

    /// <summary>
    /// The antenna tuner. **Value 02 starts a tuning cycle and keys the radio.**
    /// </summary>
    /// <remarks>
    /// <para>Read column-aware from p. 19-7: `01*  00 to 02`, "00=Send/read the
    /// antenna tuner OFF", "01=Send/read the antenna tuner ON", "02=Send/read to
    /// tuning". That third value transmits.</para>
    /// <para>It goes through the same gate, the same visibility and the same
    /// record as a CW send (§0.2, HM-DEC-084), and it is never automatic. It is
    /// offered clearly, because holding TUNER for a second is the documented fix
    /// for a high standing wave ratio (p. 11-2) and nobody should have to know
    /// that.</para>
    /// </remarks>
    public static CivWrite AntennaTuner { get; } = new(
        RigField.AntennaTuner, 0x1C, "19-7",
        "00=off, 01=on, 02=start a tuning cycle, which keys the radio",
        RigWriteTier.Keys, new byte[] { 0x01 });

    /// <summary>Start a tuning cycle, which transmits (p. 19-7).</summary>
    public const byte TuneNow = 0x02;

    /// <summary>
    /// Send the scope's waveform data to the computer (`27 11`, HM-DEC-092).
    /// </summary>
    /// <remarks>
    /// <para>**IT IS SEND/READ, AND IT IS AN ORDINARY TIER ONE WRITE.** The
    /// command table lists it with every other scope setting on p. 19-7,
    /// `00=OFF, 01=ON`. Nothing about it can put a signal on the air: it decides
    /// whether the picture the radio is already drawing on its own screen is also
    /// sent down the cable.</para>
    /// <para>The application read this setting, found it off, and printed a
    /// paragraph telling the operator to go and change two menu settings, while
    /// never once attempting the write it had a whole cited write layer for
    /// (HM-DEC-084).</para>
    /// <para>**THE PRECONDITIONS ARE REAL AND THEY ARE NOT A REASON TO DECLINE IN
    /// ADVANCE.** Footnote 4 on p. 19-7 says this can only be set with CI-V USB
    /// Port on "Unlink from [REMOTE]" and the USB baud rate at 115200. Hamlet
    /// knows the second from the port it opened itself and cannot read the first
    /// (HM-OPEN-013), so the honest move is to try it and report what the radio
    /// said, rather than guess which of two settings is at fault and send
    /// somebody across the room (§0.0).</para>
    /// </remarks>
    public static CivWrite ScopeOutput { get; } = new(
        RigField.ScopeOutput, 0x27, "19-7", "00=off, 01=on", RigWriteTier.Receive,
        new byte[] { 0x11 });

    /// <summary>Every write, so the diagnostics screen can list them.</summary>
    /// <remarks>
    /// **`16 65`, IP+, IS DELIBERATELY ABSENT.** Its row reads "Send the IP+
    /// function setting" where every neighbor reads "Send/read", so the manual
    /// documents no way to read it back. A write that cannot be confirmed and
    /// cannot be undone is not a write this app makes (HM-DEC-084), and the
    /// asterisk that would have said otherwise is not there. Recorded rather
    /// than quietly skipped, because the next session will see the row and
    /// wonder.
    /// </remarks>
    public static IReadOnlyList<CivWrite> All { get; } = new[]
    {
        Mode, ScopeOutput,
        AutoNotch, ManualNotch, NotchPosition,
        NoiseBlanker, NoiseBlankerLevel, NoiseReduction, NoiseReductionLevel,
        Agc, Preamp, Attenuator, RfGain, Squelch,
        FilterWidth, FilterShape, CwPitch, AfLevel,
        AccUsbOutput, AccUsbAfLevel, AccUsbSquelch, RfSqlFunction,
        RfPower, KeyerSpeed, BreakIn, BreakInDelay,
        AntennaTuner,
    };

    /// <summary>The two BCD bytes for a 0 to 255 level (p. 19-3).</summary>
    /// <param name="level">The value.</param>
    /// <returns>Hundreds byte, then tens and units.</returns>
    /// <remarks>
    /// The radio writes these as decimal digits rather than as a plain number,
    /// so 128 goes out as `01 28` and not as `0x80`. Sending it as a plain byte
    /// would put the CW pitch at 428 Hz when the operator asked for 600.
    /// </remarks>
    public static byte[] LevelBytes(int level)
    {
        var clamped = Math.Clamp(level, 0, 255);

        return new[]
        {
            (byte)(clamped / 100),
            (byte)((((clamped / 10) % 10) << 4) | (clamped % 10)),
        };
    }

    /// <summary>The data bytes for a mode write.</summary>
    /// <param name="mode">The mode to select.</param>
    /// <param name="dataMode">Whether the data variant is wanted.</param>
    /// <returns>The data area, sub-command included.</returns>
    public static byte[] ModeData(CivMode mode, bool dataMode)
        => new[]
        {
            SelectedVfo,
            (byte)(int)mode,
            dataMode ? DataModeOn : DataModeOff,
        };
}
