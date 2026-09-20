using System;
using System.Collections.Generic;
using System.Linq;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 368 task 4, criterion 5.2: **before his first Olivia contact nothing on the
/// achievements screen claims he has worked Olivia; after it the mode's records appear and the
/// Modes badge counts it.**
/// </summary>
/// <remarks>
/// <para>**AN ABSENCE AND THEN A PRESENCE, BOTH ASSERTED** (§3.1, decision BZ). A screen that
/// named a mode's achievement before he had worked it would be making him a promise; a screen
/// that stayed silent after he had worked it would be losing the evening. Both halves are here
/// because either one alone is easy to pass.</para>
/// <para>**AN UNWORKED ROW IS AN INVITATION AND NOT A CLAIM** (decision BZ). Olivia is allowed
/// among the modes he has not worked before his first contact, exactly as every unworked mode
/// is; what is forbidden before it is an *earned* card, an *earned* first, and a count that
/// says he has worked it.</para>
/// <para>**NO NUMBER OF MODES IS TYPED IN THIS CLASS OR IN THE CHANGE IT GUARDS** (decision BY).
/// The count comes from `AchievementScores.WorkableModes`, which counts the table, and the
/// badge's words are composed from that count.</para>
/// <para>**COMPUTED, NOT SEEN** (FACT-004): the Olivia record here is composed on the
/// development machine and is not a contact anybody made.</para>
/// </remarks>
public sealed class TheOliviaRecordsAppearTests
{
    /// <summary>The operator's own square, for the distances the page draws.</summary>
    private const string MyGrid = "FN00DJ";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the before and after are printed.</param>
    public TheOliviaRecordsAppearTests(ITestOutputHelper output) => _output = output;

    /// <summary>Hamlet's own name for the mode, from the one place it is written.</summary>
    private static string Olivia => ContactModes.OliviaName;

    /// <summary>
    /// **1, before: with no Olivia record, no earned card, no first and no count names Olivia.**
    /// </summary>
    [Fact]
    public void BeforeTheFirstOliviaContactNothingEarnedNamesOlivia()
    {
        var page = Page(WithoutOlivia());
        var earned = AchievementScores.FirstsEarned(page.Log);
        var modes = AchievementCategory.For(AchievementKinds.Modes, page)!;

        _output.WriteLine("modes worked : " + string.Join(", ", page.Log.Modes));
        _output.WriteLine("firsts earned: " + string.Join(", ", earned));

        foreach (var card in modes.Cards)
        {
            _output.WriteLine($"card \"{card.Title}\" earned {card.Earned}");
        }

        // **NOT IN THE LOG'S MODES**, which is what every count below is read from.
        Assert.DoesNotContain(Olivia, page.Log.Modes, StringComparer.OrdinalIgnoreCase);

        // **NO EARNED CARD NAMES IT.**
        Assert.DoesNotContain(
            modes.Cards.Where(c => c.Earned),
            c => c.Title.Contains(Olivia, StringComparison.OrdinalIgnoreCase));

        // **NO FIRST IS EARNED FOR IT, AND THE ONE THE PAGE OFFERS NEXT IS NOT IT** (§3.1).
        Assert.DoesNotContain("first_olivia", earned);
        Assert.NotEqual("first_olivia", AchievementBadgePage.NextFirstOf(earned)?.Key);

        // **AND NO BADGE'S COUNT CLAIMS IT.** The Modes badge counts the modes the log holds,
        // and this log holds no Olivia record.
        var badge = page.Badges.Single(b => b.Name == "Modes");

        _output.WriteLine($"the Modes badge: \"{badge.Meaning}\" · {badge.Standing} · next \"{badge.NextCard}\"");

        Assert.Equal(Worked(page.Log.Modes.Count), badge.Standing);
        Assert.DoesNotContain(Olivia, badge.Standing, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Olivia, badge.NextCard, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// **2, and it is what decision BZ allows: an unworked Olivia row before the first contact
    /// is an invitation.**
    /// </summary>
    [Fact]
    public void AnUnworkedOliviaRowIsAllowedBeforeTheFirstContact()
    {
        // **EVERY OTHER MODE WORKED**, so the one mode left to work is the one this phase
        // built and the card has room to draw it: a next card lists three callers, and on a
        // log that has worked nothing Olivia is the fourth in the table's order.
        var page = Page(EverythingButOlivia());
        var modes = AchievementCategory.For(AchievementKinds.Modes, page)!;
        var next = modes.Cards.Where(c => !c.Earned).ToList();

        foreach (var card in next)
        {
            foreach (var caller in card.Callers)
            {
                _output.WriteLine($"unworked row: {caller.Place} [{caller.CallLine}]");
            }
        }

        var rows = next.SelectMany(c => c.Callers).ToList();

        Assert.Contains(rows, r => string.Equals(r.Place, Olivia, StringComparison.OrdinalIgnoreCase));

        // **AND IT SAYS WHERE THE MODE LIVES AND WHAT THE LIST CAN SEE OF IT** (rulings 25 and
        // 26), neither of them empty and neither of them a claim that he worked it.
        var row = rows.Single(r => string.Equals(r.Place, Olivia, StringComparison.OrdinalIgnoreCase));

        Assert.NotEqual("", row.CallLine.Trim());
        Assert.Contains(AchievementCategory.ListCannotTellOlivia, row.CallLine, StringComparison.Ordinal);

        // **AND IT IS NOT AN EARNED CARD**, which is the line decision BZ draws: the row is on
        // the *A mode you have not worked* card and no card on this page is an Olivia card he
        // has earned.
        Assert.DoesNotContain(
            modes.Cards.Where(c => c.Earned),
            c => c.Title.Contains(Olivia, StringComparison.OrdinalIgnoreCase));

        Assert.DoesNotContain("first_olivia", AchievementScores.FirstsEarned(page.Log));
    }

    /// <summary>
    /// **3, after: one Olivia record earns the card, earns the first, and moves the count on.**
    /// </summary>
    [Fact]
    public void OneOliviaRecordEarnsTheCardTheFirstAndOneMoreOnTheBadge()
    {
        var before = Page(WithoutOlivia());
        var after = Page(WithoutOlivia().Append(OliviaRecord()).ToList());

        var wasBadge = before.Badges.Single(b => b.Name == "Modes");
        var isBadge = after.Badges.Single(b => b.Name == "Modes");

        _output.WriteLine($"before: {wasBadge.Meaning} · {wasBadge.Standing}");
        _output.WriteLine($"after : {isBadge.Meaning} · {isBadge.Standing}");

        // **THE LOG NOW HOLDS THE MODE**, read off the record's own pair and nothing else.
        Assert.Contains(Olivia, after.Log.Modes, StringComparer.OrdinalIgnoreCase);

        // **THE MODES CARD FOR OLIVIA IS EARNED.**
        var modes = AchievementCategory.For(AchievementKinds.Modes, after)!;
        var card = modes.Cards.Single(
            c => c.Title.Contains(Olivia, StringComparison.OrdinalIgnoreCase));

        _output.WriteLine($"the Olivia card: \"{card.Title}\" earned {card.Earned}, {card.PointsLine}");

        Assert.True(card.Earned, "the Olivia Modes card is not earned after an Olivia contact");

        // **THE FIRST IS EARNED, AND IT READS THE WORDS DECISION CA SET.**
        var earned = AchievementScores.FirstsEarned(after.Log);

        Assert.Contains("first_olivia", earned);
        Assert.Equal(
            "An Olivia contact",
            AchievementBadgePage.Firsts.Single(f => f.Key == "first_olivia").Said);

        // **AND THE BADGE'S WORKED COUNT RISES BY EXACTLY ONE**, against the same target.
        Assert.Equal(Worked(before.Log.Modes.Count), wasBadge.Standing);
        Assert.Equal(Worked(before.Log.Modes.Count + 1), isBadge.Standing);
        Assert.Equal(wasBadge.Meaning, isBadge.Meaning);
    }

    /// <summary>
    /// **4: the badge's target is counted and not typed, and Olivia is one of the modes there
    /// are to work.**
    /// </summary>
    /// <remarks>
    /// **THE WORDS FOLLOW THE COUNT** (decision BY). A badge whose words are a string and whose
    /// standing is a count is a badge that can say *five modes to work · 0 of 6*, which is the
    /// screen contradicting itself in one line (§0.0).
    /// </remarks>
    [Fact]
    public void TheTargetIsCountedFromTheTableAndTheWordsFollowIt()
    {
        var workable = ContactModes.Logged.Where(m => m.IsContactMode).ToList();

        _output.WriteLine("modes there are to work: " + string.Join(", ", workable.Select(m => m.Name)));

        Assert.Equal(workable.Count, AchievementScores.WorkableModes);
        Assert.Contains(workable, m => string.Equals(m.Name, Olivia, StringComparison.OrdinalIgnoreCase));

        // **WSPR IS STILL NOT ONE OF THEM.** It is a beacon and nobody works anybody on it.
        Assert.DoesNotContain(workable, m => m.Name == "WSPR");

        // **AND THE BADGE'S WORDS CARRY THAT COUNT**, composed rather than typed: the meaning
        // line and the standing's denominator are the same number from the same place.
        var badge = Page(WithoutOlivia()).Badges.Single(b => b.Name == "Modes");

        _output.WriteLine($"the badge says \"{badge.Meaning}\" · {badge.Standing}");

        Assert.Equal(AchievementBadgePage.ModesToWork, badge.Meaning);
        Assert.EndsWith(
            " of " + AchievementScores.WorkableModes.ToString(System.Globalization.CultureInfo.InvariantCulture),
            badge.Standing,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// **5: the other five modes are unharmed - a log with no Olivia record scores what its own
    /// records say and nothing about Olivia.**
    /// </summary>
    /// <remarks>
    /// **THE ARITHMETIC AND NOT A REMEMBERED NUMBER.** The modes score is read against the
    /// owner's own points file - one per mode worked, a special where he set one, and the *all*
    /// bonus only where every workable mode is in the log - so this holds whatever he puts in
    /// that file, and it fails if an Olivia record ever scores for a log that has none.
    /// </remarks>
    [Fact]
    public void AnFt8AndPsk31LogScoresExactlyWhatItsOwnRecordsSay()
    {
        var points = AchievementPoints.Parse(AchievementPoints.Shipped());
        var page = Page(WithoutOlivia(), points);
        var log = page.Log;

        Assert.Equal(new[] { "FT8", "PSK31" }, log.Modes.OrderBy(m => m, StringComparer.Ordinal));

        var per = points.Per(AchievementKinds.Modes) ?? 0;
        var expected = log.Modes.Sum(
            m => points.Special(AchievementKinds.Modes, m) ?? per);

        _output.WriteLine($"modes {string.Join(", ", log.Modes)} score {page.Scores.For(AchievementKinds.Modes).Points}, expected {expected}");

        Assert.Equal(expected, page.Scores.For(AchievementKinds.Modes).Points);

        // **AND THE *ALL MODES* BONUS IS NOT PAID**, because two of six is not all of them -
        // which is the clause decision BY moved and the reason it had to move: it was payable
        // at five of six before this unit.
        Assert.True(
            log.Modes.Count < AchievementScores.WorkableModes,
            "this fixture is meant to be short of the target");

        // **THE PSK31 FIRST IS EARNED AND THE OLIVIA ONE IS NOT.** Nothing about the other
        // keyboard mode moved with Olivia's arrival.
        var earned = AchievementScores.FirstsEarned(log);

        Assert.Contains("first_psk31", earned);
        Assert.DoesNotContain("first_olivia", earned);

        // **AND THE TOTAL IS STILL THE SUM OF THE EIGHT BADGES AND NOTHING ELSE.**
        Assert.Equal(page.Badges.Sum(b => b.Score.Points ?? 0), page.Scores.Total);
    }

    /// <summary>`2 of 6`, the way a badge writes its standing.</summary>
    private static string Worked(int count)
        => count.ToString(System.Globalization.CultureInfo.InvariantCulture)
           + " of "
           + AchievementScores.WorkableModes.ToString(System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>A log with an FT8 contact and a PSK31 one, and nothing on Olivia.</summary>
    private static List<AdifLogRecord> WithoutOlivia()
        =>
        [
            Record("IK4LZH", ContactModes.Named("FT8")!, "JN54", rst: null, decibels: "-12"),
            Record("G4XYZ", ContactModes.Named("PSK31")!, "IO91", rst: "599", decibels: null),
        ];

    /// <summary>A log holding every workable mode except Olivia.</summary>
    private static List<AdifLogRecord> EverythingButOlivia()
        => ContactModes.Logged
            .Where(m => m.IsContactMode
                && !string.Equals(m.Name, ContactModes.OliviaName, StringComparison.OrdinalIgnoreCase))
            .Select((m, i) => Record(
                "K" + i.ToString(System.Globalization.CultureInfo.InvariantCulture) + "ABC",
                m,
                "FN31",
                rst: "599",
                decibels: null))
            .ToList();

    /// <summary>One Olivia contact at 16/500, the pair as the export writes it.</summary>
    private static AdifLogRecord OliviaRecord()
        => Record("W1AW", ContactModes.Olivia("16/500"), "FN31", rst: "599", decibels: null);

    private static AdifLogRecord Record(
        string call, ContactMode mode, string grid, string? rst, string? decibels)
        => new(
            new AdifContact
            {
                Call = call,
                StationCallsign = "KC3QIS",
                Band = "20m",
                Mode = mode.AdifMode,
                Submode = mode.AdifSubmode,
                RstSent = rst,
                RstReceived = rst,
                ReportSent = decibels,
                ReportReceived = decibels,
                GridSquare = grid,
                MyGridSquare = MyGrid,
                StartedUtc = new DateTime(2026, 9, 19, 21, 0, 0, DateTimeKind.Utc),
                EndedUtc = new DateTime(2026, 9, 19, 21, 12, 0, DateTimeKind.Utc),
            },
            [],
            true);

    private static AchievementBadgePage Page(
        IReadOnlyList<AdifLogRecord> records, AchievementPoints? points = null)
        => new(
            new AchievementLog(records, MyGrid),
            points ?? AchievementPoints.Parse(AchievementPoints.Shipped()));
}
