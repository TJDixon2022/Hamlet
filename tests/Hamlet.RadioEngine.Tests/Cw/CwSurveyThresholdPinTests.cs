using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The tone survey decides in a different place from the settled pass, and must
/// go on doing so (HM-DEC-107 phase 2).
/// </summary>
/// <remarks>
/// <para>**THE TWO STAGES ANSWER DIFFERENT QUESTIONS AND A SINGLE DEFINITION
/// BREAKS ONE OF THEM.** The settled pass measures how long a mark is, and half
/// amplitude is where a shaped element's true edge sits, so deciding six
/// decibels below the keyed level measures the mark it actually was
/// (HM-DEC-105). The survey is not measuring lengths to report them; it is
/// deciding whether a bin holds anybody keying at all, judged on the separation
/// between two clusters of mark durations. Moving its decision up the leading
/// edge shortens every mark and tightens the very separation it exists to
/// measure.</para>
/// <para>**THE DECIDING EVIDENCE IS NOT THE FIXTURES IT COSTS.** Unifying the
/// two definitions also broke five noiseless fixtures, which would be weak
/// evidence on its own given what HM-OPEN-018 established about them. What
/// settles it is the real 13:47 off-air recording, where the tone stops being
/// found at all.</para>
/// <para>This test exists so that a later session cannot quietly unify them in
/// the name of consistency. If it fails, the survey's threshold has been moved,
/// and the thing to read is HM-OPEN-023 rather than this file.</para>
/// </remarks>
public sealed class CwSurveyThresholdPinTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the measurement is printed.</param>
    public CwSurveyThresholdPinTests(ITestOutputHelper output) => _output = output;

    /// <remarks>
    /// <para>Proves HM-DEC-107 phase 2. **The tone in the 13:47 capture is found
    /// where it actually is**, which is the measurement that decided the survey
    /// keeps its own threshold. This recording carries a strong steady signal
    /// near 500 Hz and no readable station, and finding that signal is the whole
    /// of what the survey is being asked for here.</para>
    /// </remarks>
    [Fact]
    public void TheToneInTheInterferenceCaptureIsStillFound()
    {
        var audio = WavAudio.Read(Path.Combine(
            CapturedSignalTests.Folder, "cw-2026-08-17-134712.wav"));

        var decoder = new CwDecoder(audio.SampleRate, 600);

        using var source = new BufferedAudioSource(audio);
        decoder.Listen(source);
        source.PumpAll();
        decoder.Flush();

        var report = decoder.Report;

        _output.WriteLine(
            $"tone {report.ToneHz:0} Hz, snr {report.SnrDb:0.0} dB, "
            + $"hasTone {report.HasTone}");

        Assert.True(
            report.HasTone,
            "the tone was lost in the 13:47 capture. If CwToneSurvey's decision "
            + "level was moved to half amplitude to match the settled pass, that "
            + "is the cause and HM-OPEN-023 explains why the two differ.");

        Assert.True(
            report.SnrDb >= 15,
            $"the signal reads {report.SnrDb:0.0} dB in a recording where it is "
            + "plainly audible");
    }

    /// <remarks>
    /// <para>Proves HM-DEC-107 phase 2 from the other side: **the survey still
    /// refuses to call that signal keying.** The recording holds a carrier and no
    /// station, so finding the tone and claiming somebody is sending are
    /// different things and only the first is correct (§0.0, HM-DEC-095).</para>
    /// </remarks>
    [Fact]
    public void FindingTheToneIsNotClaimingSomebodyIsSending()
    {
        var audio = WavAudio.Read(Path.Combine(
            CapturedSignalTests.Folder, "cw-2026-08-17-134712.wav"));

        var tracker = new CwToneTracker(audio.SampleRate, 600);

        var claimed = 0;

        tracker.Process(audio.Samples, 0, _ =>
        {
            if (tracker.Verdict.Keyed is not null)
            {
                claimed++;
            }
        });

        _output.WriteLine($"tracked {tracker.ToneHz:0} Hz, "
            + $"keying claimed on {claimed} measurements");

        Assert.False(tracker.HasKeying);
        Assert.Equal(0, claimed);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-107 phase 2 on the recording that has a station in it,
    /// so the pin cannot be satisfied by a survey that has simply stopped
    /// working. **The 01:33 capture still yields its keying**, at the pitch
    /// independent analysis put it.</para>
    /// </remarks>
    [Fact]
    public void TheSurveyStillFindsRealKeyingWhereThereIsSome()
    {
        var audio = WavAudio.Read(Path.Combine(
            CapturedSignalTests.Folder, "cw-2026-08-17-013347.wav"));

        var tracker = new CwToneTracker(audio.SampleRate, 600);
        var everKeyed = false;

        tracker.Process(audio.Samples, 0, _ =>
        {
            everKeyed |= tracker.Verdict.Keyed is not null;
        });

        _output.WriteLine($"tracked {tracker.ToneHz:0} Hz, keying seen: {everKeyed}");

        Assert.True(
            everKeyed,
            "no keying was ever found in a recording of a station answering a "
            + "call, so the survey is not measuring what this pins");

        Assert.InRange(tracker.ToneHz, 595, 640);
    }
}
