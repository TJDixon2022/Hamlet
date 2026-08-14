using Avalonia.Media;
using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// The CW terminal's shell-side half: the transcript that carries characters
/// from the audio thread to the screen, and the ink they are written in
/// (HM-DEC-048).
/// </summary>
public sealed class CwTerminalTests
{
    /// <remarks>
    /// THE END-TO-END CLAIM, made without a window. The training radio sends
    /// known text at a known speed, the decoder reads it, and the transcript
    /// fills with what was sent. This is the same wiring the ViewModel builds
    /// when a rig connects, so a break in it fails here rather than on Tim's
    /// screen.
    /// </remarks>
    [Fact]
    public void TheTrainingRadioFillsTheTranscriptWithWhatItIsSending()
    {
        const string sending = "CQ DE W1AW K";

        using var source = new TrainingAudioSource(sending, wordsPerMinute: 12);
        var decoder = new CwDecoder(source.SampleRate, 600);
        var transcript = new CwTranscript();

        decoder.CharacterDecoded += transcript.Append;
        decoder.Listen(source);

        // Fourteen seconds of audio, delivered with no timer and no waiting.
        source.PumpOnce(source.SampleRate * 14);
        decoder.Flush();

        Assert.Contains(sending, transcript.PlainText, StringComparison.Ordinal);
        Assert.False(transcript.IsEmpty);
        Assert.InRange(decoder.State.WordsPerMinute, 11, 13);
    }

    /// <remarks>
    /// Proves the transcript hands every character over exactly once and in
    /// order. It is the seam between the audio thread and the screen, so a
    /// dropped or repeated character here would look exactly like a decoding
    /// error and would be hunted for in the wrong place entirely.
    /// </remarks>
    [Fact]
    public void EveryCharacterIsHandedOverOnceAndInOrder()
    {
        var transcript = new CwTranscript();

        foreach (var text in new[] { "C", "Q", " ", "D", "E" })
        {
            transcript.Append(Character(text));
        }

        var first = new List<CwCharacter>();
        Assert.Equal(5, transcript.Drain(first));
        Assert.Equal("CQ DE", string.Concat(first.Select(c => c.Text)));

        // Drained means drained: a second pass gets nothing.
        var second = new List<CwCharacter>();
        Assert.Equal(0, transcript.Drain(second));
        Assert.Empty(second);

        // And the text survives for the summary, which does not consume it.
        Assert.Equal("CQ DE", transcript.PlainText);
        Assert.Equal(5, transcript.CharacterCount);
    }

    /// <remarks>
    /// Proves an evening left running does not grow without limit, and that a
    /// clear is visible to the control so two sessions never run together.
    /// </remarks>
    [Fact]
    public void HistoryIsBoundedAndAClearIsNoticed()
    {
        var transcript = new CwTranscript();
        var before = transcript.Version;

        for (var i = 0; i < CwTranscript.MaximumCharacters * 3; i++)
        {
            transcript.Append(Character("E"));
        }

        Assert.True(
            transcript.CharacterCount <= CwTranscript.MaximumCharacters * 2,
            $"the transcript grew to {transcript.CharacterCount}");

        transcript.Clear();

        Assert.True(transcript.IsEmpty);
        Assert.NotEqual(before, transcript.Version);
    }

    /// <remarks>
    /// Proves the summary tells the truth when the panel is shut (§0.5). The
    /// moment a decode is going badly is exactly the moment somebody collapses
    /// the panel and concludes the app does not work, so the tail of what was
    /// heard travels into the header rather than being hidden with the detail.
    /// </remarks>
    [Fact]
    public void TheTailIsWhatACollapsedPanelWouldShow()
    {
        var transcript = new CwTranscript();

        foreach (var c in "CQ CQ DE W1AW K")
        {
            transcript.Append(Character(c.ToString()));
        }

        Assert.Equal("W1AW K", transcript.Tail(6));
        Assert.Equal("CQ CQ DE W1AW K", transcript.Tail(500));
    }

    /// <remarks>
    /// Proves the transcript survives being written from one thread while being
    /// read from another, which is what it is for. Audio arrives on whichever
    /// thread the device chose and the screen drains on the UI thread, and a
    /// torn read here would corrupt a transcript rather than throw.
    /// </remarks>
    [Fact]
    public async Task WritingAndDrainingAtOnceLosesNothing()
    {
        const int total = 5_000;

        var transcript = new CwTranscript();
        var seen = new List<CwCharacter>();

        var writer = Task.Run(() =>
        {
            for (var i = 0; i < total; i++)
            {
                transcript.Append(Character("E"));
            }
        });

        while (!writer.IsCompleted || seen.Count < total)
        {
            transcript.Drain(seen);
        }

        await writer;
        transcript.Drain(seen);

        Assert.Equal(total, seen.Count);
    }

    /// <remarks>
    /// EVERY INK CLEARS WCAG AA AGAINST ITS OWN SURFACE, with no exceptions
    /// (§0.6, HM-DEC-036). It binds here as hard as anywhere: a character marked
    /// uncertain has to be dimmer AND still readable, because a character nobody
    /// can read is not a marked character, it is a missing one. The first
    /// attempt measured 3.8 to 1.
    /// </remarks>
    [Fact]
    public void EveryInkOnTheInstrumentSurfaceClearsAa()
    {
        foreach (var ink in InstrumentPalette.Inks)
        {
            var ratio = ContrastRatio(ink, InstrumentPalette.Surface);

            Assert.True(
                ratio >= 4.5,
                $"{ink} measures {ratio:0.00} against the instrument surface");
        }
    }

    /// <remarks>
    /// COLOR IS NEVER THE ONLY CARRIER (§0.6), and here the second carrier is
    /// brightness, which is what a grayscale print leaves behind. A character
    /// the decoder is unsure of has to be visibly dimmer than one it is sure of,
    /// or the marking says nothing to somebody who cannot separate the hues.
    /// </remarks>
    [Fact]
    public void UncertainTextIsVisiblyDimmerThanConfidentText()
    {
        var confident = Luminance(InstrumentPalette.Confident);
        var uncertain = Luminance(InstrumentPalette.Uncertain);

        Assert.True(
            uncertain < confident * 0.6,
            $"uncertain is {uncertain / confident:P0} of confident, which reads as the same");
    }

    /// <remarks>
    /// Proves the three states get three different inks. Two that matched would
    /// quietly collapse the model back into a decoder that only says yes.
    /// </remarks>
    [Fact]
    public void EachConfidenceHasItsOwnInk()
    {
        var inks = Enum.GetValues<CwConfidence>()
            .Select(InstrumentPalette.For)
            .ToList();

        Assert.Equal(inks.Count, inks.Distinct().Count());
    }

    private static CwCharacter Character(string text)
        => new(text, CwConfidence.High, 1, ".", 30, 18, TimeSpan.FromSeconds(1));

    private static double ContrastRatio(Color a, Color b)
    {
        var la = Luminance(a);
        var lb = Luminance(b);

        return (Math.Max(la, lb) + 0.05) / (Math.Min(la, lb) + 0.05);
    }

    private static double Luminance(Color color)
    {
        static double Channel(byte v)
        {
            var s = v / 255.0;
            return s <= 0.03928 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
        }

        return (0.2126 * Channel(color.R))
            + (0.7152 * Channel(color.G))
            + (0.0722 * Channel(color.B));
    }
}
