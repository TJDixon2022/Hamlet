using Ft8Sharp;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Encode;

/// <summary>
/// What the encoder asserts about its own output: the length, the alphabet, and the three sync
/// blocks at the indices task 3 measured, each checked separately.
/// </summary>
/// <remarks>
/// <b>None of this is bit-identity with <c>ft8_lib</c> and none of it stands in for it.</b> Every
/// assertion here is this library agreeing with itself. A port that ran the Gray map backwards
/// would pass all of them: the sequence would be 79 symbols long, every value would be inside the
/// alphabet, and the sync blocks would be exactly where they belong, because none of that is
/// touched by the map's direction. Step 3's second exit criterion is what settles it and unit 209
/// could not take it — there is no C toolchain on this machine to build the reference generator
/// with. It is open, and this file says so rather than letting a column of green ticks imply
/// otherwise.
/// </remarks>
public class Ft8SymbolEncoderTests
{
    private readonly ITestOutputHelper _output;

    public Ft8SymbolEncoderTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void EveryMessageProducesExactlyTheSymbolsATransmissionSends()
    {
        var corpus = EncodeCorpus.Build();
        _output.WriteLine($"corpus: {corpus.Count} messages");

        foreach (var entry in corpus)
        {
            var symbols = Ft8SymbolEncoder.Encode(entry.Message);
            Assert.Equal(Ft8SymbolEncoder.SymbolCount, symbols.Length);
        }

        _output.WriteLine(
            $"every one produced {Ft8SymbolEncoder.SymbolCount} channel symbols");
    }

    [Fact]
    public void NoSymbolIsEverOutsideTheToneAlphabet()
    {
        var corpus = EncodeCorpus.Build();
        var checkedSymbols = 0;

        foreach (var entry in corpus)
        {
            var symbols = Ft8SymbolEncoder.Encode(entry.Message);
            for (var i = 0; i < symbols.Length; i++)
            {
                Assert.True(
                    symbols[i] < Ft8SymbolEncoder.ToneCount,
                    $"'{entry.Label}' produced a value at symbol {i} that is not a tone. FT8 has "
                    + $"{Ft8SymbolEncoder.ToneCount} tones and nothing else may reach a waveform.");
                checkedSymbols++;
            }
        }

        _output.WriteLine($"{checkedSymbols} symbols checked, all inside the alphabet");
        Assert.Equal(corpus.Count * Ft8SymbolEncoder.SymbolCount, checkedSymbols);
    }

    /// <summary>
    /// The first sync block, on its own. Three separate facts get three separate tests: a single
    /// loop over all three passes if two are right and reports one failure for the pair.
    /// </summary>
    [Fact]
    public void TheFirstSyncBlockIsTheCheckedInCostasPattern() => AssertSyncBlock(0);

    [Fact]
    public void TheSecondSyncBlockIsTheCheckedInCostasPattern() => AssertSyncBlock(1);

    [Fact]
    public void TheThirdSyncBlockIsTheCheckedInCostasPattern() => AssertSyncBlock(2);

    private void AssertSyncBlock(int block)
    {
        var start = Ft8SymbolEncoder.SyncBlockStart(block);
        var costas = Ft8Tables.Ft8CostasPattern;
        _output.WriteLine($"sync block {block} begins at symbol {start} and runs {costas.Length} symbols");

        foreach (var entry in EncodeCorpus.Build())
        {
            var symbols = Ft8SymbolEncoder.Encode(entry.Message);
            for (var i = 0; i < costas.Length; i++)
            {
                Assert.True(
                    symbols[start + i] == costas[i],
                    $"'{entry.Label}' does not carry the checked-in Costas pattern at symbol "
                    + $"{start + i}, which is position {i} of sync block {block}. The value is "
                    + "deliberately not in this message.");
            }
        }

        _output.WriteLine("every message in the corpus carries it, symbol for symbol");
    }

    /// <summary>
    /// The sync blocks and the data symbols between them account for the whole transmission, and
    /// the sync symbols are the same in every message while the data symbols are not.
    /// </summary>
    [Fact]
    public void TheSyncBlocksAreTheOnlyThingEveryMessageHasInCommon()
    {
        var corpus = EncodeCorpus.Build();
        var sequences = corpus.Select(e => Ft8SymbolEncoder.Encode(e.Message)).ToList();

        var syncPositions = Enumerable.Range(0, Ft8SymbolEncoder.SymbolCount)
            .Where(Ft8SymbolEncoder.IsSyncSymbol)
            .ToList();
        Assert.Equal(Ft8SymbolEncoder.SyncBlockCount * Ft8SymbolEncoder.SyncBlockLength, syncPositions.Count);
        Assert.Equal(
            Ft8SymbolEncoder.DataSymbolCount,
            Ft8SymbolEncoder.SymbolCount - syncPositions.Count);

        // Every sync position is identical across the corpus.
        foreach (var position in syncPositions)
        {
            Assert.True(
                sequences.All(s => s[position] == sequences[0][position]),
                $"symbol {position} is a sync position and is not the same in every message.");
        }

        // And the data positions are not all identical, or the codeword is not reaching them.
        var dataPositions = Enumerable.Range(0, Ft8SymbolEncoder.SymbolCount)
            .Where(i => !Ft8SymbolEncoder.IsSyncSymbol(i))
            .ToList();
        var varying = dataPositions.Count(p => sequences.Any(s => s[p] != sequences[0][p]));
        _output.WriteLine(
            $"{syncPositions.Count} sync positions identical across the corpus; "
            + $"{varying} of {dataPositions.Count} data positions vary");
        Assert.True(
            varying > 0,
            "no data position varies across the corpus, so the codeword is not reaching the "
            + "symbols and this encoder is emitting a constant with sync blocks in it.");
    }

    /// <summary>
    /// The same message encodes to the same symbols however many times it is asked, on whatever
    /// thread. Step 4 has <em>candidate ranking is stable across runs</em> waiting on this.
    /// </summary>
    [Fact]
    public void EncodingIsPureAndCarriesNoStateBetweenCalls()
    {
        var corpus = EncodeCorpus.Build();
        var first = corpus.Select(e => Ft8SymbolEncoder.Encode(e.Message)).ToList();

        // Interleaved and in a different order, which is what would shake a static buffer loose.
        for (var round = 0; round < 3; round++)
        {
            foreach (var (entry, index) in corpus.Select((e, i) => (e, i)).Reverse())
            {
                Assert.Equal(first[index], Ft8SymbolEncoder.Encode(entry.Message));
            }
        }

        var parallel = corpus
            .AsParallel()
            .Select((e, i) => (Index: i, Symbols: Ft8SymbolEncoder.Encode(e.Message)))
            .ToList();
        foreach (var (index, symbols) in parallel)
        {
            Assert.Equal(first[index], symbols);
        }

        _output.WriteLine($"{corpus.Count} messages, 4 sequential rounds and a parallel one, all identical");
    }

    // ---- what refuses, and that no partial sequence is ever returned ----

    [Fact]
    public void AWrongLengthMessageIsRefusedAndNothingIsWritten()
    {
        var symbols = Filled();
        var before = (byte[])symbols.Clone();

        foreach (var length in new[] { 0, Ft8Payload.MessageBytes - 1, Ft8Payload.MessageBytes + 1, 12 })
        {
            var message = new byte[length];
            var thrown = Assert.Throws<ArgumentException>(() => Ft8SymbolEncoder.Encode(message, symbols));
            Assert.Equal("message", thrown.ParamName);
        }

        Assert.Equal(before, symbols);
        _output.WriteLine("a wrong-length message refuses and the caller's buffer is untouched");
    }

    [Fact]
    public void AWrongLengthSymbolBufferIsRefusedAndNothingIsWritten()
    {
        var message = EncodeCorpus.Build()[0].Message;

        foreach (var length in new[] { 0, Ft8SymbolEncoder.SymbolCount - 1, Ft8SymbolEncoder.SymbolCount + 1 })
        {
            var symbols = new byte[length];
            Array.Fill(symbols, (byte)0xEE);
            var before = (byte[])symbols.Clone();

            var thrown = Assert.Throws<ArgumentException>(() => Ft8SymbolEncoder.Encode(message, symbols));
            Assert.Equal("symbols", thrown.ParamName);
            Assert.Equal(before, symbols);
        }

        _output.WriteLine("a wrong-length symbol buffer refuses and is left exactly as it arrived");
    }

    /// <summary>
    /// A message with bits set past its 77th refuses, and the refusal comes from
    /// <c>Ft8Payload.Create</c> rather than being caught and worked around here.
    /// </summary>
    [Fact]
    public void AMessageWithBitsPastItsLastIsRefusedAndNothingIsWritten()
    {
        var message = (byte[])EncodeCorpus.Build()[0].Message.Clone();
        var spare = (1 << ((Ft8Payload.MessageBytes * 8) - Ft8Payload.MessageBits)) - 1;
        message[^1] |= (byte)spare;

        var symbols = Filled();
        var before = (byte[])symbols.Clone();

        var thrown = Assert.Throws<ArgumentException>(() => Ft8SymbolEncoder.Encode(message, symbols));
        Assert.Equal("message", thrown.ParamName);
        Assert.Equal(before, symbols);

        _output.WriteLine(
            "a message with bits past its 77th refuses through Ft8Payload.Create, uncaught, and no "
            + "partial sequence is written");
    }

    /// <summary>
    /// A message that will not pack never reaches the encoder, and the packer is the thing that
    /// says so. This is the seam unit 207 built and it is still where it was.
    /// </summary>
    [Fact]
    public void AMessageThatWillNotPackNeverBecomesASequence()
    {
        var message = new byte[Ft8Payload.MessageBytes];

        // A non-standard callsign with no cache to hash it into: refused, and the buffer stays as
        // it was, so there is nothing for an encoder to be handed.
        var result = Ft8StandardMessage.TryPack("CQ", "PJ4/K1ABC", "FN42", null, message);
        Assert.NotEqual(Ft8PackResult.Ok, result);
        Assert.All(message, b => Assert.Equal(0, b));

        _output.WriteLine($"the packer refused with {result} and wrote nothing for an encoder to take");
    }

    [Fact]
    public void TheThreeSyncBlockStartsAreTheOnlyOnesThereAre()
    {
        for (var block = 0; block < Ft8SymbolEncoder.SyncBlockCount; block++)
        {
            Assert.Equal(block * Ft8SymbolEncoder.SyncBlockOffset, Ft8SymbolEncoder.SyncBlockStart(block));
        }

        Assert.Throws<ArgumentOutOfRangeException>(() => Ft8SymbolEncoder.SyncBlockStart(-1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Ft8SymbolEncoder.SyncBlockStart(Ft8SymbolEncoder.SyncBlockCount));

        // The last sync block ends exactly at the end of the transmission.
        Assert.Equal(
            Ft8SymbolEncoder.SymbolCount,
            Ft8SymbolEncoder.SyncBlockStart(Ft8SymbolEncoder.SyncBlockCount - 1) + Ft8SymbolEncoder.SyncBlockLength);
        _output.WriteLine("the third sync block ends exactly at the end of the transmission");
    }

    private static byte[] Filled()
    {
        var symbols = new byte[Ft8SymbolEncoder.SymbolCount];
        Array.Fill(symbols, (byte)0xEE);
        return symbols;
    }
}
