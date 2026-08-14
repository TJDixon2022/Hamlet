using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// What Hamlet says about the radio's own RTTY decoder, and what it does not do
/// about it (HM-DEC-069).
/// </summary>
/// <remarks>
/// <para>The IC-7300 decodes RTTY by itself and will send the decoded text down
/// the USB cable. The catch is that "USB Serial Function" is one setting with two
/// options, CI-V or RTTY Decode, on one port (Full Manual p. 12-9, publication
/// A7292-4EX-5). Taking the decoded text costs rig control entirely.</para>
/// <para>So the constraint is a fact about the radio and it belongs in the field
/// guide where somebody reads about the mode. These hold that it is said, and
/// that nothing in the engine offers to make the switch.</para>
/// </remarks>
public sealed class RttyConstraintTests
{
    private static ModeInfo Rtty
        => ModeGuide.Modes.Single(m =>
            string.Equals(m.Name, "RTTY", StringComparison.Ordinal));

    /// <remarks>
    /// Proves HM-DEC-069: the tradeoff is said plainly where somebody meets the
    /// mode, rather than discovered by losing the radio.
    /// </remarks>
    [Fact]
    public void TheFieldGuideSaysWhatTakingTheDecodedTextCosts()
    {
        var why = Rtty.Why;

        Assert.Contains("decodes this one by itself", why, StringComparison.Ordinal);
        Assert.Contains("one setting", why, StringComparison.Ordinal);
        Assert.Contains("lose the radio", why, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves HM-DEC-069: the choice is the operator's and it is made at the
    /// radio. Nothing in the engine names the setting as something to write, and
    /// a switch that severed CI-V could not be undone by the thing that severed
    /// it.
    /// </remarks>
    [Fact]
    public void NothingOffersToMakeTheSwitch()
    {
        var why = Rtty.Why.ToLowerInvariant();

        Assert.Contains("at the radio's own screen", why, StringComparison.Ordinal);

        foreach (var offer in new[]
                 { "hamlet can switch", "turn it on for you", "click here", "let hamlet" })
        {
            Assert.False(why.Contains(offer, StringComparison.Ordinal),
                $"the guide offers to do it: '{offer}'");
        }
    }
}
