using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The per-character span log-likelihood: what it is worth, and that it survives
/// the path from the decoder to the capture sheet.
/// </summary>
/// <remarks>
/// <para>**THE WINDOW'S RATIO CANNOT TELL TWO CHARACTERS APART AND THAT IS THE
/// WHOLE PROBLEM** (§0.0.1, HM-DEC-007). Every character read out of one window
/// carries that window's average, so a letter lifted out of a clean signal and a
/// letter the path assembled out of the noise beside it arrive on the capture
/// sheet looking identical, and a wrong decode could be argued about but not
/// measured.</para>
/// <para>**NOTHING HERE ASSERTS A THRESHOLD.** No number below is a gate, and
/// this unit deliberately adds none: what is proved is that the quantity
/// separates the two cases at all, and that it reaches the sidecar intact. The
/// gate that later work derives from it is a ruling nobody has made.</para>
/// </remarks>
public sealed class EveryCharacterCarriesItsOwnEvidenceTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the distributions are printed.</param>
    public EveryCharacterCarriesItsOwnEvidenceTests(ITestOutputHelper output)
        => _output = output;

    /// <remarks>
    /// <para>**A CHARACTER READ FROM A SIGNAL SCORES LARGE AND POSITIVE.** On
    /// generated audio at a comfortable ratio the text is known, so every
    /// character emitted was sent, and every one of them should be explained far
    /// better by the keying the path chose than by the key having been up
    /// throughout its own span.</para>
    /// <para>The floor is nought rather than a fitted figure. A character whose
    /// own marks are no better explained by keying than by silence is one this
    /// decoder should never have emitted at eighteen decibels, and asserting more
    /// than that would be inventing the gate this unit is forbidden to set.</para>
    /// </remarks>
    [Fact]
    public void ARealCharacterIsExplainedFarBetterByKeyingThanBySilence()
    {
        var result = CwDecodeHarness.Decode(
            new CwSignalRequest(
                CwSensitivity.Message,
                WordsPerMinute: CwSensitivity.WordsPerMinute,
                ToneHz: CwSensitivity.ToneHz,
                NoiseAmplitude: CwSensitivity.NoiseFor(18),
                Seed: 7919),
            CwSensitivity.ToneHz);

        var letters = result.Letters;

        Assert.NotEmpty(letters);

        foreach (var letter in letters)
        {
            _output.WriteLine(
                $"{letter.Text}  {letter.SpanLogLikelihoodRatio:0.0}");
        }

        Assert.All(
            letters,
            letter =>
            {
                Assert.False(
                    double.IsNaN(letter.SpanLogLikelihoodRatio),
                    $"'{letter.Text}' reached the transcript with no span ratio.");

                Assert.True(
                    letter.SpanLogLikelihoodRatio > 0,
                    $"'{letter.Text}' scored "
                    + $"{letter.SpanLogLikelihoodRatio:0.0}, which says the key "
                    + "having been up explains its span at least as well.");
            });
    }

    /// <remarks>
    /// **THE SEPARATION IS THE POINT, AND IT IS MEASURED RATHER THAN ASSUMED.**
    /// The same message at eighteen decibels and at one produces characters whose
    /// spans hold very different amounts of signal, and if the quantity could not
    /// tell them apart it would be worth nothing to the units built on top of it.
    /// Printed as well as asserted, because the figures are what the next unit
    /// derives a gate from.
    /// </remarks>
    [Fact]
    public void TheEvidenceFallsAwayAsTheSignalDoes()
    {
        var strong = Median(18);
        var weak = Median(1);

        _output.WriteLine($"18 dB  median span ratio {strong:0.0}");
        _output.WriteLine($" 1 dB  median span ratio {weak:0.0}");

        Assert.True(
            strong > weak,
            $"18 dB scored {strong:0.0} and 1 dB scored {weak:0.0}, so this "
            + "number is not measuring how much signal was in the span.");
    }

    /// <remarks>
    /// **A WINDOW THAT EMITS NOTHING WRITES NOTHING.** The field exists to
    /// describe characters, and audio holding no station produces none, so there
    /// is nothing for it to describe. Asserted because the failure would be
    /// silent: a sheet carrying span ratios for a recording that emitted no text
    /// is a decode with no signal behind it wearing a number (§0.0).
    /// </remarks>
    [Fact]
    public void AudioHoldingNoStationCarriesNoSpanRatios()
    {
        var audio = Hamlet.RadioEngine.Audio.WavAudio.Read(
            Path.Combine(
                CapturedSignalTests.Folder,
                "unadjudicated",
                "cw-2026-08-20-014935.wav"));

        var result = CwDecodeHarness.Decode(audio, 600);

        _output.WriteLine($"{result.Letters.Count} characters emitted");

        Assert.Empty(result.Letters);
    }

    private static double Median(double snrDb)
    {
        var ratios = CwDecodeHarness.Decode(
                new CwSignalRequest(
                    CwSensitivity.Message,
                    WordsPerMinute: CwSensitivity.WordsPerMinute,
                    ToneHz: CwSensitivity.ToneHz,
                    NoiseAmplitude: CwSensitivity.NoiseFor(snrDb),
                    Seed: 7919),
                CwSensitivity.ToneHz)
            .Letters
            .Select(letter => letter.SpanLogLikelihoodRatio)
            .Where(ratio => !double.IsNaN(ratio))
            .OrderBy(ratio => ratio)
            .ToArray();

        return ratios.Length == 0 ? 0 : ratios[ratios.Length / 2];
    }
}
