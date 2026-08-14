using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The CW decoder, against generated signals with known text at known speeds
/// (HM-DEC-007, HM-DEC-048).
/// </summary>
/// <remarks>
/// Every case here runs through <see cref="BufferedAudioSource"/> and the whole
/// chain, with no clock and no sound card. Same audio in, same text out, every
/// time, which is the only reason any of these assertions can be exact (§5).
/// </remarks>
public sealed class CwDecoderTests
{
    private const string Call = "CQ DE W1AW K";

    /// <remarks>
    /// THE FLOOR THE WHOLE FEATURE STANDS ON. A clean signal decodes to exactly
    /// what was sent, at three speeds spanning what a newcomer will meet, and
    /// the decoder is sure about all of it. Determinism is what lets this be an
    /// equality rather than a similarity.
    /// </remarks>
    [Theory]
    [InlineData(12)]
    [InlineData(18)]
    [InlineData(25)]
    public void ACleanSignalDecodesExactly(int wordsPerMinute)
    {
        var result = CwDecodeHarness.Decode(
            new CwSignalRequest(Call, WordsPerMinute: wordsPerMinute));

        Assert.Equal(Call, result.Text);
        Assert.Empty(CwAlignment.ConfidentMistakes(result.Characters, Call));
        Assert.All(
            result.Letters,
            c => Assert.Equal(CwConfidence.High, c.Confidence));
    }

    /// <remarks>
    /// Proves the speed readout is the sender's actual speed rather than a
    /// number that happens to move in the right direction. The terminal's header
    /// shows this, and an operator deciding whether they can copy somebody is
    /// entitled to a figure that means what it says.
    /// </remarks>
    [Theory]
    [InlineData(12)]
    [InlineData(18)]
    [InlineData(25)]
    [InlineData(30)]
    public void TheSpeedReadoutIsTheSpeedThatWasSent(int wordsPerMinute)
    {
        var result = CwDecodeHarness.Decode(
            new CwSignalRequest(Call, WordsPerMinute: wordsPerMinute));

        Assert.InRange(result.WordsPerMinute, wordsPerMinute - 1, wordsPerMinute + 1);
    }

    /// <remarks>
    /// Proves the same audio always gives the same transcript, character for
    /// character and score for score. Without this a fixture proves nothing,
    /// because a decode that drifted would take its own regression test with it
    /// (§5, HM-DEC-007).
    /// </remarks>
    [Fact]
    public void TheSameAudioAlwaysDecodesTheSameWay()
    {
        var request = new CwSignalRequest(Call, WordsPerMinute: 18, NoiseAmplitude: 0.06);

        var first = CwDecodeHarness.Decode(request);
        var second = CwDecodeHarness.Decode(request);

        Assert.Equal(first.Text, second.Text);
        Assert.Equal(first.Characters.Count, second.Characters.Count);

        for (var i = 0; i < first.Characters.Count; i++)
        {
            Assert.Equal(first.Characters[i], second.Characters[i]);
        }
    }

    /// <remarks>
    /// Proves the pitch is hunted rather than assumed. Somebody who has never
    /// tuned a CW signal lands a couple of hundred hertz off, and a decoder that
    /// went silent about it would be teaching them that the equipment is beyond
    /// them rather than that the dial is a bit out.
    /// </remarks>
    [Theory]
    [InlineData(400)]
    [InlineData(500)]
    [InlineData(750)]
    [InlineData(875)]
    public void ASignalAtTheWrongPitchIsStillFound(double actualToneHz)
    {
        var result = CwDecodeHarness.Decode(
            new CwSignalRequest(Call, WordsPerMinute: 18, ToneHz: actualToneHz),
            expectedToneHz: 600);

        Assert.Equal(Call, result.Text);

        // Within half the filter's own width. The tracker's bins are twenty-five
        // hertz apart and the filter looking through them is a hundred wide, so
        // landing one bin either side of the true pitch costs a decibel and
        // asking for better than that would be asking for precision the
        // measurement does not have.
        Assert.InRange(result.State.ToneHz, actualToneHz - 50, actualToneHz + 50);
    }

    /// <remarks>
    /// Proves prosigns arrive as prosigns. An operator ending a message sends
    /// AR as one symbol with no gap in it, and a decoder that split it into
    /// letters would be wrong in the most confusing way available: it would look
    /// like a mistake in a sentence rather than a symbol the reader has not met.
    /// The fixture is written in the radio's own notation, where "^" means the
    /// characters after it are keyed as one (Full Manual p. 19-12).
    /// </remarks>
    [Theory]
    [InlineData("^AR", "<AR>")]
    [InlineData("^SK", "<SK>")]
    [InlineData("^BT", "<BT>")]
    [InlineData("^KN", "<KN>")]
    public void ProsignsDecodeAsProsigns(string sent, string expected)
    {
        var text = $"W1AW DE K2ABC {sent} TU";
        var result = CwDecodeHarness.Decode(new CwSignalRequest(text, WordsPerMinute: 18));

        Assert.Contains(expected, result.Text, StringComparison.Ordinal);
        Assert.Empty(CwAlignment.ConfidentMistakes(result.Characters, text));
    }

    /// <remarks>
    /// THE CENTRAL CLAIM, and the reason the confidence model exists at all
    /// (§0.0). Noise is added until the decode is plainly suffering, and the
    /// only thing asserted is the thing that matters: nothing the decoder was
    /// sure about was wrong. It is allowed to mark characters uncertain, and it
    /// is allowed to give up on them entirely. It is not allowed to print a
    /// clean wrong letter, because the person reading it will conclude the fault
    /// is theirs.
    /// </remarks>
    [Theory]
    [InlineData(0.05)]
    [InlineData(0.10)]
    [InlineData(0.18)]
    [InlineData(0.28)]
    [InlineData(0.40)]
    public void NoiseCostsConfidenceRatherThanCorrectness(double noise)
    {
        var result = CwDecodeHarness.Decode(new CwSignalRequest(
            Call, WordsPerMinute: 18, Amplitude: 0.5, NoiseAmplitude: noise));

        Assert.Empty(CwAlignment.ConfidentMistakes(result.Characters, Call));
    }

    /// <remarks>
    /// The same claim with a second station on the air a few hundred hertz away,
    /// which is what a busy evening on 40 m actually sounds like. The tracker is
    /// sticky by design, so a louder neighbour does not drag the decode off the
    /// signal the operator was reading.
    /// </remarks>
    [Theory]
    [InlineData(450)]
    [InlineData(800)]
    public void AnotherStationNearbyCostsConfidenceRatherThanCorrectness(double interferenceHz)
    {
        var result = CwDecodeHarness.Decode(new CwSignalRequest(
            Call,
            WordsPerMinute: 18,
            Amplitude: 0.5,
            NoiseAmplitude: 0.04,
            InterferenceHz: interferenceHz,
            InterferenceAmplitude: 0.35,
            InterferenceWpm: 24));

        Assert.Empty(CwAlignment.ConfidentMistakes(result.Characters, Call));
    }

    /// <remarks>
    /// Proves a fading signal comes back. The failure this guards is the nasty
    /// one: a decoder whose threshold is left stranded above a signal that sank,
    /// so it works beautifully for thirty seconds and then goes quiet forever
    /// without ever saying why. The assertion is that characters keep arriving
    /// in the last third of the transmission, which is the part after the trough.
    /// </remarks>
    [Fact]
    public void AFadingSignalComesBackRatherThanStayingDead()
    {
        var text = "CQ CQ DE W1AW W1AW K";
        var request = new CwSignalRequest(
            text,
            WordsPerMinute: 18,
            Amplitude: 0.5,
            NoiseAmplitude: 0.02,
            FadeSeconds: 6,
            FadeDepth: 0.97);

        var result = CwDecodeHarness.Decode(request);
        var duration = CwSignal.DurationOf(request);
        var lastThird = duration * (2.0 / 3.0);

        var late = result.Letters.Count(c => c.At >= lastThird);

        Assert.True(late >= 3, $"only {late} characters arrived after the fade");

        // Recovery is all that is asserted here. A fade this deep takes most of
        // the message with it, and once that much is gone there is no honest way
        // to line the survivors up against what was sent: the same transcript
        // can be read as several different sets of losses. The claim about
        // confident mistakes is made on a fade shallow enough to align, below.
    }

    /// <remarks>
    /// The claim about confidence under a fade, on one shallow enough that the
    /// transcript can still be lined up against what was sent. A sinking signal
    /// does not get noisy, it gets shorter: the dahs go under the gate before
    /// the dits do, so "W" arrives as "E" with clean timing and a healthy margin
    /// above the noise. Nothing about that character looks wrong except that the
    /// level moved while it was arriving, which is precisely why the confidence
    /// model measures that.
    /// </remarks>
    [Fact]
    public void AFadeCostsConfidenceRatherThanCorrectness()
    {
        var text = "CQ CQ DE W1AW W1AW K";
        var result = CwDecodeHarness.Decode(new CwSignalRequest(
            text,
            WordsPerMinute: 18,
            Amplitude: 0.5,
            NoiseAmplitude: 0.02,
            FadeSeconds: 5,
            FadeDepth: 0.75));

        Assert.Empty(CwAlignment.ConfidentMistakes(result.Characters, text));
    }

    /// <remarks>
    /// Proves the speed estimate follows a change rather than being destroyed by
    /// one. Nobody is a metronome, and an operator who realizes you are
    /// struggling will slow down mid-sentence, which is precisely when the
    /// readout must not lie about what it is tracking.
    /// </remarks>
    [Fact]
    public void TheSpeedEstimateFollowsAChangeWithinAFewCharacters()
    {
        var slow = CwSignal.Generate(new CwSignalRequest(
            "CQ DE W1AW", WordsPerMinute: 12, TailSeconds: 0.6));

        var fast = CwSignal.Generate(new CwSignalRequest(
            "CQ TEST DE W1AW W1AW K", WordsPerMinute: 25, LeadInSeconds: 0.1));

        var joined = slow.Samples.Concat(fast.Samples).ToArray();
        var result = CwDecodeHarness.Decode(new MonoAudio(slow.SampleRate, joined));

        // Where the fast half begins, in the joined stream.
        var changeAt = slow.Duration;
        var after = result.Letters.Where(c => c.At > changeAt).ToList();

        Assert.True(after.Count >= 8, $"only {after.Count} characters after the change");

        // Ten characters is about two and a half words, which is a reasonable
        // number to take: the estimate is drawn from a rolling window of twenty
        // elements and that is roughly how long it takes to refill.
        var settled = after.Skip(10).ToList();

        Assert.NotEmpty(settled);
        Assert.All(settled, c => Assert.InRange(c.WordsPerMinute, 23, 27));
        Assert.InRange(result.WordsPerMinute, 23, 27);
    }

    /// <remarks>
    /// Proves nothing is invented out of an empty band. A gate placed in the
    /// middle of noise produces confident nonsense, so below a few decibels of
    /// separation the decoder refuses to decide at all. Silence is the honest
    /// output when there is nothing there (§0.0).
    /// </remarks>
    [Fact]
    public void NoiseAloneDecodesToNothing()
    {
        var noise = CwSignal.Generate(new CwSignalRequest(
            "", WordsPerMinute: 18, Amplitude: 0, NoiseAmplitude: 0.2,
            LeadInSeconds: 0, TailSeconds: 12));

        var result = CwDecodeHarness.Decode(noise);

        Assert.Equal("", result.Text);
        Assert.Empty(result.Letters);
    }

    /// <remarks>
    /// Proves the decoder says so rather than sitting there looking broken. A
    /// beginner tuning across a band spends most of their time on nothing at
    /// all, and an empty terminal that never explains itself teaches them the
    /// app does not work.
    /// </remarks>
    [Fact]
    public void SilenceIsReportedAsNothingHeard()
    {
        var silence = CwSignal.Generate(new CwSignalRequest(
            "", Amplitude: 0, LeadInSeconds: 0, TailSeconds: 12));

        var result = CwDecodeHarness.Decode(silence);

        Assert.Equal(CwNote.NothingHeard, result.Note);
    }

    /// <remarks>
    /// Proves a fade is named as a fade. This is the note that does the most
    /// emotional work in the whole feature: letters coming and going looks
    /// exactly like a decoder that does not work, and the operator has no way to
    /// tell the difference unless they are told.
    /// </remarks>
    [Fact]
    public void AFadeIsNamedAsAFade()
    {
        var result = CwDecodeHarness.Decode(new CwSignalRequest(
            "CQ CQ DE W1AW W1AW K CQ CQ DE W1AW K",
            WordsPerMinute: 18,
            Amplitude: 0.5,
            NoiseAmplitude: 0.05,
            FadeSeconds: 5,
            FadeDepth: 0.99));

        Assert.Equal(CwNote.Fading, result.Note);
    }

    /// <remarks>
    /// Proves a lone character is not guessed at. Two elements with nothing to
    /// compare them against are as likely to be dahs as dits, and naming one
    /// would be exactly the guess dressed as a decode that HM-DEC-009 forbids.
    /// </remarks>
    [Fact]
    public void ACharacterWithNothingToMeasureAgainstIsNotGuessed()
    {
        var result = CwDecodeHarness.Decode(new CwSignalRequest(
            "A", WordsPerMinute: 18, TailSeconds: 3));

        Assert.All(result.Letters, c => Assert.Equal(CwConfidence.Unreadable, c.Confidence));
        Assert.All(result.Letters, c => Assert.Equal(MorseAlphabet.Unreadable, c.Text));
    }

    /// <remarks>
    /// Proves the placeholder is never a letter. Whatever the decoder shows for
    /// something it could not resolve has to be unmistakably not content, or the
    /// reader will copy it down as if it were.
    /// </remarks>
    [Fact]
    public void ThePlaceholderIsNotAnythingThatCouldBeSent()
    {
        Assert.Null(MorseAlphabet.All.Values.FirstOrDefault(
            v => string.Equals(v, MorseAlphabet.Unreadable, StringComparison.Ordinal)));

        Assert.DoesNotContain(MorseAlphabet.Unreadable, "?*#_.-", StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves what the decoder was working from travels with it (§0.0.1). A
    /// wrong decode that arrives with its pattern, its signal margin and the
    /// speed the decoder believed it was tracking is something somebody can fix.
    /// A wrong letter on its own is an argument.
    /// </remarks>
    [Fact]
    public void EveryCharacterCarriesTheEvidenceBehindIt()
    {
        var result = CwDecodeHarness.Decode(
            new CwSignalRequest(Call, WordsPerMinute: 18));

        Assert.All(result.Letters, c =>
        {
            Assert.NotEmpty(c.Pattern);
            Assert.All(c.Pattern, e => Assert.True(e is '.' or '-'));
            Assert.True(c.WordsPerMinute > 0);
            Assert.True(c.At > TimeSpan.Zero);
        });

        Assert.True(result.State.HasSignal);
        Assert.True(result.State.PeakDb > result.State.NoiseFloorDb);
        Assert.InRange(
            result.State.ThresholdDb, result.State.NoiseFloorDb, result.State.PeakDb);
    }

    /// <remarks>
    /// Proves the decoder is indifferent to the sample rate it is handed. The
    /// fixtures run at eight kilohertz to stay small enough to live in the
    /// repository, and the radio's USB codec will deliver forty-eight. Both have
    /// to give the same text, and they do because every measurement is a count
    /// of samples rather than a reading of a clock.
    /// </remarks>
    [Fact]
    public void TheSampleRateMakesNoDifferenceToTheText()
    {
        var slow = CwDecodeHarness.Decode(new CwSignalRequest(
            Call, WordsPerMinute: 18, SampleRate: 8_000));

        var fast = CwDecodeHarness.Decode(new CwSignalRequest(
            Call, WordsPerMinute: 18, SampleRate: 48_000));

        Assert.Equal(Call, slow.Text);
        Assert.Equal(Call, fast.Text);
        Assert.Equal(slow.WordsPerMinute, fast.WordsPerMinute);
    }

    /// <remarks>
    /// Proves the training radio drives the whole chain, which is the test rig
    /// HM-DEC-026 promised: known text, at a known speed, with nothing plugged
    /// in and nobody on the other end.
    /// </remarks>
    [Fact]
    public void TheTrainingRadioDecodesIntoTheTextItIsSending()
    {
        using var source = new TrainingAudioSource("CQ DE W1AW K", wordsPerMinute: 12);
        var decoder = new CwDecoder(source.SampleRate, CwSignal.DefaultToneHz);
        var characters = new List<CwCharacter>();

        decoder.CharacterDecoded += characters.Add;
        decoder.Listen(source);

        // One full repetition and a little more, delivered with no timer.
        source.PumpOnce(source.SampleRate * 14);
        decoder.Flush();

        var text = string.Concat(characters.Select(c => c.Text));

        Assert.Contains("CQ DE W1AW K", text, StringComparison.Ordinal);
        Assert.InRange(decoder.State.WordsPerMinute, 11, 13);
    }

    /// <remarks>
    /// Proves a decoder cannot be pointed at a source running at a different
    /// rate. Every timing in the chain is a sample count, so the mismatch would
    /// not fail loudly, it would silently report the wrong speed and misclassify
    /// every element. Better to refuse.
    /// </remarks>
    [Fact]
    public void ADecoderRefusesASourceAtTheWrongRate()
    {
        using var source = new BufferedAudioSource(new float[100], 48_000);
        var decoder = new CwDecoder(8_000);

        Assert.Throws<ArgumentException>(() => decoder.Listen(source));
    }
}
