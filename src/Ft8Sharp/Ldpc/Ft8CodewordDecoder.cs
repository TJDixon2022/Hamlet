using System;
using Ft8Sharp.Message;

namespace Ft8Sharp.Ldpc;

/// <summary>
/// The gate: log-likelihood ratios in, <b>a message or nothing</b> out.
/// </summary>
/// <remarks>
/// <para>
/// <b>This is the seam extraction plugs into.</b> Everything above it -- reading magnitudes out
/// of a waterfall at a candidate's position and demapping three bits per symbol through the Gray
/// code -- does not exist in this library yet. Everything below it is proved: the belief
/// propagation in <see cref="LdpcDecoder"/>, the checksum in <see cref="Ft8Payload.TryRead"/>,
/// and the 77-bits-to-text unpacking in <see cref="Ft8MessageDecoder"/>. <b>Nothing here
/// re-implements any of them</b>, and in particular there is no second CRC check in this
/// library.
/// </para>
/// <para>
/// <b>THE RULE, AND BOTH HALVES ARE REQUIRED.</b> Every one of the code's 83 parity checks must
/// be satisfied, <em>and</em> the checksum must be the checksum of the payload it arrived with.
/// If either fails, <b>nothing is returned</b> -- not a partial, not a best guess, not a message
/// with a flag on it, not a message with a confidence beside it. That is <c>CLAUDE.md</c> §0.0 /
/// HM-DEC-009 at the exact place a tired session would fudge it, and it is upstream's own rule
/// too: <c>ftx_decode_candidate</c> has exactly two <c>return false</c> statements and they are
/// these two gates.
/// </para>
/// <para>
/// <b>The order is upstream's: parity first, then the checksum.</b> The checksum is not
/// consulted at all until the bits form a codeword, because until then the 91 bits it would be
/// computed over are not a payload -- they are the decoder's closest approach to one.
/// </para>
/// <para>
/// <b>The status is an API status and not a diagnostic surface.</b> One value on one result
/// type, saying which of the three gates a set of ratios stopped at. It is not a stage log, it
/// carries no score, no signal-to-noise ratio and nothing aimed at a display; the plan parks
/// legibility as a phase of its own and this does not start it.
/// </para>
/// <para>
/// <b>And it takes ratios and nothing else.</b> No message, no payload, no expected codeword.
/// The optional callsign cache is not a truth: it is the rolling memory FT8 itself requires to
/// resolve a hashed callsign, it is written to by decodes rather than by the caller, and a
/// message that does not carry a hash decodes identically without it.
/// </para>
/// </remarks>
public static class Ft8CodewordDecoder
{
    /// <summary>The number of log-likelihood ratios a decode takes.</summary>
    public const int RatioCount = LdpcDecoder.RatioCount;

    /// <summary>
    /// Corrects a damaged codeword and returns the message it carries, or nothing.
    /// </summary>
    /// <param name="ratios">
    /// <see cref="RatioCount"/> ratios, one per codeword bit. <b>Positive means the bit is more
    /// likely 1</b>, which is upstream's convention -- see <see cref="LdpcDecoder"/> for how
    /// that was settled and why it is not something to take on trust.
    /// </param>
    /// <param name="cache">
    /// The rolling callsign memory, or <see langword="null"/>. A message naming a station by a
    /// hash cannot be read without one; every other message reads the same either way.
    /// </param>
    /// <param name="maxIterations">
    /// How hard to try, defaulting to <see cref="LdpcDecoder.DefaultMaxIterations"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="ratios"/> is the wrong length or <paramref name="maxIterations"/> is
    /// negative. Refused by <see cref="LdpcDecoder"/> on the same terms.
    /// </exception>
    public static Ft8CodewordResult Decode(
        ReadOnlySpan<float> ratios,
        Ft8CallsignCache? cache = null,
        int maxIterations = LdpcDecoder.DefaultMaxIterations)
    {
        Span<byte> codewordBits = stackalloc byte[LdpcDecoder.CodewordBits];
        var correction = LdpcDecoder.Decode(ratios, codewordBits, maxIterations);

        // GATE 1 -- parity. Until this holds, the bits are the decoder's closest approach and
        // not a codeword, so there is nothing here to compute a checksum over.
        if (!correction.ParitySatisfied)
        {
            return Ft8CodewordResult.Refused(Ft8CodewordStatus.ParityNeverSatisfied, correction);
        }

        // The payload is the codeword's first 91 bits: the 77-bit message and the 14-bit
        // checksum that travels with it. Packed most significant bit first, which is the order
        // everything in this library stores bits in.
        Span<byte> payload = stackalloc byte[Ft8Payload.PayloadBytes];
        Pack(codewordBits[..Ft8Payload.PayloadBits], payload);

        // GATE 2 -- the checksum. This is the tempting case and it is the one criterion 2 is
        // actually about: belief propagation can converge on a perfectly valid codeword that is
        // not the one that was sent, and every parity check in the code will agree with it.
        // Only the checksum knows. Ft8Payload.TryRead is where that check already lives and it
        // is not written a second time here.
        Span<byte> message = stackalloc byte[Ft8Payload.MessageBytes];
        if (!Ft8Payload.TryRead(payload, message))
        {
            return Ft8CodewordResult.Refused(Ft8CodewordStatus.ChecksumFailed, correction);
        }

        // Past both gates: 77 bits that form a codeword and carry their own checksum. What they
        // say is step 2's question, and its answer is forwarded whole -- including its refusals,
        // which are a different thing from these two and are not laundered into them.
        var decoded = Ft8MessageDecoder.Decode(message, cache);

        return decoded.Decoded
            ? Ft8CodewordResult.FromMessage(correction, decoded)
            : Ft8CodewordResult.Unreadable(correction, decoded);
    }

    /// <summary>
    /// Packs one-byte-per-bit into bytes, most significant bit of each byte first.
    /// </summary>
    /// <remarks>
    /// Upstream's <c>pack_bits</c>. The destination is cleared first because only the set bits
    /// are written, and 5 of the destination's 96 bits are spare and must end up zero --
    /// <see cref="Ft8Payload.TryRead"/> refuses a payload whose spare bits are set, so leaving
    /// them to chance would turn a good decode into a rejected one.
    /// </remarks>
    private static void Pack(ReadOnlySpan<byte> bits, Span<byte> packed)
    {
        packed.Clear();
        for (var i = 0; i < bits.Length; i++)
        {
            if (bits[i] != 0)
            {
                packed[i / 8] |= (byte)(0x80u >> (i % 8));
            }
        }
    }
}

/// <summary>Which gate a set of ratios stopped at.</summary>
/// <remarks>
/// <b>Three refusals and one success, and the three are genuinely different things.</b> A
/// caller that wanted only "did it decode" has <see cref="Ft8CodewordResult.Decoded"/> and never
/// needs to read this; a caller counting where candidates die -- which is what step 6 will do --
/// needs to tell "the code could not repair it" from "it repaired it into something else" from
/// "the message is sound and this library cannot read that type".
/// </remarks>
public enum Ft8CodewordStatus
{
    /// <summary>Parity held, the checksum held, and the 77 bits became a message.</summary>
    Decoded,

    /// <summary>
    /// The decoder never reached a codeword within its iteration bound. Too much damage, or
    /// none of the ratios meant anything.
    /// </summary>
    ParityNeverSatisfied,

    /// <summary>
    /// The bits formed a valid codeword and its checksum disagreed with its payload.
    /// <b>Almost always a codeword that was never sent</b>, and the one this gate exists for.
    /// </summary>
    ChecksumFailed,

    /// <summary>
    /// Both gates held and the 77 bits are sound, but they are not a message this library can
    /// put into words -- an unsupported type, or a hashed callsign nothing has heard yet.
    /// <b>Nothing is displayed either way</b>; this is separated from the two above because it
    /// is not a failure of the signal.
    /// </summary>
    MessageNotReadable,
}

/// <summary>What <see cref="Ft8CodewordDecoder"/> made of 174 ratios.</summary>
/// <remarks>
/// <b><see cref="Message"/> is the default on every refusal</b>, and its own <c>Text</c> is the
/// empty string, so a caller that ignores <see cref="Status"/> gets nothing to display rather
/// than something that could be mistaken for a decode.
/// </remarks>
public readonly struct Ft8CodewordResult
{
    private Ft8CodewordResult(Ft8CodewordStatus status, LdpcDecodeResult correction, Ft8DecodeResult message)
    {
        Status = status;
        Correction = correction;
        Message = message;
    }

    /// <summary>Which gate the ratios stopped at.</summary>
    public Ft8CodewordStatus Status { get; }

    /// <summary>
    /// What the belief propagation cost and how close it came: the decoder's own return, passed
    /// through unchanged.
    /// </summary>
    public LdpcDecodeResult Correction { get; }

    /// <summary>The message, or the default where nothing is returned.</summary>
    public Ft8DecodeResult Message { get; }

    /// <summary>Whether a message came back. <b>The only thing a display should ask.</b></summary>
    public bool Decoded => Status == Ft8CodewordStatus.Decoded;

    internal static Ft8CodewordResult FromMessage(LdpcDecodeResult correction, Ft8DecodeResult message) =>
        new(Ft8CodewordStatus.Decoded, correction, message);

    internal static Ft8CodewordResult Unreadable(LdpcDecodeResult correction, Ft8DecodeResult message) =>
        new(Ft8CodewordStatus.MessageNotReadable, correction, message);

    internal static Ft8CodewordResult Refused(Ft8CodewordStatus status, LdpcDecodeResult correction) =>
        new(status, correction, default);
}
