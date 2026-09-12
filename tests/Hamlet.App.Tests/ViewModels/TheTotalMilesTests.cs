using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 331 task 7, the badge half: **Total Miles on the page.**
/// </summary>
/// <remarks>
/// <para>The sum itself, the missing-grid rule and the tier arithmetic are the engine's
/// and are asserted in the engine project's class of the same name. **This is the half a
/// reader sees**: what the badge says on an empty log, what it says afterwards, and which
/// card it holds up as next.</para>
/// <para>**`0 mi so far` AND NOT `0 of 50,000`** (§3.7). The badge never counts what he has
/// not done; the next card is the target, and the corner is what he has.</para>
/// </remarks>
public sealed class TheTotalMilesTests
{
    private const string MyGrid = "FN00";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the badge is printed.</param>
    public TheTotalMilesTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **Assertion 1: the badge says `0 mi so far` on an empty log, and the lowest tier is
    /// its next card.**
    /// </summary>
    [Fact]
    public void TheBadgeSaysZeroMilesOnAnEmptyLogAndTheLowestTierIsNext()
    {
        var badge = Badge(Array.Empty<AdifLogRecord>());

        _output.WriteLine(
            badge.Name + " | " + badge.Meaning + " | next " + badge.NextCard
            + " | " + badge.Standing + " | " + badge.ScoreLine);

        Assert.Equal("Total Miles", badge.Name);
        Assert.Equal("grid to grid, added up", badge.Meaning);
        Assert.Equal("globe", badge.Emblem);
        Assert.Equal("0 mi so far", badge.Standing);
        Assert.Equal("50,000 miles", badge.NextCard);

        // **AND NOT A DENOMINATOR ANYWHERE.** `0 of 50,000` would be a count of what he
        // has not done, on night one, in the corner of a badge.
        Assert.DoesNotContain(" of ", badge.Standing, StringComparison.Ordinal);
    }

    /// <summary>
    /// **Assertion 2: the running sum is in the corner, and the next tier is the lowest
    /// unearned one.**
    /// </summary>
    [Fact]
    public void TheRunningSumIsInTheCornerAndTheNextTierIsTheLowestUnearned()
    {
        var badge = Badge(FiveWithGrids());

        _output.WriteLine("standing : " + badge.Standing);
        _output.WriteLine("next     : " + badge.NextCard);
        _output.WriteLine("score    : " + badge.ScoreLine);

        // 27,293 miles over five legs, which the engine class computes independently.
        Assert.EndsWith(" mi so far", badge.Standing, StringComparison.Ordinal);
        Assert.StartsWith("27,", badge.Standing, StringComparison.Ordinal);

        // **STILL THE FIRST TIER**, because 27,293 has not reached 50,000.
        Assert.Equal("50,000 miles", badge.NextCard);
        Assert.Equal(0, badge.Score.Points);

        // **AND PAST IT, THE NEXT ONE UP.** A log with the same five legs four times over
        // is past 100,000 and short of 250,000.
        var far = Badge(
            Enumerable.Range(0, 4).SelectMany(_ => FiveWithGrids()).ToList());

        _output.WriteLine("");
        _output.WriteLine("four times over : " + far.Standing);
        _output.WriteLine("next            : " + far.NextCard);
        _output.WriteLine("score           : " + far.ScoreLine);

        Assert.Equal("250,000 miles", far.NextCard);

        // **THE TWO TIERS PASSED, EACH ONCE**: 50,000 at 5 and 100,000 at 10 is 15.
        Assert.Equal(15, far.Score.Points);
    }

    /// <summary>
    /// **Assertion 3: earning a tier is an achievement like any other - it moves the
    /// total.**
    /// </summary>
    [Fact]
    public void EarningATierMovesTheRunningTotal()
    {
        var near = Page(FiveWithGrids());
        var far = Page(Enumerable.Range(0, 4).SelectMany(_ => FiveWithGrids()).ToList());

        _output.WriteLine(
            "at " + near.Scores.TotalMiles.ToString("#,0", CultureInfo.InvariantCulture)
            + " mi : " + near.TotalLine);
        _output.WriteLine(
            "at " + far.Scores.TotalMiles.ToString("#,0", CultureInfo.InvariantCulture)
            + " mi : " + far.TotalLine);

        Assert.NotNull(near.Scores.Total);
        Assert.NotNull(far.Scores.Total);

        // **THE ONLY DIFFERENCE BETWEEN THE TWO LOGS IS THE MILES.** The same five
        // stations, the same entities, grids, bands and modes - so the whole change in the
        // total is the two tiers the sum passed.
        Assert.Equal(
            near.Scores.For(AchievementKinds.Countries).Points,
            far.Scores.For(AchievementKinds.Countries).Points);

        Assert.Equal(15, far.Scores.Total - near.Scores.Total);
    }

    private static AchievementBadge Badge(IReadOnlyList<AdifLogRecord> records)
        => Page(records).Badges.Single(b => b.Kind == AchievementKinds.TotalMiles);

    private static AchievementBadgePage Page(IReadOnlyList<AdifLogRecord> records)
        => new(
            new AchievementLog(records, MyGrid),
            AchievementPoints.Parse(AchievementPoints.Shipped()));

    /// <summary>The engine class's own five squares, so the two agree on the fixture.</summary>
    private static IReadOnlyList<AdifLogRecord> FiveWithGrids()
        => new[] { "JO59", "PM95", "KG44", "GG66", "IO91" }
            .Select((grid, i) => Record("TEST" + i, grid))
            .ToList();

    private static AdifLogRecord Record(string call, string? grid)
        => new(
            new AdifContact
            {
                Call = call,
                StationCallsign = "KC3QIS",
                Mode = "FT8",
                Band = "20m",
                GridSquare = grid,
                MyGridSquare = MyGrid,
                StartedUtc = new DateTime(2026, 8, 14, 21, 41, 30, DateTimeKind.Utc),
                EndedUtc = new DateTime(2026, 8, 14, 21, 43, 30, DateTimeKind.Utc),
            },
            Array.Empty<string>(),
            true);
}
