using Ft8Sharp.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Message;

/// <summary>
/// The non-standard-callsign message: the type this whole unit was built to reach, and the row of
/// the type cover that moved from refused to built.
/// </summary>
public class Ft8NonstandardMessageTests
{
    private readonly ITestOutputHelper _output;

    public Ft8NonstandardMessageTests(ITestOutputHelper output) => _output = output;

    private const int Seed = 20851;

    /// <summary>
    /// The full-call leg: a general call under this type carries one callsign in full and names
    /// nobody, so it reads with no cache at all.
    /// </summary>
    [Fact]
    public void AGeneralCallCarriesItsOwnCallAndNeedsNoCache()
    {
        Span<byte> message = stackalloc byte[Ft8NonstandardMessage.MessageBytes];
        Assert.Equal(
            Ft8PackResult.Ok,
            Ft8NonstandardMessage.TryPack("CQ", "PJ4/KA1ABC", string.Empty, null, message));

        Assert.Equal(Ft8MessageType.NonstandardCallsign, Ft8MessageTypes.TypeOf(message));

        var decoded = Ft8MessageDecoder.Decode(message, null);
        Assert.True(decoded.Decoded);
        Assert.Equal("CQ PJ4/KA1ABC", decoded.Text);
        Assert.Equal(Ft8FieldType.Token, decoded.Fields.CallToType);
        Assert.Equal(Ft8FieldType.Callsign, decoded.Fields.CallDeType);
        Assert.Equal(Ft8FieldType.None, decoded.Fields.ExtraType);
    }

    /// <summary>
    /// The hashed leg and the cold-cache leg, on the same bits.
    /// </summary>
    /// <remarks>
    /// <b>This is the property FT8 actually depends on and the only thing in this unit that
    /// exercises the cache as a cache.</b> One message teaches a receiver a callsign; a second
    /// message, from a different station, refers to it by twelve bits alone; the receiver reads it.
    /// A receiver that missed the first message reads nothing at all from the second.
    /// </remarks>
    [Fact]
    public void AHashedCompanionResolvesThroughAWarmCacheAndRefusesThroughAColdOne()
    {
        const string Hashed = "W9XYZ";
        const string InFull = "PJ4/KA1ABC";

        // The transmitting station's own cache, which learns both calls as it packs.
        var transmitter = new Ft8CallsignCache();
        Span<byte> message = stackalloc byte[Ft8NonstandardMessage.MessageBytes];
        Assert.Equal(
            Ft8PackResult.Ok,
            Ft8NonstandardMessage.TryPack(Hashed, InFull, "RR73", transmitter, message));

        Assert.True(transmitter.Contains(Hashed));
        Assert.True(transmitter.Contains(InFull));

        // A receiver that heard the addressed station spell its call out earlier.
        var warm = new Ft8CallsignCache();
        Assert.Equal(Ft8CacheStore.Stored, warm.Save(Hashed));

        var resolved = Ft8MessageDecoder.Decode(message, warm);
        Assert.True(resolved.Decoded);
        Assert.Equal(Ft8MessageType.NonstandardCallsign, resolved.Type);
        Assert.Equal($"<{Hashed}> {InFull} RR73", resolved.Text);

        // The very same bits, and a receiver that has never heard that station.
        var cold = new Ft8CallsignCache();
        var refused = Ft8MessageDecoder.Decode(message, cold);
        Assert.False(refused.Decoded);
        Assert.Equal(Ft8DecodeStatus.UnresolvedCallsign, refused.Status);
        Assert.Equal(string.Empty, refused.Text);
        Assert.Equal(default, refused.Fields);

        // And no cache at all is the same answer.
        Assert.False(Ft8MessageDecoder.Decode(message, null).Decoded);

        // The cold receiver did still learn the call that was spelled out in full, which is how it
        // would come to read the reply.
        Assert.True(cold.Contains(InFull));

        _output.WriteLine($"hashed leg: <{Hashed}> resolved warm, refused cold, on identical bits.");
    }

    /// <summary>A collision in the receiver's cache refuses this type too.</summary>
    [Fact]
    public void ACollidingHashedCompanionRefusesTheWholeMessage()
    {
        var (first, second) = FindTwelveBitPair();

        var transmitter = new Ft8CallsignCache();
        Span<byte> message = stackalloc byte[Ft8NonstandardMessage.MessageBytes];
        Assert.Equal(
            Ft8PackResult.Ok,
            Ft8NonstandardMessage.TryPack(first, "PJ4/KA1ABC", "73", transmitter, message));

        var narrow = new Ft8CallsignCache();
        narrow.Save(first);
        Assert.True(Ft8MessageDecoder.Decode(message, narrow).Decoded);

        var ambiguous = new Ft8CallsignCache();
        ambiguous.Save(first);
        ambiguous.Save(second);
        var refused = Ft8MessageDecoder.Decode(message, ambiguous);
        Assert.False(refused.Decoded);
        Assert.Equal(Ft8DecodeStatus.UnresolvedCallsign, refused.Status);
        Assert.Equal(string.Empty, refused.Text);

        _output.WriteLine($"{first} and {second} share a 12-bit hash; a cache holding both decodes neither.");
    }

    /// <summary>All three report tokens, and no report at all, survive the round trip.</summary>
    [Fact]
    public void EveryReportTokenRoundTrips()
    {
        var message = new byte[Ft8NonstandardMessage.MessageBytes];

        foreach (var extra in new[] { string.Empty, "RRR", "RR73", "73" })
        {
            var transmitter = new Ft8CallsignCache();
            Assert.Equal(
                Ft8PackResult.Ok,
                Ft8NonstandardMessage.TryPack("W9XYZ", "PJ4/KA1ABC", extra, transmitter, message));

            var receiver = new Ft8CallsignCache();
            receiver.Save("W9XYZ");
            var decoded = Ft8MessageDecoder.Decode(message, receiver);

            Assert.True(decoded.Decoded);
            Assert.Equal(extra, decoded.Fields.Extra);
            Assert.Equal(
                extra.Length == 0 ? Ft8FieldType.None : Ft8FieldType.Token,
                decoded.Fields.ExtraType);
        }
    }

    /// <summary>
    /// The packer never writes a bit past the seventy-seventh, asserted directly on every message
    /// rather than discovered through the container's refusal.
    /// </summary>
    [Fact]
    public void NoPackedMessageEverSetsABitPastTheSeventySeventh()
    {
        var random = new Random(Seed);
        var packed = 0;
        var message = new byte[Ft8NonstandardMessage.MessageBytes];
        var payload = new byte[Ft8Payload.PayloadBytes];

        for (var i = 0; i < 20_000; i++)
        {
            var cache = new Ft8CallsignCache();
            var callTo = i % 3 == 0 ? "CQ" : CallsignCorpus.Generate(random, i % CallsignCorpus.ShapeCount, out _);
            var callDe = CallsignCorpus.Generate(random, (i + 3) % CallsignCorpus.ShapeCount, out _);
            var extra = (i % 4) switch { 0 => string.Empty, 1 => "RRR", 2 => "RR73", _ => "73" };

            if (Ft8NonstandardMessage.TryPack(callTo, callDe, extra, cache, message) != Ft8PackResult.Ok)
            {
                continue;
            }

            packed++;

            // Bits 77, 78 and 79 are the low three of the last byte, and they are never written.
            Assert.Equal(0, message[9] & 0x07);

            // And the container agrees, which is the same fact checked from the other side: it
            // throws on a message with a bit set past the seventy-seventh, and it never does here.
            Ft8Payload.Create(message, payload);
        }

        Assert.True(packed > 0);
        _output.WriteLine($"{packed} packed messages, none with a bit set past the seventy-seventh.");
    }

    /// <summary>What the packer refuses, and why each refusal is the right answer.</summary>
    [Fact]
    public void ThePackerRefusesWhatItCannotCarry()
    {
        var cache = new Ft8CallsignCache();
        Span<byte> message = stackalloc byte[Ft8NonstandardMessage.MessageBytes];

        // Too short to be a callsign, either side.
        Assert.Equal(
            Ft8PackResult.FirstCallInvalid,
            Ft8NonstandardMessage.TryPack("W9", "PJ4/KA1ABC", string.Empty, cache, message));
        Assert.Equal(
            Ft8PackResult.SecondCallInvalid,
            Ft8NonstandardMessage.TryPack("W9XYZ", "PJ", string.Empty, cache, message));

        // A character outside the alphabet, either side.
        Assert.Equal(
            Ft8PackResult.FirstCallInvalid,
            Ft8NonstandardMessage.TryPack("W9-XYZ", "PJ4/KA1ABC", string.Empty, cache, message));
        Assert.Equal(
            Ft8PackResult.SecondCallInvalid,
            Ft8NonstandardMessage.TryPack("W9XYZ", "PJ4-KA1ABC", string.Empty, cache, message));

        // A call longer than the 58-bit field holds. Upstream packs its first eleven characters
        // instead, which puts a callsign on the air that nobody has.
        Assert.Equal(
            Ft8PackResult.SecondCallInvalid,
            Ft8NonstandardMessage.TryPack("W9XYZ", "ABCDEFGHIJKL", string.Empty, cache, message));

        // A bracketed call, which is upstream's own refusal: the bracket is not in the alphabet the
        // hash packs against. Brackets are an output convention and not an input one.
        Assert.Equal(
            Ft8PackResult.FirstCallInvalid,
            Ft8NonstandardMessage.TryPack("<W9XYZ>", "PJ4/KA1ABC", string.Empty, cache, message));

        // And without a cache there is nowhere for the hash to come from, so it refuses rather than
        // writing twelve bits nothing could read back.
        Assert.Equal(
            Ft8PackResult.FirstCallRequiresHashCache,
            Ft8NonstandardMessage.TryPack("W9XYZ", "PJ4/KA1ABC", string.Empty, null, message));

        // A general call needs no cache and is not refused for the want of one.
        Assert.Equal(
            Ft8PackResult.Ok,
            Ft8NonstandardMessage.TryPack("CQ", "PJ4/KA1ABC", string.Empty, null, message));
    }

    /// <summary>Nothing this unpacker is handed throws, and nothing it refuses writes text.</summary>
    [Fact]
    public void NothingHandedToTheUnpackerThrows()
    {
        var random = new Random(Seed + 1);
        var bytes = new byte[Ft8NonstandardMessage.MessageBytes];
        var warm = new Ft8CallsignCache();
        for (var i = 0; i < 2_000; i++)
        {
            warm.Save(CallsignCorpus.Generate(random, i % CallsignCorpus.ShapeCount, out _));
        }

        var decoded = 0;
        var refused = 0;

        for (var i = 0; i < 200_000; i++)
        {
            random.NextBytes(bytes);
            bytes[9] = (byte)((bytes[9] & 0xC7) | (Ft8MessageTypes.PrimaryNonstandard << 3));

            var status = Ft8NonstandardMessage.TryUnpack(bytes, i % 2 == 0 ? warm : null, out var fields);
            if (status == Ft8DecodeStatus.Decoded)
            {
                decoded++;
                Assert.NotEqual(string.Empty, fields.CallDe);
            }
            else
            {
                refused++;
                Assert.Equal(default, fields);
            }
        }

        Assert.Equal(200_000, decoded + refused);
        _output.WriteLine($"200000 random patterns of this type: {decoded} decoded, {refused} refused, none threw.");
    }

    /// <summary>The wrong-sized buffer is the one thing that throws, on both sides.</summary>
    [Fact]
    public void AWrongLengthBufferIsTheOneThingThatThrows()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            var tooShort = new byte[Ft8NonstandardMessage.MessageBytes - 1];
            Ft8NonstandardMessage.TryPack("CQ", "PJ4/KA1ABC", string.Empty, null, tooShort);
        });

        Assert.Throws<ArgumentException>(() =>
        {
            var tooLong = new byte[Ft8NonstandardMessage.MessageBytes + 1];
            Ft8NonstandardMessage.TryUnpack(tooLong, null, out _);
        });
    }

    /// <summary>The field widths add up to the message, which is the arithmetic the packing rests on.</summary>
    [Fact]
    public void TheFieldWidthsAreTheMessage()
    {
        Assert.Equal(
            Ft8Payload.MessageBits,
            Ft8NonstandardMessage.HashBits + Ft8NonstandardMessage.CallBits + 1 + 2 + 1 + Ft8MessageTypes.PrimaryBits);

        Assert.Equal(Ft8CallsignHash.Bits12, Ft8NonstandardMessage.HashBits);
        Assert.Equal(Ft8CallsignHash.MaxCallsignLength, Ft8NonstandardMessage.CallLength);

        // Eleven characters of thirty-eight is what fits in fifty-eight bits, which is why a
        // non-standard callsign is limited to eleven in the first place.
        var widest = Math.Pow(Ft8NonstandardMessage.CallBase, Ft8NonstandardMessage.CallLength);
        Assert.True(widest < Math.Pow(2, Ft8NonstandardMessage.CallBits));
        Assert.True(Math.Pow(Ft8NonstandardMessage.CallBase, Ft8NonstandardMessage.CallLength + 1)
            > Math.Pow(2, Ft8NonstandardMessage.CallBits));
    }

    /// <summary>The first pair of generated callsigns sharing a 12-bit hash.</summary>
    private static (string First, string Second) FindTwelveBitPair()
    {
        var seen = new Dictionary<uint, string>();
        foreach (var call in CallsignCorpus.Distinct(Seed, 200_000))
        {
            if (!Ft8CallsignHash.TryCompute(call, out _, out var h12, out _))
            {
                continue;
            }

            if (seen.TryGetValue(h12, out var earlier))
            {
                if (!string.Equals(earlier, call, StringComparison.Ordinal))
                {
                    return (earlier, call);
                }
            }
            else
            {
                seen[h12] = call;
            }
        }

        throw new InvalidOperationException("no 12-bit colliding pair in the generated corpus.");
    }
}
