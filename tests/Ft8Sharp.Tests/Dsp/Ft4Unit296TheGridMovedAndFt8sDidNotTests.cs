using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>Unit 296 task 4 — FT4's analysis grid moved, FT8's did not, and upstream's own numbers are
/// still there for a caller who asks for them.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>THE BREAKAGE THIS CATCHES, AND IT IS THE ONE THAT WOULD COST THIS PROJECT EVERYTHING IT HAS
/// MEASURED.</b> <see cref="Ft4WaterfallGeometry"/> derives from <see cref="Ft8WaterfallGeometry"/>,
/// so FT4's new defaults are <c>new</c> constants shadowing FT8's. A shadowing constant is a quiet
/// thing: put it on the base class by mistake, or let a later hand tidy the two into one, and
/// <b>every FT8 sensitivity figure in this project silently becomes a figure about a decoder nobody
/// measured</b> — the ladders, unit 251's estimator verdict, unit 255's placement panel, unit 256's
/// crossing intervals. The port's byte-fidelity to upstream on FT8 is the instrument all of them
/// lean on. Nothing would throw; the numbers would simply stop meaning what they say.
/// </para>
/// <para>
/// <b>And the second breakage, which is the faithfulness ruling's own.</b> <c>Ft8Sharp</c> remains a
/// faithful MIT port, and the licence unit 289 recorded for
/// <see cref="Ft4SyncSearch.DefaultLastBlockOffset"/> — and unit 296 for
/// <see cref="Ft4WaterfallGeometry.DefaultTimeOversampling"/> — rests on the claim that
/// <b>a caller who asks for upstream's own 2 and 2 gets upstream's own result</b>. That is asserted
/// here rather than promised in a remark, because a default that could not be overridden back would
/// be a divergence and not a parameter.
/// </para>
/// <para>
/// <b>Nothing here runs a decode.</b> These are extents and constants; the rate the grid bought is
/// measured by <c>Ft4Unit296GridSweepTests</c> and <c>Ft4Unit296OffGridRoundTripTests</c>.
/// </para>
/// </remarks>
public class Ft4Unit296TheGridMovedAndFt8sDidNotTests(ITestOutputHelper output)
{
    /// <summary>
    /// <b>FT8's analysis grid still reads 2 and 2, on the constants and on a built geometry.</b>
    /// </summary>
    [Fact]
    public void Ft8sOwnDefaultsDoNotMoveByAnyRoute()
    {
        output.WriteLine(
            $"Ft8WaterfallGeometry.DefaultTimeOversampling      {Ft8WaterfallGeometry.DefaultTimeOversampling}");
        output.WriteLine(
            $"Ft8WaterfallGeometry.DefaultFrequencyOversampling {Ft8WaterfallGeometry.DefaultFrequencyOversampling}");

        Assert.Equal(2, Ft8WaterfallGeometry.DefaultTimeOversampling);
        Assert.Equal(2, Ft8WaterfallGeometry.DefaultFrequencyOversampling);

        var built = new Ft8WaterfallGeometry();

        output.WriteLine(
            $"new Ft8WaterfallGeometry() reports {built.TimeOversampling} and "
            + $"{built.FrequencyOversampling}, block {built.BlockSize}, sub-block "
            + $"{built.SubblockSize}, transform {built.TransformLength}, bins {built.BinCount}, "
            + $"blocks {built.MaxBlocks}");

        Assert.Equal(2, built.TimeOversampling);
        Assert.Equal(2, built.FrequencyOversampling);

        // The extents unit 251, 255 and 256's figures were taken through, written out so that a
        // change to any of them fails here with the number rather than somewhere downstream with a
        // decode rate nobody can account for.
        Assert.Equal(1920, built.BlockSize);
        Assert.Equal(960, built.SubblockSize);
        Assert.Equal(3840, built.TransformLength);
        Assert.Equal(449, built.BinCount);
        Assert.Equal(93, built.MaxBlocks);
    }

    /// <summary>
    /// <b>FT4's analysis grid reads 4 and 2, and the decoder Hamlet builds picks it up.</b>
    /// </summary>
    [Fact]
    public void Ft4sOwnDefaultsAreFourAndTwoAndTheDecoderCarriesThem()
    {
        output.WriteLine(
            $"Ft4WaterfallGeometry.DefaultTimeOversampling      {Ft4WaterfallGeometry.DefaultTimeOversampling}");
        output.WriteLine(
            $"Ft4WaterfallGeometry.DefaultFrequencyOversampling {Ft4WaterfallGeometry.DefaultFrequencyOversampling}");

        Assert.Equal(4, Ft4WaterfallGeometry.DefaultTimeOversampling);
        Assert.Equal(2, Ft4WaterfallGeometry.DefaultFrequencyOversampling);

        var built = new Ft4WaterfallGeometry();

        Assert.Equal(4, built.TimeOversampling);
        Assert.Equal(2, built.FrequencyOversampling);

        // THE OPERATOR'S OWN PATH. Ft8Reception.ReadFt4 builds `new Ft4SlotDecoder()` with no
        // arguments, so a default that moves reaches the tab. If this ever stopped being true the
        // grid would have moved in the library and not in the application.
        var decoder = new Ft4SlotDecoder();

        output.WriteLine(
            $"new Ft4SlotDecoder().Geometry reports {decoder.Geometry.TimeOversampling} and "
            + $"{decoder.Geometry.FrequencyOversampling}");

        Assert.Equal(4, decoder.Geometry.TimeOversampling);
        Assert.Equal(2, decoder.Geometry.FrequencyOversampling);
    }

    /// <summary>
    /// <b>A caller who asks for upstream's own 2 and 2 gets upstream's own geometry, extent for
    /// extent.</b> This is what makes the default a parameter rather than a divergence.
    /// </summary>
    [Fact]
    public void UpstreamsOwnTwoAndTwoIsStillReachableAndStillUpstreams()
    {
        var upstream = new Ft4WaterfallGeometry(
            timeOversampling: 2, frequencyOversampling: 2);

        output.WriteLine(
            $"asked for 2 and 2: block {upstream.BlockSize}, sub-block {upstream.SubblockSize}, "
            + $"transform {upstream.TransformLength}, bins {upstream.BinCount} from "
            + $"{upstream.MinBin} to {upstream.MaxBin}, blocks {upstream.MaxBlocks}, stride "
            + $"{upstream.BlockStride}");

        Assert.Equal(2, upstream.TimeOversampling);
        Assert.Equal(2, upstream.FrequencyOversampling);

        // HEAD a5b9628's own extents, transcribed from the trace this unit took before anything
        // moved: docs/unit296-runs/head-placement-lattice.txt.
        Assert.Equal(576, upstream.BlockSize);
        Assert.Equal(288, upstream.SubblockSize);
        Assert.Equal(1152, upstream.TransformLength);
        Assert.Equal(136, upstream.BinCount);
        Assert.Equal(156, upstream.MaxBlocks);
    }

    /// <summary>
    /// <b>Nothing about the protocol moved, and this is the assertion the faithfulness ruling
    /// actually wants.</b> Only the sub-block and the block stride differ between upstream's grid
    /// and FT4's new one; every extent that describes the <em>modulation</em> is the same number.
    /// </summary>
    [Fact]
    public void OnlyTheAnalysisMovedAndNotTheModulation()
    {
        var upstream = new Ft4WaterfallGeometry(
            timeOversampling: 2, frequencyOversampling: 2);
        var shipping = new Ft4WaterfallGeometry();

        output.WriteLine($"{"extent",-22}{"upstream 2x2",-15}{"shipping 4x2",-15}same?");

        void Same(string what, double a, double b)
        {
            output.WriteLine($"{what,-22}{a,-15}{b,-15}{(a == b ? "yes" : "NO")}");
            Assert.Equal(a, b);
        }

        void Differs(string what, double a, double b)
        {
            output.WriteLine($"{what,-22}{a,-15}{b,-15}{(a == b ? "same" : "moved")}");
            Assert.NotEqual(a, b);
        }

        Same("symbol period s", upstream.SymbolPeriod, shipping.SymbolPeriod);
        Same("slot s", upstream.SlotLengthSeconds, shipping.SlotLengthSeconds);
        Same("sample rate", upstream.SampleRate, shipping.SampleRate);
        Same("block samples", upstream.BlockSize, shipping.BlockSize);
        Same("transform length", upstream.TransformLength, shipping.TransformLength);
        Same("min bin", upstream.MinBin, shipping.MinBin);
        Same("max bin", upstream.MaxBin, shipping.MaxBin);
        Same("bins kept", upstream.BinCount, shipping.BinCount);
        Same("blocks in a slot", upstream.MaxBlocks, shipping.MaxBlocks);
        Same("frequency osr", upstream.FrequencyOversampling, shipping.FrequencyOversampling);

        Differs("time osr", upstream.TimeOversampling, shipping.TimeOversampling);
        Differs("sub-block samples", upstream.SubblockSize, shipping.SubblockSize);
        Differs("block stride", upstream.BlockStride, shipping.BlockStride);

        output.WriteLine(string.Empty);
        output.WriteLine(
            "The transform length is IDENTICAL, which is the whole of why the frequency axis did");
        output.WriteLine(
            "not move: it is BlockSize x FrequencyOversampling, so raising it would lengthen the");
        output.WriteLine(
            "analysis window past one FT4 symbol change and smear a 4-FSK tone across the frame.");
        output.WriteLine(
            "What moved is how often that same-length frame is sampled, and nothing else.");

        // The protocol constants themselves, which no geometry can reach and which are named here
        // so that the claim "nothing about the modulation changed" is a test rather than a remark.
        Assert.Equal(0.048f, Ft4Timing.SymbolPeriodSeconds);
        Assert.Equal(7.5f, Ft4Timing.SlotSeconds);
        Assert.Equal(1.0f / 0.048f, Ft4Timing.ToneSpacingHz);
        Assert.Equal(105, Ft4SymbolEncoder.SymbolCount);
        Assert.Equal(4, Ft4SymbolEncoder.ToneCount);
    }
}
