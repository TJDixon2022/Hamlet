using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE W1AW SESSION OF 2026-10-07, REPLAYED** (work instruction 554, task 2): the four pieces Hamlet captured automatically,
/// 20:00 UTC fast code practice, read in order through the app's chain on one audio clock, at the radio's state from their
/// sheets. R88 is lifted for these four pieces.
/// </summary>
public sealed class TheW1awSessionReplayTests(ITestOutputHelper output)
{
    /// <summary>The pieces, in order, as fixture names.</summary>
    internal static readonly string[] Pieces = ["w1aw-2026-10-07/piece-01", "w1aw-2026-10-07/piece-02", "w1aw-2026-10-07/piece-03", "w1aw-2026-10-07/piece-04"];

    /// <summary>One printed letter: when its run began and ended, at what pitch, and its text, with a space before it where one was.</summary>
    internal sealed record Letter(double From, double To, double PitchHz, string Text, bool SpaceBefore);

    /// <summary>What the replay gave.</summary>
    internal sealed record Replay(
        IReadOnlyList<Letter> Letters,
        IReadOnlyList<(double Seconds, double PitchHz, int Marks, double Shape, bool Printed)> Senders,
        IReadOnlyList<CwMark> Marks,
        double Seconds);

    /// <summary>Replays the four pieces as one stream.</summary>
    internal static Replay Run(int chunkMs = 10, bool wholeBand = false, Action<CwEnvelopeDetector>? setUp = null, Action<double>? onChunk = null, float[]? before = null)
    {
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(Pieces[0]);
        var rate = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(Pieces[0])).SampleRate;

        using var chain = new CwChain(rate);
        var gate = chain.Decoder.Runs;
        var letters = new List<Letter>();
        var senders = new List<(double, double, int, double, bool)>();
        var space = false;
        long index = 0;

        chain.Detector.SetPassband(wholeBand ? null : pitch, wholeBand ? null : width);
        setUp?.Invoke(chain.Detector);
        gate.CharacterRead += c => space |= c.Text == MorseAlphabet.WordGap;
        gate.RunRead += (c, run) =>
        {
            letters.Add(new Letter(run[0].FromSeconds, run[^1].ToSeconds, run.Average(m => m.PitchHz), c.Text, space));
            space = false;
        };

        foreach (var samples in (before is null ? [] : new[] { before }).Concat(Pieces.Select(p => WavAudio.Read(TheOwnersRecordingReadsTests.Wav(p)).Samples)))
        {
            var chunk = rate * chunkMs / 1000;

            for (var at = 0; at + chunk <= samples.Length; at += chunk)
            {
                chain.Process(new AudioChunk(index + at, rate, samples.AsSpan(at, chunk)));
                onChunk?.Invoke((index + at + chunk) / (double)rate);

                if ((index + at + chunk) % rate == 0)
                {
                    var t = (index + at + chunk) / (double)rate;

                    senders.AddRange(gate.SenderShapes.Select(s => (t, s.PitchHz, s.Marks, s.Shape.Score, s.Printed)));
                }
            }

            index += samples.Length - (samples.Length % chunk);
        }

        chain.Decoder.Flush();

        return new Replay(letters, senders, chain.Detector.MarksSince(0).Marks.ToList(), index / (double)rate);
    }

    /// <summary>What Hamlet printed live over the session, as the work instruction gives it.</summary>
    internal const string Live =
        "M ■AW TU CODE PRACTICE STARTS IN 5 MINUTES DE W1AW <AS> ETITI W1AW <BT> CODE PRACTICE STARTS IN 1 MINUTE DE W1AW <AS> G ST GST QST DE W1AW W1AW W1AW QST QST QST DE W1AW W1AW W1AW QST QST QST DE W1AW W1AW W1AW THE COMPL W1AW SCHEDULE APPEARS IN THE SEPTEMBER 2026 ISSUE OF QST ON PAGE 28. PRACTICE AT 35 30 25 20 15 13 AND 10 WPM FOLLOWS. THE NEXT QUALIFYING RUNS SENT BY W1AW WILL BE ON OCTOBER 5 AT 4 PM EDT, OCTOBER 7 AT 7 PM EDT, AT D OCTOBER 8 AT 10 PM EDT. WEST COAST STATION K9 OM WILL TRANSMIT THE OFFICIAL ARRL QUALIFYING RUN ON THURSDAY, OCTOBER 29 AT 9 PM PDT, OR OCTOBER 30 AT 0400Z, ON 3581.5. THE SPEEDS WILL RUN FROM 10 TO 35 WPM. <AR> A E IS FROM OCTOBER 2024 QST PAGE 51 35 WPM TEXT FOLLOWS <AR> <BT> QST DE W1AW <AS> N T T O T ■IE WPM <BT> 8 F LONG AND AT LEAST 30 INCHES DEEP. THEN LAY T IE GROUND ROD AT THE BOTTOM OF THE TRENCH AND MAKE YOUR CONNECTION USING A CRIMP OR WELDED NOT SOLDERED CONNECTOR. PACK THE BOTTOM OF THE TRENCH WITH GROUND ENHANCEMENT MATERIAL GEM BEFORE BURYING THE GROUND ROD. YOU CAN GET GEM FROM AN ELECTRICAL SUPPLY HOUSE. ATTAC I THE GROUND TO THE ANTENNA S RADIAL PLATE AND BOND CONNECT IT TO YOUR MAIN STATION GROUND ROD. THIS BONDING SHOULD ALSO INCLUDE THE HOME S ELECTRIC UTILITY GROUND ROD. MICROWAVELENGTHS THE LAST THREE MICROWAVELENGTHS COLUMNS DESCRIBED MICROWAVE TRANSVERTERS THE BASIC TRANSVERTER, THE LOCAL OSCILLATOR, AND TESTING AND TROUBLESHOOTING. I HOPE THAT YOU HAVE BEEN INSPIRED TO A MPT A TRANSVERTER. AT SOME POINT, YOU WILL WANT TO VERIFY ITS PERFORMANCE. AFTER BENCH DASH TESTING A TRANSVERTER, IT S TIME TO MAKE SOME CONTACTS. THE FIRST ONE IS USUALLY ACROSS THE BACKYARD, WITH VERY LOUD SIGNALS, WHICH CAN BE MISLEADING. INTNR ASE THE DISTANCE TO DOWN THE ROAD, THEN A FEW MILES, AND FINALLY, SOME REAL DX A NS OR <BT> END OF 35WPM TEXT <BT> QST DE W1AW <AS> NA <BT> NOW 30 WPM <BT> HUNDREDS OF KILOM A S. IF YOU CAN OPERATE NEXT TO ANOTHER STATION, PERHAPS AT A ROVER SITE, YO A CAN COMPARE SIGNALS. THE NORTH EAST WEAK SIGNAL GROUP AND THE SAN DIEGO MIK ROWAVE GROUP G OGETHER TO COMPARE AND MEASURE STATIONS. IT IS A CHANCE TO COMPARE MINIMUM DISCERNABLE SIGNAL AND TRANSMIT EFFE RTIVE RADIATED POWER ERP TESTS ON 10 GHZ AND I EGHER BANDS ■ THES STS REQUIRE SOME DEDICATED EQUIPMENT THAT MOST MICROWAVERS DON T HAVE. SO, WHAT CAN BE DONE WITH SIMPLE EQUIPMENT AT A LMWCOST? VERIFYING PERFORMANCE RELATIVE TRANSMIT ERP IS PR Y STRAIGHTFORWARD. D CT THE NADIATED SIGNAL WITH A SECOND ANTENNA AT A REASONABLE DISTANCE USING A POWER INDICATOR OR THE TINYSA ULTRA SP CTRUM ANALYZER AND ADJUST FOR THE MAXIMUM. AT MICROWAVE FREQUENCIES, PERFORMANCE IS D RMINED BY THE NOISE FIGURE NF MORE THAN GAIN. MEASURING NF IS DIFFICULT. THE TINK SA ULTRE CAN MEASURE NF BUT REQUIRES A CALIBRATED <BT> END OF 30WPM TEXT <BT> QS";

    /// <summary>
    /// Two word lists aligned by their longest common run of words: each difference as the live words and the replay words,
    /// with the replay's time where it has one.
    /// </summary>
    internal static List<(string Live, string Replay, double At)> Differences(IReadOnlyList<string> live, IReadOnlyList<(string Word, double At)> replay)
    {
        var n = live.Count;
        var m = replay.Count;
        var lcs = new int[n + 1, m + 1];

        for (var i = n - 1; i >= 0; i--)
        {
            for (var j = m - 1; j >= 0; j--)
            {
                lcs[i, j] = live[i] == replay[j].Word ? lcs[i + 1, j + 1] + 1 : Math.Max(lcs[i + 1, j], lcs[i, j + 1]);
            }
        }

        var diffs = new List<(string, string, double)>();
        var a = 0;
        var b = 0;
        var liveRun = new List<string>();
        var replayRun = new List<(string Word, double At)>();

        void Flush()
        {
            if (liveRun.Count > 0 || replayRun.Count > 0)
            {
                diffs.Add((string.Join(' ', liveRun), string.Join(' ', replayRun.Select(r => r.Word)), replayRun.Count > 0 ? replayRun[0].At : (b < m ? replay[b].At : double.NaN)));
                liveRun.Clear();
                replayRun.Clear();
            }
        }

        while (a < n || b < m)
        {
            if (a < n && b < m && live[a] == replay[b].Word)
            {
                Flush();
                a++;
                b++;
            }
            else if (b < m && (a == n || lcs[a, b + 1] >= lcs[a + 1, b]))
            {
                replayRun.Add(replay[b++]);
            }
            else
            {
                liveRun.Add(live[a++]);
            }
        }

        Flush();

        return diffs;
    }

    /// <summary>A replay's words with when each began.</summary>
    internal static List<(string Word, double At)> Words(IReadOnlyList<Letter> letters)
    {
        var words = new List<(string, double)>();
        var word = new System.Text.StringBuilder();
        var started = 0.0;

        foreach (var l in letters)
        {
            if (l.SpaceBefore && word.Length > 0)
            {
                words.Add((word.ToString(), started));
                word.Clear();
            }

            if (word.Length == 0)
            {
                started = l.From;
            }

            word.Append(l.Text);
        }

        if (word.Length > 0)
        {
            words.Add((word.ToString(), started));
        }

        return words;
    }

    /// <summary>A replay's text, a space wherever the reader put one.</summary>
    internal static string Text(IEnumerable<Letter> letters)
        => string.Concat(letters.Select(l => (l.SpaceBefore ? " " : string.Empty) + l.Text)).Trim();

    /// <remarks>
    /// **W1AW'S FAST TEXT READS WHOLE** (work instruction 554, task 2, HM-DEC-258). At HEAD the replay read AND as TND at 546.7 s:
    /// the radio's AGC lifts the noise in a word gap to 10 to 20 dB under the key-down level, the window read it by level as part
    /// of A's dit, and the mark came out twice a dit long. With a mark read by level trimmed to where it stands at its sender's
    /// level, A is read at 546.6 s and the words the live text broke read whole. Prints the window's level and the marks over AND.
    /// </remarks>
    [Fact]
    public void TheFastTextReadsWhole()
    {
        CwEnvelopeDetector? detector = null;
        var marks = new List<CwMark>();
        long seen = 0;
        var r = Run(
            setUp: d =>
            {
                detector = d;
                d.LaneTrace = [];
            },
            onChunk: t =>
            {
                var batch = detector!.MarksSince(seen);
                seen = batch.Marks.Count > 0 ? batch.Marks[^1].Sequence : seen;
                marks.AddRange(batch.Marks.Where(m => m.FromSeconds is >= 546.0 and <= 547.6));
            });

        foreach (var l in r.Letters.Where(l => l.From is >= 546.0 and <= 547.6))
        {
            output.WriteLine(FormattableString.Invariant($"letter | {l.From:F3} | {l.To:F3} | {l.Text}"));
        }

        foreach (var t in detector!.LaneTrace!.Where(t => t.Seconds is >= 546.6 and <= 546.8))
        {
            output.WriteLine(FormattableString.Invariant($"trace | {t.Seconds:F3} | lane {t.LaneDb:F1} | sender {t.SenderDb:F1} | dit {t.DitSeconds * 1000:F0} | delay {t.DelaySeconds * 1000:F0}"));
        }

        foreach (var m in marks)
        {
            output.WriteLine(FormattableString.Invariant($"mark | {m.FromSeconds:F3} | {(m.ToSeconds - m.FromSeconds) * 1000:F0} ms | {m.PitchHz:F0} Hz | {m.LevelDb:F1} dB"));
        }

        var words = Words(r.Letters).Select(w => w.Word).ToHashSet();

        Assert.Contains(r.Letters, l => l.Text == "A" && l.From is >= 546.55 and <= 546.70);
        Assert.All(new[] { "AND", "ATTEMPT", "EFFECTIVE", "PRETTY", "DETECT", "RADIATED", "SPECTRUM", "DETERMINED" }, w => Assert.Contains(w, words));
    }

    /// <remarks>
    /// **WHAT IS LEFT OF THE GHOST** (work instruction 554, task 2): the replay starts cold, and live the chain had been listening
    /// since before 19:59. Twenty seconds of a station at 550 Hz, 20 WPM, keyed in front of the session, so that a sequence stands
    /// there when W1AW begins, the way one may have stood live: whether a sender at 550 Hz then keeps taking marks through the
    /// session, as the live sheets' 147 to 978 marks did.
    /// </remarks>
    [Fact]
    public void ASequenceStandingAt550BeforeTheSession()
    {
        var rate = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(Pieces[0])).SampleRate;
        var low = AHandIsReadAgainstItselfTests.Keyed("CQ CQ CQ DE K1ABC K1ABC K", _ => new AHandIsReadAgainstItselfTests.Sending(20, 0), 5540, 24, 0, 550);
        var factor = rate / 8000;
        var before = new float[low.Length * factor];

        for (var i = 0; i < before.Length; i++)
        {
            var x = (double)i / factor;
            var k = Math.Min((int)x, low.Length - 2);

            before[i] = (float)(low[k] + ((low[k + 1] - low[k]) * (x - k)));
        }

        var r = Run(before: before);

        foreach (var g in r.Senders.GroupBy(s => Math.Round(s.PitchHz / 25) * 25).OrderBy(g => g.Key))
        {
            output.WriteLine(string.Create(CultureInfo.InvariantCulture,
                $"sender after a 550 Hz station | {g.Key:0} Hz | seen {g.Min(s => s.Seconds):0} to {g.Max(s => s.Seconds):0} s, {g.Select(s => s.Seconds).Distinct().Count()} seconds held | marks up to {g.Max(s => s.Marks)} | shape up to {g.Max(s => s.Shape):0.00} | printed {g.Count(s => s.Printed)} s"));
        }

        output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"before | {before.Length / (double)rate:0.0} s at 550 Hz; text | {Text(r.Letters)[..Math.Min(300, Text(r.Letters).Length)]}"));

        Assert.Contains(r.Letters, l => Math.Abs(l.PitchHz - 600) <= 15);
    }

    /// <remarks>
    /// The same replay with the detector summing the whole audio band, as it does whenever the radio's mode, pitch or filter is
    /// unread: whether the 550 Hz sender comes with the passband.
    /// </remarks>
    [Fact]
    public void TheSessionReplayedOverTheWholeBand()
    {
        var r = Run(50, wholeBand: true);

        foreach (var g in r.Senders.GroupBy(s => Math.Round(s.PitchHz / 25) * 25).OrderBy(g => g.Key))
        {
            output.WriteLine(string.Create(CultureInfo.InvariantCulture,
                $"sender whole band | {g.Key:0} Hz | seen {g.Min(s => s.Seconds):0} to {g.Max(s => s.Seconds):0} s, {g.Select(s => s.Seconds).Distinct().Count()} seconds held | marks up to {g.Max(s => s.Marks)} | shape up to {g.Max(s => s.Shape):0.00}"));
        }

        Assert.Contains(r.Letters, l => Math.Abs(l.PitchHz - 600) <= 15);
    }

    /// <remarks>
    /// The same replay in the live capture's own chunks, 50 ms each, as WASAPI hands them over: whether the 550 Hz sender comes
    /// with the chunk timing. Prints the senders held and the text's first words.
    /// </remarks>
    [Fact]
    public void TheSessionReplayedInLiveChunks()
    {
        var r = Run(50);

        foreach (var g in r.Senders.GroupBy(s => Math.Round(s.PitchHz / 25) * 25).OrderBy(g => g.Key))
        {
            output.WriteLine(string.Create(CultureInfo.InvariantCulture,
                $"sender 50 ms | {g.Key:0} Hz | seen {g.Min(s => s.Seconds):0} to {g.Max(s => s.Seconds):0} s, {g.Select(s => s.Seconds).Distinct().Count()} seconds held | marks up to {g.Max(s => s.Marks)} | shape up to {g.Max(s => s.Shape):0.00}"));
        }

        output.WriteLine("text 50 ms | " + Text(r.Letters));

        Assert.Contains(r.Letters, l => Math.Abs(l.PitchHz - 600) <= 15);
    }

    /// <remarks>
    /// Every sender held, by pitch: when first and last seen, the most marks and the best shape; the text; and every mark at 550
    /// or 650 Hz set against W1AW's marks in time and level. Asserts only that W1AW was read.
    /// </remarks>
    [Fact]
    public void TheSessionReplayed()
    {
        var r = Run();
        var inv = CultureInfo.InvariantCulture;

        output.WriteLine(string.Create(inv, $"replayed {r.Seconds:0} s, {r.Letters.Count} letters, {r.Marks.Count} marks"));

        foreach (var g in r.Senders.GroupBy(s => Math.Round(s.PitchHz / 25) * 25).OrderBy(g => g.Key))
        {
            output.WriteLine(string.Create(inv,
                $"sender | {g.Key:0} Hz | seen {g.Min(s => s.Seconds):0} to {g.Max(s => s.Seconds):0} s, {g.Select(s => s.Seconds).Distinct().Count()} seconds held | marks up to {g.Max(s => s.Marks)} | shape up to {g.Max(s => s.Shape):0.00} | printed {g.Count(s => s.Printed)} s"));
        }

        var w1aw = r.Marks.Where(m => Math.Abs(m.PitchHz - 600) <= 15).OrderBy(m => m.FromSeconds).ToList();
        var ghosts = r.Marks.Where(m => Math.Abs(m.PitchHz - 550) <= 15 || Math.Abs(m.PitchHz - 650) <= 15).OrderBy(m => m.FromSeconds).ToList();
        var inStep = 0;
        var inGaps = 0;
        var neither = 0;
        var levels = new List<double>();

        foreach (var g in ghosts)
        {
            var over = w1aw.Where(w => w.FromSeconds < g.ToSeconds && w.ToSeconds > g.FromSeconds).ToList();

            if (over.Count == 0)
            {
                inGaps++;
            }
            else if (over.Any(w => Math.Abs(w.FromSeconds - g.FromSeconds) <= 0.02 && Math.Abs(w.ToSeconds - g.ToSeconds) <= 0.02))
            {
                inStep++;
                levels.Add(g.LevelDb - over[0].LevelDb);
            }
            else
            {
                neither++;
                levels.Add(g.LevelDb - over[0].LevelDb);
            }
        }

        output.WriteLine(string.Create(inv,
            $"marks at 550 or 650 Hz: {ghosts.Count}; in step with a W1AW mark (both edges within 20 ms) {inStep}; wholly in W1AW's gaps {inGaps}; overlapping but not in step {neither}; level against the W1AW mark beside it, median {(levels.Count > 0 ? levels.Order().ElementAt(levels.Count / 2) : double.NaN):0.0} dB"));

        foreach (var g in ghosts.Take(40))
        {
            var over = w1aw.FirstOrDefault(w => w.FromSeconds < g.ToSeconds && w.ToSeconds > g.FromSeconds);

            output.WriteLine(string.Create(inv,
                $"  ghost {g.FromSeconds:0.000}-{g.ToSeconds:0.000} s at {g.PitchHz:0} Hz {g.LevelDb:0.0} dB; W1AW {(over is null ? "none" : $"{over.FromSeconds:0.000}-{over.ToSeconds:0.000} s {over.LevelDb:0.0} dB")}"));
        }

        foreach (var chunk in Text(r.Letters).Chunk(160))
        {
            output.WriteLine("text | " + new string(chunk));
        }

        // Every word with its time, for the comparison with what printed live.
        var word = new System.Text.StringBuilder();
        var started = 0.0;

        foreach (var l in r.Letters)
        {
            if (l.SpaceBefore && word.Length > 0)
            {
                output.WriteLine(string.Create(inv, $"word | {started:0.00} | {word}"));
                word.Clear();
            }

            if (word.Length == 0)
            {
                started = l.From;
            }

            word.Append(l.Text);
        }

        output.WriteLine(string.Create(inv, $"word | {started:0.00} | {word}"));

        // **REPLAY AGAINST LIVE** (task 2): every difference, word by word, with the replay's time.
        var live = Live.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var diffs = Differences(live, Words(r.Letters));

        foreach (var (l, p, at) in diffs)
        {
            output.WriteLine(string.Create(inv, $"diff | {at:0.0} s | live `{l}` | replay `{p}`"));
        }

        output.WriteLine($"diffs | {diffs.Count} places; live words {live.Length}, replay words {Words(r.Letters).Count}");

        Assert.Contains(r.Letters, l => Math.Abs(l.PitchHz - 600) <= 15);
    }
}
