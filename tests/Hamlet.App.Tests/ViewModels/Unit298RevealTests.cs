using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 298, task 6: **when a group opens, say so once, and never on a
/// first look at an existing log.**
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THIS WOULD HAVE CAUGHT**, and it is the one unit 278 wrote
/// its seeding rule against: a man who has been logging in another program imports
/// fourteen contacts, Hamlet reads them, and he gets a stack of notices celebrating
/// things he did last year. **That is not a reward, it is noise**, and it teaches him
/// to dismiss the notice that matters.</para>
/// <para>**AND THE QUIETER TWIN**: a group that opens and says nothing. §3.1 is
/// explicit that *a new tab appearing is itself the reward*, and a reward that
/// happens in a window he is not looking at is not one.</para>
/// <para>**IT WAS WATCHED FAILING, AND THE NUMBER IS MEASURED.** With the `null`
/// branch of `AnnounceOpenings` replaced by an empty list,
/// `AFirstLookAtAnExistingLogSaysNothing` reports **4 notices** on a log the operator
/// has never seen Hamlet read, and `AGroupThatOpensLaterIsAnnouncedOnce` fails with
/// it. Restored, both are green.</para>
/// </remarks>
public sealed class Unit298RevealTests
{
    private const string HisGrid = "FN00";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the notices are printed.</param>
    public Unit298RevealTests(ITestOutputHelper output) => _output = output;

    /// <summary>**A first look at an existing log announces nothing.**</summary>
    /// <remarks>
    /// The instruction's own task 2 line: *a fresh log at fourteen contacts announces
    /// nothing on first look.* It lives here because it is the reveal that has to be
    /// quiet, not the screen.
    /// </remarks>
    [Fact]
    public void AFirstLookAtAnExistingLogSaysNothing()
    {
        var settings = Settings();

        // **NULL IS *NOBODY HAS LOOKED YET*.** It is the state of a fresh install
        // that then reads a log somebody else wrote.
        Assert.Null(settings.AchievementGroupsAnnounced);

        var model = new MainWindowViewModel(settings, null);
        var said = new List<BadgeAward>();

        model.BadgeEarned += (_, award) => said.Add(award);

        model.AnnounceOpeningsForTests(Fourteen());

        _output.WriteLine("notices: " + said.Count);

        Assert.Empty(said);

        // **AND IT WROTE DOWN WHAT IT SAW**, so the next contact is news.
        Assert.NotNull(settings.AchievementGroupsAnnounced);
        Assert.NotEmpty(settings.AchievementGroupsAnnounced!);
    }

    /// <summary>**A group that opens afterwards is announced once.**</summary>
    [Fact]
    public void AGroupThatOpensLaterIsAnnouncedOnce()
    {
        var settings = Settings();
        var model = new MainWindowViewModel(settings, null);
        var said = new List<BadgeAward>();

        model.BadgeEarned += (_, award) => said.Add(award);

        // The quiet first look.
        model.AnnounceOpeningsForTests(Fourteen());
        Assert.Empty(said);

        // Then one contact on a band he has never worked.
        var grown = Fourteen().ToList();
        grown.Add(Contact("EI4GNB", "40m", "FT8", "IO63", -14, -09, "2026-09-11 02:20:00"));

        model.AnnounceOpeningsForTests(grown);

        foreach (var award in said)
        {
            _output.WriteLine(award.Heading + " — " + award.Says);
            _output.WriteLine("    ring: " + award.Ring + "   next: " + award.Next);
        }

        Assert.NotEmpty(said);

        var forty = said.SingleOrDefault(a => a.Opened?.Title == "40 m");

        Assert.True(forty is not null, "opening 40 m announced nothing");

        Assert.Contains("40 m is open", forty!.Heading, StringComparison.Ordinal);
        Assert.Contains("new records in it", forty.Says, StringComparison.Ordinal);

        // **ONCE.** Reading the same log again says nothing more.
        var again = said.Count;

        model.AnnounceOpeningsForTests(grown);

        Assert.Equal(again, said.Count);
    }

    /// <summary>**It names how many possibilities came with it.**</summary>
    /// <remarks>
    /// **§3.2 MADE VISIBLE.** *One contact in, three or four possibilities out.* The
    /// count is the point: an opening that revealed one card is a filled slot and one
    /// that revealed several is the screen growing.
    /// </remarks>
    [Fact]
    public void TheNoticeNamesHowMuchOpened()
    {
        var settings = Settings();
        var model = new MainWindowViewModel(settings, null);
        var said = new List<BadgeAward>();

        model.BadgeEarned += (_, award) => said.Add(award);

        model.AnnounceOpeningsForTests(Fourteen());

        var grown = Fourteen().ToList();
        grown.Add(Contact("EI4GNB", "40m", "FT8", "IO63", -14, -09, "2026-09-11 02:20:00"));

        model.AnnounceOpeningsForTests(grown);

        var forty = said.Single(a => a.Opened?.Title == "40 m");

        _output.WriteLine(forty.Says);

        Assert.True(
            forty.Opened!.Cards > 1,
            "the notice says " + forty.Opened.Cards + " card opened, and §3.2 wants "
            + "an unlock to reveal more than it fills");
    }

    /// <summary>**A fresh install with nothing logged announces nothing either.**</summary>
    /// <remarks>
    /// **EMPTY AND NULL BOTH STAY QUIET HERE**, for different reasons: null because
    /// nobody has looked, and an empty log because nothing is open to announce.
    /// </remarks>
    [Fact]
    public void AnEmptyLogAnnouncesNothing()
    {
        var settings = Settings();

        settings.AchievementGroupsAnnounced = new List<string>();

        var model = new MainWindowViewModel(settings, null);
        var said = new List<BadgeAward>();

        model.BadgeEarned += (_, award) => said.Add(award);

        model.AnnounceOpeningsForTests(Array.Empty<AdifLogRecord>());

        Assert.Empty(said);
    }

    /// <summary>Settings as they stand on his machine, with nothing announced.</summary>
    private static AppSettings Settings()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = HisGrid;

        return settings;
    }

    /// <summary>Fourteen contacts, all FT8 on 20 metres, as an import would look.</summary>
    private static IReadOnlyList<AdifLogRecord> Fourteen()
        => Enumerable.Range(0, 14)
            .Select(i => Contact(
                "W" + (i + 1) + "ABC", "20m", "FT8", "FN42", -12, -07,
                "2026-09-10 02:" + i.ToString("00", CultureInfo.InvariantCulture) + ":00"))
            .ToList();

    /// <summary>One sound record.</summary>
    private static AdifLogRecord Contact(
        string call, string band, string mode, string? grid,
        int? sent, int? received, string startedUtc)
        => new(
            new AdifContact
            {
                Call = call,
                StationCallsign = "KC3QIS",
                Band = band,
                Mode = mode,
                GridSquare = grid,
                MyGridSquare = HisGrid,
                ReportSent = sent?.ToString("+00;-00", CultureInfo.InvariantCulture),
                ReportReceived = received?.ToString("+00;-00", CultureInfo.InvariantCulture),
                StartedUtc = DateTime.Parse(
                    startedUtc, CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal),
            },
            Array.Empty<string>(),
            Terminated: true);
}
