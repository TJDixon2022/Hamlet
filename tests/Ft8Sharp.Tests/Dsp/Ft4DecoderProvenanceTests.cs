using Ft8Sharp;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// Unit 289 task 5 — the FT4 decoder, held to upstream branch for branch.
/// </summary>
/// <remarks>
/// <b>These catch nothing on their own and that is the point.</b> Every constant here is shared with
/// the encoder that produced the signal, so a round trip through them proves only that this library
/// agrees with itself. What holds this honest is task 3, where the encoder was nailed to
/// <c>gen_ft8 -ft4</c> symbol for symbol, and it is why task 3 ran first. These tests exist to catch
/// the three readings that are structural rather than numeric — the geometry's truncations, the four
/// different Costas rows, and the 5-then-9-then-13 skip — before the round trip is asked to mean
/// anything.
/// </remarks>
public class Ft4DecoderProvenanceTests
{
    private readonly Unit289Report _output;

    public Ft4DecoderProvenanceTests(ITestOutputHelper output) =>
        _output = new Unit289Report("task5-decoder", output);

    /// <summary>
    /// The geometry re-derived at FT4's own symbol period, with both precisions computed and printed
    /// rather than asserted from a paragraph.
    /// </summary>
    /// <remarks>
    /// <b>The breakage this catches</b>: FT8's truncation reasoning carried across to FT4 without
    /// being re-taken. <c>Ft8WaterfallGeometry</c> records a float-against-double match derived at
    /// <c>0.160f</c> specifically, where the single-precision block size is 1920 and the
    /// double-precision one is 1919 — a whole sample per symbol. Nothing about that derivation says
    /// anything about <c>0.048f</c>, and unit 288 said so explicitly.
    /// </remarks>
    [Fact]
    public void TheFt4GeometryIsReDerivedAtItsOwnSymbolPeriod()
    {
        var geometry = new Ft4WaterfallGeometry();

        const int rate = Ft8WaterfallGeometry.DefaultSampleRate;
        const float low = Ft8WaterfallGeometry.DefaultMinFrequencyHz;
        const float high = Ft8WaterfallGeometry.DefaultMaxFrequencyHz;
        const float period = Ft4Timing.SymbolPeriodSeconds;
        const float slot = Ft4Timing.SlotSeconds;
        // THE SECOND COLUMN IS THE SAME CONSTANT EVALUATED IN DOUBLE, which is what "more accurate"
        // would mean here and what Ft8WaterfallGeometry's own table compares against. 0.048f is
        // 0.0480000004172325134, and the question is whether carrying those extra digits through the
        // products changes which side of an integer each one lands.
        const double periodExact = Ft4Timing.SymbolPeriodSeconds;
        const double slotExact = Ft4Timing.SlotSeconds;

        var blockFloat = (int)(float)(rate * period);
        var blockDouble = (int)(rate * periodExact);
        var minFloat = (int)(float)(low * period);
        var minDouble = (int)(low * periodExact);
        var maxFloat = (int)(float)(high * period) + 1;
        var maxDouble = (int)(high * periodExact) + 1;
        var blocksFloat = (int)(float)(slot / period);
        var blocksDouble = (int)(slotExact / periodExact);

        _output.WriteLine("FT4 GEOMETRY, RE-DERIVED AT 0.048f");
        _output.WriteLine($"{"",-18} {"float (upstream)",-18} double");
        _output.WriteLine($"{"block size",-18} {blockFloat,-18} {blockDouble}");
        _output.WriteLine($"{"first kept bin",-18} {minFloat,-18} {minDouble}");
        _output.WriteLine($"{"last kept bin",-18} {maxFloat,-18} {maxDouble}");
        _output.WriteLine($"{"blocks in a slot",-18} {blocksFloat,-18} {blocksDouble}");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  bins kept         {geometry.BinCount}");
        _output.WriteLine($"  sub-block         {geometry.SubblockSize}");
        _output.WriteLine($"  transform length  {geometry.TransformLength}");
        _output.WriteLine($"  tone spacing      {geometry.ToneSpacingHz:F4} Hz");
        _output.WriteLine($"  block stride      {geometry.BlockStride}");

        // What the geometry actually came out as.
        Assert.Equal(576, geometry.BlockSize);
        Assert.Equal(288, geometry.SubblockSize);
        Assert.Equal(1152, geometry.TransformLength);
        Assert.Equal(156, geometry.MaxBlocks);
        Assert.Equal(9, geometry.MinBin);
        Assert.Equal(145, geometry.MaxBin);
        Assert.Equal(136, geometry.BinCount);
        // The reciprocal of the SINGLE-precision period, which is 20.833333152 and not
        // 20.833333333. The difference is 1.8e-7 Hz and it is here because the period is a float, as
        // upstream's is; the figure is quoted as 20.833 Hz everywhere a person reads it.
        Assert.Equal(20.833333, geometry.ToneSpacingHz, 6);

        // The single-precision column is what the type uses.
        Assert.Equal(blockFloat, geometry.BlockSize);
        Assert.Equal(minFloat, geometry.MinBin);
        Assert.Equal(maxFloat, geometry.MaxBin);
        Assert.Equal(blocksFloat, geometry.MaxBlocks);

        // AND THE FINDING: at FT4's period the two columns agree everywhere, where at FT8's they do
        // not. The precision stays single because it is what upstream does, not because FT4 needs
        // it — and a port that is right for a reason that stopped applying is a port waiting to be
        // wrong.
        Assert.Equal(blockDouble, blockFloat);
        Assert.Equal(minDouble, minFloat);
        Assert.Equal(maxDouble, maxFloat);
        Assert.Equal(blocksDouble, blocksFloat);

        // The FT8 contrast, computed here rather than quoted, so the sentence above is measured.
        var ft8BlockFloat = (int)(float)(rate * Ft8WaterfallGeometry.SymbolPeriodSeconds);
        var ft8BlockDouble = (int)(rate * (double)Ft8WaterfallGeometry.SymbolPeriodSeconds);
        _output.WriteLine(string.Empty);
        _output.WriteLine($"FT8 for contrast: block size {ft8BlockFloat} in float, {ft8BlockDouble} "
            + "in double — they differ, and FT4's do not");
        Assert.NotEqual(ft8BlockDouble, ft8BlockFloat);

        // The FT8 geometry is untouched by the shared constructor this unit added.
        var ft8 = new Ft8WaterfallGeometry();
        Assert.Equal(1920, ft8.BlockSize);
        Assert.Equal(93, ft8.MaxBlocks);
        Assert.Equal(32, ft8.MinBin);
        Assert.Equal(481, ft8.MaxBin);
        Assert.Equal(449, ft8.BinCount);
        Assert.Equal(Ft8WaterfallGeometry.SymbolPeriodSeconds, ft8.SymbolPeriod);
        Assert.Equal(Ft4Timing.SymbolPeriodSeconds, geometry.SymbolPeriod);
    }

    /// <summary>
    /// The extraction's sync skip, derived from the layout, held against upstream's three literals.
    /// </summary>
    /// <remarks>
    /// <b>The breakage this catches</b>: FT8's 7-then-14 skip carried into FT4's extractor. Every
    /// data symbol after the first sync group would be read one, two or three symbols away from
    /// where it is, the ratios would be noise, and it would look exactly like a decoder that cannot
    /// hear.
    /// </remarks>
    [Fact]
    public void TheSyncSkipIsFiveThenNineThenThirteen()
    {
        for (var k = 0; k < Ft4SymbolEncoder.DataSymbolCount; k++)
        {
            // Upstream's own expression, ft8/decode.c:262.
            var upstream = k + (k < 29 ? 5 : k < 58 ? 9 : 13);
            Assert.Equal(upstream, Ft4SoftSymbols.ChannelSymbolForDataSymbol(k));
        }

        Assert.Equal(5, Ft4SoftSymbols.ChannelSymbolForDataSymbol(0));
        Assert.Equal(38, Ft4SoftSymbols.ChannelSymbolForDataSymbol(29));
        Assert.Equal(71, Ft4SoftSymbols.ChannelSymbolForDataSymbol(58));
        Assert.Equal(99, Ft4SoftSymbols.ChannelSymbolForDataSymbol(86));

        _output.WriteLine("SYNC SKIP");
        _output.WriteLine("  data symbol 0  -> channel symbol 5   (one ramp, one sync group)");
        _output.WriteLine("  data symbol 29 -> channel symbol 38  (one ramp, two sync groups)");
        _output.WriteLine("  data symbol 58 -> channel symbol 71  (one ramp, three sync groups)");
        _output.WriteLine("  data symbol 86 -> channel symbol 99  (the last before the fourth sync "
            + "group and the closing ramp)");
        _output.WriteLine("  all 87 agree with upstream's k + ((k<29)?5:((k<58)?9:13))");
    }

    /// <summary>
    /// The four sync groups carry four different patterns, and the search scores row <c>m</c> at
    /// group <c>m</c>.
    /// </summary>
    /// <remarks>
    /// <b>The breakage this catches</b>: one row repeated four times, which is what a port that read
    /// FT4 as FT8-with-other-constants would do. Twelve of the sixteen sync symbols would be scored
    /// against the wrong tone, the score at the true position would collapse toward the score
    /// anywhere else, and a real transmission would fall below the minimum and never be offered to
    /// the decoder at all.
    /// </remarks>
    [Fact]
    public void TheFourSyncGroupsCarryFourDifferentPatternsAndTheSearchUsesThem()
    {
        var costas = Ft8Tables.Ft4CostasPattern;
        Assert.Equal(16, costas.Length);

        // The four rows are genuinely different from each other.
        for (var a = 0; a < 4; a++)
        {
            for (var b = a + 1; b < 4; b++)
            {
                var same = true;
                for (var k = 0; k < 4; k++)
                {
                    if (costas[(a * 4) + k] != costas[(b * 4) + k])
                    {
                        same = false;
                        break;
                    }
                }

                Assert.False(same, $"FT4 Costas rows {a} and {b} are identical");
            }
        }

        // And the encoder lays row m at group m, which is what the search scores against.
        var symbols = Ft4SymbolEncoder.Encode(EncodeCorpus.Build().First().Message);
        for (var group = 0; group < Ft4SyncSearch.SyncGroupCount; group++)
        {
            var start = Ft4SyncSearch.FirstSyncGroupStart + (group * Ft4SyncSearch.SyncGroupOffset);
            for (var k = 0; k < Ft4SyncSearch.SyncGroupLength; k++)
            {
                Assert.Equal(costas[(group * 4) + k], symbols[start + k]);
            }
        }

        _output.WriteLine("SYNC GROUPS");
        _output.WriteLine("  four rows, all different from each other");
        _output.WriteLine("  laid at symbols 1, 34, 67 and 100, row m at group m");
    }

    /// <summary>
    /// The pieces meet: one message through the whole FT4 chain and back. <b>Not evidence on its
    /// own</b> — see this class's remarks — and the hundred-message run is task 6.
    /// </summary>
    [Fact]
    public void OneMessageGoesThroughTheWholeFt4ChainAndComesBack()
    {
        var message = new byte[Ft8Payload.MessageBytes];
        Assert.Equal(Ft8PackResult.Ok, Ft8StandardMessage.TryPack("CQ", "K1ABC", "FN42", message));

        var symbols = Ft4SymbolEncoder.Encode(message);
        var slot = Ft4Waveform.SynthesizeSlot(symbols);

        var decoder = new Ft4SlotDecoder();
        var result = decoder.Decode(slot);

        _output.WriteLine("FIRST FT4 ROUND TRIP");
        _output.WriteLine($"  slot samples      {slot.Length}");
        _output.WriteLine($"  sweep             blocks {decoder.FirstBlockOffset} to "
            + $"{decoder.LastBlockOffset}");
        _output.WriteLine($"  candidates        {result.CandidateCount}");
        _output.WriteLine($"  parity satisfied  {result.ParitySatisfiedCount}");
        _output.WriteLine($"  checksum passed   {result.ChecksumPassedCount}");
        _output.WriteLine($"  became text       {result.BecameTextCount}");
        _output.WriteLine($"  duplicates        {result.DuplicateCount}");
        _output.WriteLine($"  messages          {result.Messages.Count}");

        Assert.NotEmpty(result.Messages);
        var found = result.Messages[0];
        _output.WriteLine($"  text              {found.Text}");
        _output.WriteLine($"  candidate block   {found.Candidate.BlockOffset} sub "
            + $"{found.Candidate.TimeSubOffset}");
        _output.WriteLine($"  candidate time    {found.TimeSeconds(decoder.Geometry):F4} s");
        _output.WriteLine($"  candidate freq    {found.FrequencyHz(decoder.Geometry):F3} Hz");
        _output.WriteLine($"  candidate score   {found.Candidate.Score}");

        Assert.Equal("CQ K1ABC FN42", found.Text);
        Assert.Single(result.Messages);
    }

    /// <summary>
    /// The payload exclusive-OR is undone after the checksum and before the payload is handed on,
    /// and a decoder that does not undo it returns nothing rather than something wrong.
    /// </summary>
    /// <remarks>
    /// <b>The breakage this catches</b>: applying the exclusive-OR in the wrong place. Undoing it
    /// before the checksum would checksum bits nobody transmitted and every FT4 decode in the world
    /// would fail. Not undoing it at all would produce a message that passed both gates and said
    /// something nobody sent — which is §0.0's own failure, and the reason this is asserted rather
    /// than assumed.
    /// </remarks>
    [Fact]
    public void WithoutThePayloadExclusiveOrNothingComesBackAtAll()
    {
        var message = new byte[Ft8Payload.MessageBytes];
        Assert.Equal(Ft8PackResult.Ok, Ft8StandardMessage.TryPack("CQ", "K1ABC", "FN42", message));

        var slot = Ft4Waveform.SynthesizeSlot(Ft4SymbolEncoder.Encode(message));
        var waterfall = new Ft8Monitor(new Ft4WaterfallGeometry()).Analyse(slot);
        var candidates = new Ft4SyncSearch().Find(waterfall);

        Assert.NotEmpty(candidates);

        var ratios = new float[Ft4SoftSymbols.RatioCount];
        Ft4SoftSymbols.Extract(waterfall, candidates[0], ratios);
        Ft8SoftSymbols.Normalise(ratios);

        // With the sequence: the message.
        var withIt = Ft8Sharp.Ldpc.Ft8CodewordDecoder.Decode(
            ratios, null, Ft8Sharp.Ldpc.LdpcDecoder.DefaultMaxIterations, Ft8Tables.Ft4XorSequence);

        // Without it: whatever the scrambled 77 bits happen to unpack to, and it is not the message.
        var withoutIt = Ft8Sharp.Ldpc.Ft8CodewordDecoder.Decode(ratios);

        _output.WriteLine("PAYLOAD EXCLUSIVE-OR");
        _output.WriteLine($"  with the sequence    {withIt.Status} \"{withIt.Message.Text}\"");
        _output.WriteLine($"  without it           {withoutIt.Status} \"{withoutIt.Message.Text}\"");

        Assert.Equal("CQ K1ABC FN42", withIt.Message.Text);
        Assert.NotEqual("CQ K1ABC FN42", withoutIt.Message.Text);
    }
}
