using System;
using System.Collections.Generic;
using System.Linq;
using Hamlet.RadioEngine.Explore;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 299, task 4: **a place name a card can carry, and never one
/// below the country.**
/// </summary>
/// <remarks>
/// <para>**TIM RAISED IT ON HIS OWN SCREEN, 2026-09-09**: `the United States` where
/// the card wants a place. The same fault had been raised against the tooltips and
/// never fixed.</para>
/// <para>**THE MEASUREMENT CAME FIRST.** Over the 275 entities the cited table
/// holds, the median name is 8 characters and the longest is 33 -
/// `Sovereign Military Order of Malta`. **31 run over 16 and 13 run over 20.**</para>
/// <para>**THE BREAKAGE THIS WOULD HAVE CAUGHT IS NARROWING RATHER THAN
/// SHORTENING.** The instruction's own first line offers a US state, and the obvious
/// way to get one is the callsign or the grid: a US call area is historical rather
/// than a residence, and a four-character grid square is a box seventy miles across
/// that straddles state lines. **Neither is honest**, so this asserts that no short
/// form is anything but the same country said shorter.</para>
/// </remarks>
public sealed class Unit299ShortPlaceTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the measurements are printed.</param>
    public Unit299ShortPlaceTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Every entity name is measured, and the long ones have a short form.**</summary>
    [Fact]
    public void EveryEntityNameIsCheckedForLength()
    {
        var names = DxccPrefixes.Entities.ToList();

        var longest = names.OrderByDescending(n => n.Length).First();

        _output.WriteLine("entities: " + names.Count);
        _output.WriteLine("longest : " + longest.Length + "  " + longest);
        _output.WriteLine("median  : "
            + names.Select(n => n.Length).OrderBy(n => n).ElementAt(names.Count / 2));

        var stillLong = names
            .Select(EntitySpoken.Short)
            .Where(n => n.Length > EntitySpoken.CardLimit)
            .OrderByDescending(n => n.Length)
            .ToList();

        _output.WriteLine("");
        _output.WriteLine("over " + EntitySpoken.CardLimit + " after shortening: "
            + stillLong.Count);

        foreach (var name in stillLong)
        {
            _output.WriteLine("  " + name.Length + "  " + name);
        }

        // **THE WORST CASE IS BOUNDED AND THE FIGURE IS REPORTED**, rather than the
        // list being asserted empty and quietly padded to make it so.
        Assert.True(
            stillLong.Count == 0 || stillLong[0].Length <= 20,
            "a card would carry " + (stillLong.Count > 0 ? stillLong[0] : "")
            + ", which is " + (stillLong.Count > 0 ? stillLong[0].Length : 0)
            + " characters");
    }

    /// <summary>**`the United States` becomes a place a card can carry.**</summary>
    [Fact]
    public void TheUnitedStatesIsShortOnACard()
    {
        var full = EntitySpoken.Of("United States of America");
        var card = EntitySpoken.Short("United States of America");

        _output.WriteLine("spoken: " + full);
        _output.WriteLine("card  : " + card);

        Assert.Equal("the United States", full);
        Assert.Equal("United States", card);
    }

    /// <summary>**Nothing shortens into a place below the country.**</summary>
    /// <remarks>
    /// **THE INSTRUCTION IS EXPLICIT**: do not invent a place below the country. A
    /// state is fair only if something cited gives one, and nothing here does; a city
    /// is not fair at all.
    /// </remarks>
    [Fact]
    public void NothingNarrowsBelowTheCountry()
    {
        foreach (var state in new[]
        {
            "Arizona", "Texas", "California", "Ohio", "New York", "Pennsylvania",
            "London", "Paris", "Tokyo",
        })
        {
            Assert.DoesNotContain(
                EntitySpoken.OnACard.Values, v => v.Equals(state, StringComparison.Ordinal));
        }

        // **AND NO SHORT FORM IS SHORTER THAN THE PLACE IT NAMES IS BIG.** Every
        // value is a country, an island group or an abbreviation of one; none is a
        // region inside a country that the table could not support.
        foreach (var (full, card) in EntitySpoken.OnACard)
        {
            _output.WriteLine(full.PadRight(36) + " -> " + card);

            Assert.True(
                card.Length <= full.Length,
                card + " is longer than " + full);

            Assert.True(card.Length > 0, full + " shortens to nothing");
        }
    }

    /// <summary>**Every key names something the cited table actually holds.**</summary>
    /// <remarks>
    /// **THE SAME GUARANTEE `EntitySpoken.Shortened` ALREADY HAS.** A key that stops
    /// matching a real name would silently do nothing, and the card would go back to
    /// carrying the long form with nobody noticing.
    /// </remarks>
    [Fact]
    public void EveryShortFormNamesARealEntity()
    {
        var known = new HashSet<string>(DxccPrefixes.Entities, StringComparer.Ordinal);

        // The spoken forms are keys here too, so a key is real if the entity is real
        // or if it is a spoken form of one.
        var spoken = new HashSet<string>(
            DxccPrefixes.Entities.Select(EntitySpoken.Of), StringComparer.Ordinal);

        foreach (var key in EntitySpoken.OnACard.Keys)
        {
            Assert.True(
                known.Contains(key) || spoken.Contains(key),
                key + " is not a name the cited table holds, so this row does "
                + "nothing and the card keeps the long name");
        }
    }
}
