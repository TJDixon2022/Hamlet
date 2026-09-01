using System.Text;
using Ft8Sharp.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Message;

/// <summary>
/// Free text and telemetry: the two types that share the same 71 bits under the type selector.
/// </summary>
/// <remarks>
/// <b>Where free text does not round-trip cleanly for a class of input, that is reported rather
/// than narrowed away.</b> The corpus includes the empty string, a full-length string, and strings
/// with leading and trailing spaces precisely because those are where the padding upstream applies
/// shows through, and the counts say what happened to each.
/// </remarks>
public class Ft8FreeTextTests
{
    private const int Seed = 20260901;
    private const int CorpusSize = 100_000;

    private readonly ITestOutputHelper _output;

    public Ft8FreeTextTests(ITestOutputHelper output) => _output = output;

    /// <summary>The alphabet free text packs against, built from the library rather than pasted.</summary>
    private static readonly char[] Alphabet =
        Enumerable.Range(0, Ft8Text.Length(Ft8CharTable.Full))
            .Select(i => Ft8Text.Character(i, Ft8CharTable.Full))
            .ToArray();

    [Fact]
    public void ASeededCorpusOfFreeTextRoundTrips()
    {
        var random = new Random(Seed);
        var roundTripped = 0;
        var trimmed = 0;
        var refusedAtPack = 0;
        var unexplained = new List<string>();

        Span<byte> message = stackalloc byte[Ft8Payload.MessageBytes];

        for (var i = 0; i < CorpusSize; i++)
        {
            var text = GenerateText(random, i);

            var packed = Ft8FreeText.TryPackText(text, message);
            if (packed != Ft8PackResult.Ok)
            {
                refusedAtPack++;
                unexplained.Add($"pack refused [{text}] : {packed}");
                continue;
            }

            Assert.Equal(Ft8MessageType.FreeText, Ft8MessageTypes.TypeOf(message));
            Assert.Equal(0, message[9] & 0x07);

            var result = Ft8MessageDecoder.Decode(message);
            Assert.True(result.Decoded, $"[{text}] did not decode.");
            Assert.Equal(Ft8MessageType.FreeText, result.Type);

            if (result.Text == text)
            {
                roundTripped++;
                continue;
            }

            // The one class that does not come back as it went in: the encoder pads to the full
            // width with spaces and the decoder trims them, so the spaces at either end are gone.
            // Upstream's shape, measured rather than avoided.
            if (result.Text == text.Trim(' '))
            {
                trimmed++;
                continue;
            }

            unexplained.Add($"[{text}] came back as [{result.Text}]");
        }

        _output.WriteLine($"corpus size            : {CorpusSize}   seed: {Seed}");
        _output.WriteLine($"round-tripped exactly  : {roundTripped}");
        _output.WriteLine($"round-tripped trimmed  : {trimmed} (leading or trailing spaces, which the padding removes)");
        _output.WriteLine($"refused at pack        : {refusedAtPack}");
        _output.WriteLine($"unexplained            : {unexplained.Count}");
        foreach (var line in unexplained.Take(20))
        {
            _output.WriteLine($"    {line}");
        }

        Assert.Empty(unexplained);
        Assert.Equal(CorpusSize, roundTripped + trimmed);
        Assert.True(trimmed > 0, "no padded string was generated, so that class is untested.");
    }

    /// <summary>The named edges of the free-text corpus, each asserted by name.</summary>
    [Fact]
    public void TheNamedEdgesOfFreeTextBehaveAsStated()
    {
        Span<byte> message = stackalloc byte[Ft8Payload.MessageBytes];

        // The empty string.
        Assert.Equal(Ft8PackResult.Ok, Ft8FreeText.TryPackText(string.Empty, message));
        Assert.Equal(Ft8MessageType.FreeText, Ft8MessageTypes.TypeOf(message));
        Assert.Equal(string.Empty, Ft8MessageDecoder.Decode(message).Text);

        // A full-length string.
        var full = new string('Z', Ft8FreeText.TextLength);
        Assert.Equal(Ft8PackResult.Ok, Ft8FreeText.TryPackText(full, message));
        Assert.Equal(full, Ft8MessageDecoder.Decode(message).Text);

        // One character longer than the type carries.
        Assert.Equal(
            Ft8PackResult.UnsupportedType,
            Ft8FreeText.TryPackText(new string('Z', Ft8FreeText.TextLength + 1), message));

        // Leading and trailing spaces, which the padding removes.
        Assert.Equal(Ft8PackResult.Ok, Ft8FreeText.TryPackText("  TNX BOB  ", message));
        Assert.Equal("TNX BOB", Ft8MessageDecoder.Decode(message).Text);

        // A character outside the alphabet is refused, not substituted.
        Assert.Equal(Ft8PackResult.UnsupportedType, Ft8FreeText.TryPackText("hello", message));
        Assert.Equal(Ft8PackResult.UnsupportedType, Ft8FreeText.TryPackText("A*B", message));
        Assert.Equal(Ft8PackResult.UnsupportedType, Ft8FreeText.TryPackText("A\tB", message));

        // The whole alphabet in one message, thirteen at a time, so no code point is untried.
        for (var start = 0; start < Alphabet.Length; start += Ft8FreeText.TextLength)
        {
            var chunk = new string(Alphabet, start, Math.Min(Ft8FreeText.TextLength, Alphabet.Length - start));
            Assert.Equal(Ft8PackResult.Ok, Ft8FreeText.TryPackText(chunk, message));
            Assert.Equal(chunk.Trim(' '), Ft8MessageDecoder.Decode(message).Text);
        }
    }

    [Fact]
    public void ASeededCorpusOfTelemetryRoundTrips()
    {
        var random = new Random(Seed);
        var roundTripped = 0;

        Span<byte> message = stackalloc byte[Ft8Payload.MessageBytes];
        Span<byte> readBack = stackalloc byte[Ft8FreeText.BodyBytes];
        var body = new byte[Ft8FreeText.BodyBytes];

        for (var i = 0; i < CorpusSize; i++)
        {
            switch (i)
            {
                case 0:
                    Array.Clear(body);
                    break;

                case 1:
                    Array.Fill(body, (byte)0xFF);
                    break;

                default:
                    random.NextBytes(body);
                    break;
            }

            // Only 71 bits are carried, so the top bit of the first byte is outside the field.
            body[0] &= 0x7F;

            Assert.Equal(Ft8PackResult.Ok, Ft8FreeText.TryPackTelemetry(body, message));
            Assert.Equal(Ft8MessageType.Telemetry, Ft8MessageTypes.TypeOf(message));
            Assert.Equal(0, message[9] & 0x07);

            Ft8FreeText.UnpackTelemetry(message, readBack);
            Assert.True(readBack.SequenceEqual(body), $"telemetry body {i} did not come back.");

            var result = Ft8MessageDecoder.Decode(message);
            Assert.True(result.Decoded);
            Assert.Equal(Ft8MessageType.Telemetry, result.Type);
            Assert.Equal(Ft8FreeText.TelemetryDigits, result.Text.Length);

            roundTripped++;
        }

        _output.WriteLine($"corpus size   : {CorpusSize}   seed: {Seed}");
        _output.WriteLine($"round-tripped : {roundTripped}, including all zeros and all ones");
        Assert.Equal(CorpusSize, roundTripped);
    }

    /// <summary>
    /// A free-text body larger than the type can carry is refused rather than shown as the low part
    /// of a number.
    /// </summary>
    [Fact]
    public void AFreeTextBodyOutsideTheTypesRangeIsRefused()
    {
        Span<byte> message = stackalloc byte[Ft8Payload.MessageBytes];
        var body = new byte[Ft8FreeText.BodyBytes];

        // All ones across the 71 bits is far above thirteen positions of a 42-character alphabet.
        Array.Fill(body, (byte)0xFF);
        body[0] &= 0x7F;
        Ft8FreeText.TryPackTelemetry(body, message);
        message[9] = 0;
        message[8] &= 0xFE;

        var result = Ft8MessageDecoder.Decode(message);
        Assert.Equal(Ft8MessageType.FreeText, result.Type);
        Assert.False(result.Decoded);
        Assert.Equal(Ft8DecodeStatus.MalformedField, result.Status);
        Assert.Equal(string.Empty, result.Text);

        // And the largest body that is inside the range still decodes: thirteen of the last
        // character of the alphabet.
        var largest = new string(Alphabet[^1], Ft8FreeText.TextLength);
        Assert.Equal(Ft8PackResult.Ok, Ft8FreeText.TryPackText(largest, message));
        Assert.Equal(largest, Ft8MessageDecoder.Decode(message).Text);
    }

    private static string GenerateText(Random random, int index)
    {
        switch (index)
        {
            case 0:
                return string.Empty;

            case 1:
                return new string(Alphabet[1], Ft8FreeText.TextLength);

            case 2:
                return "  LEADING";

            case 3:
                return "TRAILING  ";

            case 4:
                return " BOTH ENDS ";
        }

        var length = random.Next(0, Ft8FreeText.TextLength + 1);
        var text = new StringBuilder(length);
        for (var i = 0; i < length; i++)
        {
            text.Append(Alphabet[random.Next(Alphabet.Length)]);
        }

        return text.ToString();
    }
}
