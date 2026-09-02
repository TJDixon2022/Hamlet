using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// The waterfall's extents, asserted as arithmetic. <b>These are cheap to get subtly wrong and
/// silent when they are.</b>
/// </summary>
/// <remarks>
/// Every number here follows from the shapes <see cref="UpstreamWaterfallInventoryTests"/> read out
/// of the pin, and none of it needs the clone: the arithmetic is arithmetic and it runs on any
/// machine.
/// </remarks>
public class Ft8WaterfallGeometryTests
{
    private readonly ITestOutputHelper _output;

    public Ft8WaterfallGeometryTests(ITestOutputHelper output) => _output = output;

    /// <summary>Every extent at the rate FT8 is decoded at, printed and then asserted.</summary>
    [Fact]
    public void TheExtentsAtTwelveKilohertzAreWhatUpstreamsArithmeticGives()
    {
        var geometry = new Ft8WaterfallGeometry();

        _output.WriteLine($"sample rate            : {geometry.SampleRate}");
        _output.WriteLine($"block size             : {geometry.BlockSize}");
        _output.WriteLine($"subblock size          : {geometry.SubblockSize}");
        _output.WriteLine($"transform length       : {geometry.TransformLength}");
        _output.WriteLine($"blocks in a slot       : {geometry.MaxBlocks}");
        _output.WriteLine($"first kept bin         : {geometry.MinBin}");
        _output.WriteLine($"one past the last      : {geometry.MaxBin}");
        _output.WriteLine($"bins kept              : {geometry.BinCount}");
        _output.WriteLine($"block stride           : {geometry.BlockStride}");
        _output.WriteLine($"magnitudes in a slot   : {geometry.MagnitudeCount}");
        _output.WriteLine($"transform bin spacing  : {geometry.TransformBinSpacingHz} Hz");
        _output.WriteLine($"tone spacing           : {geometry.ToneSpacingHz} Hz");

        Assert.Equal(12000, geometry.SampleRate);
        Assert.Equal(1920, geometry.BlockSize);
        Assert.Equal(960, geometry.SubblockSize);
        Assert.Equal(3840, geometry.TransformLength);
        Assert.Equal(93, geometry.MaxBlocks);
        Assert.Equal(32, geometry.MinBin);
        Assert.Equal(481, geometry.MaxBin);
        Assert.Equal(449, geometry.BinCount);
        Assert.Equal(2 * 2 * 449, geometry.BlockStride);
        Assert.Equal(93 * 2 * 2 * 449, geometry.MagnitudeCount);
        Assert.Equal(3.125, geometry.TransformBinSpacingHz, 12);

        // NOT 6.25, and the difference is upstream's and is worth naming. The tone spacing is one
        // over the symbol period, and the symbol period is a single-precision 0.160f, which is
        // really 0.1599999964237213. So the spacing comes out 6.2500001397 Hz. That is one part in
        // 45 million and it does not matter; what matters is that it is not silently rounded to the
        // published figure, because two routes to the same frequency then disagree by a measurable
        // amount and a reader has to be able to find out why.
        _output.WriteLine(string.Empty);
        _output.WriteLine($"tone spacing is NOT exactly 6.25: it is {geometry.ToneSpacingHz:R} Hz,");
        _output.WriteLine($"because 1/0.160f where 0.160f = {(double)Ft8WaterfallGeometry.SymbolPeriodSeconds:R}.");
        _output.WriteLine($"the difference from the published 6.25 Hz is {geometry.ToneSpacingHz - 6.25:E3} Hz.");

        Assert.Equal(6.25, geometry.ToneSpacingHz, 6);
        Assert.NotEqual(6.25, geometry.ToneSpacingHz);
    }

    /// <summary>
    /// <b>The single precision is load-bearing, and here is the measurement rather than the claim.</b>
    /// </summary>
    /// <remarks>
    /// This is unit 212's lesson arriving on the receive side. That unit measured its waveform
    /// agreeing with upstream to one count <em>because</em> it kept the phase step in single
    /// precision as upstream does; here the same choice moves whole integers, not last places.
    /// </remarks>
    [Fact]
    public void ComputingTheGeometryInDoublePrecisionWouldGiveDifferentIntegers()
    {
        const float symbolPeriodSingle = Ft8WaterfallGeometry.SymbolPeriodSeconds;
        const double symbolPeriodWidened = symbolPeriodSingle;

        var blockSingle = (int)(float)(12000 * symbolPeriodSingle);
        var blockDouble = (int)(12000 * symbolPeriodWidened);
        var minSingle = (int)(float)(200.0f * symbolPeriodSingle);
        var minDouble = (int)(200.0 * symbolPeriodWidened);
        var maxSingle = (int)(float)(3000.0f * symbolPeriodSingle) + 1;
        var maxDouble = (int)(3000.0 * symbolPeriodWidened) + 1;

        _output.WriteLine($"0.160f is really            : {symbolPeriodWidened:R}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("                        float (upstream, and this port)   double");
        _output.WriteLine($"block size          : {blockSingle,10}  {blockDouble,25}");
        _output.WriteLine($"first kept bin      : {minSingle,10}  {minDouble,25}");
        _output.WriteLine($"one past the last   : {maxSingle,10}  {maxDouble,25}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("A block one sample short misaligns every symbol after the first. A first");
        _output.WriteLine("bin one lower shifts every frequency this library reports by one whole");
        _output.WriteLine("FT8 tone, 6.25 Hz. So the more accurate arithmetic is the wrong one, and");
        _output.WriteLine("the library's geometry is single precision on purpose.");

        Assert.Equal(1920, blockSingle);
        Assert.Equal(1919, blockDouble);
        Assert.Equal(32, minSingle);
        Assert.Equal(31, minDouble);
        Assert.Equal(481, maxSingle);
        Assert.Equal(480, maxDouble);

        // And the library takes the single-precision column, every one of them.
        var geometry = new Ft8WaterfallGeometry();
        Assert.Equal(blockSingle, geometry.BlockSize);
        Assert.Equal(minSingle, geometry.MinBin);
        Assert.Equal(maxSingle, geometry.MaxBin);
    }

    /// <summary>
    /// The receive side holds its own copy of the protocol's published constants so that nothing in
    /// the decoder depends on the encoder. <b>Two copies can drift, so the drift is asserted away.</b>
    /// </summary>
    [Fact]
    public void TheReceiveSidesProtocolConstantsMatchTheTransmitSides()
    {
        Assert.Equal(Ft8Waveform.SymbolPeriodSeconds, Ft8WaterfallGeometry.SymbolPeriodSeconds);
        Assert.Equal(Ft8Waveform.SlotSeconds, Ft8WaterfallGeometry.SlotSeconds);
        Assert.Equal(Ft8Waveform.DefaultSampleRate, Ft8WaterfallGeometry.DefaultSampleRate);
        Assert.Equal(Ft8Waveform.ToneSpacingHz, (float)new Ft8WaterfallGeometry().ToneSpacingHz);

        _output.WriteLine("symbol period, slot, default rate and tone spacing agree across the two "
            + "sides of the library.");
    }

    /// <summary>How many blocks a fifteen-second slot produces, from the sample count.</summary>
    [Fact]
    public void AFifteenSecondSlotProducesNinetyThreeBlocksWithAPartialOneLeftOver()
    {
        var geometry = new Ft8WaterfallGeometry();
        var slotSamples = Ft8Waveform.SlotSampleCount(geometry.SampleRate);

        var whole = slotSamples / geometry.BlockSize;
        var leftOver = slotSamples % geometry.BlockSize;

        _output.WriteLine($"slot samples      : {slotSamples}");
        _output.WriteLine($"whole blocks      : {whole}");
        _output.WriteLine($"samples left over : {leftOver}");
        _output.WriteLine($"waterfall capacity: {geometry.MaxBlocks}");

        Assert.Equal(180000, slotSamples);
        Assert.Equal(93, whole);
        Assert.Equal(1440, leftOver);

        // The capacity and the count a slot actually yields agree, which is not automatic: one comes
        // from the slot duration over the symbol period and the other from the sample count over the
        // block size, by two different routes through the same floats.
        Assert.Equal(whole, geometry.MaxBlocks);
    }

    /// <summary>
    /// The mapping from a bin index to a frequency in hertz, at both sub-offsets, checked against
    /// two independent routes.
    /// </summary>
    [Fact]
    public void ABinIndexMapsToAFrequencyByTwoRoutesThatAgree()
    {
        var geometry = new Ft8WaterfallGeometry();

        _output.WriteLine("bin  sub    upstream's formula      via the transform's bin spacing");
        double worst = 0;
        foreach (var bin in new[] { 0, 1, 128, 288, geometry.BinCount - 1 })
        {
            for (var sub = 0; sub < geometry.FrequencyOversampling; sub++)
            {
                // Route one: upstream's own expression from decode_ft8.c.
                var viaFormula = geometry.FrequencyHz(bin, sub);

                // Route two: the index into the transform, times the transform's own bin spacing.
                var viaTransform = geometry.TransformBin(bin, sub) * geometry.TransformBinSpacingHz;

                worst = Math.Max(worst, Math.Abs(viaFormula - viaTransform));
                _output.WriteLine($"{bin,4} {sub,4}  {viaFormula,20:F6}  {viaTransform,28:F6}");
            }
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"worst disagreement between the two routes: {worst:E3} Hz");
        _output.WriteLine("MEASURED FIRST. The two routes are not identical and should not be");
        _output.WriteLine("expected to be: upstream's formula divides by the SINGLE-PRECISION symbol");
        _output.WriteLine("period 0.160f, while the transform's bin spacing is the sample rate over");
        _output.WriteLine("the transform length, which is exact in integers. The gap is the error in");
        _output.WriteLine($"0.160f, about 2.2e-8 relative, so it grows with frequency and reaches");
        _output.WriteLine($"{worst:E3} Hz at the top of the passband. Against a 6.25 Hz tone spacing");
        _output.WriteLine($"that is one part in {6.25 / worst:N0}, which no bin decision can see.");
        _output.WriteLine("Upstream's formula is the one this library reports, because it is the one");
        _output.WriteLine("the next unit's candidate frequencies will be compared against.");

        // The bound is set from the measurement above: a thousandth of a hertz, which is four
        // orders below the 6.25 Hz that separates two tones and one order above what was measured.
        Assert.True(worst < 1e-3, $"the two routes to a frequency disagree by {worst:E3} Hz.");

        // The ends of the passband, named — to six places, which is where the single-precision
        // symbol period stops agreeing with the published one.
        Assert.Equal(200.0, geometry.FrequencyHz(0, 0), 4);
        Assert.Equal(203.125, geometry.FrequencyHz(0, 1), 4);
        Assert.Equal(206.25, geometry.FrequencyHz(1, 0), 4);
        Assert.Equal(3003.125, geometry.FrequencyHz(geometry.BinCount - 1, 1), 3);
    }

    /// <summary>The reverse mapping, and what it does with a frequency outside the passband.</summary>
    [Fact]
    public void AFrequencyMapsBackToTheBinItCameFrom()
    {
        var geometry = new Ft8WaterfallGeometry();

        for (var bin = 0; bin < geometry.BinCount; bin++)
        {
            for (var sub = 0; sub < geometry.FrequencyOversampling; sub++)
            {
                var hertz = geometry.FrequencyHz(bin, sub);
                Assert.True(geometry.TryBinFor(hertz, out var backBin, out var backSub),
                    $"{hertz} Hz came out of the passband and did not go back in.");
                Assert.Equal(bin, backBin);
                Assert.Equal(sub, backSub);
            }
        }

        _output.WriteLine($"all {geometry.BinCount * geometry.FrequencyOversampling} bin centres "
            + "map to a frequency and back to themselves.");

        Assert.False(geometry.TryBinFor(100.0, out _, out _));
        Assert.False(geometry.TryBinFor(5000.0, out _, out _));
        _output.WriteLine("100 Hz and 5000 Hz are reported as outside the passband rather than "
            + "clamped silently into it.");
    }

    /// <summary>The time a block covers, by upstream's own expression.</summary>
    [Fact]
    public void ABlockIndexMapsToATimeInSeconds()
    {
        var geometry = new Ft8WaterfallGeometry();

        _output.WriteLine($"block  0 sub 0 : {geometry.TimeSeconds(0, 0):F6} s");
        _output.WriteLine($"block  0 sub 1 : {geometry.TimeSeconds(0, 1):F6} s");
        _output.WriteLine($"block  1 sub 0 : {geometry.TimeSeconds(1, 0):F6} s");
        _output.WriteLine($"block 92 sub 0 : {geometry.TimeSeconds(92, 0):F6} s");
        _output.WriteLine($"block 92 sub 1 : {geometry.TimeSeconds(92, 1):F6} s");
        _output.WriteLine(string.Empty);
        _output.WriteLine("The last block begins at 14.72 s and the last sub-offset at 14.80 s, so");
        _output.WriteLine("93 blocks of 0.16 s cover 14.88 s of a 15 s slot. The 0.12 s remaining is");
        _output.WriteLine("the 1440 samples a whole block does not fit into.");

        Assert.Equal(0.0, geometry.TimeSeconds(0, 0), 9);
        Assert.Equal(0.08, geometry.TimeSeconds(0, 1), 6);
        Assert.Equal(0.16, geometry.TimeSeconds(1, 0), 6);
        Assert.Equal(14.72, geometry.TimeSeconds(92, 0), 6);
        Assert.Equal(14.80, geometry.TimeSeconds(92, 1), 6);
    }

    // ---- Refusals, each watched refusing. ----

    /// <summary>
    /// <b>A sample rate the geometry does not divide is refused, and each of the two shapes is
    /// watched separately.</b> Upstream truncates and carries on; this does not.
    /// </summary>
    /// <remarks>
    /// <b>The rates chosen here are deliberate and the first attempt at them was wrong.</b> The
    /// symbol period is 0.160 s = 4/25, so a rate produces a whole number of samples per symbol
    /// exactly when it is a multiple of 25 — and 8000, 11025, 22050, 44100 and 48000 all are. Every
    /// audio rate in ordinary use passes, which is precisely why upstream never met this and why the
    /// guard has to be aimed at a rate that genuinely fails rather than at one that merely looks
    /// unusual. <see cref="ASampleRateAtWhichASymbolIsWholeIsAccepted"/> carries the ones that pass,
    /// so this guard is watched refusing <em>and</em> watched not refusing.
    /// </remarks>
    [Theory]
    [InlineData(4410)]
    [InlineData(11111)]
    [InlineData(12001)]
    [InlineData(9999)]
    public void ASampleRateAtWhichASymbolIsNotAWholeNumberOfSamplesIsRefused(int sampleRate)
    {
        var refusal = Assert.Throws<ArgumentException>(() => new Ft8WaterfallGeometry(sampleRate));
        _output.WriteLine($"{sampleRate,6} Hz ({sampleRate * 0.160:F3} samples per symbol) refused: "
            + $"{refusal.Message.Split('.')[0]}.");
        Assert.Equal("sampleRate", refusal.ParamName);
    }

    /// <summary>
    /// Rates at which a symbol IS a whole number of samples are accepted — including every audio
    /// rate in common use, because all of them are multiples of 25.
    /// </summary>
    [Theory]
    [InlineData(12000, 1920)]
    [InlineData(24000, 3840)]
    [InlineData(48000, 7680)]
    [InlineData(6000, 960)]
    [InlineData(8000, 1280)]
    [InlineData(11025, 1764)]
    [InlineData(44100, 7056)]
    public void ASampleRateAtWhichASymbolIsWholeIsAccepted(int sampleRate, int expectedBlock)
    {
        var geometry = new Ft8WaterfallGeometry(sampleRate);
        _output.WriteLine($"{sampleRate,6} Hz -> block {geometry.BlockSize}, transform "
            + $"{geometry.TransformLength}, bins {geometry.BinCount}");
        Assert.Equal(expectedBlock, geometry.BlockSize);
    }

    /// <summary>
    /// <b>A block that does not divide by the time oversampling factor is refused</b>, because the
    /// remainder would be audio the analysis never looks at.
    /// </summary>
    [Fact]
    public void ATimeOversamplingFactorThatDoesNotDivideTheBlockIsRefused()
    {
        // 6000 Hz gives a block of 960 samples, which 7 does not divide.
        var refusal = Assert.Throws<ArgumentException>(
            () => new Ft8WaterfallGeometry(6000, timeOversampling: 7));

        _output.WriteLine($"refused: {refusal.Message}");
        Assert.Equal("timeOversampling", refusal.ParamName);
        Assert.Contains("never be looked at", refusal.Message, StringComparison.Ordinal);

        // And the factor that does divide it is accepted, so the guard is not simply always on.
        var fine = new Ft8WaterfallGeometry(6000, timeOversampling: 5);
        _output.WriteLine($"5 accepted at the same rate: subblock {fine.SubblockSize} x 5 = "
            + $"{fine.SubblockSize * 5} = block {fine.BlockSize}");
        Assert.Equal(fine.BlockSize, fine.SubblockSize * 5);
    }

    /// <summary>Oversampling factors below one, and an empty or inverted passband, are refused.</summary>
    [Fact]
    public void DegenerateConfigurationsAreRefused()
    {
        var refusals = new (string What, Func<Ft8WaterfallGeometry> Build)[]
        {
            ("time oversampling of 0", () => new Ft8WaterfallGeometry(timeOversampling: 0)),
            ("time oversampling of -1", () => new Ft8WaterfallGeometry(timeOversampling: -1)),
            ("frequency oversampling of 0", () => new Ft8WaterfallGeometry(frequencyOversampling: 0)),
            ("a sample rate of 0", () => new Ft8WaterfallGeometry(0)),
            ("a sample rate of -12000", () => new Ft8WaterfallGeometry(-12000)),
            ("an inverted passband", () => new Ft8WaterfallGeometry(12000, 3000f, 200f)),
            ("an empty passband", () => new Ft8WaterfallGeometry(12000, 1000f, 1000f)),
            ("a negative low edge", () => new Ft8WaterfallGeometry(12000, -100f, 3000f)),
        };

        foreach (var (what, build) in refusals)
        {
            var refusal = Assert.ThrowsAny<ArgumentException>(() => build());
            _output.WriteLine($"{what,-32} refused: {refusal.Message.Split('.', ';')[0]}.");
        }

        _output.WriteLine($"{refusals.Length} configurations watched refusing.");
    }

    /// <summary>Indexing outside any extent of the waterfall is refused rather than wrapped.</summary>
    [Fact]
    public void IndexingOutsideTheWaterfallIsRefused()
    {
        var geometry = new Ft8WaterfallGeometry();
        var monitor = new Ft8Monitor(geometry);
        var waterfall = monitor.Waterfall;

        var refusals = new (string What, Func<int> Read)[]
        {
            ("block -1", () => waterfall[-1, 0, 0, 0]),
            ("block 93", () => waterfall[geometry.MaxBlocks, 0, 0, 0]),
            ("time sub -1", () => waterfall[0, -1, 0, 0]),
            ("time sub 2", () => waterfall[0, geometry.TimeOversampling, 0, 0]),
            ("freq sub -1", () => waterfall[0, 0, -1, 0]),
            ("freq sub 2", () => waterfall[0, 0, geometry.FrequencyOversampling, 0]),
            ("bin -1", () => waterfall[0, 0, 0, -1]),
            ("bin 449", () => waterfall[0, 0, 0, geometry.BinCount]),
        };

        foreach (var (what, read) in refusals)
        {
            var refusal = Assert.Throws<ArgumentOutOfRangeException>(() => read());
            _output.WriteLine($"{what,-14} refused: {refusal.Message.Split('(')[0].Trim()}");
        }

        _output.WriteLine($"{refusals.Length} out-of-range reads watched refusing.");
    }
}
