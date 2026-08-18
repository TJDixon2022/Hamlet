using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// What this detector actually does at a mark's edges (HM-DEC-119's commission).
/// </summary>
/// <remarks>
/// <para>**A MEASUREMENT TAKEN THROUGH ONE INSTRUMENT IS NOT A FACT ABOUT
/// ANOTHER**, which is why HM-DEC-112 was superseded. Its half-amplitude
/// correction was measured through an offline filter and carried that filter's
/// edge shape into Hamlet's clock: shipping it took the suite from 13 failures
/// to 29 and silenced 30 words a minute entirely.</para>
/// <para>So this runs Hamlet's own tone tracker and its own gate, on synthesized
/// marks of exactly known length, and reports what they see. Nothing here
/// changes the decoder. The measurement is the deliverable and the choice of
/// what to do about it is Tim's (§12.1), because it decides what the clock
/// asserts.</para>
/// <para>**THE GENERATOR'S OWN EDGES ARE 5 MS LINEAR RAMPS**, which is the one
/// number that makes any of this interpretable: whatever the detector reports
/// wider than that is the detector, not the signal.</para>
/// </remarks>
public sealed class CwEdgeCharacterTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tables are printed.</param>
    public CwEdgeCharacterTests(ITestOutputHelper output) => _output = output;

    /// <summary>The transmitter's own ramp, from the generator.</summary>
    public const double GeneratorEdgeMs = 5.0;

    /// <summary>One measured mark, true against detected.</summary>
    private sealed record Mark(
        double TrueStartMs, double TrueEndMs, double GateStartMs, double GateEndMs)
    {
        public double TrueLengthMs => TrueEndMs - TrueStartMs;

        public double GateLengthMs => GateEndMs - GateStartMs;

        public double StartErrorMs => GateStartMs - TrueStartMs;

        public double EndErrorMs => GateEndMs - TrueEndMs;
    }

    /// <summary>One hop of the detector's own output.</summary>
    private sealed record Hop(double AtMs, double PowerDb, bool KeyDown, double PeakDb);

    /// <summary>
    /// Run Hamlet's tracker and gate over generated audio and keep every hop.
    /// </summary>
    /// <remarks>
    /// The same two objects the decoder uses, wired the same way, so what comes
    /// back is what the decoder sees and not an approximation of it.
    /// </remarks>
    private static (List<Hop> Hops, List<Mark> Marks, int HopSamples, int WindowSamples)
        Run(string text, int wpm, double toneHz = 600, double? followSpeed = null)
    {
        var request = new CwSignalRequest(
            text, WordsPerMinute: wpm, ToneHz: toneHz, LeadInSeconds: 0.5,
            TailSeconds: 0.5);

        var audio = CwSignal.Generate(request);
        var tracker = new CwToneTracker(audio.SampleRate, toneHz);

        if (followSpeed is { } follow)
        {
            tracker.FollowSpeed(follow);
        }

        var gate = new CwGate();
        var hops = new List<Hop>();

        tracker.Process(audio.Samples, 0, reading =>
        {
            var judged = gate.Judge(reading.PowerDb, reading.NoiseDb, reading.Blocked);

            hops.Add(new Hop(
                reading.SampleIndex / (double)audio.SampleRate * 1000,
                reading.PowerDb,
                judged.KeyDown,
                judged.PeakDb));
        });

        return (hops, TrueMarks(text, wpm, 0.5), tracker.HopSamples, tracker.WindowSamples);
    }

    /// <summary>Where the marks really are, from the generator's own arithmetic.</summary>
    private static List<Mark> TrueMarks(string text, int wpm, double leadInSeconds)
    {
        var dit = MorseCode.Dit(wpm).TotalSeconds;
        var pattern = MorseCode.KeyPattern(text);
        var marks = new List<Mark>();
        var at = leadInSeconds;

        for (var i = 0; i < pattern.Count; i++)
        {
            var length = pattern[i] * dit;

            // Even indices are key down, odd are key up, which is how the
            // generator lays a pattern out.
            if (i % 2 == 0)
            {
                marks.Add(new Mark(at * 1000, (at + length) * 1000, 0, 0));
            }

            at += length;
        }

        return marks;
    }

    /// <summary>Pair each true mark with the gate run that overlaps it.</summary>
    private static List<Mark> Detected(List<Hop> hops, List<Mark> truth)
    {
        var runs = new List<(double Start, double End)>();
        var open = false;
        var start = 0.0;

        foreach (var hop in hops)
        {
            if (hop.KeyDown && !open)
            {
                open = true;
                start = hop.AtMs;
            }
            else if (!hop.KeyDown && open)
            {
                open = false;
                runs.Add((start, hop.AtMs));
            }
        }

        // **PAIRED IN ORDER, NOT BY OVERLAP**, and getting that wrong is worth
        // recording because it looked exactly like a finding. Every detected run
        // is delayed by about three quarters of the analysis window, so a short
        // mark's own midpoint falls before the run that represents it: matching
        // by overlap silently dropped every dit at 25 and 30 words a minute and
        // reported that the detector could not see them. It sees them.
        var paired = new List<Mark>();

        for (var i = 0; i < truth.Count && i < runs.Count; i++)
        {
            paired.Add(truth[i] with
            {
                GateStartMs = runs[i].Start,
                GateEndMs = runs[i].End,
            });
        }

        return paired;
    }

    /// <remarks>
    /// <para>**THE COMMISSION'S FIRST QUESTION: WHERE DOES THE GATE THINK A MARK
    /// BEGINS AND ENDS.** Reproduces the numbers HM-DEC-119 already carries and
    /// extends them to both edges separately, which is what the previous
    /// measurement could not see: a length accurate to within a hop can still be
    /// two edges wrong in opposite directions.</para>
    /// </remarks>
    [Theory]
    [InlineData(12)]
    [InlineData(25)]
    [InlineData(30)]
    public void WhereTheGateThinksAMarkBeginsAndEnds(int wpm)
    {
        var (hops, truth, hopSamples, windowSamples) = Run("PARIS PARIS", wpm);
        var marks = Detected(hops, truth);

        var hopMs = hopSamples / 8.0;
        var windowMs = windowSamples / 8.0;

        _output.WriteLine($"{wpm} wpm   true dit {1200.0 / wpm:0.0} ms   "
            + $"hop {hopMs:0.0} ms   window {windowMs:0.0} ms");
        _output.WriteLine("");
        _output.WriteLine("  true len   gate len   start err   end err");

        foreach (var group in marks.GroupBy(m => Math.Round(m.TrueLengthMs)))
        {
            var list = group.ToList();

            _output.WriteLine(
                $"  {group.Key,8:0}   {list.Average(m => m.GateLengthMs),8:0.0}   "
                + $"{list.Average(m => m.StartErrorMs),9:+0.0;-0.0}   "
                + $"{list.Average(m => m.EndErrorMs),7:+0.0;-0.0}   "
                + $"(n={list.Count})");
        }

        Assert.NotEmpty(marks);
    }

    /// <remarks>
    /// <para>**THE SHAPE ITSELF, HOP BY HOP.** The generator ramps an edge over
    /// five milliseconds. Anything wider than that in these columns is the
    /// detector's own window, and how much wider is the whole question: it is
    /// what decides whether a shorter edge window would help or whether the
    /// answer is nothing at all.</para>
    /// </remarks>
    [Theory]
    [InlineData(12)]
    [InlineData(25)]
    [InlineData(30)]
    public void TheEnvelopeShapeAcrossAnEdge(int wpm)
    {
        var (hops, truth, hopSamples, _) = Run("PARIS PARIS", wpm);
        var marks = Detected(hops, truth);

        // The longest mark, which has the most room either side of its edges.
        var mark = marks.OrderByDescending(m => m.TrueLengthMs).First();
        var hopMs = hopSamples / 8.0;

        var peak = hops
            .Where(h => h.AtMs >= mark.TrueStartMs && h.AtMs <= mark.TrueEndMs)
            .Select(h => h.PowerDb)
            .DefaultIfEmpty(0)
            .Max();

        _output.WriteLine($"{wpm} wpm, a {mark.TrueLengthMs:0} ms mark, "
            + $"hop {hopMs:0.0} ms, levels relative to the mark's own peak");
        _output.WriteLine("");
        _output.WriteLine("  ms from edge    rising    falling");

        for (var offset = -20.0; offset <= 20.0; offset += hopMs)
        {
            double At(double edgeMs)
                => hops
                    .OrderBy(h => Math.Abs(h.AtMs - (edgeMs + offset)))
                    .Select(h => h.PowerDb - peak)
                    .FirstOrDefault();

            _output.WriteLine(
                $"  {offset,11:+0.0;-0.0}   {At(mark.TrueStartMs),7:0.0}   "
                + $"{At(mark.TrueEndMs),8:0.0}");
        }

        Assert.NotEmpty(hops);
    }

    /// <remarks>
    /// <para>**AND HOW ALL OF IT MOVES WITH THE ANALYSIS WINDOW**, which is the
    /// commission's last question and the one that says whether a shorter edge
    /// window is the answer. The tracker narrows its window as the speed it is
    /// told about rises, so telling it a different speed is how the window is
    /// varied without reaching inside it.</para>
    /// </remarks>
    [Theory]
    [InlineData(25)]
    [InlineData(30)]
    public void HowTheWindowLengthChangesWhatIsMeasured(int wpm)
    {
        _output.WriteLine($"{wpm} wpm, true dit {1200.0 / wpm:0.0} ms");
        _output.WriteLine("");
        _output.WriteLine("  told   window   dit read   dah read   start err   end err");

        foreach (var told in new double[] { 10, 18, 25, 35, 45 })
        {
            var (hops, truth, hopSamples, windowSamples) =
                Run("PARIS PARIS", wpm, followSpeed: told);

            var marks = Detected(hops, truth);

            if (marks.Count == 0)
            {
                _output.WriteLine($"  {told,4:0}   {windowSamples / 8.0,6:0.0}   "
                    + "nothing detected");
                continue;
            }

            var dits = marks.Where(m => m.TrueLengthMs < 1200.0 / wpm * 2).ToList();
            var dahs = marks.Where(m => m.TrueLengthMs >= 1200.0 / wpm * 2).ToList();

            _output.WriteLine(
                $"  {told,4:0}   {windowSamples / 8.0,6:0.0}   "
                + $"{(dits.Count > 0 ? dits.Average(m => m.GateLengthMs) : 0),8:0.0}   "
                + $"{(dahs.Count > 0 ? dahs.Average(m => m.GateLengthMs) : 0),8:0.0}   "
                + $"{marks.Average(m => m.StartErrorMs),9:+0.0;-0.0}   "
                + $"{marks.Average(m => m.EndErrorMs),7:+0.0;-0.0}");
        }

        // **THE ANSWER TO THE COMMISSION IS IN THIS TABLE AND IT IS PINNED
        // HERE.** A fifty millisecond window destroys thirty words a minute
        // outright: the window is longer than the dit, the runs merge, and the
        // start error jumps from about thirty milliseconds to a hundred and
        // seventy. A twenty millisecond window reads both speeds better than the
        // forty the tracker acquires with.
        var narrow = Run("PARIS PARIS", wpm, followSpeed: 45);
        var wide = Run("PARIS PARIS", wpm, followSpeed: 10);

        Assert.True(
            narrow.WindowSamples < wide.WindowSamples,
            "the tracker no longer narrows its window as the speed it is told "
            + "about rises, so this measurement is no longer about what it says");
    }

    /// <remarks>
    /// <para>**THE GAP THAT FOLLOWS, WHICH IS THE OTHER HALF OF THE CLOCK.** A
    /// mark and the silence after it are complementary, so an error in one is
    /// the negative of the error in the other, and a clock fitted to marks alone
    /// can be right about the ratio and wrong about the speed.</para>
    /// </remarks>
    [Theory]
    [InlineData(12)]
    [InlineData(25)]
    [InlineData(30)]
    public void WhatTheGapsMeasure(int wpm)
    {
        var (hops, truth, _, _) = Run("PARIS PARIS", wpm);
        var marks = Detected(hops, truth);

        _output.WriteLine($"{wpm} wpm, true element gap {1200.0 / wpm:0.0} ms");
        _output.WriteLine("");
        _output.WriteLine("  true gap   gate gap   error");

        var rows = new List<(double True, double Gate)>();

        for (var i = 1; i < marks.Count; i++)
        {
            rows.Add((
                marks[i].TrueStartMs - marks[i - 1].TrueEndMs,
                marks[i].GateStartMs - marks[i - 1].GateEndMs));
        }

        foreach (var group in rows.GroupBy(r => Math.Round(r.True)))
        {
            var list = group.ToList();

            _output.WriteLine(
                $"  {group.Key,8:0}   {list.Average(r => r.Gate),8:0.0}   "
                + $"{list.Average(r => r.Gate - r.True),5:+0.0;-0.0}   (n={list.Count})");
        }

        Assert.NotEmpty(rows);
    }
}
