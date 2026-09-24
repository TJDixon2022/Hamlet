using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Cw;

/// <summary>
/// Work instruction 411 task 2, criterion 6.2: the capture sidecar's `keying` and
/// `elementHz` lines stop contradicting the lines beside them.
/// </summary>
/// <remarks>
/// <para>**FROM `cw-2026-09-23-173723.txt`, AS TIM READ IT** (HM-DEC-170):
/// `keying no keying at 575 Hz, 69 ms key down, 16 dB swing, 98 key-downs` - no
/// keying and ninety-eight key-downs in one breath - and `elementHz not measured`
/// directly under `elements 169 seen, 169 resolved`.</para>
/// <para>**THE METER'S VERDICT IS NOT TOUCHED** (work instruction 411 §10). The
/// meter counts every rise above its threshold and calls them key-downs whether
/// or not it believes anybody keyed them; the sentence is what changes, so it says
/// what was counted and which of the meter's four tests the window failed.</para>
/// <para>**ON SAVED AUDIO, THE WAY THE LIVE PATH RUNS**: six-second windows, one a
/// second, through one meter, so the verdict and its fifteen-window hold are the
/// ones the operator's sidecar was written from.</para>
/// </remarks>
public sealed class TheSidecarDoesNotContradictItselfTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings and lines are printed.</param>
    public TheSidecarDoesNotContradictItselfTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// **KEYING.** A no-keying line does not count key-downs, and it says which
    /// test the window failed.
    /// </summary>
    [Theory]
    [InlineData("unadjudicated/cw-2026-09-23-173723.wav")]
    [InlineData("unadjudicated/cw-2026-08-22-014113.wav")]
    public void ANoKeyingLineDoesNotCountKeyDowns(string capture)
    {
        var reading = MeterAsTheLivePathRunsIt(WavAudio.Read(Fixture(capture)));
        var line = MainWindowViewModel.KeyingLine(reading);

        _output.WriteLine(
            $"{capture}: verdict={reading.Verdict} held={reading.Held} "
            + $"tone={reading.ToneHz:0} runs={reading.Runs} "
            + $"elementMedian={reading.ElementMedianMs:0} swing={reading.SwingDb:0.0} "
            + $"score={reading.Score:0.000}");
        _output.WriteLine($"keying     {line}");

        // The saved audio has to show the contradiction for this to test anything.
        Assert.Equal(KeyingVerdict.NoKeying, reading.Verdict);
        Assert.False(reading.Held);

        Assert.StartsWith("no keying", line, StringComparison.Ordinal);
        Assert.DoesNotContain("key-down", line, StringComparison.Ordinal);
        Assert.DoesNotContain("key down", line, StringComparison.Ordinal);

        // **THE NUMBERS STAY** - the sentence is made true, not emptied.
        Assert.Contains($"at {reading.ToneHz:0} Hz", line, StringComparison.Ordinal);
        Assert.Contains($"{reading.Runs} rises", line, StringComparison.Ordinal);
        Assert.Contains($"{reading.SwingDb:0} dB swing", line, StringComparison.Ordinal);

        // **AND WHY IT IS NO KEYING**, named against the meter's own bar.
        Assert.Contains("needs", line, StringComparison.Ordinal);
    }

    /// <summary>
    /// **ELEMENTHZ.** Under a line that resolves elements, the element pitch line
    /// does not say nothing was measured; it says what was not measured.
    /// </summary>
    [Fact]
    public void TheElementPitchLineDoesNotDenyTheElementsAboveIt()
    {
        var audio = WavAudio.Read(Fixture("unadjudicated/cw-2026-09-23-173723.wav"));
        var report = new CwDecodeReport
        {
            ToneHz = 575,
            ElementsSeen = 169,
            ElementsResolved = 169,
        };

        var elements = $"elements   {report.ElementsSeen} seen, {report.ElementsResolved} resolved";
        var line = MainWindowViewModel.ElementPitchLine(audio, report);

        _output.WriteLine(elements);
        _output.WriteLine($"elementHz  {line}");

        Assert.False(
            line.StartsWith("not measured", StringComparison.Ordinal),
            "a bare 'not measured' under a line that resolved elements reads as "
            + "no element having been measured");
        Assert.Contains("element's own pitch", line, StringComparison.Ordinal);
    }

    private static KeyingReading MeterAsTheLivePathRunsIt(MonoAudio audio)
    {
        var meter = new CwKeyingMeter();
        var window = (int)(CwKeyingThresholds.Window.TotalSeconds * audio.SampleRate);
        var reading = KeyingReading.None;

        for (var end = window; end <= audio.Samples.Length; end += audio.SampleRate)
        {
            reading = meter.Update(
                new MonoAudio(audio.SampleRate, audio.Samples[(end - window)..end]));
        }

        return reading;
    }

    private static string Fixture(string relative)
    {
        var here = new DirectoryInfo(AppContext.BaseDirectory);

        while (here is not null
               && !Directory.Exists(Path.Combine(here.FullName, "tests", "fixtures")))
        {
            here = here.Parent;
        }

        Assert.NotNull(here);

        return Path.Combine(
            here!.FullName, "tests", "fixtures", "cw", "captured", relative);
    }
}
