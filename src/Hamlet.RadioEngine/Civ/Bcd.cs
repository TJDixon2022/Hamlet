namespace Hamlet.RadioEngine.Civ;

/// <summary>
/// CI-V binary-coded-decimal frequency encoding: ten decimal digits packed
/// two per byte, least-significant pair first. 14,074,000 Hz encodes as
/// 00 40 07 14 00. The digit order is shown on p. 19-9; the little-endian byte
/// pairing is still carried from general knowledge and is the one §4 item this
/// file rests on that the manual does not state in so many words.
/// </summary>
public static class Bcd
{
    /// <summary>Number of bytes in a CI-V frequency field.</summary>
    public const int FrequencyByteCount = 5;

    /// <summary>Maximum value representable in ten BCD digits.</summary>
    public const long MaxFrequencyHz = 9_999_999_999;

    /// <summary>
    /// One packed byte as the two decimal digits printed on it (HM-DEC-094).
    /// </summary>
    /// <param name="packed">The byte, high nibble the tens digit.</param>
    /// <returns>The value 0 to 99, or -1 when either nibble is not a digit.</returns>
    /// <remarks>
    /// **THE SCOPE'S DIVISION COUNT IS BCD AND WAS READ AS HEXADECIMAL**, so
    /// `0x11` came out as seventeen where the radio meant eleven. Every part of
    /// every sweep failed its own sanity check and was discarded, for as long as
    /// the feature has existed (HM-DEC-094).
    /// </remarks>
    public static int DecodeByte(byte packed)
    {
        var high = packed >> 4;
        var low = packed & 0x0F;

        return high > 9 || low > 9 ? -1 : (high * 10) + low;
    }

    /// <summary>Encode a frequency in hertz into five BCD bytes.</summary>
    /// <param name="frequencyHz">The frequency.</param>
    /// <returns>Five bytes, least significant pair first.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Outside 0..<see cref="MaxFrequencyHz"/>.</exception>
    public static byte[] EncodeFrequencyHz(long frequencyHz)
    {
        if (frequencyHz is < 0 or > MaxFrequencyHz)
        {
            throw new ArgumentOutOfRangeException(nameof(frequencyHz), frequencyHz,
                "CI-V frequency must fit ten decimal digits.");
        }

        var bytes = new byte[FrequencyByteCount];
        var remaining = frequencyHz;
        for (var i = 0; i < FrequencyByteCount; i++)
        {
            var low = (int)(remaining % 10);
            remaining /= 10;
            var high = (int)(remaining % 10);
            remaining /= 10;
            bytes[i] = (byte)((high << 4) | low);
        }

        return bytes;
    }

    /// <summary>Decode five BCD bytes into a frequency in hertz.</summary>
    /// <exception cref="ArgumentException">Wrong length or a nibble above 9 —
    /// fail loud rather than guess (§0.0).</exception>
    public static long DecodeFrequencyHz(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != FrequencyByteCount)
        {
            throw new ArgumentException(
                $"CI-V frequency field is {FrequencyByteCount} bytes; got {bytes.Length}.",
                nameof(bytes));
        }

        long value = 0;
        for (var i = FrequencyByteCount - 1; i >= 0; i--)
        {
            var high = (bytes[i] >> 4) & 0x0F;
            var low = bytes[i] & 0x0F;
            if (high > 9 || low > 9)
            {
                throw new ArgumentException(
                    $"Byte {i} (0x{bytes[i]:X2}) is not valid BCD.", nameof(bytes));
            }

            value = value * 100 + high * 10 + low;
        }

        return value;
    }
}
