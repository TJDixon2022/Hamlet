using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Tests.Scan;
using Xunit;
using Xunit.Abstractions;
using Printed = Hamlet.RadioEngine.Tests.Cw.TheRecordingsScoreboardTests.Printed;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE SURVEY: EVERY CW RECORDING READ ONCE, NONE PROMOTED** (work instruction 567, HM-DEC-271). The owner, 2026-10-09:
/// *"Let's run a unit that tests against every WAV file we have in our library. This is a one-time exception, so we can kind
/// of measure against existing standards."* Every CW `.wav` under `tests\fixtures\cw\` is read through the app's chain at
/// HEAD, the way a board row is read, and scored by the boards' yardstick where the tree already holds what was sent. Nothing
/// here is a board and nothing is promoted to one.
/// </summary>
/// <remarks>
/// **IT NEVER RUNS BY ITSELF.** It carries the `Survey` category, which every carry-forward line filters out, and runs only by
/// its own filter, the line named at the head of the document it writes:
/// <c>timeout 1800 dotnet test tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj --filter "FullyQualifiedName~TheCwSurveyTests.EveryRecordingInTheTree"</c>.
/// </remarks>
[Trait("Category", "Survey")]
public sealed class TheCwSurveyTests(ITestOutputHelper output)
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>The single-letter words of one or two elements, the stray-letter figure.</summary>
    private static readonly HashSet<string> Strays = new(StringComparer.Ordinal) { "E", "T", "I", "A", "N", "M" };

    /// <summary>
    /// Trace recordings in `captured`, placed by the owner's order and on neither board, with no truth: read in the unadjudicated
    /// group with the old captures. `cw-2026-10-19-141708` is Ken's QSO with KB2UZK, the owner's order of 2026-10-19 (work
    /// instruction 574).
    /// </summary>
    internal static readonly HashSet<string> Traces = new(StringComparer.Ordinal) { "cw-2026-10-19-141708" };

    /// <summary>The owner's line: six lone one- or two-element words per 10 s, no capture above it (HM-DEC-276).</summary>
    internal const double StraysLine = 6;

    /// <summary>One file's row.</summary>
    internal sealed record Row(
        string Group, string File, double Seconds, double Wpm, double PitchHz, int Senders, string TruthSource, string? Truth,
        int? Right, int? OutOf, int? Wrong, int? Invented, int? Silence, string Spaces, string Read, string? Old = null,
        double? Stray = null, double? OldStray = null, string? OldInThis = null)
    {
        public int? Score => Right is { } r ? r - (Wrong ?? 0) - (Invented ?? 0) : null;
    }

    /// <remarks>
    /// Reads every CW recording in the tree once and writes `docs\cw-survey-2026-10-09.md`. Asserts only that the two boards'
    /// figures in the survey are the boards' own, since they are read by the boards' own scoring.
    /// </remarks>
    [Fact]
    public void EveryRecordingInTheTree()
    {
        var root = CwFixtures.Folder;
        var rows = new System.Collections.Concurrent.ConcurrentBag<Row>();
        var failed = new System.Collections.Concurrent.ConcurrentBag<string>();
        var count = Directory.GetFiles(root, "*.wav", SearchOption.AllDirectories)
            .GroupBy(f => Path.GetRelativePath(root, Path.GetDirectoryName(f)!))
            .ToDictionary(g => g.Key == "." ? "cw" : "cw\\" + g.Key, g => g.Count());

        // The boards, by their own scoring: the twelve-plus-three and the w1aw table.
        var board = TheRecordingsScoreboardTests.Score(limits: false);
        var w1aw = TheW1awTableTests.Score();

        foreach (var recording in board.Stretches.Select(s => s.Stretch.Recording).Distinct())
        {
            var mine = board.Stretches.Where(s => s.Stretch.Recording == recording).ToList();
            var counted = mine.Where(s => s.Stretch.Confidence >= TheRecordingsScoreboardTests.Confidence.Medium).ToList();
            var letters = mine.SelectMany(s => s.Letters ?? []).ToList();

            rows.Add(new Row(
                "the boards: twelve-plus-three", recording, Seconds(TheOwnersRecordingReadsTests.Wav(recording)), Wpm(letters), Pitch(letters), Senders(letters),
                $"the board's reference ({string.Join(", ", mine.Select(s => s.Stretch.Confidence.ToString().ToLowerInvariant()))}; {counted.Count} of {mine.Count} stretches counted)",
                string.Join(" | ", mine.Select(s => s.Stretch.Reference)),
                counted.Sum(s => s.Right), counted.Sum(s => s.ReferenceLetters), counted.Sum(s => s.Wrong), board.InventedBy.TryGetValue(recording, out var inv) ? inv.Count : 0,
                board.SilencePrints.Count(p => p.Recording == recording),
                $"{counted.Sum(s => s.Spaces.Right)} of {counted.Sum(s => s.Spaces.OfReference)}, {counted.Sum(s => s.Spaces.Added)} added",
                string.Join(" | ", mine.Select(s => s.PrintedText))));
        }

        foreach (var piece in w1aw.Stretches.Select(s => s.Stretch.Recording).Distinct())
        {
            var mine = w1aw.Stretches.Where(s => s.Stretch.Recording == piece).ToList();
            var letters = mine.SelectMany(s => s.Letters ?? []).ToList();

            rows.Add(new Row(
                "W1AW pieces", piece, Seconds(TheOwnersRecordingReadsTests.Wav(piece)), Wpm(letters), Pitch(letters), Senders(letters),
                $"the w1aw table's references ({mine.Count} stretches)", string.Join(" ", mine.Select(s => s.Stretch.Reference)),
                mine.Sum(s => s.Right), mine.Sum(s => s.ReferenceLetters), mine.Sum(s => s.Wrong), mine.Sum(s => s.Invented), null,
                $"{mine.Sum(s => s.Spaces.Right)} of {mine.Sum(s => s.Spaces.OfReference)}, {mine.Sum(s => s.Spaces.Added)} added",
                string.Join(" ", mine.Select(s => s.PrintedText))));
        }

        // Everything else, read once each, in parallel.
        var onBoards = board.Stretches.Select(s => s.Stretch.Recording).Concat(TheW1awSessionReplayTests.Pieces).ToHashSet(StringComparer.Ordinal);
        var jobs = new List<Func<Row>>();

        foreach (var f in CwFixtures.All)
        {
            jobs.Add(() => Truthful("synthetic fixtures", Path.Combine(root, f.Name + ".wav"), f.Sent, "`CwFixtures.All`, the text the generator sent", 600, 500));
        }

        foreach (var wav in Directory.GetFiles(Path.Combine(root, "receiver"), "*.wav").Order())
        {
            var name = Path.GetFileNameWithoutExtension(wav);
            var grade = name.EndsWith("-easy", StringComparison.Ordinal) ? "easy" : name.EndsWith("-working", StringComparison.Ordinal) ? "working" : name.EndsWith("-edge", StringComparison.Ordinal) ? "edge" : "ungraded";

            jobs.Add(() => Truthful($"receiver fixtures: {grade}", wav, SheetText(wav), "the `text` line of its sheet, written by the generator", 600, 500));
        }

        foreach (var wav in Directory.GetFiles(Path.Combine(root, "synthetic-cq"), "*.wav").Order())
        {
            var level = Regex.Match(Path.GetFileName(wav), @"-(\d+)db").Groups[1].Value;

            jobs.Add(() => Truthful($"synthetic CQs: {level} dB", wav, SheetText(wav), "the `text` line of its sheet and its `.key.md`, exact by construction", 600, 500));
        }

        var august = new Dictionary<string, (string? Contains, string Source)>(StringComparer.Ordinal)
        {
            ["cw-2026-08-17-013347"] = ("VA3VRR", "HM-DEC-145: the callsign VA3VRR is in it, adjudicated from its elements"),
            ["cw-2026-08-17-134712"] = ("N4L", "HM-DEC-144: the callsign N4L is in it, adjudicated from its elements"),
            ["cw-2026-08-17-013622"] = (null, "none: its sheet carries no adjudicated text"),
            ["cw-2026-08-18-004507"] = (null, "none: its sheet carries no adjudicated text"),
        };

        foreach (var wav in Directory.GetFiles(Path.Combine(root, "captured"), "*.wav").Order())
        {
            var name = Path.GetFileNameWithoutExtension(wav);

            if (onBoards.Contains(name))
            {
                continue;
            }

            if (august.TryGetValue(name, out var adjudicated))
            {
                jobs.Add(() => Partial("August adjudicated", wav, name, adjudicated.Contains, adjudicated.Source));
            }
            else if (Traces.Contains(name))
            {
                // A trace recording placed by the owner's order, with no truth: read with the old captures (work instruction 574).
                jobs.Add(() => NoTruth("unadjudicated", wav, name, OldRead(wav)));
            }
            else
            {
                failed.Add($"{name}: a captured recording on neither board nor among the August four; read as unadjudicated");
                jobs.Add(() => NoTruth("captured, elsewhere", wav, name, OldRead(wav)));
            }
        }

        foreach (var wav in Directory.GetFiles(Path.Combine(root, "captured", "unadjudicated"), "*.wav").Order())
        {
            var name = Path.GetFileNameWithoutExtension(wav);

            jobs.Add(() => NoTruth("unadjudicated", wav, "unadjudicated/" + name, OldRead(wav)));
        }

        foreach (var wav in Directory.GetFiles(Path.Combine(root, "captured", "scans"), "*.wav").Order())
        {
            jobs.Add(() => NoTruth("scan catches", wav, null, CatchRead(wav)));
        }

        // **THE FIST BANK** (work instruction 580, HM-DEC-284): ninety-six hands generated from their recipes, each scored against
        // what it sent and its own keying.
        foreach (var fist in Fixtures.CwFistBank.All)
        {
            jobs.Add(() => Fist(fist));
        }

        Parallel.ForEach(jobs, new ParallelOptions { MaxDegreeOfParallelism = 3 }, job =>
        {
            try
            {
                rows.Add(job());
            }
            catch (Exception e)
            {
                failed.Add($"a read failed: {e.GetType().Name} {e.Message}");
            }
        });

        var doc = Document(rows.ToList(), count, board, w1aw, failed.ToList());
        var path = Path.Combine(root, "..", "..", "..", "docs", "cw-survey-2026-10-09.md");

        File.WriteAllText(path, doc);
        output.WriteLine(doc);

        Assert.Equal(board.Score, rows.Where(r => r.Group.StartsWith("the boards", StringComparison.Ordinal)).Sum(r => r.Right!.Value - r.Wrong!.Value) - board.Invented);
        Assert.Equal(w1aw.Score, w1aw.Right - w1aw.Wrong - w1aw.Invented);
    }

    // A fist of the bank (work instruction 580): generated, read, and scored by the boards' yardstick against what it sent; invented is
    // a letter whose marks lie over none of the fist's own key-downs within 50 Hz of its pitch, and silence a letter inside a stretch of
    // two seconds or more with no key-down, both from the recipe rather than read off the audio.
    internal static Row Fist(Fixtures.CwFist fist)
    {
        var audio = Fixtures.CwFistBank.Audio(fist);
        var edges = Fixtures.CwFistBank.Edges(fist);
        var (letters, _) = TheRecordingsScoreboardTests.ReadLive(audio.Samples, audio.SampleRate, 600, 500);
        var downs = Enumerable.Range(0, edges.Length / 2).Select(i => (From: edges[2 * i], To: edges[(2 * i) + 1])).ToList();

        bool Keyed(Printed l) => Math.Abs(l.PitchHz - Fixtures.CwFistBank.ToneHz) <= Hamlet.RadioEngine.Cw.CwEnvelopeDetector.HandoverHz
            && downs.Any(d => d.To > l.From - 0.05 && d.From < l.Seconds + 0.05);

        bool InSilence(Printed l) => l.Seconds < downs[0].From - 2 || l.From > downs[^1].To + 2;

        var invented = letters.Where(l => !Keyed(l)).ToList();
        var printed = TheRecordingsScoreboardTests.Text(letters);
        var wrong = TheRecordingsScoreboardTests.WrongAt(printed, Fixtures.CwFistBank.Text).Count(i => i < letters.Count && !invented.Contains(letters[i]));
        var spaces = TheRecordingsScoreboardTests.SpacesOf(printed, Fixtures.CwFistBank.Text);

        return new Row(
            FistGroup, fist.Name, audio.Samples.Length / (double)audio.SampleRate, Wpm(letters), Pitch(letters), Senders(letters),
            fist.Recipe, Fixtures.CwFistBank.Text, TheRecordingsScoreboardTests.Right(printed, Fixtures.CwFistBank.Text), TheRecordingsScoreboardTests.Letters(Fixtures.CwFistBank.Text).Length,
            wrong, invented.Count, letters.Count(InSilence), $"{spaces.Right} of {spaces.OfReference}, {spaces.Added} added", printed);
    }

    /// <summary>The fist bank's group in the survey (work instruction 580).</summary>
    internal const string FistGroup = "the fist bank";

    // **THE FIST BANK'S MATRICES** (work instruction 580, HM-DEC-284): letters right of 60, one fist a cell, across the axis pairs
    // the order names, the whole bank's total, and its worst ten.
    private static void FistBank(StringBuilder sb, List<Row> fists)
    {
        if (fists.Count == 0)
        {
            return;
        }

        var bank = Fixtures.CwFistBank.All.ToDictionary(f => f.Name, f => f);
        int? Right(double ratio, double wpm, double db, bool fades)
            => fists.FirstOrDefault(r => r.File == new Fixtures.CwFist(ratio, wpm, db, fades).Name)?.Right;

        string Cell(int? right) => right is { } r ? r.ToString(Inv) : "-";

        sb.AppendLine();
        sb.AppendLine("## The fist bank");
        sb.AppendLine();
        sb.AppendLine(string.Create(Inv, $"Work instruction 580, HM-DEC-284: {fists.Count} hands generated at test time from their recipes (`CwFistBank`), each sending `{Fixtures.CwFistBank.Text}`, 60 letters, its dits and dahs each spread ±15%, gaps inside letters one dit ±20%, letter gaps 2 to 3.5 dits and word gaps 4 to 6, at {Fixtures.CwFistBank.ToneHz:0} Hz drifting {Fixtures.CwFistBank.DriftHz:0} Hz, over the receiver fixtures' 520 Hz band. Each cell is one fist's letters right of 60. Invented is a letter over none of the fist's own key-downs."));
        sb.AppendLine();
        sb.AppendLine(string.Create(Inv, $"**The bank's total: {fists.Sum(r => r.Right ?? 0)} of {fists.Sum(r => r.OutOf ?? 0)}**, {fists.Sum(r => r.Wrong ?? 0)} wrong, {fists.Sum(r => r.Invented ?? 0)} invented, {fists.Sum(r => r.Silence ?? 0)} in silence, score **{fists.Sum(r => r.Score ?? 0)}**."));

        void Matrix(string title, IReadOnlyList<double> rows, Func<double, string> rowName, IReadOnlyList<(string Name, Func<double, int?> Right)> columns)
        {
            sb.AppendLine();
            sb.AppendLine(title);
            sb.AppendLine();
            sb.AppendLine("| | " + string.Join(" | ", columns.Select(c => c.Name)) + " | row |");
            sb.AppendLine("|---|" + string.Concat(columns.Select(_ => "---|")) + "---|");

            foreach (var row in rows)
            {
                var cells = columns.Select(c => c.Right(row)).ToList();

                sb.AppendLine($"| {rowName(row)} | " + string.Join(" | ", cells.Select(Cell)) + $" | {cells.Sum(c => c ?? 0)} |");
            }
        }

        var ratios = Fixtures.CwFistBank.Ratios;

        Matrix("**Dah over dit by speed**, flat, 20 dB:", ratios, r => string.Create(Inv, $"dah {r:0.0}"),
            Fixtures.CwFistBank.Speeds.Select(w => (string.Create(Inv, $"{w:0} WPM"), (Func<double, int?>)(r => Right(r, w, 20, false)))).ToList());
        Matrix("**Dah over dit by signal**, flat, 20 WPM:", ratios, r => string.Create(Inv, $"dah {r:0.0}"),
            Fixtures.CwFistBank.Signals.Select(d => (string.Create(Inv, $"{d:0} dB"), (Func<double, int?>)(r => Right(r, 20, d, false)))).ToList());
        Matrix("**Signal by fade**, dah 2.5, 20 WPM:", Fixtures.CwFistBank.Signals, d => string.Create(Inv, $"{d:0} dB"),
            [("flat", d => Right(2.5, 20, d, false)), ("12 dB of QSB at 0.7 Hz", d => Right(2.5, 20, d, true))]);

        sb.AppendLine();
        sb.AppendLine("**The worst ten**, by letters right, then by score:");
        sb.AppendLine();

        foreach (var r in fists.OrderBy(r => r.Right ?? 0).ThenBy(r => r.Score ?? 0).ThenBy(r => r.File, StringComparer.Ordinal).Take(10))
        {
            sb.AppendLine(string.Create(Inv, $"- `{r.File}`: {r.Right} of {r.OutOf}, {r.Wrong} wrong, {r.Invented} invented; {bank[r.File].Recipe}; read `{Cut(r.Read, r.Seconds)}`"));
        }
    }

    // A file with a whole text sent: read, scored by the boards' yardstick, invented and silences on the board's keying map.
    private static Row Truthful(string group, string wav, string truth, string source, double pitch, double width)
    {
        var audio = WavAudio.Read(wav);
        var (letters, _) = TheRecordingsScoreboardTests.ReadLive(audio.Samples, audio.SampleRate, pitch, width);
        var map = TheKeyingMapTests.Read(Path.GetFileNameWithoutExtension(wav), audio.Samples, audio.SampleRate);
        var invented = letters.Where(l => !map.Keyed(l.PitchHz, l.From, l.Seconds)).ToList();
        var printed = TheRecordingsScoreboardTests.Text(letters);
        var reference = Prosigns(truth);
        var wrong = TheRecordingsScoreboardTests.WrongAt(printed, reference).Count(i => i < letters.Count && !invented.Contains(letters[i]));
        var spaces = TheRecordingsScoreboardTests.SpacesOf(printed, reference);

        return new Row(
            group, Path.GetFileNameWithoutExtension(wav), audio.Samples.Length / (double)audio.SampleRate, Wpm(letters), Pitch(letters), Senders(letters),
            source, reference, TheRecordingsScoreboardTests.Right(printed, reference), TheRecordingsScoreboardTests.Letters(reference).Length, wrong, invented.Count,
            letters.Count(l => map.SilenceAround(l.From, l.Seconds) is not null), $"{spaces.Right} of {spaces.OfReference}, {spaces.Added} added", printed);
    }

    // A file whose truth is a callsign it holds, not its whole text: read, and whether the callsign reads.
    private static Row Partial(string group, string wav, string name, string? contains, string source)
    {
        var (pitch, width) = State(name);
        var audio = WavAudio.Read(wav);
        var (letters, _) = TheRecordingsScoreboardTests.ReadLive(audio.Samples, audio.SampleRate, pitch, width);
        var printed = TheRecordingsScoreboardTests.Text(letters);
        var found = contains is null ? null : printed.Replace(" ", string.Empty, StringComparison.Ordinal).Contains(contains, StringComparison.Ordinal) ? $"`{contains}` reads" : $"`{contains}` does not read";

        return new Row(
            group, name, audio.Samples.Length / (double)audio.SampleRate, Wpm(letters), Pitch(letters), Senders(letters),
            source + (found is null ? string.Empty : $"; {found}"), contains, null, null, null, null, null, "-", printed, OldRead(wav),
            Stray(printed, audio.Samples.Length / (double)audio.SampleRate), null, InThis(wav));
    }

    // A file nobody knows the text of: the read, the old read beside it, and the stray-letter figure for each.
    private static Row NoTruth(string group, string wav, string? sheetName, string? old)
    {
        var (pitch, width) = sheetName is null ? (600.0, 500.0) : State(sheetName);
        var audio = WavAudio.Read(wav);
        var seconds = audio.Samples.Length / (double)audio.SampleRate;
        var (letters, _) = TheRecordingsScoreboardTests.ReadLive(audio.Samples, audio.SampleRate, pitch, width);
        var printed = TheRecordingsScoreboardTests.Text(letters);

        return new Row(
            group, Path.GetFileNameWithoutExtension(wav), seconds, Wpm(letters), Pitch(letters), Senders(letters),
            old is null ? "none; no earlier read beside it" : "none; the earlier read beside it is scored by nothing", null,
            null, null, null, null, null, "-", printed, old, Stray(printed, seconds), sheetName is null && old is not null ? Stray(old, seconds) : null, sheetName is null ? null : InThis(wav));
    }

    // The radio's pitch and filter from a sheet, or the boards' 600 and 500 where there is no sheet.
    private static (double PitchHz, double WidthHz) State(string name)
    {
        try
        {
            return TheRecordingsScoreboardTests.RadioState(name);
        }
        catch (Exception e) when (e is FileNotFoundException or InvalidOperationException or FormatException)
        {
            return (600, 500);
        }
    }

    private static string SheetText(string wav)
        => File.ReadLines(Path.ChangeExtension(wav, ".txt")).First(l => l.StartsWith("text ", StringComparison.Ordinal))[5..].Trim();

    // The old decoder's read in the sheet beside it, or null where there is no sheet or no read in it.
    private static string? OldRead(string wav)
    {
        var sheet = Path.ChangeExtension(wav, ".txt");

        return File.Exists(sheet) && File.ReadLines(sheet).FirstOrDefault(l => l.StartsWith("text ", StringComparison.Ordinal)) is { } line ? line[5..].Trim() : null;
    }

    // What the earlier decoder emitted in this file alone, from its sheet's `inThis` line, or null.
    private static string? InThis(string wav)
    {
        var sheet = Path.ChangeExtension(wav, ".txt");

        return File.Exists(sheet) && File.ReadLines(sheet).FirstOrDefault(l => l.StartsWith("inThis ", StringComparison.Ordinal)) is { } line
            ? line[7..].Split("  (", 2)[0].Trim()
            : null;
    }

    // What Hamlet printed when the catch was made, from its own `.json`.
    private static string? CatchRead(string wav)
    {
        var json = Path.ChangeExtension(wav, ".json");

        if (!File.Exists(json))
        {
            return null;
        }

        using var doc = System.Text.Json.JsonDocument.Parse(File.ReadAllText(json));

        return doc.RootElement.TryGetProperty("text", out var t) ? t.GetString() : null;
    }

    private static string Prosigns(string text) => Regex.Replace(text, @"\^([A-Z]{2})", "<$1>");

    private static double Seconds(string wav)
    {
        var audio = WavAudio.Read(wav);

        return audio.Samples.Length / (double)audio.SampleRate;
    }

    // Lone one- and two-element letters printed as words, per ten seconds.
    private static double Stray(string? text, double seconds)
        => text is null || seconds <= 0 ? double.NaN : text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Count(Strays.Contains) * 10 / seconds;

    private static double Wpm(IReadOnlyList<Printed> letters)
    {
        var dits = letters.Select(l => l.Lines?.GapDitSeconds ?? double.NaN).Where(d => double.IsFinite(d) && d > 0).Order().ToList();

        return dits.Count == 0 ? double.NaN : 1.2 / dits[dits.Count / 2];
    }

    // The pitch of the sender most printed.
    private static double Pitch(IReadOnlyList<Printed> letters)
        => letters.Count == 0 ? double.NaN : letters.GroupBy(l => Math.Round(l.PitchHz / 25)).OrderByDescending(g => g.Count()).First().Average(l => l.PitchHz);

    // Senders printed: pitches at least 30 Hz apart that printed three letters or more.
    private static int Senders(IReadOnlyList<Printed> letters)
    {
        var clusters = new List<(double Hz, int Count)>();

        foreach (var l in letters.OrderBy(l => l.PitchHz))
        {
            if (clusters.Count > 0 && l.PitchHz - clusters[^1].Hz <= 30)
            {
                clusters[^1] = (l.PitchHz, clusters[^1].Count + 1);
            }
            else
            {
                clusters.Add((l.PitchHz, 1));
            }
        }

        return clusters.Count(c => c.Count >= 3);
    }

    private static string N(double v, string format) => double.IsFinite(v) ? v.ToString(format, Inv) : "-";

    private static string Cut(string? s, double seconds) => s is null ? "-" : (seconds < 60 || s.Length <= 200 ? s : s[..200] + " ...").Replace("|", "\\|", StringComparison.Ordinal);

    private static string Document(List<Row> rows, Dictionary<string, int> count, TheRecordingsScoreboardTests.Board board, TheW1awTableTests.W1awBoard w1aw, List<string> failed)
    {
        var order = new[]
        {
            "synthetic fixtures", "receiver fixtures: easy", "receiver fixtures: working", "receiver fixtures: edge", "receiver fixtures: ungraded",
            "synthetic CQs: 15 dB", "synthetic CQs: 5 dB", "synthetic CQs: 0 dB", "August adjudicated", "the boards: twelve-plus-three",
            "W1AW pieces", "captured, elsewhere", "unadjudicated", "scan catches", FistGroup,
        };
        var sb = new StringBuilder();

        sb.AppendLine("# The CW survey, 2026-10-09");
        sb.AppendLine();
        sb.AppendLine("Work instruction 567, HM-DEC-271. The owner: *\"Let's run a unit that tests against every WAV file we have in our library. This is a one-time exception, so we can kind of measure against existing standards.\"* Every CW `.wav` under `tests\\fixtures\\cw\\` read once through the app's chain at HEAD and scored by the boards' yardstick where the tree holds what was sent; **none is promoted to a board**.");
        sb.AppendLine();
        sb.AppendLine("Run again with: `timeout 1800 dotnet test tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj --filter \"FullyQualifiedName~TheCwSurveyTests.EveryRecordingInTheTree\"` (it writes this file).");
        sb.AppendLine();
        sb.AppendLine("Rerun: 2026-10-22, as shipped at work instruction 587 (HM-DEC-291): a mark is a column, a rise, a flat and a drop, built in three rounds behind Columns, proved on single elements, measured, left off; both boards as before.");
        sb.AppendLine();
        sb.AppendLine("**The yardstick.** Score is letters right less wrong less invented, as on the boards. Invented is a letter over no keying on the board's keying map of the same audio; silence is a letter printed inside a silence of two seconds or more on that map. Where a file carries no truth, its read stands beside the earlier read in its sheet or its `.json`, scored by nothing, with the stray-letter figure: lone `E T I A N M` printed as words, per ten seconds.");
        sb.AppendLine();
        sb.AppendLine("**The boards' rows are the boards' own scoring**, read by `TheRecordingsScoreboardTests.Score` and `TheW1awTableTests.Score`, so their figures are the boards' exactly: twelve-plus-three " +
            string.Create(Inv, $"**{board.Score}** ({board.Total} of {board.OutOf} right, {board.Wrong} wrong, {board.Invented} invented), w1aw **{w1aw.Score}** ({w1aw.Right} of {w1aw.OutOf}, {w1aw.Wrong} wrong, {w1aw.Invented} invented)."));
        sb.AppendLine();
        sb.AppendLine("**The files, by folder:** " + string.Join(", ", count.OrderBy(c => c.Key).Select(c => $"`{c.Key}` {c.Value}")) + string.Create(Inv, $"; {count.Values.Sum()} in all. No CW `.wav` is under `assets\\fixtures\\`; its PSK31, Olivia and FT8 files are not read."));
        sb.AppendLine();
        sb.AppendLine("## Summary");
        sb.AppendLine();
        sb.AppendLine("| group | files | with a truth | letters right of sent | wrong | invented | score | worst three by score |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|");

        foreach (var g in order)
        {
            var mine = rows.Where(r => r.Group == g).ToList();

            if (mine.Count == 0)
            {
                continue;
            }

            var scored = mine.Where(r => r.Right is not null).ToList();
            var right = scored.Sum(r => r.Right!.Value);
            var of = scored.Sum(r => r.OutOf!.Value);
            var worst = string.Join(", ", scored.OrderBy(r => r.Score).ThenBy(r => r.File, StringComparer.Ordinal).Take(3).Select(r => $"`{r.File}` {r.Score}"));
            var score = g.StartsWith("the boards", StringComparison.Ordinal) ? board.Score : g == "W1AW pieces" ? w1aw.Score : scored.Sum(r => r.Score!.Value);

            sb.AppendLine(scored.Count == 0
                ? $"| {g} | {mine.Count} | 0 | - | - | - | - | - |"
                : string.Create(Inv, $"| {g} | {mine.Count} | {scored.Count} | {right} of {of} ({100.0 * right / Math.Max(1, of):0}%) | {scored.Sum(r => r.Wrong ?? 0)} | {(g.StartsWith("the boards", StringComparison.Ordinal) ? board.Invented : scored.Sum(r => r.Invented ?? 0))} | **{score}** | {worst} |"));
        }

        sb.AppendLine();
        sb.AppendLine("For the twelve-plus-three, right, wrong and spaces are over its stretches of medium confidence or better, and invented over every recording, as the board counts them; the low stretches are read and shown and not counted.");

        // **THE STRAYS BAR, RESTATED** (work instruction 572, HM-DEC-276): no capture above the owner's line of six lone one- or
        // two-element words per 10 s; the summed figure over the old captures is reported beside the board, not a condition.
        // The trace recordings are read in the group and kept out of the bar's summed figure, which is over the old captures (work instruction 574).
        var old = rows.Where(r => r.Group == "unadjudicated" && !Traces.Contains(r.File) && r.Stray is { } s && double.IsFinite(s)).ToList();
        var traces = rows.Where(r => Traces.Contains(r.File) && r.Stray is { } s && double.IsFinite(s)).ToList();

        if (old.Count > 0)
        {
            var highest = old.OrderByDescending(r => r.Stray!.Value).First();

            sb.AppendLine();
            sb.AppendLine(string.Create(Inv, $"**The strays bar** (HM-DEC-276): no capture above six lone one- or two-element words per 10 s. Highest now `{highest.File}` at **{highest.Stray!.Value:0.0}**, {(highest.Stray!.Value <= StraysLine ? "under the line" : "over the line")}; summed over the {old.Count} old captures, each as its row shows it, **{old.Sum(r => Math.Round(r.Stray!.Value, 1, MidpointRounding.AwayFromZero)):0.0}**, reported and not a condition.{string.Concat(traces.Select(t => string.Create(Inv, $" The trace recording `{t.File}`, not among them, {t.Stray!.Value:0.0}.")))}"));
        }

        FistBank(sb, rows.Where(r => r.Group == FistGroup).ToList());

        foreach (var g in order)
        {
            var mine = rows.Where(r => r.Group == g).OrderBy(r => r.File, StringComparer.Ordinal).ToList();

            if (mine.Count == 0)
            {
                continue;
            }

            sb.AppendLine();
            sb.AppendLine($"## {g}");
            sb.AppendLine();

            if (mine.Any(r => r.Right is not null))
            {
                sb.AppendLine("| file | seconds | WPM | pitch | senders | truth from | right | wrong | invented | in silence | score | spaces | read | truth |");
                sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|---|---|---|---|");

                foreach (var r in mine)
                {
                    sb.AppendLine(string.Create(Inv, $"| `{r.File}` | {r.Seconds:0} | {N(r.Wpm, "0")} | {N(r.PitchHz, "0")} | {r.Senders} | {r.TruthSource} | {r.Right} of {r.OutOf} | {r.Wrong} | {r.Invented} | {(r.Silence is { } s ? s.ToString(Inv) : "-")} | **{r.Score}** | {r.Spaces} | `{Cut(r.Read, r.Seconds)}` | `{Cut(r.Truth, r.Seconds)}` |"));
                }
            }
            else
            {
                sb.AppendLine(g == "scan catches"
                    ? "The earlier read is the catch's own `.json`, what Hamlet printed of this catch when it was made, so its stray figure is this file's."
                    : "**The earlier read covers more than this file.** A sheet's `text` is everything the decoder of the day read since its transcript was last cleared (its `textCovers` line), often minutes, so its length and its strays are not this file's; `earlier, in this file` is the sheet's own count for these 30 seconds, from its `inThis` line.");
                sb.AppendLine();
                sb.AppendLine("| file | seconds | WPM | pitch | senders | truth | strays per 10 s, now | letters now | earlier, in this file | read now | earlier read |");
                sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|---|");

                foreach (var r in mine)
                {
                    var earlier = r.OldStray is { } os ? string.Create(Inv, $"{os:0.0} strays per 10 s") : r.OldInThis ?? "-";

                    sb.AppendLine(string.Create(Inv, $"| `{r.File}` | {r.Seconds:0} | {N(r.Wpm, "0")} | {N(r.PitchHz, "0")} | {r.Senders} | {r.TruthSource} | {N(r.Stray ?? double.NaN, "0.0")} | {r.Read.Count(char.IsLetterOrDigit)} | {earlier} | `{Cut(r.Read, r.Seconds)}` | `{Cut(r.Old, r.Seconds)}` |"));
                }
            }
        }

        sb.AppendLine();
        sb.AppendLine("## What did not read");
        sb.AppendLine();
        sb.AppendLine(failed.Count == 0 ? "Every file read." : string.Join(Environment.NewLine, failed.Order(StringComparer.Ordinal).Select(f => "- " + f)));

        return sb.ToString();
    }
}
