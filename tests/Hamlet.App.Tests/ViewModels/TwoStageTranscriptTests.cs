using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// The two-stage decode, on screen at last (HM-DEC-096, phase 5 of the UI
/// order).
/// </summary>
/// <remarks>
/// <para>**IT WAS BUILT, TESTED, AND RENDERED BY NOTHING.** `CharacterSettled`,
/// `CwReadingStage` and the revision log all existed and were covered, and no
/// surface read any of them, so the design the decoder was rebuilt around was
/// invisible to the person it was rebuilt for.</para>
/// <para>The rule these prove is one sentence: the settled pass is what the
/// transcript keeps, the leading edge is a tail on the end of it, and where
/// nothing is coming behind the leading edge that is said rather than hidden
/// (§0.0).</para>
/// </remarks>
public sealed class TwoStageTranscriptTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the transcripts are printed.</param>
    public TwoStageTranscriptTests(ITestOutputHelper output) => _output = output;

    private static CwCharacter At(
        string text, double seconds, CwReadingStage stage = CwReadingStage.Provisional)
        => new(text, CwConfidence.High, 0.9, ".-", 20, 18, TimeSpan.FromSeconds(seconds))
        {
            Stage = stage,
        };

    /// <remarks>
    /// <para>Proves HM-DEC-096: **a provisional reading waits in the tip and
    /// never lands in the transcript.** It is right far more often than not and
    /// it is never final, so showing one as though it were is §0.0 broken by
    /// omission however good a guess it usually is.</para>
    /// </remarks>
    [Fact]
    public void TheLeadingEdgeWaitsInTheTipRatherThanEnteringTheTranscript()
    {
        var transcript = new CwTranscript();

        transcript.Offer(At("C", 1.0));
        transcript.Offer(At("Q", 1.4));

        _output.WriteLine($"tip '{transcript.TipText}', body '{transcript.PlainText}'");

        Assert.Equal("CQ", transcript.TipText);
        Assert.True(transcript.IsEmpty);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-096: **the second pass overtakes the first and its
    /// answer is the one that stands.** Everything at or before a settled
    /// character's own moment leaves the tip, because that audio has now been
    /// read twice.</para>
    /// </remarks>
    [Fact]
    public void TheSettledPassOvertakesTheTipAndReplacesIt()
    {
        var transcript = new CwTranscript();

        transcript.Offer(At("C", 1.0));
        transcript.Offer(At("Q", 1.4));
        transcript.Offer(At("E", 1.9));

        transcript.Settle(At("C", 1.0, CwReadingStage.Settled));
        transcript.Settle(At("Q", 1.4, CwReadingStage.Settled));

        _output.WriteLine($"tip '{transcript.TipText}', body '{transcript.PlainText}'");

        // The two the second pass reached are in the transcript and out of the
        // tip; the one it has not reached is still only the leading edge.
        Assert.Equal("CQ", transcript.PlainText);
        Assert.Equal("E", transcript.TipText);
    }

    /// <remarks>
    /// <para>**THE STATE THIS EXISTS FOR, AND IT IS AN ORDINARY ONE.** The
    /// settled pass refuses below its own working limit rather than copying into
    /// the band where it is half wrong (HM-DEC-097), and then nothing is coming
    /// behind the leading edge at all. Waiting would be waiting forever, so the
    /// reading is committed at once carrying the mark saying nothing confirmed
    /// it.</para>
    /// <para>Losing the text entirely would be worse than showing it marked, and
    /// the moment somebody answers a call is the worst possible moment for the
    /// live feed to go dark.</para>
    /// </remarks>
    [Fact]
    public void AReadingWithNothingBehindItIsKeptAndMarkedRatherThanDropped()
    {
        var transcript = new CwTranscript();

        transcript.Offer(At("V", 1.0, CwReadingStage.Unstable));
        transcript.Offer(At("A", 1.4, CwReadingStage.Unstable));

        _output.WriteLine($"tip '{transcript.TipText}', body '{transcript.PlainText}'");

        Assert.Equal("VA", transcript.PlainText);
        Assert.Equal("", transcript.TipText);

        // AND THE MARK TRAVELS WITH IT. A character nothing confirmed and one a
        // second pass stood behind may not look the same (§0.0).
        var drained = new List<CwCharacter>();
        transcript.Drain(drained);

        Assert.All(drained, c => Assert.True(c.IsUnstable));
    }

    /// <remarks>
    /// <para>Proves §0.0: **a settled pass that never catches up cannot grow the
    /// tip without limit.** Reaching the ceiling is a fault rather than a
    /// working state, and the oldest go first so what is on screen is the newest
    /// thing heard rather than the oldest.</para>
    /// </remarks>
    [Fact]
    public void TheTipCannotGrowWithoutLimit()
    {
        var transcript = new CwTranscript();

        for (var i = 0; i < CwTranscript.LongestTip * 2; i++)
        {
            transcript.Offer(At(((char)('A' + (i % 26))).ToString(), i * 0.1));
        }

        _output.WriteLine($"tip is {transcript.TipText.Length} characters");

        Assert.Equal(CwTranscript.LongestTip, transcript.TipText.Length);
        Assert.True(transcript.IsEmpty);
    }

    /// <remarks>
    /// Proves HM-DEC-051: **a clear takes the tip with it.** Leaving the leading
    /// edge on screen after a band change would run two sessions together at
    /// exactly the seam the clear exists to make.
    /// </remarks>
    [Fact]
    public void ClearingTakesTheLeadingEdgeWithIt()
    {
        var transcript = new CwTranscript();

        transcript.Offer(At("C", 1.0));
        transcript.Settle(At("E", 0.5, CwReadingStage.Settled));

        Assert.False(transcript.IsEmpty);
        Assert.True(transcript.HasTip);

        transcript.Clear();

        Assert.True(transcript.IsEmpty);
        Assert.False(transcript.HasTip);
        Assert.Equal("", transcript.TipText);
    }

    /// <remarks>
    /// <para>Proves the ordering: **settled text lands behind the tip and never
    /// after it.** A transcript in which the second pass appended after the
    /// leading edge would read as though the older reading came later, which is
    /// the one thing the arrangement is for.</para>
    /// </remarks>
    [Fact]
    public void SettledTextLandsBehindTheLeadingEdge()
    {
        var transcript = new CwTranscript();

        transcript.Offer(At("D", 2.0));
        transcript.Offer(At("E", 2.4));

        transcript.Settle(At("C", 1.0, CwReadingStage.Settled));
        transcript.Settle(At("Q", 1.4, CwReadingStage.Settled));

        _output.WriteLine($"'{transcript.PlainText}' + tip '{transcript.TipText}'");

        Assert.Equal("CQ", transcript.PlainText);
        Assert.Equal("DE", transcript.TipText);
    }
}
