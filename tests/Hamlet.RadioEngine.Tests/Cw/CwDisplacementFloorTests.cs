using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// A confirmed station is not abandoned for a candidate far below it
/// (HM-DEC-127).
/// </summary>
/// <remarks>
/// <para>**THE AUDIO HERE IS DELIBERATELY SILENT BETWEEN THE ELEMENTS**, which is
/// the opposite of what every fixture under `tests/fixtures/cw/receiver` does and
/// is the whole point of this one. Digital silence is what lets a station's own
/// image in a distant bin arrive as a hard-limited replica with nothing to bury
/// it: same dit, same dah, same timing, thirty-five decibels down, and clustering
/// three times more cleanly than the station itself. **This is the cheapest way
/// this project knows to manufacture that image**, so it is kept rather than
/// corrected, while the fixture the shipped test decodes was given a band
/// (HM-DEC-127, second half).</para>
/// <para>**IT IS NOT A PREFERENCE FOR LOUDNESS AND HM-DEC-095 IS NOT AMENDED.**
/// That ruling governs which of several signals to read when nothing is being
/// read yet, where loudness picked a carrier over a station. Here something is
/// being read and the question is whether to abandon it.</para>
/// </remarks>
public sealed class CwDisplacementFloorTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the moves are printed.</param>
    public CwDisplacementFloorTests(ITestOutputHelper output) => _output = output;

    private const string Message = "VVV VVV VVV CQ DE W1AW K";

    private (int Moves, string Text) Decode(double toneHz, double startHz, double noise)
    {
        var audio = CwSignal.Generate(new CwSignalRequest(
            Message, WordsPerMinute: 18, ToneHz: toneHz, NoiseAmplitude: noise));

        var decoder = new CwDecoder(audio.SampleRate, startHz);
        var read = new System.Text.StringBuilder();

        decoder.CharacterDecoded += c => read.Append(c.Text);

        using var source = new Hamlet.RadioEngine.Audio.BufferedAudioSource(audio);
        decoder.Listen(source);
        source.PumpAll();
        decoder.Flush();

        var text = read.ToString().Trim();

        _output.WriteLine($"{toneHz:0} Hz from {startHz:0}, noise {noise}: "
            + $"{decoder.Tracker.Retunes} moves, '{text}'");

        return (decoder.Tracker.Retunes, text);
    }

    /// <remarks>
    /// <para>**THE CASE THE RULING WAS MEASURED ON.** A station at 400 hertz found
    /// from 600, in silence. The tracker used to reach it, leave it for its own
    /// image at 575, and come back — three moves — and the `CQ` was sent while it
    /// was away. It now reaches the station and stays there.</para>
    /// </remarks>
    [Fact]
    public void TheTrackerDoesNotLeaveAStationForItsOwnImage()
    {
        var run = Decode(400, 600, 0);

        Assert.Equal(1, run.Moves);
        Assert.EndsWith("CQ DE W1AW K", run.Text, StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>**AND THE FLOOR DOES NOT MAKE THE TRACKER DEAF TO A REAL MOVE.** The
    /// same start, the same silence, a station at each of the pitches
    /// `ASignalAtTheWrongPitchIsStillFound` covers: every one is found and read. A
    /// rule that stopped the tracker following anything would pass the test above
    /// and fail the feature.</para>
    /// <para>**350 HERTZ IS NOT IN THIS LIST AND WAS NEVER READ**, before this
    /// change or after it — two moves and a row of placeholders either way. It is
    /// fifty hertz off the bottom of the survey's range and it is HM-OPEN-034, not
    /// a hole this ruling made.</para>
    /// </remarks>
    [Theory]
    [InlineData(400)]
    [InlineData(500)]
    [InlineData(750)]
    [InlineData(875)]
    public void AStationElsewhereIsStillFound(double toneHz)
    {
        var run = Decode(toneHz, 600, 0);

        Assert.EndsWith("CQ DE W1AW K", run.Text, StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>Proves the floor is measured against the station being read rather
    /// than against a level in the abstract. Nothing has been confirmed at the
    /// start of a decode, so nothing can be abandoned, and the tracker is free to
    /// go wherever the survey points it however quiet that is.</para>
    /// </remarks>
    [Fact]
    public void NothingIsRefusedBeforeAnythingIsBeingRead()
    {
        // Two hundred hertz from where it is told to look, and quiet: it is found
        // from cold, which is the branch the floor may not touch.
        var run = Decode(400, 600, 0.06);

        Assert.EndsWith("CQ DE W1AW K", run.Text, StringComparison.Ordinal);
    }
}
