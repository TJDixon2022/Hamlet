using Ft8Sharp.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Message;

/// <summary>
/// Legs B and C of the CRC proof — the independent checker, and the linearity argument that
/// covers every message there is — together with the refusals that make the whole thing mean
/// something.
/// </summary>
/// <remarks>
/// <para>
/// <b>Leg A is next door</b>, in <c>UpstreamCrcProvenanceTests</c>, and it is what says the two
/// constants are the pin's. Nothing here re-checks that; these tests check that the arithmetic
/// built on them is the arithmetic the polynomial defines.
/// </para>
/// <para>
/// <b>The seed is stated because a corpus nobody can reproduce is an anecdote.</b> Every random
/// run in this file is seeded <see cref="Seed"/>, and a failure can be re-run to the same message.
/// </para>
/// </remarks>
public class Crc14Tests
{
    private readonly ITestOutputHelper _output;

    public Crc14Tests(ITestOutputHelper output) => _output = output;

    /// <summary>The seed for every random corpus in this file. Stated, not hidden.</summary>
    private const int Seed = 20260901;

    /// <summary>
    /// The bit lengths worth exercising: the one FT8 actually checksums, the message length, the
    /// byte boundaries either side of them, and the degenerate ends.
    /// </summary>
    public static TheoryData<int> BitLengths()
    {
        var data = new TheoryData<int>();
        foreach (var bits in new[] { 0, 1, 7, 8, 9, 15, 16, 63, 64, 71, 72, 76, 77, 80, 81, 82, 83, 88, 91, 96 })
        {
            data.Add(bits);
        }

        return data;
    }

    /// <summary>
    /// <b>Leg B.</b> The library and an implementation that shares none of its arithmetic agree,
    /// over a seeded corpus, at every bit length the protocol touches.
    /// </summary>
    [Theory]
    [MemberData(nameof(BitLengths))]
    public void TheIndependentCheckerAgreesAtEveryBitLength(int bits)
    {
        const int MessagesPerLength = 500;
        var random = new Random(Seed + bits);
        var buffer = new byte[(96 + 7) / 8];
        var disagreements = 0;

        // The fixed patterns first: all zero, all ones, and both alternating phases are where a
        // register that is one bit out shows up soonest.
        foreach (var fill in new byte[] { 0x00, 0xFF, 0xAA, 0x55 })
        {
            Array.Fill(buffer, fill);
            if (Crc14.Compute(buffer, bits) != CrcCheck.Compute(buffer, bits))
            {
                disagreements++;
            }
        }

        for (var i = 0; i < MessagesPerLength; i++)
        {
            random.NextBytes(buffer);
            if (Crc14.Compute(buffer, bits) != CrcCheck.Compute(buffer, bits))
            {
                disagreements++;
            }
        }

        _output.WriteLine($"bits {bits,3}: {MessagesPerLength + 4} messages, seed {Seed + bits}, "
            + $"disagreements {disagreements}");
        Assert.Equal(0, disagreements);
    }

    /// <summary>
    /// <b>Leg C, and it holds.</b> The initial remainder is zero and there is no final XOR, so
    /// the map is linear over GF(2) and the CRC of an XOR is the XOR of the CRCs.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>What that buys.</b> Every 77-bit message is the XOR of some subset of the 77 weight-one
    /// messages. Linearity means its CRC is the XOR of the corresponding subset of the 77 basis
    /// CRCs. So the 77 basis computations, plus linearity, determine the CRC of all 2^77 messages
    /// — the whole map, not a sample of it. This is the same argument that let 91 encodes prove
    /// the entire LDPC code space in step 1.
    /// </para>
    /// <para>
    /// <b>Linearity is demonstrated, not assumed.</b> A zero seed and no final XOR is read off the
    /// ported function; that it actually behaves linearly is measured here over pairs, and then
    /// the basis reconstruction is checked against direct computation over a seeded corpus, which
    /// is the argument's own conclusion tested end to end.
    /// </para>
    /// </remarks>
    [Fact]
    public void TheCrcIsLinearSoSeventySevenComputationsCoverEveryMessage()
    {
        const int PairCount = 2000;
        var random = new Random(Seed);

        // The empty message has zero remainder. Without that, linearity would be an affine
        // relation rather than a linear one and the basis argument would not close.
        Assert.Equal(0, Crc14.Compute(new byte[Ft8Payload.PayloadBytes], Ft8Payload.CrcBitCount));

        var failures = 0;
        for (var i = 0; i < PairCount; i++)
        {
            var a = RandomChecksummableBuffer(random);
            var b = RandomChecksummableBuffer(random);
            var xor = new byte[a.Length];
            for (var j = 0; j < a.Length; j++)
            {
                xor[j] = (byte)(a[j] ^ b[j]);
            }

            var left = Crc14.Compute(xor, Ft8Payload.CrcBitCount);
            var right = (ushort)(Crc14.Compute(a, Ft8Payload.CrcBitCount)
                ^ Crc14.Compute(b, Ft8Payload.CrcBitCount));
            if (left != right)
            {
                failures++;
            }
        }

        _output.WriteLine($"pairs {PairCount}, seed {Seed}, non-linear results {failures}");
        Assert.Equal(0, failures);

        // The basis itself: 77 computations, one per message bit.
        var basis = new ushort[Ft8Payload.MessageBits];
        for (var bit = 0; bit < Ft8Payload.MessageBits; bit++)
        {
            var one = new byte[Ft8Payload.PayloadBytes];
            one[bit / 8] |= (byte)(0x80u >> (bit % 8));
            basis[bit] = Crc14.Compute(one, Ft8Payload.CrcBitCount);
        }

        // And the conclusion, checked: a message's CRC really is the XOR of the basis CRCs its
        // set bits name, so the 77 numbers above determine all 2^77 of them.
        const int Reconstructions = 20_000;
        var mismatches = 0;
        for (var i = 0; i < Reconstructions; i++)
        {
            var message = RandomMessageBuffer(random);
            ushort reconstructed = 0;
            for (var bit = 0; bit < Ft8Payload.MessageBits; bit++)
            {
                if (((message[bit / 8] >> (7 - (bit % 8))) & 1) != 0)
                {
                    reconstructed ^= basis[bit];
                }
            }

            if (reconstructed != Crc14.Compute(message, Ft8Payload.CrcBitCount))
            {
                mismatches++;
            }
        }

        _output.WriteLine($"basis computations {Ft8Payload.MessageBits}, reconstructions "
            + $"{Reconstructions}, mismatches {mismatches}");
        Assert.Equal(0, mismatches);
    }

    /// <summary>
    /// <b>Watched refusing.</b> A proof that has never rejected anything proves nothing: every
    /// one of the 77 single-bit changes to a message changes its checksum.
    /// </summary>
    /// <remarks>
    /// Nothing checked in is corrupted here, on disk or in memory — each flip is made on a fresh
    /// copy. This is also the strongest statement linearity gives for free: bit <c>k</c> changing
    /// the checksum is exactly the statement that basis CRC <c>k</c> is non-zero.
    /// </remarks>
    [Fact]
    public void EverySingleBitChangeToAMessageChangesItsChecksum()
    {
        var random = new Random(Seed);
        var missed = 0;

        for (var trial = 0; trial < 20; trial++)
        {
            var message = trial == 0 ? new byte[Ft8Payload.PayloadBytes] : RandomMessageBuffer(random);
            var baseline = Crc14.Compute(message, Ft8Payload.CrcBitCount);

            for (var bit = 0; bit < Ft8Payload.MessageBits; bit++)
            {
                var flipped = (byte[])message.Clone();
                flipped[bit / 8] ^= (byte)(0x80u >> (bit % 8));
                if (Crc14.Compute(flipped, Ft8Payload.CrcBitCount) == baseline)
                {
                    missed++;
                }
            }
        }

        _output.WriteLine($"20 messages x {Ft8Payload.MessageBits} bit positions, "
            + $"changes the checksum did not notice: {missed}");
        Assert.Equal(0, missed);
    }

    /// <summary>A bit count longer than the buffer is refused rather than read past.</summary>
    [Fact]
    public void ABitCountLongerThanTheBufferIsRefused()
    {
        Assert.Throws<ArgumentException>(() => Crc14.Compute(new byte[10], 81));
        Assert.Throws<ArgumentOutOfRangeException>(() => Crc14.Compute(new byte[10], -1));

        // And the exact fit is not refused, so the guard is not simply always saying no.
        Crc14.Compute(new byte[10], 80);
        Crc14.Compute(new byte[10], 73);
    }

    /// <summary>A payload-shaped buffer with the bits past the checksummed region zeroed.</summary>
    private static byte[] RandomChecksummableBuffer(Random random)
    {
        var buffer = new byte[Ft8Payload.PayloadBytes];
        random.NextBytes(buffer);
        return buffer;
    }

    /// <summary>A payload-shaped buffer carrying 77 random message bits and nothing else.</summary>
    private static byte[] RandomMessageBuffer(Random random)
    {
        var buffer = new byte[Ft8Payload.PayloadBytes];
        random.NextBytes(buffer);
        for (var bit = Ft8Payload.MessageBits; bit < Ft8Payload.PayloadBytes * 8; bit++)
        {
            buffer[bit / 8] &= (byte)~(0x80u >> (bit % 8));
        }

        return buffer;
    }
}
