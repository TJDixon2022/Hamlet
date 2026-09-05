using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Deep.Tests;

/// <summary>
/// <b>The sibling delegates, and the delegation is run rather than reasoned about.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>A delegating decoder returning what it delegates to is trivially true</b> and nobody needs an
/// experiment to believe it. What these tests are for is the part that is not trivial: that the
/// parameters a caller hands this type reach the port unaltered, and that a whole
/// <see cref="Ft8SlotResult"/> - all five counts and every message, in order - comes back across the
/// seam without a count being dropped or a list being reordered.
/// </para>
/// <para>
/// The identity over the ladder, the committed capture and the reference recordings is measured in
/// <c>tests/Ft8Sharp.Tests</c>, where the harness and the fixtures live. This file is the sibling's
/// own suite and stands on audio it synthesises itself.
/// </para>
/// </remarks>
public class Ft8DeepSlotDecoderTests(ITestOutputHelper output)
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    /// <summary>A clean transmission of one free-text message, laid into a whole slot.</summary>
    private static float[] CleanSlot(string text)
    {
        Span<byte> message = stackalloc byte[Ft8Payload.MessageBytes];
        var packed = Ft8FreeText.TryPackText(text, message);
        Assert.Equal(Ft8PackResult.Ok, packed);

        var symbols = Ft8SymbolEncoder.Encode(message);
        return Ft8Waveform.SynthesizeSlot(symbols, Rate);
    }

    [Fact]
    public void TheParametersHandedInReachThePortUnaltered()
    {
        var geometry = new Ft8WaterfallGeometry();
        var search = new Ft8SyncSearch();
        var deep = new Ft8DeepSlotDecoder(geometry, search, messageLimit: 7, maxIterations: 11);

        Assert.Same(geometry, deep.Geometry);
        Assert.Same(geometry, deep.Port.Geometry);
        Assert.Equal(7, deep.MessageLimit);
        Assert.Equal(7, deep.Port.MessageLimit);
        Assert.Equal(11, deep.MaxIterations);
        Assert.Equal(11, deep.Port.MaxIterations);
        Assert.Equal(search.CandidateLimit, deep.CandidateLimit);
        Assert.Equal(search.MinimumScore, deep.MinimumScore);
    }

    /// <summary>
    /// <b>The refusals are the port's, with the port's own wording.</b> A second copy of a refusal is
    /// a copy that drifts, so this type does not check what it is about to hand over.
    /// </summary>
    [Fact]
    public void TheRefusalsAreThePortsAndAreNotReimplementedHere()
    {
        var negativeLimit = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Ft8DeepSlotDecoder(messageLimit: -1));
        Assert.Contains("cannot be negative", negativeLimit.Message, StringComparison.Ordinal);

        var negativeIterations = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Ft8DeepSlotDecoder(maxIterations: -1));
        Assert.Contains("cannot be negative", negativeIterations.Message, StringComparison.Ordinal);

        Assert.Throws<ArgumentNullException>(() => new Ft8DeepSlotDecoder(null!));
        Assert.Throws<ArgumentNullException>(
            () => new Ft8DeepSlotDecoder().Decode((Ft8Waterfall)null!));

        output.WriteLine(negativeLimit.Message);
    }

    /// <summary>
    /// <b>The whole result comes back across the seam, not just the texts.</b> A comparison on text
    /// alone passes while the counts differ, and the counts are what steps 2, 3 and 4 will be read on.
    /// </summary>
    [Fact]
    public void OnCleanAudioTheWholeResultIsThePortsResult()
    {
        var samples = CleanSlot("HAMLET 245");

        var port = new Ft8SlotDecoder();
        var deep = new Ft8DeepSlotDecoder();

        var fromPort = port.Decode(samples);
        var fromDeep = deep.Decode(samples);

        output.WriteLine("count                 Ft8Sharp   Ft8Sharp.Deep");
        output.WriteLine($"candidates          {fromPort.CandidateCount,10} {fromDeep.CandidateCount,15}");
        output.WriteLine($"parity satisfied    {fromPort.ParitySatisfiedCount,10} {fromDeep.ParitySatisfiedCount,15}");
        output.WriteLine($"checksum passed     {fromPort.ChecksumPassedCount,10} {fromDeep.ChecksumPassedCount,15}");
        output.WriteLine($"became text         {fromPort.BecameTextCount,10} {fromDeep.BecameTextCount,15}");
        output.WriteLine($"duplicates          {fromPort.DuplicateCount,10} {fromDeep.DuplicateCount,15}");
        output.WriteLine($"messages            {fromPort.Messages.Count,10} {fromDeep.Messages.Count,15}");

        Assert.Equal(fromPort.CandidateCount, fromDeep.CandidateCount);
        Assert.Equal(fromPort.ParitySatisfiedCount, fromDeep.ParitySatisfiedCount);
        Assert.Equal(fromPort.ChecksumPassedCount, fromDeep.ChecksumPassedCount);
        Assert.Equal(fromPort.BecameTextCount, fromDeep.BecameTextCount);
        Assert.Equal(fromPort.DuplicateCount, fromDeep.DuplicateCount);
        Assert.Equal(fromPort.Messages.Count, fromDeep.Messages.Count);

        for (var i = 0; i < fromPort.Messages.Count; i++)
        {
            Assert.Equal(fromPort.Messages[i].Text, fromDeep.Messages[i].Text, StringComparer.Ordinal);
            Assert.Equal(fromPort.Messages[i].Candidate, fromDeep.Messages[i].Candidate);
            Assert.Equal(
                fromPort.Messages[i].FrequencyHz(port.Geometry),
                fromDeep.Messages[i].FrequencyHz(deep.Geometry),
                12);
            Assert.Equal(
                fromPort.Messages[i].TimeSeconds(port.Geometry),
                fromDeep.Messages[i].TimeSeconds(deep.Geometry),
                12);
        }

        // The audio is a clean transmission of a message the encoder packed, so the message is
        // expected back. If this ever goes red it is a finding about the decoder, not about the seam.
        Assert.Contains("HAMLET 245", fromDeep.Texts);
    }

    /// <summary>
    /// <b>The waterfall overload delegates too</b>, which matters because it is the one step 2 has to
    /// get inside: <c>Ft8SlotDecoder.Decode(Ft8Waterfall)</c> is where an OSD stage would go.
    /// </summary>
    [Fact]
    public void TheWaterfallOverloadDelegatesAsWell()
    {
        var samples = CleanSlot("HAMLET 245");
        var deep = new Ft8DeepSlotDecoder();
        var waterfall = new Ft8Monitor(deep.Geometry).Analyse(samples);

        var fromWaterfall = deep.Decode(waterfall);
        var fromSamples = deep.Decode(samples);

        Assert.Equal(fromSamples.CandidateCount, fromWaterfall.CandidateCount);
        Assert.Equal(fromSamples.BecameTextCount, fromWaterfall.BecameTextCount);
        Assert.Equal(fromSamples.Texts, fromWaterfall.Texts);
    }

    /// <summary>
    /// <b>Nothing here decodes anything the port does not, and this says so on the record.</b> The
    /// sibling's version is 0.1.0 and its capability is delegation; step 2 is where that changes.
    /// </summary>
    [Fact]
    public void TheSiblingHoldsNoDecodeStageOfItsOwn()
    {
        var types = typeof(Ft8DeepSlotDecoder).Assembly.GetTypes();

        output.WriteLine("Types in Ft8Sharp.Deep:");
        foreach (var type in types)
        {
            output.WriteLine($"  {type.FullName}");
        }

        Assert.Single(types);
        Assert.Equal(typeof(Ft8DeepSlotDecoder), types[0]);

        // Named so that the unit which lands ordered statistics decoding has to come here and change
        // this assertion deliberately, rather than discovering afterwards that step 1's claim - that
        // this version changes no behaviour - quietly stopped being true.
        Assert.DoesNotContain(
            types,
            t => t.Name.Contains("Osd", StringComparison.OrdinalIgnoreCase)
                || t.Name.Contains("Ordered", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>The default iteration count is the port's, not a second copy of the number.</summary>
    [Fact]
    public void TheDefaultsAreThePortsDefaults()
    {
        var deep = new Ft8DeepSlotDecoder();

        Assert.Equal(LdpcDecoder.DefaultMaxIterations, deep.MaxIterations);
        Assert.Equal(Ft8SlotDecoder.DefaultMessageLimit, deep.MessageLimit);
    }
}
