using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// He finds out a reply needs two sends while he is typing it.
/// </summary>
/// <remarks>
/// <para>**A REAL EXCHANGE DOES NOT FIT IN THIRTY CHARACTERS.** His call, the
/// other station's, a report and a name is past the keyer's limit before anybody
/// has said anything unusual, so a reply going out as two sends is the ordinary
/// case. Nothing told him until it had gone.</para>
/// <para>**AND THE SEAM IS NOT PRETENDED AWAY** (HM-DEC-130). The single send
/// splits at the spaces; what nobody has measured is how long the gap between the
/// two sends is, which is why that ruling refused to ship a split for the calling
/// cycle. The line says both: it will take two sends, and the pause in the middle
/// is an unmeasured length.</para>
/// </remarks>
public sealed class SendLengthIsLegibleTests
{
    private static SendButtonViewModel Button(string message)
        => new(new SendOption(
            ContactStage.Calling, "Send", message, "what it means", "why"));

    /// <remarks>
    /// Proves the ordinary case stays quiet: a message that fits says nothing, and
    /// a caption on every send is a caption nobody reads.
    /// </remarks>
    [Fact]
    public void AMessageThatFitsSaysNothing()
    {
        var button = Button("RR TU 73");

        Assert.False(button.WillSplit);
        Assert.Equal(1, button.PiecesNow);
        Assert.Equal("", button.LengthNote);
    }

    /// <remarks>
    /// **THE ONE THAT WOULD HAVE BITTEN HIM TONIGHT.** An exchange with two calls,
    /// a report and a name, typed into the box, and the panel says before he
    /// presses that it goes out in two.
    /// </remarks>
    [Fact]
    public void AnExchangeThatNeedsTwoSendsSaysSoWhileHeTypes()
    {
        var button = Button("KC3QIS DE W1AW UR RST 599 599 NAME TIM TIM BK");

        Assert.True(button.WillSplit);
        Assert.True(button.PiecesNow >= 2);
        Assert.Contains("two sends", button.LengthNote.Replace("2 sends", "two sends", StringComparison.Ordinal), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("gap in the middle", button.LengthNote, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves it follows the typing rather than the offer: the button is built
    /// from a short message and the operator makes it long, which is exactly how
    /// it happens in a contact.
    /// </remarks>
    [Fact]
    public void EditingAShortMessageIntoALongOneChangesTheLine()
    {
        var button = Button("RR TU 73");

        Assert.Equal("", button.LengthNote);

        button.Message = "KC3QIS DE W1AW UR RST 599 599 NAME TIM TIM BK";

        Assert.True(button.WillSplit);
        Assert.NotEqual("", button.LengthNote);
    }

    /// <remarks>
    /// Proves the keying time is spoken from the radio's own keyer speed when
    /// there is one, and that an unread speed does not stop the line appearing —
    /// `CwDuration` decides what to do about zero in one place (§0.0).
    /// </remarks>
    [Fact]
    public void TheKeyingTimeFollowsTheRadiosOwnSpeed()
    {
        var slow = Button("KC3QIS DE W1AW UR RST 599 599 NAME TIM TIM BK");
        var fast = Button("KC3QIS DE W1AW UR RST 599 599 NAME TIM TIM BK");

        slow.KeyerWpm = 12;
        fast.KeyerWpm = 30;

        var slowSeconds = CwDuration.Of(slow.Message, 12).TotalSeconds;
        var fastSeconds = CwDuration.Of(fast.Message, 30).TotalSeconds;

        Assert.True(slowSeconds > fastSeconds);
        Assert.NotEqual(slow.LengthNote, fast.LengthNote);

        var unread = Button("KC3QIS DE W1AW UR RST 599 599 NAME TIM TIM BK");
        Assert.NotEqual("", unread.LengthNote);
    }
}
