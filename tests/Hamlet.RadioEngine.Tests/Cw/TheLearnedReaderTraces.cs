using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE ROUNDS' TRACE AND QUICK MEASURE** (work instruction 597): what the learned reader prints beside the stretches in <c>CWNET_SPEC.md</c>, and
/// its figures. Each asserts nothing and none is on a line.
/// </summary>
[Trait("Category", "Survey")]
public sealed class TheLearnedReaderTraces(ITestOutputHelper output)
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    private void Write(string line) => output.WriteLine(line);

    private static string Between(IReadOnlyList<TheRecordingsScoreboardTests.Printed> letters, double from, double to)
        => TheRecordingsScoreboardTests.Text(letters.Where(l => l.Seconds >= from && l.Seconds < to).ToList());

    /// <remarks>The order's trace: Ken, Bryan, W1AW's two pieces and the first recording, whole.</remarks>
    [Fact]
    public void TheTexts()
    {
        using var on = CwRules.Choose(CwRules.LearnedReader);
        var ken = TheRecordingsScoreboardTests.ReadLive(CwWildHands.Recording).Letters;

        Write("KEN 0-120 s: `" + Between(ken, 0, 120) + "`");
        Write("BRYAN 120-300 s: `" + Between(ken, 120, 300) + "`");

        // Each letter's own pitch and span: what puts it in a stretch or leaves it out when it is scored.
        Write("PITCHES: " + string.Join(", ", ken.GroupBy(l => Math.Round(l.PitchHz / 25) * 25).OrderBy(g => g.Key).Select(g => string.Create(Inv, $"{g.Key:0} {g.Count()}"))));

        foreach (var l in ken.Where(l => l.Seconds >= 122 && l.Seconds < 150))
        {
            Write(string.Create(Inv, $"  {l.From:0.00}-{l.Seconds:0.00} {l.PitchHz:0} {l.Text}"));
        }

        foreach (var piece in new[] { "w1aw-2026-10-07/piece-03", "w1aw-2026-10-07/piece-04" })
        {
            var letters = TheRecordingsScoreboardTests.ReadLive(piece).Letters;

            Write($"{piece} first 60 s: `" + Between(letters, 0, 60) + "`");
        }

        Write("the first recording: `" + TheRecordingsScoreboardTests.ReadLive("cw-2026-10-02-200157").Text + "`");
    }

    /// <remarks>The three recordings the board counts where the survey shows nothing: everything the net printed, at any pitch.</remarks>
    [Fact]
    public void TheEmptyRows()
    {
        using var on = CwRules.Choose(CwRules.LearnedReader);

        foreach (var recording in new[] { "cw-2026-10-08-121324", "cw-2026-10-08-121357", "cw-2026-10-08-121414" })
        {
            var (letters, text) = TheRecordingsScoreboardTests.ReadLive(recording);

            Write(string.Create(Inv, $"{recording}: `{text}`; pitches {string.Join(", ", letters.GroupBy(l => Math.Round(l.PitchHz / 25) * 25).OrderBy(g => g.Key).Select(g => $"{g.Key:0} {g.Count()}"))}; stretches {string.Join(", ", TheRecordingsScoreboardTests.Score(limits: false).Stretches.Where(s => s.Stretch.Recording == recording).Select(s => $"{s.Stretch.From:0.#}-{s.Stretch.To:0.#} s `{s.Stretch.Reference}`"))}"));
        }
    }

    /// <remarks>The two real hands, per 20 s, as the survey scores them.</remarks>
    [Fact]
    public void TheHands()
    {
        using var on = CwRules.Choose(CwRules.LearnedReader);
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(CwWildHands.Recording));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(CwWildHands.Recording);
        var (letters, _) = CwWildHands.Read(audio.Samples, audio.SampleRate, pitch, width, []);
        var map = TheKeyingMapTests.Read(CwWildHands.Recording, audio.Samples, audio.SampleRate);

        foreach (var s in CwWildHands.Truths().Select(t => CwWildHands.ScoreOf(t, letters, map)))
        {
            Write(string.Create(Inv, $"{s.Truth.Name}, {s.Truth.From:0} to {s.Truth.To:0} s at {s.Truth.PitchHz:0} Hz: {s.Right} of {s.OutOf} right, {s.Wrong} wrong, {s.Invented} invented, {s.Silence} in silence, spaces {s.SpacesRight} of {s.SpacesOutOf}, {s.SpacesAdded} added"));

            foreach (var w in s.Windows)
            {
                Write(string.Create(Inv, $"  {w.From:0}-{w.To:0} s: {w.Right} of {w.Letters}, {w.Wrong} wrong, {w.Invented} invented `{w.Printed}`"));
            }
        }
    }

    /// <remarks>The rounds' quick measure: the wild bank, the two boards, the first recording and the noise.</remarks>
    [Fact]
    public void TheQuickMeasure()
    {
        var rows = new System.Collections.Concurrent.ConcurrentBag<(Fixtures.CwWildHand Hand, TheCwSurveyTests.Row Row)>();
        using var on = CwRules.Choose(CwRules.LearnedReader);
        var rules = CwRules.Current;

        Parallel.ForEach(Fixtures.CwWildBank.All, new ParallelOptions { MaxDegreeOfParallelism = 3 }, hand =>
        {
            using var here = CwRules.Use(rules);

            rows.Add((hand, TheCwSurveyTests.Wild(hand)));
        });

        foreach (var recipe in Fixtures.CwWildBank.Recipes)
        {
            var mine = rows.Where(r => r.Hand.Recipe == recipe).ToList();

            Write(string.Create(Inv, $"{recipe.Name}: {mine.Sum(r => r.Row.Right ?? 0)} right, {mine.Sum(r => r.Row.Wrong ?? 0)} wrong, {mine.Sum(r => r.Row.Invented ?? 0)} invented, score {mine.Sum(r => r.Row.Score ?? 0)}"));
        }

        Write(string.Create(Inv, $"total: {rows.Sum(r => r.Row.Right ?? 0)} right, {rows.Sum(r => r.Row.Wrong ?? 0)} wrong, {rows.Sum(r => r.Row.Invented ?? 0)} invented, score {rows.Sum(r => r.Row.Score ?? 0)}"));

        var board = TheRecordingsScoreboardTests.Score();
        var w1aw = TheW1awTableTests.Score();

        Write(string.Create(Inv, $"the twelve-plus-three {board.Score}: {board.Total} of {board.OutOf}, {board.Wrong} wrong, {board.Invented} invented; W1AW {w1aw.Score}: {w1aw.Right} of {w1aw.OutOf}, {w1aw.Wrong} wrong, {w1aw.Invented} invented"));
        Write("the first recording: " + board.FirstReads);
        Write(string.Create(Inv, $"letters in silences: {board.SilencePrints.Count}"));

        foreach (var n in board.Noise)
        {
            Write($"{n.What}: `{n.Reads}`");
        }
    }

    /// <remarks>Work instruction 598: the twelve-plus-three stretch by stretch, KM3STU's rows marked, with the learned reader on.</remarks>
    [Fact]
    public void TheBoardRows()
    {
        using var on = CwRules.Choose(CwRules.LearnedReader);
        var board = TheRecordingsScoreboardTests.Score(limits: false);

        foreach (var s in board.Stretches)
        {
            var km3stu = s.Stretch.Recording.StartsWith("cw-2026-10-08-12", StringComparison.Ordinal) ? "KM3STU " : string.Empty;

            Write(string.Create(Inv, $"{km3stu}{s.Stretch.Recording} {s.Stretch.From:0.#}-{s.Stretch.To:0.#} s @{s.Stretch.PitchHz:0} ({s.Stretch.Confidence}): {s.Right} of {s.ReferenceLetters}, {s.Wrong} wrong `{s.PrintedText}` against `{s.Stretch.Reference}`"));
        }

        var his = board.Stretches.Where(s => s.Stretch.Recording.StartsWith("cw-2026-10-08-12", StringComparison.Ordinal) && s.Stretch.Confidence >= TheRecordingsScoreboardTests.Confidence.Medium).ToList();

        Write(string.Create(Inv, $"KM3STU counted: {his.Sum(s => s.Right)} of {his.Sum(s => s.ReferenceLetters)}, {his.Sum(s => s.Wrong)} wrong"));
        Write(string.Create(Inv, $"the twelve-plus-three {board.Score}: {board.Total} of {board.OutOf}, {board.Wrong} wrong, {board.Invented} invented; first `{board.FirstReads}`; letters in silences {board.SilencePrints.Count}"));

        foreach (var n in board.Noise)
        {
            Write($"{n.What}: `{n.Reads}`");
        }
    }

    /// <remarks>Work instruction 598: the synthetic CQs and the easy receiver fixtures as the survey scores them, with the learned reader on.</remarks>
    [Fact]
    public void TheFixtures()
    {
        using var on = CwRules.Choose(CwRules.LearnedReader);
        var root = CwFixtures.Folder;

        foreach (var wav in Directory.GetFiles(Path.Combine(root, "synthetic-cq"), "*.wav").Order())
        {
            var row = TheCwSurveyTests.Truthful("cq", wav, TheCwSurveyTests.SheetText(wav), "sheet", 600, 500);

            Write(string.Create(Inv, $"{Path.GetFileName(wav)}: {row.Right} of {row.OutOf}"));
        }

        var easy = Directory.GetFiles(Path.Combine(root, "receiver"), "*-easy.wav").Order()
            .Select(wav => TheCwSurveyTests.Truthful("easy", wav, TheCwSurveyTests.SheetText(wav), "sheet", 600, 500)).ToList();

        Write(string.Create(Inv, $"easy receiver fixtures: {easy.Sum(r => r.Right ?? 0)} of {easy.Sum(r => r.OutOf ?? 0)}"));
    }

    /// <remarks>
    /// Work instruction 599: with the learned reader on, every figure a guard on the lines asserts that the scoreboard trace does not print: the fist
    /// bank's four rows and its fading half, `two-station`, each 5 dB CQ as the weak-end guard reads it, what prints in the 20 s of noise after each,
    /// and the nine straight-key hands.
    /// </remarks>
    [Fact]
    public void TheGuards()
    {
        using var on = CwRules.Choose(CwRules.LearnedReader);
        var rules = CwRules.Current;
        var fists = new System.Collections.Concurrent.ConcurrentBag<(Fixtures.CwFist Fist, int Right)>();

        Parallel.ForEach(Fixtures.CwFistBank.All, new ParallelOptions { MaxDegreeOfParallelism = 3 }, fist =>
        {
            using var here = CwRules.Use(rules);

            fists.Add((fist, TheCwSurveyTests.Fist(fist).Right ?? 0));
        });

        foreach (var ratio in new[] { 1.6, 2.0, 2.5, 3.0 })
        {
            Write(string.Create(Inv, $"fist bank, dah over dit {ratio:0.0}: {fists.Where(r => r.Fist.Ratio == ratio).Sum(r => r.Right)} of {fists.Count(r => r.Fist.Ratio == ratio) * 60}"));
        }

        Write(string.Create(Inv, $"fist bank, the fading half: {fists.Where(r => r.Fist.Fades).Sum(r => r.Right)} of {fists.Count(r => r.Fist.Fades) * 60}; the whole bank {fists.Sum(r => r.Right)}"));

        var two = Path.Combine(CwFixtures.Folder, "receiver", "two-station.wav");
        var twoRow = TheCwSurveyTests.Truthful("receiver fixtures: ungraded", two, TheCwSurveyTests.SheetText(two), "its sheet", 600, 500);

        Write($"two-station: `{twoRow.Read}`, {twoRow.Right} of {twoRow.OutOf}");

        var total = 0;

        foreach (var (name, wav, r) in TheWeakEndTests.Files().Where(f => f.Name.Contains("-5db", StringComparison.Ordinal)))
        {
            var audio = WavAudio.Read(wav);
            var reference = System.Text.RegularExpressions.Regex.Replace(r.Text, @"\^([A-Z]{2})", "<$1>");
            var read = TheWeakEndTests.Chain(audio.Samples, audio.SampleRate, r.ToneHz, handover: true);
            var right = TheRecordingsScoreboardTests.Right(read.Text, reference);

            total += right;
            Write($"5 dB CQ {name}: `{read.Text}`, {right} of {TheRecordingsScoreboardTests.Letters(reference).Length}");

            var lastKeyUp = TheWeakEndTests.Sent(r)[^1].To;
            var tail = audio.Samples.AsSpan(audio.Samples.Length - audio.SampleRate).ToArray();
            var tailRms = Math.Sqrt(tail.Average(x => (double)x * x));
            var silence = new float[20 * audio.SampleRate];
            var silenceRms = Fixtures.CwFixtureGenerator.ShapedNoise(silence, r.Seed + 1);

            for (var i = 0; i < silence.Length; i++)
            {
                silence[i] = (float)Math.Clamp(silence[i] * tailRms / silenceRms, -1, 1);
            }

            var samples = audio.Samples.Concat(silence).ToArray();
            using var chain = new CwChain(audio.SampleRate);
            var letters = new List<(double At, string Text)>();
            var chunk = audio.SampleRate / 100;

            chain.Detector.SetPassband(600, 500);
            chain.Decoder.Runs.RunRead += (c, run) => letters.Add((run[^1].ToSeconds, c.Text));

            for (var at = 0; at + chunk <= samples.Length; at += chunk)
            {
                chain.Process(new Hamlet.RadioEngine.Audio.AudioChunk(at, audio.SampleRate, samples.AsSpan(at, chunk)));
            }

            chain.Decoder.Flush();

            var after = letters.Where(l => l.At > lastKeyUp + 0.05).ToList();

            Write(string.Create(Inv, $"  then 20 s of noise: {after.Count} letters{(after.Count > 0 ? $", `{string.Join(" ", after.Select(a => $"{a.Text}@{a.At:0.0}"))}`" : string.Empty)}"));
        }

        Write($"5 dB CQs: {total} of 84");

        foreach (var ratio in TheStraightKeyReadsTests.Ratios)
        {
            foreach (var seed in TheStraightKeyReadsTests.Seeds)
            {
                var row = TheStraightKeyReadsTests.Read(ratio, seed);

                Write(string.Create(Inv, $"straight key, dah over dit {ratio:0.0}, seed {seed}: {row.Right} of {row.Of}, `{row.Reading.Text}`"));
            }
        }
    }

    /// <remarks>W1AW's piece-04 through the chain, shipped and learned, the process's CPU time, the first read thrown away to warm the process.</remarks>
    [Fact]
    public void TheCost()
    {
        const string Piece = "w1aw-2026-10-07/piece-04";
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(Piece));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(Piece);

        double Time(bool on)
        {
            using var chosen = on ? CwRules.Choose(CwRules.LearnedReader) : CwRules.Off(CwRules.LearnedReader);
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var cpu = process.TotalProcessorTime;

            TheRecordingsScoreboardTests.ReadLive(audio.Samples, audio.SampleRate, pitch, width);
            process.Refresh();

            return (process.TotalProcessorTime - cpu).TotalSeconds;
        }

        Time(false);
        Time(true);

        var shipped = Time(false);
        var learned = Time(true);

        Write(string.Create(Inv, $"{Piece}: {audio.Samples.Length / (double)audio.SampleRate:0} s of audio; shipped {shipped:0.0} s CPU, learned {learned:0.0} s CPU, {(learned - shipped) / shipped:+0%;-0%}"));
    }

    // One scored letter of a set: its probability (null where no net read it) and whether the board's alignment made it right (true),
    // wrong (false: substituted or inserted), or did not score it (null: a free end, or under a `?` of a draft truth).
    private readonly record struct Judged(double? Probability, bool? Right);

    private static readonly double[] Cuts = [0.25, 0.5, 0.75];

    private static bool? RightOf(TheRecordingsScoreboardTests.LetterMark mark) => mark switch
    {
        TheRecordingsScoreboardTests.LetterMark.Right => true,
        TheRecordingsScoreboardTests.LetterMark.Wrong or TheRecordingsScoreboardTests.LetterMark.Added => false,
        _ => null,
    };

    private static IEnumerable<Judged> Of(TheRecordingsScoreboardTests.Scored s)
        => s.Letters!.Select((l, i) => new Judged(l.Probability, RightOf(s.Marks![i])));

    // The twelve counts of one set: at each cut, letters under it right and wrong, letters at or over it right and wrong.
    private void Counts(string name, IReadOnlyList<Judged> set)
    {
        var scored = set.Where(j => j.Right is not null && j.Probability is not null).ToList();

        Write(string.Create(Inv, $"{name}: {set.Count} letters, {set.Count(j => j.Probability is null)} with no probability, {set.Count(j => j.Right is null)} not scored (free ends, `?`), {scored.Count} scored: {scored.Count(j => j.Right == true)} right, {scored.Count(j => j.Right == false)} wrong"));

        foreach (var cut in Cuts)
        {
            var under = scored.Where(j => j.Probability < cut).ToList();
            var over = scored.Where(j => j.Probability >= cut).ToList();

            Write(string.Create(Inv, $"  line {cut:0.00}: under {under.Count(j => j.Right == true)} right / {under.Count(j => j.Right == false)} wrong; at or over {over.Count(j => j.Right == true)} right / {over.Count(j => j.Right == false)} wrong"));
        }
    }

    /// <remarks>
    /// Work instruction 600, HM-DEC-305: the dimming line measured. A learned letter is dimmed where the net's probability for it is under one
    /// half; this counts, at one quarter, one half and three quarters, how the letters under and over each line score on the board's own
    /// alignment, on the twelve-plus-three, the fist bank, W1AW, the two real hands and the first recording; then every letter printed in the
    /// board's silences with its probability; then the twelve-plus-three's right and wrong letters in ten bins of probability. Measures; moves nothing.
    /// </remarks>
    [Fact]
    public void TheDimmingLine()
    {
        var board = TheRecordingsScoreboardTests.Score();
        var counted = board.Stretches.Where(s => s.Stretch.Confidence >= TheRecordingsScoreboardTests.Confidence.Medium).ToList();
        var twelve = counted.SelectMany(Of).ToList();

        Write(string.Create(Inv, $"the twelve-plus-three {board.Score}: {board.Total} of {board.OutOf}, {board.Wrong} wrong, {board.Invented} invented; first `{board.FirstReads}`; letters in silences {board.SilencePrints.Count}"));
        Counts("the twelve-plus-three", twelve);

        // The fist bank: every hand read as the survey reads it and aligned to the text it sent, as the bank's rows score it.
        var rules = CwRules.Current;
        var fists = new System.Collections.Concurrent.ConcurrentBag<(Fixtures.CwFist Fist, List<Judged> Letters)>();

        Parallel.ForEach(Fixtures.CwFistBank.All, new ParallelOptions { MaxDegreeOfParallelism = 3 }, fist =>
        {
            using var here = CwRules.Use(rules);
            var audio = Fixtures.CwFistBank.Audio(fist);
            var (letters, _) = TheRecordingsScoreboardTests.ReadLive(audio.Samples, audio.SampleRate, 600, 500);
            var marks = TheRecordingsScoreboardTests.MarksOf(TheRecordingsScoreboardTests.Text(letters), Fixtures.CwFistBank.Text);

            fists.Add((fist, letters.Select((l, i) => new Judged(l.Probability, RightOf(marks[i]))).ToList()));
        });

        Counts("the fist bank, every row", fists.SelectMany(f => f.Letters).ToList());

        foreach (var ratio in Fixtures.CwFistBank.Ratios)
        {
            Counts(string.Create(Inv, $"  the fist bank row {ratio:0.0}"), fists.Where(f => f.Fist.Ratio == ratio).SelectMany(f => f.Letters).ToList());
        }

        var w1aw = TheW1awTableTests.Score();

        Write(string.Create(Inv, $"W1AW {w1aw.Score}: {w1aw.Right} of {w1aw.OutOf}, {w1aw.Wrong} wrong, {w1aw.Invented} invented"));
        Counts("W1AW", w1aw.Stretches.Where(s => s.Stretch.Confidence >= TheRecordingsScoreboardTests.Confidence.Medium).SelectMany(Of).ToList());

        // The two real hands against the draft truth, as the survey scores them: a `?` matches anything and is not scored.
        var hands = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(CwWildHands.Recording));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(CwWildHands.Recording);
        var (handLetters, _) = CwWildHands.Read(hands.Samples, hands.SampleRate, pitch, width, []);
        var map = TheKeyingMapTests.Read(CwWildHands.Recording, hands.Samples, hands.SampleRate);

        foreach (var s in CwWildHands.Truths().Select(t => CwWildHands.ScoreOf(t, handLetters, map)))
        {
            var judged = s.Letters.Select((l, i) => new Judged(l.Probability, null)).ToArray();

            for (var d = 0; d < s.Kinds.Count; d++)
            {
                var right = s.Kinds[d] switch
                {
                    CwWildHands.LetterIs.Same => true,
                    CwWildHands.LetterIs.Wrong or CwWildHands.LetterIs.Added => (bool?)false,
                    _ => null,
                };

                judged[s.Of[d]] = judged[s.Of[d]] with { Right = right };
            }

            Counts(string.Create(Inv, $"{s.Truth.Name} {s.Truth.From:0}-{s.Truth.To:0} s ({s.Right} of {s.OutOf}, {s.Wrong} wrong)"), judged);
        }

        var first = board.Stretches.Single(s => s.Stretch.Recording == "cw-2026-10-02-200157");

        Counts(string.Create(Inv, $"the first recording ({first.Right} of {first.ReferenceLetters}, `{first.PrintedText}`)"), Of(first).ToList());

        // The letters printed in the board's silences, the `E I EE` the owner saw between transmissions.
        Write(string.Create(Inv, $"letters in silences: {board.SilencePrints.Count}, under one half {board.SilencePrints.Count(p => p.Letter.Probability < 0.5)}, with no probability {board.SilencePrints.Count(p => p.Letter.Probability is null)}"));

        foreach (var (recording, quiet, letter) in board.SilencePrints)
        {
            Write(string.Create(Inv, $"  `{recording}` {quiet.From:0.0}-{quiet.To:0.0} s: `{letter.Text}` at {letter.From:0.00} s {letter.PitchHz:0} Hz, probability {letter.Probability:0.000}{(letter.Probability < 0.5 ? " (dim)" : " (bright)")}"));
        }

        // Ten bins of probability on the twelve-plus-three, right and wrong apart.
        Write("probability on the twelve-plus-three | right | wrong");

        for (var b = 0; b < 10; b++)
        {
            var low = b / 10.0;
            var high = (b + 1) / 10.0;
            bool In(Judged j) => j.Probability >= low && (j.Probability < high || (b == 9 && j.Probability <= 1));

            Write(string.Create(Inv, $"  {low:0.0}-{high:0.0} | {twelve.Count(j => j.Right == true && In(j))} | {twelve.Count(j => j.Right == false && In(j))}"));
        }
    }
}
