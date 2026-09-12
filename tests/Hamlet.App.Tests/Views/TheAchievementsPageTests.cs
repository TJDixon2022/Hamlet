using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 331 task 6: **the achievements page is eight badges with scores.**
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-12**: *"I want to rework the achievements dialog. The opening
/// dialog page should be a list of badges indicating type of achievements. Nice badges,
/// real nice."* And, after discussion: **scores by difficulty** - *"we're moving away from
/// the achievements all count the same… getting all continents is hard… 10 million is
/// hard… the points don't matter"* - a **running total on the page**, and a new kind,
/// **Total Miles**.</para>
/// <para>**THE POINTS ARE HIS FILE AND NO VALUE IS IN CODE.** Every expected number below
/// is arithmetic over `data/achievements/achievement-points.json`, and the arithmetic is
/// written out beside each assertion so he can check it against the file he edits. **Change
/// the file and these numbers change** - which is the property being tested.</para>
/// <para>**COMPUTED, NOT SEEN.** The scores are read off the view model and the geometry
/// off a realized headless window.</para>
/// </remarks>
public sealed class TheAchievementsPageTests
{
    private const string MyGrid = "FN00";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the fixture's scores are printed.</param>
    public TheAchievementsPageTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **Assertion 1: eight badges render on an empty log, each with exactly one *next*
    /// card.**
    /// </summary>
    /// <remarks>
    /// **§3.1 BENT BY ONE RULING AND NO FURTHER** (ruling C, 2026-09-12). The nearest
    /// unearned card in each kind is visible on night one; **nothing beyond it**. So an
    /// empty log draws eight badges and eight cards, and not a wall of them.
    /// </remarks>
    [Fact]
    public void EightBadgesRenderOnAnEmptyLogEachWithOneNextCard()
    {
        var page = Page(Array.Empty<AdifLogRecord>());

        foreach (var badge in page.Badges)
        {
            _output.WriteLine(
                badge.Name.PadRight(14) + " | next: " + badge.NextCard.PadRight(34)
                + " | " + badge.Standing.PadRight(12) + " | " + badge.ScoreLine);
        }

        Assert.Equal(8, page.Badges.Count);

        Assert.Equal(
            new[]
            {
                "Hall of Fame", "Continents", "Countries", "States",
                "Grids", "Total Miles", "Bands", "Modes",
            },
            page.Badges.Select(b => b.Name).ToList());

        // **EXACTLY ONE NEXT CARD EACH**, and every one of them says something.
        foreach (var badge in page.Badges)
        {
            Assert.True(badge.HasNextCard, badge.Name + " has no next card");
            Assert.False(
                string.IsNullOrWhiteSpace(badge.NextCard),
                badge.Name + "'s next card is blank");
        }

        // **AND NOTHING COUNTS WHAT HE HAS NOT DONE** (§3.7). `0 worked`, never
        // `0 of 340`.
        Assert.Equal("0 worked", page.Badges.Single(b => b.Name == "Countries").Standing);
        Assert.Equal("0 mi so far", page.Badges.Single(b => b.Name == "Total Miles").Standing);

        // **AND NOWHERE DOES IT SAY *confirmed*** (ACHIEVEMENTS_PHILOSOPHY §4).
        var everything = string.Concat(
            page.Badges.Select(b => b.Name + b.Meaning + b.NextCard + b.Standing + b.ScoreLine))
            + page.TotalLine + page.Subtitle;

        Assert.DoesNotContain("confirm", everything, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// **Assertion 2: the fixture log's scores, kind by kind, with the arithmetic.**
    /// </summary>
    /// <remarks>
    /// <para>**TWELVE CONTACTS, WRITTEN OUT IN `TwelveContacts` BELOW.** Nine DXCC
    /// entities, six continents, ten grid squares, five bands and five modes, with one
    /// station worked twice and one contact carrying no grid at all - so a screen counting
    /// contacts rather than places, or reading a missing grid as a zero, goes red
    /// here.</para>
    /// <para>**THE ARITHMETIC IS AGAINST THE SHIPPED FILE** and is written beside each
    /// figure. The counts are printed as well as the points, because a wrong count and a
    /// wrong rate produce the same wrong total and only one of them is this unit's
    /// fault.</para>
    /// </remarks>
    [Fact]
    public void TheFixtureLogsScoresMatchTheHandComputedArithmetic()
    {
        var log = new AchievementLog(TwelveContacts(), MyGrid);
        var page = Page(TwelveContacts());

        _output.WriteLine("THE FIXTURE, AS THE LOG READS IT");
        _output.WriteLine("  entities   : " + string.Join(", ", log.Entities));
        _output.WriteLine("  continents : " + string.Join(", ", log.Continents));
        _output.WriteLine("  grids      : " + string.Join(", ", log.Grids));
        _output.WriteLine("  bands      : " + string.Join(", ", log.Bands));
        _output.WriteLine("  modes      : " + string.Join(", ", log.Modes));
        _output.WriteLine(
            "  miles      : "
            + page.Scores.TotalMiles.ToString("#,0", CultureInfo.InvariantCulture));
        _output.WriteLine(
            "  firsts     : " + string.Join(", ", AchievementScores.FirstsEarned(log)));
        _output.WriteLine("");

        _output.WriteLine("THE SCORES, KIND BY KIND");

        foreach (var badge in page.Badges)
        {
            _output.WriteLine(
                "  " + badge.Name.PadRight(14) + " worked "
                + badge.Score.Worked.ToString("#,0", CultureInfo.InvariantCulture).PadLeft(9)
                + "  " + (badge.Score.Points?.ToString(CultureInfo.InvariantCulture) ?? "absent")
                    .PadLeft(6) + " pts  " + badge.ScoreLine);
        }

        _output.WriteLine("");
        _output.WriteLine("  " + page.TotalLine);

        // **THE COUNTS FIRST**, because every figure below is a rate times one of these.
        //
        // **EIGHT ENTITIES AND NOT NINE, AND THAT IS THE CITED TABLE DECLINING.** The
        // fixture works twelve stations in what looks like nine countries; `VK2DEF`'s
        // prefix is one `DxccPrefixes` will not assign to a single entity, so it resolves
        // to null and neither the country nor its continent is counted. **That is the
        // table saying *I do not know* rather than guessing** (§0.0), and the fixture
        // keeps the callsign precisely so the behaviour is asserted rather than assumed.
        Assert.Equal(8, log.Entities.Count);
        Assert.Equal(5, log.Continents.Count);
        Assert.Equal(10, log.Grids.Count);
        Assert.Equal(5, log.Bands.Count);
        Assert.Equal(5, log.Modes.Count);

        // **CONTINENTS: the per-continent table, and no *all* bonus at five of seven.**
        // NA 5 + EU 15 + AS 50 + AF 75 + SA 25 = 170, and the 1000 for all seven is not
        // paid. Oceania is absent for the same reason Australia is.
        Assert.Equal(170, page.Scores.For(AchievementKinds.Continents).Points);

        // **COUNTRIES: 8 at 5 each is 40, and no milestone** - the lowest is at 10.
        Assert.Equal(40, page.Scores.For(AchievementKinds.Countries).Points);

        // **GRIDS: 10 at 1 each is 10, plus the 10-square milestone at 10 = 20.**
        Assert.Equal(20, page.Scores.For(AchievementKinds.Grids).Points);

        // **BANDS: five bands at 5 each is 25.** None of them is 160 m or 6 m, so no
        // special rate applies, and five of nine is not all of them.
        Assert.Equal(25, page.Scores.For(AchievementKinds.Bands).Points);

        // **MODES: four at 5 and CW at 15 is 35, plus the 50 for all five = 85.** The
        // *all* bonus is paid at five and not at six: WSPR is a beacon and nobody works
        // anybody on it.
        Assert.Equal(5, AchievementScores.WorkableModes);
        Assert.Equal(85, page.Scores.For(AchievementKinds.Modes).Points);

        // **STATES: nought, and it is nought rather than absent.** An ADIF record carries
        // `STATE` and `AchievementContact` does not read it yet, so the honest score is
        // nought with the badge still drawn - reported in output.md section 4.
        Assert.Equal(0, page.Scores.For(AchievementKinds.States).Points);

        // **TOTAL MILES: the tiers the sum has passed.** Printed above; asserted against
        // the tier table rather than against a mileage, because the mileage is
        // `GridPath`'s and this is a test about scoring.
        var miles = (long)Math.Round(page.Scores.TotalMiles);
        var tiers = page.Scores.Points.Milestones(AchievementKinds.TotalMiles)
            .Where(t => miles >= t.At)
            .ToList();

        _output.WriteLine("");
        _output.WriteLine(
            "  Total Miles: " + miles.ToString("#,0", CultureInfo.InvariantCulture)
            + " passes " + tiers.Count + " tier(s) worth "
            + tiers.Sum(t => t.Points) + " pts");

        Assert.Equal(
            tiers.Sum(t => t.Points), page.Scores.For(AchievementKinds.TotalMiles).Points);

        // **AND AT 42,041 MILES IT IS NOUGHT, BECAUSE THE LOWEST TIER IS 50,000.** The
        // badge says `42,041 mi so far` and its next card is `50,000 miles`: a sum with no
        // tier behind it yet is nought points and a real number of miles, which is the
        // difference between *he has not got there* and *nobody counted*.
        Assert.Equal(0, page.Scores.For(AchievementKinds.TotalMiles).Points);
        Assert.Equal("50,000 miles",
            page.Badges.Single(b => b.Name == "Total Miles").NextCard);

        // **HALL OF FAME: the firsts this log can prove.** first_contact 10 + first_dx 25
        // + first_psk31 15 + first_cw_qso 50 + first_over_5000_miles 25 = 125. **The
        // 10,000-mile first is NOT paid**: the furthest contact in the fixture is under
        // it, and a first awarded on a distance nobody reached would be the screen
        // inventing a contact.
        Assert.Equal(125, page.Scores.For(AchievementKinds.HallOfFame).Points);
        Assert.DoesNotContain(
            "first_over_10000_miles", AchievementScores.FirstsEarned(log));

        // **THE TOTAL IS THE SUM OF THE EIGHT AND NOTHING ELSE.**
        // 170 + 40 + 20 + 25 + 85 + 0 + 0 + 125 = 465.
        Assert.Equal(465, page.Scores.Total);
        Assert.Equal(
            page.Badges.Sum(b => b.Score.Points ?? 0), page.Scores.Total);

        // **AND THE RANK IS THE THRESHOLDS PASSED, PLUS ONE.** The file's ranks are
        // 25, 100, 250, 500, 1000, 2500, 5000, 10000; 465 has passed three of them, so he
        // is Rank 4 with 35 to Rank 5.
        Assert.Equal(4, page.Scores.Rank);
        Assert.Equal(35, page.Scores.ToNextRank);
        Assert.Equal("Total 465 pts · Rank 4 · 35 to Rank 5", page.TotalLine);

        // **AND THE LINE UNDER THE TITLE SAYS IT, WITH THE RANK AND THE GAP.**
        Assert.True(page.HasTotal);
        Assert.StartsWith("Total ", page.TotalLine, StringComparison.Ordinal);
        Assert.Contains(
            "Rank " + page.Scores.Rank.ToString(CultureInfo.InvariantCulture),
            page.TotalLine,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// **Assertion 3: a missing points file yields absent scores and the sentence.**
    /// </summary>
    /// <remarks>
    /// **ABSENT IS NOT NOUGHT** (§0.0). A page showing every kind at nought says he has
    /// earned nothing; a page showing them absent says nobody can tell what he has earned,
    /// and only one of those is true when a file cannot be read.
    /// </remarks>
    [Fact]
    public void AMissingPointsFileYieldsAbsentScoresAndTheSentence()
    {
        var where = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "hamlet-unit331-no-such-file-" + Guid.NewGuid().ToString("N") + ".json");

        var points = AchievementPoints.Read(where);

        var page = new AchievementBadgePage(
            new AchievementLog(TwelveContacts(), MyGrid), points);

        _output.WriteLine("problem : " + page.Problem);
        _output.WriteLine("total   : [" + page.TotalLine + "]");

        foreach (var badge in page.Badges)
        {
            _output.WriteLine(
                "  " + badge.Name.PadRight(14) + " score [" + badge.ScoreLine + "]");
        }

        Assert.False(points.Loaded);
        Assert.True(page.HasProblem);
        Assert.Contains("could not be read", page.Problem, StringComparison.Ordinal);

        // **NO TOTAL, AND NO SCORE ANYWHERE.**
        Assert.False(page.HasTotal);
        Assert.Empty(page.TotalLine);
        Assert.Null(page.Scores.Total);

        foreach (var badge in page.Badges)
        {
            Assert.False(badge.HasScore, badge.Name + " shows a score with no file");
            Assert.Null(badge.Score.Points);
        }

        // **AND THE EIGHT BADGES ARE STILL THERE, WITH THEIR COUNTS.** What the file
        // governs is the points; what he has worked is the log's and does not need one.
        Assert.Equal(8, page.Badges.Count);
        Assert.Equal(8, page.Scores.For(AchievementKinds.Countries).Worked);
    }

    /// <summary>
    /// **Assertion 4: a malformed points file is reported and never overwritten.**
    /// </summary>
    [Fact]
    public void AMalformedPointsFileIsReportedAndLeftAlone()
    {
        var where = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "hamlet-unit331-broken-" + Guid.NewGuid().ToString("N") + ".json");

        const string HisBrokenEdit = "{ \"countries\": { \"per\": 5, }";

        System.IO.File.WriteAllText(where, HisBrokenEdit);

        try
        {
            var points = AchievementPoints.ReadOrSeed(where);

            _output.WriteLine("problem : " + points.Problem);

            Assert.False(points.Loaded);
            Assert.Contains("could not be read", points.Problem, StringComparison.Ordinal);

            // **HIS EDIT IS HIS, EVEN WHEN IT IS BROKEN.** Seeding over a file he was in
            // the middle of would throw away work, and he is the only person who can tell
            // what he meant by it.
            Assert.Equal(HisBrokenEdit, System.IO.File.ReadAllText(where));
        }
        finally
        {
            System.IO.File.Delete(where);
        }
    }

    /// <summary>
    /// **Assertion 5: the eight badges are on the realized window, with their emblems and
    /// nothing clipped.**
    /// </summary>
    [AvaloniaFact]
    public void TheEightBadgesAreOnTheWindowAndNothingIsClipped()
    {
        var window = Realized(TwelveContacts());

        try
        {
            var badges = window.GetVisualDescendants().OfType<ItemsControl>()
                .FirstOrDefault(c => c.Name == "AchievementsBadges");

            Assert.True(badges is not null, "there is no control named AchievementsBadges");

            Assert.Equal(8, badges!.ItemCount);

            var emblems = badges.GetVisualDescendants().OfType<BadgeEmblemControl>()
                .Where(e => e.IsVisible)
                .ToList();

            _output.WriteLine(
                "emblems drawn: " + string.Join(", ", emblems.Select(e => e.Emblem)));

            // **EIGHT EMBLEMS, EACH A DIFFERENT ONE.** Two badges wearing one drawing
            // would be a picture asserting they are the same kind of thing.
            Assert.Equal(8, emblems.Count);
            Assert.Equal(8, emblems.Select(e => e.Emblem).Distinct().Count());

            var total = window.GetVisualDescendants().OfType<TextBlock>()
                .First(t => t.Name == "AchievementsTotalLine");

            _output.WriteLine("total line: " + total.Text);

            Assert.True(total.IsVisible, "the running total is not drawn");
            Assert.False(string.IsNullOrWhiteSpace(total.Text));

            // **AND NOTHING IS CLIPPED** (the task's own *text sized to fit, never
            // clipped*). Every visible run in the badges gets at least what it asks for.
            foreach (var text in badges.GetVisualDescendants().OfType<TextBlock>()
                .Where(t => t.IsVisible && (t.Text ?? "").Trim().Length > 0))
            {
                Assert.True(
                    text.Bounds.Height + 0.51 >= text.DesiredSize.Height,
                    "[" + text.Text + "] is " + text.Bounds.Height.ToString(
                        "0.0", CultureInfo.InvariantCulture)
                    + " px tall and wants " + text.DesiredSize.Height.ToString(
                        "0.0", CultureInfo.InvariantCulture) + " px");
            }
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// **Assertion 6: no image asset ships, and the mockup is not one of them.**
    /// </summary>
    /// <remarks>
    /// <para>**THE MOCKUP PNG IS FOR READING, NOT FOR SHIPPING** (work instruction 331
    /// section 10). Every emblem is geometry in `BadgeEmblemControl`.</para>
    /// <para>**THE STRONG FORM OF THE CHECK IS THE ASSEMBLY AND NOT THE TEXT.** A first
    /// draft searched `src/` for the file's name and went red on this unit's own comments
    /// saying what the mockup is for - a comment naming a file is documentation and not a
    /// reference. What actually matters is whether the image can reach the screen, so this
    /// asks the built assembly what resources it carries, and separately asserts that no
    /// markup loads it through an `avares:` URI or a `Source=`.</para>
    /// </remarks>
    [Fact]
    public void NoImageAssetShipsAndTheMockupIsNotOne()
    {
        var app = typeof(AchievementsWindow).Assembly;

        var resources = app.GetManifestResourceNames();

        _output.WriteLine(
            "resources in Hamlet.App: " + string.Join(", ", resources));

        Assert.DoesNotContain(
            resources, name => name.Contains("mockup", StringComparison.OrdinalIgnoreCase));

        var markup = System.IO.File.ReadAllText(
            System.IO.Path.Combine(
                Root(), "src", "Hamlet.App", "Views", "AchievementsWindow.axaml"));

        // **NOTHING LOADS IT.** The name may appear in a comment saying what it was for;
        // it may not appear in anything that resolves to a picture.
        foreach (var how in new[] { "avares://", "Source=\"", "<Image" })
        {
            var at = 0;

            while ((at = markup.IndexOf(how, at, StringComparison.Ordinal)) >= 0)
            {
                var line = markup[at..Math.Min(markup.Length, at + 160)];

                _output.WriteLine("found " + how + " -> " + line.Split('\n')[0]);

                Assert.DoesNotContain(
                    "mockup", line.Split('\n')[0], StringComparison.OrdinalIgnoreCase);

                at += how.Length;
            }
        }
    }

    /// <summary>
    /// **Assertion 7: the shipped points file is the one in the tree, and it carries the
    /// `_about` line saying it is his.**
    /// </summary>
    [Fact]
    public void TheShippedFileIsTheOneInTheTreeAndSaysItIsHisToEdit()
    {
        var shipped = AchievementPoints.Shipped();
        var inTheTree = System.IO.File.ReadAllText(
            System.IO.Path.Combine(
                Root(), "data", "achievements", "achievement-points.json"));

        var points = AchievementPoints.Parse(shipped);

        _output.WriteLine("about: " + points.About);
        _output.WriteLine("kinds: " + points.Kinds);
        _output.WriteLine("hash : " + points.Hash);
        _output.WriteLine("ranks: " + string.Join(", ", points.Ranks));

        Assert.Equal(
            inTheTree.Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd(),
            shipped.Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd());

        Assert.True(points.Loaded);
        Assert.Equal(8, points.Kinds);
        Assert.Contains("Edit freely", points.About, StringComparison.Ordinal);
        Assert.NotEmpty(points.Ranks);
    }

    /// <summary>
    /// **Assertion 8: the two inherited reds in `TheAchievementsScreenTests` are not made
    /// worse.**
    /// </summary>
    /// <remarks>
    /// **THEY ARE ON THE KNOWN-RED LIST IN `docs/carry-forward-tests.txt`** -
    /// `WsprIsNotAFirstAnybodyCanEarnAndTheCardSaysSo` and `TheWindowDrawsEverySixRows` -
    /// and neither is this unit's. What this asserts is that the shapes they are about are
    /// still there: the six mode rows the old card built, and WSPR among them. **A unit
    /// that quietly deleted what a known red was about would turn two reds into two greens
    /// and lose the finding.**
    /// </remarks>
    [Fact]
    public void TheTwoInheritedRedsStillHaveSomethingToBeAbout()
    {
        var card = new AchievementsViewModel(TwelveContacts());

        _output.WriteLine("mode-first rows: " + card.Firsts.Count);

        foreach (var row in card.Firsts)
        {
            _output.WriteLine("  " + row.Name.PadRight(8) + " " + row.Standing);
        }

        Assert.Equal(6, card.Firsts.Count);
        Assert.Contains(card.Firsts, f => f.Name == "WSPR");
    }

    /// <summary>
    /// **Twelve contacts, written out.**
    /// </summary>
    /// <remarks>
    /// <para>Nine DXCC entities, six of the seven continents, ten distinct grid squares,
    /// five bands and five modes. **`W3YNI` is worked twice and on two bands**, so a screen
    /// counting contacts rather than places reads ten countries and goes red; **`VE3PQR`
    /// carries no grid at all**, so one that reads a missing grid as a zero mile
    /// contributes a distance it never measured.</para>
    /// <para>**THE ENTITIES AND CONTINENTS ARE THE CITED TABLES' AND NOT THIS FIXTURE'S.**
    /// The callsigns are chosen for their prefixes and the test prints what
    /// `DxccPrefixes` and `DxccContinents` made of them, so a disagreement shows as a
    /// count rather than as a mystery.</para>
    /// </remarks>
    public static IReadOnlyList<AdifLogRecord> TwelveContacts()
        => new[]
        {
            Record("W3YNI", "FT8", null, "20m", "FN20", Day(14)),
            Record("K2ABC", "FT8", null, "40m", "FN31", Day(15)),
            Record("VA3VRR", "CW", null, "40m", "FN03", Day(16)),
            Record("LA8ENA", "FT8", null, "20m", "JO59", Day(17)),
            Record("DL1ABC", "PSK", "PSK31", "20m", "JN48", Day(18)),
            Record("JA1XYZ", "FT8", null, "15m", "PM95", Day(19)),
            Record("VK2DEF", "FT8", null, "10m", "QF56", Day(20)),
            Record("ZS6GHI", "FT8", null, "20m", "KG44", Day(21)),
            Record("PY2JKL", "MFSK", "FT4", "20m", "GG66", Day(22)),
            Record("W3YNI", "FT8", null, "15m", "FN20", Day(23)),
            Record("G0MNO", "SSB", null, "20m", "IO91", Day(24)),
            Record("VE3PQR", "FT8", null, "80m", null, Day(25)),
        };

    private static DateTime Day(int of)
        => new(2026, 8, of, 21, 41, 30, DateTimeKind.Utc);

    private static AdifLogRecord Record(
        string call,
        string mode,
        string? submode,
        string band,
        string? grid,
        DateTime startedUtc)
        => new(
            new AdifContact
            {
                Call = call,
                StationCallsign = "KC3QIS",
                Mode = mode,
                Submode = submode,
                Band = band,
                GridSquare = grid,
                MyGridSquare = MyGrid,
                StartedUtc = startedUtc,
                EndedUtc = startedUtc.AddMinutes(2),
            },
            Array.Empty<string>(),
            true);

    /// <summary>The page, from the shipped points file.</summary>
    /// <remarks>
    /// **THE SHIPPED FILE AND NOT A FIXTURE OF ITS OWN.** The numbers being checked are
    /// the owner's, so a test with its own private table would prove that the arithmetic
    /// works on numbers nobody uses.
    /// </remarks>
    private static AchievementBadgePage Page(IReadOnlyList<AdifLogRecord> records)
        => new(
            new AchievementLog(records, MyGrid),
            AchievementPoints.Parse(AchievementPoints.Shipped()));

    private static Window Realized(IReadOnlyList<AdifLogRecord> records)
    {
        var window = new AchievementsWindow
        {
            DataContext = new AchievementsViewModel(
                records, MyGrid, AchievementPoints.Parse(AchievementPoints.Shipped())),
            Width = 900,
            Height = 900,
        };

        window.Show();

        for (var i = 0; i < 6; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }

        return window;
    }

    private static string Root()
    {
        var here = new System.IO.DirectoryInfo(AppContext.BaseDirectory);

        while (here is not null
            && !System.IO.File.Exists(
                System.IO.Path.Combine(here.FullName, "Hamlet.sln")))
        {
            here = here.Parent;
        }

        return here?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }
}
