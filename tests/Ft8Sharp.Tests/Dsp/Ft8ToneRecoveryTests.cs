using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>The unit's target: given a signal this library synthesized itself, at a frequency and a time
/// it chose, does the energy land where we know the tones are?</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>THIS IS NOT A SEARCH. Nothing here is told to find a signal; it is told where the signal is,
/// in frequency and in time, by construction.</b> The base frequency was chosen and handed to the
/// synthesizer. The sample offset was chosen and used to place the signal. The symbol index is a
/// loop variable and the tone at it came out of the encoder. No Costas correlation runs, no
/// candidate is formed, nothing is scored and nothing is ranked. <b>Step 4's three subject criteria
/// are not met by anything in this file and are not aimed at.</b>
/// </para>
/// <para>
/// <b>What it therefore does show.</b> That the transform, the window, the geometry and the storage
/// are together good enough that a tone put at a known place is the strongest of its eight
/// neighbours at that place — with a margin, measured, that the next unit's correlator will live or
/// die on. And that the same measurement over noise alone comes back at chance, which is what makes
/// the first number mean something instead of being an artefact of asking a question with an obvious
/// answer.
/// </para>
/// <para>
/// <b>Nothing here reads the clone.</b> This is the half of tonight's evidence that survives on a
/// machine with no upstream at all.
/// </para>
/// </remarks>
public class Ft8ToneRecoveryTests
{
    private readonly ITestOutputHelper _output;

    public Ft8ToneRecoveryTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// Base frequencies, chosen so that some fall exactly on a bin centre and some deliberately do
    /// not. <b>A tone between bins is the case the spectrum has to survive</b> and the one a single
    /// well-chosen frequency would hide. Bins are 3.125 Hz apart.
    /// </summary>
    private static readonly (float Hertz, string What)[] BaseFrequencies =
    {
        (1000.0f, "on a bin centre (320 x 3.125)"),
        (800.0f, "on a bin centre (256 x 3.125)"),
        (1500.0f, "on a bin centre (480 x 3.125)"),
        (1001.5625f, "EXACTLY HALFWAY between two bins (320.5 x 3.125)"),
        (1234.0f, "off centre by 0.75 Hz (394.88 x 3.125)"),
        (2000.78125f, "a quarter of a bin off centre (640.25 x 3.125)"),
    };

    /// <summary>
    /// Sample offsets into the slot. <b>Not all of them are a whole number of blocks</b>, and one of
    /// them is not even a whole number of sub-blocks — 14160 is the offset
    /// <see cref="Ft8Waveform.SynthesizeSlot"/> itself uses, and it is 7 blocks and 720 samples,
    /// which lands 0.375 of a symbol off the analysis grid.
    /// </summary>
    private static readonly (int Samples, string What)[] Offsets =
    {
        (0, "the start of the slot, exactly on a block"),
        (1920, "one whole block in"),
        (960, "HALF a block in, which is not a whole number of blocks"),
        (14160, "the synthesizer's own slot padding: 7 blocks and 720 samples, OFF the sub-block grid"),
        (4805, "an arbitrary offset, 2 blocks and 965 samples"),
    };

    /// <summary>
    /// <b>THE HEADLINE NUMBER.</b> Every message the corpus can synthesize, at one base frequency and
    /// one offset, reported as a count of symbols over messages.
    /// </summary>
    [Fact]
    public void TonesAreRecoveredAcrossTheWholeCorpus()
    {
        var monitor = new Ft8Monitor();
        var corpus = EncodeCorpus.Build();

        var recovered = 0;
        var total = 0;
        var messages = 0;
        var worstMargin = double.MaxValue;
        var worstAt = string.Empty;
        var failures = new List<string>();

        foreach (var entry in corpus)
        {
            var result = ToneRecovery.Measure(monitor, entry.Label, entry.Message, 1000.0f, 0);
            messages++;
            recovered += result.Recovered;
            total += result.Total;

            if (result.WorstMarginDecibels < worstMargin)
            {
                worstMargin = result.WorstMarginDecibels;
                worstAt = entry.Label;
            }

            foreach (var failure in result.Failures)
            {
                failures.Add($"'{entry.Label}' symbol {failure.Symbol}: transmitted tone "
                    + $"{failure.Expected}, strongest was {failure.Strongest}, margin "
                    + $"{failure.MarginDecibels:F1} dB");
            }
        }

        _output.WriteLine("THE MEASUREMENT, AND IT IS NOT A SEARCH.");
        _output.WriteLine($"base frequency        : 1000.0 Hz, chosen and handed to the synthesizer");
        _output.WriteLine($"offset in the slot    : 0 samples, chosen");
        _output.WriteLine($"candidate tones       : {Ft8Waveform.ToneCount}, so chance is "
            + $"{100.0 / Ft8Waveform.ToneCount:F1} per cent");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"RECOVERED             : {recovered} of {total} symbols across {messages} messages");
        _output.WriteLine($"                        {100.0 * recovered / total:F3} per cent");
        _output.WriteLine($"worst margin anywhere : {worstMargin:F1} dB, in '{worstAt}'");

        if (failures.Count > 0)
        {
            _output.WriteLine(string.Empty);
            _output.WriteLine($"{failures.Count} symbols not recovered:");
            foreach (var failure in failures.Take(40))
            {
                _output.WriteLine($"    {failure}");
            }
        }

        Assert.Equal(total, recovered);
        Assert.True(worstMargin > 0, $"the worst margin was {worstMargin:F1} dB, which is not a margin.");
    }

    /// <summary>
    /// The same measurement across six base frequencies, three of which do not fall on a bin centre.
    /// </summary>
    [Fact]
    public void TonesAreRecoveredAtFrequenciesOnAndOffBinCentres()
    {
        var monitor = new Ft8Monitor();
        var corpus = EncodeCorpus.Build().Take(6).ToList();

        _output.WriteLine("base Hz      recovered   worst margin   what the frequency is");
        var overallWorst = double.MaxValue;
        var totalRecovered = 0;
        var totalSymbols = 0;

        foreach (var (hertz, what) in BaseFrequencies)
        {
            var recovered = 0;
            var total = 0;
            var worst = double.MaxValue;

            foreach (var entry in corpus)
            {
                var result = ToneRecovery.Measure(monitor, entry.Label, entry.Message, hertz, 0);
                recovered += result.Recovered;
                total += result.Total;
                worst = Math.Min(worst, result.WorstMarginDecibels);
            }

            totalRecovered += recovered;
            totalSymbols += total;
            overallWorst = Math.Min(overallWorst, worst);

            _output.WriteLine($"{hertz,10:F4} {recovered,6}/{total,-6} {worst,10:F1} dB   {what}");
            Assert.Equal(total, recovered);
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"across all six frequencies: {totalRecovered} of {totalSymbols} symbols");
        _output.WriteLine($"WORST MARGIN OVER THE WHOLE SWEEP: {overallWorst:F1} dB");
        _output.WriteLine("The halfway-between-bins case is the one that would hide behind a single");
        _output.WriteLine("well-chosen frequency, and it is in the sweep on purpose.");

        Assert.Equal(totalSymbols, totalRecovered);
    }

    /// <summary>
    /// The same measurement across five slot offsets, two of which are not a whole number of blocks
    /// and one of which is not a whole number of sub-blocks either.
    /// </summary>
    /// <remarks>
    /// <b>A finding, reported rather than worked around:</b> <see cref="Ft8Waveform"/> cannot place a
    /// signal at an arbitrary offset within a slot. <c>SynthesizeSlot</c> puts it at
    /// <c>PaddingSampleCount</c> and nowhere else, which at 12 kHz is 14160 samples. So the offset
    /// here is built in the test — the bare signal comes from <c>Synthesize</c> and is copied into a
    /// slot-sized buffer at the chosen index — rather than by changing step 3's proven code.
    /// </remarks>
    [Fact]
    public void TonesAreRecoveredAtOffsetsOnAndOffTheAnalysisGrid()
    {
        var monitor = new Ft8Monitor();
        var geometry = monitor.Geometry;
        var corpus = EncodeCorpus.Build().Take(6).ToList();

        _output.WriteLine("offset  blocks+rem   recovered   worst margin   worst residual   what it is");
        var overallWorst = double.MaxValue;
        var totalRecovered = 0;
        var totalSymbols = 0;

        foreach (var (samples, what) in Offsets)
        {
            var recovered = 0;
            var total = 0;
            var worst = double.MaxValue;
            double worstResidual = 0;

            foreach (var entry in corpus)
            {
                var result = ToneRecovery.Measure(monitor, entry.Label, entry.Message, 1000.0f, samples);
                recovered += result.Recovered;
                total += result.Total;
                worst = Math.Min(worst, result.WorstMarginDecibels);
                foreach (var symbol in result.All)
                {
                    worstResidual = Math.Max(worstResidual, Math.Abs(symbol.Where.ResidualSamples));
                }
            }

            totalRecovered += recovered;
            totalSymbols += total;
            overallWorst = Math.Min(overallWorst, worst);

            _output.WriteLine($"{samples,6}  {samples / geometry.BlockSize,3}+{samples % geometry.BlockSize,-6} "
                + $"{recovered,6}/{total,-6} {worst,10:F1} dB {worstResidual,12:F0} samples   {what}");
            Assert.Equal(total, recovered);
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"across all five offsets: {totalRecovered} of {totalSymbols} symbols");
        _output.WriteLine($"WORST MARGIN OVER THE WHOLE SWEEP: {overallWorst:F1} dB");
        _output.WriteLine("The residual is how far the nearest analysis window's centre misses the");
        _output.WriteLine("symbol's centre. It can be at most half a sub-block, 480 samples, because");
        _output.WriteLine("the alignment is computed and then rounded to the nearest cell.");

        Assert.Equal(totalSymbols, totalRecovered);
    }

    /// <summary>
    /// <b>The half that makes the rest mean anything: noise alone must not recover the tones.</b>
    /// </summary>
    /// <remarks>
    /// The same measurement is run over a slot with no signal in it at all, at the same frequency
    /// and the same offsets. A recovery rate near chance is the answer that says the number above is
    /// a measurement rather than a question with an obvious answer.
    /// </remarks>
    [Fact]
    public void NoiseAloneRecoversTheTonesAtAboutChance()
    {
        var monitor = new Ft8Monitor();
        var corpus = EncodeCorpus.Build().Take(20).ToList();

        var recovered = 0;
        var total = 0;
        var seed = 21305;

        foreach (var entry in corpus)
        {
            var noise = new GaussianNoise(seed++);
            var result = ToneRecovery.Measure(
                monitor, entry.Label, entry.Message, 1000.0f, 0, noise, 0.05, signalPresent: false);
            recovered += result.Recovered;
            total += result.Total;
        }

        var rate = 100.0 * recovered / total;
        var chance = 100.0 / Ft8Waveform.ToneCount;

        // The spread expected of a fair coin with eight sides: three standard deviations of a
        // binomial, as a percentage, so "near chance" is a number rather than a word.
        var deviation = 100.0 * Math.Sqrt(total * (1.0 / 8) * (7.0 / 8)) / total;

        _output.WriteLine("NOISE ALONE — no signal in the slot at all, same frequency, same offset.");
        _output.WriteLine($"symbols asked          : {total}");
        _output.WriteLine($"tones 'recovered'      : {recovered}");
        _output.WriteLine($"RATE                   : {rate:F3} per cent");
        _output.WriteLine($"CHANCE, for 8 candidates: {chance:F3} per cent");
        _output.WriteLine($"one standard deviation : {deviation:F3} per cent");
        _output.WriteLine($"distance from chance   : {Math.Abs(rate - chance) / deviation:F2} standard deviations");
        _output.WriteLine(string.Empty);
        _output.WriteLine("Against 100.000 per cent on the clean signal. The measurement discriminates.");

        Assert.True(
            Math.Abs(rate - chance) < 4 * deviation,
            $"noise alone recovered {rate:F3} per cent against a chance rate of {chance:F3} per cent, "
            + $"which is {Math.Abs(rate - chance) / deviation:F2} standard deviations away. Either the "
            + "noise is not noise or the measurement is finding something that is not there.");
    }

    /// <summary>
    /// <b>Displacement in frequency, measured.</b> A signal at a different base frequency lands in
    /// different bins, and the shift is the one the arithmetic predicts rather than merely non-zero.
    /// </summary>
    [Fact]
    public void ASignalAtADifferentBaseFrequencyLandsInDifferentBins()
    {
        var monitor = new Ft8Monitor();
        var geometry = monitor.Geometry;
        var message = Ft8MonitorTests.PackedFor("CQ with a grid");
        var symbols = Ft8SymbolEncoder.Encode(message);

        var peaks = new List<(float Hertz, int Bin, int Sub, double Decibels)>();
        foreach (var hertz in new[] { 1000.0f, 1500.0f })
        {
            var slot = new float[Ft8Waveform.SlotSampleCount(geometry.SampleRate)];
            Ft8Waveform.Synthesize(symbols, geometry.SampleRate, hertz).AsSpan().CopyTo(slot);
            var waterfall = monitor.Analyse(slot);

            // Symbol 0's block, which is where the first Costas tone sits. The strongest cell over
            // the WHOLE passband in that one block — this is the only place in the unit that looks
            // across the band, and it is a displacement measurement, not a detection: the answer is
            // compared against a frequency already known.
            var where = ToneRecovery.AlignmentFor(geometry, 0, 0);
            var bestBin = -1;
            var bestSub = -1;
            var best = double.NegativeInfinity;

            for (var bin = 0; bin < geometry.BinCount; bin++)
            {
                for (var sub = 0; sub < geometry.FrequencyOversampling; sub++)
                {
                    var decibels = waterfall.DecibelsAt(where.Block, where.TimeSubOffset, sub, bin);
                    if (decibels > best)
                    {
                        best = decibels;
                        bestBin = bin;
                        bestSub = sub;
                    }
                }
            }

            peaks.Add((hertz, bestBin, bestSub, best));
            _output.WriteLine($"base {hertz,8:F2} Hz -> peak at bin {bestBin} sub {bestSub} = "
                + $"{geometry.FrequencyHz(bestBin, bestSub):F4} Hz at {best:F1} dB");
        }

        var lowCells = (peaks[0].Bin * geometry.FrequencyOversampling) + peaks[0].Sub;
        var highCells = (peaks[1].Bin * geometry.FrequencyOversampling) + peaks[1].Sub;
        var shift = highCells - lowCells;
        var predicted = (int)Math.Round((1500.0 - 1000.0) / geometry.TransformBinSpacingHz);

        _output.WriteLine(string.Empty);
        _output.WriteLine($"shift measured  : {shift} cells of {geometry.TransformBinSpacingHz} Hz");
        _output.WriteLine($"shift predicted : {predicted} cells, from 500 Hz / 3.125 Hz");

        Assert.NotEqual(lowCells, highCells);
        Assert.Equal(predicted, shift);

        // And the first Costas tone is tone 3, so the peak should sit three tone spacings up.
        Assert.Equal(3, symbols[0]);
        var expectedLow = 1000.0 + (3 * Ft8Waveform.ToneSpacingHz);
        _output.WriteLine($"first Costas tone is {symbols[0]}, so the peak is expected at "
            + $"{expectedLow:F4} Hz and was found at {geometry.FrequencyHz(peaks[0].Bin, peaks[0].Sub):F4} Hz");
        Assert.True(
            Math.Abs(geometry.FrequencyHz(peaks[0].Bin, peaks[0].Sub) - expectedLow) <= geometry.TransformBinSpacingHz,
            "the peak is more than one cell from where the first Costas tone was put.");
    }

    /// <summary>
    /// <b>Displacement in time, measured.</b> A signal shifted in time lands in different blocks, and
    /// the shift is the one the arithmetic predicts.
    /// </summary>
    [Fact]
    public void ASignalShiftedInTimeLandsInDifferentBlocks()
    {
        var monitor = new Ft8Monitor();
        var geometry = monitor.Geometry;
        var message = Ft8MonitorTests.PackedFor("CQ with a grid");
        var symbols = Ft8SymbolEncoder.Encode(message);

        // Where the first Costas tone actually peaks in TIME, found by walking the blocks at the
        // frequency that tone was put at — again a displacement measurement against a known answer,
        // not a search for an unknown one.
        var hertz = 1000.0f + (symbols[0] * Ft8Waveform.ToneSpacingHz);
        Assert.True(geometry.TryBinFor(hertz, out var bin, out var sub));

        var peakBlocks = new List<double>();
        foreach (var offset in new[] { 0, 5 * geometry.BlockSize })
        {
            var slot = new float[Ft8Waveform.SlotSampleCount(geometry.SampleRate)];
            Ft8Waveform.Synthesize(symbols, geometry.SampleRate, 1000.0f).AsSpan().CopyTo(slot.AsSpan(offset));
            var waterfall = monitor.Analyse(slot);

            var bestBlock = -1;
            var bestSub = -1;
            var best = double.NegativeInfinity;
            for (var block = 0; block < waterfall.BlockCount; block++)
            {
                for (var timeSub = 0; timeSub < geometry.TimeOversampling; timeSub++)
                {
                    var decibels = waterfall.DecibelsAt(block, timeSub, sub, bin);
                    if (decibels > best)
                    {
                        best = decibels;
                        bestBlock = block;
                        bestSub = timeSub;
                    }
                }
            }

            var position = bestBlock + ((double)bestSub / geometry.TimeOversampling);
            peakBlocks.Add(position);
            // Tone 3 occurs at many symbol positions, so this peak is wherever that TONE is loudest
            // rather than symbol 0 specifically. That does not weaken the measurement: both signals
            // are measured the same way, so the difference is the displacement and nothing else.
            _output.WriteLine($"offset {offset,6} samples -> energy at the first Costas tone's "
                + "frequency peaks at block "
                + $"{bestBlock} sub {bestSub} (= {position:F2} blocks, {geometry.TimeSeconds(bestBlock, bestSub):F3} s) "
                + $"at {best:F1} dB");
        }

        var measured = peakBlocks[1] - peakBlocks[0];
        _output.WriteLine(string.Empty);
        _output.WriteLine($"shift measured  : {measured:F2} blocks");
        _output.WriteLine("shift predicted : 5.00 blocks, from an offset of 5 x 1920 samples");

        Assert.NotEqual(peakBlocks[0], peakBlocks[1]);
        Assert.Equal(5.0, measured, 6);
    }

    /// <summary>
    /// The margin, in full, so the next unit's correlator has the distribution and not just the
    /// worst case. <b>A bare "it passed" tells a correlator nothing.</b>
    /// </summary>
    [Fact]
    public void TheMarginBetweenTheRightToneAndItsNeighboursIsReportedAsADistribution()
    {
        var monitor = new Ft8Monitor();
        var corpus = EncodeCorpus.Build().Take(12).ToList();
        var margins = new List<double>();
        var syncMargins = new List<double>();
        var dataMargins = new List<double>();

        foreach (var entry in corpus)
        {
            var result = ToneRecovery.Measure(monitor, entry.Label, entry.Message, 1000.0f, 0);
            foreach (var symbol in result.All)
            {
                margins.Add(symbol.MarginDecibels);
                if (Ft8SymbolEncoder.IsSyncSymbol(symbol.Symbol))
                {
                    syncMargins.Add(symbol.MarginDecibels);
                }
                else
                {
                    dataMargins.Add(symbol.MarginDecibels);
                }
            }
        }

        margins.Sort();

        double Percentile(List<double> sorted, double fraction) =>
            sorted[Math.Clamp((int)(fraction * (sorted.Count - 1)), 0, sorted.Count - 1)];

        _output.WriteLine($"symbols measured : {margins.Count} over {corpus.Count} messages");
        _output.WriteLine($"WORST margin     : {margins[0]:F1} dB");
        _output.WriteLine($"1st percentile   : {Percentile(margins, 0.01):F1} dB");
        _output.WriteLine($"5th percentile   : {Percentile(margins, 0.05):F1} dB");
        _output.WriteLine($"median           : {Percentile(margins, 0.50):F1} dB");
        _output.WriteLine($"95th percentile  : {Percentile(margins, 0.95):F1} dB");
        _output.WriteLine($"best             : {margins[^1]:F1} dB");
        _output.WriteLine($"mean             : {margins.Average():F2} dB");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"sync symbols  ({syncMargins.Count,4}): worst {syncMargins.Min():F1} dB, mean {syncMargins.Average():F2} dB");
        _output.WriteLine($"data symbols  ({dataMargins.Count,4}): worst {dataMargins.Min():F1} dB, mean {dataMargins.Average():F2} dB");
        _output.WriteLine(string.Empty);
        _output.WriteLine("The sync symbols are the ones the next unit's Costas correlator uses, so");
        _output.WriteLine("their margin is the one that matters most to it. The store quantises to");
        _output.WriteLine("half a decibel, so every margin here is a multiple of 0.5.");

        Assert.True(margins[0] > 0, $"the worst margin over the corpus was {margins[0]:F1} dB.");
    }
}
