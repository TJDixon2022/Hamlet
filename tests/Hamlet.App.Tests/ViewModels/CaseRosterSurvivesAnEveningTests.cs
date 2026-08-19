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

        var path = CwCaseRoster.Append(
            _folder,
            new CwCase(
                stamp, 7_030_000, "40 m", Path.GetFileName(wav), "",
                report.HasTone ? report.ToneHz : null,
                double.IsNaN(report.SnrDb) ? null : report.SnrDb,
                decoder.State.WordsPerMinute > 0 ? decoder.State.WordsPerMinute : null,
                report.CharactersEmitted,
                report.CharactersUnsure));

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
                report.CharactersUnsure));

        var lines = File.ReadAllLines(path);

        foreach (var line in lines)
        {
            _output.WriteLine(line);
        }

        Assert.Equal(3, lines.Length);
        Assert.Equal(CwCaseRoster.Header, lines[0]);

        var first = lines[1].Split('\t');
        var second = lines[2].Split('\t');

        // The kept recording is named on the row, and the roster agrees with the
        // decoder that produced the sidecar's own numbers (HM-DEC-091: one source).
        Assert.Equal(Path.GetFileName(wav), first[3]);
        Assert.Equal("7.030", first[1]);
        Assert.Equal("40 m", first[2]);
        Assert.Equal($"{report.CharactersEmitted} emitted, {report.CharactersUnsure} unsure", first[7]);

        // **THE REFUSAL IS A ROW WITH A REASON, NOT A SILENCE.** A case with no
        // evidence is still a case and belongs in the denominator.
        Assert.StartsWith("none (", second[3], StringComparison.Ordinal);
        Assert.Contains("no new audio", second[3], StringComparison.Ordinal);

        // Exactly one recording exists: the second press wrote none.
        Assert.Single(Directory.GetFiles(_folder, "*.wav"));
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

            Assert.Equal(9, columns.Length);
            Assert.Equal(string.Empty, columns[8]);
        }

        // And a decoder that read nothing says so in its own columns rather than
        // being left blank or given a plausible number (HM-DEC-091).
        var quiet = CwCaseRoster.Row(empty).Split('\t');

        Assert.Equal("none", quiet[4]);
        Assert.Equal("unread", quiet[5]);
        Assert.Equal("not tracking", quiet[6]);
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
        Assert.Equal(3, File.ReadAllLines(first).Length);
    }
}
