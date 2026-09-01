using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Ldpc;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Message;

/// <summary>
/// The five things the payload container has to be true about, and the first time in this phase
/// that a message-shaped thing has gone through the proven encoder.
/// </summary>
/// <remarks>
/// <para>
/// <b>The seed is stated because a corpus nobody can reproduce is an anecdote.</b> Every random
/// corpus here is seeded <see cref="Seed"/>.
/// </para>
/// <para>
/// <b>The corpus is built once and reused</b>, so that "every payload round-tripped", "every
/// payload had zero spare bits" and "every payload cleared all 83 LDPC checks" are three
/// statements about the same messages rather than three different samples.
/// </para>
/// </remarks>
public class Ft8PayloadTests
{
    private readonly ITestOutputHelper _output;

    public Ft8PayloadTests(ITestOutputHelper output) => _output = output;

    private const int Seed = 20260901;

    private const int RandomMessages = 10_000;

    /// <summary>
    /// The corpus: the 77 weight-one messages, the four fixed patterns, and 10 000 seeded random
    /// ones.
    /// </summary>
    private static List<byte[]> Corpus()
    {
        var corpus = new List<byte[]>();

        for (var bit = 0; bit < Ft8Payload.MessageBits; bit++)
        {
            var message = new byte[Ft8Payload.MessageBytes];
            message[bit / 8] |= (byte)(0x80u >> (bit % 8));
            corpus.Add(message);
        }

        corpus.Add(Pattern(_ => 0));
        corpus.Add(Pattern(_ => 1));
        corpus.Add(Pattern(i => i % 2));
        corpus.Add(Pattern(i => (i + 1) % 2));

        var random = new Random(Seed);
        for (var i = 0; i < RandomMessages; i++)
        {
            var message = new byte[Ft8Payload.MessageBytes];
            random.NextBytes(message);

            // The three bits past the 77th are not part of a message.
            for (var bit = Ft8Payload.MessageBits; bit < Ft8Payload.MessageBytes * 8; bit++)
            {
                message[bit / 8] &= (byte)~(0x80u >> (bit % 8));
            }

            corpus.Add(message);
        }

        return corpus;
    }

    private static byte[] Pattern(Func<int, int> bitAt)
    {
        var message = new byte[Ft8Payload.MessageBytes];
        for (var bit = 0; bit < Ft8Payload.MessageBits; bit++)
        {
            if (bitAt(bit) != 0)
            {
                message[bit / 8] |= (byte)(0x80u >> (bit % 8));
            }
        }

        return message;
    }

    /// <summary>
    /// <b>One.</b> 77 bits in, payload out, 77 bits back, identical, over the whole corpus.
    /// </summary>
    [Fact]
    public void EveryMessageRoundTripsThroughThePayloadUnchanged()
    {
        var corpus = Corpus();
        var payload = new byte[Ft8Payload.PayloadBytes];
        var readBack = new byte[Ft8Payload.MessageBytes];
        var refused = 0;
        var altered = 0;

        foreach (var message in corpus)
        {
            Ft8Payload.Create(message, payload);
            if (!Ft8Payload.TryRead(payload, readBack))
            {
                refused++;
                continue;
            }

            if (!readBack.AsSpan().SequenceEqual(message))
            {
                altered++;
            }
        }

        _output.WriteLine($"corpus {corpus.Count} messages, seed {Seed}: "
            + $"refused {refused}, came back altered {altered}");
        Assert.Equal(0, refused);
        Assert.Equal(0, altered);
    }

    /// <summary>
    /// <b>Two.</b> Every payload the container produces has zero in the five spare bits, asserted
    /// directly rather than inferred from the encoder not complaining.
    /// </summary>
    [Fact]
    public void EveryPayloadHasZeroInItsSpareBits()
    {
        var corpus = Corpus();
        var payload = new byte[Ft8Payload.PayloadBytes];
        var spareMask = (byte)((1 << ((Ft8Payload.PayloadBytes * 8) - Ft8Payload.PayloadBits)) - 1);
        var withSpareBitsSet = 0;

        foreach (var message in corpus)
        {
            Ft8Payload.Create(message, payload);
            if ((payload[Ft8Payload.PayloadBytes - 1] & spareMask) != 0)
            {
                withSpareBitsSet++;
            }
        }

        _output.WriteLine($"corpus {corpus.Count} payloads: with spare bits set {withSpareBitsSet}");
        Assert.Equal(0, withSpareBitsSet);
    }

    /// <summary>
    /// <b>Three.</b> Every corpus payload encodes to a codeword that clears all 83 parity checks,
    /// through the encoder step 1 proved and the independent checker that proved it.
    /// </summary>
    /// <remarks>
    /// This is the first time in this phase that a message-shaped payload has gone through the
    /// proven encoder. Step 1 proved the encoder over payloads that were bit patterns; these are
    /// payloads with a real checksum in them, which is a different question only in the sense that
    /// it is the one the protocol will actually ask.
    /// </remarks>
    [Fact]
    public void EveryCorpusPayloadClearsAllEightyThreeParityChecks()
    {
        var corpus = Corpus();
        var payload = new byte[Ft8Payload.PayloadBytes];
        var codeword = new byte[LdpcEncoder.CodewordBytes];
        var checksFailed = 0;
        var payloadsWithAFailure = 0;

        foreach (var message in corpus)
        {
            Ft8Payload.Create(message, payload);
            LdpcEncoder.Encode(payload, codeword);

            var bits = LdpcCheck.UnpackMsbFirst(codeword, Ft8Tables.LdpcN);
            var failed = LdpcCheck.FailingCount(
                LdpcCheck.SyndromeFromNm(bits, Ft8Tables.LdpcNm, Ft8Tables.LdpcNumRows));
            if (failed != 0)
            {
                checksFailed += failed;
                payloadsWithAFailure++;
            }
        }

        _output.WriteLine($"corpus {corpus.Count} payloads x {Ft8Tables.LdpcM} checks: "
            + $"payloads with any failure {payloadsWithAFailure}, failed checks in all {checksFailed}");
        Assert.Equal(0, payloadsWithAFailure);
        Assert.Equal(0, checksFailed);
    }

    /// <summary>
    /// <b>Four.</b> A payload with any one of its 91 bits flipped is refused, every time.
    /// </summary>
    /// <remarks>
    /// Nothing checked in is corrupted — each flip is made on a copy. The corruption sweep runs
    /// over a sample of the corpus rather than all of it because 91 flips times ten thousand
    /// messages is a million CRC computations for an answer the first few hundred already give;
    /// the sample is seeded and its size is stated.
    /// </remarks>
    [Fact]
    public void EveryOneBitCorruptionOfAPayloadIsRefused()
    {
        const int MessagesSwept = 200;
        var corpus = Corpus();
        var random = new Random(Seed);
        var payload = new byte[Ft8Payload.PayloadBytes];
        var readBack = new byte[Ft8Payload.MessageBytes];
        var accepted = 0;
        var flips = 0;

        // The weight-one messages first, then a seeded sample of the rest.
        var swept = Enumerable.Range(0, Ft8Payload.MessageBits)
            .Concat(Enumerable.Range(0, MessagesSwept).Select(_ => random.Next(corpus.Count)))
            .ToList();

        foreach (var index in swept)
        {
            Ft8Payload.Create(corpus[index], payload);

            for (var bit = 0; bit < Ft8Payload.PayloadBits; bit++)
            {
                var corrupted = (byte[])payload.Clone();
                corrupted[bit / 8] ^= (byte)(0x80u >> (bit % 8));
                flips++;

                if (Ft8Payload.TryRead(corrupted, readBack))
                {
                    accepted++;
                }
            }
        }

        _output.WriteLine($"{swept.Count} payloads x {Ft8Payload.PayloadBits} bit positions = "
            + $"{flips} corruptions, seed {Seed}: accepted as valid {accepted}");
        Assert.Equal(0, accepted);
    }

    /// <summary>
    /// <b>Five.</b> No 12-byte buffer makes the container throw — including buffers with the spare
    /// bits set, which are not legal payloads at all.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the container half of step 2's criterion 3. <b>The criterion itself stays open</b>:
    /// it says any random 77-bit pattern either <em>decodes</em> or fails cleanly, and nothing in
    /// this tree decodes yet — there is no unpacker. What is settled here is that the layer
    /// underneath an unpacker will not be the thing that throws.
    /// </para>
    /// <para>
    /// A wrong-length buffer is deliberately outside this: that is a caller mistake rather than a
    /// bad signal, and it is refused loudly on purpose.
    /// </para>
    /// </remarks>
    [Fact]
    public void NoTwelveByteBufferMakesTheContainerThrow()
    {
        const int Buffers = 100_000;
        var random = new Random(Seed);
        var buffer = new byte[Ft8Payload.PayloadBytes];
        var readBack = new byte[Ft8Payload.MessageBytes];
        var validated = 0;
        var refused = 0;
        var withSpareBitsSet = 0;
        var spareMask = (byte)((1 << ((Ft8Payload.PayloadBytes * 8) - Ft8Payload.PayloadBits)) - 1);

        for (var i = 0; i < Buffers; i++)
        {
            random.NextBytes(buffer);
            if ((buffer[Ft8Payload.PayloadBytes - 1] & spareMask) != 0)
            {
                withSpareBitsSet++;
            }

            // No try/catch: an exception escaping here fails the test, which is the assertion.
            if (Ft8Payload.TryRead(buffer, readBack))
            {
                validated++;
            }
            else
            {
                refused++;
            }

            // Reading the stored checksum out of an arbitrary buffer is legal and judges nothing.
            Ft8Payload.ExtractCrc(buffer);
        }

        _output.WriteLine($"{Buffers} random 12-byte buffers, seed {Seed}: validated {validated}, "
            + $"refused {refused}, of which had spare bits set {withSpareBitsSet}");
        _output.WriteLine("Criterion 3 stays open: nothing in this tree decodes yet.");

        Assert.Equal(Buffers, validated + refused);

        // A corpus that never validated anything would prove nothing about the accept path, and
        // one that validated a lot would mean the check was not checking. Roughly one buffer in
        // 2^19 should pass both the 14-bit checksum and the 5 spare bits, so zero is the expected
        // count and anything much above it is a finding.
        Assert.True(validated < 10, $"{validated} arbitrary buffers validated, which is too many.");
    }

    /// <summary>The container refuses a message that has bits set past its 77th.</summary>
    [Fact]
    public void AMessageWithBitsPastItsSeventySeventhIsRefused()
    {
        var payload = new byte[Ft8Payload.PayloadBytes];

        for (var bit = Ft8Payload.MessageBits; bit < Ft8Payload.MessageBytes * 8; bit++)
        {
            var message = new byte[Ft8Payload.MessageBytes];
            message[bit / 8] |= (byte)(0x80u >> (bit % 8));
            Assert.Throws<ArgumentException>(() => Ft8Payload.Create(message, payload));
        }

        // And a legal message is not refused, so the guard is not simply always saying no.
        Ft8Payload.Create(new byte[Ft8Payload.MessageBytes], payload);
    }

    /// <summary>Wrong-length buffers are refused loudly on both paths.</summary>
    [Fact]
    public void WrongLengthBuffersAreRefused()
    {
        Assert.Throws<ArgumentException>(() =>
            Ft8Payload.Create(new byte[Ft8Payload.MessageBytes - 1], new byte[Ft8Payload.PayloadBytes]));
        Assert.Throws<ArgumentException>(() =>
            Ft8Payload.Create(new byte[Ft8Payload.MessageBytes], new byte[Ft8Payload.PayloadBytes + 1]));
        Assert.Throws<ArgumentException>(() =>
        {
            Span<byte> message = new byte[Ft8Payload.MessageBytes];
            Ft8Payload.TryRead(new byte[Ft8Payload.PayloadBytes - 1], message);
        });
        Assert.Throws<ArgumentException>(() => Ft8Payload.ExtractCrc(new byte[Ft8Payload.PayloadBytes + 1]));
    }
}
