using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The operator can tell Hamlet how fast a station is sending, and what that
/// does and does not change.
/// </summary>
/// <remarks>
/// <para>**THE ONE NUMBER THE MACHINE KEEPS GETTING WRONG IS THE ONE THE
/// OPERATOR CAN SUPPLY FOR FREE.** The gate reads a short mark short
/// (HM-DEC-146), so on a fist like `N4L`'s the fitted dit comes out well under
/// the truth, the sender's own dah then measures more than five dits,
/// `MeasureCoherence` falls back to a textbook three that this sender does not
/// send, every mark is scored against a length nobody keyed, and the decoder
/// goes silent on a signal it heard perfectly well. Nothing inside that loop can
/// break it, because every number in it descends from the one that is wrong. A
/// man sitting at the radio can hear that it is about twenty words a minute.</para>
/// <para>**MEASURED ON `farnsworth-heavy`**, which is `N4L`'s fist cut to the
/// millisecond by HM-DEC-144 and sent by the generator: without the figure the
/// decoder reads `AL K` out of `CQ DE N0CALL K`, and with it the callsign
/// survives whole.</para>
/// <para>**AND IT MAY NOT MAKE AN EMPTY BAND SPEAK** (§0.0). The guard that
/// keeps `cw-2026-08-20-014854` silent is how far the two mark lengths sit apart
/// counted in their own scatter, which is a ratio and has no dit in it at all,
/// so no figure the operator types can move it.</para>
/// </remarks>
public sealed class TheOperatorCanSayHowFastItIsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the transcripts are printed.</param>
    public TheOperatorCanSayHowFastItIsTests(ITestOutputHelper output)
        => _output = output;

    private static string Read(string relative, int? seedWpm)
    {
        var audio = WavAudio.Read(
            Path.Combine(CapturedSignalTests.Folder, relative));

        var decoder = new CwDecoder(audio.SampleRate, 600)
        {
            SeededWordsPerMinute = seedWpm,
        };

        var hop = decoder.Tracker.HopSamples;
        var text = new List<string>();

        decoder.CharacterDecoded += c => text.Add(c.Text);

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        return string.Concat(text);
    }

    /// <remarks>
    /// <para>Proves the whole point of it: **the callsign survives on a heavy
    /// fist once the operator says how fast it is.** Without the figure this
    /// message begins nine characters in.</para>
    /// </remarks>
    [Fact]
    public void TheCallsignSurvivesOnAHeavyFistWhenHeSaysTheSpeed()
    {
        var without = Read("../receiver/farnsworth-heavy.wav", null);
        var with = Read("../receiver/farnsworth-heavy.wav", 21);

        _output.WriteLine($"without a figure: '{without}'");
        _output.WriteLine($"with 21 wpm:      '{with}'");

        Assert.DoesNotContain("N0CALL", without, StringComparison.Ordinal);
        Assert.Contains("N0CALL", with, StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>Proves §0.0: **a recording that holds no keying at any pitch stays
    /// silent whatever speed is typed in.** The figure moves where the estimate
    /// starts and never what counts as a signal, and the separation guard that
    /// keeps this file quiet is a ratio with no dit in it.</para>
    /// </remarks>
    [Theory]
    [InlineData(null)]
    [InlineData(12)]
    [InlineData(21)]
    [InlineData(40)]
    public void ARecordingWithNoKeyingStaysSilentAtEverySpeed(int? seedWpm)
    {
        var text = Read("unadjudicated/cw-2026-08-20-014854.wav", seedWpm);

        _output.WriteLine($"seed {seedWpm?.ToString() ?? "off"}: '{text}'");

        Assert.Equal(string.Empty, text.Replace(" ", string.Empty));
    }

    /// <remarks>
    /// <para>Proves the default: **nothing is seeded unless the operator asks**,
    /// and clearing the figure puts the estimator back exactly where it was.</para>
    /// </remarks>
    [Fact]
    public void ItIsOffUntilHeAsksForIt()
    {
        var decoder = new CwDecoder(8_000, 600);

        Assert.Null(decoder.SeededWordsPerMinute);
        Assert.False(decoder.UsingSeededSpeed);

        decoder.SeededWordsPerMinute = 18;
        Assert.Equal(18, decoder.SeededWordsPerMinute);

        decoder.SeededWordsPerMinute = null;
        Assert.Null(decoder.SeededWordsPerMinute);
        Assert.False(decoder.UsingSeededSpeed);
    }

    /// <remarks>
    /// <para>Proves §0.0 again, one level lower: **a figure outside what anybody
    /// sends is refused rather than clamped.** Clamping would hand the estimator
    /// a number the operator did not mean and then read at it silently.</para>
    /// </remarks>
    /// <param name="wpm">The figure.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(200)]
    [InlineData(-5)]
    public void AFigureNobodySendsAtIsRefused(int wpm)
    {
        var estimator = new CwSpeedEstimator(8_000);

        estimator.Seed(wpm);

        Assert.Null(estimator.SeededWordsPerMinute);
        Assert.False(estimator.UsingSeededSpeed);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-048: **the figure is a starting point and not an
    /// answer.** Where the marks fit a fist anybody could have sent, the
    /// estimator's own number is the one in use, and it is free to disagree with
    /// what the operator typed.</para>
    /// </remarks>
    [Fact]
    public void WhereTheFitIsSoundTheEstimatorKeepsItsOwnAnswer()
    {
        var estimator = new CwSpeedEstimator(8_000);
        var random = new Random(7300);

        estimator.Seed(30);

        for (var i = 0; i < 24; i++)
        {
            var wobble = 1 + ((random.NextDouble() - 0.5) * 0.08);

            // Twelve words a minute, textbook, which is nothing like the thirty
            // the operator claimed.
            estimator.AddMark(8_000 * (i % 3 == 2 ? 0.300 : 0.100) * wobble / 1);
            estimator.AddGap(8_000 * 0.100);
        }

        _output.WriteLine(
            $"he said 30, the marks say {estimator.WordsPerMinute}, "
            + $"using his figure: {estimator.UsingSeededSpeed}");

        Assert.False(estimator.UsingSeededSpeed);
        Assert.InRange(estimator.WordsPerMinute, 10, 14);
    }
}
