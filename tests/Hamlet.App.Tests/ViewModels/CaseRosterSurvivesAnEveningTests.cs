using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// The press keeps the audio and marks the case, driven from a real capture.
/// </summary>
/// <remarks>
/// <para>**A CONTROL THAT WRITES FILES FAILS SILENTLY.** No exception, no file,
/// and the operator finds out at eleven at night with a station coming through.
/// This is the task that protects the evening, so it runs the fixture the project
/// already trusts through the same replay source the application uses and looks
/// for what should be on disk afterwards.</para>
/// <para>**WHAT THIS PROVES AND WHAT IT DOES NOT.** It drives the decoder, the
/// tap, the WAV writer and the roster on real off-air audio, and it exercises the
/// freshness rule with the same `SamplesSeen` comparison the command makes. It
/// does **not** drive `MainWindowViewModel.CaptureAudioAsync` itself, because the
/// decoder there is fed by `OpenAudioInput()` and there is no seam to hand it a
/// file — wiring one is a change to the decode start path and this unit does not
/// touch decoder behaviour. That gap is stated in the report rather than papered
/// over.</para>
/// <para>Everything is written to a temporary folder. The operator's own captures
/// are not a test's to write into and neither is the adjudicated fixture set.</para>
/// </remarks>
public sealed class CaseRosterSurvivesAnEveningTests : IDisposable
{
    /// <summary>One named cell of a split row.</summary>
    /// <remarks>
    /// **BY NAME AND NOT BY POSITION.** These assertions were written against
    /// literal indexes, so four of them broke the first time a column was added
    /// between two others and none of them was about the column that moved. The
    /// header is where the order lives, so the header is what is asked (§0).
    /// </remarks>
    private static string Cell(string[] columns, string column)
        => columns[Array.IndexOf(CwCaseRoster.Header.Split('	'), column)];

    /// <summary>One named cell of a row.</summary>
    private static string Cell(CwCase one, string column)
        => Cell(CwCaseRoster.Row(one).Split('	'), column);

    private readonly ITestOutputHelper _output;

    private readonly string _folder = Path.Combine(
        Path.GetTempPath(), "hamlet-cases", Guid.NewGuid().ToString("N"));

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the roster is printed.</param>
    public CaseRosterSurvivesAnEveningTests(ITestOutputHelper output)
        => _output = output;

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_folder))
            {
                Directory.Delete(_folder, recursive: true);
            }
        }
        catch (IOException)
        {
            // A leftover temp folder is not a test failure.
        }
    }

    private static string Fixture()
    {
        var here = new DirectoryInfo(AppContext.BaseDirectory);

        while (here is not null
               && !Directory.Exists(Path.Combine(here.FullName, "tests", "fixtures")))
        {
            here = here.Parent;
        }

        Assert.NotNull(here);

        return Path.Combine(
            here!.FullName, "tests", "fixtures", "cw", "captured",
            "cw-2026-08-18-004507.wav");
    }

    /// <remarks>
    /// **THE EVENING, END TO END.** A real capture is played through the decoder,
    /// the press keeps what the tap heard, and the roster gains a row carrying the
    /// same numbers the decoder had at that moment. Then a second press with no
    /// new audio writes no recording and still marks the case, with the reason on
    /// the row.
    /// </remarks>
    [Fact]
    public void OnePressKeepsTheAudioAndMarksTheCase()
    {
        var audio = WavAudio.Read(Fixture());
        var decoder = new CwDecoder(audio.SampleRate, 600);

        var settled = new List<CwCharacter>();
        decoder.CharacterSettled += settled.Add;

        using (var source = new BufferedAudioSource(audio))
        {
            decoder.Listen(source);
            source.PumpAll();
            decoder.Flush();
        }

        var tap = decoder.Tap;
        var kept = tap.Snapshot();

        Assert.NotNull(kept);

        // ---- the first press ------------------------------------------------
        Directory.CreateDirectory(_folder);

        var stamp = new DateTime(2026, 8, 19, 23, 14, 5, DateTimeKind.Utc);
        var wav = Path.Combine(_folder, $"cw-{stamp:yyyy-MM-dd-HHmmss}.wav");

        WavAudio.Write(wav, kept!);

        var report = decoder.Report;
        var seen = tap.SamplesSeen;

        // What the operator was looking at when he decided there was a station
        // there. The application takes `Transcript.Tail(120)`; here the same
        // characters come from the decode that just ran.
        var read = string.Concat(settled.Select(c => c.Text)).Trim();

        var path = CwCaseRoster.Append(
            _folder,
            new CwCase(
                stamp, 7_030_000, "40 m", Path.GetFileName(wav), "",
                report.HasTone ? report.ToneHz : null,
                double.IsNaN(report.SnrDb) ? null : report.SnrDb,
                decoder.State.WordsPerMinute > 0 ? decoder.State.WordsPerMinute : null,
                report.CharactersEmitted,
                report.CharactersUnsure,
                read,

                // **THIS PRESS KEPT A RECORDING, SO ITS COUNTS ARE ABOUT THE
                // RECORDING** (HM-DEC-091). The decoder was fed the fixture and
                // nothing else, so its totals and the recording's figures are the
                // same numbers here; on the air they are not, which is the whole
                // reason the cell now says which it is.
                CwCountsCover.Recording));

        Assert.True(File.Exists(wav), "the recording was not written");
        Assert.True(File.Exists(path), "the roster was not written");

        // ---- the second press, with nothing new ------------------------------
        //
        // The command refuses to write when `SamplesSeen` has not moved
        // (HM-DEC-090), and the case is still marked. Nothing here weakens that
        // guard: the same comparison is made and the same answer taken.
        Assert.Equal(seen, tap.SamplesSeen);

        CwCaseRoster.Append(
            _folder,
            new CwCase(
                stamp.AddSeconds(20), 7_030_000, "40 m", "",
                "no new audio since the last one",
                report.HasTone ? report.ToneHz : null,
                double.IsNaN(report.SnrDb) ? null : report.SnrDb,
                decoder.State.WordsPerMinute > 0 ? decoder.State.WordsPerMinute : null,
                report.CharactersEmitted,
                report.CharactersUnsure,
                read,

                // **AND THIS ONE KEPT NOTHING, SO THE COUNTS CANNOT BE ABOUT A
                // RECORDING THERE IS NONE OF.** The row still carries them,
                // because a case with no evidence is still a case, and the cell
                // says what they are counts of instead of passing for an answer.
                CwCountsCover.NoRecording));

        var lines = File.ReadAllLines(path);

        foreach (var line in lines)
        {
            _output.WriteLine(line);
        }

        // The evening line, then the column header, then the two presses. The
        // first line names the local evening and says the times below are UTC; it
        // sits above the header rather than in it because it describes the file
        // rather than a column (HM-DEC-091).
        Assert.Equal(4, lines.Length);
        Assert.StartsWith("# Evening of ", lines[0], StringComparison.Ordinal);
        Assert.Contains("Every time below is UTC", lines[0], StringComparison.Ordinal);
        Assert.Equal(CwCaseRoster.Header, lines[1]);

        var first = lines[2].Split('\t');
        var second = lines[3].Split('\t');

        // The kept recording is named on the row, and the roster agrees with the
        // decoder that produced the sidecar's own numbers (HM-DEC-091: one source).
        Assert.Equal(Path.GetFileName(wav), Cell(first, "wav"));
        Assert.Equal("7.030", Cell(first, "frequency"));
        Assert.Equal("40 m", Cell(first, "band"));
        Assert.Equal(
            $"{report.CharactersEmitted} emitted, {report.CharactersUnsure} unsure",
            Cell(first, "chars"));

        // **AND THE REFUSED PRESS SAYS ITS COUNTS ARE NOT ABOUT A RECORDING**
        // (HM-DEC-091). No file was written, so there is no audio for them to be
        // a count of, and a bare pair of numbers in this column would be read as
        // one anyway.
        Assert.Contains("no recording was kept", Cell(second, "chars"), StringComparison.Ordinal);

        // **THE REFUSAL IS A ROW WITH A REASON, NOT A SILENCE.** A case with no
        // evidence is still a case and belongs in the denominator.
        Assert.StartsWith("none (", Cell(second, "wav"), StringComparison.Ordinal);
        Assert.Contains("no new audio", Cell(second, "wav"), StringComparison.Ordinal);

        // Exactly one recording exists: the second press wrote none.
        Assert.Single(Directory.GetFiles(_folder, "*.wav"));

        // **THE ROW CARRIES WHAT HAMLET READ, NOT ONLY HOW MUCH.** A count is a
        // pointer to evidence; scoring thirty cases from counts alone means
        // opening thirty recordings.
        Assert.Equal(CwCaseRoster.Header.Split('	').Length, first.Length);
        Assert.NotEqual("nothing read", Cell(first, "text"));
        Assert.StartsWith(
            CwCaseRoster.Readable(read), Cell(first, "text"), StringComparison.Ordinal);

        // **AND THE CELL SAYS WHAT INTERVAL IT COVERS**, in the same words the
        // count beside it uses. The transcript is everything read since the
        // decoder started listening, which on a row beside thirty seconds of
        // audio is not a claim about that audio.
        Assert.Contains(
            "the whole session, not this case",
            Cell(first, "text"),
            StringComparison.Ordinal);

        // **AND THE REFUSED PRESS CARRIES IT TOO** (HM-DEC-090). He heard the
        // station whether or not a recording was written, so the row that records
        // the refusal is scored the same way as any other.
        Assert.StartsWith(
            CwCaseRoster.Readable(read), Cell(second, "text"), StringComparison.Ordinal);

        // The operator's column is still last and still empty.
        Assert.Equal(string.Empty, Cell(first, "read"));
        Assert.Equal(string.Empty, Cell(second, "read"));

        // **ONE ROW IS ONE LINE**, or the columns after the text land under the
        // wrong headings and tomorrow's scoring is done against a shifted file.
        foreach (var line in lines.Skip(1))
        {
            Assert.DoesNotContain('\n', line);
            Assert.DoesNotContain('\r', line);
            Assert.Equal(
                CwCaseRoster.Header.Count(c => c == '\t'),
                line.Count(c => c == '\t'));
        }

        // And the evening line carries no tabs at all, so a scorer splitting the
        // file on them sees a note rather than a row of empty columns.
        Assert.DoesNotContain('\t', lines[0]);
    }

    /// <remarks>
    /// **THE COLUMN THE INSTRUMENT MAY NOT FILL IN.** `read` is the operator's
    /// verdict and nothing derives it, defaults it, or guesses it from the
    /// character count. A threshold standing in for a judgement is the error this
    /// project tabulated five times in one week.
    /// </remarks>
    [Fact]
    public void TheReadColumnIsLeftForHim()
    {
        var busy = new CwCase(
            new DateTime(2026, 8, 19, 23, 0, 0, DateTimeKind.Utc),
            7_030_000, "40 m", "cw-2026-08-19-230000.wav", "",
            ToneHz: 640, SnrDb: 28.4, Wpm: 20, Emitted: 44, Unsure: 2);

        var empty = busy with { Emitted = 0, Unsure = 0, ToneHz = null, SnrDb = null, Wpm = null };

        foreach (var one in new[] { busy, empty })
        {
            var columns = CwCaseRoster.Row(one).Split('\t');

            Assert.Equal(CwCaseRoster.Header.Split('	').Length, columns.Length);
            Assert.Equal(string.Empty, Cell(columns, "read"));
        }

        // And a decoder that read nothing says so in its own columns rather than
        // being left blank or given a plausible number (HM-DEC-091).
        var quiet = CwCaseRoster.Row(empty).Split('\t');

        Assert.Equal("none", Cell(quiet, "toneHz"));
        Assert.Equal("unread", Cell(quiet, "tonePeakDb"));
        Assert.Equal("not tracking", Cell(quiet, "wpm"));
    }

    /// <remarks>
    /// **THE MOST IMPORTANT ROW ON THE SHEET.** A station heard and nothing read
    /// is the case the whole measure exists to count, and an empty cell would look
    /// like a column somebody forgot to fill in rather than a decoder that
    /// produced nothing (HM-DEC-091).
    /// </remarks>
    [Fact]
    public void AnEmptyTranscriptSaysSoRatherThanLeavingItBlank()
    {
        var heard = new CwCase(
            new DateTime(2026, 8, 19, 23, 30, 0, DateTimeKind.Utc),
            7_030_000, "40 m", "cw-2026-08-19-233000.wav", "",
            ToneHz: null, SnrDb: null, Wpm: null, Emitted: 0, Unsure: 0,
            Text: "");

        var columns = CwCaseRoster.Row(heard).Split('\t');

        Assert.Equal("nothing read", Cell(columns, "text"));
        Assert.NotEqual(string.Empty, Cell(columns, "text"));
        Assert.Equal(string.Empty, Cell(columns, "read"));
    }

    /// <remarks>
    /// Proves a tab or a newline in the transcript cannot split a row, which would
    /// put the operator's own column under a different heading. The decoder emits
    /// neither today; the file everything is scored from should not depend on that
    /// staying true.
    /// </remarks>
    [Fact]
    public void NothingInTheTextCanBreakTheRow()
    {
        var awkward = new CwCase(
            new DateTime(2026, 8, 19, 23, 40, 0, DateTimeKind.Utc),
            7_030_000, "40 m", "a.wav", "", 640, 28.4, 20, 12, 1,
            Text: "CQ\tDE\r\nW1AW K");

        var row = CwCaseRoster.Row(awkward);

        Assert.DoesNotContain('\n', row);
        Assert.DoesNotContain('\r', row);
        Assert.Equal(
            CwCaseRoster.Header.Count(c => c == '\t'),
            row.Count(c => c == '\t'));
        Assert.Contains("CQ DE", row, StringComparison.Ordinal);
    }

    /// <summary>The shack's own clock, so no test depends on the machine's.</summary>
    /// <remarks>
    /// Pennsylvania in August, fixed rather than looked up: a test that reads the
    /// machine's zone passes here and fails on a build agent in London, and a test
    /// that reads the real Eastern zone would change its answer in November. The
    /// offset is the subject of this test, so it is stated.
    /// </remarks>
    private static readonly TimeZoneInfo Shack = TimeZoneInfo.CreateCustomTimeZone(
        "shack", TimeSpan.FromHours(-4), "shack", "shack");

    /// <remarks>
    /// <para>**ONE EVENING AT THE RIG IS ONE FILE.** He starts around eight and
    /// works past midnight UTC without moving, so a roster named for the UTC date
    /// would put the first part of the evening in one file and everything after
    /// eight o'clock local in another named for tomorrow. Scoring the first file
    /// and taking its count reports a percentage whose denominator lost the second
    /// half of the evening, and nothing on the sheet says the rest exists.</para>
    /// <para>Both presses here are on the same local evening and **straddle
    /// midnight UTC**, which the test states rather than assumes: the later press
    /// is asserted to fall on the following UTC date before the roster is asked
    /// anything.</para>
    /// </remarks>
    [Fact]
    public void AnEveningThatCrossesUtcMidnightIsStillOneFile()
    {
        // Half past seven and half past nine, at the rig.
        var early = new DateTime(2026, 8, 19, 23, 30, 0, DateTimeKind.Utc);
        var late = new DateTime(2026, 8, 20, 1, 30, 0, DateTimeKind.Utc);

        // The crossing is the point, so it is proved rather than trusted: the two
        // presses are two hours apart on one evening and land on different UTC
        // dates.
        Assert.NotEqual(early.Date, late.Date);
        Assert.Equal(TimeSpan.FromHours(2), late - early);

        var first = CwCaseRoster.Append(
            _folder,
            new CwCase(early, 7_030_000, "40 m", "cw-2026-08-19-233000.wav", "",
                505, 42.7, 22, 19, 6, "CQ CQ DE W1AW"),
            Shack);

        var second = CwCaseRoster.Append(
            _folder,
            new CwCase(late, 7_030_000, "40 m", "cw-2026-08-20-013000.wav", "",
                505, 38.1, 22, 4, 0, "K3XYZ"),
            Shack);

        // One file, named for the evening he sat down in rather than for the UTC
        // date the second press happened to fall on.
        Assert.Equal(first, second);
        Assert.Equal("cases-2026-08-19.txt", Path.GetFileName(first));
        Assert.Single(Directory.GetFiles(_folder, "cases-*.txt"));

        var lines = File.ReadAllLines(first);

        foreach (var line in lines)
        {
            _output.WriteLine(line);
        }

        Assert.Equal(4, lines.Length);

        // **AND THE FILE SAYS WHICH CLOCK IS WHICH**, so nobody reading it cold in
        // six months has to work out why a row is stamped after midnight in a file
        // named for the day before (HM-DEC-091).
        Assert.Equal(
            "# Evening of Wednesday 19 August 2026 at the rig, local time UTC-04:00."
            + " Every time below is UTC.",
            lines[0]);

        Assert.Equal(CwCaseRoster.Header, lines[1]);

        // The rows themselves are untouched: still UTC, still in the order they
        // were pressed.
        Assert.StartsWith("23:30:00\t", lines[2], StringComparison.Ordinal);
        Assert.StartsWith("01:30:00\t", lines[3], StringComparison.Ordinal);
        Assert.Contains("CQ CQ DE W1AW", lines[2], StringComparison.Ordinal);
        Assert.Contains("K3XYZ", lines[3], StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the file is per evening and append-only: a second press on the same
    /// night lands in the same file under the same header rather than starting a
    /// new one.
    /// </remarks>
    [Fact]
    public void TheRosterIsOneFilePerEvening()
    {
        var evening = new DateTime(2026, 8, 19, 22, 0, 0, DateTimeKind.Utc);

        var first = CwCaseRoster.Append(
            _folder,
            new CwCase(evening, 7_030_000, "40 m", "a.wav", "", 640, 28.4, 20, 10, 0));

        var later = CwCaseRoster.Append(
            _folder,
            new CwCase(
                evening.AddHours(1), 14_030_000, "20 m", "b.wav", "", 620, 19.1, 18, 4, 1));

        Assert.Equal(first, later);
        Assert.Equal("cases-2026-08-19.txt", Path.GetFileName(first));

        // The evening line, the header, and the two presses.
        Assert.Equal(4, File.ReadAllLines(first).Length);
    }
}
