using Ft8Sharp;

namespace Ft8Sharp.Tests.Encode;

/// <summary>
/// An independent second implementation of the symbol assembly, in the test project, which calls
/// nothing under <c>src/Ft8Sharp/Encode/</c>. The pattern of <c>CrcCheck</c>, <c>LdpcCheck</c> and
/// <c>HashCheck</c>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Its arithmetic is deliberately a different shape from the encoder's.</b> The encoder walks
/// the codeword bit by bit with a mask and a byte index, deciding at each of the 79 positions
/// whether it is on a sync symbol or a data symbol — which is upstream's shape. This one does the
/// opposite: it expands the codeword to a flat array of 174 bits, folds those into 58 data tones
/// with no notion of position at all, and only then splices the sync blocks in by index. If both
/// arrive at the same 79 symbols, the interleaving arithmetic is not carrying a fencepost error in
/// either of them.
/// </para>
/// <para>
/// <b>What this checks and what it does not.</b> It is a second implementation of <em>the
/// layout</em>: how a codeword becomes a sequence. It takes the codeword as given and says nothing
/// about whether that codeword is right — <c>LdpcCheck</c>'s 83 parity checks are what speak to
/// that, and they are run over the same corpus in <c>Ft8SymbolCriterionOneTests</c>. It also reads
/// the same two checked-in tables the encoder does, because those are the port's data and a second
/// transcription of them would be a worse test than none.
/// </para>
/// <para>
/// <b>This is the weakest of the three legs and it is the only one available tonight.</b> Two
/// implementations written by the same session an hour apart share whatever the session
/// misunderstood; agreement here is consistency, not correctness. Work instruction 209 makes this
/// task droppable only where a comparison against upstream's own tones actually ran. It did not —
/// there is no C toolchain on this machine to build the reference generator with — so this is not
/// dropped, and it is reported as what it is.
/// </para>
/// </remarks>
internal static class SymbolCheck
{
    /// <summary>Total channel symbols, stated here rather than read from the encoder under test.</summary>
    public const int SymbolCount = 79;

    /// <summary>Symbols per sync block.</summary>
    public const int SyncLength = 7;

    /// <summary>How many sync blocks there are.</summary>
    public const int SyncCount = 3;

    /// <summary>Where each sync block starts.</summary>
    public const int SyncStride = 36;

    /// <summary>Codeword bits carried by each data symbol.</summary>
    public const int BitsPerSymbol = 3;

    /// <summary>Codeword length in bits.</summary>
    public const int CodewordBits = 174;

    /// <summary>
    /// Lays a codeword out into channel symbols, by its own arithmetic.
    /// </summary>
    /// <param name="codeword">22 bytes holding 174 bits, most significant bit of byte zero first.</param>
    public static byte[] Lay(ReadOnlySpan<byte> codeword)
    {
        // 1. Flatten. No masks, no running byte index: one bit per array element.
        var bits = new int[CodewordBits];
        for (var i = 0; i < CodewordBits; i++)
        {
            bits[i] = (codeword[i / 8] >> (7 - (i % 8))) & 1;
        }

        // 2. Fold into data tones, three bits at a time, first bit most significant. This step has
        //    no idea where in the transmission any of them will end up.
        var gray = Ft8Tables.Ft8GrayMap;
        var dataTones = new byte[CodewordBits / BitsPerSymbol];
        for (var d = 0; d < dataTones.Length; d++)
        {
            var group = 0;
            for (var b = 0; b < BitsPerSymbol; b++)
            {
                group = (group * 2) + bits[(d * BitsPerSymbol) + b];
            }

            dataTones[d] = gray[group];
        }

        // 3. Splice. Build the set of sync positions first, then fill everything else in order from
        //    the data tones — the reverse of deciding position by position as the encoder does.
        var isSync = new bool[SymbolCount];
        for (var block = 0; block < SyncCount; block++)
        {
            for (var i = 0; i < SyncLength; i++)
            {
                isSync[(block * SyncStride) + i] = true;
            }
        }

        var costas = Ft8Tables.Ft8CostasPattern;
        var symbols = new byte[SymbolCount];
        var taken = 0;
        for (var position = 0; position < SymbolCount; position++)
        {
            if (isSync[position])
            {
                symbols[position] = costas[position % SyncStride];
                continue;
            }

            symbols[position] = dataTones[taken++];
        }

        if (taken != dataTones.Length)
        {
            throw new InvalidOperationException(
                $"the second implementation placed {taken} of {dataTones.Length} data tones, so its "
                + "own geometry does not close and it cannot check anything.");
        }

        return symbols;
    }

    /// <summary>
    /// The sync positions, worked out independently of the encoder's own <c>IsSyncSymbol</c>.
    /// </summary>
    public static IReadOnlyList<int> SyncPositions()
    {
        var positions = new List<int>();
        for (var block = 0; block < SyncCount; block++)
        {
            for (var i = 0; i < SyncLength; i++)
            {
                positions.Add((block * SyncStride) + i);
            }
        }

        return positions;
    }
}
