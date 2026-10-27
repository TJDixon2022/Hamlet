using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE FIRST PROOF** (work instruction 588, section 3.1, HM-DEC-292): the sequence reader's port read on the prototype's own cases,
/// generated as <c>synth.py</c> generates them, and on the capture the prototype read. The port must match the prototype's reads
/// before it goes near the survey; where it does not, the port is wrong, not the method.
/// </summary>
public sealed class TheSequenceReaderTests(ITestOutputHelper output)
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    private const int Rate = CwFixtureGenerator.SampleRate;

    /// <summary>The prototype's note.</summary>
    private const double ToneHz = 615;

    /// <summary>What the 18 WPM cases send: the fist bank's ragchew, a CQ and an exchange, 60 letters.</summary>
    private const string Ragchew = CwFistBank.Text;

    /// <summary>One of the prototype's cases: its speed, signal, fade, dah ratio and jitter, and the lane it was read in.</summary>
    internal sealed record Case(string Name, double Wpm, double Db, bool Fades, double Ratio, double Jitter, double LaneHz, double DitMs, int Least);

    /// <summary>The prototype's cases (section 3.1), with the reads its table gives as the least letters right of 60: whole, whole but one letter at 0 dB, and about 52 for the fist; the 5 dB fading case it read whole on one run is held whole but one at three seeds.</summary>
    internal static readonly Case[] Cases =
    [
        new("18 WPM, 10 dB flat", 18, 10, false, 3.0, 0, 10, 1200.0 / 18, 60),
        new("18 WPM, 5 dB flat", 18, 5, false, 3.0, 0, 10, 1200.0 / 18, 60),
        new("18 WPM, 0 dB flat", 18, 0, false, 3.0, 0, 10, 1200.0 / 18, 59),
        new("18 WPM, 10 dB fading", 18, 10, true, 3.0, 0, 10, 1200.0 / 18, 60),
        new("18 WPM, 5 dB fading", 18, 5, true, 3.0, 0, 10, 1200.0 / 18, 59),
        new("12 WPM fist, dah 1.7, 15% jitter, 20 dB", 12, 20, false, 1.7, 0.15, 30, 100, 52),
    ];

    /// <summary><c>synth.key</c>: a dit and a dah of the ratio, each element and gap spread by the jitter, letter gaps three dits, word gaps seven.</summary>
    internal static double[] Edges(string text, double wpm, double ratio, double jitter, int seed)
    {
        var random = new Random(seed);
        var dit = 1.2 / wpm;
        var at = CwFixtureGenerator.MessageStartSeconds;
        var edges = new List<double>();
        double J(double x) => x * (1 + (jitter * ((2 * random.NextDouble()) - 1)));

        var words = text.Split(' ');

        for (var w = 0; w < words.Length; w++)
        {
            for (var l = 0; l < words[w].Length; l++)
            {
                var code = Code(words[w][l]);

                for (var e = 0; e < code.Length; e++)
                {
                    edges.Add(at);
                    at += J(code[e] == '-' ? ratio * dit : dit);
                    edges.Add(at);

                    at += e + 1 < code.Length ? J(dit) : l + 1 < words[w].Length ? J(3 * dit) : J(7 * dit);
                }
            }
        }

        return edges.ToArray();
    }

    /// <summary><c>synth.make</c>'s rate.</summary>
    internal const int SynthRate = 12_000;

    /// <summary>
    /// A case's audio exactly as <c>synth.make</c> builds it: the key gated by a Hann window of twice 5 ms, the tone at the signal over
    /// Gaussian noise band-passed 350 to 870 Hz by fourth-order Butterworths and scaled to one, the fade ±half its depth at 0.7 Hz, the
    /// whole scaled under full scale. The band-pass is a fourth-order high-pass and low-pass in cascade, where scipy designs the band
    /// at once: over a 2.5 to 1 band both are flat across it, so the signal in the passband means the same.
    /// </summary>
    internal static float[] Synth(Case c, int seed)
    {
        var edges = Edges(Ragchew, c.Wpm, c.Ratio, c.Jitter, seed);
        var n = (int)((edges[^1] + 1) * SynthRate) + SynthRate;
        var g = new double[n];

        for (var k = 0; k + 1 < edges.Length; k += 2)
        {
            for (var i = (int)(edges[k] * SynthRate); i < (int)(edges[k + 1] * SynthRate) && i < n; i++)
            {
                g[i] = 1;
            }
        }

        // np.convolve(g, hanning(2e+1)/sum, 'same').
        var e = (int)(0.005 * SynthRate);
        var kernel = Enumerable.Range(0, (2 * e) + 1).Select(i => 0.5 - (0.5 * Math.Cos(2 * Math.PI * i / (2 * e)))).ToArray();
        var sum = kernel.Sum();
        var keyed = new double[n];

        for (var i = 0; i < n; i++)
        {
            var acc = 0.0;

            for (var k = -e; k <= e; k++)
            {
                var j = i + k;

                if (j >= 0 && j < n)
                {
                    acc += g[j] * kernel[k + e];
                }
            }

            keyed[i] = acc / sum;
        }

        var random = new Random(seed + 100);
        var noise = new double[n];

        for (var i = 0; i < n; i += 2)
        {
            var u1 = 1 - random.NextDouble();
            var u2 = random.NextDouble();
            var m = Math.Sqrt(-2 * Math.Log(u1));

            noise[i] = m * Math.Cos(2 * Math.PI * u2);

            if (i + 1 < n)
            {
                noise[i + 1] = m * Math.Sin(2 * Math.PI * u2);
            }
        }

        foreach (var (hz, high, q) in new[] { (350.0, true, 0.5412), (350.0, true, 1.3066), (870.0, false, 0.5412), (870.0, false, 1.3066) })
        {
            Biquad(noise, hz, high, q);
        }

        var rms = Math.Sqrt(noise.Average(v => v * v));
        var amp = Math.Sqrt(2) * Math.Pow(10, c.Db / 20);
        var x = new double[n];

        for (var i = 0; i < n; i++)
        {
            var t = i / (double)SynthRate;
            var tone = Math.Sin(2 * Math.PI * ToneHz * t) * keyed[i];

            if (c.Fades)
            {
                tone *= Math.Pow(10, CwFixtureCatalogue.QsbDepthDb / 2 * Math.Sin(2 * Math.PI * CwFixtureCatalogue.QsbHz * t) / 20);
            }

            x[i] = (amp * tone) + (noise[i] / rms);
        }

        var peak = x.Max(Math.Abs) * 1.1;

        return x.Select(v => (float)(v / peak)).ToArray();
    }

    // One RBJ biquad, high- or low-pass, in place.
    private static void Biquad(double[] x, double hz, bool high, double q)
    {
        var w0 = 2 * Math.PI * hz / SynthRate;
        var alpha = Math.Sin(w0) / (2 * q);
        var cos = Math.Cos(w0);
        var a0 = 1 + alpha;
        var (b0, b1, b2) = high ? ((1 + cos) / 2, -(1 + cos), (1 + cos) / 2) : ((1 - cos) / 2, 1 - cos, (1 - cos) / 2);
        double x1 = 0, x2 = 0, y1 = 0, y2 = 0;

        for (var i = 0; i < x.Length; i++)
        {
            var y = ((b0 * x[i]) + (b1 * x1) + (b2 * x2) - (-2 * cos * y1) - ((1 - alpha) * y2)) / a0;

            x2 = x1;
            x1 = x[i];
            y2 = y1;
            y1 = y;
            x[i] = y;
        }
    }

    /// <summary>A case's audio by the tree's own generator: its keying over the band at its signal and fade, no drift.</summary>
    internal static MonoAudio Audio(Case c, int seed)
    {
        var recipe = new CwFixtureRecipe(
            c.Name, Ragchew, DitMilliseconds: 1200 / c.Wpm, DahMilliseconds: c.Ratio * 1200 / c.Wpm, SignalToNoiseDb: c.Db, ToneHz: ToneHz, DriftHz: 0,
            QsbHz: c.Fades ? CwFixtureCatalogue.QsbHz : 0, QsbDepthDb: c.Fades ? CwFixtureCatalogue.QsbDepthDb : 0, Seed: 7000 + seed);

        return CwFixtureGenerator.GenerateFromEdges(recipe, Edges(Ragchew, c.Wpm, c.Ratio, c.Jitter, seed));
    }

    /// <remarks>
    /// Section 3.1: each of the prototype's cases at three seeds, read by the port at the prototype's own pitch, lane and dit guess,
    /// three passes; the letters right of 60 by the boards' yardstick, each pass's learning and the pattern. Then the band alone, the
    /// pattern the port finds where nothing is. Asserts each case reaches the prototype's read at every seed, the 0 dB and 5 dB fading cases whole but one letter and the fist 52 of 60, as its table reads them, and that the
    /// band alone is not Morse.
    /// </remarks>
    [Fact]
    public void TheFirstProof()
    {
        var failures = new List<string>();

        output.WriteLine("| case | lane | seed | right of 60 (the tree's generator) | pattern | last pass learned | read |");
        output.WriteLine("|---|---|---|---|---|---|---|");

        foreach (var c in Cases)
        {
            var met = 0;

            for (var seed = 1; seed <= 3; seed++)
            {
                var reading = CwSequenceReader.Read(Synth(c, seed), SynthRate, ToneHz, c.LaneHz, c.DitMs);
                var right = TheRecordingsScoreboardTests.Right(reading.Text, Ragchew);
                var tree = Audio(c, seed);
                var treeRight = TheRecordingsScoreboardTests.Right(CwSequenceReader.Read(tree.Samples, tree.SampleRate, ToneHz, c.LaneHz, c.DitMs).Text, Ragchew);

                met += right >= c.Least ? 1 : 0;
                output.WriteLine(string.Create(Inv, $"| {c.Name} | {c.LaneHz:0} Hz | {seed} | {right} ({treeRight}) | {reading.Pattern} | {(reading.Passes.Count > 0 ? reading.Passes[^1] : null)} | `{reading.Text.Trim()}` |"));
            }

            if (met < 3)
            {
                failures.Add(string.Create(Inv, $"{c.Name}: {met} of 3 seeds reached {c.Least}"));
            }
        }

        output.WriteLine(string.Empty);

        for (var seed = 31; seed <= 33; seed++)
        {
            var reading = CwSequenceReader.Read(Synth(Cases[0] with { Db = -300 }, seed), SynthRate, ToneHz, 10, 1200.0 / 18);

            output.WriteLine(string.Create(Inv, $"the band alone, seed {seed}: {reading.Pattern}; `{Cut(reading.Text)}`"));

            if (reading.Pattern.IsMorse)
            {
                failures.Add(string.Create(Inv, $"the band alone at seed {seed} is Morse"));
            }
        }

        Assert.True(failures.Count == 0, string.Join("; ", failures));
    }

    /// <summary>The capture the prototype read: Ken's QSO with KB2UZK/M.</summary>
    private const string Capture = "cw-2026-10-19-141708";

    /// <remarks>
    /// Section 3.2: <c>cw-2026-10-19-141708</c> at 500 Hz in 60 s stretches, as the prototype read it (lane 10 Hz, dit guess 100 ms):
    /// 0 to 60 s holds N3AFS's report and QTH, 120 to 300 s KB2UZK/M's, read as one stretch as the prototype's README has it. Asserts the name, the QTH, the report, the call's <c>/M</c> and Ken's call, letters only as the boards score; reports the three of the prototype's reads the port does not make: <c>589</c>, <c>NEW KENSINGTON</c> (read <c>NEW D ENS ENBTON</c>) and <c>KB2UZK/M</c> (read <c>KB■ZY/M</c>).
    /// </remarks>
    [Fact]
    public void TheCaptureReads()
    {
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(Capture));
        var texts = new List<string>();

        foreach (var (from, to) in new[] { (0, 60), (120, 300) })
        {
            var a = Math.Min(audio.Samples.Length, from * audio.SampleRate);
            var b = Math.Min(audio.Samples.Length, to * audio.SampleRate);
            var reading = CwSequenceReader.Read(audio.Samples.AsSpan(a, b - a), audio.SampleRate, 500, 10, 100);

            texts.Add(reading.Text);
            output.WriteLine(string.Create(Inv, $"{from}-{to} s: {reading.Pattern}; {(reading.Passes.Count > 0 ? reading.Passes[^1] : null)}"));
            output.WriteLine("  " + reading.Text.Trim());
        }

        // Letters, as the boards score them: the spaces are dropped on both sides.
        var all = string.Concat(texts).Replace(" ", string.Empty, StringComparison.Ordinal);
        var wanted = new[] { "NICETOMEET", "579", "MOBILE", "BRYAN", "CHICOPEE", "/M", "KENN3AFS" };
        var notRead = new[] { "589", "NEWKENSINGTON", "KB2UZK/M" };
        var missing = wanted.Where(w => !all.Contains(w, StringComparison.Ordinal)).ToList();

        output.WriteLine(missing.Count == 0 ? "every phrase asserted reads" : "missing: " + string.Join(", ", missing));
        output.WriteLine("of the prototype's reads, not read by the port: " + string.Join(", ", notRead.Where(w => !all.Contains(w, StringComparison.Ordinal))));
        Assert.Empty(missing);
    }

    /// <remarks>The capture's first minute, N3AFS, read at lanes of 8 to 20 Hz and dit guesses of 70 and 100 ms. Asserts nothing.</remarks>
    [Fact]
    public void TheCapturesFirstMinuteSwept()
    {
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(Capture));

        foreach (var (from, to, dit) in new[] { (0, 60, 100.0), (0, 30, 100), (20, 60, 100), (0, 90, 100), (240, 300, 70), (260, 300, 70), (250, 300, 100), (180, 300, 70) })
        {
            var cut = audio.Samples.AsSpan(from * audio.SampleRate, (to - from) * audio.SampleRate).ToArray();
            var reading = CwSequenceReader.Read(cut, audio.SampleRate, 500, 10, dit);

            output.WriteLine(string.Create(Inv, $"{from}-{to} s, dit {dit:0}: {reading.Pattern}; `{reading.Text.Trim()}`"));
        }

        var samples = audio.Samples.AsSpan(0, 60 * audio.SampleRate).ToArray();

        foreach (var lane in new[] { 8.0, 10, 12, 15, 20 })
        {
            foreach (var dit in new[] { 70.0, 100 })
            {
                foreach (var pitch in new[] { 490.0, 500, 510 })
                {
                    var reading = CwSequenceReader.Read(samples, audio.SampleRate, pitch, lane, dit);

                    output.WriteLine(string.Create(Inv, $"{pitch:0} Hz, lane {lane:0} Hz, dit {dit:0} ms: {reading.Pattern}; `{reading.Text.Trim()}`"));
                }
            }
        }
    }

    /// <remarks>
    /// A recording's lane decoded window by window as the live reader decodes it, from the dit guess: the guess, the pattern, what the
    /// last pass learned and the text, at the live lane and at 10 Hz. Asserts nothing.
    /// </remarks>
    [Theory]
    [InlineData("captured/cw-2026-10-19-141708", 500.0, 0, 120)]
    [InlineData("captured/cw-2026-10-02-200157", 675.0, 0, 30)]
    [InlineData("captured/cw-2026-10-08-121357", 800.0, 0, 30)]
    public void TheLaneWindowsTraced(string file, double pitch, int from, int to)
    {
        var audio = WavAudio.Read(Path.Combine(CwFixtures.Folder, file + ".wav"));

        foreach (var lane in CwSequenceReader.LaneWidthsHz.Append(10))
        {
            var db = CwSequenceReader.Envelope(audio.Samples, audio.SampleRate, pitch, lane);
            var hops = (int)(CwSequenceReader.WindowSeconds * 1000 / CwSequenceReader.HopMs);

            for (var end = from + 6; end <= to; end += 6)
            {
                var b = Math.Min(db.Length, (int)(end * 1000 / CwSequenceReader.HopMs));
                var a = Math.Max(0, b - hops);
                var span = db.AsSpan(a, b - a);
                var guess = CwSequenceReader.GuessDitHops(span);
                var reading = CwSequenceReader.ReadCold(span);

                output.WriteLine(string.Create(Inv, $"lane {lane:0} Hz, {a * 0.005:0}-{b * 0.005:0} s: guess {guess * 5} ms; {reading.Pattern}; {(reading.Passes.Count > 0 ? reading.Passes[^1] : null)}; `{reading.Text.Trim()}`"));
            }
        }
    }

    /// <remarks>
    /// The live reader through the app's chain, rule chosen, on the proof's cases and the weak CQs, with its trace; and W1AW's piece-04
    /// with the process's CPU time beside two finders (section 2.8). Asserts nothing.
    /// </remarks>
    [Theory]
    [InlineData("18 WPM, 10 dB flat")]
    [InlineData("18 WPM, 5 dB fading")]
    [InlineData("12 WPM fist, dah 1.7, 15% jitter, 20 dB")]
    public void TheLiveReaderReads(string name)
    {
        var c = Cases.Single(x => x.Name == name);
        var audio = Audio(c, 1);
        var trace = new List<string>();
        string text;

        using (CwRules.Choose(CwRules.SequenceReader))
        {
            text = Live(audio.Samples, audio.SampleRate, 600, 500, trace);
        }

        output.WriteLine(string.Create(Inv, $"{name}: {TheRecordingsScoreboardTests.Right(text, Ragchew)} of 60: `{text}`"));

        foreach (var line in trace)
        {
            output.WriteLine(line);
        }
    }

    /// <remarks>The live reader on a synthetic CQ and on recordings, with its trace. Asserts nothing.</remarks>
    [Theory]
    [InlineData("synthetic-cq/cq-18wpm-5db", 600.0, 500.0)]
    [InlineData("synthetic-cq/cq-12wpm-15db", 600.0, 500.0)]
    [InlineData("captured/cw-2026-10-19-141708", 0.0, 0.0)]
    [InlineData("captured/cw-2026-10-02-200157", 0.0, 0.0)]
    [InlineData("captured/w1aw-2026-10-07/piece-04", 0.0, 0.0)]
    [InlineData("captured/w1aw-2026-10-07/piece-03", 0.0, 0.0)]
    public void TheLiveReaderReadsAFile(string file, double pitch, double width)
    {
        var path = Path.Combine(CwFixtures.Folder, file + ".wav");
        var audio = WavAudio.Read(path);

        if (pitch == 0)
        {
            (pitch, width) = TheRecordingsScoreboardTests.RadioState(file.Replace("captured/", string.Empty, StringComparison.Ordinal));
        }

        var trace = new List<string>();
        string text;

        var wall = System.Diagnostics.Stopwatch.StartNew();

        using (CwRules.Choose(CwRules.SequenceReader))
        {
            text = Live(audio.Samples, audio.SampleRate, pitch, width, trace);
        }

        output.WriteLine(string.Create(Inv, $"{file} at {pitch:0} Hz, {width:0} Hz wide, {audio.Samples.Length / (double)audio.SampleRate:0} s read in {wall.Elapsed.TotalSeconds:0.0} s: `{text}`"));

        foreach (var line in trace)
        {
            output.WriteLine(line);
        }
    }

    /// <remarks>
    /// Section 2.8: W1AW's piece-04, 283 s, read through the chain with the sequence reader and with two finders, the process's CPU
    /// time and the wall clock for each, the first read thrown away to warm the process. Asserts nothing; the bar is the report's.
    /// </remarks>
    [Fact]
    public void TheSequenceReaderCost()
    {
        const string Piece = "w1aw-2026-10-07/piece-04";
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(Piece));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(Piece);

        (double Cpu, double Wall, string Text) Time(bool sequence)
        {
            using var on = sequence ? CwRules.Choose(CwRules.SequenceReader) : null;
            using var off = sequence ? null : CwRules.Off(CwRules.SequenceReader);
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var cpu = process.TotalProcessorTime;
            var wall = System.Diagnostics.Stopwatch.StartNew();
            var (_, text) = TheRecordingsScoreboardTests.ReadLive(audio.Samples, audio.SampleRate, pitch, width);

            process.Refresh();

            return ((process.TotalProcessorTime - cpu).TotalSeconds, wall.Elapsed.TotalSeconds, text);
        }

        Time(true);

        var sequence = Time(true);
        var two = Time(false);

        output.WriteLine(string.Create(Inv, $"{Piece}: {audio.Samples.Length / (double)audio.SampleRate:0} s at {audio.SampleRate} Hz, filter {width:0} Hz"));
        output.WriteLine(string.Create(Inv, $"two finders:     {two.Cpu:0.00} s CPU, {two.Wall:0.00} s wall"));
        output.WriteLine(string.Create(Inv, $"sequence reader: {sequence.Cpu:0.00} s CPU, {sequence.Wall:0.00} s wall"));
        output.WriteLine("two:      " + two.Text);
        output.WriteLine("sequence: " + sequence.Text);
    }

    /// <remarks>
    /// **THE TRACE, FIRST** (work instruction 589, section 2): the reader's path against the truth on the twelve-plus-three's worst
    /// stretches under the reader and W1AW's piece-03, one row per letter that differs: the edit, what was sent and what the path laid,
    /// when, the learned rhythm the letter was committed under, and the reader's own lines between that letter and the one before it
    /// (holds, lets go, letters cut by a window's start, marks silenced under the midpoint). Asserts nothing.
    /// </remarks>
    [Theory]
    [InlineData("cw-2026-10-08-121414")]
    [InlineData("cw-2026-10-08-121324")]
    [InlineData("cw-2026-10-03-221548")]
    [InlineData("cw-2026-10-02-200157")]
    [InlineData("cw-2026-10-03-144020")]
    [InlineData("w1aw-2026-10-07/piece-02")]
    [InlineData("w1aw-2026-10-07/piece-03")]
    public void TheReaderAgainstTheTruth(string recording)
    {
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(recording));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(recording);
        var trace = new List<string>();
        IReadOnlyList<TheRecordingsScoreboardTests.Printed> letters;

        using (CwRules.Choose(CwRules.SequenceReader))
        {
            letters = ReadLiveTraced(audio.Samples, audio.SampleRate, pitch, width, trace);
        }

        var w1aw = recording.StartsWith("w1aw", StringComparison.Ordinal);
        var stretches = (w1aw ? TheW1awTableTests.Stretches : TheRecordingsScoreboardTests.Stretches).Where(s => s.Recording == recording).ToList();
        var lines = trace.Select(l => (At: double.Parse(l[..l.IndexOf(' ', StringComparison.Ordinal)], Inv), Line: l)).ToList();

        output.WriteLine(string.Create(Inv, $"## {recording} at {pitch:0} Hz, {width:0} Hz wide"));

        foreach (var s in stretches)
        {
            var mine = letters.Where(l => w1aw
                ? l.Seconds >= s.From && l.Seconds < s.To
                : Math.Abs(s.PitchHz - l.PitchHz) <= 60 && l.Seconds >= s.From - 1 && l.Seconds <= s.To + 1).OrderBy(l => l.Seconds).ToList();
            var printed = TheRecordingsScoreboardTests.Text(mine);

            output.WriteLine(string.Empty);
            output.WriteLine(string.Create(Inv, $"### {s.From:0.#}-{s.To:0.#} s at {s.PitchHz:0} Hz ({s.Confidence}): {TheRecordingsScoreboardTests.Right(printed, s.Reference)} of {TheRecordingsScoreboardTests.Letters(s.Reference).Length}"));
            output.WriteLine("sent:    `" + s.Reference + "`");
            output.WriteLine("printed: `" + printed + "`");
            output.WriteLine(string.Empty);
            output.WriteLine("| edit | sent | laid | at | committed under | the reader between |");
            output.WriteLine("|---|---|---|---|---|---|");

            var decode = TheRecordingsScoreboardTests.Letters(printed);
            var key = TheRecordingsScoreboardTests.Letters(s.Reference);

            if (decode.Length == 0)
            {
                output.WriteLine("| nothing printed | | | | | |");
                continue;
            }

            var score = CwScorer.Within(decode, key, CwKeyKind.Exact);
            var di = score.Start;
            var lastAt = di > 0 && di - 1 < mine.Count ? mine[di - 1].Seconds : s.From;

            foreach (var step in score.Steps)
            {
                TheRecordingsScoreboardTests.Printed? at = step.Edit == CwEdit.Missing || di >= mine.Count ? null : mine[di];

                if (step.Edit != CwEdit.Same)
                {
                    var until = at?.Seconds ?? (di < mine.Count ? mine[di].Seconds : s.To);
                    var commit = at is null ? null : lines.Select(l => l.Line).FirstOrDefault(l => l.Contains(string.Create(Inv, $" at {at.Value.From:0.00}-"), StringComparison.Ordinal) && l.Contains("commits", StringComparison.Ordinal));
                    var between = lines.Where(l => l.At > lastAt && l.At <= until + CommitLagSeconds + DecodeEvery && !l.Line.Contains("commits", StringComparison.Ordinal)).Select(l => l.Line[(l.Line.IndexOf(" s ", StringComparison.Ordinal) + 3)..]).ToList();
                    var sent = step.Key is { } k ? $"{k} `{Pattern(k)}`" : "-";
                    var laid = at is { } a ? $"{a.Text} `{(commit is null ? "?" : commit.Split(' ')[3])}`" : "-";

                    output.WriteLine(string.Create(Inv, $"| {step.Edit} | {sent} | {laid} | {(at is { } p ? string.Create(Inv, $"{p.From:0.00}-{p.Seconds:0.00}") : string.Create(Inv, $"after {lastAt:0.00}"))} | {(commit is null ? "-" : commit[(commit.IndexOf(';', StringComparison.Ordinal) + 2)..])} | {string.Join("; ", between.Take(4))}{(between.Count > 4 ? string.Create(Inv, $"; and {between.Count - 4} more") : string.Empty)} |"));
                }

                if (step.Edit != CwEdit.Missing)
                {
                    if (di < mine.Count)
                    {
                        lastAt = mine[di].Seconds;
                    }

                    di++;
                }
            }
        }

        output.WriteLine(string.Empty);
        output.WriteLine("The reader's own lines, holds and lets go:");

        foreach (var l in trace.Where(l => l.Contains("holds", StringComparison.Ordinal) || l.Contains("let go", StringComparison.Ordinal) || l.Contains(" warm: ", StringComparison.Ordinal) || l.Contains("someone else", StringComparison.Ordinal)))
        {
            output.WriteLine("  " + l);
        }
    }

    /// <remarks>
    /// A cold lane's starts on a recording's window, each in the width its dit asks for: the width, what one pass learned, the
    /// likelihood under the levels every first pass decodes with and the prior, each per hop, and the text. Asserts nothing.
    /// </remarks>
    [Theory]
    [InlineData("cw-2026-10-02-200157", 675.0, 6)]
    [InlineData("cw-2026-10-02-200157", 675.0, 12)]
    [InlineData("synthetic-cq/cq-12wpm-15db", 575.0, 6)]
    [InlineData("synthetic-cq/cq-12wpm-15db", 600.0, 6)]
    [InlineData("cw-2026-10-08-121414", 800.0, 8)]
    public void TheColdStartsScored(string recording, double pitch, double end)
    {
        var audio = WavAudio.Read(recording.Contains('/', StringComparison.Ordinal) ? Path.Combine(CwFixtures.Folder, recording + ".wav") : TheOwnersRecordingReadsTests.Wav(recording));
        var hops = (int)(end * 1000 / CwSequenceReader.HopMs);
        var from = Math.Max(0, hops - (int)(CwSequenceReader.WindowSeconds * 1000 / CwSequenceReader.HopMs));
        var envelopes = CwSequenceReader.LaneWidthsHz.Select(w => CwSequenceReader.Envelope(audio.Samples, audio.SampleRate, pitch, w)[from..hops]).ToArray();
        var middle = CwSequenceReader.LaneWidthsHz.Length / 2;

        foreach (var dit in new[] { CwSequenceReader.GuessDitHops(envelopes[middle]) }.Concat(CwSequenceReader.ColdStartDits).Distinct())
        {
            foreach (var w in Enumerable.Range(0, envelopes.Length))
            {
                var db = envelopes[w];
                var ranges = CwSequenceReader.RangesFromDit(dit);
                var segs = CwSequenceReader.Decode(db, ranges, CwSequenceReader.LevelsFromPercentiles(db));
                var learnedOne = CwSequenceReader.Learn(segs, db);
                var lv = learnedOne?.Levels ?? CwSequenceReader.LevelsFromPercentiles(db);
                var r = new CwSequenceReader.Reading(segs, CwSequenceReader.Text(segs), learnedOne is null ? [] : [learnedOne], lv, CwSequenceReader.Pattern(segs, lv));
                var ll = CwSequenceReader.LogLikelihood(r.Segments, db, CwSequenceReader.LevelsFromPercentiles(db)) / db.Length;
                var prior = CwSequenceReader.PriorCost(r.Segments, ranges) / db.Length;

                output.WriteLine(string.Create(Inv, $"{recording} to {end:0} s, start {dit * 5} ms, {CwSequenceReader.LaneWidthsHz[w]:0} Hz{(w == CwSequenceReader.WidthFor(dit) ? " (its width)" : string.Empty)}: ll {ll:0.000} prior {prior:0.000} score {ll - prior:0.000}; {(r.Passes.Count > 0 ? r.Passes[^1] : null)}; `{Cut(r.Text)}`"));
            }
        }

        var (cold, width) = CwSequenceReader.ReadCold(w => envelopes[w]);

        output.WriteLine(string.Create(Inv, $"chosen: {CwSequenceReader.LaneWidthsHz[width]:0} Hz, {(cold.Passes.Count > 0 ? cold.Passes[^1] : null)}; `{Cut(cold.Text)}`"));
    }

    /// <remarks>
    /// Work instruction 590, change 1, the trace: a cold lane's starts on the windows of a recording's opening, as the live reader
    /// would read them, in the narrowest width under the shared levels: each start's one-pass pattern, its learned gaps and whether
    /// they climb by <see cref="CwSequenceReader.GapStep"/>, its likelihood less prior per hop, and which start the likelihood alone
    /// takes against which the pattern takes. Asserts nothing.
    /// </remarks>
    [Theory]
    [InlineData("w1aw-2026-10-07/piece-02", 600.0, 6, 36)]
    [InlineData("cw-2026-10-02-200157", 675.0, 6, 14)]
    [InlineData("cw-2026-10-08-121414", 800.0, 6, 16)]
    public void TheColdStartsByPattern(string recording, double pitch, int from, int to)
    {
        var audio = WavAudio.Read(recording.StartsWith("w1aw", StringComparison.Ordinal) ? Path.Combine(CwFixtures.Folder, "captured", recording + ".wav") : TheOwnersRecordingReadsTests.Wav(recording));
        var whole = CwSequenceReader.Envelope(audio.Samples, audio.SampleRate, pitch, CwSequenceReader.LaneWidthsHz[0]);
        var window = (int)(CwSequenceReader.WindowSeconds * 1000 / CwSequenceReader.HopMs);

        for (var end = from; end <= to; end += 2)
        {
            var hops = Math.Min(whole.Length, (int)(end * 1000 / CwSequenceReader.HopMs));
            var db = whole[Math.Max(0, hops - window)..hops];
            var levels = CwSequenceReader.LevelsFromPercentiles(db);
            var rows = new List<(int Dit, double Score, bool Climbs, CwSequenceReader.PatternStats Pattern, string Text)>();

            foreach (var dit in new[] { CwSequenceReader.GuessDitHops(db) }.Concat(CwSequenceReader.ColdStartDits).Distinct())
            {
                var ranges = CwSequenceReader.RangesFromDit(dit);
                var first = CwSequenceReader.Read(db, ranges, levels, passes: 1);
                var score = (CwSequenceReader.LogLikelihood(first.Segments, db, levels) - CwSequenceReader.PriorCost(first.Segments, ranges)) / db.Length;
                var learned = first.Passes.Count > 0 ? first.Passes[^1] : null;
                var climbs = learned is not null && CwSequenceReader.GapsClimb(learned);

                rows.Add((dit, score, climbs, first.Pattern, first.Text));
                output.WriteLine(string.Create(Inv, $"{recording} to {end} s, start {dit * CwSequenceReader.HopMs:0} ms: score {score:0.000}; {first.Pattern}; gaps {(climbs ? "climb" : "crossed")}; {learned}; `{Cut(first.Text)}`"));
            }

            var byLikelihood = rows.MaxBy(r => r.Score);
            var morse = rows.Where(r => r.Pattern.IsMorse && r.Climbs).ToList();
            var byPattern = morse.Count > 0 ? morse.MaxBy(r => r.Score) : byLikelihood;

            output.WriteLine(string.Create(Inv, $"  to {end} s: likelihood takes {byLikelihood.Dit * CwSequenceReader.HopMs:0} ms, the pattern takes {(morse.Count > 0 ? $"{byPattern.Dit * CwSequenceReader.HopMs:0} ms" : "none Morse, the likeliest")}"));
        }
    }

    /// <remarks>
    /// Work instruction 590, change 2, the trace: three passes on a window, then what one stretch of nobody keying would score over
    /// the whole window under three noise emissions, beside the path's own score, every hop's Gaussian normaliser counted on both
    /// sides. Asserts nothing.
    /// </remarks>
    [Fact]
    public void SilenceAgainstThePath()
    {
        var windows = new List<(string Name, double[] Db, double Dit)>();

        for (var seed = 31; seed <= 33; seed++)
        {
            var band = Synth(Cases[0] with { Db = -300 }, seed);
            var all = CwSequenceReader.Envelope(band, SynthRate, ToneHz, CwSequenceReader.LaneWidthsHz[0]);

            windows.Add(($"the band alone, seed {seed}", all[200..2600], 1200.0 / 18 / CwSequenceReader.HopMs));
        }

        foreach (var (file, pitch, at) in new[] { ("synthetic-cq/cq-18wpm-5db", 600.0, 12), ("synthetic-cq/cq-12wpm-5db", 600.0, 14), ("synthetic-cq/cq-18wpm-0db", 600.0, 12) })
        {
            var audio = WavAudio.Read(Path.Combine(CwFixtures.Folder, file + ".wav"));
            var all = CwSequenceReader.Envelope(audio.Samples, audio.SampleRate, pitch, CwSequenceReader.LaneWidthsHz[0]);
            var hops = (int)(at * 1000 / CwSequenceReader.HopMs);

            windows.Add((file, all[Math.Max(0, hops - 2400)..Math.Min(all.Length, hops)], CwSequenceReader.GuessDitHops(all[Math.Max(0, hops - 2400)..hops])));
        }

        foreach (var (name, db, dit) in windows)
        {
            var reading = CwSequenceReader.Read(db, CwSequenceReader.RangesFromDit(dit), CwSequenceReader.LevelsFromPercentiles(db));
            var lv = reading.Levels;
            var ranges = reading.Passes.Count > 0 ? reading.Passes[^1].Ranges : CwSequenceReader.RangesFromDit(dit);
            var path = 0.0;

            foreach (var g in reading.Segments)
            {
                var (mu, s) = g.IsMark ? (lv.Top, lv.SpreadOn) : (lv.Floor, lv.SpreadOff);

                for (var k = g.From; k < g.To; k++)
                {
                    var z = (db[k] - mu) / s;

                    path -= (0.5 * z * z) + Math.Log(s);
                }
            }

            path -= CwSequenceReader.PriorCost(reading.Segments, ranges);

            var gaps = reading.Segments.Where(g => !g.IsMark).SelectMany(g => Enumerable.Range(g.From, g.Length)).Select(k => db[k]).ToList();
            var mean = db.Average();
            var sdWhole = Math.Sqrt(db.Average(v => (v - mean) * (v - mean)));
            var gapMean = gaps.Count > 0 ? gaps.Average() : mean;
            var sdGaps = gaps.Count > 1 ? Math.Sqrt(gaps.Average(v => (v - gapMean) * (v - gapMean))) : sdWhole;

            double One(double mu, double s) => db.Sum(v => -((0.5 * ((v - mu) / s) * ((v - mu) / s)) + Math.Log(s)));

            output.WriteLine(string.Create(Inv, $"{name}: {reading.Pattern}; {lv}; path {path / db.Length:0.000} per hop"));
            output.WriteLine(string.Create(Inv, $"  one silence: floor and key-up spread {One(lv.Floor, lv.SpreadOff) / db.Length:0.000}; floor and the gaps' spread {sdGaps:0.0} dB {One(lv.Floor, sdGaps) / db.Length:0.000}; the window's mean and spread {mean:0.0}±{sdWhole:0.0} dB {One(mean, sdWhole) / db.Length:0.000}"));

            // Narrowband noise's envelope is Rayleigh, so its level in dB is a log-exponential at the noise power, with the 0.919 of
            // the Gaussian's normaliser taken off to compare with the lines above.
            double Rayleigh(IEnumerable<double> over) => 10 * Math.Log10(over.Average(v => Math.Pow(10, v / 10)));
            double LogRayleigh(double power) => db.Sum(v => { var y = (v - power) * Math.Log(10) / 10; return y - Math.Exp(y) + Math.Log(Math.Log(10) / 10) + (0.5 * Math.Log(2 * Math.PI)); });
            var pWhole = Rayleigh(db);
            var pGaps = gaps.Count > 0 ? Rayleigh(gaps) : pWhole;

            output.WriteLine(string.Create(Inv, $"  one silence, log-Rayleigh: at the window's power {pWhole:0.0} dB {LogRayleigh(pWhole) / db.Length:0.000}; at the gaps' power {pGaps:0.0} dB {LogRayleigh(pGaps) / db.Length:0.000}"));
        }
    }

    /// <remarks>
    /// Work instruction 591, section 2, the opening: the live reader on a station's opening with its lane watched. The station's
    /// keyed marks from the keying map, the letters printed and the truth; every decode of the watched lane (its window, where the
    /// commit lag stood, how it read, its levels and rhythm, and the letters its path laid with the second each starts); every
    /// letter committed from any lane; and every 50 ms from a second before his first mark, the lane's level at each width beside
    /// whether he was keyed. Asserts nothing.
    /// </remarks>
    [Theory]
    [InlineData("cw-2026-10-02-200157", 675.0, 0, 5)]
    [InlineData("cw-2026-10-08-121414", 800.0, 0, 8)]
    [InlineData("cw-2026-10-03-144020", 600.0, 9, 15)]
    [InlineData("w1aw-2026-10-07/piece-02", 600.0, 0, 6)]
    [InlineData("w1aw-2026-10-07/piece-04", 600.0, 0, 6)]
    [InlineData("cw-2026-10-08-121414", 800.0, 10, 20)]
    public void TheOpeningTraced(string recording, double watchHz, double from, double to)
    {
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(recording));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(recording);
        var trace = new List<string>();
        IReadOnlyList<TheRecordingsScoreboardTests.Printed> letters;

        using (CwRules.Choose(CwRules.SequenceReader))
        {
            letters = ReadLiveTraced(audio.Samples, audio.SampleRate, pitch, width, trace, watchHz);
        }

        var map = TheKeyingMapTests.Of(recording);
        var station = map.Stations.MinBy(s => Math.Abs(s.PitchHz - watchHz))!;
        var keyed = station.Marks.Where(m => m.To >= from - 1 && m.From <= to).ToList();
        var first = keyed.Count > 0 ? keyed[0].From : from;
        double At(string line) => double.Parse(line[..line.IndexOf(' ', StringComparison.Ordinal)], Inv);

        output.WriteLine(string.Create(Inv, $"## {recording}: the lane at {watchHz:0} Hz, {from:0.#} to {to:0.#} s; the station at {station.PitchHz:0} Hz keys first at {first:0.00} s"));
        output.WriteLine("truth, the keying map: " + string.Join(' ', keyed.Select(m => string.Create(Inv, $"{(m.To - m.From) * 1000:0}@{m.From:0.00}"))));

        foreach (var s in TheRecordingsScoreboardTests.Stretches.Where(s => s.Recording == recording && s.From < to && s.To > from))
        {
            output.WriteLine(string.Create(Inv, $"sent {s.From:0.#}-{s.To:0.#} s at {s.PitchHz:0} Hz: `{s.Reference}`"));
        }

        output.WriteLine("printed: " + string.Join(' ', letters.Where(l => l.From >= from - 1 && l.From <= to + 2).Select(l => string.Create(Inv, $"{l.Text}@{l.From:0.00}({l.PitchHz:0})"))));
        output.WriteLine(string.Empty);

        foreach (var l in trace.Where(l => ((l.Contains(" watch ", StringComparison.Ordinal) || l.Contains(" warm: ", StringComparison.Ordinal)) && At(l) <= to + 8) || ((l.Contains(" commits ", StringComparison.Ordinal) || l.Contains("holds", StringComparison.Ordinal) || l.Contains("let go", StringComparison.Ordinal)) && At(l) <= to + 6)))
        {
            output.WriteLine("  " + l);
        }

        output.WriteLine(string.Empty);
        output.WriteLine("| s | 15 Hz | 24 Hz | 40 Hz | keyed |");
        output.WriteLine("|---|---|---|---|---|");

        var envelopes = CwSequenceReader.LaneWidthsHz.Select(w => CwSequenceReader.Envelope(audio.Samples, audio.SampleRate, watchHz, w)).ToArray();

        for (var t = Math.Max(0, first - 1); t <= Math.Min(to, first + 4); t += 0.05)
        {
            var k = (int)(t * 1000 / CwSequenceReader.HopMs);
            var on = station.Marks.Any(m => m.From <= t && m.To > t);

            output.WriteLine(string.Create(Inv, $"| {t:0.00} | {envelopes[0][k]:0.0} | {envelopes[1][k]:0.0} | {envelopes[2][k]:0.0} | {(on ? "key" : string.Empty)} |"));
        }
    }

    /// <remarks>
    /// Work instruction 591, section 2, the silences: each letter the live reader printed inside a silence of two seconds or more
    /// on the keying map, the lane that committed it with its rhythm, levels and window pattern, the lane's holds and lets go, and
    /// every station's keyed mark within a second of it. Asserts nothing.
    /// </remarks>
    [Theory]
    [InlineData("cw-2026-10-02-200157")]
    [InlineData("cw-2026-10-03-143906")]
    [InlineData("cw-2026-10-03-221502")]
    public void TheSilencesTraced(string recording)
    {
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(recording));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(recording);
        var trace = new List<string>();
        IReadOnlyList<TheRecordingsScoreboardTests.Printed> letters;

        using (CwRules.Choose(CwRules.SequenceReader))
        {
            letters = ReadLiveTraced(audio.Samples, audio.SampleRate, pitch, width, trace);
        }

        var map = TheKeyingMapTests.Of(recording);

        output.WriteLine(string.Create(Inv, $"## {recording}: printed `{string.Concat(letters.Select(l => (l.SpaceBefore ? " " : string.Empty) + l.Text))}`; silences {string.Join(", ", map.Silences.Select(s => string.Create(Inv, $"{s.From:0.00}-{s.To:0.00}")))}"));
        output.WriteLine("stations: " + string.Join("; ", map.Stations.Select(s => string.Create(Inv, $"{s.PitchHz:0} Hz, {s.Marks.Count} marks, {s.Marks[0].From:0.00}-{s.Marks[^1].To:0.00} s"))));

        foreach (var l in letters.Where(l => map.SilenceAround(l.From, l.Seconds) is not null))
        {
            var quiet = map.SilenceAround(l.From, l.Seconds)!.Value;

            output.WriteLine(string.Empty);
            output.WriteLine(string.Create(Inv, $"### `{l.Text}` at {l.From:0.00}-{l.Seconds:0.00} s from {l.PitchHz:0} Hz, inside the silence {quiet.From:0.00}-{quiet.To:0.00} s"));

            foreach (var c in trace.Where(c => c.Contains(" commits ", StringComparison.Ordinal) && c.Contains(string.Create(Inv, $"at {l.From:0.00}-"), StringComparison.Ordinal)))
            {
                output.WriteLine("  committed: " + c);
            }

            foreach (var m in map.Marks.Where(m => m.To >= l.From - 1 && m.From <= l.Seconds + 1))
            {
                output.WriteLine(string.Create(Inv, $"  keyed nearby: {m.PitchHz:0} Hz {m.From:0.00}-{m.To:0.00} s, {m.OverDb:0.0} dB over"));
            }
        }

        output.WriteLine(string.Empty);

        foreach (var c in trace.Where(c => c.Contains("holds", StringComparison.Ordinal) || c.Contains("let go", StringComparison.Ordinal) || c.Contains("someone else", StringComparison.Ordinal)))
        {
            output.WriteLine("  " + c);
        }
    }

    private const double CommitLagSeconds = CwSequenceReader.CommitLagSeconds;

    private const double DecodeEvery = CwSequenceReader.DecodeEverySeconds;

    // The true pattern of a letter the scorer names: a prosign is shown as itself.
    private static string Pattern(char c)
        => MorseAlphabet.All.FirstOrDefault(kv => kv.Value == c.ToString()).Key ?? "prosign";

    /// <summary><see cref="TheRecordingsScoreboardTests.ReadLive(float[], int, double, double)"/>, the reader's trace kept.</summary>
    internal static IReadOnlyList<TheRecordingsScoreboardTests.Printed> ReadLiveTraced(float[] samples, int rate, double pitch, double width, List<string> trace, double watchHz = double.NaN)
    {
        using var chain = new CwChain(rate);
        var gate = chain.Decoder.Runs;
        var letters = new List<TheRecordingsScoreboardTests.Printed>();
        var space = false;
        var chunk = rate / 100;

        chain.Detector.SetPassband(pitch, width);

        if (chain.Detector.Sequence is { } reader)
        {
            reader.Trace = trace;
            reader.WatchHz = watchHz;
        }

        gate.CharacterRead += c => space |= c.Text == MorseAlphabet.WordGap;
        gate.RunRead += (c, run) =>
        {
            letters.Add(new TheRecordingsScoreboardTests.Printed(run[^1].ToSeconds, run.Average(m => m.PitchHz), c.Text, space, null, run[0].FromSeconds));
            space = false;
        };

        for (var at = 0; at + chunk <= samples.Length; at += chunk)
        {
            chain.Process(new AudioChunk(at, rate, samples.AsSpan(at, chunk)));
        }

        chain.Decoder.Flush();
        return letters;
    }

    /// <summary>Audio through the app's chain, the sequence reader's trace kept.</summary>
    internal static string Live(float[] samples, int rate, double pitch, double width, List<string>? trace)
    {
        using var chain = new CwChain(rate);
        var characters = new List<CwCharacter>();
        var chunk = rate / 100;

        chain.Detector.SetPassband(pitch, width);

        if (chain.Detector.Sequence is { } reader)
        {
            reader.Trace = trace;
        }

        chain.Decoder.Runs.CharacterRead += characters.Add;

        for (var at = 0; at + chunk <= samples.Length; at += chunk)
        {
            chain.Process(new AudioChunk(at, rate, samples.AsSpan(at, chunk)));
        }

        chain.Decoder.Flush();

        return string.Join(' ', string.Concat(characters.Select(c => c.Text)).Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string Cut(string text) => text.Length <= 120 ? text.Trim() : text[..120].Trim() + " …";

    private static string Code(char c) => MorseAlphabet.All.First(kv => kv.Value == c.ToString()).Key;
}
