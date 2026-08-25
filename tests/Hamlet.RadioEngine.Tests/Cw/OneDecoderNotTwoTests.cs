using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The decode is a fact about the audio, not about the size of the buffer the
/// sound card hands over.
/// </summary>
/// <remarks>
/// <para>**THE SUITE AND THE OPERATOR WERE READING TWO DIFFERENT DECODERS.**
/// `CwDecoder.Process` set the mixer's pitch once per chunk from the tracker's
/// state after the tracker had consumed that whole chunk, and then mixed the
/// whole chunk down at that one pitch. With a chunk four hops long the first
/// three hops were mixed at a pitch the tracker only reached at the end of the
/// fourth.</para>
/// <para>The application feeds 960 samples at a time through
/// <see cref="BufferedAudioSource"/> and the floors harness feeds 240, so the two
/// disagreed. Measured on `cw-2026-08-22-032113` before the repair: fed 240 the
/// decoder tracked 650 Hz, fed 960 it tracked 500, and the text moved with it.
/// A floor set through one and a defect found through the other are not
/// statements about the same instrument (§12.5, HM-DEC-119).</para>
/// <para>**IT IS FIXED BY CONSTRUCTION RATHER THAN BY AGREEMENT.** The decoder
/// walks the audio a hop at a time whatever size it arrived in, so the pitch
/// handed to the mixer is the pitch that was in force for the audio being mixed.
/// These tests hold that: the same recording read through five buffer sizes,
/// from one hop to twenty, gives one answer.</para>
/// </remarks>
public sealed class OneDecoderNotTwoTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public OneDecoderNotTwoTests(ITestOutputHelper output) => _output = output;

    /// <summary>Every recording made on the air.</summary>
    public static TheoryData<string> Captures
    {
        get
        {
            var data = new TheoryData<string>();
            var root = CapturedSignalTests.Folder;

            foreach (var wav in Directory.GetFiles(root, "*.wav").OrderBy(p => p))
            {
                data.Add(Path.GetFileNameWithoutExtension(wav));
            }

            foreach (var wav in Directory
                .GetFiles(Path.Combine(root, "unadjudicated"), "*.wav")
                .OrderBy(p => p))
            {
                data.Add("unadjudicated/" + Path.GetFileNameWithoutExtension(wav));
            }

            return data;
        }
    }

    private static (string Text, double ToneHz) Read(MonoAudio audio, int chunk)
    {
        var decoder = new CwDecoder(
            audio.SampleRate, TheAdjudicatedReadingsKeepReadingTests.RadioPitchHz);

        var text = new System.Text.StringBuilder();

        decoder.CharacterSettled += c => text.Append(c.Text);

        for (var at = 0L; at < audio.Samples.Length; at += chunk)
        {
            var take = (int)Math.Min(chunk, audio.Samples.Length - at);

            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, take)));
        }

        decoder.Flush();

        return (text.ToString(), decoder.Report.ToneHz);
    }

    /// <remarks>
    /// Proves the property directly: one recording, five buffer sizes spanning
    /// one hop to twenty, one answer.
    /// </remarks>
    /// <param name="name">The recording.</param>
    [Theory]
    [MemberData(nameof(Captures))]
    public void TheBufferSizeChangesNothing(string name)
    {
        var audio = WavAudio.Read(
            Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

        var sizes = new[] { 240, 480, 960, 1920, 4800 };
        var first = Read(audio, sizes[0]);

        _output.WriteLine(
            $"{name}: {first.ToneHz:0.0} Hz, {first.Text.Length} characters");

        foreach (var size in sizes.Skip(1))
        {
            var other = Read(audio, size);

            Assert.True(
                other.ToneHz == first.ToneHz,
                $"{name} tracks {first.ToneHz:0.0} Hz in chunks of {sizes[0]} and "
                + $"{other.ToneHz:0.0} Hz in chunks of {size}");

            Assert.True(
                other.Text == first.Text,
                $"{name} reads differently in chunks of {size}:\n"
                + $"  {sizes[0]}: {first.Text}\n  {size}: {other.Text}");
        }
    }

    /// <remarks>
    /// **AND THE TWO ENTRY POINTS THE APPLICATION AND THE SUITE ACTUALLY USE.**
    /// `Listen` with a buffered source is what production runs and `Process` in
    /// hop-sized chunks is what most of this suite runs, so they are named
    /// explicitly rather than left implied by the sizes above.
    /// </remarks>
    /// <param name="name">The recording.</param>
    [Theory]
    [MemberData(nameof(Captures))]
    public void ListeningAndFeedingReadTheSame(string name)
    {
        var audio = WavAudio.Read(
            Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

        var fed = Read(audio, 240);

        var decoder = new CwDecoder(
            audio.SampleRate, TheAdjudicatedReadingsKeepReadingTests.RadioPitchHz);

        var heard = new System.Text.StringBuilder();

        decoder.CharacterSettled += c => heard.Append(c.Text);

        using (var source = new BufferedAudioSource(audio))
        {
            decoder.Listen(source);
            source.PumpAll();
        }

        decoder.Flush();

        _output.WriteLine(
            $"{name}: fed {fed.ToneHz:0.0} Hz, heard {decoder.Report.ToneHz:0.0} Hz");

        Assert.True(
            decoder.Report.ToneHz == fed.ToneHz,
            $"{name} tracks {fed.ToneHz:0.0} Hz when fed and "
            + $"{decoder.Report.ToneHz:0.0} Hz when listening");

        Assert.True(
            heard.ToString() == fed.Text,
            $"{name} reads differently when listening:\n"
            + $"  fed:   {fed.Text}\n  heard: {heard}");
    }
}
