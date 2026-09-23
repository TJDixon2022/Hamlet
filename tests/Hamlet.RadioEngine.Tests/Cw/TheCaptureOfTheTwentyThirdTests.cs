using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// A printer, not a test: Tim's capture of 12:55 UTC on 2026-09-23 decoded
/// through <see cref="CwDecodeHarness"/> and printed whole, so the same recording
/// can be read beside itself under two builds (work instruction 403, tasks 1 and 2).
/// </summary>
/// <remarks>
/// <para>**IT ASSERTS NO TEXT**, because nobody knows what was sent (§0.0). It
/// asserts only that the file was read and that something was emitted, and it is
/// on no carry-forward line. The capture is not adjudicated and is in no floor
/// table.</para>
/// </remarks>
public sealed class TheCaptureOfTheTwentyThirdTests
{
    private const string Name = "unadjudicated/cw-2026-09-23-125515";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the printer.</summary>
    /// <param name="output">Where the transcript and counts are printed.</param>
    public TheCaptureOfTheTwentyThirdTests(ITestOutputHelper output) => _output = output;

    /// <summary>Prints the settled transcript and the counts beside it.</summary>
    [Fact]
    public void PrintsWhatTheDecoderReads()
    {
        var audio = WavAudio.Read(
            Path.Combine(CapturedSignalTests.Folder, Name + ".wav"));

        var result = CwDecodeHarness.Decode(audio, 600);
        var report = result.Report;

        _output.WriteLine($"capture: {Name}.wav, {audio.Samples.Length} samples at {audio.SampleRate} Hz");
        _output.WriteLine($"transcript: [{result.Text}]");
        _output.WriteLine($"characters emitted: {report.CharactersEmitted}");
        _output.WriteLine($"characters unsure: {report.CharactersUnsure}");
        _output.WriteLine($"elements seen: {report.ElementsSeen}");
        _output.WriteLine($"elements resolved: {report.ElementsResolved}");
        _output.WriteLine($"winning speed: {result.WordsPerMinute} wpm");
        _output.WriteLine($"tone admitted: {report.ToneHz:0} Hz, measured {report.PitchWasMeasured}");

        Assert.True(audio.Samples.Length > 0, "the capture was not read");
        Assert.True(report.CharactersEmitted > 0, "nothing was emitted");
    }
}
