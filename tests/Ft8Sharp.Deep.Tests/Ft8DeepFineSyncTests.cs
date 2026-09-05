using System;
using System.Collections.Generic;
using System.Linq;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Deep.Tests;

/// <summary>
/// <b>The fine search, on synthesised audio with a known sub-grid displacement planted in it.</b>
/// </summary>
/// <remarks>
/// <b>These say the search finds what was planted, not that it is worth anything.</b> What it is
/// worth is a number on step 0's instrument and it is in <c>Ft8Unit248ScoreboardTests</c>.
/// </remarks>
public class Ft8DeepFineSyncTests(ITestOutputHelper output)
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    private const string Text = "HELLO WORLD";

    /// <summary>
    /// <b>A planted displacement is recovered to within the search's own step, over displacements
    /// covering the whole cell.</b>
    /// </summary>
    /// <remarks>
    /// The signal is placed at a frequency and an offset that no grid names, the search is centred
    /// on a position deliberately displaced from it, and the recovered position is compared with the
    /// truth. <b>The tolerance is the search's own step and not a number chosen to pass</b>: a grid
    /// search cannot resolve below its step and claiming otherwise would be claiming more than the
    /// representation allows.
    /// </remarks>
    [Fact]
    public void APlantedDisplacementIsRecoveredToWithinTheSearchsOwnStep()
    {
        var search = new Ft8DeepFineSync();
        var settings = search.Settings;

        output.WriteLine($"  extent  +/-{settings.TimeExtentSeconds:F3} s in "
            + $"{settings.TimeStepSeconds:F4} s steps, "
            + $"+/-{settings.FrequencyExtentHz:F4} Hz in {settings.FrequencyStepHz:F4} Hz steps");
        output.WriteLine($"  {settings.PositionCount} positions a candidate");
        output.WriteLine(string.Empty);
        output.WriteLine("   planted dt   planted df   found dt   found df   time err   freq err  edge");
        output.WriteLine("  ---------------------------------------------------------------------------");

        foreach (var plantedTime in new[] { -0.04, -0.025, -0.01, 0.0, 0.01, 0.025, 0.04 })
        {
            foreach (var plantedFrequency in new[] { -1.5625, -0.8, 0.0, 0.8, 1.5625 })
            {
                // The signal really is here; the search is told the un-displaced position instead.
                const double nominalFrequency = 1000.0;
                const int nominalOffset = 5760;

                var trueFrequency = nominalFrequency + plantedFrequency;
                var trueSeconds = (nominalOffset / (double)Rate) + plantedTime;
                var trueOffset = (int)Math.Round(trueSeconds * Rate);

                var slot = Slot(Text, trueFrequency, trueOffset);
                var baseband = Ft8DeepBaseband.Build(slot, Rate, nominalFrequency);

                var found = search.Search(baseband, nominalOffset / (double)Rate);

                var timeError = found.StartSeconds - (trueOffset / (double)Rate);
                var frequencyError = found.FrequencyOffsetHz - plantedFrequency;

                var edge = found.OnTimeEdge || found.OnFrequencyEdge ? "yes" : "";

                output.WriteLine(
                    $"  {plantedTime,10:F3}   {plantedFrequency,10:F4}   "
                    + $"{found.TimeShiftSeconds,8:F3}   {found.FrequencyShiftHz,8:F4}   "
                    + $"{timeError,8:F4}   {frequencyError,8:F4}  {edge}");

                Assert.True(
                    Math.Abs(timeError) <= settings.TimeStepSeconds + 1e-9,
                    $"planted {plantedTime:F3} s: the search landed {timeError:F4} s away, which is "
                        + $"more than its own {settings.TimeStepSeconds:F4} s step.");

                Assert.True(
                    Math.Abs(frequencyError) <= settings.FrequencyStepHz + 1e-9,
                    $"planted {plantedFrequency:F4} Hz: the search landed {frequencyError:F4} Hz "
                        + $"away, which is more than its own {settings.FrequencyStepHz:F4} Hz step.");
            }
        }
    }

    /// <summary>
    /// <b>And the recovered position decodes where the nominal one does not, once there is noise in
    /// the slot.</b>
    /// </summary>
    /// <remarks>
    /// <b>A clean loud signal decodes at the wrong position too</b>, which is why this test carries
    /// noise: the first version of it read 16 of 16 on both rows and proved nothing. The amplitude
    /// below was chosen once so that the misplaced row is not already saturated, and it is a fixed
    /// draw from a fixed seed so the row is the same in every process. <b>This is not a rate and it
    /// is not evidence of an improvement</b> - that is the scoreboard's, at 306 trials.
    /// </remarks>
    [Fact]
    public void TheRecoveredPositionDecodesWhereTheNominalOneDoesNot()
    {
        var search = new Ft8DeepFineSync();
        const double nominalFrequency = 1000.0;
        const int nominalOffset = 5760;

        var bestRecovered = 0;
        var bestNominal = 0;

        output.WriteLine("   noise sigma   nominal position   after the fine search   of");
        output.WriteLine("  ------------------------------------------------------------");

        foreach (var sigma in new[] { 2.0, 4.0, 6.0, 8.0, 10.0 })
        {
            var recovered = 0;
            var nominalDecoded = 0;
            var total = 0;
            var seed = 248_100;

            foreach (var plantedTime in new[] { -0.04, -0.02, 0.02, 0.04 })
            {
                foreach (var plantedFrequency in new[] { -1.5625, -0.78, 0.78, 1.5625 })
                {
                    var trueFrequency = nominalFrequency + plantedFrequency;
                    var trueOffset =
                        (int)Math.Round(((nominalOffset / (double)Rate) + plantedTime) * Rate);

                    var slot = Noisy(Slot(Text, trueFrequency, trueOffset), sigma, seed++);
                    var baseband = Ft8DeepBaseband.Build(slot, Rate, nominalFrequency);
                    var nominalSeconds = nominalOffset / (double)Rate;

                    total++;

                    if (Decodes(baseband, nominalSeconds, 0.0))
                    {
                        nominalDecoded++;
                    }

                    var found = search.Search(baseband, nominalSeconds);
                    if (Decodes(baseband, found.StartSeconds, found.FrequencyOffsetHz))
                    {
                        recovered++;
                    }
                }
            }

            output.WriteLine(
                $"  {sigma,12:F1}   {nominalDecoded,16}   {recovered,21}   {total,2}");

            if (recovered - nominalDecoded > bestRecovered - bestNominal)
            {
                bestRecovered = recovered;
                bestNominal = nominalDecoded;
            }
        }

        Assert.True(
            bestRecovered > bestNominal,
            $"at no noise level did the fine search recover more than the nominal position did "
                + $"({bestRecovered} against {bestNominal}). If those are equal at every level the "
                + "search is not doing anything here.");
    }

    /// <summary>
    /// Adds a fixed Gaussian draw to a slot. <b>Deterministic</b>: a fixed seed and Box-Muller, so
    /// the row is the same in every process.
    /// </summary>
    private static float[] Noisy(float[] slot, double sigma, int seed)
    {
        var random = new Random(seed);
        var noisy = new float[slot.Length];

        for (var i = 0; i < slot.Length; i++)
        {
            var u1 = 1.0 - random.NextDouble();
            var u2 = random.NextDouble();
            var normal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
            noisy[i] = (float)(slot[i] + (sigma * normal));
        }

        return noisy;
    }

    /// <summary>
    /// <b>Pure noise returns something and does not throw</b>, and so does silence.
    /// </summary>
    /// <remarks>
    /// A live receiver hands over whatever arrived. <b>A search that threw on a slot with nothing in
    /// it would take the band off the air at exactly the moment it mattered.</b>
    /// </remarks>
    [Fact]
    public void PureNoiseAndSilenceReturnSomethingAndDoNotThrow()
    {
        var search = new Ft8DeepFineSync();
        var random = new Random(248_001);

        var noise = new float[Ft8Waveform.SlotSampleCount(Rate)];
        for (var i = 0; i < noise.Length; i++)
        {
            noise[i] = (float)((random.NextDouble() * 2.0) - 1.0);
        }

        foreach (var (what, samples) in new[]
                 {
                     ("pure noise", noise),
                     ("silence", new float[Ft8Waveform.SlotSampleCount(Rate)]),
                     ("nothing at all", Array.Empty<float>()),
                     ("one sample", new float[1]),
                 })
        {
            var baseband = Ft8DeepBaseband.Build(samples, Rate, 1000.0);
            var found = search.Search(baseband, 0.48);

            output.WriteLine(
                $"  {what,-16} score {found.Score,9:F2} dB  dt {found.TimeShiftSeconds,7:F3} s  "
                + $"df {found.FrequencyShiftHz,8:F4} Hz");

            Assert.False(double.IsNaN(found.Score));
        }
    }

    /// <summary>
    /// <b>Deterministic: the same samples give the same offset twice, and in a fresh search.</b>
    /// </summary>
    [Fact]
    public void TheSameSamplesGiveTheSameOffsetTwice()
    {
        var slot = Slot(Text, 1000.9, 5760 + 317);

        var results = new List<Ft8DeepFineSyncResult>();
        for (var i = 0; i < 3; i++)
        {
            var baseband = Ft8DeepBaseband.Build(slot, Rate, 1000.0);
            results.Add(new Ft8DeepFineSync().Search(baseband, 5760 / (double)Rate));
        }

        foreach (var result in results)
        {
            output.WriteLine(
                $"  dt {result.TimeShiftSeconds,7:F4} s  df {result.FrequencyShiftHz,8:F4} Hz  "
                + $"score {result.Score,8:F3} dB");
        }

        Assert.Equal(results[0], results[1]);
        Assert.Equal(results[0], results[2]);
    }

    /// <summary>
    /// <b>A tie leaves the candidate where the coarse search put it.</b> On silence every position
    /// scores the same, and the search must not move for nothing.
    /// </summary>
    [Fact]
    public void ATieLeavesTheCandidateWhereItWas()
    {
        var baseband = Ft8DeepBaseband.Build(
            new float[Ft8Waveform.SlotSampleCount(Rate)], Rate, 1000.0);

        var found = new Ft8DeepFineSync().Search(baseband, 0.48);

        Assert.Equal(0.0, found.TimeShiftSeconds);
        Assert.Equal(0.0, found.FrequencyShiftHz);
        Assert.Equal(0.48, found.StartSeconds);
        Assert.False(found.OnTimeEdge);
        Assert.False(found.OnFrequencyEdge);
    }

    /// <summary>
    /// <b>The default extent covers the whole cell the coarse grid leaves undetermined</b>, which is
    /// step 4's first exit and is read from the geometry rather than from a constant.
    /// </summary>
    [Fact]
    public void TheDefaultExtentCoversTheWholeCoarseCell()
    {
        var geometry = new Ft8WaterfallGeometry(Rate);
        var settings = Ft8DeepFineSyncSettings.Default;

        Assert.True(settings.CoversTheCell(geometry));
        Assert.Equal(0.04, settings.TimeExtentSeconds, 9);
        Assert.Equal(1.5625, settings.FrequencyExtentHz, 9);
        Assert.Equal(8, settings.TimeStepCount);
        Assert.Equal(3, settings.FrequencyStepCount);
        Assert.Equal(119, settings.PositionCount);

        output.WriteLine($"  the cell is +/-{geometry.SubblockSize / (2.0 * geometry.SampleRate):F4} s "
            + $"and +/-{geometry.TransformBinSpacingHz / 2.0:F4} Hz");
        output.WriteLine($"  the search covers +/-{settings.TimeExtentSeconds:F4} s "
            + $"and +/-{settings.FrequencyExtentHz:F4} Hz in {settings.PositionCount} positions");

        // A search narrower than the cell can only report its own edge, and it says so.
        var narrow = new Ft8DeepFineSyncSettings(0.01, 0.005, 0.4, 0.2);
        Assert.False(narrow.CoversTheCell(geometry));

        var refused = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Ft8DeepFineSyncSettings(0.04, 0.05, 1.5625, 0.5));
        output.WriteLine(refused.Message);
    }

    private static bool Decodes(Ft8DeepBaseband baseband, double seconds, double frequencyOffsetHz)
    {
        var ratios = new float[Ft8SoftSymbols.RatioCount];
        Ft8DeepBasebandExtractor.Extract(baseband, seconds, frequencyOffsetHz, ratios);
        Ft8SoftSymbols.Normalise(ratios);

        var result = Ft8Sharp.Ldpc.Ft8CodewordDecoder.Decode(ratios);
        return result.Decoded && result.Message.Text == Text;
    }

    private static float[] Slot(string text, double baseFrequencyHz, int offsetSamples)
    {
        var message = new byte[Ft8Payload.MessageBytes];
        Assert.Equal(Ft8PackResult.Ok, Ft8FreeText.TryPackText(text, message));

        var symbols = Ft8SymbolEncoder.Encode(message);
        var signal = Ft8Waveform.Synthesize(symbols, Rate, (float)baseFrequencyHz);

        var slot = new float[Ft8Waveform.SlotSampleCount(Rate)];
        for (var i = 0; i < signal.Length && offsetSamples + i < slot.Length; i++)
        {
            slot[offsetSamples + i] = signal[i];
        }

        return slot;
    }
}
