namespace Ft8Sharp.Tests.Encode;

/// <summary>
/// Holds two tone sequences against each other and says where they first part company.
/// </summary>
/// <remarks>
/// <para>
/// <b>A position, not a count.</b> A comparison that reports "eleven symbols differ" tells the next
/// reader nothing about where the port went wrong; a comparison that reports the first differing
/// index tells them which stage of the chain to look at, because the symbol index maps onto the
/// codeword bit walk and onto the sync blocks.
/// </para>
/// <para>
/// <b>It is a separate type so it can be watched refusing.</b> The oracle may be absent or, as on
/// this machine, present and unusable — and a comparator that has only ever been exercised through
/// a gate that skipped is a comparator nobody has seen work. This one is fed a deliberately altered
/// sequence by a test that needs no oracle at all.
/// </para>
/// <para>
/// <b>It holds no values and prints none.</b> The result carries indexes and counts. What the tones
/// were belongs to upstream and to our own encoder, and neither is reported anywhere committed.
/// </para>
/// </remarks>
internal static class SymbolComparison
{
    /// <summary>What holding two sequences against each other found.</summary>
    /// <param name="Identical">True when every symbol of both agreed.</param>
    /// <param name="Compared">How many symbol positions were held against each other.</param>
    /// <param name="FirstDifference">The first index that differed, or -1 when none did.</param>
    /// <param name="DifferenceCount">How many positions differed in all.</param>
    /// <param name="Explanation">Why they differ, in words, without reproducing either sequence.</param>
    internal sealed record Result(
        bool Identical,
        int Compared,
        int FirstDifference,
        int DifferenceCount,
        string Explanation);

    /// <summary>Compares ours against upstream's, symbol by symbol.</summary>
    public static Result Compare(ReadOnlySpan<byte> ours, ReadOnlySpan<byte> theirs)
    {
        if (ours.Length != theirs.Length)
        {
            return new Result(
                Identical: false,
                Compared: 0,
                FirstDifference: -1,
                DifferenceCount: 0,
                Explanation: $"the two sequences are different lengths: ours has {ours.Length} "
                    + $"symbols and upstream's has {theirs.Length}, so there is nothing to compare "
                    + "position by position");
        }

        var first = -1;
        var differing = 0;
        for (var i = 0; i < ours.Length; i++)
        {
            if (ours[i] == theirs[i])
            {
                continue;
            }

            differing++;
            if (first < 0)
            {
                first = i;
            }
        }

        if (first < 0)
        {
            return new Result(
                Identical: true,
                Compared: ours.Length,
                FirstDifference: -1,
                DifferenceCount: 0,
                Explanation: $"all {ours.Length} symbols agreed");
        }

        return new Result(
            Identical: false,
            Compared: ours.Length,
            FirstDifference: first,
            DifferenceCount: differing,
            Explanation: $"{differing} of {ours.Length} symbols differ; the first is at position "
                + $"{first}, which is {Where(first)}");
    }

    /// <summary>
    /// Says what a symbol position <em>is</em>, because that is the first thing anyone reading a
    /// mismatch needs and it is not obvious from the index.
    /// </summary>
    /// <remarks>
    /// The three sync blocks sit at fixed offsets and everything else carries codeword bits. A
    /// difference inside a sync block means the Costas table or the block placement is wrong; a
    /// difference outside one means the codeword, the Gray map or the bit walk is. Those are very
    /// different faults and the index tells them apart for free.
    /// </remarks>
    public static string Where(int symbolIndex)
    {
        const int syncLength = 7;
        const int syncOffset = 36;

        for (var block = 0; block < 3; block++)
        {
            var start = block * syncOffset;
            if (symbolIndex >= start && symbolIndex < start + syncLength)
            {
                return $"inside sync block {block} (symbols {start} to {start + syncLength - 1}), "
                    + "so the Costas pattern or its placement is implicated rather than the codeword";
            }
        }

        var dataIndex = DataSymbolIndex(symbolIndex, syncLength, syncOffset);
        return $"a data symbol — the {Ordinal(dataIndex + 1)} of them — carrying codeword bits "
            + $"{dataIndex * 3} to {(dataIndex * 3) + 2}, so the codeword, the Gray map direction "
            + "or the bit walk is implicated rather than the sync blocks";
    }

    private static int DataSymbolIndex(int symbolIndex, int syncLength, int syncOffset)
    {
        var data = 0;
        for (var i = 0; i < symbolIndex; i++)
        {
            var isSync = false;
            for (var block = 0; block < 3; block++)
            {
                var start = block * syncOffset;
                if (i >= start && i < start + syncLength)
                {
                    isSync = true;
                    break;
                }
            }

            if (!isSync)
            {
                data++;
            }
        }

        return data;
    }

    private static string Ordinal(int n) => n switch
    {
        1 => "1st",
        2 => "2nd",
        3 => "3rd",
        _ when n % 10 == 1 && n % 100 != 11 => $"{n}st",
        _ when n % 10 == 2 && n % 100 != 12 => $"{n}nd",
        _ when n % 10 == 3 && n % 100 != 13 => $"{n}rd",
        _ => $"{n}th",
    };
}
