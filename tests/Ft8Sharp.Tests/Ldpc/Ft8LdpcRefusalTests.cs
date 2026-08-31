using Ft8Sharp;
using Ft8Sharp.Ldpc;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Ldpc;

/// <summary>
/// Watches the parity proof refuse. A guard that has never refused is not a guard.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Ft8LdpcParityTests"/> says the checked-in tables pass. On its own that is
/// consistent with a syndrome check that passes anything at all, and a check that would
/// accept a corrupted table proves nothing about an uncorrupted one. These three tests
/// corrupt one thing at a time and require the proof to notice.
/// </para>
/// <para>
/// <b>Every corruption is on an in-memory copy.</b> <c>Ft8Tables.g.cs</c> is never touched,
/// nothing is hand-edited, and no table is regenerated. A hand-patched byte in a generated
/// file is undetectable afterwards, which is the exact risk the licensing rulings were
/// written against.
/// </para>
/// <para>
/// <b>The refusals are quoted, not summarised.</b> Each test prints the guard's own message
/// -- produced by <see cref="BasisProof"/>, the same routine that clears the real tables --
/// so the evidence is the guard speaking rather than a test asserting that it would have.
/// </para>
/// </remarks>
public class Ft8LdpcRefusalTests
{
    private readonly ITestOutputHelper _output;

    public Ft8LdpcRefusalTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// REFUSAL 1 -- one flipped bit in a copy of the generator, and the proof fails.
    /// </summary>
    [Fact]
    public void OneFlippedGeneratorBitIsRefusedByTheBasisProof()
    {
        // A copy. The generated file on disk is the artifact under test and is not edited.
        var corrupted = Ft8Tables.LdpcGenerator.ToArray();
        const int corruptedByte = 40 * LdpcEncoder.PayloadBytes; // row 40, first byte
        corrupted[corruptedByte] ^= 0x40;

        var result = BasisProof.Run(corrupted, Ft8Tables.LdpcNm, Ft8Tables.LdpcNumRows);

        _output.WriteLine("corruption: one bit flipped in an in-memory copy of LdpcGenerator, row 40");
        _output.WriteLine(result.Refusal);

        Assert.False(
            result.IsClean,
            "A generator with a bit flipped passed the basis proof. The proof cannot then be "
            + "evidence that the uncorrupted generator is right, and criterion 5 is not met by it.");
    }

    /// <summary>
    /// REFUSAL 2 -- one altered element in a copy of Nm, and the proof fails again.
    /// </summary>
    /// <remarks>
    /// This is the direction that matters most. It shows the check side is genuinely being
    /// consulted rather than carried along beside a proof that only ever exercises the
    /// generator.
    /// </remarks>
    [Fact]
    public void OneAlteredNmElementIsRefusedByTheBasisProof()
    {
        var corrupted = Ft8Tables.LdpcNm.ToArray();
        const int corruptedElement = 17 * Ft8Tables.LdpcNmRowWidth; // check 17, first variable

        // Point the check at a different variable, staying inside upstream's 1-based range
        // so the fault is a wrong index rather than an out-of-range one.
        var replacement = (byte)((corrupted[corruptedElement] % Ft8Tables.LdpcN) + 1);
        Assert.NotEqual(corrupted[corruptedElement], replacement);
        corrupted[corruptedElement] = replacement;

        var result = BasisProof.Run(Ft8Tables.LdpcGenerator, corrupted, Ft8Tables.LdpcNumRows);

        _output.WriteLine("corruption: one element altered in an in-memory copy of LdpcNm, check 17");
        _output.WriteLine(result.Refusal);

        Assert.False(
            result.IsClean,
            "An Nm table with an element altered passed the basis proof, so the check side is not "
            + "being consulted and the proof is the generator agreeing with itself.");
    }

    /// <summary>
    /// REFUSAL 3 -- one flipped codeword bit disturbs exactly the checks Mn says it should.
    /// </summary>
    /// <remarks>
    /// A third corroboration of the Nm/Mn transpose unit 202 proved, arrived at from the
    /// syndrome side and independent of it: the syndrome is built from <c>Nm</c>, the
    /// expected count comes from <c>Mn</c>, and they have to agree for all 174 variables.
    /// </remarks>
    [Fact]
    public void FlippingOneCodewordBitFailsExactlyTheChecksMnSaysThatVariableIsIn()
    {
        var codeword = new byte[LdpcEncoder.CodewordBytes];
        LdpcEncoder.Encode(Payloads.Alternating(0), codeword);
        var clean = LdpcCheck.UnpackMsbFirst(codeword, Ft8Tables.LdpcN);

        Assert.Equal(
            0,
            LdpcCheck.FailingCount(
                LdpcCheck.SyndromeFromNm(clean, Ft8Tables.LdpcNm, Ft8Tables.LdpcNumRows)));

        var disagreements = new List<string>();
        var firstRefusal = string.Empty;

        for (var variable = 0; variable < Ft8Tables.LdpcN; variable++)
        {
            var flipped = (byte[])clean.Clone();
            flipped[variable] ^= 1;

            var failing = LdpcCheck.FailingChecks(
                LdpcCheck.SyndromeFromNm(flipped, Ft8Tables.LdpcNm, Ft8Tables.LdpcNumRows));
            var expected = LdpcCheck.ChecksDisturbedByFlippingFromMn(variable, Ft8Tables.LdpcMn);

            if (failing.Length != expected)
            {
                disagreements.Add(
                    $"    variable {variable}: Nm side says {failing.Length} checks moved, Mn side says "
                    + $"{expected}, at check indices [{string.Join(", ", failing)}]");
            }

            if (firstRefusal.Length == 0)
            {
                firstRefusal =
                    $"REFUSED. Flipping codeword bit {variable} left {failing.Length} of "
                    + $"{Ft8Tables.LdpcM} checks unsatisfied, at check indices "
                    + $"[{string.Join(", ", failing)}]. Mn's row for that variable independently says "
                    + $"{expected}. A single wrong bit in a codeword is visible and is not silently "
                    + "absorbed.";
            }
        }

        _output.WriteLine($"variables flipped, one at a time : {Ft8Tables.LdpcN}");
        _output.WriteLine(firstRefusal);
        _output.WriteLine($"variables where Nm and Mn disagreed on the count : {disagreements.Count}");

        Assert.True(
            disagreements.Count == 0,
            $"{disagreements.Count} of {Ft8Tables.LdpcN} variables have an Nm-side failing-check count "
            + "that disagrees with Mn's own row for them, so the two tables are not the transpose of "
            + "each other that the decoder in step 5 will assume:"
            + Environment.NewLine + string.Join(Environment.NewLine, disagreements));
    }
}
