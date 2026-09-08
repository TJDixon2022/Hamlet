using System;
using System.Linq;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Explore;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 281, task 5: a message he sent is not explained back to him.
/// </summary>
/// <remarks>
/// <para>**OBSERVED 2026-09-08, ON HIS OWN CQ**: *KC3QIS is calling anyone. He is in
/// United States of America, in grid FN00, and Hamlet needs your own grid square in
/// Settings before it can say how far away that is.* Three things wrong with one
/// sentence, and two of them are this task: **he knows who sent it**, and the name
/// reads long where every other entity reads short.</para>
/// <para>Every sentence the vocabulary table produces is about the sender — who they
/// are, where they are, what they are asking for. On his own transmission the sender
/// is him, and he composed it, saw it, and clicked once to send it.</para>
/// </remarks>
public sealed class TheSenderTooltipIsNotAboutHimTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tooltips are printed.</param>
    public TheSenderTooltipIsNotAboutHimTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>**A sent row explains nothing; a received one still does.**</summary>
    /// <remarks>
    /// Watched failing first: before this task a sent row carried the same two
    /// sentences a received one does, about the operator himself.
    /// </remarks>
    [Fact]
    public void ASentMessageHasNoSenderTooltipAndAReceivedOneDoes()
    {
        var sent = Row("CQ KC3QIS FN00", isSent: true);
        var heard = Row("CQ W3YNI FN20", isSent: false);

        _output.WriteLine("he sent    : sender [" + sent.SenderHelp
            + "]  payload [" + sent.PayloadHelp + "]");
        _output.WriteLine("he heard   : sender [" + heard.SenderHelp
            + "]  payload [" + heard.PayloadHelp + "]");

        // **NOTHING AT ALL ON HIS OWN.** Avalonia shows no tooltip for an empty
        // tip, so an empty string here is the absence and not a blank box.
        Assert.Equal("", sent.SenderHelp);
        Assert.Equal("", sent.PayloadHelp);
        Assert.False(sent.HasPayloadHelp);

        // **AND THE OTHER STATION IS STILL EXPLAINED**, which is what the tooltips
        // are for. Taking his own out must not take the useful case with it.
        Assert.NotEqual("", heard.SenderHelp);
        Assert.Contains("W3YNI", heard.SenderHelp, StringComparison.Ordinal);
        Assert.True(heard.HasPayloadHelp);

        // **AND THE DIRECTION IS STILL THERE**, so the fact that he sent it did not
        // leave the screen with the explanation (§0.0, HM-DEC-092).
        _output.WriteLine("and the caption hover reads: " + sent.DirectionTip);
        Assert.Contains("You sent this", sent.DirectionTip, StringComparison.Ordinal);
    }

    /// <summary>**The entity is said the way a person says it.**</summary>
    /// <remarks>
    /// §0.7: the ordinary word beats the correct one where they differ. The cited
    /// ARRL list is untouched — this is a spoken form laid over it.
    /// </remarks>
    [Fact]
    public void TheLongEntityNamesAreShortenedForSpeech()
    {
        foreach (var (arrl, spoken) in EntitySpoken.Shortened
            .Select(p => (p.Key, p.Value)))
        {
            _output.WriteLine(arrl.PadRight(34) + spoken);

            // **EVERY KEY IS A REAL ARRL NAME.** A key that stops matching one —
            // because the list was re-transcribed, or a name changed — would
            // silently do nothing, which is the worst of both.
            Assert.True(
                DxccPrefixes.Entities.Contains(arrl),
                "\"" + arrl + "\" is not a name in the cited DXCC list");

            Assert.True(
                spoken.Length < arrl.Length,
                "\"" + spoken + "\" is not shorter than \"" + arrl + "\"");
        }

        Assert.Equal("the United States", EntitySpoken.Of("United States of America"));

        // Anything not on the list is returned exactly as the ARRL wrote it.
        Assert.Equal("Monaco", EntitySpoken.Of("Monaco"));
        Assert.Equal("Canary Is.", EntitySpoken.Of("Canary Is."));
        Assert.Equal("", EntitySpoken.Of(null));

        // **AND NO SHORTENED NAME TAKES A COMPASS WORD**, which is what keeps the
        // article out of trouble: *southern the United States* would be the price
        // of shortening one of the ten entities that get a qualifier.
        foreach (var arrl in EntitySpoken.Shortened.Keys)
        {
            Assert.Equal(arrl, EntityQualifier.Describe(arrl, 40.0));
        }
    }

    /// <summary>**The sentence he was shown no longer names him at all.**</summary>
    [Fact]
    public void HisOwnCqIsNotDescribedBackToHim()
    {
        var sent = Row("CQ KC3QIS FN00", isSent: true);

        Assert.DoesNotContain("KC3QIS", sent.PayloadHelp, StringComparison.Ordinal);
        Assert.DoesNotContain("KC3QIS", sent.SenderHelp, StringComparison.Ordinal);

        // And when somebody else sends the same shape, the long name is gone from
        // it rather than merely being hidden on his own row.
        var heard = Row("CQ K4XYZ FN20", isSent: false);

        _output.WriteLine(heard.PayloadHelp);

        Assert.DoesNotContain(
            "United States of America", heard.PayloadHelp, StringComparison.Ordinal);
        Assert.Contains(
            "the United States", heard.PayloadHelp, StringComparison.Ordinal);
    }

    /// <summary>One row of the conversation.</summary>
    /// <param name="message">What was on the air.</param>
    /// <param name="isSent">True where the operator sent it.</param>
    /// <returns>The row.</returns>
    private static DigitalDecodeRow Row(string message, bool isSent)
    {
        // The record's own shape: `IsSent` and `ObserverGrid` are set where a row
        // is built, so a test that mutated them afterwards would be proving
        // something the application cannot produce.
        return new DigitalDecodeRow(
            "214130", isSent ? "" : "-11", isSent ? "" : "0.2", "1240", message,
            ObserverGrid: "FN00DJ",
            SlotStartUtc: new DateTime(2026, 9, 8, 21, 41, 30, DateTimeKind.Utc),
            IsSent: isSent);
    }
}
