using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests;

/// <summary>
/// Proves the conversion is sound, without running a decoder.
/// </summary>
/// <remarks>
/// <para>
/// A conversion can succeed and still be wrong. A table can arrive transposed, truncated or
/// shifted by one and every one of those files compiles. These assertions cost microseconds
/// and catch all three before four stages of work are built on top of them.
/// </para>
/// <para>
/// <b>This is not LDPC parity.</b> Encoding a known message and checking its parity against
/// reference bits is criterion 5 and a separate unit; nothing here claims to be it. What is
/// asserted here is internal consistency — that the four LDPC tables describe one and the same
/// incidence structure, counted from both sides.
/// </para>
/// <para>
/// These run against the checked-in generated file rather than against the clone, so they need
/// no reference material and never skip. The clone-gated test next door is what proves that
/// file is what the converter produces; this is what proves the thing it produced hangs together.
/// <b>No value is printed here</b> — counts, ranges stated as coverage, and pass or fail.
/// </para>
/// </remarks>
public class Ft8TableGeometryTests
{
    private readonly ITestOutputHelper _output;

    public Ft8TableGeometryTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void ElementCountsAndDerivedGeometryAgree()
    {
        Assert.Equal(7, Ft8Tables.Ft8CostasPattern.Length);
        Assert.Equal(8, Ft8Tables.Ft8GrayMap.Length);
        Assert.Equal(996, Ft8Tables.LdpcGenerator.Length);
        Assert.Equal(581, Ft8Tables.LdpcNm.Length);
        Assert.Equal(522, Ft8Tables.LdpcMn.Length);
        Assert.Equal(83, Ft8Tables.LdpcNumRows.Length);

        Assert.Equal(83, Ft8Tables.LdpcM);
        Assert.Equal(174, Ft8Tables.LdpcN);
        Assert.Equal(12, Ft8Tables.LdpcKBytes);
        Assert.Equal(7, Ft8Tables.LdpcNmRowWidth);
        Assert.Equal(3, Ft8Tables.LdpcMnRowWidth);

        // The geometry is not asserted from the outside; each row count times each stride has to
        // come back to the element count that was parsed. LDPC(174,91) is what falls out.
        Assert.Equal(Ft8Tables.LdpcM * Ft8Tables.LdpcKBytes, Ft8Tables.LdpcGenerator.Length);
        Assert.Equal(Ft8Tables.LdpcM * Ft8Tables.LdpcNmRowWidth, Ft8Tables.LdpcNm.Length);
        Assert.Equal(Ft8Tables.LdpcN * Ft8Tables.LdpcMnRowWidth, Ft8Tables.LdpcMn.Length);
        Assert.Equal(Ft8Tables.LdpcM, Ft8Tables.LdpcNumRows.Length);

        _output.WriteLine(
            $"996 = {Ft8Tables.LdpcM} x {Ft8Tables.LdpcKBytes}, "
            + $"581 = {Ft8Tables.LdpcM} x {Ft8Tables.LdpcNmRowWidth}, "
            + $"522 = {Ft8Tables.LdpcN} x {Ft8Tables.LdpcMnRowWidth}");
    }

    [Fact]
    public void GrayMapIsAPermutationOfTheEightTones()
    {
        var seen = new bool[8];
        var map = Ft8Tables.Ft8GrayMap;
        for (var i = 0; i < map.Length; i++)
        {
            Assert.InRange(map[i], 0, 7);
            Assert.False(seen[map[i]], $"Ft8GrayMap maps two symbols onto the same tone (at index {i}).");
            seen[map[i]] = true;
        }

        Assert.DoesNotContain(false, seen);
        _output.WriteLine("Ft8GrayMap: every one of the 8 tones present exactly once.");
    }

    [Fact]
    public void CostasPatternIsSevenTonesInRange()
    {
        var costas = Ft8Tables.Ft8CostasPattern;
        Assert.Equal(7, costas.Length);
        for (var i = 0; i < costas.Length; i++)
        {
            Assert.InRange(costas[i], 0, 7);
        }

        _output.WriteLine("Ft8CostasPattern: 7 entries, every one inside the 8-tone alphabet.");
    }

    [Fact]
    public void NumRowsIsAWidthPerCheckAndSumsToMnsElementCount()
    {
        var numRows = Ft8Tables.LdpcNumRows;
        var total = 0;
        for (var m = 0; m < numRows.Length; m++)
        {
            Assert.InRange(numRows[m], 1, Ft8Tables.LdpcNmRowWidth);
            total += numRows[m];
        }

        // Num_rows and Mn count the same incidence structure from opposite sides: every edge is
        // one entry of Mn, and every edge is one of the real entries Num_rows accounts for.
        Assert.Equal(Ft8Tables.LdpcMn.Length, total);
        _output.WriteLine(
            $"LdpcNumRows: every entry in 1..{Ft8Tables.LdpcNmRowWidth}, and they sum to {total}, "
            + $"which is LdpcMn's element count ({Ft8Tables.LdpcMn.Length}).");
    }

    [Fact]
    public void NmPadsWithZeroExactlyWhereNumRowsSaysItDoes()
    {
        var nm = Ft8Tables.LdpcNm;
        var numRows = Ft8Tables.LdpcNumRows;
        for (var m = 0; m < Ft8Tables.LdpcM; m++)
        {
            for (var j = numRows[m]; j < Ft8Tables.LdpcNmRowWidth; j++)
            {
                Assert.Equal(0, nm[(m * Ft8Tables.LdpcNmRowWidth) + j]);
            }

            for (var j = 0; j < numRows[m]; j++)
            {
                Assert.NotEqual(0, nm[(m * Ft8Tables.LdpcNmRowWidth) + j]);
            }
        }

        _output.WriteLine(
            "LdpcNm: every row is real up to its LdpcNumRows length and zero after it, with no "
            + "zero inside the real part and no non-zero in the padding.");
    }

    [Fact]
    public void IndexBasesAreUpstreamsAndAreMeasuredRatherThanAssumed()
    {
        var checkBase = MeasuredBase(RealMnEntries(), Ft8Tables.LdpcM);
        var variableBase = MeasuredBase(RealNmEntries(), Ft8Tables.LdpcN);

        _output.WriteLine($"LdpcMn holds check indices    : {checkBase}-based, covering all "
            + $"{Ft8Tables.LdpcM} checks with no gaps.");
        _output.WriteLine($"LdpcNm holds variable indices : {variableBase}-based, covering all "
            + $"{Ft8Tables.LdpcN} variables with no gaps.");

        // Upstream's bases are kept, not tidied. A port that reads better and indexes differently
        // is the failure mode the phase plan names for callsign hashing, and it applies here.
        Assert.Equal(1, checkBase);
        Assert.Equal(1, variableBase);
    }

    [Fact]
    public void NmAndMnAreTransposesOfEachOther()
    {
        var nm = Ft8Tables.LdpcNm;
        var mn = Ft8Tables.LdpcMn;
        var numRows = Ft8Tables.LdpcNumRows;
        const int Base = 1;

        var edgesForwards = 0;
        for (var m = 0; m < Ft8Tables.LdpcM; m++)
        {
            for (var j = 0; j < numRows[m]; j++)
            {
                var n = nm[(m * Ft8Tables.LdpcNmRowWidth) + j] - Base;
                Assert.InRange(n, 0, Ft8Tables.LdpcN - 1);
                Assert.True(
                    RowContains(mn, n, Ft8Tables.LdpcMnRowWidth, m + Base),
                    $"Check {m} lists variable {n} in LdpcNm, but variable {n} does not list check "
                    + $"{m} in LdpcMn. The two tables do not describe the same graph.");
                edgesForwards++;
            }
        }

        var edgesBackwards = 0;
        for (var n = 0; n < Ft8Tables.LdpcN; n++)
        {
            for (var j = 0; j < Ft8Tables.LdpcMnRowWidth; j++)
            {
                var m = mn[(n * Ft8Tables.LdpcMnRowWidth) + j] - Base;
                Assert.InRange(m, 0, Ft8Tables.LdpcM - 1);
                Assert.True(
                    RowContains(nm, m, Ft8Tables.LdpcNmRowWidth, n + Base, numRows[m]),
                    $"Variable {n} lists check {m} in LdpcMn, but check {m} does not list variable "
                    + $"{n} in LdpcNm. The two tables do not describe the same graph.");
                edgesBackwards++;
            }
        }

        Assert.Equal(edgesForwards, edgesBackwards);
        _output.WriteLine(
            $"LdpcNm and LdpcMn agree on all {edgesForwards} edges in both directions.");
    }

    /// <summary>
    /// The smallest index the table uses, having checked that the indices cover a contiguous
    /// range of exactly <paramref name="cardinality"/> values.
    /// </summary>
    /// <remarks>
    /// Measured rather than assumed. A 0-based table would answer 0 here and a 1-based one
    /// answers 1; either way the caller is told which, and nothing is renumbered on the strength
    /// of it.
    /// </remarks>
    private static int MeasuredBase(IReadOnlyList<byte> indices, int cardinality)
    {
        var min = indices.Min();
        var max = indices.Max();
        Assert.Equal(cardinality - 1, max - min);

        var seen = new bool[cardinality];
        foreach (var index in indices)
        {
            seen[index - min] = true;
        }

        Assert.DoesNotContain(false, seen);
        return min;
    }

    private static List<byte> RealMnEntries()
    {
        var mn = Ft8Tables.LdpcMn;
        var entries = new List<byte>(mn.Length);
        for (var i = 0; i < mn.Length; i++)
        {
            entries.Add(mn[i]);
        }

        return entries;
    }

    private static List<byte> RealNmEntries()
    {
        var nm = Ft8Tables.LdpcNm;
        var numRows = Ft8Tables.LdpcNumRows;
        var entries = new List<byte>(nm.Length);
        for (var m = 0; m < Ft8Tables.LdpcM; m++)
        {
            for (var j = 0; j < numRows[m]; j++)
            {
                entries.Add(nm[(m * Ft8Tables.LdpcNmRowWidth) + j]);
            }
        }

        return entries;
    }

    private static bool RowContains(ReadOnlySpan<byte> table, int row, int stride, int value, int? length = null)
    {
        var take = length ?? stride;
        for (var j = 0; j < take; j++)
        {
            if (table[(row * stride) + j] == value)
            {
                return true;
            }
        }

        return false;
    }
}
