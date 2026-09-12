using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Contacts;

/// <summary>
/// Work instruction 331 task 7: **Total Miles.**
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-12**: a new kind, *grid to grid, added up*, with tiers from the
/// points file - *"10 million is hard"*.</para>
/// <para>**EVERY LOGGED CONTACT WITH A GRID ON BOTH ENDS CONTRIBUTES ITS GREAT CIRCLE, AND
/// A CONTACT WITHOUT ONE CONTRIBUTES NOTHING** (§0.0, and the instruction says it twice:
/// *never an estimate from the country*). A country is a large thing and its middle is not
/// where the station was; a mileage totted up from one would be a figure with nothing
/// behind it, and this sum is the one number on the page somebody might quote.</para>
/// <para>**THE HAND COMPUTATION IS AN INDEPENDENT ONE.** Comparing `GridPath`'s sum with
/// `GridPath`'s legs would prove only that addition works. So this class decodes the
/// Maidenhead squares itself and runs its own haversine over its own earth radius, and
/// asserts the two agree **within 1%** - which is a real check on the path Hamlet draws
/// on the card, and which would have caught a wrong hemisphere, a swapped pair or a
/// kilometre.</para>
/// </remarks>
public sealed class TheTotalMilesTests
{
    /// <summary>The operator's own square, near Lancaster, Pennsylvania.</summary>
    private const string MyGrid = "FN00";

    /// <summary>The mean earth radius in miles, for this class's own haversine.</summary>
    private const double EarthMiles = 3958.7613;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the legs and the sum are printed.</param>
    public TheTotalMilesTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **Assertion 1: five contacts with grids sum to the hand-computed miles within 1%.**
    /// </summary>
    [Fact]
    public void FiveContactsWithGridsSumToTheHandComputedMiles()
    {
        var squares = new[] { "JO59", "PM95", "KG44", "GG66", "IO91" };

        var log = new AchievementLog(
            squares.Select((grid, i) => Record("TEST" + i, grid)).ToList(), MyGrid);

        var byHand = 0.0;

        foreach (var square in squares)
        {
            var leg = Haversine(MyGrid, square);

            byHand += leg;

            _output.WriteLine(
                MyGrid + " to " + square + " : "
                + leg.ToString("#,0", CultureInfo.InvariantCulture) + " mi by hand");
        }

        var hamlet = AchievementScores.MilesIn(log);

        _output.WriteLine("");
        _output.WriteLine(
            "by hand : " + byHand.ToString("#,0", CultureInfo.InvariantCulture) + " mi");
        _output.WriteLine(
            "Hamlet  : " + hamlet.ToString("#,0", CultureInfo.InvariantCulture) + " mi");
        _output.WriteLine(
            "apart by "
            + (100 * Math.Abs(hamlet - byHand) / byHand).ToString(
                "0.00", CultureInfo.InvariantCulture) + "%");

        Assert.Equal(5, log.Count);

        Assert.True(
            Math.Abs(hamlet - byHand) <= byHand * 0.01,
            "Hamlet says " + hamlet.ToString("#,0", CultureInfo.InvariantCulture)
            + " mi and the hand computation says "
            + byHand.ToString("#,0", CultureInfo.InvariantCulture)
            + " mi, which is more than 1% apart");
    }

    /// <summary>
    /// **Assertion 2: a contact without a grid adds zero, and never an estimate.**
    /// </summary>
    [Fact]
    public void AContactWithoutAGridAddsZero()
    {
        var withOne = new AchievementLog(
            new[] { Record("K1ABC", "JO59") }, MyGrid);

        var andOneWithout = new AchievementLog(
            new[] { Record("K1ABC", "JO59"), Record("VE3PQR", null) }, MyGrid);

        var before = AchievementScores.MilesIn(withOne);
        var after = AchievementScores.MilesIn(andOneWithout);

        _output.WriteLine(
            "one contact with a grid   : "
            + before.ToString("#,0", CultureInfo.InvariantCulture) + " mi");
        _output.WriteLine(
            "and one without           : "
            + after.ToString("#,0", CultureInfo.InvariantCulture) + " mi");

        Assert.Equal(2, andOneWithout.Count);
        Assert.Equal(before, after);

        // **AND THE ONE WITHOUT HAS NO DISTANCE AT ALL**, rather than a distance of
        // nought - which is what stops a later reader averaging it in.
        Assert.Null(andOneWithout.Contacts[1].Miles);

        // **NOR DOES HAVING A COUNTRY HELP.** `VE3PQR` resolves to Canada and Canada has
        // a middle; the instruction forbids using it, and nothing here knows it.
        Assert.NotNull(andOneWithout.Contacts[1].Entity);

        // **AND WITH NO GRID OF HIS OWN, NOTHING HAS A DISTANCE.** A great circle needs
        // two ends.
        var nowhere = new AchievementLog(
            new[] { Record("K1ABC", "JO59") }, operatorGrid: null);

        _output.WriteLine(
            "with no grid in Settings  : "
            + AchievementScores.MilesIn(nowhere).ToString(
                "#,0", CultureInfo.InvariantCulture) + " mi");

        Assert.Equal(0, AchievementScores.MilesIn(nowhere));
    }

    /// <summary>
    /// **Assertion 3: crossing 50,000 earns the tier once and never again.**
    /// </summary>
    /// <remarks>
    /// **A TIER IS A THRESHOLD AND NOT A COUNTER** (the instruction: *earning a tier earns
    /// it once*). The score is the sum of the tiers passed, so working the same tier's
    /// worth of miles twice cannot pay twice - and the check that matters is the one where
    /// the sum goes well past a tier: the points for it do not multiply.
    /// </remarks>
    [Fact]
    public void CrossingFiftyThousandEarnsTheTierOnceAndNeverAgain()
    {
        var points = AchievementPoints.Parse(AchievementPoints.Shipped());

        var tiers = points.Milestones(AchievementKinds.TotalMiles);

        _output.WriteLine(
            "the tiers: " + string.Join(
                ", ",
                tiers.Select(
                    t => t.At.ToString("#,0", CultureInfo.InvariantCulture) + " -> "
                        + t.Points)));

        Assert.NotEmpty(tiers);

        var first = tiers[0];

        Assert.Equal(50_000, first.At);

        // **JUST UNDER: NOTHING.** The tier is not paid early.
        var under = Scored(points, first.At - 1);

        // **JUST OVER: EXACTLY ONE TIER'S WORTH.**
        var over = Scored(points, first.At);

        // **AND FAR OVER: STILL THAT TIER ONCE, PLUS WHATEVER OTHER TIERS WERE PASSED.**
        var wayOver = Scored(points, (first.At * 2) - 1);

        _output.WriteLine("");
        _output.WriteLine("at " + (first.At - 1) + " mi : " + under + " pts");
        _output.WriteLine("at " + first.At + " mi : " + over + " pts");
        _output.WriteLine("at " + ((first.At * 2) - 1) + " mi : " + wayOver + " pts");

        Assert.Equal(0, under);
        Assert.Equal(first.Points, over);

        // Twice the first tier less a mile has passed the first tier and, on the shipped
        // file, the second one at 100,000 is not reached - so it is still the first tier
        // alone and not two of it.
        var passed = tiers.Where(t => (first.At * 2) - 1 >= t.At).ToList();

        Assert.Equal(passed.Sum(t => t.Points), wayOver);
        Assert.Equal(passed.Count, passed.Select(t => t.At).Distinct().Count());
    }

    /// <summary>
    /// **Assertion 4: an empty log is 0 miles and no tier, and the sum only grows.**
    /// </summary>
    [Fact]
    public void AnEmptyLogIsNoMilesAndTheSumOnlyGrows()
    {
        var points = AchievementPoints.Parse(AchievementPoints.Shipped());

        var empty = new AchievementScores(
            new AchievementLog(Array.Empty<AdifLogRecord>(), MyGrid), points);

        _output.WriteLine("empty log : " + empty.TotalMiles + " mi");

        Assert.Equal(0, empty.TotalMiles);
        Assert.Equal(0, empty.For(AchievementKinds.TotalMiles).Points);

        // **ADDING A CONTACT NEVER TAKES MILES AWAY**, which is the *a rank never goes
        // down* rule where it can actually be checked: the sum is over a log that is only
        // ever appended to.
        var running = 0.0;
        var records = new List<AdifLogRecord>();

        foreach (var square in new[] { "JO59", "PM95", "KG44" })
        {
            records.Add(Record("TEST" + records.Count, square));

            var now = AchievementScores.MilesIn(new AchievementLog(records, MyGrid));

            _output.WriteLine(
                "after " + records.Count + " : "
                + now.ToString("#,0", CultureInfo.InvariantCulture) + " mi");

            Assert.True(now >= running, "the sum went down");

            running = now;
        }
    }

    private static int Scored(AchievementPoints points, long miles)
        => points.Milestones(AchievementKinds.TotalMiles)
            .Where(t => miles >= t.At)
            .Sum(t => t.Points);

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

    /// <summary>
    /// **This class's own great circle, so the comparison is against something else.**
    /// </summary>
    /// <remarks>
    /// A four-character Maidenhead square decoded here, and a haversine over a mean earth
    /// radius. It is not `GridPath` and it is not meant to be: two implementations
    /// agreeing to 1% is evidence, and one implementation agreeing with itself is not.
    /// </remarks>
    private static double Haversine(string from, string to)
    {
        var (lat1, lon1) = Center(from);
        var (lat2, lon2) = Center(to);

        var p1 = lat1 * Math.PI / 180;
        var p2 = lat2 * Math.PI / 180;
        var dp = (lat2 - lat1) * Math.PI / 180;
        var dl = (lon2 - lon1) * Math.PI / 180;

        var a = (Math.Sin(dp / 2) * Math.Sin(dp / 2))
            + (Math.Cos(p1) * Math.Cos(p2) * Math.Sin(dl / 2) * Math.Sin(dl / 2));

        return EarthMiles * 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
    }

    /// <summary>The middle of a four-character square, in degrees.</summary>
    private static (double Lat, double Lon) Center(string square)
    {
        var upper = square.ToUpperInvariant();

        var lon = ((upper[0] - 'A') * 20.0) - 180.0 + ((upper[2] - '0') * 2.0) + 1.0;
        var lat = ((upper[1] - 'A') * 10.0) - 90.0 + (upper[3] - '0') + 0.5;

        return (lat, lon);
    }
}
