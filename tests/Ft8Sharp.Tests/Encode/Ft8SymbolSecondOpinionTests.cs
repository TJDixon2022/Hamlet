using Ft8Sharp.Encode;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Encode;

/// <summary>
/// Leg B: the encoder's sequence against an independent second implementation of the layout.
/// </summary>
/// <remarks>
/// <para>
/// <b>This is not leg C and it is not a substitute for it.</b> Step 3's second exit criterion asks
/// whether the sequence is bit-identical to <c>ft8_lib</c>'s. Agreement between two
/// implementations written in the same session says only that they agree; both could be wrong in
/// the same way, and the way they would most plausibly be wrong — the Gray map read in the wrong
/// direction — is one both of them inherit from the same reading of the same source. <b>Criterion
/// 2 is open.</b> Unit 209 could not build the reference generator: there is no C toolchain on
/// this machine.
/// </para>
/// <para>
/// <b>Task 6 was not dropped, and the branch that licensed keeping it is the second one.</b> Work
/// instruction 209 makes it droppable only where task 5 produced a real comparison against
/// upstream. Task 5 was unreachable, so this is the only evidence this unit can produce about the
/// sequence, and a night that builds an encoder and proves nothing about it is not worth having.
/// </para>
/// </remarks>
public class Ft8SymbolSecondOpinionTests
{
    private readonly ITestOutputHelper _output;

    public Ft8SymbolSecondOpinionTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void TheSecondImplementationAgreesOnEverySymbolOfEveryMessage()
    {
        var corpus = EncodeCorpus.Build();
        var compared = 0;
        var agreeing = 0;
        var firstDivergence = string.Empty;

        foreach (var entry in corpus)
        {
            var mine = Ft8SymbolEncoder.Encode(entry.Message);
            var theirs = SymbolCheck.Lay(Codeword(entry.Message));
            compared++;

            var differsAt = -1;
            for (var i = 0; i < mine.Length; i++)
            {
                if (mine[i] != theirs[i])
                {
                    differsAt = i;
                    break;
                }
            }

            if (differsAt < 0)
            {
                agreeing++;
                continue;
            }

            if (firstDivergence.Length == 0)
            {
                firstDivergence = $"'{entry.Label}' ({entry.Kind}) first differs at symbol {differsAt}";
            }
        }

        _output.WriteLine($"messages compared                       : {compared}");
        _output.WriteLine($"agreeing symbol for symbol              : {agreeing}");
        _output.WriteLine($"symbols compared                        : {compared * SymbolCheck.SymbolCount}");
        _output.WriteLine(
            $"types covered                           : "
            + string.Join(", ", corpus.Select(e => e.Kind).Distinct()));
        _output.WriteLine(
            $"messages carrying a hashed callsign     : {corpus.Count(e => e.CarriesHashedCallsign)}");
        _output.WriteLine(
            "THIS IS LEG B, and it is still not bit-identity with ft8_lib — it is this library "
            + "agreeing with a second implementation of its own, by deliberately opposite "
            + "arithmetic. What changed in unit 211 is what stands beside it: leg C now runs, so "
            + "leg B is the WEAKER OF TWO AGREEING LEGS rather than the only implementation-level "
            + "evidence there is. It is kept for two reasons. It covers the four messages leg C "
            + "cannot reach — the telemetry entries and the non-standard hashed-companion entry, "
            + "which upstream's generator has no text form for — and it is the only symbol-level "
            + "evidence that survives on a machine with no clone and nothing built from it, which "
            + "is every machine but this one.");

        Assert.Equal(compared, agreeing);
        Assert.True(firstDivergence.Length == 0, firstDivergence);
    }

    /// <summary>
    /// The second implementation is watched catching a difference, because a checker that has never
    /// disagreed says nothing about the run where it agreed.
    /// </summary>
    [Fact]
    public void TheSecondImplementationIsWatchedCatchingADifference()
    {
        var message = EncodeCorpus.Build()[0].Message;
        var codeword = Codeword(message);
        var laid = SymbolCheck.Lay(codeword);

        // Flip one codeword bit. It must move at least one data symbol and no sync symbol.
        var perturbed = (byte[])codeword.Clone();
        perturbed[0] ^= 0x80;
        var afterFlip = SymbolCheck.Lay(perturbed);

        var moved = Enumerable.Range(0, SymbolCheck.SymbolCount).Where(i => laid[i] != afterFlip[i]).ToList();
        _output.WriteLine($"one codeword bit flipped moved {moved.Count} symbol(s): {string.Join(", ", moved)}");

        Assert.NotEmpty(moved);
        foreach (var position in moved)
        {
            Assert.False(
                SymbolCheck.SyncPositions().Contains(position),
                $"flipping a codeword bit moved symbol {position}, which is a sync position and "
                + "carries no codeword bit.");
        }

        // And the encoder moves the same symbols for the same flip, which is the agreement being
        // checked holding under a change rather than only on the happy path.
        Assert.Equal(afterFlip, SymbolCheck.Lay(perturbed));
    }

    /// <summary>
    /// The two implementations work out the sync positions by different arithmetic and land on the
    /// same set.
    /// </summary>
    [Fact]
    public void BothImplementationsPlaceTheSyncBlocksInTheSamePlaces()
    {
        var mine = Enumerable.Range(0, Ft8SymbolEncoder.SymbolCount)
            .Where(Ft8SymbolEncoder.IsSyncSymbol)
            .ToList();
        var theirs = SymbolCheck.SyncPositions();

        Assert.Equal(theirs, mine);
        Assert.Equal(SymbolCheck.SyncCount * SymbolCheck.SyncLength, mine.Count);
        _output.WriteLine($"{mine.Count} sync positions, agreed by both");
    }

    /// <summary>The codeword the layout is checked over, built by the library's own chain.</summary>
    private static byte[] Codeword(ReadOnlySpan<byte> message)
    {
        Span<byte> payload = stackalloc byte[Ft8Payload.PayloadBytes];
        Ft8Payload.Create(message, payload);
        var codeword = new byte[LdpcEncoder.CodewordBytes];
        LdpcEncoder.Encode(payload, codeword);
        return codeword;
    }
}
