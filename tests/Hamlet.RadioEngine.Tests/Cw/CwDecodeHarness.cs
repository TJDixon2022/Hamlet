using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>What a decode run produced, with everything needed to judge it.</summary>
/// <param name="Characters">Every character in order, word gaps included.</param>
/// <param name="Text">The transcript as the operator would read it.</param>
/// <param name="Report">What was arriving while it ran.</param>
internal sealed record CwDecodeResult(
    IReadOnlyList<CwCharacter> Characters,
    string Text,
    CwDecodeReport Report = default)
{
    /// <summary>Characters only, dropping the word gaps.</summary>
    public IReadOnlyList<CwCharacter> Letters
        => Characters.Where(c => !c.IsWordGap).ToList();

    /// <summary>Characters the decoder was sure about.</summary>
    public IReadOnlyList<CwCharacter> Confident
        => Letters.Where(c => c.Confidence == CwConfidence.High).ToList();

    /// <summary>The share of characters that failed to resolve at all.</summary>
    public double UnreadableShare
        => Letters.Count == 0
            ? 0
            : (double)Letters.Count(c => c.IsUnreadable) / Letters.Count;

    /// <summary>The speed the decoder settled on, or nought.</summary>
    /// <remarks>
    /// **NAMED BY THE ONE DECODER THERE IS.** It used to come from the run-length
    /// estimator's own state record, which went with that decoder.
    /// </remarks>
    public int WordsPerMinute { get; init; }
}

/// <summary>
/// Runs audio through the decoder the way the app does, with no clock and no
/// sound card.
/// </summary>
/// <remarks>
/// Everything is pushed through <see cref="BufferedAudioSource"/> so the tests
/// exercise the same path a real capture takes, rather than reaching past the
/// seam into the decoder's internals. A test that skipped the source would stop
/// proving that the source works.
/// </remarks>
internal static class CwDecodeHarness
{
    /// <summary>Decode a generated signal.</summary>
    /// <param name="request">What to send.</param>
    /// <param name="expectedToneHz">Where to tell the decoder to start looking.</param>
    /// <returns>The result.</returns>
    public static CwDecodeResult Decode(
        CwSignalRequest request,
        double expectedToneHz = CwSignal.DefaultToneHz)
        => Decode(CwSignal.Generate(request), expectedToneHz);

    /// <summary>Decode audio.</summary>
    /// <param name="audio">The audio.</param>
    /// <param name="expectedToneHz">Where to tell the decoder to start looking.</param>
    /// <returns>The result.</returns>
    public static CwDecodeResult Decode(
        MonoAudio audio,
        double expectedToneHz = CwSignal.DefaultToneHz)
    {
        var decoder = new CwDecoder(audio.SampleRate, expectedToneHz);

        var characters = new List<CwCharacter>();
        decoder.CharacterDecoded += characters.Add;

        using var source = new BufferedAudioSource(audio);
        decoder.Listen(source);
        source.PumpAll();
        decoder.Flush();

        var text = new StringBuilder();
        foreach (var c in characters)
        {
            text.Append(c.Text);
        }

        return new CwDecodeResult(
            characters, text.ToString().Trim(), decoder.Report)
        {
            WordsPerMinute = (int)Math.Round(decoder.Reading.WordsPerMinute),
        };
    }
}
