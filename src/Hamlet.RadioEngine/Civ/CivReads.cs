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
public sealed record CivRead(
    RigField Field,
    byte Command,
    byte[] SubCommand,
    string Page,
    string Note)
{
    /// <summary>How this read is named in the diagnostics screen and the log.</summary>
    public string Label
        => SubCommand.Length == 0
            ? $"CI-V {Command:X2}"
            : $"CI-V {Command:X2} {Convert.ToHexString(SubCommand)}";
}

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
    /// <summary>Read the operating mode and filter selection together.</summary>
    /// <remarks>
    /// One command answers two fields: the reply is one byte of mode and one of
    /// filter (p. 19-9). That is why the mode badge and the filter badge on the
    /// rig display refresh together.
    /// </remarks>
    public static CivRead ModeAndFilter { get; } = new(
        RigField.Mode, 0x04, Array.Empty<byte>(), "19-3, 19-9",
        "00=LSB, 01=USB, 02=AM, 03=CW, 04=RTTY, 05=FM, 07=CW-R, 08=RTTY-R; "
        + "then 01=FIL1, 02=FIL2, 03=FIL3");

    /// <summary>Read the selected filter's width.</summary>
    /// <remarks>
    /// An index rather than a figure in hertz, and the two step scales it
    /// covers are documented on p. 4-6 rather than in the command table.
    /// <see cref="CivFilterWidth"/> does the conversion.
    /// </remarks>
    public static CivRead FilterWidth { get; } = new(
        RigField.FilterBandwidth, 0x1A, new byte[] { 0x03 }, "19-3",
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

    /// <summary>Read whether split is on.</summary>
    public static CivRead Split { get; } = new(
        RigField.Split, 0x0F, Array.Empty<byte>(), "19-3", "00=off, 01=on");

    /// <summary>Every read, in the order the diagnostics screen shows them.</summary>
    public static IReadOnlyList<CivRead> All { get; } = new[]
    {
        ModeAndFilter, FilterWidth, SMeter, TransmitStatus, Overflow,
        RfPower, RfGain, Squelch, SquelchStatus, Agc, Preamp, Attenuator,
        NoiseBlanker, NoiseBlankerLevel, NoiseReduction, NoiseReductionLevel,
        AutoNotch, ManualNotch, BreakIn, KeyerSpeed, CwPitch,
        AccUsbOutputSelect, AccUsbAfLevel, AccUsbSquelch, Split,
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

    /// <summary>The read for a field, or null when there is none.</summary>
    /// <param name="field">The field.</param>
    /// <returns>The read, or null.</returns>
    public static CivRead? For(RigField field)
        => All.FirstOrDefault(r => r.Field == field);
}
