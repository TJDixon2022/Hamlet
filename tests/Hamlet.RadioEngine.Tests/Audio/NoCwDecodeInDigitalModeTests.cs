using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// Work instruction 238, task 5: in Digital mode the tap fills and the CW
/// decoder does not run.
/// </summary>
/// <remarks>
/// <para>**IT IS A TIDY-UP AND NOT A REPAIR, AND THAT MATTERS FOR HOW IT IS
/// READ.** Before task 2 the CW decode ran on the device's callback thread and
/// starved the tap, so running it on the Data tab was actively harmful. With the
/// decode on a worker it is arithmetic nobody reads. Skipping it removes a
/// moving part; it does not fix anything, and a later reader should not take
/// this test as evidence about the audio path.</para>
/// <para>**THE TAP MUST STILL FILL**, which is the half that would be easy to
/// break. FT8 reads the tap, and a Digital mode that stopped feeding it would
/// silence the thing the mode exists for.</para>
/// </remarks>
public sealed class NoCwDecodeInDigitalModeTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the counts are printed.</param>
    public NoCwDecodeInDigitalModeTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// With Digital set, the tap fills and the probabilistic decoder's element
    /// count does not move.
    /// </summary>
    /// <remarks>
    /// **REAL MORSE, SO THE DECODER HAS SOMETHING TO FIND.** Feeding silence
    /// would leave the element count at zero whether the skip worked or not,
    /// which is a test that passes for the wrong reason.
    /// </remarks>
    [Fact]
    public void DigitalModeTapsAndDoesNotDecode()
    {
        var audio = CwSignal.Generate(new CwSignalRequest(
            "CQ CQ DE W1AW W1AW K", WordsPerMinute: 18));

        var cw = Decode(audio, digital: false);
        var digital = Decode(audio, digital: true);

        _output.WriteLine("CW mode      : tap " + cw.Tapped
            + ", elements " + cw.Elements + ", text '" + cw.Text + "'");
        _output.WriteLine("Digital mode : tap " + digital.Tapped
            + ", elements " + digital.Elements + ", text '" + digital.Text + "'");

        // The tap is identical either way. This is the assertion that would
        // catch a Digital mode that stopped feeding FT8.
        Assert.Equal(cw.Tapped, digital.Tapped);
        Assert.True(digital.Tapped > 0, "the tap saw nothing in Digital mode");

        // And the decoder did not run.
        Assert.Equal(0, digital.Elements);
        Assert.Equal("", digital.Text);

        // The control: in CW mode the same audio does move the element count,
        // so the assertion above is about the mode and not about the fixture.
        Assert.True(cw.Elements > 0,
            "the CW control decoded nothing, so this test proves nothing about "
            + "Digital mode");
    }

    private static (long Tapped, long Elements, string Text) Decode(
        MonoAudio audio, bool digital)
    {
        var decoder = new CwDecoder(audio.SampleRate, 600) { DigitalMode = digital };
        var text = new System.Text.StringBuilder();

        decoder.CharacterSettled += c => text.Append(c.Text);

        const int chunk = 960;

        for (var at = 0; at + chunk <= audio.Samples.Length; at += chunk)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan(at, chunk)));
        }

        decoder.Flush();

        return (decoder.Tap.SamplesSeen, decoder.Report.ElementsSeen, text.ToString().Trim());
    }
}
