using Ft8Sharp.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Encode;

/// <summary>
/// Unit 289 task 3 — <b>the comparison that makes the rest of the unit evidence.</b> Every symbol of
/// every corpus message with a text form, against the tones <c>gen_ft8 -ft4</c> prints for the same
/// message.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this runs before the decoder is written.</b> Step 1's exit criteria are a round trip:
/// Hamlet's FT4 encoder into Hamlet's FT4 decoder. On its own that is self-consistent by
/// construction — a wrong Gray map, a wrong Costas row, a wrong exclusive-OR byte or a wrong ramp
/// position would be applied by the encoder and undone by the decoder, and the message would come
/// back perfect. Every other FT4 test available in this tree shares its constants with the thing it
/// is testing. <c>gen_ft8 -ft4</c> is the only witness in the building that does not.
/// </para>
/// <para>
/// <b>Beside <c>EverySymbolOfEveryMessageIsIdenticalToUpstreams</c>, not instead of it.</b> That one
/// is FT8's and is not generalised: an FT8 regression has to stay attributable to the FT8 side.
/// </para>
/// <para>
/// <b>The corpus does not change.</b> It is a message-layer corpus and FT8 and FT4 share their whole
/// message layer — the same 77 bits, the same CRC-14 and the same LDPC(174,91) code. What differs is
/// applied after the corpus and before the tones.
/// </para>
/// <para>
/// <b>Nothing upstream produces is committed.</b> The tones are read at run time, compared, and
/// dropped. Whether they matched is recorded; what they were is not.
/// </para>
/// </remarks>
public class Ft4SymbolBitIdentityTests
{
    private readonly Unit289Report _output;

    public Ft4SymbolBitIdentityTests(ITestOutputHelper output) =>
        _output = new Unit289Report("task3-upstream-symbols", output);

    /// <summary>
    /// Every symbol of every message this library can put into words, against upstream's own FT4
    /// tones.
    /// </summary>
    [RequiresWorkingOracleFact]
    public void EverySymbolOfEveryMessageIsIdenticalToUpstreamsFt4()
    {
        var corpus = EncodeCorpus.Build();
        var comparable = corpus.Where(entry => entry.Text is not null).ToList();

        _output.WriteLine($"corpus            : {corpus.Count} messages");
        _output.WriteLine($"with a text form  : {comparable.Count}");
        _output.WriteLine($"telemetry omitted : {corpus.Count - comparable.Count} (no text form exists)");
        _output.WriteLine($"symbols compared  : {Ft4SymbolEncoder.SymbolCount} per message");
        _output.WriteLine(string.Empty);

        var compared = 0;
        var matching = 0;
        var failures = new List<string>();

        foreach (var entry in comparable)
        {
            var run = Ft8Oracle.Generate(entry.Text!, Ft8Oracle.Protocol.Ft4);
            if (run.ExitCode != 0)
            {
                failures.Add(
                    $"[{entry.Label}] upstream exited {run.ExitCode} (0x{run.ExitCode:X8}) rather "
                    + "than encoding the message as FT4");
                continue;
            }

            if (!Ft8Oracle.TryReadTones(
                    run.StandardOutput, Ft8Oracle.Ft4ToneSequenceLength, out var theirs))
            {
                failures.Add(
                    $"[{entry.Label}] upstream ran and printed no FT4 tone sequence this parser "
                    + "could read");
                continue;
            }

            var ours = Ft4SymbolEncoder.Encode(entry.Message);
            var result = Ft4SymbolComparison.Compare(ours, theirs);
            compared++;

            if (result.Identical)
            {
                matching++;
                _output.WriteLine($"  match   [{entry.Kind}] {entry.Label}: {result.Compared} symbols");
                continue;
            }

            _output.WriteLine($"  DIFFER  [{entry.Kind}] {entry.Label}: {result.Explanation}");
            failures.Add($"[{entry.Label}] ({entry.Kind}): {result.Explanation}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"messages compared : {compared}");
        _output.WriteLine($"matching symbol for symbol: {matching}");

        Assert.True(
            failures.Count == 0,
            "the FT4 symbol sequence is not bit-identical to ft8_lib's:"
            + Environment.NewLine
            + string.Join(Environment.NewLine, failures));

        Assert.Equal(comparable.Count, compared);
        Assert.Equal(compared, matching);
        Assert.True(compared > 0, "nothing was compared, so nothing is proved");
    }

    /// <summary>
    /// The FT4 comparison watched refusing, and it needs no oracle. A comparison that has never
    /// failed is not a comparison.
    /// </summary>
    /// <remarks>
    /// Three positions are altered in three separate runs — a ramp, a sync symbol and a data symbol —
    /// because those are three different faults and the locator is required to name which one a
    /// reader is looking at rather than to report a count.
    /// </remarks>
    [Fact]
    public void TheFt4ComparisonNamesWhatKindOfPositionWasAltered()
    {
        var entry = EncodeCorpus.Build().First();
        var ours = Ft4SymbolEncoder.Encode(entry.Message);

        Assert.Equal(Ft4SymbolEncoder.SymbolCount, ours.Length);

        // A ramp.
        Assert.True(Ft4SymbolEncoder.IsRampSymbol(104));
        Assert.Contains("ramp symbol", Alter(ours, 104).Explanation, StringComparison.Ordinal);

        // Inside the third sync group, which is the one a port repeating row 0 would get wrong.
        Assert.True(Ft4SymbolEncoder.TrySyncPosition(68, out var group, out _));
        Assert.Equal(2, group);
        var syncResult = Alter(ours, 68);
        Assert.Contains("sync group 2", syncResult.Explanation, StringComparison.Ordinal);
        Assert.Equal(68, syncResult.FirstDifference);

        // A data symbol.
        Assert.True(Ft4SymbolEncoder.IsDataSymbol(5));
        var dataResult = Alter(ours, 5);
        Assert.Contains("data symbol", dataResult.Explanation, StringComparison.Ordinal);
        Assert.Equal(1, dataResult.DifferenceCount);

        // And it agrees when it should, so the three refusals are not a comparator refusing
        // everything.
        var again = Ft4SymbolEncoder.Encode(entry.Message);
        var agreement = Ft4SymbolComparison.Compare(ours, again);
        Assert.True(agreement.Identical);
        Assert.Equal(Ft4SymbolEncoder.SymbolCount, agreement.Compared);

        // A sequence of the wrong length is refused rather than compared over the shorter of the
        // two, which would report agreement over a prefix as agreement.
        Assert.False(Ft4SymbolComparison.Compare(ours, ours[..^1]).Identical);
        Assert.Equal(0, Ft4SymbolComparison.Compare(ours, ours[..^1]).Compared);

        _output.WriteLine("the FT4 comparator names a ramp, a sync group row and a data symbol, "
            + "refuses a truncated sequence, and agrees with an unaltered one");
    }

    private static SymbolComparison.Result Alter(byte[] ours, int position)
    {
        var theirs = (byte[])ours.Clone();
        theirs[position] = (byte)((theirs[position] + 1) % Ft4SymbolEncoder.ToneCount);
        return Ft4SymbolComparison.Compare(ours, theirs);
    }

    /// <summary>
    /// The layout itself, held against upstream's own literals rather than against the macro they
    /// were derived from.
    /// </summary>
    /// <remarks>
    /// <c>ft4_encode</c> writes the four sync groups as literal ranges — 1 to 4, 34 to 37, 67 to 70,
    /// 100 to 103 — and the ramps as <c>i_tone == 0</c> and <c>i_tone == 104</c>. This library
    /// derives all of that from <c>FT4_SYNC_OFFSET</c> and <c>FT4_NN</c>, so the derivation is
    /// checked against the literals rather than believed.
    /// </remarks>
    [Fact]
    public void TheLayoutMatchesUpstreamsOwnLiteralRanges()
    {
        Assert.Equal(1, Ft4SymbolEncoder.SyncGroupStart(0));
        Assert.Equal(34, Ft4SymbolEncoder.SyncGroupStart(1));
        Assert.Equal(67, Ft4SymbolEncoder.SyncGroupStart(2));
        Assert.Equal(100, Ft4SymbolEncoder.SyncGroupStart(3));

        Assert.True(Ft4SymbolEncoder.IsRampSymbol(0));
        Assert.True(Ft4SymbolEncoder.IsRampSymbol(104));
        Assert.False(Ft4SymbolEncoder.IsRampSymbol(1));
        Assert.False(Ft4SymbolEncoder.IsRampSymbol(103));

        var data = 0;
        var sync = 0;
        var ramps = 0;
        for (var i = 0; i < Ft4SymbolEncoder.SymbolCount; i++)
        {
            if (Ft4SymbolEncoder.IsRampSymbol(i))
            {
                ramps++;
            }
            else if (Ft4SymbolEncoder.TrySyncPosition(i, out _, out _))
            {
                sync++;
            }
            else
            {
                data++;
            }
        }

        Assert.Equal(Ft4SymbolEncoder.RampSymbolCount, ramps);
        Assert.Equal(Ft4SymbolEncoder.SyncGroupCount * Ft4SymbolEncoder.SyncGroupLength, sync);
        Assert.Equal(Ft4SymbolEncoder.DataSymbolCount, data);
        Assert.Equal(Ft4SymbolEncoder.SymbolCount, ramps + sync + data);

        // 87 data symbols of 2 bits is 174 bits, which is the whole codeword and no padding.
        Assert.Equal(174, Ft4SymbolEncoder.DataSymbolCount * Ft4SymbolEncoder.BitsPerSymbol);
    }

    /// <summary>
    /// The payload exclusive-OR is its own inverse and it leaves the message's spare bits alone.
    /// </summary>
    /// <remarks>
    /// <b>The breakage this catches</b> is the one that would look like a working chain: applying the
    /// sequence at both ends and getting the message back would still be a round trip, but the CRC
    /// and the parity would have been computed over the wrong bits and no station on the band would
    /// read it. It is caught by the upstream comparison above; this is the cheap direct statement of
    /// the two properties that comparison depends on.
    /// </remarks>
    [Fact]
    public void ThePayloadExclusiveOrIsItsOwnInverseAndLeavesTheSpareBitsClear()
    {
        foreach (var entry in EncodeCorpus.Build())
        {
            var scrambled = new byte[Ft4SymbolEncoder.MessageBytes];
            var back = new byte[Ft4SymbolEncoder.MessageBytes];

            Ft4SymbolEncoder.ApplyPayloadXor(entry.Message, scrambled);
            Ft4SymbolEncoder.ApplyPayloadXor(scrambled, back);

            Assert.Equal(entry.Message, back);
            Assert.NotEqual(entry.Message, scrambled);

            // The three bits past the message's 77th must stay clear, or Ft8Payload.Create refuses
            // the scrambled message and FT4 could never be encoded at all.
            Assert.Equal(0, scrambled[^1] & 0x07);
        }
    }
}
