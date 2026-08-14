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

    /// <summary>Bytes of header before the waveform, in the first part.</summary>
    /// <remarks>
    /// Fourteen: the division number, the division maximum, the center-or-fixed
    /// flag, two five-byte frequencies and the out-of-range flag (p. 19-12).
    /// Both modes are the same length, which is what makes a short frame
    /// detectable rather than merely odd. It was thirteen at first, because the
    /// manual's diagram opens with the command and sub-command bytes and those
    /// are already stripped by the time this sees the payload.
    /// </remarks>
    public const int FirstPartHeaderLength = 14;

    /// <summary>Read a scope frame's header, or null when it will not parse.</summary>
    /// <param name="payload">The data after the echoed sub-command.</param>
    /// <returns>The header, or null.</returns>
    public static ScopeHeader? ReadHeader(ReadOnlySpan<byte> payload)
    {
        if (payload.Length < FirstPartHeaderLength)
        {
            return null;
        }

        var sequence = payload[0];
        var total = payload[1];

        if (sequence != 1 || total is < 1 or > 32)
        {
            return null;
        }

        var fixedMode = payload[2] == 1;

        if (payload[2] > 1)
        {
            return null;
        }

        var first = Bcd.DecodeFrequencyHz(payload.Slice(3, Bcd.FrequencyByteCount));
        var second = Bcd.DecodeFrequencyHz(payload.Slice(8, Bcd.FrequencyByteCount));

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
            sequence, total, fixedMode, low, high, payload[FirstPartHeaderLength - 1] == 1);
    }

    /// <summary>Which part of a sweep this frame is, or null.</summary>
    /// <param name="payload">The data after the echoed sub-command.</param>
    /// <returns>The sequence number and the total, or null.</returns>
    public static (int Sequence, int Total)? ReadPart(ReadOnlySpan<byte> payload)
    {
        if (payload.Length < 2)
        {
            return null;
        }

        var sequence = payload[0];
        var total = payload[1];

        return sequence is < 1 || total is < 1 or > 32 || sequence > total
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
        => payload.Length <= 2 ? ReadOnlySpan<byte>.Empty : payload[2..];

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
