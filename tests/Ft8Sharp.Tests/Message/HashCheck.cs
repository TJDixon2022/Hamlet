using Ft8Sharp.Message;

namespace Ft8Sharp.Tests.Message;

/// <summary>
/// A second computation of the three callsign hashes, written from the pin and deliberately
/// unaware of the library it checks.
/// </summary>
/// <remarks>
/// <para>
/// <b>This is leg B, and it is unit 206's CRC leg used again because it worked.</b> It does not
/// call <see cref="Ft8CallsignHash"/>, it does not call <see cref="Ft8Text"/>, and it does not read
/// any constant out of the library — the alphabet below is built here from its own description of
/// itself rather than borrowed, so a wrong alphabet in the library cannot make this agree with it.
/// Two implementations that agree over a large corpus have not both made the same arithmetic slip.
/// </para>
/// <para>
/// <b>What it does not catch, stated so the report cannot overclaim.</b> A misreading of the pin
/// made once is caught here. A misreading made twice — the same wrong number read the same wrong
/// way into both implementations — is not, and no amount of corpus fixes that. That is what leg A's
/// machine read of the pin is for, and what step 3's bit-identical comparison against upstream's
/// own symbols finally settles.
/// </para>
/// </remarks>
internal static class HashCheck
{
    /// <summary>
    /// The alphabet the callsign is packed against, spelled out here rather than taken from the
    /// library, so that the two computations share nothing but the pin they were both read from.
    /// </summary>
    private const string Alphabet = " 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ/";

    /// <summary>
    /// Computes the three hashes independently, or reports that the callsign cannot be hashed.
    /// </summary>
    public static bool TryCompute(string callsign, out uint hash22, out uint hash12, out uint hash10)
    {
        hash22 = 0;
        hash12 = 0;
        hash10 = 0;

        ulong packed = 0;
        for (var i = 0; i < 11; i++)
        {
            // Past the end of the call the padding is the alphabet's own space, which is what
            // upstream's second loop amounts to: it keeps multiplying by the base and adds nothing,
            // and nothing is the index of the space.
            var c = i < callsign.Length ? callsign[i] : ' ';
            var index = Alphabet.IndexOf(c);
            if (index < 0)
            {
                return false;
            }

            packed = (packed * 38) + (ulong)index;
        }

        unchecked
        {
            var product = 47055833459UL * packed;
            hash22 = (uint)(product >> 42) & 0x3FFFFFu;
        }

        hash12 = hash22 >> 10;
        hash10 = hash22 >> 12;
        return true;
    }
}
