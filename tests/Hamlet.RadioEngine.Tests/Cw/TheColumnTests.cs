using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE FIRST PROOF** (work instruction 587, section 3.2, HM-DEC-291): the column finder stands a single clean dit and a single
/// clean dah in the generator's noise, and does not stand a noise burst of the same length and mean height with no keyed edges,
/// at 20, 10 and 5 dB, twenty seeds each; then the same under 12 dB of fade at 0.7 Hz. The whole unit turns on this table.
/// </summary>
public sealed class TheColumnTests(ITestOutputHelper output)
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>The note, between two of the finder's lanes.</summary>
    private const double ToneHz = 615;

    /// <summary>A 20 WPM dit and dah, in seconds.</summary>
    private const double Dit = 0.06;
    private const double Dah = 0.18;

    /// <summary>Where the element is keyed, after the band alone.</summary>
    private const double At = CwFixtureGenerator.MessageStartSeconds;

    private const int Rate = CwFixtureGenerator.SampleRate;

    /// <summary>How near a column's edges must sit to the key's, in seconds, to be the element's column.</summary>
    private const double Near = 0.012;

    /// <remarks>
    /// Section 3.2, the whole table: per level and element, columns found of 20 (a column whose edges sit within 12 ms of the key's,
    /// at the pitch), other columns stood in the same three seconds, bursts stood of 20 for a burst with slow edges (five keying
    /// edges long, no keyed edge anywhere) and for a burst switched with the keying edge (a noise top between two keyed edges); then
    /// the same elements under the fade. Asserts the figures section 5 bars: columns found at every level, bursts stood at none at
    /// 10 and 20 dB; the 5 dB figure is reported.
    /// </remarks>
    [Fact]
    public void TheFirstProof()
    {
        var finder = new CwColumnFinder(Rate, CwFixtureGenerator.PassbandLowHz, CwFixtureGenerator.PassbandHighHz);

        output.WriteLine(string.Create(Inv, $"finder: {finder.Lanes} lanes every {CwColumnFinder.LaneSpacingHz:0} Hz, low-pass {CwColumnFinder.LaneBandwidthHz:0} Hz, step {finder.StepSeconds * 1000:0.0} ms, edge {CwColumnFinder.KeyingEdgeMs:0} ms = {finder.EdgeSteps} steps, template transition {finder.TemplateTransition:0.0} steps; figures: height {CwColumnFinder.ColumnHeightDb:0} dB, match {CwColumnFinder.EdgeMatch:0.00}, slowest {CwColumnFinder.EdgeSlowest:0.0}x, flatness {CwColumnFinder.TopFlatness:0.00}, top {CwColumnFinder.TopLeastEdges} edges"));
        output.WriteLine(string.Create(Inv, $"template: {string.Join(" ", finder.Template.Select(v => v.ToString("0.00", Inv)))}"));
        output.WriteLine(string.Empty);
        output.WriteLine("| dB | element | columns found of 20 | other columns stood | slow-edged bursts stood of 20 | keyed-edge bursts stood of 20 |");
        output.WriteLine("|---|---|---|---|---|---|");

        var failures = new List<string>();

        foreach (var db in new[] { 20.0, 10.0, 5.0 })
        {
            foreach (var (name, length) in new[] { ("dit", Dit), ("dah", Dah) })
            {
                int found = 0, other = 0, slow = 0, keyed = 0;

                for (var seed = 1; seed <= 20; seed++)
                {
                    var columns = Columns(Keyed(db, length, seed, fade: false));
                    var hit = columns.Any(c => IsTheElement(c, At, At + length));

                    found += hit ? 1 : 0;
                    other += columns.Count - (hit ? 1 : 0);
                    slow += Columns(Burst(db, length, seed, edgeSeconds: 5 * CwColumnFinder.KeyingEdgeMs / 1000)).Any(c => Overlaps(c, At, At + length)) ? 1 : 0;
                    keyed += Columns(Burst(db, length, seed, edgeSeconds: CwColumnFinder.KeyingEdgeMs / 1000)).Any(c => Overlaps(c, At, At + length)) ? 1 : 0;
                }

                output.WriteLine(string.Create(Inv, $"| {db:0} | {name} | {found} | {other} | {slow} | {keyed} |"));

                if (found == 0)
                {
                    failures.Add(string.Create(Inv, $"no {name} found at {db:0} dB"));
                }

                if (db >= 10 && (slow > 0 || keyed > 0))
                {
                    failures.Add(string.Create(Inv, $"{slow + keyed} bursts stood at {db:0} dB"));
                }
            }
        }

        output.WriteLine(string.Empty);
        output.WriteLine("Under 12 dB of fade at 0.7 Hz, the element placed at twenty points of the cycle:");
        output.WriteLine(string.Empty);
        output.WriteLine("| dB | element | columns found of 20 | other columns stood |");
        output.WriteLine("|---|---|---|---|");

        foreach (var db in new[] { 20.0, 10.0, 5.0 })
        {
            foreach (var (name, length) in new[] { ("dit", Dit), ("dah", Dah) })
            {
                int found = 0, other = 0;

                for (var seed = 1; seed <= 20; seed++)
                {
                    var at = At + ((seed - 1) * 1.4 / 20);
                    var columns = Columns(Keyed(db, length, seed, fade: true, at));
                    var hit = columns.Any(c => IsTheElement(c, at, at + length));

                    found += hit ? 1 : 0;
                    other += columns.Count - (hit ? 1 : 0);
                }

                output.WriteLine(string.Create(Inv, $"| {db:0} | {name} | {found} | {other} |"));
            }
        }

        output.WriteLine(string.Empty);
        output.WriteLine("The band alone, sixty seconds, three seeds: columns stood per minute.");

        var perMinute = new List<int>();

        for (var seed = 31; seed <= 33; seed++)
        {
            var samples = new float[60 * Rate];

            CwFixtureGenerator.ShapedNoise(samples, seed);
            perMinute.Add(Columns(samples).Count);
        }

        output.WriteLine(string.Create(Inv, $"noise alone: {string.Join(", ", perMinute)} columns per minute"));

        Assert.True(failures.Count == 0, string.Join("; ", failures));
    }

    /// <remarks>
    /// Every edge and column the finder judged on one 5 dB dit and one 5 dB slow-edged burst, with the verdict and why by shape,
    /// for the trace. Asserts nothing.
    /// </remarks>
    [Theory]
    [InlineData(5.0, 1, "dit")]
    [InlineData(5.0, 2, "dit")]
    [InlineData(20.0, 1, "dit")]
    [InlineData(10.0, 1, "dah")]
    [InlineData(10.0, 2, "dah")]
    [InlineData(10.0, 3, "dah")]
    [InlineData(10.0, 4, "dah")]
    [InlineData(5.0, 1, "dah")]
    [InlineData(5.0, 2, "dah")]
    [InlineData(10.0, 5, "dit")]
    [InlineData(10.0, 6, "dit")]
    [InlineData(10.0, 7, "dit")]
    [InlineData(10.0, 8, "dit")]
    public void OneDitAndOneBurstTraced(double db, int seed, string element)
    {
        var length = element == "dah" ? Dah : Dit;

        foreach (var (what, samples) in new[] { (element, Keyed(db, length, seed, fade: false)), ("slow-edged burst", Burst(db, length, seed, 5 * CwColumnFinder.KeyingEdgeMs / 1000)), ("keyed-edge burst", Burst(db, length, seed, CwColumnFinder.KeyingEdgeMs / 1000)) })
        {
            var finder = new CwColumnFinder(Rate, CwFixtureGenerator.PassbandLowHz, CwFixtureGenerator.PassbandHighHz) { Trace = [], TraceLane = hz => Math.Abs(hz - ToneHz) <= 25 };

            foreach (var s in samples)
            {
                finder.Push(s);
            }

            output.WriteLine(string.Create(Inv, $"## {what} at {db:0} dB, seed {seed}, keyed {At:0.000}-{At + length:0.000} s: {finder.Found.Count} columns"));

            foreach (var c in finder.Found)
            {
                output.WriteLine(string.Create(Inv, $"  found {c.FromSeconds:0.000}-{c.ToSeconds:0.000} s ({c.LengthMs:0} ms) at {c.PitchHz:0.0} Hz, level {c.LevelDb:0.0} dB, contrast {c.ContrastDb:0.0} dB, {c.Shape}"));
            }

            foreach (var line in finder.Trace!.Where(l => l.Contains("column", StringComparison.Ordinal) || l.Contains("closes", StringComparison.Ordinal) || (double.Parse(l.Split(' ')[0], Inv) > At - 0.05 && double.Parse(l.Split(' ')[0], Inv) < At + length + 0.05)))
            {
                output.WriteLine(line);
            }
        }
    }

    /// <remarks>
    /// A whole 5 dB CQ and a 15 dB CQ read through the chain with columns as the finder, beside two finders. Asserts nothing
    /// (section 3.3: the finder goes to the survey only once the proof stands).
    /// </remarks>
    [Theory]
    [InlineData("cq-18wpm-5db")]
    [InlineData("cq-18wpm-15db")]
    [InlineData("cq-12wpm-5db")]
    public void ACqReadsThroughColumns(string name)
    {
        var audio = WavAudio.Read(Path.Combine(CwFixtures.Folder, "synthetic-cq", name + ".wav"));
        var two = TheOneFinderTests.Read(audio.Samples, audio.SampleRate, 600, 500, _ => { });
        string columns;
        var found = 0;

        using (CwRules.Choose(CwRules.Columns))
        {
            columns = TheOneFinderTests.Read(audio.Samples, audio.SampleRate, 600, 500, _ => { }, (det, _) => found = det.StoodCount);
        }

        output.WriteLine(string.Create(Inv, $"{name}: two finders `{two}`; columns `{columns}` ({found} marks stood)"));
    }

    /// <remarks>
    /// **THE FIGURES ARE MEASURED, NOT GUESSED**: for a grid of shape figures, the dits and dahs found of 20 at 20, 10 and 5 dB, the
    /// slow-edged and keyed-edge bursts stood of 20 at 10 dB, and the columns the band alone stands in thirty seconds. Asserts
    /// nothing; the finder's constants are chosen from this table.
    /// </remarks>
    [Fact]
    public void TheFiguresSwept()
    {
        output.WriteLine("| figures | dit 20/10/5 dB found of 20 | dah 20/10/5 dB found of 20 | 20 dB bursts stood of 20 (slow, keyed) | 10 dB bursts (slow, keyed) | 5 dB bursts (slow, keyed) | band alone, columns per 30 s |");
        output.WriteLine("|---|---|---|---|---|---|---|");

        var noise = new float[30 * Rate];

        CwFixtureGenerator.ShapedNoise(noise, 31);

        var grid = new List<ColumnFigures>();

        foreach (var lane in new[] { 150.0 })
        {
            foreach (var top in new[] { 3 })
            {
                foreach (var height in new[] { 6.0 })
                {
                    foreach (var flat in new[] { 0.4, 0.45, 0.5 })
                    {
                        foreach (var match in new[] { 0.875, 0.9 })
                        {
                            grid.Add(new ColumnFigures(HeightDb: height, EdgeMatch: match, TopFlatness: flat, TopLeastEdges: top, LaneBandwidthHz: lane));
                        }
                    }
                }
            }
        }

        var rows = new string[grid.Count];

        Parallel.For(0, grid.Count, new ParallelOptions { MaxDegreeOfParallelism = 4 }, g =>
        {
            var figures = grid[g];
            string Found(double length) => string.Join("/", new[] { 20.0, 10.0, 5.0 }.Select(db => Enumerable.Range(1, 20).Count(seed => Columns(Keyed(db, length, seed, fade: false), figures).Any(c => IsTheElement(c, At, At + length))).ToString(Inv)));
            string Bursts(double db) => string.Create(Inv, $"{Enumerable.Range(1, 20).Count(seed => Columns(Burst(db, Dit, seed, 5 * CwColumnFinder.KeyingEdgeMs / 1000), figures).Any(c => Overlaps(c, At, At + Dit)))}, {Enumerable.Range(1, 20).Count(seed => Columns(Burst(db, Dit, seed, CwColumnFinder.KeyingEdgeMs / 1000), figures).Any(c => Overlaps(c, At, At + Dit)))}");

            rows[g] = string.Create(Inv, $"| {figures} | {Found(Dit)} | {Found(Dah)} | {Bursts(20)} | {Bursts(10)} | {Bursts(5)} | {Columns(noise, figures).Count} |");
        });

        foreach (var row in rows)
        {
            output.WriteLine(row);
        }
    }

    private static List<CwMark> Columns(float[] samples, ColumnFigures figures)
    {
        var finder = new CwColumnFinder(Rate, CwFixtureGenerator.PassbandLowHz, CwFixtureGenerator.PassbandHighHz, figures);

        foreach (var s in samples)
        {
            finder.Push(s);
        }

        return finder.Found;
    }

    /// <remarks>The bursts that stood at a level, each with every edge and column the finder judged over it. Asserts nothing.</remarks>
    [Theory]
    [InlineData(20.0)]
    [InlineData(10.0)]
    public void TheBurstsThatStood(double db)
    {
        foreach (var (name, length) in new[] { ("dit", Dit), ("dah", Dah) })
        {
            foreach (var edge in new[] { 5 * CwColumnFinder.KeyingEdgeMs / 1000, CwColumnFinder.KeyingEdgeMs / 1000 })
            {
                for (var seed = 1; seed <= 20; seed++)
                {
                    var finder = new CwColumnFinder(Rate, CwFixtureGenerator.PassbandLowHz, CwFixtureGenerator.PassbandHighHz) { Trace = [], TraceLane = _ => true };

                    foreach (var s in Burst(db, length, seed, edge))
                    {
                        finder.Push(s);
                    }

                    var stood = finder.Found.Where(c => Overlaps(c, At, At + length)).ToList();

                    if (stood.Count == 0)
                    {
                        continue;
                    }

                    output.WriteLine(string.Create(Inv, $"## {name}-length burst, edges {edge * 1000:0} ms, {db:0} dB, seed {seed}, burst {At:0.000}-{At + length:0.000} s: stood {string.Join(", ", stood.Select(c => string.Create(Inv, $"{c.FromSeconds:0.000}-{c.ToSeconds:0.000} s at {c.PitchHz:0} Hz level {c.LevelDb:0.0}")))}"));

                    foreach (var line in finder.Trace!.Where(l => double.TryParse(l.Split(' ')[0], NumberStyles.Float, Inv, out var at) && at >= At - 0.03 && at <= At + length + 0.05 && !l.Contains("with no rise open", StringComparison.Ordinal) && !l.Contains("too short over the chaos", StringComparison.Ordinal)))
                    {
                        output.WriteLine(line);
                    }
                }
            }
        }
    }

    /// <remarks>
    /// Section 3.4: W1AW's piece-04 read through the chain with columns as the finder and with two finders, the process's CPU time and
    /// the wall clock for each, the first read thrown away to warm the process. The bar: not more than two finders' plus a quarter.
    /// </remarks>
    [Fact]
    public void TheColumnsCost()
    {
        const string Piece = "w1aw-2026-10-07/piece-04";
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(Piece));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(Piece);

        (double Cpu, double Wall, string Text) Time(bool columns)
        {
            using var on = columns ? CwRules.Choose(CwRules.Columns) : null;
            using var off = columns ? null : CwRules.Off(CwRules.Columns);
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var cpu = process.TotalProcessorTime;
            var wall = System.Diagnostics.Stopwatch.StartNew();
            var (_, text) = TheRecordingsScoreboardTests.ReadLive(audio.Samples, audio.SampleRate, pitch, width);

            process.Refresh();

            return ((process.TotalProcessorTime - cpu).TotalSeconds, wall.Elapsed.TotalSeconds, text);
        }

        Time(false);

        var two = Time(false);
        var columns = Time(true);

        output.WriteLine(string.Create(Inv, $"{Piece}: {audio.Samples.Length / (double)audio.SampleRate:0} s at {audio.SampleRate} Hz, filter {width:0} Hz"));
        output.WriteLine(string.Create(Inv, $"two finders: {two.Cpu:0.00} s CPU, {two.Wall:0.00} s wall"));
        output.WriteLine(string.Create(Inv, $"columns:     {columns.Cpu:0.00} s CPU, {columns.Wall:0.00} s wall; {(columns.Cpu - two.Cpu) / two.Cpu:+0%;-0%} of the chain with two"));
        output.WriteLine("two:     " + two.Text);
        output.WriteLine("columns: " + columns.Text);
    }

    /// <remarks>
    /// **ROUND TWO'S TRACE** (section 4): a stretch of a recording the round's largest loss named, read with columns as the finder.
    /// Every keyed mark of the board's keying map in the stretch, the column delivered for it or none, and where none, every edge and
    /// column the finder judged in the station's lane over that mark and why by shape; then every column delivered over no keyed mark.
    /// Asserts nothing.
    /// </remarks>
    [Theory]
    [InlineData("w1aw-2026-10-07/piece-04", 97, 105, 600)]
    [InlineData("cw-2026-10-02-200157", 0, 8, 662.8)]
    public void TheColumnsTraced(string recording, double from, double to, double pitchHz)
    {
        var map = TheKeyingMapTests.Of(recording);
        var station = map.Stations.OrderBy(s => Math.Abs(s.PitchHz - pitchHz)).First();
        var keyed = station.Marks.Where(m => m.From >= from && m.To <= to).OrderBy(m => m.From).ToList();
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(recording));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(recording);
        var start = Math.Max(0, from - 1);
        var cut = audio.Samples[(int)(start * audio.SampleRate)..Math.Min(audio.Samples.Length, (int)((to + 1) * audio.SampleRate))];
        var trace = new List<string>();
        var letters = new List<(double At, string Text)>();
        IReadOnlyList<CwMark> columns = [];
        string read;

        using (CwRules.Choose(CwRules.Columns))
        {
            read = TheOneFinderTests.Read(cut, audio.SampleRate, pitch, width, det =>
            {
                det.Columns!.Trace = trace;
                det.Columns.TraceLane = hz => Math.Abs(hz - station.PitchHz) <= 30;
            }, (det, _) => columns = det.CandidatesKept.Where(c => c.Column).ToList(), (c, run) => letters.Add((start + run[^1].ToSeconds, c.Text)));
        }

        double Shift(double t) => t + start;

        output.WriteLine(string.Create(Inv, $"## {recording} {from}-{to} s, station {station.PitchHz:0} Hz: {keyed.Count} keyed marks; read `{read}`"));
        output.WriteLine(string.Create(Inv, $"letters: {string.Join(" ", letters.Where(l => l.At >= from && l.At <= to + 1).Select(l => string.Create(Inv, $"{l.Text}@{l.At:0.00}")))}"));
        output.WriteLine(string.Empty);
        output.WriteLine("| keyed mark | ms | column delivered | ms | pitch | level | why, by shape, where none |");
        output.WriteLine("|---|---|---|---|---|---|---|");

        var delivered = columns.Select(c => c with { FromSeconds = Shift(c.FromSeconds), ToSeconds = Shift(c.ToSeconds) }).Where(c => c.ToSeconds >= from && c.FromSeconds <= to).OrderBy(c => c.FromSeconds).ToList();
        var matched = new HashSet<CwMark>();
        int found = 0, missed = 0;

        foreach (var m in keyed)
        {
            var column = delivered.FirstOrDefault(c => Math.Abs(c.FromSeconds - m.From) <= 0.015 && Math.Abs(c.ToSeconds - m.To) <= 0.015 && Math.Abs(c.PitchHz - station.PitchHz) <= 50);

            if (column is not null)
            {
                matched.Add(column);
                found++;
                output.WriteLine(string.Create(Inv, $"| {m.From:0.000}-{m.To:0.000} | {(m.To - m.From) * 1000:0} | {column.FromSeconds:0.000}-{column.ToSeconds:0.000} | {column.LengthMs:0} | {column.PitchHz:0} | {column.LevelDb:0.0} | |"));
                continue;
            }

            missed++;

            var why = trace.Where(l => double.TryParse(l.Split(' ')[0], NumberStyles.Float, Inv, out var at) && Shift(at) >= m.From - 0.02 && Shift(at) <= m.To + 0.04 && !l.Contains("with no rise open", StringComparison.Ordinal))
                .Select(l => string.Create(Inv, $"{Shift(double.Parse(l.Split(' ')[0], Inv)):0.000}{l[l.IndexOf(' ')..]}"))
                .ToList();

            output.WriteLine(string.Create(Inv, $"| {m.From:0.000}-{m.To:0.000} | {(m.To - m.From) * 1000:0} | none | | | | {(why.Count == 0 ? "no edge judged in the lane" : string.Join("; ", why.Select(w => w.Replace("|", "/", StringComparison.Ordinal))))} |"));
        }

        output.WriteLine(string.Empty);
        output.WriteLine(string.Create(Inv, $"keyed marks with their column: {found} of {keyed.Count}; missed {missed}"));

        var extra = delivered.Where(c => !matched.Contains(c)).ToList();

        output.WriteLine(string.Create(Inv, $"columns delivered over no keyed mark of the station, or not matching one: {extra.Count}"));

        foreach (var c in extra)
        {
            var over = keyed.Where(m => m.To > c.FromSeconds && m.From < c.ToSeconds).Select(m => string.Create(Inv, $"{m.From:0.000}-{m.To:0.000}")).ToList();

            output.WriteLine(string.Create(Inv, $"  {c.FromSeconds:0.000}-{c.ToSeconds:0.000} s ({c.LengthMs:0} ms) at {c.PitchHz:0} Hz, level {c.LevelDb:0.0} dB{(over.Count > 0 ? $", over keyed {string.Join(" ", over)}" : ", over no keyed mark")}"));
        }
    }

    private static bool IsTheElement(CwMark c, double from, double to)
        => Math.Abs(c.FromSeconds - from) <= Near && Math.Abs(c.ToSeconds - to) <= Near && Math.Abs(c.PitchHz - ToneHz) <= 50;

    /// <summary>Whether a column is the burst's: more than half of it lies inside the burst's own span.</summary>
    private static bool Overlaps(CwMark c, double from, double to)
        => Math.Min(c.ToSeconds, to) - Math.Max(c.FromSeconds, from) > 0.5 * (c.ToSeconds - c.FromSeconds);

    private static List<CwMark> Columns(float[] samples)
    {
        var finder = new CwColumnFinder(Rate, CwFixtureGenerator.PassbandLowHz, CwFixtureGenerator.PassbandHighHz);

        foreach (var s in samples)
        {
            finder.Push(s);
        }

        return finder.Found;
    }

    /// <summary>One element keyed by the generator, with its 5 ms raised-cosine edges, in its band at the signal, three seconds long.</summary>
    internal static float[] Keyed(double db, double length, int seed, bool fade, double at = At)
    {
        var recipe = new CwFixtureRecipe(
            "proof", "E", DitMilliseconds: Dit * 1000, DahMilliseconds: Dah * 1000, SignalToNoiseDb: db, ToneHz: ToneHz, DriftHz: 0,
            QsbHz: fade ? CwFixtureCatalogue.QsbHz : 0, QsbDepthDb: fade ? CwFixtureCatalogue.QsbDepthDb : 0, Seed: seed);
        var audio = CwFixtureGenerator.GenerateFromEdges(recipe, [at, at + length, at + length + 1.0, at + length + 1.0]);

        return audio.Samples;
    }

    /// <summary>
    /// A burst of the band's own noise, from another seed, at the element's place and length, its mean envelope in a 150 Hz lane at
    /// the pitch equal to the tone's at this signal, switched with raised-cosine edges of the length given: five keying edges for a
    /// burst with no keyed edge, one for a noise top between two keyed edges.
    /// </summary>
    internal static float[] Burst(double db, double length, int seed, double edgeSeconds)
    {
        var samples = new float[(int)((At + length + 1.0) * Rate)];
        var noiseRms = CwFixtureGenerator.ShapedNoise(samples, seed);
        var amplitude = Math.Sqrt(2) * noiseRms * Math.Pow(10, db / 20);
        var burst = new float[samples.Length];

        CwFixtureGenerator.ShapedNoise(burst, seed ^ 0x2C01);

        // The band noise's power inside the lane, from a sum over the lane's width; the burst is scaled so its mean envelope there,
        // the Rayleigh mean of its two quadratures, equals the tone's A/2.
        var inLane = LanePower(burst, ToneHz - CwColumnFinder.LaneBandwidthHz, ToneHz + CwColumnFinder.LaneBandwidthHz);
        var gain = amplitude / 2 / (Math.Sqrt(inLane / 4) * Math.Sqrt(Math.PI / 2));

        for (var i = 0; i < samples.Length; i++)
        {
            var t = (double)i / Rate;
            var rise = Math.Clamp((t - At) / edgeSeconds, 0, 1);
            var fall = Math.Clamp((At + length - t) / edgeSeconds, 0, 1);
            var shape = Math.Min(rise, fall);
            var g = 0.5 * (1 - Math.Cos(Math.PI * shape));

            samples[i] = Math.Clamp(samples[i] + (float)(g * gain * burst[i]), -1f, 1f);
        }

        return samples;
    }

    // The mean power per sample of the signal between two frequencies, by a sum of its spectrum there at 2 Hz steps.
    private static double LanePower(float[] x, double lowHz, double highHz)
    {
        var n = Math.Min(x.Length, 2 * Rate);
        double power = 0;

        for (var f = lowHz; f <= highHz; f += 2)
        {
            var w = 2 * Math.PI * f / Rate;
            double re = 0, im = 0;

            for (var i = 0; i < n; i++)
            {
                re += x[i] * Math.Cos(w * i);
                im -= x[i] * Math.Sin(w * i);
            }

            // One-sided: the band holds both the positive and the negative frequency's share.
            power += 2 * ((re * re) + (im * im)) / ((double)n * n);
        }

        // Each 2 Hz bin of a length-n DFT at Rate stands for Rate/n Hz of spectrum; the sum is scaled to the band's share.
        return power * (2.0 / (Rate / (double)n));
    }
}
