using Ft8Sharp;
using Ft8Sharp.Ldpc;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Ldpc;

/// <summary>
/// The four things about the generator's layout that <see cref="LdpcEncoder"/> could have
/// taken on trust from a comment in upstream's source, measured instead.
/// </summary>
/// <remarks>
/// <para>
/// Each of the first three is established the same way, and it is the only way worth
/// anything: <b>the alternative reading is tried, and the reference parity tables refuse
/// it.</b> A comment saying "MSB first" is somebody's recollection; 83 checks failing on
/// the other order is a measurement of the tables actually in the tree.
/// </para>
/// <para>
/// <b>These tests read only the checked-in tables and never skip.</b> No clone, no
/// reference material, no environment variable. What ships is asserted sound on a machine
/// that has never seen ft8_lib.
/// </para>
/// </remarks>
public class Ft8LdpcLayoutTests
{
    private readonly ITestOutputHelper _output;

    public Ft8LdpcLayoutTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// MEASUREMENT 1 -- the bit order within a generator byte, established by the other
    /// order failing.
    /// </summary>
    /// <remarks>
    /// Reversing the bits within every generator byte is exactly equivalent to reading the
    /// unreversed table least-significant-bit-first against a most-significant-bit-first
    /// payload, so a reversed copy is the honest counterfactual and needs no second encoder.
    /// </remarks>
    [Fact]
    public void BitOrderInAGeneratorByteIsMostSignificantFirstBecauseTheOtherOrderFailsTheChecks()
    {
        var asShipped = TotalFailingChecksOverTheBasis(Ft8Tables.LdpcGenerator.ToArray());
        var reversed = TotalFailingChecksOverTheBasis(BitReversedWithinEachByte(Ft8Tables.LdpcGenerator));

        _output.WriteLine($"payloads encoded                     : {LdpcEncoder.PayloadBits}");
        _output.WriteLine($"checks per payload                   : {Ft8Tables.LdpcM}");
        _output.WriteLine($"failing checks, MSB-first (as shipped): {asShipped}");
        _output.WriteLine($"failing checks, LSB-first (the other) : {reversed}");

        Assert.True(
            asShipped == 0,
            $"Reading the generator most significant bit first left {asShipped} failing checks over "
            + $"the {LdpcEncoder.PayloadBits} basis payloads, and it should leave none.");

        Assert.True(
            reversed > 0,
            "Reading the generator least significant bit first left no failing check either, so the "
            + "bit order is not observable from these tables and nothing here measures it. That would "
            + "be a finding about the tables, not a passing test.");
    }

    /// <summary>
    /// MEASUREMENT 2 -- the five bits of each generator row that lie past the 91st.
    /// </summary>
    /// <remarks>
    /// <see cref="Ft8Tables.LdpcKBytes"/> is 12, which is 96 bits for a 91-bit payload.
    /// Upstream's encoder ANDs across all twelve bytes, so a non-zero spare bit would be
    /// folded into the parity of any payload that happened to set the matching spare bit --
    /// and, more to the point here, it would mean the row's width has been read wrong.
    /// </remarks>
    [Fact]
    public void TheSpareBitsPastTheNinetyFirstAreZeroInEveryGeneratorRow()
    {
        const int spareBits = (LdpcEncoder.PayloadBytes * 8) - LdpcEncoder.PayloadBits;
        var spareMask = (byte)((1 << spareBits) - 1);

        var rowsWithSpareBitsSet = new List<int>();
        for (var row = 0; row < Ft8Tables.LdpcM; row++)
        {
            var last = Ft8Tables.LdpcGenerator[(row * LdpcEncoder.PayloadBytes) + LdpcEncoder.PayloadBytes - 1];
            if ((last & spareMask) != 0)
            {
                rowsWithSpareBitsSet.Add(row);
            }
        }

        _output.WriteLine($"generator rows                : {Ft8Tables.LdpcM}");
        _output.WriteLine($"bits per row                  : {LdpcEncoder.PayloadBytes * 8}");
        _output.WriteLine($"bits the code carries         : {LdpcEncoder.PayloadBits}");
        _output.WriteLine($"spare bits per row            : {spareBits}");
        _output.WriteLine($"rows with any spare bit set   : {rowsWithSpareBitsSet.Count}");

        Assert.True(
            rowsWithSpareBitsSet.Count == 0,
            $"{rowsWithSpareBitsSet.Count} of {Ft8Tables.LdpcM} generator rows carry a set bit past "
            + $"bit {LdpcEncoder.PayloadBits}, at row indices "
            + $"[{string.Join(", ", rowsWithSpareBitsSet)}]. Those bits are outside the code, so either "
            + "the row width is being read wrong or the table is not what it is taken to be. Either is "
            + "a finding rather than a detail.");
    }

    /// <summary>
    /// MEASUREMENT 3 -- the codeword layout, with both readings tried and both reported.
    /// </summary>
    /// <remarks>
    /// This is a measurement of the check tables rather than a belief about upstream: one
    /// arrangement of message and parity satisfies all 83 checks and the other does not, and
    /// which one does is settled by <c>Nm</c>, not by a comment.
    /// </remarks>
    [Fact]
    public void TheCodewordCarriesTheMessageFirstAndTheParityAfterItBecauseParityFirstFails()
    {
        var messageFirst = 0;
        var parityFirst = 0;

        for (var bit = 0; bit < LdpcEncoder.PayloadBits; bit++)
        {
            var codeword = new byte[LdpcEncoder.CodewordBytes];
            LdpcEncoder.Encode(Payloads.Basis(bit), codeword);
            var bits = LdpcCheck.UnpackMsbFirst(codeword, Ft8Tables.LdpcN);

            messageFirst += LdpcCheck.FailingCount(
                LdpcCheck.SyndromeFromNm(bits, Ft8Tables.LdpcNm, Ft8Tables.LdpcNumRows));

            // The same bits, parity moved to the front and the message after it.
            var swapped = new byte[Ft8Tables.LdpcN];
            bits.AsSpan(LdpcEncoder.PayloadBits).CopyTo(swapped);
            bits.AsSpan(0, LdpcEncoder.PayloadBits).CopyTo(swapped.AsSpan(Ft8Tables.LdpcM));

            parityFirst += LdpcCheck.FailingCount(
                LdpcCheck.SyndromeFromNm(swapped, Ft8Tables.LdpcNm, Ft8Tables.LdpcNumRows));
        }

        _output.WriteLine($"payloads encoded                       : {LdpcEncoder.PayloadBits}");
        _output.WriteLine($"failing checks, message first + parity : {messageFirst}");
        _output.WriteLine($"failing checks, parity first + message : {parityFirst}");

        Assert.True(
            messageFirst == 0,
            $"Message-first left {messageFirst} failing checks and should leave none.");

        Assert.True(
            parityFirst > 0,
            "Parity-first left no failing check either, so the two layouts are indistinguishable "
            + "against these tables and this test measures nothing. That is a finding about the "
            + "tables rather than a pass.");
    }

    /// <summary>
    /// MEASUREMENT 4 -- the index base is upstream's, and the one comes off in exactly one
    /// place.
    /// </summary>
    /// <remarks>
    /// Unit 202 measured the base of <c>Nm</c> and <c>Mn</c> as 1. Nothing in this unit
    /// renumbers a table; the subtraction lives in <c>LdpcCheck.Variable</c> and nowhere
    /// else, and this test says the tables in the tree still carry the base that assumes.
    /// </remarks>
    [Fact]
    public void TheCheckTablesStillCarryUpstreamsOneBasedIndicesAndNothingRenumbersThem()
    {
        var nmMin = int.MaxValue;
        var nmMax = 0;
        for (var check = 0; check < Ft8Tables.LdpcM; check++)
        {
            var row = Ft8Tables.LdpcNm.Slice(check * Ft8Tables.LdpcNmRowWidth, Ft8Tables.LdpcNmRowWidth);
            for (var i = 0; i < Ft8Tables.LdpcNumRows[check]; i++)
            {
                nmMin = Math.Min(nmMin, row[i]);
                nmMax = Math.Max(nmMax, row[i]);
            }
        }

        var mnMin = int.MaxValue;
        var mnMax = 0;
        foreach (var index in Ft8Tables.LdpcMn)
        {
            mnMin = Math.Min(mnMin, index);
            mnMax = Math.Max(mnMax, index);
        }

        _output.WriteLine($"Nm index range : {nmMin}..{nmMax}  (variables, 1..{Ft8Tables.LdpcN} if 1-based)");
        _output.WriteLine($"Mn index range : {mnMin}..{mnMax}  (checks,    1..{Ft8Tables.LdpcM} if 1-based)");
        _output.WriteLine("the one comes off in LdpcCheck.Variable, and in no other place in the tree");

        Assert.True(nmMin == 1 && nmMax == Ft8Tables.LdpcN, $"Nm spans {nmMin}..{nmMax}, not 1..{Ft8Tables.LdpcN}.");
        Assert.True(mnMin == 1 && mnMax == Ft8Tables.LdpcM, $"Mn spans {mnMin}..{mnMax}, not 1..{Ft8Tables.LdpcM}.");
    }

    /// <summary>
    /// A payload with a spare bit set is refused rather than quietly encoded, because the
    /// codeword that came back would look perfectly well formed.
    /// </summary>
    [Fact]
    public void TheEncoderRefusesAPayloadWhoseSpareBitsAreSet()
    {
        var payload = new byte[LdpcEncoder.PayloadBytes];
        payload[LdpcEncoder.PayloadBytes - 1] = 0x01;

        var thrown = Assert.Throws<ArgumentException>(
            () => LdpcEncoder.Encode(payload, new byte[LdpcEncoder.CodewordBytes]));

        _output.WriteLine($"refusal: {thrown.Message}");
    }

    /// <summary>
    /// Encodes all 91 weight-one payloads through the given generator and totals the checks
    /// that fail. A count, never a codeword.
    /// </summary>
    private static int TotalFailingChecksOverTheBasis(byte[] generator)
    {
        var failing = 0;
        var codeword = new byte[LdpcEncoder.CodewordBytes];

        for (var bit = 0; bit < LdpcEncoder.PayloadBits; bit++)
        {
            LdpcEncoder.Encode(generator, Payloads.Basis(bit), codeword);
            failing += LdpcCheck.FailingCount(LdpcCheck.SyndromeFromNm(
                LdpcCheck.UnpackMsbFirst(codeword, Ft8Tables.LdpcN),
                Ft8Tables.LdpcNm,
                Ft8Tables.LdpcNumRows));
        }

        return failing;
    }

    private static byte[] BitReversedWithinEachByte(ReadOnlySpan<byte> table)
    {
        var reversed = new byte[table.Length];
        for (var i = 0; i < table.Length; i++)
        {
            var value = table[i];
            byte flipped = 0;
            for (var bit = 0; bit < 8; bit++)
            {
                flipped |= (byte)(((value >> bit) & 1) << (7 - bit));
            }

            reversed[i] = flipped;
        }

        return reversed;
    }
}
