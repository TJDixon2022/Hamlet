using Hamlet.RadioEngine.Rig;

namespace Hamlet.RadioEngine.Civ;

/// <summary>
/// One documented CI-V write: the bytes that set it, and where the manual says
/// so.
/// </summary>
/// <param name="Field">What it sets.</param>
/// <param name="Command">The command byte.</param>
/// <param name="Page">The Full Manual page this comes from (HM-DEC-049).</param>
/// <param name="Note">What the manual says the data means.</param>
public sealed record CivWrite(RigField Field, byte Command, string Page, string Note)
{
    /// <summary>How this write is named in the diagnostics screen and the log.</summary>
    public string Label => $"CI-V {Command:X2}";
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

    /// <summary>Every write, so the diagnostics screen can list them.</summary>
    public static IReadOnlyList<CivWrite> All { get; } = new[] { Mode };

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
