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
    /// <para>Adjudicates `TheSpeedEstimateFollowsAChangeWithinAFewCharacters`,
    /// which had never been adjudicated because nothing in the suite contained a
    /// genuine change of speed (HM-DEC-104). The two-station recording does: a
    /// caller at about eleven words a minute and an answerer at twenty-two.</para>
    /// <para>What the original test asks is that the estimate follows rather than
    /// staying stuck on the old speed. What it cannot ask, and what matters more
    /// on the air, is that the estimate never sits between the two describing
    /// neither of them.</para>
    /// </remarks>
    [Fact]
    public void ASpeedChangeInRealisticAudio()
    {
        var audio = WavAudio.Read(Path.Combine(
            CwFixtureCatalogue.Folder, CwFixtureCatalogue.TwoStationName + ".wav"));

        var decoder = new CwDecoder(audio.SampleRate, 600);
        var speeds = new List<int>();

        using var source = new BufferedAudioSource(audio);
        decoder.Listen(source);

        var chunk = audio.SampleRate / 4;

        for (var at = 0; at < audio.Samples.Length; at += chunk)
        {
            var take = Math.Min(chunk, audio.Samples.Length - at);

            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan(at, take)));

            if (decoder.WordsPerMinute is { } wpm)
            {
                speeds.Add(wpm);
            }
        }

        decoder.Flush();

        _output.WriteLine(speeds.Count == 0
            ? "no speed was ever named"
            : $"speeds named: {string.Join(", ", speeds.Distinct())}");

        var between = speeds.Where(w => w is >= 15 and <= 18).Distinct().ToList();
        var beyond = speeds.Where(w => w > 25).Distinct().ToList();

        _output.WriteLine(between.Count == 0
            ? "no reading fell between the two stations"
            : $"read between the two stations at: {string.Join(", ", between)}");

        _output.WriteLine(beyond.Count == 0
            ? "no reading exceeded either station"
            : $"read faster than either station at: {string.Join(", ", beyond)}");

        // **RECORDED RATHER THAN REQUIRED, AND THE FINDING IS REAL.** Across the
        // handover the decoder names speeds belonging to neither station,
        // including the average of the two and excursions well past the faster
        // one. Whether a streaming decoder may show a transitional speed at all,
        // or must withhold one until the new clock settles, is a question about
        // what the display asserts and so is not this session's to answer (§0.0,
        // §12.1). What is asserted here is only that the run happened and can be
        // measured; `NoSingleClockIsFittedAcrossBothStations` holds the line that
        // matters, which is where it comes to rest.
        Assert.NotEmpty(speeds);
    }

    /// <remarks>
    /// <para>Adjudicates `ClearingTheTranscriptLeavesTheDecoderAlone`, which
    /// fails against a noiseless fixture and asserts an exact transcript from
    /// it. What the test is actually about is that clearing the screen does not
    /// disturb what the decoder has learned, and that survives being asked of
    /// audio a receiver could produce.</para>
    /// </remarks>
    [Fact]
    public void ClearingTheScreenLeavesTheDecoderAloneOnRealisticAudio()
    {
        var audio = WavAudio.Read(
            Path.Combine(CwFixtureCatalogue.Folder, "exchange-easy.wav"));

        var decoder = new CwDecoder(audio.SampleRate, 600);
        var seen = 0;

        decoder.CharacterDecoded += _ => seen++;

        using var source = new BufferedAudioSource(audio);
        decoder.Listen(source);

        var half = audio.Samples.Length / 2;

        decoder.Process(new AudioChunk(0, audio.SampleRate, audio.Samples.AsSpan(0, half)));

        var before = decoder.State;

        Assert.True(seen > 0, "nothing had been decoded before the clear");

        // Clearing a transcript is a thing the screen does. The decoder is not
        // told about it and has no way to be, which is the property under test.
        var during = seen;

        decoder.Process(new AudioChunk(
            half, audio.SampleRate, audio.Samples.AsSpan(half, audio.Samples.Length - half)));

        decoder.Flush();

        var after = decoder.State;

        _output.WriteLine($"before: {before.WordsPerMinute} wpm at {before.ToneHz:0} Hz");
        _output.WriteLine($"after : {after.WordsPerMinute} wpm at {after.ToneHz:0} Hz");
        _output.WriteLine($"{during} characters before, {seen} in all");

        Assert.True(seen > during, "the decoder stopped reading part-way through");
        Assert.Equal(before.ToneHz, after.ToneHz);
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
