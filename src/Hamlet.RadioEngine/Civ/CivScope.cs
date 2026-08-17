namespace Hamlet.RadioEngine.Civ;

/// <summary>What one scope frame's header says (p. 19-12).</summary>
/// <param name="Sequence">Which part this is, 1 up to <paramref name="Total"/>.</param>
/// <param name="Total">How many parts the sweep is divided into.</param>
/// <param name="IsFixedMode">
/// True when the radio is in fixed-edge mode, false for center mode.
/// </param>
/// <param name="LowHz">The span's lower edge in hertz.</param>
/// <param name="HighHz">The span's upper edge in hertz.</param>
/// <param name="OutOfRange">
/// True when the radio says the data is out of range, in which case it omits the
/// waveform entirely.
/// </param>
public sealed record ScopeHeader(
    int Sequence, int Total, bool IsFixedMode, long LowHz, long HighHz, bool OutOfRange);

/// <summary>
/// Reading the spectrum scope stream, CI-V <c>27 00</c> (HM-DEC-062).
/// </summary>
/// <remarks>
/// <para>VERIFIED COLUMN-AWARE against `IC-7300_Full_English v6`, p. 19-12, which
/// is the lesson HM-DEC-050 paid for. The frame carries a sequence number, a
/// total, a center-or-fixed flag, the span, an out-of-range flag and then the
/// waveform. "When sent through the USB port, the data is divided by 11 and sent
/// in sequential order. The 1st data sends only the wave information without the
/// waveform data. The 2nd or later data sends the minimum wave information with
/// waveform data." Data range 0 to 160, data length 475.</para>
/// <para>A FRAME THAT DOES NOT MATCH ITS DOCUMENTED SHAPE PRODUCES NOTHING, never
/// a nearest guess. Falling back to the band's own edges when the header will not
/// parse would draw a waterfall whose frequencies are Hamlet's invention rather
/// than the radio's measurement, which is the prime directive broken on the one
/// surface built to show what is actually there (§0.0).</para>
/// <para>Pure: bytes in, a header or null out. No radio, no clock (§5).</para>
/// </remarks>
public static class CivScope
{
    /// <summary>The highest amplitude the radio reports (p. 19-12).</summary>
    /// <remarks>
    /// 160 rather than 255. The waterfall's palette runs to 255, so the values
    /// are scaled on the way in rather than leaving the top third of every
    /// palette unused.
    /// </remarks>
    public const int MaximumAmplitude = 160;

    /// <summary>How many amplitude points make one sweep (p. 19-12).</summary>
    public const int WaveformLength = 475;

    /// <summary>How many parts a sweep arrives in over USB (p. 19-12).</summary>
    public const int PartsOverUsb = 11;

    /// <summary>
    /// Bytes every part carries before anything else (HM-DEC-094).
    /// </summary>
    /// <remarks>
    /// <para>**THREE, AND THE FIRST OF THEM IS WHY NOTHING EVER DREW.** Field 1
    /// is a fixed `00`, field 2 is the order of this part and field 3 is the
    /// division maximum. The parser read field 1 as the order, so the order was
    /// always zero, which fails "a part number is at least one" on every part of
    /// every sweep.</para>
    /// <para>Two independent samples off the real wire, both 53 bytes:
    /// `00 08 11 2A 2F 2B …` and `00 04 11 18 1B 17 …`. Three header bytes and
    /// fifty of waveform, reading as part 8 of 11 and part 4 of 11.</para>
    /// </remarks>
    public const int PartHeaderLength = 3;

    /// <summary>Bytes of header before the waveform, in the first part.</summary>
    /// <remarks>
    /// The three every part carries, then the center-or-fixed flag, two
    /// five-byte frequencies and the out-of-range flag.
    /// </remarks>
    public const int FirstPartHeaderLength = PartHeaderLength + 12;

    /// <summary>Read a scope frame's header, or null when it will not parse.</summary>
    /// <param name="payload">The data after the echoed sub-command.</param>
    /// <returns>The header, or null.</returns>
    public static ScopeHeader? ReadHeader(ReadOnlySpan<byte> payload)
    {
        if (payload.Length < FirstPartHeaderLength)
        {
            return null;
        }

        if (ReadPart(payload) is not { } part || part.Sequence != 1)
        {
            return null;
        }

        var fixedMode = payload[PartHeaderLength] == 1;

        if (payload[PartHeaderLength] > 1)
        {
            return null;
        }

        var first = Bcd.DecodeFrequencyHz(
            payload.Slice(PartHeaderLength + 1, Bcd.FrequencyByteCount));

        var second = Bcd.DecodeFrequencyHz(
            payload.Slice(PartHeaderLength + 6, Bcd.FrequencyByteCount));

        // Center mode sends a center frequency and a span; fixed mode sends the
        // two edges. Both are five bytes of BCD, so the mode flag is the only
        // thing that says which reading is which (p. 19-12).
        var low = fixedMode ? first : first - (second / 2);
        var high = fixedMode ? second : first + (second / 2);

        if (high <= low || low <= 0)
        {
            return null;
        }

        return new ScopeHeader(
            part.Sequence, part.Total, fixedMode, low, high,
            payload[FirstPartHeaderLength - 1] == 1);
    }

    /// <summary>Which part of a sweep this frame is, or null.</summary>
    /// <param name="payload">The data after the echoed sub-command.</param>
    /// <returns>The sequence number and the total, or null.</returns>
    /// <remarks>
    /// <para>**BOTH NUMBERS ARE BCD, AND READING THEM AS HEXADECIMAL IS WHY THIS
    /// FEATURE NEVER WORKED** (HM-DEC-094). The division maximum arrives as
    /// `0x11`, which is eleven printed on the byte and seventeen if the byte is
    /// taken at face value. The order is BCD for the same reason: parts ten and
    /// eleven would otherwise read as sixteen and seventeen.</para>
    /// <para>The arithmetic settles it without the manual. The waveform is 475
    /// points and the first part carries none of it, so eleven parts means ten
    /// carrying about fifty each, which is what the wire showed. Seventeen parts
    /// would need eight hundred bytes to describe four hundred and seventy-five
    /// points.</para>
    /// <para>This is the `14 08` mistake in a different register: a value read in
    /// the wrong base (§4).</para>
    /// </remarks>
    public static (int Sequence, int Total)? ReadPart(ReadOnlySpan<byte> payload)
    {
        if (payload.Length < PartHeaderLength || payload[0] != 0)
        {
            return null;
        }

        var sequence = Bcd.DecodeByte(payload[1]);
        var total = Bcd.DecodeByte(payload[2]);

        return sequence < 1 || total is < 1 or > 32 || sequence > total
            ? null
            : (sequence, total);
    }

    /// <summary>The waveform bytes carried by a continuation frame.</summary>
    /// <param name="payload">The data after the echoed sub-command.</param>
    /// <returns>The amplitudes, which may be empty.</returns>
    /// <remarks>
    /// Only the sequence and the total precede the waveform in parts two and
    /// later; the header fields are sent once, in the first part.
    /// </remarks>
    public static ReadOnlySpan<byte> Waveform(ReadOnlySpan<byte> payload)
        => payload.Length <= PartHeaderLength
            ? ReadOnlySpan<byte>.Empty
            : payload[PartHeaderLength..];

    /// <summary>
    /// Scale a reported amplitude onto the waterfall's byte range.
    /// </summary>
    /// <param name="reported">What the radio sent, 0 to 160.</param>
    /// <returns>0 to 255.</returns>
    /// <remarks>
    /// A value above the documented maximum is clamped rather than wrapped,
    /// because a byte that wrapped would draw a strong signal as a hole.
    /// </remarks>
    public static byte Scale(byte reported)
        => (byte)Math.Clamp(reported * 255 / MaximumAmplitude, 0, 255);
}
