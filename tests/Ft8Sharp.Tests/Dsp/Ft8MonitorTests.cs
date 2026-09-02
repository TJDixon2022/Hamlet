using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// The waterfall as a structure: what shape it comes out, what it stores, and what it refuses.
/// </summary>
/// <remarks>
/// <b>Nothing here is a detection.</b> These tests check that the spectrogram has the extents and
/// the storage upstream's has, and that it turns audio into magnitudes at all. Whether the energy
/// lands where the tones were put is <see cref="Ft8ToneRecoveryTests"/>, and finding a signal nobody
/// pointed at is not in this unit at all.
/// </remarks>
public class Ft8MonitorTests
{
    private readonly ITestOutputHelper _output;

    public Ft8MonitorTests(ITestOutputHelper output) => _output = output;

    /// <summary>The output's extents for a known input length.</summary>
    [Fact]
    public void AWholeSlotFillsNinetyThreeBlocksAndTheStoreIsTheSizeTheGeometrySays()
    {
        var monitor = new Ft8Monitor();
        var geometry = monitor.Geometry;

        var symbols = Ft8SymbolEncoder.Encode(PackedFor("CQ with a grid"));
        var slot = Ft8Waveform.SynthesizeSlot(symbols);

        var waterfall = monitor.Analyse(slot);

        _output.WriteLine($"input samples    : {slot.Length}");
        _output.WriteLine($"blocks filled    : {waterfall.BlockCount}");
        _output.WriteLine($"store length     : {waterfall.Magnitudes.Length}");
        _output.WriteLine($"expected         : {geometry.MaxBlocks} x {geometry.BlockStride} "
            + $"= {geometry.MagnitudeCount}");
        _output.WriteLine($"largest reading  : {waterfall.LargestDecibels:F3} dB");

        Assert.Equal(180000, slot.Length);
        Assert.Equal(93, waterfall.BlockCount);
        Assert.Equal(geometry.MagnitudeCount, waterfall.Magnitudes.Length);
        Assert.Equal(167028, waterfall.Magnitudes.Length);
    }

    /// <summary>
    /// The store is bytes on upstream's half-decibel scale, and the round trip through
    /// <see cref="Ft8Waterfall.StoredFor"/> and <see cref="Ft8Waterfall.DecibelsFor"/> lands where
    /// upstream's macros land.
    /// </summary>
    [Fact]
    public void MagnitudesAreStoredAsBytesOnUpstreamsHalfDecibelScale()
    {
        _output.WriteLine("  dB in   byte   dB back");
        foreach (var decibels in new[] { -200f, -120f, -119.5f, -60f, -0.25f, 0f, 7.5f, 8f, 100f })
        {
            var stored = Ft8Waterfall.StoredFor(decibels);
            _output.WriteLine($"{decibels,8:F2} {stored,6} {Ft8Waterfall.DecibelsFor(stored),10:F2}");
        }

        // The two ends clamp rather than wrap, which is a 256-count difference if got wrong.
        Assert.Equal(0, Ft8Waterfall.StoredFor(-200f));
        Assert.Equal(0, Ft8Waterfall.StoredFor(-120f));
        Assert.Equal(255, Ft8Waterfall.StoredFor(100f));
        Assert.Equal(255, Ft8Waterfall.StoredFor(7.5f));

        // The middle is exactly two counts per decibel.
        Assert.Equal(240, Ft8Waterfall.StoredFor(0f));
        Assert.Equal(120, Ft8Waterfall.StoredFor(-60f));
        Assert.Equal(1, Ft8Waterfall.StoredFor(-119.5f));

        Assert.Equal(-120.0, Ft8Waterfall.DecibelsFor(0));
        Assert.Equal(0.0, Ft8Waterfall.DecibelsFor(240));
        Assert.Equal(7.5, Ft8Waterfall.DecibelsFor(255));
    }

    /// <summary>
    /// The floor inside the logarithm means a silent band reads as the bottom of the scale rather
    /// than as minus infinity or as a not-a-number.
    /// </summary>
    [Fact]
    public void SilenceReadsAsTheBottomOfTheScaleAndNotAsANotANumber()
    {
        var monitor = new Ft8Monitor();
        var silence = new float[monitor.Geometry.BlockSize * 4];

        var waterfall = monitor.Analyse(silence);

        var distinct = new HashSet<byte>();
        for (var i = 0; i < waterfall.BlockCount * monitor.Geometry.BlockStride; i++)
        {
            distinct.Add(waterfall.Magnitudes[i]);
        }

        _output.WriteLine($"blocks of silence analysed : {waterfall.BlockCount}");
        _output.WriteLine($"distinct stored values     : {string.Join(", ", distinct.OrderBy(b => b))}");
        _output.WriteLine($"largest reading            : {waterfall.LargestDecibels:F3} dB");
        _output.WriteLine("10*log10(1e-12) is -120 dB exactly, which stores as byte 0.");

        Assert.Equal(new byte[] { 0 }, distinct.OrderBy(b => b).ToArray());
        Assert.Equal(-120.0, waterfall.LargestDecibels, 6);
    }

    /// <summary>
    /// The analysis is deterministic: the same audio through two monitors gives byte-identical
    /// waterfalls. <b>Not step 4's stability criterion</b>, which is about ranking candidates and is
    /// not met by this unit; this is the layer underneath it.
    /// </summary>
    [Fact]
    public void TheSameAudioGivesAByteIdenticalWaterfall()
    {
        var symbols = Ft8SymbolEncoder.Encode(PackedFor("CQ with a grid"));
        var slot = Ft8Waveform.SynthesizeSlot(symbols, baseFrequency: 1234.0f);

        var first = new Ft8Monitor().Analyse(slot);
        var second = new Ft8Monitor().Analyse(slot);

        // And a monitor reused after a reset, which is the case that catches leftover state.
        var reused = new Ft8Monitor();
        reused.Analyse(Ft8Waveform.SynthesizeSlot(symbols, baseFrequency: 700.0f));
        var third = reused.Analyse(slot);

        var same = 0;
        var sameAfterReuse = 0;
        for (var i = 0; i < first.Magnitudes.Length; i++)
        {
            if (first.Magnitudes[i] == second.Magnitudes[i])
            {
                same++;
            }

            if (first.Magnitudes[i] == third.Magnitudes[i])
            {
                sameAfterReuse++;
            }
        }

        _output.WriteLine($"identical on a fresh monitor      : {same} of {first.Magnitudes.Length}");
        _output.WriteLine($"identical on a monitor after reset: {sameAfterReuse} of {first.Magnitudes.Length}");

        Assert.Equal(first.Magnitudes.Length, same);
        Assert.Equal(first.Magnitudes.Length, sameAfterReuse);
    }

    /// <summary>
    /// The window is upstream's: a squared sine over the whole transform, with the normalisation
    /// folded in.
    /// </summary>
    [Fact]
    public void TheWindowIsAHannWrittenAsASquaredSine()
    {
        const int length = 64;

        _output.WriteLine("  i   sin^2(pi i/N)   (1 - cos(2 pi i/N))/2      difference");
        double worst = 0;
        foreach (var i in new[] { 0, 1, 16, 32, 48, 63 })
        {
            var squaredSine = Ft8Monitor.HannSquaredSine(i, length);
            var halfCosine = (1 - Math.Cos(2 * Math.PI * i / length)) / 2;
            worst = Math.Max(worst, Math.Abs(squaredSine - halfCosine));
            _output.WriteLine($"{i,3} {squaredSine,15:F12} {halfCosine,22:F12} {Math.Abs(squaredSine - halfCosine),15:E3}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"worst difference between the two forms: {worst:E3}");
        _output.WriteLine("The two are the same function and not the same arithmetic. The squared");
        _output.WriteLine("sine is the one the pin computes, so it is the one the port computes.");

        Assert.Equal(0.0, Ft8Monitor.HannSquaredSine(0, length));
        Assert.Equal(1.0, Ft8Monitor.HannSquaredSine(length / 2, length), 12);
        Assert.True(worst < 1e-15, $"the two Hann forms differ by {worst:E3}, which is more than rounding.");
    }

    /// <summary>
    /// A block past the end of the waterfall is reported rather than thrown, matching upstream's
    /// early return — a caller streaming a slot runs past the end as a matter of course.
    /// </summary>
    [Fact]
    public void BlocksPastTheEndOfTheWaterfallAreReportedAndNotStored()
    {
        var monitor = new Ft8Monitor();
        var block = new float[monitor.Geometry.BlockSize];

        var accepted = 0;
        var refused = 0;
        for (var i = 0; i < monitor.Geometry.MaxBlocks + 5; i++)
        {
            if (monitor.ProcessBlock(block))
            {
                accepted++;
            }
            else
            {
                refused++;
            }
        }

        _output.WriteLine($"blocks accepted : {accepted}");
        _output.WriteLine($"blocks declined : {refused}");
        _output.WriteLine($"blocks stored   : {monitor.Waterfall.BlockCount}");

        Assert.Equal(monitor.Geometry.MaxBlocks, accepted);
        Assert.Equal(5, refused);
        Assert.Equal(monitor.Geometry.MaxBlocks, monitor.Waterfall.BlockCount);
    }

    // ---- Refusals, each watched refusing. ----

    /// <summary>
    /// <b>A block of the wrong length is refused, and the monitor is left exactly as it was found.</b>
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(1919)]
    [InlineData(1921)]
    [InlineData(3840)]
    public void ABlockOfTheWrongLengthIsRefusedAndTheFrameIsNotTouched(int wrongLength)
    {
        var monitor = new Ft8Monitor();
        var good = new float[monitor.Geometry.BlockSize];
        for (var i = 0; i < good.Length; i++)
        {
            good[i] = 0.5f * MathF.Sin(i * 0.01f);
        }

        // Two good blocks, so there is real history in the sliding frame to disturb.
        monitor.ProcessBlock(good);
        monitor.ProcessBlock(good);
        var afterTwo = monitor.Waterfall.Magnitudes.ToArray();

        var refusal = Assert.Throws<ArgumentException>(() => monitor.ProcessBlock(new float[wrongLength]));
        _output.WriteLine($"{wrongLength,5} samples refused: {refusal.Message.Split('.')[0]}.");

        Assert.Equal(2, monitor.Waterfall.BlockCount);
        Assert.Equal(afterTwo, monitor.Waterfall.Magnitudes.ToArray());

        // And the proof that the sliding frame really was untouched: the third good block must give
        // exactly what it would have given had the refusal never happened.
        monitor.ProcessBlock(good);

        var clean = new Ft8Monitor();
        clean.ProcessBlock(good);
        clean.ProcessBlock(good);
        clean.ProcessBlock(good);

        Assert.Equal(clean.Waterfall.Magnitudes.ToArray(), monitor.Waterfall.Magnitudes.ToArray());
        _output.WriteLine("       and the next good block matched a monitor that never saw the refusal.");
    }

    /// <summary>A signal shorter than one block is refused rather than analysed as nothing.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(959)]
    [InlineData(1919)]
    public void ASignalShorterThanOneBlockIsRefused(int length)
    {
        var monitor = new Ft8Monitor();
        var refusal = Assert.Throws<ArgumentException>(() => monitor.Analyse(new float[length]));
        _output.WriteLine($"{length,5} samples refused: {refusal.Message.Split('.')[0]}.");
        Assert.Equal("samples", refusal.ParamName);
    }

    /// <summary>Exactly one block is enough, so the guard above is not simply always on.</summary>
    [Fact]
    public void ExactlyOneBlockIsAccepted()
    {
        var monitor = new Ft8Monitor();
        var waterfall = monitor.Analyse(new float[monitor.Geometry.BlockSize]);
        _output.WriteLine($"1920 samples accepted, {waterfall.BlockCount} block analysed.");
        Assert.Equal(1, waterfall.BlockCount);
    }

    /// <summary>
    /// One packed message from the corpus the encode tests already own. <b>Reused rather than
    /// rebuilt</b> — a second corpus is a second thing to keep true.
    /// </summary>
    internal static byte[] PackedFor(string label)
    {
        var entry = Ft8Sharp.Tests.Encode.EncodeCorpus.Build().FirstOrDefault(e => e.Label == label);
        Assert.True(entry is not null, $"no corpus entry is labelled '{label}'.");
        return entry!.Message;
    }
}
