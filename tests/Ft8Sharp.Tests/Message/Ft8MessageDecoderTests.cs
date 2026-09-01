using Ft8Sharp;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Ldpc;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Message;

/// <summary>
/// The dispatcher: every type code given a defined answer, the standard message round-tripped over
/// a seeded corpus, a million random patterns refused without a single exception, and one message
/// carried end to end through the encoder step 1 proved.
/// </summary>
/// <remarks>
/// <para>
/// <b>The type cover and the fuzz counts together are what step 2's criterion 3 asks for.</b>
/// Criterion 3 is not "a packer that round-trips" — it is a dispatcher that has been handed a
/// million patterns it never asked for and refused every one it cannot read, without throwing
/// once.
/// </para>
/// <para>
/// <b>The corpora prove self-consistency, not agreement with upstream.</b> A packer and an unpacker
/// that agree with each other are inverses and nothing more; a field packed in the wrong order
/// round-trips perfectly and is wholly wrong on the air. The pin holds no message-level known value
/// to check against — its own test source drives its encoder into its decoder, which is the same
/// self-consistency measured here. What corroborates the structure is the machine-checked
/// provenance of the field boundaries and alphabet lengths; what settles the arithmetic is step 3.
/// </para>
/// </remarks>
public class Ft8MessageDecoderTests
{
    private const int Seed = 20260901;
    private const int CorpusSize = 200_000;
    private const int FuzzSize = 1_000_000;

    private readonly ITestOutputHelper _output;

    public Ft8MessageDecoderTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// Every combination of the two type selectors, with the behaviour each one has.
    /// </summary>
    /// <remarks>
    /// <b>Enumerated, not sampled — the selectors are three bits and three bits.</b> Each
    /// combination is exercised with a message body that is otherwise a valid standard message, so
    /// that a refusal is a refusal about the type and not about the fields under it.
    /// </remarks>
    [Fact]
    public void EveryTypeCombinationHasADefinedBehaviour()
    {
        var built = 0;
        var refused = 0;
        var combinations = 0;

        _output.WriteLine($"{"i3",-4}{"n3",-6}{"type",-24}{"behaviour",-28}status");

        for (var primary = 0; primary < Ft8MessageTypes.PrimaryCount; primary++)
        {
            var secondaries = primary == Ft8MessageTypes.PrimaryFreeTextFamily
                ? Enumerable.Range(0, Ft8MessageTypes.SecondaryCount).ToArray()
                : new[] { -1 };

            foreach (var secondary in secondaries)
            {
                combinations++;

                var message = StandardMessageBytes("CQ", "K1ABC", "FN42");
                SetSelectors(message, primary, secondary < 0 ? 0 : secondary);

                var type = Ft8MessageTypes.TypeOf(message);
                var result = Ft8MessageDecoder.Decode(message);

                Assert.Equal(type, result.Type);

                if (result.Decoded)
                {
                    built++;
                    Assert.True(
                        Ft8MessageTypes.IsSupported(type),
                        $"i3={primary} n3={secondary} decoded as {type}, which this library has not "
                        + "built. A decode returned for an unbuilt type is the failure this test "
                        + "exists to catch.");
                    Assert.NotNull(result.Text);
                }
                else
                {
                    refused++;
                    Assert.Equal(string.Empty, result.Text);
                    Assert.True(
                        result.Status is Ft8DecodeStatus.UnsupportedType
                            or Ft8DecodeStatus.UnresolvedCallsign
                            or Ft8DecodeStatus.MalformedField,
                        $"i3={primary} n3={secondary} refused with no reason given.");
                }

                _output.WriteLine(
                    $"{primary,-4}{(secondary < 0 ? "-" : secondary.ToString()),-6}{type,-24}"
                    + $"{(result.Decoded ? "built and round-tripping" : "refused as " + result.Status),-28}"
                    + $"{result.Status}");
            }
        }

        _output.WriteLine($"combinations enumerated : {combinations}");
        _output.WriteLine($"    built               : {built}");
        _output.WriteLine($"    refused             : {refused}");

        Assert.Equal(Ft8MessageTypes.CombinationCount, combinations);
        Assert.Equal(combinations, built + refused);
        Assert.True(built > 0, "nothing was built, so the cover measured nothing.");
    }

    /// <summary>
    /// A seeded corpus of standard messages: two callsigns, both suffix flags, and the whole of the
    /// grid-or-report field's reachable text.
    /// </summary>
    [Fact]
    public void ASeededCorpusOfStandardMessagesRoundTrips()
    {
        var random = new Random(Seed);
        var roundTripped = 0;
        var mismatches = new List<string>();
        var typeOnes = 0;
        var typeTwos = 0;
        var withSuffix = 0;

        Span<byte> message = stackalloc byte[Ft8StandardMessage.MessageBytes];

        for (var i = 0; i < CorpusSize; i++)
        {
            // One message carries one suffix meaning, so both calls in it take the same one. A
            // corpus that mixed them would be generating messages the protocol cannot express and
            // then reporting the refusal as a failure.
            var suffixKind = random.Next(2) == 0 ? "/R" : "/P";
            var callTo = GenerateFirstField(random, i, suffixKind);
            var callDe = GenerateCallsign(random, suffixKind, out var deSuffix);
            var extra = GenerateExtra(random, i);

            var packed = Ft8StandardMessage.TryPack(callTo, callDe, extra, message);
            if (packed != Ft8PackResult.Ok)
            {
                mismatches.Add($"pack refused [{callTo}] [{callDe}] [{extra}] : {packed}");
                continue;
            }

            // The container refuses a message with anything past the seventy-seventh bit, so this
            // is asserted directly rather than discovered through an exception later.
            Assert.Equal(0, message[9] & 0x07);

            var primary = Ft8MessageTypes.Primary(message);
            if (primary == Ft8MessageTypes.PrimaryStandard)
            {
                typeOnes++;
            }
            else
            {
                typeTwos++;
            }

            if (deSuffix)
            {
                withSuffix++;
            }

            var result = Ft8MessageDecoder.Decode(message);
            if (!result.Decoded)
            {
                mismatches.Add($"[{callTo}] [{callDe}] [{extra}] refused as {result.Status}");
                continue;
            }

            var expected = string.IsNullOrEmpty(extra)
                ? $"{callTo} {callDe}"
                : $"{callTo} {callDe} {extra}";

            if (result.Text != expected)
            {
                mismatches.Add($"[{expected}] came back as [{result.Text}]");
                continue;
            }

            roundTripped++;
        }

        _output.WriteLine($"corpus size          : {CorpusSize}   seed: {Seed}");
        _output.WriteLine($"round-tripped        : {roundTripped}");
        _output.WriteLine($"did not              : {mismatches.Count}");
        _output.WriteLine($"    packed under the first type code  : {typeOnes}");
        _output.WriteLine($"    packed under the second type code : {typeTwos}");
        _output.WriteLine($"    carrying a suffix on the second call : {withSuffix}");
        foreach (var mismatch in mismatches.Take(25))
        {
            _output.WriteLine($"    {mismatch}");
        }

        Assert.Empty(mismatches);
        Assert.Equal(CorpusSize, roundTripped);
        Assert.True(typeTwos > 0, "no message was packed under the second type code, so it is untested.");
    }

    /// <summary>
    /// A million random 77-bit patterns through the dispatcher. This is criterion 3 itself.
    /// </summary>
    /// <remarks>
    /// <b>Three counts, and all three must be zero.</b> Exceptions; decodes returned for a type
    /// this library has not built; decodes returned for a callsign that could not be resolved. Any
    /// one of them above zero is a decoder that would put something on the operator's screen that
    /// was never on the air.
    /// </remarks>
    [Fact]
    public void AMillionRandomPatternsEitherDecodeOrAreRefusedAndNoneThrows()
    {
        var random = new Random(Seed);
        var message = new byte[Ft8MessageDecoder.MessageBytes];

        var exceptions = 0;
        var decodesForUnbuiltTypes = 0;
        var decodesWithUnresolvedCallsigns = 0;

        var decoded = 0;
        var byStatus = new Dictionary<Ft8DecodeStatus, int>();
        var byType = new Dictionary<Ft8MessageType, int>();

        for (var i = 0; i < FuzzSize; i++)
        {
            random.NextBytes(message);

            // The three bits past the seventy-seventh are not part of the message. Left as the
            // random bytes made them: the dispatcher must not depend on them, and this is where
            // that is checked rather than assumed.
            Ft8DecodeResult result;
            try
            {
                result = Ft8MessageDecoder.Decode(message);
            }
            catch (Exception ex)
            {
                exceptions++;
                _output.WriteLine($"pattern {i} threw {ex.GetType().Name}: {ex.Message}");
                continue;
            }

            byStatus[result.Status] = byStatus.GetValueOrDefault(result.Status) + 1;
            byType[result.Type] = byType.GetValueOrDefault(result.Type) + 1;

            if (!result.Decoded)
            {
                Assert.Equal(string.Empty, result.Text);
                continue;
            }

            decoded++;

            if (!Ft8MessageTypes.IsSupported(result.Type))
            {
                decodesForUnbuiltTypes++;
                continue;
            }

            // A decode whose callsign fields could not be resolved would be the worst of the three:
            // a message on the screen with a call nobody sent. Re-derived here from the fields
            // rather than trusted from the status. Only the standard message has callsign fields;
            // free text and telemetry carry none, so there is nothing there to be unresolved.
            if (result.Type == Ft8MessageType.Standard
                && (result.Fields.CallToType == Ft8FieldType.Unknown
                    || result.Fields.CallDeType == Ft8FieldType.Unknown
                    || string.IsNullOrEmpty(result.Fields.CallTo)
                    || string.IsNullOrEmpty(result.Fields.CallDe)))
            {
                decodesWithUnresolvedCallsigns++;
            }
        }

        _output.WriteLine($"corpus size : {FuzzSize}   seed: {Seed}");
        _output.WriteLine($"decoded     : {decoded}");
        _output.WriteLine("by status:");
        foreach (var (status, count) in byStatus.OrderByDescending(p => p.Value))
        {
            _output.WriteLine($"    {status,-24}{count}");
        }

        _output.WriteLine("by declared type:");
        foreach (var (type, count) in byType.OrderByDescending(p => p.Value))
        {
            _output.WriteLine($"    {type,-24}{count}");
        }

        _output.WriteLine($"exceptions                              : {exceptions}");
        _output.WriteLine($"decodes for a type not built            : {decodesForUnbuiltTypes}");
        _output.WriteLine($"decodes with an unresolvable callsign   : {decodesWithUnresolvedCallsigns}");

        Assert.Equal(0, exceptions);
        Assert.Equal(0, decodesForUnbuiltTypes);
        Assert.Equal(0, decodesWithUnresolvedCallsigns);
        Assert.True(decoded > 0, "nothing decoded at all, so the refusals prove nothing.");
    }

    /// <summary>
    /// Text through pack, the container, the encoder, the parity check, back through the container,
    /// and out as the same text.
    /// </summary>
    /// <remarks>
    /// <b>The first time in this phase that words have made the round trip through the proven
    /// encoder.</b> Step 1 proved the encoder against the pin's tables; unit 206 put message-shaped
    /// buffers through it. This puts a callsign and a grid in one end and reads them out of the
    /// other.
    /// </remarks>
    [Fact]
    public void TextMakesTheWholeRoundTripThroughTheEncoder()
    {
        var random = new Random(Seed);
        var carried = 0;

        Span<byte> message = stackalloc byte[Ft8StandardMessage.MessageBytes];
        Span<byte> payload = stackalloc byte[Ft8Payload.PayloadBytes];
        Span<byte> readBack = stackalloc byte[Ft8Payload.MessageBytes];
        var codeword = new byte[LdpcEncoder.CodewordBytes];

        for (var i = 0; i < 2000; i++)
        {
            var suffixKind = random.Next(2) == 0 ? "/R" : "/P";
            var callTo = GenerateFirstField(random, i, suffixKind);
            var callDe = GenerateCallsign(random, suffixKind, out _);
            var extra = GenerateExtra(random, i);

            Assert.Equal(Ft8PackResult.Ok, Ft8StandardMessage.TryPack(callTo, callDe, extra, message));

            // No packed message ever has a bit set past the seventy-seventh, so the container's own
            // refusal is never tripped.
            Assert.Equal(0, message[9] & 0x07);

            Ft8Payload.Create(message, payload);
            LdpcEncoder.Encode(payload, codeword);

            var bits = LdpcCheck.UnpackMsbFirst(codeword, Ft8Tables.LdpcN);
            var failing = LdpcCheck.FailingCount(
                LdpcCheck.SyndromeFromNm(bits, Ft8Tables.LdpcNm, Ft8Tables.LdpcNumRows));
            Assert.Equal(0, failing);

            Assert.True(Ft8Payload.TryRead(payload, readBack));

            var result = Ft8MessageDecoder.Decode(readBack);
            Assert.True(result.Decoded, $"[{callTo}] [{callDe}] [{extra}] did not come back.");

            var expected = string.IsNullOrEmpty(extra)
                ? $"{callTo} {callDe}"
                : $"{callTo} {callDe} {extra}";
            Assert.Equal(expected, result.Text);
            carried++;
        }

        _output.WriteLine($"messages carried whole through pack, CRC, encode, parity, read and unpack : {carried}");
        _output.WriteLine("every one of them cleared all 83 parity checks and came back as the text that went in");
    }

    /// <summary>
    /// A wrong-length buffer is a caller mistake and is refused loudly, where a bad pattern is a
    /// bad signal and is refused quietly.
    /// </summary>
    [Fact]
    public void AWrongLengthBufferIsTheOneThingThatThrows()
    {
        Assert.Throws<ArgumentException>(() => Ft8MessageDecoder.Decode(new byte[9]));
        Assert.Throws<ArgumentException>(() => Ft8MessageDecoder.Decode(new byte[11]));
        Assert.Throws<ArgumentException>(() => Ft8MessageDecoder.Decode(Array.Empty<byte>()));
    }

    /// <summary>A message with a hashed callsign in it is refused whole, not returned with a hole.</summary>
    [Fact]
    public void AMessageCarryingAHashedCallsignIsRefusedWhole()
    {
        var message = StandardMessageBytes("CQ", "K1ABC", "FN42");

        // Put a value from the hashed sub-range into the second callsign field, leaving everything
        // else a well-formed standard message.
        var hashed = Ft8CallsignField.TokenRangeSize + 12345u;
        var n29b = (hashed << 1) | 0u;

        message[3] = (byte)((message[3] & 0xF8) | (byte)(n29b >> 26));
        message[4] = (byte)(n29b >> 18);
        message[5] = (byte)(n29b >> 10);
        message[6] = (byte)(n29b >> 2);
        message[7] = (byte)((message[7] & 0x3F) | (byte)(n29b << 6));

        var result = Ft8MessageDecoder.Decode(message);

        Assert.False(result.Decoded);
        Assert.Equal(Ft8DecodeStatus.UnresolvedCallsign, result.Status);
        Assert.Equal(string.Empty, result.Text);
        Assert.True(string.IsNullOrEmpty(result.Fields.CallTo));
        Assert.True(string.IsNullOrEmpty(result.Fields.CallDe));
        Assert.True(string.IsNullOrEmpty(result.Fields.Extra));

        _output.WriteLine(
            "A hashed callsign refuses the whole message. No placeholder, no partial message, and "
            + "no numeric field returned as if it were a call.");
    }

    private static byte[] StandardMessageBytes(string callTo, string callDe, string extra)
    {
        var message = new byte[Ft8StandardMessage.MessageBytes];
        Assert.Equal(Ft8PackResult.Ok, Ft8StandardMessage.TryPack(callTo, callDe, extra, message));
        return message;
    }

    /// <summary>Overwrites the two type selectors in place, leaving the rest of the message alone.</summary>
    private static void SetSelectors(Span<byte> message, int primary, int secondary)
    {
        message[9] = (byte)((message[9] & ~0x38) | ((primary & 0x07) << 3));

        // n3's low two bits sit at the top of the last byte and its high bit at the bottom of the
        // one before, which is upstream's layout rather than a convenience.
        message[9] = (byte)((message[9] & 0x3F) | ((secondary & 0x03) << 6));
        message[8] = (byte)((message[8] & ~0x01) | ((secondary >> 2) & 0x01));
    }

    private static string GenerateFirstField(Random random, int index, string suffixKind) => (index % 5) switch
    {
        0 => "CQ",
        1 => "DE",
        2 => "QRZ",
        3 => "CQ " + random.Next(1000).ToString("000"),
        _ => GenerateCallsign(random, suffixKind, out _),
    };

    private static string GenerateCallsign(Random random, string suffixKind, out bool suffixed)
    {
        var call = random.Next(2) == 0
            ? $"{Alphanumeric(random)}{Alphanumeric(random)}{Digit(random)}{Letters(random, random.Next(1, 4))}"
            : $"{Alphanumeric(random)}{Digit(random)}{Letters(random, random.Next(1, 4))}";

        // The prefix work-arounds collide with calls spelled their compressed way, which is
        // upstream's own asymmetry and is measured in Ft8CallsignFieldTests. Kept out of this
        // corpus so that a message that does not come back is a message-layer finding.
        if (call.StartsWith("3D0", StringComparison.Ordinal)
            || (call[0] == 'Q' && Ft8Text.IsLetter(call[1])))
        {
            call = "K" + call[1..];
        }

        suffixed = random.Next(4) == 0;
        return suffixed ? call + suffixKind : call;
    }

    private static string GenerateExtra(Random random, int index) => (index % 6) switch
    {
        0 => string.Empty,
        1 => "RRR",
        2 => "RR73",
        3 => "73",
        4 => Ft8Text.IntToDd(random.Next(-30, 100), 2, true),
        _ => GenerateGrid(random),
    };

    private static string GenerateGrid(Random random)
    {
        while (true)
        {
            var grid = $"{(char)('A' + random.Next(18))}{(char)('A' + random.Next(18))}"
                + $"{Digit(random)}{Digit(random)}";

            // The one square whose name a token has taken is refused by the field on purpose, so a
            // corpus that generated it would be measuring that refusal rather than the message.
            if (grid != "RR73")
            {
                return grid;
            }
        }
    }

    private static char Alphanumeric(Random random)
    {
        var n = random.Next(36);
        return n < 10 ? (char)('0' + n) : (char)('A' + n - 10);
    }

    private static char Digit(Random random) => (char)('0' + random.Next(10));

    private static string Letters(Random random, int count)
    {
        var chars = new char[count];
        for (var i = 0; i < count; i++)
        {
            chars[i] = (char)('A' + random.Next(26));
        }

        return new string(chars);
    }
}
