using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Tests.Psk31;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 379 task 2, **criterion 8.2**: a logged PSK31 contact and a logged Olivia
/// contact earn exactly what the same contact logged on FT8 earns.
/// </summary>
/// <remarks>
/// <para>**THE TWO CONTACTS ARE DRIVEN ALL THE WAY THROUGH AND THIS IS NOT A HAND-BUILT LOG.**
/// Each mode's contact is worked as a conversation, turned into a record by
/// `ContactLogEntryForStation` - the one place a log record is built - appended through
/// `ContactLogStore`, re-read by `ReloadContactLogForTests`, which is `RefreshWorkedBefore`, and
/// only then read back through `AchievementLog` and `AchievementScores`. A test that scored a
/// record it had typed itself would prove the scorer reads its own fixture and nothing about
/// what a press of Log earns him.</para>
/// <para>**EVERY ASSERTION IS AGAINST THE FT8 CONTACT'S OWN NUMBERS AND NEVER AGAINST A NUMBER
/// TYPED HERE** (section 6 ruling 1). The claim is *the same as FT8*, not *equal to 7*, so this
/// class goes on meaning what it says if the points file changes, if a band moves, or if the
/// station's entity is re-read. The three contacts are the same callsign, the same grid, the same
/// band and the same operator grid, so the only thing that can differ is the mode.</para>
/// <para>**A CELL WHERE ALL THREE AGREE IS PARITY, INCLUDING ALL THREE BEING NOTHING** (ruling 1
/// item 2). `Ft8ContactLogEntry.For` writes no `STATE` for any mode, so an FT8 contact Hamlet
/// logged earns no state record either; that is asserted here as parity rather than reported as a
/// gap, and it is asserted off the FT8 column rather than off this paragraph.</para>
/// <para>**COMPUTED, NOT SEEN** (FACT-004). Every contact here is a replay on the development
/// machine. No port is opened, no device enumerated, nothing keyed, and no record written here is
/// a contact anybody made.</para>
/// </remarks>
public sealed class TheKeyboardModesEarnWhatFt8EarnsTests : IDisposable
{
    /// <summary>20 m, inside a General class data segment, where the cited table gives an Olivia spot.</summary>
    private const long DialOn20m = 14_071_500;

    /// <summary>Where FT8 lives on the same band.</summary>
    private const long Ft8DialOn20m = 14_074_000;

    /// <summary>The operator's own locator.</summary>
    private const string MyGrid = "FN00DJ";

    /// <summary>The one station, worked once in each of the three modes.</summary>
    private const string His = "W1AW";

    /// <summary>His locator, the one the textbook transcript declares and the FT8 payload carries.</summary>
    private const string HisGrid = "FN31";

    /// <summary>Where a PSK31 carrier sits in the passband.</summary>
    private const double Psk31Hz = 1234;

    /// <summary>The transcript worked to 73, replayed down both keyboard panels.</summary>
    private const string Textbook = "01-textbook";

    /// <summary>The Olivia variant the channel is read at.</summary>
    private const string Variant = "16/500";

    /// <summary>What `ContactLogStore` stamps a record with here.</summary>
    private const string Version = "1.13.66";

    private readonly ITestOutputHelper _output;
    private readonly string _root;
    private readonly string _wasFolder;

    /// <summary>Creates the tests and redirects the data folder, so no real log is touched.</summary>
    /// <param name="output">Where the numbers are printed.</param>
    public TheKeyboardModesEarnWhatFt8EarnsTests(ITestOutputHelper output)
    {
        _output = output;
        _wasFolder = SettingsStore.DataFolder;
        _root = Path.Combine(Path.GetTempPath(), "hamlet-unit379-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_root);
    }

    /// <summary>Puts the real folder back.</summary>
    public void Dispose()
    {
        SettingsStore.DataFolder = _wasFolder;

        try
        {
            Directory.Delete(_root, recursive: true);
        }
        catch (IOException)
        {
        }
    }

    /// <summary>
    /// **1: a logged PSK31 contact earns what the same contact logged on FT8 earns.**
    /// </summary>
    [Fact]
    public void APsk31ContactEarnsWhatTheSameFt8ContactEarns() => EarnsWhatFt8Earns("PSK31");

    /// <summary>
    /// **2: a logged Olivia contact earns what the same contact logged on FT8 earns.**
    /// </summary>
    [Fact]
    public void AnOliviaContactEarnsWhatTheSameFt8ContactEarns()
        => EarnsWhatFt8Earns(ContactModes.OliviaName);

    /// <summary>
    /// **3: and the per-contact facts the log answers are the same facts, mode by mode.**
    /// </summary>
    /// <remarks>
    /// **THE KINDS ARE COUNTS AND THESE ARE THE ANSWERS BEHIND THEM.** `countries 1` on all three
    /// columns would be satisfied by three different countries, so the entity, the continent, the
    /// band, the grid, the miles and the state are compared one against another as well.
    /// </remarks>
    [Fact]
    public void TheRecordsBehindTheCountsAreTheSameRecordsInEveryMode()
    {
        var ft8 = Worked("FT8").Contacts.Single();

        foreach (var mode in new[] { "PSK31", ContactModes.OliviaName })
        {
            var his = Worked(mode).Contacts.Single();

            _output.WriteLine(
                mode + ": entity " + Show(his.Entity) + ", continent " + Show(his.Continent)
                + ", band " + Show(his.Band) + ", grid " + Show(his.Grid)
                + ", state " + Show(his.State) + ", miles " + Show(his.Miles?.ToString("0")));

            Assert.Equal(ft8.Callsign, his.Callsign);
            Assert.Equal(ft8.Entity, his.Entity);
            Assert.Equal(ft8.Continent, his.Continent);
            Assert.Equal(ft8.Band, his.Band);
            Assert.Equal(ft8.Grid, his.Grid);
            Assert.Equal(ft8.MyGrid, his.MyGrid);

            // **THE MILES ARE THE SAME GREAT CIRCLE**, because both ends are the same two grids.
            Assert.Equal(ft8.Miles, his.Miles);

            // **AND THE STATE IS WHAT FT8'S IS, WHATEVER THAT IS** (ruling 1 item 2). This is
            // asserted off the FT8 column and not off a literal: if Hamlet ever starts writing a
            // STATE, this goes on demanding that a keyboard mode writes one too.
            Assert.Equal(ft8.State, his.State);

            // **AND THE MODE IS THE ONE HE WORKED**, which is the fact that makes all of the above
            // a statement about parity rather than about three copies of one record.
            Assert.Equal(mode, his.Mode?.Name);
            Assert.NotEqual(ft8.Mode?.Name, his.Mode?.Name);
        }
    }

    /// <summary>
    /// **4: the Hall of Fame card for each keyboard mode names the contact that earned it.**
    /// </summary>
    /// <remarks>
    /// **THE SCORE AND THE CARD HAVE TO AGREE.** Before this unit, `first_olivia` was earned and
    /// scored at its full worth while its card carried no callsign, no grid, no band-and-mode line
    /// and no date, because `AchievementCategory.EarnerOf` had no row for the key. The kind's
    /// count could not see it - both modes scored 15 - so the card is what is asserted.
    /// </remarks>
    [Fact]
    public void TheHallOfFameCardForEachKeyboardModeCarriesTheContactThatEarnedIt()
    {
        var points = AchievementPoints.Parse(AchievementPoints.Shipped());

        foreach (var mode in new[] { "PSK31", ContactModes.OliviaName })
        {
            var page = new AchievementBadgePage(Worked(mode), points) { OperatorGrid = MyGrid };
            var category = AchievementCategory.For(AchievementKinds.HallOfFame, page);

            Assert.NotNull(category);

            var card = Assert.Single(
                category!.Cards,
                c => c.Earned && c.Title.Contains(mode, StringComparison.OrdinalIgnoreCase));

            _output.WriteLine(
                mode + ": \"" + card.Title + "\" figure \"" + card.Figure + "\" callsign \""
                + card.Callsign + "\" callGrid \"" + card.CallGridLine + "\" bandMode \""
                + card.BandModeLine + "\" date \"" + card.DateLine + "\"");

            // **THE STATION IS ON IT**, and it is the one that was worked.
            Assert.Equal(His, card.Callsign);
            Assert.NotEqual("", card.Figure);
            Assert.NotEqual("", card.CallGridLine);
            Assert.NotEqual("", card.DateLine);

            // **AND THE LINE NAMES THE MODE HE EARNED IT IN**, not another one.
            Assert.Contains(mode, card.BandModeLine, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// **5: an unearned keyboard first says what would earn it, and never that anything would.**
    /// </summary>
    /// <remarks>
    /// **A SENTENCE ON THE SCREEN IS A CLAIM** (§0.0). `Any station at all` on the Olivia card
    /// told him a contact with anybody earned the Olivia first, which is false; he could act on it
    /// and be wrong. Both modes are asserted, so neither can regress to the generic line alone.
    /// </remarks>
    [Fact]
    public void AnUnearnedKeyboardFirstSaysWhatWouldActuallyEarnIt()
    {
        var points = AchievementPoints.Parse(AchievementPoints.Shipped());

        foreach (var mode in new[] { "PSK31", ContactModes.OliviaName })
        {
            var page = new AchievementBadgePage(EveryModeBut(mode), points) { OperatorGrid = MyGrid };
            var category = AchievementCategory.For(AchievementKinds.HallOfFame, page);

            Assert.NotNull(category);

            var card = Assert.Single(category!.Cards, c => !c.Earned);

            _output.WriteLine(mode + " unearned: \"" + card.Title + "\" wants \"" + card.WantsLine + "\"");

            Assert.Contains(mode, card.Title, StringComparison.OrdinalIgnoreCase);

            // **IT NAMES THE MODE**, which is the whole of what the generic line failed to do.
            Assert.Contains(mode, card.WantsLine, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Any station at all", card.WantsLine, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// **The comparison both of the first two tests are**: every kind, every first, every score
    /// and the total, against the FT8 contact's own numbers.
    /// </summary>
    private void EarnsWhatFt8Earns(string mode)
    {
        var points = AchievementPoints.Parse(AchievementPoints.Shipped());

        var ft8Log = Worked("FT8");
        var hisLog = Worked(mode);

        var ft8Scores = new AchievementScores(ft8Log, points);
        var hisScores = new AchievementScores(hisLog, points);

        _output.WriteLine("kind            FT8        " + mode);

        foreach (var kind in AchievementKinds.All)
        {
            _output.WriteLine(
                kind.PadRight(16)
                + AchievementScores.WorkedIn(kind, ft8Log).ToString().PadRight(11)
                + AchievementScores.WorkedIn(kind, hisLog));
        }

        _output.WriteLine("FT8   firsts: " + string.Join(", ", AchievementScores.FirstsEarned(ft8Log)));
        _output.WriteLine(mode + " firsts: " + string.Join(", ", AchievementScores.FirstsEarned(hisLog)));
        _output.WriteLine("totals: FT8 " + ft8Scores.Total + ", " + mode + " " + hisScores.Total);

        // **THE SEVEN KINDS THAT DO NOT TURN ON THE MODE ARE EQUAL, CELL FOR CELL.**
        foreach (var kind in AchievementKinds.All.Where(k => k != AchievementKinds.HallOfFame))
        {
            Assert.Equal(
                AchievementScores.WorkedIn(kind, ft8Log),
                AchievementScores.WorkedIn(kind, hisLog));

            Assert.Equal(ft8Scores.For(kind).Points, hisScores.For(kind).Points);
            Assert.Equal(ft8Scores.For(kind).Level, hisScores.For(kind).Level);
        }

        // **AND THE HALL OF FAME EARNS EVERYTHING THE FT8 CONTACT EARNS.** It is a superset and
        // not an equality, and the extra is measured rather than allowed for: the owner's points
        // file carries a `first_psk31` and a `first_olivia` key and **no `first_ft8` key at all**,
        // so the same evening on a keyboard mode earns one first more than FT8 does. That is the
        // file's doing, it is in the keyboard modes' favour, and 8.2 asks that they earn what FT8
        // earns - never that they be levelled down to it.
        var ft8Firsts = AchievementScores.FirstsEarned(ft8Log);
        var hisFirsts = AchievementScores.FirstsEarned(hisLog);

        foreach (var key in ft8Firsts)
        {
            Assert.Contains(key, hisFirsts);
        }

        Assert.True(
            hisScores.For(AchievementKinds.HallOfFame).Points
            >= ft8Scores.For(AchievementKinds.HallOfFame).Points,
            mode + " earns less in the Hall of Fame than the same contact on FT8");

        Assert.True(hisScores.Total >= ft8Scores.Total, mode + " scores less in total than FT8");

        // **AND THE ONE EXTRA IS THIS MODE'S OWN FIRST AND NOTHING ELSE**, so a keyboard mode
        // cannot quietly start earning a first that has nothing to do with the mode.
        var extra = hisFirsts.Except(ft8Firsts, StringComparer.OrdinalIgnoreCase).ToList();

        Assert.Equal(new[] { "first_" + mode.ToLowerInvariant() }, extra);
    }

    /// <summary>
    /// **One contact worked in one mode and logged the way a press of Log logs it**, read back
    /// off the file.
    /// </summary>
    /// <remarks>
    /// Each mode gets a folder of its own, so the three logs are three logs and a contact worked
    /// in one mode cannot score in another's column.
    /// </remarks>
    private AchievementLog Worked(string mode)
    {
        var folder = Path.Combine(_root, mode);

        Directory.CreateDirectory(folder);
        SettingsStore.DataFolder = folder;

        var corpus = Psk31Corpus.Load();
        var model = PanelFor(mode, corpus.Operator);
        var dial = string.Equals(mode, "FT8", StringComparison.OrdinalIgnoreCase)
            ? Ft8DialOn20m
            : DialOn20m;

        WorkHim(model, mode, corpus);

        var entry = model.ContactLogEntryForStation(His, dial);

        Assert.NotNull(entry);

        // **THROUGH THE STORE, AND THEN RE-READ THROUGH THE REFRESH A PRESS OF LOG RUNS.**
        Assert.True(ContactLogStore.Append(entry!, Version));

        model.ReloadContactLogForTests();

        var records = ContactLogStore.ReadRecords();

        Assert.Single(records);

        return new AchievementLog(records, MyGrid);
    }

    /// <summary>A hand-built log holding a contact in every workable mode but one.</summary>
    /// <remarks>
    /// **THIS ONE IS A FIXTURE AND IS ALLOWED TO BE**: it is not asserting what a contact earns,
    /// it is putting the page into the one state where the mode's own first is the unearned card
    /// the Hall of Fame draws.
    /// </remarks>
    private static AchievementLog EveryModeBut(string mode)
        => new(
            ContactModes.Logged
                .Where(m => m.IsContactMode
                            && !string.Equals(m.Name, mode, StringComparison.OrdinalIgnoreCase))
                .SelectMany(m => new[] { Fixture(m, "W1AW", "FN31"), Fixture(m, "YB1ABC", "OI33") })
                .ToList(),
            MyGrid);

    /// <summary>One fixture record, for the unearned-card state only.</summary>
    private static AdifLogRecord Fixture(ContactMode mode, string call, string grid)
        => new(
            new AdifContact
            {
                Call = call,
                StationCallsign = "KC3QIS",
                Band = "20m",
                Mode = mode.AdifMode,
                Submode = mode.AdifSubmode,
                GridSquare = grid,
                MyGridSquare = MyGrid,
                StartedUtc = new DateTime(2026, 9, 19, 21, 0, 0, DateTimeKind.Utc),
                EndedUtc = new DateTime(2026, 9, 19, 21, 12, 0, DateTimeKind.Utc),
            },
            [],
            true);

    /// <summary>Works the one station in whichever mode this run is about.</summary>
    private static void WorkHim(MainWindowViewModel model, string mode, Psk31Corpus corpus)
    {
        if (string.Equals(mode, "FT8", StringComparison.OrdinalIgnoreCase))
        {
            var slot = new DateTime(2026, 9, 19, 21, 41, 30, DateTimeKind.Utc);

            model.AddDecodeRowForTests(
                "214130", "-12", "0.2", "1240", "CQ " + His + " " + HisGrid, slot, Ft8DialOn20m);
            model.AddDecodeRowForTests(
                "214145", "-12", "0.2", "1240", corpus.Operator + " " + His + " -12",
                slot.AddSeconds(15), Ft8DialOn20m);

            return;
        }

        // **THE SAME WORDS DOWN WHICHEVER KEYBOARD PANEL THIS IS**, so the two keyboard columns
        // differ in the mode and in nothing else.
        var olivia = string.Equals(mode, ContactModes.OliviaName, StringComparison.OrdinalIgnoreCase);
        var calling = olivia ? CallingOffsetOn(model) : Psk31Hz;

        var his = corpus.Transcripts.Single(t => t.Name == Textbook).Lines
            .Where(l => string.Equals(l.Frm, His, StringComparison.OrdinalIgnoreCase))
            .Select(l => l.Text + "\n")
            .ToList();

        var heard = "";

        for (var over = 0; over < his.Count; over++)
        {
            heard += his[over];

            if (olivia)
            {
                model.ShowOliviaChannelsForTests([OliviaChannelOf(68, Variant, calling, heard)]);
            }
            else
            {
                model.ShowPsk31ChannelsForTests([new Psk31Channel(1, calling, 10.0, heard)]);
            }

            if (over == 0)
            {
                model.AnswerPsk31Command.Execute(model.DigitalDecodes.First(r => r.Sender == His));
            }
            else
            {
                model.CardActionCommand.Execute(model.DigitalCards.First(c => c.Callsign == His));
            }

            Settle(model);
        }
    }

    /// <summary>An Olivia channel as the listener would list it.</summary>
    private static OliviaChannel OliviaChannelOf(int id, string variant, double centerHz, string text)
        => new(id, variant, centerHz, OliviaListener.FoundByRsid, 0, text, text.Length > 0 ? 9 : 0, 0, false, centerHz);

    /// <summary>Where the band's Olivia calling center sits, by the cited table's own arithmetic.</summary>
    private static double CallingOffsetOn(MainWindowViewModel model)
    {
        var row = OliviaData.Current.Calling!.CallingRowFor(model.SelectedBand.Band.Name)!;

        return row.CenterHz - (double)model.FrequencyHz;
    }

    private static MainWindowViewModel PanelFor(string mode, string mine)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = mine;
        settings.Operator.GridSquare = MyGrid;
        settings.Operator.OperatorName = "Tim";
        settings.Operator.Location = "Trafford PA";
        settings.Operator.LicenseClass = LicenseClass.General;

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        var dial = string.Equals(mode, "FT8", StringComparison.OrdinalIgnoreCase)
            ? Ft8DialOn20m
            : DialOn20m;

        model.SelectedBand = model.Bands.First(b => b.Band.LowHz <= dial && b.Band.HighHz >= dial);
        model.FrequencyHz = dial;
        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.UseRigPortForTests(new FakePort());
        model.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(new FakePort(), new FakeSink())));

        if (string.Equals(mode, ContactModes.OliviaName, StringComparison.OrdinalIgnoreCase))
        {
            model.TapForTests = new AudioTap();
        }

        model.ChooseDigitalModeCommand.Execute(mode);

        return model;
    }

    /// <summary>Wait for the send the press started, and run what it posted to the UI thread.</summary>
    private static void Settle(MainWindowViewModel model)
    {
        for (var tries = 0; tries < 2000 && model.HasSomethingToStop; tries++)
        {
            System.Threading.Thread.Sleep(10);
        }

        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
    }

    private static string Show(string? value) => value ?? "null";
}
