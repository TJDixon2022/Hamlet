using Ft8Sharp.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Message;

/// <summary>
/// The whole type cover taken again now that the cache exists, with each combination handed the best
/// message it could possibly carry rather than a standard message wearing another type's selectors.
/// </summary>
/// <remarks>
/// <para>
/// <b>This does not replace unit 207's cover and does not weaken it.</b> That one asks the harder
/// question — what does the dispatcher do with bits that declare a type they are not — and it still
/// runs, unchanged, and still finds every combination refusing except the ones it built. This one
/// asks the complementary question: <em>of the fifteen combinations, how many can this library
/// actually read when the bits really are of that type and the cache is warm?</em> That is the
/// number that moves when a type is built, and it is the number this unit changed.
/// </para>
/// <para>
/// <b>Exactly one row was expected to move and exactly one did.</b> Unit 207 measured four
/// combinations built and eleven refused. Tonight the non-standard-callsign row moves from refused
/// to built, which is five and ten. Nothing else in the table changed.
/// </para>
/// </remarks>
public class Ft8TypeCoverTests
{
    private readonly ITestOutputHelper _output;

    public Ft8TypeCoverTests(ITestOutputHelper output) => _output = output;

    /// <summary>How many combinations this library could read before tonight.</summary>
    private const int BuiltBeforeThisUnit = 4;

    [Fact]
    public void EveryTypeCombinationHasADefinedBehaviourWithAWarmCache()
    {
        var built = 0;
        var refused = 0;
        var combinations = 0;
        var builtTypes = new List<Ft8MessageType>();

        _output.WriteLine($"{"i3",-4}{"n3",-6}{"type",-24}{"behaviour",-30}status");

        for (var primary = 0; primary < Ft8MessageTypes.PrimaryCount; primary++)
        {
            var secondaries = primary == Ft8MessageTypes.PrimaryFreeTextFamily
                ? Enumerable.Range(0, Ft8MessageTypes.SecondaryCount).ToArray()
                : new[] { -1 };

            foreach (var secondary in secondaries)
            {
                combinations++;

                // A fresh cache for every combination, warmed only with the one call the message of
                // that type needs. No combination can see what another one stored.
                var cache = new Ft8CallsignCache();
                var message = BestCaseMessage(primary, secondary, cache);

                var type = Ft8MessageTypes.TypeOf(message);
                var result = Ft8MessageDecoder.Decode(message, cache);

                Assert.Equal(type, result.Type);

                if (result.Decoded)
                {
                    built++;
                    builtTypes.Add(type);
                    Assert.True(
                        Ft8MessageTypes.IsSupported(type),
                        $"i3={primary} n3={secondary} decoded as {type}, which this library has not built.");
                    Assert.False(string.IsNullOrEmpty(result.Text));
                }
                else
                {
                    refused++;
                    Assert.Equal(string.Empty, result.Text);
                    Assert.Equal(default, result.Fields);
                    Assert.True(
                        result.Status is Ft8DecodeStatus.UnsupportedType
                            or Ft8DecodeStatus.UnresolvedCallsign
                            or Ft8DecodeStatus.MalformedField,
                        $"i3={primary} n3={secondary} refused with no reason given.");
                }

                var moved = type == Ft8MessageType.NonstandardCallsign ? "  <-- THE ROW THAT MOVED" : string.Empty;
                _output.WriteLine(
                    $"{primary,-4}{(secondary < 0 ? "-" : secondary.ToString()),-6}{type,-24}"
                    + $"{(result.Decoded ? "built and round-tripping" : "refused as " + result.Status),-30}"
                    + $"{result.Status}{moved}");
            }
        }

        _output.WriteLine($"combinations enumerated : {combinations}");
        _output.WriteLine($"    built               : {built}  (was {BuiltBeforeThisUnit} before this unit)");
        _output.WriteLine($"    refused             : {refused}");

        Assert.Equal(Ft8MessageTypes.CombinationCount, combinations);
        Assert.Equal(combinations, built + refused);
        Assert.Equal(BuiltBeforeThisUnit + 1, built);
        Assert.Equal(combinations - built, refused);

        // Exactly one row moved, and it is the one this unit named.
        Assert.Contains(Ft8MessageType.NonstandardCallsign, builtTypes);
        Assert.Equal(
            new[]
            {
                Ft8MessageType.FreeText,
                Ft8MessageType.Telemetry,
                Ft8MessageType.Standard,
                Ft8MessageType.Standard,
                Ft8MessageType.NonstandardCallsign,
            },
            builtTypes);
    }

    /// <summary>
    /// The best message each combination could carry: a real one of that type where this library can
    /// build one, and a standard message wearing that combination's selectors where it cannot.
    /// </summary>
    private static byte[] BestCaseMessage(int primary, int secondary, Ft8CallsignCache cache)
    {
        var message = new byte[Ft8Payload.MessageBytes];

        if (primary == Ft8MessageTypes.PrimaryFreeTextFamily && secondary == 0)
        {
            Assert.Equal(Ft8PackResult.Ok, Ft8FreeText.TryPackText("HELLO WORLD", message));
            return message;
        }

        if (primary == Ft8MessageTypes.PrimaryFreeTextFamily && secondary == 5)
        {
            var telemetry = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x01 };
            Assert.Equal(Ft8PackResult.Ok, Ft8FreeText.TryPackTelemetry(telemetry, message));
            return message;
        }

        if (primary is Ft8MessageTypes.PrimaryStandard or Ft8MessageTypes.PrimaryStandardWithP)
        {
            var callDe = primary == Ft8MessageTypes.PrimaryStandard ? "K1ABC/R" : "K1ABC/P";
            Assert.Equal(Ft8PackResult.Ok, Ft8StandardMessage.TryPack("CQ", callDe, "FN42", cache, message));
            Assert.Equal(primary, Ft8MessageTypes.Primary(message));
            return message;
        }

        if (primary == Ft8MessageTypes.PrimaryNonstandard)
        {
            // The row that moved. A message naming one station by twelve bits, handed to a cache
            // that has heard that station — which is the whole of what this unit built.
            cache.Save("W9XYZ");
            Assert.Equal(
                Ft8PackResult.Ok,
                Ft8NonstandardMessage.TryPack("W9XYZ", "PJ4/KA1ABC", "RR73", cache, message));
            return message;
        }

        // Nothing this library builds. A standard message's bits with this combination's selectors
        // written over them, which is the most favourable thing an unbuilt type can be handed.
        Assert.Equal(Ft8PackResult.Ok, Ft8StandardMessage.TryPack("CQ", "K1ABC", "FN42", cache, message));
        SetSelectors(message, primary, secondary < 0 ? 0 : secondary);
        return message;
    }

    /// <summary>Writes the two type selectors into a message without disturbing anything else.</summary>
    private static void SetSelectors(Span<byte> message, int primary, int secondary)
    {
        message[9] = (byte)((message[9] & ~0x38) | ((primary & 0x07) << 3));
        message[9] = (byte)((message[9] & ~0xC0) | ((secondary & 0x03) << 6));
        message[8] = (byte)((message[8] & ~0x01) | ((secondary >> 2) & 0x01));
    }
}
