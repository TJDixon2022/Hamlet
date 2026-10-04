using System.Globalization;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Cw;

/// <summary>
/// The sidecar's `tonePeak` number is a figure measured over the capture's own audio, at the printed sender's pitch, and
/// its caption says so (R63; re-sourced to the shape side by work instruction 537).
/// </summary>
/// <remarks>
/// <para>**THE FIGURE IT IS HELD TO IS MEASURED INDEPENDENTLY**, by <see cref="ToneOverNoiseByHand"/>, which shares no
/// code with the sheet's own measurement. The audio is a call keyed in this file at a known pitch, so the test needs no
/// recording.</para>
/// <para>**AND WHERE NOBODY WAS PRINTED THE LINE SAYS SO AND PRINTS NO NUMBER** (CLAUDE.md 0.0).</para>
/// </remarks>
public sealed class TheTonePeakIsAboutThisRecordingTests
{
    private const int Rate = 8000;
    private const double Pitch = 625;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the lines are printed.</param>
    public TheTonePeakIsAboutThisRecordingTests(ITestOutputHelper output) => _output = output;

    private static CwShapeSideReading Printed(double pitchHz)
        => CwShapeSideReading.Nothing with { HeardSeconds = 20, PrintedPitchHz = pitchHz, PrintedDitSeconds = 0.06 };

    /// <summary>The number is this recording's, at the printed sender's pitch, and the caption says it is.</summary>
    [Fact]
    public void TheNumberIsMeasuredOverThisRecordingAtThePrintedPitch()
    {
        var audio = CwSignal.Generate(new CwSignalRequest(
            "CQ CQ DE N0CALL K", WordsPerMinute: 20, ToneHz: Pitch, SampleRate: Rate, Amplitude: 0.3,
            NoiseAmplitude: 0.04, LeadInSeconds: 2, TailSeconds: 2, Seed: 537));

        var line = MainWindowViewModel.TonePeakRecordLine(audio, Printed(Pitch));
        var expected = ToneOverNoiseByHand.Peak(audio, Pitch);

        _output.WriteLine(line);
        _output.WriteLine($"measured independently over this file at {Pitch:0} Hz: {expected:0.0}");

        Assert.StartsWith("tonePeak   ", line, StringComparison.Ordinal);

        var printed = double.Parse(line["tonePeak   ".Length..].Split(' ')[0], CultureInfo.InvariantCulture);

        Assert.True(Math.Abs(printed - expected) <= 0.051, $"the sheet prints {printed:0.0} and this recording measures {expected:0.0}");
        Assert.Contains("this recording", line, StringComparison.Ordinal);
        Assert.Contains("printed sender", line, StringComparison.Ordinal);
    }

    /// <summary>With nobody printed the line says so in words.</summary>
    [Fact]
    public void WithNobodyPrintedTheLineSaysSoAndPrintsNoNumber()
    {
        var audio = CwSignal.Generate(new CwSignalRequest(
            " ", SampleRate: Rate, Amplitude: 0, NoiseAmplitude: 0.04, LeadInSeconds: 5, TailSeconds: 5, Seed: 538));

        var line = MainWindowViewModel.TonePeakRecordLine(audio, CwShapeSideReading.Nothing);

        _output.WriteLine(line);

        Assert.StartsWith("tonePeak   not measured  (nobody was printed", line, StringComparison.Ordinal);
        Assert.DoesNotMatch(@"\d+\.\d", line);
    }
}
