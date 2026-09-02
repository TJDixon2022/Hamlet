using Ft8Sharp;
using Ft8Sharp.Ldpc;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Ldpc;

/// <summary>
/// The shape of the FT8 LDPC(174,91) code, re-derived from <see cref="Ft8Tables"/> itself
/// rather than taken from a work instruction, a comment or a previous report.
/// </summary>
/// <remarks>
/// <para>
/// <b>Unit 215's task 1 wanted these numbers before a decoder was written</b>, because
/// every one of them is a dimension the decoder loops over: get the width of a
/// <c>Nm</c> row wrong and the update rule reads a padding zero as variable index 0.
/// They are printed as well as asserted, so the report can quote a measurement rather
/// than a restatement.
/// </para>
/// <para>
/// <b>No value is transcribed out of <c>Ft8Tables.g.cs</c>.</b> Everything below is
/// computed from the spans and the constants that file declares. The one number here
/// that is not a declared constant is the sum of <c>LdpcNumRows</c> -- the total number
/// of edges in the Tanner graph -- and it is summed, then cross-checked against the
/// other table's independent count of the same edges.
/// </para>
/// </remarks>
public class Ft8LdpcCodeShapeTests
{
    private readonly ITestOutputHelper _output;

    public Ft8LdpcCodeShapeTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// The code's dimensions and the two tables' widths, read off the tables.
    /// </summary>
    [Fact]
    public void TheCodeIsOneHundredAndSeventyFourBitsCarryingNinetyOneOverEightyThreeChecks()
    {
        var nmWidth = Ft8Tables.LdpcNm.Length / Ft8Tables.LdpcM;
        var mnWidth = Ft8Tables.LdpcMn.Length / Ft8Tables.LdpcN;

        _output.WriteLine($"LdpcN (variables)      : {Ft8Tables.LdpcN}");
        _output.WriteLine($"LdpcM (checks)         : {Ft8Tables.LdpcM}");
        _output.WriteLine($"payload bits (N - M)   : {LdpcEncoder.PayloadBits}");
        _output.WriteLine($"LdpcKBytes             : {Ft8Tables.LdpcKBytes}");
        _output.WriteLine($"codeword bytes         : {LdpcEncoder.CodewordBytes}");
        _output.WriteLine($"Nm row width, measured : {nmWidth}   (declared {Ft8Tables.LdpcNmRowWidth})");
        _output.WriteLine($"Mn row width, measured : {mnWidth}   (declared {Ft8Tables.LdpcMnRowWidth})");
        _output.WriteLine($"LdpcNumRows length     : {Ft8Tables.LdpcNumRows.Length}");
        _output.WriteLine($"LdpcGenerator length   : {Ft8Tables.LdpcGenerator.Length}");

        Assert.Equal(Ft8Tables.LdpcNmRowWidth, nmWidth);
        Assert.Equal(Ft8Tables.LdpcMnRowWidth, mnWidth);
        Assert.Equal(Ft8Tables.LdpcM, Ft8Tables.LdpcNumRows.Length);
        Assert.Equal(Ft8Tables.LdpcM * Ft8Tables.LdpcKBytes, Ft8Tables.LdpcGenerator.Length);
        Assert.Equal(Ft8Tables.LdpcN, LdpcEncoder.PayloadBits + Ft8Tables.LdpcM);
    }

    /// <summary>
    /// The number of edges in the Tanner graph, counted from each table separately and
    /// found to agree.
    /// </summary>
    /// <remarks>
    /// This is the loop bound the decoder's message passing runs over, and it is the one
    /// figure in the code's shape that neither table declares. Summing <c>NumRows</c>
    /// counts every real entry of <c>Nm</c>; multiplying <c>LdpcN</c> by the width of
    /// <c>Mn</c> counts the same edges from the variable side, because <c>Mn</c> has no
    /// padding. <b>The two must agree or one of the tables does not describe the graph
    /// the other does</b>, and unit 202 proved them exact transposes -- this is that
    /// property re-taken as a count, cheaply, on the night the decoder starts to depend
    /// on it.
    /// </remarks>
    [Fact]
    public void BothTablesCountTheSameNumberOfEdgesInTheTannerGraph()
    {
        var edgesFromNm = 0;
        for (var check = 0; check < Ft8Tables.LdpcM; check++)
        {
            edgesFromNm += Ft8Tables.LdpcNumRows[check];
        }

        var edgesFromMn = Ft8Tables.LdpcN * Ft8Tables.LdpcMnRowWidth;

        var minRow = int.MaxValue;
        var maxRow = int.MinValue;
        for (var check = 0; check < Ft8Tables.LdpcM; check++)
        {
            minRow = System.Math.Min(minRow, Ft8Tables.LdpcNumRows[check]);
            maxRow = System.Math.Max(maxRow, Ft8Tables.LdpcNumRows[check]);
        }

        _output.WriteLine($"sum of LdpcNumRows (edges from the check side)    : {edgesFromNm}");
        _output.WriteLine($"LdpcN * LdpcMnRowWidth (edges from the variable side): {edgesFromMn}");
        _output.WriteLine($"check degree, min..max                            : {minRow}..{maxRow}");
        _output.WriteLine($"padding slots in Nm (M * width - edges)           : "
            + $"{(Ft8Tables.LdpcM * Ft8Tables.LdpcNmRowWidth) - edgesFromNm}");

        Assert.Equal(edgesFromMn, edgesFromNm);
        Assert.True(maxRow <= Ft8Tables.LdpcNmRowWidth, $"A check covers {maxRow} variables, past the row width.");
    }
}
