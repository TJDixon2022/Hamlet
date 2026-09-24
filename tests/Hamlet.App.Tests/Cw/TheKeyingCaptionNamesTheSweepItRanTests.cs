using System.Globalization;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Cw;

/// <summary>
/// Work instruction 418 task 2: the `keying` line's caption says what the meter
/// behind it actually did, on 17:37.
/// </summary>
/// <remarks>
/// <para>**THE CAPTION NAMED A SWEEP NOBODY RAN** (P10). It said 400 to 1200 Hz while
/// <see cref="KeyingEnvelope"/> sweeps the tracker's own 300 to 900, and the saved
/// `cw-2026-08-28-004844` printed `keying at 375 Hz` under it. It said the last six
/// seconds where the reading on the sheet is the one the meter last published, and
/// sharing nothing with the decoder where the meter reads the decoder's own tap on
/// purpose and takes its range from the tracker.</para>
/// <para>**THE CAPTION MOVES AND THE METER DOES NOT** (§6).</para>
/// </remarks>
public sealed class TheKeyingCaptionNamesTheSweepItRanTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the line is printed.</param>
    public TheKeyingCaptionNamesTheSweepItRanTests(ITestOutputHelper output) => _output = output;

    /// <summary>The range and step in the caption are the ones swept.</summary>
    [Fact]
    public void TheCaptionNamesTheRangeAndStepKeyingEnvelopeSweeps()
    {
        var line = KeyingLineFor173723();

        var swept = string.Format(
            CultureInfo.InvariantCulture,
            "sweep of {0:0} to {1:0} Hz in {2:0} Hz steps",
            KeyingEnvelope.LowestToneHz,
            KeyingEnvelope.HighestToneHz,
            KeyingEnvelope.ToneStepHz);

        Assert.Contains(swept, line, StringComparison.Ordinal);
        Assert.DoesNotContain("400 to 1200", line, StringComparison.Ordinal);
    }

    /// <summary>The window and what is shared are said as they are.</summary>
    [Fact]
    public void TheCaptionSaysWhichSecondsAndWhatItShares()
    {
        var line = KeyingLineFor173723();

        var seconds = string.Format(
            CultureInfo.InvariantCulture,
            "the {0:0} seconds the meter last read before the press",
            CwKeyingThresholds.Window.TotalSeconds);

        Assert.Contains(seconds, line, StringComparison.Ordinal);
        Assert.DoesNotContain("over the last six seconds", line, StringComparison.Ordinal);
        Assert.DoesNotContain("sharing nothing with the decoder", line, StringComparison.Ordinal);
        Assert.Contains("the same audio as the decoder", line, StringComparison.Ordinal);
    }

    private string KeyingLineFor173723()
    {
        var audio = WavAudio.Read(Path.Combine(
            EverySentenceOnTheSheetTests.Root(), "tests", "fixtures", "cw", "captured",
            "unadjudicated", "cw-2026-09-23-173723.wav"));

        var line = MainWindowViewModel.KeyingRecordLine(
            EverySentenceOnTheSheetTests.Meter(audio).Reading);

        _output.WriteLine(line);

        return line;
    }
}
