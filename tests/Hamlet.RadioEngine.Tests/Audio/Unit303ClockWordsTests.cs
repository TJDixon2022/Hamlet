using System;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// Work instruction 303 task 3: **the clock line says the fault that is true.**
/// </summary>
/// <remarks>
/// <para>**THE SENTENCE ON SCREEN WAS FALSE FOR ONE OF THE TWO STATES IT COVERED.**
/// `ClockOffset.Describe` says *clock not checked yet, so slots cannot be cut*
/// whenever there is no offset, and there are two ways to have none: nothing has
/// asked yet, or something asked and it failed. **Saying *not checked yet* about a
/// query that was made and failed is the application asserting something it does not
/// know** (§0.0), and it is the sentence that sent twenty minutes to the wrong
/// machine.</para>
/// <para>**IT WAS WATCHED FAILING.** With the line composed from
/// `ClockOffset.Describe` alone, `AFailedQueryDoesNotSayItWasNeverChecked` reports
/// *clock not checked yet* for a query that timed out.</para>
/// </remarks>
public sealed class Unit303ClockWordsTests
{
    private static readonly DateTime Now =
        new(2026, 9, 10, 15, 0, 0, DateTimeKind.Utc);

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the lines are quoted.</param>
    public Unit303ClockWordsTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Nothing has come back yet, and the line says so.**</summary>
    [Fact]
    public void BeforeAnythingHasAnsweredTheLineSaysItIsAsking()
    {
        var line = ClockWords.Line(ClockOffset.Unknown, null, Now);

        _output.WriteLine(line);

        Assert.Contains("is asking", line, StringComparison.Ordinal);

        // **IT DOES NOT CLAIM A CHECK WAS MADE AND FAILED.**
        Assert.DoesNotContain("not checked yet", line, StringComparison.Ordinal);
    }

    /// <summary>**A failed query does not say it was never checked.**</summary>
    [Theory]
    [InlineData("timeout", "nothing came back")]
    [InlineData("short_reply", "too short")]
    [InlineData("unreadable_timestamp", "no time in it")]
    [InlineData("socket_HostNotFound", "could not be looked up")]
    [InlineData("socket_NetworkUnreachable", "network refused")]
    [InlineData("threw_InvalidOperationException", "inside Hamlet")]
    public void AFailedQueryDoesNotSayItWasNeverChecked(
        string reason, string expected)
    {
        var line = ClockWords.Line(
            ClockOffset.Unknown,
            ClockAnswer.Failed("pool.ntp.org", reason, "the detail"),
            Now);

        _output.WriteLine(reason.PadRight(32) + line);

        Assert.Contains(expected, line, StringComparison.Ordinal);

        // **THE FALSE SENTENCE IS GONE FROM EVERY FAILURE BRANCH.**
        Assert.DoesNotContain("not checked yet", line, StringComparison.Ordinal);

        // **AND THE SERVER IS NAMED**, so he knows what Hamlet was talking to.
        Assert.Contains("pool.ntp.org", line, StringComparison.Ordinal);
    }

    /// <summary>**A measured offset reads exactly as it always did.**</summary>
    /// <remarks>
    /// **THE ORDINARY CASE IS UNTOUCHED.** Unit 302 quoted this line as *clock is
    /// 1.68 s slow, checked just now*, and it still reads that way.
    /// </remarks>
    [Fact]
    public void AMeasuredOffsetReadsAsItAlwaysDid()
    {
        var line = ClockWords.Line(
            new ClockOffset(1.66, Now.AddSeconds(-10)),
            ClockAnswer.Measured(
                "pool.ntp.org",
                new ClockOffset(1.66, Now.AddSeconds(-10)),
                TimeSpan.FromMilliseconds(38)),
            Now);

        _output.WriteLine(line);

        Assert.Equal("clock is 1.66 s slow, checked just now", line);
    }

    /// <summary>**The two states are different sentences, which is the whole point.**</summary>
    /// <remarks>
    /// One is *wait a moment* and the other is *something is wrong*. Before this
    /// unit they were the same words.
    /// </remarks>
    [Fact]
    public void WaitingAndFailingAreDifferentSentences()
    {
        var waiting = ClockWords.Line(ClockOffset.Unknown, null, Now);

        var failed = ClockWords.Line(
            ClockOffset.Unknown,
            ClockAnswer.Failed("pool.ntp.org", "timeout", "nothing came back"),
            Now);

        _output.WriteLine("waiting : " + waiting);
        _output.WriteLine("failed  : " + failed);

        Assert.NotEqual(waiting, failed);
    }

    /// <summary>**No line uses more than one dash, and none is a stack of fragments.**</summary>
    /// <remarks>
    /// **§0.7 AND HM-DEC-040.** These are sentences the operator reads on a strip
    /// that is already carrying a fault, so they are held to the same voice as
    /// everything else he reads.
    /// </remarks>
    [Fact]
    public void EveryLineIsInTheApplicationsVoice()
    {
        foreach (var line in new[]
        {
            ClockWords.Line(ClockOffset.Unknown, null, Now),
            ClockWords.Line(
                ClockOffset.Unknown,
                ClockAnswer.Failed("pool.ntp.org", "timeout", "x"), Now),
            ClockWords.Line(
                ClockOffset.Unknown,
                ClockAnswer.Failed("pool.ntp.org", "socket_HostNotFound", "x"), Now),
        })
        {
            var dashes = line.Split('—').Length - 1;

            _output.WriteLine(dashes + " dashes  " + line);

            Assert.True(dashes <= 1, "more than one em dash: " + line);
        }
    }
}
