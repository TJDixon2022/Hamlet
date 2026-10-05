using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Scan;

/// <summary>
/// **WHY A DECODER THAT HEARS THE MARKS LOSES THE LETTERS** (work instruction 544, task 1, HM-DEC-248): the scan's strong
/// station, `catch-153810-7033367`, read element by element through the app's chain beside a plain offline read, and how the
/// sender's dit, dah, gap lines and level reference drift over its 85 s.
/// </summary>
/// <remarks>
/// <para>R88 is lifted for the scan catches work instruction 544 names; this reads the three in the tree.</para>
/// <para>**THE PLAIN READ IS NOT GROUND TRUTH.** It is mixed to nought at the station's tone, through a 40 Hz two-pole
/// low-pass run forward and back, cut at the midpoint of two level clusters over the whole catch, its marks split into dit
/// and dah by two clusters in log-length, a gap of two dits ending a letter and five a word: the shack decoder's recipe. The
/// owner's ear rules on any text.</para>
/// </remarks>
public sealed class TheStrongStationsOverTests
{
    internal const string Main = "catch-153810-7033367";
    internal const string Second = "catch-154819-7050903";
    internal const string Carrier = "catch-154614-7047190";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the trace is printed.</param>
    public TheStrongStationsOverTests(ITestOutputHelper output) => _output = output;

    /// <summary>A scan catch's WAV in the tree.</summary>
    internal static string Wav(string name)
    {
        var dir = AppContext.BaseDirectory;

        while (dir is not null && !Directory.Exists(Path.Combine(dir, "tests", "fixtures", "cw", "captured", "scans")))
        {
            dir = Path.GetDirectoryName(dir);
        }

        return Path.Combine(dir ?? throw new DirectoryNotFoundException("the scan catches"), "tests", "fixtures", "cw", "captured", "scans", name + ".wav");
    }

    /// <summary>One mark the gate printed, with the sender's figures at the moment its letter was read.</summary>
    internal sealed record Labelled(double From, double To, double PitchHz, double LevelDb, char Kind, int Letter, double Split, double Dit, double CharacterLine, double WordLine, double LevelRef);

    /// <summary>What the chain made of a catch.</summary>
    internal sealed record ChainRead(string Text, IReadOnlyList<CwMark> Marks, IReadOnlyList<Labelled> Printed, IReadOnlyList<(double At, string Text)> Letters, int Senders, double Shape, double PitchHz, bool Green);

    /// <summary>Reads a catch through the app's chain at the scan's passband, as the ear does.</summary>
    internal static ChainRead ReadChain(float[] samples, int rate, double pitchHz = 600, double widthHz = 500)
    {
        using var chain = new CwChain(rate, pitchHz);
        var gate = chain.Decoder.Runs;
        var marks = new List<CwMark>();
        var printed = new List<Labelled>();
        var letters = new List<(double, string)>();
        var text = new StringBuilder();
        var letter = 0;
        var green = false;
        long sequence = 0;

        chain.Detector.SetPassband(pitchHz, widthHz);
        gate.CharacterRead += c => text.Append(c.Text);
        gate.RunRead += (c, run) =>
        {
            var split = gate.StationSplitSeconds;
            var lines = gate.StationLines;

            foreach (var m in run)
            {
                printed.Add(new Labelled(
                    m.FromSeconds, m.ToSeconds, m.PitchHz, m.LevelDb, m.ToSeconds - m.FromSeconds >= split ? '-' : '.', letter,
                    split, gate.StationDitSeconds, lines?.CharacterSeconds ?? double.NaN, lines?.WordSeconds ?? double.NaN, gate.StationLevelDb));
            }

            letters.Add((run[^1].ToSeconds, c.Text));

            if (split < 0.08)
            {
                Collapsed.Add($"{run[0].FromSeconds:0.00} s: {CwSenderGate.LastClusters}");
            }
            letter++;
        };

        var chunk = rate / 100;

        for (var at = 0; at + chunk <= samples.Length; at += chunk)
        {
            chain.Process(new AudioChunk(at, rate, samples.AsSpan(at, chunk)));

            var batch = chain.Detector.MarksSince(sequence);

            if (batch.Marks.Count > 0)
            {
                marks.AddRange(batch.Marks);
                sequence = batch.Marks.Max(m => m.Sequence);
            }

            green |= chain.Detector.Reading.ShapeLight is CwShapeLight.Found or CwShapeLight.Reading;
        }

        chain.Decoder.Flush();

        var side = chain.Decoder.ShapeSide;
        var best = side.Senders.OrderByDescending(s => s.Marks).FirstOrDefault();

        return new ChainRead(
            string.Join(' ', text.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries)),
            marks, printed, letters, side.Senders.Count, best?.ShapeScore ?? 0, best?.PitchHz ?? double.NaN, green);
    }

    /// <summary>One mark of the plain read, its kind, and the letter it is in.</summary>
    internal sealed record PlainMark(double From, double To, char Kind, int Letter, bool WordBefore);

    /// <summary>The last plain read's gap clusters and lines, in words.</summary>
    [ThreadStatic]
    internal static string? Lines;

    /// <summary>The sender's clusters wherever its split fell under 80 ms, for the trace.</summary>
    internal static readonly List<string> Collapsed = new();

    /// <summary>The plain read: marks, their kinds, letters and words, and the dit and dah it found.</summary>
    internal static (IReadOnlyList<PlainMark> Marks, string Text, double Dit, double Dah) ReadPlain(float[] x, int rate, double toneHz, double fromSeconds = 0, double toSeconds = double.PositiveInfinity)
    {
        var first = (int)(Math.Max(0, fromSeconds) * rate);
        var n = (int)Math.Min(x.Length - first, (toSeconds - Math.Max(0, fromSeconds)) * rate);
        var i0 = new double[n];
        var q0 = new double[n];

        for (var k = 0; k < n; k++)
        {
            var phase = 2 * Math.PI * toneHz * (first + k) / rate;

            i0[k] = x[first + k] * Math.Cos(phase);
            q0[k] = x[first + k] * Math.Sin(phase);
        }

        var a = 1 - Math.Exp(-2 * Math.PI * 40 / rate);

        void Pass(double[] v, bool back)
        {
            var y = 0.0;

            for (var j = 0; j < v.Length; j++)
            {
                var k = back ? v.Length - 1 - j : j;

                y += a * (v[k] - y);
                v[k] = y;
            }
        }

        foreach (var v in new[] { i0, q0 })
        {
            Pass(v, false);
            Pass(v, false);
            Pass(v, true);
            Pass(v, true);
        }

        var step = rate / 1000;
        var db = new double[n / step];

        for (var k = 0; k < db.Length; k++)
        {
            var j = k * step;

            db[k] = 10 * Math.Log10((i0[j] * i0[j]) + (q0[j] * q0[j]) + 1e-20);
        }

        var sorted = db.Order().ToArray();
        var lo = sorted[(int)(0.10 * (sorted.Length - 1))];
        var hi = sorted[(int)(0.95 * (sorted.Length - 1))];

        for (var it = 0; it < 20; it++)
        {
            var mid = (lo + hi) / 2;
            var below = db.Where(d => d < mid).ToList();
            var above = db.Where(d => d >= mid).ToList();

            if (below.Count == 0 || above.Count == 0)
            {
                break;
            }

            lo = below.Average();
            hi = above.Average();
        }

        var cut = (lo + hi) / 2;
        var hyst = Math.Min(1.5, (hi - lo) / 8);
        var runs = new List<(int From, int To)>();
        var on = false;
        var start = 0;

        for (var k = 0; k < db.Length; k++)
        {
            if (!on && db[k] > cut + hyst)
            {
                on = true;
                start = k;
            }
            else if (on && db[k] < cut - hyst)
            {
                on = false;
                runs.Add((start, k));
            }
        }

        var joined = new List<(int From, int To)>();

        foreach (var r in runs)
        {
            if (joined.Count > 0 && r.From - joined[^1].To < 15)
            {
                joined[^1] = (joined[^1].From, r.To);
            }
            else
            {
                joined.Add(r);
            }
        }

        joined.RemoveAll(r => r.To - r.From < 20);

        // Dit and dah: two clusters in log-length.
        var logs = joined.Select(r => Math.Log(r.To - r.From)).ToList();
        var c1 = logs.Min();
        var c2 = logs.Max();

        for (var it = 0; it < 30; it++)
        {
            var mid = (c1 + c2) / 2;
            var s = logs.Where(l => l < mid).ToList();
            var l2 = logs.Where(l => l >= mid).ToList();

            if (s.Count == 0 || l2.Count == 0)
            {
                break;
            }

            c1 = s.Average();
            c2 = l2.Average();
        }

        var dit = Math.Exp(c1);
        var dah = Math.Exp(c2);
        var split = Math.Exp((c1 + c2) / 2);
        // Letter and word cuts from the read's own gaps: three clusters in log-length, element, letter and word, each
        // line at the geometric middle of its two neighbours' centres. The smoothing widens every mark and narrows every
        // gap by the same few milliseconds, so the gaps are judged against each other and not against the dit.
        var gapLogs = joined.Skip(1).Select((r, k) => Math.Log(Math.Max(1, r.From - joined[k].To))).OrderBy(g => g).ToList();
        var g1 = gapLogs[(int)(0.25 * (gapLogs.Count - 1))];
        var g2 = gapLogs[(int)(0.75 * (gapLogs.Count - 1))];
        var g3 = gapLogs[^1];

        for (var it = 0; it < 30; it++)
        {
            var lines = (A: (g1 + g2) / 2, B: (g2 + g3) / 2);
            var e = gapLogs.Where(g => g < lines.A).ToList();
            var l = gapLogs.Where(g => g >= lines.A && g < lines.B).ToList();
            var w = gapLogs.Where(g => g >= lines.B).ToList();

            if (e.Count == 0 || l.Count == 0 || w.Count == 0)
            {
                break;
            }

            (g1, g2, g3) = (e.Average(), l.Average(), w.Average());
        }

        var letterLine = Math.Exp((g1 + g2) / 2);
        var wordLine = Math.Exp((g2 + g3) / 2);
        var marks = new List<PlainMark>();
        var text = new StringBuilder();
        var pattern = new StringBuilder();
        var letter = 0;

        void EndLetter()
        {
            if (pattern.Length > 0)
            {
                text.Append(MorseAlphabet.Lookup(pattern.ToString()) ?? "■");
                pattern.Clear();
            }
        }

        for (var k = 0; k < joined.Count; k++)
        {
            var word = false;

            if (k > 0)
            {
                var gap = joined[k].From - joined[k - 1].To;

                if (gap >= letterLine)
                {
                    EndLetter();
                    letter++;

                    if (gap >= wordLine)
                    {
                        text.Append(' ');
                        word = true;
                    }
                }
            }

            var kind = joined[k].To - joined[k].From >= split ? '-' : '.';

            pattern.Append(kind);
            marks.Add(new PlainMark((first / (double)rate) + (joined[k].From / 1000.0), (first / (double)rate) + (joined[k].To / 1000.0), kind, letter, word));
        }

        EndLetter();

        Lines = $"gaps: element {Math.Exp(g1):0} ms, letter {Math.Exp(g2):0} ms, word {Math.Exp(g3):0} ms; lines {letterLine:0} and {wordLine:0} ms";

        return (marks, text.ToString(), dit / 1000, dah / 1000);
    }

    /// <remarks>
    /// Task 1: the main catch, element by element against the plain read, in ten-second windows: marks each found and how
    /// many the chain missed or added, dits and dahs labelled the other way, letter cuts missed or added, and the sender's
    /// dit, split, character and word lines and level reference as the gate held them. Asserts only that the station is
    /// heard; the trace is the result.
    /// </remarks>
    [Fact]
    public void TheDriftOfTheMainOver()
    {
        var audio = WavAudio.Read(Wav(Main));
        var chain = ReadChain(audio.Samples, audio.SampleRate);
        var station = chain.PitchHz;
        var plain = ReadPlain(audio.Samples, audio.SampleRate, station);

        _output.WriteLine($"chain: {chain.Marks.Count} marks at every pitch, {chain.Marks.Count(m => Math.Abs(m.PitchHz - station) <= 30)} within 30 Hz of {station:0} Hz; {chain.Printed.Count} printed in {chain.Letters.Count} letters; senders {chain.Senders}; shape {chain.Shape:0.000}; green {chain.Green}");
        _output.WriteLine($"chain text: {chain.Text}");
        _output.WriteLine($"plain: {plain.Marks.Count} marks, dit {plain.Dit * 1000:0} ms, dah {plain.Dah * 1000:0} ms");
        _output.WriteLine($"plain text: {plain.Text}");
        _output.WriteLine($"plain {Lines}");
        _output.WriteLine($"chain gaps at the tone, ms, by count: {Histogram(chain, station)}");
        _output.WriteLine(Drift(chain, plain.Marks, station));

        // Every printed mark labelled the other way from the plain read: when, both lengths, and the split it was cut at.
        foreach (var m in chain.Printed)
        {
            var p = plain.Marks.FirstOrDefault(x => Math.Min(x.To, m.To) - Math.Max(x.From, m.From) > 0.3 * Math.Min(x.To - x.From, m.To - m.From));

            if (p is not null && p.Kind != m.Kind)
            {
                _output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"flip {m.From:0.00} s: chain {(m.To - m.From) * 1000:0} ms '{m.Kind}' at split {m.Split * 1000:0}, plain {(p.To - p.From) * 1000:0} ms '{p.Kind}', level {m.LevelDb:0.0} dB against {m.LevelRef:0.0}"));
            }
        }

        foreach (var c in Collapsed)
        {
            _output.WriteLine("collapsed " + c);
        }

        var lengths = chain.Printed.Select(m => (m.To - m.From) * 1000).ToList();

        _output.WriteLine($"chain mark lengths, ms, by count: {string.Join(" ", Enumerable.Range(0, 15).Select(b => $"{b * 20}:{lengths.Count(l => l >= b * 20 && l < (b + 1) * 20)}"))}");

        Assert.True(chain.Marks.Count(m => Math.Abs(m.PitchHz - station) <= 30) > 300);
    }

    /// <remarks>
    /// Task 1: both station catches in the tree read through the app's chain before - the gate keeping no line, as at HEAD -
    /// and after, beside the plain read of each at the station's own tone: text, marks at the tone, shape and whether the
    /// light went green.
    /// </remarks>
    [Theory]
    [InlineData(Main)]
    [InlineData(Second)]
    public void TheTwoStationsBeforeAndAfter(string name)
    {
        var audio = WavAudio.Read(Wav(name));
        ChainRead before;

        using (CwRules.Off(CwRules.KeptSplit))
        {
            before = ReadChain(audio.Samples, audio.SampleRate);
        }

        var after = ReadChain(audio.Samples, audio.SampleRate);
        var heard = after.Marks.Where(m => Math.Abs(m.PitchHz - after.PitchHz) <= 30).ToList();
        var plain = ReadPlain(audio.Samples, audio.SampleRate, after.PitchHz, heard.Min(m => m.FromSeconds) - 0.5, heard.Max(m => m.ToSeconds) + 0.5);

        _output.WriteLine($"{name} keyed {heard.Min(m => m.FromSeconds):0.0} to {heard.Max(m => m.ToSeconds):0.0} s");

        foreach (var (label, read) in new[] { ("before", before), ("after", after) })
        {
            _output.WriteLine($"{name} {label}: {read.Marks.Count(m => Math.Abs(m.PitchHz - read.PitchHz) <= 30)} marks at {read.PitchHz:0} Hz, shape {read.Shape:0.000}, green {read.Green}");
            _output.WriteLine($"  `{read.Text}`");
        }

        _output.WriteLine($"{name} plain, {plain.Marks.Count} marks, dit {plain.Dit * 1000:0} ms, dah {plain.Dah * 1000:0} ms, {Lines}:");
        _output.WriteLine($"  `{plain.Text}`");

        Assert.NotEmpty(after.Text);
    }

    /// <summary>A scan catch's stretch: its reference, read offline and **pending until the owner confirms it by ear**.</summary>
    /// <param name="Catch">The catch.</param>
    /// <param name="PitchHz">The station's tone in the catch.</param>
    /// <param name="Reference">The offline read.</param>
    /// <param name="Confirmed">Whether the owner has confirmed it by ear; only a confirmed stretch joins the total.</param>
    internal sealed record ScanStretch(string Catch, double PitchHz, string Reference, bool Confirmed);

    /// <summary>
    /// **THE SCANS TABLE** (work instruction 544, task 1): the two station catches in the tree, each against the session's
    /// offline read, anchored on the shack's plain read where it gives a word whole. Both pending: scored and reported, not
    /// totalled.
    /// </summary>
    /// <remarks>
    /// <para>**THE MAIN CATCH**: the offline read cut from its own gap clusters, with the words the shack's independent plain
    /// read gives whole - IVE NEVER HAD, THURSDAY, ROUTINE, ASK FOR XRAY TO SEE - in place of the session's where one mark
    /// garbled them. The run in the middle neither read makes sense of is the session's read as it came out.</para>
    /// <para>**THE SECOND CATCH**: the session's offline read as it came out; the shack gave no text for it.</para>
    /// </remarks>
    internal static readonly ScanStretch[] ScanStretches =
    [
        new(Main, 860, "THE MATRESS IS SOFT ES CANT FIND A SAFE SPOT WITHOUT PAIN HEE IVE NEVER HAD BACK PAIN SO THIS THURSDAY HAVE ROUTINE X EEE KK UW ESGG TO ASK FOR XRAY TO SEE WHAT I TWINTED HEE<BT> I REMEMBER AT AGE", false),
        new(Second, 470, "KS AND BTW, I AGGREIRIATE YOUR NICE KEYIE R", false),
    ];

    /// <remarks>
    /// Task 1: the scans table. Each catch read through the app's chain, scored against its pending reference: letters right,
    /// wrong or extra, and what printed. Pending stretches are reported and never totalled.
    /// </remarks>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TheScansTable(bool keptLine)
    {
        using var rule = keptLine ? null : CwRules.Off(CwRules.KeptSplit);

        _output.WriteLine($"the kept line {(keptLine ? "on" : "off, as at HEAD")}");
        _output.WriteLine("| catch | tone | reference | right | wrong | printed |");
        _output.WriteLine("|---|---|---|---|---|---|");

        foreach (var s in ScanStretches)
        {
            var audio = WavAudio.Read(Wav(s.Catch));
            var read = ReadChain(audio.Samples, audio.SampleRate);
            var right = Cw.TheRecordingsScoreboardTests.Right(read.Text, s.Reference);
            var wrong = Cw.TheRecordingsScoreboardTests.WrongAt(read.Text, s.Reference).Count;

            _output.WriteLine($"| `{s.Catch}` | {s.PitchHz:0} Hz | {(s.Confirmed ? "confirmed" : "**pending**")} | {right} of {Cw.TheRecordingsScoreboardTests.Letters(s.Reference).Length} | {wrong} | `{read.Text}` |");
        }

        Assert.All(ScanStretches, s => Assert.False(s.Confirmed));
    }

    /// <summary>The chain's gaps between consecutive marks at the tone, in 20 ms bins up to 400 ms: bin start, count.</summary>
    internal static string Histogram(ChainRead chain, double station)
    {
        var at = chain.Marks.Where(m => Math.Abs(m.PitchHz - station) <= 30).OrderBy(m => m.FromSeconds).ToList();
        var gaps = at.Skip(1).Select((m, i) => (m.FromSeconds - at[i].ToSeconds) * 1000).Where(g => g < 400).ToList();

        return string.Join(" ", Enumerable.Range(0, 20).Select(b => $"{b * 20}:{gaps.Count(g => g >= b * 20 && g < (b + 1) * 20)}"));
    }

    /// <summary>The drift table: ten-second windows of the chain against the plain read.</summary>
    internal static string Drift(ChainRead chain, IReadOnlyList<PlainMark> plain, double station)
    {
        var sb = new StringBuilder();
        var at = chain.Marks.Where(m => Math.Abs(m.PitchHz - station) <= 30).OrderBy(m => m.FromSeconds).ToList();
        var end = Math.Max(plain.Count > 0 ? plain[^1].To : 0, at.Count > 0 ? at[^1].ToSeconds : 0);

        sb.AppendLine("window | plain marks | chain marks at the tone | missed | added | printed | kind flipped | letter cuts missed / added | dit ms | split ms | char line ms | word line ms | level ref dB | mark level dB");

        static bool Overlap(double f1, double t1, double f2, double t2) => Math.Min(t1, t2) - Math.Max(f1, f2) > 0.3 * Math.Min(t1 - f1, t2 - f2);

        for (var w = 0.0; w < end; w += 10)
        {
            var p = plain.Where(m => m.From >= w && m.From < w + 10).ToList();
            var c = at.Where(m => m.FromSeconds >= w && m.FromSeconds < w + 10).ToList();
            var pr = chain.Printed.Where(m => m.From >= w && m.From < w + 10).ToList();
            var missed = p.Count(m => !c.Any(h => Overlap(m.From, m.To, h.FromSeconds, h.ToSeconds)));
            var added = c.Count(h => !p.Any(m => Overlap(m.From, m.To, h.FromSeconds, h.ToSeconds)));
            var flipped = 0;
            var cutsMissed = 0;
            var cutsAdded = 0;

            for (var i = 0; i < pr.Count; i++)
            {
                var match = plain.Select((m, k) => (m, k)).FirstOrDefault(x => Overlap(x.m.From, x.m.To, pr[i].From, pr[i].To));

                if (match.m is null)
                {
                    continue;
                }

                flipped += match.m.Kind != pr[i].Kind ? 1 : 0;

                if (i + 1 < pr.Count)
                {
                    var next = plain.Select((m, k) => (m, k)).FirstOrDefault(x => Overlap(x.m.From, x.m.To, pr[i + 1].From, pr[i + 1].To));

                    if (next.m is not null && next.k == match.k + 1)
                    {
                        var plainCut = next.m.Letter != match.m.Letter;
                        var chainCut = pr[i + 1].Letter != pr[i].Letter;

                        cutsMissed += plainCut && !chainCut ? 1 : 0;
                        cutsAdded += !plainCut && chainCut ? 1 : 0;
                    }
                }
            }

            static string Ms(IEnumerable<double> v) => v.Where(double.IsFinite).DefaultIfEmpty(double.NaN).Average() is var x && double.IsFinite(x) ? (x * 1000).ToString("0", CultureInfo.InvariantCulture) : "-";
            static string Db(IEnumerable<double> v) => v.Where(double.IsFinite).DefaultIfEmpty(double.NaN).Average() is var x && double.IsFinite(x) ? x.ToString("0.0", CultureInfo.InvariantCulture) : "-";

            sb.AppendLine(string.Create(
                CultureInfo.InvariantCulture,
                $"{w:0}-{w + 10:0} s | {p.Count} | {c.Count} | {missed} | {added} | {pr.Count} | {flipped} | {cutsMissed} / {cutsAdded} | {Ms(pr.Select(m => m.Dit))} | {Ms(pr.Select(m => m.Split))} | {Ms(pr.Select(m => m.CharacterLine))} | {Ms(pr.Select(m => m.WordLine))} | {Db(pr.Select(m => m.LevelRef))} | {Db(c.Select(m => m.LevelDb))}"));
        }

        return sb.ToString();
    }
}
