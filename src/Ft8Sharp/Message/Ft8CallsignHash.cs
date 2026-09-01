using System;

namespace Ft8Sharp.Message;

/// <summary>
/// The 22, 12 and 10-bit hashes a callsign is known by when a message refers to it without spelling
/// it out.
/// </summary>
/// <remarks>
/// <para>
/// <b>Ported from <c>ft8/message.c</c> in the pinned clone</b>, the static function that computes
/// all three widths and hands them to the callsign hash interface. Upstream computes them in one
/// place and this does too: the twelve and the ten are not independent hashes but truncations of
/// the twenty-two, and <see cref="TryCompute(string, out uint, out uint, out uint)"/> is the only
/// place any of the three is produced.
/// </para>
/// <para>
/// <b>This is the one artifact in this library where a plausible-looking guess would be invisible,
/// and it is why nothing here was written from memory.</b> Every other encoding this port has built
/// is a private agreement between its own packer and its own unpacker: if both are wrong in the
/// same way, the round trip still closes and step 3's bit-identical symbol comparison against
/// upstream is what settles it. A hash is not like that. <b>It travels on the air.</b> When another
/// station transmits a message naming a third station by the hash of its call, the only thing that
/// lets this library resolve it is that it computed <em>the same hash from the same call</em> as the
/// transmitter did. A hash function that is wrong but self-consistent round-trips perfectly through
/// its own cache, passes every corpus that can be written against it, and is silently and
/// permanently deaf on the air — and the failure looks like a quiet band rather than like a bug.
/// </para>
/// <para>
/// <b>So this is the one thing that does not rest on a round trip.</b> What it rests on is stated
/// where it can be checked: <c>UpstreamCallsignHashProvenanceTests</c> reads the pin at run time and
/// asserts this library's scalars against it by machine, and
/// <c>Ft8CallsignHashSecondOpinionTests</c> recomputes all three widths independently, from the pin
/// and without calling this class, over a large seeded corpus. Neither is proof that the port is
/// right; a misreading made twice survives both. <b>What settles it is step 3's bit-identical symbol
/// comparison against upstream, and that comparison must include a message carrying a hashed
/// callsign</b> or the hash goes unsettled into step 4.
/// </para>
/// <para>
/// <b>Faithful, not improved.</b> Upstream reads at most <see cref="MaxCallsignLength"/> characters
/// and pads the rest with the alphabet's space, so two calls that differ only past the eleventh
/// character hash identically; it is case-sensitive, because the alphabet it packs against holds
/// only upper-case letters; and it refuses any character outside that alphabet. All three of those
/// are properties of what is on the air, not choices this port is free to make, and all three are
/// measured rather than assumed.
/// </para>
/// </remarks>
public static class Ft8CallsignHash
{
    /// <summary>The number of characters of a callsign the hash reads. Anything past this is ignored.</summary>
    /// <remarks>
    /// Not a limit this port imposes and not one it may relax: a call longer than this hashes as its
    /// first <see cref="MaxCallsignLength"/> characters upstream, so the two collide on the air and
    /// must collide here.
    /// </remarks>
    public const int MaxCallsignLength = 11;

    /// <summary>The base the callsign's characters are packed against before the hash is taken.</summary>
    /// <remarks>The length of <see cref="Ft8CharTable.AlphanumericSpaceSlash"/>, and asserted to be.</remarks>
    public const ulong PackingBase = 38;

    /// <summary>The width of the widest of the three hashes.</summary>
    public const int Bits22 = 22;

    /// <summary>The width of the hash a non-standard-callsign message carries.</summary>
    public const int Bits12 = 12;

    /// <summary>The narrowest of the three hashes.</summary>
    public const int Bits10 = 10;

    /// <summary>The mask that holds the 22-bit hash. The size of the field's hashed sub-range, less one.</summary>
    public const uint Mask22 = (1u << Bits22) - 1;

    /// <summary>How far the 22-bit hash is shifted down to give the 12-bit one.</summary>
    public const int Shift12 = Bits22 - Bits12;

    /// <summary>How far the 22-bit hash is shifted down to give the 10-bit one.</summary>
    public const int Shift10 = Bits22 - Bits10;

    /// <summary>
    /// The multiplier the packed callsign is multiplied by, of whose 64-bit product the top
    /// <see cref="Bits22"/> bits are the hash.
    /// </summary>
    /// <remarks>
    /// <b>This constant is the whole of the risk described above.</b> It is asserted against the pin
    /// by machine at run time rather than trusted; the test names it and reports whether it matched,
    /// and never prints it.
    /// </remarks>
    public const ulong Multiplier = 47055833459UL;

    /// <summary>How far the 64-bit product is shifted down to leave the hash in the low bits.</summary>
    public const int ProductShift = 64 - Bits22;

    /// <summary>
    /// Computes all three hashes of a callsign, or says the callsign cannot be hashed at all.
    /// </summary>
    /// <param name="callsign">The callsign, already trimmed and upper-cased.</param>
    /// <param name="hash22">The 22-bit hash, written only on success.</param>
    /// <param name="hash12">The 12-bit hash, written only on success.</param>
    /// <param name="hash10">The 10-bit hash, written only on success.</param>
    /// <returns>
    /// <see langword="false"/> where any of the first <see cref="MaxCallsignLength"/> characters is
    /// outside the alphabet the hash packs against — which is upstream's own refusal, and the reason
    /// a lower-case call has no hash.
    /// </returns>
    /// <remarks>
    /// <b>Never throws</b>, for any string of any length including the empty one. The empty string
    /// has a hash: it is eleven spaces, and it is the hash of a station that never named itself.
    /// Nothing in this library puts one in the cache — <see cref="Ft8CallsignCache"/> refuses a call
    /// shorter than three characters — but the function itself answers rather than refusing, exactly
    /// as upstream's does.
    /// </remarks>
    public static bool TryCompute(string callsign, out uint hash22, out uint hash12, out uint hash10)
    {
        hash22 = 0;
        hash12 = 0;
        hash10 = 0;

        if (callsign is null)
        {
            return false;
        }

        // Pack the call into a base-38 integer, left-aligned in eleven positions: upstream reads
        // the characters it has and then keeps multiplying by the base for the ones it has not,
        // which is the same thing as padding on the right with the alphabet's space at index zero.
        ulong packed = 0;
        var i = 0;
        while (i < callsign.Length && i < MaxCallsignLength)
        {
            var index = Ft8Text.Index(callsign[i], Ft8CharTable.AlphanumericSpaceSlash);
            if (index < 0)
            {
                return false;
            }

            packed = (PackingBase * packed) + (ulong)index;
            i++;
        }

        while (i < MaxCallsignLength)
        {
            packed = PackingBase * packed;
            i++;
        }

        // The top bits of a 64-bit product. The multiplication is deliberately allowed to wrap:
        // upstream's is an unsigned 64-bit multiply and the overflow is part of the hash.
        hash22 = (uint)((Multiplier * packed) >> ProductShift) & Mask22;
        hash12 = hash22 >> Shift12;
        hash10 = hash22 >> Shift10;
        return true;
    }

    /// <summary>
    /// The 22-bit hash alone, for the callers that want only that one.
    /// </summary>
    /// <param name="callsign">The callsign, already trimmed and upper-cased.</param>
    /// <param name="hash22">The 22-bit hash, written only on success.</param>
    /// <returns>Whether the callsign could be hashed.</returns>
    public static bool TryCompute(string callsign, out uint hash22) =>
        TryCompute(callsign, out hash22, out _, out _);

    /// <summary>
    /// How far a 22-bit hash is shifted down to give the hash of the requested width.
    /// </summary>
    /// <remarks>
    /// Upstream's own <c>hash_shift</c>, which is what lets one stored 22-bit value answer a lookup
    /// at any of the three widths. Zero for the widest, which is not a special case.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The width is not one of the three.</exception>
    public static int ShiftFor(Ft8CallsignHashWidth width) => width switch
    {
        Ft8CallsignHashWidth.Bits22 => 0,
        Ft8CallsignHashWidth.Bits12 => Shift12,
        Ft8CallsignHashWidth.Bits10 => Shift10,
        _ => throw new ArgumentOutOfRangeException(nameof(width), width, "Not one of the three hash widths."),
    };

    /// <summary>The number of bits a hash of the given width occupies.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The width is not one of the three.</exception>
    public static int BitsOf(Ft8CallsignHashWidth width) => width switch
    {
        Ft8CallsignHashWidth.Bits22 => Bits22,
        Ft8CallsignHashWidth.Bits12 => Bits12,
        Ft8CallsignHashWidth.Bits10 => Bits10,
        _ => throw new ArgumentOutOfRangeException(nameof(width), width, "Not one of the three hash widths."),
    };
}

/// <summary>Which of the three hashes of a callsign a message is carrying.</summary>
/// <remarks>
/// Upstream's <c>ftx_callsign_hash_type_t</c>, in upstream's order. The three are not independent:
/// the narrower two are truncations of the widest, which is why one stored value answers all three.
/// </remarks>
public enum Ft8CallsignHashWidth
{
    /// <summary>The 22-bit hash a standard message carries in place of a non-standard callsign.</summary>
    Bits22,

    /// <summary>The 12-bit hash a non-standard-callsign message carries for its hashed companion.</summary>
    Bits12,

    /// <summary>The 10-bit hash. No message type this library builds carries one.</summary>
    Bits10,
}
