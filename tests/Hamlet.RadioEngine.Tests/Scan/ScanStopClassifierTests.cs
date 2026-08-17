using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Scan;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Scan;

/// <summary>
/// What a scan is allowed to stop for (HM-DEC-107, phase 7).
/// </summary>
/// <remarks>
/// <para>**A SCANNER THAT STOPS ON "CQ" IS WORTH TEN OF ONE THAT STOPS ON A
/// TONE.** The waterfall can only say something is there, and something is
/// nearly always there: a carrier, a birdie, a switching supply down the street.
/// What earns the operator's attention is a structure only a person sending
/// Morse produces.</para>
/// </remarks>
public sealed class ScanStopClassifierTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the verdicts are printed.</param>
    public ScanStopClassifierTests(ITestOutputHelper output) => _output = output;

    /// <summary>Build a window of decoded characters from plain text.</summary>
    /// <param name="text">The text, spaces becoming word gaps.</param>
    /// <param name="score">How far the decoder stands behind every character.</param>
    private static List<CwCharacter> Heard(string text, double score = 0.9)
    {
        var confidence = score >= 0.7 ? CwConfidence.High : CwConfidence.Low;
        var heard = new List<CwCharacter>();
        var at = TimeSpan.Zero;

        foreach (var c in text)
        {
            at += TimeSpan.FromMilliseconds(400);

            heard.Add(c == ' '
                ? new CwCharacter(
                    MorseAlphabet.WordGap, CwConfidence.High, 1, "", 20, 18, at)
                : new CwCharacter(
                    c.ToString(), confidence, score, ".-", 20, 18, at));
        }

        return heard;
    }

    /// <remarks>
    /// Proves HM-DEC-107 phase 7: **a CQ is the strongest reason there is.** It
    /// is an explicit invitation, so it is the one case where a scan has found
    /// not merely a signal but a person asking to be answered.
    /// </remarks>
    [Fact]
    public void ACallIsWhatAScanStopsFor()
    {
        var verdict = ScanStopClassifier.Judge(Heard("CQ CQ CQ DE VA3VRR K"));

        _output.WriteLine(verdict.Sentence);

        Assert.True(verdict.Stop);
        Assert.Equal(ScanStopReason.Calling, verdict.Reason);
        Assert.Contains("CQ", verdict.Evidence, StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-107 phase 7 and §0.0: **a tone that decodes into
    /// nothing anybody sends does not stop the scan.** This is the common case
    /// and it is not a failure. Letters arriving at random mean the decoder
    /// could not read the signal, which is a fact about the signal rather than
    /// grounds for parking the operator's radio on it.</para>
    /// </remarks>
    [Fact]
    public void AToneThatDecodesIntoNothingDoesNotStopTheScan()
    {
        var verdict = ScanStopClassifier.Judge(Heard("XZ QJ WY BF"));

        _output.WriteLine(verdict.Sentence);

        Assert.False(verdict.Stop);
        Assert.Equal(ScanStopReason.NothingRecognized, verdict.Reason);

        // And it says how much it heard, because "nothing recognizable came out
        // of eleven characters" and "nothing came out at all" are different
        // facts about a frequency (§0.0.1).
        Assert.True(verdict.Characters > 0);
    }

    /// <remarks>
    /// Proves HM-DEC-107 phase 7: silence is its own answer, and it is not the
    /// same answer as a signal that could not be read.
    /// </remarks>
    [Fact]
    public void SilenceIsNotTheSameAnswerAsAnUnreadableSignal()
    {
        var quiet = ScanStopClassifier.Judge(Array.Empty<CwCharacter>());

        Assert.False(quiet.Stop);
        Assert.Equal(ScanStopReason.NothingHeard, quiet.Reason);
        Assert.Equal(0, quiet.Characters);

        var noisy = ScanStopClassifier.Judge(Heard("XZQJ"));

        Assert.NotEqual(quiet.Reason, noisy.Reason);
    }

    /// <remarks>
    /// <para>**PROVES THE CARRY-THROUGH, WHICH IS THE POINT OF PHASE 7'S SECOND
    /// SENTENCE.** Stopping on a CQ every character of which was solid and
    /// stopping on one assembled from dim letters are different events, and a
    /// screen that draws them identically has presented a guess as a decode
    /// (§0.0).</para>
    /// </remarks>
    [Fact]
    public void AMaybeCallLooksDifferentFromACleanOne()
    {
        var clean = ScanStopClassifier.Judge(Heard("CQ DE VA3VRR", score: 0.95));
        var maybe = ScanStopClassifier.Judge(Heard("CQ DE VA3VRR", score: 0.3));

        _output.WriteLine($"clean: {clean.Confidence:0.00}  {clean.Sentence}");
        _output.WriteLine($"maybe: {maybe.Confidence:0.00}  {maybe.Sentence}");

        Assert.True(clean.Stop);
        Assert.True(maybe.Stop);

        Assert.True(
            clean.Confidence > maybe.Confidence + 0.3,
            "a dim call and a solid one came back with the same confidence, so "
            + "nothing downstream can draw them differently");

        // And the difference reaches the words, not only the number.
        Assert.NotEqual(clean.Sentence, maybe.Sentence);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-107 phase 7 against HM-DEC-073: **a callsign-shaped
    /// token stops the scan and is never claimed as a callsign.** Loose text
    /// that fits the shape is enough to justify pausing the dial and is nowhere
    /// near enough to put a name on screen, so the verdict carries no
    /// name.</para>
    /// </remarks>
    [Fact]
    public void ACallsignShapedTokenStopsTheScanWithoutBeingClaimed()
    {
        var verdict = ScanStopClassifier.Judge(Heard("TU VA3VRR FB"));

        _output.WriteLine(verdict.Sentence);

        Assert.True(verdict.Stop);
        Assert.Equal(ScanStopReason.CallsignShaped, verdict.Reason);

        // The sentence describes a shape and does not name anybody. HM-DEC-073
        // permits a name only after DE, before DE, or before a closing prosign,
        // and this window is none of those.
        Assert.DoesNotContain("VA3VRR", verdict.Sentence, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves HM-DEC-107 phase 7: **repetition survives a decode too poor to
    /// read.** Two bad readings of the same thing are bad in the same way, so a
    /// station calling over and over is findable when its words are not.
    /// </remarks>
    [Fact]
    public void SomethingComingRoundTwiceIsWorthStoppingFor()
    {
        var verdict = ScanStopClassifier.Judge(Heard("QRTMZ QRTMZ", score: 0.4));

        _output.WriteLine($"{verdict.Reason}: '{verdict.Evidence}' at "
            + $"{verdict.Confidence:0.00}");

        Assert.True(verdict.Stop);
        Assert.Equal(ScanStopReason.Repeated, verdict.Reason);
    }

    /// <remarks>
    /// Proves HM-DEC-107 phase 7: **ordinary text that happens to repeat a
    /// letter or two is not a beacon.** A scanner that stops on a coincidence is
    /// a scanner nobody trusts, so the run has to be four characters.
    /// </remarks>
    [Fact]
    public void AShortCoincidenceIsNotARepeat()
    {
        var verdict = ScanStopClassifier.Judge(Heard("ZX QY ZX WB"));

        _output.WriteLine($"{verdict.Reason}: '{verdict.Evidence}'");

        Assert.NotEqual(ScanStopReason.Repeated, verdict.Reason);
    }

    /// <remarks>
    /// Proves HM-DEC-107 phase 7: a window holding both a call and a callsign
    /// reports the call, because that is what makes the frequency worth the
    /// operator's evening.
    /// </remarks>
    [Fact]
    public void TheStrongestReasonIsTheOneReported()
    {
        var verdict = ScanStopClassifier.Judge(Heard("VA3VRR CQ DE W1AW"));

        Assert.Equal(ScanStopReason.Calling, verdict.Reason);
    }

    /// <remarks>
    /// Proves HM-DEC-107 phase 7 and §0.0: **a placeholder never welds two
    /// tokens together.** A hole in a word is not a word, and letting one close
    /// silently would let a scan read a CQ out of two halves of other things.
    /// </remarks>
    [Fact]
    public void AHoleInAWordDoesNotMakeAWord()
    {
        var heard = Heard("C");
        heard.Add(new CwCharacter(
            "?", CwConfidence.Unreadable, 0, "", 3, 18, TimeSpan.FromSeconds(1)));
        heard.AddRange(Heard("Q"));

        var verdict = ScanStopClassifier.Judge(heard);

        _output.WriteLine(verdict.Sentence);

        Assert.NotEqual(ScanStopReason.Calling, verdict.Reason);
    }
}
