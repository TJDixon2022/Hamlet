using Ft8Sharp.Ldpc;

namespace Ft8Sharp.Tests.Ldpc;

/// <summary>Payload buffers for the parity proof, built bit by bit rather than pasted.</summary>
internal static class Payloads
{
    /// <summary>
    /// The weight-one payload with bit <paramref name="bit"/> set, counted most significant
    /// bit first from the start of the buffer.
    /// </summary>
    public static byte[] Basis(int bit)
    {
        var payload = new byte[LdpcEncoder.PayloadBytes];
        payload[bit / 8] |= (byte)(0x80u >> (bit % 8));
        return payload;
    }

    /// <summary>A payload holding the given bit pattern, spare bits left zero.</summary>
    public static byte[] FromBits(ReadOnlySpan<byte> bits)
    {
        var payload = new byte[LdpcEncoder.PayloadBytes];
        for (var i = 0; i < LdpcEncoder.PayloadBits; i++)
        {
            if (bits[i] != 0)
            {
                payload[i / 8] |= (byte)(0x80u >> (i % 8));
            }
        }

        return payload;
    }

    /// <summary>A payload whose bits come from <paramref name="random"/>, spare bits zero.</summary>
    public static byte[] Random(Random random)
    {
        var bits = new byte[LdpcEncoder.PayloadBits];
        for (var i = 0; i < bits.Length; i++)
        {
            bits[i] = (byte)random.Next(2);
        }

        return FromBits(bits);
    }

    /// <summary>A payload of all ones across the 91 carried bits, spare bits zero.</summary>
    public static byte[] AllOnes()
    {
        var bits = new byte[LdpcEncoder.PayloadBits];
        Array.Fill(bits, (byte)1);
        return FromBits(bits);
    }

    /// <summary>A payload alternating 1 and 0 from the given phase.</summary>
    public static byte[] Alternating(int phase)
    {
        var bits = new byte[LdpcEncoder.PayloadBits];
        for (var i = 0; i < bits.Length; i++)
        {
            bits[i] = (byte)((i + phase) % 2);
        }

        return FromBits(bits);
    }
}
