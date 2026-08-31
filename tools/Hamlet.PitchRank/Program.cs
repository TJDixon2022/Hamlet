using System.Diagnostics;
using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;

namespace Hamlet.PitchRank;

/// <summary>
/// What a decode costs at one pitch, and which pitch across the band reads
/// best, measured over the capture corpus.
/// </summary>
/// <remarks>
/// <para>**IT MEASURES AND IT CHANGES NOTHING.** Unit 043 task 2 asks whether
/// ranking candidates by the decoder's own score picks the pitch that reads
/// best more often than the tracker does today. That question is answered by
/// running the shipped decoder at every candidate pitch and comparing, which is
/// too slow to belong in the test suite and too specific to belong in the
/// application.</para>
/// <para>**NOTHING HERE KEYS A TRANSMITTER** (§0.2). It reads WAV files.</para>
/// </remarks>
internal static class Program
{
    /// <summary>The tracker's own coarse grid, which is the candidate set.</summary>
    private const double StepHz = CwToneTracker.CoarseSpacingHz;

    private static int Main(string[] args)
    {
        // The transcripts carry Hamlet's own unreadable mark, which is not in
        // the console's default codepage.
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var what = args.Length > 0 ? args[0] : "help";

        switch (what)
        {
            case "cost":
                Cost();

                return 0;

            case "rank":
                Rank(args.Length > 1 ? args[1] : null);

                return 0;

            case "shipped":
                ShippedRank(args.Length > 1 ? args[1] : null);

                return 0;

            case "pedestal":
                Pedestal(args.Length > 1 ? args[1] : null);

                return 0;

            case "live":
                Live(
                    args.Length > 2 ? args[2] : null,
                    ranking: args.Length > 1 && args[1] == "ranked");

                return 0;

            case "floor":
                FloorSweep();

                return 0;

            case "window":
                WindowSweep();

                return 0;

            case "reference":
                Reference(args.Length > 1 ? args[1] : null);

                return 0;

            case "headtohead":
                HeadToHead();

                return 0;

            case "refcost":
                ReferenceCost();

                return 0;

            case "clock":
                ClockSweep();

                return 0;

            case "carriers":
                Carriers();

                return 0;

            case "bothscore":
                foreach (var pair in new[]
                         {
                             (Setback: 6.0, Hold: 12.0),
                         })
                {
                    CwUnitEstimator.PeakSetbackDb = pair.Setback;
                    CwUnitEstimator.HoldOverMilliseconds = pair.Hold;

                    var t = ScoreTotals();

                    Console.WriteLine(
                        "{0}	hold {1:0} ms	yield {2:0.000}	precision {3:0.000}	subs {4}",
                        double.IsNaN(pair.Setback) ? "otsu    " : "peak-6dB",
                        pair.Hold, t.Yield, t.Precision, t.Subs);
                }

                CwUnitEstimator.PeakSetbackDb = double.NaN;
                CwUnitEstimator.HoldOverMilliseconds = 12.0;

                return 0;

            case "holdscore":
                foreach (var h in args.Skip(1))
                {
                    CwUnitEstimator.HoldOverMilliseconds =
                        double.Parse(h, CultureInfo.InvariantCulture);

                    var t = ScoreTotals();

                    Console.WriteLine(
                        "{0} ms	yield {1:0.000}	precision {2:0.000}	subs {3}",
                        h, t.Yield, t.Precision, t.Subs);
                }

                CwUnitEstimator.HoldOverMilliseconds = 0;

                return 0;

            case "holdover":
                HoldOverSweep();

                return 0;

            case "scatter":
                ScatterSweep();

                return 0;

            case "setback":
                SetbackSweep(args.Skip(1).ToArray());

                return 0;

            case "fading":
                Fading();

                return 0;

            case "peakwindow":
                PeakWindow();

                return 0;

            case "presence":
                Presence();

                return 0;

            case "thresholds":
                Thresholds();

                return 0;

            case "fraction":
                FractionSweep(args.Skip(1).ToArray());

                return 0;

            case "minswing":
                MinSwingSweep(args.Skip(1).ToArray());

                return 0;

            case "peakcost":
                PeakCost();

                return 0;

            case "tone":
                ToneTable(args.Length > 1 ? args[1] : null);

                return 0;

            case "score":
                ScoreSweep(args.Length > 1 && args[1] == "peak");

                return 0;

            case "width":
                WidthSweep();

                return 0;

            case "confidence":
                ConfidenceSweep();

                return 0;

            case "magnitudes":
                Magnitudes();

                return 0;

            case "temperature":
                TemperatureSweep();

                return 0;

            default:
                Console.WriteLine("usage: pitch-rank cost | pitch-rank rank [capture]");

                return 1;
        }
    }

    /// <summary>What one window at one pitch costs, measured rather than guessed.</summary>
    private static void Cost()
    {
        var folder = CaptureFolder();
        var audio = WavAudio.Read(
            Directory.GetFiles(folder, "*.wav").OrderBy(f => f, StringComparer.Ordinal).First());

        // **THE RANKING WINDOW IS THE VARIABLE THIS UNIT HAS TO TRADE**, so the
        // cost is measured across the lengths a ranking pass might use rather
        // than at the streaming window alone.
        foreach (var seconds in new[] { 12.0, 6.0, 4.0, 3.0, 2.0 })
        {
            CostAt(audio, seconds);
            Console.WriteLine();
        }
    }

    /// <summary>What a window of one length costs, at one pitch and across the band.</summary>
    /// <param name="audio">The recording to time against.</param>
    /// <param name="seconds">How long the window is.</param>
    private static void CostAt(MonoAudio audio, double seconds)
    {
        var hop = (int)(audio.SampleRate * CwProbabilisticDecoder.HopMilliseconds / 1000.0);
        var windowSamples = (int)(seconds * audio.SampleRate);

        windowSamples = Math.Min(windowSamples, audio.Samples.Length);
        windowSamples -= windowSamples % hop;

        var window = new MonoAudio(
            audio.SampleRate, audio.Samples[..windowSamples]);

        // Warm the JIT before anything is timed.
        for (var i = 0; i < 3; i++)
        {
            var warm = CwProbabilisticDecoder.Envelope(
                window.Samples, window.SampleRate, 500);

            CwProbabilisticDecoder.Decode(warm, 500);
        }

        const int Reps = 20;

        var mix = Stopwatch.StartNew();
        double[] envelope = [];

        for (var i = 0; i < Reps; i++)
        {
            envelope = CwProbabilisticDecoder.Envelope(
                window.Samples, window.SampleRate, 500);
        }

        mix.Stop();

        var read = Stopwatch.StartNew();

        for (var i = 0; i < Reps; i++)
        {
            CwProbabilisticDecoder.Decode(envelope, 500);
        }

        read.Stop();

        var mixMs = mix.Elapsed.TotalMilliseconds / Reps;
        var readMs = read.Elapsed.TotalMilliseconds / Reps;
        var candidates = Candidates().Count;
        var everyReadMs = CwProbabilisticStream.ReadEverySeconds * 1000;

        Console.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            "window        {0:0.0} s at {1} Hz = {2} samples, {3} hops",
            seconds,
            audio.SampleRate,
            windowSamples,
            envelope.Length));
        Console.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            "mixdown       {0:0.00} ms  (quadrature mix and integrate, one pitch)",
            mixMs));
        Console.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            "decode        {0:0.00} ms  (speed grid {1} to {2}, one pitch)",
            readMs,
            CwProbabilisticDecoder.SlowestWpm,
            CwProbabilisticDecoder.FastestWpm));
        Console.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            "one candidate {0:0.00} ms",
            mixMs + readMs));
        Console.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            "candidates    {0}  ({1:0} to {2:0} Hz at {3:0} Hz)",
            candidates,
            CwToneTracker.MinimumToneHz,
            CwToneTracker.MaximumToneHz,
            StepHz));
        Console.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            "whole sweep   {0:0.0} ms",
            (mixMs + readMs) * candidates));
        Console.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            "every read    {0:0.0}% of one core, sweeping on every read {1:0.0} s apart",
            (mixMs + readMs) * candidates / everyReadMs * 100,
            CwProbabilisticStream.ReadEverySeconds));
        Console.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            "decode only   {0:0.0}% of one core, if the mixdowns were kept rolling",
            readMs * candidates / everyReadMs * 100));
    }

    /// <summary>Every candidate pitch across the band the tracker searches.</summary>
    private static List<double> Candidates()
    {
        var list = new List<double>();

        for (var hz = CwToneTracker.MinimumToneHz;
             hz <= CwToneTracker.MaximumToneHz + 1e-9;
             hz += StepHz)
        {
            list.Add(hz);
        }

        return list;
    }

    /// <summary>Rank every candidate on every capture, and say what won.</summary>
    /// <param name="only">One capture to do, or null for all of them.</param>
    private static void Rank(string? only)
    {
        var files = Directory
            .GetFiles(CaptureFolder(), "*.wav", SearchOption.AllDirectories)
            .Where(f => only is null
                || Path.GetFileName(f).Contains(only, StringComparison.Ordinal))
            .OrderBy(f => Path.GetFileName(f), StringComparer.Ordinal)
            .ToList();

        Console.WriteLine(
            "capture\tsidecarHz\tshippedHz\tatShipped\twinnerHz\twinnerScore\t"
            + "runnerUpHz\trunnerUpScore\tatSidecar\tverdict");

        foreach (var file in files)
        {
            Row(file);
        }
    }

    private static void Row(string file)
    {
        var name = Path.GetFileNameWithoutExtension(file);
        var audio = WavAudio.Read(file);

        // **THE SAME WINDOW THE STREAM WOULD BE HOLDING**, taken from the middle
        // of the recording so a station that starts late is not judged on its
        // own opening silence. Where the file is shorter than a window it is
        // used whole.
        var hop = (int)(audio.SampleRate * CwProbabilisticDecoder.HopMilliseconds / 1000.0);
        var want = (int)(CwProbabilisticStream.WindowSeconds * audio.SampleRate);
        var take = Math.Min(want, audio.Samples.Length);

        take -= take % hop;

        var from = Math.Max(0, (audio.Samples.Length - take) / 2);
        var window = new MonoAudio(audio.SampleRate, audio.Samples[from..(from + take)]);

        var scores = new List<(double Hz, double Score)>();

        foreach (var hz in Candidates())
        {
            var envelope = CwProbabilisticDecoder.Envelope(
                window.Samples, window.SampleRate, hz);

            // **UNGATED, BECAUSE THIS IS A RANKING AND NOT AN EMISSION.** The
            // gate empties the text below 1.40 and leaves the ratio alone, so
            // asking ungated changes no number here; it keeps every candidate
            // scored on the same terms whether or not it would have emitted.
            var read = CwProbabilisticDecoder.DecodeUngated(envelope, hz);

            scores.Add((hz, read.LikelihoodRatio));
        }

        var ranked = scores.OrderByDescending(s => s.Score).ToList();
        var winner = ranked[0];
        var runnerUp = ranked[1];

        var sidecarHz = SidecarToneHz(file);
        var shipped = Shipped(audio);

        var atShipped = double.IsNaN(shipped) ? double.NaN : Nearest(scores, shipped);
        var atSidecar = double.IsNaN(sidecarHz) ? double.NaN : Nearest(scores, sidecarHz);

        Console.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            "{0}\t{1:0.0}\t{2:0.0}\t{3:0.00}\t{4:0}\t{5:0.00}\t{6:0}\t{7:0.00}\t{8:0.00}\t{9}",
            name,
            sidecarHz,
            shipped,
            atShipped,
            winner.Hz,
            winner.Score,
            runnerUp.Hz,
            runnerUp.Score,
            atSidecar,
            double.IsNaN(sidecarHz)
                ? "unknown"
                : Math.Abs(winner.Hz - sidecarHz) <= StepHz ? "match" : "miss"));
    }

    /// <summary>The score at the candidate nearest a pitch.</summary>
    private static double Nearest(List<(double Hz, double Score)> scores, double hz)
        => scores.OrderBy(s => Math.Abs(s.Hz - hz)).First().Score;

    /// <summary>
    /// What every candidate floor on the winner's score would cost and buy,
    /// across the whole corpus.
    /// </summary>
    /// <remarks>
    /// **THE SWEEP IS PUBLISHED RATHER THAN A NUMBER BEING PICKED.** Unit 1.11.33
    /// found that no fixed threshold separates this corpus in the old units;
    /// these are new units and that finding does not carry, but it is the reason
    /// the whole table is reported instead of a chosen value with the working
    /// hidden.
    /// </remarks>
    private static void FloorSweep()
    {
        var files = Directory
            .GetFiles(CaptureFolder(), "*.wav", SearchOption.AllDirectories)
            .OrderBy(f => Path.GetFileName(f), StringComparer.Ordinal)
            .ToList();

        var rows = new List<(string Name, double Score, int Chars, bool Anchor)>();

        // The recordings somebody has ruled on or corroborated. A floor that
        // silences one of these is not a floor, whatever it does to the phantoms.
        // The twelve of `TheAdjudicatedReadingsKeepReadingTests`, plus the three
        // captures of 2026-08-28 that hold the net unit 044's acceptance protects.
        var anchors = new[]
        {
            "cw-2026-08-17-013347",
            "cw-2026-08-17-134712",
            "cw-2026-08-18-003758",
            "cw-2026-08-18-004507",
            "cw-2026-08-22-031838",
            "cw-2026-08-22-031905",
            "cw-2026-08-22-031948",
            "cw-2026-08-22-032012",
            "cw-2026-08-22-032050",
            "cw-2026-08-22-032113",
            "cw-2026-08-22-032129",
            "cw-2026-08-24-012403",
            "cw-2026-08-28-004844",
            "cw-2026-08-28-004902",
            "cw-2026-08-28-004915",
        };

        foreach (var file in files)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            var audio = WavAudio.Read(file);

            // **THE SWEEP RUNS WITH THE RANKING ON EVEN THOUGH IT SHIPS OFF.**
            // The question is what a floor on the winner's score would silence
            // and what it would cost, and that only has an answer where the
            // ranking is the thing driving the decode.
            var decoder = new CwDecoder(audio.SampleRate, 600)
            {
                RankThePitch = true,
            };

            var characters = 0;

            decoder.CharacterSettled += c =>
            {
                if (!c.IsWordGap)
                {
                    characters++;
                }
            };

            var hop = decoder.Tracker.HopSamples;

            for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
            {
                decoder.Process(new AudioChunk(
                    at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
            }

            decoder.Flush();

            rows.Add((
                name,
                decoder.Ranked.Score,
                characters,
                anchors.Contains(name, StringComparer.Ordinal)));
        }

        Console.WriteLine("capture\trankScore\tchars\tanchor");

        foreach (var row in rows.OrderBy(r => r.Score))
        {
            Console.WriteLine(string.Format(
                CultureInfo.InvariantCulture,
                "{0}\t{1:0.00}\t{2}\t{3}",
                row.Name,
                row.Score,
                row.Chars,
                row.Anchor ? "anchor" : ""));
        }

        Console.WriteLine();
        Console.WriteLine("floor\tsilenced\tanchorsLost\tanchorsKept");

        foreach (var floor in new[]
        {
            0.0, 0.25, 0.5, 1.0, 1.5, 2.0, 2.5, 3.0, 4.0, 5.0, 5.5, 6.0, 7.0,
            7.5, 8.0, 10.0, 12.0, 14.0,
        })
        {
            var silenced = rows.Count(r => r.Score < floor && r.Chars > 0);
            var lost = rows.Count(r => r.Anchor && r.Score < floor);

            Console.WriteLine(string.Format(
                CultureInfo.InvariantCulture,
                "{0:0.00}\t{1}\t{2}\t{3}",
                floor,
                silenced,
                lost,
                rows.Count(r => r.Anchor) - lost));
        }
    }

    /// <summary>
    /// What every capture reads through the whole shipped decoder, with whatever
    /// is in the tree right now.
    /// </summary>
    /// <param name="only">One capture to do, or null for all of them.</param>
    /// <remarks>
    /// **THIS IS THE ACCEPTANCE MEASUREMENT AND NOT A RANKING ONE.** The tables
    /// above compare candidates offline; this runs `CwDecoder` over the file the
    /// way the terminal does and prints what the operator would have seen, with
    /// the ranked pitch, both scores, and how many times the ranking ran.
    /// </remarks>
    private static void Live(string? only, bool ranking = true)
    {
        var files = Directory
            .GetFiles(CaptureFolder(), "*.wav", SearchOption.AllDirectories)
            .Where(f => only is null
                || Path.GetFileName(f).Contains(only, StringComparison.Ordinal))
            .OrderBy(f => Path.GetFileName(f), StringComparer.Ordinal)
            .ToList();

        Console.WriteLine(
            "capture\tsidecarHz\tusedHz\trankScore\trunnerUp\trankings\tchars\ttext");

        foreach (var file in files)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            var audio = WavAudio.Read(file);

            var decoder = new CwDecoder(audio.SampleRate, 600)
            {
                RankThePitch = ranking,
            };

            var text = new System.Text.StringBuilder();

            decoder.CharacterSettled += c => text.Append(c.Text);

            var hop = decoder.Tracker.HopSamples;

            for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
            {
                decoder.Process(new AudioChunk(
                    at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
            }

            decoder.Flush();

            var report = decoder.Report;
            var rank = decoder.Ranked;

            Console.WriteLine(string.Format(
                CultureInfo.InvariantCulture,
                "{0}\t{1:0.0}\t{2:0.0}\t{3:0.00}\t{4:0.00}\t{5}\t{6}\t{7}",
                name,
                SidecarToneHz(file),
                report.ToneHz,
                rank.Score,
                rank.RunnerUpScore,
                decoder.Rankings,
                report.CharactersEmitted,
                Clip(text.ToString())));
        }
    }

    /// <summary>
    /// Rank the candidates again with every envelope stood on one common noise
    /// floor, which is the smallest change that could make the score comparable.
    /// </summary>
    /// <param name="only">One capture to do, or null for all of them.</param>
    /// <remarks>
    /// <para>**WHY THE PLAIN SCORE CANNOT RANK PITCHES.** The likelihood ratio
    /// is scale invariant: the noise scale and the keyed level are both
    /// estimated from the envelope being scored, so a bin holding nothing but
    /// the receiver's rolled-off floor is scored against its own tiny sigma, and
    /// small fluctuations then look exactly like marks. The quietest bin in the
    /// band wins, and what it reads is a page of single dits.</para>
    /// <para>**THE PEDESTAL IS THE ONE-LINE TEST OF WHETHER THAT IS THE WHOLE
    /// FAULT.** Every candidate's envelope is combined in power with one noise
    /// level taken across the whole band, which is what each bin would look like
    /// if the receiver's floor were flat. A bin holding nothing goes flat and
    /// scores near nothing; a bin holding a keyed station keeps its marks well
    /// above the pedestal and keeps its structure. It is still the decoder's own
    /// score and it is still a keying measurement rather than a loudness one
    /// (HM-DEC-095), because a carrier is as flat against the pedestal as
    /// silence is.</para>
    /// <para>**IT IS A MEASUREMENT AND NOTHING IN THE APPLICATION DOES IT.**</para>
    /// </remarks>
    private static void Pedestal(string? only)
    {
        var files = Directory
            .GetFiles(CaptureFolder(), "*.wav", SearchOption.AllDirectories)
            .Where(f => only is null
                || Path.GetFileName(f).Contains(only, StringComparison.Ordinal))
            .OrderBy(f => Path.GetFileName(f), StringComparer.Ordinal)
            .ToList();

        Console.WriteLine(
            "capture\tsidecarHz\tbareHz\tbareVerdict\twinnerHz\twinnerScore\t"
            + "runnerUpHz\trunnerUpScore\tatSidecar\tverdict\twinnerText");

        foreach (var file in files)
        {
            PedestalRow(file);
        }
    }

    private static void PedestalRow(string file)
        => PedestalRow(file, CwProbabilisticStream.WindowSeconds, fromStart: false);

    /// <summary>Rank one capture over a stated stretch of it.</summary>
    /// <param name="file">The recording.</param>
    /// <param name="seconds">How long a stretch to rank over.</param>
    /// <param name="fromStart">
    /// True to take the opening seconds, which is what the live decoder sees on
    /// tune-in; false to take the tail, which is where the operator pressed.
    /// </param>
    private static void PedestalRow(string file, double seconds, bool fromStart)
    {
        var name = Path.GetFileNameWithoutExtension(file);
        var whole = WavAudio.Read(file);

        var window = fromStart
            ? new MonoAudio(
                whole.SampleRate,
                whole.Samples[..Math.Min(
                    (int)(seconds * whole.SampleRate), whole.Samples.Length)])
            : Tail(whole, seconds);

        var envelopes = new List<(double Hz, double[] Envelope)>();

        foreach (var hz in Candidates())
        {
            envelopes.Add((
                hz,
                CwProbabilisticDecoder.Envelope(
                    window.Samples, window.SampleRate, hz)));
        }

        // **THE ENGINE'S COPY, NOT THE TOOL'S.** Unit 044 task 1 moved the
        // pedestal into `CwPitchRanking` so the shipped path can call it, and a
        // measurement taken through a second implementation would be measuring
        // the second implementation (§12.5).
        var floor = CwPitchRanking.Floor(
            envelopes.Select(e => e.Envelope).ToList());

        var scores = new List<(double Hz, double Score, string Text)>();

        // **THE SAME WINDOW WITHOUT THE PEDESTAL IS THE CONTROL.** Comparing a
        // pedestal run against a differently-taken window would be comparing two
        // things at once, which is how the last six units produced numbers
        // nobody could act on.
        var bare = new List<(double Hz, double Score)>();

        foreach (var (hz, envelope) in envelopes)
        {
            var stood = CwPitchRanking.StandOn(envelope, floor);

            var read = CwProbabilisticDecoder.DecodeUngated(stood, hz);

            scores.Add((hz, read.LikelihoodRatio, read.Text ?? ""));

            bare.Add((
                hz,
                CwProbabilisticDecoder.DecodeUngated(envelope, hz).LikelihoodRatio));
        }

        var ranked = scores.OrderByDescending(s => s.Score).ToList();
        var winner = ranked[0];
        var runnerUp = ranked[1];
        var bareWinner = bare.OrderByDescending(s => s.Score).First();
        var sidecarHz = SidecarToneHz(file);

        var atSidecar = double.IsNaN(sidecarHz)
            ? double.NaN
            : scores.OrderBy(s => Math.Abs(s.Hz - sidecarHz)).First().Score;

        Console.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            "{0}\t{1:0.0}\t{2:0}\t{3}\t{4:0}\t{5:0.00}\t{6:0}\t{7:0.00}\t{8:0.00}\t{9}\t{10}",
            name,
            sidecarHz,
            bareWinner.Hz,
            Verdict(bareWinner.Hz, sidecarHz),
            winner.Hz,
            winner.Score,
            runnerUp.Hz,
            runnerUp.Score,
            atSidecar,
            Verdict(winner.Hz, sidecarHz),
            Clip(winner.Text)));
    }

    /// <summary>Whether a chosen pitch is within one step of the sheet's.</summary>
    private static string Verdict(double chosenHz, double sidecarHz)
        => double.IsNaN(sidecarHz)
            ? "unknown"
            : Math.Abs(chosenHz - sidecarHz) <= StepHz ? "match" : "miss";


    /// <summary>
    /// Rank the candidates through the path the application actually runs.
    /// </summary>
    /// <param name="only">One capture to do, or null for all of them.</param>
    /// <remarks>
    /// **THE ONE-SHOT READ AND THE STREAM DISAGREE, AND THE STREAM IS WHAT
    /// SHIPS.** `CwProbabilisticDecoder.Decode` over a whole window scores a
    /// window once with no imposed speed and no fitted gap classes; the stream
    /// supplies both and re-reads twice a second. Unit 043's own evidence table
    /// is streaming figures, so a ranking measured any other way is not
    /// measuring the quantity the ruling is about.
    /// </remarks>
    private static void ShippedRank(string? only)
    {
        var files = Directory
            .GetFiles(CaptureFolder(), "*.wav", SearchOption.AllDirectories)
            .Where(f => only is null
                || Path.GetFileName(f).Contains(only, StringComparison.Ordinal))
            .OrderBy(f => Path.GetFileName(f), StringComparer.Ordinal)
            .ToList();

        Console.WriteLine(
            "capture\tsidecarHz\tfreeHz\tfreeScore\twinnerHz\twinnerScore\t"
            + "runnerUpHz\trunnerUpScore\tatSidecar\tverdict\twinnerText");

        foreach (var file in files)
        {
            ShippedRow(file);
        }
    }

    private static void ShippedRow(string file)
    {
        var name = Path.GetFileNameWithoutExtension(file);
        var whole = WavAudio.Read(file);

        // **THE TAIL, BECAUSE THAT IS WHERE THE OPERATOR PRESSES.** A capture is
        // kept at the moment something looked worth keeping, and the window the
        // sheet reports is the one ending there. Twenty seconds is a full
        // twelve-second window plus sixteen reads of new audio on top of it.
        var tail = Tail(whole, 20);

        var scores = new List<(double Hz, double Score, string Text)>();

        foreach (var hz in Candidates())
        {
            var run = Run(tail, hz);

            scores.Add((hz, run.Score, run.Text));
        }

        var ranked = scores.OrderByDescending(s => s.Score).ToList();
        var winner = ranked[0];
        var runnerUp = ranked[1];

        var sidecarHz = SidecarToneHz(file);
        var free = Run(tail, double.NaN);

        var atSidecar = double.IsNaN(sidecarHz)
            ? double.NaN
            : scores.OrderBy(s => Math.Abs(s.Hz - sidecarHz)).First().Score;

        Console.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            "{0}\t{1:0.0}\t{2:0.0}\t{3:0.00}\t{4:0}\t{5:0.00}\t{6:0}\t{7:0.00}\t{8:0.00}\t{9}\t{10}",
            name,
            sidecarHz,
            free.ToneHz,
            free.Score,
            winner.Hz,
            winner.Score,
            runnerUp.Hz,
            runnerUp.Score,
            atSidecar,
            double.IsNaN(sidecarHz)
                ? "unknown"
                : Math.Abs(winner.Hz - sidecarHz) <= StepHz ? "match" : "miss",
            Clip(winner.Text)));
    }

    /// <summary>
    /// How often the ranking picks the station, against the length of the
    /// stretch it reads and where in the recording that stretch is taken.
    /// </summary>
    /// <remarks>
    /// **THE 34 OF 44 WAS MEASURED ON THE TAIL OF A TWELVE-SECOND WINDOW**, and
    /// the live decoder ranks four seconds taken at tune-in. Those are two
    /// different measurements and this is what separates them.
    /// </remarks>
    private static void WindowSweep()
    {
        var files = Directory
            .GetFiles(CaptureFolder(), "*.wav", SearchOption.AllDirectories)
            .OrderBy(f => Path.GetFileName(f), StringComparer.Ordinal)
            .ToList();

        Console.WriteLine("seconds	where	matches	of");

        foreach (var seconds in new[] { 4.0, 6.0, 8.0, 12.0 })
        {
            foreach (var fromStart in new[] { true, false })
            {
                var matches = 0;
                var total = 0;

                foreach (var file in files)
                {
                    var whole = WavAudio.Read(file);
                    var sidecar = SidecarToneHz(file);

                    if (double.IsNaN(sidecar))
                    {
                        continue;
                    }

                    var window = fromStart
                        ? new MonoAudio(
                            whole.SampleRate,
                            whole.Samples[..Math.Min(
                                (int)(seconds * whole.SampleRate),
                                whole.Samples.Length)])
                        : Tail(whole, seconds);

                    var ranked = CwPitchRanking.Rank(
                        window.Samples, window.SampleRate);

                    total++;

                    if (ranked.Ranked
                        && Math.Abs(ranked.ToneHz - sidecar) <= StepHz)
                    {
                        matches++;
                    }
                }

                Console.WriteLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "{0:0}	{1}	{2}	{3}",
                    seconds,
                    fromStart ? "opening" : "tail",
                    matches,
                    total));
            }
        }
    }

    /// <summary>What the ported reference decoder reads on every capture.</summary>
    /// <param name="only">One capture to do, or null for all of them.</param>
    /// <remarks>
    /// **THE PORT IS ONLY A PORT IF IT AGREES WITH ITS SOURCE**, so this prints
    /// the same fields `cwdecoder.py` prints and the two are diffed rather than
    /// eyeballed.
    /// </remarks>
    private static void Reference(string? only)
    {
        var files = Directory
            .GetFiles(CaptureFolder(), "*.wav", SearchOption.AllDirectories)
            .Where(f => only is null
                || Path.GetFileName(f).Contains(only, StringComparison.Ordinal))
            .OrderBy(f => Path.GetFileName(f), StringComparer.Ordinal)
            .ToList();

        Console.WriteLine("capture	toneHz	ditMs	dahMs	wpm	contrast	chars	text");

        foreach (var file in files)
        {
            var audio = WavAudio.Read(file);
            var read = CwReferenceDecoder.Run(audio.Samples, audio.SampleRate);

            Console.WriteLine(string.Format(
                CultureInfo.InvariantCulture,
                "{0}	{1:0}	{2:0}	{3:0}	{4:0.0}	{5:0}	{6}	{7}",
                Path.GetFileNameWithoutExtension(file),
                read.ToneHz,
                read.DitMilliseconds,
                read.DahMilliseconds,
                read.WordsPerMinute,
                read.ContrastDb,
                read.Characters.Count(c => c.Text != " "),
                read.Refusal ?? Clip(read.Text)));
        }
    }

    /// <summary>
    /// The shipped path and the ported reference over every capture, side by
    /// side.
    /// </summary>
    /// <remarks>
    /// Unit 045 task 2. One row per capture: the pitch each chose, the pitch the
    /// sheet measured where there is one, and what each read.
    /// </remarks>
    private static void HeadToHead()
    {
        var files = Directory
            .GetFiles(CaptureFolder(), "*.wav", SearchOption.AllDirectories)
            .OrderBy(f => Path.GetFileName(f), StringComparer.Ordinal)
            .ToList();

        Console.WriteLine(
            "capture	sheetHz	shippedHz	shippedChars	refHz	refChars	"
            + "refReads	shippedText	refText");

        foreach (var file in files)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            var audio = WavAudio.Read(file);

            var decoder = new CwDecoder(audio.SampleRate, 600);
            var shipped = new System.Text.StringBuilder();

            decoder.CharacterSettled += c => shipped.Append(c.Text);

            var hop = decoder.Tracker.HopSamples;

            for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
            {
                decoder.Process(new AudioChunk(
                    at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
            }

            decoder.Flush();

            var reference = CwReferenceDecoder.Run(audio.Samples, audio.SampleRate);
            var refLetters = reference.Characters
                .Count(c => c.Text != MorseAlphabet.WordGap);

            Console.WriteLine(string.Format(
                CultureInfo.InvariantCulture,
                "{0}	{1:0.0}	{2:0.0}	{3}	{4:0.0}	{5}	{6}	{7}	{8}",
                name,
                SidecarToneHz(file),
                decoder.Report.ToneHz,
                decoder.Report.CharactersEmitted,
                reference.ToneHz,
                refLetters,
                reference.Refusal is null ? "read" : "refused",
                Clip(shipped.ToString()),
                Clip(reference.Refusal ?? reference.Text)));
        }
    }

    /// <summary>What the reference's chain costs per second of audio.</summary>
    /// <remarks>
    /// Unit 045 task 4. Measure only; nothing changes on its account.
    /// </remarks>
    private static void ReferenceCost()
    {
        var file = Directory
            .GetFiles(CaptureFolder(), "*.wav", SearchOption.AllDirectories)
            .OrderBy(f => Path.GetFileName(f), StringComparer.Ordinal)
            .First(f => f.Contains("004844", StringComparison.Ordinal));

        var audio = WavAudio.Read(file);
        var seconds = audio.Samples.Length / (double)audio.SampleRate;

        var x = new double[audio.Samples.Length];

        for (var i = 0; i < x.Length; i++)
        {
            x[i] = audio.Samples[i];
        }

        // Warm the JIT.
        CwReferenceDecoder.Run(audio.Samples, audio.SampleRate);

        var mask = CwReferenceDecoder.MuteMask(x, audio.SampleRate);

        var acq = Stopwatch.StartNew();
        CwReferenceDecoder.AcquireTone(x, audio.SampleRate, mask);
        acq.Stop();

        var whole = Stopwatch.StartNew();
        CwReferenceDecoder.Run(audio.Samples, audio.SampleRate);
        whole.Stop();

        var shipped = Stopwatch.StartNew();
        var decoder = new CwDecoder(audio.SampleRate, 600);
        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();
        shipped.Stop();

        Console.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            "audio            {0:0.0} s at {1} Hz", seconds, audio.SampleRate));
        Console.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            "acquire_tone     {0:0} ms for the whole file  ({1:0.0}% of one core)",
            acq.Elapsed.TotalMilliseconds,
            acq.Elapsed.TotalMilliseconds / (seconds * 1000) * 100));
        Console.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            "reference chain  {0:0} ms for the whole file  ({1:0.0}% of one core)",
            whole.Elapsed.TotalMilliseconds,
            whole.Elapsed.TotalMilliseconds / (seconds * 1000) * 100));
        Console.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            "shipped path     {0:0} ms for the whole file  ({1:0.0}% of one core)",
            shipped.Elapsed.TotalMilliseconds,
            shipped.Elapsed.TotalMilliseconds / (seconds * 1000) * 100));
    }

    /// <summary>
    /// What the fit figure is doing, and what the clock withdrawal costs,
    /// across every capture.
    /// </summary>
    /// <remarks>
    /// Unit 044 tasks 2, 3 and 7 measured together, because all three are
    /// questions about the same run: the figure beside the share of the output
    /// that is one, two or three dits; how many settled characters arrive while
    /// the speed clock is withdrawn; and where an estimator lands on the edge of
    /// its own search space.
    /// </remarks>
    private static void ClockSweep()
    {
        var files = Directory
            .GetFiles(CaptureFolder(), "*.wav", SearchOption.AllDirectories)
            .OrderBy(f => Path.GetFileName(f), StringComparer.Ordinal)
            .ToList();

        Console.WriteLine(
            "capture	fit	chars	shortShare	wpm	atEdge	withdrawnChars	"
            + "withdrawnShare	text");

        foreach (var file in files)
        {
            var audio = WavAudio.Read(file);
            var decoder = new CwDecoder(audio.SampleRate, 600);

            var all = 0;
            var shortOnes = 0;
            var whileWithdrawn = 0;
            var text = new System.Text.StringBuilder();

            decoder.CharacterSettled += c =>
            {
                if (c.Text == MorseAlphabet.WordGap)
                {
                    text.Append(c.Text);

                    return;
                }

                all++;
                text.Append(c.Text);

                // E, I, S and T are one, two, three and one element: what a
                // decoder emits when it is chopping an envelope it has no clock
                // for.
                if (c.Text is "E" or "I" or "S" or "T")
                {
                    shortOnes++;
                }

                if (decoder.SpeedIsReacquiring)
                {
                    whileWithdrawn++;
                }
            };

            var hop = decoder.Tracker.HopSamples;

            for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
            {
                decoder.Process(new AudioChunk(
                    at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
            }

            decoder.Flush();

            var reading = decoder.Reading;
            var wpm = reading.WordsPerMinute;

            var atEdge = reading.Characters.Count > 0
                && (wpm <= CwProbabilisticDecoder.SlowestWpm + 1e-9
                    || wpm >= CwProbabilisticDecoder.FastestWpm - 1e-9);

            Console.WriteLine(string.Format(
                CultureInfo.InvariantCulture,
                "{0}	{1:0.00}	{2}	{3:0.00}	{4:0}	{5}	{6}	{7:0.00}	{8}",
                Path.GetFileNameWithoutExtension(file),
                reading.LikelihoodRatio,
                all,
                all == 0 ? 0 : (double)shortOnes / all,
                wpm,
                atEdge ? "EDGE" : "",
                whileWithdrawn,
                all == 0 ? 0 : (double)whileWithdrawn / all,
                Clip(text.ToString())));
        }
    }

    /// <summary>
    /// The truth this repository holds: the readings Tim has adjudicated on his
    /// own recordings.
    /// </summary>
    /// <remarks>
    /// **REAL DATA FROM THE REAL RADIO, WHICH IS THE ONLY KIND THAT SCORES**
    /// (Tim's ruling). These are his rulings on his own air, already in the tree
    /// with their decision ids, so they carry no third-party rights question — an
    /// ARRL bulletin vendored whole into a public GPL-3.0 repository would (§2.1,
    /// HM-DEC-049).
    /// </remarks>
    private static readonly (string Capture, string Truth, string Ruling)[] Truths =
    [
        ("cw-2026-08-17-013347", "VA3VRR", "HM-DEC-145"),
        ("cw-2026-08-17-134712", "N4L", "HM-DEC-144"),
        ("cw-2026-08-18-003758", "AA4MP/4 QNIK", "HM-DEC-126"),
        ("cw-2026-08-24-012403", "DE KD0UN KD0UN K", "work instruction 011"),
        ("cw-2026-08-18-004507",
            "AT ARRL DOT NET <BT> EACH STATION HANDLING THIS MESSAGE P",
            "HM-DEC-115"),
        ("cw-2026-08-22-031838",
            "2, 2, AND 2 WITH A MEAN OF 2.9. PRE", "Tim 2026-08-25"),
        ("cw-2026-08-22-031905",
            "DICTED 10.7 CENTIMETER FLUX IS 125, 125", "Tim 2026-08-25"),
        ("cw-2026-08-22-031948",
            "110, 110, AND 110 WITH A MEAN OF 117", "Tim 2026-08-25"),
        ("cw-2026-08-22-032012",
            "N OF 117. LINKS TO ARTICLES OR OTHER WEBSITES MENTI",
            "Tim 2026-08-25"),
        ("cw-2026-08-22-032050",
            "THIS BULLETIN CAN BE FOUND IN TELEPRINTER, PACKET, AND INTE",
            "Tim 2026-08-25"),
        ("cw-2026-08-22-032113",
            "ACKET, AND INTERNET VERSIONS", "Tim 2026-08-25"),
        ("cw-2026-08-22-032129",
            "2026 PROPAGATION FORECAST BULLETIN ARLP034", "Tim 2026-08-25"),
    ];

    /// <summary>The first accuracy baseline this project has produced.</summary>
    /// <remarks>
    /// Unit 045 tasks 4 and 6 together: the score per capture, and the fit figure
    /// beside it so the correlation between the two can be read off.
    /// </remarks>
    /// <param name="usePeak">
    /// Feed the decoder <see cref="CwSpectralPeak"/>'s answer instead of letting
    /// the tone tracker find its own (work instruction 050, task 3).
    /// </param>
    private static void ScoreSweep(bool usePeak = false)
    {
        Console.WriteLine(usePeak
            ? "PITCH: CwSpectralPeak, asserted"
            : "PITCH: the tone tracker, free-running");

        Console.WriteLine(
            "capture	truth	yield	precision	correct	subs	ins	dels	fit	read");

        var truthTotal = 0;
        var correctTotal = 0;
        var assertedTotal = 0;

        foreach (var (capture, truth, _) in Truths)
        {
            var path = Find(capture);

            if (path is null)
            {
                Console.WriteLine($"{capture}	MISSING");

                continue;
            }

            var audio = WavAudio.Read(path);
            var decoder = new CwDecoder(audio.SampleRate, 600);

            if (usePeak
                && CwSpectralPeak.Find(audio.Samples, audio.SampleRate) is { } peak)
            {
                decoder.AssertAt(peak);
            }

            var text = new System.Text.StringBuilder();

            decoder.CharacterSettled += c => text.Append(c.Text);

            var hop = decoder.Tracker.HopSamples;

            for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
            {
                decoder.Process(new AudioChunk(
                    at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
            }

            decoder.Flush();

            var read = text.ToString();
            var score = CwAccuracy.Score(read, truth);

            truthTotal += score.TruthCharacters;
            correctTotal += score.Correct;
            assertedTotal += score.Correct + score.Substitutions + score.Insertions;

            Console.WriteLine(string.Format(
                CultureInfo.InvariantCulture,
                "{0}	{1}	{2:0.000}	{3:0.000}	{4}	{5}	{6}	{7}	{8:0.00}	{9}",
                capture,
                truth.Length,
                score.Yield,
                score.Precision,
                score.Correct,
                score.Substitutions,
                score.Insertions,
                score.Deletions,
                decoder.Reading.LikelihoodRatio,
                Clip(read)));
        }

        Console.WriteLine();
        Console.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            "CORPUS	truth {0}	yield {1:0.000}	precision {2:0.000}",
            truthTotal,
            truthTotal == 0 ? 0 : (double)correctTotal / truthTotal,
            assertedTotal == 0 ? 0 : (double)correctTotal / assertedTotal));
    }

    /// <summary>
    /// What the integrator's width is worth, scored against the truth.
    /// </summary>
    /// <remarks>
    /// **THE WIDTH HAS BEEN SWEPT BEFORE AND NEVER AGAINST AN ANSWER KEY.** The
    /// reasoning behind 45 Hz was argued from character counts and E-shares,
    /// which are proxies; this scores the same widths against what was actually
    /// sent. Nothing in the application passes anything but the constant — the
    /// parameter exists on the decoder precisely so a constant can be swept and
    /// judged by what it reads.
    /// </remarks>
    private static void WidthSweep()
    {
        Console.WriteLine("widthHz	truth	yield	precision	correct	subs	ins	dels");

        foreach (var width in new[] { 20.0, 25.0, 30.0, 35.0, 40.0, 45.0, 55.0, 70.0 })
        {
            var truthTotal = 0;
            var correct = 0;
            var subs = 0;
            var ins = 0;
            var dels = 0;
            var asserted = 0;

            foreach (var (capture, truth, _) in Truths)
            {
                var path = Find(capture);

                if (path is null)
                {
                    continue;
                }

                var audio = WavAudio.Read(path);
                var decoder = new CwDecoder(audio.SampleRate, 600, width, null);
                var text = new System.Text.StringBuilder();

                decoder.CharacterSettled += c => text.Append(c.Text);

                var hop = decoder.Tracker.HopSamples;

                for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
                {
                    decoder.Process(new AudioChunk(
                        at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
                }

                decoder.Flush();

                var score = CwAccuracy.Score(text.ToString(), truth);

                truthTotal += score.TruthCharacters;
                correct += score.Correct;
                subs += score.Substitutions;
                ins += score.Insertions;
                dels += score.Deletions;
                asserted += score.Correct + score.Substitutions + score.Insertions;
            }

            Console.WriteLine(string.Format(
                CultureInfo.InvariantCulture,
                "{0:0}	{1}	{2:0.000}	{3:0.000}	{4}	{5}	{6}	{7}",
                width,
                truthTotal,
                truthTotal == 0 ? 0 : (double)correct / truthTotal,
                asserted == 0 ? 0 : (double)correct / asserted,
                correct, subs, ins, dels));
        }
    }

    /// <summary>
    /// Whether any number the decoder already attaches to a character tracks
    /// whether that character was right.
    /// </summary>
    /// <remarks>
    /// **THIS IS UNIT 046 TASK 2'S MEASUREMENT, TAKEN ON THE QUANTITIES THAT
    /// EXIST.** A forward-backward posterior needs the lattice indexed by
    /// (hop, kind) and it is indexed by (hop) alone, so that number cannot be had
    /// without the restructuring the task forbids. What can be had is the
    /// per-hop runner-up margin the Viterbi already keeps, tested per character
    /// rather than per recording for the first time.
    /// </remarks>
    private static void ConfidenceSweep()
    {
        var posterior = new List<(double Value, bool Right)>();
        var margin = new List<(double Value, bool Right)>();
        var truthTotal = 0;
        var share = new List<(double Value, bool Right)>();
        var span = new List<(double Value, bool Right)>();

        Console.WriteLine("capture	scored	correct	subs	ins	blocks");

        foreach (var (capture, truth, _) in Truths)
        {
            var path = Find(capture);

            if (path is null)
            {
                continue;
            }

            var audio = WavAudio.Read(path);
            var decoder = new CwDecoder(audio.SampleRate, 600);
            var chars = new List<CwCharacter>();

            decoder.CharacterSettled += c =>
            {
                if (c.Text != MorseAlphabet.WordGap)
                {
                    chars.Add(c);
                }
            };

            var hop = decoder.Tracker.HopSamples;

            for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
            {
                decoder.Process(new AudioChunk(
                    at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
            }

            decoder.Flush();

            // Spaces are dropped from both sides so the indices line up with the
            // character list; this correlation is about letters.
            var read = string.Concat(chars.Select(c => c.Text)).ToUpperInvariant();
            var want = new string(
                truth.ToUpperInvariant().Where(c => !char.IsWhiteSpace(c)).ToArray());

            var outcomes = CwAccuracy.Align(read, want);

            truthTotal += want.Length;

            int correct = 0, subs = 0, ins = 0, blocks = 0;

            foreach (var (index, outcome) in outcomes)
            {
                if (index >= chars.Count)
                {
                    continue;
                }

                var c = chars[index];

                switch (outcome)
                {
                    case CwAccuracy.Outcome.Correct: correct++; break;
                    case CwAccuracy.Outcome.Substitution: subs++; break;
                    case CwAccuracy.Outcome.Insertion: ins++; break;
                    default: blocks++; continue;
                }

                var right = outcome == CwAccuracy.Outcome.Correct;

                if (!double.IsNaN(c.Posterior))
                {
                    posterior.Add((c.Posterior, right));
                }

                if (!double.IsNaN(c.MarginLlr))
                {
                    margin.Add((c.MarginLlr, right));
                }

                if (!double.IsNaN(c.MarginShareForRecord))
                {
                    share.Add((c.MarginShareForRecord, right));
                }

                span.Add((c.SpanMarginForRecord, right));
            }

            Console.WriteLine(string.Format(
                CultureInfo.InvariantCulture,
                "{0}	{1}	{2}	{3}	{4}	{5}",
                capture, outcomes.Count, correct, subs, ins, blocks));
        }

        Console.WriteLine();
        Sweep(posterior, truthTotal);
        Console.WriteLine();
        Report("Posterior", posterior);
        Report("MarginLlr", margin);
        Report("MarginShare", share);
        Report("SpanMargin", span);
    }

    /// <summary>
    /// How the posterior's discrimination varies with the temperature.
    /// </summary>
    /// <remarks>
    /// Unit 049 task 3. The temperature multiplies the whole path score, so the
    /// Viterbi argmax cannot move and the decode is untouched; only the
    /// normalisation changes. **The alpha the over-count implies is 0.45**, and
    /// the sweep runs decades either side of it because the over-count is not
    /// what makes the model overconfident.
    /// </remarks>
    private static void TemperatureSweep()
    {
        // Every character, with the truth outcome, decoded once and re-scored
        // at each alpha — so the decode is provably identical across the sweep.
        var work = new List<(double[] DownTo, double[] UpTo, double Unit,
            int Count, List<(int Hop, int Kind, bool Right)> Marks)>();

        Console.WriteLine(
            "alpha	n	medianRight	medianWrong	separation	correlation	spread");

        foreach (var alpha in new[]
        {
            0.1, 0.01, 0.001, 0.0001,
        })
        {
            var points = new List<(double Value, bool Right)>();

            foreach (var (capture, truth, _) in Truths)
            {
                var path = Find(capture);

                if (path is null)
                {
                    continue;
                }

                var audio = WavAudio.Read(path);
                var decoder = new CwDecoder(audio.SampleRate, 600)
                {
                    PosteriorTemperature = alpha,
                };

                var chars = new List<CwCharacter>();

                decoder.CharacterSettled += c =>
                {
                    if (c.Text != MorseAlphabet.WordGap)
                    {
                        chars.Add(c);
                    }
                };

                var hop = decoder.Tracker.HopSamples;

                for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
                {
                    decoder.Process(new AudioChunk(
                        at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
                }

                decoder.Flush();

                var read = string.Concat(chars.Select(c => c.Text)).ToUpperInvariant();
                var want = new string(
                    truth.ToUpperInvariant()
                        .Where(c => !char.IsWhiteSpace(c)).ToArray());

                foreach (var (index, outcome) in CwAccuracy.Align(read, want))
                {
                    if (index >= chars.Count
                        || outcome == CwAccuracy.Outcome.Block
                        || double.IsNaN(chars[index].Posterior))
                    {
                        continue;
                    }

                    points.Add((
                        chars[index].Posterior,
                        outcome == CwAccuracy.Outcome.Correct));
                }
            }

            if (points.Count < 3)
            {
                Console.WriteLine(string.Format(
                    CultureInfo.InvariantCulture, "{0:0.####}	none", alpha));

                continue;
            }

            var right = points.Where(p => p.Right).Select(p => p.Value)
                .OrderBy(v => v).ToArray();
            var wrong = points.Where(p => !p.Right).Select(p => p.Value)
                .OrderBy(v => v).ToArray();
            var all = points.Select(p => p.Value).OrderBy(v => v).ToArray();

            var mr = right.Length == 0 ? 0 : right[right.Length / 2];
            var mw = wrong.Length == 0 ? 0 : wrong[wrong.Length / 2];

            var xs = points.Select(p => p.Value).ToArray();
            var ys = points.Select(p => p.Right ? 1.0 : 0.0).ToArray();
            var mx = xs.Average();
            var my = ys.Average();
            var cov = xs.Zip(ys, (a, b) => (a - mx) * (b - my)).Sum() / xs.Length;
            var sx = Math.Sqrt(xs.Select(a => (a - mx) * (a - mx)).Sum() / xs.Length);
            var sy = Math.Sqrt(ys.Select(b => (b - my) * (b - my)).Sum() / ys.Length);

            // The spread the order asks for: the gap between the tenth and the
            // ninetieth percentile, which says whether the distribution moved off
            // the ceiling at all.
            var spread = all[(int)(all.Length * 0.9)] - all[all.Length / 10];

            Console.WriteLine(string.Format(
                CultureInfo.InvariantCulture,
                "{0:0.####}	{1}	{2:0.0000}	{3:0.0000}	{4:+0.0000;-0.0000}	{5:+0.000;-0.000}	{6:0.0000}",
                alpha, points.Count, mr, mw, mr - mw,
                sx * sy < 1e-12 ? 0 : cov / (sx * sy), spread));
        }

        _ = work;
    }

    /// <summary>
    /// The evidence term and the duration penalty, side by side on real audio.
    /// </summary>
    /// <remarks>
    /// Unit 049 task 1. If the evidence dominates by the ratio the
    /// overconfidence implies, the duration prior is doing almost nothing and
    /// the decoder is fitting the envelope while ignoring how implausible the
    /// resulting element lengths are.
    /// </remarks>
    private static void Magnitudes()
    {
        Console.WriteLine(
            "capture	chars	evidencePerHop	evidencePerElement	durationPenalty	ratio");

        // The penalty a span twenty per cent off its expected length pays:
        // half the squared log-ratio over the tolerance share.
        var off20 = 0.5 * Math.Pow(Math.Log(1.2) / 0.35, 2);
        var off50 = 0.5 * Math.Pow(Math.Log(1.5) / 0.35, 2);

        foreach (var (capture, _, _) in Truths)
        {
            var path = Find(capture);

            if (path is null)
            {
                continue;
            }

            var audio = WavAudio.Read(path);
            var decoder = new CwDecoder(audio.SampleRate, 600);
            var perHop = new List<double>();
            var perElement = new List<double>();

            decoder.CharacterSettled += c =>
            {
                if (c.Text == MorseAlphabet.WordGap || c.SpanHops <= 0
                    || double.IsNaN(c.SpanLogLikelihoodRatio))
                {
                    return;
                }

                perHop.Add(Math.Abs(c.SpanLogLikelihoodRatio) / c.SpanHops);
                perElement.Add(
                    Math.Abs(c.SpanLogLikelihoodRatio)
                    / Math.Max(1, c.Pattern.Length));
            };

            var hop = decoder.Tracker.HopSamples;

            for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
            {
                decoder.Process(new AudioChunk(
                    at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
            }

            decoder.Flush();

            if (perHop.Count == 0)
            {
                continue;
            }

            perHop.Sort();
            perElement.Sort();

            var medHop = perHop[perHop.Count / 2];
            var medElement = perElement[perElement.Count / 2];

            Console.WriteLine(string.Format(
                CultureInfo.InvariantCulture,
                "{0}	{1}	{2:0.00}	{3:0.0}	{4:0.000}	{5:0}",
                capture, perHop.Count, medHop, medElement, off20,
                medElement / off20));
        }

        Console.WriteLine();
        Console.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            "duration penalty: a span 20% off its want costs {0:0.000} nats, "
            + "50% off costs {1:0.000}",
            off20, off50));
    }

    /// <summary>
    /// What blocking every character below a threshold would buy and cost.
    /// </summary>
    /// <remarks>
    /// **THE CURVE IS REPORTED RATHER THAN A POINT BEING PICKED.** Choosing a
    /// threshold by trying values until the corpus reads better is the failure
    /// unit 045 avoided on the filter width and the standard here.
    /// </remarks>
    private static void Sweep(
        List<(double Value, bool Right)> data, int truthTotal)
    {
        Console.WriteLine("threshold	kept	blocked	correct	yield	precision");

        foreach (var t in new[]
        {
            0.0, 0.50, 0.70, 0.80, 0.85, 0.90, 0.95, 0.98, 0.99, 0.999,
        })
        {
            var kept = data.Where(d => d.Value >= t).ToArray();
            var correct = kept.Count(d => d.Right);

            Console.WriteLine(string.Format(
                CultureInfo.InvariantCulture,
                "{0:0.000}	{1}	{2}	{3}	{4:0.000}	{5:0.000}",
                t,
                kept.Length,
                data.Count - kept.Length,
                correct,
                truthTotal == 0 ? 0 : (double)correct / truthTotal,
                kept.Length == 0 ? 0 : (double)correct / kept.Length));
        }
    }

    /// <summary>Correlate one candidate confidence against correctness.</summary>
    private static void Report(string name, List<(double Value, bool Right)> data)
    {
        if (data.Count < 3)
        {
            Console.WriteLine($"{name}: too few points ({data.Count})");

            return;
        }

        var xs = data.Select(d => d.Value).ToArray();
        var ys = data.Select(d => d.Right ? 1.0 : 0.0).ToArray();
        var mx = xs.Average();
        var my = ys.Average();
        var cov = xs.Zip(ys, (a, b) => (a - mx) * (b - my)).Sum() / xs.Length;
        var sx = Math.Sqrt(xs.Select(a => (a - mx) * (a - mx)).Sum() / xs.Length);
        var sy = Math.Sqrt(ys.Select(b => (b - my) * (b - my)).Sum() / ys.Length);

        var right = data.Where(d => d.Right).Select(d => d.Value).OrderBy(v => v).ToArray();
        var wrong = data.Where(d => !d.Right).Select(d => d.Value).OrderBy(v => v).ToArray();

        Console.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            "{0}: n={1}  correlation {2:+0.000;-0.000}  median right {3:0.0000}  "
            + "median wrong {4:0.0000}",
            name,
            data.Count,
            sx * sy < 1e-12 ? 0 : cov / (sx * sy),
            right.Length == 0 ? 0 : right[right.Length / 2],
            wrong.Length == 0 ? 0 : wrong[wrong.Length / 2]));
    }

    /// <summary>Where a capture lives, adjudicated or not.</summary>
    private static string? Find(string capture)
    {
        var folder = CaptureFolder();
        var direct = Path.Combine(folder, capture + ".wav");

        if (File.Exists(direct))
        {
            return direct;
        }

        var under = Path.Combine(folder, "unadjudicated", capture + ".wav");

        return File.Exists(under) ? under : null;
    }

    /// <summary>The last stretch of a recording.</summary>
    private static MonoAudio Tail(MonoAudio audio, double seconds)
    {
        var want = (int)(seconds * audio.SampleRate);
        var from = Math.Max(0, audio.Samples.Length - want);

        return new MonoAudio(audio.SampleRate, audio.Samples[from..]);
    }

    /// <summary>
    /// Run the shipped decoder over audio, at a pitch or letting it choose.
    /// </summary>
    /// <param name="audio">The audio.</param>
    /// <param name="toneHz">The pitch to hold, or NaN to let the tracker steer.</param>
    /// <returns>What it settled on.</returns>
    private static (double ToneHz, double Score, string Text) Run(
        MonoAudio audio, double toneHz)
    {
        var decoder = new CwDecoder(audio.SampleRate, 600);

        if (!double.IsNaN(toneHz))
        {
            decoder.AssertAt(toneHz);
        }

        using var source = new BufferedAudioSource(audio);

        decoder.Listen(source);
        source.PumpAll();
        decoder.Flush();

        var reading = decoder.Reading;

        return (reading.ToneHz, reading.LikelihoodRatio, reading.Text ?? "");
    }

    /// <summary>One line of text, short enough for a table.</summary>
    private static string Clip(string text)
    {
        var flat = text.Replace('\n', ' ').Replace('\r', ' ').Trim();

        return flat;
    }

    /// <summary>How many carriers share the passband with the admitted pitch.</summary>
    /// <remarks>
    /// <para>**THE STANDING REASON THE CORPUS AND THE OPERATOR'S BAND DISAGREE**
    /// (work instruction 054, task 5). A real evening on forty metres puts more
    /// than one station inside a five-hundred-hertz passband. This was written to
    /// count how often these recordings do.</para>
    /// <para>**IT DOES NOT ANSWER THAT QUESTION AND THE OUTPUT MUST NOT BE READ AS
    /// IF IT DID.** It returns four neighbours on all twelve captures, at 30 to
    /// 100 Hz from the admitted pitch and 9 to 24 dB below it — which is what the
    /// *first* station's own spectral structure looks like. The floor reference is
    /// the median of the whole spectrum, and on a band holding one strong station
    /// that median is noise, so every ripple and skirt clears it.</para>
    /// <para>**A SECOND STATION IS ONE THAT IS KEYED INDEPENDENTLY**, and nothing
    /// here tests that. Separating the two needs the neighbour's envelope compared
    /// against the first's, which is a real measurement and not this one. Until
    /// somebody builds it, **unit 053's finding stands unchanged: every capture in
    /// this corpus has one dominant station**, established there by summing two
    /// captures deliberately to get a second.</para>
    /// </remarks>
    private static void Carriers()
    {
        Console.WriteLine("capture	pitchHz	others	levels below the first, dB");

        var withSecond = 0;
        var total = 0;

        foreach (var (capture, _, _) in Truths)
        {
            var path = Find(capture);

            if (path is null)
            {
                continue;
            }

            total++;

            var audio = WavAudio.Read(path);
            var pitch = CwSpectralPeak.Find(audio.Samples, audio.SampleRate) ?? 600;
            var found = Peaks(audio, pitch);

            if (found.Count > 0)
            {
                withSecond++;
            }

            Console.WriteLine(
                "{0}	{1:0.0}	{2}	{3}",
                capture, pitch, found.Count,
                found.Count == 0
                    ? "-"
                    : string.Join(", ", found.Select(
                        f => $"{f.Hz:0} Hz at -{f.Down:0.0}")));
        }

        Console.WriteLine();
        Console.WriteLine(
            "{0} of {1} captures have a second carrier within +/-120 Hz standing "
            + "more than 15 dB over the floor",
            withSecond, total);
    }

    /// <summary>Carriers near the pitch that stand well over the band floor.</summary>
    private static List<(double Hz, double Down)> Peaks(MonoAudio audio, double pitch)
    {
        var spectrum = Spectrum(audio);
        var binHz = (double)audio.SampleRate / CwSpectralPeak.Window;

        var floor = spectrum.Where(v => v > 0).OrderBy(v => v)
            .ElementAt(spectrum.Count(v => v > 0) / 2);

        var atPitch = spectrum[(int)Math.Round(pitch / binHz)];
        var found = new List<(double, double)>();

        var low = (int)((pitch - 120) / binHz);
        var high = (int)((pitch + 120) / binHz);

        for (var i = Math.Max(1, low); i <= Math.Min(spectrum.Length - 2, high); i++)
        {
            // A local maximum, well over the floor, and far enough from the
            // admitted pitch to be a different station rather than its skirt.
            if (spectrum[i] <= spectrum[i - 1] || spectrum[i] < spectrum[i + 1])
            {
                continue;
            }

            var hz = i * binHz;

            if (Math.Abs(hz - pitch) < 25)
            {
                continue;
            }

            var overFloor = 20 * Math.Log10(spectrum[i] / floor);

            if (overFloor > 15)
            {
                found.Add((hz, 20 * Math.Log10(atPitch / spectrum[i])));
            }
        }

        return found
            .OrderBy(f => f.Item2)
            .Take(4)
            .Select(f => (f.Item1, f.Item2))
            .ToList();
    }

    /// <summary>The averaged magnitude spectrum, as the peak finder builds it.</summary>
    private static double[] Spectrum(MonoAudio audio)
    {
        var bins = (CwSpectralPeak.Window / 2) + 1;
        var total = new double[bins];
        var windows = 0;

        for (var start = 0;
             start + CwSpectralPeak.Window <= audio.Samples.Length;
             start += CwSpectralPeak.Window / 2)
        {
            var slice = audio.Samples
                .AsSpan(start, CwSpectralPeak.Window).ToArray();

            var peak = CwSpectralPeak.Find(slice, audio.SampleRate);

            if (peak is null)
            {
                continue;
            }

            windows++;
        }

        // Re-use the peak finder's own averaging by asking it for the spectrum
        // through a narrow sweep, which keeps one source of truth for the
        // transform (section 0).
        return CwSpectralPeak.AverageSpectrum(audio.Samples, audio.SampleRate);
    }

    /// <summary>Dit scatter across hold-over lengths, per capture.</summary>
    /// <remarks>
    /// **DIT CV IS THE MEASURE FOR THIS ONE** (work instruction 054, task 3): a
    /// dit whose length varies by half its own duration is a dit that has been cut
    /// in two, and that is the fault the hold-over exists to repair.
    /// </remarks>
    private static void HoldOverSweep()
    {
        var holds = new[] { 0, 8, 12, 16, 24, 32.0 };

        Console.WriteLine(
            "safe bound: {0:0} ms, a dit at {1:0} words a minute",
            CwUnitEstimator.LongestSafeHoldOverMs,
            CwProbabilisticDecoder.FastestWpm);

        Console.Write("capture");

        foreach (var hold in holds)
        {
            Console.Write("	" + hold.ToString("0", CultureInfo.InvariantCulture));
        }

        Console.WriteLine();

        foreach (var (capture, _, _) in Truths)
        {
            var path = Find(capture);

            if (path is null)
            {
                continue;
            }

            var audio = WavAudio.Read(path);
            var toneHz = CwSpectralPeak.Find(audio.Samples, audio.SampleRate) ?? 600;
            var envelope = CwProbabilisticDecoder.Envelope(
                audio.Samples, audio.SampleRate, toneHz);

            Console.Write(capture);

            foreach (var hold in holds)
            {
                CwUnitEstimator.HoldOverMilliseconds = hold;

                var (marks, _) = CwUnitEstimator.Elements(
                    envelope, CwProbabilisticDecoder.HopMilliseconds);

                if (marks.Count < 8)
                {
                    Console.Write("	-");

                    continue;
                }

                var sorted = marks.OrderBy(v => v).ToArray();
                var cut = (At(sorted, 25) + At(sorted, 75)) / 2;
                var shortOnes = marks.Where(m => m <= cut).ToArray();

                Console.Write(shortOnes.Length >= 4
                    ? "	" + Cv(shortOnes).ToString("0.000", CultureInfo.InvariantCulture)
                    : "	-");
            }

            Console.WriteLine();
        }

        CwUnitEstimator.HoldOverMilliseconds = 0;
    }

    /// <summary>Element scatter across threshold setbacks, per capture.</summary>
    /// <remarks>
    /// **NO DECODING, SO IT IS CHEAP.** The full sweep with a corpus score at each
    /// point does not finish: the peak-referenced cut puts almost nothing
    /// key-down, the speed estimate withdraws, and the decoder spends its time
    /// re-acquiring. This measures the thing the order names to watch — dah CV —
    /// straight off the envelope.
    /// </remarks>
    private static void ScatterSweep()
    {
        var setbacks = new[] { double.NaN, 3, 4, 5, 6, 8, 10, 12.0 };

        Console.Write("capture");

        foreach (var setback in setbacks)
        {
            Console.Write(double.IsNaN(setback)
                ? "	otsu"
                : "	-" + setback.ToString("0", CultureInfo.InvariantCulture));
        }

        Console.WriteLine();

        foreach (var (capture, _, _) in Truths)
        {
            var path = Find(capture);

            if (path is null)
            {
                continue;
            }

            var audio = WavAudio.Read(path);
            var toneHz = CwSpectralPeak.Find(audio.Samples, audio.SampleRate) ?? 600;
            var envelope = CwProbabilisticDecoder.Envelope(
                audio.Samples, audio.SampleRate, toneHz);

            Console.Write(capture);

            foreach (var setback in setbacks)
            {
                CwUnitEstimator.PeakSetbackDb = setback;

                var (marks, _) = CwUnitEstimator.Elements(
                    envelope, CwProbabilisticDecoder.HopMilliseconds);

                if (marks.Count < 8)
                {
                    Console.Write("	-");

                    continue;
                }

                var sorted = marks.OrderBy(v => v).ToArray();
                var cut = (At(sorted, 25) + At(sorted, 75)) / 2;
                var longOnes = marks.Where(m => m > cut).ToArray();

                Console.Write(longOnes.Length >= 4
                    ? "	" + Cv(longOnes).ToString("0.000", CultureInfo.InvariantCulture)
                    : "	-");
            }

            Console.WriteLine();
        }

        CwUnitEstimator.PeakSetbackDb = double.NaN;
    }

    /// <summary>Corpus score and element scatter across threshold setbacks.</summary>
    /// <remarks>
    /// **DAH CV IS THE MEASURE TO WATCH** (work instruction 054, task 2): it is
    /// what moved from 0.267 to 0.104 on the operator's audio, and every capture
    /// in this corpus that reads sits between 0.028 and 0.134 on it.
    /// </remarks>
    private static void SetbackSweep(string[] only)
    {
        var values = only.Length > 0
            ? only.Select(v => v == "otsu"
                ? double.NaN
                : double.Parse(v, CultureInfo.InvariantCulture)).ToArray()
            : new[] { double.NaN, 3, 4, 5, 6, 8, 10, 12.0 };

        Console.WriteLine(
            "setback	yield	precision	subs	ditCV	dahCV	worstDahCV");

        foreach (var setback in values)
        {
            CwUnitEstimator.PeakSetbackDb = setback;

            var label = double.IsNaN(setback)
                ? "otsu"
                : setback.ToString("0", CultureInfo.InvariantCulture);

            var (yieldValue, precision, subs) = ScoreTotals();
            var (dit, dah, worstDah) = Scatter();

            Console.WriteLine(
                "{0}	{1:0.000}	{2:0.000}	{3}	{4:0.000}	{5:0.000}	{6:0.000}",
                label, yieldValue, precision, subs, dit, dah, worstDah);
        }

        CwUnitEstimator.PeakSetbackDb = double.NaN;
    }

    /// <summary>The corpus score as three numbers.</summary>
    private static (double Yield, double Precision, int Subs) ScoreTotals()
    {
        var truthTotal = 0;
        var correct = 0;
        var asserted = 0;
        var subs = 0;

        foreach (var (capture, truth, _) in Truths)
        {
            var path = Find(capture);

            if (path is null)
            {
                continue;
            }

            var audio = WavAudio.Read(path);
            var decoder = new CwDecoder(audio.SampleRate, 600);
            var text = new System.Text.StringBuilder();

            decoder.CharacterSettled += c => text.Append(c.Text);

            var hop = decoder.Tracker.HopSamples;

            for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
            {
                decoder.Process(new AudioChunk(
                    at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
            }

            decoder.Flush();

            var score = CwAccuracy.Score(text.ToString(), truth);

            truthTotal += score.TruthCharacters;
            correct += score.Correct;
            // **THE SAME DENOMINATOR THE CORPUS SCORE USES.** ScoredCharacters
            // counts blocks, which are refusals rather than assertions, so using
            // it here reported a precision that could not be compared with the
            // published figure at all (work instruction 054, task 2).
            asserted += score.Correct + score.Substitutions + score.Insertions;
            subs += score.Substitutions;
        }

        return (
            truthTotal == 0 ? 0 : (double)correct / truthTotal,
            asserted == 0 ? 0 : (double)correct / asserted,
            subs);
    }

    /// <summary>Median dit and dah coefficient of variation across the corpus.</summary>
    /// <remarks>
    /// Marks are split at the midpoint of their own two clusters, which is the
    /// same cut the estimator uses, so the two populations here are the ones the
    /// decoder is working from.
    /// </remarks>
    private static (double Dit, double Dah, double WorstDah) Scatter()
    {
        var dits = new List<double>();
        var dahs = new List<double>();

        foreach (var (capture, _, _) in Truths)
        {
            var path = Find(capture);

            if (path is null)
            {
                continue;
            }

            var audio = WavAudio.Read(path);
            var toneHz = CwSpectralPeak.Find(audio.Samples, audio.SampleRate) ?? 600;

            var envelope = CwProbabilisticDecoder.Envelope(
                audio.Samples, audio.SampleRate, toneHz);

            var (marks, _) = CwUnitEstimator.Elements(
                envelope, CwProbabilisticDecoder.HopMilliseconds);

            if (marks.Count < 8)
            {
                continue;
            }

            var sorted = marks.OrderBy(v => v).ToArray();
            var cut = (At(sorted, 25) + At(sorted, 75)) / 2;

            var shortOnes = marks.Where(m => m <= cut).ToArray();
            var longOnes = marks.Where(m => m > cut).ToArray();

            if (shortOnes.Length >= 4)
            {
                dits.Add(Cv(shortOnes));
            }

            if (longOnes.Length >= 4)
            {
                dahs.Add(Cv(longOnes));
            }
        }

        return (
            dits.Count == 0 ? 0 : dits.OrderBy(v => v).ElementAt(dits.Count / 2),
            dahs.Count == 0 ? 0 : dahs.OrderBy(v => v).ElementAt(dahs.Count / 2),
            dahs.Count == 0 ? 0 : dahs.Max());
    }

    /// <summary>Coefficient of variation.</summary>
    private static double Cv(double[] values)
    {
        var mean = values.Average();

        if (mean <= 0)
        {
            return 0;
        }

        var variance = values.Sum(v => (v - mean) * (v - mean)) / values.Length;

        return Math.Sqrt(variance) / mean;
    }

    /// <summary>How much the envelope ripples inside a key-down stretch.</summary>
    /// <remarks>
    /// <para>**FADING FAST ENOUGH TO PUNCH HOLES IN SINGLE ELEMENTS** (work
    /// instruction 053, task 5). Independent analysis of the 2026-08-29 evening
    /// found 49 to 61 per cent peak-to-peak ripple during key-down with dominant
    /// modulation at 7, 37 and 53 Hz — which is why the same recording measures
    /// 21 words a minute at one threshold and 37 at another, and why the ear reads
    /// it and an envelope detector does not.</para>
    /// <para>**MEASURE ONLY.** This is the next unit's evidence, taken now while
    /// the numbers are cheap.</para>
    /// </remarks>
    private static void Fading()
    {
        Console.WriteLine(
            "capture	stretches	rippleP50%	rippleP90%	dominantHz	namedChars");

        var rows = new List<(double Ripple, string Line)>();

        foreach (var (capture, _, _) in Truths)
        {
            var path = Find(capture);

            if (path is null)
            {
                continue;
            }

            var audio = WavAudio.Read(path);
            var toneHz = CwSpectralPeak.Find(audio.Samples, audio.SampleRate) ?? 600;

            var envelope = CwProbabilisticDecoder.Envelope(
                audio.Samples, audio.SampleRate, toneHz);

            var db = envelope.Select(v => 20 * Math.Log10(Math.Max(v, 1e-12)))
                .ToArray();

            var threshold = CwUnitEstimator.Otsu(db);
            var hopMs = CwProbabilisticDecoder.HopMilliseconds;
            var minimumHops = (int)(120 / hopMs);

            var ripples = new List<double>();
            var rates = new List<double>();
            var from = -1;

            for (var i = 0; i <= db.Length; i++)
            {
                var down = i < db.Length && db[i] > threshold;

                if (down && from < 0)
                {
                    from = i;
                }
                else if (!down && from >= 0)
                {
                    if (i - from >= minimumHops)
                    {
                        var run = envelope.Skip(from).Take(i - from).ToArray();
                        var high = run.Max();
                        var low = run.Min();

                        if (high > 0)
                        {
                            ripples.Add((high - low) / high * 100);
                        }

                        rates.Add(DominantHz(run, 1000 / hopMs));
                    }

                    from = -1;
                }
            }

            if (ripples.Count == 0)
            {
                Console.WriteLine($"{capture}	0	-	-	-	-");

                continue;
            }

            var sorted = ripples.OrderBy(v => v).ToArray();
            var median = At(sorted, 50);

            rows.Add((median, string.Format(
                CultureInfo.InvariantCulture,
                "{0}	{1}	{2:0}	{3:0}	{4:0.0}	{5}",
                capture, ripples.Count, median, At(sorted, 90),
                rates.Count == 0 ? 0 : rates.OrderBy(v => v).ElementAt(rates.Count / 2),
                NamedCharacters(audio))));
        }

        foreach (var row in rows.OrderByDescending(r => r.Ripple))
        {
            Console.WriteLine(row.Line);
        }
    }

    /// <summary>The strongest modulation rate inside one key-down run.</summary>
    /// <remarks>
    /// A plain periodogram over the run's own envelope, searched from 3 Hz to a
    /// quarter of the hop rate. Below 3 Hz is the element itself rather than
    /// modulation of it.
    /// </remarks>
    private static double DominantHz(double[] run, double hopsPerSecond)
    {
        var mean = run.Average();
        var best = 0.0;
        var bestHz = 0.0;

        // **AT LEAST ONE FULL CYCLE INSIDE THE RUN**, or the answer is the
        // element's own rise and fall rather than modulation of it. The first
        // version of this searched from 3 Hz and returned 4.5 to 6 Hz on every
        // capture, which over a 120 ms run is less than one cycle — it was
        // measuring the shape of the dah, not the fading.
        var lowest = 1000.0 / (run.Length * (1000.0 / hopsPerSecond));

        for (var hz = Math.Max(lowest, 8.0); hz <= hopsPerSecond / 4; hz += 0.5)
        {
            var re = 0.0;
            var im = 0.0;

            for (var i = 0; i < run.Length; i++)
            {
                var turn = 2 * Math.PI * hz * i / hopsPerSecond;

                re += (run[i] - mean) * Math.Cos(turn);
                im += (run[i] - mean) * Math.Sin(turn);
            }

            var power = (re * re) + (im * im);

            if (power > best)
            {
                best = power;
                bestHz = hz;
            }
        }

        return bestHz;
    }

    /// <summary>How many characters the decoder names on this audio.</summary>
    private static int NamedCharacters(MonoAudio audio)
    {
        var decoder = new CwDecoder(audio.SampleRate, 600);
        var named = 0;

        decoder.CharacterSettled += c =>
        {
            if (!c.IsWordGap && c.Text != "■")
            {
                named++;
            }
        };

        using var source = new BufferedAudioSource(audio);

        decoder.Listen(source);
        source.PumpAll();
        decoder.Flush();

        return named;
    }

    /// <summary>The peak over the whole file against the loudest stretch.</summary>
    /// <remarks>
    /// **TASK 4'S ACCEPTANCE, MEASURED ON THE REAL CORPUS** (work instruction
    /// 052). The synthetic sweep says the two agree to two hundredths of a hertz;
    /// this asks whether the real recordings say the same.
    /// </remarks>
    private static void PeakWindow()
    {
        Console.WriteLine("capture	wholeHz	loudest8Hz	loudest4Hz	spread");

        foreach (var (capture, _, _) in Truths)
        {
            var path = Find(capture);

            if (path is null)
            {
                continue;
            }

            var audio = WavAudio.Read(path);

            var whole = CwSpectralPeak.Find(audio.Samples, audio.SampleRate);
            var eight = CwSpectralPeak.FindOverLoudestStretch(
                audio.Samples, audio.SampleRate, 8.0);
            var four = CwSpectralPeak.FindOverLoudestStretch(
                audio.Samples, audio.SampleRate, 4.0);

            var all = new[] { whole, eight, four }
                .Where(v => v is not null).Select(v => v!.Value).ToArray();

            Console.WriteLine(
                "{0}	{1:0.00}	{2:0.00}	{3:0.00}	{4:0.00}",
                capture, whole, eight, four,
                all.Length == 0 ? 0 : all.Max() - all.Min());
        }
    }

    /// <summary>
    /// How much of each capture holds a station, and how far the whole-file duty
    /// is from the duty where the station actually is.
    /// </summary>
    /// <remarks>
    /// <para>**THE GATING MEASUREMENT** (work instruction 052, task 1). A window
    /// change can only be demonstrated on a capture where the whole file and the
    /// busy stretch disagree. Where a station is present for 95 per cent of a
    /// recording the two numbers are the same and the capture proves nothing
    /// either way.</para>
    /// <para>**PRESENCE IS MEASURED BY A RULE THAT SHARES NO CODE WITH
    /// ADMISSION.** Not `CwToneSurvey`, which is admission; not
    /// `CwUnitEstimator.Otsu`, which is the threshold admission applies. The
    /// recording is cut into one-second blocks, each block reduced to what it
    /// reaches at its own ninetieth percentile, and a block counts as present
    /// when that is six decibels above the quietest tenth of all blocks.</para>
    /// <para>**TWO EARLIER INSTRUMENTS WERE DEGENERATE AND ARE RECORDED RATHER
    /// THAN QUIETLY REPLACED**, because both failures say something about the
    /// corpus. Six decibels above the whole envelope's median marked six of
    /// twelve captures as nought per cent present: on a continuously-keyed
    /// bulletin the median sits inside the signal. Six decibels above the
    /// quietest tenth of one-second blocks marked eight of twelve at nought, for
    /// a sharper reason — **a station present throughout leaves no quiet
    /// reference inside its own recording**, so every relative rule reports it
    /// as absent.</para>
    /// <para>**SO THE TEST IS ABSOLUTE.** A second of Morse swings fifteen to
    /// twenty-five decibels between key-down and key-up; a second of noise sits
    /// still. Ten decibels of swing inside one second is somebody keying, and it
    /// needs nothing else in the file to compare against.</para>
    /// </remarks>
    private static void Presence()
    {
        // **EIGHTEEN DECIBELS, TAKEN FROM THE CORPUS RATHER THAN GUESSED.**
        // Printed to standard error below, per capture: the quietest blocks in
        // every recording swing 11 to 15 dB and the keyed ones swing 20 to 30,
        // with the 08-22 bulletins sitting at a tenth percentile of 19 to 24
        // because they are keyed throughout. Eighteen is the gap between the two
        // populations. A first guess of 10 marked every capture present and a
        // relative rule before that marked eight of them absent; both are
        // recorded in the remarks because each failure says something about the
        // corpus.
        const double KeyedSwingDb = 18.0;

        Console.WriteLine(
            "capture	seconds	present%	longestS	dutyWhole%	dutyWindow%	"
            + "gap	p20dB	medianDb");

        var rows = new List<(string Name, double Gap, string Line)>();

        foreach (var (capture, _, _) in Truths)
        {
            var path = Find(capture);

            if (path is null)
            {
                continue;
            }

            var audio = WavAudio.Read(path);
            var seconds = audio.Samples.Length / (double)audio.SampleRate;
            var toneHz = CwSpectralPeak.Find(audio.Samples, audio.SampleRate) ?? 600;

            var envelope = CwProbabilisticDecoder.Envelope(
                audio.Samples, audio.SampleRate, toneHz);

            var db = envelope.Select(v => 20 * Math.Log10(Math.Max(v, 1e-12)))
                .ToArray();

            var sorted = (double[])db.Clone();
            Array.Sort(sorted);

            var p20 = At(sorted, 20);
            var median = At(sorted, 50);

            // **PRESENCE IS MEASURED IN ONE-SECOND BLOCKS, NOT SAMPLE BY SAMPLE.**
            // A single threshold over the whole envelope is degenerate at both
            // ends: on a mostly-silent recording the median is noise and works,
            // and on a continuously-keyed one the median sits inside the signal
            // and marks nothing present at all. Six of twelve captures read 0.0
            // per cent that way, which is a fault in the instrument rather than a
            // fact about the audio.
            //
            // A block is the right scale anyway. The question is "was somebody
            // sending during this second", and Morse is silent half of any second
            // it is sent in.
            var hopsPerBlock = (int)(1000 / CwProbabilisticDecoder.HopMilliseconds);
            var swings = new List<double>();

            for (var at = 0; at + hopsPerBlock <= db.Length; at += hopsPerBlock)
            {
                var block = db.Skip(at).Take(hopsPerBlock).OrderBy(v => v).ToArray();

                // **THE SWING INSIDE THE SECOND, WHICH IS AN ABSOLUTE TEST.**
                // Morse spends a second going up and down: key-down against
                // key-up is fifteen to twenty-five decibels. Noise sits still.
                // Nothing here is relative to the rest of the recording, which is
                // what the two rules before this one got wrong — a station
                // present throughout leaves no quiet reference to be relative to,
                // and both earlier instruments then reported it as absent.
                swings.Add(At(block, 90) - At(block, 10));
            }

            if (swings.Count == 0)
            {
                continue;
            }

            var swingSorted = swings.OrderBy(v => v).ToArray();

            Console.Error.WriteLine(
                "{0}  swing p10 {1:0.0}  p50 {2:0.0}  p90 {3:0.0}  min {4:0.0}  max {5:0.0}",
                capture, At(swingSorted, 10), At(swingSorted, 50),
                At(swingSorted, 90), swingSorted[0], swingSorted[^1]);

            var present = swings.Select(v => v > KeyedSwingDb).ToArray();            var presentShare = present.Count(v => v) / (double)present.Length;

            // The longest contiguous run of presence, allowing the gaps a sender
            // leaves between characters: a run is broken only by a second of
            // continuous absence, since Morse is absent half the time by nature.
            // One block is one second, so a run is broken by a single quiet
            // second rather than by a gap between characters.
            const int breakHops = 1;

            var bestStart = 0;
            var bestLength = 0;
            var runStart = -1;
            var absent = 0;

            for (var i = 0; i < present.Length; i++)
            {
                if (present[i])
                {
                    if (runStart < 0)
                    {
                        runStart = i;
                    }

                    absent = 0;
                }
                else if (runStart >= 0 && ++absent > breakHops)
                {
                    var length = i - absent - runStart;

                    if (length > bestLength)
                    {
                        bestLength = length;
                        bestStart = runStart;
                    }

                    runStart = -1;
                }
            }

            if (runStart >= 0 && present.Length - runStart > bestLength)
            {
                bestLength = present.Length - runStart;
                bestStart = runStart;
            }

            var longestSeconds = (double)bestLength;

            var dutyWhole = presentShare;
            var dutyWindow = bestLength == 0
                ? 0
                : present.Skip(bestStart).Take(bestLength).Count(v => v)
                  / (double)bestLength;

            var gap = dutyWindow - dutyWhole;

            rows.Add((capture, gap, string.Format(
                CultureInfo.InvariantCulture,
                "{0}	{1:0.0}	{2:0.0}	{3:0.0}	{4:0.0}	{5:0.0}	{6:+0.0;-0.0}	{7:0.0}	{8:0.0}",
                capture, seconds, presentShare * 100, longestSeconds,
                dutyWhole * 100, dutyWindow * 100, gap * 100, p20, median)));
        }

        foreach (var row in rows.OrderByDescending(r => r.Gap))
        {
            Console.WriteLine(row.Line);
        }
    }

    /// <summary>Where each threshold lands, per capture, in decibels.</summary>
    /// <remarks>
    /// **THE ORDER'S OWN ACCEPTANCE CRITERION** (work instruction 051, task 3):
    /// *on the known-good captures the threshold lands within a decibel or two of
    /// where it lands today.* This measures that directly rather than inferring
    /// it from a score, because a score conflates where the threshold is with
    /// what the decoder made of it.
    /// </remarks>
    private static void Thresholds()
    {
        Console.WriteLine("capture	otsuDb	percentileDb	moveDb	p20	p98	swing");

        foreach (var (capture, _, _) in Truths)
        {
            var path = Find(capture);

            if (path is null)
            {
                continue;
            }

            var audio = WavAudio.Read(path);
            var peak = CwSpectralPeak.Find(audio.Samples, audio.SampleRate) ?? 600;
            var envelope = Envelope(audio, peak);

            var db = envelope.Select(v => 20 * Math.Log10(Math.Max(v, 1e-12)))
                .ToArray();

            var sorted = (double[])db.Clone();
            Array.Sort(sorted);

            var p20 = At(sorted, 20);
            var p98 = At(sorted, 98);

            var otsu = CwUnitEstimator.Otsu(db);
            var pct = CwUnitEstimator.Threshold(db);

            Console.WriteLine(
                "{0}	{1:0.0}	{2:0.0}	{3:+0.0;-0.0}	{4:0.0}	{5:0.0}	{6:0.0}",
                capture, otsu, pct, pct - otsu, p20, p98, p98 - p20);
        }
    }

    /// <summary>One percentile of a sorted array.</summary>
    private static double At(double[] sorted, double share)
    {
        var at = (share / 100.0) * (sorted.Length - 1);
        var low = (int)Math.Floor(at);
        var high = Math.Min(low + 1, sorted.Length - 1);

        return sorted[low] + ((sorted[high] - sorted[low]) * (at - low));
    }

    /// <summary>The decoder's own envelope at one pitch.</summary>
    /// <remarks>
    /// The same function the decoder runs, so the thresholds compared below sit
    /// on the numbers the decoder actually sees (§0: one source of truth).
    /// </remarks>
    private static IReadOnlyList<double> Envelope(MonoAudio audio, double toneHz)
        => CwProbabilisticDecoder.Envelope(
            audio.Samples, audio.SampleRate, toneHz);

    /// <summary>Corpus score across candidate key-down fractions.</summary>
    /// <remarks>
    /// **THE ORDER FORBIDS FIXING IT AT 0.5 WITHOUT SWEEPING** (work instruction
    /// 051, task 3), and forbids adopting off a non-monotonic curve.
    /// </remarks>
    private static void FractionSweep(string[] only)
    {
        Console.WriteLine("fraction	yield	precision	correct	subs	ins	dels");

        var values = only.Length > 0
            ? only.Select(v => double.Parse(v, CultureInfo.InvariantCulture)).ToArray()
            : new[] { 0.20, 0.25, 0.30, 0.35, 0.40, 0.45, 0.50, 0.55, 0.60 };

        foreach (var fraction in values)
        {
            CwUnitEstimator.Fraction = fraction;
            ScoreQuietly(fraction.ToString("0.00", CultureInfo.InvariantCulture));
        }

        CwUnitEstimator.Fraction = 0.5;
    }

    /// <summary>Corpus score across candidate minimum swings.</summary>
    private static void MinSwingSweep(string[] only)
    {
        Console.WriteLine("minSwingDb	yield	precision	correct	subs	ins	dels");

        var values = only.Length > 0
            ? only.Select(v => double.Parse(v, CultureInfo.InvariantCulture)).ToArray()
            : new[] { 3.0, 4.0, 5.0, 6.0, 8.0, 10.0, 12.0, 15.0 };

        foreach (var swing in values)
        {
            CwUnitEstimator.MinimumSwingDb = swing;
            ScoreQuietly(swing.ToString("0.0", CultureInfo.InvariantCulture));
        }

        CwUnitEstimator.MinimumSwingDb = 6.0;
    }

    /// <summary>One corpus score as a single row.</summary>
    private static void ScoreQuietly(string label)
    {
        var truthTotal = 0;
        var correct = 0;
        var asserted = 0;
        var subs = 0;
        var ins = 0;
        var dels = 0;

        foreach (var (capture, truth, _) in Truths)
        {
            var path = Find(capture);

            if (path is null)
            {
                continue;
            }

            var audio = WavAudio.Read(path);
            var decoder = new CwDecoder(audio.SampleRate, 600);
            var text = new System.Text.StringBuilder();

            decoder.CharacterSettled += c => text.Append(c.Text);

            var hop = decoder.Tracker.HopSamples;

            for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
            {
                decoder.Process(new AudioChunk(
                    at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
            }

            decoder.Flush();

            var score = CwAccuracy.Score(text.ToString(), truth);

            truthTotal += score.TruthCharacters;
            correct += score.Correct;
            // **THE SAME DENOMINATOR THE CORPUS SCORE USES.** ScoredCharacters
            // counts blocks, which are refusals rather than assertions, so using
            // it here reported a precision that could not be compared with the
            // published figure at all (work instruction 054, task 2).
            asserted += score.Correct + score.Substitutions + score.Insertions;
            subs += score.Substitutions;
            ins += score.Insertions;
            dels += score.Deletions;
        }

        Console.WriteLine(
            "{0}	{1:0.000}	{2:0.000}	{3}	{4}	{5}	{6}",
            label,
            truthTotal == 0 ? 0 : (double)correct / truthTotal,
            asserted == 0 ? 0 : (double)correct / asserted,
            correct, subs, ins, dels);
    }

    /// <summary>What one spectral peak measurement costs, in milliseconds.</summary>
    /// <remarks>
    /// **THE DECODER RUNS ON THE AUDIO THREAD** (§8), and a decoder that stutters
    /// to take a measurement has traded the thing for the record of it. The peak
    /// is taken once a second over eight seconds of audio, which is seven
    /// half-overlapped transforms, and this says whether that fits inside a
    /// five-millisecond hop.
    /// </remarks>
    private static void PeakCost()
    {
        var rate = 8_000;
        var samples = new float[rate * 8];
        var random = new Random(20260829);

        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = (float)(Math.Sin(2 * Math.PI * 500 * i / rate) * 0.4
                                 + ((random.NextDouble() * 2) - 1) * 0.1);
        }

        // Warm the code paths so the first measurement is not the jitter.
        CwSpectralPeak.Find(samples, rate);

        var watch = System.Diagnostics.Stopwatch.StartNew();
        const int Runs = 20;

        for (var i = 0; i < Runs; i++)
        {
            CwSpectralPeak.Find(samples, rate);
        }

        watch.Stop();

        Console.WriteLine(
            "one peak over 8 s of 8 kHz audio: {0:0.00} ms, against a 5 ms hop "
            + "and one measurement a second",
            watch.Elapsed.TotalMilliseconds / Runs);
    }

    /// <summary>
    /// The tracker's answer, the FFT peak and the strongest keyed bin, per
    /// capture.
    /// </summary>
    /// <remarks>
    /// **THREE ESTIMATES OF ONE NUMBER** (work instruction 050, tasks 1 and 3).
    /// The tracker is what Hamlet commits to today; the peak is
    /// `CwSpectralPeak`; the keyed bin is `KeyingEnvelope.Best`, which is the
    /// only one of the three that asks whether anybody is keying rather than
    /// merely where the energy is.
    /// </remarks>
    private static void ToneTable(string? only)
    {
        var files = Directory
            .GetFiles(CaptureFolder(), "*.wav", SearchOption.AllDirectories)
            .Where(f => only is null
                || Path.GetFileName(f).Contains(only, StringComparison.Ordinal))
            .OrderBy(f => Path.GetFileName(f), StringComparer.Ordinal)
            .ToList();

        Console.WriteLine(
            "capture	sidecarHz	trackerHz	peakHz	keyedHz	purity	elementMs");

        foreach (var file in files)
        {
            var audio = WavAudio.Read(file);

            var tracker = Shipped(audio);
            var peak = CwSpectralPeak.Find(audio.Samples, audio.SampleRate);
            var keyed = KeyingEnvelope.Best(audio);

            Console.WriteLine(string.Join(
                "	",
                Path.GetFileNameWithoutExtension(file),
                Fmt(SidecarToneHz(file)),
                Fmt(tracker),
                peak is { } p ? p.ToString("0.0", CultureInfo.InvariantCulture) : "-",
                keyed is { } k
                    ? k.ToneHz.ToString("0.0", CultureInfo.InvariantCulture)
                    : "-",
                keyed is { } k2
                    ? k2.Profile.ElementPurity.ToString(
                        "0.00", CultureInfo.InvariantCulture)
                    : "-",
                keyed is { } k3
                    ? k3.Profile.ElementMedianMs.ToString(
                        "0.0", CultureInfo.InvariantCulture)
                    : "-"));
        }
    }

    /// <summary>A hertz figure, or a dash where nobody measured one.</summary>
    private static string Fmt(double hz)
        => double.IsNaN(hz) ? "-" : hz.ToString("0.0", CultureInfo.InvariantCulture);

    /// <summary>What the shipped decoder settles on, run over the whole file.</summary>
    private static double Shipped(MonoAudio audio)
    {
        var decoder = new CwDecoder(audio.SampleRate, 600);

        using var source = new BufferedAudioSource(audio);

        decoder.Listen(source);
        source.PumpAll();
        decoder.Flush();

        return decoder.Reading.ToneHz;
    }

    /// <summary>The pitch the capture sheet recorded, or NaN.</summary>
    private static double SidecarToneHz(string wav)
    {
        var sheet = Path.ChangeExtension(wav, ".txt");

        if (!File.Exists(sheet))
        {
            return double.NaN;
        }

        foreach (var line in File.ReadLines(sheet))
        {
            if (!line.StartsWith("toneHz", StringComparison.Ordinal))
            {
                continue;
            }

            var parts = line.Split(
                [' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 1
                && double.TryParse(
                    parts[1], CultureInfo.InvariantCulture, out var hz))
            {
                return hz;
            }
        }

        return double.NaN;
    }

    /// <summary>Where the captures live, found from the executable.</summary>
    private static string CaptureFolder()
    {
        var here = AppContext.BaseDirectory;

        while (!string.IsNullOrEmpty(here))
        {
            var candidate = Path.Combine(here, "tests", "fixtures", "cw", "captured");

            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            here = Path.GetDirectoryName(here.TrimEnd(
                Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        }

        throw new DirectoryNotFoundException("tests/fixtures/cw/captured");
    }
}
