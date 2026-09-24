using System.Globalization;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Cw;

/// <summary>
/// Work instruction 417 task 2, step 6 criterion 6.7: the sidecar's `tonePeak`
/// number is a figure measured over the capture's own audio, and its caption says
/// so (R63).
/// </summary>
/// <remarks>
/// <para>**THE FIGURE IT IS HELD TO IS MEASURED INDEPENDENTLY**, by
/// <see cref="ToneOverNoiseByHand"/>, which shares no code with the sheet's own
/// measurement, at the pitch a fresh decoder that heard only this file tracked.</para>
/// <para>**AND WHERE THERE IS NOTHING TO MEASURE THE LINE SAYS SO AND PRINTS NO
/// NUMBER** (CLAUDE.md 0.0): on a capture whose pitch the decoder never measured, a
/// strength at that pitch is a fact about wherever the tracker happened to sit.</para>
/// </remarks>
public sealed class TheTonePeakIsAboutThisRecordingTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the lines are printed.</param>
    public TheTonePeakIsAboutThisRecordingTests(ITestOutputHelper output) => _output = output;

    /// <summary>The number is this recording's, and the caption says it is.</summary>
    /// <param name="stamp">A saved capture whose pitch the decoder measured.</param>
    [Theory]
    [InlineData("cw-2026-09-23-173723")]
    [InlineData("cw-2026-08-17-013347")]
    public void TheNumberIsMeasuredOverThisRecording(string stamp)
    {
        var (audio, report) = Replay(stamp);

        Assert.True(report.HasTone && report.PitchWasMeasured, $"{stamp} no longer has a measured pitch");

        var line = MainWindowViewModel.TonePeakRecordLine(audio, report);
        var expected = ToneOverNoiseByHand.Peak(audio, report.ToneHz);

        _output.WriteLine(line);
        _output.WriteLine($"measured independently over this file at {report.ToneHz:0.0} Hz: {expected:0.0}");

        Assert.StartsWith("tonePeak   ", line, StringComparison.Ordinal);

        var printed = double.Parse(
            line["tonePeak   ".Length..].Split(' ')[0], CultureInfo.InvariantCulture);

        Assert.True(
            Math.Abs(printed - expected) <= 0.051,
            $"the sheet prints {printed:0.0} and this recording measures {expected:0.0}");
        Assert.Contains("this recording", line, StringComparison.Ordinal);
        Assert.DoesNotContain("not a figure about this recording", line, StringComparison.Ordinal);
    }

    /// <summary>With no measured pitch the line says so in words.</summary>
    [Fact]
    public void WithNoMeasuredPitchTheLineSaysSoAndPrintsNoNumber()
    {
        var (audio, report) = Replay("cw-2026-08-20-014854");

        Assert.False(report.PitchWasMeasured, "014854 now has a measured pitch");

        var line = MainWindowViewModel.TonePeakRecordLine(audio, report);

        _output.WriteLine(line);

        Assert.StartsWith("tonePeak   not measured", line, StringComparison.Ordinal);
        Assert.DoesNotMatch(@"\d+\.\d", line);
    }

    private static (MonoAudio Audio, CwDecodeReport Report) Replay(string stamp)
    {
        var wav = Directory
            .GetFiles(
                Path.Combine(Root(), "tests", "fixtures", "cw", "captured"),
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

        return (audio, decoder.Report);
    }

    private static string Root()
    {
        var here = new DirectoryInfo(AppContext.BaseDirectory);

        while (here is not null
               && !Directory.Exists(Path.Combine(here.FullName, "tests", "fixtures")))
        {
            here = here.Parent;
        }

        Assert.NotNull(here);

        return here!.FullName;
    }
}
