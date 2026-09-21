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
/// Work instruction 379 task 3, **criteria 8.3 and 8.4**: the quill on a keyboard-mode row means
/// what it means on an FT8 row, the CQ list names the mode the row actually is, and the scores and
/// the total move on the achievements page when one of those contacts is logged.
/// </summary>
/// <remarks>
/// <para>**8.3's QUILL WAS ALREADY TRUE AND IS ASSERTED, NOT REBUILT** (R14). Task 1 measured it
/// before anything moved: a PSK31 row and an Olivia row reach `MarkIfItOpensSomething` from the
/// text-row builder, the same method `PlaceRow` calls for an FT8 row, off the same `NudgeSet`, and
/// all three came back with the same kind and the same four strings. Nothing was written to make
/// that so; this class is what stops it drifting.</para>
/// <para>**8.3's CQ LABEL WAS NOT TRUE AND WAS REPAIRED AT ONE SITE.** `CqSnapshot` named the mode
/// from `IsTextOnly`, which is a question about a row's shape and not about its mode, so an Olivia
/// station calling CQ was labelled `PSK31` on the achievements page's next cards. It now reads the
/// row's own variant. **The FT8-shaped row still claims nothing**, and that is asserted, because a
/// repair that made every row name a mode would have invented two.</para>
/// <para>**8.4 IS A BEFORE AND AN AFTER, NOT A NUMBER.** The page is built the way
/// `OpenAchievements` builds it - the records, the operator's grid, the points file - read before
/// the contact and again after it, and the difference is asserted. A test that asserted the
/// figure alone would pass on a page that had always said it.</para>
/// <para>**COMPUTED, NOT SEEN** (FACT-004). No port is opened, no device enumerated, nothing
/// keyed.</para>
/// </remarks>
public sealed class TheQuillAndThePageMeanTheSameTests : IDisposable
{
    /// <summary>20 m, inside a General class data segment, where the cited table gives an Olivia spot.</summary>
    private const long DialOn20m = 14_071_500;

    /// <summary>Where FT8 lives on the same band.</summary>
    private const long Ft8DialOn20m = 14_074_000;

    /// <summary>The operator's own locator.</summary>
    private const string MyGrid = "FN00DJ";

    /// <summary>
    /// **The station the quill has something to say about.** `YB` is one entity in the cited
    /// table - Indonesia - so working him opens a country and a continent the empty log has never
    /// reached, which is what makes the mark a Door rather than nothing at all.
    /// </summary>
    private const string His = "YB1ABC";

    /// <summary>The station worked in the two 8.4 drives, whose transcript the corpus holds.</summary>
    private const string Worked = "W1AW";

    /// <summary>His locator, which the textbook transcript declares.</summary>
    private const string WorkedGrid = "FN31";

    /// <summary>Where a PSK31 carrier sits in the passband.</summary>
    private const double Psk31Hz = 1234;

    private const string Textbook = "01-textbook";

    /// <summary>The Olivia variant the channel is read at.</summary>
    private const string Variant = "16/500";

    private const string Version = "1.13.66";

    private readonly ITestOutputHelper _output;
    private readonly string _root;
    private readonly string _wasFolder;

    /// <summary>Creates the tests and redirects the data folder.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public TheQuillAndThePageMeanTheSameTests(ITestOutputHelper output)
    {
        _output = output;
        _wasFolder = SettingsStore.DataFolder;
        _root = Path.Combine(Path.GetTempPath(), "hamlet-unit379q-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_root);
        SettingsStore.DataFolder = _root;
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
    /// **8.3, first half: the quill on a PSK31 row and on an Olivia row is the FT8 row's quill,
    /// from the same nudge.**
    /// </summary>
    /// <remarks>
    /// **THE WORDS ARE COMPARED AGAINST THE FT8 ROW'S AND NEVER AGAINST A LITERAL**, so this goes
    /// on meaning *the same* if `NudgeWords` is ever reworded. **And the mark is asserted to say
    /// something first**: three rows all reading `None` with four empty strings would satisfy an
    /// equality test and prove nothing, which is the shape of a vacuous pass.
    /// </remarks>
    [Fact]
    public void TheQuillOnAKeyboardRowIsTheSameQuillWithTheSameWordsAsOnAnFt8Row()
    {
        var rows = RowsForOneStationInEveryMode();

        var ft8 = rows["FT8"];

        _output.WriteLine("FT8   " + ft8.Nudge + " | " + ft8.NudgeTip);
        _output.WriteLine("      reason: " + ft8.NudgeReasonLine);
        _output.WriteLine("      earns : " + ft8.NudgeEarnsLine);

        // **THE FT8 ROW'S QUILL SAYS SOMETHING**, or the comparison below is empty.
        Assert.NotEqual(NudgeKind.None, ft8.Nudge);
        Assert.NotEqual("", ft8.NudgeTip);
        Assert.NotEqual("", ft8.NudgeReasonLine);
        Assert.NotEqual("", ft8.NudgeEarnsLine);
        Assert.True(ft8.NudgePreview.HasPreview);

        foreach (var mode in new[] { "PSK31", ContactModes.OliviaName })
        {
            var row = rows[mode];

            _output.WriteLine(mode + " " + row.Nudge + " | " + row.NudgeTip);

            // **THE ROW REALLY IS A KEYBOARD-MODE ROW**, so this is not three FT8 rows.
            Assert.True(row.IsTextOnly);

            Assert.Equal(ft8.Nudge, row.Nudge);
            Assert.Equal(ft8.NudgeTip, row.NudgeTip);
            Assert.Equal(ft8.NudgeReasonLine, row.NudgeReasonLine);
            Assert.Equal(ft8.NudgeEarnsLine, row.NudgeEarnsLine);
            Assert.Equal(ft8.NudgePreview.HasPreview, row.NudgePreview.HasPreview);
            Assert.Equal(ft8.NudgePreview.IsDoor, row.NudgePreview.IsDoor);
        }

        // **AND THE OLIVIA ROW IS THE ONE CARRYING A VARIANT**, which is the fact the CQ list
        // below is named from - asserted here so the two tests cannot drift apart.
        Assert.True(rows[ContactModes.OliviaName].HasVariant);
        Assert.False(rows["PSK31"].HasVariant);
    }

    /// <summary>
    /// **8.3, second half: the CQ list the achievements page is handed names the mode each row
    /// actually is.**
    /// </summary>
    /// <remarks>
    /// **THE OLIVIA CALLER READ `PSK31` UNTIL THIS UNIT** - measured at task 1, on a row carrying
    /// the variant `16/500`. All three row kinds are asserted, because a repair that only checked
    /// Olivia could have been made by labelling every text row `Olivia`.
    /// </remarks>
    [Fact]
    public void TheCqListNamesTheModeEachRowIsAndClaimsNoneForAnFt8Row()
    {
        var rows = RowsForThreeCallers();
        var snapshot = CqSnapshot.From(rows, new DateTime(2026, 9, 21, 19, 0, 0, DateTimeKind.Utc));

        foreach (var call in snapshot.Calls)
        {
            _output.WriteLine(call.Callsign + " -> Mode \"" + call.Mode + "\"");
        }

        Assert.Equal(3, snapshot.Calls.Count);

        // **THE OLIVIA CALLER IS NAMED OLIVIA**, through the table that owns the spelling.
        Assert.Equal(
            ContactModes.OliviaName,
            Assert.Single(snapshot.Calls, c => c.Callsign == "W1AW").Mode);

        // **THE PSK31 CALLER IS STILL NAMED PSK31.**
        Assert.Equal("PSK31", Assert.Single(snapshot.Calls, c => c.Callsign == "G4XYZ").Mode);

        // **AND THE FT8-SHAPED ROW CLAIMS NOTHING**, because it does not know whether it was FT8
        // or FT4 and naming one would be a claim nothing measured (§0.0).
        Assert.Equal("", Assert.Single(snapshot.Calls, c => c.Callsign == "IK4LZH").Mode);
    }

    /// <summary>
    /// **8.4: the scores and the total move on the achievements page when a PSK31 contact is
    /// logged.**
    /// </summary>
    [Fact]
    public void ThePageMovesWhenAPsk31ContactIsLogged() => ThePageMoves("PSK31");

    /// <summary>
    /// **8.4: and when an Olivia contact is logged.**
    /// </summary>
    [Fact]
    public void ThePageMovesWhenAnOliviaContactIsLogged() => ThePageMoves(ContactModes.OliviaName);

    /// <summary>
    /// The page read before the contact and again after it, the same view model built the same
    /// way, with the difference asserted.
    /// </summary>
    private void ThePageMoves(string mode)
    {
        var folder = Path.Combine(_root, "page-" + mode);

        Directory.CreateDirectory(folder);
        SettingsStore.DataFolder = folder;

        var points = AchievementPoints.Parse(AchievementPoints.Shipped());
        var corpus = Psk31Corpus.Load();

        // **BEFORE**, with nothing in the log at all.
        var before = PageNow(points);

        Assert.Equal(0, before.Log.Count);

        // **THE CONTACT, WORKED AND LOGGED THE WAY A PRESS OF LOG LOGS IT.**
        var model = PanelFor(mode, corpus.Operator);

        WorkHim(model, mode, corpus);

        var entry = model.ContactLogEntryForStation(Worked, DialOn20m);

        Assert.NotNull(entry);
        Assert.True(ContactLogStore.Append(entry!, Version));

        model.ReloadContactLogForTests();

        // **AFTER**, the same view model built the same way over the same file.
        var after = PageNow(points);

        Assert.Equal(1, after.Log.Count);

        _output.WriteLine(mode + " before: \"" + before.TotalLine + "\"");
        _output.WriteLine(mode + " after : \"" + after.TotalLine + "\"");

        var moved = new List<string>();

        foreach (var kind in AchievementKinds.All)
        {
            var was = before.Scores.For(kind).Points ?? 0;
            var now = after.Scores.For(kind).Points ?? 0;

            _output.WriteLine("  " + kind.PadRight(14) + was + " -> " + now);

            // **NOTHING GOES BACKWARDS.** A contact can only add.
            Assert.True(now >= was, kind + " went down when a contact was logged");

            if (now != was)
            {
                moved.Add(kind);
            }
        }

        // **THE SCORES MOVED**, and it is named which - a total that moved with every kind
        // standing still would mean the total was computed from something else.
        _output.WriteLine(mode + " kinds that moved: " + string.Join(", ", moved));

        Assert.NotEmpty(moved);

        // **AND THE MODE'S OWN BADGE IS AMONG THEM**, which is what makes this about the keyboard
        // mode rather than about any contact at all.
        Assert.Contains(AchievementKinds.Modes, moved);
        Assert.Contains(AchievementKinds.HallOfFame, moved);

        // **AND THE TOTAL MOVED**, asserted as a difference and not as a figure.
        Assert.NotNull(after.Scores.Total);
        Assert.True(
            after.Scores.Total > (before.Scores.Total ?? 0),
            mode + " logged a contact and the total did not move");

        Assert.NotEqual(before.TotalLine, after.TotalLine);

        // **AND THE MODE HE WORKED IS THE ONE THE BADGE COUNTS.**
        Assert.Equal(new[] { mode }, after.Log.Modes);
    }

    /// <summary>The page, built the way `OpenAchievements` builds it.</summary>
    private static AchievementBadgePage PageNow(AchievementPoints points)
        => new AchievementsViewModel(ContactLogStore.ReadRecords(), MyGrid, points).Page!;

    /// <summary>One row for one station in each of the three modes, each off its own panel.</summary>
    private Dictionary<string, DigitalDecodeRow> RowsForOneStationInEveryMode()
    {
        var corpus = Psk31Corpus.Load();
        var rows = new Dictionary<string, DigitalDecodeRow>(StringComparer.Ordinal);

        var ft8 = PanelFor("FT8", corpus.Operator);

        ft8.AddDecodeRowForTests(
            "214130", "-12", "0.2", "1240", "CQ " + His + " OI33",
            new DateTime(2026, 9, 19, 21, 41, 30, DateTimeKind.Utc), Ft8DialOn20m);

        rows["FT8"] = Assert.Single(ft8.DigitalDecodes, r => r.Sender == His);

        var psk31 = PanelFor("PSK31", corpus.Operator);

        psk31.ShowPsk31ChannelsForTests([new Psk31Channel(1, Psk31Hz, 10.0, CqFrom(His))]);

        rows["PSK31"] = Assert.Single(psk31.DigitalDecodes, r => r.Sender == His);

        var olivia = PanelFor(ContactModes.OliviaName, corpus.Operator);

        olivia.ShowOliviaChannelsForTests(
            [OliviaChannelOf(68, Variant, CallingOffsetOn(olivia), CqFrom(His))]);

        rows[ContactModes.OliviaName] = Assert.Single(olivia.DigitalDecodes, r => r.Sender == His);

        return rows;
    }

    /// <summary>One decoded list holding an FT8 CQ, a PSK31 CQ and an Olivia CQ.</summary>
    private List<DigitalDecodeRow> RowsForThreeCallers()
    {
        var corpus = Psk31Corpus.Load();

        var ft8 = PanelFor("FT8", corpus.Operator);

        ft8.AddDecodeRowForTests(
            "214130", "-12", "0.2", "1240", "CQ IK4LZH JN54",
            new DateTime(2026, 9, 19, 21, 41, 30, DateTimeKind.Utc), Ft8DialOn20m);

        var psk31 = PanelFor("PSK31", corpus.Operator);

        psk31.ShowPsk31ChannelsForTests([new Psk31Channel(1, Psk31Hz, 10.0, CqFrom("G4XYZ"))]);

        var olivia = PanelFor(ContactModes.OliviaName, corpus.Operator);

        olivia.ShowOliviaChannelsForTests(
            [OliviaChannelOf(68, Variant, CallingOffsetOn(olivia), CqFrom("W1AW"))]);

        return ft8.DigitalDecodes
            .Concat(psk31.DigitalDecodes)
            .Concat(olivia.DigitalDecodes)
            .ToList();
    }

    /// <summary>A call to anybody, in the words a keyboard operator sends one in.</summary>
    private static string CqFrom(string who)
        => "CQ CQ CQ de " + who + " " + who + " " + who + " pse k\n";

    /// <summary>Works the one station down whichever keyboard panel this is.</summary>
    private static void WorkHim(MainWindowViewModel model, string mode, Psk31Corpus corpus)
    {
        var olivia = string.Equals(mode, ContactModes.OliviaName, StringComparison.OrdinalIgnoreCase);
        var calling = olivia ? CallingOffsetOn(model) : Psk31Hz;

        var his = corpus.Transcripts.Single(t => t.Name == Textbook).Lines
            .Where(l => string.Equals(l.Frm, Worked, StringComparison.OrdinalIgnoreCase))
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
                model.AnswerPsk31Command.Execute(model.DigitalDecodes.First(r => r.Sender == Worked));
            }
            else
            {
                model.CardActionCommand.Execute(model.DigitalCards.First(c => c.Callsign == Worked));
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
}
