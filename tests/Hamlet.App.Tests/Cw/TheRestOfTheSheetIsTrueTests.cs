using System.Globalization;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Cw;

/// <summary>
/// Work instruction 418 task 2: every other sentence task 1 found false about a
/// signal, each on a saved capture, through the sheet writer's own code.
/// </summary>
/// <remarks>
/// <para>**ONE FACT PER SENTENCE**, each red before its fix. The table they come
/// from is `.run-unit/unit418-table.md`.</para>
/// <para>**WHERE A BRANCH NEEDS STATE A REPLAY CANNOT MAKE, THE SAVED SHEET
/// SUPPLIES IT AND SAYS SO.** `cw-2026-08-28-004844` was written 39 seconds after a
/// clear, so the clear is set that far back. `cw-2026-08-28-005051` said 252
/// characters with no pitch measured, 29 of them in that file; a decoder replaying
/// the file alone leaves the pitch unmeasured as the evening did, and the report
/// carries the saved sheet's own count, because the sentence is about a count that
/// runs back past the file.</para>
/// </remarks>
public sealed class TheRestOfTheSheetIsTrueTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the lines are printed.</param>
    public TheRestOfTheSheetIsTrueTests(ITestOutputHelper output) => _output = output;

    /// <summary>`inputFloor` says it is the meter's running floor (013347).</summary>
    [Fact]
    public void TheInputFloorSaysItIsTheMetersRunningFloor()
    {
        var line = Line(Sheet("cw-2026-08-17-013347"), "inputFloor ");

        Assert.Contains("the level meter's running floor at the moment it was kept", line, StringComparison.Ordinal);
        Assert.Contains("not a figure about this recording alone", line, StringComparison.Ordinal);
    }

    /// <summary>`clipping` names the stretch it covers (013347).</summary>
    [Fact]
    public void TheClippingLineNamesTheStretchItCovers()
    {
        var line = Line(Sheet("cw-2026-08-17-013347"), "clipping ");

        Assert.Contains("the level meter last measured when it was kept", line, StringComparison.Ordinal);
        Assert.Contains("not the whole recording", line, StringComparison.Ordinal);
    }

    /// <summary>A measured `toneHz` says it is a bin centre (17:37).</summary>
    [Fact]
    public void TheMeasuredPitchSaysItIsTheBinItWasAdmittedIn()
    {
        var line = Line(Sheet("cw-2026-09-23-173723"), "toneHz ");

        Assert.Contains("the centre of the survey bin it was admitted in", line, StringComparison.Ordinal);
        Assert.DoesNotContain(", interpolated between bins", line, StringComparison.Ordinal);
    }

    /// <summary>`unkeyed` does not credit the whole count to today's pitch (005051, 17:37).</summary>
    [Fact]
    public void TheUnkeyedLineSaysTheCountIsNotAllFromThisPitch()
    {
        var yes = Line(
            Sheet("cw-2026-08-28-005051", r => r with { CharactersEmitted = 252, PitchWasMeasured = false }),
            "unkeyed ");
        var no = Line(Sheet("cw-2026-09-23-173723"), "unkeyed ");

        Assert.DoesNotContain("characters reached the screen from a pitch", yes, StringComparison.Ordinal);
        Assert.Contains("the pitch being followed now was chosen by", yes, StringComparison.Ordinal);
        Assert.Contains("from whatever pitch was being followed at the time", yes, StringComparison.Ordinal);
        Assert.Contains("the pitch being followed now is one the survey admitted keying at", no, StringComparison.Ordinal);
        Assert.Contains("from whatever pitch was being followed at the time", no, StringComparison.Ordinal);
    }

    /// <summary>After a clear, `elements` still says the decoder's span (004844).</summary>
    [Fact]
    public void TheElementsCountSaysItRunsFromTheDecodersStartAfterAClear()
    {
        var sheet = Cleared();

        Assert.Contains("since the decoder started listening", Line(sheet, "elements "), StringComparison.Ordinal);
        Assert.DoesNotContain("cleared", Line(sheet, "elements "), StringComparison.Ordinal);
    }

    /// <summary>After a clear, `characters` still says the decoder's span (004844).</summary>
    [Fact]
    public void TheCharactersCountSaysItRunsFromTheDecodersStartAfterAClear()
    {
        var sheet = Cleared();

        Assert.Contains("since the decoder started listening", Line(sheet, "characters "), StringComparison.Ordinal);
        Assert.DoesNotContain("cleared", Line(sheet, "characters "), StringComparison.Ordinal);
    }

    /// <summary>After a clear, the first `sinceLast` says the decoder's span (004844).</summary>
    [Fact]
    public void TheFirstSinceLastSaysItRunsFromTheDecodersStartAfterAClear()
    {
        var sheet = Cleared();
        var line = Line(sheet, "sinceLast ");

        Assert.Contains("since the decoder started listening", line, StringComparison.Ordinal);
        Assert.DoesNotContain("cleared", line, StringComparison.Ordinal);

        // The transcript's own cover is the one that moves with a clear, and stays.
        Assert.Contains("since the transcript was cleared", Line(sheet, "textCovers"), StringComparison.Ordinal);
    }

    /// <summary>`competing` does not call a present fraction keyed (014113).</summary>
    [Fact]
    public void TheCompetingLineDoesNotCallAnUnkeyedToneKeyed()
    {
        var line = Line(Sheet("cw-2026-08-22-014113"), "competing ");

        Assert.DoesNotContain("keyed ", line, StringComparison.Ordinal);
        Assert.DoesNotContain("the loudest thing in the band", line, StringComparison.Ordinal);
        Assert.Contains("over the band floor and above it", line, StringComparison.Ordinal);
    }

    /// <summary>`reading` prints the gate as it is (17:37).</summary>
    [Fact]
    public void TheReadingLinePrintsTheGateAsItIs()
    {
        var line = Line(Sheet("cw-2026-09-23-173723"), "reading ");

        Assert.Contains(
            string.Format(CultureInfo.InvariantCulture, "against a gate of {0:0.00}", CwProbabilisticDecoder.Gate),
            line,
            StringComparison.Ordinal);
    }

    /// <summary>`reading` says when it was read (17:37).</summary>
    [Fact]
    public void TheReadingLineSaysWhenItWasRead()
    {
        var line = Line(Sheet("cw-2026-09-23-173723"), "reading ");

        Assert.DoesNotContain("at the moment of the press", line, StringComparison.Ordinal);
        Assert.Contains("as it stood when this sheet was written", line, StringComparison.Ordinal);
    }

    private string Cleared() => Sheet("cw-2026-08-28-004844", clearedAgo: TimeSpan.FromSeconds(39));

    private string Sheet(
        string stamp,
        Func<CwDecodeReport, CwDecodeReport>? adjust = null,
        TimeSpan? clearedAgo = null)
    {
        var wav = Directory
            .GetFiles(
                Path.Combine(EverySentenceOnTheSheetTests.Root(), "tests", "fixtures", "cw", "captured"),
                stamp + ".wav",
                SearchOption.AllDirectories)
            .Single();
        var audio = WavAudio.Read(wav);

        var decoder = new CwDecoder(audio.SampleRate);

        using (var source = new BufferedAudioSource(audio))
        {
            decoder.Listen(source);
            source.PumpAll();
            decoder.Flush();
        }

        var report = adjust is null ? decoder.Report : adjust(decoder.Report);

        var sheet = EverySentenceOnTheSheetTests.Sheet(
            decoder,
            EverySentenceOnTheSheetTests.Meter(audio).Reading,
            audio,
            report,
            MainWindowViewModel.TonePeakRecordLine(audio, report),
            clearedAgo is { } ago ? DateTime.UtcNow - ago : null);

        _output.WriteLine($"== {stamp}");
        _output.WriteLine(sheet);

        return sheet;
    }

    private static string Line(string sheet, string label)
        => sheet.Split(Environment.NewLine).Single(l => l.StartsWith(label, StringComparison.Ordinal));
}
