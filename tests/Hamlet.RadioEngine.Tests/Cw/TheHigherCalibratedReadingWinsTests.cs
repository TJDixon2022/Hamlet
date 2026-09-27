using Hamlet.RadioEngine.Cw;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Proves HM-REQ-124 to 127 on hand-built readings of one span (work instruction
/// 465, task 2; PHASE_PLAN.md 9.6).
/// </summary>
/// <remarks>
/// <para>**BUILT FROM THE REQUIREMENTS' WORDS, NOT FROM THE ARBITER'S OUTPUT**
/// (CLAUDE.md 12.5). Each case is one span both decoders read, with the p's the
/// instruction names and a vote injected so both decoders vote, or so one or
/// neither does.</para>
/// <para>**WATCHED FAILING FIRST** against a stub <see cref="CwArbiter.Arbitrate"/>
/// that emitted ours unchanged and recorded nothing: every case below was red
/// there (`.run-unit/unit465-wins-red.txt`), the agreement and disagreement cases
/// on the character or class emitted, and the advisory and neither-votes cases,
/// whose character the stub already emitted, on the record the sheet reads.</para>
/// </remarks>
public sealed class TheHigherCalibratedReadingWinsTests
{
    private static readonly CwVote Both = new(true, true);

    // One span: ours 0.700 s to 1.000 s, the port 0.705 s to 0.995 s.
    private static CwCharacter Ours(string text, CwConfidence confidence, double p)
        => new(text, confidence, 1, text == "K" ? "-.-" : ".-.", double.NaN, 18, TimeSpan.FromSeconds(1.0))
        {
            SpanHops = 60,
            Probability = p,
        };

    private static CwSecondReading Port(string text, double p)
        => new(text, CwConfidence.High, text == "K" ? "-.-" : ".-.", p, 0.705, 0.995, 18);

    // The record is read after the character and class are checked, so a stub that emits the wrong character fails on the rule itself.
    private static (CwCharacter Emitted, Func<CwArbitration> Record) One(CwCharacter ours, CwSecondReading port, CwVote vote)
    {
        var result = CwArbiter.Arbitrate(new[] { ours }, new[] { port }, vote);

        Assert.Single(result.Characters);

        return (result.Characters[0], () => Assert.Single(result.Records));
    }

    /// <remarks>HM-REQ-125: both read K, ours dim at 0.62, the port at 0.91; K is emitted sure, the port's class.</remarks>
    [Fact]
    public void HmReq125_AnAgreementTakesTheMoreConfidentClass()
    {
        var (c, record) = One(Ours("K", CwConfidence.Low, 0.62), Port("K", 0.91), Both);

        Assert.Equal("K", c.Text);
        Assert.Equal(CwConfidence.High, c.Confidence);

        var r = record();

        Assert.Equal(CwArbitrationCase.Agree, r.Case);
        Assert.Equal(CwReader.Second, r.Emitted);
        Assert.Equal(0.62, r.OursP);
        Assert.Equal(0.91, r.SecondP);
    }

    /// <remarks>HM-REQ-125, the mirror: both read K, ours dim at 0.95, the port sure at 0.70; K is emitted dim, ours' class.</remarks>
    [Fact]
    public void HmReq125_TheMirror_OursMoreConfidentGivesOursClass()
    {
        var (c, record) = One(Ours("K", CwConfidence.Low, 0.95), Port("K", 0.70), Both);

        Assert.Equal("K", c.Text);
        Assert.Equal(CwConfidence.Low, c.Confidence);

        var r = record();

        Assert.Equal(CwArbitrationCase.Agree, r.Case);
        Assert.Equal(CwReader.Ours, r.Emitted);
    }

    /// <remarks>HM-REQ-126: ours R at 0.70, the port K at 0.90; K is emitted at its class, and the record carries both characters and both p's.</remarks>
    [Fact]
    public void HmReq126_ADisagreementGoesToTheHigherCalibratedP()
    {
        var (c, record) = One(Ours("R", CwConfidence.High, 0.70), Port("K", 0.90), Both);

        Assert.Equal("K", c.Text);
        Assert.Equal(CwConfidence.High, c.Confidence);

        var r = record();

        Assert.Equal(CwArbitrationCase.Disagree, r.Case);
        Assert.Equal(CwReader.Second, r.Emitted);
        Assert.Equal(("R", 0.70, "K", 0.90), (r.OursText, r.OursP, r.SecondText, r.SecondP));
        Assert.Equal(("R", 0.70), (r.OtherText, r.OtherP));
        Assert.Same(r, c.Arbitration);
    }

    /// <remarks>HM-REQ-126, the mirror: ours R at 0.90, the port K at 0.70; R is emitted at ours' class, both on the record.</remarks>
    [Fact]
    public void HmReq126_TheMirror_OursHigherKeepsOurs()
    {
        var (c, record) = One(Ours("R", CwConfidence.High, 0.90), Port("K", 0.70), Both);

        Assert.Equal("R", c.Text);
        Assert.Equal(CwConfidence.High, c.Confidence);

        var r = record();

        Assert.Equal(CwArbitrationCase.Disagree, r.Case);
        Assert.Equal(CwReader.Ours, r.Emitted);
        Assert.Equal(("K", 0.70), (r.OtherText, r.OtherP));
    }

    /// <remarks>HM-REQ-127: ours R at 0.88, the port K at 0.90, within 0.05; the winner K is emitted dim, never sure, and the record says tie.</remarks>
    [Fact]
    public void HmReq127_ATieWithinTheMarginIsDimAndNeverSure()
    {
        var (c, record) = One(Ours("R", CwConfidence.High, 0.88), Port("K", 0.90), Both);

        Assert.Equal("K", c.Text);
        Assert.Equal(CwConfidence.Low, c.Confidence);
        Assert.NotEqual(CwConfidence.High, c.Confidence);

        var r = record();

        Assert.Equal(CwArbitrationCase.Tie, r.Case);
        Assert.Equal(CwReader.Second, r.Emitted);
        Assert.Equal(0.05, CwArbiter.TieMargin);
    }

    /// <remarks>HM-REQ-124, advisory: the disagreement of 126 with the port's vote withdrawn; ours' R at ours' class, the port's K on the record.</remarks>
    [Fact]
    public void HmReq124_AnAdvisoryReadingNeverDisplacesACharacter()
    {
        var (c, record) = One(Ours("R", CwConfidence.High, 0.70), Port("K", 0.90), new CwVote(true, false));

        Assert.Equal("R", c.Text);
        Assert.Equal(CwConfidence.High, c.Confidence);

        var r = record();

        Assert.Equal(CwReader.Ours, r.Emitted);
        Assert.Equal(CwArbitrationCase.Disagree, r.Case);
        Assert.Equal(("K", 0.90), (r.SecondText, r.SecondP));
    }

    /// <remarks>HM-REQ-124, advisory, a class: ours dim K at 0.62, the port sure K at 0.91, the port not voting; ours' dim stands, the class is not raised.</remarks>
    [Fact]
    public void HmReq124_AnAdvisoryReadingNeverRaisesAClass()
    {
        var (c, record) = One(Ours("K", CwConfidence.Low, 0.62), Port("K", 0.91), new CwVote(true, false));

        Assert.Equal(CwConfidence.Low, c.Confidence);

        var r = record();

        Assert.Equal(CwReader.Ours, r.Emitted);
        Assert.Equal(CwArbitrationCase.Agree, r.Case);
    }

    /// <remarks>HM-REQ-124, neither votes: ours is emitted as it prints today (465 DECIDED (4)), the port's reading on the record.</remarks>
    [Fact]
    public void HmReq124_WhereNeitherVotesOursPrintsAsToday()
    {
        var ours = Ours("R", CwConfidence.High, 0.70);
        var (c, record) = One(ours, Port("K", 0.90), CwVote.Neither);

        Assert.Equal((ours.Text, ours.Confidence, ours.At), (c.Text, c.Confidence, c.At));

        var r = record();

        Assert.Equal(CwReader.Ours, r.Emitted);
        Assert.Equal("K", r.SecondText);
    }
}
