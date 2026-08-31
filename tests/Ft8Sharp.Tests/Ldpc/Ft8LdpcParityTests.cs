using Ft8Sharp;
using Ft8Sharp.Ldpc;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Ldpc;

/// <summary>
/// Proves that the generator table and the parity-check tables describe one and the same
/// code, which is what step 1's fifth exit criterion asks and the reason this unit exists.
/// </summary>
/// <remarks>
/// <para>
/// <b>The whole code space, not a sample.</b> The FT8 LDPC(174,91) code is linear over
/// GF(2): every payload is a sum of the 91 weight-one payloads, and the syndrome of a sum
/// is the sum of the syndromes. So if each of the 91 basis payloads encodes to a codeword
/// whose syndrome is zero on all 83 checks, then so does every one of the 2^91 codewords
/// the generator can produce. That is a proof, not an agreement count -- a compiled
/// reference encoder would only ever have given agreement on as many vectors as one had
/// patience for.
/// </para>
/// <para>
/// <b>Why there is no compiled C oracle.</b> There is no gcc, cc, cl, make or cmake on this
/// machine, and the sanctioned tooling is dotnet build, test and restore. Installing a
/// toolchain is the owner's call. It is also unnecessary: the argument above is stronger
/// than anything an oracle could have supplied.
/// </para>
/// <para>
/// <b>The fixed and random payloads are not there for coverage</b> -- linearity already
/// covers them. They are there to catch an encoder whose indexing depends on the payload,
/// which is not a linear fault and which the basis vectors alone could miss.
/// </para>
/// <para>
/// <b>These tests read the checked-in tables and never skip.</b> No clone, no FT8_LIB_PATH,
/// no reference material: what ships is asserted sound on a machine that has never seen
/// ft8_lib.
/// </para>
/// </remarks>
public class Ft8LdpcParityTests
{
    /// <summary>Fixed so a failure is reproducible rather than a story about one run.</summary>
    private const int RandomPayloadSeed = 20260831;

    /// <summary>Comfortably more than the 200 the unit asks for; the whole run costs milliseconds.</summary>
    private const int RandomPayloadCount = 500;

    private readonly ITestOutputHelper _output;

    public Ft8LdpcParityTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// THE PROOF. All 91 weight-one payloads encode to codewords that satisfy all 83 checks,
    /// and by linearity that settles every payload the code can carry.
    /// </summary>
    [Fact]
    public void AllNinetyOneBasisPayloadsSatisfyEveryCheckSoByLinearityEveryCodewordDoes()
    {
        var syndromeBits = 0;
        var failures = new List<string>();

        for (var bit = 0; bit < LdpcEncoder.PayloadBits; bit++)
        {
            var syndrome = SyndromeOf(Payloads.Basis(bit));
            syndromeBits += syndrome.Length;

            var failing = LdpcCheck.FailingChecks(syndrome);
            if (failing.Length > 0)
            {
                failures.Add(
                    $"payload bit {bit}: {failing.Length} of {Ft8Tables.LdpcM} checks failed, "
                    + $"at check indices [{string.Join(", ", failing)}]");
            }
        }

        _output.WriteLine(
            $"{LdpcEncoder.PayloadBits} payloads x {Ft8Tables.LdpcM} checks = {syndromeBits} syndrome bits, "
            + $"{(failures.Count == 0 ? "all zero" : "NOT all zero")}");
        _output.WriteLine(
            "the code is linear over GF(2), so 91 zero syndromes cover all 2^91 codewords");

        Assert.True(
            failures.Count == 0,
            $"{failures.Count} of the {LdpcEncoder.PayloadBits} basis payloads encoded to a codeword the "
            + "reference parity tables refuse, so kFTX_LDPC_generator and kFTX_LDPC_Nm are not "
            + "descriptions of the same code as they sit in this tree:"
            + Environment.NewLine + string.Join(Environment.NewLine, failures));

        Assert.Equal(LdpcEncoder.PayloadBits * Ft8Tables.LdpcM, syndromeBits);
    }

    /// <summary>
    /// The all-zero payload encodes to all-zero parity. Trivial, and it is what catches a
    /// checker that returns zero for everything.
    /// </summary>
    [Fact]
    public void TheAllZeroPayloadEncodesToAllZeroParity()
    {
        var codeword = new byte[LdpcEncoder.CodewordBytes];
        LdpcEncoder.Encode(new byte[LdpcEncoder.PayloadBytes], codeword);

        var bits = LdpcCheck.UnpackMsbFirst(codeword, Ft8Tables.LdpcN);
        var parityWeight = ParityWeight(bits);

        _output.WriteLine($"parity bits set for the all-zero payload: {parityWeight} of {Ft8Tables.LdpcM}");

        Assert.True(
            parityWeight == 0,
            $"The all-zero payload produced {parityWeight} set parity bits and must produce none.");
    }

    /// <summary>
    /// Every weight-one payload produces non-zero parity.
    /// </summary>
    /// <remarks>
    /// An all-zero column of the generator would satisfy every syndrome check and be
    /// silently wrong -- it would mean one payload bit contributed to no parity at all and
    /// was therefore unprotected. Only this assertion refuses it.
    /// </remarks>
    [Fact]
    public void EveryBasisPayloadProducesNonZeroParitySoNoPayloadBitIsUnprotected()
    {
        var deadColumns = new List<int>();
        var lightest = int.MaxValue;

        for (var bit = 0; bit < LdpcEncoder.PayloadBits; bit++)
        {
            var codeword = new byte[LdpcEncoder.CodewordBytes];
            LdpcEncoder.Encode(Payloads.Basis(bit), codeword);

            var weight = ParityWeight(LdpcCheck.UnpackMsbFirst(codeword, Ft8Tables.LdpcN));
            lightest = Math.Min(lightest, weight);
            if (weight == 0)
            {
                deadColumns.Add(bit);
            }
        }

        // The lightest weight only, never the profile: 91 per-column weights would be a
        // characterisation of the generator by another route.
        _output.WriteLine($"basis payloads with no parity at all : {deadColumns.Count}");
        _output.WriteLine($"lightest parity weight seen          : {lightest} of {Ft8Tables.LdpcM}");

        Assert.True(
            deadColumns.Count == 0,
            $"{deadColumns.Count} payload bits contribute to no parity bit, at bit indices "
            + $"[{string.Join(", ", deadColumns)}]. Those bits are carried by the code and protected "
            + "by none of it, and every syndrome test would still pass.");
    }

    /// <summary>
    /// Fixed patterns and seeded random payloads, to catch an encoder whose indexing depends
    /// on the payload -- a fault linearity cannot see.
    /// </summary>
    [Fact]
    public void FixedAndSeededRandomPayloadsAlsoSatisfyEveryCheck()
    {
        var fixedPayloads = new[]
        {
            ("all zero", new byte[LdpcEncoder.PayloadBytes]),
            ("all ones", Payloads.AllOnes()),
            ("alternating from 0", Payloads.Alternating(0)),
            ("alternating from 1", Payloads.Alternating(1)),
            ("first bit only", Payloads.Basis(0)),
            ("last bit only", Payloads.Basis(LdpcEncoder.PayloadBits - 1)),
            ("byte-boundary bit 87", Payloads.Basis(87)),
            ("byte-boundary bit 88", Payloads.Basis(88)),
        };

        var failures = new List<string>();

        foreach (var (name, payload) in fixedPayloads)
        {
            var failing = LdpcCheck.FailingChecks(SyndromeOf(payload));
            if (failing.Length > 0)
            {
                failures.Add($"fixed payload '{name}': {failing.Length} checks failed, "
                    + $"at [{string.Join(", ", failing)}]");
            }
        }

        var random = new Random(RandomPayloadSeed);
        for (var i = 0; i < RandomPayloadCount; i++)
        {
            var failing = LdpcCheck.FailingChecks(SyndromeOf(Payloads.Random(random)));
            if (failing.Length > 0)
            {
                failures.Add($"random payload {i}: {failing.Length} checks failed, "
                    + $"at [{string.Join(", ", failing)}]");
            }
        }

        _output.WriteLine($"fixed payloads       : {fixedPayloads.Length}, all checks satisfied: {failures.Count == 0}");
        _output.WriteLine($"random payloads      : {RandomPayloadCount}, seed {RandomPayloadSeed}");
        _output.WriteLine($"payloads that failed : {failures.Count}");

        Assert.True(
            failures.Count == 0,
            $"{failures.Count} payloads encoded to codewords the reference parity tables refuse. "
            + $"Linearity says this cannot happen if the basis passed, so an encoder whose indexing "
            + $"depends on the payload is the likely shape. Seed {RandomPayloadSeed}:"
            + Environment.NewLine + string.Join(Environment.NewLine, failures));
    }

    /// <summary>Encodes, then checks from the tables alone. Returns one bit per check.</summary>
    private static byte[] SyndromeOf(byte[] payload)
    {
        var codeword = new byte[LdpcEncoder.CodewordBytes];
        LdpcEncoder.Encode(payload, codeword);
        return LdpcCheck.SyndromeFromNm(
            LdpcCheck.UnpackMsbFirst(codeword, Ft8Tables.LdpcN),
            Ft8Tables.LdpcNm,
            Ft8Tables.LdpcNumRows);
    }

    /// <summary>How many of the 83 parity bits are set. A count, never the bits.</summary>
    private static int ParityWeight(ReadOnlySpan<byte> codewordBits)
    {
        var weight = 0;
        for (var i = LdpcEncoder.PayloadBits; i < Ft8Tables.LdpcN; i++)
        {
            weight += codewordBits[i];
        }

        return weight;
    }
}
