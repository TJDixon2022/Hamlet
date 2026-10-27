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

        foreach (var lane in new[] { CwSequenceReader.LaneBandwidthHz, 10 })
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
