using System;
using System.Linq;
using Hamlet.App.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 278, task 4: the badges, at nine thresholds, for the count and
/// for nothing else.
/// </summary>
/// <remarks>
/// <para>**TIM'S RULING, 2026-09-08**, in his own words: *"just sort of an
/// acknowledgment that you're making contacts."* Ten, twenty-five, fifty, a hundred,
/// five hundred, a thousand, two thousand, five thousand, ten thousand.</para>
/// <para>**THE CASE THAT CATCHES A NAIVE IMPLEMENTATION IS NINE TO TWENTY-SIX**,
/// which crosses two thresholds in one step. Reporting only the highest would
/// quietly swallow the ten, and a quiet evening on FT8 does exactly this.</para>
/// </remarks>
public sealed class TheBadgesCountContactsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the badge lines are printed.</param>
    public TheBadgesCountContactsTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>**Nine contacts earn nothing.**</summary>
    [Fact]
    public void NineEarnsNothing()
    {
        var at = new ContactMilestones(9);

        _output.WriteLine("9: " + at.Line);

        Assert.Empty(at.Earned);
        Assert.False(at.HasAny);
        Assert.Null(at.Highest);
        Assert.Equal(10, at.Next);
        Assert.Equal(1, at.ToGo);
        Assert.Equal("", at.Announcement(0));
    }

    /// <summary>**Ten earns the first, exactly at the threshold.**</summary>
    /// <remarks>
    /// **AT, NOT PAST.** A badge earned only at eleven would mean the tenth contact
    /// of his life passed without the acknowledgement it is named for.
    /// </remarks>
    [Fact]
    public void TenEarnsTheFirst()
    {
        var at = new ContactMilestones(10);

        _output.WriteLine("10: " + at.Line);
        _output.WriteLine("10: " + at.Announcement(0));

        Assert.Equal(new[] { 10 }, at.Earned.ToArray());
        Assert.Equal(10, at.Highest);
        Assert.Equal(25, at.Next);
        Assert.Equal(15, at.ToGo);
        Assert.Equal("That is 10 contacts logged.", at.Announcement(0));
    }

    /// <summary>
    /// **Nine to twenty-six earns both the ten and the twenty-five.**
    /// </summary>
    /// <remarks>
    /// **THE TEST THE INSTRUCTION NAMES**, and the one a naive comparison against
    /// the newest threshold fails. Both are said, because he passed both and being
    /// told only about the higher one loses the smaller.
    /// </remarks>
    [Fact]
    public void AJumpEarnsEveryBadgeItPassed()
    {
        var was = new ContactMilestones(9);
        var now = new ContactMilestones(26);

        _output.WriteLine("9 -> 26 earned: "
            + string.Join(", ", now.EarnedSince(was.Highest ?? 0)));
        _output.WriteLine("9 -> 26 says:   " + now.Announcement(was.Highest ?? 0));

        Assert.Equal(new[] { 10, 25 }, now.EarnedSince(0).ToArray());

        // **BOTH NAMED, NOT ONLY THE HIGHEST.**
        var said = now.Announcement(0);

        Assert.Contains("10", said, StringComparison.Ordinal);
        Assert.Contains("25", said, StringComparison.Ordinal);
        Assert.Equal("That is 10 and 25 contacts logged.", said);
    }

    /// <summary>**Said once: a second read after the same jump says nothing.**</summary>
    /// <remarks>
    /// He is often mid-exchange, and a congratulation that arrives every time the
    /// log is read is an interruption rather than an acknowledgement.
    /// </remarks>
    [Fact]
    public void ItIsSaidOnceAndNotAgain()
    {
        var now = new ContactMilestones(26);

        Assert.NotEqual("", now.Announcement(0));

        // Once 25 has been announced, the same count says nothing further.
        Assert.Equal("", now.Announcement(25));

        // And a later contact that crosses nothing stays quiet too.
        Assert.Equal("", new ContactMilestones(30).Announcement(25));
    }

    /// <summary>
    /// **A shrunken log shows fewer badges, and that is the stated rule.**
    /// </summary>
    /// <remarks>
    /// <para>**THE INSTRUCTION ASKS WHAT HAPPENS IF THE FILE SHRINKS RATHER THAN
    /// LEAVING IT UNDEFINED**, and this is the answer: **earned is derived from the
    /// count every time and never remembered.**</para>
    /// <para>**WHY THAT WAY ROUND** (§0.0). A badge says the log holds a hundred
    /// contacts. If it holds ninety, Hamlet cannot see a hundred, and a remembered
    /// high-water mark would keep asserting one on a new machine or after a restore
    /// from an old backup, with no evidence behind it. The log only grows in normal
    /// use, so nothing is lost by being honest in the case where it does not.</para>
    /// </remarks>
    [Fact]
    public void AShrunkenLogShowsFewerBadges()
    {
        Assert.Equal(new[] { 10, 25 }, new ContactMilestones(30).Earned.ToArray());

        // The file loses half its records. Hamlet can no longer see twenty-five.
        var shrunk = new ContactMilestones(15);

        _output.WriteLine("30 -> 15: " + shrunk.Line);

        Assert.Equal(new[] { 10 }, shrunk.Earned.ToArray());
        Assert.Equal(10, shrunk.Highest);

        // **AND IT DOES NOT RE-CONGRATULATE HIM ON THE WAY BACK UP**, because what
        // was announced is remembered even though what was earned is not.
        Assert.Equal("", new ContactMilestones(30).Announcement(25));
    }

    /// <summary>**Every threshold he ruled, and no others.**</summary>
    /// <remarks>
    /// **NO BADGE FOR ANYTHING BUT THE COUNT.** Not a country, not a band, not a
    /// first contact of the day. The list is asserted whole so a tenth cannot be
    /// added without this failing and somebody having to justify it.
    /// </remarks>
    [Fact]
    public void TheThresholdsAreTheNineHeRuled()
    {
        Assert.Equal(
            new[] { 10, 25, 50, 100, 500, 1000, 2000, 5000, 10000 },
            ContactMilestones.Thresholds.ToArray());

        foreach (var threshold in ContactMilestones.Thresholds)
        {
            // One short of each earns everything below it and not that one.
            var below = new ContactMilestones(threshold - 1);

            Assert.DoesNotContain(threshold, below.Earned);
            Assert.Equal(threshold, below.Next);
            Assert.Equal(1, below.ToGo);

            Assert.Contains(threshold, new ContactMilestones(threshold).Earned);
        }
    }

    /// <summary>**At ten thousand there is no next one, and it says so.**</summary>
    [Fact]
    public void AtTheTopThereIsNoNextBadge()
    {
        var at = new ContactMilestones(10000);

        _output.WriteLine("10000: " + at.Line);

        Assert.Null(at.Next);
        Assert.Null(at.ToGo);
        Assert.Equal(10000, at.Highest);
        Assert.Contains("every badge there is", at.Line, StringComparison.Ordinal);

        // And nothing is announced past it.
        Assert.Equal("", new ContactMilestones(12000).Announcement(10000));
    }

    /// <summary>**The lines read like a person, and carry no em dash.**</summary>
    /// <remarks>
    /// §0.7. `VoiceTests` sweeps the source for the two-dash rule; this asserts the
    /// sentences this unit adds, so a failure names them rather than a file.
    /// </remarks>
    [Fact]
    public void TheLinesAreSentences()
    {
        foreach (var count in new[] { 0, 9, 10, 26, 100, 9999, 10000 })
        {
            var line = new ContactMilestones(count).Line;

            _output.WriteLine(count + ": " + line);

            Assert.NotEqual("", line);
            Assert.DoesNotContain("—", line);
            Assert.EndsWith(".", line, StringComparison.Ordinal);
        }
    }
}
