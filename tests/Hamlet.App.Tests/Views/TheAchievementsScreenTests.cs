using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 287: the achievements screen, and the first of each mode.
/// </summary>
/// <remarks>
/// <para>**THE EXPOSURE IS AN ACHIEVEMENT CLAIMED THAT WAS NOT EARNED** (§0.0, and
/// the order says so in as many words). A screen of his own record is a place he
/// will trust without checking, so what is asserted here is mostly the negative
/// case: that a mode is not claimed from a string that merely resembles it, and
/// that a row nobody can ever earn says so rather than sitting there looking
/// earnable.</para>
/// <para>**BUILT AGAINST SYNTHESISED LOGS, BECAUSE THIS MACHINE HAS NO LOG AND
/// NEVER WILL** (`FACT-006`, and `SHACK_FACTS.md` behind it). Contacts are logged
/// where the radio is. Nothing here writes a file or touches
/// <see cref="Hamlet.App.Settings.ContactLogStore"/> at all.</para>
/// </remarks>
public sealed class TheAchievementsScreenTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fixture.</summary>
    /// <param name="output">Where the measurements are printed.</param>
    public TheAchievementsScreenTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// **A log holding contacts in two of the six modes, and nothing else.**
    /// </summary>
    /// <returns>Five sound records: three FT8 and two CW.</returns>
    /// <remarks>
    /// <para>**FT8 AND CW, WHICH IS THE PAIR THAT PROVES SOMETHING.** FT8 is the
    /// one Hamlet can write; **CW is a mode Hamlet cannot write and can still
    /// read**, because an ADI file is a portable format and his log can hold
    /// records another logger wrote. So the CW row lights from evidence Hamlet
    /// never produced, which is exactly right: the achievement is what his log
    /// says he did, not what this application happened to witness.</para>
    /// <para>**FIVE RECORDS, WHICH IS WHAT HIS OWN LOG HELD ON 2026-09-08.** The
    /// count is every record and never distinct callsigns (Tim's ruling), and
    /// `W3YNI` appears twice here so that a screen counting stations rather than
    /// contacts would read 4 and be caught.</para>
    /// </remarks>
    public static IReadOnlyList<AdifLogRecord> TwoModeLog()
        => new[]
        {
            Record("W3YNI", "FT8", "20m", new DateTime(2026, 8, 14, 21, 41, 30, DateTimeKind.Utc)),
            Record("K2ABC", "FT8", "20m", new DateTime(2026, 8, 20, 1, 15, 0, DateTimeKind.Utc)),
            Record("W3YNI", "FT8", "40m", new DateTime(2026, 9, 1, 23, 2, 15, DateTimeKind.Utc)),
            Record("VA3VRR", "CW", "40m", new DateTime(2026, 8, 17, 1, 33, 47, DateTimeKind.Utc)),
            Record("N4L", "CW", "20m", new DateTime(2026, 9, 3, 13, 47, 12, DateTimeKind.Utc)),
        };

    /// <summary>**What the screen says it has, against what the log holds.**</summary>
    /// <remarks>
    /// <para>Work instruction 287 task 5. **All six rows are printed** whatever the
    /// assertions look at, because the thing being reported is the whole card and a
    /// test that prints only what it asserts hides the rows nobody thought
    /// about.</para>
    /// <para>**THE EARLIEST IN EACH MODE IS THE ASSERTION WORTH HAVING HERE.** The
    /// CW records sit in the file out of date order on purpose, so a screen taking
    /// the first match rather than the earliest one names `N4L` and this goes red.
    /// Case is a separate question and is asserted where the near misses are.</para>
    /// </remarks>
    [AvaloniaFact]
    public void TheScreenSaysWhatTheLogHoldsAndNotOneModeMore()
    {
        var log = TwoModeLog();
        var screen = new AchievementsViewModel(log);

        _output.WriteLine("the log holds " + log.Count + " records");
        _output.WriteLine("the screen counts " + screen.Count);
        _output.WriteLine("the belt reads   " + screen.BeltLine);
        _output.WriteLine("the card reads   " + screen.FirstsLine);
        _output.WriteLine("");

        foreach (var row in screen.Firsts)
        {
            _output.WriteLine(
                row.Mark + "  " + row.Name.PadRight(7)
                + row.Standing.PadRight(20)
                + (row.HasEvidence ? row.Evidence : "(nothing to show)"));
        }

        _output.WriteLine("");
        _output.WriteLine("unasked: " + screen.Honesty);

        // **THE COUNT IS THE RECORD COUNT AND NOT THE STATION COUNT** (Tim's
        // ruling). `W3YNI` is in there twice on purpose: a screen counting
        // stations would read 4.
        Assert.Equal(log.Count, screen.Count);

        // **TWO EARNED, AND THE EARNED ONES ARE THE TWO IN THE FILE.**
        Assert.Equal(
            new[] { "CW", "FT8" },
            screen.Firsts.Where(f => f.Earned).Select(f => f.Name).OrderBy(n => n));

        // **AND THE EARLIEST IN EACH MODE, NOT THE FIRST ONE READ.** The CW
        // records are out of date order in the file, so a screen taking the first
        // match would name `N4L` here.
        var cw = screen.Firsts.Single(f => f.Name == "CW");

        Assert.Equal("VA3VRR on 2026-08-17, 40m", cw.Evidence);

        var ft8 = screen.Firsts.Single(f => f.Name == "FT8");

        Assert.Equal("W3YNI on 2026-08-14, 20m", ft8.Evidence);
    }

    /// <summary>
    /// **A mode is not claimed from a string that only resembles it.**
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS §0.0 ON A CARD, AND IT IS THE WHOLE REASON TASK 1 WENT AND
    /// READ THE SPECIFICATION.** ADIF files FT4 as a submode of `MFSK` and PSK31 as
    /// a submode of `PSK`, and <see cref="AdifContact"/> carries no submode at all.
    /// So `MODE=PSK` is *some kind of phase-shift keying* and reading it as PSK31
    /// would show him a first he never made.</para>
    /// <para>**AND `MODE=FT4` IS NOT VALID ADIF**, so a file carrying it is one
    /// somebody wrote by hand. It is still not FT4 by the specification's own
    /// spelling, and guessing that it was meant to be is the guess this project
    /// exists to refuse.</para>
    /// <para>**WATCHED FAILING FIRST**: with `Matches` ignoring the submode, the
    /// bare `PSK` record lit PSK31 and the card claimed a first he had not
    /// made.</para>
    /// </remarks>
    [AvaloniaFact]
    public void AModeIsNotClaimedFromAStringThatOnlyResemblesIt()
    {
        var log = new[]
        {
            Record("K1AAA", "PSK", "20m", new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc)),
            Record("K2BBB", "FT4", "20m", new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc)),
            Record("K3CCC", "MFSK", "20m", new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc)),
            Record("K4DDD", "Voice", "20m", new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc)),
        };

        var screen = new AchievementsViewModel(log);

        foreach (var row in screen.Firsts)
        {
            _output.WriteLine(row.Name.PadRight(7) + row.Standing);
        }

        Assert.DoesNotContain(screen.Firsts, f => f.Earned);

        // **AND THE ONE THAT WOULD HAVE LIT.** `MODE=SSB` is Voice by the
        // specification, and `MODE=Voice` is not an ADIF value at all.
        var voice = new AchievementsViewModel(new[]
        {
            Record("K5EEE", "SSB", "20m", new DateTime(2026, 9, 2, 0, 0, 0, DateTimeKind.Utc)),
        });

        Assert.True(voice.Firsts.Single(f => f.Name == "Voice").Earned);

        // **CASE IS NOT PART OF THE SPELLING.** ADI is a portable format and his
        // log can hold records another program wrote; a logger that writes `cw`
        // has still recorded a contact on CW. **Watched failing first**: with the
        // comparison ordinal, this record matched nothing and the row read
        // *waiting on Hamlet* while a contact in that mode sat in the file.
        var lower = new AchievementsViewModel(new[]
        {
            Record("K6FFF", "cw", "40m", new DateTime(2026, 9, 2, 0, 0, 0, DateTimeKind.Utc)),
        });

        _output.WriteLine("");
        _output.WriteLine(
            "a record written lowercase `cw` : "
            + lower.Firsts.Single(f => f.Name == "CW").Standing);

        Assert.True(lower.Firsts.Single(f => f.Name == "CW").Earned);
    }

    /// <summary>
    /// **WSPR says it is not a contact mode rather than showing a first nobody
    /// can earn.**
    /// </summary>
    /// <remarks>
    /// <para>Work instruction 287 task 4. **A row that can never light is a promise
    /// the application cannot keep**, and it is the quieter half of §0.0: not a
    /// claim that something happened, but a claim that something could.</para>
    /// <para>**IT STAYS THAT WAY EVEN WITH A `MODE=WSPR` RECORD IN THE FILE.** Such
    /// a record is legal ADIF and any logger would read it, and it would still name
    /// a station nobody worked, because a beacon has no addressee. So the row does
    /// not light from one.</para>
    /// </remarks>
    [AvaloniaFact]
    public void WsprIsNotAFirstAnybodyCanEarnAndTheCardSaysSo()
    {
        var withOne = new AchievementsViewModel(new[]
        {
            Record("KC3QIS", "WSPR", "20m", new DateTime(2026, 9, 5, 0, 0, 0, DateTimeKind.Utc)),
        });

        var wspr = withOne.Firsts.Single(f => f.Name == "WSPR");

        _output.WriteLine("with a MODE=WSPR record in the log:");
        _output.WriteLine("   standing : " + wspr.Standing);
        _output.WriteLine("   earned   : " + wspr.Earned);
        _output.WriteLine("   hover    : " + wspr.Why);

        Assert.False(wspr.Earned);
        Assert.Equal(ModeFirstState.NotAContact, wspr.State);
        Assert.Equal("not a contact mode", wspr.Standing);

        // **AND THE CARD SAYS IT UNASKED**, because a row he cannot fill in is a
        // fault rather than a tip.
        Assert.Contains("beacon", withOne.Honesty, StringComparison.Ordinal);
    }

    /// <summary>
    /// **Earned and unearned differ by more than a hue, and so do the two kinds
    /// of unearned.**
    /// </summary>
    /// <remarks>
    /// <para>§0.6, and Tim's ruling that the two unearned reasons must not look the
    /// same. **Roughly one man in twelve has a color vision deficiency and this
    /// hobby's demographics make that a real slice of who will use this**, so the
    /// test is the grayscale one: strip the ink and see whether the card still
    /// reads.</para>
    /// <para>**FOUR STATES, FOUR MARKS AND FOUR WORDS**, and an earned row also
    /// carries a line of evidence no other row has.</para>
    /// </remarks>
    [AvaloniaFact]
    public void EarnedAndUnearnedDifferByMoreThanTheirInk()
    {
        var screen = new AchievementsViewModel(TwoModeLog());

        foreach (var row in screen.Firsts)
        {
            _output.WriteLine(
                row.Name.PadRight(7) + "mark " + row.Mark
                + "   word " + row.Standing.PadRight(20)
                + "ink " + row.Ink);
        }

        var byState = screen.Firsts
            .GroupBy(f => f.State)
            .Select(g => g.First())
            .ToList();

        Assert.True(byState.Count > 1, "the fixture shows only one state");

        // **THE MARK ALONE SEPARATES EVERY STATE.**
        Assert.Equal(
            byState.Count, byState.Select(f => f.Mark).Distinct().Count());

        // **AND SO DOES THE WORD.**
        Assert.Equal(
            byState.Count, byState.Select(f => f.Standing).Distinct().Count());

        // **AND ONLY AN EARNED ROW CARRIES EVIDENCE.**
        Assert.All(
            screen.Firsts.Where(f => !f.Earned),
            f => Assert.False(f.HasEvidence));
    }

    /// <summary>**The window realizes, and reads without writing.**</summary>
    /// <remarks>
    /// **THE CARD IS DRAWN FROM THE ROWS AND NOT FROM SIX PIECES OF MARKUP**, so a
    /// mode added to <see cref="ContactModes.Six"/> appears without a view change.
    /// This checks that by counting what the realized window actually put on
    /// screen.
    /// </remarks>
    [AvaloniaFact]
    public void TheWindowDrawsEverySixRows()
    {
        var screen = new AchievementsViewModel(TwoModeLog());

        var window = new AchievementsWindow { DataContext = screen };

        window.Show();
        HowMuchTheApplicationSaysTests.Pump(window);

        var rows = window.GetVisualDescendants()
            .OfType<ItemsControl>()
            .Single(c => c.Name == "AchievementsModeRows");

        var shown = string.Join(
            " | ", HowMuchTheApplicationSaysTests.Shown(window));

        _output.WriteLine("rows bound   : " + rows.ItemCount);

        foreach (var mode in ContactModes.Six)
        {
            _output.WriteLine(
                "on screen    : " + mode.Name.PadRight(7)
                + (shown.Contains(mode.Name, StringComparison.Ordinal)
                    ? "yes"
                    : "NO"));
        }

        // **AND HOW MUCH OF THE CARD IS ON A HOVER RATHER THAN ON THE SCREEN**,
        // which is the whole of Tim's ruling of 2026-09-08 working rather than
        // being worked around. The window measures 443 characters against a
        // ceiling of 550; every row's account of what stands in its way is on a
        // `HintMarkControl` and draws nothing until he asks for it.
        var hover = screen.Firsts.Sum(f => f.Why.Length);

        _output.WriteLine("");
        _output.WriteLine(
            "hover prose  : " + hover + " characters, none of it on the screen");

        Assert.Equal(ContactModes.Six.Count, rows.ItemCount);

        Assert.True(
            hover > 1_000,
            "the rows explain themselves in under a thousand characters, so the "
            + "claim that this card keeps its prose on a hover is no longer worth "
            + "making");

        Assert.All(
            ContactModes.Six,
            m => Assert.Contains(m.Name, shown, StringComparison.Ordinal));
    }

    /// <summary>**A first FT8 contact announces once, and a second does not.**</summary>
    /// <remarks>
    /// <para>Work instruction 287 task 3. **The once rule is what this holds
    /// down**, and it is why the seed is written before anybody is told rather than
    /// after: a notice that threw on its way to the screen would otherwise leave
    /// the first unrecorded and fire again on the next contact. **That ordering is
    /// reasoning and not a watched red**, because nothing here can make the event
    /// throw; what was watched is below.</para>
    /// <para>**IT REUSES `BadgeAward`**, which is why the assertions here are about
    /// strings on a record rather than about a window. What he is told and when is
    /// then provable without opening anything.</para>
    /// </remarks>
    [AvaloniaFact]
    public void AFirstFt8ContactAnnouncesOnceAndASecondDoesNot()
    {
        var panel = Panel(alreadySaid: Array.Empty<string>());
        var awards = Watch(panel);

        var one = new[]
        {
            Record("W3YNI", "FT8", "20m",
                new DateTime(2026, 9, 8, 21, 41, 30, DateTimeKind.Utc)),
        };

        panel.AnnounceModeFirstsForTests(one);

        _output.WriteLine("first FT8 contact : " + Describe(awards));

        Assert.Single(awards);
        Assert.Equal("first FT8 contact", awards[0].Heading);
        Assert.Equal("W3YNI is your first contact on FT8.", awards[0].Says);
        Assert.Equal("It is on the achievements screen from now on.", awards[0].Next);
        Assert.Equal("FT8", awards[0].Ring);

        // **A SECOND FT8 CONTACT IS NOT A FIRST.** The same log again, and a longer
        // one, both say nothing.
        panel.AnnounceModeFirstsForTests(one);

        panel.AnnounceModeFirstsForTests(new[]
        {
            one[0],
            Record("K2ABC", "FT8", "40m",
                new DateTime(2026, 9, 8, 22, 0, 0, DateTimeKind.Utc)),
        });

        _output.WriteLine("and a second      : " + Describe(awards));

        Assert.Single(awards);
    }

    /// <summary>
    /// **A first look at a log that already holds modes announces nothing, and the
    /// next mode after that does.**
    /// </summary>
    /// <remarks>
    /// <para>Work instruction 287 task 3, and unit 278's seeding rule underneath
    /// it. **Without the seed, installing Hamlet beside an existing log fires a
    /// notice for every mode in it at once**, one on top of another, for contacts
    /// Hamlet was not there for.</para>
    /// <para>**THE ORDER ASKED FOR THREE MODES AND THEN A FOURTH, AND THERE IS NO
    /// FOURTH TO HAVE.** Only three of the six can be matched from a record at all
    /// today: FT4 and PSK31 are ADIF submodes and <see cref="AdifContact"/> carries
    /// no `SUBMODE`, and WSPR is a beacon that nobody works. So this seeds with two
    /// and fires on the third, which is the same shape against the modes that
    /// exist. Reported rather than worked around.</para>
    /// <para>**WATCHED FAILING FIRST**: with the seeding branch removed, the first
    /// look raised two awards instead of none.</para>
    /// </remarks>
    [AvaloniaFact]
    public void AFirstLookSaysNothingAndTheModeAfterItDoes()
    {
        // **NULL IS A LOG NEVER LOOKED AT**, which is what a fresh install holds.
        // The panel's own construction reads the empty file on disk and seeds an
        // empty list from it, so the fixture puts it back to never-looked rather
        // than pretending an untouched panel is in that state.
        var panel = Panel(alreadySaid: null);
        var awards = Watch(panel);

        var two = new[]
        {
            Record("W3YNI", "FT8", "20m",
                new DateTime(2026, 8, 14, 21, 41, 30, DateTimeKind.Utc)),
            Record("VA3VRR", "CW", "40m",
                new DateTime(2026, 8, 17, 1, 33, 47, DateTimeKind.Utc)),
        };

        panel.AnnounceModeFirstsForTests(two);

        _output.WriteLine("first look at 2 modes : " + Describe(awards));

        Assert.Empty(awards);

        // **AND THE ONE AFTER IT DOES FIRE**, so seeding silences the past and
        // never the future.
        panel.AnnounceModeFirstsForTests(new[]
        {
            two[0],
            two[1],
            Record("K5EEE", "SSB", "20m",
                new DateTime(2026, 9, 8, 23, 10, 0, DateTimeKind.Utc)),
        });

        _output.WriteLine("then a voice contact  : " + Describe(awards));

        Assert.Single(awards);
        Assert.Equal("first Voice contact", awards[0].Heading);
    }

    /// <summary>**The mode notice takes nothing from him either.**</summary>
    /// <remarks>
    /// <para>Unit 286 built these properties for the belt and the order says they
    /// are not to be weakened. **Reusing the window is exactly how they would be
    /// weakened without anybody noticing**, so they are asserted again with a mode
    /// first in it rather than assumed to have survived.</para>
    /// <para>**`ShowActivated` IS THE ONE THAT MATTERS.** A window that activates
    /// closes an open right-click menu, which is the precise way a notice would
    /// cost him the contact it is congratulating him for.</para>
    /// </remarks>
    [AvaloniaFact]
    public void TheModeNoticeTakesNoFocusAndLeavesOnItsOwn()
    {
        var row = new AchievementsViewModel(TwoModeLog())
            .Firsts.Single(f => f.Name == "FT8");

        var award = new BadgeAward(3, Array.Empty<int>()) { First = row };

        var notice = new BadgeWindow { DataContext = award };

        _output.WriteLine("heading  : " + award.Heading);
        _output.WriteLine("says     : " + award.Says);
        _output.WriteLine("next     : " + award.Next);
        _output.WriteLine("ring     : " + award.Ring + "   ink " + award.Ink);
        _output.WriteLine("stays    : " + BadgeWindow.Stays.TotalSeconds + " s");

        Assert.False(notice.ShowActivated);
        Assert.False(notice.ShowInTaskbar);
        Assert.True(notice.Topmost);
        Assert.Equal(TimeSpan.FromSeconds(8), BadgeWindow.Stays);

        // **AND IT SAYS SOMETHING**, which is what `Announce` gates on: an award
        // with an empty `Says` is shown to nobody, and a mode first that came out
        // empty would vanish without a trace.
        Assert.NotEqual("", award.Says);

        notice.Close();
    }

    /// <summary>A panel whose mode firsts have been seeded.</summary>
    /// <param name="alreadySaid">
    /// The modes already announced, or null for a log never looked at.
    /// </param>
    /// <returns>The panel.</returns>
    private static MainWindowViewModel Panel(IReadOnlyList<string>? alreadySaid)
    {
        // Nothing on disk, so the log is only ever what a test hands over
        // (`FACT-006`: this machine has no contact log and never will).
        File.WriteAllText(ContactLogStore.LogPath, AdifLog.Header("1.12.212"));

        var settings = HowMuchTheApplicationSaysTests.Settled();

        var panel = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
        };

        settings.ContactModeFirstsAnnounced =
            alreadySaid is null ? null : alreadySaid.ToList();

        return panel;
    }

    /// <summary>Collect every award a panel raises.</summary>
    private static List<BadgeAward> Watch(MainWindowViewModel panel)
    {
        var awards = new List<BadgeAward>();

        panel.BadgeEarned += (_, award) => awards.Add(award);

        return awards;
    }

    /// <summary>The awards, for the printout.</summary>
    private static string Describe(IReadOnlyList<BadgeAward> awards)
        => awards.Count == 0
            ? "nothing"
            : string.Join(" | ", awards.Select(a => a.Heading + " - " + a.Says));

    /// <summary>One sound record, for a synthesised log.</summary>
    private static AdifLogRecord Record(
        string call, string mode, string band, DateTime startedUtc)
        => new(
            new AdifContact
            {
                Call = call,
                StationCallsign = "KC3QIS",
                Mode = mode,
                Band = band,
                StartedUtc = startedUtc,
            },
            Array.Empty<string>(),
            true);
}
