using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The independent keying sweep may not report a key-down length nobody could
/// send, and may not look somewhere the decoder cannot (§0.0, HM-DEC-091).
/// </summary>
/// <remarks>
/// <para>**THE METER WAS WRONG ON THIRTEEN OF THE TWENTY-THREE RECORDINGS IN
/// THIS TREE AND ONE NUMBER WAS MOST OF IT.** A threshold crossed by noise is
/// crossed hundreds of times, so on a recording holding a real station the
/// chatter runs outnumber the element-length ones several to one and a median
/// over all of them lands among the chatter. It read four milliseconds on the
/// capture carrying an adjudicated `VA3VRR` and three on the one carrying an
/// adjudicated `N4L`, and then said there was no keying in either.</para>
/// <para>**FOUR MILLISECONDS IS NOT A THING A HAND CAN SEND.** A dit at sixty
/// words a minute is twenty and sixty is faster than anybody sends by hand, so
/// the figure was not a measurement that had gone wrong; it was a measurement of
/// something that is not Morse, printed where a reader takes it for one.</para>
/// <para>These recordings are not adjudicated and this file does not treat them
/// as though they were. What it asserts is the two things that are true whatever
/// any station sent: an element length has to be an element length, and this
/// meter has to be able to look where the decoder looks.</para>
/// </remarks>
public sealed class TheKeyingWitnessSaysNothingImpossibleTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public TheKeyingWitnessSaysNothingImpossibleTests(ITestOutputHelper output)
        => _output = output;

    private static IEnumerable<string> Recordings()
    {
        var root = CapturedSignalTests.Folder;

        foreach (var wav in Directory.GetFiles(root, "*.wav").OrderBy(p => p))
        {
            yield return Path.GetFileNameWithoutExtension(wav);
        }

        foreach (var wav in Directory
            .GetFiles(Path.Combine(root, "unadjudicated"), "*.wav")
            .OrderBy(p => p))
        {
            yield return "unadjudicated/" + Path.GetFileNameWithoutExtension(wav);
        }
    }

    /// <summary>Every recording made on the air.</summary>
    public static TheoryData<string> Captures
    {
        get
        {
            var data = new TheoryData<string>();

            foreach (var name in Recordings())
            {
                data.Add(name);
            }

            return data;
        }
    }

    /// <remarks>
    /// Proves §0.0 on the meter's own sheet: the length it prints as a key-down
    /// is one somebody could have keyed, or it prints nothing.
    /// </remarks>
    /// <param name="name">The recording.</param>
    [Theory]
    [MemberData(nameof(Captures))]
    public void NoReadingClaimsAKeyDownNobodyCouldSend(string name)
    {
        var audio = WavAudio.Read(
            Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

        var sighting = KeyingEnvelope.Best(audio);

        Assert.NotNull(sighting);

        var profile = sighting!.Value.Profile;

        _output.WriteLine(
            $"{name}: {sighting.Value.ToneHz:0} Hz, element median "
            + $"{profile.ElementMedianMs:0} ms over {profile.RunsMs.Count} key-downs, "
            + $"score {profile.Score:0.000}, duty {profile.Duty:0.000}");

        Assert.True(
            profile.ElementMedianMs == 0
            || profile.ElementMedianMs >= KeyingEnvelope.ShortestElementMs,
            $"{name} reports a key-down of {profile.ElementMedianMs:0.0} ms, which is "
            + $"shorter than a dit at any speed a hand sends");

        Assert.True(
            profile.ElementMedianMs <= KeyingEnvelope.LongestElementMs,
            $"{name} reports a key-down of {profile.ElementMedianMs:0.0} ms, which is "
            + $"longer than a dah at any speed a hand sends");
    }

    /// <remarks>
    /// Proves the meter can contradict the decoder, which is the only reason it
    /// exists (HM-DEC-091). It swept 400 to 1200 hertz while the decoder tracks
    /// 300 to 900, so two of this tree's recordings were tracked at pitches the
    /// meter could not examine, and one recording was answered at 1000 hertz.
    /// </remarks>
    [Fact]
    public void TheSweepLooksExactlyWhereTheDecoderCan()
    {
        Assert.Equal(CwToneTracker.MinimumToneHz, KeyingEnvelope.LowestToneHz);
        Assert.Equal(CwToneTracker.MaximumToneHz, KeyingEnvelope.HighestToneHz);
    }

    /// <remarks>
    /// Proves HM-DEC-120 is not traded for the repair above: the recordings that
    /// hold no station still read as holding none, and they do it on the keying
    /// score rather than on the element length.
    /// </remarks>
    [Theory]
    [InlineData("unadjudicated/cw-2026-08-20-014854")]
    [InlineData("unadjudicated/cw-2026-08-20-014935")]
    public void ARecordingHoldingNothingStillReadsAsHoldingNothing(string name)
    {
        var audio = WavAudio.Read(
            Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

        var profile = KeyingEnvelope.Best(audio)!.Value.Profile;

        _output.WriteLine(
            $"{name}: score {profile.Score:0.000} against a bar of "
            + $"{CwKeyingThresholds.KeyingScore:0.00}");

        Assert.True(
            profile.Score < CwKeyingThresholds.KeyingScore,
            $"{name} holds no keying at any pitch and scores {profile.Score:0.000}");
    }
}
