using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 300, tasks 3 and 4: **the screen reads as a collection, and
/// every ring says what its number is a percentage of.**
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THIS WOULD HAVE CAUGHT IS A RING READING 63% WITH NOTHING
/// BESIDE IT**, which is this unit's §0.0 exposure. A percentage on a screen about
/// his own record is read as *how far along am I*, and the reader who is hurt by it
/// is the one who did not stop to wonder what the denominator was. **Distance
/// covered is arithmetic and likelihood of getting there is not**, and the next
/// thousand miles is far harder than the last.</para>
/// <para>**AND ITS TWIN: A FABRICATED FRACTION.** There is no half-worked band and
/// no part of a continent reached, so a ring there would have to pick a denominator
/// that makes a number appear. Those rings are open - dashed, with the target inside
/// them and no percentage anywhere on them.</para>
/// <para>**IT WAS WATCHED FAILING.** With the band challenge given
/// `HasRing = true` and `Ring = 0`, `NoRingIsFabricated` reports it drawing `0%` for
/// a target that has no fraction to be part of.</para>
/// </remarks>
public sealed class Unit300CollectionTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the rings are printed.</param>
    public Unit300CollectionTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Your best is a row of tiles, and each says its figure once.**</summary>
    [Fact]
    public void YourBestIsTilesAndSaysItsFigureOnce()
    {
        var screen = Screen();

        Assert.True(screen.HasBest, "there is no best to show");

        foreach (var tile in screen.Best)
        {
            _output.WriteLine(
                tile.Figure.PadRight(18) + "| " + tile.Title.PadRight(34)
                + "| " + tile.TileLine);
        }

        // **THE TAB IT CAME FROM IS NOT ALSO ON THE SCREEN**, or the same record is
        // drawn twice and reads as two.
        Assert.DoesNotContain(screen.Tabs, t => t.Kind == "all");
        Assert.Contains(screen.Tabs, t => t.Kind == "band");

        var furthest = screen.Best.Single(
            c => c.Title.StartsWith("Furthest", StringComparison.Ordinal));

        _output.WriteLine("");
        _output.WriteLine("furthest tile figure : " + furthest.Figure);
        _output.WriteLine("furthest tile line   : " + furthest.TileLine);

        // **THE FIGURE IS THE BIG THING AND THE LINE UNDER IT IS WHO AND WHERE**,
        // which is unit 299's caption fault kept out of a smaller frame: a tile that
        // repeats its own number reads as two facts.
        Assert.Contains("miles", furthest.Figure, StringComparison.Ordinal);
        Assert.DoesNotContain("miles", furthest.TileLine, StringComparison.Ordinal);
        Assert.Contains("VP2MAA", furthest.TileLine, StringComparison.Ordinal);
    }

    /// <summary>**A place carries worked of total, and the bar is that same count.**</summary>
    [Fact]
    public void APlaceCarriesWorkedOfTotalAndTheBarIsThatCount()
    {
        var screen = Screen();

        foreach (var place in screen.Places)
        {
            _output.WriteLine(
                place.Title.PadRight(18) + "| " + place.Summary.PadRight(16)
                + "| bar " + (place.HasFraction
                    ? Math.Round(place.Fraction * 100).ToString(
                        "0", CultureInfo.InvariantCulture) + "%"
                    : "(none)")
                + " | " + place.Count + " named"
                + (place.HasNudge ? " | " + place.Nudge : ""));
        }

        var north = screen.Places.Single(p => p.Title == "North America");

        Assert.True(north.HasFraction, "the continent has no bar");
        Assert.Equal(north.Summary, north.Worked + " of " + north.Total + " worked");

        // **THE BAR AND THE WORDS ARE THE SAME TWO NUMBERS**, so there is nothing on
        // this card whose meaning has to be guessed at.
        Assert.Equal((double)north.Worked / north.Total, north.Fraction, 6);

        Assert.True(north.Count > 0, "the group names no entity");
    }

    /// <summary>**Every ring says what its number is a percentage of.**</summary>
    [Fact]
    public void EveryRingSaysWhatItsNumberIsAPercentageOf()
    {
        var log = Log();
        var cards = AchievementChallenges.For(log, "FN00");

        foreach (var card in cards)
        {
            _output.WriteLine(
                (card.HasRing ? card.Percent.PadLeft(4) : " ---")
                + "  " + (card.HasRing ? "solid " : "dashed")
                + "  " + card.Title.PadRight(34)
                + "  in the ring: " + card.RingWord);

            _output.WriteLine("        means: " + card.RingMeans);
            _output.WriteLine("");
        }

        Assert.All(
            cards,
            c => Assert.True(
                c.HasRingMeans,
                c.Title + " draws a ring and says nothing about what it measures"));
    }

    /// <summary>**No ring is fabricated where there is nothing linear to measure.**</summary>
    /// <remarks>
    /// **A BAND IS WORKED OR IT IS NOT.** There is no half of it to be in, so any
    /// fraction on that card would have to come from a denominator chosen to make a
    /// number appear, which is a figure with nothing behind it.
    /// </remarks>
    [Fact]
    public void NoRingIsFabricated()
    {
        var cards = AchievementChallenges.For(Log(), "FN00");

        var open = cards.Where(c => c.RingIsOpen).ToList();

        _output.WriteLine("dashed, with the target inside and no percentage:");

        foreach (var card in open)
        {
            _output.WriteLine("  " + card.Title.PadRight(38) + card.Target);
        }

        Assert.Contains(open, c => c.Key == "challenge-new-band");
        Assert.Contains(open, c => c.Key == "challenge-new-continent");

        // **AN OPEN RING CARRIES NO NUMBER AT ALL**, which is the difference between
        // *there is nothing to measure* and *you are at nought per cent*.
        Assert.All(open, c => Assert.Equal("", c.Percent));
        Assert.All(open, c => Assert.True(c.RingWord.Length > 0, c.Title));

        // **AND NEITHER DOES A FINISHED ONE.** A target that is met is not
        // ninety-nine per cent of anything; it is done, and the word says so.
        Assert.All(
            cards.Where(c => c.Earned),
            c => Assert.Equal("done", c.RingWord));

        // And a measured one is a real quantity with both its halves said in words.
        var distance = cards.Single(c => c.Key == "challenge-distance");

        _output.WriteLine("");
        _output.WriteLine("measured: " + distance.Title
            + "  ring " + distance.Percent);
        _output.WriteLine("  " + distance.RingMeans);

        Assert.True(distance.HasRing);
        Assert.Contains("to go", distance.RingMeans, StringComparison.Ordinal);
        Assert.Contains(
            "not how likely", distance.RingMeans, StringComparison.Ordinal);
    }

    /// <summary>**An unmeasured quantity is not drawn as a zero.**</summary>
    /// <remarks>
    /// **NO GRID SQUARE IN THE LOG IS NOT THE SAME AS NO MILES.** One is a thing
    /// nobody measured; the other is a measurement. Drawing 0% for the first claims a
    /// reading that was never taken.
    /// </remarks>
    [Fact]
    public void NothingMeasuredIsNotDrawnAsNought()
    {
        var bare = new AchievementLog(
            new[] { Contact("W1ABC", "20m", "2026-09-10 02:00:00", grid: null) },
            "FN00");

        var distance = AchievementChallenges.For(bare, "FN00")
            .Single(c => c.Key == "challenge-distance");

        _output.WriteLine("with no grid square anywhere in the log:");
        _output.WriteLine("  ring   : " + (distance.HasRing ? "solid" : "dashed"));
        _output.WriteLine("  percent: " + (distance.Percent.Length == 0
            ? "(none)" : distance.Percent));
        _output.WriteLine("  means  : " + distance.RingMeans);

        Assert.True(
            distance.RingIsOpen,
            "a log with no grid square drew a distance percentage anyway");

        Assert.Equal("", distance.Percent);
    }

    /// <summary>The screen from the fixture log.</summary>
    private static AchievementScreen Screen()
    {
        var log = Log();

        return new AchievementScreen(log, AchievementChallenges.For(log, "FN00"));
    }

    /// <summary>
    /// A log with a reach, a country, two bands and a report, so every kind of card
    /// on the screen has something to be about.
    /// </summary>
    private static AchievementLog Log()
        => new(
            new[]
            {
                Contact("W3YNI", "20m", "2026-09-10 02:00:00", grid: "FN20"),
                Contact("W1ABC", "20m", "2026-09-10 02:15:00", grid: "FN42"),
                Contact("K4XYZ", "40m", "2026-09-10 03:00:00", grid: "EM73"),
                Contact(
                    "VP2MAA", "20m", "2026-09-11 01:30:00",
                    grid: "FK86", received: -19),
            },
            "FN00");

    /// <summary>One sound record.</summary>
    private static AdifLogRecord Contact(
        string call, string band, string startedUtc, string? grid,
        int received = -7)
        => new(
            new AdifContact
            {
                Call = call,
                StationCallsign = "KC3QIS",
                Band = band,
                Mode = "FT8",
                GridSquare = grid,
                MyGridSquare = "FN00",
                ReportSent = "-12",
                ReportReceived = received.ToString(CultureInfo.InvariantCulture),
                StartedUtc = DateTime.Parse(
                    startedUtc, CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal),
            },
            Array.Empty<string>(),
            Terminated: true);
}
