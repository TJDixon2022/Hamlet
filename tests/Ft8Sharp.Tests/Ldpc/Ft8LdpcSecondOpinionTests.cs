using Ft8Sharp;
using Ft8Sharp.Ldpc;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Ldpc;

/// <summary>
/// Two corroborations of the parity proof, both reading only the checked-in tables, and
/// both cheap.
/// </summary>
/// <remarks>
/// Criterion 5 stands without either of these. They are here because each catches
/// something the basis-vector proof cannot: the first says the two <em>readings of a
/// codeword</em> agree, which is what a belief-propagation decoder leans on directly, and
/// the second says the code has the dimension it is published as having, which every
/// syndrome test in the suite would pass without.
/// </remarks>
public class Ft8LdpcSecondOpinionTests
{
    private const int RandomPayloadSeed = 20260831;
    private const int RandomPayloadCount = 500;

    private readonly ITestOutputHelper _output;

    public Ft8LdpcSecondOpinionTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// The syndrome built from <c>Mn</c> agrees bit for bit with the one built from
    /// <c>Nm</c>, on every codeword the parity proof tested.
    /// </summary>
    /// <remarks>
    /// Unit 202 proved <c>Nm</c> and <c>Mn</c> exact transposes as tables. This is the
    /// consequence that actually matters downstream: that reading a codeword check-by-check
    /// and reading it variable-by-variable give the same answer. Step 5's decoder passes
    /// messages in both directions along those same edges.
    /// </remarks>
    [Fact]
    public void TheMnSideSyndromeAgreesWithTheNmSideOnEveryCodewordTested()
    {
        var codewordsCompared = 0;
        var disagreements = new List<string>();

        void Compare(string label, byte[] payload)
        {
            var codeword = new byte[LdpcEncoder.CodewordBytes];
            LdpcEncoder.Encode(payload, codeword);
            var bits = LdpcCheck.UnpackMsbFirst(codeword, Ft8Tables.LdpcN);

            var fromNm = LdpcCheck.SyndromeFromNm(bits, Ft8Tables.LdpcNm, Ft8Tables.LdpcNumRows);
            var fromMn = LdpcCheck.SyndromeFromMn(bits, Ft8Tables.LdpcMn);
            codewordsCompared++;

            for (var check = 0; check < Ft8Tables.LdpcM; check++)
            {
                if (fromNm[check] != fromMn[check])
                {
                    disagreements.Add($"    {label}: the two sides differ at check {check}");
                    return;
                }
            }
        }

        for (var bit = 0; bit < LdpcEncoder.PayloadBits; bit++)
        {
            Compare($"basis payload bit {bit}", Payloads.Basis(bit));
        }

        Compare("all zero", new byte[LdpcEncoder.PayloadBytes]);
        Compare("all ones", Payloads.AllOnes());
        Compare("alternating from 0", Payloads.Alternating(0));
        Compare("alternating from 1", Payloads.Alternating(1));

        var random = new Random(RandomPayloadSeed);
        for (var i = 0; i < RandomPayloadCount; i++)
        {
            Compare($"random payload {i}", Payloads.Random(random));
        }

        // A corrupted codeword too: agreeing on codewords whose syndrome is zero either way
        // would be a weak thing to have measured.
        var disturbed = new byte[LdpcEncoder.CodewordBytes];
        LdpcEncoder.Encode(Payloads.Alternating(0), disturbed);
        var disturbedBits = LdpcCheck.UnpackMsbFirst(disturbed, Ft8Tables.LdpcN);
        var nonZeroSyndromes = 0;

        for (var variable = 0; variable < Ft8Tables.LdpcN; variable++)
        {
            disturbedBits[variable] ^= 1;

            var fromNm = LdpcCheck.SyndromeFromNm(disturbedBits, Ft8Tables.LdpcNm, Ft8Tables.LdpcNumRows);
            var fromMn = LdpcCheck.SyndromeFromMn(disturbedBits, Ft8Tables.LdpcMn);
            codewordsCompared++;

            if (LdpcCheck.FailingCount(fromNm) > 0)
            {
                nonZeroSyndromes++;
            }

            for (var check = 0; check < Ft8Tables.LdpcM; check++)
            {
                if (fromNm[check] != fromMn[check])
                {
                    disagreements.Add(
                        $"    codeword with bit {variable} flipped: the two sides differ at check {check}");
                    break;
                }
            }

            disturbedBits[variable] ^= 1;
        }

        _output.WriteLine($"codewords compared            : {codewordsCompared}");
        _output.WriteLine($"of those, non-zero syndrome   : {nonZeroSyndromes}");
        _output.WriteLine($"seed for the random payloads  : {RandomPayloadSeed}");
        _output.WriteLine($"codewords where Nm and Mn disagreed : {disagreements.Count}");

        Assert.True(
            disagreements.Count == 0,
            $"{disagreements.Count} of {codewordsCompared} codewords give a different syndrome read "
            + "from Nm than read from Mn. The two tables are then not the transpose of each other "
            + "that a belief-propagation decoder assumes when it passes messages along those edges:"
            + Environment.NewLine + string.Join(Environment.NewLine, disagreements));
    }

    /// <summary>
    /// The rank of the parity-check matrix over GF(2) is 83, so the code's dimension is
    /// exactly 174 - 83 = 91 -- the same 91 the generator takes as its payload.
    /// </summary>
    /// <remarks>
    /// <b>A rank below 83 would mean a duplicated or dependent check row.</b> The tables
    /// would still pass every syndrome test in this suite and the code would be weaker than
    /// published, carrying fewer independent constraints than its parameters claim. Nothing
    /// else in this phase before the decode-rate measurement would notice.
    /// </remarks>
    [Fact]
    public void TheRankOfTheCheckMatrixOverGf2IsEightyThreeSoTheCodesDimensionIsNinetyOne()
    {
        var matrix = LdpcCheck.CheckMatrixFromNm(Ft8Tables.LdpcNm, Ft8Tables.LdpcNumRows);
        var rank = LdpcCheck.RankOverGf2(matrix);
        var dimension = Ft8Tables.LdpcN - rank;

        _output.WriteLine($"check matrix        : {Ft8Tables.LdpcM} x {Ft8Tables.LdpcN} over GF(2)");
        _output.WriteLine($"rank                : {rank}");
        _output.WriteLine($"code dimension      : {Ft8Tables.LdpcN} - {rank} = {dimension}");
        _output.WriteLine($"generator payload   : {LdpcEncoder.PayloadBits}");

        Assert.True(
            rank == Ft8Tables.LdpcM,
            $"The check matrix has rank {rank}, not {Ft8Tables.LdpcM}. That means "
            + $"{Ft8Tables.LdpcM - rank} of its rows are dependent on the others, so the code carries "
            + $"fewer independent constraints than LDPC(174,91) claims and is weaker than published. "
            + "Every syndrome test in this suite would still pass. This is a finding of the first "
            + "importance and no table should be repaired to make it go away.");

        Assert.True(
            dimension == LdpcEncoder.PayloadBits,
            $"The check matrix leaves a code of dimension {dimension} and the generator takes "
            + $"{LdpcEncoder.PayloadBits} payload bits. Those must be the same number.");
    }
}
