using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// When the frequency changes, the decoder starts fresh — and nothing it knew
/// about the old station survives into the new one.
/// </summary>
/// <remarks>
/// <para>**TIM'S RULING OF 2026-08-29: when the frequency changes, clear and
/// reset.** On the evening of 2026-08-29 the operator moved 7.0284 to 7.0372 to
/// 7.0502 MHz and the decoder arrived at each new frequency holding a memory of
/// the last one.</para>
/// <para>**WHAT WAS ALREADY RIGHT, AND WHAT WAS NOT.**
/// <see cref="CwDecoder.Retuned"/> already existed and already cleared the
/// measured pitch, the operator's lock and the held peak — that is HM-DEC-111,
/// and the work order's fault 4 is wrong to say nothing reset. **What did
/// survive was the reading itself**: the twelve-second envelope window, the
/// speed hypothesis fitted to it, the settled mark, and the element and
/// character counters a capture sheet reports. So a sheet written after a move
/// described this frequency in every field but those.</para>
/// </remarks>
public sealed class AMoveStartsTheDecoderFreshTests
{
    private readonly ITestOutputHelper _output;

    public AMoveStartsTheDecoderFreshTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>Everything the decoder had concluded is gone after a move.</summary>
    /// <remarks>
    /// **THE STATE AFTER A FREQUENCY CHANGE IS THE STATE IT HAS WHEN IT FIRST
    /// BEGINS LISTENING**, which is what makes the next sidecar a description of
    /// the new frequency rather than of the evening (§0.0.1).
    /// </remarks>
    [Fact]
    public void NothingTheDecoderLearnedSurvivesTheMove()
    {
        var decoder = Read(Signal("CQ CQ DE W1AW K"));
        var before = decoder.Report;

        _output.WriteLine(
            $"before: {before.CharactersEmitted} characters, "
            + $"{before.ElementsResolved} elements, tone {before.ToneHz:0}, "
            + $"snr {before.SnrDb:0.0}, speed {decoder.Reading.WordsPerMinute:0.0}");

        Assert.True(
            before.CharactersEmitted > 0,
            "the fixture produced nothing, so this proves nothing");

        decoder.Retuned();

        var after = decoder.Report;

        _output.WriteLine(
            $"after : {after.CharactersEmitted} characters, "
            + $"{after.ElementsResolved} elements, tone {after.ToneHz:0}, "
            + $"snr {after.SnrDb:0.0}, speed {decoder.Reading.WordsPerMinute:0.0}");

        Assert.Equal(0, after.CharactersEmitted);
        Assert.Equal(0, after.CharactersUnsure);
        Assert.Equal(0, after.ElementsResolved);
        Assert.False(after.HasTone);

        // The held peak is not a figure about this frequency any more.
        Assert.True(
            double.IsNaN(after.SnrDb),
            $"the held peak survived the move at {after.SnrDb:0.0} dB");

        // And the reading itself, which used to carry across.
        Assert.Empty(decoder.Reading.Characters);
        Assert.Equal(string.Empty, decoder.Reading.Text ?? string.Empty);
    }

    /// <summary>
    /// A fresh decoder and one that has been retuned are in the same state.
    /// </summary>
    /// <remarks>
    /// The ruling's own words are the test: *the decoder's state after a
    /// frequency change is the state it has when it first begins listening.*
    /// Comparing against a decoder that has never heard anything is the only way
    /// to check that without listing every field and missing one.
    /// </remarks>
    [Fact]
    public void ARetunedDecoderMatchesOneThatHasNeverListened()
    {
        var moved = Read(Signal("CQ CQ DE W1AW K"));

        moved.Retuned();

        var fresh = new CwDecoder(48_000, 600);

        Assert.Equal(fresh.Report.CharactersEmitted, moved.Report.CharactersEmitted);
        Assert.Equal(fresh.Report.ElementsResolved, moved.Report.ElementsResolved);
        Assert.Equal(fresh.Report.HasTone, moved.Report.HasTone);
        Assert.Equal(fresh.Report.HasKeying, moved.Report.HasKeying);
        Assert.Equal(
            fresh.Reading.Characters.Count, moved.Reading.Characters.Count);
        Assert.Equal(fresh.IsLocked, moved.IsLocked);
        Assert.Equal(fresh.PitchWasAsserted, moved.PitchWasAsserted);
        Assert.Equal(fresh.Ranked.Ranked, moved.Ranked.Ranked);
    }

    /// <summary>The decoder goes on reading normally after a move.</summary>
    /// <remarks>
    /// **A RESET THAT LEAVES THE DECODER UNABLE TO READ IS NOT A RESET.** The
    /// window is emptied, so there is a refill to wait through, and after it the
    /// new station reads exactly as it would have read on a decoder that started
    /// there.
    /// </remarks>
    [Fact]
    public void TheDecoderReadsTheNewStationAfterTheMove()
    {
        var decoder = Read(Signal("CQ CQ DE W1AW K"));

        decoder.Retuned();

        var second = Signal("CQ CQ DE K2ABC K");
        var text = new System.Text.StringBuilder();

        decoder.CharacterSettled += c => text.Append(c.Text);

        Feed(decoder, second);

        _output.WriteLine($"after the move: {text}");

        Assert.True(
            decoder.Report.CharactersEmitted > 0,
            "nothing at all was read after the move, so the reset broke the "
            + "decoder rather than clearing it");
    }

    /// <summary>Generated Morse at 500 Hz inside a receiver's own passband.</summary>
    private static MonoAudio Signal(string text)
        => CwFixtureGenerator.Generate(new CwFixtureRecipe(
            Name: "retune",
            Text: text,
            ToneHz: 500,
            SignalToNoiseDb: 18)).Audio;

    private static CwDecoder Read(MonoAudio audio)
    {
        var decoder = new CwDecoder(audio.SampleRate, 600);

        Feed(decoder, audio);

        return decoder;
    }

    private static void Feed(CwDecoder decoder, MonoAudio audio)
    {
        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();
    }
}
