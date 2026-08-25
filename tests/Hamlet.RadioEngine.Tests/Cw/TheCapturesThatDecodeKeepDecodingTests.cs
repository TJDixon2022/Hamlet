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
/// <para>**THESE ARE FLOORS AND NOT ANSWER KEYS.** Most of these recordings have
/// not been adjudicated, so nothing here says what any station sent or how much
/// of it is right. What it says is how many characters the decoder produced when
/// the floor was set, which is the only guard available on audio nobody has
/// scored and is enough to catch the failure it exists for: a change that reads a
/// new recording by taking characters away from the ones that already worked.</para>
/// <para>Set 2026-08-20 over five recordings. **Widened 2026-08-25 to every
/// capture in the tree** and re-measured, because five of twenty-three was a
/// guard over a quarter of the evidence: `cw-2026-08-18-004507` was sitting at a
/// floor of 25 while producing 50, and `unadjudicated/cw-2026-08-18-003016` at 38
/// while producing 57. A floor that has become far too low is a floor to raise
/// with a measurement beside it, not one to leave sitting under an improvement
/// nobody noticed.</para>
/// <para>**FLOORS ONLY EVER RISE**, and never become equalities. A number here is
/// what the decoder produced on the day it was written down; the assertion is
/// that it never produces less.</para>
/// <para>**THE UNSURE COUNT IS RECORDED AND PRINTED AND IS DELIBERATELY NOT
/// ASSERTED**, and that is a departure from the instruction that widened this
/// harness, stated rather than made quietly. A `>=` floor on unsure would forbid
/// the decoder ever becoming more certain, which is the opposite of what the rest
/// of this file guards; a `&lt;=` ceiling on it would forbid the decoder ever
/// admitting doubt it currently hides, and HM-DEC-048 ranks a marked unknown
/// above a confident wrong letter. Neither direction is a property this project
/// wants held. What the number is for is the trade: a change that lifts one
/// recording's characters while turning another's into placeholders has traded
/// one failure for another, and printing both counts beside each other is what
/// makes that visible.</para>
/// </remarks>
public sealed class TheCapturesThatDecodeKeepDecodingTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the counts are printed.</param>
    public TheCapturesThatDecodeKeepDecodingTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// Every recording in the tree, with the counts it produced on the day its
    /// floor was set.
    /// </summary>
    /// <remarks>
    /// The three numbers are characters emitted, elements seen, and characters
    /// marked unsure. The first two are asserted as floors and the third is
    /// printed; see this class's own remarks for why.
    /// </remarks>
    public static TheoryData<string, int, int, int> Floors { get; } = new()
    {
        // Adjudicated or independently corroborated content.
        { "cw-2026-08-17-013347", 59, 108, 2 },
        { "cw-2026-08-17-134712", 63, 98, 10 },
        { "cw-2026-08-18-004507", 50, 118, 1 },
        { "unadjudicated/cw-2026-08-24-012403", 22, 65, 0 },

        // The seven W1AW propagation-bulletin captures of 2026-08-22.
        { "unadjudicated/cw-2026-08-22-031838", 57, 126, 3 },
        { "unadjudicated/cw-2026-08-22-031905", 42, 118, 6 },
        { "unadjudicated/cw-2026-08-22-031948", 34, 114, 3 },
        { "unadjudicated/cw-2026-08-22-032012", 44, 120, 1 },
        { "unadjudicated/cw-2026-08-22-032050", 53, 123, 9 },
        { "unadjudicated/cw-2026-08-22-032113", 55, 118, 8 },
        { "unadjudicated/cw-2026-08-22-032129", 66, 119, 1 },

        // Nothing adjudicated in any of these.
        { "cw-2026-08-17-013622", 55, 84, 0 },
        { "unadjudicated/cw-2026-08-18-003016", 57, 149, 3 },
        { "unadjudicated/cw-2026-08-18-003126", 54, 144, 6 },
        { "unadjudicated/cw-2026-08-18-003758", 63, 121, 10 },
        { "unadjudicated/cw-2026-08-23-001520", 5, 45, 1 },
        { "unadjudicated/cw-2026-08-23-001831", 55, 124, 10 },
        { "unadjudicated/cw-2026-08-23-001952", 75, 142, 13 },
        { "unadjudicated/cw-2026-08-23-002016", 75, 136, 17 },

        // Recordings that emit nothing today. A floor of nought asserts nothing
        // and records the state; what holds the silence property is
        // `ARecordingWithNoStationInItSaysNothing` (HM-DEC-120), not this.
        { "unadjudicated/cw-2026-08-20-014854", 0, 0, 0 },
        { "unadjudicated/cw-2026-08-20-014935", 0, 0, 0 },
        { "unadjudicated/cw-2026-08-22-014113", 0, 0, 0 },
        { "unadjudicated/cw-2026-08-22-014308", 0, 0, 0 },
    };

    /// <remarks>
    /// Proves §12.5: the recordings that decode still decode, by at least as much
    /// as they did when this was written.
    /// </remarks>
    /// <param name="name">The recording.</param>
    /// <param name="characters">What it emitted when the floor was set.</param>
    /// <param name="elements">What it saw when the floor was set.</param>
    /// <param name="unsure">What it marked when the floor was set, for the trade.</param>
    [Theory]
    [MemberData(nameof(Floors))]
    public void EachStillProducesWhatItDid(
        string name, int characters, int elements, int unsure)
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

        var report = decoder.Report;

        _output.WriteLine(
            $"{name}: {report.CharactersEmitted} characters against a floor of "
            + $"{characters}, {report.ElementsSeen} elements against {elements}, "
            + $"{report.CharactersUnsure} unsure where {unsure} were marked when "
            + $"the floor was set, at {report.ToneHz:0} Hz");

        Assert.True(
            report.CharactersEmitted >= characters,
            $"{name} fell from {characters} characters to {report.CharactersEmitted}");

        Assert.True(
            report.ElementsSeen >= elements,
            $"{name} fell from {elements} elements to {report.ElementsSeen}");
    }
}
