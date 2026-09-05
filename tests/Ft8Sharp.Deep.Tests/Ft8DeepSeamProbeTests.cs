using Ft8Sharp.Dsp;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Deep.Tests;

/// <summary>
/// <b>The one thing task 1.2's census found out of reach, and whether there is a public way round
/// it.</b> This is a MEASUREMENT of the port's surface, not a piece of step 2.
/// </summary>
/// <remarks>
/// <para>
/// <b>The finding.</b> Every stage of <c>Ft8SlotDecoder.Decode(Ft8Waterfall)</c> is public and the
/// loop can be reproduced from outside the assembly without <c>InternalsVisibleTo</c> - except that
/// <b>nothing outside <c>Ft8Sharp</c> can construct an <c>Ft8CodewordResult</c></b>. Its constructor
/// is private and <c>FromMessage</c>, <c>Unreadable</c> and <c>Refused</c> are internal. So a
/// codeword that ordered statistics decoding recovers, which belief propagation refused, cannot be
/// put into an <c>Ft8SlotMessage</c> and therefore cannot reach an <c>Ft8SlotResult</c> - which is
/// what the scoreboard reads.
/// </para>
/// <para>
/// <b>What this file measures is whether route A of <c>docs/unit245-deep-seam.md</c> §4 works</b>:
/// hand the recovered codeword back to <c>Ft8CodewordDecoder.Decode</c> as high-confidence ratios and
/// let the PORT produce the result, gates and all. It is worth measuring rather than asserting,
/// because the whole write-up rests on it and a route nobody ran is a route nobody knows the answer
/// to.
/// </para>
/// <para>
/// <b>NO PART OF STEP 2 IS BUILT HERE.</b> There is no ordered statistics decoding in this file and
/// none in <c>Ft8Sharp.Deep</c>. The codeword these tests hand back is one this test encoded itself,
/// which is exactly the point: it stands in for whatever a future OSD stage would produce, and what
/// is being measured is the port's willingness to turn a codeword into a result.
/// </para>
/// <para>
/// <b>And the prime directive survives the route, which is the half that matters.</b>
/// <c>CLAUDE.md</c> §0.0: never present a guess as a decode. On this route the port still applies
/// both of its own gates - parity and CRC-14 - to whatever is handed to it, so a codeword an OSD
/// stage got wrong is refused by the port in the port's own words. Nothing bypasses the checksum,
/// and <see cref="AWrongCodewordHandedBackIsStillRefused"/> is watched refusing one.
/// </para>
/// </remarks>
public class Ft8DeepSeamProbeTests(ITestOutputHelper output)
{
    /// <summary>A valid 174-bit codeword for one free-text message, as 174 bytes of 0 or 1.</summary>
    private static byte[] CodewordFor(string text)
    {
        Span<byte> message = stackalloc byte[Ft8Payload.MessageBytes];
        Assert.Equal(Ft8PackResult.Ok, Ft8FreeText.TryPackText(text, message));

        Span<byte> payload = stackalloc byte[Ft8Payload.PayloadBytes];
        Ft8Payload.Create(message, payload);

        Span<byte> packed = stackalloc byte[LdpcEncoder.CodewordBytes];
        LdpcEncoder.Encode(payload, packed);

        var bits = new byte[LdpcDecoder.CodewordBits];
        for (var i = 0; i < bits.Length; i++)
        {
            bits[i] = (byte)((packed[i / 8] >> (7 - (i % 8))) & 1);
        }

        return bits;
    }

    /// <summary>
    /// Ratios that say each bit confidently, on upstream's own scale.
    /// <c>Ft8SoftSymbols.Normalise</c> is the port's, called and not re-implemented.
    /// </summary>
    private static float[] RatiosFor(ReadOnlySpan<byte> codewordBits)
    {
        var ratios = new float[Ft8SoftSymbols.RatioCount];
        for (var i = 0; i < ratios.Length; i++)
        {
            // Positive means the bit is more likely one - Ft8SoftSymbols.Extract's own wording,
            // and LdpcDecoder.Decode's.
            ratios[i] = codewordBits[i] == 1 ? 1.0f : -1.0f;
        }

        Ft8SoftSymbols.Normalise(ratios);
        return ratios;
    }

    /// <summary>
    /// <b>Route A works.</b> A codeword handed back to the port as high-confidence ratios comes back
    /// as a real <c>Ft8CodewordResult</c> carrying the message, using public members only.
    /// </summary>
    [Fact]
    public void ACodewordHandedBackToThePortBecomesARealResultThroughPublicMembersOnly()
    {
        const string text = "HAMLET 245";

        var result = Ft8CodewordDecoder.Decode(RatiosFor(CodewordFor(text)));

        output.WriteLine($"status  {result.Status}");
        output.WriteLine($"text    \"{result.Message.Text}\"");
        output.WriteLine($"iterations spent {result.Correction.Iterations}");

        Assert.Equal(Ft8CodewordStatus.Decoded, result.Status);
        Assert.Equal(text, result.Message.Text, StringComparer.Ordinal);
        Assert.True(result.Decoded);

        // And the result can then be put into the port's own records, which is what the scoreboard
        // reads. Both of these primary constructors ARE public - it is only the codeword result that
        // cannot be constructed from outside.
        var message = new Ft8SlotMessage(default, result);
        var slot = new Ft8SlotResult(1, 1, 1, 1, 0, new[] { message });

        Assert.Equal(text, slot.Texts[0], StringComparer.Ordinal);
    }

    /// <summary>
    /// <b>And the gates are still the port's.</b> A codeword an OSD stage got wrong is refused, so
    /// nothing on this route can present a guess as a decode.
    /// </summary>
    [Fact]
    public void AWrongCodewordHandedBackIsStillRefused()
    {
        var bits = CodewordFor("HAMLET 245");

        // Flip enough bits that this is no longer a codeword of the code. This stands in for an OSD
        // stage returning the wrong answer, which is the case CLAUDE.md 0.0 is about.
        for (var i = 0; i < 40; i++)
        {
            bits[i * 4] ^= 1;
        }

        var result = Ft8CodewordDecoder.Decode(RatiosFor(bits));

        output.WriteLine($"status  {result.Status}");
        output.WriteLine($"text    \"{result.Message.Text}\"");

        Assert.NotEqual(Ft8CodewordStatus.Decoded, result.Status);
        Assert.False(result.Decoded);
        Assert.Equal(string.Empty, result.Message.Text);
    }

    /// <summary>
    /// <b>The thing that is genuinely out of reach, stated as a compilable fact.</b> There is no
    /// public constructor and no public factory on <c>Ft8CodewordResult</c>; the only public producer
    /// is <c>Ft8CodewordDecoder.Decode</c>. If a later phase makes one public, this test is what will
    /// go red and say so.
    /// </summary>
    [Fact]
    public void NothingOutsideThePortCanConstructACodewordResult()
    {
        var type = typeof(Ft8CodewordResult);

        var publicConstructors = type.GetConstructors();
        var publicFactories = type
            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(m => m.ReturnType == type)
            .ToArray();

        output.WriteLine($"public constructors on {type.Name}: {publicConstructors.Length}");
        output.WriteLine($"public static factories returning it: {publicFactories.Length}");

        Assert.Empty(publicConstructors);
        Assert.Empty(publicFactories);

        // Ft8DecodeResult is sealed the same way, but IS obtainable, because Ft8MessageDecoder.Decode
        // is public and returns one. That asymmetry is the whole of the finding.
        Assert.Empty(typeof(Ft8DecodeResult).GetConstructors());
        Assert.NotNull(typeof(Ft8MessageDecoder).GetMethod(
            "Decode",
            new[] { typeof(ReadOnlySpan<byte>), typeof(Ft8CallsignCache) }));
    }
}
