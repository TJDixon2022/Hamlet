using System.Reflection;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Cw;

/// <summary>
/// **THE CAPTURE SHEET READS THE PATH THAT REACHES THE SCREEN** (work instruction 537, HM-DEC-241): its pitch, speed,
/// counts, senders and duty come from the shape side's reading at the press, and the old decoder's lines are gone.
/// </summary>
/// <remarks>
/// The sheet is composed through the press's own writer, <c>CaptureNotes</c>, from a reading built here: a printed sender
/// at 601 Hz with a 60 ms dit, a second sender beside it, and letters and marks inside and outside the recording's span.
/// Synthetic audio; no recording is read.
/// </remarks>
public sealed class TheSheetReadsTheShapeSideTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the sheet is printed.</param>
    public TheSheetReadsTheShapeSideTests(ITestOutputHelper output) => _output = output;

    private static string Line(string sheet, string label)
        => sheet.Split(Environment.NewLine).Single(l => l.StartsWith(label, StringComparison.Ordinal));

    private string Sheet(CwShapeSideReading shape)
    {
        var audio = CwSignal.Generate(new CwSignalRequest(
            "CQ CQ DE N0CALL K", WordsPerMinute: 20, ToneHz: 601, SampleRate: 8000, Amplitude: 0.3,
            NoiseAmplitude: 0.04, LeadInSeconds: 2, TailSeconds: 2, Seed: 5371));
        var model = new MainWindowViewModel(new AppSettings(), null);
        var writer = typeof(MainWindowViewModel).GetMethod("CaptureNotes", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(writer);

        var sheet = (string)writer!.Invoke(
            model, [audio, (long)audio.Samples.Length, CwDecodeReport.None, shape, MainWindowViewModel.TonePeakRecordLine(audio, shape)])!;

        _output.WriteLine(sheet);

        return sheet;
    }

    /// <summary>Every re-sourced line says what the shape side held, and no old decoder line is left.</summary>
    [Fact]
    public void EachLineComesFromTheShapeSide()
    {
        var shape = new CwShapeSideReading(
            HeardSeconds: 100,
            PrintedPitchHz: 601.3,
            PrintedDitSeconds: 0.06,
            LettersPrinted: 40,
            LettersUnreadable: 2,
            MarksStood: 120,
            LetterTimes: [10, 20, 80, 90, 95],
            MarkTimes: [10, 15, 85, 88, 92, 96, 99],
            UnreadableTimes: [95],
            Senders:
            [
                new CwSenderStanding(498.4, 0.31, 14, false),
                new CwSenderStanding(601.3, 0.62, 60, true),
            ]);

        var sheet = Sheet(shape);

        Assert.Equal("pitch      601 Hz  (the printed sender's, from its own marks)", Line(sheet, "pitch "));
        Assert.Equal("speed      20 WPM  (the printed sender's dit, 60 ms)", Line(sheet, "speed "));
        Assert.StartsWith("inThis     ", Line(sheet, "inThis "), StringComparison.Ordinal);
        Assert.Equal("senders    601 Hz shape 0.62, 60 marks, printed; 498 Hz shape 0.31, 14 marks", Line(sheet, "senders "));
        Assert.Contains("at 601 Hz)", Line(sheet, "duty "), StringComparison.Ordinal);
        Assert.Contains("printed sender's tone at 601 Hz", Line(sheet, "tonePeak "), StringComparison.Ordinal);
        Assert.StartsWith("sinceLast  40 letters printed, 120 marks stood", Line(sheet, "sinceLast "), StringComparison.Ordinal);

        foreach (var gone in new[] { "toneHz ", "heldPeak ", "unkeyed ", "elements ", "characters ", "decoderWpm ", "spanLlr ", "arbiter ", "competing ", "reading ", "elementHz " })
        {
            Assert.DoesNotContain(sheet.Split(Environment.NewLine), l => l.StartsWith(gone, StringComparison.Ordinal));
        }
    }

    /// <summary>With nobody printed the pitch, speed, tone and duty say so.</summary>
    [Fact]
    public void NobodyPrintedSaysSo()
    {
        var sheet = Sheet(CwShapeSideReading.Nothing);

        Assert.Equal("pitch      nobody printed", Line(sheet, "pitch "));
        Assert.Equal("speed      nobody printed", Line(sheet, "speed "));
        Assert.Equal("senders    none held", Line(sheet, "senders "));
        Assert.StartsWith("inThis     nothing heard", Line(sheet, "inThis "), StringComparison.Ordinal);
        Assert.StartsWith("duty       not measured", Line(sheet, "duty "), StringComparison.Ordinal);
    }
}
