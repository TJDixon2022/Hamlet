using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// A change that reads one recording and quietly costs another is not a fix
/// (HM-DEC-091, §12.5).
/// </summary>
/// <remarks>
/// <para>**THESE ARE FLOORS AND NOT ANSWER KEYS.** None of these recordings has
/// been adjudicated, so nothing here says what any station sent or how much of it
/// is right. What it says is how many characters the decoder produced when the
/// floor was set, which is the only guard available on audio nobody has scored
/// and is enough to catch the failure it exists for: a change that reads a new
/// recording by taking characters away from the ones that already worked.</para>
/// <para>Set 2026-08-20. A floor that has become far too low is a floor to raise
/// with a measurement beside it, not one to leave sitting under an improvement
/// nobody noticed.</para>
/// </remarks>
public sealed class TheCapturesThatDecodeKeepDecodingTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the counts are printed.</param>
    public TheCapturesThatDecodeKeepDecodingTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// Every recording the decoder produced a transcript from, with the count it
    /// produced on the day the floor was set.
    /// </summary>
    public static TheoryData<string, int> Floors { get; } = new()
    {
        { "unadjudicated/cw-2026-08-18-003016", 38 },
        { "unadjudicated/cw-2026-08-18-003126", 34 },
        { "unadjudicated/cw-2026-08-18-003758", 14 },
        { "cw-2026-08-18-004507", 25 },
        { "cw-2026-08-17-013347", 8 },
    };

    /// <remarks>
    /// Proves §12.5: the recordings that decode still decode, by at least as much
    /// as they did when this was written.
    /// </remarks>
    /// <param name="name">The recording.</param>
    /// <param name="floor">What it produced when the floor was set.</param>
    [Theory]
    [MemberData(nameof(Floors))]
    public void EachStillProducesWhatItDid(string name, int floor)
    {
        var audio = WavAudio.Read(
            Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

        var decoder = new CwDecoder(audio.SampleRate, 600);
        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        var emitted = decoder.Report.CharactersEmitted;

        _output.WriteLine(
            $"{name}: {emitted} emitted against a floor of {floor}, "
            + $"{decoder.Report.ElementsSeen} elements at {decoder.Report.ToneHz:0} Hz");

        Assert.True(emitted >= floor, $"{name} fell from {floor} to {emitted}");
    }
}
