using System;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 308 task 6: **what a marked row says on a deliberate look.**
/// </summary>
/// <remarks>
/// <para>**A DOOR MAY BE MARKED AND NOT NAMED** (Tim, 2026-09-10). §3.1 is *absent,
/// not dimmed*: the CQ list must not be the thing that tells him Eastern Europe
/// exists.</para>
/// <para>**IT SAYS *WORKED*, NEVER *CONFIRMED*** (§4). Hamlet has contacts and no
/// confirmations, and nothing here shames, implies he is behind, or promises the
/// contact will succeed.</para>
/// <para>**THE DOOR SENTENCE IS A PLACEHOLDER.** Wording is the product (§3.5) and no
/// ruling has been given on it; it is built so it can be read and the question is in
/// the report.</para>
/// </remarks>
public sealed class TheNudgeHoverTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the wording is printed.</param>
    public TheNudgeHoverTests(ITestOutputHelper output) => _output = output;

    /// <summary>The ceiling unit 306 set for a hover row.</summary>
    private const int LongestRow = 66;

    /// <summary>**A visible card is named.**</summary>
    [Fact]
    public void AVisibleCardIsNamed()
    {
        var words = NudgeWords.For(NudgeKind.Visible, "Costa Rica");

        _output.WriteLine(words.Length.ToString().PadLeft(3) + "  " + words);

        Assert.Contains("Costa Rica", words, StringComparison.Ordinal);
        Assert.Contains("new country", words, StringComparison.Ordinal);

        Assert.True(
            words.Length <= LongestRow,
            "the hover is " + words.Length + " characters");
    }

    /// <summary>**A door names no area at all.**</summary>
    [Fact]
    public void ADoorNamesNoArea()
    {
        var words = NudgeWords.For(NudgeKind.Door, "");

        _output.WriteLine(words.Length.ToString().PadLeft(3) + "  " + words);

        Assert.Contains("new area", words, StringComparison.Ordinal);

        // **NOT ONE CONTINENT IS NAMED**, which is the whole of §3.1 here.
        foreach (var area in new[]
        {
            "Europe", "Africa", "Asia", "South America", "North America",
            "Oceania", "Antarctica", "Eastern", "Western",
        })
        {
            Assert.DoesNotContain(area, words, StringComparison.OrdinalIgnoreCase);
        }

        Assert.True(
            words.Length <= LongestRow,
            "the hover is " + words.Length + " characters");
    }

    /// <summary>**An entity handed to a door is still not named.**</summary>
    /// <remarks>
    /// **BELT AND BRACES, AND DELIBERATE.** `NudgeSet` already withholds the entity
    /// for a door; this says that even if a caller passed one, the words would not
    /// carry it.
    /// </remarks>
    [Fact]
    public void AnEntityHandedToADoorIsStillNotNamed()
    {
        var words = NudgeWords.For(NudgeKind.Door, "Brazil");

        _output.WriteLine(words);

        Assert.DoesNotContain("Brazil", words, StringComparison.Ordinal);
    }

    /// <summary>**Nothing says confirmed, and nothing shames.**</summary>
    [Fact]
    public void NothingSaysConfirmedAndNothingShames()
    {
        foreach (var words in new[]
        {
            NudgeWords.For(NudgeKind.Visible, "Costa Rica"),
            NudgeWords.For(NudgeKind.Door, ""),
        })
        {
            _output.WriteLine("[" + words + "]");

            foreach (var forbidden in new[]
            {
                "confirm", "missing", "behind", "still need", "only", "never",
                "failed", "should",
            })
            {
                Assert.DoesNotContain(
                    forbidden, words, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    /// <summary>**The candidate door sentences, checked but not chosen.**</summary>
    /// <remarks>
    /// <para>**WORDING IS THE PRODUCT (§3.5) AND NO RULING HAS BEEN GIVEN.** This
    /// does not replace the placeholder on its own judgment; it checks that every
    /// candidate put to Tim in the report obeys §4 - **worked, never confirmed** -
    /// names no area, shames nobody and promises nothing.</para>
    /// <para>**THE THREE ARE IN `output.md` SECTION 4 WITH THEIR COSTS.** If one is
    /// ruled in, it replaces `NudgeWords.Door` and this test already covers it.</para>
    /// </remarks>
    [Fact]
    public void EveryCandidateDoorSentenceObeysTheRules()
    {
        foreach (var candidate in new[]
        {
            NudgeWords.Door,
            "new area · somewhere you have not worked yet",
            "new area · this one opens a part of the map",
        })
        {
            _output.WriteLine(
                candidate.Length.ToString().PadLeft(3) + "  " + candidate);

            // **IT NAMES NO AREA.**
            foreach (var area in new[]
            {
                "Europe", "Africa", "Asia", "America", "Oceania", "Antarctica",
                "Eastern", "Western", "Pacific", "Atlantic",
            })
            {
                Assert.DoesNotContain(
                    area, candidate, StringComparison.OrdinalIgnoreCase);
            }

            // **AND IT SAYS WORKED, NEVER CONFIRMED, AND NOTHING SHAMES** (§4).
            foreach (var forbidden in new[]
            {
                "confirm", "missing", "behind", "still need", "failed", "should",
                "only", "never worked",
            })
            {
                Assert.DoesNotContain(
                    forbidden, candidate, StringComparison.OrdinalIgnoreCase);
            }

            // **AND NOTHING PROMISES THE CONTACT WILL SUCCEED.**
            foreach (var promise in new[] { "will open", "guarantee", "you will" })
            {
                Assert.DoesNotContain(
                    promise, candidate, StringComparison.OrdinalIgnoreCase);
            }

            Assert.True(
                candidate.Length <= LongestRow,
                "the candidate is " + candidate.Length + " characters");
        }
    }

    /// <summary>**An unmarked row says nothing at all.**</summary>
    [Fact]
    public void AnUnmarkedRowSaysNothing()
    {
        _output.WriteLine("[" + NudgeWords.For(NudgeKind.None, "Costa Rica") + "]");

        Assert.Empty(NudgeWords.For(NudgeKind.None, "Costa Rica"));

        // **AND A VISIBLE CARD WITH NO ENTITY SAYS NOTHING RATHER THAN SOMETHING
        // VAGUE** (§0.0). A hover reading *new country* with no country in it is a
        // claim with a hole where its fact should be.
        Assert.Empty(NudgeWords.For(NudgeKind.Visible, ""));
    }
}
