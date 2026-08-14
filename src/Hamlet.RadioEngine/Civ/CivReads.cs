using Hamlet.RadioEngine.Rig;

namespace Hamlet.RadioEngine.Civ;

/// <summary>
/// One documented CI-V read: the bytes that ask for it, and where the manual
/// says so.
/// </summary>
/// <param name="Field">What it reads.</param>
/// <param name="Command">The command byte.</param>
/// <param name="SubCommand">
/// The sub-command bytes, which are simply the first bytes of the data area. A
/// read sends them and the radio echoes them back in front of the payload, so
/// they are also how a response is matched to its request.
/// </param>
/// <param name="Page">
/// The Full Manual page this comes from, so anybody can check it (HM-DEC-049).
/// </param>
/// <param name="Note">What the manual says the values mean.</param>
/// <param name="AlsoAnswers">
/// Other fields this one read fills in. Mode and filter selection arrive from a
/// single command, so the filter is answered without ever being asked for, and
/// a caller that does not know that would either ask twice or conclude the
/// radio has no filter.
/// </param>
public sealed record CivRead(
    RigField Field,
    byte Command,
    byte[] SubCommand,
    string Page,
    string Note,
    RigField[]? AlsoAnswers = null)
{
    /// <summary>How this read is named in the diagnostics screen and the log.</summary>
    public string Label
        => SubCommand.Length == 0
            ? $"CI-V {Command:X2}"
            : $"CI-V {Command:X2} {Convert.ToHexString(SubCommand)}";

    /// <summary>Every field this one command fills in, its own included.</summary>
    public IReadOnlyList<RigField> Answers
        => AlsoAnswers is null or { Length: 0 }
            ? new[] { Field }
            : new[] { Field }.Concat(AlsoAnswers).ToList();
}

/// <summary>
/// A value the radio volunteers rather than one Hamlet asks for.
/// </summary>
/// <param name="Field">Which field arrives this way.</param>
/// <param name="Label">
/// The mechanism, named as itself: "transceive 00", never a poll command that is
/// not issued.
/// </param>
/// <param name="Page">The Full Manual page describing it.</param>
/// <param name="Note">What the frame carries.</param>
/// <remarks>
/// BROADCAST IS A PROVENANCE, NOT AN ABSENCE, and getting that wrong is what
/// made the diagnostics screen say the IC-7300 has no frequency while the face
/// of the radio was showing one. A field the radio pushes is supported and
/// populated; it simply has no poll command behind it, because asking for a
/// figure the radio already sent could only ever be more stale (HM-DEC-050).
/// </remarks>
public sealed record CivBroadcast(RigField Field, string Label, string Page, string Note);

/// <summary>
/// Every state read Hamlet performs, with its manual citation.
/// </summary>
/// <remarks>
/// <para>CITED, NOT RECALLED (HM-DEC-049, §4). Every command byte here was read
/// off the Full Manual's section 19 command table rather than remembered, and
/// the page is on the row so a future session can check one in a minute instead
/// of trusting this file. The manual itself is never committed: Icom permits
/// individual use and prohibits redistribution.</para>
/// <para>READS ONLY. Nothing here writes to the radio. Several of these
/// commands are documented as "send/read" and would set the value if given a
/// payload; they are issued with the sub-command alone, which is the read form.
/// Writing changes somebody's rig and gets its own ruling (HM-DEC-050).</para>
/// <para>WHAT IS DELIBERATELY ABSENT is as much the point as what is here. The
/// manual documents no read for which VFO is selected, so there is no entry for
/// it and the model reports it undocumented. Inventing a byte to fill that gap
/// is exactly what §4 forbids, and the radio would be the one to find out.</para>
/// </remarks>
public static class CivReads
{
    /// <summary>Read the operating frequency.</summary>
    /// <remarks>
    /// NOT POLLED, AND STILL READ ONCE. The radio broadcasts every change as the
    /// operator makes it, so asking over and over would spend bus traffic on a
    /// fact already in hand (HM-DEC-050). Nothing broadcasts what the radio was
    /// already sitting on before Hamlet arrived, though, so this is issued by the
    /// connect sweep and by the diagnostics screen's own refresh and at no other
    /// time. Its absence from this table was read as "the IC-7300 has no
    /// frequency", which is how the screen came to deny a value it was holding.
    /// </remarks>
    public static CivRead Frequency { get; } = new(
        RigField.Frequency, 0x03, Array.Empty<byte>(), "19-3",
        "the operating frequency, BCD, little-endian by byte pair");

    /// <summary>Read the operating mode and filter selection together.</summary>
    /// <remarks>
    /// One command answers two fields: the reply is one byte of mode and one of
    /// filter (p. 19-8, and the table row on 19-3). That is why the mode badge
    /// and the filter badge on the rig display refresh together, and why the
    /// filter is never asked for on its own. The data page was 19-9 here and it
    /// is 19-8 in this project's edition (HM-DEC-071).
    /// </remarks>
    public static CivRead ModeAndFilter { get; } = new(
        RigField.Mode, 0x04, Array.Empty<byte>(), "19-3, 19-8",
        "00=LSB, 01=USB, 02=AM, 03=CW, 04=RTTY, 05=FM, 07=CW-R, 08=RTTY-R; "
        + "then 01=FIL1, 02=FIL2, 03=FIL3",
        new[] { RigField.FilterSelection });

    /// <summary>
    /// Read the mode, its data variant and its filter, all three.
    /// </summary>
    /// <remarks>
    /// The same command Hamlet writes the mode with, in its read form
    /// (p. 19-11). It answers everything <see cref="ModeAndFilter"/> does and
    /// the data flag besides, which nothing else reports: command 04 says USB
    /// whether the radio is in USB or USB-D. Read on connect and when somebody
    /// opens the diagnostics screen, and not otherwise, since the mode itself is
    /// broadcast as it changes.
    /// </remarks>
    public static CivRead ModeDataAndFilter { get; } = new(
        RigField.DataMode, 0x26, new byte[] { 0x00 }, "19-11",
        "00=selected VFO; reply is mode, then 00=data mode off / 01=on, then "
        + "01=FIL1, 02=FIL2, 03=FIL3");

    /// <summary>Read the selected filter's width.</summary>
    /// <remarks>
    /// An index rather than a figure in hertz. The command table gives its
    /// endpoints and the two step scales between them are on p. 4-6, so both
    /// pages are needed and <see cref="CivFilterWidth"/> does the conversion.
    /// The page was 19-3 here and this row is on 19-4 (HM-DEC-071).
    /// </remarks>
    public static CivRead FilterWidth { get; } = new(
        RigField.FilterBandwidth, 0x1A, new byte[] { 0x03 }, "19-4",
        "00 to 49; AM 00=200 Hz to 49=10 kHz, other modes 00=50 Hz to "
        + "31/40=2700 Hz/3600 Hz");

    /// <summary>Read the S-meter.</summary>
    public static CivRead SMeter { get; } = new(
        RigField.SMeter, 0x15, new byte[] { 0x02 }, "19-3",
        "00 00=S0, 01 20=S9, 02 41=S9+60dB");

    /// <summary>Read whether the radio is receiving or transmitting.</summary>
    public static CivRead TransmitStatus { get; } = new(
        RigField.TransmitStatus, 0x1C, new byte[] { 0x00 }, "19-7",
        "00=receiving, 01=transmitting");

    /// <summary>Read whether the front end is overloading.</summary>
    public static CivRead Overflow { get; } = new(
        RigField.Overflow, 0x15, new byte[] { 0x07 }, "19-3",
        "00=clear, 01=overloading");

    /// <summary>Read the transmit power setting.</summary>
    public static CivRead RfPower { get; } = new(
        RigField.RfPower, 0x14, new byte[] { 0x0A }, "19-3",
        "00 00=minimum to 02 55=maximum");

    /// <summary>Read the receiver gain setting.</summary>
    public static CivRead RfGain { get; } = new(
        RigField.RfGain, 0x14, new byte[] { 0x02 }, "19-3",
        "00 00=minimum to 02 55=maximum");

    /// <summary>Read the squelch threshold.</summary>
    public static CivRead Squelch { get; } = new(
        RigField.Squelch, 0x14, new byte[] { 0x03 }, "19-3",
        "00 00=minimum to 02 55=maximum");

    /// <summary>Read whether the squelch is open right now.</summary>
    /// <remarks>
    /// A different question from the threshold, and the one somebody actually
    /// asks. "Is the squelch open" was one of the things the operator had to
    /// walk to the radio to answer, and the threshold alone does not answer it:
    /// a high threshold with a strong signal is open and a low one on a dead
    /// band is shut.
    /// </remarks>
    public static CivRead SquelchStatus { get; } = new(
        RigField.SquelchStatus, 0x15, new byte[] { 0x05 }, "19-3",
        "00=closed, 01=open");

    /// <summary>Read the AGC speed.</summary>
    public static CivRead Agc { get; } = new(
        RigField.Agc, 0x16, new byte[] { 0x12 }, "19-3",
        "01=FAST, 02=MID, 03=SLOW");

    /// <summary>Read the preamplifier setting.</summary>
    public static CivRead Preamp { get; } = new(
        RigField.Preamp, 0x16, new byte[] { 0x02 }, "19-3",
        "00=off, 01=preamp 1, 02=preamp 2");

    /// <summary>Read the attenuator setting.</summary>
    /// <remarks>
    /// The odd one out: the value is the attenuation in decibels expressed in
    /// BCD, so 20 dB reads back as the byte 0x20 rather than as an index
    /// (p. 19-3).
    /// </remarks>
    public static CivRead Attenuator { get; } = new(
        RigField.Attenuator, 0x11, Array.Empty<byte>(), "19-3",
        "00=off, 20=20 dB");

    /// <summary>Read whether the noise blanker is on.</summary>
    public static CivRead NoiseBlanker { get; } = new(
        RigField.NoiseBlanker, 0x16, new byte[] { 0x22 }, "19-3", "00=off, 01=on");

    /// <summary>Read how hard the noise blanker is working.</summary>
    public static CivRead NoiseBlankerLevel { get; } = new(
        RigField.NoiseBlankerLevel, 0x14, new byte[] { 0x12 }, "19-3",
        "00 00=0% to 02 55=100%");

    /// <summary>Read whether noise reduction is on.</summary>
    public static CivRead NoiseReduction { get; } = new(
        RigField.NoiseReduction, 0x16, new byte[] { 0x40 }, "19-3", "00=off, 01=on");

    /// <summary>Read how hard noise reduction is working.</summary>
    public static CivRead NoiseReductionLevel { get; } = new(
        RigField.NoiseReductionLevel, 0x14, new byte[] { 0x06 }, "19-3",
        "00 00=0% to 02 55=100%");

    /// <summary>Read whether the automatic notch is on.</summary>
    public static CivRead AutoNotch { get; } = new(
        RigField.AutoNotch, 0x16, new byte[] { 0x41 }, "19-3", "00=off, 01=on");

    /// <summary>Read whether the manual notch is on.</summary>
    public static CivRead ManualNotch { get; } = new(
        RigField.ManualNotch, 0x16, new byte[] { 0x48 }, "19-3", "00=off, 01=on");

    /// <summary>Read the break-in setting.</summary>
    public static CivRead BreakIn { get; } = new(
        RigField.BreakIn, 0x16, new byte[] { 0x47 }, "19-3",
        "00=off, 01=semi, 02=full");

    /// <summary>Read the internal keyer's speed.</summary>
    public static CivRead KeyerSpeed { get; } = new(
        RigField.KeyerSpeed, 0x14, new byte[] { 0x0C }, "19-3",
        "00 00=6 WPM, 02 55=48 WPM");

    /// <summary>Read the CW pitch.</summary>
    /// <remarks>
    /// SUB-COMMAND 09, NOT 08. The first reading of this table put it at 08,
    /// which is the outer Twin PBT position, because the two-column page had
    /// been flattened and the description landed against the wrong row. 08
    /// would have moved the passband instead of reading a pitch. Re-read from a
    /// column-aware extraction (p. 19-3), and the correction is recorded in
    /// HM-DEC-050.
    /// </remarks>
    public static CivRead CwPitch { get; } = new(
        RigField.CwPitch, 0x14, new byte[] { 0x09 }, "19-3",
        "00 00=300 Hz, 01 28=600 Hz, 02 55=900 Hz, 5 Hz steps");

    /// <summary>Read whether the computer is being sent AF or IF audio.</summary>
    public static CivRead AccUsbOutputSelect { get; } = new(
        RigField.AccUsbOutputSelect, 0x1A, new byte[] { 0x05, 0x00, 0x59 }, "19-4",
        "00=AF, 01=IF");

    /// <summary>Read how loud the audio sent to the computer is.</summary>
    public static CivRead AccUsbAfLevel { get; } = new(
        RigField.AccUsbAfLevel, 0x1A, new byte[] { 0x05, 0x00, 0x60 }, "19-4",
        "00 00=0% to 02 55=100%");

    /// <summary>Read whether the squelch gates the audio sent to the computer.</summary>
    public static CivRead AccUsbSquelch { get; } = new(
        RigField.AccUsbSquelch, 0x1A, new byte[] { 0x05, 0x00, 0x61 }, "19-5",
        "00=open regardless, 01=squelch gates it");

    /// <summary>Read whether the spectrum scope is switched on.</summary>
    /// <remarks>
    /// One of the two the waveform stream needs (p. 19-7). Read on connect and
    /// on demand rather than polled: a scope somebody switched on stays on, and
    /// the stream itself proves it far better than a command would.
    /// </remarks>
    public static CivRead ScopeOn { get; } = new(
        RigField.ScopeOn, 0x27, new byte[] { 0x10 }, "19-7", "00=off, 01=on");

    /// <summary>Read whether the scope data is being sent to the computer.</summary>
    /// <remarks>
    /// The other of the two (p. 19-7). Its own footnote adds two settings that
    /// are not commands at all: it can only be set with "Unlink from [REMOTE]"
    /// on the CI-V USB port screen and 115200 on the CI-V baud rate screen. A
    /// correct frame with neither of those in place is answered and does
    /// nothing.
    /// </remarks>
    public static CivRead ScopeOutput { get; } = new(
        RigField.ScopeOutput, 0x27, new byte[] { 0x11 }, "19-7", "00=off, 01=on");

    /// <summary>Read whether split is on.</summary>
    public static CivRead Split { get; } = new(
        RigField.Split, 0x0F, Array.Empty<byte>(), "19-3", "00=off, 01=on");

    /// <summary>Every read, in the order the diagnostics screen shows them.</summary>
    public static IReadOnlyList<CivRead> All { get; } = new[]
    {
        Frequency, ModeAndFilter, ModeDataAndFilter, FilterWidth, SMeter,
        TransmitStatus, Overflow,
        RfPower, RfGain, Squelch, SquelchStatus, Agc, Preamp, Attenuator,
        NoiseBlanker, NoiseBlankerLevel, NoiseReduction, NoiseReductionLevel,
        AutoNotch, ManualNotch, BreakIn, KeyerSpeed, CwPitch,
        AccUsbOutputSelect, AccUsbAfLevel, AccUsbSquelch, Split,
        ScopeOn, ScopeOutput,
    };

    /// <summary>
    /// Fields Hamlet would like and the manual documents no read for.
    /// </summary>
    /// <remarks>
    /// Recorded rather than guessed at (§4). The command table has `07 00` and
    /// `07 01` to SELECT VFO A or B and nothing that asks which one is
    /// currently selected, so the model says so instead of assuming A.
    /// </remarks>
    public static IReadOnlyDictionary<RigField, string> Undocumented { get; } =
        new Dictionary<RigField, string>
        {
            [RigField.Vfo] =
                "the command table has 07 00 and 07 01 to select VFO A or B and "
                + "nothing that asks which one is selected (p. 19-3)",
        };

    /// <summary>
    /// Fields the radio pushes without being asked.
    /// </summary>
    /// <remarks>
    /// <para>THE STATE THIS TABLE EXISTS TO KEEP OUT OF THE MODEL is
    /// <see cref="RigValueState.Unsupported"/>. A field with no poll command is
    /// not a field the radio lacks, and treating the two alike put "not on this
    /// radio" against the frequency while the IC-7300's own face was showing
    /// it.</para>
    /// <para>Transceive has to be on at the radio for any of this to arrive. When
    /// it is off nothing is pushed, the fields simply stay unknown until the
    /// connect sweep or a refresh reads them, and unknown is the honest answer
    /// for a value nobody has heard yet (§0.0).</para>
    /// </remarks>
    public static IReadOnlyList<CivBroadcast> Broadcasts { get; } = new[]
    {
        new CivBroadcast(
            RigField.Frequency, "transceive 00", "19-3",
            "the radio sends the new frequency as the operator turns the dial"),
        new CivBroadcast(
            RigField.Mode, "transceive 01", "19-3",
            "the radio sends the new mode, usually with the filter, as the "
            + "operator changes it"),
        new CivBroadcast(
            RigField.FilterSelection, "transceive 01", "19-3",
            "arrives on the back of the mode report"),
    };

    /// <summary>The read for a field, or null when there is none.</summary>
    /// <param name="field">The field.</param>
    /// <returns>The read, or null.</returns>
    public static CivRead? For(RigField field)
        => All.FirstOrDefault(r => r.Field == field);

    /// <summary>
    /// The read that fills this field in as a side effect, or null.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <returns>The read that answers it, or null when nothing does.</returns>
    /// <remarks>
    /// Only ever the filter selection today. It matters because a sweep that
    /// walked every field and found no command for this one used to conclude the
    /// radio had no filter, moments after another command had reported which
    /// filter was selected.
    /// </remarks>
    public static CivRead? AnsweredBy(RigField field)
        => All.FirstOrDefault(r => r.Field != field && r.Answers.Contains(field));

    /// <summary>How the radio volunteers this field, or null when it does not.</summary>
    /// <param name="field">The field.</param>
    /// <returns>The broadcast, or null.</returns>
    public static CivBroadcast? BroadcastFor(RigField field)
        => Broadcasts.FirstOrDefault(b => b.Field == field);
}
