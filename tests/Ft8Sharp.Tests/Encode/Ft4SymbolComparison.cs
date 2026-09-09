using Ft8Sharp.Encode;

namespace Ft8Sharp.Tests.Encode;

/// <summary>
/// Holds two FT4 tone sequences against each other and says where they first part company.
/// </summary>
/// <remarks>
/// <para>
/// <b>Beside <see cref="SymbolComparison"/> rather than a protocol argument on it.</b> The
/// comparison itself is protocol-neutral — two spans of bytes — and the part that is worth having is
/// not: saying <em>what a position is</em> needs FT4's own layout, which is two ramps, four
/// different sync groups of four at 1, 34, 67 and 100, and two codeword bits per data symbol rather
/// than three. A shared locator taking a layout parameter would put the FT8 explanation one edit
/// away from the fifty-one messages it already serves.
/// </para>
/// <para>
/// <b>It holds no values and prints none</b>, for the same reason: what the tones were belongs to
/// upstream and to this library, and neither is reported anywhere committed.
/// </para>
/// </remarks>
internal static class Ft4SymbolComparison
{
    /// <summary>Compares ours against upstream's, symbol by symbol.</summary>
    public static SymbolComparison.Result Compare(ReadOnlySpan<byte> ours, ReadOnlySpan<byte> theirs)
    {
        if (ours.Length != theirs.Length)
        {
            return new SymbolComparison.Result(
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

        return first < 0
            ? new SymbolComparison.Result(
                Identical: true,
                Compared: ours.Length,
                FirstDifference: -1,
                DifferenceCount: 0,
                Explanation: $"all {ours.Length} symbols agreed")
            : new SymbolComparison.Result(
                Identical: false,
                Compared: ours.Length,
                FirstDifference: first,
                DifferenceCount: differing,
                Explanation: $"{differing} of {ours.Length} symbols differ; the first is at position "
                    + $"{first}, which is {Where(first)}");
    }

    /// <summary>
    /// Says what an FT4 symbol position <em>is</em>, because that is the first thing anyone reading a
    /// mismatch needs and the index does not say it.
    /// </summary>
    /// <remarks>
    /// Three very different faults, told apart for free by the index: a difference at a ramp means
    /// the ends of the transmission are laid out wrongly; inside a sync group it means the Costas
    /// table, the group's row, or the group placement; anywhere else it means the codeword, the Gray
    /// map direction, the two-bit walk — or the payload exclusive-OR, which is FT4's alone and would
    /// move nearly every data symbol at once.
    /// </remarks>
    public static string Where(int symbolIndex)
    {
        if (Ft4SymbolEncoder.IsRampSymbol(symbolIndex))
        {
            return $"a ramp symbol (0 and {Ft4SymbolEncoder.SymbolCount - 1}), which carries no "
                + "payload at all, so the ends of the transmission are implicated rather than the "
                + "codeword";
        }

        if (Ft4SymbolEncoder.TrySyncPosition(symbolIndex, out var group, out var position))
        {
            var start = Ft4SymbolEncoder.SyncGroupStart(group);
            return $"position {position} of sync group {group} (symbols {start} to "
                + $"{start + Ft4SymbolEncoder.SyncGroupLength - 1}), so row {group} of the FT4 "
                + "Costas table or its placement is implicated rather than the codeword. FT4's four "
                + "groups carry four DIFFERENT patterns, so a port repeating row 0 shows up here";
        }

        var dataIndex = DataSymbolIndex(symbolIndex);
        var firstBit = dataIndex * Ft4SymbolEncoder.BitsPerSymbol;
        return $"a data symbol — the {Ordinal(dataIndex + 1)} of them — carrying codeword bits "
            + $"{firstBit} to {firstBit + Ft4SymbolEncoder.BitsPerSymbol - 1}, so the codeword, the "
            + "Gray map direction, the two-bit walk or the payload exclusive-OR is implicated rather "
            + "than the sync groups";
    }

    private static int DataSymbolIndex(int symbolIndex)
    {
        var data = 0;
        for (var i = 0; i < symbolIndex; i++)
        {
            if (Ft4SymbolEncoder.IsDataSymbol(i))
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
