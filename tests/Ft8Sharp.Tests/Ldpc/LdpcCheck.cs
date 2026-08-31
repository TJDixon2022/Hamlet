using Ft8Sharp;

namespace Ft8Sharp.Tests.Ldpc;

/// <summary>
/// The parity side of the FT8 LDPC(174,91) code, read straight out of the checked-in
/// tables, and the reason this unit's proof means anything.
/// </summary>
/// <remarks>
/// <para>
/// <b>Nothing here calls <see cref="Ft8Sharp.Ldpc.LdpcEncoder"/>, and that is deliberate.</b>
/// <c>ft8/constants.c</c> describes the code twice and independently -- once as
/// <c>kFTX_LDPC_generator</c>, which says how to make parity, and once as
/// <c>kFTX_LDPC_Nm</c> / <c>kFTX_LDPC_Mn</c> / <c>kFTX_LDPC_Num_rows</c>, which say how to
/// check it. A checker that shared code with the encoder would be agreeing with itself and
/// would prove nothing about whether the two descriptions are of the same code. This file
/// touches only the check tables.
/// </para>
/// <para>
/// <b>No table value ever leaves this file.</b> Every diagnostic it produces is a count, an
/// index or a yes/no. A syndrome is metadata about a codeword; the codeword itself is
/// licensed table data wearing a different hat, and a parity vector from a weight-one
/// payload is a column of the generator matrix by another route.
/// </para>
/// </remarks>
internal static class LdpcCheck
{
    /// <summary>
    /// The one place upstream's index base is taken off, and the only one.
    /// </summary>
    /// <remarks>
    /// Unit 202 <em>measured</em> the base of both <c>Nm</c> and <c>Mn</c> as 1 rather than
    /// assuming it, and the ruling is that the tables stay as upstream wrote them -- nothing
    /// is renumbered on the way in, because a renumbered table can no longer be compared
    /// against the source it came from. So the one comes off here, at the point of use,
    /// named, and nowhere else in the tree.
    /// </remarks>
    private const int UpstreamIndexBase = 1;

    /// <summary>Turns an index as upstream wrote it into a zero-based codeword position.</summary>
    private static int Variable(byte upstreamIndex) => upstreamIndex - UpstreamIndexBase;

    /// <summary>
    /// Unpacks a packed buffer into one byte per bit, most significant bit of each byte
    /// first, which is the order upstream stores both payload and codeword in.
    /// </summary>
    public static byte[] UnpackMsbFirst(ReadOnlySpan<byte> packed, int bitCount)
    {
        var bits = new byte[bitCount];
        for (var i = 0; i < bitCount; i++)
        {
            bits[i] = (byte)((packed[i / 8] >> (7 - (i % 8))) & 1);
        }

        return bits;
    }

    /// <summary>Packs one-byte-per-bit back into bytes, most significant bit first.</summary>
    public static byte[] PackMsbFirst(ReadOnlySpan<byte> bits)
    {
        var packed = new byte[(bits.Length + 7) / 8];
        for (var i = 0; i < bits.Length; i++)
        {
            if (bits[i] != 0)
            {
                packed[i / 8] |= (byte)(0x80u >> (i % 8));
            }
        }

        return packed;
    }

    /// <summary>
    /// The syndrome of a codeword from the check side: one bit per check node, each the
    /// XOR of the codeword bits that check names.
    /// </summary>
    /// <param name="codewordBits">
    /// <see cref="Ft8Tables.LdpcN"/> entries, one per variable node, each 0 or 1.
    /// </param>
    /// <param name="nm">
    /// The variables each check touches, <see cref="Ft8Tables.LdpcM"/> rows of
    /// <see cref="Ft8Tables.LdpcNmRowWidth"/>, in upstream's own index base.
    /// </param>
    /// <param name="numRows">How many of each <paramref name="nm"/> row's entries are real.</param>
    /// <returns>
    /// <see cref="Ft8Tables.LdpcM"/> entries, each 0 for a check that is satisfied and 1 for
    /// one that is not.
    /// </returns>
    public static byte[] SyndromeFromNm(
        ReadOnlySpan<byte> codewordBits,
        ReadOnlySpan<byte> nm,
        ReadOnlySpan<byte> numRows)
    {
        var syndrome = new byte[Ft8Tables.LdpcM];
        for (var check = 0; check < Ft8Tables.LdpcM; check++)
        {
            var row = nm.Slice(check * Ft8Tables.LdpcNmRowWidth, Ft8Tables.LdpcNmRowWidth);
            var sum = 0;
            for (var i = 0; i < numRows[check]; i++)
            {
                sum ^= codewordBits[Variable(row[i])];
            }

            syndrome[check] = (byte)sum;
        }

        return syndrome;
    }

    /// <summary>
    /// The same syndrome built from the other table: for each variable, the checks it
    /// takes part in, accumulated into those checks.
    /// </summary>
    /// <remarks>
    /// A second opinion on the same codeword. <c>Nm</c> and <c>Mn</c> were proved exact
    /// transposes by unit 202; this says the two <em>readings of a codeword</em> agree,
    /// which is the property a belief-propagation decoder leans on directly.
    /// </remarks>
    public static byte[] SyndromeFromMn(ReadOnlySpan<byte> codewordBits, ReadOnlySpan<byte> mn)
    {
        var syndrome = new byte[Ft8Tables.LdpcM];
        for (var variable = 0; variable < Ft8Tables.LdpcN; variable++)
        {
            if (codewordBits[variable] == 0)
            {
                continue;
            }

            var row = mn.Slice(variable * Ft8Tables.LdpcMnRowWidth, Ft8Tables.LdpcMnRowWidth);
            for (var i = 0; i < Ft8Tables.LdpcMnRowWidth; i++)
            {
                syndrome[Variable(row[i])] ^= 1;
            }
        }

        return syndrome;
    }

    /// <summary>How many checks a syndrome says are unsatisfied.</summary>
    public static int FailingCount(ReadOnlySpan<byte> syndrome)
    {
        var count = 0;
        foreach (var bit in syndrome)
        {
            count += bit;
        }

        return count;
    }

    /// <summary>Which checks a syndrome says are unsatisfied, by index.</summary>
    public static int[] FailingChecks(ReadOnlySpan<byte> syndrome)
    {
        var failing = new List<int>();
        for (var check = 0; check < syndrome.Length; check++)
        {
            if (syndrome[check] != 0)
            {
                failing.Add(check);
            }
        }

        return failing.ToArray();
    }

    /// <summary>
    /// The parity-check matrix as one row of <see cref="Ft8Tables.LdpcN"/> bits per check,
    /// built from <c>Nm</c>.
    /// </summary>
    public static byte[,] CheckMatrixFromNm(ReadOnlySpan<byte> nm, ReadOnlySpan<byte> numRows)
    {
        var matrix = new byte[Ft8Tables.LdpcM, Ft8Tables.LdpcN];
        for (var check = 0; check < Ft8Tables.LdpcM; check++)
        {
            var row = nm.Slice(check * Ft8Tables.LdpcNmRowWidth, Ft8Tables.LdpcNmRowWidth);
            for (var i = 0; i < numRows[check]; i++)
            {
                matrix[check, Variable(row[i])] ^= 1;
            }
        }

        return matrix;
    }

    /// <summary>
    /// How many checks flipping a given variable would disturb, according to <c>Mn</c>'s row
    /// for it.
    /// </summary>
    /// <remarks>
    /// Checks the variable appears in an <em>even</em> number of times are counted out
    /// rather than counted once: flipping the bit would toggle such a check twice and leave
    /// it satisfied. For a regular column-weight-3 code the two readings coincide, but the
    /// question being asked is "what would move", and that is the reading that answers it.
    /// </remarks>
    public static int ChecksDisturbedByFlippingFromMn(int variable, ReadOnlySpan<byte> mn)
    {
        var row = mn.Slice(variable * Ft8Tables.LdpcMnRowWidth, Ft8Tables.LdpcMnRowWidth);
        var multiplicity = new Dictionary<int, int>();
        for (var i = 0; i < Ft8Tables.LdpcMnRowWidth; i++)
        {
            var check = Variable(row[i]);
            multiplicity[check] = multiplicity.GetValueOrDefault(check) + 1;
        }

        return multiplicity.Values.Count(count => count % 2 == 1);
    }
}
