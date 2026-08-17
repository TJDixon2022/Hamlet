using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// The scope panel names nothing it has not read (HM-DEC-092).
/// </summary>
public sealed class ScopeHonestyTests
{
    private static readonly DateTime Now = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    private static RigCapabilities Radio { get; } = new(
        "IC-7300", HasSpectrumScope: true, HasBuiltInCwKeyer: true,
        HasUsbAudio: true, CanTransmit: true, new[] { "40 m" });

    private static RigState Scope(int on, int output)
        => RigState.Empty.With(new[]
        {
            RigValue.Known(RigField.ScopeOn, on, on == 1 ? "on" : "off", Now, "CI-V 27 10"),
            RigValue.Known(
                RigField.ScopeOutput, output, output == 1 ? "on" : "off",
                Now, "CI-V 27 11"),
        });

    private static CivLinkHealth Link(int baud = 115_200)
        => new("COM3", baud, 100, 100, 0, null, null);

    /// <remarks>
    /// <para>Proves HM-DEC-092, and it is the fault the whole ruling is about.
    /// **The panel named two radio settings as the cause and had read neither of
    /// them.** Both were already correct and had been for a long time, and the
    /// operator walked to the radio for nothing.</para>
    /// <para>The two names are what the message must not contain until something
    /// has actually established they are the problem.</para>
    /// </remarks>
    [Fact]
    public void TheOutputBeingOffNamesNoSettingAtAll()
    {
        var status = ScopeReadiness.Check(
            Radio, Scope(on: 1, output: 0), sweepsSeen: -1, Link(), writeRefused: false);

        Assert.Equal(ScopeReadyState.OutputOff, status.State);
        Assert.Equal("", status.WhereToLook);

        foreach (var claim in new[] { "CI-V USB Port", "Baud Rate", "MENU", "115200" })
        {
            Assert.DoesNotContain(claim, status.Detail, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <remarks>
    /// Proves HM-DEC-092: where Hamlet does know which condition is unmet, it
    /// says so plainly. The baud rate is one it never has to ask the radio about,
    /// because it opened the port itself, so this is a reading rather than a
    /// candidate.
    /// </remarks>
    [Fact]
    public void ASlowLinkIsNamedBecauseHamletOpenedIt()
    {
        var status = ScopeReadiness.Check(
            Radio, Scope(on: 1, output: 0), sweepsSeen: -1,
            Link(baud: 9_600), writeRefused: false);

        Assert.Equal(ScopeReadyState.LinkTooSlow, status.State);
        Assert.Contains("9600", status.Detail, StringComparison.Ordinal);
        Assert.Contains("115200", status.Detail, StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-092: **a refusal names the remaining candidate as a
    /// candidate.** With the link fast enough and the radio still saying no, one
    /// documented condition is left and Hamlet has no cited way to read it
    /// (HM-OPEN-013). The honest form is "this is what is left to check", not an
    /// instruction phrased as a finding.</para>
    /// </remarks>
    [Fact]
    public void ARefusalSaysWhatIsLeftToCheckAndThatItCouldNotRead()
    {
        var status = ScopeReadiness.Check(
            Radio, Scope(on: 1, output: 0), sweepsSeen: -1, Link(), writeRefused: true);

        Assert.Equal(ScopeReadyState.WriteRefused, status.State);
        Assert.Contains("refused", status.Detail, StringComparison.Ordinal);
        Assert.Contains("no way to read", status.WhereToLook, StringComparison.Ordinal);
        Assert.Contains("CI-V USB Port", status.WhereToLook, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves HM-DEC-092: everything reading as on with nothing arriving is its
    /// own state and its own sentence, and it blames nothing. Three states used
    /// to collapse into one paragraph of advice.
    /// </remarks>
    [Fact]
    public void ThreeStatesGiveThreeDifferentSentences()
    {
        var off = ScopeReadiness.Check(
            Radio, Scope(1, 0), -1, Link(), false);

        var refused = ScopeReadiness.Check(
            Radio, Scope(1, 0), -1, Link(), true);

        var silent = ScopeReadiness.Check(
            Radio, Scope(1, 1), 0, Link(), false);

        Assert.Equal(ScopeReadyState.OutputOff, off.State);
        Assert.Equal(ScopeReadyState.WriteRefused, refused.State);
        Assert.Equal(ScopeReadyState.NothingArriving, silent.State);

        var said = new[] { off.Detail, refused.Detail, silent.Detail };

        Assert.Equal(said.Length, said.Distinct().Count());
        Assert.All(said, one => Assert.NotEqual("", one));
        Assert.Equal("", silent.WhereToLook);
    }

    /// <remarks>
    /// Proves HM-DEC-092: the scope output is a write Hamlet makes, and the
    /// scope's own on/off switch is not. One decides whether the picture the
    /// radio already draws is also sent down the cable; the other changes what
    /// the operator sees on their own radio.
    /// </remarks>
    [Fact]
    public void TheOutputIsAWriteAndTheScopeItselfIsNot()
    {
        var write = Assert.Single(
            CivWrites.All, w => w.Command == CivConstants.CmdScope);

        Assert.Equal(new byte[] { 0x11 }, write.SubCommand);
        Assert.Equal(RigWriteTier.Receive, write.Tier);
        Assert.Equal("19-7", write.Page);
    }

    /// <remarks>
    /// Proves HM-DEC-092: the link reports what it knows about itself, and not
    /// having looked is different from having looked and found nothing
    /// (HM-DEC-050).
    /// </remarks>
    [Fact]
    public void TheLinkKnowsWhetherItIsFastEnough()
    {
        Assert.Null(CivLinkHealth.Unknown.FastEnoughForScope);
        Assert.True(Link().FastEnoughForScope);
        Assert.False(Link(baud: 9_600).FastEnoughForScope);

        Assert.True(Link().IsHealthy);
        Assert.False(
            new CivLinkHealth("COM3", 115_200, 10, 8, 2, 0x16, Now).IsHealthy);

        Assert.Null(CivLinkHealth.Unknown.AnsweredShare);
        Assert.Equal(0.8, new CivLinkHealth("COM3", 115_200, 10, 8, 2, null, null)
            .AnsweredShare!.Value, precision: 6);
    }
}
