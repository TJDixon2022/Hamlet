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

    /// <remarks>W1AW's piece-04 through the chain, shipped and learned, the process's CPU time, the first read thrown away to warm the process.</remarks>
    [Fact]
    public void TheCost()
    {
        const string Piece = "w1aw-2026-10-07/piece-04";
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(Piece));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(Piece);

        double Time(bool on)
        {
            using var chosen = on ? CwRules.Choose(CwRules.LearnedReader) : null;
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
}
