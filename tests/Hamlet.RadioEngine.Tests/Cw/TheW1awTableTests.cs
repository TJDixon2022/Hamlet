using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;
using static Hamlet.RadioEngine.Tests.Cw.TheRecordingsScoreboardTests;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **SEVENTEEN MINUTES OF W1AW JOIN THE BOARD** (work instruction 554, task 3, HM-DEC-258): the four pieces of the W1AW session
/// of 2026-10-07, 20:00 UTC fast code practice, scored as the twelve are, in a table of their own. R88 is lifted for these four
/// pieces. The reference is the offline read below, anchored on what Hamlet printed live and on English where the live text
/// stumbled; a stretch is high where the offline read and the live text agree and medium where they differ.
/// </summary>
public sealed class TheW1awTableTests(ITestOutputHelper output)
{
    /// <summary>
    /// The reference, stretch by stretch: the offline read (<see cref="EachPieceReadOffline"/>) in stretches of about twelve
    /// words, each word put to English where the offline read or the live text stumbled. **High** where the two reads agree on
    /// every word, **medium** where any word differed or was corrected (<see cref="TheOfflineReadAgainstLive"/>). Left out, so
    /// whatever prints there is not scored: piece 1's first QST, read `I E QSN` offline; the west coast station's call at 36 to
    /// 41 s of piece 2, a run of dahs neither read can part, after K; the `E` and the `TEA` sent between `AS` and `BT`, which the
    /// two reads do not agree on; and the Y the piece boundary cuts between pieces 3 and 4. Corrected to English: QSA DI to QST
    /// DE, 2026EISSUE, the unread 13, ATD to AND (both reads lost N's dit), TSE to THE, ATTACS, INTNR ASE to INCREASE (both reads
    /// part C), IEIGHER, BANDS with its period, WI to W at the end. MIKROWAVE is kept: both reads hear a K.
    /// </summary>
    internal static readonly Stretch[] Stretches =
    [
        new("w1aw-2026-10-07/piece-01", 600, 64.52, 103.69, "QST QST DE W1AW W1AW W1AW QST QST QST DE W1AW W1AW", Confidence.Medium, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-01", 600, 103.72, 150.57, "W1AW QST QST QST DE W1AW W1AW W1AW THE COMPLETE W1AW SCHEDULE", Confidence.Medium, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-01", 600, 150.60, 198.09, "APPEARS IN THE SEPTEMBER 2026 ISSUE OF QST ON PAGE 28. PRACTICE AT", Confidence.Medium, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-01", 600, 198.12, 240.33, "35 30 25 20 15 13 AND 10 WPM FOLLOWS. THE NEXT", Confidence.Medium, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-01", 600, 240.36, 279.85, "QUALIFYING RUNS SENT BY W1AW WILL BE ON OCTOBER 5 AT 4", Confidence.High, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-01", 600, 279.88, 299.92, "PM EDT, OCTOBER 7 AT 7 PM ED", Confidence.Medium, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-02", 600, 0.04, 36.01, "T, AND OCTOBER 8 AT 10 PM EDT. WEST COAST STATION", Confidence.Medium, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-02", 600, 41.00, 95.20, "WILL TRANSMIT THE OFFICIAL ARRL QUALIFYING RUN ON THURSDAY, OCTOBER 29 AT", Confidence.High, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-02", 600, 95.23, 144.64, "9 PM PDT, OR OCTOBER 30 AT 0400Z, ON 3581.5. THE SPEEDS", Confidence.High, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-02", 600, 144.67, 188.11, "WILL RUN FROM 10 TO 35 WPM. <AR> TEXT IS FROM OCTOBER", Confidence.Medium, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-02", 600, 188.14, 226.66, "2024 QST PAGE 51 35 WPM TEXT FOLLOWS <AR> <BT> QST DE", Confidence.High, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-02", 600, 226.69, 236.87, "W1AW <AS>", Confidence.High, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-02", 600, 237.64, 251.07, "<BT> NOW 35 WPM <BT> 8 FEET LONG AND AT LEAST 30", Confidence.Medium, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-02", 600, 251.10, 267.46, "INCHES DEEP. THEN LAY THE GROUND ROD AT THE BOTTOM OF THE", Confidence.Medium, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-02", 600, 267.49, 287.63, "TRENCH AND MAKE YOUR CONNECTION USING A CRIMP OR WELDED NOT SOLDERED", Confidence.High, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-02", 600, 287.66, 299.92, "CONNECTOR. PACK THE BOTTOM OF THE TRENC", Confidence.Medium, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-03", 600, -0.05, 21.94, "H WITH GROUND ENHANCEMENT MATERIAL GEM BEFORE BURYING THE GROUND ROD. YOU", Confidence.Medium, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-03", 600, 21.97, 41.77, "CAN GET GEM FROM AN ELECTRICAL SUPPLY HOUSE. ATTACH THE GROUND TO", Confidence.Medium, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-03", 600, 41.80, 58.09, "THE ANTENNA S RADIAL PLATE AND BOND CONNECT IT TO YOUR MAIN", Confidence.High, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-03", 600, 58.12, 79.77, "STATION GROUND ROD. THIS BONDING SHOULD ALSO INCLUDE THE HOME S ELECTRIC", Confidence.High, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-03", 600, 79.80, 112.62, "UTILITY GROUND ROD. MICROWAVELENGTHS THE LAST THREE MICROWAVELENGTHS COLUMNS DESCRIBED MICROWAVE TRANSVERTERS", Confidence.High, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-03", 600, 112.65, 138.35, "THE BASIC TRANSVERTER, THE LOCAL OSCILLATOR, AND TESTING AND TROUBLESHOOTING. I HOPE", Confidence.High, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-03", 600, 138.38, 157.48, "THAT YOU HAVE BEEN INSPIRED TO ATTEMPT A TRANSVERTER. AT SOME POINT,", Confidence.Medium, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-03", 600, 157.51, 177.31, "YOU WILL WANT TO VERIFY ITS PERFORMANCE. AFTER BENCH DASH TESTING A", Confidence.High, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-03", 600, 177.34, 194.32, "TRANSVERTER, IT S TIME TO MAKE SOME CONTACTS. THE FIRST ONE IS", Confidence.High, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-03", 600, 194.35, 219.36, "USUALLY ACROSS THE BACKYARD, WITH VERY LOUD SIGNALS, WHICH CAN BE MISLEADING.", Confidence.High, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-03", 600, 219.39, 236.30, "INCREASE THE DISTANCE TO DOWN THE ROAD, THEN A FEW MILES, AND", Confidence.Medium, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-03", 600, 236.33, 252.49, "FINALLY, SOME REAL DX AT TENS OR <BT> END OF 35WPM TEXT", Confidence.Medium, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-03", 600, 252.52, 257.95, "<BT> QST DE W1AW <AS>", Confidence.High, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-03", 600, 261.42, 283.38, "<BT> NOW 30 WPM <BT> HUNDREDS OF KILOMETERS. IF YOU CAN OPERATE", Confidence.Medium, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-03", 600, 283.41, 299.87, "NEXT TO ANOTHER STATION, PERHAPS AT A ROVER SITE,", Confidence.High, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-04", 600, 1.66, 23.71, "CAN COMPARE SIGNALS. THE NORTH EAST WEAK SIGNAL GROUP AND THE SAN", Confidence.High, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-04", 600, 23.74, 48.50, "DIEGO MIKROWAVE GROUP GET TOGETHER TO COMPARE AND MEASURE STATIONS. IT IS", Confidence.Medium, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-04", 600, 48.53, 76.36, "A CHANCE TO COMPARE MINIMUM DISCERNABLE SIGNAL AND TRANSMIT EFFECTIVE RADIATED POWER", Confidence.Medium, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-04", 600, 76.39, 97.00, "ERP TESTS ON 10 GHZ AND HIGHER BANDS. THESE TESTS REQUIRE SOME", Confidence.Medium, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-04", 600, 97.03, 120.84, "DEDICATED EQUIPMENT THAT MOST MICROWAVERS DON T HAVE. SO, WHAT CAN BE", Confidence.High, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-04", 600, 120.87, 148.52, "DONE WITH SIMPLE EQUIPMENT AT A LOW COST? VERIFYING PERFORMANCE RELATIVE TRANSMIT", Confidence.Medium, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-04", 600, 148.55, 174.19, "ERP IS PRETTY STRAIGHTFORWARD. DETECT THE RADIATED SIGNAL WITH A SECOND ANTENNA", Confidence.Medium, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-04", 600, 174.22, 196.28, "AT A REASONABLE DISTANCE USING A POWER INDICATOR OR THE TINYSA ULTRA", Confidence.High, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-04", 600, 196.31, 227.47, "SPECTRUM ANALYZER AND ADJUST FOR THE MAXIMUM. AT MICROWAVE FREQUENCIES, PERFORMANCE IS", Confidence.Medium, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-04", 600, 227.50, 248.28, "DETERMINED BY THE NOISE FIGURE NF MORE THAN GAIN. MEASURING NF IS", Confidence.Medium, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-04", 600, 248.31, 272.27, "DIFFICULT. THE TINYSA ULTRA CAN MEASURE NF BUT REQUIRES A CALIBRATED <BT>", Confidence.Medium, 0, 0, 0, string.Empty),
        new("w1aw-2026-10-07/piece-04", 600, 272.30, 299.92, "END OF 30WPM TEXT <BT> QST DE W", Confidence.Medium, 0, 0, 0, string.Empty),
    ];

    /// <summary>The table's total over the stretches of medium confidence or better, as the twelve are scored.</summary>
    internal sealed record W1awBoard(IReadOnlyList<Scored> Stretches, int Right, int OutOf, int Wrong, int Invented, int SpacesRight, int SpacesOutOf, int SpacesAdded)
    {
        /// <summary>Letters right less wrong less invented (HM-DEC-243).</summary>
        public int Score => Right - Wrong - Invented;
    }

    /// <summary>
    /// Each piece read cold through the live path, as the twelve are, and scored stretch by stretch: a letter belongs to the
    /// stretch its last mark ends in. **Invented** is a letter whose marks overlap none of the offline read's, at any pitch:
    /// no other station keyed through the session.
    /// </summary>
    internal static W1awBoard Score()
    {
        var rules = CwRules.Current;
        var reads = new System.Collections.Concurrent.ConcurrentDictionary<string, (IReadOnlyList<Printed> Letters, List<Mark> Keying)>(StringComparer.Ordinal);

        Parallel.ForEach(TheW1awSessionReplayTests.Pieces, piece =>
        {
            using var _ = CwRules.Use(rules);
            var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(piece));

            reads[piece] = (ReadLive(piece).Letters, OfflineMarks(audio.Samples, audio.SampleRate, RadioState(piece).PitchHz));
        });

        var scored = new List<Scored>();
        var invented = 0;

        foreach (var piece in TheW1awSessionReplayTests.Pieces)
        {
            var (letters, keying) = reads[piece];
            var made = letters.Where(l => !keying.Any(m => m.From < l.Seconds + 0.02 && m.To > (double.IsNaN(l.From) ? l.Seconds : l.From) - 0.02)).ToList();

            invented += made.Count;

            foreach (var s in Stretches.Where(s => s.Recording == piece))
            {
                var mine = letters.Where(l => l.Seconds >= s.From && l.Seconds < s.To).ToList();
                var printed = TheRecordingsScoreboardTests.Text(mine);
                var wrong = WrongAt(printed, s.Reference).Count(i => i < mine.Count && !made.Contains(mine[i]));

                scored.Add(new Scored(s, printed, Letters(s.Reference).Length, Right(printed, s.Reference), SpacesOf(printed, s.Reference), mine, wrong, mine.Count(made.Contains)));
            }
        }

        var counted = scored.Where(s => s.Stretch.Confidence >= Confidence.Medium).ToList();

        return new W1awBoard(
            scored, counted.Sum(s => s.Right), counted.Sum(s => s.ReferenceLetters), counted.Sum(s => s.Wrong), invented,
            counted.Sum(s => s.Spaces.Right), counted.Sum(s => s.Spaces.OfReference), counted.Sum(s => s.Spaces.Added));
    }

    /// <remarks>The `w1aw` table: every stretch as the twelve's table prints it, and the total.</remarks>
    [Fact]
    public void TheW1awTable()
    {
        var board = Score();

        output.WriteLine("| piece | stretch | confidence | reference | printed | right | spaces right | missing | added | wrong | invented |");
        output.WriteLine("|---|---|---|---|---|---|---|---|---|---|---|");

        foreach (var s in board.Stretches)
        {
            output.WriteLine(string.Create(CultureInfo.InvariantCulture,
                $"| `{s.Stretch.Recording[^8..]}` | {s.Stretch.From:0.#}-{s.Stretch.To:0.#} s | {s.Stretch.Confidence.ToString().ToLowerInvariant()} | `{s.Stretch.Reference}` | `{s.PrintedText}` | {s.Right} of {s.ReferenceLetters} | {s.Spaces.Right} of {s.Spaces.OfReference} | {s.Spaces.Missing} | {s.Spaces.Added} | {s.Wrong} | {s.Invented} |"));
        }

        output.WriteLine(string.Create(CultureInfo.InvariantCulture,
            $"w1aw total: **{board.Right} of {board.OutOf}** letters right, **{board.Wrong}** wrong, **{board.Invented}** invented; **score {board.Score}**; **{board.SpacesRight} of {board.SpacesOutOf}** spaces, {board.SpacesAdded} added"));

        Assert.True(board.Right > 0);
    }

    /// <summary>One mark of the offline read: when it began and ended, in seconds into the piece.</summary>
    internal readonly record struct Mark(double From, double To)
    {
        public double Length => To - From;
    }

    /// <summary>
    /// A piece's marks read offline and non-causally: mixed to nought at the sheet's pitch, through a two-pole low-pass run
    /// forward and back, as <see cref="TheRecordingsScoreboardTests.ReadOffline"/> does for a stretch; cut at half the key-down
    /// level of each ten seconds, and a mark kept only where its top reaches within 3 dB of it.
    /// </summary>
    internal static List<Mark> OfflineMarks(float[] x, int rate, double pitchHz, double cutoffHz = 60)
    {
        var n = x.Length;
        var step = rate / 1000;
        var i0 = new double[n];
        var q0 = new double[n];

        for (var k = 0; k < n; k++)
        {
            var phase = 2 * Math.PI * pitchHz * k / rate;

            i0[k] = x[k] * Math.Cos(phase);
            q0[k] = x[k] * Math.Sin(phase);
        }

        var a = 1 - Math.Exp(-2 * Math.PI * cutoffHz / rate);

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

        var db = new double[n / step];

        for (var k = 0; k < db.Length; k++)
        {
            var j = k * step;

            db[k] = 10 * Math.Log10((i0[j] * i0[j]) + (q0[j] * q0[j]) + 1e-20);
        }

        // The key-down level drifts with the radio's AGC over five minutes: the cut is taken over each ten seconds on its own.
        var marks = new List<Mark>();
        var on = false;
        var start = 0;

        for (var w = 0; w < db.Length; w += 10000)
        {
            var slice = db.Skip(w).Take(10000).ToArray();
            var sorted = slice.Order().ToArray();
            var lo = sorted[(int)(0.10 * (sorted.Length - 1))];
            var hi = sorted[(int)(0.95 * (sorted.Length - 1))];

            for (var it = 0; it < 20; it++)
            {
                var mid = (lo + hi) / 2;
                var below = slice.Where(d => d < mid).ToList();
                var above = slice.Where(d => d >= mid).ToList();

                if (below.Count == 0 || above.Count == 0)
                {
                    break;
                }

                lo = below.Average();
                hi = above.Average();
            }

            // Cut at half the key-down amplitude, not at the clusters' midpoint: the radio's AGC lifts the noise in a word gap to
            // within 10 to 20 dB of the key-down level, well over the midpoint.
            var cut = hi - 6;
            var hyst = 1.0;

            // A slice with no clear second level holds no keying.
            if (hi - lo < 10)
            {
                if (on)
                {
                    marks.Add(new Mark(start / 1000.0, w / 1000.0));
                    on = false;
                }

                continue;
            }

            for (var k = w; k < Math.Min(db.Length, w + 10000); k++)
            {
                if (!on && db[k] > cut + hyst)
                {
                    on = true;
                    start = k;
                }
                else if (on && db[k] < cut - hyst)
                {
                    on = false;

                    // A mark's top reaches within 3 dB of the key-down level; a noise hump over the cut does not.
                    if (db.Skip(start).Take(k - start).Max() >= hi - 3)
                    {
                        marks.Add(new Mark(start / 1000.0, k / 1000.0));
                    }
                }
            }
        }

        // A gap under 10 ms joins its marks; a mark under 15 ms is not one.
        var joined = new List<Mark>();

        foreach (var m in marks)
        {
            if (joined.Count > 0 && m.From - joined[^1].To < 0.010)
            {
                joined[^1] = new Mark(joined[^1].From, m.To);
            }
            else
            {
                joined.Add(m);
            }
        }

        joined.RemoveAll(m => m.Length < 0.015);

        return joined;
    }

    /// <summary>One-dimensional k-means on log lengths, seeded where given or at even percentiles; the cluster centres, ascending.</summary>
    internal static double[] Centres(IReadOnlyList<double> lengths, int k, double[]? seeds = null)
    {
        var logs = lengths.Select(l => Math.Log(l)).Order().ToArray();
        var c = seeds?.Select(v => Math.Log(v)).ToArray() ?? Enumerable.Range(0, k).Select(i => logs[(int)((i + 0.5) / k * (logs.Length - 1))]).ToArray();

        for (var it = 0; it < 30; it++)
        {
            var sums = new double[k];
            var counts = new int[k];

            foreach (var l in logs)
            {
                var best = 0;

                for (var j = 1; j < k; j++)
                {
                    if (Math.Abs(l - c[j]) < Math.Abs(l - c[best]))
                    {
                        best = j;
                    }
                }

                sums[best] += l;
                counts[best]++;
            }

            for (var j = 0; j < k; j++)
            {
                c[j] = counts[j] > 0 ? sums[j] / counts[j] : c[j];
            }
        }

        return c.Select(v => Math.Exp(v)).Order().ToArray();
    }

    /// <summary>
    /// The marks read as letters, each mark and gap classed against the forty marks around it: dit or dah at the geometric
    /// midpoint of their two clusters, and a gap inside a letter, between letters or between words at the midpoints of its
    /// three. W1AW changes speed between its sendings, so the lines are local.
    /// </summary>
    internal static List<(double From, string Text, bool SpaceBefore)> OfflineLetters(IReadOnlyList<Mark> marks)
    {
        var letters = new List<(double, string, bool)>();
        var pattern = new StringBuilder();
        var letterFrom = 0.0;
        var space = false;

        void End()
        {
            if (pattern.Length > 0)
            {
                letters.Add((letterFrom, MorseAlphabet.Lookup(pattern.ToString()) ?? "■", space));
                pattern.Clear();
                space = false;
            }
        }

        for (var i = 0; i < marks.Count; i++)
        {
            var lo = Math.Max(0, i - 20);
            var hi = Math.Min(marks.Count, lo + 40);

            lo = Math.Max(0, hi - 40);

            var near = marks.Skip(lo).Take(hi - lo).ToList();
            var kinds = Centres(near.Select(m => m.Length).ToList(), 2);
            var gaps = near.Zip(near.Skip(1), (p, q) => q.From - p.To).Where(g => g < 3).ToList();
            // Seeded at one, three and seven dits: element gaps outnumber the rest, and seeds at percentiles split them in two.
            var g3 = gaps.Count >= 6 ? Centres(gaps, 3, [kinds[0], 3 * kinds[0], 7 * kinds[0]]) : [kinds[0], 3 * kinds[0], 7 * kinds[0]];
            var letterLine = Math.Sqrt(g3[0] * g3[1]);
            var wordLine = Math.Sqrt(g3[1] * g3[2]);

            // Where the window holds no word gap, its third cluster is a long letter gap: the word line is never under five dits.
            wordLine = Math.Max(wordLine, 5 * kinds[0]);

            if (i > 0)
            {
                var gap = marks[i].From - marks[i - 1].To;

                if (gap >= letterLine)
                {
                    End();
                    space |= gap >= wordLine;
                }
            }

            if (pattern.Length == 0)
            {
                letterFrom = marks[i].From;
            }

            pattern.Append(marks[i].Length >= Math.Sqrt(kinds[0] * kinds[1]) ? '-' : '.');
        }

        End();

        return letters;
    }

    /// <summary>A letter list as text, a space wherever one was read.</summary>
    internal static string Text(IEnumerable<(double From, string Text, bool SpaceBefore)> letters)
        => string.Concat(letters.Select(l => (l.SpaceBefore ? " " : string.Empty) + l.Text)).Trim();

    /// <remarks>Each piece read offline, printed in lines of about ten seconds with the time each begins.</remarks>
    /// <param name="piece">The piece, 1 to 4.</param>
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void EachPieceReadOffline(int piece)
    {
        var name = TheW1awSessionReplayTests.Pieces[piece - 1];
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(name));
        var (pitch, _) = TheRecordingsScoreboardTests.RadioState(name);
        var marks = OfflineMarks(audio.Samples, audio.SampleRate, pitch);
        var letters = OfflineLetters(marks);

        output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"piece {piece}: {marks.Count} marks, {letters.Count} letters, pitch {pitch}"));

        foreach (var line in letters.GroupBy(l => (int)(l.From / 10)))
        {
            output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"offline | {piece} | {line.Key * 10,3} s | {Text(line)}"));
        }

        Assert.NotEmpty(letters);
    }
    /// <remarks>The four pieces read offline set against the live text, word by word, each difference with its piece and time.</remarks>
    [Fact]
    public void TheOfflineReadAgainstLive()
    {
        var words = new List<(string Word, double At)>();

        for (var p = 0; p < 4; p++)
        {
            var name = TheW1awSessionReplayTests.Pieces[p];
            var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(name));
            var (pitch, _) = TheRecordingsScoreboardTests.RadioState(name);
            var letters = OfflineLetters(OfflineMarks(audio.Samples, audio.SampleRate, pitch));

            words.AddRange(TheW1awSessionReplayTests.Words(letters.Select(l => new TheW1awSessionReplayTests.Letter(l.From + (300 * p), l.From + (300 * p), pitch, l.Text, l.SpaceBefore)).ToList()));

            foreach (var l in letters.Where(l => l.Text == "■"))
            {
                output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"unread | {p + 1} | {l.From:0.00} s"));
            }
        }

        foreach (var (w, at) in words)
        {
            output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"offword | {(int)(at / 300) + 1} | {at % 300:0.000} | {w}"));
        }

        var live = TheW1awSessionReplayTests.Live.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var (l, o, at) in TheW1awSessionReplayTests.Differences(live, words))
        {
            output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"offdiff | piece {(int)(at / 300) + 1} {at % 300:0.0} s | live `{l}` | offline `{o}`"));
        }

        Assert.NotEmpty(words);
    }
}
