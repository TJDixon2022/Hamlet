using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// A pitch measured on one frequency is not a claim about the next one.
/// </summary>
/// <remarks>
/// <para>**THE MISS OF 2026-08-26.** The operator tuned to 14.0275 MHz and sat
/// listening to fast CW while the terminal said nothing decoded yet. The
/// sidecar written there reported `toneHz 300.0 Hz (measured from the keying
/// the survey admitted)` and `tonePeak 50.2` — both measured twenty-four
/// minutes and one QSY earlier, from audio at a frequency the receiver had
/// left. The decoder went on mixing at 300 Hz while the station in front of him
/// keyed above 400, and refused everything. **The refusal was correct**: nothing
/// was being keyed at 300. What was wrong was that it was still pointed
/// there.</para>
/// <para>**THE HOLD ITSELF IS RIGHT AND IS NOT WHAT CHANGED.** The tracker
/// keeps its last measured pitch through the gaps in a sender's keying, which
/// is what makes a slow fist readable at all; the survey holds three seconds of
/// history and a sender at twelve words a minute leaves gaps longer than that.
/// What it could not do was let go.</para>
/// <para>**AND IT HANGS ON THE FREQUENCY RATHER THAN ON A CLOCK**, because that
/// is when the evidence stops existing. A station is entitled to pause for as
/// long as it likes. It is not entitled to be heard on a frequency the receiver
/// is no longer on.</para>
/// </remarks>
public sealed class AHeldPitchDoesNotOutliveItsEvidenceTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the pitches are printed.</param>
    public AHeldPitchDoesNotOutliveItsEvidenceTests(ITestOutputHelper output)
        => _output = output;

    private const int Rate = 48000;

    /// <summary>
    /// A real recording of a real station, decoded to the point where the
    /// tracker has measured its pitch.
    /// </summary>
    /// <remarks>
    /// **SYNTHESIZED KEYING WAS TRIED FIRST AND THE SURVEY REFUSED IT**, which
    /// is the survey behaving correctly rather than a defect: a hard-gated sine
    /// with digital silence between its elements is not what a receiver
    /// produces, and HM-OPEN-018 has that class of fixture on record for
    /// exactly this. `cw-2026-08-17-013347` holds `VA3VRR` at about twelve words
    /// a minute (HM-DEC-145) and is the pitch this test needs the decoder to be
    /// holding. A real capture outranks a synthetic one (HM-DEC-091).
    /// </remarks>
    /// <returns>The decoder, and the audio it has heard.</returns>
    private static (CwDecoder Decoder, MonoAudio Audio) Listening()
    {
        var audio = WavAudio.Read(Path.Combine(
            CapturedSignalTests.Folder, "cw-2026-08-17-013347.wav"));

        var decoder = new CwDecoder(audio.SampleRate, 600);

        Feed(decoder, audio.Samples, 0);

        return (decoder, audio);
    }

    private static void Feed(CwDecoder decoder, float[] audio, long from)
    {
        var hop = decoder.Tracker.HopSamples;

        for (var at = 0; at + hop <= audio.Length; at += hop)
        {
            decoder.Process(new AudioChunk(from + at, Rate, audio.AsSpan(at, hop)));
        }
    }

    /// <remarks>
    /// Proves the fault at its cause: after the dial moves, the decoder no
    /// longer reports a pitch it measured before the move as a measurement.
    /// </remarks>
    [Fact]
    public void MovingTheDialReleasesThePitchMeasuredBeforeIt()
    {
        var (decoder, _) = Listening();

        _output.WriteLine(
            $"before the QSY: measured={decoder.Report.PitchWasMeasured}, "
            + $"toneHz={decoder.Report.ToneHz:0.0}");

        Assert.True(
            decoder.Report.PitchWasMeasured,
            "the fixture never gave the decoder a pitch to hold, so this test "
            + "cannot show it letting go of one");

        decoder.Retuned();

        _output.WriteLine(
            $"after the QSY:  measured={decoder.Report.PitchWasMeasured}, "
            + $"toneHz={decoder.Report.ToneHz:0.0}");

        Assert.False(
            decoder.Report.PitchWasMeasured,
            $"the decoder still calls {decoder.Report.ToneHz:0.0} Hz a "
            + "measurement after the radio moved away from where it measured it");
    }

    /// <remarks>
    /// Proves the held peak goes with the pitch. It rises at once and decays
    /// about a decibel a second, so it outlives a station's gaps by design and
    /// outlived this QSY by accident — the sheet on 14.0275 MHz reported 50.2 dB
    /// measured somewhere else.
    /// </remarks>
    [Fact]
    public void MovingTheDialReleasesTheHeldPeakToo()
    {
        var (decoder, _) = Listening();

        var before = decoder.Report.SnrDb;

        decoder.Retuned();

        var after = decoder.Report.SnrDb;

        _output.WriteLine($"tonePeak {before:0.0} before the QSY, {after:0.0} after");

        Assert.False(
            double.IsNaN(before),
            "the fixture never produced a held peak, so this test cannot show "
            + "it being released");

        Assert.True(
            double.IsNaN(after),
            $"the decoder still reports a peak of {after:0.0} dB measured on a "
            + "frequency the radio has left");
    }

    /// <remarks>
    /// <para>Proves what must not change. The hold is what carries a sender
    /// across his own gaps, and a release that fired on anything other than a
    /// QSY would take that with it.</para>
    /// <para>So: keying, then four seconds of silence — longer than the survey's
    /// three seconds of history, which is exactly the case the hold exists for —
    /// and the pitch is still a measurement afterwards.</para>
    /// </remarks>
    [Fact]
    public void AStationIsEntitledToPause()
    {
        var (decoder, audio) = Listening();

        Assert.True(decoder.Report.PitchWasMeasured);

        Feed(decoder, new float[Rate * 4], audio.Samples.Length);

        _output.WriteLine(
            $"after four seconds of silence: measured="
            + $"{decoder.Report.PitchWasMeasured}, "
            + $"toneHz={decoder.Report.ToneHz:0.0}");

        Assert.True(
            decoder.Report.PitchWasMeasured,
            "four seconds of silence released the pitch, so the hold that makes "
            + "a slow fist readable has been broken");
    }

    /// <remarks>
    /// Proves the release does not reset the decoder. The speed the tracker has
    /// learned is a fact about the operator's ear and his habits rather than
    /// about a frequency, and throwing it away on every band change would make
    /// the first characters after a QSY worse rather than better.
    /// </remarks>
    [Fact]
    public void TheReleaseDoesNotThrowAwayWhatIsNotAboutTheFrequency()
    {
        var (decoder, _) = Listening();

        var before = decoder.Reading.WordsPerMinute;

        decoder.Retuned();

        var after = decoder.Reading.WordsPerMinute;

        _output.WriteLine($"{before:0.0} WPM before the QSY, {after:0.0} after");

        Assert.Equal(before, after);
    }
}
