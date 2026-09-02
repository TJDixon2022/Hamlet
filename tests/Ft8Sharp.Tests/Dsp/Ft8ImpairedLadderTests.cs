using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>The three things real air has that the fixture does not.</b> The synthetic signal task 3 walked
/// down the ladder was aligned to the sample, on the frequency grid, alone in the passband and free
/// of drift. <b>A recording is none of those.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>What these tests are really asking, so the result is read correctly.</b> They are <em>not</em>
/// asking whether this port handles offsets — unit 214 showed it finds signals off-grid and unit 216
/// decoded 288 of 288 across an offset sweep at high signal strength. They are asking whether it
/// handles them <b>at the edge of sensitivity</b>, which is an entirely different question and the one
/// that matters, because every message this port is missing on air lives near that edge.
/// </para>
/// <para>
/// <b>Each impairment separately, so the cost of each is its own number.</b> Mixing them would give
/// one number that names nothing.
/// </para>
/// <para>
/// <b>Drift is named and not tested.</b> A drifting transmitter is a fourth thing real air has, it
/// needs a synthesizer that can produce one, and step 3's proven encoder does not — building one
/// tonight would be new DSP nobody has bounded. It is stated here rather than quietly omitted.
/// </para>
/// </remarks>
public class Ft8ImpairedLadderTests
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;
    private const double OnGridHz = 1000.0;

    private readonly ITestOutputHelper _output;

    public Ft8ImpairedLadderTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// <b>5a — off-grid in time.</b> The transmission starts at sample offsets that are not multiples
    /// of the block size, including deliberately awkward ones.
    /// </summary>
    [Fact]
    public void TheLadderWalkedAgainWithTheTransmissionOffTheBlockGrid()
    {
        var messages = SensitivityLadder.Messages();
        var samplesPerSymbol = Ft8Waveform.SamplesPerSymbol(Rate);
        var subBlock = samplesPerSymbol / 2;

        // Not one of these is a multiple of the sub-block size. 5761 misses a block boundary by one
        // sample; 7013 and 3701 are nowhere near either grid; 960 + 1 and 960 + 479 straddle a
        // sub-block; 12345 is the awkward one unit 216 already used.
        var offsets = new[] { 5761, 7013, 3701, subBlock + 1, subBlock + 479, 12345 };

        _output.WriteLine("5a - OFF-GRID IN TIME. The frequency is left exactly on a bin centre so");
        _output.WriteLine("the only thing changed against task 3 is where the transmission starts.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  block size, samples      : {samplesPerSymbol} (one symbol period)");
        _output.WriteLine($"  sub-block size, samples  : {subBlock} (the waterfall oversamples time by 2)");
        _output.WriteLine($"  offsets used             : {string.Join(", ", offsets)}");
        _output.WriteLine("  NOT ONE OF THEM IS A MULTIPLE OF EITHER GRID:");
        foreach (var offset in offsets)
        {
            _output.WriteLine($"    {offset,6}  block remainder {offset % samplesPerSymbol,5}, "
                + $"sub-block remainder {offset % subBlock,4}");
        }

        _output.WriteLine(string.Empty);

        var rungs = SensitivityLadder.Walk(
            messages,
            _ => OnGridHz,
            i => offsets[i % offsets.Length],
            measureAgreement: false);

        _output.WriteLine(SensitivityLadder.Header);
        foreach (var rung in rungs)
        {
            _output.WriteLine(rung.Row());
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  Read this against task 3's ALIGNED column, rung for rung. The rungs and");
        _output.WriteLine("  the seeds are the same, so the only difference is the offset.");

        var totalWrong = rungs.Sum(r => r.Wrong);
        _output.WriteLine($"  WRONG MESSAGES RETURNED: {totalWrong} out of {rungs.Sum(r => r.Offered)}");

        Assert.Equal(0, totalWrong);
    }

    /// <summary>
    /// <b>5b — off-bin in frequency.</b> A quarter and a half bin off the grid, at several places
    /// across the passband rather than only at 1000 Hz.
    /// </summary>
    [Fact]
    public void TheLadderWalkedAgainWithTheTransmissionOffTheFrequencyGrid()
    {
        var messages = SensitivityLadder.Messages();
        var geometry = new Ft8WaterfallGeometry(Rate);
        var bin = geometry.TransformBinSpacingHz;

        // Six places across the passband, every one of them exactly on a bin centre before the
        // fraction is added, so the fraction is the whole of the impairment.
        var bases = new[] { 300.0, 700.0, 1200.0, 1800.0, 2400.0, 2700.0 };
        var fractions = new[] { bin / 4.0, bin / 2.0 };

        double Frequency(int i) => bases[i % bases.Length] + fractions[(i / bases.Length) % fractions.Length];

        _output.WriteLine("5b - OFF-BIN IN FREQUENCY. The offset is left on the block grid so the only");
        _output.WriteLine("thing changed against task 3 is where the lowest tone sits in its bin.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  transform bin spacing : {bin:F4} Hz");
        _output.WriteLine($"  quarter bin           : {bin / 4.0:F4} Hz");
        _output.WriteLine($"  half bin              : {bin / 2.0:F4} Hz  (THE WORST CASE - equidistant "
            + "from two bins)");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"message",8} {"frequency Hz",14} {"bins",10} {"fraction of a bin",18}");
        for (var i = 0; i < messages.Count; i++)
        {
            var frequency = Frequency(i);
            _output.WriteLine($"{i + 1,8} {frequency,14:F4} {frequency / bin,10:F3} "
                + $"{frequency / bin % 1.0,18:F3}");
        }

        _output.WriteLine(string.Empty);

        var rungs = SensitivityLadder.Walk(
            messages,
            Frequency,
            _ => Ft8Waveform.SamplesPerSymbol(Rate) * 3,
            measureAgreement: false);

        _output.WriteLine(SensitivityLadder.Header);
        foreach (var rung in rungs)
        {
            _output.WriteLine(rung.Row());
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  Read this against task 3's ALIGNED column, rung for rung.");

        var totalWrong = rungs.Sum(r => r.Wrong);
        _output.WriteLine($"  WRONG MESSAGES RETURNED: {totalWrong} out of {rungs.Sum(r => r.Offered)}");

        Assert.Equal(0, totalWrong);
    }

    /// <summary>
    /// <b>5c — a populated passband.</b> Twenty simultaneous transmissions walked down the ladder,
    /// the shape <see cref="Ft8SlotDecoderPassbandTests.TwentyOverlappingTransmissionsSurviveSeededNoise"/>
    /// already runs at -10 dB.
    /// </summary>
    /// <remarks>
    /// <b>This task was the unit's named drop candidate and it was NOT dropped.</b> Its condition was
    /// that it may be dropped only if 5a and 5b agree with the aligned ladder within about one rung.
    /// The report states which branch applied and the numbers that decided it.
    /// </remarks>
    [Fact]
    public void TheLadderWalkedAgainWithTwentyTransmissionsSharingTheSlot()
    {
        var corpus = EncodeCorpus.Build();
        var seeds = new[] { 218_001, 218_002, 218_003 };

        _output.WriteLine("5c - A POPULATED PASSBAND. Twenty transmissions summed into one slot, at");
        _output.WriteLine("twenty different frequencies across the band, at five different start");
        _output.WriteLine("offsets, every one at a different fraction of a bin. SUMMED, NOT COPIED -");
        _output.WriteLine("they add to one another, which is what a receiver actually gets.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("The ratio is quoted PER TRANSMISSION, against the power of one of them");
        _output.WriteLine("alone, which is unit 214's and unit 216's convention on this fixture.");
        _output.WriteLine("Quoting it against the whole slot would flatter it by about ten decibels.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"requested",10} {"delivered",11} {"seeds",6} {"offered",8} {"back",6} "
            + $"{"rate %",8} {"EXTRA",6} {"cand",6} {"par",5} {"crc",5} {"txt",5}");

        var totalExtra = 0;

        foreach (var requested in SensitivityLadder.Rungs)
        {
            var delivered = new List<double>();
            var offered = 0;
            var back = 0;
            var extra = 0;
            var candidates = 0;
            var parity = 0;
            var checksum = 0;
            var text = 0;
            var extraTexts = new List<string>();

            foreach (var seed in seeds)
            {
                var (audio, truths, ratio) = Ft8SearchPassbandTests.BuildNoisyPassbandSlot(requested, seed);
                delivered.Add(ratio);

                var result = new Ft8SlotDecoder().Decode(audio);
                candidates += result.CandidateCount;
                parity += result.ParitySatisfiedCount;
                checksum += result.ChecksumPassedCount;
                text += result.BecameTextCount;

                var outstanding = new List<string>();
                for (var i = 0; i < truths.Count; i++)
                {
                    outstanding.Add(Ft8MessageDecoder.Decode(corpus[i % corpus.Count].Message).Text);
                }

                offered += outstanding.Count;

                foreach (var got in result.Texts)
                {
                    var at = outstanding.FindIndex(e => string.Equals(e, got, StringComparison.Ordinal));
                    if (at >= 0)
                    {
                        outstanding.RemoveAt(at);
                        back++;
                    }
                    else
                    {
                        extra++;
                        extraTexts.Add(got);
                    }
                }
            }

            totalExtra += extra;

            _output.WriteLine($"{requested,10:F1} {delivered.Average(),11:F3} {seeds.Length,6} "
                + $"{offered,8} {back,6} {100.0 * back / offered,8:F1} {extra,6} "
                + $"{candidates / (double)seeds.Length,6:F1} {parity / (double)seeds.Length,5:F1} "
                + $"{checksum / (double)seeds.Length,5:F1} {text / (double)seeds.Length,5:F1}");

            foreach (var got in extraTexts)
            {
                _output.WriteLine($"      RETURNED BUT NOT TRANSMITTED: {got}");
            }
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  'EXTRA' is a message returned that nobody in this slot transmitted, and");
        _output.WriteLine("  it is the one thing this project refuses outright. In a crowded slot it");
        _output.WriteLine("  is the number that matters more than the rate.");
        _output.WriteLine($"  RETURNED BUT NOT TRANSMITTED, WHOLE LADDER: {totalExtra}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  Read the rate column against task 3's aligned single-signal column. A");
        _output.WriteLine("  crowded slot is a HARDER problem at the same per-transmission ratio,");
        _output.WriteLine("  because the other nineteen are interference to each of them.");

        Assert.Equal(0, totalExtra);
    }
}
