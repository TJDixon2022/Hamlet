using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The two presses on the 19th produce nothing, and a change that starts
/// producing something from them has gone wrong (§0.0, HM-DEC-048).
/// </summary>
/// <remarks>
/// <para>**THESE ARE THE RECORDINGS AN INDEPENDENT INSTRUMENT SAYS CONTAIN NO
/// KEYING AT ALL.** Swept 400 to 1200 Hz in 25 Hz steps, `KeyingEnvelope` reads
/// medians of 5 and 7 milliseconds across every window of both, against 44 to 57
/// on the four recordings that decoded. Fifteen hundred key-downs at a
/// six-millisecond median is a threshold being crossed by noise.</para>
/// <para>**SO THE HONEST OUTPUT IS SILENCE, AND MARKING A CHARACTER IS NOT A
/// SUBSTITUTE FOR NOT EMITTING IT** (HM-DEC-090). Seventeen hundred characters
/// once came out of half a minute of band noise, every one of them marked unsure,
/// and the marking was not enough: a screen of blocks and dimmed letters reads as
/// a signal being fought over rather than as nothing being there.</para>
/// <para>**THIS IS A GUARD AND NOT A RATCHET.** It was written the session two
/// separate candidate changes were measured and both began emitting here: one
/// took `cw-2026-08-20-014854` from one character to five and the pair together
/// put three placeholders on `cw-2026-08-20-014935`, which had never produced
/// anything. Both were withdrawn. The one character on `-014854` predates all of
/// it and is allowed for rather than approved of.</para>
/// </remarks>
public sealed class NothingIsReadFromAudioWithNoKeyingTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the counts are printed.</param>
    public NothingIsReadFromAudioWithNoKeyingTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// The two presses on the 19th, with what each produced when this was
    /// written.
    /// </summary>
    /// <remarks>
    /// **NEITHER FIGURE IS A TARGET.** Nought is the right answer for both, and
    /// the one on `-014854` is a defect this suite carries rather than a
    /// behaviour it endorses.
    /// </remarks>
    public static TheoryData<string, int> Presses { get; } = new()
    {
        { "unadjudicated/cw-2026-08-20-014854", 1 },
        { "unadjudicated/cw-2026-08-20-014935", 0 },
    };

    /// <remarks>
    /// Proves §0.0: audio with nothing being keyed in it produces nothing, and a
    /// change that lifts either count has made the decoder willing to guess.
    /// </remarks>
    /// <param name="name">The recording.</param>
    /// <param name="ceiling">What it produced when this was written.</param>
    [Theory]
    [MemberData(nameof(Presses))]
    public void NeitherPressProducesMoreThanItDid(string name, int ceiling)
    {
        var audio = WavAudio.Read(
            Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

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

        var emitted = decoder.Report.CharactersEmitted;

        _output.WriteLine(
            $"{name}: {emitted} emitted against a ceiling of {ceiling}, "
            + $"{decoder.Report.CharactersUnsure} unsure, "
            + $"{decoder.Report.ElementsSeen} elements at {decoder.Report.ToneHz:0} Hz");
        _output.WriteLine("    " + string.Concat(text));

        Assert.True(
            emitted <= ceiling,
            $"{name} went from {ceiling} to {emitted} characters, and an "
            + "independent instrument finds no keying in it at any pitch");
    }

    /// <remarks>
    /// Proves §12.5: **the instrument that says these are empty**, re-measured
    /// here rather than quoted, so the ceiling above rests on something in the
    /// tree. It shares no code with the decoder.
    /// </remarks>
    /// <param name="name">The recording.</param>
    /// <param name="ceiling">Unused here; the theory data is shared.</param>
    [Theory]
    [MemberData(nameof(Presses))]
    public void AnIndependentInstrumentFindsNoKeyingInEither(string name, int ceiling)
    {
        _ = ceiling;

        var audio = WavAudio.Read(
            Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

        var length = (int)(audio.SampleRate * CwKeyingThresholds.Window.TotalSeconds);
        var worst = 0.0;

        for (var start = 0; start + length <= audio.Samples.Length; start += length)
        {
            var slice = new float[length];

            Array.Copy(audio.Samples, start, slice, 0, length);

            if (KeyingEnvelope.Best(new MonoAudio(audio.SampleRate, slice)) is { } window)
            {
                _output.WriteLine(
                    $"  {window.ToneHz:0} Hz, {window.Profile.MedianMs:0} ms, "
                    + $"score {window.Profile.Score:0.00}");

                worst = Math.Max(worst, window.Profile.MedianMs);
            }
        }

        Assert.True(
            worst < CwKeyingThresholds.SlowestChatterMs,
            $"{name} had a window with a {worst:0} ms key-down in it");
    }
}
