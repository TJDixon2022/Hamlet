using System.Diagnostics;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>Unit 296 task 1 — the trace. Where does FT4 actually decode, and how long does one slot
/// take?</b> Nothing under <c>src/</c> moves for this test, and that is the point of it: this is
/// HEAD's starting position, and it cannot be recovered once a default has been moved.
/// </summary>
/// <remarks>
/// <para>
/// <b>WHAT THIS TEST EXISTS TO CATCH, AND IT IS A CLAIM RATHER THAN A CRASH.</b> Step 1's must-pass
/// criteria — the hundred-message round trip and the zero wrong count — were every one of them
/// measured at 1000.0 Hz with <see cref="Ft4Waveform.SynthesizeSlot"/>'s fixed padding, which is a
/// placement exactly on Hamlet's own analysis grid. <c>Ft4SensitivityLadderTests</c> reads
/// <c>const float frequency = 1000.0f</c> at its line 62 and takes the slot from
/// <c>SynthesizeSlot</c> at its line 89. So <b>106 of 106 is a true figure about a signal Hamlet
/// placed for itself</b>, and nothing in the tree measured what happens to a station on 14.080,
/// which arranges itself to no analysis grid at all. The breakage this test catches is the decode
/// rate collapsing away from the grid while every on-grid gate in the suite stays green — which is
/// exactly what <c>Ft4Unit294SnrAgreementTests</c> saw in one cell (17 of 106 at -13 dB) and could
/// not chase.
/// </para>
/// <para>
/// <b>The lattice is <see cref="Ft4Unit296PlacementLattice"/> and its five-by-five shape is argued
/// there.</b> Twenty-five placements spanning one whole tone spacing and one whole symbol period,
/// with exactly one on the analysis grid at any oversampling.
/// </para>
/// </remarks>
public class Ft4Unit296PlacementTraceTests(ITestOutputHelper output)
{
    private const int Rate = Ft4Unit296PlacementLattice.Rate;

    /// <summary>The seed, fixed, so the lattice is a number rather than a range.</summary>
    private const int Seed = 296;

    /// <summary>
    /// <b>The corpus subset, named rather than left implicit.</b> Every third entry of
    /// <c>Ft4RoundTripCorpus</c>'s 106, which is 36 messages spanning the corpus's kinds rather than
    /// a block of one of them. Twenty-five cells times three rungs times 36 messages is 2700 slot
    /// decodes, and it is sized from the clock this same file measures.
    /// </summary>
    private const int SubsetStride = 3;

    /// <summary>
    /// The rungs. <b>-10 and -13 dB are where the on-grid rate is one</b>, so anything below one off
    /// grid is the placement and not the code. -15 is added because on grid it is already 49 of 106
    /// and it brackets the deficit from below.
    /// </summary>
    private static readonly double[] Rungs = [-10.0, -13.0, -15.0];

    /// <summary>
    /// <b>The rungs for the deficit measurement</b> — the on-grid ladder re-taken finely enough to
    /// say in decibels what the worst cell costs. One decibel apart, so the answer is quoted to the
    /// nearest decibel rather than interpolated across a five-decibel gap.
    /// </summary>
    private static readonly double[] DeficitRungs =
        [-10.0, -11.0, -12.0, -13.0, -14.0, -15.0, -16.0, -17.0];

    /// <summary>
    /// <b>Task 1a — the clock. Every later run in this unit is sized from this number.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Two figures, because the tab sees both: a slot carrying one decodable signal, which is the
    /// expensive case because every candidate that passes sync runs the LDPC decoder, and a slot
    /// carrying only noise, which is what the band gives most of the time. <b>The median is
    /// reported rather than the mean</b> — one JIT warm-up or one collection would move a mean and
    /// says nothing about what a slot costs.
    /// </para>
    /// <para>
    /// <b>What this catches.</b> A decode that no longer fits inside a 7.5 second slot. Nothing in
    /// the tree timed one FT4 slot before tonight, so a grid change adopted in task 4 could have
    /// bought sensitivity with a decode the tab cannot run in real time and no test would have
    /// noticed.
    /// </para>
    /// </remarks>
    [Fact]
    public void OneFt4SlotDecodeIsTimedAtHeadsGeometry()
    {
        const int repetitions = 21;
        var corpus = Ft4RoundTripCorpus.Build();
        var decoder = new Ft4SlotDecoder();
        var geometry = decoder.Geometry;

        output.WriteLine("THE CLOCK, AT HEAD'S GEOMETRY");
        output.WriteLine(
            $"  time oversampling {geometry.TimeOversampling}, frequency oversampling "
            + $"{geometry.FrequencyOversampling}");
        output.WriteLine(
            $"  block {geometry.BlockSize} samples, sub-block {geometry.SubblockSize}, transform "
            + $"{geometry.TransformLength}, bins {geometry.BinCount}, blocks {geometry.MaxBlocks}");
        output.WriteLine($"  {repetitions} repetitions each, median reported");
        output.WriteLine(string.Empty);

        var entry = corpus[0];
        var symbols = Ft4SymbolEncoder.Encode(entry.Message);
        var signal = Ft4Waveform.Synthesize(symbols, Rate, (float)Ft4Unit296PlacementLattice.BaseFrequencyHz);
        var clean = Ft4Unit296PlacementLattice.Slot(signal, Ft4Unit296PlacementLattice.BaseLeadSamples);
        var signalPower = SignalToNoise.MeanSquare(signal);
        var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, -10.0, Rate);
        var noise = new GaussianNoise(Seed);
        var withSignal = noise.AddedTo(clean, sigma);
        var noiseOnly = noise.Block(clean.Length, sigma);

        var decodedHere = decoder.Decode(withSignal);
        Assert.Contains(decodedHere.Messages, m => m.Text == entry.Text);

        var withSignalMs = Median(Time(decoder, withSignal, repetitions));
        var noiseOnlyMs = Median(Time(decoder, noiseOnly, repetitions));

        output.WriteLine($"  one signal at -10 dB   median {withSignalMs,8:F1} ms per slot");
        output.WriteLine($"  noise only             median {noiseOnlyMs,8:F1} ms per slot");
        output.WriteLine(string.Empty);
        output.WriteLine(
            $"  the slot is {Ft4Timing.SlotSeconds:F1} s, so one decode is "
            + $"{withSignalMs / (Ft4Timing.SlotSeconds * 1000.0) * 100.0:F2}% of it");
        output.WriteLine(string.Empty);
        output.WriteLine("SIZING EVERY LATER RUN IN THIS UNIT FROM THAT NUMBER");
        var subset = Ft4Unit296PlacementLattice.Subset(corpus, SubsetStride);
        var lattice = Ft4Unit296PlacementLattice.Build();
        var latticeDecodes = lattice.Count * Rungs.Length * subset.Count;
        output.WriteLine(
            $"  task 1b lattice: {lattice.Count} cells x {Rungs.Length} rungs x {subset.Count} "
            + $"messages = {latticeDecodes} decodes, about "
            + $"{latticeDecodes * withSignalMs / 60000.0:F1} minutes");
        output.WriteLine(
            $"  task 1d deficit: {DeficitRungs.Length} rungs x {subset.Count} messages on grid = "
            + $"{DeficitRungs.Length * subset.Count} decodes, about "
            + $"{DeficitRungs.Length * subset.Count * withSignalMs / 60000.0:F1} minutes");

        // A slot decode that does not fit inside a slot is not a decoder the tab can run.
        Assert.True(
            withSignalMs < Ft4Timing.SlotSeconds * 1000.0,
            $"one FT4 slot took {withSignalMs:F1} ms, which is longer than the "
            + $"{Ft4Timing.SlotSeconds:F1} s slot it has to be decoded inside");
    }

    /// <summary>
    /// <b>Task 1b and 1d — the placement lattice at HEAD, and the deficit in decibels.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Three numbers at every rung: <b>the on-grid cell</b>, for continuity with units 289 and 294;
    /// <b>the worst cell of the lattice</b>; and <b>the placement-averaged rate over all twenty-five
    /// cells</b>. The last two are the honest ones — a station on 14.080 draws its placement from
    /// the lattice uniformly and has no reason to land on cell (0,0).
    /// </para>
    /// <para>
    /// <b>Nothing is asserted about the rate.</b> A rate that reads badly is the finding of this
    /// unit rather than a failure of this test. What <em>is</em> asserted is that the lattice really
    /// spans a protocol period and that its one on-grid cell really is on the grid — because an
    /// instrument whose off-grid cells were accidentally on grid would report a false green, and
    /// that is the one way this unit could claim a fix it did not earn.
    /// </para>
    /// </remarks>
    [Fact]
    public void TheFt4DecodeRateIsMeasuredOnAPlacementLatticeAtHead()
    {
        var started = Stopwatch.StartNew();
        var corpus = Ft4RoundTripCorpus.Build();
        var subset = Ft4Unit296PlacementLattice.Subset(corpus, SubsetStride);
        var decoder = new Ft4SlotDecoder();
        var geometry = decoder.Geometry;
        var lattice = Ft4Unit296PlacementLattice.Build();

        AssertTheLatticeIsGridIndependent(geometry, lattice);

        output.WriteLine("THE PLACEMENT LATTICE, AT HEAD'S GEOMETRY");
        output.WriteLine(
            $"  grid              time oversampling {geometry.TimeOversampling}, frequency "
            + $"oversampling {geometry.FrequencyOversampling}");
        output.WriteLine(
            $"  lattice           {Ft4Unit296PlacementLattice.Divisions} x "
            + $"{Ft4Unit296PlacementLattice.Divisions} = {lattice.Count} cells, spanning one whole "
            + $"tone spacing ({Ft4Unit296PlacementLattice.ToneSpacingHz:F4} Hz) and one whole "
            + $"symbol period ({Ft4Unit296PlacementLattice.SymbolPeriodSamples} samples, "
            + $"{Ft4Timing.SymbolPeriodSeconds:F3} s)");
        output.WriteLine(
            $"  corpus subset     every {SubsetStride}rd of Ft4RoundTripCorpus's {corpus.Count} = "
            + $"{subset.Count} messages, each sent once per cell per rung");
        output.WriteLine($"  seed              {Seed}, and the same noise draw at every cell of a rung");
        output.WriteLine("  axis              power in a 2500 Hz reference bandwidth, measured per trial");
        output.WriteLine(string.Empty);

        var rows = new List<Ft4Unit296PlacementLattice.Cell>();

        foreach (var rung in Rungs)
        {
            output.WriteLine($"RUNG {rung:F0} dB");
            output.WriteLine(
                $"{"cell",-8}{"freq Hz",-12}{"lead",-9}{"grid",-7}{"trials",-8}{"decoded",-9}"
                + $"{"WRONG",-7}missed");

            var atRung = new List<Ft4Unit296PlacementLattice.Cell>();

            foreach (var placement in lattice)
            {
                var cell = Ft4Unit296PlacementLattice.Measure(decoder, subset, placement, rung, Seed);
                atRung.Add(cell);
                rows.Add(cell);

                output.WriteLine(
                    $"{placement.Name,-8}{placement.FrequencyHz,-12:F4}{placement.LeadSamples,-9}"
                    + $"{(placement.OnGrid ? "ON" : "off"),-7}{cell.Trials,-8}{cell.Decoded,-9}"
                    + $"{cell.Wrong,-7}{cell.Missed}");
            }

            ReportRung(rung, atRung);
            output.WriteLine(string.Empty);
        }

        output.WriteLine("THE THREE NUMBERS, TOGETHER");
        output.WriteLine($"{"rung",-8}{"on grid",-12}{"worst cell",-22}{"placement-averaged",-22}WRONG");

        foreach (var rung in Rungs)
        {
            var atRung = rows.Where(r => r.Rung == rung).ToList();
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
        output.WriteLine($"WRONG DECODES OVER THE WHOLE LATTICE: {rows.Sum(r => r.Wrong)}");
        output.WriteLine($"MISSED DECODES OVER THE WHOLE LATTICE: {rows.Sum(r => r.Missed)}");
        output.WriteLine("  reported explicitly whether zero or not, per the phase's own ruling");

        output.WriteLine(string.Empty);
        output.WriteLine("TASK 1D - THE DEFICIT IN DECIBELS, NOT ONLY IN MESSAGES");
        output.WriteLine(
            "  the on-grid ladder re-taken a decibel at a time, so the rate the worst cell reaches");
        output.WriteLine("  can be found on it and the gap quoted in dB rather than guessed at");
        output.WriteLine(string.Empty);
        output.WriteLine($"{"rung",-8}{"trials",-8}{"decoded",-9}{"rate",-9}WRONG");

        var onGridCell = lattice.Single(p => p.OnGrid);
        var ladder = new List<Ft4Unit296PlacementLattice.Cell>();

        foreach (var rung in DeficitRungs)
        {
            var cell = Ft4Unit296PlacementLattice.Measure(decoder, subset, onGridCell, rung, Seed);
            ladder.Add(cell);
            output.WriteLine(
                $"{rung,-8:F0}{cell.Trials,-8}{cell.Decoded,-9}{cell.Rate,-9:P1}{cell.Wrong}");
        }

        output.WriteLine(string.Empty);

        foreach (var rung in Rungs)
        {
            var atRung = rows.Where(r => r.Rung == rung).ToList();
            var worst = atRung.OrderBy(r => r.Decoded).First();
            var averaged = atRung.Sum(r => r.Decoded) / (double)atRung.Sum(r => r.Trials);

            output.WriteLine(
                $"  at {rung:F0} dB the worst cell {worst.Placement.Name} reaches "
                + $"{worst.Rate:P1}: {Deficit(ladder, rung, worst.Rate)}");
            output.WriteLine(
                $"  at {rung:F0} dB the lattice average reaches {averaged:P1}: "
                + $"{Deficit(ladder, rung, averaged)}");
        }

        output.WriteLine(string.Empty);
        output.WriteLine($"the whole trace took {started.Elapsed.TotalMinutes:F1} minutes");
    }

    /// <summary>
    /// <b>The assertion that makes the instrument worth anything.</b> Exactly one cell of the
    /// lattice sits on the analysis grid, and the other twenty-four do not — at this geometry and at
    /// every denser one task 3 will try.
    /// </summary>
    private void AssertTheLatticeIsGridIndependent(
        Ft4WaterfallGeometry geometry,
        IReadOnlyList<Ft4Unit296PlacementLattice.Placement> lattice)
    {
        foreach (var (time, frequency) in new[] { (2, 2), (2, 4), (4, 4), (4, 8), (8, 8) })
        {
            var candidate = new Ft4WaterfallGeometry(
                timeOversampling: time,
                frequencyOversampling: frequency);
            // THE BIN SPACING IS THE GEOMETRY'S OWN and not derived from the tone spacing: it is
            // the sample rate over the transform length, which is what the waterfall really does.
            var binHz = candidate.SampleRate / (double)candidate.TransformLength;
            var onGrid = lattice
                .Where(p =>
                    p.LeadSamples % candidate.SubblockSize == 0
                    && Math.Abs(Math.Round(p.FrequencyHz / binHz) - (p.FrequencyHz / binHz)) < 1e-6)
                .ToList();

            Assert.True(
                onGrid.Count == 1 && onGrid[0].OnGrid,
                $"at time oversampling {time} and frequency oversampling {frequency} the lattice "
                + $"has {onGrid.Count} cells on the analysis grid rather than one "
                + $"({string.Join(", ", onGrid.Select(p => p.Name))}). Sub-block "
                + $"{candidate.SubblockSize}, bin {binHz:F9} Hz; cell (0,0) is "
                + $"{lattice[0].FrequencyHz:F9} Hz at lead {lattice[0].LeadSamples}, which is "
                + $"{lattice[0].FrequencyHz / binHz:F9} bins and "
                + $"{lattice[0].LeadSamples % candidate.SubblockSize} samples past a sub-block. "
                + "A lattice whose off-grid "
                + "cells become on-grid when the grid is densified would report a fix nobody "
                + "earned, which is the one way this unit could produce a false green.");
        }

        output.WriteLine("THE INSTRUMENT IS GRID-INDEPENDENT, ASSERTED BEFORE ANYTHING IS MEASURED");
        output.WriteLine(
            "  at 2x2, 2x4, 4x4, 4x8 and 8x8 exactly one of the 25 cells is on the analysis grid,");
        output.WriteLine(
            "  and it is the same cell (0,0) every time. Five divisions, and no power of two");
        output.WriteLine("  divides five.");
        output.WriteLine(
            $"  HEAD's sub-block is {geometry.SubblockSize} samples and its bin is "
            + $"{geometry.SampleRate / (double)geometry.TransformLength:F4} Hz");
        output.WriteLine(string.Empty);
    }

    /// <summary>Prints the three numbers for one rung.</summary>
    private void ReportRung(double rung, IReadOnlyList<Ft4Unit296PlacementLattice.Cell> atRung)
    {
        var onGrid = atRung.Single(r => r.Placement.OnGrid);
        var worst = atRung.OrderBy(r => r.Decoded).First();
        var best = atRung.OrderByDescending(r => r.Decoded).First();
        var trials = atRung.Sum(r => r.Trials);
        var decoded = atRung.Sum(r => r.Decoded);

        output.WriteLine(
            $"  on grid (0,0)        {onGrid.Decoded} of {onGrid.Trials} ({onGrid.Rate:P1}), "
            + $"{onGrid.Wrong} wrong");
        output.WriteLine(
            $"  worst cell {worst.Placement.Name,-8}  {worst.Decoded} of {worst.Trials} "
            + $"({worst.Rate:P1}), {worst.Wrong} wrong");
        output.WriteLine(
            $"  best cell  {best.Placement.Name,-8}  {best.Decoded} of {best.Trials} "
            + $"({best.Rate:P1}), {best.Wrong} wrong");
        output.WriteLine(
            $"  placement-averaged   {decoded} of {trials} ({decoded / (double)trials:P1}), "
            + $"{atRung.Sum(r => r.Wrong)} wrong");
        output.WriteLine(
            $"  delivered SNR        {atRung.Average(r => r.DeliveredMean):F2} dB against "
            + $"{rung:F0} dB requested");
    }

    /// <summary>
    /// <b>How far down the on-grid ladder you must go to find the rate a cell reached.</b> The
    /// answer to task 1d: the deficit in decibels rather than in messages.
    /// </summary>
    private static string Deficit(
        IReadOnlyList<Ft4Unit296PlacementLattice.Cell> ladder, double rung, double rate)
    {
        var reached = ladder
            .Where(c => c.Rate <= rate + 1e-9)
            .OrderByDescending(c => c.Rung)
            .FirstOrDefault();

        if (reached is null)
        {
            return
                $"no rung of the on-grid ladder is as bad as this, so the deficit is more than "
                + $"{rung - ladder.Min(c => c.Rung):F0} dB and this ladder cannot bracket it";
        }

        return reached.Rung >= rung - 1e-9
            ? "the on-grid ladder is no better here, so the deficit is 0 dB"
            : $"the on-grid ladder first falls to it at {reached.Rung:F0} dB, so the deficit is "
                + $"{rung - reached.Rung:F0} dB";
    }

    /// <summary>Times one slot decode, repeatedly, in milliseconds.</summary>
    private static double[] Time(Ft4SlotDecoder decoder, float[] slot, int repetitions)
    {
        var times = new double[repetitions];

        for (var i = 0; i < repetitions; i++)
        {
            var watch = Stopwatch.StartNew();
            decoder.Decode(slot);
            times[i] = watch.Elapsed.TotalMilliseconds;
        }

        return times;
    }

    /// <summary>The median, which one warm-up or one collection cannot move.</summary>
    private static double Median(double[] values)
    {
        var sorted = values.OrderBy(v => v).ToArray();
        return sorted.Length % 2 == 1
            ? sorted[sorted.Length / 2]
            : (sorted[(sorted.Length / 2) - 1] + sorted[sorted.Length / 2]) / 2.0;
    }
}
