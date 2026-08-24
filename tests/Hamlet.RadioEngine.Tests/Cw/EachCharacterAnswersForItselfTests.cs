using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The emit decision is made per character, and the silence property survives
/// the move.
/// </summary>
/// <remarks>
/// <para>**THE WINDOW RATIO WAS ASKING THE WRONG QUESTION.** It asks whether a
/// stretch of band averaged better than silence, and then every character in
/// that stretch inherits the one answer. So a window that averaged well carries
/// its own soup through, and a window that averaged badly takes a correct read
/// down with it. Measured on this corpus it is anti-correlated with
/// correctness: `cw-2026-08-17-134712` carries an adjudicated `N4L` and scores
/// 4.64, while `cw-2026-08-20-014854` holds no keying at all and scores
/// 7.98.</para>
/// <para>**THE PROPERTY IS NOT TRADED.** Both captures holding no station must
/// still emit nothing, and that is asserted here directly rather than inferred
/// from the window gate still being in place.</para>
/// </remarks>
public sealed class EachCharacterAnswersForItselfTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public EachCharacterAnswersForItselfTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The two recordings an independent sweep says hold no keying.</summary>
    public static TheoryData<string, double> Empty { get; } = new()
    {
        { "unadjudicated/cw-2026-08-20-014854", 600 },
        { "unadjudicated/cw-2026-08-20-014935", 825 },
    };

    private static MonoAudio Read(string name) => WavAudio.Read(
        Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

    /// <remarks>
    /// **THE ONE PROPERTY THAT HAS NEVER BEEN TRADED** (HM-DEC-120). Audio
    /// holding no station emits nothing, and this asserts it of the whole
    /// production path after the emit decision moved.
    /// </remarks>
    /// <param name="name">The recording.</param>
    /// <param name="toneHz">Where to look.</param>
    [Theory]
    [MemberData(nameof(Empty))]
    public void AudioHoldingNoStationStillEmitsNothing(string name, double toneHz)
    {
        var result = CwProbabilisticDecoder.Decode(Read(name), toneHz);

        _output.WriteLine(
            $"{name}: window {result.LikelihoodRatio:0.00}, "
            + $"'{string.Concat(result.Characters.Select(c => c.Text))}'");

        Assert.Empty(result.Characters);
    }

    /// <remarks>
    /// <para>**THE GAP IS REAL ON WHOLE FILES AND IT DOES NOT SURVIVE THE PATH
    /// PRODUCTION RUNS.** This pins both halves, because the first is what a
    /// margin would be derived from and the second is why this unit did not
    /// derive one.</para>
    /// <para>Read whole, `cw-2026-08-18-004507`'s weakest character clears every
    /// character either empty capture produces, so a margin between them would
    /// silence noise on the characters themselves. Read through the streaming
    /// window, the same capture's weakest real character falls to about three,
    /// far under what an empty band scores, and no positive margin separates
    /// them.</para>
    /// <para>**WHY THEY DISAGREE**: the whole-file read estimates its noise scale
    /// once over the whole recording and the streaming path re-estimates it every
    /// window, so one character is scored against two different noise floors.
    /// A margin taken through one instrument is not a fact about the other
    /// (HM-DEC-119).</para>
    /// </remarks>
    [Fact]
    public void TheGapExistsOnWholeFilesAndNotOnTheStreamingPath()
    {
        var worstEmpty = double.NegativeInfinity;

        foreach (var (name, toneHz) in new[]
        {
            ("unadjudicated/cw-2026-08-20-014854", 600.0),
            ("unadjudicated/cw-2026-08-20-014935", 825.0),
        })
        {
            var audio = Read(name);

            var ungated = CwProbabilisticDecoder.DecodeUngated(
                CwProbabilisticDecoder.Envelope(
                    audio.Samples, audio.SampleRate, toneHz),
                toneHz);

            var letters = ungated.Characters
                .Where(c => c.Pattern.Length > 0)
                .ToList();

            var highest = letters.Count == 0
                ? double.NegativeInfinity
                : letters.Max(c => c.SpanMargin);

            worstEmpty = Math.Max(worstEmpty, highest);

            _output.WriteLine(
                $"whole file, {name}: {letters.Count} characters, "
                + $"highest margin {highest:0.0}");
        }

        var reading = Read("cw-2026-08-18-004507");

        var whole = CwProbabilisticDecoder.DecodeUngated(
            CwProbabilisticDecoder.Envelope(reading.Samples, reading.SampleRate, 501),
            501);

        var lowestWhole = whole.Characters
            .Where(c => c.Pattern.Length > 0)
            .Min(c => c.SpanMargin);

        _output.WriteLine(
            $"whole file, cw-2026-08-18-004507: weakest character {lowestWhole:0.0}");

        Assert.True(
            lowestWhole > worstEmpty,
            $"the whole-file gap has closed: a reading capture's weakest "
            + $"character ({lowestWhole:0.0}) no longer clears an empty band's "
            + $"strongest ({worstEmpty:0.0}).");

        // And the other half, on the path that actually runs.
        var stream = new CwProbabilisticStream(reading.SampleRate) { ToneHz = 501 };
        var settled = new List<CwCharacter>();

        stream.CharacterSettled += settled.Add;
        stream.Process(reading.Samples);
        stream.Flush();

        var lowestStreamed = settled
            .Where(c => !c.IsWordGap && !double.IsNaN(c.SpanLogLikelihoodRatio))
            .Min(c => c.SpanMarginForRecord);

        _output.WriteLine(
            $"streaming, cw-2026-08-18-004507: weakest character "
            + $"{lowestStreamed:0.0}");
        _output.WriteLine(
            $"so the whole-file gap is {worstEmpty:0.0} to {lowestWhole:0.0}, "
            + $"and the streaming path puts a real character at {lowestStreamed:0.0} "
            + "inside it");

        Assert.True(
            lowestStreamed < worstEmpty,
            "the streaming path now keeps its weakest real character above what "
            + "an empty band scores, which would mean a margin could be derived "
            + "after all — re-measure before trusting this test's premise.");
    }

    /// <remarks>
    /// Proves the change costs nothing on a signal there is no argument about: a
    /// clean generated station reads whole, with no character marked.
    /// </remarks>
    [Fact]
    public void ACleanSignalStillReadsWholeWithNothingMarked()
    {
        var audio = CwSignal.Generate(new CwSignalRequest(
            "CQ DE W1AW K",
            WordsPerMinute: 18,
            ToneHz: 640,
            Amplitude: 0.5,
            NoiseAmplitude: CwSensitivity.NoiseFor(18),
            Seed: 7919));

        var result = CwProbabilisticDecoder.Decode(audio, 640);
        var text = string.Concat(result.Characters.Select(c => c.Text));

        _output.WriteLine($"'{text}'");
        _output.WriteLine(
            string.Join(
                " ",
                result.Characters
                    .Where(c => c.Pattern.Length > 0)
                    .Select(c => $"{c.Text}:{c.SpanMargin:0}")));

        Assert.Contains("CQ DE W1AW K", text, StringComparison.Ordinal);
        Assert.DoesNotContain("#", text, StringComparison.Ordinal);
    }

    /// <remarks>
    /// **MARKED, NOT DROPPED.** Dropping a weak character would close the gap and
    /// hand the reader a shorter word that looks like a clean decode. The count
    /// of characters is unchanged by the judgement; only what they say changes.
    /// </remarks>
    [Fact]
    public void AWeakCharacterIsMarkedRatherThanRemoved()
    {
        var audio = Read("cw-2026-08-17-013347");

        var env = CwProbabilisticDecoder.Envelope(audio.Samples, audio.SampleRate, 600);

        var ungated = CwProbabilisticDecoder.DecodeUngated(env, 600);
        var judged = CwProbabilisticDecoder.Decode(env, 600);

        var marked = judged.Characters.Count(
            c => string.Equals(c.Text, "#", StringComparison.Ordinal));

        _output.WriteLine(
            $"{ungated.Characters.Count} before, {judged.Characters.Count} after, "
            + $"{marked} marked");
        _output.WriteLine(string.Concat(judged.Characters.Select(c => c.Text)));

        Assert.Equal(ungated.Characters.Count, judged.Characters.Count);
        Assert.True(marked > 0, "nothing was marked on a recording full of soup.");
    }

    /// <remarks>
    /// A word gap carries no marks and has no evidence of its own to clear, so
    /// testing it would delete every space in the transcript.
    /// </remarks>
    [Fact]
    public void WordGapsAreNeverMarked()
    {
        var audio = Read("cw-2026-08-18-004507");
        var result = CwProbabilisticDecoder.Decode(audio, 501);

        var gaps = result.Characters
            .Where(c => c.Pattern.Length == 0)
            .ToList();

        _output.WriteLine($"{gaps.Count} word gaps");

        Assert.NotEmpty(gaps);

        Assert.All(
            gaps,
            c => Assert.Equal(" ", c.Text));
    }
}
