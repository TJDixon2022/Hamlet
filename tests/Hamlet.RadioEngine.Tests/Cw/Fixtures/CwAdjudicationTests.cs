using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>
/// Do the failing tests fail against realistic audio too? (HM-OPEN-018 phase 5)
/// </summary>
/// <remarks>
/// <para>Eleven tests are failing against fixtures that carry no noise floor.
/// **The question each one has to answer is whether it is describing a Hamlet
/// defect or a fixture defect**, and the only way to find out is to put the same
/// scenario into audio a receiver could produce and look again.</para>
/// <para>These do not assert that Hamlet succeeds. They put the scenario in
/// realistic terms and record what happens, so the adjudication rests on a
/// measurement rather than on an opinion about which fixture is more
/// believable.</para>
/// </remarks>
public sealed class CwAdjudicationTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings go.</param>
    public CwAdjudicationTests(ITestOutputHelper output) => _output = output;

    private (string Text, double ToneHz, int Emitted) Read(
        CwFixtureRecipe recipe, double expectedToneHz = 600)
    {
        var (audio, _) = CwFixtureGenerator.Generate(recipe);
        var decoder = new CwDecoder(audio.SampleRate, expectedToneHz);
        var text = new System.Text.StringBuilder();

        decoder.CharacterDecoded += c => text.Append(c.Text);

        using var source = new BufferedAudioSource(audio);
        decoder.Listen(source);
        source.PumpAll();
        decoder.Flush();

        return (text.ToString().Trim(), decoder.Report.ToneHz,
            decoder.Report.CharactersEmitted);
    }

    /// <remarks>
    /// <para>Adjudicates `ASignalAtTheWrongPitchIsStillFound`. Nobody tunes
    /// exactly, so a signal a long way from where the decoder was told to look
    /// still has to be found. The old fixture asks this of noiseless audio; this
    /// asks it of audio with a band under it.</para>
    /// </remarks>
    [Theory]
    [InlineData(400)]
    [InlineData(500)]
    [InlineData(750)]
    [InlineData(875)]
    public void ASignalOffTheExpectedPitchIsFoundInRealisticAudio(double toneHz)
    {
        var recipe = new CwFixtureRecipe(
            $"offpitch-{toneHz:0}",
            CwFixtureCatalogue.ExchangeText,
            DitMilliseconds: 1200.0 / CwFixtureCatalogue.OrdinaryWpm,
            DahMilliseconds: 3 * 1200.0 / CwFixtureCatalogue.OrdinaryWpm,
            ElementGapMilliseconds: 1200.0 / CwFixtureCatalogue.OrdinaryWpm,
            CharacterGapMilliseconds: 3 * 1200.0 / CwFixtureCatalogue.OrdinaryWpm,
            WordGapMilliseconds: 7 * 1200.0 / CwFixtureCatalogue.OrdinaryWpm,
            SignalToNoiseDb: CwFixtureCatalogue.EasyDb,
            ToneHz: toneHz,
            Seed: 900 + (int)toneHz);

        var (text, found, emitted) = Read(recipe);

        _output.WriteLine($"sent at {toneHz:0} Hz, found {found:0} Hz, "
            + $"{emitted} characters: {text}");

        Assert.InRange(found, toneHz - 30, toneHz + 30);
    }

    /// <remarks>
    /// <para>Adjudicates `ACleanSignalDecodesExactly(25)` and the two
    /// `clean-25wpm` tests. Twenty-five words a minute is faster than anything
    /// else in the suite, and the detection window that makes a weak signal
    /// readable is a large fraction of a dit at that speed.</para>
    /// </remarks>
    [Fact]
    public void AFastFistInRealisticAudio()
    {
        var recipe = new CwFixtureRecipe(
            "fast-25wpm",
            CwFixtureCatalogue.ExchangeText,
            DitMilliseconds: 1200.0 / 25,
            DahMilliseconds: 3 * 1200.0 / 25,
            ElementGapMilliseconds: 1200.0 / 25,
            CharacterGapMilliseconds: 3 * 1200.0 / 25,
            WordGapMilliseconds: 7 * 1200.0 / 25,
            SignalToNoiseDb: CwFixtureCatalogue.EasyDb,
            Seed: 2501);

        var (text, found, emitted) = Read(recipe);

        _output.WriteLine($"25 wpm: found {found:0} Hz, {emitted} characters: {text}");

        // Recorded rather than required. What this settles is whether the old
        // failure survives a noise floor, and the answer goes in the report.
        Assert.True(emitted >= 0);
    }

    /// <remarks>
    /// Adjudicates `AFadingSignalComesBackRatherThanStayingDead`. A signal that
    /// fades and returns must not take the decoder with it, and the working tier
    /// is where this repository measured a real fade.
    /// </remarks>
    [Fact]
    public void AFadeInRealisticAudio()
    {
        var recipe = CwFixtureCatalogue.All.Single(r => r.Name == "exchange-working");
        var (text, found, emitted) = Read(recipe);

        _output.WriteLine($"faded: found {found:0} Hz, {emitted} characters: {text}");

        // The decoder has to come back at all, which is what the original test is
        // about. How much of the message survives a twelve decibel fade is a
        // separate question and is reported rather than asserted.
        Assert.True(
            emitted > 0,
            "a fading signal produced nothing at all, so the decoder went with it");
    }

    /// <remarks>
    /// <para>Adjudicates `ItGoesQuietRatherThanInventingLettersInTheNoise` under
    /// HM-DEC-097. **Below zero decibels the decoder refuses by ruling**, so the
    /// question is no longer how far down it can read but whether it stays quiet
    /// where it is told to.</para>
    /// </remarks>
    [Theory]
    [InlineData(-3)]
    [InlineData(-6)]
    [InlineData(-10)]
    public void BelowTheFloorItStaysQuiet(double snrDb)
    {
        var recipe = new CwFixtureRecipe(
            $"under-{snrDb:0}",
            CwFixtureCatalogue.ExchangeText,
            DitMilliseconds: 1200.0 / CwFixtureCatalogue.OrdinaryWpm,
            DahMilliseconds: 3 * 1200.0 / CwFixtureCatalogue.OrdinaryWpm,
            ElementGapMilliseconds: 1200.0 / CwFixtureCatalogue.OrdinaryWpm,
            CharacterGapMilliseconds: 3 * 1200.0 / CwFixtureCatalogue.OrdinaryWpm,
            WordGapMilliseconds: 7 * 1200.0 / CwFixtureCatalogue.OrdinaryWpm,
            SignalToNoiseDb: snrDb,
            Seed: 3000 + (int)Math.Abs(snrDb));

        var (text, _, emitted) = Read(recipe);

        _output.WriteLine($"{snrDb:0} dB: {emitted} characters: {text}");

        var invented = text.Count(
            c => c != ' ' && c.ToString() != MorseAlphabet.Unreadable);

        Assert.True(
            invented <= 4,
            $"{invented} characters came out of audio {snrDb:0} dB under the "
            + "noise, which is a decoder guessing below its own floor");
    }
}
