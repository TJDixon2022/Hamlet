using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// A refinement and a follow are different events (HM-DEC-123).
/// </summary>
/// <remarks>
/// <para>**THE WHOLE COST WAS PAID IN ONE LINE**, `_settled.Reset()` on every
/// tracker move. HM-DEC-096 put it there because a move usually does mean
/// somebody else started transmitting, and sometimes it means the survey
/// preferred its neighbouring bin on the station already being read.</para>
/// <para>**THE CRITERION IS THE SURVEY'S OWN GRID AND IT WAS MEASURED RATHER
/// THAN CHOSEN.** Across every recording this repository holds, a move within one
/// station is exactly one coarse bin, twenty-five hertz, and the one genuine
/// station change — the caller at 615 handing over to the answerer at 730 — is a
/// hundred. There is nothing in between to choose from.</para>
/// </remarks>
public sealed class CwRefiningRetuneTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the counts are printed.</param>
    public CwRefiningRetuneTests(ITestOutputHelper output) => _output = output;

    private static string Captured(string name)
        => Path.Combine(CwFixtureCatalogue.Folder, "..", "captured", name + ".wav");

    private (int Retunes, int Follows, string Settled) Decode(string path, double start)
    {
        var audio = WavAudio.Read(path);
        var decoder = new CwDecoder(audio.SampleRate, start);
        var settled = new System.Text.StringBuilder();

        decoder.CharacterSettled += c => settled.Append(c.Text);

        using var source = new BufferedAudioSource(audio);
        decoder.Listen(source);
        source.PumpAll();
        decoder.Flush();

        _output.WriteLine($"{decoder.Tracker.Retunes} moves, "
            + $"{decoder.Tracker.Follows} of them to a different station");

        _output.WriteLine($"settled '{settled}'");

        return (decoder.Tracker.Retunes, decoder.Tracker.Follows, settled.ToString());
    }

    /// <remarks>
    /// <para>**THE RECORDING THIS COST A CALLSIGN ON.** Thirty seconds of a
    /// station answering a call, and the tracker moves three times: once from
    /// cold, then twenty-five hertz down and twenty-five back as the survey
    /// settles which of two neighbouring bins holds a station sitting between
    /// them. All three used to throw the settled window away and the pass got as
    /// far as four placeholders.</para>
    /// <para>Nobody knows what that station sent beyond what can be read from the
    /// audio, so what is asserted is the two counts and the prefix, not a
    /// transcript (HM-DEC-091).</para>
    /// </remarks>
    [Fact]
    public void TheSurveySettlingBetweenTwoBinsIsNotAStationChange()
    {
        var run = Decode(Captured("cw-2026-08-17-013347"), 600);

        Assert.True(
            run.Retunes > run.Follows,
            $"the tracker moved {run.Retunes} times and every one of them counted "
            + "as a different station, so the distinction is not being read");

        // One follow: the move from cold, before anything had been confirmed.
        Assert.Equal(1, run.Follows);
        Assert.Contains("VA3VRR", run.Settled, StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>**AND THE CONTROL, WHICH IS A REAL HANDOVER.** The two-station
    /// fixture is a caller at 615 hertz answered by a station at 730, joined
    /// across a gap (HM-DEC-104). That move is a hundred hertz and it has to keep
    /// resetting, because the settled window really is full of somebody else by
    /// then.</para>
    /// </remarks>
    [Fact]
    public void AHandoverToAnotherStationStillResets()
    {
        var run = Decode(
            Path.Combine(CwFixtureCatalogue.Folder, CwFixtureCatalogue.TwoStationName + ".wav"),
            600);

        Assert.True(
            run.Follows >= 2,
            $"only {run.Follows} of {run.Retunes} moves counted as a different "
            + "station on a recording that contains two of them");
    }

    /// <remarks>
    /// <para>Proves the criterion is the survey's grid rather than a number
    /// somebody liked. A tracker that has never reported a pitch has nothing to
    /// refine, so its first move is a follow however small it is.</para>
    /// </remarks>
    [Fact]
    public void AMoveBeforeAnythingHasBeenReadIsAFollow()
    {
        var audio = WavAudio.Read(Captured("cw-2026-08-18-004507"));
        var decoder = new CwDecoder(audio.SampleRate, 500);

        using var source = new BufferedAudioSource(audio);
        decoder.Listen(source);
        source.PumpAll();
        decoder.Flush();

        _output.WriteLine($"{decoder.Tracker.Retunes} moves, "
            + $"{decoder.Tracker.Follows} follows");

        Assert.True(decoder.Tracker.Follows >= 1);
        Assert.True(decoder.Tracker.Retunes > decoder.Tracker.Follows);
    }
}
