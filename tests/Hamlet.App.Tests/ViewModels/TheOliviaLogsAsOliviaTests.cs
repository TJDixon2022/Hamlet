using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Hamlet.App.Settings;
using Hamlet.App.Telemetry;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Tests.Psk31;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 368 task 2, criterion 5.1's first half: **a contact worked on Olivia goes
/// into the log as an Olivia contact, with the report and the grid the card showed.**
/// </summary>
/// <remarks>
/// <para>**THE MODE IS READ OFF THE CONVERSATION AND NEVER OFF THE TAB** (decision BU). Since
/// unit 364 an Olivia row and a PSK31 row are the same row and an Olivia card is a PSK31 card,
/// so `Psk31ContactWith` answered `PSK31` for any station with a card - and an evening spent on
/// Olivia went into the operator's permanent file saying it was PSK31. **That is a false
/// sentence in the longest-lived thing Hamlet writes** (§0.0), and it is what this class
/// holds down.</para>
/// <para>**THE REPORTS AND THE GRID WERE ALREADY RIGHT** (work instruction 368 task 1 item 2,
/// R14). `Psk31ReportsFor` and `Psk31GridFor` read the shared card dictionaries an Olivia row
/// fills, so nothing was built for them; they are asserted here against the conversation - the
/// macro Hamlet sent and the corpus's own declared reading - rather than against a literal, so
/// this class fails if they ever stop working.</para>
/// <para>**COMPUTED, NOT SEEN, AND NOT EVIDENCE ABOUT THE RADIO** (FACT-004). Every contact
/// here is a loopback on the development machine; no Olivia signal from Hamlet has been on the
/// air and no record written here is a contact anybody made.</para>
/// </remarks>
public sealed class TheOliviaLogsAsOliviaTests : IDisposable
{
    /// <summary>20 m, inside a General class data segment, where the cited table gives an Olivia spot.</summary>
    private const long DialOn20m = 14_071_500;

    /// <summary>The other station of the corpus's textbook exchange, worked on Olivia here.</summary>
    private const string His = "W1AW";

    /// <summary>The station worked on PSK31 in the same session, from the chatty transcript.</summary>
    private const string HisOnPsk31 = "G4XYZ";

    /// <summary>Where a PSK31 carrier sits in the passband, for the PSK31 half.</summary>
    private const double Psk31Hz = 1234;

    private const string Textbook = "01-textbook";

    private readonly ITestOutputHelper _output;
    private readonly string _folder;
    private readonly string _wasFolder;

    /// <summary>Creates the tests and redirects the data folder.</summary>
    /// <param name="output">Where the records are printed.</param>
    public TheOliviaLogsAsOliviaTests(ITestOutputHelper output)
    {
        _output = output;
        _wasFolder = SettingsStore.DataFolder;
        _folder = Path.Combine(Path.GetTempPath(), "hamlet-unit368-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_folder);
        SettingsStore.DataFolder = _folder;
    }

    /// <summary>The variant the cited calling table works its spot at, never typed here.</summary>
    private static string CallingVariant => OliviaCallingTable.CallingVariant;

    /// <summary>Puts the real folder back.</summary>
    public void Dispose()
    {
        SettingsStore.DataFolder = _wasFolder;

        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (IOException)
        {
        }
    }

    /// <summary>
    /// **1: the station from a loopback Olivia exchange logs `MODE=OLIVIA`, with the reports and
    /// the grid the card showed.**
    /// </summary>
    /// <remarks>
    /// **NOT ONE FIGURE IN THIS TEST IS A LITERAL.** The mode comes from
    /// `ContactModes.Olivia(...)` at the variant the card is showing, the report Hamlet sent is
    /// read out of the macro that went on the air, the report he sent is the corpus's own
    /// declared reading of his line, and the grid is the one the card is printing - so a change
    /// that broke any of the four fails here rather than agreeing with a number copied into the
    /// test beside it.
    /// </remarks>
    [Fact]
    public void AnOliviaContactLogsAsOliviaWithTheReportsAndTheGridTheCardShowed()
    {
        var corpus = Psk31Corpus.Load();
        var model = OliviaPanel(corpus.Operator);

        Work(model, corpus);

        var card = Assert.Single(model.DigitalCards, c => c.Callsign == His);
        var entry = model.ContactLogEntryForStation(His, DialOn20m);

        Assert.NotNull(entry);

        _output.WriteLine($"the card is showing variant \"{card.OliviaVariant}\", grid \"{card.Facts.Grid}\"");
        _output.WriteLine(AdifLog.Record(entry!));

        // **THE MODE THE CONTACT WAS MADE IN, THROUGH THE TABLE THAT OWNS THE SPELLING.**
        var olivia = ContactModes.Olivia(card.OliviaVariant);

        Assert.Equal(olivia.AdifMode, entry!.Mode);
        Assert.Equal(olivia.AdifSubmode, entry.Submode);
        Assert.True(olivia.Matches(entry.Mode, entry.Submode));

        // **AND IT IS NOT PSK31**, which is the sentence this task exists to stop being written.
        Assert.False(
            ContactModes.Named("PSK31")!.Matches(entry.Mode, entry.Submode),
            "an Olivia contact still logs as PSK31");

        // **THE REPORT HAMLET SENT IS THE ONE IN THE LINE HAMLET SENT.**
        var mineOnTheAir = Psk31Macros.Report(
            His, corpus.Operator, "Tim", "Trafford PA", "FN00DJ");

        Assert.Equal(RstIn(mineOnTheAir), entry.RstSent);

        // **AND THE REPORT HE SENT IS THE CORPUS'S OWN READING OF HIS LINE.**
        var hisReport = corpus.Transcripts
            .Single(t => t.Name == Textbook)
            .Lines
            .Last(l => string.Equals(l.Frm, His, StringComparison.OrdinalIgnoreCase) && l.Rst is not null);

        Assert.Equal(hisReport.Rst, entry.RstReceived);

        // **THE GRID IS THE ONE THE CARD IS PRINTING**, which is the one the parser read for
        // certain out of his prose.
        Assert.Equal(card.Facts.Grid, entry.GridSquare);
        Assert.Equal(hisReport.Grid, entry.GridSquare);

        // **AND THE DECIBEL FIELDS ARE UNTOUCHED** (§3.2). Olivia exchanged no ratio.
        Assert.Null(entry.ReportSent);
        Assert.Null(entry.ReportReceived);
    }

    /// <summary>
    /// **2: a PSK31 station still logs `MODE=PSK31`, and FT8 and FT4 records are untouched.**
    /// </summary>
    /// <remarks>
    /// **THIS IS DECISION BU's REGRESSION ASSERTION AND IT IS NOT OPTIONAL.** The change reads a
    /// card that every keyboard-mode contact goes through, and `Ft8StationConditions` is what
    /// every mode's record is built from, so the other modes are asserted here rather than
    /// assumed. **The three panels are separate because the tabs are** - leaving Olivia puts its
    /// listener down (unit 364) - and all three run inside this one test.
    /// </remarks>
    [Fact]
    public void APsk31StationStillLogsPsk31AndFt8AndFt4AreUntouched()
    {
        var corpus = Psk31Corpus.Load();

        // **PSK31**, the mode that shares the cards.
        var psk31Panel = Psk31Panel(corpus.Operator);

        WorkPsk31(psk31Panel, corpus);

        var psk31 = psk31Panel.ContactLogEntryForStation(HisOnPsk31, DialOn20m);

        Assert.NotNull(psk31);

        _output.WriteLine("PSK31: " + AdifLog.Record(psk31!));

        var expected = ContactModes.Named("PSK31")!;

        Assert.Equal(expected.AdifMode, psk31!.Mode);
        Assert.Equal(expected.AdifSubmode, psk31.Submode);
        Assert.NotNull(psk31.RstSent);

        // **FT8**, whose record is built from `DigitalMode` and not from a card at all.
        var ft8Panel = SlottedPanel(corpus.Operator, "FT8");
        var slot = new DateTime(2026, 9, 19, 21, 41, 30, DateTimeKind.Utc);

        ft8Panel.AddDecodeRowForTests("214130", "-12", "0.2", "1240", "CQ IK4LZH JN54", slot, 14_074_000);
        ft8Panel.AddDecodeRowForTests(
            "214145", "-12", "0.2", "1240", corpus.Operator + " IK4LZH -12", slot.AddSeconds(15), 14_074_000);

        var ft8 = ft8Panel.ContactLogEntryForStation("IK4LZH", 14_074_000);

        Assert.NotNull(ft8);

        _output.WriteLine("FT8  : " + AdifLog.Record(ft8!));

        Assert.Equal(ContactModes.Named("FT8")!.AdifMode, ft8!.Mode);
        Assert.Null(ft8.Submode);
        Assert.Equal("-12", ft8.ReportReceived);
        Assert.Null(ft8.RstReceived);

        // **FT4**, the mode whose pair is `MODE=MFSK` plus `SUBMODE=FT4`.
        var ft4Panel = SlottedPanel(corpus.Operator, "FT4");

        ft4Panel.AddDecodeRowForTests("214130", "-08", "0.2", "1240", "CQ IK4LZH JN54", slot, 14_080_000);
        ft4Panel.AddDecodeRowForTests(
            "214137", "-08", "0.2", "1240", corpus.Operator + " IK4LZH -08", slot.AddSeconds(7), 14_080_000);

        var ft4 = ft4Panel.ContactLogEntryForStation("IK4LZH", 14_080_000);

        Assert.NotNull(ft4);

        _output.WriteLine("FT4  : " + AdifLog.Record(ft4!));

        var ft4Mode = ContactModes.Named("FT4")!;

        Assert.Equal(ft4Mode.AdifMode, ft4!.Mode);
        Assert.Equal(ft4Mode.AdifSubmode, ft4.Submode);

        // **AND NONE OF THE THREE IS OLIVIA**, by the table's own question rather than by
        // looking at the strings.
        foreach (var other in new[] { psk31, ft8, ft4 })
        {
            Assert.False(
                ContactModes.Olivia(null).Matches(other.Mode, other.Submode),
                "a record that was not made on Olivia matches Olivia");
        }
    }

    /// <summary>
    /// **3: an Olivia conversation whose variant is not readable logs the mode with no
    /// submode** (decision BX).
    /// </summary>
    /// <remarks>
    /// **ABSENT, NOT GUESSED** (§0.0). A variant Hamlet did not measure is a fact it does not
    /// have; writing the calling spot's variant into the record because it is the likely one
    /// would put a measurement in the file that nobody made. **And the mode is still Olivia** -
    /// answering `PSK31` because the variant is missing would replace a missing fact with a
    /// wrong one, which is worse.
    /// </remarks>
    [Fact]
    public void AnOliviaCardWithNoVariantLogsTheModeAndNoSubmode()
    {
        var corpus = Psk31Corpus.Load();
        var model = OliviaPanel(corpus.Operator);

        Work(model, corpus, variant: "");

        var card = Assert.Single(model.DigitalCards, c => c.Callsign == His);

        Assert.Equal("", card.OliviaVariant);
        Assert.False(card.IsOlivia);

        var entry = model.ContactLogEntryForStation(His, DialOn20m);

        Assert.NotNull(entry);

        _output.WriteLine(AdifLog.Record(entry!));

        Assert.Equal(ContactModes.Olivia(null).AdifMode, entry!.Mode);
        Assert.Null(entry.Submode);

        // **AND THE TAG IS ABSENT FROM THE FILE, NOT EMPTY IN IT.**
        Assert.DoesNotContain("<SUBMODE:", AdifLog.Record(entry), StringComparison.Ordinal);

        // **STILL NOT PSK31.**
        Assert.False(ContactModes.Named("PSK31")!.Matches(entry.Mode, entry.Submode));
    }

    /// <summary>
    /// **4: the logged event fires for an Olivia contact, carries the mode and the variant, and
    /// names nobody** (§R13, HM-DEC-018 §2.1).
    /// </summary>
    /// <remarks>
    /// **LOGGING IS A STAGE AND THIS STEP ADDS IT FOR OLIVIA**, so it writes a line a person can
    /// diagnose it from: which mode, which variant, whether the reports and a grid went in.
    /// **The variant is inside the submode** rather than in a field of its own, because the
    /// submode is where a record carries it and a second copy could disagree with the first.
    /// </remarks>
    [Fact]
    public void TheLoggedEventCarriesTheModeAndTheVariantAndNothingPersonal()
    {
        var corpus = Psk31Corpus.Load();
        var folder = Path.Combine(_folder, "telemetry");

        Directory.CreateDirectory(folder);

        AdifContact? entry = null;
        string variant;

        using (var telemetry = new JsonlTelemetry(folder, "368", _ => true))
        {
            var model = OliviaPanel(corpus.Operator, telemetry);

            Work(model, corpus);

            variant = Assert.Single(model.DigitalCards, c => c.Callsign == His).OliviaVariant;
            entry = model.ContactLogEntryForStation(His, DialOn20m);

            Assert.NotNull(entry);
            Assert.True(model.WriteLoggedContactForTests(entry!));
        }

        var lines = Directory.GetFiles(folder, "*.jsonl").SelectMany(File.ReadAllLines).ToList();
        var logged = Assert.Single(Events(lines, "psk31_contact_logged"));

        _output.WriteLine(logged.ToString());

        var data = logged.GetProperty("data");
        var olivia = ContactModes.Olivia(variant);

        Assert.Equal(olivia.AdifMode, data.GetProperty("mode").GetString());
        Assert.Equal(olivia.AdifSubmode, data.GetProperty("submode").GetString());
        Assert.Equal(entry!.RstSent, data.GetProperty("rstSent").GetString());
        Assert.Equal(entry.RstReceived, data.GetProperty("rstReceived").GetString());
        Assert.Equal(
            !string.IsNullOrWhiteSpace(entry.GridSquare),
            data.GetProperty("gridCarried").GetBoolean());

        // **THE VARIANT IS IN THE LINE, WHICH IS THE WHOLE POINT OF WRITING IT.**
        Assert.Contains(variant, logged.ToString(), StringComparison.Ordinal);

        // **AND NOTHING PERSONAL, ANYWHERE IN IT.**
        var text = logged.ToString();

        foreach (var personal in new[] { His, corpus.Operator, "FN31", "FN00", "Tim", "Bob", "NEWINGTON" })
        {
            Assert.DoesNotContain(personal, text, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>The RST inside a line that went on the air, read out of the line itself.</summary>
    private static string RstIn(string line)
        => line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .First(w => w.Length == 3 && w.All(char.IsDigit));

    /// <summary>Works the textbook exchange to 73 on the Olivia calling spot, a press at a time.</summary>
    private static void Work(MainWindowViewModel model, Psk31Corpus corpus, string? variant = null)
    {
        var calling = CallingOffsetOn(model);
        var his = corpus.Transcripts.Single(t => t.Name == Textbook).Lines
            .Where(l => string.Equals(l.Frm, His, StringComparison.OrdinalIgnoreCase))
            .Select(l => l.Text + "\n")
            .ToList();

        var heard = "";

        for (var over = 0; over < his.Count; over++)
        {
            heard += his[over];

            model.ShowOliviaChannelsForTests(
                [Channel(68, variant ?? CallingVariant, calling, heard)]);

            if (over == 0)
            {
                model.AnswerPsk31Command.Execute(
                    model.DigitalDecodes.First(r => r.Sender == His));
            }
            else
            {
                model.CardActionCommand.Execute(
                    model.DigitalCards.First(c => c.Callsign == His));
            }

            Settle(model);
        }
    }

    /// <summary>Works the chatty exchange on PSK31, the same way `ThePsk31LogsWithRstTests` does.</summary>
    private static void WorkPsk31(MainWindowViewModel model, Psk31Corpus corpus)
    {
        var his = corpus.Transcripts.Single(t => t.Name == "02-chatty").Lines
            .Where(l => !string.Equals(l.Frm, corpus.Operator, StringComparison.OrdinalIgnoreCase))
            .Select(l => l.Text + "\n")
            .ToList();

        var heard = "";

        for (var over = 0; over < 3; over++)
        {
            heard += his[over];

            model.ShowPsk31ChannelsForTests([new Psk31Channel(1, Psk31Hz, 10.0, heard)]);

            if (over == 0)
            {
                model.AnswerPsk31Command.Execute(
                    model.DigitalDecodes.First(r => r.Sender == HisOnPsk31));
            }
            else
            {
                model.CardActionCommand.Execute(
                    model.DigitalCards.First(c => c.Callsign == HisOnPsk31));
            }

            Settle(model);
        }
    }

    /// <summary>An Olivia channel as the listener would list it.</summary>
    private static OliviaChannel Channel(int id, string variant, double centerHz, string text)
        => new(id, variant, centerHz, OliviaListener.FoundByRsid, 0, text, text.Length > 0 ? 9 : 0, 0, false, centerHz);

    /// <summary>Where the band's Olivia calling center sits, by the cited table's own arithmetic.</summary>
    private static double CallingOffsetOn(MainWindowViewModel model)
    {
        var row = OliviaData.Current.Calling!.CallingRowFor(model.SelectedBand.Band.Name)!;

        return row.CenterHz - (double)model.FrequencyHz;
    }

    /// <summary>A panel on 20 m with the Olivia tab pressed, the tap running and a fake wire.</summary>
    private static MainWindowViewModel OliviaPanel(string mine, JsonlTelemetry? telemetry = null)
    {
        var model = Panel(mine, telemetry, "Olivia");

        model.TapForTests = new AudioTap();
        model.ChooseDigitalModeCommand.Execute("Olivia");

        return model;
    }

    /// <summary>The same panel with the PSK31 tab pressed.</summary>
    private static MainWindowViewModel Psk31Panel(string mine)
    {
        var model = Panel(mine, null, "PSK31");

        model.ChooseDigitalModeCommand.Execute("PSK31");

        return model;
    }

    /// <summary>The same panel on one of the slotted modes, where no card decides the mode.</summary>
    private static MainWindowViewModel SlottedPanel(string mine, string mode)
    {
        var model = Panel(mine, null, mode);

        model.ChooseDigitalModeCommand.Execute(mode);

        return model;
    }

    private static MainWindowViewModel Panel(string mine, JsonlTelemetry? telemetry, string mode)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = mine;
        settings.Operator.GridSquare = "FN00DJ";
        settings.Operator.OperatorName = "Tim";
        settings.Operator.Location = "Trafford PA";
        settings.Operator.LicenseClass = LicenseClass.General;

        var model = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        model.SelectedBand = model.Bands.First(b => b.Band.LowHz <= DialOn20m && b.Band.HighHz >= DialOn20m);
        model.FrequencyHz = DialOn20m;
        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.UseRigPortForTests(new FakePort());
        model.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(new FakePort(), new FakeSink())));

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

    private static List<JsonElement> Events(IEnumerable<string> lines, string name)
        => lines
            .Where(l => l.Contains("\"" + name + "\"", StringComparison.Ordinal))
            .Select(l => JsonDocument.Parse(l).RootElement)
            .Where(e => e.GetProperty("event").GetString() == name)
            .ToList();
}
