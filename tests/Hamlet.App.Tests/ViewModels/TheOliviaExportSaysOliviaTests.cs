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
/// Work instruction 368 task 3, criterion 5.1's second half: **the export says `MODE=OLIVIA`
/// with the variant as `SUBMODE`, and the pair cannot come apart.**
/// </summary>
/// <remarks>
/// <para>**THE PAIR IS WHAT ANOTHER LOGGER AND AN AWARD PROGRAM READ.** `MODE=OLIVIA` on its
/// own says the mode and not which Olivia, and a submode beside the wrong mode says nothing at
/// all - so both halves are projections of one `ContactMode` and there is no assignment
/// anywhere that can set one without the other (`Ft8ContactLogEntry.For`).</para>
/// <para>**THE VARIANT IS THE ONE THE QSO ENDED AT** (decision BW), because unit 367's one
/// click means a contact can open at 8/250 and finish at 16/500, and what the operator read on
/// the card at the end is what his log should say.</para>
/// <para>**THE ROUND TRIP HERE IS HAMLET READING HAMLET AND IT IS NOT CRITERION 5.4.** It
/// proves the file is self-consistent - what `AdifLog` writes, `AdifLog` reads back unchanged -
/// and it proves nothing whatever about Log4OM, N1MM or LoTW. No logger on this machine has
/// opened this file, because there is none on it (decision CC).</para>
/// <para>**COMPUTED, NOT SEEN** (FACT-004): every contact here is a loopback on the
/// development machine.</para>
/// </remarks>
public sealed class TheOliviaExportSaysOliviaTests : IDisposable
{
    /// <summary>20 m, inside a General class data segment, where the cited table gives an Olivia spot.</summary>
    private const long DialOn20m = 14_071_500;

    /// <summary>The station worked on Olivia here.</summary>
    private const string His = "W1AW";

    private const string Textbook = "01-textbook";

    /// <summary>R29's amount, for the new channel's center only. Nothing here moves anything.</summary>
    private const double UpHz = 500;

    private readonly ITestOutputHelper _output;
    private readonly string _folder;
    private readonly string _wasFolder;

    /// <summary>Creates the tests and redirects the data folder.</summary>
    /// <param name="output">Where the records are printed.</param>
    public TheOliviaExportSaysOliviaTests(ITestOutputHelper output)
    {
        _output = output;
        _wasFolder = SettingsStore.DataFolder;
        _folder = Path.Combine(Path.GetTempPath(), "hamlet-unit368-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_folder);
        SettingsStore.DataFolder = _folder;
    }

    /// <summary>The variant the cited calling table works its spot at, never typed here.</summary>
    private static string CallingVariant => OliviaCallingTable.CallingVariant;

    /// <summary>What R29's one click widens to, read from the one place it is written.</summary>
    private static string WidenedVariant => MainWindowViewModel.OliviaMoveToVariant;

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
    /// **1: a contact that stayed on the calling spot exports the pair with that variant.**
    /// </summary>
    [Fact]
    public void AContactThatStayedAt8250ExportsThePairWithThatVariant()
    {
        var corpus = Psk31Corpus.Load();
        var model = OliviaPanel(corpus.Operator);

        Work(model, corpus);

        var card = Assert.Single(model.DigitalCards, c => c.Callsign == His);
        var entry = model.ContactLogEntryForStation(His, DialOn20m);

        Assert.NotNull(entry);

        var record = AdifLog.Record(entry!);

        _output.WriteLine($"the card ended at \"{card.OliviaVariant}\"");
        _output.WriteLine(record);

        Assert.Equal(CallingVariant, card.OliviaVariant);

        // **THE TWO TAGS, QUOTED AS THE FILE CARRIES THEM.**
        Assert.Contains("<MODE:6>OLIVIA", record, StringComparison.Ordinal);
        Assert.Contains("<SUBMODE:12>OLIVIA 8/250", record, StringComparison.Ordinal);

        // **AND THE SAME PAIR, ASKED OF THE TABLE RATHER THAN OF THE STRING.**
        Assert.True(ContactModes.Olivia(card.OliviaVariant).Matches(entry!.Mode, entry.Submode));
    }

    /// <summary>
    /// **2, decision BW: a contact that opened at 8/250 and moved to 16/500 exports the variant
    /// it ended at.**
    /// </summary>
    /// <remarks>
    /// **THE ONE THE CONVERSATION ENDED AT, BECAUSE THAT IS WHAT THE CARD SHOWED HIM.** The
    /// alternative is the variant it opened at, which the card no longer shows and which he
    /// never read at the moment he pressed Log; two submodes on one record is not a choice,
    /// because ADIF has one `SUBMODE` tag.
    /// </remarks>
    [Fact]
    public void AContactThatMovedTo16500ExportsTheVariantItEndedAt()
    {
        var corpus = Psk31Corpus.Load();
        var model = OliviaPanel(corpus.Operator);
        var calling = CallingOffsetOn(model);
        var his = HisLines(corpus);

        // His CQ on the calling spot, and Hamlet's answer.
        model.ShowOliviaChannelsForTests([Channel(70, CallingVariant, calling, his[0], ended: false)]);
        model.AnswerPsk31Command.Execute(model.DigitalDecodes.First(r => r.Sender == His));
        Settle(model);

        // His certain answer to that, which is the over the move is offered on.
        var answered = his[0] + corpus.Operator + " de " + His + " " + His + " K\n";

        model.ShowOliviaChannelsForTests([Channel(70, CallingVariant, calling, answered, ended: false)]);

        var before = Assert.Single(model.DigitalCards, c => c.Callsign == His);

        Assert.Equal(CallingVariant, before.OliviaVariant);
        Assert.True(before.HasOliviaMove, "the move is not offered, so decision BW's case cannot be driven");

        // **UNIT 367'S ONE CLICK, USED AND NOT RE-ARGUED.** Step 4 is closed.
        model.OliviaMoveUpCommand.Execute(before);
        Settle(model);

        // His channel ends where it was and a new one opens 500 Hz up at the wider variant -
        // which is what the listener does when a station announces a different variant at a
        // different place. The rest of the QSO happens there.
        model.ShowOliviaChannelsForTests(
        [
            Channel(70, CallingVariant, calling, answered, ended: true),
            Channel(71, WidenedVariant, calling + UpHz, his[1], ended: false),
        ]);

        model.CardActionCommand.Execute(model.DigitalCards.First(c => c.Callsign == His));
        Settle(model);

        model.ShowOliviaChannelsForTests(
        [
            Channel(70, CallingVariant, calling, answered, ended: true),
            Channel(71, WidenedVariant, calling + UpHz, his[1] + his[2], ended: false),
        ]);

        model.CardActionCommand.Execute(model.DigitalCards.First(c => c.Callsign == His));
        Settle(model);

        var card = Assert.Single(model.DigitalCards, c => c.Callsign == His);
        var entry = model.ContactLogEntryForStation(His, DialOn20m);

        Assert.NotNull(entry);

        var record = AdifLog.Record(entry!);

        _output.WriteLine($"the QSO opened at {CallingVariant} and the card ended at \"{card.OliviaVariant}\"");
        _output.WriteLine(record);

        // **THE CARD ENDED AT THE WIDER VARIANT AND THE RECORD SAYS SO.**
        Assert.Equal(WidenedVariant, card.OliviaVariant);
        Assert.Contains("<MODE:6>OLIVIA", record, StringComparison.Ordinal);
        Assert.Contains("<SUBMODE:13>OLIVIA 16/500", record, StringComparison.Ordinal);

        // **AND IT IS NOT THE VARIANT IT OPENED AT.**
        Assert.NotEqual(ContactModes.Olivia(CallingVariant).AdifSubmode, entry!.Submode);
        Assert.Equal(ContactModes.Olivia(card.OliviaVariant).AdifSubmode, entry.Submode);
    }

    /// <summary>
    /// **3: the pair cannot be set apart - every mode the log can write projects both halves
    /// from one object, and `Submode` is assigned in one line of `src`.**
    /// </summary>
    /// <remarks>
    /// **THE WAY `Ft8ContactLogEntry`'S OWN REMARK ASKS.** The remark claims there is no
    /// assignment anywhere that could set one without the other; this reads `src` and checks
    /// that the claim is still true, and walks every entry in the table to check that what
    /// comes out of `For` is what the mode says on both halves and never on one.
    /// </remarks>
    [Fact]
    public void ThePairCannotBeSetApart()
    {
        var variants = OliviaData.Current.Format!.Variants.Select(v => v.Name);
        var modes = ContactModes.Logged.Concat(variants.Select(ContactModes.Olivia)).ToList();

        foreach (var mode in modes)
        {
            var entry = Ft8ContactLogEntry.For(
                Ledger(),
                "KC3QIS",
                new Ft8StationConditions(DialOn20m, "20m", mode, "FN00DJ"));

            _output.WriteLine(
                $"{mode.Name} ({mode.AdifSpelling}) -> MODE {entry.Mode ?? "(none)"}, SUBMODE "
                + (entry.Submode ?? "(none)"));

            Assert.Equal(mode.AdifMode, entry.Mode);
            Assert.Equal(mode.AdifMode is null ? null : mode.AdifSubmode, entry.Submode);

            // **A SUBMODE WITHOUT A MODE IS THE SHAPE THAT CANNOT HAPPEN.**
            Assert.False(
                entry.Mode is null && entry.Submode is not null,
                mode.Name + " produced a submode with no mode");
        }

        // **AND ONE PLACE IN `src` WRITES IT.** `AdifLog` reads a submode back out of a file
        // and `ContactLogViewModel` shows one on the screen; only `Ft8ContactLogEntry.For`
        // composes one, and it composes it beside the mode from the same object.
        var composed = Sites(Path.Combine(Root(), "src"), "Submode = ")
            .Where(l => !l.Contains("AdifLog.cs", StringComparison.Ordinal)
                        && !l.Contains("ContactLogViewModel.cs", StringComparison.Ordinal))
            .ToList();

        foreach (var line in composed)
        {
            _output.WriteLine("composes a submode: " + line);
        }

        Assert.Single(composed);
        Assert.Contains("Ft8ContactLogEntry.cs", composed[0], StringComparison.Ordinal);
    }

    /// <summary>
    /// **4: the file re-read by Hamlet's own reader gives back the same record. THIS IS NOT
    /// CRITERION 5.4.**
    /// </summary>
    /// <remarks>
    /// **HAMLET READING HAMLET.** It proves the export is self-consistent and nothing more:
    /// no third-party logger is on this machine, a package would be a `MOVE: stop`, and the
    /// tests reach no network, so nothing here has imported this file into anything (decision
    /// CC). 5.4 is carried to step 6, where Tim has a logger.
    /// </remarks>
    [Fact]
    public void TheFileReReadByHamletsOwnReaderGivesBackTheSameRecord()
    {
        var corpus = Psk31Corpus.Load();
        var model = OliviaPanel(corpus.Operator);

        Work(model, corpus);

        var entry = model.ContactLogEntryForStation(His, DialOn20m);

        Assert.NotNull(entry);

        var path = Path.Combine(_folder, "unit368.adi");

        File.WriteAllText(path, AdifLog.Header(AboutViewModel.AppVersion) + AdifLog.Record(entry!));

        var back = Assert.Single(AdifLog.ReadRecords(File.ReadAllText(path)));

        _output.WriteLine(File.ReadAllText(path));

        Assert.True(back.IsSound, "the record Hamlet wrote did not read back whole: " + string.Join("; ", back.Faults));

        var read = back.Contact;

        Assert.Equal(entry!.Mode, read.Mode);
        Assert.Equal(entry.Submode, read.Submode);
        Assert.Equal(entry.Band, read.Band);
        Assert.Equal(entry.GridSquare, read.GridSquare);
        Assert.Equal(entry.MyGridSquare, read.MyGridSquare);
        Assert.Equal(entry.FrequencyMhz, read.FrequencyMhz);

        // **THE TIMES TO THE RESOLUTION THE FILE CARRIES THEM AT.** ADIF writes `TIME_ON` as
        // `HHmmss`, so a record's own second is what comes back; comparing the ticks would be
        // asserting a precision the format does not have.
        Assert.Equal(Moment(entry.StartedUtc), Moment(read.StartedUtc));
        Assert.Equal(Moment(entry.EndedUtc), Moment(read.EndedUtc));

        // **AND THE REPORTS COME BACK IN THE FIELD THEY WENT OUT IN** - a readability and not
        // a decibel figure, because Olivia exchanges an RST exactly as PSK31 does.
        Assert.Equal(entry.RstSent, read.RstSent);
        Assert.Equal(entry.RstReceived, read.RstReceived);
        Assert.Null(read.ReportSent);
        Assert.Null(read.ReportReceived);
    }

    /// <summary>
    /// **5: the two spellings, checked against the citation the tree carries, with no network
    /// call.**
    /// </summary>
    /// <remarks>
    /// **A CITATION AND NOT A FETCH** (decisions BV and CC). No copy of the ADIF submode
    /// enumeration is vendored in this repository, so what can honestly be checked from inside
    /// it is that the pair has the shape the cited edition gives - the mode's own name, one
    /// space, and the variant as `data/olivia/format.json` writes it - and that the file's
    /// citation still names that edition. **Nothing here has read adif.org.**
    /// </remarks>
    [Fact]
    public void TheSpellingsMatchTheCitationTheTreeCarries()
    {
        _output.WriteLine("the citation: " + ContactModes.Cite);

        Assert.Contains("ADIF Specification 3.1.4", ContactModes.Cite, StringComparison.Ordinal);
        Assert.Contains("https://www.adif.org/314/", ContactModes.Cite, StringComparison.Ordinal);

        var format = OliviaData.Current.Format!;

        foreach (var variant in format.Variants)
        {
            var mode = ContactModes.Olivia(variant.Name);

            _output.WriteLine($"{variant.Name} -> {mode.AdifSpelling}");

            Assert.Equal("OLIVIA", mode.AdifMode);
            Assert.Equal("OLIVIA " + variant.Name, mode.AdifSubmode);

            // **THE VARIANT IS THE FORMAT FILE'S OWN NAME**, not a second spelling of it.
            Assert.Equal(variant.Name, mode.AdifSubmode!["OLIVIA ".Length..]);
        }

        // The three the phase works, quoted for the report.
        foreach (var variant in new[] { "8/250", "16/500", "32/1000" })
        {
            Assert.Equal("OLIVIA " + variant, ContactModes.Olivia(variant).AdifSubmode);
        }

        // **AND A VARIANT HAMLET DID NOT MEASURE IS NO SUBMODE AT ALL** (decision BX).
        Assert.Null(ContactModes.Olivia(null).AdifSubmode);
        Assert.Null(ContactModes.Olivia("").AdifSubmode);
        Assert.Equal("OLIVIA", ContactModes.Olivia(null).AdifMode);
    }

    /// <summary>
    /// **6, work instruction 368 task 5: the whole exported file for one Olivia contact, field by
    /// field against the citation the tree carries - and what criterion 5.4 still needs.**
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS NOT 5.4 AND IT DOES NOT CLAIM TO BE** (decision CC). 5.4 asks that the
    /// export import cleanly into one named logger. There is no third-party logger on this
    /// machine, a package would be a `MOVE: stop` (`PHASE_PLAN.md` §6), and the tests reach no
    /// network (`TheTestsStayOffTheNetworkTests`) - so **nothing here has imported this file
    /// into anything**, and no sentence in this unit says a logger read it.</para>
    /// <para>**WHAT IT DOES INSTEAD** is check every field of the record against what the cited
    /// specification requires of it, as that requirement is carried in this tree: the tag names
    /// `AdifLog` writes, the pair `ContactModes` spells, the RST fields `AdifLog.IsRstMode`
    /// decides between, the band, the frequency in megahertz, and the times in ADIF's own
    /// shapes. **A file that is right by every check here can still be refused by a real
    /// logger**, and finding that out is step 6's, at a machine with one on it.</para>
    /// </remarks>
    [Fact]
    public void TheWholeExportedFileForOneOliviaContactCheckedAgainstTheCitation()
    {
        var corpus = Psk31Corpus.Load();
        var model = OliviaPanel(corpus.Operator);

        Work(model, corpus);

        var entry = model.ContactLogEntryForStation(His, DialOn20m);

        Assert.NotNull(entry);

        var file = AdifLog.Header(AboutViewModel.AppVersion) + AdifLog.Record(entry!);

        _output.WriteLine("THE WHOLE FILE, as it would go to another logger:");
        _output.WriteLine(file);

        var fields = AdifLog.Record(entry!)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Where(l => l.StartsWith('<') && !l.StartsWith("<EOR", StringComparison.OrdinalIgnoreCase))
            .Select(Tag)
            .ToDictionary(t => t.Name, t => t.Value, StringComparer.Ordinal);

        foreach (var (name, value) in fields)
        {
            _output.WriteLine($"  {name,-18} {value}");
        }

        // **THE HEADER IS A HEADER AND THE RECORD ENDS.** A reader that cannot find `<EOH>`
        // and `<EOR>` has no records at all, whatever the fields say.
        Assert.Contains("<EOH>", file, StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith("<EOR>\n", file, StringComparison.Ordinal);

        // **EVERY LENGTH IS THE VALUE'S OWN.** `<CALL:4>W1AW` and not `<CALL:5>W1AW`, which is
        // the one thing about the ADI format a reader cannot recover from.
        foreach (var line in AdifLog.Record(entry!).Split('\n', StringSplitOptions.RemoveEmptyEntries)
                     .Where(l => l.StartsWith('<') && !l.StartsWith("<EOR", StringComparison.OrdinalIgnoreCase)))
        {
            var tag = Tag(line);

            Assert.Equal(tag.Value.Length, tag.Length);
        }

        // **THE PAIR, THE WAY THE TABLE SPELLS IT** - the mode's own name and the variant the
        // conversation ended at, and nothing composed here.
        var card = Assert.Single(model.DigitalCards, c => c.Callsign == His);
        var mode = ContactModes.Olivia(card.OliviaVariant);

        Assert.Equal(mode.AdifMode, fields["MODE"]);
        Assert.Equal(mode.AdifSubmode, fields["SUBMODE"]);

        // **AN RST MODE CARRIES `RST_SENT` AND `RST_RCVD`**, and the reader agrees that is what
        // they are: the same question `AdifLog` asks on the way back in.
        Assert.Matches("^[0-9]{3}$", fields["RST_SENT"]);
        Assert.Matches("^[0-9]{3}$", fields["RST_RCVD"]);
        Assert.Equal(fields["RST_SENT"], AdifLog.ReadRecords(file).Single().Contact.RstSent);
        Assert.Null(AdifLog.ReadRecords(file).Single().Contact.ReportSent);

        // **THE BAND IS THE ONE THE DIAL IS IN, SPELLED THE WAY ADIF SPELLS IT.** Hamlet's own
        // band plan writes `20 m` with a space, for a person to read; the cited enumeration's
        // value is `20m`, and the record carries that.
        Assert.Equal(
            Hamlet.RadioEngine.Bands.HfBands.BandFor(DialOn20m)!.Name.Replace(" ", "", StringComparison.Ordinal),
            fields["BAND"]);
        Assert.Matches("^[0-9]+c?m$", fields["BAND"]);
        Assert.Equal(
            (DialOn20m / 1_000_000.0).ToString("0.000000", System.Globalization.CultureInfo.InvariantCulture),
            fields["FREQ"]);

        // **THE TIMES IN ADIF'S OWN SHAPES**, `YYYYMMDD` and `HHMMSS`, and the off is not
        // before the on.
        Assert.Matches("^[0-9]{8}$", fields["QSO_DATE"]);
        Assert.Matches("^[0-9]{6}$", fields["TIME_ON"]);
        Assert.Matches("^[0-9]{6}$", fields["TIME_OFF"]);
        Assert.True(
            string.CompareOrdinal(fields["TIME_OFF"], fields["TIME_ON"]) >= 0,
            "the contact ends before it starts");

        // **BOTH GRIDS, AND HIS IS NOT THE OPERATOR'S.**
        Assert.Equal(4, fields["GRIDSQUARE"].Length);
        Assert.NotEqual(fields["GRIDSQUARE"], fields["MY_GRIDSQUARE"]);

        // **AND THE PLAIN SENTENCE, WHICH IS THE DELIVERABLE** (decision CC).
        _output.WriteLine("");
        _output.WriteLine(
            "CRITERION 5.4 IS NOT MET. No third-party logger is on this machine, installing one "
            + "would be a MOVE: stop, and the tests reach no network, so nothing in this unit has "
            + "imported this file into anything; every check above is Hamlet checking Hamlet. "
            + "5.4 is carried to step 6, where Tim is at a machine with a logger on it.");
    }

    /// <summary>One ADI field line, split into its name, its declared length and its value.</summary>
    private static (string Name, int Length, string Value) Tag(string line)
    {
        var close = line.IndexOf('>', StringComparison.Ordinal);
        var head = line[1..close].Split(':');

        return (
            head[0],
            int.Parse(head[1], System.Globalization.CultureInfo.InvariantCulture),
            line[(close + 1)..]);
    }

    /// <summary>A moment at the resolution ADIF writes it, or "(none)".</summary>
    private static string Moment(DateTime? at)
        => at is { } when
            ? when.ToString("yyyyMMdd HHmmss", System.Globalization.CultureInfo.InvariantCulture)
            : "(none)";

    /// <summary>His three lines of the textbook transcript, each with its newline.</summary>
    private static List<string> HisLines(Psk31Corpus corpus)
        => corpus.Transcripts.Single(t => t.Name == Textbook).Lines
            .Where(l => string.Equals(l.Frm, His, StringComparison.OrdinalIgnoreCase))
            .Select(l => l.Text + "\n")
            .ToList();

    /// <summary>Works the textbook exchange to 73 on the calling spot, a press at a time.</summary>
    private static void Work(MainWindowViewModel model, Psk31Corpus corpus)
    {
        var calling = CallingOffsetOn(model);
        var his = HisLines(corpus);
        var heard = "";

        for (var over = 0; over < his.Count; over++)
        {
            heard += his[over];

            model.ShowOliviaChannelsForTests([Channel(72, CallingVariant, calling, heard, ended: false)]);

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

    /// <summary>A ledger with one message each way, so `For` has a contact to describe.</summary>
    private static Ft8StationRecord Ledger()
    {
        var ledger = new Ft8ContactLedger("KC3QIS");
        var slot = new DateTime(2026, 9, 19, 21, 0, 0, DateTimeKind.Utc);

        ledger.RecordHeard("CQ " + His + " FN31", slot);
        ledger.RecordHeard("KC3QIS " + His + " R-12", slot.AddSeconds(15));

        return ledger.For(His)!;
    }

    /// <summary>An Olivia channel as the listener would list it.</summary>
    private static OliviaChannel Channel(int id, string variant, double centerHz, string text, bool ended)
        => new(id, variant, centerHz, OliviaListener.FoundByRsid, 0, text, text.Length > 0 ? 9 : 0, 0, ended, centerHz);

    /// <summary>Where the band's Olivia calling center sits, by the cited table's own arithmetic.</summary>
    private static double CallingOffsetOn(MainWindowViewModel model)
    {
        var row = OliviaData.Current.Calling!.CallingRowFor(model.SelectedBand.Band.Name)!;

        return row.CenterHz - (double)model.FrequencyHz;
    }

    /// <summary>A panel on 20 m with the Olivia tab pressed, the tap running and a fake wire.</summary>
    private static MainWindowViewModel OliviaPanel(string mine)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = mine;
        settings.Operator.GridSquare = "FN00DJ";
        settings.Operator.OperatorName = "Tim";
        settings.Operator.Location = "Trafford PA";
        settings.Operator.LicenseClass = LicenseClass.General;

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
            TapForTests = new AudioTap(),
        };

        model.SelectedBand = model.Bands.First(b => b.Band.LowHz <= DialOn20m && b.Band.HighHz >= DialOn20m);
        model.FrequencyHz = DialOn20m;
        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.UseRigPortForTests(new FakePort());
        model.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(new FakePort(), new FakeSink())));
        model.ChooseDigitalModeCommand.Execute("Olivia");

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

    /// <summary>Every code line under a folder holding this text, a comment not being one.</summary>
    private static List<string> Sites(string folder, string needle)
    {
        var found = new List<string>();

        foreach (var path in Directory.EnumerateFiles(folder, "*.cs", SearchOption.AllDirectories)
                     .Where(p => !p.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                                 && !p.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                     .OrderBy(p => p, StringComparer.Ordinal))
        {
            var lines = File.ReadAllLines(path);

            for (var i = 0; i < lines.Length; i++)
            {
                var trimmed = lines[i].TrimStart();

                if (!trimmed.StartsWith("//", StringComparison.Ordinal)
                    && !trimmed.StartsWith("*", StringComparison.Ordinal)
                    && !trimmed.StartsWith("///", StringComparison.Ordinal)
                    && lines[i].Contains(needle, StringComparison.Ordinal))
                {
                    found.Add($"{Path.GetRelativePath(folder, path)}:{i + 1}: {trimmed}");
                }
            }
        }

        return found;
    }

    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }
}
