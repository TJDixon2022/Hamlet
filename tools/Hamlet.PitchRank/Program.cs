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
