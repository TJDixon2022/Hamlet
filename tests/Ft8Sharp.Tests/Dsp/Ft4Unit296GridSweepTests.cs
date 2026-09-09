using System.Diagnostics;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>Unit 296 task 3 — the cheap experiment, and it is a measurement rather than a build.</b>
/// <c>Ft4WaterfallGeometry</c> already takes <c>timeOversampling</c> and
/// <c>frequencyOversampling</c> and <c>Ft4SlotDecoder</c> already takes a geometry, so a denser
/// analysis grid costs zero lines of source to try. <b>Nothing under <c>src/</c> changes for any of
/// these tests.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>WHAT THIS CATCHES, AND IT IS THE ONE WAY THIS UNIT COULD PRODUCE A FALSE GREEN.</b> A denser
/// grid makes more hypotheses, and more hypotheses is exactly how a decoder starts inventing
/// messages. The phase's own ruling is that a wrong decode is counted separately from a missed one
/// everywhere, and a grid that buys sensitivity with a wrong decode is refused whatever it does to
/// the rate. So every cell here reports its wrong count apart from its missed count, and the sweep
/// reports the per-slot decode time beside the rate, because a grid that decodes beautifully in
/// nine seconds is not a fix for a 7.5 second slot.
/// </para>
/// <para>
/// <b>The lattice is task 1b's, unchanged</b> — <see cref="Ft4Unit296PlacementLattice"/>, five
/// divisions a side so that no power-of-two oversampling turns an off-grid cell into an on-grid
/// one. Measuring a densified grid at unit 294's fixed cell centre and calling it closed is exactly
/// what this instrument exists to make impossible.
/// </para>
/// <para>
/// <b>Each grid is its own <c>[Fact]</c> rather than one theory</b>, so a session can run them one
/// at a time inside the status cadence's twelve minutes rather than backgrounding one long sweep.
/// </para>
/// <para>
/// <b>WHAT THE SWEEP FOUND, AND IT IS THE OPPOSITE OF WHAT WAS EXPECTED ON ONE AXIS.</b> The whole
/// table is in <c>docs/unit296-runs/grid-sweep.txt</c>. <b>Frequency oversampling above 2 destroys
/// the decode, and it destroys the <em>on-grid</em> one first</b> — 2x4 and 4x4 both fall from 36
/// of 36 to 0 of 36 on grid at -13 dB — which is not what an off-grid problem looks like. The
/// reason is in <see cref="Ft8Monitor.ProcessBlock"/> and is read out of the port rather than
/// guessed: the analysis frame is <c>BlockSize × FrequencyOversampling</c> samples long, so
/// <b>frequency oversampling lengthens the analysis window</b> — at 2 the transform spans two whole
/// FT4 symbol periods and at 4 it spans four. FT4 is 4-FSK and its tone changes every symbol, so a
/// window spanning four symbols sees four different tones and the Costas correlation is smeared
/// away. <b>Time oversampling costs nothing of the kind</b>: it changes only how often the
/// same-length frame is sampled, so it buys sub-symbol alignment resolution with no smearing — and
/// that is exactly the axis the off-grid loss lives on. At 4x2 the placement average at -13 dB goes
/// from 808 of 900 to 894 of 900 and the worst cell from 19 of 36 to 34 of 36, while the on-grid
/// cell does not move.
/// </para>
/// <para>
/// <b>None of that is a defect in the port.</b> <c>freq_osr</c> is a parameter of
/// <c>monitor_init</c> and <c>demo/decode_ft8.c</c> passes 2; 2 is the largest value that does not
/// span more than one FT4 symbol change. Upstream's number is right on the frequency axis and
/// improvable on the time one.
/// </para>
/// </remarks>
public class Ft4Unit296GridSweepTests(ITestOutputHelper output)
{
    private const int Rate = Ft4Unit296PlacementLattice.Rate;

    /// <summary>The seed, and it is task 1b's, so the two runs are comparable trial for trial.</summary>
    private const int Seed = 296;

    /// <summary>Every third entry of the corpus's 106 — task 1b's own subset.</summary>
    private const int SubsetStride = 3;

    /// <summary>Task 1b's rungs, unchanged.</summary>
    private static readonly double[] Rungs = [-10.0, -13.0, -15.0];

    /// <summary>
    /// <b>The clock at every grid, and the refusal boundary measured rather than trusted.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>Ft8WaterfallGeometry</c> refuses a <c>timeOversampling</c> that does not divide the block
    /// size, and FT4's block is 576 samples. 576 is 2^6 × 3^2, so 2, 3, 4, 6 and 8 divide it and 5
    /// and 7 do not. <b>The instruction said to measure the refusal rather than trust the line</b>,
    /// and that is what this does — including catching a refusal where none was expected, which
    /// would silently take a grid out of the sweep.
    /// </para>
    /// <para>
    /// <b>Frequency oversampling is not bounded by the block at all</b> — it multiplies the
    /// transform length rather than dividing the block — so it is measured too, and separately.
    /// </para>
    /// </remarks>
    [Fact]
    public void EveryCandidateGridIsTimedAndTheRefusalsAreMeasured()
    {
        const int repetitions = 11;
        var corpus = Ft4RoundTripCorpus.Build();
        var entry = corpus[0];
        var symbols = Ft4SymbolEncoder.Encode(entry.Message);
        var signal = Ft4Waveform.Synthesize(symbols, Rate, (float)Ft4Unit296PlacementLattice.BaseFrequencyHz);
        var clean = Ft4Unit296PlacementLattice.Slot(signal, Ft4Unit296PlacementLattice.BaseLeadSamples);
        var signalPower = SignalToNoise.MeanSquare(signal);
        var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, -13.0, Rate);
        var slot = new GaussianNoise(Seed).AddedTo(clean, sigma);

        output.WriteLine("THE REFUSAL BOUNDARY, MEASURED AND NOT TRUSTED");
        output.WriteLine($"  FT4's block is {new Ft4WaterfallGeometry().BlockSize} samples");

        foreach (var time in new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 16 })
        {
            try
            {
                var geometry = new Ft4WaterfallGeometry(timeOversampling: time);
                output.WriteLine(
                    $"  timeOversampling {time,2}  accepted, sub-block {geometry.SubblockSize}");
            }
            catch (ArgumentException refused)
            {
                output.WriteLine(
                    $"  timeOversampling {time,2}  REFUSED: {refused.Message.Split('.')[0]}.");
            }
        }

        foreach (var frequency in new[] { 1, 2, 3, 4, 5, 8, 16 })
        {
            try
            {
                var geometry = new Ft4WaterfallGeometry(frequencyOversampling: frequency);
                output.WriteLine(
                    $"  freqOversampling {frequency,2}  accepted, transform "
                    + $"{geometry.TransformLength}, bin "
                    + $"{geometry.SampleRate / (double)geometry.TransformLength:F4} Hz");
            }
            catch (ArgumentException refused)
            {
                output.WriteLine(
                    $"  freqOversampling {frequency,2}  REFUSED: {refused.Message.Split('.')[0]}.");
            }
        }

        output.WriteLine(string.Empty);
        output.WriteLine("THE CLOCK AT EVERY GRID THE SWEEP WILL USE");
        output.WriteLine(
            $"{"grid",-8}{"sub-block",-11}{"transform",-11}{"bin Hz",-10}{"blocks",-8}"
            + $"{"ms/slot",-10}{"x HEAD",-9}sweep");

        var headMs = 0.0;
        var subset = Ft4Unit296PlacementLattice.Subset(corpus, SubsetStride).Count;
        var cells = Ft4Unit296PlacementLattice.Build().Count;

        foreach (var (time, frequency) in Grids)
        {
            var geometry = new Ft4WaterfallGeometry(
                timeOversampling: time, frequencyOversampling: frequency);
            var decoder = new Ft4SlotDecoder(geometry);

            decoder.Decode(slot);

            var times = new double[repetitions];

            for (var i = 0; i < repetitions; i++)
            {
                var watch = Stopwatch.StartNew();
                decoder.Decode(slot);
                times[i] = watch.Elapsed.TotalMilliseconds;
            }

            var median = times.OrderBy(t => t).ToArray()[repetitions / 2];

            if (headMs == 0.0)
            {
                headMs = median;
            }

            var sweepMinutes = cells * Rungs.Length * subset * median / 60000.0;

            output.WriteLine(
                $"{time + "x" + frequency,-8}{geometry.SubblockSize,-11}{geometry.TransformLength,-11}"
                + $"{geometry.SampleRate / (double)geometry.TransformLength,-10:F4}"
                + $"{geometry.MaxBlocks,-8}{median,-10:F1}{median / headMs,-9:F2}"
                + $"{sweepMinutes:F1} min");
        }

        output.WriteLine(string.Empty);
        output.WriteLine(
            $"the slot is {Ft4Timing.SlotSeconds:F1} s; the budget this unit judges a grid against");
        output.WriteLine(
            "is stated in the sweep tests and is not this test's to set");
    }

    /// <summary>The grids the sweep runs, HEAD first.</summary>
    private static IReadOnlyList<(int Time, int Frequency)> Grids =>
        [(2, 2), (2, 4), (4, 4), (4, 8)];

    /// <summary>HEAD's own grid, re-run here so the sweep carries its own control row.</summary>
    [Fact]
    public void TheLatticeAtGrid2x2() => Sweep(2, 2);

    /// <summary>Twice the frequency resolution, the same time resolution.</summary>
    [Fact]
    public void TheLatticeAtGrid2x4() => Sweep(2, 4);

    /// <summary>
    /// <b>Twice the time resolution and HEAD's frequency resolution — the axis isolated.</b>
    /// </summary>
    /// <remarks>
    /// Not in the work instruction's list, and added because <see cref="TheLatticeAtGrid2x4"/>
    /// answered a question nobody had asked: densifying <em>frequency</em> alone does not improve
    /// the off-grid rate, it destroys the on-grid one. A sweep that only ever moved both axes
    /// together could not say which one did that, and this unit's whole product is a measurement.
    /// </remarks>
    [Fact]
    public void TheLatticeAtGrid4x2() => Sweep(4, 2);

    /// <summary>Twice both.</summary>
    [Fact]
    public void TheLatticeAtGrid4x4() => Sweep(4, 4);

    /// <summary>Twice the time and four times the frequency.</summary>
    [Fact]
    public void TheLatticeAtGrid4x8() => Sweep(4, 8);

    /// <summary>
    /// Runs task 1b's whole lattice at one analysis grid and prints the three numbers, the wrong
    /// count and the time.
    /// </summary>
    private void Sweep(int time, int frequency)
    {
        var started = Stopwatch.StartNew();
        var corpus = Ft4RoundTripCorpus.Build();
        var subset = Ft4Unit296PlacementLattice.Subset(corpus, SubsetStride);
        var geometry = new Ft4WaterfallGeometry(
            timeOversampling: time, frequencyOversampling: frequency);
        var decoder = new Ft4SlotDecoder(geometry);
        var lattice = Ft4Unit296PlacementLattice.Build();

        output.WriteLine($"THE PLACEMENT LATTICE AT {time}x{frequency}");
        output.WriteLine(
            $"  grid              time oversampling {geometry.TimeOversampling}, frequency "
            + $"oversampling {geometry.FrequencyOversampling}");
        output.WriteLine(
            $"  sub-block         {geometry.SubblockSize} samples; bin "
            + $"{geometry.SampleRate / (double)geometry.TransformLength:F4} Hz; transform "
            + $"{geometry.TransformLength}; blocks {geometry.MaxBlocks}");
        output.WriteLine(
            $"  lattice           {lattice.Count} cells, task 1b's own, spanning one whole tone "
            + $"spacing and one whole symbol period");
        output.WriteLine(
            $"  corpus subset     every {SubsetStride}rd of {corpus.Count} = {subset.Count} "
            + $"messages, seed {Seed}");
        output.WriteLine(string.Empty);
        output.WriteLine(
            $"{"rung",-8}{"on grid",-12}{"worst cell",-22}{"placement-averaged",-22}WRONG");

        var rows = new List<Ft4Unit296PlacementLattice.Cell>();

        foreach (var rung in Rungs)
        {
            var atRung = lattice
                .Select(p => Ft4Unit296PlacementLattice.Measure(decoder, subset, p, rung, Seed))
                .ToList();

            rows.AddRange(atRung);

            var onGrid = atRung.Single(r => r.Placement.OnGrid);
            var worst = atRung.OrderBy(r => r.Decoded).First();
            var averaged = atRung.Sum(r => r.Decoded) / (double)atRung.Sum(r => r.Trials);

            output.WriteLine(
                $"{rung,-8:F0}{onGrid.Decoded + " of " + onGrid.Trials,-12}"
                + $"{worst.Decoded + " of " + worst.Trials + " at " + worst.Placement.Name,-22}"
                + $"{atRung.Sum(r => r.Decoded) + " of " + atRung.Sum(r => r.Trials) + $" ({averaged:P1})",-22}"
                + $"{atRung.Sum(r => r.Wrong)}");
        }

        output.WriteLine(string.Empty);
        output.WriteLine("EVERY CELL, SO A GRID CANNOT HIDE ONE BAD PLACEMENT IN AN AVERAGE");

        foreach (var rung in Rungs)
        {
            output.WriteLine(
                $"  {rung,5:F0} dB  "
                + string.Join(" ", rows
                    .Where(r => r.Rung == rung)
                    .Select(r => $"{r.Decoded,3}")));
        }

        output.WriteLine(string.Empty);
        output.WriteLine($"WRONG DECODES AT THIS GRID: {rows.Sum(r => r.Wrong)}");
        output.WriteLine($"MISSED DECODES AT THIS GRID: {rows.Sum(r => r.Missed)}");

        var slots = rows.Sum(r => r.Trials);

        output.WriteLine(string.Empty);
        output.WriteLine(
            $"{slots} slots in {started.Elapsed.TotalMinutes:F1} minutes, which is "
            + $"{started.Elapsed.TotalMilliseconds / slots:F1} ms a slot including synthesis and "
            + "noise - the decode alone is timed by "
            + nameof(EveryCandidateGridIsTimedAndTheRefusalsAreMeasured));
    }
}
