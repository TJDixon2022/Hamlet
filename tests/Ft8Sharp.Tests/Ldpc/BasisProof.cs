using Ft8Sharp;
using Ft8Sharp.Ldpc;

namespace Ft8Sharp.Tests.Ldpc;

/// <summary>
/// The basis-vector parity proof, in one place so that the tests which watch it refuse are
/// watching the same routine that passes on the real tables.
/// </summary>
/// <remarks>
/// A corruption test that composed its own failure message would be reporting its own
/// opinion of what the guard would have said. This way the refusal quoted in the report is
/// the guard's own words, produced by the same code path that clears the shipped tables.
/// </remarks>
internal static class BasisProof
{
    /// <summary>What the proof found. Counts and indices only, never a value.</summary>
    internal sealed record Result(int PayloadsFailing, int ChecksFailing, int SyndromeBits, string Refusal)
    {
        public bool IsClean => PayloadsFailing == 0;
    }

    /// <summary>
    /// Encodes all <see cref="LdpcEncoder.PayloadBits"/> weight-one payloads through
    /// <paramref name="generator"/> and checks each resulting codeword against
    /// <paramref name="nm"/>.
    /// </summary>
    public static Result Run(ReadOnlySpan<byte> generator, ReadOnlySpan<byte> nm, ReadOnlySpan<byte> numRows)
    {
        var detail = new List<string>();
        var payloadsFailing = 0;
        var checksFailing = 0;
        var syndromeBits = 0;

        var codeword = new byte[LdpcEncoder.CodewordBytes];
        for (var bit = 0; bit < LdpcEncoder.PayloadBits; bit++)
        {
            LdpcEncoder.Encode(generator, Payloads.Basis(bit), codeword);
            var syndrome = LdpcCheck.SyndromeFromNm(
                LdpcCheck.UnpackMsbFirst(codeword, Ft8Tables.LdpcN), nm, numRows);
            syndromeBits += syndrome.Length;

            var failing = LdpcCheck.FailingChecks(syndrome);
            if (failing.Length == 0)
            {
                continue;
            }

            payloadsFailing++;
            checksFailing += failing.Length;

            // Enough to diagnose with and no more. Ten lines names the shape of the fault;
            // ninety-one would be a transcript nobody reads.
            if (detail.Count < 10)
            {
                detail.Add(
                    $"    payload bit {bit,2}: {failing.Length,2} of {Ft8Tables.LdpcM} checks failed, "
                    + $"at check indices [{string.Join(", ", failing)}]");
            }
        }

        if (payloadsFailing == 0)
        {
            return new Result(0, 0, syndromeBits, string.Empty);
        }

        if (payloadsFailing > detail.Count)
        {
            detail.Add($"    ... and {payloadsFailing - detail.Count} more payloads, not listed");
        }

        var refusal =
            $"REFUSED. {payloadsFailing} of {LdpcEncoder.PayloadBits} basis payloads encoded to a codeword "
            + $"the parity tables reject, {checksFailing} failing checks in all out of {syndromeBits} "
            + "syndrome bits." + Environment.NewLine
            + "kFTX_LDPC_generator and kFTX_LDPC_Nm are not descriptions of the same code as they stand "
            + "here. Because the code is linear over GF(2), a single basis payload failing means codewords "
            + "throughout the space fail, and a decoder built on these tables would go wrong in ways "
            + "nearly impossible to attribute." + Environment.NewLine
            + "No table value is printed below, by ruling -- a parity vector from a weight-one payload is "
            + "a column of the generator matrix wearing a different hat." + Environment.NewLine
            + string.Join(Environment.NewLine, detail);

        return new Result(payloadsFailing, checksFailing, syndromeBits, refusal);
    }
}
