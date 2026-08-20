using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The two mark lengths have to be two things before the timings count as Morse
/// (HM-DEC-095, HM-DEC-090).
/// </summary>
/// <remarks>
/// <para>**THE COHERENCE CHECK MEASURED EACH MARK AGAINST TWO FITTED LENGTHS AND
/// NEVER ASKED WHETHER THOSE TWO WERE REALLY TWO THINGS.** A gate chattering on
/// band noise produces a continuum; a two-means fit cuts any continuum in half;
/// and the halves land near one and three of each other by construction. On
/// `cw-2026-08-20-014854`, which an independent instrument reads as holding no
/// keying at any pitch, the twenty marks in the window when a character is
/// invented run 10, 20, 35, 40, 45, 50, 55, 55, 60, 60, 80, 110, 115, 120, 125,
/// 125, 125, 130, 135, 135 milliseconds. **That is a smear, and it scored 0.46 on
/// coherence against a floor of 0.35.**</para>
/// <para>**WHAT A REAL FIST HAS THAT A SMEAR DOES NOT IS A GAP BETWEEN THE TWO
/// GROUPS.** HM-DEC-095 settled that this is the statistic that tells them apart,
/// and `CwToneSurvey` has asked it of a candidate pitch ever since. The speed
/// estimator never asked it at all.</para>
/// <para>**MEASURED AT THE MOMENT OF EVERY CHARACTER IN THIS REPOSITORY**: the
/// easy tier emits nothing below 4.4 and mostly far above, `cw-2026-08-17-134712`
/// at 6.9 and `cw-2026-08-17-013347` at 5.3, while the characters
/// `cw-2026-08-20-014854` produces sit between 2.1 and about 3.5.</para>
/// <para>**THE FIGURE IS NOT A SECOND COPY** (§0). It is read from
/// `CwToneSurvey.MinimumSeparation`, where it was measured, so the two cannot
/// drift apart.</para>
/// </remarks>
public sealed class ASmearIsNotTwoLengthsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the separations are printed.</param>
    public ASmearIsNotTwoLengthsTests(ITestOutputHelper output) => _output = output;

    private const int Rate = 48_000;

    private static double Ms(double milliseconds) => Rate * milliseconds / 1000;

    /// <remarks>
    /// <para>Proves HM-DEC-090: **the smear that invented a character is not
    /// Morse.** These are the twenty marks in the window at that moment, to the
    /// millisecond.</para>
    /// </remarks>
    [Fact]
    public void TheSmearThatInventedACharacterIsNotMorse()
    {
        var speed = new CwSpeedEstimator(Rate);

        foreach (var length in new[]
                 {
                     10, 20, 35, 40, 45, 50, 55, 55, 60, 60,
                     80, 110, 115, 120, 125, 125, 125, 130, 135, 135,
                 })
        {
            speed.AddMark(Ms(length));
            speed.AddGap(Ms(60));
        }

        _output.WriteLine(
            $"separation {speed.MarkSeparationInScatter:0.0}, "
            + $"coherence {speed.Coherence:0.00}, "
            + $"looksLikeMorse {speed.LooksLikeMorse}");

        Assert.True(
            speed.MarkSeparationInScatter < CwToneSurvey.MinimumSeparation,
            $"the smear separated at {speed.MarkSeparationInScatter:0.0}");

        Assert.False(speed.LooksLikeMorse);
    }

    /// <remarks>
    /// Proves the no-op: a fist sending two lengths clears the test easily, at
    /// both ends of the range this project has measured — `VA3VRR` at 2.73 dits
    /// to the dah (HM-DEC-145) and `N4L` at 4.24 (HM-DEC-144).
    /// </remarks>
    /// <param name="ditMs">The dit.</param>
    /// <param name="dahMs">The dah.</param>
    [Theory]
    [InlineData(100, 274)]
    [InlineData(56, 238)]
    [InlineData(60, 180)]
    public void ARealFistClearsItEasily(int ditMs, int dahMs)
    {
        var speed = new CwSpeedEstimator(Rate);
        var random = new Random(7300);

        for (var i = 0; i < 24; i++)
        {
            // A few percent of scatter, which is what a hand key gives even from
            // a steady operator. Perfectly identical marks have no scatter at
            // all, and a separation counted in scatter is then a division by
            // nothing rather than a measurement.
            var wobble = 1 + ((random.NextDouble() - 0.5) * 0.08);

            speed.AddMark(Ms((i % 3 == 2 ? dahMs : ditMs) * wobble));
            speed.AddGap(Ms(ditMs));
        }

        _output.WriteLine(
            $"dit {ditMs} dah {dahMs}: separation {speed.MarkSeparationInScatter:0.0}, "
            + $"looksLikeMorse {speed.LooksLikeMorse}");

        Assert.True(
            speed.MarkSeparationInScatter > CwToneSurvey.MinimumSeparation * 2,
            $"a real fist separated at only {speed.MarkSeparationInScatter:0.0}");

        Assert.True(speed.LooksLikeMorse);
    }

    /// <remarks>
    /// <para>Proves §0.0: **a sender who has sent only dits is not refused for
    /// having no dah.** There is nothing to separate yet, so this says nothing
    /// either way and the coherence check still has to pass on its own. The same
    /// answer is given when every mark is exactly the same length, where a
    /// separation counted in scatter would otherwise be a division by
    /// nothing.</para>
    /// </remarks>
    [Fact]
    public void ASenderWithNoDahYetIsNotRefusedForIt()
    {
        var speed = new CwSpeedEstimator(Rate);

        for (var i = 0; i < 24; i++)
        {
            speed.AddMark(Ms(60));
            speed.AddGap(Ms(60));
        }

        _output.WriteLine(
            $"all dits: separation {speed.MarkSeparationInScatter:0.0}, "
            + $"looksLikeMorse {speed.LooksLikeMorse}");

        Assert.True(speed.MarkSeparationInScatter >= CwToneSurvey.MinimumSeparation);
    }

    /// <remarks>
    /// <para>Proves it end to end: **`cw-2026-08-20-014854` now produces nothing
    /// at all.** It holds no keying at any pitch and emitted one marked character
    /// before this test existed, which HM-DEC-090 settles is not good enough.</para>
    /// </remarks>
    [Fact]
    public void TheRecordingWithNoKeyingInItNowSaysNothing()
    {
        var audio = WavAudio.Read(Path.Combine(
            CapturedSignalTests.Folder,
            "unadjudicated",
            "cw-2026-08-20-014854.wav"));

        var decoder = new CwDecoder(audio.SampleRate, 600);
        var hop = decoder.Tracker.HopSamples;
        var text = new List<string>();

        decoder.CharacterDecoded += c => text.Add(c.Text);

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        _output.WriteLine($"read '{string.Concat(text)}'");

        Assert.Equal(0, decoder.Report.CharactersEmitted);
    }
}
